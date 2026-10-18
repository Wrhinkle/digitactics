# Aegiomon model identity defect

Status: **FAIL — wrong form assigned; replacement not yet available.** Reviewed 2026-10-18.

`ac:Aegiomon` is used for the Champion Aegiomon stage in Auto Chess, Coliseum,
Reckoning and Underlords. It currently resolves to
`models/files/rip_ts_aegiomon.glb`, whose mesh names are `chr563`, `chr563.001`
and `chr563.002`. The model is an orange quadruped with a fan of tails.

The import map `tools/convert/maps/autochess-45.json` explicitly aliases
`TS_Aegiomon` to `TS_Elecmon_Elecmon_Aegiomon`, selects `chr563.fbx`, and labels
the result `Aegiomon`. The archive inventory records that FBX with SHA-256
`7c42a17f736a9e7f28862a52a767dcac541fe24f55287c0372fba185b8d39728`.
This is an original source-mapping error, not a native bone or texture failure.

The credited [Models Resource asset 604898](https://models.spriters-resource.com/pc_computer/digimonstorytimestranger/asset/604898/)
is titled **Elecmon (+ Elecmon Aegiomon)**, is filed under Rookie Digimon, and
contains `chr563` and `chr564`; its `chr563` texture folder is labelled
`Coo-chan Textures`. That source does not establish the shipped quadruped as the
Champion form.

The [official Digimon Encyclopedia](https://digimon.net/reference_en/detail.php?directory_name=aegiomon)
classifies Aegiomon as Champion and depicts a humanoid upper body with goat-like
legs, horns, white hair, gloves and a red scarf. The
[official Time Stranger character page](https://en.bandainamcoent.eu/digimon/digimon-story-time-stranger/characters/aegiomon)
also shows the humanoid character. Both were compared with the fitted native
front/back evidence in `native/artifacts/model-visual-audit-v2/0000.texture.png`.

No correctly identified Champion Aegiomon asset was found in the local model
catalog. The currently credited Time Stranger model listing contains Baby and
Rookie assets, including the Elecmon disguise. Search results for unrelated
mods and print meshes were not treated as validated replacements. The current
mapping remains a known defect until a compatible, correctly identified model
is obtained and its rig, materials, actions and in-game presentation are tested.
Changing the label to Elecmon, substituting another species, or accepting parity
with the old client would not fix the intended Aegiomon stage.

Machine-readable provenance and the reviewed reference images are retained in
`native/artifacts/model-identity-aegiomon/identity-review.json`. These reference
images are audit evidence only and are not distribution assets.
