using System.Net;
using System.Reflection;
using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;
using Newtonsoft.Json.Linq;

namespace goalongapi.Services;

/// <summary>
/// ค่าที่ <b>คงที่ทั้งเล่ม</b> สำหรับหัว/ท้ายกระดาษที่ซ้ำทุกหน้า (SPEC §9 ข้อ 42) — port จาก PageFurniture ใน pdf.service.ts
/// ทุกช่องเป็นข้อความล้วน · <b>ห้ามมีตัวเลขจากงานวิเคราะห์</b> (ช่องนี้อยู่นอกต้นไม้ที่เกตทุกตัวมองอยู่ · ข้อ 38)
/// </summary>
/// <param name="Enabled">ปิดท้ายกระดาษทั้งแถบ (false) — คืน element ว่าง ไม่ใช่สตริงว่าง</param>
/// <param name="LogoPx">ความสูงโลโก้ (px) · 0 = ไม่แสดงโลโก้</param>
/// <param name="ShowPageNumber">พิมพ์ "หน้า N จาก M" ไหม</param>
/// <param name="CompanyName">ชื่อบริษัทผู้ออกเอกสาร ประกอบตาม page.footer.companyName แล้ว (ว่าง = ไม่พิมพ์)</param>
/// <param name="Text">ข้อความเพิ่มท้ายกระดาษที่ผู้ใช้พิมพ์เอง</param>
/// <param name="ContractNo">เลขที่สัญญา</param>
/// <param name="PeriodLabel">ป้ายรอบรายงาน</param>
/// <param name="Service">กรอบหน้าแบบ "เอกสารบริการ NIS" — null = เอกสารทั่วไป</param>
public sealed record NisPdfPageFurniture(
    bool Enabled,
    int LogoPx,
    bool ShowPageNumber,
    string CompanyName,
    string Text,
    string ContractNo,
    string PeriodLabel,
    NisPdfServiceFrame? Service);

/// <summary>กรอบหน้าแบบ "เอกสารบริการ NIS" (PageFurniture.service) — ทุกค่าคงที่ทั้งเล่ม</summary>
/// <param name="Title">ชื่อเอกสาร (docMeta.reportTitle)</param>
/// <param name="ContractNo">เลขที่สัญญา — "" เมื่อ page.header.showContractNo = false</param>
/// <param name="IssueDate">วันที่ออกเอกสารแบบไทยเต็ม — "" เมื่อ page.header.showIssueDate = false</param>
/// <param name="CompanyName">ชื่อบริษัทข้างโลโก้ท้ายกระดาษ</param>
/// <param name="LogoPx">ความสูงโลโก้ท้ายกระดาษ (px) · 0 = ไม่แสดง</param>
/// <param name="ShowPageNumber">พิมพ์ "หน้า N จาก M" ไหม</param>
/// <param name="Text">ข้อความเพิ่มท้ายกระดาษ</param>
/// <param name="Navy">สีแถบข้าง/แถบบน (palette.c3)</param>
/// <param name="Accent">สีแถบบนด้านขวา (palette.ca)</param>
/// <param name="AccentInk">สีเลขหน้า (palette.cad)</param>
/// <param name="Rule">สีเส้นคั่น</param>
/// <param name="Font">ฟอนต์ของเอกสาร (documentFont) — ค่าจาก enum ของ schema เท่านั้น</param>
public sealed record NisPdfServiceFrame(
    string Title,
    string ContractNo,
    string IssueDate,
    string CompanyName,
    int LogoPx,
    bool ShowPageNumber,
    string Text,
    string Navy,
    string Accent,
    string AccentInk,
    string Rule,
    string Font);

/// <summary>
/// ประกอบหัว/ท้ายกระดาษของ PDF จาก layout/data snapshot ที่แช่แข็งไว้ — port จาก pdf-queue.service.ts:229-290 (ค่า) และ
/// pdf.service.ts:208-281 (HTML)
///
/// 🔴 <b>ต้องระบุฟอนต์ในเทมเพลตเสมอ</b> — หัว/ท้ายกระดาษของ page.pdf() เรนเดอร์ในบริบทแยกจากหน้าเว็บ CSS ของหน้าไม่มีผล
///    ไม่ระบุ = Chromium ใช้ค่าปริยายของเครื่อง (ในคอนเทนเนอร์คือ Loma) แล้วด่านตรวจฟอนต์จะล้มงานทั้งใบ (SPEC §9 ข้อ 36 · 38)
/// 🔴 โลโก้เป็น data URI ที่ฝังในแอสเซมบลี — บริบทนี้โหลดไฟล์ภายนอกไม่ได้ และเครื่อง on-prem อาจไม่มีเน็ตออก
/// 🔴 ชื่อบริษัทมาจาก <c>DataSnapshotJson.context.company</c> ที่แช่แข็งตอนสร้างรายงาน ไม่ใช่ค่าสดจากฐาน — เอกสารต้องนิ่ง
/// </summary>
public static class NisReportPdfFurniture
{
    private const string LogoResourceName = "goalongapi.Resources.nsr-logo-nis.png";
    private static readonly Lazy<string> LogoDataUri = new(LoadLogoDataUri);

