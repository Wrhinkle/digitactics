"""Small synthetic package tests for managed runtime notice copying; no binaries run."""
from contextlib import contextmanager
import base64
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import uuid
from zipfile import ZipFile


tools = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location("managed_notices", tools / "copy-managed-notices.py")
notices = importlib.util.module_from_spec(spec)
spec.loader.exec_module(notices)
work = tools.parent / "artifacts" / "managed-notices-checks" / uuid.uuid4().hex
work.mkdir(parents=True)
app = work / "app"
app.mkdir()
cache = work / "packages"
assets_file = work / "project.assets.json"
deps_file = app / "Fixture.deps.json"
config_file = app / "Fixture.runtimeconfig.json"
fixture_tools = work / "tools"
fixture_tools.mkdir()
shutil.copyfile(tools / "managed-license-supplements.json", fixture_tools / "managed-license-supplements.json")
(fixture_tools / "licenses/dotnet").mkdir(parents=True)
shutil.copyfile(tools / "licenses/dotnet/LICENSE.TXT", fixture_tools / "licenses/dotnet/LICENSE.TXT")
notices.TOOLS = fixture_tools
checks = []


def check(condition, label):
    if not condition:
        raise AssertionError(label)
    checks.append(label)


@contextmanager
def changed(path, content=None):
    original = path.read_bytes() if path.exists() else None
    if content is None:
        path.unlink()
    else:
        path.write_bytes(content)
    try:
        yield
    finally:
        if original is None:
            path.unlink(missing_ok=True)
        else:
            path.write_bytes(original)


def json_bytes(value):
    return (json.dumps(value, indent=2) + "\n").encode()


def reject(label, expected, verify=False):
    try:
        notices.run(assets_file, deps_file, app, verify)
    except (ValueError, FileNotFoundError) as error:
        check(expected in str(error), label)
    else:
        raise AssertionError("Unexpected acceptance: " + label)


runtime = "Microsoft.NETCore.App.Runtime.win-x64/10.0.13"
library = "Fixture.Library/1.2.3"
extension = "Fixture.Extensions/1.0.0"
keys = [runtime, library, extension]
target = {}
libraries = {}
assets_libraries = {}
for key in keys:
    package_id, version = key.split("/")
    root = cache / key.lower()
    root.mkdir(parents=True)
    is_runtime = key == runtime
    is_extension = key == extension
    filename = "System.Runtime.dll" if is_runtime else package_id + ".dll"
    source = "runtimes/win-x64/lib/net10.0/" + filename if is_runtime else "lib/net10.0/" + filename
    data = ("synthetic unexecuted " + key).encode()
    (app / filename).write_bytes(data)
    license_xml = '<license type="expression">MIT</license>' if is_extension or is_runtime else '<license type="file">License.md</license>'
    repository_xml = '<repository url="https://github.com/dotnet/dotnet" commit="b0f34d51fccc69fd334253924abd8d6853fad7aa"/>'
    nuspec = f'<package><metadata><id>{package_id}</id><version>{version}</version>{license_xml}{repository_xml}</metadata></package>'
    archive = root / f"{package_id.lower()}.{version}.nupkg"
    with ZipFile(archive, "w") as zip_file:
        zip_file.writestr(package_id + ".nuspec", nuspec)
        zip_file.writestr(source, data)
        zip_file.writestr("THIRD-PARTY-NOTICES.TXT", "Synthetic third-party notice")
        if not is_extension:
            zip_file.writestr("LICENSE.TXT" if is_runtime else "License.md", "Synthetic package license")
    archive.with_suffix(".nupkg.sha512").write_text(base64.b64encode(hashlib.sha512(archive.read_bytes()).digest()).decode())
    (root / ".nupkg.metadata").write_bytes(json_bytes({"contentHash": "fixture-hash", "source": "https://api.nuget.org/v3/index.json"}))
    published_key = "runtimepack." + key if is_runtime else key
    libraries[published_key] = {"type": "runtimepack" if is_runtime else "package", "sha512": "sha512-fixture-hash"}
    target[published_key] = {"runtime": {filename if is_runtime else source: {}}}
    assets_libraries[key] = {"sha512": "fixture-hash"}

assets = {"packageFolders": {str(cache): {}}, "libraries": assets_libraries}
deps = {"runtimeTarget": {"name": "fixture/win-x64"}, "targets": {"fixture/win-x64": target}, "libraries": libraries}
assets_file.write_bytes(json_bytes(assets))
deps_file.write_bytes(json_bytes(deps))
config_file.write_bytes(json_bytes({"runtimeOptions": {"includedFrameworks": [{"name": "Microsoft.NETCore.App", "version": "10.0.13"}]}}))
result = notices.run(assets_file, deps_file, app)
check(result["packages"] == 3 and result["binaries"] == 3 and result["noticeFiles"] == 7, "Runtime, packaged license, and exact source supplement copied")
outputs, _, _ = notices.gather(assets_file, deps_file, app)
check(notices.run(assets_file, deps_file, app, True) == result, "Read-back verification matches")
check(notices.run(assets_file, deps_file, app) == result and all((app / path).read_bytes() == data for path, data in outputs.items()), "Copy is deterministic")
for name in ("DOTNET_LICENSE.TXT", "DOTNET_THIRD-PARTY-NOTICES.TXT", "MANAGED_DEPENDENCY_NOTICES.json", "LICENSES/Fixture.Library-1.2.3/License.md"):
    with changed(app / name):
        reject("Missing notice rejected: " + name, "Missing or modified managed dependency notice", True)
    with changed(app / name, b"changed"):
        reject("Changed notice rejected: " + name, "Missing or modified managed dependency notice", True)
with changed(app / "System.Runtime.dll", b"changed binary"):
    reject("Modified runtime binary rejected", "Published binary differs")
with changed(app / "Fixture.Library.dll"):
    reject("Missing package binary rejected", "Published binary differs")
archive = cache / library.lower() / "fixture.library.1.2.3.nupkg"
with changed(archive, b"corrupted archive"):
    reject("Corrupted NuGet archive rejected", "NuGet archive integrity mismatch")
with changed(cache / library.lower() / ".nupkg.metadata", json_bytes({"contentHash": "other-hash"})):
    reject("Restored package identity mismatch rejected", "Published dependency does not match restored package")
with changed(config_file, json_bytes({"runtimeOptions": {"includedFrameworks": [{"name": "Microsoft.NETCore.App", "version": "9.0.0"}]}})):
    reject("Published runtime version mismatch rejected", "Runtime config does not match")
with changed(fixture_tools / "licenses/dotnet/LICENSE.TXT", b"changed source"):
    reject("Modified pinned license supplement rejected", "Pinned license supplement differs")
with changed(fixture_tools / "managed-license-supplements.json", json_bytes({"supplements": {}})):
    reject("Unknown source license cannot be guessed", "Package license text needs an exact source supplement")
deps_without_runtime = json.loads(json.dumps(deps))
del deps_without_runtime["libraries"]["runtimepack." + runtime]
with changed(deps_file, json_bytes(deps_without_runtime)):
    reject("Framework-dependent output cannot masquerade as selfcontained", "Expected exactly one published")
check(all((app / path).read_bytes() == data for path, data in outputs.items()), "Failures preserve existing notices")
(work / "checks.json").write_bytes(json_bytes({"passed": len(checks), "checks": checks}))
print(f"Managed notice checks passed: {len(checks)}; evidence: {work}")
