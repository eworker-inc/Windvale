[CmdletBinding()]
param([switch]$Quiet)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$DeclarationPath = Join-Path $RepositoryRoot 'Tests/Native/Development-Owner-Dependencies.txt'
$Planner = Join-Path $PSScriptRoot 'Get-Native-Changed-Verification-Plan.ps1'
$Owners = @(
    'database-storage',
    'os-x64-code-emission',
    'seed-native-front-door',
    'webassembly-engine'
)
$RequiredKinds = @('artifact', 'producer', 'source')

$Bytes = [IO.File]::ReadAllBytes($DeclarationPath)
if ($Bytes.Count -eq 0 -or $Bytes[-1] -ne 10 -or $Bytes -contains 13) {
    throw 'The native development dependency declaration must be LF text with one trailing newline.'
}
try {
    $Text = [Text.UTF8Encoding]::new($false, $true).GetString($Bytes)
} catch {
    throw 'The native development dependency declaration is not valid UTF-8.'
}
$Lines = @($Text.Substring(0, $Text.Length - 1).Split("`n"))
if ($Lines.Count -lt 2 -or
    $Lines[0] -ne 'windvale-native-development-owner-dependencies 1') {
    throw 'The native development dependency declaration header differs.'
}

$EntryLines = @($Lines | Select-Object -Skip 1)
$SortedLines = [string[]]$EntryLines.Clone()
[Array]::Sort($SortedLines, [StringComparer]::Ordinal)
if (![Linq.Enumerable]::SequenceEqual([string[]]$EntryLines, $SortedLines)) {
    throw 'The native development dependency declarations are not in ordinal order.'
}
$UniqueLines = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$Entries = @(
    foreach ($Line in $EntryLines) {
        if (!$UniqueLines.Add($Line)) {
            throw "Duplicate native development dependency declaration: $Line"
        }
        $Fields = $Line -split '\|', 3
        if ($Fields.Count -ne 3 -or $Fields[0] -notin $Owners -or
            $Fields[1] -notin @('artifact', 'checkpoint', 'producer', 'source')) {
            throw "Malformed native development dependency declaration: $Line"
        }
        if ($Fields[1] -eq 'checkpoint') {
            if ($Fields[2] -notmatch '^[a-z0-9][a-z0-9-]{0,63}$') {
                throw "Invalid native development checkpoint family: $Line"
            }
        } else {
            if ($Fields[2] -notmatch '^[A-Za-z0-9][A-Za-z0-9._/-]*$' -or
                $Fields[2].Split('/') -contains '..') {
                throw "Invalid native development dependency path: $Line"
            }
            $AbsolutePath = Join-Path $RepositoryRoot $Fields[2]
            if (!(Test-Path -LiteralPath $AbsolutePath -PathType Leaf)) {
                throw "Missing native development dependency: $($Fields[2])"
            }
            $Item = Get-Item -LiteralPath $AbsolutePath -Force
            if (($Item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Linked native development dependency is forbidden: $($Fields[2])"
            }
        }
        [pscustomobject]@{ Owner = $Fields[0]; Kind = $Fields[1]; Value = $Fields[2] }
    }
)

foreach ($Owner in $Owners) {
    $OwnerEntries = @($Entries | Where-Object Owner -eq $Owner)
    foreach ($Kind in $RequiredKinds) {
        if (!($OwnerEntries | Where-Object Kind -eq $Kind)) {
            throw "Native development owner '$Owner' has no declared $Kind closure."
        }
    }
    $Paths = @($OwnerEntries | Where-Object Kind -ne 'checkpoint' | ForEach-Object Value)
    $Plan = & $Planner -ChangedPath $Paths -PassThru -Quiet
    if ($Plan.Gaps.Count -ne 0) {
        throw "Native development owner '$Owner' has planner gaps: $($Plan.Gaps -join ', ')"
    }
    $OwnerSelected = switch ($Owner) {
        'database-storage' { $Plan.Suites -contains 'database-storage' }
        'os-x64-code-emission' { $Plan.Suites -contains 'os-x64-code-emission' }
        'seed-native-front-door' { $Plan.Suites -contains 'seed-native-front-door' }
        'webassembly-engine' { $Plan.RunWebAssemblyEngineVerification }
    }
    if (!$OwnerSelected) {
        throw "Native development dependency closure does not select owner '$Owner'."
    }
}

$DatabaseCheckpoints = @(
    $Entries |
        Where-Object { $_.Owner -eq 'database-storage' -and $_.Kind -eq 'checkpoint' } |
        ForEach-Object Value
)
$ExpectedCheckpoints = @(
    'build-driver-v1',
    'hosted-application-v1',
    'linked-image-v1',
    'project-object-v2',
    'project-wvb-v2'
)
if (![Linq.Enumerable]::SequenceEqual(
        [string[]]$DatabaseCheckpoints, [string[]]$ExpectedCheckpoints)) {
    throw 'The database development checkpoint-family closure differs.'
}

$OsX64Checkpoints = @(
    $Entries |
        Where-Object {
            $_.Owner -eq 'os-x64-code-emission' -and $_.Kind -eq 'checkpoint'
        } |
        ForEach-Object Value
)
if (![Linq.Enumerable]::SequenceEqual(
        [string[]]$OsX64Checkpoints, [string[]]@('project-wvb-v2'))) {
    throw 'The OS x64 development checkpoint-family closure differs.'
}

if (!$Quiet) {
    Write-Host (
        "Native development dependency closures passed " +
        "($($Owners.Count) owners, $($Entries.Count) declarations).")
}
