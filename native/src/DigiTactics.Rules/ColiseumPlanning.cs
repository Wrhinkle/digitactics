using System.Text.Json;
using System.Globalization;

namespace DigiTactics.Rules;

public sealed record ColiseumPowerUp(string Id, string[] Tags);
public sealed record ColiseumPlanningRules(ColiseumPowerUp[] PowerUps, string[] CoreRounds);

/// <summary>Digi-Core inventory and choices share the same pending target and refund rules.</summary>
public sealed class ColiseumPlanning(MatchState state) : IMatchShopHooks
{
    private ColiseumPlanningRules Rules => state.Rules.Coliseum!;
    private readonly MatchLoot loot = new(state);
    private readonly PlanningAi ai = new(state.Rules);
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);
    private readonly HashSet<string> shopLines = state.Rules.Lines.Select(l => l.Id).ToHashSet();

    public void AiTurn(MatchPlayer player)
    {
        bool Free(PlanningUnit unit) => shopLines.Contains(unit.Line) &&
            !(unit.Mods is { } mods && mods.TryGetProperty("pu", out var power) && Truthy(power));
        for (int guard = 0; guard < 4 && player.Items.Contains("col_core"); guard++)
        {
            var target = ai.Holders(player).Select(h => h.Unit).FirstOrDefault(Free) ??
                player.Roster.Board.Select(e => e.Unit).Where(Free).OrderByDescending(u => UnitValues.Value(lines[u.Line], u.Stage)).FirstOrDefault();
            if (target is null) break;
            var options = PowerOptions(target, []);
            if (options.Length == 0) break;
            player.Items.RemoveAt(player.Items.IndexOf("col_core"));
            string chosen = options[(int)(state.Random.Next() * options.Length)];
            var mods = target.Mods?.EnumerateObject().ToDictionary(p => p.Name, p => p.Value) ?? [];
            mods["pu"] = JsonSerializer.SerializeToElement(chosen);
            player.Roster.UpdateUnit(target with { Mods = JsonSerializer.SerializeToElement(mods) });
        }
    }

    public void RoundEnd(RoundEndContext context, int stage)
    {
        if (context.Match.Combat is null) return;
        var player = context.Player;
        var units = context.Units.Select(u => u.State ?? throw new InvalidOperationException("A combat result is missing unit state.")).ToArray();
        CombatUnit? top = null;
        foreach (var unit in units)
            if (unit.Team == context.Team && !unit.Summoned && unit.Traits.Contains("SC") &&
                (top is null || unit.Dealt > top.Dealt)) top = unit;
        if (top is not null) player.Mechanics.ColiseumSupreme = top.Line.Id;
        if (context.Pve) return;
        int level = TraitRules.Level(state.Rules.Traits.Single(t => t.Id == "CG"), context.Traits.GetValueOrDefault("CG"));
        if (level > 0)
        {
            var gambit = player.Mechanics.ColiseumGambit;
            int kills = units.Count(u => u.Team != context.Team && !u.Alive && !u.Summoned);
            double multiplier = stage >= 4 ? 2 : stage >= 3 ? 1.5 : 1;
            double gained = kills * multiplier + (context.Won ? 0 : 4 * multiplier);
            gambit.Level = level;
            if (gambit.DoubleDown && !context.Won) gained *= 2;
            gambit.Gems += gained;
            if (level >= 2)
            {
                gambit.Kills += kills;
                int target = gambit.KillTarget != 0 ? gambit.KillTarget : 8;
                if (gambit.Kills >= target)
                {
                    gambit.Kills -= target;
                    if (loot.RandomComponent() is { } item) player.GiveItem(item);
                    gambit.KillTarget = 6 + (int)(state.Random.Next() * 5);
                }
            }
            if (gambit.DoubleDown && context.Won) { gambit.Gems *= .5; CashOut(player); }
            else { gambit.Count++; if (gambit.Count >= 3) gambit.Ready = true; }
        }
        if (context.Traits.GetValueOrDefault("CH") != 0)
        {
            var champ = units.FirstOrDefault(u => u.Team == context.Team && u.Line.Id == "col_craniamon");
            if (context.Won) player.Mechanics.ColiseumFans += champ is null ? 1 : champ.Stage + 1;
            else if (player.Mechanics.ColiseumFans != 0)
            {
                player.Health += Math.Min(player.Mechanics.ColiseumFans, state.Rules.Loss!.BaseDamage(stage));
                player.Mechanics.ColiseumFans = 0;
            }
        }
    }

    public void RoundStart(MatchPlayer player, RoundPosition round)
    {
        if (Rules.CoreRounds.Contains(round.Label)) player.Mechanics.ColiseumCores++;
        while (player.Mechanics.ColiseumCores > 0 && player.Items.Count < state.Rules.Loot!.ItemCap)
        {
            player.GiveItem("col_core"); player.Mechanics.ColiseumCores--;
        }
        player.Shop.FreeRolls += player.Roster.Board.Count(e => e.Unit.Stage == 2 &&
            state.Rules.Lines.Concat(state.Rules.ExtraLines).Single(l => l.Id == e.Unit.Line).Traits.Contains("CREW"));
        if (round.Stage >= 2 && player.Roster.Board.Any(e => e.Unit.Line == "col_piedmon")) Bounty(player);
        var gambit = player.Mechanics.ColiseumGambit;
        if (gambit.Ready && gambit.Gems > 0)
        {
            var options = GambitOptions(player);
            if (!player.IsHuman)
            {
                string best = options[0]; double bestScore = double.NegativeInfinity;
                foreach (string id in options)
                {
                    double score = (id.StartsWith("double", StringComparison.Ordinal)
                        ? player.Health > 10 && player.Economy.Streak < 0 ? 2 : -1 : 1) + state.Random.Next();
                    if (score > bestScore) { bestScore = score; best = id; }
                }
                ChooseGambit(player, best);
            }
            else if (player.Offer is null)
            {
                player.Offer = new("col_gambit", options, 0, null);
                return;
            }
        }
        // AI resolves every missing form in board/bench order; humans get one pending offer.
        foreach (var entry in player.Roster.Entries.ToArray())
        {
            var unit = entry.Unit;
            if (!state.Rules.Forms!.TryGetValue(unit.Line, out var forms) ||
                unit.Mods is { } mods && mods.TryGetProperty("form", out var current) && Truthy(current)) continue;
            if (!player.IsHuman) { player.Roster.UpdateUnit(ApplyForm(unit, AiForm(player, forms))); continue; }
            if (player.Offer is null)
            {
                player.Mechanics.ColiseumFormUnitId = unit.Id;
                player.Offer = new("col_form", forms.Select(f => f.Id).ToArray(), 0, null);
            }
            break;
        }
    }

    private void Bounty(MatchPlayer player)
    {
        var pirate = player.Mechanics.ColiseumPirate;
        int count = ++pirate.Count;
        List<string> rewards = ["gold", "rolls", "ap", "shield"];
        if (count >= 3) rewards.AddRange(["comp", "comp"]);
        if (count >= 6) rewards.Add("big");
        if (count >= 8) rewards.Add("item");
        string reward = rewards[(int)(state.Random.Next() * rewards.Count)];
        pirate.LastReward = reward;
        switch (reward)
        {
            case "gold": player.Economy.Credit(2 + count / 3); break;
            case "rolls": player.Shop.FreeRolls += 2; break;
            case "ap": pirate.AbilityPower += 8; break;
            case "shield": player.NextBuffs = (player.NextBuffs ?? new()) with { Armor = (player.NextBuffs?.Armor ?? 0) + 25 }; break;
            case "comp": if (loot.RandomComponent() is { } component) player.GiveItem(component); break;
            case "big": player.Economy.Credit(8); break;
            case "item": if (loot.RandomCompleted() is { } item) player.GiveItem(item); break;
        }
    }

    public PlanningUnit Bought(MatchPlayer player, PlanningUnit unit, int slot, MatchPhase phase)
    {
        if (!state.Rules.Forms!.TryGetValue(unit.Line, out var forms) ||
            unit.Mods is { } mods && mods.TryGetProperty("form", out var current) && Truthy(current)) return unit;
        if (!player.IsHuman) return ApplyForm(unit, AiForm(player, forms));
        if (player.Offer is null && phase == MatchPhase.Planning)
        {
            player.Mechanics.ColiseumFormUnitId = unit.Id;
            player.Offer = new("col_form", forms.Select(f => f.Id).ToArray(), 0, null);
        }
        return unit;
    }

    public void Sold(MatchPlayer player, PlanningUnit unit)
    {
        if (unit.Mods is { } mods && mods.TryGetProperty("pu", out var power) && Truthy(power)) GiveCore(player);
    }

    public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources)
    {
        var mods = new Dictionary<string, JsonElement>();
        int powers = 0;
        foreach (var source in sources)
        {
            if (source.Mods is not { } sourceMods) continue;
            if (sourceMods.TryGetProperty("pu", out var power) && Truthy(power))
            {
                powers++;
                mods.TryAdd("pu", power);
            }
            if (sourceMods.TryGetProperty("form", out var form) && Truthy(form)) mods.TryAdd("form", form);
        }
        var forms = state.Rules.Forms!.GetValueOrDefault(unit.Line);
        var selected = mods.TryGetValue("form", out var choice) && choice.ValueKind == JsonValueKind.String
            ? forms?.FirstOrDefault(f => f.Id == choice.GetString()) : null;
        player.Roster.UpdateUnit(unit with { Mods = mods.Count == 0 ? null : JsonSerializer.SerializeToElement(mods),
            BonusTraits = selected?.Trait is { } trait ? new() { [trait] = 1 } : forms is not null ? null : unit.BonusTraits });
        for (int i = 1; i < powers; i++) GiveCore(player);
    }

    private FormDefinition AiForm(MatchPlayer player, FormDefinition[] forms)
    {
        var counts = TraitRules.Count(state.Rules, player.Roster.Board.Select(e => new CombatEntry(e.Unit.Line,
            e.Unit.Stage, e.Spot.Column, e.Spot.Row, Items: e.Unit.Items, BonusTraits: e.Unit.BonusTraits)), player.Augments.ToArray());
        FormDefinition? best = null;
        double bestScore = double.NegativeInfinity;
        foreach (var form in forms)
        {
            double score = state.Random.Next();
            if (form.Trait is { } trait)
            {
                int count = counts.GetValueOrDefault(trait);
                score += count + (state.Rules.Traits.Single(t => t.Id == trait).Breakpoints.Contains(count + 1) ? 3 : 0);
            }
            if (form.Id == "ballista" && player.Roster.Board.Count(e => lines[e.Unit.Line].Ai!.Tank is > 0 and <= 2) < 2) score += 2;
            if (score > bestScore) { bestScore = score; best = form; }
        }
        return best ?? throw new InvalidDataException("A form-bearing unit has no forms.");
    }

    private static PlanningUnit ApplyForm(PlanningUnit unit, FormDefinition form)
    {
        var mods = unit.Mods?.EnumerateObject().ToDictionary(p => p.Name, p => p.Value) ?? [];
        mods["form"] = JsonSerializer.SerializeToElement(form.Id);
        return unit with { Mods = JsonSerializer.SerializeToElement(mods),
            BonusTraits = form.Trait is null ? unit.BonusTraits : new() { [form.Trait] = 1 } };
    }

    public string[] FormOptions(MatchPlayer player)
    {
        var unit = player.Roster.Entries.FirstOrDefault(e => e.Unit.Id == player.Mechanics.ColiseumFormUnitId)?.Unit;
        return unit is null ? [] : (state.Rules.Forms!.GetValueOrDefault(unit.Line) ?? []).Select(f => f.Id).ToArray();
    }

    public void ChooseForm(MatchPlayer player, string id)
    {
        var unit = player.Roster.Entries.FirstOrDefault(e => e.Unit.Id == player.Mechanics.ColiseumFormUnitId)?.Unit;
        player.Mechanics.ColiseumFormUnitId = null;
        var form = unit is null ? null : state.Rules.Forms!.GetValueOrDefault(unit.Line)?.FirstOrDefault(f => f.Id == id);
        if (unit is null || form is null) return;
        player.Roster.UpdateUnit(ApplyForm(unit, form));
    }

    public string[] GambitOptions(MatchPlayer player)
    {
        string gems = Math.Floor(player.Mechanics.ColiseumGambit.Gems + .5).ToString(CultureInfo.InvariantCulture);
        return [$"cash:{gems}", $"double:{gems}"];
    }

    public void ChooseGambit(MatchPlayer player, string id)
    {
        var gambit = player.Mechanics.ColiseumGambit;
        if (id.StartsWith("double", StringComparison.Ordinal))
        {
            gambit.DoubleDown = true; gambit.Count = 0; gambit.Ready = false;
            return;
        }
        CashOut(player);
    }

    public void CashOut(MatchPlayer player)
    {
        var gambit = player.Mechanics.ColiseumGambit;
        double gems = gambit.Gems * (gambit.Level >= 3 ? 1.3 : 1);
        player.Economy.Credit(checked((int)Math.Floor(gems * .5 + .5)));
        if (gems >= 80) { Give(loot.RandomCompleted()); Give(loot.RandomCompleted()); }
        else if (gems >= 45) Give(loot.RandomCompleted());
        else if (gems >= 25) Give(loot.RandomComponent());
        gambit.Gems = 0; gambit.Count = 0; gambit.DoubleDown = false; gambit.Ready = false;
        void Give(string? item) { if (item is not null) player.GiveItem(item); }
    }

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
        return unit is null ? [] : PowerOptions(unit, exclude);
    }

    private string[] PowerOptions(PlanningUnit unit, IReadOnlyList<string> exclude)
    {
        var hints = lines[unit.Line].Ai!;
        string kind = !hints.Carry && hints.Tank > 0 || hints.Place == "front" && hints.Tank is > 0 and <= 2
            ? "tank" : hints.Damage == "ap" ? "ap" : "ad";
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
