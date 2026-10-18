using System.Text.Json;
using Brutal.ImGuiApi;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showSoundboard;
    private MatchPlanningSession? soundboardSession;
    private MatchSoundboard soundboard = new();
    internal int ReceivedSoundboardEffects { get; private set; }
    private void EnsureSoundboard()
    {
        if (ReferenceEquals(soundboardSession, Session)) return;
        soundboardSession = Session; soundboard = new(); soundboard.Play += audio.PlayBoard;
    }
    private void NetworkEffect(string sender, JsonElement effect)
    {
        if (Session is null || PlayerMuted(sender)) return;
        if (effect.GetProperty("kind"u8).GetString() == "emote")
        {
            EnsureEmotes(); emotes.Receive((int)effect.GetProperty("s"u8).GetDouble(), effect.GetProperty("id"u8).GetString()!, localSeat, muteEmotes, Environment.TickCount64); return;
        }
        if (effect.GetProperty("kind"u8).GetString() != "sound") return;
        EnsureSoundboard();
        if (soundboard.Receive((int)effect.GetProperty("s"u8).GetDouble(), effect.GetProperty("id"u8).GetString()!, localSeat, audio.MuteOthers, Environment.TickCount64)) ReceivedSoundboardEffects++;
    }
    private void Soundboard()
    {
        EnsureSoundboard(); long now = Environment.TickCount64;
        bool keyboard = !ImGui.GetIO().WantTextInput;
        if (keyboard && ImGui.IsKeyPressed(ImGuiKey.G, false)) { showSoundboard = !showSoundboard; if (showSoundboard) showEmotes = false; }
        if (!showSoundboard) return;
        Window("Soundboard", size.X / 2 - 210, Top + 30, 420, 330);
        ImGui.Text("Two clips per five seconds - G to close"u8);
        try
        {
            var collection = new CosmeticCollection(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            var slots = collection.Equipped("sound")!.AsArray();
            for (int i = 0; i < 6; i++)
            {
                string? id = slots.ElementAtOrDefault(i)?.GetValue<string>();
                ImGui.BeginDisabled(IsSpectating || id is null || soundboard.Cooldown(now) > 0);
                if (Button("soundboard/" + i, $"{i + 1}: {CosmeticCollection.Item("sound", id)?.Name ?? "Empty slot"}", 370, 30) ||
                    keyboard && !IsSpectating && ImGui.IsKeyPressed((ImGuiKey)((int)ImGuiKey._1 + i), false))
                    soundboard.Send(collection, i, localSeat, now, networkRoom is null ? null : clip => networkRoom.SendEffect(JsonSerializer.SerializeToElement(new { s = localSeat, kind = "sound", id = clip })));
                ImGui.EndDisabled();
            }
            if (soundboard.Cooldown(now) > 0) ImGui.Text($"Ready in {soundboard.Cooldown(now) / 1000d:F1}s");
            ImGui.TextWrapped(IsSpectating ? "Take a seat to use your soundboard." : "Choose your six clips in Collection and store.");
        }
        catch (Exception error) when (error is JsonException or InvalidDataException or InvalidOperationException or FormatException)
        { ImGui.TextWrapped("Soundboard could not be loaded: " + error.Message); }
        if (Button("closeSoundboard", "Close")) showSoundboard = false;
        ImGui.End();
    }
}
