using System.Runtime.InteropServices;
using System.Text;

namespace DigiTactics.Storage;

/// <summary>Fatal diagnostics remain available when a Windows GUI application has no console.</summary>
public static class NativeApplicationError
{
    public static string WriteLog(string directory, string component, Exception error)
    {
        if (component is not ("game" or "launcher")) throw new ArgumentException("Unknown application component.", nameof(component));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, component + "-error.log");
        if (File.Exists(path) && new FileInfo(path).Length > 1024 * 1024)
            File.Move(path, path + ".previous", overwrite: true);
        string detail = error.ToString();
        if (detail.Length > 32768) detail = detail[..32768] + "\n[truncated]";
        File.AppendAllText(path, $"[{DateTimeOffset.UtcNow:O}] {detail}\n\n", new UTF8Encoding(false));
        return path;
    }

    public static void Report(string component, Exception error, bool interactive)
    {
        Console.Error.WriteLine($"Digi-Tactics {component}: {error}");
        string? log = null;
        try
        {
            log = WriteLog(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DigiTactics", "logs"), component, error);
        }
        catch (Exception failure) when (failure is IOException or UnauthorizedAccessException or ArgumentException)
        { Console.Error.WriteLine("Could not write startup log: " + failure.Message); }
        // Redirected command-line tests/tools receive stderr and the exit code;
        // Explorer launches have no standard handles and need a visible error.
        if (!interactive || !OperatingSystem.IsWindows() || !Environment.UserInteractive || Redirected(-11) || Redirected(-12)) return;
        string message = "Digi-Tactics could not start.\n\n" + error.Message;
        if (message.Length > 2000) message = message[..2000];
        message += log is null ? "\n\nThe error log could not be saved." : "\n\nDetails were saved to:\n" + log;
        MessageBoxW(IntPtr.Zero, message, "Digi-Tactics", 0x10);
    }

    private static bool Redirected(int handle) => GetFileType(GetStdHandle(handle)) is 1 or 3;
    [DllImport("kernel32.dll")] private static extern IntPtr GetStdHandle(int handle);
    [DllImport("kernel32.dll")] private static extern uint GetFileType(IntPtr handle);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int MessageBoxW(IntPtr owner, string text, string caption, uint type);
}
