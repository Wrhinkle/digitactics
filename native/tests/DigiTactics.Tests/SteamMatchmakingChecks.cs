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
            long now = 1000;
            using (var client = new SteamClient(path, _ => false, 480, () => now))
            {
                var api = client.Matchmaking;
                int initialLeaves = leaves();
                var lateCreate = api.Create(SteamLobbyVisibility.Private);
                now += 30000; client.PumpCallbacks();
                Check(lateCreate.IsFaulted && lateCreate.Exception!.InnerException is TimeoutException, "late create remains timed out");
                Check(leaves() == initialLeaves + 1, "late successful creation leaves abandoned lobby");
                var create = api.Create(SteamLobbyVisibility.Private); client.PumpCallbacks(); ulong id = create.Result;
                api.Leave(id); initialLeaves = leaves();
                var lateJoin = api.Join(id); now += 30000; client.PumpCallbacks();
                Check(lateJoin.IsFaulted && lateJoin.Exception!.InnerException is TimeoutException && leaves() == initialLeaves + 1, "late successful join leaves abandoned lobby");
                initialLeaves = leaves();
                var oldJoin = api.Join(id); now += 30000;
                var retry = api.Join(id); client.PumpCallbacks();
                Check(oldJoin.IsFaulted && oldJoin.Exception!.InnerException is TimeoutException && retry.Result == id, "new join survives earlier timeout");
                Check(leaves() == initialLeaves && api.SetData(id, new Dictionary<string,string> { ["name"] = "Retry" }), "late cleanup does not leave newer successful membership");
                var duplicateJoin = api.Join(id); now += 30000; client.PumpCallbacks();
                Check(duplicateJoin.IsFaulted && duplicateJoin.Exception!.InnerException is TimeoutException && leaves() == initialLeaves, "late duplicate join preserves existing membership");
                api.Leave(id); initialLeaves = leaves();
                var failed = api.Join(999); now += 30000; client.PumpCallbacks();
                Check(failed.IsFaulted && failed.Exception!.InnerException is TimeoutException && leaves() == initialLeaves, "late failed join does not leave unrelated lobby");
                var search = api.ListCompatible(); now += 30000; client.PumpCallbacks();
                Check(search.IsFaulted && search.Exception!.InnerException is TimeoutException, "search timeout retained after callback");
                search = api.ListCompatible(); client.PumpCallbacks(); Check(search.IsCompletedSuccessfully, "search can retry after timeout");
                var requests = Enumerable.Range(0, 64).Select(_ => api.Join(id)).ToArray();
                bool bounded = false; try { api.Join(id); } catch (IOException) { bounded = true; }
                Check(bounded, "unresolved native requests bounded before starting another call");
                now += 30000; client.PumpCallbacks();
                Check(requests.All(t => t.IsFaulted && t.Exception!.InnerException is TimeoutException), "bounded batch faults exactly once");
                create = api.Create(SteamLobbyVisibility.Private); client.PumpCallbacks();
                Check(create.IsCompletedSuccessfully, "callback cleanup releases request capacity");
                api.Leave(create.Result);
                create = api.Create(SteamLobbyVisibility.Private); now += 29999; client.PumpCallbacks();
                Check(create.IsCompletedSuccessfully, "callback before deadline succeeds");
            }
        }
        finally { NativeLibrary.Free(module); }
        Console.WriteLine($"Steam matchmaking: {checks} native lobby checks passed.");
        return checks;
    }
}
