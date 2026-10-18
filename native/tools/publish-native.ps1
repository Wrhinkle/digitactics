param([Parameter(Mandatory=$true)][string]$Destination,[Parameter(Mandatory=$true)][string]$Version)
$ErrorActionPreference='Stop'
if($Version -notmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\z') {throw 'Use a three-part numeric native version.'}
if($Version.Length -gt 32 -or -not [Version]::TryParse($Version,[ref]([Version]$null))) {throw 'Native version exceeds supported numeric bounds.'}
$nativeRoot=Split-Path -Parent $PSScriptRoot
$repoRoot=Split-Path -Parent $nativeRoot
$Destination=[IO.Path]::GetFullPath($Destination)
if(Test-Path -LiteralPath $Destination) {throw 'Publish destination must be new; existing packages are never overwritten.'}
$ancestor=$Destination
while($ancestor) {
    if((Test-Path -LiteralPath $ancestor) -and (([IO.File]::GetAttributes($ancestor) -band [IO.FileAttributes]::ReparsePoint) -ne 0)) {throw 'Publish outside linked or cloud-placeholder directories, for example under LOCALAPPDATA/DigiTacticsBuild/releases.'}
    $ancestor=[IO.Path]::GetDirectoryName($ancestor)
}
$sdk=Join-Path $env:LOCALAPPDATA 'DigiTacticsBuild/dotnet/dotnet.exe'
if(-not(Test-Path -LiteralPath $sdk)) {$sdk=(Get-Command dotnet).Source}
. (Join-Path $PSScriptRoot 'NativeProcess.ps1')
$app=Join-Path $Destination 'app'
$steamRuntime = & (Join-Path $PSScriptRoot 'setup-steam.ps1')
$mediaRuntime = & (Join-Path $PSScriptRoot 'setup-media.ps1')
$crtRuntime = & (Join-Path $PSScriptRoot 'setup-crt.ps1')
Invoke-Checked $sdk @('publish',(Join-Path $nativeRoot 'src/DigiTactics.Launcher'),'-c','Release','-r','win-x64','--self-contained','true','-m:1','-nr:false','-p:UseSharedCompilation=false',('-p:Version='+$Version),'-o',$Destination)
Invoke-Checked $sdk @('publish',(Join-Path $nativeRoot 'src/DigiTactics.Desktop'),'-c','Release','-r','win-x64','--self-contained','true','-m:1','-nr:false','-p:UseSharedCompilation=false',('-p:Version='+$Version),'-o',$app)
foreach($assembly in @((Join-Path $Destination 'DigiTactics.Launcher.dll'),(Join-Path $app 'DigiTactics.Desktop.dll'))) {
    $metadata=[Diagnostics.FileVersionInfo]::GetVersionInfo($assembly)
    if($metadata.ProductVersion.Split('+')[0] -cne $Version -or $metadata.FileVersion -cne ($Version+'.0')) {throw "Published assembly version does not match release $Version`: $assembly"}
}
$steamPin=Get-Content -LiteralPath (Join-Path $steamRuntime 'steam-runtime-provenance.json') -Raw | ConvertFrom-Json
if ((Get-FileHash -LiteralPath (Join-Path $app 'steam_api64.dll') -Algorithm SHA256).Hash.ToLowerInvariant() -cne $steamPin.dllSha256) {throw 'Published Steam DLL is missing or does not match its pin.'}
$expectedMedia=@(Get-ChildItem -LiteralPath $mediaRuntime -File -Recurse)
$publishedMedia=Join-Path $app 'media'
$actualMedia=@(Get-ChildItem -LiteralPath $publishedMedia -File -Recurse)
if($actualMedia.Count -ne $expectedMedia.Count) {throw 'Published audio runtime contains unexpected or missing files.'}
foreach($sourceFile in $expectedMedia) {
    $relative=$sourceFile.FullName.Substring($mediaRuntime.TrimEnd('\','/').Length+1)
    $publishedFile=Join-Path $publishedMedia $relative
    if(-not(Test-Path -LiteralPath $publishedFile -PathType Leaf) -or (Get-FileHash -LiteralPath $sourceFile.FullName -Algorithm SHA256).Hash -cne (Get-FileHash -LiteralPath $publishedFile -Algorithm SHA256).Hash) {throw "Published audio runtime differs from verified runtime: $relative"}
}
foreach($file in @('CREDITS.md','THIRD_PARTY_NOTICES.txt')) {Copy-Item -LiteralPath (Join-Path $repoRoot $file) -Destination $app}
Copy-Item -LiteralPath (Join-Path $nativeRoot 'README.md') -Destination $app
$assets=Get-Content (Join-Path $nativeRoot 'src/DigiTactics.Desktop/obj/project.assets.json') -Raw | ConvertFrom-Json
$packageRoot=$assets.packageFolders.PSObject.Properties.Name | Select-Object -First 1
foreach($library in $assets.libraries.PSObject.Properties) {
    if($library.Name -notlike 'Ahwoo.Brutal.*') {continue}
    $source=Join-Path $packageRoot $library.Value.path
    $notices=Join-Path $app ('LICENSES/'+$library.Name.Replace('/','-'))
    New-Item -ItemType Directory -Force -Path $notices | Out-Null
    foreach($name in @('LICENSE.txt','NOTICE.txt','CONTRIBUTORS.md','LICENSES')) {
        $item=Join-Path $source $name
        if(Test-Path -LiteralPath $item) {Copy-Item -LiteralPath $item -Destination $notices -Recurse}
    }
    if($library.Name -like 'Ahwoo.Brutal.Core/*') {Copy-Item -LiteralPath (Join-Path $source 'NOTICE.txt') -Destination $app}
}
Invoke-Checked 'python' @((Join-Path $PSScriptRoot 'verify-native-dependencies.py'),$app,'--require-brutal')
Invoke-Checked 'python' @((Join-Path $PSScriptRoot 'verify-native-dependencies.py'),$Destination)
[IO.File]::WriteAllText((Join-Path $Destination 'native-launcher.json'),(@{schemaVersion=1;bundledVersion=$Version;updateOwner="Wrhinkle";updateRepository="digitactics"}|ConvertTo-Json -Compress))
[IO.File]::WriteAllText((Join-Path $Destination 'START-HERE.txt'),"Start DigiTactics.Launcher.exe to play.`r`nKeep the app folder beside the launcher. Saves and staged updates are stored under your Windows user account.`r`n")
$savedRoot=$env:DOTNET_ROOT;$savedX64=$env:DOTNET_ROOT_X64;$savedLookup=$env:DOTNET_MULTILEVEL_LOOKUP
try {
    # Prove the launcher and game resolve their bundled runtime, independently of the build SDK.
    $env:DOTNET_ROOT=Join-Path $Destination 'no-installed-dotnet'
    $env:DOTNET_ROOT_X64=$env:DOTNET_ROOT;$env:DOTNET_MULTILEVEL_LOOKUP='0'
    $state=Join-Path $env:TEMP ('digitactics-package-smoke-'+[Guid]::NewGuid().ToString('N'))
    Invoke-Checked (Join-Path $app 'DigiTactics.Desktop.exe') @('--ui-frame-lifetime-smoke')
    Invoke-Checked (Join-Path $app 'DigiTactics.Desktop.exe') @('--smoke')
    Invoke-Checked (Join-Path $Destination 'DigiTactics.Launcher.exe') @('run','--no-updates','--state',$state,'--','--match-smoke','--minimum-window')
    Invoke-Checked (Join-Path $app 'DigiTactics.Desktop.exe') @('--custom-music-smoke')
    Invoke-Checked (Join-Path $app 'DigiTactics.Desktop.exe') @('--music-routing-smoke')
    & (Join-Path $PSScriptRoot 'test-packaged-update.ps1') -PackageDirectory $Destination
} finally {$env:DOTNET_ROOT=$savedRoot;$env:DOTNET_ROOT_X64=$savedX64;$env:DOTNET_MULTILEVEL_LOOKUP=$savedLookup}
Write-Host "Self-contained native launcher/game layout verified: $Destination"
