[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$DecisionDirectory = Join-Path $RepositoryRoot 'Documents/Decisions'
$SpecificationDirectory = Join-Path $RepositoryRoot 'Specifications'
$CollisionPath = Join-Path $DecisionDirectory 'Legacy-Id-Collisions.txt'
$DecisionMissingStatusPath = Join-Path $DecisionDirectory 'Legacy-Missing-Status.txt'
$SpecificationMissingStatusPath = Join-Path $SpecificationDirectory 'Legacy-Missing-Status.txt'
$CatalogScript = Join-Path $RepositoryRoot 'Tools/Documentation/Update-Documentation-Catalogs.ps1'
$Failures = [System.Collections.Generic.List[string]]::new()
$CheckedLinks = 0
$CheckedAnchors = 0
$AnchorCache = [System.Collections.Generic.Dictionary[string, object]]::new(
    [StringComparer]::OrdinalIgnoreCase)

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
        $Target.StartsWith('/', [StringComparison]::Ordinal) -or
        $Target -match '^[A-Za-z][A-Za-z0-9+.-]*:' -or
        $Target.StartsWith('{', [StringComparison]::Ordinal)
    )
}

function ConvertTo-GitHubHeadingAnchor {
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string]$Heading
    )

    $Value = [Net.WebUtility]::HtmlDecode($Heading)
    $Value = [regex]::Replace($Value, '\[([^\]]+)\]\([^)]*\)', '$1')
    $Value = [regex]::Replace($Value, '<[^>]+>', '')
    $Value = $Value.Replace('`', '').Replace('*', '').Replace('~', '')
    $Value = $Value.ToLowerInvariant()
    $Value = [regex]::Replace($Value, '[^\p{L}\p{N}\s_-]', '')
    $Value = [regex]::Replace($Value.Trim(), '\s+', '-')
    return $Value
}

function Get-MarkdownAnchors {
    param(
        [Parameter(Mandatory)]
        [string]$DocumentPath
    )

    if ($AnchorCache.ContainsKey($DocumentPath)) {
        return $AnchorCache[$DocumentPath]
    }

    $Anchors = [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
    $BaseCounts = @{}
    $InFence = $false
    foreach ($Line in Get-Content -LiteralPath $DocumentPath) {
        if ($Line -match '^\s*(```|~~~)') {
            $InFence = !$InFence
            continue
        }
        if ($InFence) {
            continue
        }

        foreach ($Match in [regex]::Matches(
            $Line,
            '(?:id|name)\s*=\s*["''](?<anchor>[^"'']+)["'']',
            [Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            $null = $Anchors.Add($Match.Groups['anchor'].Value)
        }

        if ($Line -notmatch '^#{1,6}\s+(?<heading>.+?)\s*#*\s*$') {
            continue
        }
        $Base = ConvertTo-GitHubHeadingAnchor $Matches['heading']
        if ($Base.Length -eq 0) {
            continue
        }
        $Count = if ($BaseCounts.ContainsKey($Base)) { $BaseCounts[$Base] } else { 0 }
        $Anchor = if ($Count -eq 0) { $Base } else { "$Base-$Count" }
        $BaseCounts[$Base] = $Count + 1
        $null = $Anchors.Add($Anchor)
    }

    $AnchorCache[$DocumentPath] = $Anchors
    return $Anchors
}

function Get-OpeningStatusText {
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string[]]$Lines
    )

    for ($Index = 0; $Index -lt [Math]::Min($Lines.Count, 80); $Index++) {
        if ($Lines[$Index] -match '^[-*]\s+Status:\s*(.+?)\s*$') {
            return $Matches[1].Trim()
        }
        if ($Lines[$Index] -notmatch '(?i)^##\s+.*\bstatus\b.*$') {
            continue
        }

        $Parts = [System.Collections.Generic.List[string]]::new()
        $Started = $false
        for ($StatusIndex = $Index + 1;
            $StatusIndex -lt [Math]::Min($Lines.Count, $Index + 16);
            $StatusIndex++) {
            $Line = $Lines[$StatusIndex].Trim()
            if ($Line -match '^##\s+' -or ($Started -and $Line.Length -eq 0)) {
                break
            }
            if ($Line.Length -ne 0) {
                $Started = $true
                $Parts.Add($Line)
            }
        }
        return ($Parts -join ' ')
    }
    return ''
}

function Get-LegacyRegistryEntries {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Header,
        [Parameter(Mandatory)][string]$Description
    )

    if (!(Test-Path -LiteralPath $Path -PathType Leaf)) {
        Add-DocumentationFailure "The $Description registry is missing."
        return @()
    }
    $Text = Get-Content -Raw -LiteralPath $Path
    if ($Text -match '(?:\r?\n){2,}\z') {
        Add-DocumentationFailure (
            "The $Description registry has a blank line at EOF.")
    }
    $Lines = @(Get-Content -LiteralPath $Path |
        Where-Object { ![string]::IsNullOrWhiteSpace($_) })
    if ($Lines.Count -eq 0 -or $Lines[0] -cne $Header) {
        Add-DocumentationFailure (
            "The $Description registry has an invalid header.")
        return @()
    }
    return @($Lines | Select-Object -Skip 1 | Sort-Object)
}

