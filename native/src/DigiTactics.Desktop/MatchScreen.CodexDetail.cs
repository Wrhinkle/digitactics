using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool codexMeasureDetailBounds;
    internal (float2 Min,float2 Max) CodexDetailViewport {get;private set;}
    internal (float2 Min,float2 Max) CodexDetailTextBounds {get;private set;}
    internal (float2 Min,float2 Max) CodexDetailFooterBounds {get;private set;}
    internal float CodexDetailScrollY {get;private set;}
    internal float CodexDetailScrollMaxY {get;private set;}
    internal float CodexDetailScrollMaxX {get;private set;}

    private void CodexUnitDetail(CodexLine line,float? contentWidth=null)
    {
        CodexRows++;
        var at=ImGui.GetCursorScreenPos();float width=contentWidth??ImGui.GetContentRegionAvail().X;
        float height=CodexDetailLayout(line,at,width,false,codexFull);
        // Place the inline links, then reserve the measured content as one item.
        // Link hit targets must not change the next row's cursor position.
        CodexDetailLayout(line,at,width,true,codexFull);
        ImGui.SetCursorScreenPos(at);ImGui.Dummy(new(width,height));
    }

    private void CodexUnitModal()
    {
        var line=CodexData.Lines.Single(item=>item.Id==codexUnit);
        float boundary=ClientHomeVisible?ClientPageTop:0,width=Math.Min(460,size.X-32);
        float maxHeight=size.Y-boundary-32,contentWidth=width-32;
        float desiredHeight=CodexDetailLayout(line,default,contentWidth,false,true)+28;
        if(desiredHeight>maxHeight)
        {
            contentWidth-=8; // The shared Codex scrollbar is eight pixels wide.
            desiredHeight=CodexDetailLayout(line,default,contentWidth,false,true)+28;
        }
        float height=Math.Min(desiredHeight,maxHeight),left=(size.X-width)/2,top=boundary+(size.Y-boundary-height)/2;
        bool changed=codexModalLine!=codexUnit;
        if(changed){codexModalLine=codexUnit;codexModalFrame=ImGui.GetFrameCount();}
        ImGui.SetNextWindowFocus();
        CodexBeginWindow("Codex Digimon detail",new(left,top),new(width,height),true,padding:new float2(16,14));
        if(changed)ImGui.SetScrollY(0);
        var start=ImGui.GetCursorScreenPos();
        // Keep the close hitbox inside padding, including the scrollbar gutter.
        // An item in that gutter creates a hidden horizontal scroll range.
        ImGui.SetCursorScreenPos(new(left+16+contentWidth-28,top+10));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(0,0));
        if(Button("codexCloseUnit","",28,24))codexUnit=null;
        var close=ImGui.GetItemRectMin();var draw=ImGui.GetWindowDrawList();
        draw.AddLine(close+new float2(9,7),close+new float2(19,17),CodexGold,1.5f);
        draw.AddLine(close+new float2(19,7),close+new float2(9,17),CodexGold,1.5f);
        ImGui.PopStyleVar();ImGui.SetCursorScreenPos(start);
        CodexDetailViewport=(draw.GetClipRectMin(),draw.GetClipRectMax());
        CodexDetailTextBounds=(new(float.MaxValue,float.MaxValue),new(float.MinValue,float.MinValue));
        codexMeasureDetailBounds=true;
        CodexUnitDetail(line,contentWidth);
        codexMeasureDetailBounds=false;
        CodexDetailScrollY=ImGui.GetScrollY();CodexDetailScrollMaxY=ImGui.GetScrollMaxY();CodexDetailScrollMaxX=ImGui.GetScrollMaxX();
        CodexEndWindow();
        var pointer=ImGui.GetIO().MousePos;
        if(ImGui.GetFrameCount()>codexModalFrame&&ImGui.IsMouseClicked(ImGuiMouseButton.Left)&&
            (pointer.X<left||pointer.X>left+width||pointer.Y<top||pointer.Y>top+height))codexUnit=null;
    }

    // The original unit sheet is a compact header, one common-stat strip and
    // three portrait/text rows. Measure and paint the same layout so a short
    // unit cannot inherit an empty footer, and a long ability remains scrollable.
    private float CodexDetailLayout(CodexLine line,float2 at,float width,bool draw,bool modal)
    {
        float y=0;
        y+=CodexDetailParagraph(at,width-(modal?34:0),16,draw,(line.Forms[0].Name,CodexInk),(" line",CodexMuted))+4;
        var pills=new List<(string Text,ImColor8 Color)>
        {
            (line.Shop?$"{line.Tier} gold":$"Wild (tier {line.Tier})",CodexColor(line.Color)),
            (line.Attribute,CodexColor(line.AttributeColor))
        };
        if(Session is not null&&line.Shop)pills.Add(($"{Session.State.Pool.Remaining(line.Id)}/{Rules.PoolSizes[line.Tier]} left in the pool",CodexMuted));
        y+=CodexDetailPills(at+new float2(0,y),width,pills,draw)+4;
        y+=CodexDetailTraits(line,at+new float2(0,y),width,draw)+5;
        y+=CodexDetailParagraph(at+new float2(0,y),width,13,draw,(line.Attribute+": "+line.AttributeText,CodexMuted))+7;
        if(codexOwned.TryGetValue(line.Id,out var owned))
        {
            string summary="You have: "+string.Join(", ",owned.Select(entry=>line.Forms[entry.Unit.Stage].Name+(entry.Unit.Stage>0?$" ({entry.Unit.Stage+1}-star)":"")+(entry.Spot.IsBoard?" (board)":" (bench)")));
            y+=CodexDetailParagraph(at+new float2(0,y),width,13,draw,(summary,new ImColor8(89,211,151)))+7;
        }
        CodexDetailRule(at,width,ref y,draw);
        var stats=line.Forms[0].Stats;
        y+=CodexDetailParagraph(at+new float2(0,y),width,13,draw,
            ("AS ",CodexMuted),($"{stats.As:0.##}",CodexInk),("   Range ",CodexMuted),($"{stats.Range:0.#}",CodexInk),
            ("   Armor ",CodexMuted),($"{stats.Armor:0.#}",CodexInk),("   MR ",CodexMuted),($"{stats.Mr:0.#}",CodexInk),
            ("   Mana ",CodexMuted),($"{stats.Mana:0}/{stats.MaxMana:0}",CodexInk))+7;
        foreach(var form in line.Forms)
        {
            CodexDetailRule(at,width,ref y,draw);
            float portrait=modal?64:48,bodyWidth=Math.Max(40,width-portrait-8);
            var row=at+new float2(0,y);var body=row+new float2(portrait+8,0);
            if(draw)CodexPortrait(line,form.Stage,row,portrait);
            float bodyHeight=CodexDetailFormName(form,body,bodyWidth,draw)+3;
            bodyHeight+=CodexDetailParagraph(body+new float2(0,bodyHeight),bodyWidth,13,draw,
                ("HP ",CodexMuted),($"{form.Stats.MaxHp}",CodexInk),(" · AD ",CodexMuted),($"{form.Stats.Ad}",CodexInk))+3;
            bodyHeight+=CodexDetailParagraph(body+new float2(0,bodyHeight),bodyWidth,14,draw,
                (form.AbilityName+" ",CodexAbility),(form.AbilityText,CodexBody));
            y+=Math.Max(portrait,bodyHeight)+7;
        }
        CodexDetailRule(at,width,ref y,draw);
        string footer=line.Shop?"Three of one form digivolve into the next. Sells for "+string.Join(" / ",line.Forms.Select(form=>form.SellValue))+" gold.":"Wild Digimon appear in PvE rounds only and never count for traits.";
        var footerAt=at+new float2(0,y);
        float footerHeight=CodexDetailParagraph(footerAt,width,13,draw,(footer,CodexMuted));
        if(draw&&codexMeasureDetailBounds)CodexDetailFooterBounds=(footerAt,footerAt+new float2(width,footerHeight));
        return y+footerHeight;
    }

    private static void CodexDetailRule(float2 at,float width,ref float y,bool draw)
    {
        if(draw)ImGui.GetWindowDrawList().AddLine(at+new float2(0,y),at+new float2(width,y),CodexStroke);
        y+=8;
    }

    private float CodexDetailPills(float2 at,float width,IEnumerable<(string Text,ImColor8 Color)> pills,bool draw)
    {
        float x=0,y=0,row=0;
        foreach(var pill in pills)
        {
            float cellWidth=Math.Min(width,MathF.Ceiling(CodexDetailTextWidth(pill.Text,12))+14);
            float height=CodexDetailParagraph(default,cellWidth-14,12,false,(pill.Text,pill.Color))+6;
            if(x>0&&x+cellWidth>width){y+=row+4;x=0;row=0;}
            if(draw)
            {
                var start=at+new float2(x,y);var painter=ImGui.GetWindowDrawList();
                painter.AddRectFilled(start,start+new float2(cellWidth,height),new ImColor8(14,28,44),9);
                painter.AddRect(start,start+new float2(cellWidth,height),pill.Color,9);
                CodexDetailParagraph(start+new float2(7,3),cellWidth-14,12,true,(pill.Text,pill.Color));
            }
            x+=cellWidth+4;row=Math.Max(row,height);
        }
        return y+row;
    }

    private float CodexDetailTraits(CodexLine line,float2 at,float width,bool draw)
    {
        float x=0,y=0,row=0;
        foreach(string id in line.Traits)
        {
            var trait=CodexData.Traits.Single(candidate=>candidate.Id==id);var color=CodexColor(trait.Color);
            float cellWidth=Math.Min(width,MathF.Ceiling(CodexDetailTextWidth(trait.Name,13))+28);
            float height=Math.Max(22,CodexDetailParagraph(default,cellWidth-28,13,false,(trait.Name,color))+6);
            if(x>0&&x+cellWidth>width){y+=row+4;x=0;row=0;}
            if(draw)
            {
                var start=at+new float2(x,y);ImGui.SetCursorScreenPos(start);
                ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(0,0,0,0));
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
                bool selected=Button("codexTrait/"+id,"",cellWidth,height),hovered=ImGui.IsItemHovered();
                ImGui.PopStyleVar();ImGui.PopStyleColor(3);
                var painter=ImGui.GetWindowDrawList();painter.AddRectFilled(start,start+new float2(cellWidth,height),hovered?new ImColor8(28,44,61):new ImColor8(14,28,44),10);painter.AddRect(start,start+new float2(cellWidth,height),color,10);
                ImGui.SetCursorScreenPos(start+new float2(4,(height-15)/2));ImGui.PushFont(NativeUiTheme.BodyFont,7);CodexBadge(trait,13);ImGui.PopFont();
                CodexDetailParagraph(start+new float2(22,3),cellWidth-28,13,true,(trait.Name,color));
                if(selected)
                {
                    codexUnit=null;codexFocusTrait=id;codexSideRules=false;codexTab="traits";codexKind="all";codexSearch.SetValue(""u8);
                    if(codexCounts.GetValueOrDefault(id)==0)codexPool=true;
                }
            }
            x+=cellWidth+4;row=Math.Max(row,height);
        }
        return y+row;
    }

    private float CodexDetailFormName(CodexForm form,float2 at,float width,bool draw)
    {
        float nameWidth=CodexDetailTextWidth(form.Name,14),stageWidth=CodexDetailTextWidth(form.StageName,12);
        float nameHeight=CodexDetailParagraph(at,width,14,draw,(form.Name,CodexInk));
        bool sameLine=nameWidth+(form.Stage+1)*9+stageWidth+12<=width;
        var stars=at+new float2(sameLine?nameWidth+5:0,sameLine?3:nameHeight+3);
        if(draw)
        {
            var painter=ImGui.GetWindowDrawList();
            for(int star=0;star<=form.Stage;star++)
            {
                var center=stars+new float2(star*9+4,4);
                for(int point=0;point<10;point++)
                {
                    float a=(point*36-90)*MathF.PI/180,b=((point+1)*36-90)*MathF.PI/180,r=point%2==0?4:1.8f,next=point%2==0?1.8f:4;
                    painter.AddTriangleFilled(center,center+new float2(MathF.Cos(a),MathF.Sin(a))*r,center+new float2(MathF.Cos(b),MathF.Sin(b))*next,CodexGold);
                }
            }
        }
        float stageX=(sameLine?nameWidth+5:0)+(form.Stage+1)*9+4;
        float stageHeight=CodexDetailParagraph(at+new float2(stageX,sameLine?1:nameHeight+1),Math.Max(1,width-stageX),12,draw,(form.StageName,CodexMuted));
        return sameLine?Math.Max(nameHeight,stageHeight+1):nameHeight+stageHeight+1;
    }

    // CalcTextSize rounds each call to whole pixels. Raw font advances keep a
    // label's measured width equal to the sum of its words and spaces, so tight
    // pills cannot gain an accidental extra line from repeated rounding.
    private static float CodexDetailTextWidth(string value,float font)=>NativeUiTheme.BodyFont.CalcTextSizeA(font,float.MaxValue,0,value).X;

    private void CodexRecordDetailText(float2 at,float width,float height)
    {
        if(!codexMeasureDetailBounds)return;
        var bounds=CodexDetailTextBounds;
        CodexDetailTextBounds=(new(Math.Min(bounds.Min.X,at.X),Math.Min(bounds.Min.Y,at.Y)),
            new(Math.Max(bounds.Max.X,at.X+width),Math.Max(bounds.Max.Y,at.Y+height)));
    }

    private float CodexDetailParagraph(float2 at,float width,float font,bool draw,params (string Text,ImColor8 Color)[] runs)
        =>CodexDetailParagraph(at,width,font,0,draw,runs);
    /// <summary>Wrapped runs; <paramref name="lineHeight"/> (0 = the font size) sets a CSS line box with the text centred in it.</summary>
    private float CodexDetailParagraph(float2 at,float width,float font,float lineHeight,bool draw,params (string Text,ImColor8 Color)[] runs)
    {
        ImGui.PushFont(NativeUiTheme.BodyFont,font);float line=lineHeight>0?lineHeight:ImGui.GetTextLineHeight(),x=0,y=0;
        if(lineHeight>0)at.Y+=MathF.Round((lineHeight-ImGui.GetTextLineHeight())*.5f);
        var painter=draw?ImGui.GetWindowDrawList():default;
        foreach(var run in runs)
            foreach(System.Text.RegularExpressions.Match token in System.Text.RegularExpressions.Regex.Matches(run.Text,@"\S+|\s+"))
            {
                string word=token.Value;
                if(char.IsWhiteSpace(word[0]))
                {
                    if(word.Contains('\n')){x=0;y+=line;}else if(x>0)x+=CodexDetailTextWidth(word,font);
                    continue;
                }
                float wordWidth=CodexDetailTextWidth(word,font);
                if(x>0&&x+wordWidth>width){x=0;y+=line;}
                if(wordWidth<=width)
                {
                    if(draw)
                    {
                        var position=at+new float2(x,y);painter.AddText(ImGui.GetFont(),font,position,run.Color,word);
                        CodexRecordDetailText(position,wordWidth,line);
                    }
                    x+=wordWidth;continue;
                }
                // Do not clip an unusually long form/ability token on a narrow side panel.
                foreach(var rune in word.EnumerateRunes())
                {
                    string glyph=rune.ToString();float advance=CodexDetailTextWidth(glyph,font);
                    if(x>0&&x+advance>width){x=0;y+=line;}
                    if(draw)
                    {
                        var position=at+new float2(x,y);painter.AddText(ImGui.GetFont(),font,position,run.Color,glyph);
                        CodexRecordDetailText(position,advance,line);
                    }
                    x+=advance;
                }
            }
        ImGui.PopFont();return y+line;
    }
}
