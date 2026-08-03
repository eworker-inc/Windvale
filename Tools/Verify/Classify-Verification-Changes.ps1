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
$DeployHomepage = $true
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
        $Path.StartsWith('Compiler/Reference/', [StringComparison]::Ordinal) -or
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

function Test-LightweightPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    if ($Path.StartsWith('Specifications/', [StringComparison]::Ordinal)) {
        return $false
    }

    if ($Path.StartsWith('Tools/Editors/', [StringComparison]::Ordinal)) {
        return $true
    }

    return $Path.EndsWith('.md', [StringComparison]::OrdinalIgnoreCase) -or $Path -eq 'LICENSE'
}

function Test-WebsitePath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return (
        $Path.StartsWith('Website/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('functions/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Website/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Windvale.Playground/Editor/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Windvale.Playground/wwwroot/', [StringComparison]::Ordinal) -or
        $Path -in @(
            '.github/workflows/deploy-homepage.yml',
            'package.json',
            'package-lock.json',
            'Vite-Config.mjs',
            'Tools/Windvale.Playground/package.json',
            'Tools/Windvale.Playground/package-lock.json'
        )
    )
}

function Test-HomepageDeploymentPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return (
        $Path.StartsWith('Compiler/Reference/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Object-Model/Windvale.ObjectModel/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Runtime/Windvale.Bytecode/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Runtime/Windvale.Runtime/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Windvale.Playground.Engine/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Windvale.Playground/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Website/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('functions/', [StringComparison]::Ordinal) -or
        $Path -in @(
            '.github/workflows/deploy-homepage.yml',
            'Tools/Website/Verify-Wasm-Demo.mjs',
            'Directory.Build.props',
            'global.json'
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
        $DeployHomepage = $false
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
                if (Test-HomepageDeploymentPath $Path) {
                    $DeployHomepage = $true
                }

                if (Test-Editor-RelevantPath $Path) {
                    $RunEditorVerification = $true
                }

                if (!(Test-LightweightPath $Path)) {
                    if (Test-WebsitePath $Path) {
                        if ($Scope -ne 'qualification') {
                            $Scope = 'website'
                        }
                    } else {
                        $Scope = 'qualification'
                    }
                }
            }
        }
    }
}

$EditorValue = $RunEditorVerification.ToString().ToLowerInvariant()
$HomepageValue = $DeployHomepage.ToString().ToLowerInvariant()
$OutputLines = @(
    "scope=$Scope",
    "editor=$EditorValue",
    "homepage=$HomepageValue",
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
    Write-Host "Homepage deployment: $HomepageValue"
    Write-Host "Changed paths: $($Paths.Count)"
    Write-Host "Reason: $Reason"
}
if ($PassThru) {
    [pscustomobject]@{
        Scope = $Scope
        Editor = $RunEditorVerification
        Homepage = $DeployHomepage
        BaseSha = $ResolvedBase
        HeadSha = $ResolvedHead
        ChangedCount = $Paths.Count
        Reason = $Reason
    }
}
