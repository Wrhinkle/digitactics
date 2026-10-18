using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Current production camera/arena evidence. Captures require independent visual review.</summary>
internal static class MysteryBoardVisualSmoke
{
    public static unsafe void Run(ContentCatalog catalog,string directory)
    {
        Directory.CreateDirectory(directory);
        if(!Glfw.Init())throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor,4);Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor,5);Glfw.WindowHint(GlfwWindowHint.Visible,0);
            using var window=Glfw.CreateWindow(new GlfwWindow.CreateInfo{Title="Mystery board production camera verification",Size=new(1040,480)});window.MakeContextCurrent();
            if(!Glad.Initialize(name=>Glfw.PInvoke.GetProcAddress(name)))throw new InvalidOperationException("OpenGL initialization failed.");
            using var renderer=new ModelRenderer();using var roster=new RosterModelView();using var arena=new ArenaEnvironment();
            var rules=catalog.Variants[0];var lines=rules.Lines.Take(6).ToArray();
            var actors=Enumerable.Range(0,12).Select(i=>new PlanningUnit(91000+i,lines[i%lines.Length].Id,0,[])).ToArray();
            var evidence=new List<object>();
            foreach(string theme in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory,"content/boards"),"mystery_*.json.gz").Select(p=>Path.GetFileName(p)[..^8]).Order())
            {
                arena.SetTheme(theme);
                var scene=BoardScene.Load(Path.Combine(AppContext.BaseDirectory,"content/boards",theme+".json.gz"));
                foreach(var layout in new[]{(Name:"minimum",Width:568,Height:254,Columns:7),(Name:"normal",Width:1032,Height:420,Columns:7),(Name:"duos-minimum",Width:568,Height:254,Columns:14)})
                {
                    roster.Update(rules,actors.Select((unit,i)=>(unit,CombatLayout.Cell(new(i%3*(layout.Columns-1)/2,new[]{0,3,4,7}[i/3]),layout.Columns,false))),0);
                    int landmarkChanged=CheckLandmarkMotion(renderer,arena,scene,layout.Width,layout.Height,layout.Columns);
                    byte[]? first=null;
                    foreach(double time in new[]{0d,3.7})
                    {
                        arena.Time=time;arena.Camera.Reset();
                        var camera=arena.Camera.Matrix(layout.Width,layout.Height,layout.Columns);var eye=arena.Camera.Eye(layout.Width,layout.Height,layout.Columns);
                        roster.RenderedUnits.Clear();
                        arena.Draw(renderer,camera,eye,layout.Columns,true,0,0,layout.Width,layout.Height,(view,transparent)=>roster.Draw(renderer,view,transparent));
                        if(roster.RenderedUnits.Count!=actors.Length)throw new InvalidOperationException("Board scenery lost representative roster actors.");
                        var pixels=new byte[layout.Width*layout.Height*4];fixed(byte* p=pixels)OpenGl.ReadPixels(0,0,layout.Width,layout.Height,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
                        int changed=0;if(first is null)first=pixels;else for(int i=0;i<pixels.Length;i+=4)if(Math.Abs(pixels[i]-first[i])+Math.Abs(pixels[i+1]-first[i+1])+Math.Abs(pixels[i+2]-first[i+2])>5)changed++;
                        if(time>0&&changed<8)throw new InvalidOperationException("Board ambient motion did not reach the live camera framebuffer: "+theme);
                        string file=$"{theme}.{layout.Name}.{(time==0?0:1)}.bmp",path=Path.Combine(directory,file);FrameCapture.SaveBitmap(path,layout.Width,layout.Height);
                        evidence.Add(new{theme,layout=layout.Name,width=layout.Width,height=layout.Height,columns=layout.Columns,time,camera="ArenaEnvironment.Camera default / current production ArenaCamera",file,sha256=Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))),sceneSha256=Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"content/boards",theme+".json.gz")))),drawCalls=arena.DrawCalls,rosterActors=roster.RenderedUnits.Count,changedPixels=changed,landmarkChangedPixels=landmarkChanged,visualStatus="unreviewed"});
                    }
                }
            }
            File.WriteAllText(Path.Combine(directory,"manifest.json"),JsonSerializer.Serialize(evidence,new JsonSerializerOptions{WriteIndented=true}));
            Console.WriteLine($"Mystery board production-camera smoke: {evidence.Count} captures across ten themes, Solo normal/minimum and Duos minimum; visible actor submissions and motion verified. Visual acceptance remains separate.");
        }
        finally{Glfw.Terminate();}
    }

    private static unsafe int CheckLandmarkMotion(ModelRenderer renderer,ArenaEnvironment arena,BoardScene scene,int width,int height,int columns)
    {
        using var layer=new BoardMeshLayer(scene with{Objects=scene.Objects.Where(o=>o.Motion?.Kind=="mystery-kinetic").ToArray()});
        layer.Root=Matrix4D.Compose(new(0,0,0),new(0,0,0,1),new(columns>7?1.8:1,1,1));
        renderer.BoardLight=new(scene);renderer.FarmEye=arena.Camera.Eye(width,height,columns);renderer.ShadowTexture=0;
        byte[] Render(double time)
        {
            layer.Update(time,0,false,1);ModelRenderer.Begin(width,height);OpenGl.Viewport(0,0,width,height);OpenGl.ClearColor(new(0,0,0,1));OpenGl.Clear(GlClearBit.Color);
            var camera=arena.Camera.Matrix(width,height,columns);layer.Draw(renderer,camera,false,height);layer.Draw(renderer,camera,true,height);
            var pixels=new byte[width*height*4];fixed(byte* p=pixels)OpenGl.ReadPixels(0,0,width,height,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);ModelRenderer.End();return pixels;
        }
        var first=Render(0);var second=Render(3.7);int changed=0;
        for(int i=0;i<first.Length;i+=4)if(Math.Abs(first[i]-second[i])+Math.Abs(first[i+1]-second[i+1])+Math.Abs(first[i+2]-second[i+2])>5)changed++;
        if(changed<4)throw new InvalidOperationException("Authored landmark motion is not visible through the current camera: "+scene.Id);
        return changed;
    }
}
