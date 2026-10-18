using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private static readonly Dictionary<string, string> TrainingTitles = new() { ["strength"] = "Power Swing", ["speed"] = "Beat Dash", ["wisdom"] = "Data Memory", ["stamina"] = "Endurance Run" };
    private TrainingGame? training;
    private string trainingName = "", trainingPet = "";
    private double trainingTime;
    private double? trainingScore;
    private long trainingTimestamp;
    private bool trainingSaved;
    internal TrainingGame? ActiveTraining => training;
    internal bool TrainingStarted => trainingTime >= 0;
    private void StartTraining(string name)
    {
        if (farmPet is null || !farm!.CanTrain(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), farmPet)) return;
        training = TrainingGame.Create(name); trainingName = name; trainingPet = farmPet;
        trainingTime = -1.5; trainingScore = null; trainingSaved = false;
        farmView?.StartTraining(trainingPet);
    }
    private void UpdateTraining(double delta, double wallDelta)
    {
        if (trainingScore is not null) return;
        double step = Math.Min(.05, delta);
        if (trainingTime < 0) { trainingTime += step; return; }
        trainingTime += step; training!.Update(step, trainingTime, wallDelta);
        foreach (string sound in training.DrainSounds()) audio.Play(sound);
        if (training.Score is { } result) FinishTraining(result);
    }
    private void FinishTraining(double score)
    {
        trainingScore = score; trainingTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        SaveTraining(); audio.Play(score >= .6 ? "win" : "loss");
    }
    private void SaveTraining()
    {
        farm!.Act(trainingTimestamp, FarmCareAction.Train, trainingPet, trainingName, score: trainingScore!.Value);
        trainingSaved = farm.Error is null;
        if (trainingSaved) farmView?.FinishTraining(trainingPet, trainingScore.Value);
    }
    private void TrainingInput(int value)
    {
        if (trainingTime < 0 || trainingScore is not null) return;
        training!.Input(value);
        foreach (string sound in training.DrainSounds()) audio.Play(sound);
    }
    private void TrainingPanel()
    {
        ImGui.Text(TrainingTitles[trainingName] + " - " + CosmeticCollection.Item("legend", trainingPet)!.Name);
        if (trainingScore is { } score)
        {
            ImGui.Text((score >= .9 ? "Excellent!" : score >= .7 ? "Great!" : score >= .4 ? "Good" : "Keep practicing") + $"  Score {Math.Floor(score * 100 + .5)} / 100");
            if (!trainingSaved)
            {
                CollectionText(farm!.Error ?? "Training result could not be saved.");
                if (Button("retryTraining", "Retry saving result")) SaveTraining();
                return;
            }
            if (farm!.LastResult?["gains"] is null || farm.Message != farm.LastResult?["msg"]?.GetValue<string>()) CollectionText(farm.Message ?? "Training complete.");
            if (farm.LastResult is { } receipt && receipt["ok"]?.GetValue<bool>() == true)
            {
                if (receipt["gains"] is System.Text.Json.Nodes.JsonObject gains)
                    foreach (var gain in gains) ImGui.Text((gain.Key switch { "str" => "Strength", "spd" => "Speed", "wis" => "Wisdom", _ => "Stamina" }) + " +" + gain.Value);
                if (receipt["reward"] is { } reward) ImGui.Text($"+{reward["bits"]} Bits    +{reward["data"]} Digi-Data");
            }
            CollectionText("Training uses energy and hunger and trims a little weight.");
            if (Button("trainingAgain", "Again")) StartTraining(trainingName);
            ImGui.SameLine(); if (Button("trainingDone", "Done")) training = null;
            return;
        }
        if (Button("trainingQuit", "Quit")) { FinishTraining(0); return; }
        if (ImGui.IsKeyPressed(ImGuiKey.Escape, false)) { FinishTraining(0); return; }
        CollectionText(trainingName switch
        {
            "strength" => "Press Space / Enter or click when the marker enters the gold zone. Five swings.",
            "speed" => "Press F / Up for the top lane and J / Down for the bottom lane as notes reach the ring. You can click each lane.",
            "wisdom" => "Watch the four pads light up, then repeat the sequence. Click a pad or press 1-4.",
            _ => "Tap Space / Enter or click to keep the meter in the green band for twelve seconds."
        });
        var game = training!;
        ImGui.Text(trainingTime < 0 ? trainingTime < -1 ? "Ready" : "Go!" : game.Feedback);
        var origin = ImGui.GetCursorScreenPos(); float width = Math.Min(820, ImGui.GetContentRegionAvail().X), height = 280;
        var draw = ImGui.GetWindowDrawList();
        void Rect(float x, float y, float w, float h, ImColor8 color) => draw.AddRectFilled(new(origin.X + x, origin.Y + y), new(origin.X + x + w, origin.Y + y + h), color);
        void Label(float x, float y, string label, ImColor8 color) => draw.AddText(ImGui.GetFont(), 18, new(origin.X + x, origin.Y + y), color, label);
        var white = new ImColor8(235, 242, 250); var green = new ImColor8(72, 159, 99); var gold = new ImColor8(255, 207, 74);
        Rect(0, 0, width, height, new(15, 25, 39)); Rect(0, 0, width * (float)game.TimeLeft, 5, green);
        if (game is SwingTraining swing)
        {
            Rect(25, 123, width - 50, 35, new(48, 65, 86));
            Rect(25 + (width - 50) * (float)(swing.Center - swing.Zone / 2), 123, (width - 50) * (float)swing.Zone, 35, gold);
            Rect(25 + (width - 50) * (float)swing.Position - 3, 108, 6, 65, white);
            Label(25, 200, "SPACE / ENTER", white);
        }
        else if (game is BeatTraining beats)
        {
            for (int lane = 0; lane < 2; lane++)
            {
                float y = 65 + lane * 110;
                Rect(90, y + 20, width - 110, 2, new(48, 65, 86));
                draw.AddCircle(new(origin.X + 90, origin.Y + y + 20), 22, gold);
                Label(8, y - 35, lane == 0 ? "F / UP" : "J / DOWN", white);
                foreach (var note in beats.Notes.Where(n => n.Lane == lane && !n.Done && n.At - beats.Time <= 1.6))
                {
                    float x = 90 + (width - 110) * (float)((note.At - beats.Time) / 1.6);
                    draw.AddCircleFilled(new(origin.X + x, origin.Y + y + 20), 13, lane == 0 ? gold : new ImColor8(79, 179, 255));
                }
            }
        }
        else if (game is MemoryTraining memory)
        {
            ImColor8[] colors = [gold, new(255, 111, 154), new(79, 179, 255), new(159, 224, 122)];
            for (int pad = 0; pad < 4; pad++)
            {
                float x = 20 + pad * (width - 40) / 4;
                Rect(x, 60, (width - 60) / 4, 150, memory.LitPad == pad ? colors[pad] : new(40, 53, 72));
                Label(x + 25, 120, (pad + 1).ToString(), memory.LitPad == pad ? new(15, 25, 39) : colors[pad]);
            }
        }
        else if (game is EnduranceTraining endurance)
        {
            Rect(80, 30, 100, 220, new(48, 65, 86)); Rect(80, 30 + 220 * .18f, 100, 220 * .44f, green);
            Rect(110, 30 + 220 * (1 - (float)endurance.Meter), 40, 220 * (float)endurance.Meter, gold);
            Label(220, 110, "KEEP THE METER IN THE GREEN BAND", white); Label(220, 150, "SPACE / ENTER", gold);
        }
        ImGui.InvisibleButton("trainingStage"u8, new(width, height));
        Buttons["trainingStage"] = new(origin.X + width / 2, origin.Y + height / 2);
        for (int pad = 0; pad < 4; pad++) Buttons["trainingPad/" + pad] = new(origin.X + 20 + (pad + .5f) * (width - 40) / 4, origin.Y + 130);
        for (int lane = 0; lane < 2; lane++) Buttons["trainingLane/" + lane] = new(origin.X + width / 2, origin.Y + 85 + lane * 110);
        if (ImGui.IsItemActivated())
        {
            var mouse = ImGui.GetMousePos();
            int input = game is BeatTraining ? mouse.Y - origin.Y < 140 ? 0 : 1
                : game is MemoryTraining ? (int)((mouse.X - origin.X - 20) / ((width - 40) / 4)) : 0;
            if (game is not MemoryTraining || mouse.Y - origin.Y is >= 60 and <= 210 && mouse.X - origin.X >= 20 && mouse.X - origin.X <= width - 20) TrainingInput(input);
        }
        foreach (var (key, value) in new (ImGuiKey, string)[] { (ImGuiKey.Space, " "), (ImGuiKey.Enter, "Enter"), (ImGuiKey.F, "f"), (ImGuiKey.UpArrow, "ArrowUp"), (ImGuiKey.W, "w"), (ImGuiKey.J, "j"), (ImGuiKey.DownArrow, "ArrowDown"), (ImGuiKey.S, "s"), (ImGuiKey._1, "1"), (ImGuiKey._2, "2"), (ImGuiKey._3, "3"), (ImGuiKey._4, "4") })
            if (ImGui.IsKeyPressed(key, false) && game.Key(value) is { } input) TrainingInput(input);
        ImGui.Text(game.Left + "     " + game.Right);
    }
}
