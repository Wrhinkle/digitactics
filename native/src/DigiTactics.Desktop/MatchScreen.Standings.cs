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
    internal string MatchReadyLabel=>networkReplica?.Match is {} online?MatchHudProjection.ReadyLabel(Session!.State,online.Votes,localSeat):"FIGHT!";
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
    private void StandingsCards()
    {
        StandingAvatarCount=0;StandingTooltipSeat=null;
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(8,2));
        float rowHeight=Height<460?44:52;
        var state=Session!.State;
        int alive=state.Duos?state.Teams.Count(t=>t.Alive):state.Players.Count(p=>p.Alive);
        HudText(state.Duos?"TEAMS":"TAMERS",15,HudMuted);ImGui.SameLine();HudText($"{alive} left",13,HudMuted);
        var opponent=StandingOpponent(state.Players[viewed]);
        EnsureSoundboard();var sounds=soundboard.Labels(Environment.TickCount64);
        foreach(var seat in ScoutOrder())
        {
            var player=state.Players[seat.Id];float width=ImGui.GetContentRegionAvail().X;
            bool pressed=HudButton("view/"+player.Id,"",width,rowHeight);
            var at=ImGui.GetItemRectMin();var end=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
            bool hovered=ImGui.IsItemHovered();bool own=player.Id==localSeat;
            bool ready=Session.Rounds.Phase==MatchPhase.Planning&&networkReplica?.Match?.Votes.Contains(player.Id)==true;
            bool next=opponent is {} rival&&(state.Duos?player.Team==rival.Player.Team:player.Id==rival.Player.Id);
            var ink=player.Alive?HudInk:HudMuted;
            draw.PushClipRect(at,end,true);
            draw.AddRectFilled(at,end,hovered?new ImColor8(28,39,58):own?new ImColor8(29,31,34):new ImColor8(15,22,37),7);
            draw.AddRect(at,end,next?new ImColor8(255,96,112):own?HudGold:player.Id==viewed?new ImColor8(63,214,255):HudLine,7);
            if(portraits.DrawCompanion(ResultCompanion(player),at+new float2(5,5),rowHeight-14))StandingAvatarCount++;
            string health=player.Alive?Math.Max(0,player.Health).ToString():"#"+player.Placement;
            HudClipText(draw,new(end.X-36,at.Y+(rowHeight-20)/2),32,health,ink,20);
            string tag=ready?"READY":next?"VS":own?"YOU":state.Duos?"T"+(player.Team+1):"";
            float tagWidth=tag.Length>0?tag.Length*7+8:0;
            float nameWidth=width-91-tagWidth;
            HudClipText(draw,at+new float2(49,5),nameWidth,player.Name,ink,16);
            if(tag.Length>0)
            {
                var tagAt=at+new float2(width-40-tagWidth,7);var tagColor=ready?new ImColor8(79,224,143):next?new ImColor8(255,96,112):HudGold;
                draw.AddRectFilled(tagAt,tagAt+new float2(tagWidth,15),tagColor,3);
                HudClipText(draw,tagAt+new float2(4,0),tagWidth-6,tag,new ImColor8(16,19,26),11);
            }
            float hp=Math.Clamp((float)player.Health/(state.Duos?Rules.Duo!.TeamHp:Rules.Economy!.StartHp),0,1);
            float barWidth=width-126;var bar=at+new float2(49,rowHeight-17);
            draw.AddRectFilled(bar,bar+new float2(barWidth,5),new ImColor8(38,46,64),2);
            if(player.Alive)draw.AddRectFilled(bar,bar+new float2(barWidth*hp,5),hp<=.25f?new ImColor8(255,84,104):hp<=.5f?HudGold:new ImColor8(79,224,143),2);
            HudClipText(draw,new(end.X-72,at.Y+rowHeight-23),36,"Lv "+player.Economy.Level,HudMuted,12);
            var recent=matchHud.Recent(player.Id).Where(r=>!r.Pve).TakeLast(5).ToArray();
            for(int i=0;i<recent.Length;i++)draw.AddRectFilled(at+new float2(49+i*7,rowHeight-8),at+new float2(54+i*7,rowHeight-4),RoundOutcomeColor(recent[i].Outcome),1);
            if(!player.Alive)draw.AddRectFilled(at,end,new ImColor8(1,10,19,130),7);
            draw.PopClipRect();
            if(hovered)StandingTooltip(player,at);
            if(pressed)Watch(player.Id);
            if(sounds.FirstOrDefault(label=>label.Seat==player.Id) is {} clip)UiText.Wrapped("Playing: "+DigiTactics.Storage.CosmeticCollection.Item("sound",clip.Id)!.Name);
            EmoteRow(player.Id);
        }
        ImGui.PopStyleVar();
    }
    internal int RoundHistoryCards {get;private set;}
    private void RoundHistoryStrip()
    {
        var rounds=matchHud.Recent(viewed);RoundHistoryCards=0;
        if(rounds.Count==0)return;
        ImGui.Spacing();HudText("YOUR ROUNDS",15,HudMuted);ImGui.SameLine();
        HudText($"PvP {rounds.Count(r=>!r.Pve&&r.Outcome=="w")}-{rounds.Count(r=>!r.Pve&&r.Outcome=="l")}",13,HudMuted);
        float width=ImGui.GetContentRegionAvail().X;int columns=Math.Max(1,(int)((width+3)/33));
        foreach(var round in rounds.TakeLast(14))
        {
            if(RoundHistoryCards%columns!=0)ImGui.SameLine(0,3);
            var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(30,30));var draw=ImGui.GetWindowDrawList();
            var fill=round.Outcome=="w"?new ImColor8(30,74,51):round.Outcome=="l"?new ImColor8(77,31,39):HudSurface;
            draw.AddRectFilled(at,at+new float2(30,30),fill,5);
            if(round.Pve)
            {
                for(float edge=4;edge<27;edge+=6)
                {
                    draw.AddLine(at+new float2(edge,0),at+new float2(Math.Min(edge+3,27),0),RoundOutcomeColor(round.Outcome));
                    draw.AddLine(at+new float2(edge,30),at+new float2(Math.Min(edge+3,27),30),RoundOutcomeColor(round.Outcome));
                    draw.AddLine(at+new float2(0,edge),at+new float2(0,Math.Min(edge+3,27)),RoundOutcomeColor(round.Outcome));
                    draw.AddLine(at+new float2(30,edge),at+new float2(30,Math.Min(edge+3,27)),RoundOutcomeColor(round.Outcome));
                }
            }
            else draw.AddRect(at,at+new float2(30,30),RoundOutcomeColor(round.Outcome),5);
            HudClipText(draw,at+new float2(3,2),26,round.Round,HudInk,13);
            HudClipText(draw,at+new float2(3,15),26,round.Outcome.ToUpperInvariant()+(round.Damage>0?$" -{round.Damage}":""),HudInk,12);
            if(ImGui.IsItemHovered())ResultTooltip($"{round.Round} {(round.Pve?"Wild Digimon":"vs "+round.Opponent)}: {(round.Outcome=="w"?"won":round.Outcome=="l"?"lost":"draw")}"+(round.Damage>0?$", -{round.Damage} HP":""));
            RoundHistoryCards++;
        }
        ImGui.Spacing();
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
