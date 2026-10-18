using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO.Compression;
using DigiTactics.Presentation;

internal static class MatchKeyboardChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/match-keyboard.json.gz"));
        using var gzip=new GZipStream(file,CompressionMode.Decompress);
        using var source=JsonDocument.Parse(gzip);
        var json=new JsonSerializerOptions{PropertyNameCaseInsensitive=true,PropertyNamingPolicy=JsonNamingPolicy.CamelCase};
        int checks=0;
        void Check(bool okay,string message){if(!okay)throw new InvalidOperationException(message);checks++;}
        foreach(var row in source.RootElement.EnumerateArray())
        {
            var context=row.GetProperty("context").Deserialize<MatchKeyContext>(json)!;
            var action=MatchKeyboard.Resolve(row.GetProperty("key").GetString()!,context);
            Check(JsonNode.DeepEquals(JsonSerializer.SerializeToNode(action,json),JsonNode.Parse(row.GetProperty("expected").GetRawText())),"Match keyboard differs from original source at case "+checks+": "+row.GetRawText());
        }
        ScoutSeat[] seats=[new(0,false,0,8,0),new(1,true,80,0,0),new(2,true,80,0,1),new(3,false,0,2,1)];
        var sample=new MatchKeyContext(seats,0,1,true,false,true);
        Check(MatchKeyboard.Order(seats,true).Select(p=>p.Id).SequenceEqual([1,2,3,0]),"Standings order must include placement/team/seat tie breaks.");
        Check(MatchKeyboard.Resolve("q",sample)?.Seat==0,"Scouting cycle must retain an eliminated local seat for return.");
        Check(MatchKeyboard.Resolve("e",sample)?.Seat==2,"Scouting cycle must skip eliminated rivals.");
        Check(MatchKeyboard.Resolve("d",sample) is null,"Eliminated players cannot buy through shortcuts.");
        foreach(string key in new[]{"d","f","e","q","1"," ","escape","r","+","-"})
        {
            Check(MatchKeyboard.Resolve(key,sample with{Modified=true}) is null,"Modified keys must not activate gameplay.");
            Check(MatchKeyboard.Resolve(key,sample with{TextInput=true}) is null,"Text entry must own keyboard input.");
            Check(MatchKeyboard.Resolve(key,sample with{Blocked=true}) is null,"An overlay must own keyboard input.");
        }
        foreach(string action in new[]{"buy","sell","equip","wisp","move"})Check(MatchKeyboard.RequiresOwnBoard(action),"Scouting allowed a board mutation: "+action);
        foreach(string action in new[]{"reroll","buyXp","lock"})Check(!MatchKeyboard.RequiresOwnBoard(action),"Scouting incorrectly blocked global planning control: "+action);
        Console.WriteLine($"Match keyboard: {checks} original capture/bubble projections, scouting order, modifier/text/overlay guards and own-board checks passed.");return checks;
    }
}
