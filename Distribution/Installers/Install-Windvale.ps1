[CmdletBinding()]
param(
    [string]$InstallRoot,
    [switch]$NoPath
)

$ErrorActionPreference = 'Stop'

& {
    param(
        [string]$RequestedInstallRoot,
        [bool]$PublishPath
    )

    $Version = '0.1.0'
    $Target = 'windows-x64'
    $ArchiveName = "windvale-$Version-$Target.zip"
    $ArchiveSha256 = '8e6e5dcd16ae437933e0eab739e84f5c48bf1d4045089495dccdef7f2de7deee'
    $DownloadUrl = "https://github.com/eworker-inc/Windvale/releases/download/v$Version/$ArchiveName"
    $TemporaryParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
    $TemporaryRoot = Join-Path $TemporaryParent ("windvale-install-$PID-" + [Guid]::NewGuid().ToString('N'))

    try {
        New-Item -ItemType Directory -Path $TemporaryRoot | Out-Null
        $ArchivePath = Join-Path $TemporaryRoot $ArchiveName

        Write-Output 'windvale bootstrap step=download item=1/5'
        Invoke-WebRequest -Uri $DownloadUrl -OutFile $ArchivePath

        Write-Output 'windvale bootstrap step=verify-download item=2/5'
        $ObservedSha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $ArchivePath).Hash.ToLowerInvariant()
        if ($ObservedSha256 -ne $ArchiveSha256) {
            throw 'The downloaded Windvale installer SHA-256 differs.'
        }

        Write-Output 'windvale bootstrap step=extract item=3/5'
        $ExtractRoot = Join-Path $TemporaryRoot 'extracted'
        Expand-Archive -LiteralPath $ArchivePath -DestinationPath $ExtractRoot
        $PackageRoot = Join-Path $ExtractRoot "windvale-$Version-$Target"
        $Installer = Join-Path $PackageRoot 'Install-Windvale.ps1'
        if (!(Test-Path -LiteralPath $Installer -PathType Leaf)) {
            throw 'The downloaded Windvale installer entry point is missing.'
        }
        Unblock-File -LiteralPath $Installer

        Write-Output 'windvale bootstrap step=install item=4/5'
        $InstallerArguments = @{}
        if ($RequestedInstallRoot) { $InstallerArguments.InstallRoot = $RequestedInstallRoot }
        if ($PublishPath) { $InstallerArguments.AddToPath = $true }
        & $Installer @InstallerArguments

        $ResolvedInstallRoot = if ($RequestedInstallRoot) {
            [IO.Path]::GetFullPath($RequestedInstallRoot)
        } else {
            $LocalApplicationData = [Environment]::GetFolderPath(
                [Environment+SpecialFolder]::LocalApplicationData)
            if (!$LocalApplicationData) {
                throw 'The per-user local application data directory is unavailable.'
            }
            Join-Path $LocalApplicationData 'Windvale'
        }
        $BinRoot = Join-Path $ResolvedInstallRoot 'bin'

        Write-Output 'windvale bootstrap step=activate-path item=5/5'
        if ($PublishPath) {
            $ProcessEntries = @($env:Path -split ';' | Where-Object { $_ })
            if (!($ProcessEntries | Where-Object { $_.TrimEnd('\') -ieq $BinRoot.TrimEnd('\') })) {
                $env:Path = if ($env:Path) { "$BinRoot;$env:Path" } else { $BinRoot }
            }
            Write-Output 'The current PowerShell session and future user sessions can now use Windvale.'
        } else {
            Write-Output "PATH publication was skipped. Windvale commands are in: $BinRoot"
        }
        Write-Output "windvale bootstrap status=Installed version=$Version target=$Target root=$ResolvedInstallRoot"
    } finally {
        if (Test-Path -LiteralPath $TemporaryRoot) {
            $ResolvedTemporaryRoot = [IO.Path]::GetFullPath($TemporaryRoot)
            $ResolvedParent = [IO.Path]::GetDirectoryName($ResolvedTemporaryRoot).TrimEnd('\')
            $Leaf = [IO.Path]::GetFileName($ResolvedTemporaryRoot)
            if ($ResolvedParent -ne $TemporaryParent -or !$Leaf.StartsWith('windvale-install-')) {
                throw "Refusing to remove an unexpected temporary directory: $ResolvedTemporaryRoot"
            }
            Remove-Item -LiteralPath $ResolvedTemporaryRoot -Recurse -Force
        }
    }
} $InstallRoot (!$NoPath)
