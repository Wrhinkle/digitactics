using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal int CarouselCardsDrawn { get; private set; }

    private void CarouselChoices()
    {
        var carousel = Session!.Units.Carousel;
        bool myTurn = carousel.Turn == Human;
        int columns = Math.Max(1, (Width - 32) / 175);
        int rows = (carousel.Options.Count + columns - 1) / columns;
        Window("Carousel", Left, Top + 12, Width, Math.Min(Height - 24, 75 + rows * 158));
        ImGui.TextWrapped(myTurn ? "Your pick: one Digimon and its item." : carousel.Done ? "Everyone has picked." : $"Waiting for {carousel.Turn!.Name}...");
        ImGui.TextWrapped(Session.Rounds.Position.Stage == 1 ? "Pick order is random." : "Lowest HP picks first.");
        float width = (Width - 24f - (columns - 1) * 8) / columns;
        for (int i = 0; i < carousel.Options.Count; i++)
        {
            if (i % columns != 0) ImGui.SameLine();
            var option = carousel.Options[i];
            var line = Rules.Lines.Single(l => l.Id == option.Line);
            bool taken = option.PickedBy is not null;
            ImGui.BeginDisabled(taken || !myTurn);
            bool pick = Button("carousel/" + i, "", width, 150);
            var min = ImGui.GetItemRectMin(); var max = ImGui.GetItemRectMax();
            bool hovered = ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled);
            var draw = ImGui.GetWindowDrawList();
            var ink = taken ? new ImColor8(140, 150, 165) : new ImColor8(235, 240, 248);
            draw.PushClipRect(min, max, true);
            draw.AddRectFilled(min, new(max.X, min.Y + 3), tierColors[Math.Clamp(line.Tier, 0, 5)]);
            portraits.Draw(Rules.Id, option.Line, 0, new(min.X + (width - 64) / 2, min.Y + 5), 64, taken ? (byte)100 : (byte)255);
            void Text(float y, string value, ImColor8 color) => draw.AddText(ImGui.GetFont(), 12, new(min.X + 6, min.Y + y), color, value);
            Text(70, line.Forms[0], ink);
            Text(87, $"{line.Tier}-cost / {string.Join(", ", line.Traits.Select(Name))}", ink);
            if (option.Item is { } item)
            {
                itemArtwork.Draw(Rules.Id, item, new(min.X + 5, min.Y + 104), 22);
                draw.AddText(ImGui.GetFont(), 12, new(min.X + 32, min.Y + 109), ink, Name(item));
            }
            if (taken) Text(132, "Taken by " + option.PickedBy, new ImColor8(220, 190, 110));
            draw.PopClipRect();
            ImGui.EndDisabled();
            CarouselCardsDrawn++;
            if (hovered)
            {
                ImGui.SetNextWindowSize(new(340, 0)); ImGui.BeginTooltip(); UnitDetails(option.Line, 0);
                if (option.Item is { } tooltipItem) { var description = text.Get(Rules.Id, tooltipItem); ImGui.Separator(); ImGui.Text(description.Name); ImGui.TextWrapped(description.Description); }
                if (taken) ImGui.TextWrapped("Taken by " + option.PickedBy);
                ImGui.EndTooltip();
            }
            if (pick) { Act("carousel", new { i }); break; }
        }
        ImGui.End();
    }
}
