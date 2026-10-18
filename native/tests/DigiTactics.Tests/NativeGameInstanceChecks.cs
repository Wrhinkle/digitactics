using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using DigiTactics.Storage;

internal static class NativeGameInstanceChecks
{
    public static async Task RunChild(string[] arguments)
    {
        if (arguments.Length != 3 || arguments[0] != "--native-game-instance-child") throw new ArgumentException("Invalid instance test child arguments.");
        ulong? lobby = arguments[2] == "focus" ? null : ulong.Parse(arguments[2], System.Globalization.CultureInfo.InvariantCulture);
        using var instance = NativeGameInstance.Open(arguments[1], lobby);
        Console.WriteLine(instance.IsPrimary ? "PRIMARY" : "SECONDARY"); Console.Out.Flush();
        if (!instance.IsPrimary) return;
        Task<string?> stop = Task.Run(Console.In.ReadLine);
        while (!stop.IsCompleted)
        {
            while (instance.TryTake(out ulong? received)) Console.WriteLine(received.HasValue ? "LOBBY:" + received.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "FOCUS");
            Console.Out.Flush(); await Task.Delay(10);
        }
    }

    public static async Task<int> Run()
    {
        int checks = 0;
        void Check(bool condition, string reason) { checks++; if (!condition) throw new InvalidOperationException("Native game instance: " + reason); }
        string root = Path.Combine(Path.GetTempPath(), "digitactics-instance-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var children = new List<Process>();
        Process Start(string directory, string lobby)
        {
            string executable = Environment.ProcessPath ?? throw new IOException("Test host executable missing.");
            var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
            if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase)) start.ArgumentList.Add(Assembly.GetEntryAssembly()!.Location);
            start.ArgumentList.Add("--native-game-instance-child"); start.ArgumentList.Add(directory); start.ArgumentList.Add(lobby);
            var process = Process.Start(start) ?? throw new IOException("Instance child did not start."); children.Add(process); return process;
        }
        async Task<string?> Line(Process process)
        {
            string? line = await process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(12));
            if (line is null) throw new IOException("Instance test child exited unexpectedly: " + await process.StandardError.ReadToEndAsync());
            return line;
        }
        async Task Stop(Process process)
        {
            if (!process.HasExited) { await process.StandardInput.WriteLineAsync("STOP"); await process.StandardInput.FlushAsync(); }
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Check(process.ExitCode == 0, "child exits cleanly: " + await process.StandardError.ReadToEndAsync());
        }
        try
        {
            foreach (string value in new[] { "1", "00001", "18446744073709551615", "00000000000000000001" })
                Check(NativeGameInstance.ParseLobbyArguments(new[] { "unrelated", "+connect_lobby", value }) == ulong.Parse(value), "valid decimal lobby " + value);
            foreach (string value in new[] { "", "0", "000", "-1", "+1", " 1", "1 ", "0x1", "1.0", "1e2", "１", "18446744073709551616", "000000000000000000001", "--shutdown" })
                Check(NativeGameInstance.ParseLobbyArguments(new[] { "+connect_lobby", value }) is null, "malformed lobby ignored");
            Check(NativeGameInstance.ParseLobbyArguments(new[] { "+connect_lobby" }) is null, "incomplete switch ignored");
            Check(NativeGameInstance.ParseLobbyArguments(new[] { "--connect_lobby", "1", "+CONNECT_LOBBY", "2" }) is null, "only exact original switch accepted");
            Check(NativeGameInstance.ParseLobbyArguments(new[] { "+connect_lobby", "bad", "+connect_lobby", "22" }) is null, "malformed first lobby switch is not replaced by a later switch");
            Check(NativeGameInstance.ParseLobbyArguments(new[] { "+connect_lobby", "21", "+connect_lobby", "22" }) == 21, "first valid lobby switch wins");
            Check(NativeGameInstance.IsInteractiveLaunch(Array.Empty<string>()) && NativeGameInstance.IsInteractiveLaunch(new[] { "--minimum-window", "+connect_lobby", "22" }), "normal launch options remain interactive");
            foreach (string diagnostic in new[] { "--workbench", "--smoke", "--import-profile", "--stage-profile", "--native-file-picker-smoke", "--profile-ui-smoke", "--steam-live-smoke", "--future-diagnostic-smoke" })
                Check(!NativeGameInstance.IsInteractiveLaunch(new[] { "--minimum-window", diagnostic, "fixture" }), "diagnostic launch excluded from instance routing");
            Check(NativeGameInstance.ProfileDirectory(Array.Empty<string>()) == NativeGameInstance.DefaultProfileDirectory, "default profile location is shared");
            Check(NativeGameInstance.ProfileDirectory(new[] { "--profile-directory", Path.Combine(root, ".", "Profile") }) == Path.GetFullPath(Path.Combine(root, "Profile")), "explicit profile root is absolute");
            foreach (string[] malformed in new[] { new[] { "--profile-directory" }, new[] { "--profile-directory", "--smoke" } })
            {
                try { NativeGameInstance.ProfileDirectory(malformed); throw new InvalidOperationException("Accepted a missing profile root."); }
                catch (ArgumentException) { checks++; }
            }
            Check(!NativeGameInstance.TryForward(Path.Combine(root, "absent"), null, TimeSpan.FromMilliseconds(100)) && !Directory.Exists(Path.Combine(root, "absent")), "forward-only launch creates no profile directory");

            string directory = Path.Combine(root, "Profile");
            using (var owner = NativeGameInstance.Open(directory, 7))
            {
                Check(owner.IsPrimary && owner.TryTake(out ulong? initial) && initial == 7 && !owner.TryTake(out _), "initial lobby queued exactly once");
                Check(File.Exists(Path.Combine(directory, "instance.lock")) && Directory.GetFiles(directory).Length == 1, "only stable empty instance lease created");
                using (var second = NativeGameInstance.Open(directory, 8)) Check(!second.IsPrimary && !second.TryTake(out _), "second instance forwards without claiming owner");
                Check(owner.TryTake(out ulong? forwarded) && forwarded == 8 && !owner.TryTake(out _), "ACK means forwarded lobby is already queued once");
                string alias = Path.Combine(directory, "..", "Profile").ToLowerInvariant().Replace('\\', '/') + "/";
                var scope = typeof(NativeGameInstance).GetMethod("Scope", BindingFlags.Static | BindingFlags.NonPublic)!;
                Check(Equals(scope.Invoke(null, new object[] { Path.GetFullPath(alias) }), scope.Invoke(null, new object[] { Path.GetFullPath(directory) })), "path aliases produce identical scope");
                bool aliasAccepted = NativeGameInstance.TryForward(alias, null, TimeSpan.FromSeconds(2));
                bool aliasQueued = owner.TryTake(out ulong? focused);
                Check(aliasAccepted && aliasQueued && focused is null, $"case, slash, trailing separator and relative alias focus same profile (accepted={aliasAccepted}, queued={aliasQueued}, lobby={focused})");
                using (var independent = NativeGameInstance.Open(Path.Combine(root, "Other"), 9)) Check(independent.IsPrimary && independent.TryTake(out ulong? other) && other == 9, "different profile has an independent owner");
                string pipeName = (string)typeof(NativeGameInstance).GetField("pipeName", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owner)!;
                async Task<int> Raw(byte[] bytes)
                {
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                    await pipe.ConnectAsync(timeout.Token); await pipe.WriteAsync(bytes, timeout.Token); await pipe.FlushAsync(timeout.Token);
                    byte[] response = new byte[1];
                    try { return await pipe.ReadAsync(response, timeout.Token) == 1 ? response[0] : -1; }
                    catch (IOException) { return -1; }
                }
                byte[] Message(Guid nonce, ulong lobby)
                {
                    byte[] bytes = new byte[28]; "DTI1"u8.CopyTo(bytes); nonce.TryWriteBytes(bytes.AsSpan(4, 16)); BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(20), lobby); return bytes;
                }
                byte[] valid = Message(Guid.NewGuid(), 19);
                Check(await Raw(valid) == 1 && await Raw(valid) == 1 && owner.TryTake(out ulong? repeated) && repeated == 19 && !owner.TryTake(out _), "lost-ACK retry nonce is acknowledged without duplicate delivery");
                byte[] lost = Message(Guid.NewGuid(), 21);
                using (var disconnected = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly))
                {
                    await disconnected.ConnectAsync(2000); await disconnected.WriteAsync(lost); await disconnected.FlushAsync();
                    // Close without reading an ACK, as a launching process killed during forwarding would.
                }
                await Task.Delay(40);
                Check(await Raw(lost) == 1 && owner.TryTake(out ulong? retried) && retried == 21 && !owner.TryTake(out _), "client disconnect before ACK can retry without duplicate delivery");
                byte[] invalid = (byte[])valid.Clone(); invalid[0] = 0;
                Check(await Raw(invalid) != 1 && !owner.TryTake(out _), "invalid protocol rejected");
                Check(await Raw(Message(Guid.Empty, 9)) != 1 && !owner.TryTake(out _), "empty nonce rejected");
                Check(await Raw(valid[..8]) != 1 && !owner.TryTake(out _), "partial message rejected");
                Check(await Raw(valid.Concat(new byte[] { 0 }).ToArray()) != 1 && !owner.TryTake(out _), "oversized message rejected without reading arbitrary content");
                using (var stalled = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly))
                {
                    await stalled.ConnectAsync(2000);
                    Check(await Task.Run(() => NativeGameInstance.TryForward(directory, 20, TimeSpan.FromSeconds(4))) && owner.TryTake(out ulong? afterStall) && afterStall == 20, "connected client sending nothing times out and does not block next launch");
                }
                for (ulong i = 1; i <= 64; i++) Check(NativeGameInstance.TryForward(directory, i, TimeSpan.FromSeconds(2)), "bounded queue accepts available entry");
                Check(await Raw(Message(Guid.NewGuid(), 99)) == 0, "full queue does not ACK a dropped request");
                for (ulong i = 1; i <= 64; i++) Check(owner.TryTake(out ulong? queued) && queued == i, "bounded queue preserves accepted order");
                Check(!owner.TryTake(out _) && NativeGameInstance.TryForward(directory, null, TimeSpan.FromSeconds(2)) && owner.TryTake(out _), "queue resumes after the game consumes requests");
            }
            Check(new FileInfo(Path.Combine(directory, "instance.lock")).Length == 0 && Directory.GetFiles(directory).Length == 1, "dispose retains stable empty lease without profile writes");
            using (var replacement = NativeGameInstance.Open(directory)) Check(replacement.IsPrimary, "disposed owner can be replaced without deleting lease");
            Check(!NativeGameInstance.TryForward(directory, null, TimeSpan.FromMilliseconds(150)), "stale lease cannot report successful forwarding");
            using (var stopping = NativeGameInstance.Open(directory))
            {
                string name = (string)typeof(NativeGameInstance).GetField("pipeName", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stopping)!;
                using var stalled = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await stalled.ConnectAsync(2000);
                await Task.Run(stopping.Dispose).WaitAsync(TimeSpan.FromSeconds(2));
                using var reopened = NativeGameInstance.Open(directory);
                Check(reopened.IsPrimary, "dispose cancels a stalled client before releasing ownership");
            }

            string heldDirectory = Path.Combine(root, "Held"); Directory.CreateDirectory(heldDirectory);
            using (var held = new FileStream(Path.Combine(heldDirectory, "instance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                var elapsed = Stopwatch.StartNew();
                try { using var impossible = NativeGameInstance.Open(heldDirectory, null, TimeSpan.FromMilliseconds(150)); throw new InvalidOperationException("A held lease was stolen."); }
                catch (TimeoutException) { Check(elapsed.Elapsed < TimeSpan.FromSeconds(2), "unresponsive owner has a bounded startup wait without a second primary"); }
            }

            string processDirectory = Path.Combine(root, "Process");
            var primary = Start(processDirectory, "101");
            Check(await Line(primary) == "PRIMARY" && await Line(primary) == "LOBBY:101", "real owner process receives its initial lobby");
            var secondary = Start(processDirectory, "102");
            Check(await Line(secondary) == "SECONDARY" && await Line(primary) == "LOBBY:102", "real second process forwards lobby once"); await Stop(secondary);
            Check(NativeGameInstance.TryForward(processDirectory, null, TimeSpan.FromSeconds(3)) && await Line(primary) == "FOCUS", "real owner process receives focus-only launch");
            primary.Kill(); await primary.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            using (var recovered = NativeGameInstance.Open(processDirectory, 103)) Check(recovered.IsPrimary && recovered.TryTake(out ulong? recoveredLobby) && recoveredLobby == 103, "abrupt owner death releases lease and pipe");

            string delayedDirectory = Path.Combine(root, "Delayed"); Directory.CreateDirectory(delayedDirectory);
            using (var delayedLease = new FileStream(Path.Combine(delayedDirectory, "instance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                var waiting = Start(delayedDirectory, "104");
                await Task.Delay(250); delayedLease.Dispose();
                Check(await Line(waiting) == "PRIMARY" && await Line(waiting) == "LOBBY:104", "launch waiting on a dying startup can become the new owner"); await Stop(waiting);
            }

            string raceDirectory = Path.Combine(root, "Race");
            var racers = Enumerable.Range(201, 6).Select(i => Start(raceDirectory, i.ToString())).ToArray();
            string?[] roles = await Task.WhenAll(racers.Select(Line));
            Check(roles.Count(role => role == "PRIMARY") == 1 && roles.Count(role => role == "SECONDARY") == 5, "simultaneous real launches elect exactly one owner");
            Process winner = racers[Array.IndexOf(roles, "PRIMARY")];
            var delivered = new HashSet<string?>();
            for (int i = 0; i < racers.Length; i++) delivered.Add(await Line(winner));
            Check(delivered.SetEquals(Enumerable.Range(201, 6).Select(i => "LOBBY:" + i)), "simultaneous launches deliver every initial lobby exactly once");
            foreach (var racer in racers) await Stop(racer);
            Console.WriteLine($"Native game instance: {checks} parser, bounded IPC, ownership and real process checks passed.");
            return checks;
        }
        finally
        {
            foreach (var child in children)
            {
                try { if (!child.HasExited) { child.Kill(entireProcessTree: true); await child.WaitForExitAsync(); } }
                catch (InvalidOperationException) { }
                child.Dispose();
            }
            if (Path.GetFullPath(root).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase)) Directory.Delete(root, true);
        }
    }
}
