using System.Reflection;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Original Galaxy announcements, with actual dismiss/carousel inputs and per-match lifetime.</summary>
internal sealed class GalaxyIntroUiSmoke(ContentCatalog catalog)
{
    private static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    private readonly string[] galaxies=MatchScreen.GalaxyAnnouncements.Value.Keys.ToArray();
    private int index,frame,phase;
    private long serial;
    private bool down;
    public string Pending=>$"Galaxy announcement {index}/{phase}/{frame}";
    private static void Require(bool value,string error){if(!value)throw new InvalidOperationException(error);}
    private void Create(MatchScreen screen,string id,string pace="normal")
    {
        typeof(MatchScreen).GetMethod("LeaveMatch",Private)!.Invoke(screen,null);
        var rules=catalog.Variants.Single(r=>r.Id=="galaxies/"+pace);
        Require(rules.GameStart!.Galaxies.Select(g=>g.Id).Order().SequenceEqual(galaxies.Order()),"Galaxy announcements do not cover the original seeded pool.");
        var session=new MatchPlanningSession(rules,771u,Enumerable.Range(0,8).Select(i=>new MatchSeat("Galaxy Tamer "+i,i==0)).ToArray());
        session.State.Galaxy=id;
        typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen,session);
        typeof(MatchScreen).GetField("localPaused",Private)!.SetValue(screen,true);
        typeof(MatchScreen).GetField("viewed",Private)!.SetValue(screen,0);
        ImGui.GetIO().AddMousePosEvent(2,2);
    }
    private bool Click(MatchScreen screen,string key)
    {
        if(down){ImGui.GetIO().AddMouseButtonEvent(0,false);down=false;return true;}
        if(!screen.Buttons.TryGetValue(key,out var at))return false;
        if(key.StartsWith("carousel/",StringComparison.Ordinal))
        {
            var b=screen.CarouselChoicesBounds;
            var card=screen.CarouselCardBounds[int.Parse(key.AsSpan("carousel/".Length))];
            if(card.Min.Y<b.Min.Y||card.Max.Y>b.Max.Y)
            {ImGui.GetIO().AddMousePosEvent(b.Max.X-3,(b.Min.Y+b.Max.Y)/2);ImGui.GetIO().AddMouseWheelEvent(0,card.Min.Y<b.Min.Y?1:-1);return false;}
        }
        ImGui.GetIO().AddMousePosEvent(at.X,at.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;return false;
    }
    public void Input(MatchScreen screen)
    {
        serial=screen.GalaxyIntroSerial;
        if(index<galaxies.Length)
        {
            if(frame==0)Create(screen,galaxies[index],index%2==0?"normal":"hyper");
            if(frame is 4 or 5)Click(screen,"galaxyIntroDismiss");
            frame++;return;
        }
        if(phase==0){Create(screen,galaxies[0]);phase++;frame=0;return;}
        if(frame++<3)return;
        switch(phase)
        {
            case 1:
                var carousel=screen.Session!.Units.Carousel;
                int option=Enumerable.Range(0,carousel.Options.Count).First(i=>carousel.Options[i].PickedBy is null);
                if(Click(screen,"carousel/"+option)){phase++;frame=0;}break;
            case 2:if(Click(screen,"carouselGo")){phase++;frame=0;}break;
            case 3:if(Click(screen,"galaxyIntroDismiss")){phase++;frame=0;}break;
            case 4:Create(screen,galaxies[0]);phase++;frame=0;break;
            case 5:
                typeof(MatchRounds).GetProperty(nameof(MatchRounds.Position))!.SetValue(screen.Session!.Rounds,new RoundPosition(2,1));phase++;frame=0;break;
        }
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        void Capture(string name){if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,name+".bmp"),size.X,size.Y);}
        void Visible(string id)
        {
            var expected=MatchScreen.GalaxyAnnouncements.Value[id];
            Require(screen.GalaxyIntroSerial>serial&&screen.GalaxyIntroText==expected.Title+"\n"+expected.Name+"\n"+expected.Description,"Source Galaxy rule was not rendered this frame: "+Pending);
            Require(screen.GalaxyIntroBodyFits,"Galaxy rule text exceeds its viewport: "+id);
            var b=screen.GalaxyIntroBounds;var button=screen.Buttons["galaxyIntroDismiss"];
            Require(b.Min.X>=0&&b.Max.X<=size.X&&b.Min.Y>=0&&b.Max.Y<=size.Y&&button.X-44>=b.Min.X&&button.X+44<=b.Max.X&&button.Y-19>=b.Min.Y&&button.Y+19<=b.Max.Y,"Galaxy announcement or dismiss button is clipped.");
        }
        if(index<galaxies.Length)
        {
            if(frame==4){Visible(galaxies[index]);if(index is 0 or 5||index==galaxies.Length-1)Capture("galaxy-"+galaxies[index]);}
            if(frame<8)return false;
            Require(screen.GalaxyIntroText is null&&!screen.Buttons.ContainsKey("galaxyIntroDismiss"),"Got it did not dismiss the current match's announcement.");
            index++;frame=0;return false;
        }
        if(frame==3)
        {
            if(phase==3){Visible(galaxies[0]);Require(!screen.Session!.Units.Carousel.Active,"Carousel did not advance through its real footer.");Capture("planning");}
            if(phase==4)Require(screen.GalaxyIntroText is null,"Dismissed planning announcement reappeared.");
            if(phase==5){Visible(galaxies[0]);Capture("new-match");}
            if(phase==6)
            {
                Require(screen.GalaxyIntroText is null&&!screen.Buttons.ContainsKey("galaxyIntroDismiss"),"Galaxy intro remained after stage one.");
                Console.WriteLine($"Galaxy announcement UI passed: all {galaxies.Length} source rules/art, both paces, actual dismiss, carousel and planning continuity, new-match reset and stage-two expiry.");return true;
            }
        }
        return false;
    }
}
