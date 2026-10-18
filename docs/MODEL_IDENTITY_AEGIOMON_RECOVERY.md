# Aegiomon recovery handoff

Status: **candidate only, not approved or installed**. Investigated 2026-10-18;
work stopped at the user's requested checkpoint. The shipped identity failure
in [MODEL_IDENTITY_AEGIOMON.md](MODEL_IDENTITY_AEGIOMON.md) remains open.

## Original source located

[Nuffle's Time Stranger viewer](https://nuffle.me/dsts-viewer?m=aegiomon)
publishes this exact model-index entry in
`https://nuffle.me/dsts-viewer/assets/index-Bwi7YmMN.js`:

```json
{"dex_number":81,"name":"Aegiomon","jp_name":"アイギオモン","slug":"aegiomon","chr_id":"chr183a","char_key":"char_AEGIOMON","category":"digimon"}
```

Its original asset endpoint is
`https://cdn.nuffle.me/v2/dsts/digimon/aegiomon/aegiomon.glb`.
The downloaded GLB is 70,058,844 bytes, SHA-256
`6fa0d32b77d9b78111e07aa5cff620a87c57bee62ba4a9f2ea805419bcb02f53`.
Generator: `dsts-extractor 0.8.3`. The source has 277 nodes, 22 meshes,
two skins, and original body and facial animation clips. This is a credible
Champion-form candidate, unlike the existing `chr563` Elecmon-disguise archive;
the source identity still needs confirmation against actual rendered views and
the official references recorded in the original defect report.

The facial skeleton is already attached under body `head`: the source node 5
includes face root node 100 (`GRP_joint.face`). Preserve that hierarchy and both
skins. Do not replace it with the wrong quadruped's generated motion.

Source-viewer JavaScript SHA-256:
`08e186a06e2b5ba6edd52711e407176d65b95ccc3225e15ba0c7806959182aa9`.
The source author's code links are `https://tangled.org/nuffle.me/dsts-viewer`
and `https://tangled.org/nuffle.me/dsts-extractor`; they were not fetched or run.
This investigation downloaded only the public viewer and its public asset.

## Local evidence and reproduction

All large files remain untracked under `native/artifacts/aegiomon-recovery/`:

- `aegiomon-original.glb`, `original-gltf.json`, `viewer.js`, `viewer.html`.
- `textures/`: the original embedded PNGs, extracted without image changes.
- `prepare-candidate.mjs`: original audit reducer; `check-candidate.cjs` samples
  all skinned geometry at 61 timestamps per role through the vendored Three loader.
- `candidate.glb`: latest **bs03** special variant, 4,373,580 bytes, SHA-256
  `c0bb5805fb0ac5d4063cfc21f11d5c9856a63641c90bd2482f25a468bca34541`.
- `check-bs02.json`, `check-bs03.json`: sampled original-motion bounds.
- `candidate-preparation.json`: selected source clips and omitted effect hulls.
- `build/`: isolated desktop build, succeeded with zero warnings/errors.
- `isolated-candidate.json`: earlier **bs01** candidate installed into that
  isolated build only; it differs from the latest `candidate.glb` deliberately.
- `candidate-01.log`: first native attempt failed before rendering because a
  prepared-motion sidecar was missing. `install-isolated.cjs` now creates it,
  but **no second native render was run**. Do not interpret process exit zero
  or `candidate-01/` as a visual pass.

The tracked `native/tools/prepare-aegiomon-candidate.cjs` reproduces the reduced
candidate from the exact original download. It never writes production assets.
Dependencies used: `@gltf-transform/core@4.5.1` and
`@gltf-transform/functions@4.5.1`, installed in the local recovery artifact
directory. Example from the repository root:

```powershell
npm install --prefix native/artifacts/aegiomon-recovery --no-audit --no-fund @gltf-transform/core@4.5.1 @gltf-transform/functions@4.5.1
node native/tools/prepare-aegiomon-candidate.cjs native/artifacts/aegiomon-recovery/aegiomon-original.glb native/artifacts/aegiomon-recovery/reproduced bs03
```

The tracked reducer was run once during handoff validation and reproduced the
exact latest candidate SHA-256 above. Its JavaScript syntax check also passed.

The reducer retains original idle `bn01`, run `br01`, attack `ba01`, selected
special `bs01/bs02/bs03`, and victory `bv01`. It merges the matching facial
special/victory channels because the native player plays one combined clip.
It omits six duplicate source outline hulls, whose original shader inflates
back faces; standard unmodified surfaces are not equivalent. It removes
non-PBR overlay-mask metallic/roughness bindings and source normal maps for
the preliminary candidate. Those material choices remain **unapproved**.

## Findings requiring completion

All 305 sampled bounds in each tested five-role candidate were finite. This
is a CPU geometry check, not an animation-quality approval.

| Original special | Duration | Sampled center travel, X/Y/Z | Finding |
|---|---:|---|---|
| `chr183a_bs01` | 2.000 s | 0.192 / 0.188 / 10.429 | Charges far off the board; unsafe as-is. |
| `chr183a_bs02` | 5.767 s | 0.001 / 0.023 / 0.031 | Bounded; readability and gameplay timing unreviewed. |
| `chr183a_bs03` | 2.667 s | 0.054 / 0.005 / 0.125 | Bounded; latest candidate, visually unreviewed. |

Victory `bv01` includes approximately 1.359 source units of vertical center
travel. Review its jump, framing, endpoint and loop seam rather than assuming
finite bounds mean it is acceptable. Attack starts/ends at matching sampled
body bounds; run is bounded and starts/ends at matching sampled bounds.

Eye materials require explicit conversion. `eye_h` uses source `InnerGrowAValue`
as a view-dependent additive matcap, not ordinary UV albedo. Its source PNGs
contain alpha. `eye_s` has source `DiffuseAlpha` approximately
`[0.03,0.08,0.20,0.40]`; the preliminary standard material has not applied this
tint/transparency. Underlying iris, sclera, brow, lashes, teeth and tongue are
separate meshes. Do not ship white opaque eye overlays or silently discard
anatomy. Source-viewer shader excerpts are in the saved `viewer.js`.

Before installation: inspect native full-role atlases, fitted front/back,
actual-board motion and action transitions; verify face attachment and
expressions, eye materials, source scale, loop seams, and the chosen special.
Then update the precise source asset, mapping, metadata, portrait, credits,
import-map protection and regression checks. Preserve the historical exact-hash
identity FAIL while recording a new reviewed identity for the replacement.
Coordinate generated model/motion/dance-binding exports with their owners;
the shared generated catalog remains root-owned. No production asset, mapping,
portrait, identity-review status or generated catalog changed in this recovery.
