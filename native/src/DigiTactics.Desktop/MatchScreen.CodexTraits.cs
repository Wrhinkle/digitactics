using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private void CodexTraitList(bool live)
    {
        IEnumerable<CodexTrait> rows=CodexData.Traits;
        if(live)
        {
            var poolTraits=CodexData.Lines.Where(line=>line.Shop).SelectMany(line=>line.Traits).ToHashSet();
            rows=rows.Where(trait=>codexCounts.GetValueOrDefault(trait.Id)>0||codexPool&&poolTraits.Contains(trait.Id))
                .OrderByDescending(trait=>trait.Breakpoints.Count(bp=>bp.Count<=codexCounts.GetValueOrDefault(trait.Id)))
                .ThenByDescending(trait=>codexCounts.GetValueOrDefault(trait.Id)).ThenBy(trait=>CodexKindOrder(trait.Kind)).ThenBy(trait=>trait.Name,StringComparer.OrdinalIgnoreCase);
            if(!rows.Any())CodexText("No traits on your board yet. Place Digimon to activate them, or switch to All in game.",HomeMute);
            foreach(var trait in rows)CodexTraitCard(trait,true);
        }
        else CodexGrid("Codex trait grid",rows.Where(trait=>(codexKind=="all"||trait.Kind==codexKind)&&CodexMatch(trait.SearchText))
            .OrderBy(trait=>CodexKindOrder(trait.Kind)).ThenBy(trait=>trait.Name,StringComparer.OrdinalIgnoreCase),290,trait=>CodexTraitCard(trait,false));
    }
    private void CodexTraitCard(CodexTrait trait,bool live)
    {
        CodexRows++;ImGui.PushID(trait.Id);var color=CodexColor(trait.Color);
        if(codexFocusTrait==trait.Id){ImGui.SetScrollHereY(.05f);codexFocusTrait=null;}
        if(!CodexBeginCard("Trait card",color,compact:live,trait:true)){CodexEndCard();ImGui.PopID();return;}
        int count=live?codexCounts.GetValueOrDefault(trait.Id):0,next=trait.Breakpoints.FirstOrDefault(bp=>bp.Count>count)?.Count??0;
        int activeThreshold=live?trait.Breakpoints.LastOrDefault(bp=>bp.Count<=count)?.Count??0:0;
        var origin=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X,y=30;
        var draw=ImGui.GetWindowDrawList();var rgb=color.AsByte4();
        // The source card uses a 30px header, compact breakpoint rows and 30px
        // unit chips. Position those groups explicitly; default ImGui row gaps
        // otherwise accumulate into a much taller card and a second count row.
        if(activeThreshold>0)
        {
            var panel=ImGui.GetWindowPos();var end=panel+ImGui.GetWindowSize();var tint=new ImColor8(rgb.X,rgb.Y,rgb.Z,24);var clear=new ImColor8(rgb.X,rgb.Y,rgb.Z,0);
            draw.AddRectFilledMultiColor(panel+new float2(4,1),end-new float2(1,1),tint,clear,clear,tint);
        }
        ImGui.PushFont(NativeUiTheme.BodyBoldFont,14);CodexBadge(trait,28);ImGui.PopFont();
        string numerator=count.ToString(),denominator=next>0?"/"+next:"";
        float countWidth=live?CodexLineMeasure(numerator,19,true).X+CodexLineMeasure(denominator,14,true).X:0;
        float titleWidth=Math.Max(1,width-35-(live?countWidth+8:0));
        CodexLineLabel(origin+new float2(35,0),titleWidth,trait.Name,17,color,bold:true);
        CodexLineLabel(origin+new float2(35,17),titleWidth,trait.Kind.ToUpperInvariant(),13,CodexMuted,bold:true);
        if(live)
        {
            var at=origin+new float2(width-countWidth,5);float numeratorWidth=CodexLineMeasure(numerator,19,true).X;
            CodexLineLabel(at,numeratorWidth,numerator,19,activeThreshold>0?HomeGold:CodexMuted,bold:true);
            if(denominator.Length>0)CodexLineLabel(at+new float2(numeratorWidth,5),countWidth-numeratorWidth,denominator,14,CodexMuted,bold:true);
            if(ImGui.IsMouseHoveringRect(at,at+new float2(countWidth,20)))ImGui.SetTooltip("Different Digimon on your board with this trait"u8);
        }
        if(trait.Description.Length>0)
        {
            y+=5;y+=CodexDetailParagraph(origin+new float2(0,y),width,15,true,(trait.Description,CodexBody));
        }
        foreach(string note in trait.Notes)
        {
            y+=3;var at=origin+new float2(0,y);float height=CodexDetailParagraph(at+new float2(8,0),width-8,14,true,(note,CodexMuted));
            draw.AddRectFilled(at,at+new float2(2,height),CodexStroke);y+=height;
        }
        y+=6;
        foreach(var point in trait.Breakpoints)
        {
            bool active=live&&count>=point.Count;var at=origin+new float2(0,y);
            float height=Math.Max(15,CodexDetailParagraph(default,width-30,15,false,(point.Text,CodexMuted)))+2;
            if(active&&point.Count==activeThreshold)draw.AddRectFilled(at,at+new float2(width,height),new ImColor8(rgb.X,rgb.Y,rgb.Z,36),4);
            draw.AddRectFilled(at+new float2(0,1),at+new float2(20,16),active?color:new ImColor8(28,39,62),4);
            CodexLineLabel(at+new float2(0,1.5f),20,point.Count.ToString(),14,active?CodexNavy:CodexMuted,bold:true,center:true);
            CodexDetailParagraph(at+new float2(26,1),width-30,15,true,(point.Text,active?CodexInk:CodexMuted));
            y+=height+2;
        }
        if(trait.Breakpoints.Length>0)y-=2;
        var lines=trait.LineIds.Select(id=>CodexData.Lines.Single(line=>line.Id==id)).Where(line=>!live||line.Shop)
            .OrderBy(line=>!line.Shop).ThenBy(line=>line.Tier).ThenBy(line=>line.Forms[0].Name,StringComparer.OrdinalIgnoreCase).ToArray();
        int columns=Math.Max(1,(int)((width+4)/34));
        foreach(var group in lines.GroupBy(line=>line.Shop))
        {
            y+=6;
            if(!group.Key){CodexLineLabel(origin+new float2(0,y),width,"WILD",13,CodexMuted);y+=19;}
            int column=0;var clear=new ImColor8(0,0,0,0);
            ImGui.PushStyleColor(ImGuiCol.Button,clear);ImGui.PushStyleColor(ImGuiCol.ButtonHovered,clear);ImGui.PushStyleColor(ImGuiCol.ButtonActive,clear);ImGui.PushStyleColor(ImGuiCol.Border,clear);
            foreach(var line in group)
            {
                if(column==columns){y+=34;column=0;}
                var owned=live?codexOwned.GetValueOrDefault(line.Id):null;int stage=owned?.Max(entry=>entry.Unit.Stage)??0;
                var at=origin+new float2(column++*34,y);ImGui.SetCursorScreenPos(at);
                if(Button("codexLine/"+trait.Id+"/"+line.Id,"##codexLine/"+line.Id,30,30)){codexUnit=line.Id;codexResetScroll=true;}
                bool hovered=ImGui.IsItemHovered(),focused=ImGui.IsItemFocused();CodexPortrait(line,stage,at,30);
                if(hovered||focused)draw.AddRect(at,at+new float2(30,30),CodexInk,5);
                if(owned is not null)
                {
                    draw.AddCircleFilled(at+new float2(28,2),5,new ImColor8(14,18,27));
                    draw.AddCircleFilled(at+new float2(28,2),3,owned.Any(entry=>entry.Spot.IsBoard)?new ImColor8(71,206,142):new ImColor8(81,161,238));
                }
                if(stage>0)
                    for(int star=0;star<=stage;star++)
                    {
                        var center=at+new float2(15-(stage+1)*4+star*8+4,27);
                        for(int point=0;point<10;point++)
                        {
                            float a=(point*36-90)*MathF.PI/180,b=((point+1)*36-90)*MathF.PI/180,r=point%2==0?3.5f:1.5f,rNext=point%2==0?1.5f:3.5f;
                            draw.AddTriangleFilled(center,center+new float2(MathF.Cos(a),MathF.Sin(a))*r,center+new float2(MathF.Cos(b),MathF.Sin(b))*rNext,HomeGold);
                        }
                    }
                if(hovered)
                {
                    ImGui.BeginTooltip();ImGui.Text(line.Forms[Math.Min(stage,line.Forms.Length-1)].Name+(line.Shop?$" / {line.Tier} gold":" / Wild")+(owned is null?"":owned.Any(entry=>entry.Spot.IsBoard)?" / On your board":" / On your bench"));ImGui.EndTooltip();
                }
            }
            ImGui.PopStyleColor(4);y+=30;
        }
        foreach(string id in trait.CrestIds)
        {
            y+=6;var item=CodexData.Items.Single(entry=>entry.Id==id);
            string source=item.Recipes.Length>0?string.Join(" + ",item.Recipes[0].Select(component=>CodexData.Items.Single(i=>i.Id==component).Name)):"Found in "+item.DropSource;
            string label=item.Name+": "+source;
            float height=CodexDetailParagraph(default,width-32,14,false,(label,CodexMuted));
            ImGui.SetCursorScreenPos(origin+new float2(0,y+Math.Max(0,(height-26)/2)));CodexItemIcon(item.Id,26);
            CodexDetailParagraph(origin+new float2(32,y+Math.Max(0,(26-height)/2)),width-32,14,true,(label,CodexMuted));
            y+=Math.Max(26,height);
        }
        ImGui.SetCursorScreenPos(origin);ImGui.Dummy(new(width,y));
        CodexEndCard();ImGui.PopID();
    }
    private static void CodexBadge(CodexTrait trait,float size)
    {
        var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(size,size+2));var draw=ImGui.GetWindowDrawList();var color=CodexColor(trait.Color);
        float2 center=at+new float2(size/2,(size+2)/2);
        // One convex hexagon: separate triangles left anti-aliased seams through the badge.
        for(int i=0;i<6;i++){float a=(i*60-90)*MathF.PI/180;draw.PathLineTo(center+new float2(MathF.Cos(a)*size/2,MathF.Sin(a)*(size+2)/2));}
        draw.PathFillConvex(color);
        string icon=trait.Icon;
        if(icon.Length==0||icon.Contains('/')||icon.Contains('.'))
        {icon=new string(trait.Name.Where(char.IsUpper).Take(2).ToArray());if(icon.Length==0)icon=trait.Name[..Math.Min(2,trait.Name.Length)];}
        var textSize=ImGui.CalcTextSize(icon);draw.AddText(center-textSize*.5f,new ImColor8(16,19,26),icon);
    }
    private void CodexPortrait(CodexLine line,int stage,float2 at,float extent)
    {
        if(!ImGui.IsRectVisible(at,at+new float2(extent,extent)))return;
        // style.css .pt: the family-coloured radial tile behind the portrait, framed in the tier colour.
        CodexLinePortrait(line,stage,at,extent);
    }
    private void CodexTraitLinks(CodexLine line,bool interactive)
    {
        float right=ImGui.GetCursorScreenPos().X+ImGui.GetContentRegionAvail().X;bool first=true;
        foreach(string id in line.Traits)
        {
            var trait=CodexData.Traits.Single(t=>t.Id==id);
            float width=ImGui.CalcTextSize(trait.Name).X+(interactive?26:16);if(!first)CodexFlowNext(width,right);first=false;
            if(interactive)
            {
                if(CodexButton("codexTrait/"+id,trait.Name,Math.Min(width,ImGui.GetContentRegionAvail().X),color:CodexColor(trait.Color)))
                {
                    codexUnit=null;codexFocusTrait=id;codexSideRules=false;codexTab="traits";codexKind="all";codexSearch.SetValue(""u8);
                    if(codexCounts.GetValueOrDefault(id)==0)codexPool=true;
                }
            }
            else CodexPill(trait.Name,CodexColor(trait.Color));
        }
    }
}
