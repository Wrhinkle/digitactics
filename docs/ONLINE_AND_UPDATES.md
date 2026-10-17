# Online play, local data and updates

## What connects to what

| Component | Role |
| --- | --- |
| Windows game | Renders the board, simulates matches and stores local progress |
| Steam desktop client | Supplies SteamID/display name, friends, lobby discovery, invites and networking |
| Lobby host | Orders valid player actions and maintains the shared match log |
| Other players | Receive the ordered log and simulate the same match |
| GitHub Releases | Hosts full downloads and the update manifest/payload |
| Optional web transport | Developer-configured Firebase backend; unconfigured in this public source |

The current Windows integration uses **Spacewar App ID 480**, a development/sample integration. It is not a Digi-Tactics store listing. Online availability depends on that third-party service and may change. Steam's [Spacewar example documentation](https://partner.steamgames.com/doc/sdk/api/example) describes the sample application; it is not a service guarantee for this project.

Rooms are filtered by the Digi-Tactics game marker and compatible multiplayer protocol. The shipped protocol is **7**. Update every player's game to the latest release to see and join the same rooms. Friends-only and public visibility control room discovery. This is lobby-based multiplayer, with no skill ranking, global queue or project-operated account service.

Each normal lobby has eight seats. Duos groups those into four teams of two, with shared health, partner transfers and a combined battlefield. AI controls empty seats. A disconnected player's seat returns to AI after approximately 20 seconds; a returning player can take an available living seat. A remaining player can take over hosting after the old host disappears. These recovery paths do not guarantee every disconnected match can be recovered.

## Security and abuse controls

The desktop renderer is sandboxed without Node integration. Native networking accepts current lobby peers and validates/bounds messages; host-side checks bind actions to seats and enforce game phases. Rate limits, replay/duplicate checks and bounded snapshots/logs reduce spam and malformed-input abuse. Players can mute peers, and hosts can remove disruptive players. Removal is an application-level control, not a global account ban.

The native configuration disables direct peer IP connections and requests relayed transport. If that configuration cannot be established, online initialization fails rather than silently accepting the weaker configuration. This has not been independently audited as a network privacy guarantee.

Steam identity does not make client game logic trustworthy. A modified host or client can cheat, and there is no central authoritative server or VAC integration. Avoid treating local progression or match results as competitive proof. Public scale and live cross-device recovery have not been load-tested; no concurrency or uptime guarantee is offered.

## Your account and data

SteamID identifies your online account across devices; Steam supplies the current display name. **The game profile itself stays local.** Collections, cosmetics, settings, match history and progression live in the application's local browser storage. They are not Steam Cloud saves and are not partitioned into separate progression profiles for each Steam account on the same Windows profile.

With the game fully closed, back up `%APPDATA%\Digi-Tactics` before migrating or resetting. This folder also contains logs, caches and downloaded update payloads; treat the backup as private. Copying the closed application's profile is a manual backup approach, not a guaranteed cross-platform save format. Clearing the folder loses local progress. There is no blockchain token, wallet or server-side progress account.

Online peers receive the identity, lobby presence, game actions and social messages needed to play. GitHub receives update requests; Steam handles its own service traffic. The UI requests its web fonts from Google Fonts when available and uses fallback fonts when unavailable. Local logs and crash dumps are not automatically uploaded by the game. Do not publish an entire profile or unredacted log in an issue.

## How updates work

1. Once per launch, the app checks `Wrhinkle/digitactics`'s latest GitHub Release, using an ETag cache where possible.
2. It reads the release's `latest.json`, which names a version, required shell version, payload file, byte count and SHA-256 digest.
3. A newer compatible payload is downloaded into the local update directory. Its size and SHA-256 must match before extraction/staging.
4. The next restart activates the staged payload. Startup failure/watchdog handling can restore the previous payload. Updates do not replace the running game mid-match.
5. If a release needs a newer desktop shell, the app directs you to a full download instead.

Public downloads are anonymous: no GitHub account or credential is required or bundled. GitHub rate limits, outages and network failures can delay checks; they do not delete the installed game. The full release zip is the manual fallback. The manifest's checksum detects corruption; it is not an independent signature against a compromised release account. Only install updates from maintainers you trust.

| Release asset | Intended use |
| --- | --- |
| `DigiTactics-Steam-<version>.zip` | Recommended full Windows download; extract and run `Digi-Tactics.exe` |
| `DigiTactics-Steam.exe` | Alternate self-extracting portable launcher |
| `payload-<version>.zip` and `latest.json` | Automatic updater; do not launch or manually install these |
| `SHA256SUMS.txt` | Checksums for full downloads and update assets |
| GitHub's Source code archives | Developers; not a packaged desktop application |

Existing private test builds still point at the old private repository. **Download this public full build once to switch to the public update channel.** It uses the same local profile location, so close the old game first. The private release history is not part of this public repository.
