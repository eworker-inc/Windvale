[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [AllowEmptyCollection()]
    [string[]]$ChangedPath,
    [switch]$PassThru,
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$Classifier = Join-Path $PSScriptRoot 'Classify-Verification-Changes.ps1'
$AllAreas = @(
    'assembler',
    'bytecode',
    'compiler',
    'database',
    'foundation',
    'golden',
    'linker',
    'object-model',
    'runtime'
)
$Areas = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)

function Add-Area {
    param(
        [Parameter(Mandatory)]
        [string[]]$Name
    )

    foreach ($AreaName in $Name) {
        if ($AreaName -notin $AllAreas) {
            throw "Unknown verification area in path mapping: $AreaName"
        }
        $null = $Areas.Add($AreaName)
    }
}

function Add-AllAreas {
    Add-Area $AllAreas
}

function Test-LanguagePaperSourcePath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return $Path -match (
        '^Documents/Project/Language-1\.0-(?:Paper-Corpus|Localization-Workloads)/' +
        '[0-9]+-[^/]+/Source/[^/]+\.wv$')
}

function Test-LanguagePaperDataPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return (
        $Path -match (
            '^Documents/Project/Language-1\.0-Paper-Corpus/' +
            '[0-9]+-[^/]+/Package-Data/[^/]+$') -or
        $Path -match (
            '^Documents/Project/Language-1\.0-Localization-Workloads/' +
            '[0-9]+-[^/]+/Reference-Artifacts/[^/]+$')
    )
}

function Test-LanguageFrozenSourceDesignPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    if ($Path.StartsWith(
        'Documents/Project/Language-1.0-Paper-Corpus/',
        [StringComparison]::Ordinal) -or
        $Path.StartsWith(
            'Documents/Project/Language-1.0-Localization-Workloads/',
            [StringComparison]::Ordinal)) {
        return $true
    }
    if ($Path -match '^Documents/Decisions/(\d{4})-') {
        $Number = [int]$Matches[1]
        if ($Number -ge 751 -and $Number -le 766) { return $true }
    }
    return $Path -in @(
        'Specifications/Windvale-Language-1.0.md',
        'Specifications/Windvale-Language-1.0-Grammar.md',
        'Specifications/Windvale-Language-1.0.ebnf',
        'Specifications/Windvale-Language-1.0-Localized-Source.md',
        'Specifications/Windvale-Language-1.0-Source-Profile-Formats.md',
        'Specifications/Windvale-Language-1.0-Foundation.md',
        'Specifications/Windvale-Language-1.0-Foundation-Registry.md',
        'Specifications/Source-Naming.md',
        'Documents/Project/Windvale-Language-1.0-Design.md',
        'Documents/Project/Windvale-Language-1.0-Migration.md',
        'Documents/Project/Windvale-Language-1.0-Paper-Corpus.md',
        'Documents/Project/Windvale-Accelerator-Compute-And-AI-Design.md',
        'Documents/Project/Windvale-Language-1.0-Localization-Workloads.md',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0815-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0833-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0857-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0861-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0870-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0894-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt'
    )
}

