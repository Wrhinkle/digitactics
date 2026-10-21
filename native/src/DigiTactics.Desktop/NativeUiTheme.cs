using System.Security.Cryptography;
using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>The original client's typefaces and navy/gold tokens, shared by native screens.</summary>
internal static class NativeUiTheme
{
    internal const float BodySize = 17;
    internal const float SmallBodySize = 15;
    internal const float HeadingSize = 22;
    internal static ImFontPtr BodyFont { get; private set; }
    internal static ImFontPtr BodyBoldFont { get; private set; }
    internal static ImFontPtr BodySemiBoldFont { get; private set; }
    internal static ImFontPtr BodyExtraBoldFont { get; private set; }
    internal static ImFontPtr DisplayFont { get; private set; }
    internal static ImFontPtr DisplayBoldFont { get; private set; }

    internal static readonly ImColor8 Navy0 = new(1, 10, 19);
    internal static readonly ImColor8 Navy1 = new(10, 20, 40);
    internal static readonly ImColor8 Navy2 = new(11, 26, 44);
    internal static readonly ImColor8 Gold = new(200, 170, 110);
    internal static readonly ImColor8 GoldBright = new(240, 230, 210);
    internal static readonly ImColor8 GoldBorder = new(120, 90, 40);
    internal static readonly ImColor8 Muted = new(167, 178, 199);
    internal static readonly ImColor8 Play = new(255, 138, 61);
    internal static readonly ImColor8 PlayMiddle = new(255, 194, 74);
    internal static readonly ImColor8 PlayTop = new(255, 210, 106);
    internal static readonly ImColor8 PlayText = new(20, 13, 5);
    internal static readonly ImColor8 PlayBorder = new(255, 224, 154);
    // Remaining style.css :root tokens used by the client shell (Gold = --lc-gold-2, GoldBright = --lc-gold-1,
    // GoldBorder = --lc-gold-4, Muted = --c-text-2).
    internal static readonly ImColor8 GoldDeep = new(200, 155, 60);     // --lc-gold-3
    internal static readonly ImColor8 GoldDim = new(70, 55, 20);        // --lc-gold-5
    internal static readonly ImColor8 LcText = new(205, 190, 145);      // --lc-text
    internal static readonly ImColor8 LcMute = new(160, 155, 140);      // --lc-mute
    internal static readonly ImColor8 Text1 = new(238, 242, 250);       // --c-text-1
    internal static readonly ImColor8 Text3 = new(116, 129, 154);       // --c-text-3
    internal static readonly ImColor8 MutedNote = new(143, 154, 176);   // --muted (.muted)
    internal static readonly ImColor8 Accent = new(63, 214, 255);       // --c-accent
    internal static readonly ImColor8 AccentInk = new(205, 250, 250);   // .lc-cta / PLAY label
    internal static readonly ImColor8 Success = new(79, 224, 143);      // --c-success
    internal static readonly ImColor8 Danger = new(255, 84, 104);       // --c-danger
    internal static readonly ImColor8 Bits = new(79, 200, 255);         // --c-bits

    /// <summary>
    /// ImGui sizes a face by its ascent-to-descent height while CSS font-size is the em size. Multiply CSS pixels by the
    /// face's (ascent - descent) / unitsPerEm: 1.3 for Chakra Petch, 1.21 for Inter. Use for 1:1 sizes with the original.
    /// </summary>
    internal static float Css(ImFontPtr font, float pixels)
        => pixels * (font.FontId == DisplayFont.FontId || font.FontId == DisplayBoldFont.FontId ? 1.3f : 1.21f);

    private static bool IsDisplay(ImFontPtr font) => font.FontId == DisplayFont.FontId || font.FontId == DisplayBoldFont.FontId;
    // hhea ascent/descent per em: Chakra Petch 992/308 of 1000, Inter 1984/494 of 2048.
    internal static float Ascent(ImFontPtr font) => IsDisplay(font) ? .992f : 1984f / 2048;
    internal static float Descent(ImFontPtr font) => IsDisplay(font) ? .308f : 494f / 2048;

    /// <summary>
    /// CSS line-height: normal as Chromium computes it on Windows: the ascent and descent are each rounded up to whole
    /// pixels (14px Chakra Petch is 19px tall, 12.5px Inter 17px). Text drawn with NativeUiPaint.Tracked at a line-box
    /// top lands on the same baseline.
    /// </summary>
    internal static float LineHeight(ImFontPtr font, float pixels)
        => MathF.Ceiling(Ascent(font) * pixels - .01f) + MathF.Ceiling(Descent(font) * pixels - .01f);

    private static readonly Dictionary<uint, (float UnitsPerEm, Dictionary<int, short> Pairs)> kerning = [];

