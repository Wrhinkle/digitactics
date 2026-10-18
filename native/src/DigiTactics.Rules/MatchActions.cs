using System.Text.Json;

namespace DigiTactics.Rules;

public abstract record MatchActionResult
{
    public sealed record Flag(bool Value) : MatchActionResult;
    public sealed record Code(string Value) : MatchActionResult;
    public sealed record Evolutions(IReadOnlyList<PlanningUnit> Units) : MatchActionResult;
    public sealed record Orb(LootOrb Value) : MatchActionResult;
    public sealed record Transfer(string Kind, IReadOnlyList<PlanningUnit> Units) : MatchActionResult;
}

/// <summary>Shared player-command boundary for local input and authorized multiplayer commands.</summary>
public sealed class MatchActions(MatchPlanningSession session)
{
    private readonly MatchPlacement placement = new(session.State);
    private readonly MatchLoot loot = new(session.State);
    private readonly FactionWarsPlanning? faction = session.State.Rules.Id.StartsWith("faction-wars/", StringComparison.Ordinal) ? new(session.State) : null;

    private static JsonElement Property(JsonElement args, string name) => args.ValueKind == JsonValueKind.Object && args.TryGetProperty(name, out var value) ? value : default;
    private static int Index(JsonElement args, string name)
    {
        var value = Property(args, name);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out double number) ||
            !double.IsFinite(number) || number < 0 || number > int.MaxValue || number != Math.Truncate(number)) return -1;
        return (int)number;
    }
    private static UnitSpot? Spot(JsonElement args, string name)
    {
        var value = Property(args, name); var type = Property(value, "type");
        if (type.ValueKind != JsonValueKind.String) return null;
        return type.GetString() switch
        {
            "board" => UnitSpot.Board(Index(value, "c"), Index(value, "r")),
            "bench" => UnitSpot.Bench(Index(value, "i")),
            _ => null
        };
    }
    private static void Rename(MatchPlayer player, JsonElement args)
    {
        var value = Property(args, "name");
        // Names are text in the native command protocol; absent/empty text retains the seat name.
        string name = value.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(value.GetString()) ? value.GetString()! : player.Name;
        player.Name = name[..Math.Min(16, name.Length)];
    }

    public MatchActionResult? Apply(int playerId, string action, JsonElement args = default)
    {
        var state = session.State;
        if ((uint)playerId >= state.Players.Count) return null;
        var player = state.Players[playerId];
        var phase = session.Rounds.Phase;
        if (action is "claim" or "release")
        {
            player.IsHuman = action == "claim"; Rename(player, args);
            if (action == "release" && session.Units.Carousel.Turn == player) session.Units.Carousel.Step(phase);
            return new MatchActionResult.Flag(true);
        }
        if (!player.Alive || state.Over) return null;
        switch (action)
        {
            case "buy":
                var evolved = session.Units.Shop.BuyWithEvolutions(player, Index(args, "slot"), phase);
                return evolved is null ? null : new MatchActionResult.Evolutions(evolved);
            case "sell": return new MatchActionResult.Flag(session.Units.Shop.Sell(player, Index(args, "uid"), phase));
            case "move":
                if (Spot(args, "from") is not { } from || Spot(args, "to") is not { } to) return new MatchActionResult.Flag(false);
                return placement.Move(player, from, to, phase, session.Rounds.Position.Stage) switch
                {
                    MoveResult.Capacity => new MatchActionResult.Code("cap"),
                    MoveResult.Moved => new MatchActionResult.Flag(true),
                    _ => new MatchActionResult.Flag(false)
                };
            case "reroll": return new MatchActionResult.Flag(player.Shop.Reroll());
            case "lock": return new MatchActionResult.Flag(player.Shop.Locked = !player.Shop.Locked);
            case "equip": return new MatchActionResult.Code(session.Units.Items.Equip(player, Index(args, "idx"), Index(args, "uid"), phase));
            case "swapItems": return new MatchActionResult.Flag(session.Units.Items.Swap(player, Index(args, "a"), Index(args, "b")));
            case "buyXp": return new MatchActionResult.Flag(phase != MatchPhase.Combat && player.Economy.BuyXp());
            case "offer":
                string? choice = phase == MatchPhase.Planning ? session.Offers.Choose(player, Index(args, "i"), false) : null;
                return choice is null ? null : new MatchActionResult.Code(choice);
            case "offerReroll": return new MatchActionResult.Flag(phase == MatchPhase.Planning && session.Offers.Reroll(player, session.Rounds.Position));
            case "carousel":
                int index = Property(args, "i").ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                    ? session.Units.Carousel.Options.Select((o, i) => (o, i)).FirstOrDefault(e => string.IsNullOrEmpty(e.o.PickedBy), (null!, -1)).i : Index(args, "i");
                return new MatchActionResult.Flag(session.PickCarousel(player, index));
            case "orb":
                var orb = loot.Collect(player, Index(args, "id"), phase == MatchPhase.Combat && player.IsHuman);
                return orb is null ? null : new MatchActionResult.Orb(orb);
            case "portal":
                if (!state.Duos) return null;
                var request = Property(args, "uid").ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                    ? new PortalRequest(ItemIndex: Index(args, "item")) : new PortalRequest(Uid: Index(args, "uid"));
                var result = session.Units.Portal.Send(player, request, phase, session.Rounds.RoundType);
                return result.Status is "unit" or "item" ? new MatchActionResult.Transfer(result.Status, result.Evolved) : new MatchActionResult.Code(result.Status);
            case "wisp" when faction is not null: return new MatchActionResult.Code(faction.Act(player, phase));
            default: return null;
        }
    }
}
