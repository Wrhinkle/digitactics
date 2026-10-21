using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // Match the captured browser glyph height, rather than its CSS font-size:
    // the native Inter rasterizer's 14px title was visibly only about 11px tall.
    private const float CodexLineTitleFont=18,CodexLineBodyFont=14,CodexLineAbilityFont=13;
    private static readonly ImColor8 CodexLineStrong=new(238,242,250);

    private void CodexLineList()
    {
        var lines=CodexData.Lines.Where(line=>CodexMatch(line.SearchText)&&(codexCost=="all"||(codexCost=="wild"?!line.Shop:line.Shop&&line.Tier.ToString()==codexCost))).ToArray();
        foreach(var group in lines.GroupBy(line=>line.Shop?line.Tier:100).OrderBy(group=>group.Key))
        {
            CodexHeading(group.Key==100?"WILD DIGIMON (PvE ONLY)":group.Key+" GOLD",18,CodexColor(group.First().Color));
            ImGui.SameLine(0,10);CodexText(group.Count().ToString(),CodexMuted,14);
            CodexGrid("Codex lines/"+group.Key,group,260,CodexLineCard);ImGui.Spacing();
        }
    }
    private void CodexLineCard(CodexLine line)
    {
        CodexRows++;ImGui.PushID(line.Id);
        if(!CodexBeginCard("Digimon card",CodexColor(line.Color),topStripe:true)){CodexEndCard();ImGui.PopID();return;}
        try
        {
            var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
            float height=CodexLineCardLayout(line,at,width,false);
            // The whole source card opens the unit, including its portraits.
            // A single transparent hit target keeps keyboard focus and smoke
            // anchors without adding a framed title button to the composition.
            var clear=new ImColor8(0,0,0,0);
            ImGui.PushStyleColor(ImGuiCol.Button,clear);ImGui.PushStyleColor(ImGuiCol.ButtonHovered,clear);ImGui.PushStyleColor(ImGuiCol.ButtonActive,clear);ImGui.PushStyleColor(ImGuiCol.Border,clear);
            bool clicked=Button("codexLine/"+line.Id,"",width,height),hovered=ImGui.IsItemHovered(),focused=ImGui.IsItemFocused();ImGui.PopStyleColor(4);
            if(clicked)codexUnit=line.Id;
            if(hovered)ImGui.GetWindowDrawList().AddRectFilled(at,at+new float2(width,height),new ImColor8(90,110,145,15));
            CodexLineCardLayout(line,at,width,true);
            if(focused)ImGui.GetWindowDrawList().AddRect(at,at+new float2(width,height),CodexGold,thickness:2);
        }
        finally{CodexEndCard();ImGui.PopID();}
    }
    private float CodexLineCardLayout(CodexLine line,float2 at,float width,bool draw)
    {
        // codexstyle.js .cx-line / .cx-lh / .cx-lf: one heading row,
        // 54px forms, ellipsized labels, compact traits and bold stat values.
        string cost=line.Shop?line.Tier+"g":"wild";
        float costWidth=CodexLineMeasure(cost,CodexLineBodyFont,true).X+14,attributeWidth=CodexLineMeasure(line.Attribute,CodexLineBodyFont,true).X+14;
        float pillHeight=CodexLineMeasure(cost,CodexLineBodyFont).Y*1.35f+2,headerHeight=Math.Max(CodexLineMeasure(line.Forms[0].Name,CodexLineTitleFont).Y*1.35f,pillHeight);
        float titleWidth=Math.Max(1,width-costWidth-attributeWidth-12),y=headerHeight+6;
        if(draw)
        {
            CodexLineLabel(at+new float2(0,(headerHeight-CodexLineMeasure(line.Forms[0].Name,CodexLineTitleFont).Y)*.5f),titleWidth,line.Forms[0].Name,CodexLineTitleFont,CodexLineStrong,bold:true);
            CodexLinePill(at+new float2(width-attributeWidth-costWidth-6,(headerHeight-pillHeight)*.5f),new(costWidth,pillHeight),cost,CodexColor(line.Color));
            CodexLinePill(at+new float2(width-attributeWidth,(headerHeight-pillHeight)*.5f),new(attributeWidth,pillHeight),line.Attribute,CodexColor(line.AttributeColor));
        }
        var forms=(line.Forms.Select(form=>form.Name).Distinct().Count()==1?line.Forms.Take(1):line.Forms).ToArray();
        float cell=(width-8)/3,nameHeight=CodexLineMeasure("Ag",CodexLineBodyFont).Y*1.15f,abilityHeight=CodexLineMeasure("Ag",CodexLineAbilityFont).Y*1.15f;
        for(int i=0;i<forms.Length;i++)
        {
            var form=forms[i];var start=at+new float2(i*(cell+4),y);
            if(draw)
            {
                CodexLinePortrait(line,form.Stage,start+new float2((cell-54)*.5f,0),54);
                CodexLineLabel(start+new float2(0,57),cell,form.Name,CodexLineBodyFont,CodexLineStrong,bold:true,center:true);
                CodexLineLabel(start+new float2(0,57+nameHeight),cell,form.AbilityName,CodexLineAbilityFont,new ImColor8(201,166,255),center:true);
            }
        }
        y+=57+nameHeight+abilityHeight+6;
        float x=0,row=0;
        foreach(string id in line.Traits)
        {
            var trait=CodexData.Traits.Single(candidate=>candidate.Id==id);var text=CodexLineMeasure(trait.Name,CodexLineBodyFont,true);
            float chipWidth=Math.Min(width,text.X+28),chipHeight=Math.Max(15,text.Y)+2;
            if(x>0&&x+chipWidth>width){x=0;y+=row+4;row=0;}
            if(draw)CodexLineTrait(trait,at+new float2(x,y),new(chipWidth,chipHeight));
            x+=chipWidth+4;row=Math.Max(row,chipHeight);
        }
        y+=row+6;
        y+=CodexLineParagraph(line.Forms[0].AbilityText,at+new float2(0,y),width,draw)+12;
        x=0;row=0;
        foreach(var stat in new[]{("HP ",string.Join('/',line.Forms.Select(form=>form.Stats.MaxHp))),("AD ",string.Join('/',line.Forms.Select(form=>form.Stats.Ad))),("AS ",line.Forms[0].Stats.As.ToString()),("Range ",line.Forms[0].Stats.Range.ToString())})
        {
            float labelWidth=CodexLineMeasure(stat.Item1,CodexLineBodyFont).X,valueWidth=CodexLineMeasure(stat.Item2,CodexLineBodyFont,true).X,statHeight=CodexLineMeasure(stat.Item1,CodexLineBodyFont).Y*1.35f;
            if(x>0&&x+labelWidth+valueWidth>width){x=0;y+=row+3;row=0;}
            if(draw)
            {
                CodexLineLabel(at+new float2(x,y),labelWidth,stat.Item1,CodexLineBodyFont,CodexMuted);
                CodexLineLabel(at+new float2(x+labelWidth,y),Math.Min(valueWidth,width-x-labelWidth),stat.Item2,CodexLineBodyFont,CodexLineStrong,bold:true);
            }
            x+=labelWidth+valueWidth+10;row=Math.Max(row,statHeight);
        }
        return y+row+6;
    }
    private static float2 CodexLineMeasure(string text,float font,bool bold=false)
    {
        ImGui.PushFont(bold?NativeUiTheme.BodyBoldFont:NativeUiTheme.BodyFont,font);var size=ImGui.CalcTextSize(text);ImGui.PopFont();return size;
    }
    /// <summary><paramref name="chip"/>: a trait card's .cx-u chip (img at .88 opacity unless owned, Wild chips framed by a dashed #6a7488 border).</summary>
    private void CodexLinePortrait(CodexLine line,int stage,float2 at,float extent,bool chip=false,bool owned=false)
    {
        if(!ImGui.IsRectVisible(at,at+new float2(extent,extent)))return;
        var draw=ImGui.GetWindowDrawList();var family=CodexColor(line.PortraitColor??"#888888").AsByte4();
        // style.css .pt: circle at 50% 30%, with original sRGB color-mix stops
        // (70% family + white) and (40% family + #0b0e15) at 80% of farthest corner.
        var light=new float3(family.X*.7f+76.5f,family.Y*.7f+76.5f,family.Z*.7f+76.5f);
        var dark=new float3(family.X*.4f+6.6f,family.Y*.4f+8.4f,family.Z*.4f+12.6f);
        var center=at+new float2(extent*.5f,extent*.3f);float stop=extent*MathF.Sqrt(.25f+.49f)*.8f,radius=Math.Min(8,extent*.2f);
        var uv=ImGui.GetFontTexUvWhitePixel();
        float2 Edge(int index)
        {
            int corner=index/8;float angle=(180+corner*90+(index%8)*90f/7)*MathF.PI/180;
            var pivot=at+new float2(corner is 0 or 3?radius:extent-radius,corner<2?radius:extent-radius);
            return pivot+new float2(MathF.Cos(angle),MathF.Sin(angle))*radius;
        }
        ImColor8 Tint(float fraction)
        {
            var rgb=light+(dark-light)*Math.Clamp(fraction,0,1);
            return new((byte)MathF.Round(rgb.X),(byte)MathF.Round(rgb.Y),(byte)MathF.Round(rgb.Z));
        }
        draw.PrimReserve(32*9,32*9);
        for(int i=0;i<32;i++)
        {
            var edgeA=Edge(i);var edgeB=Edge((i+1)%32);var deltaA=edgeA-center;var deltaB=edgeB-center;
            float lengthA=MathF.Sqrt(deltaA.X*deltaA.X+deltaA.Y*deltaA.Y),lengthB=MathF.Sqrt(deltaB.X*deltaB.X+deltaB.Y*deltaB.Y);
            var innerA=center+deltaA*Math.Min(1,stop/lengthA);var innerB=center+deltaB*Math.Min(1,stop/lengthB);
            var colorA=Tint(lengthA/stop);var colorB=Tint(lengthB/stop);
            draw.PrimVtx(center,uv,Tint(0));draw.PrimVtx(innerA,uv,colorA);draw.PrimVtx(innerB,uv,colorB);
            draw.PrimVtx(innerA,uv,colorA);draw.PrimVtx(edgeA,uv,colorA);draw.PrimVtx(edgeB,uv,colorB);
            draw.PrimVtx(innerA,uv,colorA);draw.PrimVtx(edgeB,uv,colorB);draw.PrimVtx(innerB,uv,colorB);
        }
        int before=portraits.DrawCalls;portraits.Draw(CodexData.Id,line.Id,stage,at,extent,chip&&!owned?(byte)224:(byte)255);
        if(portraits.DrawCalls==before)
        {
            string name=line.Forms[Math.Min(stage,line.Forms.Length-1)].Name;
            CodexLineLabel(at+new float2(4,18),extent-8,name[..Math.Min(2,name.Length)],CodexLineBodyFont,CodexLineStrong,bold:true,center:true);
        }
        if(chip&&!line.Shop)CodexDashedFrame(draw,at,extent,radius,new ImColor8(106,116,136));
        else draw.AddRect(at,at+new float2(extent,extent),CodexColor(line.Color),radius,thickness:2);
        CodexPortraits++;
    }
    /// <summary>border: 2px dashed with rounded corners, as Chromium draws it (dashes about twice the width, square caps).</summary>
    private static void CodexDashedFrame(ImDrawListPtr draw,float2 at,float extent,float radius,ImColor8 color)
    {
        float inset=1,lo=inset+radius,hi=extent-inset-radius;
        draw.PathClear();
        draw.PathArcTo(at+new float2(lo,lo),radius,MathF.PI,MathF.PI*1.5f,6);draw.PathStroke(color,ImDrawFlags.None,2);
        draw.PathArcTo(at+new float2(hi,lo),radius,MathF.PI*1.5f,MathF.Tau,6);draw.PathStroke(color,ImDrawFlags.None,2);
        draw.PathArcTo(at+new float2(hi,hi),radius,0,MathF.PI*.5f,6);draw.PathStroke(color,ImDrawFlags.None,2);
        draw.PathArcTo(at+new float2(lo,hi),radius,MathF.PI*.5f,MathF.PI,6);draw.PathStroke(color,ImDrawFlags.None,2);
        float span=hi-lo;int dashes=Math.Max(1,(int)MathF.Round(span/8));float step=span/dashes;
        for(int i=0;i<dashes;i++)
        {
            float a=lo+i*step+step*.25f,b=a+step*.5f;
            draw.AddLine(at+new float2(a,inset),at+new float2(b,inset),color,2);draw.AddLine(at+new float2(a,extent-inset),at+new float2(b,extent-inset),color,2);
            draw.AddLine(at+new float2(inset,a),at+new float2(inset,b),color,2);draw.AddLine(at+new float2(extent-inset,a),at+new float2(extent-inset,b),color,2);
        }
    }
    private static float CodexLineParagraph(string text,float2 at,float width,bool draw)
    {
        ImGui.PushFont(NativeUiTheme.BodyFont,CodexLineBodyFont);var lines=new List<string>();string line="";
        foreach(string word in text.Split(' ',StringSplitOptions.RemoveEmptyEntries))
        {
            string next=line.Length==0?word:line+" "+word;
            if(line.Length>0&&ImGui.CalcTextSize(next).X>width){lines.Add(line);line=word;}else line=next;
        }
        if(line.Length>0)lines.Add(line);
        float leading=ImGui.GetFontSize()*1.3f;
        if(draw)for(int i=0;i<lines.Count;i++)ImGui.GetWindowDrawList().AddText(at+new float2(0,i*leading),CodexMuted,lines[i]);
        ImGui.PopFont();return lines.Count*leading;
    }
    private static void CodexLineLabel(float2 at,float width,string text,float font,ImColor8 color,bool bold=false,bool center=false)
    {
        ImGui.PushFont(bold?NativeUiTheme.BodyBoldFont:NativeUiTheme.BodyFont,font);string label=text;
        // Subtracting pill padding can introduce a tiny float error; exact-fit
        // measured labels should not acquire an ellipsis on larger UI scales.
        if(ImGui.CalcTextSize(label).X>width+.01f)
        {
            while(label.Length>0&&ImGui.CalcTextSize(label+"...").X>width+.01f)label=label[..^1];
            label+="...";
        }
        var size=ImGui.CalcTextSize(label);var position=at+new float2(center?Math.Max(0,(width-size.X)*.5f):0,0);var painter=ImGui.GetWindowDrawList();
        painter.PushClipRect(at,at+new float2(Math.Max(1,width),size.Y+1),true);painter.AddText(position,color,label);
        painter.PopClipRect();
        if(label!=text&&ImGui.IsMouseHoveringRect(at,at+new float2(width,size.Y)))ImGui.SetTooltip(text);
        ImGui.PopFont();
    }
    private static void CodexLinePill(float2 at,float2 extent,string text,ImColor8 color)
    {
        var painter=ImGui.GetWindowDrawList();painter.AddRectFilled(at,at+extent,new ImColor8(14,28,44),9);var rgb=color.AsByte4();
        painter.AddRect(at,at+extent,new ImColor8(rgb.X,rgb.Y,rgb.Z,150),9);
        CodexLineLabel(at+new float2(7,(extent.Y-CodexLineMeasure(text,CodexLineBodyFont).Y)*.5f),extent.X-14,text,CodexLineBodyFont,color,bold:true,center:true);
    }
    private static void CodexLineTrait(CodexTrait trait,float2 at,float2 extent)
    {
        var painter=ImGui.GetWindowDrawList();var color=CodexColor(trait.Color);var rgb=color.AsByte4();
        painter.AddRectFilled(at,at+extent,new ImColor8(14,28,44),9);painter.AddRectFilled(at,at+extent,new ImColor8(rgb.X,rgb.Y,rgb.Z,30),9);painter.AddRect(at,at+extent,new ImColor8(rgb.X,rgb.Y,rgb.Z,115),9);
        var center=at+new float2(10,extent.Y*.5f);
        for(int i=0;i<6;i++)
        {
            float a=(i*60-90)*MathF.PI/180,b=((i+1)*60-90)*MathF.PI/180;
            painter.AddTriangleFilled(center,center+new float2(MathF.Cos(a)*7,MathF.Sin(a)*7.5f),center+new float2(MathF.Cos(b)*7,MathF.Sin(b)*7.5f),color);
        }
        string icon=trait.Icon;
        if(icon.Length==0||icon.Contains('/')||icon.Contains('.')){icon=new string(trait.Name.Where(char.IsUpper).Take(2).ToArray());if(icon.Length==0)icon=trait.Name[..Math.Min(2,trait.Name.Length)];}
        CodexLineLabel(center-new float2(6,CodexLineMeasure(icon,7).Y*.5f),12,icon,7,new ImColor8(16,19,26),bold:true,center:true);
        CodexLineLabel(at+new float2(21,(extent.Y-CodexLineMeasure(trait.Name,CodexLineBodyFont).Y)*.5f),extent.X-28,trait.Name,CodexLineBodyFont,color,bold:true);
    }
}
