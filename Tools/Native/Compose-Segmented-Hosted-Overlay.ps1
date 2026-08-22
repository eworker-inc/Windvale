param(
    [Parameter(Mandatory = $true)][string] $ApplicationPrefix,
    [Parameter(Mandatory = $true)][string] $ApplicationManifest,
    [Parameter(Mandatory = $true)][string] $CommonProviderObject,
    [Parameter(Mandatory = $true)][string] $PlatformProviderObject,
    [Parameter(Mandatory = $true)][string] $OutputPrefix
)

$ErrorActionPreference = 'Stop'
$maximumChunkBytes = 4MB
$maximumImageBytes = 64MB
$maximumChunkCount = 16

function Read-U32Little([byte[]] $Value, [int] $Offset) {
    return [uint32](([uint32]$Value[$Offset]) -bor
        (([uint32]$Value[$Offset + 1]) -shl 8) -bor
        (([uint32]$Value[$Offset + 2]) -shl 16) -bor
        (([uint32]$Value[$Offset + 3]) -shl 24))
}

function Resolve-Prefix([string] $Value) {
    $parent = Split-Path -Parent $Value
    if ([string]::IsNullOrEmpty($parent)) { $parent = '.' }
    $resolvedParent = (Resolve-Path -LiteralPath $parent).Path
    return Join-Path $resolvedParent (Split-Path -Leaf $Value)
}

