using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;
using System.Text.Json;

namespace DigiTactics.Desktop;

internal sealed class UpdateUiSmoke : IDisposable
{
    private readonly string directory,id=Guid.NewGuid().ToString("N");
    private int step,cooldown=4;
    private bool down,seenMatch,captured;
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
        string key=step switch {0=>"start",1=>"gameMenu",2=>"requestLeave",3=>"confirmLeave",4=>"restartUpdate",_=>throw new InvalidOperationException("Update UI restart did not close the game.")};
        if(!screen.Buttons.TryGetValue(key,out var point))return;
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;
    }
    public void Verify(MatchScreen screen,int2 size,string? capture)
    {
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
        if(!seenMatch || !captured || screen.Session is not null || request?.Session!=id || request.Version!="1.2.3")throw new InvalidOperationException("Update restart UI did not preserve session/version or menu-only flow.");
        Console.WriteLine("Update UI smoke passed: visible minimum-window restart, no mid-match restart, confirmed leave and explicit restart request.");
    }
    public void Dispose()=>Directory.Delete(directory,true);
}
