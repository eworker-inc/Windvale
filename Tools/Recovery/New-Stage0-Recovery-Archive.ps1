[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$Destination = [IO.Path]::GetFullPath($OutputDirectory)
$DependencyPath = Join-Path $RepositoryRoot 'Documents/Project/Stage0-Recovery-Dependencies.json'
$RecoveryInventoryPath = Join-Path $RepositoryRoot 'Documents/Project/Dotnet-Retirement-Inventory.json'
$RunbookPath = Join-Path $RepositoryRoot 'Documents/Runbooks/Stage0-Recovery-Archive.md'
$Utf8 = [Text.UTF8Encoding]::new($false)
$PathComparison = if ($IsWindows) {
    [StringComparison]::OrdinalIgnoreCase
} else {
    [StringComparison]::Ordinal
}

function Invoke-Checked {
    param([scriptblock]$Command, [string]$Failure)
    $Output = @(& $Command 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "$Failure`n$($Output -join "`n")"
    }
    $Output
}

function Write-LfText {
    param([string]$Path, [string[]]$Line)
    [IO.File]::WriteAllText($Path, (($Line -join "`n") + "`n"), $Utf8)
}

function Copy-LfText {
    param([string]$Source, [string]$Destination)
    $Content = (Get-Content -LiteralPath $Source -Raw).
        Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd("`n")
    [IO.File]::WriteAllText($Destination, ($Content + "`n"), $Utf8)
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
        throw 'Git could not enumerate committed blobs for the recovery archive.'
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

$Status = @(git -C $RepositoryRoot status --porcelain --untracked-files=normal)
if ($LASTEXITCODE -ne 0) {
    throw 'Git could not inspect the recovery source tree.'
}
if ($Status.Count -ne 0) {
    throw 'The final recovery archive requires a clean committed source tree.'
}

$RepositoryPrefix = $RepositoryRoot.TrimEnd(
    [IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
if ($Destination -eq $RepositoryRoot -or
    $Destination.StartsWith($RepositoryPrefix, $PathComparison)) {
    throw 'The recovery output directory must be outside the source repository.'
}

$Commit = (& git -C $RepositoryRoot rev-parse HEAD).Trim()
$Tree = (& git -C $RepositoryRoot rev-parse 'HEAD^{tree}').Trim()
$CommitTimestamp = (& git -C $RepositoryRoot show -s --format=%cI HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $Commit -notmatch '^[0-9a-f]{40}$' -or
    $Tree -notmatch '^[0-9a-f]{40}$') {
    throw 'Git could not resolve the exact recovery commit and tree.'
}
$ReleaseId = "windvale-stage0-recovery-$($Commit.Substring(0, 12))"

if (Test-Path -LiteralPath $Destination) {
    if (!(Test-Path -LiteralPath $Destination -PathType Container)) {
        throw 'The recovery output exists and is not a directory.'
    }
    if (@(Get-ChildItem -LiteralPath $Destination -Force).Count -ne 0) {
        throw 'The recovery output directory must be empty.'
    }
} else {
    New-Item -ItemType Directory -Path $Destination | Out-Null
}

$BundleName = "$ReleaseId-source.bundle"
$SourceInventoryName = "$ReleaseId-source-inventory.txt"
$ArtifactInventoryName = "$ReleaseId-artifact-inventory.txt"
$RecoveryInventoryName = "$ReleaseId-recovery-entry-inventory.json"
$DependencyName = "$ReleaseId-dependencies.json"
$LicenseInventoryName = "$ReleaseId-license-inventory.txt"
$RunbookName = "$ReleaseId-runbook.md"
$ManifestName = "$ReleaseId-manifest.json"
$ChecksumsName = "$ReleaseId-SHA256SUMS"

$BundlePath = Join-Path $Destination $BundleName
Invoke-Checked { git -C $RepositoryRoot bundle create $BundlePath HEAD } `
    'Git could not create the complete recovery source bundle.' | Out-Null
Invoke-Checked { git -C $RepositoryRoot bundle verify $BundlePath } `
    'The generated recovery source bundle failed verification.' | Out-Null
$BundleHeads = @(git -C $RepositoryRoot bundle list-heads $BundlePath)
if ($LASTEXITCODE -ne 0 -or $BundleHeads.Count -ne 1 -or
    $BundleHeads[0] -cne "$Commit HEAD") {
    throw 'The recovery source bundle does not expose the exact commit as HEAD.'
}

$SourceInventory = @(git -C $RepositoryRoot ls-tree -r --full-tree HEAD)
if ($LASTEXITCODE -ne 0 -or $SourceInventory.Count -eq 0) {
    throw 'Git could not create the exact source inventory.'
}
Write-LfText (Join-Path $Destination $SourceInventoryName) $SourceInventory

$ArtifactRecords = @(Get-GitTreeBlobs $RepositoryRoot 'Artifacts' | Sort-Object Path)
$ArtifactInventory = foreach ($Record in $ArtifactRecords) {
    "$($Record.Path)`t$($Record.Bytes)`t$(Get-GitBlobSha256 $RepositoryRoot $Record.ObjectId)"
}
Write-LfText (Join-Path $Destination $ArtifactInventoryName) $ArtifactInventory

$RecoveryInventory = Get-Content -LiteralPath $RecoveryInventoryPath -Raw | ConvertFrom-Json
$RecoveryEntries = @($RecoveryInventory.directManagedEntrypoints)
if ($RecoveryEntries.Count -ne 11 -or
    @($RecoveryEntries | Where-Object { $_.mode -ne 'recovery' }).Count -ne 0) {
    throw 'The final recovery archive requires exactly eleven recovery-only managed entry points.'
}
Copy-LfText $RecoveryInventoryPath (Join-Path $Destination $RecoveryInventoryName)
Copy-LfText $DependencyPath (Join-Path $Destination $DependencyName)
Copy-LfText $RunbookPath (Join-Path $Destination $RunbookName)

$LicenseRecords = @(
    Get-GitTreeBlobs $RepositoryRoot '' |
        Where-Object {
            [IO.Path]::GetFileName($_.Path) -match '(?i)(?:^LICENSE|LICENSE\.|-LICENSE\.|^NOTICE)'
        } |
        Sort-Object Path
)
$LicenseInventory = foreach ($Record in $LicenseRecords) {
    "$($Record.Path)`t$($Record.Bytes)`t$(Get-GitBlobSha256 $RepositoryRoot $Record.ObjectId)"
}
if ($LicenseInventory.Count -eq 0) {
    throw 'The recovery release has no license inventory.'
}
Write-LfText (Join-Path $Destination $LicenseInventoryName) $LicenseInventory

$Assets = @(
    $BundleName,
    $SourceInventoryName,
    $ArtifactInventoryName,
    $RecoveryInventoryName,
    $DependencyName,
    $LicenseInventoryName,
    $RunbookName
)
$Manifest = [ordered]@{
    format = 'windvale-stage0-recovery-release-1'
    releaseId = $ReleaseId
    commit = $Commit
    tree = $Tree
    commitTimestamp = $CommitTimestamp
    sourceBundle = $BundleName
    sourceInventory = $SourceInventoryName
    artifactInventory = $ArtifactInventoryName
    recoveryEntryInventory = $RecoveryInventoryName
    dependencyInventory = $DependencyName
    licenseInventory = $LicenseInventoryName
    runbook = $RunbookName
    artifactCount = $ArtifactRecords.Count
    recoveryEntryCount = $RecoveryEntries.Count
    assets = $Assets
}
$ManifestJson = $Manifest | ConvertTo-Json -Depth 5
Write-LfText (Join-Path $Destination $ManifestName) @($ManifestJson)

$ChecksumAssets = @($Assets; $ManifestName) | Sort-Object
$Checksums = foreach ($Name in $ChecksumAssets) {
    "$(Get-Sha256 (Join-Path $Destination $Name))  $Name"
}
Write-LfText (Join-Path $Destination $ChecksumsName) $Checksums

Write-Output 'windvale-stage0-recovery-release 1'
Write-Output "release-id=$ReleaseId"
Write-Output "commit=$Commit"
Write-Output "tree=$Tree"
Write-Output "artifacts=$($ArtifactRecords.Count)"
Write-Output "recovery-entries=$($RecoveryEntries.Count)"
Write-Output "output=$Destination"
