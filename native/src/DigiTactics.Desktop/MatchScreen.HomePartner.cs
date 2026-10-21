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
        HomeCard("Home Little Legend",at,extent,false);
        float stageHeight=Math.Max(118,extent.Y-206);var draw=ImGui.GetWindowDrawList();
        homeArtwork.Draw("lobby-bg",at,extent,255,true);
        draw.AddRectFilledMultiColor(at,at+new float2(extent.X,stageHeight),new ImColor8(1,10,19,60),new ImColor8(1,10,19,60),new ImColor8(1,10,19,155),new ImColor8(1,10,19,155));
        draw.AddRectFilled(at+new float2(0,stageHeight),at+extent,new ImColor8(1,10,19,245));
        try
        {
            string? wallet=profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),legends=profile?.Storage.GetValueOrDefault("dt.legendevo.v1");
            var collection=new CosmeticCollection(wallet);string id=collection.Equipped("legend")?.GetValue<string>()??"tokomon";
            var view=LegendEvolution.Inspect(legends,wallet,profile?.Storage.GetValueOrDefault("dt.farm.v1"),id);
            float imageWidth=extent.X,imageHeight=imageWidth*CosmeticScenePreview.Height/CosmeticScenePreview.Width;
            var imageAt=at+new float2(0,Math.Max(0,stageHeight-imageHeight));
            homeLegend??=new(legendPedestal);
            Buttons["homeLegend"]=imageAt+new float2(imageWidth/2,imageHeight/2);
            Buttons["homeLegend/outside"]=imageAt+new float2(-12,imageHeight/2);
            if(homeLegend.PointerItem("homeLegend",imageAt,new(imageWidth,imageHeight)) is not null)audio.PlayClient("emote",true);
            homeLegend.Draw("legend",id,LegendEvolution.VisualKey(id,view.Form),boardSceneTime,ClientMotion);
            draw.PushClipRect(at,at+new float2(extent.X,stageHeight),true);
            draw.AddImage(new ImTextureRef{_TexID=new((nint)homeLegend.Texture)},imageAt,imageAt+new float2(imageWidth,imageHeight),new(0,1),new(1,0));draw.PopClipRect();
            draw.AddLine(at+new float2(0,stageHeight),at+new float2(extent.X,stageHeight),HomeLine);
            ImGui.SetCursorScreenPos(at+new float2(12,stageHeight+8));HomeLabel("YOUR LITTLE LEGEND",13);
            ImGui.SetCursorScreenPos(at+new float2(12,stageHeight+27));HomeLabel(LobbyEllipsis(LegendEvolution.Form(view.Form).Name,extent.X-74),18,HomeInk);
            ImGui.SetCursorScreenPos(at+new float2(extent.X-54,stageHeight+28));HomeLabel("Lv "+view.Level,13,HomeGold);
            ImGui.SetCursorScreenPos(at+new float2(12,stageHeight+54));
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram,new ImColor8(49,185,202));ImGui.ProgressBar(view.Need>0?(float)view.Data/view.Need:1,new(extent.X-132,6),""u8);ImGui.PopStyleColor();
            ImGui.SetCursorScreenPos(at+new float2(extent.X-112,stageHeight+48));HomeLabel(view.Need>0?$"{view.Data}/{view.Need} Data":"Max level",13,view.CanDigivolve?HomeGold:HomeMute);
            ImGui.SetCursorScreenPos(at+new float2(8,stageHeight+73));ImGui.BeginDisabled(profile is null||EvolutionBusy);
            float actionsWidth=extent.X-16;
            if(ClientTextButton("homeLegendChange","Change",actionsWidth*.29f,30,false,14))
            {collectionOwnership=collectionRarity="all";OpenCollectionPage(kind:"legend",item:id);}
            ImGui.SameLine(0,0);if(ClientTextButton("homeLegendEvolution","Digivolve",actionsWidth*.43f,30,false,14))OpenHomeEvolution(id);
            ImGui.SameLine(0,0);if(ClientTextButton("homeLegendFarm","Farm",actionsWidth*.28f,30,false,14))OpenCollectionFarm(collection,id);
            ImGui.EndDisabled();
        }
        catch(Exception error) when(error is JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        {ImGui.SetCursorScreenPos(at+new float2(12,stageHeight+8));UiText.Wrapped("Your Little Legend could not be displayed: "+error.Message);}
        ImGui.SetCursorScreenPos(at+new float2(12,extent.Y-94));
        bool inRoom=networkRoom is not null||steam?.Room is not null;ImGui.BeginDisabled(inRoom);
        if(HomeButton("start",duos?"PLAY DUOS":"PLAY SOLO",extent.X-24,44,true))StartLocalMatch(duos);
        ImGui.EndDisabled();
        if(inRoom&&ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ResultTooltip("Leave your Steam room before starting a solo game.");
        ImGui.SetCursorScreenPos(at+new float2(12,extent.Y-44));
        if(HomeButton("multiplayer","MULTIPLAYER",extent.X-24,32))OpenMultiplayerView();
        ImGui.End();
    }
    private void OpenHomeEvolution(string id)
    {
        if(EvolutionBusy||profile is null)return;
        OpenCollectionPage(kind:"legend",item:id);evolutionLegend=id;evolutionReturnToHome=true;
    }
}
