[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$InputPath,

    [Parameter(Mandatory)]
    [string]$HistoryPath,

    [string]$AnalysisPath,

    [ValidateRange(2, 20)]
    [int]$MinimumSamplesPerHost = 5,

    [ValidateRange(5, 50)]
    [int]$MaximumSamplesPerOwnerHost = 20,

    [string]$HostName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$MAXIMUM_INPUT_FILES = 256
$MAXIMUM_INPUT_DIRECTORIES = 512
$MAXIMUM_INPUT_ENTRIES = 2048
$MAXIMUM_INPUT_FILE_BYTES = 64 * 1024
$MAXIMUM_INPUT_TOTAL_BYTES = 16 * 1024 * 1024
$MAXIMUM_HISTORY_BYTES = 2 * 1024 * 1024
$MAXIMUM_ANALYSIS_BYTES = 256 * 1024
$MAXIMUM_ELAPSED_MILLISECONDS = 3700000
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$DurationProfilePath = Join-Path $RepositoryRoot `
    'Tests/Native/Verification-Duration-Profiles.txt'
$OwnerRegistryPath = Join-Path $RepositoryRoot `
    'Tests/Native/Verification-Owners.txt'
$Utf8 = [Text.UTF8Encoding]::new($false, $true)

function Get-JsonProperty {
    param(
        [Parameter(Mandatory)]$Object,
        [Parameter(Mandatory)][string]$Name
    )

    $Property = $Object.PSObject.Properties[$Name]
    if ($null -eq $Property) { return $null }
    return $Property.Value
}

function Read-BoundedJson {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][long]$MaximumBytes,
        [Parameter(Mandatory)][string]$Label
    )

    $Item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($Item.PSIsContainer -or
        ($Item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or
        $Item.Length -lt 2 -or $Item.Length -gt $MaximumBytes) {
        throw "$Label must be a bounded non-linked ordinary file."
    }
    try {
        $Text = $Utf8.GetString([IO.File]::ReadAllBytes($Item.FullName))
        return $Text | ConvertFrom-Json -Depth 16 -ErrorAction Stop
    } catch {
        throw "$Label is not strict UTF-8 JSON: $($_.Exception.Message)"
    }
}

function Read-RegistryLines {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][int]$MaximumBytes,
        [Parameter(Mandatory)][string]$Label
    )

    $Item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
    if ($Item.PSIsContainer -or
        ($Item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or
        $Item.Length -lt 2 -or $Item.Length -gt $MaximumBytes) {
        throw "$Label must be a bounded non-linked ordinary file."
    }
    $Bytes = [IO.File]::ReadAllBytes($Item.FullName)
    if ($Bytes[-1] -ne 10 -or $Bytes -contains 13) {
        throw "$Label must use LF-only text with a final newline."
    }
    $Text = $Utf8.GetString($Bytes)
    $Lines = @($Text.Split("`n", [StringSplitOptions]::None))
    return @($Lines[0..($Lines.Count - 2)])
}

function Convert-BoundedInteger {
    param(
        [AllowNull()]$Value,
        [Parameter(Mandatory)][long]$Minimum,
        [Parameter(Mandatory)][long]$Maximum,
        [Parameter(Mandatory)][string]$Label
    )

    $Parsed = [long]0
    $Text = if ($null -eq $Value) { '' } else {
        [Convert]::ToString($Value, [Globalization.CultureInfo]::InvariantCulture)
    }
    if (![long]::TryParse(
            $Text,
            [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$Parsed) -or
        $Parsed -lt $Minimum -or $Parsed -gt $Maximum) {
        throw "$Label is not an integer in the permitted range."
    }
    return $Parsed
}

function Convert-ObservedUtc {
    param(
        [AllowNull()]$Value,
        [Parameter(Mandatory)][datetime]$Fallback
    )

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace($Value.ToString())) {
        return $Fallback.ToUniversalTime().ToString('O')
    }
    $Parsed = [DateTimeOffset]::MinValue
    if (![DateTimeOffset]::TryParse(
            $Value.ToString(),
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::RoundtripKind,
            [ref]$Parsed)) {
        throw 'A verification timing record has an invalid observed timestamp.'
    }
    return $Parsed.UtcDateTime.ToString('O')
}

