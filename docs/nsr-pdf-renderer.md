# NIS Service Report — ตัวเรนเดอร์ PDF (Playwright/Chromium)

เอกสารสำหรับ DevOps/ผู้ดูแลระบบ: สิ่งที่ต้องมีในเครื่อง/ภาพ Docker เพื่อให้ `NisReportPdfRenderer` ทำงาน
วิธีทดสอบบน macOS และขั้นตอน smoke test ก่อนถือว่า slice 9 (MERGE_PLAN §6) เสร็จ

โค้ดที่เกี่ยว: `Services/NisReportPdfRenderer.cs` (เบราว์เซอร์ · รอสถานะ · พิมพ์ · ด่านฟอนต์) ·
`Services/NisReportPdfFurniture.cs` (หัว/ท้ายกระดาษจาก snapshot) · `Services/NisReportPdfTools.cs` (PDFsharp: นับหน้า · แทรกปก · อ่านฟอนต์)
· `Services/NisReportPdfQueueService.cs` (คิว — ผู้เรียก)

> **ยังไม่ได้แก้ `Dockerfile` / `docker-compose*.yml` ของ repo นี้** — ต้องแจ้งก่อนตามกฎ · ด้านล่างคือการเปลี่ยนแปลงที่ต้องทำเป๊ะ ๆ

---

## 1. ภาพรวมการทำงาน

```
NisReportPdfQueueService (หยิบงาน · ออก print key ใหม่)
   └─ NisReportPdfRenderer.RenderAsync
        ├─ Chromium เปิด {NisReport:CrmBaseUrl}/nsr-print/{reportId}?key=…        ← หน้าพิมพ์ใน CRM SPA (go-crm-24v4)
        │     หน้าพิมพ์เรียก GET {VITE_SERVER_URL}/api/nis/report/reports/{id}/print-data?key=…   ← coreapi เอง
        ├─ รอ [data-print-state]=ready → window.__CHARTS_READY__ → รูปครบ   (error = ล้มทันที · ไม่มี waitForTimeout)
        ├─ page.pdf A4 + หัว/ท้ายกระดาษ (เอกสาร service: พิมพ์ ?part=body แล้วต่อ ?part=cover / ?part=back ด้วย PDFsharp)
        ├─ ตรวจชื่อฟอนต์ที่ฝังจริงในไฟล์ — นอกชุดอนุญาต = งานล้ม
        └─ เขียน .part → rename → คืน path ให้คิวอ่าน sha256 แล้วเก็บ blob ลงฐาน
```

🔴 **เครือข่าย — จุดที่พังบ่อยสุด:** Chromium รัน **ในคอนเทนเนอร์ coreapi** และต้องเข้าถึง **2 ที่** ด้วย URL ชุดเดียวกับที่ SPA ใช้
1. `NisReport:CrmBaseUrl` — ที่อยู่ของ CRM SPA (nginx ของ go-crm-24v4) ที่ resolve ได้จากคอนเทนเนอร์ coreapi
2. โฮสต์ของ coreapi ที่ **build เข้า SPA** (`VITE_SERVER_URL`) — หน้าพิมพ์ยิง API ด้วยค่านี้ ไม่ใช่ `localhost` ของคอนเทนเนอร์
   ถ้าเป็นชื่อสาธารณะ (เช่น `https://api.xxx.co.th`) คอนเทนเนอร์ต้อง resolve + ออกไปถึงชื่อนั้นได้ (hairpin ผ่าน reverse proxy)
   ถ้าไม่ได้ ให้ใส่ `extra_hosts`/DNS ภายในให้ชื่อนั้นชี้กลับเข้ามา — **ห้ามแก้ SPA ให้เรียก localhost**

---

## 2. Dockerfile — การเปลี่ยนแปลงที่ต้องทำ (ยังไม่ได้ทำ)

เลือกทาง **A** (แนะนำ — ไม่ต้องดูแล apt ของ Chromium เอง) หรือ **B** (คง base เดิม)

### ทาง A — สลับ base runtime เป็นภาพ Playwright .NET (มี Chromium + system libs ครบ)

