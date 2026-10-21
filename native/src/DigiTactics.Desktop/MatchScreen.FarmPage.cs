using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;
using S = DigiTactics.Desktop.CosmeticStyle;

namespace DigiTactics.Desktop;

/// <summary>
/// Digi-Farm chrome from v1.0.14 js/farm-ui.js, js/farmstyle.js and the theme.js .df-* restyle:
/// teal-to-navy header with supply pills and room buttons, resident chips, the rounded action bar and
/// the compact resident card (meters, tags, training, next digivolution, care pushes).
/// </summary>
internal sealed partial class MatchScreen
{
    private static readonly ImColor8 FarmPanel2=new(21,30,49),FarmLine=new(34,48,73),FarmLine2=new(49,66,95),FarmBg=new(10,15,26),FarmMuted=new(143,154,176),FarmYellow=new(255,207,74);
    private static ImColor8 FarmCareColor(string care)=>care switch{"bond"=>new(255,143,176),"battle"=>new(255,154,90),"food"=>new(255,211,106),_=>new(159,224,122)};

    private void FarmHeader(JsonNode state,string[] residents)
    {
        float top=ClientPageTop+12,width=size.X-48;bool narrow=FarmHeaderHeight>52;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
        Window("Digi-Farm header",24,top,width,FarmHeaderHeight,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleVar(2);
        var draw=ImGui.GetWindowDrawList();var at=new float2(24,top);var end=at+new float2(width,FarmHeaderHeight);
        // .df-win frame and .df-head: teal-to-navy wash, gold-5 bottom rule
        draw.AddRectFilledMultiColor(at,end,new ImColor8(14,40,40,230),new ImColor8(14,40,40,230),new ImColor8(1,10,19,179),new ImColor8(1,10,19,179));
        draw.AddRectFilled(at,end,new ImColor8(5,14,25,120));
        draw.AddRect(at,end+new float2(0,1),S.Gold4);draw.AddLine(new(at.X,end.Y-.5f),new(end.X,end.Y-.5f),S.Gold5);
        float rowY=narrow?at.Y+4:at.Y,rowHeight=narrow?40:FarmHeaderHeight;
        float x=at.X+13+S.TextIn(draw,new(at.X+13,rowY),rowHeight,S.Gold1,"DIGI-FARM",S.Display(19,.08f))+14;
        string theme=FarmCare.Themes.FirstOrDefault(t=>t.Id==state["theme"]!.GetValue<string>())?.Name??"Room";
        FarmHeaderGlyph(draw,"theme",new(x+6,rowY+rowHeight*.5f));
        S.TextIn(draw,new(x+16,rowY),rowHeight,FarmMuted,theme,S.Body(12));
        // buttons, right to left: Close, Expand, Care staff, Themes, Digi-Bank
        var buttonType=S.Body(13,true);float bx=end.X-12;
        ImGui.BeginDisabled(farmManagement||training is not null);
        bool Head(string key,string label,float rightEdge,out float left,bool active=false,int? price=null,bool disabled=false)
        {
            float w=buttonType.Width(label)+22+(price is null?0:6+S.Price(draw,default,26,price.Value,null,S.LcText,13,true));left=rightEdge-w;
            var bAt=new float2(left,rowY+(rowHeight-28)*.5f);
            bool pressed=CosmeticButton(key,bAt,new(w,28),price is null?label:"",buttonType,disabled:disabled,ink:active?FarmYellow:null);
            if(price is not null)
            {
                float lw=buttonType.Width(label);var ink=S.Alpha(S.LcText,disabled?.45f:1);
                S.TextIn(draw,bAt+new float2(11,0),28,ink,label,buttonType);
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha,disabled?.45f:1);S.Price(draw,bAt+new float2(11+lw+6,0),28,price.Value,null,S.LcText,13);ImGui.PopStyleVar();
            }
            if(active)draw.AddRect(bAt,bAt+new float2(w,28),FarmYellow);
            return pressed;
        }
        if(Head("closeFarm","Close",bx,out float left))BackClient();bx=left-8;
        int cap=state["cap"]!.GetValue<int>(),price=FarmCare.ExpansionPrice(cap);
        if(cap<12){if(Head("farmHeadExpand","Expand ·",bx,out left,price:price,disabled:profile!.Progress.Bits<price))FarmAct(FarmCareAction.Expand);bx=left-8;}
        if(Head("farmCaretakers","Care staff",bx,out left,FarmCaretaker.Current(state,DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) is not null)){farmManagement=true;farmManagementPage="caretakers";farmActionMenu=null;}bx=left-8;
        if(Head("farmThemes","Themes",bx,out left)){farmManagement=true;farmManagementPage="themes";farmActionMenu=null;}bx=left-8;
        if(Head("farmRoom","Digi-Bank",bx,out left)){farmManagement=true;farmManagementPage="bank";farmActionMenu=null;}bx=left-10;
        ImGui.EndDisabled();
        // .df-pill supplies, right-aligned before the buttons (second row when narrow)
        var pill=S.Body(12);var pillBold=S.Body(12,true);var daily=state["daily"]!;
        var pills=new (string Icon,string? Value,string? Suffix,bool Capped,string Tip)[]{
            ("room",$"{residents.Length}/{cap}",(string?)null,false,"Legends in the room / room size"),
            ("meat",$"{state["meat"]!["n"]}/3",(string?)null,false,"Free meat on the tray"),
            ("bandage",$"{state["bandage"]!["n"]}/2",(string?)null,false,"Bandages"),
            ("bits",$"{daily["bits"]}/60","today",daily["bits"]!.GetValue<int>()>=60,"Bits earned on the farm today (daily cap)"),
            ("data",$"{daily["data"]}/100",(string?)null,daily["data"]!.GetValue<int>()>=100,"Digi-Data earned on the farm today (daily cap)"),
            ("bits",profile!.Progress.Bits.ToString(),(string?)null,false,"Your Bits")};
        float PillWidth((string Icon,string? Value,string? Suffix,bool Capped,string Tip) p)=>8+17+4+pillBold.Width(p.Value!)+(p.Suffix is null?0:4+pill.Width(p.Suffix))+8;
        float total=pills.Sum(PillWidth)+5*(pills.Length-1);
        float px=narrow?at.X+13:bx-total,py=narrow?at.Y+44+(FarmHeaderHeight-44-24)*.5f:rowY+(rowHeight-24)*.5f;
        foreach(var p in pills)
        {
            float w=PillWidth(p);var pAt=new float2(px,py);
            draw.AddRectFilled(pAt,pAt+new float2(w,24),new ImColor8(1,10,19,191));draw.AddRect(pAt,pAt+new float2(w,24),p.Capped?new ImColor8(255,211,106,136):S.Gold5);
            FarmHeaderGlyph(draw,p.Icon,pAt+new float2(8+8,12));
            float tx=pAt.X+8+17+4;var ink=p.Capped?new ImColor8(255,211,106):S.Text1;
            tx+=S.TextIn(draw,new(tx,pAt.Y),24,ink,p.Value!,pillBold);
            if(p.Suffix is not null)S.TextIn(draw,new(tx+4,pAt.Y),24,p.Capped?ink:S.Text1,p.Suffix,pill);
            if(ImGui.IsMouseHoveringRect(pAt,pAt+new float2(w,24)))ImGui.SetTooltip(p.Tip);
            px+=w+5;
        }
        ImGui.End();
    }
    /// <summary>Small drawn stand-ins for the header's emoji (house, meat, bandage, gem, disk, flower).</summary>
    private void FarmHeaderGlyph(ImDrawListPtr draw,string icon,float2 c)
    {
        switch(icon)
        {
            case "bits":S.BitsGem(draw,c,10);break;
            case "meat":case "bandage":
                farmSymbolTexture??=new(DigiTactics.Assets.PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"content/farm-symbols.png"))),new(MinFilter:9729,WrapS:33071,WrapT:33071),false);
                int index=(int)(icon=="meat"?FarmSymbol.Meat:FarmSymbol.Bandage);
                draw.AddImage(new ImTextureRef{_TexID=new((nint)farmSymbolTexture.Id)},c-new float2(8,8),c+new float2(8,8),new(index/18f,0),new((index+1)/18f,1));
                break;
            case "room":
                draw.AddTriangleFilled(c+new float2(0,-7),c+new float2(7,-1),c+new float2(-7,-1),new ImColor8(230,90,70));
                draw.AddRectFilled(c+new float2(-5,-1),c+new float2(5,6),new ImColor8(240,200,140));draw.AddRectFilled(c+new float2(-1.5f,1.5f),c+new float2(1.5f,6),new ImColor8(140,90,60));
                break;
            case "data":
                draw.AddRectFilled(c-new float2(6,6),c+new float2(6,6),new ImColor8(110,90,190),2);draw.AddRectFilled(c+new float2(-3.5f,-6),c+new float2(3.5f,-1.5f),new ImColor8(230,230,240));
                draw.AddRectFilled(c+new float2(-4,1.5f),c+new float2(4,6),new ImColor8(240,240,250));
                break;
            case "theme":
                draw.AddCircleFilled(c,2.2f,new ImColor8(255,214,90));
                for(int i=0;i<5;i++){float a=i*MathF.PI*2/5-MathF.PI/2;draw.AddCircleFilled(c+new float2(MathF.Cos(a),MathF.Sin(a))*3.6f,2.4f,new ImColor8(255,170,200));}
                draw.AddCircleFilled(c,2.2f,new ImColor8(255,214,90));
                break;
        }
    }
    /// <summary>.df-chip resident buttons over the scene: 42px, translucent navy, gold ring when selected, mood icon badge.</summary>
    internal (float2 Min,float2 Max) FarmSceneBounds=>(new(FarmSceneX,FarmSceneTop),new(FarmSceneX+FarmSceneWidth,FarmSceneTop+FarmSceneHeight));
    private void FarmResidentChips(string[] residents)
    {
        var draw=ImGui.GetWindowDrawList();float x=FarmSceneX+8,y=FarmSceneTop+8;
        foreach(string id in residents)
        {
            if(x+42>FarmSceneX+FarmSceneWidth-8){x=FarmSceneX+8;y+=47;}
            var at=new float2(x,y);bool selected=id==farmPet;
            bool pressed=CosmeticHit("farmResident/"+id,at,new(42,42),out bool hover);
            draw.AddRectFilled(at,at+new float2(42,42),new ImColor8(20,26,38,191),9);
            draw.AddRect(at,at+new float2(42,42),hover?S.Gold2:S.Gold4,9);
            if(selected)draw.AddRect(at-new float2(2,2),at+new float2(44,44),FarmYellow,10,thickness:2);
            portraits.DrawEvolution(FarmDetails(id).Evolution.Form,at+new float2(2,2),38,true);
            if(farmView?.Mood(id) is {Icon: not 9} mood)
            {
                farmMoodTexture??=new(DigiTactics.Assets.PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"content/farm-moods.png"))),new(MinFilter:9729,WrapS:33071,WrapT:33071),false);
                var badge=at+new float2(42,-1);draw.AddCircleFilled(badge,8,new ImColor8(255,255,255));
                draw.AddImage(new ImTextureRef{_TexID=new((nint)farmMoodTexture.Id)},badge-new float2(7,7),badge+new float2(7,7),new(mood.Icon/10f,0),new((mood.Icon+1)/10f,1));
            }
            if(hover){ImGui.BeginTooltip();ImGui.Text(CosmeticCollection.Item("legend",id)!.Name);ImGui.EndTooltip();}
            if(pressed){farmPet=id;FarmAct(FarmCareAction.Pet);}
            x+=47;
        }
        ImGui.SetCursorScreenPos(new(FarmSceneX+8,y+47));
    }
    /// <summary>.df-bar: rounded translucent dock of 64px .df-act buttons (icon over bold label), active and alert frames.</summary>
    private void FarmActionBar(JsonNode state)
    {
        var draw=ImGui.GetWindowDrawList();const int count=8;
        float buttonWidth=Math.Min(64,(FarmSceneWidth-16-12-(count-1)*6)/(float)count);const float buttonHeight=48;
        float dockWidth=buttonWidth*count+(count-1)*6+12+2,dockHeight=buttonHeight+12+2;
        var dockAt=new float2(FarmSceneX+(FarmSceneWidth-dockWidth)*.5f,FarmSceneTop+FarmSceneHeight-10-dockHeight);
        for(int i=1;i<=8;i++)draw.AddRect(dockAt+new float2(-i,6-i*.5f),dockAt+new float2(dockWidth+i,dockHeight+6+i),new ImColor8(0,0,0,(byte)(16*(1-i/9f))),14+i,thickness:2);
        draw.AddRectFilled(dockAt,dockAt+new float2(dockWidth,dockHeight),new ImColor8(14,18,28,209),14);
        draw.AddRect(dockAt,dockAt+new float2(dockWidth,dockHeight),FarmLine2,14);
        string? selected=farmPet;var details=selected is null?null:FarmDetails(selected);var pet=selected is null?null:state["pets"]?[selected];
        bool lights=state["lights"]!.GetValue<bool>();
        var label=S.Body(11,true);float x=dockAt.X+7;
        ImGui.BeginDisabled(farmManagement||training is not null||selected is null);
        void Act(string key,string text,FarmUiIcon icon,Action action,bool active=false,bool alert=false)
        {
            var at=new float2(x,dockAt.Y+7);var extent=new float2(buttonWidth,buttonHeight);
            bool pressed=CosmeticHit(key,at,extent,out bool hover);
            bool disabled=farmManagement||training is not null||selected is null;
            float alpha=disabled?.45f:1;
            var top=hover&&!disabled?new ImColor8(38,45,54):new ImColor8(30,35,40);var bottom=hover&&!disabled?new ImColor8(26,33,43):new ImColor8(20,26,34);
            int first=draw.VtxBuffer.Count;
            draw.AddRectFilled(at,at+extent,top,10);
            S.VerticalGradient(draw,first,at.Y,at.Y+extent.Y,top,S.Mix(top,.5f,bottom),.5f,bottom);
            if(alert&&!disabled)for(int i=1;i<=5;i++)draw.AddRect(at-new float2(i,i),at+extent+new float2(i,i),new ImColor8(255,138,90,(byte)(26*(1-i/6f))),10+i,thickness:2);
            draw.AddRect(at,at+extent,S.Alpha(active?FarmYellow:alert?new ImColor8(255,138,90):hover?S.Gold2:S.Gold4,alpha),10);
            DrawFarmUiIcon(draw,icon,at+new float2(buttonWidth*.5f,5+11),13);
            var ink=S.Alpha(active?FarmYellow:hover?S.Gold1:S.LcText,alpha);
            S.TextIn(draw,new(at.X+(buttonWidth-label.Width(text))*.5f,at.Y+5+22+1),13.3f,ink,text,label);
            if(pressed)action();
            x+=buttonWidth+6;
        }
        double hunger=pet?["hunger"]?.GetValue<double>()??100,energy=pet?["energy"]?.GetValue<double>()??100;
        bool piles=state["pets"]!.AsObject().Any(p=>p.Value?["poop"]?.AsArray().Count>0);
        Act("farmAction/feed","Feed",FarmUiIcon.Meat,()=>farmActionMenu=farmActionMenu=="feed"?null:"feed",farmActionMenu=="feed",hunger<25);
        Act("farmClean","Clean",FarmUiIcon.Broom,()=>FarmAct(FarmCareAction.Clean),false,piles);
        Act("farmAction/cure","Medicine",FarmUiIcon.Pill,()=>farmActionMenu=farmActionMenu=="cure"?null:"cure",farmActionMenu=="cure",details?.Sick==true);
        Act("farmLights",lights?"Lights off":"Lights on",FarmUiIcon.Bulb,()=>FarmAct(FarmCareAction.Lights),!lights,lights&&energy<20);
        Act("farmPraise","Praise",FarmUiIcon.Clap,()=>FarmAct(FarmCareAction.Praise));
        Act("farmScold","Scold",FarmUiIcon.Anger,()=>FarmAct(FarmCareAction.Scold),false,FarmFlag(pet?["tantrum"]));
        Act("farmAction/train","Train",FarmUiIcon.Lifter,()=>farmActionMenu=farmActionMenu=="train"?null:"train",farmActionMenu=="train");
        Act("farmPlay","Play",FarmUiIcon.Ball,()=>FarmAct(FarmCareAction.Play));
        ImGui.EndDisabled();
    }

    /// <summary>Farm flags are saved as booleans or timestamps; any truthy value counts.</summary>
    private static bool FarmFlag(JsonNode? node)=>node?.GetValueKind() switch
    {
        System.Text.Json.JsonValueKind.True=>true,System.Text.Json.JsonValueKind.Number=>node.GetValue<double>()!=0,_=>false
    };
    private void FarmResidentCard(JsonNode state,string[] residents,string id)
    {
        var pet=state["pets"]![id]!;var details=FarmDetails(id);DisplayedFarmDetails=details;
        var form=LegendEvolution.Form(details.Evolution.Form);
        var draw=ImGui.GetWindowDrawList();var origin=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;float y=origin.Y,x=origin.X;
        // .df-card: navy gradient with a gold-5 left rule; boxes paint their fills on channel 0 beneath the text.
        var window=ImGui.GetWindowPos();var windowEnd=window+ImGui.GetWindowSize();
        draw.AddRectFilledMultiColor(window,windowEnd,new ImColor8(9,18,35),new ImColor8(9,18,35),new ImColor8(1,10,19),new ImColor8(1,10,19));
        draw.AddLine(new(window.X+.5f,window.Y),new(window.X+.5f,windowEnd.Y),S.Gold5);
        draw.ChannelsSplit(2);draw.ChannelsSetCurrent(1);
        // .df-who
        FarmPortraitTile(draw,new(x,y),56,12);
        portraits.DrawEvolution(form.Id,new(x+2,y+2),52,true);
        var mood=farmView?.Mood(id);var moodType=S.Body(12,true);string moodText=mood?.Text??"";
        float moodWidth=moodText.Length==0?0:moodType.Width(moodText)+16;
        S.TextFit(draw,new(x+66,y+28-21+(21-S.Body(17,true).Size)*.5f),S.Text1,form.Name,S.Body(17,true),width-66-moodWidth-10);
        string baseName=details.Evolution.Form!=id?CosmeticCollection.Item("legend",id)!.Name+"'s ":"";
        S.TextFit(draw,new(x+66,y+29+(16-S.Body(12).Size)*.5f),FarmMuted,$"{baseName}{form.Level} · Lv {details.Evolution.Level}",S.Body(12),width-66-moodWidth-10);
        if(moodWidth>0)
        {
            var mAt=new float2(x+width-moodWidth,y+28-10);
            draw.AddRectFilled(mAt,mAt+new float2(moodWidth,20),FarmBg,9);draw.AddRect(mAt,mAt+new float2(moodWidth,20),FarmLine2,9);
            S.TextIn(draw,mAt+new float2(8,0),20,S.Text1,moodText,moodType);
        }
        y+=56+8;
        // meters
        var statType=S.Body(12);
        float boxTop=y;y+=9;
        foreach(var (key,name,a,b) in new[]{("hunger","Hunger",new ImColor8(255,154,60),new ImColor8(255,211,106)),("energy","Energy",new ImColor8(47,155,255),new ImColor8(143,240,255)),("happy","Happiness",new ImColor8(255,95,143),new ImColor8(255,179,200)),("disc","Discipline",new ImColor8(139,108,255),new ImColor8(198,182,255))})
        {
            double value=pet[key]!.GetValue<double>();
            FarmStatIcon(draw,key,new(x+11+9,y+9));
            S.TextIn(draw,new(x+11+18+6,y),18,S.Text1,name,statType);
            float meterX=x+11+18+6+74+6,meterW=width-22-18-74-30-18;
            FarmMeterBar(draw,new(meterX,y+4.5f),new(meterW,9),(float)(value/100),a,b,value<20);
            string n=$"{Math.Round(value):0}";S.TextIn(draw,new(x+width-11-statType.Width(n),y),18,FarmMuted,n,statType);
            y+=18;
        }
        y+=9;FarmBoxFrame(draw,new(x,boxTop),new(width,y-boxTop));y+=8;
        // .df-tags
        var tagType=S.Body(11);float tx=x;
        void Tag(string text,ImColor8? tone,string? glyph=null)
        {
            float w=8+(glyph is null?0:15)+tagType.Width(text)+8;if(tx>x&&tx+w>x+width){tx=x;y+=25;}
            var at=new float2(tx,y);var ink=tone??S.Text1;
            draw.AddRectFilled(at,at+new float2(w,21),FarmBg,9);draw.AddRect(at,at+new float2(w,21),tone is {} t?S.Alpha(t,.4f):FarmLine2,9);
            float gx=at.X+8;if(glyph is not null){FarmTagGlyph(draw,glyph,new(gx+6,at.Y+10.5f),ink);gx+=15;}
            S.TextIn(draw,new(gx,at.Y),21,ink,text,tagType);tx+=w+4;
        }
        var good=new ImColor8(159,224,122);var bad=new ImColor8(255,154,122);
        Tag($"{pet["weight"]} G"+(details.Fat?" · chubby":details.Thin?" · thin":""),details.Fat||details.Thin?bad:good,"weight");
        Tag($"{details.AgeDays} day{(details.AgeDays==1?"":"s")}",null,"age");
        Tag($"✗ {details.Mistakes} care mistake{(details.Mistakes==1?"":"s")}",details.Mistakes>0?bad:good);
        if(details.Sick)Tag("Sick"+(pet["sickWhy"]?.GetValue<string>() switch{"dirty"=>" (dirty room)","hungry"=>" (hungry)",_=>""}),bad);
        if(FarmFlag(pet["tantrum"]))Tag("Tantrum: scold it",bad);
        if(FarmFlag(pet["flags"]?["asleep"]))Tag("Asleep",null);
        y+=21+8;
        // training
        boxTop=y;y+=9;
        FarmBoxHeading(draw,new(x+11,y),width-22,"TRAINING","cap 300");y+=14.85f+5;
        float column=(width-22-10)/2;
        string[][] stats=[["str","Strength"],["spd","Speed"],["wis","Wisdom"],["sta","Stamina"]];
        for(int i=0;i<4;i++)
        {
            var at=new float2(x+11+i%2*(column+10),y+i/2*(16.2f+2+6+4));
            S.TextIn(draw,at,16.2f,S.Text1,stats[i][1],statType);string v=pet[stats[i][0]]!.ToString();S.TextIn(draw,new(at.X+column-S.Body(12,true).Width(v),at.Y),16.2f,S.Text1,v,S.Body(12,true));
            FarmMeterBar(draw,at+new float2(0,16.2f+2),new(column,6),(float)(pet[stats[i][0]]!.GetValue<double>()/300),new(159,224,122),new(232,255,154),false);
        }
        y+=2*(16.2f+2+6)+4;
        if(details.TrainingRefusal is {} refusal){y+=4;y+=S.Paragraph(draw,new(x+11,y),bad,refusal,S.Body(11),width-22,1.35f);}
        y+=9;FarmBoxFrame(draw,new(x,boxTop),new(width,y-boxTop));y+=8;
        // next digivolution
        boxTop=y;y+=9;var evolution=details.Evolution;FarmEvolutionBranches=0;
        FarmBoxHeading(draw,new(x+11,y),width-22,"NEXT DIGIVOLUTION",evolution.CanDigivolve?"ready now":evolution.Next is {} next?$"at level {next}":null,evolution.CanDigivolve);y+=14.85f+5;
        var known=LegendEvolution.Discovery(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),id).Dex.ToHashSet();
        foreach(var branch in evolution.Odds)
        {
            bool isKnown=known.Contains(branch.Form);var at=new float2(x+11,y+1);
            HudPortraitTile(draw,at,26,FarmFormColor(branch.Form),6);portraits.DrawEvolution(branch.Form,at,26,isKnown);
            if(!isKnown)draw.AddRect(at+new float2(1,1),at+new float2(25,25),new ImColor8(120,170,255,60),6);
            S.TextFit(draw,new(at.X+32,at.Y+(26-statType.Size)*.5f),S.Text1,isKnown?LegendEvolution.Form(branch.Form).Name:"???",statType,width-22-32-64-34-18);
            var care=FarmCareColor(branch.Care);var tgType=S.Body(10);string tg=char.ToUpperInvariant(branch.Care[0])+branch.Care[1..];
            var tAt=new float2(x+width-11-34-6-64,at.Y+5);
            draw.AddRect(tAt,tAt+new float2(64,16),FarmLine2,8);
            float tw=tgType.Width(tg)+11;FarmTagGlyph(draw,branch.Care,new(tAt.X+(64-tw)*.5f+4,tAt.Y+8),care);S.TextIn(draw,new(tAt.X+(64-tw)*.5f+11,tAt.Y),16,care,tg,tgType);
            string pc=$"{branch.Pct}%";S.TextIn(draw,new(x+width-11-S.Body(12,true).Width(pc),at.Y+5),16,S.Text1,pc,S.Body(12,true));
            y+=26+2;FarmEvolutionBranches++;
        }
        if(evolution.Odds.Length==0){y+=S.Paragraph(draw,new(x+11,y),FarmMuted,"Final form. Revert in the Digivolve panel to try other branches.",S.Body(12),width-22,1.35f);}
        y+=6;
        var evoType=S.Body(12,true);
        if(CosmeticButton("farmDigivolve",new(x+11,y),new(width-22,24),"Open Digivolve",evoType,ink:evolution.CanDigivolve?S.TextInv:null))OpenFarmEvolution(id);
        if(evolution.CanDigivolve)
        {
            var lo=new float2(x+11,y);var hi=lo+new float2(width-22,24);
            draw.AddRectFilledMultiColor(lo,hi,new ImColor8(255,210,122),new ImColor8(255,210,122),S.Brand,S.Brand);draw.AddRect(lo,hi,new ImColor8(240,210,140));
            S.TextIn(draw,new(lo.X+(width-22-evoType.Width("Open Digivolve"))*.5f,lo.Y),24,S.TextInv,"Open Digivolve",evoType);
        }
        y+=24+9;FarmBoxFrame(draw,new(x,boxTop),new(width,y-boxTop));y+=8;
        // care pushes
        boxTop=y;y+=9;FarmBoxHeading(draw,new(x+11,y),width-22,"FARM CARE PUSHES BRANCHES",null);y+=14.85f+5;
        var bonus=details.Care.Bonus;var pushType=S.Body(11);var pushBold=S.Body(11,true);
        foreach(var (key,name,description) in new[]{("bond","Bond","Praise, play and petting"),("battle","Battle","Training stats (every 25 points)"),("food","Food","Meals when hungry (carrots count more)"),("wild","Wild",$"{details.Mistakes} mistakes ×3, {details.PoopNeglect} poop left out ×2")})
        {
            var care=FarmCareColor(key);float rowH=Math.Max(14,S.ParagraphHeight(description,pushType,width-22-62-34-12,1.25f));
            FarmTagGlyph(draw,key,new(x+11+4,y+7),care);S.TextIn(draw,new(x+11+10,y),14,care,name,pushBold);
            S.Paragraph(draw,new(x+11+62+6,y),FarmMuted,description,pushType,width-22-62-34-12,1.25f);
            string v=$"+{bonus[key]:0}";S.TextIn(draw,new(x+width-11-pushBold.Width(v),y),14,S.Text1,v,pushBold);
            y+=rowH+2;
        }
        y+=9;FarmBoxFrame(draw,new(x,boxTop),new(width,y-boxTop));y+=8;
        draw.ChannelsMerge();
        ImGui.SetCursorScreenPos(new(x,y));ImGui.Dummy(new(width,0));
        // Native shortcuts kept for keyboard and pointer tests: pet, play with a partner, clean a single pile.
        ImGui.PushFont(NativeUiTheme.BodyFont,S.Body(12).Size);
        ImGui.PushStyleColor(ImGuiCol.Header,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.HeaderHovered,new ImColor8(200,170,110,20));ImGui.PushStyleColor(ImGuiCol.Text,FarmMuted);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
        bool more=ImGui.CollapsingHeader("More care actions"u8);
        ImGui.PopStyleVar();ImGui.PopStyleColor(3);
        if(more)
        {
            if(Button("farmPet","Pet "+CosmeticCollection.Item("legend",id)!.Name,ImGui.GetContentRegionAvail().X,30))FarmAct(FarmCareAction.Pet);
            foreach(string other in residents.Where(r=>r!=id))
                if(Button("farmPlay/"+other,"Play with "+CosmeticCollection.Item("legend",other)!.Name,0,30))FarmAct(FarmCareAction.Play,other);
            foreach(var pile in pet["poop"]!.AsArray())
                if(Button("farmPile/"+pile!["k"],"Clean pile "+pile["k"],0,30))FarmAct(FarmCareAction.CleanPile,pile:pile["k"]!.GetValue<int>());
        }
        ImGui.PopFont();
    }
    /// <summary>A legend .pt tile: radial-gradient(circle at 50% 30%) between light and dark mixes of --fc #8a6d1f.</summary>
    /// <summary>
    /// DT.Legends.html: a form drawn with a shop line's model gets that line's family colour (render3d.js familyColor,
    /// exported as the Codex portrait colour); legend-model forms keep --fc #8a6d1f.
    /// </summary>
    private static Rgb FarmFormColor(string form)
    {
        if(LegendEvolution.FindForm(form)?.Line is not {} line)return HudHex("8a6d1f");
        foreach(var variant in codexCatalog.Value.Variants)
            if(variant.Lines.FirstOrDefault(l=>l.Id==line) is {PortraitColor:{} color})return HudCss(color);
        return HudHex("5b6b8c");
    }
    private static void FarmPortraitTile(ImDrawListPtr draw,float2 at,float size,float rounding)
    {
        var light=new ImColor8(173,153,98);var dark=new ImColor8(62,52,27);
        draw.AddRectFilled(at,at+new float2(size,size),dark,rounding);
        var c=at+new float2(size*.5f,size*.3f);float reach=size*.68f;
        draw.PushClipRect(at+new float2(1,1),at+new float2(size-1,size-1),true);
        for(int i=0;i<14;i++){float t=i/13f;draw.AddCircleFilled(c,reach*(1-t)+1,S.Mix(light,t,dark),32);}
        draw.PopClipRect();
    }
    private static void FarmBoxFrame(ImDrawListPtr draw,float2 at,float2 extent)
    {
        draw.ChannelsSetCurrent(0);draw.AddRectFilled(at,at+extent,FarmPanel2);draw.AddRect(at,at+extent,FarmLine2);draw.ChannelsSetCurrent(1);
    }
    private static void FarmBoxHeading(ImDrawListPtr draw,float2 at,float width,string title,string? detail,bool ready=false)
    {
        S.TextIn(draw,at,14.85f,FarmMuted,title,S.Body(11,true,1/11f));
        if(detail is not null){var type=ready?S.Body(11,true):S.Body(11);S.TextIn(draw,new(at.X+width-type.Width(detail),at.Y),14.85f,ready?FarmYellow:FarmMuted,detail,type);}
    }
    private static void FarmMeterBar(ImDrawListPtr draw,float2 at,float2 extent,float fraction,ImColor8 from,ImColor8 to,bool low)
    {
        float r=extent.Y*.5f;
        draw.AddRectFilled(at,at+extent,FarmBg,r);draw.AddRect(at,at+extent,FarmLine,r);
        float w=Math.Clamp(fraction,0,1)*extent.X;if(w<1)return;
        int first=draw.VtxBuffer.Count;
        draw.AddRectFilled(at,at+new float2(w,extent.Y),new ImColor8(255,255,255,(byte)(low?(int)(255*(.72+.28*Math.Cos(ImGui.GetTime()*Math.PI*2))):255)),r);
        HorizontalGradient(draw,first,at.X,at.X+w,from,to);
    }
    private static unsafe void HorizontalGradient(ImDrawListPtr draw,int first,float left,float right,ImColor8 from,ImColor8 to)
    {
        var a=from.AsByte4();var b=to.AsByte4();
        for(int i=first;i<draw.VtxBuffer.Count;i++)
        {
            ref var vertex=ref draw.VtxBuffer.Data[i];float t=Math.Clamp((vertex.pos.X-left)/Math.Max(1,right-left),0,1);
            vertex.col=(vertex.col&0xFF000000)|((uint)Math.Round(a.Z+(b.Z-a.Z)*t)<<16)|((uint)Math.Round(a.Y+(b.Y-a.Y)*t)<<8)|(uint)Math.Round(a.X+(b.X-a.X)*t);
        }
    }
    /// <summary>js/farm-ui.js emoji (ACTIONS, BARS, card tags), baked from Segoe UI Emoji into content/farm-ui-icons.png.</summary>
    private enum FarmUiIcon { Meat, Broom, Pill, Bulb, Clap, Anger, Lifter, Ball, Bolt, Ruler, Cake }
    private GpuTexture? farmUiIconTexture;
    private void DrawFarmUiIcon(ImDrawListPtr draw,FarmUiIcon icon,float2 c,float half)
    {
        farmUiIconTexture??=new(DigiTactics.Assets.PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"content/farm-ui-icons.png"))),new(MinFilter:9729,WrapS:33071,WrapT:33071),false);
        const float count=11;int i=(int)icon;
        draw.AddImage(new ImTextureRef{_TexID=new((nint)farmUiIconTexture.Id)},c-new float2(half,half),c+new float2(half,half),new(i/count,0),new((i+1)/count,1));
    }
    private void FarmStatIcon(ImDrawListPtr draw,string key,float2 c)
    {
        switch(key)
        {
            case "hunger":DrawFarmUiIcon(draw,FarmUiIcon.Meat,c,7);break;
            case "energy":DrawFarmUiIcon(draw,FarmUiIcon.Bolt,c,7);break;
            case "happy":FarmTagGlyph(draw,"bond",c,new ImColor8(255,255,255));break;
            default:DrawFarmUiIcon(draw,FarmUiIcon.Ruler,c,7);break;
        }
    }
    private void FarmTagGlyph(ImDrawListPtr draw,string glyph,float2 c,ImColor8 color)
    {
        switch(glyph)
        {
            case "bond":
                draw.AddCircleFilled(c+new float2(-2,-1),2.4f,color,12);draw.AddCircleFilled(c+new float2(2,-1),2.4f,color,12);
                draw.AddTriangleFilled(c+new float2(-4.3f,0),c+new float2(4.3f,0),c+new float2(0,4.5f),color);break;
            case "battle":draw.AddLine(c+new float2(-4,-4),c+new float2(4,4),color,1.6f);draw.AddLine(c+new float2(4,-4),c+new float2(-4,4),color,1.6f);break;
            case "food":DrawFarmUiIcon(draw,FarmUiIcon.Meat,c,6);break;
            case "wild":S.Text(draw,c-new float2(3,6.5f),color,"?",S.Body(10.5f,true));break;
            case "weight":draw.AddLine(c+new float2(-4,-3),c+new float2(4,-3),color,1.2f);draw.AddLine(c+new float2(0,-4),c+new float2(0,4),color,1.2f);draw.AddTriangle(c+new float2(-4,-3),c+new float2(-6,1),c+new float2(-2,1),color);draw.AddTriangle(c+new float2(4,-3),c+new float2(2,1),c+new float2(6,1),color);break;
            case "age":DrawFarmUiIcon(draw,FarmUiIcon.Cake,c,6.5f);break;
        }
    }
}
