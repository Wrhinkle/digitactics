# Native release artifact assembly

`create-native-release.ps1` consumes a self-contained directory produced and checked by `publish-native.ps1`. It does not build, upload, create a GitHub release, or establish release acceptance.

```powershell
./native/tools/create-native-release.ps1 `
  -PackageDirectory "$env:LOCALAPPDATA/DigiTacticsBuild/releases/<published-package>" `
  -Destination "$env:LOCALAPPDATA/DigiTacticsBuild/release-assets/<new-directory>" `
  -NotesFile ./release-notes.txt
```

The version comes from the package's `native-launcher.json`. `-NotesFile` is optional UTF-8 text; `-MinimumLauncher` defaults to `1` and must match the oldest launcher compatible with this update. The output directory must be new and outside the input package. Input and output must have ordinary directory ancestors: OneDrive cloud placeholders can carry reparse-point attributes and are rejected by the same policy as linked paths. Use a local directory outside OneDrive for publication and assembly.

The completed directory contains:

- `DigiTactics-native-<version>-win-x64.zip`: full installation, with `DigiTactics.Launcher.exe` at its root and the game under `app/`.
- `DigiTactics-native-update-<version>-win-x64.zip`: application-only payload created and verified by the production launcher package writer.
- `native-latest.json`: the exact schema consumed by `NativeReleaseDiscovery`; its file, size, SHA-256, version, platform and minimum launcher match the update payload.
- `SHA256SUMS.txt`: hashes of both ZIPs and `native-latest.json`.
- `native-release-artifacts.json`: deterministic assembly report with the expected tag, file counts, sizes and hashes.

Supply `-MediaMaterialsArchive <zip>[,<zip>...]` to attach the immutable ZIPs produced by `setup-media-sources.ps1` and the dependency-material tools. Each ZIP retains its original filename, is read back against its `materials-manifest.json` file hashes, and must identify the same runtime SHA-256 as `app/media/provenance.json`. The packaged `MEDIA_RUNTIME_NOTICES.txt` is also copied beside the ZIPs. These files enter `SHA256SUMS.txt` and the assembly report. The report preserves each materials bundle's `distributionReady` and remaining requirements; a partial bundle is not labeled complete corresponding source. `releaseApproved` is always false because this tool only verifies local artifact integrity.

ZIP ordering and timestamps are fixed. Repeating assembly with identical input bytes, notes and minimum launcher produces identical artifacts. Every full-install entry is read back and hashed, and its application content is checked against the update manifest. Output becomes visible under the requested name only after assembly succeeds. A failed attempt leaves a uniquely named `.partial` directory for inspection and never overwrites previous output.

When release acceptance is eventually complete, the five base files and all media source/notice attachments belong on the same GitHub release, tagged `native-v<version>` or `v<version>`. Discovery reads GitHub's latest non-draft, non-prerelease release and ignores the existing Electron `latest.json`. Creating these local files does **not** authorize or perform publication. Live Steam, clean-machine, complete redistribution materials and remaining port acceptance retain their separate gates.

## Verification

After building the test executable, run the isolated fixture test using a published self-contained launcher:

```powershell
./native/tools/test-native-release.ps1 `
  -PackageDirectory "$env:LOCALAPPDATA/DigiTacticsBuild/releases/<published-package>" `
  -TestsExecutable ./native/tests/DigiTactics.Tests/bin/Release/net10.0/win-x64/DigiTactics.Tests.exe
```

The fixture contains a synthetic application that is never executed. The test assembles it twice, checks byte-for-byte reproducibility including a source timestamp change, verifies hidden/Unicode files and output preservation, then passes the produced files through the real native discovery, downloader and package verifier using a local controlled HTTP transport. The test accepts the same optional `-MediaMaterialsArchive` parameter to cover attachment copying and closure-status preservation. The test executable also supports `--native-release-artifacts <artifact-directory>` for verification of an actual full release payload. These checks do not contact GitHub or modify player data.

The framework-dependent test executable requires the build SDK runtime (set `DOTNET_ROOT` to its directory when it is not installed globally). The release ZIPs themselves use the self-contained package and require no installed .NET runtime.
