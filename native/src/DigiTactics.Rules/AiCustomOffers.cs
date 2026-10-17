using System.Globalization;

namespace DigiTactics.Rules;

public sealed partial class AiOffers
{
    public int Select(MatchPlayer player, PlayerOffer offer, int stage) => Select(player, offer, stage, (o, id) => CustomScore(player, o.Kind, id, stage));

    private double CustomScore(MatchPlayer player, string kind, string id, int stage)
    {
        var units = player.Roster.Entries.Select(e => e.Unit).ToArray();
        var counts = ai.Counts(player.Roster.Board.Select(e => e.Unit));
        var holders = ai.Holders(player);
        string[] Preferred(string role) => state.Rules.ItemRoles!.GetValueOrDefault(role) ?? [];
        switch (kind)
        {
            case "upgrade":
                if (!state.Rules.OfferAi!.TryGetValue("augment:" + id, out var augment)) return 0;
                var board = player.Roster.Board.Select(e => new CombatEntry(e.Unit.Line, e.Unit.Stage, 0, 4, Items: e.Unit.Items, BonusTraits: e.Unit.BonusTraits));
                return (augment.Tags.Contains("econ") ? stage <= 2 ? 3 : .5 : augment.Tags.Contains("items") ? 2 : stage >= 3 ? 3 : 1.5)
                    + AugmentBonus(player, id, counts, TraitRules.Count(state.Rules, board, player.Augments.ToArray())) + augment.Tier * .1;
            case "rec_bless": case "rec_radiant":
                string? baseId = state.Rules.Loot!.Mode!.BaseOf!.GetValueOrDefault(id);
                double radiant = baseId is not null && units.SelectMany(u => u.Items).Concat(player.Items).Contains(baseId) ? 6 : 0;
                foreach (var holder in holders)
                {
                    if (baseId is not null && holder.Unit.Items.Contains(baseId)) radiant += 8;
                    int index = Array.IndexOf(Preferred(holder.Kind), id); if (index >= 0) radiant += 4 - index * .1;
                }
                return radiant;
            case "rec_tome":
                string emblem = items[id].Emblem!; int count = counts.GetValueOrDefault(emblem);
                return traits[emblem].Breakpoints.Contains(count + 1) ? 9 : count != 0 ? 5 : 1;
            case "drg_treasure":
                var fields = id.Split(':'); string value = fields.Length > 1 ? fields[1].Split('.').FirstOrDefault(s => s.Length > 0) ?? "" : "";
                switch (fields[0])
                {
                    case "gold": return (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var gold) ? gold : 0) * .5 + (stage <= 5 ? 1 : 0);
                    case "comp": case "unit": return 6;
                    case "item": return 7;
                    case "dup": return 5;
                    case "rad": return 9;
                    case "emblem":
                        var augmented = TraitRules.Count(state.Rules, player.Roster.Board.Select(e => new CombatEntry(e.Unit.Line, e.Unit.Stage, 0, 4, Items: e.Unit.Items, BonusTraits: e.Unit.BonusTraits)), player.Augments.ToArray());
                        return items.TryGetValue(value, out var item) && item.Emblem is { } trait && augmented.GetValueOrDefault(trait) != 0 ? 7.5 : 3;
                    default: return 6.5;
                }
            case "col_core": case "col_form": return state.Random.Next();
            case "col_gambit": return (id.StartsWith("double", StringComparison.Ordinal) ? player.Health > 10 && player.Economy.Streak < 0 ? 2 : -1 : 1) + state.Random.Next();
            case "ul_crew": return 0;
            case "ul_tamer":
                return state.Rules.OfferAi!.GetValueOrDefault("power:" + id)?.Tamer switch
                {
                    "anessa" => counts.GetValueOrDefault("DEMON") * .6 + counts.GetValueOrDefault("SUMMONER") * .3,
                    "hob" => counts.GetValueOrDefault("WIZARD") * .3 + counts.GetValueOrDefault("JUNGLE") * .4,
                    "jules" => counts.GetValueOrDefault("KNIGHT") * .3 + counts.GetValueOrDefault("METAL") * .3,
                    "enno" => counts.GetValueOrDefault("TOXIC") * .6 + counts.GetValueOrDefault("HOLY") * .3,
                    _ => 0
                };
            case "ac_relic":
                return (id switch { "ac_r_treasure" or "ac_r_eggnest" => 3, "ac_r_forge" or "ac_r_reaper" => 3.5,
                    "ac_r_vault" or "ac_r_card" => 2, "ac_r_charm" => 1.5, "ac_r_recruiter" or "ac_r_grace" => 2.5,
                    "ac_r_key" => 4, "ac_r_discount" => .5, _ => 1 }) + (id == "ac_r_vault" && player.Economy.Gold >= 40 ? 2 : 0) + (id == "ac_r_key" && stage >= 5 ? 1 : 0);
            case "ac_loot":
                if (!items.TryGetValue(id, out var loot)) return 0;
                if (id == "ac_digiegg") return new AutoChessPlanning(state).EggTarget(player) is not null ? 9 : 3;
                double score = loot.Component ? loot.Tier != 0 ? loot.Tier : 1 : 4 + loot.AiRank * .5;
                foreach (var holder in holders)
                {
                    var preferred = Preferred(holder.Kind); int index = Array.IndexOf(preferred, id);
                    if (index >= 0) score += 6 - index * .2;
                    if (loot.Component) score += preferred.Count(p => items[p].Ingredients is { } ingredients && ingredients.Contains(id) && ingredients.Any(c => c != id && player.Items.Contains(c))) * 2;
                }
                return score;
            case "arcJunk": return id == "spare" ? 1 : 3;
            case "arcMarket":
                var syn = units.Where(u => TraitRules.UnitTraits(new(u.Line, u.Stage, 0, 4, Items: u.Items), state.Rules).Contains("SYN")).ToArray();
                if (id.StartsWith("arc_pass:", StringComparison.Ordinal)) return syn.Length > 0 ? 2 : 8;
                return 4 + syn.Count(u => (items[id].Tags ?? []).Contains(UnitKind(u))) * 1.5;
            case "arcAnomaly":
                var target = new ArcanePlanning(state).AnomalyTarget(player);
                if (target is null) return 0;
                string role = UnitKind(target), tag = state.Rules.AnomalyTags![id];
                return tag == role ? 5 : tag == "dmg" && role != "tank" ? 4 : tag == "any" ? 3 : 0;
            default: return 0;
        }
    }

    private string UnitKind(PlanningUnit unit)
    {
        var hints = state.Rules.Lines.Concat(state.Rules.ExtraLines).First(l => l.Id == unit.Line).Ai!;
        return hints.Carry ? hints.Damage == "ap" ? "ap" : "ad" : "tank";
    }
}
