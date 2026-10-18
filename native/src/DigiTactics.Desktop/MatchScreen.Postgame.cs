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
    internal bool PostgameHistoryVisible => ProfileVisible && Session is not null;
    internal bool PostgameHomeVisible => Session is not null && ReferenceEquals(dismissedHomeSession,Session);
    private bool ClientHomeVisible => Session is null || PostgameHomeVisible;
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
        float margin = 24, width = size.X - margin * 2, height = size.Y - margin * 2;
        Window("Match results", margin, margin, width, height, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        var origin = ImGui.GetWindowPos(); var draw = ImGui.GetWindowDrawList();
        postgameArtwork.Draw(gold && !outNow ? "postgame-bg" : "loading-bg", origin, new(width,height), 255, true, .3f);
        var topTint=new ImColor8(1,10,19,140); var bottomTint=new ImColor8(1,10,19,235);
        draw.AddRectFilledMultiColor(origin,origin+new float2(width,height*.55f),topTint,topTint,bottomTint,bottomTint);
        draw.AddRectFilled(origin+new float2(0,height*.55f),origin+new float2(width,height),bottomTint);
        draw.AddRect(origin, origin + new float2(width,height), gold?new ImColor8(200,170,110):new ImColor8(120,90,40), 8, ImDrawFlags.None, 1);
        ImGui.SetCursorPos(new(width-46,10));
        if(Button("resultDismiss","X",30,30)) { DismissResults(); ImGui.End(); return true; }
        ImGui.SetCursorPos(new(24,22));
        ImGui.BeginGroup();
        var emblem = ImGui.GetCursorScreenPos();
        if (!IsSpectating) postgameArtwork.Draw(place == 1 ? "emblem-mega" : place <= 2 ? "emblem-ultimate" : place <= 4 ? "emblem-champion" : "emblem-rookie", emblem, new(130,130));
        ImGui.Dummy(new(150,195));
        if(!IsSpectating)
        {
            string number=$"#{place}",label=PostgameSummary.PlaceLabel(place,Session.State.Duos).ToUpperInvariant();
            float numberWidth=ImGui.CalcTextSize(number).X*46/ImGui.GetFontSize(),labelWidth=ImGui.CalcTextSize(label).X*12/ImGui.GetFontSize();
            draw.AddText(ImGui.GetFont(),46,emblem+new float2((130-numberWidth)/2,116),accent,number);
            draw.AddText(ImGui.GetFont(),12,emblem+new float2((130-labelWidth)/2,166),new ImColor8(200,170,110),label);
        }
        ImGui.EndGroup(); ImGui.SameLine();
        ImGui.BeginGroup();
        ImGui.Text($"{(networkRoom is null ? "Solo vs AI" : "Multiplayer")} / {MatchSetTitle} / {MatchPaceTitle} / results");
        ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(240,230,210)); ImGui.PushFont(default,Math.Clamp(size.X*.04f,38,64)); ImGui.Text(title.ToUpperInvariant()); ImGui.PopFont(); ImGui.PopStyleColor();
        var record = history?.Match;
        var winningPlayer = Session.State.Players.FirstOrDefault(p => p.Placement == 1);
        string? winner = winningPlayer is not null && winningPlayer.Id != localSeat ? winningPlayer.Name : null;
        UiText.Wrapped(outNow ? "The others are still playing. Spectate their fights or leave."
            : $"Reached round {Session.Rounds.Position.Label} at level {Human.Economy.Level}." + (winner is null ? "" : " Winner: " + winner + "."));
        if (!IsSpectating && record is not null) ResultComposition(record, Rules.Id, width-240);
        ImGui.EndGroup();
        float colsY = 253, columnsHeight = height - colsY - 76, columnWidth = (width - 72) / 3;
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(1,10,19,204));
        ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(70,55,20));
        ImGui.SetCursorPos(new(24,colsY));
        ImGui.BeginChild("Result standings"u8,new(columnWidth,columnsHeight),ImGuiChildFlags.Borders);
        ImGui.SeparatorText("Standings"u8);
        foreach (var player in Session.State.Players.OrderBy(p => p.Placement))
        {
            var at = ImGui.GetCursorScreenPos();
            if (player.Id == localSeat)
            {
                var tint=new ImColor8(200,170,110,46);var clear=new ImColor8(200,170,110,0);
                ImGui.GetWindowDrawList().AddRectFilledMultiColor(at-new float2(2,1),at+new float2(columnWidth-28,34),tint,clear,clear,tint);
                ImGui.GetWindowDrawList().AddLine(at-new float2(2,1),at+new float2(-2,34),new ImColor8(200,170,110),2);
            }
            ImGui.BeginGroup();
            ImGui.Text(player.Placement > 0 ? $"#{player.Placement}" : "-"); ImGui.SameLine(34);
            var avatar=ImGui.GetCursorScreenPos(); ImGui.Dummy(new(26,26));
            if(portraits.DrawCompanion(ResultCompanion(player),avatar,26))PostgameAvatarCount++;
            ImGui.SameLine();
            string name = (Session.State.Duos ? $"Team {player.Team+1} / " : "") + player.Name;
            float maxName = Math.Max(40,columnWidth-171);
            string shown = ResultTrim(name,maxName);
            ImGui.Text(shown); if (ImGui.IsItemHovered()) ResultTooltip(name);
            ImGui.SameLine(columnWidth-96);
            ImGui.Text(player.Placement == 1 ? "Winner" : player.Placement > 0 ? "Eliminated" : $"{Math.Max(0,player.Health)} HP");
            ImGui.Dummy(new(0,2)); ImGui.EndGroup(); PostgameStandings++;
        }
        ImGui.EndChild();
        ImGui.SameLine();
        ImGui.BeginChild("Result progress"u8,new(columnWidth,columnsHeight),ImGuiChildFlags.Borders);
        ImGui.SeparatorText("Progress"u8);
        if (profile is null || IsSpectating) UiText.Wrapped(IsSpectating ? "Spectating does not award match progress." : "No player profile is attached to this session.");
        else DrawResultProgress();
        ImGui.EndChild(); ImGui.SameLine();
        ImGui.BeginChild("Result rewards"u8,new(columnWidth,columnsHeight),ImGuiChildFlags.Borders);
        ImGui.SeparatorText("Bits earned"u8);
        if (history?.Award is { } award)
        {
            if(!ReferenceEquals(postgameAnimatedAward,award)) {postgameAnimatedAward=award;postgameAwardStart=boardSceneTime;postgameAwardSounds=0;}
            double time=boardSceneTime-postgameAwardStart;
            PostgameDisplayedBits=history.FreshAward?PostgameAwardCount.Total(award.Total,award.Lines.Length,time):award.Total;
            ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(245,207,109)); ImGui.PushFont(default,32); ImGui.Text($"+{PostgameDisplayedBits.Value} Bits"); ImGui.PopFont(); ImGui.PopStyleColor();
            if(!history.FreshAward)ImGui.Text("Already added"u8);
            for(int i=0;i<award.Lines.Length;i++)
            {
                var line=award.Lines[i]; int count=history.FreshAward?PostgameAwardCount.Line(line.N,i,time):line.N;
                UiText.Wrapped($"{line.Label}  +{count}"); ImGui.Separator();
            }
            if (history.LegendAward is { } legend) UiText.Wrapped($"{CosmeticCollection.Item("legend",legend.Legend)?.Name ?? legend.Legend}: +{legend.Data} Digi-Data (level {legend.Level})");
            ImGui.Separator();ImGui.Text($"Wallet: {profile?.Progress.Bits ?? 0} Bits");
            if(Button("resultStore","Store",130,30)){showCollection=true;postgameStore=true;}
        }
        else UiText.Wrapped(IsSpectating ? "Join a seat to earn rewards in your next match." : history?.Error ?? (networkRoom is null ? "Rewards are unavailable for this session." : "Verifying the shared match before saving rewards..."));
        ImGui.BeginDisabled(networkRoom is not null && networkReplica?.LastDesync is not null);
        if (history?.Error is not null && Button("retryHistory","Retry saving progress")) history.Complete(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        ImGui.EndDisabled();
        ImGui.EndChild();
        ImGui.PopStyleColor(2);
        ImGui.SetCursorPos(new(24,height-56));
        ImGui.BeginDisabled(profile is null); if(Button("resultHistory","Match history",180,36)) OpenProfile(); ImGui.EndDisabled();
        ImGui.SameLine(width-390);
        if(Button("menu",outNow?"Leave":"Home",160,36))
        {
            if(outNow){bool online=networkRoom is not null;LeaveMatch();showMultiplayer=online;}
            else DismissResults();
        }
        ImGui.SameLine();
        if (Session is not null)
        {
            if (outNow)
            {
                if(Button("spectateRemaining","Spectate",190,36)) { postgameDismissed=true; if(networkRoom is null) ContinueAfterElimination(); }
            }
            else if(Button("resultAgain",networkRoom is null?"Play again":"Back to lobbies",190,36))
            {
                bool online=networkRoom is not null; bool nextDuos=Session.State.Duos;
                LeaveMatch();
                if(online) showMultiplayer=true;
                else StartLocalMatch(nextDuos);
            }
        }
        ImGui.End(); return true;
    }

    private void DrawResultProgress()
    {
        try
        {
            var level = TamerProgress.Read(profile!.Storage.GetValueOrDefault("dt.client.v1"));
            ImGui.Text($"Tamer level {level.Level}" + (history?.LeveledUp == true ? " / Level up!" : ""));
            ImGui.Text(history?.XpGained > 0 ? $"+{history.XpGained} XP" : "");
            ImGui.ProgressBar((float)((double)level.Xp/level.Need),new(-1,10), ""u8);
            ImGui.Text($"{level.Xp} / {level.Need} XP"); ImGui.SeparatorText("Daily missions"u8);
            var daily=MatchRewards.Progress(profile.Storage.GetValueOrDefault("dt.shop.v1"),profile.Storage.GetValueOrDefault("dt.client.v1"),DateOnly.FromDateTime(DateTime.Now));
            foreach(var mission in daily.Missions)
            {
                UiText.Wrapped($"{mission.Name} / {mission.Bits} Bits");
                ImGui.ProgressBar((float)mission.Current/mission.Goal,new(-1,8), ""u8);
                ImGui.Text(mission.Done ? "Complete" : $"Best today {mission.Current}/{mission.Goal}"); PostgameMissionCount++;
            }
            ImGui.Separator(); ImGui.Text($"Daily bonus / {daily.Bits} Bits");
            UiText.Wrapped(daily.Claimed ? "Claimed. Back tomorrow." : "Complete your first game today.");
        }
        catch(Exception error) when(error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or ArgumentOutOfRangeException)
        { UiText.Wrapped("Saved progress could not be read. Your data has been preserved."u8); }
    }

    private void UpdatePostgameAward()
    {
        if(postgameAnimatedAward is not { } award || !ReferenceEquals(postgameSession,Session) || history?.FreshAward!=true)return;
        int completed=PostgameAwardCount.CompletedRows(award.Lines.Length,boardSceneTime-postgameAwardStart);
        while(postgameAwardSounds<completed)audio.Play(++postgameAwardSounds>award.Lines.Length?"pick":"tick_normal");
    }

    private void ResultComposition(HistoryMatch record,string variantId,float width)
    {
        var validRules=catalog.Variants.FirstOrDefault(r=>r.Id==variantId) ?? catalog.Variants.FirstOrDefault(r=>r.Id.Split('/')[0]==record.Set);
        if(validRules is null) return;
        float rowWidth=0;
        ImGui.PushFont(default,12);
        foreach(var trait in (record.Traits ?? []).Take(6).Where(t=>validRules.Traits.Any(x=>x.Id==t.Id)))
        {
            string label=$"{text.Get(validRules.Id,trait.Id).Name} {trait.N}";
            float chipWidth=ImGui.CalcTextSize(label).X+30;
            if(rowWidth>0 && rowWidth+chipWidth+5<=width){ImGui.SameLine();rowWidth+=5;}else rowWidth=0;
            rowWidth+=chipWidth;var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(chipWidth,21));
            var tint=ResultTraitTint(resultTraitColors[validRules.Id].GetValueOrDefault(trait.Id,"#9aa4b1"));
            var draw=ImGui.GetWindowDrawList();
            // CSS mixes 14% opaque trait color with 86% rgba(1,10,19,.8), using premultiplied channels.
            draw.AddRectFilled(at,at+new float2(chipWidth,21),new ImColor8((byte)((tint.R*.14+.688)/.828),(byte)((tint.G*.14+6.88)/.828),(byte)((tint.B*.14+13.072)/.828),211));
            draw.AddRect(at,at+new float2(chipWidth,21),new ImColor8((byte)tint.R,(byte)tint.G,(byte)tint.B,115));
            var medal=trait.Lv>=3?new ImColor8(255,207,74):trait.Lv==2?new ImColor8(200,210,224):new ImColor8(201,138,82);
            draw.AddNgonFilled(at+new float2(10,10.5f),6.5f,medal,6);
            draw.AddText(at+new float2(21,3),new ImColor8(239,231,216),label);
            if(ImGui.IsItemHovered())ResultTooltip(text.Get(validRules.Id,trait.Id).Name);
        }
        ImGui.PopFont();
        int index=0;
        foreach(var unit in (record.Units ?? []).Take(9))
        {
            var line=validRules.Lines.Concat(validRules.ExtraLines).FirstOrDefault(l=>l.Id==unit.Line);
            if(line is null || unit.Stage<0 || unit.Stage>=line.Forms.Length)continue;
            if(index++>0)ImGui.SameLine(); var position=ImGui.GetCursorScreenPos(); ImGui.Dummy(new(52,52));
            portraits.Draw(validRules.Id,unit.Line,unit.Stage,position,52); PostgameUnitPortraits++;
            if(ImGui.IsItemHovered())ResultTooltip(line.Forms[unit.Stage]+" "+new string('*',unit.Stage+1));
            ImGui.GetWindowDrawList().AddText(position+new float2(3,34),new ImColor8(255,220,128),new string('*',unit.Stage+1));
        }
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
