namespace DigiTactics.Rules;

public sealed record WispDefinition(string Id, int Base, int MinStage);
public sealed record WispOffer(string Id, int Cost);

/// <summary>Persistent Faction Wars rewards and one-use Data Wisp purchases.</summary>
public sealed class FactionWarsPlanning(MatchState state)
{
    private readonly MatchLoot loot = new(state);
    public void AugmentPicked(MatchPlayer player, string id)
    {
        switch (id)
        {
            case "pocket": player.Economy.Credit(8); break;
            case "bank": player.Economy.Credit(18); break;
            case "jackpot": player.Economy.Credit(35); break;
            case "cache": player.GiveItem(loot.RandomComponent()!); player.GiveItem(loot.RandomComponent()!); break;
            case "trove": player.GiveItem(loot.RandomCompleted()!); player.GiveItem(loot.RandomComponent()!); break;
            case "armory": player.GiveItem(loot.RandomCompleted()!); player.GiveItem(loot.RandomCompleted()!); break;
            case "egg": player.GiveItem("egg"); break;
            case "goldenEgg": player.GiveItem("golden_egg"); break;
            case "heart": player.Health += 5; break;
            case "rolls": player.Shop.FreeRolls++; break;
            case "rolls2": player.Shop.FreeRolls += 2; break;
            case "evolve": loot.GiveUnit(player, [2, 3], 1); break;
            case "megaEvolve": loot.GiveUnit(player, [4], 1); break;
        }
    }
    public IncomeBreakdown Income(MatchPlayer player, IncomeBreakdown income) =>
        player.Augments.Contains("income") ? income with { Augment = income.Augment + 2 } : income;

    public void RoundStart(MatchPlayer player, RoundPosition round)
    {
        if (player.Augments.Contains("rolls")) player.Shop.FreeRolls++;
        if (player.Augments.Contains("rolls2")) player.Shop.FreeRolls += 2;
        player.Wisp = null;
        if (round.Stage < 2 || round.Round % 2 != 0) return;
        var options = state.Rules.Wisps!.Where(w => w.MinStage <= round.Stage).ToArray();
        var wisp = options[(int)(state.Random.Next() * options.Length)];
        player.Wisp = new(wisp.Id, wisp.Base + round.Stage);
    }
    public void AiTurn(MatchPlayer player, MatchPhase phase)
    {
        if (player.Wisp is { } wisp && player.Economy.Gold >= wisp.Cost + 2 && state.Random.Next() < .5)
            Buy(player, phase);
    }
    public string Act(MatchPlayer player, MatchPhase phase) => phase == MatchPhase.Planning ? Buy(player, phase) : "phase";

    private string Buy(MatchPlayer player, MatchPhase phase)
    {
        if (player.Wisp is not { } wisp) return "none";
        if (!player.Economy.Spend(wisp.Cost)) return "gold";
        player.Wisp = null;
        void Combine()
        {
            player.Roster.Combine(phase == MatchPhase.Combat && player.IsHuman, player.GiveItem);
            player.Shop.ClearMaxed();
        }
        switch (wisp.Id)
        {
            case "gold": player.Economy.Credit(wisp.Cost * 2); break;
            case "comp": player.GiveItem(loot.RandomComponent()!); break;
            case "egg": player.GiveItem("egg"); break;
            case "copy":
                var rookies = player.Roster.Entries.Select(e => e.Unit).Where(u => u.Stage == 0 && state.Pool.Remaining(u.Line) > 0).ToArray();
                if (rookies.Length > 0 && player.Roster.FreeBenchSlot >= 0)
                {
                    var source = rookies[(int)(state.Random.Next() * rookies.Length)];
                    state.Pool.Take(source.Line); player.Roster.AddToBench(state.CreateUnit(source.Line, 0)); Combine();
                }
                else player.Economy.Credit(wisp.Cost);
                break;
            case "evolve":
                var rookie = player.Roster.Board.Select(e => e.Unit).Where(u => u.Stage == 0 && state.Pool.Remaining(u.Line) >= 2)
                    .OrderBy(u => state.Rules.Lines.Single(l => l.Id == u.Line).Tier).FirstOrDefault();
                if (rookie is not null)
                { player.Roster.UpdateUnit(rookie with { Stage = 1 }); state.Pool.Take(rookie.Line, 2); Combine(); }
                else player.Economy.Credit(wisp.Cost);
                break;
            case "level": player.TeamBonus++; break;
            case "battle": player.NextBuffs = (player.NextBuffs ?? new()) with { AttackSpeed = (player.NextBuffs?.AttackSpeed ?? 0) + .3 }; break;
            case "iron": player.NextBuffs = (player.NextBuffs ?? new()) with { Armor = (player.NextBuffs?.Armor ?? 0) + 35 }; break;
            case "mana": player.NextBuffs = (player.NextBuffs ?? new()) with { Mana = (player.NextBuffs?.Mana ?? 0) + 40 }; break;
            case "heal": player.Health += 3; break;
            case "gamble": if (state.Random.Next() < .5) player.Economy.Credit(wisp.Cost * 3); break;
            case "pact": player.Health -= 3; player.GiveItem(loot.RandomCompleted()!); break;
            case "rolls": player.Shop.FreeRolls += 4; break;
            case "fortune": player.Shop.Refresh(2); break;
            case "crest": player.GiveItem(loot.RandomEmblem(player)!); break;
        }
        return wisp.Id;
    }
}
