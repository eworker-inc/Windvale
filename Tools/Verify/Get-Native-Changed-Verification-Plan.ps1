[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [AllowEmptyCollection()]
    [string[]]$ChangedPath,
    [switch]$PassThru,
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$SuitePlanPath = Join-Path $RepositoryRoot 'Tests/Native/Retirement-Suite.txt'
$SelectedSuites = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$Gaps = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$RunPlanVerification = $false
$RunWebAssemblyVerification = $false
$RunGitHubQualificationVerification = $false

$SuiteEntries = @(
    Get-Content -LiteralPath $SuitePlanPath |
        Select-Object -Skip 1 |
        ForEach-Object {
            $Fields = $_ -split '\|', 4
            if ($Fields.Count -ne 4) {
                throw "Malformed native retirement-suite entry: $_"
            }
            [pscustomobject]@{
                Name = $Fields[0]
                Command = $Fields[1]
            }
        }
)
$KnownSuites = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$SuiteByCommand = @{}
foreach ($Entry in $SuiteEntries) {
    if (!$KnownSuites.Add($Entry.Name)) {
        throw "Duplicate native retirement suite '$($Entry.Name)'."
    }
    $SuiteByCommand[$Entry.Command] = $Entry.Name
}

function Add-Suite {
    param([Parameter(Mandatory)][string[]]$Name)
    foreach ($SuiteName in $Name) {
        if (!$KnownSuites.Contains($SuiteName)) {
            throw "Unknown native retirement suite '$SuiteName'."
        }
        $null = $SelectedSuites.Add($SuiteName)
    }
}

function Add-Gap {
    param([Parameter(Mandatory)][string]$Name)
    $null = $Gaps.Add($Name)
}

function Add-WebAssemblyVerification {
    $script:RunWebAssemblyVerification = $true
}

function Add-GitHubQualificationVerification {
    $script:RunGitHubQualificationVerification = $true
}

function Add-Compiler-Suites {
    Add-Suite @(
        'seed',
        'unsafe-wvb',
        'source-containment',
        'lowerer-rejections',
        'console-packager-source-reconstruction'
    )
}

function Add-Bytecode-Suites {
    Add-Suite @('seed', 'unsafe-wvb', 'wvb-containment')
}

function Add-Object-Suites {
    Add-Suite @(
        'wvo-read-only',
        'wvo-differential',
        'wvo-containment',
        'wvo-hostile-size',
        'publisher-rejections'
    )
}

function Add-Assembler-Suites {
    Add-Suite @('assembler-rejections', 'assembler-golden', 'wva-differential')
}

function Add-Linker-Suites {
    Add-Suite @('linker-rejections', 'linker-hostile', 'linker-map-limit')
}

function Add-Console-Packager-Reconstruction-Suites {
    Add-Suite @(
        'console-packager-source-reconstruction',
        'console-packager-container-reconstruction'
    )
}

function Add-Hosted-Publisher-Suites {
    Add-Suite @(
        'wvb-runner-reconstruction',
        'wvo-inspector-reconstruction',
        'wvo-publisher-reconstruction',
        'console-publisher-reconstruction',
        'hosted-verifier-publisher-files'
    )
}

function Add-Os-Suite {
    param([Parameter(Mandatory)][string]$Path)
    if ($Path -match 'Process-Foundation|Process-Policy') {
        Add-Suite @('os-process-policy', 'os-probe')
    } elseif (
        $Path -match 'Process-Object|Process-Code-Extractor|Process-Resource-Store|Process-Directory-Snapshot|Boot-Resource-Object|Bytecode-Interpreter|Init-Resource-Service|Directory-Process-Service|Process-User-Shim|Boot-Resource-Service'
    ) {
        Add-Suite @('os-process-object', 'os-probe')
    } elseif ($Path -match 'System-Kernel|Kernel-Markers|Hello-World\.wv$') {
        Add-Suite @('os-kernel-target', 'os-probe')
    } elseif ($Path -match 'Probe-Object-Producer|Memory-Object-Producer|Loader-Object-Producer') {
        Add-Suite @('os-probe-object', 'os-probe')
    } else {
        Add-Suite 'os-probe'
    }
}

function Add-Native-Tool-Suite {
    param([Parameter(Mandatory)][string]$Path)
    $Stem = [IO.Path]::GetFileNameWithoutExtension($Path)
    if ($SuiteByCommand.ContainsKey($Stem)) {
        Add-Suite $SuiteByCommand[$Stem]
        return
    }
    if ($Stem -eq 'Test-Retirement-Suite') {
        $script:RunPlanVerification = $true
    } elseif ($Stem -match 'Os-Process-Object') {
        Add-Suite @('os-process-object', 'os-probe')
    } elseif ($Stem -match 'Os-Process-Policy') {
        Add-Suite @('os-process-policy', 'os-probe')
    } elseif ($Stem -match 'Os-Kernel') {
        Add-Suite @('os-kernel-target', 'os-probe')
    } elseif ($Stem -match 'Os-Probe') {
        Add-Suite 'os-probe'
    } elseif ($Stem -in @(
        'Bootstrap-Compiler',
        'Construct-Compiler-Reconstruction'
    )) {
        Add-Suite 'compiler-reconstruction'
    } elseif ($Stem -eq 'Build-Source-Compiler-Product') {
        Add-Compiler-Suites
        Add-Suite 'compiler-reconstruction'
        Add-Suite 'seed-native-front-door'
    } elseif ($Stem -in @(
        'Stage-Compiler-Wvb',
        'Link-Staged-Compiler-Wvo',
        'Transport-Compiler-Image'
    )) {
        Add-Suite @(
            'compiler-reconstruction',
            'segmented-compiler-toolset-reconstruction',
            'wvb-to-wvo-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'wvo-publisher-reconstruction',
            'console-packager-container-reconstruction'
        )
    } elseif ($Stem -in @(
        'Construct-Segmented-Compiler-Toolset',
        'Test-Segmented-Compiler-Packaging',
        'Package-Segmented-Compiler-Wvb'
    )) {
        Add-Suite 'segmented-compiler-toolset-reconstruction'
    } elseif ($Stem -eq 'Construct-Wvb-To-Wvo-Reconstruction') {
        Add-Suite @(
            'wvb-to-wvo-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Stem -eq 'Construct-Wvb-Runner-Reconstruction') {
        Add-Suite 'wvb-runner-reconstruction'
    } elseif ($Stem -eq 'Construct-Wv-Linker-Reconstruction') {
        Add-Suite 'wv-linker-reconstruction'
    } elseif ($Stem -in @(
        'Construct-Wvo-Inspector-Reconstruction',
        'Test-Wvo-Inspector-Reconstruction'
    )) {
        Add-Suite 'wvo-inspector-reconstruction'
    } elseif ($Stem -eq 'Construct-Console-Verifier-Reconstruction') {
        Add-Suite 'console-verifier-reconstruction'
    } elseif ($Stem -eq 'Construct-Console-Application-Publisher') {
        Add-Suite 'console-publisher-reconstruction'
    } elseif ($Stem -eq 'Construct-Wvo-Publisher') {
        Add-Suite 'wvo-publisher-reconstruction'
    } elseif ($Stem -eq 'Construct-Console-Packager-Reconstruction') {
        Add-Suite 'console-packager-container-reconstruction'
    } elseif ($Stem -in @(
        'Test-Baseline-Jit-Patch-Plan',
        'Test-Baseline-Jit-Publisher'
    )) {
        Add-Suite 'baseline-jit'
    } elseif ($Stem -match 'Assemble-Wva') {
        Add-Assembler-Suites
        Add-Suite @('wvb-runner-reconstruction', 'console-verifier-reconstruction')
    } elseif ($Stem -match 'Link-Wvo') {
        Add-Linker-Suites
        Add-Suite @(
            'wvb-runner-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction'
        )
    } elseif ($Stem -eq 'Lower-Wvb-To-Wvo') {
        Add-Suite @(
            'wv-linker-reconstruction',
            'wvb-runner-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'lowerer-rejections',
            'wvo-export-renamer',
            'aot-chain'
        )
    } elseif ($Stem -match 'Rename-Wvo') {
        Add-Suite @('lowerer-rejections', 'wvo-export-renamer', 'aot-chain')
    } elseif ($Stem -match 'Verify-Wvo|Inspect-Wvo') {
        Add-Object-Suites
        Add-Suite 'wvo-inspector-reconstruction'
    } elseif ($Stem -match 'Publish-Wvo') {
        Add-Object-Suites
    } elseif ($Stem -eq 'Build-Wvb') {
        Add-Bytecode-Suites
        Add-Suite @(
            'wv-linker-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction'
        )
    } elseif ($Stem -eq 'Build-Wvdb-Query-Package') {
        Add-Suite 'packages'
    } elseif ($Stem -eq 'Run-Wvb') {
        Add-Bytecode-Suites
        Add-Suite 'wvb-runner-reconstruction'
        Add-Suite 'seed-native-front-door'
    } elseif ($Stem -eq 'Random-Containment-Binary') {
        Add-Suite @('wvb-containment', 'wvo-containment')
    } elseif ($Stem -match 'Verify-Wvb|Inspect-Wvb') {
        Add-Bytecode-Suites
    } elseif ($Stem -match 'Package-Uefi') {
        Add-Suite 'uefi-packager'
    } elseif ($Stem -eq 'Publish-Hosted-Verifier-Application') {
        Add-Suite 'publisher-rejections'
    } elseif ($Stem -eq 'Install-Hosted-Verifier-Publisher') {
        Add-Suite @('publisher-rejections', 'hosted-verifier-publisher-files')
    } elseif ($Stem -in @(
        'Admit-Hosted-Verifier-Publisher',
        'Construct-Hosted-Verifier-Publisher',
        'Construct-Hosted-Verifier-Publisher-Admitter',
        'Construct-Hosted-Verifier-Publisher-Promoter',
        'Construct-Wvb-Publisher'
    )) {
        Add-Suite 'hosted-verifier-publisher-files'
        if ($Stem -eq 'Construct-Hosted-Verifier-Publisher') {
            Add-Suite @('console-publisher-reconstruction', 'wvo-publisher-reconstruction')
        }
    } elseif ($Stem -eq 'Package-Hosted-Wvb') {
        Add-Suite @(
            'wv-linker-reconstruction',
            'console-packager-container-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'hosted-verifier-publisher-files'
        )
    } elseif ($Stem -eq 'Test-Hosted-Wvb-Packaging') {
        Add-Hosted-Publisher-Suites
    } elseif ($Stem -in @('Package-Console', 'Publish-Console')) {
        Add-Suite @(
            'console-publisher-reconstruction',
            'console-packager-rejections',
            'console-container-mutations',
            'hosted-console-container-mutations',
            'console-segmented-size',
            'console-segmented-construction',
            'console-packager-source-reconstruction'
        )
    } elseif ($Stem -match 'Console|Package-Hosted|Segmented') {
        Add-Suite @(
            'console-packager-rejections',
            'console-container-mutations',
            'hosted-console-container-mutations',
            'console-segmented-size',
            'console-segmented-construction',
            'console-packager-source-reconstruction'
        )
    } elseif ($Stem -match 'Compiler|Aot|Baseline-Jit') {
        Add-Suite @('seed', 'aot-chain')
    } else {
        Add-Gap "native-tool:$Stem"
    }
}

$Paths = @(
    $ChangedPath |
        ForEach-Object {
            $Normalized = $_.Replace('\', '/')
            while ($Normalized.StartsWith('./', [StringComparison]::Ordinal)) {
                $Normalized = $Normalized.Substring(2)
            }
            $Normalized.TrimStart('/')
        } |
        Where-Object { ![string]::IsNullOrWhiteSpace($_) } |
        Sort-Object -Unique
)

if ($Paths.Count -eq 0) {
    Add-Gap 'empty-changed-path-set'
}

foreach ($Path in $Paths) {
    $IsDocumentationImage = (
        $Path.StartsWith('Documents/Project/Images/', [StringComparison]::Ordinal) -and
        [IO.Path]::GetExtension($Path) -in @('.gif', '.jpeg', '.jpg', '.png', '.svg', '.webp')
    )
    if (
        $Path.StartsWith('Tools/Editors/', [StringComparison]::Ordinal) -or
        (
            !$Path.StartsWith('Specifications/', [StringComparison]::Ordinal) -and
            (
                $Path.EndsWith('.md', [StringComparison]::OrdinalIgnoreCase) -or
                $IsDocumentationImage
            )
        )
    ) {
        continue
    } elseif ($Path -in @(
        'Documents/Project/Dotnet-Retirement-Inventory.json',
        'Documents/Project/Stage0-Recovery-Dependencies.json',
        'Specifications/README.md',
        'Specifications/Windvale-Native-Changed-Verification.md',
        'Specifications/Windvale-Native-Retirement-Test-Suite.md',
        'Tests/Native/Retirement-Suite.txt'
    )) {
        $RunPlanVerification = $true
    } elseif ($Path -in @(
        'Tools/Recovery/Rebuild-Native-Front-Door.ps1',
        'Tools/Recovery/Rebuild-Native-Front-Door.sh',
        'Tools/Recovery/New-Stage0-Recovery-Archive.ps1',
        'Tools/Recovery/Test-Stage0-Recovery-Archive.ps1'
    )) {
        $RunPlanVerification = $true
    } elseif ($Path -eq 'Tools/Recovery/Rebuild-Baseline-Jit-Publisher.ps1') {
        Add-Suite 'baseline-jit'
    } elseif ($Path -in @(
        'Tools/Recovery/Rebuild-Native-Compiler-Seed.ps1',
        'Tools/Recovery/Rebuild-Native-Compiler-Seed.sh'
    )) {
        Add-Compiler-Suites
        Add-Suite 'compiler-reconstruction'
    } elseif ($Path -in @(
        'Tools/Recovery/Rebuild-WebAssembly-Native-Backend.ps1',
        'Tools/Recovery/Rebuild-WebAssembly-Native-Compiler.ps1'
    )) {
        Add-WebAssemblyVerification
    } elseif ($Path -in @(
        'Tools/Recovery/Verify-Managed-Bootstrap.ps1',
        'Tools/Recovery/Verify-Managed-Bootstrap.sh'
    )) {
        $RunPlanVerification = $true
    } elseif ($Path -in @(
        'Tools/Verify/Verify-WebAssembly.ps1',
        'Tools/Verify/Verify-WebAssembly-Engine.mjs'
    ) -or
        $Path.StartsWith('Tools/WebAssembly/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Windvale.WebAssembly/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Artifacts/WebAssembly-', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Artifacts/webassembly-verification/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/WebAssembly/', [StringComparison]::Ordinal)) {
        Add-WebAssemblyVerification
    } elseif ($Path.StartsWith('Tools/Verify/', [StringComparison]::Ordinal)) {
        if ([IO.Path]::GetFileName($Path) -in @(
            'Verify-GitHub-Native-Qualification.ps1'
        )) {
            Add-GitHubQualificationVerification
            $RunPlanVerification = $true
        } elseif ([IO.Path]::GetFileName($Path) -in @(
            'Verify-Seed-Native-Console-Aot.ps1',
            'Verify-Seed-Native-Console-Aot.sh'
        )) {
            Add-Suite 'seed-native-console-aot'
        } elseif ([IO.Path]::GetFileName($Path) -in @(
            'Verify-Seed-Native-Front-Door.ps1',
            'Verify-Seed-Native-Front-Door.sh'
        )) {
            Add-Suite 'seed-native-front-door'
        } elseif ([IO.Path]::GetFileName($Path) -in @(
            'Classify-Verification-Changes.ps1',
            'Get-Verification-Plan.ps1',
            'Get-Native-Changed-Verification-Plan.ps1',
            'Verify-Changed.ps1',
            'Verify-Change-Classification.ps1',
            'Verify-Dotnet-Retirement-Inventory.ps1',
            'Verify-Stage0-Recovery-Archive.ps1',
            'Verify-Verification-Plan.ps1'
        )) {
            $RunPlanVerification = $true
        } else {
            Add-Gap "verification:$([IO.Path]::GetFileName($Path))"
        }
    } elseif ($Path.StartsWith('Tools/Native/', [StringComparison]::Ordinal)) {
        Add-Native-Tool-Suite $Path
    } elseif ($Path.StartsWith('Tools/Windvale.Project/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Windvale.Build/', [StringComparison]::Ordinal)) {
        Add-Suite 'workspace-project2'
        Add-Compiler-Suites
    } elseif ($Path -eq
        'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Semantic-Core.wv') {
        Add-Bytecode-Suites
        Add-Suite 'libraries'
    } elseif ($Path.StartsWith('Projects/Libraries/', [StringComparison]::Ordinal)) {
        Add-Suite @('workspace-project2', 'libraries')
    } elseif ($Path.StartsWith('Applications/Database/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Distribution/Applications/Wvdb-Query/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Projects/Applications/', [StringComparison]::Ordinal) -or
        $Path -eq 'Specifications/Windvale-Package.md') {
        Add-Suite 'packages'
        if ($Path.StartsWith('Projects/Applications/', [StringComparison]::Ordinal)) {
            Add-Suite 'workspace-project2'
        }
    } elseif ($Path -eq 'Windvale.wvws' -or
        $Path -eq 'Specifications/Windvale-Project.md' -or
        $Path.StartsWith('Tests/Fixtures/Project/', [StringComparison]::Ordinal)) {
        Add-Suite 'workspace-project2'
    } elseif ($Path.StartsWith('Libraries/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Libraries/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Specifications/Windvale-Database', [StringComparison]::Ordinal) -or
        $Path -in @(
            'Specifications/Read-Only-Directory-Capability.md',
            'Specifications/Random-Access-Storage-Capability.md'
        )) {
        Add-Suite 'libraries'
        if ($Path -in @(
            'Libraries/Database/Wvdb-Reader.wv',
            'Libraries/Platform/Filesystem/Read-Only-Directory.wv',
            'Libraries/Platform/Database/Read-Only-Wvdb.wv'
        )) {
            Add-Suite 'packages'
        }
    } elseif ($Path.StartsWith('Operating-System/', [StringComparison]::Ordinal) -or
        $Path -match '^Windvale-Os-.+\.wvproj$') {
        if ($Path.EndsWith('.cs', [StringComparison]::OrdinalIgnoreCase) -or
            $Path.EndsWith('.csproj', [StringComparison]::OrdinalIgnoreCase)) {
            Add-Gap 'managed-os-recovery-source'
        } else {
            Add-Os-Suite $Path
        }
    } elseif ($Path.StartsWith(
        'Compiler/Windvale/Baseline-Jit-',
        [StringComparison]::Ordinal)) {
        Add-Suite 'baseline-jit'
    } elseif ($Path.StartsWith(
        'Compiler/Windvale/WebAssembly-',
        [StringComparison]::Ordinal)) {
        Add-Compiler-Suites
        Add-WebAssemblyVerification
    } elseif ($Path -in @(
        'Compiler/Windvale/Native-X64-Lowering-Core.wv',
        'Compiler/Windvale/Native-X64-Lowering-Bytes-Concatenation.wv',
        'Compiler/Windvale/Native-X64-Lowering-Call-Arguments.wv',
        'Compiler/Windvale/Native-X64-Lowering-Call-Instructions.wv',
        'Compiler/Windvale/Native-X64-Lowering-Capabilities.wv',
        'Compiler/Windvale/Native-X64-Lowering-Data.wv',
        'Compiler/Windvale/Native-X64-Lowering-Descriptors.wv',
        'Compiler/Windvale/Native-X64-Lowering-Descriptor-Calls.wv',
        'Compiler/Windvale/Native-X64-Lowering-Descriptor-Instructions.wv',
        'Compiler/Windvale/Native-X64-Lowering-Enums.wv',
        'Compiler/Windvale/Native-X64-Lowering-Enum-Instructions.wv',
        'Compiler/Windvale/Native-X64-Lowering-Layout.wv',
        'Compiler/Windvale/Native-X64-Lowering-Object.wv',
        'Compiler/Windvale/Native-X64-Lowering-Publication.wv',
        'Compiler/Windvale/Native-X64-Lowering-Records.wv',
        'Compiler/Windvale/Native-X64-Lowering-Record-Allocation.wv',
        'Compiler/Windvale/Native-X64-Lowering-Record-Instructions.wv',
        'Compiler/Windvale/Native-X64-Lowering-Record-Local-Liveness.wv',
        'Compiler/Windvale/Native-X64-Lowering-Record-Storage.wv',
        'Compiler/Windvale/Native-X64-Lowering-Runtime-Descriptors.wv',
        'Compiler/Windvale/Native-X64-Lowering-Staging-Manifest.wv',
        'Compiler/Windvale/Native-X64-Lowering-Staging-Tool.wv',
        'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Envelope.wv',
        'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Relocations.wv',
        'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Symbols.wv',
        'Compiler/Windvale/Native-X64-Lowering-Static-Data-Instructions.wv',
        'Compiler/Windvale/Native-X64-Lowering-Types.wv'
    )) {
        Add-Compiler-Suites
        Add-Suite 'segmented-compiler-toolset-reconstruction'
        Add-Suite @(
            'wvb-to-wvo-reconstruction',
            'wv-linker-reconstruction',
            'wvb-runner-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Path -eq 'Compiler/Windvale/Native-X64-Lowering-Tool.wv') {
        Add-Compiler-Suites
        Add-Suite @(
            'wvb-to-wvo-reconstruction',
            'wv-linker-reconstruction',
            'wvb-runner-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Path -in @(
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
        'Compiler/Windvale/Native-Stencil-Core.wvproj',
        'Compiler/Windvale/Native-Stencil-Bridge.wv',
        'Compiler/Windvale/Native-Stencil-Bridge.wvproj',
        'Compiler/Windvale/Native-Enum-Metadata-Core.wv',
        'Compiler/Windvale/Native-Enum-Metadata-Core.wvproj',
        'Compiler/Windvale/Native-Enum-Metadata-Bridge.wv',
        'Compiler/Windvale/Native-Enum-Metadata.wvproj',
        'Compiler/Windvale/Native-Publication-Core.wv',
        'Compiler/Windvale/Native-Publication-Core.wvproj',
        'Compiler/Windvale/Native-Publication-Bridge.wv',
        'Compiler/Windvale/Native-Publication.wvproj',
        'Compiler/Windvale/Native-Publication-Lifetime-Core.wv',
        'Compiler/Windvale/Native-Publication-Lifetime-Core.wvproj',
        'Compiler/Windvale/Native-Publication-Lifetime-Bridge.wv',
        'Compiler/Windvale/Native-Publication-Lifetime.wvproj',
        'Windvale-Native-Enum-Metadata.wvproj',
        'Projects/Runtime/Windvale-Native-Service-Bundle-Materialization-Core.wvproj',
        'Projects/Runtime/Windvale-Native-Service-Bundle-Materialization.wvproj'
    )) {
        Add-Compiler-Suites
        Add-Suite 'seed-native-front-door'
    } elseif ($Path.StartsWith('Compiler/Windvale/', [StringComparison]::Ordinal)) {
        Add-Compiler-Suites
    } elseif ($Path.StartsWith('Compiler/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-compiler-recovery-source'
    } elseif ($Path -in @(
        'Runtime/Windvale/Native-X64-Utf8-Service.wv',
        'Runtime/Windvale/Native-X64-Utf8-Service-Bridge.wv',
        'Runtime/Windvale/Native-X64-Utf8-Service-Core.wvproj',
        'Runtime/Windvale/Native-X64-Utf8-Service.wvproj',
        'Runtime/Windvale/Native-X64-Integer-Format-Services.wv',
        'Runtime/Windvale/Native-X64-Integer-Format-Services-Bridge.wv',
        'Runtime/Windvale/Native-X64-Integer-Format-Services-Core.wvproj',
        'Runtime/Windvale/Native-X64-Integer-Format-Services.wvproj',
        'Runtime/Windvale/Native-X64-Service-Code-Builder.wv',
        'Runtime/Windvale/Native-X64-Service-Code-Builder.wvproj',
        'Runtime/Windvale/Native-X64-Output-Service-Windows.wv',
        'Runtime/Windvale/Native-X64-Output-Service-Windows.wvproj',
        'Runtime/Windvale/Native-X64-Output-Service-Linux.wv',
        'Runtime/Windvale/Native-X64-Output-Service-Linux.wvproj',
        'Runtime/Windvale/Native-X64-Output-Services-Bridge.wv',
        'Runtime/Windvale/Native-X64-Output-Services.wvproj',
        'Runtime/Windvale/Native-X64-File-Output-Service-Code.wv',
        'Runtime/Windvale/Native-X64-File-Output-Service-Code.wvproj',
        'Runtime/Windvale/Native-X64-File-Output-Service-Windows.wv',
        'Runtime/Windvale/Native-X64-File-Output-Service-Windows.wvproj',
        'Runtime/Windvale/Native-X64-File-Output-Service-Linux.wv',
        'Runtime/Windvale/Native-X64-File-Output-Service-Linux.wvproj',
        'Runtime/Windvale/Native-X64-File-Output-Services-Bridge.wv',
        'Runtime/Windvale/Native-X64-File-Output-Services.wvproj',
        'Runtime/Windvale/Native-X64-File-Input-Service-Code.wv',
        'Runtime/Windvale/Native-X64-File-Input-Service-Code.wvproj',
        'Runtime/Windvale/Native-X64-File-Input-Service-Windows.wv',
        'Runtime/Windvale/Native-X64-File-Input-Service-Windows.wvproj',
        'Runtime/Windvale/Native-X64-File-Input-Service-Linux.wv',
        'Runtime/Windvale/Native-X64-File-Input-Service-Linux.wvproj',
        'Runtime/Windvale/Native-X64-File-Input-Services-Bridge.wv',
        'Runtime/Windvale/Native-X64-File-Input-Services.wvproj',
        'Runtime/Windvale/Native-X64-Text-Concat-Service.wv',
        'Runtime/Windvale/Native-X64-Text-Concat-Service-Core.wvproj',
        'Runtime/Windvale/Native-X64-Text-Concat-Service-Bridge.wv',
        'Runtime/Windvale/Native-X64-Text-Concat-Service.wvproj',
        'Runtime/Windvale/Native-X64-Text-Quote-Service.wv',
        'Runtime/Windvale/Native-X64-Text-Quote-Service-Core.wvproj',
        'Runtime/Windvale/Native-X64-Text-Quote-Service-Bridge.wv',
        'Runtime/Windvale/Native-X64-Text-Quote-Service.wvproj',
        'Runtime/Windvale/Native-X64-Enum-Name-Service.wv',
        'Runtime/Windvale/Native-X64-Enum-Name-Service-Core.wvproj',
        'Runtime/Windvale/Native-X64-Enum-Name-Service-Bridge.wv',
        'Runtime/Windvale/Native-X64-Enum-Name-Service.wvproj',
        'Runtime/Windvale/Native-Service-Bundle-Materialization-Core.wv',
        'Runtime/Windvale/Native-Service-Bundle-Materialization-Bridge.wv',
        'Runtime/Windvale/Native-Output-Table-Core.wv',
        'Runtime/Windvale/Native-Output-Table-Core.wvproj',
        'Runtime/Windvale/Native-Output-Table-Bridge.wv',
        'Runtime/Windvale/Native-Output-Table.wvproj',
        'Runtime/Windvale/Native-File-Output-Table-Core.wv',
        'Runtime/Windvale/Native-File-Output-Table-Core.wvproj',
        'Runtime/Windvale/Native-File-Output-Table-Bridge.wv',
        'Runtime/Windvale/Native-File-Output-Table.wvproj',
        'Runtime/Windvale/Native-File-Input-Table-Core.wv',
        'Runtime/Windvale/Native-File-Input-Table-Core.wvproj',
        'Runtime/Windvale/Native-File-Input-Table-Bridge.wv',
        'Runtime/Windvale/Native-File-Input-Table.wvproj',
        'Runtime/Windvale/Native-Service-Table-Core.wv',
        'Runtime/Windvale/Native-Service-Table-Core.wvproj',
        'Runtime/Windvale/Native-Service-Table-Bridge.wv',
        'Runtime/Windvale/Native-Service-Table.wvproj',
        'Runtime/Windvale/Native-Execution-Context-Core.wv',
        'Runtime/Windvale/Native-Execution-Context-Core.wvproj',
        'Runtime/Windvale/Native-Execution-Context-Bridge.wv',
        'Runtime/Windvale/Native-Execution-Context.wvproj',
        'Runtime/Windvale/Native-Argument-Table-Core.wv',
        'Runtime/Windvale/Native-Argument-Table-Core.wvproj',
        'Runtime/Windvale/Native-Argument-Table-Bridge.wv',
        'Runtime/Windvale/Native-Argument-Table.wvproj',
        'Runtime/Windvale/Native-Entry-Bridge-Core.wv',
        'Runtime/Windvale/Native-Entry-Bridge-Core.wvproj',
        'Runtime/Windvale/Native-Entry-Bridge-Bridge.wv',
        'Runtime/Windvale/Native-Entry-Bridge.wvproj',
        'Runtime/Windvale/Native-Byte-Result-Admission-Core.wv',
        'Runtime/Windvale/Native-Byte-Result-Admission-Core.wvproj',
        'Runtime/Windvale/Native-Byte-Result-Admission-Bridge.wv',
        'Runtime/Windvale/Native-Byte-Result-Admission.wvproj',
        'Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wv',
        'Runtime/Windvale/Native-Hosted-Tool-Metadata-Admission.wvproj',
        'Runtime/Windvale/Native-Hosted-Tool-Metadata-Construction-Core.wv',
        'Runtime/Windvale/Native-Hosted-Tool-Metadata-Construction-Bridge.wv',
        'Runtime/Windvale/Native-Hosted-Tool-Runtime-Header-Core.wv',
        'Runtime/Windvale/Native-Hosted-Tool-Runtime-Header-Bridge.wv',
        'Projects/Runtime/Windvale-Native-Hosted-Tool-Metadata-Construction-Core.wvproj',
        'Projects/Runtime/Windvale-Native-Hosted-Tool-Metadata.wvproj',
        'Projects/Runtime/Windvale-Native-Hosted-Tool-Runtime-Header-Core.wvproj',
        'Projects/Runtime/Windvale-Native-Hosted-Tool-Runtime-Header.wvproj',
        'Windvale-Native-X64-Text-Concat-Service.wvproj',
        'Windvale-Native-X64-Text-Quote-Service.wvproj',
        'Windvale-Native-X64-Enum-Name-Service.wvproj',
        'Windvale-Native-Output-Table.wvproj',
        'Windvale-Native-File-Output-Table.wvproj',
        'Windvale-Native-File-Input-Table.wvproj',
        'Windvale-Native-Service-Table.wvproj',
        'Windvale-Native-Execution-Context.wvproj',
        'Windvale-Native-Argument-Table.wvproj',
        'Windvale-Native-Entry-Bridge.wvproj',
        'Windvale-Native-Byte-Result-Admission.wvproj'
    )) {
        Add-Bytecode-Suites
        Add-Suite 'seed-native-front-door'
    } elseif ($Path -in @(
        'Runtime/Windvale.Native/Consumers/Native-X64-Argument-Count-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Argument-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Enum-Name-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-I32-Format-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Linux-Console-Output-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Linux-Diagnostic-Output-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Linux-File-Input-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Text-Concat-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Text-Quote-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-U32-Format-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Utf8-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Windows-Console-Output-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Windows-Diagnostic-Output-Service.bin',
        'Runtime/Windvale.Native/Consumers/Native-X64-Windows-File-Input-Service.bin'
    )) {
        Add-Suite 'console-verifier-reconstruction'
        if ($Path -notmatch 'Enum-Name|Text-Quote') {
            Add-Suite 'wvb-runner-reconstruction'
        }
        if ($Path -in @(
            'Runtime/Windvale.Native/Consumers/Native-X64-Argument-Count-Service.bin',
            'Runtime/Windvale.Native/Consumers/Native-X64-Argument-Service.bin',
            'Runtime/Windvale.Native/Consumers/Native-X64-Linux-Console-Output-Service.bin',
            'Runtime/Windvale.Native/Consumers/Native-X64-Linux-Diagnostic-Output-Service.bin',
            'Runtime/Windvale.Native/Consumers/Native-X64-Linux-File-Input-Service.bin',
            'Runtime/Windvale.Native/Consumers/Native-X64-Utf8-Service.bin',
            'Runtime/Windvale.Native/Consumers/Native-X64-Windows-Console-Output-Service.bin',
            'Runtime/Windvale.Native/Consumers/Native-X64-Windows-Diagnostic-Output-Service.bin',
            'Runtime/Windvale.Native/Consumers/Native-X64-Windows-File-Input-Service.bin'
        )) {
            Add-Suite 'console-publisher-reconstruction'
        }
    } elseif ($Path -in @(
        'Runtime/Windvale/Native-Hosted-Verifier-Metadata-Admission.wv',
        'Runtime/Windvale/Native-Hosted-Verifier-Metadata-Construction-Core.wv',
        'Runtime/Windvale/Native-Hosted-Verifier-Metadata-Request-Core.wv',
        'Runtime/Windvale/Native-Hosted-Verifier-Publisher-Base-Metadata-Core.wv',
        'Runtime/Windvale/Native-Hosted-Verifier-Publisher-Base-Metadata-Tool.wv',
        'Runtime/Windvale/Native-Hosted-Verifier-Runtime-Header-Core.wv',
        'Runtime/Windvale/Native-Hosted-Verifier-Service-Bundle-Request-Core.wv',
        'Runtime/Windvale/Native-Hosted-Verifier-Service-Bundle-Request-Tool.wv'
    )) {
        Add-Hosted-Publisher-Suites
        Add-Suite 'console-verifier-reconstruction'
    } elseif ($Path.StartsWith(
        'Runtime/Windvale/Native-Hosted-Verifier-Publisher-Base-',
        [StringComparison]::Ordinal)) {
        Add-Hosted-Publisher-Suites
        Add-Suite 'console-verifier-reconstruction'
    } elseif ($Path.StartsWith(
        'Runtime/Windvale/Baseline-Jit-',
        [StringComparison]::Ordinal) -or
        $Path.StartsWith(
            'Runtime/Native/Baseline-Jit-',
            [StringComparison]::Ordinal) -or
        $Path.StartsWith(
            'Runtime/Native/Windows-X64-Baseline-Jit-',
            [StringComparison]::Ordinal) -or
        $Path.StartsWith(
            'Runtime/Native/Linux-X64-Baseline-Jit-',
            [StringComparison]::Ordinal)) {
        Add-Suite 'baseline-jit'
    } elseif ($Path.StartsWith('Runtime/Windvale.Bytecode/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Runtime/Windvale.Runtime/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Runtime/Windvale.Native/', [StringComparison]::Ordinal)) {
        if ($Path.EndsWith('.wv', [StringComparison]::OrdinalIgnoreCase) -or
            $Path.EndsWith('.wva', [StringComparison]::OrdinalIgnoreCase)) {
            Add-Bytecode-Suites
        } else {
            Add-Gap 'managed-runtime-recovery-source'
        }
    } elseif ($Path -in @(
        'Foundation/Byte-Construction.wvproj',
        'Foundation/Byte-Ordering.wvproj',
        'Foundation/Decimal-Parsing.wvproj',
        'Foundation/Machine-Contracts.wvproj'
    )) {
        Add-Suite 'seed'
        Add-Suite 'seed-native-front-door'
    } elseif ($Path -in @(
        'Foundation/Byte-Construction.wv',
        'Foundation/Byte-Ordering.wv',
        'Foundation/Decimal-Parsing.wv',
        'Foundation/Machine-Contracts.wv',
        'Foundation/Sha256.wv'
    )) {
        Add-Suite @('seed', 'wv-linker-reconstruction')
        if ($Path -in @(
            'Foundation/Byte-Construction.wv',
            'Foundation/Sha256.wv'
        )) {
            Add-Suite @('console-verifier-reconstruction', 'console-publisher-reconstruction')
        }
        if ($Path -in @(
            'Foundation/Byte-Construction.wv',
            'Foundation/Decimal-Parsing.wv'
        )) {
            Add-Console-Packager-Reconstruction-Suites
        }
        if ($Path -ne 'Foundation/Sha256.wv') {
            Add-Suite 'seed-native-front-door'
        }
        if ($Path -eq 'Foundation/Decimal-Parsing.wv') {
            Add-Suite 'packages'
        }
    } elseif ($Path -eq 'Foundation/Immutable-Source-Regions.wv') {
        Add-Suite @(
            'seed',
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction'
        )
    } elseif ($Path.StartsWith('Foundation/', [StringComparison]::Ordinal)) {
        Add-Suite 'seed'
    } elseif ($Path -eq 'Object-Model/Windvale/Wvo-Object-Verification.wv') {
        Add-Object-Suites
        Add-Suite @(
            'wvo-inspector-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Path -eq 'Object-Model/Windvale/Wvo-Object-Core.wv') {
        Add-Object-Suites
        Add-Suite 'wvo-inspector-reconstruction'
        Add-Suite 'seed-native-front-door'
    } elseif ($Path.StartsWith('Object-Model/Windvale/', [StringComparison]::Ordinal)) {
        Add-Object-Suites
    } elseif ($Path.StartsWith('Object-Model/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-object-recovery-source'
    } elseif ($Path.StartsWith('Assembler/Windvale/', [StringComparison]::Ordinal)) {
        Add-Assembler-Suites
        if ($Path -eq 'Assembler/Windvale/Wva-Assembler-Core.wv') {
            Add-Suite 'seed-native-front-door'
        }
    } elseif ($Path.StartsWith('Assembler/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-assembler-recovery-source'
    } elseif ($Path -eq 'Examples/Assembler/Hello-Object.wva') {
        Add-Assembler-Suites
        Add-Suite 'seed-native-front-door'
    } elseif ($Path -in @(
        'Linker/Windvale/Console-Application-Admission-Core.wv',
        'Linker/Windvale/Console-Application-Verification-Core.wv',
        'Linker/Windvale/Hosted-Console-Application-Verification-Common.wv',
        'Linker/Windvale/Hosted-Console-Application-Verification-Linux.wv',
        'Linker/Windvale/Hosted-Console-Application-Verification-Windows.wv'
    )) {
        Add-Linker-Suites
        Add-Suite @('console-verifier-reconstruction', 'console-publisher-reconstruction')
    } elseif ($Path -in @(
        'Linker/Windvale/Console-Application-Construction-Core.wv',
        'Linker/Windvale/Console-Application-Packager.wv',
        'Linker/Windvale/Console-Application-Plan-Core.wv',
        'Linker/Windvale/Console-Application-Segmented-Construction.wv',
        'Linker/Windvale/Console-Application-Segmented-Packager.wv',
        'Linker/Windvale/Console-Application-Segmented-Recipe.wv',
        'Linker/Windvale/Console-Application-Staging-Manifest.wv'
    )) {
        Add-Linker-Suites
        Add-Console-Packager-Reconstruction-Suites
        if ($Path -in @(
            'Linker/Windvale/Console-Application-Construction-Core.wv',
            'Linker/Windvale/Console-Application-Plan-Core.wv'
        )) {
            Add-Suite @('console-verifier-reconstruction', 'console-publisher-reconstruction')
        }
    } elseif ($Path -in @(
        'Linker/Windvale/Native-Hosted-Verifier-Container-Core.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Container-Tool.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Bundle-Admission.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Layout-Core.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Platform-Linux.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Platform-Tool.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Platform-Windows.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Startup-Admission.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Startup-Request-Core.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Startup-Targets.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Startup-Tool.wv'
    )) {
        Add-Hosted-Publisher-Suites
        Add-Suite 'console-verifier-reconstruction'
    } elseif ($Path.StartsWith(
        'Linker/Windvale/Native-Hosted-Verifier-Application-',
        [StringComparison]::Ordinal)) {
        Add-Suite 'publisher-rejections'
        if ($Path.StartsWith(
            'Linker/Windvale/Native-Hosted-Verifier-Application-Publisher-',
            [StringComparison]::Ordinal)) {
            Add-Hosted-Publisher-Suites
        }
    } elseif ($Path.StartsWith(
        'Linker/Windvale/Native-Hosted-Verifier-Publisher-',
        [StringComparison]::Ordinal)) {
        Add-Hosted-Publisher-Suites
    } elseif ($Path -in @(
        'Linker/Windvale/Compiler-Flat-Image-Staging-Manifest.wv',
        'Linker/Windvale/Compiler-Flat-Image-Staging-Resources.wv',
        'Linker/Windvale/Compiler-Image-Canonical-Transport-Tool.wv',
        'Linker/Windvale/Compiler-Image-Transport-Resources.wv',
        'Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image-Staging-Tool.wv',
        'Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image.wv',
        'Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image-Verification.wv'
    )) {
        Add-Suite @(
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction'
        )
        Add-Linker-Suites
    } elseif ($Path -eq 'Linker/Windvale/Wv-Linker-Core.wv') {
        Add-Linker-Suites
        Add-Suite @('wv-linker-reconstruction', 'console-publisher-reconstruction')
        Add-Suite 'seed-native-front-door'
    } elseif ($Path -in @(
        'Linker/Windvale/Native-Hosted-Startup-Instantiation-Core.wv',
        'Linker/Windvale/Native-Hosted-Startup-Instantiation-Bridge.wv',
        'Linker/Windvale/Native-Hosted-Startup-Instantiation.wvproj',
        'Linker/Windvale/Native-Hosted-Container-Construction-Core.wv',
        'Linker/Windvale/Native-Hosted-Container-Byte-Construction.wv',
        'Linker/Windvale/Native-Hosted-Container-Layout.wv',
        'Linker/Windvale/Native-Hosted-Container-Windows.wv',
        'Linker/Windvale/Native-Hosted-Container-Linux.wv',
        'Linker/Windvale/Native-Hosted-Container-Segmentation-Core.wv',
        'Linker/Windvale/Native-Hosted-Container-Segmentation.wv',
        'Windvale-Native-Hosted-Startup-Instantiation.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Container-Construction.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Container-Windows.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Container-Linux.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Container-Segmentation.wvproj'
    )) {
        Add-Linker-Suites
        Add-Suite 'seed-native-front-door'
    } elseif ($Path.StartsWith('Linker/Windvale/', [StringComparison]::Ordinal)) {
        Add-Linker-Suites
    } elseif ($Path -in @(
        'Linker/Startup/Windows-X64-Hosted-Inspector.wva',
        'Linker/Startup/Linux-X64-Hosted-Inspector.wva'
    )) {
        Add-Suite @(
            'wvb-runner-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction'
        )
    } elseif ($Path -in @(
        'Linker/Startup/Windows-X64-Wvb-Publisher.wva',
        'Linker/Startup/Linux-X64-Wvb-Publisher.wva',
        'Linker/Reference/Consumers/Windows-X64-Wvb-Publisher.wvo',
        'Linker/Reference/Consumers/Linux-X64-Wvb-Publisher.wvo',
        'Linker/Reference/Consumers/Windows-X64-Wvb-Publication-Adapter.wvo',
        'Linker/Reference/Consumers/Linux-X64-Wvb-Publication-Adapter.wvo',
        'Linker/Reference/Consumers/X64-Wvb-Publication-Sha256.wvo'
    )) {
        Add-Suite 'console-publisher-reconstruction'
    } elseif ($Path.StartsWith('Linker/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-linker-recovery-source'
    } elseif ($Path.StartsWith(
        'Artifacts/Baseline-Jit-Publisher/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'baseline-jit'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Os-Probe-Memory-Object-Producer-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite @('os-probe-object', 'os-probe')
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Compiler-Reconstruction-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'compiler-reconstruction'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Segmented-Compiler-Toolset-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite @(
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction'
        )
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wvb-To-Wvo-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite @(
            'wvb-to-wvo-reconstruction',
            'wv-linker-reconstruction',
            'wvb-runner-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wvb-Runner-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'wvb-runner-reconstruction'
        Add-Suite 'seed-native-front-door'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wvo-Publisher-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'wvo-publisher-reconstruction'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wvo-Object-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'wvo-inspector-reconstruction'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Console-Application-Verifier-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'console-verifier-reconstruction'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Console-Application-Publisher-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'console-publisher-reconstruction'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Console-Packager-Candidate/',
        [StringComparison]::Ordinal) -or
        $Path.StartsWith(
            'Artifacts/Native-Console-Segmented-Packager-Candidate/',
            [StringComparison]::Ordinal)) {
        Add-Suite 'console-packager-container-reconstruction'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Hosted-Container-Toolset-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Hosted-Publisher-Suites
        Add-Suite @('wv-linker-reconstruction', 'console-verifier-reconstruction')
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wv-Linker-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite @(
            'wv-linker-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction'
        )
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Front-Door/',
        [StringComparison]::Ordinal)) {
        Add-Suite @(
            'wvb-runner-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction'
        )
    } elseif ($Path -in @(
        'Artifacts/Native-Aot-Composition-Probe/Return-42.exe',
        'Artifacts/Native-Aot-Composition-Probe/Return-42.elf'
    )) {
        Add-Suite @('console-verifier-reconstruction', 'aot-chain')
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Hosted-Verifier-Publisher-Admission-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'hosted-verifier-publisher-files'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Hosted-Publisher-Suites
        Add-Suite 'console-verifier-reconstruction'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Hosted-Verifier-Publisher-Promoter-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite @('publisher-rejections', 'hosted-verifier-publisher-files')
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wvb-Publisher-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'hosted-verifier-publisher-files'
    } elseif ($Path -eq 'Tools/Windvale.Run/Wvb-Runner-Tool.wv') {
        Add-Suite 'wvb-runner-reconstruction'
        Add-Suite 'seed-native-front-door'
    } elseif ($Path -eq
        'Tools/Windvale.Verify/Console-Application-Verifier-Tool.wv') {
        Add-Suite 'console-verifier-reconstruction'
    } elseif ($Path -eq
        'Tools/Windvale.Publish/Console-Application-Publisher.wv') {
        Add-Suite 'console-publisher-reconstruction'
    } elseif ($Path.StartsWith('Tools/Windvale.Publish/', [StringComparison]::Ordinal) -or
        $Path.StartsWith(
            'Artifacts/Native-Hosted-Verifier-Application-Publisher-Candidate/',
            [StringComparison]::Ordinal)) {
        Add-Suite @('publisher-rejections', 'hosted-verifier-publisher-files')
        if ($Path.StartsWith('Tools/Windvale.Publish/', [StringComparison]::Ordinal) -and
            ($Path -match '/Wvo-Publisher-Tool\.wv$|/Wvb-Publication-')) {
            Add-Suite 'wvo-publisher-reconstruction'
        }
        if ($Path -match '/Wvb-Publication-(Native-Bridge|Transaction)\.wv$') {
            Add-Suite 'console-publisher-reconstruction'
        }
    } elseif (
        $Path.StartsWith(
            'Artifacts/Native-Hosted-Verifier-Application-',
            [StringComparison]::Ordinal) -or
        $Path.StartsWith('Artifacts/Native-Hosted-Verifier-Publisher-',
            [StringComparison]::Ordinal)) {
        Add-Suite 'publisher-rejections'
    } elseif ($Path -eq 'Tests/Fixtures/Native-X64/Baseline-Jit-Patch-Plan-Self-Test.wv') {
        Add-Suite 'baseline-jit'
    } elseif ($Path -eq 'Tests/Fixtures/Native-X64/Wvb-To-Wvo-Return-42.wv') {
        Add-Suite 'wvb-to-wvo-reconstruction'
    } elseif ($Path -eq 'Tests/Native/Plan.txt' -or
        $Path.StartsWith('Tests/Native/Malformed-Wvb/', [StringComparison]::Ordinal)) {
        Add-Suite 'seed'
    } elseif ($Path.StartsWith('Tests/Native/Wvo/', [StringComparison]::Ordinal)) {
        Add-Object-Suites
    } elseif ($Path.StartsWith('Tests/Native/', [StringComparison]::Ordinal)) {
        Add-Gap "native-test:$([IO.Path]::GetFileName($Path))"
    } elseif ($Path.StartsWith('Tests/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-test-recovery-source'
    } elseif ($Path.StartsWith('Specifications/', [StringComparison]::Ordinal)) {
        if ($Path -eq 'Specifications/Seed-CLI.md') {
            Add-Suite @('seed', 'wvb-runner-reconstruction')
            Add-Suite 'seed-native-front-door'
            Add-Suite 'seed-native-console-aot'
        } elseif ($Path -in @(
            'Specifications/Windvale-Baseline-Jit-Patch-Plan.md',
            'Specifications/Windvale-Native-Baseline-Jit-Publication.md'
        )) {
            Add-Suite 'baseline-jit'
        } elseif ($Path -eq
            'Specifications/Windvale-Native-Wvb-Read-Only-Front-Door.md') {
            Add-Bytecode-Suites
        } elseif ($Path -eq 'Specifications/Windvale-WebAssembly.md') {
            Add-WebAssemblyVerification
        } elseif ($Path -eq 'Specifications/Windvale-Native-Compiler-Reconstruction.md') {
            Add-Suite 'compiler-reconstruction'
        } elseif ($Path -in @(
            'Specifications/Windvale-Linking.md',
            'Specifications/Windvale-Native-Hosted-Container-Packaging.md'
        )) {
            Add-Suite @(
                'segmented-compiler-toolset-reconstruction',
                'wv-linker-reconstruction'
            )
            if ($Path -eq 'Specifications/Windvale-Native-Hosted-Container-Packaging.md') {
                Add-Suite @(
                    'wvo-inspector-reconstruction',
                    'console-verifier-reconstruction',
                    'console-publisher-reconstruction'
                )
            } else {
                Add-Linker-Suites
            }
        } elseif ($Path -eq 'Specifications/Windvale-Native-Wvb-To-Wvo.md') {
            Add-Suite 'wvb-to-wvo-reconstruction'
        } elseif ($Path -eq 'Specifications/Windvale-Native-Wvb-Runner.md') {
            Add-Suite 'wvb-runner-reconstruction'
            Add-Suite 'seed-native-front-door'
        } elseif ($Path -in @(
            'Specifications/Windvale-Native-Wv-Linker.md',
            'Specifications/Wv-Linker-Core.md'
        )) {
            Add-Linker-Suites
            Add-Suite 'wv-linker-reconstruction'
        } elseif ($Path -eq 'Specifications/Windvale-Native-Wvo-Publisher.md') {
            Add-Suite 'wvo-publisher-reconstruction'
        } elseif ($Path -eq
            'Specifications/Windvale-Native-Console-Application-Publisher.md') {
            Add-Suite 'console-publisher-reconstruction'
        } elseif ($Path -in @(
            'Specifications/Windvale-Native-Wvo-Inspector.md',
            'Specifications/Wvo-Object-Core.md'
        )) {
            Add-Suite 'wvo-inspector-reconstruction'
        } elseif ($Path -in @(
            'Specifications/Windvale-Native-Hosted-Verifier-Service-Bundle.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Metadata-Request.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Metadata.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Runtime-Header.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Startup.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Platform-Bytes.md',
            'Specifications/Windvale-Native-Hosted-Verifier-Container.md'
        )) {
            Add-Hosted-Publisher-Suites
            Add-Suite 'console-verifier-reconstruction'
        } elseif ($Path -eq 'Specifications/Windvale-Native-Console-Packager.md') {
            Add-Console-Packager-Reconstruction-Suites
        } elseif ($Path -in @(
            'Specifications/Windvale-Native-Console-Application-Segmented-Construction.md',
            'Specifications/Windvale-Native-Console-Application-Segmented-Size-Tests.md',
            'Specifications/Windvale-Native-Hosted-Container-Segmenter.md'
        )) {
            Add-Console-Packager-Reconstruction-Suites
        } elseif ($Path -eq 'Specifications/Windvale-Native-Wvb-Publisher.md') {
            Add-Hosted-Publisher-Suites
            Add-Suite 'publisher-rejections'
        } elseif ($Path -eq 'Specifications/Windvale-Native-X64-Lowering.md') {
            Add-Compiler-Suites
            Add-Suite 'wvb-to-wvo-reconstruction'
        } elseif ($Path -eq 'Specifications/Windvale-Uefi-Application.md') {
            Add-Suite 'uefi-packager'
        } elseif ($Path -eq 'Specifications/Wv-Dump-Core.md') {
            Add-Object-Suites
            Add-Suite 'wvo-inspector-reconstruction'
        } elseif ($Path -eq 'Specifications/Windvale-Native-Test-Plan.md') {
            $RunPlanVerification = $true
        } elseif ($Path -eq 'Specifications/Windvale-Console-Application-Verification.md') {
            Add-Suite 'console-verifier-reconstruction'
        } elseif ($Path -eq 'Specifications/Windvale-Hosted-Verifier-Application.md') {
            Add-Hosted-Publisher-Suites
            Add-Suite 'console-verifier-reconstruction'
        } elseif ($Path.StartsWith(
            'Specifications/Windvale-Native-Hosted-Verifier-Application-Publisher',
            [StringComparison]::Ordinal) -or
            $Path.StartsWith(
                'Specifications/Windvale-Native-Hosted-Verifier-Publisher-',
                [StringComparison]::Ordinal)) {
            Add-Hosted-Publisher-Suites
            if ($Path.StartsWith(
                'Specifications/Windvale-Native-Hosted-Verifier-Application-Publisher',
                [StringComparison]::Ordinal)) {
                Add-Suite 'publisher-rejections'
            }
        } elseif ($Path -match 'Os-|Kernel|Probe') {
            Add-Os-Suite $Path
        } elseif ($Path -match 'Assembly|Wva-') {
            Add-Assembler-Suites
        } elseif ($Path -match 'Linking|Linker') {
            Add-Linker-Suites
        } elseif ($Path -match 'Object|Wvo-') {
            Add-Object-Suites
        } elseif ($Path -match 'Compiler|Source|Seed-Language|Project') {
            Add-Compiler-Suites
        } elseif ($Path -match 'Bytecode|Runtime|Hosted-Resources') {
            Add-Bytecode-Suites
        } else {
            Add-Gap "specification:$([IO.Path]::GetFileName($Path))"
        }
    } elseif (($Path.StartsWith(
        'Windvale-Native-Baseline-Jit-',
        [StringComparison]::Ordinal) -or
        $Path.StartsWith(
            'Projects/Compiler/Windvale-Native-Baseline-Jit-',
            [StringComparison]::Ordinal)) -and
        $Path.EndsWith('.wvproj', [StringComparison]::OrdinalIgnoreCase)) {
        Add-Suite 'baseline-jit'
    } elseif ($Path.StartsWith(
        'Windvale-WebAssembly',
        [StringComparison]::Ordinal) -and
        $Path.EndsWith('.wvproj', [StringComparison]::OrdinalIgnoreCase)) {
        Add-Suite 'seed'
        Add-WebAssemblyVerification
    } elseif ($Path -in @(
        'Projects/Tools/Windvale-Native-Hosted-Verifier-Application-Publisher.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Verifier-Application-Publisher-Metadata.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Verifier-Application-Tool.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Construction-Request.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Application-Tool.wvproj',
        'Projects/Tools/Windvale-Native-Hosted-Verifier-Publisher-Promoter.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Object-Instantiation.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Windows-Imports.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Linux-Materialization.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Windows-Materialization.wvproj',
        'Projects/Linker/Windvale-Native-Hosted-Verifier-Publisher-Target-Request-Tool.wvproj',
        'Projects/Tools/Windvale-Wvb-Publisher.wvproj'
    )) {
        Add-Suite @('publisher-rejections', 'hosted-verifier-publisher-files')
        Add-Hosted-Publisher-Suites
    } elseif ($Path -eq 'Projects/Linker/Windvale-Wv-Linker.wvproj') {
        Add-Suite @('wv-linker-reconstruction', 'console-publisher-reconstruction')
        Add-Suite 'seed-native-front-door'
    } elseif ($Path -eq 'Projects/Assembler/Windvale-Wva-Assembler.wvproj') {
        Add-Assembler-Suites
        Add-Suite 'seed-native-front-door'
    } elseif ($Path -eq 'Projects/Tools/Windvale-Wvo-Publisher.wvproj') {
        Add-Suite 'wvo-publisher-reconstruction'
    } elseif ($Path -eq 'Projects/Tools/Windvale-Wvb-Runner.wvproj') {
        Add-Suite 'wvb-runner-reconstruction'
        Add-Suite 'seed-native-front-door'
    } elseif ($Path -eq 'Projects/Object-Model/Windvale-Wvo-Object.wvproj') {
        Add-Suite 'wvo-inspector-reconstruction'
        Add-Suite 'seed-native-front-door'
    } elseif ($Path -eq 'Projects/Tools/Windvale-Console-Application-Verifier.wvproj') {
        Add-Suite 'console-verifier-reconstruction'
    } elseif ($Path -eq 'Projects/Tools/Windvale-Console-Application-Publisher.wvproj') {
        Add-Suite 'console-publisher-reconstruction'
    } elseif ($Path -in @(
        'Projects/Runtime/Windvale-Native-Hosted-Verifier-Publisher-Base-Metadata-Tool.wvproj',
        'Projects/Runtime/Windvale-Native-Hosted-Verifier-Publisher-Base-Runtime-Tool.wvproj'
    )) {
        Add-Hosted-Publisher-Suites
        Add-Suite 'console-verifier-reconstruction'
    } elseif ($Path -in @(
        'Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj',
        'Projects/Linker/Windvale-Compiler-Image-Staging.wvproj',
        'Projects/Linker/Windvale-Compiler-Image-Canonical-Transport.wvproj'
    )) {
        Add-Suite @(
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction'
        )
    } elseif ($Path -eq 'Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj') {
        Add-Suite @(
            'wvb-to-wvo-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Path -eq 'Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj') {
        Add-Suite 'wvb-to-wvo-reconstruction'
    } elseif ($Path -in @(
        'Projects/Linker/Windvale-Console-Application-Packager.wvproj',
        'Projects/Linker/Windvale-Console-Application-Segmented-Packager.wvproj'
    )) {
        Add-Console-Packager-Reconstruction-Suites
    } elseif ($Path.StartsWith(
        'Windvale-Native-Hosted-Verifier-Publisher-',
        [StringComparison]::Ordinal)) {
        Add-Hosted-Publisher-Suites
    } elseif ($Path -in @(
        'Examples/Seed/Sum-Data.wv',
        'Examples/Seed/Sum-Data.wvproj',
        'Examples/Seed/Hello-Windvale.wv',
        'Examples/Seed/Hello-Windvale.wvproj',
        'Examples/Foundation/Read-Wvb-Header.wv',
        'Examples/Foundation/Read-Wvb-Header.wvproj',
        'Examples/Foundation/Module-Composition-Demo.wv',
        'Examples/Foundation/Module-Composition-Demo.wvproj',
        'Examples/Foundation/Module-Composition-Leaf.wv',
        'Examples/Foundation/Module-Composition-Middle.wv',
        'Examples/Foundation/Machine-Contracts-Demo.wv',
        'Examples/Foundation/Byte-Ordering-Demo.wv',
        'Examples/Foundation/Decimal-Parsing-Demo.wv',
        'Examples/Foundation/Byte-Construction-Demo.wv',
        'Examples/Foundation/Wv-Dump-Core.wv',
        'Projects/Examples/Windvale-Wvb-Inspector.wvproj',
        'Examples/Compiler/Native-Stencil-Demo.wv',
        'Projects/Examples/Native-Stencil-Demo.wvproj',
        'Projects/Examples/Foundation-Machine-Contracts-Demo.wvproj',
        'Projects/Examples/Foundation-Byte-Ordering-Demo.wvproj',
        'Projects/Examples/Foundation-Decimal-Parsing-Demo.wvproj',
        'Projects/Examples/Foundation-Byte-Construction-Demo.wvproj'
    )) {
        Add-Suite 'seed'
        Add-Suite 'seed-native-front-door'
        if ($Path -in @(
            'Examples/Seed/Sum-Data.wv',
            'Examples/Seed/Sum-Data.wvproj'
        )) {
            Add-Suite 'seed-native-console-aot'
        }
    } elseif ($Path.EndsWith('.wvproj', [StringComparison]::OrdinalIgnoreCase)) {
        Add-Suite 'seed'
    } elseif ($Path.StartsWith('Examples/', [StringComparison]::Ordinal)) {
        Add-Suite 'seed'
        if ($Path.StartsWith('Examples/Compiler/WebAssembly-', [StringComparison]::Ordinal)) {
            Add-WebAssemblyVerification
        }
    } elseif ($Path.StartsWith('.github/', [StringComparison]::Ordinal)) {
        Add-GitHubQualificationVerification
    } elseif ($Path -in @('Directory.Build.props', 'global.json', 'Windvale.slnx')) {
        Add-Gap 'managed-build-closure'
    } else {
        Add-Gap "unmapped:$Path"
    }
}

$OrderedSuites = @($SuiteEntries.Name | Where-Object { $SelectedSuites.Contains($_) })
$OrderedGaps = @($Gaps | Sort-Object)
if (!$Quiet) {
    Write-Host "Native suites: [$($OrderedSuites -join ', ')]"
    Write-Host "Native coverage gaps: [$($OrderedGaps -join ', ')]"
    Write-Host "Plan verification: $($RunPlanVerification.ToString().ToLowerInvariant())"
    Write-Host "WebAssembly verification: $($RunWebAssemblyVerification.ToString().ToLowerInvariant())"
    Write-Host "GitHub qualification verification: $($RunGitHubQualificationVerification.ToString().ToLowerInvariant())"
}
if ($PassThru) {
    [pscustomobject]@{
        Suites = $OrderedSuites
        Gaps = $OrderedGaps
        RunPlanVerification = $RunPlanVerification
        RunWebAssemblyVerification = $RunWebAssemblyVerification
        RunGitHubQualificationVerification = $RunGitHubQualificationVerification
        ChangedCount = $Paths.Count
    }
}
