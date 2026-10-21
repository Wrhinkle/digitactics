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
        RememberClientRoute("profile");
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
        float width=size.X-48,height=size.Y-ClientPageTop-28;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
        ClientPageWindow("Tamer profile",24,ClientPageTop+12,width,height);ImGui.PopStyleVar();
        try
        {
            string? client=profile!.Storage.GetValueOrDefault("dt.client.v1"),wallet=profile.Storage.GetValueOrDefault("dt.cosmetics.v1");
            var summary=ProfileSummary.Read(client,wallet);DisplayedProfile=summary;
            var collection=new CosmeticCollection(wallet);var level=TamerProgress.Read(client);
            var bannerPosition=ImGui.GetCursorScreenPos();float bannerWidth=ImGui.GetContentRegionAvail().X,bannerHeight=Math.Clamp(size.Y*.24f,180,216);
            ImGui.Dummy(new(bannerWidth,bannerHeight));postgameArtwork.Draw(summary.Banner,bannerPosition,new(bannerWidth,bannerHeight),255,true,.4f);
            var bannerDraw=ImGui.GetWindowDrawList();
            for(int strip=0;strip<20;strip++)bannerDraw.AddRectFilled(bannerPosition+new float2(0,bannerHeight*strip/20),bannerPosition+new float2(bannerWidth,bannerHeight*(strip+1)/20),new ImColor8(1,10,19,(byte)(40+strip*10)));
            bannerDraw.AddRect(bannerPosition,bannerPosition+new float2(bannerWidth,bannerHeight),ClientGold);
            ImGui.SetCursorScreenPos(bannerPosition+new float2(10,8));
            if(HomeButton("closeProfile","Back",90,36)||!chooseProfileAvatar&&!KeyboardPopup&&ImGui.IsKeyPressed(ImGuiKey.Escape)) {BackClient();ImGui.End();return;}
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
            float identityWidth=Math.Clamp(bannerWidth-460,220,420);
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
                float leftWidth=Math.Clamp(bannerWidth*.30f,300,400);
                ImGui.BeginChild("Profile statistics"u8,new(leftWidth,bodyHeight));ProfileStatsCenter=ImGui.GetWindowPos()+new float2(leftWidth/2,bodyHeight/2);
                ProfileStats(summary,leftWidth-24);ProfileDailyMissions();ProfileStatsScroll=ImGui.GetScrollY();ImGui.EndChild();ImGui.SameLine();
                ImGui.BeginGroup();float rightWidth=ImGui.GetContentRegionAvail().X;
                float bitsHeight=Math.Min(148,bodyHeight*.4f),historyHeight=Math.Max(1,bodyHeight-bitsHeight-ImGui.GetStyle().ItemSpacing.Y);
                ClientCardBegin("Profile history",new(rightWidth,historyHeight));ProfileHistoryCenter=ImGui.GetWindowPos()+new float2(rightWidth/2,Math.Min(90,historyHeight/2));
                ProfileHistory(summary);ProfileHistoryScroll=ImGui.GetScrollY();ImGui.EndChild();
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
        string[] labels=["GAMES","WINS","TOP FINISHES","AVG PLACE"];
        float tile=(ImGui.GetContentRegionAvail().X-18)/4;
        var rowAt=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        for(int i=0;i<4;i++)
        {
            var at=rowAt+new float2(i*(tile+6),0);
            draw.AddRectFilled(at,at+new float2(tile,60),new ImColor8(1,10,19));draw.AddRect(at,at+new float2(tile,60),ClientBorder);
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,23);float valueWidth=ImGui.CalcTextSize(values[i]).X;
            draw.AddText(ImGui.GetFont(),23,at+new float2((tile-valueWidth)/2,9),ClientInk,values[i]);ImGui.PopFont();
            string label=i==2&&tile<92?"TOP 4":labels[i];
            ImGui.PushFont(NativeUiTheme.DisplayFont,14);float labelWidth=ImGui.CalcTextSize(label).X;
            draw.AddText(ImGui.GetFont(),ImGui.GetFontSize(),at+new float2((tile-labelWidth)/2,38),ClientMuted,label);ImGui.PopFont();
        }
        ImGui.Dummy(new(ImGui.GetContentRegionAvail().X,60));
        ImGui.Spacing();
        ProfileStatCard("Profile placements card","PLACEMENTS",$"last {summary.Matches.Length}");
        if(summary.Matches.Length==0)UiText.Wrapped("No games yet."u8);else DrawPlacementChart(summary,ImGui.GetContentRegionAvail().X);ImGui.EndChild();
        ProfileStatCard("Profile trend card","TREND","oldest to newest");
        if(summary.Trend.Length<2)UiText.Wrapped("Play two games to see your trend."u8);else DrawPlacementTrend(summary,ImGui.GetContentRegionAvail().X);ImGui.EndChild();
        ProfileStatCard("Profile favorite traits card","FAVORITE TRAITS");
        int traits=0;
        foreach(var favorite in summary.Traits.Where(f=>Rules.Traits.Any(t=>t.Id==f.Id))) {UiText.Wrapped($"{text.Get(Rules.Id,favorite.Id).Name} / {favorite.Games} game{(favorite.Games>1?"s":"")}");traits++;}
        if(traits==0)UiText.Wrapped("Play a game to see your favorite traits."u8);ImGui.EndChild();
        ProfileStatCard("Profile favorite units card","FAVORITE DIGIMON");
        int units=0;
        foreach(var favorite in summary.Units)
        {
            var line=Rules.Lines.FirstOrDefault(l=>l.Id==favorite.Id);if(line is null)continue;
            var position=ImGui.GetCursorScreenPos();ImGui.Dummy(new(42,42));portraits.Draw(Rules.Id,line.Id,0,position,42);ProfilePortraits++;
            ImGui.SameLine();ImGui.BeginGroup();ImGui.Text(line.Forms[0]);ImGui.Text($"{favorite.Games} game{(favorite.Games>1?"s":"")}");ImGui.EndGroup();units++;
        }
        if(units==0)UiText.Wrapped("Play a game to see the Digimon you field most."u8);ImGui.EndChild();
    }
    private static void ProfileStatCard(string id,string heading,string? subtitle=null)
    {
        ClientCardBegin(id,new(0,0));
        var at=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        draw.AddText(NativeUiTheme.DisplayBoldFont,18,at,ClientInk,heading);
        if(subtitle is not null)
        {
            ImGui.PushFont(default,14);float width=ImGui.CalcTextSize(subtitle).X;
            draw.AddText(ImGui.GetFont(),14,new(at.X+ImGui.GetContentRegionAvail().X-width,at.Y+3),ClientMuted,subtitle);ImGui.PopFont();
        }
        ImGui.Dummy(new(1,26));
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
    private static void ProfileSectionHeading(string heading,string? subtitle=null,string? wallet=null)
    {
        var at=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();float width=ImGui.GetContentRegionAvail().X,height=wallet is null?20:30;
        draw.AddText(NativeUiTheme.DisplayBoldFont,18,at+new float2(0,(height-18)/2),ClientInk,heading);
        if(subtitle is not null)
        {
            ImGui.PushFont(NativeUiTheme.BodyFont,14);float subtitleWidth=ImGui.CalcTextSize(subtitle).X;
            draw.AddText(ImGui.GetFont(),14,at+new float2(width-subtitleWidth,(height-14)/2),ClientMuted,subtitle);ImGui.PopFont();
        }
        if(wallet is not null)
        {
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,17);float badgeWidth=ImGui.CalcTextSize(wallet).X+40;var badge=at+new float2(width-badgeWidth,0);
            draw.AddRectFilled(badge,badge+new float2(badgeWidth,30),new ImColor8(1,10,19));draw.AddRect(badge,badge+new float2(badgeWidth,30),ClientBorder);
            ProfileBitsDiamond(badge+new float2(15,15));draw.AddText(ImGui.GetFont(),17,badge+new float2(28,6),ClientInk,wallet);ImGui.PopFont();
        }
        ImGui.Dummy(new(width,height+6));
    }
    private static void ProfileBitsDiamond(float2 center)
    {
        var draw=ImGui.GetWindowDrawList();
        draw.AddQuadFilled(center+new float2(0,-7),center+new float2(7,0),center+new float2(0,7),center+new float2(-7,0),new ImColor8(63,214,255));
        draw.AddQuadFilled(center+new float2(0,-5),center+new float2(5,0),center,center+new float2(-5,0),new ImColor8(176,239,255));
    }
    private void ProfileHistory(ProfileSummary summary)
    {
        ProfileSectionHeading("MATCH HISTORY",summary.Matches.Length>0?$"last {summary.Matches.Length}":null);
        if(summary.Matches.Length==0)UiText.Wrapped("No games yet. Your last 30 games show up here with placement, traits and Digimon."u8);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(ImGui.GetStyle().ItemSpacing.X,0));
        foreach(var match in summary.Matches)
        {
            var color=match.Place==1?ClientGold:match.Place<=(match.Format=="duos"?2:4)?new ImColor8(88,217,204):ClientMuted;
            var validRules=catalog.Variants.FirstOrDefault(r=>r.Id.Split('/')[0]==match.Set)??Rules;
            var start=ImGui.GetCursorScreenPos();float rowWidth=ImGui.GetContentRegionAvail().X;
            string mode=catalog.Variants.FirstOrDefault(r=>r.Id.Split('/')[0]==match.Set)?.Name??match.Set??"";
            string heading=$"{PostgameSummary.PlaceLabel(match.Place,match.Format=="duos")}{(match.Format=="duos"?" \u00b7 Duos":"")}{(mode.Length>0?" \u00b7 "+mode:"")} \u00b7 round {match.Round} \u00b7 level {match.Level}";
            string when=DateTimeOffset.FromUnixTimeMilliseconds(match.At).LocalDateTime.ToString("MMM d, h:mm tt");
            string reward=match.Bits is{} bits?$"+{bits}":"";
            ImGui.PushFont(NativeUiTheme.BodyFont,16);float rewardWidth=reward.Length>0?ImGui.CalcTextSize(reward).X+20:0,bodyWidth=Math.Max(80,rowWidth-108-rewardWidth);
            float headingWidth=ImGui.CalcTextSize(heading).X,headingHeight=ImGui.CalcTextSize(heading,false,bodyWidth).Y;ImGui.PopFont();
            ImGui.PushFont(NativeUiTheme.BodyFont,14);float dateWidth=ImGui.CalcTextSize(when).X;ImGui.PopFont();
            bool inlineDate=headingWidth+dateWidth+12<=bodyWidth;
            float topHeight=headingHeight+(inlineDate?0:19);
            var traits=match.Traits.Take(6).Where(t=>validRules.Traits.Any(x=>x.Id==t.Id)).ToArray();
            var chips=new List<(string Label,string Id,int Level,float X,float Y,float Width)>();float chipX=0,chipY=0;
            ImGui.PushFont(NativeUiTheme.BodyFont,15);
            foreach(var trait in traits)
            {
                string label=ResultTrim($"{text.Get(validRules.Id,trait.Id).Name} {trait.N}",Math.Max(1,bodyWidth-30));float chipWidth=Math.Min(bodyWidth,ImGui.CalcTextSize(label).X+30);
                if(chipX>0&&chipX+chipWidth>bodyWidth){chipX=0;chipY+=30;}
                chips.Add((label,trait.Id,trait.Lv,chipX,chipY,chipWidth));chipX+=chipWidth+5;
            }
            ImGui.PopFont();float traitHeight=chips.Count>0?chipY+25:15;
            var units=match.Units.Take(8).Where(u=>validRules.Lines.Concat(validRules.ExtraLines).Any(l=>l.Id==u.Line&&u.Stage>=0&&u.Stage<l.Forms.Length)).ToArray();
            int columns=Math.Max(1,(int)((bodyWidth+5)/35)),rows=(units.Length+columns-1)/columns;
            float portraitHeight=rows*30+Math.Max(0,rows-1)*5,rowHeight=Math.Max(92,16+topHeight+5+traitHeight+(rows>0?5+portraitHeight:0));
            ImGui.Dummy(new(rowWidth,rowHeight));var draw=ImGui.GetWindowDrawList();
            var left=match.Place==1?new ImColor8(48,39,22,220):new ImColor8(10,20,40,220);var right=new ImColor8(1,10,19,220);
            draw.AddRectFilledMultiColor(start,start+new float2(rowWidth,rowHeight),left,right,right,left);
            draw.AddRect(start,start+new float2(rowWidth,rowHeight),ClientBorder);draw.AddRectFilled(start,start+new float2(3,rowHeight),color);
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,30);string placement=$"#{match.Place}";
            draw.AddText(ImGui.GetFont(),30,start+new float2(12+(60-ImGui.CalcTextSize(placement).X)/2,(rowHeight-43)/2),color,placement);ImGui.PopFont();
            ImGui.PushFont(NativeUiTheme.BodyFont,12);string gameMode=match.Mode=="mp"?"MULTI":"SOLO";
            draw.AddText(ImGui.GetFont(),12,start+new float2(12+(60-ImGui.CalcTextSize(gameMode).X)/2,(rowHeight-43)/2+31),ClientMuted,gameMode);ImGui.PopFont();
            var body=start+new float2(84,8);ImGui.SetCursorScreenPos(body);ImGui.BeginGroup();
            ImGui.PushFont(NativeUiTheme.BodyFont,16);ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+bodyWidth);ImGui.Text(heading);ImGui.PopTextWrapPos();ImGui.PopFont();
            draw.AddText(NativeUiTheme.BodyFont,14,body+new float2(bodyWidth-dateWidth,inlineDate?1:headingHeight+5),ClientMuted,when);
            var composition=body+new float2(0,topHeight+5);
            ImGui.PushFont(NativeUiTheme.BodyFont,15);
            if(chips.Count==0)
            {
                ImGui.SetCursorScreenPos(composition);ImGui.PushStyleColor(ImGuiCol.Text,ClientMuted);ImGui.Text("No active traits"u8);ImGui.PopStyleColor();
            }
            foreach(var chip in chips)
            {
                var at=composition+new float2(chip.X,chip.Y);ImGui.SetCursorScreenPos(at);ImGui.Dummy(new(chip.Width,25));
                var tint=ResultTraitTint(resultTraitColors.GetValueOrDefault(validRules.Id)?.GetValueOrDefault(chip.Id,"#9aa4b1")??"#9aa4b1");
                draw.AddRectFilled(at,at+new float2(chip.Width,25),new ImColor8((byte)((tint.R*.14+.688)/.828),(byte)((tint.G*.14+6.88)/.828),(byte)((tint.B*.14+13.072)/.828),211));
                draw.AddRect(at,at+new float2(chip.Width,25),new ImColor8((byte)tint.R,(byte)tint.G,(byte)tint.B,115));
                var medal=chip.Level>=3?new ImColor8(255,207,74):chip.Level==2?new ImColor8(200,210,224):new ImColor8(201,138,82);
                draw.AddNgonFilled(at+new float2(10,12.5f),6.5f,medal,6);draw.AddText(ImGui.GetFont(),15,at+new float2(21,5),ClientInk,chip.Label);
                if(ImGui.IsItemHovered())ResultTooltip(text.Get(validRules.Id,chip.Id).Name);
            }
            ImGui.PopFont();
            for(int i=0;i<units.Length;i++)
            {
                var unit=units[i];var line=validRules.Lines.Concat(validRules.ExtraLines).First(l=>l.Id==unit.Line);
                var position=composition+new float2(i%columns*35,traitHeight+5+i/columns*35);ImGui.SetCursorScreenPos(position);ImGui.Dummy(new(30,30));
                draw.AddRectFilled(position,position+new float2(30,30),NativeUiTheme.Navy1,4);portraits.Draw(validRules.Id,unit.Line,unit.Stage,position,30);ProfilePortraits++;
                draw.AddRect(position,position+new float2(30,30),NativeUiTheme.GoldBorder,4);
                draw.AddText(NativeUiTheme.BodyFont,12,position+new float2(2,18),ClientGold,new string('*',unit.Stage+1));if(ImGui.IsItemHovered())ResultTooltip(line.Forms[unit.Stage]+" "+new string('*',unit.Stage+1));
            }
            ImGui.EndGroup();
            ProfileHistoryOverflow=Math.Max(ProfileHistoryOverflow,ImGui.GetItemRectMax().Y-start.Y-rowHeight);
            if(reward.Length>0)
            {
                var rewardAt=start+new float2(rowWidth-12-rewardWidth,(rowHeight-16)/2);ProfileBitsDiamond(rewardAt+new float2(7,8));
                draw.AddText(NativeUiTheme.BodyFont,16,rewardAt+new float2(20,0),ClientInk,reward);
            }
            ImGui.SetCursorScreenPos(start+new float2(0,rowHeight+6));ImGui.Dummy(new(0,0));ProfileHistoryRows++;
        }
        ImGui.PopStyleVar();
    }
    private void ProfileBits(ProfileSummary summary)
    {
        ProfileSectionHeading("BITS HISTORY",wallet:$"{profile!.Progress.Bits??0:N0}");
        if(summary.BitsHistory.Length==0)UiText.Wrapped("No Bits earned yet. Finish a game to earn some."u8);
        ImGui.PushFont(NativeUiTheme.BodyFont,16);float amountWidth=Math.Max(40,summary.BitsHistory.Select(entry=>ImGui.CalcTextSize($"+{entry.N}").X).DefaultIfEmpty(0).Max());ImGui.PopFont();
        ImGui.PushFont(NativeUiTheme.BodyFont,14);float dateWidth=Math.Max(86,summary.BitsHistory.Select(entry=>ImGui.CalcTextSize(DateTimeOffset.FromUnixTimeMilliseconds(entry.At).LocalDateTime.ToString("d")).X).DefaultIfEmpty(0).Max());ImGui.PopFont();
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(ImGui.GetStyle().ItemSpacing.X,0));
        foreach(var entry in summary.BitsHistory)
        {
            var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;var draw=ImGui.GetWindowDrawList();ImGui.Dummy(new(width,28));
            ImGui.PushFont(NativeUiTheme.BodyFont,16);string reason=ResultTrim(entry.Reason,Math.Max(20,width-dateWidth-amountWidth-24));
            draw.AddText(ImGui.GetFont(),16,at+new float2(0,4),ClientInk,reason);
            string amount=$"+{entry.N}";draw.AddText(ImGui.GetFont(),16,at+new float2(width-dateWidth-12-ImGui.CalcTextSize(amount).X,4),new ImColor8(63,214,255),amount);ImGui.PopFont();
            ImGui.PushFont(NativeUiTheme.BodyFont,14);string when=DateTimeOffset.FromUnixTimeMilliseconds(entry.At).LocalDateTime.ToString("d");
            draw.AddText(ImGui.GetFont(),14,at+new float2(width-ImGui.CalcTextSize(when).X,5),ClientMuted,when);ImGui.PopFont();
            draw.AddLine(at+new float2(0,27),at+new float2(width,27),new ImColor8(41,55,68));
            if(reason!=entry.Reason&&ImGui.IsItemHovered())ResultTooltip(entry.Reason);
        }
        ImGui.PopStyleVar();
    }
}
