using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>
/// The original #topbar: stage label and round track, phase, opponent, the centered planning
/// timer and the .tbtns row, laid out with the same flex gaps, paddings and breakpoints.
/// </summary>
internal sealed partial class MatchScreen
{
    // style.css tokens used by the match HUD.
    private static readonly Rgb CssPanel=HudHex("0f1625"),CssPanel2=HudHex("151e31"),CssPanel3=HudHex("1c273e"),CssLine=HudHex("223049"),CssLine2=HudHex("31425f"),
        CssText=HudHex("eef2fa"),CssMuted=HudHex("8f9ab0"),CssGold=HudHex("ffcf4a"),CssBad=HudHex("ff5468"),CssGood=HudHex("4fe08f"),CssEnemy=HudHex("ff6070"),
        CssRail=HudHex("0e121b"),CssInk=HudHex("10131a"),CssTopTop=HudHex("182033");

    private readonly record struct HudTool(string Key,string Label,bool Enabled,int Style);
    private const int ToolNormal=0,ToolPrimary=1,ToolDim=2,ToolCodexOn=3,ToolSoundboardOn=4;
    private readonly record struct TopBarLayout(bool Wraps,int Height,float RowTop,float RowHeight,float ToolsTop,float ToolHeight,float ToolFont,float ToolPadding,float Gap,float Padding);
    private int topBarFrame=-1;private TopBarLayout topBar;

    private float TopToolFont=>HudCompact?13:14;
    private float TopToolPadding=>HudCompact?8:10;
    private float TopToolHeight=>MathF.Round(TopToolFont*1.35f+2*(HudCompact?5:6)+2);
    private float TopGap=>HudNarrow?8:14;
    private float StageFont=>size.X<=1360?17:size.Y<=800?20:22;
    private float RoundCellWidth=>size.X<=1360?22:26;
    private float TimerWidth=>size.X<=1360?70:120;

    // Measured once at the start of Draw. Measuring bakes font sizes, which must never happen
    // between ImGui.Render and RenderDrawData (Render() reads Top after the frame is closed).
    private TopBarLayout CurrentTopBar=>topBarFrame>=0?topBar:new(false,52,0,51,9,33,14,10,14,12);
    private void UpdateTopBarLayout(){topBarFrame=ImGui.GetFrameCount();topBar=MeasureTopBar();}
    private TopBarLayout MeasureTopBar()
    {
        float padding=HudNarrow?8:12,gap=TopGap,toolHeight=TopToolHeight;
        if(Session is null)return new(false,52,0,52,(52-toolHeight)/2,toolHeight,TopToolFont,TopToolPadding,gap,padding);
        float left=StageWidth()+10+RoundTrackWidth()+gap+PhaseWidth();
        float opponent=OpponentWidth();if(opponent>0)left+=gap+opponent;
        float tools=ToolbarWidth(gap);
        float content=left+gap+TimerWidth+gap+tools;
        float tallest=Math.Max(Math.Max(29.9f,OpponentHeight()),toolHeight);
        // Above 1360px the grid row is fixed (52px, 48px when short); below it the bar is auto-height and wraps.
        if(size.X>1360&&content+2*padding<=size.X)
        {
            int height=size.Y<=800?48:52;
            return new(false,height,0,height-1,(height-1-toolHeight)/2,toolHeight,TopToolFont,TopToolPadding,gap,padding);
        }
        float pad=size.X<=1360?6:0;padding=HudNarrow?8:size.X<=1360?8:12;
        if(content+2*padding<=size.X)
        {
            int height=(int)MathF.Ceiling(2*pad+tallest+1);
            return new(false,height,pad,tallest,pad+(tallest-toolHeight)/2,toolHeight,TopToolFont,TopToolPadding,gap,padding);
        }
        float firstRow=Math.Max(29.9f,OpponentHeight());
        int wrapped=(int)MathF.Ceiling(2*pad+firstRow+gap+toolHeight+1);
        return new(true,wrapped,pad,firstRow,pad+firstRow+gap,toolHeight,TopToolFont,TopToolPadding,gap,padding);
    }

