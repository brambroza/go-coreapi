using System.Globalization;
using goalongapi.Dtos.Nis;
using Newtonsoft.Json.Linq;

namespace goalongapi.Services;

// ── NisReportCoverContrast ──────────────────────────────────────────────────
// ด่านสีหน้าปก (SPEC §9 ข้อ 18 · 20) — port จาก packages/report-schema/src/{cover-theme,cover-registry,cover-contrast}.ts
// เกณฑ์คำนวณจากขนาด+น้ำหนักจริงของตัวอักษร (4.5:1 ถ้าไม่ใช่ตัวใหญ่ · 3:1 ถ้าใหญ่) · วัดพื้นหลังที่ตัวอักษร
// วางอยู่จริง (ไล่เฉดวัดที่จุดแย่ที่สุด) · ปฏิเสธที่ทางออก ไม่ใช่ที่ช่องเลือกสี
// เลขทุกตัว (ชุดสี · ขนาดตัวอักษร · alpha ของวงกลมตกแต่ง) คัดลอกจาก TS ตรงตัว — หน้าออกแบบฝั่ง CRM ใช้เลขชุดเดียวกัน

/// <summary>เฉดทั้ง 6 ของหน้าปก (CoverPalette)</summary>
public sealed record NisCoverPalette(string C1, string C2, string C3, string Ca, string Ca2, string Cad, string OnDark);

/// <summary>สเปกของข้อความหนึ่งชิ้นบนปก (CoverTextSpec)</summary>
public sealed record NisCoverTextSpec(double SizePx, int Weight, string Surface, string Ink);

/// <summary>คำนวณสี · ทะเบียนข้อความบนปก · ตัวตรวจ contrast · ด่าน export</summary>
public static class NisReportCoverContrast
{
    /// <summary>ชุดสำเร็จรูป 6 ชุด (COVER_THEME_COLORS) — เก็บแค่สีหลักกับสีเน้น</summary>
    public static readonly Dictionary<string, (string Main, string Accent)> ThemeColors = new(StringComparer.Ordinal)
    {
        ["nis"] = ("#1e5a8e", "#f78e1e"),
        ["navy"] = ("#1b3a63", "#e0873a"),
        ["emerald"] = ("#175e49", "#e8a33d"),
        ["wine"] = ("#7a2438", "#d19a4e"),
        ["graphite"] = ("#3c4147", "#c9852f"),
        ["violet"] = ("#4a2b6b", "#d9a13c"),
    };

    public const string CoverSoftBg = "#f6f7f9";
    /// <summary>เส้นคั่นของกรอบหน้าแบบ service (COVER_SERVICE_LINES.rule) — หัว/ท้ายกระดาษ PDF ใช้ตัวนี้</summary>
    public const string CoverServiceRule = "#e2e6ea";
    public const double CoverTintAlpha = 0.1;
    public const double CoverRingAlpha = 0.22;
    private const double AaText = 4.5;

    /// <summary>ชิ้นส่วนข้อความบนปกทุกชิ้น (COVER_TEXT_IDS)</summary>
    public static readonly string[] TextIds =
    {
        "eyebrow", "title", "period",
        "preparedForLabel", "customerNameTh", "customerNameEn",
        "contractLabel", "contractValue",
        "companyName", "companyAddress", "preparedBy", "confidential",
        "titleAccent", "sideBrand", "bandLabel", "bandWebsite", "lineId",
    };

    private static readonly Dictionary<string, string> LabelTh = new(StringComparer.Ordinal)
    {
        ["eyebrow"] = "ข้อความเหนือชื่อ",
        ["title"] = "ชื่อรายงาน",
        ["period"] = "รอบรายงาน",
        ["preparedForLabel"] = "ป้าย \"จัดทำเพื่อ\"",
        ["customerNameTh"] = "ชื่อลูกค้า (ไทย)",
        ["customerNameEn"] = "ชื่อลูกค้า (อังกฤษ)",
        ["contractLabel"] = "หัวช่องในตารางสัญญา",
        ["contractValue"] = "ค่าในตารางสัญญา",
        ["companyName"] = "ชื่อบริษัทท้ายหน้า",
        ["companyAddress"] = "ที่อยู่บริษัท",
        ["preparedBy"] = "ผู้จัดทำ / วันที่",
        ["confidential"] = "ป้าย CONFIDENTIAL",
        ["titleAccent"] = "คำในชื่อรายงานที่เน้นสี",
        ["sideBrand"] = "ชื่อบริษัทแนวตั้งบนแถบข้าง",
        ["bandLabel"] = "หัวช่องในแถบติดต่อ",
        ["bandWebsite"] = "เว็บไซต์ในแถบติดต่อ",
        ["lineId"] = "ชื่อ LINE ในแถบติดต่อ",
    };

