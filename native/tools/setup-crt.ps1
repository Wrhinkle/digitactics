param([string]$CacheDirectory,[string]$Destination)
$ErrorActionPreference='Stop'
$nativeRoot=Split-Path -Parent $PSScriptRoot
$pinPath=Join-Path $PSScriptRoot 'crt-runtime.json'
$pin=Get-Content -LiteralPath $pinPath -Raw | ConvertFrom-Json
if(-not $CacheDirectory){$CacheDirectory=Join-Path $nativeRoot 'artifacts/crt-downloads'}
if(-not $Destination){$Destination=Join-Path $nativeRoot ('artifacts/crt-runtime/'+$pin.sha256)}
$CacheDirectory=[IO.Path]::GetFullPath($CacheDirectory)
$Destination=[IO.Path]::GetFullPath($Destination)
function Test-PinnedFile([string]$Path,$Expected) {
    if(-not(Test-Path -LiteralPath $Path -PathType Leaf) -or (Get-Item -LiteralPath $Path).Length -ne $Expected.size -or
       (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $Expected.sha256){throw "CRT integrity check failed; file preserved: $Path"}
}
function Test-MicrosoftBinary([string]$Path) {
    $signature=Get-AuthenticodeSignature -LiteralPath $Path
    if($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch '(^|, )CN=Microsoft (Corporation|Windows)(,|$)' -or
       $signature.SignerCertificate.Subject -notmatch '(^|, )O=Microsoft Corporation(,|$)'){throw "CRT file is not validly signed by Microsoft: $Path"}
    if((Get-Item -LiteralPath $Path).VersionInfo.FileVersion -cne $pin.version){throw "CRT file version differs from the qualified pin: $Path"}
}
New-Item -ItemType Directory -Force -Path $CacheDirectory,$Destination | Out-Null
$archive=Join-Path $CacheDirectory ($pin.sha256+'.exe')
if(-not(Test-Path -LiteralPath $archive)) {
    $download=$archive+'.'+[Guid]::NewGuid().ToString('N')+'.tmp'
    Invoke-WebRequest -Uri $pin.url -OutFile $download -UseBasicParsing
    Test-PinnedFile $download $pin
    Move-Item -LiteralPath $download -Destination $archive
}
Test-PinnedFile $archive $pin
Test-MicrosoftBinary $archive
$complete=$true
foreach($file in $pin.files){
    $existing=Join-Path $Destination $file.name
    if(Test-Path -LiteralPath $existing){Test-PinnedFile $existing $file}else{$complete=$false}
}
$existingLicense=Join-Path $Destination $pin.license.path
if(Test-Path -LiteralPath $existingLicense){Test-PinnedFile $existingLicense $pin.license}else{$complete=$false}
if(-not $complete) {
    $stage=Join-Path $CacheDirectory ('extract-'+[Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $stage | Out-Null
    try {
        $bytes=[IO.File]::ReadAllBytes($archive)
        foreach($cabinet in $pin.cabinets) {
            $path=Join-Path $stage $cabinet.name
            $stream=[IO.File]::Create($path)
            try {$stream.Write($bytes,[int]$cabinet.offset,[int]$cabinet.size)}finally{$stream.Dispose()}
            Test-PinnedFile $path $cabinet
        }
        $expand=Join-Path $env:WINDIR 'System32/expand.exe'
        & $expand (Join-Path $stage 'payload.cab') ('-F:'+$pin.crtCabinet.entry) $stage | Out-Null
        if($LASTEXITCODE -ne 0){throw 'Could not extract pinned Microsoft CRT cabinet.'}
        $crtCabinet=Join-Path $stage $pin.crtCabinet.entry
        Test-PinnedFile $crtCabinet $pin.crtCabinet
        foreach($file in $pin.files) {
            & $expand $crtCabinet ('-F:'+$file.entry) $stage | Out-Null
            if($LASTEXITCODE -ne 0){throw "Could not extract $($file.name)."}
            $source=Join-Path $stage $file.entry
            Test-PinnedFile $source $file
            Test-MicrosoftBinary $source
            Copy-Item -LiteralPath $source -Destination (Join-Path $Destination $file.name)
        }
        & $expand (Join-Path $stage 'ux.cab') ('-F:'+$pin.license.entry) $stage | Out-Null
        if($LASTEXITCODE -ne 0){throw 'Could not extract Microsoft CRT license.'}
        $license=Join-Path $stage $pin.license.entry
        Test-PinnedFile $license $pin.license
        $licenseTarget=Join-Path $Destination $pin.license.path
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $licenseTarget) | Out-Null
        Copy-Item -LiteralPath $license -Destination $licenseTarget
    } finally {
        $resolved=[IO.Path]::GetFullPath($stage)
        if(-not $resolved.StartsWith($CacheDirectory.TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or
           -not ([IO.Path]::GetFileName($resolved)).StartsWith('extract-',[StringComparison]::Ordinal)){throw 'Refusing cleanup outside generated CRT extraction directory.'}
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
}
foreach($file in $pin.files){$path=Join-Path $Destination $file.name;Test-PinnedFile $path $file;Test-MicrosoftBinary $path}
Test-PinnedFile (Join-Path $Destination $pin.license.path) $pin.license
Copy-Item -LiteralPath $pinPath -Destination (Join-Path $Destination 'crt-runtime-provenance.json') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CRT_RUNTIME_NOTICES.txt') -Destination $Destination -Force
$expected=@($pin.files | ForEach-Object {$_.name})+@($pin.license.path,'crt-runtime-provenance.json','CRT_RUNTIME_NOTICES.txt')
$actual=@(Get-ChildItem -LiteralPath $Destination -File -Recurse | ForEach-Object {$_.FullName.Substring($Destination.TrimEnd('\','/').Length+1).Replace('\','/')})
if($actual.Count -ne $expected.Count -or @($actual | Where-Object {$_ -cnotin $expected}).Count -gt 0){throw 'Unexpected files in pinned CRT output were preserved; use a fresh destination.'}
Write-Host "Verified application-local Microsoft CRT $($pin.version) x64 (174336 bytes)."
$Destination
