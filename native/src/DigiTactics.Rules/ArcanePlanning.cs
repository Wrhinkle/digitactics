using System.Globalization;
using System.Text.Json;

namespace DigiTactics.Rules;

public sealed class ArcanePlanningState
{
    public int MarketsOpened { get; set; }
    public bool MarketPlus { get; set; }
    public int SyndicateLevel { get; set; }
    public double SyndicateHealth { get; set; }
    public int Skips { get; set; }
    public List<string> Junk { get; } = [];
    public List<string> Seen { get; } = [];
    public List<string> Anomalies { get; } = [];
    public string? Anomaly { get; set; }
    public int ExtraAnomalies { get; set; }
    public bool PendingAnomaly { get; set; }
}

/// <summary>Arcane choices mutate persistent planning records; combat consumes their modifiers.</summary>
public sealed class ArcanePlanning(MatchState state)
{
    private readonly PlanningAi ai = new(state.Rules);
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);
    private readonly Dictionary<string, ItemDefinition> items = state.Rules.Items.ToDictionary(i => i.Id);
    private T[] Shuffle<T>(IEnumerable<T> values)
    {
        var list = values.ToArray();
        for (int i = list.Length - 1; i > 0; i--) { int j = (int)(state.Random.Next() * (i + 1)); (list[i], list[j]) = (list[j], list[i]); }
        return list;
    }
    public string[] MarketOptions(MatchPlayer player, IReadOnlyList<string> exclude)
    {
        var m = player.Mechanics.Arcane; int tier = m.MarketsOpened >= 2 ? 2 : 1;
        var options = Shuffle(state.Rules.Items.Where(i => i.Category == "contraband" && i.Tier == tier).Select(i => i.Id)).Take(m.MarketPlus ? 4 : 3).ToList();
        var hp = state.Rules.Traits.Single(t => t.Id == "SYN").Parameters["skipHp"];
        int index = Math.Max(1, m.SyndicateLevel) - 1;
        options.Add("arc_pass:" + (index < hp.Length ? hp[index].ToString(CultureInfo.InvariantCulture) : "undefined"));
        return options.Where(id => !exclude.Contains(id)).ToArray();
    }
    public void ChooseMarket(MatchPlayer player, string id)
    {
        if (id.StartsWith("arc_pass:", StringComparison.Ordinal))
        {
            string value = id.Split(':')[1];
            player.Mechanics.Arcane.SyndicateHealth += double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var hp) ? hp : 0;
            player.Mechanics.Arcane.Skips++;
        }
        else if (items.ContainsKey(id)) player.GiveItem(id);
    }
    public void ChooseJunk(MatchPlayer player, string id)
    {
        if (id is "flame" or "bots" or "rod" or "destruct" or "buster" or "spare") player.Mechanics.Arcane.Junk.Add(id);
    }
    public string[] AnomalyOptions(MatchPlayer player, IReadOnlyList<string> exclude)
    {
        if (exclude.Count > 0 && !player.Economy.Spend(1)) return exclude.ToArray();
        var m = player.Mechanics.Arcane;
        var pool = state.Rules.AnomalyTags!.Keys.Where(id => !exclude.Contains(id) && !m.Seen.Contains(id)).ToArray();
        if (pool.Length < 3) { m.Seen.Clear(); pool = state.Rules.AnomalyTags.Keys.Where(id => !exclude.Contains(id)).ToArray(); }
        var options = Shuffle(pool).Take(3).ToArray(); m.Seen.AddRange(options); return options;
    }
    private static bool Free(PlanningUnit? unit) => unit is not null && !(unit.Mods is { } mods && mods.TryGetProperty("arcAnom", out var value) &&
        value.ValueKind is not (JsonValueKind.Null or JsonValueKind.False or JsonValueKind.Undefined) &&
        (value.ValueKind != JsonValueKind.String || value.GetString() != "") && (value.ValueKind != JsonValueKind.Number || value.GetDouble() != 0));
    public PlanningUnit? AnomalyTarget(MatchPlayer player)
    {
        if (!player.IsHuman && ai.Holders(player).FirstOrDefault(h => Free(h.Unit)) is { } holder) return holder.Unit;
        var center = player.Roster.At(UnitSpot.Board(3, 5)); if (Free(center)) return center;
        return player.Roster.Board.Select(e => e.Unit).Where(Free).OrderByDescending(u => UnitValues.Value(lines[u.Line], u.Stage))
            .ThenByDescending(u => u.Items.Length).ThenBy(u => u.Id).FirstOrDefault() ?? player.Roster.Bench.FirstOrDefault(Free);
    }
    public void ChooseAnomaly(MatchPlayer player, string id)
    {
        if (!state.Rules.AnomalyTags!.ContainsKey(id)) return;
        var unit = AnomalyTarget(player); if (unit is null) { player.Economy.Credit(8); return; }
        var mods = unit.Mods is { ValueKind: JsonValueKind.Object } existing ? existing.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone()) : [];
        mods["arcAnom"] = JsonSerializer.SerializeToElement(id);
        player.Roster.UpdateUnit(unit with { Mods = JsonSerializer.SerializeToElement(mods) });
        var m = player.Mechanics.Arcane; m.Anomalies.Add(id); m.Anomaly = id;
        if (id == "u_crest")
        {
            var counts = ai.Counts(player.Roster.Board.Select(e => e.Unit));
            var traits = state.Rules.Traits.ToDictionary(t => t.Id);
            var candidates = lines[unit.Line].Traits.Where(t => items.ContainsKey("arc_crest_" + t.ToLowerInvariant()) && traits[t].Breakpoints[0] > 1).ToArray();
            int Gap(string trait) { int n = counts.GetValueOrDefault(trait); int next = traits[trait].Breakpoints.FirstOrDefault(b => b > n); return next != 0 ? next - n : 99; }
            if (candidates.Length == 0) player.Economy.Credit(6);
            else player.GiveItem("arc_crest_" + candidates.OrderBy(Gap).First().ToLowerInvariant());
        }
        if (id == "u_dual")
        {
            var hints = lines[unit.Line].Ai!; string role = hints.Carry ? hints.Damage == "ap" ? "ap" : "ad" : "tank";
            var preferred = (state.Rules.ItemRoles!.GetValueOrDefault(role) ?? []).Where(state.Rules.CoreItems.Contains).Take(8);
            foreach (string item in Shuffle(preferred).Take(2)) player.GiveItem(item);
        }
        if (m.ExtraAnomalies > 0) { m.ExtraAnomalies--; m.PendingAnomaly = true; }
    }
}
