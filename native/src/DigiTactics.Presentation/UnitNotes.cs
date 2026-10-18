using System.Text.Json;
using DigiTactics.Rules;

namespace DigiTactics.Presentation;

/// <summary>Original set-authored tooltip notes, conditioned on the actual unit modifiers.</summary>
public sealed class UnitNotes
{
    private sealed record Variant(Dictionary<string,string> Traits,Dictionary<string,string> Partner,
        Dictionary<string,string> Anomalies,Dictionary<string,string> PowerUps,Dictionary<string,Dictionary<string,string>> Forms);
    private readonly Dictionary<string,Variant> variants;
    public UnitNotes(string path) => variants=JsonSerializer.Deserialize<Dictionary<string,Variant>>(File.ReadAllText(path),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
    public string[] Describe(string variant,PlanningUnit unit)=>Describe(variant,unit.Line,unit.Mods);
    public string[] Describe(string variant,string line,JsonElement? mods)
    {
        JsonElement Field(string key)=>mods is { ValueKind:JsonValueKind.Object } value&&value.TryGetProperty(key,out var field)?field:default;
        return Describe(variant,line,Truthy(Field("partner")),JsString(Field("trait")),JsString(Field("bonus")),
            JsString(Field("arcAnom")),JsString(Field("pu")),JsString(Field("form")));
    }
    public string[] Describe(string variant,CombatEntry unit)=>Describe(variant,unit.Line,unit.Partner is not null,
        unit.Partner?.Trait??"undefined",unit.Partner?.Bonus??"undefined",unit.ArcaneAnomaly??"undefined",unit.Coliseum?.PowerUp??"undefined",unit.Coliseum?.Form??"undefined");
    private string[] Describe(string variant,string line,bool partner,string trait,string bonus,string anomaly,string power,string form)
    {
        if(!variants.TryGetValue(variant,out var rules))return [];
        List<string> result=[];
        if(partner&&rules.Partner.Count>0)
            result.Add(rules.Partner.GetValueOrDefault(bonus,rules.Partner[""]).Replace("{trait}",rules.Traits.GetValueOrDefault(trait,trait),StringComparison.Ordinal));
        if(rules.Anomalies.TryGetValue(anomaly,out var anomalyNote))result.Add(anomalyNote);
        if(rules.PowerUps.TryGetValue(power,out var powerNote))result.Add(powerNote);
        if(rules.Forms.TryGetValue(line,out var forms)&&forms.TryGetValue(form,out var formNote))result.Add(formNote);
        return result.ToArray();
    }
    private static bool Truthy(JsonElement value)=>value.ValueKind switch
    {
        JsonValueKind.Undefined or JsonValueKind.Null or JsonValueKind.False=>false,
        JsonValueKind.String=>value.GetString()!.Length>0,JsonValueKind.Number=>value.GetDouble()!=0,_=>true
    };
    private static string JsString(JsonElement value)=>value.ValueKind switch
    {
        JsonValueKind.Undefined=>"undefined",JsonValueKind.Null=>"null",JsonValueKind.True=>"true",JsonValueKind.False=>"false",
        JsonValueKind.String=>value.GetString()!,JsonValueKind.Object=>"[object Object]",
        JsonValueKind.Array=>string.Join(",",value.EnumerateArray().Select(item=>item.ValueKind==JsonValueKind.Null?"":JsString(item))),_=>value.GetRawText()
    };
}
