using DigiTactics.Storage;

namespace DigiTactics.Presentation;

public sealed class FarmProgress(NativeProfileStore profile)
{
    private double elapsed = 30;
    public string? Error { get; private set; }
    public string? Message { get; private set; }
    public string? Save => profile.Storage.GetValueOrDefault("dt.farm.v1");
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
            return Commit(new() { ["dt.farm.v1"] = result.Save });
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or KeyNotFoundException)
        { Error = "Farm progress could not be updated: " + error.Message; return false; }
    }
    public bool Act(long timestamp, FarmCareAction action, string? id = null, string? option = null, int pile = 0, bool? lights = null, double score = 0)
    {
        try
        {
            var result = FarmCare.Apply(Save, profile.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile.Storage.GetValueOrDefault("dt.legendevo.v1"), timestamp, action, id, option, pile, lights, score);
            var changes = new Dictionary<string, string> { ["dt.farm.v1"] = result.Farm };
            if (result.Wallet is not null) changes["dt.cosmetics.v1"] = result.Wallet;
            if (result.Legends is not null) changes["dt.legendevo.v1"] = result.Legends;
            if (!Commit(changes)) return false;
            Message = result.Result["msg"]!.GetValue<string>();
            return result.Result["ok"]!.GetValue<bool>();
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or KeyNotFoundException)
        { Error = "Farm action could not be saved: " + error.Message; return false; }
    }
}
