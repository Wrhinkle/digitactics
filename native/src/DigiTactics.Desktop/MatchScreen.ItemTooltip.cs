using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal int ItemTooltipRecipes { get; private set; }
    private void ItemTooltip(string id)
    {
        var item = Rules.Items.Single(item => item.Id == id);
        BeginInspectionTooltip(item.Component?420:380,ItemInspectionTone(item),itemCard:true);
        ItemInspectionBody(id);
        ItemTooltipRecipes = 0;
        if(item.Component)ItemInspectionRecipes(id);
        else
        {
            var recipes=Rules.Recipes!.Where(recipe=>recipe.Value==id).Select(recipe=>string.Join(" + ",recipe.Key.Split('+').Select(Name))).ToArray();
            if(recipes.Length>0){ImGui.Separator();CodexText("Made from "+string.Join(" or ",recipes),CodexMuted,15);ItemTooltipRecipes=recipes.Length;}
        }
        if(item.Consumable)CodexText("Drag onto a Digimon to use it.",CodexMuted,15);
        EndInspectionTooltip();
    }
    private ImColor8 ItemInspectionTone(ItemDefinition item)
    {
        if(item.Emblem is {} emblem&&CodexData.Traits.FirstOrDefault(trait=>trait.Id==emblem) is {} trait)return CodexColor(trait.Color);
        if(item.Id=="golden_egg")return new(255,216,74);
        if(item.Consumable)return new(255,143,163);
        if(item.Component)return item.Id=="egg"?new(255,243,176):new(183,192,207);
        return new(255,207,74);
    }
    private void ItemInspectionBody(string id)
    {
        var item=Rules.Items.Single(candidate=>candidate.Id==id);
        var description = text.Get(Rules.Id, id);
        var entry=CodexData.Items.FirstOrDefault(candidate=>candidate.Id==id);
        var min = ImGui.GetCursorScreenPos();
        ImGui.Dummy(new(58,58));itemArtwork.Draw(Rules.Id,id,min,58);
        ImGui.SameLine(0,10);ImGui.BeginGroup();CodexHeading(description.Name,20);
        string category=item.Consumable?"Consumable":item.Emblem is not null?"Crest":id=="golden_egg"?"Special item":item.Component?id=="egg"?"Special component":"Component":"Completed item";
        CodexText(category.ToUpperInvariant(),ItemInspectionTone(item),13);ImGui.EndGroup();
        if(entry is { StatsText.Length:>0 })CodexText(entry.StatsText,CodexGold,16);
        if(!item.Component||id=="egg"||entry is null||entry.StatsText.Length==0)CodexText(description.Description);
        if(item.Emblem is {} emblem&&CodexData.Traits.FirstOrDefault(trait=>trait.Id==emblem) is {} trait)
        {
            ImGui.Separator();CodexHeading("Adds "+trait.Name,17,CodexColor(trait.Color));CodexText(trait.Description,CodexMuted,15);
            foreach(var breakpoint in trait.Breakpoints)CodexText($"({breakpoint.Count}) {breakpoint.Text}",CodexMuted,14);
        }
    }
    private void ItemInspectionRecipes(string id)
    {
        // itemcards.js counts spare components on both the bench and deployed
        // Digimon, excluding the hovered component for a same-component recipe.
        var owned=Human.Items.Concat(Human.Roster.Entries.SelectMany(entry=>entry.Unit.Items)).GroupBy(value=>value).ToDictionary(group=>group.Key,group=>group.Count());
        var recipes=Rules.Items.Where(item=>item.Component).Select(component=>
        {
            string key=string.CompareOrdinal(id,component.Id)<=0?id+"+"+component.Id:component.Id+"+"+id;
            return (Component:component.Id,Result:Rules.Recipes!.GetValueOrDefault(key),Have:owned.GetValueOrDefault(component.Id)-(component.Id==id?1:0)>0&&owned.GetValueOrDefault(id)>0);
        }).Where(recipe=>recipe.Result is not null).ToArray();
        ImGui.Separator();CodexHeading("RECIPES",16,CodexMuted);
        int ready=recipes.Count(recipe=>recipe.Have);
        if(ready>0){ImGui.SameLine();CodexText($"{ready} you can make now",new ImColor8(90,223,138),14);}
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding,new float2(3,3));
        if(ImGui.BeginTable("Item combinations"u8,3,ImGuiTableFlags.SizingStretchSame))
        {
            foreach(var recipe in recipes)
            {
                ImGui.TableNextColumn();ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(5,5));ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,6);
                ImGui.PushStyleColor(ImGuiCol.Border,recipe.Have?new ImColor8(90,223,138):new ImColor8(47,55,70));
                ImGui.PushStyleColor(ImGuiCol.ChildBg,recipe.Have?new ImColor8(28,48,42):new ImColor8(27,34,48));
                if(ImGui.BeginChild("Item recipe/"+recipe.Component,new(0,0),ImGuiChildFlags.Borders|ImGuiChildFlags.AutoResizeY|ImGuiChildFlags.AlwaysUseWindowPadding,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse))
                {
                    var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
                    ImGui.Dummy(new(width,30));var start=at+new float2(Math.Max(0,(width-68)/2),0);
                    itemArtwork.Draw(Rules.Id,recipe.Component,start+new float2(0,5),20);
                    ImGui.GetWindowDrawList().AddText(start+new float2(23,7),CodexMuted,"→"u8);
                    itemArtwork.Draw(Rules.Id,recipe.Result!,start+new float2(39,0),30);
                    CodexText(Name(recipe.Result!),recipe.Have?new ImColor8(90,223,138):CodexBody,14);
                }
                ImGui.EndChild();ImGui.PopStyleColor(2);ImGui.PopStyleVar(2);ItemTooltipRecipes++;
            }
            ImGui.EndTable();
        }
        ImGui.PopStyleVar();
    }
    private void ItemInspectionEquation(string first,string second,string? result)
    {
        float width=ImGui.GetContentRegionAvail().X;var at=ImGui.GetCursorScreenPos()+new float2(Math.Max(0,(width-156)/2),0);
        ImGui.Dummy(new(width,42));itemArtwork.Draw(Rules.Id,first,at+new float2(0,5),32);
        var draw=ImGui.GetWindowDrawList();draw.AddText(at+new float2(39,12),CodexMuted,"+"u8);
        itemArtwork.Draw(Rules.Id,second,at+new float2(57,5),32);draw.AddText(at+new float2(96,12),CodexMuted,"→"u8);
        if(result is not null)itemArtwork.Draw(Rules.Id,result,at+new float2(117,0),40);
        else{draw.AddRect(at+new float2(117,0),at+new float2(157,40),CodexMuted,5);draw.AddText(at+new float2(132,11),CodexMuted,"×"u8);}
        ImGui.Separator();
    }
    private void ItemInspectionPair(string first,string second)
    {
        var a=Rules.Items.Single(item=>item.Id==first);var b=Rules.Items.Single(item=>item.Id==second);
        string? result=a.Component&&b.Component?Session!.Units.Items.Combine(first,second):null;
        BeginInspectionTooltip(390,result is not null?ItemInspectionTone(Rules.Items.Single(item=>item.Id==result)):CodexMuted,itemCard:true);
        ItemInspectionEquation(first,second,result);
        if(result is not null)ItemInspectionBody(result);
        else
        {
            CodexHeading("Doesn't combine",18,new ImColor8(255,90,106));
            CodexText(a.Consumable||b.Consumable?"Consumables are used on a Digimon.":a.Emblem is not null&&b.Emblem is not null?"Crests give their holders a trait. Put them on different Digimon.":first=="golden_egg"||second=="golden_egg"?"The Golden Digi-Egg doesn't combine.":"Only two components combine. Completed items stay as they are.");
        }
        ImGui.Separator();CodexText("Items combine on a Digimon. Drop here to swap places.",CodexMuted,15);EndInspectionTooltip();
    }
    private void ItemInspectionDrop(string id,PlanningUnit unit,string feedback)
    {
        var item=Rules.Items.Single(candidate=>candidate.Id==id);
        bool allowed=item.Consumable||feedback.StartsWith("Equip ",StringComparison.Ordinal)||feedback.StartsWith("Combines into ",StringComparison.Ordinal);
        string result=id;int replaced=-1;
        if(item.Component)
            for(int index=0;index<unit.Items.Length;index++)
                if(Rules.Items.Single(candidate=>candidate.Id==unit.Items[index]).Component&&Session!.Units.Items.Combine(unit.Items[index],id) is {} combined)
                {result=combined;replaced=index;break;}
        ImColor8 tone=allowed?ItemInspectionTone(Rules.Items.Single(candidate=>candidate.Id==result)):new(255,90,106);
        BeginInspectionTooltip(410,tone,itemCard:true);
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        ImGui.Dummy(new(width,48));portraits.Draw(Rules.Id,unit.Line,unit.Stage,at,40);
        var draw=ImGui.GetWindowDrawList();ImGui.PushFont(NativeUiTheme.DisplayFont,18);
        draw.AddText(ImGui.GetFont(),18,at+new float2(49,0),CodexInk,UnitName(unit),Math.Max(100,width-140));ImGui.PopFont();
        ImGui.PushFont(default,13);draw.AddText(at+new float2(49,31),CodexMuted,Human.Roster.Entries.First(entry=>entry.Unit.Id==unit.Id).Spot.IsBoard?"On your board":"On your bench");ImGui.PopFont();
        int highlighted=replaced>=0?replaced:unit.Items.Length;
        for(int slot=0;slot<3;slot++)
        {
            var slotAt=at+new float2(width-82+slot*28,10);bool active=allowed&&!item.Consumable&&slot==highlighted;
            draw.AddRectFilled(slotAt,slotAt+new float2(24,24),new ImColor8(14,19,29),4);draw.AddRect(slotAt,slotAt+new float2(24,24),active?tone:CodexStroke,4);
            string? held=unit.Items.ElementAtOrDefault(slot);
            if(held is not null)itemArtwork.Draw(Rules.Id,held,slotAt+new float2(1,1),22);
            else if(active)itemArtwork.Draw(Rules.Id,id,slotAt+new float2(1,1),22);
        }
        ImGui.Separator();CodexHeading(feedback,18,allowed?new ImColor8(90,223,138):new ImColor8(255,90,106));
        if(allowed)
        {
            if(!item.Consumable&&replaced>=0)ItemInspectionEquation(unit.Items[replaced],id,result);
            ItemInspectionBody(result);
            if(!item.Consumable)
            {
                var after=unit.Items.Where((_,index)=>index!=replaced).Append(result);
                int slots=ItemCapacity.Used(after,Rules.Items.ToDictionary(candidate=>candidate.Id));
                CodexText($"Items after: {slots}/3",CodexMuted,15);
            }
        }
        EndInspectionTooltip();
    }
}
