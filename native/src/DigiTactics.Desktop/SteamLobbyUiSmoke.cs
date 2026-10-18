using Brutal.ImGuiApi;
using Brutal.Numerics;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using DigiTactics.Networking;
using DigiTactics.Storage;

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
    private bool down, roomVerified, matchVerified, inviteVerified, rejectVerified, rejoinVerified,browserVerified,carouselChoiceDown;
    private object? protectedMatch;
    private ulong lobby;
    private int navigationChecks;
    private bool evolutionInviteVerified;
    private bool holdSnapshot,pendingJoinCancelled;
    private int roomNameInput;
    private TaskCompletionSource<string?>? pendingBackup;
    public SteamLobbyUiSmoke(string? library=null,NativeProfileStore? profile=null)
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
              "__prepareSocialRoom","__friends","socialRefresh","codex","__codex","socialJoin/"+Peer,"__joined","steamLeave","steamBack",
              "__friends","homeRuleset","homeRuleset/faction-wars","homeHeroAction","__friends","__homePlay","socialLobby","__lobby","steamBack",
              "__prepareSocialRoom","__friends","socialRefresh","homeRuleset","homeRuleset/faction-wars","homeHeroAction","__friends","__homePlay","socialJoin/"+Peer,"__joined","steamLeave","steamBack",
              "farm","farmDigivolve","digivolve","__inviteDuringEvolution","__waitDeferredEvolution","__joined","steamLeave","steamBack","__evolutionCleaned",
              "__pendingInvite","__cancelPendingInvite","__pendingCancelled","steamBack"];
        if(callback is not null)
        {
            if(profile is null||profile.Exists)throw new InvalidOperationException("Fake lobby navigation requires a new isolated profile directory.");
            string wallet="{\"bits\":10000,\"owned\":{},\"equipped\":{}}";
            string? legends=null;
            for(int i=0;i<2;i++){var level=LegendEvolution.Apply(legends,wallet,null,"tokomon",LegendAction.BuyLevel);if(!level.Changed)throw new InvalidOperationException("Evolution invite fixture could not prepare its levels.");legends=level.Legends;wallet=level.Wallet??wallet;}
            if(!profile.UpdateStorage(new Dictionary<string,string>{{"dt.cosmetics.v1",wallet},{"dt.legendevo.v1",legends!}}))throw new InvalidOperationException(profile.Error);
        }
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
        if(screen.SettingsVisible || screen.HomePlayVisible || screen.CodexVisible || screen.SaveDataVisible || screen.ProfileVisible || screen.CollectionVisible || screen.AboutVisible || !screen.Buttons.ContainsKey("steamBack"))
            throw new InvalidOperationException($"Multiplayer navigation left the previous client screen visible at step {index} ({Pending}): Settings={screen.SettingsVisible}, HomePlay={screen.HomePlayVisible}, Codex={screen.CodexVisible}, SaveData={screen.SaveDataVisible}, Profile={screen.ProfileVisible}, Collection={screen.CollectionVisible}, About={screen.AboutVisible}, lobby controls={screen.Buttons.ContainsKey("steamBack")}.");
    }
    public void Input(MatchScreen screen)
    {
        screen.UsePrivateLobbyForSmoke();
        if(invitedRoom is not null&&!holdSnapshot)
            while(Marshal.PtrToStringUTF8(popPacket!()) is { Length: > 0 } packet)invitedRoom.Receive(Self.ToString(),packet,Environment.TickCount64);
        if (index >= commands.Length || cooldown-- > 0) return;
        string command=commands[index];
        if(carouselChoiceDown){ImGui.GetIO().AddMouseButtonEvent(0,false);carouselChoiceDown=false;cooldown=3;return;}
        if(command=="homeRuleset"&&screen.Buttons.TryGetValue("closeSocial",out var closeFriends))
        {ImGui.GetIO().AddMousePosEvent(closeFriends.X,closeFriends.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);carouselChoiceDown=true;return;}
        if(command=="leaveOnline"&&screen.Session?.Units.Carousel is {Active:true} carousel)
        {
            if(carousel.Done||carousel.Turn!=screen.Session.State.Players[1])return;
            int choice=carousel.Options.Select((option,index)=>(option,index)).First(entry=>entry.option.PickedBy is null).index;
            if(!screen.Buttons.TryGetValue("carousel/"+choice,out var card))return;
            var bounds=screen.CarouselChoicesBounds;
            if(card.Y<bounds.Min.Y+10||card.Y>bounds.Max.Y-10)
            {ImGui.GetIO().AddMousePosEvent((bounds.Min.X+bounds.Max.X)/2,(bounds.Min.Y+bounds.Max.Y)/2);ImGui.GetIO().AddMouseWheelEvent(0,card.Y<bounds.Min.Y+10?1:-1);cooldown=3;return;}
            ImGui.GetIO().AddMousePosEvent(card.X,card.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);carouselChoiceDown=true;return;
        }
        if(command=="steamCreate"&&roomNameInput<6)
        {
            var io=ImGui.GetIO();
            switch(roomNameInput)
            {
                case 0:if(!screen.Buttons.TryGetValue("steamRoomName",out var input))return;io.AddMousePosEvent(input.X,input.Y);io.AddMouseButtonEvent(0,true);break;
                case 1:io.AddMouseButtonEvent(0,false);break;
                case 2:io.AddKeyEvent(ImGuiKey.Mod_Ctrl,true);io.AddKeyEvent(ImGuiKey.A,true);break;
                case 3:io.AddKeyEvent(ImGuiKey.A,false);io.AddKeyEvent(ImGuiKey.Mod_Ctrl,false);io.AddInputCharactersUTF8("Native UI lobby"u8);break;
                case 4:io.AddKeyEvent(ImGuiKey.Enter,true);break;
                case 5:io.AddKeyEvent(ImGuiKey.Enter,false);break;
            }
            roomNameInput++;cooldown=3;return;
        }
        switch(command)
        {
            case "__pendingInvite":PrepareInvitedRoom();holdSnapshot=true;JoinRequest(lobby);NextCommand();return;
            case "__cancelPendingInvite":
                if(!screen.InSteamRoom)return;
                if(screen.SteamRoomReady)throw new InvalidOperationException("Delayed-host fixture unexpectedly supplied its initial snapshot.");
                if(!screen.Buttons.ContainsKey("steamLeave")||!screen.Buttons.ContainsKey("steamBack"))throw new InvalidOperationException("Waiting for a host snapshot removed the lobby's Cancel/Back controls.");
                command="steamLeave";break;
            case "__pendingCancelled":
                if(screen.InSteamRoom||screen.Session is not null)throw new InvalidOperationException("Cancel left the pending room attached.");
                holdSnapshot=false;invitedRoom=null;pendingJoinCancelled=true;NextCommand();return;
            case "__inviteDuringEvolution":
                if(!screen.EvolutionBusy||!screen.CollectionVisible)throw new InvalidOperationException("The invite fixture did not start its real transformation.");
                PrepareInvitedRoom();JoinRequest(lobby);NextCommand();return;
            case "__waitDeferredEvolution":
                if(screen.EvolutionBusy)
                {
                    if(screen.InSteamRoom||!screen.CollectionVisible||!screen.Buttons.ContainsKey("closeEvolution"))throw new InvalidOperationException("An accepted invite interrupted the active digivolution.");
                    evolutionInviteVerified=true;return;
                }
                if(!evolutionInviteVerified)throw new InvalidOperationException("The transformation ended before the deferred invite could be verified.");
                NextCommand();return;
            case "__evolutionCleaned":
                if(screen.CollectionVisible||screen.DisplayedEvolutionForm is not null||screen.Buttons.ContainsKey("closeFarm")||!screen.Buttons.ContainsKey("start"))throw new InvalidOperationException("Deferred invite cleanup restored a stale Farm or evolution page.");
                NextCommand();return;
            case "__joinFromInvite":PrepareInvitedRoom();JoinRequest(lobby);NextCommand();return;
            case "__prepareSocialRoom":PrepareInvitedRoom();NextCommand();return;
            case "__settings":
                if(!screen.SettingsVisible)throw new InvalidOperationException("Navigation smoke did not open Settings through its Home control.");
                NextCommand();return;
            case "__codex":
                if(!screen.CodexFullVisible)throw new InvalidOperationException("Navigation smoke did not open Codex through its Home control.");
                NextCommand();return;
            case "__homePlay":
                if(!screen.HomePlayVisible || !screen.Buttons.ContainsKey("homePlayStart") || !screen.SocialVisible)throw new InvalidOperationException("Navigation smoke did not open the match launch panel through its Home control while keeping Friends visible.");
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
        if(command=="digivolve")
        {
            var bounds=screen.EvolutionCareBounds;
            if(point.Y<bounds.Min.Y+6||point.Y>=bounds.Max.Y-6)
            {ImGui.GetIO().AddMousePosEvent(bounds.Max.X-4,(bounds.Min.Y+bounds.Max.Y)/2);ImGui.GetIO().AddMouseWheelEvent(0,point.Y<bounds.Min.Y+6?4:-4);cooldown=3;return;}
        }
        ImGui.GetIO().AddMousePosEvent(point.X, point.Y); ImGui.GetIO().AddMouseButtonEvent(0, true); down = true;
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if(Pending=="__cancelPendingInvite"&&screen.InSteamRoom&&!screen.SteamRoomReady&&capture is not null)
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"pending-host.bmp"),size.X,size.Y);
        if(index==3&&!browserVerified&&screen.Buttons.ContainsKey("steamCreate"))
        {
            foreach(string key in new[]{"steamRoomName","steamVisibility","steamSet","steamPace","steamFormat","planningSeconds/45","steamCreate","steamBack"})
                if(!screen.Buttons.TryGetValue(key,out var point)||point.Y<96||point.Y>=size.Y-12||point.X<0||point.X>=size.X)throw new InvalidOperationException("Lobby form is clipped or missing: "+key);
            if(screen.LobbyBodyFontSize<17||!screen.LobbyLayoutFits)throw new InvalidOperationException("Lobby form is below its supported readable size.");
            if(screen.Buttons["steamRoomName"].X>=screen.Buttons["steamVisibility"].X||
                screen.Buttons["steamSet"].X>=screen.Buttons["steamPace"].X||screen.Buttons["steamPace"].X>=screen.Buttons["steamFormat"].X||
                screen.Buttons["steamFormat"].X>=screen.Buttons["planningSeconds/45"].X)
                throw new InvalidOperationException("Lobby fields overlap instead of staying under their labels.");
            browserVerified=true;if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"browser.bmp"),size.X,size.Y);
        }
        if (index == 4 && screen.InSteamRoom && !roomVerified)
        {
            if(screen.DisplayedLobbyName!="Native UI lobby")throw new InvalidOperationException("The editable room name did not reach the created lobby: '"+screen.DisplayedLobbyName+"'.");
            if(screen.LobbySeatCards!=8||screen.LobbyBodyFontSize<17||!screen.LobbyLayoutFits)throw new InvalidOperationException("Lobby seat grid is clipped, overlapping or below its supported readable size.");
            foreach(string key in new[]{"steamFree/0","steamSeat/7","steamStart","steamInvite","steamLeave"})
                if(!screen.Buttons.TryGetValue(key,out var point)||point.Y<96||point.Y>=size.Y-12||point.X<0||point.X>=size.X)throw new InvalidOperationException("Lobby room control is clipped or missing: "+key);
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
        if (!browserVerified || !roomVerified || !matchVerified || screen.Session is not null || screen.InSteamRoom) throw new InvalidOperationException("Steam lobby UI did not enter and cleanly leave a native match.");
        if(callback is not null && (!inviteVerified || !rejectVerified || !rejoinVerified))throw new InvalidOperationException("Fake Steam invite, active-match guard or automatic accepted-invite join was not verified.");
        if(callback is not null && (navigationChecks!=10||!evolutionInviteVerified))throw new InvalidOperationException("Not all Settings, Codex, match launch panel, social, invite, pending-save and deferred evolution navigation routes were verified.");
        if(callback is not null&&!pendingJoinCancelled)throw new InvalidOperationException("Pending host snapshot cancellation was not verified.");
        if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
        Console.WriteLine("Steam lobby UI smoke passed: connect, private create, free/take seat, native match start, leave and return to menu."+(callback is null?"":" Fake-only overlay, current-room no-op, active-match protection, accepted-invite join and ten cross-screen navigation/pending-save/evolution routes also passed; no invitations sent.")); return true;
    }
    public void Dispose(){capturePackets?.Invoke(0);if(module!=0)NativeLibrary.Free(module);}
}
