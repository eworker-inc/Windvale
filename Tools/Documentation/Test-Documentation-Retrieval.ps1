[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$CasePath = Join-Path $PSScriptRoot 'Documentation-Retrieval-Cases.json'
$Failures = [System.Collections.Generic.List[string]]::new()
$SeenIds = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$CatalogsChecked = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)

function Resolve-RepositoryPath {
    param(
        [Parameter(Mandatory)][string]$RelativePath,
        [Parameter(Mandatory)][string]$Description
    )

    $Normalized = $RelativePath.Replace('\', '/')
    if ($Normalized.StartsWith('/', [StringComparison]::Ordinal) -or
        $Normalized -match '^[A-Za-z]:' -or
        $Normalized.Split('/') -contains '..') {
        throw "$Description path '$RelativePath' is not repository-relative."
    }
    $FullPath = [IO.Path]::GetFullPath((Join-Path $RepositoryRoot $Normalized))
    if (!(Test-Path -LiteralPath $FullPath -PathType Leaf)) {
        throw "$Description path '$RelativePath' is missing."
    }
    return $FullPath
}

function Test-DirectMarkdownLink {
    param(
        [Parameter(Mandatory)][string]$StartPath,
        [Parameter(Mandatory)][string]$ExpectedPath,
        [AllowEmptyString()][string]$ExpectedAnchor = ''
    )

    $StartDirectory = Split-Path -Parent $StartPath
    foreach ($Match in [regex]::Matches(
        (Get-Content -Raw -LiteralPath $StartPath),
        '!?\[[^\]]*\]\((?<target><[^>]+>|[^\s)]+)')) {
        $Target = $Match.Groups['target'].Value.Trim('<', '>')
        if ($Target -match '^[A-Za-z][A-Za-z0-9+.-]*:' -or
            $Target.StartsWith('/', [StringComparison]::Ordinal)) {
            continue
        }
        $Fragment = ''
        $HashIndex = $Target.IndexOf('#')
        if ($HashIndex -ge 0) {
            $Fragment = $Target.Substring($HashIndex + 1)
            $Target = $Target.Substring(0, $HashIndex)
        }
        $QuestionIndex = $Target.IndexOf('?')
        if ($QuestionIndex -ge 0) {
            $Target = $Target.Substring(0, $QuestionIndex)
        }
        try {
            $Resolved = [IO.Path]::GetFullPath((Join-Path `
                $StartDirectory `
                ([Uri]::UnescapeDataString($Target).Replace(
                    '/',
                    [IO.Path]::DirectorySeparatorChar))))
        } catch {
            continue
        }
        if ($Resolved -cne $ExpectedPath) {
            continue
        }
        if ($ExpectedAnchor.Length -eq 0 -or $Fragment -ceq $ExpectedAnchor) {
            return $true
        }
    }
    return $false
}

if (!(Test-Path -LiteralPath $CasePath -PathType Leaf)) {
    throw 'The documentation retrieval case file is missing.'
}
$Document = Get-Content -Raw -LiteralPath $CasePath | ConvertFrom-Json
if ($Document.schemaVersion -ne 1 -or $null -eq $Document.cases -or
    @($Document.cases).Count -eq 0) {
    throw 'The documentation retrieval case file has an invalid shape.'
}

foreach ($Case in $Document.cases) {
    $Id = [string]$Case.id
    try {
        if ($Id -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$' -or !$SeenIds.Add($Id)) {
            throw "Case id '$Id' is invalid or duplicated."
        }
        if ([string]::IsNullOrWhiteSpace([string]$Case.question)) {
            throw 'The day-to-day question is empty.'
        }
        $StartPath = Resolve-RepositoryPath `
            -RelativePath ([string]$Case.startPath) `
            -Description "Retrieval case '$Id' start"
        $ExpectedPath = Resolve-RepositoryPath `
            -RelativePath ([string]$Case.expectedPath) `
            -Description "Retrieval case '$Id' target"
        $ExpectedAnchor = if ($Case.PSObject.Properties.Name -contains
            'expectedAnchor') {
            [string]$Case.expectedAnchor
        } else {
            ''
        }
        if (!(Test-DirectMarkdownLink `
            -StartPath $StartPath `
            -ExpectedPath $ExpectedPath `
            -ExpectedAnchor $ExpectedAnchor)) {
            $Suffix = if ($ExpectedAnchor.Length -eq 0) {
                ''
            } else {
                "#$ExpectedAnchor"
            }
            throw "No direct link reaches '$($Case.expectedPath)$Suffix'."
        }

        if ($Case.PSObject.Properties.Name -contains 'catalogPath') {
            $CatalogRelativePath = [string]$Case.catalogPath
            $CatalogPath = Resolve-RepositoryPath `
                -RelativePath $CatalogRelativePath `
                -Description "Retrieval case '$Id' catalog"
            $Catalog = Get-Content -Raw -LiteralPath $CatalogPath |
                ConvertFrom-Json
            $CollectionName = [string]$Case.catalogCollection
            if ($CollectionName -notmatch '^[A-Za-z][A-Za-z0-9]*$' -or
                !($Catalog.PSObject.Properties.Name -contains $CollectionName)) {
                throw "Catalog collection '$CollectionName' is missing."
            }
            $ExpectedMatch = $Case.catalogMatch
            if ($null -eq $ExpectedMatch -or
                @($ExpectedMatch.PSObject.Properties).Count -eq 0) {
                throw 'Catalog match fields are empty.'
            }
            $Matched = $false
            foreach ($Entry in @($Catalog.$CollectionName)) {
                $MatchesAll = $true
                foreach ($Property in $ExpectedMatch.PSObject.Properties) {
                    if (!($Entry.PSObject.Properties.Name -contains $Property.Name) -or
                        (ConvertTo-Json $Entry.($Property.Name) -Compress) -cne
                        (ConvertTo-Json $Property.Value -Compress)) {
                        $MatchesAll = $false
                        break
                    }
                }
                if ($MatchesAll) {
                    $Matched = $true
                    break
                }
            }
            if (!$Matched) {
                throw "Catalog '$CatalogRelativePath' has no matching entry."
            }
            $null = $CatalogsChecked.Add($CatalogRelativePath)
        }
    } catch {
        $Failures.Add("$Id`: $($_.Exception.Message)")
    }
}

if ($Failures.Count -ne 0) {
    foreach ($Failure in $Failures) {
        Write-Error $Failure -ErrorAction Continue
    }
    throw "Documentation retrieval verification failed with $($Failures.Count) issue(s)."
}

Write-Host (
    'documentation retrieval status=Passed ' +
    "cases=$($SeenIds.Count) catalogs=$($CatalogsChecked.Count)")
