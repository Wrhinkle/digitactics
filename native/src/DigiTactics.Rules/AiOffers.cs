using System.Text.Json;

namespace DigiTactics.Rules;

public sealed record OfferAiDefinition(int Tier, string[] Tags, string? Tamer = null);

/// <summary>Original offer ranking with one shared RNG draw per option, including stable ties.</summary>
public sealed partial class AiOffers(MatchState state)
{
    private readonly PlanningAi ai = new(state.Rules);
    private readonly Dictionary<string, ItemDefinition> items = state.Rules.Items.ToDictionary(i => i.Id);
    private readonly Dictionary<string, TraitDefinition> traits = state.Rules.Traits.ToDictionary(t => t.Id);

    public int Select(MatchPlayer player, PlayerOffer offer, int stage, Func<PlayerOffer, string, double> customScore)
    {
        var holders = ai.Holders(player); int best = 0; double bestScore = double.NegativeInfinity;
        var board = player.Roster.Board.Select(e => new CombatEntry(e.Unit.Line, e.Unit.Stage, 0, 4, Items: e.Unit.Items, BonusTraits: e.Unit.BonusTraits)).ToArray();
        var counts = TraitRules.Count(state.Rules, board);
        var augmentedCounts = TraitRules.Count(state.Rules, board, player.Augments.ToArray());
        for (int i = 0; i < offer.Options.Length; i++)
        {
            string id = offer.Options[i]; double score = state.Random.Next();
            if (offer.Kind is "armory" or "itemTier")
            {
                items.TryGetValue(id, out var item);
                foreach (var holder in holders)
                {
                    var preferred = state.Rules.ItemRoles!.GetValueOrDefault(holder.Kind) ?? [];
                    int index = Array.IndexOf(preferred, id);
                    if (index >= 0) score += 10 - index * .5;
                    if (item?.Component == true)
                        score += preferred.Count(p => items.TryGetValue(p, out var recipe) && recipe.Ingredients is { } ingredients && ingredients.Contains(id) && ingredients.Any(c => c != id && player.Items.Contains(c))) * 2;
                }
                if (item?.Consumable == true) score -= 3;
                if (item?.Emblem is { } emblem)
                {
                    int count = counts.GetValueOrDefault(emblem);
                    score += traits[emblem].Breakpoints.Contains(count + 1) ? 9 : count != 0 ? 5 : 1;
                }
                if (item is not null) score += ItemBonus(player, id, stage, counts);
            }
            else if (offer.Kind is "augment" or "power")
            {
                var definition = state.Rules.OfferAi!.GetValueOrDefault(offer.Kind + ":" + id);
                var tags = definition?.Tags ?? [];
                score += tags.Contains("econ") ? stage <= 2 ? 3 : .5 : tags.Contains("items") ? 2 : stage >= 3 ? 3 : 1.5;
                if (offer.Kind == "augment" && definition is not null) score += AugmentBonus(player, id, counts, augmentedCounts);
                score += (definition?.Tier ?? 1) * .1;
            }
            else score += customScore(offer, id);
            if (score > bestScore) { bestScore = score; best = i; }
        }
        return best;
    }

    private double ItemBonus(MatchPlayer player, string id, int stage, IReadOnlyDictionary<string, int> counts) => id switch
    {
        "ul_g_riches" => stage <= 3 ? 6 : 2,
        "ul_g_silver" => player.Economy.Streak < 0 ? 6 : 3,
        "ul_g_aegis" => player.Health < state.Rules.Economy!.StartHp / 2.0 ? 8 : 3,
        "ul_g_stone" => counts.GetValueOrDefault("SUMMONER") >= 2 ? 8 : counts.GetValueOrDefault("SHAMAN") >= 2 ? 6 : 1,
        "ul_g_vicious" => 4, "ul_g_ristul" => 3,
        "ul_g_assault" => counts.GetValueOrDefault("JUNGLE") >= 2 ? 8 : 0,
        "ul_g_chivalry" => counts.GetValueOrDefault("KNIGHT") >= 2 ? 8 : 0,
        "ul_g_recruiter" => 5, "ul_g_smuggler" => stage <= 6 ? 5 : 1,
        "ul_g_roster" => 3, "ul_g_criminal" => 2, _ => 0
    };

    private double AugmentBonus(MatchPlayer player, string id, IReadOnlyDictionary<string, int> counts, IReadOnlyDictionary<string, int> augmented)
    {
        string? trait = id switch
        {
            "giz_a_runic" or "giz_a_spellblade" => "ARC", "giz_a_overload" or "giz_a_injection" => "XA",
            "giz_a_sosmall" => "PET", "giz_a_cutthroat" or "giz_a_smoke" => "ASN", "giz_a_shrug" => "BRU",
            "giz_a_stopwatch" => "CLOCK", "giz_a_reserves" => "PIR", "giz_a_junkyard" => "JUNK", _ => null
        };
        if (trait is not null) { int n = augmented.GetValueOrDefault(trait); return n >= 2 ? 3 : n != 0 ? .5 : -3; }
        bool HasOrigin(bool multiple) => counts.Any(c => traits.TryGetValue(c.Key, out var t) && t.Kind == "origin" && (!multiple || t.Breakpoints.Length > 1) && c.Value >= t.Breakpoints[0]);
        switch (id)
        {
            case "crest": return counts.Any(c => traits[c.Key].Kind == "family" && c.Value >= 2) ? 3 : 0;
            case "gal_a_stars": return HasOrigin(false) ? 3 : 0;
            case "col_a_arena": return HasOrigin(true) ? 3 : 0;
            case "fat_bond": case "fat_twin": case "fat_soulmates":
                return player.Roster.Entries.Any(e => e.Unit.Mods is { } mods && mods.TryGetProperty("partner", out var partner) && partner.ValueKind is not (JsonValueKind.Null or JsonValueKind.False or JsonValueKind.Undefined) && (partner.ValueKind != JsonValueKind.Number || partner.GetDouble() != 0) && (partner.ValueKind != JsonValueKind.String || partner.GetString() != "")) ? 3 : -1;
            case "arc_a_shimmer": case "arc_a_market":
                int syn = player.Roster.Entries.Count(e => TraitRules.UnitTraits(new(e.Unit.Line, e.Unit.Stage, 0, 4, Items: e.Unit.Items), state.Rules).Contains("SYN"));
                return id == "arc_a_shimmer" ? syn > 0 ? 3 : -1 : syn >= 2 ? 3 : -2;
            default: return 0;
        }
    }
}
