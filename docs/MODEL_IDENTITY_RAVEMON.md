# Ravemon form mapping correction

The source key `drg:Ravemon` declares the normal Ravemon form, but originally
mapped to `models/files/rip_cs_ravemon.glb`. That asset contains the `chr778`
Burst Mode meshes, magenta feathers and glow cards. This is a form-selection
error; the opaque glow-card appearance is not evidence that black texture pixels
should be discarded across unrelated models.

The [original Cyber Sleuth archive](https://models.spriters-resource.com/pc_computer/digimonstorycybersleuthcompleteedition/asset/324581/)
contains both `Ravemon/chr740` and `Ravemon/Burst Mode/chr778` source models.
Its SHA256 is `91b84a1c423663da61a7d3bf2d91e43ce74d2af565ea2483c7235beddc157e0a`.
The normal DAE SHA256 is
`56643839ec6b8817bb685fde301f73cdd73453b08a1f8d92aa18c542da2dcac5`;
the Burst DAE SHA256 is
`1f6ff1d88f95498128087d41f4c1db0ad4aafd024c775a6ac82854790d92ce8f`.
The original import inventory also lists their separate `chr740` and `chr778`
FBX inputs, despite associating the broad folder name with both outputs.

The incorrect asset SHA256 is
`b01388965bf9f8bb77241a035eb9ffeb865e80efc0457659feed29aa69619434`.
Its meshes/materials use `chr778`, `chr778a01` and `eff_lig_04b` from the Burst
source. Frozen native fitted views at
`native/artifacts/model-visual-audit-v3/0499.texture.png` show that form.

The correction reuses the existing normal asset
`models/files/rip_cat_cs_ravemon_chr740.glb`, SHA256
`f56e6805e6f837890955cabec82b457969d9aed688ac480b15d7f8a24a91ae70`.
It contains `chr740_0` through `chr740_3` and the normal `chr740a01` atlas.
Its existing independent source key is `catalog:CS_Ravemon:chr740`; the frozen
front/back views at `native/artifacts/model-visual-audit-v3/0215.texture.png`
show the normal black/white feathered body, red head details and sword.
The model and portrait indexes now point `drg:Ravemon` to this same normal asset
and its existing matching portrait. No mesh, material or animation is fabricated
or copied between incompatible rigs. Separately named Burst variants are kept.

`native/tools/model-material-repairs.json` pins the corrected mapping, asset and
portrait. The original identity failure remains tied to the original mapping
and exact asset hashes. Fresh exported animation and fitted visual acceptance
of the corrected source key remain separate from this identity correction.
