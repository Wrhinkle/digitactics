# Digi-Tactics BRUTAL 2.0.2

Windows update for the BRUTAL client, published as the current build at the owner's request.

- Repaired T-pose and bind-pose idles, victories and actions across the roster, including generated biped and non-biped rigs, clipless rips that now borrow matching authored clips, and Links Apocalymon's core and chain arms.
- Fixed Renamon's attack growing to many times its size, anchored traveling bodies to their hex, and held armature-group scale constant during actions.
- Restored eyes on Next Order models; restored Hackmon, CatchMamemon, Sistermon Blanc Awakened and Boltboutamon textures; added eye highlights to five Time Stranger models.
- Fixed Crabmon, Seadramon, Shamanmon, Snow Goblimon, V-mon and CatchMamemon poses and movement. Aegiomon now uses its own Champion model instead of an Elecmon disguise.
- Combat animation switches crossfade from the displayed pose; dropped and moved units glide into their cells. Transparent model materials blend correctly again.
- Restored the original layout and styling for the client shell, Home, Play, party, lobby browser, loading, results, postgame, match HUD, shop, carousel, tooltips, Collection, Store, Loot, Digi-Farm, Codex, Settings and Profile. UI text uses ClearType-style rendering, and lists use the original scrollbars.
- The postgame result stays on Home while you visit other client pages, until you dismiss it or choose a result action.
- Multiplayer protocol is unchanged (protocol 9); 2.0.1 and 2.0.2 players can join the same rooms.

## Install or update

Existing BRUTAL users: restart `DigiTactics.Launcher.exe` with updates enabled to receive 2.0.2. For a fresh installation, download `DigiTactics-native-2.0.2-win-x64.zip`, extract the complete archive, and run the launcher. Keep the `app` and `launcher` folders beside it. The update ZIP is for the launcher's updater, not manual installation.

The earlier Electron client is a separate runtime and is not converted by this native updater. Electron users should download the full BRUTAL Windows ZIP.

## Known issues and verification scope

This build contains ongoing parity and model repairs; it is not a declaration of full Electron appearance/function parity or complete model acceptance. Remaining issues include small UI offsets on some screens, approximated Time Stranger eye highlights, and some specials that can leave the frame (for example CatchMamemon's special). Two-account Steam reconnect/host migration and clean-machine coverage are not fully verified.

The native CPU test suite (5,699 reference battles), the Steam ABI test, UI smokes, and packaged startup/playback and updater checks accompany this update. See `docs/BRUTAL_CHECKPOINT.md` and `docs/MODEL_AUDIT_CHECKPOINT.md` for the detailed remaining work. The release artifact integrity report intentionally does not certify whole-game acceptance.
