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
$NativePackager = if ($IsWindows) {
    Join-Path $RepositoryRoot 'Tools/Native/Package-Uefi.cmd'
} else {
    Join-Path $RepositoryRoot 'Tools/Native/Package-Uefi.sh'
}
if (!(Test-Path -LiteralPath $NativeLinker -PathType Leaf)) {
    Fail-Recovery 'The digest-bound native WVO linker launcher is unavailable.'
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
            --object-directory $ObjectDirectory `
            --scenario $Scenario 2>&1
    )
    if ($LASTEXITCODE -ne 0) {
        $BuilderMessage = ($BuilderOutput | ForEach-Object { $_.ToString() }) -join ' '
        Fail-Recovery "The Stage 0 firmware-probe object build failed. $BuilderMessage"
    }

    $BuilderLines = @($BuilderOutput | ForEach-Object { $_.ToString() })
    if (@($BuilderLines | Where-Object {
        $_ -eq 'windvale-os-probe-object-inventory 40'
    }).Count -ne 1) {
        Fail-Recovery 'The Stage 0 object builder did not report the Probe 40 inventory format.'
    }
    $EntryLines = @($BuilderLines | Where-Object { $_ -like 'entry-symbol=*' })
    if ($EntryLines.Count -ne 1 -or
        $EntryLines[0] -notmatch '^entry-symbol=([A-Za-z_.$][A-Za-z0-9_.$]*)$') {
        Fail-Recovery 'The Stage 0 object builder did not report one machine entry symbol.'
    }
    $EntrySymbol = $Matches[1]
    $CountLines = @($BuilderLines | Where-Object { $_ -like 'object-count=*' })
    if ($CountLines.Count -ne 1 -or $CountLines[0] -ne 'object-count=14') {
        Fail-Recovery 'The Stage 0 object builder did not report the reviewed 14-object inventory.'
    }
    $ObjectLines = @($BuilderLines | Where-Object { $_ -like 'object=*' })
    if ($ObjectLines.Count -ne 14) {
        Fail-Recovery 'The Stage 0 object builder did not report 14 ordered object names.'
    }
    $ObjectPaths = @()
    $ObjectNames = @{}
    foreach ($ObjectLine in $ObjectLines) {
        $ObjectName = $ObjectLine.Substring('object='.Length)
        if ([string]::IsNullOrWhiteSpace($ObjectName) -or
            [IO.Path]::GetFileName($ObjectName) -cne $ObjectName -or
            [IO.Path]::GetExtension($ObjectName) -cne '.wvo' -or
            $ObjectNames.ContainsKey($ObjectName)) {
            Fail-Recovery 'The Stage 0 object builder reported an invalid or duplicate object name.'
        }
        $ObjectNames.Add($ObjectName, $true)
        $ObjectPath = Join-Path $ObjectDirectory $ObjectName
        if (!(Test-Path -LiteralPath $ObjectPath -PathType Leaf)) {
            Fail-Recovery "The Stage 0 object builder did not publish '$ObjectName'."
        }
        $ObjectPaths += $ObjectPath
    }

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
