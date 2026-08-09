param(
    [Parameter(Mandatory = $true)]
    [string] $InputApplication,

    [Parameter(Mandatory = $true)]
    [string] $LinkMap,

    [Parameter(Mandatory = $true)]
    [string] $OutputApplication
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Read-U16([byte[]] $Value, [int] $Offset) {
    return [uint32]$Value[$Offset] -bor ([uint32]$Value[$Offset + 1] -shl 8)
}

function Read-U32([byte[]] $Value, [int] $Offset) {
    return [uint32]$Value[$Offset] `
        -bor ([uint32]$Value[$Offset + 1] -shl 8) `
        -bor ([uint32]$Value[$Offset + 2] -shl 16) `
        -bor ([uint32]$Value[$Offset + 3] -shl 24)
}

function Write-U32([byte[]] $Value, [int] $Offset, [uint32] $Field) {
    $Value[$Offset] = [byte]($Field -band 255)
    $Value[$Offset + 1] = [byte](($Field -shr 8) -band 255)
    $Value[$Offset + 2] = [byte](($Field -shr 16) -band 255)
    $Value[$Offset + 3] = [byte](($Field -shr 24) -band 255)
}

function Require-U32([byte[]] $Value, [int] $Offset, [uint32] $Expected, [string] $Name) {
    $Actual = Read-U32 $Value $Offset
    if ($Actual -ne $Expected) {
        throw "$Name is $Actual, expected $Expected."
    }
}

function Require-Bytes([byte[]] $Value, [int] $Offset, [byte[]] $Expected, [string] $Name) {
    for ($ByteIndex = 0; $ByteIndex -lt $Expected.Length; $ByteIndex++) {
        if ($Value[$Offset + $ByteIndex] -ne $Expected[$ByteIndex]) {
            throw "$Name differs at byte $ByteIndex."
        }
    }
}

function Symbol-Address([string[]] $Lines, [string] $Name) {
    $Pattern = "\bname=$([Regex]::Escape($Name)) address=([0-9]+)\b"
    foreach ($Line in $Lines) {
        $Match = [Regex]::Match($Line, $Pattern)
        if ($Match.Success) {
            return [uint32]::Parse($Match.Groups[1].Value)
        }
    }
    throw "The link map does not define $Name."
}

$InputPath = (Resolve-Path -LiteralPath $InputApplication).Path
$MapPath = (Resolve-Path -LiteralPath $LinkMap).Path
$OutputPath = [IO.Path]::GetFullPath($OutputApplication)
$OutputDirectory = [IO.Path]::GetDirectoryName($OutputPath)
if (-not [IO.Directory]::Exists($OutputDirectory)) {
    throw "The output directory does not exist: $OutputDirectory"
}

[byte[]] $Application = [IO.File]::ReadAllBytes($InputPath)
if ($Application.Length -ne 59904) {
    throw "The input application is $($Application.Length) bytes, expected 59904."
}
if ((Read-U16 $Application 0) -ne 23117) {
    throw 'The input application does not begin with MZ.'
}
$PeOffset = Read-U32 $Application 60
if ($PeOffset -ne 128) {
    throw "The PE offset is $PeOffset, expected 128."
}
Require-U32 $Application $PeOffset 17744 'PE signature'
if ((Read-U16 $Application ($PeOffset + 6)) -ne 3) {
    throw 'The input application does not have exactly three sections.'
}
$Optional = $PeOffset + 24
if ((Read-U16 $Application $Optional) -ne 523) {
    throw 'The input application is not PE32+.'
}
Require-U32 $Application ($Optional + 108) 16 'data-directory count'

$ImportDirectory = $Optional + 120
$IatDirectory = $Optional + 208
Require-U32 $Application $ImportDirectory 0 'initial import RVA'
Require-U32 $Application ($ImportDirectory + 4) 0 'initial import size'
Require-U32 $Application $IatDirectory 0 'initial IAT RVA'
Require-U32 $Application ($IatDirectory + 4) 0 'initial IAT size'

$Lines = [IO.File]::ReadAllLines($MapPath)
$LookupRva = Symbol-Address $Lines 'Windows_jit_import_lookup'
$IatRva = Symbol-Address $Lines 'Windows_jit_import_addresses'
$DescriptorRva = Symbol-Address $Lines 'Windows_jit_import_descriptor'
$DllNameRva = Symbol-Address $Lines 'Windows_jit_kernel32_name'
$VirtualAllocNameRva = Symbol-Address $Lines 'Windows_jit_name_virtual_alloc'
$VirtualProtectNameRva = Symbol-Address $Lines 'Windows_jit_name_virtual_protect'
$FlushNameRva = Symbol-Address $Lines 'Windows_jit_name_flush_instruction_cache'
$VirtualFreeNameRva = Symbol-Address $Lines 'Windows_jit_name_virtual_free'

$SectionTable = $Optional + 240
$TextRva = Read-U32 $Application ($SectionTable + 12)
$TextRaw = Read-U32 $Application ($SectionTable + 20)
if ($TextRva -ne 4096 -or $TextRaw -ne 512) {
    throw 'The version-1 text placement differs.'
}
$IatOffset = [int]($TextRaw + $IatRva - $TextRva)
$DescriptorOffset = [int]($TextRaw + $DescriptorRva - $TextRva)

[uint32[]] $ExpectedIat = $VirtualAllocNameRva, $VirtualProtectNameRva, $FlushNameRva, $VirtualFreeNameRva, 0
for ($Index = 0; $Index -lt $ExpectedIat.Length; $Index++) {
    Require-U32 $Application ($IatOffset + $Index * 8) $ExpectedIat[$Index] "IAT entry $Index low word"
    Require-U32 $Application ($IatOffset + $Index * 8 + 4) 0 "IAT entry $Index high word"
}

[uint32[]] $ExpectedDescriptor = $LookupRva, 0, 0, $DllNameRva, $IatRva, 0, 0, 0, 0, 0
for ($Index = 0; $Index -lt $ExpectedDescriptor.Length; $Index++) {
    Require-U32 $Application ($DescriptorOffset + $Index * 4) $ExpectedDescriptor[$Index] "import descriptor field $Index"
}

$NameOffset = { param([uint32] $Rva) [int]($TextRaw + $Rva - $TextRva) }
Require-Bytes $Application (& $NameOffset $DllNameRva) ([Text.Encoding]::ASCII.GetBytes("KERNEL32.dll`0")) 'DLL name'
Require-Bytes $Application ((& $NameOffset $VirtualAllocNameRva) + 2) ([Text.Encoding]::ASCII.GetBytes("VirtualAlloc`0")) 'VirtualAlloc name'
Require-Bytes $Application ((& $NameOffset $VirtualProtectNameRva) + 2) ([Text.Encoding]::ASCII.GetBytes("VirtualProtect`0")) 'VirtualProtect name'
Require-Bytes $Application ((& $NameOffset $FlushNameRva) + 2) ([Text.Encoding]::ASCII.GetBytes("FlushInstructionCache`0")) 'FlushInstructionCache name'
Require-Bytes $Application ((& $NameOffset $VirtualFreeNameRva) + 2) ([Text.Encoding]::ASCII.GetBytes("VirtualFree`0")) 'VirtualFree name'

Write-U32 $Application $ImportDirectory $DescriptorRva
Write-U32 $Application ($ImportDirectory + 4) 40
Write-U32 $Application $IatDirectory $IatRva
Write-U32 $Application ($IatDirectory + 4) 40

[IO.File]::WriteAllBytes($OutputPath, $Application)
Write-Output "baseline jit Windows application status=Complete bytes=$($Application.Length) imports=4"
