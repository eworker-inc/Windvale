param(
    [Parameter(Mandatory = $true)]
    [string] $Destination
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-Checked([scriptblock] $Action, [string] $Label) {
    & $Action
    if ($LASTEXITCODE -ne 0) {
        throw "$Label failed with exit code $LASTEXITCODE."
    }
}

function Assert-File([string] $Path, [long] $Bytes, [string] $Sha256, [string] $Label) {
    $Item = Get-Item -LiteralPath $Path
    if ($Item.Length -ne $Bytes) {
        throw "$Label is $($Item.Length) bytes, expected $Bytes."
    }
    $Actual = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($Actual -ne $Sha256) {
        throw "$Label SHA-256 is $Actual, expected $Sha256."
    }
}

function Write-Link-Map([string] $Path, [object[]] $Lines) {
    [IO.File]::WriteAllLines(
        $Path,
        [string[]]$Lines,
        [Text.UTF8Encoding]::new($false)
    )
}

$RepositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$DestinationRoot = [IO.Path]::GetFullPath($Destination)
$WindowsDestination = Join-Path $DestinationRoot 'windows-x64'
$LinuxDestination = Join-Path $DestinationRoot 'linux-x64'
New-Item -ItemType Directory -Force -Path $WindowsDestination | Out-Null
New-Item -ItemType Directory -Force -Path $LinuxDestination | Out-Null

$TemporaryRoot = [IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) ("windvale-baseline-jit-publisher-" + [Guid]::NewGuid().ToString('N'))))
New-Item -ItemType Directory -Path $TemporaryRoot | Out-Null

$PlanWvo = Join-Path $TemporaryRoot 'Plan.wvo'
$LinuxWvo = Join-Path $TemporaryRoot 'Linux.wvo'
$WindowsWvo = Join-Path $TemporaryRoot 'Windows.wvo'
$LinuxImage = Join-Path $TemporaryRoot 'Linux.bin'
$WindowsImage = Join-Path $TemporaryRoot 'Windows.bin'
$LinuxMap = Join-Path $TemporaryRoot 'Linux.wvmap'
$WindowsMap = Join-Path $TemporaryRoot 'Windows.wvmap'
$LinuxApplication = Join-Path $TemporaryRoot 'Baseline-Jit-Publisher.elf'
$WindowsUnpatched = Join-Path $TemporaryRoot 'Baseline-Jit-Publisher-Unpatched.exe'
$WindowsApplication = Join-Path $TemporaryRoot 'Baseline-Jit-Publisher.exe'

try {
    Invoke-Checked {
        & (Join-Path $RepositoryRoot 'Tools\Native\Assemble-Wva.cmd') `
            (Join-Path $RepositoryRoot 'Runtime\Native\Baseline-Jit-Patch-Plan-X64.wva') `
            $PlanWvo
    } 'shared plan assembly'
    Invoke-Checked {
        & (Join-Path $RepositoryRoot 'Tools\Native\Assemble-Wva.cmd') `
            (Join-Path $RepositoryRoot 'Runtime\Native\Linux-X64-Baseline-Jit-Publisher.wva') `
            $LinuxWvo
    } 'Linux adapter assembly'
    Invoke-Checked {
        & (Join-Path $RepositoryRoot 'Tools\Native\Assemble-Wva.cmd') `
            (Join-Path $RepositoryRoot 'Runtime\Native\Windows-X64-Baseline-Jit-Publisher.wva') `
            $WindowsWvo
    } 'Windows adapter assembly'

    Assert-File $PlanWvo 1196 '9074413259924bb50e8a98ca14690e0ec34a65b28c15f0d27a69799c7071f763' 'shared plan WVO'
    Assert-File $LinuxWvo 1310 'b3cfb37c9d9bf17821673ad04a1e3fcd2a6cbb28d65df59838c56599626867c7' 'Linux adapter WVO'
    Assert-File $WindowsWvo 2429 '3f5069815b01798374b0974f20e8d344b562d1a08797c6f15dc9125373ba18d6' 'Windows adapter WVO'

    $LinuxMapLines = & (Join-Path $RepositoryRoot 'Tools\Native\Link-Wvo.cmd') `
        1048576 Main $LinuxImage $LinuxWvo $PlanWvo
    if ($LASTEXITCODE -ne 0) { throw 'Linux adapter linking failed.' }
    Write-Link-Map $LinuxMap $LinuxMapLines

    $WindowsMapLines = & (Join-Path $RepositoryRoot 'Tools\Native\Link-Wvo.cmd') `
        4208 Main $WindowsImage $WindowsWvo $PlanWvo
    if ($LASTEXITCODE -ne 0) { throw 'Windows adapter linking failed.' }
    Write-Link-Map $WindowsMap $WindowsMapLines

    Assert-File $LinuxImage 1374 '991b6218758fe34514733b5ca71ff98baf61f1ab6103f15dc8c6b4c6b6623902' 'Linux flat image'
    Assert-File $WindowsImage 1714 '43c58d27a733f74fdec15413a2cc649356eade3c0b9f7651b0c8d81d47b219d9' 'Windows flat image'

    Invoke-Checked {
        & (Join-Path $RepositoryRoot 'Tools\Native\Package-Console.cmd') `
            linux-x64-console-v1 $LinuxImage 389 $LinuxApplication
    } 'Linux application packaging'
    Invoke-Checked {
        & (Join-Path $RepositoryRoot 'Tools\Native\Package-Console.cmd') `
            windows-x64-console-v1 $WindowsImage 513 $WindowsUnpatched
    } 'Windows application packaging'

    Assert-File $LinuxApplication 8304 '371f0aaaa5200c5767947892f99376e3c649b86dfa8ae5d78e2474aad4a667ea' 'Linux application'
    Assert-File $WindowsUnpatched 3584 '0c27a724a85daa54fc23a5d7f09e1e6f9344d711080e46aef80cfc2d91b1ceed' 'unpatched Windows application'

    & (Join-Path $RepositoryRoot 'Tools\Recovery\New-Baseline-Jit-Windows-Application.ps1') `
        -InputApplication $WindowsUnpatched `
        -LinkMap $WindowsMap `
        -OutputApplication $WindowsApplication
    Assert-File $WindowsApplication 3584 'fc7566f38457229444836b88aff48df09309b3bad242d1cac2eb2f432311ab39' 'Windows application'

    $Process = Start-Process -FilePath $WindowsApplication -WindowStyle Hidden -Wait -PassThru
    if ($Process.ExitCode -ne 0) {
        throw "The Windows application returned $($Process.ExitCode), expected zero."
    }

    Copy-Item -LiteralPath $WindowsApplication -Destination (Join-Path $WindowsDestination 'Baseline-Jit-Publisher.exe') -Force
    Copy-Item -LiteralPath $LinuxApplication -Destination (Join-Path $LinuxDestination 'Baseline-Jit-Publisher.elf') -Force
    Write-Output 'baseline jit publisher rebuild status=Complete windows-result=0 linux-execution=pending'
}
finally {
    $ExpectedParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
    $ActualParent = [IO.Path]::GetDirectoryName($TemporaryRoot).TrimEnd('\')
    if ($ActualParent -ne $ExpectedParent -or -not ([IO.Path]::GetFileName($TemporaryRoot).StartsWith('windvale-baseline-jit-publisher-'))) {
        throw "Refusing to remove unexpected temporary directory: $TemporaryRoot"
    }
    if (Test-Path -LiteralPath $TemporaryRoot) {
        Remove-Item -LiteralPath $TemporaryRoot -Recurse -Force
    }
}
