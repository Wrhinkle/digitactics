using System.Reflection;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

/// <summary>Real live-match cosmetic entry points, input isolation, equipment and nested return routes.</summary>
internal sealed class MatchCosmeticsSmoke
{
    private sealed record Step(string Name,Func<MatchScreen,bool> Input,Action<MatchScreen>? Check=null,string? Capture=null);
    private readonly List<Step> steps=[];
    private readonly NativeProfileStore profile;
    private readonly MatchPlanningSession session;
    private readonly HashSet<int> captures=[];
    private int index,checkedIndex=-1,cooldown=4,gold,roster;
    private bool down,mouse;
    private ImGuiKey key;
    private double countdown;
    private static readonly BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
    public string Pending=>steps.ElementAtOrDefault(index)?.Name??"done";
    public MatchCosmeticsSmoke(ContentCatalog catalog,NativeProfileStore profile)
    {
        this.profile=profile;
        Require(!profile.Exists,"Cosmetic smoke requires an empty isolated profile directory.");
        var wallet=new CosmeticCollection("{\"bits\":10000,\"owned\":{},\"equipped\":{},\"history\":[{\"n\":125,\"reason\":\"Game: placed #2\",\"at\":1720000000000},{\"n\":60,\"reason\":\"First game of the day\",\"at\":1720000000000}]}");
        string wheelEmote=CosmeticCollection.Items("emote").First(item=>!item.Secret).Id;
        wallet.Grant("emote",wheelEmote);
        string wheelSound=CosmeticCollection.Items("sound").First(item=>!item.Secret).Id;wallet.Grant("sound",wheelSound);
        wallet.Grant("legend","bukamon");wallet.Equip("legend","tokomon");
        Require(profile.UpdateStorage(new Dictionary<string,string>{{"dt.name","Cosmetic Tamer"},{"dt.cosmetics.v1",wallet.Json}}),profile.Error??"Could not save cosmetic fixture.");
        var rules=catalog.Variants[0];
        session=new(rules,731u,Enumerable.Range(0,8).Select(i=>new MatchSeat("Cosmetic Tamer "+i,i==0)).ToArray());
        if(session.Rounds.RoundType=="carousel"){session.StartCombat();session.FinishCombat();session.Advance();}
        var player=session.State.Players[0];
        if(player.Offer is not null)session.Offers.Choose(player,0,false);
        if(!player.Roster.Entries.Any(e=>e.Spot.IsBoard))
        {
            var unit=session.State.CreateUnit(rules.Lines[0].Id,0);player.Roster.AddToBench(unit);
            var spot=player.Roster.Entries.Single(e=>e.Unit.Id==unit.Id).Spot;player.Roster.Move(spot,UnitSpot.Board(3,4),false,()=>9);
        }
        string buy=CosmeticCollection.Items("legend").First(item=>!wallet.Owns("legend",item.Id)&&!item.EggOnly&&!item.Secret&&item.Linked is null).Id;
        steps.Add(new("planning fixture",s=>{typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(s,session);typeof(MatchScreen).GetField("history",Private)!.SetValue(s,new MatchHistoryRecorder(session,profile));return true;}));
        steps.Add(new("planning toolbar",s=>true,s=>Require(s.Buttons.ContainsKey("matchStore")&&s.Buttons.ContainsKey("matchLocker")&&s.Buttons.ContainsKey("matchLegend"),"Planning toolbar lost an original direct route."),"menu-planning"));
        steps.Add(new("capture planning clock",s=>{countdown=s.PlanningRemaining;gold=player.Economy.Gold;roster=player.Roster.Entries.Count();return true;}));
        Click("matchStore",s=>{CheckOverlay(s);Require(s.CollectionStoreVisible&&s.PlanningRemaining<countdown,"Store paused the live planning clock.");},"store");
        Click("collectionBitsHistory",s=>Require(s.CollectionBitsHistoryRows==2&&s.CollectionMissions.Length==2,"Store lost recent Bits or daily missions."),"history");
        Key("History Escape",ImGuiKey.Escape,s=>Require(s.MatchCosmeticsVisible&&s.CollectionBitsHistoryRows==0,"History Escape closed the Store underneath."));
        Click("preview/legend/"+buy);Click("buyCosmetic/legend/"+buy,s=>Require(s.Buttons.ContainsKey("confirmPurchase"),"Store purchase did not open its confirmation."),"purchase");
        Key("Purchase Escape",ImGuiKey.Escape,s=>Require(s.MatchCosmeticsVisible&&!s.Buttons.ContainsKey("confirmPurchase"),"Escape did not cancel only the purchase layer."));
        Key("Store owns refresh",ImGuiKey.D,Unchanged);Key("Store owns XP",ImGuiKey.F,Unchanged);Key("Store owns ready",ImGuiKey.Space,Unchanged);
        Click("closeCollection",s=>Require(!s.MatchCosmeticsVisible&&ReferenceEquals(s.Session,session),"Return to match lost the running session."));
        Click("planningTimer",s=>Require(!s.PlanningTimerEnabled,"Timer fixture did not switch off through its control."));
        Click("matchLegend",s=>
        {
            CheckOverlay(s);Require(s.CollectionFilterKind=="legend"&&s.CollectionFilterOwnership=="owned"&&s.CollectionFilterIds.All(id=>Wallet().Owns("legend",id))&&Wallet().Owns("legend",s.CollectionSelectedId),"Little Legend picker includes unowned cosmetics.");
            Require(!s.Buttons.ContainsKey("collectionOwnership/all")&&!s.Buttons.ContainsKey("toggleEggs"),"Locker exposed store-only inventory controls.");
        },"legend");
        Click("preview/legend/bukamon");Click("equip/legend/bukamon",s=>Require(Wallet().Equipped("legend")?.GetValue<string>()=="bukamon"&&((string?)typeof(MatchScreen).GetField("companionKey",Private)!.GetValue(s))?.StartsWith("bukamon",StringComparison.Ordinal)==true,"Equipped legend did not reach the live arena companion."));
        foreach(var (kind,tab,item) in new[]{("emote",1,wheelEmote),("sound",5,wheelSound)})
        {
            Click("collectionTab/"+tab);Click("preview/"+kind+"/"+item);
            Click("collectionSlot/5",null,"locker-wheel-"+kind);Click("equip/"+kind+"/"+item,s=>Require(Wallet().Equipped(kind)!.AsArray()[5]?.GetValue<string>()==item,"Locker wheel selection did not equip the chosen sixth slot: "+kind));
            Click("collectionSlot/5");Click("clearCollectionSlot",s=>Require(Wallet().Equipped(kind)!.AsArray()[5] is null,"Locker wheel clear changed the wrong slot: "+kind));
        }
        foreach(var (kind,tab) in new[]{("board",2),("killscreen",3),("dance",4),("elimsound",6)})
        {
            string item=CosmeticCollection.Items(kind).First(entry=>wallet.Owns(kind,entry.Id)).Id;
            Click("collectionTab/"+tab,s=>Require(s.CollectionFilterKind==kind&&s.CollectionFilterIds.All(id=>Wallet().Owns(kind,id))&&!s.Buttons.ContainsKey("collectionRarity"),"Locker category did not preserve its owned-card composition: "+kind));
            Click("preview/"+kind+"/"+item,s=>Require(s.CollectionSelectedId==item,"Locker card selection failed: "+kind),"locker-"+kind);
        }
        Click("collectionTab/0");Click("preview/legend/bukamon");
        Click("evolution/bukamon");Key("Evolution Escape",ImGuiKey.Escape,s=>Require(s.MatchCosmeticsVisible&&!s.Buttons.ContainsKey("closeEvolution"),"Evolution Escape lost its Locker origin."));
        Click("collectionFarm/bukamon",s=>Require(s.Buttons.ContainsKey("closeFarm")&&s.FarmSelectedResident=="bukamon","Owned-legend Farm action lost the selected resident."));Click("matchCollectionBack",CheckOverlay);
        Click("matchCollectionStore");Click("toggleEggs");Click("lootLegend/bukamon");Click("lootFarm",s=>Require(s.MatchCosmeticsVisible&&s.Buttons.ContainsKey("closeFarm")&&s.FarmDrawCalls>0,"Live Loot → Farm opened without its room or return route."),"farm");
        Click("matchCollectionBack",CheckOverlay);Key("Locker Escape returns to match",ImGuiKey.Escape,s=>Require(!s.MatchCosmeticsVisible&&!s.Buttons.ContainsKey("resume"),"Escape opened the match menu beneath Collection."));
        steps.Add(new("combat fixture",s=>{typeof(MatchScreen).GetField("localPaused",Private)!.SetValue(s,true);session.StartCombat();typeof(MatchScreen).GetMethod("CombatStarted",Private)!.Invoke(s,null);return true;}));
        steps.Add(new("combat toolbar",s=>true,s=>Require(!s.MatchStoreEntryAvailable&&!s.Buttons.ContainsKey("matchStore")&&s.Buttons.ContainsKey("matchLocker")&&s.Buttons.ContainsKey("matchLegend"),"Living combat player Store gate differs from original."),"menu-combat"));
        Click("matchLocker",s=>{CheckOverlay(s);Require(session.Rounds.Phase==MatchPhase.Combat,"Opening combat Locker changed match phase.");},"locker-combat");
        Key("Combat Locker Escape",ImGuiKey.Escape,s=>Require(!s.MatchCosmeticsVisible&&ReferenceEquals(s.Session,session),"Combat Locker did not return to the same session."));
        steps.Add(new("eliminated entry gate",s=>{typeof(MatchPlayer).GetProperty(nameof(MatchPlayer.Alive))!.SetValue(player,false);return true;},s=>Require(s.MatchStoreEntryAvailable,"Eliminated players lost Store access.")));
        steps.Add(new("eliminated toolbar",s=>true,s=>Require(s.Buttons.ContainsKey("matchStore"),"Eliminated Store entry is missing."),"menu-eliminated"));
        Click("matchStore",s=>Require(s.MatchCosmeticsVisible&&s.CollectionStoreVisible,"Eliminated Store button did not open its page."));
        Key("Eliminated Store Escape",ImGuiKey.Escape,s=>Require(!s.MatchCosmeticsVisible&&!s.Buttons.ContainsKey("resume"),"Store Escape opened pause underneath."));
        steps.Add(new("client Collection fixture",s=>{typeof(MatchScreen).GetMethod("LeaveMatch",Private)!.Invoke(s,null);return true;}));
        string linked=CosmeticCollection.Items("elimsound").First(item=>item.Linked is {Length:2}&&!wallet.Owns("elimsound",item.Id)).Id;
        var link=CosmeticCollection.Item("elimsound",linked)!.Linked!;
        Click("collection");Click("collectionRarity");Key("Rarity Escape",ImGuiKey.Escape,s=>Require(s.CollectionVisible&&!s.Buttons.ContainsKey("collectionRarity/rare"),"Rarity Escape also closed Collection."));Click("collectionTab/1");Click("preview/emote/"+wheelEmote);
        Click("collectionSlot/4",s=>Require(Wallet().Equipped("emote")!.AsArray()[4]?.GetValue<string>()==wheelEmote,"Collection named slot grid did not equip immediately."),"collection-slot-grid");
        Click("clearCollectionSlot",s=>Require(Wallet().Equipped("emote")!.AsArray()[4] is null,"Collection grid clear did not empty its selected slot."));
        Click("collectionTab/6");Click("collectionOwnership/all");Click("preview/elimsound/"+linked);
        Click("linkedCosmeticStore",s=>Require(s.CollectionStoreVisible&&s.CollectionFilterKind==link[0]&&s.CollectionSelectedId==link[1],"Linked elimination sound did not open its priced source clip in the Store."),"linked-store");
    }
    private CosmeticCollection Wallet()=>new(profile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
    private void Unchanged(MatchScreen s)=>Require(ReferenceEquals(s.Session,session)&&session.Rounds.Phase==MatchPhase.Planning&&session.State.Players[0].Economy.Gold==gold&&session.State.Players[0].Roster.Entries.Count()==roster,"A match shortcut leaked through cosmetics.");
    private static void Require(bool value,string error){if(!value)throw new InvalidOperationException(error);}
    private static void CheckOverlay(MatchScreen s)
    {
        var b=s.MatchCosmeticsBounds;var size=ImGui.GetIO().DisplaySize;
        Require(s.MatchCosmeticsVisible&&b.Min.X>=16&&b.Min.Y>=16&&b.Max.X<=size.X-16&&b.Max.Y<=size.Y-16,"Match cosmetic overlay exceeds the client bounds.");
        Require(!s.Buttons.ContainsKey("fight")&&!s.Buttons.ContainsKey("buy/0"),"Underlying match buttons remained active behind cosmetics.");
        Require(s.Buttons.TryGetValue("closeCollection",out var close)&&close.X>b.Min.X&&close.X<b.Max.X&&close.Y>b.Min.Y&&close.Y<b.Max.Y,"Return to match is clipped.");
    }
    private void Click(string id,Action<MatchScreen>? check=null,string? capture=null)=>steps.Add(new(id,s=>Pointer(s,id),check,capture));
    private void Key(string name,ImGuiKey next,Action<MatchScreen>? check=null)=>steps.Add(new(name,_=>{ImGui.GetIO().AddMousePosEvent(2,2);ImGui.GetIO().AddKeyEvent(next,true);key=next;down=true;return true;},check));
    private bool Pointer(MatchScreen s,string id)
    {
        if(!s.Buttons.TryGetValue(id,out var target))return false;
        var io=ImGui.GetIO();
        if(id.StartsWith("lootLegend/",StringComparison.Ordinal)&&s.IncubatorInventoryBounds is {} inventory)
        {
            if(target.Y<inventory.Min.Y+49||target.Y>inventory.Max.Y-49)
            {io.AddMousePosEvent(inventory.Max.X-4,(inventory.Min.Y+inventory.Max.Y)/2);io.AddMouseWheelEvent(0,target.Y<inventory.Min.Y+49?3:-3);cooldown=3;return false;}
        }
        if(id.StartsWith("preview/",StringComparison.Ordinal))
        {
            var b=s.CollectionListBounds;
            if(target.Y<b.Min.Y+6||target.Y>b.Max.Y-6){io.AddMousePosEvent(b.Max.X-4,(b.Min.Y+b.Max.Y)/2);io.AddMouseWheelEvent(0,target.Y<b.Min.Y?3:-3);cooldown=3;return false;}
        }
        if(id.StartsWith("collectionSlot/",StringComparison.Ordinal)||id=="clearCollectionSlot"||id.StartsWith("equip/",StringComparison.Ordinal)||id.StartsWith("evolution/",StringComparison.Ordinal)||id.StartsWith("buyCosmetic/",StringComparison.Ordinal)||id.StartsWith("collectionFarm/",StringComparison.Ordinal)||id=="linkedCosmeticStore")
        {
            var b=(id.StartsWith("collectionSlot/",StringComparison.Ordinal)||id=="clearCollectionSlot")&&s.MatchCosmeticsVisible&&!s.CollectionStoreVisible&&!s.CollectionLootVisible?s.CollectionListBounds:s.CollectionDetailBounds;
            if(target.Y<b.Min.Y+6||target.Y>b.Max.Y-6){io.AddMousePosEvent(b.Max.X-4,(b.Min.Y+b.Max.Y)/2);io.AddMouseWheelEvent(0,target.Y<b.Min.Y?3:-3);cooldown=3;return false;}
        }
        if(target.Y<16||target.Y>io.DisplaySize.Y-30){io.AddMousePosEvent(target.X,io.DisplaySize.Y*.65f);io.AddMouseWheelEvent(0,target.Y<16?3:-3);cooldown=3;return false;}
        io.AddMousePosEvent(target.X,target.Y);io.AddMouseButtonEvent(0,true);down=mouse=true;return true;
    }
    public void Input(MatchScreen screen)
    {
        if(cooldown-->0)return;
        if(down){if(mouse)ImGui.GetIO().AddMouseButtonEvent(0,false);else ImGui.GetIO().AddKeyEvent(key,false);down=mouse=false;cooldown=3;return;}
        if(index>=steps.Count)return;
        if(index>0&&checkedIndex<index-1){steps[index-1].Check?.Invoke(screen);checkedIndex=index-1;}
        if(steps[index].Input(screen)){index++;cooldown=3;}
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(!down&&cooldown==0&&index>0&&steps[index-1].Capture is {} name&&captures.Add(index-1)&&capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,name+".bmp"),size.X,size.Y);
        if(index<steps.Count||down||cooldown>0)return false;
        steps[^1].Check?.Invoke(screen);
        Console.WriteLine($"Match cosmetics UI passed {steps.Count} steps: source Store gating, owned Locker/Legend, live equip, clock continuity, bounded layout, one-layer Escape, shortcut isolation and Loot/Farm return preserve the same match.");return true;
    }
}
