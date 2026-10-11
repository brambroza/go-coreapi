using System.Text.RegularExpressions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace goalongapi.Services;

/// <summary>
/// งานบนไฟล์ PDF ที่ตัวเรนเดอร์ต้องทำหลัง Chromium พิมพ์เสร็จ — นับหน้า · แทรกหน้าเต็มขอบ · อ่านชื่อฟอนต์ที่ฝังจริง
/// ใช้ PDFsharp (MIT · managed ล้วน) แทน poppler CLI (pdfinfo/pdfunite/pdfseparate/pdffonts) ของ NSR เดิม —
/// ไม่ต้องติดตั้งไบนารีเพิ่มในภาพ และไม่พึ่ง PATH ของเครื่อง
///
/// 🔴 ชื่อฟอนต์อ่านจาก <c>/Font</c> ใน Resources ของทุกหน้า <b>และของ Form XObject ทุกตัวแบบ recursive</b> —
///    ของที่วาดนอกต้นไม้ที่ตรวจจะมองไม่เห็นเสมอ (SPEC §9 ข้อ 38) · pdffonts ก็ไล่ทางเดียวกัน
/// </summary>
public static class NisReportPdfTools
{
    /// <summary>prefix ของ subset (<c>ABCDEF+</c>) ที่ Chromium ใส่หน้าชื่อฟอนต์ที่ฝัง</summary>
    private static readonly Regex SubsetPrefix = new("^[A-Z]{6}\\+", RegexOptions.Compiled);

    /// <summary>จำนวนหน้าของ PDF</summary>
    public static int PageCount(string file)
    {
        using var doc = PdfReader.Open(file, PdfDocumentOpenMode.Import);
        return doc.PageCount;
    }

    /// <summary>
    /// แทรก<b>หน้าแรก</b>ของ <paramref name="extraPdf"/> เข้าไปใน <paramref name="mainPdf"/> หลังหน้าที่ <paramref name="afterPage"/>
    /// (0 = ไว้หน้าแรก · เท่ากับจำนวนหน้า = ต่อท้าย) แล้วเขียนทับ <paramref name="mainPdf"/> — ใช้ต่อปกหน้า/ปกหลังเต็มขอบ
    /// ที่พิมพ์แยกรอบ (ขอบ 0 · ไม่มีหัว/ท้าย ซึ่ง page.pdf() ตั้งได้ทีละไฟล์)
    /// </summary>
    /// <returns>จำนวนหน้าหลังแทรก</returns>
    /// <exception cref="InvalidOperationException">ตำแหน่งแทรกเกินจำนวนหน้า หรือจำนวนหน้าหลังแทรกไม่เท่าเดิม + 1</exception>
    public static int InsertPage(string mainPdf, string extraPdf, int afterPage, string label)
    {
        using var main = PdfReader.Open(mainPdf, PdfDocumentOpenMode.Import);
        using var extra = PdfReader.Open(extraPdf, PdfDocumentOpenMode.Import);
        var pages = main.PageCount;
        if (pages < 1) throw new InvalidOperationException("อ่านจำนวนหน้าของ PDF เนื้อหาไม่ได้");
        if (extra.PageCount < 1) throw new InvalidOperationException($"PDF ของ{label}ไม่มีหน้าเลย");
        if (afterPage < 0 || afterPage > pages)
            throw new InvalidOperationException($"ตำแหน่งแทรก{label}ไม่ถูกต้อง ({afterPage} จาก {pages} หน้า)");

        using var merged = new PdfDocument();
        merged.Version = Math.Max(main.Version, extra.Version);
        for (var i = 0; i < afterPage; i++) merged.AddPage(main.Pages[i]);
        merged.AddPage(extra.Pages[0]);
        for (var i = afterPage; i < pages; i++) merged.AddPage(main.Pages[i]);

        var tmp = mainPdf + ".merge";
        merged.Save(tmp);
        var after = PageCount(tmp);
        if (after != pages + 1)
        {
            File.Delete(tmp);
            throw new InvalidOperationException($"ต่อหน้า{label}แล้วจำนวนหน้าไม่ตรง — เนื้อหา {pages} + 1 แต่ได้ {after}");
        }
        File.Move(tmp, mainPdf, overwrite: true);
        return after;
    }

