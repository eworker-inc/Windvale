[CmdletBinding()]
param(
    [string]$InstallRoot,
    [switch]$AddToPath
)

$ErrorActionPreference = 'Stop'
$Version = '@@VERSION@@'
$Target = '@@TARGET@@'
$Payload = '@@PAYLOAD_SHA256@@'
$Generation = '@@GENERATION@@'
$PackageRoot = [IO.Path]::GetFullPath($PSScriptRoot)
$LocalApplicationData = [Environment]::GetFolderPath(
    [Environment+SpecialFolder]::LocalApplicationData)
$UserProfile = [Environment]::GetFolderPath(
    [Environment+SpecialFolder]::UserProfile)
if (!$InstallRoot) {
    if (!$LocalApplicationData) {
        throw 'The per-user local application data directory is unavailable.'
    }
    $InstallRoot = Join-Path $LocalApplicationData 'Windvale'
}
$InstallRoot = [IO.Path]::GetFullPath($InstallRoot)
$InstallDriveRoot = [IO.Path]::GetPathRoot($InstallRoot)
foreach ($ProtectedRoot in @($InstallDriveRoot, $UserProfile, $LocalApplicationData)) {
    if (!$ProtectedRoot) { continue }
    $ProtectedRoot = [IO.Path]::GetFullPath($ProtectedRoot).TrimEnd('\')
    $CandidatePrefix = $InstallRoot.TrimEnd('\') + '\'
    if ($InstallRoot.TrimEnd('\') -eq $ProtectedRoot -or
        $ProtectedRoot.StartsWith($CandidatePrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The installation root is too broad.'
    }
}

Write-Output 'windvale installer step=verify-package item=1/5'
$Verifier = Join-Path $PackageRoot 'bin/wv-verify-installation.ps1'
& $Verifier -Root $PackageRoot -ExpectedTarget $Target -ExpectedPayload $Payload

$ManifestPath = Join-Path $PackageRoot 'Payload-Manifest.txt'
$PayloadFiles = @()
foreach ($Line in [IO.File]::ReadAllLines($ManifestPath) | Select-Object -Skip 3) {
    if ($Line -notmatch '^file [0-9a-f]{64} [0-9]+ 0[0-7]{3} ([A-Za-z0-9._/-]+)$') {
        throw 'The admitted payload manifest changed during installation.'
    }
    $PayloadFiles += $Matches[1]
}

$GenerationsRoot = Join-Path $InstallRoot 'generations'
$GenerationRoot = Join-Path $GenerationsRoot $Generation
$CandidateRoot = Join-Path $GenerationsRoot ".candidate-$Generation-$PID"
New-Item -ItemType Directory -Path $GenerationsRoot -Force | Out-Null
if (Test-Path -LiteralPath $CandidateRoot) {
    throw 'A prior installer candidate needs inspection before retry.'
}

Write-Output 'windvale installer step=publish-generation item=2/5'
if (Test-Path -LiteralPath $GenerationRoot) {
    & (Join-Path $GenerationRoot 'bin/wv-verify-installation.ps1') -Root $GenerationRoot -ExpectedTarget $Target -ExpectedPayload $Payload
} else {
    try {
        New-Item -ItemType Directory -Path $CandidateRoot | Out-Null
        foreach ($RelativePath in $PayloadFiles) {
            $NativeRelativePath = $RelativePath.Replace('/', [IO.Path]::DirectorySeparatorChar)
            $Source = Join-Path $PackageRoot $NativeRelativePath
            $Destination = Join-Path $CandidateRoot $NativeRelativePath
            $DestinationParent = Split-Path -Parent $Destination
            New-Item -ItemType Directory -Path $DestinationParent -Force | Out-Null
            Copy-Item -LiteralPath $Source -Destination $Destination
        }
        Copy-Item -LiteralPath $ManifestPath -Destination (
            Join-Path $CandidateRoot 'Payload-Manifest.txt')
        & (Join-Path $CandidateRoot 'bin/wv-verify-installation.ps1') -Root $CandidateRoot -ExpectedTarget $Target -ExpectedPayload $Payload
        Rename-Item -LiteralPath $CandidateRoot -NewName $Generation
    } finally {
        if (Test-Path -LiteralPath $CandidateRoot) {
            $ResolvedCandidate = [IO.Path]::GetFullPath($CandidateRoot)
            $RequiredPrefix = [IO.Path]::GetFullPath($GenerationsRoot).TrimEnd('\') + '\.candidate-'
            if (!$ResolvedCandidate.StartsWith($RequiredPrefix, [StringComparison]::OrdinalIgnoreCase)) {
                throw 'Refusing to remove an unexpected installer candidate.'
            }
            Remove-Item -LiteralPath $ResolvedCandidate -Recurse -Force
        }
    }
}

Write-Output 'windvale installer step=publish-command-shims item=3/5'
$BinRoot = Join-Path $InstallRoot 'bin'
New-Item -ItemType Directory -Path $BinRoot -Force | Out-Null
$Commands = @('wv', 'wvbuild', 'wvasm', 'wvlink', 'wvrun', 'wvdump', 'wvverify', 'wvpublish')
foreach ($Command in $Commands) {
    $Executable = if ($Command -eq 'wv') { 'wv.cmd' } else { "$Command.exe" }
    $Prefix = if ($Command -eq 'wv') { 'call ' } else { '' }
    $Doctor = if ($Command -eq 'wv') {
        "if /i `"%~1`"==`"doctor`" powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File `"%~dp0..\generations\$Generation\bin\wv-verify-installation.ps1`" -Root `"%~dp0..\generations\$Generation`" -ExpectedTarget `"$Target`" -ExpectedPayload `"$Payload`"`r`n" +
        "if /i `"%~1`"==`"doctor`" exit /b %ERRORLEVEL%`r`n"
    } else { '' }
    $Wrapper = "@echo off`r`n$Doctor$Prefix`"%~dp0..\generations\$Generation\bin\$Executable`" %*`r`nexit /b %ERRORLEVEL%`r`n"
    $WrapperPath = Join-Path $BinRoot "$Command.cmd"
    [IO.File]::WriteAllText($WrapperPath, $Wrapper, [Text.UTF8Encoding]::new($false))
}

Write-Output 'windvale installer step=record-installation item=4/5'
$InstallationsRoot = Join-Path $InstallRoot 'installations'
New-Item -ItemType Directory -Path $InstallationsRoot -Force | Out-Null
$Record = "@@INSTALLATION_RECORD@@`nversion $Version`ntarget $Target`ngeneration $Generation`npayload $Payload`n"
[IO.File]::WriteAllText(
    (Join-Path $InstallationsRoot "$Generation.txt"),
    $Record,
    [Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath (Join-Path $PackageRoot 'Uninstall-Windvale.ps1') -Destination (Join-Path $InstallRoot 'Uninstall-Windvale.ps1') -Force

Write-Output 'windvale installer step=finish item=5/5'
if ($AddToPath) {
    $UserPath = [Environment]::GetEnvironmentVariable('Path', 'User')
    $Entries = @($UserPath -split ';' | Where-Object { $_ })
    if (!($Entries | Where-Object { $_.TrimEnd('\') -ieq $BinRoot.TrimEnd('\') })) {
        $UpdatedPath = if ($UserPath) { "$UserPath;$BinRoot" } else { $BinRoot }
        [Environment]::SetEnvironmentVariable('Path', $UpdatedPath, 'User')
    }
    Write-Output 'Open a new terminal to use the updated per-user PATH.'
} else {
    Write-Output "Add this directory to PATH when desired: $BinRoot"
}
Write-Output "windvale installer status=Installed version=$Version target=$Target generation=$Generation root=$InstallRoot"
