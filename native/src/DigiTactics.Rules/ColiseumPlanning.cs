using System.Text.Json;

namespace DigiTactics.Rules;

public sealed record ColiseumPowerUp(string Id, string[] Tags);
public sealed record ColiseumPlanningRules(ColiseumPowerUp[] PowerUps, Dictionary<string, string> UnitKinds);

/// <summary>Digi-Core inventory and choices share the same pending target and refund rules.</summary>
public sealed class ColiseumPlanning(MatchState state)
{
    private ColiseumPlanningRules Rules => state.Rules.Coliseum!;

    public void GiveCore(MatchPlayer player)
    {
        if (player.Items.Count < state.Rules.Loot!.ItemCap) player.GiveItem("col_core");
        else player.Mechanics.ColiseumCores++;
    }

    public string UseCore(MatchPlayer player, int index, PlanningUnit unit, MatchPhase phase)
    {
        if (phase != MatchPhase.Planning) return "locked";
        if (player.Offer is not null || !state.Rules.Lines.Any(l => l.Id == unit.Line)) return "none";
        player.Items.RemoveAt(index);
        player.Mechanics.ColiseumCoreUnitId = unit.Id;
        player.Offer = new("col_core", CoreOptions(player, []), 1, null);
        return "core";
    }

    public string[] CoreOptions(MatchPlayer player, IReadOnlyList<string> exclude)
    {
        var unit = Target(player);
        if (unit is null) return [];
        string kind = Rules.UnitKinds[unit.Line];
        string? current = unit.Mods is { } mods && mods.TryGetProperty("pu", out var pu) && pu.ValueKind == JsonValueKind.String ? pu.GetString() : null;
        var choices = Rules.PowerUps.Where(p => (p.Tags.Contains(kind) || p.Tags.Contains("any")) &&
            p.Id != current && !exclude.Contains(p.Id)).Select(p => p.Id).ToArray();
        for (int i = choices.Length - 1; i > 0; i--)
        {
            int j = (int)(state.Random.Next() * (i + 1));
            (choices[i], choices[j]) = (choices[j], choices[i]);
        }
        return choices.Take(3).ToArray();
    }

    public void ChooseCore(MatchPlayer player, string id)
    {
        var unit = Target(player);
        player.Mechanics.ColiseumCoreUnitId = null;
        if (unit is null || !Rules.PowerUps.Any(p => p.Id == id)) { GiveCore(player); return; }
        var mods = unit.Mods?.EnumerateObject().ToDictionary(p => p.Name, p => p.Value) ?? [];
        mods["pu"] = JsonSerializer.SerializeToElement(id);
        player.Roster.UpdateUnit(unit with { Mods = JsonSerializer.SerializeToElement(mods) });
    }

    public string Extract(MatchPlayer player, int index, PlanningUnit unit)
    {
        if (unit.Mods is not { } mods || !mods.TryGetProperty("pu", out var power) || !Truthy(power)) return "none";
        player.Items.RemoveAt(index);
        var remaining = mods.EnumerateObject().Where(p => p.Name != "pu").ToDictionary(p => p.Name, p => p.Value);
        player.Roster.UpdateUnit(unit with { Mods = remaining.Count == 0 ? null : JsonSerializer.SerializeToElement(remaining) });
        GiveCore(player);
        return "extractor";
    }

    private PlanningUnit? Target(MatchPlayer player) => player.Roster.Entries.FirstOrDefault(e => e.Unit.Id == player.Mechanics.ColiseumCoreUnitId)?.Unit;
    private static bool Truthy(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined or JsonValueKind.False => false,
        JsonValueKind.String => value.GetString()!.Length != 0,
        JsonValueKind.Number => value.GetDouble() != 0,
        _ => true,
    };
}
