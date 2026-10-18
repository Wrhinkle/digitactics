using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class MatchHudProjectionChecks
{
    internal static void Run(string root)
    {
        int checks=0;
        void Check(bool okay,string message){checks++;if(!okay)throw new InvalidOperationException("Match HUD: "+message);}
        var catalog=ContentCatalog.Load(Path.Combine(root,"content/catalog.json"));
        var seats=Enumerable.Range(0,8).Select(i=>new MatchSeat("Tamer "+i,i<2)).ToArray();
        var rules=catalog.Variants.First(r=>r.Id=="faction-wars/hyper");
        var session=new MatchPlanningSession(rules,143,seats);
        var hud=new MatchHudProjection();hud.Reset(session);
        Check(MatchHudProjection.ReadyLabel(session.State,new HashSet<int>(),0)=="Ready (0/2)","Empty vote label is wrong.");
        Check(MatchHudProjection.ReadyLabel(session.State,new HashSet<int>{0,3},0)=="Ready 1/2","AI votes must not inflate the ready count.");
        Check(MatchHudProjection.ReadyLabel(session.State,new HashSet<int>{0},1)=="Ready (1/2)","Other player's vote must not mark the local seat ready.");
        foreach(var variant in catalog.Variants)
        {
            var game=new MatchPlanningSession(variant,143,seats);
            var timeline=hud.Timeline(game,0);
            Check(timeline.Length==variant.Schedule!.RoundsInStage(1),variant.Id+" opening timeline length.");
            Check(timeline.Count(r=>r.Current)==1&&timeline[0].Current,variant.Id+" current marker.");
            for(int i=0;i<timeline.Length;i++)Check(timeline[i].Kind==variant.Schedule.TypeOf(new(1,i+1))&&timeline[i].Offer==game.Offers.At(new(1,i+1))?.Kind,variant.Id+" schedule/offer projection.");
        }
        if(session.Units.Carousel.Active){session.StartCombat();session.FinishCombat();session.Advance();}
        string round=session.Rounds.Position.Label;session.StartCombat();session.FinishCombat();hud.Capture(session);hud.Capture(session);
        foreach(var player in session.State.Players)
        {
            var recent=hud.Recent(player.Id);Check(recent.Count==1&&recent[0].Round==round,"Settlement must be recorded exactly once for every PvE seat.");
            var result=session.Rounds.Results.Single(r=>r.Match.A==player.Id);
            Check(recent[0].Pve&&recent[0].Opponent=="Wild Digimon"&&recent[0].Damage==result.DamageA,"PvE summary must include actual damage and opponent.");
            Check(recent[0].Outcome==(result.AWon?"w":result.BWon?"l":"d"),"Round outcome mismatch.");
        }
        Check(hud.Timeline(session,0).Single(r=>r.Round==round).Outcome==hud.Recent(0)[0].Outcome,"Timeline must share recorded result.");
        hud.Reset(session);Check(hud.Recent(0).Count==0,"New/rebuilt match must reset prior round records.");
        foreach(bool duo in new[]{false,true})
        {
            var game=new MatchPlanningSession(rules,145,duo?seats:seats.Take(3).ToArray(),duo);
            var history=new MatchHudProjection();history.Reset(game);
            while(game.Rounds.RoundType!="pvp")
            {
                foreach(var player in game.State.Players)player.Health=10000;
                game.StartCombat();game.FinishCombat();history.Capture(game);game.Advance();
            }
            foreach(var player in game.State.Players)player.Health=10000;
            string label=game.Rounds.Position.Label;game.StartCombat();game.FinishCombat();history.Capture(game);
            Check(duo||game.Rounds.Results.Any(r=>r.Match.Ghost),"Odd-player fixture must contain a real ghost matchup.");
            foreach(var player in game.State.Players)
            {
                var result=game.Rounds.Results.First(r=>r.Match.TeamA is {} ta?player.Team==ta||!r.Match.Ghost&&player.Team==r.Match.TeamB:r.Match.A==player.Id||!r.Match.Ghost&&r.Match.B==player.Id);
                bool sideA=result.Match.TeamA is {} team?player.Team==team:result.Match.A==player.Id;
                var record=history.Recent(player.Id).Where(r=>r.Round==label).ToArray();
                Check(record.Length==1,"Every Tamer must get exactly one result, including a ghost copy target and duo partners.");
                Check(!record[0].Pve&&record[0].Outcome==(result.Match.Winner==-1?"d":result.Match.Winner==(sideA?0:1)?"w":"l"),"Ghost/duo history used the wrong battle side.");
            }
        }
        Console.WriteLine($"Match HUD projection: {checks} schedule, offer, ready-vote, settlement and reset checks passed.");
    }
}
