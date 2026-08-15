[CmdletBinding()]
param(
    [string]$InstallRoot
)

$ErrorActionPreference = 'Stop'
$Version = '@@VERSION@@'
$Target = '@@TARGET@@'
$Payload = '@@PAYLOAD_SHA256@@'
$Generation = '@@GENERATION@@'
if (!$InstallRoot) {
    $LocalApplicationData = [Environment]::GetFolderPath(
        [Environment+SpecialFolder]::LocalApplicationData)
    $InstallRoot = Join-Path $LocalApplicationData 'Windvale'
}
$InstallRoot = [IO.Path]::GetFullPath($InstallRoot)
$DriveRoot = [IO.Path]::GetPathRoot($InstallRoot)
$UserRoot = [IO.Path]::GetFullPath([Environment]::GetFolderPath(
    [Environment+SpecialFolder]::UserProfile))
$LocalApplicationData = [IO.Path]::GetFullPath([Environment]::GetFolderPath(
    [Environment+SpecialFolder]::LocalApplicationData))
foreach ($ProtectedRoot in @($DriveRoot, $UserRoot, $LocalApplicationData)) {
    $ProtectedRoot = $ProtectedRoot.TrimEnd('\')
    $CandidatePrefix = $InstallRoot.TrimEnd('\') + '\'
    if ($InstallRoot.TrimEnd('\') -eq $ProtectedRoot -or
        $ProtectedRoot.StartsWith($CandidatePrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to remove a broad filesystem root.'
    }
}

$RecordPath = Join-Path $InstallRoot "installations/$Generation.txt"
$ExpectedRecord = "@@INSTALLATION_RECORD@@`nversion $Version`ntarget $Target`ngeneration $Generation`npayload $Payload`n"
if (!(Test-Path -LiteralPath $RecordPath -PathType Leaf) -or
    [IO.File]::ReadAllText($RecordPath).Replace("`r`n", "`n") -ne $ExpectedRecord) {
    throw 'The exact @@INSTALLATION_DESCRIPTION@@ record is absent.'
}

$BinRoot = Join-Path $InstallRoot 'bin'
$UserPath = [Environment]::GetEnvironmentVariable('Path', 'User')
$Entries = @($UserPath -split ';' | Where-Object {
    $_ -and $_.TrimEnd('\') -ine $BinRoot.TrimEnd('\')
})
if ($Entries.Count -ne @($UserPath -split ';' | Where-Object { $_ }).Count) {
    [Environment]::SetEnvironmentVariable('Path', ($Entries -join ';'), 'User')
}

Write-Output 'windvale uninstaller step=remove-installation item=1/1'
Remove-Item -LiteralPath $InstallRoot -Recurse -Force
Write-Output "windvale uninstaller status=Removed version=$Version target=$Target root=$InstallRoot"
