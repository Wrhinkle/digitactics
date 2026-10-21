using System.Reflection;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Real carousel input across reachable opening/late rounds, including another human's turn and lower-row choices.</summary>
internal sealed class CarouselUiSmoke
{
    private static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    private readonly (Ruleset Rules,RoundPosition Round)[] cases;
    private int index,step,cooldown,selected,units,items,frames;
    private string? selectedItem,selectedLine,captureState;
    private bool down;
    public string Pending=>$"Carousel UI {index}/{cases.Length}, step {step}";
    public CarouselUiSmoke(ContentCatalog catalog)
    {
        cases=catalog.Variants.SelectMany(r=>new[]{1,5}.SelectMany(stage=>Enumerable.Range(1,r.Schedule!.RoundsInStage(stage))
            .Select(round=>new RoundPosition(stage,round)).Where(round=>r.Schedule.TypeOf(round)=="carousel").Take(1).Select(round=>(r,round)))).ToArray();
        Require(cases.Length>0,"No reachable source carousel rounds were found.");
    }
    private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    private void Create(MatchScreen screen)
    {
        typeof(MatchScreen).GetMethod("LeaveMatch",Private)!.Invoke(screen,null);
        var current=cases[index];MatchPlanningSession? session=null;
        // All seats are human so the fixture controls remote picks independently
        // of the actual local mouse input, including rejected out-of-turn clicks.
        for(uint seed=771;seed<803;seed++)
        {
            session=new(current.Rules,seed,Enumerable.Range(0,8).Select(i=>new MatchSeat(i==0?"Carousel 100% ## Ω":$"Long carousel opponent number {i}",true)).ToArray());
            if(current.Round!=RoundPosition.First)
            {
                session.Units.Carousel.Finish(MatchPhase.Planning);
                foreach(var player in session.State.Players)
                {
                    foreach(var entry in player.Roster.Entries.ToArray())player.Roster.Remove(entry.Unit.Id,false);
                    player.Health=player.Id==0?75:50+player.Id;
                }
                typeof(MatchRounds).GetProperty(nameof(MatchRounds.Position))!.SetValue(session.Rounds,current.Round);
                session.Units.Carousel.Start(current.Round.Stage,MatchPhase.Planning);
            }
            if(session.Units.Carousel.Turn?.Id is not (null or 0))break;
        }
        Require(session?.Units.Carousel.Active==true&&session.Units.Carousel.Turn?.Id is not (null or 0),"Carousel fixture did not begin on another human's turn.");
        typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen,session);
        typeof(MatchScreen).GetField("localPaused",Private)!.SetValue(screen,true);
        typeof(MatchScreen).GetField("viewed",Private)!.SetValue(screen,0);
        typeof(MatchScreen).GetField("localSeat",Private)!.SetValue(screen,0);
        selected=session!.Units.Carousel.Options.Count-1;
        units=session.State.Players[0].Roster.Entries.Count();items=session.State.Players[0].Items.Count;
        ImGui.GetIO().AddMousePosEvent(2,2);cooldown=4;frames=0;
    }
    private bool Visible(MatchScreen screen,int choice)
    {
        if(!screen.CarouselCardBounds.TryGetValue(choice,out var card))return false;
        var viewport=screen.CarouselChoicesBounds;
        if(card.Min.Y>=viewport.Min.Y&&card.Max.Y<=viewport.Max.Y)return true;
        ImGui.GetIO().AddMousePosEvent(viewport.Max.X-3,(viewport.Min.Y+viewport.Max.Y)/2);
        ImGui.GetIO().AddMouseWheelEvent(0,card.Min.Y<viewport.Min.Y?1:-1);cooldown=3;return false;
    }
    private void Click(MatchScreen screen,string key)
    {
        if(!screen.Buttons.TryGetValue(key,out var at))throw new InvalidOperationException("Missing carousel action: "+key);
        ImGui.GetIO().AddMousePosEvent(at.X,at.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;
    }
    private static void RemotePick(MatchPlanningSession session)
    {
        var carousel=session.Units.Carousel;
        int choice=Enumerable.Range(0,carousel.Options.Count).First(i=>carousel.Options[i].PickedBy is null);
        Require(session.PickCarousel(carousel.Turn!,choice),"Fixture remote pick was rejected.");
    }
    public void Input(MatchScreen screen)
    {
        if(index>=cases.Length)return;
        if(++frames>400)throw new InvalidOperationException(Pending+" did not reach its visible input target.");
        if(cooldown-->0)return;
        if(down){ImGui.GetIO().AddMouseButtonEvent(0,false);down=false;step++;cooldown=3;return;}
        if(step==0){Create(screen);step=1;return;}
        var session=screen.Session!;var carousel=session.Units.Carousel;var player=session.State.Players[0];
        bool captureOpening=index==0,captureLate=cases[index].Rules.Id=="reckoning/normal"&&cases[index].Round.Stage==5;
        switch(step)
        {
            case 1:
                if(!Visible(screen,selected))return;
                if(captureOpening)captureState="waiting";
                Click(screen,"carousel/"+selected);break;
            case 2:
                Require(carousel.Options[selected].PickedBy is null&&player.Roster.Entries.Count()==units&&player.Items.Count==items,"Out-of-turn click awarded carousel contents.");
                while(carousel.Turn?.Id is not (null or 0))RemotePick(session);
                Require(carousel.Turn==player,"Remote picks did not reach the local player.");
                selected=Enumerable.Range(0,carousel.Options.Count).Last(i=>carousel.Options[i].PickedBy is null);
                selectedLine=carousel.Options[selected].Line;selectedItem=carousel.Options[selected].Item;
                cooldown=3;step=3;break;
            case 3:
                if(!Visible(screen,selected))return;
                var at=screen.Buttons["carousel/"+selected];ImGui.GetIO().AddMousePosEvent(at.X,at.Y);cooldown=3;step=4;break;
            case 4:
                Require(screen.CarouselHoveredOption==selected,"Carousel hover did not expose the complete unit details.");
                if(captureOpening)captureState="tooltip";
                cooldown=2;step=5;break;
            case 5:ImGui.GetIO().AddMousePosEvent(2,2);cooldown=3;step=6;break;
            case 6:
                if(captureOpening)captureState="opening-choices";
                if(captureLate)captureState="late-choices";
                cooldown=2;step=7;break;
            case 7:
                if(!Visible(screen,selected))return;
                Click(screen,"carousel/"+selected);break;
            case 8:
                Require(carousel.Options[selected].PickedBy==player.Name&&player.Roster.Entries.Count()==units+1&&player.Roster.Entries.Any(e=>e.Unit.Line==selectedLine)&&player.Items.Count==items+(selectedItem is null?0:1),"Local lower-row choice did not award exactly the selected unit and item.");
                if(!Visible(screen,selected))return;
                if(captureOpening)captureState="taken";
                Click(screen,"carousel/"+selected);break;
            case 9:
                Require(player.Roster.Entries.Count()==units+1&&player.Items.Count==items+(selectedItem is null?0:1),"Taken carousel choice awarded contents again.");
                while(carousel.Turn is not null)RemotePick(session);
                ImGui.GetIO().AddMousePosEvent(2,2);cooldown=3;step=10;break;
            case 10:Require(carousel.Done,"Carousel completion fixture still has a pending pick.");if(captureOpening)captureState="complete";Click(screen,"carouselGo");break;
            case 11:
                Require(!carousel.Active&&session.Rounds.Position==cases[index].Rules.Schedule!.Next(cases[index].Round)&&session.Rounds.Phase==MatchPhase.Planning,"Start round did not navigate to the source next planning round.");
                index++;step=0;cooldown=2;break;
        }
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(index>=cases.Length)
        {
            Console.WriteLine($"Carousel UI passed: {cases.Length} reachable opening/late cases, readable complete card text, actual lower-row scrolling, hover details, rejected waiting/taken clicks, exact unit/item awards and next-round navigation.");return true;
        }
        if(screen.Session?.Units.Carousel.Active==true&&step is 1 or 3 or 4 or 5 or 6 or 7 or 10)
        {
            Require(screen.CarouselCardsDrawn==screen.Session.Units.Carousel.Options.Count&&screen.CarouselTextFits,"Carousel dropped a source option or clipped text inside a card: "+Pending);
            var viewport=screen.CarouselChoicesBounds;
            Require(viewport.Min.X>=0&&viewport.Min.Y>=0&&viewport.Max.X<=size.X&&viewport.Max.Y<=size.Y,"Carousel choices leave the visible window.");
            foreach(var bounds in screen.CarouselCardBounds.Values)Require(bounds.Min.X>=viewport.Min.X-1&&bounds.Max.X<=viewport.Max.X+1,"Carousel columns exceed their visible width.");
            foreach(var row in screen.CarouselCardBounds.Values.GroupBy(bounds=>Math.Round(bounds.Min.Y)))
                Require(Math.Abs((row.Min(bounds=>bounds.Min.X)+row.Max(bounds=>bounds.Max.X))/2-(viewport.Min.X+viewport.Max.X)/2)<12,"A partial carousel row lost the source centered layout.");
            if(step==10&&screen.Buttons.TryGetValue("carouselGo",out var footer))Require(footer.Y-20>=viewport.Max.Y&&footer.Y+20<=size.Y,"Carousel Continue is clipped or overlaps its scrolling choices.");
        }
        if(captureState is {} state&&capture is not null){FrameCapture.SaveBitmap(Path.ChangeExtension(capture,state+".bmp"),size.X,size.Y);captureState=null;}
        return false;
    }
}
