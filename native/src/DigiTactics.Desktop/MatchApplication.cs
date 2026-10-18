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
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Digi-Tactics", Size = options.MinimumWindow ? new int2(1100, 760) : new int2(1440, 900) });
            window.SetSizeLimits(new(1100, 760), new(-1, -1));
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            Glfw.SwapInterval(options.Smoke ? 0 : 1);
            var context = ImGui.CreateContext();
            try
            {
                ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
                ImGui.StyleColorsDark(); ImGui.GetIO().IniFilename = default; ImGui.GetStyle().FontSizeBase = 16;
                string font = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf");
                if (File.Exists(font)) ImGui.GetIO().Fonts.AddFontFromFileTTF(font, 17);
                MatchScreen.LoadEvolutionFont();
                MatchScreen.LoadEliminationFonts();
                var style = ImGui.GetStyle(); style.WindowRounding = 8; style.FrameRounding = 5;
                style.WindowPadding = new(12, 10); style.ItemSpacing = new(6, 6);
                style.Colors[(int)ImGuiCol.WindowBg] = new(.045f, .065f, .095f, .98f);
                style.Colors[(int)ImGuiCol.TitleBgActive] = new(.08f, .14f, .2f, 1);
                style.Colors[(int)ImGuiCol.Button] = new(.10f, .22f, .31f, 1);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new(.13f, .38f, .47f, 1);
                style.Colors[(int)ImGuiCol.ButtonActive] = new(.12f, .5f, .48f, 1);
                using var platform = new GlfwBackend(window, GlfwBackend.ClientApi.OpenGL);
                using var audio = new GameAudio();
                audio.EffectsVolume = options.Smoke ? 0 : .8f; audio.MusicVolume = options.Smoke ? 0 : .6f; audio.BoardVolume = options.Smoke ? 0 : .8f;
                string saveDirectory = options.ProfileDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigiTactics", "native");
                var profile = options.Smoke && options.ProfileDirectory is null ? null : new NativeProfileStore(saveDirectory);
                using var preferences = options.Smoke ? null : new AudioSettingsBinding(audio, profile!, saveDirectory);
                using var customMusic = options.Smoke ? null : new CustomMusicController(audio, profile!, options.MusicDecoder ?? Path.Combine(AppContext.BaseDirectory,"media","ffmpeg.exe"));
                using var farmPointer = options.FarmPointerSmoke ? new FarmPointerSmoke(profile!, options.ProfileDirectory!) : null;
                using var renderer = new OpenGlBackend(); using var screen = new MatchScreen(catalog, audio, options.Smoke ? 143u : null, options.Duos, profile);
                var mysteryDriver=options.MysteryLootUiSmoke?new MysteryLootUiSmoke(screen,profile!):null;
                var selectionDriver=options.SetSelectionSmoke?new SetSelectionSmoke(catalog,screen,profile!,audio,options.SteamLibrary!):null;
                using var keyboardDriver=options.MatchKeyboardSmoke?new MatchKeyboardSmoke(screen,profile!,audio):null;
                var eliminationDriver=options.EliminationCinematicSmoke?new EliminationCinematicSmoke():null;
                var postgameDriver=options.PostgameSmoke?new PostgameSmoke(catalog,profile!):null;
                using var profileDriver=options.ProfileUiSmoke?new ProfileUiSmoke(screen,profile!):null;
                using var saveDataDriver=options.SaveDataUiSmoke?new SaveDataUiSmoke(screen,profile!,audio):null;
                using var socialDriver=options.SocialUiSmoke?new SocialUiSmoke(profile!,options.SteamLibrary,options.MinimumWindow):null;
                var clientPreferencesDriver=options.ClientPreferencesSmoke?new ClientPreferencesSmoke(profile!,audio):null;
                using var updateDriver=options.UpdateUiSmoke?new UpdateUiSmoke(screen):null;
                using var musicDriver=options.MusicLibrarySmoke?new MusicLibraryUiSmoke(screen,audio):null;
                screen.SteamLibraryPath = options.SteamLibrary;if(!options.UpdateUiSmoke)screen.UpdateClient=updateClient;
                if(earlySteam is not null)screen.AdoptSteam(earlySteam,borrowed:true);
                screen.SteamStartupError(steamStartupError);
                var driver = options.Smoke && !options.MysteryLootUiSmoke && !options.SetSelectionSmoke && !options.MatchKeyboardSmoke && !options.ClientPreferencesSmoke && !options.SocialUiSmoke && !options.SaveDataUiSmoke && !options.EliminationCinematicSmoke && !options.PostgameSmoke && !options.ProfileUiSmoke && !options.UpdateUiSmoke && !options.MusicLibrarySmoke && !options.AudioVisibilitySmoke && !options.ModeBonusSmoke && !options.TrainingSmoke && !options.FarmPointerSmoke && !options.NetworkMatchSmoke && !options.SteamLobbyUiSmoke ? new MatchScreenSmoke(options.CollectionSmoke ? profile : null, options.EvolutionSmoke) : null;
                var visibilityDriver = options.AudioVisibilitySmoke ? new AudioVisibilitySmoke() : null;
                var modeDriver = options.ModeBonusSmoke ? new ModeBonusSmoke(catalog) : null;
                using var lobbyDriver = options.SteamLobbyUiSmoke ? new SteamLobbyUiSmoke(options.SteamLibrary) : null;
                using var networkDriver = options.NetworkMatchSmoke ? new NetworkMatchSmoke(catalog, screen, options.Duos, options.SpectatorSmoke) : null;
                using var trainer = options.TrainingSmoke ? new TrainingSmoke(profile!, options.ProfileDirectory!) : null;
                var clock = System.Diagnostics.Stopwatch.StartNew(); double last = 0;
                for (int frame = 0; ; frame++)
                {
                    UiFrameLifetime.Begin(); Glfw.PollEvents(); if(window.ShouldClose){window.ShouldClose=false;screen.RequestWindowClose();} if(options.Instance is not null)while(options.Instance.TryTake(out var requestedLobby)){if(window.IsIconified)window.Restore();window.Focus();if(requestedLobby is {} lobby)screen.ReceiveSteamJoin(lobby);} audio.SetHidden(window.IsIconified); renderer.NewFrame(); platform.NewFrame(); driver?.Input(screen); trainer?.Input(screen); farmPointer?.Input(screen); networkDriver?.Input(screen);
                    lobbyDriver?.Input(screen); modeDriver?.Input(screen); musicDriver?.Input(screen);updateDriver?.Input(screen); profileDriver?.Input(screen); postgameDriver?.Input(screen); eliminationDriver?.Input(screen);
                    saveDataDriver?.Input(screen); keyboardDriver?.Input(screen);socialDriver?.Input(screen);
                    clientPreferencesDriver?.Input(screen); selectionDriver?.Input(screen); mysteryDriver?.Input(screen);
                    ImGui.NewFrame(); double now = clock.Elapsed.TotalSeconds;
                    double delta = options.Smoke ? CombatSimulation.Tick : Math.Clamp(now - last, 0, .1);
                    screen.Update(delta, options.Smoke ? delta : now - last); last = now;
                    if (options.Smoke && frame == 1) UiTextChecks.Run(catalog);
                    screen.Draw(window.Size); ImGui.Render();
                    if(screen.RestartForUpdateRequested) {updateDriver?.VerifyRestart(screen);break;}
                    audio.Update();
                    screen.UpdateFileOperationClose();
                    preferences?.Update(delta);
                    var size = window.FramebufferSize;
                    OpenGl.Viewport(0, 0, size.X, size.Y); OpenGl.ClearColor(new float4(.025f, .035f, .06f, 1)); OpenGl.Clear(GlClearBit.Color);
                    screen.RenderEliminationComposite(renderer);
                    screen.Render(size); OpenGl.Viewport(0, 0, size.X, size.Y); renderer.RenderDrawData(ImGui.GetDrawData());
                    updateDriver?.Verify(screen,size,options.Capture);
                    if(eliminationDriver is not null && eliminationDriver.Verify(screen,size,options.Capture))break;
                    if(postgameDriver is not null && postgameDriver.Verify(screen,size,options.Capture))break;
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
                    if (networkDriver is not null && networkDriver.Verify(screen, size, options.Capture)) break;
                    if (lobbyDriver is not null && lobbyDriver.Verify(screen, size, options.Capture)) break;
                    if(screen.ExitAfterFileOperationsReady)break;
                    window.SwapBuffers();
                    if(frame==0 && startupSignal is not null)_=startupSignal.ReportReady();
                    string? phaseTimeout=driver?.Timeout(frame);
                    if (options.Smoke && (phaseTimeout is not null || driver is null && frame > (options.TrainingSmoke ? 6000 : options.PostgameSmoke ? 1800 : 1400)))
                    {
                        if (options.Capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(options.Capture, "failure.bmp"), size.X, size.Y);
                        throw new InvalidOperationException("Match UI smoke did not complete: " + (mysteryDriver?.Step.ToString() ?? postgameDriver?.Pending ?? selectionDriver?.Pending ?? keyboardDriver?.Pending ?? socialDriver?.Pending ?? lobbyDriver?.Pending ?? networkDriver?.Pending ?? farmPointer?.Pending ?? trainer?.Pending ?? driver?.Pending)+(phaseTimeout is null?"":"; "+phaseTimeout));
                    }
                }
            }
            finally { ImGui.DestroyContext(context); }
        }
        finally { Glfw.Terminate(); }
    }
}
