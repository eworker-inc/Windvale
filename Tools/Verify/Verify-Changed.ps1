[CmdletBinding()]
param(
    [string]$BaseReference,
    [string]$HeadReference = 'HEAD',
    [AllowEmptyCollection()]
    [string[]]$ChangedPath,
    [switch]$PlanOnly,
    [switch]$AllowLongRun,
    [switch]$NoFailFast,
    [string]$TimingReportPath,
    [switch]$NoResultCache,
    [string]$ResultCacheRoot,
    [switch]$SkipDocumentationVerification,
    [switch]$AllowIncompleteInfrastructure,
    [switch]$PlanVerificationInClassification,
    [switch]$GitHubVerificationOnLinux
)

$ErrorActionPreference = 'Stop'
$LOCAL_DEVELOPMENT_BUDGET_SECONDS = 600
$VerificationStartedUtc = [DateTime]::UtcNow
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$Planner = Join-Path $PSScriptRoot 'Get-Verification-Plan.ps1'
$NativePlanner = Join-Path $PSScriptRoot 'Get-Native-Changed-Verification-Plan.ps1'
$PlanVerifier = Join-Path $PSScriptRoot 'Verify-Verification-Plan.ps1'
$WebAssemblyEngineVerifier = Join-Path $PSScriptRoot 'Verify-WebAssembly-Engine.ps1'
$WebAssemblyVerifier = Join-Path $PSScriptRoot 'Verify-WebAssembly.ps1'
$GitHubQualificationVerifier = Join-Path $PSScriptRoot 'Verify-GitHub-Native-Qualification.ps1'
$WebsiteVerifier = Join-Path $PSScriptRoot 'Verify-Website.ps1'
$DocumentationVerifier = Join-Path $PSScriptRoot 'Verify-Documentation.ps1'
$ChangeClassificationVerifier = Join-Path $PSScriptRoot 'Verify-Change-Classification.ps1'
$EditorVerifier = Join-Path (Split-Path -Parent $PSScriptRoot) 'Editors/Verify-Windvale-Editor.ps1'
$ResultCacheTool = Join-Path (
    Split-Path -Parent $PSScriptRoot) 'Native/Verification-Owner-Result-Cache.mjs'
$VerificationOwnerRegistry = Join-Path $RepositoryRoot `
    'Tests/Native/Verification-Owners.txt'
$CompatibleResultCacheBarrierPaths = @(
    'Tests/Native/Verification-Owners.txt',
    'Tests/Native/Verification-Duration-Profiles.txt',
    'Tools/Native/Stream-Verification-Owner.mjs',
    'Tools/Native/Verification-Owner-Result-Cache.mjs',
    'Tools/Native/Verification-Owner-Stream-Path.mjs',
    'Tools/Verify/Get-Native-Changed-Verification-Plan.ps1',
    'Tools/Verify/Get-Verification-Plan.ps1',
    'Tools/Verify/Invoke-WindvaleTests.ps1',
    'Tools/Verify/Verify-Changed.ps1'
)

function Get-VerificationHostName {
    if ($env:RUNNER_OS -in @('Windows', 'Linux', 'macOS')) {
        return $env:RUNNER_OS
    }
    if ([Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
            [Runtime.InteropServices.OSPlatform]::Windows)) {
        return 'Windows'
    }
    if ([Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
            [Runtime.InteropServices.OSPlatform]::Linux)) {
        return 'Linux'
    }
    if ([Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
            [Runtime.InteropServices.OSPlatform]::OSX)) {
        return 'macOS'
    }
    return [Environment]::OSVersion.Platform.ToString()
}

function Invoke-VerificationResultCache {
    param(
        [Parameter(Mandatory)]
        [string[]]$CacheArgument
    )

    $Output = @(& node $ResultCacheTool @CacheArgument 2>&1)
    $ExitCode = $LASTEXITCODE
    $Text = ($Output | ForEach-Object { $_.ToString() }) -join "`n"
    if ($ExitCode -ne 0) {
        throw "Verification result cache command failed: $Text"
    }
    return $Text.Trim()
}

