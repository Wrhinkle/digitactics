# Online play, local data and updates

This guide describes the **native 2.0.0 Windows edition**. Electron 1.x uses different save and update paths, documented at the end.

## What connects to what

| Component | Role |
| --- | --- |
| Native Windows game | Renders with BRUTAL/OpenGL, simulates matches and stores local progress |
| Native launcher | Starts and supervises the game; discovers, verifies and stages native updates |
| Steam desktop client | Supplies SteamID/display name, friends, lobby discovery, invites and networking |
| Lobby host | Orders valid player actions and maintains the shared match log |
| Other players | Receive that log and simulate the same match |
| GitHub Releases | Hosts full downloads and the native update manifest/payload |

Start Steam and sign in before launching Digi-Tactics. Solo does not require Steam. The native integration uses **Spacewar App ID 480**, a development/sample integration, not a Digi-Tactics store listing. Steam may display Spacewar; availability depends on Steam and may change. Keep Digi-Tactics open before accepting an invite because the shared development App ID may otherwise launch Spacewar. Native launch arguments and in-game invite callbacks can forward a lobby request to an already running game.

Rooms are filtered by the Digi-Tactics marker and compatible multiplayer protocol. The native edition uses **protocol 9**. Use the same release on each PC; a shared App ID does not establish compatibility with older Electron clients. Public rooms appear in the room browser, and friends-only rooms can be discovered through friends. This is lobby-based multiplayer, with no ranked matchmaking queue or project-operated account service.

Each match has eight seats. Duos groups them into four teams of two with shared health, partner transfers and a combined battlefield. AI controls empty seats. An idle host can change the set, pace and format before starting. Chaos set and pace choices are resolved once from the host's match seed; peers receive those resolved values and do not roll their own choices.

A disconnected player's seat can return to AI after about 20 seconds; returning players can claim available living seats. A remaining player can take over hosting after the old host disappears. These recovery paths do not guarantee every interrupted match can be recovered. Closing results or returning Home does not leave a connected room; use the explicit **Leave** control to disconnect.

## Security and abuse controls

The native game is a C# desktop application without an embedded Electron renderer or browser. Native networking accepts current lobby peers and validates and bounds messages. Host-side checks bind actions to seats and enforce game phases; rate limits, duplicate/replay checks and bounded logs/snapshots reduce malformed-input and spam abuse. Players can mute peers, and hosts can remove disruptive players. Removal is an application-level control, not a global account ban.

Steam transport requests relays and disables direct peer address sharing before creating sessions. Initialization fails if that configuration cannot be established. This has not been independently audited as a network privacy guarantee.

Steam identity does not make client game logic trustworthy. A modified host or client can cheat. There is no central authoritative server or VAC integration, and local progression/results are not competitive proof. Automated source comparisons, fake-Steam transport checks and local UI tests do not establish live cross-device recovery, clean-machine compatibility or public-scale concurrency. Real two-account Steam recovery, actual Windows chooser interaction, physical audio-device switching and clean-machine execution remain unverified.

## Saves and migration

Steam supplies online identity and display name. **Progress stays on the Windows user account**, without Steam Cloud or another account server. Switching Steam accounts within the same Windows profile does not create a separate progression save.

| Data | Native location |
| --- | --- |
| Profile, settings, custom music and preserved imports | `%LOCALAPPDATA%\DigiTactics\native` |
| Downloaded/staged updates and launcher state | `%LOCALAPPDATA%\DigiTactics\native-updates` |
| Fatal game startup log | `%LOCALAPPDATA%\DigiTactics\logs\game-error.log` |
| Fatal launcher startup log | `%LOCALAPPDATA%\DigiTactics\logs\launcher-error.log` |

Use **Settings → Export save backup** to make a portable JSON containing progress, collection, companion evolution, Digi-Farm, match history, settings and custom music. Choose a destination outside the profile folder and keep it private. The archive supports up to 256 MiB of music and 384 MiB total JSON size; keep a separate copy of a larger music library. With the game fully closed, copying the entire native profile folder is also a manual filesystem backup. Do not copy only `profile.json`: music and preserved imports are separate files.

To migrate from Electron 1.x:

1. Open the old game, choose **Settings → Export save backup**, retain that JSON and close the old game.
2. Install the **full native ZIP** in a separate folder. On first Home, choose **Import save backup...** before playing or changing settings.
3. Select the JSON, review the profile summary, then choose **Import profile**. Canceling or rejecting an invalid archive leaves the native save unchanged.

