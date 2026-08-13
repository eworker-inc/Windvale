[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot '..' '..'))
$CanonicalRoot = Join-Path $RepositoryRoot 'Artifacts' 'WebAssembly-Native-Backend'
$Destination = [System.IO.Path]::GetFullPath($OutputDirectory)
if ([System.StringComparer]::OrdinalIgnoreCase.Equals(
        $CanonicalRoot,
        $Destination)) {
    throw 'Recovery reconstruction must use a separate output directory.'
}

New-Item -ItemType Directory -Force -Path @(
    (Join-Path $Destination 'Wvb')
    (Join-Path $Destination 'windows-x64')
    (Join-Path $Destination 'linux-x64')
) | Out-Null

dotnet build (Join-Path $RepositoryRoot 'Tools/Windvale.Tool/Windvale.Tool.csproj') `
    -c Release --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$Tool = Join-Path $RepositoryRoot 'Tools/Windvale.Tool/bin/Release/net10.0/windvale.exe'
$CompilerWvb = Join-Path $Destination 'Wvb/WebAssembly-Compiler.wvb'

& $Tool build (Join-Path $RepositoryRoot 'Projects/Tools/Windvale-WebAssembly-Artifact-Tool.wvproj') `
    -o $CompilerWvb
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $Tool aot $CompilerWvb `
    --target windows-x64-console-v3 `
    -o (Join-Path $Destination 'windows-x64/wvwasm.exe')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $Tool aot $CompilerWvb `
    --target linux-x64-console-v3 `
    -o (Join-Path $Destination 'linux-x64/wvwasm.elf')
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
        throw "Recovered WebAssembly native backend differs: $($Artifact.path)"
    }
}
Write-Output "Recovered WebAssembly native backend: $Destination"
