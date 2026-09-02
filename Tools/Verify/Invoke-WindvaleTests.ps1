[CmdletBinding()]
param(
    [Alias('Filter')]
    [string]$Owner,

    [string]$Shard,

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$USAGE_EXIT_CODE = 64
$USAGE = @'
Usage: pwsh -NoProfile -File Tools/Verify/Invoke-WindvaleTests.ps1 [-Owner <owner-name> | -Shard <1-4>] [-PlanOnly]
'@
$OwnerWasSpecified = $PSBoundParameters.ContainsKey('Owner')
$ShardWasSpecified = $PSBoundParameters.ContainsKey('Shard')

function Write-UsageFailure {
    param([Parameter(Mandatory)][string]$Message)

    [Console]::Error.WriteLine($Message)
    [Console]::Error.WriteLine($USAGE.TrimEnd())
    return $USAGE_EXIT_CODE
}

function Read-VerificationRegistry {
    param(
        [Parameter(Mandatory)][string]$RegistryPath,
        [Parameter(Mandatory)][string]$RepositoryRoot,
        [Parameter(Mandatory)][string]$NativeRoot,
        [Parameter(Mandatory)][string]$HostExtension
    )

    $Bytes = [IO.File]::ReadAllBytes($RegistryPath)
    if ($Bytes.Length -eq 0 -or $Bytes[-1] -ne 10 -or $Bytes -contains 13) {
        throw 'The verification-owner registry must be nonempty LF-only text with a final newline.'
    }
    $Utf8 = [Text.UTF8Encoding]::new($false, $true)
    $Text = $Utf8.GetString($Bytes)
    $Lines = @($Text.Split("`n", [StringSplitOptions]::None))
    $Lines = @($Lines[0..($Lines.Count - 2)])
    if ($Lines.Count -lt 2 -or
        $Lines[0] -cne 'windvale-native-verification-owners 1') {
        throw 'The verification-owner registry header or inventory is invalid.'
    }

    $Names = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    $Commands = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    $Shards = [Collections.Generic.HashSet[int]]::new()
    $Entries = [Collections.Generic.List[object]]::new()
    $TotalCases = [long]0

    foreach ($Line in $Lines | Select-Object -Skip 1) {
        if ([string]::IsNullOrWhiteSpace($Line)) {
            throw 'The verification-owner registry contains an empty entry.'
        }
        $Fields = $Line.Split([char]'|')
        if ($Fields.Count -ne 5) {
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
        if ([string]::IsNullOrWhiteSpace($Fields[4])) {
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

        if ($TotalCases -gt [long]::MaxValue - [long]$CaseCount) {
            throw 'The verification-owner case total exceeds the supported range.'
        }
        $TotalCases += [long]$CaseCount
        $null = $Shards.Add($QualificationShard)
        $Entries.Add([pscustomobject]@{
            Name = $Name
            Command = $Command
            CommandPath = $CommandPath
            Cases = $CaseCount
            Shard = $QualificationShard
            ExpectedSummary = $Fields[4]
        })
    }
    if ($Shards.Count -ne 4) {
        throw 'The verification-owner registry must assign at least one owner to each of four shards.'
    }

    return [pscustomobject]@{
        Entries = @($Entries)
        TotalCases = $TotalCases
    }
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
    $RunningOnWindows = (
        [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT)
    $HostExtension = if ($RunningOnWindows) { '.cmd' } else { '.sh' }
    $RegistryArguments = @{
        RegistryPath = $RegistryPath
        RepositoryRoot = $RepositoryRoot
        NativeRoot = $NativeRoot
        HostExtension = $HostExtension
    }
    $Registry = Read-VerificationRegistry @RegistryArguments

    $Selected = @($Registry.Entries | Where-Object {
        ([string]::IsNullOrWhiteSpace($Owner) -or $_.Name -ceq $Owner) -and
        ([string]::IsNullOrWhiteSpace($Shard) -or $_.Shard -eq [int]$Shard)
    })
    if ($Selected.Count -eq 0) {
        if (![string]::IsNullOrWhiteSpace($Owner)) {
            return (Write-UsageFailure "Unknown verification owner: $Owner")
        }
        return (Write-UsageFailure "Empty qualification shard: $Shard")
    }
    $SelectedCases = [long](($Selected | Measure-Object -Property Cases -Sum).Sum)
    $Mode = if (![string]::IsNullOrWhiteSpace($Owner)) {
        "owner:$Owner"
    } elseif (![string]::IsNullOrWhiteSpace($Shard)) {
        "shard:$Shard"
    } else {
        'complete'
    }

    Write-Host (
        "Verification plan mode=$Mode owners=$($Selected.Count) " +
        "cases=$SelectedCases host=$([Environment]::OSVersion.Platform)")
    if ($PlanOnly) {
        foreach ($Entry in $Selected) {
            Write-Host (
                "PLAN  owner=$($Entry.Name) command=$($Entry.Command)$HostExtension " +
                "cases=$($Entry.Cases) shard=$($Entry.Shard)")
        }
        return 0
    }

    $Node = Get-Command node -CommandType Application -ErrorAction Stop
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
    $SuiteOutput = Join-Path $TemporaryDirectory 'Suite.out'
    $SuiteError = Join-Path $TemporaryDirectory 'Suite.err'
    $TotalStopwatch = [Diagnostics.Stopwatch]::StartNew()
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
            $SuiteStopwatch = [Diagnostics.Stopwatch]::StartNew()
            & $Node.Source $StreamHelper $SuiteOutput $SuiteError $Entry.CommandPath
            $SuiteExitCode = $LASTEXITCODE
            $SuiteStopwatch.Stop()
            $SuiteElapsed = $SuiteStopwatch.ElapsedMilliseconds

            if ($SuiteExitCode -ne 0) {
                [Console]::Error.WriteLine(
                    "FAIL  suite $($Entry.Name): native command exited " +
                    "$SuiteExitCode elapsed-ms=$SuiteElapsed")
                [Console]::Error.WriteLine(
                    "Timing: elapsed-ms=$($TotalStopwatch.ElapsedMilliseconds)")
                [Console]::Error.WriteLine(
                    "Suites: $AttemptedSuites, Passed: $PassedSuites, " +
                    "Failed: $($AttemptedSuites - $PassedSuites), Cases: $AttemptedCases")
                return 1
            }
            if ((Get-Item -LiteralPath $SuiteError).Length -ne 0) {
                [Console]::Error.WriteLine(
                    "FAIL  suite $($Entry.Name): native command wrote standard error")
                [Console]::Error.WriteLine(
                    "Timing: elapsed-ms=$($TotalStopwatch.ElapsedMilliseconds)")
                [Console]::Error.WriteLine(
                    "Suites: $AttemptedSuites, Passed: $PassedSuites, " +
                    "Failed: $($AttemptedSuites - $PassedSuites), Cases: $AttemptedCases")
                return 1
            }
            $ActualSummary = @(
                Get-Content -LiteralPath $SuiteOutput |
                    Where-Object { ![string]::IsNullOrWhiteSpace($_) } |
                    Select-Object -Last 1
            )
            if ($ActualSummary.Count -ne 1 -or
                $ActualSummary[0] -cne $Entry.ExpectedSummary) {
                [Console]::Error.WriteLine(
                    "FAIL  suite $($Entry.Name): summary differs")
                [Console]::Error.WriteLine(
                    "Timing: elapsed-ms=$($TotalStopwatch.ElapsedMilliseconds)")
                [Console]::Error.WriteLine(
                    "Suites: $AttemptedSuites, Passed: $PassedSuites, " +
                    "Failed: $($AttemptedSuites - $PassedSuites), Cases: $AttemptedCases")
                return 1
            }

            [IO.File]::Delete($SuiteOutput)
            [IO.File]::Delete($SuiteError)
            $PassedSuites += 1
            Write-Host (
                "PASS  suite $($Entry.Name) cases=$($Entry.Cases) " +
                "elapsed-ms=$SuiteElapsed")
        }

        $TotalStopwatch.Stop()
        Write-Host "Timing: elapsed-ms=$($TotalStopwatch.ElapsedMilliseconds)"
        Write-Host (
            "Suites: $AttemptedSuites, Passed: $PassedSuites, Failed: 0, " +
            "Cases: $AttemptedCases")
        return 0
    } finally {
        $TotalStopwatch.Stop()
        $CleanupArguments = @{
            TemporaryRoot = $TemporaryRoot
            TemporaryDirectory = $TemporaryDirectory
            Files = @($SuiteOutput, $SuiteError)
        }
        Remove-RunnerTemporaryDirectory @CleanupArguments
    }
}

$ExitCode = try {
    Invoke-WindvaleTestRunner
} catch {
    [Console]::Error.WriteLine("Verification framework error: $($_.Exception.Message)")
    1
}
exit $ExitCode
