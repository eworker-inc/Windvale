[CmdletBinding()]
param(
    [string]$QemuPath,
    [string]$FirmwareCodePath,
    [string]$FirmwareVariablesTemplatePath,
    [switch]$PassThru,
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$EXPECTED_QEMU_VERSION = '11.0.0'
$EXPECTED_MACHINE = 'pc-q35-11.0'
$EXPECTED_ACCELERATOR = 'tcg'
$EXPECTED_FIRMWARE_CODE_BYTES = 3653632L
$EXPECTED_FIRMWARE_CODE_SHA256 = '33090cc07675baa5190d9f1e84bf5176b33bcbfa9bacac522961150cdb6dbb2a'
$EXPECTED_FIRMWARE_VARIABLES_BYTES = 540672L
$EXPECTED_FIRMWARE_VARIABLES_SHA256 = '5d2ac383371b408398accee7ec27c8c09ea5b74a0de0ceea6513388b15be5d1e'

function Fail-Preflight {
    param(
        [Parameter(Mandatory)]
        [string]$Code,
        [Parameter(Mandatory)]
        [string]$Message
    )

    throw "$Code`: $Message"
}

function Resolve-RequiredFile {
    param(
        [string]$ExplicitPath,
        [Parameter(Mandatory)]
        [string[]]$Candidate,
        [Parameter(Mandatory)]
        [string]$Code,
        [Parameter(Mandatory)]
        [string]$Description
    )

    $Paths = if (![string]::IsNullOrWhiteSpace($ExplicitPath)) {
        @($ExplicitPath)
    } else {
        @($Candidate)
    }

    foreach ($Path in $Paths) {
        if ([string]::IsNullOrWhiteSpace($Path)) {
            continue
        }
        if (Test-Path -LiteralPath $Path -PathType Leaf) {
            return (Get-Item -LiteralPath $Path).FullName
        }
    }

    Fail-Preflight $Code "The $Description was not found. Supply its path explicitly."
}

function Get-FileIdentity {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $Item = Get-Item -LiteralPath $Path
    [pscustomobject]@{
        Bytes = $Item.Length
        Sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
    }
}

$QemuCommand = Get-Command qemu-system-x86_64.exe -CommandType Application -ErrorAction SilentlyContinue |
    Select-Object -First 1
if ($null -eq $QemuCommand) {
    $QemuCommand = Get-Command qemu-system-x86_64 -CommandType Application -ErrorAction SilentlyContinue |
        Select-Object -First 1
}

$QemuCandidates = [System.Collections.Generic.List[string]]::new()
if ($null -ne $QemuCommand) {
    $QemuCandidates.Add($QemuCommand.Source)
}
if (![string]::IsNullOrWhiteSpace(${env:ProgramFiles})) {
    $QemuCandidates.Add((Join-Path ${env:ProgramFiles} 'qemu/qemu-system-x86_64.exe'))
}
$QemuCandidates.Add('/usr/bin/qemu-system-x86_64')
$QemuCandidates.Add('/usr/local/bin/qemu-system-x86_64')

$ResolvedQemu = Resolve-RequiredFile `
    -ExplicitPath $QemuPath `
    -Candidate $QemuCandidates.ToArray() `
    -Code 'WVOS1001' `
    -Description 'QEMU x86-64 executable'

$QemuRoot = Split-Path -Parent $ResolvedQemu
$FirmwareCodeCandidates = @(
    (Join-Path $QemuRoot 'share/edk2-x86_64-code.fd'),
    '/usr/share/qemu/edk2-x86_64-code.fd',
    '/usr/share/OVMF/OVMF_CODE.fd',
    '/usr/share/OVMF/OVMF_CODE_4M.fd',
    '/usr/share/edk2/x64/OVMF_CODE.fd'
)
$FirmwareVariablesCandidates = @(
    (Join-Path $QemuRoot 'share/edk2-i386-vars.fd'),
    '/usr/share/qemu/edk2-i386-vars.fd',
    '/usr/share/OVMF/OVMF_VARS.fd',
    '/usr/share/OVMF/OVMF_VARS_4M.fd',
    '/usr/share/edk2/x64/OVMF_VARS.fd'
)

$ResolvedFirmwareCode = Resolve-RequiredFile `
    -ExplicitPath $FirmwareCodePath `
    -Candidate $FirmwareCodeCandidates `
    -Code 'WVOS1005' `
    -Description 'accepted x86-64 UEFI firmware code image'
$ResolvedFirmwareVariables = Resolve-RequiredFile `
    -ExplicitPath $FirmwareVariablesTemplatePath `
    -Candidate $FirmwareVariablesCandidates `
    -Code 'WVOS1005' `
    -Description 'accepted UEFI variable-store template'

$VersionOutput = @(& $ResolvedQemu --version 2>&1)
if ($LASTEXITCODE -ne 0 -or $VersionOutput.Count -eq 0) {
    Fail-Preflight 'WVOS1001' 'QEMU could not report its version.'
}
$VersionLine = $VersionOutput[0].ToString().Trim()
$VersionMatch = [regex]::Match(
    $VersionLine,
    '^QEMU emulator version (?<Version>[0-9]+\.[0-9]+\.[0-9]+)(?: \((?<Build>[^)]+)\))?$')
if (!$VersionMatch.Success) {
    Fail-Preflight 'WVOS1001' 'QEMU returned an unrecognized version line.'
}
$Version = $VersionMatch.Groups['Version'].Value
if ($Version -ne $EXPECTED_QEMU_VERSION) {
    Fail-Preflight `
        'WVOS1002' `
        "QEMU version $Version is installed; version $EXPECTED_QEMU_VERSION is required."
}
$Build = if ($VersionMatch.Groups['Build'].Success) {
    $VersionMatch.Groups['Build'].Value
} else {
    'unreported'
}
if ($Build -notmatch '^[A-Za-z0-9.+_-]+$') {
    $Build = 'unreported'
}

$AcceleratorOutput = @(& $ResolvedQemu -accel help 2>&1)
if ($LASTEXITCODE -ne 0) {
    Fail-Preflight 'WVOS1003' 'QEMU could not enumerate its accelerators.'
}
$Accelerators = @(
    $AcceleratorOutput |
        ForEach-Object { $_.ToString().Trim() } |
        Where-Object { $_ -match '^[a-z0-9_-]+$' }
)
if ($EXPECTED_ACCELERATOR -notin $Accelerators) {
    Fail-Preflight 'WVOS1003' "QEMU does not expose accelerator $EXPECTED_ACCELERATOR."
}

$MachineOutput = @(& $ResolvedQemu -machine help 2>&1)
if ($LASTEXITCODE -ne 0) {
    Fail-Preflight 'WVOS1004' 'QEMU could not enumerate its machines.'
}
$MachinePattern = '^\s*' + [regex]::Escape($EXPECTED_MACHINE) + '\s'
if (!($MachineOutput | Where-Object { $_.ToString() -match $MachinePattern })) {
    Fail-Preflight 'WVOS1004' "QEMU does not expose machine $EXPECTED_MACHINE."
}

$QemuIdentity = Get-FileIdentity $ResolvedQemu
$FirmwareCodeIdentity = Get-FileIdentity $ResolvedFirmwareCode
if (
    $FirmwareCodeIdentity.Bytes -ne $EXPECTED_FIRMWARE_CODE_BYTES -or
    $FirmwareCodeIdentity.Sha256 -ne $EXPECTED_FIRMWARE_CODE_SHA256
) {
    Fail-Preflight 'WVOS1006' 'The UEFI firmware code image does not match boot environment version 1.'
}
$FirmwareVariablesIdentity = Get-FileIdentity $ResolvedFirmwareVariables
if (
    $FirmwareVariablesIdentity.Bytes -ne $EXPECTED_FIRMWARE_VARIABLES_BYTES -or
    $FirmwareVariablesIdentity.Sha256 -ne $EXPECTED_FIRMWARE_VARIABLES_SHA256
) {
    Fail-Preflight 'WVOS1007' 'The UEFI variable-store template does not match boot environment version 1.'
}

$Report = [pscustomobject][ordered]@{
    Status = 'ready'
    Architecture = 'x86-64'
    Firmware = 'uefi-2.11'
    QemuVersion = $Version
    QemuBuild = $Build
    QemuBytes = $QemuIdentity.Bytes
    QemuSha256 = $QemuIdentity.Sha256
    Machine = $EXPECTED_MACHINE
    Cpu = 'qemu64'
    Accelerator = $EXPECTED_ACCELERATOR
    VirtualCpus = 1
    MemoryMib = 128
    Network = 'none'
    SecureBoot = 'disabled'
    FirmwareCodeBytes = $FirmwareCodeIdentity.Bytes
    FirmwareCodeSha256 = $FirmwareCodeIdentity.Sha256
    FirmwareVariablesTemplateBytes = $FirmwareVariablesIdentity.Bytes
    FirmwareVariablesTemplateSha256 = $FirmwareVariablesIdentity.Sha256
}

if ($PassThru) {
    $Report
} elseif (!$Quiet) {
    Write-Output 'windvale-os-environment 1'
    Write-Output "status=$($Report.Status)"
    Write-Output "architecture=$($Report.Architecture)"
    Write-Output "firmware=$($Report.Firmware)"
    Write-Output "qemu-version=$($Report.QemuVersion)"
    Write-Output "qemu-build=$($Report.QemuBuild)"
    Write-Output "qemu-bytes=$($Report.QemuBytes)"
    Write-Output "qemu-sha256=$($Report.QemuSha256)"
    Write-Output "machine=$($Report.Machine)"
    Write-Output "cpu=$($Report.Cpu)"
    Write-Output "accelerator=$($Report.Accelerator)"
    Write-Output "virtual-cpus=$($Report.VirtualCpus)"
    Write-Output "memory-mib=$($Report.MemoryMib)"
    Write-Output "network=$($Report.Network)"
    Write-Output "secure-boot=$($Report.SecureBoot)"
    Write-Output "firmware-code-bytes=$($Report.FirmwareCodeBytes)"
    Write-Output "firmware-code-sha256=$($Report.FirmwareCodeSha256)"
    Write-Output "firmware-variables-template-bytes=$($Report.FirmwareVariablesTemplateBytes)"
    Write-Output "firmware-variables-template-sha256=$($Report.FirmwareVariablesTemplateSha256)"
}
