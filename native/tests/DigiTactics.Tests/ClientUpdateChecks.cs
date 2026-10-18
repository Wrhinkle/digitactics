using DigiTactics.Presentation;
using DigiTactics.Storage;

internal static class ClientUpdateChecks
{
    public static int Run()
    {
        int checks = 0;
        void Check(bool ok, string message) { checks++; if (!ok) throw new InvalidOperationException("Client update display: " + message); }
        Check(ClientUpdateDisplay.InstalledVersion("2.3.4+abcdef", new(2,3,4,0), "Release") == "Digi-Tactics 2.3.4", "publish version excludes commit metadata");
        Check(ClientUpdateDisplay.InstalledVersion("1.0.0+abcdef", new(1,0,0,0), "Debug") == "Digi-Tactics 1.0.0 (development build)", "debug is not presented as an installed release");
        Check(ClientUpdateDisplay.InstalledVersion(null, new(2,3,4,0), "Release") == "Digi-Tactics 2.3.4", "assembly version fallback");
        Check(ClientUpdateDisplay.InstalledVersion("invalid", null, "Release") == "Digi-Tactics / version unavailable", "missing version does not invent a release");
        Check(ClientUpdateDisplay.Status(false,null) == "Automatic updates are not active for this session.", "direct launch or disabled updater remains explicit");
        Check(ClientUpdateDisplay.Status(true,null) == "Update status is unavailable.", "missing status never claims current");
        var expected = new Dictionary<string,string>
        {
            ["idle"]="Waiting to check for updates.", ["current"]="Up to date.", ["checking"]="Checking for updates...",
            ["downloading"]="Downloading version 9.0.0... 25%", ["verifying"]="Preparing version 9.0.0...",
            ["ready"]="Version 9.0.0 is downloaded and applies on restart.", ["manual"]="Version 9.0.0 requires a new full download.",
            ["unavailable"]="Updates are unavailable. The launcher will try again next start.", ["future"]="Update status is unavailable."
        };
        foreach (var (phase,text) in expected)
        {
            var status = new NativeUpdateStatus("fixture", phase, "9.0.0", 25, 100);
            Check(ClientUpdateDisplay.Status(true,status) == text, "native phase " + phase);
        }
        Check(ClientUpdateDisplay.Status(true,new("fixture","downloading","9.0.0",10,0)) == "Downloading version 9.0.0...", "unknown size has no invented percentage");
        Check(ClientUpdateDisplay.Status(true,new("fixture","downloading","9.0.0",120,100)).EndsWith("100%",StringComparison.Ordinal), "percentage capped");
        Check(ClientUpdateDisplay.Status(true,new("fixture","downloading","9.0.0",-1,100)).EndsWith("0%",StringComparison.Ordinal), "negative progress clamped");
        Check(ClientUpdateDisplay.Status(true,new("fixture","ready","bad")) == "Update status is unavailable.", "invalid target cannot advertise ready release");
        Console.WriteLine($"Client update display: {checks} installed-version and updater-state checks passed."); return checks;
    }
}
