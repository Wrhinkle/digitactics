using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;
using static DigiTactics.Desktop.ClientCss;

namespace DigiTactics.Desktop;

// Play page: mode select (js/setpicker.js, theme.js "Play: mode select") laid out in the
// original CSS pixels. Like the original .cpage it scrolls vertically when the cards, options
// and queue footer are taller than the window.
internal sealed partial class MatchScreen
{
    // Set accents are each set's colors.main in js/sets/<id>.js; Chaos uses --prism.
    private static readonly Dictionary<string,ImColor8> SetAccent=new()
    {
        ["faction-wars"]=new(255,176,46),["fates"]=new(255,93,122),["galaxies"]=new(122,140,255),["gizmos"]=new(79,208,255),
        ["reckoning"]=new(138,60,255),["dragonlands"]=new(255,106,61),["arcane"]=new(224,90,216),["coliseum"]=new(255,90,61),
        ["underlords"]=new(192,57,43),["underlords_ko"]=new(192,57,43),["autochess"]=new(224,160,64),["chaos"]=new(229,139,255),
    };
    private const string ChaosDescription="A random set every game, picked from the game seed.";
    private int homeModeScrollRequest=-1;
    private string? homeModeShownSet;
    private (float X,float Scroll)? homeModeDrag;
    private bool homeModeDragged,homePlayWasDrawn,homePlayDrawn;
    private float homePlayScroll,homeModeScroll,homeModeScrollFrom,homeModeScrollGoal;
    private double homeModeScrollAt=double.NaN;
    private readonly Dictionary<string,(float2 Min,float2 Max)> homePlayControls=[];
    internal int HomeModeCardCount {get;private set;}
    internal int HomePartySeatCount {get;private set;}
    internal bool HomePlayLayoutFits {get;private set;}
    internal (float2 Min,float2 Max) HomeModeCarouselBounds {get;private set;}
    internal (float2 Min,float2 Max) HomePlayPageBounds {get;private set;}
    internal float HomePlayScroll=>homePlayScroll;

    /// <summary>For drivers: scrolls the Play page so a control is fully visible; true once it is.</summary>
    internal bool RevealHomePlayControl(string key)
    {
        if(!showHomePlay||homePlayParty||!homePlayControls.TryGetValue(key,out var box))return true;
        var row=HomeModeCarouselBounds;
        if(key.StartsWith("homeMode/",StringComparison.Ordinal)&&(box.Min.X<row.Min.X||box.Max.X>row.Max.X))
        {
            // Scroll the card row like a swipe: the card comes into view without being selected.
            homeModeScroll+=box.Min.X<row.Min.X?box.Min.X-row.Min.X-4:box.Max.X-row.Max.X+4;homeModeScrollAt=double.NaN;
            return false;
        }
        var page=HomePlayPageBounds;
        if(box.Min.Y>=page.Min.Y&&box.Max.Y<=page.Max.Y)return true;
        homePlayScroll+=box.Min.Y<page.Min.Y?box.Min.Y-page.Min.Y-8:box.Max.Y-page.Max.Y+8;
        return false;
    }

    private static string SetModeName(string? mode)=>mode switch{"tft"=>"Classic","underlords"=>"Undermons","autochess"=>"Auto Chess",_=>"Any set"};

