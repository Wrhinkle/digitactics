using System.Buffers.Binary;
using System.Runtime.InteropServices;
using DigiTactics.Networking;

internal static class SteamClientChecks
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void FailFn(int status);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int CountFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void CallbackFn(int id, byte[] bytes, int size);
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Steam client: " + why); }
        string path = Path.GetFullPath(Path.Combine(root, "artifacts/steam-native/fake_steam_api.dll"));
        nint module = NativeLibrary.Load(path);
        try
        {
            T Bind<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(module, name));
            var fail = Bind<FailFn>("FakeSteam_FailInit"); var shuts = Bind<CountFn>("FakeSteam_ShutdownCount");
            var releases = Bind<CountFn>("FakeSteam_CallbackReleases"); var errors = Bind<CountFn>("FakeSteam_CallbackErrors"); var inject = Bind<CallbackFn>("FakeSteam_Callback");
            string? priorApp = Environment.GetEnvironmentVariable("SteamAppId"), priorGame = Environment.GetEnvironmentVariable("SteamGameId");
            fail(1); bool initFailed = false;
            try { using var rejected = new SteamClient(path, _ => true, 480); } catch (InvalidOperationException e) { initFailed = e.Message == "Fake Steam is unavailable"; }
            Check(initFailed && shuts() == 0, "failed init reports error without shutting down uninitialized API");
            Check(Environment.GetEnvironmentVariable("SteamAppId") == priorApp && Environment.GetEnvironmentVariable("SteamGameId") == priorGame, "failed init restores environment");
            fail(0);
            using (var client = new SteamClient(path, _ => true, 480))
            {
                Check(client.SteamId == 76561190000000001 && client.AppId == 480 && client.Name == "Native Test Tamer", "identity interfaces initialized");
                bool duplicate = false;
                try { using var other = new SteamClient(path, _ => true); } catch (InvalidOperationException) { duplicate = true; }
                Check(duplicate && shuts() == 0, "second client cannot take over lifecycle");
                var copied = new List<SteamCallback>(); client.Callback += copied.Add;
                inject(506, [1, 2, 3, 4], 4); inject(999, [], -1);
                Check(client.PumpCallbacks() == 2 && releases() == 2 && copied.Count == 1 && copied[0].Data.SequenceEqual(new byte[] { 1, 2, 3, 4 }) && client.DroppedCallbacks == 1, "callback copied and native storage freed, malformed callback skipped");
                var calls = new List<SteamCallResult>(); client.CallResult += calls.Add;
                byte[] completed = new byte[16]; BinaryPrimitives.WriteUInt64LittleEndian(completed, 77); BinaryPrimitives.WriteInt32LittleEndian(completed.AsSpan(8), 504); BinaryPrimitives.WriteUInt32LittleEndian(completed.AsSpan(12), 4);
                inject(703, completed, 16); BinaryPrimitives.WriteUInt64LittleEndian(completed, 88); inject(703, completed, 16);
                client.PumpCallbacks();
                Check(calls.Count == 2 && !calls[0].Failed && BinaryPrimitives.ReadUInt32LittleEndian(calls[0].Data) == 0x12345678 && calls[1].Failed && calls[1].Data.Length == 0, "async call results and IO failures dispatched");
                void Throws(SteamCallback _) => throw new ApplicationException("test callback");
                client.Callback += Throws; inject(123, [9], 1); bool thrown = false;
                try { client.PumpCallbacks(); } catch (ApplicationException) { thrown = true; } client.Callback -= Throws;
                Check(thrown && releases() == 5 && errors() == 0, "callback exception cannot retain native callback memory");
                for (int i = 0; i < 260; i++) inject(123, [], 0);
                Check(client.PumpCallbacks() == 256 && client.PumpCallbacks() == 4, "callback budget prevents starving game loop");
                bool wrongThread = Task.Run(() => { try { client.PumpCallbacks(); return false; } catch (InvalidOperationException) { return true; } }).GetAwaiter().GetResult();
                Check(wrongThread, "callbacks restricted to game thread");
            }
            Check(shuts() == 1 && errors() == 0, "one shutdown with balanced callback ownership");
            Check(Environment.GetEnvironmentVariable("SteamAppId") == priorApp && Environment.GetEnvironmentVariable("SteamGameId") == priorGame, "dispose restores development app environment");
            using (var again = new SteamClient(path, _ => true)) { Check(again.AppId == 480, "lifecycle can restart after dispose"); }
            Check(shuts() == 2, "second lifecycle shuts down once");
            var invalidIdentity = Bind<FailFn>("FakeSteam_InvalidIdentity"); invalidIdentity(1); bool partialFailed = false;
            try { using var partial = new SteamClient(path, _ => true); } catch (InvalidOperationException) { partialFailed = true; }
            Check(partialFailed && shuts() == 3, "failure after successful init still shuts down");
            invalidIdentity(0);
            using (var recovered = new SteamClient(path, _ => true)) { Check(recovered.SteamId != 0, "partial failure releases singleton ownership"); }
        }
        finally { NativeLibrary.Free(module); }
        Console.WriteLine($"Steam client: {checks} initialization, identity, callback and shutdown checks passed.");
        return checks;
    }
}
