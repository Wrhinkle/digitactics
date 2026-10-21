using System.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Screen anchors for the original .ulabel overlays and floating combat text.</summary>
internal sealed partial class BattleView
{
    internal readonly record struct LabelAnchor(CombatUnit Unit,CombatVisual View,Brutal.Numerics.float2 Head);
    // render3d.js: STAGE_HEIGHT[stage] (+.15 for tier 5) plus the .35 the label sits above the head.
    internal static double LabelHeight(int stage,int tier)=>(stage<=0?1.3:stage==1?1.65:2.0)+(tier==5?.15:0)+.35;
    internal bool IsAlly(CombatUnit unit)=>unit.Team==(mirror?1:0);

    internal Brutal.Numerics.float2? ProjectWorld(double x,double y,double z,int width,int height,int windowHeight,int left,int bottom)
    {
        var camera=board.Camera.Matrix(width,height,Combat.Columns);
        var point=Vector4.Transform(new Vector4((float)x,(float)y,(float)z,1),camera);
        if(point.W<=0)return null;
        return new(left+(point.X/point.W*.5f+.5f)*width,windowHeight-bottom-(point.Y/point.W*.5f+.5f)*height);
    }
    internal IEnumerable<LabelAnchor> LabelAnchors(int width,int height,int windowHeight,int left,int bottom)
    {
        foreach(var view in choreography.Views)
        {
            var unit=Combat.Units[view.UnitId];if(!unit.Alive)continue;
            if(ProjectWorld(view.Position.X,view.Position.Y+LabelHeight(unit.Stage,unit.Line.Tier),view.Position.Z,width,height,windowHeight,left,bottom) is {} head)
                yield return new(unit,view,head);
        }
    }
    internal WorldPosition BenchPosition(int index)=>CombatLayout.Bench(index,Combat.Columns>7);
    /// <summary>The unit's current board position, or its layout cell once its view is gone.</summary>
    internal WorldPosition UnitPosition(int id)
    {
        foreach(var view in choreography.Views)if(view.UnitId==id)return view.Position;
        return CombatLayout.Unit(Combat.Units[id],Combat.Columns,mirror);
    }
}
