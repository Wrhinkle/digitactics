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
            .OrderBy(trait=>CodexKindOrder(trait.Kind)).ThenBy(trait=>trait.Name,StringComparer.OrdinalIgnoreCase),300,trait=>CodexTraitCard(trait,false));
    }
    private void CodexTraitCard(CodexTrait trait,bool live)
    {
        CodexRows++;ImGui.PushID(trait.Id);var color=CodexColor(trait.Color);
        int count=live?codexCounts.GetValueOrDefault(trait.Id):0,next=trait.Breakpoints.FirstOrDefault(bp=>bp.Count>count)?.Count??0;
        CodexBadge(trait,28);ImGui.SameLine();ImGui.BeginGroup();CodexText(trait.Name,color);HomeLabel(CodexTitle(trait.Kind),11,HomeMute);ImGui.EndGroup();
        if(codexFocusTrait==trait.Id){ImGui.SetScrollHereY(.05f);codexFocusTrait=null;}
        if(live)CodexText(next>0?$"{count} / {next} different Digimon":$"{count} different Digimon",count>=trait.Breakpoints.FirstOrDefault()?.Count?HomeGold:HomeMute);
        if(trait.Description.Length>0)CodexText(trait.Description);
        foreach(string note in trait.Notes)CodexText(note,HomeMute);
        foreach(var point in trait.Breakpoints)CodexText($"({point.Count}) {point.Text}",live&&count>=point.Count?HomeInk:HomeMute);
        var lines=trait.LineIds.Select(id=>CodexData.Lines.Single(line=>line.Id==id)).Where(line=>!live||line.Shop)
            .OrderBy(line=>!line.Shop).ThenBy(line=>line.Tier).ThenBy(line=>line.Forms[0].Name,StringComparer.OrdinalIgnoreCase).ToArray();
        int columns=Math.Max(1,(int)(ImGui.GetContentRegionAvail().X/38)),column=0;bool wild=false;
        for(int i=0;i<lines.Length;i++)
        {
            var line=lines[i];
            if(!line.Shop&&!wild){HomeLabel("Wild",11,HomeMute);wild=true;column=0;}
            if(column++%columns!=0)ImGui.SameLine(0,4);
            var owned=live?codexOwned.GetValueOrDefault(line.Id):null;
            int stage=owned?.Max(entry=>entry.Unit.Stage)??0;
            var at=ImGui.GetCursorScreenPos();
            if(Button("codexLine/"+trait.Id+"/"+line.Id,"##codexLine/"+line.Id,34,34)){codexUnit=line.Id;codexResetScroll=true;}
            CodexPortrait(line,stage,at+new float2(2,2),30);
            if(owned is not null)ImGui.GetWindowDrawList().AddCircleFilled(at+new float2(31,3),4,owned.Any(entry=>entry.Spot.IsBoard)?new ImColor8(71,206,142):new ImColor8(81,161,238));
            if(stage>0)ImGui.GetWindowDrawList().AddText(at+new float2(4,23),HomeGold,new string('*',stage+1));
            if(ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();ImGui.Text(line.Forms[Math.Min(stage,line.Forms.Length-1)].Name+(line.Shop?$" / {line.Tier} gold":" / Wild")+(owned is null?"":owned.Any(entry=>entry.Spot.IsBoard)?" / On your board":" / On your bench"));ImGui.EndTooltip();
            }
        }
        foreach(string id in trait.CrestIds)
        {
            var item=CodexData.Items.Single(entry=>entry.Id==id);ImGui.Spacing();CodexItemIcon(item.Id,24);ImGui.SameLine();
            string origin=item.Recipes.Length>0?string.Join(" + ",item.Recipes[0].Select(component=>CodexData.Items.Single(i=>i.Id==component).Name)):"Found in "+item.DropSource;
            CodexText(item.Name+": "+origin,HomeMute);
        }
        ImGui.Separator();ImGui.Spacing();ImGui.PopID();
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
            HomeLabel((group.Key==100?"Wild Digimon (PvE only)":group.Key+" gold")+$" / {group.Count()}",16,CodexColor(group.First().Color));
            CodexGrid("Codex lines/"+group.Key,group,280,CodexLineCard);ImGui.Spacing();
        }
    }
    private void CodexLineCard(CodexLine line)
    {
        CodexRows++;ImGui.PushID(line.Id);
        if(HomeButton("codexLine/"+line.Id,line.Forms[0].Name,ImGui.GetContentRegionAvail().X,29))codexUnit=line.Id;
        CodexText((line.Shop?$"{line.Tier} gold":"Wild")+" / "+line.Attribute,CodexColor(line.AttributeColor));
        var forms=line.Forms.Select(form=>form.Name).Distinct().Count()==1?line.Forms.Take(1):line.Forms;
        float cell=ImGui.GetContentRegionAvail().X/3;var start=ImGui.GetCursorScreenPos();int i=0;float formBottom=0;
        foreach(var form in forms)
        {
            var at=start+new float2(cell*i++,0);CodexPortrait(line,form.Stage,at+new float2((cell-50)/2,0),50);
            ImGui.SetCursorScreenPos(at+new float2(0,54));ImGui.BeginGroup();ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+cell-4);ImGui.Text(form.Name);ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(201,166,255));ImGui.Text(form.AbilityName);ImGui.PopStyleColor();ImGui.PopTextWrapPos();ImGui.EndGroup();formBottom=Math.Max(formBottom,ImGui.GetItemRectMax().Y-start.Y);
        }
        ImGui.SetCursorScreenPos(start+new float2(0,formBottom+8));
        CodexTraitLinks(line,false);CodexText(line.Forms[0].AbilityName+": "+line.Forms[0].AbilityText,HomeMute);
        CodexText($"HP {string.Join('/',line.Forms.Select(form=>form.Stats.MaxHp))} / AD {string.Join('/',line.Forms.Select(form=>form.Stats.Ad))} / AS {line.Forms[0].Stats.As} / Range {line.Forms[0].Stats.Range}",HomeMute);
        ImGui.PopID();
    }
    private void CodexTraitLinks(CodexLine line,bool interactive)
    {
        foreach(string id in line.Traits)
        {
            var trait=CodexData.Traits.Single(t=>t.Id==id);
            if(interactive)
            {
                if(Button("codexTrait/"+id,trait.Name))
                {
                    codexUnit=null;codexFocusTrait=id;codexSideRules=false;codexTab="traits";codexKind="all";codexSearch.SetValue(""u8);
                    if(codexCounts.GetValueOrDefault(id)==0)codexPool=true;
                }
            }
            else CodexText(trait.Name,CodexColor(trait.Color));
        }
    }
    private void CodexUnitDetail(CodexLine line)
    {
        CodexRows++;HomeLabel(line.Forms[0].Name+" line",20,CodexColor(line.Color));
        CodexText((line.Shop?$"{line.Tier} gold":$"Wild (tier {line.Tier})")+" / "+line.Attribute,CodexColor(line.AttributeColor));
        if(Session is not null&&line.Shop)CodexText($"{Session.State.Pool.Remaining(line.Id)}/{Rules.PoolSizes[line.Tier]} left in the pool",HomeMute);
        CodexTraitLinks(line,true);CodexText(line.Attribute+": "+line.AttributeText,HomeMute);
        if(codexOwned.TryGetValue(line.Id,out var owned))CodexText("You have: "+string.Join(", ",owned.Select(entry=>line.Forms[entry.Unit.Stage].Name+(entry.Unit.Stage>0?" "+new string('*',entry.Unit.Stage+1):"")+(entry.Spot.IsBoard?" (board)":" (bench)"))),new ImColor8(89,211,151));
        var stats=line.Forms[0].Stats;CodexText($"AS {stats.As} / Range {stats.Range} / Armor {stats.Armor} / MR {stats.Mr} / Mana {stats.Mana}/{stats.MaxMana}",HomeMute);
        foreach(var form in line.Forms)
        {
            ImGui.Separator();var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(48,48));CodexPortrait(line,form.Stage,at,48);ImGui.SameLine();
            CodexText(form.Name+" "+new string('*',form.Stage+1)+" / "+form.StageName,HomeInk);
            CodexText($"HP {form.Stats.MaxHp} / AD {form.Stats.Ad}");CodexText(form.AbilityName,new ImColor8(217,184,255));CodexText(form.AbilityText);
        }
        ImGui.Separator();CodexText(line.Shop?"Three of one form digivolve into the next. Sells for "+string.Join(" / ",line.Forms.Select(form=>form.SellValue))+" gold.":"Wild Digimon appear in PvE rounds only and never count for traits.",HomeMute);
    }
    private void CodexUnitModal()
    {
        var line=CodexData.Lines.Single(item=>item.Id==codexUnit);
        // Modal input stays in this window; the full Codex remains available after Back or Escape.
        float width=Math.Min(460,size.X-48),height=Math.Min(650,size.Y-64);
        if(codexModalLine!=codexUnit){codexModalLine=codexUnit;codexModalFrame=ImGui.GetFrameCount();}
        ImGui.SetNextWindowFocus();ImGui.SetNextWindowBgAlpha(1);ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(5,15,25));
        Window("Codex Digimon detail",(size.X-width)/2,(size.Y-height)/2,width,height,ImGuiWindowFlags.NoTitleBar);ImGui.PopStyleColor();
        if(Button("codexCloseUnit","Back to Codex",140,28))codexUnit=null;
        ImGui.Separator();CodexUnitDetail(line);ImGui.End();
        var pointer=ImGui.GetIO().MousePos;
        if(ImGui.GetFrameCount()>codexModalFrame&&ImGui.IsMouseClicked(ImGuiMouseButton.Left)&&
            (pointer.X<(size.X-width)/2||pointer.X>(size.X+width)/2||pointer.Y<(size.Y-height)/2||pointer.Y>(size.Y+height)/2))codexUnit=null;
    }
}
