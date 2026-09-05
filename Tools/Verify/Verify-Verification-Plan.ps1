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
        Name = 'PowerShell test runner and native owner live stream'
        Paths = @(
            'Tools/Native/Verification-Owner-Stream-Path.mjs',
            'Tools/Native/Verification-Owner-Result-Cache.mjs',
            'Tools/Native/Stream-Verification-Owner.mjs',
            'Tools/Native/Test-Verification-Owner-Stream.mjs',
            'Tools/Native/Test-Verification-Owner-Stream.cmd',
            'Tools/Native/Test-Verification-Owner-Stream.sh',
            'Tools/Verify/Invoke-WindvaleTests.ps1'
        )
        Suites = @('verification-owner-stream')
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'retired paired owner coordinator tombstones'
        Paths = @(
            'Tools/Native/Test-Verification-Owners.cmd',
            'Tools/Native/Test-Verification-Owners.sh'
        )
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'verification owner registry and duration policy'
        Paths = @(
            'Tests/Native/Verification-Owners.txt',
            'Tests/Native/Verification-Duration-Profiles.txt',
            'Tests/Native/Qualification-Owner-Timing-Baseline.txt',
            'Tools/Verify/Plan-Qualification-Work.mjs'
        )
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'bounded verification timing calibration'
        Paths = @(
            'Tools/Verify/Update-Verification-Timing-History.ps1',
            'Documents/Decisions/0927-Calibrate-Verification-Durations-From-Bounded-History.md'
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
        Name = 'Shared bounded development command lifecycle'
        Paths = @('Tools/Native/Development-Command-Core.mjs')
        Suites = @('language-1-front-door', 'language-1-memory-budget-split-execution')
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
        Name = 'Language 1.0 bounded memory-budget accounting owner'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-Memory-Budget-Accounting.wvproj',
            'Tests/Fixtures/Language-1.0/Memory-Budget-Accounting-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-Memory-Budget-Accounting.cmd',
            'Tools/Native/Test-Language-1.0-Memory-Budget-Accounting.sh'
        )
        Suites = @('language-1-memory-budget-accounting')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 shared memory-budget runtime oracle'
        Paths = @(
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Memory-Budget-Core.wv'
        )
        Suites = @(
            'language-1-memory-budget-accounting',
            'language-1-memory-budget-split-execution'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 bounded parallel hosted-task scheduler owner'
        Paths = @(
            'Runtime/Hosted/Tasks/Bounded-Parallel-Task-Scheduler.mjs',
            'Runtime/Hosted/Tasks/Bounded-Parallel-Task-Worker.mjs',
            'Documents/Decisions/0875-Add-A-Bounded-Parallel-Hosted-Task-Scheduler.md',
            'Specifications/Windvale-Hosted-Task-Scheduling.md',
            'Tests/Fixtures/Hosted/Bounded-Parallel-Task-Executor.mjs',
            'Tools/Native/Test-Bounded-Parallel-Task-Scheduler.cmd',
            'Tools/Native/Test-Bounded-Parallel-Task-Scheduler.mjs',
            'Tools/Native/Test-Bounded-Parallel-Task-Scheduler.sh'
        )
        Suites = @('language-1-parallel-task-scheduler')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 executable resource and structured-task owner'
        Paths = @(
            'Libraries/Foundation/Operations/Operation.wv',
            'Libraries/Foundation/Tasks/Task.wv',
            'Projects/Tests/Windvale-Language-1-Structured-Task-Call-Depth-Limit.wvproj',
            'Projects/Tests/Windvale-Language-1-Structured-Task-Retained-Result.wvproj',
            'Projects/Tests/Windvale-Language-1-Structured-Task-Trap.wvproj',
            'Projects/Tests/Windvale-Language-1-Structured-Task-Work-Limit.wvproj',
            'Projects/Tests/Windvale-Language-1-Structured-Tasks.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Structured-Task-Runtime.wvproj',
            'Specifications/Compiler-Source-Structured-Tasks.md',
            'Tests/Fixtures/Language-1.0/Async-Direct-Aggregate-Call-Executable.wv',
            'Tests/Fixtures/Language-1.0/Async-Direct-Call-Executable.wv',
            'Tests/Fixtures/Language-1.0/Async-Direct-Call-Missing-Await.wv',
            'Tests/Fixtures/Language-1.0/Async-Indirect-Call-Executable.wv',
            'Tests/Fixtures/Language-1.0/Async-Indirect-Call-Missing-Await.wv',
            'Tests/Fixtures/Language-1.0/Memory-Budget-Split-Executable.wv',
            'Tests/Fixtures/Language-1.0/Memory-Budget-Split-Failure-Executable.wv',
            'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Executable.wv',
            'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Failure-Executable.wv',
            'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Zero-Executable.wv',
            'Tests/Fixtures/Language-1.0/Owned-Vector-Calls-And-Joins-Wir.wv',
            'Tests/Fixtures/Language-1.0/Owned-Vector-Call-Use-After.wv',
            'Tests/Fixtures/Language-1.0/Owned-Vector-Call-Duplicate.wv',
            'Tests/Fixtures/Language-1.0/Owned-Vector-Call-Asymmetric-Join.wv',
            'Tests/Fixtures/Language-1.0/Owned-Vector-Loop-Invariant-Wir.wv',
            'Tests/Fixtures/Language-1.0/Owned-Vector-Loop-State-Mismatch.wv',
            'Tests/Fixtures/Language-1.0/Owned-Aggregate-Duplicate-Move.wv',
            'Tests/Fixtures/Language-1.0/Owned-Aggregate-Field-Move.wv',
            'Tests/Fixtures/Language-1.0/Owned-Aggregate-Mutable-Borrow-From-Let.wv',
            'Tests/Fixtures/Language-1.0/Owned-Aggregate-Use-After-Move.wv',
            'Tests/Fixtures/Language-1.0/Owned-Aggregate-Vector-Executable.wv',
            'Tests/Fixtures/Language-1.0/Source-File-Snapshot-Executable.wv',
            'Tests/Fixtures/Language-1.0/Structured-Task-Call-Depth-Limit-Executable.wv',
            'Tests/Fixtures/Language-1.0/Structured-Task-Completion-Order-Executable.wv',
            'Tests/Fixtures/Language-1.0/Structured-Task-Environment-Executable.wv',
            'Tests/Fixtures/Language-1.0/Structured-Task-Four-Child-Cancellation-Executable.wv',
            'Tests/Fixtures/Language-1.0/Structured-Task-Memory-Limit-Executable.wv',
            'Tests/Fixtures/Language-1.0/Structured-Task-Provider-Recovery-Executable.wv',
            'Tests/Fixtures/Language-1.0/Structured-Task-Retained-Result-Executable.wv',
            'Tests/Fixtures/Language-1.0/Structured-Task-Runtime-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Structured-Task-Trap-Executable.wv',
            'Tests/Fixtures/Language-1.0/Structured-Task-Work-Limit-Executable.wv',
            'Tests/Fixtures/Language-1.0/Structured-Tasks-Executable.wv',
            'Tests/Fixtures/Language-1.0/Sync-Call-Awaited.wv',
            'Tests/Fixtures/Language-1.0/Sync-Caller-Awaits-Async.wv',
            'Libraries/Platform/Filesystem/File.wv',
            'Tests/Fixtures/Language-1.0/Using-Non-Resource.wv',
            'Tests/Fixtures/Language-1.0/Using-Vector-Fallthrough-Wir.wv',
            'Tests/Fixtures/Language-1.0/Using-Vector-Loop-Exits-Wir.wv',
            'Tests/Fixtures/Language-1.0/Using-Vector-Moved-Before-Release.wv',
            'Tests/Fixtures/Language-1.0/Using-Vector-Nested-Return-Wir.wv',
            'Tests/Fixtures/Language-1.0/Using-Vector-Try-Propagation-Wir.wv',
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Task-Core.wv',
            'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.cmd',
            'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs',
            'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.sh',
            'Tools/Native/Verify-Language-1.0-Async-Call-Await.mjs',
            'Tools/Native/Verify-Language-1.0-Owned-Vector-Calls-Wir.mjs',
            'Compiler/Windvale/Source-Wvb-Foundation-Borrow-Plan.wv',
            'Projects/Tests/Windvale-Native-Test-Foundation-Value-Borrow-Plan.wvproj',
            'Tests/Fixtures/Language-1.0/Foundation-Value-Borrow-Plan-Self-Test.wv',
            'Projects/Tests/Windvale-Native-Test-Wvb-Typed-Directories.wvproj',
            'Tests/Fixtures/Source-Wvb/Typed-Directories-Self-Test.wv',
            'Projects/Tests/Windvale-Native-Test-Foundation-Owner-Flow.wvproj',
        'Tests/Fixtures/Source-Wvb/Foundation-Borrow-Calls-Self-Test.wv',
        'Tests/Fixtures/Source-Wvb/Foundation-Borrow-Metadata-Self-Test.wv',
        'Tests/Fixtures/Source-Wvb/Foundation-Borrow-Stack-Self-Test.wv',
        'Tests/Fixtures/Source-Wvb/Foundation-Borrow-Lifetime-Self-Test.wv',
            'Tests/Fixtures/Source-Wvb/Foundation-Owner-Flow-Self-Test.wv',
            'Tools/Native/Verify-Language-1.0-Using-Wir.mjs'
        )
        Suites = @('language-1-memory-budget-split-execution')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 frozen source and descriptor front door'
        Paths = @(
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
            'Tests/Fixtures/Language-1.0/Generic-Nested-Specialization-Discovery.wv',
            'Tests/Fixtures/Language-1.0/Generic-Resolution-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Generic-Type-Catalog-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Unsupported-Source-Profile.wv',
            'Tests/Fixtures/Language-1.0/Value-Front-End-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Fixed-Integer-Program.wv',
            'Tests/Fixtures/Language-1.0/Rune-Program.wv',
            'Tests/Fixtures/Language-1.0/Floating-Program.wv',
            'Tests/Fixtures/Language-1.0/Multi-Field-Variant.wv',
            'Tests/Fixtures/Language-1.0/Named-Variant-Field.wv',
            'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Wir.wv',
            'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Inferred.wv',
            'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Wrong-Maximum.wv',
            'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Wrong-Result.wv',
            'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Wrong-Budget.wv',
            'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Wrong-Allocation-Failure.wv',
            'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Use-After.wv',
            'Tests/Fixtures/Language-1.0/Closure-Borrow-Main-Pipeline.wv',
            'Tests/Fixtures/Language-1.0/Closure-Borrow-Mutable.wv',
            'Tests/Fixtures/Language-1.0/Closure-Copy-Main-Pipeline.wv',
            'Tests/Fixtures/Language-1.0/Closure-Move-Main-Pipeline.wv',
            'Tests/Fixtures/Language-1.0/Closure-Move-Use-After-Move.wv',
            'Projects/Tests/Windvale-Native-Test-Wvb-Fixed-Integer-Runtime.wvproj',
            'Projects/Tests/Windvale-Native-Test-Wvb-Rune-Runtime.wvproj',
            'Projects/Tests/Windvale-Native-Test-Wvb-Floating-Runtime.wvproj',
            'Tests/Native/Language-1.0-Fixture-Inventory.txt',
            'Tools/Native/Verify-Language-1.0-Migration-Fixtures.mjs',
            'Tools/Native/Verify-Source-Analysis-Diagnostic.mjs',
            'Tools/Native/Verify-Source-Wir-Incremental-Generics.mjs',
            'Tools/Native/Verify-Language-1.0-Fixed-Integers.mjs',
            'Tools/Native/Verify-Language-1.0-Runes.mjs',
            'Tools/Native/Verify-Language-1.0-Floating.mjs',
            'Tools/Native/Verify-Language-1.0-Multi-Field-Variants.mjs',
            'Tools/Native/Verify-Language-1.0-Vector-Sequence-Types.mjs',
            'Tools/Native/Verify-Language-1.0-Vector-Sequence-Runtime.mjs',
            'Tools/Native/Verify-Language-1.0-Sequence-Reads.mjs',
            'Tools/Native/Verify-Language-1.0-Vector-Reads-Freeze.mjs',
            'Tools/Native/Verify-Language-1.0-Memory-Budget-Split-Wir.mjs',
            'Tools/Native/Verify-Language-1.0-Vector-Construct-Reserved-Wir.mjs',
            'Tools/Native/Verify-Language-1.0-U8-Enums.mjs',
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Collection-Core.wv',
            'Tools/Native/Test-Language-1.0-Front-Door.cmd',
            'Tools/Native/Test-Language-1.0-Front-Door.sh',
            'Tools/Native/Verify-Language-1.0-Closure-Compiler-Pipeline.mjs',
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
            'Compiler/Windvale/Source-Wvb-Generic-Nominal-Types-Core.wv',
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
            'language-1-front-door',
            'language-1-memory-budget-split-execution'
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
        Name = 'Language 1.0 callable semantics routing'
        Paths = @(
            'Compiler/Windvale/Source-Bindings-Closures-Core.wv',
            'Compiler/Windvale/Source-Closure-Capture-Effects-Core.wv',
            'Compiler/Windvale/Source-Closure-Captures-Core.wv',
            'Compiler/Windvale/Source-Closure-Lowering-Core.wv',
            'Compiler/Windvale/Source-Callable-Types-Core.wv',
            'Compiler/Windvale/Source-Effects-Core.wv',
            'Compiler/Windvale/Source-Function-Type-Lowering-Core.wv',
            'Projects/Compiler/Windvale-Source-Bindings-Closures-Core.wvproj',
            'Projects/Compiler/Windvale-Source-Closure-Captures-Core.wvproj',
            'Projects/Compiler/Windvale-Source-Closure-Lowering-Core.wvproj',
            'Projects/Compiler/Windvale-Source-Callable-Types-Core.wvproj',
            'Projects/Compiler/Windvale-Source-Effects-Core.wvproj',
            'Projects/Compiler/Windvale-Source-Function-Type-Lowering-Core.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Closure-Capture-Semantics.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Closure-Lowering-Catalog.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Callable-Type-Catalog.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Effect-Semantics.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Function-Type-Catalog.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Function-Value-Front-End.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Named-Argument-Semantics.wvproj',
            'Specifications/Compiler-Source-Closure-Captures.md',
            'Specifications/Compiler-Source-Closure-Lowering.md',
            'Specifications/Compiler-Source-Callable-Types.md',
            'Specifications/Compiler-Source-Effects.md',
            'Specifications/Compiler-Source-Function-Types.md',
            'Tests/Fixtures/Language-1.0/Closure-Capture-Semantics-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Closure-Lowering-Catalog-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Callable-Indirect-Execution.wv',
            'Tests/Fixtures/Language-1.0/Callable-Type-Catalog-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Effect-Semantics-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Function-Type-Catalog-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Function-Value-Front-End-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Named-Argument-Semantics-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-Callable-Semantics.cmd',
            'Tools/Native/Test-Language-1.0-Callable-Semantics.mjs',
            'Tools/Native/Test-Language-1.0-Callable-Semantics.sh'
        )
        Suites = @('language-1-callable-semantics')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 shared callable runner fixture routing'
        Paths = @(
            'Tools/Native/Language-1.0-Callable-Wvb-Fixtures.mjs',
            'Tools/Native/Verify-Language-1.0-Callable-Runner.mjs'
        )
        Suites = @(
            'language-1-callable-semantics',
            'language-1-memory-budget-split-execution'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 effect clause front-end routing'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-Effect-Clause-Front-End.wvproj',
            'Tests/Fixtures/Language-1.0/Effect-Clause-Front-End-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-Effect-Clause-Front-End.cmd',
            'Tools/Native/Test-Language-1.0-Effect-Clause-Front-End.sh',
            'Tools/Native/Test-Language-1.0-Effect-Clause-Front-End.mjs'
        )
        Suites = @('language-1-effect-clause-front-end')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 admission evidence format routing'
        Paths = @(
            'Compiler/Windvale/Admission-Evidence-Core.wv',
            'Compiler/Windvale/Admission-Evidence-Validator-Core.wv',
            'Compiler/Windvale/Admission-Source-Set-Core.wv',
            'Documents/Decisions/0887-Use-A-Separately-Bounded-Admission-Validator.md',
            'Projects/Compiler/Windvale-Admission-Evidence-Core.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Admission-Evidence.wvproj',
            'Projects/Tools/Windvale-Compiler-Admission-Evidence-Validator.wvproj',
            'Specifications/Compiler-Admission-Evidence.md',
            'Tests/Fixtures/Language-1.0/Admission-Evidence-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-Admission-Evidence-Format.cmd',
            'Tools/Native/Test-Language-1.0-Admission-Evidence-Format.mjs',
            'Tools/Native/Test-Language-1.0-Admission-Evidence-Format.sh',
            'Tools/Windvale.Build/Compiler-Admission-Evidence-Validator-Driver.wv'
        )
        Suites = @(
            'language-1-admission-evidence-format',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 admission validator decision routing'
        Paths = @(
            'Documents/Decisions/0887-Use-A-Separately-Bounded-Admission-Validator.md'
        )
        Suites = @(
            'language-1-admission-evidence-format',
            'language-1-source-admission-coordinator'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 foreign catalog producer decision routing'
        Paths = @(
            'Documents/Decisions/0888-Publish-The-Canonical-WVFC-Producer.md'
        )
        Suites = @(
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 source admission coordinator decision routing'
        Paths = @(
            'Documents/Decisions/0892-Coordinate-Authenticated-Source-Admission.md'
        )
        Suites = @('language-1-source-admission-coordinator')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 admission parser evidence routing'
        Paths = @(
            'Compiler/Windvale/Source-Admission-Parser-Evidence-Core.wv',
            'Projects/Compiler/Windvale-Source-Admission-Parser-Evidence-Core.wvproj'
        )
        Suites = @(
            'language-1-system-ffi-front-end',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 source admission production dependency routing'
        Paths = @(
            'Compiler/Windvale/Source-Admission-Coordinator-Core.wv',
            'Compiler/Windvale/Source-Target-Admission-Core.wv'
        )
        Suites = @(
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 source admission coordinator routing'
        Paths = @(
            'Documents/Decisions/0892-Coordinate-Authenticated-Source-Admission.md',
            'Projects/Compiler/Windvale-Source-Admission-Coordinator-Core.wvproj',
            'Projects/Compiler/Windvale-Source-Target-Admission-Core.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Source-Admission-Coordinator.wvproj',
            'Tests/Fixtures/Language-1.0/Source-Admission-Coordinator-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-Source-Admission-Coordinator.cmd',
            'Tools/Native/Test-Language-1.0-Source-Admission-Coordinator.mjs',
            'Tools/Native/Test-Language-1.0-Source-Admission-Coordinator.sh'
        )
        Suites = @('language-1-source-admission-coordinator')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 production admission ingress owner routing'
        Paths = @(
            'Documents/Decisions/0893-Authenticate-Production-Source-Analysis-Ingress.md',
            'Tools/Native/Test-Language-1.0-Production-Admission-Ingress.cmd',
            'Tools/Native/Test-Language-1.0-Production-Admission-Ingress.mjs',
            'Tools/Native/Test-Language-1.0-Production-Admission-Ingress.sh'
        )
        Suites = @('language-1-production-admission-ingress')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 shared foreign catalog authentication routing'
        Paths = @('Compiler/Windvale/Source-Foreign-Catalog-Authentication-Core.wv')
        Suites = @(
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 production foreign catalog authentication project routing'
        Paths = @(
            'Projects/Compiler/Windvale-Source-Foreign-Catalog-Authentication-Core.wvproj'
        )
        Suites = @('language-1-production-admission-ingress')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 production wvadmit routing'
        Paths = @(
            'Projects/Tools/Windvale-Compiler-Admission-Driver.wvproj',
            'Tools/Windvale.Build/Compiler-Admission-Driver.wv'
        )
        Suites = @(
            'language-1-front-door',
            'language-1-production-admission-ingress'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 production wvauth routing'
        Paths = @(
            'Compiler/Windvale/Source-Admission-Authentication-Core.wv',
            'Projects/Tools/Windvale-Compiler-Source-Authenticator.wvproj',
            'Tools/Windvale.Build/Compiler-Source-Authenticator-Driver.wv'
        )
        Suites = @('language-1-production-admission-ingress')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 production Analyzer ingress guard routing'
        Paths = @(
            'Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj',
            'Tools/Windvale.Build/Compiler-Analysis-Driver.wv'
        )
        Suites = @(
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding',
            'compiler-split-development'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 hosted foreign binding driver project routing'
        Paths = @('Projects/Tools/Windvale-Compiler-Foreign-Binding-Driver.wvproj')
        Suites = @('language-1-production-admission-ingress')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 hosted foreign binding driver source routing'
        Paths = @('Tools/Windvale.Build/Compiler-Foreign-Binding-Driver.wv')
        Suites = @('language-1-production-admission-ingress')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 post-analysis foreign lowering builder routing'
        Paths = @('Compiler/Windvale/Source-Foreign-Lowering-Builder-Core.wv')
        Suites = @('language-1-production-admission-ingress')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 production split compiler runner routing'
        Paths = @('Tools/Native/Run-Split-Compiler.mjs')
        Suites = @(
            'language-1-front-door',
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 authenticated admission helper routing'
        Paths = @('Tools/Native/Run-Authenticated-Source-Admission.mjs')
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 target descriptor writer routing'
        Paths = @(
            'Tools/Native/Write-Canonical-Language-1.0-Target-Descriptor.mjs'
        )
        Suites = @('language-1-front-door')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 production source analysis contract routing'
        Paths = @('Specifications/Compiler-Source-Analysis.md')
        Suites = @(
            'language-1-front-door',
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 authenticated foreign binding owner routing'
        Paths = @(
            'Documents/Decisions/0923-Carry-Bound-Foreign-Facts-To-Typed-Lowering.md',
            'Projects/Compiler/Windvale-Source-Foreign-Binding-Core.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Authenticated-Foreign-Binding-Combined.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Authenticated-Foreign-Binding-Portable.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Authenticated-Foreign-Binding.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Foreign-Lowering-Pairing.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Typed-Foreign-Call-Wir.wvproj',
            'Tests/Fixtures/Language-1.0/Authenticated-Foreign-Binding-Combined-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Authenticated-Foreign-Binding-Portable-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Authenticated-Foreign-Binding-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Foreign-Lowering-Pairing-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Typed-Foreign-Call-Wir-Validation-Self-Test.wv',
            'Tests/Fixtures/Language-1.0/Typed-Foreign-Call-Wir-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-Authenticated-Foreign-Binding.cmd',
            'Tools/Native/Test-Language-1.0-Authenticated-Foreign-Binding.mjs',
            'Tools/Native/Test-Language-1.0-Authenticated-Foreign-Binding.sh'
        )
        Suites = @('language-1-authenticated-foreign-binding')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 authenticated foreign pairing production routing'
        Paths = @(
            'Compiler/Windvale/Source-Foreign-Binding-Core.wv',
            'Compiler/Windvale/Source-Foreign-Lowering-Carrier-Core.wv',
            'Compiler/Windvale/Source-Foreign-Lowering-Pairing-Core.wv'
        )
        Suites = @(
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 authenticated foreign lowering carrier contract routing'
        Paths = @('Specifications/Compiler-Source-Foreign-Lowering-Carrier.md')
        Suites = @(
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 authenticated foreign binding production routing'
        Paths = @(
            'Documents/Decisions/0895-Bind-Authenticated-Foreign-Declarations-In-A-Private-Compiler-Phase.md',
            'Documents/Decisions/0925-Publish-And-Retain-Authenticated-Foreign-Lowering-Carrier.md',
            'Documents/Decisions/0933-Pair-Authenticated-Foreign-Calls-Before-Wvb-Emission.md'
        )
        Suites = @(
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 authenticated foreign binding compiler routing'
        Paths = @('Compiler/Windvale/Source-Bindings-Core.wv')
        Suites = @(
            'source-containment',
            'generic-nominal-type-binding',
            'generic-nominal-type-layout',
            'generic-nominal-type-materialization',
            'generic-nominal-wvlb-carrier',
            'language-1-front-door',
            'language-1-authenticated-foreign-binding',
            'language-1-callable-semantics'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 authenticated foreign binding specification routing'
        Paths = @(
            'Specifications/Compiler-Source-Symbols.md',
            'Specifications/Compiler-Source-Bindings.md'
        )
        Suites = @(
            'seed',
            'unsafe-wvb',
            'source-containment',
            'language-1-front-door',
            'language-1-authenticated-foreign-binding',
            'language-1-callable-semantics',
            'lowerer-rejections',
            'console-packager-source-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'current forward-language WVB builder routing'
        Paths = @(
            'Tools/Native/Build-Current-Wvb.cmd',
            'Tools/Native/Build-Current-Wvb.sh'
        )
        Suites = @(
            'seed',
            'compiler-reconstruction',
            'unsafe-wvb',
            'wvb-containment',
            'language-1-admission-evidence-format',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'libraries',
            'packages'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 System FFI front-end routing'
        Paths = @(
            'Compiler/Windvale/Source-Target-Core.wv',
            'Projects/Tests/Windvale-Native-Test-Language-1-System-Ffi-Front-End.wvproj',
            'Tests/Fixtures/Language-1.0/System-Ffi-Front-End-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-System-Ffi-Front-End.cmd',
            'Tools/Native/Test-Language-1.0-System-Ffi-Front-End.sh',
            'Tools/Native/Test-Language-1.0-System-Ffi-Front-End.mjs'
        )
        Suites = @(
            'language-1-system-ffi-front-end',
            'language-1-admission-evidence-format',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding',
            'language-1-foreign-memory-semantics'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 foreign memory target descriptor contract routing'
        Paths = @('Specifications/Windvale-Language-1.0-Target-Descriptor.md')
        Suites = @(
            'language-1-system-ffi-front-end',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding',
            'language-1-foreign-memory-semantics'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 shared foreign catalog contract routing'
        Paths = @(
            'Specifications/Windvale-Language-1.0-Foreign-Catalog.md'
        )
        Suites = @(
            'language-1-foreign-catalog-format',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding',
            'language-1-foreign-memory-semantics'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 foreign catalog production dependency routing'
        Paths = @('Compiler/Windvale/Source-Foreign-Catalog-Core.wv')
        Suites = @(
            'language-1-foreign-catalog-format',
            'language-1-admission-evidence-format',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding',
            'language-1-foreign-memory-semantics'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 foreign catalog format routing'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-Foreign-Catalog-Format.wvproj',
            'Tests/Fixtures/Language-1.0/Foreign-Catalog-Format-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-Foreign-Catalog-Format.cmd',
            'Tools/Native/Test-Language-1.0-Foreign-Catalog-Format.sh',
            'Tools/Native/Test-Language-1.0-Foreign-Catalog-Format.mjs'
        )
        Suites = @('language-1-foreign-catalog-format')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 foreign catalog producer production dependency routing'
        Paths = @('Compiler/Windvale/Source-Foreign-Catalog-Producer-Core.wv')
        Suites = @(
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 foreign catalog producer routing'
        Paths = @(
            'Documents/Decisions/0888-Publish-The-Canonical-WVFC-Producer.md',
            'Projects/Compiler/Windvale-Source-Foreign-Catalog-Producer-Core.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Foreign-Catalog-Producer.wvproj',
            'Tests/Fixtures/Language-1.0/Foreign-Catalog-Producer-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-Foreign-Catalog-Producer.cmd',
            'Tools/Native/Test-Language-1.0-Foreign-Catalog-Producer.sh',
            'Tools/Native/Test-Language-1.0-Foreign-Catalog-Producer.mjs'
        )
        Suites = @(
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 foreign memory semantics routing'
        Paths = @(
            'Compiler/Windvale/Source-Foreign-Semantics-Core.wv',
            'Documents/Decisions/0889-Publish-The-Bounded-System-Ffi-Foreign-Memory-Semantic-Oracle.md',
            'Projects/Tests/Windvale-Native-Test-Language-1-Foreign-Memory-Profile-Regression.wvproj',
            'Projects/Tests/Windvale-Native-Test-Language-1-Foreign-Memory-Semantics.wvproj',
            'Specifications/Windvale-Language-1.0-Foreign-Memory-Semantics.md',
            'Tests/Fixtures/Language-1.0/Foreign-Memory-Profile-Regression-System.wv',
            'Tests/Fixtures/Language-1.0/Foreign-Memory-Profile-Regression.wv',
            'Tests/Fixtures/Language-1.0/Foreign-Memory-Semantics-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-Foreign-Memory-Semantics.cmd',
            'Tools/Native/Test-Language-1.0-Foreign-Memory-Semantics.sh',
            'Tools/Native/Test-Language-1.0-Foreign-Memory-Semantics.mjs'
        )
        Suites = @('language-1-foreign-memory-semantics')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 foreign memory decision routing'
        Paths = @(
            'Documents/Decisions/0889-Publish-The-Bounded-System-Ffi-Foreign-Memory-Semantic-Oracle.md'
        )
        Suites = @('language-1-foreign-memory-semantics')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 using front-end routing'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-Using-Front-End.wvproj',
            'Tests/Fixtures/Language-1.0/Using-Front-End-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-Using-Front-End.cmd',
            'Tools/Native/Test-Language-1.0-Using-Front-End.sh',
            'Tools/Native/Test-Language-1.0-Using-Front-End.mjs'
        )
        Suites = @('language-1-using-front-end')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 System FFI unsafe-context routing'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Language-1-System-Ffi-Unsafe-Context.wvproj',
            'Tests/Fixtures/Language-1.0/System-Ffi-Unsafe-Context-Self-Test.wv',
            'Tools/Native/Test-Language-1.0-System-Ffi-Unsafe-Context.cmd',
            'Tools/Native/Test-Language-1.0-System-Ffi-Unsafe-Context.sh',
            'Tools/Native/Test-Language-1.0-System-Ffi-Unsafe-Context.mjs'
        )
        Suites = @('language-1-system-ffi-unsafe-context')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 unsafe WIR and type-surface routing'
        Paths = @(
            'Tools/Native/Test-Language-1.0-Unsafe-Wir.mjs',
            'Tools/Native/Test-Language-1.0-Unsafe-Scratch-Wir.mjs',
            'Tools/Native/Test-Language-1.0-Unsafe-Type-Surface.mjs',
            'Libraries/Foundation/Unsafe/Unsafe.wv',
            'Documents/Decisions/0898-Publish-Canonical-Foundation-Unsafe-Type-Identities.md',
            'Documents/Decisions/0899-Lower-Canonical-Unsafe-Scratch-Construction-To-Wvir.md',
            'Documents/Decisions/0909-Lower-Mutable-Unsafe-Write-Region-Borrowing-To-Wvir.md',
            'Documents/Decisions/0914-Lower-Canonical-Unsafe-Write-Pointer-To-Wvir.md'
        )
        Suites = @('language-1-callable-semantics')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Native x64 unsafe write-pointer development routing'
        Paths = @(
            'Tools/Native/Test-Language-1.0-Unsafe-Write-Region-Wir.mjs',
            'Tools/Native/Test-Native-Unsafe-Write-Pointer-Lowering.cmd',
            'Tools/Native/Test-Native-Unsafe-Write-Pointer-Lowering.mjs',
            'Tools/Native/Test-Native-Unsafe-Write-Pointer-Lowering.sh',
            'Tests/Native/Wvb-To-Wvo-Rejections/Foreign-Runtime-Stale.wvb.b64',
            'Tests/Native/Wvb-To-Wvo-Rejections/Foreign-Runtime-Success.wvb.b64',
            'Tests/Native/Wvb-To-Wvo-Rejections/Unsafe-Write-Pointer.wvb.b64',
            'Tests/Native/Wvb-To-Wvo-Rejections/Unsafe-Write-Pointer-Runtime.wvb.b64',
            'Tests/Native/X64-Paper-Buffer-Source.wva'
        )
        Suites = @('native-x64-lowering-development')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Slice 8 runtime-owned Foreign provider routing'
        Paths = @('Runtime/Native/Linux-X64-Paper-Buffer-Source.wva')
        Suites = @(
            'native-x64-lowering-development',
            'language-1-production-admission-ingress'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Slice 8 source Foreign runtime consumer routing'
        Paths = @('Runtime/Windvale/Foreign-Record-Consumer.wv')
        Suites = @('language-1-production-admission-ingress')
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
            'language-1-front-door',
            'language-1-authenticated-foreign-binding'
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
            'language-1-front-door',
            'language-1-effect-clause-front-end',
            'language-1-system-ffi-front-end',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'language-1-callable-semantics'
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
            'language-1-front-door',
            'language-1-system-ffi-unsafe-context',
            'language-1-foreign-catalog-producer',
            'language-1-using-front-end',
            'language-1-callable-semantics'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 descriptor producer dependency routing'
        Paths = @('Compiler/Windvale/Source-Descriptor-Core.wv')
        Suites = @(
            'language-1-front-door',
            'language-1-admission-evidence-format',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 foreign catalog producer format dependency routing'
        Paths = @('Compiler/Windvale/Source-Foreign-Catalog-Core.wv')
        Suites = @(
            'language-1-foreign-catalog-format',
            'language-1-admission-evidence-format',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding',
            'language-1-foreign-memory-semantics'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 lexer and source set producer dependency routing'
        Paths = @(
            'Compiler/Windvale/Source-Lexer-Core.wv',
            'Compiler/Windvale/Source-Set-Core.wv'
        )
        Suites = @(
            'source-containment',
            'generic-nominal-type-binding',
            'generic-nominal-type-layout',
            'generic-nominal-type-materialization',
            'generic-nominal-wvlb-carrier',
            'language-1-front-door',
            'language-1-effect-clause-front-end',
            'language-1-system-ffi-front-end',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'language-1-using-front-end',
            'language-1-callable-semantics'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 SHA-256 producer dependency routing'
        Paths = @('Foundation/Sha256.wv')
        Suites = @(
            'seed',
            'wv-linker-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'Language 1.0 body parser specification routing'
        Paths = @('Specifications/Compiler-Source-Body-Parser.md')
        Suites = @(
            'seed',
            'unsafe-wvb',
            'source-containment',
            'language-1-system-ffi-unsafe-context',
            'language-1-using-front-end',
            'lowerer-rejections',
            'console-packager-source-reconstruction'
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
        Name = 'retired Seed native front-door reconstruction names'
        Paths = @(
            'Tools/Native/Test-Seed-Native-Front-Door-Reconstruction.cmd',
            'Tools/Native/Test-Seed-Native-Front-Door-Reconstruction.sh',
            'Tools/Verify/Verify-Seed-Native-Front-Door-Reconstruction.ps1',
            'Tools/Verify/Verify-Seed-Native-Front-Door-Reconstruction.sh',
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
            'Compiler/Windvale/Source-Wir-Consumer-Core.wv',
            'Compiler/Windvale/Source-Wir-Core.wv',
            'Compiler/Windvale/Source-Wvb-Compilation-Core.wv',
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
            'language-1-effect-clause-front-end',
            'language-1-system-ffi-front-end',
            'language-1-system-ffi-unsafe-context',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'language-1-authenticated-foreign-binding',
            'language-1-using-front-end',
            'language-1-callable-semantics',
            'language-1-memory-budget-split-execution',
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
            'Tools/Native/Verify-Compiler-Convergence.cmd',
            'Tools/Native/Verify-Compiler-Convergence.sh',
            'Tools/Native/Verify-Current-Split-Compiler-Convergence.mjs',
            'Tools/Verify/Verify-Bootstrap.cmd',
            'Tools/Verify/Verify-Bootstrap.sh',
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
            'native-sha256-lowering',
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'language-1-admission-evidence-format',
            'compiler-split-development'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'target-aware segmented compiler package consumers'
        Paths = @(
            'Tools/Native/Package-Segmented-Compiler-Wvb.cmd',
            'Tools/Native/Package-Segmented-Compiler-Wvb.sh'
        )
        Suites = @(
            'segmented-compiler-toolset-reconstruction',
            'wvb-runner-reconstruction',
            'language-1-admission-evidence-format',
            'compiler-split-development'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'unconsumed staging project excludes native SHA-256 owner'
        Paths = @(
            'Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Admission.wvproj'
        )
        Suites = @(
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction'
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
        Name = 'hosted enum request service reader'
        Paths = @('Runtime/Windvale/Native-Hosted-Enum-Service-Request.wv')
        Suites = @(
            'seed',
            'seed-native-front-door',
            'wvb-runner-reconstruction',
            'wvb-inspector-reconstruction',
            'wvo-inspector-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'unsafe-wvb',
            'wvb-containment',
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
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'wvo-publisher-reconstruction',
            'compiler-split-development',
            'console-packager-container-reconstruction'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'segmented compiler source closure'
        Paths = @(
            'Compiler/Windvale/Native-X64-Lowering-Core.wv',
            'Compiler/Windvale/Native-X64-Lowering-Descriptor-Instructions.wv',
            'Compiler/Windvale/Native-X64-Lowering-Object.wv',
            'Compiler/Windvale/Native-X64-Lowering-Publication.wv',
            'Compiler/Windvale/Native-X64-Lowering-Record-Storage.wv',
            'Compiler/Windvale/Native-X64-Lowering-Staging-Tool.wv',
            'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Envelope.wv',
            'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Relocations-Native-Bridge.wv',
            'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Relocations.wv',
            'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Symbols.wv'
        )
        Suites = @(
            'segmented-compiler-toolset-reconstruction',
            'native-x64-lowering-development'
        )
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $false
    },
    @{
        Name = 'shared lowerer source selects current-source development owner'
        Paths = @('Compiler/Windvale/Native-X64-Lowering-Bytes-Concatenation.wv')
        Suites = @('native-x64-lowering-development')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $false
    },
    @{
        Name = 'native x64 lowering specification selects focused database host boundary'
        Paths = @('Specifications/Windvale-Native-X64-Lowering.md')
        Suites = @(
            'seed',
            'wvb-to-wvo-reconstruction',
            'unsafe-wvb',
            'source-containment',
            'lowerer-rejections',
            'console-packager-source-reconstruction',
            'native-u64-lowering',
            'model-provider',
            'database-storage'
        )
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'host-storage'
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
        Name = 'native SHA-256 lowering owner'
        Paths = @(
            'Compiler/Windvale/Native-X64-Lowering-Sha256.wv',
            'Projects/Tests/Windvale-Native-Test-Sha256-Native-Kat.wvproj',
            'Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Sha256.wvproj',
            'Tests/Fixtures/Native-X64/Sha256-Native-Kat.wv',
            'Tests/Fixtures/Native-X64/Wvb-To-Wvo-Sha256-Smoke.wv',
            'Tools/Native/Test-Native-Sha256.cmd',
            'Tools/Native/Test-Native-Sha256.mjs',
            'Tools/Native/Test-Native-Sha256.sh'
        )
        Suites = @('native-sha256-lowering')
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
            'native-x64-lowering-development',
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
        Name = 'unconsumed lowerer project excludes native SHA-256 owner'
        Paths = @('Projects/Compiler/Windvale-Native-X64-Lowering.wvproj')
        Suites = @(
            'wvb-to-wvo-reconstruction',
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
        Name = 'portable WVB metadata-aware verifier owner'
        Paths = @(
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Metadata-Core.wv',
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Semantic-Core.wv',
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Executable-Core.wv',
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Typed-Directories.wv',
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Foundation-Owner-Flow.wv',
            'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Tool.wv',
            'Tools/Windvale.Verify/Wvb-Metadata-Normalization.wv',
            'Projects/Tests/Windvale-Wvb-Metadata-Normalization-Self-Test.wvproj',
            'Tests/Fixtures/Source-Wvb/Metadata-Normalization-Self-Test.wv'
        )
        Suites = @(
            'seed',
            'wvb-to-wvo-reconstruction',
            'unsafe-wvb',
            'wvb-containment',
            'language-1-front-door',
            'language-1-callable-semantics',
            'language-1-memory-budget-split-execution',
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
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
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
            'compiler-split-development',
            'console-packager-container-reconstruction',
            'hosted-verifier-publisher-files'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'current WVB-to-WVO source root'
        Paths = @('Compiler/Windvale/Native-X64-Lowering-Tool.wv')
        Suites = @('native-x64-lowering-development')
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
            'Projects/Tests/Language-1.0-Source-Analysis-Self-Test.wvproj'
        )
        Suites = @('language-1-front-door', 'language-1-callable-semantics')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'segmented hosted WVB cache safety owner'
        Paths = @(
            'Tools/Native/Test-Cached-Segmented-Hosted-Wvb.cmd',
            'Tools/Native/Test-Cached-Segmented-Hosted-Wvb.sh',
            'Tools/Native/Test-Cached-Segmented-Hosted-Wvb.mjs'
        )
        Suites = @('segmented-hosted-wvb-cache')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'segmented hosted WVB cache producer owner routing'
        Paths = @(
            'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.cmd',
            'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.sh',
            'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.mjs'
        )
        Suites = @('segmented-hosted-wvb-cache')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'legacy hosted application cache routes to focused cache owner'
        Paths = @(
            'Tools/Native/Build-Cached-Hosted-Application.cmd',
            'Tools/Native/Build-Cached-Hosted-Application.sh',
            'Tools/Native/Native-Hosted-Application-Cache-Core.mjs'
        )
        Suites = @('segmented-hosted-wvb-cache')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'retired aggregate verification aliases are deletion tombstones'
        Paths = @(
            'Tools/Native/Test-Retirement-Suite.cmd',
            'Tools/Native/Test-Retirement-Suite.sh'
        )
        Suites = @()
        Gaps = @()
        VerifyPlan = $true
    },
    @{
        Name = 'hosted WVB packaging excludes unrelated database owners'
        Paths = @(
            'Tools/Native/Package-Hosted-Wvb.cmd',
            'Tools/Native/Package-Hosted-Wvb.sh'
        )
        Suites = @(
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'compiler-split-development',
            'console-packager-container-reconstruction',
            'hosted-verifier-publisher-files',
            'native-u64-lowering'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'compiler artifact reader generator routing'
        Paths = @('Tools/Native/Generate-Compiler-Artifact-Readers.mjs')
        Suites = @('wvb-runner-reconstruction', 'compiler-split-development')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'compiler split development owner'
        Paths = @(
            'Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj',
            'Specifications/Compiler-Split-Development-Cache.md',
            'Tools/Native/Test-Cached-Split-Project-Wvb.mjs',
            'Tools/Native/Test-Compiler-Split-Development.cmd',
            'Tools/Native/Test-Compiler-Split-Development.sh',
            'Tools/Native/Test-Compiler-Split-Development.mjs',
            'Tools/Native/Write-Split-Compiler-Producer-Identity.mjs',
            'Tools/Windvale.Build/Compiler-Analysis-Driver.wv'
        )
        Suites = @(
            'wvb-runner-reconstruction',
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding',
            'compiler-split-development'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'compiler split development specification routing'
        Paths = @('Specifications/Compiler-Split-Development-Cache.md')
        Suites = @('compiler-split-development')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'production cached split builder routing'
        Paths = @('Tools/Native/Build-Cached-Split-Project-Wvb.mjs')
        Suites = @(
            'wvb-runner-reconstruction',
            'language-1-production-admission-ingress',
            'compiler-split-development'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'production emitter project routing'
        Paths = @('Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj')
        Suites = @(
            'language-1-production-admission-ingress',
            'compiler-split-development'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'production emitter driver routing'
        Paths = @('Tools/Windvale.Build/Compiler-Emission-Driver.wv')
        Suites = @(
            'language-1-production-admission-ingress',
            'compiler-split-development'
        )
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'current split project builder'
        Paths = @(
            'Tools/Native/Build-Current-Split-Project-Wvb.mjs'
        )
        Suites = @(
            'wvb-runner-reconstruction',
            'language-1-authenticated-foreign-binding',
            'compiler-split-development'
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
        Suites = @(
            'seed-native-front-door',
            'wvb-runner-reconstruction',
            'scripting',
            'language-1-memory-budget-split-execution'
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
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
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
            'compiler-split-development',
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
        Name = 'native scalar runner allocation execution owner'
        Paths = @(
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Envelope.wv',
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Fixed-Integer-Core.wv',
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Floating-Core.wv',
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Main.wv',
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Rune-Core.wv'
        )
        Suites = @(
            'language-1-front-door',
            'language-1-callable-semantics',
            'language-1-memory-budget-split-execution'
        )
        Gaps = @()
        VerifyPlan = $false
        VerifyWebAssembly = $false
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
        Name = 'native unsafe WVB rejection specification owner'
        Paths = @('Specifications/Windvale-Native-Wvb-Unsafe-Rejection-Tests.md')
        Suites = @('unsafe-wvb')
        Gaps = @()
        VerifyPlan = $false
    },
    @{
        Name = 'focused database qualification-fixture development targets'
        Paths = @(
            'Tests/Fixtures/Database/Database-Storage-Publication-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Storage-Recovery-Self-Test.wv',
            'Tests/Fixtures/Database/Database-Single-Writer-Commit-Self-Test.wv'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'publication+recovery+single-writer'
        DatabaseCases = 3
        DatabaseExecutions = 2
        DatabaseExpectedSeconds = 110
        DatabaseMaximumSeconds = 300
    },
    @{
        Name = 'bundled database publication and recovery development target'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Database-Storage-Publication-Recovery-Bundle.wvproj',
            'Tests/Fixtures/Database/Database-Storage-Publication-Recovery-Bundle-Self-Test.wv'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'publication+recovery'
        DatabaseCases = 2
        DatabaseExecutions = 1
        DatabaseExpectedSeconds = 65
        DatabaseMaximumSeconds = 210
    },
    @{
        Name = 'bundled database root-growth development target'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth-Bundle.wvproj',
            'Tests/Fixtures/Database/Database-Transaction-Root-Growth-Bundle-Self-Test.wv'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-root-growth-bundle'
        DatabaseCases = 2
        DatabaseExecutions = 1
        DatabaseExpectedSeconds = 65
        DatabaseMaximumSeconds = 210
    },
    @{
        Name = 'bundled database leaf-groups-pages development target'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Groups-Pages-Bundle.wvproj',
            'Tests/Fixtures/Database/Database-Transaction-Leaf-Groups-Pages-Bundle-Self-Test.wv'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-leaf-groups-pages-bundle'
        DatabaseCases = 2
        DatabaseExecutions = 1
        DatabaseExpectedSeconds = 65
        DatabaseMaximumSeconds = 210
    },
    @{
        Name = 'bundled database root-split-depth-two development target'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Database-Root-Split-Depth-Two-Bundle.wvproj',
            'Tests/Fixtures/Database/Database-Root-Split-Depth-Two-Bundle-Self-Test.wv'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'root-split-depth-two-bundle'
        DatabaseCases = 2
        DatabaseExecutions = 1
        DatabaseExpectedSeconds = 65
        DatabaseMaximumSeconds = 210
    },
    @{
        Name = 'bundled database ancestor-groups development target'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups-Bundle.wvproj',
            'Tests/Fixtures/Database/Database-Transaction-Ancestor-Groups-Bundle-Self-Test.wv'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-ancestor-groups-bundle'
        DatabaseCases = 2
        DatabaseExecutions = 1
        DatabaseExpectedSeconds = 65
        DatabaseMaximumSeconds = 210
    },
    @{
        Name = 'bundled database ancestor-pages development target'
        Paths = @(
            'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages-Bundle.wvproj',
            'Tests/Fixtures/Database/Database-Transaction-Ancestor-Pages-Bundle-Self-Test.wv'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'transaction-ancestor-pages-bundle'
        DatabaseCases = 2
        DatabaseExecutions = 1
        DatabaseExpectedSeconds = 65
        DatabaseMaximumSeconds = 210
    },
    @{
        Name = 'database development target-set union'
        Paths = @(
            'Libraries/Database/Local-Database-Put.wv',
            'Specifications/Windvale-Database-Json-Value.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'host-local-service+json-value+local-service'
        DatabaseCases = 4
        DatabaseExecutions = 4
        DatabaseExpectedSeconds = 290
        DatabaseMaximumSeconds = 660
    },
    @{
        Name = 'database development target-set retains transaction peers'
        Paths = @(
            'Specifications/Windvale-Database-Transaction-Commit.md',
            'Specifications/Windvale-Database-Json-Protocol.md'
        )
        Suites = @('database-storage')
        Gaps = @()
        VerifyPlan = $false
        DatabaseDevelopment = $true
        DatabaseTarget = 'json-protocol+transaction-commit'
        DatabaseCases = 3
        DatabaseExecutions = 3
        DatabaseExpectedSeconds = 155
        DatabaseMaximumSeconds = 390
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
        DatabaseTarget = 'bootstrap+depth-three+depth-three-upsert+depth-two+engine+host-local-service+host-root-writer+host-storage+host-tree-delete+host-tree-reader+host-tree-scan+host-tree-writer+publication+recovery+root-split+single-leaf+single-writer+transaction-ancestor-groups+transaction-ancestor-pages+transaction-branch-pages+transaction-leaf-groups+transaction-leaf-pages+transaction-parent-groups+transaction-paths+transaction-root-growth+transaction-tree-completion+tree-path-delete+tree-path-upsert'
        DatabaseCases = 34
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
        Suites = @(
            'segmented-hosted-wvb-cache',
            'os-x64-code-emission',
            'database-storage'
        )
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
            'language-1-authenticated-foreign-binding',
            'language-1-using-front-end',
            'language-1-callable-semantics',
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
            'Artifacts/Native-Wvb-Runner-0.1.0/README.md',
            'Artifacts/Native-Wvb-Runner-0.1.0/windows-x64-wvrun.exe',
            'Artifacts/Native-Wvb-Runner-0.1.0/linux-x64-wvrun.elf',
            'Artifacts/Native-Wvb-Verifier-0.1.0/README.md',
            'Artifacts/Native-Wvb-Verifier-0.1.0/windows-x64-wvverify.exe',
            'Artifacts/Native-Wvb-Verifier-0.1.0/linux-x64-wvverify.elf',
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
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0833-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0857-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0861-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0870-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0894-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0901-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0915-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0917-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0919-Candidate.txt',
            'Documents/Project/Windvale-Language-1.0-Source-Amendment-0942-Candidate.txt',
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
            'Documents/Decisions/0458-Native-Changed-File-Verification.md',
            'Specifications/Legacy-Status-Classifications.json'
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

$VerificationDurationPlan = Join-Path $RepositoryRoot `
    'Tests/Native/Verification-Duration-Profiles.txt'
$VerificationDurationLines = @(Get-Content -LiteralPath $VerificationDurationPlan)
if ($VerificationDurationLines.Count -lt 2 -or
    $VerificationDurationLines[0] -ne
        'windvale-native-verification-duration-profiles 1') {
    throw 'The native verification duration-profile registry is invalid.'
}
$VerificationDurationProfiles = @{}
foreach ($Line in $VerificationDurationLines | Select-Object -Skip 1) {
    $Fields = $Line -split '\|', 4
    $ExpectedSeconds = 0
    $MaximumSeconds = 0
    $InfrastructureRetries = 0
    if ($Fields.Count -ne 4 -or
        $Fields[0] -cnotmatch '^[a-z]+(?:-[a-z]+)*$' -or
        $VerificationDurationProfiles.ContainsKey($Fields[0]) -or
        ![int]::TryParse($Fields[1], [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$ExpectedSeconds) -or
        ![int]::TryParse($Fields[2], [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$MaximumSeconds) -or
        ![int]::TryParse($Fields[3], [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$InfrastructureRetries) -or
        $ExpectedSeconds -lt 1 -or $ExpectedSeconds -gt 3600 -or
        $MaximumSeconds -lt $ExpectedSeconds -or $MaximumSeconds -gt 3600 -or
        $InfrastructureRetries -lt 0 -or $InfrastructureRetries -gt 1) {
        throw "Invalid native verification duration profile: $Line"
    }
    $VerificationDurationProfiles[$Fields[0]] = $true
}

$VerificationOwnerPlan = Join-Path $RepositoryRoot 'Tests/Native/Verification-Owners.txt'
$VerificationOwnerLines = @(Get-Content -LiteralPath $VerificationOwnerPlan)
if ($VerificationOwnerLines.Count -lt 2 -or
    $VerificationOwnerLines[0] -ne 'windvale-native-verification-owners 2') {
    throw 'The native verification-owner header or inventory is invalid.'
}
$VerificationOwnerNames = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$VerificationOwnerCommands = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$VerificationOwnerProfiles = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$VerificationOwnerShards = [System.Collections.Generic.HashSet[int]]::new()
foreach ($Line in $VerificationOwnerLines | Select-Object -Skip 1) {
    $Fields = $Line -split '\|', 6
    if ($Fields.Count -ne 6) {
        throw "Malformed native verification-owner entry: $Line"
    }
    $VerificationOwnerEntryCases = 0
    $VerificationOwnerEntryShard = 0
    if ($Fields[0] -cnotmatch '^[a-z0-9]+(?:[.-][a-z0-9]+)*$' -or
        !$VerificationOwnerNames.Add($Fields[0])) {
        throw "Invalid or duplicate native verification-owner name: $Line"
    }
    if ($Fields[1] -cnotmatch '^[A-Za-z0-9]+(?:[.-][A-Za-z0-9]+)*$' -or
        !$VerificationOwnerCommands.Add($Fields[1])) {
        throw "Invalid or duplicate native verification-owner command: $Line"
    }
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
    if (!$VerificationDurationProfiles.ContainsKey($Fields[4])) {
        throw "Unknown native verification duration profile: $Line"
    }
    $null = $VerificationOwnerProfiles.Add($Fields[4])
    if ([string]::IsNullOrWhiteSpace($Fields[5])) {
        throw "Missing native verification-owner terminal summary: $Line"
    }
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
if ($VerificationOwnerShards.Count -ne 4) {
    throw 'The native verification-owner registry does not cover all four qualification shards.'
}
if ($VerificationOwnerProfiles.Count -ne $VerificationDurationProfiles.Count) {
    throw 'Not every native verification duration profile is assigned.'
}

$QualificationWorkPlanner = Join-Path $RepositoryRoot `
    'Tools/Verify/Plan-Qualification-Work.mjs'
$QualificationWorkPlan = (& node $QualificationWorkPlanner --json) |
    ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or
    $QualificationWorkPlan.Format -ne 'windvale-qualification-work-plan-4' -or
    $QualificationWorkPlan.Owners -ne 126 -or
    $QualificationWorkPlan.Cases -ne ($VerificationOwnerLines | Select-Object -Skip 1 |
        ForEach-Object { [int]$_.Split('|')[2] } | Measure-Object -Sum).Sum -or
    $QualificationWorkPlan.ObservedCases -ne 5981 -or
    $QualificationWorkPlan.TimingEvidence -ne 'historical-only' -or
    $QualificationWorkPlan.TimingCaseCountMismatches.Count -ne @(
        $QualificationWorkPlan.OwnerAnalysis | Where-Object { $_.Cases -ne $_.ObservedCases }).Count -or
    $QualificationWorkPlan.TotalExpectedSeconds -ne 19845 -or
    $QualificationWorkPlan.TotalMaximumSeconds -ne 79500 -or
    $QualificationWorkPlan.DualHostExpectedWorkSeconds -ne
        2 * $QualificationWorkPlan.TotalExpectedSeconds -or
    $QualificationWorkPlan.DeclaredCriticalPathExpectedSeconds -ne 7260 -or
    $QualificationWorkPlan.DeclaredCriticalPathMaximumSeconds -ne 25200 -or
    $QualificationWorkPlan.MinimumShardExpectedSeconds -ne 2820 -or
    $QualificationWorkPlan.IdealShardExpectedSeconds -ne 4962 -or
    $QualificationWorkPlan.ShardExpectedSpreadSeconds -ne 4440 -or
    $QualificationWorkPlan.DeclaredParallelEfficiencyBasisPoints -ne 6833 -or
    $QualificationWorkPlan.TimingSourceCommit -ne
        '47dd3d69fef8a0ac5b894885b0a1917e21033622' -or
    $QualificationWorkPlan.TimingRunId -ne '33894696448' -or
    $QualificationWorkPlan.TimingDate -ne '2026-09-04' -or
    $QualificationWorkPlan.TimingComparisonCommit -ne
        'adf6e6bfe9a4d8222f28a6e169827551cccf4469' -or
    $QualificationWorkPlan.PriorObservedCriticalPathMilliseconds -ne 6547869 -or
    $QualificationWorkPlan.ObservedWindowsTotalMilliseconds -ne 17904835 -or
    $QualificationWorkPlan.ObservedLinuxTotalMilliseconds -ne 16467401 -or
    $QualificationWorkPlan.ObservedWindowsCriticalPathMilliseconds -ne 4655707 -or
    $QualificationWorkPlan.ObservedLinuxCriticalPathMilliseconds -ne 4521081 -or
    $QualificationWorkPlan.ObservedCriticalPathMilliseconds -ne 4655707 -or
    $QualificationWorkPlan.IdealObservedCriticalPathMilliseconds -ne 4476209 -or
    $QualificationWorkPlan.ObservedWindowsParallelEfficiencyBasisPoints -ne 9614 -or
    $QualificationWorkPlan.ObservedLinuxParallelEfficiencyBasisPoints -ne 9105 -or
    $QualificationWorkPlan.ObservedCombinedParallelEfficiencyBasisPoints -ne 9228 -or
    $QualificationWorkPlan.LongOwners -ne 10 -or
    $QualificationWorkPlan.LongOwnerExpectedSeconds -ne 13440 -or
    $QualificationWorkPlan.OwnerAnalysis.Count -ne
        $QualificationWorkPlan.Owners -or
    $QualificationWorkPlan.AnalysisFiles -ne
        ($QualificationWorkPlan.OwnerAnalysis.AnalysisFiles |
            Measure-Object -Sum).Sum -or
    $QualificationWorkPlan.SourceLines -ne
        ($QualificationWorkPlan.OwnerAnalysis.SourceLines |
            Measure-Object -Sum).Sum -or
    $QualificationWorkPlan.OwnerProjectReferences -ne
        ($QualificationWorkPlan.OwnerAnalysis.UniqueProjects |
            Measure-Object -Sum).Sum -or
    $QualificationWorkPlan.AnalysisFiles -lt
        2 * $QualificationWorkPlan.Owners -or
    $QualificationWorkPlan.SourceLines -lt 40000 -or
    $QualificationWorkPlan.Shards.Count -ne 4 -or
    ($QualificationWorkPlan.Shards.ExpectedSeconds |
        Measure-Object -Sum).Sum -ne
        $QualificationWorkPlan.TotalExpectedSeconds -or
    $QualificationWorkPlan.TopExpectedOwners[0].Name -ne
        'language-1-authenticated-foreign-binding' -or
    $QualificationWorkPlan.TopExpectedOwners[0].AnalysisFiles -ne 3 -or
    $QualificationWorkPlan.TopExpectedOwners[0].UniqueProjects -ne 2 -or
    $QualificationWorkPlan.TopExpectedOwners[0].PipelineCallSites -ne 1 -or
    $QualificationWorkPlan.TopExpectedOwners[1].Name -ne 'database-storage' -or
    $QualificationWorkPlan.TopExpectedOwners[1].UniqueProjects -ne 66 -or
    $QualificationWorkPlan.TopExpectedOwners[1].PipelineCallSites -ne 86 -or
    $QualificationWorkPlan.TopExpectedOwners[5].Name -ne
        'language-1-front-door' -or
    $QualificationWorkPlan.TopExpectedOwners[5].PipelineCallSites -ne 237 -or
    $QualificationWorkPlan.TopObservedOwners[0].Name -ne
        'language-1-front-door' -or
    $QualificationWorkPlan.TopObservedOwners[1].Name -ne
        'wvb-runner-reconstruction' -or
    $QualificationWorkPlan.TopObservedOwners[2].Name -ne
        'language-1-memory-budget-split-execution' -or
    $QualificationWorkPlan.TopObservedOwners[3].Name -ne 'database-storage' -or
    $QualificationWorkPlan.RepeatedProjects.Count -ne 11 -or
    $QualificationWorkPlan.NestedOwnerEdges.Count -ne 0 -or
    $QualificationWorkPlan.PipelineUses.Count -ne 19) {
    throw 'The complete qualification work inventory differs.'
}
$QualificationShardSignature = @(
    $QualificationWorkPlan.Shards | ForEach-Object {
        $ShardNumber = $_.Shard
        $CurrentShardCases = ($VerificationOwnerLines | Select-Object -Skip 1 |
            Where-Object { [int]$_.Split('|')[3] -eq $ShardNumber } |
            ForEach-Object { [int]$_.Split('|')[2] } | Measure-Object -Sum).Sum
        if ($_.Cases -ne $CurrentShardCases) { throw 'Current shard coverage differs from its registry.' }
        "$($_.Shard)|$($_.Owners)|$($_.ExpectedSeconds)|$($_.MaximumSeconds)|$($_.ObservedWindowsMilliseconds)|$($_.ObservedLinuxMilliseconds)"
    }
) -join ','
if ($QualificationShardSignature -cne
    '1|13|2820|11400|4610402|4077157,2|40|4950|25200|4041125|4521081,3|37|4815|23700|4597601|4504598,4|36|7260|19200|4655707|3364565') {
    throw "The measured qualification shard assignment differs: $QualificationShardSignature"
}
$QualificationPipelineExpected = @{
    'Build-Current-Wvb' = '11|41'
    'Build-Wvb' = '48|221'
    'Build-Cached-Project-Object' = '1|2'
    'Build-Cached-Hosted-Application' = '12|44'
    'Build-Cached-Split-Project-Wvb' = '3|18'
    'Build-Cached-Segmented-Hosted-Wvb' = '8|11'
    'Stage-Compiler-Wvb' = '3|10'
    'Lower-Wvb-To-Wvo' = '16|45'
    'Check-Wvo' = '20|57'
    'Link-Wvo' = '39|122'
    'Package-Hosted-Wvb' = '19|111'
    'Package-Console' = '19|77'
    'Package-Segmented-Compiler-Wvb' = '21|60'
    'Verify-Wvb' = '5|16'
    'Verify-Wvo' = '10|34'
    'Verify-Source-Analysis-Diagnostic' = '1|11'
    'Run-Wvb' = '8|60'
    'Run-Split-Compiler' = '3|83'
    'Run-Authenticated-Source-Admission' = '1|30'
}
foreach ($PipelineUse in $QualificationWorkPlan.PipelineUses) {
    $ActualPipelineUse = "$($PipelineUse.Owners)|$($PipelineUse.ScriptCallSites)"
    if (!$QualificationPipelineExpected.ContainsKey($PipelineUse.Marker) -or
        $QualificationPipelineExpected[$PipelineUse.Marker] -ne $ActualPipelineUse) {
        throw "The complete qualification pipeline inventory differs at '$($PipelineUse.Marker)'."
    }
}
$QualificationWorkOwners = @(& node $QualificationWorkPlanner --owners)
if ($LASTEXITCODE -ne 0 -or
    $QualificationWorkOwners.Count -ne $QualificationWorkPlan.Owners -or
    !$QualificationWorkOwners[0].StartsWith(
        'verification-owner-stream|2|6|quick|15|300|',
        [StringComparison]::Ordinal) -or
    !$QualificationWorkOwners[-1].StartsWith(
        'wvdb-approval|4|13|quick|15|300|',
        [StringComparison]::Ordinal)) {
    throw 'The complete qualification owner rows differ.'
}
$QualificationWorkTimings = @(& node $QualificationWorkPlanner --timings)
if ($LASTEXITCODE -ne 0 -or
    $QualificationWorkTimings.Count -ne $QualificationWorkPlan.Owners -or
    !$QualificationWorkTimings[0].StartsWith(
        'verification-owner-stream|2|6|20316|5881|',
        [StringComparison]::Ordinal) -or
    !$QualificationWorkTimings[-1].StartsWith(
        'wvdb-approval|4|13|1815|587|',
        [StringComparison]::Ordinal)) {
    throw 'The complete qualification timing rows differ.'
}

$PowerShellTestRunner = Join-Path $PSScriptRoot 'Invoke-WindvaleTests.ps1'
$PowerShellTestRunnerSource = Get-Content -Raw -LiteralPath $PowerShellTestRunner
foreach ($Fragment in @(
    'Get-Command node -All -CommandType Application',
    '$Node = $NodeCandidates[0]',
    '& $Node.Source $StreamHelper',
    'Out-Host'
)) {
    if (!$PowerShellTestRunnerSource.Contains(
            $Fragment, [StringComparison]::Ordinal)) {
        throw "The PowerShell test runner lacks deterministic Node selection: $Fragment"
    }
}
$TestRunnerResultPath = Join-Path ([IO.Path]::GetTempPath()) (
    'windvale-test-plan-' + [Guid]::NewGuid().ToString('N') + '.json')
try {
    $TestRunnerPlan = @(& pwsh -NoProfile -File $PowerShellTestRunner `
        -Owner verification-owner-stream -PlanOnly `
        -ResultPath $TestRunnerResultPath 2>&1)
    if ($LASTEXITCODE -ne 0 -or
        ($TestRunnerPlan -join "`n") -notmatch
            '(?m)^PLAN  owner=verification-owner-stream command=Test-Verification-Owner-Stream\.(?:cmd|sh) cases=6 shard=2 duration-profile=quick expected-seconds=15 maximum-seconds=300$') {
        throw 'The PowerShell test runner did not return its bounded focused owner plan.'
    }
    $TestRunnerResult = Get-Content -Raw -LiteralPath $TestRunnerResultPath |
        ConvertFrom-Json
    if ($TestRunnerResult.format -ne 'windvale-verification-run-result-1' -or
        $TestRunnerResult.host -notin @('Windows', 'Linux', 'macOS') -or
        $TestRunnerResult.outcome -ne 'planned' -or
        $TestRunnerResult.exitCode -ne 0 -or
        $TestRunnerResult.ownersPlanned -ne 1 -or
        $TestRunnerResult.casesPlanned -ne 6 -or
        $TestRunnerResult.expectedSeconds -ne 15 -or
        $TestRunnerResult.maximumSeconds -ne 300) {
        throw 'The PowerShell test runner did not write its structured plan result.'
    }
} finally {
    if ([IO.File]::Exists($TestRunnerResultPath)) {
        [IO.File]::Delete($TestRunnerResultPath)
    }
}
$MeasuredShardPlans = @($QualificationWorkPlan.Shards)
foreach ($MeasuredShardPlan in $MeasuredShardPlans) {
    $ShardPlanOutput = @(& pwsh -NoProfile -File $PowerShellTestRunner `
        -Shard $MeasuredShardPlan.Shard -PlanOnly 2>&1)
    $ShardPlanRows = @(
        $ShardPlanOutput | Where-Object { $_ -match '^PLAN  owner=' })
    $ExpectedShardSummary = (
        "Verification plan mode=shard:$($MeasuredShardPlan.Shard) " +
        "owners=$($MeasuredShardPlan.Owners) " +
        "cases=$($MeasuredShardPlan.Cases) " +
        "expected-seconds=$($MeasuredShardPlan.ExpectedSeconds) " +
        "maximum-seconds=$($MeasuredShardPlan.MaximumSeconds) ")
    if ($LASTEXITCODE -ne 0 -or
        $ShardPlanRows.Count -ne $MeasuredShardPlan.Owners -or
        !($ShardPlanOutput -join "`n").Contains(
            $ExpectedShardSummary, [StringComparison]::Ordinal)) {
        throw (
            "Qualification shard $($MeasuredShardPlan.Shard) did not expose " +
            'the measured runner plan.')
    }
}
$TailPlanResultPath = Join-Path ([IO.Path]::GetTempPath()) (
    'windvale-test-tail-plan-' + [Guid]::NewGuid().ToString('N') + '.json')
try {
    $TailPlan = @(& pwsh -NoProfile -File $PowerShellTestRunner `
        -Shard 2 -StartAtOwner language-1-production-admission-ingress `
        -PlanOnly -ResultPath $TailPlanResultPath 2>&1)
    $TailPlanLines = @($TailPlan | Where-Object { $_ -match '^PLAN  owner=' })
    $TailPlanResult = Get-Content -Raw -LiteralPath $TailPlanResultPath |
        ConvertFrom-Json
    if ($LASTEXITCODE -ne 0 -or $TailPlanLines.Count -lt 2 -or
        $TailPlanLines[0] -notmatch
            '^PLAN  owner=language-1-production-admission-ingress ' -or
        ($TailPlan -join "`n") -match
            '(?m)^PLAN  owner=language-1-foreign-catalog-producer ' -or
        $TailPlanResult.mode -ne
            'shard:2:start:language-1-production-admission-ingress' -or
        $TailPlanResult.outcome -ne 'planned' -or
        $TailPlanResult.ownersPlanned -ne $TailPlanLines.Count) {
        throw 'The PowerShell test runner did not return its resumable shard tail plan.'
    }
} finally {
    if ([IO.File]::Exists($TailPlanResultPath)) {
        [IO.File]::Delete($TailPlanResultPath)
    }
}
$MissingShardResult = @(& pwsh -NoProfile -File $PowerShellTestRunner `
    -StartAtOwner verification-owner-stream -PlanOnly 2>&1)
if ($LASTEXITCODE -ne 64 -or
    ($MissingShardResult -join "`n") -notmatch
        '(?m)^-StartAtOwner requires one explicit -Shard\.$') {
    throw 'The PowerShell test runner accepted a start owner without a shard.'
}
$WrongShardStartResult = @(& pwsh -NoProfile -File $PowerShellTestRunner `
    -Shard 3 -StartAtOwner verification-owner-stream -PlanOnly 2>&1)
if ($LASTEXITCODE -ne 64 -or
    ($WrongShardStartResult -join "`n") -notmatch
        "(?m)^Unknown start owner 'verification-owner-stream' in qualification shard 3\.$") {
    throw 'The PowerShell test runner accepted a start owner from another shard.'
}
$UnknownOwnerResult = @(& pwsh -NoProfile -File $PowerShellTestRunner `
    -Owner windvale-unknown-owner -PlanOnly 2>&1)
if ($LASTEXITCODE -ne 64 -or
    ($UnknownOwnerResult -join "`n") -notmatch
        '(?m)^Unknown verification owner: windvale-unknown-owner$') {
    throw 'The PowerShell test runner did not reject an unknown owner with usage status 64.'
}
$UnapprovedLongRun = @(& pwsh -NoProfile -File $PowerShellTestRunner 2>&1)
if ($LASTEXITCODE -ne 64 -or
    ($UnapprovedLongRun -join "`n") -notmatch
        '(?m)^Selected plan expects [0-9]+ seconds, which exceeds the 600-second local development budget\.') {
    throw 'The PowerShell test runner did not refuse an unapproved long run.'
}
$FailedOwnerResultPath = Join-Path ([IO.Path]::GetTempPath()) (
    'windvale-test-failure-' + [Guid]::NewGuid().ToString('N') + '.json')
$PriorFailureFixture = $env:WINDVALE_VERIFICATION_OWNER_FAILURE_FIXTURE
try {
    $env:WINDVALE_VERIFICATION_OWNER_FAILURE_FIXTURE = '1'
    $FailedOwnerOutput = @(& pwsh -NoProfile -File $PowerShellTestRunner `
        -Owner verification-owner-stream -ResultPath $FailedOwnerResultPath `
        2>&1)
    $FailedOwnerExitCode = $LASTEXITCODE
    $FailedOwnerResult = Get-Content -Raw -LiteralPath $FailedOwnerResultPath |
        ConvertFrom-Json
    if ($FailedOwnerExitCode -ne 1 -or
        ($FailedOwnerOutput -join "`n") -notmatch
            '(?m)^verification owner forced failure$' -or
        ($FailedOwnerOutput -join "`n") -notmatch
            '(?m)^FAIL  suite verification-owner-stream outcome=test-failed ' -or
        $FailedOwnerResult.outcome -ne 'test-failed' -or
        $FailedOwnerResult.exitCode -ne 1 -or
        @($FailedOwnerResult.owners).Count -ne 1 -or
        $FailedOwnerResult.owners[0].outcome -ne 'test-failed' -or
        $FailedOwnerResult.owners[0].exitCode -ne 1 -or
        $FailedOwnerResult.owners[0].detail -ne 'Native command exited 7.') {
        throw 'The PowerShell test runner did not propagate a streamed owner failure.'
    }
} finally {
    if ($null -eq $PriorFailureFixture) {
        Remove-Item Env:WINDVALE_VERIFICATION_OWNER_FAILURE_FIXTURE `
            -ErrorAction SilentlyContinue
    } else {
        $env:WINDVALE_VERIFICATION_OWNER_FAILURE_FIXTURE = $PriorFailureFixture
    }
    if ([IO.File]::Exists($FailedOwnerResultPath)) {
        [IO.File]::Delete($FailedOwnerResultPath)
    }
}

$TimingAnalyzer = Join-Path $PSScriptRoot `
    'Update-Verification-Timing-History.ps1'
$TimingAnalysisRoot = Join-Path ([IO.Path]::GetTempPath()) (
    'windvale-timing-analysis-' + [Guid]::NewGuid().ToString('N'))
$TimingInputRoot = Join-Path $TimingAnalysisRoot 'input'
$TimingHistoryPath = Join-Path $TimingAnalysisRoot 'history.json'
$TimingAnalysisPath = Join-Path $TimingAnalysisRoot 'analysis.json'
$TimingUtf8 = [Text.UTF8Encoding]::new($false, $true)
try {
    $null = [IO.Directory]::CreateDirectory($TimingInputRoot)
    foreach ($HostName in @('Windows', 'Linux')) {
        foreach ($SampleIndex in 1..5) {
            $StartedUtc = [DateTime]::new(
                2026, 1, $SampleIndex, 0, 0, 0,
                [DateTimeKind]::Utc).ToString('O')
            $TimingReport = if ($HostName -eq 'Linux' -and $SampleIndex -eq 5) {
                [ordered]@{
                    format = 'windvale-native-changed-verification-timing-2'
                    host = $HostName
                    startedUtc = $StartedUtc
                    entries = @(
                        [ordered]@{
                            name = 'wvb-inspector-reconstruction'
                            status = 'executed'
                            outcome = 'passed'
                            elapsedMilliseconds = 5000 + $SampleIndex
                        },
                        [ordered]@{
                            name = 'seed'
                            status = 'cached'
                            outcome = 'passed'
                            elapsedMilliseconds = 1
                        },
                        [ordered]@{
                            name = 'verification-plan'
                            status = 'executed'
                            elapsedMilliseconds = 1
                        }
                    )
                }
            } else {
                [ordered]@{
                    format = 'windvale-verification-run-result-1'
                    host = $HostName
                    startedUtc = $StartedUtc
                    owners = @([ordered]@{
                        name = 'wvb-inspector-reconstruction'
                        outcome = 'passed'
                        elapsedMilliseconds = 5000 + $SampleIndex
                    })
                }
            }
            $TimingReportPath = Join-Path $TimingInputRoot (
                "$($HostName.ToLowerInvariant())-$SampleIndex.json")
            [IO.File]::WriteAllText(
                $TimingReportPath,
                (($TimingReport | ConvertTo-Json -Depth 6 -Compress) + "`n"),
                $TimingUtf8)
        }
    }
    [IO.File]::Copy(
        (Join-Path $TimingInputRoot 'windows-1.json'),
        (Join-Path $TimingInputRoot 'duplicate.json'))
    [IO.File]::WriteAllText(
        (Join-Path $TimingInputRoot 'empty.json'),
        (([ordered]@{
            format = 'windvale-native-changed-verification-timing-2'
            host = 'Windows'
            startedUtc = '2026-01-10T00:00:00.0000000Z'
            entries = @()
        } | ConvertTo-Json -Depth 4 -Compress) + "`n"),
        $TimingUtf8)
    $TimingAnalysisRun = @(& pwsh -NoProfile -File $TimingAnalyzer `
        -InputPath $TimingInputRoot -HistoryPath $TimingHistoryPath `
        -AnalysisPath $TimingAnalysisPath 2>&1)
    if ($LASTEXITCODE -ne 0 -or
        ($TimingAnalysisRun -join "`n") -notmatch
            '(?m)^verification timing analysis status=Passed ') {
        throw (
            'The verification timing analyzer did not accept bounded dual-host evidence: ' +
            ($TimingAnalysisRun -join "`n"))
    }
    $TimingHistory = Get-Content -Raw -LiteralPath $TimingHistoryPath |
        ConvertFrom-Json -Depth 12
    $TimingAnalysis = Get-Content -Raw -LiteralPath $TimingAnalysisPath |
        ConvertFrom-Json -Depth 12
    $OwnerTimingAnalysis = @($TimingAnalysis.owners | Where-Object {
        $_.owner -eq 'wvb-inspector-reconstruction'
    })
    if ($TimingHistory.format -ne 'windvale-verification-timing-history-1' -or
        @($TimingHistory.samples).Count -ne 10 -or
        $TimingAnalysis.format -ne 'windvale-verification-timing-analysis-1' -or
        $TimingAnalysis.reportsAccepted -ne 12 -or
        $TimingAnalysis.samplesAdded -ne 10 -or
        $OwnerTimingAnalysis.Count -ne 1 -or
        $OwnerTimingAnalysis[0].windowsPassingSamples -ne 5 -or
        $OwnerTimingAnalysis[0].linuxPassingSamples -ne 5 -or
        $OwnerTimingAnalysis[0].action -ne 'downgrade' -or
        $OwnerTimingAnalysis[0].recommendedProfile -ne 'quick') {
        throw 'The verification timing analyzer did not produce the conservative profile recommendation.'
    }
    $HistoryDigestBeforeInvalidInput = (
        Get-FileHash -LiteralPath $TimingHistoryPath -Algorithm SHA256).Hash
    $InvalidTimingReportPath = Join-Path $TimingInputRoot 'invalid.json'
    $InvalidTimingReport = [ordered]@{
        format = 'windvale-verification-run-result-1'
        host = 'Linux'
        startedUtc = '2026-01-20T00:00:00.0000000Z'
        owners = @([ordered]@{
            name = 'wvb-inspector-reconstruction'
            outcome = 'passed'
            elapsedMilliseconds = 3700001
        })
    }
    [IO.File]::WriteAllText(
        $InvalidTimingReportPath,
        (($InvalidTimingReport | ConvertTo-Json -Depth 6 -Compress) + "`n"),
        $TimingUtf8)
    $InvalidTimingRun = @(& pwsh -NoProfile -File $TimingAnalyzer `
        -InputPath $TimingInputRoot -HistoryPath $TimingHistoryPath `
        -AnalysisPath $TimingAnalysisPath 2>&1)
    $HistoryDigestAfterInvalidInput = (
        Get-FileHash -LiteralPath $TimingHistoryPath -Algorithm SHA256).Hash
    if ($LASTEXITCODE -eq 0 -or
        ($InvalidTimingRun -join "`n") -notmatch
            'Observed elapsed milliseconds is not an integer in the permitted range' -or
        $HistoryDigestAfterInvalidInput -cne $HistoryDigestBeforeInvalidInput) {
        throw 'The verification timing analyzer did not reject malformed evidence atomically.'
    }
} finally {
    if ([IO.Directory]::Exists($TimingAnalysisRoot)) {
        [IO.Directory]::Delete($TimingAnalysisRoot, $true)
    }
}

$CompilerDevelopmentWindows = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Compiler-Reconstruction.cmd')
$CompilerDevelopmentLinux = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Compiler-Reconstruction.sh')
$Language1FrontDoorWindows = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Front-Door.cmd')
$Language1FrontDoorLinux = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Front-Door.sh')
$GenericNominalDevelopmentRunner = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Generic-Nominal-Development-Bundle.mjs')
$GenericNominalDevelopmentProject = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Development-Bundle.wvproj')
$GenericNominalDevelopmentRoot = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tests/Fixtures/Language-1.0/Generic-Nominal-Development-Bundle-Self-Test.wv')
$ChangedVerification = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Verify/Verify-Changed.ps1')
$ResultCacheImplementation = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Verification-Owner-Result-Cache.mjs')
foreach ($Fragment in @(
    '$Plan.Scope -eq ''development'' -and !$NoResultCache',
    '''windvale-verification-owner-action-2''',
    'ownerContractSha256',
    '''probe''',
    '''candidates''',
    '''changes''',
    '''confirm''',
    '''publish''',
    '''StateChanged''',
    'source-state=Exact',
    'source-state=Compatible',
    '$CompatibleResultCacheBarrierPaths'
)) {
    if (!$ChangedVerification.Contains($Fragment, [StringComparison]::Ordinal)) {
        throw "Changed-file result-cache dispatch is missing '$Fragment'."
    }
}
foreach ($Fragment in @(
    '$Coordinator = Join-Path $PSScriptRoot ''Invoke-WindvaleTests.ps1''',
    '$OwnerArguments = @(''-Owner'', $Suite)',
    '$OwnerArguments += ''-AllowLongRun''',
    '& pwsh -NoProfile -File $OwnerCommand @OwnerArguments',
    '$LOCAL_DEVELOPMENT_BUDGET_SECONDS = 600',
    'DatabaseStorageDevelopmentExpectedSeconds -gt',
    '''--development-target-set''',
    '$AllowIncompleteInfrastructure',
    '$PlanVerificationInClassification',
    '$GitHubVerificationOnLinux',
    '$env:GITHUB_ACTIONS -ne ''true''',
    '$env:RUNNER_OS -ne ''Windows''',
    '!$PlanVerificationInClassification)',
    '!$GitHubVerificationOnLinux)',
    '''windvale-native-changed-verification-timing-2''',
    'host = Get-VerificationHostName',
    'startedUtc = $VerificationStartedUtc.ToString(''O'')',
    '''verification-incomplete'''
)) {
    if (!$ChangedVerification.Contains($Fragment, [StringComparison]::Ordinal)) {
        throw "Changed-file PowerShell test dispatch is missing '$Fragment'."
    }
}
foreach ($Fragment in @(
    "const CACHE_FAMILY = 'owner-result-v1'",
    "const STATE_RECORD_FORMAT = 'windvale-verification-owner-state-record-1'",
    "const CANDIDATE_FORMAT = 'windvale-verification-owner-candidates-1'",
    "const CHANGED_PATH_FORMAT = 'windvale-verification-owner-changed-paths-1'",
    'const MAX_STATE_DIRECTORIES = 16',
    'const MAX_COMPATIBLE_CANDIDATES = 15',
    'const MAX_RESULTS_PER_STATE = 512',
    'const MAX_RESULT_BYTES = 16 * 1024',
    'await Measureˉsourceˉsentinel(resolve(Repositoryˉinput))',
    'Listˉverificationˉresultˉcandidates',
    'Getˉverificationˉchangedˉpaths',
    'Confirmˉverificationˉsourceˉstate',
    'const Sentinelˉbefore = await Measureˉsourceˉsentinel',
    'await Ensureˉstateˉrecord',
    'return ''StateChanged''',
    'await rm(Temporary, { force: true })'
)) {
    if (!$ResultCacheImplementation.Contains($Fragment, [StringComparison]::Ordinal)) {
        throw "Verification result-cache implementation is missing '$Fragment'."
    }
}
foreach ($Contract in @(
    @{
        Name = 'Windows compiler development owner'
        Text = $CompilerDevelopmentWindows
        Required = @(
            'Usage: Tools\Native\Test-Compiler-Reconstruction.cmd [--development]',
            'Verify-Compiler-Convergence.cmd',
            'retained candidate inventory',
            'Function-Only.wv',
            'Build-Current-Wvb.cmd',
            'Verify-Wvb.cmd',
            'retained-to-current compiler differential smoke'
        )
    },
    @{
        Name = 'Linux compiler development owner'
        Text = $CompilerDevelopmentLinux
        Required = @(
            'Test-Compiler-Reconstruction.sh [--development]',
            'Verify-Compiler-Convergence.sh',
            'retained candidate inventory',
            'Function-Only.wv',
            'Build-Current-Wvb.sh',
            'Verify-Wvb.sh',
            'retained-to-current compiler differential smoke'
        )
    },
    @{
        Name = 'changed-file compiler development dispatch'
        Text = $ChangedVerification
        Required = @(
            '$Suite -eq ''compiler-reconstruction''',
            '$Plan.Scope -eq ''development''',
            'mode=development-smoke',
            '$OwnerArguments = @(''--development'')',
            '& $OwnerCommand @OwnerArguments'
        )
    },
    @{
        Name = 'Windows Language 1 front-door development owner'
        Text = $Language1FrontDoorWindows
        Required = @(
            'Test-Language-1.0-Front-Door.cmd [--development]',
            'if "%Development%"=="1"',
            'phase=value-front-end item=3/13',
            'Test-Language-1.0-Front-Door-Development.mjs',
            'status=Passed cases=492'
        )
    },
    @{
        Name = 'Linux Language 1 front-door development owner'
        Text = $Language1FrontDoorLinux
        Required = @(
            'Test-Language-1.0-Front-Door.sh [--development]',
            'if [[ $development == true ]]',
            'phase=value-front-end item=3/13',
            'Test-Language-1.0-Front-Door-Development.mjs',
            'status=Passed cases=492'
        )
    },
    @{
        Name = 'changed-file Language 1 front-door development dispatch'
        Text = $ChangedVerification
        Required = @(
            '$Suite -eq ''language-1-front-door''',
            '$Plan.Scope -eq ''development''',
            'mode=development-front-end cases=$($NativePlan.Language1FrontDoorDevelopmentCaseCount)',
            "'--development-target'",
            '$OwnerArguments = @(''--development'')',
            '& $OwnerCommand @OwnerArguments'
        )
    }
)) {
    foreach ($Fragment in $Contract.Required) {
        if (!$Contract.Text.Contains($Fragment, [StringComparison]::Ordinal)) {
            throw "$($Contract.Name) is missing '$Fragment'."
        }
    }
}

$Language1FrontDoorDevelopmentPlan = & $NativePlanner -ChangedPath (
    'Tools/Native/Test-Language-1.0-Front-Door.cmd') -PassThru -Quiet
if (!$Language1FrontDoorDevelopmentPlan.UseLanguage1FrontDoorDevelopment -or
    $Language1FrontDoorDevelopmentPlan.Suites.Count -ne 1 -or
    $Language1FrontDoorDevelopmentPlan.Suites[0] -ne 'language-1-front-door' -or
    $Language1FrontDoorDevelopmentPlan.ExpectedSeconds -ne 330 -or
    $Language1FrontDoorDevelopmentPlan.MaximumSeconds -ne 600 -or
    $Language1FrontDoorDevelopmentPlan.Language1FrontDoorDevelopmentCaseCount -ne 329 -or
    $Language1FrontDoorDevelopmentPlan.Language1FrontDoorDevelopmentExpectedSeconds -ne 330 -or
    $Language1FrontDoorDevelopmentPlan.Language1FrontDoorDevelopmentMaximumSeconds -ne 600) {
    throw 'The Language 1 front-door development checkpoint plan differs.'
}

foreach ($BorrowPath in @(
    'Compiler/Windvale/Source-Wvb-Foundation-Borrow-Plan.wv',
    'Projects/Tests/Windvale-Native-Test-Foundation-Value-Borrow-Plan.wvproj',
    'Tests/Fixtures/Language-1.0/Foundation-Value-Borrow-Plan-Self-Test.wv'
)) {
    $BorrowPlan = & $NativePlanner -ChangedPath $BorrowPath -PassThru -Quiet
    if (!$BorrowPlan.UseFoundationBorrowPlanDevelopment -or
        $BorrowPlan.ExpectedSeconds -ne 30 -or $BorrowPlan.MaximumSeconds -ne 600 -or
        $BorrowPlan.Suites.Count -ne 1 -or $BorrowPlan.Gaps.Count -ne 0) {
        throw "The focused Foundation borrow plan differs for '$BorrowPath'."
    }
}
foreach ($OtherBorrowPath in @(
    'Compiler/Windvale/Source-Wvb-Core.wv',
    'Tests/Fixtures/Language-1.0/Foundation-Value-Payload-Borrow-Wvb.wv',
    'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs',
    'Tools/Native/Development-Command-Core.mjs'
)) {
    $BorrowPlan = & $NativePlanner -ChangedPath @(
        'Compiler/Windvale/Source-Wvb-Foundation-Borrow-Plan.wv', $OtherBorrowPath
    ) -PassThru -Quiet
    if ($BorrowPlan.UseFoundationBorrowPlanDevelopment) {
        throw "Foundation planning hid integration changes in '$OtherBorrowPath'."
    }
}

foreach ($DirectoryPath in @(
    'Projects/Tests/Windvale-Native-Test-Wvb-Typed-Directories.wvproj',
    'Tests/Fixtures/Source-Wvb/Typed-Directories-Self-Test.wv'
)) {
    $DirectoryPlan = & $NativePlanner -ChangedPath $DirectoryPath -PassThru -Quiet
    if (!$DirectoryPlan.UseFoundationBorrowDirectoryDevelopment -or
        $DirectoryPlan.UseFoundationBorrowPlanDevelopment -or
        $DirectoryPlan.ExpectedSeconds -ne 30 -or $DirectoryPlan.MaximumSeconds -ne 600 -or
        $DirectoryPlan.Suites.Count -ne 1 -or $DirectoryPlan.Gaps.Count -ne 0) {
        throw "The focused WVB typed-directory plan differs for '$DirectoryPath'."
    }
}
foreach ($OtherDirectoryPath in @(
    'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Typed-Directories.wv',
    'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Executable-Core.wv',
    'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Semantic-Core.wv',
    'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs',
    'Tests/Fixtures/Language-1.0/Foundation-Value-Borrow-Plan-Self-Test.wv'
)) {
    $DirectoryPlan = & $NativePlanner -ChangedPath @(
        'Tests/Fixtures/Source-Wvb/Typed-Directories-Self-Test.wv', $OtherDirectoryPath
    ) -PassThru -Quiet
    if ($DirectoryPlan.UseFoundationBorrowDirectoryDevelopment -or
        $DirectoryPlan.UseFoundationBorrowPlanDevelopment) {
        throw "WVB directory selection hid integration changes in '$OtherDirectoryPath'."
    }
}

foreach ($OwnerPath in @(
    'Projects/Tests/Windvale-Native-Test-Foundation-Owner-Flow.wvproj',
    'Tests/Fixtures/Source-Wvb/Foundation-Borrow-Calls-Self-Test.wv',
    'Tests/Fixtures/Source-Wvb/Foundation-Borrow-Metadata-Self-Test.wv',
    'Tests/Fixtures/Source-Wvb/Foundation-Borrow-Stack-Self-Test.wv',
    'Tests/Fixtures/Source-Wvb/Foundation-Borrow-Lifetime-Self-Test.wv',
    'Tests/Fixtures/Source-Wvb/Foundation-Owner-Flow-Self-Test.wv'
)) {
    $OwnerPlan = & $NativePlanner -ChangedPath $OwnerPath -PassThru -Quiet
    if (!$OwnerPlan.UseFoundationBorrowOwnerDevelopment -or
        $OwnerPlan.UseFoundationBorrowPlanDevelopment -or
        $OwnerPlan.UseFoundationBorrowDirectoryDevelopment -or
        $OwnerPlan.ExpectedSeconds -ne 180 -or $OwnerPlan.MaximumSeconds -ne 600 -or
        $OwnerPlan.Suites.Count -ne 1 -or $OwnerPlan.Gaps.Count -ne 0) {
        throw "The focused Foundation owner-flow plan differs for '$OwnerPath'."
    }
}
foreach ($PublisherPath in @(
    'Projects/Tools/Windvale-Wvb-Publisher.wvproj',
    'Tools/Windvale.Publish/Wvb-Publisher-Tool.wv',
    'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Executable-Core.wv',
    'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Foundation-Owner-Flow.wv'
)) {
    $PublisherPlan = & $NativePlanner -ChangedPath @(
        'Projects/Tools/Windvale-Wvb-Publisher.wvproj', $PublisherPath
    ) -PassThru -Quiet
    if (!$PublisherPlan.UsePublisherCurrentSourceDevelopment -or
        $PublisherPlan.Suites -cnotcontains 'hosted-verifier-publisher-files' -or
        $PublisherPlan.Gaps.Count -ne 0) {
        throw "The current-source publisher selection differs for '$PublisherPath'."
    }
}
foreach ($OtherPublisherPath in @(
    'Tools/Native/Test-Hosted-Verifier-Publisher-File-Pipeline.cmd',
    'Tools/Native/Test-Hosted-Verifier-Publisher-File-Pipeline.sh',
    'Tools/Native/Construct-Hosted-Verifier-Publisher.cmd',
    'Tools/Windvale.Publish/Wvb-Publication-Transaction.wv',
    'Foundation/Byte-Construction.wv',
    'Artifacts/Native-Wvb-Publisher-Candidate/SHA256SUMS'
)) {
    $PublisherPlan = & $NativePlanner -ChangedPath @(
        'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Executable-Core.wv', $OtherPublisherPath
    ) -PassThru -Quiet
    if ($PublisherPlan.UsePublisherCurrentSourceDevelopment) {
        throw "Current-source publisher selection hid frozen reconstruction changes in '$OtherPublisherPath'."
    }
}
foreach ($OtherOwnerPath in @(
    'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Foundation-Owner-Flow.wv',
    'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Typed-Directories.wv',
    'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Executable-Core.wv',
    'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs',
    'Tests/Fixtures/Source-Wvb/Typed-Directories-Self-Test.wv'
)) {
    $OwnerPlan = & $NativePlanner -ChangedPath @(
        'Tests/Fixtures/Source-Wvb/Foundation-Owner-Flow-Self-Test.wv', $OtherOwnerPath
    ) -PassThru -Quiet
    if ($OwnerPlan.UseFoundationBorrowOwnerDevelopment -or
        $OwnerPlan.UseFoundationBorrowPlanDevelopment -or
        $OwnerPlan.UseFoundationBorrowDirectoryDevelopment) {
        throw "Foundation owner selection hid integration changes in '$OtherOwnerPath'."
    }
}

$FrontEndRunner = Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Front-Door-Development.mjs'
& node $FrontEndRunner --check-runner
if ($LASTEXITCODE -ne 0) { throw 'Front-end development runner fault checks failed.' }
foreach ($Selection in @(
    @{ Paths = @('Projects/Tests/Windvale-Native-Test-Language-1-Generic-Declarations.wvproj'); Target = 'generic-declarations'; Cases = 254; Seconds = 20 },
    @{ Paths = @('Tests/Fixtures/Language-1.0/Generic-Call-Front-End-Self-Test.wv'); Target = 'generic-calls'; Cases = 252; Seconds = 150 },
    @{ Paths = @('Tests/Fixtures/Language-1.0/Generic-Call-Front-End-Self-Test.wv', 'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Declarations.wvproj'); Target = 'generic-declarations+generic-calls'; Cases = 255; Seconds = 170 },
    @{ Paths = @('Libraries/Foundation/Values/Option.wv'); Target = 'all'; Cases = 329; Seconds = 330 },
    @{ Paths = @('Tools/Native/Test-Language-1.0-Front-Door-Development.mjs'); Target = 'all'; Cases = 329; Seconds = 330 }
)) {
    $Selected = & $NativePlanner -ChangedPath $Selection.Paths -PassThru -Quiet
    if ($Selected.Language1FrontDoorDevelopmentTarget -cne $Selection.Target -or
        $Selected.Language1FrontDoorDevelopmentCaseCount -ne $Selection.Cases -or
        $Selected.Language1FrontDoorDevelopmentExpectedSeconds -ne $Selection.Seconds -or
        $Selected.Gaps.Count -ne 0) {
        throw "Front-end product selection differs for '$($Selection.Paths -join ',')'."
    }
}

foreach ($Fragment in @(
    'Windvale-Native-Test-Language-1-Generic-Nominal-Development-Bundle.wvproj',
    "Package, ['6', Wvb, Application, '--development-cache']",
    'The generic nominal development bundle wrote output.',
    'Execution.Code !== 42',
    'native generic nominal type binding status=Passed cases=59 result=42',
    'native generic nominal type layout status=Passed cases=21 result=42',
    'native generic nominal type materialization status=Passed cases=28 result=42'
)) {
    if (!$GenericNominalDevelopmentRunner.Contains(
            $Fragment, [StringComparison]::Ordinal)) {
        throw "Generic nominal development runner is missing '$Fragment'."
    }
}
foreach ($Fragment in @(
    'Generic-Nominal-Type-Binding-Self-Test.wv',
    'Generic-Nominal-Type-Layout-Self-Test.wv',
    'Generic-Nominal-Type-Materialization-Self-Test.wv',
    'emit wvb'
)) {
    if (!$GenericNominalDevelopmentProject.Contains(
            $Fragment, [StringComparison]::Ordinal)) {
        throw "Generic nominal development project is missing '$Fragment'."
    }
}
foreach ($Fragment in @(
    'Binding.Main()',
    'Layout.Main()',
    'Materialization.Main()',
    'return 64 + Layoutˉresult;',
    'return 96 + Materializationˉresult;',
    'return 42;'
)) {
    if (!$GenericNominalDevelopmentRoot.Contains(
            $Fragment, [StringComparison]::Ordinal)) {
        throw "Generic nominal development root is missing '$Fragment'."
    }
}
$GenericNominalWrapperContracts = @(
    @{
        Stem = 'Test-Generic-Nominal-Type-Binding'
        Selector = 'type-binding'
    },
    @{
        Stem = 'Test-Generic-Nominal-Type-Layout'
        Selector = 'type-layout'
    },
    @{
        Stem = 'Test-Generic-Nominal-Type-Materialization'
        Selector = 'type-materialization'
    }
)
foreach ($Contract in $GenericNominalWrapperContracts) {
    foreach ($Extension in @('cmd', 'sh')) {
        $Wrapper = Get-Content -Raw -LiteralPath (Join-Path $RepositoryRoot (
            "Tools/Native/$($Contract.Stem).$Extension"))
        foreach ($Fragment in @(
            '[--development]',
            'Test-Generic-Nominal-Development-Bundle.mjs',
            $Contract.Selector,
            "$($Contract.Stem).mjs"
        )) {
            if (!$Wrapper.Contains($Fragment, [StringComparison]::Ordinal)) {
                throw (
                    "$($Contract.Stem).$Extension is missing '$Fragment'.")
            }
        }
    }
}
foreach ($Fragment in @(
    '$NativePlan.UseGenericNominalDevelopmentBundle',
    'mode=development-bundle',
    'bundle-cases=108',
    '$OwnerArguments = @(''--development'')'
)) {
    if (!$ChangedVerification.Contains($Fragment, [StringComparison]::Ordinal)) {
        throw "Changed-file generic nominal dispatch is missing '$Fragment'."
    }
}

$GenericNominalDevelopmentPlan = & $NativePlanner -ChangedPath (
    'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Development-Bundle.wvproj') `
    -PassThru -Quiet
$ExpectedGenericNominalDevelopmentSuites = @(
    'generic-nominal-type-binding',
    'generic-nominal-type-layout',
    'generic-nominal-type-materialization'
)
if (!$GenericNominalDevelopmentPlan.UseGenericNominalDevelopmentBundle -or
    ![Linq.Enumerable]::SequenceEqual(
        [string[]]$GenericNominalDevelopmentPlan.Suites,
        [string[]]$ExpectedGenericNominalDevelopmentSuites) -or
    $GenericNominalDevelopmentPlan.ExpectedSeconds -ne 330 -or
    $GenericNominalDevelopmentPlan.MaximumSeconds -ne 600 -or
    $GenericNominalDevelopmentPlan.GenericNominalDevelopmentBundleSelectedOwnerCount -ne 3 -or
    $GenericNominalDevelopmentPlan.GenericNominalDevelopmentBundleCaseCount -ne 108) {
    throw 'The three-owner generic nominal development bundle plan differs.'
}
$SingleGenericNominalDevelopmentPlan = & $NativePlanner -ChangedPath (
    'Tests/Fixtures/Language-1.0/Generic-Nominal-Type-Binding-Self-Test.wv') `
    -PassThru -Quiet
if ($SingleGenericNominalDevelopmentPlan.UseGenericNominalDevelopmentBundle -or
    $SingleGenericNominalDevelopmentPlan.Suites.Count -ne 1 -or
    $SingleGenericNominalDevelopmentPlan.Suites[0] -ne
        'generic-nominal-type-binding' -or
    $SingleGenericNominalDevelopmentPlan.ExpectedSeconds -ne 300 -or
    $SingleGenericNominalDevelopmentPlan.MaximumSeconds -ne 600) {
    throw 'A single generic nominal owner did not retain focused development.'
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
            '$OwnerArguments = @(''--compiler-only'')',
            '& $OwnerCommand @OwnerArguments'
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
    "group: verify-`${{ github.workflow }}-`${{ github.ref }}-`${{ github.event_name == 'workflow_dispatch' && 'qualification' || 'automatic' }}",
    "cancel-in-progress: `${{ github.event_name != 'workflow_dispatch' }}",
    'queue: single',
    'windows_required: ${{ steps.host-scope.outputs.windows_required }}',
    'qualification_shard: ${{ steps.qualification-selection.outputs.shard }}',
    'qualification_start_owner: ${{ steps.qualification-selection.outputs.start_owner }}',
    'qualification_shards: ${{ steps.qualification-selection.outputs.shards }}',
    'qualification_full: ${{ steps.qualification-selection.outputs.full }}',
    'name: Select automatic Windows host',
    'name: Validate qualification selection',
    "`$_ -match '(?i)(?:^|[/_.-])(?:Windows|Win32)(?:`$|[/_.-])'",
    "`$_ -match '(?i)\.(?:cmd|bat|ps1|exe|dll|pdb)`$'",
    "if: `${{ needs.classify-changes.outputs.scope == 'development' && needs.classify-changes.outputs.windows_required == 'true' }}",
    'WINDOWS_REQUIRED: ${{ needs.classify-changes.outputs.windows_required }}',
    './Tools/Verify/Verify-Verification-Plan.ps1',
    'uses: actions/cache/restore@27d5ce7f107fe9357f9df03efb73ab90386fccae # v5.0.5',
    'uses: actions/cache/save@27d5ce7f107fe9357f9df03efb73ab90386fccae # v5.0.5',
    "if: `${{ always() && steps.native-development-cache.outputs.cache-hit != 'true' }}",
    'if ([string]::IsNullOrWhiteSpace($env:BASE_SHA) -or',
    'git diff --check HEAD^ HEAD --',
    '-AllowIncompleteInfrastructure -PlanVerificationInClassification -TimingReportPath $env:VERIFICATION_TIMING_REPORT',
    '-AllowIncompleteInfrastructure -PlanVerificationInClassification -GitHubVerificationOnLinux -TimingReportPath $env:VERIFICATION_TIMING_REPORT',
    'Tools/Verify/Update-Verification-Timing-History.ps1 -InputPath $env:VERIFICATION_TIMING_REPORT -HistoryPath $env:VERIFICATION_TIMING_HISTORY -AnalysisPath $env:VERIFICATION_TIMING_ANALYSIS',
    '${{ runner.temp }}/windvale-development-timing-analysis.json',
    'uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2',
    'shard: ${{ fromJSON(needs.classify-changes.outputs.qualification_shards) }}',
    'QUALIFICATION_START_OWNER: ${{ needs.classify-changes.outputs.qualification_start_owner }}',
    '$Arguments += @(''-StartAtOwner'', $env:QUALIFICATION_START_OWNER)',
    "name: `${{ needs.classify-changes.outputs.scope == 'qualification' && needs.classify-changes.outputs.qualification_full != 'true' && 'Partial qualification gate' || 'Verification gate' }}"
)
foreach ($Fragment in $RequiredWorkflowFragments) {
    if (!$GitHubVerificationWorkflow.Contains($Fragment, [StringComparison]::Ordinal)) {
        throw "The GitHub verification workflow is missing '$Fragment'."
    }
}
if ([regex]::Matches(
        $GitHubVerificationWorkflow,
        [regex]::Escape('& pwsh @Arguments')).Count -ne 2) {
    throw 'The GitHub qualification workflow does not use the PowerShell test runner on both hosts.'
}
if ($GitHubVerificationWorkflow.Contains(
        'windows-documentation:', [StringComparison]::Ordinal)) {
    throw 'The GitHub verification workflow still duplicates documentation on Windows.'
}
foreach ($JobName in @('windows-development', 'linux-development')) {
    $JobMatch = [regex]::Match(
        $GitHubVerificationWorkflow,
        "(?ms)^  ${JobName}:.*?(?=^  [a-z0-9-]+:|\z)")
    if (!$JobMatch.Success -or
        !$JobMatch.Value.Contains('timeout-minutes: 15', [StringComparison]::Ordinal)) {
        throw "The GitHub $JobName job does not have the 15-minute development bound."
    }
}
if ([regex]::Matches(
        $GitHubVerificationWorkflow,
        [regex]::Escape(
            'shard: ${{ fromJSON(needs.classify-changes.outputs.qualification_shards) }}')).Count -ne 2) {
    throw 'The GitHub verification workflow must consume the validated shard matrix on both hosts.'
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

$DatabaseCasePlan = Join-Path $RepositoryRoot `
    'Tests/Native/Database-Storage-Development-Cases.txt'
$DatabaseCaseLines = @(Get-Content -LiteralPath $DatabaseCasePlan)
if ($DatabaseCaseLines.Count -ne 54 -or
    $DatabaseCaseLines[0] -ne
        'windvale-database-storage-development-cases 3') {
    throw 'The database storage development-case inventory differs.'
}
$DatabaseCaseNames = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
foreach ($Line in @($DatabaseCaseLines | Select-Object -Skip 1)) {
    $Fields = $Line.Split('|')
    $Selectors = if ($Fields.Count -in @(3, 4)) { @($Fields[2].Split(',')) } else { @() }
    $Bundle = if ($Fields.Count -eq 4) { $Fields[3] } else { '-' }
    if ($Fields.Count -notin @(3, 4) -or
        $Fields[0] -notmatch '^[A-Z][A-Za-z0-9]*$' -or
        $Fields[1] -notmatch '^(?:portable|hosted)$' -or
        $Selectors.Count -eq 0 -or
        @($Selectors | Where-Object {
            $_ -notmatch '^[a-z0-9][a-z0-9-]*$'
        }).Count -ne 0 -or
        @($Selectors | Sort-Object -Unique).Count -ne $Selectors.Count -or
        ($Bundle -ne '-' -and
            ($Bundle -notmatch '^[A-Z][A-Za-z0-9]*$' -or
             $Fields[1] -ne 'portable')) -or
        !$DatabaseCaseNames.Add($Fields[0])) {
        throw "Invalid or duplicate database development case: $Line"
    }
}
$DatabaseCasePlanner = Join-Path $RepositoryRoot `
    'Tools/Native/Plan-Database-Storage-Development.mjs'
$DatabaseCasePlannerText = Get-Content -Raw -LiteralPath $DatabaseCasePlanner
if (!$DatabaseCasePlannerText.Contains(
        'Database-Storage-Development-Cases.txt',
        [StringComparison]::Ordinal)) {
    throw 'The database development planner does not consume its case inventory.'
}
$DatabaseAllPlan = (& node $DatabaseCasePlanner all).Trim()
if ($LASTEXITCODE -ne 0) {
    throw 'The database development all-case plan failed.'
}
$DatabaseAllFields = $DatabaseAllPlan.Split('|')
if ($DatabaseAllFields.Count -ne 7 -or
    $DatabaseAllFields[0] -ne 'windvale-database-storage-development-plan-2' -or
    $DatabaseAllFields[1] -ne 'all' -or
    $DatabaseAllFields[2] -ne '53' -or
    $DatabaseAllFields[3] -ne '47' -or
    $DatabaseAllFields[4].Split(',').Count -ne 53 -or
    $DatabaseAllFields[5].Split(',').Count -ne 6 -or
    $DatabaseAllFields[6].Split(',').Count -ne 12) {
    throw "The database development all-case plan differs: $DatabaseAllPlan"
}
$DatabaseUnionPlan = (& node $DatabaseCasePlanner `
    'host-local-service+local-service').Trim()
if ($LASTEXITCODE -ne 0 -or
    $DatabaseUnionPlan -ne
        'windvale-database-storage-development-plan-2|host-local-service+local-service|3|3|LocalService,HostStorage,HostLocalService|-|-') {
    throw "The database development target-set union differs: $DatabaseUnionPlan"
}
$DatabaseSinglePlan = (& node $DatabaseCasePlanner publication).Trim()
if ($LASTEXITCODE -ne 0 -or
    $DatabaseSinglePlan -ne
        'windvale-database-storage-development-plan-2|publication|1|1|Publication|-|-') {
    throw "The database development single-case plan differs: $DatabaseSinglePlan"
}
$DatabaseBundlePlan = (& node $DatabaseCasePlanner `
    'publication+recovery').Trim()
if ($LASTEXITCODE -ne 0 -or
    $DatabaseBundlePlan -ne
        'windvale-database-storage-development-plan-2|publication+recovery|2|1|Publication,Recovery|PublicationRecovery|Publication,Recovery') {
    throw "The database development bundle plan differs: $DatabaseBundlePlan"
}
$DatabaseOverlapBundlePlan = (& node $DatabaseCasePlanner `
    'transaction-leaf-groups-pages-bundle').Trim()
if ($LASTEXITCODE -ne 0 -or
    $DatabaseOverlapBundlePlan -ne
        'windvale-database-storage-development-plan-2|transaction-leaf-groups-pages-bundle|2|1|TransactionLeafGroups,TransactionLeafPages|TransactionLeafGroupsPagesBundle|TransactionLeafGroups,TransactionLeafPages') {
    throw "The database development overlap-bundle plan differs: $DatabaseOverlapBundlePlan"
}
$DatabaseRootBundlePlan = (& node $DatabaseCasePlanner `
    'root-split-depth-two-bundle').Trim()
if ($LASTEXITCODE -ne 0 -or
    $DatabaseRootBundlePlan -ne
        'windvale-database-storage-development-plan-2|root-split-depth-two-bundle|2|1|RootSplit,DepthTwo|RootSplitDepthTwoBundle|RootSplit,DepthTwo') {
    throw "The database development root bundle plan differs: $DatabaseRootBundlePlan"
}
$DatabaseInvalidPlan = @(& node $DatabaseCasePlanner `
    'recovery+recovery' 2>&1)
if ($LASTEXITCODE -ne 64) {
    throw 'The database development target-set planner admitted a duplicate target.'
}
$DatabaseQualificationPlanner = Join-Path $RepositoryRoot `
    'Tools/Native/Plan-Database-Storage-Qualification.mjs'
$DatabaseQualificationPlan = (& node $DatabaseQualificationPlanner --json) |
    ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or
    $DatabaseQualificationPlan.Format -ne
        'windvale-database-storage-qualification-plan-3' -or
    $DatabaseQualificationPlan.Steps -ne 54 -or
    $DatabaseQualificationPlan.Cases -ne 57 -or
    $DatabaseQualificationPlan.Prerequisites -ne 3 -or
    $DatabaseQualificationPlan.PortableSteps -ne 43 -or
    $DatabaseQualificationPlan.HostedSteps -ne 11 -or
    $DatabaseQualificationPlan.PortableCases -ne 46 -or
    $DatabaseQualificationPlan.HostedCases -ne 11 -or
    $DatabaseQualificationPlan.ProjectReferences -ne 58 -or
    $DatabaseQualificationPlan.UniqueProjects -ne 57 -or
    $DatabaseQualificationPlan.SourceReferences -ne 673 -or
    $DatabaseQualificationPlan.UniqueSources -ne 146 -or
    $DatabaseQualificationPlan.ManifestDuplication -ne 4.61 -or
    $DatabaseQualificationPlan.AllPairedSourceVisits -ne
        2 * $DatabaseQualificationPlan.SourceReferences -or
    $DatabaseQualificationPlan.PortableSingleConstructionSteps -ne 42 -or
    $DatabaseQualificationPlan.PortableDuplicateSourceVisitsDelegated -ne 385 -or
    $DatabaseQualificationPlan.DependencyEdges -ne 10 -or
    $DatabaseQualificationPlan.StepsWithDependencies -ne 10 -or
    $DatabaseQualificationPlan.RepeatedSources.Count -eq 0 -or
    $DatabaseQualificationPlan.SharedClosureCandidates.Count -ne 1 -or
    $DatabaseQualificationPlan.SharedClosureCandidates[0].Steps.Count -ne 3 -or
    $DatabaseQualificationPlan.SharedClosureCandidates[0].Cases -notcontains
        'TransactionBranchPages' -or
    $DatabaseQualificationPlan.OverlapMergeCandidates.Count -ne 12 -or
    $DatabaseQualificationPlan.OverlapMergeCandidates[0].SharedSources -ne 14 -or
    $DatabaseQualificationPlan.OverlapMergeCandidates[0].UnionSources -ne 17 -or
    $DatabaseQualificationPlan.OverlapMergeCandidates[0].DeclarationVisitReductionBasisPoints -ne 4516) {
    throw 'The database qualification work-graph plan differs.'
}
$DatabaseQualificationCounts = (& node $DatabaseQualificationPlanner `
    --counts).Trim()
if ($LASTEXITCODE -ne 0 -or $DatabaseQualificationCounts -ne
    'windvale-database-storage-qualification-counts-1|54|57|3|43|11|46|11') {
    throw "The database qualification count plan differs: $DatabaseQualificationCounts"
}
$PreviousQualificationStep = $env:WINDVALE_DATABASE_QUALIFICATION_STEP
try {
    $env:WINDVALE_DATABASE_QUALIFICATION_STEP = 'HostEngine'
    $DatabaseHostedClosure = @(& node $DatabaseQualificationPlanner --closure-env)
    if ($LASTEXITCODE -ne 0 -or $DatabaseHostedClosure.Count -ne 3 -or
        !$DatabaseHostedClosure[0].StartsWith('HostStorage|', [StringComparison]::Ordinal) -or
        !$DatabaseHostedClosure[1].StartsWith('HostTreeReader|', [StringComparison]::Ordinal) -or
        !$DatabaseHostedClosure[2].StartsWith('HostEngine|', [StringComparison]::Ordinal)) {
        throw 'The database hosted dependency closure differs.'
    }
} finally {
    $env:WINDVALE_DATABASE_QUALIFICATION_STEP = $PreviousQualificationStep
}
$DatabasePortableRows = @(& node $DatabaseQualificationPlanner --rows portable)
$DatabasePortableRowResult = $LASTEXITCODE
$DatabaseHostedRows = @(& node $DatabaseQualificationPlanner --rows hosted)
$DatabaseHostedRowResult = $LASTEXITCODE
if ($DatabasePortableRowResult -ne 0 -or
    $DatabaseHostedRowResult -ne 0 -or
    $DatabasePortableRows.Count -ne $DatabaseQualificationPlan.PortableSteps -or
    $DatabaseHostedRows.Count -ne $DatabaseQualificationPlan.HostedSteps -or
    !$DatabasePortableRows[0].StartsWith('Nested|project|', [StringComparison]::Ordinal) -or
    @($DatabasePortableRows | Where-Object {
        $_ -like 'PublicationRecovery|*|Publication,Recovery'
    }).Count -ne 1 -or
    @($DatabasePortableRows | Where-Object {
        $_ -like 'TransactionLeafGroupsPagesBundle|*|TransactionLeafGroups,TransactionLeafPages'
    }).Count -ne 1 -or
    @($DatabasePortableRows | Where-Object {
        $_ -like 'RootSplitDepthTwoBundle|*|RootSplit,DepthTwo'
    }).Count -ne 1 -or
    @($DatabasePortableRows | Where-Object {
        $_ -like 'TransactionRootGrowthBundle|*|TransactionRootGrowth,TransactionRootGrowthMultiLevel'
    }).Count -ne 1 -or
    @($DatabasePortableRows | Where-Object {
        $_ -like 'TransactionAncestorGroupsBundle|*|TransactionAncestorGroups,TransactionAncestorGroupsDepthFour'
    }).Count -ne 1 -or
    @($DatabasePortableRows | Where-Object {
        $_ -like 'TransactionAncestorPagesBundle|*|TransactionAncestorPages,TransactionAncestorPagesIntermediate'
    }).Count -ne 1 -or
    !$DatabasePortableRows[-1].StartsWith('StorageLowering|storage-lowering|', [StringComparison]::Ordinal) -or
    !$DatabaseHostedRows[0].StartsWith('HostStorage|host-storage|', [StringComparison]::Ordinal) -or
    !$DatabaseHostedRows[-1].StartsWith('PersistentTransactionWriter|persistent-transaction-writer|', [StringComparison]::Ordinal)) {
    throw 'The database qualification row plans differ.'
}
$DatabaseQualificationRowLabels = [Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
foreach ($Row in @($DatabasePortableRows + $DatabaseHostedRows)) {
    $Fields = $Row.Split('|')
    if ($Fields.Count -ne 6 -or !$DatabaseQualificationRowLabels.Add($Fields[0])) {
        throw "Malformed or duplicate database qualification row: $Row"
    }
}
$DatabaseWindowsOwner = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Database-Storage.cmd')
$DatabaseLinuxOwner = Get-Content -Raw -LiteralPath (
    Join-Path $RepositoryRoot 'Tools/Native/Test-Database-Storage.sh')
foreach ($OwnerContract in @(
    @{ Host = 'Windows'; Text = $DatabaseWindowsOwner },
    @{ Host = 'Linux'; Text = $DatabaseLinuxOwner }
)) {
    foreach ($Fragment in @(
        'Plan-Database-Storage-Development.mjs',
        '--development-target-set',
        'development_case_selected',
        'development_bundle_selected',
        'development_case_bundled',
        'verify_development_bundle',
        'executions='
    )) {
        if (!$OwnerContract.Text.Contains($Fragment, [StringComparison]::Ordinal)) {
            throw "The $($OwnerContract.Host) database owner is missing '$Fragment'."
        }
    }
}
foreach ($OwnerContract in @(
    @{ Host = 'Windows'; Text = $DatabaseWindowsOwner; Row = '--rows portable'; Lane = '--rows hosted'; PortableCases = '%QualificationPortableCases%'; HostedCases = '%QualificationHostedCases%'; Cases = '%QualificationCases%' },
    @{ Host = 'Linux'; Text = $DatabaseLinuxOwner; Row = '--rows "$lane"'; Lane = 'run_qualification_lane portable'; PortableCases = '$qualification_portable_cases'; HostedCases = '$qualification_hosted_cases'; Cases = '$qualification_cases' }
)) {
    foreach ($Fragment in @(
        'Plan-Database-Storage-Qualification.mjs',
        '--counts',
        '--qualification-step',
        '--closure-env',
        $OwnerContract.Row,
        $OwnerContract.Lane,
        'dispatch_qualification_step',
        'current-host-behavior=Verified',
        'portable-reproducibility=Delegated',
        'cross-target-packaging=Delegated',
        'support-steps=',
        "cases=$($OwnerContract.PortableCases)",
        "cases=$($OwnerContract.HostedCases)",
        "cases=$($OwnerContract.Cases)"
    )) {
        if (!$OwnerContract.Text.Contains($Fragment, [StringComparison]::Ordinal)) {
            throw "The $($OwnerContract.Host) database qualification owner differs from the work-graph inventory at '$Fragment'."
        }
    }
}
$DatabasePortableFunctionContracts = @(
    @{
        Host = 'Windows segmented'
        Text = $DatabaseWindowsOwner.Substring(
            $DatabaseWindowsOwner.LastIndexOf(':verify_segmented_target', [StringComparison]::Ordinal),
            $DatabaseWindowsOwner.LastIndexOf(':verify_target', [StringComparison]::Ordinal) -
                $DatabaseWindowsOwner.LastIndexOf(':verify_segmented_target', [StringComparison]::Ordinal))
    },
    @{
        Host = 'Windows ordinary'
        Text = $DatabaseWindowsOwner.Substring(
            $DatabaseWindowsOwner.LastIndexOf(':verify_target', [StringComparison]::Ordinal),
            $DatabaseWindowsOwner.LastIndexOf(':build_cached_hosted_application', [StringComparison]::Ordinal) -
                $DatabaseWindowsOwner.LastIndexOf(':verify_target', [StringComparison]::Ordinal))
    },
    @{
        Host = 'Linux segmented'
        Text = $DatabaseLinuxOwner.Substring(
            $DatabaseLinuxOwner.IndexOf('verify_segmented_target() {', [StringComparison]::Ordinal),
            $DatabaseLinuxOwner.IndexOf('verify_target() {', [StringComparison]::Ordinal) -
                $DatabaseLinuxOwner.IndexOf('verify_segmented_target() {', [StringComparison]::Ordinal))
    },
    @{
        Host = 'Linux ordinary'
        Text = $DatabaseLinuxOwner.Substring(
            $DatabaseLinuxOwner.IndexOf('verify_target() {', [StringComparison]::Ordinal),
            $DatabaseLinuxOwner.IndexOf('verify_storage_lowering() {', [StringComparison]::Ordinal) -
                $DatabaseLinuxOwner.IndexOf('verify_target() {', [StringComparison]::Ordinal))
    }
)
foreach ($FunctionContract in $DatabasePortableFunctionContracts) {
    if (!$FunctionContract.Text.Contains(
            'qualification owns compiler', [StringComparison]::Ordinal) -or
        $FunctionContract.Text -match '(?i)second[_a-z]*wv[bo]|fc /b|cmp --silent') {
        throw "The $($FunctionContract.Host) portable database function reintroduced private reproducibility work."
    }
}
$DatabaseStorageLoweringContracts = @(
    $DatabaseWindowsOwner.Substring(
        $DatabaseWindowsOwner.LastIndexOf(':verify_storage_lowering', [StringComparison]::Ordinal),
        $DatabaseWindowsOwner.LastIndexOf(':verify_segmented_target', [StringComparison]::Ordinal) -
            $DatabaseWindowsOwner.LastIndexOf(':verify_storage_lowering', [StringComparison]::Ordinal)),
    $DatabaseLinuxOwner.Substring(
        $DatabaseLinuxOwner.IndexOf('verify_storage_lowering() {', [StringComparison]::Ordinal),
        $DatabaseLinuxOwner.IndexOf('verify_host_storage_interruption() {', [StringComparison]::Ordinal) -
            $DatabaseLinuxOwner.IndexOf('verify_storage_lowering() {', [StringComparison]::Ordinal))
)
foreach ($StorageLoweringContract in $DatabaseStorageLoweringContracts) {
    if ($StorageLoweringContract -notmatch '(?i)second[_a-z]*wv[bo]' -or
        $StorageLoweringContract -notmatch '(?i)(fc /b|cmp --silent)') {
        throw 'The database storage-lowering owner no longer retains paired construction evidence.'
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
            '0a0894901341d71ef09712fb63ed0a9f7ac2b93c64b357d123dd09674045cfda',
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
            '4f7aa0abdf870ada362defee6258ba4e6b8ce1f0f67329563d20ed3eb6c9ff24',
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
$NativePlannerInitializationCache = @{}
$NativePlannerCommand = Get-Command -Name $NativePlanner
Write-Host "START verification plan phase=native-routing item=0/$($NativeCases.Count)"
foreach ($Case in $NativeCases) {
    $NativeCaseIndex += 1
    if ($NativeCaseIndex -eq 1 -or $NativeCaseIndex % 10 -eq 0 -or
        $NativeCaseIndex -eq $NativeCases.Count) {
        Write-Host (
            "PROGRESS verification plan phase=native-routing " +
            "item=$NativeCaseIndex/$($NativeCases.Count) case=$($Case.Name)")
    }
    $Plan = & $NativePlannerCommand `
        -ChangedPath $Case.Paths `
        -PassThru `
        -Quiet `
        -InitializationCache $NativePlannerInitializationCache
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
    $DatabaseCaseCountDiffers = (
        $Case.ContainsKey('DatabaseCases') -and
        $Plan.DatabaseStorageDevelopmentCaseCount -ne $Case.DatabaseCases)
    $DatabaseExecutionCountDiffers = (
        $Case.ContainsKey('DatabaseExecutions') -and
        $Plan.DatabaseStorageDevelopmentExecutionCount -ne
            $Case.DatabaseExecutions)
    $DatabaseExpectedSecondsDiffers = (
        $Case.ContainsKey('DatabaseExpectedSeconds') -and
        $Plan.DatabaseStorageDevelopmentExpectedSeconds -ne
            $Case.DatabaseExpectedSeconds)
    $DatabaseMaximumSecondsDiffers = (
        $Case.ContainsKey('DatabaseMaximumSeconds') -and
        $Plan.DatabaseStorageDevelopmentMaximumSeconds -ne
            $Case.DatabaseMaximumSeconds)
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
        $DatabaseTargetDiffers -or
        $DatabaseCaseCountDiffers -or
        $DatabaseExecutionCountDiffers -or
        $DatabaseExpectedSecondsDiffers -or
        $DatabaseMaximumSecondsDiffers
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
            "database-target=$($Plan.DatabaseStorageDevelopmentTarget), " +
            "database-cases=$($Plan.DatabaseStorageDevelopmentCaseCount), " +
            "database-executions=$($Plan.DatabaseStorageDevelopmentExecutionCount), " +
            "database-expected-seconds=" +
            "$($Plan.DatabaseStorageDevelopmentExpectedSeconds), " +
            "database-maximum-seconds=" +
            "$($Plan.DatabaseStorageDevelopmentMaximumSeconds)."
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
