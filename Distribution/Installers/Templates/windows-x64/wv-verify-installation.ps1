[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Root,
    [string]$ExpectedTarget = 'windows-x64',
    [string]$ExpectedPayload
)

$ErrorActionPreference = 'Stop'

function Get-Sha256 {
    param([Parameter(Mandatory)][string]$Path)

    $Stream = [IO.File]::OpenRead($Path)
    try {
        $Hasher = [Security.Cryptography.SHA256]::Create()
        try {
            return ([BitConverter]::ToString($Hasher.ComputeHash($Stream))).Replace('-', '').ToLowerInvariant()
        } finally {
            $Hasher.Dispose()
        }
    } finally {
        $Stream.Dispose()
    }
}

$ResolvedRoot = [IO.Path]::GetFullPath($Root)
$ManifestPath = Join-Path $ResolvedRoot 'Payload-Manifest.txt'
if (!(Test-Path -LiteralPath $ManifestPath -PathType Leaf)) {
    throw 'The payload manifest is missing.'
}

$ObservedPayload = Get-Sha256 -Path $ManifestPath
if ($ExpectedPayload -and $ObservedPayload -ne $ExpectedPayload) {
    throw 'The payload manifest identity differs from the installer.'
}

$Lines = [IO.File]::ReadAllLines($ManifestPath)
if ($Lines.Count -lt 4 -or $Lines[0] -ne 'windvale-installer-payload 1') {
    throw 'The payload manifest header is invalid.'
}
if ($Lines[1] -notmatch '^version ([0-9]+\.[0-9]+\.[0-9]+-dev\.[0-9]+)$') {
    throw 'The payload version record is invalid.'
}
if ($Lines[2] -ne "target $ExpectedTarget") {
    throw 'The payload target record is invalid.'
}

$Count = 0
foreach ($Line in $Lines | Select-Object -Skip 3) {
    if ($Line -notmatch '^file ([0-9a-f]{64}) ([0-9]+) (0[0-7]{3}) ([A-Za-z0-9._/-]+)$') {
        throw "Invalid payload file record: $Line"
    }
    $ExpectedSha256 = $Matches[1]
    $ExpectedBytes = [UInt64]::Parse($Matches[2], [Globalization.CultureInfo]::InvariantCulture)
    $RelativePath = $Matches[4]
    if ($RelativePath.StartsWith('/') -or $RelativePath.Split('/') -contains '..') {
        throw 'The payload contains an unsafe path.'
    }
    $NativeRelativePath = $RelativePath.Replace('/', [IO.Path]::DirectorySeparatorChar)
    $FilePath = [IO.Path]::GetFullPath((Join-Path $ResolvedRoot $NativeRelativePath))
    $RequiredPrefix = $ResolvedRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) +
        [IO.Path]::DirectorySeparatorChar
    if (!$FilePath.StartsWith($RequiredPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The payload file escapes its root.'
    }
    $File = Get-Item -LiteralPath $FilePath -Force -ErrorAction Stop
    if (!$File.PSIsContainer -and [UInt64]$File.Length -eq $ExpectedBytes) {
        $ObservedSha256 = Get-Sha256 -Path $FilePath
        if ($ObservedSha256 -eq $ExpectedSha256) {
            $Count++
            continue
        }
    }
    throw "Payload verification failed: $RelativePath"
}

Write-Output "windvale installation status=Verified target=$ExpectedTarget files=$Count payload=$ObservedPayload"
