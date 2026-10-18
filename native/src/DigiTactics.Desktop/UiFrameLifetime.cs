using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal static class UiFrameLifetime
{
    // BRUTAL strings live for one frame. Reclaim only after the previous frame's
    // draw data has been consumed, before event handlers or widgets create strings.
    public static void Begin() => ImString.SharedStorage.Clear();

    public static void Verify()
    {
        string value = new('x', 4096);
        for (int frame = 0; frame < 8192; frame++)
        {
            Begin();
            ImString first = value;
            ImString second = $"Frame {frame}: café";
            if (first.Length != value.Length || first.AsSpan()[0] != (byte)'x' ||
                second.ToString() != $"Frame {frame}: café")
                throw new InvalidOperationException("Frame string storage corrupted a live UI label.");
        }
        Begin();
        Console.WriteLine("UI frame lifetime passed: 8192 frames, over 32 MiB of labels through the 8 MiB arena, concurrent UTF-8 labels preserved.");
    }
}
