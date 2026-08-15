param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string] $Bundle,

    [Parameter(Mandatory = $true, Position = 1)]
    [ValidatePattern('^[0-9a-f]{64}$')]
    [string] $ExpectedSha256,

    [Parameter(Mandatory = $true, Position = 2)]
    [string] $StoreRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-Sha256([byte[]] $Bytes) {
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function Assert-OrdinaryPath([string] $Path, [string] $Description) {
    $item = Get-Item -LiteralPath $Path -Force
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "$Description must not be a reparse point: $Path"
    }
}

function Ensure-Directory([string] $Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        [IO.Directory]::CreateDirectory($Path) | Out-Null
    }
    Assert-OrdinaryPath $Path 'Package-store directory'
}

function Publish-Immutable(
    [byte[]] $Bytes,
    [string] $Digest,
    [string] $Destination,
    [ref] $Created,
    [ref] $Existing
) {
    if (Test-Path -LiteralPath $Destination) {
        Assert-OrdinaryPath $Destination 'Existing package-store object'
        $observed = [IO.File]::ReadAllBytes($Destination)
        if ((Get-Sha256 $observed) -ne $Digest -or $observed.Length -ne $Bytes.Length) {
            throw "Package-store corruption at immutable identity $Digest"
        }
        $Existing.Value++
        return
    }

    $parent = Split-Path -Parent $Destination
    Ensure-Directory $parent
    $candidate = Join-Path $parent ('.new-' + $Digest + '-' + [Guid]::NewGuid().ToString('N'))
    try {
        $stream = [IO.FileStream]::new(
            $candidate,
            [IO.FileMode]::CreateNew,
            [IO.FileAccess]::Write,
            [IO.FileShare]::None,
            65536,
            [IO.FileOptions]::WriteThrough
        )
        try {
            $stream.Write($Bytes, 0, $Bytes.Length)
            $stream.Flush($true)
        } finally {
            $stream.Dispose()
        }
        $reread = [IO.File]::ReadAllBytes($candidate)
        if ($reread.Length -ne $Bytes.Length -or (Get-Sha256 $reread) -ne $Digest) {
            throw "Private package-store candidate failed reread for $Digest"
        }
        try {
            [IO.File]::Move($candidate, $Destination)
            $Created.Value++
        } catch [IO.IOException] {
            if (-not (Test-Path -LiteralPath $Destination)) { throw }
            $observed = [IO.File]::ReadAllBytes($Destination)
            if ($observed.Length -ne $Bytes.Length -or (Get-Sha256 $observed) -ne $Digest) {
                throw "Package-store publication race exposed corruption for $Digest"
            }
            $Existing.Value++
        }
    } finally {
        if (Test-Path -LiteralPath $candidate) {
            Remove-Item -LiteralPath $candidate -Force
        }
    }
}

$bundlePath = (Resolve-Path -LiteralPath $Bundle).Path
Assert-OrdinaryPath $bundlePath 'Admitted bundle'
$bundleBytes = [IO.File]::ReadAllBytes($bundlePath)
if ($bundleBytes.Length -lt 128 -or $bundleBytes.Length -gt 4194304) {
    throw 'The admitted in-memory Bundle 1 candidate is outside the 128-byte through 4-MiB policy.'
}
$bundleDigest = Get-Sha256 $bundleBytes
if ($bundleDigest -ne $ExpectedSha256) {
    throw "Admitted bundle identity mismatch: expected $ExpectedSha256, observed $bundleDigest"
}
Write-Output "package store step=admission-recheck bundle=$bundleDigest bytes=$($bundleBytes.Length)"

if ([Text.Encoding]::ASCII.GetString($bundleBytes, 0, 4) -ne 'WVPB') {
    throw 'The admitted bundle magic is invalid.'
}
$indexBytes = [BitConverter]::ToUInt64($bundleBytes, 32)
$contentOffset = [BitConverter]::ToUInt64($bundleBytes, 40)
$contentBytes = [BitConverter]::ToUInt64($bundleBytes, 48)
$blobCount = [BitConverter]::ToUInt32($bundleBytes, 56)
if ($indexBytes -eq 0 -or $indexBytes -gt 1048576 -or
    $contentOffset -ne 128 + $indexBytes -or
    $contentBytes -ne $bundleBytes.Length - $contentOffset) {
    throw 'The admitted bundle geometry changed before publication.'
}
$index = [Text.Encoding]::UTF8.GetString($bundleBytes, 128, [int] $indexBytes)
$blobLines = @($index.Split("`n", [StringSplitOptions]::RemoveEmptyEntries) |
    Where-Object { $_.StartsWith('blob ', [StringComparison]::Ordinal) })
if ($blobLines.Count -ne $blobCount) {
    throw 'The admitted bundle blob count changed before publication.'
}

$rootPath = [IO.Path]::GetFullPath($StoreRoot)
Ensure-Directory $rootPath
$objectsRoot = Join-Path $rootPath 'objects\sha256'
$bundlesRoot = Join-Path $rootPath 'bundles\sha256'
Ensure-Directory $objectsRoot
Ensure-Directory $bundlesRoot
$created = 0
$existing = 0
$ordinal = 0
foreach ($line in $blobLines) {
    $match = [regex]::Match($line, '^blob ([0-9a-f]{64}) ([0-9]+) ([0-9]+)$')
    if (-not $match.Success) { throw 'The admitted bundle index changed before publication.' }
    $digest = $match.Groups[1].Value
    $length = [UInt64]::Parse($match.Groups[2].Value, [Globalization.CultureInfo]::InvariantCulture)
    $offset = [UInt64]::Parse($match.Groups[3].Value, [Globalization.CultureInfo]::InvariantCulture)
    if ($length -gt [UInt32]::MaxValue -or $offset -gt [UInt32]::MaxValue -or
        $offset + $length -gt $contentBytes) {
        throw 'The admitted bundle blob geometry changed before publication.'
    }
    $value = [byte[]]::new([int] $length)
    [Array]::Copy($bundleBytes, [int] ($contentOffset + $offset), $value, 0, [int] $length)
    if ((Get-Sha256 $value) -ne $digest) {
        throw "The admitted blob identity changed before publication: $digest"
    }
    $ordinal++
    Write-Output "package store step=publish-object object=$ordinal/$blobCount sha256=$digest bytes=$length"
    $fanout = Join-Path $objectsRoot $digest.Substring(0, 2)
    Ensure-Directory $fanout
    Publish-Immutable $value $digest (Join-Path $fanout $digest.Substring(2)) ([ref] $created) ([ref] $existing)
}

Write-Output "package store step=publish-bundle sha256=$bundleDigest"
$bundleFanout = Join-Path $bundlesRoot $bundleDigest.Substring(0, 2)
Ensure-Directory $bundleFanout
$bundleDestination = Join-Path $bundleFanout ($bundleDigest.Substring(2) + '.wvbundle')
Publish-Immutable $bundleBytes $bundleDigest $bundleDestination ([ref] $created) ([ref] $existing)
Write-Output "package store status=Published bundle=$bundleDigest objects=$blobCount created=$created existing=$existing"
