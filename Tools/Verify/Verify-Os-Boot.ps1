[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$EfiPath,
    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9a-fA-F]{64}$')]
    [string]$ExpectedEfiSha256,
    [string]$QemuPath,
    [string]$FirmwareCodePath,
    [string]$FirmwareVariablesTemplatePath,
    [ValidateSet('normal', 'invalid-opcode', 'general-protection', 'user-fault', 'service-fault')]
    [string]$Scenario = 'normal',
    [ValidateRange(5, 300)]
    [int]$TimeoutSeconds = 60,
    [switch]$KeepRunDirectory,
    [switch]$PassThru,
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$ExpectedQemuExitCode = if ($Scenario -in @('normal', 'user-fault', 'service-fault')) { 0 } else { 3 }
$ExpectedSerialMarker = switch ($Scenario) {
    'normal' {
        "windvale-os-boot 40`nentry=pass`nsystem-table=pass`nmemory-map=pass`nboot-services=exited`nmemory-owned=pass`nallocator=pass`nkernel-stack=pass`npaging=owned`nwvb-admission=pass`nprocesses=isolated`ndirectory-process=isolated`ndispatcher=ready-wait`nservice-endpoints=2`nresource-grant=pass`ntyped-resources=pass`nresource-revoked=pass`nprocess-reuse=pass`nmemory-object-reuse=pass`nresource-domain=pass current=0 peak=3/144/2`nwvb-runtime=interpreted`ninit-service=pass`ndirectory-service=pass`nipc=resource-and-directory`nHello from Windvale`ntimer-preemption=pass`ncpu-exceptions=armed`nnative-context=pass`nnative-wvb=pass`nwindvale-source=pass`nstatus=pass`nshutdown=poweroff`n"
    }
    'invalid-opcode' {
        "windvale-os-boot 40`nentry=pass`nsystem-table=pass`nmemory-map=pass`nboot-services=exited`nmemory-owned=pass`nallocator=pass`nkernel-stack=pass`npaging=owned`nwvb-admission=pass`nprocesses=isolated`ndirectory-process=isolated`ndispatcher=ready-wait`nservice-endpoints=2`nresource-grant=pass`ntyped-resources=pass`nresource-revoked=pass`nprocess-reuse=pass`nmemory-object-reuse=pass`nresource-domain=pass current=0 peak=3/144/2`nwvb-runtime=interpreted`ninit-service=pass`ndirectory-service=pass`nipc=resource-and-directory`nHello from Windvale`npanic=invalid-opcode`nvector=6`nerror-code=0`nstatus=panic`n"
    }
    'general-protection' {
        "windvale-os-boot 40`nentry=pass`nsystem-table=pass`nmemory-map=pass`nboot-services=exited`nmemory-owned=pass`nallocator=pass`nkernel-stack=pass`npaging=owned`nwvb-admission=pass`nprocesses=isolated`ndirectory-process=isolated`ndispatcher=ready-wait`nservice-endpoints=2`nresource-grant=pass`ntyped-resources=pass`nresource-revoked=pass`nprocess-reuse=pass`nmemory-object-reuse=pass`nresource-domain=pass current=0 peak=3/144/2`nwvb-runtime=interpreted`ninit-service=pass`ndirectory-service=pass`nipc=resource-and-directory`nHello from Windvale`npanic=general-protection`nvector=13`nerror-code=0`nstatus=panic`n"
    }
    'user-fault' {
        "windvale-os-boot 40`nentry=pass`nsystem-table=pass`nmemory-map=pass`nboot-services=exited`nmemory-owned=pass`nallocator=pass`nkernel-stack=pass`npaging=owned`nwvb-admission=pass`nprocesses=isolated`ndirectory-process=isolated`ndispatcher=ready-wait`nservice-endpoints=2`nresource-grant=pass`ntyped-resources=pass`nresource-revoked=pass`nprocess-reuse=pass`nmemory-object-reuse=pass`nwvb-runtime=interpreted`ninit-service=pass`ndirectory-service=pass`nipc=resource-and-directory`nHello from Windvale`ntimer-preemption=pass`ncpu-exceptions=armed`nnative-context=pass`nnative-wvb=pass`nwindvale-source=pass`nuser-fault=contained`nstatus=pass`nshutdown=poweroff`n"
    }
    'service-fault' {
        "windvale-os-boot 40`nentry=pass`nsystem-table=pass`nmemory-map=pass`nboot-services=exited`nmemory-owned=pass`nallocator=pass`nkernel-stack=pass`npaging=owned`nwvb-admission=pass`nprocesses=isolated`ndirectory-process=isolated`ndispatcher=ready-wait`nservice-endpoints=2`nresource-grant=pass`ntyped-resources=pass`nresource-revoked=pass`nwvb-runtime=interpreted`nservice-fault=contained`nipc=service-peer-loss`nHello from Windvale`ntimer-preemption=pass`ncpu-exceptions=armed`nnative-context=pass`nnative-wvb=pass`nwindvale-source=pass`nstatus=pass`nshutdown=poweroff`n"
    }
}
$OppositeTerminalMarker = if ($Scenario -in @('normal', 'user-fault', 'service-fault')) {
    "status=panic`n"
} else {
    "status=pass`n"
}
$OtherFaultMarker = switch ($Scenario) {
    'invalid-opcode' { "panic=general-protection`n" }
    'general-protection' { "panic=invalid-opcode`n" }
    'user-fault' { 'panic=' }
    'service-fault' { 'panic=' }
    default { $null }
}

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

