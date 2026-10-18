using System.Text.Json;
using DigiTactics.Storage;

internal static class LiveMissionChecks
{
    internal static void Run()
    {
        int checks=0;
        void Check(bool okay,string message){checks++;if(!okay)throw new InvalidOperationException("Live mission display: "+message);}
        var observed=new HashSet<string>();
        // Source shop.js uses current-match counters, not the client's best completed match today.
        var expected=new Dictionary<string,int>{{"win3",2},{"win6",2},{"lvl7",7},{"star3",3},{"top4",0},{"top2",0},{"kill1",1}};
        var initial=new Dictionary<string,int>{{"win3",0},{"win6",0},{"lvl7",0},{"star3",1},{"top4",0},{"top2",0},{"kill1",0}};
        for(int i=0;i<366;i++)
        {
            var day=new DateOnly(2026,1,1).AddDays(i);
            var live=MatchRewards.LiveProgress(null,new(2,1,3,9,0),day);
            var fresh=MatchRewards.LiveProgress(null,null,day);
            Check(live.Missions.Select(m=>m.Id).SequenceEqual(MatchRewards.Progress(null,null,day).Missions.Select(m=>m.Id)),"rotation differs from daily rewards");
            foreach(var mission in live.Missions)
            {
                observed.Add(mission.Id);Check(mission.Current==expected[mission.Id]&&!mission.Done,"current game progress or clamp differs");
                Check(fresh.Missions.Single(m=>m.Id==mission.Id).Current==initial[mission.Id],"no-match fallback differs from source statsOf");
            }
            string shop=JsonSerializer.Serialize(new{lastDay=day.ToString("yyyy-MM-dd"),missions=new{day=day.ToString("yyyy-MM-dd"),done=live.Missions.Select(m=>m.Id).ToArray()}});
            var completed=MatchRewards.LiveProgress(shop,new(0,0,1,0,0),day);
            Check(completed.Claimed&&completed.Missions.All(m=>m.Done&&m.Current==m.Goal),"saved completion lost to a fresh match");
            var tomorrow=MatchRewards.LiveProgress(shop,new(0,0,1,0,0),day.AddDays(1));
            Check(!tomorrow.Claimed&&tomorrow.Missions.All(m=>!m.Done),"midnight failed to reset the daily mission display");
        }
        Check(observed.SetEquals(expected.Keys),"year did not exercise all seven source missions");
        Console.WriteLine($"Live missions: {checks} current-match, source default, daily rotation, completion and midnight checks passed.");
    }
}
