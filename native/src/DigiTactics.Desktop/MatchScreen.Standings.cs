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
        var state=Session!.State;
        int alive=state.Duos?state.Teams.Count(t=>t.Alive):state.Players.Count(p=>p.Alive);
        HudText($"{(state.Duos?"TEAMS":"TAMERS")} / {alive} left",17,HudMuted);
        var opponent=StandingOpponent(state.Players[viewed]);
        EnsureSoundboard();var sounds=soundboard.Labels(Environment.TickCount64);
        foreach(var seat in ScoutOrder())
        {
            var player=state.Players[seat.Id];float width=ImGui.GetContentRegionAvail().X;
            bool pressed=HudButton("view/"+player.Id,"",width,76,player.Id==viewed);
            var at=ImGui.GetItemRectMin();var end=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
            bool hovered=ImGui.IsItemHovered();bool own=player.Id==localSeat;
            bool ready=Session.Rounds.Phase==MatchPhase.Planning&&networkReplica?.Match?.Votes.Contains(player.Id)==true;
            bool next=opponent is {} rival&&(state.Duos?player.Team==rival.Player.Team:player.Id==rival.Player.Id);
            var ink=player.Alive?HudInk:HudMuted;
            draw.PushClipRect(at,end,true);
            if(portraits.DrawCompanion(ResultCompanion(player),at+new float2(6,7),36))StandingAvatarCount++;
            HudClipText(draw,at+new float2(49,7),width-57,player.Name,ink,16);
            string tag=own?"YOU":networkRoom is not null&&player.IsHuman?"PLAYER":"AI";
            if(state.Duos)tag="T"+(player.Team+1)+" "+(player.Team==Human.Team&&!own?"PARTNER":tag);
            if(ready)tag+=" / READY";
            else if(next)tag+=" / "+(Session.Rounds.Phase==MatchPhase.Planning?"NEXT":"VS");
            HudClipText(draw,at+new float2(49,27),width-57,tag,ready?new(111,224,138):next?new(255,138,122):HudGold,15);
            string streak=player.Economy.Streak>1?" W"+player.Economy.Streak:player.Economy.Streak< -1?" L"+-player.Economy.Streak:"";
            HudClipText(draw,at+new float2(49,47),width-100,$"Lv {player.Economy.Level}"+streak,ink,15);
            string health=player.Alive?Math.Max(0,player.Health).ToString():"#"+player.Placement;
            ImGui.PushFont(default,17);float hpWidth=ImGui.CalcTextSize(health).X;ImGui.PopFont();
            HudClipText(draw,new(end.X-hpWidth-8,at.Y+47),hpWidth+1,health,player.Alive?HudInk:HudMuted,17);
            float hp=Math.Clamp((float)player.Health/(state.Duos?Rules.Duo!.TeamHp:Rules.Economy!.StartHp),0,1);
            float barWidth=width-57;var bar=at+new float2(49,67);
            draw.AddRectFilled(bar,bar+new float2(barWidth,4),new ImColor8(40,50,65),2);
            if(player.Alive)draw.AddRectFilled(bar,bar+new float2(barWidth*hp,4),hp<=.25f?new ImColor8(255,138,122):hp<=.5f?HudGold:new ImColor8(111,224,138),2);
            var recent=matchHud.Recent(player.Id).Where(r=>!r.Pve).TakeLast(5).ToArray();
            for(int i=0;i<recent.Length;i++)draw.AddRectFilled(at+new float2(7+i*7,53),at+new float2(12+i*7,64),RoundOutcomeColor(recent[i].Outcome),1);
            if(!player.Alive)draw.AddRectFilled(at,end,new ImColor8(1,10,19,85),6);
            if(next)draw.AddRect(at+new float2(1,1),end-new float2(1,1),new ImColor8(255,138,122),6);
            else if(own)draw.AddRect(at+new float2(1,1),end-new float2(1,1),HudGold,6);
            draw.PopClipRect();
            if(hovered)StandingTooltip(player);
            if(pressed)Watch(player.Id);
            if(sounds.FirstOrDefault(label=>label.Seat==player.Id) is {} clip)UiText.Wrapped("Playing: "+DigiTactics.Storage.CosmeticCollection.Item("sound",clip.Id)!.Name);
            EmoteRow(player.Id);
        }
    }
    private void StandingTooltip(MatchPlayer player)
    {
        StandingTooltipSeat=player.Id;ImGui.SetNextWindowSize(new(390,0));ImGui.BeginTooltip();
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
        ImGui.EndTooltip();
    }
}
