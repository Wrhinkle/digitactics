using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using DigiTactics.Storage;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showFarm;
    private string? farmPet;
    private bool farmManagement;
    private FarmView? farmView;
    private string? farmViewError;
    private bool farmWasVisible;
    private readonly List<(string Text, double Remaining)> farmNotices = [];
    private readonly FarmPops farmPops = new();
    private GpuTexture? farmMoodTexture;
    private GpuTexture? farmSymbolTexture;
    private double farmMoodTime;
    private int FarmSceneWidth => Math.Max(250, (int)(size.X * .54f) - 24);
    private int FarmSceneHeight => Math.Max(200, size.Y - 150);
    public int FarmDrawCalls => farmView?.DrawCalls ?? 0;
    public string? FarmTheme => farmView?.Theme;
    internal string? FarmSelectedResident => farmPet;
    internal (float X, float Y)? FarmPilePoint(string id, int key)
    {
        if (farmView is null) return null;
        foreach (var pile in farmView.PileAnchors(FarmSceneWidth, FarmSceneHeight))
            if (pile.Id == id && pile.Key == key) return (12 + pile.X, 70 + pile.Y);
        return null;
    }
    private void FarmAct(FarmCareAction action, string? option = null, int pile = 0, string? resident = null)
    {
        string? before = farm!.Save;
        string? target = resident ?? farmPet;
        if (action == FarmCareAction.Play && option is null) option = farmView?.NearestPlaymate(target);
        farm.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), action, target, option, pile);
        if (farm.Error is null && farm.LastResult is { } receipt)
        {
            farmView?.Care(target, action, option, receipt, before);
            farmPops.Add(target, receipt);
            bool asleep = target is not null && before is not null && JsonNode.Parse(before)?["pets"]?[target]?["flags"]?["asleep"]?.GetValue<bool>() == true;
            if (target is not null && FarmFeedback.Symbol(action, option, receipt, asleep) is { } symbol) FarmSymbolPop(target, symbol);
            if (FarmFeedback.Sound(action, receipt) is { } sound) audio.Play(sound);
        }
    }
    private void UpdateFarmView(double delta)
    {
        farmMoodTime += delta;
        for (int i = farmNotices.Count - 1; i >= 0; i--)
        {
            var notice = farmNotices[i];
            if (notice.Remaining <= delta) farmNotices.RemoveAt(i);
            else farmNotices[i] = (notice.Text, notice.Remaining - delta);
        }
        if (!showFarm) { farmWasVisible = false; farmPops.Clear(); }
        if (!showFarm || farm?.Save is null || farm.Error is not null) return;
        try
        {
            farmView ??= new();
            farmView.Sync(farm.Save, profile!.Storage.GetValueOrDefault("dt.legendevo.v1"), profile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            FarmEvents(farm.DrainEvents(), !farmWasVisible); farmWasVisible = true;
            farmView.Update(delta); farmViewError = null;
            foreach (var feedback in farmView.DrainFeedback()) FarmSymbolPop(feedback.Id, feedback.Symbol);
            var anchors = farmView.Anchors(FarmSceneWidth, FarmSceneHeight, .3).ToDictionary(a => a.Id);
            farmPops.Update(delta, id => anchors.TryGetValue(id, out var a) ? (a.X, a.Y) : null);
        }
        catch (Exception error) when (error is InvalidDataException or IOException or System.Text.Json.JsonException or InvalidOperationException or KeyNotFoundException)
        { farmViewError = "Room view unavailable: " + error.Message; }
    }
    private void FarmEvents(JsonObject[] events, bool away)
    {
        void Notice(string text) { farmNotices.Add((text, 4.2)); if (farmNotices.Count > 5) farmNotices.RemoveAt(0); }
        if (away)
        {
            var parts = new List<string>();
            foreach (var (type, singular, plural) in new[] { ("poop", "poop", "poops"), ("toilet", "toilet trip", "toilet trips"), ("sick", "got sick", "got sick"), ("mistake", "care mistake", "care mistakes") })
            {
                int count = events.Count(e => e["type"]?.GetValue<string>() == type);
                if (count > 0) parts.Add($"{count} {(count == 1 ? singular : plural)}");
            }
            if (parts.Count > 0) Notice("While you were away: " + string.Join(", ", parts) + ".");
            return;
        }
        foreach (var e in events)
        {
            string id = e["id"]!.GetValue<string>();
            string name = CosmeticCollection.Item("legend", id)?.Name ?? id;
            string? why = e["why"]?.GetValue<string>();
            switch (e["type"]?.GetValue<string>())
            {
                case "poop": FarmSymbolPop(id, FarmSymbol.Poop); audio.Play("place"); break;
                case "toilet": farmView?.Toilet(id); break;
                case "sick": Notice($"{name} got sick{(why == "dirty" ? " from the dirty room" : "")}. Use a bandage or medicine."); break;
                case "tantrum": Notice($"{name} is throwing a tantrum! Scold it."); farmView?.React(id, "taunt"); FarmSymbolPop(id, FarmSymbol.Tantrum); break;
                case "mistake":
                    string reason = why switch { "hungry" => "was hungry too long", "sad" => "was sad too long", "lights" => "was sleepy with the lights on", "tantrum" => "had a tantrum nobody handled", "sick" => "was sick too long", _ => "" };
                    Notice($"Care mistake: {name} {reason}."); FarmSymbolPop(id, FarmSymbol.Mistake); break;
            }
        }
    }
    private void FarmPanel()
    {
        DisplayedFarmDetails=null;FarmEvolutionBranches=0;
        if (training is null)
        {
            Window("Digi-Farm room", 12, 70, FarmSceneWidth, FarmSceneHeight, ImGuiWindowFlags.NoBackground);
            if (farmViewError is not null) CollectionText(farmViewError);
            else if (farmView is not null)
                foreach (var (id, x, y) in farmView.Anchors(FarmSceneWidth, FarmSceneHeight))
                {
                    var pointer = ImGui.GetMousePos();
                    bool hovered = ImGui.IsWindowHovered() && farmView.Pick(pointer.X - 12, pointer.Y - 70, FarmSceneWidth, FarmSceneHeight) is { Pile: null } hit && hit.Id == id;
                    if (id == farmPet || hovered)
                    {
                        ImGui.SetCursorScreenPos(new(12 + x - 25, 70 + y));
                        if (Button("farmResident/" + id, CosmeticCollection.Item("legend", id)!.Name, active: id == farmPet)) { farmPet = id; FarmAct(FarmCareAction.Pet); }
                    }
                    if (farmView.Mood(id) is { } mood)
                    {
                        if (hovered) { ImGui.BeginTooltip(); ImGui.Text(mood.Text); ImGui.EndTooltip(); }
                        DrawFarmMood(mood, 12 + x, 70 + y - 16);
                    }
                }
            if (farmView is not null && ImGui.IsWindowHovered() && !ImGui.IsAnyItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                var mouse=ImGui.GetMousePos();
                if(farmView.Pick(mouse.X-12,mouse.Y-70,FarmSceneWidth,FarmSceneHeight) is { } hit)
                {
                    if(hit.Pile is { } key) FarmAct(FarmCareAction.CleanPile,pile:key,resident:hit.Id);
                    else { farmPet=hit.Id; FarmAct(FarmCareAction.Pet); }
                }
            }
            DrawFarmPops();
            ImGui.End();
        }
        Window("Digi-Farm", training is null ? size.X * .55f : size.X * .12f, size.Y * .06f, training is null ? size.X * .44f : size.X * .76f, size.Y * .88f);
        if (training is not null) { TrainingPanel(); ImGui.End(); return; }
        if (Button("closeFarm", "Back")) showFarm = false;
        if (farm!.Error is { } farmError) { CollectionText(farmError); if (Button("retryFarm", "Retry saving farm")) farm.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); ImGui.End(); return; }
        if (farm.Message is { Length: > 0 } message) CollectionText(message);
        foreach (var notice in farmNotices) CollectionText(notice.Text);
        try
        {
            if (farm.Save is null) { ImGui.End(); return; }
            var state = JsonNode.Parse(farm.Save)!;
            var residents = state["residents"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
            ImGui.SameLine(); ImGui.Text($"{profile!.Progress.Bits} Bits");
            ImGui.Text($"Residents: {residents.Length}/{state["cap"]}    Meat: {state["meat"]!["n"]}/3    Bandages: {state["bandage"]!["n"]}/2");
            ImGui.Text($"Daily rewards: {state["daily"]!["bits"]}/60 Bits, {state["daily"]!["data"]}/100 Digi-Data");
            if (Button("farmRoom", farmManagement ? "Care" : "Room, bank and themes")) farmManagement = !farmManagement;
            if (!residents.Contains(farmPet)) farmPet = residents.FirstOrDefault();
            if (farmManagement) { FarmRoomControls(state, residents); ImGui.End(); return; }
            if (Button("farmLights", state["lights"]!.GetValue<bool>() ? "Turn lights off" : "Turn lights on")) FarmAct(FarmCareAction.Lights);
            ImGui.SameLine(); if (Button("farmClean", "Clean room")) FarmAct(FarmCareAction.Clean);
            if (!residents.Contains(farmPet)) farmPet = residents.FirstOrDefault();
            if (farmPet is not null && ImGui.BeginCombo("Resident"u8, CosmeticCollection.Item("legend", farmPet)!.Name))
            {
                foreach (string id in residents) if (ImGui.Selectable(CosmeticCollection.Item("legend", id)!.Name, id == farmPet)) farmPet = id;
                ImGui.EndCombo();
            }
            if (farmPet is not null)
            {
                var pet = state["pets"]![farmPet]!;
                var details=FarmDetails(farmPet);DisplayedFarmDetails=details;
                var form=LegendEvolution.Form(details.Evolution.Form);
                ImGui.Text($"{form.Name} / {form.Level} / level {details.Evolution.Level}");
                if (farmView?.Mood(farmPet) is { } mood) ImGui.Text(mood.Text);
                ImGui.Text($"Hunger {pet["hunger"]!.GetValue<double>():0}    Energy {pet["energy"]!.GetValue<double>():0}    Happiness {pet["happy"]!.GetValue<double>():0}    Discipline {pet["disc"]!.GetValue<double>():0}");
                UiText.Wrapped($"Weight {pet["weight"]} G / ideal {details.IdealWeight} G"+(details.Fat?" / chubby":details.Thin?" / thin":"")+$" / age {details.AgeDays} days / {details.Mistakes} care mistakes");
                ImGui.Text($"{(pet["sick"]!.GetValue<bool>() ? "Sick: " + pet["sickWhy"] : "Healthy")} / {(pet["flags"]?["asleep"]?.GetValue<bool>() == true ? "Sleeping" : "Awake")}");
                ImGui.Text($"Strength {pet["str"]}    Speed {pet["spd"]}    Wisdom {pet["wis"]}    Stamina {pet["sta"]} (cap 300)");
                if(Button("farmDigivolve","Open Digivolve"+(details.Evolution.CanDigivolve?" / ready now":""))) {OpenFarmEvolution(farmPet);ImGui.End();return;}
                if(details.TrainingRefusal is {} refusal)UiText.Wrapped("Training: "+refusal);
                ImGui.BeginDisabled(details.TrainingRefusal is not null);
                foreach (var game in TrainingTitles)
                {
                    if (Button("farmTrain/" + game.Key, game.Value)) StartTraining(game.Key);
                    ImGui.SameLine();
                }
                ImGui.EndDisabled();
                ImGui.NewLine();
                int foodIndex=0;
                foreach (var food in new[] { ("meat", "Meat (tray, then 5 Bits)"), ("carrot", "Supercarrot (8 Bits)"), ("bigmeat", "Giant Meat (15 Bits)"), ("sirloin", "Sirloin (25 Bits)"), ("cake", "Digi-Cake (10 Bits)") })
                {
                    if(foodIndex++%2!=0)ImGui.SameLine();
                    if (Button("farmFeed/" + food.Item1, food.Item2)) FarmAct(FarmCareAction.Feed, food.Item1);
                }
                if (Button("farmBandage", "Use bandage")) FarmAct(FarmCareAction.Cure);
                ImGui.SameLine(); if (Button("farmMedicine", "Medicine (20 Bits)")) FarmAct(FarmCareAction.Cure, "medicine");
                if (Button("farmPraise", "Praise")) FarmAct(FarmCareAction.Praise);
                ImGui.SameLine(); if (Button("farmScold", "Scold")) FarmAct(FarmCareAction.Scold);
                ImGui.SameLine(); if (Button("farmPet", "Pet")) FarmAct(FarmCareAction.Pet);
                if (Button("farmPlay", "Play")) FarmAct(FarmCareAction.Play);
                foreach (string other in residents.Where(x => x != farmPet))
                    if (Button("farmPlay/" + other, "Play with " + CosmeticCollection.Item("legend", other)!.Name)) FarmAct(FarmCareAction.Play, other);
                foreach (var pile in pet["poop"]!.AsArray()) if (Button("farmPile/" + pile!["k"], "Clean pile " + pile["k"])) FarmAct(FarmCareAction.CleanPile, pile: pile["k"]!.GetValue<int>());
                FarmEvolutionDetails(farmPet,details);
            }
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or KeyNotFoundException)
        { CollectionText("Farm unavailable: " + error.Message); }
        ImGui.End();
    }
    private void DrawFarmPops()
    {
        var draw = ImGui.GetWindowDrawList();
        foreach (var pop in farmPops.Items)
        {
            if (pop.Age < 0 || pop.Position is not { } p) continue;
            if (pop.Symbol is { } symbol)
            {
                farmSymbolTexture ??= new(DigiTactics.Assets.PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content/farm-symbols.png"))), new(MinFilter: 9729, WrapS: 33071, WrapT: 33071), false);
                float x = 12 + p.X, y = 70 + p.Y + pop.Offset;
                draw.AddImage(new ImTextureRef { _TexID = new((nint)farmSymbolTexture.Id) }, new(x - 14, y - 28), new(x + 14, y), new((int)symbol / 18f, 0), new(((int)symbol + 1) / 18f, 1), new ImColor8(255, 255, 255, (byte)Math.Clamp((int)(pop.Opacity * 255), 0, 255)));
                continue;
            }
            var textSize = ImGui.CalcTextSize(pop.Text);
            var position = new Brutal.Numerics.float2(12 + p.X - textSize.X / 2, 70 + p.Y + pop.Offset - textSize.Y);
            byte alpha = (byte)Math.Clamp((int)(pop.Opacity * 255), 0, 255);
            draw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), position + new Brutal.Numerics.float2(1, 1), new ImColor8(0, 0, 0, alpha), pop.Text);
            draw.AddText(ImGui.GetFont(), ImGui.GetFontSize(), position, new ImColor8(255, 211, 106, alpha), pop.Text);
        }
    }
    private void FarmSymbolPop(string id, FarmSymbol symbol)
    {
        if (farmView is null) return;
        foreach (var anchor in farmView.Anchors(FarmSceneWidth, FarmSceneHeight, .3))
            if (anchor.Id == id) { farmPops.AddSymbol(id, symbol, (anchor.X, anchor.Y)); return; }
    }
    private void DrawFarmMood(FarmMood mood, float x, float y)
    {
        if (mood.Icon == 9) return;
        farmMoodTexture ??= new(DigiTactics.Assets.PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content/farm-moods.png"))), new(MinFilter: 9729, WrapS: 33071, WrapT: 33071), false);
        // Original 1.6-second bubble bob, with the same three-pixel excursion.
        y -= (float)(1.5 * (1 - Math.Cos(farmMoodTime * Math.PI * 2 / 1.6)));
        var draw = ImGui.GetWindowDrawList();
        draw.AddCircleFilled(new(x + 1, y + 2), 14, new ImColor8(0, 0, 0, 70));
        draw.AddCircleFilled(new(x, y), 13, new ImColor8(255, 255, 255, 235));
        draw.AddImage(new ImTextureRef { _TexID = new((nint)farmMoodTexture.Id) }, new(x - 13, y - 13), new(x + 13, y + 13), new(mood.Icon / 10f, 0), new((mood.Icon + 1) / 10f, 1));
    }
    private void FarmRoomControls(JsonNode state, string[] residents)
    {
        int cap = state["cap"]!.GetValue<int>(), price = FarmCare.ExpansionPrice(cap);
        ImGui.BeginDisabled(cap >= 12 || profile!.Progress.Bits < price);
        if (Button("farmExpand", cap >= 12 ? "Maximum room size" : $"Expand to {cap + 2} residents ({price} Bits)")) FarmAct(FarmCareAction.Expand);
        ImGui.EndDisabled();
        ImGui.Separator(); ImGui.Text("Residents"u8);
        foreach (string id in residents)
        {
            if (Button("farmSelect/" + id, CosmeticCollection.Item("legend", id)!.Name, active: farmPet == id)) farmPet = id;
            ImGui.SameLine(); ImGui.BeginDisabled(residents.Length <= 1);
            if (Button("farmOut/" + id, "Send to bank")) farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), FarmCareAction.MoveOut, id);
            ImGui.EndDisabled();
        }
        var collection = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
        ImGui.Separator(); ImGui.Text("Digi-Bank (care is paused)"u8);
        foreach (var item in CosmeticCollection.Items("legend").Where(x => collection.Owns("legend", x.Id) && !residents.Contains(x.Id)))
        {
            ImGui.Text(item.Name); ImGui.SameLine(); ImGui.BeginDisabled(residents.Length >= cap);
            if (Button("farmIn/" + item.Id, "Move in")) farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), FarmCareAction.MoveIn, item.Id);
            ImGui.EndDisabled();
            if (farmPet is not null)
            {
                ImGui.SameLine(); if (Button("farmSwap/" + item.Id, "Swap with " + CosmeticCollection.Item("legend", farmPet)!.Name))
                    farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), FarmCareAction.Swap, item.Id, farmPet);
            }
        }
        ImGui.Separator(); ImGui.Text("Room theme"u8);
        foreach (var theme in FarmCare.Themes)
        {
            bool owned = FarmCare.OwnsTheme(state, collection, theme), selected = state["theme"]!.GetValue<string>() == theme.Id;
            ImGui.BeginDisabled(selected || !owned && collection.Bits < theme.Price);
            if (Button("farmTheme/" + theme.Id, theme.Name + (selected ? " (selected)" : owned ? " - use" : $" - {theme.Price} Bits")))
                farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), owned ? FarmCareAction.SetTheme : FarmCareAction.BuyTheme, theme.Id);
            ImGui.EndDisabled();
            if (theme.Board is not null && !owned) CollectionText("Also unlocked by owning " + (CosmeticCollection.Item("board", theme.Board)?.Name ?? theme.Board) + ".");
        }
    }
}
