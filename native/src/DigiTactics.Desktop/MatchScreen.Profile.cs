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
    internal bool ProfileStatsOverflow {get;private set;}
    // RENDER.profile: .prof (12px 24px 16px, 12px gaps) = .prof-banner over .prof-cols (clamp(300px, 27vw, 400px) | 1fr).
    private void ProfilePanel()
    {
        ProfileHistoryRows=ProfilePortraits=0;ProfileHistoryOverflow=0;DisplayedProfile=null;
        if(!chooseProfileAvatar&&!KeyboardPopup&&ImGui.IsKeyPressed(ImGuiKey.Escape)){BackClient();return;}
        bool narrow=socialWindowSize.X<=1500,shortWindow=socialWindowSize.Y<=820;
        float margin=narrow?16:24,top=ClientPageTop+(shortWindow?8:12),width=Math.Min(size.X-margin*2,1720),left=MathF.Round((size.X-width)/2);
        float height=size.Y-top-(shortWindow?10:16);
        ClientLayer("Tamer profile",new(left,top),new(width,height));
        var draw=ImGui.GetWindowDrawList();
        try
        {
            string? client=profile!.Storage.GetValueOrDefault("dt.client.v1"),wallet=profile.Storage.GetValueOrDefault("dt.cosmetics.v1");
            var summary=ProfileSummary.Read(client,wallet);DisplayedProfile=summary;
            var collection=new CosmeticCollection(wallet);var level=TamerProgress.Read(client);
            float bannerHeight=MathF.Round(shortWindow?140:Math.Clamp(socialWindowSize.Y*.24f,150,220));
            ProfileBanner(draw,summary,collection,level,new(left,top),new(width,bannerHeight),shortWindow);
            float colsTop=top+bannerHeight+12,colsHeight=top+height-colsTop;
            float leftWidth=socialWindowSize.X<=1380?300:Math.Clamp(socialWindowSize.X*.27f,300,400),rightLeft=left+leftWidth+12,rightWidth=width-leftWidth-12;
            // .prof-left scrolls on its own.
            ImGui.SetCursorScreenPos(new(left,colsTop));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
            ImGui.BeginChild("Profile statistics"u8,new(leftWidth,colsHeight),ImGuiChildFlags.None,ImGuiWindowFlags.NoBackground);
            ProfileStatsCenter=ImGui.GetWindowPos()+new float2(leftWidth/2,colsHeight/2);
            var statsAt=ImGui.GetCursorScreenPos();float statsHeight=ProfileStats(summary,statsAt,leftWidth-2-(ImGui.GetScrollMaxY()>0?ImGui.GetStyle().ScrollbarSize:0));
            ImGui.SetCursorScreenPos(statsAt);ImGui.Dummy(new(1,statsHeight));
            ProfileStatsScroll=ImGui.GetScrollY();ProfileStatsOverflow=ImGui.GetScrollMaxY()>0;
            ImGui.EndChild();ImGui.PopStyleVar();
            // .prof-right: match history (1fr) above Bits history (auto, at most 200px).
            float heading=NativeUiTheme.TextLine(14),chipHeight=12+NativeUiTheme.TextLine(14);
            float bitsRows=summary.BitsHistory.Length==0?ProfileEmptyHeight(true):Math.Min(14,summary.BitsHistory.Length)*ProfileBitsRowHeight;
            float bitsHeight=MathF.Round(Math.Min(200,2+16+Math.Max(heading,chipHeight)+12+bitsRows+16)),historyHeight=colsHeight-12-bitsHeight;
            var historyAt=new float2(rightLeft,colsTop);
            PaintTile(draw,historyAt,historyAt+new float2(rightWidth,historyHeight),box:true);
            TileHeading(draw,historyAt+new float2(17,17),rightWidth-34,"Match history",summary.Matches.Length>0?$"last {summary.Matches.Length}":null,14);
            float listTop=17+heading+12;
            ImGui.SetCursorScreenPos(historyAt+new float2(17,listTop));
            ImGui.BeginChild("Profile history"u8,new(rightWidth-34+4,Math.Max(1,historyHeight-listTop-17)),ImGuiChildFlags.None,ImGuiWindowFlags.NoBackground);
            ProfileHistoryCenter=ImGui.GetWindowPos()+new float2(rightWidth/2,Math.Min(90,historyHeight/2));
            if(summary.Matches.Length==0)ProfileEmpty(ImGui.GetWindowDrawList(),ImGui.GetCursorScreenPos(),rightWidth-34-4,"No games yet. Your last 30 games show up here with placement, traits and Digimon.",false);
            else ProfileHistory(summary);
            ProfileHistoryScroll=ImGui.GetScrollY();ImGui.EndChild();
            var bitsAt=new float2(rightLeft,colsTop+historyHeight+12);
            PaintTile(draw,bitsAt,bitsAt+new float2(rightWidth,bitsHeight),box:true);
            ProfileBits(summary,bitsAt,new(rightWidth,bitsHeight));
            ProfileContentBottom=bitsAt.Y+bitsHeight;
            if(profileEditMessage is not null)NativeUiPaint.Tracked(draw,NativeUiTheme.BodySemiBoldFont,12.5f,new(left+25,top+bannerHeight-17-18),NativeUiTheme.Text1,profileEditMessage);
            if(chooseProfileAvatar||ImGui.IsPopupOpen("Choose your icon"u8))ProfileAvatarChoices(collection,colsHeight);
        }
        catch(Exception error) when(error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or ArgumentOutOfRangeException)
        {Paragraph(draw,NativeUiTheme.BodyFont,14,new(left+24,top+24),width-48,NativeUiTheme.Muted,"Profile could not be read. Your saved data has been preserved.");}
        ImGui.End();
    }
    private void ProfileBanner(ImDrawListPtr draw,ProfileSummary summary,CosmeticCollection collection,TamerLevel level,float2 at,float2 extent,bool shortWindow)
    {
        var hi=at+extent;
        draw.PushClipRect(at,hi,true);
        postgameArtwork.Draw(summary.Banner,at,extent,255,true,.4f,draw);
        // ::before: linear-gradient(0deg, rgba(1,10,19,.92), rgba(1,10,19,.15) 70%), linear-gradient(90deg, rgba(1,10,19,.6), transparent 50%).
        float y70=hi.Y-extent.Y*.7f;
        draw.AddRectFilled(at,new(hi.X,y70),NativeUiPaint.Fade(new ImColor8(1,10,19,38)));
        NativeUiPaint.VerticalGradient(draw,new(at.X,y70),hi,new ImColor8(1,10,19,38),new ImColor8(1,10,19,235));
        NativeUiPaint.HorizontalGradient(draw,at,new(at.X+extent.X*.5f,hi.Y),new ImColor8(1,10,19,153),new ImColor8(1,10,19,0));
        draw.PopClipRect();
        draw.AddRect(at,hi,NativeUiPaint.Fade(NativeUiTheme.GoldBorder));
        // Content box: 16px 24px padding inside the border, items aligned to its bottom edge.
        float contentLeft=at.X+25,contentRight=hi.X-25,bottom=hi.Y-17,avatarSize=shortWindow?92:112;
        // .prof-rank on the right.
        float emblem=shortWindow?50:64;
        string rankLabel=summary.Rank.Label.ToUpperInvariant();
        float rankText=Math.Max(NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,16,rankLabel,.08f),NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,11,summary.Rank.Detail,0));
        float rankWidth=6+emblem+10+rankText+14+2,rankHeight=6+emblem+6+2;
        var rankLo=new float2(contentRight-rankWidth,bottom-rankHeight);
        draw.AddRectFilled(rankLo,rankLo+new float2(rankWidth,rankHeight),NativeUiPaint.Fade(new ImColor8(1,10,19,191)));
        draw.AddRect(rankLo,rankLo+new float2(rankWidth,rankHeight),NativeUiPaint.Fade(NativeUiTheme.GoldBorder));
        postgameArtwork.Draw("emblem-"+summary.Rank.Id,rankLo+new float2(7,7),new(emblem,emblem),255,false,.5f,draw,summary.Rank.Unranked,(byte)(summary.Rank.Unranked?153:255));
        float rankTextTop=rankLo.Y+(rankHeight-NativeUiTheme.TextLine(16)-NativeUiTheme.TextLine(11))/2;
        NativeUiPaint.TrackedIn(draw,NativeUiTheme.DisplayBoldFont,16,new(rankLo.X+7+emblem+10,rankTextTop),NativeUiTheme.TextLine(16),NativeUiTheme.GoldBright,rankLabel,.08f);
        NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,11,new(rankLo.X+7+emblem+10,rankTextTop+NativeUiTheme.TextLine(16)),NativeUiTheme.TextLine(11),NativeUiTheme.Muted,summary.Rank.Detail);
        // .prof-av: ring (r 58 of 128, 4px stroke), avatar inset 10px, level badge 6px below.
        var avatarAt=new float2(contentLeft,bottom-avatarSize);var center=avatarAt+new float2(avatarSize/2);float scale=avatarSize/128;
        ImGui.SetCursorScreenPos(avatarAt);
        if(ClientHitButton("profileAvatar",avatarSize,avatarSize)){chooseProfileAvatar=true;ImGui.OpenPopup("Choose your icon"u8);}
        bool avatarHot=ImGui.IsItemHovered();
        if(avatarHot)ResultTooltip("Change avatar");
        float inner=avatarSize/2-10;
        draw.AddCircleFilled(center,inner,NativeUiPaint.Fade(NativeUiTheme.Navy1),48);
        NativeUiPaint.Radial(draw,center-new float2(0,inner*.4f),new(inner*1.4f),new ImColor8(42,58,92),new ImColor8(10,20,40,0),48);
        string? avatar=summary.Avatar is { } owned && collection.Owns("legend",owned)?owned:collection.Equipped("legend")?.GetValue<string>();
        if(avatar is not null)DrawProfileAvatar(avatar,center-new float2(inner),inner*2);
        float ring=58*scale;draw.AddCircle(center,ring,NativeUiPaint.Fade(NativeUiTheme.GoldDim),64,4*scale);
        float progress=level.Need>0?Math.Clamp((float)level.Xp/level.Need,0,1):0;
        if(progress>0)
        {
            draw.PathArcTo(center,ring,-MathF.PI/2,-MathF.PI/2+progress*MathF.Tau,64);draw.PathStroke(NativeUiPaint.Fade(new ImColor8(240,210,140,70)),ImDrawFlags.None,4*scale+3);
            draw.PathArcTo(center,ring,-MathF.PI/2,-MathF.PI/2+progress*MathF.Tau,64);draw.PathStroke(NativeUiPaint.Fade(NativeUiTheme.Gold),ImDrawFlags.None,4*scale);
        }
        if(avatarHot)
        {
            // .pav-l: "Change" strip 30px above the bottom, shown on hover.
            float stripHeight=NativeUiTheme.TextLine(10)+6;var stripLo=new float2(avatarAt.X+10,avatarAt.Y+avatarSize-30-stripHeight);
            draw.AddRectFilled(stripLo,stripLo+new float2(avatarSize-20,stripHeight),NativeUiPaint.Fade(new ImColor8(1,10,19,217)));
            float changeWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyExtraBoldFont,10,"CHANGE",.08f);
            NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyExtraBoldFont,10,new(center.X-changeWidth/2,stripLo.Y+3),NativeUiTheme.TextLine(10),NativeUiTheme.Text1,"CHANGE",.08f);
        }
        string levelText=level.Level.ToString();float badgeLine=NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,12.5f);
        float badgeWidth=Math.Max(30,NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,12.5f,levelText,0)+16+2),badgeHeight=badgeLine+2+2;
        var badgeLo=new float2(MathF.Round(center.X-badgeWidth/2),avatarAt.Y+avatarSize+6-badgeHeight);
        draw.AddRectFilled(badgeLo,badgeLo+new float2(badgeWidth,badgeHeight),NativeUiPaint.Fade(NativeUiTheme.Navy0));
        draw.AddRect(badgeLo,badgeLo+new float2(badgeWidth,badgeHeight),NativeUiPaint.Fade(NativeUiTheme.Gold));
        float levelWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,12.5f,levelText,0);
        NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,12.5f,new(MathF.Round(center.X-levelWidth/2),badgeLo.Y+2),NativeUiTheme.GoldBright,levelText);
        // .prof-id: eyebrow, name input, XP note; centred on the avatar.
        float idLeft=avatarAt.X+avatarSize+16,idWidth=Math.Min(420,rankLo.X-16-idLeft);
        float inputHeight=NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,20)+10+2;
        float idHeight=NativeUiTheme.TextLine(11)+4+inputHeight+4+NativeUiTheme.TextLine(12.5f),y=MathF.Round(center.Y-idHeight/2);
        Eyebrow(draw,new(idLeft,y),$"Tamer · level {level.Level}");y+=NativeUiTheme.TextLine(11)+4;
        bool persona=steam is not null;
        var inputLo=new float2(idLeft,y);var inputHi=inputLo+new float2(idWidth,inputHeight);
        draw.AddRectFilled(inputLo,inputHi,NativeUiPaint.Fade(new ImColor8(1,10,19,153)));
        ImGui.SetCursorScreenPos(inputLo+new float2(1,1));ImGui.SetNextItemWidth(idWidth-2);
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,NativeUiTheme.Css(NativeUiTheme.DisplayBoldFont,20));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(9,(inputHeight-2-NativeUiTheme.Css(NativeUiTheme.DisplayBoldFont,20))/2));ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
        ImGui.PushStyleColor(ImGuiCol.FrameBg,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.FrameBgActive,new ImColor8(0,0,0,0));
        ImGui.PushStyleColor(ImGuiCol.Text,NativeUiTheme.GoldBright);ImGui.PushStyleColor(ImGuiCol.TextDisabled,new ImColor8(169,169,169));
        ImGui.InputText("##profileName"u8,profileName,persona?ImGuiInputTextFlags.ReadOnly:ImGuiInputTextFlags.None);
        ImGui.PopStyleColor(5);ImGui.PopStyleVar(2);ImGui.PopFont();
        bool focused=ImGui.IsItemActive();
        // ::placeholder: Chromium's darkGray, not bold.
        if(profileName.ToString().Length==0)NativeUiPaint.TrackedIn(draw,NativeUiTheme.DisplayFont,20,inputLo+new float2(11,6),inputHeight-12,new ImColor8(169,169,169),"Your Tamer name");
        Buttons["profileName"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
        if(focused)NativeUiPaint.BoxGlow(draw,inputLo,inputHi,new ImColor8(200,170,110,46),3);
        draw.AddRect(inputLo,inputHi,NativeUiPaint.Fade(focused?NativeUiTheme.Gold:NativeUiTheme.GoldDim));
        if(!persona && ImGui.IsItemDeactivatedAfterEdit())
        {
            string cleaned=ProfileSummary.CleanName(profileName.ToString());
            if(profile!.UpdateStorage(new Dictionary<string,string>{{"dt.name",cleaned}})) {profileName.SetValue(cleaned.AsSpan());profileEditMessage=cleaned.Length>0?"Name saved: "+cleaned:"Name cleared";}
            else profileEditMessage=profile.Error;
        }
        if(persona && ImGui.IsItemHovered())ResultTooltip("Your Steam name");
        y+=inputHeight+4;
        NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,12.5f,new(idLeft,y),NativeUiTheme.TextLine(12.5f),NativeUiTheme.MutedNote,
            SocialEllipsis($"{level.Xp} / {level.Need} XP to level {level.Level+1}. Other players see your name in lobbies.",NativeUiTheme.BodyFont,12.5f,Math.Max(idWidth,rankLo.X-16-idLeft)));
        // .prof-banners: top 10px, right 10px, 46x26 thumbnails 4px apart.
        float x=hi.X-1-10-(ProfileSummary.Banners.Length*46+(ProfileSummary.Banners.Length-1)*4);
        foreach(string banner in ProfileSummary.Banners)
        {
            var lo=new float2(x,at.Y+1+10);var bhi=lo+new float2(46,26);
            ImGui.SetCursorScreenPos(lo);
            if(ClientHitButton("profileBanner/"+banner,46,26))ChangeProfile("banner",banner);
            bool on=summary.Banner==banner,hot=ImGui.IsItemHovered();
            if(on)NativeUiPaint.BoxGlow(draw,lo,bhi,new ImColor8(240,210,140,102),8);
            postgameArtwork.Draw(banner,lo,new(46,26),(byte)(on||hot?255:179),true,.5f,draw);
            draw.AddRect(lo,bhi,NativeUiPaint.Fade(on?NativeUiTheme.GoldBright:hot?NativeUiTheme.GoldDeep:NativeUiTheme.GoldDim,on||hot?1:.7f));
            if(hot)ResultTooltip("Use this banner");
            x+=46+4;
        }
    }
    private void DrawProfileAvatar(string id,float2 position,float pixels)
    {
        var discovered=LegendEvolution.Discovery(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),id);
        portraits.DrawEvolution(discovered.Current,position,pixels,true);ProfilePortraits++;
    }
    /// <summary>.empty (24px padding, 14px) or .empty.small (12px padding, 12.5px): centred c-text-3 text in a dashed gold-5 frame.</summary>
    private static float ProfileEmptyHeight(bool small)=>small?12*2+NativeUiTheme.TextLine(12.5f)+2:24*2+NativeUiTheme.TextLine(14)+2;
    private static float ProfileEmpty(ImDrawListPtr draw,float2 at,float width,string text,bool small)
    {
        float pad=small?12:24,px=small?12.5f:14,line=NativeUiTheme.TextLine(px);
        var lines=WrapText(NativeUiTheme.BodyFont,px,text,width-pad*2-2);
        float height=pad*2+lines.Count*line+2;var hi=at+new float2(width,height);
        DashedRect(draw,at,hi,NativeUiTheme.GoldDim);
        for(int i=0;i<lines.Count;i++)
        {
            float lineWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,px,lines[i],0);
            NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,px,new(MathF.Round(at.X+(width-lineWidth)/2),at.Y+1+pad+i*line),line,NativeUiTheme.Text3,lines[i]);
        }
        ImGui.SetCursorScreenPos(at);ImGui.Dummy(new(width,height));
        return height;
    }
    /// <summary>A 1px dashed border (Chromium draws 3px dashes with 3px gaps at this width).</summary>
    private static void DashedRect(ImDrawListPtr draw,float2 lo,float2 hi,ImColor8 color)
    {
        var c=NativeUiPaint.Fade(color);
        for(float x=lo.X;x<hi.X;x+=6){float e=Math.Min(x+3,hi.X);draw.AddRectFilled(new(x,lo.Y),new(e,lo.Y+1),c);draw.AddRectFilled(new(x,hi.Y-1),new(e,hi.Y),c);}
        for(float y=lo.Y;y<hi.Y;y+=6){float e=Math.Min(y+3,hi.Y);draw.AddRectFilled(new(lo.X,y),new(lo.X+1,e),c);draw.AddRectFilled(new(hi.X-1,y),new(hi.X,e),c);}
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
    /// <summary>.prof-left: four stat tiles then the Placements, Trend, Favorite traits and Favorite Digimon boxes (10px 12px padding).</summary>
    private float ProfileStats(ProfileSummary summary,float2 at,float width)
    {
        var draw=ImGui.GetWindowDrawList();float y=at.Y;
        string[] values=[summary.Games.ToString(),summary.Wins.ToString(),summary.TopFour.ToString(),summary.Games>0?summary.Average.ToString("F2"):"-"];
        string[] labels=["GAMES","WINS","TOP FINISHES","AVG PLACE"];
        float tile=(width-18)/4,tileHeight=2+8+20*1.1f+2+NativeUiTheme.TextLine(10)+8;
        for(int i=0;i<4;i++)
        {
            var lo=new float2(MathF.Round(at.X+i*(tile+6)),y);var hi=new float2(MathF.Round(at.X+i*(tile+6)+tile),y+tileHeight);
            draw.AddRectFilled(lo,hi,NativeUiPaint.Fade(new ImColor8(1,10,19,204)));draw.AddRect(lo,hi,NativeUiPaint.Fade(NativeUiTheme.GoldDim));
            float valueWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,20,values[i],0);
            NativeUiPaint.TrackedIn(draw,NativeUiTheme.DisplayBoldFont,20,new(MathF.Round((lo.X+hi.X-valueWidth)/2),y+1+8),20*1.1f,NativeUiTheme.GoldBright,values[i]);
            float labelWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyBoldFont,10,labels[i],.06f);
            NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyBoldFont,10,new(MathF.Round((lo.X+hi.X-labelWidth)/2),y+1+8+20*1.1f+2),NativeUiTheme.TextLine(10),NativeUiTheme.Text3,labels[i],.06f);
        }
        y+=tileHeight+10;
        float heading=NativeUiTheme.TextLine(14);
        float Box(string title,string? note,Func<float2,float,float> content)
        {
            float top=y,inner=width-26;
            var contentAt=new float2(at.X+13,top+11+heading+6);
            // Paint the frame behind the content once the content height is known.
            draw.ChannelsSplit(2);draw.ChannelsSetCurrent(1);
            TileHeading(draw,new(at.X+13,top+11),inner,title,note,14);
            float contentHeight=content(contentAt,inner);
            draw.ChannelsSetCurrent(0);
            float boxHeight=11+heading+6+contentHeight+11;
            PaintTile(draw,new(at.X,top),new(at.X+width,top+boxHeight),box:true);
            draw.ChannelsMerge();
            y+=boxHeight+10;return boxHeight;
        }
        Box("Placements",$"last {summary.Matches.Length}",(p,w)=>
        {
            if(summary.Matches.Length==0)return ProfileEmpty(draw,p,w,"No games yet.",true);
            ImGui.SetCursorScreenPos(p);DrawPlacementChart(summary,w);return 128;
        });
        Box("Trend","oldest to newest",(p,w)=>
        {
            if(summary.Trend.Length<2)return ProfileEmpty(draw,p,w,"Play two games to see your trend.",true);
            ImGui.SetCursorScreenPos(p);DrawPlacementTrend(summary,w);return 120;
        });
        Box("Favorite traits",null,(p,w)=>
        {
            var favorites=summary.Traits.Where(f=>Rules.Traits.Any(t=>t.Id==f.Id)).ToArray();
            if(favorites.Length==0)return ProfileEmpty(draw,p,w,"Play a game to see your favorite traits.",true);
            float rowY=p.Y;
            foreach(var favorite in favorites)
            {
                // .fav: a trait chip and the muted game count, 4px vertical padding.
                string name=text.Get(Rules.Id,favorite.Id).Name;float chipHeight=NativeUiTheme.TextLine(11)+4+2,rowHeight=8+chipHeight;
                var tint=ResultTraitTint(resultTraitColors.GetValueOrDefault(Rules.Id)?.GetValueOrDefault(favorite.Id,"#9aa4b1")??"#9aa4b1");
                var traitColor=new ImColor8((byte)tint.R,(byte)tint.G,(byte)tint.B);
                float chipWidth=4+12+5+NativeUiPaint.TrackedWidth(NativeUiTheme.BodyBoldFont,11,name,0)+8+2;var chipLo=new float2(p.X,rowY+4);var chipHi=chipLo+new float2(chipWidth,chipHeight);
                draw.AddRectFilled(chipLo,chipHi,NativeUiPaint.Fade(Mix(new ImColor8(1,10,19,204),traitColor,.14f)));draw.AddRect(chipLo,chipHi,NativeUiPaint.Fade(NativeUiPaint.Alpha(traitColor,115)));
                draw.AddNgonFilled(chipLo+new float2(1+4+6,chipHeight/2),6.5f,NativeUiPaint.Fade(new ImColor8(200,210,224)),6);
                NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyBoldFont,11,chipLo+new float2(1+4+12+5,2),NativeUiTheme.TextLine(11),NativeUiTheme.Text1,name);
                string games=$"{favorite.Games} game{(favorite.Games>1?"s":"")}";float gamesWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,13,games,0);
                NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,13,new(p.X+w-gamesWidth,rowY+(rowHeight-NativeUiTheme.TextLine(13))/2),NativeUiTheme.TextLine(13),NativeUiTheme.MutedNote,games);
                rowY+=rowHeight;
            }
            return rowY-p.Y;
        });
        Box("Favorite Digimon",null,(p,w)=>
        {
            var favorites=summary.Units.Select(f=>(Favorite:f,Line:Rules.Lines.FirstOrDefault(l=>l.Id==f.Id))).Where(f=>f.Line is not null).ToArray();
            if(favorites.Length==0)return ProfileEmpty(draw,p,w,"Play a game to see the Digimon you field most.",true);
            // .favs-u: auto-fill columns of at least 140px, 8px gaps; 38px portrait, name and game count.
            int columns=Math.Max(1,(int)((w+8)/(140+8)));float cell=(w-(columns-1)*8)/columns,rowHeight=38;
            for(int i=0;i<favorites.Length;i++)
            {
                var (favorite,line)=favorites[i];var cellAt=p+new float2(i%columns*(cell+8),i/columns*(rowHeight+8));
                draw.AddRectFilled(cellAt,cellAt+new float2(38,38),NativeUiPaint.Fade(NativeUiTheme.Navy1));portraits.Draw(Rules.Id,line!.Id,0,cellAt,38);ProfilePortraits++;
                draw.AddRect(cellAt,cellAt+new float2(38,38),NativeUiPaint.Fade(NativeUiTheme.GoldBorder));
                float textTop=cellAt.Y+(38-NativeUiTheme.TextLine(12.5f)-NativeUiTheme.TextLine(11))/2;
                NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,12.5f,new(cellAt.X+38+8,textTop),NativeUiTheme.TextLine(12.5f),NativeUiTheme.Text1,SocialEllipsis(line.Forms[0],NativeUiTheme.BodyFont,12.5f,cell-46));
                NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,11,new(cellAt.X+38+8,textTop+NativeUiTheme.TextLine(12.5f)),NativeUiTheme.TextLine(11),NativeUiTheme.MutedNote,$"{favorite.Games} game{(favorite.Games>1?"s":"")}");
            }
            return (favorites.Length+columns-1)/columns*(rowHeight+8)-8;
        });
        return y-10-at.Y;
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
    private static void ProfileBitsDiamond(float2 center)
    {
        var draw=ImGui.GetWindowDrawList();
        draw.AddQuadFilled(center+new float2(0,-7),center+new float2(7,0),center+new float2(0,7),center+new float2(-7,0),new ImColor8(63,214,255));
        draw.AddQuadFilled(center+new float2(0,-5),center+new float2(5,0),center,center+new float2(-5,0),new ImColor8(176,239,255));
    }
    private void ProfileHistory(ProfileSummary summary)
    {
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
    private static float ProfileBitsRowHeight=>8+NativeUiTheme.TextLine(12.5f)+1;
    /// <summary>.bits-card: heading with the static Bits chip, then .bh-row lines (reason | +n | date) or the empty note.</summary>
    private void ProfileBits(ProfileSummary summary,float2 at,float2 extent)
    {
        var draw=ImGui.GetWindowDrawList();float heading=NativeUiTheme.TextLine(14);
        // The static .chip-bits sits in .card-h, so it inherits Chakra Petch 14px, .1em tracking and line-height 1.35.
        string balance=$"{profile!.Progress.Bits??0:N0}";
        float balanceWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,14,balance,.1f);
        float chipWidth=1+10+15+6+balanceWidth+12+1,chipHeight=12+NativeUiTheme.TextLine(14),rowHeight=Math.Max(heading,chipHeight);
        TileHeading(draw,at+new float2(17,17+(rowHeight-heading)/2),extent.X-34-chipWidth-8,"Bits history",null,14);
        var chipLo=new float2(at.X+extent.X-17-chipWidth,at.Y+17+(rowHeight-chipHeight)/2);var chipHi=chipLo+new float2(chipWidth,chipHeight);
        draw.AddRectFilled(chipLo,chipHi,NativeUiPaint.Fade(new ImColor8(1,10,19,179)));draw.AddRect(chipLo,chipHi,NativeUiPaint.Fade(NativeUiTheme.GoldDim));
        NativeUiPaint.Bit(draw,new(chipLo.X+1+10+1+5.5f,(chipLo.Y+chipHi.Y)/2),11);
        NativeUiPaint.TrackedIn(draw,NativeUiTheme.DisplayBoldFont,14,new(chipLo.X+1+10+15+6,chipLo.Y+6),NativeUiTheme.TextLine(14),NativeUiTheme.GoldBright,balance,.1f);
        var listAt=at+new float2(17,17+rowHeight+12);float listWidth=extent.X-34,listHeight=extent.Y-(17+rowHeight+12)-17;
        if(summary.BitsHistory.Length==0){ImGui.SetCursorScreenPos(listAt);ProfileEmpty(draw,listAt,listWidth,"No Bits earned yet. Finish a game to earn some.",true);return;}
        ImGui.SetCursorScreenPos(listAt);ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
        ImGui.BeginChild("Profile Bits"u8,new(listWidth,Math.Max(1,listHeight)),ImGuiChildFlags.None,ImGuiWindowFlags.NoBackground);ImGui.PopStyleVar();
        var list=ImGui.GetWindowDrawList();var origin=ImGui.GetCursorScreenPos();float width=listWidth-(ImGui.GetScrollMaxY()>0?ImGui.GetStyle().ScrollbarSize:0);
        float line=NativeUiTheme.TextLine(12.5f);
        for(int i=0;i<Math.Min(14,summary.BitsHistory.Length);i++)
        {
            var entry=summary.BitsHistory[i];var rowAt=origin+new float2(0,i*ProfileBitsRowHeight);
            string amount=$"+{entry.N}",when=DateTimeOffset.FromUnixTimeMilliseconds(entry.At).LocalDateTime.ToString("d");
            float amountWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyBoldFont,12.5f,amount,0),whenWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,11,when,0);
            float reasonWidth=width-86-12-amountWidth-12;string reason=SocialEllipsis(entry.Reason,NativeUiTheme.BodyFont,12.5f,reasonWidth);
            NativeUiPaint.TrackedIn(list,NativeUiTheme.BodyFont,12.5f,rowAt+new float2(0,4),line,NativeUiTheme.Muted,reason);
            NativeUiPaint.TrackedIn(list,NativeUiTheme.BodyBoldFont,12.5f,rowAt+new float2(width-86-12-amountWidth,4),line,NativeUiTheme.Bits,amount);
            NativeUiPaint.TrackedIn(list,NativeUiTheme.BodyFont,11,rowAt+new float2(width-whenWidth,4+(line-NativeUiTheme.TextLine(11))/2),NativeUiTheme.TextLine(11),NativeUiTheme.Text3,when);
            list.AddLine(rowAt+new float2(0,ProfileBitsRowHeight-.5f),rowAt+new float2(width,ProfileBitsRowHeight-.5f),NativeUiPaint.Fade(new ImColor8(34,48,73)));
            if(reason!=entry.Reason&&ImGui.IsMouseHoveringRect(rowAt,rowAt+new float2(reasonWidth,ProfileBitsRowHeight)))ResultTooltip(entry.Reason);
        }
        ImGui.SetCursorScreenPos(origin);ImGui.Dummy(new(1,Math.Min(14,summary.BitsHistory.Length)*ProfileBitsRowHeight));
        ImGui.EndChild();
    }
}