function Get-DefaultHostName {
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

function Resolve-HostName {
    param([AllowNull()]$RecordHost)

    $Resolved = if (![string]::IsNullOrWhiteSpace($HostName)) {
        $HostName
    } elseif ($null -ne $RecordHost -and
        ![string]::IsNullOrWhiteSpace($RecordHost.ToString())) {
        $RecordHost.ToString()
    } else {
        Get-DefaultHostName
    }
    if ($Resolved -cnotmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$') {
        throw "Invalid verification timing host name: $Resolved"
    }
    return $Resolved
}

function Get-HostFamily {
    param([Parameter(Mandatory)][string]$Value)

    if ($Value -match '^(?i:Windows|Win32NT)$') { return 'windows' }
    if ($Value -match '^(?i:Linux|Unix)$') { return 'linux' }
    return 'other'
}

function Write-BoundedJson {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value,
        [Parameter(Mandatory)][int]$MaximumBytes,
        [Parameter(Mandatory)][string]$Label
    )

    $FullPath = [IO.Path]::GetFullPath($Path)
    $Parent = [IO.Path]::GetDirectoryName($FullPath)
    if ([string]::IsNullOrWhiteSpace($Parent)) {
        throw "$Label path has no parent directory."
    }
    $ParentItem = Get-Item -LiteralPath $Parent -Force -ErrorAction Stop
    if (!$ParentItem.PSIsContainer -or
        ($ParentItem.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw "$Label parent must be a non-linked ordinary directory."
    }
    if ([IO.File]::Exists($FullPath)) {
        $Existing = Get-Item -LiteralPath $FullPath -Force
        if ($Existing.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "$Label destination must not be a link."
        }
    } elseif ([IO.Directory]::Exists($FullPath)) {
        throw "$Label destination must not be a directory."
    }
    $Json = ($Value | ConvertTo-Json -Depth 10 -Compress) + "`n"
    $Bytes = $Utf8.GetBytes($Json)
    if ($Bytes.Length -gt $MaximumBytes) {
        throw "$Label exceeds its $MaximumBytes-byte output bound."
    }
    $TemporaryPath = Join-Path $Parent (
        '.windvale-verification-timing-' + [Guid]::NewGuid().ToString('N') + '.tmp')
    try {
        [IO.File]::WriteAllBytes($TemporaryPath, $Bytes)
        [IO.File]::Move($TemporaryPath, $FullPath, $true)
    } finally {
        if ([IO.File]::Exists($TemporaryPath)) {
            [IO.File]::Delete($TemporaryPath)
        }
    }
}

function Get-BoundedInputFiles {
    param([Parameter(Mandatory)]$InputItem)

    $Files = [Collections.Generic.List[object]]::new()
    if (!$InputItem.PSIsContainer) {
        $Files.Add($InputItem)
        return @($Files)
    }
    $Directories = [Collections.Generic.Queue[string]]::new()
    $Directories.Enqueue($InputItem.FullName)
    $DirectoryCount = 1
    $EntryCount = 0
    while ($Directories.Count -ne 0) {
        $Directory = $Directories.Dequeue()
        foreach ($Child in Get-ChildItem -LiteralPath $Directory -Force) {
            $EntryCount += 1
            if ($EntryCount -gt $MAXIMUM_INPUT_ENTRIES) {
                throw "Verification timing discovery exceeds $MAXIMUM_INPUT_ENTRIES entries."
            }
            if ($Child.Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw "The verification timing input contains a link: $($Child.FullName)"
            }
            if ($Child.PSIsContainer) {
                $DirectoryCount += 1
                if ($DirectoryCount -gt $MAXIMUM_INPUT_DIRECTORIES) {
                    throw "Verification timing discovery exceeds $MAXIMUM_INPUT_DIRECTORIES directories."
                }
                $Directories.Enqueue($Child.FullName)
            } elseif ($Child.Extension -ieq '.json') {
                if ($Files.Count -ge $MAXIMUM_INPUT_FILES) {
                    throw "Verification timing input exceeds $MAXIMUM_INPUT_FILES JSON files."
                }
                $Files.Add($Child)
            }
        }
    }
    return @($Files)
}

$ProfileLines = Read-RegistryLines -Path $DurationProfilePath `
    -MaximumBytes (16 * 1024) -Label 'Verification duration-profile registry'
if ($ProfileLines.Count -lt 2 -or
    $ProfileLines[0] -cne 'windvale-native-verification-duration-profiles 1') {
    throw 'The verification duration-profile registry header is invalid.'
}
$Profiles = [Collections.Generic.List[object]]::new()
$ProfilesByName = @{}
$PreviousExpected = 0
$PreviousMaximum = 0
foreach ($Line in $ProfileLines | Select-Object -Skip 1) {
    $Fields = $Line -split '\|', 4
    if ($Fields.Count -ne 4 -or
        $Fields[0] -cnotmatch '^[a-z]+(?:-[a-z]+)*$' -or
        $ProfilesByName.ContainsKey($Fields[0])) {
        throw "Malformed verification duration profile: $Line"
    }
    $Expected = Convert-BoundedInteger -Value $Fields[1] -Minimum 1 `
        -Maximum 3600 -Label 'Duration-profile expected seconds'
    $Maximum = Convert-BoundedInteger -Value $Fields[2] -Minimum $Expected `
        -Maximum 3600 -Label 'Duration-profile maximum seconds'
    $Retries = Convert-BoundedInteger -Value $Fields[3] -Minimum 0 `
        -Maximum 1 -Label 'Duration-profile retry count'
    if ($Expected -le $PreviousExpected -or $Maximum -lt $PreviousMaximum) {
        throw 'Duration profiles must increase by expected time without reducing the maximum.'
    }
    $Profile = [pscustomobject]@{
        name = $Fields[0]
        expectedSeconds = $Expected
        maximumSeconds = $Maximum
        infrastructureRetries = $Retries
        index = $Profiles.Count
    }
    $Profiles.Add($Profile)
    $ProfilesByName[$Profile.name] = $Profile
    $PreviousExpected = $Expected
    $PreviousMaximum = $Maximum
}

