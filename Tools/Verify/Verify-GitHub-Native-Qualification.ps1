[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$WorkflowPath = Join-Path $RepositoryRoot '.github/workflows/verify.yml'
$InventoryVerifier = Join-Path $PSScriptRoot 'Verify-Dotnet-Retirement-Inventory.ps1'
$InventoryPath = Join-Path $RepositoryRoot 'Documents/Project/Dotnet-Retirement-Inventory.json'
$Content = Get-Content -LiteralPath $WorkflowPath -Raw
$Lines = @(Get-Content -LiteralPath $WorkflowPath)

function Assert-Workflow {
    param(
        [Parameter(Mandatory)][bool]$Condition,
        [Parameter(Mandatory)][string]$Message
    )
    if (!$Condition) {
        throw $Message
    }
}

function Get-JobBlock {
    param([Parameter(Mandatory)][string]$Job)

    $Start = -1
    for ($Index = 0; $Index -lt $Lines.Count; $Index++) {
        if ($Lines[$Index] -eq "  ${Job}:") {
            $Start = $Index
            break
        }
    }
    Assert-Workflow ($Start -ge 0) "The GitHub workflow is missing job '$Job'."

    $End = $Lines.Count
    for ($Index = $Start + 1; $Index -lt $Lines.Count; $Index++) {
        if ($Lines[$Index] -match '^  [a-z0-9-]+:$') {
            $End = $Index
            break
        }
    }
    ($Lines[$Start..($End - 1)] -join "`n")
}

Assert-Workflow ($Content -notmatch '(?im)Verify-Seed\.(?:ps1|sh)') `
    'The normal GitHub workflow invokes a managed Seed recovery verifier.'
Assert-Workflow ($Content -notmatch "`t") 'The GitHub workflow contains a tab.'
Assert-Workflow (
    $Content.Contains(
        '  group: verify-${{ github.workflow }}-${{ github.ref }}',
        [StringComparison]::Ordinal) -and
    $Content.Contains('  cancel-in-progress: true', [StringComparison]::Ordinal)
) 'The GitHub workflow does not cancel superseded runs on the same ref.'
Assert-Workflow (
    ([regex]::Matches($Content, '\$\{\{').Count -eq
        [regex]::Matches($Content, '\}\}').Count)
) 'The GitHub workflow has unbalanced expression delimiters.'

$ExpectedJobs = @(
    'classify-changes',
    'lightweight-verifier',
    'website-verifier',
    'windows-development',
    'linux-development',
    'windows-native-suite',
    'linux-native-suite',
    'windows-webassembly',
    'linux-webassembly',
    'windows-bootstrap',
    'linux-bootstrap',
    'verification-gate'
)
$JobsStart = [Array]::IndexOf($Lines, 'jobs:')
Assert-Workflow ($JobsStart -ge 0) 'The GitHub workflow has no jobs mapping.'
$ActualJobs = @(
    $Lines[($JobsStart + 1)..($Lines.Count - 1)] |
        Where-Object { $_ -match '^  ([a-z0-9-]+):$' } |
        ForEach-Object { $Matches[1] }
)
Assert-Workflow (
    [System.Linq.Enumerable]::SequenceEqual(
        [string[]]$ActualJobs,
        [string[]]$ExpectedJobs)
) "The GitHub workflow job order differs: $($ActualJobs -join ', ')."

$DevelopmentJobs = @('windows-development', 'linux-development')
foreach ($Job in $DevelopmentJobs) {
    $Block = Get-JobBlock $Job
    Assert-Workflow ($Block -match '(?m)^    needs: classify-changes$') `
        "Development job '$Job' does not depend on classification."
    Assert-Workflow (
        $Block.Contains("    if: `${{ needs.classify-changes.outputs.scope == 'development' }}")
    ) "Development job '$Job' does not use the focused-development condition."
    Assert-Workflow (
        $Block.Contains(
            'run: pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1 -BaseReference $env:BASE_SHA -HeadReference $env:HEAD_SHA')
    ) "Development job '$Job' does not invoke changed-file verification for the classified comparison."
    Assert-Workflow ($Block -match '(?m)^        uses: actions/setup-node@[0-9a-f]{40} # v[0-9]') `
        "Development job '$Job' does not pin the Node setup action."
    Assert-Workflow ($Block -match '(?m)^          node-version: 24$') `
        "Development job '$Job' does not pin Node.js 24."
    Assert-Workflow (
        $Block.Contains(
            'uses: actions/cache@27d5ce7f107fe9357f9df03efb73ab90386fccae # v5.0.5')
    ) "Development job '$Job' does not pin the accepted checkpoint cache action."
    Assert-Workflow (
        $Block.Contains(
            'key: windvale-native-development-v1-${{ runner.os }}-${{ github.run_id }}-${{ github.run_attempt }}') -and
        $Block.Contains(
            'windvale-native-development-v1-${{ runner.os }}-') -and
        $Block.Contains(
            'WINDVALE_NATIVE_CACHE_ROOT: ${{ runner.temp }}/windvale-native-development-cache')
    ) "Development job '$Job' does not bind the isolated versioned checkpoint cache."
}