Import is explicit. The native app validates the archive and music hashes, preserves the original archive before applying its contents, and does not modify `%APPDATA%\Digi-Tactics`. It never silently merges or replaces an existing native profile. To intentionally replace native progress, export it first, leave any match/lobby, use **Settings → Reset save...**, type **RESET**, and confirm. Then import from Home before making more changes. Reset clears active progress/settings/music while retaining previously exported backups and preserved import archives; it is not a secure erasure of all recovery files.

Keep the old game and exported archive until you have checked your native profile. Saves in the two editions are independent after migration. There is no automatic synchronization back to Electron or between PCs.

Online peers receive the identity, lobby presence, actions and social messages needed to play. GitHub receives update requests, and Steam handles its own traffic. Native UI fonts are loaded locally. Logs are not automatically uploaded by the game. Do not publish an entire profile or unredacted log in an issue.

## Native updates

Always start **`DigiTactics.Launcher.exe`** with the accompanying files and `app` folder intact. Starting `app/DigiTactics.Desktop.exe` directly bypasses launcher updates and supervision.

1. After an interactive game startup is confirmed, the launcher checks `Wrhinkle/digitactics`'s latest public GitHub Release, using an ETag cache where available. Drafts and prereleases are excluded.
2. It reads **`native-latest.json`**, which identifies the version, `win-x64` platform, minimum launcher version, payload filename, size and SHA-256 hash. Electron's `latest.json` is ignored.
3. A newer compatible payload is downloaded, verified and staged under the native update directory. Extraction also validates the package manifest and individual files before a candidate is queued.
4. A ready update appears on Home. **Restart to update** is offered outside a connected match/lobby and waits for file operations to finish. Closing and reopening through the launcher also activates a queued candidate; the running match is not replaced mid-game.
5. The launcher checks candidate integrity again and supervises startup. A rejected or failed candidate can roll back to the previously confirmed application or the bundled application. This recovery protects startup; it is not a guarantee against every later gameplay bug.
6. If the update requires a newer launcher, the UI directs you to the release page for a new full ZIP. The application-only updater does not replace the launcher itself.

No GitHub credential is bundled or required. Offline operation, rate limits or invalid update metadata do not prevent launching the installed game. A full native package is the manual fallback; retain the separate save folder when replacing an installation. SHA-256 checks detect corruption but are not an independent signature against a compromised release account.

| Native asset | Intended use |
| --- | --- |
| `DigiTactics-native-<version>-win-x64.zip` | Full installation; extract everything and run `DigiTactics.Launcher.exe` |
| `DigiTactics-native-update-<version>-win-x64.zip` and `native-latest.json` | Native automatic updater; not standalone installations |
| `SHA256SUMS.txt` | Checksums for the attached artifacts |
| `native-release-artifacts.json` | Local assembly/integrity report; it does not grant release approval |
| Matching `digitactics-media-audio-sources-*.zip` and `MEDIA_RUNTIME_NOTICES.txt` | Source/build materials and notices for the packaged FFmpeg audio runtime; not game installers |
| GitHub's Source code archives | Repository source for developers |

Use the full installation ZIP for the first native installation; the update ZIP requires the native launcher. See [artifact assembly](../native/tools/NATIVE_RELEASE_ARTIFACTS.md) for maintainer verification.

## Historical Electron 1.x

Electron releases use `DigiTactics-Steam-<version>.zip` with **`Digi-Tactics.exe`**, or the alternate `DigiTactics-Steam.exe` self-extracting package. Extract the full ZIP and keep its files together. Their application data is **`%APPDATA%\Digi-Tactics`**, with errors in `main.log`. Back up that folder with the old game closed before removing the installation or clearing data.

The Electron updater consumes **`latest.json`** and **`payload-<version>.zip`**, with its own shell-version requirement. Those payloads cannot migrate Electron to the native runtime, and native payloads must not be installed into Electron. The first native installation is a separate full download followed by explicit save import.

For historical maintenance, Electron 1.0.12 and earlier required a full 1.0.13-or-later Electron package to repair a missing networking runtime dependency. Private Electron test builds likewise required a public full package to switch their update channel. Neither historical workaround replaces the native migration procedure above. Browser source still has an optional developer-configured Firebase transport; it is not the native client's transport and no public web backend is configured here.
