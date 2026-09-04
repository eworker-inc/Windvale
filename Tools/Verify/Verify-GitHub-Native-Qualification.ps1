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
        "  group: verify-`${{ github.workflow }}-`${{ github.ref }}-`${{ github.event_name == 'workflow_dispatch' && 'qualification' || 'automatic' }}",
        [StringComparison]::Ordinal) -and
    $Content.Contains(
        "  cancel-in-progress: `${{ github.event_name != 'workflow_dispatch' }}",
        [StringComparison]::Ordinal) -and
    $Content.Contains('  queue: single', [StringComparison]::Ordinal)
) 'The GitHub workflow does not cancel superseded automatic runs while preserving explicit qualification.'
Assert-Workflow (
    ([regex]::Matches($Content, '\$\{\{').Count -eq
        [regex]::Matches($Content, '\}\}').Count)
) 'The GitHub workflow has unbalanced expression delimiters.'

$ClassificationBlock = Get-JobBlock 'classify-changes'
foreach ($Fragment in @(
    'windows_required: ${{ steps.host-scope.outputs.windows_required }}',
    'qualification_shard: ${{ steps.qualification-selection.outputs.shard }}',
    'qualification_start_owner: ${{ steps.qualification-selection.outputs.start_owner }}',
    'qualification_shards: ${{ steps.qualification-selection.outputs.shards }}',
    'qualification_full: ${{ steps.qualification-selection.outputs.full }}',
    'name: Select automatic Windows host',
    'name: Validate qualification selection',
    "`$_ -match '(?i)(?:^|[/_.-])(?:Windows|Win32)(?:`$|[/_.-])'",
    "`$_ -match '(?i)\.(?:cmd|bat|ps1|exe|dll|pdb)$'",
    'uses: actions/setup-node@48b55a011bda9f5d6aeb4c2d9c7362e8dae4041e # v6.4.0',
    'node-version: 24',
    './Tools/Verify/Verify-Verification-Plan.ps1'
)) {
    Assert-Workflow (
        $ClassificationBlock.Contains($Fragment, [StringComparison]::Ordinal)
    ) "The change classifier is missing the automatic Windows selection fragment '$Fragment'."
}

$ExpectedJobs = @(
    'classify-changes',
    'linux-documentation',
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

$DocumentationJobs = @('linux-documentation')
foreach ($Job in $DocumentationJobs) {
    $Block = Get-JobBlock $Job
    Assert-Workflow ($Block -match '(?m)^    needs: classify-changes$') `
        "Documentation job '$Job' does not depend on classification."
    Assert-Workflow (
        $Block.Contains(
            "    if: `${{ needs.classify-changes.outputs.documentation == 'true' }}")
    ) "Documentation job '$Job' does not use the documentation condition."
    Assert-Workflow (
        $Block.Contains(
            'run: pwsh -NoProfile -File Tools/Verify/Verify-Documentation.ps1')
    ) "Documentation job '$Job' does not invoke documentation verification."
}

