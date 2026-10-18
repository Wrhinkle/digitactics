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
    private readonly nint module;
    private readonly CountFn? inviteCalls;
    private readonly LobbyFn? inviteLobby;
    private readonly CallbackFn? callback;
    private readonly IntFn? capturePackets;
    private readonly PointerFn? popPacket,matchmakingApi;
    private readonly InjectFn? inject;
    private readonly OwnerFn? setOwner;
    private NetworkRoom? invitedRoom;
    private const ulong Self=76561190000000001,Peer=76561190000000002;
    private readonly int originalInvites;
    private readonly string[] commands;
    private int index, cooldown = 5;
    private bool down, roomVerified, matchVerified, inviteVerified, rejectVerified, rejoinVerified;
    private object? protectedMatch;
    private ulong lobby;
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
                originalInvites=inviteCalls();
            }
            else NativeLibrary.Free(loaded);
        }
        commands=callback is null
            ?["multiplayer","steamConnect","planningSeconds/45","steamCreate","steamFree/0","steamSeat/1","steamStart","leaveOnline","confirmLeave","steamBack"]
            :["multiplayer","steamConnect","planningSeconds/45","steamCreate","steamInvite","steamFree/0","steamSeat/1","steamStart","leaveOnline","confirmLeave","steamBack","__joinFromInvite","steamLeave","steamBack"];
    }
    private void JoinRequest(ulong requested)
    {byte[] data=new byte[16];BinaryPrimitives.WriteUInt64LittleEndian(data,requested);callback!(333,data,data.Length);}
    private void PrepareInvitedRoom()
    {
        capturePackets!(1);
        if(!setOwner!(matchmakingApi!(),lobby,Peer))throw new InvalidOperationException("Fake Steam room ownership could not be set.");
        var state=new JsonObject{["host"]=Peer.ToString(),["hostName"]="Fixture host",["name"]="Invited fixture room",["status"]="lobby",
            ["setId"]="faction-wars",["pace"]="hyper",["format"]="solo",["planTime"]=45,
            ["slots"]=new JsonObject{["0"]=new JsonObject{["uid"]=Peer.ToString(),["name"]="Fixture host"}}};
        invitedRoom=new(lobby.ToString(),Peer.ToString(),state,Environment.TickCount64);
        invitedRoom.Members([Self.ToString(),Peer.ToString()],Peer.ToString(),Environment.TickCount64);
        invitedRoom.Send+=packet=>{byte[] bytes=Encoding.UTF8.GetBytes(packet.Text);inject!(Peer,bytes,bytes.Length);};
    }
    public string Pending => index < commands.Length ? commands[index] : "cleanup";
    public void Input(MatchScreen screen)
    {
        screen.UsePrivateLobbyForSmoke();
        if(invitedRoom is not null)
            while(Marshal.PtrToStringUTF8(popPacket!()) is { Length: > 0 } packet)invitedRoom.Receive(Self.ToString(),packet,Environment.TickCount64);
        if (index >= commands.Length || cooldown-- > 0) return;
        if(commands[index]=="__joinFromInvite"){PrepareInvitedRoom();JoinRequest(lobby);index++;cooldown=5;return;}
        if(callback is not null && index==12 && !screen.SteamRoomReady)return;
        if (down) { ImGui.GetIO().AddMouseButtonEvent(0, false); down = false; index++; cooldown = 5; return; }
        if (!screen.Buttons.TryGetValue(commands[index], out var point)) return;
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
        if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
        Console.WriteLine("Steam lobby UI smoke passed: connect, private create, free/take seat, native match start, leave and return to menu."+(callback is null?"":" Fake-only overlay, current-room no-op, active-match protection and accepted-invite join also passed; no invitations sent.")); return true;
    }
    public void Dispose(){capturePackets?.Invoke(0);if(module!=0)NativeLibrary.Free(module);}
}