    /// <summary>โลโก้ NIS เป็น data URI (PNG) — แหล่งเดียวของตัวเรนเดอร์ PDF</summary>
    public static string NisLogoDataUri => LogoDataUri.Value;

    private static string LoadLogoDataUri()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(LogoResourceName)
                           ?? throw new InvalidOperationException($"ไม่พบโลโก้ NIS ที่ฝังในแอสเซมบลี ({LogoResourceName}) — ตรวจ EmbeddedResource ใน goalongapi.csproj");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// ค่าคงที่ทั้งเล่มของรายงานหนึ่งฉบับ — อ่านจาก LayoutSnapshotJson (page.footer · page.header · docMeta · blocks · theme)
    /// และบริษัทที่แช่แข็งใน DataSnapshotJson.context.company
    ///
    /// รายงานที่แช่แข็งก่อนมีช่อง logoSize/companyName ต้องออกหน้าตาเดิม (โลโก้เล็ก · ชื่อไทย) ไม่ใช่ถูกเติมค่าตั้งต้นใหม่
    /// ให้หน้าตาเปลี่ยนเงียบ ๆ ตอนออก PDF ซ้ำ (SPEC §9 ข้อ 59)
    /// </summary>
    public static NisPdfPageFurniture For(NisReport report)
    {
        var layout = NisReportJson.TryParseObject(report.LayoutSnapshotJson) ?? new JObject();
        var snapshot = NisReportJson.TryParseObject(report.DataSnapshotJson);
        var company = NisReportCompanyService.FromJson(snapshot?["context"]?["company"] as JObject);

        var meta = layout["docMeta"] as JObject ?? new JObject();
        var page = layout["page"] as JObject;
        var footer = page?["footer"] as JObject ?? new JObject();
        var header = page?["header"] as JObject;
        var blocks = layout["blocks"] as JArray;

        var enabled = Bool(footer, "enabled", true);
        var showLogo = Bool(footer, "showLogo", true);
        var showPageNumber = Bool(footer, "showPageNumber", true);
        var text = Str(footer, "text");
        var logoSize = Enum(footer, "logoSize", NisReportSchema.FooterLogoSizes, "small");
        var companyNameMode = Enum(footer, "companyName", NisReportSchema.FooterCompanyNames, "th");

        var name = companyNameMode switch
        {
            "th" => company.NameTh,
            "both" => string.Join(" · ", new[] { company.NameEn, company.NameTh }.Where(x => !string.IsNullOrEmpty(x))),
            _ => company.NameEn,
        };
        var logoPx = showLogo ? NisReportSchema.FooterLogoPx[logoSize] : 0;
        var contractNo = Str(meta, "contractNo");

        NisPdfServiceFrame? service = null;
        if (NisReportSchema.IsServiceLayout(blocks))
        {
            var colors = NisReportSchema.ServiceFrameColors(blocks);
            service = new NisPdfServiceFrame(
                Title: Str(meta, "reportTitle"),
                // ปิดได้ทีละช่อง (ตั้งค่าเอกสาร → หัวกระดาษทุกหน้า) · รายงานเก่าที่ไม่มีช่องนี้ = เปิด (หน้าตาเดิม)
                ContractNo: header?["showContractNo"]?.Type == JTokenType.Boolean && !(bool)header["showContractNo"]! ? "" : contractNo,
                IssueDate: header?["showIssueDate"]?.Type == JTokenType.Boolean && !(bool)header["showIssueDate"]! ? "" : NisReportSchema.ThaiLongDate((string?)meta["preparedDate"]),
                CompanyName: name,
                LogoPx: logoPx,
                ShowPageNumber: showPageNumber,
                Text: text,
                Navy: colors.Navy,
                Accent: colors.Accent,
                AccentInk: colors.AccentInk,
                Rule: colors.Rule,
                Font: NisReportSchema.DocumentFont(blocks, (string?)layout["theme"]?["fontFamily"]));
        }

        return new NisPdfPageFurniture(
            Enabled: enabled,
            LogoPx: logoPx,
            ShowPageNumber: showPageNumber,
            CompanyName: name,
            Text: text,
            ContractNo: contractNo,
            PeriodLabel: Str(meta, "periodLabelTh"),
            Service: service);
    }

