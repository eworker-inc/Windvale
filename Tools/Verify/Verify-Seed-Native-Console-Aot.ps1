[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (!(Test-Path -LiteralPath $OutputDirectory -PathType Container)) {
    throw 'The native Seed console AOT output directory must already exist.'
}
$OutputRoot = (Resolve-Path -LiteralPath $OutputDirectory).Path

$NativeLower = Join-Path $RepositoryRoot 'Tools/Native/Lower-Wvb-To-Wvo.cmd'
$NativeVerify = Join-Path $RepositoryRoot 'Tools/Native/Verify-Wvo.cmd'
$NativeLink = Join-Path $RepositoryRoot 'Tools/Native/Link-Wvo.cmd'
$NativePackage = Join-Path $RepositoryRoot 'Tools/Native/Package-Console.cmd'
$SumModule = Join-Path $OutputRoot 'Sum-Data.wvb'
$WindowsApplication = Join-Path $OutputRoot 'Sum-Data-Windows.exe'
$LinuxApplication = Join-Path $OutputRoot 'Sum-Data-Linux.elf'

function Assert-File(
    [string]$Path,
    [long]$ExpectedBytes,
    [string]$ExpectedSha256,
    [string]$Label
) {
    $Information = Get-Item -LiteralPath $Path
    $Digest = (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
    if ($Information.Length -ne $ExpectedBytes -or $Digest -ne $ExpectedSha256) {
        throw "The native Seed $Label identity is invalid."
    }
}

Assert-File `
    $SumModule `
    494 `
    '76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df' `
    'input WVB'

$TemporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$TemporaryDirectory = Join-Path `
    $TemporaryRoot `
    "windvale-seed-console-aot-$PID-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $TemporaryDirectory | Out-Null
try {
    $Wvo = Join-Path $TemporaryDirectory 'Sum-Data.wvo'
    $Image = Join-Path $TemporaryDirectory 'Sum-Data.bin'

    $LowerOutput = @(& $NativeLower $SumModule $Wvo 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $LowerOutput.Count -ne 1 -or
        $LowerOutput[0] -ne
            'native x64 status=Valid abi=22 code-bytes=3088 object-bytes=3288'
    ) {
        throw 'The native Seed Sum-Data lowering report is invalid.'
    }
    Assert-File `
        $Wvo `
        3288 `
        '4e4958f8f0d611e00e912b925b837aa968e06f85abb116b721e3d6e9b8eed4e1' `
        'WVO'

    $VerifyOutput = @(& $NativeVerify $Wvo 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $VerifyOutput.Count -ne 2 -or
        $VerifyOutput[0].Length -le 'Verified object: '.Length -or
        !$VerifyOutput[0].StartsWith(
            'Verified object: ',
            [StringComparison]::Ordinal) -or
        $VerifyOutput[1] -ne
            'SHA-256: 4e4958f8f0d611e00e912b925b837aa968e06f85abb116b721e3d6e9b8eed4e1'
    ) {
        throw 'The native Seed Sum-Data WVO verification report is invalid.'
    }

    $ExpectedLinkOutput = @(
        'windvale-link-map 1'
        'target name=flat-x86-64-v1 architecture=x86-64 base-address=0 image-bytes=3104'
        'entry name=Main address=774'
        'image sha256=8185a8893587d8d5a8d0430e53310c5e6725dea30a76073292864b90c5150c8a'
        'inputs count=1'
        'input index=0 sha256=4e4958f8f0d611e00e912b925b837aa968e06f85abb116b721e3d6e9b8eed4e1'
        'sections count=2'
        'section index=0 input=0 source-index=0 kind=code name=.text image-offset=0 address=0 memory-bytes=3088 data-bytes=3088 alignment=16'
        'section index=1 input=0 source-index=1 kind=read-only-data name=.rodata image-offset=3088 address=3088 memory-bytes=16 data-bytes=16 alignment=16'
        'defined-symbols count=3'
        'symbol index=0 input=0 source-index=0 binding=local kind=data name=$data_0000 address=3088 size=16'
        'symbol index=1 input=0 source-index=1 binding=local kind=function name=$function_0000 address=0 size=774'
        'symbol index=2 input=0 source-index=2 binding=export kind=function name=Main address=774 size=2300'
        'imports count=0'
        'relocations count=1'
        'relocation index=0 input=0 source-index=0 kind=relative-i32 patch-offset=2302 patch-address=2302 target=$data_0000 target-input=0 target-source-index=0 target-address=3088 addend=-4 value=782'
    )
    $LinkOutput = @(& $NativeLink 0 Main $Image $Wvo 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        ![Linq.Enumerable]::SequenceEqual(
            [string[]]$LinkOutput,
            [string[]]$ExpectedLinkOutput)
    ) {
        throw 'The native Seed Sum-Data link map is invalid.'
    }
    Assert-File `
        $Image `
        3104 `
        '8185a8893587d8d5a8d0430e53310c5e6725dea30a76073292864b90c5150c8a' `
        'flat image'

    $WindowsPackageOutput = @(
        & $NativePackage windows-x64-console-v1 $Image 774 $WindowsApplication 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $WindowsPackageOutput.Count -ne 1 -or
        $WindowsPackageOutput[0] -ne
            'package status=Valid target=windows-x64-console-v1 native-image-bytes=3104 entry-offset=774 application-bytes=5120'
    ) {
        throw 'The native Seed Windows console package report is invalid.'
    }
    Assert-File `
        $WindowsApplication `
        5120 `
        '5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77' `
        'Windows application'

    $LinuxPackageOutput = @(
        & $NativePackage linux-x64-console-v1 $Image 774 $LinuxApplication 2>&1)
    if (
        $LASTEXITCODE -ne 0 -or
        $LinuxPackageOutput.Count -ne 1 -or
        $LinuxPackageOutput[0] -ne
            'package status=Valid target=linux-x64-console-v1 native-image-bytes=3104 entry-offset=774 application-bytes=8304'
    ) {
        throw 'The native Seed Linux console package report is invalid.'
    }
    Assert-File `
        $LinuxApplication `
        8304 `
        '8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4' `
        'Linux application'

    & $WindowsApplication
    if ($LASTEXITCODE -ne 29) {
        throw "The native Seed Windows console application returned $LASTEXITCODE instead of 29."
    }
} finally {
    $ResolvedTemporary = [IO.Path]::GetFullPath($TemporaryDirectory)
    if (!$ResolvedTemporary.StartsWith($TemporaryRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Refusing to remove an unexpected native Seed AOT temporary directory.'
    }
    Remove-Item -LiteralPath $ResolvedTemporary -Recurse -Force
}

$global:LASTEXITCODE = 0
Write-Output 'native Seed console AOT verification status=Complete artifacts=2 cases=1'
