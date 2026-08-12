[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ReleaseDirectory,
    [switch]$RunRecovery
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$ReleaseRoot = [IO.Path]::GetFullPath($ReleaseDirectory)
$TemporaryBase = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$TemporaryPrefix = $TemporaryBase.TrimEnd([IO.Path]::DirectorySeparatorChar) +
    [IO.Path]::DirectorySeparatorChar
$TemporaryRoot = Join-Path $TemporaryBase (
    'windvale-stage0-recovery-verification-' + [guid]::NewGuid().ToString('N'))

function Invoke-Checked {
    param([scriptblock]$Command, [string]$Failure)
    $Output = @(& $Command 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "$Failure`n$($Output -join "`n")"
    }
    $Output
}

function Get-Sha256 {
    param([string]$Path)
    (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
}

function Get-GitBlobSha256 {
    param([string]$Repository, [string]$ObjectId)

    $StartInfo = [Diagnostics.ProcessStartInfo]::new()
    $StartInfo.FileName = 'git'
    $StartInfo.UseShellExecute = $false
    $StartInfo.RedirectStandardOutput = $true
    $StartInfo.RedirectStandardError = $true
    foreach ($Argument in @('-C', $Repository, 'cat-file', 'blob', $ObjectId)) {
        $StartInfo.ArgumentList.Add($Argument)
    }

    $Process = [Diagnostics.Process]::new()
    $Process.StartInfo = $StartInfo
    $Hash = [Security.Cryptography.IncrementalHash]::CreateHash(
        [Security.Cryptography.HashAlgorithmName]::SHA256)
    try {
        if (!$Process.Start()) {
            throw "Git could not start while hashing object $ObjectId."
        }
        $Buffer = [byte[]]::new(65536)
        while (($Read = $Process.StandardOutput.BaseStream.Read(
                    $Buffer, 0, $Buffer.Length)) -gt 0) {
            $Hash.AppendData($Buffer, 0, $Read)
        }
        $ErrorText = $Process.StandardError.ReadToEnd()
        $Process.WaitForExit()
        if ($Process.ExitCode -ne 0) {
            throw "Git could not read object $ObjectId.`n$ErrorText"
        }
        [Convert]::ToHexString($Hash.GetHashAndReset()).ToLowerInvariant()
    } finally {
        $Hash.Dispose()
        $Process.Dispose()
    }
}

function Get-GitTreeBlobs {
    param([string]$Repository, [string]$PathPrefix)

    $Lines = if ([string]::IsNullOrWhiteSpace($PathPrefix)) {
        @(git -c core.quotePath=false -C $Repository ls-tree -r -l --full-tree HEAD)
    } else {
        @(git -c core.quotePath=false -C $Repository ls-tree -r -l --full-tree HEAD -- $PathPrefix)
    }
    if ($LASTEXITCODE -ne 0) {
        throw 'Git could not enumerate committed blobs from the recovery bundle.'
    }
    foreach ($Line in $Lines) {
        if ($Line -notmatch '^[0-9]{6} blob ([0-9a-f]{40})\s+([0-9]+)\t(.+)$') {
            throw "Git returned an unsupported recovery blob record: $Line"
        }
        [pscustomobject]@{
            ObjectId = $Matches[1]
            Bytes = [long]$Matches[2]
            Path = $Matches[3]
        }
    }
}

function Get-LfText {
    param([string]$Path)
    (Get-Content -LiteralPath $Path -Raw).
        Replace("`r`n", "`n").Replace("`r", "`n")
}

if (!(Test-Path -LiteralPath $ReleaseRoot -PathType Container)) {
    throw 'The recovery release directory does not exist.'
}
$ManifestFiles = @(Get-ChildItem -LiteralPath $ReleaseRoot -Filter '*-manifest.json' -File)
if ($ManifestFiles.Count -ne 1) {
    throw 'The recovery release must contain exactly one manifest.'
}
$Manifest = Get-Content -LiteralPath $ManifestFiles[0].FullName -Raw | ConvertFrom-Json
if ($Manifest.format -ne 'windvale-stage0-recovery-release-1' -or
    [string]$Manifest.commit -notmatch '^[0-9a-f]{40}$' -or
    [string]$Manifest.tree -notmatch '^[0-9a-f]{40}$') {
    throw 'The recovery release manifest is malformed or unsupported.'
}
if ([string]$Manifest.releaseId -notmatch '^windvale-stage0-recovery-[0-9a-f]{12}$' -or
    [string]$Manifest.releaseId -cne
        "windvale-stage0-recovery-$(([string]$Manifest.commit).Substring(0, 12))") {
    throw 'The recovery release identifier differs from its commit.'
}
if ($ManifestFiles[0].Name -cne "$($Manifest.releaseId)-manifest.json") {
    throw 'The recovery release manifest filename differs from its release identifier.'
}
if ([int]$Manifest.recoveryEntryCount -ne 11) {
    throw 'The recovery release must retain exactly eleven managed recovery entry points.'
}