    private float StageWidth()=>Math.Max(40,HudWidth(HudWeight(800),StageFont,Session!.Rounds.Position.Label,.5f));
    private float PhaseWidth()
        =>Math.Max(82,Math.Max(HudWidth(HudWeight(700),14,PhaseLabel),HudWidth(HudWeight(400),12,PhaseSub)));
    private string PhaseLabel=>Session!.Rounds.Phase switch{MatchPhase.Planning=>"Planning",MatchPhase.Combat=>"Combat",_=>"Result"};
    private string PhaseSub=>Session!.Rounds.RoundType=="pve"?"Wild Digimon":Session.Rounds.RoundType=="carousel"?"Carousel":Session.State.Duos?"Duos":"PvP";

    private void Header()
    {
        var session=Session!;var layout=CurrentTopBar;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
        ImGui.PushStyleColor(ImGuiCol.WindowBg,CssPanel);ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));
        Window("Match",0,0,size.X,Top,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleColor(2);ImGui.PopStyleVar();
        var draw=ImGui.GetWindowDrawList();
        draw.PushClipRect(new(0,0),new(size.X,Top));
        draw.AddRectFilledMultiColor(new(0,0),new(size.X,Top-1),(ImColor8)CssTopTop,(ImColor8)CssTopTop,(ImColor8)CssPanel,(ImColor8)CssPanel);
        draw.AddLine(new(0,Top-.5f),new(size.X,Top-.5f),CssLine);
        draw.PopClipRect();
        float center=layout.RowTop+layout.RowHeight/2,x=layout.Padding;
        // #stageLabel: 800 weight, .5px letter spacing, min-width 40.
        HudRun(draw,HudWeight(800),StageFont,x,center-StageFont*1.35f/2,StageFont*1.35f,session.Rounds.Position.Label,CssText,0,.5f);
        x+=StageWidth()+10;
        RoundTimeline(x,center);x=TimelineBounds.Max.X+layout.Gap;
        float phaseTop=center-(14*1.15f+12*1.15f)/2;
        HudRun(draw,HudWeight(700),14,x,phaseTop,14*1.15f,PhaseLabel,CssText);
        HudRun(draw,HudWeight(400),12,x,phaseTop+14*1.15f,12*1.15f,PhaseSub,CssMuted);
        x+=PhaseWidth();
        float opponent=OpponentWidth();
        HeaderOpponentName=null;HeaderOpponentTooltip=null;
        if(opponent>0){x+=layout.Gap;OpponentCard(x,center);x+=opponent;}
        else HeaderOpponentBounds=(new(x,center),new(x,center));
        var tools=Toolbar();
        float toolsWidth=ToolbarWidth(layout.Gap,tools);
        float toolsX=size.X-layout.Padding-toolsWidth;
        // Two flex spacers share the free space, so the timer sits midway between the opponent and the buttons.
        float timerRight=layout.Wraps?size.X-layout.Padding:toolsX-layout.Gap;
        float timerX=x+layout.Gap+Math.Max(0,(timerRight-(x+layout.Gap)-TimerWidth)/2);
        PlanningTimer(draw,timerX,center);
        DrawToolbar(tools,toolsX,layout.ToolsTop,layout);
        ImGui.End();
    }

    private void PlanningTimer(ImDrawListPtr draw,float x,float center)
    {
        var session=Session!;
        double? remaining=networkRoom is not null?NetworkPlanningRemaining:PlanningTimerEnabled&&session.Rounds.Phase==MatchPhase.Planning&&session.Rounds.RoundType!="carousel"?PlanningRemaining:null;
        if(remaining is not {} seconds)return;
        bool low=seconds<=5;var ink=low?CssBad:CssGold;
        float top=center-(20+3+4)/2f;
        HudRunCentered(draw,HudWeight(800),20,x+TimerWidth/2,top,20,$"{Math.Max(0,Math.Ceiling(seconds))}s",ink);
        float total=networkRoom is not null?Math.Max(1,(float)Rules.PlanningSeconds):Math.Max(1,(float)Rules.PlanningSeconds);
        float fraction=Math.Clamp((float)seconds/total,0,1);
        var bar=new float2(x,top+23);
        draw.AddRectFilled(bar,bar+new float2(TimerWidth,4),CssPanel3,2);
        if(fraction>0)draw.AddRectFilled(bar,bar+new float2(TimerWidth*fraction,4),ink,2);
    }

    private string SoundButtonLabel=>audio.Muted?"Muted":audio.MusicOff?"Music off":"Sound on";
    private List<HudTool> Toolbar()
    {
        var session=Session!;var tools=new List<HudTool>{new("gameMenu","Menu",true,ToolNormal)};
        if(profile is not null)
        {
            if(MatchStoreEntryAvailable)tools.Add(new("matchStore","Store",true,ToolNormal));
            tools.Add(new("matchLocker","Locker",true,ToolNormal));
        }
        tools.Add(new("codex","Codex",true,CodexVisible?ToolCodexOn:ToolNormal));
        tools.Add(new("emotes","Emote",true,ToolNormal));
        if(profile is not null)tools.Add(new("matchLegend",HudCompact?"Legend":"Little Legend",true,ToolNormal));
        if(networkRoom is null&&!session.State.Over&&Human.Alive)tools.Add(new("planningTimer",PlanningTimerEnabled?"Timer on":"Timer off",true,PlanningTimerEnabled?ToolNormal:ToolDim));
        tools.Add(new("replaySpeed",$"{ReplaySpeed}x",true,ToolNormal));
        tools.Add(new("soundboard","Soundboard",true,showSoundboard?ToolSoundboardOn:ToolNormal));
        tools.Add(new("audioSettings",SoundButtonLabel,true,ToolNormal));
        if(networkRoom is not null)
        {
            tools.Add(new("players","Players",true,ToolNormal));
            if(!session.State.Over)
            {
                if(IsSpectating)
                {
                    var slots=networkRoom.Authority.Lobby?["slots"];
                    int available=Enumerable.Range(0,8).FirstOrDefault(i=>slots?[i.ToString()] is null&&session.State.Players[i].Alive,-1);
                    if(available>=0)tools.Add(new("joinSeat",$"Take seat {available+1}",true,ToolNormal));
                }
                else tools.Add(new("spectate","Spectate",true,ToolNormal));
            }
            tools.Add(new("leaveOnline","Leave",true,ToolNormal));
        }
        if(session.State.Over)
        {
            tools.Add(CanWatchAfterElimination?new("spectateRemaining","Spectate",true,ToolPrimary):new("menu","Home",true,ToolPrimary));
            return tools;
        }
        bool combat=session.Rounds.Phase==MatchPhase.Combat;
        tools.Add(new("finish",networkRoom is null?"Skip":"Finish replay",combat,ToolNormal));
        bool watching=WatchingAfterElimination||IsSpectating;
        if(session.Rounds.Phase==MatchPhase.Result&&networkRoom is null&&!watching)tools.Add(new("next","Next round",true,ToolPrimary));
        else tools.Add(new("fight",MatchReadyLabel,session.Rounds.Phase==MatchPhase.Planning&&session.Rounds.RoundType!="carousel"&&!watching&&Human.Alive,ToolPrimary));
        return tools;
    }
    private float ToolWidth(HudTool tool)
    {
        float width=HudWidth(HudWeight(tool.Style==ToolPrimary?700:400),TopToolFont,tool.Label)+2*TopToolPadding+2;
        return tool.Key=="fight"||tool.Key=="next"?Math.Max(92,width):width;
    }
    private float ToolbarWidth(float gap,List<HudTool>? tools=null)
    {
        tools??=Toolbar();
        return tools.Sum(ToolWidth)+Math.Max(0,tools.Count-1)*6;
    }
    private void DrawToolbar(List<HudTool> tools,float x,float y,TopBarLayout layout)
    {
        var draw=ImGui.GetWindowDrawList();
        foreach(var tool in tools)
        {
            float width=ToolWidth(tool);var min=new float2(x,y);var max=min+new float2(width,layout.ToolHeight);
            var (fill,border,ink)=tool.Style switch
            {
                ToolPrimary=>(HudHex("1f63b0"),HudHex("4a92e0"),CssText),
                ToolCodexOn=>(CssPanel2,CssGold,CssGold),
                ToolSoundboardOn=>(HudHex("13362b"),HudHex("5adfa8"),CssText),
                _=>(CssPanel2,CssLine2,CssText),
            };
            if(tool.Enabled)
            {
                ImGui.SetCursorScreenPos(min);
                bool pressed=ImGui.InvisibleButton(tool.Key,max-min);
                bool hovered=ImGui.IsItemHovered();
                if(ImGui.IsItemFocused())KeyboardFocusedButton=tool.Key;
                if(tool.Key=="emotes")emotesToggleHovered=hovered;
                if(tool.Key=="soundboard")soundboardToggleHovered=hovered;
                if(tool.Key=="audioSettings")audioToggleHovered=hovered;
                Buttons[tool.Key]=(min+max)*.5f;
                ClientButtonSound(tool.Key,pressed);
                if(hovered&&tool.Style!=ToolPrimary){border=HudHex("3fd6ff");fill=CssPanel3;}
                if(hovered&&tool.Style==ToolPrimary)fill=HudHex("2672c8");
                if(tool.Key=="replaySpeed"&&hovered){ImGui.BeginTooltip();ImGui.Text("Battle playback speed"u8);ImGui.EndTooltip();}
                float alpha=tool.Style==ToolDim?.6f:1;
                var under=CssPanel;
                draw.AddRectFilled(min,max,HudOver(fill,under,alpha),6);
                draw.AddRect(min,max,HudOver(border,under,alpha),6);
                HudRunCentered(draw,HudWeight(tool.Style==ToolPrimary?700:400),TopToolFont,(min.X+max.X)/2,min.Y+1,layout.ToolHeight-2,tool.Label,HudOver(ink,under,alpha));
                if(pressed)ToolbarAction(tool.Key);
            }
            else
            {
                // button:disabled { opacity: .45 } over the top bar gradient.
                var under=HudMix(CssTopTop,CssPanel,.5f);
                draw.AddRectFilled(min,max,HudOver(fill,under,.45f),6);
                draw.AddRect(min,max,HudOver(border,under,.45f),6);
                HudRunCentered(draw,HudWeight(tool.Style==ToolPrimary?700:400),TopToolFont,(min.X+max.X)/2,min.Y+1,layout.ToolHeight-2,tool.Label,HudOver(ink,under,.45f));
            }
            x+=width+6;
        }
    }
    private void ToolbarAction(string key)
    {
        switch(key)
        {
            case "gameMenu":OpenGameMenu();break;
            case "matchStore":OpenMatchCosmetics(true);break;
            case "matchLocker":OpenMatchCosmetics(false);break;
            case "matchLegend":OpenMatchCosmetics(false,"legend");break;
            case "codex":ToggleCodex();break;
            case "emotes":showEmotes=!showEmotes;if(showEmotes)showSoundboard=false;break;
            case "planningTimer":PlanningTimerEnabled=!PlanningTimerEnabled;PlanningRemaining=Rules.PlanningSeconds;break;
            case "replaySpeed":ReplaySpeed=ReplaySpeed==1?2:ReplaySpeed==2?4:1;break;
            case "soundboard":showSoundboard=!showSoundboard;if(showSoundboard)showEmotes=false;break;
            case "audioSettings":showAudio=!showAudio;break;
            case "players":showPlayers=!showPlayers;break;
            case "joinSeat":
            {
                var slots=networkRoom!.Authority.Lobby?["slots"];
                int available=Enumerable.Range(0,8).FirstOrDefault(i=>slots?[i.ToString()] is null&&Session!.State.Players[i].Alive,-1);
                if(available>=0)networkRoom.ClaimSeat(available,SteamName,networkClock());
                break;
            }
            case "spectate":networkRoom!.FreeSeat(localSeat);break;
            case "leaveOnline":OpenGameMenu();confirmLeave=true;break;
            case "spectateRemaining":ContinueAfterElimination();break;
            case "menu":LeaveMatch();break;
            case "finish":SkipCombat();break;
            case "next":Next();break;
            case "fight":Fight();break;
        }
    }
}
