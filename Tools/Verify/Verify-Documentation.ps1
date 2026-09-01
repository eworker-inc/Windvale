[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$DecisionDirectory = Join-Path $RepositoryRoot 'Documents/Decisions'
$CollisionPath = Join-Path $DecisionDirectory 'Legacy-Id-Collisions.txt'
$Failures = [System.Collections.Generic.List[string]]::new()
$CheckedLinks = 0

function Add-DocumentationFailure {
    param(
        [Parameter(Mandatory)]
        [string]$Message
    )

    $Failures.Add($Message)
}

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return [IO.Path]::GetRelativePath($RepositoryRoot, $Path).Replace('\', '/')
}

function Test-ExternalMarkdownTarget {
    param(
        [Parameter(Mandatory)]
        [string]$Target
    )

    return (
        $Target.StartsWith('#', [StringComparison]::Ordinal) -or
        $Target.StartsWith('/', [StringComparison]::Ordinal) -or
        $Target -match '^[A-Za-z][A-Za-z0-9+.-]*:' -or
        $Target.StartsWith('{', [StringComparison]::Ordinal)
    )
}

function Test-MarkdownTarget {
    param(
        [Parameter(Mandatory)]
        [string]$DocumentPath,
        [Parameter(Mandatory)]
        [int]$LineNumber,
        [Parameter(Mandatory)]
        [string]$RawTarget
    )

    $Target = $RawTarget.Trim()
    if ($Target.StartsWith('<', [StringComparison]::Ordinal) -and
        $Target.EndsWith('>', [StringComparison]::Ordinal)) {
        $Target = $Target.Substring(1, $Target.Length - 2)
    }
    if (Test-ExternalMarkdownTarget $Target) {
        return
    }

    $Target = ($Target -split '[#?]', 2)[0]
    if ([string]::IsNullOrWhiteSpace($Target)) {
        return
    }

    try {
        $Target = [Uri]::UnescapeDataString($Target)
        $Candidate = [IO.Path]::GetFullPath((Join-Path `
            (Split-Path -Parent $DocumentPath) `
            $Target.Replace('/', [IO.Path]::DirectorySeparatorChar)))
    } catch {
        Add-DocumentationFailure (
            "$(Get-RepositoryRelativePath $DocumentPath):$LineNumber " +
            "has invalid local target '$RawTarget'.")
        return
    }

    $script:CheckedLinks++
    if (!(Test-Path -LiteralPath $Candidate)) {
        Add-DocumentationFailure (
            "$(Get-RepositoryRelativePath $DocumentPath):$LineNumber " +
            "targets missing path '$RawTarget'.")
    }
}

$TrackedMarkdown = @(& git -C $RepositoryRoot ls-files -- '*.md')
if ($LASTEXITCODE -ne 0) {
    throw 'Git could not enumerate tracked Markdown files.'
}
$UntrackedMarkdown = @(
    & git -C $RepositoryRoot ls-files --others --exclude-standard -- '*.md'
)
if ($LASTEXITCODE -ne 0) {
    throw 'Git could not enumerate untracked Markdown files.'
}
$TrackedMarkdown = @($TrackedMarkdown; $UntrackedMarkdown | Sort-Object -Unique)
$UntrackedMarkdownSet = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
foreach ($RelativePath in $UntrackedMarkdown) {
    $null = $UntrackedMarkdownSet.Add($RelativePath.Replace('\', '/'))
}

foreach ($RelativePath in $TrackedMarkdown) {
    $DocumentPath = Join-Path $RepositoryRoot $RelativePath
    if (!(Test-Path -LiteralPath $DocumentPath -PathType Leaf)) {
        Add-DocumentationFailure "$RelativePath is tracked but missing."
        continue
    }

    if ($UntrackedMarkdownSet.Contains($RelativePath.Replace('\', '/'))) {
        $NewText = Get-Content -Raw -LiteralPath $DocumentPath
        if ($NewText -match '(?:\r?\n){2,}\z') {
            Add-DocumentationFailure (
                "$RelativePath has a blank line at the end of the new file.")
        }
    }

    $InFence = $false
    $LineNumber = 0
    foreach ($Line in Get-Content -LiteralPath $DocumentPath) {
        $LineNumber++
        if ($Line -match '^\s*(```|~~~)') {
            $InFence = !$InFence
            continue
        }
        if ($InFence) {
            continue
        }

        foreach ($Match in [regex]::Matches(
            $Line,
            '!?' +
            '\[[^\]]*\]' +
            '\(' +
            '(?<target><[^>]+>|[^\s)]+)' +
            '(?:\s+["''][^"'']*["''])?' +
            '\)')) {
            Test-MarkdownTarget `
                -DocumentPath $DocumentPath `
                -LineNumber $LineNumber `
                -RawTarget $Match.Groups['target'].Value
        }

        foreach ($Match in [regex]::Matches(
            $Line,
            '(?:href|src)\s*=\s*["''](?<target>[^"'']+)["'']',
            [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            Test-MarkdownTarget `
                -DocumentPath $DocumentPath `
                -LineNumber $LineNumber `
                -RawTarget $Match.Groups['target'].Value
        }
    }
}

$ActiveDocuments = [ordered]@{
    'Documents/README.md' = 2000
    'Documents/Documentation-Policy.md' = 2500
    'Documents/Project/Progress.md' = 2500
    'Documents/Project/Roadmap.md' = 3500
    'Documents/Project/Project-Vision.md' = 3000
    'Documents/Architecture/Seed-Implementation.md' = 2500
    'Documents/Runbooks/Seed-Development.md' = 2500
}
$RawDigestPattern = '(?i)(?<![0-9a-f])[0-9a-f]{64}(?![0-9a-f])'
foreach ($Entry in $ActiveDocuments.GetEnumerator()) {
    $RelativePath = $Entry.Key
    $DocumentPath = Join-Path $RepositoryRoot $RelativePath
    if (!(Test-Path -LiteralPath $DocumentPath -PathType Leaf)) {
        Add-DocumentationFailure "Required active document '$RelativePath' is missing."
        continue
    }

    $Text = Get-Content -Raw -LiteralPath $DocumentPath
    foreach ($Field in @('Status', 'Authority', 'Last reviewed')) {
        if ($Text -notmatch "(?m)^> $([regex]::Escape($Field)):") {
            Add-DocumentationFailure (
                "$RelativePath is missing '> ${Field}:' metadata.")
        }
    }

    $WordCount = [regex]::Matches(
        $Text,
        "\b[\p{L}\p{N}][\p{L}\p{N}'ˉ.-]*\b").Count
    if ($WordCount -gt $Entry.Value) {
        Add-DocumentationFailure (
            "$RelativePath has $WordCount words; its active-context limit is " +
            "$($Entry.Value).")
    }

    if ([regex]::IsMatch($Text, $RawDigestPattern)) {
        Add-DocumentationFailure (
            "$RelativePath contains a raw 64-hex digest; link its canonical " +
            'manifest or evidence record instead.')
    }
}

if (!(Test-Path -LiteralPath $CollisionPath -PathType Leaf)) {
    Add-DocumentationFailure 'The legacy decision-collision registry is missing.'
    $DeclaredCollisions = @()
} else {
    $CollisionText = Get-Content -Raw -LiteralPath $CollisionPath
    if ($CollisionText -match '(?:\r?\n){2,}\z') {
        Add-DocumentationFailure (
            'The legacy decision-collision registry has a blank line at EOF.')
    }
    $CollisionLines = @(
        Get-Content -LiteralPath $CollisionPath |
            Where-Object { ![string]::IsNullOrWhiteSpace($_) }
    )
    if ($CollisionLines.Count -eq 0 -or
        $CollisionLines[0] -ne 'windvale-legacy-decision-id-collisions 1') {
        Add-DocumentationFailure (
            'The legacy decision-collision registry has an invalid header.')
    }
    $DeclaredCollisions = @($CollisionLines | Select-Object -Skip 1 | Sort-Object)
}

$DecisionFiles = @(Get-ChildItem -LiteralPath $DecisionDirectory -File -Filter '*.md')
$DecisionEntries = foreach ($File in $DecisionFiles) {
    if ($File.Name -notmatch '^(?<id>\d{4})-.+\.md$') {
        Add-DocumentationFailure (
            "Decision file '$($File.Name)' does not start with a four-digit id.")
        continue
    }

    [pscustomobject]@{
        Id = $Matches['id']
        Number = [int]$Matches['id']
        Name = $File.Name
        Path = $File.FullName
    }
}

$ActualCollisions = @(
    $DecisionEntries |
        Group-Object Id |
        Where-Object { $_.Count -gt 1 } |
        ForEach-Object {
            "$($_.Name)|$((@($_.Group.Name | Sort-Object)) -join '|')"
        } |
        Sort-Object
)
if ((Compare-Object $DeclaredCollisions $ActualCollisions).Count -ne 0) {
    Add-DocumentationFailure (
        'Decision-number collisions differ from the frozen legacy registry. ' +
        'Do not add a collision or silently change published history.')
}

foreach ($Decision in $DecisionEntries | Where-Object { $_.Number -ge 893 }) {
    $Opening = (Get-Content -LiteralPath $Decision.Path -TotalCount 16) -join "`n"
    if ($Opening -notmatch '(?m)^(## Status\s*|[-] Status:)') {
        Add-DocumentationFailure (
            "Documents/Decisions/$($Decision.Name) lacks explicit status near " +
            'its title.')
    }
}

if ($Failures.Count -ne 0) {
    foreach ($Failure in $Failures) {
        Write-Error $Failure -ErrorAction Continue
    }
    throw "Documentation verification failed with $($Failures.Count) issue(s)."
}

Write-Host (
    'documentation verification status=Passed ' +
    "markdown-files=$($TrackedMarkdown.Count) links=$CheckedLinks " +
    "active-documents=$($ActiveDocuments.Count) " +
    "decisions=$($DecisionEntries.Count) " +
    "legacy-collisions=$($ActualCollisions.Count)")
