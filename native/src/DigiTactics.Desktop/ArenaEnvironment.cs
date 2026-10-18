using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Owns theme resources and composes environment, gameplay surfaces and unit shadow/color passes.</summary>
internal sealed class ArenaEnvironment : IDisposable
{
    private static readonly string folder=Path.Combine(AppContext.BaseDirectory,"content/boards");
    private static readonly HashSet<string> ids=Directory.GetFiles(folder,"*.json.gz").Select(p=>Path.GetFileName(p)[..^8]).ToHashSet();
    private static readonly Lazy<BoardSurfaceGeometry> geometry=new(()=>BoardSurfaceGeometry.Load(Path.Combine(AppContext.BaseDirectory,"content/board-surfaces.json")));
    private readonly SceneShadowMap shadows=new(2048);
    private BoardScene? scene;
    private BoardMeshLayer? meshes;
    private BoardBackground? background;
    private BoardSurfaceView? surfaces;
    private BoardLighting? lighting;
    public string? Theme => scene?.Id;
    public int DrawCalls { get; private set; }
    public BoardSurfaceFeedback Feedback { get; } = new();
    public double Time { get; set; }
    private double? previousTime;

    public void SetTheme(string? id)
    {
        if(id is null || !ids.Contains(id)) id="file-island";
        if(scene?.Id==id) return;
        var next=BoardScene.Load(Path.Combine(folder,id+".json.gz"));
        BoardMeshLayer? nextMeshes=null; BoardBackground? nextBackground=null; BoardSurfaceView? nextSurfaces=null;
        try { nextMeshes=new(next); nextBackground=new(next); nextSurfaces=new(next,geometry.Value,Feedback); }
        catch { nextMeshes?.Dispose(); nextBackground?.Dispose(); nextSurfaces?.Dispose(); throw; }
        meshes?.Dispose(); background?.Dispose(); surfaces?.Dispose();
        scene=next; meshes=nextMeshes; background=nextBackground; surfaces=nextSurfaces; lighting=new(next);
        previousTime=Time;
    }

    public void Draw(ModelRenderer renderer,Matrix4x4 camera,Vector3 eye,int columns,bool planning,int left,int bottom,int width,int height,Action<Matrix4x4,bool> units)
    {
        if(scene is null) SetTheme("file-island");
        double delta=previousTime is { } previous?Math.Clamp(Time-previous,0,.1):0;
        previousTime=Time; meshes!.Update(Time,delta,!planning);
        if(meshes.LightningColor is {} flashColor)
        {
            lighting!.FlashColor=flashColor;lighting.Flash=meshes.LightningFlash;lighting.Update(0);
        }
        bool wide=columns>7;
        meshes!.Root=Matrix4D.Compose(new(0,0,0),new(0,0,0,1),new(wide?1.8:1,1,1));
        lighting!.FogNear=wide?65:scene!.Fog[1].GetSingle(); lighting.FogFar=wide?120:scene!.Fog[2].GetSingle();
        renderer.BoardLight=lighting; renderer.FarmEye=eye; renderer.ShadowCamera=SceneShadowMap.BoardCamera;
        renderer.ShadowMapSize=shadows.Size; renderer.ShadowBias=-.0005f;
        shadows.Begin(); renderer.ShadowPass=true;
        try
        {
            foreach(bool transparent in new[] {false,true}) { meshes.Draw(renderer,SceneShadowMap.BoardCamera,transparent,shadows.Size); units(SceneShadowMap.BoardCamera,transparent); }
        }
        finally { renderer.ShadowPass=false; shadows.End(); }
        renderer.ShadowTexture=shadows.Texture;
        ModelRenderer.Begin(width,height); OpenGl.Viewport(left,bottom,width,height);
        try
        {
            background!.Draw(renderer); meshes.Draw(renderer,camera,false,height); DrawCalls=meshes.DrawCalls;
            surfaces!.Draw(renderer,camera,columns,planning); DrawCalls+=surfaces.DrawCalls;
            renderer.ReceiveShadow=true; units(camera,false);
            meshes.Draw(renderer,camera,true,height); DrawCalls+=meshes.DrawCalls;
            renderer.ReceiveShadow=true; units(camera,true);
        }
        finally { ModelRenderer.End(); }
    }

    public void Dispose() { meshes?.Dispose(); background?.Dispose(); surfaces?.Dispose(); shadows.Dispose(); }
}