$InputEfiPath = [IO.Path]::GetFullPath($EfiPath)
if (!(Test-Path -LiteralPath $InputEfiPath -PathType Leaf)) {
    Fail-Boot 'WVOS3001' "The supplied EFI application is unavailable: $InputEfiPath"
}
$ExpectedEfiSha256 = $ExpectedEfiSha256.ToLowerInvariant()
$InputEfiIdentity = Get-Item -LiteralPath $InputEfiPath
$InputEfiSha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $InputEfiPath).Hash.ToLowerInvariant()
if ($InputEfiSha256 -ne $ExpectedEfiSha256) {
    Fail-Boot 'WVOS3001' 'The supplied EFI application does not match its expected SHA-256 identity.'
}

$RunDirectory = Join-Path (
    [IO.Path]::GetFullPath([IO.Path]::GetTempPath())) (
    'windvale-os-boot-' + [guid]::NewGuid().ToString('N'))
$BootRoot = Join-Path $RunDirectory 'boot'
$EfiDirectory = Join-Path $BootRoot 'EFI/BOOT'
$RunEfiPath = Join-Path $EfiDirectory 'BOOTX64.EFI'
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
    Copy-Item -LiteralPath $InputEfiPath -Destination $RunEfiPath
    $EfiIdentity = Get-Item -LiteralPath $RunEfiPath
    $EfiSha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $RunEfiPath).Hash.ToLowerInvariant()
    if ($EfiIdentity.Length -ne $InputEfiIdentity.Length -or $EfiSha256 -ne $ExpectedEfiSha256) {
        Fail-Boot 'WVOS3001' 'The run-private EFI application does not match the admitted input.'
    }

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
    if ($Process.ExitCode -ne $ExpectedQemuExitCode) {
        Fail-Boot 'WVOS3004' "QEMU exited with code $($Process.ExitCode); expected $ExpectedQemuExitCode for scenario '$Scenario'."
    }
    if (!(Test-Path -LiteralPath $SerialPath -PathType Leaf)) {
        Fail-Boot 'WVOS3005' 'QEMU produced no serial evidence.'
    }
    $Serial = [IO.File]::ReadAllText($SerialPath, [Text.Encoding]::ASCII)
    $MarkerIndex = $Serial.IndexOf($ExpectedSerialMarker, [StringComparison]::Ordinal)
    if ($MarkerIndex -lt 0) {
        Fail-Boot 'WVOS3005' "The serial evidence does not contain the complete '$Scenario' marker."
    }
    if ($Serial.LastIndexOf($ExpectedSerialMarker, [StringComparison]::Ordinal) -ne $MarkerIndex) {
        Fail-Boot 'WVOS3005' "The serial evidence contains the '$Scenario' marker more than once."
    }
    if ($Serial.Contains($OppositeTerminalMarker, [StringComparison]::Ordinal)) {
        Fail-Boot 'WVOS3005' "The serial evidence contains the opposite terminal marker for scenario '$Scenario'."
    }
    if ($null -ne $OtherFaultMarker -and $Serial.Contains($OtherFaultMarker, [StringComparison]::Ordinal)) {
        Fail-Boot 'WVOS3005' "The serial evidence contains the other fault marker for scenario '$Scenario'."
    }

    if (!(Test-Path -LiteralPath $RunEfiPath -PathType Leaf)) {
        Fail-Boot 'WVOS3006' 'The run-private EFI application disappeared during the boot run.'
    }
    if (!(Test-Path -LiteralPath $InputEfiPath -PathType Leaf)) {
        Fail-Boot 'WVOS3006' 'The supplied EFI application disappeared during the boot run.'
    }
    $EfiHashAfter = (Get-FileHash -Algorithm SHA256 -LiteralPath $RunEfiPath).Hash.ToLowerInvariant()
    $InputEfiHashAfter = (Get-FileHash -Algorithm SHA256 -LiteralPath $InputEfiPath).Hash.ToLowerInvariant()
    $VariablesTemplateHashAfter = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $Environment.FirmwareVariablesTemplatePath
    ).Hash.ToLowerInvariant()
    $FirmwareCodeHashAfter = (
        Get-FileHash -Algorithm SHA256 -LiteralPath $Environment.FirmwareCodePath
    ).Hash.ToLowerInvariant()
    if (
        $EfiHashAfter -ne $EfiSha256 -or
        $InputEfiHashAfter -ne $InputEfiSha256 -or
        $VariablesTemplateHashAfter -ne $VariablesTemplateHashBefore -or
        $FirmwareCodeHashAfter -ne $FirmwareCodeHashBefore
    ) {
        Fail-Boot 'WVOS3006' 'An EFI application or installed firmware input changed during the boot run.'
    }

    $Report = [pscustomobject][ordered]@{
        Status = 'pass'
        Scenario = $Scenario
        Architecture = 'x86-64'
        ApplicationFormat = 'pe32-plus-uefi-application-v3'
        ProbeVersion = 40
        EfiBytes = $EfiIdentity.Length
        EfiSha256 = $EfiSha256
        SerialMarker = switch ($Scenario) {
            'normal' {
                'windvale-os-boot-40-entry-system-table-memory-map-boot-services-exited-memory-owned-allocator-kernel-stack-paging-owned-wvb-admission-processes-isolated-directory-process-isolated-dispatcher-ready-wait-service-endpoints-2-resource-grant-pass-typed-resources-pass-resource-revoked-pass-process-reuse-pass-memory-object-reuse-pass-resource-domain-pass-current-0-peak-3-144-2-wvb-runtime-interpreted-init-service-pass-directory-service-pass-ipc-resource-and-directory-hello-timer-preemption-pass-cpu-exceptions-armed-native-context-native-wvb-windvale-source-status-pass-shutdown-poweroff'
            }
            'invalid-opcode' {
                'windvale-os-boot-40-entry-system-table-memory-map-boot-services-exited-memory-owned-allocator-kernel-stack-paging-owned-wvb-admission-processes-isolated-directory-process-isolated-dispatcher-ready-wait-service-endpoints-2-resource-grant-pass-typed-resources-pass-resource-revoked-pass-process-reuse-pass-memory-object-reuse-pass-resource-domain-pass-current-0-peak-3-144-2-wvb-runtime-interpreted-init-service-pass-directory-service-pass-ipc-resource-and-directory-hello-panic-invalid-opcode-vector-6-error-code-0-status-panic'
            }
            'general-protection' {
                'windvale-os-boot-40-entry-system-table-memory-map-boot-services-exited-memory-owned-allocator-kernel-stack-paging-owned-wvb-admission-processes-isolated-directory-process-isolated-dispatcher-ready-wait-service-endpoints-2-resource-grant-pass-typed-resources-pass-resource-revoked-pass-process-reuse-pass-memory-object-reuse-pass-resource-domain-pass-current-0-peak-3-144-2-wvb-runtime-interpreted-init-service-pass-directory-service-pass-ipc-resource-and-directory-hello-panic-general-protection-vector-13-error-code-0-status-panic'
            }
            'user-fault' {
                'windvale-os-boot-40-entry-system-table-memory-map-boot-services-exited-memory-owned-allocator-kernel-stack-paging-owned-wvb-admission-processes-isolated-directory-process-isolated-dispatcher-ready-wait-service-endpoints-2-resource-grant-pass-typed-resources-pass-resource-revoked-pass-process-reuse-pass-memory-object-reuse-pass-wvb-runtime-interpreted-init-service-pass-directory-service-pass-ipc-resource-and-directory-hello-timer-preemption-pass-cpu-exceptions-armed-native-context-native-wvb-windvale-source-user-fault-contained-status-pass-shutdown-poweroff'
            }
            'service-fault' {
                'windvale-os-boot-40-entry-system-table-memory-map-boot-services-exited-memory-owned-allocator-kernel-stack-paging-owned-wvb-admission-processes-isolated-directory-process-isolated-dispatcher-ready-wait-service-endpoints-2-resource-grant-pass-typed-resources-pass-resource-revoked-pass-wvb-runtime-interpreted-service-fault-contained-ipc-service-peer-loss-hello-timer-preemption-pass-cpu-exceptions-armed-native-context-native-wvb-windvale-source-status-pass-shutdown-poweroff'
            }
        }
        QemuExitCode = $Process.ExitCode
        RunDirectory = if ($KeepRunDirectory) { $RunDirectory } else { $null }
    }

    if ($PassThru) {
        $Report
    } elseif (!$Quiet) {
        Write-Output 'windvale-os-boot-report 40'
        Write-Output "status=$($Report.Status)"
        Write-Output "scenario=$($Report.Scenario)"
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
