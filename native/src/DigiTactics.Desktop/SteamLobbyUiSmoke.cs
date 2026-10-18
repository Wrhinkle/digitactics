using Brutal.ImGuiApi;
using Brutal.Numerics;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using DigiTactics.Networking;

namespace DigiTactics.Desktop;

internal sealed class SteamLobbyUiSmoke : IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int CountFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ulong LobbyFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void CallbackFn(int id,byte[] bytes,int size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void IntFn(int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint PointerFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void InjectFn(ulong peer,byte[] bytes,int size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool OwnerFn(nint api,ulong lobby,ulong owner);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool DataFn(nint api,ulong lobby,[MarshalAs(UnmanagedType.LPUTF8Str)]string key,[MarshalAs(UnmanagedType.LPUTF8Str)]string value);
    private readonly nint module;
    private readonly CountFn? inviteCalls;
    private readonly LobbyFn? inviteLobby;
    private readonly CallbackFn? callback;
    private readonly IntFn? capturePackets;
    private readonly PointerFn? popPacket,matchmakingApi;
    private readonly InjectFn? inject;
    private readonly OwnerFn? setOwner;
    private readonly DataFn? setData;
    private NetworkRoom? invitedRoom;
    private const ulong Self=76561190000000001,Peer=76561190000000002;
    private readonly int originalInvites;
    private readonly string[] commands;
    private int index, cooldown = 5;
    private bool down, roomVerified, matchVerified, inviteVerified, rejectVerified, rejoinVerified;
    private object? protectedMatch;
    private ulong lobby;
    private int navigationChecks;
    private TaskCompletionSource<string?>? pendingBackup;
    public SteamLobbyUiSmoke(string? library=null)
    {
        if(library is not null && File.Exists(library))
        {
            nint loaded=NativeLibrary.Load(library);
            if(NativeLibrary.TryGetExport(loaded,"FakeSteam_InviteOverlayCalls",out nint calls))
            {
                module=loaded;inviteCalls=Marshal.GetDelegateForFunctionPointer<CountFn>(calls);
                inviteLobby=Marshal.GetDelegateForFunctionPointer<LobbyFn>(NativeLibrary.GetExport(module,"FakeSteam_InviteOverlayLobby"));
                callback=Marshal.GetDelegateForFunctionPointer<CallbackFn>(NativeLibrary.GetExport(module,"FakeSteam_Callback"));
                capturePackets=Marshal.GetDelegateForFunctionPointer<IntFn>(NativeLibrary.GetExport(module,"FakeSteam_CaptureSent"));
                popPacket=Marshal.GetDelegateForFunctionPointer<PointerFn>(NativeLibrary.GetExport(module,"FakeSteam_PopSent"));
                inject=Marshal.GetDelegateForFunctionPointer<InjectFn>(NativeLibrary.GetExport(module,"FakeSteam_Inject"));
                matchmakingApi=Marshal.GetDelegateForFunctionPointer<PointerFn>(NativeLibrary.GetExport(module,"SteamAPI_SteamMatchmaking_v009"));
                setOwner=Marshal.GetDelegateForFunctionPointer<OwnerFn>(NativeLibrary.GetExport(module,"SteamAPI_ISteamMatchmaking_SetLobbyOwner"));
                setData=Marshal.GetDelegateForFunctionPointer<DataFn>(NativeLibrary.GetExport(module,"SteamAPI_ISteamMatchmaking_SetLobbyData"));
                originalInvites=inviteCalls();
            }
            else NativeLibrary.Free(loaded);
        }
        commands=callback is null
            ?["multiplayer","steamConnect","planningSeconds/45","steamCreate","steamFree/0","steamSeat/1","steamStart","leaveOnline","confirmLeave","steamBack"]
            :["multiplayer","steamConnect","planningSeconds/45","steamCreate","steamInvite","steamFree/0","steamSeat/1","steamStart","leaveOnline","confirmLeave","steamBack","__joinFromInvite","steamLeave","steamBack",
              "__friends","settings","__settings","setMotion","setMotion","socialLobby","__lobby","steamBack",
              "__friends","codex","__codex","socialLobby","__lobby","steamBack",
              "settings","__settings","__joinFromInvite","__joined","steamLeave","steamBack",
              "codex","__codex","__joinFromInvite","__joined","steamLeave","steamBack",
              "saveData","__prepareBackup","exportSave","__inviteWithBackup","__joinedWithBackup","__backupCanceled","steamLeave","steamBack",
              "__prepareSocialRoom","__friends","socialRefresh","settings","__settings","socialJoin/"+Peer,"__joined","steamLeave","steamBack",
              "__prepareSocialRoom","__friends","socialRefresh","codex","__codex","socialJoin/"+Peer,"__joined","steamLeave","steamBack"];
    }
    private void JoinRequest(ulong requested)
    {byte[] data=new byte[16];BinaryPrimitives.WriteUInt64LittleEndian(data,requested);callback!(333,data,data.Length);}
    private void PrepareInvitedRoom()
    {
        capturePackets!(1);
        if(!setOwner!(matchmakingApi!(),lobby,Peer))throw new InvalidOperationException("Fake Steam room ownership could not be set.");
        foreach(var pair in new Dictionary<string,string>{{"dt_game","digitactics"},{"dt_ver",NetworkProtocol.Version},{"state","lobby"},{"status","lobby"},{"name","Invited fixture room"}})
            if(!setData!(matchmakingApi!(),lobby,pair.Key,pair.Value))throw new InvalidOperationException("Fake Steam room metadata could not be set.");
        var state=new JsonObject{["host"]=Peer.ToString(),["hostName"]="Fixture host",["name"]="Invited fixture room",["status"]="lobby",
            ["setId"]="faction-wars",["pace"]="hyper",["format"]="solo",["planTime"]=45,
            ["slots"]=new JsonObject{["0"]=new JsonObject{["uid"]=Peer.ToString(),["name"]="Fixture host"}}};
        invitedRoom=new(lobby.ToString(),Peer.ToString(),state,Environment.TickCount64);
        invitedRoom.Members([Self.ToString(),Peer.ToString()],Peer.ToString(),Environment.TickCount64);
        invitedRoom.Send+=packet=>{byte[] bytes=Encoding.UTF8.GetBytes(packet.Text);inject!(Peer,bytes,bytes.Length);};
    }
    public string Pending => index < commands.Length ? commands[index] : "cleanup";
    private void NextCommand(){index++;cooldown=5;}
    private void AssertLobbyView(MatchScreen screen)
    {
        if(screen.SettingsVisible || screen.CodexVisible || screen.SaveDataVisible || screen.ProfileVisible || screen.CollectionVisible || screen.AboutVisible || !screen.Buttons.ContainsKey("steamBack"))
            throw new InvalidOperationException($"Multiplayer navigation left the previous client screen visible at step {index} ({Pending}): Settings={screen.SettingsVisible}, Codex={screen.CodexVisible}, SaveData={screen.SaveDataVisible}, Profile={screen.ProfileVisible}, Collection={screen.CollectionVisible}, About={screen.AboutVisible}, lobby controls={screen.Buttons.ContainsKey("steamBack")}.");
    }
    public void Input(MatchScreen screen)
    {
        screen.UsePrivateLobbyForSmoke();
        if(invitedRoom is not null)
            while(Marshal.PtrToStringUTF8(popPacket!()) is { Length: > 0 } packet)invitedRoom.Receive(Self.ToString(),packet,Environment.TickCount64);
        if (index >= commands.Length || cooldown-- > 0) return;
        string command=commands[index];
        switch(command)
        {
            case "__joinFromInvite":PrepareInvitedRoom();JoinRequest(lobby);NextCommand();return;
            case "__prepareSocialRoom":PrepareInvitedRoom();NextCommand();return;
            case "__settings":
                if(!screen.SettingsVisible)throw new InvalidOperationException("Navigation smoke did not open Settings through its Home control.");
                NextCommand();return;
            case "__codex":
                if(!screen.CodexFullVisible)throw new InvalidOperationException("Navigation smoke did not open Codex through its Home control.");
                NextCommand();return;
            case "__lobby":AssertLobbyView(screen);navigationChecks++;NextCommand();return;
            case "__joined":
                if(!screen.SteamRoomReady)return;
                AssertLobbyView(screen);
                if(screen.SteamRoomIdentity!=lobby || screen.Session is not null)throw new InvalidOperationException("Cross-screen navigation joined the wrong room.");
                navigationChecks++;NextCommand();return;
            case "__prepareBackup":
                if(!screen.SaveDataVisible)throw new InvalidOperationException("Navigation smoke did not open Save data through its Home control.");
                pendingBackup=new(TaskCreationOptions.RunContinuationsAsynchronously);
                screen.BackupPicker=_=>pendingBackup.Task;NextCommand();return;
            case "__inviteWithBackup":
                if(!screen.SaveDataOperationPending)throw new InvalidOperationException("The export control did not start the isolated pending chooser.");
                PrepareInvitedRoom();JoinRequest(lobby);NextCommand();return;
            case "__joinedWithBackup":
                if(!screen.SteamRoomReady)return;
                AssertLobbyView(screen);
                if(!screen.SaveDataOperationPending || pendingBackup!.Task.IsCompleted)throw new InvalidOperationException("Navigation discarded a pending save operation.");
                pendingBackup.SetResult(null);NextCommand();return;
            case "__backupCanceled":
                if(screen.SaveDataOperationPending)return;
                if(screen.SaveDataStatus!="Backup canceled. Your save was not changed.")throw new InvalidOperationException("The hidden save panel did not finish its pending chooser safely.");
                navigationChecks++;NextCommand();return;
            case "__friends":
                if(!down && screen.SocialVisible){NextCommand();return;}
                command="socialToggle";break;
        }
        if(callback is not null && index==12 && !screen.SteamRoomReady)return;
        if (down) { ImGui.GetIO().AddMouseButtonEvent(0, false); down = false; NextCommand(); return; }
        if(command.StartsWith("socialJoin/",StringComparison.Ordinal) && !screen.DisplayedSocialRows.Any(row=>row.Id==Peer && row.CanJoin))return;
        if (!screen.Buttons.TryGetValue(command, out var point)) return;
        ImGui.GetIO().AddMousePosEvent(point.X, point.Y); ImGui.GetIO().AddMouseButtonEvent(0, true); down = true;
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if (index == 4 && screen.InSteamRoom && !roomVerified)
        {
            roomVerified = true;
            lobby=screen.SteamRoomIdentity;
            if(callback is not null)JoinRequest(lobby); // An invite to the current room must be a no-op.
            if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "lobby.bmp"), size.X, size.Y);
        }
        if(callback is not null && index==5 && !inviteVerified && cooldown<3)
        {
            if(inviteCalls!()!=originalInvites+1 || inviteLobby!()!=lobby || screen.SteamRoomIdentity!=lobby)throw new InvalidOperationException("Fake Steam invite dialog did not target the current lobby exactly once.");
            inviteVerified=true;
        }
        if (index == (callback is null?7:8) && screen.Session is not null && screen.NetworkStateHash is not null)
        {
            if (!screen.Session.State.Players[1].IsHuman || screen.Session.State.Players[0].IsHuman) throw new InvalidOperationException("Steam lobby seat switch did not reach the match.");
            if (callback is not null && screen.Session.State.Players[1].Name != "Native Test Tame") throw new InvalidOperationException("The Steam persona name did not take precedence over the saved profile name.");
            if (screen.NetworkPlanningDuration != 45) throw new InvalidOperationException("Lobby planning duration did not reach match initialization.");
            if(!matchVerified && callback is not null){protectedMatch=screen.Session;JoinRequest(lobby+1);}
            matchVerified = true;
            if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "match.bmp"), size.X, size.Y);
        }
        if(callback is not null && index==8 && matchVerified && cooldown<3)
        {
            if(!ReferenceEquals(protectedMatch,screen.Session) || screen.SteamRoomIdentity!=lobby)throw new InvalidOperationException("An accepted Steam invite replaced an active match.");
            rejectVerified=true;
        }
        if(callback is not null && index==12 && screen.SteamRoomReady)rejoinVerified=screen.SteamRoomIdentity==lobby && screen.Session is null;
        if (index < commands.Length) return false;
        if (!roomVerified || !matchVerified || screen.Session is not null || screen.InSteamRoom) throw new InvalidOperationException("Steam lobby UI did not enter and cleanly leave a native match.");
        if(callback is not null && (!inviteVerified || !rejectVerified || !rejoinVerified))throw new InvalidOperationException("Fake Steam invite, active-match guard or automatic accepted-invite join was not verified.");
        if(callback is not null && navigationChecks!=7)throw new InvalidOperationException("Not all Settings, Codex, social, invite and pending-save navigation routes were verified.");
        if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
        Console.WriteLine("Steam lobby UI smoke passed: connect, private create, free/take seat, native match start, leave and return to menu."+(callback is null?"":" Fake-only overlay, current-room no-op, active-match protection, accepted-invite join and seven cross-screen navigation/pending-save routes also passed; no invitations sent.")); return true;
    }
    public void Dispose(){capturePackets?.Invoke(0);if(module!=0)NativeLibrary.Free(module);}
}
