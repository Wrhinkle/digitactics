# Digi-Tactics

![Digi-Tactics — Build. Digivolve. Battle. Solo + Duos.](media/github-social-preview.jpg)

**Build your team. Digivolve your favorites. Battle solo or bring a partner.**

A free, unofficial Digimon autobattler made for fun. Buy Digimon, combine copies to evolve them, build trait synergies, equip items and position your team before automatic battles. Explore twelve ruleset choices, including Undermons and Auto Chess, with Normal, Hyper Roll and cooperative Double Up options.

**[Download for Windows](https://github.com/Wrhinkle/digitactics/releases/latest)** · [Watch the trailer](media/digi-tactics-trailer.mp4) · [Full credits](CREDITS.md) · [Report a bug](https://github.com/Wrhinkle/digitactics/issues)

## Start playing

1. Open **[the latest release](https://github.com/Wrhinkle/digitactics/releases/latest)**. Under **Assets**, download `DigiTactics-Steam-1.0.11.zip` (or the newer version's full zip). The automatic **Source code** downloads are for developers, not the ready-to-play game.
2. Extract the entire zip to a writable folder. Keep all its files together and launch **`Digi-Tactics.exe`** inside the extracted folder. Do not run it from inside the zip.
3. For online play, have the Steam desktop client running and signed in before launching. Solo works without an online lobby.
4. Open **Play**, browse the mode carousel, choose your ruleset and pace, then start solo or open Multiplayer to create/join a lobby.

The release is for **64-bit Windows** and includes its desktop runtime. No development tools, GitHub account, GitHub token or payment are needed. A standalone `DigiTactics-Steam.exe` is also provided; it extracts its runtime at launch. The zip is the recommended download. Builds are currently unsigned, so Windows may display a publisher warning; verify your download came from this repository. Release checksums are in `SHA256SUMS.txt`.

## Your first match

- **Buy and build:** buy from the shop, move units from your bench onto the board and position your team. Matching copies automatically combine; most rulesets use three copies, with exceptions explained in the in-game Codex.
- **Make a team:** combine matching traits, equip items and choose upgrades. The Codex describes the selected ruleset's units and mechanics.
- **Fight:** your team battles automatically. Normal uses the longer economy-focused format; Hyper Roll offers a faster pace.
- **Double Up / Duos:** four teams of two share health. Send units through the partner portal and fight together on a combined board. Empty seats are controlled by AI.
- **Take a break:** press **Esc** or click **Menu** to leave a match. Solo has an explicit **Pause** option; opening the menu alone does not pause, and online matches cannot be paused.

Winning finishes with the actual final attack and a ground-level team victory dance. Cosmetic collections and progression are earned locally; this project is not a real-money storefront.

## Play online

The Windows build uses Steam identity, lobbies, friend invites and relayed peer networking. Open the game on both computers first, then create a **public** or **friends-only** lobby. Public rooms appear in the lobby browser; the host can invite friends through the overlay. Everyone should use the latest release. Lobby discovery is a room browser, not a ranked matchmaking queue.

**The current integration uses the Spacewar development App ID, 480.** Steam may show you as playing Spacewar. This is an experimental fan-project setup, not an official store release or a promise of permanent service. If an invite opens Spacewar, open Digi-Tactics and accept the invite again. A player hosts each match, with host handover and AI filling empty seats.

The build includes peer/message validation, host-side action checks, rate limits, mute and host removal controls. A modified host/client can still cheat. There is no authoritative ranked service, VAC integration or claim that thousands of simultaneous players have been tested.

**[Online, privacy, saves and updates explained](docs/ONLINE_AND_UPDATES.md)**

## Updates and saves

The game checks this public repository's latest release at launch, downloads and verifies a newer game payload, and applies it on restart. **No token is included or required.** If GitHub is offline or rate-limited, the installed game remains usable; you can always download a full release manually. A desktop-runtime change may require a new full download. Private testers should download this public full build once to switch update channels.

Progress, cosmetics, settings and match history are **stored locally on this computer**. Steam supplies your online identity and display name, but progress does **not** sync through Steam Cloud, another account server or a blockchain. Back up `%APPDATA%\Digi-Tactics` with the game closed before moving computers or clearing app data. Keep your profile folder when updating.

## Help and development

- **No lobbies / cannot connect:** confirm Steam is signed in, restart the game, check both players have the latest release, and confirm room visibility. Solo remains available if online services are unavailable.
- **Invites:** have Digi-Tactics running before accepting; the shared development App ID can otherwise launch the sample application.
- **Update trouble:** close the game and download/extract the newest full zip. The version is visible under **Settings → About**.
- **Slow rendering:** reduce the window size and close other GPU-heavy applications. The trailer uses fixed-step capture and cinematic cameras; it is not a performance benchmark.
- **Bug reports:** include the version, ruleset, solo/online mode, reproduction steps and a screenshot in an [issue](https://github.com/Wrhinkle/digitactics/issues). The local log is `%APPDATA%\Digi-Tactics\main.log`; redact account identifiers and personal paths before sharing excerpts.
- **Build or run the source:** see [the development guide](docs/DEVELOPMENT.md). Browser source runs support solo by default; the Windows download is the supported online route.
- **Security reports:** see [SECURITY.md](SECURITY.md).

## Made for fun, with thanks

**This is a free, non-commercial fan project. No profit is intended or sought.** It is made for people to enjoy, with no official affiliation or endorsement. The original characters, games, models, textures, animations, trademarks and other third-party work belong to their respective creators and rights holders. Free distribution and attribution do not grant rights in those works.

Read **[CREDITS.md](CREDITS.md)** for the original games, companies, mechanics, art, music, tools and contributors; **[the complete model credits](docs/credits/MODELS.md)** for every bundled model and its source; and **[third-party notices](THIRD_PARTY_NOTICES.txt)** for software acknowledgements and licenses. Original artists and community extractors/uploaders are credited separately where known. Missing names are marked explicitly. Attribution corrections or removal requests are welcome through [the issue tracker](https://github.com/Wrhinkle/digitactics/issues).

This repository is shared as source-available. It does not claim a blanket open-source license over the game or its third-party assets. Keep the attribution and applicable notices with downstream work.
