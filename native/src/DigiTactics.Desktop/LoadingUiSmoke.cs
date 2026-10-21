using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Networking;

namespace DigiTactics.Desktop;

/// <summary>Real local setup and lockstep initialization drive the original entrance cards.</summary>
internal sealed class LoadingUiSmoke
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private int scenario, frame;
    private NetworkRoom? room;
    private long now = 10000;
    internal string Pending => $"Match entrance {scenario}/{frame}";
    private static void Invoke(MatchScreen screen, string method, params object[] args) =>
        typeof(MatchScreen).GetMethod(method, Private)!.Invoke(screen, args);
    private static void Require(bool value, string message)
    { if (!value) throw new InvalidOperationException("Match entrance UI: " + message); }

    public void Input(MatchScreen screen)
    {
        if (frame == 0)
        {
            ImGui.GetIO().AddMousePosEvent(1, 1);
            if (scenario < 2)
            {
                Invoke(screen, "LeaveMatch");
                Invoke(screen, "SelectSet", scenario == 0 ? "gizmos" : "chaos");
                Invoke(screen, "SelectPace", "normal", false);
                Invoke(screen, "StartLocalMatch", scenario == 1);
            }
            else if (scenario == 2)
            {
                Invoke(screen, "LeaveMatch");
                var lobby = JsonNode.Parse("{\"host\":\"76561198000000123\",\"status\":\"playing\",\"slots\":{\"0\":{\"uid\":\"76561198000000123\",\"name\":\"Loading Host\"},\"1\":{\"uid\":\"76561198000000234\",\"name\":\"Loading Guest\"}}}"u8)!.AsObject();
                lobby["name"] = "Loading test room";
                lobby["hostName"] = "Loading Host";
                room = new("10977524000000123", "76561198000000123", lobby);
                room.Members(["76561198000000123", "76561198000000234"], "76561198000000123", now);
                Require(room.PublishInit(JsonSerializer.SerializeToElement(new
                {
                    seed = 143, at = now, planTime = 30, setId = "gizmos", pace = "normal", format = "solo",
                    slots = Enumerable.Range(0, 8).Select(i => new { name = i == 0 ? "Loading Host" : i == 1 ? "Loading Guest" : "AI Tamer " + i, human = i < 2 }).ToArray()
                })), "Network fixture initialization was rejected.");
                screen.AttachNetworkMatch(room, () => now);
            }
            else if (scenario == 3) screen.AttachNetworkMatch(room!, () => now);
        }
        // Pausing solo must not leave the presentation overlay frozen forever.
        if (scenario == 1 && frame == 18) Invoke(screen, "PauseLocal");
        // Leaving while the entrance is visible cancels it immediately.
        if (scenario == 1 && frame == 40) Invoke(screen, "LeaveMatch");
        now += 33; frame++;
    }

    public bool Verify(MatchScreen screen, int2 viewport, string? capture)
    {
        void Capture(string name)
        { if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, name + ".bmp"), viewport.X, viewport.Y); }
        if (scenario < 3 && frame == 12)
        {
            Require(screen.MatchLoadingVisible, "Match setup did not show the entrance cards.");
            Require(screen.CarouselCardsDrawn == 0 && screen.Buttons.Count == 0,
                "Interactive match controls were drawn over the entrance cards.");
            Require(screen.MatchLoadingCards == 8 && screen.MatchLoadingPortraits == 8,
                $"Expected eight Tamer cards and portraits, got {screen.MatchLoadingCards}/{screen.MatchLoadingPortraits}.");
            Require(screen.MatchLoadingLocalSeat == 0, "Your own seat did not receive the gold frame.");
            Require(screen.MatchLoadingNames.SequenceEqual(screen.Session!.State.Players.Select(p => p.Name)),
                "The cards do not describe the actual match seats.");
            var bounds = screen.MatchLoadingCardsBounds;
            Require(bounds.Min.X >= 24 && bounds.Max.X <= viewport.X - 24 && bounds.Min.Y > 80 && bounds.Max.Y < viewport.Y - 48,
                "The Tamer cards leave the viewport or cover the title/footer.");
            Require(screen.MatchLoadingTitle == (scenario == 1 ? "Chaos: " : "") + screen.Session.State.Rules.Name,
                "The heading does not identify the actual resolved ruleset.");
            Require(!screen.IsPaused, "The entrance overlay paused the match.");
            Capture(scenario == 0 ? "solo" : scenario == 1 ? "chaos-duos" : "multiplayer");
        }
        if (scenario == 1 && frame == 35)
        {
            Require(screen.IsPaused && screen.MatchLoadingElapsed > 1 && screen.MatchLoadingVisible,
                "The presentation timer froze or changed the pause state.");
            Capture("paused-entrance");
        }
        if (scenario == 1 && frame == 42)
        {
            Require(screen.Session is null && !screen.MatchLoadingVisible, "Leave retained the entrance overlay.");
            scenario++; frame = 0; return false;
        }
        if (scenario is 0 or 2 && frame == 68)
            Require(screen.MatchLoadingVisible, "Entrance ended before the original transition duration.");
        if (scenario is 0 or 2 && frame == 82)
        {
            Require(!screen.MatchLoadingVisible && screen.Session is not null, "Entrance did not dismiss itself without replacing the match.");
            Capture(scenario == 0 ? "solo-ready" : "multiplayer-ready");
            scenario++; frame = 0; return false;
        }
        if (scenario == 3 && frame == 4)
        {
            Require(!screen.MatchLoadingVisible && screen.Session is not null,
                "Rebuilding the same network match replayed its entrance.");
            Console.WriteLine("Match entrance UI passed: actual solo/Chaos Duos/network setup, eight portraits and names, own-seat frame, viewport bounds, wall-time transition, pause/leave safety, and no replay on network rebuild.");
            return true;
        }
        return false;
    }
}
