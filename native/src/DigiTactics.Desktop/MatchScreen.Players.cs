using System.Text.Json;
using Brutal.ImGuiApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showPlayers;
    private string? muteSource, playerMessage;
    private PlayerMutes playerMutes = new(null);
    private void ReadPlayerMutes()
    {
        if (profile is null) return;
        string? source = profile.Storage.GetValueOrDefault("dt.muted");
        if (source == muteSource) return;
        muteSource = source;
        try { playerMutes = new(source); playerMessage = null; }
        catch (JsonException error) { playerMessage = "Mute preferences could not be loaded: " + error.Message; }
    }
    private bool PlayerMuted(string id) { ReadPlayerMutes(); return playerMutes.Contains(id); }
    private void TogglePlayerMute(string id)
    {
        try
        {
            if (profile is null) { playerMutes.Toggle(id); return; }
            var next = new PlayerMutes(profile.Storage.GetValueOrDefault("dt.muted"));
            string saved = next.Toggle(id);
            if (!profile.UpdateStorage(new Dictionary<string, string> { ["dt.muted"] = saved })) { playerMessage = profile.Error; return; }
            muteSource = saved; playerMutes = next; playerMessage = null;
        }
        catch (JsonException error) { playerMessage = "Mute preferences were preserved: " + error.Message; }
    }
    private void PlayersPanel()
    {
        var room = networkRoom ?? steam?.Room;
        if (!showPlayers || room is null) return;
        ReadPlayerMutes();
        Window("Players", size.X / 2 - 230, Top + 25, 460, 460);
        UiText.Wrapped("Mute hides a player's emotes and soundboard. Remove blocks them from this room."u8);
        var slots = room.Authority.Lobby?["slots"]?.AsObject();
        bool any = false;
        if (slots is not null) foreach (var slot in slots)
        {
            string? id = slot.Value?["uid"]?.GetValue<string>();
            if (id is null || id == room.Authority.LocalId) continue;
            any = true;
            ImGui.Separator(); ImGui.Text(slot.Value?["name"]?.GetValue<string>() ?? "Tamer");
            if (Button("playerMute/" + id, playerMutes.Contains(id) ? "Unmute" : "Mute")) TogglePlayerMute(id);
            if (room.Authority.IsHost)
            {
                ImGui.SameLine(); ImGui.BeginDisabled(!room.Authority.Member(id));
                if (Button("playerRemove/" + id, "Remove")) playerMessage = room.Ban(id) ? "Player removed from this room." : "Player could not be removed.";
                ImGui.EndDisabled();
            }
        }
        if (!any) ImGui.Text("No other seated players."u8);
        if (playerMessage is not null) UiText.Wrapped(playerMessage);
        if (Button("closePlayers", "Close")) showPlayers = false;
        ImGui.End();
    }
}
