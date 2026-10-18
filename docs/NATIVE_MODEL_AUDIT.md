# Native model evidence

The audit enumerates every entry in `native/content/models.json`, including unused, fallback, static and procedural models. Battle aliases retain their variant, line and stage; companion bindings retain their role clips and procedural effects. Nothing is removed from the matrix because it cannot be rendered or has no skeletal clips.

`VERIFIED` applies only to its named check. Valid files, finite poses and nonblank pictures do not establish correct motion or appearance. A row's overall status stays `UNVERIFIED` until current dependency, rig, runtime, rendered and visual-review evidence all agree. An explicit failure in any stage remains `FAIL`.

Run the CPU inventory with the existing test executable:

```powershell
dotnet <tests-output>/DigiTactics.Tests.dll native --model-inventory-audit
python native/tools/model-audit-report.py
python native/tools/test-model-audit-report.py
```

The inventory writes `native/artifacts/model-audit-inventory.json` and CSV. It loads every compressed asset through the shipped loader, checks the catalog digest and byte count, decodes every primitive, validates every weighted joint index and material/texture reference, and compares every decoded image with the original-browser texture fixture. It validates authored and prepared rig hierarchies, rest transforms, inverse binds, channel targets, timelines and finite keys. It records the actual loaded asset assembly hash and relevant source hashes.

Every prepared clip records its authored index or generated fallback origin, duration, channel/key counts, affected mesh/bone ancestors and whether its keys vary. Role resolution follows `ModelAnimator.FindIdle`, exact `attack`, `special` and `win` names (`win` is the celebration role), and named movement candidates. An available clip is not called a verified action merely because its name exists. Clipless companion models retain their explicit procedural bindings for a separate playback check.

The runtime audit's `native/artifacts/model-runtime-summary.json` supplies sampled full-cycle skinned bounds, transition extrema, the exact sampling schedule, suspects and a hash-linked compressed detailed trace. The matrix links this evidence instead of duplicating its large vertex-bound trace. Finite samples are reported separately from motion fidelity.

The current visual audit writes `native/artifacts/model-visual-audit-v2/NNNN.json` and PNG contact sheets. Per-frame draw and pixel counts establish only a technical render. Reviewed appearance and animation decisions belong in `reviews.json`, keyed by `sourceKey`, with `visualFidelityStatus`, `fineTextureStatus`, `motionReviewStatus`, evidence paths and a concrete explanation. An evidence-free approval is ignored. Missing review scopes stay `UNVERIFIED`; one approved scope does not approve another. Failures and cases needing an original-renderer comparison stay visible.

`model-audit-report.py` writes `model-audit-matrix.json` and CSV. It rejects an inventory for a different catalog and does not accept stale runtime evidence. It retains distinct `assetStatus`, `structureStatus`, `finiteSamplingStatus`, `runtimeStatus`, `renderedTechnicalStatus` and `visualFidelityStatus` fields.

Rendered evidence requires a measured `capture-manifest.json`, produced by `freeze-model-audit.py snapshot` before building and `finish --runtime <output>` after a successful build. The manifest binds the unchanged source dependency lists to the four shipped assembly hashes and small frozen model/companion catalogs. The merger validates those frozen catalog hashes and compares each source's asset payload, prepared motion, complete metadata, companion binding and fallback policy with the current catalog. An unrelated model export can therefore retain a source's valid capture. A changed model or applicable runtime dependency cannot. The original capture catalog hash remains in each matrix row; it is never rewritten to pretend an old capture was newly rendered.

Runtime source groups separate shared rendering/rig dependencies, pose evaluation, combat playback, and companion playback. The complete dependency list comes from the freeze tool, so omitting a file does not satisfy provenance. Changed or newly required dependencies invalidate their group; unaffected groups remain usable. Texture views require current rendering and pose evaluation, while complete motion also requires combat and any applicable companion playback. An older measured manifest's pose hash can be read from its exact recorded `ModelAnimator` entry in the combat group; a missing hash is never inferred. Older captures without a measured source manifest remain historical even if their images look correct.

A technical render pass requires every recorded frame for every required role, including `companion-walk` when applicable. Manual motion approval also requires the animation image among its evidence and explicit `reviewScope.roles`, `framesPerRole` and `animationFramesInspected` coverage. Texture approval requires both fitted views (`textureViewsInspected >= 2`) and their image among the evidence. Supplied `evidenceSha256` values must still match the reviewed files. Partial evidence can establish a concrete failure, but cannot establish an overall approval. Historical failures are retained in review records without being mislabeled as current failures after their model or runtime changes.

The generated `model-audit-visual-groups.json` records identical exported asset bytes, prepared-motion bytes, complete metadata and the companion policy (height, unit normalization, clip bindings and procedural effects). It is informational: it never authorizes review reuse. Reuse requires the capture's hash-matched renderer `identity-index.json` and an explicit `AliasOf` record with matching context signature and representative index; both source mappings must still match the current data and applicable runtime. Every original source key still gets its own matrix row. Different companion, normalization, rotation or material metadata prevents reuse even when two sources point to the same GLB.

These are diagnostic tools, not proof of continuous-time safety or automatic artistic correctness. Uniform full-cycle sampling can miss behavior between samples; transition checks cover only their recorded schedules. The final report must state remaining review coverage and any source/native comparison limitations.
