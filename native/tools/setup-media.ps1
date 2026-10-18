$ErrorActionPreference = 'Stop'
$nativeRoot = Split-Path -Parent $PSScriptRoot
$pinPath = Join-Path $PSScriptRoot 'media-runtime.json'
$pin = Get-Content -LiteralPath $pinPath -Raw | ConvertFrom-Json
$downloads = Join-Path $nativeRoot 'artifacts/media-downloads'
$destination = Join-Path $nativeRoot ('artifacts/media-runtime/' + $pin.sha256)
New-Item -ItemType Directory -Force -Path $downloads, $destination | Out-Null
$archivePath = Join-Path $downloads ($pin.sha256 + '.zip')
if (-not (Test-Path -LiteralPath $archivePath)) {
    $temporary = $archivePath + '.' + [Guid]::NewGuid().ToString('N') + '.download'
    & curl.exe --http1.1 --location --fail --silent --show-error --output $temporary $pin.url
    if ($LASTEXITCODE -ne 0) { throw 'Could not download the pinned media runtime.' }
    if ((Get-FileHash -LiteralPath $temporary -Algorithm SHA256).Hash.ToLowerInvariant() -ne $pin.sha256) { throw "Downloaded runtime failed SHA-256 verification; preserved at $temporary" }
    Move-Item -LiteralPath $temporary -Destination $archivePath
}
if ((Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $pin.sha256) { throw 'Cached media runtime failed SHA-256 verification; it was preserved.' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($archivePath)
try {
    $entries = @{}
    foreach ($name in $pin.files) { $entries[$name] = $pin.archiveRoot + '/bin/' + $name }
    $entries['LICENSE.txt'] = $pin.archiveRoot + '/LICENSE.txt'
    foreach ($name in $entries.Keys) {
        $entry = $archive.GetEntry($entries[$name])
        if ($null -eq $entry) { throw "Missing runtime file: $name" }
        # Only exact manifest entries are extracted; archive paths never determine destination paths.
        $target = Join-Path $destination $name
        $temporary = $target + '.' + [Guid]::NewGuid().ToString('N') + '.tmp'
        $inputStream = $entry.Open()
        try {
            $outputStream = [IO.File]::Open($temporary, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
            try { $inputStream.CopyTo($outputStream); $outputStream.Flush($true) } finally { $outputStream.Dispose() }
        } finally { $inputStream.Dispose() }
        Move-Item -LiteralPath $temporary -Destination $target -Force
    }
} finally { $archive.Dispose() }
Copy-Item -LiteralPath $pinPath -Destination (Join-Path $destination 'provenance.json') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'MEDIA_RUNTIME_NOTICES.txt') -Destination $destination -Force
$configuration = & (Join-Path $destination 'ffmpeg.exe') -version 2>&1
if ($LASTEXITCODE -ne 0) { throw 'The pinned decoder did not run.' }
if (($configuration -join "`n") -match '--enable-gpl|--enable-nonfree') { throw 'Unexpected decoder licensing configuration.' }
$configuration | Set-Content -LiteralPath (Join-Path $destination 'build-configuration.txt') -Encoding utf8
Write-Host "Verified pinned media runtime: $($pin.build)"
$destination
