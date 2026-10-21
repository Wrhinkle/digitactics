using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal bool RefreshEnabled { get; private set; }
    internal bool BuyXpEnabled { get; private set; }
    private bool showMatchInventory;
    internal (float2 Min,float2 Max) MatchInventoryBounds {get;private set;}

    // The source #shopbar has an economy block, vertical action buttons and one
    // uninterrupted row of offers. The physical bench remains on the board.
    private void ShopControls()
    {
        var economy=Human.Economy;var origin=ImGui.GetCursorScreenPos();
        float economyWidth=Math.Clamp(size.X*.125f,150,200),controlsWidth=size.X<1250?90:110;
        var draw=ImGui.GetWindowDrawList();
        int capacity=new MatchPlacement(Session!.State).Capacity(Human,Session.Rounds.Position.Stage);
        HudClipText(draw,origin,economyWidth-52,$"Level {economy.Level}",HudInk,19);
        HudClipText(draw,origin+new float2(economyWidth-49,0),49,$"{Human.Roster.BoardCount}/{capacity}",Human.Roster.BoardCount<capacity?HudGold:HudInk,18);
        HudClipText(draw,origin+new float2(economyWidth-66,22),66,"on board",HudMuted,13);
        if(Rules.Economy!.Xp is {} xp)
        {
            bool maxed=economy.Level>=xp.MaxLevel;int need=maxed?0:xp.Table[economy.Level];
            HudClipText(draw,origin+new float2(0,24),economyWidth-72,maxed?"MAX":$"{economy.Xp}/{need} XP",HudMuted,14);
            float fraction=maxed?1:Math.Clamp((float)economy.Xp/Math.Max(1,need),0,1);
            draw.AddRectFilled(origin+new float2(0,45),origin+new float2(economyWidth,49),new ImColor8(34,42,59),2);
            draw.AddRectFilled(origin+new float2(0,45),origin+new float2(economyWidth*fraction,49),new ImColor8(63,214,255),2);
        }
        HudClipText(draw,origin+new float2(23,54),economyWidth-23,economy.Gold.ToString(),HudGold,26);
        draw.AddCircleFilled(origin+new float2(9,68),8,HudGold);draw.AddCircle(origin+new float2(9,68),8,new ImColor8(138,95,8));
        ImGui.PushFont(default,26);float goldWidth=ImGui.CalcTextSize(economy.Gold.ToString()).X;ImGui.PopFont();
        ImGui.SetCursorScreenPos(origin+new float2(Math.Min(economyWidth-82,29+goldWidth),61));IncomeDisplay();
        var income=Session.Rounds.PreviewIncome(Human);
        if(Rules.Economy.Interest is {} interest)
            HudClipText(draw,origin+new float2(0,86),economyWidth,$"Interest +{income.Interest}"+(income.Interest<interest.Max?$" ({interest.Per*(income.Interest+1)-Math.Max(0,economy.Gold)} to +{income.Interest+1})":" (max)"),HudMuted,13);
        if(Rules.Economy.Streak.Length>0)
            HudClipText(draw,origin+new float2(0,102),economyWidth,(Math.Abs(economy.Streak)>1?$"{(economy.Streak>0?"Win":"Loss")} streak {Math.Abs(economy.Streak)}":"No streak")+$" +{income.Streak}",HudMuted,13);
        ImGui.SetCursorScreenPos(origin+new float2(0,118));
        if(HudButton("matchInventory",$"Bench {Human.Roster.Bench.Count(unit=>unit is not null)}/9  Items {Human.Items.Count}/{Rules.Loot!.ItemCap}",economyWidth,22,showMatchInventory,13))showMatchInventory=!showMatchInventory;
        var actions=origin+new float2(economyWidth+10,0);
        ImGui.SetCursorScreenPos(actions+new float2(0,Rules.Economy.Xp is null?0:40));
        RefreshEnabled=Human.Shop.FreeRolls>0||economy.Gold>=Rules.RerollCost;
        ImGui.BeginDisabled(!RefreshEnabled);
        if(HudButton("reroll","Reroll [D]\n"+(Human.Shop.FreeRolls>0?"Free x"+Human.Shop.FreeRolls:Rules.RerollCost+" gold"),controlsWidth,52,font:14))Act("reroll");
        ImGui.EndDisabled();
        ImGui.SetCursorScreenPos(actions+new float2(0,Rules.Economy.Xp is null?58:98));
        if(HudButton("lock",Human.Shop.Locked?"Locked":"Lock shop",controlsWidth,30,Human.Shop.Locked,14))Act("lock");
        BuyXpEnabled=false;
        if(Rules.Economy.Xp is {} levelXp)
        {
            BuyXpEnabled=economy.Gold>=levelXp.BuyCost&&economy.Level<levelXp.MaxLevel;
            ImGui.SetCursorScreenPos(actions);ImGui.BeginDisabled(!BuyXpEnabled);
            if(HudButton("xp",$"Buy XP / {levelXp.BuyCost}",controlsWidth,34,font:14))Act("buyXp");
            ImGui.EndDisabled();
            if(ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ResultTooltip(economy.Level>=levelXp.MaxLevel?"Max level":$"Buy {levelXp.BuyXp} XP for {levelXp.BuyCost} gold. +{levelXp.PerRound} XP each round.");
        }
        ImGui.SetCursorScreenPos(actions+new float2(controlsWidth+10,0));
    }

    private void DrawMatchInventory()
    {
        // Returned merge items can exceed the normal shelf capacity. Surface the
        // drawer automatically so none of those items become inaccessible.
        if(Human.Items.Count>Rules.Loot!.ItemCap)showMatchInventory=true;
        if(!showMatchInventory)return;
        float width=Math.Min(720,size.X-32),height=Math.Min(240,Height*.42f);
        ImGui.SetNextWindowFocus();
        Window("Inventory",(size.X-width)/2,Top+8,width,height,ImGuiWindowFlags.NoTitleBar);
        MatchInventoryBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        HudText("ITEM INVENTORY",18,HudGold);ImGui.SameLine();
        if(HudButton("closeMatchInventory","Close",70,28)){showMatchInventory=false;ImGui.End();return;}
        HudText("Drag an item onto a Digimon, or select an item and its holder.",14,HudMuted);
        ImGui.BeginChild("Inventory items"u8,new(0,0));
        MatchInventoryBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        float itemWidth=(ImGui.GetContentRegionAvail().X-16)/3;
        ImGui.BeginDisabled(Session!.State.Over||!Human.Alive);
        for(int i=0;i<Human.Items.Count;i++)
        {
            if(i%3!=0)ImGui.SameLine();
            if(ItemButton(i,itemWidth)&&!itemDragMoved)
            {
                if(selectedItem is {} old&&old!=i){Act("swapItems",new{a=old,b=i});selectedItem=null;}
                else{selectedItem=selectedItem==i?null:i;selectedUnit=null;}
            }
            ItemDragSource(i);if(!itemDragMoved)Tooltip(Human.Items[i]);
        }
        if(selectedItem is {} item&&Session.State.Duos&&HudButton("sendItem","Send selected item",190,34)){Act("portal",new{item});selectedItem=null;}
        ImGui.EndDisabled();ImGui.EndChild();ImGui.End();
    }
}
