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
    Invoke-Checked 'node' @('tools/export-economy.cjs')
    Invoke-Checked 'node' @('tools/export-planning.cjs')
    Invoke-Checked 'node' @('tools/export-match-state.cjs')
    Invoke-Checked 'node' @('tools/export-settlement.cjs')
    Invoke-Checked 'node' @('tools/export-mode-rewards.cjs')
    Invoke-Checked 'node' @('tools/export-offers.cjs')
    Invoke-Checked 'node' @('tools/export-round-flow.cjs')
    Invoke-Checked 'node' @('tools/export-custom-offers.cjs')
    Invoke-Checked 'node' @('tools/export-planning-items.cjs')
    Invoke-Checked 'node' @('tools/export-consumables.cjs')
    Invoke-Checked 'node' @('tools/export-copy-consumables.cjs')
    Invoke-Checked 'node' @('tools/export-special-consumables.cjs')
    Invoke-Checked 'node' @('tools/export-coliseum-core.cjs')
    Invoke-Checked 'node' @('tools/export-coliseum-choices.cjs')
    Invoke-Checked 'node' @('tools/export-coliseum-shop.cjs')
    Invoke-Checked 'node' @('tools/export-coliseum-rounds.cjs')
    Invoke-Checked 'node' @('tools/export-coliseum-settlement.cjs')
    Invoke-Checked 'node' @('tools/export-ai-holders.cjs')
    Invoke-Checked 'node' @('tools/export-ai-decisions.cjs')
    Invoke-Checked 'node' @('tools/export-ai-shopping.cjs')
    Invoke-Checked 'node' @('tools/export-portals.cjs')
    Invoke-Checked 'node' @('tools/export-duos-planning.cjs')
    Invoke-Checked 'node' @('tools/export-ai-offers.cjs')
    Invoke-Checked 'node' @('tools/export-ai-custom-offers.cjs')
    Invoke-Checked 'node' @('tools/export-relic-upgrades.cjs')
    Invoke-Checked 'node' @('tools/export-underlords-offers.cjs')
    Invoke-Checked 'node' @('tools/export-arcane-offers.cjs')
    Invoke-Checked 'node' @('tools/export-ai-turns.cjs')
    Invoke-Checked 'node' @('tools/export-carousels.cjs')
    Invoke-Checked 'node' @('tools/export-autochess-planning.cjs')
    Invoke-Checked 'node' @('tools/export-shop-rolls.cjs')
    Invoke-Checked 'node' @('tools/export-fates-partners.cjs')
    Invoke-Checked 'node' @('tools/export-dragonlands-shops.cjs')
    Invoke-Checked 'node' @('tools/export-match-shop-rolls.cjs')
    Invoke-Checked 'node' @('tools/export-match-start.cjs')
    Invoke-Checked 'node' @('tools/export-opening-rounds.cjs')
    Invoke-Checked 'node' @('tools/export-galaxies-planning.cjs')
    Invoke-Checked 'node' @('tools/export-galaxies-settlement.cjs')
    Invoke-Checked 'node' @('tools/export-fates-ai.cjs')
    Invoke-Checked 'node' @('tools/export-fates-settlement.cjs')
    Invoke-Checked 'node' @('tools/export-match-placement.cjs')
    Invoke-Checked 'node' @('tools/export-underlords-tamers.cjs')
    Invoke-Checked 'node' @('tools/export-underlords-planning.cjs')
    Invoke-Checked 'node' @('tools/export-underlords-settlement.cjs')
    Invoke-Checked 'node' @('tools/export-dragon-planning.cjs')
    Invoke-Checked 'node' @('tools/export-dragon-settlement.cjs')
    Invoke-Checked 'node' @('tools/export-dragon-augments.cjs')
    Invoke-Checked 'node' @('tools/export-fates-trait-rewards.cjs')
    Invoke-Checked 'node' @('tools/export-fates-augments.cjs')
    Invoke-Checked 'node' @('tools/export-gizmos-planning.cjs')
    Invoke-Checked 'node' @('tools/export-gizmos-augments.cjs')
    Invoke-Checked 'node' @('tools/export-galaxy-augments.cjs')
    Invoke-Checked 'node' @('tools/export-autochess-priest.cjs')
    Invoke-Checked 'node' @('tools/export-arcane-rounds.cjs')
    Invoke-Checked 'node' @('tools/export-arcane-augments.cjs')
    Invoke-Checked 'node' @('tools/export-arcane-ai.cjs')
    Invoke-Checked 'node' @('tools/export-arcane-settlement.cjs')
    Invoke-Checked 'node' @('tools/export-reckoning-planning.cjs')
    Invoke-Checked 'node' @('tools/export-reckoning-ai.cjs')
    Invoke-Checked 'node' @('tools/export-ai-inventory.cjs')
    Invoke-Checked 'node' @('tools/export-models.cjs')
    Invoke-Checked 'node' @('tools/export-motions.cjs')
    Invoke-Checked 'node' @('tools/export-material-fixtures.cjs')
    Invoke-Checked 'node' @('tools/export-mesh-fixtures.cjs')
    Invoke-Checked 'node' @('tools/export-rig-fixtures.cjs')
    Invoke-Checked 'node' @('tools/export-damage-fixtures.cjs')
    Invoke-Checked 'node' @('tools/export-combat-fixtures.cjs')
    Invoke-Checked 'node' @('tools/export-event-fixtures.cjs')
    Invoke-Checked 'node' @('tools/export-scene-fixtures.cjs')
    Invoke-Checked 'node' @('tools/export-choreography.cjs')
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
