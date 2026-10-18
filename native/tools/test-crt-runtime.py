"""Exercise CRT extraction and package dependency rejection using small fixtures.

Run after setup-crt and a Desktop publish. Only the four BRUTAL libraries and
their CRT files are copied; the full application payload is never duplicated.
"""
from __future__ import annotations

import argparse
from contextlib import contextmanager
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
import uuid


TOOLS = Path(__file__).resolve().parent
NATIVE = TOOLS.parent
spec = importlib.util.spec_from_file_location("native_dependencies", TOOLS / "verify-native-dependencies.py")
dependencies = importlib.util.module_from_spec(spec)
spec.loader.exec_module(dependencies)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("package_directory", type=Path)
    parser.add_argument("--compare-pefile", action="store_true", help="Also compare every import table against the independent pefile package.")
    args = parser.parse_args()
    package = args.package_directory.resolve(strict=True)
    pin = json.loads((TOOLS / "crt-runtime.json").read_text(encoding="utf-8-sig"))
    work = NATIVE / "artifacts" / "crt-checks" / uuid.uuid4().hex
    work.mkdir(parents=True)
    checks = []

    def check(condition, label):
        if not condition:
            raise AssertionError(label)
        checks.append(label)

    def reject(directory, label, expected, require_brutal=True):
        try:
            dependencies.audit(directory, require_brutal)
        except (ValueError, FileNotFoundError) as error:
            check(expected in str(error), label + ": correct rejection")
        else:
            raise AssertionError(label + ": unexpectedly accepted")

    @contextmanager
    def changed(path, contents=None):
        original = path.read_bytes() if path.exists() else None
        if contents is None:
            path.unlink()
        else:
            path.write_bytes(contents)
        try:
            yield
        finally:
            if original is None:
                path.unlink(missing_ok=True)
            else:
                path.write_bytes(original)

    def setup(destination, cache=None):
        command = ["powershell.exe", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", str(TOOLS / "setup-crt.ps1"), "-Destination", str(destination)]
        if cache:
            command += ["-CacheDirectory", str(cache)]
        result = subprocess.run(command, capture_output=True, text=True, creationflags=subprocess.CREATE_NO_WINDOW)
        with (work / "setup.log").open("a", encoding="utf-8") as log:
            log.write(result.stdout + result.stderr + "\n")
        return result

    def fingerprints(directory):
        return {str(path.relative_to(directory)): hashlib.sha256(path.read_bytes()).hexdigest()
                for path in directory.rglob("*") if path.is_file()}

    report = dependencies.audit(package, True)
    check(not report["vulkanRequired"], "Selfcontained package has complete dependency closure without Vulkan")
    (work / "package-dependencies.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    if args.compare_pefile:
        import pefile
        for record in report["files"]:
            file = package / record["path"]
            pe = pefile.PE(str(file))
            expected = sorted({entry.dll.decode("ascii").lower()
                               for table in ("DIRECTORY_ENTRY_IMPORT", "DIRECTORY_ENTRY_DELAY_IMPORT")
                               for entry in getattr(pe, table, [])})
            check(record["imports"] == expected, "Independent PE import comparison: " + record["path"])
            pe.close()

    fixture = work / "app"
    fixture.mkdir()
    for name in (*dependencies.BRUTAL_FILES, *(entry["name"] for entry in pin["files"]),
                 pin["license"]["path"], "crt-runtime-provenance.json", "CRT_RUNTIME_NOTICES.txt"):
        target = fixture / name
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(package / name, target)
    check(len(dependencies.audit(fixture, True)["files"]) == 6, "Minimal BRUTAL + CRT fixture passes")
    for name in ("vcruntime140.dll", "vcruntime140_1.dll"):
        with changed(fixture / name):
            reject(fixture, "Missing exact " + name, "Missing or modified pinned Microsoft CRT")
            reject(fixture, "Import closure catches missing " + name, "Missing application-local import", False)
        with changed(fixture / name, b"corrupt DLL"):
            reject(fixture, "Tampered " + name, "Missing or modified pinned Microsoft CRT")
    with changed(fixture / "vcruntime140.dll"):
        with changed(fixture / "vcruntime140_cor3.dll", (package / "vcruntime140.dll").read_bytes()):
            reject(fixture, "The cor3 runtime does not satisfy vcruntime140 imports", "Missing application-local import", False)
    with changed(fixture / pin["license"]["path"], b"changed license"):
        reject(fixture, "Changed Microsoft license", "Missing or modified pinned Microsoft CRT")
    with changed(fixture / "crt-runtime-provenance.json", b"{}"):
        reject(fixture, "Changed provenance", "Packaged CRT provenance differs")
    with changed(fixture / "CRT_RUNTIME_NOTICES.txt", b"changed notice"):
        reject(fixture, "Changed notices", "Packaged CRT notices")
    with changed(fixture / "VulkanEx.dll", (fixture / "glad.dll").read_bytes()):
        reject(fixture, "Unused VulkanEx native backend", "Unused VulkanEx.dll")
    # Change an import string without executing the DLL. This also exercises the
    # import-based Vulkan gate, independently of the native backend filename gate.
    original = (fixture / "glad.dll").read_bytes()
    needle = b"KERNEL32.dll\0"
    check(needle in original, "Native import mutation fixture exists")
    with changed(fixture / "glad.dll", original.replace(needle, b"vulkan-1.dll\0")):
        reject(fixture, "Vulkan loader import", "Unexpected Vulkan import")
    with changed(fixture / "glad.dll", original.replace(needle, b"missingx.dll\0")):
        reject(fixture, "Missing arbitrary native dependency", "Missing application-local import")
    with changed(fixture / "SDL3.dll"):
        reject(fixture, "Missing BRUTAL library", "Missing BRUTAL native runtime")

    extracted = work / "extracted"
    result = setup(extracted)
    check(result.returncode == 0, "Fresh pinned cabinet extraction: " + result.stderr)
    first = fingerprints(extracted)
    check(len(first) == 5, "Extraction contains exactly two DLLs, license, provenance, and notice")
    check(setup(extracted).returncode == 0 and first == fingerprints(extracted), "Cached extraction is reproducible")
    for name in ("vcruntime140.dll", pin["license"]["path"]):
        with changed(extracted / name, b"preserve corrupt evidence"):
            result = setup(extracted)
            check(result.returncode != 0 and "CRT integrity check failed" in result.stderr, "Setup rejects corrupted " + name)
            check((extracted / name).read_bytes() == b"preserve corrupt evidence", "Setup preserves corrupted " + name)
    with changed(extracted / "vcruntime140.dll", b"preserve partial evidence"):
        with changed(extracted / "vcruntime140_1.dll"):
            result = setup(extracted)
            check(result.returncode != 0 and (extracted / "vcruntime140.dll").read_bytes() == b"preserve partial evidence",
                  "Partial extraction does not overwrite a corrupted existing runtime")
    with changed(extracted / "unexpected.txt", b"preserve unexpected file"):
        result = setup(extracted)
        check(result.returncode != 0 and "Unexpected files" in result.stderr and (extracted / "unexpected.txt").is_file(),
              "Unexpected output files are rejected and preserved")
    bad_cache = work / "bad-cache"
    bad_cache.mkdir()
    bad_archive = bad_cache / (pin["sha256"] + ".exe")
    bad_archive.write_bytes(b"preserve invalid archive")
    result = setup(work / "bad-output", bad_cache)
    check(result.returncode != 0 and "CRT integrity check failed" in result.stderr and bad_archive.read_bytes() == b"preserve invalid archive",
          "Wrong archive hash is rejected and preserved")
    (work / "checks.json").write_text(json.dumps({"checks": checks, "passed": len(checks)}, indent=2) + "\n", encoding="utf-8")
    print(f"CRT checks passed: {len(checks)}; evidence: {work}")


if __name__ == "__main__":
    main()
