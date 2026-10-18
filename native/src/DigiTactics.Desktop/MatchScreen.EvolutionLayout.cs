using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal float EvolutionStageWidth {get;private set;}
    internal float EvolutionStageHeight {get;private set;}
    private void EvolutionControls(string id)
    {
        EvolutionTreeNodes=0;EvolutionTreePortraits=0;
        if(Button("closeEvolution",evolutionReturnToFarm?"Back to Digi-Farm":"Back to collection")){CloseEvolution();return;}
        ImGui.SameLine();ImGui.Text($"Digivolve / {CosmeticCollection.Item("legend",id)!.Name}");
        ImGui.Separator();
        var view=LegendEvolution.Inspect(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),profile.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile.Storage.GetValueOrDefault("dt.farm.v1"),id);
        if(showEvolutionTree)
        {
            if(Button("closeEvolutionTree","Back to care"))showEvolutionTree=false;
            EvolutionTreePanel(id,view);return;
        }
        float leftWidth=330,stageHeight=size.Y<=800?196:230;
        evolutionPreview??=new(){OnFlash=()=>audio.Play("digivolve"),OnSettled=RevealEvolution};
        evolutionSaid.MotionEnabled=ClientMotion;evolutionPreview.MotionEnabled=ClientMotion;
        evolutionPreview.Select(LegendEvolution.VisualKey(id,view.Form));evolutionPreview.Draw(boardSceneTime,leftWidth,stageHeight);
        if(evolutionBeforeReveal is {} previous)view=previous;
        DisplayedEvolutionForm=view.Form;
        var care=LegendEvolution.Care(profile.Storage.GetValueOrDefault("dt.legendevo.v1"),profile.Storage.GetValueOrDefault("dt.farm.v1"),id,view.Form);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(6,6));
        ImGui.BeginChild("evolutionCareColumn"u8,new(leftWidth,0));
        try
        {
            DrawEvolutionStage(view,leftWidth,stageHeight);
            ImGui.PushFont(default,13);
            try
            {
                EvolutionProgressCard(id,view);
                EvolutionCareCard(id,care);
                ImGui.BeginDisabled(evolutionPreview.Evolving);
                ImGui.BeginDisabled(!view.CanDigivolve);
                if(view.CanDigivolve){ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(255,190,80));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(255,217,137));ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(234,162,57));ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(29,20,6));}
                if(Button("digivolve","Digivolve",162,32))Evolve(id,LegendAction.Digivolve);
                if(view.CanDigivolve)ImGui.PopStyleColor(4);
                ImGui.EndDisabled();ImGui.SameLine();
                ImGui.BeginDisabled(!view.CanRevert || profile.Progress.Bits<50);
                if(Button("revertLegend","Revert / 50 Bits",162,32))Evolve(id,LegendAction.Revert);
                ImGui.EndDisabled();
                CollectionText("Revert keeps level, Digi-Data and care, and rolls your next branch again.");
                if(Button("openEvolutionTree","Expand evolution tree",330))showEvolutionTree=true;
                ImGui.EndDisabled();
            }
            finally {ImGui.PopFont();}
        }
        finally {ImGui.EndChild();}
        ImGui.SameLine(0,14);
        ImGui.BeginChild("evolutionBranchesColumn"u8,new(0,0));
        try
        {
            EvolutionOddsCard(id,view);
            EvolutionTreePanel(id,view);
        }
        finally {ImGui.EndChild();ImGui.PopStyleVar();}
    }

    private void DrawEvolutionStage(EvolutionView view,float width,float height)
    {
        EvolutionStageWidth=width;EvolutionStageHeight=height;
        var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(width,height));
        var draw=ImGui.GetWindowDrawList();
        draw.AddImageRounded(new ImTextureRef{_TexID=new((nint)evolutionPreview!.Texture)},at,new(at.X+width,at.Y+height),new(0,1),new(1,0),new ImColor8(255,255,255),12);
        if(evolutionPreview.FlashOpacity>0)draw.AddRectFilled(at,new(at.X+width,at.Y+height),new ImColor8(255,255,255,(byte)Math.Round(evolutionPreview.FlashOpacity*255)),12);
        var form=LegendEvolution.Form(view.Form);
        ImGui.PushFont(evolutionAnnouncementFont,20);
        draw.AddText(ImGui.GetFont(),20,new(at.X+10,at.Y+10),new ImColor8(0,0,0,200),form.Name);
        draw.AddText(ImGui.GetFont(),20,new(at.X+10,at.Y+8),new ImColor8(255,255,255),form.Name);
        ImGui.PopFont();
        ImGui.PushFont(default,12);string level=form.Level.ToUpperInvariant();
        draw.AddText(ImGui.GetFont(),12,new(at.X+width-10-ImGui.CalcTextSize(level).X,at.Y+13),new ImColor8(164,180,202),level);ImGui.PopFont();
        DrawEvolutionAnnouncement(at,(int)width,height);
        draw.AddRect(at,new(at.X+width,at.Y+height),new ImColor8(48,65,89),12);
    }

    private void EvolutionProgressCard(string id,EvolutionView view)
    {
        ImGui.BeginChild("evolutionProgress"u8,new(0,80),ImGuiChildFlags.Borders);
        try
        {
            ImGui.Text($"Level {view.Level} / 10");ImGui.SameLine();
            ImGui.BeginDisabled(evolutionPreview!.Evolving || view.Need==0 || profile!.Progress.Bits<view.LevelBits);
            if(Button("levelLegend",view.Need==0?"Maximum level":$"Level up / {view.LevelBits} Bits"))Evolve(id,LegendAction.BuyLevel);
            ImGui.EndDisabled();
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram,new ImColor8(47,155,255));ImGui.ProgressBar(view.Need>0?(float)view.Data/view.Need:1,new(-1,9),""u8);ImGui.PopStyleColor();
            ImGui.Text(view.Need>0?$"{view.Data}/{view.Need} Digi-Data":"Maximum level");
            ImGui.SameLine();ImGui.Text(view.CanDigivolve?"Ready to digivolve!":view.Next is {} next?$"Next form at level {next}":"Final form");
        }
        finally {ImGui.EndChild();}
    }

    private void EvolutionCareCard(string id,EvolutionCare care)
    {
        bool bonus=care.Bonus.Values.Any(value=>value>0);
        ImGui.BeginChild("evolutionCareStats"u8,new(0,bonus?138:114),ImGuiChildFlags.Borders);
        try
        {
            foreach(string key in new[]{"bond","battle","food"})
            {
                ImGui.AlignTextToFramePadding();ImGui.Text(key=="bond"?"Bond":key=="battle"?"Battle":"Food");ImGui.SameLine(58);
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram,new ImColor8(255,154,60));ImGui.ProgressBar((float)Math.Min(care.Values[key],20)/20,new(80,12),""u8);ImGui.PopStyleColor();ImGui.SameLine();
                ImGui.Text($"{Math.Min(care.Values[key],99):0}/20");
                if(key=="food")
                {
                    ImGui.SameLine(190);ImGui.BeginDisabled(evolutionPreview!.Evolving || profile!.Progress.Bits<30 || care.Values[key]>=20);
                    if(Button("feedLegend",care.Values[key]>=20?"Full":"Feed / 30 Bits"))Evolve(id,LegendAction.Feed);
                    ImGui.EndDisabled();
                }
            }
            ImGui.PushFont(default,11);CollectionText("Care raises matching branch odds. Bond: games played. Battle: top 4. Food: feeding.");
            if(bonus)CollectionText("Digi-Farm: "+string.Join(", ",care.Bonus.Where(p=>p.Value>0).Select(p=>$"+{p.Value:0.#} {p.Key}"))+".");
            ImGui.PopFont();
        }
        finally {ImGui.EndChild();}
    }

    private void EvolutionOddsCard(string id,EvolutionView view)
    {
        ImGui.BeginChild("evolutionOdds"u8,new(0,view.Odds.Length==0?84:62+view.Odds.Length*38),ImGuiChildFlags.Borders);
        try
        {
            ImGui.Text("Next digivolution"u8);
            ImGui.PushFont(default,12);
            CollectionText(view.Odds.Length==0?$"{LegendEvolution.Form(view.Form).Name} is a final form. Revert to explore another branch.":$"{LegendEvolution.Form(view.Odds[0].Form).Level} at level {view.Next}"+(view.CanDigivolve?" / Ready now":$" / You are level {view.Level}"));
            var discovered=DisplayedEvolutionDiscovery(id).Dex.ToHashSet();
            foreach(var branch in view.Odds)
            {
                var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;ImGui.Dummy(new(width,32));
                portraits.DrawEvolution(branch.Form,at,32,discovered.Contains(branch.Form));
                var draw=ImGui.GetWindowDrawList();
                draw.AddText(ImGui.GetFont(),12,new(at.X+40,at.Y+2),new ImColor8(230,237,248),LegendEvolution.Form(branch.Form).Name);
                draw.AddText(ImGui.GetFont(),11,new(at.X+40,at.Y+18),new ImColor8(163,183,208),branch.Care+" care");
                float bar=at.X+Math.Min(185,width*.45f),barWidth=width-(bar-at.X)-48;
                draw.AddRectFilled(new(bar,at.Y+11),new(bar+barWidth,at.Y+20),new ImColor8(8,15,25),4);
                draw.AddRectFilled(new(bar,at.Y+11),new(bar+barWidth*branch.Pct/100,at.Y+20),new ImColor8(244,191,76),4);
                draw.AddText(ImGui.GetFont(),12,new(at.X+width-38,at.Y+8),new ImColor8(244,211,135),$"{branch.Pct}%");
            }
            ImGui.PopFont();
        }
        finally {ImGui.EndChild();}
    }
}