$Paths = @(
    $ChangedPath |
        ForEach-Object {
            $NormalizedPath = $_.Replace('\', '/')
            while ($NormalizedPath.StartsWith('./', [StringComparison]::Ordinal)) {
                $NormalizedPath = $NormalizedPath.Substring(2)
            }
            $NormalizedPath.TrimStart('/')
        } |
        Where-Object { ![string]::IsNullOrWhiteSpace($_) } |
        Sort-Object -Unique
)
$Classification = & $Classifier -ChangedPath $Paths -PassThru -Quiet

if ($Paths.Count -eq 0) {
    Add-AllAreas
}

foreach ($Path in $Paths) {
    if ($Classification.Scope -notin @('development', 'qualification')) {
        break
    }

    if ($Path.StartsWith('Tools/Editors/', [StringComparison]::Ordinal)) {
        continue
    }

    if (Test-LanguageFrozenSourceDesignPath $Path) {
        if ($Path -in @(
            'Specifications/Windvale-Language-1.0.md',
            'Specifications/Windvale-Language-1.0-Foundation.md'
        )) {
            Add-Area @('compiler', 'foundation', 'runtime')
        } elseif ($Path -eq 'Specifications/Source-Naming.md') {
            Add-Area @('compiler', 'runtime')
        } else {
            Add-Area 'compiler'
        }
        continue
    }

    if (
        (Test-LanguagePaperSourcePath $Path) -or
        (Test-LanguagePaperDataPath $Path) -or
        (
            !$Path.StartsWith('Specifications/', [StringComparison]::Ordinal) -and
            (
                $Path.EndsWith('.md', [StringComparison]::OrdinalIgnoreCase) -or
                $Path -eq 'LICENSE.md' -or
                (
                    $Path.StartsWith('Documents/Project/Images/', [StringComparison]::Ordinal) -and
                    [System.IO.Path]::GetExtension($Path) -in @('.gif', '.jpeg', '.jpg', '.png', '.svg', '.webp')
                )
            )
        )
    ) {
        continue
    }

    if ($Path.StartsWith('Compiler/', [StringComparison]::Ordinal)) {
        Add-Area 'compiler'
    } elseif ($Path.StartsWith('Libraries/Database/', [StringComparison]::Ordinal)) {
        Add-Area 'database'
    } elseif ($Path.StartsWith('Runtime/Windvale.Bytecode/', [StringComparison]::Ordinal)) {
        Add-Area 'bytecode'
    } elseif ($Path.StartsWith('Runtime/Windvale.Native/', [StringComparison]::Ordinal)) {
        Add-Area 'runtime'
    } elseif ($Path.StartsWith('Runtime/Windvale/', [StringComparison]::Ordinal)) {
        Add-Area 'runtime'
    } elseif ($Path.StartsWith('Object-Model/', [StringComparison]::Ordinal)) {
        Add-Area 'object-model'
    } elseif ($Path.StartsWith('Assembler/', [StringComparison]::Ordinal)) {
        Add-Area 'assembler'
    } elseif ($Path.StartsWith('Linker/', [StringComparison]::Ordinal)) {
        Add-Area 'linker'
    } elseif ($Path.StartsWith('Foundation/', [StringComparison]::Ordinal)) {
        Add-Area 'foundation'
    } elseif ($Path.StartsWith('Examples/Seed/', [StringComparison]::Ordinal)) {
        Add-Area @('bytecode', 'compiler', 'runtime')
    } elseif ($Path.StartsWith('Examples/Foundation/', [StringComparison]::Ordinal)) {
        Add-Area 'foundation'
    } elseif ($Path.StartsWith('Examples/Compiler/', [StringComparison]::Ordinal)) {
        Add-Area 'compiler'
    } elseif ($Path.EndsWith('.wvproj', [StringComparison]::Ordinal)) {
        Add-Area @('bytecode', 'compiler')
    } elseif ($Path.StartsWith('Examples/Assembler/', [StringComparison]::Ordinal)) {
        Add-Area 'assembler'
    } elseif ($Path.StartsWith('Examples/Linker/', [StringComparison]::Ordinal)) {
        Add-Area 'linker'
    } elseif ($Path -eq 'Specifications/Windvale-Language-1.0-Grammar.md') {
        Add-Area 'compiler'
    } elseif ($Path -in @(
        'Specifications/Windvale-Language-1.0.md',
        'Specifications/Windvale-Language-1.0-Foundation.md'
    )) {
        Add-Area @('compiler', 'foundation', 'runtime')
    } elseif ($Path -match '^Specifications/(Compiler-|Source-Naming|Seed-Language|Seed-Records|Seed-Enums)') {
        Add-Area @('compiler', 'runtime')
    } elseif ($Path -eq 'Specifications/Windvale-Project.md') {
        Add-Area @('bytecode', 'compiler')
    } elseif ($Path -eq 'Specifications/Seed-Bytecode.md') {
        Add-Area @('bytecode', 'runtime')
    } elseif ($Path -eq 'Specifications/Hosted-Resources.md') {
        Add-Area 'runtime'
    } elseif ($Path.StartsWith('Specifications/Windvale-Database', [StringComparison]::Ordinal)) {
        Add-Area 'database'
    } elseif ($Path -match '^Specifications/Foundation-') {
        Add-Area 'foundation'
    } elseif ($Path -match '^Specifications/Wv-Dump-') {
        Add-Area @('bytecode', 'foundation', 'runtime')
    } elseif ($Path -match '^Specifications/(Windvale-Object-Format|Wvo-)') {
        Add-Area @('foundation', 'object-model', 'runtime')
    } elseif ($Path -match '^Specifications/(Windvale-Assembly|Wva-)') {
        Add-Area @('assembler', 'object-model', 'runtime')
    } elseif ($Path -match '^Specifications/(Windvale-Linking|Wv-Linker-)') {
        Add-Area @('linker', 'object-model', 'runtime')
    } elseif ($Path.StartsWith('Specifications/', [StringComparison]::Ordinal)) {
        Add-AllAreas
    } elseif (
        $Path.StartsWith('Tools/Windvale.Project/', [StringComparison]::Ordinal)
    ) {
        Add-Area @('bytecode', 'compiler')
    } elseif (
        $Path.StartsWith('Tests/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Verify/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('.github/', [StringComparison]::Ordinal) -or
        $Path -eq 'Documents/Project/Dotnet-Retirement-Inventory.json'
    ) {
        Add-AllAreas
    } else {
        Add-AllAreas
    }
}

$SelectedAreas = @($AllAreas | Where-Object { $Areas.Contains($_) })
$Plan = [pscustomobject]@{
    Scope = $Classification.Scope
    Editor = $Classification.Editor
    Areas = $SelectedAreas
    ChangedCount = $Paths.Count
}
if (!$Quiet) {
    Write-Host "Changed paths: $($Plan.ChangedCount)"
    Write-Host "Verification scope: $($Plan.Scope)"
    Write-Host "Editor verification: $($Plan.Editor.ToString().ToLowerInvariant())"
    Write-Host "Seed test areas: [$($Plan.Areas -join ', ')]"
}
if ($PassThru) {
    $Plan
}
