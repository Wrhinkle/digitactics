# Application-local Microsoft CRT

BRUTAL's pinned SDL3, GLFW, GLAD and ImGui native DLLs import `vcruntime140.dll`; ImGui also imports `vcruntime140_1.dll`. A self-contained .NET publish does not satisfy these exact names. Digi-Tactics therefore ships the two unchanged Microsoft release DLLs beside the application.

`crt-runtime.json` pins Microsoft Visual C++ 2015–2022 Redistributable x64 **14.44.35211.0**, including its immutable Microsoft URL, installer hash, cabinet offsets and hashes, exact extracted file hashes, and original license. The selected runtime is newer than the pinned BRUTAL binaries' linker version 14.41. Updating BRUTAL requires checking that relationship again.

```powershell
./native/tools/setup-crt.ps1
python ./native/tools/verify-native-dependencies.py <published-app-directory> --require-brutal --report ./native/artifacts/native-dependencies.json
python ./native/tools/verify-native-dependencies.py <full-launcher-package-directory>
```

Setup downloads approximately 25.6 MB once into `native/artifacts/crt-downloads` and extracts 174,336 bytes of DLLs. It uses the Windows cabinet extractor without running the installer or changing the machine's runtime. SHA-256, exact size, Microsoft Authenticode signature and release version are checked before use. Cached files are checked again; corrupted DLLs, archives and licenses are preserved and rejected. Optional `-CacheDirectory` and `-Destination` support isolated validation. Standard output ends with the verified runtime directory.

The default output is `native/artifacts/crt-runtime/<installer-sha256>/`. The Desktop project copies exactly these five files, preserving their layout:

- `vcruntime140.dll`
- `vcruntime140_1.dll`
- `LICENSES/Microsoft.VisualCpp.VCRuntime/LICENSE.rtf`
- `crt-runtime-provenance.json`
- `CRT_RUNTIME_NOTICES.txt`

These are application payload files and travel in both the full-install and update ZIPs. No separate CRT installer or source archive is required by this packaging design. Redistribution remains subject to the applicable Visual Studio license; the [Visual Studio 2022 REDIST list](https://learn.microsoft.com/en-us/visualstudio/releases/2022/redistribution#visual-c-runtime-files) identifies unchanged release runtime files and Microsoft's [deployment documentation](https://learn.microsoft.com/en-us/cpp/windows/redistributing-visual-cpp-files?view=msvc-170#install-individual-redistributable-files) describes application-local deployment. Do not extract debug runtimes or treat an installed SDK as an independent redistribution grant. App-local servicing is the application's responsibility.

The dependency verifier uses only Python's standard library. It reads normal and delayed PE import tables recursively, requires each non-system library beside its importing binary, and never consults DLLs installed on the build host. The `--require-brutal` mode additionally requires all four BRUTAL native libraries, exact pinned CRT bytes, original license, provenance and notices. Windows 10/11 system DLLs and API sets are the only allowed external imports. Dynamically loaded graphics/audio drivers and Steam availability still need their separate runtime checks.

The game's renderer constructs `GlfwBackend.ClientApi.OpenGL` and `OpenGlBackend`. BRUTAL's optional Vulkan initialization is not called. Publishing excludes the unused `VulkanEx.dll`, and the dependency verifier rejects either that filename or a native Vulkan loader import. The managed Vulkan assembly remains because BRUTAL public signatures reference its types. The packaged real-render smoke remains the check for the actual selected renderer.

For focused regression checks after publishing:

```powershell
python ./native/tools/test-crt-runtime.py <published-app-directory>
# Optional independent parser comparison, when the development pefile package is installed:
python ./native/tools/test-crt-runtime.py <published-app-directory> --compare-pefile
```

The checks exercise fresh and cached extraction, corruption preservation, exact-name import resolution, missing runtime and arbitrary native dependencies, altered licenses/notices/provenance, and the Vulkan exclusion. They copy a small native fixture rather than the full game and retain reports under `native/artifacts/crt-checks`.
