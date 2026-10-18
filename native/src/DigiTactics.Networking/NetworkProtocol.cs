using System.Globalization;
using System.Text.Json;

namespace DigiTactics.Networking;

/// <summary>Version 9 Steam message contract, ported from js/net-guard.js. No transport or game side effects.</summary>
public static class NetworkProtocol
{
    public const string Version = "9";
    public const int MaximumLog = 32768, Chunk = 150, MaximumWire = 256 * 1024;
    private const double SafeInteger = 9007199254740991;
    private static JsonElement Get(JsonElement v, string key) => v.ValueKind == JsonValueKind.Object && v.TryGetProperty(key, out var p) ? p : default;
    private static bool Object(JsonElement v) => v.ValueKind == JsonValueKind.Object;
    private static bool Null(JsonElement v) => v.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;
    private static bool Boolean(JsonElement v) => v.ValueKind is JsonValueKind.True or JsonValueKind.False;
    private static string? String(JsonElement v) => v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    private static bool Is(JsonElement v, params string[] values) => String(v) is { } s && values.Contains(s);
    private static bool Integer(JsonElement v, double max = SafeInteger) => v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out double n) && double.IsFinite(n) && n >= 0 && n <= max && n <= SafeInteger && Math.Truncate(n) == n;
    private static bool Seat(JsonElement v) => Integer(v, 7);
    private static bool Text(JsonElement v, int max) => String(v) is { } s && s.Length <= max && !s.Any(c => c < 32 || c is '<' or '>');
    private static bool Token(JsonElement v) => String(v) is { Length: >= 1 and <= 80 } s && s.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '@' or '.' or '-');
    private static bool Id(JsonElement v) => String(v) is { Length: >= 1 and <= 20 } s && s[0] is >= '1' and <= '9' && s.All(char.IsAsciiDigit) && ulong.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out _);
    private static bool Keys(JsonElement v, params string[] allowed) => Object(v) && v.EnumerateObject().All(p => allowed.Contains(p.Name));
    private static bool Array(JsonElement v, int max, Func<JsonElement, bool> each) => v.ValueKind == JsonValueKind.Array && v.GetArrayLength() <= max && v.EnumerateArray().All(each);
    private static bool Falsy(JsonElement v) => Null(v) || v.ValueKind == JsonValueKind.False || v.ValueKind == JsonValueKind.String && v.GetString() == "" || v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out double n) && n == 0;
    private static readonly JsonElement Empty = JsonSerializer.SerializeToElement(new { });
    private static bool Spot(JsonElement v) => Is(Get(v, "type"), "board")
        ? Keys(v, "type", "c", "r") && Integer(Get(v, "c"), 6) && Integer(Get(v, "r"), 7) && Get(v, "r").GetDouble() >= 4
        : Is(Get(v, "type"), "bench") && Keys(v, "type", "i") && Integer(Get(v, "i"), 8);

    public static bool Action(string? action, JsonElement args, bool internalAction = false)
    {
        if (args.ValueKind == JsonValueKind.Undefined) args = Empty;
        if (!Object(args)) return false;
        bool One(string key, double max) => Keys(args, key) && Integer(Get(args, key), max);
        return action switch
        {
            "claim" or "release" => internalAction && Keys(args, "name") && Text(Get(args, "name"), 16),
            "ready" or "done" => !internalAction && Keys(args),
            "reroll" or "lock" or "buyXp" or "offerReroll" or "wisp" => Keys(args),
            "buy" => One("slot", 15), "sell" => One("uid", 10000000), "offer" => One("i", 15), "orb" => One("id", 10000000),
            "carousel" => Keys(args, "i") && (Null(Get(args, "i")) || Integer(Get(args, "i"), 31)),
            "move" => Keys(args, "from", "to") && Spot(Get(args, "from")) && Spot(Get(args, "to")),
            "equip" => Keys(args, "idx", "uid") && Integer(Get(args, "idx"), 511) && Integer(Get(args, "uid"), 10000000),
            "swapItems" => Keys(args, "a", "b") && Integer(Get(args, "a"), 511) && Integer(Get(args, "b"), 511),
            "portal" => One("uid", 10000000) || One("item", 511),
            _ => false
        };
    }
    public static bool Entry(JsonElement e)
    {
        var digest = Get(e, "d");
        if (!Object(e) || !Integer(Get(e, "at")) || !Null(digest) && !(String(digest) is { Length: 8 } s && s.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'))) return false;
        if (Is(Get(e, "k"), "a")) { var args = Get(e, "g"); return Seat(Get(e, "s")) && Action(String(Get(e, "a")), Falsy(args) ? Empty : args, true); }
        return Is(Get(e, "k"), "vote") ? Seat(Get(e, "s")) : Is(Get(e, "k"), "start", "end", "next");
    }
    public static bool Look(JsonElement v) => Object(v) && v.EnumerateObject().Count() <= 9 && v.EnumerateObject().All(p =>
        p.Name is "emote" or "sound" ? Array(p.Value, 8, x => Null(x) || Token(x)) :
        p.Name is "legendForm" or "legend" or "board" or "killscreen" or "dance" or "elimsound" && (Null(p.Value) || Token(p.Value)));
    public static bool Presence(JsonElement v) => Keys(v, "name", "look", "lobbySet", "lobbyPace", "lobbyFormat", "at") &&
        (Null(Get(v, "name")) || Text(Get(v, "name"), 32)) && (Null(Get(v, "look")) || Look(Get(v, "look"))) &&
        (Null(Get(v, "lobbySet")) || Token(Get(v, "lobbySet"))) && (Null(Get(v, "lobbyPace")) || Is(Get(v, "lobbyPace"), "hyper", "normal", "chaos")) &&
        (Null(Get(v, "lobbyFormat")) || Is(Get(v, "lobbyFormat"), "solo", "duos")) && (Null(Get(v, "at")) || Integer(Get(v, "at")));
    private static bool People(JsonElement v) => Object(v) && v.EnumerateObject().Count() <= 8 && v.EnumerateObject().All(p => Id(JsonSerializer.SerializeToElement(p.Name)) && Presence(p.Value));
    public static bool Lobby(JsonElement v)
    {
        if (!Object(v) || !Id(Get(v, "host")) || !Text(Get(v, "name"), 64) || !Text(Get(v, "hostName"), 32) || !Is(Get(v, "status"), "lobby", "playing", "over") || !Object(Get(v, "slots"))) return false;
        var ids = new HashSet<string>();
        foreach (var slot in Get(v, "slots").EnumerateObject())
            if (slot.Name.Length != 1 || slot.Name[0] is < '0' or > '7' || !Object(slot.Value) || !Id(Get(slot.Value, "uid")) || !Text(Get(slot.Value, "name"), 16) || !ids.Add(Get(slot.Value, "uid").GetString()!)) return false;
        var banned = Get(v, "banned"); var plan = Get(v, "planTime"); var alive = Get(v, "alive");
        return (Null(banned) || Array(banned, 128, Id) && !banned.EnumerateArray().Any(x => x.GetString() == Get(v, "host").GetString())) &&
            (Null(plan) || Integer(plan, 300) && plan.GetDouble() >= 5) && (Null(Get(v, "setId")) || Token(Get(v, "setId"))) &&
            (Null(Get(v, "round")) || Text(Get(v, "round"), 20)) && (Null(alive) || Array(alive, 8, Boolean) && alive.GetArrayLength() == 8);
    }
    public static bool Init(JsonElement v)
    {
        var plan = Get(v, "planTime"); var slots = Get(v, "slots"); var teams = Get(v, "teams");
        if (!Object(v) || !Integer(Get(v, "seed"), uint.MaxValue) || !Integer(Get(v, "at")) || !Integer(plan, 300) || plan.GetDouble() < 5 || !Token(Get(v, "setId"))) return false;
        if (!Array(slots, 8, s => Object(s) && Text(Get(s, "name"), 16) && Boolean(Get(s, "human"))) || slots.GetArrayLength() != 8) return false;
        if (!Is(Get(v, "pace"), "hyper", "normal") || !Falsy(Get(v, "format")) && !Is(Get(v, "format"), "solo", "duos")) return false;
        return Null(teams) || Array(teams, 4, t => Array(t, 2, Seat) && t.GetArrayLength() == 2) && teams.GetArrayLength() == 4 && teams.EnumerateArray().SelectMany(t => t.EnumerateArray()).Select(t => t.GetDouble()).Distinct().Count() == 8;
    }
    public static bool Wire(JsonElement m)
    {
        if (!Object(m) || !Id(Get(m, "l"))) return false;
        bool Entries() => Integer(Get(m, "start"), MaximumLog) && Array(Get(m, "entries"), Chunk, Entry) && Get(m, "start").GetDouble() + Get(m, "entries").GetArrayLength() <= MaximumLog;
        var v = Get(m, "v");
        return String(Get(m, "t")) switch
        {
            "snapreq" => Integer(Get(m, "rid")),
            "seat" => Integer(Get(m, "rid")) && Seat(Get(m, "i")) && Text(Get(m, "name"), 16),
            "seatres" => Integer(Get(m, "rid")) && Boolean(Get(m, "ok")),
            "unseat" => Seat(Get(m, "i")),
            "pres" or "presup" => Presence(Get(m, "rec")), "presdel" => true,
            "intent" => Seat(Get(m, "s")) && Integer(Get(m, "q")) && Integer(Get(m, "round"), 10000) && Action(String(Get(m, "a")), Get(m, "g")),
            "logreq" => Integer(Get(m, "from"), MaximumLog),
            "ping" => Integer(Get(m, "t0")), "pong" => Integer(Get(m, "t0")) && Integer(Get(m, "now")),
            "snap" => Integer(Get(m, "rid")) && Integer(Get(m, "total"), MaximumLog) && Entries() && Get(m, "start").GetDouble() + Get(m, "entries").GetArrayLength() <= Get(m, "total").GetDouble() && Lobby(Get(m, "lobby")) && People(Get(m, "presence")) && (Null(Get(m, "init")) || Init(Get(m, "init"))) && Integer(Get(m, "now")),
            "lobby" => Null(Get(m, "lobby")) || Lobby(Get(m, "lobby")), "presence" => People(Get(m, "presence")), "init" => Init(Get(m, "init")),
            "log" => Integer(Get(m, "n"), MaximumLog - 1) && Entry(Get(m, "e")), "logs" => Entries(), "head" => Integer(Get(m, "n"), MaximumLog),
            "fx" => Object(v) && Seat(Get(v, "s")) && Is(Get(v, "kind"), "emote", "sound") && Token(Get(v, "id")),
            _ => false
        };
    }
    public static JsonElement? Parse(string? json)
    {
        if (json is null || json.Length > MaximumWire) return null;
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 14 });
            var pending = new Stack<(JsonElement Value, int Depth)>(); pending.Push((document.RootElement, 0)); int count = 0;
            while (pending.TryPop(out var item))
            {
                if (++count > 12000 || item.Depth > 12) return null;
                if (item.Value.ValueKind == JsonValueKind.Number && (!item.Value.TryGetDouble(out var n) || !double.IsFinite(n))) return null;
                if (item.Value.ValueKind == JsonValueKind.String && item.Value.GetString()!.Length > 256) return null;
                if (Object(item.Value))
                    foreach (var p in item.Value.EnumerateObject())
                    {
                        if (p.Name is "__proto__" or "prototype" or "constructor") return null;
                        pending.Push((p.Value, item.Depth + 1));
                    }
                else if (item.Value.ValueKind == JsonValueKind.Array)
                    foreach (var v in item.Value.EnumerateArray()) pending.Push((v, item.Depth + 1));
            }
            return Wire(document.RootElement) ? document.RootElement.Clone() : null;
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException) { return null; }
    }
}
