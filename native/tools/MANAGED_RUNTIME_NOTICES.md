# Managed runtime redistribution files

`dotnet publish --self-contained` copies the runtime binaries but does not copy all of their license files. Run `copy-managed-notices.py` after publishing, once for the launcher directory and once for its `app` directory:

```powershell
python native/tools/copy-managed-notices.py --assets native/src/DigiTactics.Launcher/obj/project.assets.json --deps <package>/DigiTactics.Launcher.deps.json --destination <package>
python native/tools/copy-managed-notices.py --assets native/src/DigiTactics.Desktop/obj/project.assets.json --deps <package>/app/DigiTactics.Desktop.deps.json --destination <package>/app
```

The helper reads the published `.deps.json`, matching restore assets and exact NuGet archives. It checks the archive SHA-512 sidecar, package identity and restored NuGet content hash, then compares every shipped .NET runtime and non-BRUTAL managed dependency binary to its archive. It does not guess the runtime version from an SDK installation or download a newer license. There are no network requests during packaging.

Both publish roots receive `DOTNET_LICENSE.TXT`, `DOTNET_THIRD-PARTY-NOTICES.TXT` and deterministic `MANAGED_DEPENDENCY_NOTICES.json`. The game also receives the unchanged licenses and third-party notices for CommunityToolkit.HighPerformance and Microsoft.Extensions under `LICENSES/<package>-<version>/`. The manifest records package versions, original package URLs, archive hashes, repository commits, notice hashes, and the verified published binaries. These app files are included in application updates as well as full installs.

The resolved `Microsoft.NETCore.App.Runtime.win-x64/10.0.13` archive contains its own MIT license and third-party notices; these exact files are used. Its [official NuGet listing](https://www.nuget.org/packages/Microsoft.NETCore.App.Runtime.win-x64/10.0.13) also identifies MIT. Some older general .NET licensing pages describe different Windows distribution licenses; do not replace the actual resolved package's license with those general examples. [.NET's asset licensing model](https://github.com/dotnet/runtime/blob/main/docs/project/licensing-assets.md) explains that binary distributions carry their applicable license and third-party notices.

Microsoft.Extensions packages declare MIT in their NuGet metadata and include third-party notices, but omit the license text. `managed-license-supplements.json` maps the exact repository commits from those packages to their unchanged upstream license, vendored at `licenses/dotnet/LICENSE.TXT` with a size and SHA-256 pin. Both qualified commits contain identical bytes. A new package with missing license text requires an explicit source supplement; its license is never inferred from its name. Packages that already contain the text use their own files. BRUTAL, Steam, Microsoft CRT and FFmpeg retain their existing separate notice paths.

Add `--verify-only` to either command to verify the packaged notices without writing. `python native/tools/test-managed-notices.py` runs small synthetic archive tests, including reproducibility, missing or altered notices, wrong runtime versions, archive corruption, binary mismatch, and missing or altered source supplements. Test binaries are never executed.
