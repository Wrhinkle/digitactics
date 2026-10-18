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

Time Stranger Tentomon also retains its correct embedded atlas and all 104 wing
triangles/UVs. Its wing material had no texture binding and a saturated yellow
tint. The same tool restores the source DAE's explicit diffuse binding and a
neutral texture multiplier; the complete binary chunk stays unchanged. The
untextured eye material is preserved because its source DAE is also untextured.

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
UV orientation against surviving source UVs. Meshes that lost their UVs need
vertex duplication at original texture seams. After GLB
serialization, every original per-triangle position, normal, skin joint, skin
weight and surviving UV remains bit-identical. Node, skin and animation metadata
remain unchanged. It rejects changed archives, changed originals, ambiguous
triangle matches and changed geometry. The source PNG is embedded unchanged.

Goldramon, Grademon, Megidramon and Neptunemon use the same source recovery path.
Each surviving mesh's original DAE material determines its atlas; material names
are not used to guess colors. Grademon needs two atlases and Megidramon needs
three, so wrongly shared materials are split. Goldramon's three coincident-position
triangles are distinguished by their surviving UVs. The DAE diffuse semantic
is resolved through `bind_vertex_input` to its exact UV set; an additional UV
set never silently replaces it. Missing UVs are restored only where every
source triangle matches uniquely. All recipes pin the output hash and reject
changed inputs, source bindings, or output drift.

```powershell
python native/tools/recover-model-materials.py --model goldramon --archive C:/path/324339.zip --apply
python native/tools/recover-model-materials.py --model grademon --archive C:/path/324410.zip --apply
python native/tools/recover-model-materials.py --model megidramon --archive C:/path/324340.zip --apply
python native/tools/recover-model-materials.py --model neptunemon --archive C:/path/324351.zip --apply
```

After any source edit, coordinate with other artifact consumers before running
`export-models.cjs`, then `export-motions.cjs`. The model exporter replaces the
catalog and prunes old generated payloads, so it must not overlap an application
copy/publish. Refresh companion playback only when the motion source changed.
Inspect new fitted front/back captures before recording a material fix as
visually verified; numeric preservation checks do not establish visual quality.

Palmon's Time Stranger import had a separate geometry defect: the historical
`repair-catalog.mjs` removed `chr348.001` to hide parked attack vines, also
removing the flower from the same mesh. `recover-model-geometry.py` restores the
original 1,822 flower triangles, UVs and skin weights from pinned archive
`606444.zip`. Every selected corner is weighted entirely to the original head,
hair or flower bones. The 1,780 vine triangles are disjoint; no mixed triangles
are guessed or discarded. The existing rig, animations, decoded vertex data and
other meshes remain unchanged. The corrected import cleanup preserves the same
flower partition on future conversions.

```powershell
python native/tools/recover-model-geometry.py --model palmon --archive C:/path/606444.zip --apply
```

The restored flower has its own visual acceptance. It does not establish eye
shader fidelity or completeness of the missing vine attack. The source archive
retains the original vine geometry and weights for a separate visibility and
choreography review; the flower repair does not fabricate an attack animation.
The manifest records these outstanding scopes alongside the exact repaired hash.

ToyAgumon's Time Stranger import similarly dropped `chr708.003`, which contains
the torso, forearms and left foot. The original mesh has 5,660 triangles and no
`J_fire` weights. The same recovery tool restores it with the original atlas,
UVs and weights; the import cleanup now keeps it. The 302 parked fire triangles
in the separate surviving body mesh remain excluded by their original weights.
The existing quantized skin frame is checked against the retained body geometry.
Its existing spine bind/rest offset of 0.645 mm is preserved, not corrected by
moving recovered vertices or replacing the rig.

```powershell
python native/tools/recover-model-geometry.py --model toyagumon --archive C:/path/603392.zip --apply
```

Both geometry recipes reject changed archives, changed originals and any output
that differs from the pinned recovered bytes. Recovery validates all retained
decoded attributes, rig/animation metadata, source weights and source material
assignments. Normal builds verify only the checked-in asset hashes.

Leopardmon X's Links import uses the correct original atlases but inverted V
coordinates. `recover-model-uv.py` matches all 11,982 original DAE triangles and
their explicit diffuse UV bindings. It preserves U and negates V on only the
three reviewed UV accessors. All 272 decoded accessors are checked; other data,
original binary bytes, images, materials, rig and animations are retained.
The optional tool writes a pinned candidate and proof report without replacing
the production asset. Its archive, original, source transforms, diffuse bindings
and output are all checked before writing. The tracked repaired asset is checked
by the normal `repair-model-materials.py --check` command.

```powershell
python native/tools/recover-model-uv.py --model leopardmonx --archive C:/path/310676.zip --original C:/path/original.glb --output-dir native/artifacts/leopardmonx-recovery
```

Five Digimon World imports had explicit transparent pixels in their embedded
color images but omitted material alpha mode: Birdramon, Cherrymon, DemiMeramon,
Patamon and Poyomon. The reviewed transparent areas follow the original flame,
leaf, ear and body-fringe boundaries. Their pinned `alphaRepairs` add only
`alphaMode: MASK` and `alphaCutoff: 0.5`; every image and binary chunk remains
unchanged. Poyomon also has alpha 254 pixels, which remain visible. These exact
asset repairs do not infer transparency from black RGB or modify other models.
`repair-model-materials.py --apply` reproduces them from their original hashes;
`--check` requires the exact repaired bytes. Source geometry and animation quality
remain separate review scopes.

