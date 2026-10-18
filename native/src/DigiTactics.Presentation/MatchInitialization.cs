using System.Text.Json;
using DigiTactics.Networking;
using DigiTactics.Rules;

namespace DigiTactics.Presentation;

/// <summary>The host publishes resolved choices. Receivers never reroll Chaos using their own registry.</summary>
public static class MatchInitialization
{
    public static JsonElement Host(SetSelection choices,string? set,string? pace,uint seed,IReadOnlyList<MatchSeat> seats,bool duos,int planningSeconds,long at)
    {
        var resolved=choices.Resolve(set,SetSelection.LobbyPace(pace),seed);
        var init=JsonSerializer.SerializeToElement(new {seed,at,slots=seats.Select(s=>new{name=s.Name,human=s.Human}),
            setId=resolved.SetId,pace=resolved.Pace,chaos=resolved.Chaos,chaosPace=resolved.ChaosPace,
            format=duos?"duos":"solo",teams=new[]{new[]{0,1},new[]{2,3},new[]{4,5},new[]{6,7}},planTime=planningSeconds});
        if(!NetworkProtocol.Init(init))throw new InvalidDataException("Invalid multiplayer match initialization.");
        return init;
    }
    public static LockstepMatch Load(ContentCatalog catalog,JsonElement init)
    {
        if(!NetworkProtocol.Init(init))throw new InvalidDataException("Invalid multiplayer match initialization.");
        bool Flag(string name)=>init.TryGetProperty(name,out var value)&&value.ValueKind==JsonValueKind.True;
        var selection=new ResolvedSet(init.GetProperty("setId").GetString()!,init.GetProperty("pace").GetString()!,Flag("chaos"),Flag("chaosPace"));
        var seats=init.GetProperty("slots").EnumerateArray().Select(s=>new MatchSeat(s.GetProperty("name").GetString()!,s.GetProperty("human").GetBoolean())).ToArray();
        int[][]? teams=init.TryGetProperty("teams",out var teamData)&&teamData.ValueKind==JsonValueKind.Array
            ?teamData.EnumerateArray().Select(t=>t.EnumerateArray().Select(s=>(int)s.GetDouble()).ToArray()).ToArray():null;
        return new(SetSelection.Rules(catalog,selection),(uint)init.GetProperty("seed").GetDouble(),seats,init.GetProperty("planTime").GetDouble(),(long)init.GetProperty("at").GetDouble(),
            init.TryGetProperty("format",out var format)&&format.ValueKind==JsonValueKind.String&&format.GetString()=="duos",teams,selection.Chaos,selection.ChaosPace);
    }
}
