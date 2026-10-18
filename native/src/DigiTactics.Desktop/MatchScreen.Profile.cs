using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showProfile,chooseProfileAvatar;
    private readonly ImInputString profileName=new(128);
    private string? profileEditMessage;
    internal bool ProfileVisible=>showProfile;
    internal ProfileSummary? DisplayedProfile {get;private set;}
    internal int ProfileHistoryRows {get;private set;}
    internal int ProfilePortraits {get;private set;}
    internal int ProfileMissions {get;private set;}
    internal float ProfileHistoryScroll {get;private set;}
    internal float ProfileStatsScroll {get;private set;}
    internal void OpenProfile()
    {
        if(profile is null)return;
        profileName.SetValue((steam?.Name??profile.Storage.GetValueOrDefault("dt.name","")).AsSpan());
        showProfile=true;chooseProfileAvatar=false;profileEditMessage=null;
    }
    private void ChangeProfile(string field,string value)
    {
        var collection=new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
        string saved=ProfileSummary.Customize(profile.Storage.GetValueOrDefault("dt.client.v1"),field,value,collection);
        profileEditMessage=profile.UpdateStorage(new Dictionary<string,string>{{"dt.client.v1",saved}})?null:profile.Error;
        if(profileEditMessage is null)audio.PlayClient("equip",false);
    }
    private void ProfilePanel()
    {
        ProfileHistoryRows=ProfilePortraits=ProfileMissions=0;DisplayedProfile=null;
        float width=size.X-48,height=size.Y-48;
        Window("Tamer profile",24,24,width,height,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar);
        if(Button("closeProfile","Back",110,32) || ImGui.IsKeyPressed(ImGuiKey.Escape))showProfile=false;
        ImGui.SameLine();ImGui.Text("TAMER PROFILE"u8);
        try
        {
            string? client=profile!.Storage.GetValueOrDefault("dt.client.v1"),wallet=profile.Storage.GetValueOrDefault("dt.cosmetics.v1");
            var summary=ProfileSummary.Read(client,wallet);DisplayedProfile=summary;
            var collection=new CosmeticCollection(wallet);var level=TamerProgress.Read(client);
            var bannerPosition=ImGui.GetCursorScreenPos();float bannerWidth=width-24;
            ImGui.Dummy(new(bannerWidth,132));postgameArtwork.Draw(summary.Banner,bannerPosition,new(bannerWidth,132),255,true);
            ImGui.GetWindowDrawList().AddRectFilled(bannerPosition,bannerPosition+new float2(bannerWidth,132),new ImColor8(6,15,27,150));
            string? avatar=summary.Avatar is { } owned && collection.Owns("legend",owned)?owned:collection.Equipped("legend")?.GetValue<string>();
            ImGui.SetCursorScreenPos(bannerPosition+new float2(12,13));
            if(Button("profileAvatar","##profileAvatar",92,92))chooseProfileAvatar=!chooseProfileAvatar;
            if(avatar is not null)DrawProfileAvatar(avatar,bannerPosition+new float2(18,19),80);
            ImGui.GetWindowDrawList().AddText(bannerPosition+new float2(20,109),new ImColor8(240,225,180),$"Level {level.Level}");
            ImGui.SetCursorScreenPos(bannerPosition+new float2(120,12));ImGui.BeginGroup();
            ImGui.Text($"Tamer / level {level.Level}");ImGui.SetNextItemWidth(Math.Max(180,bannerWidth-460));
            bool persona=steam is not null;
            ImGui.InputText("##profileName"u8,profileName,persona?ImGuiInputTextFlags.ReadOnly:ImGuiInputTextFlags.None);
            Buttons["profileName"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
            if(!persona && ImGui.IsItemDeactivatedAfterEdit())
            {
                string cleaned=ProfileSummary.CleanName(profileName.ToString());
                if(profile.UpdateStorage(new Dictionary<string,string>{{"dt.name",cleaned}})) {profileName.SetValue(cleaned.AsSpan());profileEditMessage=cleaned.Length>0?"Name saved: "+cleaned:"Name cleared";}
                else profileEditMessage=profile.Error;
            }
            if(persona && ImGui.IsItemHovered())ResultTooltip("Your Steam name");
            ImGui.Text($"{level.Xp} / {level.Need} XP to level {level.Level+1}");
            ImGui.ProgressBar((float)((double)level.Xp/level.Need),new(Math.Max(180,bannerWidth-460),8),""u8);
            ImGui.EndGroup();
            var rankPosition=bannerPosition+new float2(bannerWidth-310,14);
            postgameArtwork.Draw("emblem-"+summary.Rank.Id,rankPosition,new(64,64),summary.Rank.Unranked?(byte)120:(byte)255);
            ImGui.SetCursorScreenPos(rankPosition+new float2(74,4));ImGui.BeginGroup();ImGui.Text(summary.Rank.Label);
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+215);ImGui.Text(summary.Rank.Detail);ImGui.PopTextWrapPos();ImGui.EndGroup();
            ImGui.SetCursorScreenPos(bannerPosition+new float2(bannerWidth-306,88));
            foreach(string banner in ProfileSummary.Banners)
            {
                if(banner!="profile-1")ImGui.SameLine();var position=ImGui.GetCursorScreenPos();
                if(Button("profileBanner/"+banner,"##"+banner,64,30,summary.Banner==banner))ChangeProfile("banner",banner);
                postgameArtwork.Draw(banner,position+new float2(3,3),new(58,24),255,true);
                if(ImGui.IsItemHovered())ResultTooltip("Use this banner");
            }
            ImGui.SetCursorScreenPos(bannerPosition+new float2(0,140));
            if(profileEditMessage is not null) {UiText.Wrapped(profileEditMessage);}
            float bodyHeight=Math.Max(150,height-ImGui.GetCursorPosY()-14);
            if(chooseProfileAvatar)ProfileAvatarChoices(collection,bodyHeight);
            else
            {
                float leftWidth=Math.Clamp((width-36)*.34f,310,410);
                ImGui.BeginChild("Profile statistics"u8,new(leftWidth,bodyHeight),ImGuiChildFlags.Borders);
                ProfileStats(summary,leftWidth-24);ProfileDailyMissions();ProfileStatsScroll=ImGui.GetScrollY();ImGui.EndChild();ImGui.SameLine();
                ImGui.BeginChild("Profile history"u8,new(0,bodyHeight),ImGuiChildFlags.Borders);
                ProfileHistory(summary,width-leftWidth-66);ProfileHistoryScroll=ImGui.GetScrollY();ImGui.EndChild();
            }
        }
        catch(Exception error) when(error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or ArgumentOutOfRangeException)
        {UiText.Wrapped("Profile could not be read. Your saved data has been preserved."u8);}
        ImGui.End();
    }
    private void DrawProfileAvatar(string id,float2 position,float pixels)
    {
        var discovered=LegendEvolution.Discovery(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),id);
        portraits.DrawEvolution(discovered.Current,position,pixels,true);ProfilePortraits++;
    }
    private void ProfileAvatarChoices(CosmeticCollection collection,float height)
    {
        if(Button("profileAvatarBack","Back to profile"))chooseProfileAvatar=false;
        UiText.Wrapped("Choose your icon. Any Little Legend you own."u8);
        ImGui.BeginChild("Profile avatar choices"u8,new(0,height-70));
        int column=0,columns=Math.Max(1,(int)(ImGui.GetContentRegionAvail().X/120));
        foreach(var item in CosmeticCollection.Items("legend").Where(x=>collection.Owns("legend",x.Id)))
        {
            if(column++%columns!=0)ImGui.SameLine();ImGui.BeginGroup();var position=ImGui.GetCursorScreenPos();
            if(Button("profileAvatar/"+item.Id,"##avatar"+item.Id,110,92)) {ChangeProfile("avatar",item.Id);chooseProfileAvatar=false;}
            DrawProfileAvatar(item.Id,position+new float2(13,4),84);
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+110);ImGui.Text(item.Name);ImGui.PopTextWrapPos();ImGui.EndGroup();
        }
        ImGui.EndChild();
    }
    private void ProfileStats(ProfileSummary summary,float width)
    {
        ImGui.Text($"{summary.Games} Games     {summary.Wins} Wins");
        ImGui.Text($"{summary.TopFour} Top finishes     {(summary.Games>0?summary.Average.ToString("F2"):"-")} Avg place");
        ImGui.SeparatorText("Placements"u8);ImGui.Text($"last {summary.Matches.Length}");
        if(summary.Matches.Length==0)UiText.Wrapped("No games yet."u8);else DrawPlacementChart(summary,width);
        ImGui.SeparatorText("Trend / oldest to newest"u8);
        if(summary.Trend.Length<2)UiText.Wrapped("Play two games to see your trend."u8);else DrawPlacementTrend(summary,width);
        ImGui.SeparatorText("Favorite traits"u8);
        int traits=0;
        foreach(var favorite in summary.Traits.Where(f=>Rules.Traits.Any(t=>t.Id==f.Id))) {UiText.Wrapped($"{text.Get(Rules.Id,favorite.Id).Name} / {favorite.Games} game{(favorite.Games>1?"s":"")}");traits++;}
        if(traits==0)UiText.Wrapped("Play a game to see your favorite traits."u8);
        ImGui.SeparatorText("Favorite Digimon"u8);
        int units=0;
        foreach(var favorite in summary.Units)
        {
            var line=Rules.Lines.FirstOrDefault(l=>l.Id==favorite.Id);if(line is null)continue;
            var position=ImGui.GetCursorScreenPos();ImGui.Dummy(new(42,42));portraits.Draw(Rules.Id,line.Id,0,position,42);ProfilePortraits++;
            ImGui.SameLine();ImGui.BeginGroup();ImGui.Text(line.Forms[0]);ImGui.Text($"{favorite.Games} game{(favorite.Games>1?"s":"")}");ImGui.EndGroup();units++;
        }
        if(units==0)UiText.Wrapped("Play a game to see the Digimon you field most."u8);
    }
    private void DrawPlacementChart(ProfileSummary summary,float width)
    {
        var origin=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();int max=Math.Max(1,summary.Placements.Max());
        float unit=width/320,bar=30*unit,gap=80f/7*unit;
        draw.AddLine(origin+new float2(0,102),origin+new float2(width,102),new ImColor8(90,110,130));
        for(int i=0;i<8;i++)
        {
            int count=summary.Placements[i];float h=count>0?Math.Max(4,86f*count/max):0,x=i*(bar+gap),y=102-h;
            var color=i==0?new ImColor8(245,207,109):i<4?new ImColor8(88,217,204):new ImColor8(133,151,170);
            if(h>0) {draw.AddRectFilled(origin+new float2(x,y),origin+new float2(x+bar,102),color,4);draw.AddText(origin+new float2(x+bar*.5f-4,y-18),color,count.ToString());}
            draw.AddText(origin+new float2(x+bar*.5f-4,105),new ImColor8(180,193,210),(i+1).ToString());
            if(ImGui.IsMouseHoveringRect(origin+new float2(x-gap*.5f,0),origin+new float2(x+bar+gap*.5f,120)))ResultTooltip($"#{i+1}: {count} game{(count==1?"":"s")}");
        }
        ImGui.Dummy(new(width,128));
    }
    private void DrawPlacementTrend(ProfileSummary summary,float width)
    {
        var origin=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        float2 Point(int i)=>origin+new float2(14+(width-28)*i/(summary.Trend.Length-1),8+94f*(summary.Trend[i].Place-1)/7);
        draw.AddLine(origin+new float2(0,55),origin+new float2(width,55),new ImColor8(70,88,108));
        draw.AddText(origin+new float2(width-42,37),new ImColor8(155,170,190),"top 4"u8);
        draw.AddText(origin+new float2(0,-6),new ImColor8(155,170,190),"1st"u8);draw.AddText(origin+new float2(0,102),new ImColor8(155,170,190),"8th"u8);
        for(int i=0;i<summary.Trend.Length;i++)
        {
            var match=summary.Trend[i];var point=Point(i);
            if(i>0)draw.AddLine(Point(i-1),point,new ImColor8(115,165,210),2);
            draw.AddCircleFilled(point,4,match.Place<=(match.Format=="duos"?2:4)?new ImColor8(88,217,204):new ImColor8(150,165,190));
            if(ImGui.IsMouseHoveringRect(point-new float2(7,7),point+new float2(7,7)))ResultTooltip($"Game {i+1}: #{match.Place} ({(match.Mode=="mp"?"multiplayer":"solo")})");
        }
        ImGui.Dummy(new(width,120));
    }
    private void ProfileDailyMissions()
    {
        ImGui.SeparatorText("Missions / reset at midnight"u8);
        var daily=MatchRewards.Progress(profile!.Storage.GetValueOrDefault("dt.shop.v1"),profile.Storage.GetValueOrDefault("dt.client.v1"),DateOnly.FromDateTime(DateTime.Now));
        foreach(var mission in daily.Missions)
        {
            UiText.Wrapped($"{mission.Name} / {mission.Bits} Bits");ImGui.ProgressBar((float)mission.Current/mission.Goal,new(-1,8),""u8);
            UiText.Wrapped(mission.Done?"Complete":mission.Goal>1?$"Best today {mission.Current}/{mission.Goal} / in one game":"Complete it in one game");ProfileMissions++;
        }
        ImGui.SeparatorText("Daily bonus"u8);UiText.Wrapped(daily.Claimed?"Claimed. Back tomorrow.":$"First game today pays {daily.Bits} extra Bits.");
    }
    private void ProfileHistory(ProfileSummary summary,float width)
    {
        ImGui.SeparatorText("Match history"u8);ImGui.Text($"last {summary.Matches.Length}");
        if(summary.Matches.Length==0)UiText.Wrapped("No games yet. Your last 30 games show up here with placement, traits and Digimon."u8);
        foreach(var match in summary.Matches)
        {
            var color=match.Place==1?new ImColor8(245,207,109):match.Place<=(match.Format=="duos"?2:4)?new ImColor8(88,217,204):new ImColor8(220,164,164);
            ImGui.PushStyleColor(ImGuiCol.Text,color);ImGui.Text($"#{match.Place} / {PostgameSummary.PlaceLabel(match.Place,match.Format=="duos")} / {(match.Mode=="mp"?"Multi":"Solo")}");ImGui.PopStyleColor();
            string mode=catalog.Variants.FirstOrDefault(r=>r.Id.Split('/')[0]==match.Set)?.Name??match.Set??"";
            UiText.Wrapped($"{(match.Format=="duos"?"Duos / ":"")}{mode} / round {match.Round} / level {match.Level}");
            UiText.Wrapped((DateTimeOffset.FromUnixTimeMilliseconds(match.At).LocalDateTime.ToString("MMM d")+" "+DateTimeOffset.FromUnixTimeMilliseconds(match.At).LocalDateTime.ToString("t"))+(match.Bits is { } bits?$" / +{bits} Bits":""));
            var traits=match.Traits.Take(6).Where(t=>Rules.Traits.Any(x=>x.Id==t.Id)).Select(t=>$"{text.Get(Rules.Id,t.Id).Name} {t.N}").ToArray();
            UiText.Wrapped(traits.Length>0?string.Join(" / ",traits):"No active traits");
            int row=0,columns=Math.Max(1,(int)(width/48));
            foreach(var unit in match.Units.Take(8))
            {
                var line=Rules.Lines.FirstOrDefault(l=>l.Id==unit.Line);if(line is null || unit.Stage<0 || unit.Stage>=line.Forms.Length)continue;
                if(row++%columns!=0)ImGui.SameLine();var position=ImGui.GetCursorScreenPos();ImGui.Dummy(new(42,42));
                portraits.Draw(Rules.Id,unit.Line,unit.Stage,position,42);ProfilePortraits++;
                ImGui.GetWindowDrawList().AddText(position+new float2(2,25),new ImColor8(255,220,128),new string('*',unit.Stage+1));
                if(ImGui.IsItemHovered())ResultTooltip(line.Forms[unit.Stage]+" "+new string('*',unit.Stage+1));
            }
            ImGui.Separator();ProfileHistoryRows++;
        }
        ImGui.SeparatorText("Bits history"u8);ImGui.Text($"{profile!.Progress.Bits??0:N0} Bits");
        if(summary.BitsHistory.Length==0)UiText.Wrapped("No Bits earned yet. Finish a game to earn some."u8);
        foreach(var entry in summary.BitsHistory) {UiText.Wrapped($"{entry.Reason} / +{entry.N} / {DateTimeOffset.FromUnixTimeMilliseconds(entry.At).LocalDateTime:d}");ImGui.Separator();}
    }
}
