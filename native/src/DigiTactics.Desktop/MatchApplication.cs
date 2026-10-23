using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal static class MatchApplication
{
    public static unsafe void Run(ContentCatalog catalog, MatchRunOptions options)
    {
        using var startupSignal=NativeStartupSignal.FromEnvironment();
        var updateClient=NativeUpdateClient.FromEnvironment();
        using var earlySteam=SteamBootstrap.Start(options.SteamLibrary,!options.Smoke,out string? steamStartupError);
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5);
            Glfw.WindowHint(GlfwWindowHint.Visible, options.Smoke && !options.AudioVisibilitySmoke ? 0 : 1);
            var desiredLogical = options.Smoke && options.CaptureSize is { } requestedCapture ? requestedCapture : options.Smoke && options.CompactSmokeWindow ? new int2(1100,600) : options.Smoke && options.FullHdWindow ? new int2(1920,1080) : options.Smoke && options.ReferenceWindow ? new int2(1600,900) : options.Smoke && options.ShortWindow ? new int2(1342,670) : options.MinimumWindow ? NativeUiMetrics.MinimumLogicalSize : options.FarmPointerSmoke ? new int2(1200,800) : new int2(1440, 900);
            // Diagnostic windows keep the requested logical layout while the
            // hidden framebuffer grows to exercise real high-density rendering.
            var initialSize = options.Smoke ? new int2((int)MathF.Round(desiredLogical.X*options.SmokeUiScale),(int)MathF.Round(desiredLogical.Y*options.SmokeUiScale)) : desiredLogical;
            var workOrigin = new int2(0, 0);
            var workArea = new int2(0, 0);
            float monitorScale = 1;
            if (!options.Smoke)
            {
                var monitor = Glfw.PrimaryMonitor;
                if (!monitor.IsNull())
                {
                    monitor.GetMonitorWorkArea(out workOrigin, out workArea);
                    monitorScale = OperatingSystem.IsWindows() ? monitor.ContentScale.X : 1;
                    if (workArea.X > 0 && workArea.Y > 0)
                        initialSize = NativeUiMetrics.WindowSize(desiredLogical, workArea, monitorScale);
                }
            }
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Digi-Tactics", Size = initialSize });
            NativeWindowIcon.Apply(window);
            if (!options.Smoke && workArea.X > 0 && workArea.Y > 0)
            {
                Glfw.PInvoke.GetWindowFrameSize(window, out int left, out int top, out int right, out int bottom);
                var frame = new int4(left, top, right, bottom);
                var clientSize = NativeUiMetrics.WindowSize(desiredLogical, workArea, monitorScale, frame);
                window.Size = clientSize;
                window.SetSizeLimits(NativeUiMetrics.WindowSize(NativeUiMetrics.MinimumLogicalSize, workArea, monitorScale, frame), new(-1, -1));
                window.Position = NativeUiMetrics.WindowPosition(clientSize, workOrigin, workArea, frame);
            }
            else window.SetSizeLimits(options.ShortWindow||options.Smoke&&(options.CompactSmokeWindow||options.CaptureSize is not null)?desiredLogical:NativeUiMetrics.MinimumLogicalSize, new(-1, -1));
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            Glfw.SwapInterval(options.Smoke ? 0 : 1);
            var context = ImGui.CreateContext();
            try
            {
                ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
                ImGui.GetIO().IniFilename = default;
                NativeUiTheme.Initialize();
                MatchScreen.LoadEvolutionFont();
                MatchScreen.LoadEliminationFonts();
                MatchScreen.LoadHudFonts();
                using var platform = new NativeUiPlatform(window, options.Smoke ? options.SmokeUiScale : null);
                using var audio = new GameAudio();
                audio.EffectsVolume = options.Smoke ? 0 : .8f; audio.MusicVolume = options.Smoke ? 0 : .6f; audio.BoardVolume = options.Smoke ? 0 : .8f;
                // Parity captures show the default volumes in Settings, like the muted Electron reference, with playback paused.
                if (options.ParityState is not null) { audio.EffectsVolume = .8f; audio.MusicVolume = .6f; audio.BoardVolume = .8f; audio.SetHidden(true); }
                string saveDirectory = options.ProfileDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigiTactics", "native");
                var profile = options.Smoke && options.ProfileDirectory is null ? null : new NativeProfileStore(saveDirectory);
                using var preferences = options.Smoke ? null : new AudioSettingsBinding(audio, profile!, saveDirectory);
                using var customMusic = options.Smoke ? null : new CustomMusicController(audio, profile!, options.MusicDecoder ?? Path.Combine(AppContext.BaseDirectory,"media","ffmpeg.exe"));
                using var farmPointer = options.FarmPointerSmoke ? new FarmPointerSmoke(profile!, options.ProfileDirectory!) : null;
                var caretakerDriver = options.FarmCaretakerSmoke ? new FarmCaretakerUiSmoke(profile!, options.ProfileDirectory!) : null;
                using var renderer = new OpenGlBackend(); using var uiText = new SubpixelText(); using var screen = new MatchScreen(catalog, audio, options.Smoke ? 143u : null, options.Duos, profile);
                var mysteryDriver=options.MysteryLootUiSmoke?new MysteryLootUiSmoke(screen,profile!):null;
                var selectionDriver=options.SetSelectionSmoke?new SetSelectionSmoke(catalog,screen,profile!,audio,options.SteamLibrary!):null;
                using var keyboardDriver=options.MatchKeyboardSmoke?new MatchKeyboardSmoke(screen,profile!,audio):null;
                var eliminationDriver=options.EliminationCinematicSmoke?new EliminationCinematicSmoke():null;
                var postgameDriver=options.PostgameSmoke?new PostgameSmoke(catalog,profile!):null;
                var unitNoteDriver=options.UnitNoteUiSmoke?new UnitNoteUiSmoke(catalog):null;
                var matchControlsDriver=options.MatchControlsUiSmoke?new MatchControlsUiSmoke(catalog,options.Duos):null;
                var matchCosmeticsDriver=options.MatchCosmeticsSmoke?new MatchCosmeticsSmoke(catalog,profile!):null;
                var legendNavigationDriver=options.LegendNavigationSmoke?new LegendNavigationSmoke(profile!):null;
                var galaxyIntroDriver=options.GalaxyIntroUiSmoke?new GalaxyIntroUiSmoke(catalog):null;
                var choicePresentationDriver=options.ChoicePresentationUiSmoke?new ChoicePresentationUiSmoke(catalog):null;
                var carouselDriver=options.CarouselUiSmoke?new CarouselUiSmoke(catalog):null;
                var loadingDriver=options.LoadingUiSmoke?new LoadingUiSmoke():null;
                var homeDriver=options.HomeUiSmoke?new HomeSmoke(profile):null;
                var castFeedDriver=options.CastFeedUiSmoke?new CastFeedUiSmoke(catalog):null;
                var lootNoticeDriver=options.LootNoticeUiSmoke?new LootNoticeUiSmoke(catalog,profile!):null;
                using var profileDriver=options.ProfileUiSmoke?new ProfileUiSmoke(screen,profile!):null;
                using var saveDataDriver=options.SaveDataUiSmoke?new SaveDataUiSmoke(screen,profile!,audio):null;
                using var socialDriver=options.SocialUiSmoke?new SocialUiSmoke(profile!,options.SteamLibrary,options.MinimumWindow):null;
                var clientPreferencesDriver=options.ClientPreferencesSmoke?new ClientPreferencesSmoke(profile!,audio):null;
                using var updateDriver=options.UpdateUiSmoke?new UpdateUiSmoke(screen):null;
                using var musicDriver=options.MusicLibrarySmoke?new MusicLibraryUiSmoke(screen,audio):null;
                screen.SteamLibraryPath = options.SteamLibrary;if(!options.UpdateUiSmoke)screen.UpdateClient=updateClient;
                if(earlySteam is not null)screen.AdoptSteam(earlySteam,borrowed:true);
                screen.SteamStartupError(steamStartupError);
                var parityDriver = options.ParityState is { } parityState ? new ParityCaptureDriver(parityState, catalog, profile, options.ParitySettleFrames, options.ParityCombatSeconds) : null;
                var driver = options.Smoke && parityDriver is null && !options.MatchControlsUiSmoke && !options.HomeUiSmoke && !options.LoadingUiSmoke && !options.CarouselUiSmoke && !options.ChoicePresentationUiSmoke && !options.GalaxyIntroUiSmoke && !options.LegendNavigationSmoke && !options.LootNoticeUiSmoke && !options.MatchCosmeticsSmoke && !options.CastFeedUiSmoke && !options.UnitNoteUiSmoke && !options.MysteryLootUiSmoke && !options.SetSelectionSmoke && !options.MatchKeyboardSmoke && !options.ClientPreferencesSmoke && !options.SocialUiSmoke && !options.SaveDataUiSmoke && !options.EliminationCinematicSmoke && !options.PostgameSmoke && !options.ProfileUiSmoke && !options.UpdateUiSmoke && !options.MusicLibrarySmoke && !options.AudioVisibilitySmoke && !options.ModeBonusSmoke && !options.TrainingSmoke && !options.FarmPointerSmoke && !options.FarmCaretakerSmoke && !options.NetworkMatchSmoke && !options.SteamLobbyUiSmoke ? new MatchScreenSmoke(options.CollectionSmoke ? profile : null, options.EvolutionSmoke, options.CollectionLayoutSmoke) : null;
                var visibilityDriver = options.AudioVisibilitySmoke ? new AudioVisibilitySmoke() : null;
                var modeDriver = options.ModeBonusSmoke ? new ModeBonusSmoke(catalog) : null;
                using var lobbyDriver = options.SteamLobbyUiSmoke ? new SteamLobbyUiSmoke(options.SteamLibrary,profile) : null;
                using var networkDriver = options.NetworkMatchSmoke ? new NetworkMatchSmoke(catalog, screen, options.Duos, options.SpectatorSmoke) : null;
                using var trainer = options.TrainingSmoke ? new TrainingSmoke(profile!, options.ProfileDirectory!) : null;
                var clock = System.Diagnostics.Stopwatch.StartNew(); double last = 0;
                for (int frame = 0; ; frame++)
                {
                    UiFrameLifetime.Begin(); Glfw.PollEvents(); if(window.ShouldClose){window.ShouldClose=false;screen.RequestWindowClose();} if(options.Instance is not null)while(options.Instance.TryTake(out var requestedLobby)){if(window.IsIconified)window.Restore();window.Focus();if(requestedLobby is {} lobby)screen.ReceiveSteamJoin(lobby);} audio.SetHidden(window.IsIconified); screen.WindowMinimized = window.IsIconified; renderer.NewFrame(); platform.NewFrame(); driver?.Input(screen); trainer?.Input(screen); farmPointer?.Input(screen); caretakerDriver?.Input(screen); networkDriver?.Input(screen);
                    lobbyDriver?.Input(screen); modeDriver?.Input(screen); musicDriver?.Input(screen);updateDriver?.Input(screen); profileDriver?.Input(screen); postgameDriver?.Input(screen); eliminationDriver?.Input(screen);
                    unitNoteDriver?.Input(screen); matchControlsDriver?.Input(screen); castFeedDriver?.Input(screen); lootNoticeDriver?.Input(screen);
                    matchCosmeticsDriver?.Input(screen);
                    legendNavigationDriver?.Input(screen);
                    galaxyIntroDriver?.Input(screen);
                    choicePresentationDriver?.Input(screen);
                    carouselDriver?.Input(screen); loadingDriver?.Input(screen); homeDriver?.Input(screen);
                    saveDataDriver?.Input(screen); keyboardDriver?.Input(screen);socialDriver?.Input(screen);
                    clientPreferencesDriver?.Input(screen); selectionDriver?.Input(screen); mysteryDriver?.Input(screen); parityDriver?.Input(screen);
                    ImGui.NewFrame(); double now = clock.Elapsed.TotalSeconds;
                    double delta = options.Smoke ? CombatSimulation.Tick : Math.Clamp(now - last, 0, .1);
                    screen.Update(delta, options.Smoke ? delta : now - last); last = now;
                    if (options.Smoke && frame == 1) UiTextChecks.Run(catalog);
                    if(options.Smoke&&frame==1&&options.SmokeUiScale!=1)Console.WriteLine($"Rendered UI scale: requested {options.SmokeUiScale:0.##}, effective {platform.Scale:0.##}, logical {platform.LogicalSize.X}x{platform.LogicalSize.Y}, framebuffer {window.FramebufferSize.X}x{window.FramebufferSize.Y}.");
                    screen.Draw(platform.LogicalSize); ImGui.Render(); platform.CompleteFrame();
                    if(screen.RestartForUpdateRequested) {updateDriver?.VerifyRestart(screen);break;}
                    audio.Update();
                    screen.UpdateFileOperationClose();
                    preferences?.Update(delta);
                    var size = window.FramebufferSize;
                    OpenGl.Viewport(0, 0, size.X, size.Y); OpenGl.ClearColor(new float4(.025f, .035f, .06f, 1)); OpenGl.Clear(GlClearBit.Color);
                    screen.RenderEliminationComposite(renderer);
                    screen.Render(size); OpenGl.Viewport(0, 0, size.X, size.Y); uiText.Render(renderer, ImGui.GetDrawData());
                    updateDriver?.Verify(screen,size,options.Capture);
                    if(eliminationDriver is not null && eliminationDriver.Verify(screen,size,options.Capture))break;
                    if(postgameDriver is not null && postgameDriver.Verify(screen,size,options.Capture))break;
                    if(unitNoteDriver is not null && unitNoteDriver.Verify(screen,size,options.Capture))break;
                    if(matchControlsDriver is not null && matchControlsDriver.Verify(screen,size,options.Capture))break;
                    if(matchCosmeticsDriver is not null && matchCosmeticsDriver.Verify(screen,size,options.Capture))break;
                    if(legendNavigationDriver is not null && legendNavigationDriver.Verify(screen,size,options.Capture))break;
                    if(galaxyIntroDriver is not null && galaxyIntroDriver.Verify(screen,size,options.Capture))break;
                    if(choicePresentationDriver is not null && choicePresentationDriver.Verify(screen,size,options.Capture))break;
                    if(carouselDriver is not null && carouselDriver.Verify(screen,size,options.Capture))break;
                    if(loadingDriver is not null && loadingDriver.Verify(screen,size,options.Capture))break;
                    if(homeDriver is not null) {homeDriver.Verify(screen,size,options.Capture);if(homeDriver.Done)break;}
                    if(castFeedDriver is not null && castFeedDriver.Verify(screen,size,options.Capture))break;
                    if(lootNoticeDriver is not null && lootNoticeDriver.Verify(screen,size,options.Capture))break;
                    if(profileDriver is not null && profileDriver.Verify(screen,size,options.Capture))break;
                    if(saveDataDriver is not null && saveDataDriver.Verify(screen,size,options.Capture))break;
                    if(socialDriver is not null && socialDriver.Verify(screen,size,options.Capture))break;
                    if(clientPreferencesDriver is not null && clientPreferencesDriver.Verify(screen,size,options.Capture))break;
                    if(mysteryDriver is not null && mysteryDriver.Verify(screen,size,options.Capture))break;
                    if(selectionDriver is not null && selectionDriver.Verify(screen,size,options.Capture))break;
                    if(keyboardDriver is not null && keyboardDriver.Verify(screen,size,options.Capture))break;
                    if (visibilityDriver is not null && visibilityDriver.Verify(window, audio)) break;
                    if (modeDriver is not null && modeDriver.Verify(screen, size, options.Capture)) break;
                    if (musicDriver is not null && musicDriver.Verify(size,options.Capture)) break;
                    if (driver is not null && driver.Verify(screen, size, options.Capture))
                    {Console.WriteLine($"Match UI smoke completed in {frame+1} frames.");break;}
                    if (trainer is not null && trainer.Verify(screen, size, options.Capture)) break;
                    if (farmPointer is not null && farmPointer.Verify(screen, size, options.Capture)) break;
                    if (caretakerDriver is not null && caretakerDriver.Verify(screen, size, options.Capture)) break;
                    if (networkDriver is not null && networkDriver.Verify(screen, size, options.Capture)) break;
                    if (lobbyDriver is not null && lobbyDriver.Verify(screen, size, options.Capture)) break;
                    if (parityDriver is not null && parityDriver.Verify(screen, size, options.Capture)) break;
                    if(screen.ExitAfterFileOperationsReady)break;
                    window.SwapBuffers();
                    if(frame==0 && startupSignal is not null)_=startupSignal.ReportReady();
                    string? phaseTimeout=driver?.Timeout(frame) ?? parityDriver?.Timeout(20000);
                    if (options.Smoke && (phaseTimeout is not null || driver is null && parityDriver is null && frame > (options.TrainingSmoke || options.CarouselUiSmoke ? 6000 : options.PostgameSmoke ? 1800 : 1400)))
                    {
                        if (options.Capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(options.Capture, "failure.bmp"), size.X, size.Y);
                        throw new InvalidOperationException("Match UI smoke did not complete: " + (updateDriver?.Pending ?? matchControlsDriver?.Pending ?? homeDriver?.Pending ?? loadingDriver?.Pending ?? carouselDriver?.Pending ?? choicePresentationDriver?.Pending ?? galaxyIntroDriver?.Pending ?? lootNoticeDriver?.Pending ?? mysteryDriver?.Step.ToString() ?? legendNavigationDriver?.Pending ?? matchCosmeticsDriver?.Pending ?? unitNoteDriver?.Pending ?? castFeedDriver?.Pending ?? postgameDriver?.Pending ?? selectionDriver?.Pending ?? keyboardDriver?.Pending ?? socialDriver?.Pending ?? lobbyDriver?.Pending ?? networkDriver?.Pending ?? farmPointer?.Pending ?? caretakerDriver?.Pending ?? trainer?.Pending ?? parityDriver?.Pending ?? driver?.Pending)+(phaseTimeout is null?"":"; "+phaseTimeout));
                    }
                }
            }
            finally { ImGui.DestroyContext(context); }
        }
        finally { Glfw.Terminate(); }
    }
}
