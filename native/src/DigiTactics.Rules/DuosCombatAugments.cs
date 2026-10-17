namespace DigiTactics.Rules;

internal static class DuosCombatAugments
{
    internal static bool TryBind(CombatHookRegistry hooks, CombatHookContext context, double partnerTop)
    {
        switch (context.Id)
        {
            case "duo_ace": hooks.Add(context, CombatHook.CombatStart, (c, _) => {
                double n = Math.Min(10, partnerTop);
                foreach (var u in c.Mine) { u.AttackDamage += u.BaseAttackDamage * 0.03 * n; u.AbilityPower += 3 * n; }
            }); break;
            case "duo_sync": hooks.Add(context, CombatHook.CombatStart, (c, _) => { foreach (var u in c.Mine) u.Equipment.Amp += 0.08; }); break;
            case "duo_tag": hooks.Add(context, CombatHook.AnyDeath, (c, e) => {
                var dead = e.Unit!;
                if (dead.Team != c.Team || dead.Entry.Owner is null || dead.Entry.Owner == c.Owner || c.Owner is null) return;
                foreach (var u in c.Mine.Where(u => u.Alive)) { c.Heal(u, u.MaxHealth * 0.1); c.Buff(u, CombatStat.AttackSpeed, 0.15, 4); }
            }); break;
            case "duo_crest_link": case "duo_wide_portal": case "duo_proud": case "duo_gear": case "duo_interest":
            case "duo_care": case "duo_guard": case "duo_trove": break;
            default: return false;
        }
        return true;
    }
}
