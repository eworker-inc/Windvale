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
        'wvo-inspector-reconstruction',
        'wvo-publisher-reconstruction',
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
            'wvo-publisher-reconstruction'
        )
    } elseif ($Stem -eq 'Construct-Wv-Linker-Reconstruction') {
        Add-Suite 'wv-linker-reconstruction'
    } elseif ($Stem -in @(
        'Construct-Wvo-Inspector-Reconstruction',
        'Test-Wvo-Inspector-Reconstruction'
    )) {
        Add-Suite 'wvo-inspector-reconstruction'
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
    } elseif ($Stem -match 'Link-Wvo') {
        Add-Linker-Suites
    } elseif ($Stem -eq 'Lower-Wvb-To-Wvo') {
        Add-Suite @(
            'wv-linker-reconstruction',
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
        Add-Suite 'wv-linker-reconstruction'
    } elseif ($Stem -match 'Verify-Wvb|Inspect-Wvb|Run-Wvb') {
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
            Add-Suite 'wvo-publisher-reconstruction'
        }
    } elseif ($Stem -eq 'Package-Hosted-Wvb') {
        Add-Suite @(
            'wv-linker-reconstruction',
            'console-packager-container-reconstruction',
            'wvo-inspector-reconstruction',
            'wvo-publisher-reconstruction',
            'hosted-verifier-publisher-files'
        )
    } elseif ($Stem -eq 'Test-Hosted-Wvb-Packaging') {
        Add-Hosted-Publisher-Suites
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
        'Specifications/README.md',
        'Specifications/Windvale-Native-Changed-Verification.md',
        'Specifications/Windvale-Native-Retirement-Test-Suite.md',
        'Tests/Native/Retirement-Suite.txt'
    )) {
        $RunPlanVerification = $true
    } elseif ($Path.StartsWith('Tools/Verify/', [StringComparison]::Ordinal)) {
        if ([IO.Path]::GetFileName($Path) -in @(
            'Classify-Verification-Changes.ps1',
            'Get-Verification-Plan.ps1',
            'Get-Native-Changed-Verification-Plan.ps1',
            'Verify-Changed.ps1',
            'Verify-Change-Classification.ps1',
            'Verify-Dotnet-Retirement-Inventory.ps1',
            'Verify-Verification-Plan.ps1'
        )) {
            $RunPlanVerification = $true
        } else {
            Add-Gap "verification:$([IO.Path]::GetFileName($Path))"
        }
    } elseif ($Path.StartsWith('Tools/Native/', [StringComparison]::Ordinal)) {
        Add-Native-Tool-Suite $Path
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
            'wvo-inspector-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Path -eq 'Compiler/Windvale/Native-X64-Lowering-Tool.wv') {
        Add-Compiler-Suites
        Add-Suite @(
            'wvb-to-wvo-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Path.StartsWith('Compiler/Windvale/', [StringComparison]::Ordinal)) {
        Add-Compiler-Suites
    } elseif ($Path.StartsWith('Compiler/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-compiler-recovery-source'
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
    } elseif ($Path.StartsWith(
        'Runtime/Windvale/Native-Hosted-Verifier-Publisher-Base-',
        [StringComparison]::Ordinal)) {
        Add-Hosted-Publisher-Suites
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
        'Foundation/Byte-Construction.wv',
        'Foundation/Byte-Ordering.wv',
        'Foundation/Decimal-Parsing.wv',
        'Foundation/Machine-Contracts.wv',
        'Foundation/Sha256.wv'
    )) {
        Add-Suite @('seed', 'wv-linker-reconstruction')
        if ($Path -in @(
            'Foundation/Byte-Construction.wv',
            'Foundation/Decimal-Parsing.wv'
        )) {
            Add-Console-Packager-Reconstruction-Suites
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
        Add-Suite @('wvo-inspector-reconstruction', 'wvo-publisher-reconstruction')
    } elseif ($Path -eq 'Object-Model/Windvale/Wvo-Object-Core.wv') {
        Add-Object-Suites
        Add-Suite 'wvo-inspector-reconstruction'
    } elseif ($Path.StartsWith('Object-Model/Windvale/', [StringComparison]::Ordinal)) {
        Add-Object-Suites
    } elseif ($Path.StartsWith('Object-Model/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-object-recovery-source'
    } elseif ($Path.StartsWith('Assembler/Windvale/', [StringComparison]::Ordinal)) {
        Add-Assembler-Suites
    } elseif ($Path.StartsWith('Assembler/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-assembler-recovery-source'
    } elseif ($Path -in @(
        'Linker/Windvale/Console-Application-Construction-Core.wv',
        'Linker/Windvale/Console-Application-Packager.wv',
        'Linker/Windvale/Console-Application-Plan-Core.wv',
        'Linker/Windvale/Console-Application-Segmented-Construction.wv',
        'Linker/Windvale/Console-Application-Segmented-Packager.wv',
        'Linker/Windvale/Console-Application-Segmented-Recipe.wv',
        'Linker/Windvale/Console-Application-Staging-Manifest.wv',
        'Linker/Windvale/Console-Application-Verification-Core.wv'
    )) {
        Add-Linker-Suites
        Add-Console-Packager-Reconstruction-Suites
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
    } elseif ($Path.StartsWith(
        'Linker/Windvale/Native-Hosted-Verifier-Application-',
        [StringComparison]::Ordinal)) {
        Add-Suite 'publisher-rejections'
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
        Add-Suite 'wv-linker-reconstruction'
    } elseif ($Path.StartsWith('Linker/Windvale/', [StringComparison]::Ordinal)) {
        Add-Linker-Suites
    } elseif ($Path -in @(
        'Linker/Startup/Windows-X64-Hosted-Inspector.wva',
        'Linker/Startup/Linux-X64-Hosted-Inspector.wva'
    )) {
        Add-Suite 'wvo-inspector-reconstruction'
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
            'wvo-inspector-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wvo-Publisher-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'wvo-publisher-reconstruction'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wvo-Object-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'wvo-inspector-reconstruction'
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
        Add-Suite 'wv-linker-reconstruction'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wv-Linker-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'wv-linker-reconstruction'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Hosted-Verifier-Publisher-Admission-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'hosted-verifier-publisher-files'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Hosted-Publisher-Suites
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Hosted-Verifier-Publisher-Promoter-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite @('publisher-rejections', 'hosted-verifier-publisher-files')
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wvb-Publisher-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'hosted-verifier-publisher-files'
    } elseif ($Path.StartsWith('Tools/Windvale.Publish/', [StringComparison]::Ordinal) -or
        $Path.StartsWith(
            'Artifacts/Native-Hosted-Verifier-Application-Publisher-Candidate/',
            [StringComparison]::Ordinal)) {
        Add-Suite @('publisher-rejections', 'hosted-verifier-publisher-files')
        if ($Path.StartsWith('Tools/Windvale.Publish/', [StringComparison]::Ordinal) -and
            ($Path -match '/Wvo-Publisher-Tool\.wv$|/Wvb-Publication-')) {
            Add-Suite 'wvo-publisher-reconstruction'
        }
    } elseif (
        $Path.StartsWith(
            'Artifacts/Native-Hosted-Verifier-Application-',
            [StringComparison]::Ordinal) -or
        $Path.StartsWith('Artifacts/Native-Hosted-Verifier-Publisher-',
            [StringComparison]::Ordinal)) {
        Add-Suite 'publisher-rejections'
    } elseif ($Path.StartsWith('Libraries/Database/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Specifications/Windvale-Database', [StringComparison]::Ordinal)) {
        Add-Gap 'database-native-tests'
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
        if ($Path -in @(
            'Specifications/Windvale-Baseline-Jit-Patch-Plan.md',
            'Specifications/Windvale-Native-Baseline-Jit-Publication.md'
        )) {
            Add-Suite 'baseline-jit'
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
                Add-Suite 'wvo-inspector-reconstruction'
            } else {
                Add-Linker-Suites
            }
        } elseif ($Path -eq 'Specifications/Windvale-Native-Wvb-To-Wvo.md') {
            Add-Suite 'wvb-to-wvo-reconstruction'
        } elseif ($Path -in @(
            'Specifications/Windvale-Native-Wv-Linker.md',
            'Specifications/Wv-Linker-Core.md'
        )) {
            Add-Linker-Suites
            Add-Suite 'wv-linker-reconstruction'
        } elseif ($Path -eq 'Specifications/Windvale-Native-Wvo-Publisher.md') {
            Add-Suite 'wvo-publisher-reconstruction'
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
        } elseif ($Path -eq 'Specifications/Windvale-Native-Console-Packager.md') {
            Add-Console-Packager-Reconstruction-Suites
        } elseif ($Path -eq 'Specifications/Windvale-Hosted-Verifier-Application.md') {
            Add-Hosted-Publisher-Suites
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
    } elseif ($Path.StartsWith(
        'Windvale-Native-Baseline-Jit-',
        [StringComparison]::Ordinal) -and
        $Path.EndsWith('.wvproj', [StringComparison]::OrdinalIgnoreCase)) {
        Add-Suite 'baseline-jit'
    } elseif ($Path -in @(
        'Windvale-Native-Hosted-Verifier-Application-Publisher.wvproj',
        'Windvale-Native-Hosted-Verifier-Application-Publisher-Metadata.wvproj',
        'Windvale-Native-Hosted-Verifier-Application-Tool.wvproj',
        'Windvale-Wvb-Publisher.wvproj'
    )) {
        Add-Suite @('publisher-rejections', 'hosted-verifier-publisher-files')
    } elseif ($Path -eq 'Windvale-Wv-Linker.wvproj') {
        Add-Suite 'wv-linker-reconstruction'
    } elseif ($Path -eq 'Windvale-Wvo-Publisher.wvproj') {
        Add-Suite 'wvo-publisher-reconstruction'
    } elseif ($Path -eq 'Windvale-Wvo-Object.wvproj') {
        Add-Suite 'wvo-inspector-reconstruction'
    } elseif ($Path -in @(
        'Windvale-Native-X64-Lowering-Staging-Tool.wvproj',
        'Windvale-Compiler-Image-Staging.wvproj',
        'Windvale-Compiler-Image-Canonical-Transport.wvproj'
    )) {
        Add-Suite @(
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction'
        )
    } elseif ($Path -eq 'Windvale-Native-X64-Lowering-Tool.wvproj') {
        Add-Suite @(
            'wvb-to-wvo-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Path -eq 'Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj') {
        Add-Suite 'wvb-to-wvo-reconstruction'
    } elseif ($Path -in @(
        'Windvale-Console-Application-Packager.wvproj',
        'Windvale-Console-Application-Segmented-Packager.wvproj'
    )) {
        Add-Console-Packager-Reconstruction-Suites
    } elseif ($Path.StartsWith(
        'Windvale-Native-Hosted-Verifier-Publisher-',
        [StringComparison]::Ordinal)) {
        Add-Hosted-Publisher-Suites
    } elseif ($Path.EndsWith('.wvproj', [StringComparison]::OrdinalIgnoreCase)) {
        Add-Suite 'seed'
    } elseif ($Path.StartsWith('Examples/', [StringComparison]::Ordinal)) {
        Add-Suite 'seed'
    } elseif ($Path.StartsWith('.github/', [StringComparison]::Ordinal)) {
        Add-Gap 'github-native-qualification'
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
}
if ($PassThru) {
    [pscustomobject]@{
        Suites = $OrderedSuites
        Gaps = $OrderedGaps
        RunPlanVerification = $RunPlanVerification
        ChangedCount = $Paths.Count
    }
}
