# Local media compiler support

Run `native/tools/setup-media-build-tools.ps1` from PowerShell. It downloads about
4.4 MB into `native/artifacts/media-build-tools`, verifies the exact size and SHA-256
of every archive on every run, and extracts GNU Make 4.4.1 and NASM 2.16.03. It does
not change an installed application or the machine/user PATH. All downloads and
corresponding source archives are pinned in `media-build-tools.json`.

The qualified existing prerequisites are Git for Windows 2.53.0.windows.1
(Bash 5.2.37 and MSYS runtime 3.6.6) and Visual Studio 2022's x64 MSVC toolset
14.38.33130 plus its Windows SDK. The script requires that Git version, verifies
the pinned hashes of the installed MSYS support files, and records them in
`host-toolchain.json`; it does
not install or redistribute Git, MSVC, or the SDK. GNU Make is GPL-3.0-or-later;
NASM is BSD-2-Clause. Their license texts are extracted under `licenses`, and the
MSYS2 Make source package includes its build recipe and patch. These tools are
build inputs, not game payload files.

Binary paths relative to the artifact directory:

- `make/usr/bin/make.exe`
- `nasm/nasm-2.16.03/nasm.exe`

Start the shell from a process that has called the x64 Visual Studio environment:

```bat
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" -vcvars_ver=14.38.33130
if errorlevel 1 exit /b %errorlevel%
"C:\Program Files\Git\bin\bash.exe" --noprofile --norc build.sh
```

In `build.sh`, convert Windows paths before adding them to the colon-separated
Bash PATH. Put MSVC before Git's utilities: Git's `link.exe` creates hard links
and cannot link C objects. Git Bash may prepend `/usr/bin` during startup, so
ordering must be set inside Bash even with `--noprofile --norc`.

```sh
set -eu
# Set DT_MEDIA_BUILD_TOOLS to the artifact directory before invoking Bash.
tools_dir="$(/usr/bin/cygpath -u "$DT_MEDIA_BUILD_TOOLS")"
msvc_dir="$(/usr/bin/cygpath -u "$VCToolsInstallDir")"
export PATH="$tools_dir/make/usr/bin:$tools_dir/nasm/nasm-2.16.03:$msvc_dir/bin/Hostx64/x64:$PATH:/usr/bin"
make -j2
```

A local smoke build compiled a C source with `cl -MD -O2`, assembled an x64 COFF
function with `nasm -fwin64`, linked both using MSVC `link`, and ran the resulting
executable successfully under parallel GNU Make. FFmpeg compilation and decoder
acceptance are separate checks owned by the media runtime build.
