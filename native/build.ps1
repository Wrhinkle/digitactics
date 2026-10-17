param([switch]$Publish)
$ErrorActionPreference = 'Stop'
$nativeRoot = $PSScriptRoot
$repoRoot = Split-Path -Parent $nativeRoot
$localSdk = Join-Path $env:LOCALAPPDATA 'DigiTacticsBuild\dotnet\dotnet.exe'
$dotnet = if (Test-Path -LiteralPath $localSdk) { $localSdk } else { (Get-Command dotnet).Source }
function Invoke-Checked([string]$Program, [string[]]$Arguments) {
    & $Program @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Program failed with exit code $LASTEXITCODE" }
}
Push-Location $nativeRoot
try {
    Invoke-Checked 'node' @('tools/export-content.cjs')
    Invoke-Checked 'node' @('tools/export-damage-fixtures.cjs')
    Invoke-Checked $dotnet @('restore', 'src/DigiTactics.Desktop', '--locked-mode')
    Invoke-Checked $dotnet @('run', '--project', 'tests/DigiTactics.Tests', '-c', 'Release', '--', $nativeRoot)
    Invoke-Checked $dotnet @('build', 'src/DigiTactics.Desktop', '-c', 'Release', '--no-restore')
    Invoke-Checked $dotnet @('run', '--project', 'src/DigiTactics.Desktop', '-c', 'Release', '--no-build', '--', '--smoke', '--capture', 'artifacts/native-smoke.bmp')
    if ($Publish) {
        $destination = Join-Path $nativeRoot 'artifacts\win-x64'
        Invoke-Checked $dotnet @('publish', 'src/DigiTactics.Desktop', '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-o', $destination)
        Copy-Item -LiteralPath (Join-Path $nativeRoot 'README.md') -Destination $destination
        Copy-Item -LiteralPath (Join-Path $repoRoot 'CREDITS.md') -Destination $destination
        Copy-Item -LiteralPath (Join-Path $repoRoot 'THIRD_PARTY_NOTICES.txt') -Destination $destination
        # Preserve notices for every pinned BRUTAL package and its bundled native dependencies.
        $assets = Get-Content 'src/DigiTactics.Desktop/obj/project.assets.json' -Raw | ConvertFrom-Json
        $packageRoot = $assets.packageFolders.PSObject.Properties.Name | Select-Object -First 1
        foreach ($library in $assets.libraries.PSObject.Properties) {
            if ($library.Name -notlike 'Ahwoo.Brutal.*') { continue }
            $source = Join-Path $packageRoot $library.Value.path
            $notices = Join-Path $destination ('LICENSES\' + $library.Name.Replace('/', '-'))
            New-Item -ItemType Directory -Force -Path $notices | Out-Null
            foreach ($name in @('LICENSE.txt', 'NOTICE.txt', 'CONTRIBUTORS.md', 'LICENSES')) {
                $item = Join-Path $source $name
                if (Test-Path -LiteralPath $item) { Copy-Item -LiteralPath $item -Destination $notices -Recurse -Force }
            }
            if ($library.Name -like 'Ahwoo.Brutal.Core/*') {
                Copy-Item -LiteralPath (Join-Path $source 'NOTICE.txt') -Destination $destination
            }
        }
        Invoke-Checked (Join-Path $destination 'DigiTactics.Desktop.exe') @('--smoke')
        Write-Host "Native migration build: $destination"
    }
} finally { Pop-Location }
