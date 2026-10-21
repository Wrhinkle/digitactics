using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Uses real controls/text input; isolates destructive confirmation and substitutes only picker selection.</summary>
internal sealed class SaveDataUiSmoke : IDisposable
{
    private readonly NativeProfileStore profile;
    private readonly GameAudio audio;
    private FileOperationUiSmoke? operations;
    private readonly string directory, backup;
    private int step = -1, cooldown = 5, selections;
    private bool down, checkedInvalid, checkedCancel, checkedExport, checkedReset;
    public SaveDataUiSmoke(MatchScreen screen, NativeProfileStore profile,GameAudio audio)
    {
        if (profile.Exists) throw new InvalidOperationException("Save data smoke requires a new isolated profile.");
        this.profile = profile;this.audio=audio;
        directory = Path.Combine(Path.GetTempPath(), "digitactics-save-data-ui-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        backup = Path.Combine(directory, "backup.json");
        if (!profile.UpdateStorage(new Dictionary<string, string> { ["dt.name"] = "Reset smoke", ["dt.cosmetics.v1"] = "{\"bits\":1234}", ["dt.future"] = "retained through export" })) throw new InvalidOperationException(profile.Error);
        screen.BackupPicker = _ => Task.FromResult(++selections == 1 ? null : (string?)backup);
    }
    public void Input(MatchScreen screen)
    {
        if(operations is not null){operations.Input(screen);return;}
        if (cooldown-- > 0 || screen.SaveDataOperationPending) return;
        if (down) { ImGui.GetIO().AddMouseButtonEvent(0, false); down = false; step++; cooldown = 5; return; }
        if (step is 4 or 10)
        {
            ImGui.GetIO().AddInputCharactersUTF8(step == 4 ? "RESE"u8 : " reset "u8); step++; cooldown = 5; return;
        }
        if (step >= 13) {operations??=new(screen,profile,audio);operations.Input(screen);return;}
        string key = step switch { -1 => "settings", 0 => "saveData", 1 or 7 => "exportSave", 2 or 8 => "resetSave", 3 or 9 => "resetSaveText", 5 or 11 => "confirmResetSave", 6 => "cancelResetSave", 12 => "closeSaveData", _ => throw new InvalidOperationException("Unexpected save data step.") };
        if (!screen.Buttons.TryGetValue(key, out var point)) return;
        if (point.X < 0 || point.Y < 0 || point.X >= ImGui.GetIO().DisplaySize.X || point.Y >= ImGui.GetIO().DisplaySize.Y) throw new InvalidOperationException("Save data control outside window: " + key);
        ImGui.GetIO().AddMousePosEvent(point.X, point.Y); ImGui.GetIO().AddMouseButtonEvent(0, true); down = true;
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if (step == 2 && cooldown < 2 && !checkedCancel)
        {
            if (selections != 1 || File.Exists(backup) || profile.Progress.Bits != 1234 || screen.SaveDataStatus?.Contains("canceled", StringComparison.OrdinalIgnoreCase) != true) throw new InvalidOperationException("Canceled picker changed the profile or wrote a backup.");
            checkedCancel = true;
        }
        if (step == 6 && cooldown < 2 && !checkedInvalid)
        {
            if (screen.ResetSaveEnabled || !screen.ResetSaveConfirmation || profile.Progress.Bits != 1234) throw new InvalidOperationException("Incorrect confirmation allowed reset.");
            if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "confirmation.bmp"), size.X, size.Y);
            checkedInvalid = true;
        }
        if (step == 8 && !screen.SaveDataOperationPending && !checkedExport)
        {
            var exported = new ProfileImport(backup);
            if (selections != 2 || exported.Progress.Bits != 1234 || exported.Storage["dt.future"] != "retained through export" || profile.Progress.Bits != 1234) throw new InvalidOperationException("Export button did not retain active profile values.");
            checkedExport = true;
        }
        if (step == 12 && cooldown < 2 && !checkedReset)
        {
            if (screen.ResetSaveConfirmation || !profile.CanImport || profile.Storage.Count != 0 || profile.Progress.Bits != 0 || profile.OpenMusicLibrary().Tracks.Count != 0 || new ProfileImport(backup).Progress.Bits != 1234) throw new InvalidOperationException("Confirmed reset did not clear only the active save.");
            if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
            checkedReset = true;
        }
        if (step < 13 || cooldown > 0) return false;
        if(operations is not null)return operations.Verify(screen,size,capture);
        if (screen.SaveDataVisible || !checkedInvalid || !checkedCancel || !checkedExport || !checkedReset || !profile.CanImport || !screen.SettingsVisible || !screen.Buttons.ContainsKey("saveData")) throw new InvalidOperationException("Save data flow did not return to Settings with a fresh import-ready profile.");
        Console.WriteLine("Save data UI passed: canceled picker, current-profile export, invalid RESET disabled, cancellation, typed reset, backup preservation and return to Settings with fresh import available."); return false;
    }
    public void Dispose()
    {
        operations?.Dispose();
        string resolved = Path.GetFullPath(directory);
        if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-save-data-ui-", StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe save data smoke cleanup.");
        Directory.Delete(resolved, true);
    }
}
