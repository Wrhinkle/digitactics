using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>
/// CSS-metric drawing for screens restored from the Electron client: font sizes and line boxes are
/// given in CSS pixels, letter spacing in pixels, colors as the original tokens.
/// ImGui sizes a face by ascender+descender, CSS by the em square, so each face converts by its
/// own (ascender - descender) / unitsPerEm: Inter 2478/2048, Chakra Petch 1300/1000.
/// </summary>
internal static class ClientCss
{
    internal enum Face { Body, BodyBold, Display, DisplayBold }

    internal static readonly ImColor8 Gold1=new(240,230,210),Gold2=new(200,170,110),Gold3=new(200,155,60),Gold4=new(120,90,40),Gold5=new(70,55,20);
    internal static readonly ImColor8 Text1=new(238,242,250),Text2=new(167,178,199),Text3=new(116,129,154),LcText=new(205,190,145),LcMute=new(160,155,140),Muted=new(143,154,176);
    internal static readonly ImColor8 Accent=new(63,214,255),Navy0=new(1,10,19);

    internal static ImFontPtr Font(Face face) => face switch
    {
        Face.Body => NativeUiTheme.BodyFont,
        Face.BodyBold => NativeUiTheme.BodyBoldFont,
        Face.Display => NativeUiTheme.DisplayFont,
        _ => NativeUiTheme.DisplayBoldFont,
    };

    internal static float Size(Face face, float px) => px * (face >= Face.Display ? 1.3f : 2478f / 2048f);

    internal static ImColor8 Alpha(ImColor8 color, float alpha)
    {
        var rgba = color.AsByte4();
        return new(rgba.X, rgba.Y, rgba.Z, (byte)Math.Clamp(Math.Round(rgba.W * alpha), 0, 255));
    }

    internal static float Width(Face face, float px, string text, float tracking = 0)
    {
        if (text.Length == 0) return 0;
        ImGui.PushFont(Font(face), Size(face, px));
        float width = ImGui.CalcTextSize(text).X;
        ImGui.PopFont();
        return width + tracking * text.Length;
    }

    /// <summary>Draws one line whose CSS box starts at <paramref name="at"/>; lineHeight 0 means "normal".</summary>
    internal static float Text(ImDrawListPtr draw, Face face, float px, float2 at, ImColor8 color, string text, float tracking = 0, float lineHeight = 0)
    {
        float size = Size(face, px);
        if (lineHeight > 0) at.Y += (lineHeight - size) * .5f;
        var font = Font(face);
        if (tracking == 0)
        {
            draw.AddText(font, size, at, color, text);
            return Width(face, px, text);
        }
        ImGui.PushFont(font, size);
        float x = at.X;
        for (int i = 0; i < text.Length; i++)
        {
            int length = char.IsHighSurrogate(text[i]) && i + 1 < text.Length ? 2 : 1;
            string glyph = text.Substring(i, length);
            draw.AddText(font, size, new(x, at.Y), color, glyph);
            x += ImGui.CalcTextSize(glyph).X + tracking;
            i += length - 1;
        }
        ImGui.PopFont();
        return x - at.X;
    }

    /// <summary>Text centered horizontally in a box of the given width.</summary>
    internal static void TextCentered(ImDrawListPtr draw, Face face, float px, float2 at, float width, ImColor8 color, string text, float tracking = 0, float lineHeight = 0)
    {
        // CSS letter spacing trails the last glyph as well, which shifts centered text left by half of it.
        float measured = Width(face, px, text, tracking);
        Text(draw, face, px, at + new float2((width - measured) * .5f, 0), color, text, tracking, lineHeight);
    }

