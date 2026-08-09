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

$BridgeWvb = Join-Path $TemporaryRoot 'Bridge.wvb'
$RetainedBridgeWvb = Join-Path $RepositoryRoot 'Artifacts\Baseline-Jit-Publisher\Wvb\Baseline-Jit-Patch-Plan-Bridge.wvb'
$BridgeWvo = Join-Path $RepositoryRoot 'Artifacts\Baseline-Jit-Publisher\Wvo\Baseline-Jit-Patch-Plan-Bridge.wvo'
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
        & (Join-Path $RepositoryRoot 'Tools\Native\Build-Wvb.cmd') `
            (Join-Path $RepositoryRoot 'Windvale-Native-Baseline-Jit-Patch-Plan-Bridge.wvproj') `
            $BridgeWvb
    } 'Windvale producer-bridge build'
    Assert-File $BridgeWvb 4574 '2dc536e9d3511d4fde3191e1084d9634543154a525623fd3c7c669f9d3bf20d9' 'producer-bridge WVB'
    Assert-File $RetainedBridgeWvb 4574 '2dc536e9d3511d4fde3191e1084d9634543154a525623fd3c7c669f9d3bf20d9' 'retained producer-bridge WVB'
    Assert-File $BridgeWvo 56226 'bcc02cdc6134da2388265ad308d3dc739a7e10c1911effa918d5f2577c86ae8c' 'retained producer-bridge WVO'
    Invoke-Checked {
        & (Join-Path $RepositoryRoot 'Tools\Native\Verify-Wvo.cmd') $BridgeWvo
    } 'producer-bridge WVO verification'

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

    Assert-File $PlanWvo 1463 '8cc9c7460229a479adf34631a970c9d196b37361ceaa35fdea85e15fce9d91b1' 'shared plan WVO'
    Assert-File $LinuxWvo 1472 '7a6556a0b5f59935edfa5fd380874a63ae594ac91deaeea88fd31383a60267b8' 'Linux adapter WVO'
    Assert-File $WindowsWvo 2632 'fc9c59e7005a0c60dd1a9a0240635b4416e509ef5e273745e35f1b2aca94b4ca' 'Windows adapter WVO'

    $LinuxMapLines = & (Join-Path $RepositoryRoot 'Tools\Native\Link-Wvo.cmd') `
        1048576 Linux_baseline_jit_entry $LinuxImage $LinuxWvo $PlanWvo $BridgeWvo
    if ($LASTEXITCODE -ne 0) { throw 'Linux adapter linking failed.' }
    Write-Link-Map $LinuxMap $LinuxMapLines

    $WindowsMapLines = & (Join-Path $RepositoryRoot 'Tools\Native\Link-Wvo.cmd') `
        4208 Windows_baseline_jit_entry $WindowsImage $WindowsWvo $PlanWvo $BridgeWvo
    if ($LASTEXITCODE -ne 0) { throw 'Windows adapter linking failed.' }
    Write-Link-Map $WindowsMap $WindowsMapLines

    Assert-File $LinuxImage 57500 'c77ab84774f7c1f188855c095b30b7e8182c31523d579a6a72b4735d7524c78a' 'Linux flat image'
    Assert-File $WindowsImage 57836 'db35482f2886077701c4a8a78f6783fae5adeeaf2821411cbcf21bb480f1bdd3' 'Windows flat image'

    Invoke-Checked {
        & (Join-Path $RepositoryRoot 'Tools\Native\Package-Console.cmd') `
            linux-x64-console-v1 $LinuxImage 595 $LinuxApplication
    } 'Linux application packaging'
    Invoke-Checked {
        & (Join-Path $RepositoryRoot 'Tools\Native\Package-Console.cmd') `
            windows-x64-console-v1 $WindowsImage 718 $WindowsUnpatched
    } 'Windows application packaging'

    Assert-File $LinuxApplication 65648 '29538c93d28bcd1feae175519f5b2950d5e8dfcde24afa3f0039863fb1706a90' 'Linux application'
    Assert-File $WindowsUnpatched 59904 'e53b7aa85eb65db57bb93a1ad00065ab1462219d8030096120f7ce32a1eeb599' 'unpatched Windows application'

    & (Join-Path $RepositoryRoot 'Tools\Recovery\New-Baseline-Jit-Windows-Application.ps1') `
        -InputApplication $WindowsUnpatched `
        -LinkMap $WindowsMap `
        -OutputApplication $WindowsApplication
    Assert-File $WindowsApplication 59904 '8ea1a0d6371c9447031db4ae2b56ecfef5f022a83b6bdd7831020a2628bee01c' 'Windows application'

    $Process = Start-Process -FilePath $WindowsApplication -WindowStyle Hidden -Wait -PassThru
    if ($Process.ExitCode -ne 0) {
        throw "The Windows application returned $($Process.ExitCode), expected zero."
    }

    Copy-Item -LiteralPath $WindowsApplication -Destination (Join-Path $WindowsDestination 'Baseline-Jit-Publisher.exe') -Force
    Copy-Item -LiteralPath $LinuxApplication -Destination (Join-Path $LinuxDestination 'Baseline-Jit-Publisher.elf') -Force
    Write-Output 'baseline jit publisher rebuild status=Complete bridge-wvo=retained-stage0 windows-result=0 linux-execution=pending'
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
