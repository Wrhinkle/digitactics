namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private readonly bool[] underlordsSummoningStone = new bool[2];
    private void BindUnderlordsGlobals(int team, string[] ids, int? owner = null)
    {
        foreach (string id in ids) {
            var context = Hooks.Context(CombatHookSource.GlobalItem, id, team, owner: owner);
            switch (id) {
                case "ul_g_ristul": Hooks.Add(context, CombatHook.CombatStart, (_, _) => { HealingAmplification[0] -= 0.4; HealingAmplification[1] -= 0.4; }); break;
                case "ul_g_assault": Hooks.Add(context, CombatHook.CombatStart, (c, _) => {
                    foreach (var j in c.Mine) if (j.Traits.Contains("JUNGLE")) foreach (var a in c.Adjacent(j)) if (a.Team == c.Team && a != j) a.AttackSpeed *= 1.25;
                }, 20); break;
                case "ul_g_chivalry": Hooks.Add(context, CombatHook.BeforeDamageTaken, (c, e) => {
                    var u = e.Unit!; if (u.Team != c.Team || u.Traits.Contains("KNIGHT")) return;
                    int adjacent = c.Adjacent(u).Count(a => a.Alive && a.Team == u.Team && a.Traits.Contains("KNIGHT")); if (adjacent > 0) e.Reduction += 0.1 * adjacent;
                }); break;
                // Summoning Stone is read from the merged side by the summon helper, independently of owner.
                case "ul_g_stone": break;
                // These have only offer, planning, economy or settlement behavior.
                case "ul_g_riches": case "ul_g_silver": case "ul_g_aegis": case "ul_g_vicious":
                case "ul_g_recruiter": case "ul_g_smuggler": case "ul_g_roster": case "ul_g_criminal": break;
                default: throw new NotSupportedException($"Underlords global item {id} has not been migrated.");
            }
        }
    }
}
