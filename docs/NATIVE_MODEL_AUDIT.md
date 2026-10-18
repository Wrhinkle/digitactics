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

The visual audit writes `native/artifacts/model-visual-audit/NNNN.json` and PNG contact sheets. Per-frame draw and pixel counts establish only a technical render. Reviewed appearance and animation decisions belong in `reviews.json`, keyed by `sourceKey`, with `visualFidelityStatus`, evidence paths and a concrete explanation. An evidence-free approval is ignored. Failures and cases needing an original-renderer comparison stay visible.

`model-audit-report.py` writes `model-audit-matrix.json` and CSV. It rejects an inventory for a different catalog and does not accept stale runtime evidence. It retains distinct `assetStatus`, `structureStatus`, `finiteSamplingStatus`, `runtimeStatus`, `renderedTechnicalStatus` and `visualFidelityStatus` fields.

The generated `model-audit-visual-groups.json` allows rendered evidence reuse only for identical exported asset bytes, prepared-motion bytes and complete metadata. Every original source key still gets its own matrix row. Different normalization, rotation or material metadata prevents reuse even when two sources point to the same GLB.

These are diagnostic tools, not proof of continuous-time safety or automatic artistic correctness. Uniform full-cycle sampling can miss behavior between samples; transition checks cover only their recorded schedules. The final report must state remaining review coverage and any source/native comparison limitations.