    /// <summary>GPOS pair kerning in em for two characters of <paramref name="font"/> (content/fonts/kerning.json).</summary>
    internal static float Kerning(ImFontPtr font, char first, char second)
        => kerning.TryGetValue(font.FontId, out var face) && face.Pairs.TryGetValue(first << 16 | second, out short value) ? value / face.UnitsPerEm : 0;

    internal static bool HasKerning(ImFontPtr font) => kerning.ContainsKey(font.FontId);

    private static void LoadKerning(params (ImFontPtr Font, string Path)[] faces)
    {
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content", "fonts", "kerning.json")));
        var table = document.RootElement.GetProperty("faces"u8);
        kerning.Clear();
        foreach (var (font, path) in faces)
        {
            var face = table.GetProperty(path);
            var pairs = new Dictionary<int, short>();
            foreach (var pair in face.GetProperty("pairs"u8).EnumerateObject()) pairs[pair.Name[0] << 16 | pair.Name[1]] = pair.Value.GetInt16();
            kerning[font.FontId] = (face.GetProperty("unitsPerEm"u8).GetInt32(), pairs);
        }
    }

    // Call once after creating each ImGui context, before constructing the renderer.
    // Explicitly select the body face so an auxiliary cinematic font can never become the UI default.
    internal static void Initialize()
    {
        BodyFont = Load("inter/Inter[opsz,wght].ttf", BodySize,
            "29160a80ff49ddcab2c97711247e08b1fab27a484a329ce8b813d820dc559031");
        BodyBoldFont = Load("inter/Inter-Bold.ttf", BodySize,
            "202f240469f916722233188ec316107f6cecaf13b801e9b3939b82b9f7a816d6");
        BodySemiBoldFont = Load("inter/Inter-SemiBold.ttf", BodySize,
            "b5996a92e34bb74df9080bf429c33cbed0d3b911926659fcf8b8871ac15ca8a4");
        BodyExtraBoldFont = Load("inter/Inter-ExtraBold.ttf", BodySize,
            "6885a1141a15827fc0c0ed03b6da7393c77f062abed4a1ed722747899e961656");
        DisplayFont = Load("chakrapetch/ChakraPetch-SemiBold.ttf", HeadingSize,
            "45264de3204ddbd5fb3e14a2402acd5c630d16650ae5fc221d2c52da46a6734b");
        DisplayBoldFont = Load("chakrapetch/ChakraPetch-Bold.ttf", HeadingSize,
            "65fbf76d95651697275e19db4d717c0e95a789ddd3476478b05292104db278a0");
        LoadKerning((BodyFont, "inter/Inter[opsz,wght].ttf"), (BodySemiBoldFont, "inter/Inter-SemiBold.ttf"), (BodyBoldFont, "inter/Inter-Bold.ttf"),
            (BodyExtraBoldFont, "inter/Inter-ExtraBold.ttf"), (DisplayFont, "chakrapetch/ChakraPetch-SemiBold.ttf"), (DisplayBoldFont, "chakrapetch/ChakraPetch-Bold.ttf"));
        ImGui.GetIO().FontDefault = BodyFont;
        ImGui.StyleColorsDark();
        var style = ImGui.GetStyle();
        style.FontSizeBase = BodySize;
        style.FontScaleMain = 1;
        style.FontScaleDpi = 1;
        // v1.0.14 theme.js uses square frames throughout the client and match
        // overlays. Keep this at the shared layer so secondary dialogs match too.
        style.WindowRounding = 0;
        style.ChildRounding = 0;
        style.FrameRounding = 0;
        style.PopupRounding = 0;
        style.TabRounding = 0;
        style.WindowBorderSize = 1;
        style.ChildBorderSize = 1;
        style.FrameBorderSize = 1;
        style.PopupBorderSize = 1;
        style.WindowPadding = new(16, 14);
        style.FramePadding = new(10, 6);
        style.ItemSpacing = new(8, 8);
        style.ItemInnerSpacing = new(6, 4);
        style.CellPadding = new(8, 6);
        style.ScrollbarSize = 8;
        style.GrabMinSize = 12;
        style.DisabledAlpha = .55f;

        Set(ImGuiCol.Text, 240, 230, 210);
        Set(ImGuiCol.TextDisabled, 167, 178, 199);
        Set(ImGuiCol.WindowBg, 1, 10, 19, 250);
        Set(ImGuiCol.ChildBg, 10, 20, 40, 110);
        Set(ImGuiCol.PopupBg, 10, 20, 40, 255);
        Set(ImGuiCol.Border, 120, 90, 40);
        Set(ImGuiCol.BorderShadow, 0, 0, 0, 0);
        Set(ImGuiCol.FrameBg, 10, 20, 40);
        Set(ImGuiCol.FrameBgHovered, 28, 39, 62);
        Set(ImGuiCol.FrameBgActive, 36, 50, 77);
        Set(ImGuiCol.TitleBg, 10, 20, 40);
        Set(ImGuiCol.TitleBgActive, 11, 26, 44);
        Set(ImGuiCol.TitleBgCollapsed, 1, 10, 19);
        Set(ImGuiCol.MenuBarBg, 10, 20, 40);
        Set(ImGuiCol.ScrollbarBg, 1, 10, 19);
        Set(ImGuiCol.ScrollbarGrab, 91, 79, 57);
        Set(ImGuiCol.ScrollbarGrabHovered, 151, 123, 71);
        Set(ImGuiCol.ScrollbarGrabActive, 200, 170, 110);
        Set(ImGuiCol.CheckMark, 200, 170, 110);
        Set(ImGuiCol.SliderGrab, 200, 170, 110);
        Set(ImGuiCol.SliderGrabActive, 240, 230, 210);
        Set(ImGuiCol.Button, 25, 31, 40);
        Set(ImGuiCol.ButtonHovered, 38, 45, 54);
        Set(ImGuiCol.ButtonActive, 48, 53, 61);
        Set(ImGuiCol.Header, 25, 38, 55);
        Set(ImGuiCol.HeaderHovered, 34, 48, 69);
        Set(ImGuiCol.HeaderActive, 48, 62, 82);
        Set(ImGuiCol.Separator, 70, 55, 20);
        Set(ImGuiCol.SeparatorHovered, 120, 90, 40);
        Set(ImGuiCol.SeparatorActive, 200, 170, 110);
        Set(ImGuiCol.ResizeGrip, 120, 90, 40, 150);
        Set(ImGuiCol.ResizeGripHovered, 200, 170, 110);
        Set(ImGuiCol.ResizeGripActive, 240, 230, 210);
        Set(ImGuiCol.InputTextCursor, 240, 230, 210);
        Set(ImGuiCol.Tab, 10, 20, 40);
        Set(ImGuiCol.TabHovered, 38, 45, 54);
        Set(ImGuiCol.TabSelected, 30, 35, 40);
        Set(ImGuiCol.TabSelectedOverline, 200, 170, 110);
        Set(ImGuiCol.TabDimmed, 1, 10, 19);
        Set(ImGuiCol.TabDimmedSelected, 11, 26, 44);
        Set(ImGuiCol.TabDimmedSelectedOverline, 120, 90, 40);
        Set(ImGuiCol.TableHeaderBg, 11, 26, 44);
        Set(ImGuiCol.TableBorderStrong, 120, 90, 40);
        Set(ImGuiCol.TableBorderLight, 70, 55, 20);
        Set(ImGuiCol.TableRowBgAlt, 36, 50, 77, 55);
        Set(ImGuiCol.TextLink, 63, 214, 255);
        Set(ImGuiCol.TextSelectedBg, 63, 214, 255, 70);
        Set(ImGuiCol.TreeLines, 120, 90, 40);
        Set(ImGuiCol.DragDropTarget, 255, 194, 74);
        Set(ImGuiCol.NavCursor, 200, 170, 110);
        Set(ImGuiCol.NavWindowingHighlight, 240, 230, 210, 180);
        Set(ImGuiCol.NavWindowingDimBg, 1, 10, 19, 155);
        Set(ImGuiCol.ModalWindowDimBg, 1, 10, 19, 190);
    }

