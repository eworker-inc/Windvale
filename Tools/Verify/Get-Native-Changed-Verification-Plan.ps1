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
    } elseif ($Stem -match 'Assemble-Wva') {
        Add-Assembler-Suites
    } elseif ($Stem -match 'Link-Wvo') {
        Add-Linker-Suites
    } elseif ($Stem -match 'Lower-Wvb|Rename-Wvo') {
        Add-Suite @('lowerer-rejections', 'wvo-export-renamer', 'aot-chain')
    } elseif ($Stem -match 'Verify-Wvo|Inspect-Wvo|Publish-Wvo') {
        Add-Object-Suites
    } elseif ($Stem -match 'Build-Wvb|Verify-Wvb|Inspect-Wvb|Run-Wvb') {
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
    } elseif ($Stem -in @('Package-Hosted-Wvb', 'Test-Hosted-Wvb-Packaging')) {
        Add-Suite 'hosted-verifier-publisher-files'
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
    } elseif ($Path.StartsWith('Compiler/Windvale/', [StringComparison]::Ordinal)) {
        Add-Compiler-Suites
    } elseif ($Path.StartsWith('Compiler/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-compiler-recovery-source'
    } elseif ($Path -in @(
        'Runtime/Windvale/Native-Hosted-Verifier-Metadata-Admission.wv',
        'Runtime/Windvale/Native-Hosted-Verifier-Metadata-Construction-Core.wv',
        'Runtime/Windvale/Native-Hosted-Verifier-Metadata-Request-Core.wv',
        'Runtime/Windvale/Native-Hosted-Verifier-Runtime-Header-Core.wv'
    )) {
        Add-Suite 'hosted-verifier-publisher-files'
    } elseif ($Path.StartsWith(
        'Runtime/Windvale/Native-Hosted-Verifier-Publisher-Base-',
        [StringComparison]::Ordinal)) {
        Add-Suite 'hosted-verifier-publisher-files'
    } elseif ($Path.StartsWith('Runtime/Windvale.Bytecode/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Runtime/Windvale.Runtime/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Runtime/Windvale.Native/', [StringComparison]::Ordinal)) {
        if ($Path.EndsWith('.wv', [StringComparison]::OrdinalIgnoreCase) -or
            $Path.EndsWith('.wva', [StringComparison]::OrdinalIgnoreCase)) {
            Add-Bytecode-Suites
        } else {
            Add-Gap 'managed-runtime-recovery-source'
        }
    } elseif ($Path.StartsWith('Foundation/', [StringComparison]::Ordinal)) {
        Add-Suite 'seed'
    } elseif ($Path.StartsWith('Object-Model/Windvale/', [StringComparison]::Ordinal)) {
        Add-Object-Suites
    } elseif ($Path.StartsWith('Object-Model/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-object-recovery-source'
    } elseif ($Path.StartsWith('Assembler/Windvale/', [StringComparison]::Ordinal)) {
        Add-Assembler-Suites
    } elseif ($Path.StartsWith('Assembler/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-assembler-recovery-source'
    } elseif ($Path -in @(
        'Linker/Windvale/Native-Hosted-Verifier-Container-Core.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Container-Tool.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Layout-Core.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Platform-Linux.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Platform-Tool.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Platform-Windows.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Startup-Admission.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Startup-Request-Core.wv',
        'Linker/Windvale/Native-Hosted-Verifier-Startup-Tool.wv'
    )) {
        Add-Suite 'hosted-verifier-publisher-files'
    } elseif ($Path.StartsWith(
        'Linker/Windvale/Native-Hosted-Verifier-Application-',
        [StringComparison]::Ordinal)) {
        Add-Suite 'publisher-rejections'
    } elseif ($Path.StartsWith(
        'Linker/Windvale/Native-Hosted-Verifier-Publisher-',
        [StringComparison]::Ordinal)) {
        Add-Suite 'hosted-verifier-publisher-files'
    } elseif ($Path.StartsWith('Linker/Windvale/', [StringComparison]::Ordinal)) {
        Add-Linker-Suites
    } elseif ($Path.StartsWith('Linker/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-linker-recovery-source'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Hosted-Container-Toolset-Candidate/',
        [StringComparison]::Ordinal) -or
        $Path.StartsWith(
            'Artifacts/Native-Hosted-Verifier-Publisher-Admission-Candidate/',
            [StringComparison]::Ordinal)) {
        Add-Suite 'hosted-verifier-publisher-files'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'hosted-verifier-publisher-files'
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
        if ($Path -eq 'Specifications/Windvale-Hosted-Verifier-Application.md') {
            Add-Suite 'hosted-verifier-publisher-files'
        } elseif ($Path.StartsWith(
            'Specifications/Windvale-Native-Hosted-Verifier-Application-Publisher',
            [StringComparison]::Ordinal) -or
            $Path.StartsWith(
                'Specifications/Windvale-Native-Hosted-Verifier-Publisher-',
                [StringComparison]::Ordinal)) {
            Add-Suite 'hosted-verifier-publisher-files'
            if ($Path.StartsWith(
                'Specifications/Windvale-Native-Hosted-Verifier-Application-Publisher',
                [StringComparison]::Ordinal)) {
                Add-Suite 'publisher-rejections'
            }
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
        } elseif ($Path -match 'Os-|Kernel|Probe') {
            Add-Suite 'os-probe'
        } else {
            Add-Gap "specification:$([IO.Path]::GetFileName($Path))"
        }
    } elseif ($Path -in @(
        'Windvale-Native-Hosted-Verifier-Application-Publisher.wvproj',
        'Windvale-Native-Hosted-Verifier-Application-Publisher-Metadata.wvproj',
        'Windvale-Native-Hosted-Verifier-Application-Tool.wvproj',
        'Windvale-Wvb-Publisher.wvproj'
    )) {
        Add-Suite @('publisher-rejections', 'hosted-verifier-publisher-files')
    } elseif ($Path.StartsWith(
        'Windvale-Native-Hosted-Verifier-Publisher-',
        [StringComparison]::Ordinal)) {
        Add-Suite 'hosted-verifier-publisher-files'
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