    private static readonly Dictionary<string, int> WeightNum = new(StringComparer.Ordinal)
    {
        ["normal"] = 400, ["medium"] = 500, ["bold"] = 700, ["black"] = 800,
    };

    // ── ทะเบียนข้อความ (cover-registry.ts) ────────────────────────────────

    private static readonly Dictionary<string, NisCoverTextSpec> OnPaper = new(StringComparer.Ordinal)
    {
        ["preparedForLabel"] = new(8.5, 600, "paper", "cad"),
        ["customerNameTh"] = new(19, 600, "paper", "c3"),
        ["customerNameEn"] = new(10, 400, "paper", "ink2"),
        ["contractLabel"] = new(8, 400, "paper", "ink2"),
        ["contractValue"] = new(11, 500, "paper", "ink"),
        ["companyName"] = new(10.5, 600, "paper", "c3"),
        ["companyAddress"] = new(8, 400, "paper", "ink2"),
        ["preparedBy"] = new(9, 400, "paper", "ink2"),
        ["confidential"] = new(7.5, 600, "c3", "onDark"),
        ["titleAccent"] = new(30, 700, "paper", "cad"),
        ["sideBrand"] = new(7.5, 400, "c3", "onDark"),
        ["bandLabel"] = new(8, 400, "paper", "ink2"),
        ["bandWebsite"] = new(8.5, 400, "paper", "ink2"),
        ["lineId"] = new(13, 400, "paper", "c3"),
    };

    private static readonly Dictionary<string, NisCoverTextSpec> Service = new(StringComparer.Ordinal)
    {
        ["eyebrow"] = new(10, 400, "paper", "cad"),
        ["title"] = new(42, 600, "tint", "c3"),
        ["titleAccent"] = new(42, 600, "tint", "cad"),
        ["period"] = new(15, 400, "tint", "c3"),
        ["preparedForLabel"] = new(9, 400, "paper", "ink2"),
        ["customerNameTh"] = new(25, 400, "paper", "c3"),
        ["customerNameEn"] = new(9.5, 400, "paper", "cad"),
        ["contractLabel"] = new(8.5, 400, "tint", "ink2"),
        ["contractValue"] = new(15, 400, "tint", "c3"),
        ["companyName"] = new(14, 400, "soft", "c3"),
        ["companyAddress"] = new(10.5, 400, "soft", "c3"),
        ["bandLabel"] = new(8, 400, "soft", "ink2"),
        ["bandWebsite"] = new(8.5, 400, "soft", "ink2"),
        ["lineId"] = new(13, 400, "soft", "c3"),
        ["preparedBy"] = new(9, 400, "paper", "ink2"),
        ["confidential"] = new(13, 500, "paper", "cad"),
        ["sideBrand"] = new(7.5, 400, "c3", "onDark"),
    };

    private static readonly Dictionary<string, (double Title, string Surface, bool Dark)> Head = new(StringComparer.Ordinal)
    {
        ["network"] = (30, "gradient", true),
        ["solid"] = (42, "solid", true),
        ["clean"] = (34, "paper", false),
        ["split"] = (24, "gradient", true),
        ["framed"] = (32, "paper", false),
        ["fullbleed"] = (38, "gradient", true),
        ["corner"] = (32, "paper", false),
        ["minimal"] = (28, "paper", false),
        ["service"] = (42, "tint", false),
    };

