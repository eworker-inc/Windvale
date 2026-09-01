[CmdletBinding()]
param(
    [switch]$Check
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$SpecificationDirectory = Join-Path $RepositoryRoot 'Specifications'
$DecisionDirectory = Join-Path $RepositoryRoot 'Documents/Decisions'
$Failures = [System.Collections.Generic.List[string]]::new()
$Written = 0

$Domains = @(
    [pscustomobject]@{
        Key = 'language-and-compiler'
        Title = 'Language and compiler'
        Description = 'Source syntax, semantics, Foundation contracts, analysis, and compiler inputs.'
    },
    [pscustomobject]@{
        Key = 'bytecode-runtime-and-tools'
        Title = 'Bytecode, runtime, and user tools'
        Description = 'Portable bytecode, execution, command-line behavior, WebAssembly, and inspection tools.'
    },
    [pscustomobject]@{
        Key = 'assembly-object-and-linking'
        Title = 'Assembly, objects, and linking'
        Description = 'Textual assembly, object records, relocation, linking, and native image construction.'
    },
    [pscustomobject]@{
        Key = 'native-toolchain'
        Title = 'Native toolchain'
        Description = 'Windvale-owned native compilers, publishers, runtime services, and their focused verification.'
    },
    [pscustomobject]@{
        Key = 'packages-capabilities-and-services'
        Title = 'Packages, capabilities, and services'
        Description = 'Projects, packages, installation, applications, capabilities, storage, and service contracts.'
    },
    [pscustomobject]@{
        Key = 'database'
        Title = 'Database'
        Description = 'WVDB data, query, transaction, storage, durability, and hosted service contracts.'
    },
    [pscustomobject]@{
        Key = 'operating-system'
        Title = 'Operating system'
        Description = 'Boot, kernel, memory, processes, drivers, filesystems, and Windvale OS services.'
    },
    [pscustomobject]@{
        Key = 'network-models-and-host-boundaries'
        Title = 'Network, models, and host boundaries'
        Description = 'Bounded operations, networking, TLS, external models, credentials, and host adapters.'
    }
)

function ConvertTo-LfText {
    param([Parameter(Mandatory)][string]$Text)

    return (($Text -replace "`r`n", "`n") -replace "`r", "`n")
}

function ConvertTo-CompactCatalogJson {
    param(
        [Parameter(Mandatory)][object[]]$Entries
    )

    $Lines = [System.Collections.Generic.List[string]]::new()
    $Lines.Add('{')
    $Lines.Add('  "schemaVersion": 1,')
    $Lines.Add('  "generatedBy": "Tools/Documentation/Update-Documentation-Catalogs.ps1",')
    $Lines.Add('  "entries": [')
    for ($Index = 0; $Index -lt $Entries.Count; $Index++) {
        $Suffix = if ($Index -eq $Entries.Count - 1) { '' } else { ',' }
        $Lines.Add('    ' + (ConvertTo-Json $Entries[$Index] -Depth 6 -Compress) + $Suffix)
    }
    $Lines.Add('  ]')
    $Lines.Add('}')
    return ($Lines -join "`n")
}

function Write-Or-CheckGeneratedFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Text
    )

    $Normalized = (ConvertTo-LfText $Text).TrimEnd("`n") + "`n"
    $RelativePath = [IO.Path]::GetRelativePath($RepositoryRoot, $Path).Replace('\', '/')
    if ($Check) {
        if (!(Test-Path -LiteralPath $Path -PathType Leaf)) {
            $Failures.Add("Generated catalog '$RelativePath' is missing.")
            return
        }
        $Existing = ConvertTo-LfText (Get-Content -Raw -LiteralPath $Path)
        if ($Existing -cne $Normalized) {
            $Failures.Add(
                "Generated catalog '$RelativePath' is stale. Run " +
                'Tools/Documentation/Update-Documentation-Catalogs.ps1.')
        }
        return
    }

    $Parent = Split-Path -Parent $Path
    if (!(Test-Path -LiteralPath $Parent -PathType Container)) {
        $null = New-Item -ItemType Directory -Path $Parent
    }
    [IO.File]::WriteAllText(
        $Path,
        $Normalized,
        [Text.UTF8Encoding]::new($false))
    $script:Written++
}

