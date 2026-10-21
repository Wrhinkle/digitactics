using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

/// <summary>Uses the actual Loot selection and Digivolve controls before the existing egg transaction smoke.</summary>
internal sealed class IncubatorSmoke
{
    private readonly string[] commands=["lootEgg/legend","lootContents","lootContentsBottom","lootContents","lootLegend/bukamon","lootDigivolve","closeEvolution","lootEgg/legend"];
    private int step,cooldown=3,contentScrolls;
    private bool down,checkedEgg,checkedLegend,checkedExpanded,checkedContentsBottom;
    public bool Done {get;private set;}
    public string? CaptureRequested {get;set;}
    public string Pending => commands.ElementAtOrDefault(step) ?? "Incubator verification";
    public void Input(MatchScreen screen)
    {
        if(Done||cooldown-->0)return;
        var input=ImGui.GetIO();
        if(down){input.AddMouseButtonEvent(0,false);down=false;cooldown=commands.ElementAtOrDefault(step)=="lootContents"?6:3;step++;return;}
        if(step==1&&!checkedEgg)
        {
            var egg=screen.DisplayedIncubator?.Eggs.Single(candidate=>candidate.Definition.Id=="legend");
            if(screen.IncubatorSelected!="legend"||egg is null||egg.Exclusives.Length==0||screen.IncubatorExclusiveCount!=egg.Exclusives.Length||egg.Total<egg.Owned)
                throw new InvalidOperationException("Incubator did not display selected egg's exclusive odds and owned/pool totals.");
            checkedEgg=true;CaptureRequested="incubator-egg";cooldown=3;return;
        }
        if(step==2)
        {
            if(!screen.IncubatorContentsExpanded||screen.IncubatorContentRows!=screen.IncubatorExclusiveCount||screen.IncubatorContentRows==0
                ||screen.IncubatorInformationBounds is not {} information||screen.IncubatorLastContentBounds is not {} last)
                throw new InvalidOperationException("Incubator disclosure did not expand into all exclusive content and duplicate-reward details.");
            if(!checkedExpanded){checkedExpanded=true;CaptureRequested="incubator-egg-contents";cooldown=3;return;}
            if(last.Max.Y>information.Max.Y-10)
            {
                if(++contentScrolls>40)throw new InvalidOperationException("Incubator disclosure could not scroll to its final exclusive item.");
                input.AddMousePosEvent((information.Min.X+information.Max.X)*.5f,(information.Min.Y+information.Max.Y)*.5f);
                input.AddMouseWheelEvent(0,-4);cooldown=3;return;
            }
            if(last.Min.Y<information.Min.Y+10||last.Min.X<information.Min.X||last.Max.X>information.Max.X
                ||screen.IncubatorInformationScrollMax>1&&screen.IncubatorInformationScroll<=0)
                throw new InvalidOperationException("Incubator disclosure's final content row was clipped or unreachable by scrolling.");
            checkedContentsBottom=true;CaptureRequested="incubator-egg-contents-bottom";step++;cooldown=3;return;
        }
        if(step==4&&screen.IncubatorContentsExpanded)throw new InvalidOperationException("Incubator disclosure did not collapse after scrolling back to its header.");
        if(step==5&&!checkedLegend)
        {
            if(screen.IncubatorSelected!="bukamon"||screen.CollectionPreviewKind!="legend"||screen.CollectionPreviewDrawCalls<=0||!screen.Buttons.ContainsKey("lootDigivolve"))
                throw new InvalidOperationException("Incubator omitted the owned legend preview/progress and evolution action.");
            checkedLegend=true;CaptureRequested="incubator-legend";cooldown=3;return;
        }
        if(step>=commands.Length)
        {
            if(!checkedEgg||!checkedLegend||!checkedExpanded||!checkedContentsBottom||screen.IncubatorSelected!="legend")throw new InvalidOperationException("Incubator navigation did not return from Digivolve to the selected egg.");
            Done=true;Console.WriteLine("Incubator UI passed: egg-specific odds, owned/pool totals, expanded contents scrolling to the final item and collapse, owned legend preview, inventory scrolling and Digivolve return.");return;
        }
        string key=commands[step];
        if(!screen.Buttons.TryGetValue(key,out var target))return;
        if(key=="lootContents")
        {
            if(screen.IncubatorInformationBounds is not {} information)return;
            if(target.Y<information.Min.Y+18||target.Y>information.Max.Y-18)
            {
                input.AddMousePosEvent((information.Min.X+information.Max.X)*.5f,(information.Min.Y+information.Max.Y)*.5f);
                input.AddMouseWheelEvent(0,target.Y<information.Min.Y?4:-4);cooldown=3;return;
            }
        }
        if(key.StartsWith("lootEgg/",StringComparison.Ordinal)||key.StartsWith("lootLegend/",StringComparison.Ordinal))
        {
            if(screen.IncubatorInventoryBounds is not { } bounds)return;
            if(target.Y<bounds.Min.Y+14||target.Y>bounds.Max.Y-14)
            {
                input.AddMousePosEvent((bounds.Min.X+bounds.Max.X)*.5f,(bounds.Min.Y+bounds.Max.Y)*.5f);
                input.AddMouseWheelEvent(0,target.Y<bounds.Min.Y?4:-4);cooldown=3;return;
            }
        }
        input.AddMousePosEvent(target.X,target.Y);input.AddMouseButtonEvent(0,true);down=true;
    }
}
