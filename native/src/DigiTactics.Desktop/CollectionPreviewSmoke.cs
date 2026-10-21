using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Visits every category through real controls and checks its native preview before leaving.</summary>
internal sealed class CollectionPreviewSmoke
{
    public CollectionPreviewSmoke()
    {
        // Independent source keyframes from shopstyle.js, plus shop.js timer boundaries.
        static void Near(double actual,double expected){if(Math.Abs(actual-expected)>1e-8)throw new InvalidOperationException("Egg hatch source keyframe diverged.");}
        Near(EggArtwork.Motion(.105).Rotation,-12*Math.PI/180);Near(EggArtwork.Motion(.315).Rotation,12*Math.PI/180);
        Near(EggArtwork.Motion(1.3).X,-2);Near(EggArtwork.Motion(1.36).X,2);Near(EggArtwork.Motion(1.36).Y,-1);
        Near(EggArtwork.Motion(2.05).Scale,1);Near(EggArtwork.Motion(2.43).Scale,1.7);Near(EggArtwork.Motion(2.43).Opacity,0);
        Near(EggArtwork.RevealProgress(0),0);Near(EggArtwork.RevealProgress(.4),1);
    }
    private int category,step,wait;
    private bool down;
    private readonly PedestalPointerSmoke pointer=new(false);
    private readonly HashSet<string> verified=[];
    private string? pendingCapture;
    public bool Done {get;private set;}
    public bool Input(MatchScreen screen)
    {
        if(Done)return true;
        if(wait-->0)return false;
        var io=ImGui.GetIO();
        if(down){io.AddMouseButtonEvent(0,false);down=false;step++;wait=2;return false;}
        if(step==2)
        {
            string kind=CosmeticCollection.Kinds[category];
            if(screen.CollectionPreviewKind!=kind || screen.CollectionPreviewDrawCalls<=0)throw new InvalidOperationException("Collection omitted "+kind+" preview.");
            if(kind=="legend"&&!pointer.Input(screen))return false;
            if(kind=="killscreen" && screen.CollectionPreviewAge<1.5)return false;
            verified.Add(kind);pendingCapture=kind;
            if(kind=="killscreen")
            {
                if(screen.CollectionPreviewMembers!=10)throw new InvalidOperationException("Kill-screen store preview omitted the nine sample Digimon or equipped companion.");
                Click("previewKill/"+(screen.CollectionSelectedId??throw new InvalidOperationException("Kill-screen preview has no selected cosmetic.")));return false;
            }
            if(kind is not("sound" or "elimsound")){step=3;return false;}
            string id=screen.CollectionSelectedId??throw new InvalidOperationException("Audio preview has no selected cosmetic.");
            Click("previewSound/"+kind+"/"+id);return false;
        }
        if(step==3)
        {
            category++;step=0;
            if(category==CosmeticCollection.Kinds.Length)category=0;
            if(verified.Count==CosmeticCollection.Kinds.Length && category==0){step=4;}
            return false;
        }
        if(step==6)
        {
            if(screen.CollectionPreviewKind!="legend")throw new InvalidOperationException("Collection category did not restore after previews.");
            Done=true;Console.WriteLine("Collection previews passed: live legend, emote, board, kill theme, dance and both audio categories.");return true;
        }
        Click("collectionTab/"+category);return false;
        void Click(string key)
        {
            if(!screen.Buttons.TryGetValue(key,out var target))return;
            io.AddMousePosEvent(target.X,target.Y);io.AddMouseButtonEvent(0,true);down=true;
        }
    }
    private readonly HashSet<string> captured=[];
    public void Capture(MatchScreen screen,int2 size,string? path)
    {
        if(path is not null && pendingCapture is {} kind && captured.Add(kind))
            FrameCapture.SaveBitmap(Path.ChangeExtension(path,"collection-"+kind+".bmp"),size.X,size.Y);
        pendingCapture=null;
    }
}
