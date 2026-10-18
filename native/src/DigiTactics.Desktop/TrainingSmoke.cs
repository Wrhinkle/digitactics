using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Plays the visible training controls via ImGui input, then reopens the profile to verify rewards.</summary>
internal sealed class TrainingSmoke(NativeProfileStore profile, string directory) : IDisposable
{
    private readonly string[] games = ["strength", "speed", "wisdom", "stamina", "strength", "strength", "strength"];
    private int index, stage, delay = 3, memoryIndex, previousPad = -1, routeStage;
    private readonly List<int> sequence = [];
    private ImGuiKey? held;
    private bool mouseDown, captured, verified, blocked, socialGuardPending, socialGuardExercised;
    private TrainingGame? socialGuardTraining;
    private string before = "";
    private JsonNode? initialPet;
    private long initialBits;
    private FileStream? guard;
    public string Pending => $"training {index}/{games.Length}, stage {stage}";
    public void Dispose() => guard?.Dispose();
    private bool Click(MatchScreen screen, string key)
    {
        if (!screen.Buttons.TryGetValue(key, out var point)) return false;
        ImGui.GetIO().AddMousePosEvent(point.X, point.Y); ImGui.GetIO().AddMouseButtonEvent(0, true); mouseDown = true;
        return true;
    }
    private void Key(ImGuiKey key) { ImGui.GetIO().AddKeyEvent(key, true); held = key; }
    public void Input(MatchScreen screen)
    {
        if (held is { } key) { ImGui.GetIO().AddKeyEvent(key, false); held = null; return; }
        if (mouseDown) { ImGui.GetIO().AddMouseButtonEvent(0, false); mouseDown = false; delay = 2; return; }
        if (delay-- > 0) return;
        if(index>=games.Length)
        {
            if(routeStage==0 && Click(screen,"farmDigivolve"))routeStage=1;
            else if(routeStage==1 && Click(screen,"closeEvolution"))routeStage=2;
            else if(routeStage==3)
            {
                var point=screen.Buttons["farmDigivolve"];ImGui.GetIO().AddMousePosEvent(point.X,point.Y);
                ImGui.GetIO().AddMouseWheelEvent(0,-20);routeStage=4;delay=3;
            }
            return;
        }
        if (stage == 0) { if (Click(screen, "farm")) stage = 1; return; }
        if (stage == 1)
        {
            if (!screen.Buttons.ContainsKey("farmTrain/" + games[index])) { Click(screen, "farmAction/train"); return; }
            if (Click(screen, "farmTrain/" + games[index])) { stage = 2; captured = verified = blocked = false; sequence.Clear(); memoryIndex = 0; previousPad = -1; }
            return;
        }
        var game = screen.ActiveTraining;
        if (stage == 2)
        {
            if (game is null) return;
            before = File.ReadAllText(Path.Combine(directory, "profile.json"));
            initialPet = JsonNode.Parse(profile.Storage["dt.farm.v1"])!["pets"]!["tokomon"]!.DeepClone(); initialBits = profile.Progress.Bits ?? 0;
            stage = 3;
            if (index == 0 && screen.Buttons.ContainsKey("socialLobby"))
            {
                socialGuardTraining = game;
                socialGuardPending = Click(screen, "socialLobby");
                return;
            }
        }
        if (stage == 3)
        {
            if (socialGuardPending)
            {
                if (!ReferenceEquals(screen.ActiveTraining, socialGuardTraining) || !screen.Buttons.ContainsKey("trainingQuit") || screen.Buttons.ContainsKey("steamBack"))
                    throw new InvalidOperationException("Social Lobbies navigation replaced or hid active training.");
                socialGuardPending = false; socialGuardExercised = true;
            }
            if (screen.Buttons.ContainsKey("retryTraining"))
            {
                if (File.ReadAllText(Path.Combine(directory, "profile.json")) != before) throw new InvalidOperationException("Training partially wrote a locked profile.");
                guard?.Dispose(); guard = null;
                Click(screen, "retryTraining"); return;
            }
            if (screen.Buttons.ContainsKey("trainingDone"))
            {
                if (!verified) return;
                if (Click(screen, "trainingDone")) { stage = 1; index++; }
                return;
            }
            // A missed game must still commit a real score of zero. Quit during Ready
            // and Escape after play starts must instead leave the entire save unchanged.
            if (index == 4) return;
            if (index == 5) { Click(screen, "trainingQuit"); return; }
            if (index == 6) { if (screen.TrainingStarted) Key(ImGuiKey.Escape); return; }
            if (!screen.TrainingStarted) return;
            switch (game)
            {
                case SwingTraining swing:
                    if (swing.Swings == 5 && !blocked) { guard = new FileStream(Path.Combine(directory, "profile.json.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); blocked = true; }
                    if (swing.Ready && Math.Abs(swing.Position - swing.Center) < .035) Key(ImGuiKey.Space);
                    break;
                case BeatTraining beats:
                    var note = beats.Notes.FirstOrDefault(n => !n.Done && Math.Abs(n.At - beats.Time) < .045);
                    if (note is not null) Click(screen, "trainingLane/" + note.Lane);
                    break;
                case MemoryTraining memory:
                    if (memory.Phase == "show")
                    {
                        if (memory.LitPad >= 0 && memory.LitPad != previousPad) sequence.Add(memory.LitPad);
                        previousPad = memory.LitPad;
                    }
                    else if (memory.Phase == "input" && memoryIndex < sequence.Count)
                    {
                        int pad = sequence[memoryIndex];
                        if (memoryIndex % 2 == 0) { if (Click(screen, "trainingPad/" + pad)) memoryIndex++; }
                        else { Key((ImGuiKey)((int)ImGuiKey._1 + pad)); memoryIndex++; }
                    }
                    else if (memory.Phase == "wait") { sequence.Clear(); memoryIndex = 0; previousPad = -1; }
                    break;
                case EnduranceTraining endurance:
                    if (endurance.Meter < .58) Key(ImGuiKey.Space);
                    break;
            }
        }
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if(index>=games.Length)
        {
            if(routeStage<2 || !screen.Buttons.ContainsKey("farmDigivolve"))return false;
            if(screen.FarmSelectedResident!="tokomon" || screen.DisplayedFarmDetails is not {} details || screen.FarmEvolutionBranches!=details.Evolution.Odds.Length || screen.DisplayedEvolutionForm is not null)
                throw new InvalidOperationException("Farm care/evolution route lost its resident, branches or return state.");
            if(routeStage==2){if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"farm-card.bmp"),size.X,size.Y);routeStage=3;return false;}
            if(routeStage<4 || delay>0)return false;
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"farm-care-details.bmp"),size.X,size.Y);
            if (size.X >= 1200 && screen.SocialVisible && !socialGuardExercised)
                throw new InvalidOperationException("Docked social Lobbies click was not exercised during training.");
            Console.WriteLine("Training UI smoke passed: four playable games, keyboard/mouse input, locked-save retry, rewards/restart, completed zero-score rewards, unchanged saves on Quit/Escape, active-training social navigation guard and Farm/Digivolve return.");return true;
        }
        if (stage != 3) return false;
        if (index >= 5 && screen.ActiveTraining is null)
        {
            if (File.ReadAllText(Path.Combine(directory, "profile.json")) != before)
                throw new InvalidOperationException("Cancelling training changed the saved profile: " + (index == 5 ? "Quit" : "Escape"));
            var reopened = new NativeProfileStore(directory);
            foreach (string key in new[] { "dt.farm.v1", "dt.cosmetics.v1", "dt.legendevo.v1" })
                if (reopened.Storage.GetValueOrDefault(key) != profile.Storage.GetValueOrDefault(key))
                    throw new InvalidOperationException("Cancelled training disagreed after restart: " + key);
            if (screen.Buttons.ContainsKey("trainingDone") || screen.Buttons.ContainsKey("retryTraining"))
                throw new InvalidOperationException("Cancelled training displayed a saved result.");
            stage = 1; index++; return false;
        }
        if (!captured && screen.TrainingStarted && capture is not null)
        {
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture, games[index] + (index == 4 ? "-zero" : index == 5 ? "-quit" : index == 6 ? "-escape" : "") + ".bmp"), size.X, size.Y); captured = true;
        }
        if (!verified && screen.Buttons.ContainsKey("trainingDone"))
        {
            if (index >= 5) throw new InvalidOperationException("Cancellation committed a training result.");
            double score = screen.ActiveTraining!.Score!.Value;
            if (index < 4 && score < .7) throw new InvalidOperationException("Training UI failed to play " + games[index] + ": " + score);
            if (index == 4 && score != 0) throw new InvalidOperationException("Missed training did not finish with a real score of zero.");
            var reopened = new NativeProfileStore(directory);
            var pet = JsonNode.Parse(reopened.Storage["dt.farm.v1"])!["pets"]!["tokomon"]!;
            string stat = new[] { "str", "spd", "wis", "sta", "str" }[index];
            double N(JsonNode node, string field) => node[field]!.GetValue<double>();
            int gain = 2 + (int)Math.Floor(8 * score + .5), bits = 3 + (int)Math.Floor(3 * score + .5);
            if (N(pet, "trained") != N(initialPet!, "trained") + 1 || N(pet, stat) != N(initialPet!, stat) + gain || N(pet, "energy") != N(initialPet!, "energy") - 15 || N(pet, "hunger") != N(initialPet!, "hunger") - 8 || reopened.Progress.Bits != initialBits + bits)
                throw new InvalidOperationException("Training result/restart mismatch: " + games[index]);
            if (index == 0 && !blocked) throw new InvalidOperationException("Training retry was not exercised.");
            if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, games[index] + (index == 4 ? "-zero" : "") + "-result.bmp"), size.X, size.Y);
            verified = true;
        }
        return false;
    }
}
