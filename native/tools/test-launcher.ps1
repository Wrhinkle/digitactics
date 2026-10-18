param([Parameter(Mandatory=$true)][string]$Launcher,[Parameter(Mandatory=$true)][string]$TestsDirectory)
$ErrorActionPreference='Stop'
$Launcher=(Resolve-Path -LiteralPath $Launcher).Path
$TestsDirectory=(Resolve-Path -LiteralPath $TestsDirectory).Path
$testRoot=Join-Path $env:TEMP ('digitactics-launcher-smoke-'+[Guid]::NewGuid().ToString('N'))
$bundle=Join-Path $testRoot 'bundle'
$stateRoot=Join-Path $testRoot 'state'
New-Item -ItemType Directory -Path $bundle | Out-Null
Copy-Item -Path (Join-Path $TestsDirectory '*') -Destination $bundle
Copy-Item -LiteralPath (Join-Path $bundle 'DigiTactics.Tests.exe') -Destination (Join-Path $bundle 'DigiTactics.Desktop.exe')
function Run-Launcher([string[]]$Arguments) {
    $result=& $Launcher @Arguments
    if($LASTEXITCODE -ne 0) {throw "Launcher exited $LASTEXITCODE ($Arguments)"}
    return $result
}
$archive=Join-Path $testRoot 'candidate.zip'
$descriptor=Join-Path $testRoot 'candidate.json'
$metadata=Run-Launcher @('package',$bundle,$archive,'1.1.0')
[IO.File]::WriteAllText($descriptor,($metadata -join "`n"))
$common=@('--bundle',$bundle,'--version','1.0.0','--state',$stateRoot)
Run-Launcher (@('stage',$archive,$descriptor)+$common)
Run-Launcher (@('run')+$common+@('--','--native-supervisor-child','ready'))
$statePath=Join-Path $stateRoot 'launcher-state.json'
$saved=Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
if($saved.active.version -ne '1.1.0' -or $null -ne $saved.trial) {throw 'Candidate did not confirm'}
Run-Launcher (@('run')+$common+@('--','--native-supervisor-child','ready'))
$candidateExe=Join-Path (Join-Path (Join-Path $stateRoot 'candidates') $saved.active.candidate) 'DigiTactics.Desktop.exe'
[IO.File]::AppendAllText($candidateExe,'corruption')
Run-Launcher (@('run')+$common+@('--','--native-supervisor-child','ready'))
$saved=Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
if($null -ne $saved.active -or $saved.badVersions -notcontains '1.1.0') {throw 'Corrupted active candidate did not fall back'}
Write-Host "Launcher executable smoke passed: package, stage, confirm, restart, corruption rejection and bundled fallback. Fixture: $testRoot"
