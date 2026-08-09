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
$NativeLinker = if ($IsWindows) {
    Join-Path $RepositoryRoot 'Tools/Native/Link-Wvo.cmd'
} elseif ($IsLinux) {
    Join-Path $RepositoryRoot 'Tools/Native/Link-Wvo.sh'
} else {
    Fail-Recovery 'The native WVO linker supports only Windows and Linux.'
}
$NativeAssembler = if ($IsWindows) {
    Join-Path $RepositoryRoot 'Tools/Native/Assemble-Wva.cmd'
} else {
    Join-Path $RepositoryRoot 'Tools/Native/Assemble-Wva.sh'
}
$NativePackager = if ($IsWindows) {
    Join-Path $RepositoryRoot 'Tools/Native/Package-Uefi.cmd'
} else {
    Join-Path $RepositoryRoot 'Tools/Native/Package-Uefi.sh'
}
if (!(Test-Path -LiteralPath $NativeLinker -PathType Leaf)) {
    Fail-Recovery 'The digest-bound native WVO linker launcher is unavailable.'
}
if (!(Test-Path -LiteralPath $NativeAssembler -PathType Leaf)) {
    Fail-Recovery 'The digest-bound native WVA assembler launcher is unavailable.'
}
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
$ObjectDirectory = Join-Path $OutputDirectory (
    '.windvale-os-probe-objects-' + $CandidateToken)
