using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Checks the direct client routes through their real persistent navigation.</summary>
internal sealed class CollectionRouteSmoke
{
    private int step,wait=3;
    private bool down;
    public void Input(MatchScreen screen)
    {
        if(wait-->0||step>=4)return;
        var io=ImGui.GetIO();
        if(down){io.AddMouseButtonEvent(0,false);down=false;step++;wait=3;return;}
        string key=step switch{0=>"clientStore",1=>"collection",2=>"clientLoot",_=>"closeCollection"};
        if(!screen.Buttons.TryGetValue(key,out var target))return;
        io.AddMousePosEvent(target.X,target.Y);io.AddMouseButtonEvent(0,true);down=true;
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(wait!=1)return false;
        if(step==1)
        {
            if(!screen.CollectionStoreVisible||screen.CollectionLootVisible)throw new InvalidOperationException("Store route did not open its dedicated page.");
            foreach(var offer in CosmeticCollection.Featured(DateOnly.FromDateTime(DateTime.Now)))
                if(!screen.Buttons.TryGetValue("collectionOffer/"+offer.Kind+"/"+offer.Item.Id,out var point)||point.Y<screen.CollectionListBounds.Min.Y+70||point.Y>screen.CollectionListBounds.Max.Y-70)
                    throw new InvalidOperationException("Store opened with a daily featured offer clipped or scrolled away.");
        }
        if(step==2&&(!screen.CollectionVisible||screen.CollectionStoreVisible||screen.CollectionLootVisible))throw new InvalidOperationException("Collection route retained another page's state.");
        if(step==3&&!screen.CollectionLootVisible)throw new InvalidOperationException("Loot route did not open the incubator directly.");
        if(capture is not null&&step is >=1 and <=3)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"collection-route-"+step+".bmp"),size.X,size.Y);
        if(step<4)return false;
        if(screen.CollectionVisible)throw new InvalidOperationException("Collection Back did not return to the client.");
        Console.WriteLine("Collection routes passed: distinct Store, Collection and Loot pages, all four daily featured offers and Back navigation.");return true;
    }
}
