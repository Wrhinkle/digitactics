using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace DigiTactics.Storage;

/// <summary>One game owner per stable profile root. Only focus and a lobby identity cross the pipe.</summary>
public sealed class NativeGameInstance : IDisposable
{
    public const int ForwardedExitCode = 73;
    private const int QueueLimit = 64, MessageSize = 28;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(8), ClientTimeout = TimeSpan.FromSeconds(1);
    private readonly FileStream? lease;
    private readonly NamedPipeServerStream? pipe;
    private readonly CancellationTokenSource cancellation = new();
    private readonly Queue<ulong?> requests = new();
    private readonly Queue<Guid> recentOrder = new();
    private readonly HashSet<Guid> recent = new();
    private readonly object gate = new();
    private readonly Task listener = Task.CompletedTask;
    private int disposed;
    private readonly string pipeName;
    public bool IsPrimary => lease is not null;
    public static string DefaultProfileDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigiTactics", "native");

    public static bool IsInteractiveLaunch(IReadOnlyList<string> arguments) => !arguments.Any(argument =>
        argument is "--workbench" or "--smoke" or "--import-profile" or "--stage-profile" || argument.EndsWith("-smoke", StringComparison.Ordinal));

    public static string ProfileDirectory(IReadOnlyList<string> arguments)
    {
        for (int i = 0; i < arguments.Count; i++)
        {
            if (arguments[i] != "--profile-directory") continue;
            if (i + 1 >= arguments.Count || arguments[i + 1].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException("Provide a path after --profile-directory.");
            return Path.GetFullPath(arguments[i + 1]);
        }
        return DefaultProfileDirectory;
    }

    private NativeGameInstance(string pipeName) => this.pipeName = pipeName;
    private NativeGameInstance(string pipeName, FileStream lease, ulong? lobby) : this(pipeName)
    {
        this.lease = lease;
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Native game instances require Windows.");
        pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message,
            PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly | PipeOptions.FirstPipeInstance, 128, 128);
        requests.Enqueue(lobby);
        listener = Listen();
    }

