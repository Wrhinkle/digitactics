using System.Reflection;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class ChoicePresentationUiSmoke(ContentCatalog catalog)
{
    private static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    private readonly ChoiceArtSample[] samples=Samples(catalog);
    private int index,frame,phase,longSettled;
    private bool down,longScrolled;
    private string? selected;
    private string bounds="";
    public string Pending=>$"Choice presentation {index}/{phase}/{frame} {bounds}";
    private static ChoiceArtSample[] Samples(ContentCatalog catalog)
    {
        const string variant="dragonlands/normal",kind="drg_treasure";
        string unit=catalog.Variants.Single(r=>r.Id==variant).Lines.First(l=>l.Tier==5).Id;
        return [..ChoiceArtwork.Samples,..new[]{"gold:25","unit:"+unit,"chaos"}.Select(id=>new ChoiceArtSample(variant,kind,id,ChoiceArtwork.Resolve(variant,kind,id)!)),
            new("underlords/normal","ul_tamer","ul_enno_balm",ChoiceArtwork.Resolve("underlords/normal","ul_tamer","ul_enno_balm")!)];
    }
    private static void Require(bool valid,string error){if(!valid)throw new InvalidOperationException(error);}
    private void Create(MatchScreen screen,string variant)
    {
        typeof(MatchScreen).GetMethod("LeaveMatch",Private)!.Invoke(screen,null);
        var rules=catalog.Variants.Single(r=>r.Id==variant);
        var session=new MatchPlanningSession(rules,771u,Enumerable.Range(0,8).Select(i=>new MatchSeat("Choice Tamer "+i,i==0)).ToArray());
        if(session.Units.Carousel.Active){session.StartCombat();session.FinishCombat();session.Advance();}
        typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen,session);
        typeof(MatchScreen).GetField("localPaused",Private)!.SetValue(screen,true);
        typeof(MatchScreen).GetField("viewed",Private)!.SetValue(screen,0);
        typeof(MatchRounds).GetProperty(nameof(MatchRounds.Position))!.SetValue(session.Rounds,new RoundPosition(2,1));
        ImGui.GetIO().AddMousePosEvent(2,2);
    }
    private static void Offer(MatchScreen screen,string kind,string[] ids,int rerolls=0)=>
        typeof(MatchPlayer).GetProperty(nameof(MatchPlayer.Offer))!.SetValue(screen.Session!.State.Players[0],new PlayerOffer(kind,ids,rerolls,null));
    private bool Click(MatchScreen screen,string id)
    {
        if(down){ImGui.GetIO().AddMouseButtonEvent(0,false);down=false;return true;}
        if(!screen.Buttons.TryGetValue(id,out var at))return false;
        ImGui.GetIO().AddMousePosEvent(at.X,at.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;return false;
    }
    public void Input(MatchScreen screen)
    {
        if(index<samples.Length)
        {
            if(frame==0)
            {
                longSettled=0;longScrolled=false;
                var sample=samples[index];
                if(screen.Session?.State.Rules.Id!=sample.Variant)Create(screen,sample.Variant);
                Offer(screen,sample.Kind,sample.Id=="ul_enno_balm"?["ul_anessa_dark","ul_hob_fire","ul_jules_brawl","ul_enno_balm"]:[sample.Id,sample.Id,sample.Id]);
            }
            if(frame>=4&&samples[index].Id=="ul_enno_balm"&&screen.ChoiceCardBounds.TryGetValue(3,out var card))
            {
                if(card.Max.Y>screen.ChoiceViewportBounds.Max.Y)
                {
                    var viewport=screen.ChoiceViewportBounds;
                    ImGui.GetIO().AddMousePosEvent(viewport.Max.X-3,(viewport.Min.Y+viewport.Max.Y)/2);ImGui.GetIO().AddMouseWheelEvent(0,-2);longSettled=0;longScrolled=true;
                }
                else{ImGui.GetIO().AddMousePosEvent(2,2);longSettled++;}
            }
            frame++;return;
        }
        if(phase==0)
        {
            Create(screen,"gizmos/normal");screen.Session!.State.GizmosGates=[1,2,3];
            var ids=screen.Session.State.Rules.Offers!.Augments.Where(a=>a.Tier==1).Take(3).Select(a=>a.Id).ToArray();
            Offer(screen,"upgrade",ids,1);phase++;frame=0;return;
        }
        if(frame++<4)return;
        switch(phase)
        {
            case 1:if(Click(screen,"peekChoices")){phase++;frame=0;}break;
            case 2:if(Click(screen,"peekChoices")){phase++;frame=0;}break;
            case 3:if(Click(screen,"offerReroll")){phase++;frame=0;}break;
            case 4:
                screen.Session!.State.GizmosGates=[3,1,2];phase++;frame=0;break;
            case 5:
                selected=screen.Session!.State.Players[0].Offer!.Options[0];
                if(Click(screen,"offer/0")){phase++;frame=0;}break;
        }
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        void Capture(string name){if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,name+".bmp"),size.X,size.Y);}
        if(index<samples.Length)
        {
            if(frame<4)return false;
            var sample=samples[index];
            var offered=screen.Session!.State.Players[0].Offer!.Options;
            Require(screen.OfferArtworkCount==offered.Count(id=>ChoiceArtwork.Resolve(sample.Variant,sample.Kind,id)?.File is not null)&&screen.DisplayedChoiceArt.Count==offered.Length&&screen.DisplayedChoiceArt.All(pair=>pair.Value==ChoiceArtwork.Resolve(sample.Variant,sample.Kind,offered[pair.Key])),"Original choice art was not rendered at real card width: "+sample);
            Require(screen.ChoiceTextFits,"A source choice description is clipped inside its card: "+sample);
            Require(screen.ChoiceCardBounds.Values.All(card=>card.Max.X-card.Min.X<=245),"Source compact choice cards were stretched across the modal.");
            Require(screen.Buttons["peekChoices"].Y<screen.ChoiceViewportBounds.Min.Y,"View board left the source modal heading row.");
            // These ordinary three-card offers fit comfortably even at the
            // minimum window. Their borders must be visible without scrolling;
            // text fitting inside an offscreen card is not sufficient.
            // The original modal lives in the arena (max-width min(860px, 100% - 24px)), so a narrow
            // arena wraps wide cards onto a second row; only a single-row offer must avoid scrolling.
            if(offered.Length<=3&&screen.ChoiceCardBounds.Values.All(card=>card.Max.Y-card.Min.Y<=320)&&screen.ChoiceCardBounds.Values.Select(card=>card.Min.Y).Distinct().Count()==1)
            {
                var viewport=screen.ChoiceViewportBounds;
                Require(screen.ChoiceCardBounds.Values.All(card=>card.Min.X>=viewport.Min.X&&card.Max.X<=viewport.Max.X&&card.Min.Y>=viewport.Min.Y&&card.Max.Y<=viewport.Max.Y),
                    "An ordinary single-row offer clips a card border: "+sample+" viewport="+viewport+" cards="+string.Join("; ",screen.ChoiceCardBounds.Values));
                Require(screen.ChoiceScrollMaxY<1,"An ordinary single-row offer requires unnecessary scrolling: "+sample);
            }
            if(sample.Id=="ul_enno_balm")
            {
                if(frame==4)Capture("long-description-top");
                bounds=$"window={size} viewport={screen.ChoiceViewportBounds} scrollMaxY={screen.ChoiceScrollMaxY} cards="+
                    string.Join("; ",screen.ChoiceCardBounds.Select(pair=>$"{pair.Key}: {pair.Value}"));
                if(longSettled<3||screen.ChoiceCardBounds[3].Max.Y>screen.ChoiceViewportBounds.Max.Y)return false;
                Require(screen.ChoiceScrollMaxY<1||longScrolled,"The lower Tamer row did not exercise actual scrolling. "+bounds);
                Require(screen.ChoiceCardBounds.Values.All(card=>card.Min.X>=screen.ChoiceViewportBounds.Min.X&&card.Max.X<=screen.ChoiceViewportBounds.Max.X),"Scrolling the long Tamer description clips a choice column. "+bounds);
                Capture("long-description-scrolled");
            }
            if(sample.Kind=="augment")
            {
                string tier=sample.Art.Tier switch{1=>"Silver",2=>"Gold",3=>"Prismatic",_=>"Silver"};
                Require(screen.DisplayedOfferTitle==tier+" Augment"&&screen.DisplayedOfferDescription=="Pick one augment. It lasts the whole game.","Augment tier or selection instructions differ from the source.");
            }
            if(sample.Variant.EndsWith("/normal",StringComparison.Ordinal)&&sample.Kind!="augment"||sample.Variant=="faction-wars/normal")
                Capture(sample.Variant.Replace('/','-')+"-"+sample.Kind+(sample.Id=="ul_enno_balm"?"-long":sample.Art.Tier is {} tier?"-"+tier:sample.Art.File is null?"-text-"+index:""));
            index++;frame=0;return false;
        }
        if(frame!=4)return false;
        string suffix=phase>=5?"Gates: Prismatic, Silver, Gold.":"Gates: Silver, Gold, Prismatic.";
        if(phase<=5)Require(screen.DisplayedOfferDescription?.EndsWith(suffix,StringComparison.Ordinal)==true,"Current game's Hextech Gates schedule is missing or stale.");
        if(screen.Buttons.TryGetValue("offerReroll",out var reroll)&&!screen.PeekingChoices)
            Require(reroll.Y-19>=screen.ChoiceViewportBounds.Max.Y&&reroll.Y+19<=size.Y,"Reroll left the source footer or overlaps scrolling choices.");
        switch(phase)
        {
            case 1:Capture("gates");break;
            case 2:Require(screen.PeekingChoices&&screen.OfferArtworkCount==0&&!screen.Buttons.ContainsKey("offer/0"),"View board retained hidden choice targets/art.");break;
            case 3:Require(!screen.PeekingChoices&&screen.OfferArtworkCount==3,"Show choices did not restore source art.");break;
            case 4:Require(screen.Session!.State.Players[0].Offer!.Rerolls==0&&screen.OfferArtworkCount==3,"Real upgrade reroll lost source cards.");Capture("gates-rerolled");break;
            case 5:Capture("gates-changed");break;
            case 6:
                var player=screen.Session!.State.Players[0];Require(player.Offer is null&&player.Augments.Contains(selected!),"Actual upgrade pick did not grant the selected augment.");
                Console.WriteLine($"Choice presentation UI passed {samples.Length} source artwork/tier routes across 22 variants, plus long-description scrolling, live Gates schedule, View board/show, reroll and actual upgrade pick.");return true;
        }
        return false;
    }
}
