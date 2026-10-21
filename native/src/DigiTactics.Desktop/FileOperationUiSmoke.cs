using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Exercises deferred choosers and an accepted real backup write on an isolated profile.</summary>
internal sealed class FileOperationUiSmoke : IDisposable
{
    private readonly NativeProfileStore profile;
    private readonly CustomMusicController controller;
    private readonly string directory,track,backup;
    private readonly TaskCompletionSource<string[]> musicChoice=new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<string[]> abandonedChoice=new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource writerGate=new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource failureGate=new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int step,cooldown=4,musicChoices,backupChoices;
    private bool down,navigationClick;
    public FileOperationUiSmoke(MatchScreen screen,NativeProfileStore profile,GameAudio audio)
    {
        this.profile=profile;
        directory=Path.Combine(Path.GetTempPath(),"digitactics-file-operations-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        track=Path.Combine(directory,"old-selection.wav");backup=Path.Combine(directory,"accepted-backup.json");
        using(var writer=new BinaryWriter(File.Create(track)))
        {writer.Write("RIFF"u8);writer.Write(40);writer.Write("WAVEfmt "u8);writer.Write(16);writer.Write((short)1);writer.Write((short)2);writer.Write(44100);writer.Write(176400);writer.Write((short)4);writer.Write((short)16);writer.Write("data"u8);writer.Write(4);writer.Write(0);}
        controller=new(audio,profile,Path.Combine(directory,"unused.exe"));
        screen.PickMusicFiles=()=>++musicChoices==1?musicChoice.Task:abandonedChoice.Task;
        screen.BackupPicker=_=>{backupChoices++;return Task.FromResult<string?>(backup);};
        screen.BackupWriter=async(destination,settings)=>{await writerGate.Task;return await Task.Run(()=>profile.ExportBackup(destination,settings));};
    }
    public void Input(MatchScreen screen)
    {
        if(cooldown-->0||controller.LibraryBusy)return;
        if(down){ImGui.GetIO().AddMouseButtonEvent(0,false);down=false;if(!navigationClick)step++;navigationClick=false;cooldown=4;return;}
        switch(step)
        {
            case 5:
                if(screen.ResetSaveConfirmation||!screen.MusicSelectionPending)throw new InvalidOperationException("Pending hidden music chooser allowed Reset.");
                screen.RequestWindowClose();step++;cooldown=4;return;
            case 7:
                if(screen.CloseDeferred||!screen.MusicSelectionPending)throw new InvalidOperationException("Stay in game discarded pending selection.");
                if(!profile.Reset("RESET"))throw new InvalidOperationException(profile.Error);
                controller.Reload();musicChoice.SetResult([track]);step++;cooldown=4;return;
            case 8:
                if(screen.MusicSelectionPending||controller.Library?.Tracks.Count!=0||!profile.CanImport)throw new InvalidOperationException("Old chooser imported audio into fresh profile generation.");
                step++;return;
            case 12:
                if(!screen.SaveDataOperationPending||File.Exists(backup))throw new InvalidOperationException("Backup writer fixture did not remain pending before durable export.");
                screen.RequestWindowClose();step++;cooldown=4;return;
            case 14:
                if(screen.CloseDeferred||!screen.SaveDataOperationPending)throw new InvalidOperationException("Stay discarded accepted backup write.");
                writerGate.SetResult();step++;return;
            case 15:
                if(screen.SaveDataOperationPending)return;
                if(!File.Exists(backup)||new ProfileImport(backup).Summary.MusicTracks!=0)throw new InvalidOperationException("Accepted backup did not finish safely.");
                screen.BackupWriter=async(_,_)=>{await failureGate.Task;throw new IOException("Isolated failed-write fixture.");};
                step++;return;
            case 17:
                if(!screen.SaveDataOperationPending)throw new InvalidOperationException("Failed-writer fixture did not await completion.");
                screen.RequestWindowClose();failureGate.SetResult();step++;cooldown=4;return;
            case 18:
                if(screen.SaveDataOperationPending)return;
                if(screen.CloseDeferred||screen.ExitAfterFileOperationsReady||!screen.Buttons.ContainsKey("dismissFileOperationError")||new ProfileImport(backup).Summary.MusicTracks!=0)throw new InvalidOperationException("Failed backup closed the game or lost the previous backup.");
                step++;return;
            case 26:
                if(backupChoices!=2||!screen.MusicSelectionPending)throw new InvalidOperationException("Pending music chooser allowed another backup export.");
                screen.RequestWindowClose();step++;cooldown=4;return;
        }
        if(step>=28)return;
        string key=step switch
        {
            0 or 21=>"musicLibrary",1 or 22=>"addMusic",2 or 23=>"closeMusicLibrary",3 or 10 or 24=>"saveData",4=>"resetSave",
            6 or 13=>"stayAfterFileOperation",9 or 20=>"closeSaveData",11 or 16 or 25=>"exportSave",19=>"dismissFileOperationError",27=>"cancelSelectionAndClose",_=>throw new InvalidOperationException("Unexpected file operation step "+step)
        };
        // Music now lives in the original Settings page. Exercise that real route
        // without advancing the pending chooser/write scenario itself.
        if(!screen.Buttons.ContainsKey(key))
        {
            string? route=key=="musicLibrary"&&!screen.SettingsVisible?"settings"
                :key=="saveData"&&!screen.SettingsVisible?"settings":null;
            if(route is null||!screen.Buttons.TryGetValue(route,out var navigation))return;
            ImGui.GetIO().AddMousePosEvent(navigation.X,navigation.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=navigationClick=true;return;
        }
        var point=screen.Buttons[key];
        if(key=="musicLibrary"&&(point.Y<screen.SettingsScrollMin.Y+8||point.Y>screen.SettingsScrollMax.Y-18))
        {
            var hover=screen.SettingsScrollMin+new float2(100,100);
            ImGui.GetIO().AddMousePosEvent(hover.X,hover.Y);ImGui.GetIO().AddMouseWheelEvent(0,point.Y<screen.SettingsScrollMin.Y+8?3:-3);cooldown=3;return;
        }
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(step==13&&screen.CloseDeferred)
        {
            if(screen.ExitAfterFileOperationsReady||screen.Buttons.ContainsKey("cancelSelectionAndClose"))throw new InvalidOperationException("Accepted backup write was cancelable or allowed premature exit.");
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"deferred-close.bmp"),size.X,size.Y);
        }
        if(step==19&&capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"failed-close.bmp"),size.X,size.Y);
        if(step<28)return false;
        if(!screen.ExitAfterFileOperationsReady||screen.FileOperationsPending)throw new InvalidOperationException("Canceled chooser did not release deferred close.");
        abandonedChoice.SetResult([track]);
        if(profile.OpenMusicLibrary().Tracks.Count!=0||new ProfileImport(backup).Summary.MusicTracks!=0)throw new InvalidOperationException("Abandoned chooser changed current save or completed backup.");
        Console.WriteLine("File operation UI passed: hidden chooser blocks reset/export, stale generation result discarded, deferred close waits for accepted backup, Stay preserves work, failed write retains game and prior backup, canceled selection releases close.");return true;
    }
    public void Dispose()
    {
        writerGate.TrySetCanceled();failureGate.TrySetCanceled();musicChoice.TrySetResult([]);abandonedChoice.TrySetResult([]);controller.Dispose();
        string resolved=Path.GetFullPath(directory);
        if(!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase)||!Path.GetFileName(resolved).StartsWith("digitactics-file-operations-",StringComparison.Ordinal))throw new InvalidOperationException("Unsafe file operation smoke cleanup.");
        Directory.Delete(resolved,true);
    }
}
