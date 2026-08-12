[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$DependencyPath = Join-Path $RepositoryRoot 'Documents/Project/Stage0-Recovery-Dependencies.json'
$RunbookPath = Join-Path $RepositoryRoot 'Documents/Runbooks/Stage0-Recovery-Archive.md'
$GeneratorPath = Join-Path $RepositoryRoot 'Tools/Recovery/New-Stage0-Recovery-Archive.ps1'
$VerifierPath = Join-Path $RepositoryRoot 'Tools/Recovery/Test-Stage0-Recovery-Archive.ps1'
$GlobalJsonPath = Join-Path $RepositoryRoot 'global.json'
$PlaygroundProjectPath = Join-Path $RepositoryRoot 'Tools/Windvale.Playground/Windvale.Playground.csproj'

foreach ($Path in @(
        $DependencyPath,
        $RunbookPath,
        $GeneratorPath,
        $VerifierPath,
        $GlobalJsonPath,
        $PlaygroundProjectPath
    )) {
    if (!(Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "The Stage 0 recovery archive contract is missing a required file: $Path"
    }
}

foreach ($ScriptPath in @($GeneratorPath, $VerifierPath)) {
    $Tokens = $null
    $ParseErrors = $null
    [System.Management.Automation.Language.Parser]::ParseFile(
        $ScriptPath,
        [ref]$Tokens,
        [ref]$ParseErrors) | Out-Null
    if ($ParseErrors.Count -ne 0) {
        throw (
            "Stage 0 recovery script '$ScriptPath' has parser errors: " +
            (($ParseErrors | ForEach-Object Message) -join '; ')
        )
    }
}

$Dependencies = Get-Content -LiteralPath $DependencyPath -Raw | ConvertFrom-Json
if ($Dependencies.format -ne 'windvale-stage0-recovery-dependencies-1') {
    throw 'The Stage 0 recovery dependency inventory has an unsupported format.'
}
$ExpectedHosts = @('windows-x64', 'linux-x64')
if (![Linq.Enumerable]::SequenceEqual(
        [string[]]@($Dependencies.hostProfiles),
        [string[]]$ExpectedHosts)) {
    throw 'The Stage 0 recovery dependency inventory must own Windows x64 and Linux x64.'
}

$GlobalJson = Get-Content -LiteralPath $GlobalJsonPath -Raw | ConvertFrom-Json
$DotnetDependencies = @($Dependencies.required | Where-Object { $_.name -eq '.NET SDK' })
if ($DotnetDependencies.Count -ne 1 -or
    [string]$DotnetDependencies[0].version -cne [string]$GlobalJson.sdk.version) {
    throw 'The Stage 0 recovery .NET SDK dependency differs from global.json.'
}

[xml]$PlaygroundProject = Get-Content -LiteralPath $PlaygroundProjectPath -Raw
$ExpectedPackages = @(
    $PlaygroundProject.Project.ItemGroup.PackageReference |
        ForEach-Object {
            "$($_.Include)|$($_.Version)"
        } |
        Sort-Object
)
$ActualPackages = @(
    $Dependencies.managedPackages |
        ForEach-Object { "$($_.name)|$($_.version)" } |
        Sort-Object
)
if (![Linq.Enumerable]::SequenceEqual(
        [string[]]$ActualPackages,
        [string[]]$ExpectedPackages)) {
    throw 'The Stage 0 recovery managed-package inventory differs from the retained project.'
}
if (@($Dependencies.coreRecoveryExternalPackages).Count -ne 0) {
    throw 'The core Stage 0 recovery path unexpectedly lists an external package.'
}

$Runbook = Get-Content -LiteralPath $RunbookPath -Raw
foreach ($RequiredText in @(
        'New-Stage0-Recovery-Archive.ps1',
        'Test-Stage0-Recovery-Archive.ps1',
        '-RunRecovery',
        'Windows x64',
        'Linux x64',
        'E-Worker'
    )) {
    if (!$Runbook.Contains($RequiredText)) {
        throw "The Stage 0 recovery runbook is missing '$RequiredText'."
    }
}

$Generator = Get-Content -LiteralPath $GeneratorPath -Raw
foreach ($RequiredText in @(
        'status --porcelain',
        'bundle create',
        'bundle verify',
        'ls-tree -r --full-tree HEAD',
        'SHA256SUMS'
    )) {
    if (!$Generator.Contains($RequiredText)) {
        throw "The Stage 0 recovery generator is missing '$RequiredText'."
    }
}

$Verifier = Get-Content -LiteralPath $VerifierPath -Raw
foreach ($RequiredText in @(
        'git init --quiet',
        'checkout --quiet --detach',
        'Verify-Managed-Bootstrap',
        'Rebuild-Native-Compiler-Seed',
        'Rebuild-Native-Front-Door',
        'Verify-Bootstrap'
    )) {
    if (!$Verifier.Contains($RequiredText)) {
        throw "The Stage 0 recovery verifier is missing '$RequiredText'."
    }
}

Write-Host 'Stage 0 recovery archive contract verification passed.'
