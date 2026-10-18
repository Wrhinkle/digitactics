$ErrorActionPreference = 'Stop'
$nativeRoot = Split-Path -Parent $PSScriptRoot
$pinPath = Join-Path $PSScriptRoot 'media-runtime.json'
$pin = Get-Content -LiteralPath $pinPath -Raw | ConvertFrom-Json
$archivePath = Join-Path $nativeRoot ('artifacts/media-downloads/' + $pin.sha256 + '.zip')
if (-not (Test-Path -LiteralPath $archivePath)) {
    $stage = & (Join-Path $PSScriptRoot 'build-media-audio.ps1') | Select-Object -Last 1
    & python (Join-Path $PSScriptRoot 'prepare-media-audio.py') --stage $stage --runtime-only | Write-Host
    if ($LASTEXITCODE -ne 0) { throw 'The locally built runtime differs from the qualified production pin.' }
}
if ((Get-Item -LiteralPath $archivePath).Length -ne $pin.size -or
    (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $pin.sha256) {
    throw 'Cached media runtime failed verification; it was preserved.'
}
$destination = Join-Path $nativeRoot ('artifacts/media-runtime/' + $pin.sha256)
New-Item -ItemType Directory -Force -Path $destination | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($archivePath)
try {
    foreach ($name in $pin.files) {
        $entry = $archive.GetEntry($name)
        if ($null -eq $entry) { throw "Missing runtime file: $name" }
        $temporary = Join-Path $destination ($name + '.' + [Guid]::NewGuid().ToString('N') + '.tmp')
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $temporary, $false)
        if ((Get-FileHash -LiteralPath $temporary -Algorithm SHA256).Hash.ToLowerInvariant() -ne $pin.fileHashes.$name) { throw "Modified runtime file: $name" }
        Move-Item -LiteralPath $temporary -Destination (Join-Path $destination $name) -Force
    }
} finally { $archive.Dispose() }
$toolchain = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'media-audio/toolchain.json') -Raw | ConvertFrom-Json
if ($pin.sourceCommit -ne $toolchain.sourceCommit) { throw 'Runtime and source pins differ.' }
Copy-Item -LiteralPath $pinPath -Destination (Join-Path $destination 'provenance.json') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'MEDIA_RUNTIME_NOTICES.txt') -Destination $destination -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'media-audio/toolchain.json') -Destination (Join-Path $destination 'source-materials.json') -Force
$licenseDestination = Join-Path $destination 'licenses'
New-Item -ItemType Directory -Force -Path $licenseDestination | Out-Null
$licensePins = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'media-audio/licenses.json') -Raw | ConvertFrom-Json
foreach ($license in $licensePins.licenses) {
    $path = Join-Path (Join-Path $PSScriptRoot 'media-licenses') $license.name
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $license.sha256) { throw "Modified media license: $($license.name)" }
    Copy-Item -LiteralPath $path -Destination $licenseDestination -Force
}
$configuration = & (Join-Path $destination 'ffmpeg.exe') -version 2>&1
if ($LASTEXITCODE -ne 0 -or -not (($configuration -join "`n").StartsWith('ffmpeg version ' + $pin.build + ' '))) { throw 'Qualified audio decoder version failed verification.' }
if (($configuration -join "`n") -match '--enable-gpl|--enable-nonfree|--enable-version3') { throw 'Unexpected decoder licensing configuration.' }
$configuration | Set-Content -LiteralPath (Join-Path $destination 'build-configuration.txt') -Encoding utf8
Write-Host "Verified native audio decoder: $($pin.build)"
$destination
