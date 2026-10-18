using DigiTactics.Storage;

namespace DigiTactics.Presentation;

public sealed record IncubatorLegend(CosmeticItem Item, EvolutionView Evolution);
public sealed record EggExclusive(string Kind, CosmeticItem Item, bool Owned, double Percent)
{
    public string Chance => Percent.ToString(Percent < 1 ? "0.00" : "0.0", System.Globalization.CultureInfo.InvariantCulture) + "%";
}
public sealed record IncubatorEgg(EggDefinition Definition, int Owned, int Total, EggOdds[] Odds, EggExclusive[] Exclusives);

/// <summary>Read-only Loot inventory, using the original ready-first ordering and per-rarity egg pools.</summary>
public sealed record IncubatorOverview(IncubatorLegend[] Legends, IncubatorEgg[] Eggs, long DigiData, int Ready)
{
    public static IncubatorOverview Read(string? wallet, string? legends, string? farm)
    {
        var collection = new CosmeticCollection(wallet);
        var owned = CosmeticCollection.Items("legend").Where(item => collection.Owns("legend", item.Id))
            .Select(item => new IncubatorLegend(item, LegendEvolution.Inspect(legends, wallet, farm, item.Id)))
            .OrderByDescending(legend => legend.Evolution.CanDigivolve).ThenByDescending(legend => legend.Evolution.Level).ToArray();
        var eggs = DigiEggs.Definitions.Select(egg =>
        {
            var pool = DigiEggs.Pool(egg);
            var odds = DigiEggs.Odds(egg);
            var exclusive = pool.Where(entry => entry.Item.EggOnly && !entry.Item.Free).Select(entry =>
            {
                var rarity = odds.FirstOrDefault(odd => odd.Rarity == (entry.Item.Rarity ?? "common"));
                return new EggExclusive(entry.Kind, entry.Item, collection.Owns(entry.Kind, entry.Item.Id), rarity is null ? 0 : rarity.Pct / rarity.Count);
            }).ToArray();
            return new IncubatorEgg(egg, pool.Count(entry => collection.Owns(entry.Kind, entry.Item.Id)), pool.Length, odds, exclusive);
        }).ToArray();
        return new(owned, eggs, owned.Sum(legend => (long)legend.Evolution.Data), owned.Count(legend => legend.Evolution.CanDigivolve));
    }
}
