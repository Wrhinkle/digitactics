using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal (float2 Min,float2 Max) HomePartnerBounds {get;private set;}
    private void HomePartner(float2 at,float2 extent)
    {
        HomePartnerBounds=(at,at+extent);
        HomeCard("Home Little Legend",at,extent,false);HomeLabel("YOUR LITTLE LEGEND",15);
        try
        {
            string? wallet=profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),legends=profile?.Storage.GetValueOrDefault("dt.legendevo.v1");
            var collection=new CosmeticCollection(wallet);string id=collection.Equipped("legend")?.GetValue<string>()??"tokomon";
            var view=LegendEvolution.Inspect(legends,wallet,profile?.Storage.GetValueOrDefault("dt.farm.v1"),id);
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,21);ImGui.PushStyleColor(ImGuiCol.Text,HomeInk);CollectionText(LegendEvolution.Form(view.Form).Name);ImGui.PopStyleColor();ImGui.PopFont();
            ImGui.BeginDisabled(profile is null||EvolutionBusy);
            float actionsWidth=ImGui.GetContentRegionAvail().X;
            if(HomeButton("homeLegendChange","Change",(actionsWidth-10)*.29f,36))
            {collectionOwnership=collectionRarity="all";OpenCollectionPage(kind:"legend",item:id);}
            ImGui.SameLine(0,5);if(HomeButton("homeLegendEvolution","Digivolve",(actionsWidth-10)*.44f,36))OpenHomeEvolution(id);
            ImGui.SameLine(0,5);if(HomeButton("homeLegendFarm","Farm",(actionsWidth-10)*.27f,36))OpenCollectionFarm(collection,id);
            ImGui.EndDisabled();
            float progressY=at.Y+extent.Y-170;
            float imageTop=ImGui.GetCursorScreenPos().Y;
            float imageHeight=Math.Clamp(progressY-imageTop-8,32,190),imageWidth=imageHeight*CosmeticScenePreview.Width/CosmeticScenePreview.Height;
            var imageAt=new float2(at.X+(extent.X-imageWidth)/2,imageTop);
            homeLegend??=new(legendPedestal);
            Buttons["homeLegend"]=imageAt+new float2(imageWidth/2,imageHeight/2);
            Buttons["homeLegend/outside"]=imageAt+new float2(-12,imageHeight/2);
            if(homeLegend.PointerItem("homeLegend",imageAt,new(imageWidth,imageHeight)) is not null)audio.PlayClient("emote",true);
            homeLegend.Draw("legend",id,LegendEvolution.VisualKey(id,view.Form),boardSceneTime,ClientMotion);
            ImGui.GetWindowDrawList().AddImage(new ImTextureRef{_TexID=new((nint)homeLegend.Texture)},imageAt,imageAt+new float2(imageWidth,imageHeight),new(0,1),new(1,0));
            ImGui.SetCursorScreenPos(new(at.X+14,progressY));HomeLabel($"Level {view.Level}  /  "+(view.CanDigivolve?"Ready to digivolve":view.Need>0?$"{view.Data}/{view.Need} Digi-Data":"Maximum level"),15,view.CanDigivolve?HomeGold:HomeMute);
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram,new ImColor8(49,185,202));ImGui.ProgressBar(view.Need>0?(float)view.Data/view.Need:1,new(extent.X-28,5),""u8);ImGui.PopStyleColor();
        }
        catch(Exception error) when(error is JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        {UiText.Wrapped("Your Little Legend could not be displayed: "+error.Message);}
        ImGui.SetCursorScreenPos(at+new float2(14,extent.Y-136));
        ImGui.SetNextItemWidth(extent.X-28);DrawSetChoice("homeRuleset",selectedSet,SelectHomeHeroSet);
        ImGui.SetNextItemWidth((extent.X-34)*.56f);DrawPaceChoice("homePace",selectedPace,false,pace=>SelectPace(pace));
        ImGui.SameLine(0,6);ImGui.SetNextItemWidth((extent.X-34)*.44f);DrawFormatChoice("homeFormat",duos,SelectFormat);
        ImGui.SetCursorScreenPos(at+new float2(14,extent.Y-60));
        bool inRoom=networkRoom is not null||steam?.Room is not null;
        ImGui.BeginDisabled(inRoom);
        if(HomeButton("start",duos?"PLAY DUOS":"PLAY SOLO",extent.X-28,42,true))StartLocalMatch(duos);
        ImGui.EndDisabled();
        if(inRoom&&ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ResultTooltip("Leave your Steam room before starting a solo game.");
        ImGui.End();
    }
    private void OpenHomeEvolution(string id)
    {
        if(EvolutionBusy||profile is null)return;
        OpenCollectionPage(kind:"legend",item:id);evolutionLegend=id;evolutionReturnToHome=true;
    }
}
