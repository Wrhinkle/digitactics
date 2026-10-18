using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Exercises visible controls through ImGui mouse events, not direct gameplay mutations.</summary>
internal sealed class MatchScreenSmoke(NativeProfileStore? collectionProfile = null, bool evolutionSmoke = false)
{
    private readonly string[] collectionCommands = evolutionSmoke
        ? ["collection", "buyCosmetic/legend/bukamon", "confirmPurchase", "equip/legend/bukamon", "evolution/bukamon", "levelLegend", "levelLegend", "feedLegend", "digivolve", "revertLegend", "digivolve", "closeEvolution", "toggleEggs", "egg/legend", "confirmEgg", "closeCollection"]
        : ["collection", "buyCosmetic/legend/bukamon", "confirmPurchase", "equip/legend/bukamon", "toggleEggs", "egg/legend", "confirmEgg", "closeCollection"];
    private readonly long initialBits = collectionProfile?.Progress.Bits ?? 0;
    private int collectionIndex;
    private bool collectionVerified;
    private readonly string[] farmCommands = evolutionSmoke
        ? ["farm", "farmFeed/meat", "farmPlay", "farmPraise", "farmPet", "farmLights", "farmLights", "farmRoom", "farmOut/tsunomon", "farmIn/tsunomon", "farmOut/motimon", "farmSelect/tsunomon", "farmSwap/motimon", "farmIn/tsunomon", "farmExpand", "farmTheme/sewer", "closeFarm"]
        : ["farm", "farmFeed/meat", "farmPlay", "farmPraise", "farmPet", "farmLights", "farmLights", "closeFarm"];
    private int farmIndex;
    private bool farmVerified;
    private long farmInitialBits;
    public string Pending => collectionProfile is not null && collectionIndex < collectionCommands.Length ? collectionCommands[collectionIndex]
        : collectionProfile is not null && farmIndex < farmCommands.Length ? farmCommands[farmIndex] : commands.ElementAtOrDefault(index) ?? "verification";
    private readonly string[] commands = ["start", "carousel", "fight", "next", "buy", "bench/0", "hex/3/4", "bench/1", "hex/2/4", "fight", "finish", "next",
        "fight", "finish", "next", "fight", "finish", "next", "fight", "opponent", "finish", "next", "audioSettings", "closeAudio", "mute", "mute"];
    private int index, cooldown = 3, battleFrames, pvpFrames;
    private bool down, planningCaptured, battleCaptured, pvpCaptured;
    private long emoteTime;
    public void Input(MatchScreen screen)
    {
        emoteTime+=100;screen.AiEmoteClock=()=>emoteTime;screen.AiEmoteRandom=()=>.01;
        if (collectionProfile is not null && collectionIndex < collectionCommands.Length)
        {
            if (cooldown-- > 0) return;
            var input = ImGui.GetIO();
            if (down) { input.AddMouseButtonEvent(0, false); down = false; collectionIndex++; cooldown = 3; return; }
            if (!screen.Buttons.TryGetValue(collectionCommands[collectionIndex], out var target)) return;
            input.AddMousePosEvent(target.X, target.Y); input.AddMouseButtonEvent(0, true); down = true; return;
        }
        if (collectionProfile is not null && farmIndex < farmCommands.Length)
        {
            if (cooldown-- > 0) return;
            var input = ImGui.GetIO();
            if (down) { input.AddMouseButtonEvent(0, false); down = false; farmIndex++; cooldown = 3; return; }
            if (!screen.Buttons.TryGetValue(farmCommands[farmIndex], out var target)) return;
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
            if (collectionIndex == collectionCommands.Length - 1 && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "egg.bmp"), size.X, size.Y);
            if (evolutionSmoke && collectionIndex == 11 && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "evolution.bmp"), size.X, size.Y);
            if (collectionIndex < collectionCommands.Length) return false;
            var collection = new CosmeticCollection(collectionProfile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            int price = CosmeticCollection.Price("legend", CosmeticCollection.Item("legend", "bukamon")!, DateOnly.FromDateTime(DateTime.Now));
            if (!collection.Owns("legend", "bukamon") || collection.Equipped("legend")?.GetValue<string>() != "bukamon" || collection.Bits != initialBits - price - 250 - (evolutionSmoke ? 200 : 0))
                throw new InvalidOperationException("Collection purchase/equipment smoke failed.");
            var wallet = System.Text.Json.Nodes.JsonNode.Parse(collection.Json)!;
            var legends = System.Text.Json.Nodes.JsonNode.Parse(collectionProfile.Storage.GetValueOrDefault("dt.legendevo.v1", "{\"legends\":{}}"))!;
            if (evolutionSmoke)
            {
                var record = legends["legends"]!["bukamon"]!;
                if (record["xp"]!.GetValue<int>() < 200 || record["form"]!.GetValue<string>() == "bukamon" || record["nonce"]!.GetValue<int>() != 1 || record["care"]!["food"]!.GetValue<int>() != 1 || record["path"]!.AsArray().Count != 1)
                    throw new InvalidOperationException("Evolution actions did not persist the expected level, form, care and nonce.");
                Console.WriteLine("Evolution UI smoke passed: buy levels, feed, digivolve, revert and digivolve again.");
            }
            if (wallet["owned"]!["legend"]!.AsArray().Count < 3 && !legends["legends"]!.AsObject().Any(x => x.Value?["xp"]?.GetValue<int>() > 0))
                throw new InvalidOperationException("Egg granted neither an item nor duplicate Digi-Data.");
            collectionVerified = true;
            farmInitialBits = collection.Bits;
            Console.WriteLine("Collection UI smoke passed: purchase, equipment, egg confirmation, wallet debit and hatch reward persistence.");
        }
        if (collectionProfile is not null && !farmVerified)
        {
            if (farmIndex == 2 && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "farm-feed.bmp"), size.X, size.Y);
            if (farmIndex == farmCommands.Length - 1 && cooldown == 1 && (screen.FarmDrawCalls < 150 || screen.FarmTheme != (evolutionSmoke ? "sewer" : "meadow")))
                throw new InvalidOperationException("Farm scene did not render the selected theme.");
            if (farmIndex == farmCommands.Length - 1 && cooldown == 1 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "farm.bmp"), size.X, size.Y);
            if (farmIndex < farmCommands.Length) return false;
            var save = System.Text.Json.Nodes.JsonNode.Parse(collectionProfile.Storage["dt.farm.v1"])!;
            var pet = save["pets"]!["tokomon"]!;
            if (collectionProfile.Progress.Bits != farmInitialBits + 4 - (evolutionSmoke ? 450 : 0) || Math.Abs(pet["goodBond"]!.GetValue<double>() - .73) > 1e-9 || save["meat"]!["n"]!.GetValue<int>() != 2 || save["lights"]!.GetValue<bool>() != true)
                throw new InvalidOperationException("Farm UI care/reward persistence failed.");
            if (evolutionSmoke && (save["cap"]!.GetValue<int>() != 8 || save["theme"]!.GetValue<string>() != "sewer" || !save["residents"]!.AsArray().Any(x => x?.GetValue<string>() == "tsunomon") || !save["residents"]!.AsArray().Any(x => x?.GetValue<string>() == "motimon")))
                throw new InvalidOperationException("Farm room management persistence failed.");
            farmVerified = true; Console.WriteLine("Farm UI smoke passed: feed, play, praise, pet, lights and saved rewards.");
        }
        if (index == 0 && cooldown == 1 && capture is not null)
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "menu.bmp"), size.X, size.Y);
        if (index == 9 && cooldown == 1 && !planningCaptured)
        {
            if (screen.Session!.State.Players[0].Roster.Board.Count != 2 || screen.PlanningDrawCalls == 0 || screen.BoardDrawCalls <= 75)
                throw new InvalidOperationException("Match UI buy/place/model smoke failed.");
            if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
            planningCaptured = true;
        }
        if (index == 10)
        {
            battleFrames++;
            if (battleFrames == 45)
            {
                if (screen.BattleDrawCalls == 0 || screen.BoardDrawCalls <= (screen.BattleColumns==14?130:75)) throw new InvalidOperationException("Match UI battle did not render units and board scenery.");
                if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "battle.bmp"), size.X, size.Y);
                battleCaptured = true;
            }
        }
        if (index == 20)
        {
            pvpFrames++;
            if (pvpFrames == 45)
            {
                if (!screen.WatchingMirrored || screen.BattleColumns != (screen.Session!.State.Duos ? 14 : 7) || screen.BattleDrawCalls == 0 || screen.OpponentCompanionDrawCalls == 0)
                    throw new InvalidOperationException("Match UI mirrored PvP smoke failed.");
                if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "pvp.bmp"), size.X, size.Y);
                pvpCaptured = true;
            }
        }
        if (index < commands.Length || cooldown > 0) return false;
        if (!planningCaptured || !battleCaptured || !pvpCaptured || screen.CompletedRounds != 5 || screen.Session!.Rounds.Phase != MatchPhase.Planning || screen.AiEmotesShown == 0)
            throw new InvalidOperationException("Match UI round transition smoke failed.");
        Console.WriteLine("Match UI smoke passed: menu, carousel, purchase, two placements, 3D planning, PvE, mirrored PvP, five settlements and round advances.");
        return true;
    }
}
