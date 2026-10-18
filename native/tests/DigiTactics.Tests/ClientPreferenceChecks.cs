using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Presentation;
using DigiTactics.Storage;

internal static class ClientPreferenceChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string message) { checks++; if (!ok) throw new InvalidOperationException("Client preferences: " + message); }
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/client-preferences.json")));
        foreach(var sample in fixture.RootElement.GetProperty("preferences").EnumerateArray())
        {
            var expected = sample.GetProperty("expected");
            Check(ClientPreferences.Read(sample.GetProperty("json").GetString()) == new ClientPreferences(expected.GetProperty("motion").GetBoolean(),expected.GetProperty("uiSfx").GetBoolean(),expected.GetProperty("social").GetBoolean()),"source default merge and truthiness");
        }
        var gate = new ClientMenuSoundGate();
        foreach(var sample in fixture.RootElement.GetProperty("events").EnumerateArray())
            Check(gate.TryPlay(sample.GetProperty("milliseconds").GetDouble(),sample.GetProperty("enabled").GetBoolean(),sample.GetProperty("quiet").GetBoolean()) == sample.GetProperty("played").GetBoolean(),"source quiet gate and nonquiet clock");
        foreach(var sample in fixture.RootElement.GetProperty("motion").EnumerateArray())
            Check(Math.Abs(ClientMotion.PedestalYaw(sample.GetProperty("seconds").GetDouble(),sample.GetProperty("enabled").GetBoolean())-sample.GetProperty("yaw").GetDouble())<1e-12,"source pedestal motion");
        Check(ClientMotion.CssDuration(.3,false)==.001 && ClientMotion.CssDuration(.3,true)==.3,"source reduced CSS duration");
        var boundary = new ClientMenuSoundGate();
        Check(!boundary.TryPlay(0,true,true) && !boundary.TryPlay(69,true,true) && boundary.TryPlay(70,true,true),"initial and exact 70 ms boundary");
        Check(boundary.TryPlay(71,false,false) && !boundary.TryPlay(140,true,true) && boundary.TryPlay(141,true,true),"nonquiet ignores preference and delays later quiet event");
        Check(!boundary.TryPlay(300,false,true) && boundary.TryPlay(301,true,true),"disabled quiet event does not advance clock");
        string directory = Path.Combine(Path.GetTempPath(),"digitactics-client-preferences-"+Guid.NewGuid().ToString("N"));
        try
        {
            var profile = new NativeProfileStore(directory); var store = new ClientPreferencesStore(profile); store.Refresh();
            Check(store.Settings == new ClientPreferences() && !Directory.Exists(directory),"defaults do not create or mutate profile");
            string legacy="{\"prefs\":{\"motion\":false,\"uiSfx\":false,\"social\":false,\"future\":7},\"profile\":{\"avatar\":\"tokomon\"},\"matches\":[{\"place\":2}],\"recent\":[\"id\"],\"totals\":{\"games\":5},\"unknown\":{\"keep\":true}}";
            Check(profile.UpdateStorage(new Dictionary<string,string>{{"dt.client.v1",legacy},{"dt.name","Tester"}}),"seed isolated imported client");
            store.Refresh(); Check(store.Settings==new ClientPreferences(false,false,false),"import already in source record is applied");
            Check(store.Save(store.Settings with{UiSfx=true}),"save a toggle");
            var saved=JsonNode.Parse(profile.Storage["dt.client.v1"])!;var before=JsonNode.Parse(legacy)!;
            foreach(string key in new[]{"profile","matches","recent","totals","unknown"})Check(JsonNode.DeepEquals(saved[key],before[key]),"preserve "+key);
            Check(saved["prefs"]!["future"]!.GetValue<int>()==7 && profile.Storage["dt.name"]=="Tester","preserve unrelated preferences and profile keys");
            var restart=new ClientPreferencesStore(new NativeProfileStore(directory));restart.Refresh();Check(restart.Settings==new ClientPreferences(false,true,false),"restart reads saved settings");
            saved["totals"]!["games"]=9;profile.UpdateStorage(new Dictionary<string,string>{{"dt.client.v1",saved.ToJsonString()}});
            Check(store.Save(store.Settings with{Social=true}) && JsonNode.Parse(profile.Storage["dt.client.v1"])!["totals"]!["games"]!.GetValue<int>()==9,"edit merges newer match history");
            Check(profile.Reset("RESET"),"reset isolated profile");store.Refresh();Check(store.Settings==new ClientPreferences() && !profile.Storage.ContainsKey("dt.client.v1"),"reset restores defaults without resurrecting settings");
            Check(store.Save(new(false,true,true)),"new preferences after reset");
            var staleProfile=new NativeProfileStore(directory);var stale=new ClientPreferencesStore(staleProfile);stale.Refresh();
            Check(store.Save(new(true,false,true)),"active instance edit");string current=File.ReadAllText(Path.Combine(directory,"profile.json"));
            Check(!stale.Save(new(false,false,false)) && File.ReadAllText(Path.Combine(directory,"profile.json"))==current,"stale writer preserves newer preferences");
            Check(profile.UpdateStorage(new Dictionary<string,string>{{"dt.client.v1","{"}}),"seed corrupt imported record");store.Refresh();
            Check(store.Error is not null && !store.Save(new()) && profile.Storage["dt.client.v1"]=="{","corrupt imported record preserved");
        }
        finally
        {
            string full=Path.GetFullPath(directory);
            if(!full.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(full).StartsWith("digitactics-client-preferences-",StringComparison.Ordinal))throw new InvalidOperationException("Unexpected test directory.");
            if(Directory.Exists(full))Directory.Delete(full,true);
        }
        Console.WriteLine($"Client preferences: {checks} source, sound gate, motion and persistence checks passed.");return checks;
    }
}