$OwnerLines = Read-RegistryLines -Path $OwnerRegistryPath `
    -MaximumBytes (1024 * 1024) -Label 'Verification owner registry'
if ($OwnerLines.Count -lt 2 -or
    $OwnerLines[0] -cne 'windvale-native-verification-owners 2') {
    throw 'The verification owner registry header is invalid.'
}
$OwnersByName = @{}
foreach ($Line in $OwnerLines | Select-Object -Skip 1) {
    $Fields = $Line -split '\|', 6
    if ($Fields.Count -ne 6 -or
        $Fields[0] -cnotmatch '^[a-z0-9]+(?:[.-][a-z0-9]+)*$' -or
        $OwnersByName.ContainsKey($Fields[0]) -or
        !$ProfilesByName.ContainsKey($Fields[4])) {
        throw "Malformed verification owner registry entry: $Line"
    }
    $OwnersByName[$Fields[0]] = [pscustomobject]@{
        name = $Fields[0]
        profile = $Fields[4]
    }
}

$FullHistoryPath = [IO.Path]::GetFullPath($HistoryPath)
$FullAnalysisPath = if ([string]::IsNullOrWhiteSpace($AnalysisPath)) {
    $null
} else {
    [IO.Path]::GetFullPath($AnalysisPath)
}
$InputItem = Get-Item -LiteralPath $InputPath -Force -ErrorAction Stop
if ($InputItem.Attributes -band [IO.FileAttributes]::ReparsePoint) {
    throw 'The verification timing input must not be a link.'
}
$InputFiles = @(Get-BoundedInputFiles -InputItem $InputItem)
$InputFiles = @($InputFiles | Where-Object {
    $FullName = [IO.Path]::GetFullPath($_.FullName)
    $FullName -cne $FullHistoryPath -and
        ($null -eq $FullAnalysisPath -or $FullName -cne $FullAnalysisPath)
} | Sort-Object FullName)
if ($InputFiles.Count -eq 0 -or $InputFiles.Count -gt $MAXIMUM_INPUT_FILES) {
    throw "Verification timing input must contain 1-$MAXIMUM_INPUT_FILES JSON files."
}
$InputTotalBytes = [long]0
foreach ($File in $InputFiles) {
    if ($File.Length -lt 2 -or $File.Length -gt $MAXIMUM_INPUT_FILE_BYTES) {
        throw "Verification timing input file is outside its byte bound: $($File.FullName)"
    }
    $InputTotalBytes += $File.Length
}
if ($InputTotalBytes -gt $MAXIMUM_INPUT_TOTAL_BYTES) {
    throw 'Verification timing inputs exceed their aggregate byte bound.'
}

$AllSamples = [Collections.Generic.List[object]]::new()
if ([IO.File]::Exists($FullHistoryPath)) {
    $History = Read-BoundedJson -Path $FullHistoryPath `
        -MaximumBytes $MAXIMUM_HISTORY_BYTES -Label 'Verification timing history'
    if ((Get-JsonProperty -Object $History -Name 'format') -cne
        'windvale-verification-timing-history-1') {
        throw 'The verification timing history format is invalid.'
    }
    $HistorySamples = Get-JsonProperty -Object $History -Name 'samples'
    if ($null -eq $HistorySamples) {
        throw 'The verification timing history has no samples.'
    }
    foreach ($Sample in @($HistorySamples)) {
        $OwnerName = Get-JsonProperty -Object $Sample -Name 'owner'
        $SampleHost = Get-JsonProperty -Object $Sample -Name 'host'
        $ObservedUtc = Get-JsonProperty -Object $Sample -Name 'observedUtc'
        $Outcome = Get-JsonProperty -Object $Sample -Name 'outcome'
        $SourceFormat = Get-JsonProperty -Object $Sample -Name 'sourceFormat'
        $SourceDigest = Get-JsonProperty -Object $Sample -Name 'sourceDigest'
        if ($null -eq $OwnerName -or !$OwnersByName.ContainsKey($OwnerName.ToString()) -or
            $null -eq $SampleHost -or
            $SampleHost.ToString() -cnotmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$' -or
            $null -eq $ObservedUtc -or
            $Outcome -cnotin @('passed', 'test-failed', 'timed-out', 'framework-error') -or
            $SourceFormat -cnotin @(
                'windvale-verification-run-result-1',
                'windvale-native-changed-verification-timing-2') -or
            $null -eq $SourceDigest -or
            $SourceDigest.ToString() -cnotmatch '^[0-9a-f]{64}$') {
            throw 'The verification timing history contains a malformed sample.'
        }
        $Elapsed = Convert-BoundedInteger `
            -Value (Get-JsonProperty -Object $Sample -Name 'elapsedMilliseconds') `
            -Minimum 0 -Maximum $MAXIMUM_ELAPSED_MILLISECONDS `
            -Label 'Historical elapsed milliseconds'
        $Observed = Convert-ObservedUtc -Value $ObservedUtc `
            -Fallback ([DateTime]::MinValue)
        $AllSamples.Add([pscustomobject]@{
            owner = $OwnerName.ToString()
            host = $SampleHost.ToString()
            observedUtc = $Observed
            elapsedMilliseconds = $Elapsed
            outcome = $Outcome.ToString()
            sourceFormat = $SourceFormat.ToString()
            sourceDigest = $SourceDigest.ToString()
        })
    }
}

