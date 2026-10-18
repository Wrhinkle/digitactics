param([Parameter(Mandatory=$true)][string]$PackageDirectory)
$ErrorActionPreference='Stop'
$PackageDirectory=(Resolve-Path -LiteralPath $PackageDirectory).Path
$launcher=Join-Path $PackageDirectory 'DigiTactics.Launcher.exe'
$config=Get-Content (Join-Path $PackageDirectory 'native-launcher.json') -Raw | ConvertFrom-Json
$version=[Version]$config.bundledVersion
if($version.Build -eq [int]::MaxValue) {throw 'Cannot increment patch version for isolated update test.'}
$testVersion='{0}.{1}.{2}' -f $version.Major,$version.Minor,($version.Build+1)
$testRoot=Join-Path $env:TEMP ('digitactics-packaged-update-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot | Out-Null
$archive=Join-Path $testRoot 'test-update.zip'
$descriptor=Join-Path $testRoot 'test-update.json'
$stateRoot=Join-Path $testRoot 'state'
function Run-Launcher([string[]]$Arguments) {
    $result=& $launcher @Arguments
    if($LASTEXITCODE -ne 0) {throw "Packaged launcher exited $LASTEXITCODE ($Arguments)"}
    return $result
}
$metadata=Run-Launcher @('package',(Join-Path $PackageDirectory 'app'),$archive,$testVersion)
[IO.File]::WriteAllText($descriptor,($metadata -join "`n"))
Run-Launcher @('stage',$archive,$descriptor,'--state',$stateRoot)
Run-Launcher @('run','--state',$stateRoot,'--','--match-smoke','--minimum-window')
$saved=Get-Content (Join-Path $stateRoot 'launcher-state.json') -Raw | ConvertFrom-Json
if($saved.active.version -ne $testVersion -or $null -ne $saved.trial -or $null -ne $saved.pending) {throw 'Packaged desktop did not confirm the staged update.'}
if($saved.active.manifestSha256.Length -ne 64) {throw 'Confirmed candidate lost manifest pin.'}
Write-Host "Packaged desktop update passed: archive, stage, leased candidate launch and rendered-start confirmation. Isolated fixture: $testRoot"
