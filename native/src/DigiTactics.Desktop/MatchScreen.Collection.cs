using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showCollection;
    private bool showEggs;
    private bool collectionStore;
    private bool collectionStoreEggs;
    private string? evolutionLegend;
    private (string Id, double RarityRoll, double ItemRoll, long At)? eggPurchase;
    private int collectionKind, collectionSlot;
    private string? collectionMessage;
    private string? collectionNoticeMessage;
    private double collectionNoticeStarted;
    private (string Kind, string Id, DateOnly Day)? purchase;
    private bool CollectionModalVisible=>showCollection&&(purchase is not null||eggPurchase is not null||hatching is not null||boughtCosmetic is not null||mysteryReward is not null);
    private static readonly string[] CollectionNames = ["Little Legends", "Emotes", "Boards", "Kill Screens", "Dances", "Soundboard", "Elim Sounds"];
    private bool ChangeCollection(Func<CosmeticCollection, bool> change, string success)
    {
        try
        {
            var next = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            if (!change(next)) { collectionMessage = "That change is unavailable. Check ownership and your Bits balance."; return false; }
            bool saved=profile.UpdateStorage(new Dictionary<string, string> { ["dt.cosmetics.v1"] = next.Json });
            collectionMessage = saved ? success : profile.Error; return saved;
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException)
        { collectionMessage = "Collection could not be updated: " + error.Message; return false; }
    }
    private static void CollectionText(string text)
    {
        ImGui.PushTextWrapPos(0); ImGui.Text(text); ImGui.PopTextWrapPos();
    }
    private void CollectionNotice()
    {
        if(collectionMessage!=collectionNoticeMessage){collectionNoticeMessage=collectionMessage;collectionNoticeStarted=boardSceneTime;}
        if(collectionMessage is null||boardSceneTime-collectionNoticeStarted>=2.2)return;
        float maxWidth=Math.Min(640,ImGui.GetWindowSize().X-48);ImGui.PushFont(NativeUiTheme.BodyFont,15);
        var lines=new List<string>();string line="";
        foreach(string word in collectionMessage.Split(' ',StringSplitOptions.RemoveEmptyEntries))
        {
            string next=line.Length==0?word:line+" "+word;
            if(line.Length>0&&ImGui.CalcTextSize(next).X>maxWidth){lines.Add(line);line=word;}else line=next;
        }
        if(line.Length>0)lines.Add(line);
        float width=lines.Count==0?0:lines.Max(value=>ImGui.CalcTextSize(value).X),height=lines.Count*19+14;
        var position=ImGui.GetWindowPos();var extent=ImGui.GetWindowSize();var at=position+new float2((extent.X-width-28)/2,extent.Y-height-14);var draw=ImGui.GetForegroundDrawList();
        draw.PushClipRect(position,position+extent,true);draw.AddRectFilled(at,at+new float2(width+28,height),new ImColor8(5,14,24,242));draw.AddRect(at,at+new float2(width+28,height),HomeLine);
        for(int i=0;i<lines.Count;i++)draw.AddText(at+new float2(14,i*19+7),HomeGold,lines[i]);
        draw.PopClipRect();ImGui.PopFont();
    }
    private void CollectionNavigationAnchors()
    {
        if(!showCollection||MatchCosmeticsVisible||evolutionLegend is not null||purchase is not null||eggPurchase is not null||hatching is not null||boughtCosmetic is not null||mysteryReward is not null)return;
        // Legacy test routes now point to the same persistent navigation that
        // players use; these are aliases of real, visible controls.
        if(Buttons.TryGetValue("homeOverview",out var home))Buttons["closeCollection"]=home;
        if(Buttons.TryGetValue("clientLoot",out var loot))Buttons["toggleEggs"]=loot;
    }
    private void Collection()
    {
        ImGui.PushFont(default,17);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(10,9));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(8,8));
        try { CollectionPage(); }
        finally { ImGui.PopStyleVar(2); ImGui.PopFont(); }
    }
    private void CollectionPage()
    {
        ImGui.SetNextWindowBgAlpha(1);
        if(evolutionLegend is not null)
        {
            float width=Math.Min(1180,socialWindowSize.X-20),height=Math.Min(740,socialWindowSize.Y-20);
            var position=new float2((socialWindowSize.X-width)/2,(socialWindowSize.Y-height)/2);
            ImGui.SetNextWindowFocus();
            HomeCard("Digivolve",position,new(width,height),true,true);
            var draw=ImGui.GetWindowDrawList();
            draw.PushClipRect(new(0,0),new(socialWindowSize.X,socialWindowSize.Y),false);
            draw.AddRectFilled(new(0,0),new(socialWindowSize.X,socialWindowSize.Y),new ImColor8(1,10,19,210));
            draw.AddRectFilled(position,position+new float2(width,height),new ImColor8(5,15,25));
            draw.AddRect(position,position+new float2(width,height),HomeLine);
            draw.PopClipRect();
            try{EvolutionControls(evolutionLegend);}
            finally{ImGui.End();}
            return;
        }
        else if(MatchCosmeticsVisible)
        {
            float width=Math.Min(1480,size.X-32),height=size.Y-32;
            var at=new float2((size.X-width)/2,(size.Y-height)/2);
            if(matchCosmeticsFocus){ImGui.SetNextWindowFocus();matchCosmeticsFocus=false;}
            HomeCard("Match cosmetics",at,new(width,height),false,true);MatchCosmeticsBounds=(at,at+new float2(width,height));
            var draw=ImGui.GetWindowDrawList();draw.PushClipRect(new(0,0),new(socialWindowSize.X,socialWindowSize.Y),false);
            draw.AddRectFilled(new(0,0),new(socialWindowSize.X,socialWindowSize.Y),new ImColor8(1,10,19,205));draw.AddRectFilled(at,at+new float2(width,height),new ImColor8(5,15,25));draw.AddRect(at,at+new float2(width,height),HomeLine);draw.PopClipRect();
        }
        else
        {
            // The original client gives each page its own composition. The shell
            // already supplies navigation and the wallet; do not repeat them in
            // a second toolbar above every collection/loot surface.
            // .coll/.loot/#dtStore sit directly on the client backdrop (no page frame).
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
            // .coll/.loot padding: 12px 24px 16px; 16px sides at <=1500px wide, 8px/10px at <=820px tall.
            // #dtStore keeps 12px 24px 16px at every size.
            bool page=!(collectionStore&&!showEggs);
            float padX=page&&CosmeticStyle.Medium?16:24,padTop=page&&CosmeticStyle.Short?8:12,padBottom=page&&CosmeticStyle.Short?10:16;
            Window("Collection page",padX,ClientPageTop+padTop,size.X-padX*2,size.Y-ClientPageTop-padTop-padBottom,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
            ImGui.PopStyleVar(2);
            CollectionPageBackdrop();
        }
        bool nested=CollectionModalVisible;
        if(nested)
        {
            // Purchases, hatches and discoveries are modal moments in the old
            // client. Keep the page behind them instead of replacing it with a
            // blank full-window form.
            var backgroundCollection=new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            ImGui.BeginDisabled();
            try{if(MatchCosmeticsVisible)MatchCollectionHeader(backgroundCollection);if(showEggs)IncubatorControls(backgroundCollection);else CollectionBrowser(backgroundCollection);}
            finally{ImGui.EndDisabled();ImGui.End();}
            // The modal window is drawn above ordinary windows; use a separate
            // background window so its scrim does not cover the modal itself.
            ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(0,0,0,150));
            Window("Collection modal shade",0,0,size.X,size.Y,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoInputs|ImGuiWindowFlags.NoBackground);
            ImGui.GetWindowDrawList().AddRectFilled(new(0,0),new(size.X,size.Y),new ImColor8(0,0,0,150));ImGui.End();ImGui.PopStyleColor();
            // Source .dts-mbox is a compact, centered content dialog. Giving
            // every purchase a fixed 700px square left most of it empty.
            string? previewKind=mysteryReward?.Kind??(hatchRevealed?hatching?.Award.Kind:null)??boughtCosmetic?.Kind??purchase?.Kind;
            float previewControls=previewKind is "sound" or "elimsound"?70:previewKind=="killscreen"?46:0;
            float wantedHeight=(mysteryReward is not null?520:hatching is not null?hatchRevealed?420:330:boughtCosmetic is not null?390:eggPurchase is not null?410:440)+previewControls;
            float modalWidth=Math.Min(mysteryReward is not null?620:560,size.X-48),modalHeight=Math.Min(wantedHeight,size.Y-ClientPageTop-36);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(24,20));
            HomeCard("Collection reward",new((size.X-modalWidth)/2,ClientPageTop+(size.Y-ClientPageTop-modalHeight)/2),new(modalWidth,modalHeight),true,true);
            ImGui.PopStyleVar();
        }
        bool closeCollection=false;
        if(nested)
        {
            var body=ImGui.GetCursorScreenPos();
            ImGui.SetCursorScreenPos(ImGui.GetWindowPos()+new float2(ImGui.GetWindowSize().X-38,8));
            closeCollection=Button("closeCollection","X",28,28);
            if(ImGui.IsItemHovered())ImGui.SetTooltip("Close"u8);
            ImGui.SetCursorScreenPos(body+new float2(0,12));
        }
        else if(MatchCosmeticsVisible)closeCollection=MatchCollectionHeader(new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1")));
        if(closeCollection&&!EvolutionBusy)
        {
            bool returnToCollection=nested;
            CloseEvolution();
            purchase=null;eggPurchase=null;hatching=null;boughtCosmetic=null;mysteryReward=null;
            if(!returnToCollection){showCollection=false;DisposeCollectionPreview();}
            if(nested)ImGui.Dummy(new(0,0));
            ImGui.End();return;
        }
        try
        {
            var collection = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            if(nested){if(collectionMessage is not null)CollectionText(collectionMessage);}else CollectionNotice();
            if (evolutionLegend is not null) { EvolutionControls(evolutionLegend); ImGui.End(); return; }
            if(boughtCosmetic is not null){BoughtPanel(collection);ImGui.End();return;}
            if(mysteryReward is not null){MysteryRevealPanel(collection);ImGui.End();return;}
            if (hatching is not null) { HatchPanel(collection); ImGui.End(); return; }
            if (eggPurchase is { } eggPending)
            {
                var egg = DigiEggs.Definitions.Single(x => x.Id == eggPending.Id);
                CollectionModalTitle("Hatch a "+egg.Name+"?");EggArt(egg.Id,180);
                CollectionModalText($"{egg.Price:N0} of your {collection.Bits:N0} Bits",HomeInk);
                CollectionModalText("Duplicate Little Legends grant Digi-Data. Other duplicates refund 40% of their item price.",HomeMute);
                CollectionModalButtons(248);
                if (CollectionActionButton("confirmEgg", "Hatch",120,38,true))
                {
                    var result = DigiEggs.Purchase(profile.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile.Storage.GetValueOrDefault("dt.legendevo.v1"),
                        eggPending.Id, eggPending.RarityRoll, eggPending.ItemRoll, eggPending.At);
                    if (result is null) collectionMessage = "Not enough Bits to hatch this egg.";
                    else
                    {
                        var changes = new Dictionary<string, string> { ["dt.cosmetics.v1"] = result.Wallet };
                        if (result.Legends is not null && result.Legends != profile.Storage.GetValueOrDefault("dt.legendevo.v1")) changes["dt.legendevo.v1"] = result.Legends;
                        if (!profile.UpdateStorage(changes)) collectionMessage = profile.Error;
                        else
                        {
                            eggPurchase = null; hatching = (eggPending.Id, result.Award, boardSceneTime); hatchCracked = hatchRevealed = false; collectionMessage = null; audio.Play("reroll");
                        }
                    }
                }
                ImGui.SameLine(); if (CollectionActionButton("cancelEgg", "Cancel",120,38)) eggPurchase = null;
                ImGui.End(); return;
            }
            if (purchase is { } pending)
            {
                var item = CosmeticCollection.Item(pending.Kind, pending.Id)!;
                CollectionModalTitle("Buy "+item.Name+"?");
                CollectionModalPreview(pending.Kind,item,collection,320);
                CollectionModalText(CollectionKindName(pending.Kind)+" / "+(item.Rarity??"common"),RarityColor(item.Rarity));
                CollectionModalText($"{CosmeticCollection.Price(pending.Kind,item,pending.Day):N0} of your {collection.Bits:N0} Bits",HomeInk);
                CollectionModalButtons(248);
                if (CollectionActionButton("confirmPurchase", "Buy",120,38,true)) { if(ChangeCollection(c => c.Buy(pending.Kind, pending.Id, pending.Day), "Bought " + item.Name + ".")){boughtCosmetic=(pending.Kind,pending.Id);previewCosmetic=pending.Id;audio.Play("buy");} purchase=null; }
                ImGui.SameLine(); if (CollectionActionButton("cancelPurchase", "Cancel",120,38)) purchase = null;
                ImGui.End(); return;
            }
            if (showEggs)
            {
                IncubatorControls(collection);
                ImGui.End(); return;
            }
            CollectionBrowser(collection);
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or IOException or KeyNotFoundException)
        { CollectionText("Collection unavailable: " + error.Message); }
        ImGui.End();
    }
}
