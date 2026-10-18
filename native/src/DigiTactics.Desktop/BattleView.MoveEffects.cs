using System.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class BattleView
{
    private NativeMoveEffects? moveEffects;
    private CombatMovePlayback? combatMoves;
    private double moveShakeAmplitude,moveShakeRemaining;
    private readonly Random moveShakeRandom=new(84219);
    private Vector3 moveShakeOffset;
    internal int MoveCastCount=>combatMoves?.Casts??0;
    internal int MoveBasicCount=>combatMoves?.Basics??0;
    internal int MoveEquipmentCount=>combatMoves?.EquipmentCues??0;
    internal int MoveParticleCount=>(moveEffects?.Effects.Add.Count??0)+(moveEffects?.Effects.Alpha.Count??0);
    private void InitializeMoveEffects()
    {
        moveEffects=new(rules.Id);
        moveEffects.Effects.Shake=(amplitude,duration)=>{moveShakeAmplitude=Math.Max(moveShakeRemaining>0?moveShakeAmplitude:0,amplitude);moveShakeRemaining=Math.Max(moveShakeRemaining,duration);};
        combatMoves=new(Combat,moveEffects.Playback,id=>actors.TryGetValue(id,out var actor)?actor.Model.Rig.Animations.FirstOrDefault(c=>c.Name=="special")?.Duration:null);
        combatMoves.Consume(mirror);
    }
    private void AdvanceMoveEffects(double delta)
    {
        moveShakeRemaining=Math.Max(0,moveShakeRemaining-delta);combatMoves!.Consume(mirror);moveEffects!.Advance(delta);
        double a=moveShakeAmplitude*Math.Min(1,moveShakeRemaining*4);
        moveShakeOffset=moveShakeRemaining>0?new((float)((moveShakeRandom.NextDouble()*2-1)*a),(float)((moveShakeRandom.NextDouble()*2-1)*a*.6),(float)((moveShakeRandom.NextDouble()*2-1)*a)):Vector3.Zero;
    }
    private Matrix4x4 MoveCamera(Matrix4x4 camera)=>Matrix4x4.CreateTranslation(-moveShakeOffset)*camera;
    private void DrawMoveEffects(Matrix4x4 camera,int width,int height)
    {
        var eye=board.Camera.Eye(width,height,Combat.Columns)+moveShakeOffset;moveEffects!.Effects.CameraPosition.Set(eye.X,eye.Y,eye.Z);
        DrawCalls+=moveEffects.Draw(renderer,camera,width,height,includeLit:false);
    }
}
