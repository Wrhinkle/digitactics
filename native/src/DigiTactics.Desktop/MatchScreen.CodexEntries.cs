using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private void CodexItemIcon(string id,float extent)
    {
        var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(extent,extent));
        if(ImGui.IsRectVisible(at,at+new float2(extent,extent))&&ItemArtwork.Has(CodexData.Id,id))itemArtwork.Draw(CodexData.Id,id,at,extent);
        if(ImGui.IsItemHovered())
        {
            var item=CodexData.Items.Single(entry=>entry.Id==id);ImGui.SetNextWindowSize(new(380,0));ImGui.BeginTooltip();CodexHeading(item.Name);CodexText(item.Description);ImGui.EndTooltip();
        }
    }
    private void CodexItems()
    {
        var entries=CodexData.Items.Where(item=>CodexMatch(item.SearchText)).ToArray();
        if(entries.Length==0)return;
        void Group(string kind,string title)
        {
            var items=entries.Where(item=>item.Kind==kind).ToArray();if(items.Length==0)return;
            CodexHeading(title+" / "+items.Length);CodexGrid("Codex items/"+kind,items,330,CodexItemCard);ImGui.Spacing();
        }
        Group("component","Components");CodexRecipes();Group("core","Completed items");Group("crest","Crests");Group("special","Special");
    }
    private void CodexItemCard(CodexItem item)
    {
        CodexRows++;ImGui.PushID(item.Id);
        if(codexFocusItem==item.Id){ImGui.SetScrollHereY(.1f);codexFocusItem=null;}
        if(!CodexBeginCard("Item card")){CodexEndCard();ImGui.PopID();return;}
        CodexItemIcon(item.Id,44);ImGui.SameLine();CodexHeading(item.Name);
        Buttons["codexItem/"+item.Id]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
        if(item.StatsText.Length>0&&item.Kind!="component")CodexText(item.StatsText,HomeGold);
        CodexText(item.Description);
        foreach(var recipe in item.Recipes)
        {
            for(int i=0;i<recipe.Length;i++){if(i>0){ImGui.SameLine();ImGui.Text("+"u8);ImGui.SameLine();}CodexItemIcon(recipe[i],30);}
            CodexText(string.Join(" + ",recipe.Select(id=>CodexData.Items.Single(entry=>entry.Id==id).Name)),HomeMute);
        }
        if(item.Recipes.Length==0&&item.Drop)CodexText("No recipe: found in "+item.DropSource,HomeMute);
        CodexEndCard();ImGui.PopID();
    }
    private void CodexRecipes()
    {
        var data=CodexData;if(data.ComponentIds.Length==0)return;
        CodexHeading("RECIPE GRID");CodexText("Two components make one item.",CodexMuted);
        const float cell=44,icon=36;
        ImGui.BeginChild("Codex recipes"u8,new(0,(data.ComponentIds.Length+1)*cell+20),ImGuiChildFlags.None,ImGuiWindowFlags.HorizontalScrollbar);
        var start=ImGui.GetCursorScreenPos();
        for(int i=0;i<data.ComponentIds.Length;i++)
        {
            ImGui.SetCursorScreenPos(start+new float2((i+1)*cell,0));CodexItemIcon(data.ComponentIds[i],icon);
            ImGui.SetCursorScreenPos(start+new float2(0,(i+1)*cell));CodexItemIcon(data.ComponentIds[i],icon);
            for(int j=0;j<data.ComponentIds.Length;j++)
            {
                string id=data.RecipeGrid[i][j];if(string.IsNullOrEmpty(id))continue;
                var at=start+new float2((j+1)*cell,(i+1)*cell);ImGui.SetCursorScreenPos(at);
                if(Button("codexRecipe/"+i+"/"+j,"##recipe/"+i+"/"+j,40,40))codexFocusItem=id;
                if(ItemArtwork.Has(data.Id,id)&&ImGui.IsRectVisible(at,at+new float2(40,40)))itemArtwork.Draw(data.Id,id,at+new float2(2,2),icon);
                var item=data.Items.Single(entry=>entry.Id==id);
                if(!CodexMatch(item.SearchText))ImGui.GetWindowDrawList().AddRectFilled(at,at+new float2(40,40),new ImColor8(5,15,25,195));
                if(ImGui.IsItemHovered()){ImGui.SetNextWindowSize(new(380,0));ImGui.BeginTooltip();CodexHeading(item.Name);CodexText(item.StatsText,CodexGold);CodexText(item.Description);ImGui.EndTooltip();}
                CodexRecipeCells++;
            }
        }
        ImGui.SetCursorScreenPos(start+new float2((data.ComponentIds.Length+1)*cell,(data.ComponentIds.Length+1)*cell));ImGui.Dummy(new(1,1));ImGui.EndChild();
    }
    private void CodexAugments()
    {
        foreach(var group in CodexData.Augments.Where(augment=>CodexMatch(augment.SearchText)).GroupBy(augment=>augment.Tier).OrderBy(group=>group.Key))
        {
            var first=group.First();CodexHeading(first.TierName+" / "+group.Count(),22,CodexColor(first.Color));
            CodexGrid("Codex augments/"+group.Key,group,330,augment=>
            {
                CodexRows++;var color=CodexColor(augment.Color);
                if(!CodexBeginCard("Augment/"+augment.Id,color)){CodexEndCard();return;}
                var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(22,26));
                var draw=ImGui.GetWindowDrawList();draw.AddQuadFilled(at+new float2(11,2),at+new float2(21,12),at+new float2(11,22),at+new float2(1,12),color);
                ImGui.SameLine();CodexHeading(augment.Name,22,color);CodexText(augment.Description);CodexEndCard();
            });ImGui.Spacing();
        }
    }
    private void CodexWisps()
    {
        CodexText("A one-use Wisp appears in the sixth shop slot on even rounds from stage 2. Its price is its base cost plus the stage number. Unbought Wisps leave at the end of the round.",HomeMute);
        foreach(var group in CodexData.Wisps.Where(wisp=>CodexMatch(wisp.SearchText)).GroupBy(wisp=>wisp.Kind))
        {
            CodexHeading(group.Key+" / "+group.Count());
            CodexGrid("Codex wisps/"+group.Key,group,330,wisp=>
            {
                CodexRows++;
                if(!CodexBeginCard("Wisp/"+wisp.Id)){CodexEndCard();return;}
                var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(26,26));var draw=ImGui.GetWindowDrawList();
                draw.AddCircleFilled(at+new float2(13,13),12,new ImColor8(42,123,216));draw.AddCircleFilled(at+new float2(12,12),8,new ImColor8(127,227,255));draw.AddCircleFilled(at+new float2(9,9),4,new ImColor8(255,255,255));
                ImGui.SameLine();CodexHeading(wisp.Name);CodexText((wisp.BasePrice>0?wisp.BasePrice+" + stage":"stage")+" gold",CodexGold);
                CodexText(wisp.Description);CodexText($"From stage {wisp.MinStage}. At stage {wisp.ExampleStage}: {wisp.ExampleCost} gold.",CodexMuted);CodexEndCard();
            });ImGui.Spacing();
        }
    }
    private void CodexRules(bool compact)
    {
        var entries=CodexData.Rules.Where(rule=>compact||CodexMatch(rule.SearchText)).ToArray();
        if(compact)
        {
            foreach(var rule in entries)
            {
                CodexRows++;ImGui.SetNextItemOpen(rule==entries[0],ImGuiCond.Once);
                if(ImGui.CollapsingHeader(rule.Title+"##codexRule/"+rule.Id))CodexRuleBody(rule);
            }
        }
        else CodexGrid("Codex rules",entries,440,rule=>
        {
            CodexRows++;
            if(CodexBeginCard("Rule/"+rule.Id)){CodexHeading(rule.Title,22,CodexGold);CodexRuleBody(rule);}
            CodexEndCard();
        });
    }
    private static void CodexRuleBody(CodexRule rule)
    {
        ImGui.PushID(rule.Id);int index=0;
        foreach(var block in rule.Blocks)
        {
            if(block.Kind=="paragraph"){CodexText(block.Text);ImGui.Spacing();continue;}
            int columns=block.Rows.Max(row=>row.Length);if(columns==0)continue;
            ImGui.PushID(index++);ImGui.BeginChild("Rule table"u8,new(0,Math.Min(320,block.Rows.Length*34+24)),ImGuiChildFlags.None,ImGuiWindowFlags.HorizontalScrollbar);
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding,new float2(7,6));
            if(ImGui.BeginTable("Rule values"u8,columns,ImGuiTableFlags.Borders|ImGuiTableFlags.SizingStretchSame,new(Math.Max(columns*62,ImGui.GetContentRegionAvail().X),0)))
            {
                for(int row=0;row<block.Rows.Length;row++)
                {
                    ImGui.TableNextRow();
                    for(int col=0;col<columns;col++){ImGui.TableSetColumnIndex(col);CodexText(block.Rows[row].ElementAtOrDefault(col)??"",row==0?CodexGold:CodexBody);}
                }
                ImGui.EndTable();
            }
            ImGui.PopStyleVar();ImGui.EndChild();ImGui.PopID();
        }
        ImGui.PopID();
    }
}
