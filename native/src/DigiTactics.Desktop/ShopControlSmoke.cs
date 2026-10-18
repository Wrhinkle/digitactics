using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class ShopControlSmoke
{
    private int step, gold, totalXp, freeRolls;
    private int sold, saleValue;
    public bool Input(MatchScreen screen)
    {
        var rules = screen.Session!.State.Rules; var economy = screen.Session.State.Players[0].Economy;
        var xp = rules.Economy!.Xp!;
        int Earned() => xp.Table.Take(economy.Level).Sum() + economy.Xp;
        switch (step++)
        {
            case 0:
                if (!screen.BuyXpEnabled) throw new InvalidOperationException("XP purchase fixture requires an affordable purchase.");
                gold = economy.Gold; totalXp = Earned();
                var point = screen.Buttons["xp"]; ImGui.GetIO().AddMousePosEvent(point.X,point.Y); ImGui.GetIO().AddMouseButtonEvent(0,true); break;
            case 1: ImGui.GetIO().AddMouseButtonEvent(0,false); break;
            case 2:
                if (economy.Gold != gold - xp.BuyCost || Earned() != totalXp + xp.BuyXp)
                    throw new InvalidOperationException("XP button did not purchase exactly one XP bundle.");
                gold = economy.Gold; totalXp = Earned();
                if (gold < xp.BuyCost) throw new InvalidOperationException("Keyboard XP fixture needs an affordable purchase.");
                ImGui.GetIO().AddKeyEvent(ImGuiKey.F,true); break;
            case 3: ImGui.GetIO().AddKeyEvent(ImGuiKey.F,false); break;
            case 4:
                if (economy.Gold != gold-xp.BuyCost || Earned() != totalXp+xp.BuyXp)
                    throw new InvalidOperationException("F shortcut did not purchase exactly one XP bundle.");
                gold=economy.Gold; freeRolls=screen.Session.State.Players[0].Shop.FreeRolls;
                if (freeRolls == 0 && gold < rules.RerollCost) throw new InvalidOperationException("Keyboard refresh fixture needs an affordable refresh.");
                ImGui.GetIO().AddKeyEvent(ImGuiKey.D,true); break;
            case 5: ImGui.GetIO().AddKeyEvent(ImGuiKey.D,false); break;
            case 6:
                if (economy.Gold != gold-(freeRolls>0?0:rules.RerollCost) || screen.Session.State.Players[0].Shop.FreeRolls != Math.Max(0,freeRolls-1))
                    throw new InvalidOperationException("D shortcut charged the wrong gold or free-roll amount.");
                var entry = screen.Session.State.Players[0].Roster.Entries.First(entry => !entry.Unit.NoSell && !rules.Lines.Concat(rules.ExtraLines).Single(line => line.Id == entry.Unit.Line).NoSell);
                sold = entry.Unit.Id; gold = economy.Gold;
                saleValue = UnitValues.SellValue(rules.Lines.Concat(rules.ExtraLines).Single(line => line.Id == entry.Unit.Line),entry.Unit.Stage);
                var target = screen.Buttons[entry.Spot.IsBoard ? $"hex/{entry.Spot.Column}/{entry.Spot.Row}" : $"bench/{entry.Spot.Index}"];
                ImGui.GetIO().AddMousePosEvent(target.X,target.Y); ImGui.GetIO().AddKeyEvent(ImGuiKey.E,true); break;
            case 7: ImGui.GetIO().AddKeyEvent(ImGuiKey.E,false); break;
            case 8:
                if (screen.Session.State.Players[0].Roster.Entries.Any(entry => entry.Unit.Id == sold) || economy.Gold != gold+saleValue)
                    throw new InvalidOperationException("E shortcut did not sell exactly the hovered owned unit.");
                return true;
        }
        return false;
    }
}