    /// <summary>
    /// ตระกูลฟอนต์ที่ฝังอยู่ในไฟล์จริง — ตัด prefix ของ subset และส่วนหลังขีด (<c>ABCDEF+Sarabun-Bold</c> → <c>Sarabun</c>)
    /// แบบเดียวกับที่ NSR อ่านผลของ pdffonts · Type 3 ใช้ชื่อจาก /FontDescriptor/FontName · ไม่มีชื่อเลย ("[none]") ไม่นับ
    /// </summary>
    public static HashSet<string> EmbeddedFontFamilies(string file)
    {
        var families = new HashSet<string>(StringComparer.Ordinal);
        using var doc = PdfReader.Open(file, PdfDocumentOpenMode.Import);
        var seen = new HashSet<PdfDictionary>(ReferenceEqualityComparer.Instance);
        foreach (var page in doc.Pages)
        {
            var resources = page.Elements.GetDictionary("/Resources");
            if (resources != null) CollectFonts(resources, families, seen);
        }
        return families;
    }

    /// <summary>ตระกูลใน PDF ที่ไม่ได้อยู่ในชุดอนุญาต · ว่าง = สะอาด</summary>
    public static List<string> ForeignFonts(string file, IReadOnlySet<string> allowed) =>
        EmbeddedFontFamilies(file).Where(f => !allowed.Contains(f)).OrderBy(f => f, StringComparer.Ordinal).ToList();

    private static void CollectFonts(PdfDictionary resources, HashSet<string> families, HashSet<PdfDictionary> seen)
    {
        if (!seen.Add(resources)) return;

        var fonts = resources.Elements.GetDictionary("/Font");
        if (fonts != null)
        {
            foreach (var key in fonts.Elements.Keys)
            {
                if (Resolve(fonts.Elements[key]) is not PdfDictionary font) continue;
                var baseFont = font.Elements.GetName("/BaseFont");
                if (string.IsNullOrEmpty(baseFont))
                {
                    // Type 3 ไม่มี /BaseFont — pdffonts อ่านชื่อจาก /FontDescriptor → /FontName แทน (Chromium ใส่ไว้ เช่น BAAAAA+Ayuthaya
                    // เมื่อถอยไปฟอนต์ระบบที่ฝังเป็นเส้นโค้ง) · ไม่มีทั้งคู่ = "[none]" ของ pdffonts ไม่นับ
                    baseFont = (Resolve(font.Elements["/FontDescriptor"]) as PdfDictionary)?.Elements.GetName("/FontName") ?? string.Empty;
                }
                if (string.IsNullOrEmpty(baseFont)) continue;
                families.Add(Family(baseFont));
            }
        }

        // ฟอนต์ที่ใช้ภายใน Form XObject (เช่น SVG/กราฟที่ Skia แยกเป็น object) อยู่ใน /Resources ของมันเอง
        var xobjects = resources.Elements.GetDictionary("/XObject");
        if (xobjects == null) return;
        foreach (var key in xobjects.Elements.Keys)
        {
            if (Resolve(xobjects.Elements[key]) is not PdfDictionary xo) continue;
            if (xo.Elements.GetName("/Subtype") != "/Form") continue;
            var inner = xo.Elements.GetDictionary("/Resources");
            if (inner != null) CollectFonts(inner, families, seen);
        }
    }

    private static PdfItem? Resolve(PdfItem? item) => item is PdfReference r ? r.Value : item;

    /// <summary><c>/ABCDEF+IBMPlexSansThai-Medium</c> → <c>IBMPlexSansThai</c></summary>
    public static string Family(string baseFontName)
    {
        var name = baseFontName.TrimStart('/');
        name = SubsetPrefix.Replace(name, string.Empty);
        var dash = name.IndexOf('-');
        return dash >= 0 ? name[..dash] : name;
    }
}
