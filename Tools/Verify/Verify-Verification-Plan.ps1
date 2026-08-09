[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RetirementInventoryVerifier = Join-Path $PSScriptRoot 'Verify-Dotnet-Retirement-Inventory.ps1'
$Planner = Join-Path $PSScriptRoot 'Get-Verification-Plan.ps1'
$NativePlanner = Join-Path $PSScriptRoot 'Get-Native-Changed-Verification-Plan.ps1'
$AllAreas = @('assembler', 'bytecode', 'compiler', 'database', 'foundation', 'golden', 'linker', 'object-model', 'runtime')
$Cases = @(
    @{ Name = 'documentation'; Paths = @('README.md'); Scope = 'lightweight'; Editor = $false; Areas = @() },
    @{ Name = 'documentation image'; Paths = @('README.md', 'Documents/Project/Images/Progress.png'); Scope = 'lightweight'; Editor = $false; Areas = @() },
    @{ Name = 'editor'; Paths = @('Tools/Editors/Windvale/package.json'); Scope = 'lightweight'; Editor = $true; Areas = @() },
    @{ Name = 'website'; Paths = @('Website/index.html'); Scope = 'website'; Editor = $false; Areas = @() },
    @{ Name = 'website editor'; Paths = @('Tools/Windvale.Playground/Editor/Vite-Config.mjs', 'Tools/Editors/Windvale/package.json'); Scope = 'website'; Editor = $true; Areas = @() },
    @{ Name = 'website and compiler'; Paths = @('Website/site.js', 'Compiler/Reference/Seed-Compiler.cs'); Scope = 'qualification'; Editor = $true; Areas = $AllAreas },
    @{ Name = 'compiler'; Paths = @('Compiler/Reference/Seed-Compiler.cs'); Scope = 'qualification'; Editor = $true; Areas = @('compiler') },
    @{ Name = 'compiler and documentation image'; Paths = @('Compiler/Reference/Seed-Compiler.cs', 'Documents/Project/Images/Progress.png'); Scope = 'qualification'; Editor = $true; Areas = @('compiler') },
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
    @{ Name = 'database'; Paths = @('Libraries/Database/Wvdb-Reader.wv'); Scope = 'qualification'; Editor = $false; Areas = @('database') },
    @{ Name = 'database specification'; Paths = @('Specifications/Windvale-Database-Reader.md'); Scope = 'qualification'; Editor = $false; Areas = @('database') },
    @{ Name = 'Seed example'; Paths = @('Examples/Seed/Sum-Data.wv'); Scope = 'qualification'; Editor = $false; Areas = @('bytecode', 'compiler', 'runtime') },
    @{ Name = 'project tool'; Paths = @('Tools/Windvale.Project/Project-Parser.cs'); Scope = 'qualification'; Editor = $false; Areas = @('bytecode', 'compiler') },
    @{ Name = 'project manifest'; Paths = @('Windvale-Compiler.wvproj'); Scope = 'qualification'; Editor = $false; Areas = @('bytecode', 'compiler') },
    @{ Name = 'project specification'; Paths = @('Specifications/Windvale-Project.md'); Scope = 'qualification'; Editor = $false; Areas = @('bytecode', 'compiler') },
    @{ Name = 'bytecode specification'; Paths = @('Specifications/Seed-Bytecode.md'); Scope = 'qualification'; Editor = $false; Areas = @('bytecode', 'runtime') },
    @{ Name = 'test harness'; Paths = @('Tests/Windvale.Seed.Tests/Program.cs'); Scope = 'qualification'; Editor = $false; Areas = $AllAreas }
)
$NativeCases = @(
    @{
        Name = 'Windvale compiler'
        Paths = @('Compiler/Windvale/Source-Wvb-Compiler.wv')
        Suites = @('seed', 'unsafe-wvb', 'source-containment', 'lowerer-rejections', 'console-packager-source-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'managed compiler recovery source'
        Paths = @('Compiler/Reference/Seed-Compiler.cs')
        Suites = @()
        Gaps = @('managed-compiler-recovery-source')
        VerifyPlan = $false
    },
    @{
        Name = 'Windvale assembler'
        Paths = @('Assembler/Windvale/Wva-Assembler-Core.wv')
        Suites = @('assembler-rejections', 'assembler-golden', 'wva-differential')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Windvale linker'
        Paths = @('Linker/Windvale/Wv-Linker-Core.wv')
        Suites = @('linker-rejections', 'linker-hostile', 'linker-map-limit')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'normal process object'
        Paths = @('Operating-System/Tools/Process-Object-Tool.wv')
        Suites = @('os-process-object', 'os-probe')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'exact native suite owner'
        Paths = @('Tools/Native/Test-Linker-Map-Limit.cmd')
        Suites = @('linker-map-limit')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'hosted verifier publisher boundaries'
        Paths = @(
            'Linker/Windvale/Native-Hosted-Verifier-Application-Admission.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Application-Tool.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Application-Publisher-Metadata-Admission.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Construction-Admission.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Object-Instantiation-Core.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Windows-Imports-Core.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Linux-Materialization-Core.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Windows-Materialization-Core.wv',
            'Runtime/Windvale/Native-Hosted-Verifier-Publisher-Base-Metadata-Core.wv',
            'Runtime/Windvale/Native-Hosted-Verifier-Publisher-Base-Runtime-Tool.wv',
            'Tools/Windvale.Publish/Native-Hosted-Verifier-Application-Publisher.wv',
            'Tools/Native/Publish-Hosted-Verifier-Application.cmd',
            'Tools/Native/Construct-Hosted-Verifier-Publisher.cmd',
            'Artifacts/Native-Hosted-Verifier-Application-Publisher-Candidate/Manifest.json',
            'Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate/Manifest.json',
            'Specifications/Windvale-Native-Hosted-Verifier-Application-Publisher.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Application-Publisher-Metadata.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Construction-Requests.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Object-Instantiation.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Windows-Imports.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Linux-Materialization.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Windows-Materialization.md',
            'Windvale-Native-Hosted-Verifier-Application-Publisher.wvproj',
            'Windvale-Native-Hosted-Verifier-Application-Publisher-Metadata.wvproj',
            'Windvale-Native-Hosted-Verifier-Publisher-Construction-Request.wvproj',
            'Windvale-Native-Hosted-Verifier-Publisher-Object-Instantiation.wvproj',
            'Windvale-Native-Hosted-Verifier-Publisher-Windows-Imports.wvproj',
            'Windvale-Native-Hosted-Verifier-Publisher-Linux-Materialization.wvproj',
            'Windvale-Native-Hosted-Verifier-Publisher-Windows-Materialization.wvproj',
            'Windvale-Native-Hosted-Verifier-Publisher-Target-Request-Tool.wvproj',
            'Windvale-Native-Hosted-Verifier-Publisher-Base-Metadata-Tool.wvproj',
            'Windvale-Native-Hosted-Verifier-Application-Tool.wvproj'
        )
        Suites = @('publisher-rejections', 'hosted-verifier-publisher-files')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'database gap'
        Paths = @('Libraries/Database/Wvdb-Reader.wv')
        Suites = @()
        Gaps = @('database-native-tests')
        VerifyPlan = $false
    },
    @{
        Name = 'verification planner'
        Paths = @('Tools/Verify/Get-Native-Changed-Verification-Plan.ps1')
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'verification planner with ordinary documentation'
        Paths = @(
            'Tools/Verify/Get-Native-Changed-Verification-Plan.ps1',
            'AGENTS.md',
            'Documents/Decisions/0458-Native-Changed-File-Verification.md'
        )
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'native changed-file specification'
        Paths = @(
            'Specifications/README.md',
            'Specifications/Windvale-Native-Changed-Verification.md'
        )
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'combined deterministic order'
        Paths = @('Compiler/Windvale/Source-Wvb-Compiler.wv', 'Assembler/Windvale/Wva-Assembler-Core.wv')
        Suites = @('seed', 'unsafe-wvb', 'assembler-rejections', 'assembler-golden', 'wva-differential', 'source-containment', 'lowerer-rejections', 'console-packager-source-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'unmapped gap'
        Paths = @('Unknown/Boundary.bin')
        Suites = @()
        Gaps = @('unmapped:Unknown/Boundary.bin')
        VerifyPlan = $false
    },
    @{
        Name = 'empty gap'
        Paths = @()
        Suites = @()
        Gaps = @('empty-changed-path-set')
        VerifyPlan = $false
    }
)

& $RetirementInventoryVerifier -Quiet

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

foreach ($Case in $NativeCases) {
    $Plan = & $NativePlanner -ChangedPath $Case.Paths -PassThru -Quiet
    if (
        !([System.Linq.Enumerable]::SequenceEqual(
            [string[]]@($Plan.Suites),
            [string[]]$Case.Suites)) -or
        !([System.Linq.Enumerable]::SequenceEqual(
            [string[]]@($Plan.Gaps),
            [string[]]$Case.Gaps)) -or
        $Plan.RunPlanVerification -ne $Case.VerifyPlan
    ) {
        throw (
            "Native plan '$($Case.Name)' expected suites=[$($Case.Suites -join ', ')], " +
            "gaps=[$($Case.Gaps -join ', ')], verify-plan=$($Case.VerifyPlan); found " +
            "suites=[$($Plan.Suites -join ', ')], gaps=[$($Plan.Gaps -join ', ')], " +
            "verify-plan=$($Plan.RunPlanVerification)."
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

Write-Host (
    "Changed-file verification planning passed " +
    "($($Cases.Count + 1) general, $($NativeCases.Count) native cases).")