$DevelopmentConditions = @{
    'windows-development' = "    if: `${{ needs.classify-changes.outputs.scope == 'development' && needs.classify-changes.outputs.windows_required == 'true' }}"
    'linux-development' = "    if: `${{ needs.classify-changes.outputs.scope == 'development' }}"
}
$DevelopmentJobs = @('windows-development', 'linux-development')
foreach ($Job in $DevelopmentJobs) {
    $Block = Get-JobBlock $Job
    $ExpectedTimingInvocation = if ($Job -eq 'windows-development') {
        '-AllowIncompleteInfrastructure -PlanVerificationInClassification -GitHubVerificationOnLinux -TimingReportPath $env:VERIFICATION_TIMING_REPORT'
    } else {
        '-AllowIncompleteInfrastructure -PlanVerificationInClassification -TimingReportPath $env:VERIFICATION_TIMING_REPORT'
    }
    Assert-Workflow ($Block -match '(?m)^    needs: classify-changes$') `
        "Development job '$Job' does not depend on classification."
    Assert-Workflow (
        $Block.Contains($DevelopmentConditions[$Job], [StringComparison]::Ordinal)
    ) "Development job '$Job' does not use the focused-development condition."
    Assert-Workflow (
        $Block.Contains('    timeout-minutes: 15', [StringComparison]::Ordinal)
    ) "Development job '$Job' does not enforce the 15-minute automatic bound."
    Assert-Workflow (
        $Block.Contains(
            'run: pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1 -BaseReference $env:BASE_SHA -HeadReference $env:HEAD_SHA')
    ) "Development job '$Job' does not invoke changed-file verification for the classified comparison."
    Assert-Workflow (
        $Block.Contains($ExpectedTimingInvocation) -and
        $Block.Contains(
            'Tools/Verify/Update-Verification-Timing-History.ps1 -InputPath $env:VERIFICATION_TIMING_REPORT -HistoryPath $env:VERIFICATION_TIMING_HISTORY -AnalysisPath $env:VERIFICATION_TIMING_ANALYSIS') -and
        $Block.Contains(
            '${{ runner.temp }}/windvale-development-timing-analysis.json') -and
        $Block.Contains(
            'uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2') -and
        $Block.Contains('if-no-files-found: warn') -and
        $Block.Contains('retention-days: 14')
    ) "Development job '$Job' does not retain nonblocking structured timing evidence."
    Assert-Workflow (
        [regex]::Matches($Block, '(?m)^        continue-on-error: true$').Count -eq 5
    ) "Development job '$Job' does not isolate all five optional infrastructure steps."
    Assert-Workflow (
        $Block.Contains('-PlanVerificationInClassification')
    ) "Development job '$Job' does not consume classification-owned plan verification."
    if ($Job -eq 'windows-development') {
        Assert-Workflow (
            $Block.Contains('-GitHubVerificationOnLinux')
        ) 'Windows development does not delegate GitHub verification to Linux.'
        Assert-Workflow (
            !$Block.Contains('          TEMP: ${{ runner.temp }}') -and
            !$Block.Contains('          TMP: ${{ runner.temp }}')
        ) 'Windows development overrides the runner-owned process temporary root.'
    } else {
        Assert-Workflow (
            !$Block.Contains('-GitHubVerificationOnLinux')
        ) 'Linux development incorrectly delegates its GitHub verification.'
    }
    Assert-Workflow ($Block -match '(?m)^        uses: actions/setup-node@[0-9a-f]{40} # v[0-9]') `
        "Development job '$Job' does not pin the Node setup action."
    Assert-Workflow ($Block -match '(?m)^          node-version: 24$') `
        "Development job '$Job' does not pin Node.js 24."
    Assert-Workflow (
        $Block.Contains(
            'uses: actions/cache/restore@27d5ce7f107fe9357f9df03efb73ab90386fccae # v5.0.5') -and
        $Block.Contains(
            'uses: actions/cache/save@27d5ce7f107fe9357f9df03efb73ab90386fccae # v5.0.5') -and
        $Block.Contains(
            "if: `${{ always() && steps.native-development-cache.outputs.cache-hit != 'true' }}")
    ) "Development job '$Job' does not pin the accepted restore/save checkpoint actions."
    Assert-Workflow (
        $Block.Contains('id: native-development-cache') -and
        $Block.Contains(
            'key: windvale-native-development-v1-${{ runner.os }}-${{ github.run_id }}-${{ github.run_attempt }}') -and
        $Block.Contains(
            'windvale-native-development-v1-${{ runner.os }}-') -and
        $Block.Contains(
            'key: ${{ steps.native-development-cache.outputs.cache-primary-key }}') -and
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
    $ExpectedCondition = if ($Job -in @(
            'windows-native-suite', 'linux-native-suite')) {
        "    if: `${{ needs.classify-changes.outputs.scope == 'qualification' }}"
    } else {
        "    if: `${{ needs.classify-changes.outputs.scope == 'qualification' && needs.classify-changes.outputs.qualification_full == 'true' }}"
    }
    Assert-Workflow (
        $Block.Contains($ExpectedCondition, [StringComparison]::Ordinal)
    ) "Qualification job '$Job' does not use its fail-closed qualification condition."
    Assert-Workflow (
        !$Block.Contains('actions/cache') -and
        !$Block.Contains('WINDVALE_NATIVE_CACHE_ROOT')
    ) "Qualification job '$Job' consults development checkpoint state."
    if ($Job.StartsWith('windows-', [StringComparison]::Ordinal)) {
        Assert-Workflow (
            !$Block.Contains('          TEMP: ${{ runner.temp }}') -and
            !$Block.Contains('          TMP: ${{ runner.temp }}')
        ) "Qualification job '$Job' overrides the runner-owned process temporary root."
    }
}

