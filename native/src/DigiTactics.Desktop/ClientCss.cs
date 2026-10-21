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
    internal enum Face { Body, BodyBold, BodySemi, Display, DisplayBold }

    internal static readonly ImColor8 Gold1=new(240,230,210),Gold2=new(200,170,110),Gold3=new(200,155,60),Gold4=new(120,90,40),Gold5=new(70,55,20);
    internal static readonly ImColor8 Text1=new(238,242,250),Text2=new(167,178,199),Text3=new(116,129,154),LcText=new(205,190,145),LcMute=new(160,155,140),Muted=new(143,154,176);
    internal static readonly ImColor8 Accent=new(63,214,255),Navy0=new(1,10,19);

    internal static ImFontPtr Font(Face face) => face switch
    {
        Face.Body => NativeUiTheme.BodyFont,
        Face.BodyBold => NativeUiTheme.BodyBoldFont,
        Face.BodySemi => NativeUiTheme.BodySemiBoldFont,
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
        // ImFont.CalcTextSizeA is unrounded (ImGui.CalcTextSize rounds up to whole pixels).
        return Font(face).CalcTextSizeA(Size(face, px), float.MaxValue, 0, text).X + tracking * text.Length;
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
        // Place every glyph by the exact width of the text before it, plus the spacing so far.
        float advance = 0;
        for (int i = 0, count = 0; i < text.Length; count++)
        {
            int length = char.IsHighSurrogate(text[i]) && i + 1 < text.Length ? 2 : 1;
            float prefix = i == 0 ? 0 : font.CalcTextSizeA(size, float.MaxValue, 0, text[..i]).X;
            draw.AddText(font, size, new(at.X + prefix + count * tracking, at.Y), color, text.Substring(i, length));
            i += length;
            if (i >= text.Length) advance = font.CalcTextSizeA(size, float.MaxValue, 0, text).X + (count + 1) * tracking;
        }
        return advance;
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
    internal static void Glow(ImDrawListPtr draw, float2 min, float2 max, ImColor8 color, float blur, float rounding = 0, bool filled = false)
    {
        // A filled shadow also darkens its own box (an offset drop shadow shows below the element).
        if (filled) { var c = color.AsByte4(); draw.AddRectFilled(min, max, new ImColor8(c.X, c.Y, c.Z, (byte)(c.W * .5f)), rounding); }
        // A CSS blur radius is twice the Gaussian sigma; outside a straight edge the shadow falls off as
        // 0.5 * erfc(d / (sigma * sqrt 2)). Paint that profile as adjacent 1px rings.
        var rgba = color.AsByte4();float sigma = Math.Max(.5f, blur / 2);int rings = (int)Math.Ceiling(blur * 1.2f);
        for (int i = 0; i < rings; i++)
        {
            float d = i + .5f, intensity = .5f * Erfc(d / (sigma * 1.41421356f));
            byte alpha = (byte)Math.Clamp(Math.Round(rgba.W * intensity), 0, 255);
            if (alpha == 0) continue;
            // Ring i covers the pixels i..i+1 outside the edge (AddRect strokes 0.5px inside its corners).
            draw.AddRect(min - new float2(i + 1), max + new float2(i + 1), new ImColor8(rgba.X, rgba.Y, rgba.Z, alpha), rounding > 0 ? rounding + d : 0, ImDrawFlags.None, 1);
        }
    }
    private static float Erfc(float x)
    {
        // Abramowitz and Stegun 7.1.26, |error| < 1.5e-7 for x >= 0.
        float t = 1 / (1 + .3275911f * x);
        return t * (.254829592f + t * (-.284496736f + t * (1.421413741f + t * (-1.453152027f + t * 1.061405429f)))) * MathF.Exp(-x * x);
    }

    /// <summary>Approximates an inset glow (inset 0 0 blur color).</summary>
    internal static void InsetGlow(ImDrawListPtr draw, float2 min, float2 max, ImColor8 color, float blur)
    {
        // The inside of an inset shadow follows the same edge profile as Glow, measured inward.
        var rgba = color.AsByte4();float sigma = Math.Max(.5f, blur / 2);int rings = (int)Math.Min(Math.Ceiling(blur * 1.2f), Math.Min(max.X - min.X, max.Y - min.Y) / 2);
        for (int i = 0; i < rings; i++)
        {
            float d = i + .5f, intensity = .5f * Erfc(d / (sigma * 1.41421356f));
            byte alpha = (byte)Math.Clamp(Math.Round(rgba.W * intensity), 0, 255);
            if (alpha == 0) continue;
            draw.AddRect(min + new float2(i), max - new float2(i), new ImColor8(rgba.X, rgba.Y, rgba.Z, alpha), 0, ImDrawFlags.None, 1);
        }
    }
    /// <summary>
    /// CSS radial-gradient(circle at center, color, transparent radius): a triangle fan whose vertex colors
    /// interpolate from the center color to transparent at the given radius.
    /// </summary>
    internal static void Radial(ImDrawListPtr draw, float2 center, float radius, ImColor8 color)
    {
        const int segments = 64;
        var rgba = color.AsByte4(); var clear = new ImColor8(rgba.X, rgba.Y, rgba.Z, 0);
        var uv = ImGui.GetFontTexUvWhitePixel();
        draw.PrimReserve(segments * 3, segments * 3);
        for (int i = 0; i < segments; i++)
        {
            float a = i * MathF.Tau / segments, b = (i + 1) * MathF.Tau / segments;
            draw.PrimVtx(center, uv, color);
            draw.PrimVtx(center + new float2(MathF.Cos(a), MathF.Sin(a)) * radius, uv, clear);
            draw.PrimVtx(center + new float2(MathF.Cos(b), MathF.Sin(b)) * radius, uv, clear);
        }
    }

    /// <summary>Radius of a CSS "circle at x% y%" gradient's farthest-corner size inside a box.</summary>
    internal static float FarthestCorner(float2 extent, float x, float y)
        => MathF.Sqrt(MathF.Pow(extent.X * Math.Max(x, 1 - x), 2) + MathF.Pow(extent.Y * Math.Max(y, 1 - y), 2));

    /// <summary>Dashed 1px border, like CSS border-style: dashed.</summary>
    internal static void Dashed(ImDrawListPtr draw, float2 at, float2 extent, ImColor8 color, float dash = 3, float gap = 3)
    {
        var end = Snap(at + extent); at = Snap(at); extent = end - at;
        for (float x = 0; x < extent.X; x += dash + gap)
        {
            draw.AddLine(at + new float2(x, 0), at + new float2(Math.Min(x + dash, extent.X), 0), color);
            draw.AddLine(at + new float2(x, extent.Y - 1), at + new float2(Math.Min(x + dash, extent.X), extent.Y - 1), color);
        }
        for (float y = 0; y < extent.Y; y += dash + gap)
        {
            draw.AddLine(at + new float2(0, y), at + new float2(0, Math.Min(y + dash, extent.Y)), color);
            draw.AddLine(at + new float2(extent.X - 1, y), at + new float2(extent.X - 1, Math.Min(y + dash, extent.Y)), color);
        }
    }

    /// <summary>1px border drawn inside the box edge, as a CSS border-box border.</summary>
    internal static void Border(ImDrawListPtr draw, float2 min, float2 max, ImColor8 color, float width = 1, float rounding = 0)
        // Browsers paint borders on whole pixels; ImDrawList.AddRect already strokes 0.5px inside its
        // corners, which is pixel-exact for 1px once the box is snapped.
        => draw.AddRect(Snap(min) + new float2((width - 1) * .5f), Snap(max) - new float2((width - 1) * .5f), color, rounding, ImDrawFlags.None, width);

    internal static float2 Snap(float2 point) => new(MathF.Round(point.X), MathF.Round(point.Y));
}
