using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private ArenaCompanionView? arenaCompanion;
    private MatchPlanningSession? companionSession;
    private string? companionWallet, companionProgress;
    private string companionKey = "tokomon";
    internal int CompanionDrawCalls => arenaCompanion?.DrawCalls ?? 0;
    private void UpdateArenaCompanion(double delta)
    {
        if (Session is null || IsSpectating) return;
        if (!ReferenceEquals(companionSession, Session))
        {
            arenaCompanion?.Dispose(); arenaCompanion = new(); companionSession = Session;
            companionWallet = companionProgress = null; companionKey = "tokomon";
        }
        string? wallet = profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"), progress = profile?.Storage.GetValueOrDefault("dt.legendevo.v1");
        if (wallet != companionWallet || progress != companionProgress)
        {
            try
            {
                var collection = new CosmeticCollection(wallet);
                string id = collection.Equipped("legend")?.GetValue<string>() ?? "tokomon";
                string form = LegendEvolution.Inspect(progress, wallet, profile?.Storage.GetValueOrDefault("dt.farm.v1"), id).Form;
                companionKey = id == form ? id : id + "@" + form;
            }
            catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
            { status = "Companion appearance could not be read. Saved progress was preserved: " + error.Message; }
            companionWallet = wallet; companionProgress = progress;
        }
        EnsureEmotes(); long now = Environment.TickCount64;
        arenaCompanion!.Update(companionKey, emotes.At(localSeat, now), now, delta);
    }
}
