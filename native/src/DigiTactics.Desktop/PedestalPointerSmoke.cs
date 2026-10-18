using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>Exercises the image widget through real mouse events, including captured release outside it.</summary>
internal sealed class PedestalPointerSmoke(bool home)
{
    private int step,wait=3;
    private float2 center,outside;
    private double initialYaw,draggedYaw;
    private int initialReactions;
    public bool Done {get;private set;}
    public bool Input(MatchScreen screen)
    {
        if(Done)return true;
        if(wait-->0)return false;
        string key=home?"homeLegend":"collectionLegend";
        double yaw=home?screen.HomeLegendYaw:screen.CollectionLegendYaw;
        int reactions=home?screen.HomeLegendReactions:screen.CollectionLegendReactions;
        var io=ImGui.GetIO();
        void Check(bool valid,string message){if(!valid)throw new InvalidOperationException(key+" pointer: "+message);}
        void Near(double expected,string message)=>Check(Math.Abs(yaw-expected)<1e-7,$"{message}: {yaw:R} != {expected:R}");
        void Position(float2 point)=>io.AddMousePosEvent(point.X,point.Y);
        switch(step)
        {
            case 0:
                if(!screen.Buttons.TryGetValue(key,out center)||!screen.Buttons.TryGetValue(key+"/outside",out outside))return false;
                center=new(MathF.Floor(center.X),MathF.Floor(center.Y));outside=new(MathF.Floor(outside.X),MathF.Floor(outside.Y));
                initialYaw=yaw;initialReactions=reactions;Position(center);io.AddMouseButtonEvent(0,true);break;
            case 1:Position(center+new float2(35,0));break;
            case 2:
                Near(initialYaw+35*.012,"drag does not use source pixel-to-yaw scale");Position(outside);break;
            case 3:
                draggedYaw=initialYaw+(outside.X-center.X)*.012;
                Near(draggedYaw,"drag lost capture outside the preview");io.AddMouseButtonEvent(0,false);break;
            case 4:
                Near(draggedYaw,"release changed the final yaw");Check(reactions==initialReactions,"drag release triggered a click reaction");
                Position(center);io.AddMouseButtonEvent(0,true);break;
            case 5:io.AddMouseButtonEvent(0,false);break;
            case 6:
                Check(reactions==initialReactions+1,"click did not trigger exactly one reaction");
                Position(center);io.AddMouseButtonEvent(0,true);break;
            case 7:Position(center+new float2(4,0));break;
            case 8:Position(center);break;
            case 9:io.AddMouseButtonEvent(0,false);break;
            case 10:
                Near(draggedYaw,"out-and-back drag did not restore its starting yaw");Check(reactions==initialReactions+1,"crossing the drag threshold then returning triggered a reaction");
                Position(center);io.AddMouseButtonEvent(0,true);break;
            case 11:Position(center+new float2(3,0));break;
            case 12:io.AddMouseButtonEvent(0,false);break;
            case 13:
                Near(draggedYaw+3*.012,"small click motion lost its yaw offset");Check(reactions==initialReactions+2,"exact three-pixel click threshold diverged");
                Position(center);io.AddMouseButtonEvent(2,true);break;
            case 14:Position(center+new float2((float)((initialYaw-yaw)/.012),0));break;
            case 15:io.AddMouseButtonEvent(2,false);break;
            case 16:
                Near(initialYaw,"middle-button drag did not restore the original orientation");Check(reactions==initialReactions+2,"middle-button drag triggered a click reaction");
                Done=true;Console.WriteLine($"{key} pointer passed: rotation, outside capture/release, click reaction, sticky drag threshold, three-pixel click and middle-button drag.");return true;
        }
        step++;wait=2;return false;
    }
}
