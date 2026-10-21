using Brutal.ImGuiApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private StoreDancePreviewCache? storeDancePreviews;
    internal int StoreDancePreviewCacheCount=>storeDancePreviews?.Count??0;
    internal int StoreDancePreviewRendererCount=>storeDancePreviews?.HasRenderer==true?1:0;
    internal int StoreDancePreviewRenders=>storeDancePreviews?.Renders??0;
    internal int StoreDancePreviewDrawCalls=>storeDancePreviews?.DrawCalls??0;
    internal long StoreDancePreviewUpdates=>storeDancePreviews?.Updates??0;
    internal string[] StoreDancePreviewIds=>storeDancePreviews?.VisibleIds??[];
    private void BeginStoreDancePreviewFrame()=>storeDancePreviews?.BeginFrame(ImGui.GetFrameCount());
    private void ClearStoreDancePreviews(){RetireCollectionPreview(storeDancePreviews);storeDancePreviews=null;}
    private bool StoreDanceThumbnail(CosmeticCollection collection,CosmeticItem item,float2 at,float2 extent)
    {
        if(!ImGui.IsRectVisible(at,at+extent))return true;
        storeDancePreviews??=new();storeDancePreviews.BeginFrame(ImGui.GetFrameCount());
        uint texture=storeDancePreviews.Draw(item.Id,CurrentPreviewLegend(collection),boardSceneTime,ClientMotion);
        if(texture==0)return false;
        // dances.js draws a square 192px render centered inside the card.
        float side=Math.Min(extent.X,extent.Y);var picture=at+(extent-new float2(side,side))*.5f;
        var tint=new ImColor8(255,255,255,(byte)Math.Clamp(Math.Round(ImGui.GetStyle().Alpha*255),0,255));
        ImGui.GetWindowDrawList().AddImage(new ImTextureRef{_TexID=new((nint)texture)},picture,picture+new float2(side,side),new(0,1),new(1,0),tint);
        return true;
    }
}

