param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$CanonicalRoot = Join-Path $RepositoryRoot 'Artifacts/Native-Compiler-Seed'
$Destination = [System.IO.Path]::GetFullPath($OutputDirectory)
if ($Destination -eq [System.IO.Path]::GetFullPath($CanonicalRoot)) {
    throw 'Seed reconstruction must use a separate output directory.'
}

$ReconstructionCommit = '3824f39d0997e3d7ab523f7cc1fe0f4bd8288e35'
$SemanticFreezeCommit = '524e84afb6e5bab6bbd95ebc0b9eeaf886af834b'
$TemporaryBase = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$TemporaryPrefix = $TemporaryBase.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
$TemporaryDirectory = Join-Path $TemporaryBase ('windvale-native-compiler-seed-' + [System.Guid]::NewGuid().ToString('N'))
$Reconstructor = Join-Path $TemporaryDirectory 'Reconstructor'
$SeedSource = Join-Path $TemporaryDirectory 'Seed-Source'

function Invoke-Checked {
    param([scriptblock]$Command, [string]$Failure)
    & $Command
    if ($LASTEXITCODE -ne 0) { throw $Failure }
}

function Assert-Artifact {
    param([string]$Path, [long]$Bytes, [string]$Sha256)
    $Item = Get-Item -LiteralPath $Path
    if ($Item.Length -ne $Bytes) { throw "Unexpected artifact length: $Path" }
    $Actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
    if ($Actual -ne $Sha256) { throw "Unexpected artifact digest: $Path" }
}

try {
    New-Item -ItemType Directory -Path $Reconstructor, $SeedSource -Force | Out-Null
    New-Item -ItemType Directory -Path `
        (Join-Path $Destination 'Wvb'), `
        (Join-Path $Destination 'windows-x64'), `
        (Join-Path $Destination 'linux-x64') -Force | Out-Null

    $ReconstructorArchive = Join-Path $TemporaryDirectory 'Reconstructor.tar'
    Invoke-Checked { git -C $RepositoryRoot archive --format=tar --output=$ReconstructorArchive $ReconstructionCommit } `
        'The exact reconstruction source archive could not be created.'
    Invoke-Checked { tar -xf $ReconstructorArchive -C $Reconstructor } `
        'The exact reconstruction source archive could not be extracted.'

    $SeedArchive = Join-Path $TemporaryDirectory 'Seed-Source.tar'
    Invoke-Checked {
        git -C $RepositoryRoot archive --format=tar --output=$SeedArchive $SemanticFreezeCommit -- `
            Projects/Examples/Windvale-Compiler.wvproj `
            Examples/Compiler/Source-Wvb-Tool.wv `
            Compiler/Windvale `
            Foundation/Byte-Construction.wv `
            Foundation/Decimal-Parsing.wv
    } 'The semantic-freeze compiler source archive could not be created.'
    Invoke-Checked { tar -xf $SeedArchive -C $SeedSource } `
        'The semantic-freeze compiler source archive could not be extracted.'

    Push-Location $Reconstructor
    try {
        Invoke-Checked { dotnet build Tools/Windvale.Tool/Windvale.Tool.csproj --configuration Release --nologo --verbosity quiet } `
            'The exact Stage 0 recovery tool could not be built.'
    }
    finally { Pop-Location }

    $Tool = Join-Path $Reconstructor 'Tools/Windvale.Tool/bin/Release/net10.0/windvale.dll'
    $Wvb = Join-Path $Destination 'Wvb/Windvale-Compiler.wvb'
    Push-Location $SeedSource
    try {
        Invoke-Checked { dotnet $Tool build Projects/Examples/Windvale-Compiler.wvproj -o $Wvb } `
            'The semantic-freeze compiler WVB could not be reconstructed.'
    }
    finally { Pop-Location }
    Invoke-Checked { dotnet $Tool aot $Wvb --target windows-x64-console-v3 -o (Join-Path $Destination 'windows-x64/wvcompiler.exe') } `
        'The Windows native compiler seed could not be reconstructed.'
    Invoke-Checked { dotnet $Tool aot $Wvb --target linux-x64-console-v3 -o (Join-Path $Destination 'linux-x64/wvcompiler.elf') } `
        'The Linux native compiler seed could not be reconstructed.'

    Assert-Artifact $Wvb 914746 '48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6'
    Assert-Artifact (Join-Path $Destination 'windows-x64/wvcompiler.exe') 27467776 '344940f66b26b516b8b4e10a712a6b2c01cbff95aa7ff18aac0789ba9197f970'
    Assert-Artifact (Join-Path $Destination 'linux-x64/wvcompiler.elf') 27467776 '2f745e2c4dddb7333926783796f06b6f02ef356742fb5873a2efffdca16c696a'
    Write-Output "Recovered native compiler seed: $Destination"
}
finally {
    $ResolvedTemporary = [System.IO.Path]::GetFullPath($TemporaryDirectory)
    if (-not $ResolvedTemporary.StartsWith($TemporaryPrefix, [System.StringComparison]::OrdinalIgnoreCase) -or
        -not ([System.IO.Path]::GetFileName($ResolvedTemporary)).StartsWith('windvale-native-compiler-seed-', [System.StringComparison]::Ordinal)) {
        throw "Refusing to remove unexpected temporary path: $ResolvedTemporary"
    }
    if (Test-Path -LiteralPath $ResolvedTemporary) {
        Remove-Item -LiteralPath $ResolvedTemporary -Recurse -Force
    }
}
