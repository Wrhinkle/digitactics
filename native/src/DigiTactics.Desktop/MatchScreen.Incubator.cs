using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private IncubatorOverview? incubator;
    private (string? Wallet, string? Legends, string? Farm) incubatorSave;
    private (bool Legend, string Id)? incubatorSelection;
    private float incubatorInformationHeight;
    internal IncubatorOverview? DisplayedIncubator => incubator;
    internal string? IncubatorSelected => incubatorSelection?.Id;
    internal int IncubatorExclusiveCount { get; private set; }
    internal (float2 Min, float2 Max)? IncubatorInventoryBounds { get; private set; }
    internal (float2 Min, float2 Max)? IncubatorInformationBounds { get; private set; }
    internal (float2 Min, float2 Max)? IncubatorLastContentBounds { get; private set; }
    internal bool IncubatorContentsExpanded { get; private set; }
    internal int IncubatorContentRows { get; private set; }
    internal float IncubatorInformationScroll { get; private set; }
    internal float IncubatorInformationScrollMax { get; private set; }

    private void IncubatorControls(CosmeticCollection collection)
    {
        IncubatorContentsExpanded=false;IncubatorContentRows=0;IncubatorLastContentBounds=null;
        var saved = (profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile.Storage.GetValueOrDefault("dt.legendevo.v1"), profile.Storage.GetValueOrDefault("dt.farm.v1"));
        if (incubator is null || incubatorSave != saved)
        { incubator = IncubatorOverview.Read(saved.Item1, saved.Item2, saved.Item3); incubatorSave = saved; }
        if (incubatorSelection is not { } selected || (selected.Legend
            ? !incubator.Legends.Any(legend => legend.Item.Id == selected.Id)
            : !incubator.Eggs.Any(egg => egg.Definition.Id == selected.Id)))
        {
            var ready = incubator.Legends.FirstOrDefault(legend => legend.Evolution.CanDigivolve);
            incubatorSelection = ready is not null ? (true, ready.Item.Id) : (false, incubator.Eggs[0].Definition.Id);
        }
        float width = ImGui.GetContentRegionAvail().X;
        float inventoryWidth=Math.Clamp(width*.288f,318,380),stageWidth=width-inventoryWidth-12;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
        ImGui.BeginChild("Incubator detail"u8, new(stageWidth, 0), ImGuiChildFlags.Borders,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleVar();
        try
        {
            var at=ImGui.GetWindowPos();float height=ImGui.GetWindowSize().Y;var draw=ImGui.GetWindowDrawList();
            collectionArtwork.Draw("loot-bg",at,new(stageWidth,height),255,true);
            draw.AddRectFilledMultiColor(at,at+new float2(stageWidth,height),new ImColor8(1,10,19,130),new ImColor8(1,10,19,140),new ImColor8(1,10,19,185),new ImColor8(1,10,19,100));
            ImGui.SetCursorScreenPos(at+new float2(24,18));ImGui.BeginGroup();HomeLabel("LOOT",15,HomeGold);HomeLabel("INCUBATOR",28,HomeInk);ImGui.EndGroup();
            float statWidth=stageWidth>=720?148:112;string bits=collection.Bits.ToString("N0");
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,18);float bitsWidth=Math.Max(74,ImGui.CalcTextSize(bits).X+38);ImGui.PopFont();
            float bitsLeft=stageWidth-16-bitsWidth,readyLeft=bitsLeft-8-statWidth;
            IncubatorMaterial("loot-shard",incubator.DigiData.ToString("N0"),"Digi-Data",at+new float2(readyLeft-8-statWidth,16),statWidth);
            IncubatorMaterial("loot-chest",incubator.Ready.ToString(),"Ready to digivolve",at+new float2(readyLeft,16),statWidth);
            IncubatorMaterial(null,bits,"Bits",at+new float2(bitsLeft,16),bitsWidth);
            var bodyAt=at+new float2(16,100);float bodyHeight=height-184;
            ImGui.SetCursorScreenPos(bodyAt);
            ImGui.BeginChild("Incubator altar"u8,new(stageWidth-32,bodyHeight),ImGuiChildFlags.None,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
            float artWidth=(stageWidth-44)*.44f,infoWidth=stageWidth-44-artWidth;
            var selection = incubatorSelection.Value;
            try
            {
                // Reuse the previous frame's measured content height so the
                // centered card follows its text, action rows and disclosure.
                float infoHeight=Math.Min(bodyHeight,incubatorInformationHeight>0?incubatorInformationHeight:bodyHeight);
                ImGui.SetCursorPos(new(0,Math.Max(0,(bodyHeight-300)*.5f)));
                ImGui.BeginChild("Incubator altar artwork"u8,new(artWidth,Math.Min(300,bodyHeight)),ImGuiChildFlags.None,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
                if(selection.Legend)
                {
                    var legend=incubator.Legends.Single(legend=>legend.Item.Id==selection.Id);
                    CosmeticPreview("legend",legend.Item,collection,ImGui.GetContentRegionAvail().X,Math.Min(280,bodyHeight));
                    // Commit the preview pointer's restored cursor before ending
                    // this child; it can sit just past the clipped artwork edge.
                    ImGui.Dummy(new(0,0));
                }
                else
                {
                    float eggSize=Math.Min(280,Math.Min(artWidth,bodyHeight));var eggAt=ImGui.GetCursorScreenPos()+new float2((artWidth-eggSize)*.5f,(Math.Min(300,bodyHeight)-eggSize)*.5f);
                    collectionArtwork.Draw("loot-egg-"+selection.Id,eggAt,new(eggSize,eggSize));ImGui.Dummy(new(artWidth,Math.Min(300,bodyHeight)));
                }
                ImGui.EndChild();
                ImGui.SetCursorPos(new(artWidth+12,Math.Max(0,(bodyHeight-infoHeight)*.5f)));
                ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(1,10,19,225));ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(18,18));
                ImGui.BeginChild("Incubator altar information"u8,new(infoWidth,infoHeight),ImGuiChildFlags.Borders);
                ImGui.PopStyleVar();ImGui.PopStyleColor();
                IncubatorInformationBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
                try
                {
                    if(selection.Legend)IncubatorLegendControls(collection,incubator.Legends.Single(legend=>legend.Item.Id==selection.Id));
                    else IncubatorEggControls(collection,incubator.Eggs.Single(egg=>egg.Definition.Id==selection.Id));
                }
                finally
                {
                    incubatorInformationHeight=ImGui.GetCursorPosY()+18;
                    IncubatorInformationScroll=ImGui.GetScrollY();IncubatorInformationScrollMax=ImGui.GetScrollMaxY();
                    ImGui.EndChild();
                }
            }
            finally{ImGui.EndChild();}
            var footer=at+new float2(0,height-72);draw.AddRectFilled(footer,at+new float2(stageWidth,height),new ImColor8(10,22,35,235));
            ImGui.SetCursorScreenPos(footer+new float2(24,12));ImGui.PushTextWrapPos(stageWidth-24);ImGui.PushFont(NativeUiTheme.BodyFont,15);ImGui.PushStyleColor(ImGuiCol.Text,HomeMute);
            ImGui.Text("Digi-Data is XP for each Little Legend. Earn it through matches, duplicate hatches and Digi-Farm care to level up and unlock digivolution. Bits are your spending currency."u8);
            ImGui.PopStyleColor();ImGui.PopFont();ImGui.PopTextWrapPos();
        }
        finally { ImGui.EndChild(); }
        ImGui.SameLine();
        ImGui.BeginChild("Incubator inventory"u8, new(0, 0), ImGuiChildFlags.Borders);
        try
        {
            IncubatorInventoryBounds = (ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize());
            MysteryLootControls(collection);
            HomeLabel("DIGI-EGGS",17,HomeInk);
            int columns=4;float tileWidth=(ImGui.GetContentRegionAvail().X-18)/columns;int index=0;
            foreach(var egg in incubator.Eggs)
            {
                if(index++%columns!=0)ImGui.SameLine(0,6);
                var at=ImGui.GetCursorScreenPos();bool chosen=IncubatorTile("lootEgg/"+egg.Definition.Id,egg.Definition.Name.Replace("Digi-","").Replace(" Egg",""),egg.Definition.Price.ToString("N0"),at,new(tileWidth,98),incubatorSelection==(false,egg.Definition.Id),bits:true);
                collectionArtwork.Draw("loot-egg-"+egg.Definition.Id,at+new float2((tileWidth-48)*.5f,4),new(48,48));
                if(chosen)incubatorSelection=(false,egg.Definition.Id);
                if(ImGui.IsItemHovered())ImGui.SetTooltip(egg.Definition.Name+$" / {egg.Owned} of {egg.Total} owned");
            }
            ImGui.Spacing();HomeLabel("LITTLE LEGENDS",17,HomeInk);index=0;
            foreach (var legend in incubator.Legends)
            {
                var evolution = legend.Evolution;
                if(index++%columns!=0)ImGui.SameLine(0,6);
                var at=ImGui.GetCursorScreenPos();bool chosen=IncubatorTile("lootLegend/"+legend.Item.Id,LegendEvolution.Form(evolution.Form).Name,evolution.CanDigivolve?"READY":"Lv "+evolution.Level,at,new(tileWidth,98),incubatorSelection==(true,legend.Item.Id));
                portraits.DrawCompanion(CurrentPreviewLegend(collection,legend.Item.Id),at+new float2((tileWidth-48)*.5f,4),48);
                if(chosen)incubatorSelection=(true,legend.Item.Id);
            }
            if (incubator.Legends.Length == 0) CollectionText("Owned Little Legends appear here.");
        }
        finally { ImGui.EndChild(); }
    }
    private void IncubatorMaterial(string? art,string value,string label,float2 at,float width)
    {
        var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(at,at+new float2(width,58),new ImColor8(1,10,19,225));draw.AddRect(at,at+new float2(width,58),HomeLine);
        if(art is not null)collectionArtwork.Draw(art,at+new float2(8,12),new(27,27));
        else ProfileBitsDiamond(at+new float2(15,16));
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,18);draw.AddText(at+new float2(art is null?30:42,7),HomeInk,value);ImGui.PopFont();
        ImGui.PushFont(NativeUiTheme.BodyFont,13);draw.AddText(at+new float2(8,36),HomeGold,label);ImGui.PopFont();
    }
    private bool IncubatorTile(string key,string label,string sub,float2 at,float2 extent,bool selected,bool bits=false)
    {
        ImGui.InvisibleButton(key,extent);CollectionFilterAnchor(key);bool clicked=ImGui.IsItemClicked();ClientItemSound(key,clicked);
        var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(at,at+extent,new ImColor8(1,10,19));draw.AddRect(at,at+extent,selected?HomeInk:ImGui.IsItemHovered()?HomeGold:HomeLine,thickness:selected?2:1);
        draw.PushClipRect(at+new float2(3,50),at+extent-new float2(3,2),true);
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,13);var labelSize=ImGui.CalcTextSize(label);draw.AddText(at+new float2(Math.Max(3,(extent.X-labelSize.X)*.5f),56),HomeInk,label);ImGui.PopFont();
        ImGui.PushFont(NativeUiTheme.BodyFont,13);var subSize=ImGui.CalcTextSize(sub);float subLeft=(extent.X-subSize.X-(bits?17:0))*.5f;
        if(bits)ProfileBitsDiamond(at+new float2(subLeft+7,83));
        draw.AddText(at+new float2(subLeft+(bits?17:0),77),HomeGold,sub);ImGui.PopFont();draw.PopClipRect();
        return clicked;
    }

    private void BeginIncubatorHatch(string id)
    {
        incubatorSelection = (false, id);
        eggPurchase = (id, Random.Shared.NextDouble(), Random.Shared.NextDouble(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    private void IncubatorEggControls(CosmeticCollection collection, IncubatorEgg egg)
    {
        HomeLabel("DIGI-EGG",15,HomeGold);ImGui.PushTextWrapPos(0);HomeLabel(egg.Definition.Name.ToUpperInvariant(),28,HomeInk);ImGui.PopTextWrapPos();
        CollectionText(egg.Definition.Id=="mixed"?"Any cosmetic from any category.":"A random "+CollectionNames[Array.IndexOf(CosmeticCollection.Kinds,egg.Definition.Kinds[0])].ToLowerInvariant()+" cosmetic.");
        var oddsAt=ImGui.GetCursorScreenPos();float oddsWidth=ImGui.GetContentRegionAvail().X,x=0,y=0;var draw=ImGui.GetWindowDrawList();
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,14);
        foreach(var odd in egg.Odds)
        {
            string text=$"{odd.Rarity.ToUpperInvariant()} {odd.Pct:0.#}%";float badgeWidth=ImGui.CalcTextSize(text).X+28;
            if(x>0&&x+badgeWidth>oddsWidth){x=0;y+=28;}
            var badgeAt=oddsAt+new float2(x,y);draw.AddRectFilled(badgeAt,badgeAt+new float2(badgeWidth,22),new ImColor8(1,10,19));draw.AddRect(badgeAt,badgeAt+new float2(badgeWidth,22),HomeLine);
            var diamond=badgeAt+new float2(11,11);var color=RarityColor(odd.Rarity);
            draw.AddQuadFilled(diamond+new float2(0,-4),diamond+new float2(4,0),diamond+new float2(0,4),diamond+new float2(-4,0),color);
            draw.AddText(badgeAt+new float2(20,3),color,text);x+=badgeWidth+6;
        }
        ImGui.PopFont();ImGui.SetCursorScreenPos(oddsAt+new float2(0,y+32));
        var priceAt=ImGui.GetCursorScreenPos();ImGui.PushFont(NativeUiTheme.DisplayBoldFont,20);
        string price=egg.Definition.Price.ToString("N0");var priceSize=ImGui.CalcTextSize(price);
        ProfileBitsDiamond(priceAt+new float2(7,20));draw.AddText(priceAt+new float2(19,(40-priceSize.Y)*.5f),HomeInk,price);
        ImGui.Dummy(new(priceSize.X+19,40));ImGui.PopFont();ImGui.SameLine(0,10);
        ImGui.BeginDisabled(collection.Bits < egg.Definition.Price || egg.Total == 0);
        if (HomeButton("hatchSelectedEgg", "HATCH",150,40,true)) BeginIncubatorHatch(egg.Definition.Id);
        ImGui.EndDisabled();
        Buttons["egg/"+egg.Definition.Id]=Buttons["hatchSelectedEgg"];
        ImGui.PushFont(NativeUiTheme.BodyFont,15);CollectionText($"{egg.Owned} / {egg.Total} items owned");ImGui.PopFont();
        IncubatorExclusiveCount = egg.Exclusives.Length;
        IncubatorContentsExpanded=ImGui.CollapsingHeader("Contents and duplicate rewards"u8);CollectionFilterAnchor("lootContents");
        if(!IncubatorContentsExpanded)return;
        CollectionText("Rarity is rolled first, then each item of that rarity is equally likely. Duplicate legends grant Digi-Data until max level, then Bits. Other duplicates refund 40% of their item price.");
        foreach (var exclusive in egg.Exclusives)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, RarityColor(exclusive.Item.Rarity));
            CollectionText($"{exclusive.Item.Name} / {exclusive.Chance}" + (exclusive.Owned ? " / owned" : ""));
            IncubatorContentRows++;IncubatorLastContentBounds=(ImGui.GetItemRectMin(),ImGui.GetItemRectMax());
            ImGui.PopStyleColor();
        }
    }

    private void IncubatorLegendControls(CosmeticCollection collection, IncubatorLegend legend)
    {
        var evolution = legend.Evolution;
        var form = LegendEvolution.Form(evolution.Form);
        HomeLabel("LITTLE LEGEND / "+form.Level.ToUpperInvariant(),15,HomeGold);ImGui.PushTextWrapPos(0);HomeLabel(form.Name.ToUpperInvariant(),28,HomeInk);ImGui.PopTextWrapPos();
        var progressAt=ImGui.GetCursorScreenPos();float contentWidth=ImGui.GetContentRegionAvail().X;
        HomeLabel(evolution.Need == 0 ? "Max level" : $"{evolution.Data} / {evolution.Need} Digi-Data",15,HomeMute);
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,15);string level="Lv "+evolution.Level;var levelSize=ImGui.CalcTextSize(level);
        ImGui.GetWindowDrawList().AddText(progressAt+new float2(contentWidth-levelSize.X,0),HomeGold,level);ImGui.PopFont();
        ImGui.ProgressBar(evolution.Need == 0 ? 1 : (float)evolution.Data / evolution.Need, new(-1, 8), ""u8);
        CollectionText(evolution.CanDigivolve ? "Ready to digivolve. Open Digivolve to discover its next form."
            : evolution.Next is { } next ? $"Digivolves at level {next}. Gain Digi-Data from games played with it equipped, duplicate hatches and Digi-Farm care." : "Final form reached.");
        float actionsRight=ImGui.GetCursorScreenPos().X+ImGui.GetContentRegionAvail().X;
        if (HomeButton("lootDigivolve", "Digivolve",112,38,evolution.CanDigivolve)) { ClearEvolutionPreview(); evolutionLegend = legend.Item.Id; collectionMessage = null; }
        IncubatorNextAction(100,actionsRight);
        if (HomeButton("lootFarm", "Digi-Farm",100,38))OpenCollectionFarm(collection,legend.Item.Id);
        IncubatorNextAction(82,actionsRight);
        if (collection.Equipped("legend")?.GetValue<string>() == legend.Item.Id)
        {
            var badge=ImGui.GetCursorScreenPos()+new float2(0,7);var draw=ImGui.GetWindowDrawList();
            draw.AddRectFilled(badge,badge+new float2(82,24),new ImColor8(24,55,43));draw.AddRect(badge,badge+new float2(82,24),new ImColor8(88,170,121));
            draw.AddText(NativeUiTheme.BodyFont,15,badge+new float2(9,4),new ImColor8(135,225,171),"Equipped"u8);ImGui.Dummy(new(82,38));
        }
        else if (HomeButton("lootEquip", "Equip",82,38) && ChangeCollection(next => next.Equip("legend", legend.Item.Id), "Equipped " + legend.Item.Name + ".")) audio.PlayClient("equip", false);
    }
    private static void IncubatorNextAction(float width,float right)
    {
        if(ImGui.GetItemRectMax().X+8+width<=right)ImGui.SameLine(0,8);
    }
    private void OpenCollectionFarm(CosmeticCollection collection,string legend)
    {
        farm!.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        if (farm.Save is null || farm.Error is not null) { collectionMessage = farm.Error ?? "Digi-Farm could not be opened."; return; }
        var residents = System.Text.Json.Nodes.JsonNode.Parse(farm.Save)!["residents"]!.AsArray().Select(id => id!.GetValue<string>()).ToArray();
        string? equipped = collection.Equipped("legend")?.GetValue<string>();
        farmPet = residents.Contains(legend) ? legend : residents.Contains(equipped) ? equipped : residents.FirstOrDefault();
        if(ClientHomeVisible)RememberClientRoute("farm");
        farmManagement = false; showCollection = false; showFarm = true;
    }
}
