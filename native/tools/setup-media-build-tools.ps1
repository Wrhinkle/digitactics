param(
    [string]$GitDirectory = (Join-Path $env:ProgramFiles 'Git'),
    [string]$DestinationRoot = (Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts/media-build-tools')
)

$ErrorActionPreference = 'Stop'
$pinPath = Join-Path $PSScriptRoot 'media-build-tools.json'
$pins = Get-Content -LiteralPath $pinPath -Raw | ConvertFrom-Json
$DestinationRoot = [IO.Path]::GetFullPath($DestinationRoot)
$gitBin = Join-Path $GitDirectory 'usr/bin'
$bash = Join-Path $GitDirectory 'bin/bash.exe'
foreach ($required in @($bash, (Join-Path $gitBin 'msys-2.0.dll'), (Join-Path $gitBin 'msys-intl-8.dll'))) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) { throw "Required Git for Windows support tool is missing: $required" }
}
foreach ($supportFile in $pins.hostPrerequisites.supportFiles) {
    $supportPath = Join-Path $GitDirectory $supportFile.path
    if (-not (Test-Path -LiteralPath $supportPath -PathType Leaf) -or
        (Get-FileHash -LiteralPath $supportPath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $supportFile.sha256) {
        throw "Unqualified Git for Windows build support file: $supportPath"
    }
}
$gitVersion = & (Join-Path $GitDirectory 'cmd/git.exe') --version
if ($LASTEXITCODE -ne 0 -or $gitVersion -ne ('git version ' + $pins.hostPrerequisites.gitForWindows)) {
    throw "This toolchain is qualified with Git for Windows $($pins.hostPrerequisites.gitForWindows); found: $gitVersion"
}
New-Item -ItemType Directory -Force -Path $DestinationRoot | Out-Null

function Confirm-Archive($Path, $Pin) {
    if ((Get-Item -LiteralPath $Path).Length -ne $Pin.size -or
        (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $Pin.sha256) {
        throw "Media build tool archive failed size/SHA-256 verification; preserved at $Path"
    }
}

function Get-Archive($Pin) {
    $target = Join-Path $DestinationRoot $Pin.file
    if (-not (Test-Path -LiteralPath $target)) {
        $temporary = $target + '.' + [Guid]::NewGuid().ToString('N') + '.download'
        & curl.exe --http1.1 --location --fail --silent --show-error --retry 2 --output $temporary $Pin.url
        if ($LASTEXITCODE -ne 0) { throw "Could not download build tool archive: $($Pin.file)" }
        Confirm-Archive $temporary $Pin
        Move-Item -LiteralPath $temporary -Destination $target
    }
    Confirm-Archive $target $Pin
    return $target
}

$makeArchive = Get-Archive $pins.make.binary
$makeSourceArchive = Get-Archive $pins.make.source
$nasmArchive = Get-Archive $pins.nasm.binary
$nasmSourceArchive = Get-Archive $pins.nasm.source
$makeRoot = Join-Path $DestinationRoot 'make'
$nasmRoot = Join-Path $DestinationRoot 'nasm'
$sourcesRoot = Join-Path $DestinationRoot 'sources'
$licensesRoot = Join-Path $DestinationRoot 'licenses'
New-Item -ItemType Directory -Force -Path $makeRoot, $nasmRoot, $sourcesRoot, $licensesRoot | Out-Null
# Only explicit members from hash-verified packages are extracted. No host installation is changed.
& tar.exe -xf $makeArchive -C $makeRoot -- usr/bin/make.exe .PKGINFO
if ($LASTEXITCODE -ne 0) { throw 'Could not extract GNU Make.' }
& tar.exe -xf $makeSourceArchive -C $sourcesRoot -- make/make-4.4.1.tar.gz make/PKGBUILD make/0001-fixes-building-with-gcc-15.patch make/.SRCINFO
if ($LASTEXITCODE -ne 0) { throw 'Could not extract GNU Make source materials.' }
& tar.exe -xf (Join-Path $sourcesRoot 'make/make-4.4.1.tar.gz') -C $licensesRoot -- make-4.4.1/COPYING
if ($LASTEXITCODE -ne 0) { throw 'Could not extract the GNU Make license.' }

Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($nasmArchive)
try {
    $nasmDirectory = Join-Path $nasmRoot 'nasm-2.16.03'
    New-Item -ItemType Directory -Force -Path $nasmDirectory | Out-Null
    foreach ($name in @('nasm.exe', 'ndisasm.exe', 'LICENSE')) {
        $entry = $zip.GetEntry('nasm-2.16.03/' + $name)
        if ($null -eq $entry) { throw "Missing NASM package member: $name" }
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $nasmDirectory $name), $true)
    }
} finally { $zip.Dispose() }
Copy-Item -LiteralPath (Join-Path $nasmDirectory 'LICENSE') -Destination (Join-Path $licensesRoot 'NASM-LICENSE.txt') -Force
Copy-Item -LiteralPath $pinPath -Destination (Join-Path $DestinationRoot 'provenance.json') -Force

$previousPath = $env:PATH
try {
    $env:PATH = $gitBin + ';' + $previousPath
    $makeVersion = & (Join-Path $makeRoot 'usr/bin/make.exe') --version
    if ($LASTEXITCODE -ne 0 -or $makeVersion[0] -ne 'GNU Make 4.4.1') { throw 'Pinned GNU Make did not run.' }
    $nasmVersion = & (Join-Path $nasmDirectory 'nasm.exe') --version
    if ($LASTEXITCODE -ne 0 -or -not $nasmVersion.StartsWith('NASM version 2.16.03 ')) { throw 'Pinned NASM did not run.' }
} finally { $env:PATH = $previousPath }

$hostRecord = [ordered]@{
    git = $gitVersion
    gitDirectory = [IO.Path]::GetFullPath($GitDirectory)
    make = $makeVersion[0]
    nasm = $nasmVersion
    prerequisiteSupportFiles = @(
        foreach ($supportFile in $pins.hostPrerequisites.supportFiles) {
            [ordered]@{ path = $supportFile.path; sha256 = $supportFile.sha256 }
        }
    )
}
$hostRecord | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $DestinationRoot 'host-toolchain.json') -Encoding utf8
Write-Host "Verified local GNU Make 4.4.1 and NASM 2.16.03 using $gitVersion."
$DestinationRoot
