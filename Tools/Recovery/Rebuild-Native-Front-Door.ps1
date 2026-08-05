[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot '..' '..'))
$CanonicalRoot = Join-Path $RepositoryRoot 'Artifacts' 'Native-Front-Door'
$Destination = [System.IO.Path]::GetFullPath($OutputDirectory)
if ([System.StringComparer]::OrdinalIgnoreCase.Equals(
        $CanonicalRoot,
        $Destination)) {
    throw 'Recovery reconstruction must use a separate output directory.'
}

$Directories = @(
    (Join-Path $Destination 'Wvb')
    (Join-Path $Destination 'windows-x64')
    (Join-Path $Destination 'linux-x64')
)
New-Item -ItemType Directory -Force -Path $Directories | Out-Null

dotnet build (Join-Path $RepositoryRoot 'Tools/Windvale.Tool/Windvale.Tool.csproj') `
    -c Release --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$Tool = Join-Path $RepositoryRoot 'Tools/Windvale.Tool/bin/Release/net10.0/windvale.exe'

& $Tool build (Join-Path $RepositoryRoot 'Windvale-Compiler-Build-Driver.wvproj') `
    -o (Join-Path $Destination 'Wvb/Compiler-Build-Driver.wvb')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $Tool aot (Join-Path $Destination 'Wvb/Compiler-Build-Driver.wvb') `
    --target windows-x64-build-driver-v1 `
    -o (Join-Path $Destination 'windows-x64/wvbuild.exe')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $Tool aot (Join-Path $Destination 'Wvb/Compiler-Build-Driver.wvb') `
    --target linux-x64-build-driver-v1 `
    -o (Join-Path $Destination 'linux-x64/wvbuild.elf')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $Tool build (Join-Path $RepositoryRoot 'Windvale-Wvb-Publisher.wvproj') `
    -o (Join-Path $Destination 'Wvb/Wvb-Publisher.wvb')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $Tool aot (Join-Path $Destination 'Wvb/Wvb-Publisher.wvb') `
    --target windows-x64-wvb-publisher-v1 `
    -o (Join-Path $Destination 'windows-x64/wvpublish.exe')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $Tool aot (Join-Path $Destination 'Wvb/Wvb-Publisher.wvb') `
    --target linux-x64-wvb-publisher-v1 `
    -o (Join-Path $Destination 'linux-x64/wvpublish.elf')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$Manifest = Get-Content -LiteralPath (Join-Path $CanonicalRoot 'Manifest.json') `
    -Raw | ConvertFrom-Json
foreach ($Artifact in $Manifest.artifacts) {
    $Path = Join-Path $Destination $Artifact.path
    $Info = Get-Item -LiteralPath $Path
    $Digest = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
    if ($Info.Length -ne [long]$Artifact.bytes -or
        -not [System.StringComparer]::OrdinalIgnoreCase.Equals(
            $Digest,
            [string]$Artifact.sha256)) {
        throw "Recovered native-front-door artifact differs: $($Artifact.path)"
    }
}
Write-Output "Recovered native front door: $Destination"
