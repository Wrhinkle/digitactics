using Brutal.ImGuiApi;
using DigiTactics.Networking;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool borrowedSteam;
    internal ulong SteamRoomIdentity=>ulong.TryParse(steam?.Room?.Authority.RoomId,out ulong id)?id:0;
    internal bool SteamRoomReady=>enteringRoom is null && steam?.Room?.Authority.Lobby is not null;
    internal void AdoptSteam(SteamRoomSession session,bool borrowed=false)
    {
        if(steam is not null)throw new InvalidOperationException("Steam is already connected.");
        steam=session;
        borrowedSteam=borrowed;
        steam.ConnectionLost+=reason=>{lobbyMessage=reason;EndOnlineView();};
        steam.JoinRequested+=(lobby,_)=>ReceiveSteamJoin(lobby);
        lobbyMessage="Connected as "+steam.Name;
        RefreshSteamRooms();
    }
    private void DisposeSteamConnection(){if(!borrowedSteam)steam?.Dispose();}
    internal void SteamStartupError(string? error) {if(error is not null)lobbyMessage=error;}
    private void OpenMultiplayerView()
    {
        if(ClientNavigationBlocked)return;
        if(PostgameClientOverlay&&!PostgameHomeVisible)DismissResults();
        if(evolutionLegend is not null)CloseEvolution();
        // Every entry route leaves the current client module, like the original
        // client's play-page navigation. Async save/file work keeps completing
        // in Update even when its panel is no longer visible.
        showMultiplayer=true;
        showProfile=showFarm=showCollection=showAbout=showSettings=showSaveData=showHomePlay=false;
        codexFull=codexSide=false;codexUnit=codexModalLine=null;
        socialDrawer=showMusicLibrary=showAudio=showPlayers=showEmotes=showSoundboard=false;
        confirmResetSave=false;resetSaveText.SetValue("".AsSpan());
        CancelArenaDrag();cameraDrag=null;
    }
    internal void ReceiveSteamJoin(ulong lobby)
    {
        if(lobby==0 || steam?.Room?.Authority.RoomId==lobby.ToString(System.Globalization.CultureInfo.InvariantCulture))return;
        // Match the original accepted-invite rule: never discard an active match.
        if(Session is not null && !(PostgameHomeVisible && networkRoom is null) || training is not null)
        {status=lobbyMessage=profileMessage="Leave your current game first, then accept the invite again.";return;}
        if(PostgameHomeVisible)LeaveMatch();
        invitedLobby=lobby;
        OpenMultiplayerView();
        if(steam is null)ConnectSteam();
    }
    private void ApplySteamJoinRequest()
    {
        // An accepted invite waits for the visible transformation to settle;
        // retaining invitedLobby also preserves callbacks received mid-animation.
        if(invitedLobby is not {} lobby || steam is null || enteringRoom is not null || steam.IsEntering || EvolutionBusy)return;
        invitedLobby=null;
        if(steam.Room?.Authority.RoomId==lobby.ToString(System.Globalization.CultureInfo.InvariantCulture))return;
        if(Session is not null || training is not null)
        {status=lobbyMessage="Leave your current game first, then accept the invite again.";return;}
        OpenMultiplayerView();
        if(steam.Room is not null)LeaveSteamRoom();
        JoinSteamRoom(lobby);
    }
    private void InviteFriendsButton(string key)
    {
        if(steam?.Room is not {} room || !HomeButton(key,"Invite Steam friends",200,40))return;
        try
        {
            string message=steam.Friends.OpenInviteOverlay(ulong.Parse(room.Authority.RoomId,System.Globalization.CultureInfo.InvariantCulture))
                ?"Choose friends in the Steam overlay."
                :"Steam overlay is not available yet. Enable the overlay in Steam and launch the game through Steam, then try again.";
            lobbyMessage=playerMessage=message;
        }
        catch(Exception error) when(error is InvalidOperationException or ArgumentException)
        {lobbyMessage=playerMessage="The Steam invite dialog could not be opened: "+error.Message;}
    }
}
