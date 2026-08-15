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
    @{ Name = 'editor'; Paths = @('Tools/Editors/Windvale/package.json'); Scope = 'lightweight'; Editor = $true; Areas = @() },
    @{ Name = 'website'; Paths = @('Website/index.html'); Scope = 'website'; Editor = $false; Areas = @() },
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
    @{ Name = 'bytecode specification'; Paths = @('Specifications/Seed-Bytecode.md'); Scope = 'development'; Editor = $false; Areas = @('bytecode', 'runtime') },
    @{ Name = 'test fixture'; Paths = @('Tests/Native/Wvo-Rejections/Bad-Version.wvo.b64'); Scope = 'development'; Editor = $false; Areas = $AllAreas }
)
$NativeCases = @(
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
            'Projects/Runtime/Windvale-Native-Service-Bundle-Materialization-Core.wvproj',
            'Examples/Compiler/Native-Stencil-Demo.wv',
            'Projects/Examples/Native-Stencil-Demo.wvproj'
        )
        Suites = @(
            'seed',
            'seed-native-front-door',
            'unsafe-wvb',
            'source-containment',
            'lowerer-rejections',
            'console-packager-source-reconstruction',
            'database-storage'
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
            'seed-native-front-door',
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
            'Tools/Native/Test-Random-Containment.mjs',
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
            'Tools/Native/Construct-Compiler-Reconstruction.cmd',
            'Artifacts/Native-Compiler-Reconstruction-Candidate/Manifest.json',
            'Specifications/Windvale-Native-Compiler-Reconstruction.md'
        )
        Suites = @('compiler-reconstruction')
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
            'Specifications/Windvale-Linking.md'
        )
        Suites = @(
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction',
            'linker-rejections',
            'linker-hostile',
            'linker-map-limit'
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
            'Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj',
            'Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj',
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
            'wvo-publisher-reconstruction'
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
            'development-installers'
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
        Name = 'WVB runner reconstruction owner'
        Paths = @(
            'Tools/Native/Test-Wvb-Runner-Reconstruction.cmd',
            'Tools/Native/Construct-Wvb-Runner-Reconstruction.cmd',
            'Artifacts/Native-Wvb-Runner-Candidate/Manifest.json',
            'Projects/Tools/Windvale-Wvb-Runner.wvproj',
            'Tools/Windvale.Run/Wvb-Runner-Tool.wv',
            'Specifications/Windvale-Native-Wvb-Runner.md'
        )
        Suites = @('seed-native-front-door', 'wvb-runner-reconstruction')
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
        Suites = @('seed', 'seed-native-front-door')
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
        Name = 'normal process object'
        Paths = @('Operating-System/Tools/Process-Object-Tool.wv')
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
            'Projects/Operating-System/Windvale-Os-Resource-Domain-Policy.wvproj',
            'Projects/Tests/Windvale-Native-Test-Os-Resource-Domain.wvproj',
            'Tests/Fixtures/Operating-System/Os-Resource-Domain-Self-Test.wv',
            'Specifications/Windvale-Os-Resource-Domain-Policy.md'
        )
        Suites = @('os-resource-domain')
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
        Suites = @()
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
        Name = 'database library owner'
        Paths = @('Libraries/Database/Wvdb-Reader.wv')
        Suites = @('libraries', 'packages')
        Gaps = @()
        VerifyPlan = $false
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
            'Libraries/Database/Tree-Node.wv',
            'Libraries/Platform/Database/Durable-Storage-Executor.wv',
            'Libraries/Platform/Database/Durable-Tree-Reader.wv',
            'Tests/Fixtures/Database/Database-Storage-Publication-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Storage-Recovery-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Single-Writer-Commit-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Tree-Node-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Root-Split-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Depth-Two-Upsert-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Storage-Self-Test.wv',
            'Tests/Fixtures/Database/Native-Hosted-Durable-Tree-Reader-Self-Test.wv',
            'Specifications/Windvale-Database-Tree-Reading-And-Root-Split.md',
            'Runtime/Native/X64-Random-Access-Storage-Host.wva',
            'Runtime/Native/Windows-X64-Random-Access-Storage.wva',
            'Runtime/Native/Linux-X64-Random-Access-Storage.wva'
        )
        Suites = @('database-storage', 'libraries')
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
            'Projects/Libraries/Windvale-Library-Database-Tree-Node.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Storage-Executor.wvproj',
            'Projects/Libraries/Windvale-Library-Durable-Tree-Reader.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Storage-Publication.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Storage-Recovery.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Single-Writer-Commit.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Tree-Node.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Root-Split.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Storage.wvproj',
            'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Reader.wvproj'
        )
        Suites = @('database-storage', 'workspace-project2', 'libraries')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'native tool checkpoint owner'
        Paths = @(
            'Specifications/Windvale-Native-Tool-Checkpoint.md',
            'Tools/Native/Build-Cached-Hosted-Application.cmd',
            'Tools/Native/Build-Cached-Hosted-Application.sh',
            'Tools/Native/Build-Cached-Linked-Image.cmd',
            'Tools/Native/Build-Cached-Linked-Image.sh',
            'Tools/Native/Build-Cached-Project-Object.cmd',
            'Tools/Native/Build-Cached-Project-Object.sh',
            'Tools/Native/Build-Cached-Project-Wvb.cmd',
            'Tools/Native/Build-Cached-Project-Wvb.sh',
            'Tools/Native/Get-Native-Hosted-Application-Cache-Key.mjs',
            'Tools/Native/Get-Native-Linked-Image-Cache-Key.mjs',
            'Tools/Native/Get-Native-Project-Cache-Key.mjs'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
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
            'lowerer-rejections',
            'console-packager-source-reconstruction',
            'database-storage'
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
            'wvdb-query-capability'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'portable package-format owner'
        Paths = @(
            'Libraries/Package/Package-Consistency.wv',
            'Libraries/Package/Package-Manifest.wv',
            'Libraries/Package/Package-Lock.wv',
            'Libraries/Package/Package-Resource-Admission.wv',
            'Tests/Fixtures/Package/Package-Consistency-Self-Test.wv',
            'Tests/Fixtures/Package/Package-Manifest-Self-Test.wv',
            'Tests/Fixtures/Package/Package-Lock-Self-Test.wv',
            'Tests/Fixtures/Package/Package-Resource-Admission-Self-Test.wv',
            'Projects/Libraries/Windvale-Library-Package-Consistency.wvproj',
            'Projects/Libraries/Windvale-Library-Package-Lock.wvproj',
            'Projects/Libraries/Windvale-Library-Package-Resource-Admission.wvproj',
            'Projects/Tests/Windvale-Native-Test-Package-Consistency.wvproj',
            'Projects/Tests/Windvale-Native-Test-Package-Manifest.wvproj',
            'Projects/Tests/Windvale-Native-Test-Package-Lock.wvproj',
            'Projects/Tests/Windvale-Native-Test-Package-Resource-Admission.wvproj'
        )
        Suites = @('package-format', 'package-bundle')
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
        Suites = @('package-format', 'package-bundle')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'development installer owner'
        Paths = @(
            'Distribution/Installers/Windvale-Development-Installer.json',
            'Distribution/Installers/Templates/windows-x64/Install-Windvale.ps1',
            'Distribution/Installers/Templates/windows-x64/Uninstall-Windvale.ps1',
            'Distribution/Installers/Templates/windows-x64/wv.cmd',
            'Distribution/Installers/Templates/windows-x64/wv-verify-installation.ps1',
            'Distribution/Installers/Templates/linux-x64/install.sh',
            'Distribution/Installers/Templates/linux-x64/uninstall.sh',
            'Distribution/Installers/Templates/linux-x64/wv',
            'Distribution/Installers/Templates/linux-x64/wv-verify-installation',
            'Tools/Release/Build-Development-Installers.mjs',
            'Tools/Native/Test-Development-Installers.cmd',
            'Tools/Native/Test-Development-Installers.sh',
            'Specifications/Windvale-Development-Installer.md',
            'LICENSE.md'
        )
        Suites = @('development-installers')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'library project owner'
        Paths = @('Projects/Libraries/Windvale-Database-Reader.wvproj')
        Suites = @('workspace-project2', 'libraries')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'workspace Project 2 owner'
        Paths = @(
            'Windvale.wvws',
            'Specifications/Windvale-Project.md',
            'Tests/Fixtures/Project/Workspace-Project2-Build.wvproj'
        )
        Suites = @('workspace-project2')
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

& $RetirementInventoryVerifier -Quiet
& $DevelopmentDependencyVerifier -Quiet

$RetirementSuitePlan = Join-Path $RepositoryRoot 'Tests/Native/Retirement-Suite.txt'
$RetirementSuiteLines = @(Get-Content -LiteralPath $RetirementSuitePlan)
if ($RetirementSuiteLines.Count -ne 60 -or
    $RetirementSuiteLines[0] -ne 'windvale-native-retirement-suite 2') {
    throw 'The native retirement-suite header or exact 59-suite inventory differs.'
}
$RetirementSuiteCases = 0
$RetirementSuiteShards = [System.Collections.Generic.HashSet[int]]::new()
foreach ($Line in $RetirementSuiteLines | Select-Object -Skip 1) {
    $Fields = $Line -split '\|', 5
    if ($Fields.Count -ne 5) {
        throw "Malformed native retirement-suite entry: $Line"
    }
    $RetirementEntryCases = 0
    $RetirementEntryShard = 0
    if (![int]::TryParse($Fields[2], [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$RetirementEntryCases) -or
        $RetirementEntryCases -le 0) {
        throw "Invalid native retirement-suite case count: $Line"
    }
    if (![int]::TryParse($Fields[3], [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$RetirementEntryShard) -or
        $RetirementEntryShard -lt 1 -or $RetirementEntryShard -gt 4) {
        throw "Invalid native retirement-suite shard: $Line"
    }
    $RetirementSuiteCases += $RetirementEntryCases
    $null = $RetirementSuiteShards.Add($RetirementEntryShard)
    $WindowsOwner = "Tools/Native/$($Fields[1]).cmd"
    $LinuxOwner = "Tools/Native/$($Fields[1]).sh"
    foreach ($Owner in @($WindowsOwner, $LinuxOwner)) {
        if (!(Test-Path -LiteralPath (Join-Path $RepositoryRoot $Owner) -PathType Leaf)) {
            throw "The native retirement suite is missing owner '$Owner'."
        }
    }
    $LinuxIndex = @(git -C $RepositoryRoot ls-files -s -- $LinuxOwner)
    if ($LASTEXITCODE -ne 0 -or $LinuxIndex.Count -ne 1 -or
        $LinuxIndex[0] -notmatch '^100755 ') {
        throw "Linux retirement-suite owner '$LinuxOwner' is not executable in Git."
    }
}
if ($RetirementSuiteCases -ne 3383 -or $RetirementSuiteShards.Count -ne 4) {
    throw 'The native retirement-suite case total or four-shard coverage differs.'
}

$GitHubVerificationWorkflow = Get-Content -LiteralPath (
    Join-Path $RepositoryRoot '.github/workflows/verify.yml') -Raw
$RequiredWorkflowFragments = @(
    'group: verify-${{ github.workflow }}-${{ github.ref }}',
    'cancel-in-progress: true',
    'if ([string]::IsNullOrWhiteSpace($env:BASE_SHA) -or',
    'git diff --check HEAD^ HEAD --',
    'run: Tools\Native\Test-Retirement-Suite.cmd --shard ${{ matrix.shard }}',
    'run: ./Tools/Native/Test-Retirement-Suite.sh --shard ${{ matrix.shard }}'
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
if (@($GitAttributes | Where-Object { $_ -eq '*.wvprov text eol=lf' }).Count -ne 1) {
    throw 'Windvale provenance files must have one exact LF text policy in .gitattributes.'
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
        $DatabaseDevelopmentDiffers
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
            "database-development=$($Plan.UseDatabaseStorageDevelopment)."
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
