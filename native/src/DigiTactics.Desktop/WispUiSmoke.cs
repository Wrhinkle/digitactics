using System.Reflection;
using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Source descriptions through real hover targets, then actual guarded/one-use purchases.</summary>
internal sealed class WispUiSmoke(ContentCatalog catalog)
{
    private static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    private readonly CodexWisp[] wisps=new CodexCatalog(Path.Combine(AppContext.BaseDirectory,"content/codex.json")).Variant("faction-wars/normal").Wisps;
    private int scenario,frame;
    private long hoverBefore;
    private int initialGold;
    public string Pending=>$"Data Wisp {scenario}/{frame}";
    private static void Invoke(MatchScreen screen,string method)=>typeof(MatchScreen).GetMethod(method,Private)!.Invoke(screen,null);
    private void Create(MatchScreen screen)
    {
        Invoke(screen,"LeaveMatch");
        var rules=catalog.Variants.Single(r=>r.Id=="faction-wars/normal");
        var session=new MatchPlanningSession(rules,771u,Enumerable.Range(0,8).Select(i=>new MatchSeat("Wisp Tamer "+i,i==0)).ToArray());
        if(session.Rounds.RoundType=="carousel"){session.StartCombat();session.FinishCombat();session.Advance();}
        var human=session.State.Players[0];
        if(human.Offer is not null)session.Offers.Choose(human,0,false);
        human.Economy.Credit(100);
        typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen,session);
        typeof(MatchScreen).GetField("localPaused",Private)!.SetValue(screen,true);
        typeof(MatchScreen).GetField("viewed",Private)!.SetValue(screen,0);
        if(JsonSerializer.Serialize(new ShopMetadata(2,"Partner"))!="{\"Cost\":2,\"Tag\":\"Partner\",\"Unit\":null}")
            throw new InvalidOperationException("Optional offer note changed existing shop metadata serialization.");
    }
    public void Input(MatchScreen screen)
    {
        if(scenario==0&&frame==0)Create(screen);
        hoverBefore=scenario>=wisps.Length+5?screen.ShopTooltipSerial:screen.WispTooltipSerial;
        var human=screen.Session!.State.Players[0];
        if(frame==0)
        {
            ImGui.GetIO().AddMousePosEvent(0,0);
            if(scenario<wisps.Length)human.Wisp=new(wisps[scenario].Id,wisps[scenario].ExampleCost);
            else switch(scenario-wisps.Length)
            {
                case 0:human.Wisp=new("gold",2);human.Economy.Spend(human.Economy.Gold);break;
                case 1:human.Economy.Credit(10);break;
                case 4:human.Wisp=new("gold",2);screen.Session.StartCombat();Invoke(screen,"CombatStarted");break;
                case 5:Create(screen);human=screen.Session!.State.Players[0];human.Wisp=null;human.Shop.SetMetadata(0,null);break;
                case 6:human.Shop.SetMetadata(0,new(Tag:"Partner"));break;
                case 7:human.Shop.SetMetadata(0,new(Tag:"Partner",Note:"Offer note with 25% text."));break;
            }
            initialGold=human.Economy.Gold;
        }
        if(scenario>=wisps.Length+5)
        {
            if(frame>=2)Hover(screen,"buy/0");
        }
        else
        {
            int action=scenario-wisps.Length;
            if((action is 1 or 2)&&(frame is 1 or 2))Click(screen,action==1?"view/1":"view/0",frame==1);
            if(frame>=2&&!((action is 1 or 2)&&frame==2))Hover(screen,"wisp");
            if((action is 0 or 1 or 2 or 4)&&(frame is 4 or 5))Click(screen,"wisp",frame==4);
        }
        frame++;
    }
    private static void Hover(MatchScreen screen,string key)
    {
        if(!screen.Buttons.TryGetValue(key,out var point))throw new InvalidOperationException("Missing Wisp smoke target "+key);
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y);
    }
    private static void Click(MatchScreen screen,string key,bool down){Hover(screen,key);ImGui.GetIO().AddMouseButtonEvent(0,down);}
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(frame!=8)return false;
        var human=screen.Session!.State.Players[0];
        if(scenario<wisps.Length)
        {
            var wisp=wisps[scenario];
            string expected=$"{wisp.Name}\n{wisp.Kind} Wisp · {wisp.ExampleCost} gold · one use, this round only\n{wisp.Description}";
            if(screen.WispTooltipSerial<=hoverBefore||screen.WispTooltipText!=expected||screen.WispArtworkDrawCalls!=2||screen.DisplayedWisp!=wisp.Id)
                throw new InvalidOperationException("Original Wisp artwork/description/round limit was not rendered: "+Pending);
            var bounds=screen.WispCardBounds;var viewport=screen.ShopViewportBounds;
            if(bounds.Min.X<viewport.Min.X||bounds.Max.X>viewport.Max.X+1||bounds.Min.Y<viewport.Min.Y||bounds.Max.Y>viewport.Max.Y||bounds.Max.X-bounds.Min.X<150||screen.ShopCardBounds.Count!=5
                ||screen.ShopCardBounds.Any(card=>card.Value.Max.X-card.Value.Min.X<150||card.Value.Max.X>=bounds.Min.X))
                throw new InvalidOperationException("Sixth-slot layout clipped the Wisp or five unit cards: "+Pending);
        }
        else switch(scenario-wisps.Length)
        {
            case 0:if(human.Economy.Gold!=0||human.Wisp?.Id!="gold")throw new InvalidOperationException("Unaffordable Wisp purchase mutated the offer/economy.");break;
            case 1:if(screen.ViewedPlayer!=1||human.Economy.Gold!=initialGold||human.Wisp?.Id!="gold")throw new InvalidOperationException("Scouting Wisp purchase bypassed own-board guard.");break;
            case 2:if(screen.ViewedPlayer!=0||human.Wisp is not null||human.Economy.Gold!=initialGold+2)throw new InvalidOperationException("Gold Wisp click did not charge, apply and consume exactly once.");break;
            case 3:if(screen.WispTooltipSerial<=hoverBefore||screen.WispTooltipText!="A Data Wisp appears here every other round from stage 2"||human.Wisp is not null)throw new InvalidOperationException("Consumed Wisp did not become the source empty sixth slot.");break;
            case 4:if(human.Wisp?.Id!="gold"||human.Economy.Gold!=initialGold)throw new InvalidOperationException("Combat Wisp purchase bypassed planning-only rule.");break;
            default:
                string id=human.Shop.Slots[0]!;var line=screen.Session.State.Rules.Lines.Single(l=>l.Id==id);
                string prefix=(scenario-wisps.Length) switch{6=>"Partner offer\n",7=>"Partner offer: Offer note with 25% text.\n",_=>""};
                string footer=prefix+$"{screen.Session.State.Pool.Remaining(id)} more in the shared pool (of {screen.Session.State.Rules.PoolSizes[line.Tier]})";
                if(screen.ShopTooltipSerial<=hoverBefore||screen.ShopTooltipFooter!=footer)throw new InvalidOperationException("Shop hover lost source offer note or shared-pool availability: "+Pending);
                break;
        }
        if(capture is not null&&((scenario<wisps.Length&&(wisps[scenario].Id is "gold" or "evolve" or "iron"))||scenario>=wisps.Length))
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture,$"wisp-{scenario:00}.bmp"),size.X,size.Y);
        scenario++;frame=0;
        if(scenario<wisps.Length+8)return false;
        Console.WriteLine("Data Wisp UI passed: all15 source effects/kinds/art, readable sixth slot, poor/scouted/combat guards, actual one-use purchase, empty-slot hint, and plain/tagged/noted pool tooltips.");return true;
    }
}