    // A transparent hit target with the shared button bookkeeping (Buttons, focus, click sound).
    private bool CssHit(string key,float2 at,float2 extent,out bool hovered)
    {
        ImGui.SetCursorScreenPos(at);
        ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(0,0,0,0));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
        bool pressed=Button(key,"",Math.Max(1,extent.X),Math.Max(1,extent.Y));hovered=ImGui.IsItemHovered();
        ImGui.PopStyleVar();ImGui.PopStyleColor(4);
        return pressed;
    }

    // .pace-pick .pp-btn
    private bool PaceToggle(string key,string label,float2 at,bool on)
    {
        string text=label.ToUpperInvariant();float width=Width(Face.DisplayBold,12.5f,text,.75f)+30;
        bool pressed=CssHit(key,at,new(width,30),out bool hover);var draw=ImGui.GetWindowDrawList();var max=at+new float2(width,30);
        if(on)Glow(draw,at,max,new ImColor8(240,210,140,64),14,2);
        draw.AddRectFilled(at,max,new ImColor8(1,10,19,179),2);
        Border(draw,at,max,on?Gold1:hover?Gold2:Gold4,1,2);if(on)draw.AddRect(at-new float2(.5f),max+new float2(.5f),Gold1,2);
        Text(draw,Face.DisplayBold,12.5f,at+new float2(15,0),on||hover?Gold1:Text2,text,.75f,30);
        return pressed;
    }
    private float PacePickWidth(string[] labels)=>labels.Sum(l=>Width(Face.DisplayBold,12.5f,l.ToUpperInvariant(),.75f)+30)+6*(labels.Length-1);

    /// <summary>Pace and Format rows of .setpick-options / the party header; returns the block height.</summary>
    private float DrawPaceFormat(string keyPrefix,float2 at,float columnWidth,float columnGap,bool stacked)
    {
        var draw=ImGui.GetWindowDrawList();
        void Row(string label,string group,(string Id,string Name)[] options,string current,float2 rowAt,string description,Action<string> choose)
        {
            float x=Text(draw,Face.DisplayBold,11,rowAt+new float2(0,8),Text2,label.ToUpperInvariant(),1.54f)+4+6;
            foreach(var option in options)
            {
                float width=Width(Face.DisplayBold,12.5f,option.Name.ToUpperInvariant(),.75f)+30;
                string key=keyPrefix+group+"/"+option.Id;
                if(PaceToggle(key,option.Name,rowAt+new float2(x,0),current==option.Id))choose(option.Id);
                homePlayControls[key]=(rowAt+new float2(x,0),rowAt+new float2(x+width,30));
                x+=width+6;
            }
            Paragraph(draw,Face.Body,12.5f,rowAt+new float2(0,36),columnWidth,16.875f,Text2,description);
        }
        string paceDescription=selectedPace=="normal"?"100 HP, buy XP, interest and streaks, carousels each stage. About 35-40 rounds.":"20 HP, levels by stage, fixed income, 1-gold rerolls. About 25 rounds.";
        Row("Pace","Pace",[("hyper","Hyper Roll"),("normal","Normal")],selectedPace,at,paceDescription,pace=>SelectPace(pace));
        float paceHeight=36+ParagraphHeight(Face.Body,12.5f,columnWidth,16.875f,paceDescription);
        string formatDescription=duos?"Four teams of two. Share health, send Digimon through the portal, combine your traits in one team battle.":"Eight Tamers. Every board for itself.";
        var formatAt=stacked?at+new float2(0,paceHeight+6):at+new float2(columnWidth+columnGap,0);
        Row("Format","Format",[("solo","Free for all"),("duos","Duos · 2v2")],duos?"duos":"solo",formatAt,formatDescription,SelectFormat);
        float formatHeight=36+ParagraphHeight(Face.Body,12.5f,columnWidth,16.875f,formatDescription);
        return stacked?paceHeight+6+formatHeight:Math.Max(paceHeight,formatHeight);
    }

    private void HomePlayPanel()
    {
        RefreshSetSelection();HomeModeCardCount=HomePartySeatCount=0;HomePlayRankLayoutFits=false;
        homePlayWasDrawn=homePlayDrawn;homePlayDrawn=true;
        float viewWidth=socialWindowSize.X,viewHeight=socialWindowSize.Y;
        float pageWidth=Math.Min(size.X,1720),left=(size.X-pageWidth)/2,pad=viewWidth<=1500?16:24,padTop=viewHeight<=820?8:12,padBottom=viewHeight<=820?10:16;
        float pageTop=ClientPageTop,pageHeight=size.Y-ClientPageTop;
        if(homePlayParty)
        {
            var partyAt=new float2(left+pad,pageTop+padTop);var partyExtent=new float2(pageWidth-2*pad,pageHeight-padTop-padBottom);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
            Window("Choose your match",partyAt.X,partyAt.Y,partyExtent.X,partyExtent.Y,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
            ImGui.PopStyleVar(2);HomePlayLayoutFits=partyExtent.X>=700&&partyExtent.Y>=440;
            HomeSoloParty(partyAt,partyExtent);ImGui.End();return;
        }
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
        Window("Choose your match",0,pageTop,size.X,pageHeight,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleVar(2);
        HomePlayPageBounds=(new(0,pageTop),new(size.X,pageTop+pageHeight));
        var draw=ImGui.GetWindowDrawList();homePlayControls.Clear();
        float inner=pageWidth-2*pad;

        string[] ids=SetChoices.Options.Select(s=>s.Id).Concat(SetChoices.HasChaos?["chaos"]:Array.Empty<string>()).ToArray();
        int selected=Math.Max(0,Array.IndexOf(ids,selectedSet));
        // .set-card sizes: flex-basis and art height follow the viewport (setpicker.js).
        float cardWidth=viewHeight<=850&&viewWidth>700?Math.Clamp(viewWidth*.23f,240,300):Math.Clamp(viewWidth*.27f,250,370);
        float artHeight=Math.Clamp(viewHeight-410,250,555),titlePx=Math.Clamp(viewWidth*.02f,22,30),textWidth=cardWidth-2-36;
        string Description(string id)=>id=="chaos"?ChaosDescription:HomeSets.Value[id].Description;
        string Title(string id)=>id=="chaos"?"Chaos":HomeSets.Value[id].Name;
        float textHeight=ids.Max(id=>18+14+8+ParagraphHeight(Face.DisplayBold,titlePx,textWidth,titlePx*1.12f,Title(id),titlePx*.02f)+8+ParagraphHeight(Face.BodyBold,12.5f,textWidth,18.75f,Description(id))+18);
        float cardHeight=2+artHeight+textHeight;
        float headTop=padTop,navTop=headTop+60+12,rowTop=navTop+42.4f+10,rowHeight=4+cardHeight+16;
        bool stacked=viewWidth<=700;
        float optionsTop=rowTop+rowHeight;
        // Measure the option block once at its final position (the text wraps by column width).
        float columnWidth=stacked?inner:(inner-24)/2;
        bool compactModes=viewWidth<=1380;
        float modeHeight=compactModes?64:68.6f;
        string[] modeText=["You and 7 AI Tamers. No account needed.","Lobbies of 8. Friends take seats, the AI plays the rest."];

        // Wheel scrolls the page; a horizontal wheel or a drag scrolls the cards.
        var io=ImGui.GetIO();bool pageHovered=ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows|ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        if(pageHovered&&io.MouseWheel!=0)homePlayScroll-=io.MouseWheel*100;
        float optionsHeightEstimate=4+12+Math.Max(52.9f,36+ParagraphHeight(Face.Body,12.5f,columnWidth,16.875f,duos?"Four teams of two. Share health, send Digimon through the portal, combine your traits in one team battle.":"Eight Tamers. Every board for itself."))+12;
        if(stacked)optionsHeightEstimate+=58.9f;
        float footTop=optionsTop+optionsHeightEstimate+12,contentHeight=footTop+1+12+modeHeight+padBottom;
        homePlayScroll=Math.Clamp(homePlayScroll,0,Math.Max(0,contentHeight-pageHeight));
        var origin=new float2(left,pageTop-homePlayScroll);

        // Header: eyebrow/title on the left, rank status on the right (.ps-head, flex-end aligned).
        var headAt=origin+new float2(pad,headTop);
        float rankHeight=DrawHomePlayRank(headAt,inner);
        float titleTop=headTop+Math.Max(0,rankHeight-45.4f);
        Text(draw,Face.DisplayBold,11,origin+new float2(pad,titleTop),Gold2,"CHOOSE A MODE",1.76f,14.85f);
        Text(draw,Face.DisplayBold,26,origin+new float2(pad,titleTop+14.8f+2),Gold1,"PLAY",1.04f,28.6f);
        HomePlayLayoutFits=HomePlayRankLayoutFits&&pageWidth>=700;

        // .setpick-nav
        var navAt=origin+new float2(pad,navTop);
        Text(draw,Face.Body,12.5f,navAt+new float2(0,12.75f),Text2,"Choose a set. Swipe or scroll to explore.",0,16.875f);
        float arrowWidth=46.7f,arrowHeight=42.4f,controlsLeft=inner-(arrowWidth*2+54+24);
        bool Arrow(string key,string glyph,float x)
        {
            var at=navAt+new float2(x,0);bool pressed=CssHit(key,at,new(arrowWidth,arrowHeight),out bool hover);var max=at+new float2(arrowWidth,arrowHeight);
            if(hover)Glow(draw,at,max,new ImColor8(200,170,110,46),10,2);
            Gradient(draw,at+new float2(1),max-new float2(1),hover?new ImColor8(38,45,54):new ImColor8(30,35,40),hover?new ImColor8(26,33,43):new ImColor8(20,26,34));
            Border(draw,at,max,hover?Gold2:Gold4,1,2);
            float glyphWidth=Width(Face.Body,24,glyph);Text(draw,Face.Body,24,at+new float2((arrowWidth-glyphWidth)/2,5),hover?Gold1:LcText,glyph,0,32.4f);
            return pressed;
        }
        if(Arrow("homeModePrevious","←",controlsLeft))homeModeScrollRequest=(selected+ids.Length-1)%ids.Length;
        TextCentered(draw,Face.Body,14,navAt+new float2(controlsLeft+arrowWidth+12,11.75f),54,Gold1,$"{selected+1} / {ids.Length}",0,18.9f);
        if(Arrow("homeModeNext","→",controlsLeft+arrowWidth+12+54+12))homeModeScrollRequest=(selected+1)%ids.Length;
        if(homeModeScrollRequest>=0&&homeModeScrollRequest<ids.Length)SelectHomeHeroSet(ids[homeModeScrollRequest]);

        // .setpick-row: horizontally scrolling cards, centered on the selection.
        var rowAt=origin+new float2(pad,rowTop);var rowMax=rowAt+new float2(inner,rowHeight);
        HomeModeCarouselBounds=(rowAt,rowMax);
        float rowContent=4+ids.Length*cardWidth+(ids.Length-1)*18+4,maxScroll=Math.Max(0,rowContent-inner);
        float Centered(int index)=>Math.Clamp(4+index*(cardWidth+18)-(inner-cardWidth)/2,0,maxScroll);
        double now=HomeHeroClock();
        if(homeModeShownSet!=selectedSet)
        {
            int index=Array.IndexOf(ids,selectedSet);
            if(index>=0)
            {
                if(homeModeShownSet is null||!homePlayWasDrawn||!ClientMotion){homeModeScroll=homeModeScrollGoal=Centered(index);homeModeScrollAt=double.NaN;}
                else{homeModeScrollFrom=homeModeScroll;homeModeScrollGoal=Centered(index);homeModeScrollAt=now;}
            }
        }
        else if(!homePlayWasDrawn&&selected>=0){homeModeScroll=homeModeScrollGoal=Centered(selected);homeModeScrollAt=double.NaN;}
        if(!double.IsNaN(homeModeScrollAt))
        {
            float t=(float)Math.Clamp((now-homeModeScrollAt)/.35,0,1),eased=t<.5f?4*t*t*t:1-MathF.Pow(-2*t+2,3)/2;
            homeModeScroll=homeModeScrollFrom+(homeModeScrollGoal-homeModeScrollFrom)*eased;if(t>=1)homeModeScrollAt=double.NaN;
        }
        var mouse=io.MousePos;bool overRow=pageHovered&&mouse.X>=rowAt.X&&mouse.X<rowMax.X&&mouse.Y>=rowAt.Y&&mouse.Y<rowMax.Y;
        if(overRow&&io.MouseWheelH!=0){homeModeScroll-=io.MouseWheelH*(cardWidth+18);homeModeScrollAt=double.NaN;}
        if(overRow&&ImGui.IsMouseClicked(ImGuiMouseButton.Left)){homeModeDrag=(mouse.X,homeModeScroll);homeModeDragged=false;}
        if(homeModeDrag is {} drag&&ImGui.IsMouseDown(ImGuiMouseButton.Left)&&Math.Abs(mouse.X-drag.X)>6)
        {homeModeDragged=true;homeModeScroll=drag.Scroll+drag.X-mouse.X;homeModeScrollAt=double.NaN;}
        homeModeScroll=Math.Clamp(homeModeScroll,0,maxScroll);
        ImGui.PushClipRect(rowAt,rowMax,true);
        for(int index=0;index<ids.Length;index++)
        {
            string id=ids[index];bool on=selectedSet==id;
            var cardAt=rowAt+new float2(4+index*(cardWidth+18)-homeModeScroll,4);var cardMax=cardAt+new float2(cardWidth,cardHeight);
            string key="homeMode/"+id;homePlayControls[key]=(cardAt,cardMax);
            bool chosen=CssHit(key,cardAt,new(cardWidth,cardHeight),out bool hover);
            HomeModeCardCount++;
            if(cardMax.X<rowAt.X-4||cardAt.X>rowMax.X+4){if(chosen&&!homeModeDragged)SelectHomeHeroSet(id);continue;}
            if(on)draw.AddRect(cardAt-new float2(1),cardMax+new float2(1),Gold1,7,ImDrawFlags.None,2);
            else if(hover)Glow(draw,cardAt,cardMax,new ImColor8(200,164,98,48),18,6);
            draw.AddRectFilled(cardAt,cardMax,new ImColor8(7,19,32),6);
            var artAt=cardAt+new float2(1,1);float artWidth=Math.Min(cardWidth-2,artHeight*2/3),drawnHeight=artWidth*1.5f;
            homeArtwork.Draw("mode-"+id,artAt+new float2((cardWidth-2-artWidth)/2,(artHeight-drawnHeight)/2),new(artWidth,drawnHeight));
            var textAt=artAt+new float2(0,artHeight);
            // linear-gradient(150deg, #102438, #071320): lighter at the top left.
            draw.AddRectFilledMultiColor(textAt,cardMax-new float2(1,1),new ImColor8(16,36,56),new ImColor8(11,27,43),new ImColor8(7,19,32),new ImColor8(12,28,45));
            Border(draw,cardAt,cardMax,on?Gold1:hover?Gold2:Gold4,1,6);
            var text=textAt+new float2(18,18);
            Text(draw,Face.DisplayBold,11,text,SetAccent.GetValueOrDefault(id,new ImColor8(63,214,255)),SetModeName(SetChoices.Options.FirstOrDefault(o=>o.Id==id)?.Mode).ToUpperInvariant(),1.54f,14);
            float titleHeight=Paragraph(draw,Face.DisplayBold,titlePx,text+new float2(0,22),textWidth,titlePx*1.12f,Gold1,Title(id),titlePx*.02f);
            Paragraph(draw,Face.BodyBold,12.5f,text+new float2(0,22+titleHeight+8),textWidth,18.75f,Text2,Description(id));
            if(on)
            {
                var check=cardAt+new float2(cardWidth-1-12-14,1+12+14);
                draw.AddCircleFilled(check,14,new ImColor8(7,19,32,232));draw.AddCircle(check,13,Gold1,0,2);
                draw.AddLine(check+new float2(-5.5f,.5f),check+new float2(-1.5f,4.5f),Gold1,2);draw.AddLine(check+new float2(-1.5f,4.5f),check+new float2(5.5f,-5),Gold1,2);
            }
            if(chosen&&!homeModeDragged)SelectHomeHeroSet(id);
        }
        ImGui.PopClipRect();
        if(!ImGui.IsMouseDown(ImGuiMouseButton.Left)){homeModeDrag=null;homeModeDragged=false;}
        homeModeShownSet=selectedSet;homeModeScrollRequest=-1;

        // .setpick-options: Pace and Format toggles with their descriptions.
        float optionsHeight=4+12+DrawPaceFormat("homePlay",origin+new float2(pad,optionsTop+16),columnWidth,24,stacked)+12;

        // .ps-foot: queue cards and the hextech Confirm.
        footTop=optionsTop+optionsHeight+12;var footAt=origin+new float2(pad,footTop);
        draw.AddLine(footAt+new float2(0,.5f),footAt+new float2(inner,.5f),Gold5);
        var modeAt=footAt+new float2(0,13);float modeX=0;
        for(int i=0;i<2;i++)
        {
            bool mp=i==1;string key=mp?"homeModeOnline":"homeModeSolo",title=mp?"MULTIPLAYER":"SOLO VS AI";
            float titleWidth=Width(Face.DisplayBold,14,title,1.12f),smallWidth=compactModes?0:Width(Face.Body,11,modeText[i]);
            float cardWidthMode=Math.Min(330,8+46+10+Math.Max(titleWidth,smallWidth)+14+2);
            var at=modeAt+new float2(modeX,0);
            if(DrawPlayMode(key,title,compactModes?null:modeText[i],mp?"mode-multiplayer":"mode-solo",at,new(cardWidthMode,modeHeight),homePlayOnline==mp))homePlayOnline=mp;
            homePlayControls[key]=(at,at+new float2(cardWidthMode,modeHeight));modeX+=cardWidthMode+10;
        }
        var confirmAt=footAt+new float2(inner-(compactModes?180:220),13+(modeHeight-52)/2);float confirmWidth=compactModes?180:220;
        if(!compactModes)
        {
            // .ps-pick summary, right-aligned before the Confirm button.
            string setName=selectedSet=="chaos"?"Chaos":SetChoices.Label(selectedSet);
            (string Text,bool Bold)[] parts=[("Selected",false),(setName,true),("·",false),(SetSelection.PaceLabel(selectedPace),true),(duos?"Duos":"Free for all",true),("·",false),(homePlayOnline?"Multiplayer":"Solo vs AI",true)];
            float total=parts.Sum(p=>p.Bold?Width(Face.DisplayBold,12.5f,p.Text,.5f):Width(Face.Body,13,p.Text))+6*(parts.Length-1);
            float x=confirmAt.X-16-total,y=footAt.Y+13+(modeHeight-17.5f)/2;
            foreach(var part in parts)
            {
                x+=part.Bold?Text(draw,Face.DisplayBold,12.5f,new(x,y+.3f),Gold1,part.Text,.5f,16.875f):Text(draw,Face.Body,13,new(x,y),Muted,part.Text,0,17.55f);
                x+=6;
            }
        }
        homePlayControls["homePlayConfirm"]=(confirmAt,confirmAt+new float2(confirmWidth,52));
        if(HextechButton("homePlayConfirm","CONFIRM",confirmAt,new(confirmWidth,52))){if(homePlayOnline)OpenMultiplayerView();else homePlayParty=true;}
        ImGui.End();
    }

    /// <summary>#overlay .lc-confirm: hexagonal navy-teal button with a gold frame.</summary>
    private bool HextechButton(string key,string label,float2 at,float2 extent,float px=20,float tracking=3.6f)
    {
        bool pressed=CssHit(key,at,extent,out bool hover);var draw=ImGui.GetWindowDrawList();var max=at+extent;
        float cut=14,midY=at.Y+extent.Y/2;
        float2[] Outline(float inset)=>[new(at.X+cut+inset*.4f,at.Y+inset),new(max.X-cut-inset*.4f,at.Y+inset),new(max.X-inset,midY),new(max.X-cut-inset*.4f,max.Y-inset),new(at.X+cut+inset*.4f,max.Y-inset),new(at.X+inset,midY)];
        float alpha=ImGui.GetStyle().Alpha;
        // Gold border-image (gold-1 to gold-4, top to bottom) clipped to the hexagon, then the 2px-inset fill.
        foreach(var point in Outline(0))draw.PathLineTo(point);draw.PathFillConvex(Alpha(Gold2,alpha));
        var outer=Outline(0);
        draw.AddTriangleFilled(outer[0],outer[1],outer[2],Alpha(Gold1,alpha*.55f));
        foreach(var point in Outline(2))draw.PathLineTo(point);draw.PathFillConvex(Alpha(hover?new ImColor8(20,104,128):new ImColor8(15,82,102),alpha));
        var inner=Outline(2);
        // Lower half fades to #062033 (linear-gradient 180deg).
        draw.AddQuadFilled(new(inner[5].X,midY),new(inner[2].X,midY),inner[3],inner[4],Alpha(hover?new ImColor8(8,42,66):new ImColor8(6,32,51),alpha));
        draw.AddRectFilledMultiColor(new(inner[0].X,midY-extent.Y*.18f),new(inner[1].X,midY),Alpha(new ImColor8(15,82,102,0),alpha),Alpha(new ImColor8(15,82,102,0),alpha),Alpha(new ImColor8(6,32,51,120),alpha),Alpha(new ImColor8(6,32,51,120),alpha));
        draw.PushClipRect(at+new float2(2),max-new float2(2),true);InsetGlow(draw,at+new float2(2),max-new float2(2),new ImColor8(63,214,255,hover?(byte)153:(byte)89),20);draw.PopClipRect();
        float width=Width(Face.DisplayBold,px,label,tracking);
        Text(draw,Face.DisplayBold,px,new(at.X+(extent.X-width)/2,at.Y),Alpha(hover?new ImColor8(255,255,255):new ImColor8(205,250,250),alpha),label,tracking,extent.Y);
        return pressed;
    }

    // .btn.btn-ghost
    private bool GhostButton(string key,string label,float2 at,float px=14)
    {
        float width=Width(Face.BodyBold,px,label)+34,height=px*1.2f+18;
        bool pressed=CssHit(key,at,new(width,height),out bool hover);var draw=ImGui.GetWindowDrawList();
        if(hover){draw.AddRectFilled(at,at+new float2(width,height),new ImColor8(200,170,110,20),2);Border(draw,at,at+new float2(width,height),Gold5,1,2);}
        Text(draw,Face.BodyBold,px,at+new float2(17,9),hover?Gold1:LcMute,label,0,px*1.2f);
        return pressed;
    }

    // .btn-play: brand-orange call to action with two cut corners.
    private bool PlayButton(string key,string label,float2 at,float px,float paddingX,float paddingY,float tracking)
    {
        float lineHeight=px*1.2f,width=Width(Face.DisplayBold,px,label,tracking)+paddingX*2+2,height=lineHeight+paddingY*2+2;
        bool pressed=CssHit(key,at,new(width,height),out bool hover);var draw=ImGui.GetWindowDrawList();var max=at+new float2(width,height);
        float alpha=ImGui.GetStyle().Alpha;const float cut=10;
        float2[] shape=[new(at.X+cut,at.Y),new(max.X,at.Y),new(max.X,max.Y-cut),new(max.X-cut,max.Y),new(at.X,max.Y),new(at.X,at.Y+cut)];
        foreach(var point in shape)draw.PathLineTo(point);draw.PathFillConvex(Alpha(new ImColor8(255,224,154),alpha));
        // linear-gradient(180deg, #ffd26a 0%, #ffc24a 35%, #ff8a3d 100%) inside the 1px border.
        float split=at.Y+1+(height-2)*.35f;var top=hover?new ImColor8(255,222,128):new ImColor8(255,210,106);var middle=hover?new ImColor8(255,206,96):new ImColor8(255,194,74);var bottom=hover?new ImColor8(255,152,80):new ImColor8(255,138,61);
        draw.PushClipRect(at+new float2(1),max-new float2(1),true);
        float2[] inner=[new(at.X+cut+.6f,at.Y+1),new(max.X-1,at.Y+1),new(max.X-1,max.Y-cut-.6f),new(max.X-cut-.6f,max.Y-1),new(at.X+1,max.Y-1),new(at.X+1,at.Y+cut+.6f)];
        foreach(var point in inner)draw.PathLineTo(point);draw.PathFillConvex(Alpha(middle,alpha));
        draw.AddRectFilledMultiColor(new(at.X+cut,at.Y+1),new(max.X-1,split),Alpha(top,alpha),Alpha(top,alpha),Alpha(middle,alpha),Alpha(middle,alpha));
        draw.AddTriangleFilled(new(at.X+1,at.Y+cut+.6f),new(at.X+cut+.6f,at.Y+1),new(at.X+cut+.6f,at.Y+cut+.6f),Alpha(top,alpha));
        draw.AddRectFilled(new(at.X+1,at.Y+cut),new(at.X+cut+1,split),Alpha(middle,alpha));
        draw.AddRectFilledMultiColor(new(at.X+1,split),new(max.X-1,max.Y-cut),Alpha(middle,alpha),Alpha(middle,alpha),Alpha(Mix(middle,bottom,(max.Y-cut-split)/(max.Y-1-split)),alpha),Alpha(Mix(middle,bottom,(max.Y-cut-split)/(max.Y-1-split)),alpha));
        draw.AddRectFilledMultiColor(new(at.X+1,max.Y-cut),new(max.X-cut,max.Y-1),Alpha(Mix(middle,bottom,(max.Y-cut-split)/(max.Y-1-split)),alpha),Alpha(Mix(middle,bottom,(max.Y-cut-split)/(max.Y-1-split)),alpha),Alpha(bottom,alpha),Alpha(bottom,alpha));
        draw.AddTriangleFilled(new(max.X-cut-.6f,max.Y-cut),new(max.X-1,max.Y-cut),new(max.X-cut-.6f,max.Y-1),Alpha(Mix(middle,bottom,(max.Y-cut-split)/(max.Y-1-split)),alpha));
        // inset 0 1px rgba(255,255,255,.55), inset 0 -3px rgba(120,50,0,.25)
        draw.AddLine(new(at.X+cut,at.Y+1.5f),new(max.X-1,at.Y+1.5f),Alpha(new ImColor8(255,255,255,140),alpha));
        draw.AddRectFilled(new(at.X+1,max.Y-4),new(max.X-cut,max.Y-1),Alpha(new ImColor8(120,50,0,64),alpha));
        draw.PopClipRect();
        if(hover)Glow(draw,at,max,new ImColor8(255,138,61,90),24);
        float textWidth=Width(Face.DisplayBold,px,label,tracking);
        Text(draw,Face.DisplayBold,px,new(at.X+(width-textWidth)/2,at.Y+1+paddingY),Alpha(new ImColor8(20,13,5),alpha),label,tracking,lineHeight);
        return pressed;
    }
    private static ImColor8 Mix(ImColor8 a,ImColor8 b,float t)
    {
        var x=a.AsByte4();var y=b.AsByte4();t=Math.Clamp(t,0,1);
        return new((byte)(x.X+(y.X-x.X)*t),(byte)(x.Y+(y.Y-x.Y)*t),(byte)(x.Z+(y.Z-x.Z)*t),(byte)(x.W+(y.W-x.W)*t));
    }
    private static void Keycap(ImDrawListPtr draw,float2 at,string key,out float width)
    {
        width=Width(Face.Body,11,key)+14;var max=at+new float2(width,17.8f);
        draw.AddRectFilled(at,max,new ImColor8(28,39,62),4);draw.AddRectFilled(new(at.X+1,max.Y-2),new(max.X-1,max.Y),new ImColor8(49,66,95),2);
        Border(draw,at,max,new ImColor8(49,66,95),1,4);
        Text(draw,Face.Body,11,at+new float2(7,1),Text1,key,0,14.85f);
    }

    // .play-solo: the solo party, inside a framed panel over the lobby painting.
    private void HomeSoloParty(float2 at,float2 extent)
    {
        string setName=selectedSet=="chaos"?"Chaos":HomeSets.Value[selectedSet].Name,description=selectedSet=="chaos"?"The game seed picks the set.":HomeSets.Value[selectedSet].Description;
        float viewWidth=socialWindowSize.X,viewHeight=socialWindowSize.Y,width=extent.X,height=extent.Y;bool shortView=viewHeight<=820;
        var draw=ImGui.GetWindowDrawList();var max=at+extent;
        draw.AddRectFilled(at+new float2(0,6),max+new float2(0,6),new ImColor8(0,0,0,40));
        draw.PushClipRect(at+new float2(1),max-new float2(1),true);
        homeArtwork.Draw("lobby-bg",at+new float2(1),extent-new float2(2),255,true,.4f);
        // linear-gradient(180deg, rgba(1,10,19,.55), rgba(1,10,19,.92) 70%)
        float fadeEnd=at.Y+1+(height-2)*.7f;
        Gradient(draw,at+new float2(1),new(max.X-1,fadeEnd),new ImColor8(1,10,19,140),new ImColor8(1,10,19,235));
        draw.AddRectFilled(new(at.X+1,fadeEnd),max-new float2(1),new ImColor8(1,10,19,235));
        draw.PopClipRect();
        Border(draw,at,max,Gold4);
        float padX=shortView?16:24,padY=shortView?12:16,footPadY=shortView?8:12,footPadX=shortView?16:24;
        float startPx=shortView?20:26,startPadX=shortView?40:56,startPadY=shortView?10:14,startHeight=startPx*1.2f+startPadY*2+2;
        float footHeight=startHeight+footPadY*2+1,bodyHeight=height-2-footHeight;
        var body=at+new float2(1,1);var content=body+new float2(padX,padY);float contentWidth=width-2-2*padX;

        // .party-head
        if(GhostButton("homePlayModes","‹ Modes",content+new float2(0,2)))homePlayParty=false;
        float headLeft=Width(Face.BodyBold,14,"‹ Modes")+34+12,headWidth=Math.Min(760,contentWidth-headLeft);
        var head=content+new float2(headLeft,0);
        Text(draw,Face.DisplayBold,11,head,Gold2,"SOLO VS AI · PARTY",1.76f,14.85f);
        Text(draw,Face.DisplayBold,26,head+new float2(0,16.8f),Gold1,setName.ToUpperInvariant(),1.04f,28.6f);
        float subHeight=Paragraph(draw,Face.Body,12.5f,head+new float2(0,49.4f),headWidth,16.875f,Text2,description);
        float pickTop=49.4f+subHeight+6;
        float pickHeight=DrawPaceFormat("homeParty",head+new float2(0,pickTop),headWidth,0,true);
        float headHeight=pickTop+pickHeight;

        // .party-slots: the host's live pedestal plus seven AI Tamers (1.6fr + 7×1fr).
        bool showHelp=!shortView,fourColumns=viewWidth<=1100;
        int helpColumns=viewWidth<=1700?3:4;
        string[] help=["Buy Digimon from the shop and drag them onto your half of the board. Your level is how many Digimon you can field.","\u0001Three copies digivolve.\u0001 3 Agumon become a Greymon, 3 Greymon become a MetalGreymon.","Items sit on the pedestals left of the board. Drag one onto a Digimon. Two components make a completed item.","Your Little Legend walks where you right-click. Walk it into loot orbs to open them."];
        float helpWidth=(contentWidth-8*(helpColumns-1))/helpColumns;
        float helpHeight=showHelp?help.Take(helpColumns).Max(h=>ParagraphHeight(Face.Body,11,helpWidth-22,15.95f,h.Replace("\u0001","")))+18:0;
        float slotsTop=headHeight+14,slotsHeight=Math.Clamp(bodyHeight-padY*2-headHeight-14-(showHelp?14+helpHeight:0),220,470);
        float gap=viewWidth<=1500?6:8,lead=viewWidth<=1500?1.5f:1.6f;
        for(int seat=0;seat<8;seat++)
        {
            float2 cardAt,cardExtent;
            if(fourColumns)
            {
                float cardWidth=(contentWidth-gap*3)/4,rowHeight=(slotsHeight-gap)/2;
                cardAt=content+new float2((seat%4)*(cardWidth+gap),slotsTop+(seat/4)*(rowHeight+gap));cardExtent=new(cardWidth,rowHeight);
            }
            else
            {
                float unit=(contentWidth-gap*7)/(lead+7),cardWidth=seat==0?unit*lead:unit;
                cardAt=content+new float2(seat==0?0:unit*lead+gap+(seat-1)*(unit+gap),slotsTop);cardExtent=new(cardWidth,slotsHeight);
            }
            DrawSoloPartySeat(seat,cardAt,cardExtent);
        }
        if(showHelp)
        {
            float helpTop=slotsTop+slotsHeight+14;
            for(int i=0;i<helpColumns;i++)
            {
                var helpAt=content+new float2(i*(helpWidth+8),helpTop);var helpMax=helpAt+new float2(helpWidth,helpHeight);
                draw.AddRectFilled(helpAt,helpMax,new ImColor8(1,10,19,191));Border(draw,helpAt,helpMax,Gold5);draw.AddRectFilled(helpAt,new(helpAt.X+2,helpMax.Y),Gold3);
                HelpParagraph(draw,helpAt+new float2(12,9),helpWidth-22,help[i]);
            }
        }

        // .play-foot: key reminders and Start.
        var footAt=new float2(body.X,body.Y+bodyHeight);var footMax=new float2(max.X-1,max.Y-1);
        draw.AddRectFilled(footAt,footMax,new ImColor8(1,10,19,230));draw.AddLine(footAt+new float2(0,.5f),new(footMax.X,footAt.Y+.5f),Gold4);
        float keysY=footAt.Y+1+footPadY+(startHeight-17.8f)/2,x=footAt.X+footPadX;
        foreach(var (key,action) in new[]{("D","reroll"),("E","sell"),("Space","fight"),("T","emotes"),("C","codex")})
        {
            x+=6;Keycap(draw,new(x,keysY),key,out float capWidth);x+=capWidth+6;
            // .play-keys: gap 6 between items plus each kbd
        }
        float startWidth=Width(Face.DisplayBold,startPx,"START",startPx*.14f)+startPadX*2+2;
        bool inRoom=networkRoom is not null||steam?.Room is not null;ImGui.BeginDisabled(inRoom);
        if(PlayButton("homePlayStart","START",new(footMax.X-footPadX-startWidth,footAt.Y+1+footPadY),startPx,startPadX,startPadY,startPx*.14f)){showHomePlay=false;StartLocalMatch(duos);}
        ImGui.EndDisabled();
        HomePlayLayoutFits&=slotsHeight>=100&&padY+slotsTop+slotsHeight+(showHelp?14+helpHeight:0)<=bodyHeight+.5f;
    }
    // .howto li: muted text with an optional bold lead sentence (marked with \u0001).
    private static void HelpParagraph(ImDrawListPtr draw,float2 at,float width,string text)
    {
        string plain=text.Replace("\u0001","");int boldEnd=text.StartsWith('\u0001')?text.IndexOf('\u0001',1)-1:0;
        var lines=Wrap(Face.Body,11,plain,width);int offset=0;
        for(int line=0;line<lines.Count;line++)
        {
            string value=lines[line];float x=0;var lineAt=at+new float2(0,line*15.95f);
            int boldCount=Math.Clamp(boldEnd-offset,0,value.Length);
            if(boldCount>0)x+=Text(draw,Face.BodyBold,11,lineAt,Gold1,value[..boldCount],0,15.95f);
            string rest=value[boldCount..];
            if(rest.StartsWith(' ')){x+=Width(Face.Body,11,"a a")-Width(Face.Body,11,"aa");rest=rest.TrimStart();}
            Text(draw,Face.Body,11,lineAt+new float2(x,0),Text2,rest,0,15.95f);
            offset+=value.Length+1;
        }
    }
    private static uint PartyHash(string text){uint h=2166136261;foreach(char c in text)h=unchecked((h^c)*16777619);return h;}
    private void DrawSoloPartySeat(int seat,float2 at,float2 extent)
    {
        bool yours=seat==0;string name=yours?profile?.Progress.Name is {Length:>0} own?own:"You":LobbyAiNames[seat-1];var draw=ImGui.GetWindowDrawList();var max=at+extent;
        const float plateHeight=45.8f;float stageHeight=extent.Y-2-plateHeight;
        string? wallet=profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),legends=profile?.Storage.GetValueOrDefault("dt.legendevo.v1");
        string? ownForm=null;int ownLevel=1;
        if(yours)
        {
            Glow(draw,at,max,new ImColor8(200,170,110,51),24);
            Gradient(draw,at+new float2(1),max-new float2(1),new ImColor8(10,20,40,77),new ImColor8(1,10,19,230));
            // radial-gradient(circle at 50% 55%, rgba(63,214,255,.18), transparent 60%)
            draw.PushClipRect(at,max,true);Radial(draw,at+new float2(extent.X*.5f,extent.Y*.55f),FarthestCorner(extent,.5f,.55f)*.6f,new ImColor8(63,214,255,46));draw.PopClipRect();
            var look=CosmeticLoadouts.Local(wallet,legends);string id=look["legend"]?.GetValue<string>()??"tokomon";
            var evolution=LegendEvolution.Inspect(legends,wallet,profile?.Storage.GetValueOrDefault("dt.farm.v1"),id);
            ownForm=LegendEvolution.Form(evolution.Form).Name;ownLevel=evolution.Level;
            // Frame the whole pedestal for this card instead of cropping a wide preview.
            homeLegend??=new(legendPedestal);homeLegend.Draw("legend",id,LegendEvolution.VisualKey(id,evolution.Form),boardSceneTime,ClientMotion,stageAspect:(extent.X-2)/stageHeight);
            var stageAt=at+new float2(1);draw.PushClipRect(stageAt,stageAt+new float2(extent.X-2,stageHeight),true);
            draw.AddImage(new ImTextureRef{_TexID=new((nint)homeLegend.Texture)},stageAt,stageAt+new float2(extent.X-2,stageHeight),new(0,1),new(1,0));draw.PopClipRect();
            Border(draw,at,max,Gold2);
            var crown=at+new float2(9,9);
            float2[] points=[crown+new float2(0,12),crown+new float2(0,2.4f),crown+new float2(4.5f,7.2f),crown+new float2(9,0),crown+new float2(13.5f,7.2f),crown+new float2(18,2.4f),crown+new float2(18,12)];
           foreach(var point in points)draw.PathLineTo(point);draw.PathFillConcave(Gold2);
        }
        else
        {
            Gradient(draw,at+new float2(1),max-new float2(1),new ImColor8(10,20,40,128),new ImColor8(1,10,19,235));Border(draw,at,max,Gold5);
            // Each AI seat shows the same legend the original party picks: hash("party:<name>:<index>").
            var pool=CosmeticCollection.Items("legend").Where(item=>!item.Secret).ToArray();
            string? key=pool.Length==0?null:LegendEvolution.VisualKey(pool[PartyHash($"party:{name}:{seat-1}")%pool.Length].Id,null);
            float portrait=Math.Max(24,extent.X-2-16);var portraitAt=at+new float2(1+8,1+stageHeight-14-portrait);
            var glow=at+new float2(extent.X*.5f,1+stageHeight-8-8);
            for(int ring=8;ring>=1;ring--)draw.AddEllipseFilled(glow,new float2(extent.X*.36f*ring/8,8*ring/8f),new ImColor8(63,214,255,(byte)(89f/8*(1-(ring-1)/8f)*1.2f)));
            portraits.DrawCompanion(key,portraitAt,portrait,230);
        }
        var plateAt=at+new float2(1,1+stageHeight);var plateMax=max-new float2(1);draw.AddRectFilled(plateAt,plateMax,new ImColor8(1,10,19,217));draw.AddLine(plateAt+new float2(0,.5f),new(plateMax.X,plateAt.Y+.5f),Gold5);
        string label=Ellipsis(Face.DisplayBold,12.5f,name.ToUpperInvariant(),extent.X-10,.5f);
        TextCentered(draw,Face.DisplayBold,12.5f,plateAt+new float2(0,7),extent.X-2,yours?new ImColor8(255,255,255):Gold1,label,.5f,16);
        string sub=ownForm is not null?ownForm+" · Lv "+ownLevel:duos&&seat==1?"AI Partner":"AI Tamer";
        TextCentered(draw,Face.Body,11,plateAt+new float2(0,23),extent.X-2,LcMute,Ellipsis(Face.Body,11,sub,extent.X-10),0,14.85f);
        Buttons["homePartySeat/"+seat]=at+extent*.5f;HomePartySeatCount++;
    }

    // #overlay .mode-card
    private bool DrawPlayMode(string key,string title,string? description,string art,float2 at,float2 extent,bool selected)
    {
        bool clicked=CssHit(key,at,extent,out bool hover);var draw=ImGui.GetWindowDrawList();var max=at+extent;
        if(selected)
        {
            GradientX(draw,at,max,new ImColor8(11,58,74,179),new ImColor8(1,10,19,204));
            draw.PushClipRect(at,max,true);InsetGlow(draw,at,max,new ImColor8(63,214,255,31),22);draw.PopClipRect();
            draw.AddRect(at-new float2(.5f),max+new float2(.5f),Gold2,2);Border(draw,at,max,Gold1,1,2);
        }
        else{draw.AddRectFilled(at,max,new ImColor8(1,10,19,191),2);Border(draw,at,max,hover?Gold3:Gold5,1,2);}
        var artCenter=at+new float2(1+8+23,extent.Y/2);
        if(selected)Glow(draw,artCenter-new float2(23),artCenter+new float2(23),new ImColor8(240,210,140,102),12,23);
        homeArtwork.Draw(art,artCenter-new float2(23),new(46,46),255,true,rounding:23);
        draw.AddCircle(artCenter,22.5f,selected?Gold1:Gold4,48,1);
        float textLeft=1+8+46+10,textWidth=extent.X-textLeft-15;
        float blockHeight=description is null?18.9f:18.9f+2+ParagraphHeight(Face.Body,11,textWidth,14.85f,description);
        float top=(extent.Y-blockHeight)/2;
        Text(draw,Face.DisplayBold,14,at+new float2(textLeft,top),Gold1,title,1.12f,18.9f);
        if(description is not null)Paragraph(draw,Face.Body,11,at+new float2(textLeft,top+18.9f+2),textWidth,14.85f,Text2,description);
        return clicked;
    }
}
