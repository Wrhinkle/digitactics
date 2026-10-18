using System.Text.Json;

namespace DigiTactics.Rules;

public sealed record ProjectedCombatSide(CombatEntry[] Units, CombatSideEffects Effects, int Columns);

/// <summary>Copies persistent planning state into isolated Solo or owner-scoped Duos combat inputs.</summary>
public sealed class MatchCombatProjection(MatchState state)
{
    private readonly string mode = state.Rules.Id.Split('/')[0];
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);

    private static string? Text(PlanningUnit unit, string key) => unit.Mods is { } mods &&
        mods.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static double Number(PlanningUnit unit, string key) => unit.Mods is { } mods &&
        mods.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : 0;

    public CombatEntry[] Board(MatchPlayer player, int columnOffset = 0, int? owner = null) => player.Roster.Board.Select(entry =>
    {
        var unit = entry.Unit;
        string? form = Text(unit, "form"), power = Text(unit, "pu");
        return new CombatEntry(unit.Line, unit.Stage, entry.Spot.Column + columnOffset, entry.Spot.Row, unit.Id, owner,
            unit.Items.ToArray(), unit.BonusTraits is { } traits ? new(traits) : null,
            FatesPlanning.IsPartner(unit) ? new(Text(unit, "bonus"), Text(unit, "trait")) : null,
            Number(unit, "glutHp"), Number(unit, "glutAp"), Text(unit, "arcAnom"),
            form is not null || power is not null ? new(form, power) : null);
    }).ToArray();

    public double PartnerTop(MatchPlayer player)
    {
        if (!state.Duos || player.Team is not { } team || state.Teams[team].Members.FirstOrDefault(p => p != player && p.Alive) is not { } partner) return 0;
        return partner.Roster.Board.Select(e => (double)UnitValues.Value(lines[e.Unit.Line], e.Unit.Stage)).DefaultIfEmpty(0).Max();
    }

    private CombatSideEffects Effects(IReadOnlyList<MatchPlayer> players)
    {
        var first = players[0];
        var mechanics = players.Select(p => p.Mechanics).ToArray();
        return new(Augments: players.SelectMany(p => p.Augments).Distinct().ToArray(),
            Buffs: players.Count == 1 ? first.NextBuffs : null,
            PartnerTop: players.Count == 1 ? PartnerTop(first) : 0,
            Fates: mode == "fates" ? new(mechanics.Max(m => m.FatesDeva), mechanics.Select(m => m.FatesPath).FirstOrDefault(p => p is not null), mechanics.Max(m => m.FatesBlossom)) : null,
            Galaxies: mode == "galaxies" ? new(new(mechanics.Select(m => m.GalaxiesBounties).FirstOrDefault(b => b.Count > 0) ?? [])) : null,
            Gizmos: mode == "gizmos" ? new(mechanics.Max(m => m.Gizmos.GoldStart)) : null,
            Dragonlands: mode == "dragonlands" ? new(mechanics.Max(m => m.DragonBank), mechanics.FirstOrDefault(m => m.HasDragonHpFull)?.DragonHpFull ?? false, mechanics.Max(m => m.DragonTrainingKills)) : null,
            Arcane: mode == "arcane" ? new(mechanics.Max(m => m.Arcane.SyndicateHealth), mechanics.Select(m => m.Arcane).FirstOrDefault(m => m.HasSyndicateAugment)?.SyndicateAugment ?? false,
                mechanics.Max(m => m.Arcane.Chests), mechanics.SelectMany(m => m.Arcane.Junk).ToArray()) : null,
            Coliseum: mode == "coliseum" ? new(mechanics.Max(m => m.ColiseumPirate.AbilityPower), mechanics.Select(m => m.ColiseumSupreme).FirstOrDefault(p => p is not null)) : null,
            Underlords: mode is "underlords" or "underlords_ko" ? new(mechanics.Max(m => m.UnderlordsDuel), mechanics.Max(m => m.UnderlordsBeastKills)) : null,
            Globals: players.SelectMany(p => p.Globals).ToArray(), Power: first.Power);
    }

    public ProjectedCombatSide Player(MatchPlayer player) => new(Board(player), Effects([player]), 7);

    public ProjectedCombatSide Team(MatchTeam team)
    {
        var units = team.Members.SelectMany((player, index) => Board(player, index * 7, player.Id)).ToArray();
        var parts = team.Members.Select(player =>
        {
            var side = Effects([player]);
            return new CombatPartner(player.Id, side.Augments, side.Buffs, side.PartnerTop, side.Gizmos,
                side.Dragonlands, side.Arcane, side.Coliseum, side.Underlords, side.Globals, side.Power);
        }).ToArray();
        var effects = Effects(team.Members) with { Parts = parts, Counts = TraitRules.TeamCount(state.Rules, units, parts) };
        return new(units, effects, 14);
    }

    public CombatSetup Setup(ProjectedCombatSide a, ProjectedCombatSide b, RoundPosition round) =>
        new(a.Effects, b.Effects, state.Galaxy, state.GizmosMutation, state.Rules.Schedule!.TypeOf(round), round.Stage,
            state.DragonMirage, state.Rules.Pace, state.ArcaneSponsor.ToArray());
}
