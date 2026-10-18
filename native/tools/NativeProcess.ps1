function Invoke-Checked([string]$Program, [string[]]$Arguments) {
    $executable = @(Get-Command -Name $Program -CommandType Application -ErrorAction Stop)[0].Source
    # Windows PowerShell turns redirected native stderr into ErrorRecords.
    # Keep diagnostics as text; the process exit code determines success.
    $savedErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $global:LASTEXITCODE = $null
        & $executable @Arguments 2>&1 | ForEach-Object {
            if ($_ -is [System.Management.Automation.ErrorRecord]) {
                if ($_.FullyQualifiedErrorId -notlike 'NativeCommandError*') { throw $_ }
                $_.ToString()
            } else { $_ }
        }
        $code = $LASTEXITCODE
    } finally { $ErrorActionPreference = $savedErrorActionPreference }
    if ($code -ne 0) { throw "$Program failed with exit code $code" }
}
