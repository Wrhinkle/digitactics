using DigiTactics.Networking;
using DigiTactics.Storage;

namespace DigiTactics.Presentation;

public enum SocialGroup { Party, Online, InGame, Offline, Recent }
public sealed record SocialPartyMember(ulong Id, string Name, bool Host, bool Me, bool Online);
public sealed record SocialRow(ulong Id, string Name, SocialGroup Group, string Note, ulong? JoinLobby = null, bool CanJoin = false, bool Online = true);

/// <summary>Client social column projection. Steam identity, not display name, determines friend uniqueness.</summary>
public static class SocialRoster
{
    public static IReadOnlyList<SocialRow> Build(ulong self, string selfName, ulong currentLobby, bool inMatch,
        IEnumerable<SocialPartyMember> party, IEnumerable<SteamFriendPresence> friends,
        IEnumerable<SteamFriendRoom> rooms, IEnumerable<RecentPlayer> recent)
    {
        var result = new List<SocialRow>();
        var ids = new HashSet<ulong>();
        var names = new HashSet<string>(StringComparer.Ordinal) { Clean(selfName) };
        foreach (var member in party)
        {
            if (member.Id == 0 || !ids.Add(member.Id)) continue;
            string name = Clean(member.Name); names.Add(name);
            result.Add(new(member.Id, name, SocialGroup.Party,
                (member.Host ? "Host" : "Member") + (member.Me ? " / you" : "") + (!member.Online ? " / offline" : inMatch ? " / in game" : ""), Online: member.Online));
        }
        ids.Add(self);
        var compatible = rooms.Where(room => room.Lobby.Id != 0 && room.Lobby.Data.GetValueOrDefault("dt_game") == "digitactics" &&
            room.Lobby.Data.GetValueOrDefault("dt_ver") == NetworkProtocol.Version && room.Lobby.Data.GetValueOrDefault("state") != "over" &&
            room.Lobby.Data.GetValueOrDefault("status") != "over").GroupBy(room => room.Lobby.Id).ToDictionary(group => group.Key, group => group.First());
        foreach (var friend in friends)
        {
            if (friend.Id == 0 || !ids.Add(friend.Id)) continue;
            string name = Clean(friend.Name); names.Add(name);
            if (friend.State == SteamPersonaState.Offline)
            { result.Add(new(friend.Id, name, SocialGroup.Offline, "Offline", Online: false)); continue; }
            bool hasRoom = compatible.TryGetValue(friend.LobbyId, out var room) && room.Friends.Any(person => person.Id == friend.Id);
            if (hasRoom)
            {
                bool playing = room!.Lobby.Data.GetValueOrDefault("state") == "playing" || room.Lobby.Data.GetValueOrDefault("status") == "playing";
                string note = playing ? "In game / " + Clean(room.Lobby.Data.GetValueOrDefault("round", "1-1")) : "In lobby / " + Clean(room.Lobby.Data.GetValueOrDefault("name", "Digi-Tactics"));
                ulong? join = friend.LobbyId != currentLobby ? friend.LobbyId : null;
                result.Add(new(friend.Id, name, playing ? SocialGroup.InGame : SocialGroup.Online, note, join, join.HasValue && currentLobby == 0 && !inMatch));
            }
            else if (friend.GameId != 0) result.Add(new(friend.Id, name, SocialGroup.InGame, "Playing another game"));
            else result.Add(new(friend.Id, name, SocialGroup.Online, friend.State switch
            {
                SteamPersonaState.Busy => "Do not disturb", SteamPersonaState.Away => "Away", SteamPersonaState.Snooze => "Away",
                SteamPersonaState.LookingToTrade => "Looking to trade", SteamPersonaState.LookingToPlay => "Looking to play", _ => "Online"
            }));
        }
        foreach (var player in recent.Take(20))
        {
            string name = Clean(player.Name);
            if (!names.Add(name)) continue;
            string when;
            try { when = DateTimeOffset.FromUnixTimeMilliseconds(player.At).ToLocalTime().ToString("MMM d"); }
            catch (ArgumentOutOfRangeException) { when = "previously"; }
            result.Add(new(0, name, SocialGroup.Recent, "Played together " + when, Online: false));
        }
        return result.AsReadOnly();
    }

    public static string Clean(string? value)
    {
        string text = new((value ?? "").Where(c => !char.IsControl(c)).Take(48).ToArray());
        return string.IsNullOrWhiteSpace(text) ? "Tamer" : text.Trim();
    }
}
