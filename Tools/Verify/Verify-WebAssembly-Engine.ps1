[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$PackageVerifier = Join-Path `
    $RepositoryRoot 'Tools/Website/Verify-WebAssembly-Playground-Package.mjs'
$CoreVerifier = Join-Path `
    $RepositoryRoot 'Tools/Website/Verify-WebAssembly-Compiler-Core.mjs'

function Invoke-EngineCheck {
    param(
        [Parameter(Mandatory)]
        [string]$Name,
        [Parameter(Mandatory)]
        [string]$Script
    )

    if (!(Test-Path -LiteralPath $Script -PathType Leaf)) {
        throw "The WebAssembly engine checkpoint owner is missing: $Script"
    }
    $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
    & node --no-liftoff $Script
    $ExitCode = $LASTEXITCODE
    $Stopwatch.Stop()
    if ($ExitCode -ne 0) {
        throw "The WebAssembly $Name check exited $ExitCode."
    }
    Write-Output "PASS webassembly-engine $Name elapsed-ms=$($Stopwatch.ElapsedMilliseconds)"
}

Invoke-EngineCheck 'package' $PackageVerifier
Invoke-EngineCheck 'compiler-core' $CoreVerifier
Write-Output 'WebAssembly engine checkpoint verification passed.'