    /// <summary>headerTemplate ของ page.pdf() — แบบ service มีแถบ/ชื่อเอกสาร/โลโก้ · เอกสารทั่วไปเป็น element ว่าง</summary>
    public static string HeaderHtml(NisPdfPageFurniture f) => f.Service != null ? ServiceHeaderHtml(f.Service) : "<div></div>";

    /// <summary>footerTemplate ของ page.pdf()</summary>
    public static string FooterHtml(NisPdfPageFurniture f) => f.Service != null ? ServiceFooterHtml(f.Service) : PlainFooterHtml(f);

    /// <summary>ชื่อฟอนต์ที่ใส่ลง HTML ของหัว/ท้ายกระดาษได้ — ต้องอยู่ในรายการที่ฝังในภาพเท่านั้น ไม่งั้นถอยเป็น IBM Plex Sans Thai</summary>
    private static string FontName(string font) => NisReportSchema.SelectableFonts.Contains(font) ? font : "IBM Plex Sans Thai";

    private static string Esc(string s) => WebUtility.HtmlEncode(s);

    /// <summary>หัวกระดาษแบบ service — แถบบน · แถบข้างช่วงขอบบน · ชื่อเอกสาร · เลขที่สัญญา | วันที่ออก · โลโก้ · เส้นคั่น</summary>
    private static string ServiceHeaderHtml(NisPdfServiceFrame s)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(s.ContractNo)) parts.Add(Esc($"เลขที่สัญญา: {s.ContractNo}"));
        if (!string.IsNullOrEmpty(s.IssueDate)) parts.Add(Esc($"วันที่ออกเอกสาร: {s.IssueDate}"));
        var line2 = string.Join("&nbsp; | &nbsp;", parts);
        var font = FontName(s.Font);
        // Chromium ใส่ระยะเว้นให้แม่แบบหัว/ท้ายเอง (วัดได้ 20px) — แถบข้าง/แถบบนเลยไม่ชนขอบ · ล้างทิ้ง
        return
            "<style>html,body{margin:0!important;padding:0!important}#header,#footer{padding:0!important}</style>" +
            $"<div style=\"position:relative;width:100%;height:{NisReportSchema.ServicePdfMarginTopMm}mm;margin:0;-webkit-print-color-adjust:exact;print-color-adjust:exact;" +
            $"font-family:'{font}','IBM Plex Sans Thai',sans-serif;\">" +
            $"<div style=\"position:absolute;left:0;top:0;width:30px;height:100%;background:{s.Navy}\"></div>" +
            $"<div style=\"position:absolute;left:30px;top:0;width:239px;height:8px;background:{s.Navy}\"></div>" +
            $"<div style=\"position:absolute;left:269px;right:0;top:0;height:8px;background:{s.Accent}\"></div>" +
            "<div style=\"position:absolute;left:96px;right:61px;top:48px;display:flex;justify-content:space-between;align-items:flex-start\">" +
            "<div style=\"display:flex;flex-direction:column;gap:5px\">" +
            $"<span style=\"font-family:'IBM Plex Mono','{font}',monospace;font-size:8.5px;letter-spacing:0.02em;color:#52514e\">{Esc(s.Title)}</span>" +
            (line2.Length > 0 ? $"<span style=\"font-size:11px;color:#52514e\">{line2}</span>" : "") +
            "</div>" +
            $"<img src=\"{NisLogoDataUri}\" alt=\"\" style=\"height:16px;width:auto\" />" +
            "</div>" +
            $"<div style=\"position:absolute;left:96px;right:61px;top:84px;height:1px;background:{s.Rule}\"></div>" +
            "</div>";
    }

    /// <summary>ท้ายกระดาษแบบ service — แถบข้างช่วงขอบล่าง · เส้นคั่น · โลโก้ + ชื่อบริษัท · หน้า N จาก M สีเน้น</summary>
    private static string ServiceFooterHtml(NisPdfServiceFrame s)
    {
        var font = FontName(s.Font);
        return
            "<style>html,body{margin:0!important;padding:0!important}#header,#footer{padding:0!important}</style>" +
            $"<div style=\"position:relative;width:100%;height:{NisReportSchema.ServicePdfMarginBottomMm}mm;margin:0;-webkit-print-color-adjust:exact;print-color-adjust:exact;" +
            $"font-family:'{font}','IBM Plex Sans Thai',sans-serif;\">" +
            $"<div style=\"position:absolute;left:0;top:0;width:30px;height:100%;background:{s.Navy}\"></div>" +
            $"<div style=\"position:absolute;left:96px;right:61px;top:4px;height:1px;background:{s.Rule}\"></div>" +
            "<div style=\"position:absolute;left:96px;right:61px;top:16px;display:flex;align-items:center;justify-content:space-between;gap:12px\">" +
            "<span style=\"display:flex;align-items:center;gap:14px;overflow:hidden\">" +
            (s.LogoPx > 0 ? $"<img src=\"{NisLogoDataUri}\" alt=\"\" style=\"height:{s.LogoPx}px;width:auto;flex:none\" />" : "") +
            (!string.IsNullOrEmpty(s.CompanyName) ? $"<span style=\"font-size:9px;color:#52514e;white-space:nowrap\">{Esc(s.CompanyName)}</span>" : "") +
            (!string.IsNullOrEmpty(s.Text) ? $"<span style=\"font-size:9px;color:#52514e;overflow:hidden;text-overflow:ellipsis;white-space:nowrap\">{Esc(s.Text)}</span>" : "") +
            "</span>" +
            (s.ShowPageNumber
                ? $"<span style=\"font-family:'IBM Plex Mono','{font}',monospace;font-size:9px;letter-spacing:0.02em;color:{s.AccentInk};white-space:nowrap\">" +
                  "หน้า <span class=\"pageNumber\"></span> จาก <span class=\"totalPages\"></span></span>"
                : "") +
            "</div>" +
            "</div>";
    }

    /// <summary>
    /// ท้ายกระดาษของเอกสารทั่วไป — โลโก้ + ชื่อบริษัท<b>ผู้ออกเอกสาร</b> (ไม่ใช่ชื่อลูกค้า) · สัญญา · รอบ · ข้อความ · เลขหน้า
    /// ปิดทั้งแถบ (Enabled=false) ต้องคืน element ว่าง ไม่ใช่สตริงว่าง — ไม่งั้น Chromium ใส่ท้ายกระดาษค่าตั้งต้นของตัวเองแทน
    /// </summary>
    private static string PlainFooterHtml(NisPdfPageFurniture v)
    {
        if (!v.Enabled) return "<span></span>";
        var rest = string.Join(" · ", new[]
        {
            string.IsNullOrEmpty(v.ContractNo) ? null : $"สัญญาเลขที่ {v.ContractNo}",
            string.IsNullOrEmpty(v.PeriodLabel) ? null : v.PeriodLabel,
            string.IsNullOrEmpty(v.Text) ? null : v.Text,
        }.Where(x => x != null).Select(x => Esc(x!)));
        // ชื่อบริษัทอยู่ชิดโลโก้ ตัวใหญ่กว่าบรรทัดรอง · ขนาดตามโลโก้ให้สมดุล
        var nameSize = v.LogoPx >= 22 ? "9pt" : v.LogoPx >= 16 ? "8pt" : "6.5pt";
        return
            "<div style=\"width:100%;font-family:Sarabun,'Noto Sans Thai','Noto Sans',sans-serif;" +
            "font-size:6.5pt;color:#52514e;padding:0 16px;display:flex;align-items:center;justify-content:space-between;gap:12px\">" +
            "<span style=\"display:flex;align-items:center;gap:8px;overflow:hidden\">" +
            (v.LogoPx > 0 ? $"<img src=\"{NisLogoDataUri}\" alt=\"\" style=\"height:{v.LogoPx}px;width:auto;flex:none\" />" : "") +
            (!string.IsNullOrEmpty(v.CompanyName)
                ? $"<span style=\"font-size:{nameSize};font-weight:600;color:#1f1e1c;white-space:nowrap\">{Esc(v.CompanyName)}</span>"
                : "") +
            (rest.Length > 0 ? $"<span style=\"overflow:hidden;text-overflow:ellipsis;white-space:nowrap\">{rest}</span>" : "") +
            "</span>" +
            (v.ShowPageNumber
                ? "<span style=\"white-space:nowrap\">หน้า <span class=\"pageNumber\"></span> จาก <span class=\"totalPages\"></span></span>"
                : "") +
            "</div>";
    }

    // ── อ่านค่าจาก snapshot แบบทน schema เก่า (เหมือน FooterSchema.safeParse + ค่าตั้งต้นที่ตั้งใจ) ──

    private static bool Bool(JObject o, string key, bool dflt) => o[key]?.Type == JTokenType.Boolean ? (bool)o[key]! : dflt;

    private static string Str(JObject o, string key) => o[key]?.Type == JTokenType.String ? (string)o[key]! : string.Empty;

    private static string Enum(JObject o, string key, string[] allowed, string dflt)
    {
        var v = o[key]?.Type == JTokenType.String ? (string)o[key]! : null;
        return v != null && allowed.Contains(v) ? v : dflt;
    }
}
