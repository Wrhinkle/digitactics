using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
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
        var slots = room.Authority.Lobby?["slots"]?.AsObject();
        int count=slots?.Count(slot=>slot.Value?["uid"]?.GetValue<string>() is {} id&&id!=room.Authority.LocalId)??0;
        float width=Math.Min(600,size.X-48),height=Math.Min(220+Math.Max(1,count)*52,size.Y-80);
        Window("Players backdrop",0,0,size.X,size.Y,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoFocusOnAppearing|ImGuiWindowFlags.NoBringToFrontOnFocus|ImGuiWindowFlags.NoScrollbar);
        ImGui.GetWindowDrawList().AddRectFilled(new(0,0),new(size.X,size.Y),new ImColor8(0,0,0,153));ImGui.End();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(24,22));
        ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(21,28,41));ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(82,97,118));
        Window("Players",(size.X-width)/2,(size.Y-height)/2,width,height,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar);
        ImGui.PopStyleColor(2);ImGui.PopStyleVar();
        HudText("Players",26,HudInk);
        ImGui.PushFont(default,16);ImGui.PushStyleColor(ImGuiCol.Text,HudMuted);
        UiText.Wrapped("Mute hides a player's emotes and soundboard. Remove blocks them from this lobby."u8);
        ImGui.PopStyleColor();ImGui.PopFont();ImGui.Spacing();
        ImGui.BeginChild("Room players"u8,new(0,-54));
        bool any = false;
        if (slots is not null) foreach (var slot in slots)
        {
            string? id = slot.Value?["uid"]?.GetValue<string>();
            if (id is null || id == room.Authority.LocalId) continue;
            any = true;
            var row=ImGui.GetCursorScreenPos();float available=ImGui.GetContentRegionAvail().X;
            float actionWidth=room.Authority.IsHost?198:94;
            string name=slot.Value?["name"]?.GetValue<string>()??"Tamer";
            ImGui.Dummy(new(Math.Max(100,available-actionWidth-10),36));
            ImGui.GetWindowDrawList().AddText(ImGui.GetFont(),18,row+new float2(0,8),HudInk,name,0,new float4(row.X,row.Y,row.X+available-actionWidth-10,row.Y+36));
            if(ImGui.IsItemHovered())ResultTooltip(name);
            ImGui.SameLine();
            if (HudButton("playerMute/" + id, playerMutes.Contains(id) ? "Unmute" : "Mute",94,36,playerMutes.Contains(id))) TogglePlayerMute(id);
            if (room.Authority.IsHost)
            {
                ImGui.SameLine(); ImGui.BeginDisabled(!room.Authority.Member(id));
                if (HudButton("playerRemove/" + id, "Remove",94,36)) playerMessage = room.Ban(id) ? "Player removed from this room." : "Player could not be removed.";
                ImGui.EndDisabled();
            }
            ImGui.Spacing();
        }
        if (!any) HudText("No other seated players.",16,HudMuted);
        if (playerMessage is not null) UiText.Wrapped(playerMessage);
        ImGui.EndChild();
        if (HudButton("closePlayers", "Close",100,36)) showPlayers = false;
        ImGui.SameLine();
        InviteFriendsButton("steamInvitePlayers");
        ImGui.End();
    }
}
