[CmdletBinding()]
param(
    [string]$BaseReference,
    [string]$HeadReference = 'HEAD',
    [AllowEmptyCollection()]
    [string[]]$ChangedPath,
    [switch]$PlanOnly,
    [switch]$NoFailFast,
    [string]$TimingReportPath
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$Planner = Join-Path $PSScriptRoot 'Get-Verification-Plan.ps1'
$SeedVerifier = Join-Path $PSScriptRoot 'Verify-Seed.ps1'
$EditorVerifier = Join-Path (Split-Path -Parent $PSScriptRoot) 'Editors/Verify-Windvale-Editor.ps1'

if ($PSBoundParameters.ContainsKey('ChangedPath')) {
    $Paths = @($ChangedPath)
} elseif (![string]::IsNullOrWhiteSpace($BaseReference)) {
    $Paths = @(& git -C $RepositoryRoot diff `
        --name-only `
        --no-renames `
        --diff-filter=ACDMRTUXB `
        $BaseReference `
        $HeadReference `
        --)
    if ($LASTEXITCODE -ne 0) {
        throw 'Git could not enumerate the requested committed changes.'
    }
} else {
    $TrackedPaths = @(& git -C $RepositoryRoot diff `
        --name-only `
        --no-renames `
        --diff-filter=ACDMRTUXB `
        HEAD `
        --)
    if ($LASTEXITCODE -ne 0) {
        throw 'Git could not enumerate tracked working-tree changes.'
    }
    $UntrackedPaths = @(& git -C $RepositoryRoot ls-files --others --exclude-standard)
    if ($LASTEXITCODE -ne 0) {
        throw 'Git could not enumerate untracked working-tree changes.'
    }
    $Paths = @($TrackedPaths; $UntrackedPaths)
}

$Paths = @($Paths | Where-Object { ![string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
if ($Paths.Count -eq 0) {
    throw 'No changed paths were found. Supply -BaseReference or -ChangedPath when the working tree is clean.'
}

$Plan = & $Planner -ChangedPath $Paths -PassThru
if ($PlanOnly) {
    return
}

if ($PSBoundParameters.ContainsKey('ChangedPath')) {
    git -C $RepositoryRoot diff --check
} elseif (![string]::IsNullOrWhiteSpace($BaseReference)) {
    git -C $RepositoryRoot diff --check $BaseReference $HeadReference --
} else {
    git -C $RepositoryRoot diff --check HEAD --
}
if ($LASTEXITCODE -ne 0) {
    throw 'Changed-file whitespace verification failed.'
}

if ($Plan.Editor) {
    & $EditorVerifier
}

if ($Plan.Areas.Count -ne 0) {
    Write-Warning 'Changed-file verification is development feedback, not conformance or qualification evidence.'
    $Arguments = @{
        Level = 'Fast'
        TestArea = $Plan.Areas
    }
    if (!$NoFailFast) {
        $Arguments.FailFast = $true
    }
    if (![string]::IsNullOrWhiteSpace($TimingReportPath)) {
        $Arguments.TimingReportPath = $TimingReportPath
    }
    & $SeedVerifier @Arguments
} else {
    Write-Host 'Changed-file verification passed without Seed execution.'
}
