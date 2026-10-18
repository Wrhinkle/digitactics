using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using DigiTactics.Networking;

internal static class SteamMessageChecks
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void InjectFn(ulong from, byte[] bytes, int size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void RawFn(int type, ulong from, byte[] bytes, int size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int StatsFn([Out] byte[] bytes, int size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int IntFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint TextFn();
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Steam native messages: " + why); }
        string dll = Path.Combine(root, "artifacts/steam-native/fake_steam_api.dll");
        nint module = NativeLibrary.Load(Path.GetFullPath(dll));
        try
        {
            T Bind<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(module, name));
            var inject = Bind<InjectFn>("FakeSteam_Inject"); var raw = Bind<RawFn>("FakeSteam_InjectRaw"); var stats = Bind<StatsFn>("FakeSteam_Stats");
            var lastFlags = Bind<IntFn>("FakeSteam_LastFlags"); var lastChannel = Bind<IntFn>("FakeSteam_LastChannel"); var lastSent = Bind<TextFn>("FakeSteam_LastSent");
            const ulong self = 76561190000000001, peer = 76561190000000002, stranger = 76561190000000003;
            var members = new HashSet<ulong> { self, peer };
            using var transport = new SteamMessageTransport(module, members.Contains);
            string text = "{\"hello\":\"Tamer ♥ 漢字\"}";
            Check(transport.Send(self, text, true) && lastFlags() == 41 && lastChannel() == 0, "reliable channel/flags and identity ABI");
            Check(Marshal.PtrToStringUTF8(lastSent()) == text, "UTF-8 send buffer");
            var delivered = new List<(ulong, string)>();
            Check(transport.Poll(1000, (id, data) => delivered.Add((id, data))) == 1 && delivered[0] == (self, text), "native loopback identity and bytes");
            Check(!transport.Send(stranger, "hello", true) && !transport.Send(peer, "", true) && !transport.Send(peer, new string('x', 512 * 1024 + 1), true) && !transport.Send(peer, "\ud800", true), "send rejection before native API");
            byte[] valid = Encoding.UTF8.GetBytes("incoming");
            inject(peer, valid, valid.Length); Check(transport.Accept(peer), "pending member session accepted");
            Check(transport.Send(peer, "effect", false) && lastFlags() == 33, "unreliable no-Nagle and auto-restart flags");
            Check(!transport.Accept(stranger), "nonmember session refused");
            raw(1, peer, valid, valid.Length); inject(stranger, valid, valid.Length); inject(peer, valid, 0); inject(peer, valid, -1);
            inject(peer, new byte[512 * 1024 + 1], 512 * 1024 + 1); inject(peer, [0xff], 1);
            delivered.Clear(); Check(transport.Poll(2000, (id, data) => delivered.Add((id, data))) == 1 && delivered[0] == (peer, "incoming"), "bad identity, nonmember, size and UTF-8 messages dropped");
            transport.Block(peer); inject(peer, valid, valid.Length);
            Check(!transport.Send(peer, "blocked", true) && !transport.Accept(peer) && transport.Poll(3000, (_, _) => throw new InvalidOperationException("Blocked peer delivered")) == 0, "blocked session stays blocked");
            // A fresh transport gives the member an independent room/session budget.
            using var burst = new SteamMessageTransport(module, members.Contains);
            for (int i = 0; i < 350; i++) inject(peer, [65], 1);
            int received = 0;
            for (int i = 0; i < 6; i++) received += burst.Poll(100000, (_, _) => { });
            Check(received == 300 && burst.Dropped == 50, "native message count budget");
            inject(peer, [65], 1); Check(burst.Poll(101000, (_, _) => { }) == 1, "native budget refill");
            inject(peer, valid, valid.Length); inject(peer, valid, valid.Length);
            bool callbackFailed = false;
            try { burst.Poll(102000, (_, _) => throw new ApplicationException("test callback")); } catch (ApplicationException) { callbackFailed = true; }
            Check(callbackFailed, "application callback exception reaches caller");
            long drops = burst.Dropped;
            var large = new byte[512 * 1024];
            for (int i = 0; i < 17; i++) inject(peer, large, large.Length);
            Check(burst.Poll(200000, (_, _) => { }) == 16 && burst.Dropped == drops + 1, "native eight-megabyte burst limit");
            var buffer = new byte[4096]; int length = stats(buffer, buffer.Length);
            using var counters = JsonDocument.Parse(buffer.AsMemory(0, length)); var c = counters.RootElement;
            Check(c.GetProperty("allocated").GetInt64() == c.GetProperty("released").GetInt64() && c.GetProperty("queued").GetInt64() == 0, "all messages released even after callback exception");
            Check(c.GetProperty("badRelease").GetInt64() == 0 && c.GetProperty("badIdentity").GetInt64() == 0 && c.GetProperty("badBuffer").GetInt64() == 0, "no ABI, double-release or buffer errors");
            burst.Dispose(); bool disposed = false;
            try { burst.Send(peer, "closed", true); } catch (ObjectDisposedException) { disposed = true; }
            Check(disposed, "disposed transport refuses calls into Steam");
        }
        finally { NativeLibrary.Free(module); }
        Console.WriteLine($"Steam native messages: {checks} fake-DLL ABI, send, receive, release and traffic checks passed.");
        return checks;
    }
}