$ManifestAssets = @($Manifest.assets)
$NamedAssets = @(
    $Manifest.sourceBundle,
    $Manifest.sourceInventory,
    $Manifest.artifactInventory,
    $Manifest.recoveryEntryInventory,
    $Manifest.dependencyInventory,
    $Manifest.licenseInventory,
    $Manifest.runbook
)
if ($ManifestAssets.Count -ne 7 -or
    @($ManifestAssets | Sort-Object -Unique).Count -ne $ManifestAssets.Count -or
    @($ManifestAssets | Where-Object { $_ -notmatch '^[^/\\]+$' }).Count -ne 0 -or
    ![Linq.Enumerable]::SequenceEqual(
        [string[]]@($ManifestAssets | Sort-Object),
        [string[]]@($NamedAssets | Sort-Object))) {
    throw 'The recovery release manifest has an invalid asset inventory.'
}

$ChecksumsPath = Join-Path $ReleaseRoot "$($Manifest.releaseId)-SHA256SUMS"
$ChecksumLines = @(Get-Content -LiteralPath $ChecksumsPath)
$ExpectedAssets = @($Manifest.assets; $ManifestFiles[0].Name) | Sort-Object
$ActualAssets = [System.Collections.Generic.List[string]]::new()
foreach ($Line in $ChecksumLines) {
    if ($Line -notmatch '^([0-9a-f]{64})  ([^/\\]+)$') {
        throw "Malformed recovery checksum line: $Line"
    }
    $ExpectedSha256 = $Matches[1]
    $Name = $Matches[2]
    $Path = Join-Path $ReleaseRoot $Name
    if (!(Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "The recovery release asset is missing: $Name"
    }
    if ((Get-Sha256 $Path) -cne $ExpectedSha256) {
        throw "The recovery release asset has an unexpected digest: $Name"
    }
    $ActualAssets.Add($Name)
}
if (![Linq.Enumerable]::SequenceEqual(
        [string[]]@($ActualAssets | Sort-Object),
        [string[]]$ExpectedAssets)) {
    throw 'The recovery checksum inventory does not match the manifest assets.'
}
$BundlePath = Join-Path $ReleaseRoot $Manifest.sourceBundle
Invoke-Checked { git -C $RepositoryRoot bundle verify $BundlePath } `
    'The recovery Git bundle failed verification.' | Out-Null
$BundleHeads = @(git -C $RepositoryRoot bundle list-heads $BundlePath)
if ($LASTEXITCODE -ne 0 -or $BundleHeads.Count -ne 1 -or
    $BundleHeads[0] -cne "$($Manifest.commit) HEAD") {
    throw 'The recovery Git bundle head differs from the manifest.'
}

try {
    New-Item -ItemType Directory -Path $TemporaryRoot | Out-Null
    $Checkout = Join-Path $TemporaryRoot 'Checkout'
    Invoke-Checked { git init --quiet $Checkout } `
        'The recovery verifier could not initialize a separate checkout.' | Out-Null
    Invoke-Checked { git -C $Checkout config core.autocrlf false } `
        'The recovery verifier could not select canonical checkout line endings.' | Out-Null
    Invoke-Checked { git -C $Checkout fetch --quiet $BundlePath HEAD } `
        'The separate checkout could not fetch the recovery bundle.' | Out-Null
    Invoke-Checked { git -C $Checkout checkout --quiet --detach $Manifest.commit } `
        'The separate checkout could not select the archived commit.' | Out-Null
    $CheckoutTree = (& git -C $Checkout rev-parse 'HEAD^{tree}').Trim()
    if ($LASTEXITCODE -ne 0 -or $CheckoutTree -cne [string]$Manifest.tree) {
        throw 'The separate recovery checkout has an unexpected tree.'
    }

    $SourceInventoryPath = Join-Path $ReleaseRoot $Manifest.sourceInventory
    $ExpectedSourceInventory = Get-LfText $SourceInventoryPath
    $ActualSourceInventory = ((git -C $Checkout ls-tree -r --full-tree HEAD) -join "`n") + "`n"
    if ($LASTEXITCODE -ne 0 -or $ActualSourceInventory -cne $ExpectedSourceInventory) {
        throw 'The separate checkout source inventory differs from the release.'
    }

    $ArtifactInventoryPath = Join-Path $ReleaseRoot $Manifest.artifactInventory
    $ArtifactLines = @(Get-Content -LiteralPath $ArtifactInventoryPath)
    if ($ArtifactLines.Count -ne [int]$Manifest.artifactCount) {
        throw 'The recovery artifact count differs from the manifest.'
    }
    $ActualArtifactRecords = @(
        Get-GitTreeBlobs $Checkout 'Artifacts' | Sort-Object Path)
    if ($ActualArtifactRecords.Count -ne $ArtifactLines.Count) {
        throw 'The bundled checkout artifact count differs from the release inventory.'
    }
    $ActualArtifactByPath = @{}
    foreach ($Record in $ActualArtifactRecords) {
        $ActualArtifactByPath[$Record.Path] = $Record
    }
    $SeenArtifacts = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    foreach ($Line in $ArtifactLines) {
        $Fields = $Line -split "`t", 3
        if ($Fields.Count -ne 3 -or $Fields[1] -notmatch '^[0-9]+$' -or
            $Fields[2] -notmatch '^[0-9a-f]{64}$' -or
            !$SeenArtifacts.Add($Fields[0])) {
            throw "Malformed or duplicate recovery artifact inventory line: $Line"
        }
        if (!$ActualArtifactByPath.ContainsKey($Fields[0])) {
            throw "The separate checkout artifact is missing: $($Fields[0])"
        }
        $Record = $ActualArtifactByPath[$Fields[0]]
        if ($Record.Bytes -ne [long]$Fields[1] -or
            (Get-GitBlobSha256 $Checkout $Record.ObjectId) -cne $Fields[2]) {
            throw "The separate checkout artifact differs: $($Fields[0])"
        }
    }

    $RecoveryInventory = Get-Content -LiteralPath (
        Join-Path $ReleaseRoot $Manifest.recoveryEntryInventory) -Raw | ConvertFrom-Json
    $RecoveryEntries = @($RecoveryInventory.directManagedEntrypoints)
    if ($RecoveryEntries.Count -ne [int]$Manifest.recoveryEntryCount -or
        @($RecoveryEntries | Where-Object { $_.mode -ne 'recovery' }).Count -ne 0) {
        throw 'The archived managed-entry inventory is not recovery-only.'
    }
    $Dependencies = Get-Content -LiteralPath (
        Join-Path $ReleaseRoot $Manifest.dependencyInventory) -Raw | ConvertFrom-Json
    if ($Dependencies.format -ne 'windvale-stage0-recovery-dependencies-1') {
        throw 'The archived dependency inventory is unsupported.'
    }
    if (@(Get-Content -LiteralPath (Join-Path $ReleaseRoot $Manifest.licenseInventory)).Count -eq 0) {
        throw 'The archived license inventory is empty.'
    }

    $ArchivedCopies = @(
        @{
            Asset = $Manifest.recoveryEntryInventory
            Source = 'Documents/Project/Dotnet-Retirement-Inventory.json'
        },
        @{
            Asset = $Manifest.dependencyInventory
            Source = 'Documents/Project/Stage0-Recovery-Dependencies.json'
        },
        @{
            Asset = $Manifest.runbook
            Source = 'Documents/Runbooks/Stage0-Recovery-Archive.md'
        }
    )
    foreach ($Copy in $ArchivedCopies) {
        if ((Get-LfText (Join-Path $ReleaseRoot $Copy.Asset)) -cne
            (Get-LfText (Join-Path $Checkout $Copy.Source))) {
            throw "The archived copy differs from the bundled source: $($Copy.Source)"
        }
    }

    $SeenLicenses = [Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    $ActualLicenseRecords = @(
        Get-GitTreeBlobs $Checkout '' |
            Where-Object {
                [IO.Path]::GetFileName($_.Path) -match
                    '(?i)(?:^LICENSE|LICENSE\.|-LICENSE\.|^NOTICE)'
            } |
            Sort-Object Path
    )
    $ActualLicenseByPath = @{}
    foreach ($Record in $ActualLicenseRecords) {
        $ActualLicenseByPath[$Record.Path] = $Record
    }
    foreach ($Line in Get-Content -LiteralPath (
            Join-Path $ReleaseRoot $Manifest.licenseInventory)) {
        $Fields = $Line -split "`t", 3
        if ($Fields.Count -ne 3 -or $Fields[1] -notmatch '^[0-9]+$' -or
            $Fields[2] -notmatch '^[0-9a-f]{64}$' -or
            !$SeenLicenses.Add($Fields[0])) {
            throw "Malformed or duplicate recovery license inventory line: $Line"
        }
        if (!$ActualLicenseByPath.ContainsKey($Fields[0])) {
            throw "The separate checkout license is missing: $($Fields[0])"
        }
        $Record = $ActualLicenseByPath[$Fields[0]]
        if ($Record.Bytes -ne [long]$Fields[1] -or
            (Get-GitBlobSha256 $Checkout $Record.ObjectId) -cne $Fields[2]) {
            throw "The separate checkout license differs: $($Fields[0])"
        }
    }
    if ($SeenLicenses.Count -ne $ActualLicenseRecords.Count) {
        throw 'The recovery license inventory omits a bundled license or notice.'
    }

    if ($RunRecovery) {
        if ($IsWindows) {
            $ManagedBootstrap = Join-Path $Checkout 'Tools/Recovery/Verify-Managed-Bootstrap.ps1'
            $SeedRebuilder = Join-Path $Checkout 'Tools/Recovery/Rebuild-Native-Compiler-Seed.ps1'
            $FrontDoorRebuilder = Join-Path $Checkout 'Tools/Recovery/Rebuild-Native-Front-Door.ps1'
            $NativeBootstrap = Join-Path $Checkout 'Tools/Verify/Verify-Bootstrap.cmd'
        } elseif ($IsLinux) {
            $ManagedBootstrap = Join-Path $Checkout 'Tools/Recovery/Verify-Managed-Bootstrap.sh'
            $SeedRebuilder = Join-Path $Checkout 'Tools/Recovery/Rebuild-Native-Compiler-Seed.sh'
            $FrontDoorRebuilder = Join-Path $Checkout 'Tools/Recovery/Rebuild-Native-Front-Door.sh'
            $NativeBootstrap = Join-Path $Checkout 'Tools/Verify/Verify-Bootstrap.sh'
        } else {
            throw 'Complete recovery verification supports only Windows and Linux.'
        }

        Push-Location $Checkout
        try {
            Invoke-Checked { & $ManagedBootstrap } `
                'Managed Stage 1/Stage 2 recovery convergence failed.' | Out-Null
            Invoke-Checked { & $SeedRebuilder (Join-Path $TemporaryRoot 'Native-Compiler-Seed') } `
                'Native compiler seed recovery failed.' | Out-Null
            Invoke-Checked { & $FrontDoorRebuilder (Join-Path $TemporaryRoot 'Native-Front-Door') } `
                'Native front-door recovery failed.' | Out-Null
            Invoke-Checked { & $NativeBootstrap } `
                'The recovered source could not hand off to native compiler convergence.' | Out-Null
        } finally {
            Pop-Location
        }
    }

    $HostProfile = if ($IsWindows) { 'windows-x64' } elseif ($IsLinux) { 'linux-x64' } else { 'unsupported' }

    Write-Output 'windvale-stage0-recovery-verification 1'
    Write-Output "release-id=$($Manifest.releaseId)"
    Write-Output "commit=$($Manifest.commit)"
    Write-Output "tree=$($Manifest.tree)"
    Write-Output "artifacts=$($ArtifactLines.Count)"
    Write-Output "recovery-entries=$($RecoveryEntries.Count)"
    Write-Output "host-profile=$HostProfile"
    if ($RunRecovery) {
        Write-Output 'managed-bootstrap=passed'
        Write-Output 'native-seed-reconstruction=passed'
        Write-Output 'native-front-door-reconstruction=passed'
        Write-Output 'native-bootstrap-handoff=passed'
    }
    Write-Output "recovery-executed=$($RunRecovery.IsPresent.ToString().ToLowerInvariant())"
} finally {
    $ResolvedTemporary = [IO.Path]::GetFullPath($TemporaryRoot)
    if (!$ResolvedTemporary.StartsWith($TemporaryPrefix, [StringComparison]::OrdinalIgnoreCase) -or
        ![IO.Path]::GetFileName($ResolvedTemporary).StartsWith(
            'windvale-stage0-recovery-verification-', [StringComparison]::Ordinal)) {
        throw "Refusing to remove unexpected recovery-verification path: $ResolvedTemporary"
    }
    if (Test-Path -LiteralPath $ResolvedTemporary) {
        Remove-Item -LiteralPath $ResolvedTemporary -Recurse -Force
    }
}
