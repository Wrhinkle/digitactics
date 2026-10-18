using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>A cached native glyph mask with the source stage's black and gold Gaussian text shadows.</summary>
internal sealed class EvolutionAnnouncementArtwork : IDisposable
{
    private const int Padding = 24;
    private const float FontSize = 15, LineHeight = FontSize * 1.35f;
    private GpuTexture? texture;
    private string? key;
    public int Lines { get; private set; }
    public int InkPixels { get; private set; }
    public int ShadowPixels { get; private set; }
    public void Draw(string text, float2 at, int width, float height, double opacity, double slide)
    {
        string nextKey = width + ":" + text;
        if (key != nextKey)
        {
            var image = Rasterize(text, width, out int lines);
            InkPixels = Enumerable.Range(0, image.Width * image.Height).Count(i => image.Rgba[i * 4 + 3] > 128 && image.Rgba[i * 4 + 1] > 250 && image.Rgba[i * 4 + 2] > 250);
            ShadowPixels = Enumerable.Range(0, image.Width * image.Height).Count(i => image.Rgba[i * 4 + 3] > 4 && image.Rgba[i * 4 + 1] > image.Rgba[i * 4 + 2] + 10);
            var next = new GpuTexture(image, new(MinFilter:9729, WrapS:33071, WrapT:33071), false);
            texture?.Dispose(); texture = next; key = nextKey; Lines = lines;
        }
        var draw = ImGui.GetWindowDrawList();
        var min = new float2(MathF.Floor(at.X - Padding), MathF.Floor(at.Y + height - 10 - Lines * LineHeight - Padding) + (float)slide);
        draw.PushClipRect(at, new(at.X + width, at.Y + height), true);
        try
        {
            draw.AddImage(new ImTextureRef { _TexID = new((nint)texture!.Id) }, min, new(min.X + texture.Width, min.Y + texture.Height), new(0, 0), new(1, 1), new ImColor8(255, 255, 255, (byte)Math.Round(opacity * 255)));
        }
        finally { draw.PopClipRect(); }
    }

    private static PngImage Rasterize(string text, int width, out int lineCount)
    {
        var lines = new List<string>();
        string line = "";
        foreach (string word in text.Split(' '))
        {
            string next = line.Length == 0 ? word : line + " " + word;
            if (line.Length > 0 && ImGui.CalcTextSize(next).X > width - 16) { lines.Add(line); line = word; }
            else line = next;
        }
        if (line.Length > 0) lines.Add(line);
        lineCount = lines.Count;
        int imageWidth = width + Padding * 2, imageHeight = (int)Math.Ceiling(lineCount * LineHeight) + Padding * 2;
        var mask = new float[imageWidth * imageHeight];
        var font = ImGui.GetFont(); var baked = font.GetFontBaked(FontSize);
        // Load all glyphs before reading UVs: adding a glyph may resize the atlas.
        foreach (var rune in text.EnumerateRunes()) _ = baked.FindGlyph((uint)rune.Value);
        var atlas = font.ContainerAtlas.TexData;
        for (int i = 0; i < lines.Count; i++)
        {
            float cursorX = MathF.Floor(Padding + (width - ImGui.CalcTextSize(lines[i]).X) / 2);
            float cursorY = MathF.Floor(Padding + i * LineHeight + (LineHeight - FontSize) / 2);
            foreach (var rune in lines[i].EnumerateRunes())
            {
                var glyph = baked.FindGlyph((uint)rune.Value);
                float left = cursorX + glyph.X0, right = cursorX + glyph.X1, top = cursorY + glyph.Y0, bottom = cursorY + glyph.Y1;
                if (right > left && bottom > top)
                    for (int y = Math.Max(0, (int)Math.Ceiling(top - .5f)); y < Math.Min(imageHeight, (int)Math.Ceiling(bottom - .5f)); y++)
                        for (int x = Math.Max(0, (int)Math.Ceiling(left - .5f)); x < Math.Min(imageWidth, (int)Math.Ceiling(right - .5f)); x++)
                        {
                            float tx = (glyph.U0 + (x + .5f - left) / (right - left) * (glyph.U1 - glyph.U0)) * atlas.Width - .5f;
                            float ty = (glyph.V0 + (y + .5f - top) / (bottom - top) * (glyph.V1 - glyph.V0)) * atlas.Height - .5f;
                            int sx = (int)MathF.Floor(tx), sy = (int)MathF.Floor(ty); float fx = tx - sx, fy = ty - sy;
                            float alpha = (Pixel(sx, sy) * (1 - fx) + Pixel(sx + 1, sy) * fx) * (1 - fy) + (Pixel(sx, sy + 1) * (1 - fx) + Pixel(sx + 1, sy + 1) * fx) * fy;
                            int index = y * imageWidth + x; mask[index] = alpha + mask[index] * (1 - alpha);
                        }
                cursorX += glyph.AdvanceX;
            }
        }
        float[] gold = Blur(mask, imageWidth, imageHeight, 8), black = Blur(mask, imageWidth, imageHeight, 3);
        var rgba = new byte[mask.Length * 4];
        for (int y = 0; y < imageHeight; y++)
            for (int x = 0; x < imageWidth; x++)
            {
                int index = y * imageWidth + x;
                float aGold = gold[index], aBlack = y >= 2 ? black[index - imageWidth * 2] : 0, aText = mask[index];
                float alpha = aText + (1 - aText) * (aBlack + aGold * (1 - aBlack));
                if (alpha <= 0) continue;
                float color = aGold * (1 - aBlack) * (1 - aText);
                rgba[index * 4] = Byte((aText + color) / alpha);
                rgba[index * 4 + 1] = Byte((aText + color * 211 / 255) / alpha);
                rgba[index * 4 + 2] = Byte((aText + color * 106 / 255) / alpha);
                rgba[index * 4 + 3] = Byte(alpha);
            }
        return new(imageWidth, imageHeight, rgba);

        float Pixel(int x, int y)
        {
            x = Math.Clamp(x, 0, atlas.Width - 1); y = Math.Clamp(y, 0, atlas.Height - 1);
            int offset = (y * atlas.Width + x) * atlas.BytesPerPixel + (atlas.BytesPerPixel == 4 ? 3 : 0);
            return atlas.Pixels[offset] / 255f;
        }
    }

    private static byte Byte(float value) => (byte)Math.Clamp((int)MathF.Round(value * 255), 0, 255);
    private static float[] Blur(float[] mask, int width, int height, float sigma)
    {
        int radius = (int)Math.Ceiling(3 * sigma);
        var weights = Enumerable.Range(-radius, 2 * radius + 1).Select(x => MathF.Exp(-x * x / (2 * sigma * sigma))).ToArray();
        float sum = weights.Sum();
        for (int i = 0; i < weights.Length; i++) weights[i] /= sum;
        var temporary = new float[mask.Length]; var blurred = new float[mask.Length];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int d = -radius; d <= radius; d++)
                {
                    if (x + d >= 0 && x + d < width) temporary[y * width + x] += mask[y * width + x + d] * weights[d + radius];
                }
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int d = -radius; d <= radius; d++)
                {
                    if (y + d >= 0 && y + d < height) blurred[y * width + x] += temporary[(y + d) * width + x] * weights[d + radius];
                }
        return blurred;
    }
    public void Dispose() { texture?.Dispose(); texture = null; key = null; }
}
