using System.Numerics;
using System.Reflection;
using System.Text.Json;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal static class MoveEffectSmoke
{
    public static unsafe void Run(ContentCatalog catalog,string? capture)
    {
        if(!Glfw.Init())throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor,4);Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor,5);Glfw.WindowHint(GlfwWindowHint.Visible,0);
            using var window=Glfw.CreateWindow(new GlfwWindow.CreateInfo{Title="Move effects verification",Size=new int2(1200,1200)});window.MakeContextCurrent();
            if(!Glad.Initialize(name=>Glfw.PInvoke.GetProcAddress(name)))throw new InvalidOperationException("OpenGL initialization failed.");
            using var models=new ModelRenderer();using var renderer=new MoveEffectRenderer();var fx=new MoveEffects(84219);
            int cell=Math.Min(window.FramebufferSize.X,window.FramebufferSize.Y)/6;if(cell<128)throw new InvalidOperationException("Move effect capture framebuffer is too small.");var eye=new Vector3(5,5,8);var camera=Matrix4x4.CreateLookAt(eye,new(0,.8f,0),Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(.72f,1,.1f,100);
            fx.CameraPosition.Set(eye.X,eye.Y,eye.Z);int visible=0,totalCalls=0;List<string> labels=[];
            OpenGl.ClearColor(new(0,0,0,1));OpenGl.Clear(GlClearBit.Color);
            string[] names=[..MovePrimitives.Names,"equipment beam","equipment burst","equipment shield"];
            for(int index=0;index<names.Length;index++)
            {
                string name=names[index];fx.Clear();fx=new((uint)(84219+index));fx.CameraPosition.Set(eye.X,eye.Y,eye.Z);var legacy=new LegacyCombatEffects(fx);
                var context=new MoveContext{A=new(-1.6,1.05,-.7),P=new(-1.8,0,-1),C=new(2.1,0,1.7),T=[new(2.1,.8,1.7),new(1.2,1.1,2.3)],H=1.65,R=1.8,S=1.3,Stage=1,Yaw=.84,Ca=MoveColor.Hex("#e57630"),Cb=MoveColor.Hex("#ffe9a4"),Spec=new("fire","none",.3,"hit",0,[],[],[])};
                switch(name)
                {
                    case "equipment beam":legacy.Beam(new(-1.8,0,-1),new(2.1,0,1.7),LegacyCombatEffects.AttributeColor("Vaccine"),0);break;
                    case "equipment burst":legacy.Burst(new(0,0,0),2,0,0);break;
                    case "equipment shield":legacy.Shield(new(0,0,0),0);break;
                    default:MovePrimitives.Play(name,fx,context,new(),0);break;
                }
                int left=index%6*cell,bottom=(5-index/6)*cell;int changed=0;
                for(int sample=0;sample<3;sample++)
                {
                    for(int i=0;i<5;i++)fx.Update(1.0/60);
                    legacy.Update((sample+1)/12.0);
                    ModelRenderer.Begin(cell,cell);OpenGl.Viewport(left,bottom,cell,cell);OpenGl.Native.glEnable(GlCapability.ScissorTest);OpenGl.Native.glScissor(left,bottom,cell,cell);OpenGl.Clear(GlClearBit.Color);OpenGl.Native.glDisable(GlCapability.ScissorTest);
                    totalCalls+=renderer.Draw(models,camera,fx,cell,cell);var pixels=new byte[cell*cell*4];fixed(byte* data=pixels)OpenGl.ReadPixels(left,bottom,cell,cell,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,data);
                    changed=Math.Max(changed,Enumerable.Range(0,cell*cell).Count(i=>pixels[i*4]+pixels[i*4+1]+pixels[i*4+2]>12));
                }
                if(changed<4)throw new InvalidOperationException($"Move primitive {name} did not reach rendered pixels ({changed}).");visible++;labels.Add($"{index}: {name}; {changed} pixels");
            }
            if(capture is not null){FrameCapture.SaveBitmap(capture,cell*6,cell*6);File.WriteAllLines(Path.ChangeExtension(capture,"txt"),labels);}
            // Real battle integration: no hidden simulation mutations or duplicate cue playback.
            var rules=catalog.Variants[0];using var battle=new BattleView(rules,rules.Lines.Take(12).Select((line,i)=>new CombatEntry(line.Id,i%3,i%7,4+i/7,Items:[])).ToArray());
            for(int i=0;i<540;i++){battle.Advance(1.0/60);if(i%30==0)battle.Draw(512,512);}
            if(battle.MoveBasicCount<1||battle.MoveCastCount<1)throw new InvalidOperationException("Real combat did not dispatch native basic and cast effects.");
            if(capture is not null){battle.Draw(1024,768);FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"battle.bmp"),1024,768);}
            int speedChecks=CheckBattleSpeeds(rules);
            Console.WriteLine($"Move effect GPU smoke passed: {visible} primitive/equipment families, {totalCalls} batched draws; {battle.MoveCastCount} casts, {battle.MoveBasicCount} basic attacks and {battle.MoveEquipmentCount} equipment cues from real combat.");
            Console.WriteLine($"Battle speed regression passed: {speedChecks} checks for 1x/2x/4x outcomes, combat events, VFX clocks, unscaled board time, pause, zero delta and frame clamping.");
        }
        finally{Glfw.Terminate();}
    }

    private sealed record BattleSnapshot(string State,CombatEvent[] Events,int Casts,int Basics,int Equipment);
    private static BattleSnapshot Snapshot(BattleView battle)=>new(JsonSerializer.Serialize(new
    {
        battle.Combat.Time,battle.Combat.Done,battle.Combat.Winner,battle.Combat.FinalHits,
        Units=battle.Combat.Units.Select(unit=>new
        {
            unit.Id,Line=unit.Line.Id,unit.Stage,unit.Team,unit.Alive,unit.Cell,unit.MoveOrigin,unit.MoveElapsed,unit.MoveDuration,
            unit.Health,unit.MaxHealth,unit.Mana,unit.Shield,unit.AttackDamage,unit.AttackSpeed,unit.AbilityPower,
            unit.AttackCooldown,unit.MoveCooldown,unit.CastLock,unit.Stun,unit.Dealt,unit.Taken,unit.Healed,unit.Attacks,
            Target=unit.Target?.Id
        })
    }),battle.Combat.Events.ToArray(),battle.MoveCastCount,battle.MoveBasicCount,battle.MoveEquipmentCount);

    private static int CheckBattleSpeeds(Ruleset rules)
    {
        // ui.js scales the fixed-tick accumulator; render3d.js scales MoveFx only.
        // At a fixed real frame rate, compare authoritative results at common
        // simulation times. Particle integration and animation sampling may differ
        // with frame size, so compare VFX clock/cues and a scheduled completion.
        var effectsField=typeof(BattleView).GetField("moveEffects",BindingFlags.Instance|BindingFlags.NonPublic)!;
        var pausedField=typeof(BattleView).GetField("paused",BindingFlags.Instance|BindingFlags.NonPublic)!;
        var roster=rules.Lines.Take(12).Select((line,i)=>new CombatEntry(line.Id,i%3,i%7,4+i/7,Items:[])).ToArray();
        List<BattleSnapshot> reference=[];int checks=0;
        void Check(bool condition,string message){checks++;if(!condition)throw new InvalidOperationException("Battle speed regression: "+message);}
        void Near(double actual,double expected,string message)=>Check(Math.Abs(actual-expected)<1e-8,$"{message}: {actual:R} != {expected:R}");
        foreach(int speed in new[]{1,2,4})
        {
            using var battle=new BattleView(rules,roster);
            var effects=((NativeMoveEffects)effectsField.GetValue(battle)!).Effects;
            var initial=Snapshot(battle);int completed=0;
            effects.Later(.2,()=>completed++);
            for(int i=0;i<8;i++)battle.Advance(0,speed);
            Near(effects.Now,0,$"{speed}x zero delta preserves VFX time");
            Check(initial.State==Snapshot(battle).State&&initial.Events.SequenceEqual(battle.Combat.Events),$"{speed}x zero delta preserves combat state/events");
            Check(completed==0,$"{speed}x zero delta does not complete delayed work");
            pausedField.SetValue(battle,true);
            try{for(int i=0;i<8;i++)battle.Advance(.1,speed);}
            finally{pausedField.SetValue(battle,false);}
            Near(effects.Now,0,$"{speed}x pause preserves VFX time");
            Check(initial.State==Snapshot(battle).State&&initial.Events.SequenceEqual(battle.Combat.Events)&&completed==0,$"{speed}x pause preserves combat and scheduled work");
            // An identical warm-up in all three battles also checks resume and
            // the source's clamp-before-speed rule for a long real frame.
            battle.Advance(.05,4);
            Near(effects.Now,.2,$"{speed}x resume advances VFX by selected speed");
            Check(completed==1,$"{speed}x resume completes delayed work exactly once");
            double boardBefore=battle.SceneTime,combatBefore=battle.Combat.Time;
            battle.Advance(.35,4);
            Near(effects.Now,.6,$"{speed}x long frame is clamped before speed scaling");
            Near(battle.SceneTime-boardBefore,.1,$"{speed}x board keeps clamped wall time");
            Near(battle.Combat.Time-combatBefore,.4,$"{speed}x clamped frame advances fixed ticks");
            double effectStart=effects.Now,boardStart=battle.SceneTime;
            var cues=battle.Combat.Hooks.Context(CombatHookSource.Mechanic,"speed-effect-regression");
            var from=battle.Combat.Units[0];var to=battle.Combat.Units[^1];
            cues.Effect("beam",from,to);cues.Burst(to.Cell,2,from.Team);cues.Effect("shield",from);
            effects.Later(1.5,()=>completed++);
            int checkpoints=speed==1?30:reference.Count;
            for(int checkpoint=0;checkpoint<checkpoints;checkpoint++)
            {
                for(int frame=0;frame<120/speed;frame++)battle.Advance(1.0/60,speed);
                var actual=Snapshot(battle);double simulated=(checkpoint+1)*2;
                Near(effects.Now-effectStart,simulated,$"{speed}x VFX time at checkpoint {checkpoint}");
                Near(battle.SceneTime-boardStart,simulated/speed,$"{speed}x board wall time at checkpoint {checkpoint}");
                Check(completed==2,$"{speed}x scheduled VFX completes once at common simulated time");
                if(speed==1)reference.Add(actual);
                else
                {
                    var expected=reference[checkpoint];
                    Check(actual.State==expected.State,$"{speed}x authoritative combat state/result differs at checkpoint {checkpoint}");
                    Check(actual.Events.SequenceEqual(expected.Events),$"{speed}x ordered combat events differ at checkpoint {checkpoint}");
                    Check((actual.Casts,actual.Basics,actual.Equipment)==(expected.Casts,expected.Basics,expected.Equipment),$"{speed}x effects omitted or duplicated combat cues at checkpoint {checkpoint}");
                }
                if(speed==1&&battle.Combat.Done)break;
            }
            Check(battle.Combat.Done,$"{speed}x deterministic battle did not reach an outcome");
            Check(battle.MoveCastCount>0&&battle.MoveBasicCount>0&&battle.MoveEquipmentCount>=3,$"{speed}x battle exercised cast, basic and equipment effect dispatch");
        }
        return checks;
    }
}
