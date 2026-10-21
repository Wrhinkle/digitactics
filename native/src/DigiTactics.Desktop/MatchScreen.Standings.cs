using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly MatchHudProjection matchHud=new();
    internal int StandingAvatarCount {get;private set;}
    internal int? StandingTooltipSeat {get;private set;}
    internal (float2 Min,float2 Max) StandingTooltipBounds {get;private set;}
    private float standingTooltipHeight=220;
    internal IReadOnlyList<HudRound> StandingRecent(int seat)=>matchHud.Recent(seat);
    internal string MatchReadyLabel=>networkReplica?.Match is {} online?MatchHudProjection.ReadyLabel(Session!.State,online.Votes,localSeat):"Fight!";
    private void MatchHudBuilding(LockstepMatch match)=>matchHud.Reset(match.Session);
    private void MatchHudAdvanced(LockstepMatch match,JsonElement entry,bool replaying)
    {if(entry.GetProperty("k"u8).GetString()=="end")matchHud.Capture(match.Session);}
    private static ImColor8 RoundOutcomeColor(string outcome)=>outcome=="w"?new(111,224,138):outcome=="l"?new(255,138,122):new(160,173,194);

    private (MatchPlayer Player,bool Ghost)? StandingOpponent(MatchPlayer player)
    {
        if(Session!.Rounds.RoundType!="pvp")return null;
        int id=Session.State.Duos?player.Team!.Value:player.Id;
        var pair=Session.Rounds.Pairs.FirstOrDefault(p=>p.A==id||!p.Ghost&&p.B==id);
        if(pair is null)return null;
        int other=pair.A==id?pair.B:pair.A;
        return(Session.State.Duos?Session.State.Teams[other].Members[0]:Session.State.Players[other],pair.Ghost&&pair.A==id);
    }
    // ui.js avatarHTML: the local Tamer's Little Legend; AI Tamers show their most valuable unit.
    private bool DrawStandingAvatar(ImDrawListPtr draw,MatchPlayer player,float2 at,float size)
    {
        var legend=ResultCompanion(player);
        if(player.IsHuman&&legend is not null)
        {
            HudPortraitTile(draw,at,size,HudHex("8a6d1f"),6);
            return portraits.DrawCompanion(legend,at,size);
        }
        var units=player.Roster.Board.Select(e=>e.Unit).ToArray();
        if(units.Length==0)units=player.Roster.Entries.Select(e=>e.Unit).ToArray();
        var lines=Rules.Lines.Concat(Rules.ExtraLines).ToDictionary(l=>l.Id);
        double Value(PlanningUnit unit)=>lines[unit.Line].Tier*Math.Pow(lines[unit.Line].Copies,unit.Stage)+unit.Items.Length*.5;
        var best=units.OrderByDescending(Value).FirstOrDefault();
        if(best is null)
        {
            draw.AddRectFilled(at,at+new float2(size,size),HudHex("555566"),6);
            HudRunCentered(draw,HudWeight(800),11,at.X+size/2,at.Y,size,new string(player.Name.Where(char.IsLetter).Take(2).ToArray()),new ImColor8(255,255,255,217));
            return legend is not null;
        }
        HudPortraitTile(draw,at,size,FamilyColor(best.Line),6);
        portraits.Draw(Rules.Id,best.Line,best.Stage,at,size);
        return true;
    }
    // #players: .ph, then .pl rows (34px avatar, name and tags, HP bar, level, streak and pips, HP on the right).
    private void StandingsCards()
    {
        StandingAvatarCount=0;StandingTooltipSeat=null;
        var state=Session!.State;
        int alive=state.Duos?state.Teams.Count(t=>t.Alive):state.Players.Count(p=>p.Alive);
        var header=ImGui.GetCursorScreenPos();float railWidth=ImGui.GetContentRegionAvail().X;
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(0,0));
        ImGui.Dummy(new(railWidth,MathF.Round(HudSectionHeader(ImGui.GetWindowDrawList(),header.X,header.Y,railWidth,state.Duos?"Teams":"Tamers",$"{alive} left"))));
        var opponent=StandingOpponent(state.Players[viewed]);
        EnsureSoundboard();var sounds=soundboard.Labels(Environment.TickCount64);
        // .hpv small: the HP each Tamer lost in the last results, shown outside combat.
        var lastDamage=new Dictionary<int,int>();
        string? lastRound=LastResolvedRound();
        if(Session.Rounds.Phase!=MatchPhase.Combat&&lastRound is not null)
            foreach(var recent in state.Players)if(matchHud.Recent(recent.Id).LastOrDefault() is {} last&&last.Round==lastRound&&last.Damage>0)lastDamage[recent.Id]=last.Damage;
        bool compact=HudCompact;float avatar=compact?30:34,rowHeight=avatar+8+2;
        foreach(var seat in ScoutOrder())
        {
            var player=state.Players[seat.Id];float width=ImGui.GetContentRegionAvail().X;
            var at=HudSnap(ImGui.GetCursorScreenPos());var end=at+new float2(width,rowHeight);ImGui.SetCursorScreenPos(at);
            bool pressed=ImGui.InvisibleButton("view/"+player.Id,new(width,rowHeight));
            Buttons["view/"+player.Id]=(at+end)*.5f;
            var draw=ImGui.GetWindowDrawList();
            bool hovered=ImGui.IsItemHovered();bool own=player.Id==localSeat;
            bool ready=Session.Rounds.Phase==MatchPhase.Planning&&networkReplica?.Match?.Votes.Contains(player.Id)==true;
            bool next=opponent is {} rival&&(state.Duos?player.Team==rival.Player.Team:player.Id==rival.Player.Id);
            bool me=player.Id==viewed;
            draw.AddRectFilled(at,end,me?HudHex("1d1f22"):CssPanel,8);
            draw.AddRect(at,end,next?CssEnemy:me?CssGold:CssLine,8);
            if(next)draw.AddRect(at+new float2(1,1),end-new float2(1,1),CssEnemy,7);
            if(DrawStandingAvatar(draw,player,at+new float2(5,5),avatar))StandingAvatarCount++;
            // .hpv: 800 17px, the last loss in 10px/700 red underneath.
            string health=player.Alive?Math.Max(0,player.Health).ToString():"#"+player.Placement;
            float right=end.X-1-8,center=at.Y+rowHeight/2;
            lastDamage.TryGetValue(player.Id,out int lost);bool showLost=player.Alive&&lost>0;
            float hpTop=center-(showLost?17+2+13.5f:17)/2;
            float hpWidth=HudRunRight(draw,HudWeight(800),17,right,hpTop,17,health,CssText);
            if(showLost)hpWidth=Math.Max(hpWidth,HudRunRight(draw,HudWeight(700),10,right,hpTop+19,13.5f,$"-{lost}",CssBad));
            float x=at.X+1+4+avatar+8,column=right-hpWidth-8-x;
            float rowsTop=center-(17.55f+14.85f)/2;
            // .nm: 13px/700 name, then tags (9px/800, .5px tracking).
            var tags=new List<(string Label,Rgb Fill,Rgb Ink)>();
            if(state.Duos)tags.Add(($"T{player.Team+1}",CssPanel,CssText));
            if(own)tags.Add(("YOU",CssGold,CssInk));
            else if(networkRoom is not null)tags.Add(player.IsHuman?("PLAYER",HudHex("1e5a8a"),HudHex("cfe9ff")):("AI",HudHex("2c3446"),HudHex("9aa4b8")));
            if(next)tags.Add((Session.Rounds.Phase==MatchPhase.Planning?"NEXT":"VS",CssEnemy,HudHex("ffffff")));
            if(ready)tags.Add(("READY",HudHex("1e6a3e"),HudHex("c8ffd9")));
            float tagsWidth=tags.Sum(t=>HudWidth(HudWeight(800),9,t.Label,.5f)+8+5);
            float nameWidth=HudRun(draw,HudWeight(700),13,x,rowsTop,17.55f,player.Name,CssText,Math.Max(20,column-tagsWidth));
            float tx=x+nameWidth+5;
            foreach(var tag in tags)
            {
                float tagWidth=HudWidth(HudWeight(800),9,tag.Label,.5f)+8;var tagTop=rowsTop+(17.55f-12.15f)/2;
                draw.AddRectFilled(new(tx,tagTop),new(tx+tagWidth,tagTop+12.15f),tag.Fill,3);
                HudRun(draw,HudWeight(800),9,tx+4,tagTop,12.15f,tag.Label,tag.Ink,0,.5f);
                tx+=tagWidth+5;
            }
            // .pm: HP bar, level, streak and the last five PvP results.
            float pmTop=rowsTop+17.55f,pmEnd=x+column;
            var recent=matchHud.Recent(player.Id).Where(r=>!r.Pve).TakeLast(5).ToArray();
            int streak=player.Economy.Streak;string streakLabel=streak>1?$"W{streak}":streak< -1?$"L{-streak}":"";
            float pipsWidth=recent.Length>0?recent.Length*7-2:0;
            float trailing=HudWidth(HudWeight(400),11,"Lv "+player.Economy.Level)+(streakLabel.Length>0?6+HudWidth(HudWeight(800),11,streakLabel):0)+(pipsWidth>0?6+pipsWidth:0);
            float barWidth=Math.Max(30,column-trailing-6);
            if(player.Alive)
            {
                float hp=Math.Clamp((float)Math.Max(0,player.Health)/(state.Duos?Rules.Duo!.TeamHp:Rules.Economy!.StartHp),0,1);
                var bar=new float2(x,pmTop+(14.85f-5)/2);
                draw.AddRectFilled(bar,bar+new float2(barWidth,5),HudHex("262e40"),3);
                if(hp>0)draw.AddRectFilled(bar,bar+new float2(barWidth*hp,5),hp<=.25f?CssBad:hp<=.5f?HudHex("e6c04a"):CssGood,3);
            }
            else barWidth=HudRun(draw,HudWeight(400),11,x,pmTop,14.85f,"out",CssMuted);
            float px=x+barWidth+6;
            px+=HudRun(draw,HudWeight(400),11,px,pmTop,14.85f,"Lv "+player.Economy.Level,CssMuted)+6;
            if(streakLabel.Length>0)px+=HudRun(draw,HudWeight(800),11,px,pmTop,14.85f,streakLabel,streak>0?HudHex("ff9a4a"):HudHex("6fc3ff"))+6;
            for(int i=0;i<recent.Length;i++)
            {
                var pip=new float2(px+i*7,pmTop+(14.85f-9)/2);
                draw.AddRectFilled(pip,pip+new float2(5,9),recent[i].Outcome=="w"?CssGood:recent[i].Outcome=="l"?CssBad:HudHex("3a4356"),1);
            }
            // .pl.dead { opacity: .38 }
            if(!player.Alive)draw.AddRectFilled(at,end,HudAlpha(CssRail,.62f),8);
            if(hovered)StandingTooltip(player,at);
            if(pressed)Watch(player.Id);
            if(sounds.FirstOrDefault(label=>label.Seat==player.Id) is {} clip)UiText.Wrapped("Playing: "+DigiTactics.Storage.CosmeticCollection.Item("sound",clip.Id)!.Name);
            EmoteRow(player.Id);
            ImGui.Dummy(new(width,3));
        }
        ImGui.PopStyleVar();
    }
    private string? LastResolvedRound()
    {
        static (int,int) Order(string round){var parts=round.Split('-');return(parts.Length>0&&int.TryParse(parts[0],out int s)?s:0,parts.Length>1&&int.TryParse(parts[1],out int r)?r:0);}
        return Session!.State.Players.Select(p=>matchHud.Recent(p.Id).LastOrDefault()?.Round).OfType<string>().MaxBy(Order);
    }
    internal int RoundHistoryCards {get;private set;}
    // #history: .ph "Your rounds PvP w-l", then 30px .hc cells (dashed for PvE).
    private void RoundHistoryStrip()
    {
        var rounds=matchHud.Recent(viewed);RoundHistoryCards=0;
        if(rounds.Count==0)return;
        ImGui.Dummy(new(0,10-ImGui.GetStyle().ItemSpacing.Y));
        var header=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(3,3));
        ImGui.Dummy(new(width,MathF.Round(HudSectionHeader(ImGui.GetWindowDrawList(),header.X,header.Y,width,"Your rounds",$"PvP {rounds.Count(r=>!r.Pve&&r.Outcome=="w")}-{rounds.Count(r=>!r.Pve&&r.Outcome=="l")}"))-3));
        int columns=Math.Max(1,(int)((width+3)/33));
        foreach(var round in rounds.TakeLast(14))
        {
            if(RoundHistoryCards%columns!=0)ImGui.SameLine(0,3);
            var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(30,30));var draw=ImGui.GetWindowDrawList();
            var (fill,border,ink)=round.Outcome=="w"?(HudHex("1e4a33"),HudHex("2f7a52"),HudHex("bff5d3")):round.Outcome=="l"?(HudHex("4d1f27"),HudHex("8a3340"),HudHex("ffc9cf")):(CssPanel2,CssLine2,CssMuted);
            draw.AddRectFilled(at,at+new float2(30,30),fill,5);
            if(round.Pve)HudDashedRect(draw,at,at+new float2(30,30),border,1,3,2);
            else draw.AddRect(at,at+new float2(30,30),border,5);
            float top=at.Y+(30-10.5f-9.45f)/2;
            HudRunCentered(draw,HudWeight(800),10,at.X+15,top,10.5f,round.Round,ink);
            HudRunCentered(draw,HudWeight(600),9,at.X+15,top+10.5f,9.45f,round.Outcome.ToUpperInvariant()+(round.Damage>0?$" -{round.Damage}":""),ink);
            if(ImGui.IsItemHovered())ResultTooltip($"{round.Round} {(round.Pve?"Wild Digimon":"vs "+round.Opponent)}: {(round.Outcome=="w"?"won":round.Outcome=="l"?"lost":"draw")}"+(round.Damage>0?$", -{round.Damage} HP":""));
            RoundHistoryCards++;
        }
        ImGui.PopStyleVar();
    }
    private void StandingTooltip(MatchPlayer player,float2 standingAt)
    {
        StandingTooltipSeat=player.Id;
        float width=Math.Min(390,Math.Max(200,standingAt.X-16));
        ImGui.SetNextWindowPos(new(Math.Max(8,standingAt.X-width-8),Math.Clamp(standingAt.Y,8,Math.Max(8,size.Y-standingTooltipHeight-8))));
        ImGui.SetNextWindowSize(new(width,0));ImGui.BeginTooltip();
        var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(52,52));portraits.DrawCompanion(ResultCompanion(player),at,52);
        ImGui.SameLine();ImGui.BeginGroup();HudText(player.Name,22,HudGold);
        HudText((player.Alive?$"{Math.Max(0,player.Health)} HP / Level {player.Economy.Level}":$"Placed #{player.Placement}")+" / "+(player.IsHuman?"Player":"AI"));ImGui.EndGroup();
        if(Session!.State.Duos)HudText("Team "+(player.Team+1)+" / Partner: "+Session.State.Teams[player.Team!.Value].Members.First(p=>p.Id!=player.Id).Name);
        var board=Session.CombatProjection.Board(player);var counts=TraitRules.Count(Rules,board,player.Augments.ToArray());
        var active=counts.Where(pair=>Rules.Traits.Any(t=>t.Id==pair.Key&&TraitRules.Level(t,pair.Value)>0)).Select(pair=>$"{pair.Value} {Name(pair.Key)}").ToArray();
        if(active.Length>0)HudText(string.Join(" / ",active),16,HudGold);
        var lines=Rules.Lines.Concat(Rules.ExtraLines).ToDictionary(l=>l.Id);
        var units=player.Roster.Board.Select(e=>e.Unit).OrderByDescending(u=>lines[u.Line].Tier+u.Stage*2).ToArray();
        if(units.Length>0)HudText("Board: "+string.Join(", ",units.Select(u=>lines[u.Line].Forms[u.Stage]+" "+new string('*',u.Stage+1)+(u.Items.Length>0?$" ({u.Items.Length} items)":""))));
        HudText("Augments: "+(player.Augments.Count==0?"none yet":string.Join(", ",player.Augments.Select(Name))));
        foreach(var round in matchHud.Recent(player.Id).TakeLast(6))
            HudText($"{round.Round} / {(round.Outcome=="w"?"Won":round.Outcome=="l"?"Lost":"Draw")} vs {round.Opponent}"+(round.Damage>0?$" / -{round.Damage} HP":""),15,RoundOutcomeColor(round.Outcome));
        standingTooltipHeight=ImGui.GetWindowSize().Y;
        StandingTooltipBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
        ImGui.EndTooltip();
    }
}
