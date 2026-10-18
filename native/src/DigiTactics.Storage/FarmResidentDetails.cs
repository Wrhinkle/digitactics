using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

/// <summary>Read-only details shown by the original Farm.card and its evolution-care breakdown.</summary>
public sealed record FarmResidentDetails(EvolutionView Evolution, EvolutionCare Care, long AgeDays, int IdealWeight,
    bool Fat, bool Thin, int Mistakes, int PoopNeglect, int Overfeed, bool Sick, string? TrainingRefusal)
{
    public static FarmResidentDetails Read(string farmJson,string? legendJson,string? walletJson,string id,long timestamp)
    {
        var farm=JsonNode.Parse(farmJson)??throw new InvalidDataException("Invalid farm save.");
        var pet=farm["pets"]?[id]??throw new InvalidDataException("Farm resident is unavailable.");
        var evolution=LegendEvolution.Inspect(legendJson,walletJson,farmJson,id);
        int ideal=new[]{10,20,30,40}[Math.Clamp(LegendEvolution.Form(evolution.Form).Tier,0,3)];
        double Number(string key)=>pet[key]?.GetValue<double>()??0;
        return new(evolution,LegendEvolution.Care(legendJson,farmJson,id),(long)Math.Floor((timestamp-Number("born"))/86_400_000),ideal,
            Number("weight")>=ideal*1.4,Number("weight")<=ideal*.7,(int)Number("mistakes"),(int)Number("poopNeglect"),(int)Number("overfeed"),
            pet["sick"]?.GetValue<bool>()==true,FarmCare.TrainingRefusal(farm,legendJson,id));
    }
}
