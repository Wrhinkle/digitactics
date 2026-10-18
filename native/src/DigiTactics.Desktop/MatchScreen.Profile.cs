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
    internal float2 ProfileStatsCenter {get;private set;}
    internal float2 ProfileHistoryCenter {get;private set;}
    internal float ProfileContentBottom {get;private set;}
    internal float ProfileHistoryOverflow {get;private set;}
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
        ProfileHistoryRows=ProfilePortraits=ProfileMissions=0;ProfileHistoryOverflow=0;DisplayedProfile=null;
        float width=size.X-32,height=size.Y-ClientPageTop-16;
        ClientPageWindow("Tamer profile",16,ClientPageTop,width,height);
        try
        {
            string? client=profile!.Storage.GetValueOrDefault("dt.client.v1"),wallet=profile.Storage.GetValueOrDefault("dt.cosmetics.v1");
            var summary=ProfileSummary.Read(client,wallet);DisplayedProfile=summary;
            var collection=new CosmeticCollection(wallet);var level=TamerProgress.Read(client);
            var bannerPosition=ImGui.GetCursorScreenPos();float bannerWidth=ImGui.GetContentRegionAvail().X,bannerHeight=Math.Clamp(size.Y*.24f,170,210);
            ImGui.Dummy(new(bannerWidth,bannerHeight));postgameArtwork.Draw(summary.Banner,bannerPosition,new(bannerWidth,bannerHeight),255,true,.4f);
            var bannerDraw=ImGui.GetWindowDrawList();
            for(int strip=0;strip<20;strip++)bannerDraw.AddRectFilled(bannerPosition+new float2(0,bannerHeight*strip/20),bannerPosition+new float2(bannerWidth,bannerHeight*(strip+1)/20),new ImColor8(1,10,19,(byte)(40+strip*10)));
            bannerDraw.AddRect(bannerPosition,bannerPosition+new float2(bannerWidth,bannerHeight),ClientGold);
            ImGui.SetCursorScreenPos(bannerPosition+new float2(10,8));
            if(HomeButton("closeProfile","Back",90,36)||!chooseProfileAvatar&&!KeyboardPopup&&ImGui.IsKeyPressed(ImGuiKey.Escape))showProfile=false;
            string? avatar=summary.Avatar is { } owned && collection.Owns("legend",owned)?owned:collection.Equipped("legend")?.GetValue<string>();
            var avatarAt=bannerPosition+new float2(20,bannerHeight-126);ImGui.SetCursorScreenPos(avatarAt);
            ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(0,0,0,0));if(Button("profileAvatar","##profileAvatar",112,112)){chooseProfileAvatar=true;ImGui.OpenPopup("Choose your icon"u8);}ImGui.PopStyleColor();
            if(ImGui.IsItemHovered())ResultTooltip("Change avatar");
            var center=avatarAt+new float2(56,56);bannerDraw.AddCircleFilled(center,52,new ImColor8(10,20,40));bannerDraw.AddCircle(center,54,ClientBorder,64,4);
            for(int arc=0,count=(int)(64d*level.Xp/level.Need);arc<count;arc++)
            {double a=-Math.PI/2+arc*Math.Tau/64,b=-Math.PI/2+(arc+1)*Math.Tau/64;bannerDraw.AddLine(center+new float2((float)Math.Cos(a)*54,(float)Math.Sin(a)*54),center+new float2((float)Math.Cos(b)*54,(float)Math.Sin(b)*54),ClientGold,4);}
            if(avatar is not null)DrawProfileAvatar(avatar,avatarAt+new float2(14,14),84);
            bannerDraw.AddRectFilled(avatarAt+new float2(38,98),avatarAt+new float2(76,121),new ImColor8(1,10,19));bannerDraw.AddRect(avatarAt+new float2(38,98),avatarAt+new float2(76,121),ClientGold);
            bannerDraw.AddText(avatarAt+new float2(49,101),ClientInk,level.Level.ToString());
            float identityWidth=Math.Max(220,bannerWidth-460);
            ImGui.SetCursorScreenPos(bannerPosition+new float2(150,bannerHeight-114));ImGui.BeginGroup();
            ClientPageTitle($"TAMER / LEVEL {level.Level}",17);ImGui.SetNextItemWidth(identityWidth);
            bool persona=steam is not null;
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,25);ImGui.PushStyleColor(ImGuiCol.Text,ClientInk);
            ImGui.InputTextWithHint("##profileName"u8,"Your Tamer name"u8,profileName,persona?ImGuiInputTextFlags.ReadOnly:ImGuiInputTextFlags.None);
            ImGui.PopStyleColor();ImGui.PopFont();
            Buttons["profileName"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
            if(!persona && ImGui.IsItemDeactivatedAfterEdit())
            {
                string cleaned=ProfileSummary.CleanName(profileName.ToString());
                if(profile.UpdateStorage(new Dictionary<string,string>{{"dt.name",cleaned}})) {profileName.SetValue(cleaned.AsSpan());profileEditMessage=cleaned.Length>0?"Name saved: "+cleaned:"Name cleared";}
                else profileEditMessage=profile.Error;
            }
            if(persona && ImGui.IsItemHovered())ResultTooltip("Your Steam name");
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+identityWidth);ImGui.PushStyleColor(ImGuiCol.Text,ClientMuted);ImGui.Text($"{level.Xp} / {level.Need} XP to level {level.Level+1}. Other players see your name in lobbies.");ImGui.PopStyleColor();ImGui.PopTextWrapPos();
            ImGui.EndGroup();
            var rankPosition=bannerPosition+new float2(bannerWidth-280,bannerHeight-96);
            bannerDraw.AddRectFilled(rankPosition-new float2(6,4),rankPosition+new float2(268,78),new ImColor8(1,10,19,210));bannerDraw.AddRect(rankPosition-new float2(6,4),rankPosition+new float2(268,78),ClientBorder);
            postgameArtwork.Draw("emblem-"+summary.Rank.Id,rankPosition,new(64,64),summary.Rank.Unranked?(byte)120:(byte)255);
            ImGui.SetCursorScreenPos(rankPosition+new float2(74,4));ImGui.BeginGroup();ClientPageTitle(summary.Rank.Label.ToUpperInvariant(),19);
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+182);ImGui.Text(summary.Rank.Detail);ImGui.PopTextWrapPos();ImGui.EndGroup();
            ImGui.SetCursorScreenPos(bannerPosition+new float2(bannerWidth-224,8));
            foreach(string banner in ProfileSummary.Banners)
            {
                if(banner!="profile-1")ImGui.SameLine();var position=ImGui.GetCursorScreenPos();
                if(Button("profileBanner/"+banner,"##"+banner,48,36,summary.Banner==banner))ChangeProfile("banner",banner);
                postgameArtwork.Draw(banner,position+new float2(2,2),new(44,32),255,true);
                if(summary.Banner==banner)bannerDraw.AddRect(position,position+new float2(48,36),ClientGold,0,ImDrawFlags.None,2);
                if(ImGui.IsItemHovered())ResultTooltip("Use this banner");
            }
            ImGui.SetCursorScreenPos(bannerPosition+new float2(0,bannerHeight+12));
            if(profileEditMessage is not null) {UiText.Wrapped(profileEditMessage);}
            float bodyHeight=Math.Max(150,height-ImGui.GetCursorPosY()-14);
            {
                float leftWidth=Math.Clamp(size.X*.27f,300,400);
                ImGui.BeginChild("Profile statistics"u8,new(leftWidth,bodyHeight));ProfileStatsCenter=ImGui.GetWindowPos()+new float2(leftWidth/2,bodyHeight/2);
                ProfileStats(summary,leftWidth-24);ProfileDailyMissions();ProfileStatsScroll=ImGui.GetScrollY();ImGui.EndChild();ImGui.SameLine();
                ImGui.BeginGroup();float rightWidth=ImGui.GetContentRegionAvail().X;
                float bitsHeight=Math.Min(148,bodyHeight*.4f),historyHeight=Math.Max(1,bodyHeight-bitsHeight-ImGui.GetStyle().ItemSpacing.Y);
                ClientCardBegin("Profile history",new(rightWidth,historyHeight));ProfileHistoryCenter=ImGui.GetWindowPos()+new float2(rightWidth/2,Math.Min(90,historyHeight/2));
                ProfileHistory(summary,rightWidth-32);ProfileHistoryScroll=ImGui.GetScrollY();ImGui.EndChild();
                ClientCardBegin("Profile Bits",new(rightWidth,bitsHeight));ProfileBits(summary);ProfileContentBottom=ImGui.GetWindowPos().Y+ImGui.GetWindowSize().Y;ImGui.EndChild();ImGui.EndGroup();
            }
            if(chooseProfileAvatar||ImGui.IsPopupOpen("Choose your icon"u8))ProfileAvatarChoices(collection,bodyHeight);
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
        float width=Math.Min(860,size.X-48);height=Math.Min(540,size.Y-128);
        ImGui.SetNextWindowPos(new((size.X-width)/2,Math.Max(ClientPageTop+12,(size.Y-height)/2)));ImGui.SetNextWindowSize(new(width,height));
        if(!ImGui.BeginPopupModal("Choose your icon"u8,ref chooseProfileAvatar,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoResize))return;
        ClientCardHeading("CHOOSE YOUR ICON");
        if(HomeButton("profileAvatarBack","Back to profile",180,36))chooseProfileAvatar=false;
        UiText.Wrapped("Choose your icon. Any Little Legend you own."u8);
        ImGui.BeginChild("Profile avatar choices"u8,new(0,0));
        int column=0,columns=Math.Max(1,(int)(ImGui.GetContentRegionAvail().X/120));
        foreach(var item in CosmeticCollection.Items("legend").Where(x=>collection.Owns("legend",x.Id)))
        {
            if(column++%columns!=0)ImGui.SameLine();ImGui.BeginGroup();var position=ImGui.GetCursorScreenPos();
            if(Button("profileAvatar/"+item.Id,"##avatar"+item.Id,110,92)) {ChangeProfile("avatar",item.Id);chooseProfileAvatar=false;}
            DrawProfileAvatar(item.Id,position+new float2(13,4),84);
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+110);ImGui.Text(item.Name);ImGui.PopTextWrapPos();ImGui.EndGroup();
        }
        ImGui.EndChild();
        if(ImGui.IsKeyPressed(ImGuiKey.Escape,false))chooseProfileAvatar=false;
        if(!chooseProfileAvatar)ImGui.CloseCurrentPopup();ImGui.EndPopup();
    }
    private void ProfileStats(ProfileSummary summary,float width)
    {
        string[] values=[summary.Games.ToString(),summary.Wins.ToString(),summary.TopFour.ToString(),summary.Games>0?summary.Average.ToString("F2"):"-"];
        string[] labels=["GAMES","WINS","TOP FINISHES","AVG PLACE"];float tile=(ImGui.GetContentRegionAvail().X-8)/2;
        for(int i=0;i<4;i++){if(i%2==1)ImGui.SameLine();ClientCardBegin("Profile stat "+i,new(tile,78));ClientPageTitle(values[i],25,ClientInk);ClientPageTitle(labels[i],16,ClientMuted);ImGui.EndChild();}
        ImGui.Spacing();
        ClientCardBegin("Profile placements card",new(0,0));ClientCardHeading("PLACEMENTS");ImGui.Text($"last {summary.Matches.Length}");
        if(summary.Matches.Length==0)UiText.Wrapped("No games yet."u8);else DrawPlacementChart(summary,ImGui.GetContentRegionAvail().X);ImGui.EndChild();
        ClientCardBegin("Profile trend card",new(0,0));ClientCardHeading("TREND");ImGui.Text("oldest to newest"u8);
        if(summary.Trend.Length<2)UiText.Wrapped("Play two games to see your trend."u8);else DrawPlacementTrend(summary,ImGui.GetContentRegionAvail().X);ImGui.EndChild();
        ClientCardBegin("Profile favorite traits card",new(0,0));ClientCardHeading("FAVORITE TRAITS");
        int traits=0;
        foreach(var favorite in summary.Traits.Where(f=>Rules.Traits.Any(t=>t.Id==f.Id))) {UiText.Wrapped($"{text.Get(Rules.Id,favorite.Id).Name} / {favorite.Games} game{(favorite.Games>1?"s":"")}");traits++;}
        if(traits==0)UiText.Wrapped("Play a game to see your favorite traits."u8);ImGui.EndChild();
        ClientCardBegin("Profile favorite units card",new(0,0));ClientCardHeading("FAVORITE DIGIMON");
        int units=0;
        foreach(var favorite in summary.Units)
        {
            var line=Rules.Lines.FirstOrDefault(l=>l.Id==favorite.Id);if(line is null)continue;
            var position=ImGui.GetCursorScreenPos();ImGui.Dummy(new(42,42));portraits.Draw(Rules.Id,line.Id,0,position,42);ProfilePortraits++;
            ImGui.SameLine();ImGui.BeginGroup();ImGui.Text(line.Forms[0]);ImGui.Text($"{favorite.Games} game{(favorite.Games>1?"s":"")}");ImGui.EndGroup();units++;
        }
        if(units==0)UiText.Wrapped("Play a game to see the Digimon you field most."u8);ImGui.EndChild();
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
        ClientCardHeading("MATCH HISTORY");ImGui.SameLine();ImGui.Text($"last {summary.Matches.Length}");
        if(summary.Matches.Length==0)UiText.Wrapped("No games yet. Your last 30 games show up here with placement, traits and Digimon."u8);
        foreach(var match in summary.Matches)
        {
            var color=match.Place==1?ClientGold:match.Place<=(match.Format=="duos"?2:4)?new ImColor8(88,217,204):ClientMuted;
            var start=ImGui.GetCursorScreenPos();float rowWidth=ImGui.GetContentRegionAvail().X,bodyWidth=Math.Max(80,rowWidth-188);
            string mode=catalog.Variants.FirstOrDefault(r=>r.Id.Split('/')[0]==match.Set)?.Name??match.Set??"";
            string heading=$"{PostgameSummary.PlaceLabel(match.Place,match.Format=="duos")}{(match.Format=="duos"?" / Duos":"")} / {mode} / round {match.Round} / level {match.Level}";
            string when=DateTimeOffset.FromUnixTimeMilliseconds(match.At).LocalDateTime.ToString("MMM d, h:mm tt");
            var traits=match.Traits.Take(6).Where(t=>Rules.Traits.Any(x=>x.Id==t.Id)).Select(t=>$"{text.Get(Rules.Id,t.Id).Name} {t.N}").ToArray();
            string traitText=traits.Length>0?string.Join(" / ",traits):"No active traits";
            var units=match.Units.Take(8).Where(u=>Rules.Lines.Any(l=>l.Id==u.Line&&u.Stage>=0&&u.Stage<l.Forms.Length)).ToArray();
            var spacing=ImGui.GetStyle().ItemSpacing;int columns=Math.Max(1,(int)((bodyWidth+spacing.X)/(36+spacing.X))),rows=(units.Length+columns-1)/columns;
            float portraitHeight=rows*36+Math.Max(0,rows-1)*spacing.Y;
            float rowHeight=Math.Max(116,20+ImGui.CalcTextSize(heading,false,bodyWidth).Y+ImGui.CalcTextSize(when,false,bodyWidth).Y+ImGui.CalcTextSize(traitText,false,bodyWidth).Y+spacing.Y*3+portraitHeight);
            ImGui.Dummy(new(rowWidth,rowHeight));var draw=ImGui.GetWindowDrawList();
            draw.AddRectFilled(start,start+new float2(rowWidth,rowHeight),match.Place==1?new ImColor8(48,39,22,190):new ImColor8(10,20,36,220));
            draw.AddRect(start,start+new float2(rowWidth,rowHeight),ClientBorder);draw.AddRectFilled(start,start+new float2(3,rowHeight),color);
            ImGui.SetCursorScreenPos(start+new float2(12,22));ImGui.BeginGroup();ClientPageTitle($"#{match.Place}",30,color);ClientPageTitle(match.Mode=="mp"?"MULTI":"SOLO",16,ClientMuted);ImGui.EndGroup();
            ImGui.SetCursorScreenPos(start+new float2(78,9));ImGui.BeginGroup();ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+bodyWidth);
            ImGui.Text(heading);
            ImGui.PushStyleColor(ImGuiCol.Text,ClientMuted);ImGui.Text(when);ImGui.PopStyleColor();
            ImGui.Text(traitText);ImGui.PopTextWrapPos();
            int count=0;foreach(var unit in units)
            {
                var line=Rules.Lines.FirstOrDefault(l=>l.Id==unit.Line);if(line is null||unit.Stage<0||unit.Stage>=line.Forms.Length)continue;
                if(count++%columns!=0)ImGui.SameLine();var position=ImGui.GetCursorScreenPos();ImGui.Dummy(new(36,36));portraits.Draw(Rules.Id,unit.Line,unit.Stage,position,36);ProfilePortraits++;
                draw.AddText(position+new float2(2,20),ClientGold,new string('*',unit.Stage+1));if(ImGui.IsItemHovered())ResultTooltip(line.Forms[unit.Stage]+" "+new string('*',unit.Stage+1));
            }
            ImGui.EndGroup();
            ProfileHistoryOverflow=Math.Max(ProfileHistoryOverflow,ImGui.GetItemRectMax().Y-start.Y-rowHeight);
            if(match.Bits is{} bits){ImGui.SetCursorScreenPos(start+new float2(rowWidth-96,rowHeight-29));ClientPageTitle($"+{bits} Bits",16,ClientGold);}
            ImGui.SetCursorScreenPos(start+new float2(0,rowHeight+8));ImGui.Dummy(new(0,1));ProfileHistoryRows++;
        }
    }
    private void ProfileBits(ProfileSummary summary)
    {
        ClientCardHeading($"BITS HISTORY / {profile!.Progress.Bits??0:N0} Bits");
        if(summary.BitsHistory.Length==0)UiText.Wrapped("No Bits earned yet. Finish a game to earn some."u8);
        foreach(var entry in summary.BitsHistory)
        {
            ImGui.Text(entry.Reason);ImGui.SameLine();ImGui.PushStyleColor(ImGuiCol.Text,ClientGold);ImGui.Text($"+{entry.N}");ImGui.PopStyleColor();
            ImGui.SameLine();ImGui.PushStyleColor(ImGuiCol.Text,ClientMuted);ImGui.Text(DateTimeOffset.FromUnixTimeMilliseconds(entry.At).LocalDateTime.ToString("d"));ImGui.PopStyleColor();ImGui.Separator();
        }
    }
}
