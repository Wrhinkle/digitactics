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
        loadingElapsed += Math.Max(0, wallDelta);
    }

    private unsafe void DrawMatchLoading(int2 viewport)
    {
        MatchLoadingVisible = false; MatchLoadingCards = MatchLoadingPortraits = 0;
        MatchLoadingLocalSeat = null; MatchLoadingTitle = null; MatchLoadingNames = [];
        if (!MatchEntranceActive || Session is null) return;
        MatchLoadingVisible = true;
        float opacity = ClientMotion ? (float)Math.Min(Math.Clamp(loadingElapsed / .25, 0, 1),
            Math.Clamp((loadingDuration - loadingElapsed) / .5, 0, 1)) : 1;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new float2(0, 0));
        Window("Entering the arena", 0, 0, viewport.X, viewport.Y, ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoFocusOnAppearing);
        ImGui.PopStyleVar();
        var draw = ImGui.GetWindowDrawList(); int firstVertex = draw.VtxBuffer.Count;
        var extent = new float2(viewport.X, viewport.Y);
        draw.AddRectFilled(new(0, 0), extent, NativeUiTheme.Navy0);
        postgameArtwork.Draw("loading-bg", new(0, 0), extent, 140, true);
        LoadingVignette(draw, extent);

        int count = Session.State.Players.Count;
        float cardsWidth = Math.Min(1500, viewport.X - 48), gap = 10;
        float cardWidth = (cardsWidth - gap * (count - 1)) / count;
        float cardHeight = cardWidth * 15 / 9;
        float headingSize = Math.Clamp(viewport.X * .032f, 30, 52);
        float headerHeight = headingSize + 35, totalHeight = headerHeight + cardHeight + 75;
        float top = (viewport.Y - totalHeight) * .5f, x = (viewport.X - cardsWidth) * .5f;
        string setId = Session.State.Chaos ? "chaos" : Rules.Id.Split('/')[0];
        string tag = HomeSets.Value.GetValueOrDefault(setId)?.Tag ?? "SET";
        string eyebrow = $"{(loadingMultiplayer ? "Multiplayer" : "Solo vs AI")} · {tag} · {MatchPaceTitle}";
        LoadingCenteredText(draw, viewport.X * .5f, top, cardsWidth, eyebrow.ToUpperInvariant(),
            13, NativeUiTheme.Gold, NativeUiTheme.DisplayBoldFont, 1.2f);
        MatchLoadingTitle = MatchSetTitle;
        LoadingCenteredText(draw, viewport.X * .5f + 2, top + 28, cardsWidth, MatchSetTitle.ToUpperInvariant(),
            headingSize, new ImColor8(0, 0, 0, 220), NativeUiTheme.DisplayBoldFont, headingSize * .1f);
        LoadingCenteredText(draw, viewport.X * .5f, top + 25, cardsWidth, MatchSetTitle.ToUpperInvariant(),
            headingSize, NativeUiTheme.GoldBright, NativeUiTheme.DisplayBoldFont, headingSize * .1f);
        float cardsY = top + headerHeight + 22;
        MatchLoadingCardsBounds = (new(x, cardsY), new(x + cardsWidth, cardsY + cardHeight));
        MatchLoadingNames = Session.State.Players.Select(p => p.Name).ToArray();
        for (int i = 0; i < count; i++)
        {
            var player = Session.State.Players[i]; bool local = player.Id == localSeat;
            var at = new float2(x + i * (cardWidth + gap), cardsY);
            var max = at + new float2(cardWidth, cardHeight);
            for (int shadow = 12; shadow >= 3; shadow -= 3)
                draw.AddRectFilled(at + new float2(-shadow, 7), max + new float2(shadow, shadow + 7), new ImColor8(0, 0, 0, 12));
            draw.AddRectFilledMultiColor(at, max, new ImColor8(14, 39, 55), new ImColor8(14, 39, 55),
                NativeUiTheme.Navy0, NativeUiTheme.Navy0);
            float footerHeight = 54, portraitSize = cardWidth - 16;
            var portraitAt = at + new float2(8, (cardHeight - footerHeight - portraitSize) * .5f);
            if (portraits.DrawCompanion(ResultCompanion(player), portraitAt, portraitSize)) MatchLoadingPortraits++;
            else LoadingCenteredText(draw, at.X + cardWidth * .5f, portraitAt.Y + portraitSize * .4f,
                cardWidth - 16, LoadingInitials(player.Name), 28, NativeUiTheme.GoldBright, NativeUiTheme.DisplayBoldFont);
            float footerY = max.Y - footerHeight;
            draw.AddRectFilled(new(at.X, footerY), max, new ImColor8(1, 10, 19, 235));
            draw.AddLine(new(at.X, footerY), new(max.X, footerY), new ImColor8(70, 55, 20));
            LoadingCenteredText(draw, at.X + cardWidth * .5f, footerY + 8, cardWidth - 12,
                player.Name.ToUpperInvariant(), 15, NativeUiTheme.GoldBright, NativeUiTheme.DisplayBoldFont);
            LoadingCenteredText(draw, at.X + cardWidth * .5f, footerY + 30, cardWidth - 12,
                local ? "You" : player.IsHuman ? "Tamer" : "AI Tamer", 12, NativeUiTheme.Muted, NativeUiTheme.BodyFont);
            draw.AddRect(at, max, local ? NativeUiTheme.GoldBright : player.IsHuman ? new ImColor8(63, 214, 255) : NativeUiTheme.GoldBorder);
            if (local) { draw.AddRect(at - new float2(1, 1), max + new float2(1, 1), NativeUiTheme.Gold); MatchLoadingLocalSeat = player.Id; }
            MatchLoadingCards++;
        }
        float lineY = cardsY + cardHeight + 22;
        float lineWidth = Math.Min(320, cardsWidth);
        var lineAt = new float2((viewport.X - lineWidth) * .5f, lineY);
        draw.AddRectFilled(lineAt, lineAt + new float2(lineWidth, 3), new ImColor8(70, 55, 20));
        // A traveling highlight communicates an entrance animation, never a
        // fabricated completion percentage for work that already finished.
        float sweep = ClientMotion ? (float)(loadingElapsed % 1.2 / 1.2) : .5f;
        float highlight = 64, sweepX = lineAt.X + sweep * (lineWidth - highlight);
        draw.AddRectFilledMultiColor(new(sweepX, lineY + 1), new(sweepX + highlight, lineY + 2),
            NativeUiTheme.GoldBorder, new ImColor8(63, 214, 255), new ImColor8(63, 214, 255), NativeUiTheme.GoldBorder);
        LoadingCenteredText(draw, viewport.X * .5f, lineY + 11, cardsWidth, "Entering the arena",
            14, NativeUiTheme.Muted, NativeUiTheme.BodyFont, .7f);
        if (opacity < 1)
            for (int i = firstVertex; i < draw.VtxBuffer.Count; i++)
            {
                ref var vertex = ref draw.VtxBuffer.Data[i];
                vertex.col = (vertex.col & 0x00ffffffu) | ((uint)Math.Round((vertex.col >> 24) * opacity) << 24);
            }
        ImGui.End();
    }

    private static string LoadingInitials(string name) => string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(n => char.ToUpperInvariant(n[0])));

    private static void LoadingCenteredText(ImDrawListPtr draw, float centerX, float y, float width, string value,
        float fontSize, ImColor8 color, ImFontPtr font, float spacing = 0)
    {
        // The native rasterizer sizes the full ascent/descent box; CSS uses
        // the em square. Match the original visible glyph size on these cards.
        ImGui.PushFont(font, fontSize * 1.25f);
        float Measure(string text) => ImGui.CalcTextSize(text).X + Math.Max(0, text.Length - 1) * spacing;
        if (Measure(value) > width)
        {
            while (value.Length > 0 && Measure(value + "...") > width) value = value[..^1];
            value += "...";
        }
        float x = centerX - Measure(value) * .5f;
        if (spacing == 0) draw.AddText(new(x, y), color, value);
        else foreach (char letter in value)
        {
            string glyph = letter.ToString(); draw.AddText(new(x, y), color, glyph);
            x += ImGui.CalcTextSize(glyph).X + spacing;
        }
        ImGui.PopFont();
    }

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
