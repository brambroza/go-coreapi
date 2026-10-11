using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using goalongapi.Models.Nis;
using Microsoft.Playwright;

namespace goalongapi.Services;

/// <summary>
/// ตัวเรนเดอร์ PDF ของ NIS Service Report ด้วย Playwright/Chromium — port จาก apps/api/src/pdf/pdf.service.ts (NSR)
///
/// เปิด <c>{NisReport:CrmBaseUrl}/nsr-print/{reportId}?key={printKey}[&amp;part=cover|body|back][&amp;seg=pre]</c>
/// หน้าพิมพ์ฝั่ง CRM ดึงข้อมูลเองจาก <c>GET api/nis/report/reports/{id}/print-data?key=</c> ของ coreapi
/// 🔴 <b>Chromium ต้องถึงทั้ง CRM และ coreapi ด้วย base URL ชุดเดียวกับที่ SPA ใช้</b> (VITE_SERVER_URL ที่ build เข้า SPA) —
///    ในคอนเทนเนอร์ที่ไม่มีเน็ตออก/ไม่ resolve ชื่อสาธารณะ ต้องให้ CrmBaseUrl ชี้ไปที่ที่ Chromium ในคอนเทนเนอร์เปิดได้จริง
///    และชื่อโฮสต์ของ coreapi ที่ฝังอยู่ใน SPA ต้อง resolve ได้จากคอนเทนเนอร์เดียวกัน (ดู docs/nsr-pdf-renderer.md)
///
/// ## 🔴 รอสถานะ ไม่ใช่รอเวลา (SPEC §9 ข้อ 31 · 35)
/// ห้าม <c>WaitForTimeout</c> ทุกกรณี · รอ <c>[data-print-state]</c> = ready|error (error = ล้มทันทีด้วยข้อความของหน้าเอง)
/// → <c>window.__CHARTS_READY__ === true</c> → รูปทุกใบโหลดครบ · เพรดิเคตคืน null ระหว่างยังไม่พร้อม
///
/// ## 🔴 เขียน <c>.part</c> แล้วค่อย rename — ไฟล์ปลายทางมีสองสถานะเท่านั้น (ยังไม่มี / มีครบ)
/// ## 🔴 ตรวจฟอนต์ที่ฝังจริงทุกฉบับ (ข้อ 32 · 36) — ตระกูลนอก <see cref="AllowedFonts"/> = งานล้ม ไม่ใช่คำเตือน
///
/// ## <c>--no-sandbox</c> — การตัดสินใจ ไม่ใช่แฟล็กที่โผล่มาเฉย ๆ
/// Chromium sandbox ต้องการ user namespace ซึ่งคอนเทนเนอร์ไม่มี · ยอมรับได้เพราะหน้าเดียวที่เปิดคือหน้าพิมพ์ของเราเอง
/// เนื้อหามาจาก snapshot ที่เราเขียน และโลโก้ที่อัปโหลดผ่านการตรวจไบต์ต้นไฟล์แล้ว (NisReportAssetStorage) · ตั้งค่าได้ที่
/// <c>NisReport:ChromiumArgs</c>
///
/// ## วงจรชีวิต
/// singleton (AddSingleton ใน Program.cs) · เบราว์เซอร์ตัวเดียวใช้ซ้ำ สร้างใหม่เมื่อหลุด · เรนเดอร์ได้ทีละงาน (SemaphoreSlim 1)
/// · ไม่ดาวน์โหลด Chromium เอง — ไม่พบไบนารีจะโยนข้อความบอกวิธีติดตั้ง (production ต้องมาในภาพ)
///
/// Configuration (<c>NisReport:*</c>): CrmBaseUrl · PdfTimeoutMs · PdfOutputDir (ว่าง = {ContentRoot}/App_Data/nis-report-pdf)
/// · ChromiumArgs · ChromiumExecutablePath (ว่าง = Chromium ที่ Playwright ติดตั้งเอง)
/// </summary>
public sealed class NisReportPdfRenderer : INisReportPdfRenderer, IAsyncDisposable
{
    /// <summary>
    /// ตระกูลที่ยอมให้ปรากฏใน PDF — ห้าตัวที่ผู้ใช้เลือกได้ + ตัวสำรองที่เราเลือกเอง (Noto Sans · Noto Sans Symbols 2)
    /// + IBM Plex Mono (ป้ายเล็กบนปก/เลขหน้าแบบ service) · ทุกตัว SIL OFL 1.1 · ชื่อหลังตัด subset prefix และส่วนหลังขีด
    /// </summary>
    public static readonly IReadOnlySet<string> AllowedFonts = new HashSet<string>(StringComparer.Ordinal)
    {
        "Sarabun", "NotoSansThai", "Prompt", "IBMPlexSansThai", "Trirong", "NotoSans", "NotoSansSymbols2", "IBMPlexMono",
    };

