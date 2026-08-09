[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$OutputPath,
    [ValidateSet('normal', 'invalid-opcode', 'general-protection', 'user-fault', 'service-fault')]
    [string]$Scenario = 'normal'
)

$ErrorActionPreference = 'Stop'

function Fail-Recovery {
    param(
        [Parameter(Mandatory)]
        [string]$Message
    )

    throw "WVOS2001: $Message"
}

$Dotnet = Get-Command dotnet -CommandType Application -ErrorAction SilentlyContinue |
    Select-Object -First 1
if ($null -eq $Dotnet) {
    Fail-Recovery 'The .NET SDK host is unavailable.'
}

$RepositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$BuilderProject = Join-Path $RepositoryRoot 'Operating-System/Windvale.Bootstrap/Windvale.Bootstrap.csproj'
$NativePackager = Join-Path $RepositoryRoot 'Tools/Native/Package-Uefi.cmd'
if (!(Test-Path -LiteralPath $NativePackager -PathType Leaf)) {
    Fail-Recovery 'The digest-bound native UEFI packager launcher is unavailable.'
}
$ResolvedOutputPath = [IO.Path]::GetFullPath($OutputPath)
if ([IO.Path]::GetExtension($ResolvedOutputPath) -ine '.efi') {
    Fail-Recovery 'The recovery output must use the .efi suffix.'
}
if (Test-Path -LiteralPath $ResolvedOutputPath) {
    Fail-Recovery 'The recovery output already exists.'
}

$OutputDirectory = Split-Path -Parent $ResolvedOutputPath
if (!(Test-Path -LiteralPath $OutputDirectory -PathType Container)) {
    Fail-Recovery 'The recovery output directory does not exist.'
}

$CandidateToken = [guid]::NewGuid().ToString('N')
$CandidatePath = Join-Path $OutputDirectory (
    '.windvale-os-probe-recovery-' + $CandidateToken + '.efi')
$LinkedPath = Join-Path $OutputDirectory (
    '.windvale-os-probe-linked-' + $CandidateToken + '.bin')
try {
    $BuilderOutput = @(
        & $Dotnet.Source run `
            --project $BuilderProject `
            --configuration Release `
            -- `
            --linked-output $LinkedPath `
            --scenario $Scenario 2>&1
    )
    if ($LASTEXITCODE -ne 0 -or !(Test-Path -LiteralPath $LinkedPath -PathType Leaf)) {
        $BuilderMessage = ($BuilderOutput | ForEach-Object { $_.ToString() }) -join ' '
        Fail-Recovery "The Stage 0 firmware-probe linked-image build failed. $BuilderMessage"
    }
    $EntryLines = @($BuilderOutput | ForEach-Object { $_.ToString() } |
        Where-Object { $_ -like 'entry-offset=*' })
    if ($EntryLines.Count -ne 1 -or $EntryLines[0] -notmatch '^entry-offset=([0-9]+)$') {
        Fail-Recovery 'The Stage 0 linked-image builder did not report one decimal entry offset.'
    }
    $EntryOffset = $Matches[1]

    $PackagerOutput = @(
        & $NativePackager $LinkedPath $EntryOffset $CandidatePath 2>&1
    )
    if ($LASTEXITCODE -ne 0 -or !(Test-Path -LiteralPath $CandidatePath -PathType Leaf)) {
        $PackagerMessage = ($PackagerOutput | ForEach-Object { $_.ToString() }) -join ' '
        Fail-Recovery "The native UEFI packaging step failed. $PackagerMessage"
    }

    $Identity = Get-Item -LiteralPath $CandidatePath
    $Sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $CandidatePath).Hash.ToLowerInvariant()
    Move-Item -LiteralPath $CandidatePath -Destination $ResolvedOutputPath

    Write-Output 'windvale-os-probe-recovery 40'
    Write-Output "scenario=$Scenario"
    Write-Output "efi-bytes=$($Identity.Length)"
    Write-Output "efi-sha256=$Sha256"
    Write-Output "output=$ResolvedOutputPath"
} finally {
    if (Test-Path -LiteralPath $LinkedPath) {
        Remove-Item -LiteralPath $LinkedPath -Force
    }
    if (Test-Path -LiteralPath $CandidatePath) {
        Remove-Item -LiteralPath $CandidatePath -Force
    }
}