$ExpectedCommands = @{
    'windows-webassembly' = 'pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly-Engine.ps1'
    'linux-webassembly' = 'pwsh -NoProfile -File Tools/Verify/Verify-WebAssembly-Engine.ps1'
    'windows-bootstrap' = 'Tools\Verify\Verify-Bootstrap.cmd'
    'linux-bootstrap' = './Tools/Verify/Verify-Bootstrap.sh'
}

foreach ($Job in @('windows-native-suite', 'linux-native-suite')) {
    $Block = Get-JobBlock $Job
    Assert-Workflow ($Block -match '(?m)^    strategy:\n      fail-fast: false\n      max-parallel: 4\n      matrix:\n        shard: \$\{\{ fromJSON\(needs\.classify-changes\.outputs\.qualification_shards\) \}\}$') `
        "Qualification job '$Job' does not consume the validated shard selection."
    foreach ($Fragment in @(
        'QUALIFICATION_START_OWNER: ${{ needs.classify-changes.outputs.qualification_start_owner }}',
        "'-File', 'Tools/Verify/Invoke-WindvaleTests.ps1'",
        "'-Shard', '`${{ matrix.shard }}'",
        "`$Arguments += @('-StartAtOwner', `$env:QUALIFICATION_START_OWNER)",
        '& pwsh @Arguments',
        'exit $LASTEXITCODE'
    )) {
        Assert-Workflow (
            $Block.Contains($Fragment, [StringComparison]::Ordinal)
        ) "Qualification job '$Job' is missing resumable runner fragment '$Fragment'."
    }
    Assert-Workflow (
        $Block.Contains(
            'uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2') -and
        $Block.Contains('if-no-files-found: warn') -and
        $Block.Contains('retention-days: 30')
    ) "Qualification job '$Job' does not retain its structured owner result."
}
foreach ($Job in $ExpectedCommands.Keys) {
    $Block = Get-JobBlock $Job
    Assert-Workflow ($Block.Contains("run: $($ExpectedCommands[$Job])")) `
        "Qualification job '$Job' does not invoke its exact native owner."
}

foreach ($Job in $QualificationJobs) {
    $Block = Get-JobBlock $Job
    Assert-Workflow ($Block -match '(?m)^        uses: actions/setup-node@[0-9a-f]{40} # v[0-9]') `
        "Qualification job '$Job' does not pin the Node setup action."
    Assert-Workflow ($Block -match '(?m)^          node-version: 24$') `
        "Qualification job '$Job' does not pin Node.js 24."
}

$PinnedDebian = 'debian:12-slim@sha256:7b140f374b289a7c2befc338f42ebe6441b7ea838a042bbd5acbfca6ec875818'
foreach ($Job in @('linux-native-suite', 'linux-bootstrap')) {
    $Block = Get-JobBlock $Job
    Assert-Workflow ($Block.Contains("image: $PinnedDebian")) `
        "Qualification job '$Job' does not use the pinned Debian image."
    Assert-Workflow (
        $Block.Contains(
            'git config --global --add safe.directory "$GITHUB_WORKSPACE"')
    ) "Qualification job '$Job' does not trust its container checkout."
}
$LinuxNativeSuite = Get-JobBlock 'linux-native-suite'
foreach ($Fragment in @(
    'libgssapi-krb5-2 libicu72 libssl3 libstdc++6 libunwind8 libuuid1 tar xz-utils zlib1g',
    'powershell_version=7.6.5',
    'b34ab3b19acac1d3d4d0d3cfdb02acf62f457b0b6a962ff008132033f7566844',
    'pwsh -NoLogo -NoProfile -Command ''$PSVersionTable.PSVersion.ToString()'''
)) {
    Assert-Workflow (
        $LinuxNativeSuite.Contains($Fragment, [StringComparison]::Ordinal)
    ) "The Debian qualification job is missing pinned PowerShell setup fragment '$Fragment'."
}

