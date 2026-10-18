using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

/// <summary>Uses the actual Loot selection and Digivolve controls before the existing egg transaction smoke.</summary>
internal sealed class IncubatorSmoke
{
    private readonly string[] commands=["lootEgg/legend","lootLegend/bukamon","lootDigivolve","closeEvolution","lootEgg/legend"];
    private int step,cooldown=3;
    private bool down,checkedEgg,checkedLegend;
    public bool Done {get;private set;}
    public string? CaptureRequested {get;set;}
    public string Pending => commands.ElementAtOrDefault(step) ?? "Incubator verification";
    public void Input(MatchScreen screen)
    {
        if(Done||cooldown-->0)return;
        var input=ImGui.GetIO();
        if(down){input.AddMouseButtonEvent(0,false);down=false;step++;cooldown=3;return;}
        if(step==1&&!checkedEgg)
        {
            var egg=screen.DisplayedIncubator?.Eggs.Single(candidate=>candidate.Definition.Id=="legend");
            if(screen.IncubatorSelected!="legend"||egg is null||egg.Exclusives.Length==0||screen.IncubatorExclusiveCount!=egg.Exclusives.Length||egg.Total<egg.Owned)
                throw new InvalidOperationException("Incubator did not display selected egg's exclusive odds and owned/pool totals.");
            checkedEgg=true;CaptureRequested="incubator-egg";
        }
        if(step==2&&!checkedLegend)
        {
            if(screen.IncubatorSelected!="bukamon"||screen.CollectionPreviewKind!="legend"||screen.CollectionPreviewDrawCalls<=0||!screen.Buttons.ContainsKey("lootDigivolve"))
                throw new InvalidOperationException("Incubator omitted the owned legend preview/progress and evolution action.");
            checkedLegend=true;CaptureRequested="incubator-legend";
        }
        if(step>=commands.Length)
        {
            if(!checkedEgg||!checkedLegend||screen.IncubatorSelected!="legend")throw new InvalidOperationException("Incubator navigation did not return from Digivolve to the selected egg.");
            Done=true;Console.WriteLine("Incubator UI passed: egg-specific odds, owned/pool totals, owned legend preview, inventory scrolling and Digivolve return.");return;
        }
        string key=commands[step];
        if(!screen.Buttons.TryGetValue(key,out var target))return;
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
