using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;
using System.Text.Json;

namespace DigiTactics.Desktop;

internal sealed class UpdateUiSmoke : IDisposable
{
    private readonly string directory,id=Guid.NewGuid().ToString("N");
    private int step,cooldown=4;
    private bool down,seenMatch,captured,deferredVerified,seenSettings;
    private readonly CarouselFlowSmoke carousel = new();
    public string Pending => $"Update UI step {step}; {carousel.Pending}";
    public UpdateUiSmoke(MatchScreen screen)
    {
        directory=Path.Combine(Path.GetTempPath(),"digitactics-update-ui-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory,"status.json"),JsonSerializer.Serialize(new NativeUpdateStatus(id,"ready","1.2.3"),new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase}));
        screen.UpdateClient=new(directory,id);
    }
    public void Input(MatchScreen screen)
    {
        if(cooldown-->0)return;
        if(down) {ImGui.GetIO().AddMouseButtonEvent(0,false);down=false;step++;cooldown=4;return;}
        if(step==1&&!deferredVerified)
        {
            if(screen.Buttons.ContainsKey("restartUpdate")||screen.Buttons.ContainsKey("deferUpdate"))throw new InvalidOperationException("Later did not dismiss the update notification.");
            deferredVerified=true;
            File.WriteAllText(Path.Combine(directory,"status.json"),JsonSerializer.Serialize(new NativeUpdateStatus(id,"ready","1.2.4"),new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase}));
            cooldown=4;return;
        }
        if(step==1&&!screen.Buttons.ContainsKey("restartUpdate"))return; // Wait for the client's real 500ms status polling interval.
        if(step==6 && screen.MatchLoadingVisible)return;
        if(step==6 && screen.Session?.Units.Carousel.Active==true){carousel.Input(screen);cooldown=3;return;}
        string key=step switch {0=>"deferUpdate",1=>"settings",2=>"homeOverview",3=>"clientPlay",4=>"homePlayConfirm",5=>"homePlayStart",6=>"gameMenu",7=>"requestLeave",8=>"confirmLeave",9=>"restartUpdate",_=>throw new InvalidOperationException("Update UI restart did not close the game.")};
        if(!screen.Buttons.TryGetValue(key,out var point)||!screen.RevealHomePlayControl(key))return;
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;
    }
    public void Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(screen.SettingsVisible)
        {
            if(!screen.Buttons.ContainsKey("restartUpdate"))throw new InvalidOperationException("Update notification disappeared outside Home.");
            seenSettings=true;
        }
        if(screen.Session is not null)
        {
            seenMatch=true;
            if(screen.Buttons.ContainsKey("restartUpdate"))throw new InvalidOperationException("Restart update was offered during a match.");
        }
        if(!captured && screen.Buttons.TryGetValue("restartUpdate",out var point))
        {
            if(point.X<0 || point.Y<0 || point.X>=size.X || point.Y>=size.Y)throw new InvalidOperationException("Update restart button is outside the window.");
            if(capture is not null)FrameCapture.SaveBitmap(capture,size.X,size.Y);captured=true;
        }
    }
    public void VerifyRestart(MatchScreen screen)
    {
        var request=JsonSerializer.Deserialize<NativeRestartRequest>(File.ReadAllBytes(Path.Combine(directory,"restart.json")),new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
        if(!deferredVerified || !seenSettings || !seenMatch || !captured || screen.Session is not null || request?.Session!=id || request.Version!="1.2.4")throw new InvalidOperationException("Update restart UI did not preserve session/version or menu-only flow.");
        Console.WriteLine("Update UI smoke passed: compact visible notification, Later dismisses current version, new version reappears, no mid-match restart, confirmed leave and explicit restart request.");
    }
    public void Dispose()=>Directory.Delete(directory,true);
}
