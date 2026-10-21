using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // v1.0.14 client.js #cload: eight Tamer cards between the set title and
    // arena entrance line. This is a presentation transition after setup;
    // it never controls simulation, network clocks, readiness or downloads.
    private MatchPlanningSession? loadingSession;
    private string? loadingNetworkKey;
    private double loadingElapsed, loadingDuration;
    private bool loadingMultiplayer;
    internal bool MatchLoadingVisible { get; private set; }
    internal int MatchLoadingCards { get; private set; }
    internal int MatchLoadingPortraits { get; private set; }
    internal int? MatchLoadingLocalSeat { get; private set; }
    internal string? MatchLoadingTitle { get; private set; }
    internal string[] MatchLoadingNames { get; private set; } = [];
    internal (float2 Min, float2 Max) MatchLoadingCardsBounds { get; private set; }
    internal double MatchLoadingElapsed => loadingElapsed;
    /// <summary>For capture drivers: holds the entrance at a fixed time so a still frame is deterministic.</summary>
    internal double? MatchLoadingPin { get; set; }
    private bool MatchEntranceActive => loadingSession is not null && ReferenceEquals(loadingSession, Session) &&
        Session!.State.Over == false && loadingElapsed < loadingDuration;

    private void BeginMatchLoading()
    {
        loadingSession = Session; loadingElapsed = 0;
        loadingMultiplayer = networkRoom is not null;
        loadingDuration = loadingMultiplayer ? 2.3 : 2.6;
    }

    private void BeginNetworkMatchLoading()
    {
        if (networkRoom?.Init is not { } init) return;
        string key = networkRoom.Authority.RoomId + ":" + init.GetRawText();
        // Lockstep rebuilds and seat changes reuse the same initialization.
        // They must not put the entrance cards over an ongoing match again.
        if (loadingNetworkKey == key) return;
        loadingNetworkKey = key;
        BeginMatchLoading();
    }

    private void UpdateMatchLoading(double wallDelta)
    {
        if (!ReferenceEquals(loadingSession, Session) || Session?.State.Over != false)
        { loadingSession = null; MatchLoadingVisible = false; return; }
        loadingElapsed = MatchLoadingPin ?? loadingElapsed + Math.Max(0, wallDelta);
    }

    // #cload (theme.js "match loading screen") in CSS pixels: a centered column of the set heading,
    // eight 9:15 cards and the 2 s loading bar.
    private unsafe void DrawMatchLoading(int2 viewport)
    {
        MatchLoadingVisible = false; MatchLoadingCards = MatchLoadingPortraits = 0;
        MatchLoadingLocalSeat = null; MatchLoadingTitle = null; MatchLoadingNames = [];
        if (!MatchEntranceActive || Session is null) return;
        MatchLoadingVisible = true;
        // dtFade 0.25 s in; .out fades over 0.45 s from 2.1 s (1.8 s online).
        double fadeAt = loadingMultiplayer ? 1.8 : 2.1;
        float opacity = ClientMotion ? (float)Math.Min(Math.Clamp(loadingElapsed / .25, 0, 1),
            1 - Math.Clamp((loadingElapsed - fadeAt) / .45, 0, 1)) : 1;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new float2(0, 0));
        Window("Entering the arena", 0, 0, viewport.X, viewport.Y, ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoFocusOnAppearing);
        ImGui.PopStyleVar();
        var draw = ImGui.GetWindowDrawList(); int firstVertex = draw.VtxBuffer.Count;
        var extent = new float2(viewport.X, viewport.Y);
        draw.AddRectFilled(new(0, 0), extent, NativeUiTheme.Navy0);
        // .ld-bg: cover, opacity .55, saturate(.8) (approximated by a slightly dimmer tint).
        postgameArtwork.Draw("loading-bg", new(0, 0), extent, 140, true);
        LoadingVignette(draw, extent);

        int count = Session.State.Players.Count;
        float cardsWidth = Math.Min(viewport.Y <= 820 ? 1180 : 1500, viewport.X - 48), gap = 10;
        float cardWidth = (cardsWidth - gap * (count - 1)) / count, cardHeight = cardWidth * 15 / 9;
        float headingPx = Math.Clamp(viewport.X * .032f, 30, 52), headingLine = headingPx * 1.35f;
        float headHeight = 14.85f + 4 + headingLine, footHeight = 3 + 8 + 16.875f;
        float totalHeight = headHeight + 22 + cardHeight + 22 + footHeight;
        float top = Math.Max(24, (viewport.Y - totalHeight) * .5f), x = (viewport.X - cardsWidth) * .5f;
        string setId = Session.State.Chaos ? "chaos" : Rules.Id.Split('/')[0];
        string tag = HomeSets.Value.GetValueOrDefault(setId)?.Tag ?? "SET";
        string eyebrow = $"{(loadingMultiplayer ? "Multiplayer" : "Solo vs AI")} · {tag} · {MatchPaceTitle}";
        ClientCss.TextCentered(draw, ClientCss.Face.DisplayBold, 11, new(0, top), viewport.X, ClientCss.Gold2, eyebrow.ToUpperInvariant(), 1.76f, 14.85f);
        MatchLoadingTitle = MatchSetTitle;
        string heading = MatchSetTitle.ToUpperInvariant();
        float headingTop = top + 14.85f + 4, headingWidth = ClientCss.Width(ClientCss.Face.DisplayBold, headingPx, heading, headingPx * .1f);
        // text-shadow: 0 0 24px rgba(200,170,110,.4), 0 3px 0 #000
        ClientCss.Glow(draw, new((viewport.X - headingWidth) * .5f + headingWidth * .1f, headingTop + headingLine * .35f),
            new((viewport.X + headingWidth) * .5f - headingWidth * .1f, headingTop + headingLine * .65f), new ImColor8(200, 170, 110, 60), 24, headingPx * .2f);
        ClientCss.TextCentered(draw, ClientCss.Face.DisplayBold, headingPx, new(0, headingTop + 3), viewport.X, new ImColor8(0, 0, 0), heading, headingPx * .1f, headingLine);
        ClientCss.TextCentered(draw, ClientCss.Face.DisplayBold, headingPx, new(0, headingTop), viewport.X, ClientCss.Gold1, heading, headingPx * .1f, headingLine);
        float cardsY = top + headHeight + 22;
        MatchLoadingCardsBounds = (new(x, cardsY), new(x + cardsWidth, cardsY + cardHeight));
        MatchLoadingNames = Session.State.Players.Select(p => p.Name).ToArray();
        const float nameHeight = 51.8f;
        for (int i = 0; i < count; i++)
        {
            var player = Session.State.Players[i]; bool local = player.Id == localSeat;
            var at = new float2(x + i * (cardWidth + gap), cardsY);
            var max = at + new float2(cardWidth, cardHeight);
            // .ld-card: 0 10px 30px rgba(0,0,0,.6); the local card adds a gold ring and glow.
            ClientCss.Glow(draw, at + new float2(0, 10), max + new float2(0, 10), new ImColor8(0, 0, 0, 120), 30);
            if (local) ClientCss.Glow(draw, at, max, new ImColor8(240, 210, 140, 90), 30);
            ClientCss.Gradient(draw, at, max, new ImColor8(11, 26, 44), NativeUiTheme.Navy0);
            var glow = at + new float2(cardWidth * .5f, cardHeight * .35f); float radius = MathF.Sqrt(cardWidth * cardWidth + cardHeight * cardHeight) * .5f * .6f;
            draw.PushClipRect(at, max, true);
            for (int ring = 10; ring >= 1; ring--) draw.AddCircleFilled(glow, radius * ring / 10, new ImColor8(63, 214, 255, (byte)(56f / 10 * (1 - (ring - 1) / 10f) * 1.5f)), 48);
            draw.PopClipRect();
            float picHeight = cardHeight - 2 - nameHeight, portraitSize = Math.Min(cardWidth - 2 - 16, picHeight - 16);
            var portraitAt = at + new float2((cardWidth - portraitSize) * .5f, 1 + (picHeight - portraitSize) * .5f);
            if (portraits.DrawCompanion(ResultCompanion(player), portraitAt, portraitSize)) MatchLoadingPortraits++;
            else ClientCss.TextCentered(draw, ClientCss.Face.DisplayBold, 28, portraitAt + new float2(0, portraitSize * .5f - 18), portraitSize, ClientCss.Gold1, LoadingInitials(player.Name), 0, 36.4f);
            var nameAt = new float2(at.X + 1, max.Y - 1 - nameHeight);
            ClientCss.Gradient(draw, nameAt, max - new float2(1), new ImColor8(1, 10, 19, 153), NativeUiTheme.Navy0);
            draw.AddLine(nameAt + new float2(0, .5f), new(max.X - 1, nameAt.Y + .5f), ClientCss.Gold5);
            string name = ClientCss.Ellipsis(ClientCss.Face.DisplayBold, 14, player.Name.ToUpperInvariant(), cardWidth - 14, .56f);
            ClientCss.TextCentered(draw, ClientCss.Face.DisplayBold, 14, nameAt + new float2(0, 9), cardWidth - 2, ClientCss.Gold1, name, .56f, 18);
            ClientCss.TextCentered(draw, ClientCss.Face.Body, 11, nameAt + new float2(0, 27), cardWidth - 2, ClientCss.LcMute, local ? "You" : player.IsHuman ? "Tamer" : "AI Tamer", 0, 14.85f);
            if (local) { draw.AddRect(at - new float2(1), max + new float2(1), ClientCss.Gold2); MatchLoadingLocalSeat = player.Id; }
            ClientCss.Border(draw, at, max, local ? ClientCss.Gold1 : player.IsHuman ? ClientCss.Accent : ClientCss.Gold4);
            MatchLoadingCards++;
        }
        // .ld-foot: 320x3 bar filling over 2 s (ease-out), then the caption.
        float lineY = cardsY + cardHeight + 22, lineWidth = Math.Min(320, cardsWidth);
        var lineAt = new float2((viewport.X - lineWidth) * .5f, lineY);
        draw.AddRectFilled(lineAt, lineAt + new float2(lineWidth, 3), new ImColor8(1, 10, 19, 217));
        float t = ClientMotion ? (float)Math.Clamp(loadingElapsed / 2, 0, 1) : 1, fill = LoadingEaseOut(t) * (lineWidth - 2);
        if (fill > 0) ClientCss.GradientX(draw, lineAt + new float2(1, 1), lineAt + new float2(1 + fill, 2), ClientCss.Gold3, ClientCss.Accent);
        ClientCss.Border(draw, lineAt, lineAt + new float2(lineWidth, 3), ClientCss.Gold5);
        ClientCss.TextCentered(draw, ClientCss.Face.Body, 12.5f, new(0, lineY + 3 + 8), viewport.X, ClientCss.LcMute, "Loading the arena", .75f, 16.875f);
        if (opacity < 1)
            for (int i = firstVertex; i < draw.VtxBuffer.Count; i++)
            {
                ref var vertex = ref draw.VtxBuffer.Data[i];
                vertex.col = (vertex.col & 0x00ffffffu) | ((uint)Math.Round((vertex.col >> 24) * opacity) << 24);
            }
        ImGui.End();
    }

    // CSS "ease-out": cubic-bezier(0, 0, .58, 1), solved for x by bisection.
    private static float LoadingEaseOut(float x)
    {
        float lo = 0, hi = 1, s = x;
        for (int i = 0; i < 24; i++)
        {
            s = (lo + hi) * .5f; float bx = 3 * (1 - s) * s * s * .58f + s * s * s;
            if (bx < x) lo = s; else hi = s;
        }
        return 3 * (1 - s) * (1 - s) * s * 0 + 3 * (1 - s) * s * s + s * s * s;
    }

    private static string LoadingInitials(string name) => string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(n => char.ToUpperInvariant(n[0])));
    private static void LoadingVignette(ImDrawListPtr draw, float2 extent)
    {
        ImColor8 At(float x, float y)
        {
            double dx = (x / extent.X - .5) * 2, dy = (y / extent.Y - .45) * 2;
            double amount = Math.Clamp((Math.Sqrt(dx * dx + dy * dy) - .3) / .75, 0, 1);
            return new(1, 10, 19, (byte)(amount * 217));
        }
        for (int y = 0; y < 16; y++) for (int x = 0; x < 24; x++)
        {
            var min = new float2(extent.X * x / 24, extent.Y * y / 16);
            var max = new float2(extent.X * (x + 1) / 24, extent.Y * (y + 1) / 16);
            draw.AddRectFilledMultiColor(min, max, At(min.X, min.Y), At(max.X, min.Y), At(max.X, max.Y), At(min.X, max.Y));
        }
    }
}