    /// <summary>Greedy word wrap, the way a browser breaks normal text.</summary>
    internal static List<string> Wrap(Face face, float px, string text, float width, float tracking = 0)
    {
        var lines = new List<string>();
        foreach (string paragraph in text.Split('\n'))
        {
            string line = "";
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                // Browsers may also break after a hyphen that is followed by a letter ("Champion-|form").
                var pieces = new List<string>(); int start = 0;
                for (int i = 0; i < word.Length - 1; i++)
                    if (word[i] == '-' && char.IsLetter(word[i + 1]) && i > 0) { pieces.Add(word[start..(i + 1)]); start = i + 1; }
                pieces.Add(word[start..]);
                for (int p = 0; p < pieces.Count; p++)
                {
                    string separator = line.Length == 0 || p > 0 ? "" : " ";
                    string next = line + separator + pieces[p];
                    if (line.Length > 0 && Width(face, px, next, tracking) - tracking > width) { lines.Add(line); line = pieces[p]; }
                    else line = next;
                }
            }
            lines.Add(line);
        }
        return lines;
    }

    /// <summary>Wrapped paragraph; returns its height (lines × line height).</summary>
    internal static float Paragraph(ImDrawListPtr draw, Face face, float px, float2 at, float width, float lineHeight, ImColor8 color, string text, float tracking = 0, int maxLines = int.MaxValue)
    {
        var lines = Wrap(face, px, text, width, tracking);
        int count = Math.Min(lines.Count, maxLines);
        for (int i = 0; i < count; i++) Text(draw, face, px, at + new float2(0, i * lineHeight), color, lines[i], tracking, lineHeight);
        return count * lineHeight;
    }

    internal static float ParagraphHeight(Face face, float px, float width, float lineHeight, string text, float tracking = 0)
        => Wrap(face, px, text, width, tracking).Count * lineHeight;

    internal static string Ellipsis(Face face, float px, string text, float width, float tracking = 0)
    {
        if (Width(face, px, text, tracking) <= width) return text;
        int length = text.Length;
        while (length > 0 && Width(face, px, text[..length] + "…", tracking) > width) length--;
        return text[..length] + "…";
    }

    /// <summary>Vertical two-stop gradient (CSS linear-gradient(180deg, top, bottom)).</summary>
    internal static void Gradient(ImDrawListPtr draw, float2 min, float2 max, ImColor8 top, ImColor8 bottom)
        => draw.AddRectFilledMultiColor(min, max, top, top, bottom, bottom);

    /// <summary>Horizontal two-stop gradient (CSS linear-gradient(90deg, left, right)).</summary>
    internal static void GradientX(ImDrawListPtr draw, float2 min, float2 max, ImColor8 left, ImColor8 right)
        => draw.AddRectFilledMultiColor(min, max, left, right, right, left);

    /// <summary>Approximates a blurred box-shadow (0 0 blur color) outside the box.</summary>
    internal static void Glow(ImDrawListPtr draw, float2 min, float2 max, ImColor8 color, float blur, float rounding = 0)
    {
        int steps = Math.Max(3, (int)Math.Ceiling(blur / 2));
        var rgba = color.AsByte4();
        for (int i = steps; i >= 1; i--)
        {
            float t = i / (float)steps, grow = blur * .5f * t;
            float alpha = rgba.W / 255f * (1 - t) * (1 - t) * 2f / steps;
            draw.AddRect(min - new float2(grow), max + new float2(grow), new ImColor8(rgba.X, rgba.Y, rgba.Z, (byte)Math.Clamp(alpha * 255 * 3, 0, 255)), rounding + grow, ImDrawFlags.None, blur / steps + 1);
        }
    }

    /// <summary>Approximates an inset glow (inset 0 0 blur color).</summary>
    internal static void InsetGlow(ImDrawListPtr draw, float2 min, float2 max, ImColor8 color, float blur)
    {
        int steps = Math.Max(3, (int)Math.Ceiling(blur / 3));
        var rgba = color.AsByte4();
        for (int i = 0; i < steps; i++)
        {
            float t = i / (float)steps, inset = blur * .5f * t;
            byte alpha = (byte)Math.Clamp(rgba.W * (1 - t) * (1 - t) * 1.6f / steps * 2, 0, 255);
            draw.AddRect(min + new float2(inset), max - new float2(inset), new ImColor8(rgba.X, rgba.Y, rgba.Z, alpha), 0, ImDrawFlags.None, blur * .5f / steps + 1);
        }
    }

    /// <summary>Dashed 1px border, like CSS border-style: dashed.</summary>
    internal static void Dashed(ImDrawListPtr draw, float2 at, float2 extent, ImColor8 color, float dash = 3, float gap = 3)
    {
        for (float x = 0; x < extent.X; x += dash + gap)
        {
            draw.AddLine(at + new float2(x, .5f), at + new float2(Math.Min(x + dash, extent.X), .5f), color);
            draw.AddLine(at + new float2(x, extent.Y - .5f), at + new float2(Math.Min(x + dash, extent.X), extent.Y - .5f), color);
        }
        for (float y = 0; y < extent.Y; y += dash + gap)
        {
            draw.AddLine(at + new float2(.5f, y), at + new float2(.5f, Math.Min(y + dash, extent.Y)), color);
            draw.AddLine(at + new float2(extent.X - .5f, y), at + new float2(extent.X - .5f, Math.Min(y + dash, extent.Y)), color);
        }
    }

    /// <summary>1px border drawn inside the box edge, as a CSS border-box border.</summary>
    internal static void Border(ImDrawListPtr draw, float2 min, float2 max, ImColor8 color, float width = 1, float rounding = 0)
        => draw.AddRect(min + new float2(width * .5f), max - new float2(width * .5f), color, rounding, ImDrawFlags.None, width);
}
