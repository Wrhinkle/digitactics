<#
Native parity captures: parity/<state>/<size>/native.png (same layout as capture-electron.cjs).

  powershell -NoProfile -ExecutionPolicy Bypass -File tools/parity/capture-native.ps1
  powershell -NoProfile -ExecutionPolicy Bypass -File tools/parity/capture-native.ps1 -States home,store -Sizes 1600x900
  powershell -NoProfile -ExecutionPolicy Bypass -File tools/parity/capture-native.ps1 -Model catalog:CS_Renamon:chr391 -ModelTimes attack=0.3,special=0.6,win=0.5

Each capture runs DigiTactics.Desktop.exe (GUI subsystem, hidden GLFW window, CreateNoWindow) with
--parity-capture <state> --capture-size WxH --capture <png> against a fresh temporary profile that has
native/tests/fixtures/collection-profile.json imported. Nothing touches the real player profile.
Model mode writes parity/model-<key>/native.png (strip: portrait | idle | attack | special | win, board idle | board attack).
Requires a built Desktop: native/build.ps1 -Configuration Debug -SkipMedia -SkipExport -BuildOnly
#>
param(
    [string[]]$States,
    [string[]]$Sizes = @('1280x720','1600x900','1920x1080'),
    [string]$Out,
    [ValidateSet('Debug','Release')][string]$Configuration = 'Debug',
    [string]$Model,
    [string]$ModelTimes,
    [int]$SettleFrames = 45,
    [double]$CombatSeconds = 2,
    [int]$TimeoutSeconds = 600
)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (-not $Out) { $Out = Join-Path $repo 'parity' }
$exe = Join-Path $repo "native/src/DigiTactics.Desktop/bin/$Configuration/net10.0/win-x64/DigiTactics.Desktop.exe"
if (-not (Test-Path -LiteralPath $exe)) { throw "Desktop build not found: $exe. Run native/build.ps1 -Configuration $Configuration -SkipMedia -SkipExport -BuildOnly first." }
$fixture = Join-Path $repo 'native/tests/fixtures/collection-profile.json'
$logs = Join-Path $Out '_logs'
New-Item -ItemType Directory -Force -Path $Out, $logs | Out-Null
# Comma-separated values from cmd/bash arrive as one string; split them.
if ($States) { $States = @($States | ForEach-Object { $_ -split ',' } | Where-Object { $_ }) }
$Sizes = @($Sizes | ForEach-Object { $_ -split ',' } | Where-Object { $_ })

function Invoke-Hidden([string[]]$Arguments, [string]$Log) {
    $info = New-Object System.Diagnostics.ProcessStartInfo
    $info.FileName = $exe
    $info.Arguments = ($Arguments | ForEach-Object { if ($_ -match '[\s"]') { '"' + ($_ -replace '"', '\"') + '"' } else { $_ } }) -join ' '
    $info.UseShellExecute = $false; $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true; $info.RedirectStandardError = $true
    $info.WorkingDirectory = Split-Path -Parent $exe
    $process = [System.Diagnostics.Process]::Start($info)
    $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) { $process.Kill(); $process.WaitForExit() ; $code = -1 } else { $code = $process.ExitCode }
    [IO.File]::WriteAllText($Log, $stdout.Result + $stderr.Result)
    return $code
}

function New-Profile {
    $profileDir = Join-Path $env:TEMP ('digitactics-parity-' + [Guid]::NewGuid().ToString('N'))
    $code = Invoke-Hidden @('--import-profile', $fixture, '--profile-directory', $profileDir) (Join-Path $logs 'import.log')
    if ($code -ne 0) { throw "Profile fixture import failed ($code); see $logs/import.log" }
    return $profileDir
}

$report = New-Object System.Collections.ArrayList
$all = [Diagnostics.Stopwatch]::StartNew()
if ($Model) {
    $safe = $Model -replace '[:\\/]', '_'
    $directory = Join-Path $Out ('model-' + $safe)
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    $arguments = @('--model-sweep', $directory, '--sweep-filter', $Model)
    if ($ModelTimes) { $arguments += @('--sweep-times', $ModelTimes) }
    $code = Invoke-Hidden $arguments (Join-Path $logs "model-$safe.log")
    Get-Content (Join-Path $logs "model-$safe.log") | Select-Object -Last 3
    $png = Get-ChildItem -LiteralPath $directory -Filter '????.png' | Select-Object -First 1
    if ($code -eq 0 -and $png) { Copy-Item -LiteralPath $png.FullName -Destination (Join-Path $directory 'native.png') -Force; Write-Output "native model strip: $(Join-Path $directory 'native.png')" }
    else { throw "Model capture failed ($code); see $logs/model-$safe.log" }
    return
}
if (-not $States) { $States = @((Get-Content -Raw (Join-Path $PSScriptRoot 'states.json') | ConvertFrom-Json).states | ForEach-Object { $_.id }) }
foreach ($state in $States) {
    foreach ($size in $Sizes) {
        $directory = Join-Path $Out "$state/$size"
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
        $png = Join-Path $directory 'native.png'
        if (Test-Path -LiteralPath $png) { Remove-Item -LiteralPath $png }
        $profileDir = New-Profile
        $timer = [Diagnostics.Stopwatch]::StartNew()
        $log = Join-Path $logs "$state-$size.log"
        $arguments = @('--parity-capture', $state, '--capture-size', $size, '--capture', $png, '--profile-directory', $profileDir,
            '--parity-settle-frames', "$SettleFrames", '--parity-combat-seconds', ([string]::Format([Globalization.CultureInfo]::InvariantCulture, '{0}', $CombatSeconds)))
        # The lobby browser needs a Steam connection: use the fake Steam library (built by native/test-steam.ps1),
        # like the Electron capture's MockSteamHub.
        $fakeSteam = Join-Path $repo 'native/artifacts/steam-native/fake_steam_api.dll'
        if ($state -eq 'lobby' -and (Test-Path -LiteralPath $fakeSteam)) { $arguments += @('--steam-api', $fakeSteam) }
        $code = Invoke-Hidden $arguments $log
        $ok = $code -eq 0 -and (Test-Path -LiteralPath $png)
        $line = "{0,-24} {1,-10} {2} {3,6:0.0}s" -f $state, $size, $(if ($ok) { 'ok  ' } else { "FAIL($code)" }), $timer.Elapsed.TotalSeconds
        Write-Output $line
        [void]$report.Add([ordered]@{ state = $state; size = $size; ok = $ok; exitCode = $code; seconds = [Math]::Round($timer.Elapsed.TotalSeconds, 1); image = $(if ($ok) { $png } else { $null }); log = $log })
        Remove-Item -LiteralPath $profileDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
$summary = [ordered]@{ generated = (Get-Date).ToString('o'); seconds = [Math]::Round($all.Elapsed.TotalSeconds, 1); captures = $report }
[IO.File]::WriteAllText((Join-Path $Out 'native-report.json'), ($summary | ConvertTo-Json -Depth 4))
Write-Output ("native captures: {0}/{1} ok in {2:0}s; report {3}" -f @($report | Where-Object { $_.ok }).Count, $report.Count, $all.Elapsed.TotalSeconds, (Join-Path $Out 'native-report.json'))
