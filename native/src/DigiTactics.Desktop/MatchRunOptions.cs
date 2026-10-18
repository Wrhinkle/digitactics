using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Named launch settings keep normal play and diagnostic drivers explicit at the call site.</summary>
internal sealed record MatchRunOptions
{
    public bool Smoke { get; init; }
    public string? Capture { get; init; }
    public bool Duos { get; init; }
    public string? ProfileDirectory { get; init; }
    public bool CollectionSmoke { get; init; }
    public bool CollectionLayoutSmoke { get; init; }
    public bool EvolutionSmoke { get; init; }
    public bool TrainingSmoke { get; init; }
    public bool FarmPointerSmoke { get; init; }
    public bool FarmCaretakerSmoke { get; init; }
    public bool NetworkMatchSmoke { get; init; }
    public string? SteamLibrary { get; init; }
    public bool SteamLobbyUiSmoke { get; init; }
    public bool SpectatorSmoke { get; init; }
    public bool ModeBonusSmoke { get; init; }
    public bool AudioVisibilitySmoke { get; init; }
    public string? MusicDecoder { get; init; }
    public bool MusicLibrarySmoke { get; init; }
    public bool MinimumWindow { get; init; }
    public bool ShortWindow { get; init; }
    public bool UpdateUiSmoke { get; init; }
    public bool ProfileUiSmoke { get; init; }
    public bool PostgameSmoke { get; init; }
    public bool UnitNoteUiSmoke { get; init; }
    public bool MatchCosmeticsSmoke { get; init; }
    public bool LegendNavigationSmoke { get; init; }
    public bool GalaxyIntroUiSmoke { get; init; }
    public bool ChoicePresentationUiSmoke { get; init; }
    public bool CarouselUiSmoke { get; init; }
    public bool CastFeedUiSmoke { get; init; }
    public bool LootNoticeUiSmoke { get; init; }
    public bool EliminationCinematicSmoke { get; init; }
    public bool SaveDataUiSmoke { get; init; }
    public NativeGameInstance? Instance { get; init; }
    public bool SocialUiSmoke { get; init; }
    public bool ClientPreferencesSmoke { get; init; }
    public bool MysteryLootUiSmoke { get; init; }
    public bool SetSelectionSmoke { get; init; }
    public bool MatchKeyboardSmoke { get; init; }
}