function Get-DocumentTitle {
    param([Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines)

    foreach ($Line in $Lines) {
        if ($Line -match '^#\s+(.+?)\s*$') {
            return $Matches[1]
        }
    }
    return ''
}

function Get-OpeningStatus {
    param([Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines)

    $Limit = [Math]::Min($Lines.Count, 80)
    for ($Index = 0; $Index -lt $Limit; $Index++) {
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

function Get-SpecificationStatusCategory {
    param([AllowEmptyString()][string]$Status)

    if ([string]::IsNullOrWhiteSpace($Status)) { return 'Unclassified' }
    if ($Status -match '(?i)\bsuperseded\b') { return 'Superseded' }
    if ($Status -match '(?i)^historical\b') { return 'Historical' }
    if ($Status -match '(?i)\bproposed\b') { return 'Proposed' }
    if ($Status -match '(?i)\bexperimental\b') { return 'Experimental' }
    if ($Status -match '(?i)\bcandidate\b') { return 'Candidate' }
    if ($Status -match '(?i)\bimplemented\b') { return 'Implemented' }
    if ($Status -match '(?i)\b(current|accepted|normative)\b') { return 'Current' }
    return 'Documented'
}

function Get-DecisionStatusCategory {
    param([AllowEmptyString()][string]$Status)

    if ([string]::IsNullOrWhiteSpace($Status)) { return 'Unclassified' }
    if ($Status -match '(?i)\bsuperseded\b') { return 'Superseded' }
    if ($Status -match '(?i)^historical\b') { return 'Historical' }
    if ($Status -match '(?i)(^proposed\b|remains\s+proposed\b)') {
        return 'Proposed'
    }
    if ($Status -match '(?i)^accepted\b') { return 'Accepted' }
    if ($Status -match '(?i)\bqualified\b') { return 'Qualified' }
    if ($Status -match '(?i)\bimplemented\b') { return 'Implemented' }
    if ($Status -match '(?i)^retired\b') { return 'Historical' }
    return 'Recorded'
}

function Get-SpecificationDomain {
    param([Parameter(Mandatory)][string]$Name)

    switch -Regex ($Name) {
        '^(Windvale-Os-|Windvale-Kernel-|Windvale-Uefi|Windvale-X64-(Kernel|Exception)|Windvale-System-Kernel|Windvale-Protected-Process)' {
            return 'operating-system'
        }
        '^(Windvale-Database-|Database-)' {
            return 'database'
        }
        '^(Bounded-Https|Bounded-Operation-Core|Host-Network|Host-Tls|Hosted-Model|Network-|Protected-Provider|Supervised-External|Native-External|Windvale-(Bounded-Operation|Bound-Model|External-Model|Model|Network))' {
            return 'network-models-and-host-boundaries'
        }
        '^(Windvale-Assembly|Windvale-Object|Windvale-Linking|Wva-|Wvo-|Wv-Linker|Windvale-Wvo-)' {
            return 'assembly-object-and-linking'
        }
        '^(Compiler-|Foundation-|Hosted-Resources|Seed-(Language|Records|Enums)|Source-Naming|Windvale-(Language|Immutable-Source-Geometry|Compiler|Scripting))' {
            return 'language-and-compiler'
        }
        '^(Windvale-Native-|Native-Fragment)' {
            return 'native-toolchain'
        }
        '^(Seed-(Bytecode|CLI|Conformance)|Browser-Playground|Windvale-(Baseline-Jit|WebAssembly|Shell)|Wv-Dump)' {
            return 'bytecode-runtime-and-tools'
        }
        '^(Random-Access|Read-Only|Standard-|Windvale-(Backend-Libraries|Binary-Data|Capability|Console|Directory|Filesystem|Hosted|Installation|Installer|Libraries|Linux-Console|Package|Project|Release|Resource|Segmented-Hosted|Windows-Console|Wvb-Publication))' {
            return 'packages-capabilities-and-services'
        }
        default {
            throw "Specification '$Name.md' has no catalog domain rule."
        }
    }
}

function ConvertTo-MarkdownCell {
    param([AllowEmptyString()][string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) { return '—' }
    return (($Text -replace '\|', '\|') -replace "`r?`n", ' ')
}

$SpecificationEntries = @(
    foreach ($File in Get-ChildItem -LiteralPath $SpecificationDirectory -File -Filter '*.md' |
        Where-Object { $_.Name -notin @('README.md', 'AGENTS.md') } |
        Sort-Object Name) {
        $Lines = @(Get-Content -LiteralPath $File.FullName)
        $Title = Get-DocumentTitle $Lines
        if ([string]::IsNullOrWhiteSpace($Title)) {
            throw "Specification '$($File.Name)' has no level-one title."
        }
        $OpeningStatus = Get-OpeningStatus $Lines
        [pscustomobject][ordered]@{
            key = $File.BaseName
            path = "Specifications/$($File.Name)"
            title = $Title
            domain = Get-SpecificationDomain $File.BaseName
            status = Get-SpecificationStatusCategory $OpeningStatus
            statusText = if ($OpeningStatus.Length -eq 0) { $null } else { $OpeningStatus }
        }
    }
)

$SpecificationJson = ConvertTo-CompactCatalogJson $SpecificationEntries
Write-Or-CheckGeneratedFile `
    -Path (Join-Path $SpecificationDirectory 'Specification-Catalog.json') `
    -Text $SpecificationJson

$StatusCounts = @($SpecificationEntries | Group-Object status | Sort-Object Name)
$RootLines = [System.Collections.Generic.List[string]]::new()
$RootLines.Add('# Windvale specification index')
$RootLines.Add('')
$RootLines.Add('> Generated by `Tools/Documentation/Update-Documentation-Catalogs.ps1`; do not edit by hand.')
$RootLines.Add('')
$RootLines.Add('Specifications contain the exact contracts used by implementations and verifiers. Start with one domain index, then open only the contract needed for the task. Dated rationale belongs in the [decision catalog](../Documents/Decisions/README.md), while present standing belongs in [Progress](../Documents/Project/Progress.md).')
$RootLines.Add('')
$RootLines.Add('The [machine-readable catalog](Specification-Catalog.json) contains every Markdown specification, its full path, title, domain, normalized search status, and original opening status. A normalized status is only a filter; the specification text remains authoritative.')
$RootLines.Add('')
$RootLines.Add('## Common starting points')
$RootLines.Add('')
$RootLines.Add('| Entry point | Use it for | It does not by itself prove |')
$RootLines.Add('| --- | --- | --- |')
$RootLines.Add('| [Windvale Language 1.0](Windvale-Language-1.0.md) | Accepted source semantics and rule ownership. | That every feature is implemented, qualified, or released. |')
$RootLines.Add('| [Seed language](Seed-Language.md) | The smaller working bootstrap and recovery language. | The complete Language 1.0 design. |')
$RootLines.Add('| [Seed bytecode](Seed-Bytecode.md) | WVB encoding, verification, instructions, versions, and limits. | That every execution consumer supports every minor version. |')
$RootLines.Add('| [Object format](Windvale-Object-Format.md), [assembly](Windvale-Assembly.md), and [linking](Windvale-Linking.md) | Native object construction and deterministic image production. | An executable container, loader policy, or host ABI. |')
$RootLines.Add('| [Native verification owners](Windvale-Native-Verification-Owners.md) | The focused owner for a native behavior or boundary. | Qualification outside the selected owner and host. |')
$RootLines.Add('| [Local database service](Windvale-Database-Local-Service.md) | Sequential local WVDB sessions and exact completion behavior. | Networking, concurrent writers, or safe replay of uncertain mutations. |')
$RootLines.Add('| [OS boot environment](Windvale-Os-Boot-Environment.md) | Pinned emulator and firmware preflight. | Firmware entry, kernel handoff, or a working OS. |')
$RootLines.Add('')
$RootLines.Add('## Browse by domain')
$RootLines.Add('')
$RootLines.Add('| Domain | What it covers | Specifications |')
$RootLines.Add('| --- | --- | ---: |')
foreach ($Domain in $Domains) {
    $Count = @($SpecificationEntries | Where-Object domain -eq $Domain.Key).Count
    $RootLines.Add(
        "| [$($Domain.Title)](Indexes/$($Domain.Key).md) | " +
        "$($Domain.Description) | $Count |")
}
$RootLines.Add('')
$RootLines.Add('## Catalog status')
$RootLines.Add('')
$RootLines.Add('| Search status | Specifications |')
$RootLines.Add('| --- | ---: |')
foreach ($Group in $StatusCounts) {
    $RootLines.Add("| $($Group.Name) | $($Group.Count) |")
}
$RootLines.Add('')
$RootLines.Add('`Unclassified` means an older specification has no recognized opening status. It is a prompt to inspect the document and its decisions, not an acceptance claim. New specifications must state their status explicitly.')
Write-Or-CheckGeneratedFile `
    -Path (Join-Path $SpecificationDirectory 'README.md') `
    -Text ($RootLines -join "`n")

foreach ($Domain in $Domains) {
    $Entries = @($SpecificationEntries | Where-Object domain -eq $Domain.Key)
    $Lines = [System.Collections.Generic.List[string]]::new()
    $Lines.Add("# $($Domain.Title) specifications")
    $Lines.Add('')
    $Lines.Add('> Generated by `Tools/Documentation/Update-Documentation-Catalogs.ps1`; do not edit by hand.')
    $Lines.Add('')
    $Lines.Add($Domain.Description)
    $Lines.Add('')
    $Lines.Add('[Back to the specification index](../README.md).')
    $Lines.Add('')
    $Lines.Add('| Specification | Search status |')
    $Lines.Add('| --- | --- |')
    foreach ($Entry in $Entries) {
        $Title = ConvertTo-MarkdownCell $Entry.title
        $Lines.Add("| [$Title](../$($Entry.key).md) | $($Entry.status) |")
    }
    Write-Or-CheckGeneratedFile `
        -Path (Join-Path $SpecificationDirectory "Indexes/$($Domain.Key).md") `
        -Text ($Lines -join "`n")
}

$DecisionEntries = @(
    foreach ($File in Get-ChildItem -LiteralPath $DecisionDirectory -File -Filter '*.md' |
        Where-Object { $_.Name -ne 'README.md' } |
        Sort-Object Name) {
        if ($File.Name -notmatch '^(?<number>[0-9]{4})-.+\.md$') {
            throw "Decision '$($File.Name)' does not start with a four-digit number."
        }
        $Number = $Matches['number']
        $Lines = @(Get-Content -LiteralPath $File.FullName)
        $Title = Get-DocumentTitle $Lines
        if ([string]::IsNullOrWhiteSpace($Title)) {
            throw "Decision '$($File.Name)' has no level-one title."
        }
        $OpeningStatus = Get-OpeningStatus $Lines
        $Date = $null
        foreach ($Line in $Lines | Select-Object -First 30) {
            if ($Line -match '^[-*]\s+Date:\s*([0-9]{4}-[0-9]{2}-[0-9]{2})\s*$') {
                $Date = $Matches[1]
                break
            }
        }
        [pscustomobject][ordered]@{
            key = $File.BaseName
            number = $Number
            path = "Documents/Decisions/$($File.Name)"
            title = $Title
            date = $Date
            status = Get-DecisionStatusCategory $OpeningStatus
            statusText = if ($OpeningStatus.Length -eq 0) { $null } else { $OpeningStatus }
        }
    }
)

$DecisionJson = ConvertTo-CompactCatalogJson $DecisionEntries
Write-Or-CheckGeneratedFile `
    -Path (Join-Path $DecisionDirectory 'Decision-Catalog.json') `
    -Text $DecisionJson

$DecisionLines = [System.Collections.Generic.List[string]]::new()
$DecisionLines.Add('# Windvale decision catalog')
$DecisionLines.Add('')
$DecisionLines.Add('> Generated by `Tools/Documentation/Update-Documentation-Catalogs.ps1`; do not edit by hand.')
$DecisionLines.Add('')
$DecisionLines.Add('Decisions explain why durable choices were made. They preserve history; they are not a substitute for current specifications or Progress. Search the [machine-readable catalog](Decision-Catalog.json) by title, full key, number, or status, then open the exact record.')
$DecisionLines.Add('')
$DecisionLines.Add('The normalized status is a search aid derived from the opening status. The copied `statusText` remains the more exact claim. A number is not a unique key because twelve early number collisions are intentionally preserved.')
$DecisionLines.Add('')
$DecisionLines.Add('## Status summary')
$DecisionLines.Add('')
$DecisionLines.Add('| Search status | Decisions |')
$DecisionLines.Add('| --- | ---: |')
foreach ($Group in $DecisionEntries | Group-Object status | Sort-Object Name) {
    $DecisionLines.Add("| $($Group.Name) | $($Group.Count) |")
}

$NeedsAttention = @($DecisionEntries | Where-Object status -in @('Proposed', 'Unclassified'))
$DecisionLines.Add('')
$DecisionLines.Add('## Open or unclassified records')
$DecisionLines.Add('')
if ($NeedsAttention.Count -eq 0) {
    $DecisionLines.Add('None.')
} else {
    foreach ($Entry in $NeedsAttention) {
        $Title = ConvertTo-MarkdownCell $Entry.title
        $DecisionLines.Add("- [$Title]($($Entry.key).md) — $($Entry.status)")
    }
}

$DecisionLines.Add('')
$DecisionLines.Add('## Recently numbered records')
$DecisionLines.Add('')
$DecisionLines.Add('| Decision | Search status |')
$DecisionLines.Add('| --- | --- |')
foreach ($Entry in $DecisionEntries |
    Sort-Object @{ Expression = { [int]$_.number }; Descending = $true }, key |
    Select-Object -First 25) {
    $Title = ConvertTo-MarkdownCell $Entry.title
    $DecisionLines.Add("| [$Title]($($Entry.key).md) | $($Entry.status) |")
}

$Replaced = @($DecisionEntries | Where-Object status -in @('Superseded', 'Historical'))
$DecisionLines.Add('')
$DecisionLines.Add('## Superseded or historical records')
$DecisionLines.Add('')
foreach ($Entry in $Replaced) {
    $Title = ConvertTo-MarkdownCell $Entry.title
    $DecisionLines.Add("- [$Title]($($Entry.key).md) — $($Entry.status)")
}
$DecisionLines.Add('')
$DecisionLines.Add('See [`Legacy-Id-Collisions.txt`](Legacy-Id-Collisions.txt) for the frozen list of duplicated early numbers.')
Write-Or-CheckGeneratedFile `
    -Path (Join-Path $DecisionDirectory 'README.md') `
    -Text ($DecisionLines -join "`n")

if ($Failures.Count -ne 0) {
    foreach ($Failure in $Failures) {
        Write-Error $Failure -ErrorAction Continue
    }
    throw "Documentation catalog check failed with $($Failures.Count) issue(s)."
}

if ($Check) {
    Write-Host (
        'documentation catalogs status=Current ' +
        "specifications=$($SpecificationEntries.Count) " +
        "decisions=$($DecisionEntries.Count) domains=$($Domains.Count)")
} else {
    Write-Host (
        'documentation catalogs status=Updated ' +
        "files=$Written specifications=$($SpecificationEntries.Count) " +
        "decisions=$($DecisionEntries.Count) domains=$($Domains.Count)")
}
