using DigiTactics.Storage;

namespace DigiTactics.Presentation;

public sealed class FarmProgress(NativeProfileStore profile)
{
    private double elapsed = 30;
    private readonly Queue<System.Text.Json.Nodes.JsonObject> events = new();
    public Func<string, (double X, double Z)?>? ResidentPosition { get; set; }
    public System.Text.Json.Nodes.JsonObject[] DrainEvents()
    {
        var result = events.ToArray(); events.Clear(); return result;
    }
    private void Publish(System.Text.Json.Nodes.JsonArray committed)
    {
        foreach (var entry in committed) { events.Enqueue((System.Text.Json.Nodes.JsonObject)entry!.DeepClone()); if (events.Count > 200) events.Dequeue(); }
    }
    private string PositionPiles(string save, System.Text.Json.Nodes.JsonArray pending)
    {
        if (ResidentPosition is null || !pending.Any(x => x?["type"]?.GetValue<string>() == "poop")) return save;
        var state = System.Text.Json.Nodes.JsonNode.Parse(save)!;
        foreach (var entry in pending)
        {
            if (entry?["type"]?.GetValue<string>() != "poop") continue;
            string id = entry["id"]!.GetValue<string>();
            if (ResidentPosition(id) is not { } position || !double.IsFinite(position.X) || !double.IsFinite(position.Z)) continue;
            var pile = state["pets"]?[id]?["poop"]?.AsArray().LastOrDefault();
            if (pile is null) continue;
            pile["x"] = Math.Round(position.X + .35, 2, MidpointRounding.AwayFromZero);
            pile["z"] = Math.Round(position.Z + .2, 2, MidpointRounding.AwayFromZero);
        }
        return state.ToJsonString();
    }
    public string? Error { get; private set; }
    public string? Message { get; private set; }
    public System.Text.Json.Nodes.JsonObject? LastResult { get; private set; }
    public string? Save => profile.Storage.GetValueOrDefault("dt.farm.v1");
    public bool CanTrain(long timestamp, string id)
    {
        try
        {
            if (!Refresh(timestamp)) return false;
            Message = FarmCare.TrainingRefusal(System.Text.Json.Nodes.JsonNode.Parse(Save!)!, profile.Storage.GetValueOrDefault("dt.legendevo.v1"), id);
            return Message is null;
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or KeyNotFoundException)
        { Error = "Training is unavailable: " + error.Message; return false; }
    }
    public void Update(double delta, long timestamp)
    {
        if (!profile.Exists) return;
        elapsed += delta;
        if (elapsed < 30) return;
        elapsed = 0; Refresh(timestamp);
    }
    private bool Commit(Dictionary<string, string> changes)
    {
        var changed = changes.Where(x => profile.Storage.GetValueOrDefault(x.Key) != x.Value).ToDictionary();
        bool saved = changed.Count == 0 || profile.UpdateStorage(changed);
        Error = saved ? null : profile.Error; return saved;
    }
    public bool Refresh(long timestamp)
    {
        try
        {
            var result = FarmSimulation.Advance(Save, profile.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile.Storage.GetValueOrDefault("dt.legendevo.v1"), timestamp);
            if (!Commit(new() { ["dt.farm.v1"] = PositionPiles(result.Save, result.Events) })) return false;
            Publish(result.Events); return true;
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or KeyNotFoundException)
        { Error = "Farm progress could not be updated: " + error.Message; return false; }
    }
    public bool Act(long timestamp, FarmCareAction action, string? id = null, string? option = null, int pile = 0, bool? lights = null, double score = 0)
    {
        try
        {
            var result = FarmCare.Apply(Save, profile.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile.Storage.GetValueOrDefault("dt.legendevo.v1"), timestamp, action, id, option, pile, lights, score);
            var changes = new Dictionary<string, string> { ["dt.farm.v1"] = PositionPiles(result.Farm, result.Events) };
            if (result.Wallet is not null) changes["dt.cosmetics.v1"] = result.Wallet;
            if (result.Legends is not null) changes["dt.legendevo.v1"] = result.Legends;
            if (!Commit(changes)) return false;
            Publish(result.Events);
            LastResult = result.Result;
            Message = result.Result["msg"]!.GetValue<string>();
            return result.Result["ok"]!.GetValue<bool>();
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or KeyNotFoundException)
        { Error = "Farm action could not be saved: " + error.Message; return false; }
    }
}
