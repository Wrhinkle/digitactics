using System.Runtime.InteropServices;
using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Drives real social/settings controls against the fake Steam DLL; never uses live Steam.</summary>
internal sealed class SocialUiSmoke : IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void ModeFn(int mode);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int CountFn();
    private readonly nint module;
    private readonly ModeFn mode;
    private readonly CountFn invites;
    private readonly NativeProfileStore profile;
    private readonly string[] commands;
    private readonly int initialStep, refreshedStep, hiddenStep, originalInvites;
    private int step, cooldown = 6;
    private bool down, checkedInitial, checkedRefresh, checkedHidden;
    public SocialUiSmoke(NativeProfileStore profile, string? library, bool minimum)
    {
        if (profile.Exists || library is null) throw new InvalidOperationException("Social UI smoke requires a new isolated profile and --steam-api fake DLL.");
        module = NativeLibrary.Load(library);
        try
        {
            mode = Marshal.GetDelegateForFunctionPointer<ModeFn>(NativeLibrary.GetExport(module,"FakeSteam_PersonaMode"));
            invites = Marshal.GetDelegateForFunctionPointer<CountFn>(NativeLibrary.GetExport(module,"FakeSteam_InviteOverlayCalls"));
            originalInvites = invites(); mode(0);
        }
        catch { NativeLibrary.Free(module); throw new InvalidOperationException("Social UI smoke requires the fake Steam API, never a live library."); }
        this.profile=profile;
        if(!profile.UpdateStorage(new Dictionary<string,string>{{"dt.name","Social smoke"},{"dt.client.v1","{\"prefs\":{\"social\":true},\"recent\":[{\"name\":\"Recent tamer\",\"at\":0}]}"}}))throw new InvalidOperationException(profile.Error);
        commands = (minimum ? new[]{"socialToggle"} : []).Concat(new[]{"socialConnect","socialGroup/Offline","socialRefresh","settings","setSocial","closeSettings","settings","setSocial","closeSettings"}).Concat(minimum ? new[]{"socialToggle"} : []).ToArray();
        initialStep=Array.IndexOf(commands,"socialConnect")+1;refreshedStep=Array.IndexOf(commands,"socialRefresh")+1;hiddenStep=Array.IndexOf(commands,"setSocial")+1;
    }
    public string Pending=>commands.ElementAtOrDefault(step)??"verification";
    public void Input(MatchScreen screen)
    {
        if(cooldown-->0 || step>=commands.Length)return;
        if(down){ImGui.GetIO().AddMouseButtonEvent(0,false);down=false;step++;cooldown=6;return;}
        string key=commands[step];
        if(!screen.Buttons.TryGetValue(key,out var point))return;
        if(point.X<0 || point.Y<0 || point.X>=ImGui.GetIO().DisplaySize.X || point.Y>=ImGui.GetIO().DisplaySize.Y)throw new InvalidOperationException("Social control outside supported window: "+key);
        if(key=="socialRefresh")mode(1);
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(step==initialStep && cooldown==1)
        {
            if(!screen.SocialVisible || !screen.HomeLayoutFits || !screen.DisplayedSocialRows.Any(row=>row.Group==SocialGroup.InGame) || !screen.DisplayedSocialRows.Any(row=>row.Group==SocialGroup.Offline))
            {
                if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"initial-failure.bmp"),size.X,size.Y);
                throw new InvalidOperationException($"Social panel omitted native game/offline states or broke the supported Home layout: visible={screen.SocialVisible}, fits={screen.HomeLayoutFits}, rows={string.Join(';',screen.DisplayedSocialRows.Select(row=>row.Group+":"+row.Name))}.");
            }
            checkedInitial=true;
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"initial.bmp"),size.X,size.Y);
        }
        if(step==refreshedStep && cooldown==1)
        {
            if(!screen.DisplayedSocialRows.Any(row=>row.Group==SocialGroup.Online&&row.Note=="Away") || screen.DisplayedSocialRows.Any(row=>row.Group==SocialGroup.InGame))throw new InvalidOperationException("Manual social refresh did not reflect changed native persona state.");
            checkedRefresh=true;
        }
        if(step==hiddenStep && cooldown==1)
        {
            using var saved=JsonDocument.Parse(profile.Storage["dt.client.v1"]);
            if(screen.SocialVisible || saved.RootElement.GetProperty("prefs"u8).GetProperty("social"u8).GetBoolean())throw new InvalidOperationException("Friends preference did not hide and persist the social column.");
            checkedHidden=true;
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"hidden.bmp"),size.X,size.Y);
        }
        if(step<commands.Length || cooldown>0)return false;
        using var final=JsonDocument.Parse(profile.Storage["dt.client.v1"]);
        if(!checkedInitial || !checkedRefresh || !checkedHidden || !screen.SocialVisible || !screen.HomeLayoutFits || !final.RootElement.GetProperty("prefs"u8).GetProperty("social"u8).GetBoolean() || invites()!=originalInvites)throw new InvalidOperationException("Social UI flow did not restore the visible preference without invitations.");
        if(capture is not null)FrameCapture.SaveBitmap(capture,size.X,size.Y);
        Console.WriteLine("Social UI passed: native online/offline/game presence, refresh, grouped rows, supported layout, settings persistence, hide/show and no invitations.");return true;
    }
    public void Dispose(){mode(0);NativeLibrary.Free(module);}
}