$QualificationJobs = @(
    'windows-native-suite',
    'linux-native-suite',
    'windows-webassembly',
    'linux-webassembly',
    'windows-bootstrap',
    'linux-bootstrap'
)
foreach ($Job in $QualificationJobs) {
    $Block = Get-JobBlock $Job
    Assert-Workflow ($Block -match '(?m)^    needs: classify-changes$') `
        "Qualification job '$Job' does not depend on classification."
    Assert-Workflow (
        $Block.Contains("    if: `${{ needs.classify-changes.outputs.scope == 'qualification' }}")
    ) "Qualification job '$Job' does not use the fail-closed qualification condition."
    Assert-Workflow (
        !$Block.Contains('actions/cache@') -and
        !$Block.Contains('WINDVALE_NATIVE_CACHE_ROOT')
    ) "Qualification job '$Job' consults development checkpoint state."
}

$ExpectedCommands = @{
    'windows-native-suite' = 'Tools\Native\Test-Retirement-Suite.cmd --shard ${{ matrix.shard }}'
    'linux-native-suite' = './Tools/Native/Test-Retirement-Suite.sh --shard ${{ matrix.shard }}'
    'windows-webassembly' = 'pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly.ps1'
    'linux-webassembly' = 'pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly.ps1'
    'windows-bootstrap' = 'Tools\Verify\Verify-Bootstrap.cmd'
    'linux-bootstrap' = './Tools/Verify/Verify-Bootstrap.sh'
}

foreach ($Job in @('windows-native-suite', 'linux-native-suite')) {
    $Block = Get-JobBlock $Job
    Assert-Workflow ($Block -match '(?m)^    strategy:\n      fail-fast: false\n      max-parallel: 4\n      matrix:\n        shard: \[1, 2, 3, 4\]$') `
        "Retirement job '$Job' does not declare the exact four-shard matrix."
}
foreach ($Job in $ExpectedCommands.Keys) {
    $Block = Get-JobBlock $Job
    Assert-Workflow ($Block.Contains("run: $($ExpectedCommands[$Job])")) `
        "Qualification job '$Job' does not invoke its exact native owner."
}

foreach ($Job in @('windows-native-suite', 'linux-native-suite', 'windows-webassembly', 'linux-webassembly')) {
    $Block = Get-JobBlock $Job
    Assert-Workflow ($Block -match '(?m)^        uses: actions/setup-node@[0-9a-f]{40} # v[0-9]') `
        "Qualification job '$Job' does not pin the Node setup action."
    Assert-Workflow ($Block -match '(?m)^          node-version: 24$') `
        "Qualification job '$Job' does not pin Node.js 24."
}

$PinnedDebian = 'debian:12-slim@sha256:7b140f374b289a7c2befc338f42ebe6441b7ea838a042bbd5acbfca6ec875818'
foreach ($Job in @('linux-native-suite', 'linux-bootstrap')) {
    Assert-Workflow ((Get-JobBlock $Job).Contains("image: $PinnedDebian")) `
        "Qualification job '$Job' does not use the pinned Debian image."
}

foreach ($Line in $Lines | Where-Object { $_ -match '^\s+uses:\s+' }) {
    Assert-Workflow ($Line -match '@[0-9a-f]{40}(?:\s+#\s+v[^\s]+)?$') `
        "The GitHub workflow contains an unpinned action: $($Line.Trim())"
}

$Gate = Get-JobBlock 'verification-gate'
foreach ($Job in @($DevelopmentJobs; $QualificationJobs)) {
    Assert-Workflow ($Gate -match "(?m)^      - $([regex]::Escape($Job))$") `
        "The verification gate does not depend on '$Job'."
}
foreach ($Variable in @('WINDOWS_DEVELOPMENT_RESULT', 'LINUX_DEVELOPMENT_RESULT')) {
    $SuccessPattern = '(?m)^              test "\$' +
        [regex]::Escape($Variable) + '" = success$'
    Assert-Workflow ($Gate -match $SuccessPattern) `
        "The development branch does not require '$Variable' success."
}
foreach ($Variable in @(
    'WINDOWS_NATIVE_RESULT',
    'LINUX_NATIVE_RESULT',
    'WINDOWS_WEBASSEMBLY_RESULT',
    'LINUX_WEBASSEMBLY_RESULT',
    'WINDOWS_BOOTSTRAP_RESULT',
    'LINUX_BOOTSTRAP_RESULT'
)) {
    $SuccessPattern = '(?m)^              test "\$' +
        [regex]::Escape($Variable) + '" = success$'
    Assert-Workflow ($Gate -match $SuccessPattern) `
        "The qualification branch does not require '$Variable' success."
}

& $InventoryVerifier -Quiet
$Inventory = Get-Content -LiteralPath $InventoryPath -Raw | ConvertFrom-Json
$InventoryEntries = @($Inventory.directManagedEntrypoints)
Assert-Workflow (
    @($InventoryEntries | Where-Object { $_.mode -eq 'normal' }).Count -eq 0
) 'The retirement inventory still contains a normal managed entry point.'
Assert-Workflow ($InventoryEntries.Count -eq 0) `
    "The archival inventory contains $($InventoryEntries.Count) direct managed entry points instead of zero."
Write-Host 'GitHub native workflow verification passed (2 focused development jobs; 6 qualification definitions, 12 matrix-expanded native jobs; 0 managed entry points).'
