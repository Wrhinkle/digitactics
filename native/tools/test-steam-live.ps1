param(
    [Parameter(Mandatory=$true)][string]$Desktop,
    [string]$LogPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts/steam-one-account-live.log')
)
$ErrorActionPreference = 'Stop'
$desktopPath = (Resolve-Path -LiteralPath $Desktop).Path
if ([IO.Path]::GetFileName($desktopPath) -cne 'DigiTactics.Desktop.exe') { throw 'Provide the built DigiTactics.Desktop.exe.' }
if (-not (Test-Path -LiteralPath (Join-Path ([IO.Path]::GetDirectoryName($desktopPath)) 'steam_api64.dll'))) { throw 'The packaged steam_api64.dll is missing beside the executable.' }
$logFullPath = [IO.Path]::GetFullPath($LogPath)
New-Item -ItemType Directory -Force -Path ([IO.Path]::GetDirectoryName($logFullPath)) | Out-Null
$savedDotnetRoot = $env:DOTNET_ROOT
$localSdk = Join-Path $env:LOCALAPPDATA 'DigiTacticsBuild/dotnet'
if (-not $env:DOTNET_ROOT -and (Test-Path -LiteralPath (Join-Path $localSdk 'dotnet.exe'))) { $env:DOTNET_ROOT = $localSdk }
$process = New-Object System.Diagnostics.Process
try {
    $process.StartInfo.FileName = $desktopPath
    $process.StartInfo.Arguments = '--steam-live-smoke'
    $process.StartInfo.WorkingDirectory = [IO.Path]::GetDirectoryName($desktopPath)
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.CreateNoWindow = $true
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    if (-not $process.Start()) { throw 'Could not start the opt-in Steam check.' }
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(150000)) {
        $process.Kill()
        $process.WaitForExit()
        throw 'The opt-in Steam check exceeded its bounded timeout.'
    }
    $exitCode = $process.ExitCode
    # The native SDK writes the account's SteamID to stderr during startup even
    # though the managed check logs no names or identities. Retain diagnostics,
    # including SDK assertions, while removing identifiers before output/storage.
    $report = $stdout.GetAwaiter().GetResult() + "`nNative SDK diagnostics:`n" + $stderr.GetAwaiter().GetResult()
    $report = [regex]::Replace($report, '\b[0-9]{17}\b', '[Steam identity omitted]')
    [IO.File]::WriteAllText($logFullPath, $report)
    Write-Output $report
    if ($exitCode -eq 2) { Write-Output 'No live checks ran. Sign into Steam before retrying.' }
    elseif ($exitCode -ne 0) { throw "Steam live smoke failed with exit code $exitCode. See the sanitized report: $logFullPath" }
    exit $exitCode
}
finally {
    $process.Dispose()
    $env:DOTNET_ROOT = $savedDotnetRoot
}
