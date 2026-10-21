using Brutal.ImGuiApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Checks live Store card pixels, visibility, modal lifetime and navigation cleanup.</summary>
internal sealed class StoreDanceRouteSmoke
{
    private enum Phase { Open, First, Second, Scroll, Purchase, Confirm, Cancel, AfterCancel, Leave, Cleanup, Done }
    private Phase phase,afterClick;
    private int wait,settleFrame,offscreenChecks;
    private bool down,scrollReady,sawDrawCalls;
    private byte[]? firstPixels;
    private float2 samplePoint;
    private double firstTime;
    private long firstUpdates;
    private readonly HashSet<string> featured=CosmeticCollection.Featured(DateOnly.FromDateTime(DateTime.Now)).Where(offer=>offer.Kind=="dance").Select(offer=>offer.Item.Id).ToHashSet();
    public bool Done=>phase==Phase.Done;
    public string Pending=>phase.ToString();
    public void Input(MatchScreen screen)
    {
        if(Done)return;
        var io=ImGui.GetIO();
        if(down){io.AddMouseButtonEvent(0,false);down=false;phase=afterClick;wait=3;settleFrame=ImGui.GetFrameCount()+2;return;}
        if(wait-->0)return;
        switch(phase)
        {
            case Phase.Open: Click("collectionTab/4",Phase.First);break;
            case Phase.First: Position("preview/dance/floss",64);break;
            case Phase.Scroll:
                if(!scrollReady)
                {
                    // Reach the end of the actual stock grid, including its
                    // action row, instead of merely changing selected items.
                    string last=screen.Buttons.Where(pair=>pair.Key.StartsWith("buyCosmetic/dance/",StringComparison.Ordinal)).OrderByDescending(pair=>pair.Value.Y).Select(pair=>pair.Key).FirstOrDefault()??throw new InvalidOperationException("Store dance stock has no priced card.");
                    if(Position(last,20)){scrollReady=true;settleFrame=ImGui.GetFrameCount()+2;}
                }
                break;
            case Phase.Purchase: Click("buyCosmetic/dance/robot",Phase.Confirm,true);break;
            case Phase.Cancel: Click("cancelPurchase",Phase.AfterCancel);break;
            case Phase.Leave: Click("collection",Phase.Cleanup);break;
        }
        void Click(string key,Phase next,bool scroll=false)
        {
            if(!screen.Buttons.TryGetValue(key,out var target))throw new InvalidOperationException("Store dance route omitted "+key+".");
            if(scroll&&!Position(key,20))return;
            Require(target.X>=0&&target.Y>=0&&target.X<io.DisplaySize.X&&target.Y<io.DisplaySize.Y,"Store dance control is outside the viewport: "+key);
            io.AddMousePosEvent(target.X,target.Y);io.AddMouseButtonEvent(0,true);down=true;afterClick=next;
        }
        bool Position(string key,float margin)
        {
            if(!screen.Buttons.TryGetValue(key,out var point))throw new InvalidOperationException("Store dance route omitted "+key+".");
            var bounds=screen.CollectionListBounds;
            if(point.Y<bounds.Min.Y+margin||point.Y>bounds.Max.Y-margin)
            {
                io.AddMousePosEvent(bounds.Max.X-5,(bounds.Min.Y+bounds.Max.Y)*.5f);io.AddMouseWheelEvent(0,point.Y<bounds.Min.Y+margin?2:-2);wait=3;return false;
            }
            io.AddMousePosEvent(bounds.Max.X-5,(bounds.Min.Y+bounds.Max.Y)*.5f);return true;
        }
    }
    public void Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(Done)return;
        if(screen.StoreDancePreviewDrawCalls>0)sawDrawCalls=true;
        if(down||wait>0)return;
        if(phase is not(Phase.Open or Phase.Cleanup))
        {
            Require(screen.CollectionStoreVisible&&screen.CollectionFilterKind=="dance","Store dance route left its selected category.");
            Require(screen.StoreDancePreviewCacheCount<=StoreDancePreviewCache.Capacity&&screen.StoreDancePreviewRendererCount<=1,"Store dance cards allocated an unbounded cache or multiple renderers.");
        }
        switch(phase)
        {
            case Phase.First:
                if(!screen.Buttons.TryGetValue("preview/dance/floss",out samplePoint)||!ImageFullyVisible(screen,samplePoint))return;
                Require(screen.StoreDancePreviewIds.Contains("floss")&&screen.StoreDancePreviewRendererCount==1&&screen.StoreDancePreviewUpdates>0&&sawDrawCalls,"Visible dance card omitted its native companion render.");
                CheckVisibility(screen);firstPixels=ReadPreview(size,samplePoint);firstTime=ImGui.GetTime();firstUpdates=screen.StoreDancePreviewUpdates;
                Save("dance-a");phase=Phase.Second;break;
            case Phase.Second:
                Require(screen.Buttons.TryGetValue("preview/dance/floss",out var point)&&Math.Abs(point.Y-samplePoint.Y)<.5f&&Math.Abs(point.X-samplePoint.X)<.5f,"Dance animation capture moved between phases.");
                if(ImGui.GetTime()-firstTime<.43)return;
                int changed=ChangedPixels(firstPixels!,ReadPreview(size,samplePoint));
                if(changed<8&&ImGui.GetTime()-firstTime<1.5)return;
                Require(screen.StoreDancePreviewUpdates>firstUpdates&&changed>=8,"Visible dance preview did not advance its rendered pose between captured phases.");
                Save("dance-b");Console.WriteLine($"Store dance animation changed {changed} preview pixels; updates {firstUpdates}->{screen.StoreDancePreviewUpdates}.");phase=Phase.Scroll;break;
            case Phase.Scroll:
                if(!scrollReady||ImGui.GetFrameCount()<settleFrame)return;
                CheckVisibility(screen);
                Require(screen.StoreDancePreviewCacheCount==screen.StoreDancePreviewIds.Length,"Offscreen Store dance textures remained cached after two consumed frames.");
                Save("dance-scrolled");phase=Phase.Purchase;break;
            case Phase.Confirm:
                Require(screen.Buttons.ContainsKey("confirmPurchase")&&screen.Buttons.ContainsKey("cancelPurchase"),"Dance purchase did not open its confirmation above cached Store previews.");
                Save("dance-purchase");phase=Phase.Cancel;break;
            case Phase.AfterCancel:
                Require(!screen.Buttons.ContainsKey("confirmPurchase")&&screen.Buttons.ContainsKey("buyCosmetic/dance/robot"),"Cancel changed ownership or left the dance purchase dialog open.");
                phase=Phase.Leave;break;
            case Phase.Cleanup:
                if(ImGui.GetFrameCount()<settleFrame)return;
                Require(screen.CollectionVisible&&!screen.CollectionStoreVisible&&!screen.CollectionLootVisible,"Dance Store navigation did not reach Collection.");
                Require(screen.StoreDancePreviewCacheCount==0&&screen.StoreDancePreviewRendererCount==0&&screen.StoreDancePreviewIds.Length==0,"Store dance textures or renderer survived navigation cleanup.");
                Console.WriteLine($"Store dance route passed: animated pixels, one bounded shared renderer, {offscreenChecks} offscreen stock checks, purchase/cancel and cleanup after two frames.");phase=Phase.Done;break;
        }
        void Save(string name){if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"collection-route-"+name+".bmp"),size.X,size.Y);}
    }
    private void CheckVisibility(MatchScreen screen)
    {
        var bounds=screen.CollectionListBounds;var visible=screen.StoreDancePreviewIds.ToHashSet();
        foreach(var item in CosmeticCollection.Items("dance").Where(item=>!item.Secret))
        {
            Require(screen.Buttons.TryGetValue("preview/dance/"+item.Id,out var point),"Store dance stock omitted a preview control.");
            if(ImageFullyVisible(screen,point))Require(visible.Contains(item.Id),"A visible Store dance card was left static: "+item.Id);
            else if(!featured.Contains(item.Id)&&(point.Y+62<bounds.Min.Y||point.Y-62>bounds.Max.Y))
            {Require(!visible.Contains(item.Id),"An offscreen Store dance card performed rendering work: "+item.Id);offscreenChecks++;}
        }
    }
    private static bool ImageFullyVisible(MatchScreen screen,float2 point)=>point.Y>=screen.CollectionListBounds.Min.Y+64&&point.Y<=screen.CollectionListBounds.Max.Y-64;
    private static unsafe byte[] ReadPreview(int2 size,float2 point)
    {
        const int side=96;var pixels=new byte[side*side*4];
        int left=Math.Clamp((int)Math.Round(point.X)-side/2,0,size.X-side),bottom=Math.Clamp(size.Y-(int)Math.Round(point.Y)-side/2,0,size.Y-side);
        fixed(byte* data=pixels)OpenGl.ReadPixels(left,bottom,side,side,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,data);
        return pixels;
    }
    private static int ChangedPixels(byte[] before,byte[] after)
    {
        int changed=0;for(int i=0;i<before.Length;i+=4)if(Math.Abs(before[i]-after[i])+Math.Abs(before[i+1]-after[i+1])+Math.Abs(before[i+2]-after[i+2])>20)changed++;
        return changed;
    }
    private static void Require(bool value,string error){if(!value)throw new InvalidOperationException(error);}
}
