[CmdletBinding()]
param(
    [string]$BaseReference,
    [string]$HeadReference = 'HEAD',
    [AllowEmptyCollection()]
    [string[]]$ChangedPath,
    [switch]$ForceQualification,
    [string]$GitHubOutputPath,
    [switch]$PassThru,
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$Scope = 'qualification'
$RunEditorVerification = $true
$ResolvedBase = ''
$ResolvedHead = ''
$Paths = @()
$Reason = 'explicit qualification request'

function Resolve-Commit {
    param(
        [Parameter(Mandatory)]
        [string]$Reference
    )

    $Resolved = & git -C $RepositoryRoot rev-parse --verify "${Reference}^{commit}" 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($Resolved)) {
        return $null
    }

    return $Resolved.Trim()
}

function Test-Editor-RelevantPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return (
        $Path.StartsWith('Tools/Editors/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Compiler/Windvale/', [StringComparison]::Ordinal) -or
        $Path -in @(
            'Specifications/Compiler-Source-Body-Parser.md',
            'Specifications/Compiler-Source-Declaration-Parser.md',
            'Specifications/Compiler-Source-Lexer.md',
            'Specifications/Seed-Language.md',
            'Specifications/Source-Naming.md'
        )
    )
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
        'Documents/Project/Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt'
    )
}

function Test-LightweightPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    if ((Test-LanguageFrozenSourceDesignPath $Path) -or
        $Path.StartsWith('Specifications/', [StringComparison]::Ordinal)) {
        return $false
    }

    if ($Path.StartsWith('Tools/Editors/', [StringComparison]::Ordinal)) {
        return $true
    }

    if ((Test-LanguagePaperSourcePath $Path) -or
        (Test-LanguagePaperDataPath $Path)) {
        return $true
    }

    $IsDocumentationImage = (
        $Path.StartsWith('Documents/Project/Images/', [StringComparison]::Ordinal) -and
        [System.IO.Path]::GetExtension($Path) -in @('.gif', '.jpeg', '.jpg', '.png', '.svg', '.webp')
    )

    return (
        $Path.EndsWith('.md', [StringComparison]::OrdinalIgnoreCase) -or
        $Path -eq 'LICENSE.md' -or
        $Path -in @(
            'Tools/Verify/Classify-Verification-Changes.ps1',
            'Tools/Verify/Verify-Change-Classification.ps1',
            'Tools/Verify/Verify-Verification-Plan.ps1'
        ) -or
        $IsDocumentationImage
    )
}

function Test-WebsitePath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return (
        $Path.StartsWith('Applications/Web/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Libraries/Web/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Website/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('functions/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Website/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Windvale.Playground/Editor/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Windvale.Playground/wwwroot/', [StringComparison]::Ordinal) -or
        $Path -in @(
            '.github/workflows/deploy-homepage.yml',
            'package.json',
            'package-lock.json',
            'Specifications/Browser-Playground.md',
            'Tools/Verify/Verify-Website.ps1',
            'Vite-Config.mjs',
            'Tools/Windvale.Playground/package.json',
            'Tools/Windvale.Playground/package-lock.json',
            'Applications/Web/Wvdb-Workbench/package.json',
            'Applications/Web/Wvdb-Workbench/package-lock.json'
        )
    )
}

if (!$ForceQualification) {
    $CanClassify = $true
    if ($PSBoundParameters.ContainsKey('ChangedPath')) {
        $Paths = @($ChangedPath)
        $Reason = 'explicit changed paths'
    } elseif (
        [string]::IsNullOrWhiteSpace($BaseReference) -or
        $BaseReference -match '^0+$'
    ) {
        $CanClassify = $false
        $Reason = 'missing or zero base reference'
    } else {
        $ResolvedBase = Resolve-Commit $BaseReference
        $ResolvedHead = Resolve-Commit $HeadReference
        if ([string]::IsNullOrWhiteSpace($ResolvedBase) -or [string]::IsNullOrWhiteSpace($ResolvedHead)) {
            $CanClassify = $false
            $Reason = 'unresolved comparison reference'
        } else {
            $Paths = @(& git -C $RepositoryRoot diff `
                --name-only `
                --no-renames `
                --diff-filter=ACDMRTUXB `
                $ResolvedBase `
                $ResolvedHead `
                --)
            if ($LASTEXITCODE -ne 0) {
                $CanClassify = $false
                $Paths = @()
                $Reason = 'Git could not enumerate changed paths'
            } else {
                $Reason = 'classified changed paths'
            }
        }
    }

    if ($CanClassify) {
        $Paths = @(
            $Paths |
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
        if ($Paths.Count -eq 0) {
            $Reason = 'empty changed-path set'
        } else {
            $Scope = 'lightweight'
            $RunEditorVerification = $false
            foreach ($Path in $Paths) {
                if (Test-Editor-RelevantPath $Path) {
                    $RunEditorVerification = $true
                }

                if (!(Test-LightweightPath $Path)) {
                    if (Test-WebsitePath $Path) {
                        if ($Scope -ne 'development') {
                            $Scope = 'website'
                        }
                    } else {
                        $Scope = 'development'
                    }
                }
            }
        }
    }
}

$EditorValue = $RunEditorVerification.ToString().ToLowerInvariant()
$OutputLines = @(
    "scope=$Scope",
    "editor=$EditorValue",
    "base_sha=$ResolvedBase",
    "head_sha=$ResolvedHead",
    "changed_count=$($Paths.Count)"
)
if (![string]::IsNullOrWhiteSpace($GitHubOutputPath)) {
    foreach ($Line in $OutputLines) {
        Add-Content -LiteralPath $GitHubOutputPath -Value $Line -Encoding utf8
    }
}

if (!$Quiet) {
    Write-Host "Verification scope: $Scope"
    Write-Host "Editor verification: $EditorValue"
    Write-Host "Changed paths: $($Paths.Count)"
    Write-Host "Reason: $Reason"
}
if ($PassThru) {
    [pscustomobject]@{
        Scope = $Scope
        Editor = $RunEditorVerification
        BaseSha = $ResolvedBase
        HeadSha = $ResolvedHead
        ChangedCount = $Paths.Count
        Reason = $Reason
    }
}
