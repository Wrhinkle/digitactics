# Run and build from source

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

To build Windows desktop packages on Windows:

```sh
cd desktop-steam
npm ci
npm run dist
```

Outputs are `dist/DigiTactics-Steam/` and `dist/DigiTactics-Steam.exe` at the repository root. Staging includes the game, model inventory and attribution files. Electron's build retains dependency licenses. `stage.cjs` and `audit-package.cjs` reject updater credentials in packages. Never commit tokens, runtime profiles or signing credentials.

## Project layout

- `js/`: game, combat, UI, renderer and networking modules; `js/sets/` contains rulesets.
- `assets/`, `models/`, `icons/`: illustrations, model/portrait data and app icons.
- `desktop-steam/`: Electron shell, native bridge, packaging and updater.
- `tests/`: gameplay, browser, native bridge and update checks.
- `docs/credits/`, `CREDITS.md`, `THIRD_PARTY_NOTICES.txt`: provenance and attribution.
- `docs/inspo/`, `docs/sets/`: research and ruleset mappings.
- `media/`, `tools/capture-trailer-cinematic.cjs`, `tools/render-trailer-cinematic.py`: trailer, cover and production recipes.
- `desktop/`: older offline WebView2 wrapper; the public Windows release uses Electron.

## Useful checks

```sh
node tests/security.cjs
node tests/duos.cjs
node desktop-steam/audit-package.cjs desktop-steam/payload
node tests/update.cjs desktop-steam/out/win-unpacked/Digi-Tactics.exe
node desktop-steam/smoke.cjs
```

The native smoke check loads the packaged game without a visible window and, when Steam is running, creates/leaves a friends-only test lobby. Browser checks need Playwright and its browser installed; for example:

```sh
npm install --no-save --package-lock=false playwright esbuild
npx playwright install chromium
node tests/launch-copy.cjs playwright ./qa/launch-copy
```

Other test scripts document their arguments at the top. Keep generated screenshots and test profiles out of commits. No test result establishes production-scale concurrency or immunity to cheating.

## Publishing a fork or update

Change `desktop-steam/update-config.example.json` to your own repository before distributing a fork. An ignored `update-config.json` can override it locally; only `owner` and `repo` are staged. A fork pointed at the original repository will receive the original maintainer's updates.

`publish-update.cjs --full --version <x.y.z> --dry-run` builds full and payload archives under `desktop-steam/out/updates/` without publishing. Use `--esbuild <package-path>` if needed. Inspect and audit the artifacts, commit/tag the matching source, then attach the archives and `latest.json` to the matching release. SHA-256 checksums should cover the downloadable assets. The publisher also supports direct publication using your own authenticated GitHub CLI; credentials remain on the maintainer's machine.

Change `PROTO_VERSION` in `js/net-steam.js` when rules or messages become incompatible. Shell/native dependency changes require updating `SHELL` in `boot.js`, `minShell` in `version.json`, and a full build. Preserve the build's source revision and publish-state hashes for later comparisons. Never bypass a protocol warning without checking compatibility.

This is source-available fan work, not a blanket license for third-party characters or assets. Preserve credits and applicable software licenses; obtain the rights needed for your own distribution.
