[CmdletBinding()]
param(
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$InventoryPath = Join-Path $RepositoryRoot 'Documents/Project/Dotnet-Retirement-Inventory.json'
$AllowedLanes = @('development', 'verification', 'release', 'recovery')
$AllowedModes = @('normal', 'recovery')

function Test-DirectManagedInvocation {
    param(
        [Parameter(Mandatory)]
        [string]$Content
    )

    return (
        $Content -match '(?im)actions/setup-dotnet@' -or
        $Content -match '(?im)\bGet-Command\s+dotnet\b' -or
        $Content -match '(?im)(^|[\s;&|($=])dotnet(?:\.exe)?\s+'
    )
}

if (!(Test-Path -LiteralPath $InventoryPath -PathType Leaf)) {
    throw "The .NET retirement inventory is missing: $InventoryPath"
}

$Inventory = Get-Content -Raw -LiteralPath $InventoryPath | ConvertFrom-Json
if ($Inventory.format -ne 'windvale-dotnet-retirement-inventory-1') {
    throw "Unsupported .NET retirement inventory format '$($Inventory.format)'."
}

$Entries = @($Inventory.directManagedEntrypoints)
if ($Entries.Count -eq 0) {
    throw 'The .NET retirement inventory must contain at least one direct managed entry point.'
}

$ExpectedPaths = [System.Collections.Generic.List[string]]::new()
$SeenPaths = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::Ordinal)
$ModeCounts = @{
    normal = 0
    recovery = 0
}

foreach ($Entry in $Entries) {
    $Path = [string]$Entry.path
    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw 'A .NET retirement inventory entry has no path.'
    }

    if ($Path -ne $Path.Replace('\', '/')) {
        throw "Inventory path '$Path' must use forward slashes."
    }

    if ([System.IO.Path]::IsPathRooted($Path) -or $Path -match '(^|/)\.\.(/|$)') {
        throw "Inventory path '$Path' must stay relative to the repository root."
    }

    if (!$SeenPaths.Add($Path)) {
        throw "Duplicate .NET retirement inventory path '$Path'."
    }

    $Owner = [string]$Entry.owner
    if ([string]::IsNullOrWhiteSpace($Owner)) {
        throw "Inventory path '$Path' has no owner."
    }

    $Mode = [string]$Entry.mode
    if ($Mode -notin $AllowedModes) {
        throw "Inventory path '$Path' has unsupported mode '$Mode'."
    }

    $Lanes = @($Entry.lanes)
    if ($Lanes.Count -eq 0) {
        throw "Inventory path '$Path' has no lane."
    }

    $SeenLanes = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)
    foreach ($LaneValue in $Lanes) {
        $Lane = [string]$LaneValue
        if ($Lane -notin $AllowedLanes) {
            throw "Inventory path '$Path' has unsupported lane '$Lane'."
        }

        if (!$SeenLanes.Add($Lane)) {
            throw "Inventory path '$Path' repeats lane '$Lane'."
        }
    }

    if ($Mode -eq 'recovery' -and ($Lanes.Count -ne 1 -or $Lanes[0] -ne 'recovery')) {
        throw "Recovery inventory path '$Path' must own only the recovery lane."
    }
    if ($Mode -eq 'normal' -and 'recovery' -in $Lanes) {
        throw "Normal inventory path '$Path' cannot own the recovery lane."
    }

    $FullPath = Join-Path $RepositoryRoot $Path
    if (!(Test-Path -LiteralPath $FullPath -PathType Leaf)) {
        throw "Inventory path '$Path' does not exist."
    }

    $Content = Get-Content -Raw -LiteralPath $FullPath
    if (!(Test-DirectManagedInvocation -Content $Content)) {
        throw "Inventory path '$Path' no longer contains a direct .NET invocation."
    }

    $ExpectedPaths.Add($Path)
    $ModeCounts[$Mode]++
}

$CandidateFiles = @(
    Get-ChildItem -LiteralPath (Join-Path $RepositoryRoot '.github/workflows') -File |
        Where-Object { $_.Extension -in @('.yml', '.yaml') }
    Get-ChildItem -LiteralPath (Join-Path $RepositoryRoot 'Tools/Verify') -File |
        Where-Object { $_.Extension -in @('.ps1', '.sh', '.cmd') }
    Get-ChildItem -LiteralPath (Join-Path $RepositoryRoot 'Tools/Recovery') -File |
        Where-Object { $_.Extension -in @('.ps1', '.sh', '.cmd') }
    Get-Item -LiteralPath (Join-Path $RepositoryRoot 'Website/package.json')
)

$DiscoveredPaths = @(
    foreach ($File in $CandidateFiles) {
        if ($File.FullName -eq $PSCommandPath) {
            continue
        }

        $Content = Get-Content -Raw -LiteralPath $File.FullName
        if (Test-DirectManagedInvocation -Content $Content) {
            [System.IO.Path]::GetRelativePath($RepositoryRoot, $File.FullName).Replace('\', '/')
        }
    }
) | Sort-Object -Unique

$ExpectedPaths = @($ExpectedPaths) | Sort-Object
$UntrackedPaths = @($DiscoveredPaths | Where-Object { $_ -notin $ExpectedPaths })
$StalePaths = @($ExpectedPaths | Where-Object { $_ -notin $DiscoveredPaths })

if ($UntrackedPaths.Count -ne 0 -or $StalePaths.Count -ne 0) {
    $Details = @()
    if ($UntrackedPaths.Count -ne 0) {
        $Details += "untracked direct entry points: $($UntrackedPaths -join ', ')"
    }
    if ($StalePaths.Count -ne 0) {
        $Details += "stale inventory entries: $($StalePaths -join ', ')"
    }
    throw "The .NET retirement inventory is out of date ($($Details -join '; '))."
}

if (!$Quiet) {
    Write-Host (
        ".NET retirement inventory passed ($($ExpectedPaths.Count) direct entry points; " +
        "$($ModeCounts.normal) normal, $($ModeCounts.recovery) recovery).")
}
