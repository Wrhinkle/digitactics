using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Exercises visible controls through ImGui mouse events, not direct gameplay mutations.</summary>
internal sealed class MatchScreenSmoke(NativeProfileStore? collectionProfile = null)
{
    private readonly string[] collectionCommands = ["collection", "buyCosmetic/legend/bukamon", "confirmPurchase", "equip/legend/bukamon", "closeCollection"];
    private readonly long initialBits = collectionProfile?.Progress.Bits ?? 0;
    private int collectionIndex;
    private bool collectionVerified;
    private readonly string[] commands = ["start", "carousel", "fight", "next", "buy", "bench/0", "hex/3/4", "bench/1", "hex/2/4", "fight", "finish", "next",
        "fight", "finish", "next", "fight", "finish", "next", "fight", "opponent", "finish", "next", "audioSettings", "closeAudio", "mute", "mute"];
    private int index, cooldown = 3, battleFrames, pvpFrames;
    private bool down, planningCaptured, battleCaptured, pvpCaptured;
    public void Input(MatchScreen screen)
    {
        if (collectionProfile is not null && collectionIndex < collectionCommands.Length)
        {
            if (cooldown-- > 0) return;
            var input = ImGui.GetIO();
            if (down) { input.AddMouseButtonEvent(0, false); down = false; collectionIndex++; cooldown = 3; return; }
            if (!screen.Buttons.TryGetValue(collectionCommands[collectionIndex], out var target)) return;
            input.AddMousePosEvent(target.X, target.Y); input.AddMouseButtonEvent(0, true); down = true; return;
        }
        if (cooldown-- > 0 || index >= commands.Length) return;
        if (index == 10 && battleFrames < 60) return;
        if (index == 20 && pvpFrames < 60) return;
        var io = ImGui.GetIO();
        if (down) { io.AddMouseButtonEvent(0, false); down = false; index++; cooldown = 3; return; }
        string command = commands[index];
        if (command == "carousel") command = "carousel/" + screen.Session!.Units.Carousel.Options.Select((o, i) => (o, i)).First(e => e.o.PickedBy is null).i;
        if (command == "buy") command = "buy/" + screen.Session!.State.Players[0].Shop.Slots.Select((id, i) => (id, i)).First(e => e.id is not null).i;
        if (command == "opponent")
        {
            var match = screen.Session!.Battles[0];
            int player = match.TeamB is { } team ? screen.Session.State.Teams[team].Members[0].Id : match.B!.Value;
            command = "view/" + player;
        }
        if (command == "finish" && screen.Session!.Rounds.Phase == MatchPhase.Result) { index++; return; }
        if (!screen.Buttons.TryGetValue(command, out var point)) return;
        io.AddMousePosEvent(point.X, point.Y); io.AddMouseButtonEvent(0, true); down = true;
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if (collectionProfile is not null && !collectionVerified)
        {
            if (collectionIndex == 4 && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "collection.bmp"), size.X, size.Y);
            if (collectionIndex < collectionCommands.Length) return false;
            var collection = new CosmeticCollection(collectionProfile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            int price = CosmeticCollection.Price("legend", CosmeticCollection.Item("legend", "bukamon")!, DateOnly.FromDateTime(DateTime.Now));
            if (!collection.Owns("legend", "bukamon") || collection.Equipped("legend")?.GetValue<string>() != "bukamon" || collection.Bits != initialBits - price)
                throw new InvalidOperationException("Collection purchase/equipment smoke failed.");
            collectionVerified = true;
            Console.WriteLine("Collection UI smoke passed: purchase confirmation, wallet debit, ownership and equipment persistence.");
        }
        if (index == 0 && cooldown == 1 && capture is not null)
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "menu.bmp"), size.X, size.Y);
        if (index == 9 && cooldown == 1 && !planningCaptured)
        {
            if (screen.Session!.State.Players[0].Roster.Board.Count != 2 || screen.PlanningDrawCalls == 0)
                throw new InvalidOperationException("Match UI buy/place/model smoke failed.");
            if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
            planningCaptured = true;
        }
        if (index == 10)
        {
            battleFrames++;
            if (battleFrames == 45)
            {
                if (screen.BattleDrawCalls == 0) throw new InvalidOperationException("Match UI battle rendered no models.");
                if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "battle.bmp"), size.X, size.Y);
                battleCaptured = true;
            }
        }
        if (index == 20)
        {
            pvpFrames++;
            if (pvpFrames == 45)
            {
                if (!screen.WatchingMirrored || screen.BattleColumns != (screen.Session!.State.Duos ? 14 : 7) || screen.BattleDrawCalls == 0)
                    throw new InvalidOperationException("Match UI mirrored PvP smoke failed.");
                if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "pvp.bmp"), size.X, size.Y);
                pvpCaptured = true;
            }
        }
        if (index < commands.Length || cooldown > 0) return false;
        if (!planningCaptured || !battleCaptured || !pvpCaptured || screen.CompletedRounds != 5 || screen.Session!.Rounds.Phase != MatchPhase.Planning)
            throw new InvalidOperationException("Match UI round transition smoke failed.");
        Console.WriteLine("Match UI smoke passed: menu, carousel, purchase, two placements, 3D planning, PvE, mirrored PvP, five settlements and round advances.");
        return true;
    }
}
