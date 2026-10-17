using System.Globalization;

namespace DigiTactics.Rules;

public sealed record WeightedGalaxy(string Id, int Weight);
public sealed record GameStartRules(int[][] Gates, WeightedGalaxy[] Galaxies, string[] Portals);

/// <summary>Game-wide selections and starting rewards precede all initial shop reservations.</summary>
public static class MatchStart
{
    public static MatchState Create(Ruleset rules, uint seed, IReadOnlyList<MatchSeat> seats,
        Func<RoundPosition> position, bool duos = false, int[][]? teams = null) =>
        new(rules, seed, seats, duos, teams, state =>
        {
            Initialize(state);
            new MatchShopRolls(state, position).Install();
        });

    private static void Initialize(MatchState state)
    {
        var config = state.Rules.GameStart!;
        var loot = new MatchLoot(state);
        T Pick<T>(T[] values) => values[(int)(state.Random.Next() * values.Length)];
        switch (state.Rules.Id.Split('/')[0])
        {
            case "gizmos":
                state.GizmosGates = Pick(config.Gates).ToArray();
                state.GizmosMutation = Pick<string>(["cyber", "voracious", "voidborne", "adrenal", "synaptic", "leech", "meta"]);
                break;
            case "dragonlands":
                state.DragonMirage = Pick<string>(["dancer", "warlord", "overload", "executioner"]);
                double treasure = state.Random.Next();
                state.TreasureVariant = treasure < .4 ? "classic" : treasure < .7 ? "order" : "chaos";
                break;
            case "galaxies":
                uint hash = 0x9e3779b9;
                foreach (char character in "galaxy:" + state.Seed.ToString(CultureInfo.InvariantCulture))
                    hash = unchecked((hash ^ character) * 0x01000193);
                uint draw = hash % (uint)config.Galaxies.Sum(g => g.Weight);
                foreach (var galaxy in config.Galaxies)
                {
                    if (draw < galaxy.Weight) { state.Galaxy = galaxy.Id; break; }
                    draw -= (uint)galaxy.Weight;
                }
                if (state.Galaxy == "armory")
                {
                    var a = loot.RandomComponent(); var b = loot.RandomComponent();
                    foreach (var player in state.Players) { player.GiveItem(a!); player.GiveItem(b!); }
                }
                if (state.Galaxy == "shrine") foreach (var player in state.Players) player.GiveItem("gal_mental");
                break;
            case "arcane":
                var sponsor = state.Rules.CoreItems.ToArray();
                for (int i = sponsor.Length - 1; i > 0; i--) { int j = (int)(state.Random.Next() * (i + 1)); (sponsor[i], sponsor[j]) = (sponsor[j], sponsor[i]); }
                state.ArcaneSponsor = sponsor.Take(3).ToArray();
                state.ArcanePortal = Pick(config.Portals);
                foreach (var player in state.Players)
                    switch (state.ArcanePortal)
                    {
                        case "gold": player.Economy.Credit(6); break;
                        case "anvils": player.GiveItem(loot.RandomComponent()!); player.GiveItem(loot.RandomComponent()!); break;
                        case "tag": player.GiveItem("arc_spat"); break;
                        case "device": player.GiveItem("arc_pan"); break;
                        case "copier": player.GiveItem("arc_copier"); break;
                        case "upgraded": loot.GiveUnit(player, [1], 1); break;
                    }
                break;
        }
    }
}
