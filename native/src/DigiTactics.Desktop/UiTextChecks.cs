using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal static class UiTextChecks
{
    public static void Run()
    {
        ImGui.SetNextWindowPos(new(0, 0)); ImGui.SetNextWindowSize(new(380, 800));
        ImGui.Begin("Literal text check"u8, ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoScrollbar);
        string[] samples = ["+15% AD", "+12% AS", "Tamer %s %n %08x %%", "100% ready ## visible", "Digi-Data: 50% — café", string.Concat(Enumerable.Repeat("+20% power, ", 240))];
        foreach (string sample in samples)
        {
            var expected = ImGui.CalcTextSize(sample, false, ImGui.GetContentRegionAvail().X);
            UiText.Wrapped(sample);
            var actual = ImGui.GetItemRectSize();
            if (Math.Abs(expected.X - actual.X) > 1 || Math.Abs(expected.Y - actual.Y) > 1)
                throw new InvalidOperationException($"Literal wrapped text changed dimensions: {actual} versus {expected}.");
        }
        ImGui.End();
        Console.WriteLine("Literal UI text checks passed: percent stats, format-like player names, Unicode, visible hashes and long wrapped descriptions.");
    }
}
