"""Verify the clean installation root and the original Windows executable icons."""
from pathlib import Path
import argparse
import json
import runpy
import struct

PE = runpy.run_path(str(Path(__file__).with_name("verify-native-dependencies.py")))["PE"]
ICON = Path(__file__).resolve().parents[2] / "icons/digitactics.ico"


def icon_images(data):
    reserved, kind, count = struct.unpack_from("<HHH", data)
    if reserved or kind != 1 or count != 7:
        raise ValueError("Expected the seven-size original application icon.")
    images = []
    for index in range(count):
        size, offset = struct.unpack_from("<II", data, 6 + index * 16 + 8)
        image = data[offset:offset + size]
        if len(image) != size:
            raise ValueError("Truncated icon image.")
        images.append(image)
    return images


def verify_icon(path):
    pe = PE(path)
    address, size = pe.directories[2]
    if not address or not size:
        raise ValueError(f"Executable has no icon resources: {path}")
    base = pe.offset(address)
    resources = {}

    def walk(relative, names=()):
        if len(names) > 3:
            raise ValueError("Unexpected resource tree depth.")
        named, numeric = pe.unpack("<HH", base + relative + 12)
        for index in range(named + numeric):
            name, target = pe.unpack("<II", base + relative + 16 + index * 8)
            key = names + (name,)
            if target & 0x80000000:
                walk(target & 0x7fffffff, key)
            else:
                rva, length, _, _ = pe.unpack("<IIII", base + target)
                offset = pe.offset(rva)
                resources[key] = pe.data[offset:offset + length]

    walk(0)
    expected = icon_images(ICON.read_bytes())
    groups = [(key, value) for key, value in resources.items() if key[0] == 14]
    if not groups:
        raise ValueError(f"Executable has no Windows group icon: {path}")
    for key, group in groups:
        reserved, kind, count = struct.unpack_from("<HHH", group)
        if reserved or kind != 1 or count != len(expected):
            raise ValueError(f"Unexpected executable icon group: {path}")
        actual = []
        for index in range(count):
            length, identity = struct.unpack_from("<IH", group, 6 + index * 14 + 8)
            image = resources.get((3, identity, key[2]))
            if image is None or len(image) != length:
                raise ValueError(f"Missing Windows icon image: {path}")
            actual.append(image)
        if actual != expected:
            raise ValueError(f"Executable icon differs from the original application artwork: {path}")
    print(f"Original seven-size Windows icon verified: {path.name}")


def verify_package(directory):
    expected = {"DigiTactics.Launcher.exe", "START-HERE.txt", "native-launcher.json", "app", "launcher"}
    if {item.name for item in directory.iterdir()} != expected:
        raise ValueError("Installation root must contain only the launcher, start guide, configuration, app/ and launcher/.")
    runtime = directory / "launcher"
    for name in ("DigiTactics.Launcher.dll", "DigiTactics.Launcher.deps.json", "DigiTactics.Launcher.runtimeconfig.json",
                 "hostfxr.dll", "coreclr.dll", "DOTNET_LICENSE.TXT", "DOTNET_THIRD-PARTY-NOTICES.TXT", "MANAGED_DEPENDENCY_NOTICES.json"):
        if not (runtime / name).is_file():
            raise ValueError(f"Launcher runtime/notice missing: {name}")
    if (runtime / "DigiTactics.Launcher.exe").exists():
        raise ValueError("Duplicate launcher executable obscures the player entrypoint.")
    settings = json.loads((directory / "native-launcher.json").read_text(encoding="utf-8-sig"))
    if settings["schemaVersion"] != 1:
        raise ValueError("Unexpected launcher configuration.")
    for executable in (directory / "DigiTactics.Launcher.exe", directory / "app/DigiTactics.Desktop.exe"):
        verify_icon(executable)
    print("Clean native layout verified: one root launcher; dependencies/notices in launcher/ and app/.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    group = parser.add_mutually_exclusive_group(required=True)
    group.add_argument("--package", type=Path)
    group.add_argument("--executables", type=Path, nargs="+")
    args = parser.parse_args()
    if args.package:
        verify_package(args.package)
    else:
        for executable in args.executables:
            verify_icon(executable)
