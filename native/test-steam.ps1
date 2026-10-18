$ErrorActionPreference = 'Stop'
$nativeRoot = $PSScriptRoot
$repoRoot = Split-Path -Parent $nativeRoot
$output = Join-Path $nativeRoot 'artifacts/steam-native'
$localSdk = Join-Path $env:LOCALAPPDATA 'DigiTacticsBuild/dotnet/dotnet.exe'
$dotnet = if (Test-Path -LiteralPath $localSdk) { $localSdk } else { (Get-Command dotnet).Source }
New-Item -ItemType Directory -Force -Path $output | Out-Null
& (Join-Path $repoRoot 'tests/native/build.cmd') $output
if ($LASTEXITCODE -ne 0) { throw 'Could not build the Steam test DLL; MSVC x64 tools are required.' }
& $dotnet run --project (Join-Path $nativeRoot 'tests/DigiTactics.Tests') -c Release -- $nativeRoot --steam-native-only
if ($LASTEXITCODE -ne 0) { throw 'Native Steam message verification failed.' }
