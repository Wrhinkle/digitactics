using System.Text.Json;
using DigiTactics.Presentation;

internal static class AiEmoteChecks
{
    public static int Run(string root)
    {
        using var file=File.OpenRead(Path.Combine(root,"tests/fixtures/ai-emotes.json.gz"));
        using var gzip=new System.IO.Compression.GZipStream(file,System.IO.Compression.CompressionMode.Decompress);
        using var fixture=JsonDocument.Parse(gzip);int checks=0;
        foreach(var test in fixture.RootElement.EnumerateArray())
        {
            uint state=test.GetProperty("seed").GetUInt32();int calls=0;
            double Random(){calls++;state=unchecked(state*1664525+1013904223);return state/4294967296d;}
            var reactions=new AiEmoteReactions(Enumerable.Range(0,8));
            foreach(var round in test.GetProperty("rounds").EnumerateArray())
            {
                var players=round.GetProperty("players").EnumerateArray().Select(p=>new AiEmotePlayer(p.GetProperty("id").GetInt32(),p.GetProperty("name").GetString()!,p.GetProperty("isHuman").GetBoolean(),p.GetProperty("alive").GetBoolean())).ToArray();
                var results=round.GetProperty("results").EnumerateArray().Select(r=>new AiEmoteResult(r.GetProperty("a").GetInt32(),r.GetProperty("b").GetInt32(),r.GetProperty("pve").GetBoolean(),r.GetProperty("ghost").GetBoolean(),r.GetProperty("winner").GetInt32()));
                int before=calls;var actual=reactions.Choose(players,results,round.GetProperty("localSeat").GetInt32(),Random,p=>round.GetProperty("looks").GetProperty(p.Seat.ToString()).GetProperty("emote").EnumerateArray().Select(x=>x.ValueKind==JsonValueKind.Null?null:x.GetString()).ToArray());
                var expected=round.GetProperty("expected");
                void Check(bool ok,string why){checks++;if(!ok)throw new InvalidOperationException($"AI emotes seed {test.GetProperty("seed")}: {why}");}
                Check(calls-before==round.GetProperty("calls").GetInt32(),$"random draw count {calls-before} vs {round.GetProperty("calls")}");
                Check((actual is null)==(expected.ValueKind==JsonValueKind.Null),"reaction presence");
                if(actual is null)continue;
                Check(actual.Seat==expected.GetProperty("seat").GetInt32(),"seat");Check(actual.Id==expected.GetProperty("id").GetString(),"mood/loadout selection");Check(Math.Abs(actual.Delay-expected.GetProperty("delay").GetDouble())<1e-9,"delay");
            }
        }
        Console.WriteLine($"AI emotes: {checks:N0} original eligibility, elimination, chance, mood, ordering and delay checks passed.");return checks;
    }
}
