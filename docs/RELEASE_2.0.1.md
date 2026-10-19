# Digi-Tactics BRUTAL 2.0.1

Windows update for the BRUTAL client, published as the current build at the owner's request.

- Restored and improved Home, music/settings, collection, choice and match interfaces. Carousel cards show wrapped names, traits and items, with improved scrolling and selection behavior.
- Lucky Glove consumes all three equipment slots. Three-star units are removed from that player's unit offers.
- Chest/key earning rates are halved, with clearer earned-reward notifications.
- Added recurring farm caretaker upkeep and reduced offline care deterioration.
- Improved secret collectible boards, emotes, dances, sounds and finales.
- Fixed multiple model materials, walking animations, cinematic movement, original DW1 rigid-part animations and Brachiomon assembly. Restored MachGaogamon sonic opacity and MagnaGarurumon material mapping.
- Preserves the organized launcher/app folders and restored launcher icon.

## Install or update

Existing BRUTAL users: restart `DigiTactics.Launcher.exe` with updates enabled to receive 2.0.1. For a fresh installation, download `DigiTactics-native-2.0.1-win-x64.zip`, extract the complete archive, and run the launcher. Keep the `app` and `launcher` folders beside it. The update ZIP is for the launcher's updater, not manual installation.

The earlier Electron client is a separate runtime and is not converted by this native updater. Electron users should download the full BRUTAL Windows ZIP.

## Known issues and verification scope

This build contains ongoing parity and model repairs; it is not a declaration of full Electron appearance/function parity or complete model acceptance. Remaining work includes postgame navigation/result persistence, some portraits, Aegiomon's incorrect model identity, additional animation/board-containment cases, and separate MachGaogamon glow/Lucemon transparency effects. Two-account Steam reconnect/host migration and clean-machine coverage are not fully verified.

Desktop/test compilation, focused source/material/animation checks, packaged startup/playback and updater checks accompany this update. See `docs/BRUTAL_CHECKPOINT.md` and `docs/MODEL_AUDIT_CHECKPOINT.md` for the detailed remaining work. The release artifact integrity report intentionally does not certify whole-game acceptance.
