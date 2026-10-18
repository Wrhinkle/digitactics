# Native audio decoder

Digi-Tactics uses FFmpeg 8.1.3 from exact upstream commit
`29e619e767cde9045a75c29bc9a8278ae7b3a98b`, built without external codec
libraries, networking, GPL components, or nonfree components. The game launches
`media/ffmpeg.exe` and reads stereo float PCM through a pipe. The executable
loads the accompanying five replaceable FFmpeg DLLs. The selected build is
LGPL-2.1-or-later; upstream copyright and license texts are retained.

The release source archive contains the exact unmodified FFmpeg source ZIP,
an empty `changes.diff`, the build scripts, qualified toolchain pins, generated
configuration and configure log, and GNU Make/NASM source packages. It contains
every FFmpeg source file, including notices for optional code that is not built.
No external codec source dependencies are required by this configuration.
Microsoft's standard compiler/runtime and Windows SDK are system toolchain
prerequisites; they are not redistributed as standalone tools in this archive.
The build statically links the standard Microsoft C runtime and dynamically
links FFmpeg. PE imports contain only the five FFmpeg libraries and Windows
system libraries (KERNEL32, BCRYPT, PSAPI and SHELL32).

## Rebuilding

Use Windows x64, Python 3, Visual Studio 2022 C++ toolset 14.38.33130 (compiler
19.38.33133), Windows SDK 10.0.22621.0 and Git for Windows 2.53.0.windows.1.
The scripts verify compiler and shell hashes and provision hash-pinned GNU Make
4.4.1 and NASM 2.16.03 locally. They do not install software system-wide.

From this source archive, extract `build/` to a writable workspace. Copy the
included `sources/FFmpeg-*.zip` to
`build/native/artifacts/media-source-downloads/69ea98adb1b80aabdbecefdb0b545ff19ebcd7c6fc0cea9dd550b02f5b642a21.zip`.
Copy the included Make and NASM source archives, plus the two binary packages
in `build-tools/`, to `build/native/artifacts/media-build-tools/`, retaining their
filenames. Then run:

```powershell
& build/native/tools/build-media-audio.ps1 -BuildRoot C:/DigiTacticsMediaBuild
```

The script downloads the two hash-pinned Make/NASM binary packages only if absent.
With these included archives copied, the qualified host can build offline.
The resulting runtime lives at
`C:/DigiTacticsMediaBuild/stage/digitactics-media-runtime/bin`.
The FFmpeg source is unmodified; configuration is in `media-audio/build.sh`.
Two consecutive builds on the qualified host reproduced all six runtime binaries
and five import libraries byte for byte. This is measured same-host reproducibility;
other host paths and tool installations still need independent qualification.
Generated logs record the actual build environment and can differ between builds.

The decoder only encodes float PCM for the game's playback pipe. Normal tests use
ten checked-in synthetic fixtures (643,141 bytes) with hashes and provenance.
Intentional fixture regeneration can use the separate development encoder
provisioned by `setup-media-fixture-encoder.ps1`. Its historical BtbN runtime and
partial source recovery materials are not included in the native game release.

## Packaging and distribution

In the repository, run `python native/tools/prepare-media-audio.py` after the
qualified build. It verifies the current production runtime pin and creates a
deterministic binary ZIP and a source ZIP with an integrity manifest. The source
archive's `distributionReady` describes source-material completeness for this
specific FFmpeg configuration; it is not a blanket legal review of the game.

Ship `digitactics-media-audio-sources-8.1.3-digitactics-audio-29e619e767.zip`
beside the full/update game ZIPs on the same release, and link it wherever the
game download is offered. The installed game includes the notices, license
texts, source provenance and build configuration; the source ZIP is a separate
release attachment. Preserve the user's ability to replace the FFmpeg DLLs.
The packaging hash is for release integrity and is not enforced by the game
when a user supplies a replacement decoder.

Upstream guidance and licensing details:
- https://ffmpeg.org/legal.html
- https://ffmpeg.org/platform.html#Microsoft-Visual-C_002b_002b-or-Intel-C_002b_002b-Compiler-for-Windows
- `licenses/LICENSE.md` and `licenses/COPYING.LGPLv2.1` in this archive.
