using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal float EvolutionStageWidth {get;private set;}
    internal float EvolutionStageHeight {get;private set;}
    internal (float2 Min,float2 Max) EvolutionCareBounds {get;private set;}
    internal float EvolutionCareScroll {get;private set;}
    internal (float2 Min,float2 Max) EvolutionOddsBounds {get;private set;}
    internal Dictionary<string,(float2 Min,float2 Max)> EvolutionOddsRows {get;}=[];
    private void EvolutionControls(string id)
    {
        EvolutionTreeNodes=0;EvolutionTreePortraits=0;
        var headerAt=ImGui.GetCursorScreenPos();float headerWidth=ImGui.GetContentRegionAvail().X;
        ImGui.GetWindowDrawList().AddText(NativeUiTheme.DisplayBoldFont,24,headerAt+new float2(0,18),ClientInk,"DIGIVOLVE"u8);
        ImGui.SetCursorScreenPos(headerAt+new float2(148,0));
        bool changed=EvolutionLegendSelector(id,Math.Max(160,headerWidth-432),66);
        ImGui.SetCursorScreenPos(headerAt+new float2(headerWidth-272,20));
        ImGui.PushStyleColor(ImGuiCol.Text,ClientGold);ImGui.Text($"{profile!.Progress.Bits:N0} Bits");ImGui.PopStyleColor();
        ImGui.SetCursorScreenPos(headerAt+new float2(headerWidth-166,14));
        ImGui.BeginDisabled(EvolutionBusy);
        bool close=HomeButton("closeEvolution",evolutionReturnToFarm?"Back to farm":evolutionReturnToHome?"Back to Home":"Back to collection",166,36);ImGui.EndDisabled();
        ImGui.SetCursorScreenPos(headerAt+new float2(0,72));ImGui.Separator();
        if(close){CloseEvolution();return;}
        if(changed)return;
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
        EvolutionCareBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
        EvolutionCareScroll=ImGui.GetScrollY();
        try
        {
            DrawEvolutionStage(view,leftWidth,stageHeight);
            ImGui.PushFont(default,17);
            try
            {
                EvolutionProgressCard(id,view);
                EvolutionCareCard(id,care);
                ImGui.BeginDisabled(evolutionPreview.Evolving);
                ImGui.BeginDisabled(!view.CanDigivolve);
                if(view.CanDigivolve){ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(255,190,80));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(255,217,137));ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(234,162,57));ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(29,20,6));}
                if(Button("digivolve","Digivolve",162,38))
                {
                    Evolve(id,LegendAction.Digivolve);
                    if(EvolutionBusy)ImGui.SetScrollY(0);
                }
                if(view.CanDigivolve)ImGui.PopStyleColor(4);
                ImGui.EndDisabled();ImGui.SameLine();
                ImGui.BeginDisabled(!view.CanRevert || profile.Progress.Bits<50);
                if(Button("revertLegend","Revert / 50 Bits",162,38))Evolve(id,LegendAction.Revert);
                ImGui.EndDisabled();
                ImGui.PushFont(default,15);
                CollectionText("Duplicates add Digi-Data. Revert keeps level, data and care, and rolls your next branch again.");
                EvolutionEquip(id);
                ImGui.PopFont();
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
        float nameHeight=ImGui.CalcTextSize(form.Name,false,width-20).Y;
        draw.AddText(ImGui.GetFont(),20,new(at.X+10,at.Y+10),new ImColor8(0,0,0,200),form.Name,width-20);
        draw.AddText(ImGui.GetFont(),20,new(at.X+10,at.Y+8),new ImColor8(255,255,255),form.Name,width-20);
        ImGui.PopFont();
        ImGui.PushFont(default,16);string level=form.Level.ToUpperInvariant();
        draw.AddText(ImGui.GetFont(),16,new(at.X+10,at.Y+12+nameHeight),new ImColor8(164,180,202),level);ImGui.PopFont();
        DrawEvolutionAnnouncement(at,(int)width,height);
        draw.AddRect(at,new(at.X+width,at.Y+height),new ImColor8(48,65,89),12);
    }

    private void EvolutionProgressCard(string id,EvolutionView view)
    {
        FarmBox("evolutionProgress");
        try
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(4,3));
            ImGui.AlignTextToFramePadding();ImGui.Text($"Lv {view.Level} / 10");ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram,new ImColor8(47,155,255));
            ImGui.ProgressBar(view.Need>0?(float)view.Data/view.Need:1,new(76,9),""u8);ImGui.PopStyleColor();ImGui.SameLine();
            ImGui.BeginDisabled(evolutionPreview!.Evolving || view.Need==0 || profile!.Progress.Bits<view.LevelBits);
            if(Button("levelLegend",view.Need==0?"Max level":$"Level up / {view.LevelBits}",124,28))Evolve(id,LegendAction.BuyLevel);
            ImGui.EndDisabled();
            ImGui.PopStyleVar();ImGui.PushFont(default,15);
            ImGui.Text(view.Need>0?$"{view.Data}/{view.Need} Digi-Data":"Maximum level");
            ImGui.SameLine();ImGui.Text(view.CanDigivolve?"Ready to digivolve!":view.Next is {} next?$"Next form: level {next}":"Final form");
            ImGui.PopFont();
        }
        finally {ImGui.EndChild();}
    }

    private void EvolutionCareCard(string id,EvolutionCare care)
    {
        bool bonus=care.Bonus.Values.Any(value=>value>0);
        FarmBox("evolutionCareStats");
        try
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(4,3));
            foreach(string key in new[]{"bond","battle","food"})
            {
                ImGui.AlignTextToFramePadding();ImGui.Text(key=="bond"?"Bond":key=="battle"?"Battle":"Food");ImGui.SameLine(58);
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram,new ImColor8(255,154,60));ImGui.ProgressBar((float)Math.Min(care.Values[key],20)/20,new(80,12),""u8);ImGui.PopStyleColor();ImGui.SameLine();
                ImGui.Text($"{Math.Min(care.Values[key],99):0}/20");
                if(key=="food")
                {
                    ImGui.SameLine(190);ImGui.BeginDisabled(evolutionPreview!.Evolving || profile!.Progress.Bits<30 || care.Values[key]>=20);
                    if(Button("feedLegend",care.Values[key]>=20?"Full":"Feed / 30 Bits",110,28))Evolve(id,LegendAction.Feed);
                    ImGui.EndDisabled();
                }
            }
            ImGui.PopStyleVar();
            ImGui.PushFont(default,16);CollectionText("Care raises matching branch odds. Bond: games played. Battle: top 4. Food: feeding.");
            if(bonus)CollectionText("Digi-Farm: "+string.Join(", ",care.Bonus.Where(p=>p.Value>0).Select(p=>$"+{p.Value:0.#} {p.Key}"))+".");
            ImGui.PopFont();
        }
        finally {ImGui.EndChild();}
    }

    private void EvolutionOddsCard(string id,EvolutionView view)
    {
        // Long branch names keep their own line, while a bounded list leaves
        // enough space for the evolution tree at the minimum window size.
        float wantedHeight=view.Odds.Length==0?110:82+view.Odds.Length*50;
        float height=Math.Min(wantedHeight,Math.Max(160,ImGui.GetContentRegionAvail().Y*.48f));
        ImGui.BeginChild("evolutionOdds"u8,new(0,height),ImGuiChildFlags.Borders);
        EvolutionOddsBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
        EvolutionOddsRows.Clear();
        try
        {
            FarmBoxHeading("NEXT DIGIVOLUTION");
            ImGui.PushFont(default,17);
            CollectionText(view.Odds.Length==0?$"{LegendEvolution.Form(view.Form).Name} is a final form. Revert to explore another branch.":$"{LegendEvolution.Form(view.Odds[0].Form).Level} at level {view.Next}"+(view.CanDigivolve?" / Ready now":$" / You are level {view.Level}"));
            var discovered=DisplayedEvolutionDiscovery(id).Dex.ToHashSet();
            foreach(var branch in view.Odds)
            {
                var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
                string name=LegendEvolution.Form(branch.Form).Name;
                float nameWidth=Math.Max(100,width*.35f),rowHeight=Math.Max(44,ImGui.CalcTextSize(name,false,nameWidth).Y+10);
                ImGui.Dummy(new(width,rowHeight));
                EvolutionOddsRows[branch.Form]=(at,at+new float2(width,rowHeight));
                portraits.DrawEvolution(branch.Form,at,36,discovered.Contains(branch.Form));
                var draw=ImGui.GetWindowDrawList();
                draw.AddText(ImGui.GetFont(),17,new(at.X+44,at.Y+4),ClientInk,name,nameWidth);
                float careX=at.X+nameWidth+52,bar=careX+64,barWidth=Math.Max(12,at.X+width-52-bar);
                var careColor=branch.Care switch { "bond"=>new ImColor8(255,139,184),"food"=>new ImColor8(255,211,106),"battle"=>new ImColor8(255,154,90),_=>new ImColor8(159,224,122) };
                draw.AddText(ImGui.GetFont(),15,new(careX,at.Y+11),careColor,branch.Care);
                draw.AddRectFilled(new(bar,at.Y+14),new(bar+barWidth,at.Y+25),new ImColor8(8,15,25),4);
                draw.AddRectFilled(new(bar,at.Y+14),new(bar+barWidth*branch.Pct/100,at.Y+25),new ImColor8(100,198,236),4);
                draw.AddText(ImGui.GetFont(),17,new(at.X+width-44,at.Y+9),ClientInk,$"{branch.Pct}%");
            }
            ImGui.PopFont();
        }
        finally {ImGui.EndChild();}
    }
}