$ExistingKeys = [Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
foreach ($Sample in $AllSamples) {
    $null = $ExistingKeys.Add(
        "$($Sample.sourceDigest)|$($Sample.owner)|$($Sample.host)")
}
$ReportsAccepted = 0
$ReportsSkipped = 0
$SamplesAdded = 0
foreach ($File in $InputFiles) {
    $Report = Read-BoundedJson -Path $File.FullName `
        -MaximumBytes $MAXIMUM_INPUT_FILE_BYTES -Label 'Verification timing input'
    $FormatValue = Get-JsonProperty -Object $Report -Name 'format'
    $Format = if ($null -eq $FormatValue) { '' } else { $FormatValue.ToString() }
    if ($Format -notin @(
            'windvale-verification-run-result-1',
            'windvale-native-changed-verification-timing-2')) {
        $ReportsSkipped += 1
        continue
    }
    $ReportsAccepted += 1
    $Digest = (Get-FileHash -LiteralPath $File.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    $ReportHost = Resolve-HostName `
        -RecordHost (Get-JsonProperty -Object $Report -Name 'host')
    $ObservedUtc = Convert-ObservedUtc `
        -Value (Get-JsonProperty -Object $Report -Name 'startedUtc') `
        -Fallback $File.LastWriteTimeUtc
    $Records = if ($Format -eq 'windvale-verification-run-result-1') {
        $Value = Get-JsonProperty -Object $Report -Name 'owners'
        if ($null -eq $Value) {
            throw 'A verification run result has no owner records.'
        }
        @($Value)
    } else {
        $Value = Get-JsonProperty -Object $Report -Name 'entries'
        if ($null -eq $Value) {
            throw 'A changed-verification timing result has no entries.'
        }
        @($Value | Where-Object {
            (Get-JsonProperty -Object $_ -Name 'status') -eq 'executed'
        })
    }
    foreach ($Record in $Records) {
        $OwnerValue = Get-JsonProperty -Object $Record -Name 'name'
        if ($null -eq $OwnerValue -or
            !$OwnersByName.ContainsKey($OwnerValue.ToString())) {
            continue
        }
        $OutcomeValue = Get-JsonProperty -Object $Record -Name 'outcome'
        if ($null -eq $OutcomeValue -or
            $OutcomeValue.ToString() -notin @(
                'passed', 'test-failed', 'timed-out', 'framework-error')) {
            throw 'A verification timing owner has an invalid outcome.'
        }
        $Elapsed = Convert-BoundedInteger `
            -Value (Get-JsonProperty -Object $Record -Name 'elapsedMilliseconds') `
            -Minimum 0 -Maximum $MAXIMUM_ELAPSED_MILLISECONDS `
            -Label 'Observed elapsed milliseconds'
        $Key = "$Digest|$($OwnerValue.ToString())|$ReportHost"
        if (!$ExistingKeys.Add($Key)) { continue }
        $AllSamples.Add([pscustomobject]@{
            owner = $OwnerValue.ToString()
            host = $ReportHost
            observedUtc = $ObservedUtc
            elapsedMilliseconds = $Elapsed
            outcome = $OutcomeValue.ToString()
            sourceFormat = $Format
            sourceDigest = $Digest
        })
        $SamplesAdded += 1
    }
}
if ($ReportsAccepted -eq 0) {
    throw 'No recognized verification timing reports were found.'
}

$RetainedSamples = [Collections.Generic.List[object]]::new()
foreach ($Group in @($AllSamples | Group-Object {
            "$($_.owner)`n$($_.host)"
        })) {
    foreach ($Sample in @($Group.Group |
            Sort-Object observedUtc, sourceDigest -Descending |
            Select-Object -First $MaximumSamplesPerOwnerHost)) {
        $RetainedSamples.Add($Sample)
    }
}
$SortedSamples = @($RetainedSamples |
    Sort-Object owner, host, observedUtc, sourceDigest)

