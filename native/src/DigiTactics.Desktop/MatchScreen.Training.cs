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
        float contentWidth = ImGui.GetContentRegionAvail().X;
        FarmHeading(TrainingTitles[trainingName], 28);
        ImGui.Text(CosmeticCollection.Item("legend", trainingPet)!.Name);
        if (trainingScore is { } score)
        {
            ImGui.Spacing(); FarmHeading(score >= .9 ? "Excellent!" : score >= .7 ? "Great!" : score >= .4 ? "Good" : "Keep practicing", 32);
            FarmHeading($"{Math.Floor(score * 100 + .5)} / 100", 44);
            if (!trainingSaved)
            {
                CollectionText(farm!.Error ?? "Training result could not be saved.");
                if (Button("retryTraining", "Retry saving result", 240, 44)) SaveTraining();
                return;
            }
            if (farm!.LastResult?["gains"] is null || farm.Message != farm.LastResult?["msg"]?.GetValue<string>()) CollectionText(farm.Message ?? "Training complete.");
            if (farm.LastResult is { } receipt && receipt["ok"]?.GetValue<bool>() == true)
            {
                if (receipt["gains"] is System.Text.Json.Nodes.JsonObject gains)
                {
                    if (ImGui.BeginTable("training-gains"u8, 2, ImGuiTableFlags.SizingStretchSame))
                    {
                        foreach (var gain in gains)
                        {
                            ImGui.TableNextColumn(); FarmHeading((gain.Key switch { "str" => "Strength", "spd" => "Speed", "wis" => "Wisdom", _ => "Stamina" }) + " +" + gain.Value, 24);
                        }
                        ImGui.EndTable();
                    }
                }
                if (receipt["reward"] is { } reward) ImGui.Text($"+{reward["bits"]} Bits    +{reward["data"]} Digi-Data");
            }
            ImGui.Spacing(); CollectionText("Training uses energy and hunger and trims a little weight.");
            if (Button("trainingAgain", "Again", 150, 44)) StartTraining(trainingName);
            ImGui.SameLine(); if (Button("trainingDone", "Done", 150, 44)) training = null;
            return;
        }
        var afterHeading = ImGui.GetCursorScreenPos();
        ImGui.SetCursorScreenPos(new(afterHeading.X + contentWidth - 100, afterHeading.Y - 62));
        // Cancelling does not consume care or award training rewards.
        if (Button("trainingQuit", "Quit", 100, 38) || ImGui.IsKeyPressed(ImGuiKey.Escape, false)) { training = null; return; }
        ImGui.SetCursorScreenPos(afterHeading);
        CollectionText(trainingName switch
        {
            "strength" => "Press Space / Enter or click when the marker enters the gold zone. Five swings.",
            "speed" => "F / Up: top lane. J / Down: bottom lane. Hit each note as it reaches the ring, or click the lane.",
            "wisdom" => "Watch the four pads light up, then repeat the sequence. Click a pad or press 1-4.",
            _ => "Tap Space / Enter or click to keep the meter in the green band for twelve seconds."
        });
        ImGui.Spacing();
        var game = training!;
        var origin = ImGui.GetCursorScreenPos(); float width = contentWidth, height = 280;
        var draw = ImGui.GetWindowDrawList();
        void Rect(float x, float y, float w, float h, ImColor8 color, float rounding = 0) => draw.AddRectFilled(new(origin.X + x, origin.Y + y), new(origin.X + x + w, origin.Y + y + h), color, rounding);
        void Label(float x, float y, string label, ImColor8 color, float fontSize = 18) => draw.AddText(ImGui.GetFont(), fontSize, new(origin.X + x, origin.Y + y), color, label);
        var white = new ImColor8(235, 242, 250); var green = new ImColor8(72, 159, 99); var gold = new ImColor8(255, 207, 74);
        Rect(0, 0, width, height, new(12, 18, 32), 6);
        Rect(0, 0, width * (float)game.TimeLeft, 5, green);
        string feedback = trainingTime < 0 ? trainingTime < -1 ? "Ready" : "Go!" : game.Feedback;
        var feedbackSize = ImGui.CalcTextSize(feedback) * (28 / ImGui.GetFontSize());
        Label((width - feedbackSize.X) / 2, 14, feedback, gold, 28);
        if (game is SwingTraining swing)
        {
            float trackX = width * .08f, trackWidth = width * .84f;
            Rect(width / 2 - 30, 64, 60, 70, new(133, 87, 54), 8);
            for (int line = 0; line < 4; line++) Rect(width / 2 - 24 + line * 14, 70, 3, 55, new(90, 58, 36));
            Rect(trackX, 150, trackWidth, 34, new(48, 65, 86), 4);
            Rect(trackX + trackWidth * (float)(swing.Center - swing.Zone / 2), 150, trackWidth * (float)swing.Zone, 34, gold);
            Rect(trackX + trackWidth * (float)swing.Position - 3, 140, 6, 54, white);
            Label(trackX, 220, "SPACE / ENTER", white, 20);
        }
        else if (game is BeatTraining beats)
        {
            for (int lane = 0; lane < 2; lane++)
            {
                float y = 94 + lane * 110;
                Rect(90, y, width - 110, 2, new(48, 65, 86));
                draw.AddCircle(new(origin.X + 90, origin.Y + y), 27, gold);
                Label(12, y - 47, lane == 0 ? "F / UP" : "J / DOWN", white);
                foreach (var note in beats.Notes.Where(n => n.Lane == lane && !n.Done && n.At - beats.Time <= 1.6))
                {
                    float x = 90 + (width - 110) * (float)((note.At - beats.Time) / 1.6);
                    draw.AddCircleFilled(new(origin.X + x, origin.Y + y), 20, lane == 0 ? gold : new ImColor8(79, 179, 255));
                }
            }
        }
        else if (game is MemoryTraining memory)
        {
            ImColor8[] colors = [gold, new(255, 111, 154), new(79, 179, 255), new(159, 224, 122)];
            for (int pad = 0; pad < 4; pad++)
            {
                float x = width / 2 - 102 + pad % 2 * 108, y = 50 + pad / 2 * 108;
                Rect(x, y, 96, 96, memory.LitPad == pad ? colors[pad] : new(40, 53, 72), 10);
                draw.AddRect(new(origin.X + x, origin.Y + y), new(origin.X + x + 96, origin.Y + y + 96), colors[pad], 10);
                Label(x + 37, y + 28, (pad + 1).ToString(), memory.LitPad == pad ? new(15, 25, 39) : colors[pad], 38);
            }
        }
        else if (game is EnduranceTraining endurance)
        {
            float meterX = width / 2 - 28;
            Rect(meterX, 48, 56, 202, new(48, 65, 86), 6);
            Rect(meterX, 48 + 202 * .18f, 56, 202 * .44f, green);
            Rect(meterX + 10, 48 + 202 * (1 - (float)endurance.Meter), 36, 202 * (float)endurance.Meter, gold, 3);
            Label(24, 120, "KEEP IT", white, 20); Label(24, 150, "IN THE GREEN", white, 20);
            Label(width / 2 + 70, 134, "SPACE / ENTER", gold, 20);
        }
        ImGui.InvisibleButton("trainingStage"u8, new(width, height));
        Buttons["trainingStage"] = new(origin.X + width / 2, origin.Y + height / 2);
        for (int pad = 0; pad < 4; pad++) Buttons["trainingPad/" + pad] = new(origin.X + width / 2 - 54 + pad % 2 * 108, origin.Y + 98 + pad / 2 * 108);
        for (int lane = 0; lane < 2; lane++) Buttons["trainingLane/" + lane] = new(origin.X + width / 2, origin.Y + 94 + lane * 110);
        if (ImGui.IsItemActivated())
        {
            var mouse = ImGui.GetMousePos(); float x = mouse.X - origin.X, y = mouse.Y - origin.Y;
            if (game is MemoryTraining)
            {
                float gridX = x - (width / 2 - 102), gridY = y - 50;
                if (gridX is >= 0 and < 204 && gridY is >= 0 and < 204 && gridX % 108 < 96 && gridY % 108 < 96)
                    TrainingInput((int)(gridY / 108) * 2 + (int)(gridX / 108));
            }
            else TrainingInput(game is BeatTraining ? y < 149 ? 0 : 1 : 0);
        }
        foreach (var (key, value) in new (ImGuiKey, string)[] { (ImGuiKey.Space, " "), (ImGuiKey.Enter, "Enter"), (ImGuiKey.F, "f"), (ImGuiKey.UpArrow, "ArrowUp"), (ImGuiKey.W, "w"), (ImGuiKey.J, "j"), (ImGuiKey.DownArrow, "ArrowDown"), (ImGuiKey.S, "s"), (ImGuiKey._1, "1"), (ImGuiKey._2, "2"), (ImGuiKey._3, "3"), (ImGuiKey._4, "4") })
            if (ImGui.IsKeyPressed(key, false) && game.Key(value) is { } input) TrainingInput(input);
        ImGui.Text(game.Left + "     " + game.Right);
    }
}
