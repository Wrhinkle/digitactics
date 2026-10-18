using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record ClientPreferences(bool Motion = true, bool UiSfx = true, bool Social = true)
{
    public static ClientPreferences Read(string? json)
    {
        var client = Parse(json);
        var prefs = client["prefs"] as JsonObject;
        return new(Value(prefs, "motion"), Value(prefs, "uiSfx"), Value(prefs, "social"));
    }
    // JavaScript uses truthiness after merging defaults, including older imported values.
    private static bool Value(JsonObject? prefs, string key)
    {
        if (prefs is null || !prefs.TryGetPropertyValue(key, out var value)) return true;
        if (value is null) return false;
        if (value is JsonValue primitive)
        {
            if (primitive.TryGetValue<bool>(out bool boolean)) return boolean;
            if (primitive.TryGetValue<double>(out double number)) return number != 0;
            if (primitive.TryGetValue<string>(out var text)) return text.Length != 0;
        }
        return true;
    }
    private static JsonObject Parse(string? json) => json is null ? new() : JsonNode.Parse(json) as JsonObject ?? throw new JsonException("Client settings must be an object.");
    public string Merge(string? json)
    {
        var client = Parse(json);
        if (client["prefs"] is not null && client["prefs"] is not JsonObject)
            throw new JsonException("Client preferences must be an object.");
        var prefs = client["prefs"] as JsonObject ?? new JsonObject();
        if (client["prefs"] is null) client["prefs"] = prefs;
        prefs["motion"] = Motion; prefs["uiSfx"] = UiSfx; prefs["social"] = Social;
        return client.ToJsonString();
    }
}

/// <summary>Preferences remain in the original portable client record. Each edit merges the
/// latest profile so match history, profile art and future fields are never replaced.</summary>
public sealed class ClientPreferencesStore(NativeProfileStore profile)
{
    private string? observed;
    private bool loaded;
    public ClientPreferences Settings { get; private set; } = new();
    public string? Error { get; private set; }
    public void Refresh()
    {
        string? current = profile.Storage.GetValueOrDefault("dt.client.v1");
        if (loaded && current == observed) return;
        loaded = true; observed = current;
        try { Settings = ClientPreferences.Read(current); Error = null; }
        catch (Exception error) when (error is JsonException or InvalidOperationException or FormatException)
        { Settings = new(); Error = "Interface preferences could not be loaded. Your saved data was preserved: " + error.Message; }
    }
    public bool Save(ClientPreferences settings)
    {
        Refresh();
        try
        {
            string next = settings.Merge(profile.Storage.GetValueOrDefault("dt.client.v1"));
            if (!profile.UpdateStorage(new Dictionary<string,string> { ["dt.client.v1"] = next })) { Error = profile.Error; return false; }
            observed = next; loaded = true; Settings = settings; Error = null; return true;
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or FormatException)
        { Error = "Interface preferences could not be saved. Your saved data was preserved: " + error.Message; return false; }
    }
}