if (-not $ApplicationManifest.EndsWith('.wvli', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'The segmented application manifest must use the .wvli extension.'
}
$manifestPath = (Resolve-Path -LiteralPath $ApplicationManifest).Path
$commonPath = (Resolve-Path -LiteralPath $CommonProviderObject).Path
$platformPath = (Resolve-Path -LiteralPath $PlatformProviderObject).Path
$applicationPrefixPath = Resolve-Prefix $ApplicationPrefix
$outputPrefixPath = Resolve-Prefix $OutputPrefix
$manifest = [IO.File]::ReadAllBytes($manifestPath)
if ($manifest.Length -lt 28 -or
    $manifest[0] -ne 87 -or $manifest[1] -ne 86 -or
    $manifest[2] -ne 76 -or $manifest[3] -ne 73 -or
    $manifest[4] -ne 1 -or $manifest[5] -ne 0 -or
    $manifest[6] -ne 0 -or $manifest[7] -ne 0) {
    throw 'The segmented application manifest identity is invalid.'
}
$applicationBytes = Read-U32Little $manifest 12
$applicationEntry = Read-U32Little $manifest 16
$applicationChunks = Read-U32Little $manifest 20
if ((Read-U32Little $manifest 8) -ne $manifest.Length -or
    $applicationBytes -eq 0 -or $applicationBytes -gt $maximumImageBytes -or
    $applicationEntry -ge $applicationBytes -or
    $applicationChunks -eq 0 -or $applicationChunks -gt $maximumChunkCount -or
    (Read-U32Little $manifest 24) -ne $maximumChunkBytes -or
    $manifest.Length -ne 28 + $applicationChunks * 12) {
    throw 'The segmented application manifest bounds are invalid.'
}

$position = [uint32]0
for ($index = 0; $index -lt $applicationChunks; $index++) {
    $entry = 28 + $index * 12
    $chunkIndex = Read-U32Little $manifest $entry
    $chunkPosition = Read-U32Little $manifest ($entry + 4)
    $chunkBytes = Read-U32Little $manifest ($entry + 8)
    $chunkPath = "$applicationPrefixPath.chunk-$index"
    $item = Get-Item -LiteralPath $chunkPath
    if ($item.PSIsContainer -or
        ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 -or
        $chunkIndex -ne $index -or $chunkPosition -ne $position -or
        $chunkBytes -eq 0 -or $chunkBytes -gt $maximumChunkBytes -or
        $item.Length -ne $chunkBytes -or
        ($index + 1 -lt $applicationChunks -and $chunkBytes -ne $maximumChunkBytes)) {
        throw "The segmented application chunk $index is invalid."
    }
    $position += $chunkBytes
}
if ($position -ne $applicationBytes) {
    throw 'The segmented application chunks do not cover the declared image.'
}
for ($index = 0; $index -lt $maximumChunkCount; $index++) {
    if (Test-Path -LiteralPath "$outputPrefixPath.chunk-$index") {
        throw "The overlay output chunk already exists: $outputPrefixPath.chunk-$index"
    }
}

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$assembler = Join-Path $PSScriptRoot 'Assemble-Wva.cmd'
$linker = Join-Path $PSScriptRoot 'Link-Wvo.cmd'
$trampolineSource = Join-Path $repositoryRoot 'Runtime\Native\X64-Segmented-Hosted-Main-Trampoline.wva'
$outputDirectory = Split-Path -Parent $outputPrefixPath
$temporaryDirectory = Join-Path $outputDirectory ('.windvale-segmented-overlay-' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($temporaryDirectory) | Out-Null

try {
    $trampolineObject = Join-Path $temporaryDirectory 'Main-Trampoline.wvo'
    $assemblerReport = Join-Path $temporaryDirectory 'Assemble.txt'
    & $assembler $trampolineSource $trampolineObject > $assemblerReport
    if ($LASTEXITCODE -ne 0) { throw 'The segmented hosted trampoline assembly failed.' }

    $providerPadding = [uint32]((16 - ($applicationBytes % 16)) % 16)
    $providerStart = [uint32]($applicationBytes + $providerPadding)
    $providerImage = Join-Path $temporaryDirectory 'Provider.bin'
    $providerMap = Join-Path $temporaryDirectory 'Provider.map'
    & $linker $providerStart Storage_host_entry $providerImage $commonPath $platformPath $trampolineObject > $providerMap
    if ($LASTEXITCODE -ne 0) { throw 'The segmented hosted provider link failed.' }

    $mapLines = [IO.File]::ReadAllLines($providerMap)
    $entryMatches = @($mapLines | Select-String '^entry name=Storage_host_entry address=([0-9]+)$')
    $mainMatches = @($mapLines | Select-String '^symbol .* binding=export kind=function name=Main address=([0-9]+) size=5$')
    if ($entryMatches.Count -ne 1 -or $mainMatches.Count -ne 1) {
        throw 'The segmented hosted provider map does not contain one exact entry and trampoline.'
    }
    $providerEntry = [uint32]::Parse($entryMatches[0].Matches[0].Groups[1].Value)
    $trampolineAddress = [uint32]::Parse($mainMatches[0].Matches[0].Groups[1].Value)
    $providerLength = [uint32](Get-Item -LiteralPath $providerImage).Length
    if ($providerLength -eq 0 -or $providerLength -gt $maximumChunkBytes -or
        $providerEntry -lt $providerStart -or $providerEntry -ge $providerStart + $providerLength -or
        $trampolineAddress -lt $providerStart -or
        $trampolineAddress + 5 -gt $providerStart + $providerLength) {
        throw 'The segmented hosted provider layout is invalid.'
    }

    $trampolineOffset = [uint32]($trampolineAddress - $providerStart)
    $providerStream = [IO.File]::Open($providerImage, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    try {
        $providerStream.Position = $trampolineOffset
        $expected = New-Object byte[] 5
        if ($providerStream.Read($expected, 0, 5) -ne 5 -or
            $expected[0] -ne 233 -or $expected[1] -ne 251 -or
            $expected[2] -ne 255 -or $expected[3] -ne 255 -or $expected[4] -ne 255) {
            throw 'The segmented hosted trampoline placeholder is invalid.'
        }
        $displacement = [int64]$applicationEntry - ([int64]$trampolineAddress + 5)
        if ($displacement -lt [int32]::MinValue -or $displacement -gt [int32]::MaxValue) {
            throw 'The segmented hosted trampoline target is out of relative range.'
        }
        $patch = [BitConverter]::GetBytes([int32]$displacement)
        if (-not [BitConverter]::IsLittleEndian) { [Array]::Reverse($patch) }
        $providerStream.Position = $trampolineOffset + 1
        $providerStream.Write($patch, 0, 4)
        $providerStream.Flush($true)
    }
    finally {
        $providerStream.Dispose()
    }

    $paddingBytes = [uint32]($providerStart - $applicationBytes)
    $lastApplicationBytes = Read-U32Little $manifest (28 + ($applicationChunks - 1) * 12 + 8)
    $lastOverlayBytes = [uint64]$lastApplicationBytes + $paddingBytes + $providerLength
    if ($lastOverlayBytes -gt $maximumChunkBytes) {
        throw 'The provider overlay does not fit in the final bounded fragment.'
    }
    if ([uint64]$providerStart + $providerLength -gt $maximumImageBytes) {
        throw 'The segmented hosted overlay exceeds the image limit.'
    }

    for ($index = 0; $index + 1 -lt $applicationChunks; $index++) {
        [IO.File]::Copy(
            "$applicationPrefixPath.chunk-$index",
            (Join-Path $temporaryDirectory "Result.chunk-$index")
        )
    }
    $lastIndex = [int]$applicationChunks - 1
    $lastResult = Join-Path $temporaryDirectory "Result.chunk-$lastIndex"
    $resultStream = [IO.File]::Open($lastResult, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    try {
        $applicationTail = [IO.File]::OpenRead("$applicationPrefixPath.chunk-$lastIndex")
        try { $applicationTail.CopyTo($resultStream) } finally { $applicationTail.Dispose() }
        if ($paddingBytes -gt 0) {
            $resultStream.Write((New-Object byte[] $paddingBytes), 0, $paddingBytes)
        }
        $providerSource = [IO.File]::OpenRead($providerImage)
        try { $providerSource.CopyTo($resultStream) } finally { $providerSource.Dispose() }
        $resultStream.Flush($true)
    }
    finally {
        $resultStream.Dispose()
    }
    if ((Get-Item -LiteralPath $lastResult).Length -ne $lastOverlayBytes) {
        throw 'The segmented hosted overlay final fragment length is invalid.'
    }

    for ($index = 0; $index -lt $applicationChunks; $index++) {
        [IO.File]::Move(
            (Join-Path $temporaryDirectory "Result.chunk-$index"),
            "$outputPrefixPath.chunk-$index"
        )
    }
    Write-Output ("segmented hosted overlay status=Valid application-bytes={0} provider-bytes={1} image-bytes={2} fragments={3} application-entry={4} provider-entry={5} trampoline-address={6} padding-bytes={7}" -f
        $applicationBytes, $providerLength, ($providerStart + $providerLength),
        $applicationChunks, $applicationEntry, $providerEntry,
        $trampolineAddress, $paddingBytes)
}
finally {
    if (Test-Path -LiteralPath $temporaryDirectory) {
        $resolvedTemporary = (Resolve-Path -LiteralPath $temporaryDirectory).Path
        $resolvedOutputDirectory = (Resolve-Path -LiteralPath $outputDirectory).Path
        if ((Split-Path -Parent $resolvedTemporary) -ne $resolvedOutputDirectory -or
            -not (Split-Path -Leaf $resolvedTemporary).StartsWith('.windvale-segmented-overlay-', [StringComparison]::Ordinal)) {
            throw "Refusing to remove unexpected temporary path: $resolvedTemporary"
        }
        Remove-Item -LiteralPath $resolvedTemporary -Recurse -Force
    }
}