$Recommendations = [Collections.Generic.List[object]]::new()
foreach ($OwnerName in @($OwnersByName.Keys | Sort-Object)) {
    $Owner = $OwnersByName[$OwnerName]
    $CurrentProfile = $ProfilesByName[$Owner.profile]
    $OwnerSamples = @($SortedSamples | Where-Object { $_.owner -ceq $OwnerName })
    $Passing = @($OwnerSamples | Where-Object { $_.outcome -eq 'passed' })
    $WindowsPassing = @($Passing | Where-Object {
        (Get-HostFamily $_.host) -eq 'windows'
    }).Count
    $LinuxPassing = @($Passing | Where-Object {
        (Get-HostFamily $_.host) -eq 'linux'
    }).Count
    $Timeouts = @($OwnerSamples | Where-Object { $_.outcome -eq 'timed-out' }).Count
    $SortedElapsed = @($Passing | ForEach-Object {
        $_.elapsedMilliseconds
    } | Sort-Object)
    $P50 = $null
    $P95 = $null
    $ObservedMaximum = $null
    if ($SortedElapsed.Count -ne 0) {
        $P50 = $SortedElapsed[[Math]::Ceiling($SortedElapsed.Count * 0.50) - 1]
        $P95 = $SortedElapsed[[Math]::Ceiling($SortedElapsed.Count * 0.95) - 1]
        $ObservedMaximum = $SortedElapsed[-1]
    }
    $Action = 'collect-more'
    $RecommendedProfile = $CurrentProfile.name
    $Reason = 'Requires representative passing samples from both Windows and Linux.'
    $HasDualHostEvidence = (
        $WindowsPassing -ge $MinimumSamplesPerHost -and
        $LinuxPassing -ge $MinimumSamplesPerHost)
    if ($Timeouts -ne 0) {
        $Action = 'review-timeout'
        $Reason = 'At least one retained owner run reached its enforced deadline.'
    } elseif ($HasDualHostEvidence) {
        $ExpectedWithMargin = [Math]::Ceiling(($P95 / 1000.0) * 1.5)
        $MaximumWithMargin = [Math]::Ceiling(($ObservedMaximum / 1000.0) * 1.25)
        if ($ExpectedWithMargin -gt $CurrentProfile.expectedSeconds -or
            $MaximumWithMargin -gt $CurrentProfile.maximumSeconds) {
            $Candidate = @($Profiles | Where-Object {
                $_.expectedSeconds -ge $ExpectedWithMargin -and
                $_.maximumSeconds -ge $MaximumWithMargin
            } | Select-Object -First 1)
            if ($Candidate.Count -eq 0) {
                $Action = 'review-over-profile'
                $Reason = 'Observed duration plus margin exceeds every registered profile.'
            } else {
                $RecommendedProfile = $Candidate[0].name
                $Action = if ($Candidate[0].index -gt $CurrentProfile.index) {
                    'promote'
                } else {
                    'retain'
                }
                $Reason = 'Recommendation preserves 50% expected and 25% maximum margins.'
            }
        } elseif ($CurrentProfile.index -gt 0) {
            $LowerProfile = $Profiles[$CurrentProfile.index - 1]
            if ($ExpectedWithMargin -le $LowerProfile.expectedSeconds -and
                $MaximumWithMargin -le $LowerProfile.maximumSeconds) {
                $Action = 'downgrade'
                $RecommendedProfile = $LowerProfile.name
                $Reason = 'Both hosts fit the next smaller profile with conservative margins.'
            } else {
                $Action = 'retain'
                $Reason = 'Observed duration fits the current profile but not the next smaller one.'
            }
        } else {
            $Action = 'retain'
            $Reason = 'Observed duration fits the smallest registered profile.'
        }
    }
    $Recommendations.Add([pscustomobject]@{
        owner = $OwnerName
        currentProfile = $CurrentProfile.name
        action = $Action
        recommendedProfile = $RecommendedProfile
        windowsPassingSamples = $WindowsPassing
        linuxPassingSamples = $LinuxPassing
        otherPassingSamples = $Passing.Count - $WindowsPassing - $LinuxPassing
        timeoutSamples = $Timeouts
        p50Milliseconds = $P50
        p95Milliseconds = $P95
        maximumMilliseconds = $ObservedMaximum
        reason = $Reason
    })
}