    /// <summary>สถานะของหน้าพิมพ์ — คืน null จนกว่าจะนิ่งที่ ready/error (คืน "loading" ตรง ๆ = บอกว่าเสร็จแล้ว · ข้อ 35)</summary>
    private const string PrintStateExpr =
        "(() => { const s = document.querySelector('[data-print-state]')?.getAttribute('data-print-state');" +
        " return s === 'ready' || s === 'error' ? s : null; })()";

    private const string ChartsReadyExpr = "window.__CHARTS_READY__ === true";

    /// <summary>รูปทุกใบในหน้าโหลดสำเร็จแล้ว (โลโก้ลูกค้า · QR LINE) — รูปที่เสียไม่มีวันครบ → ล้มพร้อมเหตุผลเมื่อหมดเวลา</summary>
    private const string ImagesReadyExpr =
        "(() => [...document.images].every((i) => i.complete && i.naturalWidth > 0) ? true : null)()";

    private static readonly Regex KeyInUrl = new("([?&]key=)[^&]*", RegexOptions.Compiled);

    private readonly ILogger<NisReportPdfRenderer> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly SemaphoreSlim _browserLock = new(1, 1);
    private readonly string _crmBaseUrl;
    private readonly int _timeoutMs;
    private readonly string _outputRoot;
    private readonly string[] _chromiumArgs;
    private readonly string? _executablePath;

    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public NisReportPdfRenderer(IConfiguration config, IHostEnvironment env, ILogger<NisReportPdfRenderer> logger)
    {
        _logger = logger;
        _crmBaseUrl = (config["NisReport:CrmBaseUrl"] ?? "http://localhost:8080").TrimEnd('/');
        _timeoutMs = config.GetValue<int?>("NisReport:PdfTimeoutMs") ?? 60_000;
        var outDir = config["NisReport:PdfOutputDir"];
        _outputRoot = string.IsNullOrWhiteSpace(outDir)
            ? Path.Combine(env.ContentRootPath, "App_Data", "nis-report-pdf")
            : outDir;
        var args = config.GetSection("NisReport:ChromiumArgs").Get<string[]>();
        _chromiumArgs = args is { Length: > 0 } ? args : new[] { "--no-sandbox", "--disable-dev-shm-usage" };
        var exe = config["NisReport:ChromiumExecutablePath"];
        _executablePath = string.IsNullOrWhiteSpace(exe) ? null : exe;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // สัญญา INisReportPdfRenderer
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public async Task<NisPdfRenderResult> RenderAsync(NisReport report, string printKey, Action<string> onStage, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            return await RenderCoreAsync(report, printKey, onStage, ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<NisPdfRenderResult> RenderCoreAsync(NisReport report, string printKey, Action<string> onStage, CancellationToken ct)
    {
        var started = Stopwatch.StartNew();
        var outPath = FilePathFor(report);
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        var tmp = outPath + ".part";
        var work = Directory.CreateTempSubdirectory("nsr-pdf-");

        var furniture = NisReportPdfFurniture.For(report);
        var browser = await EnsureBrowserAsync(ct);
        var page = await browser.NewPageAsync();
        try
        {
            onStage("rendering");
            var baseUrl = PrintUrl(report.Id, printKey);
            await OpenAndWaitAsync(page, baseUrl, ct);

            // หน้าเว็บประกาศเองว่าเอกสารฉบับนี้มีปกเต็มขอบ / ปกหลัง / บล็อกหลังปกหลังไหม (ค่าเดียวกับที่ NSR ใช้)
            var fullBleedCover = await page.QuerySelectorAsync("[data-print-full-bleed-cover=\"1\"]") != null;
            var hasBackCover = await page.QuerySelectorAsync("[data-print-has-back-cover=\"1\"]") != null;
            var hasAfterBack = await page.QuerySelectorAsync("[data-print-has-after-back=\"1\"]") != null;
            // เอกสารแบบ service: รอบหลักพิมพ์เฉพาะเนื้อหาหลังปก (?part=body) — "หน้าปกไม่นับ หลังหน้าปกเริ่มนับ 1"
            if (fullBleedCover) await OpenAndWaitAsync(page, baseUrl + "&part=body", ct);

            onStage("writing");
            await page.PdfAsync(BodyPdfOptions(furniture, tmp));
            ct.ThrowIfCancellationRequested();

            // มีบล็อกวางไว้หลังปกหลัง → พิมพ์ช่วงแรก (seg=pre) ด้วยค่าชุดเดียวกันเพื่อนับหน้า แล้วแทรกปกหลังตรงรอยต่อ
            int? backAt = null;
            if (fullBleedCover && hasBackCover && hasAfterBack)
            {
                var pre = await browser.NewPageAsync();
                try
                {
                    await OpenAndWaitAsync(pre, baseUrl + "&part=body&seg=pre", ct);
                    var prePdf = Path.Combine(work.FullName, "pre.pdf");
                    await pre.PdfAsync(BodyPdfOptions(furniture, prePdf));
                    var prePages = NisReportPdfTools.PageCount(prePdf);
                    var allPages = NisReportPdfTools.PageCount(tmp);
                    if (prePages < 1 || prePages >= allPages)
                        throw new InvalidOperationException($"นับหน้าช่วงก่อนปกหลังไม่ได้ (ช่วงแรก {prePages} · ทั้งหมด {allPages} หน้า)");
                    backAt = 1 + prePages; // +1 = หน้าปกหน้าที่จะต่อไว้ข้างหน้าก่อน
                }
                finally
                {
                    await pre.CloseAsync();
                }
            }

            // ปกหน้า/ปกหลังเต็มขอบพิมพ์แยกรอบ (ขอบ 0 · ไม่มีหัว/ท้าย · ไม่มีเลขหน้า) แล้วค่อยต่อเข้าไฟล์เนื้อหา
            if (fullBleedCover)
            {
                await AttachFullBleedPageAsync(browser, baseUrl, tmp, "cover", afterPage: 0, work.FullName, ct);
                if (hasBackCover)
                {
                    var pages = NisReportPdfTools.PageCount(tmp);
                    var at = backAt is { } b && b < pages ? b : pages;
                    await AttachFullBleedPageAsync(browser, baseUrl, tmp, "back", at, work.FullName, ct);
                }
            }

            // ด่านฟอนต์ — ไฟล์ยังอยู่ที่ .part จนกว่าจะผ่าน (ข้อ 36)
            var strangers = NisReportPdfTools.ForeignFonts(tmp, AllowedFonts);
            if (strangers.Count > 0)
            {
                throw new InvalidOperationException(
                    $"ฟอนต์ที่ฝังไม่อยู่ในชุดอนุญาต: {string.Join(" · ", strangers)} — " +
                    "แปลว่ามีอักขระที่ฟอนต์ของเราไม่มี แล้ว Chromium ไปหยิบตัวสำรองมาวาดเอง " +
                    "· วรรณยุกต์กับสระไทยจะเพี้ยนตำแหน่งโดยไม่มีใครเห็น (SPEC §9 ข้อ 32 · 36)");
            }

            File.Move(tmp, outPath, overwrite: true);
            var bytes = new FileInfo(outPath).Length;
            var ms = (int)Math.Min(int.MaxValue, started.ElapsedMilliseconds);
            _logger.LogInformation("NIS report pdf: เรนเดอร์ {ReportId} เสร็จ — {Kb} KB · {Ms} ms", report.Id, bytes / 1024, ms);
            return new NisPdfRenderResult(outPath, bytes, ms);
        }
        catch (PlaywrightException e)
        {
            // ข้อความของ Playwright เป็นอังกฤษ/ยาว — ครอบด้วยภาษาคน แต่คง detail ไว้ท้าย (ถูกเก็บลง NisReportJob.Error)
            throw new InvalidOperationException($"Chromium เรนเดอร์ไม่สำเร็จ — {Trim(e.Message)}", e);
        }
        finally
        {
            try { await page.CloseAsync(); } catch { /* เบราว์เซอร์อาจหลุดไปแล้ว */ }
            TryDelete(tmp);
            TryDelete(tmp + ".merge");
            try { work.Delete(recursive: true); } catch { /* best-effort */ }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // เบราว์เซอร์
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>เบราว์เซอร์ตัวเดียวใช้ซ้ำ — สร้างใหม่เมื่อยังไม่มีหรือหลุดการเชื่อมต่อ (Chromium ตาย/ถูก OOM kill)</summary>
    private async Task<IBrowser> EnsureBrowserAsync(CancellationToken ct)
    {
        if (_browser is { IsConnected: true }) return _browser;
        await _browserLock.WaitAsync(ct);
        try
        {
            if (_browser is { IsConnected: true }) return _browser;
            if (_browser != null)
            {
                _logger.LogWarning("NIS report pdf: Chromium หลุดการเชื่อมต่อ — เปิดใหม่");
                try { await _browser.CloseAsync(); } catch { /* ตายไปแล้ว */ }
                _browser = null;
            }
            _playwright ??= await Playwright.CreateAsync();

            var exe = _executablePath ?? _playwright.Chromium.ExecutablePath;
            if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
            {
                // ไม่ดาวน์โหลดเองโดยเด็ดขาด — production ต้องมีมาในภาพ (docs/nsr-pdf-renderer.md)
                throw new InvalidOperationException(
                    $"ยังไม่ได้ติดตั้ง Chromium สำหรับ Playwright บนเครื่องนี้ (คาดว่าอยู่ที่ {exe ?? "(ไม่ทราบ)"}) — " +
                    "เครื่องพัฒนา: รัน `pwsh bin/Debug/net9.0/playwright.ps1 install chromium` " +
                    "(macOS ไม่มี pwsh: `brew install --cask powershell` ก่อน) · " +
                    "production: build image ตาม docs/nsr-pdf-renderer.md หรือตั้ง NisReport:ChromiumExecutablePath — ระบบไม่ดาวน์โหลดเองอัตโนมัติ");
            }

            try
            {
                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = _chromiumArgs,
                    ExecutablePath = _executablePath,
                    Timeout = _timeoutMs,
                });
            }
            catch (PlaywrightException e)
            {
                throw new InvalidOperationException($"เปิด Chromium ไม่ได้ — {Trim(e.Message)}", e);
            }
            _logger.LogInformation("NIS report pdf: เปิด Chromium {Version} (args: {Args})", _browser.Version, string.Join(" ", _chromiumArgs));
            return _browser;
        }
        finally
        {
            _browserLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            try { await _browser.CloseAsync(); } catch { /* host กำลังปิด */ }
            _browser = null;
        }
        _playwright?.Dispose();
        _playwright = null;
        _gate.Dispose();
        _browserLock.Dispose();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // เปิดหน้าพิมพ์แล้วรอจนพร้อมพิมพ์จริง
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>เปิดหน้าพิมพ์แล้วรอจนพร้อมพิมพ์จริง — สถานะนิ่ง · กราฟวาดเสร็จ · รูปทุกใบโหลดครบ (§9 ข้อ 31 · 35)</summary>
    private async Task OpenAndWaitAsync(IPage page, string url, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        IResponse? res;
        try
        {
            res = await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = _timeoutMs });
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException($"เปิดหน้าพิมพ์ไม่ทัน {_timeoutMs / 1000} วินาที ที่ {Redact(url)} — CRM ตอบช้าหรือเข้าไม่ถึงจาก Chromium");
        }
        catch (PlaywrightException e)
        {
            throw new InvalidOperationException($"เปิดหน้าพิมพ์ไม่ได้ ที่ {Redact(url)} — {Trim(e.Message)} (Chromium ต้องถึง CRM ด้วย NisReport:CrmBaseUrl)", e);
        }
        if (res == null || !res.Ok)
            throw new InvalidOperationException($"เปิดหน้าพิมพ์ไม่ได้ — HTTP {(res?.Status.ToString() ?? "(ไม่มีคำตอบ)")} ที่ {Redact(url)}");

        var state = await WaitAsync<string>(page, PrintStateExpr, "หน้าพิมพ์พร้อม (data-print-state)");
        if (state == "error")
        {
            var why = string.Empty;
            try { why = await page.Locator("[data-print-state=\"error\"]").First.InnerTextAsync(new LocatorInnerTextOptions { Timeout = 5_000 }); }
            catch (PlaywrightException) { /* ไม่มีข้อความให้อ่าน */ }
            why = Regex.Replace(why, @"\s+", " ").Trim();
            throw new InvalidOperationException($"หน้าพิมพ์ตอบ error: {(why.Length > 400 ? why[..400] : why)}");
        }
        if (state != "ready") throw new InvalidOperationException($"หน้าพิมพ์อยู่ในสถานะ \"{state}\" ซึ่งไม่ใช่ ready");

        await WaitAsync<bool>(page, ChartsReadyExpr, "กราฟวาดเสร็จ (window.__CHARTS_READY__)");
        await WaitAsync<bool>(page, ImagesReadyExpr, "รูปทุกใบโหลดครบ");
    }

    /// <summary>waitForFunction แบบ poll 100 ms — หมดเวลา → ข้อความไทยบอกว่ารออะไรอยู่</summary>
    private async Task<T> WaitAsync<T>(IPage page, string expression, string what)
    {
        try
        {
            var handle = await page.WaitForFunctionAsync(expression, null, new PageWaitForFunctionOptions { Timeout = _timeoutMs, PollingInterval = 100 });
            return await handle.JsonValueAsync<T>();
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException($"รอหน้าพิมพ์พร้อมเกิน {_timeoutMs / 1000} วินาที — ยังไม่ถึงขั้น \"{what}\"");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // พิมพ์
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>ค่าพิมพ์ของเนื้อหา — A4 · พื้นหลัง · หัว/ท้ายกระดาษจากตัวเรนเดอร์ · ระยะขอบตามแบบเอกสาร</summary>
    private static PagePdfOptions BodyPdfOptions(NisPdfPageFurniture f, string path)
    {
        var service = f.Service != null;
        return new PagePdfOptions
        {
            Path = path,
            Format = "A4",
            PrintBackground = true,
            // เลขหน้ามาจากตัวเรนเดอร์ — หน้าเว็บไม่รู้ว่าเอกสารจะยาวกี่หน้า
            DisplayHeaderFooter = true,
            HeaderTemplate = NisReportPdfFurniture.HeaderHtml(f),
            FooterTemplate = NisReportPdfFurniture.FooterHtml(f),
            // ค่ากลางตัวเดียวกับที่หน้าพิมพ์ใช้ยืดหน้าปก (printCoverMinHeightPx) — NisReportSchema เป็นแหล่งเดียว
            Margin = service
                ? new Margin { Top = $"{NisReportSchema.ServicePdfMarginTopMm}mm", Right = "0mm", Bottom = $"{NisReportSchema.ServicePdfMarginBottomMm}mm", Left = "0mm" }
                : new Margin { Top = $"{NisReportSchema.PdfPageMarginMm}mm", Right = "0mm", Bottom = $"{NisReportSchema.PdfPageMarginMm}mm", Left = "0mm" },
        };
    }

    /// <summary>
    /// พิมพ์หน้าเต็มขอบ (ไม่มีหัว/ท้าย · ขอบ 0 · หน้าเดียว) จาก <c>?part=cover|back</c> แล้วแทรกเข้า <paramref name="mainPdf"/>
    /// หลังหน้าที่ <paramref name="afterPage"/> (0 = ปกหน้า · เท่ากับจำนวนหน้า = ต่อท้าย)
    /// </summary>
    private async Task AttachFullBleedPageAsync(IBrowser browser, string baseUrl, string mainPdf, string part, int afterPage, string workDir, CancellationToken ct)
    {
        var page = await browser.NewPageAsync();
        try
        {
            await OpenAndWaitAsync(page, $"{baseUrl}&part={part}", ct);
            var extra = Path.Combine(workDir, $"{part}.pdf");
            await page.PdfAsync(new PagePdfOptions
            {
                Path = extra,
                Format = "A4",
                PrintBackground = true,
                DisplayHeaderFooter = false,
                Margin = new Margin { Top = "0mm", Right = "0mm", Bottom = "0mm", Left = "0mm" },
                PageRanges = "1",
            });
            NisReportPdfTools.InsertPage(mainPdf, extra, afterPage, part == "cover" ? "ปกหน้า" : "ปกหลัง");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // path · url · helpers
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// ที่อยู่ไฟล์ตาม SPEC 6.4 — <c>{PdfOutputDir}/{yyyy}/{MM}/{customerId}-{reportId}.pdf</c> · เดือนตามเวลาไทย
    /// (PeriodStart คือเที่ยงคืนไทยของวันที่ 1 → อ่านเป็น UTC จะได้เดือนก่อนหน้า)
    /// </summary>
    public string FilePathFor(NisReport report)
    {
        if (!NisReportSchema.CustomerIdRegex.IsMatch(report.CustomerId))
            throw new InvalidOperationException($"รหัสลูกค้า \"{report.CustomerId}\" ใช้เป็นชื่อไฟล์ไม่ได้ (A-Z a-z 0-9 _ - เท่านั้น)");
        var bkk = goalongapi.Helpers.NisBangkokTime.ToBangkok(report.PeriodStart);
        return Path.Combine(_outputRoot, bkk.ToString("yyyy", CultureInfo.InvariantCulture), bkk.ToString("MM", CultureInfo.InvariantCulture), $"{report.CustomerId}-{report.Id}.pdf");
    }

    private string PrintUrl(Guid reportId, string printKey) => $"{_crmBaseUrl}/nsr-print/{reportId}?key={Uri.EscapeDataString(printKey)}";

    /// <summary>ซ่อน print key ก่อนเอา URL ไปใส่ข้อความ error/log (key คือสิทธิ์เปิดข้อมูลรายงาน)</summary>
    private static string Redact(string url) => KeyInUrl.Replace(url, "$1…");

    private static string Trim(string message)
    {
        var oneLine = Regex.Replace(message, @"\s+", " ").Trim();
        return oneLine.Length > 300 ? oneLine[..300] + "…" : oneLine;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* best-effort */ }
    }
}
