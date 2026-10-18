using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal static class UiTextChecks
{
    private sealed record AbilityCase(string Variant, string Line, int Stage, double Ap, string Text);
    private sealed record ConsumableCase(string Variant, string Id, int Held, int Bench, string Text);
    public static void Run(ContentCatalog catalog)
    {
        IncomePreviewChecks.Run(catalog);
        PlayerRoundResultChecks.Run(catalog);
        var display = new DisplayText(); int abilities = 0;
        foreach (var rules in catalog.Variants)
        {
            var line = rules.Lines.First(line => line.Traits.Length > 0);
            CombatEntry[] duplicates = [new(line.Id,0,0,4,1),new(line.Id,2,1,4,2),new(line.Id,1,2,4,3)];
            var solo = TraitContributors.Select(rules, duplicates, line.Traits[0], false);
            var combined = TraitContributors.Select(rules, duplicates, line.Traits[0], true);
            if (solo.Length != 1 || solo[0].Uid != 2 || combined.Length != 3)
                throw new InvalidOperationException($"Trait contributor duplicate/form selection failed: {rules.Id}");
            var emblem = rules.Items.FirstOrDefault(item => item.Emblem is not null && !line.Traits.Contains(item.Emblem));
            if (emblem is not null)
            {
                CombatEntry[] equipped = [duplicates[0] with { Items = [emblem.Id] }];
                if (TraitContributors.Select(rules,equipped,emblem.Emblem!,false).Length != 1 || TraitContributors.Select(rules,equipped,emblem.Emblem!,true).Length != 0)
                    throw new InvalidOperationException($"Trait contributor emblem selection failed: {rules.Id}");
            }
        }
        foreach (var rules in catalog.Variants)
            foreach (var line in rules.Lines.Concat(rules.ExtraLines))
                for (int stage = 0; stage < line.Forms.Length; stage++)
                {
                    var ability = display.Ability(rules.Id, line.Id, stage);
                    if (ability.Name != line.Ability.Names[stage]) throw new InvalidOperationException($"Ability display name mismatch: {rules.Id}/{line.Id}/{stage}");
                    abilities++;
                }
        Console.WriteLine($"Ability display coverage passed: {abilities} forms across {catalog.Variants.Length} rulesets.");
        var abilityCases = System.Text.Json.JsonSerializer.Deserialize<AbilityCase[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content/ability-display-cases.json")), new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        foreach (var sample in abilityCases)
            if (display.Ability(sample.Variant, sample.Line, sample.Stage, sample.Ap).Description != sample.Text)
                throw new InvalidOperationException($"AP-scaled ability text differs from source: {sample}");
        Console.WriteLine($"Shared ability AP display checks passed: {abilityCases.Length} source comparisons.");
        int breakpoints = 0;
        foreach (var rules in catalog.Variants)
            foreach (var trait in rules.Traits)
                for (int i = 0; i < trait.Breakpoints.Length; i++)
                {
                    if (display.TraitBreakpoint(rules.Id, trait.Id, i).Name != trait.Breakpoints[i].ToString(System.Globalization.CultureInfo.InvariantCulture))
                        throw new InvalidOperationException($"Trait breakpoint display mismatch: {rules.Id}/{trait.Id}/{i}");
                    breakpoints++;
                }
        Console.WriteLine($"Trait breakpoint display coverage passed: {breakpoints} levels.");
        var cases = System.Text.Json.JsonSerializer.Deserialize<ConsumableCase[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content/consumable-preview-cases.json")), new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        foreach (var sample in cases)
        {
            var rules = catalog.Variants.Single(rules => rules.Id == sample.Variant);
            if (display.ConsumablePreview(rules, sample.Id, sample.Held, sample.Bench) != sample.Text)
                throw new InvalidOperationException($"Consumable preview differs from source: {sample}");
        }
        Console.WriteLine($"Consumable preview source parity passed: {cases.Length} target/bench combinations.");
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
