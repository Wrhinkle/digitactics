using DigiTactics.Presentation;
using DigiTactics.Storage;

internal static class SoundboardChecks
{
    public static int Run()
    {
        int checks = 0, played = 0, sent = 0;
        void Check(bool ok, string label) { checks++; if (!ok) throw new InvalidOperationException("Soundboard: " + label); }
        var collection = new CosmeticCollection(null);
        string id = collection.Equipped("sound")![0]!.GetValue<string>();
        var board = new MatchSoundboard(); board.Play += _ => played++;
        Check(!board.Send(collection, 0, -1, 0), "spectators cannot send");
        Check(!board.Send(collection, 6, 0, 0), "only six slots");
        Check(!board.Send(collection, 0, 1, 0, _ => false) && played == 0, "failed send has no local playback");
        Check(board.Send(collection, 0, 1, 0, _ => { sent++; return true; }), "equipped clip sent");
        Check(board.Send(collection, 0, 1, 1), "second clip allowed");
        Check(!board.Send(collection, 0, 1, 2) && board.Cooldown(2) == 4998 && played == 2 && sent == 1, "third clip blocked");
        Check(board.Send(collection, 0, 1, 5000), "source sender deadline boundary");
        Check(!board.Receive(1, id, 1, false, 5001), "local echo ignored");
        Check(!board.Receive(0, id, 1, true, 5001), "remote mute");
        Check(!board.Receive(0, "missing", 1, false, 5001), "unknown clip ignored");
        Check(board.Receive(0, id, 1, false, 5001) && board.Receive(0, id, 1, false, 5002), "remote two clips");
        Check(!board.Receive(0, id, 1, false, 10001) && board.Receive(0, id, 1, false, 10002), "strict source receiver boundary");
        Check(board.Receive(2, id, 1, false, 10002), "independent per-seat limits");
        Check(board.Labels(10003).Count == 2 && board.Labels(12302).Count == 0, "labels replace per player and expire at 2300ms");
        collection.Equip("sound", null, 0);
        Check(!board.Send(collection, 0, 1, 20000), "empty slot cannot send");
        Console.WriteLine($"Soundboard: {checks} equipment, playback, mute, rate and label checks passed.");
        return checks;
    }
}
