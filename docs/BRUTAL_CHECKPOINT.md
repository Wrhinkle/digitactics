# BRUTAL development checkpoint — 2026-10-18

Saved and pushed at the user's request to stop the agents and publish current source progress. This is a development checkpoint, not a release approval or a claim of complete Electron UI parity. The long-running completion goal is paused.

## Included work

- Restored substantial Home, settings/music, collection, choice, loot-notice, farm and match UI behavior. Carousel cards now wrap full names, traits and items; actual selection, scrolling, taken-card rejection and continuation passed 24 cases each at minimum and short window sizes.
- Lucky Glove occupies all three equipment slots. Three-star units are excluded from the owning player's shop.
- Chest/key earning rates were halved. Earned rewards have explicit notices. Farm caretakers use recurring Bits upkeep, with basic and full care tiers and reduced offline biological decay.
- Expanded secret collectible boards, emotes, dances, sounds and finales, separate from the shop. Validation is scoped to the recorded tests; audio listening and every model/dance combination are not fully accepted.
- Repaired original short-leg and fore/hind locomotion, several cinematic body roots, material/UV errors, normal Ravemon identity, three original DW1 rigid-part animation sets, and Brachiomon's exploded source assembly.
- Added source vertex-color/opacity support and exact MachGaogamon sonic-effect recovery, plus MagnaGarurumon source material/UV recovery.

## Verification and limits

The combined model checkpoint uses catalog SHA-256 `7d7ecc00436c2b6815ab7778d0a1f088e097a75cd2d9f91f780a54f63bc25a1e`: 749 assets and 937 source selections. Its CPU audit sampled all 937 selections with zero execution errors and 278 explicit review holds. Passing reference checks include 2,506,908 action checks, 1,385,456 prepared-motion checks, 2,975,711 companion-playback checks and 631 locomotion/anatomy checks. These do not constitute whole-roster visual acceptance.

Original built-in music metadata and all three tested window layouts passed. Brachiomon's sampled native views show a connected assembly with distinct movement/attack/special/celebration; continuous transitions and original-game animation fidelity remain limited. Several repaired cinematic roots passed specific center/rear-edge live-board samples.

Final wrap-up checks: Desktop and test projects both compiled with zero warnings/errors; the browser bundle was rebuilt from current sources. Fresh material checks passed 36 RGB/RGBA contracts and 34,446 reference checks across 1,242 material variants. Root-travel regressions and 72 browser vertex-color material checks passed. The complete release suite was not rerun during wrap-up.

Detailed screenshots, frozen runtimes and raw logs are retained locally under `native/artifacts/`, which is intentionally excluded from Git. Relevant folders include `model-visual-audit-v3`, `model-visual-audit-after-7d7ecc`, `model-visual-audit-after-forehind`, `vertex-colors-check`, and `carousel-acceptance-*`. Tracked source tests, repair provenance and fixtures accompany the code. Historical audit verdicts must be matched to exact current source/payload hashes; changed hashes invalidate old evidence without proving a repair.

## Required follow-up

1. Finish appearance and functional parity throughout the UI. A confirmed remaining postgame gap: Electron preserves results in Home while navigating Settings/Profile; native navigation dismisses results when leaving its limited Profile/Store overlay and omits the main result screen's normal navigation/social chrome. No fix for that gap was started during wrap-up.
2. Independently review the new carousel at normal size and in a full match, then add its accepted interaction driver to the normal build gate. A gray wedge portrait for Kudamon was observed in late-carousel captures and needs asset diagnosis.
3. Complete current whole-roster animation/material/transition/scale/board-containment acceptance. Tiny overview atlases are insufficient for gait or continuous motion approval. More original DW1 assemblies/animations and cinematic displacement cases remain.
4. Replace the confirmed wrong Aegiomon form only after candidate identity, animation and native rendering acceptance. The candidate is preserved separately; production replacement was deliberately not made at this checkpoint.
5. Resolve remaining MachGaogamon opaque glow cards and Lucemon fractional-alpha effect cards. Sonic-effect alpha recovery does not approve those separate effects.
6. Reconcile current acceptance manifests and catalog hashes, merge explicit v3 reviews rather than the historical v2 default, and rerun the complete integrated release gates. Package creation alone does not enforce model acceptance.
7. Verify real two-account Steam reconnect/host migration, actual Windows chooser interaction, physical audio switching and clean-machine execution. Same-machine/simulated tests are narrower evidence.
8. After acceptance, build and validate the Windows package, updater/rollback and release attachments, then publish the release. No new Windows release is approved by this checkpoint.

See `native/MIGRATION.md` for the detailed historical record; some older follow-up notes there are superseded by later entries.

`docs/MODEL_AUDIT_CHECKPOINT.md` and its compressed metadata archive preserve review decisions/provenance in Git without the large local raster captures. `docs/MODEL_IDENTITY_AEGIOMON_RECOVERY.md` records the unshipped identity candidate and reproduction instructions.
