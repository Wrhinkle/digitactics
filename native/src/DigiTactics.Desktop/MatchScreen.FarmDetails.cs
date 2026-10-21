using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool evolutionReturnToFarm;
    internal FarmResidentDetails? DisplayedFarmDetails {get;private set;}
    internal int FarmEvolutionBranches {get;private set;}
    private FarmResidentDetails FarmDetails(string id)=>FarmResidentDetails.Read(farm!.Save!,profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),
        profile.Storage.GetValueOrDefault("dt.cosmetics.v1"),id,DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    private void OpenFarmEvolution(string id)
    {
        ClearEvolutionPreview();evolutionLegend=id;evolutionReturnToFarm=true;collectionMessage=null;
        showFarm=false;showCollection=true;
    }
    private void CloseEvolution()
    {
        if(EvolutionBusy)return;
        evolutionLegend=null;ClearEvolutionPreview();
        if(evolutionReturnToFarm){showCollection=false;showFarm=true;farmManagement=false;}
        if(evolutionReturnToHome)showCollection=false;
        evolutionReturnToHome=false;
        evolutionReturnToFarm=false;
    }
    private void FarmEvolutionDetails(string id,FarmResidentDetails details)
    {
        FarmEvolutionBranches=0;
        var evolution=details.Evolution;
        FarmBox("Farm digivolution");
        FarmBoxHeading("NEXT DIGIVOLUTION", evolution.CanDigivolve?"ready now":evolution.Next is {} next?$"at level {next}":null);
        var known=LegendEvolution.Discovery(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),id).Dex.ToHashSet();
        foreach(var branch in evolution.Odds)
        {
            var at=ImGui.GetCursorScreenPos(); float width=ImGui.GetContentRegionAvail().X;
            var draw=ImGui.GetWindowDrawList(); string name=known.Contains(branch.Form)?LegendEvolution.Form(branch.Form).Name:"???";
            ImGui.PushFont(default,15);
            float nameWidth=Math.Max(80,width-148),rowHeight=Math.Max(32,ImGui.CalcTextSize(name,false,nameWidth).Y+10);
            ImGui.Dummy(new(width,rowHeight)); portraits.DrawEvolution(branch.Form,at,30,known.Contains(branch.Form));
            draw.AddText(ImGui.GetFont(),15,at+new float2(38,6),ClientInk,name,nameWidth);
            var careColor=branch.Care switch { "bond"=>new ImColor8(255,139,184),"food"=>new ImColor8(255,211,106),"battle"=>new ImColor8(255,154,90),_=>new ImColor8(159,224,122) };
            draw.AddText(ImGui.GetFont(),15,new(at.X+width-104,at.Y+6),careColor,branch.Care);
            string chance=$"{branch.Pct}%";
            draw.AddText(ImGui.GetFont(),15,new(at.X+width-ImGui.CalcTextSize(chance).X,at.Y+6),ClientInk,chance);
            ImGui.PopFont(); FarmEvolutionBranches++;
        }
        if(evolution.Odds.Length==0)UiText.Wrapped("Final form. Revert in Digivolve to explore other branches."u8);
        if(Button("farmDigivolve",evolution.CanDigivolve?"Open Digivolve · ready now":"Open Digivolve",ImGui.GetContentRegionAvail().X,32))OpenFarmEvolution(id);
        ImGui.EndChild();
        FarmBox("Farm evolution care"); FarmBoxHeading("FARM CARE PUSHES BRANCHES");
        var bonus=details.Care.Bonus;
        foreach(var (key,name,description,color) in new[] {
            ("bond","Bond","Praise, play and petting",new ImColor8(255,139,184)),
            ("battle","Battle","Training stats (every 25 points)",new ImColor8(255,154,90)),
            ("food","Food","Meals when hungry; carrots count more",new ImColor8(255,211,106)),
            ("wild","Wild",$"{details.Mistakes} mistakes x3, {details.PoopNeglect} neglected piles x2",new ImColor8(159,224,122)) })
        {
            var at=ImGui.GetCursorScreenPos(); float width=ImGui.GetContentRegionAvail().X;
            ImGui.GetWindowDrawList().AddText(at,color,name);
            string value=$"+{bonus[key]:0}";
            ImGui.GetWindowDrawList().AddText(new(at.X+width-ImGui.CalcTextSize(value).X,at.Y),ClientMuted,value);
            ImGui.Dummy(new(width,18));
            ImGui.PushFont(default,14);ImGui.PushStyleColor(ImGuiCol.Text,ClientMuted);UiText.Wrapped(description);ImGui.PopStyleColor();ImGui.PopFont();
        }
        ImGui.EndChild();
    }
}
