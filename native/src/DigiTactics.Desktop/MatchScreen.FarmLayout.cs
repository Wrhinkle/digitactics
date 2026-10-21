using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private string? farmActionMenu;
    private string farmManagementPage = "bank";

    private void FarmWindow(string name, float x, float y, float width, float height, ImGuiWindowFlags flags = 0)
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new ImColor8(1, 10, 19));
        ImGui.PushStyleColor(ImGuiCol.Border, ClientBorder);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new float2(12, 10));
        Window(name, x, y, width, height, flags | ImGuiWindowFlags.NoTitleBar);
        ImGui.PopStyleVar(2); ImGui.PopStyleColor(2);
    }

    private static void FarmBox(string name)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new ImColor8(22, 31, 48));
        ImGui.PushStyleColor(ImGuiCol.Border, new ImColor8(48, 65, 89));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new float2(10, 9));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new float2(6, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0);
        ImGui.BeginChild(name, new(0, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);
        ImGui.PopStyleVar(3); ImGui.PopStyleColor(2);
    }

    private static void FarmBoxHeading(string name, string? detail = null)
    {
        var at = ImGui.GetCursorScreenPos(); var draw = ImGui.GetWindowDrawList();
        draw.AddText(NativeUiTheme.DisplayBoldFont, 16, at, new ImColor8(146, 160, 181), name);
        if (detail is not null)
        {
            ImGui.PushFont(default, 15); float width = ImGui.CalcTextSize(detail).X;
            draw.AddText(ImGui.GetFont(), 15, new(at.X + ImGui.GetContentRegionAvail().X - width, at.Y), ClientMuted, detail); ImGui.PopFont();
        }
        ImGui.Dummy(new(1, 20));
    }

    private static void FarmHeading(string text, float height = 24)
    {
        var at = ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddText(NativeUiTheme.DisplayBoldFont, height, at, NativeUiTheme.GoldBright, text);
        ImGui.Dummy(new(1, height + 4));
    }

    private void FarmPanel()
    {
        DisplayedFarmDetails = null; FarmEvolutionBranches = 0;
        JsonNode? state;
        try { state = farm?.Save is { } saved ? JsonNode.Parse(saved) : null; }
        catch (System.Text.Json.JsonException) { state = null; }
        if (state is null)
        {
            FarmWindow("Digi-Farm unavailable", 16, ClientPageTop + 12, size.X - 32, 190);
            FarmHeading("Digi-Farm"); CollectionText(farm?.Error ?? "Your farm is loading.");
            if (Button("retryFarm", "Retry saving farm", 200, 38)) farm?.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            ImGui.SameLine(); if (Button("closeFarm", "Close", 90, 38)) BackClient();
            ImGui.End(); return;
        }
        var residents = state["residents"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
        if (!residents.Contains(farmPet)) farmPet = residents.FirstOrDefault();
        FarmHeader(state, residents);
        FarmRoomScene(state, residents);
        FarmWindow("Digi-Farm resident", FarmSceneX + FarmSceneWidth, FarmSceneTop, FarmCardWidth, FarmSceneHeight);
        ImGui.BeginDisabled(farmManagement || farmActionMenu is not null || training is not null);
        if (farm!.Error is { } error)
        {
            CollectionText(error);
            if (Button("retryFarm", "Retry saving farm", ImGui.GetContentRegionAvail().X, 38)) farm.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
        if (farmPet is not null) FarmResidentCard(state, residents, farmPet);
        ImGui.EndDisabled();
        ImGui.End();
        if (training is not null) FarmTrainingOverlay();
        else
        {
            if (farmActionMenu is not null) FarmActionChoices(state);
            if (farmManagement) FarmRoomControls(state, residents);
        }
    }

    private void FarmTrainingOverlay()
    {
        // Training is a layer over the living room, including the resident card
        // and action bar. Keep that context visible while blocking its controls.
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new ImColor8(0, 0, 0, 174));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new float2(0, 0));
        Window("Digi-Farm training shade", 24, ClientPageTop + 12, size.X - 48, size.Y - ClientPageTop - 28,
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.End(); ImGui.PopStyleVar(); ImGui.PopStyleColor();
        float width = Math.Min(760, size.X - 72), height = Math.Min(TrainingDialogHeight(width), size.Y - ClientPageTop - 52);
        ImGui.SetNextWindowFocus();
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new ImColor8(1, 10, 19));
        ImGui.PushStyleColor(ImGuiCol.Border, NativeUiTheme.GoldBorder);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new float2(16, 14));
        Window("Digi-Farm training", (size.X - width) / 2, ClientPageTop + 12 + (size.Y - ClientPageTop - 28 - height) / 2, width, height, ImGuiWindowFlags.NoTitleBar);
        ImGui.PopStyleVar(2); ImGui.PopStyleColor(2);
        var at = ImGui.GetWindowPos() + new float2(1, 1);
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(at, at + ImGui.GetWindowSize() - new float2(2, 2),
            new ImColor8(10, 20, 40), new ImColor8(10, 20, 40), new ImColor8(1, 10, 19), new ImColor8(1, 10, 19));
        TrainingPanel(); ImGui.End();
    }

    private void FarmHeader(JsonNode state, string[] residents)
    {
        float top = ClientPageTop + 12, width = size.X - 48;
        FarmWindow("Digi-Farm header", 24, top, width, FarmHeaderHeight, ImGuiWindowFlags.NoScrollbar);
        var draw = ImGui.GetWindowDrawList();
        draw.AddRectFilledMultiColor(new(25, top + 1), new(24 + width - 1, top + FarmHeaderHeight - 1), new ImColor8(10, 32, 38), new ImColor8(10, 32, 38), new ImColor8(1, 10, 19), new ImColor8(1, 10, 19));
        ImGui.SetCursorScreenPos(new(38, top + 16)); FarmHeading("DIGI-FARM", 22);
        ImGui.SetCursorScreenPos(new(188, top + 19));
        ImGui.PushFont(default, 16); ImGui.PushStyleColor(ImGuiCol.Text, ClientMuted);
        ImGui.Text(FarmCare.Themes.FirstOrDefault(t => t.Id == state["theme"]!.GetValue<string>())?.Name ?? "Room");
        ImGui.PopStyleColor(); ImGui.PopFont();
        float controlsX = 24 + width - 406;
        ImGui.SetCursorScreenPos(new(controlsX, top + 10));
        ImGui.BeginDisabled(farmManagement || training is not null);
        if (Button("farmRoom", "Digi-Bank", 104, 32)) { farmManagement = true; farmManagementPage = "bank"; farmActionMenu = null; }
        ImGui.SameLine(0, 6);
        if (Button("farmThemes", "Themes", 88, 32)) { farmManagement = true; farmManagementPage = "themes"; farmActionMenu = null; }
        ImGui.SameLine(0, 6);
        if (Button("farmCaretakers", "Care staff", 106, 32, FarmCaretaker.Current(state, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) is not null)) { farmManagement = true; farmManagementPage = "caretakers"; farmActionMenu = null; }
        ImGui.SameLine(0, 6);
        if (Button("closeFarm", "Close", 72, 32)) BackClient();
        ImGui.EndDisabled();
        bool narrow = FarmHeaderHeight > 52;
        float pillsX = narrow ? 38 : Math.Max(360, controlsX - 458);
        ImGui.SetCursorScreenPos(new(pillsX, top + (narrow ? 54 : 16)));
        ImGui.PushFont(default, 15);
        string[] supplies = [$"ROOM {residents.Length}/{state["cap"]}", $"MEAT {state["meat"]!["n"]}/3", $"HEAL {state["bandage"]!["n"]}/2", $"BITS {state["daily"]!["bits"]}/60", $"DATA {state["daily"]!["data"]}/100"];
        foreach (string supply in supplies)
        {
            var at = ImGui.GetCursorScreenPos(); var extent = ImGui.CalcTextSize(supply) + new float2(12, 8);
            draw.AddRectFilled(at, at + extent, new ImColor8(1, 10, 19)); draw.AddRect(at, at + extent, ClientBorder);
            draw.AddText(at + new float2(6, 4), ClientInk, supply); ImGui.Dummy(extent); ImGui.SameLine(0, 5);
        }
        ImGui.PopFont(); ImGui.End();
    }

    private void FarmRoomScene(JsonNode state, string[] residents)
    {
        FarmWindow("Digi-Farm room", FarmSceneX, FarmSceneTop, FarmSceneWidth, FarmSceneHeight, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
        ImGui.BeginDisabled(farmManagement || farmActionMenu is not null || training is not null);
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
            if (!farmManagement && farmActionMenu is null && training is null && picked is { } hit2 && !ImGui.IsAnyItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                if (hit2.Pile is { } key) FarmAct(FarmCareAction.CleanPile, pile: key, resident: hit2.Id);
                else { farmPet = hit2.Id; FarmAct(FarmCareAction.Pet); }
            }
        }
        DrawFarmPops();
        ImGui.EndDisabled();
        float buttonWidth = Math.Min(74, (FarmSceneWidth - 40 - 7 * 6) / 8f);
        float dockWidth = buttonWidth * 8 + 7 * 6 + 16;
        var dockAt = new float2(FarmSceneX + (FarmSceneWidth - dockWidth) / 2, FarmSceneTop + FarmSceneHeight - 76);
        var dockDraw = ImGui.GetWindowDrawList();
        dockDraw.AddRectFilled(dockAt, dockAt + new float2(dockWidth, 66), new ImColor8(9, 20, 30, 242), 12);
        dockDraw.AddRect(dockAt, dockAt + new float2(dockWidth, 66), ClientBorder, 12);
        ImGui.SetCursorScreenPos(dockAt + new float2(8, 7));
        ImGui.BeginDisabled(farmManagement || training is not null);
        void CareButton(string key, string text, FarmSymbol? symbol, Action action, bool active = false)
        {
            var at = ImGui.GetCursorScreenPos();
            if (Button(key, "##" + key, buttonWidth, 52, active)) action();
            DrawFarmActionIcon(symbol, text, at + new float2(buttonWidth / 2, 17));
            ImGui.PushFont(default, 14);
            float labelWidth = ImGui.CalcTextSize(text).X;
            dockDraw.AddText(ImGui.GetFont(), 14, at + new float2((buttonWidth - labelWidth) / 2, 32), ClientGold, text);
            ImGui.PopFont(); ImGui.SameLine(0, 6);
        }
        CareButton("farmAction/feed", "Feed", FarmSymbol.Meat, () => farmActionMenu = farmActionMenu == "feed" ? null : "feed", farmActionMenu == "feed");
        CareButton("farmClean", "Clean", null, () => FarmAct(FarmCareAction.Clean));
        CareButton("farmAction/cure", "Medicine", FarmSymbol.Medicine, () => farmActionMenu = farmActionMenu == "cure" ? null : "cure", farmActionMenu == "cure");
        CareButton("farmLights", state["lights"]!.GetValue<bool>() ? "Lights off" : "Lights on", FarmSymbol.Sleep, () => FarmAct(FarmCareAction.Lights), !state["lights"]!.GetValue<bool>());
        CareButton("farmPraise", "Praise", FarmSymbol.Heart, () => FarmAct(FarmCareAction.Praise));
        CareButton("farmScold", "Scold", FarmSymbol.Exclaim, () => FarmAct(FarmCareAction.Scold));
        CareButton("farmAction/train", "Train", null, () => farmActionMenu = farmActionMenu == "train" ? null : "train", farmActionMenu == "train");
        CareButton("farmPlay", "Play", FarmSymbol.Music, () => FarmAct(FarmCareAction.Play));
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

    private void DrawFarmActionIcon(FarmSymbol? symbol, string label, float2 center)
    {
        var draw = ImGui.GetWindowDrawList();
        if (symbol is { } icon)
        {
            farmSymbolTexture ??= new(DigiTactics.Assets.PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content/farm-symbols.png"))), new(MinFilter: 9729, WrapS: 33071, WrapT: 33071), false);
            draw.AddImage(new ImTextureRef { _TexID = new((nint)farmSymbolTexture.Id) }, center - new float2(13, 13), center + new float2(13, 13), new((int)icon / 18f, 0), new(((int)icon + 1) / 18f, 1));
        }
        else if (label == "Train")
        {
            draw.AddLine(center - new float2(10, 0), center + new float2(10, 0), ClientGold, 3);
            draw.AddRectFilled(center - new float2(12, 8), center + new float2(-7, 8), ClientGold, 1);
            draw.AddRectFilled(center + new float2(7, -8), center + new float2(12, 8), ClientGold, 1);
        }
        else
        {
            draw.AddLine(center + new float2(6, -12), center + new float2(-3, 3), new ImColor8(190, 130, 70), 3);
            draw.AddTriangleFilled(center + new float2(-4, 0), center + new float2(-12, 10), center + new float2(6, 10), ClientGold);
        }
    }

    private void FarmResidentCard(JsonNode state, string[] residents, string id)
    {
        var pet = state["pets"]![id]!; var details = FarmDetails(id); DisplayedFarmDetails = details;
        var form = LegendEvolution.Form(details.Evolution.Form);
        ImGui.PushFont(default, 16); ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new float2(6, 6));
        var at = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(58, 58)); portraits.DrawEvolution(form.Id, at, 58, true);
        ImGui.SameLine(); ImGui.BeginGroup(); FarmHeading(form.Name, 22);
        ImGui.Text($"{form.Level} · Lv {details.Evolution.Level}");
        if (farmView?.Mood(id) is { } mood) { ImGui.PushStyleColor(ImGuiCol.Text, ClientMuted); ImGui.Text(mood.Text); ImGui.PopStyleColor(); }
        ImGui.EndGroup();
        FarmBox("Farm needs");
        FarmMeter("Hunger", pet["hunger"]!.GetValue<double>(), new(255, 178, 95));
        FarmMeter("Energy", pet["energy"]!.GetValue<double>(), new(108, 204, 255));
        FarmMeter("Happiness", pet["happy"]!.GetValue<double>(), new(255, 139, 184));
        FarmMeter("Discipline", pet["disc"]!.GetValue<double>(), new(173, 153, 246));
        ImGui.EndChild();
        ImGui.PushStyleColor(ImGuiCol.Text, details.Sick ? new ImColor8(255, 140, 120) : new ImColor8(173, 208, 139));
        UiText.Wrapped($"{pet["weight"]} G  ·  {details.AgeDays} days  ·  {details.Mistakes} care mistakes");
        ImGui.PopStyleColor();
        if (details.Sick || details.Fat || details.Thin || pet["flags"]?["asleep"]?.GetValue<bool>() == true)
            UiText.Wrapped((details.Sick ? "Sick: " + pet["sickWhy"] : "Healthy") + (details.Fat ? " · Chubby" : details.Thin ? " · Thin" : "") + (pet["flags"]?["asleep"]?.GetValue<bool>() == true ? " · Sleeping" : ""));
        FarmBox("Farm training stats"); FarmBoxHeading("TRAINING", "cap 300");
        if (ImGui.BeginTable("farm-stats"u8, 2, ImGuiTableFlags.SizingStretchSame))
        {
            foreach (var (key, name) in new[] { ("str", "Strength"), ("spd", "Speed"), ("wis", "Wisdom"), ("sta", "Stamina") })
            {
                ImGui.TableNextColumn(); ImGui.Text($"{name}  {pet[key]}");
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new ImColor8(175, 218, 126)); ImGui.ProgressBar((float)(pet[key]!.GetValue<double>() / 300), new(-1, 5), ""u8); ImGui.PopStyleColor();
            }
            ImGui.EndTable();
        }
        if (details.TrainingRefusal is { } refusal) UiText.Wrapped(refusal);
        ImGui.EndChild();
        FarmEvolutionDetails(id, details);
        if (ImGui.CollapsingHeader("More care actions"u8))
        {
            if (Button("farmPet", "Pet " + CosmeticCollection.Item("legend", id)!.Name, ImGui.GetContentRegionAvail().X, 32)) FarmAct(FarmCareAction.Pet);
            foreach (string other in residents.Where(x => x != id))
                if (Button("farmPlay/" + other, "Play with " + CosmeticCollection.Item("legend", other)!.Name, 0, 34)) FarmAct(FarmCareAction.Play, other);
            foreach (var pile in pet["poop"]!.AsArray())
                if (Button("farmPile/" + pile!["k"], "Clean pile " + pile["k"], 0, 34)) FarmAct(FarmCareAction.CleanPile, pile: pile["k"]!.GetValue<int>());
        }
        ImGui.PopStyleVar(); ImGui.PopFont();
    }

    private static void FarmMeter(string label, double value, ImColor8 color)
    {
        var at = ImGui.GetCursorScreenPos(); float width = ImGui.GetContentRegionAvail().X;
        var draw = ImGui.GetWindowDrawList(); float barX = at.X + 96, barWidth = Math.Max(24, width - 130);
        draw.AddText(at, ClientInk, label);
        draw.AddRectFilled(new(barX, at.Y + 5), new(barX + barWidth, at.Y + 13), new ImColor8(6, 14, 26), 4);
        draw.AddRectFilled(new(barX, at.Y + 5), new(barX + barWidth * (float)Math.Clamp(value / 100, 0, 1), at.Y + 13), color, 4);
        string number = $"{value:0}"; draw.AddText(new(at.X + width - ImGui.CalcTextSize(number).X, at.Y), ClientMuted, number);
        ImGui.Dummy(new(width, 19));
    }

    private bool FarmChoiceButton(string key,string title,string caption,string price,FarmSymbol? icon=null)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        ImGui.PushFont(default,14);float captionHeight=ImGui.CalcTextSize(caption,false,width-68).Y;ImGui.PopFont();
        float height=Math.Max(62,captionHeight+36);
        bool clicked=Button(key,"##"+key,width,height);
        if(icon is not null)DrawFarmActionIcon(icon,title,at+new float2(22,height/2));
        var draw=ImGui.GetWindowDrawList();
        draw.AddText(NativeUiTheme.DisplayBoldFont,18,at+new float2(46,8),ClientInk,title);
        ImGui.PushFont(default,14);
        draw.AddText(ImGui.GetFont(),14,new(at.X+width-ImGui.CalcTextSize(price).X-10,at.Y+9),ClientGold,price);
        draw.AddText(ImGui.GetFont(),14,at+new float2(46,31),ClientMuted,caption,width-60);
        ImGui.PopFont();return clicked;
    }

    private void FarmActionChoices(JsonNode state)
    {
        string menu = farmActionMenu!;
        float width = Math.Min(500, FarmSceneWidth - 24), height = Math.Min(menu == "feed" ? 438 : menu == "train" ? 400 : 220,FarmSceneHeight-96);
        ImGui.SetNextWindowFocus();
        FarmWindow("Digi-Farm care choices", FarmSceneX + (FarmSceneWidth - width) / 2, FarmSceneTop + FarmSceneHeight - 84 - height, width, height);
        var headerAt=ImGui.GetCursorScreenPos();
        FarmHeading(menu == "feed" ? "Choose a meal" : menu == "train" ? "Training games" : "Medicine", 22);
        ImGui.SetCursorScreenPos(new(headerAt.X+width-100,headerAt.Y-3));
        if (Button("farmActionClose", "Close", 76, 28)) farmActionMenu = null;
        ImGui.SetCursorScreenPos(headerAt+new float2(0,34));
        if (menu == "feed")
        {
            foreach (var (id,title,caption,price,icon) in new[] {
                ("meat","Meat","+30 hunger, +1 G.",state["meat"]!["n"]!.GetValue<int>()>0?$"Free ({state["meat"]!["n"]} left)":"5 Bits",FarmSymbol.Meat),
                ("carrot","Supercarrot","+25 hunger, +6 energy. No weight gain.","8 Bits",FarmSymbol.Carrot),
                ("bigmeat","Giant Meat","+70 hunger, +3 G, +4 happiness.","15 Bits",FarmSymbol.BigMeat),
                ("sirloin","Sirloin","+50 hunger, +2 G, +15 happiness, +10 energy.","25 Bits",FarmSymbol.Sirloin),
                ("cake","Digi-Cake","+10 hunger, +3 G, +20 happiness, -5 discipline.","10 Bits",FarmSymbol.Cake) })
                if (FarmChoiceButton("farmFeed/"+id,title,caption,price,icon)) { FarmAct(FarmCareAction.Feed, id); farmActionMenu = null; }
        }
        else if (menu == "train")
        {
            string? refusal = farmPet is null ? "Select a resident." : FarmDetails(farmPet).TrainingRefusal;
            if (refusal is not null) UiText.Wrapped(refusal);
            ImGui.BeginDisabled(refusal is not null);
            foreach (var game in TrainingTitles)
            {
                string help=game.Key switch {
                    "strength"=>"Press Space or click in the gold zone. Five swings.",
                    "speed"=>"Hit F / Up or J / Down as notes reach the ring.",
                    "wisdom"=>"Watch, then repeat the pads. Click or press 1-4.",
                    _=>"Tap Space or click to keep the meter in the green band." };
                if (FarmChoiceButton("farmTrain/"+game.Key,game.Value,help,game.Key)) { StartTraining(game.Key); farmActionMenu = null; }
            }
            ImGui.EndDisabled();
        }
        else
        {
            ImGui.BeginDisabled(state["bandage"]!["n"]!.GetValue<int>()<=0);
            if (FarmChoiceButton("farmBandage","Bandage","Cures sickness. Free supplies refill over time.",$"{state["bandage"]!["n"]}/2",FarmSymbol.Bandage)) { FarmAct(FarmCareAction.Cure); farmActionMenu = null; }
            ImGui.EndDisabled();ImGui.BeginDisabled(profile!.Progress.Bits<20);
            if (FarmChoiceButton("farmMedicine","Medicine","Cures sickness. Bitter: -5 happiness.","20 Bits",FarmSymbol.Medicine)) { FarmAct(FarmCareAction.Cure, "medicine"); farmActionMenu = null; }
            ImGui.EndDisabled();
        }
        ImGui.End();
    }

    private void FarmRoomControls(JsonNode state, string[] residents)
    {
        float width = Math.Min(780, size.X - 64), height = Math.Min(640, size.Y - ClientPageTop - 32);
        ImGui.SetNextWindowFocus();
        FarmWindow("Digi-Farm management", (size.X - width) / 2, ClientPageTop + 16, width, height);
        FarmManagementBounds = (ImGui.GetWindowDrawList().GetClipRectMin(), ImGui.GetWindowDrawList().GetClipRectMax());
        FarmHeading(farmManagementPage == "bank" ? "Digi-Bank" : farmManagementPage == "caretakers" ? "Care staff" : "Room themes", 26);
        if (Button("farmManageBank", "Digi-Bank", 130, 38, farmManagementPage == "bank")) farmManagementPage = "bank";
        ImGui.SameLine(); if (Button("farmManageThemes", "Themes", 110, 38, farmManagementPage == "themes")) farmManagementPage = "themes";
        ImGui.SameLine(); if (Button("farmManageCaretakers", "Care staff", 130, 38, farmManagementPage == "caretakers")) farmManagementPage = "caretakers";
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
            FarmBankSlots(residents,true,residents,cap);
            ImGui.SeparatorText("Digi-Bank"u8);
            var bank=CosmeticCollection.Items("legend").Where(x=>collection.Owns("legend",x.Id)&&!residents.Contains(x.Id)).Select(x=>x.Id).ToArray();
            if(bank.Length==0)UiText.Wrapped("Nobody in the bank. Every Little Legend you own lives in the room."u8);
            else FarmBankSlots(bank,false,residents,cap);
        }
        else if (farmManagementPage == "caretakers") FarmCaretakerControls(state);
        else
        {
            float available=ImGui.GetContentRegionAvail().X;int columns=Math.Max(1,(int)(available/220));
            float tileWidth=(available-(columns-1)*10)/columns;
            int index=0;
            foreach (var theme in FarmCare.Themes)
            {
                if(index++%columns!=0)ImGui.SameLine(0,10);
                bool owned = FarmCare.OwnsTheme(state, collection, theme), selected = state["theme"]!.GetValue<string>() == theme.Id;
                ClientCardBegin("Farm theme "+theme.Id,new(tileWidth,224));
                var colors = theme.Id switch
                {
                    "tropical" => (new ImColor8(63,182,240), new ImColor8(230,248,255),new ImColor8(234,213,156)),
                    "snowy" => (new ImColor8(143,169,204), new ImColor8(238,243,250),new ImColor8(242,246,251)),
                    "sewer" => (new ImColor8(23,34,27), new ImColor8(58,74,56),new ImColor8(95,110,88)),
                    "toy" => (new ImColor8(255,196,225), new ImColor8(255,244,200),new ImColor8(159,224,176)),
                    "grid" => (new ImColor8(5,13,28), new ImColor8(15,42,76),new ImColor8(15,36,64)),
                    _ => (new ImColor8(124,200,242), new ImColor8(217,242,255),new ImColor8(116,194,85))
                };
                var at = ImGui.GetCursorScreenPos();float swatchWidth=ImGui.GetContentRegionAvail().X; ImGui.Dummy(new(swatchWidth,68));
                var draw=ImGui.GetWindowDrawList();
                draw.AddRectFilledMultiColor(at,at+new float2(swatchWidth,44),colors.Item1,colors.Item1,colors.Item2,colors.Item2);
                draw.AddRectFilled(at+new float2(0,44),at+new float2(swatchWidth,68),colors.Item3);
                ImGui.PushFont(NativeUiTheme.DisplayBoldFont,19);UiText.Wrapped(theme.Name);ImGui.PopFont();
                ImGui.BeginDisabled(selected || !owned && collection.Bits < theme.Price);
                if (Button("farmTheme/" + theme.Id, selected ? "In use" : owned ? "Use theme" : $"Buy / {theme.Price} Bits", ImGui.GetContentRegionAvail().X, 34))
                    farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), owned ? FarmCareAction.SetTheme : FarmCareAction.BuyTheme, theme.Id);
                ImGui.EndDisabled();
                if(theme.Board is not null&&!owned)
                {
                    ImGui.PushFont(default,14);ImGui.PushStyleColor(ImGuiCol.Text,ClientMuted);
                    CollectionText("Also included with "+(CosmeticCollection.Item("board",theme.Board)?.Name??theme.Board)+".");
                    ImGui.PopStyleColor();ImGui.PopFont();
                }
                ImGui.EndChild();
            }
        }
        ImGui.End();
    }
    private void FarmBankSlots(string[] ids,bool inRoom,string[] residents,int capacity)
    {
        float available=ImGui.GetContentRegionAvail().X;int columns=Math.Max(1,(int)(available/220));float width=(available-(columns-1)*10)/columns;
        for(int index=0;index<ids.Length;index++)
        {
            string id=ids[index];if(index%columns!=0)ImGui.SameLine(0,10);
            ClientCardBegin("Farm bank slot "+id,new(width,196));
            var evolution=LegendEvolution.Inspect(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),profile.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile.Storage.GetValueOrDefault("dt.farm.v1"),id);
            var at=ImGui.GetCursorScreenPos();float portraitX=at.X+(ImGui.GetContentRegionAvail().X-64)/2;
            ImGui.SetCursorScreenPos(new(portraitX,at.Y));
            if(Button("farmSelect/"+id,"##select"+id,64,64,inRoom&&farmPet==id)&&inRoom)farmPet=id;
            portraits.DrawEvolution(evolution.Form,new(portraitX+2,at.Y+2),60,true);
            ImGui.SetCursorScreenPos(new(at.X,at.Y+70));
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,18);UiText.Wrapped(LegendEvolution.Form(evolution.Form).Name);ImGui.PopFont();
            ImGui.PushFont(default,14);ImGui.Text($"Level {evolution.Level}");ImGui.PopFont();
            if(inRoom)
            {
                ImGui.BeginDisabled(residents.Length<=1);
                if(Button("farmOut/"+id,"To Digi-Bank",ImGui.GetContentRegionAvail().X,32))farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),FarmCareAction.MoveOut,id);
                ImGui.EndDisabled();
            }
            else if(residents.Length<capacity)
            {
                if(Button("farmIn/"+id,"Move in",ImGui.GetContentRegionAvail().X,32))farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),FarmCareAction.MoveIn,id);
            }
            else if(farmPet is not null&&Button("farmSwap/"+id,"Swap with selected",ImGui.GetContentRegionAvail().X,32))
                farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),FarmCareAction.Swap,id,farmPet);
            ImGui.EndChild();
        }
    }

}
