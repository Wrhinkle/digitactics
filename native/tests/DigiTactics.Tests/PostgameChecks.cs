using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

internal static class PostgameChecks
{
    public static int Run(string root, ContentCatalog catalog)
    {
        int checks=0; void Check(bool ok,string message) { checks++; if(!ok)throw new InvalidOperationException("Postgame: "+message); }
        using var fixture=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/postgame-progress.json")));
        using var colors=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"content/postgame/trait-colors.json")));
        foreach(var rules in catalog.Variants)foreach(var trait in rules.Traits)
            Check(colors.RootElement.GetProperty(rules.Id).GetProperty(trait.Id).GetString() is { Length:7 } color && color[0]=='#' && color.Skip(1).All(Uri.IsHexDigit),"source trait palette covers every ruleset");
        foreach(var sample in fixture.RootElement.GetProperty("levels").EnumerateArray())
        {
            var actual=TamerProgress.Read("{\"totals\":"+sample.GetProperty("totals").GetRawText()+"}"); var expected=sample.GetProperty("expected");
            Check(actual.Level==expected.GetProperty("level").GetInt32() && actual.Xp==expected.GetProperty("xp").GetInt64() && actual.Need==expected.GetProperty("need").GetInt64() && actual.Total==expected.GetProperty("total").GetInt64(),"original account level/XP");
        }
        foreach(var sample in fixture.RootElement.GetProperty("placements").EnumerateArray())
            Check(PostgameSummary.PlaceLabel(sample.GetProperty("place").GetInt32(),sample.GetProperty("duos").GetBoolean())==sample.GetProperty("expected").GetString(),"original solo/duo label");
        string client="{\"matches\":"+fixture.RootElement.GetProperty("matches").GetRawText()+"}";
        var day=DateOnly.Parse(fixture.RootElement.GetProperty("day").GetString()!,System.Globalization.CultureInfo.InvariantCulture);
        var progress=MatchRewards.Progress(null,client,day);
        foreach(var sample in fixture.RootElement.GetProperty("missions").EnumerateArray())
        {
            var mission=progress.Missions.Single(m=>m.Id==sample.GetProperty("id").GetString());
            Check(mission.Name==sample.GetProperty("name").GetString() && mission.Goal==sample.GetProperty("goal").GetInt32() && mission.Current==sample.GetProperty("current").GetInt32() && mission.Bits==sample.GetProperty("bits").GetInt32() && !mission.Done,"source mission selection, best single game and previous-day exclusion");
        }
        var first=progress.Missions[0]; string shop=JsonSerializer.Serialize(new{lastDay=day.ToString("yyyy-MM-dd"),missions=new{day=day.ToString("yyyy-MM-dd"),done=new[]{first.Id}}});
        var claimed=MatchRewards.Progress(shop,client,day);
        Check(claimed.Claimed && claimed.Missions[0].Done && claimed.Missions[0].Current==first.Goal,"claimed mission fills progress");
        Check(!MatchRewards.Progress(shop,client,day.AddDays(1)).Claimed,"new day resets bonus projection without writing save");
        foreach(var sample in fixture.RootElement.GetProperty("dailyCases").EnumerateArray())
        {
            var sampleDay=DateOnly.Parse(sample.GetProperty("day").GetString()!,System.Globalization.CultureInfo.InvariantCulture);
            var matches=JsonNode.Parse(sample.GetProperty("matches").GetRawText())!.AsArray();
            // Match JS local dates even when the fixture is consumed on a machine in another time zone.
            for(int i=0;i<matches.Count;i++)matches[i]!["at"]=new DateTimeOffset(sampleDay.AddDays(i==2?-1:0).ToDateTime(new TimeOnly(12,0),DateTimeKind.Local)).ToUnixTimeMilliseconds();
            var actual=MatchRewards.Progress(null,"{\"matches\":"+matches.ToJsonString()+"}",sampleDay);
            int index=0;
            foreach(var expected in sample.GetProperty("missions").EnumerateArray())
            {
                var mission=actual.Missions[index++];
                Check(mission.Id==expected.GetProperty("id").GetString() && mission.Current==expected.GetProperty("current").GetInt32() && mission.Goal==expected.GetProperty("goal").GetInt32(),"month of original daily selections and progress");
            }
        }
        foreach(var sample in fixture.RootElement.GetProperty("counts").EnumerateArray())
            Check(PostgameAwardCount.Value(sample.GetProperty("n").GetInt32(),sample.GetProperty("k").GetDouble(),0,1)==sample.GetProperty("value").GetInt32(),"original cubic Bits count-up");
        Check(PostgameAwardCount.Line(120,0,.34)==0 && PostgameAwardCount.Line(120,0,.73)==120 && PostgameAwardCount.Line(120,1,.73)==0,"sequential line timing");
        Check(PostgameAwardCount.Total(200,2,1.11)==0 && PostgameAwardCount.Total(200,2,1.82)==200 && PostgameAwardCount.CompletedRows(2,1.82)==3,"total follows completed lines");
        string directory=Path.Combine(Path.GetTempPath(),"digitactics-postgame-check-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var session=new MatchPlanningSession(catalog.Variants[0],143,Enumerable.Range(0,8).Select(i=>new MatchSeat("Tamer "+i,i==0)).ToArray());
            var profile=new NativeProfileStore(directory); var recorder=new MatchHistoryRecorder(session,profile);
            session.State.Players[0].Health=-10000; session.StartCombat(); recorder.Capture(); session.FinishCombat();
            using(var guard=new FileStream(Path.Combine(directory,"profile.json.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None))
                Check(!recorder.Complete(DateTimeOffset.Now.ToUnixTimeMilliseconds()) && recorder.XpGained==0 && !recorder.FreshAward,"failed transaction does not display earned XP");
            Check(recorder.Complete(DateTimeOffset.Now.ToUnixTimeMilliseconds()) && recorder.Match is not null && recorder.XpGained==100 && recorder.FreshAward,"successful transaction supplies frozen composition and XP");
            string before=File.ReadAllText(Path.Combine(directory,"profile.json"));
            Check(recorder.Complete(DateTimeOffset.Now.ToUnixTimeMilliseconds()) && recorder.XpGained==100 && before==File.ReadAllText(Path.Combine(directory,"profile.json")),"redrawing result preserves earned XP and does not pay again");
            var reopened=new MatchHistoryRecorder(session,new NativeProfileStore(directory));
            Check(reopened.Complete(DateTimeOffset.Now.ToUnixTimeMilliseconds()) && reopened.XpGained==0 && !reopened.FreshAward && before==File.ReadAllText(Path.Combine(directory,"profile.json")),"replay reports zero new XP and never awards twice");
        }
        finally
        {
            string resolved=Path.GetFullPath(directory);
            if(!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-postgame-check-",StringComparison.Ordinal))throw new InvalidOperationException("Unsafe test cleanup target.");
            Directory.Delete(resolved,true);
        }
        Console.WriteLine($"Postgame progress: {checks} source parity and transaction checks passed."); return checks;
    }
}
