using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly PostgameArtwork postgameArtwork = new();
    private static readonly Dictionary<string,Dictionary<string,string>> resultTraitColors=System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,Dictionary<string,string>>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"content/postgame/trait-colors.json")))!;
    private MatchPlanningSession? postgameSession;
    private MatchPlanningSession? dismissedHomeSession;
    private bool postgameDismissed;
    private bool postgameStore;
    private RewardBreakdown? postgameAnimatedAward;
    private double postgameAwardStart;
    private int postgameAwardSounds;
    internal bool PostgameVisible { get; private set; }
    internal int PostgameStandings { get; private set; }
    internal int PostgameUnitPortraits { get; private set; }
    internal int PostgameMissionCount { get; private set; }
    internal int PostgameAvatarCount { get; private set; }
    internal long PostgameXpGain => history?.XpGained ?? 0;
    internal int? PostgameBits => history?.Award?.Total;
    internal int? PostgameDisplayedBits { get; private set; }
    internal bool PostgameStoreVisible => postgameStore;
    internal bool PostgameRewardsBelow { get; private set; }
    internal bool PostgameStoreButtonVisible { get; private set; }
    internal float2 PostgameRewardsMin { get; private set; }
    internal float2 PostgameRewardsMax { get; private set; }
    internal float2 PostgameStandingsMin { get; private set; }
    internal float2 PostgameStandingsMax { get; private set; }
    internal float2 PostgameProgressMin { get; private set; }
    internal float2 PostgameProgressMax { get; private set; }
    internal bool PostgameHistoryVisible => ProfileVisible && Session is not null;
    internal bool PostgameHomeVisible => Session is not null && ReferenceEquals(dismissedHomeSession,Session);
    private bool PostgameClientOverlay => Session is not null && ReferenceEquals(postgameSession,Session)
        && (showProfile || postgameStore&&showCollection) && (Session.State.Over || !IsSpectating&&Human.Placement>0);
    private bool ClientHomeVisible => Session is null || PostgameHomeVisible || PostgameClientOverlay;
    private void DismissResults()
    {
        // The original result X/Home actions hide the result in the client shell.
        // They do not invoke the separate leave-match callback or drop the room.
        dismissedHomeSession=Session;postgameDismissed=true;PostgameVisible=false;
        postgameStore=showMultiplayer=showProfile=showCollection=showFarm=showAbout=false;
        CancelArenaDrag();cameraDrag=null;
    }

    private bool DrawPostgame()
    {
        PostgameVisible = false;
        if (!ReferenceEquals(postgameSession, Session)) { postgameSession = Session; postgameDismissed = postgameStore = false; postgameAnimatedAward = null; }
        if (Session is null || PostgameHomeVisible) return false;
        bool stillPlaying = (Session.State.Duos ? Session.State.Teams.Count(t => t.Alive) : Session.State.Players.Count(p => p.Alive)) > 1;
        if ((postgameDismissed && stillPlaying) || !Session.State.Over && (IsSpectating || Human.Placement == 0)) return false;
        PostgameVisible = true; PostgameStandings = PostgameUnitPortraits = PostgameMissionCount = PostgameAvatarCount = 0;
        if(postgameStore) { Collection(); if(!showCollection)postgameStore=false; return true; }
        bool outNow = !IsSpectating && !Human.Alive && stillPlaying;
        int place = Human.Placement;
        bool gold = !IsSpectating && place == 1;
        bool top = !IsSpectating && place <= (Session.State.Duos ? 2 : 4);
        string title = IsSpectating ? "Match complete" : outNow ? "Eliminated" : gold ? "Victory" : top ? Session.State.Duos ? "Top 2 teams" : "Top 4" : "Defeat";
        var accent = gold ? new ImColor8(255,213,106) : top ? new ImColor8(63,214,255) : new ImColor8(167,178,199);
        bool compact = size.X <= 1100;
        PostgameRewardsBelow = size.X <= 1500;
        PostgameStoreButtonVisible = false;
        float margin = 24, width = size.X - margin * 2, height = size.Y - margin * 2;
        float padding = compact ? 16 : 24, topPadding = compact ? 12 : 16, gap = compact ? 8 : 12;
        Window("Match results", margin, margin, width, height, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        var origin = ImGui.GetWindowPos(); var draw = ImGui.GetWindowDrawList();
        postgameArtwork.Draw(gold && !outNow ? "postgame-bg" : "loading-bg", origin, new(width,height), 255, true, .3f);
        var topTint=new ImColor8(1,10,19,140); var bottomTint=new ImColor8(1,10,19,235);
        draw.AddRectFilledMultiColor(origin,origin+new float2(width,height*.55f),topTint,topTint,bottomTint,bottomTint);
        draw.AddRectFilled(origin+new float2(0,height*.55f),origin+new float2(width,height),bottomTint);
        draw.AddRect(origin, origin + new float2(width,height), gold?new ImColor8(200,170,110):new ImColor8(120,90,40), 8, ImDrawFlags.None, 1);
        ImGui.SetCursorPos(new(width-46,10));
        if(Button("resultDismiss","X",36,36)) { DismissResults(); ImGui.End(); return true; }
        ImGui.PushFont(NativeUiTheme.BodyFont,NativeUiTheme.BodySize);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(8,6));
        float heroX = padding + (IsSpectating ? 0 : 194), heroWidth = width-padding-heroX;
        ImGui.SetCursorPos(new(heroX,topPadding));
        ImGui.BeginGroup();
        ResultText($"{(networkRoom is null ? "Solo vs AI" : "Multiplayer")} / {MatchSetTitle} / results",NativeUiTheme.DisplayFont,16,NativeUiTheme.Gold);
        ResultText(title.ToUpperInvariant(),NativeUiTheme.DisplayBoldFont,Math.Clamp(size.X*.04f,38,64),NativeUiTheme.GoldBright);
        var record = history?.Match;
        var winningPlayer = Session.State.Players.FirstOrDefault(p => p.Placement == 1);
        string? winner = winningPlayer is not null && winningPlayer.Id != localSeat ? winningPlayer.Name : null;
        ImGui.PushStyleColor(ImGuiCol.Text,NativeUiTheme.Muted); ImGui.PushTextWrapPos(heroX+heroWidth);
        UiText.Wrapped(outNow ? "The others are still playing. Spectate their fights or leave."
            : $"Reached round {Session.Rounds.Position.Label} at level {Human.Economy.Level}." + (winner is null ? "" : " Winner: " + winner + "."));
        ImGui.PopTextWrapPos(); ImGui.PopStyleColor();
        if (!IsSpectating && record is not null) ResultComposition(record, Rules.Id, heroWidth,compact?32:40);
        ImGui.EndGroup();
        float heroHeight = Math.Max(compact?145:184,ImGui.GetItemRectSize().Y);
        if(!IsSpectating)
        {
            float emblemSize=compact?96:130,numberSize=compact?36:46,placeHeight=emblemSize-14+numberSize+19;
            var emblem=origin+new float2(padding+(170-emblemSize)/2,topPadding+(heroHeight-placeHeight)/2);
            postgameArtwork.Draw(place == 1 ? "emblem-mega" : place <= 2 ? "emblem-ultimate" : place <= 4 ? "emblem-champion" : "emblem-rookie",emblem,new(emblemSize,emblemSize));
            string number=$"#{place}",label=PostgameSummary.PlaceLabel(place,Session.State.Duos).ToUpperInvariant();
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,numberSize);
            draw.AddText(emblem+new float2((emblemSize-ImGui.CalcTextSize(number).X)/2,emblemSize-14),accent,number); ImGui.PopFont();
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,16);
            draw.AddText(emblem+new float2((emblemSize-ImGui.CalcTextSize(label).X)/2,emblemSize-14+numberSize+4),NativeUiTheme.Gold,label); ImGui.PopFont();
        }
        float colsY = topPadding+heroHeight+gap, footerY = height-topPadding-38;
        float columnsHeight = Math.Max(120,footerY-gap-colsY), contentWidth = width-padding*2;
        float topHeight = PostgameRewardsBelow ? Math.Min(300,Math.Max(120,(columnsHeight-gap)*.60f)) : columnsHeight;
        float standingsWidth = PostgameRewardsBelow ? (contentWidth-gap)/2 : (contentWidth-gap*2)*1.1f/3.1f;
        float progressWidth = PostgameRewardsBelow ? standingsWidth : (contentWidth-gap*2)/3.1f;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(12,10));
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(1,10,19,204));
        ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(70,55,20));
        ImGui.SetCursorPos(new(padding,colsY));
        ImGui.BeginChild("Result standings"u8,new(standingsWidth,topHeight),ImGuiChildFlags.Borders);
        PostgameStandingsMin=ImGui.GetWindowPos(); PostgameStandingsMax=PostgameStandingsMin+ImGui.GetWindowSize();
        DrawResultStandings();
        ImGui.EndChild();
        ImGui.SetCursorPos(new(padding+standingsWidth+gap,colsY));
        ImGui.BeginChild("Result progress"u8,new(progressWidth,topHeight),ImGuiChildFlags.Borders);
        PostgameProgressMin=ImGui.GetWindowPos(); PostgameProgressMax=PostgameProgressMin+ImGui.GetWindowSize();
        ResultHeading("Progress");
        if (profile is null || IsSpectating) UiText.Wrapped(IsSpectating ? "Spectating does not award match progress." : "No player profile is attached to this session.");
        else DrawResultProgress();
        ImGui.EndChild();
        float2 rewardsPosition=PostgameRewardsBelow?new(padding,colsY+topHeight+gap):new(padding+standingsWidth+progressWidth+gap*2,colsY);
        float2 rewardsSize=PostgameRewardsBelow?new(contentWidth,Math.Max(80,columnsHeight-topHeight-gap)):new(progressWidth,columnsHeight);
        ImGui.SetCursorPos(rewardsPosition);
        ImGui.BeginChild("Result rewards"u8,rewardsSize,ImGuiChildFlags.Borders);
        PostgameRewardsMin=ImGui.GetWindowPos(); PostgameRewardsMax=PostgameRewardsMin+ImGui.GetWindowSize();
        DrawResultRewards();
        ImGui.EndChild();
        ImGui.PopStyleColor(2); ImGui.PopStyleVar();
        ImGui.SetCursorPos(new(width-padding-536,footerY));
        ImGui.BeginDisabled(profile is null); if(Button("resultHistory","Match history",180,38)) OpenProfile(); ImGui.EndDisabled();
        ImGui.SameLine(0,8);
        if(Button("menu",outNow?"Leave":"Home",140,38))
        {
            if(outNow){bool online=networkRoom is not null;LeaveMatch();if(online)OpenMultiplayerView();}
            else DismissResults();
        }
        ImGui.SameLine(0,8);
        if (Session is not null)
        {
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,18);
            ImGui.PushStyleColor(ImGuiCol.Button,NativeUiTheme.Play); ImGui.PushStyleColor(ImGuiCol.ButtonHovered,NativeUiTheme.PlayMiddle);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive,NativeUiTheme.PlayTop); ImGui.PushStyleColor(ImGuiCol.Text,NativeUiTheme.PlayText);
            if (outNow)
            {
                if(Button("spectateRemaining","Spectate",200,38)) { postgameDismissed=true; if(networkRoom is null) ContinueAfterElimination(); }
            }
            else if(Button("resultAgain",networkRoom is null?"Play again":"Back to lobbies",200,38))
            {
                bool online=networkRoom is not null; bool nextDuos=Session.State.Duos;
                LeaveMatch();
                if(online) OpenMultiplayerView();
                else StartLocalMatch(nextDuos);
            }
            ImGui.PopStyleColor(4); ImGui.PopFont();
        }
        ImGui.PopStyleVar(); ImGui.PopFont();
        ImGui.End(); return true;
    }

    private void DrawResultStandings()
    {
        ResultHeading("Standings");
        foreach (var player in Session!.State.Players.OrderBy(p => p.Placement))
        {
            var at = ImGui.GetCursorScreenPos(); float rowWidth=ImGui.GetContentRegionAvail().X;
            if (player.Id == localSeat)
            {
                var tint=new ImColor8(200,170,110,46);var clear=new ImColor8(200,170,110,0);
                ImGui.GetWindowDrawList().AddRectFilledMultiColor(at,at+new float2(rowWidth,30),tint,clear,clear,tint);
                ImGui.GetWindowDrawList().AddLine(at,at+new float2(0,30),NativeUiTheme.Gold,2);
            }
            var draw=ImGui.GetWindowDrawList();
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,17);
            draw.AddText(at+new float2(8,6),NativeUiTheme.Gold,player.Placement > 0 ? $"#{player.Placement}" : "-"); ImGui.PopFont();
            if(portraits.DrawCompanion(ResultCompanion(player),at+new float2(44,2),26))PostgameAvatarCount++;
            string name = (Session.State.Duos ? $"Team {player.Team+1} / " : "") + player.Name;
            string status=player.Placement == 1 ? "Winner" : player.Placement > 0 ? "Eliminated" : $"{Math.Max(0,player.Health)} HP";
            ImGui.PushFont(NativeUiTheme.BodyFont,16);
            float statusWidth=ImGui.CalcTextSize(status).X;
            draw.AddText(at+new float2(rowWidth-statusWidth-6,7),NativeUiTheme.Muted,status); ImGui.PopFont();
            draw.AddText(at+new float2(78,6),NativeUiTheme.GoldBright,ResultTrim(name,Math.Max(20,rowWidth-statusWidth-98)));
            ImGui.Dummy(new(rowWidth,30)); if (ImGui.IsItemHovered()) ResultTooltip(name);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY()-4); PostgameStandings++;
        }
        ImGui.Dummy(new(0,0));
    }

    private void DrawResultRewards()
    {
        ResultHeading("Bits earned"+(history?.FreshAward==false?" / already added":""));
        if (history?.Award is { } award)
        {
            if(!ReferenceEquals(postgameAnimatedAward,award)) {postgameAnimatedAward=award;postgameAwardStart=boardSceneTime;postgameAwardSounds=0;}
            double time=boardSceneTime-postgameAwardStart;
            PostgameDisplayedBits=history.FreshAward?PostgameAwardCount.Total(award.Total,award.Lines.Length,time):award.Total;
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(8,3));
            for(int i=0;i<award.Lines.Length;i++)
            {
                var line=award.Lines[i]; int count=history.FreshAward?PostgameAwardCount.Line(line.N,i,time):line.N;
                ResultPair(line.Label,$"+{count}",new ImColor8(245,207,109),17);
            }
            ImGui.Separator(); ResultPair("Total",$"{PostgameDisplayedBits.Value} Bits",NativeUiTheme.GoldBright,19);
            ImGui.PopStyleVar();
            float rowY=ImGui.GetCursorPosY(),right=ImGui.GetCursorPosX()+ImGui.GetContentRegionAvail().X;
            ResultText($"Wallet: {profile?.Progress.Bits ?? 0} Bits",NativeUiTheme.BodyFont,16,NativeUiTheme.Muted);
            ImGui.SameLine(); ImGui.SetCursorPos(new(right-86,rowY));
            if(Button("resultStore","Store",86,36))
            {
                // The multiplayer client stays selected behind an online game.
                // Results must open Store above that retained lobby selection.
                showMultiplayer=false;OpenCollectionPage(store:true);postgameStore=true;
            }
            var storeMin=ImGui.GetItemRectMin();var storeMax=ImGui.GetItemRectMax();
            var clipMin=ImGui.GetWindowDrawList().GetClipRectMin();var clipMax=ImGui.GetWindowDrawList().GetClipRectMax();
            PostgameStoreButtonVisible=storeMin.Y>=clipMin.Y&&storeMax.Y<=clipMax.Y;
            if (history.LegendAward is { } legend) ResultText($"{CosmeticCollection.Item("legend",legend.Legend)?.Name ?? legend.Legend}: +{legend.Data} Digi-Data (level {legend.Level})",NativeUiTheme.BodyFont,16,NativeUiTheme.Muted);
        }
        else UiText.Wrapped(IsSpectating ? "Join a seat to earn rewards in your next match." : history?.Error ?? (networkRoom is null ? "Rewards are unavailable for this session." : "Verifying the shared match before saving rewards..."));
        ImGui.BeginDisabled(networkRoom is not null && networkReplica?.LastDesync is not null);
        if (history?.Error is not null && Button("retryHistory","Retry saving progress")) history.Complete(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        ImGui.EndDisabled();
    }

    private void DrawResultProgress()
    {
        try
        {
            var level = TamerProgress.Read(profile!.Storage.GetValueOrDefault("dt.client.v1"));
            ResultPair($"Tamer level {level.Level}"+(history?.LeveledUp==true?" / Level up!":""),history?.XpGained>0?$"+{history.XpGained} XP":"",NativeUiTheme.Gold,17);
            ImGui.ProgressBar((float)((double)level.Xp/level.Need),new(-1,6), ""u8);
            if(ImGui.IsItemHovered())ResultTooltip($"{level.Xp} / {level.Need} XP");
            ImGui.Dummy(new(0,2));
            var daily=MatchRewards.Progress(profile.Storage.GetValueOrDefault("dt.shop.v1"),profile.Storage.GetValueOrDefault("dt.client.v1"),DateOnly.FromDateTime(DateTime.Now));
            foreach(var mission in daily.Missions)
            {
                ResultPair(mission.Name,$"{mission.Bits} Bits",NativeUiTheme.Gold,17);
                ImGui.ProgressBar((float)mission.Current/mission.Goal,new(-1,5), ""u8);
                ResultText(mission.Done ? "Complete" : $"Best today {mission.Current}/{mission.Goal}",NativeUiTheme.BodyFont,16,NativeUiTheme.Muted);
                ImGui.Dummy(new(0,1)); PostgameMissionCount++;
            }
            ImGui.Separator(); ResultPair("Daily bonus",$"{daily.Bits} Bits",NativeUiTheme.Gold,17);
            ResultText(daily.Claimed ? "Claimed. Back tomorrow." : "Complete your first game today.",NativeUiTheme.BodyFont,16,NativeUiTheme.Muted);
        }
        catch(Exception error) when(error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or ArgumentOutOfRangeException)
        { UiText.Wrapped("Saved progress could not be read. Your data has been preserved."u8); }
    }

    private static void ResultText(string value,ImFontPtr font,float fontSize,ImColor8 color)
    {
        ImGui.PushFont(font,fontSize); ImGui.PushStyleColor(ImGuiCol.Text,color);
        UiText.Wrapped(value); ImGui.PopStyleColor(); ImGui.PopFont();
    }
    private static void ResultHeading(string label)
        => ResultText(label.ToUpperInvariant(),NativeUiTheme.DisplayBoldFont,17,NativeUiTheme.GoldBright);
    private static void ResultPair(string label,string value,ImColor8 valueColor,float fontSize)
    {
        ImGui.PushFont(NativeUiTheme.BodyFont,fontSize);
        float x=ImGui.GetCursorPosX(),y=ImGui.GetCursorPosY(),width=ImGui.GetContentRegionAvail().X,valueWidth=ImGui.CalcTextSize(value).X;
        float labelWidth=Math.Max(32,width-valueWidth-12);
        ImGui.PushTextWrapPos(x+labelWidth); ImGui.Text(label); ImGui.PopTextWrapPos();
        float nextY=ImGui.GetCursorPosY(); ImGui.SetCursorPos(new(x+width-valueWidth,y));
        ImGui.PushStyleColor(ImGuiCol.Text,valueColor); ImGui.Text(value); ImGui.PopStyleColor();
        ImGui.SetCursorPos(new(x,Math.Max(nextY,ImGui.GetCursorPosY()))); ImGui.PopFont();
    }

    private void UpdatePostgameAward()
    {
        if(postgameAnimatedAward is not { } award || !ReferenceEquals(postgameSession,Session) || history?.FreshAward!=true)return;
        int completed=PostgameAwardCount.CompletedRows(award.Lines.Length,boardSceneTime-postgameAwardStart);
        while(postgameAwardSounds<completed)audio.Play(++postgameAwardSounds>award.Lines.Length?"pick":"tick_normal");
    }

    private void ResultComposition(HistoryMatch record,string variantId,float width,float portraitSize)
    {
        var validRules=catalog.Variants.FirstOrDefault(r=>r.Id==variantId) ?? catalog.Variants.FirstOrDefault(r=>r.Id.Split('/')[0]==record.Set);
        if(validRules is null) return;
        float rowWidth=0;
        ImGui.PushFont(NativeUiTheme.BodyFont,16);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(5,5));
        foreach(var trait in (record.Traits ?? []).Take(6).Where(t=>validRules.Traits.Any(x=>x.Id==t.Id)))
        {
            string label=$"{text.Get(validRules.Id,trait.Id).Name} {trait.N}";
            float chipWidth=ImGui.CalcTextSize(label).X+30;
            if(rowWidth>0 && rowWidth+chipWidth+5<=width){ImGui.SameLine(0,5);rowWidth+=5;}else rowWidth=0;
            rowWidth+=chipWidth;var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(chipWidth,25));
            var tint=ResultTraitTint(resultTraitColors[validRules.Id].GetValueOrDefault(trait.Id,"#9aa4b1"));
            var draw=ImGui.GetWindowDrawList();
            // CSS mixes 14% opaque trait color with 86% rgba(1,10,19,.8), using premultiplied channels.
            draw.AddRectFilled(at,at+new float2(chipWidth,25),new ImColor8((byte)((tint.R*.14+.688)/.828),(byte)((tint.G*.14+6.88)/.828),(byte)((tint.B*.14+13.072)/.828),211));
            draw.AddRect(at,at+new float2(chipWidth,25),new ImColor8((byte)tint.R,(byte)tint.G,(byte)tint.B,115));
            var medal=trait.Lv>=3?new ImColor8(255,207,74):trait.Lv==2?new ImColor8(200,210,224):new ImColor8(201,138,82);
            draw.AddNgonFilled(at+new float2(10,12.5f),6.5f,medal,6);
            draw.AddText(at+new float2(21,5),new ImColor8(239,231,216),label);
            if(ImGui.IsItemHovered())ResultTooltip(text.Get(validRules.Id,trait.Id).Name);
        }
        rowWidth=0;
        foreach(var unit in (record.Units ?? []).Take(9))
        {
            var line=validRules.Lines.Concat(validRules.ExtraLines).FirstOrDefault(l=>l.Id==unit.Line);
            if(line is null || unit.Stage<0 || unit.Stage>=line.Forms.Length)continue;
            if(rowWidth>0 && rowWidth+portraitSize+5<=width){ImGui.SameLine(0,5);rowWidth+=5;}else rowWidth=0;
            rowWidth+=portraitSize;var position=ImGui.GetCursorScreenPos(); ImGui.Dummy(new(portraitSize,portraitSize));
            portraits.Draw(validRules.Id,unit.Line,unit.Stage,position,portraitSize); PostgameUnitPortraits++;
            if(ImGui.IsItemHovered())ResultTooltip(line.Forms[unit.Stage]+" "+new string('*',unit.Stage+1));
            ImGui.GetWindowDrawList().AddText(position+new float2(2,portraitSize-16),new ImColor8(255,220,128),new string('*',unit.Stage+1));
        }
        ImGui.PopStyleVar(); ImGui.PopFont();
    }

    private static string ResultTrim(string value,float width)
    {
        if(ImGui.CalcTextSize(value).X<=width)return value;
        while(value.Length>0 && ImGui.CalcTextSize(value+"...").X>width)value=value[..^1];
        return value+"...";
    }
    private static void ResultTooltip(string value) { ImGui.BeginTooltip(); ImGui.Text(value); ImGui.EndTooltip(); }
    private static (double R,double G,double B) ResultTraitTint(string hex)
    {
        int rgb=int.Parse(hex.AsSpan(1),System.Globalization.NumberStyles.HexNumber,System.Globalization.CultureInfo.InvariantCulture);
        return ((rgb>>16)&255,(rgb>>8)&255,rgb&255);
    }

    private string? ResultCompanion(MatchPlayer player)
    {
        try
        {
            System.Text.Json.Nodes.JsonObject? look=null;
            if(player.IsHuman && player.Id==localSeat) look=CosmeticLoadouts.Local(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile?.Storage.GetValueOrDefault("dt.legendevo.v1"));
            else if(player.IsHuman && networkRoom is not null)
            {
                string? uid=networkRoom.Authority.Lobby?["slots"]?[player.Id.ToString()]?["uid"]?.GetValue<string>();
                if(uid is not null)look=networkRoom.Authority.Presence[uid]?["look"] as System.Text.Json.Nodes.JsonObject;
            }
            if(look is null && !aiLooks.TryGetValue((player.Id,player.Name),out look))aiLooks[(player.Id,player.Name)]=look=CosmeticLoadouts.Ai(player.Id,player.Name);
            return CosmeticLoadouts.CompanionKey(look);
        }
        catch(Exception error) when(error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException){return null;}
    }

    private void StartLocalMatch(bool duo)
    {
        if(networkRoom is not null || steam?.Room is not null)return;
        if(Session is not null)LeaveMatch();
        audio.PlayClient("fight",false);
        uint matchSeed=seed??(uint)Random.Shared.Next(1_000_000_000);
        var resolved=SetChoices.Resolve(selectedSet,selectedPace,matchSeed);var rules=SetSelection.Rules(catalog,resolved);
        variant=Array.IndexOf(catalog.Variants,rules); duos=duo;
        Session=new(rules,matchSeed,Enumerable.Range(0,8).Select(i=>new MatchSeat(i==0?profile?.Progress.Name??"You":$"Tamer {i}",i==0)).ToArray(),duo,chaos:resolved.Chaos,chaosPace:resolved.ChaosPace);
        history=profile is null?null:new(Session,profile); localPaused=showGameMenu=confirmLeave=false;
        viewed=localSeat=0; CompletedRounds=0; selectedUnit=selectedItem=null; backgroundTime=0;
        audio.Music("planning"); status="Select a unit, then a hex. Select an item, then its holder.";
    }
}