function Get-Sha256Text {
    param(
        [Parameter(Mandatory)]
        [string]$Value
    )

    $Bytes = [Text.UTF8Encoding]::new($false).GetBytes($Value)
    return [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData($Bytes)
    ).ToLowerInvariant()
}

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
        UseSourceContainmentCompilerDevelopment = $false
        ChangedCount = $Paths.Count
    }
}
if ($PlanVerificationInClassification -and
    ($Plan.Scope -ne 'development' -or
        $env:GITHUB_ACTIONS -ne 'true' -or
        $env:RUNNER_OS -notin @('Windows', 'Linux'))) {
    throw (
        '-PlanVerificationInClassification is reserved for automatic ' +
        'development jobs whose required classification predecessor passed.')
}
if ($GitHubVerificationOnLinux -and
    ($Plan.Scope -ne 'development' -or
        $env:GITHUB_ACTIONS -ne 'true' -or
        $env:RUNNER_OS -ne 'Windows')) {
    throw (
        '-GitHubVerificationOnLinux is reserved for automatic Windows ' +
        'development jobs whose Linux peer runs the GitHub verifier.')
}
if ($PlanOnly) {
    return
}
if ($AllowIncompleteInfrastructure -and $Plan.Scope -ne 'development') {
    throw '-AllowIncompleteInfrastructure is valid only for development scope.'
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

$RunDocumentationVerification = @(
    $Paths | Where-Object {
        $_.EndsWith('.md', [StringComparison]::OrdinalIgnoreCase) -or
        $_.StartsWith('Documents/Evidence/', [StringComparison]::Ordinal) -or
        $_.StartsWith('Tools/Documentation/', [StringComparison]::Ordinal) -or
        $_ -in @(
            'Documents/Decisions/Decision-Catalog.json',
            'Documents/Decisions/Legacy-Id-Collisions.txt',
            'Documents/Decisions/Legacy-Missing-Status.txt',
            'Specifications/Legacy-Missing-Status.txt',
            'Specifications/Legacy-Status-Classifications.json',
            'Specifications/Specification-Catalog.json',
            'Tools/Verify/Verify-Documentation.ps1'
        )
    }
).Count -ne 0
if ($RunDocumentationVerification -and !$SkipDocumentationVerification) {
    & $DocumentationVerifier
}

$RunChangeClassificationVerification = @(
    $Paths | Where-Object {
        $_ -in @(
            'Tools/Verify/Classify-Verification-Changes.ps1',
            'Tools/Verify/Verify-Changed.ps1',
            'Tools/Verify/Verify-Change-Classification.ps1'
        )
    }
).Count -ne 0
if ($RunChangeClassificationVerification) {
    & $ChangeClassificationVerifier
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
    $Incomplete = [System.Collections.Generic.List[string]]::new()
    $Timings = [System.Collections.Generic.List[object]]::new()
    if ($NativePlan.RunPlanVerification -and
        !$PlanVerificationInClassification) {
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

    if ($NativePlan.RunGitHubQualificationVerification -and
        !$GitHubVerificationOnLinux) {
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
    $Coordinator = Join-Path $PSScriptRoot 'Invoke-WindvaleTests.ps1'
    $OwnerContractHashes = @{}
    if (@($NativePlan.Suites).Count -ne 0) {
        $OwnerLines = [IO.File]::ReadAllLines($VerificationOwnerRegistry)
        if ($OwnerLines.Count -lt 2 -or
            $OwnerLines[0] -cne 'windvale-native-verification-owners 2') {
            throw 'The verification-owner registry header differs.'
        }
        foreach ($OwnerLine in @($OwnerLines | Select-Object -Skip 1)) {
            $OwnerFields = $OwnerLine -split '\|', 6
            if ($OwnerFields.Count -ne 6 -or
                [string]::IsNullOrWhiteSpace($OwnerFields[0]) -or
                $OwnerContractHashes.ContainsKey($OwnerFields[0])) {
                throw "The verification-owner registry row is malformed: $OwnerLine"
            }
            $OwnerContractHashes[$OwnerFields[0]] = Get-Sha256Text $OwnerLine
        }
        foreach ($SelectedOwner in @($NativePlan.Suites)) {
            if (!$OwnerContractHashes.ContainsKey($SelectedOwner)) {
                throw "The selected native owner is not registered: $SelectedOwner"
            }
        }
    }
    $ResultCacheState = $null
    $CompatibleResultPlanCache = @{}
    if ($Plan.Scope -eq 'development' -and !$NoResultCache -and
        @($NativePlan.Suites).Count -ne 0) {
        try {
            $PrepareArguments = @('prepare', $RepositoryRoot)
            if ($PSBoundParameters.ContainsKey('ResultCacheRoot')) {
                $PrepareArguments += $ResultCacheRoot
            }
            $ResultCacheState = (
                Invoke-VerificationResultCache -CacheArgument $PrepareArguments
            ) | ConvertFrom-Json
            if ($ResultCacheState.format -ne 'windvale-verification-owner-state-1' -or
                $ResultCacheState.stateKey -notmatch '^[0-9a-f]{64}$' -or
                $ResultCacheState.sourceTree -notmatch '^[0-9a-f]{40}(?:[0-9a-f]{24})?$' -or
                $ResultCacheState.sourceSentinel -notmatch '^[0-9a-f]{64}$' -or
                $ResultCacheState.repositoryKey -notmatch '^[0-9a-f]{64}$' -or
                $ResultCacheState.hostKey -notmatch '^[0-9a-f]{64}$') {
                throw 'Verification result cache returned an invalid state record.'
            }
            Write-Host (
                'Verification result cache status=Ready ' +
                "state=$($ResultCacheState.stateKey.Substring(0, 12))"
            )
        } catch {
            Write-Warning (
                'Persistent verification resume is unavailable; owners will run: ' +
                $_.Exception.Message
            )
            $ResultCacheState = $null
        }
    }
    foreach ($Suite in $NativePlan.Suites) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        $TimingStatus = 'executed'
        $TimingOutcome = 'passed'
        $OwnerExitCode = 0
        $StopAfterOwner = $false
        try {
            $OwnerCommand = $Coordinator
            $OwnerArguments = @('-Owner', $Suite)
            if ($AllowLongRun) {
                $OwnerArguments += '-AllowLongRun'
            }
            $OwnerMessage = $null
            if ($Suite -eq 'compiler-reconstruction' -and
                $Plan.Scope -eq 'development') {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Compiler-Reconstruction.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Compiler-Reconstruction.sh'
                }
                $OwnerArguments = @('--development')
                $OwnerMessage = (
                    'Native owner compiler-reconstruction ' +
                    'mode=development-smoke')
            } elseif ($Suite -eq 'wvb-runner-reconstruction' -and
                $Plan.Scope -eq 'development') {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Wvb-Runner-Reconstruction.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Wvb-Runner-Reconstruction.sh'
                }
                $OwnerArguments = @('--development')
                $OwnerMessage = (
                    'Native owner wvb-runner-reconstruction ' +
                    'mode=development-candidate-smoke')
            } elseif ($Suite -eq 'hosted-verifier-publisher-files' -and
                $Plan.Scope -eq 'development' -and $NativePlan.UsePublisherCurrentSourceDevelopment) {
                $OwnerExtension = if ($IsWindowsHost) { 'cmd' } else { 'sh' }
                $OwnerCommand = Join-Path $RepositoryRoot (
                    "Tools/Native/Test-Hosted-Verifier-Publisher-File-Pipeline.$OwnerExtension")
                $OwnerArguments = @('--current-source')
                $OwnerMessage = 'Native owner hosted-verifier-publisher-files mode=current-source cases=1 expected-seconds=60'
            } elseif ($Suite -eq 'language-1-front-door' -and
                $Plan.Scope -eq 'development') {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Front-Door.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Front-Door.sh'
                }
                $OwnerArguments = @('--development')
                $OwnerMessage = (
                    'Native owner language-1-front-door ' +
                    "mode=development-front-end cases=$($NativePlan.Language1FrontDoorDevelopmentCaseCount) " +
                    "target=$($NativePlan.Language1FrontDoorDevelopmentTarget) " +
                    "expected-seconds=$($NativePlan.Language1FrontDoorDevelopmentExpectedSeconds)")
                if ($NativePlan.Language1FrontDoorDevelopmentTarget -ne 'all') {
                    $OwnerArguments = @('--development-target', $NativePlan.Language1FrontDoorDevelopmentTarget)
                }
            } elseif ($Suite -eq 'language-1-memory-budget-split-execution' -and
                $Plan.Scope -eq 'development' -and
                ($NativePlan.UseFoundationBorrowPlanDevelopment -or
                    $NativePlan.UseFoundationBorrowDirectoryDevelopment -or
                    $NativePlan.UseFoundationBorrowOwnerDevelopment)) {
                $OwnerExtension = if ($IsWindowsHost) { 'cmd' } else { 'sh' }
                $OwnerCommand = Join-Path $RepositoryRoot (
                    "Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.$OwnerExtension")
                $OwnerArguments = @('--foundation-borrow-plan')
                $OwnerMessage = 'Native owner language-1-memory-budget-split-execution mode=foundation-borrow-plan cases=16 expected-seconds=30'
                if ($NativePlan.UseFoundationBorrowDirectoryDevelopment) {
                    $OwnerArguments = @('--foundation-borrow-directories')
                    $OwnerMessage = 'Native owner language-1-memory-budget-split-execution mode=foundation-borrow-directories cases=24 expected-seconds=30'
                }
                if ($NativePlan.UseFoundationBorrowOwnerDevelopment) {
                    $OwnerArguments = @('--foundation-borrow-owners')
                    $OwnerMessage = 'Native owner language-1-memory-budget-split-execution mode=foundation-borrow-owners cases=116 expected-seconds=180'
                }
            } elseif ($Suite -in @(
                    'generic-nominal-type-binding',
                    'generic-nominal-type-layout',
                    'generic-nominal-type-materialization') -and
                $Plan.Scope -eq 'development' -and
                $NativePlan.UseGenericNominalDevelopmentBundle) {
                $OwnerStem = switch ($Suite) {
                    'generic-nominal-type-binding' {
                        'Test-Generic-Nominal-Type-Binding'
                    }
                    'generic-nominal-type-layout' {
                        'Test-Generic-Nominal-Type-Layout'
                    }
                    'generic-nominal-type-materialization' {
                        'Test-Generic-Nominal-Type-Materialization'
                    }
                }
                $OwnerExtension = if ($IsWindowsHost) { 'cmd' } else { 'sh' }
                $OwnerCommand = Join-Path $RepositoryRoot (
                    "Tools/Native/$OwnerStem.$OwnerExtension")
                $OwnerArguments = @('--development')
                $OwnerMessage = (
                    "Native owner $Suite mode=development-bundle " +
                    'bundle-cases=108 ' +
                    "selected-owners=$($NativePlan.GenericNominalDevelopmentBundleSelectedOwnerCount) " +
                    'expected-seconds=330')
            } elseif ($Suite -eq 'source-containment' -and
                $Plan.Scope -eq 'development' -and
                $NativePlan.UseSourceContainmentCompilerDevelopment) {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Source-Containment.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Source-Containment.sh'
                }
                $OwnerArguments = @('--compiler-only')
                $OwnerMessage = 'Native owner source-containment mode=compiler-only'
            } elseif ($Suite -eq 'database-storage' -and
                $NativePlan.UseDatabaseStorageDevelopment) {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Database-Storage.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Database-Storage.sh'
                }
                $DatabaseTarget = $NativePlan.DatabaseStorageDevelopmentTarget
                $DatabaseCases = $NativePlan.DatabaseStorageDevelopmentCaseCount
                $DatabaseExecutions =
                    $NativePlan.DatabaseStorageDevelopmentExecutionCount
                $DatabaseBundles =
                    $NativePlan.DatabaseStorageDevelopmentBundleCount
                $DatabasePortableCases =
                    $NativePlan.DatabaseStorageDevelopmentPortableCaseCount
                $DatabaseHostedCases =
                    $NativePlan.DatabaseStorageDevelopmentHostedCaseCount
                $DatabaseExpectedSeconds =
                    $NativePlan.DatabaseStorageDevelopmentExpectedSeconds
                $OwnerArguments = @('--development-target-set', $DatabaseTarget)
                $OwnerMessage = (
                    'Native owner database-storage mode=development-checkpoint ' +
                    "target=$DatabaseTarget cases=$DatabaseCases " +
                    "executions=$DatabaseExecutions " +
                    "bundles=$DatabaseBundles " +
                    "portable-cases=$DatabasePortableCases " +
                    "hosted-cases=$DatabaseHostedCases " +
                    "expected-seconds=$DatabaseExpectedSeconds")
            } elseif ($Suite -eq 'libraries' -and
                $NativePlan.UseLibraryDevelopment) {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Libraries.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Libraries.sh'
                }
                $LibraryTarget = $NativePlan.LibraryDevelopmentTarget
                $OwnerArguments = @('--development-target', $LibraryTarget)
                $OwnerMessage = (
                    'Native owner libraries mode=development-target ' +
                    "target=$LibraryTarget")
            } elseif ($Suite -eq 'os-x64-code-emission' -and
                $NativePlan.UseOsX64CodeEmissionDevelopment) {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Os-X64-Code-Emission.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Os-X64-Code-Emission.sh'
                }
                $OsX64Target = $NativePlan.OsX64CodeEmissionDevelopmentTarget
                if ($OsX64Target -eq 'all') {
                    $OwnerArguments = @('--development-all')
                    $OwnerMessage = (
                        'Native owner os-x64-code-emission ' +
                        'mode=development-checkpoint target=all')
                } else {
                    $OwnerArguments = @('--development-target', $OsX64Target)
                    $OwnerMessage = (
                        'Native owner os-x64-code-emission ' +
                        "mode=development-checkpoint target=$OsX64Target")
                }
            }

            $RelativeOwnerCommand = [IO.Path]::GetRelativePath(
                $RepositoryRoot,
                $OwnerCommand
            ).Replace('\', '/')
            $OwnerAction = [ordered]@{
                format = 'windvale-verification-owner-action-2'
                suite = $Suite
                command = $RelativeOwnerCommand
                arguments = @($OwnerArguments)
                scope = $Plan.Scope
                ownerContractSha256 = $OwnerContractHashes[$Suite]
            } | ConvertTo-Json -Compress

            $ResultCacheReused = $false
            if ($null -ne $ResultCacheState) {
                try {
                    $Probe = Invoke-VerificationResultCache -CacheArgument @(
                        'probe',
                        $ResultCacheState.root,
                        $ResultCacheState.stateKey,
                        $Suite,
                        $OwnerAction
                    )
                    if ($Probe -eq 'Hit') {
                        $Confirmation = Invoke-VerificationResultCache `
                            -CacheArgument @(
                                'confirm',
                                $RepositoryRoot,
                                $ResultCacheState.sourceSentinel
                            )
                        if ($Confirmation -eq 'Unchanged') {
                            $TimingStatus = 'reused'
                            Write-Host (
                                "PASS  native owner $Suite result=Reused " +
                                'source-state=Exact'
                            )
                            $ResultCacheReused = $true
                        } elseif ($Confirmation -eq 'Changed') {
                            Write-Warning (
                                'Repository inputs changed before exact result ' +
                                'reuse; the owner will run.')
                            $ResultCacheState = $null
                        } else {
                            throw "Unexpected source-state confirmation '$Confirmation'."
                        }
                    } elseif ($Probe -ne 'Miss') {
                        throw "Unexpected cache probe result '$Probe'."
                    } else {
                        $CandidateRecord = (
                            Invoke-VerificationResultCache -CacheArgument @(
                                'candidates',
                                $ResultCacheState.root,
                                $ResultCacheState.stateKey,
                                $ResultCacheState.repositoryKey,
                                $ResultCacheState.hostKey,
                                $Suite,
                                $OwnerAction
                            )
                        ) | ConvertFrom-Json
                        $Candidates = @($CandidateRecord.candidates)
                        if ($CandidateRecord.format -ne
                                'windvale-verification-owner-candidates-1' -or
                            $Candidates.Count -gt 15) {
                            throw 'Verification result cache returned invalid candidates.'
                        }
                        foreach ($Candidate in $Candidates) {
                            if ($Candidate.stateKey -notmatch '^[0-9a-f]{64}$' -or
                                $Candidate.sourceTree -notmatch
                                    '^[0-9a-f]{40}(?:[0-9a-f]{24})?$') {
                                throw 'Verification result cache returned an invalid candidate.'
                            }
                            $Compatibility = $CompatibleResultPlanCache[$Candidate.sourceTree]
                            if ($null -eq $Compatibility) {
                                $ChangeRecord = (
                                    Invoke-VerificationResultCache -CacheArgument @(
                                        'changes',
                                        $RepositoryRoot,
                                        $Candidate.sourceTree,
                                        $ResultCacheState.sourceTree
                                    )
                                ) | ConvertFrom-Json
                                $CandidatePaths = @($ChangeRecord.paths)
                                if ($ChangeRecord.format -ne
                                        'windvale-verification-owner-changed-paths-1' -or
                                    $CandidatePaths.Count -gt 65536 -or
                                    @($CandidatePaths | Where-Object {
                                        $_ -isnot [string] -or
                                        [string]::IsNullOrWhiteSpace($_)
                                    }).Count -ne 0) {
                                    throw 'Verification compatibility paths are invalid.'
                                }
                                $Barrier = @($CandidatePaths | Where-Object {
                                    $CompatibleResultCacheBarrierPaths -ccontains $_
                                }).Count -ne 0
                                $DeltaPlan = if ($Barrier -or
                                    $CandidatePaths.Count -eq 0) {
                                    $null
                                } else {
                                    & $NativePlanner `
                                        -ChangedPath $CandidatePaths `
                                        -PassThru `
                                        -Quiet
                                }
                                $Compatibility = [pscustomobject]@{
                                    PathCount = $CandidatePaths.Count
                                    Barrier = $Barrier
                                    Plan = $DeltaPlan
                                }
                                $CompatibleResultPlanCache[$Candidate.sourceTree] =
                                    $Compatibility
                            }
                            if ($Compatibility.Barrier -or
                                $null -eq $Compatibility.Plan -or
                                @($Compatibility.Plan.Gaps).Count -ne 0 -or
                                @($Compatibility.Plan.Suites) -contains $Suite) {
                                continue
                            }
                            $CandidateProbe = Invoke-VerificationResultCache `
                                -CacheArgument @(
                                    'probe',
                                    $ResultCacheState.root,
                                    $Candidate.stateKey,
                                    $Suite,
                                    $OwnerAction
                                )
                            if ($CandidateProbe -ne 'Hit') {
                                continue
                            }
                            $Promotion = Invoke-VerificationResultCache `
                                -CacheArgument @(
                                    'publish',
                                    $RepositoryRoot,
                                    $ResultCacheState.root,
                                    $ResultCacheState.stateKey,
                                    $ResultCacheState.sourceTree,
                                    $ResultCacheState.sourceSentinel,
                                    $Suite,
                                    $OwnerAction
                                )
                            if ($Promotion -eq 'StateChanged') {
                                Write-Warning (
                                    'Repository inputs changed during compatible ' +
                                    'result reuse; the owner will run.')
                                $ResultCacheState = $null
                                break
                            }
                            if ($Promotion -ne 'Stored') {
                                throw "Unexpected compatible result publication '$Promotion'."
                            }
                            $TimingStatus = 'reused'
                            $ResultCacheReused = $true
                            Write-Host (
                                "PASS  native owner $Suite result=Reused " +
                                'source-state=Compatible ' +
                                "changed-paths=$($Compatibility.PathCount) " +
                                'from-state=' +
                                $Candidate.stateKey.Substring(0, 12)
                            )
                            break
                        }
                    }
                } catch {
                    Write-Warning (
                        "Verification result cache probe failed for '$Suite'; " +
                        'the owner will run: ' + $_.Exception.Message
                    )
                    $ResultCacheState = $null
                }
            }
            if ($ResultCacheReused) {
                continue
            }

            if ($null -ne $OwnerMessage) {
                Write-Host $OwnerMessage
            }
            if ($Suite -eq 'database-storage' -and
                $NativePlan.UseDatabaseStorageDevelopment -and
                $NativePlan.DatabaseStorageDevelopmentExpectedSeconds -gt
                    $LOCAL_DEVELOPMENT_BUDGET_SECONDS -and
                !$AllowLongRun) {
                $BudgetMessage = (
                    'The focused database development plan selects ' +
                    "$($NativePlan.DatabaseStorageDevelopmentCaseCount) cases " +
                    "in $($NativePlan.DatabaseStorageDevelopmentExecutionCount) executions " +
                    'and expects ' +
                    "$($NativePlan.DatabaseStorageDevelopmentExpectedSeconds) " +
                    'seconds, which exceeds the ' +
                    "$LOCAL_DEVELOPMENT_BUDGET_SECONDS-second local budget. " +
                    'Inspect -PlanOnly, narrow the changed-path set, or pass ' +
                    '-AllowLongRun only for an approved named longer run.')
                Write-Warning $BudgetMessage
                throw $BudgetMessage
            }
            if ($OwnerCommand -ceq $Coordinator) {
                & pwsh -NoProfile -File $OwnerCommand @OwnerArguments
            } else {
                & $OwnerCommand @OwnerArguments
            }
            $OwnerExitCode = $LASTEXITCODE
            $OwnerSucceeded = $OwnerExitCode -eq 0
            if (!$OwnerSucceeded -and
                ($OwnerCommand -ceq $Coordinator -or $Suite -eq 'language-1-front-door') -and
                $OwnerExitCode -ne 1) {
                $TimingOutcome = if ($OwnerExitCode -eq 124) {
                    'timed-out'
                } else {
                    'framework-error'
                }
                $Incomplete.Add($Suite)
                Write-Warning (
                    "Native owner '$Suite' is verification-incomplete " +
                    "outcome=$TimingOutcome exit=$OwnerExitCode. " +
                    'No passing evidence was recorded.')
                if (!$AllowIncompleteInfrastructure) {
                    $StopAfterOwner = $true
                }
            } elseif (!$OwnerSucceeded) {
                $TimingOutcome = 'test-failed'
                throw "Native owner '$Suite' exited $OwnerExitCode."
            }
            if ($OwnerSucceeded -and $null -ne $ResultCacheState) {
                try {
                    $Publish = Invoke-VerificationResultCache -CacheArgument @(
                        'publish',
                        $RepositoryRoot,
                        $ResultCacheState.root,
                        $ResultCacheState.stateKey,
                        $ResultCacheState.sourceTree,
                        $ResultCacheState.sourceSentinel,
                        $Suite,
                        $OwnerAction
                    )
                    if ($Publish -eq 'StateChanged') {
                        Write-Warning (
                            'Repository inputs changed during verification; ' +
                            'new passes will not be cached in this run.'
                        )
                        $ResultCacheState = $null
                    } elseif ($Publish -ne 'Stored') {
                        throw "Unexpected cache publication result '$Publish'."
                    }
                } catch {
                    Write-Warning (
                        "Verification result cache publication failed for '$Suite': " +
                        $_.Exception.Message
                    )
                }
            }
        } catch {
            if ($TimingOutcome -eq 'passed') {
                $TimingOutcome = 'framework-error'
            }
            $Failures.Add($Suite)
            if (!$NoFailFast) { $StopAfterOwner = $true }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = $Suite
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
                status = $TimingStatus
                outcome = $TimingOutcome
                exitCode = $OwnerExitCode
            })
        }
        if ($StopAfterOwner) { break }
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
        $OverallOutcome = if ($Failures.Count -ne 0) {
            'failed'
        } elseif ($Incomplete.Count -ne 0) {
            'verification-incomplete'
        } else {
            'passed'
        }
        [pscustomobject]@{
            format = 'windvale-native-changed-verification-timing-2'
            host = Get-VerificationHostName
            startedUtc = $VerificationStartedUtc.ToString('O')
            outcome = $OverallOutcome
            incompleteOwners = @($Incomplete)
            entries = @($Timings)
        } | ConvertTo-Json -Depth 4 |
            Set-Content -LiteralPath $TimingReportPath -Encoding utf8
    }
    if ($Failures.Count -ne 0) {
        throw "Native changed-file verification failed: $($Failures -join ', ')."
    }
    if ($Incomplete.Count -ne 0) {
        $Message = (
            'Native changed-file verification is incomplete: ' +
            ($Incomplete -join ', ') + '.')
        if (!$AllowIncompleteInfrastructure) {
            throw $Message
        }
        Write-Warning "$Message Automatic development feedback remains nonblocking."
    }
} else {
    Write-Host 'Changed-file verification passed without native owner execution.'
}