```dockerfile
# ─── base: เดิม mcr.microsoft.com/dotnet/aspnet:9.0 → ภาพ Playwright .NET (Ubuntu 24.04 noble · มี Chromium ของ Playwright 1.63 ติดมา)
# 🔴 เวอร์ชันต้องตรงกับ Microsoft.Playwright ใน goalongapi.csproj (1.63.0) — ไม่ตรง Playwright จะหา browser revision ไม่เจอตอนรัน ไม่ใช่ตอน build
FROM mcr.microsoft.com/playwright/dotnet:v1.63.0-noble AS base
WORKDIR /app
EXPOSE 6600
ENV ASPNETCORE_URLS=http://+:6600 \
    TZ=Asia/Bangkok
# Chromium ของ Playwright ในภาพนี้ติดตั้งไว้ที่ /ms-playwright (root) — ให้ user ที่รันแอปมองเห็นผ่าน env นี้
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN apt-get update && apt-get install -y --no-install-recommends fontconfig \
 && rm -rf /var/lib/apt/lists/* \
 && ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app

# ─── fonts: stage แยก (แก้โค้ดแล้ว cache ชั้นนี้ไม่พัง) — 8 ตระกูล SIL OFL 1.1 จาก repo google/fonts พร้อม OFL.txt (ข้อ 4 ของ OFL บังคับแนบสัญญา)
#     5 ตัวที่ผู้ใช้เลือกได้ + Noto Sans / Noto Sans Symbols 2 (ตัวสำรองที่เราเลือกเอง — ไม่ใส่ fontconfig จะหยิบ WenQuanYi/Loma มาเงียบ ๆ)
#     + IBM Plex Mono (ป้ายเล็กบนปก/เลขหน้าแบบ service)
FROM debian:bookworm-slim AS fonts
RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /fonts
ARG GF=https://raw.githubusercontent.com/google/fonts/main/ofl
# -g (globoff) จำเป็น — ฟอนต์ variable ชื่อไฟล์มี [wdth,wght] ซึ่ง curl ตีความเป็นช่วง URL
RUN set -eux; \
    for F in sarabun notosansthai prompt ibmplexsansthai trirong notosans notosanssymbols2 ibmplexmono; do \
      mkdir -p "/fonts/$F"; \
      curl -fsSL "$GF/$F/OFL.txt" -o "/fonts/$F/OFL.txt"; \
      curl -fsSL "$GF/$F/METADATA.pb" -o /tmp/meta.pb; \
      grep -o 'filename: "[^"]*\.ttf"' /tmp/meta.pb | sed 's/filename: "//;s/"//' | sort -u \
      | while read -r TTF; do curl -g -fsSL "$GF/$F/$TTF" -o "/fonts/$F/$TTF"; done; \
    done; \
    find /fonts -name '*.ttf' | wc -l

# ─── build / publish: เหมือนเดิมทุกบรรทัด (sdk:9.0)

# ─── final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# ฟอนต์ต้องอยู่ในคอนเทนเนอร์ — ขาดแล้ว Chromium ถอยไปตัวสำรองเงียบ ๆ แล้วด่านฟอนต์จะล้มงานทุกใบ
COPY --from=fonts /fonts /usr/share/fonts/truetype/nsr
RUN fc-cache -f >/dev/null && fc-list :lang=th family | sort -u
# โฟลเดอร์ PDF ที่เรนเดอร์แล้ว (NisReport:PdfOutputDir ว่าง = /app/App_Data/nis-report-pdf) — mount เป็น volume ใน compose
RUN mkdir -p /app/App_Data && chown -R appuser /app/App_Data
USER appuser
ENTRYPOINT ["dotnet", "goalongapi.dll"]
```

หมายเหตุ: ภาพ `playwright/dotnet` ใหญ่กว่า `aspnet:9.0` ประมาณ **+500 MB** (Chromium + libs) · ถ้าเลือกทางนี้ `NisReport:ChromiumExecutablePath` เว้นว่างได้

