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
$NativePlanner = Join-Path $PSScriptRoot 'Get-Native-Changed-Verification-Plan.ps1'
$PlanVerifier = Join-Path $PSScriptRoot 'Verify-Verification-Plan.ps1'
$WebAssemblyEngineVerifier = Join-Path $PSScriptRoot 'Verify-WebAssembly-Engine.ps1'
$WebAssemblyVerifier = Join-Path $PSScriptRoot 'Verify-WebAssembly.ps1'
$GitHubQualificationVerifier = Join-Path $PSScriptRoot 'Verify-GitHub-Native-Qualification.ps1'
$WebsiteVerifier = Join-Path $PSScriptRoot 'Verify-Website.ps1'
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
$NativePlan = if ($Plan.Scope -in @('development', 'qualification')) {
    & $NativePlanner -ChangedPath $Paths -PassThru
} else {
    [pscustomobject]@{
        Suites = @()
        Gaps = @()
        RunPlanVerification = $false
        RunWebAssemblyEngineVerification = $false
        RunWebAssemblyVerification = $false
        RunGitHubQualificationVerification = $false
        ChangedCount = $Paths.Count
    }
}
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

if ($Plan.Scope -eq 'website') {
    & $WebsiteVerifier
} elseif ($Plan.Scope -in @('development', 'qualification')) {
    if ($NativePlan.Gaps.Count -ne 0) {
        throw (
            'Changed-file verification has uncovered native evidence gaps: ' +
            ($NativePlan.Gaps -join ', ') +
            '. Add or select a native owner; no managed fallback was invoked.'
        )
    }

    Write-Warning 'Changed-file verification is native development feedback, not conformance or qualification evidence.'
    $Failures = [System.Collections.Generic.List[string]]::new()
    $Timings = [System.Collections.Generic.List[object]]::new()
    if ($NativePlan.RunPlanVerification) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        try {
            & $PlanVerifier
        } catch {
            $Failures.Add('verification-plan')
            if (!$NoFailFast) { throw }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = 'verification-plan'
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
            })
        }
    }

    if ($NativePlan.RunGitHubQualificationVerification) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        try {
            & $GitHubQualificationVerifier
        } catch {
            $Failures.Add('github-qualification')
            if (!$NoFailFast) { throw }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = 'github-qualification'
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
            })
        }
    }

    $IsWindowsHost = [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT
    $Coordinator = if ($IsWindowsHost) {
        Join-Path $RepositoryRoot 'Tools/Native/Test-Verification-Owners.cmd'
    } else {
        Join-Path $RepositoryRoot 'Tools/Native/Test-Verification-Owners.sh'
    }
    foreach ($Suite in $NativePlan.Suites) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        try {
            if ($Suite -eq 'database-storage' -and
                $NativePlan.UseDatabaseStorageDevelopment) {
                $DevelopmentOwner = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Database-Storage.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Database-Storage.sh'
                }
                $DatabaseTarget = $NativePlan.DatabaseStorageDevelopmentTarget
                Write-Host (
                    'Native owner database-storage mode=development-checkpoint ' +
                    "target=$DatabaseTarget")
                & $DevelopmentOwner --development-target $DatabaseTarget
            } elseif ($Suite -eq 'os-x64-code-emission' -and
                $NativePlan.UseOsX64CodeEmissionDevelopment) {
                $DevelopmentOwner = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Os-X64-Code-Emission.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Os-X64-Code-Emission.sh'
                }
                $OsX64Target = $NativePlan.OsX64CodeEmissionDevelopmentTarget
                Write-Host (
                    'Native owner os-x64-code-emission mode=development-target ' +
                    "target=$OsX64Target")
                & $DevelopmentOwner --development-target $OsX64Target
            } else {
                & $Coordinator --filter $Suite
            }
            if ($LASTEXITCODE -ne 0) {
                throw "Native owner '$Suite' exited $LASTEXITCODE."
            }
        } catch {
            $Failures.Add($Suite)
            if (!$NoFailFast) { throw }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = $Suite
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
            })
        }
    }

    if ($NativePlan.RunWebAssemblyEngineVerification) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        try {
            & $WebAssemblyEngineVerifier
        } catch {
            $Failures.Add('webassembly-engine')
            if (!$NoFailFast) { throw }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = 'webassembly-engine'
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
            })
        }
    }

    if ($NativePlan.RunWebAssemblyVerification) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        try {
            & $WebAssemblyVerifier
        } catch {
            $Failures.Add('webassembly')
            if (!$NoFailFast) { throw }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = 'webassembly'
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
            })
        }
    }

    if (![string]::IsNullOrWhiteSpace($TimingReportPath)) {
        $TimingParent = Split-Path -Parent $TimingReportPath
        if (![string]::IsNullOrWhiteSpace($TimingParent) -and
            !(Test-Path -LiteralPath $TimingParent -PathType Container)) {
            throw 'The native changed-file timing-report parent does not exist.'
        }
        [pscustomobject]@{
            format = 'windvale-native-changed-verification-timing-1'
            entries = @($Timings)
        } | ConvertTo-Json -Depth 4 |
            Set-Content -LiteralPath $TimingReportPath -Encoding utf8
    }
    if ($Failures.Count -ne 0) {
        throw "Native changed-file verification failed: $($Failures -join ', ')."
    }
} else {
    Write-Host 'Changed-file verification passed without native owner execution.'
}
