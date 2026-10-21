using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>
/// Draw-list equivalents of the original client's CSS effects: letter-spacing, text-shadow glows,
/// box-shadow glows, radial gradients and the Bits diamond. All colors respect the current style alpha.
/// </summary>
internal static class NativeUiPaint
{
    internal static ImColor8 Fade(ImColor8 color, float opacity = 1)
    {
        var rgba = color.AsByte4();
        return new(rgba.X, rgba.Y, rgba.Z, (byte)Math.Clamp(Math.Round(rgba.W * opacity * ImGui.GetStyle().Alpha), 0, 255));
    }

    internal static ImColor8 Alpha(ImColor8 color, byte alpha)
    {
        var rgba = color.AsByte4();
        return new(rgba.X, rgba.Y, rgba.Z, alpha);
    }

    // Text sizes in this class are CSS pixels (em size); NativeUiTheme.Css converts them to ImGui's ascent-to-descent size.
    /// <summary>Width of <paramref name="text"/> with CSS letter-spacing in em (trailing spacing included, as in Chromium).</summary>
    internal static float TrackedWidth(ImFontPtr font, float size, string text, float tracking)
    {
        if (text.Length == 0) return 0;
        float scaled = NativeUiTheme.Css(font, size);
        // CalcTextSizeA keeps fractional advances; ImGui.CalcTextSize rounds each call up to whole pixels.
        float width = font.CalcTextSizeA(scaled, float.MaxValue, 0, text, default).X;
        if (tracking != 0) width += text.EnumerateRunes().Count() * tracking * size;
        return width;
    }

    internal static float2 TrackedSize(ImFontPtr font, float size, string text, float tracking)
        => new(TrackedWidth(font, size, text, tracking), NativeUiTheme.Css(font, size));

    internal static void Tracked(ImDrawListPtr draw, ImFontPtr font, float size, float2 at, ImColor8 color, string text, float tracking = 0)
    {
        color = Fade(color);
        float scaled = NativeUiTheme.Css(font, size);
        if (tracking == 0) { draw.AddText(font, scaled, at, color, text); return; }
        float x = at.X;
        foreach (var rune in text.EnumerateRunes())
        {
            string glyph = rune.ToString();
            draw.AddText(font, scaled, new(MathF.Round(x), at.Y), color, glyph);
            x += font.CalcTextSizeA(scaled, float.MaxValue, 0, glyph, default).X + tracking * size;
        }
    }

    /// <summary>CSS text-shadow: 0 0 radius color, approximated with faint offset copies under the text.</summary>
    internal static void TextGlow(ImDrawListPtr draw, ImFontPtr font, float size, float2 at, string text, float tracking, ImColor8 glow, float radius)
    {
        var rgba = glow.AsByte4();
        var box = TrackedSize(font, size, text, tracking);
        // A soft halo over the text box, then a tight blurred copy of the glyphs.
        Radial(draw, at + box * 0.5f, new float2(box.X * 0.5f + radius, box.Y * 0.5f + radius * 0.6f), new(rgba.X, rgba.Y, rgba.Z, (byte)(rgba.W * 0.35f)), new(rgba.X, rgba.Y, rgba.Z, 0), 32);
        for (int ring = 1; ring <= 2; ring++)
        {
            float distance = radius * 0.12f * ring;
            byte alpha = (byte)Math.Clamp(rgba.W * 0.16f, 0, 255);
            for (int step = 0; step < 8; step++)
            {
                float angle = step * MathF.PI / 4 + ring * 0.4f;
                Tracked(draw, font, size, at + new float2(MathF.Cos(angle), MathF.Sin(angle)) * distance, new(rgba.X, rgba.Y, rgba.Z, alpha), text, tracking);
            }
        }
    }

    /// <summary>CSS box-shadow with zero offset: an outer glow that fades over <paramref name="radius"/> pixels.</summary>
    internal static void BoxGlow(ImDrawListPtr draw, float2 lo, float2 hi, ImColor8 color, float radius)
    {
        var rgba = color.AsByte4();
        int steps = Math.Max(1, (int)MathF.Ceiling(radius / 2));
        for (int i = 0; i < steps; i++)
        {
            float t = (i + 0.5f) / steps, grow = t * radius;
            byte alpha = (byte)Math.Clamp(rgba.W * (1 - t) * (1 - t) * 2.2f / steps * 2, 0, 255);
            draw.AddRect(lo - new float2(grow), hi + new float2(grow), Fade(new(rgba.X, rgba.Y, rgba.Z, alpha)), 0, ImDrawFlags.None, 2);
        }
    }