    // Chromium's Windows text (DirectWrite grayscale with contrast enhancement) renders the same faces visibly heavier
    // than stb_truetype's linear coverage. Boost glyph coverage so strokes have the original's weight and contrast.
    private const float RasterizerMultiply = 1.3f;

    private static unsafe ImFontPtr Load(string relative, float size, string sha256)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "content", "fonts", relative);
        if (!File.Exists(path)) throw new FileNotFoundException("The bundled UI font is missing. Repair the application files.", path);
        using var stream = File.OpenRead(path);
        if (!Convert.ToHexStringLower(SHA256.HashData(stream)).Equals(sha256, StringComparison.Ordinal))
            throw new InvalidDataException($"The bundled UI font has changed: {relative}");
        // ImFontConfig's constructor defaults, plus the coverage boost; AddFontFromFileTTF copies the template.
        var config = new ImFontConfig { FontDataOwnedByAtlas = true, GlyphMaxAdvanceX = float.MaxValue, RasterizerMultiply = RasterizerMultiply, RasterizerDensity = 1 };
        var font = ImGui.GetIO().Fonts.AddFontFromFileTTF(path, size, new ImFontConfigPtr(&config), default);
        if (font.IsNull()) throw new InvalidOperationException($"Could not load the bundled UI font: {relative}");
        return font;
    }

    private static void Set(ImGuiCol color, byte red, byte green, byte blue, byte alpha = 255)
        => ImGui.GetStyle().Colors[(int)color] = new float4(red / 255f, green / 255f, blue / 255f, alpha / 255f);
}
