using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private static ImFontPtr evolutionAnnouncementFont;
    private readonly EvolutionAnnouncement evolutionSaid = new();
    private EvolutionAnnouncementFrame evolutionSaidFrame;
    private EvolutionAnnouncementArtwork? evolutionAnnouncementArtwork;
    internal double EvolutionAnnouncementOpacity => evolutionSaidFrame.Opacity;
    internal double EvolutionAnnouncementSlide => evolutionSaidFrame.Slide;
    internal int EvolutionAnnouncementLines { get; private set; }
    internal int EvolutionAnnouncementInkPixels => evolutionAnnouncementArtwork?.InkPixels ?? 0;
    internal int EvolutionAnnouncementShadowPixels => evolutionAnnouncementArtwork?.ShadowPixels ?? 0;

    internal static void LoadEvolutionFont()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeuib.ttf");
        evolutionAnnouncementFont = File.Exists(path) ? ImGui.GetIO().Fonts.AddFontFromFileTTF(path, 15) : default;
    }

    private void DrawEvolutionAnnouncement(float2 at, int width, float height)
    {
        evolutionSaidFrame = evolutionSaid.Sample(boardSceneTime);
        EvolutionAnnouncementLines = 0;
        if (evolutionSaidFrame.Text is not { } text || evolutionSaidFrame.Opacity <= 0) return;
        ImGui.PushFont(evolutionAnnouncementFont, 15);
        try
        {
            evolutionAnnouncementArtwork ??= new();
            evolutionAnnouncementArtwork.Draw(text, at, width, height, evolutionSaidFrame.Opacity, evolutionSaidFrame.Slide);
            EvolutionAnnouncementLines = evolutionAnnouncementArtwork.Lines;
        }
        finally { ImGui.PopFont(); }
    }
}
