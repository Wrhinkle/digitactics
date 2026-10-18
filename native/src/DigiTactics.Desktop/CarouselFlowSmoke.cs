using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Uses the displayed choices and footer, including a rejected repeated pick.</summary>
internal sealed class CarouselFlowSmoke
{
    private int step,selected,units,items;
    private string line="",round="";
    private string? item;
    public string Pending=>"Carousel step "+step;
    public string CaptureName=>step<2?"carousel":"carousel-picked";
    public bool Input(MatchScreen screen)
    {
        var session=screen.Session!;var player=session.State.Players[0];var carousel=session.Units.Carousel;
        var io=ImGui.GetIO();
        bool Click(string key,bool choice=false)
        {
            if(!screen.Buttons.TryGetValue(key,out var point))return false;
            if(choice)
            {
                var bounds=screen.CarouselChoicesBounds;
                if(point.Y<bounds.Min.Y+10||point.Y>bounds.Max.Y-10)
                {io.AddMousePosEvent((bounds.Min.X+bounds.Max.X)/2,(bounds.Min.Y+bounds.Max.Y)/2);io.AddMouseWheelEvent(0,point.Y<bounds.Min.Y?1:-1);return false;}
            }
            io.AddMousePosEvent(point.X,point.Y);io.AddMouseButtonEvent(0,true);return true;
        }
        switch(step)
        {
            case 0:
                if(carousel.Turn!=player)throw new InvalidOperationException("Carousel fixture did not reach the local player's turn.");
                selected=carousel.Options.Select((option,index)=>(option,index)).First(pair=>pair.option.PickedBy is null).index;
                var option=carousel.Options[selected];line=option.Line;item=option.Item;
                units=player.Roster.Entries.Count();items=player.Items.Count;round=session.Rounds.Position.Label;
                if(!Click("carousel/"+selected,true))return false;break;
            case 1:io.AddMouseButtonEvent(0,false);break;
            case 2:
                if(carousel.Options[selected].PickedBy!=player.Name||!carousel.Done||player.Roster.Entries.Count()!=units+1||!player.Roster.Entries.Any(entry=>entry.Unit.Line==line)||player.Items.Count!=items+(item is null?0:1)||item is not null&&!player.Items.Contains(item))
                    throw new InvalidOperationException("Carousel click did not award exactly the selected unit and item or complete the remaining AI picks.");
                if(!screen.Buttons.ContainsKey("carouselGo"))throw new InvalidOperationException("Completed carousel has no visible Start round action.");
                if(!Click("carousel/"+selected,true))return false;break;
            case 3:io.AddMouseButtonEvent(0,false);break;
            case 4:
                if(player.Roster.Entries.Count()!=units+1||player.Items.Count!=items+(item is null?0:1))throw new InvalidOperationException("Taken carousel choice awarded its contents twice.");
                if(!Click("carouselGo"))return false;break;
            case 5:io.AddMouseButtonEvent(0,false);break;
            case 6:
                if(carousel.Active||session.Rounds.Position.Label==round||session.Rounds.RoundType=="carousel"||session.Rounds.Phase!=MatchPhase.Planning||screen.Buttons.ContainsKey("carouselGo"))
                    throw new InvalidOperationException("Start round did not leave the carousel for the next planning round.");
                Console.WriteLine("Carousel input passed: selected unit/item, rejected repeat pick, visible Start round and next planning round.");return true;
        }
        step++;return false;
    }
}

