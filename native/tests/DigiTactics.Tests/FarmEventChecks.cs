using System.Text.Json.Nodes;
using DigiTactics.Storage;
using DigiTactics.Presentation;

internal static class FarmEventChecks
{
    public static int Run()
    {
        int checks = 0;
        void Check(bool value, string message) { checks++; if (!value) throw new InvalidOperationException("Farm events: " + message); }
        var pops = new FarmPops();
        JsonObject Receipt(bool ok, int bits, int data) => new() { ["ok"] = ok, ["reward"] = new JsonObject { ["bits"] = bits, ["data"] = data } };
        pops.Add("tokomon", Receipt(false, 1, 2)); pops.Add("tokomon", Receipt(true, 0, 0));
        Check(pops.Items.Count == 0, "no popup for refused or capped reward");
        pops.Add("tokomon", Receipt(true, 1, 2)); pops.Update(.3, _ => (10, 20));
        Check(pops.Items.Count == 1 && pops.Items[0].Position is null, "reward delay");
        pops.Update(.1, _ => (30, 40));
        Check(pops.Items[0].Position == (30, 40) && pops.Items[0].Text == "+1 Bits  +2 Data", "capture position when delay expires");
        pops.Update(.5, _ => (50, 60));
        Check(pops.Items[0].Position == (30, 40) && pops.Items[0].Opacity == 1 && pops.Items[0].Offset < 0, "rise stays at captured position");
        pops.Update(1, _ => (50, 60)); Check(pops.Items.Count == 0, "popup expiry");
        pops.AddSymbol("tokomon", FarmSymbol.Heart, (10, 20));
        Check(pops.Items[0].Age == 0 && pops.Items[0].Position == (10, 20), "symbol captures position immediately");
        pops.Clear();
        foreach (var (action, option, receipt, asleep, expected) in new (FarmCareAction, string?, string, bool, FarmSymbol?)[] {
            (FarmCareAction.Feed, "meat", "{\"ok\":true}", false, FarmSymbol.Meat),
            (FarmCareAction.Feed, "carrot", "{\"ok\":true}", false, FarmSymbol.Carrot),
            (FarmCareAction.Feed, "bigmeat", "{\"ok\":true}", false, FarmSymbol.BigMeat),
            (FarmCareAction.Feed, "sirloin", "{\"ok\":true}", false, FarmSymbol.Sirloin),
            (FarmCareAction.Feed, "cake", "{\"ok\":true}", false, FarmSymbol.Cake),
            (FarmCareAction.Feed, null, "{\"ok\":false,\"refused\":true}", false, FarmSymbol.Refused),
            (FarmCareAction.Feed, null, "{\"ok\":false}", false, null),
            (FarmCareAction.Cure, null, "{\"ok\":true}", false, FarmSymbol.Bandage),
            (FarmCareAction.Cure, "medicine", "{\"ok\":true}", false, FarmSymbol.Medicine),
            (FarmCareAction.Praise, null, "{\"ok\":true,\"spoiled\":true}", false, FarmSymbol.Spoiled),
            (FarmCareAction.Praise, null, "{\"ok\":true}", false, FarmSymbol.Heart),
            (FarmCareAction.Scold, null, "{\"ok\":true,\"unfair\":true}", false, FarmSymbol.Tear),
            (FarmCareAction.Scold, null, "{\"ok\":true}", false, FarmSymbol.Exclaim),
            (FarmCareAction.Pet, null, "{\"ok\":true,\"small\":true}", false, null),
            (FarmCareAction.Pet, null, "{\"ok\":true,\"small\":true}", true, FarmSymbol.Sleep),
            (FarmCareAction.Pet, null, "{\"ok\":true}", false, FarmSymbol.Heart)
        }) Check(FarmFeedback.Symbol(action, option, JsonNode.Parse(receipt)!.AsObject(), asleep) == expected, "symbol " + action + option + receipt);
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-farm-events-" + Guid.NewGuid().ToString("N"));
        try
        {
            long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var state = JsonNode.Parse(FarmSimulation.Advance(null, null, null, time).Save)!;
            state["pets"]!["tokomon"]!["poopClock"] = 179;
            state["pets"]!["tsunomon"]!["poopClock"] = 179;
            state["pets"]!["tsunomon"]!["disc"] = 65;
            var profile = new NativeProfileStore(directory);
            Check(profile.UpdateStorage(new Dictionary<string, string> { ["dt.farm.v1"] = state.ToJsonString() }), "seed");
            var progress = new FarmProgress(profile) { ResidentPosition = id => id == "tokomon" ? (1.12, 2.34) : null };
            using (var guard = new FileStream(Path.Combine(directory, "profile.json.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                Check(!progress.Refresh(time + 60000), "locked tick fails");
                Check(progress.DrainEvents().Length == 0, "failed tick publishes no events");
            }
            Check(progress.Refresh(time + 60000), "retry tick");
            var events = progress.DrainEvents();
            Check(events.Count(x => x["type"]!.GetValue<string>() == "poop") == 1 && events.Count(x => x["type"]!.GetValue<string>() == "toilet") == 1, "committed tick delivered");
            Check(progress.DrainEvents().Length == 0, "drain once");
            var reopened = new NativeProfileStore(directory);
            var saved = JsonNode.Parse(reopened.Storage["dt.farm.v1"])!;
            var pile = saved["pets"]!["tokomon"]!["poop"]![0]!;
            Check(Math.Abs(pile["x"]!.GetValue<double>() - 1.47) < 1e-9 && Math.Abs(pile["z"]!.GetValue<double>() - 2.54) < 1e-9, "live coordinates survive reload");
            Check(progress.Refresh(time + 60000) && progress.DrainEvents().Length == 0, "same timestamp never replays events");
            // Care can advance simulation too; its events must follow the same commit contract.
            saved["pets"]!["tokomon"]!["poopClock"] = 179;
            Check(profile.UpdateStorage(new Dictionary<string, string> { ["dt.farm.v1"] = saved.ToJsonString() }), "seed next tick");
            progress.ResidentPosition = null;
            var expected = FarmCare.Apply(saved.ToJsonString(), null, null, time + 120000, FarmCareAction.Pet, "tokomon");
            Check(progress.Act(time + 120000, FarmCareAction.Pet, "tokomon"), "care advances simulation");
            Check(progress.DrainEvents().Any(e => e["type"]!.GetValue<string>() == "poop"), "care publishes tick events");
            Check(JsonNode.DeepEquals(JsonNode.Parse(progress.Save!), JsonNode.Parse(expected.Farm)), "away coordinates remain original");
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-farm-events-", StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe farm test cleanup.");
            if (Directory.Exists(resolved)) Directory.Delete(resolved, true);
        }
        Console.WriteLine($"Farm events: {checks} commit, retry, positioning, drain and care checks passed.");
        return checks;
    }
}
