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
        float width=Math.Min(620,size.X-48),height=Math.Min(580,size.Y-80);
        Window("Players",(size.X-width)/2,(size.Y-height)/2,width,height,ImGuiWindowFlags.NoTitleBar);
        HudText("PLAYERS",22,HudGold);
        HudText("Mute a player's emotes and soundboard. Hosts can remove players from the room.",16,HudMuted);
        ImGui.Separator();
        ImGui.BeginChild("Room players"u8,new(0,height-190));
        var slots = room.Authority.Lobby?["slots"]?.AsObject();
        bool any = false;
        if (slots is not null) foreach (var slot in slots)
        {
            string? id = slot.Value?["uid"]?.GetValue<string>();
            if (id is null || id == room.Authority.LocalId) continue;
            any = true;
            HudText(slot.Value?["name"]?.GetValue<string>() ?? "Tamer",18);
            if (HudButton("playerMute/" + id, playerMutes.Contains(id) ? "Unmute" : "Mute",120,38,playerMutes.Contains(id))) TogglePlayerMute(id);
            if (room.Authority.IsHost)
            {
                ImGui.SameLine(); ImGui.BeginDisabled(!room.Authority.Member(id));
                if (HudButton("playerRemove/" + id, "Remove",120)) playerMessage = room.Ban(id) ? "Player removed from this room." : "Player could not be removed.";
                ImGui.EndDisabled();
            }
            ImGui.Separator();
        }
        if (!any) HudText("No other seated players.",16,HudMuted);
        ImGui.EndChild();
        InviteFriendsButton("steamInvitePlayers");
        if (playerMessage is not null) UiText.Wrapped(playerMessage);
        if (HudButton("closePlayers", "Back",140)) showPlayers = false;
        ImGui.End();
    }
}
