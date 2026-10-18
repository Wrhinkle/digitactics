# Run and build from source

This branch prepares the native 2.0.0 release candidate. The C# application under
`native/` uses BRUTAL GLFW, OpenGL and ImGui; browser/Electron source is retained
as the original implementation and as input to reference comparisons and asset
exporters. A successful local build does not publish or approve a release.

## Native Windows candidate

See [the native guide](../native/README.md) for prerequisites and current
acceptance status. Build on Windows x64 with the pinned .NET SDK, MSVC x64 tools,
Node.js, Python and an OpenGL 4.5 capable graphics driver. From the repository root:

```powershell
python -m pip install -r native/tools/requirements.txt
./native/build.ps1 -Publish -NativeVersion 2.0.0
```

The script exports content/reference fixtures, runs checks and creates a fresh
self-contained package under `%LOCALAPPDATA%/DigiTacticsBuild/releases/`.
`-Publish` means a local .NET package; it does not upload to GitHub. Start
`DigiTactics.Launcher.exe` from the printed package directory and keep its `app`
folder beside it. Players need no installed .NET SDK/runtime.

Native profile data is separate from Electron. Use an isolated
`--profile-directory` for development and diagnostics; do not use a real player
profile for smoke tests. Follow [explicit migration and backup steps](ONLINE_AND_UPDATES.md)
when moving a player's save. The normal launcher supervises startup and handles
native updates; running the game executable directly bypasses that work.

For individual .NET commands, work from `native/` so `global.json` applies, and
use `-m:1 -nr:false -p:UseSharedCompilation=false` for serial builds. Source
comparison fixtures are generated from the JavaScript implementation by
`native/tools/export-*.cjs`, then read by `native/tests/DigiTactics.Tests`.
Run GPU/UI checks serially against fresh test profiles. Live Steam checks need
separate acceptance; fake-library tests do not prove service availability.

[Native artifact assembly](../native/tools/NATIVE_RELEASE_ARTIFACTS.md) documents
the full ZIP, update ZIP, `native-latest.json`, checksums and matching audio
source materials. [Audio runtime instructions](../native/tools/MEDIA_AUDIO_RUNTIME.md)
describe the pinned minimal FFmpeg build. Final package, live Steam and source
redistribution checks must be reviewed before publication.

Native forks must change the update repository written to `native-launcher.json`
by `native/tools/publish-native.ps1`; otherwise they check the original public
repository. Protocol compatibility is defined by
`native/src/DigiTactics.Networking/NetworkProtocol.cs`. Keep its source reference
and tests aligned when changing the wire contract. Native manifests and Electron
manifests are deliberately separate.

## Browser reference implementation

Use Node.js 22 or newer. The checked-in bundle runs without installing development packages:

```sh
git clone https://github.com/Wrhinkle/digitactics.git
cd digitactics
node serve.cjs
```

Open `http://localhost:8080`. Use HTTP rather than opening `index.html` directly, because browsers restrict model loading from `file://`. The local browser build supports solo. Its optional Firebase transport requires your own backend and rules; no shared public web backend is configured here. A developer can define `window.DT_FIREBASE_CONFIG` with their Firebase client config before loading the bundle. Configure authentication, database rules and abuse limits before exposing that service.

To rebuild the browser bundle after changing module files:

```sh
npm install --no-save --package-lock=false esbuild
node tools/build.cjs ./node_modules/esbuild
```

## Historical Electron packages

To build the retained Electron desktop implementation on Windows:

```sh
cd desktop-steam
npm ci
npm run dist
```

Outputs are `dist/DigiTactics-Steam/` and `dist/DigiTactics-Steam.exe` at the repository root. Staging includes the game, model inventory and attribution files. Electron's build retains dependency licenses. `stage.cjs` and `audit-package.cjs` reject updater credentials in packages. Never commit tokens, runtime profiles or signing credentials.

## Project layout

- `js/`: game, combat, UI, renderer and networking modules; `js/sets/` contains rulesets.
- `native/`: BRUTAL desktop game, deterministic rules, presentation, storage, Steam networking, launcher and source comparison/UI tests.
- `assets/`, `models/`, `icons/`: illustrations, model/portrait data and app icons.
- `desktop-steam/`: retained Electron shell, native bridge, packaging and updater for 1.x.
- `tests/`: gameplay, browser, native bridge and update checks.
- `docs/credits/`, `CREDITS.md`, `THIRD_PARTY_NOTICES.txt`: provenance and attribution.
- `docs/inspo/`, `docs/sets/`: research and ruleset mappings.
- `media/`, `tools/capture-trailer-cinematic.cjs`, `tools/render-trailer-cinematic.py`: trailer, cover and production recipes.
- `desktop/`: older offline WebView2 wrapper, separate from both the Electron and native editions.

## Browser and Electron checks

```sh
node tests/security.cjs
node tests/duos.cjs
node desktop-steam/audit-package.cjs desktop-steam/payload
node tests/update.cjs desktop-steam/out/win-unpacked/Digi-Tactics.exe
node desktop-steam/smoke.cjs
```

The Electron smoke check loads its packaged game without a visible window and, when Steam is running, creates/leaves a friends-only test lobby. Browser checks need Playwright and its browser installed; for example:

```sh
npm install --no-save --package-lock=false playwright esbuild
npx playwright install chromium
node tests/launch-copy.cjs playwright ./qa/launch-copy
```

Other test scripts document their arguments at the top. Keep generated screenshots and test profiles out of commits. No test result establishes production-scale concurrency or immunity to cheating.

## Historical Electron publishing

These commands produce the Electron `latest.json`/payload channel, not a native
release. Use the native artifact guide above for 2.0.0 candidates.

Change `desktop-steam/update-config.example.json` to your own repository before distributing a fork. An ignored `update-config.json` can override it locally; only `owner` and `repo` are staged. A fork pointed at the original repository will receive the original maintainer's updates.

`publish-update.cjs --full --version <x.y.z> --dry-run` builds full and payload archives under `desktop-steam/out/updates/` without publishing. Use `--esbuild <package-path>` if needed. Inspect and audit the artifacts, commit/tag the matching source, then attach the archives and `latest.json` to the matching release. SHA-256 checksums should cover the downloadable assets. The publisher also supports direct publication using your own authenticated GitHub CLI; credentials remain on the maintainer's machine.

Change `PROTO_VERSION` in `js/net-steam.js` when rules or messages become incompatible. Shell/native dependency changes require updating `SHELL` in `boot.js`, `minShell` in `version.json`, and a full build. Preserve the build's source revision and publish-state hashes for later comparisons. Never bypass a protocol warning without checking compatibility.

This is source-available fan work, not a blanket license for third-party characters or assets. Preserve credits and applicable software licenses; obtain the rights needed for your own distribution.
