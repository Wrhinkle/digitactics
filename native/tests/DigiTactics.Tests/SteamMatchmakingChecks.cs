using System.Runtime.InteropServices;
using DigiTactics.Networking;

internal static class SteamMatchmakingChecks
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int CountFn();
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Matchmaking: " + why); }
        string path = Path.GetFullPath(Path.Combine(root, "artifacts/steam-native/fake_steam_api.dll"));
        nint module = NativeLibrary.Load(path);
        try
        {
            var leaves = Marshal.GetDelegateForFunctionPointer<CountFn>(NativeLibrary.GetExport(module, "FakeSteam_Leaves"));
            int before = leaves();
            using (var client = new SteamClient(path, _ => false, 480))
            {
                var api = client.Matchmaking;
                var create = api.Create(SteamLobbyVisibility.Private, 50);
                Check(!create.IsCompleted, "creation waits for native callback");
                client.PumpCallbacks(); ulong id = create.GetAwaiter().GetResult();
                Check(id != 0 && api.Owner(id) == client.SteamId && api.Read(id).Capacity == 8, "create layout, owner, capacity clamp");
                Check(api.Members(id).SequenceEqual(new ulong[] { client.SteamId, 76561190000000002 }), "member enumeration");
                Check(api.SetData(id, new Dictionary<string,string> { ["dt_game"] = "digitactics", ["dt_ver"] = NetworkProtocol.Version, ["name"] = "Tamer \u96ea" }), "owner writes metadata");
                Check(api.Read(id).Data["name"] == "Tamer \u96ea", "UTF8 metadata survives native boundary");
                Check(!api.SetData(id, new Dictionary<string,string> { ["name"] = "changed", ["bad-key"] = "bad" }) && api.Read(id).Data["name"] == "Tamer \u96ea", "validate entire metadata update before writing");
                var search = api.ListCompatible(); bool duplicate = false;
                try { api.ListCompatible(); } catch (InvalidOperationException) { duplicate = true; }
                client.PumpCallbacks();
                Check(duplicate && search.Result.Count == 1 && search.Result[0].Id == id, "search callback and concurrent search exclusion");
                api.SetData(id, new Dictionary<string,string> { ["dt_ver"] = "old" });
                search = api.ListCompatible(); client.PumpCallbacks();
                Check(search.Result.Count == 0, "incompatible protocol excluded even if native filter returns it");
                var join = api.Join(id); client.PumpCallbacks();
                Check(join.Result == id, "join callback layout");
                var missing = api.Join(999); client.PumpCallbacks();
                Check(missing.IsFaulted && missing.Exception!.InnerException is IOException, "failed join propagates result");
                Check(!api.SetOwner(id, 999) && api.SetOwner(id, 76561190000000002), "only member can receive ownership");
                Check(!api.SetData(id, new Dictionary<string,string> { ["name"] = "unauthorized" }), "former owner cannot edit metadata");
                api.Leave(id); api.Leave(id);
                Check(leaves() == before + 1, "leave idempotent");
                create = api.Create(SteamLobbyVisibility.Private); client.PumpCallbacks(); _ = create.Result;
                var pending = api.Join(id); api.Dispose();
                Check(pending.IsFaulted && pending.Exception!.InnerException is ObjectDisposedException, "dispose cancels pending tasks");
                client.PumpCallbacks();
            }
            Check(leaves() == before + 2, "disposal leaves joined lobby once");
        }
        finally { NativeLibrary.Free(module); }
        Console.WriteLine($"Steam matchmaking: {checks} native lobby checks passed.");
        return checks;
    }
}