    /// <summary>Accept only the original Steam launch switch and a nonzero decimal uint64 identity.</summary>
    public static ulong? ParseLobbyArguments(IReadOnlyList<string> arguments)
    {
        for (int i = 0; i + 1 < arguments.Count; i++)
        {
            if (arguments[i] != "+connect_lobby") continue;
            string value = arguments[i + 1];
            if (value.Length is >= 1 and <= 20 && value.All(c => c is >= '0' and <= '9') &&
                ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out ulong lobby) && lobby != 0) return lobby;
            return null;
        }
        return null;
    }

    /// <summary>Acquire the owner lease, or return a secondary instance only after the owner accepts its request.</summary>
    public static NativeGameInstance Open(string profileDirectory, ulong? lobby = null, TimeSpan? forwardingTimeout = null)
    {
        ValidateLobby(lobby);
        TimeSpan timeout = ValidateTimeout(forwardingTimeout ?? DefaultTimeout);
        string directory = Path.GetFullPath(profileDirectory);
        Directory.CreateDirectory(directory);
        string name = Scope(directory), lockPath = Path.Combine(directory, "instance.lock");
        var elapsed = Stopwatch.StartNew();
        Guid request = Guid.NewGuid();
        do
        {
            FileStream? owner = null;
            try { owner = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException error) when ((error.HResult & 0xffff) is 32 or 33) { }
            if (owner is not null)
            {
                try { return new NativeGameInstance(name, owner, lobby); }
                catch { owner.Dispose(); throw; }
            }
            TimeSpan left = timeout - elapsed.Elapsed;
            if (left <= TimeSpan.Zero) break;
            if (ForwardOnce(name, lobby, request, left < ClientTimeout ? left : ClientTimeout).GetAwaiter().GetResult()) return new NativeGameInstance(name);
            Pause(elapsed, timeout);
        } while (elapsed.Elapsed < timeout);
        throw new TimeoutException("The running DigiTactics instance did not accept the launch request. Try again after it finishes starting or closes.");
    }

    /// <summary>Forward to an existing game without acquiring a game lease or changing profile data.</summary>
    public static bool TryForward(string profileDirectory, ulong? lobby, TimeSpan timeout)
    {
        ValidateLobby(lobby); ValidateTimeout(timeout);
        string directory = Path.GetFullPath(profileDirectory);
        if (!Directory.Exists(directory) || !File.Exists(Path.Combine(directory, "instance.lock"))) return false;
        string name = Scope(directory);
        var elapsed = Stopwatch.StartNew();
        Guid request = Guid.NewGuid();
        do
        {
            TimeSpan left = timeout - elapsed.Elapsed;
            if (left <= TimeSpan.Zero) return false;
            if (ForwardOnce(name, lobby, request, left < ClientTimeout ? left : ClientTimeout).GetAwaiter().GetResult()) return true;
            Pause(elapsed, timeout);
        } while (elapsed.Elapsed < timeout);
        return false;
    }

    /// <summary>Consume on the game thread. A null request asks the existing window to focus.</summary>
    public bool TryTake(out ulong? lobby)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);
        lock (gate) return requests.TryDequeue(out lobby);
    }

    private async Task Listen()
    {
        while (!cancellation.IsCancellationRequested)
        {
            bool connected = false;
            try
            {
                await pipe!.WaitForConnectionAsync(cancellation.Token).ConfigureAwait(false);
                connected = true;
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token);
                deadline.CancelAfter(ClientTimeout);
                byte[] message = new byte[MessageSize];
                int size = await pipe.ReadAsync(message, deadline.Token).ConfigureAwait(false);
                if (size != MessageSize || !pipe.IsMessageComplete || !message.AsSpan(0, 4).SequenceEqual("DTI1"u8)) continue;
                Guid request = new(message.AsSpan(4, 16));
                if (request == Guid.Empty) continue;
                ulong id = BinaryPrimitives.ReadUInt64LittleEndian(message.AsSpan(20));
                bool accepted;
                lock (gate)
                {
                    accepted = recent.Contains(request);
                    if (!accepted && requests.Count < QueueLimit && !cancellation.IsCancellationRequested)
                    {
                        requests.Enqueue(id == 0 ? null : id);
                        recent.Add(request); recentOrder.Enqueue(request);
                        if (recentOrder.Count > QueueLimit * 2) recent.Remove(recentOrder.Dequeue());
                        accepted = true;
                    }
                }
                await pipe.WriteAsync(new byte[] { accepted ? (byte)1 : (byte)0 }, deadline.Token).ConfigureAwait(false);
                await pipe.FlushAsync(deadline.Token).ConfigureAwait(false);
                // Disconnect discards unread pipe data. Let the client consume the ACK and close first.
                await pipe.ReadAsync(new byte[1], deadline.Token).ConfigureAwait(false);
            }
            catch (Exception error) when (error is IOException or OperationCanceledException or UnauthorizedAccessException or ObjectDisposedException) { }
            finally
            {
                if (connected) try { pipe!.Disconnect(); } catch (IOException) { }
            }
        }
    }

    private static async Task<bool> ForwardOnce(string name, ulong? lobby, Guid request, TimeSpan timeout)
    {
        using var deadline = new CancellationTokenSource(timeout);
        using var client = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        try
        {
            await client.ConnectAsync(deadline.Token).ConfigureAwait(false);
            byte[] message = new byte[MessageSize];
            "DTI1"u8.CopyTo(message); request.TryWriteBytes(message.AsSpan(4, 16));
            BinaryPrimitives.WriteUInt64LittleEndian(message.AsSpan(20), lobby ?? 0);
            await client.WriteAsync(message, deadline.Token).ConfigureAwait(false);
            await client.FlushAsync(deadline.Token).ConfigureAwait(false);
            byte[] acknowledgement = new byte[1];
            int size = await client.ReadAsync(acknowledgement, deadline.Token).ConfigureAwait(false);
            return size == 1 && acknowledgement[0] == 1;
        }
        catch (Exception error) when (error is IOException or OperationCanceledException or UnauthorizedAccessException) { return false; }
    }

    private static void ValidateLobby(ulong? lobby)
    { if (lobby == 0) throw new ArgumentOutOfRangeException(nameof(lobby), "A lobby identity must be nonzero; use null for focus."); }
    private static TimeSpan ValidateTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromMinutes(1)) throw new ArgumentOutOfRangeException(nameof(timeout));
        return timeout;
    }
    private static void Pause(Stopwatch elapsed, TimeSpan timeout)
    {
        double left = (timeout - elapsed.Elapsed).TotalMilliseconds;
        if (left >= 1) Thread.Sleep(TimeSpan.FromMilliseconds(Math.Min(20, left)));
    }
    private static string Scope(string directory)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Native game instances require Windows.");
        // Resolve directory junctions as well as case, separators and relative components before hashing.
        using var handle = CreateFile(directory, 0, 7, 0, 3, 0x02000000, 0);
        if (handle.IsInvalid) throw new IOException("The profile directory could not be resolved.", new Win32Exception(Marshal.GetLastWin32Error()));
        var path = new StringBuilder(512);
        uint size = GetFinalPathNameByHandle(handle, path, (uint)path.Capacity, 0);
        if (size >= path.Capacity) { path.EnsureCapacity(checked((int)size + 1)); size = GetFinalPathNameByHandle(handle, path, (uint)path.Capacity, 0); }
        if (size == 0 || size >= path.Capacity) throw new IOException("The profile directory could not be resolved.", new Win32Exception(Marshal.GetLastWin32Error()));
        using var identity = WindowsIdentity.GetCurrent();
        string user = identity.User?.Value ?? throw new UnauthorizedAccessException("The current Windows identity is unavailable.");
        string canonical = Path.TrimEndingDirectorySeparator(path.ToString()).ToUpperInvariant();
        return "digitactics-instance-" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(user + "\n" + canonical)));
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0) return;
        cancellation.Cancel();
        // Finish pending pipe I/O before releasing ownership so the next owner cannot race the old server.
        try { listener.GetAwaiter().GetResult(); }
        finally
        {
            pipe?.Dispose(); lease?.Dispose(); cancellation.Dispose();
            lock (gate) requests.Clear();
        }
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string name, uint access, uint sharing, nint security, uint creation, uint flags, nint template);
    [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandle(SafeFileHandle handle, StringBuilder path, uint size, uint flags);
}