/// <summary>One shared companion renderer with small, stable card textures.</summary>
internal sealed class StoreDancePreviewCache : IDisposable
{
    internal const int Capacity=64,Resolution=192;
    private sealed class Snapshot : IDisposable
    {
        public GlTexture Texture;
        public int Seen=-1;
        public string? Legend;
        public bool Motion;
        public double RenderedAt=double.NegativeInfinity;
        public Snapshot()
        {
            Span<GlTexture> names=stackalloc GlTexture[1];OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D,names);Texture=names[0];
            try
            {
                OpenGl.TextureStorage2D(Texture.Id,1,OpenGl.GL_RGBA8,Resolution,Resolution);
                OpenGl.TextureParameterFloat(Texture.Id,OpenGl.GL_TEXTURE_MIN_FILTER,OpenGl.GL_LINEAR);OpenGl.TextureParameterFloat(Texture.Id,OpenGl.GL_TEXTURE_MAG_FILTER,OpenGl.GL_LINEAR);
                OpenGl.TextureParameterFloat(Texture.Id,OpenGl.GL_TEXTURE_WRAP_S,OpenGl.GL_CLAMP_TO_EDGE);OpenGl.TextureParameterFloat(Texture.Id,OpenGl.GL_TEXTURE_WRAP_T,OpenGl.GL_CLAMP_TO_EDGE);
            }
            catch{Dispose();throw;}
        }
        public void Dispose()=>OpenGl.DeleteTextures(new[]{Texture});
    }
    private readonly Dictionary<string,Snapshot> snapshots=[];
    private CosmeticScenePreview? renderer;
    private GlFramebuffer sourceFrame,targetFrame;
    private int frame=-1;
    internal int Count=>snapshots.Count;
    internal bool HasRenderer=>renderer is not null;
    internal int Renders {get;private set;}
    internal int DrawCalls {get;private set;}
    internal long Updates {get;private set;}
    internal string[] VisibleIds=>snapshots.Where(pair=>pair.Value.Seen==frame).Select(pair=>pair.Key).ToArray();
    internal void BeginFrame(int current)
    {
        if(frame==current)return;
        frame=current;Renders=DrawCalls=0;
        // Called at the next MatchScreen.Draw, after ImGui consumed the prior
        // frame. Offscreen cards therefore cannot delete a queued texture.
        foreach(string id in snapshots.Where(pair=>pair.Value.Seen<frame-1).Select(pair=>pair.Key).ToArray())
        {snapshots[id].Dispose();snapshots.Remove(id);}
        if(snapshots.Count==0)ReleaseRenderer();
    }
    internal uint Draw(string id,string legend,double time,bool motion)
    {
        if(!snapshots.TryGetValue(id,out var snapshot))
        {
            if(snapshots.Count>=Capacity)
            {
                // A texture already queued this frame must never be recycled.
                var oldest=snapshots.Where(pair=>pair.Value.Seen!=frame).OrderBy(pair=>pair.Value.Seen).FirstOrDefault();
                if(oldest.Value is null)return 0;
                snapshot=oldest.Value;snapshots.Remove(oldest.Key);snapshot.Legend=null;
            }
            else snapshot=new();
            snapshots.Add(id,snapshot);
        }
        if(snapshot.Seen==frame)return snapshot.Texture.Id;
        snapshot.Seen=frame;
        if(snapshot.Legend!=legend||snapshot.Motion!=motion||(motion&&time-snapshot.RenderedAt>=1d/30))
        {
            renderer??=new(renderWidth:Resolution,renderHeight:Resolution);
            renderer.Draw("dance",id,legend,motion?time:0,motion,1);
            CopyRender(snapshot.Texture.Id);
            snapshot.Legend=legend;snapshot.Motion=motion;snapshot.RenderedAt=time;Renders++;Updates++;DrawCalls+=renderer.DrawCalls;
        }
        return snapshot.Texture.Id;
    }
    private void CopyRender(uint destination)
    {
        if(sourceFrame.Id==0)
        {
            Span<GlFramebuffer> frames=stackalloc GlFramebuffer[2];OpenGl.CreateFramebuffers(frames);sourceFrame=frames[0];targetFrame=frames[1];
            OpenGl.NamedFramebufferReadBuffer(sourceFrame,OpenGl.GL_COLOR_ATTACHMENT0);OpenGl.NamedFramebufferDrawBuffer(targetFrame,OpenGl.GL_COLOR_ATTACHMENT0);
        }
        OpenGl.NamedFramebufferTexture(sourceFrame,OpenGl.GL_COLOR_ATTACHMENT0,renderer!.Texture,0);
        OpenGl.NamedFramebufferTexture(targetFrame,OpenGl.GL_COLOR_ATTACHMENT0,destination,0);
        try
        {
            if(OpenGl.CheckNamedFramebufferStatus(sourceFrame,OpenGl.GL_FRAMEBUFFER)!=OpenGl.GL_FRAMEBUFFER_COMPLETE||OpenGl.CheckNamedFramebufferStatus(targetFrame,OpenGl.GL_FRAMEBUFFER)!=OpenGl.GL_FRAMEBUFFER_COMPLETE)
                throw new InvalidOperationException("Store dance preview framebuffer is incomplete.");
            // Copy the straight-alpha output before the shared renderer draws
            // another dance. ImGui keeps a distinct texture for each card.
            OpenGl.BlitNamedFramebuffer(sourceFrame.Id,targetFrame.Id,0,0,Resolution,Resolution,0,0,Resolution,Resolution,OpenGl.GL_COLOR_BUFFER_BIT,OpenGl.GL_NEAREST);
        }
        finally
        {
            OpenGl.NamedFramebufferTexture(sourceFrame,OpenGl.GL_COLOR_ATTACHMENT0,0,0);
            OpenGl.NamedFramebufferTexture(targetFrame,OpenGl.GL_COLOR_ATTACHMENT0,0,0);
        }
    }
    private void ReleaseRenderer()
    {
        if(renderer is null&&sourceFrame.Id==0&&targetFrame.Id==0)return;
        renderer?.Dispose();renderer=null;
        OpenGl.DeleteFramebuffers(new[]{sourceFrame,targetFrame});sourceFrame=targetFrame=default;
    }
    public void Dispose(){foreach(var snapshot in snapshots.Values)snapshot.Dispose();snapshots.Clear();ReleaseRenderer();}
}
