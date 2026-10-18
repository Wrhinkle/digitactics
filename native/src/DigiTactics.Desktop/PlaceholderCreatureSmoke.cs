using System.Numerics;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal static class PlaceholderCreatureSmoke
{
    // Runs in the existing cinematic smoke's GL context. All current packaged
    // forms have models, so unassign one in an isolated catalog copy to exercise
    // the original fallback through both production roster paths.
    public static void Run(int2 size,string? capture)
    {
        var catalog=ContentCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/catalog.json"));
        var models=ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/models.json"));
        var missing=(Rules:catalog.Variants[0],Line:catalog.Variants[0].Lines[0],Stage:0);
        models.Variants[missing.Rules.Id][missing.Line.Id][missing.Stage]=null;
        using(var renderer=new ModelRenderer{CosmeticStage=true,FarmEye=new(0,1.1f,3)})
        using(var roster=new RosterModelView(models))
        {
            roster.Update(missing.Rules,[(new PlanningUnit(987,missing.Line.Id,missing.Stage,[]),new WorldPosition())],0);
            var camera=Matrix4x4.CreateLookAt(new(0,1.1f,3),new(0,.45f,0),Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(.6f,(float)size.X/size.Y,.1f,60);
            ModelRenderer.Begin(size.X,size.Y);OpenGl.ClearColor(new(.02f,.03f,.05f,1));OpenGl.Clear(GlClearBit.Color|GlClearBit.Depth);
            int calls=roster.Draw(renderer,camera,false);ModelRenderer.End();
            if(calls!=11||!roster.RenderedUnits.Contains(987))throw new InvalidOperationException("Missing-model roster actor disappeared.");
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"fallback-roster.bmp"),size.X,size.Y);
            roster.DraggedUnit=987;roster.DragPosition=new(.5,0,0);roster.Draw(renderer,camera,false);ModelRenderer.End();
        }
        var combat=new CombatSimulation(missing.Rules,[new(missing.Line.Id,missing.Stage,2,4)],[new(missing.Line.Id,missing.Stage,2,4)],92);
        using(var battle=new BattleView(missing.Rules,combat,false,modelCatalog:models))
        {
            battle.Draw(size.X,size.Y);
            if(battle.ModelCount!=0||battle.FallbackDrawCalls!=22)throw new InvalidOperationException("Missing-model combat actors disappeared.");
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"fallback-combat.bmp"),size.X,size.Y);
        }
        if(OpenGl.GetError()!=OpenGl.GL_NO_ERROR)throw new InvalidOperationException("Fallback creature rendering generated a GL error.");
        Console.WriteLine("Missing-model creatures: original 11-part geometry renders through roster/drag and both combat sides.");
    }
}
