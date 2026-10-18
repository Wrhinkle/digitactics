using System.Globalization;
using System.Text.Json;

namespace DigiTactics.Rules;

public sealed record SetChoice(string Id,string Name,string Description,string Mode,string NativePace,bool Hidden,bool ChaosEligible);
public sealed record ResolvedSet(string SetId,string Pace,bool Chaos,bool ChaosPace)
{
    public string Variant=>SetId+"/"+Pace;
}

/// <summary>Original set registry and seed-only selection. These hashes never advance gameplay RNG.</summary>
public sealed class SetSelection
{
    private sealed record Document(int SchemaVersion,string DefaultId,SetChoice[] Sets,string[] PickableIds);
    private readonly Dictionary<string,SetChoice> definitions;
    private readonly string[] chaosPool;
    public string DefaultId {get;}
    public IReadOnlyList<SetChoice> Options {get;}
    public IReadOnlyList<string> ChaosPool=>Array.AsReadOnly(chaosPool);
    public bool HasChaos=>Options.Count>1;
    public SetSelection(string defaultId,IEnumerable<SetChoice> sets,IEnumerable<string> pickableIds)
    {
        DefaultId=defaultId;definitions=sets.ToDictionary(s=>s.Id,StringComparer.Ordinal);
        if(!definitions.ContainsKey(defaultId) || definitions.Values.Any(s=>s.NativePace is not ("normal" or "hyper")))throw new InvalidDataException("Invalid set selection definitions.");
        var ids=pickableIds.ToArray();
        if(ids.Length==0 || ids.Distinct(StringComparer.Ordinal).Count()!=ids.Length || ids.Any(id=>!definitions.TryGetValue(id,out var choice) || choice.Hidden))throw new InvalidDataException("Invalid pickable set order.");
        Options=Array.AsReadOnly(ids.Select(id=>definitions[id]).ToArray());
        chaosPool=definitions.Values.Where(s=>!s.Hidden && s.ChaosEligible).Select(s=>s.Id).Order(StringComparer.Ordinal).ToArray();
    }
    public static SetSelection Load(string path)
    {
        var document=JsonSerializer.Deserialize<Document>(File.ReadAllBytes(path),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})??throw new InvalidDataException("Missing set selection definitions.");
        if(document.SchemaVersion!=1)throw new InvalidDataException("Unsupported set selection definitions.");
        return new(document.DefaultId,document.Sets,document.PickableIds);
    }
    public bool Pickable(string? id)=>id=="chaos"?HasChaos:Options.Any(s=>s.Id==id);
    public string SavedSet(string? value)=>Pickable(value)?value!:DefaultId;
    public static string SavedPace(string? value)=>value is "hyper" or "normal"?value:"hyper";
    public static string SavedFormat(string? value)=>value=="duos"?"duos":"solo";
    public static string LobbyPace(string? value)=>value is "hyper" or "normal" or "chaos"?value:"hyper";
    public string Label(string? id)=>id=="chaos"?"Chaos":definitions.TryGetValue(id??"",out var definition)?definition.Name:id??DefaultId;
    public static string PaceLabel(string? pace)=>pace switch{"normal"=>"Normal","chaos"=>"Chaos pace",_=>"Hyper Roll"};
    public string ResolveSet(string? id,uint seed)
    {
        if(id=="chaos")return chaosPool.Length==0?DefaultId:chaosPool[Hash(seed.ToString(CultureInfo.InvariantCulture),0x811c9dc5)%chaosPool.Length];
        return id is not null && definitions.ContainsKey(id)?id:DefaultId;
    }
    public static string? ResolvePace(string? pace,uint seed)=>pace=="chaos"
        ? (Hash("p"+seed.ToString(CultureInfo.InvariantCulture),0x9e3779b9)%2==0?"hyper":"normal")
        : pace is "hyper" or "normal"?pace:null;
    public ResolvedSet Resolve(string? set,string? pace,uint seed)
    {
        string resolved=ResolveSet(set,seed);
        return new(resolved,ResolvePace(pace,seed)??definitions[resolved].NativePace,set=="chaos",pace=="chaos");
    }
    public static Ruleset Rules(ContentCatalog catalog,ResolvedSet selection)=>catalog.Variants.SingleOrDefault(r=>r.Id==selection.Variant)
        ??throw new InvalidDataException("This build does not contain the selected ruleset: "+selection.Variant);
    private static uint Hash(string value,uint hash)
    {foreach(char c in value)hash=unchecked((hash^c)*0x01000193);return hash;}
}
