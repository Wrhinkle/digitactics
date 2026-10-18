param([Parameter(Mandatory=$true)][string]$PackageDirectory,[Parameter(Mandatory=$true)][string]$TestsExecutable,[string[]]$MediaMaterialsArchive=@())
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$PackageDirectory=(Resolve-Path -LiteralPath $PackageDirectory).Path
$TestsExecutable=(Resolve-Path -LiteralPath $TestsExecutable).Path
$testRoot=Join-Path $env:TEMP ('digitactics-release-assembly-'+[Guid]::NewGuid().ToString('N'))
$fixture=Join-Path $testRoot 'package'
$app=Join-Path $fixture 'app'
New-Item -ItemType Directory -Path $app | Out-Null
# Use the real self-contained packager with a deliberately tiny, non-executed app.
Get-ChildItem -LiteralPath $PackageDirectory -File -Force | Copy-Item -Destination $fixture
$utf8=New-Object System.Text.UTF8Encoding($false)
[IO.File]::WriteAllText((Join-Path $fixture 'native-launcher.json'),' {"schemaVersion":1,"bundledVersion":"1.2.3","updateOwner":"test","updateRepository":"native-artifacts"} ',$utf8)
[IO.File]::WriteAllText((Join-Path $app 'DigiTactics.Desktop.exe'),'synthetic fixture; never executed',$utf8)
$nested=Join-Path $app ('content/spaces '+[char]0x00e9)
New-Item -ItemType Directory -Path $nested | Out-Null
[IO.File]::WriteAllBytes((Join-Path $nested 'sample.bin'),[byte[]](0..255))
$hidden=Join-Path $app '.hidden-notice.txt'
[IO.File]::WriteAllText($hidden,'Preserve hidden package files.',$utf8)
[IO.File]::SetAttributes($hidden,[IO.FileAttributes]::Hidden)
if($MediaMaterialsArchive.Count -gt 0) {
    $media=Join-Path $app 'media'
    New-Item -ItemType Directory -Path $media | Out-Null
    foreach($name in @('provenance.json','MEDIA_RUNTIME_NOTICES.txt')) {Copy-Item -LiteralPath (Join-Path $PackageDirectory ('app/media/'+$name)) -Destination $media}
}
$notes=Join-Path $testRoot 'notes.txt'
[IO.File]::WriteAllText($notes,"Local artifact verification.`nUnicode: $([char]0x00e9).",$utf8)
$first=Join-Path $testRoot 'first'
$second=Join-Path $testRoot 'second'
& (Join-Path $PSScriptRoot 'create-native-release.ps1') -PackageDirectory $fixture -Destination $first -NotesFile $notes -MediaMaterialsArchive $MediaMaterialsArchive
# File timestamps must not alter artifact bytes.
[IO.File]::SetLastWriteTimeUtc((Join-Path $app 'DigiTactics.Desktop.exe'),[DateTime]::UtcNow.AddDays(-7))
& (Join-Path $PSScriptRoot 'create-native-release.ps1') -PackageDirectory $fixture -Destination $second -NotesFile $notes -MediaMaterialsArchive $MediaMaterialsArchive
$checks=0
foreach($file in Get-ChildItem -LiteralPath $first -File) {
    $other=Join-Path $second $file.Name
    if(-not (Test-Path -LiteralPath $other) -or (Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $other).Hash) {throw "Non-reproducible release artifact: $($file.Name)"}
    $checks++
}
$before=(Get-FileHash -LiteralPath (Join-Path $first 'native-latest.json')).Hash
$rejected=$false
try {& (Join-Path $PSScriptRoot 'create-native-release.ps1') -PackageDirectory $fixture -Destination $first -NotesFile $notes}
catch {if($_.Exception.Message -notlike '*destination must be new*') {throw};$rejected=$true}
if(-not $rejected -or (Get-FileHash -LiteralPath (Join-Path $first 'native-latest.json')).Hash -ne $before) {throw 'Existing release artifacts were not preserved.'}
$checks++
$rejected=$false
try {& (Join-Path $PSScriptRoot 'create-native-release.ps1') -PackageDirectory $fixture -Destination (Join-Path $fixture 'nested-release')}
catch {if($_.Exception.Message -notlike '*outside the published package*') {throw};$rejected=$true}
if(-not $rejected) {throw 'Allowed release output inside its package input.'}
$checks++
[IO.File]::WriteAllText($notes,('x'*16385),$utf8)
$rejected=$false
try {& (Join-Path $PSScriptRoot 'create-native-release.ps1') -PackageDirectory $fixture -Destination (Join-Path $testRoot 'oversized-notes') -NotesFile $notes}
catch {if($_.Exception.Message -notlike '*notes exceed*') {throw};$rejected=$true}
if(-not $rejected) {throw 'Allowed oversized release notes.'}
$checks++
if($MediaMaterialsArchive.Count -gt 0) {
    Add-Type -AssemblyName System.IO.Compression
    $runtime=Get-Content -LiteralPath (Join-Path $app 'media/provenance.json') -Raw | ConvertFrom-Json
    foreach($failure in @('hash','runtime')) {
        $invalid=Join-Path $testRoot ('digitactics-media-invalid-'+$failure+'.zip')
        $stream=[IO.File]::Open($invalid,[IO.FileMode]::CreateNew,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None)
        $zip=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create,$false)
        try {
            $source=$zip.CreateEntry('source.txt').Open()
            try {$source.WriteByte(65)} finally {$source.Dispose()}
            $manifest=@{schemaVersion=1;runtimeSha256=$(if($failure -eq 'runtime') {'0'*64} else {$runtime.sha256});distributionReady=$false;remainingRequirements=@('Synthetic rejected fixture.');files=@(@{path='source.txt';size=1;sha256='0'*64})}
            $contents=$zip.CreateEntry('materials-manifest.json').Open()
            $writer=[IO.StreamWriter]::new($contents,$utf8)
            try {$writer.Write(($manifest | ConvertTo-Json -Depth 5))} finally {$writer.Dispose()}
        } finally {$zip.Dispose();$stream.Dispose()}
        $rejected=$false
        $invalidOutput=Join-Path $testRoot ('invalid-'+$failure)
        try {& (Join-Path $PSScriptRoot 'create-native-release.ps1') -PackageDirectory $fixture -Destination $invalidOutput -MediaMaterialsArchive $invalid}
        catch {
            $reason=if($failure -eq 'runtime') {'*do not match the packaged runtime*'} else {'*hash differs*'}
            if($_.Exception.Message -notlike $reason) {throw}
            $rejected=$true
        }
        if(-not $rejected -or (Test-Path -LiteralPath $invalidOutput)) {throw "Invalid media $failure was published as a completed local release."}
        $checks++
    }
}
& $TestsExecutable '--native-release-artifacts' $first
if($LASTEXITCODE -ne 0) {throw "Native release consumer checks exited $LASTEXITCODE"}
Write-Host "Native release assembly: $checks reproducibility and output-preservation checks passed. Fixture retained: $testRoot"