    /// <summary>CSS inset box-shadow: a glow that fades inward from the edges.</summary>
    internal static void InsetGlow(ImDrawListPtr draw, float2 lo, float2 hi, ImColor8 color, float radius)
    {
        var rgba = color.AsByte4();
        int steps = Math.Max(1, (int)MathF.Ceiling(radius / 2));
        for (int i = 0; i < steps; i++)
        {
            float t = (i + 0.5f) / steps, shrink = t * radius;
            if (lo.X + shrink >= hi.X - shrink || lo.Y + shrink >= hi.Y - shrink) break;
            byte alpha = (byte)Math.Clamp(rgba.W * (1 - t) * (1 - t) * 2.2f / steps * 2, 0, 255);
            draw.AddRect(lo + new float2(shrink), hi - new float2(shrink), Fade(new(rgba.X, rgba.Y, rgba.Z, alpha)), 0, ImDrawFlags.None, 2);
        }
    }

    internal static void VerticalGradient(ImDrawListPtr draw, float2 lo, float2 hi, ImColor8 top, ImColor8 bottom)
        => draw.AddRectFilledMultiColor(lo, hi, Fade(top), Fade(top), Fade(bottom), Fade(bottom));

    internal static void HorizontalGradient(ImDrawListPtr draw, float2 lo, float2 hi, ImColor8 left, ImColor8 right)
        => draw.AddRectFilledMultiColor(lo, hi, Fade(left), Fade(right), Fade(right), Fade(left));

    /// <summary>Three-stop vertical gradient (top, middle at <paramref name="split"/>, bottom).</summary>
    internal static void VerticalGradient(ImDrawListPtr draw, float2 lo, float2 hi, ImColor8 top, ImColor8 middle, ImColor8 bottom, float split)
    {
        float y = lo.Y + (hi.Y - lo.Y) * split;
        VerticalGradient(draw, lo, new(hi.X, y), top, middle);
        VerticalGradient(draw, new(lo.X, y), hi, middle, bottom);
    }

    /// <summary>Horizontal line that fades to transparent at both ends (CSS linear-gradient(90deg, transparent, c, transparent)).</summary>
    internal static void FadedLine(ImDrawListPtr draw, float2 lo, float2 hi, ImColor8 color)
    {
        float middle = (lo.X + hi.X) / 2;
        var clear = Alpha(color, 0);
        HorizontalGradient(draw, lo, new(middle, hi.Y), clear, color);
        HorizontalGradient(draw, new(middle, lo.Y), hi, color, clear);
    }

    /// <summary>Elliptical radial gradient (CSS radial-gradient(rx ry at center, inner, outer)) clipped to the current clip rect.</summary>
    internal static void Radial(ImDrawListPtr draw, float2 center, float2 radius, ImColor8 inner, ImColor8 outer, int segments = 40)
    {
        var uv = ImGui.GetFontTexUvWhitePixel();
        var innerColor = Fade(inner);
        var outerColor = Fade(outer);
        draw.PrimReserve(segments * 3, segments * 3);
        for (int i = 0; i < segments; i++)
        {
            float a = i * MathF.Tau / segments, b = (i + 1) * MathF.Tau / segments;
            draw.PrimVtx(center, uv, innerColor);
            draw.PrimVtx(center + new float2(MathF.Cos(a) * radius.X, MathF.Sin(a) * radius.Y), uv, outerColor);
            draw.PrimVtx(center + new float2(MathF.Cos(b) * radius.X, MathF.Sin(b) * radius.Y), uv, outerColor);
        }
    }

    /// <summary>The original .bit: a rotated rounded square with a cyan gradient and soft glow.</summary>
    internal static void Bit(ImDrawListPtr draw, float2 center, float size)
    {
        float half = size * 0.72f;
        Radial(draw, center, new(half * 1.7f), new ImColor8(79, 200, 255, 110), new ImColor8(79, 200, 255, 0), 24);
        var top = center + new float2(0, -half);
        var right = center + new float2(half, 0);
        var bottom = center + new float2(0, half);
        var left = center + new float2(-half, 0);
        var uv = ImGui.GetFontTexUvWhitePixel();
        // 135deg gradient: #c8fdff at the top-left edge, --c-bits in the middle, #1b5fd0 at the bottom-right.
        var light = Fade(new ImColor8(200, 253, 255));
        var middle = Fade(new ImColor8(79, 200, 255));
        var dark = Fade(new ImColor8(27, 95, 208));
        draw.PrimReserve(6, 6);
        draw.PrimVtx(left, uv, light); draw.PrimVtx(top, uv, light); draw.PrimVtx(right, uv, middle);
        draw.PrimVtx(left, uv, middle); draw.PrimVtx(right, uv, middle); draw.PrimVtx(bottom, uv, dark);
    }

    /// <summary>A single right-pointing (or, when open, downward) caret as drawn by .cs-car.</summary>
    internal static void Caret(ImDrawListPtr draw, float2 center, bool open, ImColor8 color)
    {
        color = Fade(color);
        if (open) draw.AddTriangleFilled(center + new float2(-4, -2.5f), center + new float2(4, -2.5f), center + new float2(0, 2.5f), color);
        else draw.AddTriangleFilled(center + new float2(-2.5f, -4), center + new float2(2.5f, 0), center + new float2(-2.5f, 4), color);
    }
}
