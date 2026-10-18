# Reviewed source material repairs

The native and browser clients consume the same tracked `models/files/*.glb`
assets. Some older FBX conversions bound a small reflection/specular gradient as
base color. These repairs restore the original color atlas; they do not infer a
palette from a character name. A filename containing `spec` is only a candidate
for investigation, not sufficient evidence to change a material.

`model-material-repairs.json` pins the exact original and repaired bytes. Run
`python native/tools/repair-model-materials.py --check` to verify the tracked
repairs. This check uses only Python's standard library and needs no downloads.

Tankmon, Agunimon and EmperorGreymon already contained their correct atlases and
UVs. `repair-model-materials.py --apply` reproduces those three exact texture-index
changes from the pinned originals. Their binary chunks are unchanged. Separate
EmperorGreymon cube/reflection surfaces are untouched: their lack of UVs alone
does not establish a visual defect.

PileVolcamon and Triceramon lacked their color atlas. Their exact public source
archives are identified in `recover-model-materials.py` and in the recovery
records in the manifest. This optional tool requires Node, NumPy and SciPy;
ordinary game builds do not. Download the archive from its recorded source page,
then run, for example:

```powershell
python native/tools/recover-model-materials.py --model pilevolcamon --archive C:/path/324353.zip --apply
python native/tools/recover-model-materials.py --model triceramon --archive C:/path/324322.zip --apply
```

The input GLB must match the recorded original hash. After applying a repair,
`--original C:/path/original.glb` can reproduce and verify it again without
replacing the tracked file. Original bytes are retained under ignored
`native/artifacts/model-material-originals/` when applying. Existing backups
with different bytes are rejected.

The recovery tool verifies each surviving mesh's original DAE diffuse assignment
and matches every triangle uniquely against the quantized GLB. It verifies the
UV orientation against surviving source UVs. Only PileVolcamon's two meshes that
lost their UVs need vertex duplication at original texture seams. After GLB
serialization, every original per-triangle position, normal, skin joint, skin
weight and surviving UV remains bit-identical. Node, skin and animation metadata
remain unchanged. It rejects changed archives, changed originals, ambiguous
triangle matches and changed geometry. The source PNG is embedded unchanged.

After any source edit, coordinate with other artifact consumers before running
`export-models.cjs`, then `export-motions.cjs`. The model exporter replaces the
catalog and prunes old generated payloads, so it must not overlap an application
copy/publish. Refresh companion playback only when the motion source changed.
Inspect new fitted front/back captures before recording a material fix as
visually verified; numeric preservation checks do not establish visual quality.

Source archive pages:

- [PileVolcamon](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324353/)
- [Triceramon](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324322/)
