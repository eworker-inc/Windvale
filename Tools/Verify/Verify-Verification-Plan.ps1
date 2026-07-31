[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$Planner = Join-Path $PSScriptRoot 'Get-Verification-Plan.ps1'
$AllAreas = @('assembler', 'bytecode', 'compiler', 'foundation', 'golden', 'linker', 'object-model', 'runtime')
$Cases = @(
    @{ Name = 'documentation'; Paths = @('README.md'); Scope = 'lightweight'; Editor = $false; Areas = @() },
    @{ Name = 'editor'; Paths = @('Tools/Editors/Windvale/package.json'); Scope = 'lightweight'; Editor = $true; Areas = @() },
    @{ Name = 'compiler'; Paths = @('Compiler/Reference/Seed-Compiler.cs'); Scope = 'qualification'; Editor = $true; Areas = @('compiler') },
    @{ Name = 'native compiler'; Paths = @('Compiler/Native/X64-Native-Backend.cs'); Scope = 'qualification'; Editor = $false; Areas = @('compiler') },
    @{ Name = 'bytecode'; Paths = @('Runtime/Windvale.Bytecode/Module-Codec.cs'); Scope = 'qualification'; Editor = $false; Areas = @('bytecode') },
    @{ Name = 'native runtime'; Paths = @('Runtime/Windvale.Native/X64-Native-Executor.cs'); Scope = 'qualification'; Editor = $false; Areas = @('runtime') },
    @{ Name = 'runtime'; Paths = @('Runtime/Windvale.Runtime/Reference-Runtime.cs'); Scope = 'qualification'; Editor = $false; Areas = @('runtime') },
    @{ Name = 'object model'; Paths = @('Object-Model/Windvale.ObjectModel/Object-Codec.cs'); Scope = 'qualification'; Editor = $false; Areas = @('object-model') },
    @{ Name = 'assembler reference'; Paths = @('Assembler/Reference/Assembly-Compiler.cs'); Scope = 'qualification'; Editor = $false; Areas = @('assembler') },
    @{ Name = 'assembler Windvale'; Paths = @('Assembler/Windvale/Wva-Assembler-Core.wv'); Scope = 'qualification'; Editor = $false; Areas = @('assembler') },
    @{ Name = 'linker reference'; Paths = @('Linker/Reference/Link-Compiler.cs'); Scope = 'qualification'; Editor = $false; Areas = @('linker') },
    @{ Name = 'linker Windvale'; Paths = @('Linker/Windvale/Wv-Linker-Core.wv'); Scope = 'qualification'; Editor = $false; Areas = @('linker') },
    @{ Name = 'Foundation'; Paths = @('Foundation/Byte-Ordering.wv'); Scope = 'qualification'; Editor = $false; Areas = @('foundation') },
    @{ Name = 'Seed example'; Paths = @('Examples/Seed/Sum-Data.wv'); Scope = 'qualification'; Editor = $false; Areas = @('bytecode', 'compiler', 'runtime') },
    @{ Name = 'bytecode specification'; Paths = @('Specifications/Seed-Bytecode.md'); Scope = 'qualification'; Editor = $false; Areas = @('bytecode', 'runtime') },
    @{ Name = 'test harness'; Paths = @('Tests/Windvale.Seed.Tests/Program.cs'); Scope = 'qualification'; Editor = $false; Areas = $AllAreas }
)

foreach ($Case in $Cases) {
    $Plan = & $Planner -ChangedPath $Case.Paths -PassThru -Quiet
    if (
        $Plan.Scope -ne $Case.Scope -or
        $Plan.Editor -ne $Case.Editor -or
        !([System.Linq.Enumerable]::SequenceEqual(
            [string[]]@($Plan.Areas),
            [string[]]$Case.Areas))
    ) {
        throw (
            "Plan '$($Case.Name)' expected scope=$($Case.Scope), editor=$($Case.Editor), " +
            "areas=[$($Case.Areas -join ', ')]; found scope=$($Plan.Scope), " +
            "editor=$($Plan.Editor), areas=[$($Plan.Areas -join ', ')]."
        )
    }
}

$EmptyPlan = & $Planner -ChangedPath @() -PassThru -Quiet
if (
    $EmptyPlan.Scope -ne 'qualification' -or
    !$EmptyPlan.Editor -or
    !([System.Linq.Enumerable]::SequenceEqual(
        [string[]]@($EmptyPlan.Areas),
        [string[]]$AllAreas))
) {
    throw 'An empty changed-path plan did not fail closed to qualification, editor verification, and all Seed areas.'
}

Write-Host "Changed-file verification planning passed ($($Cases.Count + 1) cases)."
