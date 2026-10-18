$ErrorActionPreference = 'Stop'
$nativeRoot = $PSScriptRoot
$repoRoot = Split-Path -Parent $nativeRoot
$output = Join-Path $nativeRoot 'artifacts/steam-native'
$localSdk = Join-Path $env:LOCALAPPDATA 'DigiTacticsBuild/dotnet/dotnet.exe'
$dotnet = if (Test-Path -LiteralPath $localSdk) { $localSdk } else { (Get-Command dotnet).Source }
New-Item -ItemType Directory -Force -Path $output | Out-Null
. (Join-Path $nativeRoot 'tools/NativeProcess.ps1')
Invoke-Checked (Join-Path $repoRoot 'tests/native/build.cmd') @($output)
Invoke-Checked $dotnet @('run','--project',(Join-Path $nativeRoot 'tests/DigiTactics.Tests'),'-c','Release','--',$nativeRoot,'--steam-native-only')