Gammamon has a separate reviewed functional correction in `alphaRepairs`: its
global 0.6 material opacity exposed the opposite face through its head and body.
The original source PNG is binary alpha (109 transparent pixels, 1,048,467 opaque),
while source exports disagree about global opacity: FBX/DAE use 0.5, but OBJ/MTL
uses image opacity only. The exact-asset correction sets the global factor to 1
and uses `MASK` with cutoff 0.5, preserving the embedded image and its cutouts.
This is documented as a functional correction rather than strict reproduction
of the inconsistent FBX material. Other color factors and material properties
remain unchanged.

Tsunomon's Digimon World archive includes `Tsunomon.mtl`, but its OBJ references
the nonexistent `untitled.mtl`. The imported GLB consequently lost its diffuse
texture and UVs. `recover-model-obj-materials.py` matches all 150 triangles to the
original OBJ and restores its 233 UV vertices and exact `Tsunomon.png` bytes.
The original white diffuse factor comes from the supplied MTL. Every existing
attribute and index remains bit-identical; no seam duplication is needed.
The helper rejects changed source archives, members, originals and outputs and
writes a candidate only. Normal builds verify the pinned repaired asset.

```powershell
python native/tools/recover-model-obj-materials.py --model dw1_tsunomon --archive C:/path/334142.zip --original C:/path/original.glb
```

Sukamon's import selected the supplied PlatinumSukamon image. The original DAE's
material name mentions `chr752`, but its actual diffuse sampler resolves to the
normal `chr313a01.png`. `recover-model-source-atlas.py` follows that full binding
and checks all 2,604 surviving UV triangles before appending the exact source
image and changing only the diffuse index. All 185 existing accessors, image
bytes, rig and animation remain unchanged. The tool writes a pinned candidate.

```powershell
python native/tools/recover-model-source-atlas.py --model sukamon --archive C:/path/304086.zip --original C:/path/original.glb
```

`drg:Ravemon` previously selected the archive's `chr778` Burst Mode geometry
despite declaring the normal form. Its model and portrait now reuse the existing
normal `chr740` asset, also used by `catalog:CS_Ravemon:chr740`. The separately
named Burst Mode remains available. The manifest's `mappingCorrections` pins
the normal asset, mapping and portrait; normal checks detect either model or
portrait drift. This avoids inventing transparency rules for the mistakenly
selected Burst effect cards.

MachGaogamon's four sonic jet meshes lost the original FBX `COLOR_0` opacity
streams. `recover-model-vertex-colors.py` restores all 216 source RGBA vertices
after matching 320 triangles by position, UV and winding. The exact source
levels are 0, 64/255, 128/255, 191/255 and 1; RGB remains white. All 179 original
accessors, compressed data, images, geometry, rig and animation remain unchanged.
The sonic material uses `BLEND`; separate opaque glow cards are untouched and
still require their own source blending investigation.

The native loader supports float and normalized unsigned byte/short RGB/RGBA.
It preserves the legacy path for absent/RGB colors, and resolves the source
cutout correction separately for RGBA primitives so a shared material cannot
erase authored opacity. OPAQUE ignores vertex alpha; MASK and BLEND use it.
The source browser correction follows the same rule. CPU contracts, shared
material tests and `--model-vertex-colors-smoke` cover these behaviors. The
[glTF specification](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#meshes-overview)
defines the supported color accessors and alpha modes.

MagnaGarurumon's body material recovery restores two missing UV streams and
five original diffuse assignments. `recover-model-body-materials.py` uniquely
matches all 11,300 body triangles using positions, surviving UVs and preserved
winding, including coincident front/back faces. It appends the exact original
`chr675a01.png` and `chr675a02.png` atlases and material variants; all 398 old
accessors, original primitive indices, rig and animation remain unchanged.
The already correct body mesh and sword/effect meshes are preserved. Plain
white sword imagery is supplied by the source and is not recolored.

Both helpers produce isolated candidates and enforce pinned archive, member,
original and output hashes. Their `sourceRecoveries` entries gate normal builds;
source correspondence alone is not native visual acceptance.

```powershell
python native/tools/recover-model-vertex-colors.py --model machgaogamon --archive C:/path/304020.zip --original C:/path/original.glb
python native/tools/recover-model-body-materials.py --model magnagarurumon --archive C:/path/331597.zip --original C:/path/original.glb
```

Source archive pages:

- [PileVolcamon](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324353/)
- [Triceramon](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324322/)
- [Palmon](https://models.spriters-resource.com/pc_computer/digimonstorytimestranger/asset/606444/)
- [ToyAgumon](https://models.spriters-resource.com/pc_computer/digimonstorytimestranger/asset/603392/)
- [Tentomon](https://models.spriters-resource.com/pc_computer/digimonstorytimestranger/asset/603374/)
- [Goldramon](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324339/)
- [Grademon](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324410/)
- [Megidramon](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324340/)
- [Neptunemon](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324351/)
- [Leopardmon X](https://models.spriters-resource.com/mobile/digimonlinks/asset/310676/)
- [Tsunomon](https://models.spriters-resource.com/playstation/digimonworld/asset/334142/)
- [Sukamon / PlatinumSukamon](https://models.spriters-resource.com/mobile/digimonlinks/asset/304086/)
- [Ravemon / Burst Mode](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324581/)
- [MachGaogamon](https://models.spriters-resource.com/mobile/digimonlinks/asset/304020/)
- [MagnaGarurumon](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/331597/)