    /// <summary>สเปกของข้อความทุกชิ้นในแบบหน้าปกหนึ่ง (coverTextSpecs)</summary>
    public static Dictionary<string, NisCoverTextSpec> TextSpecs(string style)
    {
        if (style == "service") return new Dictionary<string, NisCoverTextSpec>(Service, StringComparer.Ordinal);
        if (!Head.TryGetValue(style, out var h)) h = Head["network"];
        string HeadInk(string onDark, string onPaper) => h.Dark ? onDark : onPaper;
        var specs = new Dictionary<string, NisCoverTextSpec>(OnPaper, StringComparer.Ordinal)
        {
            ["eyebrow"] = new(11, 600, h.Surface, HeadInk("ca2", "cad")),
            ["title"] = new(h.Title, 700, h.Surface, HeadInk("onDark", "c3")),
            ["period"] = new(12, 400, h.Surface, HeadInk("onDark", "ink2")),
        };
        return specs;
    }

    // ── แปลงสี (cover-theme.ts) ───────────────────────────────────────────

    private static (int R, int G, int B) HexToRgb(string hex)
    {
        if (!NisReportSchema.HexColorRegex.IsMatch(hex)) throw new ArgumentException($"สีต้องเป็นรูปแบบ #rrggbb (ได้ \"{hex}\")");
        var n = int.Parse(hex[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return ((n >> 16) & 255, (n >> 8) & 255, n & 255);
    }

    private static string RgbToHex(double r, double g, double b)
    {
        static string C(double v) => ((int)Math.Round(Math.Min(255, Math.Max(0, v)), MidpointRounding.AwayFromZero)).ToString("x2");
        return $"#{C(r)}{C(g)}{C(b)}";
    }

    private static (double H, double S, double L) RgbToHsl(int r, int g, int b)
    {
        double R = r / 255d, G = g / 255d, B = b / 255d;
        var max = Math.Max(R, Math.Max(G, B));
        var min = Math.Min(R, Math.Min(G, B));
        var l = (max + min) / 2;
        if (max == min) return (0, 0, l);
        var d = max - min;
        var s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        double h;
        if (max == R) h = (G - B) / d + (G < B ? 6 : 0);
        else if (max == G) h = (B - R) / d + 2;
        else h = (R - G) / d + 4;
        return (h * 60, s, l);
    }

    private static string HslToHex(double h, double s, double l)
    {
        var c = (1 - Math.Abs(2 * l - 1)) * s;
        var hp = ((h % 360) + 360) % 360 / 60;
        var x = c * (1 - Math.Abs(hp % 2 - 1));
        double r1, g1, b1;
        if (hp < 1) (r1, g1, b1) = (c, x, 0);
        else if (hp < 2) (r1, g1, b1) = (x, c, 0);
        else if (hp < 3) (r1, g1, b1) = (0, c, x);
        else if (hp < 4) (r1, g1, b1) = (0, x, c);
        else if (hp < 5) (r1, g1, b1) = (x, 0, c);
        else (r1, g1, b1) = (c, 0, x);
        var m = l - c / 2;
        return RgbToHex((r1 + m) * 255, (g1 + m) * 255, (b1 + m) * 255);
    }

    private static double Clamp(double v, double lo, double hi) => Math.Min(hi, Math.Max(lo, v));

    /// <summary>อัตราส่วนความต่างตาม WCAG (contrastRatio)</summary>
    public static double ContrastRatio(string a, string b)
    {
        static double Lum(string hex)
        {
            var (r, g, bl) = HexToRgb(hex);
            double Lin(int v) { var s = v / 255d; return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4); }
            return 0.2126 * Lin(r) + 0.7152 * Lin(g) + 0.0722 * Lin(bl);
        }
        var l1 = Lum(a);
        var l2 = Lum(b);
        var (hi, lo) = l1 >= l2 ? (l1, l2) : (l2, l1);
        return (hi + 0.05) / (lo + 0.05);
    }

    private static string SeekLightness(double h, double s, double startL, string against, double target)
    {
        for (var step = 0; step <= 100; step++)
        {
            foreach (var dir in step == 0 ? new[] { 0 } : new[] { -1, 1 })
            {
                var l = startL + dir * step * 0.01;
                if (l < 0 || l > 1) continue;
                var hex = HslToHex(h, s, l);
                if (ContrastRatio(hex, against) >= target) return hex;
            }
        }
        return ContrastRatio("#ffffff", against) >= target ? "#ffffff" : "#000000";
    }

    /// <summary>คำนวณเฉดทั้ง 6 จากสองสี (coverPalette) — บังคับแค่ช่วงความสว่างตามข้อ 6.2 ไม่ดึงสีให้เงียบ ๆ</summary>
    public static NisCoverPalette CoverPalette(string main, string accent)
    {
        var (mr, mg, mb) = HexToRgb(main);
        var (ar, ag, ab) = HexToRgb(accent);
        var (mh, ms, ml) = RgbToHsl(mr, mg, mb);
        var (ah, asat, al) = RgbToHsl(ar, ag, ab);

        var c1 = HslToHex(mh, ms, Clamp(ml + 0.09, 0.20, 0.44));
        var c2 = HslToHex(mh, ms, Clamp(ml, 0.20, 0.38));
        var c3 = HslToHex(mh, ms, Clamp(ml - 0.13, 0.11, 0.25));
        var onDark = ContrastRatio("#ffffff", c1) >= ContrastRatio("#000000", c1) ? "#ffffff" : "#000000";
        var ca2 = SeekLightness(ah, asat, Clamp(al + 0.14, 0.55, 0.78), c1, AaText);
        var cad = SeekLightness(ah, asat, Clamp(al - 0.10, 0.26, 0.40), "#ffffff", AaText);
        return new NisCoverPalette(c1, c2, c3, accent, ca2, cad, onDark);
    }

    /// <summary>เฉดของบล็อกหน้าปกใบหนึ่ง (paletteOf) — custom ใช้สีที่ผู้ใช้เลือก นอกนั้นใช้ชุดสำเร็จรูป</summary>
    public static NisCoverPalette PaletteOf(string theme, string? mainColor, string? accentColor)
    {
        if (theme == "custom")
            return CoverPalette(mainColor ?? ThemeColors["nis"].Main, accentColor ?? ThemeColors["nis"].Accent);
        var t = ThemeColors.TryGetValue(theme, out var tc) ? tc : ThemeColors["nis"];
        return CoverPalette(t.Main, t.Accent);
    }

    // ── พื้นหลังที่ตัวอักษรวางอยู่จริง (cover-contrast.ts) ───────────────

    /// <summary>ผสมสองสีเชิงเส้นในปริภูมิ sRGB (mixHex)</summary>
    public static string MixHex(string a, string b, double t)
    {
        var pa = HexToRgb(a);
        var pb = HexToRgb(b);
        int C(int x, int y) => (int)Math.Round(x + (y - x) * t, MidpointRounding.AwayFromZero);
        return $"#{C(pa.R, pb.R):x2}{C(pa.G, pb.G):x2}{C(pa.B, pb.B):x2}";
    }

    /// <summary>ไล่เฉด c1 → c3 สิบขั้น (gradientStops) — ตรวจที่จุดแย่ที่สุดตลอดแนว</summary>
    public static List<string> GradientStops(NisCoverPalette p)
    {
        var steps = new List<string>();
        for (var i = 0; i <= 10; i++) steps.Add(MixHex(p.C1, p.C3, i / 10d));
        return steps;
    }

    /// <summary>สีตัวอักษรที่ชื่อเฉดหนึ่งหมายถึง (inkColor) — จุดเดียวในระบบ</summary>
    public static string InkColor(NisCoverPalette p, string ink) => ink switch
    {
        "onDark" => p.OnDark,
        "ink" => "#0b0b0b",
        "ink2" => "#52514e",
        "c3" => p.C3,
        "ca2" => p.Ca2,
        "cad" => p.Cad,
        _ => p.C3,
    };

    private static List<string> Surfaces(NisCoverPalette p, string s)
    {
        switch (s)
        {
            case "gradient": return GradientStops(p);
            case "solid": return new List<string> { p.C2 };
            case "c3": return new List<string> { p.C3 };
            case "soft": return new List<string> { CoverSoftBg };
            case "tint":
                var fill = MixHex("#ffffff", p.Ca, CoverTintAlpha);
                return new List<string> { "#ffffff", fill, MixHex("#ffffff", p.Ca, CoverRingAlpha), MixHex(fill, p.Ca, CoverRingAlpha) };
            default: return new List<string> { "#ffffff" };
        }
    }

    /// <summary>WCAG: ตัวใหญ่ = ≥24px หรือ ≥18.66px ที่ weight ≥700 (isLargeText)</summary>
    public static bool IsLargeText(double sizePx, int weight) => sizePx >= 24 || (sizePx >= 18.66 && weight >= 700);

    /// <summary>เกณฑ์ที่ตัวอักษรขนาด/น้ำหนักนี้ต้องผ่าน (requiredRatio) — 4.5 ถ้าไม่ใช่ตัวใหญ่</summary>
    public static double RequiredRatio(double sizePx, int weight) => IsLargeText(sizePx, weight) ? 3 : 4.5;

    private static (double Ratio, string Bg) WorstAgainst(string fg, IReadOnlyList<string> backgrounds)
    {
        var worst = (Ratio: double.PositiveInfinity, Bg: backgrounds.Count > 0 ? backgrounds[0] : "#ffffff");
        foreach (var bg in backgrounds)
        {
            var r = ContrastRatio(fg, bg);
            if (r < worst.Ratio) worst = (r, bg);
        }
        return worst;
    }

    /// <summary>ค่าที่ผู้ใช้ตั้งเองรายบรรทัด (CoverLineOverride)</summary>
    public sealed record LineOverride(string Id, string? Color, int? Size, string? Weight, bool? Show);

    /// <summary>ตรวจหน้าปกหนึ่งใบ รวมสีและขนาดที่ผู้ใช้ตั้งเองรายบรรทัด (auditCover)</summary>
    public static List<NisContrastFindingDto> AuditCover(NisCoverPalette p, string style, IReadOnlyList<LineOverride> lines)
    {
        var specs = TextSpecs(style);
        var out_ = new List<NisContrastFindingDto>();
        foreach (var id in TextIds)
        {
            if (!specs.TryGetValue(id, out var spec)) continue;
            LineOverride? o;
            if (id == "titleAccent")
            {
                var t = lines.FirstOrDefault(l => l.Id == "title");
                o = t == null ? null : t with { Color = null };
            }
            else o = lines.FirstOrDefault(l => l.Id == id);
            if (o?.Show == false) continue;
            var fg = o?.Color ?? InkColor(p, spec.Ink);
            var sizePx = o?.Size ?? spec.SizePx;
            var weight = o?.Weight != null && WeightNum.TryGetValue(o.Weight, out var w) ? w : spec.Weight;
            var (ratio, bg) = WorstAgainst(fg, Surfaces(p, spec.Surface));
            var required = RequiredRatio(sizePx, weight);
            out_.Add(new NisContrastFindingDto
            {
                Id = id, Label = LabelTh.TryGetValue(id, out var lb) ? lb : id, Style = style,
                Required = required, Actual = ratio, Fg = fg, Bg = bg,
                UserSet = o?.Color != null || o?.Size != null || o?.Weight != null,
                Passes = ratio + 1e-9 >= required,
            });
        }
        return out_;
    }

    /// <summary>คู่ที่ตกของชุดสีหนึ่ง (coverContrastFailures)</summary>
    public static List<NisContrastFindingDto> Failures(string theme, string? mainColor, string? accentColor, string style, IReadOnlyList<LineOverride> lines) =>
        AuditCover(PaletteOf(theme, mainColor, accentColor), style, lines).Where(f => !f.Passes).ToList();

    private static string ShiftLightness(string hex, double delta)
    {
        if (delta == 0) return hex;
        var (ri, gi, bi) = HexToRgb(hex);
        double r = ri / 255d, g = gi / 255d, b = bi / 255d;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var l = (max + min) / 2;
        var target = Math.Min(1, Math.Max(0, l + delta));
        return target > l
            ? MixHex(hex, "#ffffff", (target - l) / ((1 - l) == 0 ? 1 : (1 - l)))
            : MixHex(hex, "#000000", (l - target) / (l == 0 ? 1 : l));
    }

    /// <summary>หาสีที่ใกล้ที่สุดที่ทำให้ทุกคู่ผ่าน โดยขยับทีละน้อย (suggestPassingColors) — คืนให้ผู้ใช้กดรับเอง</summary>
    public static NisColorSuggestionDto? SuggestPassingColors(string main, string accent)
    {
        var none = Array.Empty<LineOverride>();
        for (var total = 0; total <= 60; total++)
        {
            for (var dm = 0; dm <= total; dm++)
            {
                var da = total - dm;
                foreach (var sa in da == 0 ? new[] { 0 } : new[] { -1, 1 })
                {
                    var m = dm == 0 ? main : ShiftLightness(main, -dm * 0.01);
                    var a = da == 0 ? accent : ShiftLightness(accent, sa * da * 0.01);
                    if (AuditCover(CoverPalette(m, a), "network", none).All(f => f.Passes))
                        return new NisColorSuggestionDto { Main = m, Accent = a, MainShiftPct = dm, AccentShiftPct = da * (sa == 0 ? 1 : sa) };
                }
            }
        }
        return null;
    }

    /// <summary>
    /// ด่าน export (coverContrastGate) — ตรวจหน้าปกทุกใบในเอกสาร ปฏิเสธเมื่อมีคู่ไหนต่ำกว่าเกณฑ์
    /// เสนอสีได้เฉพาะเมื่อคู่ที่ตกมาจากสีธีม (ไม่ใช่สีที่ตั้งเองรายบรรทัด)
    /// </summary>
    public static NisCoverContrastGateDto Gate(JObject layout)
    {
        var failures = new List<NisContrastFindingDto>();
        NisColorSuggestionDto? suggestion = null;

        foreach (var b in NisReportSchema.Blocks(layout))
        {
            if ((string?)b["type"] != "cover") continue;
            var p = b["props"] as JObject ?? new JObject();
            var theme = (string?)p["theme"] ?? "nis";
            var mainColor = (string?)p["mainColor"];
            var accentColor = (string?)p["accentColor"];
            var style = (string?)p["style"] ?? "network";
            var lines = (p["lines"] as JArray)?.OfType<JObject>().Select(l => new LineOverride(
                (string?)l["id"] ?? "",
                (string?)l["color"],
                l["size"] is { Type: JTokenType.Integer } sz ? (int)sz : null,
                (string?)l["weight"],
                l["show"] is { Type: JTokenType.Boolean } sh ? (bool)sh : null)).ToList() ?? new List<LineOverride>();
            var f = Failures(theme, mainColor, accentColor, style, lines);
            if (f.Count == 0) continue;
            failures.AddRange(f);
            if (suggestion == null && f.Any(x => !x.UserSet))
            {
                var baseColors = theme == "custom"
                    ? (Main: mainColor ?? ThemeColors["nis"].Main, Accent: accentColor ?? ThemeColors["nis"].Accent)
                    : (ThemeColors.TryGetValue(theme, out var tc) ? tc : ThemeColors["nis"]);
                suggestion = SuggestPassingColors(baseColors.Main, baseColors.Accent);
            }
        }
        var allUserSet = failures.Count > 0 && failures.All(f => f.UserSet);
        if (failures.Count == 0)
            return new NisCoverContrastGateDto { Blocked = false, Failures = failures, Suggestion = null, Message = null };

        var lines2 = string.Join("\n", failures.Select(f =>
            $"  · {f.Label}{(f.UserSet ? " (สีที่ตั้งเอง)" : "")} — {f.Actual.ToString("0.00", CultureInfo.InvariantCulture)}:1 " +
            $"ต้องการ {f.Required.ToString(CultureInfo.InvariantCulture)}:1 (ตัวอักษร {f.Fg} บนพื้น {f.Bg})"));
        return new NisCoverContrastGateDto
        {
            Blocked = true, Failures = failures, Suggestion = suggestion,
            Message =
                $"ออกรายงานไม่ได้ — ข้อความบนหน้าปก {failures.Count} จุดมีความต่างของสีต่ำกว่าเกณฑ์\n" +
                $"{lines2}\n" +
                "เอกสารนี้จะถูกพิมพ์และส่งต่อ · ข้อความที่จมบนพื้นจะอ่านไม่ออกทั้งบรรทัด" +
                (suggestion != null
                    ? $"\nมีเฉดใกล้เคียงที่ผ่านเกณฑ์ให้กดรับได้ — สีหลัก {suggestion.Main} · สีเน้น {suggestion.Accent}"
                    : allUserSet
                        ? "\nจุดที่ตกมาจากสีที่ตั้งเองรายบรรทัด — เปลี่ยนสีหลัก/สีเน้นไม่ช่วย " +
                          "ต้องแก้สีของบรรทัดนั้น หรือล้างช่องสีเพื่อกลับไปใช้สีอัตโนมัติ"
                        : "\nยังหาเฉดใกล้เคียงที่ผ่านไม่ได้ — ต้องเลือกสีใหม่"),
        };
    }
}
