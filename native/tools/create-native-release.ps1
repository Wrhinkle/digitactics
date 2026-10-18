param(
    [Parameter(Mandatory=$true)][string]$PackageDirectory,
    [Parameter(Mandatory=$true)][string]$Destination,
    [ValidateRange(1,2147483647)][int]$MinimumLauncher=1,
    [string]$NotesFile,
    [string[]]$MediaMaterialsArchive=@()
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.IO.Compression
$utf8=New-Object System.Text.UTF8Encoding($false,$true)

function Assert-OrdinaryAncestors([string]$Path) {
    $current=[IO.Path]::GetFullPath($Path)
    while($current) {
        if(Test-Path -LiteralPath $current) {
            if(([IO.File]::GetAttributes($current) -band [IO.FileAttributes]::ReparsePoint) -ne 0) {throw "Release paths cannot contain links: $current"}
        }
        $current=[IO.Path]::GetDirectoryName($current)
    }
}
function Get-PackageFiles([string]$Directory) {
    foreach($path in [IO.Directory]::EnumerateFileSystemEntries($Directory)) {
        $attributes=[IO.File]::GetAttributes($path)
        if(($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {throw "Release sources cannot contain links: $path"}
        if(($attributes -band [IO.FileAttributes]::Directory) -ne 0) {Get-PackageFiles $path}
        else {$path}
    }
}
function Get-Digest([IO.Stream]$Stream) {
    $hash=[Security.Cryptography.SHA256]::Create()
    try {return ([BitConverter]::ToString($hash.ComputeHash($Stream))).Replace('-','').ToLowerInvariant()}
    finally {$hash.Dispose()}
}
function Get-FileDigest([string]$Path) {
    $inputFile=[IO.File]::Open($Path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
    try {return Get-Digest $inputFile} finally {$inputFile.Dispose()}
}
function Write-Json([string]$Path,$Value) {[IO.File]::WriteAllText($Path,($Value | ConvertTo-Json -Depth 8 -Compress)+"`n",$utf8)}
function Copy-MediaMaterials([string]$Archive,[string]$OutputDirectory,[string]$RuntimeSha256) {
    $Archive=(Resolve-Path -LiteralPath $Archive).Path
    $name=[IO.Path]::GetFileName($Archive)
    if($name -cnotmatch '^digitactics-media-[a-zA-Z0-9.-]+\.zip$') {throw 'Media materials must retain their digitactics-media-*.zip attachment name.'}
    $target=Join-Path $OutputDirectory $name
    $inputFile=[IO.File]::Open($Archive,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
    try {
        $digest=Get-Digest $inputFile
        $inputFile.Position=0
        $zip=[IO.Compression.ZipArchive]::new($inputFile,[IO.Compression.ZipArchiveMode]::Read,$true)
        try {
            $entries=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
            foreach($entry in $zip.Entries) {$entries.Add($entry.FullName,$entry)}
            if(-not $entries.ContainsKey('materials-manifest.json') -or $entries['materials-manifest.json'].Length -gt 16MB) {throw 'Missing or oversized media materials manifest.'}
            $reader=[IO.StreamReader]::new($entries['materials-manifest.json'].Open(),$utf8)
            try {$manifest=$reader.ReadToEnd() | ConvertFrom-Json} finally {$reader.Dispose()}
            if($manifest.schemaVersion -ne 1 -or $manifest.runtimeSha256 -cne $RuntimeSha256 -or $manifest.distributionReady -isnot [bool]) {throw 'Media materials do not match the packaged runtime provenance.'}
            if($entries.Count -ne @($manifest.files).Count+1) {throw 'Media materials archive contains unlisted files.'}
            $listed=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
            foreach($file in $manifest.files) {
                if(-not $listed.Add($file.path) -or $file.path -ceq 'materials-manifest.json' -or -not $entries.ContainsKey($file.path) -or $file.sha256 -cnotmatch '^[a-f0-9]{64}$' -or $file.size -lt 0 -or $entries[$file.path].Length -ne $file.size) {throw 'Invalid media material file metadata.'}
                $contents=$entries[$file.path].Open()
                try {if((Get-Digest $contents) -cne $file.sha256) {throw "Media materials hash differs: $($file.path)"}} finally {$contents.Dispose()}
            }
        } finally {$zip.Dispose()}
        $inputFile.Position=0
        $outputFile=[IO.File]::Open($target,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
        try {$inputFile.CopyTo($outputFile,131072);$outputFile.Flush($true)} finally {$outputFile.Dispose()}
        if((Get-FileDigest $target) -cne $digest) {throw 'Copied media source materials differ from the verified archive.'}
    } finally {$inputFile.Dispose()}
    return [ordered]@{file=$name;runtimeSha256=$RuntimeSha256;distributionReady=$manifest.distributionReady;remainingRequirements=@($manifest.remainingRequirements)}
}

$PackageDirectory=(Resolve-Path -LiteralPath $PackageDirectory).Path.TrimEnd([IO.Path]::DirectorySeparatorChar)
$Destination=[IO.Path]::GetFullPath($Destination).TrimEnd([IO.Path]::DirectorySeparatorChar)
if(-not [IO.Directory]::Exists($PackageDirectory)) {throw 'PackageDirectory must be a published launcher/application directory.'}
Assert-OrdinaryAncestors $PackageDirectory
Assert-OrdinaryAncestors $Destination
if(Test-Path -LiteralPath $Destination) {throw 'Release destination must be new; existing artifacts are never overwritten.'}
if($Destination.StartsWith($PackageDirectory+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) {throw 'Release output must be outside the published package.'}
foreach($required in @('DigiTactics.Launcher.exe','native-launcher.json','START-HERE.txt','app/DigiTactics.Desktop.exe')) {
    if(-not [IO.File]::Exists((Join-Path $PackageDirectory $required))) {throw "Published package is missing $required"}
}
$configurationBytes=[IO.File]::ReadAllBytes((Join-Path $PackageDirectory 'native-launcher.json'))
$configuration=$utf8.GetString($configurationBytes) | ConvertFrom-Json
$version=[string]$configuration.bundledVersion
$parsedVersion=$null
if($configuration.schemaVersion -ne 1 -or $version.Length -gt 32 -or $version -cnotmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\z' -or -not [Version]::TryParse($version,[ref]$parsedVersion)) {throw 'Invalid bundled native version or launcher configuration schema.'}
$notes=''
if($NotesFile) {$notes=[IO.File]::ReadAllText((Resolve-Path -LiteralPath $NotesFile).Path,$utf8)}
if($notes.Length -gt 16384) {throw 'Release notes exceed the native manifest limit (16384 characters).'}

# Enumerate before creating output so linked directories cannot be followed implicitly.
[string[]]$sources=@(Get-PackageFiles $PackageDirectory)
[Array]::Sort($sources,[StringComparer]::Ordinal)
$parent=[IO.Path]::GetDirectoryName($Destination)
[IO.Directory]::CreateDirectory($parent) | Out-Null
Assert-OrdinaryAncestors $parent
$temporary=Join-Path $parent ('.native-release-'+[Guid]::NewGuid().ToString('N')+'.partial')
[IO.Directory]::CreateDirectory($temporary) | Out-Null
$fullName="DigiTactics-native-$version-win-x64.zip"
$updateName="DigiTactics-native-update-$version-win-x64.zip"
$fullPath=Join-Path $temporary $fullName
$updatePath=Join-Path $temporary $updateName

try {
    # This is the production package writer and verifier used by the launcher.
    $packageOutput=@(& (Join-Path $PackageDirectory 'DigiTactics.Launcher.exe') 'package' (Join-Path $PackageDirectory 'app') $updatePath $version $MinimumLauncher)
    if($LASTEXITCODE -ne 0) {throw "Native update packaging exited $LASTEXITCODE"}
    $expected=($packageOutput -join "`n") | ConvertFrom-Json
    if($expected.version -cne $version -or $expected.size -ne ([IO.FileInfo]$updatePath).Length -or $expected.sha256 -cne (Get-FileDigest $updatePath)) {throw 'Native package descriptor differs from the generated update archive.'}

    $updateStream=[IO.File]::OpenRead($updatePath)
    try {
        $updateZip=[IO.Compression.ZipArchive]::new($updateStream,[IO.Compression.ZipArchiveMode]::Read,$true)
        try {
            $manifestStream=$updateZip.GetEntry('native-package.json').Open()
            $reader=[IO.StreamReader]::new($manifestStream,$utf8)
            try {$packageManifest=$reader.ReadToEnd() | ConvertFrom-Json} finally {$reader.Dispose()}
        } finally {$updateZip.Dispose()}
    } finally {$updateStream.Dispose()}
    if($packageManifest.version -cne $version -or $packageManifest.minimumLauncher -ne $MinimumLauncher -or $packageManifest.platform -cne 'win-x64' -or $packageManifest.schemaVersion -ne 1) {throw 'Native package metadata differs from release metadata.'}
    $appFiles=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
    foreach($file in $packageManifest.files) {$appFiles.Add('app/'+$file.path,$file)}

    # Full installs and in-place updates must carry exactly the same application bytes.
    # Fixed metadata and ordinal ordering make repeated assembly byte-for-byte stable.
    $files=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
    $names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $output=[IO.File]::Open($fullPath,[IO.FileMode]::CreateNew,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None)
    try {
        $zip=[IO.Compression.ZipArchive]::new($output,[IO.Compression.ZipArchiveMode]::Create,$true)
        try {
            foreach($source in $sources) {
                $relative=$source.Substring($PackageDirectory.Length+1).Replace('\','/')
                if(-not $names.Add($relative)) {throw "Duplicate release path: $relative"}
                $inputFile=[IO.File]::Open($source,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
                try {
                    $length=$inputFile.Length
                    $digest=Get-Digest $inputFile
                    $inputFile.Position=0
                    if($relative.StartsWith('app/',[StringComparison]::Ordinal)) {
                        if(-not $appFiles.ContainsKey($relative) -or $appFiles[$relative].size -ne $length -or $appFiles[$relative].sha256 -cne $digest) {throw "Application changed between update and full packaging: $relative"}
                    }
                    $entry=$zip.CreateEntry($relative,[IO.Compression.CompressionLevel]::Optimal)
                    $entry.LastWriteTime=[DateTimeOffset]::new(2000,1,1,0,0,0,[TimeSpan]::Zero)
                    $entry.ExternalAttributes=0
                    $target=$entry.Open()
                    try {$inputFile.CopyTo($target,131072)} finally {$target.Dispose()}
                    if($inputFile.Position -ne $length) {throw "Release source changed while packaging: $relative"}
                    $files.Add($relative,@{size=$length;sha256=$digest})
                } finally {$inputFile.Dispose()}
            }
        } finally {$zip.Dispose()}
        $output.Flush($true)
    } finally {$output.Dispose()}
    foreach($name in $appFiles.Keys) {if(-not $files.ContainsKey($name)) {throw "Full package is missing update content: $name"}}
    $configurationStream=[IO.MemoryStream]::new($configurationBytes,$false)
    try {if($files['native-launcher.json'].sha256 -cne (Get-Digest $configurationStream)) {throw 'Launcher configuration changed while packaging.'}} finally {$configurationStream.Dispose()}

    # Read every compressed entry back; a matching archive hash alone does not prove its contents.
    $verification=[IO.File]::OpenRead($fullPath)
    try {
        $zip=[IO.Compression.ZipArchive]::new($verification,[IO.Compression.ZipArchiveMode]::Read,$true)
        try {
            if($zip.Entries.Count -ne $files.Count) {throw 'Full release archive has an unexpected entry count.'}
            foreach($entry in $zip.Entries) {
                if(-not $files.ContainsKey($entry.FullName) -or $entry.Length -ne $files[$entry.FullName].size) {throw 'Full release archive content differs from input.'}
                $contents=$entry.Open()
                try {if((Get-Digest $contents) -cne $files[$entry.FullName].sha256) {throw "Full release archive failed verification: $($entry.FullName)"}} finally {$contents.Dispose()}
            }
        } finally {$zip.Dispose()}
    } finally {$verification.Dispose()}

    $release=[ordered]@{schemaVersion=1;version=$version;platform='win-x64';minimumLauncher=$MinimumLauncher;file=$updateName;size=[long]$expected.size;sha256=[string]$expected.sha256;notes=$notes}
    Write-Json (Join-Path $temporary 'native-latest.json') $release
    if(([IO.FileInfo](Join-Path $temporary 'native-latest.json')).Length -gt 65536) {throw 'Encoded native manifest exceeds the discovery byte limit (65536 bytes).'}
    $artifacts=@($fullName,$updateName,'native-latest.json')
    $mediaMaterials=@()
    if($MediaMaterialsArchive.Count -gt 0) {
        $runtime=Get-Content -LiteralPath (Join-Path $PackageDirectory 'app/media/provenance.json') -Raw | ConvertFrom-Json
        if($runtime.sha256 -cnotmatch '^[a-f0-9]{64}$') {throw 'Missing packaged media runtime provenance.'}
        foreach($archive in $MediaMaterialsArchive) {
            $material=Copy-MediaMaterials $archive $temporary $runtime.sha256
            $mediaMaterials+=$material
            $artifacts+=$material.file
        }
        $notice='MEDIA_RUNTIME_NOTICES.txt'
        Copy-Item -LiteralPath (Join-Path $PackageDirectory ('app/media/'+$notice)) -Destination (Join-Path $temporary $notice)
        if((Get-FileDigest (Join-Path $temporary $notice)) -cne $files['app/media/'+$notice].sha256) {throw 'Media redistribution notice changed while packaging.'}
        $artifacts+=$notice
    }
    $checksums=@()
    $reportArtifacts=@()
    foreach($name in $artifacts) {
        $path=Join-Path $temporary $name
        $digest=Get-FileDigest $path
        $checksums+="$digest  $name"
        $reportArtifacts+=[ordered]@{file=$name;size=([IO.FileInfo]$path).Length;sha256=$digest}
    }
    [IO.File]::WriteAllText((Join-Path $temporary 'SHA256SUMS.txt'),($checksums -join "`n")+"`n",$utf8)
    Write-Json (Join-Path $temporary 'native-release-artifacts.json') ([ordered]@{schemaVersion=1;tag="native-v$version";version=$version;platform='win-x64';minimumLauncher=$MinimumLauncher;releaseApproved=$false;verificationScope='local artifact integrity only';applicationFiles=$appFiles.Count;fullPackageFiles=$files.Count;mediaMaterials=$mediaMaterials;artifacts=$reportArtifacts})
    [IO.Directory]::Move($temporary,$Destination)
} catch {
    Write-Warning "Release assembly failed. Incomplete artifacts remain inactive at $temporary"
    throw
}
Write-Host "Verified native release artifacts: $Destination"
Write-Host "Expected GitHub tag: native-v$version (v$version is also supported). No release was published."
