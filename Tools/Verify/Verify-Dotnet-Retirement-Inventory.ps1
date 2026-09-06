[CmdletBinding()]
param(
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$InventoryPath = Join-Path $RepositoryRoot 'Documents/Project/Dotnet-Retirement-Inventory.json'

function Test-DirectManagedInvocation {
    param(
        [Parameter(Mandatory)]
        [string]$Content
    )

    return (
        $Content -match '(?im)actions/setup-dotnet@' -or
        $Content -match '(?im)\bGet-Command\s+dotnet\b' -or
        $Content -match '(?im)(^|[\s;&|($=''"])dotnet(?:\.exe)?\s+'
    )
}

if (!(Test-Path -LiteralPath $InventoryPath -PathType Leaf)) {
    throw "The .NET archival inventory is missing: $InventoryPath"
}

$Inventory = Get-Content -Raw -LiteralPath $InventoryPath | ConvertFrom-Json
if ($Inventory.format -ne 'windvale-dotnet-retirement-inventory-2') {
    throw "Unsupported .NET archival inventory format '$($Inventory.format)'."
}
if ([int]$Inventory.trackedManagedFiles -ne 0) {
    throw 'The .NET archival inventory must declare zero tracked managed files.'
}
if (@($Inventory.directManagedEntrypoints).Count -ne 0) {
    throw 'The .NET archival inventory must declare zero direct managed entry points.'
}

$ExpectedArchive = [ordered]@{
    tag = 'stage0-recovery-e5a1a7473c57'
    releaseUrl = 'https://github.com/eworker-inc/Windvale/releases/tag/stage0-recovery-e5a1a7473c57'
    commit = 'e5a1a7473c57935c5dfcf09b78b18c3c099e70ef'
    tree = '9950150f14cd4864b06c853ab6a716fa6e04495a'
    sourceBundleSha256 = '1830bf95b583267b69229125edb83521733a36f27a4d49fe371534734bcc0892'
    supplementalChecksumsSha256 = 'de18793e13fa4cf429070739708e2e3bebc4cebbd5eacde5832dca9781928267'
    restoreDocument = 'Bootstrap/Stage0/README.md'
}
foreach ($Name in $ExpectedArchive.Keys) {
    if ([string]$Inventory.archive.$Name -cne $ExpectedArchive[$Name]) {
        throw "The Stage 0 archive field '$Name' differs from the qualified identity."
    }
}

$RestorePath = Join-Path $RepositoryRoot ([string]$Inventory.archive.restoreDocument)
if (!(Test-Path -LiteralPath $RestorePath -PathType Leaf)) {
    throw "The Stage 0 recovery pointer is missing: $RestorePath"
}

$TrackedPaths = @(git -C $RepositoryRoot ls-files --cached --others --exclude-standard)
if ($LASTEXITCODE -ne 0) {
    throw 'Git could not enumerate the repository inventory.'
}
$ManagedExtensions = @('.cs', '.csproj', '.fs', '.fsproj', '.vb', '.vbproj', '.razor', '.sln', '.slnx')
$ManagedBuildFiles = @(
    'Directory.Build.props',
    'Directory.Build.targets',
    'Directory.Packages.props',
    'global.json',
    'NuGet.Config',
    'packages.lock.json'
)
$TrackedManagedPaths = @(
    $TrackedPaths | Where-Object {
        ([IO.Path]::GetExtension($_) -in $ManagedExtensions -or
            [IO.Path]::GetFileName($_) -in $ManagedBuildFiles) -and
        (Test-Path -LiteralPath (Join-Path $RepositoryRoot $_) -PathType Leaf)
    }
)
if ($TrackedManagedPaths.Count -ne 0) {
    throw "Managed source or build metadata returned to main: $($TrackedManagedPaths -join ', ')"
}

$OperationalPaths = @(
    $TrackedPaths | Where-Object {
        $_ -ne 'Tools/Verify/Verify-Dotnet-Retirement-Inventory.ps1' -and
        (
            $_.StartsWith('.github/workflows/', [StringComparison]::Ordinal) -or
            $_.StartsWith('Tools/Verify/', [StringComparison]::Ordinal) -or
            $_.StartsWith('Tools/Recovery/', [StringComparison]::Ordinal) -or
            [IO.Path]::GetFileName($_) -eq 'package.json'
        )
    }
)
$DirectManagedPaths = @(
    foreach ($Path in $OperationalPaths) {
        $FullPath = Join-Path $RepositoryRoot $Path
        if (!(Test-Path -LiteralPath $FullPath -PathType Leaf)) {
            continue
        }
        $Content = Get-Content -Raw -LiteralPath $FullPath
        if (Test-DirectManagedInvocation -Content $Content) {
            $Path
        }
    }
)
if ($DirectManagedPaths.Count -ne 0) {
    throw "A direct managed invocation returned to an operational path: $($DirectManagedPaths -join ', ')"
}

if (!$Quiet) {
    Write-Host (
        ".NET archival inventory passed (0 tracked managed files; " +
        "0 direct managed entry points; archive $($Inventory.archive.tag)).")
}
