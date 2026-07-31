[CmdletBinding()]
param(
    [string]$QemuPath,
    [string]$FirmwareCodePath,
    [string]$FirmwareVariablesTemplatePath,
    [ValidateRange(5, 300)]
    [int]$TimeoutSeconds = 60,
    [switch]$KeepRunDirectory,
    [switch]$PassThru,
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$EXPECTED_QEMU_EXIT_CODE = 1
$EXPECTED_SERIAL_MARKER =
    "windvale-os-boot 5`nentry=pass`nsystem-table=pass`nmemory-map=pass`nboot-services=exited`nHello from Windvale`nwindvale-source=pass`nstatus=pass`n"

function Fail-Boot {
    param(
        [Parameter(Mandatory)]
        [string]$Code,
        [Parameter(Mandatory)]
        [string]$Message
    )

    throw "$Code`: $Message"
}

function Remove-ValidatedRunDirectory {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $ResolvedTemp = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    $ResolvedRun = [IO.Path]::GetFullPath($Path).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    $RequiredPrefix = $ResolvedTemp + [IO.Path]::DirectorySeparatorChar + 'windvale-os-boot-'
    if (!$ResolvedRun.StartsWith($RequiredPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        Fail-Boot 'WVOS3007' 'The temporary run directory failed its cleanup boundary check.'
    }
    if (Test-Path -LiteralPath $ResolvedRun) {
        Remove-Item -LiteralPath $ResolvedRun -Recurse -Force
    }
}

$RepositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$EnvironmentVerifier = Join-Path $PSScriptRoot 'Verify-Os-Environment.ps1'
$EnvironmentArguments = @{
    PassThru = $true
    Quiet = $true
}
if (![string]::IsNullOrWhiteSpace($QemuPath)) {
    $EnvironmentArguments.QemuPath = $QemuPath
}
if (![string]::IsNullOrWhiteSpace($FirmwareCodePath)) {
    $EnvironmentArguments.FirmwareCodePath = $FirmwareCodePath
}
if (![string]::IsNullOrWhiteSpace($FirmwareVariablesTemplatePath)) {
    $EnvironmentArguments.FirmwareVariablesTemplatePath = $FirmwareVariablesTemplatePath
}
$Environment = & $EnvironmentVerifier @EnvironmentArguments

$Dotnet = Get-Command dotnet -CommandType Application -ErrorAction SilentlyContinue |
    Select-Object -First 1
if ($null -eq $Dotnet) {
    Fail-Boot 'WVOS3001' 'The .NET SDK host is unavailable.'
}

$RunDirectory = Join-Path (
    [IO.Path]::GetFullPath([IO.Path]::GetTempPath())) (
    'windvale-os-boot-' + [guid]::NewGuid().ToString('N'))
$BootRoot = Join-Path $RunDirectory 'boot'
$EfiDirectory = Join-Path $BootRoot 'EFI/BOOT'
$EfiPath = Join-Path $EfiDirectory 'BOOTX64.EFI'
$VariablesPath = Join-Path $RunDirectory 'OVMF_VARS.fd'
$SerialPath = Join-Path $RunDirectory 'serial.log'
$StandardOutputPath = Join-Path $RunDirectory 'qemu.stdout.log'
$StandardErrorPath = Join-Path $RunDirectory 'qemu.stderr.log'
$VariablesTemplateHashBefore = (
    Get-FileHash -Algorithm SHA256 -LiteralPath $Environment.FirmwareVariablesTemplatePath
).Hash.ToLowerInvariant()
$FirmwareCodeHashBefore = (
    Get-FileHash -Algorithm SHA256 -LiteralPath $Environment.FirmwareCodePath
).Hash.ToLowerInvariant()

try {
    $null = New-Item -ItemType Directory -Path $EfiDirectory
    Copy-Item -LiteralPath $Environment.FirmwareVariablesTemplatePath -Destination $VariablesPath

    $BuilderProject = Join-Path $RepositoryRoot 'Operating-System/Windvale.Bootstrap/Windvale.Bootstrap.csproj'
    $BuilderOutput = @(
        & $Dotnet.Source run `
            --project $BuilderProject `
            --configuration Release `
            -- `
            --output $EfiPath 2>&1
    )
    if ($LASTEXITCODE -ne 0 -or !(Test-Path -LiteralPath $EfiPath -PathType Leaf)) {
        $BuilderMessage = ($BuilderOutput | ForEach-Object { $_.ToString() }) -join ' '
        Fail-Boot 'WVOS3001' "The firmware probe build failed. $BuilderMessage"
    }

    $EfiIdentity = Get-Item -LiteralPath $EfiPath
    $EfiSha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $EfiPath).Hash.ToLowerInvariant()

    $StartInfo = [Diagnostics.ProcessStartInfo]::new()
    $StartInfo.FileName = $Environment.QemuPath
    $StartInfo.UseShellExecute = $false
    $StartInfo.CreateNoWindow = $true
    $StartInfo.RedirectStandardOutput = $true
    $StartInfo.RedirectStandardError = $true
    $QemuArguments = @(
        '-machine', 'pc-q35-11.0,accel=tcg',
        '-cpu', 'qemu64',
        '-smp', '1',
        '-m', '128M',
        '-drive', "if=pflash,unit=0,format=raw,readonly=on,file=$($Environment.FirmwareCodePath)",
        '-drive', "if=pflash,unit=1,format=raw,file=$VariablesPath",
        '-drive', "if=ide,format=raw,file=fat:rw:$BootRoot",
        '-boot', 'order=c,menu=off,strict=on',
        '-display', 'none',
        '-monitor', 'none',
        '-serial', "file:$SerialPath",
        '-nic', 'none',
        '-no-reboot',
        '-device', 'isa-debug-exit,iobase=0xf4,iosize=0x04'
    )
    foreach ($Argument in $QemuArguments) {
        $StartInfo.ArgumentList.Add($Argument)
    }

    $Process = [Diagnostics.Process]::new()
    $Process.StartInfo = $StartInfo
    if (!$Process.Start()) {
        Fail-Boot 'WVOS3002' 'QEMU did not start.'
    }
    $OutputTask = $Process.StandardOutput.ReadToEndAsync()
    $ErrorTask = $Process.StandardError.ReadToEndAsync()
    if (!$Process.WaitForExit($TimeoutSeconds * 1000)) {
        $Process.Kill($true)
        $Process.WaitForExit()
        $null = $OutputTask.GetAwaiter().GetResult()
        $null = $ErrorTask.GetAwaiter().GetResult()
        Fail-Boot 'WVOS3003' "QEMU did not complete within $TimeoutSeconds seconds."
    }
    $QemuOutput = $OutputTask.GetAwaiter().GetResult()
    $QemuError = $ErrorTask.GetAwaiter().GetResult()
    [IO.File]::WriteAllText($StandardOutputPath, $QemuOutput)
    [IO.File]::WriteAllText($StandardErrorPath, $QemuError)
    if ($Process.ExitCode -ne $EXPECTED_QEMU_EXIT_CODE) {
        Fail-Boot 'WVOS3004' "QEMU exited with code $($Process.ExitCode); expected $EXPECTED_QEMU_EXIT_CODE."
    }
    if (!(Test-Path -LiteralPath $SerialPath -PathType Leaf)) {
        Fail-Boot 'WVOS3005' 'QEMU produced no serial evidence.'
    }
    $Serial = [IO.File]::ReadAllText($SerialPath, [Text.Encoding]::ASCII)
    if (!$Serial.Contains($EXPECTED_SERIAL_MARKER, [StringComparison]::Ordinal)) {
        Fail-Boot 'WVOS3005' 'The serial evidence does not contain the complete success marker.'
    }

    if (!(Test-Path -LiteralPath $EfiPath -PathType Leaf)) {
        Fail-Boot 'WVOS3006' 'The generated EFI application disappeared during the boot run.'
    }
    $EfiHashAfter = (Get-FileHash -Algorithm SHA256 -LiteralPath $EfiPath).Hash.ToLowerInvariant()
    $VariablesTemplateHashAfter = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $Environment.FirmwareVariablesTemplatePath
    ).Hash.ToLowerInvariant()
    $FirmwareCodeHashAfter = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $Environment.FirmwareCodePath
    ).Hash.ToLowerInvariant()
    if (
        $EfiHashAfter -ne $EfiSha256 -or
        $VariablesTemplateHashAfter -ne $VariablesTemplateHashBefore -or
        $FirmwareCodeHashAfter -ne $FirmwareCodeHashBefore
    ) {
        Fail-Boot 'WVOS3006' 'A generated application or installed firmware input changed during the boot run.'
    }

    $Report = [pscustomobject][ordered]@{
        Status = 'pass'
        Architecture = 'x86-64'
        ApplicationFormat = 'pe32-plus-uefi-application-v2'
        ProbeVersion = 5
        EfiBytes = $EfiIdentity.Length
        EfiSha256 = $EfiSha256
        SerialMarker = 'windvale-os-boot-5-entry-system-table-memory-map-boot-services-exited-hello-windvale-source-status-pass'
        QemuExitCode = $Process.ExitCode
        RunDirectory = if ($KeepRunDirectory) { $RunDirectory } else { $null }
    }

    if ($PassThru) {
        $Report
    } elseif (!$Quiet) {
        Write-Output 'windvale-os-boot-report 5'
        Write-Output "status=$($Report.Status)"
        Write-Output "architecture=$($Report.Architecture)"
        Write-Output "application-format=$($Report.ApplicationFormat)"
        Write-Output "probe-version=$($Report.ProbeVersion)"
        Write-Output "efi-bytes=$($Report.EfiBytes)"
        Write-Output "efi-sha256=$($Report.EfiSha256)"
        Write-Output "serial-marker=$($Report.SerialMarker)"
        Write-Output "qemu-exit-code=$($Report.QemuExitCode)"
        if ($KeepRunDirectory) {
            Write-Output "run-directory=$RunDirectory"
        }
    }
} finally {
    if (!$KeepRunDirectory) {
        Remove-ValidatedRunDirectory $RunDirectory
    }
}
