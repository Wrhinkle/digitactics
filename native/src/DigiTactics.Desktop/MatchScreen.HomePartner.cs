using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal (float2 Min,float2 Max) HomePartnerBounds {get;private set;}
    // .legend-card: stage (flex 1) | .legend-info (8px 12px, 6px gaps, gold-5 rule) | .qp-btns (8px 12px 12px, 6px gap).
    private void HomePartner(float2 at,float2 extent)
    {
        HomePartnerBounds=(at,at+extent);
        ClientLayer("Home Little Legend",at,extent);var draw=ImGui.GetWindowDrawList();var hi=at+extent;
        float nameHeight=NativeUiTheme.TextLine(16),eyebrowHeight=NativeUiTheme.TextLine(11);
        float ghostHeight=MathF.Round(12.5f*1.2f+10+2),barHeight=NativeUiTheme.TextLine(13);
        float playHeight=MathF.Round(16*1.2f+22+2),secondaryHeight=MathF.Round(14*1.2f+14+2);
        float buttonsHeight=8+playHeight+6+secondaryHeight+12,infoHeight=1+8+eyebrowHeight+nameHeight+6+barHeight+6+ghostHeight+8;
        float stageHeight=MathF.Round(Math.Max(110,extent.Y-2-buttonsHeight-infoHeight));
        // Background: lobby-bg cover, linear-gradient(180deg, rgba(1,10,19,.4), #010a13 80%), radial cyan glow at 50% 40%.
        draw.PushClipRect(at,hi,true);
        homeArtwork.Draw("lobby-bg",at,extent,255,true);
        NativeUiPaint.VerticalGradient(draw,at,new(hi.X,at.Y+extent.Y*.8f),new ImColor8(1,10,19,102),NativeUiTheme.Navy0);
        draw.AddRectFilled(new(at.X,at.Y+extent.Y*.8f),hi,NativeUiPaint.Fade(NativeUiTheme.Navy0));
        NativeUiPaint.Radial(draw,at+new float2(extent.X*.5f,extent.Y*.4f),new float2(260,220)*.7f,new ImColor8(63,214,255,41),new ImColor8(63,214,255,0),48);
        draw.PopClipRect();
        var stageLo=at+new float2(1,1);var stageHi=new float2(hi.X-1,at.Y+1+stageHeight);
        var infoTop=stageHi.Y;
        try
        {
            string? wallet=profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),legends=profile?.Storage.GetValueOrDefault("dt.legendevo.v1");
            var collection=new CosmeticCollection(wallet);string id=collection.Equipped("legend")?.GetValue<string>()??"tokomon";
            var view=LegendEvolution.Inspect(legends,wallet,profile?.Storage.GetValueOrDefault("dt.farm.v1"),id);
            // The stage canvas fills .stage-host; render at its size with the original Stage camera for that aspect.
            var stageSize=stageHi-stageLo;
            int renderWidth=(int)MathF.Round(stageSize.X),renderHeight=(int)MathF.Round(stageSize.Y);
            if(homeLegend is not null&&(Math.Abs(homeLegend.RenderWidth-Math.Clamp(renderWidth,120,1600))>8||Math.Abs(homeLegend.RenderHeight-Math.Clamp(renderHeight,120,1600))>8)){homeLegend.Dispose();homeLegend=null;}
            homeLegend??=new(legendPedestal,renderWidth,renderHeight);
            Buttons["homeLegend"]=(stageLo+stageHi)*.5f;
            Buttons["homeLegend/outside"]=stageLo+new float2(-12,stageSize.Y/2);
            if(homeLegend.PointerItem("homeLegend",stageLo,stageSize) is not null)audio.PlayClient("emote",true);
            homeLegend.Draw("legend",id,LegendEvolution.VisualKey(id,view.Form),boardSceneTime,ClientMotion,stageAspect:stageSize.X/stageSize.Y);
            draw.AddImage(new ImTextureRef{_TexID=new((nint)homeLegend.Texture)},stageLo,stageHi,new(0,1),new(1,0));
            // .legend-info
            draw.AddRectFilled(new(at.X,infoTop),new(hi.X,infoTop+infoHeight),NativeUiPaint.Fade(new ImColor8(1,10,19,179)));
            draw.AddLine(new(at.X,infoTop+.5f),new(hi.X,infoTop+.5f),NativeUiPaint.Fade(NativeUiTheme.GoldDim));
            float left=at.X+13,inner=extent.X-26,y=infoTop+1+8;
            Eyebrow(draw,new(left,y),"Your Little Legend");
            string level="Lv "+view.Level;float levelWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,11,level,0)+14+2,levelHeight=eyebrowHeight+2+2;
            var levelLo=new float2(hi.X-13-levelWidth,MathF.Round(y+(eyebrowHeight+nameHeight-levelHeight)/2));
            draw.AddRectFilled(levelLo,levelLo+new float2(levelWidth,levelHeight),NativeUiPaint.Fade(NativeUiTheme.Navy0));
            draw.AddRect(levelLo,levelLo+new float2(levelWidth,levelHeight),NativeUiPaint.Fade(NativeUiTheme.GoldDeep));
            NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,11,levelLo+new float2(8,2),NativeUiTheme.GoldBright,level);
            NativeUiPaint.TrackedIn(draw,NativeUiTheme.DisplayBoldFont,16,new(left,y+eyebrowHeight),nameHeight,NativeUiTheme.GoldBright,SocialEllipsis(LegendEvolution.Form(view.Form).Name,NativeUiTheme.DisplayBoldFont,16,inner-levelWidth-8));
            y+=eyebrowHeight+nameHeight+6;
            // .li-bar: thin progress | 13px muted count (or the green "Ready to digivolve").
            string count=view.CanDigivolve?"Ready to digivolve":view.Need>0?$"{view.Data}/{view.Need} Digi-Data":"Max level";
            var countFont=view.CanDigivolve?NativeUiTheme.BodyBoldFont:NativeUiTheme.BodyFont;
            float countWidth=NativeUiPaint.TrackedWidth(countFont,13,count,0);
            NativeUiPaint.TrackedIn(draw,countFont,13,new(hi.X-13-countWidth,y),barHeight,view.CanDigivolve?NativeUiTheme.Success:NativeUiTheme.MutedNote,count);
            Progress(draw,new(left,MathF.Round(y+barHeight/2-3)),new(hi.X-13-countWidth-12,MathF.Round(y+barHeight/2+3)),view.Need>0?(float)view.Data/view.Need:1);
            y+=barHeight+6;
            // .li-btns: small ghost buttons, 4px apart.
            ImGui.BeginDisabled(profile is null||EvolutionBusy);
            float x=left;
            foreach(var action in new[]{("homeLegendChange","Change"),("homeLegendEvolution","Digivolve"),("homeLegendFarm","Farm")})
            {
                float buttonWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodySemiBoldFont,12.5f,action.Item2,0)+22+2;
                ImGui.SetCursorScreenPos(new(x,y));
                var kind=action.Item1=="homeLegendEvolution"&&view.CanDigivolve?ClientButtonKind.Primary:ClientButtonKind.Ghost;
                if(ClientButton(action.Item1,action.Item2,buttonWidth,ghostHeight,kind,12.5f))
                {
                    if(action.Item1=="homeLegendChange"){collectionOwnership=collectionRarity="all";OpenCollectionPage(kind:"legend",item:id);}
                    else if(action.Item1=="homeLegendEvolution")OpenHomeEvolution(id);
                    else OpenCollectionFarm(collection,id);
                }
                x+=buttonWidth+4;
            }
            ImGui.EndDisabled();
        }
        catch(Exception error) when(error is JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        {CssParagraph(draw,NativeUiTheme.BodyFont,12.5f,new(at.X+12,infoTop+8),extent.X-24,NativeUiTheme.Muted,"Your Little Legend could not be displayed: "+error.Message);}
        // .qp-btns on #010a13.
        float buttonsTop=hi.Y-1-buttonsHeight;
        draw.AddRectFilled(new(at.X,buttonsTop),hi,NativeUiPaint.Fade(NativeUiTheme.Navy0));
        ImGui.SetCursorScreenPos(new(at.X+13,buttonsTop+8));
        bool inRoom=networkRoom is not null||steam?.Room is not null;ImGui.BeginDisabled(inRoom);
        if(ClientButton("start",duos?"Play duos":"Play solo",extent.X-26,playHeight,ClientButtonKind.Play,16))StartLocalMatch(duos);
        ImGui.EndDisabled();
        if(inRoom&&ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ResultTooltip("Leave the multiplayer lobby first");
        ImGui.SetCursorScreenPos(new(at.X+13,buttonsTop+8+playHeight+6));
        if(ClientButton("multiplayer","Multiplayer",extent.X-26,secondaryHeight,ClientButtonKind.Secondary,14))OpenMultiplayerView();
        draw.AddRect(at,hi,NativeUiPaint.Fade(NativeUiTheme.GoldBorder));
        ImGui.End();
    }
    private void OpenHomeEvolution(string id)
    {
        if(EvolutionBusy||profile is null)return;
        OpenCollectionPage(kind:"legend",item:id);evolutionLegend=id;evolutionReturnToHome=true;
    }
}