### ทาง B — คง `mcr.microsoft.com/dotnet/aspnet:9.0` แล้วติดตั้ง Chromium ด้วย playwright.ps1

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# … (เดิม) …
# ติดตั้ง Chromium + system deps ด้วยสคริปต์ที่ build ออกมากับโปรเจค — ต้องมี pwsh และรันเป็น root ก่อนสลับเป็น appuser
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN apt-get update && apt-get install -y --no-install-recommends wget ca-certificates fontconfig \
 && wget -qO /tmp/pwsh.tgz https://github.com/PowerShell/PowerShell/releases/download/v7.4.6/powershell-7.4.6-linux-x64.tar.gz \
 && mkdir -p /opt/pwsh && tar -xzf /tmp/pwsh.tgz -C /opt/pwsh && chmod +x /opt/pwsh/pwsh && ln -s /opt/pwsh/pwsh /usr/bin/pwsh \
 && pwsh /app/playwright.ps1 install --with-deps chromium \
 && rm -rf /var/lib/apt/lists/* /tmp/pwsh.tgz
COPY --from=fonts /fonts /usr/share/fonts/truetype/nsr
RUN fc-cache -f >/dev/null && chmod -R a+rX /ms-playwright && mkdir -p /app/App_Data && chown -R appuser /app/App_Data
USER appuser
ENTRYPOINT ["dotnet", "goalongapi.dll"]
```

(stage `fonts` เหมือนทาง A) · `--with-deps` ติดตั้ง lib ของ Chromium ผ่าน apt ให้เอง · บน arm64 ให้เปลี่ยน URL ของ pwsh เป็น `linux-arm64`

### docker-compose (prod) — สิ่งที่ต้องเพิ่ม (แจ้งก่อนแก้)

```yaml
services:
  erpapi:
    environment:
      NisReport__CrmBaseUrl: "http://crm"            # ชื่อ service/host ของ nginx CRM ที่ resolve ได้จากคอนเทนเนอร์นี้
      NisReport__PdfOutputDir: "/app/App_Data/nis-report-pdf"
      # NisReport__ChromiumExecutablePath: ""        # เว้นว่าง = Chromium ของ Playwright ในภาพ
    volumes:
      - nis-report-pdf:/app/App_Data/nis-report-pdf  # ไฟล์ PDF อยู่นอกภาพ (ตัวจริงยังอยู่ใน NisReport.PdfBlob ด้วย)
    shm_size: "512m"                                 # Chromium ใช้ /dev/shm · --disable-dev-shm-usage ช่วยอยู่แล้ว แต่ให้เผื่อ
    extra_hosts:
      - "api.ชื่อสาธารณะของ-coreapi:host-gateway"   # ถ้า SPA ถูก build ด้วย VITE_SERVER_URL เป็นชื่อสาธารณะ และคอนเทนเนอร์ resolve ไม่ได้
volumes:
  nis-report-pdf:
```

---

## 3. RAM / CPU / ความพร้อมกัน

- Chromium ใช้ **~0.5–1 GB RAM ต่อการเรนเดอร์** (เอกสาร 20–40 หน้า · กราฟ SVG) + โปรเซส node driver ของ Playwright ~100 MB
  → เผื่อ limit ของคอนเทนเนอร์ coreapi เพิ่มอย่างน้อย **1.5 GB** จากเดิม
- **ความพร้อมกัน = 1** — บังคับสองชั้น: `NisReportPdfQueueService` (SqlAppLock `goalongapi:nis-report-pdf` ข้าม replica)
  และ `SemaphoreSlim(1)` ในตัวเรนเดอร์ · 9–15 ฉบับ/เดือน ไม่ต้องขนาน
- เบราว์เซอร์เปิดค้างไว้ตัวเดียวทั้งโปรเซส (เปิดใหม่เองถ้าหลุด) — RAM ค้างราว 150–250 MB ตอนว่าง
- `ERPAPI_REPLICAS=2` ได้ — lock ของคิวทำให้มีตัวเรนเดอร์วิ่งทีละตัว แต่ **ทุก replica ต้องมี Chromium + ฟอนต์ในภาพ** (ภาพเดียวกันอยู่แล้ว)
- เวลาเรนเดอร์ปกติ 5–20 วินาที/ฉบับ · `NisReport:PdfTimeoutMs` (60000) ใช้กับทุกขั้นรอ (เปิดหน้า · ready · กราฟ · รูป) แยกกัน

---

## 4. Config keys (`appsettings.json` → section `NisReport`)

| key | ค่าเริ่มต้น | ความหมาย |
|---|---|---|
| `CrmBaseUrl` | `http://localhost:8080` | ที่อยู่ CRM SPA ที่ Chromium เปิด (`/nsr-print/{id}`) — ต้องเข้าถึงได้จากคอนเทนเนอร์ coreapi |
| `PdfTimeoutMs` | `60000` | เพดานรอแต่ละขั้น (goto · data-print-state · `__CHARTS_READY__` · รูป) |
| `PdfOutputDir` | `""` = `{ContentRoot}/App_Data/nis-report-pdf` | รากของไฟล์ PDF — โครง `YYYY/MM/{customerId}-{reportId}.pdf` (เดือนตามเวลาไทย) |
| `ChromiumArgs` | `["--no-sandbox","--disable-dev-shm-usage"]` | แฟล็กเปิด Chromium — `--no-sandbox` เป็นการตัดสินใจ (ดูคอมเมนต์หัวคลาส) |
| `ChromiumExecutablePath` | `""` = Chromium ที่ Playwright ติดตั้ง | override เมื่อใช้ Chromium ของ distro (`/usr/bin/chromium`) |
| `PrintKeyTtlSeconds` | `300` | อายุ print key — หน้าหนึ่งฉบับถูกเปิดได้ถึง 5 รอบ (หลัก · body · seg=pre · cover · back) ภายในอายุนี้ · key ไม่ถูก consume |

**ไม่ใช่ config (ตั้งใจ):** ระยะขอบ (14 มม. · service 30/18 มม.) และชุดฟอนต์ที่อนุญาต — เป็นค่าคงที่ใน `NisReportSchema`/`NisReportPdfRenderer`
เพราะหน้าพิมพ์ CRM ใช้ตัวเลขเดียวกันยืดหน้าปก (`printCoverMinHeightPx`) · ตั้งต่างกันสองฝั่ง = หน้าปกล้นหน้า

ชุดฟอนต์ที่อนุญาตใน PDF (หลังตัด subset prefix `ABCDEF+` และส่วนหลังขีด): `Sarabun · NotoSansThai · Prompt · IBMPlexSansThai · Trirong · NotoSans · NotoSansSymbols2 · IBMPlexMono`
— เจอตัวอื่น (เช่น `Loma`, `WenQuanYiZenHei`, `Thonburi`) งานล้มด้วยข้อความ `ฟอนต์ที่ฝังไม่อยู่ในชุดอนุญาต: …`

---

## 5. ทดสอบบนเครื่องพัฒนา (macOS)

```bash
cd coreapi-new
dotnet build
# ติดตั้ง Chromium ของ Playwright ครั้งเดียว (ลง ~/Library/Caches/ms-playwright · ~170 MB)
pwsh bin/Debug/net9.0/playwright.ps1 install chromium
#   ไม่มี pwsh: brew install --cask powershell
# ฟอนต์: ติดตั้ง 8 ตระกูลจาก https://fonts.google.com ลง Font Book (Sarabun · Noto Sans Thai · Prompt · IBM Plex Sans Thai · Trirong · Noto Sans · Noto Sans Symbols 2 · IBM Plex Mono)
#   ไม่ติดตั้ง → ด่านฟอนต์จะล้มงานด้วยชื่อฟอนต์ของ macOS (Thonburi/Sukhumvit) — นั่นคือพฤติกรรมที่ถูกต้อง ไม่ใช่บั๊ก
# ตรวจผลด้วย poppler (ไม่บังคับ — ตัวเรนเดอร์ใช้ PDFsharp อ่านเองแล้ว)
brew install poppler
```

`appsettings.Development.json` (หรือ env): `NisReport:CrmBaseUrl` = ที่อยู่ `yarn dev` ของ go-crm-24v4 (`http://localhost:8080`)
และ SPA ต้องถูกตั้ง `VITE_SERVER_URL` ให้ชี้มาที่ coreapi เครื่องเดียวกัน (`http://localhost:5052`) — Chromium จะเปิดหน้าใน SPA แล้ว SPA ยิงมาที่นี่

ถ้ายังไม่ติดตั้ง Chromium งานในคิวจะล้มด้วยข้อความ `ยังไม่ได้ติดตั้ง Chromium สำหรับ Playwright บนเครื่องนี้ (คาดว่าอยู่ที่ …) — …` (ไม่ดาวน์โหลดเอง)

---

## 6. Smoke test (ทำครั้งแรกบนเครื่อง production จริงด้วย — SPEC §9.3)

1. `GET /api/nis/report/company?cmpid=…` มีชื่อบริษัท (ท้ายกระดาษพิมพ์ชื่อผู้ออกเอกสาร — ว่าง = ไม่พิมพ์ ไม่ใช่ error)
2. สร้างรายงานจากแม่แบบ (`POST /api/nis/report/reports`) → ได้ `reportId` สถานะ `draft`
3. `POST /api/nis/report/reports/{id}/generate?cmpid=…` → job `queued`
4. poll `GET /api/nis/report/reports/{id}/job?cmpid=…` — ต้องเห็น `running` + `stage` `rendering` → `writing` → `done` (ไม่ใช่เปอร์เซ็นต์)
   · ล้ม → `error` ต้องเป็นข้อความไทยอ่านออก (เช่น `หน้าพิมพ์ตอบ error: …`, `รอหน้าพิมพ์พร้อมเกิน 60 วินาที — …`, `ฟอนต์ที่ฝังไม่อยู่ในชุดอนุญาต: Loma`)
5. `GET /api/nis/report/reports/{id}/pdf?cmpid=…` → เปิดไฟล์ด้วยตา: ปกเต็มขอบไม่มีเลขหน้า · หน้าถัดไปเริ่ม "หน้า 1 จาก N" · วรรณยุกต์ไทยไม่ลอย · กราฟครบ
6. ตรวจฟอนต์ซ้ำด้วย poppler: `pdffonts <file>.pdf` — ทุกบรรทัดต้องเป็นตระกูลในชุดอนุญาต, `emb = yes`, ไม่มี `Loma`/`DejaVu`/`WenQuanYi`
7. ตรวจไฟล์บนดิสก์: `{PdfOutputDir}/YYYY/MM/{customerId}-{reportId}.pdf` (เดือนไทยของ PeriodStart) ไม่มีไฟล์ `.part` ค้าง
8. ทดสอบกู้งานค้าง: สั่ง generate แล้ว restart coreapi กลางคัน → งานต้องถูกยึดคืน (`queued` + Error บอกว่า "งานถูกยึดคืน (สตาร์ตใหม่)") แล้วเรนเดอร์ใหม่จนสำเร็จ
9. key ใช้ซ้ำหลังหมดอายุ: เปิด `{CrmBaseUrl}/nsr-print/{id}?key=<key เก่า>` หลัง 5 นาที → หน้าพิมพ์ต้องขึ้น `data-print-state="error"` (print-data คืน 401)

---

## 7. สิ่งที่ยังค้าง / ต้องรู้

- **หน้าพิมพ์ฝั่ง CRM (`/nsr-print/:id`) ยังเป็น stub** (`go-crm-24v4/src/sections/nsr/reports/print/nsr-print-report.tsx`) — ตัวเรนเดอร์จะได้ `รอหน้าพิมพ์พร้อมเกิน 60 วินาที — ยังไม่ถึงขั้น "หน้าพิมพ์พร้อม (data-print-state)"` จนกว่าจะ port `print-report.tsx` ของ NSR มาครบ
  สัญญาที่หน้าพิมพ์ต้องทำ: `[data-print-state]` = `loading|ready|error` (error → ข้อความใน element เดียวกัน) · `window.__CHARTS_READY__ = true` หลังกราฟวาดเสร็จ ·
  `[data-print-full-bleed-cover="1"]` / `[data-print-has-back-cover="1"]` / `[data-print-has-after-back="1"]` · query `part=cover|body|back`, `seg=pre` · อ่าน `key` จาก query แล้วเรียก `print-data?key=`
- ฟอนต์ที่ CRM โหลดผ่าน `@fontsource/*` ใช้ได้กับหน้าเว็บ แต่ **PDF ใช้ฟอนต์ที่ติดตั้งในเครื่องที่ Chromium รัน** (fontconfig) — ชื่อ family ต้องตรง (`Sarabun`, `Noto Sans Thai`, …)
- `App_Data/nis-report-pdf` ควรเพิ่มใน `.gitignore` (ยังไม่ได้แก้)