foreach ($Line in $Lines | Where-Object { $_ -match '^\s+uses:\s+' }) {
    Assert-Workflow ($Line -match '@[0-9a-f]{40}(?:\s+#\s+v[^\s]+)?$') `
        "The GitHub workflow contains an unpinned action: $($Line.Trim())"
}

$Gate = Get-JobBlock 'verification-gate'
foreach ($Job in @($DocumentationJobs; $DevelopmentJobs; $QualificationJobs)) {
    Assert-Workflow ($Gate -match "(?m)^      - $([regex]::Escape($Job))$") `
        "The verification gate does not depend on '$Job'."
}
foreach ($Variable in @('LINUX_DOCUMENTATION_RESULT')) {
    $SuccessPattern = '(?m)^            test "\$' +
        [regex]::Escape($Variable) + '" = success$'
    $SkippedPattern = '(?m)^            test "\$' +
        [regex]::Escape($Variable) + '" = skipped$'
    Assert-Workflow (
        $Gate -match $SuccessPattern -and $Gate -match $SkippedPattern
    ) "The gate does not enforce both selected and skipped states for '$Variable'."
}
Assert-Workflow (
    $Gate.Contains('          WINDOWS_REQUIRED: ${{ needs.classify-changes.outputs.windows_required }}') -and
    $Gate.Contains('              if [ "$WINDOWS_REQUIRED" = true ]; then') -and
    $Gate.Contains('                test "$WINDOWS_DEVELOPMENT_RESULT" = success') -and
    $Gate.Contains('                test "$WINDOWS_DEVELOPMENT_RESULT" = skipped') -and
    $Gate.Contains('              test "$LINUX_DEVELOPMENT_RESULT" = success')
) 'The development gate does not enforce Linux plus conditionally selected Windows results.'
foreach ($Variable in @(
    'WINDOWS_NATIVE_RESULT',
    'LINUX_NATIVE_RESULT'
)) {
    $SuccessPattern = '(?m)^              test "\$' +
        [regex]::Escape($Variable) + '" = success$'
    Assert-Workflow ($Gate -match $SuccessPattern) `
        "The qualification branch does not require '$Variable' success."
}
Assert-Workflow (
    $Gate.Contains(
        "    name: `${{ needs.classify-changes.outputs.scope == 'qualification' && needs.classify-changes.outputs.qualification_full != 'true' && 'Partial qualification gate' || 'Verification gate' }}") -and
    $Gate.Contains(
        '          QUALIFICATION_FULL: ${{ needs.classify-changes.outputs.qualification_full }}') -and
    $Gate.Contains('              if [ "$QUALIFICATION_FULL" = true ]; then')
) 'The verification gate does not distinguish complete and partial qualification.'
foreach ($Variable in @(
    'WINDOWS_WEBASSEMBLY_RESULT',
    'LINUX_WEBASSEMBLY_RESULT',
    'WINDOWS_BOOTSTRAP_RESULT',
    'LINUX_BOOTSTRAP_RESULT'
)) {
    $SuccessPattern = '(?m)^                test "\$' +
        [regex]::Escape($Variable) + '" = success$'
    $SkippedPattern = '(?m)^                test "\$' +
        [regex]::Escape($Variable) + '" = skipped$'
    Assert-Workflow (
        $Gate -match $SuccessPattern -and $Gate -match $SkippedPattern
    ) "The qualification branch does not distinguish complete and partial '$Variable' results."
}

& $InventoryVerifier -Quiet
$Inventory = Get-Content -LiteralPath $InventoryPath -Raw | ConvertFrom-Json
$InventoryEntries = @($Inventory.directManagedEntrypoints)
Assert-Workflow (
    @($InventoryEntries | Where-Object { $_.mode -eq 'normal' }).Count -eq 0
) 'The retirement inventory still contains a normal managed entry point.'
Assert-Workflow ($InventoryEntries.Count -eq 0) `
    "The archival inventory contains $($InventoryEntries.Count) direct managed entry points instead of zero."
Write-Host 'GitHub native workflow verification passed (1 documentation job; Linux-focused development plus conditional Windows; complete or resumable dual-host qualification; 0 managed entry points).'
