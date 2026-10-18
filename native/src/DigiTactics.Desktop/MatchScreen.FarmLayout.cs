using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private string? farmActionMenu;
    private string farmManagementPage = "bank";

    private void FarmWindow(string name, float x, float y, float width, float height, ImGuiWindowFlags flags = 0)
        => Window(name, x, y, width, height, flags | ImGuiWindowFlags.NoTitleBar);

    private static void FarmHeading(string text, float height = 24)
    {
        var at = ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddText(NativeUiTheme.DisplayBoldFont, height, at, NativeUiTheme.GoldBright, text);
        ImGui.Dummy(new(1, height + 4));
    }

    private void FarmPanel()
    {
        DisplayedFarmDetails = null; FarmEvolutionBranches = 0;
        if (training is not null)
        {
            float width = Math.Min(800, size.X - 48), height = Math.Min(590, size.Y - ClientPageTop - 32);
            FarmWindow("Digi-Farm training", (size.X - width) / 2, ClientPageTop + (size.Y - ClientPageTop - height) / 2, width, height);
            TrainingPanel(); ImGui.End(); return;
        }
        JsonNode? state;
        try { state = farm?.Save is { } saved ? JsonNode.Parse(saved) : null; }
        catch (System.Text.Json.JsonException) { state = null; }
        if (state is null)
        {
            FarmWindow("Digi-Farm unavailable", 16, ClientPageTop + 12, size.X - 32, 190);
            FarmHeading("Digi-Farm"); CollectionText(farm?.Error ?? "Your farm is loading.");
            if (Button("retryFarm", "Retry saving farm", 200, 38)) farm?.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            ImGui.SameLine(); if (Button("closeFarm", "Close", 90, 38)) showFarm = false;
            ImGui.End(); return;
        }
        var residents = state["residents"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
        if (!residents.Contains(farmPet)) farmPet = residents.FirstOrDefault();
        FarmHeader(state, residents);
        FarmRoomScene(state, residents);
        FarmWindow("Digi-Farm resident", FarmSceneX + FarmSceneWidth + 12, FarmSceneTop, FarmCardWidth, FarmSceneHeight);
        ImGui.BeginDisabled(farmManagement || farmActionMenu is not null);
        if (farm!.Error is { } error)
        {
            CollectionText(error);
            if (Button("retryFarm", "Retry saving farm", ImGui.GetContentRegionAvail().X, 38)) farm.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
        if (farmPet is not null) FarmResidentCard(state, residents, farmPet);
        ImGui.EndDisabled();
        ImGui.End();
        if (farmActionMenu is not null) FarmActionChoices(state);
        if (farmManagement) FarmRoomControls(state, residents);
    }

    private void FarmHeader(JsonNode state, string[] residents)
    {
        FarmWindow("Digi-Farm header", 16, ClientPageTop + 8, size.X - 32, 88, ImGuiWindowFlags.NoScrollbar);
        FarmHeading("Digi-Farm", 26);
        ImGui.SetCursorScreenPos(new(190, ClientPageTop + 25));
        ImGui.Text(FarmCare.Themes.FirstOrDefault(t => t.Id == state["theme"]!.GetValue<string>())?.Name ?? "Room");
        ImGui.SetCursorScreenPos(new(size.X - 410, ClientPageTop + 20));
        ImGui.BeginDisabled(farmManagement);
        if (Button("farmRoom", "Digi-Bank", 110, 36)) { farmManagement = true; farmManagementPage = "bank"; farmActionMenu = null; }
        ImGui.SameLine(0, 8);
        if (Button("farmThemes", "Themes", 100, 36)) { farmManagement = true; farmManagementPage = "themes"; farmActionMenu = null; }
        ImGui.SameLine(0, 8);
        if (Button("closeFarm", "Close", 90, 36)) showFarm = false;
        ImGui.EndDisabled();
        ImGui.SetCursorScreenPos(new(32, ClientPageTop + 64));
        ImGui.Text($"{residents.Length}/{state["cap"]} residents    Meat {state["meat"]!["n"]}/3    Bandages {state["bandage"]!["n"]}/2    Daily {state["daily"]!["bits"]}/60 Bits · {state["daily"]!["data"]}/100 Data");
        ImGui.End();
    }

    private void FarmRoomScene(JsonNode state, string[] residents)
    {
        FarmWindow("Digi-Farm room", FarmSceneX, FarmSceneTop, FarmSceneWidth, FarmSceneHeight, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
        ImGui.BeginDisabled(farmManagement || farmActionMenu is not null);
        int residentColumns = Math.Max(1, (int)((ImGui.GetContentRegionAvail().X + 8) / 56));
        for (int index = 0; index < residents.Length; index++)
        {
            string id = residents[index];
            if (index % residentColumns != 0) ImGui.SameLine(0, 8);
            var at = ImGui.GetCursorScreenPos();
            if (Button("farmResident/" + id, "##resident-" + id, 48, 48, id == farmPet)) { farmPet = id; FarmAct(FarmCareAction.Pet); }
            portraits.DrawEvolution(FarmDetails(id).Evolution.Form, at + new float2(4, 4), 40, true);
            if (ImGui.IsItemHovered()) { ImGui.BeginTooltip(); ImGui.Text(CosmeticCollection.Item("legend", id)!.Name); ImGui.EndTooltip(); }
        }
        float residentBottom = ImGui.GetCursorScreenPos().Y;
        if (farmViewError is not null) CollectionText(farmViewError);
        if (farmView is not null)
        {
            var pointer = ImGui.GetMousePos();
            var picked = ImGui.IsWindowHovered() ? farmView.Pick(pointer.X - FarmSceneX, pointer.Y - FarmSceneTop, FarmSceneWidth, FarmSceneHeight) : null;
            foreach (var (id, x, y) in farmView.Anchors(FarmSceneWidth, FarmSceneHeight))
            {
                if (farmView.Mood(id) is { } mood)
                {
                    if (picked is { Pile: null } hit && hit.Id == id && !ImGui.IsAnyItemHovered()) { ImGui.BeginTooltip(); ImGui.Text(mood.Text); ImGui.EndTooltip(); }
                    DrawFarmMood(mood, FarmSceneX + x, FarmSceneTop + y - 16);
                }
            }
            if (!farmManagement && farmActionMenu is null && picked is { } hit2 && !ImGui.IsAnyItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                if (hit2.Pile is { } key) FarmAct(FarmCareAction.CleanPile, pile: key, resident: hit2.Id);
                else { farmPet = hit2.Id; FarmAct(FarmCareAction.Pet); }
            }
        }
        DrawFarmPops();
        ImGui.EndDisabled();
        ImGui.SetCursorScreenPos(new(FarmSceneX + 12, FarmSceneTop + FarmSceneHeight - 68));
        ImGui.BeginDisabled(farmManagement);
        float buttonWidth = (FarmSceneWidth - 24 - 7 * 6) / 8f;
        void CareButton(string key, string text, Action action, bool active = false)
        {
            if (Button(key, text, buttonWidth, 48, active)) action();
            ImGui.SameLine(0, 6);
        }
        CareButton("farmAction/feed", "Feed", () => farmActionMenu = farmActionMenu == "feed" ? null : "feed", farmActionMenu == "feed");
        CareButton("farmClean", "Clean", () => FarmAct(FarmCareAction.Clean));
        CareButton("farmAction/cure", "Heal", () => farmActionMenu = farmActionMenu == "cure" ? null : "cure", farmActionMenu == "cure");
        CareButton("farmLights", "Lights", () => FarmAct(FarmCareAction.Lights), !state["lights"]!.GetValue<bool>());
        CareButton("farmPraise", "Praise", () => FarmAct(FarmCareAction.Praise));
        CareButton("farmScold", "Scold", () => FarmAct(FarmCareAction.Scold));
        CareButton("farmAction/train", "Train", () => farmActionMenu = farmActionMenu == "train" ? null : "train", farmActionMenu == "train");
        CareButton("farmPlay", "Play", () => FarmAct(FarmCareAction.Play));
        ImGui.NewLine(); ImGui.EndDisabled();
        string? notice = farmNotices.LastOrDefault().Text ?? farm!.Message;
        if (!string.IsNullOrWhiteSpace(notice))
        {
            ImGui.SetCursorScreenPos(new(FarmSceneX + 18, Math.Max(FarmSceneTop + 76, residentBottom + 12)));
            var draw = ImGui.GetWindowDrawList(); var at = ImGui.GetCursorScreenPos();
            draw.AddRectFilled(at - new float2(8, 6), new(FarmSceneX + FarmSceneWidth - 12, at.Y + 56), new ImColor8(5, 15, 29, 220), 4);
            ImGui.PushTextWrapPos(FarmSceneWidth - 26); ImGui.TextWrapped(notice); ImGui.PopTextWrapPos();
        }
        ImGui.End();
    }

    private void FarmResidentCard(JsonNode state, string[] residents, string id)
    {
        var pet = state["pets"]![id]!; var details = FarmDetails(id); DisplayedFarmDetails = details;
        var form = LegendEvolution.Form(details.Evolution.Form);
        var at = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(64, 64)); portraits.DrawEvolution(form.Id, at, 64, true);
        ImGui.SameLine(); ImGui.BeginGroup(); FarmHeading(CosmeticCollection.Item("legend", id)!.Name, 22);
        ImGui.Text($"{form.Name} · Lv {details.Evolution.Level}");
        if (farmView?.Mood(id) is { } mood) ImGui.Text(mood.Text); ImGui.EndGroup();
        ImGui.Spacing();
        FarmMeter("Hunger", pet["hunger"]!.GetValue<double>(), new(255, 178, 95));
        FarmMeter("Energy", pet["energy"]!.GetValue<double>(), new(108, 204, 255));
        FarmMeter("Happiness", pet["happy"]!.GetValue<double>(), new(255, 139, 184));
        FarmMeter("Discipline", pet["disc"]!.GetValue<double>(), new(173, 229, 119));
        UiText.Wrapped($"{pet["weight"]} G / ideal {details.IdealWeight} G · {details.AgeDays} days · {details.Mistakes} care mistakes");
        UiText.Wrapped((details.Sick ? "Sick: " + pet["sickWhy"] : "Healthy") + (details.Fat ? " · Chubby" : details.Thin ? " · Thin" : "") + (pet["flags"]?["asleep"]?.GetValue<bool>() == true ? " · Sleeping" : " · Awake"));
        if (Button("farmPet", "Pet " + CosmeticCollection.Item("legend", id)!.Name, ImGui.GetContentRegionAvail().X, 36)) FarmAct(FarmCareAction.Pet);
        ImGui.SeparatorText("Training stats"u8);
        if (ImGui.BeginTable("farm-stats"u8, 2, ImGuiTableFlags.SizingStretchSame))
        {
            foreach (var (key, name) in new[] { ("str", "Strength"), ("spd", "Speed"), ("wis", "Wisdom"), ("sta", "Stamina") })
            { ImGui.TableNextColumn(); ImGui.Text($"{name}  {pet[key]} / 300"); }
            ImGui.EndTable();
        }
        if (details.TrainingRefusal is { } refusal) UiText.Wrapped("Training: " + refusal);
        if (Button("farmDigivolve", details.Evolution.CanDigivolve ? "Open Digivolve · ready now" : "Open Digivolve", ImGui.GetContentRegionAvail().X, 40)) OpenFarmEvolution(id);
        FarmEvolutionDetails(id, details);
        if (ImGui.CollapsingHeader("Playmates and cleaning"u8))
        {
            foreach (string other in residents.Where(x => x != id))
                if (Button("farmPlay/" + other, "Play with " + CosmeticCollection.Item("legend", other)!.Name, 0, 34)) FarmAct(FarmCareAction.Play, other);
            foreach (var pile in pet["poop"]!.AsArray())
                if (Button("farmPile/" + pile!["k"], "Clean pile " + pile["k"], 0, 34)) FarmAct(FarmCareAction.CleanPile, pile: pile["k"]!.GetValue<int>());
        }
    }

    private static void FarmMeter(string label, double value, ImColor8 color)
    {
        ImGui.Text($"{label}  {value:0}/100");
        var at = ImGui.GetCursorScreenPos(); float width = ImGui.GetContentRegionAvail().X;
        var draw = ImGui.GetWindowDrawList();
        draw.AddRectFilled(at, at + new float2(width, 10), new ImColor8(35, 49, 67), 5);
        draw.AddRectFilled(at, at + new float2(width * (float)Math.Clamp(value / 100, 0, 1), 10), color, 5);
        ImGui.Dummy(new(width, 12));
    }

    private void FarmActionChoices(JsonNode state)
    {
        string menu = farmActionMenu!;
        float width = Math.Min(440, FarmSceneWidth - 32), height = menu == "feed" ? 348 : menu == "train" ? 300 : 200;
        ImGui.SetNextWindowFocus();
        FarmWindow("Digi-Farm care choices", FarmSceneX + (FarmSceneWidth - width) / 2, FarmSceneTop + FarmSceneHeight - 82 - height, width, height);
        FarmHeading(menu == "feed" ? "Choose a meal" : menu == "train" ? "Training games" : "Medicine", 22);
        if (Button("farmActionClose", "Close", 84, 32)) farmActionMenu = null;
        if (menu == "feed")
        {
            foreach (var (id, text) in new[] { ("meat", $"Meat · {state["meat"]!["n"]} in tray, then 5 Bits"), ("carrot", "Supercarrot · 8 Bits"), ("bigmeat", "Giant Meat · 15 Bits"), ("sirloin", "Sirloin · 25 Bits"), ("cake", "Digi-Cake · 10 Bits") })
                if (Button("farmFeed/" + id, text, ImGui.GetContentRegionAvail().X, 38)) { FarmAct(FarmCareAction.Feed, id); farmActionMenu = null; }
        }
        else if (menu == "train")
        {
            string? refusal = farmPet is null ? "Select a resident." : FarmDetails(farmPet).TrainingRefusal;
            if (refusal is not null) UiText.Wrapped(refusal);
            ImGui.BeginDisabled(refusal is not null);
            foreach (var game in TrainingTitles)
                if (Button("farmTrain/" + game.Key, game.Value, ImGui.GetContentRegionAvail().X, 38)) { StartTraining(game.Key); farmActionMenu = null; }
            ImGui.EndDisabled();
        }
        else
        {
            if (Button("farmBandage", $"Use bandage · {state["bandage"]!["n"]} available", ImGui.GetContentRegionAvail().X, 38)) { FarmAct(FarmCareAction.Cure); farmActionMenu = null; }
            if (Button("farmMedicine", "Medicine · 20 Bits", ImGui.GetContentRegionAvail().X, 38)) { FarmAct(FarmCareAction.Cure, "medicine"); farmActionMenu = null; }
        }
        ImGui.End();
    }

    private void FarmRoomControls(JsonNode state, string[] residents)
    {
        float width = Math.Min(780, size.X - 64), height = Math.Min(640, size.Y - ClientPageTop - 32);
        ImGui.SetNextWindowFocus();
        FarmWindow("Digi-Farm management", (size.X - width) / 2, ClientPageTop + 16, width, height);
        FarmHeading(farmManagementPage == "bank" ? "Digi-Bank" : "Room themes", 26);
        if (Button("farmManageBank", "Digi-Bank", 130, 38, farmManagementPage == "bank")) farmManagementPage = "bank";
        ImGui.SameLine(); if (Button("farmManageThemes", "Themes", 110, 38, farmManagementPage == "themes")) farmManagementPage = "themes";
        ImGui.SameLine(); if (Button("farmManagementClose", "Done", 90, 38)) farmManagement = false;
        var collection = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
        if (farmManagementPage == "bank")
        {
            int cap = state["cap"]!.GetValue<int>(), price = FarmCare.ExpansionPrice(cap);
            UiText.Wrapped($"{residents.Length}/{cap} residents. Care pauses for residents stored in the bank.");
            ImGui.BeginDisabled(cap >= 12 || profile.Progress.Bits < price);
            if (Button("farmExpand", cap >= 12 ? "Maximum room size" : $"Expand to {cap + 2} residents · {price} Bits", ImGui.GetContentRegionAvail().X, 40)) FarmAct(FarmCareAction.Expand);
            ImGui.EndDisabled();
            ImGui.SeparatorText("In your farm"u8);
            foreach (string id in residents)
            {
                var at = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(54, 54)); portraits.DrawEvolution(FarmDetails(id).Evolution.Form, at, 54, true);
                ImGui.SameLine();
                if (Button("farmSelect/" + id, CosmeticCollection.Item("legend", id)!.Name, 160, 42, farmPet == id)) farmPet = id;
                ImGui.SameLine(); ImGui.BeginDisabled(residents.Length <= 1);
                if (Button("farmOut/" + id, "Send to bank", 160, 42)) farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), FarmCareAction.MoveOut, id);
                ImGui.EndDisabled();
            }
            ImGui.SeparatorText("In the bank"u8);
            foreach (var item in CosmeticCollection.Items("legend").Where(x => collection.Owns("legend", x.Id) && !residents.Contains(x.Id)))
            {
                ImGui.Text(item.Name); ImGui.SameLine(); ImGui.BeginDisabled(residents.Length >= cap);
                if (Button("farmIn/" + item.Id, "Move in", 110, 38)) farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), FarmCareAction.MoveIn, item.Id);
                ImGui.EndDisabled();
                if (farmPet is not null)
                {
                    ImGui.SameLine(); if (Button("farmSwap/" + item.Id, "Swap with " + CosmeticCollection.Item("legend", farmPet)!.Name, 230, 38))
                        farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), FarmCareAction.Swap, item.Id, farmPet);
                }
            }
        }
        else
        {
            foreach (var theme in FarmCare.Themes)
            {
                bool owned = FarmCare.OwnsTheme(state, collection, theme), selected = state["theme"]!.GetValue<string>() == theme.Id;
                var colors = theme.Id switch
                {
                    "tropical" => (new ImColor8(63,182,240), new ImColor8(230,248,255)),
                    "snowy" => (new ImColor8(143,169,204), new ImColor8(238,243,250)),
                    "sewer" => (new ImColor8(23,34,27), new ImColor8(58,74,56)),
                    "toy" => (new ImColor8(255,196,225), new ImColor8(255,244,200)),
                    "grid" => (new ImColor8(5,13,28), new ImColor8(15,42,76)),
                    _ => (new ImColor8(124,200,242), new ImColor8(217,242,255))
                };
                var at = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(70, 48));
                ImGui.GetWindowDrawList().AddRectFilledMultiColor(at, at + new float2(70, 48), colors.Item1, colors.Item1, colors.Item2, colors.Item2);
                ImGui.SameLine(); ImGui.BeginGroup();
                ImGui.BeginDisabled(selected || !owned && collection.Bits < theme.Price);
                if (Button("farmTheme/" + theme.Id, theme.Name + (selected ? " · selected" : owned ? " · use" : $" · {theme.Price} Bits"), 0, 38))
                    farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), owned ? FarmCareAction.SetTheme : FarmCareAction.BuyTheme, theme.Id);
                ImGui.EndDisabled();
                if (theme.Board is not null && !owned) CollectionText("Also unlocked by owning " + (CosmeticCollection.Item("board", theme.Board)?.Name ?? theme.Board) + ".");
                ImGui.EndGroup(); ImGui.Spacing();
            }
        }
        ImGui.End();
    }
}
