# Digi-Tactics credits and acknowledgements

Digi-Tactics is an unofficial, non-commercial fan project created by William, with AI-assisted programming and illustration. No affiliation, endorsement, ownership of the Digimon franchise, or redistribution permission is implied by these credits.

## Characters, original games and artists

Thanks to the character designers, original modelers, texture artists, riggers, animators and development teams behind Digimon. The original artists are distinct from the community members who extracted or uploaded their work.

The model inventory records assets originating from **Digimon Links**, **Digimon ReArise**, **Digimon Story Cyber Sleuth / Complete Edition**, **Digimon Story: Time Stranger**, **Digimon Masters**, **Digimon New Century**, **Digimon World**, and **Digimon World: Next Order**. Each available source page and the actual bundled filenames are linked in [the model credits](docs/credits/MODELS.md). Those original games' staff credits remain the authoritative record of their production teams; individual original-modeler assignments are not established by the extraction metadata.

Digimon character and franchise rights remain with their respective owners. The [official Digimon portal](https://digimon.net/) and [official game portal](https://digimon-gameportal.bn-ent.net/) identify the franchise and original works. Akiyoshi Hongo, Bandai / Bandai Namco, Toei Animation and the relevant original game developers and rights holders are acknowledged here. The project does not grant a license to their characters, models, textures or animations.

## Community model contributions

The credits are generated from the **actual bundled GLB files**, including unused files still present in the distribution, rather than only the current roster. [MODEL credits](docs/credits/MODELS.md) list source pages, uploaders/extractors where verified, recorded license labels and files. [The inventory](docs/credits/model-assets.json) also maps model keys and records unresolved attribution and permission status. [Contributor evidence](docs/credits/contributors.json) records the checked source or archive evidence.

The Models Resource and Sketchfab provided source hosting. Contributors are credited for their documented upload, extraction or model contribution, not automatically as the original artist or copyright owner. “Game rip” describes provenance; it is not a license. Source upload license labels do not settle rights in underlying game content. Source-page credits were checked on October 16, 2026; unavailable information is marked rather than invented.

Conversions and project adaptations include GLB conversion, texture optimization, material fixes, orientation and scale corrections, animation selection/retargeting, procedural poses and dances, and rendered portraits. These changes do not transfer rights in the originals.

Rigged models without usable source clips use project-generated skeletal idle, attack, special and victory motions. Existing authored clips retain their original attribution. Restored eye textures come from the same credited source archives as their models.

## Mechanics and design references

The game's presentation uses **Undermons** and **Auto Chess**. The following acknowledges the actual design references, separately from game copy:

| Digi-Tactics content | Design reference and contribution |
| --- | --- |
| Faction Wars | Teamfight Tactics Set 1: factions, roles, shared shops, economy, board positioning and three-copy upgrades; additional Auto Chess piece concepts |
| Galaxies, Fates, Reckoning, Gizmos & Gadgets, Dragonlands, Into the Arcane, K.O. Coliseum | Corresponding Teamfight Tactics sets: set themes, traits and set-specific systems, adapted into Digimon rosters and this combat engine |
| Undermons / Undermons Knockout | Dota Underlords: alliances, tiered items, Tamer-equivalent board leaders, progression and Knockout pacing |
| Auto Chess | The original Dota Auto Chess and standalone Auto Chess: races/classes, Druids/Wizards, economy, creep loot and item recipes |
| Duos | Cooperative autobattler team formats, adapted here into shared team health, partner portals and simultaneous combined-board combat |
| Final attack / victory dance | Project implementation using the actual final-hit combat record and the winners' models; procedural camera, effects and dance choreography |

Acknowledgements to the original design teams at Riot Games, Valve and Drodo Studio. These credits are not an endorsement or a claim that the adapted implementation is their software. Detailed research, source links and rule mappings remain in [docs/inspo](docs/inspo/) and [docs/sets](docs/sets/).

## Illustration, interface, sound and tools

- Client and mode illustrations: AI-assisted fan artwork coordinated through the recovered Claude/Astra workflow; the set-art prompts, reference roles and asset hashes are in [the art manifest](docs/art/set-carousel-art.json). The GitHub cover uses built-in image generation; its prompt and provenance are in [media/README.md](media/README.md).
- Character portraits and item previews: rendered by the project's tools from the credited models or procedural objects. They inherit any underlying asset-rights limitations.
- Boards, UI, particles, move effects and procedural dance layers: project code. Existing model animation clips retain their original asset attribution.
- Built-in music, effects, elimination sounds and soundboard cues: synthesized by project code. User-imported music is local user content, not part of the release. Trailer music: **“Tamer's Morning WIP” by mikl_pls**, supplied by William (2026-10-16 1335 version). Trailer purchase ticks, evolution chime and transition swishes are original synthesized editorial cues. The source WAV is not bundled with the game.
- Three.js and its contributors: 3D rendering and loaders (MIT); meshoptimizer and Arseny Kapoulkine: model decoding (MIT).
- Electron and its contributors, Chromium and Node.js: desktop runtime; steamworks.js contributors: native integration. Their licenses/notices are preserved in the package and [THIRD_PARTY_NOTICES.txt](THIRD_PARTY_NOTICES.txt). Steamworks service/SDK use is separately governed by its provider's terms; the wrapper's MIT license does not license the service.
- Inter by Rasmus Andersson and contributors; Chakra Petch by Cadson Demak: interface typefaces (SIL Open Font License). System fallback fonts are provided by the operating system.
- Firebase SDK/services support the optional web transport. Playwright, esbuild, glTF Transform, Meshopt, Blender, FBX2glTF and FFmpeg support development, conversion, QA and media production; their use does not imply those tools' authors endorse this project.
- Claude and Codex assisted development, research, testing and asset coordination. AI-generated artwork and code are identified as such rather than presented as the work of the original game's artists.

## Permission status and corrections

This is a free fan project, made for fun with no profit intended or sought. These credits record attribution; they do not establish permission to redistribute third-party work. The [Models Resource terms](https://models.spriters-resource.com/page/tou/) and the original rights holders' rights still apply. Some contributor names remain unverified where source pages and archive notes were unavailable. Those gaps are explicitly marked in the inventory rather than assigned to an assumed author; the project does not claim that every original artist has been identified or that all asset permissions have been cleared.

For attribution corrections or removal requests, contact the repository owner through a GitHub issue with the affected asset path and source. Credits and permission evidence should be retained in downstream versions.
