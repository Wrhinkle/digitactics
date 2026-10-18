"""Bundle notices from the exact NuGet packages used by a self-contained publish.

BRUTAL has its existing license copier in publish-native.ps1. This helper covers
the .NET runtime pack and other managed dependencies, without downloading or
rebuilding anything. --verify-only detects absent or changed packaged notices.
"""
from __future__ import annotations

import argparse
import base64
import hashlib
import json
from pathlib import Path, PurePosixPath
import re
import xml.etree.ElementTree as ET
from zipfile import ZipFile


TOOLS = Path(__file__).resolve().parent


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def load_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def safe_relative(value):
    path = PurePosixPath(value)
    if path.is_absolute() or any(part in (".", "..") or ":" in part or "\\" in part for part in path.parts):
        raise ValueError(f"Unsafe package path: {value}")
    return path


def gather(assets_file: Path, deps_file: Path, destination: Path):
    assets = load_json(assets_file)
    deps = load_json(deps_file)
    target = deps["targets"][deps["runtimeTarget"]["name"]]
    supplements = load_json(TOOLS / "managed-license-supplements.json")["supplements"]
    outputs = {}
    packages = []
    runtime_count = 0
    binary_count = 0
    for key, library in sorted(deps["libraries"].items()):
        if library["type"] not in ("package", "runtimepack") or key.lower().startswith("ahwoo.brutal."):
            continue
        runtime = library["type"] == "runtimepack"
        identity = key.removeprefix("runtimepack.") if runtime else key
        package_id, version = identity.split("/")
        safe_relative(identity)
        relative = identity.lower()
        roots = [Path(folder) / relative for folder in assets["packageFolders"]]
        root = next((path for path in roots if path.is_dir()), None)
        if root is None:
            raise ValueError(f"Resolved package is absent from restore folders: {identity}")
        archive_path = root / f"{package_id.lower()}.{version}.nupkg"
        archive_data = archive_path.read_bytes()
        archive_sha512 = base64.b64encode(hashlib.sha512(archive_data).digest()).decode("ascii")
        if archive_sha512 != archive_path.with_suffix(".nupkg.sha512").read_text(encoding="utf-8-sig").strip():
            raise ValueError(f"NuGet archive integrity mismatch: {identity}")
        restore_metadata = load_json(root / ".nupkg.metadata")
        if not runtime:
            resolved = assets["libraries"].get(identity)
            if not resolved or resolved["sha512"] != restore_metadata["contentHash"] or library["sha512"].removeprefix("sha512-") != resolved["sha512"]:
                raise ValueError(f"Published dependency does not match restored package: {identity}")
        with ZipFile(archive_path) as archive:
            names = archive.namelist()
            nuspec_name = next(name for name in names if name.lower() == package_id.lower() + ".nuspec")
            metadata = ET.fromstring(archive.read(nuspec_name)).find("{*}metadata")
            if metadata.findtext("{*}id").lower() != package_id.lower() or metadata.findtext("{*}version") != version:
                raise ValueError(f"NuGet identity mismatch: {identity}")
            license_element = metadata.find("{*}license")
            if license_element is None:
                raise ValueError(f"Package has no explicit license metadata: {identity}")
            repository_element = metadata.find("{*}repository")
            repository = repository_element.attrib if repository_element is not None else {}
            notices = {name: archive.read(name) for name in names
                       if "/" not in name and re.match(r"^(license|licence|notice|third.?party.?notices|copyright|patents)([._-]|$)", name, re.IGNORECASE)}
            if license_element.attrib.get("type") == "file":
                license_name = license_element.text
                safe_relative(license_name)
                notices[license_name] = archive.read(license_name)
            license_files = [name for name in notices if PurePosixPath(name).name.lower().startswith(("license", "licence"))]
            supplement = None
            if not license_files:
                supplement = supplements.get(repository.get("commit"))
                if (not supplement or supplement["repository"] != repository.get("url")
                        or license_element.attrib.get("type") != "expression" or license_element.text != "MIT"):
                    raise ValueError(f"Package license text needs an exact source supplement: {identity}")
                license_data = (TOOLS / safe_relative(supplement["path"])).read_bytes()
                if len(license_data) != supplement["size"] or sha256(license_data) != supplement["sha256"]:
                    raise ValueError(f"Pinned license supplement differs: {identity}")
                notices["LICENSE.TXT"] = license_data
            if not any("notice" in name.lower() for name in notices):
                raise ValueError(f"Package third-party notices are absent: {identity}")
            shipped = []
            for kind in ("runtime", "native", "resources"):
                for asset in target[key].get(kind, {}):
                    if asset.endswith("/_._"):
                        continue
                    safe_relative(asset)
                    filename = PurePosixPath(asset).name
                    if runtime:
                        candidates = [name for name in names if name.startswith("runtimes/") and PurePosixPath(name).name == filename]
                        if len(candidates) != 1:
                            raise ValueError(f"Ambiguous runtime pack asset: {identity}: {asset}")
                        source = candidates[0]
                    else:
                        source = asset
                    published_relative = str(PurePosixPath(PurePosixPath(asset).parent.name) / filename) if kind == "resources" else filename
                    published = destination / published_relative
                    original = archive.read(source)
                    if not published.is_file() or sha256(published.read_bytes()) != sha256(original):
                        raise ValueError(f"Published binary differs from exact resolved package: {identity}: {published_relative}")
                    shipped.append({"path": published_relative, "source": source, "sha256": sha256(original)})
                    binary_count += 1
            if not shipped:
                continue
            copied = []
            if runtime:
                runtime_count += 1
                if package_id.lower() != "microsoft.netcore.app.runtime.win-x64":
                    raise ValueError(f"Unexpected runtime pack requires explicit notice layout: {identity}")
                config = load_json(deps_file.with_name(deps_file.name.removesuffix(".deps.json") + ".runtimeconfig.json"))
                included = config["runtimeOptions"].get("includedFrameworks", [])
                if {"name": "Microsoft.NETCore.App", "version": version} not in included:
                    raise ValueError("Runtime config does not match the published runtime pack.")
            for source, data in sorted(notices.items()):
                safe_relative(source)
                if runtime:
                    output = "DOTNET_" + source
                else:
                    output = f"LICENSES/{package_id}-{version}/{source}"
                if output in outputs:
                    raise ValueError(f"Duplicate notice destination: {output}")
                outputs[output] = data
                copied.append({"path": output, "source": source, "size": len(data), "sha256": sha256(data)})
            packages.append({"id": package_id, "version": version, "type": library["type"],
                             "packageUrl": f"https://api.nuget.org/v3-flatcontainer/{relative}/{package_id.lower()}.{version}.nupkg",
                             "archiveSha256": sha256(archive_data), "archiveSize": len(archive_data),
                             "nugetContentHash": restore_metadata["contentHash"],
                             "licenseType": license_element.attrib.get("type"), "license": license_element.text,
                             "repository": repository, "licenseSupplement": supplement,
                             "files": copied, "publishedBinaries": shipped})
    if runtime_count != 1:
        raise ValueError("Expected exactly one published Microsoft.NETCore.App runtime pack.")
    manifest = {"schemaVersion": 1, "runtimeTarget": deps["runtimeTarget"]["name"], "packages": packages,
                "scope": ".NET runtime pack and non-BRUTAL managed NuGet dependencies; BRUTAL, Steam, CRT and audio have separate notices."}
    outputs["MANAGED_DEPENDENCY_NOTICES.json"] = (json.dumps(manifest, indent=2) + "\n").encode("utf-8")
    return outputs, len(packages), binary_count


def run(assets_file, deps_file, destination, verify_only=False):
    outputs, packages, binaries = gather(assets_file, deps_file, destination)
    for relative, contents in outputs.items():
        target = destination / safe_relative(relative)
        if verify_only:
            if not target.is_file() or target.read_bytes() != contents:
                raise ValueError(f"Missing or modified managed dependency notice: {relative}")
        else:
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(contents)
    return {"packages": packages, "binaries": binaries, "noticeFiles": len(outputs), "noticeBytes": sum(map(len, outputs.values()))}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--assets", type=Path, required=True)
    parser.add_argument("--deps", type=Path, required=True)
    parser.add_argument("--destination", type=Path, required=True)
    parser.add_argument("--verify-only", action="store_true")
    args = parser.parse_args()
    result = run(args.assets, args.deps, args.destination, args.verify_only)
    print(f"Managed notices {'verified' if args.verify_only else 'copied'}: {result['packages']} exact packages, {result['binaries']} verified binaries, {result['noticeFiles']} files ({result['noticeBytes']} bytes).")


if __name__ == "__main__":
    main()
