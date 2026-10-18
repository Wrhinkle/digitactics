using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal bool RefreshEnabled { get; private set; }
    internal bool BuyXpEnabled { get; private set; }

    private void ShopControls()
    {
        var economy = Human.Economy;
        RefreshEnabled = Human.Shop.FreeRolls > 0 || economy.Gold >= Rules.RerollCost;
        ImGui.BeginDisabled(!RefreshEnabled);
        if (HudButton("reroll", $"Refresh  {(Human.Shop.FreeRolls > 0 ? "Free x" + Human.Shop.FreeRolls : Rules.RerollCost + "g")}  [D]",180,40)) Act("reroll");
        ImGui.EndDisabled();
        ImGui.SameLine(); if (HudButton("lock", Human.Shop.Locked ? "Locked" : "Lock shop",110,40,Human.Shop.Locked)) Act("lock");
        BuyXpEnabled = false;
        if (Rules.Economy!.Xp is { } xp)
        {
            BuyXpEnabled = economy.Gold >= xp.BuyCost && economy.Level < xp.MaxLevel;
            ImGui.SameLine(); ImGui.BeginDisabled(!BuyXpEnabled);
            if (HudButton("xp", $"Buy XP  {xp.BuyCost}g  [F]",160,40)) Act("buyXp");
            ImGui.EndDisabled();
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.BeginTooltip();
                UiText.Wrapped(economy.Level >= xp.MaxLevel ? "Max level" : $"Buy {xp.BuyXp} XP for {xp.BuyCost} gold. +{xp.PerRound} XP each round.");
                ImGui.EndTooltip();
            }
            bool maxed = economy.Level >= xp.MaxLevel;
            int need = maxed ? 0 : xp.Table[economy.Level];
            ImGui.SameLine();
            var position = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(180,40));
            var draw = ImGui.GetWindowDrawList();
            draw.AddRectFilled(new(position.X,position.Y+32),new(position.X+172,position.Y+38),new ImColor8(40,50,65),3);
            float fraction = maxed ? 1 : Math.Clamp((float)economy.Xp / Math.Max(1,need),0,1);
            draw.AddRectFilled(new(position.X,position.Y+32),new(position.X+172*fraction,position.Y+38),new ImColor8(79,179,255),3);
            HudClipText(draw,position,172,maxed ? $"Level {economy.Level}  MAX" : $"Level {economy.Level}  {economy.Xp}/{need} XP",HudInk);
        }
        else { ImGui.SameLine(); HudText($"Level {economy.Level}"); }
        ImGui.SameLine();
        var at=ImGui.GetCursorScreenPos();float remaining=ImGui.GetContentRegionAvail().X;ImGui.Dummy(new(Math.Max(1,remaining),40));
        var list=ImGui.GetWindowDrawList();
        HudClipText(list,at,remaining,$"{economy.Gold} gold",HudGold,22);
        HudClipText(list,at+new float2(0,23),remaining,$"Bench {Human.Roster.Bench.Count(unit => unit is not null)}/9   Items {Human.Items.Count}/{Rules.Loot!.ItemCap}",HudMuted);
    }
}
