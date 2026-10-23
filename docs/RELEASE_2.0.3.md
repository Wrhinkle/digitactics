# Digi-Tactics BRUTAL 2.0.3

Windows update for the BRUTAL client, published as the current build at the owner's request.

- Special cut-ins: when a Digimon casts its special, the arena dims and a tilted panel slides in with a live close-up of the caster's head, rim-lit in the special's element colour, with the Digimon and special names. The camera then follows the caster through the special and eases back to the board.
- The head close-up is framed from the head's own geometry. Without a head joint it aims at the eyes, or at the upper body for estimated heads. When armour or an arm blocks the face, it switches to a clearer side or a front three-quarter view.
- The tracking camera keeps the caster and its effects centred inside the visible arena and follows leaps.
- New setting: Settings > Interface > Special cut-ins, with All, Mine only (default) and Off. Mine only shows cut-ins for your own units in your own fight. Turning Animations off also turns cut-ins off. Cut-ins only change presentation; battle results are unchanged.
- A caster's glow is now tinted with its special's element colour instead of white, so a unit whose special ends the fight no longer stays bright white through the result phase.
- Automated test runs, smokes and captures no longer play sound through the speakers.
- Multiplayer protocol is unchanged (protocol 9); 2.0.1, 2.0.2 and 2.0.3 players can join the same rooms.

## Install or update

Existing BRUTAL users: restart `DigiTactics.Launcher.exe` with updates enabled to receive 2.0.3. For a fresh installation, download `DigiTactics-native-2.0.3-win-x64.zip`, extract the complete archive, and run the launcher. Keep the `app` and `launcher` folders beside it. The update ZIP is for the launcher's updater, not manual installation.

The earlier Electron client is a separate runtime and is not converted by this native updater. Electron users should download the full BRUTAL Windows ZIP.

## Known issues and verification scope

This build contains ongoing parity and model repairs; it is not a declaration of full Electron appearance/function parity or complete model acceptance. Known issues: Omnimon's cut-in drifts toward its chest in later frames; some specials can leave the frame (for example CatchMamemon's special); small UI offsets remain on some screens; Time Stranger eye highlights are approximated. Two-account Steam reconnect/host migration and clean-machine coverage are not fully verified.

The native CPU test suite (5,699 reference battles), the Steam ABI test, the special cut-in checks, UI smokes, and packaged startup/playback and updater checks accompany this update. See `docs/BRUTAL_CHECKPOINT.md` and `docs/MODEL_AUDIT_CHECKPOINT.md` for the detailed remaining work. The release artifact integrity report intentionally does not certify whole-game acceptance.
