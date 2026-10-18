using System.Text.Json;
using Brutal.ImGuiApi;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showEmotes, muteEmotes;
    private MatchPlanningSession? emoteSession;
    private MatchEmotes emotes = new();
    private readonly EmoteArtwork emoteArtwork = new();
    private AiEmoteReactions? aiEmotes;
    internal Func<double>? AiEmoteRandom { get; set; }
    internal Func<long> AiEmoteClock { get; set; } = () => Environment.TickCount64;
    internal int AiEmotesShown { get; private set; }
    private readonly List<(AiEmoteReaction Reaction, double At)> pendingAiEmotes = [];
    private void ScheduleAiEmotes()
    {
        EnsureEmotes(); if(Session is null || aiEmotes is null) return;
        var players=Session.State.Players.Select(p=>new AiEmotePlayer(p.Id,p.Name,p.IsHuman,p.Alive)).ToArray();
        var results=Session.Rounds.Results.Select(r=>new AiEmoteResult(r.Match.A,r.Match.B,r.Match.Pve,r.Match.Ghost,r.Match.Winner));
        if(aiEmotes.Choose(players,results,localSeat,AiEmoteRandom) is { } reaction) pendingAiEmotes.Add((reaction,AiEmoteClock()+reaction.Delay));
    }
    private void UpdateAiEmotes()
    {
        EnsureEmotes();long now=Environment.TickCount64;
        for(int i=pendingAiEmotes.Count-1;i>=0;i--)
        {
            var pending=pendingAiEmotes[i];if(AiEmoteClock()<pending.At)continue;
            if(emotes.Receive(pending.Reaction.Seat,pending.Reaction.Id,localSeat,muteEmotes,now))AiEmotesShown++;
            pendingAiEmotes.RemoveAt(i);
        }
    }
    internal int ArenaEmoteDraws { get; private set; }
    private void ArenaEmotes()
    {
        ArenaEmoteDraws = 0;
        if (showEmotes || showSoundboard || showAudio || showPlayers) return;
        EnsureEmotes(); long now = Environment.TickCount64;
        Window("Arena emote overlay", Left, Top, Width, Height, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs);
        void Draw(int seat, ArenaCompanionView? view)
        {
            if (view is null || emotes.At(seat, now) is not { } emote) return;
            var p = System.Numerics.Vector4.Transform(new System.Numerics.Vector4(view.Anchor, 1), BattleView.Camera(Width, Height, battle?.Combat.Columns ?? 7));
            if (p.W <= 0) return;
            var anchor = EmoteLayout.Anchor((p.X / p.W + 1) * Width / 2, (1 - p.Y / p.W) * Height / 2, false, Width, Height);
            if (!anchor.Visible) return;
            emoteArtwork.Draw(emote.Id, now - emote.Started, new(Left + (float)anchor.X - 56, Top + (float)anchor.Y - 112), 112, true, true);
            ArenaEmoteDraws++;
        }
        if (!IsSpectating) Draw(localSeat, arenaCompanion);
        Draw(opponentCompanionSeat, opponentCompanion);
        ImGui.End();
    }
    private void EnsureEmotes()
    {
        if (ReferenceEquals(emoteSession, Session)) return;
        emoteSession = Session; emotes = new(); emotes.Played += () => audio.Play("emote");
        pendingAiEmotes.Clear();aiEmotes=Session is null?null:new(Session.State.Players.Where(p=>p.Alive).Select(p=>p.Id));
        muteEmotes = profile?.Storage.GetValueOrDefault("dt.emotes.muteOthers") == "1";
    }
    private void EmoteRow(int seat)
    {
        EnsureEmotes(); long now = Environment.TickCount64;
        if (emotes.At(seat, now) is not { } emote) return;
        emoteArtwork.Draw(emote.Id, now - emote.Started, ImGui.GetCursorScreenPos(), 52, true); ImGui.Dummy(new(52, 52));
    }
    private void EmotePanel()
    {
        EnsureEmotes(); long now = Environment.TickCount64;
        bool keyboard = !ImGui.GetIO().WantTextInput;
        if (keyboard && ImGui.IsKeyPressed(ImGuiKey.T, false)) { showEmotes = !showEmotes; if (showEmotes) showSoundboard = false; }
        if (!showEmotes) return;
        Window("Emotes", size.X / 2 - 220, Top + 20, 440, 540);
        ImGui.Text("Three emotes per six seconds - T to close"u8);
        try
        {
            var collection = new CosmeticCollection(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            var slots = collection.Equipped("emote")!.AsArray();
            for (int i = 0; i < 6; i++)
            {
                string? id = slots.ElementAtOrDefault(i)?.GetValue<string>();
                if (id is not null) emoteArtwork.Draw(id, now % 2600, ImGui.GetCursorScreenPos(), 60);
                ImGui.Dummy(new(60, 60)); ImGui.SameLine();
                ImGui.BeginDisabled(IsSpectating || id is null);
                if (Button("emote/" + i, $"{i + 1}: {CosmeticCollection.Item("emote", id)?.Name ?? "Empty slot"}", 320, 50) || keyboard && !IsSpectating && ImGui.IsKeyPressed((ImGuiKey)((int)ImGuiKey._1 + i), false))
                {
                    if (id is not null && emotes.Send(collection, id, localSeat, now, networkRoom is null ? null : clip => networkRoom.SendEffect(JsonSerializer.SerializeToElement(new { s = localSeat, kind = "emote", id = clip })))) showEmotes = false;
                    else status = "Too many emotes. Wait a few seconds.";
                }
                ImGui.EndDisabled();
            }
            bool muted = muteEmotes;
            if (ImGui.Checkbox("Mute other players' emotes"u8, ref muted))
            {
                if (profile is null || profile.UpdateStorage(new Dictionary<string, string> { ["dt.emotes.muteOthers"] = muted ? "1" : "0" })) { muteEmotes = muted; if (muted) emotes.ClearOthers(localSeat); }
                else status = profile.Error ?? "Mute preference could not be saved.";
            }
        }
        catch (Exception error) when (error is JsonException or InvalidDataException or InvalidOperationException or IOException or FormatException)
        { ImGui.TextWrapped("Emotes could not be loaded: " + error.Message); }
        if (Button("closeEmotes", "Close")) showEmotes = false;
        ImGui.End();
    }
}
