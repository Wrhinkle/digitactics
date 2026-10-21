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

    // Call once after creating each ImGui context, before constructing the renderer.
    // Explicitly select the body face so an auxiliary cinematic font can never become the UI default.
    internal static void Initialize()
    {
        BodyFont = Load("inter/Inter[opsz,wght].ttf", BodySize,
            "29160a80ff49ddcab2c97711247e08b1fab27a484a329ce8b813d820dc559031");
        BodyBoldFont = Load("inter/Inter-Bold.ttf", BodySize,
            "202f240469f916722233188ec316107f6cecaf13b801e9b3939b82b9f7a816d6");
        DisplayFont = Load("chakrapetch/ChakraPetch-SemiBold.ttf", HeadingSize,
            "45264de3204ddbd5fb3e14a2402acd5c630d16650ae5fc221d2c52da46a6734b");
        DisplayBoldFont = Load("chakrapetch/ChakraPetch-Bold.ttf", HeadingSize,
            "65fbf76d95651697275e19db4d717c0e95a789ddd3476478b05292104db278a0");
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

    private static ImFontPtr Load(string relative, float size, string sha256)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "content", "fonts", relative);
        if (!File.Exists(path)) throw new FileNotFoundException("The bundled UI font is missing. Repair the application files.", path);
        using var stream = File.OpenRead(path);
        if (!Convert.ToHexStringLower(SHA256.HashData(stream)).Equals(sha256, StringComparison.Ordinal))
            throw new InvalidDataException($"The bundled UI font has changed: {relative}");
        var font = ImGui.GetIO().Fonts.AddFontFromFileTTF(path, size);
        if (font.IsNull()) throw new InvalidOperationException($"Could not load the bundled UI font: {relative}");
        return font;
    }

    private static void Set(ImGuiCol color, byte red, byte green, byte blue, byte alpha = 255)
        => ImGui.GetStyle().Colors[(int)color] = new float4(red / 255f, green / 255f, blue / 255f, alpha / 255f);
}
