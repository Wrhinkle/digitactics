using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;
using System.Text.Json.Nodes;

namespace DigiTactics.Desktop;

/// <summary>Real checkbox/navigation input against an explicitly empty isolated profile.</summary>
internal sealed class ClientPreferencesSmoke
{
    private readonly NativeProfileStore profile;
    private readonly GameAudio audio;
    private int step, cooldown = 5;
    private bool down, checkedOff;
    public ClientPreferencesSmoke(NativeProfileStore profile, GameAudio audio)
    {
        // Source reduced-motion CSS: one 1 ms iteration, with the original fill mode.
        static void Near(double actual,double expected) { if(Math.Abs(actual-expected)>1e-8)throw new InvalidOperationException("Reduced egg CSS animation diverged."); }
        Near(EggArtwork.Motion(.00025,false).Rotation,-12*Math.PI/180);
        Near(EggArtwork.Motion(.001,false).Rotation,0);Near(EggArtwork.Motion(.8,false).Rotation,0);
        Near(EggArtwork.Motion(1.3005,false).X,2);Near(EggArtwork.Motion(1.302,false).X,0);
        Near(EggArtwork.Motion(2.05,false).Opacity,1);Near(EggArtwork.Motion(2.052,false).Opacity,0);
        Near(EggArtwork.RevealProgress(.001,false),1);Near(EggArtwork.RevealProgress(.4,true),1);
        this.profile = profile; this.audio = audio;
        if (profile.Exists) throw new InvalidOperationException("Client preference smoke requires an empty isolated profile directory.");
        if (!profile.UpdateStorage(new Dictionary<string,string> { ["dt.client.v1"] = "{\"totals\":{\"games\":9},\"future\":{\"keep\":7},\"prefs\":{\"future\":3}}" })) throw new InvalidOperationException(profile.Error);
    }
    public void Input(MatchScreen screen)
    {
        if (cooldown-- > 0 || step >= 9) return;
        if (down) { ImGui.GetIO().AddMouseButtonEvent(0, false); down = false; step++; cooldown = 4; return; }
        string key = step switch { 0 or 5 => "settings", 1 => "setMotion", 2 => "setUiSfx", 3 => "setSocial", 4 => "closeSettings", 6=>"settingsMusicVolume",7=>"settingsEffectsVolume",8=>"settingsBoardVolume", _ => throw new InvalidOperationException() };
        if (!screen.Buttons.TryGetValue(key, out var point)) return;
        if (point.X < 0 || point.Y < 0 || point.X >= ImGui.GetIO().DisplaySize.X || point.Y >= ImGui.GetIO().DisplaySize.Y) throw new InvalidOperationException("Preference control outside supported window: " + key);
        ImGui.GetIO().AddMousePosEvent(point.X, point.Y); ImGui.GetIO().AddMouseButtonEvent(0, true); down = true;
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if (step == 4 && cooldown < 2 && !checkedOff)
        {
            if (!screen.SettingsVisible || screen.InterfacePreferences != new ClientPreferences(false,false,false) || audio.MenuSounds) throw new InvalidOperationException("Interface checkboxes did not apply runtime preferences.");
            int played = audio.Played; audio.PlayClient("levelup");
            if (audio.Played != played) throw new InvalidOperationException("Disabled menu sounds still played a quiet event.");
            audio.PlayClient("levelup", false);
            if (audio.Available && audio.Played != played + 1) throw new InvalidOperationException("Menu sound preference suppressed a nonquiet action cue.");
            if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y); checkedOff = true;
        }
        if (step < 9 || cooldown > 0) return false;
        var restart = new ClientPreferencesStore(new NativeProfileStore(profile.DataDirectory)); restart.Refresh();
        var client = JsonNode.Parse(profile.Storage["dt.client.v1"])!;
        if (!checkedOff || !screen.SettingsVisible || restart.Settings != new ClientPreferences(false,false,false) || client["totals"]!["games"]!.GetValue<int>() != 9 || client["future"]!["keep"]!.GetValue<int>() != 7 || client["prefs"]!["future"]!.GetValue<int>() != 3)
            throw new InvalidOperationException("Interface preferences did not survive reopening/restart or preserve saved history.");
        if(Math.Abs(audio.MusicVolume-.5f)>.03f||Math.Abs(audio.EffectsVolume-.5f)>.03f||Math.Abs(audio.BoardVolume-.5f)>.03f)
            throw new InvalidOperationException("Restored volume rows did not apply their actual slider input.");
        Console.WriteLine("Client preferences UI passed: actual checkboxes and three volume sliders, source quiet/nonquiet audio behavior, persisted reopening and preserved history/future fields."); return true;
    }
}
