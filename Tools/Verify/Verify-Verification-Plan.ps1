[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$RetirementInventoryVerifier = Join-Path $PSScriptRoot 'Verify-Dotnet-Retirement-Inventory.ps1'
$DevelopmentDependencyVerifier = Join-Path $PSScriptRoot 'Verify-Native-Development-Dependencies.ps1'
$Planner = Join-Path $PSScriptRoot 'Get-Verification-Plan.ps1'
$NativePlanner = Join-Path $PSScriptRoot 'Get-Native-Changed-Verification-Plan.ps1'
$AllAreas = @('assembler', 'bytecode', 'compiler', 'database', 'foundation', 'golden', 'linker', 'object-model', 'runtime')
$Cases = @(
    @{ Name = 'documentation'; Paths = @('README.md'); Scope = 'lightweight'; Editor = $false; Areas = @() },
    @{ Name = 'documentation image'; Paths = @('README.md', 'Documents/Project/Images/Progress.png'); Scope = 'lightweight'; Editor = $false; Areas = @() },
    @{ Name = 'Language 1.0 paper source'; Paths = @('Documents/Project/Language-1.0-Paper-Corpus/11-Local-AI-Accelerator-Inference/Source/Inference-Application.wv'); Scope = 'development'; Editor = $false; Areas = @('compiler') },
    @{ Name = 'Language 1.0 localization paper source'; Paths = @('Documents/Project/Language-1.0-Localization-Workloads/01-Source-Profile-Admission/Source/Test-Unicode-Admission.wv'); Scope = 'development'; Editor = $false; Areas = @('compiler') },
    @{ Name = 'Language 1.0 paper package data'; Paths = @('Documents/Project/Language-1.0-Paper-Corpus/07-Gui-Retained-State/Package-Data/Theme.wvtheme'); Scope = 'development'; Editor = $false; Areas = @('compiler') },
    @{ Name = 'Language 1.0 localization reference artifact'; Paths = @('Documents/Project/Language-1.0-Localization-Workloads/01-Source-Profile-Admission/Reference-Artifacts/Source-Inputs.wvlock'); Scope = 'development'; Editor = $false; Areas = @('compiler') },
    @{ Name = 'editor'; Paths = @('Tools/Editors/Windvale/package.json'); Scope = 'lightweight'; Editor = $true; Areas = @() },
    @{ Name = 'website'; Paths = @('Website/index.html'); Scope = 'website'; Editor = $false; Areas = @() },
    @{ Name = 'browser application'; Paths = @('Applications/Web/Wvdb-Workbench/Source/Main.ts', 'Libraries/Web/Framework/State/State-Owner.ts'); Scope = 'website'; Editor = $false; Areas = @() },
    @{ Name = 'browser playground website contract'; Paths = @('Specifications/Browser-Playground.md', 'Tools/Verify/Verify-Website.ps1'); Scope = 'website'; Editor = $false; Areas = @() },
    @{ Name = 'website editor'; Paths = @('Tools/Windvale.Playground/Editor/Vite-Config.mjs', 'Tools/Editors/Windvale/package.json'); Scope = 'website'; Editor = $true; Areas = @() },
    @{ Name = 'website and compiler'; Paths = @('Website/site.js', 'Compiler/Windvale/Source-Lexer-Core.wv'); Scope = 'development'; Editor = $true; Areas = $AllAreas },
    @{ Name = 'compiler'; Paths = @('Compiler/Windvale/Source-Lexer-Core.wv'); Scope = 'development'; Editor = $true; Areas = @('compiler') },
    @{ Name = 'compiler and documentation image'; Paths = @('Compiler/Windvale/Source-Lexer-Core.wv', 'Documents/Project/Images/Progress.png'); Scope = 'development'; Editor = $true; Areas = @('compiler') },
    @{ Name = 'native compiler'; Paths = @('Compiler/Windvale/Native-X64-Lowering-Core.wv'); Scope = 'development'; Editor = $true; Areas = @('compiler') },
    @{ Name = 'runtime'; Paths = @('Runtime/Windvale/Native-Execution-Context-Core.wv'); Scope = 'development'; Editor = $false; Areas = @('runtime') },
    @{ Name = 'object model'; Paths = @('Object-Model/Windvale/Wvo-Object-Core.wv'); Scope = 'development'; Editor = $false; Areas = @('object-model') },
    @{ Name = 'assembler Windvale'; Paths = @('Assembler/Windvale/Wva-Assembler-Core.wv'); Scope = 'development'; Editor = $false; Areas = @('assembler') },
    @{ Name = 'linker Windvale'; Paths = @('Linker/Windvale/Wv-Linker-Core.wv'); Scope = 'development'; Editor = $false; Areas = @('linker') },
    @{ Name = 'Foundation'; Paths = @('Foundation/Byte-Ordering.wv'); Scope = 'development'; Editor = $false; Areas = @('foundation') },
    @{ Name = 'database'; Paths = @('Libraries/Database/Wvdb-Reader.wv'); Scope = 'development'; Editor = $false; Areas = @('database') },
    @{ Name = 'database specification'; Paths = @('Specifications/Windvale-Database-Reader.md'); Scope = 'development'; Editor = $false; Areas = @('database') },
    @{ Name = 'Seed example'; Paths = @('Examples/Seed/Sum-Data.wv'); Scope = 'development'; Editor = $false; Areas = @('bytecode', 'compiler', 'runtime') },
    @{ Name = 'project tool'; Paths = @('Tools/Windvale.Project/Project-Manifest-Core.wv'); Scope = 'development'; Editor = $false; Areas = @('bytecode', 'compiler') },
    @{ Name = 'project manifest'; Paths = @('Projects/Examples/Windvale-Compiler.wvproj'); Scope = 'development'; Editor = $false; Areas = @('bytecode', 'compiler') },
    @{ Name = 'project specification'; Paths = @('Specifications/Windvale-Project.md'); Scope = 'development'; Editor = $false; Areas = @('bytecode', 'compiler') },
    @{ Name = 'Language 1.0 grammar candidate'; Paths = @('Specifications/Windvale-Language-1.0-Grammar.md'); Scope = 'development'; Editor = $false; Areas = @('compiler') },
    @{ Name = 'Language 1.0 semantic and Foundation candidates'; Paths = @('Specifications/Windvale-Language-1.0.md', 'Specifications/Windvale-Language-1.0-Foundation.md'); Scope = 'development'; Editor = $false; Areas = @('compiler', 'foundation', 'runtime') },
    @{ Name = 'bytecode specification'; Paths = @('Specifications/Seed-Bytecode.md'); Scope = 'development'; Editor = $false; Areas = @('bytecode', 'runtime') },
    @{ Name = 'test fixture'; Paths = @('Tests/Native/Wvo-Rejections/Bad-Version.wvo.b64'); Scope = 'development'; Editor = $false; Areas = $AllAreas }
)
$NativeCases = @(
    @{
        Name = 'native verification owner live-stream coordinator'
        Paths = @(
            'Tools/Native/Stream-Verification-Owner.mjs',
            'Tools/Native/Test-Verification-Owners.cmd',
            'Tools/Native/Test-Verification-Owners.sh'
        )
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'Language 1.0 paper package-data evidence'
        Paths = @(
            'Documents/Project/Language-1.0-Paper-Corpus/07-Gui-Retained-State/Package-Data/Theme.wvtheme'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 localization reference-artifact evidence'
        Paths = @(
            'Documents/Project/Language-1.0-Localization-Workloads/01-Source-Profile-Admission/Reference-Artifacts/Source-Inputs.wvlock',
            'Documents/Project/Language-1.0-Localization-Workloads/01-Source-Profile-Admission/Reference-Artifacts/En-Foundation-Result.wvcat'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 Foundation generic typed-failure owner'
        Paths = @(
            'Libraries/Foundation/Values/Option.wv',
            'Libraries/Foundation/Values/Result.wv',
            'Projects/Tests/Language-1.0-Foundation-Generic-Result.wvproj',
            'Tests/Fixtures/Language-1.0/Foundation-Generic-Result.wv',
            'Tests/Fixtures/Language-1.0/Foundation-Generic-Result-Wrong-Arity.wv',
            'Tests/Fixtures/Language-1.0/Foundation-Generic-Result-Extra-Argument.wv',
            'Tests/Fixtures/Language-1.0/Foundation-Generic-Result-Bare.wv',
            'Tests/Fixtures/Language-1.0/Foundation-Generic-Try-Wrong-Error.wv',
            'Documents/Project/Windvale-Language-1.0-Migration-Evidence.md',
            'Documents/Decisions/0780-Implement-Language-1.0-Generic-Option-And-Result.md'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 frozen source and descriptor front door'
        Paths = @(
            'Compiler/Windvale/Source-Descriptor-Core.wv',
            'Projects/Compiler/Windvale-Source-Descriptor-Core.wvproj',
            'Projects/Tests/Windvale-Native-Test-Source-Descriptor.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Calls.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Collection-Analysis-Publication.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Declarations.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Multiple-Specializations.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Resolution.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Type-Catalog.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Value-Front-End.wvproj',
            'Tests/Fixtures/Language-1.0/Descriptorless-Edition-Header.wv',
            'Tests/Fixtures/Language-1.0/Minimum-Program.wv',
            'Tests/Fixtures/Language-1.0/Missing-Edition-Profile.wv',
            'Tests/Fixtures/Language-1.0/Seed-Only-Void.wv',
            'Tests/Fixtures/Language-1.0/Source-Descriptor-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Generic-Call-Front-End-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Generic-Collection-Analysis-Publication-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Generic-Collection-Monomorphic-Oracle.wv',
            'Tests/Fixtures/Language-1.0/Generic-Declaration-Front-End-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Generic-Multiple-Specializations.wv',
            'Tests/Fixtures/Language-1.0/Generic-Resolution-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Generic-Type-Catalog-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Unsupported-Source-Profile.wv',
            'Tests/Fixtures/Language-1.0/Value-Front-End-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Fixed-Integer-Program.wv',
            'Tests/Fixtures/Language-1.0/Rune-Program.wv',
            'Tests/Fixtures/Language-1.0/Floating-Program.wv',
            'Tests/Fixtures/Language-1.0/Multi-Field-Variant.wv',
            'Tests/Fixtures/Language-1.0/Named-Variant-Field.wv',
            'Projects/Tests/Windvale-Native-Test-Wvb-Fixed-Integer-Runtime.wvproj',
            'Projects/Tests/Windvale-Native-Test-Wvb-Rune-Runtime.wvproj',
            'Projects/Tests/Windvale-Native-Test-Wvb-Floating-Runtime.wvproj',
            'Tests/Native/Language-1.0-Fixture-Inventory.txt',
            'Tools/Native/Verify-Language-1.0-Migration-Fixtures.mjs',
            'Tools/Native/Verify-Language-1.0-Fixed-Integers.mjs',
            'Tools/Native/Verify-Language-1.0-Runes.mjs',
            'Tools/Native/Verify-Language-1.0-Floating.mjs',
            'Tools/Native/Verify-Language-1.0-Multi-Field-Variants.mjs',
            'Tools/Native/Test-Language-1.0-Front-Door.cmd',
            'Tools/Native/Test-Language-1.0-Front-Door.sh',
            'Specifications/Windvale-Language-1.0-Grammar.md',
            'Documents/Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md',
            'Documents/Project/Language-1.0-Paper-Corpus/01-Command-Line-Application/Source/Inspect-Application.wv'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic resolution boundary routing'
        Paths = @(
            'Compiler/Windvale/Source-Generic-Resolution-Core.wv',
            'Specifications/Compiler-Source-Generic-Resolution.md'
        )
        Suites = @(
            'source-containment',
            'language-1-front-door'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal type catalog routing'
        Paths = @(
            'Compiler/Windvale/Source-Generic-Lowering-Core.wv',
            'Compiler/Windvale/Source-Bindings-Generic-Types-Core.wv',
            'Compiler/Windvale/Source-Generic-Type-Binding-Core.wv',
            'Compiler/Windvale/Source-Generic-Type-Layout-Core.wv',
            'Compiler/Windvale/Source-Generic-Type-Materialization-Core.wv',
            'Compiler/Windvale/Source-Generic-Type-Lowering-Core.wv',
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Type-Binding.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Type-Layout.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Type-Materialization.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Type-Catalog.wvproj',
            'Specifications/Compiler-Source-Generic-Types.md',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Type-Binding-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Type-Layout-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Type-Materialization-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Generic-Type-Catalog-Self-Test.wv'
        )
        Suites = @(
            'source-containment',
            'generic-nominal-type-binding',
            'generic-nominal-type-layout',
            'generic-nominal-type-materialization',
            'generic-nominal-wvlb-carrier',
            'language-1-front-door'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal main-pipeline routing'
        Paths = @(
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Main-Pipeline.wv',
            'Tools/Native/Verify-Generic-Nominal-Main-Pipeline.mjs'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal function-body routing'
        Paths = @(
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Function-Body.wv',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Function-Body-Type-Mismatch.wv',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Function-Body-Unknown-Field.wv',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Function-Body-Inference-Mismatch.wv',
            'Tools/Native/Verify-Generic-Nominal-Function-Body.mjs'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal declaration-dependency routing'
        Paths = @(
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Declaration-Dependency.wv',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Declaration-Cycle.wv',
            'Tools/Native/Verify-Generic-Nominal-Declaration-Dependency.mjs'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal variant routing'
        Paths = @(
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Variant.wv',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Variant-Type-Mismatch.wv',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Variant-Missing-Field.wv',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Variant-Pattern-Type-Mismatch.wv',
            'Tools/Native/Verify-Generic-Nominal-Variant.mjs'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal type-binding owner routing'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Type-Binding.wvproj',
            'Tests/Fixtures/Language-1.0/Borrow-Parser-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Type-Binding-Self-Test.wv',
            'Tools/Native/Test-Generic-Nominal-Type-Binding.cmd',
            'Tools/Native/Test-Generic-Nominal-Type-Binding.sh',
            'Tools/Native/Test-Generic-Nominal-Type-Binding.mjs'
        )
        Suites = @('generic-nominal-type-binding')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 borrow semantic routing'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-Borrow-Call.wvproj',
            'Tests/Fixtures/Language-1.0/Borrow-Call-Main-Pipeline.wv',
            'Tests/Fixtures/Language-1.0/Borrow-Escape-Local.wv',
            'Tests/Fixtures/Language-1.0/Borrow-Immutable-To-Mutable.wv',
            'Tests/Fixtures/Language-1.0/Borrow-Missing-Explicit.wv',
            'Tests/Fixtures/Language-1.0/Borrow-Mutable-From-Let.wv',
            'Tests/Fixtures/Language-1.0/Borrow-Owned-Read-Through.wv',
            'Tests/Fixtures/Language-1.0/Borrow-Sequence-Read-Through.wv',
            'Tests/Fixtures/Language-1.0/Borrow-Vector-Owned-Read-Through.wv',
            'Tests/Fixtures/Language-1.0/Borrow-Return.wv'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal type-layout owner routing'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Type-Layout.wvproj',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Type-Layout-Self-Test.wv',
            'Tools/Native/Test-Generic-Nominal-Type-Layout.cmd',
            'Tools/Native/Test-Generic-Nominal-Type-Layout.sh',
            'Tools/Native/Test-Generic-Nominal-Type-Layout.mjs'
        )
        Suites = @('generic-nominal-type-layout')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal type-materialization owner routing'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Type-Materialization.wvproj',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Type-Materialization-Self-Test.wv',
            'Tools/Native/Test-Generic-Nominal-Type-Materialization.cmd',
            'Tools/Native/Test-Generic-Nominal-Type-Materialization.sh',
            'Tools/Native/Test-Generic-Nominal-Type-Materialization.mjs'
        )
        Suites = @('generic-nominal-type-materialization')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal WVLB carrier owner routing'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Wvlb-Carrier.wvproj',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Wvlb-Carrier-Self-Test.wv',
            'Tools/Native/Test-Generic-Nominal-Wvlb-Carrier.cmd',
            'Tools/Native/Test-Generic-Nominal-Wvlb-Carrier.sh',
            'Tools/Native/Test-Generic-Nominal-Wvlb-Carrier.mjs'
        )
        Suites = @('generic-nominal-wvlb-carrier')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal declaration routing'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Declaration.wvproj',
            'Tests/Fixtures/Language-1.0/Generic-Nominal-Declaration-Self-Test.wv',
            'Tools/Native/Test-Generic-Nominal-Declarations.cmd',
            'Tools/Native/Test-Generic-Nominal-Declarations.sh',
            'Tools/Native/Test-Generic-Nominal-Declarations.mjs'
        )
        Suites = @('generic-nominal-declarations')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 generic nominal declaration compiler routing'
        Paths = @('Compiler/Windvale/Source-Symbols-Core.wv')
        Suites = @(
            'source-containment',
            'generic-nominal-type-binding',
            'generic-nominal-type-layout',
            'generic-nominal-type-materialization',
            'generic-nominal-wvlb-carrier',
            'language-1-front-door'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 declaration parser integration routing'
        Paths = @('Compiler/Windvale/Source-Declaration-Parser.wv')
        Suites = @(
            'source-containment',
            'generic-nominal-type-binding',
            'generic-nominal-type-layout',
            'generic-nominal-type-materialization',
            'generic-nominal-wvlb-carrier',
            'language-1-front-door'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 body parser integration routing'
        Paths = @('Compiler/Windvale/Source-Body-Parser.wv')
        Suites = @(
            'source-containment',
            'generic-nominal-type-binding',
            'generic-nominal-type-layout',
            'generic-nominal-type-materialization',
            'generic-nominal-wvlb-carrier',
            'language-1-front-door'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Seed native front-door source owners'
        Paths = @(
            'Tools/Native/Test-Seed-Native-Front-Door.cmd',
            'Tools/Native/Test-Seed-Native-Front-Door.sh',
            'Examples/Seed/Hello-Windvale.wvproj',
            'Examples/Foundation/Read-Wvb-Header.wvproj',
            'Foundation/Machine-Contracts.wvproj',
            'Foundation/Byte-Ordering.wvproj',
            'Foundation/Decimal-Parsing.wvproj',
            'Foundation/Byte-Construction.wvproj',
            'Examples/Compiler/Native-Stencil-Demo.wv',
            'Projects/Examples/Native-Stencil-Demo.wvproj',
            'Projects/Examples/Foundation-Machine-Contracts-Demo.wvproj',
            'Projects/Examples/Foundation-Byte-Ordering-Demo.wvproj',
            'Projects/Examples/Foundation-Decimal-Parsing-Demo.wvproj',
            'Projects/Examples/Foundation-Byte-Construction-Demo.wvproj'
        )
        Suites = @('seed', 'seed-native-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Seed native front-door smoke owner'
        Paths = @(
            'Tools/Native/Test-Seed-Native-Front-Door.cmd',
            'Tools/Native/Test-Seed-Native-Front-Door.sh'
        )
        Suites = @('seed-native-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Seed native front-door reconstruction owner'
        Paths = @(
            'Tools/Verify/Verify-Seed-Native-Front-Door-Reconstruction.ps1',
            'Tools/Verify/Verify-Seed-Native-Front-Door-Reconstruction.sh'
        )
        Suites = @('seed-native-front-door-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'superseded Seed native front-door reconstruction names'
        Paths = @(
            'Tools/Verify/Verify-Seed-Native-Front-Door.ps1',
            'Tools/Verify/Verify-Seed-Native-Front-Door.sh'
        )
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'Seed native Foundation source transfer'
        Paths = @(
            'Foundation/Machine-Contracts.wv',
            'Examples/Foundation/Machine-Contracts-Demo.wv'
        )
        Suites = @('seed', 'seed-native-front-door', 'wv-linker-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Seed native compiler-service source transfer'
        Paths = @(
            'Compiler/Windvale/Source-Lexer-Core.wv',
            'Compiler/Windvale/Source-Declaration-Parser.wv',
            'Compiler/Windvale/Source-Body-Parser.wv',
            'Compiler/Windvale/Source-Profile-Core.wv',
            'Compiler/Windvale/Source-Set-Core.wv',
            'Compiler/Windvale/Source-Graph-Core.wv',
            'Compiler/Windvale/Source-Symbols-Core.wv',
            'Compiler/Windvale/Source-Bindings-Core.wv',
            'Compiler/Windvale/Source-Wir-Core.wv',
            'Compiler/Windvale/Source-Wvb-Core.wv',
            'Compiler/Windvale/Source-Wvb-Temporary-Slots.wv',
            'Examples/Compiler/Source-Lexer-Demo.wv',
            'Examples/Compiler/Source-Declaration-Parser-Demo.wv',
            'Examples/Compiler/Source-Declaration-Parser-Tool.wv',
            'Examples/Compiler/Source-Body-Parser-Demo.wv',
            'Examples/Compiler/Source-Body-Parser-Tool.wv',
            'Examples/Compiler/Source-Set-Demo.wv',
            'Examples/Compiler/Source-Set-Tool.wv',
            'Examples/Compiler/Source-Graph-Demo.wv',
            'Examples/Compiler/Source-Graph-Tool.wv',
            'Examples/Compiler/Source-Symbols-Demo.wv',
            'Examples/Compiler/Source-Symbols-Tool.wv',
            'Examples/Compiler/Source-Bindings-Demo.wv',
            'Examples/Compiler/Source-Bindings-Tool.wv',
            'Examples/Compiler/Source-Wir-Demo.wv',
            'Examples/Compiler/Source-Wir-Tool.wv',
            'Examples/Compiler/Source-Wvb-Demo.wv',
            'Examples/Compiler/Source-Wvb-Tool.wv',
            'Projects/Compiler/Windvale-Source-Lexer-Core.wvproj',
            'Projects/Examples/Windvale-Source-Lexer-Demo.wvproj',
            'Projects/Compiler/Windvale-Source-Declaration-Parser.wvproj',
            'Projects/Examples/Windvale-Source-Declaration-Parser-Demo.wvproj',
            'Projects/Examples/Windvale-Source-Declaration-Parser-Tool.wvproj',
            'Projects/Compiler/Windvale-Source-Body-Parser.wvproj',
            'Projects/Examples/Windvale-Source-Body-Parser-Demo.wvproj',
            'Projects/Examples/Windvale-Source-Body-Parser-Tool.wvproj',
            'Projects/Compiler/Windvale-Source-Profile-Core.wvproj',
            'Projects/Compiler/Windvale-Source-Set-Core.wvproj',
            'Projects/Examples/Windvale-Source-Set-Demo.wvproj',
            'Projects/Examples/Windvale-Source-Set-Tool.wvproj',
            'Projects/Compiler/Windvale-Source-Graph-Core.wvproj',
            'Projects/Examples/Windvale-Source-Graph-Demo.wvproj',
            'Projects/Examples/Windvale-Source-Graph-Tool.wvproj',
            'Projects/Compiler/Windvale-Source-Symbols-Core.wvproj',
            'Projects/Examples/Windvale-Source-Symbols-Demo.wvproj',
            'Projects/Examples/Windvale-Source-Symbols-Tool.wvproj',
            'Projects/Compiler/Windvale-Source-Bindings-Core.wvproj',
            'Projects/Examples/Windvale-Source-Bindings-Demo.wvproj',
            'Projects/Examples/Windvale-Source-Bindings-Tool.wvproj',
            'Projects/Compiler/Windvale-Source-Wir-Core.wvproj',
            'Projects/Examples/Windvale-Source-Wir-Demo.wvproj',
            'Projects/Examples/Windvale-Source-Wir-Tool.wvproj',
            'Projects/Compiler/Windvale-Source-Wvb-Core.wvproj',
            'Projects/Examples/Windvale-Source-Wvb-Demo.wvproj',
            'Projects/Examples/Windvale-Compiler.wvproj',
            'Compiler/Windvale/Native-Stencil-Core.wv',
            'Compiler/Windvale/Native-Stencil-Bridge.wvproj',
            'Compiler/Windvale/Native-Enum-Metadata-Core.wvproj',
            'Compiler/Windvale/Native-Publication-Bridge.wv',
            'Compiler/Windvale/Native-Publication-Lifetime-Core.wvproj',
            'Compiler/Windvale/Native-Publication-Lifetime.wvproj',
            'Windvale-Native-Enum-Metadata.wvproj',
            'Projects/Runtime/Windvale-Native-Service-Bundle-Materialization-Core.wvproj'
        )
        Suites = @(
            'seed',
            'segmented-compiler-toolset-reconstruction',
            'unsafe-wvb',
            'source-containment',
            'generic-nominal-type-binding',
            'generic-nominal-type-layout',
            'generic-nominal-type-materialization',
            'generic-nominal-wvlb-carrier',
            'language-1-front-door',
            'lowerer-rejections',
            'console-packager-source-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'compiler build-driver project staging capacity'
        Paths = @('Projects/Tools/Windvale-Compiler-Build-Driver.wvproj')
        Suites = @(
            'seed',
            'segmented-compiler-toolset-reconstruction',
            'unsafe-wvb',
            'source-containment',
            'language-1-front-door',
            'lowerer-rejections',
            'console-packager-source-reconstruction',
            'workspace-project2'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Seed native source compiler product launcher'
        Paths = @(
            'Tools/Native/Build-Source-Compiler-Product.cmd',
            'Tools/Native/Build-Source-Compiler-Product.sh'
        )
        Suites = @(
            'seed',
            'compiler-reconstruction',
            'unsafe-wvb',
            'source-containment',
            'lowerer-rejections',
            'console-packager-source-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'native front-door artifacts select their admission owner'
        Paths = @(
            'Artifacts/Native-Front-Door/Manifest.json',
            'Artifacts/Native-Front-Door/SHA256SUMS'
        )
        Suites = @(
            'seed-native-front-door',
            'wvb-runner-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'installers'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'source compiler compilation benchmark'
        Paths = @('Tools/Native/Measure-Source-Wvb-Compilation.ps1')
        Suites = @(
            'seed',
            'unsafe-wvb',
            'source-containment',
            'lowerer-rejections',
            'console-packager-source-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Seed native runtime-service source transfer'
        Paths = @(
            'Runtime/Windvale/Native-X64-Utf8-Service-Core.wvproj',
            'Runtime/Windvale/Native-X64-Utf8-Service-Bridge.wv',
            'Runtime/Windvale/Native-X64-Integer-Format-Services.wvproj',
            'Runtime/Windvale/Native-X64-Service-Code-Builder.wv',
            'Runtime/Windvale/Native-X64-Output-Service-Windows.wvproj',
            'Runtime/Windvale/Native-X64-Output-Services-Bridge.wv',
            'Runtime/Windvale/Native-X64-File-Output-Service-Code.wvproj',
            'Runtime/Windvale/Native-X64-File-Output-Services.wvproj',
            'Runtime/Windvale/Native-X64-File-Input-Service-Linux.wv',
            'Runtime/Windvale/Native-X64-File-Input-Services.wvproj',
            'Runtime/Windvale/Native-X64-Text-Concat-Service-Core.wvproj',
            'Runtime/Windvale/Native-X64-Text-Quote-Service-Bridge.wv',
            'Runtime/Windvale/Native-X64-Enum-Name-Service.wvproj',
            'Runtime/Windvale/Native-Service-Bundle-Materialization-Core.wv',
            'Runtime/Windvale/Native-Output-Table-Core.wvproj',
            'Runtime/Windvale/Native-Execution-Context-Bridge.wv',
            'Runtime/Windvale/Native-Byte-Result-Admission.wvproj',
            'Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wvproj',
            'Runtime/Windvale/Native-Hosted-Tool-Runtime-Header-Bridge.wv',
            'Projects/Runtime/Windvale-Native-Hosted-Tool-Metadata-Construction-Core.wvproj',
            'Projects/Runtime/Windvale-Native-Hosted-Tool-Runtime-Header.wvproj',
            'Windvale-Native-X64-Text-Concat-Service.wvproj',
            'Windvale-Native-Output-Table.wvproj'
        )
        Suites = @(
            'seed',
            'seed-native-front-door',
            'unsafe-wvb',
            'wvb-containment'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Seed native hosted construction source transfer'
        Paths = @(
            'Linker/Windvale/Native-Hosted-Startup-Instantiation.wvproj',
            'Linker/Windvale/Native-Hosted-Container-Construction-Core.wv',
            'Linker/Windvale/Native-Hosted-Container-Segmentation.wv',
            'Windvale-Native-Hosted-Startup-Instantiation.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Container-Construction.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Container-Windows.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Container-Linux.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Container-Segmentation.wvproj'
        )
        Suites = @(
            'seed-native-front-door',
            'linker-rejections',
            'linker-hostile',
            'linker-map-limit'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Seed native console AOT qualification smoke'
        Paths = @(
            'Tools/Verify/Verify-Seed-Native-Console-Aot.ps1',
            'Tools/Verify/Verify-Seed-Native-Console-Aot.sh',
            'Examples/Seed/Sum-Data.wvproj',
            'Specifications/Seed-CLI.md'
        )
        Suites = @(
            'seed',
            'seed-native-front-door',
            'seed-native-console-aot',
            'wvb-runner-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Seed native console AOT audit owner'
        Paths = @(
            'Tools/Verify/Verify-Seed-Native-Console-Aot.ps1',
            'Tools/Verify/Verify-Seed-Native-Console-Aot.sh'
        )
        Suites = @('seed-native-console-aot')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'native baseline JIT owner'
        Paths = @(
            'Tools/Native/Test-Baseline-Jit-Publisher.cmd',
            'Compiler/Windvale/Baseline-Jit-Patch-Plan-Core.wv',
            'Runtime/Windvale/Baseline-Jit-Patch-Plan-Verifier-Core.wv',
            'Runtime/Native/Windows-X64-Baseline-Jit-Publisher.wva',
            'Tests/Fixtures/Native-X64/Baseline-Jit-Patch-Plan-Self-Test.wv',
            'Projects/Compiler/Windvale-Native-Baseline-Jit-Patch-Plan-Bridge.wvproj',
            'Artifacts/Baseline-Jit-Publisher/Manifest.json',
            'Specifications/Windvale-Native-Baseline-Jit-Publication.md'
        )
        Suites = @('baseline-jit')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'native random binary containment and WVB front-door specification owners'
        Paths = @(
            'Tools/Native/Random-Containment-Binary.mjs',
            'Specifications/Windvale-Native-Wvb-Read-Only-Front-Door.md'
        )
        Suites = @(
            'seed',
            'unsafe-wvb',
            'wvb-containment',
            'wvo-containment'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'native shared random containment host owner'
        Paths = @(
            'Tools/Native/Random-Containment-Host.mjs',
            'Tools/Native/Random-Containment-Corpus.mjs',
            'Tools/Native/Random-Containment-Source.mjs',
            'Tools/Native/Test-Random-Containment.mjs',
            'Tools/Native/Test-Source-Containment.cmd',
            'Tools/Native/Test-Source-Containment.sh',
            'Tests/Native/Random-Containment/Corpus.tar.gz.b64',
            'Specifications/Windvale-Native-Random-Containment-Tests.md'
        )
        Suites = @(
            'wvb-containment',
            'wvo-containment',
            'source-containment'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'native compiler reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Compiler-Reconstruction.cmd',
            'Tools/Native/Test-Compiler-Reconstruction.sh',
            'Tools/Native/Construct-Compiler-Reconstruction.cmd',
            'Artifacts/Native-Compiler-Reconstruction-Candidate/Manifest.json',
            'Specifications/Windvale-Native-Compiler-Reconstruction.md'
        )
        Suites = @('compiler-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'native compiler development oracle'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Function-Only.wvproj',
            'Tests/Fixtures/Source-Wvb/Function-Only.wv'
        )
        Suites = @('seed', 'compiler-reconstruction', 'language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'segmented compiler toolset reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Segmented-Compiler-Toolset-Reconstruction.cmd',
            'Tools/Native/Test-Segmented-Compiler-Packaging.cmd',
            'Tools/Native/Construct-Segmented-Compiler-Toolset.cmd',
            'Tools/Native/Package-Segmented-Compiler-Wvb.cmd',
            'Artifacts/Native-Segmented-Compiler-Toolset-Candidate/Manifest.json',
            'Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj',
            'Projects/Linker/Windvale-Compiler-Image-Staging.wvproj',
            'Projects/Linker/Windvale-Compiler-Image-Canonical-Transport.wvproj',
            'Specifications/Windvale-Native-Hosted-Container-Packaging.md'
        )
        Suites = @(
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'segmented compiler packaging benchmark'
        Paths = @('Tools/Native/Measure-Segmented-Compiler-Packaging.ps1')
        Suites = @('segmented-compiler-toolset-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'compiler-scale hosted publication specifications'
        Paths = @(
            'Specifications/Windvale-Native-Hosted-Metadata-Request.md',
            'Specifications/Windvale-Native-Hosted-Tool-Metadata-Construction.md',
            'Specifications/Windvale-Native-Publication-Lifetime.md',
            'Specifications/Windvale-Native-Publication-Plan.md',
            'Specifications/Windvale-Native-Service-Bundle-Materialization.md',
            'Specifications/Windvale-Native-Streaming-Sha256-Evidence.md'
        )
        Suites = @(
            'seed-native-front-door',
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'wvb-inspector-reconstruction',
            'wvo-inspector-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'console-packager-container-reconstruction',
            'hosted-verifier-publisher-files'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'shared segmented compiler construction primitives'
        Paths = @(
            'Tools/Native/Stage-Compiler-Wvb.cmd',
            'Tools/Native/Link-Staged-Compiler-Wvo.cmd',
            'Tools/Native/Transport-Compiler-Image.cmd'
        )
        Suites = @(
            'compiler-reconstruction',
            'segmented-compiler-toolset-reconstruction',
            'wvb-to-wvo-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'wvo-publisher-reconstruction',
            'console-packager-container-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'segmented compiler source closure'
        Paths = @(
            'Compiler/Windvale/Native-X64-Lowering-Core.wv',
            'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Envelope.wv'
        )
        Suites = @(
            'seed',
            'segmented-compiler-toolset-reconstruction',
            'wvb-to-wvo-reconstruction',
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'unsafe-wvb',
            'source-containment',
            'lowerer-rejections',
            'console-packager-source-reconstruction',
            'native-u64-lowering',
            'model-provider',
            'file-read-application',
            'database-superblock',
            'database-durable-commit',
            'database-storage',
            'wvdb-query-capability'
        )
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $false
    },
    @{
        Name = 'rights-reduced WVDB query host owner'
        Paths = @(
            'Runtime/Native/X64-Read-Only-Directory-Host.wva',
            'Runtime/Native/Windows-X64-Read-Only-Directory.wva',
            'Runtime/Native/Linux-X64-Read-Only-Directory.wva',
            'Tools/Native/Create-Wvdb-Query-Fixture.mjs'
        )
        Suites = @(
            'assembler-rejections',
            'assembler-golden',
            'wva-differential',
            'file-read-application',
            'wvdb-query-capability'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'segmented compiler linker closure'
        Paths = @(
            'Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image.wv',
            'Linker/Windvale/Compiler-Image-Transport-Resources.wv',
            'Specifications/Windvale-Linking.md',
            'Specifications/Windvale-Segmented-Hosted-Overlay.md',
            'Tools/Native/Compose-Segmented-Hosted-Overlay.ps1',
            'Runtime/Native/X64-Segmented-Hosted-Main-Trampoline.wva'
        )
        Suites = @(
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction',
            'linker-rejections',
            'linker-hostile',
            'linker-map-limit',
            'database-storage'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'segmented compiler immutable transport source'
        Paths = @('Foundation/Immutable-Source-Regions.wv')
        Suites = @(
            'seed',
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'WVB-to-WVO reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Wvb-To-Wvo-Reconstruction.cmd',
            'Tools/Native/Construct-Wvb-To-Wvo-Reconstruction.cmd',
            'Artifacts/Native-Wvb-To-Wvo-Candidate/Manifest.json',
            'Compiler/Windvale/Native-X64-Lowering-Metadata.wv',
            'Projects/Compiler/Windvale-Native-X64-Lowering.wvproj',
            'Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj',
            'Projects/Tests/Windvale-Native-Test-X64-Lowering-Metadata.wvproj',
            'Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj',
            'Tests/Fixtures/Native-X64/Native-X64-Lowering-Metadata-Self-Test.wv',
            'Tests/Fixtures/Native-X64/Wvb-To-Wvo-Metadata.wv',
            'Tests/Fixtures/Native-X64/Wvb-To-Wvo-Return-42.wv',
            'Specifications/Windvale-Native-Wvb-To-Wvo.md'
        )
        Suites = @(
            'wvb-to-wvo-reconstruction',
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'lowerer-rejections'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'portable WVB metadata-aware verifier owner'
        Paths = @(
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Metadata-Core.wv',
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Semantic-Core.wv',
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Executable-Core.wv',
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Typed-Directories.wv',
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Tool.wv',
            'Tools/Windvale.Verify/Wvb-Metadata-Normalization.wv',
            'Projects/Tests/Windvale-Wvb-Metadata-Normalization-Self-Test.wvproj',
            'Tests/Fixtures/Source-Wvb/Metadata-Normalization-Self-Test.wv'
        )
        Suites = @(
            'seed',
            'wvb-to-wvo-reconstruction',
            'wvb-inspector-reconstruction',
            'unsafe-wvb',
            'wvb-containment',
            'language-1-front-door',
            'model-provider',
            'file-read-application',
            'libraries'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Wv-Linker reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Wv-Linker-Reconstruction.cmd',
            'Tools/Native/Construct-Wv-Linker-Reconstruction.cmd',
            'Artifacts/Native-Wv-Linker-Candidate/Manifest.json',
            'Projects/Linker/Windvale-Wv-Linker.wvproj'
        )
        Suites = @(
            'seed-native-front-door',
            'wv-linker-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'installers'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Wv-Linker source and contract'
        Paths = @(
            'Linker/Windvale/Wv-Linker-Core.wv',
            'Foundation/Sha256.wv',
            'Specifications/Windvale-Native-Wv-Linker.md'
        )
        Suites = @(
            'seed',
            'seed-native-front-door',
            'wv-linker-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'linker-rejections',
            'linker-hostile',
            'linker-map-limit'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Wv-Linker segmented and hosted construction dependencies'
        Paths = @(
            'Tools/Native/Stage-Compiler-Wvb.cmd',
            'Artifacts/Native-Hosted-Container-Toolset-Candidate/Manifest.json'
        )
        Suites = @(
            'compiler-reconstruction',
            'segmented-compiler-toolset-reconstruction',
            'wvb-to-wvo-reconstruction',
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'wvb-inspector-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'console-packager-container-reconstruction',
            'hosted-verifier-publisher-files'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'current WVB-to-WVO source root'
        Paths = @('Compiler/Windvale/Native-X64-Lowering-Tool.wv')
        Suites = @(
            'seed',
            'wvb-to-wvo-reconstruction',
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'unsafe-wvb',
            'source-containment',
            'lowerer-rejections',
            'console-packager-source-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'compiler source-analysis implementation owner'
        Paths = @(
            'Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/Manifest.json',
            'Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/README.md',
            'Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/Wvb/wvemit.wvb',
            'Compiler/Windvale/Source-Analysis-Core.wv',
            'Compiler/Windvale/Source-Emission-Core.wv',
            'Projects/Tests/Language-1.0-Source-Analysis-Self-Test.wvproj',
            'Tools/Native/Compile-Compiler-Source-Set.cmd',
            'Tools/Native/Compile-Compiler-Source-Set.sh'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'compiler split development owner'
        Paths = @(
            'Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj',
            'Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj',
            'Specifications/Compiler-Split-Development-Cache.md',
            'Tools/Native/Build-Cached-Split-Project-Wvb.mjs',
            'Tools/Native/Test-Cached-Split-Project-Wvb.mjs',
            'Tools/Native/Test-Compiler-Split-Development.cmd',
            'Tools/Native/Test-Compiler-Split-Development.sh',
            'Tools/Native/Test-Compiler-Split-Development.mjs',
            'Tools/Native/Write-Split-Compiler-Producer-Identity.mjs',
            'Tools/Windvale.Build/Compiler-Analysis-Driver.wv',
            'Tools/Windvale.Build/Compiler-Emission-Driver.wv'
        )
        Suites = @('compiler-split-development')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'WVB runner reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Wvb-Runner-Reconstruction.cmd',
            'Tools/Native/Construct-Wvb-Runner-Reconstruction.cmd',
            'Artifacts/Native-Wvb-Runner-Candidate/Manifest.json',
            'Projects/Tools/Windvale-Wvb-Runner.wvproj',
            'Tools/Windvale.Run/Wvb-Runner-Tool.wv',
            'Specifications/Windvale-Native-Wvb-Runner.md'
        )
        Suites = @(
            'seed-native-front-door',
            'wvb-runner-reconstruction',
            'scripting'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'scripting owner'
        Paths = @(
            'Tools/Native/Test-Scripting.cmd',
            'Tools/Native/Test-Scripting.sh',
            'Tests/Fixtures/Scripting/Portable-Main.wv',
            'Specifications/Windvale-Scripting.md'
        )
        Suites = @('scripting')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'WVO inspector reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Wvo-Inspector-Reconstruction.cmd',
            'Tools/Native/Construct-Wvo-Inspector-Reconstruction.cmd',
            'Tools/Native/Check-Wvo.cmd',
            'Artifacts/Native-Wvo-Object-Candidate/Manifest.json',
            'Linker/Startup/Windows-X64-Hosted-Inspector.wva',
            'Linker/Startup/Linux-X64-Hosted-Inspector.wva',
            'Projects/Object-Model/Windvale-Wvo-Object.wvproj',
            'Specifications/Windvale-Native-Wvo-Inspector.md'
        )
        Suites = @(
            'seed-native-front-door',
            'wvb-runner-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'wvo-read-only',
            'wvo-differential',
            'wvo-containment',
            'wvo-hostile-size',
            'publisher-rejections'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'WVO inspector shared profile and object sources'
        Paths = @(
            'Runtime/Windvale/Native-Hosted-Verifier-Service-Bundle-Request-Core.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Startup-Targets.wv',
            'Object-Model/Windvale/Wvo-Object-Core.wv'
        )
        Suites = @(
            'seed-native-front-door',
            'wvb-runner-reconstruction',
            'wvb-inspector-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'wvo-read-only',
            'wvo-differential',
            'wvo-containment',
            'wvo-hostile-size',
            'publisher-rejections',
            'hosted-verifier-publisher-files'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'console verifier reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Console-Verifier-Reconstruction.cmd',
            'Tools/Native/Construct-Console-Verifier-Reconstruction.cmd',
            'Artifacts/Native-Console-Application-Verifier-Candidate/Manifest.json',
            'Projects/Tools/Windvale-Console-Application-Verifier.wvproj',
            'Tools/Windvale.Verify/Console-Application-Verifier-Tool.wv',
            'Specifications/Windvale-Console-Application-Verification.md'
        )
        Suites = @('console-verifier-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'console verifier source closure'
        Paths = @(
            'Foundation/Byte-Construction.wv',
            'Foundation/Sha256.wv',
            'Linker/Windvale/Console-Application-Admission-Core.wv',
            'Linker/Windvale/Console-Application-Construction-Core.wv',
            'Linker/Windvale/Console-Application-Plan-Core.wv',
            'Linker/Windvale/Console-Application-Verification-Core.wv',
            'Linker/Windvale/Hosted-Console-Application-Verification-Common.wv'
        )
        Suites = @(
            'seed',
            'seed-native-front-door',
            'wv-linker-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'linker-rejections',
            'linker-hostile',
            'linker-map-limit',
            'console-packager-source-reconstruction',
            'console-packager-container-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'console verifier shared hosted toolsets and startup'
        Paths = @(
            'Artifacts/Native-Hosted-Container-Toolset-Candidate/Manifest.json',
            'Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate/Manifest.json',
            'Linker/Startup/Windows-X64-Hosted-Inspector.wva',
            'Runtime/Windvale.Native/Consumers/Native-X64-Windows-File-Input-Service.bin'
        )
        Suites = @(
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'wvb-inspector-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'hosted-verifier-publisher-files'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'console publisher reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Console-Application-Publisher-Reconstruction.cmd',
            'Tools/Native/Construct-Console-Application-Publisher.cmd',
            'Artifacts/Native-Console-Application-Publisher-Candidate/Manifest.json',
            'Projects/Tools/Windvale-Console-Application-Publisher.wvproj',
            'Tools/Windvale.Publish/Console-Application-Publisher.wv',
            'Specifications/Windvale-Native-Console-Application-Publisher.md'
        )
        Suites = @('console-publisher-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'console publisher shared construction closure'
        Paths = @(
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Construction-Request-Core.wv',
            'Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate/Manifest.json'
        )
        Suites = @(
            'wvb-runner-reconstruction',
            'wvb-inspector-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'hosted-verifier-publisher-files'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'console publisher native construction dependencies'
        Paths = @(
            'Tools/Native/Build-Wvb.cmd',
            'Tools/Native/Lower-Wvb-To-Wvo.cmd',
            'Tools/Native/Link-Wvo.cmd'
        )
        Suites = @(
            'seed',
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'unsafe-wvb',
            'wvb-containment',
            'lowerer-rejections',
            'linker-rejections',
            'linker-hostile',
            'linker-map-limit',
            'wvo-export-renamer',
            'aot-chain'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'console publisher exact package and publication consumers'
        Paths = @(
            'Tools/Native/Package-Console.cmd',
            'Tools/Native/Publish-Console.cmd'
        )
        Suites = @(
            'console-publisher-reconstruction',
            'console-packager-rejections',
            'console-container-mutations',
            'hosted-console-container-mutations',
            'console-segmented-size',
            'console-segmented-construction',
            'console-packager-source-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'WVO publisher reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Wvo-Publisher-Reconstruction.cmd',
            'Tools/Native/Construct-Wvo-Publisher.cmd',
            'Artifacts/Native-Wvo-Publisher-Candidate/Manifest.json',
            'Projects/Tools/Windvale-Wvo-Publisher.wvproj',
            'Specifications/Windvale-Native-Wvo-Publisher.md'
        )
        Suites = @('wvo-publisher-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'WVO publisher shared publication source'
        Paths = @(
            'Tools/Windvale.Publish/Wvo-Publisher-Tool.wv',
            'Tools/Windvale.Publish/Wvb-Publication-Transaction.wv'
        )
        Suites = @(
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'publisher-rejections',
            'hosted-verifier-publisher-files'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'console packager container reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Console-Packager-Container-Reconstruction.cmd',
            'Tools/Native/Construct-Console-Packager-Reconstruction.cmd',
            'Artifacts/Native-Console-Packager-Candidate/Manifest.json',
            'Artifacts/Native-Console-Segmented-Packager-Candidate/Manifest.json'
        )
        Suites = @('console-packager-container-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'console packager reconstruction projects and contract'
        Paths = @(
            'Projects/Linker/Windvale-Console-Application-Packager.wvproj',
            'Projects/Linker/Windvale-Console-Application-Segmented-Packager.wvproj',
            'Specifications/Windvale-Native-Console-Packager.md'
        )
        Suites = @(
            'console-packager-source-reconstruction',
            'console-packager-container-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'current lowerer launcher and Wv-Linker consumer'
        Paths = @('Tools/Native/Lower-Wvb-To-Wvo.cmd')
        Suites = @(
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'lowerer-rejections',
            'wvo-export-renamer',
            'aot-chain'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Windvale compiler'
        Paths = @('Compiler/Windvale/Source-Wvb-Compiler.wv')
        Suites = @('seed', 'unsafe-wvb', 'source-containment', 'lowerer-rejections', 'console-packager-source-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'archived managed compiler source'
        Paths = @('Compiler/Reference/Seed-Compiler.cs')
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'WvDump native front-door construction owner'
        Paths = @(
            'Examples/Foundation/Wv-Dump-Core.wv',
            'Projects/Examples/Windvale-Wvb-Inspector.wvproj'
        )
        Suites = @('seed', 'seed-native-front-door', 'wvb-inspector-reconstruction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Windvale assembler'
        Paths = @(
            'Assembler/Windvale/Wva-Assembler-Core.wv',
            'Projects/Assembler/Windvale-Wva-Assembler.wvproj',
            'Examples/Assembler/Hello-Object.wva'
        )
        Suites = @(
            'seed-native-front-door',
            'assembler-rejections',
            'assembler-golden',
            'wva-differential'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Windvale linker'
        Paths = @('Linker/Windvale/Wv-Linker-Core.wv')
        Suites = @(
            'seed-native-front-door',
            'wv-linker-reconstruction',
            'console-publisher-reconstruction',
            'linker-rejections',
            'linker-hostile',
            'linker-map-limit'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS x64 process-machine emission ownership'
        Paths = @(
            'Operating-System/Architecture/X64-Code-Emission.wv',
            'Operating-System/Kernel/X64-Process-Coordinator-Emission.wv',
            'Operating-System/Kernel/X64-Process-Endpoint-Emission.wv',
            'Operating-System/Kernel/X64-Process-Memory-Allocation-Emission.wv',
            'Operating-System/Kernel/X64-Process-Record-Emission.wv',
            'Operating-System/Kernel/X64-Process-Paging-Emission.wv',
            'Operating-System/Kernel/X64-Process-Image-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Reservation-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Record-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Paging-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Image-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Program-Resource-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Budget-Resource-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Store-Resource-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Directory-Resource-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Store-Validation-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Directory-Validation-Emission.wv',
            'Operating-System/Kernel/X64-Process-Privileged-Entry-Emission.wv',
            'Operating-System/Kernel/X64-Process-Thread-Timer-State-Emission.wv',
            'Operating-System/Kernel/X64-Process-Timer-Activation-Emission.wv',
            'Operating-System/Kernel/X64-Process-Provider-User-Transfer-Emission.wv',
            'Operating-System/Kernel/X64-Process-Provider-Return-Init-Transfer-Emission.wv',
            'Operating-System/Kernel/X64-Process-Init-Return-Program-Validation-Emission.wv',
            'Operating-System/Kernel/X64-Process-Init-Return-Budget-Validation-Emission.wv',
            'Operating-System/Kernel/X64-Process-Init-Return-Store-Directory-Validation-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-User-Transfer-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Return-Init-Transfer-Emission.wv',
            'Operating-System/Kernel/X64-Process-Init-Reply-Publish-Resume-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Reply-Delivery-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Directory-Request-Delivery-Emission.wv',
            'Operating-System/Kernel/X64-Process-Directory-Reply-Publish-Resume-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Directory-Reply-Delivery-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Completion-Cleanup-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Reclamation-Preflight-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Memory-Recycle-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Record-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Paging-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Image-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Endpoint-Rebind-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Reentry-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Return-Validation-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-User-Transfer-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Return-Init-Transfer-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Init-Reply-Publish-Resume-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Reply-Delivery-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Directory-Request-Delivery-Emission.wv',
            'Operating-System/Kernel/X64-Process-Directory-Generation-Two-Reply-Publish-Resume-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Directory-Reply-Lifecycle-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Completion-Cleanup-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Completion-Finalize-Resume-Emission.wv',
            'Operating-System/Kernel/X64-Process-Final-State-Validation-Epilogue-Emission.wv',
            'Operating-System/Kernel/X64-Process-Directory-Allocation-Emission.wv',
            'Operating-System/Kernel/X64-Process-Directory-Record-Emission.wv',
            'Operating-System/Kernel/X64-Process-Directory-Paging-Emission.wv',
            'Operating-System/Kernel/X64-Process-Directory-Image-Emission.wv',
            'Operating-System/Kernel/X64-Process-Entry-Emission.wv',
            'Projects/Operating-System/Windvale-Os-X64-Process-Coordinator-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Endpoint-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Memory-Allocation-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Record-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Paging-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Image-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Reservation-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Record-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Paging-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Image-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Program-Resource-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Budget-Resource-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Store-Resource-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Directory-Resource-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Store-Validation-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Directory-Validation-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Privileged-Entry-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Thread-Timer-State-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Timer-Activation-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Provider-User-Transfer-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Provider-Return-Init-Transfer-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Init-Return-Program-Validation-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Init-Return-Budget-Validation-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Init-Return-Store-Directory-Validation-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-User-Transfer-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Return-Init-Transfer-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Init-Reply-Publish-Resume-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Reply-Delivery-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Directory-Request-Delivery-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Directory-Reply-Publish-Resume-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Directory-Reply-Delivery-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Completion-Cleanup-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Reclamation-Preflight-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Memory-Recycle-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Record-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Paging-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Image-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Endpoint-Rebind-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Reentry-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Return-Validation-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-User-Transfer-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Return-Init-Transfer-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Init-Reply-Publish-Resume-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Reply-Delivery-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Directory-Request-Delivery-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Directory-Generation-Two-Reply-Publish-Resume-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Directory-Reply-Lifecycle-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Completion-Cleanup-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Client-Generation-Two-Completion-Finalize-Resume-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Final-State-Validation-Epilogue-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Directory-Allocation-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Directory-Record-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Directory-Paging-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Directory-Image-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Code-Emission.wvproj',
            'Projects/Operating-System/Windvale-Os-X64-Process-Entry-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Coordinator-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Endpoint-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Memory-Allocation-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Record-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Paging-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Image-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Reservation-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Record-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Paging-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Image-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Program-Resource-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Budget-Resource-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Store-Resource-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Directory-Resource-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Store-Validation-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Directory-Validation-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Privileged-Entry-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Thread-Timer-State-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Timer-Activation-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Provider-User-Transfer-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Provider-Return-Init-Transfer-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Init-Return-Program-Validation-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Init-Return-Budget-Validation-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Init-Return-Store-Directory-Validation-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-User-Transfer-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Return-Init-Transfer-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Init-Reply-Publish-Resume-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Reply-Delivery-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Directory-Request-Delivery-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Reply-Publish-Resume-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Directory-Reply-Delivery-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Completion-Cleanup-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Reclamation-Preflight-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Memory-Recycle-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Record-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Paging-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Image-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Endpoint-Rebind-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Reentry-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Return-Validation-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-User-Transfer-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Return-Init-Transfer-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Init-Reply-Publish-Resume-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Reply-Delivery-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Directory-Request-Delivery-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Generation-Two-Reply-Publish-Resume-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Directory-Reply-Lifecycle-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Completion-Cleanup-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Completion-Finalize-Resume-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Final-State-Validation-Epilogue-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Allocation-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Record-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Paging-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Directory-Image-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Code-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Entry-Emission.wvproj',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Coordinator-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Endpoint-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Memory-Allocation-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Record-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Paging-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Image-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Reservation-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Record-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Paging-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Image-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Program-Resource-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Budget-Resource-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Store-Resource-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Directory-Resource-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Store-Validation-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Directory-Validation-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Privileged-Entry-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Thread-Timer-State-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Timer-Activation-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Provider-User-Transfer-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Provider-Return-Init-Transfer-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Init-Return-Program-Validation-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Init-Return-Budget-Validation-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Init-Return-Store-Directory-Validation-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-User-Transfer-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Return-Init-Transfer-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Init-Reply-Publish-Resume-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Reply-Delivery-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Directory-Request-Delivery-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Directory-Reply-Publish-Resume-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Directory-Reply-Delivery-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Completion-Cleanup-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Reclamation-Preflight-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Memory-Recycle-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Record-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Paging-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Image-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Endpoint-Rebind-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Reentry-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Return-Validation-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-User-Transfer-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Return-Init-Transfer-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Init-Reply-Publish-Resume-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Reply-Delivery-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Directory-Request-Delivery-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Directory-Generation-Two-Reply-Publish-Resume-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Directory-Reply-Lifecycle-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Completion-Cleanup-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Completion-Finalize-Resume-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Final-State-Validation-Epilogue-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Directory-Allocation-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Directory-Record-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Directory-Paging-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Directory-Image-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Code-Emission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Entry-Emission-Self-Test.wv'
        )
        Suites = @('os-x64-code-emission')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS x64 filesystem-machine emission ownership'
        Paths = @(
            'Operating-System/Kernel/X64-Process-Filesystem-Record-Emission.wv',
            'Projects/Operating-System/Windvale-Os-X64-Process-Filesystem-Paging-Emission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Filesystem-Image-Emission.wvproj',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Filesystem-Record-Emission-Self-Test.wv',
            'Specifications/Windvale-Os-X64-Process-Filesystem-Machine-Emission.md',
            'Tools/Native/Test-Os-X64-Process-Filesystem-Machine-Emission.cmd'
        )
        Suites = @('os-x64-filesystem-machine-emission')
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
        Name = 'normal process object candidate and boot verifier ownership'
        Paths = @(
            'Artifacts/Native-Os-Process-Object-Toolset-Candidate/Manifest.json',
            'Artifacts/Native-Os-Process-Object-Toolset-Candidate/normal-x64-process.bin',
            'Artifacts/Native-Os-Process-Object-Toolset-Candidate/SHA256SUMS',
            'Tools/Verify/Verify-Os-Boot.ps1'
        )
        Suites = @('os-process-object', 'os-probe')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'protected process specification'
        Paths = @('Specifications/Windvale-Protected-Process.md')
        Suites = @('os-process-policy', 'os-process-object', 'os-probe')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS service source and project ownership'
        Paths = @(
            'Operating-System/Services/Resource-Service-Core.wv',
            'Projects/Operating-System/Windvale-Os-Directory-Snapshot-Service.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Resource-Service.wvproj',
            'Tests/Fixtures/Operating-System/Os-Directory-Service-Self-Test.wv'
        )
        Suites = @('os-services')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS resource-domain policy ownership'
        Paths = @(
            'Operating-System/Kernel/Resource-Domain-Policy.wv',
            'Operating-System/Kernel/Resource-Domain-Record.wv',
            'Projects/Operating-System/Windvale-Os-Resource-Domain-Policy.wvproj',
            'Projects/Operating-System/Windvale-Os-Resource-Domain-Record.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Resource-Domain.wvproj',
            'Tests/Fixtures/Operating-System/Os-Resource-Domain-Self-Test.wv',
            'Specifications/Windvale-Os-Resource-Domain-Policy.md',
            'Specifications/Windvale-Os-Resource-Domain-Record.md'
        )
        Suites = @('os-resource-domain')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS application-launch policy ownership'
        Paths = @(
            'Operating-System/Kernel/Application-Launch-Policy.wv',
            'Operating-System/Kernel/Application-Start-Request.wv',
            'Operating-System/Kernel/Application-Start-User-Copy.wv',
            'Operating-System/Kernel/X64-Application-Start-User-Copy.wva',
            'Operating-System/Kernel/X64-Application-Start-Syscall-Context.wva',
            'Operating-System/Kernel/Application-Machine-Construction-Policy.wv',
            'Projects/Operating-System/Windvale-Os-Application-Launch-Policy.wvproj',
            'Projects/Operating-System/Windvale-Os-Application-Start-Request.wvproj',
            'Projects/Operating-System/Windvale-Os-Application-Start-User-Copy.wvproj',
            'Projects/Operating-System/Windvale-Os-Application-Machine-Construction-Policy.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Application-Launch.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Application-Start-Request.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Application-Start-User-Copy.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Application-Machine-Construction.wvproj',
            'Tests/Fixtures/Operating-System/Os-Application-Launch-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-Application-Start-Request-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-Application-Start-User-Copy-Self-Test.wv',
            'Tests/Native/X64-Application-Start-User-Copy-Self-Test.wva',
            'Tests/Native/X64-Application-Start-Syscall-Context-Self-Test.wva',
            'Tests/Fixtures/Operating-System/Os-Application-Machine-Construction-Self-Test.wv',
            'Specifications/Windvale-Os-Application-Launch-Policy.md',
            'Specifications/Windvale-Os-Application-Start-Request.md',
            'Specifications/Windvale-Os-Application-Start-User-Copy.md'
        )
        Suites = @('os-process-policy', 'os-application-launch', 'os-probe')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS provider-launch transaction ownership'
        Paths = @(
            'Operating-System/Kernel/Provider-Launch-Transaction-Policy.wv',
            'Operating-System/Kernel/Provider-Launch-Lifecycle-Policy.wv',
            'Projects/Operating-System/Windvale-Os-Provider-Launch-Transaction-Policy.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Provider-Launch-Transaction.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Provider-Launch-Lifecycle.wvproj',
            'Tests/Fixtures/Operating-System/Os-Provider-Launch-Transaction-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-Provider-Launch-Lifecycle-Self-Test.wv',
            'Specifications/Windvale-Os-Provider-Launch-Transaction.md'
        )
        Suites = @('os-provider-launch-transaction')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS boot-service composition ownership'
        Paths = @(
            'Operating-System/Kernel/Process-Foundation.wv',
            'Specifications/Windvale-Os-Boot-Service-Composition.md'
        )
        Suites = @('os-process-policy', 'os-probe')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS endpoint transfer profile ownership'
        Paths = @(
            'Operating-System/Kernel/Endpoint-Transfer-Profile.wv',
            'Projects/Operating-System/Windvale-Os-Endpoint-Transfer-Profile.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Endpoint-Transfer-Profile.wvproj',
            'Tests/Fixtures/Operating-System/Os-Endpoint-Transfer-Profile-Self-Test.wv',
            'Specifications/Windvale-Os-Endpoint-Transfer-Profile.md'
        )
        Suites = @('os-endpoint-transfer', 'native-u64-lowering')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS FAT32 volume-chain-directory-file ownership'
        Paths = @(
            'Operating-System/Services/Fat32-Volume-Admission.wv',
            'Operating-System/Services/Fat32-Cluster-Chain.wv',
            'Operating-System/Services/Fat32-Directory-Admission.wv',
            'Operating-System/Services/Fat32-File-Read-Plan.wv',
            'Projects/Operating-System/Windvale-Os-Fat32-Volume-Admission.wvproj',
            'Projects/Operating-System/Windvale-Os-Fat32-Cluster-Chain.wvproj',
            'Projects/Operating-System/Windvale-Os-Fat32-Directory-Admission.wvproj',
            'Projects/Operating-System/Windvale-Os-Fat32-File-Read-Plan.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Fat32-Volume-Admission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Fat32-Directory-Admission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Fat32-File-Read-Plan.wvproj',
            'Tests/Fixtures/Operating-System/Os-Fat32-Volume-Admission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-Fat32-Directory-Admission-Self-Test.wv',
            'Tests/Fixtures/Operating-System/Os-Fat32-File-Read-Plan-Self-Test.wv',
            'Specifications/Windvale-Os-Fat32-Volume-Admission.md',
            'Specifications/Windvale-Os-Fat32-Cluster-Chain.md',
            'Specifications/Windvale-Os-Fat32-Directory-Admission.md',
            'Specifications/Windvale-Os-Fat32-File-Read-Plan.md'
        )
        Suites = @('os-fat32-volume', 'native-u64-lowering')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS FAT32 block-read ownership'
        Paths = @(
            'Operating-System/Services/Fat32-Block-Read-Transaction.wv',
            'Operating-System/Services/Fat32-Block-Provider-Protocol.wv',
            'Operating-System/Services/Fat32-Block-Exchange-State.wv',
            'Operating-System/Services/Fat32-Block-Image-Provider.wv',
            'Projects/Operating-System/Windvale-Os-Fat32-Block-Read-Transaction.wvproj',
            'Projects/Operating-System/Windvale-Os-Fat32-Block-Provider-Protocol.wvproj',
            'Projects/Operating-System/Windvale-Os-Fat32-Block-Exchange-State.wvproj',
            'Projects/Operating-System/Windvale-Os-Fat32-Block-Image-Provider.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Fat32-Block-Read-Transaction.wvproj',
            'Tests/Fixtures/Operating-System/Os-Fat32-Block-Read-Transaction-Self-Test.wv',
            'Specifications/Windvale-Os-Fat32-Block-Read-Transaction.md',
            'Specifications/Windvale-Os-Fat32-Block-Provider-Protocol.md',
            'Specifications/Windvale-Os-Fat32-Block-Exchange-State.md',
            'Specifications/Windvale-Os-Fat32-Block-Image-Provider.md'
        )
        Suites = @('os-fat32-block-read', 'native-u64-lowering')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS FAT32 file-read transaction ownership'
        Paths = @(
            'Operating-System/Services/Fat32-Chain-Position.wv',
            'Operating-System/Services/Fat32-File-Read-Transaction.wv',
            'Projects/Operating-System/Windvale-Os-Fat32-Chain-Position.wvproj',
            'Projects/Operating-System/Windvale-Os-Fat32-File-Read-Transaction.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Fat32-File-Read-Transaction.wvproj',
            'Tests/Fixtures/Operating-System/Os-Fat32-File-Read-Transaction-Self-Test.wv',
            'Specifications/Windvale-Os-Fat32-File-Read-Transaction.md'
        )
        Suites = @('os-fat32-file-read', 'native-u64-lowering')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'portable network authority ownership'
        Paths = @(
            'Libraries/Platform/Networking/Network-Authority.wv',
            'Projects/Libraries/Windvale-Library-Network-Authority.wvproj',
            'Tests/Fixtures/Libraries/Network-Authority-Self-Test.wv',
            'Projects/Tests/Windvale-Native-Test-Network-Authority.wvproj',
            'Specifications/Windvale-Network-Authority.md'
        )
        Suites = @(
            'os-network-authority',
            'native-u64-lowering',
            'workspace-project2'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'unconstructed OS process fault scenario'
        Paths = @('Operating-System/Kernel/Hello-Service-Fault.wv')
        Suites = @()
        Gaps = @('os-process-fault-scenario-construction')
        VerifyPlan = $false
    },
    @{
        Name = 'OS Probe memory producer artifacts'
        Paths = @(
            'Artifacts/Native-Os-Probe-Memory-Object-Producer-Candidate/Manifest.json',
            'Artifacts/Native-Os-Probe-Memory-Object-Producer-Candidate/windows-x64-os-probe-memory-object.exe',
            'Specifications/Windvale-Os-Memory-Object-Producer.md'
        )
        Suites = @('os-probe-object', 'os-probe')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'OS Probe object producer candidate artifacts'
        Paths = @(
            'Artifacts/Native-Os-Probe-Object-Producer-Candidate/Manifest.json',
            'Artifacts/Native-Os-Probe-Object-Producer-Candidate/Os-Probe-Object-Producer.wvb',
            'Artifacts/Native-Os-Probe-Object-Producer-Candidate/SHA256SUMS',
            'Artifacts/Native-Os-Probe-Object-Producer-Candidate/linux-x64-os-probe-object.elf',
            'Artifacts/Native-Os-Probe-Object-Producer-Candidate/windows-x64-os-probe-object.exe'
        )
        Suites = @('os-probe-object', 'os-probe')
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
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Application-Admission.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Application-Tool.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Object-Instantiation-Core.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Windows-Imports-Core.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Linux-Materialization-Core.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Publisher-Windows-Materialization-Core.wv',
            'Runtime/Windvale/Native-Hosted-Verifier-Publisher-Base-Metadata-Core.wv',
            'Runtime/Windvale/Native-Hosted-Verifier-Publisher-Base-Runtime-Tool.wv',
            'Runtime/Windvale/Native-Hosted-Verifier-Metadata-Admission.wv',
            'Linker/Windvale/Native-Hosted-Verifier-Layout-Core.wv',
            'Tools/Windvale.Publish/Native-Hosted-Verifier-Application-Publisher.wv',
            'Tools/Windvale.Publish/Native-Hosted-Verifier-Publisher-Promoter.wv',
            'Tools/Native/Publish-Hosted-Verifier-Application.cmd',
            'Tools/Native/Construct-Hosted-Verifier-Publisher.cmd',
            'Tools/Native/Construct-Hosted-Verifier-Publisher-Admitter.cmd',
            'Tools/Native/Admit-Hosted-Verifier-Publisher.cmd',
            'Artifacts/Native-Hosted-Verifier-Application-Publisher-Candidate/Manifest.json',
            'Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate/Manifest.json',
            'Artifacts/Native-Hosted-Verifier-Publisher-Admission-Candidate/Manifest.json',
            'Specifications/Windvale-Hosted-Verifier-Application.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Application-Publisher.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Application-Publisher-Metadata.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Construction-Requests.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Application-Admission.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Promotion.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Object-Instantiation.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Windows-Imports.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Linux-Materialization.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Publisher-Windows-Materialization.md',
            'Projects/Tools/Windvale-Native-Hosted-Verifier-Application-Publisher.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Verifier-Application-Publisher-Metadata.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Construction-Request.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Application-Tool.wvproj',
            'Projects/Tools/Windvale-Native-Hosted-Verifier-Publisher-Promoter.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Object-Instantiation.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Windows-Imports.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Linux-Materialization.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Windows-Materialization.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Target-Request-Tool.wvproj',
            'Projects/Runtime/Windvale-Native-Hosted-Verifier-Publisher-Base-Metadata-Tool.wvproj',
            'Projects/Linker/Windvale-Native-Hosted-Verifier-Application-Tool.wvproj',
            'Projects/Tools/Windvale-Wvb-Publisher.wvproj'
        )
        Suites = @(
            'wvb-runner-reconstruction',
            'wvb-inspector-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'publisher-rejections',
            'hosted-verifier-publisher-files'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'hosted verifier publisher promoter dispatch'
        Paths = @(
            'Tools/Native/Construct-Hosted-Verifier-Publisher-Promoter.cmd',
            'Tools/Native/Install-Hosted-Verifier-Publisher.cmd',
            'Artifacts/Native-Hosted-Verifier-Publisher-Promoter-Candidate/Manifest.json'
        )
        Suites = @('publisher-rejections', 'hosted-verifier-publisher-files')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'WVB publisher construction dispatch'
        Paths = @(
            'Tools/Native/Construct-Wvb-Publisher.cmd',
            'Artifacts/Native-Wvb-Publisher-Candidate/Manifest.json'
        )
        Suites = @('hosted-verifier-publisher-files')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'WebAssembly engine checkpoint owner'
        Paths = @(
            'Tools/Verify/Verify-WebAssembly-Engine.ps1',
            'Tools/Website/Verify-WebAssembly-Playground-Package.mjs',
            'Tools/Website/Verify-WebAssembly-Compiler-Core.mjs',
            'Tools/Windvale.Playground/wwwroot/js/windvale-compiler-core.js',
            'Artifacts/WebAssembly-Playground/Manifest.json',
            'Artifacts/WebAssembly-Playground/Windvale-Compiler-Direct.wasm',
            'Artifacts/WebAssembly-Playground/Wvb-Scalar-Interpreter.wasm'
        )
        Suites = @('source-containment')
        Gaps = @()
        VerifyPlan = $false
        VerifyWebAssemblyEngine = $true
    },
    @{
        Name = 'WebAssembly standalone native owner'
        Paths = @(
            'Tools/Verify/Verify-WebAssembly-Engine.ps1',
            'Tools/Verify/Verify-WebAssembly.ps1',
            'Tools/Verify/Verify-WebAssembly-Engine.mjs',
            'Tools/WebAssembly/Build-Compiler-Wvb.mjs',
            'Tools/WebAssembly/Compile-Wvb-To-Wasm.mjs',
            'Artifacts/WebAssembly-Native-Backend/Manifest.json'
        )
        Suites = @()
        Gaps = @()
        VerifyPlan = $false
        VerifyWebAssembly = $true
    },
    @{
        Name = 'WebAssembly source and contract owner'
        Paths = @(
            'Compiler/Windvale/WebAssembly-Core.wv',
            'Examples/Compiler/WebAssembly-Tool.wv',
            'Projects/Examples/Windvale-WebAssembly.wvproj',
            'Specifications/Windvale-WebAssembly.md'
        )
        Suites = @(
            'seed',
            'unsafe-wvb',
            'source-containment',
            'lowerer-rejections',
            'console-packager-source-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
        VerifyWebAssembly = $true
    },
    @{
        Name = 'GitHub native qualification workflow owner'
        Paths = @('.github/workflows/verify.yml')
        Suites = @()
        Gaps = @()
        VerifyPlan = $false
        VerifyGitHub = $true
    },
    @{
        Name = 'GitHub native qualification verifier owner'
        Paths = @('Tools/Verify/Verify-GitHub-Native-Qualification.ps1')
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
        VerifyGitHub = $true
    },
    @{
        Name = 'model protocol owner'
        Paths = @(
            'Libraries/Models/Model-Protocol.wv',
            'Libraries/Models/Scripted-Model-Provider.wv',
            'Tests/Fixtures/Models/Model-Protocol-Self-Test.wv',
            'Projects/Libraries/Windvale-Library-Model-Protocol.wvproj',
            'Projects/Libraries/Windvale-Library-Scripted-Model-Provider.wvproj',
            'Projects/Tests/Windvale-Native-Test-Model-Protocol.wvproj',
            'Specifications/Windvale-Model-Protocol.md'
        )
        Suites = @('model-provider', 'workspace-project2', 'libraries')
        Gaps = @()
        VerifyPlan = $false
        LibraryDevelopment = $true
        LibraryTarget = 'models'
    },
    @{
        Name = 'external model reference owner'
        Paths = @(
            'Tools/Models/External-Model-Reference-Core.mjs',
            'Tools/Models/External-Model-Reference.mjs',
            'Tools/Models/Test-External-Model-Reference.mjs',
            'Tools/Native/Test-External-Model-Reference.cmd',
            'Tools/Native/Test-External-Model-Reference.sh',
            'Specifications/Windvale-External-Model-Reference.md'
        )
        Suites = @('external-model-reference')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'shared host stream mechanism owners'
        Paths = @(
            'Runtime/Hosted/Network/Host-Network-Protocol.mjs',
            'Runtime/Hosted/Network/Host-Network-Provider-Core.mjs',
            'Runtime/Hosted/Network/Host-Network-Provider-Process.mjs',
            'Runtime/Hosted/Network/Host-Network-Supervisor.mjs'
        )
        Suites = @('host-network-provider', 'host-tls-provider')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'host network provider owner'
        Paths = @(
            'Runtime/Hosted/Network/Host-Network-Provider.mjs',
            'Tools/Network/Test-Host-Network-Provider.mjs',
            'Tools/Native/Test-Host-Network-Provider.cmd',
            'Tools/Native/Test-Host-Network-Provider.sh',
            'Specifications/Host-Network-Provider.md'
        )
        Suites = @('host-network-provider')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'host TLS provider owner'
        Paths = @(
            'Runtime/Hosted/Network/Host-Tls-Provider-Core.mjs',
            'Runtime/Hosted/Network/Host-Tls-Provider.mjs',
            'Runtime/Hosted/Network/Host-Tls-Supervisor.mjs',
            'Tools/Network/Ephemeral-Tls-Fixture.mjs',
            'Tools/Network/Test-Host-Tls-Provider.mjs',
            'Tools/Native/Test-Host-Tls-Provider.cmd',
            'Tools/Native/Test-Host-Tls-Provider.sh',
            'Specifications/Host-Tls-Provider.md'
        )
        Suites = @('host-tls-provider')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'bounded HTTPS owner'
        Paths = @(
            'Runtime/Hosted/Http/Bounded-Http1.mjs',
            'Runtime/Hosted/Http/Bounded-Https-Client.mjs',
            'Tools/Network/Test-Bounded-Https.mjs',
            'Tools/Native/Test-Bounded-Https.cmd',
            'Tools/Native/Test-Bounded-Https.sh',
            'Specifications/Bounded-Https.md'
        )
        Suites = @('bounded-https')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'protected credential owner'
        Paths = @(
            'Runtime/Hosted/Credentials/Protected-Credential.mjs',
            'Tools/Credentials/Test-Protected-Credential.mjs',
            'Tools/Native/Test-Protected-Credential.cmd',
            'Tools/Native/Test-Protected-Credential.sh',
            'Specifications/Protected-Provider-Credential.md'
        )
        Suites = @('protected-credential')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'external model gateway owner'
        Paths = @(
            'Runtime/Hosted/Models/External-Model-Gateway-Core.mjs',
            'Runtime/Hosted/Models/External-Model-Gateway-Process.mjs',
            'Runtime/Hosted/Models/External-Model-Gateway-Protocol.mjs',
            'Runtime/Hosted/Models/External-Model-Gateway-Supervisor.mjs',
            'Tools/Models/Test-External-Model-Gateway-Core.mjs',
            'Tools/Models/Test-Supervised-External-Model-Gateway.mjs',
            'Tools/Native/Test-External-Model-Gateway.cmd',
            'Tools/Native/Test-External-Model-Gateway.sh',
            'Specifications/Supervised-External-Model-Gateway.md'
        )
        Suites = @('external-model-gateway', 'native-external-model-gateway')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'hosted model chat owner'
        Paths = @(
            'Applications/Model-Chat/Model-Chat-Core.mjs',
            'Applications/Model-Chat/Windvale-Model-Chat.mjs',
            'Applications/Model-Chat/Windvale-Model-Chat.cmd',
            'Applications/Model-Chat/Windvale-Model-Chat.sh',
            'Runtime/Hosted/Models/External-Model-Gateway-Client.mjs',
            'Tools/Models/Test-Model-Chat.mjs',
            'Tools/Native/Test-Model-Chat.cmd',
            'Tools/Native/Test-Model-Chat.sh',
            'Specifications/Hosted-Model-Chat-Command.md'
        )
        Suites = @('model-chat')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'native external model gateway owner'
        Paths = @(
            'Runtime/Hosted/Models/Native-External-Model-Gateway-Supervisor.mjs',
            'Runtime/Native/X64-External-Model-Gateway-Host.wva',
            'Runtime/Native/Windows-X64-External-Model-Gateway.wva',
            'Runtime/Native/Linux-X64-External-Model-Gateway.wva',
            'Tests/Native/X64-External-Model-Gateway-Probe.wva',
            'Tools/Models/Fixtures/Native-Model-Gateway-Peer.mjs',
            'Tools/Models/Test-Native-External-Model-Gateway-Supervisor.mjs',
            'Tools/Models/Test-Native-External-Model-Gateway-Execution.mjs',
            'Tools/Native/Test-Native-External-Model-Gateway.cmd',
            'Tools/Native/Test-Native-External-Model-Gateway.sh',
            'Specifications/Native-External-Model-Gateway-Bridge.md'
        )
        Suites = @('native-external-model-gateway')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'hosted model provider owner'
        Paths = @(
            'Libraries/Platform/Models/Bound-Model-Provider.wv',
            'Projects/Libraries/Windvale-Library-Bound-Model-Provider.wvproj',
            'Projects/Tests/Windvale-Native-Test-Hosted-Model-Provider.wvproj',
            'Tests/Fixtures/Models/Native-Hosted-Model-Provider-Self-Test.wv',
            'Runtime/Native/X64-Scripted-Model-Provider-Host.wva',
            'Tools/Native/Test-Model-Provider.cmd',
            'Tools/Native/Test-Model-Provider.sh',
            'Specifications/Windvale-Bound-Model-Provider.md'
        )
        Suites = @(
            'assembler-rejections',
            'assembler-golden',
            'wva-differential',
            'model-provider'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'bounded operation core owner'
        Paths = @(
            'Libraries/Foundation/Operations/Bounded-Operation-Core.wv',
            'Projects/Libraries/Windvale-Library-Bounded-Operation-Core.wvproj',
            'Projects/Tests/Windvale-Native-Test-Bounded-Operation-Core.wvproj',
            'Tests/Fixtures/Network/Bounded-Operation-Core-Self-Test.wv',
            'Tools/Native/Test-Bounded-Operation-Core.cmd',
            'Tools/Native/Test-Bounded-Operation-Core.sh',
            'Specifications/Bounded-Operation-Core.md'
        )
        Suites = @('operation-core', 'network-connect-stream')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'standard byte output core owner'
        Paths = @(
            'Libraries/Platform/Streams/Standard-Byte-Output-Core.wv',
            'Projects/Libraries/Windvale-Library-Standard-Byte-Output-Core.wvproj',
            'Projects/Tests/Windvale-Native-Test-Standard-Byte-Output-Core.wvproj',
            'Tests/Fixtures/Streams/Standard-Byte-Output-Core-Self-Test.wv',
            'Tools/Native/Test-Standard-Byte-Output-Core.cmd',
            'Tools/Native/Test-Standard-Byte-Output-Core.sh',
            'Specifications/Standard-Byte-Output-Core.md'
        )
        Suites = @('standard-byte-output', 'file-read-application')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'network address authority owner'
        Paths = @(
            'Libraries/Network/Address-Authority.wv',
            'Projects/Libraries/Windvale-Library-Network-Address-Authority.wvproj',
            'Projects/Tests/Windvale-Native-Test-Network-Address-Authority.wvproj',
            'Tests/Fixtures/Network/Address-Authority-Self-Test.wv',
            'Tools/Native/Test-Network-Address-Authority.cmd',
            'Tools/Native/Test-Network-Address-Authority.sh',
            'Specifications/Network-Address-Authority.md'
        )
        Suites = @('network-authority', 'network-connect-stream')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'network connect stream owner'
        Paths = @(
            'Libraries/Network/Connect-Stream-Core.wv',
            'Projects/Libraries/Windvale-Library-Network-Connect-Stream-Core.wvproj',
            'Projects/Tests/Windvale-Native-Test-Network-Connect-Stream-Core.wvproj',
            'Tests/Fixtures/Network/Connect-Stream-Core-Self-Test.wv',
            'Tools/Native/Test-Network-Connect-Stream-Core.cmd',
            'Tools/Native/Test-Network-Connect-Stream-Core.sh',
            'Specifications/Network-Connect-Stream-Core.md'
        )
        Suites = @('network-connect-stream')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'database library owner'
        Paths = @('Libraries/Database/Wvdb-Reader.wv')
        Suites = @('libraries', 'packages')
        Gaps = @()
        VerifyPlan = $false
        LibraryDevelopment = $true
        LibraryTarget = 'read-only-wvdb'
    },
    @{
        Name = 'focused database tree-leaf operations owner'
        Paths = @(
            'Libraries/Database/Tree-Leaf-Scan.wv',
            'Tests/Fixtures/Database/Database-Tree-Node-Self-Test.wv',
            'Specifications/Windvale-Database-Tree-Leaf-Operations.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'tree-scan'
    },
    @{
        Name = 'focused database durable range-scan owner'
        Paths = @(
            'Libraries/Platform/Database/Durable-Tree-Scan.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Tree-Scan-Self-Test.wv',
            'Specifications/Windvale-Database-Durable-Range-Scan.md',
            'Projects/Libraries/Windvale-Library-Durable-Tree-Scan.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Scan.wvproj'
        )
        Suites = @('database-storage', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'host-tree-scan'
    },
    @{
        Name = 'focused database durable tree-delete owner'
        Paths = @(
            'Libraries/Database/Tree-Path-Delete.wv',
            'Libraries/Platform/Database/Durable-Tree-Path.wv',
            'Libraries/Platform/Database/Durable-Tree-Delete.wv',
            'Tests/Fixtures/Database/Database-Tree-Path-Delete-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Tree-Delete-Self-Test.wv',
            'Specifications/Windvale-Database-Tree-Path-Delete.md',
            'Specifications/Windvale-Database-Hosted-Tree-Delete.md',
            'Projects/Libraries/Windvale-Library-Database-Tree-Path-Delete.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Tree-Path.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Tree-Delete.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Delete.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Delete.wvproj'
        )
        Suites = @('database-storage', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'host-tree-delete'
    },
    @{
        Name = 'focused database lifecycle owner'
        Paths = @('Libraries/Platform/Database/Durable-Database-Lifecycle.wv')
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'engine'
    },
    @{
        Name = 'focused database logical-record owner'
        Paths = @('Tests/Fixtures/Database/Database-Logical-Record-Self-Test.wv')
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'logical-record'
    },
    @{
        Name = 'focused database typed-row owner'
        Paths = @(
            'Libraries/Database/Schema-Definition.wv',
            'Libraries/Database/Typed-Row.wv',
            'Tests/Fixtures/Database/Database-Typed-Row-Self-Test.wv',
            'Specifications/Windvale-Database-Typed-Rows-And-Schemas.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'typed-query-sql'
    },
    @{
        Name = 'focused database query-ir owner'
        Paths = @(
            'Libraries/Database/Query-Ir.wv',
            'Tests/Fixtures/Database/Database-Query-Ir-Self-Test.wv',
            'Specifications/Windvale-Database-Query-Ir.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'query-sql'
    },
    @{
        Name = 'focused database transaction-mutations owner'
        Paths = @(
            'Libraries/Database/Transaction-Mutations.wv',
            'Tests/Fixtures/Database/Database-Transaction-Mutations-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Mutations.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction'
    },
    @{
        Name = 'focused database transaction-leaf-rewrite owner'
        Paths = @(
            'Libraries/Database/Transaction-Leaf-Rewrite.wv',
            'Tests/Fixtures/Database/Database-Transaction-Leaf-Rewrite-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Leaf-Rewrite.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-leaf-rewrite'
    },
    @{
        Name = 'focused database transaction-paths owner'
        Paths = @(
            'Libraries/Database/Transaction-Paths.wv',
            'Tests/Fixtures/Database/Database-Transaction-Paths-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Paths.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-paths'
    },
    @{
        Name = 'focused database transaction-leaf-groups owner'
        Paths = @(
            'Libraries/Database/Transaction-Leaf-Groups.wv',
            'Tests/Fixtures/Database/Database-Transaction-Leaf-Groups-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Leaf-Groups.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-leaf-groups'
    },
    @{
        Name = 'focused database transaction-leaf-partition owner'
        Paths = @(
            'Libraries/Database/Transaction-Leaf-Partition.wv',
            'Tests/Fixtures/Database/Database-Transaction-Leaf-Partition-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Leaf-Partition.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-leaf-partition'
    },
    @{
        Name = 'focused database transaction-leaf-pages owner'
        Paths = @(
            'Libraries/Database/Transaction-Leaf-Pages.wv',
            'Tests/Fixtures/Database/Database-Transaction-Leaf-Pages-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Leaf-Pages.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-leaf-pages'
    },
    @{
        Name = 'focused database transaction-branch-partition owner'
        Paths = @(
            'Libraries/Database/Transaction-Child-Replacements.wv',
            'Libraries/Database/Transaction-Branch-Partition.wv',
            'Tests/Fixtures/Database/Database-Transaction-Branch-Partition-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Child-Replacements.md',
            'Specifications/Windvale-Database-Transaction-Branch-Partition.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-branch-partition'
    },
    @{
        Name = 'focused database transaction-parent-groups owner'
        Paths = @(
            'Libraries/Database/Transaction-Parent-Groups.wv',
            'Tests/Fixtures/Database/Database-Transaction-Parent-Groups-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Parent-Groups.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-parent-groups'
    },
    @{
        Name = 'focused database transaction-branch-pages owner'
        Paths = @(
            'Libraries/Database/Transaction-Branch-Pages.wv',
            'Projects/Libraries/Windvale-Library-Database-Transaction-Branch-Pages.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages-Validation.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages-Depth-Three.wvproj',
            'Tests/Fixtures/Database/Database-Transaction-Branch-Pages-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Transaction-Branch-Pages-Validation-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Transaction-Branch-Pages-Depth-Three-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Branch-Pages.md'
        )
        Suites = @('database-storage', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-branch-pages'
    },
    @{
        Name = 'focused database transaction-ancestor-groups owner'
        Paths = @(
            'Libraries/Database/Transaction-Ancestor-Groups.wv',
            'Projects/Libraries/Windvale-Library-Database-Transaction-Ancestor-Groups.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups-Depth-Four.wvproj',
            'Tests/Fixtures/Database/Database-Transaction-Ancestor-Groups-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Transaction-Ancestor-Groups-Depth-Four-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Ancestor-Groups.md'
        )
        Suites = @('database-storage', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-ancestor-groups'
    },
    @{
        Name = 'focused database transaction-ancestor-pages owner'
        Paths = @(
            'Libraries/Database/Transaction-Ancestor-Pages.wv',
            'Projects/Libraries/Windvale-Library-Database-Transaction-Ancestor-Pages.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages-Intermediate.wvproj',
            'Tests/Fixtures/Database/Database-Transaction-Ancestor-Pages-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Transaction-Ancestor-Pages-Intermediate-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Ancestor-Pages.md'
        )
        Suites = @('database-storage', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-ancestor-pages'
    },
    @{
        Name = 'focused database transaction-root-growth owner'
        Paths = @(
            'Libraries/Database/Transaction-Root-Growth.wv',
            'Projects/Libraries/Windvale-Library-Database-Transaction-Root-Growth.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth-Multi-Level.wvproj',
            'Tests/Fixtures/Database/Database-Transaction-Root-Growth-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Transaction-Root-Growth-Multi-Level-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Root-Growth.md'
        )
        Suites = @('database-storage', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-root-growth'
    },
    @{
        Name = 'focused database transaction-tree-completion owner'
        Paths = @(
            'Libraries/Database/Transaction-Tree-Completion.wv',
            'Projects/Libraries/Windvale-Library-Database-Transaction-Tree-Completion.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Tree-Completion.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Tree-Completion-Root-Growth.wvproj',
            'Tests/Fixtures/Database/Database-Transaction-Tree-Completion-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Transaction-Tree-Completion-Root-Growth-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Tree-Completion.md'
        )
        Suites = @('database-storage', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-tree-completion'
    },
    @{
        Name = 'focused database transaction-commit owner'
        Paths = @(
            'Libraries/Database/Commit-Batch.wv',
            'Libraries/Database/Transaction-Commit.wv',
            'Projects/Libraries/Windvale-Library-Database-Transaction-Commit.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Commit-Batch-Capacity.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Commit.wvproj',
            'Tests/Fixtures/Database/Database-Commit-Batch-Capacity-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Transaction-Commit-Self-Test.wv',
            'Specifications/Windvale-Database-Transaction-Commit.md'
        )
        Suites = @('database-storage', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-commit'
    },
    @{
        Name = 'focused persistent database transaction-writer owner'
        Paths = @(
            'Libraries/Platform/Database/Durable-Transaction-Writer.wv',
            'Libraries/Platform/Database/Durable-Persistent-Transaction-Writer.wv',
            'Projects/Libraries/Windvale-Library-Durable-Transaction-Writer.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Persistent-Transaction-Writer.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Persistent-Transaction-Writer.wvproj',
            'Tests/Fixtures/Database/Native-Hosted-Persistent-Transaction-Writer-Self-Test.wv',
            'Specifications/Windvale-Database-Persistent-Transaction-Writer.md'
        )
        Suites = @('database-storage', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'persistent-transaction-writer'
    },
    @{
        Name = 'focused database sql-lowerer owner'
        Paths = @(
            'Libraries/Database/Sql-Lowerer.wv',
            'Tests/Fixtures/Database/Database-Sql-Lowerer-Self-Test.wv',
            'Specifications/Windvale-Database-Sql.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'sql-lowerer'
    },
    @{
        Name = 'focused database json-value owner'
        Paths = @(
            'Libraries/Database/Json-Value.wv',
            'Tests/Fixtures/Database/Database-Json-Value-Self-Test.wv',
            'Specifications/Windvale-Database-Json-Value.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'json'
    },
    @{
        Name = 'focused database json-protocol owner'
        Paths = @(
            'Libraries/Database/Json-Protocol.wv',
            'Tests/Fixtures/Database/Database-Json-Protocol-Self-Test.wv',
            'Specifications/Windvale-Database-Json-Protocol.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'json-protocol'
    },
    @{
        Name = 'native lowerer rejection specification owner'
        Paths = @('Specifications/Windvale-Native-Wvb-To-Wvo-Rejection-Tests.md')
        Suites = @('lowerer-rejections')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'durable database superblock owner'
        Paths = @('Libraries/Database/Durable-Superblock.wv')
        Suites = @('database-superblock', 'database-durable-commit', 'libraries')
        Gaps = @()
        VerifyPlan = $false
        LibraryDevelopment = $true
        LibraryTarget = 'durability'
    },
    @{
        Name = 'durable database commit owner'
        Paths = @(
            'Libraries/Database/Durable-Page.wv',
            'Libraries/Database/Durable-Commit-Record.wv',
            'Libraries/Database/Commit-Publication.wv',
            'Tests/Fixtures/Database/Database-Durable-Commit-Self-Test.wv'
        )
        Suites = @('database-durable-commit', 'database-storage', 'libraries')
        Gaps = @()
        VerifyPlan = $false
        LibraryDevelopment = $true
        LibraryTarget = 'durability'
    },
    @{
        Name = 'durable database commit project owner'
        Paths = @(
            'Projects/Libraries/Windvale-Library-Database-Durable-Page.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Durable-Commit.wvproj'
        )
        Suites = @(
            'database-durable-commit',
            'database-storage',
            'workspace-project2',
            'libraries'
        )
        Gaps = @()
        VerifyPlan = $false
        LibraryDevelopment = $true
        LibraryTarget = 'durability'
    },
    @{
        Name = 'database storage publication and recovery owner'
        Paths = @(
            'Libraries/Database/Storage-Publication.wv',
            'Libraries/Database/Storage-Recovery.wv',
            'Libraries/Database/Single-Writer-Commit.wv',
            'Libraries/Database/Single-Leaf-Upsert.wv',
            'Libraries/Database/Commit-Batch.wv',
            'Libraries/Database/Root-Split-Upsert.wv',
            'Libraries/Database/Depth-Two-Upsert.wv',
            'Libraries/Database/Tree-Path-Upsert.wv',
            'Libraries/Database/Tree-Node.wv',
            'Libraries/Database/Tree-Leaf-Scan.wv',
            'Libraries/Database/Logical-Record.wv',
            'Libraries/Database/Logical-Record-Write.wv',
            'Libraries/Database/Schema-Definition.wv',
            'Libraries/Database/Typed-Row.wv',
            'Libraries/Database/Query-Ir.wv',
            'Libraries/Database/Sql-Lowerer.wv',
            'Libraries/Database/Json-Value.wv',
            'Libraries/Database/Json-Protocol.wv',
            'Libraries/Database/Local-Database-Contracts.wv',
            'Libraries/Database/Local-Database-Session.wv',
            'Libraries/Database/Local-Database-Put.wv',
            'Libraries/Database/Local-Database-Get.wv',
            'Libraries/Database/Local-Database-Control.wv',
            'Libraries/Database/Collection-Catalog.wv',
            'Libraries/Database/Database-Bootstrap.wv',
            'Libraries/Platform/Database/Durable-Storage-Executor.wv',
            'Libraries/Platform/Database/Durable-Database-Bootstrap.wv',
            'Libraries/Platform/Database/Durable-Database-Engine.wv',
            'Libraries/Platform/Database/Durable-Database-Lifecycle.wv',
            'Libraries/Platform/Database/Durable-Tree-Reader.wv',
            'Libraries/Platform/Database/Durable-Tree-Scan.wv',
            'Libraries/Platform/Database/Durable-Root-Writer.wv',
            'Libraries/Platform/Database/Durable-Root-Split-Writer.wv',
            'Libraries/Platform/Database/Durable-Local-Open.wv',
            'Libraries/Platform/Database/Durable-Local-Root-Put.wv',
            'Libraries/Platform/Database/Durable-Local-Get.wv',
            'Libraries/Platform/Database/Durable-Tree-Writer.wv',
            'Libraries/Platform/Database/Durable-Logical-Tree-Writer.wv',
            'Tests/Fixtures/Database/Database-Storage-Publication-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Storage-Recovery-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Single-Writer-Commit-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Tree-Node-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Logical-Record-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Typed-Row-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Query-Ir-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Sql-Lowerer-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Json-Value-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Json-Protocol-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Collection-Catalog-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Bootstrap-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Root-Split-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Depth-Two-Upsert-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Tree-Path-Upsert-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Storage-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Database-Engine-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Tree-Reader-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Tree-Scan-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Root-Writer-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Root-Fill-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Root-Split-Writer-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Local-Put-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Local-Get-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Tree-Writer-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Logical-Tree-Writer-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Logical-Tree-Get-Self-Test.wv',
            'Specifications/Windvale-Database-Tree-Reading-And-Root-Split.md',
            'Specifications/Windvale-Database-Tree-Leaf-Operations.md',
            'Specifications/Windvale-Database-Logical-Records.md',
            'Specifications/Windvale-Database-Typed-Rows-And-Schemas.md',
            'Specifications/Windvale-Database-Query-Ir.md',
            'Specifications/Windvale-Database-Sql.md',
            'Specifications/Windvale-Database-Json-Value.md',
            'Specifications/Windvale-Database-Json-Protocol.md',
            'Specifications/Windvale-Database-Collection-Catalog.md',
            'Specifications/Windvale-Database-Bootstrap.md',
            'Specifications/Windvale-Database-Tree-Path-Upsert.md',
            'Specifications/Windvale-Database-Durable-Range-Scan.md',
            'Specifications/Windvale-Database-Engine-Lifecycle.md',
            'Specifications/Windvale-Database-Hosted-Root-Writer.md',
            'Specifications/Windvale-Database-Hosted-Root-Split-Writer.md',
            'Specifications/Windvale-Database-Local-Service.md',
            'Specifications/Windvale-Database-Hosted-Local-Service.md',
            'Specifications/Windvale-Database-Hosted-Tree-Writer.md',
            'Runtime/Native/X64-Random-Access-Storage-Host.wva',
            'Runtime/Native/Windows-X64-Random-Access-Storage.wva',
            'Runtime/Native/Linux-X64-Random-Access-Storage.wva',
            'Tools/Database/Measure-Database-Comparison.ps1',
            'Tools/Database/SQLite-Durable-Cycle.py'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'database storage project owner'
        Paths = @(
            'Projects/Libraries/Windvale-Library-Database-Storage-Publication.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Storage-Recovery.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Single-Writer-Commit.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Single-Leaf-Upsert.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Commit-Batch.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Root-Split-Upsert.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Depth-Two-Upsert.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Tree-Path-Upsert.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Tree-Node.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Tree-Leaf-Scan.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Logical-Record.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Schema-Definition.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Typed-Row.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Transaction-Mutations.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Transaction-Leaf-Rewrite.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Transaction-Paths.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Query-Ir.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Sql-Lowerer.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Json-Value.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Json-Protocol.wvproj',
            'Projects/Libraries/Windvale-Library-Local-Database-Contracts.wvproj',
            'Projects/Libraries/Windvale-Library-Local-Database-Session.wvproj',
            'Projects/Libraries/Windvale-Library-Local-Database-Put.wvproj',
            'Projects/Libraries/Windvale-Library-Local-Database-Get.wvproj',
            'Projects/Libraries/Windvale-Library-Local-Database-Control.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Collection-Catalog.wvproj',
            'Projects/Libraries/Windvale-Library-Database-Bootstrap.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Storage-Executor.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Database-Bootstrap.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Database-Engine.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Database-Lifecycle.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Tree-Reader.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Tree-Scan.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Root-Writer.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Root-Split-Writer.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Local-Open.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Local-Root-Put.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Local-Get.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Tree-Writer.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Logical-Tree-Writer.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Storage-Publication.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Storage-Recovery.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Single-Writer-Commit.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Tree-Node.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Logical-Record.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Typed-Row.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Mutations.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Rewrite.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Paths.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Query-Ir.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Sql-Lowerer.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Json-Value.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Json-Protocol.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Collection-Catalog.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Bootstrap.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Root-Split.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Upsert.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Storage.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Engine.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Reader.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Scan.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Writer.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Fill.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Split-Writer.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Local-Put.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Local-Get.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Writer.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Writer.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj'
        )
        Suites = @('database-storage', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'native tool checkpoint owner'
        Paths = @(
            'Specifications/Windvale-Native-Tool-Checkpoint.md',
            'Tools/Native/Build-Cached-Hosted-Application-Session.mjs',
            'Tools/Native/Build-Cached-Hosted-Application.cmd',
            'Tools/Native/Build-Cached-Hosted-Application.sh',
            'Tools/Native/Build-Cached-Os-X64-Project-Wvbs.mjs',
            'Tools/Native/Build-Cached-Linked-Image-Set.mjs',
            'Tools/Native/Build-Cached-Project-Object.cmd',
            'Tools/Native/Build-Cached-Project-Object.mjs',
            'Tools/Native/Build-Cached-Project-Object.sh',
            'Tools/Native/Build-Cached-Project-Wvb.cmd',
            'Tools/Native/Build-Cached-Project-Wvb.sh',
            'Tools/Native/Build-Cached-Segmented-Project.cmd',
            'Tools/Native/Build-Cached-Segmented-Project.mjs',
            'Tools/Native/Build-Cached-Segmented-Project.sh',
            'Tools/Native/Get-Native-Hosted-Application-Cache-Key.mjs',
            'Tools/Native/Get-Native-Project-Cache-Key.mjs',
            'Tools/Native/Native-Hosted-Application-Cache-Core.mjs',
            'Tools/Native/Native-Project-Cache-Key-Core.mjs',
            'Tools/Native/Test-Hosted-Application-Session.mjs',
            'Tools/Native/Test-Linked-Image-Set-Checkpoint.mjs',
            'Tools/Native/Test-Project-Object-Checkpoint.mjs',
            'Tools/Native/Test-Segmented-Project-Checkpoint.mjs'
        )
        Suites = @('os-x64-code-emission', 'database-storage')
        Gaps = @()
        VerifyPlan = $false
        OsX64Development = $true
        OsX64Target = 'all'
        DatabaseDevelopment = $true
    },
    @{
        Name = 'nested record database owner'
        Paths = @(
            'Tests/Fixtures/Native-X64/Nested-Record-Fields.wv',
            'Projects/Tests/Windvale-Native-Test-Nested-Record-Fields.wvproj'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'nested record language contract owner'
        Paths = @(
            'Specifications/Seed-Language.md',
            'Specifications/Seed-Records.md',
            'Specifications/Compiler-Source-Symbols.md',
            'Specifications/Compiler-Source-Wir.md'
        )
        Suites = @(
            'seed',
            'unsafe-wvb',
            'source-containment',
            'language-1-front-door',
            'lowerer-rejections',
            'console-packager-source-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'package application owner'
        Paths = @(
            'Applications/Database/Wvdb-Query.wv',
            'Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock',
            'Projects/Applications/Windvale-Wvdb-Query.wvproj',
            'Specifications/Windvale-Package.md'
        )
        Suites = @(
            'workspace-project2',
            'packages',
            'package-format',
            'package-bundle',
            'wvdb-query-capability',
            'offline-package-stage',
            'wvdb-approval'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'portable package-format owner'
        Paths = @(
            'Libraries/Package/Package-Consistency.wv',
            'Libraries/Package/Installation-Generation.wv',
            'Libraries/Package/Package-Manifest.wv',
            'Libraries/Package/Package-Lock.wv',
            'Libraries/Package/Package-Resource-Admission.wv',
            'Specifications/Windvale-Installation-Generation.md',
            'Tests/Fixtures/Package/Package-Consistency-Self-Test.wv',
            'Tests/Fixtures/Package/Installation-Generation-Self-Test.wv',
            'Tests/Fixtures/Package/Package-Manifest-Self-Test.wv',
            'Tests/Fixtures/Package/Package-Lock-Self-Test.wv',
            'Tests/Fixtures/Package/Package-Resource-Admission-Self-Test.wv',
            'Projects/Libraries/Windvale-Library-Package-Consistency.wvproj',
            'Projects/Libraries/Windvale-Library-Installation-Generation.wvproj',
            'Projects/Libraries/Windvale-Library-Package-Lock.wvproj',
            'Projects/Libraries/Windvale-Library-Package-Resource-Admission.wvproj',
            'Projects/Tools/Windvale-Installation-Command-Resolver.wvproj',
            'Projects/Tools/Windvale-Installation-Activation-Planner.wvproj',
            'Projects/Tools/Windvale-Installation-Generation-Verifier.wvproj',
            'Projects/Tests/Windvale-Native-Test-Package-Consistency.wvproj',
            'Projects/Tests/Windvale-Native-Test-Installation-Generation.wvproj',
            'Projects/Tests/Windvale-Native-Test-Package-Manifest.wvproj',
            'Projects/Tests/Windvale-Native-Test-Package-Lock.wvproj',
            'Projects/Tests/Windvale-Native-Test-Package-Resource-Admission.wvproj'
        )
        Suites = @(
            'echo-command-launch',
            'package-format',
            'installation-activation',
            'offline-generation-lifecycle',
            'installation-command-resolution',
            'installation-command-dispatch',
            'installation-generation-publication',
            'package-bundle',
            'offline-package-stage'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'WVB inspector package owner'
        Paths = @(
            'Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvpack',
            'Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvlock',
            'Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvprov',
            'Tools/Native/Build-Wvb-Inspector-Package.cmd',
            'Tools/Native/Build-Wvb-Inspector-Package.sh'
        )
        Suites = @('packages', 'package-format', 'offline-package-stage')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Bundle 1 and immutable store owner'
        Paths = @(
            'Libraries/Package/Package-Bundle-Writer.wv',
            'Libraries/Package/Package-Bundle-Verifier.wv',
            'Tests/Fixtures/Package/Package-Bundle-Self-Test.wv',
            'Projects/Tests/Windvale-Native-Test-Package-Bundle.wvproj',
            'Projects/Tools/Windvale-Package-Bundle-Writer.wvproj',
            'Projects/Tools/Windvale-Package-Bundle-Verifier.wvproj',
            'Tools/Windvale.Package/Package-Bundle-Writer-Tool.wv',
            'Tools/Windvale.Package/Package-Bundle-Verifier-Tool.wv',
            'Tools/Package/Publish-Admitted-Bundle.ps1',
            'Specifications/Windvale-Package-Bundle.md'
        )
        Suites = @('package-format', 'package-bundle', 'offline-package-stage')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'installation activation host owner'
        Paths = @(
            'Tools/Package/Publish-Installation-Activation.mjs',
            'Tools/Package/Verify-Installation-Activation-Publisher.mjs',
            'Tools/Native/Test-Installation-Activation.cmd',
            'Tools/Native/Test-Installation-Activation.sh'
        )
        Suites = @('installation-activation', 'offline-generation-lifecycle')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'installation generation publication owner'
        Paths = @(
            'Tools/Package/Publish-Installation-Generation.mjs',
            'Tools/Package/Verify-Installation-Generation-Publisher.mjs',
            'Tools/Native/Test-Installation-Generation-Publication.cmd',
            'Tools/Native/Test-Installation-Generation-Publication.sh',
            'Tools/Windvale.Package/Installation-Generation-Verifier-Tool.wv',
            'Projects/Tools/Windvale-Installation-Generation-Verifier.wvproj'
        )
        Suites = @(
            'package-format',
            'offline-generation-lifecycle',
            'installation-generation-publication',
            'offline-package-stage'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'installation command resolution owner'
        Paths = @(
            'Tools/Windvale.Package/Installation-Command-Resolver-Tool.wv',
            'Projects/Tools/Windvale-Installation-Command-Resolver.wvproj',
            'Tools/Package/Verify-Installation-Command-Resolver.mjs',
            'Tools/Native/Test-Installation-Command-Resolution.cmd',
            'Tools/Native/Test-Installation-Command-Resolution.sh'
        )
        Suites = @(
            'package-format',
            'offline-generation-lifecycle',
            'installation-command-resolution'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'offline generation lifecycle owner'
        Paths = @(
            'Tools/Windvale.Package/Installation-Activation-Planner-Tool.wv',
            'Projects/Tools/Windvale-Installation-Activation-Planner.wvproj',
            'Tools/Package/Verify-Installation-Activation-Planner.mjs',
            'Tools/Package/Verify-Offline-Generation-Lifecycle.mjs',
            'Tools/Native/Test-Offline-Generation-Lifecycle.cmd',
            'Tools/Native/Test-Offline-Generation-Lifecycle.sh'
        )
        Suites = @('package-format', 'offline-generation-lifecycle')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'offline package uninstall owner'
        Paths = @(
            'Tools/Package/Uninstall-Offline-Package-State.mjs',
            'Tools/Package/Verify-Offline-Package-Uninstall.mjs',
            'Tools/Native/Test-Offline-Package-Uninstall.cmd',
            'Tools/Native/Test-Offline-Package-Uninstall.sh'
        )
        Suites = @('offline-generation-lifecycle', 'offline-package-uninstall')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'installation command dispatch owner'
        Paths = @(
            'Tools/Package/Dispatch-Installation-Command.mjs',
            'Tools/Package/Verify-Installation-Command-Dispatcher.mjs',
            'Tools/Native/Test-Installation-Command-Dispatch.cmd',
            'Tools/Native/Test-Installation-Command-Dispatch.sh'
        )
        Suites = @('echo-command-launch', 'installation-command-dispatch')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'installer owner'
        Paths = @(
            'Distribution/Installers/Windvale-Development-Installer.json',
            'Distribution/Installers/Windvale-Release-Installer.json',
            'Distribution/Installers/Templates/windows-x64/Install-Windvale.ps1',
            'Distribution/Installers/Templates/windows-x64/Uninstall-Windvale.ps1',
            'Distribution/Installers/Templates/windows-x64/wv.cmd',
            'Distribution/Installers/Templates/windows-x64/wv-verify-installation.ps1',
            'Distribution/Installers/Templates/linux-x64/install.sh',
            'Distribution/Installers/Templates/linux-x64/uninstall.sh',
            'Distribution/Installers/Templates/linux-x64/wv',
            'Distribution/Installers/Templates/linux-x64/wv-verify-installation',
            'Tools/Release/Build-Installers.mjs',
            'Tools/Native/Test-Installers.cmd',
            'Tools/Native/Test-Installers.sh',
            'Specifications/Windvale-Installer.md',
            'LICENSE.md'
        )
        Suites = @(
            'scripting',
            'installers',
            'installer-repository',
            'offline-package-stage'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'installer repository owner'
        Paths = @(
            'Tools/Release/Build-Installer-Repository.mjs',
            'Tools/Release/Verify-Installer-Repository.mjs',
            'Tools/Native/Test-Installer-Repository.cmd',
            'Tools/Native/Test-Installer-Repository.mjs',
            'Tools/Native/Test-Installer-Repository.sh',
            'Specifications/Windvale-Installer-Repository.md'
        )
        Suites = @('installer-repository')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'deterministic installer compression owners'
        Paths = @('Tools/Release/Deterministic-Compression.mjs')
        Suites = @('installers', 'installer-repository')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'release envelope owner'
        Paths = @(
            'Distribution/Releases/Windvale-Root-Policy-1.json',
            'Tools/Release/Create-Release-Envelope.mjs',
            'Tools/Release/Verify-Release-Envelope.mjs',
            'Tools/Native/Create-Release-Envelope-Fixture.mjs',
            'Tools/Native/Test-Release-Envelope.cmd',
            'Tools/Native/Test-Release-Envelope.sh',
            'Specifications/Windvale-Release-Envelope.md'
        )
        Suites = @('installer-repository', 'release-envelope', 'offline-package-stage')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'offline package stage owner'
        Paths = @(
            'Tools/Package/Create-Offline-Package-Stage-Input.mjs',
            'Tools/Native/Test-Offline-Package-Stage.cmd',
            'Tools/Native/Test-Offline-Package-Stage.sh'
        )
        Suites = @('offline-package-stage')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'application approval and launch owner'
        Paths = @(
            'Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvapproval',
            'Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.windows-x64.wvlaunch',
            'Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.linux-x64.wvlaunch',
            'Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvapproval',
            'Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.windows-x64.wvlaunch',
            'Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.linux-x64.wvlaunch',
            'Tools/Release/Verify-Wvdb-Approval-Records.mjs',
            'Tools/Native/Test-Wvdb-Approval-Records.cmd',
            'Tools/Native/Test-Wvdb-Approval-Records.sh',
            'Specifications/Windvale-Capability-Approval-And-Launch.md'
        )
        Suites = @(
            'echo-command-launch',
            'packages',
            'package-format',
            'installation-command-dispatch',
            'offline-package-stage',
            'wvdb-approval'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'library project owner'
        Paths = @('Projects/Libraries/Windvale-Database-Reader.wvproj')
        Suites = @('workspace-project2')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'workspace Project 2 owner'
        Paths = @(
            'Windvale.wvws',
            'Tests/Fixtures/Project/Workspace-Project2-Build.wvproj'
        )
        Suites = @('workspace-project2')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 Project 3 routing'
        Paths = @(
            'Specifications/Windvale-Project.md',
            'Tests/Fixtures/Project/Language-1.0-Project3-Build.wvproj'
        )
        Suites = @('language-1-front-door', 'workspace-project2')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 candidate specification routing'
        Paths = @(
            'Specifications/Windvale-Language-1.0.md',
            'Specifications/Windvale-Language-1.0-Grammar.md',
            'Specifications/Windvale-Language-1.0.ebnf',
            'Specifications/Windvale-Language-1.0-Localized-Source.md',
            'Specifications/Windvale-Language-1.0-Source-Profile-Formats.md',
            'Specifications/Windvale-Language-1.0-Foundation.md',
            'Specifications/Windvale-Language-1.0-Foundation-Registry.md',
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0815-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Source-Freeze-Candidate.txt'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'Language 1.0 paper source routing'
        Paths = @(
            'Documents/Project/Language-1.0-Paper-Corpus/11-Local-AI-Accelerator-Inference/Source/Inference-Application.wv',
            'Documents/Project/Language-1.0-Localization-Workloads/01-Source-Profile-Admission/Source/Test-Unicode-Admission.wv'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
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
        Name = 'repository text attributes'
        Paths = @('.gitattributes')
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'native development dependency closure'
        Paths = @(
            'Tests/Native/Development-Owner-Dependencies.txt',
            'Tools/Verify/Verify-Native-Development-Dependencies.ps1'
        )
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'removed Stage 0 live recovery owner'
        Paths = @(
            'Documents/Project/Stage0-Recovery-Dependencies.json',
            'Tools/Recovery/New-Stage0-Recovery-Archive.ps1',
            'Tools/Recovery/Test-Stage0-Recovery-Archive.ps1',
            'Tools/Verify/Verify-Stage0-Recovery-Archive.ps1'
        )
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'Stage 0 archival status specifications'
        Paths = @(
            'Specifications/Seed-Conformance.md',
            'Specifications/Windvale-Directory-Service-Ipc.md',
            'Specifications/Windvale-Directory-Snapshot.md'
        )
        Suites = @('os-services')
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
        Name = 'OS x64 code-emission development target manifest'
        Paths = @('Tests/Native/Os-X64-Code-Emission-Development-Targets.txt')
        Suites = @('os-x64-code-emission')
        Gaps = @()
        VerifyPlan = $true
        OsX64Development = $true
        OsX64Target = 'all'
    },
    @{
        Name = 'OS x64 code-emission exact development target'
        Paths = @(
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Reentry-Emission.wv'
        )
        Suites = @('os-x64-code-emission')
        Gaps = @()
        VerifyPlan = $false
        OsX64Development = $true
        OsX64Target = 'process-client-generation-two-reentry'
    },
    @{
        Name = 'OS x64 code-emission same-target closure'
        Paths = @(
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Reentry-Emission.wv',
            'Tests/Fixtures/Operating-System/Os-X64-Process-Client-Generation-Two-Reentry-Emission-Self-Test.wv',
            'Projects/Tests/Windvale-Native-Test-Os-X64-Process-Client-Generation-Two-Reentry-Emission.wvproj'
        )
        Suites = @('os-x64-code-emission')
        Gaps = @()
        VerifyPlan = $false
        OsX64Development = $true
        OsX64Target = 'process-client-generation-two-reentry'
    },
    @{
        Name = 'OS x64 code-emission multiple development targets'
        Paths = @(
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Reentry-Emission.wv',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Paging-Emission.wv'
        )
        Suites = @('os-x64-code-emission')
        Gaps = @()
        VerifyPlan = $false
        OsX64Development = $true
        OsX64Target = 'all'
    },
    @{
        Name = 'OS x64 code-emission owner change stays complete'
        Paths = @(
            'Tools/Native/Test-Os-X64-Code-Emission.cmd',
            'Operating-System/Kernel/X64-Process-Client-Generation-Two-Reentry-Emission.wv'
        )
        Suites = @('os-x64-code-emission')
        Gaps = @()
        VerifyPlan = $false
        OsX64Development = $true
        OsX64Target = 'all'
    },
    @{
        Name = 'library development target manifest'
        Paths = @('Tests/Native/Library-Development-Targets.txt')
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'library exact development target'
        Paths = @('Libraries/Models/Scripted-Model-Provider.wv')
        Suites = @('model-provider', 'workspace-project2', 'libraries')
        Gaps = @()
        VerifyPlan = $false
        LibraryDevelopment = $true
        LibraryTarget = 'models'
    },
    @{
        Name = 'library same-target closure'
        Paths = @(
            'Libraries/Models/Model-Protocol.wv',
            'Libraries/Models/Scripted-Model-Provider.wv',
            'Projects/Tests/Windvale-Native-Test-Model-Protocol.wvproj'
        )
        Suites = @('model-provider', 'workspace-project2', 'libraries')
        Gaps = @()
        VerifyPlan = $false
        LibraryDevelopment = $true
        LibraryTarget = 'models'
    },
    @{
        Name = 'library multiple development targets'
        Paths = @(
            'Libraries/Database/Storage-Geometry.wv',
            'Libraries/Models/Model-Protocol.wv'
        )
        Suites = @('model-provider', 'workspace-project2', 'libraries')
        Gaps = @()
        VerifyPlan = $false
        LibraryDevelopment = $false
        LibraryTarget = 'all'
    },
    @{
        Name = 'library owner change stays complete'
        Paths = @(
            'Tools/Native/Test-Libraries.cmd',
            'Libraries/Models/Scripted-Model-Provider.wv'
        )
        Suites = @('model-provider', 'workspace-project2', 'libraries')
        Gaps = @()
        VerifyPlan = $false
        LibraryDevelopment = $false
        LibraryTarget = 'all'
    },
    @{
        Name = 'combined deterministic order'
        Paths = @('Compiler/Windvale/Source-Wvb-Compiler.wv', 'Assembler/Windvale/Wva-Assembler-Core.wv')
        Suites = @(
            'seed',
            'seed-native-front-door',
            'unsafe-wvb',
            'assembler-rejections',
            'assembler-golden',
            'wva-differential',
            'source-containment',
            'lowerer-rejections',
            'console-packager-source-reconstruction'
        )
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

Write-Host 'START verification plan phase=contracts item=1/3'
& $RetirementInventoryVerifier -Quiet
& $DevelopmentDependencyVerifier -Quiet

$VerificationOwnerPlan = Join-Path $RepositoryRoot 'Tests/Native/Verification-Owners.txt'
$VerificationOwnerLines = @(Get-Content -LiteralPath $VerificationOwnerPlan)
if ($VerificationOwnerLines.Count -ne 109 -or
    $VerificationOwnerLines[0] -ne 'windvale-native-verification-owners 1') {
    throw 'The native verification-owner header or exact 108-owner inventory differs.'
}
$VerificationOwnerCases = 0
$VerificationOwnerShards = [System.Collections.Generic.HashSet[int]]::new()
foreach ($Line in $VerificationOwnerLines | Select-Object -Skip 1) {
    $Fields = $Line -split '\|', 5
    if ($Fields.Count -ne 5) {
        throw "Malformed native verification-owner entry: $Line"
    }
    $VerificationOwnerEntryCases = 0
    $VerificationOwnerEntryShard = 0
    if (![int]::TryParse($Fields[2], [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$VerificationOwnerEntryCases) -or
        $VerificationOwnerEntryCases -le 0) {
        throw "Invalid native verification-owner case count: $Line"
    }
    if (![int]::TryParse($Fields[3], [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$VerificationOwnerEntryShard) -or
        $VerificationOwnerEntryShard -lt 1 -or $VerificationOwnerEntryShard -gt 4) {
        throw "Invalid native qualification shard: $Line"
    }
    $VerificationOwnerCases += $VerificationOwnerEntryCases
    $null = $VerificationOwnerShards.Add($VerificationOwnerEntryShard)
    $WindowsOwner = "Tools/Native/$($Fields[1]).cmd"
    $LinuxOwner = "Tools/Native/$($Fields[1]).sh"
    foreach ($Owner in @($WindowsOwner, $LinuxOwner)) {
        if (!(Test-Path -LiteralPath (Join-Path $RepositoryRoot $Owner) -PathType Leaf)) {
            throw "The native verification plan is missing owner '$Owner'."
        }
    }
    $LinuxIndex = @(git -C $RepositoryRoot ls-files -s -- $LinuxOwner)
    if ($LASTEXITCODE -ne 0 -or $LinuxIndex.Count -ne 1 -or
        $LinuxIndex[0] -notmatch '^100755 ') {
        throw "Linux verification owner '$LinuxOwner' is not executable in Git."
    }
}
if ($VerificationOwnerCases -ne 5155 -or $VerificationOwnerShards.Count -ne 4) {
    throw 'The native verification-owner case total or four-shard coverage differs.'
}

$CompilerDevelopmentWindows = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Compiler-Reconstruction.cmd')
$CompilerDevelopmentLinux = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Compiler-Reconstruction.sh')
$ChangedVerification = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Verify/Verify-Changed.ps1')
foreach ($Contract in @(
    @{
        Name = 'Windows compiler development owner'
        Text = $CompilerDevelopmentWindows
        Required = @(
            'Usage: Tools\Native\Test-Compiler-Reconstruction.cmd [--development]',
            'if "%Development%"=="1" goto :development',
            'Function-Only.wv',
            'Build-Current-Wvb.cmd',
            'Verify-Wvb.cmd',
            'current candidate compiler and build-driver smoke',
            'native paired reconstruction'
        )
    },
    @{
        Name = 'Linux compiler development owner'
        Text = $CompilerDevelopmentLinux
        Required = @(
            'Test-Compiler-Reconstruction.sh [--development]',
            'if $development; then',
            'Function-Only.wv',
            'Build-Current-Wvb.sh',
            'Verify-Wvb.sh',
            'current candidate compiler and build-driver smoke',
            'native paired reconstruction'
        )
    },
    @{
        Name = 'changed-file compiler development dispatch'
        Text = $ChangedVerification
        Required = @(
            '$Suite -eq ''compiler-reconstruction''',
            '$Plan.Scope -eq ''development''',
            'mode=development-smoke',
            '& $DevelopmentOwner --development'
        )
    }
)) {
    foreach ($Fragment in $Contract.Required) {
        if (!$Contract.Text.Contains($Fragment, [StringComparison]::Ordinal)) {
            throw "$($Contract.Name) is missing '$Fragment'."
        }
    }
}

$SourceContainmentWindows = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Source-Containment.cmd')
$SourceContainmentLinux = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Source-Containment.sh')
$SourceContainmentRunner = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Random-Containment.mjs')
$SourceContainmentImplementation = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Random-Containment-Source.mjs')
foreach ($Contract in @(
    @{
        Name = 'Windows compiler-only source containment owner'
        Text = $SourceContainmentWindows
        Required = @(
            'Test-Source-Containment.cmd [--compiler-only]',
            'Test-Random-Containment.mjs" source %Mode%'
        )
    },
    @{
        Name = 'Linux compiler-only source containment owner'
        Text = $SourceContainmentLinux
        Required = @(
            'Test-Source-Containment.sh [--compiler-only]',
            'Test-Random-Containment.mjs" source "$@"'
        )
    },
    @{
        Name = 'source containment mode parser'
        Text = $SourceContainmentRunner
        Required = @(
            '<source|wvb|wvo> [--compiler-only]',
            'process.argv.length === 4 && Compilerˉonly && Family === "source"',
            'Compilerˉonly,'
        )
    },
    @{
        Name = 'source containment compiler-only boundary'
        Text = $SourceContainmentImplementation
        Required = @(
            'Compilerˉonly = false',
            'if (Compilerˉonly)',
            'const Assemblerˉartifact = Hostˉartifact'
        )
    },
    @{
        Name = 'changed-file source containment dispatch'
        Text = $ChangedVerification
        Required = @(
            '$Suite -eq ''source-containment''',
            '$NativePlan.UseSourceContainmentCompilerDevelopment',
            'mode=compiler-only',
            '& $DevelopmentOwner --compiler-only'
        )
    }
)) {
    foreach ($Fragment in $Contract.Required) {
        if (!$Contract.Text.Contains($Fragment, [StringComparison]::Ordinal)) {
            throw "$($Contract.Name) is missing '$Fragment'."
        }
    }
}

$CompilerSourceContainmentPlan = & $NativePlanner -ChangedPath (
    'Compiler/Windvale/Source-Lexer-Core.wv') -PassThru -Quiet
if (!$CompilerSourceContainmentPlan.UseSourceContainmentCompilerDevelopment -or
    $CompilerSourceContainmentPlan.Suites -notcontains 'source-containment') {
    throw 'Compiler source containment does not select compiler-only development.'
}
$ContainmentOwnerPlan = & $NativePlanner -ChangedPath @(
    'Tools/Native/Test-Source-Containment.cmd',
    'Tools/Native/Test-Source-Containment.sh',
    'Tests/Native/Random-Containment/Corpus.tar.gz.b64'
) -PassThru -Quiet
if ($ContainmentOwnerPlan.UseSourceContainmentCompilerDevelopment -or
    $ContainmentOwnerPlan.Suites -notcontains 'source-containment' -or
    $ContainmentOwnerPlan.Gaps.Count -ne 0) {
    throw 'Source containment owner changes do not retain complete development.'
}
$DirectCompilerContainmentPlan = & $NativePlanner -ChangedPath (
    'Artifacts/WebAssembly-Playground/Windvale-Compiler-Direct.wasm') `
    -PassThru -Quiet
if (!$DirectCompilerContainmentPlan.UseSourceContainmentCompilerDevelopment -or
    $DirectCompilerContainmentPlan.Suites -notcontains 'source-containment' -or
    !$DirectCompilerContainmentPlan.RunWebAssemblyEngineVerification) {
    throw 'The direct compiler artifact does not select both development owners.'
}

$GitHubVerificationWorkflow = Get-Content -LiteralPath (
    Join-Path $RepositoryRoot '.github/workflows/verify.yml') -Raw
$RequiredWorkflowFragments = @(
    'group: verify-${{ github.workflow }}-${{ github.ref }}',
    'cancel-in-progress: true',
    'if ([string]::IsNullOrWhiteSpace($env:BASE_SHA) -or',
    'git diff --check HEAD^ HEAD --',
    'run: Tools\Native\Test-Verification-Owners.cmd --shard ${{ matrix.shard }}',
    'run: ./Tools/Native/Test-Verification-Owners.sh --shard ${{ matrix.shard }}'
)
foreach ($Fragment in $RequiredWorkflowFragments) {
    if (!$GitHubVerificationWorkflow.Contains($Fragment, [StringComparison]::Ordinal)) {
        throw "The GitHub verification workflow is missing '$Fragment'."
    }
}
if ([regex]::Matches(
        $GitHubVerificationWorkflow,
        [regex]::Escape('shard: [1, 2, 3, 4]')).Count -ne 2) {
    throw 'The GitHub verification workflow must declare four shards for both hosts.'
}

$GitAttributes = @(Get-Content -LiteralPath (Join-Path $RepositoryRoot '.gitattributes'))
foreach ($Policy in @(
    '*.wvprov text eol=lf',
    '*.wvapproval text eol=lf',
    '*.wvlaunch text eol=lf'
)) {
    if (@($GitAttributes | Where-Object { $_ -eq $Policy }).Count -ne 1) {
        throw "Windvale record policy '$Policy' must occur exactly once in .gitattributes."
    }
}

$LinuxArtifactIndex = @(git -C $RepositoryRoot ls-files -s -- 'Artifacts/**/*.elf')
if ($LASTEXITCODE -ne 0 -or $LinuxArtifactIndex.Count -eq 0) {
    throw 'The tracked Linux ELF artifact inventory could not be read.'
}
foreach ($Entry in $LinuxArtifactIndex) {
    if ($Entry -notmatch '^100755 .+\t(.+\.elf)$') {
        $Artifact = if ($Entry -match '\t(.+)$') { $Matches[1] } else { $Entry }
        throw "Linux ELF artifact '$Artifact' is not executable in Git."
    }
}

$OsX64TargetPlan = Join-Path $RepositoryRoot `
    'Tests/Native/Os-X64-Code-Emission-Development-Targets.txt'
$OsX64TargetLines = @(Get-Content -LiteralPath $OsX64TargetPlan)
if ($OsX64TargetLines.Count -ne 57 -or
    $OsX64TargetLines[0] -ne
        'windvale-os-x64-code-emission-development-targets 2') {
    throw 'The OS x64 code-emission development-target inventory differs.'
}
$OsX64TargetNames = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$OsX64TargetProjects = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$OsX64TargetArtifacts = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$OsX64ExpectedExit = 50
foreach ($Line in @($OsX64TargetLines | Select-Object -Skip 1)) {
    $Fields = $Line.Split('|')
    if ($Fields.Count -lt 16 -or $Fields.Count -gt 17 -or
        $Fields[0] -notmatch '^[a-z0-9][a-z0-9-]*$' -or
        $Fields[1] -notmatch
            '^Projects/Tests/Windvale-Native-Test-Os-X64-.+-Emission\.wvproj$' -or
        $Fields[2] -notmatch '^[A-Za-z][A-Za-z0-9]*$' -or
        $Fields[3] -ne [string]$OsX64ExpectedExit -or
        @($Fields[4], $Fields[6], $Fields[8], $Fields[10], $Fields[12] |
            Where-Object { $_ -notmatch '^[0-9]+$' }).Count -ne 0 -or
        @($Fields[5], $Fields[7], $Fields[9], $Fields[11], $Fields[13] |
            Where-Object { $_ -notmatch '^[0-9a-f]{64}$' }).Count -ne 0 -or
        !$OsX64TargetNames.Add($Fields[0]) -or
        !$OsX64TargetProjects.Add($Fields[1]) -or
        !$OsX64TargetArtifacts.Add($Fields[2])) {
        throw "Invalid or duplicate OS x64 code-emission development target: $Line"
    }
    $OsX64ExpectedExit++
    $ProjectLeaf = [IO.Path]::GetFileName($Fields[1])
    $ExpectedTarget = $ProjectLeaf.Substring(
        'Windvale-Native-Test-Os-X64-'.Length,
        $ProjectLeaf.Length -
            'Windvale-Native-Test-Os-X64-'.Length -
            '-Emission.wvproj'.Length).ToLowerInvariant()
    if ($Fields[0] -ne $ExpectedTarget) {
        throw "OS x64 code-emission target name differs from its project: $Line"
    }
    $InputPaths = @($Fields[1]) + @($Fields | Select-Object -Skip 14)
    foreach ($InputPath in $InputPaths) {
        if (!(Test-Path -LiteralPath (Join-Path $RepositoryRoot $InputPath) -PathType Leaf)) {
            throw "Missing OS x64 code-emission development input: $InputPath"
        }
    }
    $ProjectDeclarations = @(
        Get-Content -LiteralPath (Join-Path $RepositoryRoot $Fields[1]) |
            ForEach-Object {
                if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                    $Matches[1]
                }
            }
    )
    if (!([System.Linq.Enumerable]::SequenceEqual(
            [string[]]$ProjectDeclarations,
            [string[]]@($Fields | Select-Object -Skip 14)))) {
        throw "OS x64 code-emission project closure differs: $($Fields[1])"
    }
}
$OsX64WindowsOwner = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Os-X64-Code-Emission.cmd')
$OsX64LinuxOwner = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Os-X64-Code-Emission.sh')
$OsX64BatchBuilder = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Build-Cached-Os-X64-Project-Wvbs.mjs')
foreach ($Fragment in @(
    'Removeˉtemporaryˉcheckpoint',
    '} finally {',
    'await rm(candidate, { recursive: true, force: false, maxRetries: 2 });',
    "['EEXIST', 'ENOTEMPTY', 'EPERM', 'EACCES']",
    'await Validateˉcheckpoint(checkpointDirectory, key);'
)) {
    if (!$OsX64BatchBuilder.Contains($Fragment, [StringComparison]::Ordinal)) {
        throw "The OS x64 project-WVB batch is missing '$Fragment'."
    }
}
$OsX64OwnerContracts = @(
    @{
        Host = 'Windows'
        Text = $OsX64WindowsOwner
        Required = @(
            'Os-X64-Code-Emission-Development-Targets.txt',
            'windvale-os-x64-code-emission-development-targets 2',
            ':consider_case',
            ':run_case',
            ':stage_toolchain',
            '--development-all',
            'Build-Cached-Os-X64-Project-Wvbs.mjs',
            '%CaseProject%',
            '%CaseArtifact%',
            '%CaseExpectedExit%',
            'wvbuild.exe',
            'wvpublish.exe',
            'Wvb-To-Wvo.exe',
            'windows-x64-wvopublish.exe',
            'Wv-Linker.exe',
            'Console-Packager.exe',
            'windows-x64-wvappublish.exe',
            '65602cd41bd929f9d698d9a4a74f683a8525b7dc2c903a5462e8b22fe1fe34ec',
            'b9fd1b11bc1e4a726e4a43b16830a9351fe573b30e547ba8d8f6660f688ed421',
            '61a0789f80c7a44e828bfc7bede7725c9c7871b6434c6d464a90fe00347cd9e9',
            '76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910',
            'f47a952867203fbff53abb131ea155b4fe9e14a8be153cc61c0ca5fd8e4a74e0',
            '0dddbe6cfd38c37e3fd5332567b3323480a5548a6fbeb41b6b50aed0e57ac3d2',
            '0bafe84096859f4b88dc14be92c6cdc5336d791b7c5b0a332dccb76b913dd24e',
            'copy /b "%BuildDriverSource%" "%Work%\wvbuild.exe"',
            'fsutil reparsepoint query',
            'dir /a:l /s /b',
            '"%BuildDriver%" --workspace',
            '"%WvbPublisher%" "%CandidateWvb%"',
            '"%Lowerer%" "%Work%\%CaseArtifact%.wvb"',
            '"%WvoPublisher%" "%CandidateWvo%"',
            '"%Linker%" ^',
            '"%ConsolePublisher%" "%CandidateExe%"',
            '"%ConsolePublisher%" "%CandidateElf%"'
        )
        Forbidden = @(
            'Build-Wvb.cmd',
            'Lower-Wvb-To-Wvo.cmd',
            'Link-Wvo.cmd',
            'Package-Console.cmd',
            'Publish-Wvo.cmd',
            'Publish-Console.cmd'
        )
        LegacySelector = 'if defined DevelopmentTarget if /I not "%DevelopmentTarget%"=="[a-z]'
    },
    @{
        Host = 'Linux'
        Text = $OsX64LinuxOwner
        Required = @(
            'Os-X64-Code-Emission-Development-Targets.txt',
            'windvale-os-x64-code-emission-development-targets 2',
            'run_case()',
            '--development-all',
            'Build-Cached-Os-X64-Project-Wvbs.mjs',
            '$repository_root/$project',
            '$work/$artifact',
            '$expected_exit',
            'linux-x64/wvbuild.elf',
            'linux-x64/wvpublish.elf',
            'Wvb-To-Wvo.elf',
            'linux-x64-wvopublish.elf',
            'Wv-Linker.elf',
            'Console-Packager.elf',
            'linux-x64-wvappublish.elf',
            'd228db89c17cc8124776d6bd39cb061a1414168a22ca075168e44439b1253969',
            'b8efb90f7d7c4eae99de01df6c0a3c24a7396d9b9e717ff69d005282ed3d63af',
            'a58fd44c8c19da19a1699b33392996a673e291f6d9f951eb578f829c4b2b5452',
            '2889237d7fdb20b1d420c05834f19183d18b02112e3f4eea0ed7ff43414814f2',
            '8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a',
            'd399c935e906ab42d7572e337226577055396cb6204766106e21790e22ea43af',
            'e9b8771978c9fb06c3a8ecc55c7b9a3ba1acd24faa541dc669920c10ed792925',
            'cp -- "$build_driver" "$work/wvbuild.elf"',
            'sha256sum --check --strict --quiet SHA256SUMS',
            'find "$repository_root" -type l -print -quit',
            '"$build_driver" --workspace',
            '"$wvb_publisher" "$candidate_wvb"',
            '"$lowerer" "$work/$artifact.wvb"',
            '"$wvo_publisher" "$candidate_wvo"',
            '"$linker" 0 Main',
            '"$console_publisher" "$candidate_exe"',
            '"$console_publisher" "$candidate_elf"'
        )
        Forbidden = @(
            'Build-Wvb.sh',
            'Lower-Wvb-To-Wvo.sh',
            'Link-Wvo.sh',
            'Package-Console.sh',
            'Publish-Wvo.sh',
            'Publish-Console.sh'
        )
        LegacySelector = "if selected '[a-z]"
    }
)
foreach ($OwnerContract in $OsX64OwnerContracts) {
    foreach ($Fragment in $OwnerContract.Required) {
        if (!$OwnerContract.Text.Contains($Fragment, [StringComparison]::Ordinal)) {
            throw "$($OwnerContract.Host) OS x64 code-emission owner is missing '$Fragment'."
        }
    }
    foreach ($Fragment in $OwnerContract.Forbidden) {
        if ($OwnerContract.Text.Contains($Fragment, [StringComparison]::Ordinal)) {
            throw "$($OwnerContract.Host) OS x64 code-emission owner still invokes '$Fragment'."
        }
    }
    if ([regex]::IsMatch(
            $OwnerContract.Text,
            $OwnerContract.LegacySelector)) {
        throw "$($OwnerContract.Host) OS x64 code-emission owner retains repeated selectors."
    }
}

$LibraryTargetPlan = Join-Path $RepositoryRoot `
    'Tests/Native/Library-Development-Targets.txt'
$LibraryTargetLines = @(Get-Content -LiteralPath $LibraryTargetPlan)
if ($LibraryTargetLines.Count -ne 30 -or
    $LibraryTargetLines[0] -ne 'windvale-library-development-targets 1') {
    throw 'The library development-target inventory differs.'
}
$LibraryTargetNames = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$LibraryTargetProjects = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$LibraryTargetKindCounts = @{ project = 0; conformance = 0; negative = 0 }
foreach ($Line in @($LibraryTargetLines | Select-Object -Skip 1)) {
    $Fields = $Line.Split('|')
    if ($Fields.Count -ne 3 -or
        !$LibraryTargetKindCounts.ContainsKey($Fields[1]) -or
        !$LibraryTargetProjects.Add($Fields[2])) {
        throw "Invalid or duplicate library development target: $Line"
    }
    $ExpectedKind = if ($Fields[2].StartsWith(
            'Projects/Libraries/', [StringComparison]::Ordinal) -or
        $Fields[2].EndsWith('-Import-Smoke.wvproj', [StringComparison]::Ordinal)) {
        'project'
    } elseif ($Fields[2].StartsWith(
            'Projects/Tests/', [StringComparison]::Ordinal)) {
        'conformance'
    } else {
        'negative'
    }
    if ($Fields[1] -ne $ExpectedKind) {
        throw "Library development-target evidence kind differs: $Line"
    }
    $null = $LibraryTargetNames.Add($Fields[0])
    $LibraryTargetKindCounts[$Fields[1]]++
    $ProjectPath = Join-Path $RepositoryRoot $Fields[2]
    if (!(Test-Path -LiteralPath $ProjectPath -PathType Leaf)) {
        throw "Missing library development project: $($Fields[2])"
    }
    foreach ($InputPath in @(
        Get-Content -LiteralPath $ProjectPath |
            ForEach-Object {
                if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                    $Matches[1]
                }
            }
    )) {
        if (!(Test-Path -LiteralPath (Join-Path $RepositoryRoot $InputPath) -PathType Leaf)) {
            throw "Missing library development input: $InputPath"
        }
    }
}
$ExpectedLibraryTargetProjects = [string[]]@(
    'Projects/Libraries/Windvale-Library-Resource-Store.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Storage-Geometry.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Storage-Page.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Durable-Superblock.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Durable-Page.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Durable-Commit-Record.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Commit-Publication.wvproj',
    'Projects/Libraries/Windvale-Library-Wvdb-Reader.wvproj',
    'Projects/Libraries/Windvale-Library-Hosted-Resource-Store.wvproj',
    'Projects/Libraries/Windvale-Library-Read-Only-Directory.wvproj',
    'Projects/Libraries/Windvale-Library-Random-Access-Storage.wvproj',
    'Projects/Libraries/Windvale-Library-Random-Access-Database-Page.wvproj',
    'Projects/Libraries/Windvale-Library-Native-Hosted-Snapshot-Page.wvproj',
    'Projects/Libraries/Windvale-Library-Read-Only-Wvdb.wvproj',
    'Projects/Libraries/Windvale-Library-Model-Protocol.wvproj',
    'Projects/Libraries/Windvale-Library-Scripted-Model-Provider.wvproj',
    'Tests/Fixtures/Libraries/Directory-Import-Smoke.wvproj',
    'Tests/Fixtures/Libraries/Random-Access-Page-Import-Smoke.wvproj',
    'Tests/Fixtures/Libraries/Random-Access-Storage-Import-Smoke.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Geometry.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Storage-Page.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Storage-Page-Accept.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Durable-Superblock.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Durable-Commit.wvproj',
    'Projects/Tests/Windvale-Native-Test-Native-Hosted-Snapshot-Page.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Reader.wvproj',
    'Projects/Tests/Windvale-Native-Test-Model-Protocol.wvproj',
    'Tests/Fixtures/Libraries/Capability-Import-No-Root-Declaration.wvproj',
    'Tests/Fixtures/Libraries/Capability-Profile-Rejection.wvproj'
)
if (!$LibraryTargetNames.SetEquals([string[]]@(
        'capability-rejections',
        'durability',
        'models',
        'page-storage',
        'read-only-wvdb',
        'resource-store',
        'storage-geometry'
    )) -or
    !$LibraryTargetProjects.SetEquals($ExpectedLibraryTargetProjects) -or
    $LibraryTargetKindCounts.project -ne 19 -or
    $LibraryTargetKindCounts.conformance -ne 8 -or
    $LibraryTargetKindCounts.negative -ne 2) {
    throw 'The library development-target names or evidence totals differ.'
}
$LibraryWindowsOwner = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Libraries.cmd')
$LibraryLinuxOwner = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Libraries.sh')
foreach ($OwnerText in @($LibraryWindowsOwner, $LibraryLinuxOwner)) {
    if (!$OwnerText.Contains('Library-Development-Targets.txt') -or
        !$OwnerText.Contains('--development-target') -or
        !$OwnerText.Contains('native libraries development status=Passed')) {
        throw 'A library owner does not implement the development-target contract.'
    }
}
Write-Host 'PASS  verification plan phase=contracts item=1/3'

$GeneralCaseIndex = 0
Write-Host "START verification plan phase=general-routing item=0/$($Cases.Count)"
foreach ($Case in $Cases) {
    $GeneralCaseIndex += 1
    if ($GeneralCaseIndex -eq 1 -or $GeneralCaseIndex % 10 -eq 0 -or
        $GeneralCaseIndex -eq $Cases.Count) {
        Write-Host (
            "PROGRESS verification plan phase=general-routing " +
            "item=$GeneralCaseIndex/$($Cases.Count) case=$($Case.Name)")
    }
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
Write-Host "PASS  verification plan phase=general-routing item=$($Cases.Count)/$($Cases.Count)"

$NativeCaseIndex = 0
Write-Host "START verification plan phase=native-routing item=0/$($NativeCases.Count)"
foreach ($Case in $NativeCases) {
    $NativeCaseIndex += 1
    if ($NativeCaseIndex -eq 1 -or $NativeCaseIndex % 10 -eq 0 -or
        $NativeCaseIndex -eq $NativeCases.Count) {
        Write-Host (
            "PROGRESS verification plan phase=native-routing " +
            "item=$NativeCaseIndex/$($NativeCases.Count) case=$($Case.Name)")
    }
    $Plan = & $NativePlanner -ChangedPath $Case.Paths -PassThru -Quiet
    $ExpectedWebAssemblyVerification = if ($Case.ContainsKey('VerifyWebAssembly')) {
        $Case.VerifyWebAssembly
    } else {
        $false
    }
    $ExpectedWebAssemblyEngineVerification = if (
        $Case.ContainsKey('VerifyWebAssemblyEngine')) {
        $Case.VerifyWebAssemblyEngine
    } else {
        $false
    }
    $ExpectedGitHubVerification = if ($Case.ContainsKey('VerifyGitHub')) {
        $Case.VerifyGitHub
    } else {
        $false
    }
    $DatabaseDevelopmentDiffers = (
        $Case.ContainsKey('DatabaseDevelopment') -and
        $Plan.UseDatabaseStorageDevelopment -ne $Case.DatabaseDevelopment)
    $OsX64DevelopmentDiffers = (
        $Case.ContainsKey('OsX64Development') -and
        $Plan.UseOsX64CodeEmissionDevelopment -ne $Case.OsX64Development)
    $ExpectedLibraryDevelopment = if ($Case.ContainsKey('LibraryDevelopment')) {
        $Case.LibraryDevelopment
    } else {
        $false
    }
    $LibraryDevelopmentDiffers = (
        $Plan.UseLibraryDevelopment -ne $ExpectedLibraryDevelopment)
    $ExpectedOsX64Target = if ($Case.ContainsKey('OsX64Target')) {
        $Case.OsX64Target
    } else {
        'all'
    }
    $OsX64TargetDiffers = (
        $Plan.OsX64CodeEmissionDevelopmentTarget -ne $ExpectedOsX64Target)
    $ExpectedLibraryTarget = if ($Case.ContainsKey('LibraryTarget')) {
        $Case.LibraryTarget
    } else {
        'all'
    }
    $LibraryTargetDiffers = (
        $Plan.LibraryDevelopmentTarget -ne $ExpectedLibraryTarget)
    $ExpectedDatabaseTarget = if ($Case.ContainsKey('DatabaseTarget')) {
        $Case.DatabaseTarget
    } else {
        'all'
    }
    $DatabaseTargetDiffers = (
        $Plan.DatabaseStorageDevelopmentTarget -ne $ExpectedDatabaseTarget)
    if (
        !([System.Linq.Enumerable]::SequenceEqual(
            [string[]]@($Plan.Suites),
            [string[]]$Case.Suites)) -or
        !([System.Linq.Enumerable]::SequenceEqual(
            [string[]]@($Plan.Gaps),
            [string[]]$Case.Gaps)) -or
        $Plan.RunPlanVerification -ne $Case.VerifyPlan -or
        $Plan.RunWebAssemblyEngineVerification -ne
            $ExpectedWebAssemblyEngineVerification -or
        $Plan.RunWebAssemblyVerification -ne $ExpectedWebAssemblyVerification -or
        $Plan.RunGitHubQualificationVerification -ne $ExpectedGitHubVerification -or
        $OsX64DevelopmentDiffers -or
        $OsX64TargetDiffers -or
        $LibraryDevelopmentDiffers -or
        $LibraryTargetDiffers -or
        $DatabaseDevelopmentDiffers -or
        $DatabaseTargetDiffers
    ) {
        throw (
            "Native plan '$($Case.Name)' expected suites=[$($Case.Suites -join ', ')], " +
            "gaps=[$($Case.Gaps -join ', ')], verify-plan=$($Case.VerifyPlan), " +
            "verify-webassembly-engine=$ExpectedWebAssemblyEngineVerification, " +
            "verify-webassembly=$ExpectedWebAssemblyVerification, " +
            "verify-github=$ExpectedGitHubVerification; found " +
            "suites=[$($Plan.Suites -join ', ')], gaps=[$($Plan.Gaps -join ', ')], " +
            "verify-plan=$($Plan.RunPlanVerification), " +
            "verify-webassembly-engine=$($Plan.RunWebAssemblyEngineVerification), " +
            "verify-webassembly=$($Plan.RunWebAssemblyVerification), " +
            "verify-github=$($Plan.RunGitHubQualificationVerification), " +
            "os-x64-development=$($Plan.UseOsX64CodeEmissionDevelopment), " +
            "os-x64-target=$($Plan.OsX64CodeEmissionDevelopmentTarget), " +
            "library-development=$($Plan.UseLibraryDevelopment), " +
            "library-target=$($Plan.LibraryDevelopmentTarget), " +
            "database-development=$($Plan.UseDatabaseStorageDevelopment), " +
            "database-target=$($Plan.DatabaseStorageDevelopmentTarget)."
        )
    }
}
Write-Host "PASS  verification plan phase=native-routing item=$($NativeCases.Count)/$($NativeCases.Count)"

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
