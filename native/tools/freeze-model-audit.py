"""Freeze only audit provenance/catalogs, never duplicate model assets or approve visuals.

Run snapshot before the build, then finish after a successful build. Finish refuses
source changes during that interval, so its source hashes describe the built runtime.
"""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GROUPS = {
    "renderer": [
        "Assets/GltfAsset.cs", "Assets/GltfMaterial.cs", "Assets/GltfRig.cs",
        "Assets/GltfMesh.cs", "Assets/Matrix4D.cs", "Assets/PngImage.cs", "Assets/ModelCatalog.cs",
        "Desktop/ModelRenderer.cs", "Desktop/ModelRenderer.Transparency.cs",
        "Desktop/GpuModel.cs", "Desktop/GpuTexture.cs", "Desktop/GpuPrimitive.cs",
    ],
    "capture": ["Desktop/ModelVisualAudit.cs"],
    "board": ["Desktop/ArenaCamera.cs", "Desktop/BattleView.cs", "Desktop/ArenaEnvironment.cs",
              "Presentation/CombatScene.cs", "Desktop/GpuModel.cs"],
    "pose": ["Assets/ModelAnimator.cs"],
    "combat": [
        "Assets/CombatModelAnimator.cs", "Assets/CompanionLocomotion.cs", "Presentation/CombatScene.cs",
        "Presentation/CombatChoreography.cs", "Desktop/CombatModelAnimations.cs",
    ],
    "companion": [
        "Assets/CompanionAnimator.cs", "Assets/CompanionMotion.cs",
        "Assets/CompanionLocomotion.cs", "Assets/CompanionEmoteMotion.cs",
        "Assets/CompanionCatalog.cs",
    ],
}


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def source_paths():
    result = {group: ["native/src/DigiTactics." + name for name in files] for group, files in GROUPS.items()}
    # Simulation details select real movement samples. Keep all rules sources, not
    # a guessed subset that can silently miss a future movement dependency.
    result["combat"].extend(path.relative_to(ROOT).as_posix() for path in sorted((ROOT / "native/src/DigiTactics.Rules").glob("*.cs")))
    return result


def sources():
    return {group: {name: sha(ROOT / name) for name in names} for group, names in source_paths().items()}


def validate_payloads(runtime):
    """Reject a build copied while an exporter was replacing its prepared inputs."""
    catalog = json.loads((runtime / "content/models.json").read_text(encoding="utf-8"))
    files = {asset["file"] for asset in catalog["assets"].values()}
    files.update(source["motion"]["file"] for source in catalog["sources"].values() if source.get("motion"))
    missing = [name for name in sorted(files) if not (runtime / "assets" / name).is_file()]
    if missing:
        raise ValueError("Incomplete prepared runtime; wait for exporters and rebuild before freezing: " + ", ".join(missing))
    return len(files)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("mode", choices=("snapshot", "finish"))
    parser.add_argument("directory", type=Path)
    parser.add_argument("--runtime", type=Path)
    args = parser.parse_args()
    args.directory.mkdir(parents=True, exist_ok=True)
    snapshot = args.directory / "build-source-snapshot.json"
    current = sources()
    if args.mode == "snapshot":
        if (args.directory / "capture-manifest.json").exists():
            raise SystemExit("Capture already frozen; choose a new evidence directory.")
        snapshot.write_text(json.dumps(current, indent=2) + "\n", encoding="utf-8")
        print(f"Recorded {sum(map(len, current.values()))} source hashes before build.")
        return
    if args.runtime is None:
        parser.error("finish requires --runtime after a successful build")
    before = json.loads(snapshot.read_text(encoding="utf-8"))
    if before != current:
        changed = [key for group, values in current.items() for key, value in values.items() if before.get(group, {}).get(key) != value]
        raise SystemExit("Sources changed during build; repeat snapshot/build: " + ", ".join(changed))
    try:
        payload_count = validate_payloads(args.runtime)
    except ValueError as error:
        raise SystemExit(str(error)) from error
    assemblies = {name: sha(args.runtime / name) for name in ("DigiTactics.Desktop.dll", "DigiTactics.Assets.dll", "DigiTactics.Presentation.dll", "DigiTactics.Rules.dll")}
    catalogs = {}
    for name in ("models.json", "companion-models.json"):
        original = args.runtime / "content" / name
        target = args.directory / ("frozen-" + name)
        target.write_bytes(original.read_bytes())
        catalogs[name] = {"file": target.name, "sha256": sha(target)}
    manifest = {"schemaVersion": 1, "sourceStableBeforeAfterBuild": True,
                "policy": "ModelAnimator+CombatModelAnimator+CombatChoreography+CompanionAnimator/v3",
                "sourceGroups": current, "runtimeHashes": assemblies, "frozenCatalogs": catalogs}
    (args.directory / "capture-manifest.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(f"Frozen source, assembly and catalog provenance; {payload_count} prepared payloads present; no visual status changed.")


if __name__ == "__main__":
    main()
