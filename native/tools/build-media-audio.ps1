param(
    [string]$BuildRoot = (Join-Path $env:LOCALAPPDATA 'DigiTacticsBuild/ffmpeg-audio'),
    [string]$VisualStudioDirectory = (Join-Path $env:ProgramFiles 'Microsoft Visual Studio/2022/Community'),
    [ValidateRange(1, 32)][int]$Jobs = 4
)

$ErrorActionPreference = 'Stop'
$nativeRoot = Split-Path -Parent $PSScriptRoot
$pinsPath = Join-Path $PSScriptRoot 'media-audio/toolchain.json'
$pins = Get-Content -LiteralPath $pinsPath -Raw | ConvertFrom-Json
$BuildRoot = [IO.Path]::GetFullPath($BuildRoot)
if ($BuildRoot -match '\s') { throw 'FFmpeg GNU Make builds require a build path without spaces; supply -BuildRoot with a suitable local directory.' }
$toolsRoot = & (Join-Path $PSScriptRoot 'setup-media-build-tools.ps1')
$sourceCache = Join-Path $nativeRoot 'artifacts/media-source-downloads'
New-Item -ItemType Directory -Force -Path $sourceCache, $BuildRoot | Out-Null
$sourceArchive = Join-Path $sourceCache ($pins.sourceSha256 + '.zip')
if (-not (Test-Path -LiteralPath $sourceArchive)) {
    $temporary = $sourceArchive + '.' + [Guid]::NewGuid().ToString('N') + '.download'
    & curl.exe --http1.1 --proto '=https' --proto-redir '=https' --location --fail --silent --show-error --output $temporary $pins.sourceUrl
    if ($LASTEXITCODE -ne 0) { throw 'Could not download the pinned audio decoder source.' }
    if ((Get-Item -LiteralPath $temporary).Length -ne $pins.sourceSize -or (Get-FileHash -LiteralPath $temporary -Algorithm SHA256).Hash.ToLowerInvariant() -ne $pins.sourceSha256) { throw 'Downloaded audio decoder source failed verification.' }
    Move-Item -LiteralPath $temporary -Destination $sourceArchive
}
if ((Get-Item -LiteralPath $sourceArchive).Length -ne $pins.sourceSize -or (Get-FileHash -LiteralPath $sourceArchive -Algorithm SHA256).Hash.ToLowerInvariant() -ne $pins.sourceSha256) { throw 'Cached audio decoder source failed verification.' }

$vcvars = Join-Path $VisualStudioDirectory 'VC/Auxiliary/Build/vcvars64.bat'
$variables = & cmd.exe /d /s /c "`"$vcvars`" $($pins.windowsSdkVersion) -vcvars_ver=$($pins.msvcToolsVersion) >nul && set"
if ($LASTEXITCODE -ne 0) { throw 'Could not initialize the installed Microsoft C/C++ compiler.' }
$originalEnvironment = @{}
try {
    foreach ($line in $variables) {
        if ($line -match '^([^=]+)=(.*)$') {
            $originalEnvironment[$matches[1]] = [Environment]::GetEnvironmentVariable($matches[1], 'Process')
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2], 'Process')
        }
    }
    if ($env:VCToolsVersion -ne $pins.msvcToolsVersion -or $env:WindowsSDKVersion.TrimEnd('\') -ne $pins.windowsSdkVersion) { throw 'Unqualified MSVC or Windows SDK version.' }
    foreach ($compiler in $pins.compilerFiles) {
        $path = Join-Path (Join-Path $env:VCToolsInstallDir 'bin/Hostx64/x64') $compiler.name
        if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $compiler.sha256) { throw "Unqualified compiler binary: $($compiler.name)" }
    }
    $originalEnvironment['DT_MEDIA_BUILD_TOOLS'] = $env:DT_MEDIA_BUILD_TOOLS
    $originalEnvironment['DT_MEDIA_BUILD_JOBS'] = $env:DT_MEDIA_BUILD_JOBS
    $env:DT_MEDIA_BUILD_TOOLS = $toolsRoot
    $env:DT_MEDIA_BUILD_JOBS = [string]$Jobs
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($sourceArchive)
    $sourceRootName = 'FFmpeg-' + $pins.sourceCommit
    try {
        foreach ($entry in $archive.Entries) {
            if (-not $entry.FullName.StartsWith($sourceRootName + '/', [StringComparison]::Ordinal)) { throw 'Unexpected FFmpeg source root.' }
            $target = [IO.Path]::GetFullPath((Join-Path $BuildRoot $entry.FullName))
            if (-not $target.StartsWith($BuildRoot.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe source archive entry.' }
            if ($entry.FullName.EndsWith('/')) { New-Item -ItemType Directory -Force -Path $target | Out-Null }
            else {
                New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
                [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
            }
        }
    } finally { $archive.Dispose() }
    $sourceRoot = Join-Path $BuildRoot $sourceRootName
    $staging = Join-Path $BuildRoot 'stage'
    $install = Join-Path $staging 'digitactics-media-runtime'
    $script = Join-Path $PSScriptRoot 'media-audio/build.sh'
    Write-Host "Building native audio FFmpeg with MSVC $($pins.msvcToolsVersion), SDK $($pins.windowsSdkVersion)."
    & (Join-Path $env:ProgramFiles 'Git/bin/bash.exe') --noprofile --norc $script $sourceRoot $staging
    if ($LASTEXITCODE -ne 0) { throw 'Native audio FFmpeg build failed; source and build logs remain in the build directory.' }
    Copy-Item -LiteralPath $pinsPath -Destination (Join-Path $install 'toolchain.json') -Force
    Copy-Item -LiteralPath $script -Destination (Join-Path $install 'build.sh') -Force
    foreach ($name in @('config.h', 'config_components.h', 'ffbuild/config.mak', 'ffbuild/config.log')) {
        $destination = Join-Path $install $name
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
        Copy-Item -LiteralPath (Join-Path $sourceRoot $name) -Destination $destination -Force
    }
    Write-Host 'Candidate only: run decoder, mixer and routing checks before changing the production runtime pin.'
    $install
} finally {
    foreach ($name in $originalEnvironment.Keys) { [Environment]::SetEnvironmentVariable($name, $originalEnvironment[$name], 'Process') }
}
