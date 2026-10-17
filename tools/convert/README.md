# Model archive inventory

`catalog-inventory.json` audits locally downloaded creature archives; it is not an inventory of every asset released for those games. `unique-imports.json`, `catalog-imports.json` and `skin-imports.json` record added forms, game-specific meshes and texture variants. Import manifests retain their original discovery status; rerun `node tools/convert/audit-catalog.cjs` for current coverage after generating credits. Alternate game models are not separate species.

Original archives are ignored and must be obtained separately from the credited sources. `static-rigs.json` and `catalog-rigs.json` preserve manual rig settings. `assign-catalog.cjs` generates the assignments across sets. `patch-textures.cjs` handles the source material conventions; `repair-catalog.mjs` removes parked effect geometry from specific models. Use explicit file filters with conversion, optimization and portrait tools when changing a subset.

The mystery-box browser flow runs with `node tests/mystery-loot.cjs <playwright-package-path> <evidence-directory>` from the repository root. It exercises combat drops, opening and equipping rewards, persistence, hidden collection items and production previews. Model renderer checks are in `tests/model-motion.cjs`.

Run packaged smoke checks with `DT_USERDATA` pointing to an isolated test profile; `DT_SMOKE` alone does not isolate saves. The smoke command must pass its native-module check even when Steam itself is offline.
