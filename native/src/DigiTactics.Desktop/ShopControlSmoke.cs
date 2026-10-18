using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed class ShopControlSmoke
{
    private int step, gold, totalXp;
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
                return true;
        }
        return false;
    }
}