try {
    $null = New-Item -ItemType Directory -Path $ObjectDirectory
    $BuilderOutput = @(
        & $Dotnet.Source run `
            --project $BuilderProject `
            --configuration Release `
            -- `
            --object-directory-native-wva $ObjectDirectory `
            --scenario $Scenario 2>&1
    )
    if ($LASTEXITCODE -ne 0) {
        $BuilderMessage = ($BuilderOutput | ForEach-Object { $_.ToString() }) -join ' '
        Fail-Recovery "The Stage 0 firmware-probe object build failed. $BuilderMessage"
    }

    $BuilderLines = @($BuilderOutput | ForEach-Object { $_.ToString() })
    if (@($BuilderLines | Where-Object {
        $_ -eq 'windvale-os-probe-native-wva-inventory 40'
    }).Count -ne 1) {
        Fail-Recovery 'The Stage 0 object builder did not report the native-WVA inventory format.'
    }
    $EntryLines = @($BuilderLines | Where-Object { $_ -like 'entry-symbol=*' })
    if ($EntryLines.Count -ne 1 -or
        $EntryLines[0] -notmatch '^entry-symbol=([A-Za-z_.$][A-Za-z0-9_.$]*)$') {
        Fail-Recovery 'The Stage 0 object builder did not report one machine entry symbol.'
    }
    $EntrySymbol = $Matches[1]
    $CountLines = @($BuilderLines | Where-Object { $_ -like 'object-count=*' })
    if ($CountLines.Count -ne 1 -or $CountLines[0] -ne 'object-count=11') {
        Fail-Recovery 'The Stage 0 object builder did not report the reviewed 11-object inventory.'
    }
    $ObjectLines = @($BuilderLines | Where-Object { $_ -like 'object=*' })
    $StageZeroObjectNames = @(
        '00-loader.wvo',
        '01-kernel.wvo',
        '02-wvb-admission-native.wvo',
        '03-native-wvb-probe.wvo',
        '04-process-policy.wvo',
        '05-process.wvo',
        '08-memory.wvo',
        '09-exceptions.wvo',
        '10-paging.wvo',
        '12-wvb-admission-bridge.wvo',
        '13-native-bridge-and-support.wvo'
    )
    $ReportedObjectNames = @($ObjectLines | ForEach-Object {
        $_.Substring('object='.Length)
    })
    if ($ReportedObjectNames.Count -ne $StageZeroObjectNames.Count -or
        (Compare-Object $StageZeroObjectNames $ReportedObjectNames -SyncWindow 0)) {
        Fail-Recovery 'The Stage 0 object builder did not report the reviewed ordered object names.'
    }

    foreach ($ObjectName in $StageZeroObjectNames) {
        $ObjectPath = Join-Path $ObjectDirectory $ObjectName
        if (!(Test-Path -LiteralPath $ObjectPath -PathType Leaf)) {
            Fail-Recovery "The Stage 0 object builder did not publish '$ObjectName'."
        }
    }

    $NativeWvaObjects = @(
        @(
            'Operating-System/Kernel/X64-Memory-Object-Shims.wva',
            '06-memory-object-shims.wvo',
            'fe0a94461b743be58319d2e2f8b737840ec1216e61a98ee7e210f96f97f85bee'
        ),
        @(
            'Operating-System/Kernel/X64-Timer-Shims.wva',
            '07-timer-shims.wvo',
            'e331a1db404b8b8359d35d410792496683a63acee621ff64f128a6eae128c344'
        ),
        @(
            'Operating-System/Kernel/X64-Kernel-Shims.wva',
            '11-kernel-shims.wvo',
            '845d45d6787ec819ca300ffc81a9ffe3e86c7b3998f3dd2a50a017a353d86193'
        )
    )
    foreach ($NativeWvaObject in $NativeWvaObjects) {
        $SourcePath = Join-Path $RepositoryRoot $NativeWvaObject[0]
        $ObjectPath = Join-Path $ObjectDirectory $NativeWvaObject[1]
        $AssemblerOutput = @(& $NativeAssembler $SourcePath $ObjectPath 2>&1)
        if ($LASTEXITCODE -ne 0 -or !(Test-Path -LiteralPath $ObjectPath -PathType Leaf)) {
            $AssemblerMessage = ($AssemblerOutput | ForEach-Object { $_.ToString() }) -join ' '
            Fail-Recovery "The native WVA assembly step failed. $AssemblerMessage"
        }
        $ObjectSha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $ObjectPath).Hash.ToLowerInvariant()
        if ($ObjectSha256 -cne $NativeWvaObject[2]) {
            Fail-Recovery "The native WVA object '$($NativeWvaObject[1])' has an unexpected digest."
        }
    }

    $ObjectPaths = @(
        '00-loader.wvo',
        '01-kernel.wvo',
        '02-wvb-admission-native.wvo',
        '03-native-wvb-probe.wvo',
        '04-process-policy.wvo',
        '05-process.wvo',
        '06-memory-object-shims.wvo',
        '07-timer-shims.wvo',
        '08-memory.wvo',
        '09-exceptions.wvo',
        '10-paging.wvo',
        '11-kernel-shims.wvo',
        '12-wvb-admission-bridge.wvo',
        '13-native-bridge-and-support.wvo'
    ) | ForEach-Object { Join-Path $ObjectDirectory $_ }

    $LinkerOutput = @(
        & $NativeLinker 0 $EntrySymbol $LinkedPath @ObjectPaths 2>&1
    )
    if ($LASTEXITCODE -ne 0 -or !(Test-Path -LiteralPath $LinkedPath -PathType Leaf)) {
        $LinkerMessage = ($LinkerOutput | ForEach-Object { $_.ToString() }) -join ' '
        Fail-Recovery "The native WVO linking step failed. $LinkerMessage"
    }
    $EscapedEntrySymbol = [Regex]::Escape($EntrySymbol)
    $NativeEntryLines = @($LinkerOutput | ForEach-Object { $_.ToString() } |
        Where-Object { $_ -match "^entry name=$EscapedEntrySymbol address=([0-9]+)$" })
    if ($NativeEntryLines.Count -ne 1 -or
        $NativeEntryLines[0] -notmatch "^entry name=$EscapedEntrySymbol address=([0-9]+)$") {
        Fail-Recovery 'The native WVO linker did not report one decimal entry address.'
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
    if (Test-Path -LiteralPath $ObjectDirectory) {
        Remove-Item -LiteralPath $ObjectDirectory -Recurse -Force
    }
}
