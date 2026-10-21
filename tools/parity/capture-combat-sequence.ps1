<#
Frame-sequence capture of a real match combat with one unit line on the board (two copies).

  powershell -NoProfile -ExecutionPolicy Bypass -File tools/parity/capture-combat-sequence.ps1 -Line renamon -Stage 0 -Frames 90 -Out parity/combat-renamon

Writes <Out>/frame_000.png ... (one frame per 1/30 s simulation tick) via
--parity-capture combat-sequence:<line>:<stage>:<frames>[:<start>] against a fresh temporary profile.
The process is windowless (CreateNoWindow, hidden GLFW window).
#>
param(
    [Parameter(Mandatory)][string]$Line,
    [int]$Stage = 0,
    [int]$Frames = 90,
    [double]$Start = 0,
    [string]$Size = '1600x900',
    [string]$Out,
    [ValidateSet('Debug','Release')][string]$Configuration = 'Debug',
    [int]$TimeoutSeconds = 600,
    [string]$Exe
)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (-not $Out) { $Out = Join-Path $repo "parity/combat-$Line-$Stage" }
$exe = if ($Exe) { (Resolve-Path -LiteralPath $Exe).Path } else { Join-Path $repo "native/src/DigiTactics.Desktop/bin/$Configuration/net10.0/win-x64/DigiTactics.Desktop.exe" }
if (-not (Test-Path -LiteralPath $exe)) { throw "Desktop build not found: $exe" }
New-Item -ItemType Directory -Force -Path $Out | Out-Null
$Out = (Resolve-Path -LiteralPath $Out).Path
Get-ChildItem -LiteralPath $Out -Filter 'frame_*.png' | Remove-Item

function Invoke-Hidden([string[]]$Arguments, [string]$Log) {
    $info = New-Object System.Diagnostics.ProcessStartInfo
    $info.FileName = $exe
    $info.Arguments = ($Arguments | ForEach-Object { if ($_ -match '[\s"]') { '"' + ($_ -replace '"', '\"') + '"' } else { $_ } }) -join ' '
    $info.UseShellExecute = $false; $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true; $info.RedirectStandardError = $true
    $info.WorkingDirectory = Split-Path -Parent $exe
    $process = [System.Diagnostics.Process]::Start($info)
    $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) { $process.Kill(); $process.WaitForExit(); $code = -1 } else { $code = $process.ExitCode }
    [IO.File]::WriteAllText($Log, $stdout.Result + $stderr.Result)
    return $code
}

$profileDir = Join-Path $env:TEMP ('digitactics-sequence-' + [Guid]::NewGuid().ToString('N'))
$fixture = Join-Path $repo 'native/tests/fixtures/collection-profile.json'
try {
    $code = Invoke-Hidden @('--import-profile', $fixture, '--profile-directory', $profileDir) (Join-Path $Out 'import.log')
    if ($code -ne 0) { throw "Profile import failed ($code)" }
    $state = "combat-sequence:${Line}:${Stage}:${Frames}"
    if ($Start -gt 0) { $state += ':' + $Start.ToString([Globalization.CultureInfo]::InvariantCulture) }
    $code = Invoke-Hidden @('--parity-capture', $state, '--capture-size', $Size, '--capture', (Join-Path $Out 'frame.png'), '--profile-directory', $profileDir) (Join-Path $Out 'capture.log')
    Get-Content (Join-Path $Out 'capture.log') | Select-Object -Last 3
    if ($code -ne 0) { throw "Sequence capture failed ($code); see $Out/capture.log" }
} finally { if (Test-Path -LiteralPath $profileDir) { Remove-Item -Recurse -Force -LiteralPath $profileDir } }