$HistoryOutput = [ordered]@{
    format = 'windvale-verification-timing-history-1'
    updatedUtc = [DateTime]::UtcNow.ToString('O')
    maximumSamplesPerOwnerHost = $MaximumSamplesPerOwnerHost
    samples = $SortedSamples
}
$Actionable = @($Recommendations | Where-Object {
    $_.action -in @('promote', 'downgrade', 'review-timeout', 'review-over-profile')
})
$AnalysisOutput = [ordered]@{
    format = 'windvale-verification-timing-analysis-1'
    generatedUtc = [DateTime]::UtcNow.ToString('O')
    minimumSamplesPerHost = $MinimumSamplesPerHost
    maximumSamplesPerOwnerHost = $MaximumSamplesPerOwnerHost
    reportsAccepted = $ReportsAccepted
    reportsSkipped = $ReportsSkipped
    samplesAdded = $SamplesAdded
    samplesRetained = $SortedSamples.Count
    actionableRecommendations = $Actionable.Count
    owners = @($Recommendations)
}

Write-BoundedJson -Path $FullHistoryPath -Value $HistoryOutput `
    -MaximumBytes $MAXIMUM_HISTORY_BYTES -Label 'Verification timing history'
if ($null -ne $FullAnalysisPath) {
    Write-BoundedJson -Path $FullAnalysisPath -Value $AnalysisOutput `
        -MaximumBytes $MAXIMUM_ANALYSIS_BYTES -Label 'Verification timing analysis'
}

Write-Host (
    "verification timing history status=Updated reports=$ReportsAccepted " +
    "skipped=$ReportsSkipped added=$SamplesAdded retained=$($SortedSamples.Count)")
foreach ($Recommendation in @($Recommendations | Where-Object {
            $_.windowsPassingSamples -ne 0 -or $_.linuxPassingSamples -ne 0 -or
            $_.otherPassingSamples -ne 0 -or $_.timeoutSamples -ne 0
        })) {
    $P95Text = if ($null -eq $Recommendation.p95Milliseconds) {
        'none'
    } else {
        $Recommendation.p95Milliseconds.ToString(
            [Globalization.CultureInfo]::InvariantCulture)
    }
    Write-Host (
        "TIMING owner=$($Recommendation.owner) " +
        "current=$($Recommendation.currentProfile) action=$($Recommendation.action) " +
        "recommended=$($Recommendation.recommendedProfile) " +
        "windows=$($Recommendation.windowsPassingSamples) " +
        "linux=$($Recommendation.linuxPassingSamples) " +
        "timeouts=$($Recommendation.timeoutSamples) p95-ms=$P95Text")
}
Write-Host (
    "verification timing analysis status=Passed owners=$($Recommendations.Count) " +
    "actionable=$($Actionable.Count)")
