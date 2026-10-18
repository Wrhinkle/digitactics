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
        if (Button("reroll", $"Refresh ({(Human.Shop.FreeRolls > 0 ? "free x" + Human.Shop.FreeRolls : Rules.RerollCost + "g")})")) Act("reroll");
        ImGui.EndDisabled();
        ImGui.SameLine(); if (Button("lock", Human.Shop.Locked ? "Unlock shop" : "Lock shop", active: Human.Shop.Locked)) Act("lock");
        BuyXpEnabled = false;
        if (Rules.Economy!.Xp is { } xp)
        {
            BuyXpEnabled = economy.Gold >= xp.BuyCost && economy.Level < xp.MaxLevel;
            ImGui.SameLine(); ImGui.BeginDisabled(!BuyXpEnabled);
            if (Button("xp", $"Buy XP ({xp.BuyCost}g)")) Act("buyXp");
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
            var position = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(130,24));
            var draw = ImGui.GetWindowDrawList();
            draw.AddRectFilled(new(position.X,position.Y+20),new(position.X+124,position.Y+24),new ImColor8(40,50,65));
            float fraction = maxed ? 1 : Math.Clamp((float)economy.Xp / Math.Max(1,need),0,1);
            draw.AddRectFilled(new(position.X,position.Y+20),new(position.X+124*fraction,position.Y+24),new ImColor8(80,200,180));
            draw.AddText(position,new ImColor8(235,240,248),maxed ? $"Level {economy.Level} / Max" : $"Level {economy.Level} / XP {economy.Xp}/{need}");
        }
        else { ImGui.SameLine(); ImGui.Text($"Level {economy.Level}"); }
        ImGui.SameLine(); ImGui.Text($"Bench {Human.Roster.Bench.Count(unit => unit is not null)}/9 / Items {Human.Items.Count}/{Rules.Loot!.ItemCap}");
    }
}