function Test-MarkdownAnchor {
    param(
        [Parameter(Mandatory)][string]$SourcePath,
        [Parameter(Mandatory)][int]$LineNumber,
        [Parameter(Mandatory)][string]$TargetPath,
        [Parameter(Mandatory)][AllowEmptyString()][string]$Fragment,
        [Parameter(Mandatory)][string]$RawTarget
    )

    if ($Fragment.Length -eq 0 -or
        !(Test-Path -LiteralPath $TargetPath -PathType Leaf) -or
        [IO.Path]::GetExtension($TargetPath) -ine '.md') {
        return
    }

    try {
        $DecodedFragment = [Uri]::UnescapeDataString($Fragment)
    } catch {
        Add-DocumentationFailure (
            "$(Get-RepositoryRelativePath $SourcePath):$LineNumber " +
            "has invalid anchor encoding in '$RawTarget'.")
        return
    }

    $script:CheckedAnchors++
    $Anchors = Get-MarkdownAnchors $TargetPath
    if (!$Anchors.Contains($DecodedFragment)) {
        Add-DocumentationFailure (
            "$(Get-RepositoryRelativePath $SourcePath):$LineNumber " +
            "targets missing anchor '#$DecodedFragment' in " +
            "'$(Get-RepositoryRelativePath $TargetPath)'.")
    }
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

    $HashIndex = $Target.IndexOf('#')
    $Fragment = ''
    if ($HashIndex -ge 0) {
        $Fragment = $Target.Substring($HashIndex + 1)
        $Target = $Target.Substring(0, $HashIndex)
    }
    $QuestionIndex = $Target.IndexOf('?')
    if ($QuestionIndex -ge 0) {
        $Target = $Target.Substring(0, $QuestionIndex)
    }

    try {
        if ([string]::IsNullOrWhiteSpace($Target)) {
            $Candidate = $DocumentPath
        } else {
            $Target = [Uri]::UnescapeDataString($Target)
            $Candidate = [IO.Path]::GetFullPath((Join-Path `
                (Split-Path -Parent $DocumentPath) `
                $Target.Replace('/', [IO.Path]::DirectorySeparatorChar)))
        }
    } catch {
        Add-DocumentationFailure (
            "$(Get-RepositoryRelativePath $DocumentPath):$LineNumber " +
            "has invalid local target '$RawTarget'.")
        return
    }

    $script:CheckedLinks++
    $RelativeCandidate = Get-RepositoryRelativePath $Candidate
    if ($RelativeCandidate -eq '..' -or $RelativeCandidate.StartsWith('../')) {
        Add-DocumentationFailure (
            "$(Get-RepositoryRelativePath $DocumentPath):$LineNumber " +
            "targets a path outside the repository in '$RawTarget'.")
        return
    }
    if (!(Test-Path -LiteralPath $Candidate)) {
        Add-DocumentationFailure (
            "$(Get-RepositoryRelativePath $DocumentPath):$LineNumber " +
            "targets missing path '$RawTarget'.")
        return
    }

    $RelativeCandidate = $RelativeCandidate.TrimEnd('/')
    if ($RelativeCandidate -ne '.' -and
        !$KnownRepositoryPaths.Contains($RelativeCandidate)) {
        Add-DocumentationFailure (
            "$(Get-RepositoryRelativePath $DocumentPath):$LineNumber " +
            "uses incorrect path casing in '$RawTarget'; Linux paths are " +
            'case-sensitive.')
    }

    Test-MarkdownAnchor `
        -SourcePath $DocumentPath `
        -LineNumber $LineNumber `
        -TargetPath $Candidate `
        -Fragment $Fragment `
        -RawTarget $RawTarget
}

$TrackedFiles = @(& git -C $RepositoryRoot ls-files)
if ($LASTEXITCODE -ne 0) {
    throw 'Git could not enumerate tracked files.'
}
$UntrackedFiles = @(& git -C $RepositoryRoot ls-files --others --exclude-standard)
if ($LASTEXITCODE -ne 0) {
    throw 'Git could not enumerate untracked files.'
}
$KnownRepositoryPaths = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
foreach ($RelativePath in @($TrackedFiles; $UntrackedFiles)) {
    $Normalized = $RelativePath.Replace('\', '/')
    $null = $KnownRepositoryPaths.Add($Normalized)
    $Segments = $Normalized.Split('/')
    for ($Length = 1; $Length -lt $Segments.Count; $Length++) {
        $null = $KnownRepositoryPaths.Add(($Segments[0..($Length - 1)] -join '/'))
    }
}
$TrackedMarkdown = @($TrackedFiles | Where-Object { $_ -match '(?i)\.md$' })
$UntrackedMarkdown = @($UntrackedFiles | Where-Object { $_ -match '(?i)\.md$' })
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
    'Documents/README.md' = @{ MaximumWords = 2000; MaximumAgeDays = 180 }
    'Documents/Documentation-Policy.md' = @{ MaximumWords = 2500; MaximumAgeDays = 180 }
    'Documents/Terminology.md' = @{ MaximumWords = 2000; MaximumAgeDays = 180 }
    'Documents/Evidence/README.md' = @{ MaximumWords = 2000; MaximumAgeDays = 180 }
    'Documents/Project/Progress.md' = @{ MaximumWords = 2500; MaximumAgeDays = 45 }
    'Documents/Project/Roadmap.md' = @{ MaximumWords = 3500; MaximumAgeDays = 90 }
    'Documents/Project/Project-Vision.md' = @{ MaximumWords = 3000; MaximumAgeDays = 180 }
    'Documents/Architecture/Seed-Implementation.md' = @{ MaximumWords = 2500; MaximumAgeDays = 180 }
    'Documents/Architecture/Browser-Application-Development.md' = @{ MaximumWords = 2500; MaximumAgeDays = 180 }
    'Documents/Runbooks/Seed-Development.md' = @{ MaximumWords = 2500; MaximumAgeDays = 180 }
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

    if ($Text -match '(?m)^> Last reviewed:\s*(\d{4}-\d{2}-\d{2})\s*$') {
        try {
            $ReviewDate = [datetime]::ParseExact(
                $Matches[1],
                'yyyy-MM-dd',
                [Globalization.CultureInfo]::InvariantCulture)
            $Today = [datetime]::UtcNow.Date
            if ($ReviewDate -gt $Today) {
                Add-DocumentationFailure (
                    "$RelativePath has a future last-reviewed date.")
            } elseif (($Today - $ReviewDate).TotalDays -gt
                $Entry.Value.MaximumAgeDays) {
                Add-DocumentationFailure (
                    "$RelativePath was last reviewed $([int](($Today - $ReviewDate).TotalDays)) " +
                    "days ago; its review window is $($Entry.Value.MaximumAgeDays) days.")
            }
        } catch {
            Add-DocumentationFailure (
                "$RelativePath has an invalid last-reviewed date.")
        }
    }

    $WordCount = [regex]::Matches(
        $Text,
        "\b[\p{L}\p{N}][\p{L}\p{N}'ˉ.-]*\b").Count
    if ($WordCount -gt $Entry.Value.MaximumWords) {
        Add-DocumentationFailure (
            "$RelativePath has $WordCount words; its active-context limit is " +
            "$($Entry.Value.MaximumWords).")
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

$DecisionFiles = @(
    Get-ChildItem -LiteralPath $DecisionDirectory -File -Filter '*.md' |
        Where-Object { $_.Name -ne 'README.md' }
)
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

$DeclaredDecisionMissingStatus = Get-LegacyRegistryEntries `
    -Path $DecisionMissingStatusPath `
    -Header 'windvale-legacy-decision-missing-status 1' `
    -Description 'legacy decision missing-status'
$ActualDecisionMissingStatus = @(
    $DecisionEntries |
        Where-Object {
            $Lines = @(Get-Content -LiteralPath $_.Path -TotalCount 80)
            [string]::IsNullOrWhiteSpace((Get-OpeningStatusText $Lines))
        } |
        Select-Object -ExpandProperty Name |
        Sort-Object
)
if ((Compare-Object `
    $DeclaredDecisionMissingStatus `
    $ActualDecisionMissingStatus).Count -ne 0) {
    Add-DocumentationFailure (
        'Decision files without an opening status differ from the frozen ' +
        'legacy registry. New decisions require explicit status; remove a ' +
        'registry entry when an old decision is backfilled.')
}

$SpecificationFiles = @(
    Get-ChildItem -LiteralPath $SpecificationDirectory -File -Filter '*.md' |
        Where-Object { $_.Name -notin @('README.md', 'AGENTS.md') }
)
$DeclaredSpecificationMissingStatus = Get-LegacyRegistryEntries `
    -Path $SpecificationMissingStatusPath `
    -Header 'windvale-legacy-specification-missing-status 1' `
    -Description 'legacy specification missing-status'
$ActualSpecificationMissingStatus = @(
    $SpecificationFiles |
        Where-Object {
            $Lines = @(Get-Content -LiteralPath $_.FullName -TotalCount 80)
            [string]::IsNullOrWhiteSpace((Get-OpeningStatusText $Lines))
        } |
        Select-Object -ExpandProperty Name |
        Sort-Object
)
if ((Compare-Object `
    $DeclaredSpecificationMissingStatus `
    $ActualSpecificationMissingStatus).Count -ne 0) {
    Add-DocumentationFailure (
        'Specifications without an opening status differ from the frozen ' +
        'legacy registry. New specifications require explicit status; remove ' +
        'a registry entry when an old specification is backfilled.')
}

if (!(Test-Path -LiteralPath $CatalogScript -PathType Leaf)) {
    Add-DocumentationFailure 'The documentation catalog generator is missing.'
} else {
    try {
        $CatalogOutput = @(& $CatalogScript -Check)
        foreach ($Line in $CatalogOutput) {
            Write-Host $Line
        }
    } catch {
        Add-DocumentationFailure (
            'Generated documentation catalogs are missing or stale: ' +
            $_.Exception.Message)
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
    "anchors=$CheckedAnchors " +
    "active-documents=$($ActiveDocuments.Count) " +
    "decisions=$($DecisionEntries.Count) " +
    "specifications=$($SpecificationFiles.Count) " +
    "legacy-collisions=$($ActualCollisions.Count)")
