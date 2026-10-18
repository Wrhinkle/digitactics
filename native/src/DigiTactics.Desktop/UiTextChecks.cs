using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal static class UiTextChecks
{
    public static void Run(ContentCatalog catalog)
    {
        var display = new DisplayText(); int abilities = 0;
        foreach (var rules in catalog.Variants)
            foreach (var line in rules.Lines.Concat(rules.ExtraLines))
                for (int stage = 0; stage < line.Forms.Length; stage++)
                {
                    var ability = display.Ability(rules.Id, line.Id, stage);
                    if (ability.Name != line.Ability.Names[stage]) throw new InvalidOperationException($"Ability display name mismatch: {rules.Id}/{line.Id}/{stage}");
                    abilities++;
                }
        Console.WriteLine($"Ability display coverage passed: {abilities} forms across {catalog.Variants.Length} rulesets.");
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
