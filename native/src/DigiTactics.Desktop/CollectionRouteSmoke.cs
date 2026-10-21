using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Checks the direct client routes through their real persistent navigation.</summary>
internal sealed class CollectionRouteSmoke
{
    private int step,wait=3;
    private bool down;
    private readonly HashSet<int> captured=[];
    private readonly StoreDanceRouteSmoke dances=new();
    public string Pending=>step==8&&!dances.Done?"Store dance / "+dances.Pending:"route "+step;
    public void Input(MatchScreen screen)
    {
        if(wait-->0||step>=11)return;
        var io=ImGui.GetIO();
        if(down){io.AddMouseButtonEvent(0,false);down=false;step++;wait=3;return;}
        if(step==8&&!dances.Done){dances.Input(screen);return;}
        string key=step switch{0=>"clientStore",1=>"collectionTab/7",2=>"egg/legend",3=>"cancelEgg",4=>"collectionTab/6",5=>"storeSoundboardClips",6=>"collectionTab/0",7=>screen.Buttons.Keys.FirstOrDefault(k=>k.StartsWith("storeEgg/legend/",StringComparison.Ordinal))??"missingStoreEgg",8=>"collection",9=>"clientLoot",_=>"closeCollection"};
        if(!screen.Buttons.TryGetValue(key,out var target))return;
        if(step is 2 or 5 or 7)
        {
            var bounds=screen.CollectionListBounds;
            if(target.Y<bounds.Min.Y+22||target.Y>bounds.Max.Y-22)
            {io.AddMousePosEvent((bounds.Min.X+bounds.Max.X)/2,(bounds.Min.Y+bounds.Max.Y)/2);io.AddMouseWheelEvent(0,target.Y<bounds.Min.Y+22?3:-3);wait=3;return;}
        }
        if(target.X<0||target.Y<0||target.X>=io.DisplaySize.X||target.Y>=io.DisplaySize.Y)throw new InvalidOperationException("Store route control outside viewport: "+key);
        io.AddMousePosEvent(target.X,target.Y);io.AddMouseButtonEvent(0,true);down=true;
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(step==8)dances.Verify(screen,size,capture);
        if(wait!=1)return false;
        if(step==1)
        {
            if(!screen.CollectionStoreVisible||screen.CollectionLootVisible)throw new InvalidOperationException("Store route did not open its dedicated page.");
            foreach(var offer in CosmeticCollection.Featured(DateOnly.FromDateTime(DateTime.Now)))
                if(!screen.Buttons.TryGetValue("collectionOffer/"+offer.Kind+"/"+offer.Item.Id,out var point)||point.Y<screen.CollectionListBounds.Min.Y+70||point.Y>screen.CollectionListBounds.Max.Y-70)
                    throw new InvalidOperationException("Store opened with a daily featured offer clipped or scrolled away.");
        }
        if(step==9&&(!screen.CollectionVisible||screen.CollectionStoreVisible||screen.CollectionLootVisible))throw new InvalidOperationException("Collection route retained another page's state.");
        if(step==10&&!screen.CollectionLootVisible)throw new InvalidOperationException("Loot route did not open the incubator directly.");
        if((step==2||step==8)&&(!screen.CollectionStoreEggsVisible||screen.CollectionFilterIds.Length!=8||!screen.CollectionFilterIds.Contains("legend")))throw new InvalidOperationException("Store egg tab omitted its eight egg choices.");
        if(step==3&&!screen.Buttons.ContainsKey("confirmEgg"))throw new InvalidOperationException("Store hatch did not open egg confirmation.");
        if(step==6&&(!screen.CollectionStoreVisible||screen.CollectionFilterKind!="sound"))throw new InvalidOperationException("Soundboard clips link did not select Soundboard in Store.");
        if(capture is not null&&step is >=1 and <=10&&captured.Add(step))FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"collection-route-"+step+".bmp"),size.X,size.Y);
        if(step<11)return false;
        if(screen.CollectionVisible)throw new InvalidOperationException("Collection Back did not return to the client.");
        Console.WriteLine("Collection routes passed: distinct Store, Collection and Loot pages, all four daily featured offers, eight Digi-Egg choices/confirmation/cancel, linked Soundboard route, egg-exclusive Store redirect, animated Store dance cache and Back navigation.");return true;
    }
}
