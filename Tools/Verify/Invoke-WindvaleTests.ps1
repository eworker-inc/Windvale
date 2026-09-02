[CmdletBinding()]
param(
    [Alias('Filter')]
    [string]$Owner,

    [string]$Shard,

    [switch]$PlanOnly,

    [switch]$AllowLongRun,

    [string]$ResultPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$USAGE_EXIT_CODE = 64
$TEST_FAILURE_EXIT_CODE = 1
$FRAMEWORK_ERROR_EXIT_CODE = 2
$TIMEOUT_EXIT_CODE = 124
$LOCAL_DEVELOPMENT_BUDGET_SECONDS = 600
$MAXIMUM_RESULT_BYTES = 64 * 1024
$MAXIMUM_DETAIL_CHARACTERS = 2048
$USAGE = @'
Usage: pwsh -NoProfile -File Tools/Verify/Invoke-WindvaleTests.ps1 [-Owner <owner-name> | -Shard <1-4>] [-PlanOnly] [-AllowLongRun] [-ResultPath <new-json-path>]
'@
$OwnerWasSpecified = $PSBoundParameters.ContainsKey('Owner')
$ShardWasSpecified = $PSBoundParameters.ContainsKey('Shard')
$RunStartedUtc = [DateTime]::UtcNow
$RunStopwatch = [Diagnostics.Stopwatch]::StartNew()
$script:RunMode = 'unresolved'
$script:PlannedOwners = 0
$script:PlannedCases = [long]0
$script:PlannedExpectedSeconds = [long]0
$script:PlannedMaximumSeconds = [long]0
$script:OwnerResults = [Collections.Generic.List[object]]::new()
$script:RunReport = $null

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

function Write-UsageFailure {
    param([Parameter(Mandatory)][string]$Message)

    [Console]::Error.WriteLine($Message)
    [Console]::Error.WriteLine($USAGE.TrimEnd())
    return $USAGE_EXIT_CODE
}

function Get-BoundedDetail {
    param([AllowNull()][string]$Value)

    if ([string]::IsNullOrEmpty($Value)) { return $null }
    if ($Value.Length -le $MAXIMUM_DETAIL_CHARACTERS) { return $Value }
    return $Value.Substring(0, $MAXIMUM_DETAIL_CHARACTERS) +
        "...[truncated characters=$($Value.Length)]"
}

function Read-StrictTextLines {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Label,
        [Parameter(Mandatory)][int]$MaximumBytes
    )

    $Item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($Item.PSIsContainer -or
        ($Item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or
        $Item.Length -lt 1 -or $Item.Length -gt $MaximumBytes) {
        throw "$Label must be a bounded non-linked ordinary file."
    }
    $Bytes = [IO.File]::ReadAllBytes($Path)
    if ($Bytes.Length -eq 0 -or $Bytes[-1] -ne 10 -or $Bytes -contains 13) {
        throw "$Label must be nonempty LF-only text with a final newline."
    }
    $Utf8 = [Text.UTF8Encoding]::new($false, $true)
    $Text = $Utf8.GetString($Bytes)
    $Lines = @($Text.Split("`n", [StringSplitOptions]::None))
    return @($Lines[0..($Lines.Count - 2)])
}

function Read-DurationProfiles {
    param([Parameter(Mandatory)][string]$ProfilePath)

    $Lines = @(Read-StrictTextLines -Path $ProfilePath -MaximumBytes 16384 `
        -Label 'The verification duration-profile registry')
    if ($Lines.Count -lt 2 -or
        $Lines[0] -cne 'windvale-native-verification-duration-profiles 1') {
        throw 'The verification duration-profile header or inventory is invalid.'
    }

    $Profiles = @{}
    foreach ($Line in $Lines | Select-Object -Skip 1) {
        $Fields = $Line.Split([char]'|')
        $ExpectedSeconds = 0
        $MaximumSeconds = 0
        $InfrastructureRetries = 0
        if ($Fields.Count -ne 4 -or
            $Fields[0] -cnotmatch '^[a-z]+(?:-[a-z]+)*$' -or
            $Profiles.ContainsKey($Fields[0])) {
            throw "Malformed or duplicate verification duration profile: $Line"
        }
        if (![int]::TryParse(
                $Fields[1],
                [Globalization.NumberStyles]::None,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$ExpectedSeconds) -or
            $ExpectedSeconds -lt 1 -or $ExpectedSeconds -gt 3600) {
            throw "Invalid expected verification duration: $Line"
        }
        if (![int]::TryParse(
                $Fields[2],
                [Globalization.NumberStyles]::None,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$MaximumSeconds) -or
            $MaximumSeconds -lt $ExpectedSeconds -or $MaximumSeconds -gt 3600) {
            throw "Invalid maximum verification duration: $Line"
        }
        if (![int]::TryParse(
                $Fields[3],
                [Globalization.NumberStyles]::None,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$InfrastructureRetries) -or
            $InfrastructureRetries -lt 0 -or $InfrastructureRetries -gt 1) {
            throw "Invalid infrastructure retry count: $Line"
        }
        $Profiles[$Fields[0]] = [pscustomobject]@{
            Name = $Fields[0]
            ExpectedSeconds = $ExpectedSeconds
            MaximumSeconds = $MaximumSeconds
            InfrastructureRetries = $InfrastructureRetries
        }
    }
    return $Profiles
}

function Read-VerificationRegistry {
    param(
        [Parameter(Mandatory)][string]$RegistryPath,
        [Parameter(Mandatory)][string]$RepositoryRoot,
        [Parameter(Mandatory)][string]$NativeRoot,
        [Parameter(Mandatory)][string]$HostExtension,
        [Parameter(Mandatory)][hashtable]$Profiles
    )

    $Lines = @(Read-StrictTextLines -Path $RegistryPath -MaximumBytes 1048576 `
        -Label 'The verification-owner registry')
    if ($Lines.Count -lt 2 -or
        $Lines[0] -cne 'windvale-native-verification-owners 2') {
        throw 'The verification-owner registry header or inventory is invalid.'
    }

    $Names = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    $Commands = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    $UsedProfiles = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    $Shards = [Collections.Generic.HashSet[int]]::new()
    $Entries = [Collections.Generic.List[object]]::new()

    foreach ($Line in $Lines | Select-Object -Skip 1) {
        if ([string]::IsNullOrWhiteSpace($Line)) {
            throw 'The verification-owner registry contains an empty entry.'
        }
        $Fields = $Line.Split([char]'|')
        if ($Fields.Count -ne 6) {
            throw "Malformed verification-owner registry entry: $Line"
        }
        $Name = $Fields[0]
        $Command = $Fields[1]
        $CaseCount = 0
        $QualificationShard = 0
        if ($Name -cnotmatch '^[a-z0-9]+(?:[.-][a-z0-9]+)*$' -or
            !$Names.Add($Name)) {
            throw "Invalid or duplicate verification-owner name: $Name"
        }
        if ($Command -cnotmatch '^[A-Za-z0-9]+(?:[.-][A-Za-z0-9]+)*$' -or
            !$Commands.Add($Command)) {
            throw "Invalid or duplicate verification-owner command: $Command"
        }
        if (![int]::TryParse(
                $Fields[2],
                [Globalization.NumberStyles]::None,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$CaseCount) -or $CaseCount -le 0) {
            throw "Invalid verification-owner case count: $Line"
        }
        if (![int]::TryParse(
                $Fields[3],
                [Globalization.NumberStyles]::None,
                [Globalization.CultureInfo]::InvariantCulture,
                [ref]$QualificationShard) -or
            $QualificationShard -lt 1 -or $QualificationShard -gt 4) {
            throw "Invalid verification-owner qualification shard: $Line"
        }
        if (!$Profiles.ContainsKey($Fields[4])) {
            throw "Unknown verification duration profile: $Line"
        }
        if ([string]::IsNullOrWhiteSpace($Fields[5])) {
            throw "Missing verification-owner terminal summary: $Name"
        }

        $CommandPath = Join-Path $NativeRoot ($Command + $HostExtension)
        $CommandItem = Get-Item -LiteralPath $CommandPath -Force -ErrorAction Stop
        if ($CommandItem.PSIsContainer -or
            ($CommandItem.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw "Verification owner must be a non-linked ordinary file: $CommandPath"
        }
        if ($HostExtension -eq '.sh') {
            $RelativeCommand = [IO.Path]::GetRelativePath(
                $RepositoryRoot,
                $CommandPath).Replace('\', '/')
            $IndexEntry = @(git -C $RepositoryRoot ls-files -s -- $RelativeCommand)
            if ($LASTEXITCODE -ne 0 -or $IndexEntry.Count -ne 1 -or
                $IndexEntry[0] -notmatch '^100755 ') {
                throw "Linux verification owner is not executable in Git: $RelativeCommand"
            }
        }

        $Profile = $Profiles[$Fields[4]]
        $null = $UsedProfiles.Add($Profile.Name)
        $null = $Shards.Add($QualificationShard)
        $Entries.Add([pscustomobject]@{
            Name = $Name
            Command = $Command
            CommandPath = $CommandPath
            Cases = $CaseCount
            Shard = $QualificationShard
            DurationProfile = $Profile.Name
            ExpectedSeconds = $Profile.ExpectedSeconds
            MaximumSeconds = $Profile.MaximumSeconds
            InfrastructureRetries = $Profile.InfrastructureRetries
            ExpectedSummary = $Fields[5]
        })
    }
    if ($Shards.Count -ne 4) {
        throw 'The verification-owner registry must assign at least one owner to each of four shards.'
    }
    if ($UsedProfiles.Count -ne $Profiles.Count) {
        throw 'Every verification duration profile must be assigned to at least one owner.'
    }
    return @($Entries)
}

function Read-OwnerProcessStatus {
    param([Parameter(Mandatory)][string]$Path)

    $Item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($Item.PSIsContainer -or
        ($Item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or
        $Item.Length -lt 2 -or $Item.Length -gt 8192) {
        throw 'The owner process-status record is not a bounded ordinary file.'
    }
    $Status = [IO.File]::ReadAllText($Path) | ConvertFrom-Json
    if ($Status.format -cne 'windvale-verification-owner-process-1' -or
        $Status.outcome -cnotin @('exited', 'timed-out', 'framework-error') -or
        $Status.category -cnotmatch '^[a-z]+(?:-[a-z]+)*$' -or
        $Status.elapsedMilliseconds -isnot [long] -or
        $Status.elapsedMilliseconds -lt 0 -or
        $Status.elapsedMilliseconds -gt 86400000 -or
        $Status.stdoutBytes -isnot [long] -or $Status.stdoutBytes -lt 0 -or
        $Status.stderrBytes -isnot [long] -or $Status.stderrBytes -lt 0 -or
        $Status.retryable -isnot [bool] -or
        ($null -ne $Status.detail -and
            ($Status.detail -isnot [string] -or
                $Status.detail.Length -gt $MAXIMUM_DETAIL_CHARACTERS))) {
        throw 'The owner process-status record is invalid.'
    }
    if ($Status.outcome -eq 'exited' -and
        ($Status.exitCode -isnot [long] -or
            $Status.exitCode -lt 0 -or $Status.exitCode -gt 255)) {
        throw 'The owner process-status exit code is invalid.'
    }
    if ($Status.outcome -ne 'exited' -and $null -ne $Status.exitCode) {
        throw 'A non-exit owner process-status record has an exit code.'
    }
    return $Status
}

function Remove-RunnerTemporaryDirectory {
    param(
        [Parameter(Mandatory)][string]$TemporaryRoot,
        [Parameter(Mandatory)][string]$TemporaryDirectory,
        [Parameter(Mandatory)][string[]]$Files
    )

    $FullRoot = [IO.Path]::TrimEndingDirectorySeparator(
        [IO.Path]::GetFullPath($TemporaryRoot))
    $FullDirectory = [IO.Path]::TrimEndingDirectorySeparator(
        [IO.Path]::GetFullPath($TemporaryDirectory))
    if ([IO.Path]::GetDirectoryName($FullDirectory) -cne $FullRoot -or
        ![IO.Path]::GetFileName($FullDirectory).StartsWith(
            'windvale-tests-', [StringComparison]::Ordinal)) {
        throw "Refusing to clean unexpected test-runner directory: $FullDirectory"
    }
    foreach ($File in $Files) {
        if ([IO.File]::Exists($File)) {
            [IO.File]::Delete($File)
        }
    }
    if ([IO.Directory]::Exists($FullDirectory)) {
        [IO.Directory]::Delete($FullDirectory, $false)
    }
}

function Set-RunReport {
    param(
        [Parameter(Mandatory)][string]$Outcome,
        [Parameter(Mandatory)][int]$ExitCode,
        [string]$Detail
    )

    $script:RunReport = [ordered]@{
        format = 'windvale-verification-run-result-1'
        mode = $script:RunMode
        host = Get-VerificationHostName
        outcome = $Outcome
        exitCode = $ExitCode
        startedUtc = $RunStartedUtc.ToString('O')
        elapsedMilliseconds = $RunStopwatch.ElapsedMilliseconds
        ownersPlanned = $script:PlannedOwners
        casesPlanned = $script:PlannedCases
        expectedSeconds = $script:PlannedExpectedSeconds
        maximumSeconds = $script:PlannedMaximumSeconds
        detail = Get-BoundedDetail -Value $Detail
        owners = @($script:OwnerResults)
    }
}

function Write-RunResult {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Report
    )

    $FullPath = [IO.Path]::GetFullPath($Path)
    $Parent = [IO.Path]::GetDirectoryName($FullPath)
    if ([string]::IsNullOrWhiteSpace($Parent)) {
        throw 'The verification result path has no parent directory.'
    }
    $ParentItem = Get-Item -LiteralPath $Parent -Force -ErrorAction Stop
    if (!$ParentItem.PSIsContainer -or
        ($ParentItem.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw 'The verification result parent must be a non-linked ordinary directory.'
    }
    $Json = ($Report | ConvertTo-Json -Depth 8 -Compress) + "`n"
    $Utf8 = [Text.UTF8Encoding]::new($false, $true)
    $Bytes = $Utf8.GetBytes($Json)
    if ($Bytes.Length -gt $MAXIMUM_RESULT_BYTES) {
        throw 'The verification result exceeds its byte limit.'
    }
    $Stream = [IO.FileStream]::new(
        $FullPath,
        [IO.FileMode]::CreateNew,
        [IO.FileAccess]::Write,
        [IO.FileShare]::None)
    try {
        $Stream.Write($Bytes, 0, $Bytes.Length)
        $Stream.Flush($true)
    } finally {
        $Stream.Dispose()
    }
}

function Invoke-WindvaleTestRunner {
    if ($PSVersionTable.PSVersion.Major -lt 7) {
        throw 'The Windvale test runner requires PowerShell 7 or newer.'
    }
    if (![string]::IsNullOrWhiteSpace($Owner) -and
        ![string]::IsNullOrWhiteSpace($Shard)) {
        return (Write-UsageFailure '-Owner and -Shard are mutually exclusive.')
    }
    if ($OwnerWasSpecified -and [string]::IsNullOrWhiteSpace($Owner)) {
        return (Write-UsageFailure '-Owner requires a nonempty owner name.')
    }
    if ($ShardWasSpecified -and $Shard -notmatch '^[1-4]$') {
        return (Write-UsageFailure '-Shard must be one of 1, 2, 3, or 4.')
    }

    $RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    $NativeRoot = Join-Path $RepositoryRoot 'Tools/Native'
    $RegistryPath = Join-Path $RepositoryRoot 'Tests/Native/Verification-Owners.txt'
    $ProfilePath = Join-Path $RepositoryRoot (
        'Tests/Native/Verification-Duration-Profiles.txt')
    $RunningOnWindows = (
        [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT)
    $HostExtension = if ($RunningOnWindows) { '.cmd' } else { '.sh' }
    $Profiles = Read-DurationProfiles -ProfilePath $ProfilePath
    $RegistryArguments = @{
        RegistryPath = $RegistryPath
        RepositoryRoot = $RepositoryRoot
        NativeRoot = $NativeRoot
        HostExtension = $HostExtension
        Profiles = $Profiles
    }
    $Registry = @(Read-VerificationRegistry @RegistryArguments)

    $Selected = @($Registry | Where-Object {
        ([string]::IsNullOrWhiteSpace($Owner) -or $_.Name -ceq $Owner) -and
        ([string]::IsNullOrWhiteSpace($Shard) -or $_.Shard -eq [int]$Shard)
    })
    if ($Selected.Count -eq 0) {
        if (![string]::IsNullOrWhiteSpace($Owner)) {
            return (Write-UsageFailure "Unknown verification owner: $Owner")
        }
        return (Write-UsageFailure "Empty qualification shard: $Shard")
    }

    $script:RunMode = if (![string]::IsNullOrWhiteSpace($Owner)) {
        "owner:$Owner"
    } elseif (![string]::IsNullOrWhiteSpace($Shard)) {
        "shard:$Shard"
    } else {
        'complete'
    }
    $script:PlannedOwners = $Selected.Count
    $script:PlannedCases = [long](
        ($Selected | Measure-Object -Property Cases -Sum).Sum)
    $script:PlannedExpectedSeconds = [long](
        ($Selected | Measure-Object -Property ExpectedSeconds -Sum).Sum)
    $script:PlannedMaximumSeconds = [long](
        ($Selected | Measure-Object -Property MaximumSeconds -Sum).Sum)

    Write-Host (
        "Verification plan mode=$($script:RunMode) " +
        "owners=$($script:PlannedOwners) cases=$($script:PlannedCases) " +
        "expected-seconds=$($script:PlannedExpectedSeconds) " +
        "maximum-seconds=$($script:PlannedMaximumSeconds) " +
        "host=$([Environment]::OSVersion.Platform)")
    if ($PlanOnly) {
        foreach ($Entry in $Selected) {
            Write-Host (
                "PLAN  owner=$($Entry.Name) command=$($Entry.Command)$HostExtension " +
                "cases=$($Entry.Cases) shard=$($Entry.Shard) " +
                "duration-profile=$($Entry.DurationProfile) " +
                "expected-seconds=$($Entry.ExpectedSeconds) " +
                "maximum-seconds=$($Entry.MaximumSeconds)")
        }
        Set-RunReport -Outcome 'planned' -ExitCode 0
        return 0
    }
    if ($script:PlannedExpectedSeconds -gt
            $LOCAL_DEVELOPMENT_BUDGET_SECONDS -and !$AllowLongRun) {
        $Message = (
            "Selected plan expects $($script:PlannedExpectedSeconds) seconds, " +
            "which exceeds the $LOCAL_DEVELOPMENT_BUDGET_SECONDS-second local " +
            'development budget. Inspect -PlanOnly and pass -AllowLongRun only ' +
            'for an approved qualification or named longer run.')
        [Console]::Error.WriteLine($Message)
        Set-RunReport -Outcome 'refused' -ExitCode $USAGE_EXIT_CODE `
            -Detail $Message
        return $USAGE_EXIT_CODE
    }

    $Node = Get-Command node -CommandType Application -ErrorAction Stop
    $NodeVersion = @(& $Node.Source --version 2>&1)
    if ($LASTEXITCODE -ne 0 -or $NodeVersion.Count -ne 1 -or
        $NodeVersion[0] -notmatch '^v24\.[0-9]+\.[0-9]+$') {
        throw 'The Windvale test runner requires Node.js 24.'
    }
    $StreamHelper = Join-Path $NativeRoot 'Stream-Verification-Owner.mjs'
    $StreamItem = Get-Item -LiteralPath $StreamHelper -Force -ErrorAction Stop
    if ($StreamItem.PSIsContainer -or
        ($StreamItem.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw "Verification stream helper must be a non-linked ordinary file: $StreamHelper"
    }

    $TemporaryRoot = [IO.Path]::GetTempPath()
    $TemporaryDirectory = Join-Path $TemporaryRoot (
        'windvale-tests-' + [Guid]::NewGuid().ToString('N'))
    $null = [IO.Directory]::CreateDirectory($TemporaryDirectory)
    $TemporaryFiles = [Collections.Generic.List[string]]::new()
    $AttemptedSuites = 0
    $PassedSuites = 0
    $AttemptedCases = [long]0

    try {
        foreach ($Entry in $Selected) {
            $AttemptedSuites += 1
            if ($AttemptedCases -gt [long]::MaxValue - [long]$Entry.Cases) {
                throw 'The attempted verification case total exceeds the supported range.'
            }
            $AttemptedCases += [long]$Entry.Cases
            Write-Host (
                "Progress: step=native-owner item=$AttemptedSuites/$($Selected.Count) " +
                "owner=$($Entry.Name)")

            $OwnerStopwatch = [Diagnostics.Stopwatch]::StartNew()
            $OwnerOutcome = 'framework-error'
            $OwnerExitCode = $FRAMEWORK_ERROR_EXIT_CODE
            $OwnerDetail = 'No owner attempt completed.'
            $Attempt = 0
            $MaximumAttempts = 1 + $Entry.InfrastructureRetries
            while ($Attempt -lt $MaximumAttempts) {
                $MaximumMilliseconds = [long]$Entry.MaximumSeconds * 1000
                $RemainingMilliseconds = (
                    $MaximumMilliseconds - $OwnerStopwatch.ElapsedMilliseconds)
                if ($RemainingMilliseconds -le 0) {
                    $OwnerOutcome = 'timed-out'
                    $OwnerExitCode = $TIMEOUT_EXIT_CODE
                    $OwnerDetail = (
                        "Owner exhausted its $MaximumMilliseconds ms total deadline.")
                    break
                }
                $Attempt += 1
                $Prefix = "$($AttemptedSuites)-$Attempt"
                $SuiteOutput = Join-Path $TemporaryDirectory "$Prefix.out"
                $SuiteError = Join-Path $TemporaryDirectory "$Prefix.err"
                $ProcessStatusPath = Join-Path $TemporaryDirectory "$Prefix.process.json"
                $TemporaryFiles.Add($SuiteOutput)
                $TemporaryFiles.Add($SuiteError)
                $TemporaryFiles.Add($ProcessStatusPath)
                & $Node.Source $StreamHelper $SuiteOutput $SuiteError `
                    $Entry.CommandPath $RemainingMilliseconds $ProcessStatusPath
                $HelperExitCode = $LASTEXITCODE
                $ProcessStatus = $null
                try {
                    $ProcessStatus = Read-OwnerProcessStatus -Path $ProcessStatusPath
                } catch {
                    $OwnerDetail = (
                        'Missing or invalid owner process-status record: ' +
                        $_.Exception.Message)
                }

                $RetryableInfrastructure = $false
                if ($null -eq $ProcessStatus) {
                    $OwnerOutcome = 'framework-error'
                    $OwnerExitCode = $FRAMEWORK_ERROR_EXIT_CODE
                    $RetryableInfrastructure = $HelperExitCode -eq 70
                } elseif ($ProcessStatus.outcome -eq 'timed-out') {
                    $OwnerOutcome = 'timed-out'
                    $OwnerExitCode = $TIMEOUT_EXIT_CODE
                    $OwnerDetail = $ProcessStatus.detail
                } elseif ($ProcessStatus.outcome -eq 'framework-error') {
                    $OwnerOutcome = 'framework-error'
                    $OwnerExitCode = $FRAMEWORK_ERROR_EXIT_CODE
                    $OwnerDetail = $ProcessStatus.detail
                    $RetryableInfrastructure = $ProcessStatus.retryable
                } elseif ($HelperExitCode -ne $ProcessStatus.exitCode) {
                    $OwnerOutcome = 'framework-error'
                    $OwnerExitCode = $FRAMEWORK_ERROR_EXIT_CODE
                    $OwnerDetail = (
                        "Stream helper exit $HelperExitCode differs from owner " +
                        "exit $($ProcessStatus.exitCode).")
                    $RetryableInfrastructure = $true
                } elseif ($ProcessStatus.exitCode -ne 0) {
                    $OwnerOutcome = 'test-failed'
                    $OwnerExitCode = $TEST_FAILURE_EXIT_CODE
                    $OwnerDetail = "Native command exited $($ProcessStatus.exitCode)."
                } elseif ((Get-Item -LiteralPath $SuiteError).Length -ne 0) {
                    $OwnerOutcome = 'test-failed'
                    $OwnerExitCode = $TEST_FAILURE_EXIT_CODE
                    $OwnerDetail = 'Native command wrote standard error.'
                } else {
                    $ActualSummary = @(
                        Get-Content -LiteralPath $SuiteOutput |
                            Where-Object { ![string]::IsNullOrWhiteSpace($_) } |
                            Select-Object -Last 1
                    )
                    if ($ActualSummary.Count -ne 1 -or
                        $ActualSummary[0] -cne $Entry.ExpectedSummary) {
                        $OwnerOutcome = 'framework-error'
                        $OwnerExitCode = $FRAMEWORK_ERROR_EXIT_CODE
                        $OwnerDetail = 'Native command terminal summary differs.'
                    } else {
                        $OwnerOutcome = 'passed'
                        $OwnerExitCode = 0
                        $OwnerDetail = $null
                    }
                }

                $OwnerDetail = Get-BoundedDetail -Value $OwnerDetail

                if ($OwnerOutcome -eq 'framework-error' -and
                    $RetryableInfrastructure -and $Attempt -lt $MaximumAttempts) {
                    Write-Warning (
                        "Retrying owner '$($Entry.Name)' after infrastructure " +
                        "error attempt=$Attempt/${MaximumAttempts}: $OwnerDetail")
                    continue
                }
                break
            }

            $OwnerStopwatch.Stop()
            $OwnerResult = [ordered]@{
                name = $Entry.Name
                outcome = $OwnerOutcome
                exitCode = $OwnerExitCode
                cases = $Entry.Cases
                shard = $Entry.Shard
                durationProfile = $Entry.DurationProfile
                expectedSeconds = $Entry.ExpectedSeconds
                maximumSeconds = $Entry.MaximumSeconds
                attempts = $Attempt
                elapsedMilliseconds = $OwnerStopwatch.ElapsedMilliseconds
                detail = $OwnerDetail
            }
            $script:OwnerResults.Add([pscustomobject]$OwnerResult)

            if ($OwnerOutcome -eq 'passed') {
                $PassedSuites += 1
                Write-Host (
                    "PASS  suite $($Entry.Name) cases=$($Entry.Cases) " +
                    "attempts=$Attempt elapsed-ms=$($OwnerStopwatch.ElapsedMilliseconds)")
                continue
            }

            [Console]::Error.WriteLine(
                "FAIL  suite $($Entry.Name) outcome=$OwnerOutcome " +
                "attempts=$Attempt elapsed-ms=$($OwnerStopwatch.ElapsedMilliseconds) " +
                "detail=$OwnerDetail")
            [Console]::Error.WriteLine(
                "Timing: elapsed-ms=$($RunStopwatch.ElapsedMilliseconds)")
            [Console]::Error.WriteLine(
                "Suites: $AttemptedSuites, Passed: $PassedSuites, " +
                "Failed: $($AttemptedSuites - $PassedSuites), Cases: $AttemptedCases")
            Set-RunReport -Outcome $OwnerOutcome -ExitCode $OwnerExitCode `
                -Detail "Owner $($Entry.Name): $OwnerDetail"
            return $OwnerExitCode
        }

        Write-Host "Timing: elapsed-ms=$($RunStopwatch.ElapsedMilliseconds)"
        Write-Host (
            "Suites: $AttemptedSuites, Passed: $PassedSuites, Failed: 0, " +
            "Cases: $AttemptedCases")
        Set-RunReport -Outcome 'passed' -ExitCode 0
        return 0
    } finally {
        $CleanupArguments = @{
            TemporaryRoot = $TemporaryRoot
            TemporaryDirectory = $TemporaryDirectory
            Files = @($TemporaryFiles)
        }
        Remove-RunnerTemporaryDirectory @CleanupArguments
    }
}

$ExitCode = try {
    $Code = Invoke-WindvaleTestRunner
    if ($null -eq $script:RunReport) {
        Set-RunReport -Outcome 'refused' -ExitCode $Code `
            -Detail 'The requested runner selection was invalid.'
    }
    $Code
} catch {
    $Message = Get-BoundedDetail -Value $_.Exception.Message
    [Console]::Error.WriteLine("Verification framework error: $Message")
    Set-RunReport -Outcome 'framework-error' `
        -ExitCode $FRAMEWORK_ERROR_EXIT_CODE -Detail $Message
    $FRAMEWORK_ERROR_EXIT_CODE
}

$RunStopwatch.Stop()
if (![string]::IsNullOrWhiteSpace($ResultPath)) {
    try {
        $script:RunReport.elapsedMilliseconds = $RunStopwatch.ElapsedMilliseconds
        Write-RunResult -Path $ResultPath -Report $script:RunReport
    } catch {
        [Console]::Error.WriteLine(
            "Verification framework error: could not write result: " +
            $_.Exception.Message)
        $ExitCode = $FRAMEWORK_ERROR_EXIT_CODE
    }
}
exit $ExitCode
