using DigiTactics.Rules;
using DigiTactics.Storage;
using System.Numerics;
using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private ArenaCompanionView? arenaCompanion;
    private ArenaCompanionView? opponentCompanion;
    private readonly Dictionary<(int Seat, string Name), System.Text.Json.Nodes.JsonObject> aiLooks = [];
    private int opponentCompanionSeat = -1;
    internal int OpponentCompanionDrawCalls => opponentCompanion?.DrawCalls ?? 0;
    private void UpdateOpponentCompanion(double delta)
    {
        int seat = Session!.Rounds.Phase != MatchPhase.Planning && watching is { Pve: false } && battle is not null
            ? battle.Mirrored ? watching.A : watching.B ?? -1 : -1;
        if (seat < 0) { opponentCompanion?.Dispose(); opponentCompanion = null; opponentCompanionSeat = -1; return; }
        var player = Session.State.Players[seat];
        System.Text.Json.Nodes.JsonObject? look = null;
        if (player.IsHuman && networkRoom is not null)
        {
            string? uid = networkRoom.Authority.Lobby?["slots"]?[seat.ToString()]?["uid"]?.GetValue<string>();
            if (uid is not null) look = networkRoom.Authority.Presence[uid]?["look"] as System.Text.Json.Nodes.JsonObject;
        }
        if (look is null)
        {
            if (aiLooks.Count > 64) aiLooks.Clear();
            if (!aiLooks.TryGetValue((seat, player.Name), out look)) aiLooks[(seat, player.Name)] = look = CosmeticLoadouts.Ai(seat, player.Name);
        }
        string? key = CosmeticLoadouts.CompanionKey(look);
        if (key is null) { opponentCompanion?.Dispose(); opponentCompanion = null; opponentCompanionSeat = -1; return; }
        if (opponentCompanionSeat != seat) { opponentCompanion?.Dispose(); opponentCompanion = new(true); opponentCompanionSeat = seat; }
        EnsureEmotes(); long now = Environment.TickCount64;
        opponentCompanion!.Update(key, emotes.At(seat, now), now, delta, look["dance"]?.GetValue<string>());
    }
    private MatchPlanningSession? companionSession;
    private string? companionWallet, companionProgress;
    private string companionKey = "tokomon";
    private string companionDance = "floss";
    internal bool CompanionDancing => arenaCompanion?.Dancing == true;
    internal int CompanionDrawCalls => arenaCompanion?.DrawCalls ?? 0;
    internal DigiTactics.Presentation.CompanionTravel? CompanionTravel => arenaCompanion?.Travel;
    private (double X, double Z, long At)? companionClick;
    private void CompanionPointer()
    {
        if (IsSpectating || arenaCompanion is null || !ImGui.IsWindowHovered() || !ImGui.IsMouseClicked(ImGuiMouseButton.Right)) return;
        var pointer = ImGui.GetMousePos();
        float x = (pointer.X - Left) / Width * 2 - 1, y = 1 - (pointer.Y - Top) / Height * 2;
        if (x is < -1 or > 1 || y is < -1 or > 1 || !Matrix4x4.Invert(BattleView.Camera(Width, Height, battle?.Combat.Columns ?? 7), out var inverse)) return;
        var near = Vector4.Transform(new Vector4(x, y, 0, 1), inverse); near /= near.W;
        var far = Vector4.Transform(new Vector4(x, y, 1, 1), inverse); far /= far.W;
        var direction = far - near;
        if (Math.Abs(direction.Y) < 1e-6) return;
        float t = -near.Y / direction.Y; if (t < 0) return;
        var target = near + direction * t;
        if (arenaCompanion.Travel.Move(target.X, target.Z)) companionClick = (arenaCompanion.Travel.TargetX!.Value, arenaCompanion.Travel.TargetZ!.Value, Environment.TickCount64);
    }
    private void CompanionClickMark()
    {
        if (companionClick is not { } click) return;
        double age = (Environment.TickCount64 - click.At) / 450d;
        if (age >= 1) { companionClick = null; return; }
        var camera = BattleView.Camera(Width, Height, battle?.Combat.Columns ?? 7);
        var center = Vector4.Transform(new Vector4((float)click.X, .2f, (float)click.Z, 1), camera);
        if (center.W <= 0) return;
        var draw = ImGui.GetForegroundDrawList();
        draw.AddCircle(new(Left + (center.X / center.W + 1) * Width / 2, Top + (1 - center.Y / center.W) * Height / 2),
            (float)(8 + age * 10), new ImColor8(127, 224, 255, (byte)(230 * (1 - age))), 24, 2);
    }
    private void UpdateArenaCompanion(double delta)
    {
        if (Session is null) return;
        UpdateOpponentCompanion(delta);
        if (IsSpectating) return;
        if (!ReferenceEquals(companionSession, Session))
        {
            arenaCompanion?.Dispose(); arenaCompanion = new(); companionSession = Session;
            companionClick = null;
            companionWallet = companionProgress = null; companionKey = "tokomon";
        }
        string? wallet = profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"), progress = profile?.Storage.GetValueOrDefault("dt.legendevo.v1");
        if (wallet != companionWallet || progress != companionProgress)
        {
            try
            {
                var collection = new CosmeticCollection(wallet);
                companionDance = collection.Equipped("dance")?.GetValue<string>() ?? "floss";
                string id = collection.Equipped("legend")?.GetValue<string>() ?? "tokomon";
                string form = LegendEvolution.Inspect(progress, wallet, profile?.Storage.GetValueOrDefault("dt.farm.v1"), id).Form;
                companionKey = id == form ? id : id + "@" + form;
            }
            catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
            { status = "Companion appearance could not be read. Saved progress was preserved: " + error.Message; }
            companionWallet = wallet; companionProgress = progress;
        }
        EnsureEmotes(); long now = Environment.TickCount64;
        arenaCompanion!.Update(companionKey, emotes.At(localSeat, now), now, delta, companionDance);
    }
}
