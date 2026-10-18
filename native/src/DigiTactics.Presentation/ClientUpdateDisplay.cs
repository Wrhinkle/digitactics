using DigiTactics.Storage;

namespace DigiTactics.Presentation;

/// <summary>Installed identity comes from the running application, never the pending update.</summary>
public static class ClientUpdateDisplay
{
    public static string InstalledVersion(string? informationalVersion, Version? assemblyVersion, string? configuration)
    {
        string? version = informationalVersion?.Split('+')[0];
        if (!NativeUpdatePackage.ValidVersion(version))
            version = assemblyVersion is { Build: >= 0 } ? assemblyVersion.ToString(3) : null;
        string title = version is null ? "Digi-Tactics / version unavailable" : "Digi-Tactics " + version;
        return configuration == "Debug" ? title + " (development build)" : title;
    }
    public static string Status(bool updaterActive, NativeUpdateStatus? status)
    {
        if (!updaterActive) return "Automatic updates are not active for this session.";
        string? version = NativeUpdatePackage.ValidVersion(status?.Version) ? status!.Version : null;
        string target = version is null ? "the update" : "version " + version;
        return status?.Phase switch
        {
            "idle" => "Waiting to check for updates.",
            "current" => "Up to date.",
            "checking" => "Checking for updates...",
            "downloading" => "Downloading " + target + (status.Total > 0 ? $"... {Math.Clamp(Math.Floor(100d * status.Received / status.Total), 0, 100):0}%" : "..."),
            "verifying" => "Preparing " + target + "...",
            "ready" when version is not null => $"Version {version} is downloaded and applies on restart.",
            "manual" when version is not null => $"Version {version} requires a new full download.",
            // The native updater combines offline, throttled and failed checks in this phase.
            "unavailable" => "Updates are unavailable. The launcher will try again next start.",
            _ => "Update status is unavailable."
        };
    }
}
