using System.Numerics;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal static class BoardMeshSmoke
{
    public static unsafe void Run(string? capture)
    {
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            const int columns = 6, rows = 7;
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Board mesh verification", Size = new int2(1440, 1680) });
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            int cell = Math.Min(window.FramebufferSize.X / columns, window.FramebufferSize.Y / rows);
            if (cell < 32) throw new InvalidOperationException("Board capture framebuffer is too small.");
            using var renderer = new ModelRenderer();
            CheckVertexColors(renderer);
            CheckPoints(renderer);
            CheckLines(renderer);
            CheckFog(renderer);
            CheckParticleMotion(renderer);
            CheckPropMotion(renderer);
            CheckLightning(renderer);
        CheckGraveyard(renderer);
        CheckCity(renderer);
            CheckWindowEmission(renderer);
            CheckTransparentOrder(renderer);
            using(var arena=new ArenaEnvironment())
            {
                var rules=DigiTactics.Rules.ContentCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/catalog.json")).Variants[0];
                arena.SetTheme("spiral-mountain"); arena.Time=3.7;
                arena.Feedback.Flash(1,1,100,Vector3.One);
                using(var planning=new PlanningView(arena))
                {
                    var combat=new DigiTactics.Rules.CombatSimulation(rules,[],[],84219,7,setup:new(new(),new()));
                    using var battle=new BattleView(rules,combat,false,arena);
                    if(battle.SceneTime!=3.7 || planning.SceneTime!=3.7 || arena.Theme!="spiral-mountain")
                        throw new InvalidOperationException("View creation reset the shared arena.");
                    battle.SceneTime=4.2;
                    if(planning.SceneTime!=4.2) throw new InvalidOperationException("Planning and combat do not share the arena clock.");
                }
                arena.Draw(renderer,BattleView.Camera(256,256,7),BattleView.CameraEye(256,256,7),7,true,0,0,256,256,(_,_)=>{});
                if(arena.DrawCalls<=75 || arena.Theme!="spiral-mountain") throw new InvalidOperationException("Disposing match views destroyed the shared arena.");
                var view=BattleView.Camera(256,256,14);
                foreach(string theme in new[] {"file-island","digital-grid","../invalid"})
                {
                    arena.SetTheme(theme); arena.SetTheme(theme);
                    arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,256,256,(_,_)=>{});
                    if(arena.DrawCalls<=130 || arena.Theme!=(theme=="../invalid"?"file-island":theme) || renderer.BoardLight?.FogNear!=65 || renderer.BoardLight?.FogFar!=120)
                        throw new InvalidOperationException("Live board theme replacement, fallback or Duos fog failed.");
                }
                arena.Draw(renderer,BattleView.Camera(256,256,7),BattleView.CameraEye(256,256,7),7,true,0,0,256,256,(_,_)=>{});
                if(renderer.BoardLight?.FogNear!=30 || renderer.BoardLight?.FogFar!=60) throw new InvalidOperationException("Planning did not restore theme fog.");
                arena.SetTheme("kowloon");arena.CombatActive=true;arena.Time+=.1;
                arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,256,256,(_,_)=>{});
                if(Math.Abs(renderer.BoardLight!.FogNear-22.92f)>.001 || Math.Abs(renderer.BoardLight.FogFar-56.32f)>.001)
                    throw new InvalidOperationException("Kowloon combat weather did not override Duos fog.");
                arena.CombatActive=false;arena.Time+=.1;
                arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,256,256,(_,_)=>{});
                if(Math.Abs(renderer.BoardLight!.FogNear-23.0496f)>.001)throw new InvalidOperationException("Kowloon result weather did not ease out.");
                arena.Won=false;arena.SetTheme("circuit-core");arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,64,64,(_,_)=>{});
                float circuitBase=renderer.BoardLight!.HemisphereIntensity;
                arena.Won=true;arena.Time+=.016;arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,64,64,(_,_)=>{});
                if(Math.Abs(renderer.BoardLight!.HemisphereIntensity-circuitBase-1.12f)>.001)throw new InvalidOperationException("Circuit win did not flash scene lighting.");
                arena.Time+=.1;arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,64,64,(_,_)=>{});
                if(Math.Abs(renderer.BoardLight!.HemisphereIntensity-circuitBase-.62f)>.001)throw new InvalidOperationException("Held win signal retriggered the flash.");
                arena.SetTheme("file-island");arena.SetTheme("circuit-core");arena.Time+=.016;
                arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,64,64,(_,_)=>{});
                if(Math.Abs(renderer.BoardLight!.HemisphereIntensity-circuitBase)>.001)throw new InvalidOperationException("Initial won state triggered a false win reaction.");
                arena.Evolution=0;arena.SetTheme("flame-temple");arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,64,64,(_,_)=>{});
                float templeBase=renderer.BoardLight!.HemisphereIntensity,templeSun=renderer.BoardLight.SunIntensity;
                arena.Evolution=1;arena.Time+=.016;arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,64,64,(_,_)=>{});
                if(Math.Abs(renderer.BoardLight!.HemisphereIntensity-templeBase-2.12f)>.001 || renderer.BoardLight.SunIntensity<templeSun*2)
                    throw new InvalidOperationException("Temple evolution did not reach scene lighting.");
                arena.SetTheme("file-island");arena.SetTheme("flame-temple");arena.Time+=.016;
                arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,64,64,(_,_)=>{});
                if(Math.Abs(renderer.BoardLight!.HemisphereIntensity-templeBase)>.001)throw new InvalidOperationException("Initial evolution count triggered a false flare.");
                arena.Evolution=0;arena.SetTheme("witchelny-library");arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,64,64,(_,_)=>{});
                float libraryBase=renderer.BoardLight!.HemisphereIntensity;
                arena.Evolution=1;arena.Time+=.016;arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,64,64,(_,_)=>{});
                var expectedTint=Vector3.Lerp(BoardLighting.Linear("#ffe0b0"),BoardLighting.Linear("#ffd090"),(float)((1-.016*.45)*.8));
                if(Math.Abs(renderer.BoardLight!.HemisphereIntensity-libraryBase-1.12f)>.001 || Vector3.Distance(renderer.BoardLight.Sun,expectedTint)>.0001)
                    throw new InvalidOperationException("Library evolution did not reach scene lighting.");
                arena.SetTheme("file-island");arena.SetTheme("witchelny-library");arena.Time+=.016;
                arena.Draw(renderer,view,BattleView.CameraEye(256,256,14),14,false,0,0,64,64,(_,_)=>{});
                if(Math.Abs(renderer.BoardLight!.HemisphereIntensity-libraryBase)>.001)throw new InvalidOperationException("Initial library evolution triggered a false flare.");
            }
            renderer.BoardLight=null; renderer.ShadowTexture=0;
            var scenes=Directory.GetFiles(Path.Combine(AppContext.BaseDirectory,"content/boards"),"*.json.gz").Order().Select(BoardScene.Load).ToArray();
            foreach(var scene in scenes)
            {
                using var background=new BoardBackground(scene);
                CheckBackground(renderer,background,scene);
            }
            OpenGl.ClearColor(new float4(.025f, .035f, .06f, 1)); OpenGl.Clear(GlClearBit.Color);
            var camera = Matrix4x4.CreateLookAt(new(0, 25, 28), Vector3.Zero, Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView(.9f, 1, .1f, 150);
            var surfaceGeometry=BoardSurfaceGeometry.Load(Path.Combine(AppContext.BaseDirectory,"content/board-surfaces.json"));
            using var shadows = new SceneShadowMap(2048);
            renderer.ShadowCamera=SceneShadowMap.BoardCamera; renderer.ShadowMapSize=shadows.Size; renderer.ShadowBias=-.0005f;
            int index = 0, draws = 0;
            foreach (var scene in scenes)
            {
                renderer.BoardLight = new(scene); renderer.FarmEye = new(0,25,28);
                using var layer = new BoardMeshLayer(scene);
                using var board = new BoardSurfaceView(scene,surfaceGeometry);
                using var background = new BoardBackground(scene);
                shadows.Begin(); renderer.ShadowPass=true;
                try
                {
                    layer.Draw(renderer,SceneShadowMap.BoardCamera,false,shadows.Size); int shadowDraws=layer.DrawCalls;
                    layer.Draw(renderer,SceneShadowMap.BoardCamera,true,shadows.Size); shadowDraws+=layer.DrawCalls;
                    int expectedShadows=scene.Objects.Where(o=>o.Visible && o.CastShadow).Sum(o=>(o.Instances?.Length??1)*(o.Materials.Length==1?1:scene.Geometries[o.Geometry].Groups.Length));
                    if(shadowDraws!=expectedShadows) throw new InvalidOperationException("Board shadow caster count mismatch: "+scene.Id);
                }
                finally { renderer.ShadowPass=false; shadows.End(); }
                int expected = scene.Objects.Where(o => o.Visible).Sum(o => (o.Instances?.Length ?? 1) * (o.Materials.Length == 1 ? 1 : scene.Geometries[o.Geometry].Groups.Length));
                int left=index%columns*cell,bottom=(rows-1-index/columns)*cell;
                void Render(bool withShadows,bool planning=false,int columns=7)
                {
                    renderer.ShadowTexture=withShadows ? shadows.Texture : 0;
                    ModelRenderer.Begin(cell,cell); OpenGl.Viewport(left,bottom,cell,cell); background.Draw(renderer);
                    layer.Draw(renderer,camera,false,cell); int actual=layer.DrawCalls;
                    board.Draw(renderer,camera,columns,planning);
                    if(board.DrawCalls!=(columns==7?75:130)) throw new InvalidOperationException("Board gameplay surface count mismatch.");
                    layer.Draw(renderer,camera,true,cell); actual+=layer.DrawCalls;
                    if(actual!=expected || actual==0) throw new InvalidOperationException("Board mesh draw inventory mismatch: "+scene.Id);
                    ModelRenderer.End();
                }
                Render(true,true,7);
                if(scene.Id=="file-beach")
                {
                    byte[] Pixels() { var pixels=new byte[cell*cell*4]; fixed(byte* p=pixels) OpenGl.ReadPixels(left,bottom,cell,cell,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p); return pixels; }
                    Render(false); var resting=Pixels(); layer.Update(2.7); Render(false); var moving=Pixels();
                    int changed=Enumerable.Range(0,cell*cell).Count(i=>Math.Abs(resting[i*4]-moving[i*4])+Math.Abs(resting[i*4+1]-moving[i*4+1])+Math.Abs(resting[i*4+2]-moving[i*4+2])>3);
                    if(changed<10) throw new InvalidOperationException("Beach animation did not change rendered pixels.");
                    Console.WriteLine($"Beach animation pixels verified: {changed} pixels changed.");
                    layer.Update(0); Render(true,true,7);
                }
                if(scene.Id=="file-island")
                {
                    byte[] Pixels() { var pixels=new byte[cell*cell*4]; fixed(byte* p=pixels) OpenGl.ReadPixels(left,bottom,cell,cell,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p); return pixels; }
                    Render(true,true,14);
                    Render(false); var lit=Pixels(); Render(true); var shaded=Pixels();
                    int darkened=Enumerable.Range(0,cell*cell).Count(i=>lit[i*4]+lit[i*4+1]+lit[i*4+2]-shaded[i*4]-shaded[i*4+1]-shaded[i*4+2]>8);
                    if(darkened<10) throw new InvalidOperationException("Board shadows did not darken receiving surfaces.");
                    Console.WriteLine($"Board shadow pixels verified: {darkened} receiving pixels darkened.");
                    board.Feedback.Time=1; board.Feedback.Flash(3,4,1.45,BoardLighting.Linear("#ff7a6a"));
                    board.Feedback.UnitDragMoved=true; board.Feedback.HoverCell=(4,5); board.Feedback.HoverBench=4; board.Feedback.HoverItem=3;
                    Render(true); var highlighted=Pixels();
                    int brighter=Enumerable.Range(0,cell*cell).Count(i=>highlighted[i*4]+highlighted[i*4+1]+highlighted[i*4+2]-shaded[i*4]-shaded[i*4+1]-shaded[i*4+2]>8);
                    if(brighter<10) throw new InvalidOperationException("Board surface feedback did not reach the framebuffer.");
                    Console.WriteLine($"Board feedback pixels verified: {brighter} pixels brightened.");
                    board.Feedback.ClearFlashes(); board.Feedback.UnitDragMoved=false; board.Feedback.HoverItem=-1;
                    Render(true);
                    var resting=Pixels(); layer.Update(2.7); Render(true); var moving=Pixels();
                    int changed=Enumerable.Range(0,cell*cell).Count(i=>Math.Abs(resting[i*4]-moving[i*4])+Math.Abs(resting[i*4+1]-moving[i*4+1])+Math.Abs(resting[i*4+2]-moving[i*4+2])>3);
                    if(changed<10) throw new InvalidOperationException("Animated board geometry did not change rendered pixels.");
                    Console.WriteLine($"Board animation pixels verified: {changed} pixels changed.");
                    layer.Update(0); Render(true);
                }
                else Render(true);
                draws+=expected; index++;
            }
            if (index != 37) throw new InvalidOperationException("Board mesh smoke must cover every theme.");
            if (capture is not null) FrameCapture.SaveBitmap(capture, cell * columns, cell * rows);
            Console.WriteLine($"Board geometry GPU smoke: {index} themes, {draws:N0} material/instance draws; original sky pixels, fog, vertex colors, wireframe, lines, point sizing/orientation and shader state reset verified.");
        }
        finally { Glfw.Terminate(); }
    }

    private static unsafe void CheckVertexColors(ModelRenderer renderer)
    {
        float[] vertices = new float[3 * GltfPrimitive.Stride];
        float[] positions = [-1,-1,.5f, 1,-1,.5f, 0,1,.5f];
        for (int i = 0; i < 3; i++) { Array.Copy(positions, i * 3, vertices, i * GltfPrimitive.Stride, 3); vertices[i * GltfPrimitive.Stride + 5] = 1; }
        using var triangle = new GpuPrimitive(new(vertices, [0,1,2], -1, true, false) { Colors = [1,0,0, 1,0,0, 1,0,0] });
        byte[] Pixel() { var p = new byte[4]; fixed (byte* bytes = p) OpenGl.ReadPixels(16, 16, 1, 1, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, bytes); return p; }
        ModelRenderer.Begin(32,32);
        renderer.DrawRoom(triangle, Matrix4D.Identity, Matrix4x4.Identity, new(1,1,1,1), null, true, false, true, vertexColors: true);
        var red = Pixel(); ModelRenderer.End();
        if (red[0] < 250 || red[1] > 2 || red[2] > 2) throw new InvalidOperationException("Vertex colors did not reach the framebuffer.");
        ModelRenderer.Begin(32,32);
        OpenGl.ClearColor(new(0,0,0,1)); OpenGl.Clear(GlClearBit.Color);
        renderer.DrawRoom(triangle, Matrix4D.Identity, Matrix4x4.Identity, new(1,1,1,1), null, true, false, true, wireframe: true);
        var interior = Pixel(); ModelRenderer.End();
        if (interior.Take(3).Any(c => c > 2)) throw new InvalidOperationException("Wireframe triangle interior was filled.");
        ModelRenderer.Begin(32,32);
        renderer.DrawStatic(triangle, Matrix4D.Identity, Matrix4x4.Identity, new(1,1,1,1));
        var white = Pixel(); ModelRenderer.End();
        if (white.Take(3).Any(c => c < 250)) throw new InvalidOperationException("Vertex colors leaked into an unrelated draw.");
    }

    private static unsafe void CheckPoints(ModelRenderer renderer)
    {
        float[] vertices = new float[GltfPrimitive.Stride]; vertices[2] = -4; vertices[5] = 1;
        using var point = new GpuPrimitive(new(vertices,[0],-1,true,false), OpenGl.GL_POINTS);
        var camera = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI/2,1,.1f,100);
        byte[] Draw(float depth, float size, bool attenuate, GpuTexture? texture = null)
        {
            vertices[2] = -depth; point.UpdateVertices(vertices);
            ModelRenderer.Begin(64,64); OpenGl.ClearColor(new(0,0,0,1)); OpenGl.Clear(GlClearBit.Color);
            renderer.DrawRoom(point,Matrix4D.Identity,camera,new(1,1,1,1),texture,true,false,true,
                points:new(size,64,attenuate,new(1,0,0,0),new(0,1,0,0)));
            byte[] pixels = new byte[64*64*4];
            fixed(byte* p=pixels) OpenGl.ReadPixels(0,0,64,64,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
            ModelRenderer.End(); return pixels;
        }
        foreach (var sample in new[] { (4f,4f,true,32), (8f,4f,true,16), (4f,12f,false,12), (8f,12f,false,12) })
        {
            var pixels = Draw(sample.Item1,sample.Item2,sample.Item3);
            int width = Enumerable.Range(0,64).Count(x=>pixels[(32*64+x)*4]>250);
            if (width != sample.Item4) throw new InvalidOperationException($"Point size mismatch: {width} vs {sample.Item4}.");
        }
        var source = new BoardTexture(1,2,Convert.ToBase64String([255,0,0,255,0,0,255,255]),true,[1,1],[0,0],[0,0],0,1001,1001,"srgb");
        using var texture = new GpuTexture(BoardMesh.Image(source),new(MagFilter:9728,MinFilter:9728,WrapS:33071,WrapT:33071),true);
        var image = Draw(4,32,false,texture);
        int top = (40*64+32)*4, bottom = (24*64+32)*4;
        if (image[top]<250 || image[top+2]>2 || image[bottom]>2 || image[bottom+2]<250) throw new InvalidOperationException("Point sprite texture orientation changed.");
    }

    private static unsafe void CheckLines(ModelRenderer renderer)
    {
        var vertices = new float[GltfPrimitive.Stride*2];
        vertices[0]=-.5f; vertices[2]=.5f; vertices[GltfPrimitive.Stride]=.5f; vertices[GltfPrimitive.Stride+2]=.5f;
        using var line = new GpuPrimitive(new(vertices,[0,1],-1,false,false),OpenGl.GL_LINES);
        ModelRenderer.Begin(32,32); OpenGl.ClearColor(new(0,0,0,1)); OpenGl.Clear(GlClearBit.Color);
        renderer.DrawRoom(line,Matrix4D.Identity,Matrix4x4.Identity,new(1,1,1,1),null,true,false,true);
        var pixels=new byte[32*32*4];
        fixed(byte* p=pixels) OpenGl.ReadPixels(0,0,32,32,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
        ModelRenderer.End();
        int visible=Enumerable.Range(0,32*32).Count(i=>pixels[i*4]>250);
        if (visible<14 || visible>18) throw new InvalidOperationException("Native line segment did not rasterize correctly.");
    }

    private static unsafe void CheckFog(ModelRenderer renderer)
    {
        var scene=BoardScene.Load(Path.Combine(AppContext.BaseDirectory,"content/boards/file-island.json.gz"));
        renderer.BoardLight=new(scene) { FogNear=2,FogFar=6,FogColor=new(0,0,1) };
        float[] vertices=new float[3*GltfPrimitive.Stride];
        float[] positions=[-2,-2,0, 2,-2,0, 0,2,0];
        for(int i=0;i<3;i++) { Array.Copy(positions,i*3,vertices,i*GltfPrimitive.Stride,3); vertices[i*GltfPrimitive.Stride+5]=1; }
        using var triangle=new GpuPrimitive(new(vertices,[0,1,2],-1,true,false));
        var camera=Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI/2,1,.1f,100);
        foreach(var sample in new[] { (2f,true,255,0), (4f,true,128,128), (6f,true,0,255), (6f,false,255,0) })
        {
            for(int i=0;i<3;i++) vertices[i*GltfPrimitive.Stride+2]=-sample.Item1;
            triangle.UpdateVertices(vertices); ModelRenderer.Begin(64,64);
            renderer.DrawRoom(triangle,Matrix4D.Identity,camera,new(1,0,0,1),null,true,false,true,fog:sample.Item2);
            byte[] pixel=new byte[4]; fixed(byte* p=pixel) OpenGl.ReadPixels(32,32,1,1,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
            ModelRenderer.End();
            if(Math.Abs(pixel[0]-sample.Item3)>1 || pixel[1]>1 || Math.Abs(pixel[2]-sample.Item4)>1) throw new InvalidOperationException("Board fog depth or material opt-out changed.");
        }
        renderer.BoardLight=null;
    }

    private static unsafe void CheckBackground(ModelRenderer renderer,BoardBackground background,BoardScene scene)
    {
        ModelRenderer.Begin(2,256); background.Draw(renderer);
        var pixels=new byte[2*256*4];
        fixed(byte* p=pixels) OpenGl.ReadPixels(0,0,2,256,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
        ModelRenderer.End();
        byte[] expected;
        if(scene.Background is { } index)
        {
            var image=BoardMesh.Image(scene.Textures[index]);
            if(image.Width!=2 || image.Height!=256) throw new InvalidOperationException("Source board gradient dimensions changed.");
            expected=image.Rgba;
        }
        else
        {
            var color=scene.Sky[0]; byte[] rgba=[Convert.ToByte(color[1..3],16),Convert.ToByte(color[3..5],16),Convert.ToByte(color[5..7],16),255];
            expected=Enumerable.Range(0,pixels.Length).Select(i=>rgba[i%4]).ToArray();
        }
        for(int i=0;i<pixels.Length;i++)
            if(Math.Abs(pixels[i]-expected[i])>1) throw new InvalidOperationException($"Board sky color/orientation mismatch: {scene.Id}, byte {i}: {pixels[i]} vs {expected[i]}.");
    }

    private static unsafe void CheckTransparentOrder(ModelRenderer renderer)
    {
        var source=BoardScene.Load(Path.Combine(AppContext.BaseDirectory,"content/boards/file-island.json.gz"));
        var material=source.Materials[0] with {Map=null,EmissiveMap=null,Unlit=true,Transparent=true,DepthWrite=false,Side=2,Blending=1};
        var geometry=new BoardGeometry([-.8f,-.8f,0,.8f,-.8f,0,0,.8f,0],null,null,null,[0,1,2],[]);
        BoardObject Triangle(int id,double z,int mat,int order)=>new("mesh",0,[mat],new double[] {1,0,0,0,0,1,0,0,0,0,1,0,0,0,z,1},true,false,false,null,SortCenter:[0,0,0],SortId:id,RenderOrder:order);
        byte[] Render(int nearOrder,Matrix4x4 camera)
        {
            var scene=source with {Geometries=[geometry],Materials=[material with {Color=[0,0,1,.5f]},material with {Color=[1,0,0,.5f]}],Objects=[Triangle(1,.3,0,nearOrder),Triangle(2,.7,1,0)]};
            using var layer=new BoardMeshLayer(scene);ModelRenderer.Begin(32,32);OpenGl.ClearColor(new(0,0,0,1));OpenGl.Clear(GlClearBit.Color);
            layer.Draw(renderer,camera,true,32);var pixel=new byte[4];fixed(byte* p=pixel)OpenGl.ReadPixels(16,16,1,1,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);ModelRenderer.End();return pixel;
        }
        var depth=Render(0,Matrix4x4.Identity);var explicitOrder=Render(-1,Matrix4x4.Identity);var reversed=Render(0,Matrix4x4.CreateScale(1,1,-1)*Matrix4x4.CreateTranslation(0,0,1));
        if(depth[2]<depth[0]+40 || explicitOrder[0]<explicitOrder[2]+40 || reversed[0]<reversed[2]+40)
            throw new InvalidOperationException($"Transparent depth/render-order did not control alpha blending: {string.Join(",",depth)} / {string.Join(",",explicitOrder)} / {string.Join(",",reversed)}.");
        Console.WriteLine("Transparent GPU ordering: depth, explicit render order and reversed camera verified.");
    }
    private static unsafe void CheckWindowEmission(ModelRenderer renderer)
    {
        var scene=BoardScene.Load(Path.Combine(AppContext.BaseDirectory,"content/boards/kowloon.json.gz"));
        var buildings=scene.Objects.Where(o=>o.Materials.Any(i=>scene.Materials[i].EmissiveMap is not null)).ToArray();
        if(buildings.Length!=1)throw new InvalidOperationException("Missing original city-window emissive material.");
        using var masked=new BoardMeshLayer(scene with {Objects=buildings});
        using var unmasked=new BoardMeshLayer(scene with {Objects=buildings,Materials=scene.Materials.Select(m=>m with {EmissiveMap=null}).ToArray()});
        byte[] Render(BoardMeshLayer layer)
        {
            ModelRenderer.Begin(256,256);OpenGl.ClearColor(new(0,0,0,1));OpenGl.Clear(GlClearBit.Color);
            var camera=Matrix4x4.CreateLookAt(new(0,25,28),new(0,-8,0),Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(.9f,1,.1f,150);
            layer.Draw(renderer,camera,false,256);
            var pixels=new byte[256*256*4];fixed(byte* p=pixels)OpenGl.ReadPixels(0,0,256,256,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
            ModelRenderer.End();return pixels;
        }
        var plain=Render(unmasked);var windows=Render(masked);var restored=Render(unmasked);
        int darker=Enumerable.Range(0,256*256).Count(i=>plain[i*4]+plain[i*4+1]+plain[i*4+2]-windows[i*4]-windows[i*4+1]-windows[i*4+2]>30);
        if(darker<100 || !plain.SequenceEqual(restored))throw new InvalidOperationException("City emissive texture or per-draw shader reset failed.");
        Console.WriteLine($"City emissive map: {darker} wall pixels darkened; shader reset verified.");
    }
    private static void CheckCity(ModelRenderer renderer)
    {
        var scene=BoardScene.Load(Path.Combine(AppContext.BaseDirectory,"content/boards/file-city.json.gz"));
        using var layer=new BoardMeshLayer(scene with {Objects=scene.Objects.Where(o=>o.Motion?.Building is not null).ToArray()},initialStage:7);
        var camera=BattleView.Camera(256,256,7);
        ModelRenderer.Begin(256,256);layer.Draw(renderer,camera,false,256);
        if(layer.DrawCalls!=13)throw new InvalidOperationException("Late-stage city did not initialize all buildings.");
        for(int i=0;i<240;i++)layer.Update(i/60d,1/60d,stage:1);
        layer.Draw(renderer,camera,false,256);
        if(layer.DrawCalls!=3)throw new InvalidOperationException("City buildings did not hide after stage reset.");
        renderer.ShadowPass=true;
        try {layer.Draw(renderer,SceneShadowMap.BoardCamera,false,256);if(layer.DrawCalls!=3)throw new InvalidOperationException("Hidden city buildings still cast shadows.");}
        finally {renderer.ShadowPass=false;ModelRenderer.End();}
        Console.WriteLine("City GPU: initial stage seven, stage reset and shadow visibility verified.");
    }
    private static unsafe void CheckGraveyard(ModelRenderer renderer)
    {
        var scene=BoardScene.Load(Path.Combine(AppContext.BaseDirectory,"content/boards/overdell-graveyard.json.gz"));
        using var layer=new BoardMeshLayer(scene with {Objects=scene.Objects.Where(o=>o.Motion?.Lightning is not null).ToArray()});
        layer.Update(.016,.016,true);layer.Update(.032,.016,true);
        var camera=Matrix4x4.CreateLookAt(new(0,25,28),Vector3.Zero,Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(.9f,1,.1f,150);
        ModelRenderer.Begin(256,256);OpenGl.ClearColor(new(0,0,0,1));OpenGl.Clear(GlClearBit.Color);layer.Draw(renderer,camera,true,256);
        var pixels=new byte[256*256*4];fixed(byte* p=pixels)OpenGl.ReadPixels(0,0,256,256,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);ModelRenderer.End();
        int lit=Enumerable.Range(0,256*256).Count(i=>pixels[i*4+1]>pixels[i*4]+10);
        if(layer.DrawCalls!=1 || lit<5)throw new InvalidOperationException("Green graveyard lightning did not reach the framebuffer.");
        using var arena=new ArenaEnvironment();arena.CombatActive=true;arena.SetTheme("overdell-graveyard");
        for(int i=0;i<40;i++){arena.Time=i*.1;arena.Draw(renderer,camera,new(0,25,28),14,false,0,0,64,64,(_,_)=>{});}
        if(renderer.BoardLight!.FogNear>10 || renderer.BoardLight.FogFar>39)throw new InvalidOperationException("Graveyard combat fog did not override wide-board defaults.");
        float near=renderer.BoardLight.FogNear;arena.CombatActive=false;arena.Time+=.1;arena.Draw(renderer,camera,new(0,25,28),14,false,0,0,64,64,(_,_)=>{});
        if(renderer.BoardLight!.FogNear<=near)throw new InvalidOperationException("Graveyard result fog did not ease out.");
        renderer.BoardLight=null;renderer.ShadowTexture=0;
        Console.WriteLine($"Graveyard GPU: {lit} green lightning pixels and combat/result fog verified.");
    }
    private static unsafe void CheckLightning(ModelRenderer renderer)
    {
        var scene=BoardScene.Load(Path.Combine(AppContext.BaseDirectory,"content/boards/spiral-mountain.json.gz"));
        var bolts=scene.Objects.Where(o=>o.Motion?.Lightning is not null).ToArray();
        if(bolts.Length!=3) throw new InvalidOperationException("Missing lightning meshes.");
        using var layer=new BoardMeshLayer(scene with {Objects=bolts});
        layer.Update(.016,.016,true);
        var overview=Matrix4x4.CreateLookAt(new(0,25,28),Vector3.Zero,Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(.9f,1,.1f,150);
        ModelRenderer.Begin(256,256);OpenGl.ClearColor(new(0,0,0,1));OpenGl.Clear(GlClearBit.Color);
        layer.Draw(renderer,overview,true,256);
        var pixels=new byte[256*256*4];fixed(byte* p=pixels)OpenGl.ReadPixels(0,0,256,256,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
        ModelRenderer.End();
        int lit=Enumerable.Range(0,256*256).Count(i=>pixels[i*4]+pixels[i*4+1]+pixels[i*4+2]>10);
        if(layer.DrawCalls!=1 || lit<5)throw new InvalidOperationException($"Combat lightning did not reach the framebuffer: {layer.DrawCalls} draws, {lit} lit pixels.");
        layer.Update(.1,.084,true);layer.Draw(renderer,BattleView.Camera(256,256,7),true,256);
        if(layer.DrawCalls!=0)throw new InvalidOperationException("Lightning flicker gap did not hide the bolt.");
        using var arena=new ArenaEnvironment();arena.SetTheme("spiral-mountain");
        var camera=BattleView.Camera(256,256,7);var eye=BattleView.CameraEye(256,256,7);
        arena.Draw(renderer,camera,eye,7,true,0,0,256,256,(_,_)=>{});
        float initial=renderer.BoardLight!.HemisphereIntensity;
        arena.Time=.016;arena.Draw(renderer,camera,eye,7,false,0,0,256,256,(_,_)=>{});
        if(renderer.BoardLight!.HemisphereIntensity<initial+1)throw new InvalidOperationException("Lightning flash did not reach scene lighting.");
        arena.CombatActive=false;
        for(int i=0;i<120;i++)
        {
            arena.Time+=.1;arena.Draw(renderer,camera,eye,7,false,0,0,64,64,(_,_)=>{});
            if(i>4 && Math.Abs(renderer.BoardLight!.HemisphereIntensity-initial)>.001)
                throw new InvalidOperationException("Result view continued scheduling combat lightning.");
        }
        using var combatLoaded=new ArenaEnvironment {CombatActive=true};combatLoaded.SetTheme("spiral-mountain");combatLoaded.Time=.016;
        combatLoaded.Draw(renderer,camera,eye,7,false,0,0,256,256,(_,_)=>{});
        if(Math.Abs(renderer.BoardLight!.HemisphereIntensity-initial)>.001)throw new InvalidOperationException("Loading a combat board emitted a false phase transition.");
        renderer.BoardLight=null;renderer.ShadowTexture=0;
        Console.WriteLine($"Lightning GPU: {lit} lit pixels, flicker gap and scene flash verified.");
    }
    private static unsafe void CheckPropMotion(ModelRenderer renderer)
    {
        foreach(var (theme,kind) in new[] {
            ("primary-village","village-eggs"),("primary-village","village-blocks"),("toy-town","toy-tops"),("toy-town","toy-train"),
            ("server-desert","desert-haze"),("ancient-coliseum","coliseum-banners"),("ancient-coliseum","coliseum-flames"),
            ("gear-savanna","savanna-gears"),("infinity-mountain","mountain-stars"),
            ("digital-grid","grid-polys"),("digital-grid","grid-rings"),("digital-grid","grid-floor"),("dark-ocean","rotating-parent"),
            ("spiral-mountain","spiral-rocks"),("spiral-mountain","spiral-islets"),
            ("spiral-mountain","spiral-crystals"),("spiral-mountain","spiral-halos"),
            ("vampire-castle","castle-flames"),("vampire-castle","castle-bats"),
            ("vampire-castle","castle-moon"),("vampire-castle","castle-halo"),
            ("kowloon","kowloon-band"),("kowloon","kowloon-lattice"),("kowloon","kowloon-signs"),("kowloon","particles"),
            ("circuit-core","circuit-pulses"),("circuit-core","circuit-screen"),("circuit-core","circuit-floor"),
            ("flame-temple","temple-outer"),("flame-temple","temple-inner"),("flame-temple","temple-geysers"),
            ("flame-temple","temple-lava"),("flame-temple","particles"),
            ("gekomon-castle","underwater-caustics"),("gekomon-castle","underwater-caustics2"),
            ("gekomon-castle","underwater-shafts"),("gekomon-castle","underwater-kelp"),
            ("gekomon-castle","underwater-clams"),("gekomon-castle","underwater-fish"),("freezeland","freeze-men"),("freezeland","freeze-aurora"),("numemon-sewer","sewer-numemon"),("numemon-sewer","sewer-duck"),("numemon-sewer","sewer-pour"),("witchelny-library","library-pages"),("witchelny-library","library-candles"),("witchelny-library","library-flames"),("witchelny-library","library-halos"),("witchelny-library","library-circle"),("witchelny-library","library-outer"),("knights-coliseum","knights-crowd"),("knights-coliseum","knights-banners"),("knights-coliseum","knights-flashes"),("knights-coliseum","knights-confetti"),("battle-studio","studio-strip"),("battle-studio","studio-led"),("battle-studio","studio-audience"),("battle-studio","studio-sticks"),("battle-studio","studio-beam"),("battle-studio","studio-pool"),("battle-studio","studio-crane"),("battle-studio","studio-confetti"),("trailmon-station","train-cars"),("trailmon-station","train-engine"),("trailmon-station","train-clock"),("trailmon-station","particles"),("overdell-graveyard","graveyard-ghosts"),("server-pyramid","pyramid-beams"),("server-pyramid","pyramid-bands"),("server-pyramid","pyramid-flames"),("file-city","city-building"),("file-city","city-crops"),("holiday-world","holiday-tree"),("holiday-world","holiday-string"),("holiday-world","particles")})
        {
            var scene=BoardScene.Load(Path.Combine(AppContext.BaseDirectory,$"content/boards/{theme}.json.gz"));
            var objects=scene.Objects.Where(o=>o.Motion?.Kind==kind).ToArray();
            if(objects.Length!=(theme=="holiday-world" && kind=="particles"?2:kind=="city-building"?13:kind=="pyramid-beams"?2:kind=="pyramid-bands"?6:kind is "studio-beam" or "studio-pool"?6:kind is "studio-strip" or "studio-crane"?2:kind=="library-pages"?2:kind=="freeze-aurora"?3:kind=="savanna-gears"?4:kind=="rotating-parent"?2:1)) throw new InvalidOperationException("Missing original animated prop: "+kind);
            using var layer=new BoardMeshLayer(scene with { Objects=objects });
            byte[] Render()
            {
                ModelRenderer.Begin(256,256); OpenGl.ClearColor(new(0,0,0,1)); OpenGl.Clear(GlClearBit.Color);
                var camera=Matrix4x4.CreateLookAt(new(0,25,28),Vector3.Zero,Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(.9f,1,.1f,150);
                if(kind=="mountain-stars") camera=Matrix4x4.CreateLookAt(Vector3.Zero,new(0,30,-50),Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(.9f,1,.1f,200);
                if(kind=="train-clock")
                {
                    OpenGl.ClearColor(new(.5f,.5f,.5f,1));OpenGl.Clear(GlClearBit.Color);
                    var target=new Vector3(-6.5f,3,-10.42f);
                    camera=Matrix4x4.CreateLookAt(target+new Vector3(0,0,2),target,Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(.9f,1,.1f,150);
                }
                if(kind=="sewer-pour")
                {
                    var target=new Vector3(-12.6f,-.1f,-6);
                    camera=Matrix4x4.CreateLookAt(target+new Vector3(0,1,3),target,Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(.9f,1,.1f,150);
                }
                if(kind=="castle-flames")
                {
                    var flame=objects[0].Motion!.Castle![0];var target=new Vector3((float)flame.X,(float)flame.Y,(float)flame.Z);
                    camera=Matrix4x4.CreateLookAt(target+new Vector3(0,1,2),target,Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(.9f,1,.1f,150);
                }
                layer.Draw(renderer,camera,false,256);
                layer.Draw(renderer,camera,true,256);
                var pixels=new byte[256*256*4]; fixed(byte* p=pixels) OpenGl.ReadPixels(0,0,256,256,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
                ModelRenderer.End(); return pixels;
            }
            var initial=Render();
            for(int frame=1;frame<=162;frame++) layer.Update(frame/60d,1/60d,true,7,won:(theme=="numemon-sewer" || theme=="knights-coliseum" || theme=="battle-studio" || theme=="holiday-world") && frame>=60,evolution:frame>=60?1:0);
            var moved=Render();
            int changed=Enumerable.Range(0,256*256).Count(i=>Math.Abs(initial[i*4]-moved[i*4])+Math.Abs(initial[i*4+1]-moved[i*4+1])+Math.Abs(initial[i*4+2]-moved[i*4+2])>3);
            if(changed<5) throw new InvalidOperationException("Board prop animation did not reach the framebuffer: "+kind);
            Console.WriteLine($"Board prop {kind}: {changed} changed pixels verified.");
        }
    }
    private static unsafe void CheckParticleMotion(ModelRenderer renderer)
    {
        var scene=BoardScene.Load(Path.Combine(AppContext.BaseDirectory,"content/boards/server-desert.json.gz"));
        var particles=scene.Objects.Where(o=>o.Motion?.Kind=="particles").ToArray();
        if(particles.Length==0) throw new InvalidOperationException("Particle GPU check requires original desert dust.");
        using var layer=new BoardMeshLayer(scene with { Objects=particles });
        byte[] Render()
        {
            ModelRenderer.Begin(256,256); OpenGl.ClearColor(new(0,0,0,1)); OpenGl.Clear(GlClearBit.Color);
            layer.Draw(renderer,BattleView.Camera(256,256,7),true,256);
            var pixels=new byte[256*256*4]; fixed(byte* p=pixels) OpenGl.ReadPixels(0,0,256,256,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
            ModelRenderer.End(); return pixels;
        }
        var initial=Render();
        for(int frame=1;frame<=120;frame++) layer.Update(frame/60d,1/60d);
        var moved=Render();
        int changed=Enumerable.Range(0,256*256).Count(i=>Math.Abs(initial[i*4]-moved[i*4])+Math.Abs(initial[i*4+1]-moved[i*4+1])+Math.Abs(initial[i*4+2]-moved[i*4+2])>3);
        if(changed<5) throw new InvalidOperationException("Board particle updates did not reach the framebuffer.");
        Console.WriteLine($"Board particle GPU updates verified: {changed} pixels changed.");
    }
}
