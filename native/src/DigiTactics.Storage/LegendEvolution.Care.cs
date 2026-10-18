namespace DigiTactics.Storage;

public sealed record EvolutionCare(IReadOnlyDictionary<string,double> Values, IReadOnlyDictionary<string,double> Bonus);

public static partial class LegendEvolution
{
    public static EvolutionCare Care(string? legendJson, string? farmJson, string id,string? displayedForm=null)
    {
        var (_, record) = LegendRewards.Snapshot(legendJson,id);
        var values = new[] {"bond","battle","food"}.ToDictionary(key=>key,key=>Number(record["care"]!,key));
        return new(values,FarmCare(farmJson,id,Form(displayedForm??record["form"]!.GetValue<string>()).Tier));
    }
}
