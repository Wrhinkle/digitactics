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
            .OrderBy(trait=>CodexKindOrder(trait.Kind)).ThenBy(trait=>trait.Name,StringComparer.OrdinalIgnoreCase),340,trait=>CodexTraitCard(trait,false));
    }
    private void CodexTraitCard(CodexTrait trait,bool live)
    {
        CodexRows++;ImGui.PushID(trait.Id);var color=CodexColor(trait.Color);
        if(codexFocusTrait==trait.Id){ImGui.SetScrollHereY(.05f);codexFocusTrait=null;}
        if(!CodexBeginCard("Trait card",color,compact:live)){CodexEndCard();ImGui.PopID();return;}
        int count=live?codexCounts.GetValueOrDefault(trait.Id):0,next=trait.Breakpoints.FirstOrDefault(bp=>bp.Count>count)?.Count??0;
        CodexBadge(trait,live?30:36);ImGui.SameLine();ImGui.BeginGroup();CodexHeading(trait.Name,22,color);CodexText(CodexTitle(trait.Kind),CodexMuted,16);ImGui.EndGroup();
        if(live)CodexText(next>0?$"{count} / {next} different Digimon":$"{count} different Digimon",count>=trait.Breakpoints.FirstOrDefault()?.Count?HomeGold:HomeMute);
        if(trait.Description.Length>0)CodexText(trait.Description);
        foreach(string note in trait.Notes)
        {
            var noteAt=ImGui.GetCursorScreenPos();ImGui.Indent(10);CodexText(note,CodexMuted);ImGui.Unindent(10);
            ImGui.GetWindowDrawList().AddRectFilled(noteAt,new(noteAt.X+2,ImGui.GetItemRectMax().Y),CodexStroke);
        }
        ImGui.Spacing();
        foreach(var point in trait.Breakpoints)
        {
            bool active=live&&count>=point.Count;var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(30,26));
            var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(at,at+new float2(30,26),active?color:new ImColor8(28,39,62),2);
            string threshold=point.Count.ToString();draw.AddText(at+new float2((30-ImGui.CalcTextSize(threshold).X)/2,4),active?CodexNavy:CodexMuted,threshold);
            ImGui.SameLine(0,8);CodexText(point.Text,active?CodexInk:CodexMuted);
        }
        var lines=trait.LineIds.Select(id=>CodexData.Lines.Single(line=>line.Id==id)).Where(line=>!live||line.Shop)
            .OrderBy(line=>!line.Shop).ThenBy(line=>line.Tier).ThenBy(line=>line.Forms[0].Name,StringComparer.OrdinalIgnoreCase).ToArray();
        int columns=Math.Max(1,(int)((ImGui.GetContentRegionAvail().X+6)/46)),column=0;bool wild=false;
        ImGui.Spacing();
        for(int i=0;i<lines.Length;i++)
        {
            var line=lines[i];
            if(!line.Shop&&!wild){CodexText("WILD",CodexMuted,16);wild=true;column=0;}
            if(column++%columns!=0)ImGui.SameLine(0,6);
            var owned=live?codexOwned.GetValueOrDefault(line.Id):null;
            int stage=owned?.Max(entry=>entry.Unit.Stage)??0;
            var at=ImGui.GetCursorScreenPos();
            if(Button("codexLine/"+trait.Id+"/"+line.Id,"##codexLine/"+line.Id,40,40)){codexUnit=line.Id;codexResetScroll=true;}
            CodexPortrait(line,stage,at+new float2(2,2),36);
            if(owned is not null)ImGui.GetWindowDrawList().AddCircleFilled(at+new float2(37,4),5,owned.Any(entry=>entry.Spot.IsBoard)?new ImColor8(71,206,142):new ImColor8(81,161,238));
            if(stage>0)ImGui.GetWindowDrawList().AddText(at+new float2(4,26),HomeGold,new string('*',stage+1));
            if(ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();ImGui.Text(line.Forms[Math.Min(stage,line.Forms.Length-1)].Name+(line.Shop?$" / {line.Tier} gold":" / Wild")+(owned is null?"":owned.Any(entry=>entry.Spot.IsBoard)?" / On your board":" / On your bench"));ImGui.EndTooltip();
            }
        }
        foreach(string id in trait.CrestIds)
        {
            var item=CodexData.Items.Single(entry=>entry.Id==id);ImGui.Spacing();CodexItemIcon(item.Id,32);ImGui.SameLine();
            string origin=item.Recipes.Length>0?string.Join(" + ",item.Recipes[0].Select(component=>CodexData.Items.Single(i=>i.Id==component).Name)):"Found in "+item.DropSource;
            CodexText(item.Name+": "+origin,HomeMute);
        }
        CodexEndCard();ImGui.PopID();
    }
    private static void CodexBadge(CodexTrait trait,float size)
    {
        var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(size,size+2));var draw=ImGui.GetWindowDrawList();var color=CodexColor(trait.Color);
        float2 center=at+new float2(size/2,(size+2)/2);
        for(int i=0;i<6;i++)
        {
            float a=(i*60-90)*MathF.PI/180,b=((i+1)*60-90)*MathF.PI/180;
            draw.AddTriangleFilled(center,center+new float2(MathF.Cos(a)*size/2,MathF.Sin(a)*(size+2)/2),center+new float2(MathF.Cos(b)*size/2,MathF.Sin(b)*(size+2)/2),color);
        }
        string icon=trait.Icon;
        if(icon.Length==0||icon.Contains('/')||icon.Contains('.'))
        {icon=new string(trait.Name.Where(char.IsUpper).Take(2).ToArray());if(icon.Length==0)icon=trait.Name[..Math.Min(2,trait.Name.Length)];}
        var textSize=ImGui.CalcTextSize(icon);draw.AddText(center-textSize*.5f,new ImColor8(16,19,26),icon);
    }
    private void CodexPortrait(CodexLine line,int stage,float2 at,float extent)
    {
        if(!ImGui.IsRectVisible(at,at+new float2(extent,extent)))return;
        var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(at,at+new float2(extent,extent),new ImColor8(22,32,48),4);
        int before=portraits.DrawCalls;portraits.Draw(CodexData.Id,line.Id,stage,at,extent);
        if(portraits.DrawCalls==before){string name=line.Forms[Math.Min(stage,line.Forms.Length-1)].Name;draw.AddText(at+new float2(4,4),HomeMute,name[..Math.Min(2,name.Length)]);}
        draw.AddRect(at,at+new float2(extent,extent),CodexColor(line.Color),4);CodexPortraits++;
    }
    private void CodexLineList()
    {
        var lines=CodexData.Lines.Where(line=>CodexMatch(line.SearchText)&&(codexCost=="all"||(codexCost=="wild"?!line.Shop:line.Shop&&line.Tier.ToString()==codexCost))).ToArray();
        foreach(var group in lines.GroupBy(line=>line.Shop?line.Tier:100).OrderBy(group=>group.Key))
        {
            CodexHeading((group.Key==100?"Wild Digimon (PvE only)":group.Key+" gold")+$" / {group.Count()}",22,CodexColor(group.First().Color));
            CodexGrid("Codex lines/"+group.Key,group,340,CodexLineCard);ImGui.Spacing();
        }
    }
    private void CodexLineCard(CodexLine line)
    {
        CodexRows++;ImGui.PushID(line.Id);
        if(!CodexBeginCard("Digimon card",CodexColor(line.Color),topStripe:true)){CodexEndCard();ImGui.PopID();return;}
        ImGui.PushFont(NativeUiTheme.DisplayFont,22);float titleWidth=ImGui.GetContentRegionAvail().X;var titleAt=ImGui.GetCursorScreenPos();
        float titleHeight=Math.Max(36,ImGui.CalcTextSize(line.Forms[0].Name,false,titleWidth-12).Y+12);
        ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(1,10,19,0));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(28,44,61));
        if(Button("codexLine/"+line.Id,"##lineTitle",titleWidth,titleHeight))codexUnit=line.Id;
        ImGui.GetWindowDrawList().AddText(ImGui.GetFont(),22,titleAt+new float2(6,6),CodexInk,line.Forms[0].Name,titleWidth-12);
        ImGui.PopStyleColor(2);ImGui.PopFont();
        CodexPill(line.Shop?$"{line.Tier} gold":"Wild",CodexColor(line.Color));ImGui.SameLine();CodexPill(line.Attribute,CodexColor(line.AttributeColor));
        var forms=line.Forms.Select(form=>form.Name).Distinct().Count()==1?line.Forms.Take(1):line.Forms;
        float cell=ImGui.GetContentRegionAvail().X/3;var start=ImGui.GetCursorScreenPos();int i=0;float formBottom=0;
        foreach(var form in forms)
        {
            var at=start+new float2(cell*i++,0);CodexPortrait(line,form.Stage,at+new float2((cell-62)/2,0),62);
            ImGui.SetCursorScreenPos(at+new float2(0,68));ImGui.BeginGroup();ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+cell-6);ImGui.Text(form.Name);ImGui.PushStyleColor(ImGuiCol.Text,CodexAbility);ImGui.Text(form.AbilityName);ImGui.PopStyleColor();ImGui.PopTextWrapPos();ImGui.EndGroup();formBottom=Math.Max(formBottom,ImGui.GetItemRectMax().Y-start.Y);
        }
        ImGui.SetCursorScreenPos(start+new float2(0,formBottom+8));
        CodexTraitLinks(line,false);CodexText(line.Forms[0].AbilityText,CodexMuted);
        CodexText($"HP {string.Join('/',line.Forms.Select(form=>form.Stats.MaxHp))} / AD {string.Join('/',line.Forms.Select(form=>form.Stats.Ad))} / AS {line.Forms[0].Stats.As} / Range {line.Forms[0].Stats.Range}",HomeMute);
        CodexEndCard();ImGui.PopID();
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
    private void CodexUnitDetail(CodexLine line)
    {
        CodexRows++;CodexHeading(line.Forms[0].Name+" line",codexFull?28:22,CodexColor(line.Color));
        CodexPill(line.Shop?$"{line.Tier} gold":$"Wild (tier {line.Tier})",CodexColor(line.Color));
        if(ImGui.GetItemRectMax().X+ImGui.CalcTextSize(line.Attribute).X+24<ImGui.GetWindowPos().X+ImGui.GetWindowWidth()-16)ImGui.SameLine();
        CodexPill(line.Attribute,CodexColor(line.AttributeColor));
        if(Session is not null&&line.Shop)CodexText($"{Session.State.Pool.Remaining(line.Id)}/{Rules.PoolSizes[line.Tier]} left in the pool",HomeMute);
        CodexTraitLinks(line,true);CodexText(line.Attribute+": "+line.AttributeText,HomeMute);
        if(codexOwned.TryGetValue(line.Id,out var owned))CodexText("You have: "+string.Join(", ",owned.Select(entry=>line.Forms[entry.Unit.Stage].Name+(entry.Unit.Stage>0?" "+new string('*',entry.Unit.Stage+1):"")+(entry.Spot.IsBoard?" (board)":" (bench)"))),new ImColor8(89,211,151));
        var stats=line.Forms[0].Stats;CodexText($"AS {stats.As} / Range {stats.Range} / Armor {stats.Armor} / MR {stats.Mr} / Mana {stats.Mana}/{stats.MaxMana}",HomeMute);
        foreach(var form in line.Forms)
        {
            if(!CodexBeginCard("Form/"+form.Stage,CodexColor(line.Color),topStripe:true,compact:!codexFull)){CodexEndCard();continue;}
            float portraitSize=codexFull?76:48;var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(portraitSize,portraitSize));CodexPortrait(line,form.Stage,at,portraitSize);
            if(codexFull)ImGui.SameLine(0,14);
            ImGui.BeginGroup();CodexHeading(form.Name,22);CodexText(new string('*',form.Stage+1)+" / "+form.StageName,CodexGold);ImGui.EndGroup();
            CodexText($"HP {form.Stats.MaxHp}   /   AD {form.Stats.Ad}");CodexHeading(form.AbilityName,22,CodexAbility);CodexText(form.AbilityText);
            CodexEndCard();
        }
        ImGui.Separator();CodexText(line.Shop?"Three of one form digivolve into the next. Sells for "+string.Join(" / ",line.Forms.Select(form=>form.SellValue))+" gold.":"Wild Digimon appear in PvE rounds only and never count for traits.",HomeMute);
    }
    private void CodexUnitModal()
    {
        var line=CodexData.Lines.Single(item=>item.Id==codexUnit);
        // Modal input stays in this window; the full Codex remains available after Back or Escape.
        float width=Math.Min(640,size.X-64),height=Math.Min(780,size.Y-ClientPageTop-48),top=ClientPageTop+(size.Y-ClientPageTop-height)/2;
        if(codexModalLine!=codexUnit){codexModalLine=codexUnit;codexModalFrame=ImGui.GetFrameCount();}
        ImGui.SetNextWindowFocus();CodexBeginWindow("Codex Digimon detail",new((size.X-width)/2,top),new(width,height),true);
        if(CodexButton("codexCloseUnit","Back to Codex",160))codexUnit=null;
        ImGui.Separator();CodexUnitDetail(line);CodexEndWindow();
        var pointer=ImGui.GetIO().MousePos;
        if(ImGui.GetFrameCount()>codexModalFrame&&ImGui.IsMouseClicked(ImGuiMouseButton.Left)&&
            (pointer.X<(size.X-width)/2||pointer.X>(size.X+width)/2||pointer.Y<top||pointer.Y>top+height))codexUnit=null;
    }
}
