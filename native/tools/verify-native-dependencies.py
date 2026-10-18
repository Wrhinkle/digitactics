"""Audit Windows PE imports without relying on DLLs installed on the build host.

Only documented Windows 10/11 system/API-set libraries may be external. Other
imports must resolve beside the importing binary. No PE files are executed.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re
import struct


SYSTEM_DLLS = frozenset("""
advapi32.dll avrt.dll bcrypt.dll cfgmgr32.dll comctl32.dll comdlg32.dll
crypt32.dll dbghelp.dll dinput8.dll dnsapi.dll dwmapi.dll gdi32.dll hid.dll
imm32.dll iphlpapi.dll kernel32.dll mscoree.dll msvcrt.dll ncrypt.dll
netapi32.dll normaliz.dll ntdll.dll ole32.dll oleacc.dll oleaut32.dll
opengl32.dll powrprof.dll propsys.dll psapi.dll rpcrt4.dll secur32.dll
setupapi.dll shell32.dll shcore.dll shlwapi.dll sspicli.dll user32.dll
userenv.dll uxtheme.dll version.dll winhttp.dll winmm.dll winspool.drv
wintrust.dll ws2_32.dll wtsapi32.dll
""".split())
BRUTAL_FILES = ("SDL3.dll", "glfw3.dll", "glad.dll", "imgui.dll")


class PE:
    def __init__(self, path: Path):
        self.path = path
        self.data = path.read_bytes()
        if self.data[:2] != b"MZ":
            raise ValueError(f"Not a Windows PE file: {path}")
        header = self.unpack("<I", 0x3C)[0]
        if self.data[header:header + 4] != b"PE\0\0":
            raise ValueError(f"Invalid PE signature: {path}")
        self.machine, sections = self.unpack("<HH", header + 4)
        optional_size = self.unpack("<H", header + 20)[0]
        optional = header + 24
        magic = self.unpack("<H", optional)[0]
        if magic not in (0x10B, 0x20B):
            raise ValueError(f"Unsupported PE optional header: {path}")
        self.image_base = self.unpack("<Q" if magic == 0x20B else "<I", optional + (24 if magic == 0x20B else 28))[0]
        self.header_size = self.unpack("<I", optional + 60)[0]
        directory_offset = 112 if magic == 0x20B else 96
        directory_count = self.unpack("<I", optional + directory_offset - 4)[0]
        count = min(directory_count, (optional_size - directory_offset) // 8)
        self.directories = [self.unpack("<II", optional + directory_offset + index * 8) for index in range(count)]
        self.sections = []
        for index in range(sections):
            start = optional + optional_size + index * 40
            virtual_size, address, raw_size, raw = self.unpack("<IIII", start + 8)
            self.sections.append((address, max(virtual_size, raw_size), raw, raw_size))

    def unpack(self, fmt: str, offset: int):
        if offset < 0 or offset + struct.calcsize(fmt) > len(self.data):
            raise ValueError(f"PE structure outside file: {self.path}")
        return struct.unpack_from(fmt, self.data, offset)

    def offset(self, address: int) -> int:
        if 0 <= address < min(self.header_size, len(self.data)):
            return address
        for start, size, raw, raw_size in self.sections:
            if start <= address < start + size and address - start < raw_size:
                result = raw + address - start
                if result < len(self.data):
                    return result
        raise ValueError(f"PE address outside raw sections: {self.path}: {address:x}")

    def name(self, address: int) -> str:
        start = self.offset(address)
        end = self.data.find(b"\0", start, min(len(self.data), start + 512))
        if end == -1:
            raise ValueError(f"Unterminated import name: {self.path}")
        name = self.data[start:end].decode("ascii").lower()
        if not re.fullmatch(r"[a-z0-9_.-]+", name):
            raise ValueError(f"Unexpected import library name: {name}")
        return name

    def imports(self) -> list[str]:
        result = set()
        for index, width in ((1, 20), (13, 32)):
            if index >= len(self.directories):
                continue
            address, size = self.directories[index]
            if not address or not size:
                continue
            offset = self.offset(address)
            for index_in_table in range(min(size // width + 1, 65536)):
                entry = self.unpack("<" + "I" * (width // 4), offset + index_in_table * width)
                if not any(entry):
                    break
                name = entry[3] if width == 20 else entry[1] - (0 if entry[0] & 1 else self.image_base)
                result.add(self.name(name))
            else:
                raise ValueError(f"Unterminated import table: {self.path}")
        return sorted(result)


def verify_file(path: Path, pin: dict):
    if not path.is_file() or path.stat().st_size != pin["size"] or hashlib.sha256(path.read_bytes()).hexdigest() != pin["sha256"]:
        raise ValueError(f"Missing or modified pinned Microsoft CRT file: {path}")


def audit(directory: Path, require_brutal: bool = False) -> dict:
    directory = directory.resolve(strict=True)
    pin = json.loads(Path(__file__).with_name("crt-runtime.json").read_text(encoding="utf-8-sig"))
    files = sorted(path for path in directory.rglob("*") if path.is_file() and path.suffix.lower() in (".dll", ".exe"))
    if not files:
        raise ValueError("No Windows executables or libraries in package.")
    if require_brutal:
        for name in BRUTAL_FILES:
            if not (directory / name).is_file():
                raise ValueError(f"Missing BRUTAL native runtime: {name}")
        for file in pin["files"]:
            verify_file(directory / file["name"], file)
        verify_file(directory / pin["license"]["path"], pin["license"])
        if json.loads((directory / "crt-runtime-provenance.json").read_text(encoding="utf-8-sig")) != pin:
            raise ValueError("Packaged CRT provenance differs from the release pin.")
        notice = directory / "CRT_RUNTIME_NOTICES.txt"
        if not notice.is_file() or notice.read_bytes() != Path(__file__).with_name(notice.name).read_bytes():
            raise ValueError("Packaged CRT notices are missing or modified.")
    records = []
    for file in files:
        if file.name.lower() == "vulkanex.dll":
            raise ValueError("Unused VulkanEx.dll must not be published by the OpenGL application.")
        for expected in pin["files"]:
            if file.name.lower() == expected["name"]:
                verify_file(file, expected)
        pe = PE(file)
        neighbors = {path.name.lower() for path in file.parent.iterdir() if path.is_file()}
        dependencies = pe.imports()
        for dependency in dependencies:
            if dependency in ("vulkan-1.dll", "vulkanex.dll"):
                raise ValueError(f"Unexpected Vulkan import in OpenGL package: {file.relative_to(directory)}")
            if dependency in SYSTEM_DLLS or dependency.startswith(("api-ms-win-", "ext-ms-win-")):
                continue
            if dependency not in neighbors:
                raise ValueError(f"Missing application-local import: {file.relative_to(directory)} -> {dependency}")
        records.append({"path": file.relative_to(directory).as_posix(), "machine": hex(pe.machine), "imports": dependencies})
    return {"schemaVersion": 1, "directory": str(directory), "crtVersion": pin["version"], "files": records,
            "externalDependencies": "Windows 10/11 system DLLs and API sets only", "vulkanRequired": False}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directory", type=Path)
    parser.add_argument("--require-brutal", action="store_true")
    parser.add_argument("--report", type=Path)
    args = parser.parse_args()
    report = audit(args.directory, args.require_brutal)
    if args.report:
        args.report.parent.mkdir(parents=True, exist_ok=True)
        args.report.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"Native dependency closure passed: {len(report['files'])} PE files; pinned CRT {report['crtVersion']}; no Vulkan dependency.")


if __name__ == "__main__":
    main()
