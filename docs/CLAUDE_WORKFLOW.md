# Claude workflow: build, capture, compare

Exact commands for the native parity / model work on William's Windows PC
(RTX 2060 SUPER, .NET SDK 10.0.402, MSVC 2022 Build Tools, Node 22, Python 3.13).
Run from the repository root in PowerShell unless noted. All tools run hidden;
the GLFW window in captures is created invisible and never takes focus.

## One-time setup

```powershell
git config core.autocrlf false        # generated fixtures hash LF sources; CRLF checkouts break them
python -m pip install -r native/tools/requirements.txt
# build.ps1 installs playwright@1.56.1 + esbuild into native/artifacts/audio-tools on first run;
# its Chromium (build 1194) needs: node native/artifacts/audio-tools/node_modules/playwright/cli.js install chromium
```

If the checkout was made with `autocrlf=true`, re-check out the tracked files once
after the config change (delete them and `git checkout -- .`).

## Build

```powershell
# Full export + Debug build (first time, or after changing js/ sources or exporters): ~47 min
./native/build.ps1 -Configuration Debug -SkipMedia -BuildOnly

# C# only, after editing native/src: ~30 s
cd native; dotnet build src/DigiTactics.Desktop -c Debug --no-restore -m:1 -nr:false -p:UseSharedCompilation=false; cd ..
# or: ./native/build.ps1 -Configuration Debug -SkipMedia -SkipExport -BuildOnly   (~1-2 min, also builds tests and launcher)
```

`build.ps1` switches (defaults reproduce the original release gate): `-Configuration`,
`-SkipMedia` (no pinned FFmpeg; it needs the exact qualified MSVC compiler, so music decoding is
unavailable here), `-SkipExport` (reuse generated content), `-BuildOnly` (no test/smoke gates).
The exporters rewrite `native/content`, `native/tests/fixtures` and `js/bundle.js`; on this machine
they are deterministic, so a rebuild without source changes leaves `git status` clean.

Executable: `native/src/DigiTactics.Desktop/bin/Debug/net10.0/win-x64/DigiTactics.Desktop.exe`.

## Run the game

```powershell
native/src/DigiTactics.Desktop/bin/Debug/net10.0/win-x64/DigiTactics.Desktop.exe --profile-directory $env:TEMP\dt-dev-profile
```

Always use a throwaway `--profile-directory`; never the real profile under `%LOCALAPPDATA%\DigiTactics\native`.

## Capture native states

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/parity/capture-native.ps1                       # all 22 states x 3 sizes
powershell -NoProfile -ExecutionPolicy Bypass -File tools/parity/capture-native.ps1 -States store,match-combat -Sizes 1600x900
powershell -NoProfile -ExecutionPolicy Bypass -File tools/parity/capture-native.ps1 -Model catalog:CS_Renamon:chr391 -ModelTimes attack=0.3,special=0.6,win=0.5
```

Output: `parity/<state>/<size>/native.png`, logs in `parity/_logs/`, `parity/native-report.json`.
States are listed in `tools/parity/states.json`. Each capture uses a fresh temp profile with
`native/tests/fixtures/collection-profile.json` imported. ~10-15 s per capture (one process each);
all 66 take ~15 min.
Other options: `-SettleFrames 45` (frames after the state is reached), `-CombatSeconds 2`.

Direct form (one capture):

```powershell
DigiTactics.Desktop.exe --parity-capture match-combat --capture-size 1920x1080 --capture out.png --profile-directory <tmp> [--parity-combat-seconds 4]
```

Driver: `native/src/DigiTactics.Desktop/ParityCaptureDriver.cs`. Client routes use the shell's own
`NavigateClient`; match states click the real controls (launch, carousel, Space to fight, Skip, Next).
Postgame uses PostgameSmoke's deterministic victory fixture.

## Capture Electron reference states

```powershell
node tools/parity/capture-electron.cjs                               # all 22 states x 3 sizes (~35 min; software WebGL is slow at 1920)
node tools/parity/capture-electron.cjs --states home,store --sizes 1600x900
node tools/parity/capture-electron.cjs --states home --model agumon --model-times attack=0.35,special=0.5,win=0.5
```

Output: `parity/<state>/<size>/electron.png`, `parity/model-<unitId>/<size>/electron-{portrait,idle,attack,special,win}.png`,
`parity/electron-report.json`. It starts `node serve.cjs` (port 8190, hidden) and headless Chromium from
`../dungeon-delver/node_modules/playwright`. Fresh context per capture, localStorage seeded from the same
collection fixture, seeded `Math.random`, Playwright fake clock, audio muted, fake Firebase/Steam.
Note: Electron model mode takes a unit id (`agumon`); native `-Model` takes a model source key
(`agumon:0`, `catalog:CS_Renamon:chr391`; see `native/content/models.json`).

## Vision check (Codex)

```powershell
node tools/parity/vision-check.cjs --pair store --size 1600x900        # electron.png vs native.png, default question
node tools/parity/vision-check.cjs a.png b.png --question "Is the unit in image 2 T-posing?" --out answer.txt
```

Runs `codex exec --skip-git-repo-check -s read-only --ephemeral -o <tmp> --image=<a> --image=<b> -- "<question>"`
hidden; prints the answer. ~30-90 s per call. Library: `require('./tools/parity/vision-check.cjs').visionCheck([a,b], q)`.

## Model sweep (T-pose / animation checks)

```powershell
native/src/DigiTactics.Desktop/bin/Debug/net10.0/win-x64/DigiTactics.Desktop.exe --model-sweep parity/model-sweep
#   [--sweep-filter renamon | --sweep-filter catalog:CS_Renamon:chr391] [--sweep-start 0 --sweep-count 50] [--sweep-times attack=0.3,special=0.6,win=0.5]
```

~9-12 min for all 937 sources. Per source `NNNN.png` (1280x534): portrait | fitted idle | attack mid |
special mid | win mid, then the live board camera (2.5x crop) at idle and attack mid. `sweep.json`
lists flags per source and summary counts; flag definitions are in the file. Flags:
`t-pose`, `<role>-t-pose`, `bind-pose-idle`, `motionless-idle`, `<role>-height-xR`, `no-idle-clip`,
`no-<role>-role`, `no-portrait`. They are heuristics for triage, not verdicts.
Code: `native/src/DigiTactics.Desktop/ModelSweep.cs`. Older audit modes (`--model-visual-audit`,
`--model-fitted-audit`, `--model-board-audit`) are in `ModelVisualAudit.cs`.

## Tests

```powershell
cd native
dotnet run --project tests/DigiTactics.Tests -c Debug --no-build -- (Get-Location).Path      # CPU suite, ~20 min (Debug)
./test-steam.ps1                                                                               # Steam ABI with fake steam_api (MSVC 2022 Build Tools), ~25 s
dotnet run --project src/DigiTactics.Desktop -c Debug --no-build -- --match-smoke --capture artifacts/match-ui.bmp   # one UI smoke
cd ..
./native/build.ps1 -Configuration Debug -SkipMedia -SkipExport                                 # all gates except FFmpeg ones
```

## Rules

- `parity/` and `native/artifacts/` are git-ignored. Do not commit captures.
- Commit generated `native/content` / fixtures / `js/bundle.js` only together with the source change that produced them.
