using System.Runtime.InteropServices;

namespace DigiTactics.Networking;

// Valve EPersonaState values; invisible friends are reported as offline by Steam.
public enum SteamPersonaState { Offline, Online, Busy, Away, Snooze, LookingToTrade, LookingToPlay }
public sealed record SteamFriendPresence(ulong Id, string Name, SteamPersonaState State, ulong GameId, ulong LobbyId);

public sealed partial class SteamFriends
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int PersonaStateFn(nint self, ulong friend);
    private PersonaStateFn? personaState;

    /// <summary>Read Steam's cached friend state on the owning game thread. No invites or chat are sent.
    /// See https://partner.steamgames.com/doc/api/ISteamFriends#GetFriendPersonaState.</summary>
    public IReadOnlyList<SteamFriendPresence> ListPresence()
    {
        Verify();
        personaState ??= client.Bind<PersonaStateFn>("SteamAPI_ISteamFriends_GetFriendPersonaState");
        var result = new List<SteamFriendPresence>();
        var seen = new HashSet<ulong>();
        for (int i = 0, size = Math.Clamp(count(friends, 4), 0, 2000); i < size; i++)
        {
            ulong id = byIndex(friends, i, 4);
            if (id == 0 || id == client.SteamId || !seen.Add(id)) continue;
            int state = personaState(friends, id);
            var status = state is >= 0 and <= 6 ? (SteamPersonaState)state : SteamPersonaState.Offline;
            // An offline/private persona must not acquire a visible game from stale cached data.
            GameInfo info = default;
            bool playing = status != SteamPersonaState.Offline && game(friends, id, out info);
            result.Add(new(id, Name(id), status, playing ? info.Game : 0, playing ? info.Lobby : 0));
        }
        return result.AsReadOnly();
    }
}
