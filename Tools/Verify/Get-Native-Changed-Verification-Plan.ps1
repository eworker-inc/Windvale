[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [AllowEmptyCollection()]
    [string[]]$ChangedPath,
    [switch]$PassThru,
    [switch]$Quiet,
    [hashtable]$InitializationCache
)

$ErrorActionPreference = 'Stop'
$InitializationCacheKey = 'native-changed-verification-plan-v1'
$InitializationVariables = if (
    $null -ne $InitializationCache -and
    $InitializationCache.ContainsKey($InitializationCacheKey)) {
    $InitializationCache[$InitializationCacheKey]
} else {
    $null
}
if ($null -ne $InitializationVariables) {
    foreach ($Entry in $InitializationVariables.GetEnumerator()) {
        Set-Variable -Name $Entry.Key -Value $Entry.Value
    }
} else {
    $InitializationVariableNamesBefore = if ($null -ne $InitializationCache) {
        [System.Collections.Generic.HashSet[string]]::new(
            [string[]]@(Get-Variable | ForEach-Object Name),
            [StringComparer]::Ordinal)
    } else {
        $null
    }
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$SuitePlanPath = Join-Path $RepositoryRoot 'Tests/Native/Verification-Owners.txt'
$DurationPlanPath = Join-Path $RepositoryRoot `
    'Tests/Native/Verification-Duration-Profiles.txt'
$SelectedSuites = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$Gaps = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$RunPlanVerification = $false
$RunWebAssemblyVerification = $false
$RunWebAssemblyEngineVerification = $false
$RunGitHubQualificationVerification = $false
$SourceContainmentCompilerDevelopmentEligible = $true
$OsX64CodeEmissionDevelopmentEligible = $true
$OsX64CodeEmissionDevelopmentRequiresAllTargets = $false
$SelectedOsX64CodeEmissionDevelopmentTargets =
    [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$OsX64CodeEmissionDevelopmentTargetsByPath = @{}
$OsX64CodeEmissionDevelopmentPlan = Join-Path $RepositoryRoot `
    'Tests/Native/Os-X64-Code-Emission-Development-Targets.txt'
$OsX64CodeEmissionDevelopmentLines = if (
    Test-Path -LiteralPath $OsX64CodeEmissionDevelopmentPlan -PathType Leaf) {
    @(Get-Content -LiteralPath $OsX64CodeEmissionDevelopmentPlan)
} else {
    @()
}
$OsX64CodeEmissionDevelopmentTargetNames =
    [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$OsX64CodeEmissionDevelopmentTargetProjects =
    [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$OsX64CodeEmissionDevelopmentTargetArtifacts =
    [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$OsX64CodeEmissionDevelopmentExpectedExit = 50
if ($OsX64CodeEmissionDevelopmentLines.Count -ne 57 -or
    $OsX64CodeEmissionDevelopmentLines[0] -ne
        'windvale-os-x64-code-emission-development-targets 2') {
    $OsX64CodeEmissionDevelopmentEligible = $false
}
foreach ($Line in @($OsX64CodeEmissionDevelopmentLines | Select-Object -Skip 1)) {
    $Fields = $Line.Split('|')
    if ($Fields.Count -lt 16 -or $Fields.Count -gt 17 -or
        $Fields[0] -notmatch '^[a-z0-9][a-z0-9-]*$' -or
        $Fields[1] -notmatch
            '^Projects/Tests/Windvale-Native-Test-Os-X64-.+-Emission\.wvproj$' -or
        $Fields[2] -notmatch '^[A-Za-z][A-Za-z0-9]*$' -or
        $Fields[3] -ne [string]$OsX64CodeEmissionDevelopmentExpectedExit -or
        @($Fields[4], $Fields[6], $Fields[8], $Fields[10], $Fields[12] |
            Where-Object { $_ -notmatch '^[0-9]+$' }).Count -ne 0 -or
        @($Fields[5], $Fields[7], $Fields[9], $Fields[11], $Fields[13] |
            Where-Object { $_ -notmatch '^[0-9a-f]{64}$' }).Count -ne 0 -or
        !$OsX64CodeEmissionDevelopmentTargetNames.Add($Fields[0]) -or
        !$OsX64CodeEmissionDevelopmentTargetProjects.Add($Fields[1]) -or
        !$OsX64CodeEmissionDevelopmentTargetArtifacts.Add($Fields[2])) {
        $OsX64CodeEmissionDevelopmentEligible = $false
        continue
    }
    $OsX64CodeEmissionDevelopmentExpectedExit++
    $TargetName = $Fields[0]
    $TargetPaths = @($Fields[1]) + @($Fields | Select-Object -Skip 14)
    foreach ($TargetPath in $TargetPaths) {
        if (!$OsX64CodeEmissionDevelopmentTargetsByPath.ContainsKey($TargetPath)) {
            $OsX64CodeEmissionDevelopmentTargetsByPath[$TargetPath] =
                [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::Ordinal)
        }
        $null = $OsX64CodeEmissionDevelopmentTargetsByPath[$TargetPath].Add(
            $TargetName)
    }
}
$LibraryDevelopmentEligible = $true
$LibraryDevelopmentRequiresAllTargets = $false
$SelectedLibraryDevelopmentTargets =
    [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$LibraryDevelopmentTargetsByPath = @{}
$LibraryDevelopmentPlan = Join-Path $RepositoryRoot `
    'Tests/Native/Library-Development-Targets.txt'
$LibraryDevelopmentLines = if (
    Test-Path -LiteralPath $LibraryDevelopmentPlan -PathType Leaf) {
    @(Get-Content -LiteralPath $LibraryDevelopmentPlan)
} else {
    @()
}
$LibraryDevelopmentTargetNames =
    [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$LibraryDevelopmentTargetProjects =
    [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$LibraryDevelopmentKindCounts = @{ project = 0; conformance = 0; negative = 0 }
if ($LibraryDevelopmentLines.Count -ne 30 -or
    $LibraryDevelopmentLines[0] -ne 'windvale-library-development-targets 1') {
    $LibraryDevelopmentEligible = $false
}
foreach ($Line in @($LibraryDevelopmentLines | Select-Object -Skip 1)) {
    $Fields = $Line.Split('|')
    if ($Fields.Count -ne 3 -or
        $Fields[0] -notmatch '^[a-z0-9][a-z0-9-]*$' -or
        !$LibraryDevelopmentKindCounts.ContainsKey($Fields[1]) -or
        $Fields[2] -notmatch
            '^(?:Projects/Libraries|Projects/Tests|Tests/Fixtures/Libraries)/.+\.wvproj$' -or
        !$LibraryDevelopmentTargetProjects.Add($Fields[2])) {
        $LibraryDevelopmentEligible = $false
        continue
    }
    $TargetName = $Fields[0]
    $ExpectedKind = if ($Fields[2].StartsWith(
            'Projects/Libraries/', [StringComparison]::Ordinal) -or
        $Fields[2].EndsWith('-Import-Smoke.wvproj', [StringComparison]::Ordinal)) {
        'project'
    } elseif ($Fields[2].StartsWith(
            'Projects/Tests/', [StringComparison]::Ordinal)) {
        'conformance'
    } else {
        'negative'
    }
    if ($Fields[1] -ne $ExpectedKind) {
        $LibraryDevelopmentEligible = $false
        continue
    }
    $null = $LibraryDevelopmentTargetNames.Add($TargetName)
    $LibraryDevelopmentKindCounts[$Fields[1]]++
    $TargetPaths = @($Fields[2])
    $TargetProjectAbsolute = Join-Path $RepositoryRoot $Fields[2]
    if (!(Test-Path -LiteralPath $TargetProjectAbsolute -PathType Leaf)) {
        $LibraryDevelopmentEligible = $false
        continue
    }
    $TargetPaths += @(
        Get-Content -LiteralPath $TargetProjectAbsolute |
            ForEach-Object {
                if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                    $Matches[1]
                }
            }
    )
    foreach ($TargetPath in $TargetPaths) {
        if (!$LibraryDevelopmentTargetsByPath.ContainsKey($TargetPath)) {
            $LibraryDevelopmentTargetsByPath[$TargetPath] =
                [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::Ordinal)
        }
        $null = $LibraryDevelopmentTargetsByPath[$TargetPath].Add($TargetName)
    }
}
if (!$LibraryDevelopmentTargetNames.SetEquals([string[]]@(
        'capability-rejections',
        'durability',
        'models',
        'page-storage',
        'read-only-wvdb',
        'resource-store',
        'storage-geometry'
    )) -or
    $LibraryDevelopmentKindCounts.project -ne 19 -or
    $LibraryDevelopmentKindCounts.conformance -ne 8 -or
    $LibraryDevelopmentKindCounts.negative -ne 2) {
    $LibraryDevelopmentEligible = $false
}
$LibraryDevelopmentContractTargets = @{
    'Specifications/Read-Only-Directory-Capability.md' = 'read-only-wvdb'
    'Specifications/Random-Access-Storage-Capability.md' = 'page-storage'
    'Specifications/Windvale-Database-Durable-Commit.md' = 'durability'
    'Specifications/Windvale-Database-Durable-Superblock.md' = 'durability'
    'Specifications/Windvale-Database-Reader.md' = 'read-only-wvdb'
    'Specifications/Windvale-Database-Storage-Page.md' = 'page-storage'
    'Specifications/Windvale-Model-Protocol.md' = 'models'
}
$DatabaseStorageDevelopmentEligible = $true
$DatabaseDevelopmentRequiresAllTargets = $false
$SelectedDatabaseDevelopmentTargets = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$DatabaseDevelopmentPaths = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$DatabaseDevelopmentTargetProjects = [ordered]@{
    'tree-node' = 'Projects/Tests/Windvale-Native-Test-Database-Tree-Node.wvproj'
    'logical-record' = 'Projects/Tests/Windvale-Native-Test-Database-Logical-Record.wvproj'
    'typed-row' = 'Projects/Tests/Windvale-Native-Test-Database-Typed-Row.wvproj'
    'transaction-mutations' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Mutations.wvproj'
    'transaction-leaf-rewrite' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Rewrite.wvproj'
    'transaction-paths' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Paths.wvproj'
    'transaction-leaf-groups' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Groups.wvproj'
    'transaction-leaf-partition' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Partition.wvproj'
    'transaction-leaf-pages' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Pages.wvproj'
    'transaction-branch-partition' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Partition.wvproj'
    'transaction-parent-groups' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Parent-Groups.wvproj'
    'transaction-branch-pages' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages.wvproj'
    'transaction-ancestor-groups' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups.wvproj'
    'transaction-ancestor-pages' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages.wvproj'
    'transaction-root-growth' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth.wvproj'
    'transaction-tree-completion' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Tree-Completion.wvproj'
    'transaction-commit' = 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Commit.wvproj'
    'query-ir' = 'Projects/Tests/Windvale-Native-Test-Database-Query-Ir.wvproj'
    'sql-lowerer' = 'Projects/Tests/Windvale-Native-Test-Database-Sql-Lowerer.wvproj'
    'json-value' = 'Projects/Tests/Windvale-Native-Test-Database-Json-Value.wvproj'
    'json-protocol' = 'Projects/Tests/Windvale-Native-Test-Database-Json-Protocol.wvproj'
    'local-service' = 'Projects/Tests/Windvale-Native-Test-Local-Database-Service.wvproj'
    'collection-catalog' = 'Projects/Tests/Windvale-Native-Test-Database-Collection-Catalog.wvproj'
    'bootstrap' = 'Projects/Tests/Windvale-Native-Test-Database-Bootstrap.wvproj'
    'single-leaf' = 'Projects/Tests/Windvale-Native-Test-Database-Single-Leaf-Upsert.wvproj'
    'branch-split' = 'Projects/Tests/Windvale-Native-Test-Database-Branch-Split.wvproj'
    'root-split' = 'Projects/Tests/Windvale-Native-Test-Database-Root-Split.wvproj'
    'depth-two' = 'Projects/Tests/Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj'
    'depth-three' = 'Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Root-Growth.wvproj'
    'depth-three-upsert' = 'Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Upsert.wvproj'
    'tree-path-upsert' = 'Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Upsert.wvproj'
    'tree-path-delete' = 'Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Delete.wvproj'
    'host-storage' = 'Projects/Tests/Windvale-Native-Test-Database-Host-Storage.wvproj'
    'host-root-writer' = 'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Writer.wvproj'
    'host-local-service' = 'Projects/Tests/Windvale-Native-Test-Database-Host-Local-Put.wvproj'
    'host-tree-reader' = 'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Reader.wvproj'
    'host-tree-delete' = 'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Delete.wvproj'
    'host-tree-scan' = 'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Scan.wvproj'
    'engine' = 'Projects/Tests/Windvale-Native-Test-Database-Engine.wvproj'
    'host-tree-writer' = 'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Writer.wvproj'
    'persistent-transaction-writer' = 'Projects/Tests/Windvale-Native-Test-Database-Persistent-Transaction-Writer.wvproj'
}
$DatabaseDevelopmentTargetsByPath = @{}
foreach ($TargetEntry in $DatabaseDevelopmentTargetProjects.GetEnumerator()) {
    $TargetPaths = @($TargetEntry.Value)
    $TargetProjectAbsolute = Join-Path $RepositoryRoot $TargetEntry.Value
    if (!(Test-Path -LiteralPath $TargetProjectAbsolute -PathType Leaf)) {
        $DatabaseStorageDevelopmentEligible = $false
        continue
    }
    $TargetPaths += @(
        Get-Content -LiteralPath $TargetProjectAbsolute |
            ForEach-Object {
                if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                    $Matches[1]
                }
            }
    )
    foreach ($TargetPath in $TargetPaths) {
        if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($TargetPath)) {
            $DatabaseDevelopmentTargetsByPath[$TargetPath] =
                [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::Ordinal)
        }
        $null = $DatabaseDevelopmentTargetsByPath[$TargetPath].Add($TargetEntry.Key)
    }
}
foreach ($TreeLeafProject in @(
    'Projects/Libraries/Windvale-Library-Database-Tree-Node.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Tree-Leaf-Scan.wvproj'
)) {
    if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($TreeLeafProject)) {
        $DatabaseDevelopmentTargetsByPath[$TreeLeafProject] =
            [System.Collections.Generic.HashSet[string]]::new(
                [StringComparer]::Ordinal)
    }
    $null = $DatabaseDevelopmentTargetsByPath[$TreeLeafProject].Add('tree-node')
}
$TransactionLeafGroupsProject =
    'Projects/Libraries/Windvale-Library-Database-Transaction-Leaf-Groups.wvproj'
if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($TransactionLeafGroupsProject)) {
    $DatabaseDevelopmentTargetsByPath[$TransactionLeafGroupsProject] =
        [System.Collections.Generic.HashSet[string]]::new(
            [StringComparer]::Ordinal)
}
$null = $DatabaseDevelopmentTargetsByPath[$TransactionLeafGroupsProject].Add(
    'transaction-leaf-groups')
$TransactionLeafPartitionProject =
    'Projects/Libraries/Windvale-Library-Database-Transaction-Leaf-Partition.wvproj'
if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($TransactionLeafPartitionProject)) {
    $DatabaseDevelopmentTargetsByPath[$TransactionLeafPartitionProject] =
        [System.Collections.Generic.HashSet[string]]::new(
            [StringComparer]::Ordinal)
}
$null = $DatabaseDevelopmentTargetsByPath[$TransactionLeafPartitionProject].Add(
    'transaction-leaf-partition')
$TransactionLeafPagesProject =
    'Projects/Libraries/Windvale-Library-Database-Transaction-Leaf-Pages.wvproj'
if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($TransactionLeafPagesProject)) {
    $DatabaseDevelopmentTargetsByPath[$TransactionLeafPagesProject] =
        [System.Collections.Generic.HashSet[string]]::new(
            [StringComparer]::Ordinal)
}
$null = $DatabaseDevelopmentTargetsByPath[$TransactionLeafPagesProject].Add(
    'transaction-leaf-pages')
$TransactionParentGroupsProject =
    'Projects/Libraries/Windvale-Library-Database-Transaction-Parent-Groups.wvproj'
if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($TransactionParentGroupsProject)) {
    $DatabaseDevelopmentTargetsByPath[$TransactionParentGroupsProject] =
        [System.Collections.Generic.HashSet[string]]::new(
            [StringComparer]::Ordinal)
}
$null = $DatabaseDevelopmentTargetsByPath[$TransactionParentGroupsProject].Add(
    'transaction-parent-groups')
$TransactionBranchPagesProjects = @(
    'Projects/Libraries/Windvale-Library-Database-Transaction-Branch-Pages.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages-Validation.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages-Depth-Three.wvproj'
)
foreach ($TransactionBranchPagesProject in $TransactionBranchPagesProjects) {
    $TransactionBranchPagesPaths = @($TransactionBranchPagesProject)
    $TransactionBranchPagesAbsolute =
        Join-Path $RepositoryRoot $TransactionBranchPagesProject
    if (Test-Path -LiteralPath $TransactionBranchPagesAbsolute -PathType Leaf) {
        $TransactionBranchPagesPaths += @(
            Get-Content -LiteralPath $TransactionBranchPagesAbsolute |
                ForEach-Object {
                    if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                        $Matches[1]
                    }
                }
        )
    }
    foreach ($TransactionBranchPagesPath in $TransactionBranchPagesPaths) {
        if (!$DatabaseDevelopmentTargetsByPath.ContainsKey(
                $TransactionBranchPagesPath)) {
            $DatabaseDevelopmentTargetsByPath[$TransactionBranchPagesPath] =
                [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::Ordinal)
        }
        $null = $DatabaseDevelopmentTargetsByPath[$TransactionBranchPagesPath].Add(
            'transaction-branch-pages')
    }
}
$TransactionAncestorGroupsProjects = @(
    'Projects/Libraries/Windvale-Library-Database-Transaction-Ancestor-Groups.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups-Depth-Four.wvproj'
)
foreach ($TransactionAncestorGroupsProject in $TransactionAncestorGroupsProjects) {
    $TransactionAncestorGroupsPaths = @($TransactionAncestorGroupsProject)
    $TransactionAncestorGroupsAbsolute =
        Join-Path $RepositoryRoot $TransactionAncestorGroupsProject
    if (Test-Path -LiteralPath $TransactionAncestorGroupsAbsolute -PathType Leaf) {
        $TransactionAncestorGroupsPaths += @(
            Get-Content -LiteralPath $TransactionAncestorGroupsAbsolute |
                ForEach-Object {
                    if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                        $Matches[1]
                    }
                }
        )
    }
    foreach ($TransactionAncestorGroupsPath in $TransactionAncestorGroupsPaths) {
        if (!$DatabaseDevelopmentTargetsByPath.ContainsKey(
                $TransactionAncestorGroupsPath)) {
            $DatabaseDevelopmentTargetsByPath[$TransactionAncestorGroupsPath] =
                [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::Ordinal)
        }
        $null = $DatabaseDevelopmentTargetsByPath[$TransactionAncestorGroupsPath].Add(
            'transaction-ancestor-groups')
    }
}
$TransactionAncestorPagesProjects = @(
    'Projects/Libraries/Windvale-Library-Database-Transaction-Ancestor-Pages.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages-Intermediate.wvproj'
)
foreach ($TransactionAncestorPagesProject in $TransactionAncestorPagesProjects) {
    $TransactionAncestorPagesPaths = @($TransactionAncestorPagesProject)
    $TransactionAncestorPagesAbsolute =
        Join-Path $RepositoryRoot $TransactionAncestorPagesProject
    if (Test-Path -LiteralPath $TransactionAncestorPagesAbsolute -PathType Leaf) {
        $TransactionAncestorPagesPaths += @(
            Get-Content -LiteralPath $TransactionAncestorPagesAbsolute |
                ForEach-Object {
                    if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                        $Matches[1]
                    }
                }
        )
    }
    foreach ($TransactionAncestorPagesPath in $TransactionAncestorPagesPaths) {
        if (!$DatabaseDevelopmentTargetsByPath.ContainsKey(
                $TransactionAncestorPagesPath)) {
            $DatabaseDevelopmentTargetsByPath[$TransactionAncestorPagesPath] =
                [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::Ordinal)
        }
        $null = $DatabaseDevelopmentTargetsByPath[$TransactionAncestorPagesPath].Add(
            'transaction-ancestor-pages')
    }
}
$TransactionRootGrowthProjects = @(
    'Projects/Libraries/Windvale-Library-Database-Transaction-Root-Growth.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth-Multi-Level.wvproj'
)
foreach ($TransactionRootGrowthProject in $TransactionRootGrowthProjects) {
    $TransactionRootGrowthPaths = @($TransactionRootGrowthProject)
    $TransactionRootGrowthAbsolute =
        Join-Path $RepositoryRoot $TransactionRootGrowthProject
    if (Test-Path -LiteralPath $TransactionRootGrowthAbsolute -PathType Leaf) {
        $TransactionRootGrowthPaths += @(
            Get-Content -LiteralPath $TransactionRootGrowthAbsolute |
                ForEach-Object {
                    if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                        $Matches[1]
                    }
                }
        )
    }
    foreach ($TransactionRootGrowthPath in $TransactionRootGrowthPaths) {
        if (!$DatabaseDevelopmentTargetsByPath.ContainsKey(
                $TransactionRootGrowthPath)) {
            $DatabaseDevelopmentTargetsByPath[$TransactionRootGrowthPath] =
                [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::Ordinal)
        }
        $null = $DatabaseDevelopmentTargetsByPath[$TransactionRootGrowthPath].Add(
            'transaction-root-growth')
    }
}
$TransactionTreeCompletionProjects = @(
    'Projects/Libraries/Windvale-Library-Database-Transaction-Tree-Completion.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Tree-Completion-Root-Growth.wvproj'
)
foreach ($TransactionTreeCompletionProject in $TransactionTreeCompletionProjects) {
    $TransactionTreeCompletionPaths = @($TransactionTreeCompletionProject)
    $TransactionTreeCompletionAbsolute =
        Join-Path $RepositoryRoot $TransactionTreeCompletionProject
    if (Test-Path -LiteralPath $TransactionTreeCompletionAbsolute -PathType Leaf) {
        $TransactionTreeCompletionPaths += @(
            Get-Content -LiteralPath $TransactionTreeCompletionAbsolute |
                ForEach-Object {
                    if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                        $Matches[1]
                    }
                }
        )
    }
    foreach ($TransactionTreeCompletionPath in $TransactionTreeCompletionPaths) {
        if (!$DatabaseDevelopmentTargetsByPath.ContainsKey(
                $TransactionTreeCompletionPath)) {
            $DatabaseDevelopmentTargetsByPath[$TransactionTreeCompletionPath] =
                [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::Ordinal)
        }
        $null = $DatabaseDevelopmentTargetsByPath[$TransactionTreeCompletionPath].Add(
            'transaction-tree-completion')
    }
}
$TransactionCommitBoundaryPaths = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
foreach ($TransactionCommitPath in @(
    'Libraries/Database/Commit-Batch.wv',
    'Libraries/Database/Transaction-Commit.wv',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Commit.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Commit-Batch-Capacity.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Commit.wvproj',
    'Tests/Fixtures/Database/Database-Commit-Batch-Capacity-Self-Test.wv',
    'Tests/Fixtures/Database/Database-Transaction-Commit-Self-Test.wv',
    'Specifications/Windvale-Database-Transaction-Commit.md'
)) {
    $null = $TransactionCommitBoundaryPaths.Add($TransactionCommitPath)
    if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($TransactionCommitPath)) {
        $DatabaseDevelopmentTargetsByPath[$TransactionCommitPath] =
            [System.Collections.Generic.HashSet[string]]::new(
                [StringComparer]::Ordinal)
    }
    $null = $DatabaseDevelopmentTargetsByPath[$TransactionCommitPath].Add(
        'transaction-commit')
}
foreach ($DatabaseDevelopmentPath in @($DatabaseDevelopmentTargetsByPath.Keys)) {
    if (!$TransactionCommitBoundaryPaths.Contains($DatabaseDevelopmentPath)) {
        $null = $DatabaseDevelopmentTargetsByPath[$DatabaseDevelopmentPath].Remove(
            'transaction-commit')
    }
}
$PersistentTransactionWriterBoundaryPaths =
    [System.Collections.Generic.HashSet[string]]::new(
        [StringComparer]::Ordinal)
foreach ($PersistentTransactionWriterPath in @(
    'Libraries/Platform/Database/Durable-Transaction-Writer.wv',
    'Libraries/Platform/Database/Durable-Persistent-Transaction-Writer.wv',
    'Projects/Libraries/Windvale-Library-Durable-Transaction-Writer.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Persistent-Transaction-Writer.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Persistent-Transaction-Writer.wvproj',
    'Tests/Fixtures/Database/Native-Hosted-Persistent-Transaction-Writer-Self-Test.wv',
    'Specifications/Windvale-Database-Persistent-Transaction-Writer.md'
)) {
    $null = $PersistentTransactionWriterBoundaryPaths.Add(
        $PersistentTransactionWriterPath)
    if (!$DatabaseDevelopmentTargetsByPath.ContainsKey(
            $PersistentTransactionWriterPath)) {
        $DatabaseDevelopmentTargetsByPath[$PersistentTransactionWriterPath] =
            [System.Collections.Generic.HashSet[string]]::new(
                [StringComparer]::Ordinal)
    }
    $null = $DatabaseDevelopmentTargetsByPath[
        $PersistentTransactionWriterPath].Add('persistent-transaction-writer')
}
foreach ($DatabaseDevelopmentPath in @($DatabaseDevelopmentTargetsByPath.Keys)) {
    if (!$PersistentTransactionWriterBoundaryPaths.Contains(
            $DatabaseDevelopmentPath)) {
        $null = $DatabaseDevelopmentTargetsByPath[
            $DatabaseDevelopmentPath].Remove('persistent-transaction-writer')
    }
}
$DurableTreeScanProject =
    'Projects/Libraries/Windvale-Library-Durable-Tree-Scan.wvproj'
if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($DurableTreeScanProject)) {
    $DatabaseDevelopmentTargetsByPath[$DurableTreeScanProject] =
        [System.Collections.Generic.HashSet[string]]::new(
            [StringComparer]::Ordinal)
}
$null = $DatabaseDevelopmentTargetsByPath[$DurableTreeScanProject].Add(
    'host-tree-scan')
foreach ($HostTreeDeleteProject in @(
    'Projects/Libraries/Windvale-Library-Database-Tree-Path-Delete.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Tree-Path.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Tree-Delete.wvproj'
)) {
    if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($HostTreeDeleteProject)) {
        $DatabaseDevelopmentTargetsByPath[$HostTreeDeleteProject] =
            [System.Collections.Generic.HashSet[string]]::new(
                [StringComparer]::Ordinal)
    }
    $null = $DatabaseDevelopmentTargetsByPath[$HostTreeDeleteProject].Add(
        'host-tree-delete')
}
$HostLocalGetProject =
    'Projects/Tests/Windvale-Native-Test-Database-Host-Local-Get.wvproj'
$HostLocalGetAbsolute = Join-Path $RepositoryRoot $HostLocalGetProject
if (Test-Path -LiteralPath $HostLocalGetAbsolute -PathType Leaf) {
    foreach ($TargetPath in @(
        $HostLocalGetProject
        Get-Content -LiteralPath $HostLocalGetAbsolute |
            ForEach-Object {
                if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                    $Matches[1]
                }
            }
    )) {
        if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($TargetPath)) {
            $DatabaseDevelopmentTargetsByPath[$TargetPath] =
                [System.Collections.Generic.HashSet[string]]::new(
                    [StringComparer]::Ordinal)
        }
        $null = $DatabaseDevelopmentTargetsByPath[$TargetPath].Add(
            'host-local-service')
    }
} else {
    $DatabaseStorageDevelopmentEligible = $false
}
$HostLogicalTreeProjects = @(
    'Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Writer.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj'
)
foreach ($HostLogicalTreeProject in $HostLogicalTreeProjects) {
    $HostLogicalTreeAbsolute = Join-Path $RepositoryRoot $HostLogicalTreeProject
    if (Test-Path -LiteralPath $HostLogicalTreeAbsolute -PathType Leaf) {
        foreach ($TargetPath in @(
            $HostLogicalTreeProject
            Get-Content -LiteralPath $HostLogicalTreeAbsolute |
                ForEach-Object {
                    if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                        $Matches[1]
                    }
                }
        )) {
            if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($TargetPath)) {
                $DatabaseDevelopmentTargetsByPath[$TargetPath] =
                    [System.Collections.Generic.HashSet[string]]::new(
                        [StringComparer]::Ordinal)
            }
            $null = $DatabaseDevelopmentTargetsByPath[$TargetPath].Add(
                'host-tree-writer')
        }
    } else {
        $DatabaseStorageDevelopmentEligible = $false
    }
}
$HostRootSplitProjects = @(
    'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Fill.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Split-Writer.wvproj'
)
foreach ($HostRootSplitProject in $HostRootSplitProjects) {
    $HostRootSplitAbsolute = Join-Path $RepositoryRoot $HostRootSplitProject
    if (Test-Path -LiteralPath $HostRootSplitAbsolute -PathType Leaf) {
        foreach ($TargetPath in @(
            $HostRootSplitProject
            Get-Content -LiteralPath $HostRootSplitAbsolute |
                ForEach-Object {
                    if ($_ -match '^(?:root|source) "([^"\r\n]+)"$') {
                        $Matches[1]
                    }
                }
        )) {
            if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($TargetPath)) {
                $DatabaseDevelopmentTargetsByPath[$TargetPath] =
                    [System.Collections.Generic.HashSet[string]]::new(
                        [StringComparer]::Ordinal)
            }
            $null = $DatabaseDevelopmentTargetsByPath[$TargetPath].Add(
                'host-root-writer')
        }
    } else {
        $DatabaseStorageDevelopmentEligible = $false
    }
}
foreach ($DatabasePerformancePath in @(
    'Tools/Database/Measure-Database-Comparison.ps1',
    'Tools/Database/SQLite-Durable-Cycle.py'
)) {
    if (!$DatabaseDevelopmentTargetsByPath.ContainsKey($DatabasePerformancePath)) {
        $DatabaseDevelopmentTargetsByPath[$DatabasePerformancePath] =
            [System.Collections.Generic.HashSet[string]]::new(
                [StringComparer]::Ordinal)
    }
    $null = $DatabaseDevelopmentTargetsByPath[$DatabasePerformancePath].Add(
        'host-root-writer')
}
$DatabaseDevelopmentContractTargets = @{
    'Specifications/Windvale-Database-Bootstrap.md' = @('bootstrap', 'engine')
    'Specifications/Windvale-Database-Collection-Catalog.md' = @('collection-catalog')
    'Specifications/Windvale-Database-Logical-Records.md' = @('logical-record')
    'Specifications/Windvale-Database-Typed-Rows-And-Schemas.md' = @('typed-row')
    'Specifications/Windvale-Database-Transaction-Mutations.md' = @('transaction-mutations')
    'Specifications/Windvale-Database-Transaction-Leaf-Rewrite.md' = @('transaction-leaf-rewrite')
    'Specifications/Windvale-Database-Transaction-Paths.md' = @('transaction-paths', 'transaction-ancestor-groups', 'transaction-ancestor-pages')
    'Specifications/Windvale-Database-Transaction-Leaf-Groups.md' = @('transaction-leaf-groups')
    'Specifications/Windvale-Database-Transaction-Leaf-Partition.md' = @('transaction-leaf-partition')
    'Specifications/Windvale-Database-Transaction-Leaf-Pages.md' = @('transaction-leaf-pages', 'transaction-parent-groups', 'transaction-branch-pages')
    'Specifications/Windvale-Database-Transaction-Child-Replacements.md' = @('transaction-branch-partition', 'transaction-parent-groups', 'transaction-branch-pages', 'transaction-ancestor-groups', 'transaction-ancestor-pages', 'transaction-root-growth', 'transaction-tree-completion')
    'Specifications/Windvale-Database-Transaction-Branch-Partition.md' = @('transaction-branch-partition', 'transaction-parent-groups', 'transaction-branch-pages', 'transaction-ancestor-groups', 'transaction-ancestor-pages', 'transaction-root-growth', 'transaction-tree-completion')
    'Specifications/Windvale-Database-Transaction-Parent-Groups.md' = @('transaction-parent-groups', 'transaction-branch-pages')
    'Specifications/Windvale-Database-Transaction-Branch-Pages.md' = @('transaction-branch-pages', 'transaction-tree-completion')
    'Specifications/Windvale-Database-Transaction-Ancestor-Groups.md' = @('transaction-ancestor-groups', 'transaction-ancestor-pages', 'transaction-tree-completion')
    'Specifications/Windvale-Database-Transaction-Ancestor-Pages.md' = @('transaction-ancestor-pages', 'transaction-tree-completion')
    'Specifications/Windvale-Database-Transaction-Root-Growth.md' = @('transaction-root-growth', 'transaction-tree-completion')
    'Specifications/Windvale-Database-Transaction-Tree-Completion.md' = @('transaction-tree-completion')
    'Specifications/Windvale-Database-Transaction-Commit.md' = @('transaction-commit')
    'Specifications/Windvale-Database-Query-Ir.md' = @('query-ir')
    'Specifications/Windvale-Database-Sql.md' = @('sql-lowerer')
    'Specifications/Windvale-Database-Json-Value.md' = @('json-value')
    'Specifications/Windvale-Database-Json-Protocol.md' = @('json-protocol')
    'Specifications/Windvale-Database-Local-Service.md' = @('local-service', 'host-local-service')
    'Specifications/Windvale-Database-Hosted-Local-Service.md' = @('host-local-service')
    'Specifications/Windvale-Database-Tree-Node.md' = @('tree-node')
    'Specifications/Windvale-Database-Tree-Leaf-Operations.md' = @('tree-node')
    'Specifications/Windvale-Database-Durable-Range-Scan.md' = @('host-tree-scan')
    'Specifications/Windvale-Database-Hosted-Tree-Delete.md' = @('host-tree-delete')
    'Specifications/Windvale-Database-Depth-Two-Upsert.md' = @('depth-two')
    'Specifications/Windvale-Database-Depth-Three-Root-Growth.md' = @('depth-three')
    'Specifications/Windvale-Database-Depth-Three-Upsert.md' = @('depth-three-upsert')
    'Specifications/Windvale-Database-Tree-Path-Upsert.md' = @('tree-path-upsert')
    'Specifications/Windvale-Database-Tree-Path-Delete.md' = @('tree-path-delete')
    'Specifications/Windvale-Database-Engine-Lifecycle.md' = @('engine')
    'Specifications/Windvale-Database-Hosted-Root-Writer.md' = @('host-root-writer')
    'Specifications/Windvale-Database-Hosted-Root-Split-Writer.md' = @('host-root-writer')
    'Specifications/Windvale-Database-Hosted-Tree-Writer.md' = @('host-tree-writer')
    'Specifications/Windvale-Database-Persistent-Transaction-Writer.md' = @('persistent-transaction-writer')
}
$DatabaseDevelopmentProjects = @(
    'Projects/Libraries/Windvale-Library-Database-Tree-Node.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Tree-Leaf-Scan.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Tree-Scan.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Tree-Path-Delete.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Tree-Path.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Tree-Delete.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Bootstrap.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Collection-Catalog.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Logical-Record.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Schema-Definition.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Typed-Row.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Mutations.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Leaf-Rewrite.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Paths.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Leaf-Groups.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Leaf-Partition.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Leaf-Pages.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Child-Replacements.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Branch-Partition.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Parent-Groups.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Branch-Pages.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Ancestor-Groups.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Ancestor-Pages.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Root-Growth.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Tree-Completion.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Transaction-Commit.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Query-Ir.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Sql-Lowerer.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Json-Value.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Json-Protocol.wvproj',
    'Projects/Libraries/Windvale-Library-Local-Database-Contracts.wvproj',
    'Projects/Libraries/Windvale-Library-Local-Database-Session.wvproj',
    'Projects/Libraries/Windvale-Library-Local-Database-Put.wvproj',
    'Projects/Libraries/Windvale-Library-Local-Database-Get.wvproj',
    'Projects/Libraries/Windvale-Library-Local-Database-Control.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Tree-Branch-Split.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Depth-Three-Root-Growth.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Depth-Three-Upsert.wvproj',
    'Projects/Libraries/Windvale-Library-Database-Tree-Path-Upsert.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Database-Engine.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Database-Bootstrap.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Database-Lifecycle.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Root-Writer.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Root-Split-Writer.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Local-Open.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Local-Root-Put.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Local-Get.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Tree-Writer.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Logical-Tree-Writer.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Transaction-Writer.wvproj',
    'Projects/Libraries/Windvale-Library-Durable-Persistent-Transaction-Writer.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Tree-Node.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Logical-Record.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Typed-Row.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Mutations.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Rewrite.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Paths.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Groups.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Partition.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Pages.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Partition.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Parent-Groups.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages-Validation.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages-Depth-Three.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups-Depth-Four.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages-Intermediate.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth-Multi-Level.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Tree-Completion.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Tree-Completion-Root-Growth.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Commit-Batch-Capacity.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Transaction-Commit.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Query-Ir.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Sql-Lowerer.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Json-Value.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Json-Protocol.wvproj',
    'Projects/Tests/Windvale-Native-Test-Local-Database-Service.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Collection-Catalog.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Bootstrap.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Single-Leaf-Upsert.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Branch-Split.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Root-Split.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Root-Growth.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Upsert.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Upsert.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Delete.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Storage.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Writer.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Fill.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Split-Writer.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Local-Put.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Local-Get.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Engine.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Reader.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Delete.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Scan.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Writer.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Writer.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj',
    'Projects/Tests/Windvale-Native-Test-Database-Persistent-Transaction-Writer.wvproj'
)
foreach ($ProjectPath in $DatabaseDevelopmentProjects) {
    $null = $DatabaseDevelopmentPaths.Add($ProjectPath)
    $ProjectAbsolute = Join-Path $RepositoryRoot $ProjectPath
    if (!(Test-Path -LiteralPath $ProjectAbsolute -PathType Leaf)) {
        $DatabaseStorageDevelopmentEligible = $false
        continue
    }
    foreach ($Line in Get-Content -LiteralPath $ProjectAbsolute) {
        if ($Line -match '^(?:root|source) "([^"\r\n]+)"$') {
            $null = $DatabaseDevelopmentPaths.Add($Matches[1])
        }
    }
}
foreach ($RuntimePath in @(
    'Runtime/Native/X64-Segmented-Hosted-Main-Trampoline.wva',
    'Runtime/Native/X64-Random-Access-Storage-Host.wva',
    'Runtime/Native/Windows-X64-Random-Access-Storage.wva',
    'Runtime/Native/Linux-X64-Random-Access-Storage.wva'
)) {
    $null = $DatabaseDevelopmentPaths.Add($RuntimePath)
}
foreach ($DatabasePerformancePath in @(
    'Tools/Database/Measure-Database-Comparison.ps1',
    'Tools/Database/SQLite-Durable-Cycle.py'
)) {
    $null = $DatabaseDevelopmentPaths.Add($DatabasePerformancePath)
}
foreach ($ContractPath in @(
    'Specifications/Windvale-Database-Bootstrap.md',
    'Specifications/Windvale-Database-Collection-Catalog.md',
    'Specifications/Windvale-Database-Logical-Records.md',
    'Specifications/Windvale-Database-Typed-Rows-And-Schemas.md',
    'Specifications/Windvale-Database-Transaction-Mutations.md',
    'Specifications/Windvale-Database-Transaction-Leaf-Rewrite.md',
    'Specifications/Windvale-Database-Transaction-Paths.md',
    'Specifications/Windvale-Database-Transaction-Leaf-Groups.md',
    'Specifications/Windvale-Database-Transaction-Leaf-Partition.md',
    'Specifications/Windvale-Database-Transaction-Leaf-Pages.md',
    'Specifications/Windvale-Database-Transaction-Child-Replacements.md',
    'Specifications/Windvale-Database-Transaction-Branch-Partition.md',
    'Specifications/Windvale-Database-Transaction-Parent-Groups.md',
    'Specifications/Windvale-Database-Transaction-Branch-Pages.md',
    'Specifications/Windvale-Database-Transaction-Ancestor-Groups.md',
    'Specifications/Windvale-Database-Transaction-Ancestor-Pages.md',
    'Specifications/Windvale-Database-Transaction-Root-Growth.md',
    'Specifications/Windvale-Database-Transaction-Tree-Completion.md',
    'Specifications/Windvale-Database-Transaction-Commit.md',
    'Specifications/Windvale-Database-Query-Ir.md',
    'Specifications/Windvale-Database-Sql.md',
    'Specifications/Windvale-Database-Json-Value.md',
    'Specifications/Windvale-Database-Json-Protocol.md',
    'Specifications/Windvale-Database-Local-Service.md',
    'Specifications/Windvale-Database-Tree-Node.md',
    'Specifications/Windvale-Database-Tree-Leaf-Operations.md',
    'Specifications/Windvale-Database-Durable-Range-Scan.md',
    'Specifications/Windvale-Database-Hosted-Tree-Delete.md',
    'Specifications/Windvale-Database-Depth-Two-Upsert.md',
    'Specifications/Windvale-Database-Depth-Three-Root-Growth.md',
    'Specifications/Windvale-Database-Depth-Three-Upsert.md',
    'Specifications/Windvale-Database-Tree-Path-Upsert.md',
    'Specifications/Windvale-Database-Tree-Path-Delete.md',
    'Specifications/Windvale-Database-Engine-Lifecycle.md',
    'Specifications/Windvale-Database-Hosted-Root-Writer.md',
    'Specifications/Windvale-Database-Hosted-Local-Service.md',
    'Specifications/Windvale-Database-Hosted-Tree-Writer.md',
    'Specifications/Windvale-Database-Persistent-Transaction-Writer.md'
)) {
    $null = $DatabaseDevelopmentPaths.Add($ContractPath)
}

$DurationPlanLines = @(Get-Content -LiteralPath $DurationPlanPath)
if ($DurationPlanLines.Count -lt 2 -or
    $DurationPlanLines[0] -ne
        'windvale-native-verification-duration-profiles 1') {
    throw 'The native verification duration-profile header differs.'
}
$DurationProfiles = @{}
foreach ($Line in $DurationPlanLines | Select-Object -Skip 1) {
    $Fields = $Line -split '\|', 4
    $ExpectedSeconds = 0
    $MaximumSeconds = 0
    $InfrastructureRetries = 0
    if ($Fields.Count -ne 4 -or
        $Fields[0] -cnotmatch '^[a-z]+(?:-[a-z]+)*$' -or
        $DurationProfiles.ContainsKey($Fields[0]) -or
        ![int]::TryParse($Fields[1], [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$ExpectedSeconds) -or
        ![int]::TryParse($Fields[2], [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$MaximumSeconds) -or
        ![int]::TryParse($Fields[3], [Globalization.NumberStyles]::None,
            [Globalization.CultureInfo]::InvariantCulture,
            [ref]$InfrastructureRetries) -or
        $ExpectedSeconds -lt 1 -or $ExpectedSeconds -gt 3600 -or
        $MaximumSeconds -lt $ExpectedSeconds -or $MaximumSeconds -gt 3600 -or
        $InfrastructureRetries -lt 0 -or $InfrastructureRetries -gt 1) {
        throw "Malformed native verification duration profile: $Line"
    }
    $DurationProfiles[$Fields[0]] = [pscustomobject]@{
        ExpectedSeconds = $ExpectedSeconds
        MaximumSeconds = $MaximumSeconds
    }
}

$SuitePlanLines = @(Get-Content -LiteralPath $SuitePlanPath)
if ($SuitePlanLines.Count -lt 2 -or
    $SuitePlanLines[0] -ne 'windvale-native-verification-owners 2') {
    throw 'The native verification-owner header differs.'
}
$SuiteEntries = @(
    $SuitePlanLines |
        Select-Object -Skip 1 |
        ForEach-Object {
            $Fields = $_ -split '\|', 6
            if ($Fields.Count -ne 6) {
                throw "Malformed native verification-owner entry: $_"
            }
            if ($Fields[3] -notin @('1', '2', '3', '4')) {
                throw "Invalid native qualification shard: $_"
            }
            if (!$DurationProfiles.ContainsKey($Fields[4])) {
                throw "Unknown native verification duration profile: $_"
            }
            $Duration = $DurationProfiles[$Fields[4]]
            [pscustomobject]@{
                Name = $Fields[0]
                Command = $Fields[1]
                ExpectedSeconds = $Duration.ExpectedSeconds
                MaximumSeconds = $Duration.MaximumSeconds
            }
        }
)
$KnownSuites = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$SuiteByCommand = @{}
foreach ($Entry in $SuiteEntries) {
    if (!$KnownSuites.Add($Entry.Name)) {
        throw "Duplicate native verification owner '$($Entry.Name)'."
    }
    $SuiteByCommand[$Entry.Command] = $Entry.Name
}
$BaseOsX64CodeEmissionDevelopmentEligible =
    $OsX64CodeEmissionDevelopmentEligible
$BaseLibraryDevelopmentEligible = $LibraryDevelopmentEligible
$BaseDatabaseStorageDevelopmentEligible = $DatabaseStorageDevelopmentEligible
if ($null -ne $InitializationCache) {
    $InitializationVariables = @{}
    foreach ($Variable in Get-Variable) {
        if (!$InitializationVariableNamesBefore.Contains($Variable.Name) -and
            $Variable.Name -notin @(
                'InitializationVariableNamesBefore',
                'InitializationVariables',
                'Variable'
            )) {
            $InitializationVariables[$Variable.Name] = $Variable.Value
        }
    }
    $InitializationCache[$InitializationCacheKey] = $InitializationVariables
}
}

# Selection state is intentionally fresh for every changed-path set. The optional
# caller-owned cache contains only the immutable routing registries initialized
# above and never carries a prior plan's choices into a later plan.
$SelectedSuites = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$Gaps = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::Ordinal)
$RunPlanVerification = $false
$RunWebAssemblyVerification = $false
$RunWebAssemblyEngineVerification = $false
$RunGitHubQualificationVerification = $false
$SourceContainmentCompilerDevelopmentEligible = $true
$OsX64CodeEmissionDevelopmentEligible =
    $BaseOsX64CodeEmissionDevelopmentEligible
$OsX64CodeEmissionDevelopmentRequiresAllTargets = $false
$SelectedOsX64CodeEmissionDevelopmentTargets =
    [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$LibraryDevelopmentEligible = $BaseLibraryDevelopmentEligible
$LibraryDevelopmentRequiresAllTargets = $false
$SelectedLibraryDevelopmentTargets =
    [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$DatabaseStorageDevelopmentEligible =
    $BaseDatabaseStorageDevelopmentEligible
$DatabaseDevelopmentRequiresAllTargets = $false
$SelectedDatabaseDevelopmentTargets =
    [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$script:CurrentChangedPath = $null

function Add-Suite {
    param([Parameter(Mandatory)][string[]]$Name)
    foreach ($SuiteName in $Name) {
        if (!$KnownSuites.Contains($SuiteName)) {
            throw "Unknown native verification owner '$SuiteName'."
        }
        if ($SuiteName -eq 'libraries' -and
            ![string]::IsNullOrWhiteSpace($script:CurrentChangedPath) -and
            !$LibraryDevelopmentTargetsByPath.ContainsKey(
                $script:CurrentChangedPath) -and
            !$LibraryDevelopmentContractTargets.ContainsKey(
                $script:CurrentChangedPath)) {
            $script:LibraryDevelopmentRequiresAllTargets = $true
        }
        $null = $SelectedSuites.Add($SuiteName)
    }
}

function Add-Library-Suite-If-Owned {
    if ($LibraryDevelopmentTargetsByPath.ContainsKey(
            $script:CurrentChangedPath) -or
        $LibraryDevelopmentContractTargets.ContainsKey(
            $script:CurrentChangedPath)) {
        Add-Suite 'libraries'
    }
}

function Add-Gap {
    param([Parameter(Mandatory)][string]$Name)
    $null = $Gaps.Add($Name)
}

function Test-ArchivedManagedPath {
    param([Parameter(Mandatory)][string]$Path)

    $Extension = [IO.Path]::GetExtension($Path)
    if ($Extension -in @(
        '.cs', '.csproj', '.fs', '.fsproj', '.vb', '.vbproj', '.razor', '.sln', '.slnx'
    )) {
        return $true
    }

    return $Path -in @(
        'Directory.Build.props',
        'Directory.Build.targets',
        'Directory.Packages.props',
        'global.json',
        'NuGet.Config',
        'packages.lock.json',
        'Documents/Project/Stage0-Recovery-Dependencies.json',
        'Tools/Recovery/New-Stage0-Recovery-Archive.ps1',
        'Tools/Recovery/Rebuild-Native-Compiler-Seed.ps1',
        'Tools/Recovery/Rebuild-Native-Compiler-Seed.sh',
        'Tools/Recovery/Rebuild-Os-Probe.ps1',
        'Tools/Recovery/Rebuild-WebAssembly-Native-Backend.ps1',
        'Tools/Recovery/Rebuild-WebAssembly-Native-Compiler.ps1',
        'Tools/Recovery/Test-Stage0-Recovery-Archive.ps1',
        'Tools/Recovery/Verify-Managed-Bootstrap.ps1',
        'Tools/Recovery/Verify-Managed-Bootstrap.sh',
        'Tools/Verify/Verify-Seed.ps1',
        'Tools/Verify/Verify-Seed.sh',
        'Tools/Verify/Verify-Stage0-Recovery-Archive.ps1',
        'Tools/Windvale.Playground/Properties/launchSettings.json'
    )
}

function Test-LanguagePaperSourcePath {
    param([Parameter(Mandatory)][string]$Path)

    return $Path -match (
        '^Documents/Project/Language-1\.0-(?:Paper-Corpus|Localization-Workloads)/' +
        '[0-9]+-[^/]+/Source/[^/]+\.wv$')
}

function Test-LanguagePaperDataPath {
    param([Parameter(Mandatory)][string]$Path)

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
    param([Parameter(Mandatory)][string]$Path)

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
        'Documents/Project/Windvale-Language-1.0-Migration-Evidence.md',
        'Documents/Project/Windvale-Language-1.0-Paper-Corpus.md',
        'Documents/Project/Windvale-Accelerator-Compute-And-AI-Design.md',
        'Documents/Project/Windvale-Language-1.0-Localization-Workloads.md',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0815-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0833-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0857-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0861-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0870-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0894-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0901-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0915-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0917-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0919-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0942-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt',
        'Documents/Decisions/0780-Implement-Language-1.0-Generic-Option-And-Result.md'
    )
}

function Require-Full-Database-Storage {
    $script:DatabaseStorageDevelopmentEligible = $false
    Add-Suite 'database-storage'
}

function Add-WebAssemblyVerification {
    $script:RunWebAssemblyVerification = $true
    $script:RunWebAssemblyEngineVerification = $false
}

function Add-WebAssemblyEngineVerification {
    if (!$script:RunWebAssemblyVerification) {
        $script:RunWebAssemblyEngineVerification = $true
    }
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

function Add-Source-Front-End-Suites {
    Add-Suite @(
        'source-containment',
        'generic-nominal-type-binding',
        'generic-nominal-type-layout',
        'generic-nominal-type-materialization',
        'generic-nominal-wvlb-carrier',
        'language-1-front-door'
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
        'wvb-inspector-reconstruction',
        'wvo-inspector-reconstruction',
        'wvo-publisher-reconstruction',
        'console-publisher-reconstruction',
        'hosted-verifier-publisher-files'
    )
}

function Add-Os-Suite {
    param([Parameter(Mandatory)][string]$Path)
    if ($Path -match 'X64-Process-Filesystem-(?:Record|Paging|Image|Machine)-Emission') {
        Add-Suite 'os-x64-filesystem-machine-emission'
        return
    }
    if ($Path -match 'X64-Process-Final-State-Validation-Epilogue-Emission') {
        Add-Suite 'os-x64-code-emission'
        return
    }
    if ($Path -match 'X64-(?:Code|Process-(?:Entry|Privileged-Entry|Thread-Timer-State|Timer-Activation|Init-Reply-Publish-Resume|Directory-Reply-Publish-Resume|Init-Return-(?:Program-Validation|Budget-Validation|Store-Directory-Validation)|Provider-(?:User-Transfer|Return-Init-Transfer)|Coordinator|Endpoint|Memory-Allocation|Record|Paging|Image|Client-(?:Reservation|Record|Paging|Image|User-Transfer|Return-Init-Transfer|Reply-Delivery|Directory-Request-Delivery|Directory-Reply-Delivery|Completion-Cleanup|Reclamation-Preflight|Memory-Recycle|Generation-Two-(?:Record|Paging|Image|Endpoint-Rebind|Reentry|Return-Validation|User-Transfer|Return-Init-Transfer|Init-Reply-Publish-Resume|Reply-Delivery|Directory-Request-Delivery|Directory-Reply-Lifecycle|Completion-Cleanup|Completion-Finalize-Resume)|Program-Resource|Budget-Resource|Store-Resource|Directory-Resource|Store-Validation|Directory-Validation)|Directory-(?:Allocation|Record|Paging|Image|Generation-Two-Reply-Publish-Resume)))-Emission') {
        Add-Suite 'os-x64-code-emission'
    } elseif ($Path -match 'Provider-Launch-(?:Transaction|Lifecycle)') {
        Add-Suite 'os-provider-launch-transaction'
    } elseif ($Path -match 'Endpoint-Transfer-Profile') {
        Add-Suite @('os-endpoint-transfer', 'native-u64-lowering')
    } elseif ($Path -match '(?:Filesystem|Network)-Process-Service|Provider-Images') {
        Add-Suite @('os-provider-images', 'os-application-launch')
    } elseif ($Path -match 'Fat32-Block-(Read|Provider|Exchange|Image)') {
        Add-Suite @('os-fat32-block-read', 'native-u64-lowering')
    } elseif ($Path -match 'Fat32-(Chain-Position|File-Read-Transaction)') {
        Add-Suite @('os-fat32-file-read', 'native-u64-lowering')
    } elseif ($Path -match 'Fat32-(Volume-Admission|Cluster-Chain|Directory-Admission|File-Read-Plan)') {
        Add-Suite @('os-fat32-volume', 'native-u64-lowering')
    } elseif ($Path -match 'Filesystem-(?:Service|Provider)') {
        Add-Suite @('os-filesystem-service', 'native-u64-lowering')
    } elseif ($Path -match 'Application-(?:Launch|Machine-Construction|Start-(?:Request|User-Copy))|Service-Launch') {
        Add-Suite @('os-application-launch', 'os-process-policy', 'os-probe')
    } elseif ($Path -match 'Resource-Domain') {
        Add-Suite 'os-resource-domain'
    } elseif ($Path -match 'Hello-Service-Fault|Process-Service-Fault-Shim|Process-User-Fault-Shim') {
        Add-Gap 'os-process-fault-scenario-construction'
    } elseif ($Path -match '/Services/|Windvale-Os-(?:Resource-Service|Resource-Store-Service|Directory-Service|Directory-Snapshot)|Windvale-(?:Resource-Service-Ipc|Directory-Service-Ipc|Directory-Snapshot)\.md$') {
        Add-Suite 'os-services'
    } elseif ($Path -match 'Process-Foundation|Process-Policy|Boot-Service-Composition') {
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
    if ($Stem -in @(
        'Test-Seed-Native-Front-Door-Reconstruction',
        'Test-Retirement-Suite'
    )) {
        # Deletion tombstones: these redundant aggregate entry points are retired.
        $script:RunPlanVerification = $true
        return
    }
    if ($Stem -eq 'Test-Source-Containment') {
        $script:SourceContainmentCompilerDevelopmentEligible = $false
    }
    if ($Stem -eq 'Test-Language-1.0-Unsafe-Write-Region-Wir') {
        Add-Suite 'native-x64-lowering-development'
        return
    }
    if ($Stem -in @(
        'Test-Language-1.0-Unsafe-Wir',
        'Test-Language-1.0-Unsafe-Scratch-Wir',
        'Test-Language-1.0-Unsafe-Write-Region-Wir',
        'Test-Language-1.0-Unsafe-Type-Surface'
    )) {
        Add-Suite 'language-1-callable-semantics'
        return
    }
    if ($Stem -in @(
        'Verify-Language-1.0-Owned-Vector-Calls-Wir',
        'Verify-Language-1.0-Using-Wir'
    )) {
        Add-Suite 'language-1-memory-budget-split-execution'
        return
    }
    if ($Stem -in @(
        'Verify-Language-1.0-Migration-Fixtures',
        'Verify-Source-Analysis-Diagnostic',
        'Verify-Language-1.0-Fixed-Integers',
        'Verify-Language-1.0-Fixed-Arrays',
        'Verify-Language-1.0-Runes',
        'Verify-Language-1.0-Floating',
        'Verify-Language-1.0-Unit-Never',
        'Verify-Language-1.0-Multi-Field-Variants',
        'Verify-Language-1.0-Vector-Sequence-Types',
        'Verify-Language-1.0-Vector-Sequence-Runtime',
        'Verify-Language-1.0-Sequence-Reads',
        'Verify-Language-1.0-Vector-Reads-Freeze',
        'Verify-Language-1.0-Memory-Budget-Split-Wir',
        'Verify-Language-1.0-Vector-Construct-Reserved-Wir',
        'Verify-Language-1.0-U8-Enums',
        'Run-WebAssembly-Scalar-Wvb',
        'Verify-Generic-Nominal-Main-Pipeline',
        'Verify-Generic-Nominal-Function-Body',
        'Verify-Generic-Nominal-Declaration-Dependency',
        'Verify-Generic-Nominal-Variant'
    )) {
        Add-Suite 'language-1-front-door'
        if ($Path -eq
            'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Floating-Core.wv') {
            Add-Suite 'language-1-memory-budget-split-execution'
        }
        return
    }
    if ($Stem -eq 'Build-Cached-Os-X64-Project-Wvbs') {
        $script:OsX64CodeEmissionDevelopmentRequiresAllTargets = $true
        Add-Suite 'os-x64-code-emission'
        return
    }
    if ($Stem -eq 'Build-Cached-Segmented-Hosted-Wvb') {
        Add-Suite 'segmented-hosted-wvb-cache'
        return
    }
    if ($Stem -eq 'Build-Cached-Hosted-Application') {
        Add-Suite 'segmented-hosted-wvb-cache'
        return
    }
    if ($Stem -eq 'Native-Hosted-Application-Cache-Core') {
        Add-Suite 'segmented-hosted-wvb-cache'
        return
    }
    if ($Stem -in @(
        'Compile-Compiler-Source-Set',
        'Compile-Project-2-With-Compiler'
    )) {
        Add-Suite 'language-1-front-door'
        return
    }
    if ($Stem -in @(
        'Run-Authenticated-Source-Admission',
        'Write-Canonical-Language-1.0-Target-Descriptor'
    )) {
        Add-Suite 'language-1-front-door'
        return
    }
    if ($Stem -eq 'Run-Split-Compiler') {
        Add-Suite @(
            'language-1-front-door',
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding'
        )
        return
    }
    if ($Stem -eq 'Test-Language-1.0-Authenticated-Foreign-Binding') {
        Add-Suite 'language-1-authenticated-foreign-binding'
        return
    }
    if ($Stem -eq 'Build-Cached-Split-Project-Wvb') {
        Add-Suite @(
            'compiler-split-development',
            'wvb-runner-reconstruction',
            'language-1-production-admission-ingress'
        )
        return
    }
    if ($Stem -in @(
        'Generate-Compiler-Artifact-Readers',
        'Split-Project-Source-Ordering-Core',
        'Test-Cached-Split-Project-Wvb',
        'Test-Compiler-Split-Development',
        'Write-Split-Compiler-Producer-Identity'
    )) {
        Add-Suite @('compiler-split-development', 'wvb-runner-reconstruction')
        return
    }
    if ($Stem -eq 'Test-Compiler-Source-Sentinel') {
        Add-Suite 'language-1-front-door'
        return
    }
    if ($Stem -eq 'Run-Database-Storage-Qualification') {
        Require-Full-Database-Storage
        return
    }
    # Retain retired version-1 stems as deletion tombstones for checkout diffs.
    if ($Stem -in @(
        'Build-Cached-Hosted-Application-Session',
        'Build-Cached-Linked-Image',
        'Build-Cached-Linked-Image-Set',
        'Build-Cached-Project-Object',
        'Build-Cached-Project-Wvb',
        'Build-Cached-Segmented-Project',
        'Get-Native-Hosted-Application-Cache-Key',
        'Get-Native-Linked-Image-Cache-Key',
        'Get-Native-Project-Cache-Key',
        'Native-Project-Cache-Key-Core',
        'Test-Hosted-Application-Session',
        'Test-Linked-Image-Set-Checkpoint',
        'Test-Segmented-Project-Checkpoint',
        'Test-Project-Object-Checkpoint',
        'Test-Database-Storage'
    )) {
        $script:DatabaseDevelopmentRequiresAllTargets = $true
        Add-Suite 'database-storage'
        if ($Stem -in @(
            'Build-Cached-Project-Wvb',
            'Get-Native-Project-Cache-Key',
            'Native-Project-Cache-Key-Core'
        )) {
            $script:OsX64CodeEmissionDevelopmentRequiresAllTargets = $true
            Add-Suite 'os-x64-code-emission'
        }
        return
    }
    if ($SuiteByCommand.ContainsKey($Stem)) {
        Add-Suite $SuiteByCommand[$Stem]
        return
    }
    if ($Stem -eq 'Verify-Echo-Application') {
        Add-Suite 'echo-application'
    } elseif ($Stem -eq 'Verify-File-Read-Application') {
        Add-Suite 'file-read-application'
    } elseif ($Stem -eq 'Build-Echo-Package') {
        Add-Suite @('echo-application', 'echo-command-launch')
    } elseif ($Stem -eq 'Create-Wvdb-Query-Fixture') {
        Add-Suite 'wvdb-query-capability'
    } elseif ($Stem -eq 'Create-Release-Envelope-Fixture') {
        Add-Suite @('release-envelope', 'offline-package-stage')
    } elseif ($Stem -in @(
        'Verification-Owner-Stream-Path',
        'Verification-Owner-Result-Cache',
        'Stream-Verification-Owner'
    )) {
        Add-Suite 'verification-owner-stream'
        $script:RunPlanVerification = $true
    } elseif ($Stem -eq 'Test-Verification-Owners') {
        # Deletion tombstone for the replaced paired coordinators.
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
        'Construct-Compiler-Reconstruction',
        'Verify-Compiler-Convergence',
        'Verify-Current-Split-Compiler-Convergence'
    )) {
        Add-Suite 'compiler-reconstruction'
    } elseif ($Stem -eq 'Build-Source-Compiler-Product') {
        Add-Compiler-Suites
        Add-Suite 'compiler-reconstruction'
    } elseif ($Stem -eq 'Measure-Source-Wvb-Compilation') {
        Add-Compiler-Suites
    } elseif ($Stem -in @(
        'Stage-Compiler-Wvb',
        'Link-Staged-Compiler-Wvo',
        'Transport-Compiler-Image'
    )) {
        Add-Suite @(
            'compiler-reconstruction',
            'compiler-split-development',
            'segmented-compiler-toolset-reconstruction',
            'wvb-to-wvo-reconstruction',
            'wvb-runner-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'wvo-publisher-reconstruction',
            'console-packager-container-reconstruction'
        )
    } elseif ($Stem -eq 'Compose-Segmented-Hosted-Overlay') {
        Add-Suite @(
            'database-storage',
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction'
        )
    } elseif ($Stem -in @(
        'Construct-Segmented-Compiler-Toolset',
        'Test-Segmented-Compiler-Packaging',
        'Package-Segmented-Compiler-Wvb',
        'Measure-Segmented-Compiler-Packaging'
    )) {
        Add-Suite 'segmented-compiler-toolset-reconstruction'
        if ($Stem -eq 'Package-Segmented-Compiler-Wvb') {
            Add-Suite @(
                'wvb-runner-reconstruction',
                'compiler-split-development',
                'language-1-admission-evidence-format'
            )
        }
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
    } elseif ($Stem -match 'Check-Wvo|Verify-Wvo|Inspect-Wvo') {
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
    } elseif ($Stem -eq 'Build-Current-Wvb') {
        Add-Bytecode-Suites
        Add-Suite @(
            'compiler-reconstruction',
            'libraries',
            'packages',
            'language-1-admission-evidence-format',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator'
        )
    } elseif ($Stem -eq 'Build-Current-Split-Project-Wvb') {
        Add-Suite @(
            'compiler-split-development',
            'wvb-runner-reconstruction',
            'language-1-authenticated-foreign-binding'
        )
    } elseif ($Stem -in @('Build-Wvdb-Query-Package', 'Build-Wvb-Inspector-Package')) {
        Add-Suite @('packages', 'offline-package-stage')
    } elseif ($Stem -eq 'Test-Package-Format') {
        Add-Suite 'package-format'
    } elseif ($Stem -eq 'Run-Wvb') {
        Add-Bytecode-Suites
        Add-Suite 'wvb-runner-reconstruction'
        Add-Suite 'seed-native-front-door'
    } elseif ($Stem -eq 'Random-Containment-Binary') {
        Add-Suite @('wvb-containment', 'wvo-containment')
    } elseif ($Stem -in @(
        'Random-Containment-Corpus',
        'Random-Containment-Host',
        'Random-Containment-Source',
        'Test-Random-Containment'
    )) {
        $script:SourceContainmentCompilerDevelopmentEligible = $false
        Add-Suite @('wvb-containment', 'wvo-containment', 'source-containment')
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
            'native-u64-lowering',
            'wv-linker-reconstruction',
            'console-packager-container-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction',
            'hosted-verifier-publisher-files',
            'wvb-runner-reconstruction',
            'compiler-split-development'
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
    $script:CurrentChangedPath = $Path
    $IsDocumentationCatalogPath = (
        $Path.StartsWith('Documents/Evidence/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Specifications/Indexes/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Documentation/', [StringComparison]::Ordinal) -or
        $Path -in @(
            'Documents/Decisions/Decision-Catalog.json',
            'Documents/Decisions/Legacy-Missing-Status.txt',
            'Specifications/AGENTS.md',
            'Specifications/Legacy-Missing-Status.txt',
            'Specifications/Legacy-Status-Classifications.json',
            'Specifications/README.md',
            'Specifications/Specification-Catalog.json'
        )
    )
    if ($IsDocumentationCatalogPath) {
        continue
    }
    if ($OsX64CodeEmissionDevelopmentTargetsByPath.ContainsKey($Path)) {
        foreach ($OsX64Target in
            $OsX64CodeEmissionDevelopmentTargetsByPath[$Path]) {
            $null = $SelectedOsX64CodeEmissionDevelopmentTargets.Add($OsX64Target)
        }
    } elseif ($Path -match 'X64-(?:Code|Process-.+)-Emission' -or
        $Path -in @(
            'Tests/Native/Os-X64-Code-Emission-Development-Targets.txt',
            'Tools/Native/Test-Os-X64-Code-Emission.cmd',
            'Tools/Native/Test-Os-X64-Code-Emission.sh'
        )) {
        $OsX64CodeEmissionDevelopmentRequiresAllTargets = $true
    }
    if ($LibraryDevelopmentTargetsByPath.ContainsKey($Path)) {
        foreach ($LibraryTarget in $LibraryDevelopmentTargetsByPath[$Path]) {
            $null = $SelectedLibraryDevelopmentTargets.Add($LibraryTarget)
        }
    } elseif ($LibraryDevelopmentContractTargets.ContainsKey($Path)) {
        $null = $SelectedLibraryDevelopmentTargets.Add(
            $LibraryDevelopmentContractTargets[$Path])
    }
    if ($DatabaseDevelopmentTargetsByPath.ContainsKey($Path)) {
        foreach ($DatabaseTarget in $DatabaseDevelopmentTargetsByPath[$Path]) {
            $null = $SelectedDatabaseDevelopmentTargets.Add($DatabaseTarget)
        }
    } elseif ($DatabaseDevelopmentContractTargets.ContainsKey($Path)) {
        foreach ($DatabaseTarget in $DatabaseDevelopmentContractTargets[$Path]) {
            $null = $SelectedDatabaseDevelopmentTargets.Add($DatabaseTarget)
        }
    } elseif ($DatabaseDevelopmentPaths.Contains($Path)) {
        $DatabaseDevelopmentRequiresAllTargets = $true
    }
    $IsDocumentationImage = (
        $Path.StartsWith('Documents/Project/Images/', [StringComparison]::Ordinal) -and
        [IO.Path]::GetExtension($Path) -in @('.gif', '.jpeg', '.jpg', '.png', '.svg', '.webp')
    )
    if ($Path -eq
        'Documents/Decisions/0887-Use-A-Separately-Bounded-Admission-Validator.md') {
        Add-Suite @(
            'language-1-admission-evidence-format',
            'language-1-source-admission-coordinator'
        )
        continue
    } elseif ($Path -eq
        'Documents/Decisions/0888-Publish-The-Canonical-WVFC-Producer.md') {
        Add-Suite @(
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator'
        )
        continue
    } elseif ($Path -eq
        'Documents/Decisions/0889-Publish-The-Bounded-System-Ffi-Foreign-Memory-Semantic-Oracle.md') {
        Add-Suite 'language-1-foreign-memory-semantics'
        continue
    } elseif ($Path -eq
        'Documents/Decisions/0892-Coordinate-Authenticated-Source-Admission.md') {
        Add-Suite 'language-1-source-admission-coordinator'
        continue
    } elseif ($Path -eq
        'Documents/Decisions/0893-Authenticate-Production-Source-Analysis-Ingress.md') {
        Add-Suite 'language-1-production-admission-ingress'
        continue
    } elseif ($Path -eq
        'Documents/Decisions/0895-Bind-Authenticated-Foreign-Declarations-In-A-Private-Compiler-Phase.md') {
        Add-Suite @(
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding'
        )
        continue
    } elseif ($Path -eq
        'Documents/Decisions/0923-Carry-Bound-Foreign-Facts-To-Typed-Lowering.md') {
        Add-Suite 'language-1-authenticated-foreign-binding'
        continue
    } elseif ($Path -in @(
        'Documents/Decisions/0925-Publish-And-Retain-Authenticated-Foreign-Lowering-Carrier.md',
        'Documents/Decisions/0933-Pair-Authenticated-Foreign-Calls-Before-Wvb-Emission.md'
    )) {
        Add-Suite @(
            'language-1-production-admission-ingress',
            'language-1-authenticated-foreign-binding'
        )
        continue
    } elseif ($Path -in @(
        'Documents/Decisions/0898-Publish-Canonical-Foundation-Unsafe-Type-Identities.md',
        'Documents/Decisions/0899-Lower-Canonical-Unsafe-Scratch-Construction-To-Wvir.md'
    )) {
        Add-Suite 'language-1-callable-semantics'
        continue
    } elseif (Test-LanguageFrozenSourceDesignPath $Path) {
        Add-Suite 'language-1-front-door'
        continue
    } elseif (
        (Test-LanguagePaperSourcePath $Path) -or
        (Test-LanguagePaperDataPath $Path) -or
        $Path.StartsWith('Tools/Editors/', [StringComparison]::Ordinal) -or
        (
            $Path -ne 'LICENSE.md' -and
            !$Path.StartsWith('Specifications/', [StringComparison]::Ordinal) -and
            (
                $Path.EndsWith('.md', [StringComparison]::OrdinalIgnoreCase) -or
                $IsDocumentationImage
            )
        )
    ) {
        continue
    } elseif (Test-ArchivedManagedPath $Path) {
        $RunPlanVerification = $true
    } elseif ($Path -in @(
        'Projects/Tools/Windvale-Compiler-Admission-Driver.wvproj',
        'Tools/Windvale.Build/Compiler-Admission-Driver.wv'
    )) {
        Add-Suite @(
            'language-1-front-door',
            'language-1-production-admission-ingress'
        )
    } elseif ($Path.StartsWith(
        'Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'language-1-front-door'
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Analysis-Core.wv',
        'Compiler/Windvale/Source-Emission-Core.wv',
        'Projects/Compiler/Windvale-Source-Analysis-Core.wvproj',
        'Projects/Compiler/Windvale-Source-Emission-Core.wvproj',
        'Projects/Tests/Language-1.0-Source-Analysis-Self-Test.wvproj',
        'Specifications/Compiler-Source-Analysis.md',
        'Tests/Fixtures/Language-1.0/Closure-Borrow-Main-Pipeline.wv',
        'Tests/Fixtures/Language-1.0/Closure-Borrow-Mutable.wv',
        'Tests/Fixtures/Language-1.0/Closure-Copy-Main-Pipeline.wv',
        'Tests/Fixtures/Language-1.0/Closure-Move-Main-Pipeline.wv',
        'Tests/Fixtures/Language-1.0/Closure-Move-Use-After-Move.wv',
        'Tests/Fixtures/Language-1.0/Source-Analysis-Self-Test.wv',
        'Tools/Native/Verify-Language-1.0-Closure-Compiler-Pipeline.mjs'
    )) {
        Add-Suite 'language-1-front-door'
        if ($Path -eq 'Specifications/Compiler-Source-Analysis.md') {
            Add-Suite @(
                'language-1-production-admission-ingress',
                'language-1-authenticated-foreign-binding'
            )
        }
        if ($Path -in @(
            'Compiler/Windvale/Source-Emission-Core.wv',
            'Projects/Compiler/Windvale-Source-Emission-Core.wvproj'
        )) {
            Add-Suite 'language-1-callable-semantics'
        }
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Foreign-Lowering-Builder-Core.wv',
        'Projects/Tools/Windvale-Compiler-Foreign-Binding-Driver.wvproj',
        'Tools/Windvale.Build/Compiler-Foreign-Binding-Driver.wv'
    )) {
        Add-Suite 'language-1-production-admission-ingress'
    } elseif ($Path -in @(
        'Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj',
        'Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj',
        'Specifications/Compiler-Split-Development-Cache.md',
        'Tools/Windvale.Build/Compiler-Analysis-Driver.wv',
        'Tools/Windvale.Build/Compiler-Emission-Driver.wv'
    )) {
        Add-Suite 'compiler-split-development'
        if ($Path -in @(
            'Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj',
            'Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj',
            'Tools/Windvale.Build/Compiler-Analysis-Driver.wv',
            'Tools/Windvale.Build/Compiler-Emission-Driver.wv'
        )) {
            Add-Suite 'language-1-production-admission-ingress'
            if ($Path -in @(
                'Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj',
                'Tools/Windvale.Build/Compiler-Analysis-Driver.wv'
            )) {
                Add-Suite 'language-1-authenticated-foreign-binding'
            }
        }
    } elseif ($Path -in @(
        'Tools/Native/Language-1.0-Callable-Wvb-Fixtures.mjs',
        'Tools/Native/Verify-Language-1.0-Callable-Runner.mjs'
    )) {
        Add-Suite @(
            'language-1-callable-semantics',
            'language-1-memory-budget-split-execution'
        )
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Bindings-Closures-Core.wv',
        'Compiler/Windvale/Source-Closure-Capture-Effects-Core.wv',
        'Compiler/Windvale/Source-Closure-Captures-Core.wv',
        'Compiler/Windvale/Source-Closure-Lowering-Core.wv',
        'Compiler/Windvale/Source-Callable-Types-Core.wv',
        'Compiler/Windvale/Source-Effects-Core.wv',
        'Compiler/Windvale/Source-Function-Type-Lowering-Core.wv',
        'Projects/Compiler/Windvale-Source-Bindings-Closures-Core.wvproj',
        'Projects/Compiler/Windvale-Source-Closure-Captures-Core.wvproj',
        'Projects/Compiler/Windvale-Source-Closure-Lowering-Core.wvproj',
        'Projects/Compiler/Windvale-Source-Callable-Types-Core.wvproj',
        'Projects/Compiler/Windvale-Source-Effects-Core.wvproj',
        'Projects/Compiler/Windvale-Source-Function-Type-Lowering-Core.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Closure-Capture-Semantics.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Closure-Lowering-Catalog.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Callable-Type-Catalog.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Effect-Semantics.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Function-Type-Catalog.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Function-Value-Front-End.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Named-Argument-Semantics.wvproj',
        'Specifications/Compiler-Source-Closure-Captures.md',
        'Specifications/Compiler-Source-Closure-Lowering.md',
        'Specifications/Compiler-Source-Callable-Types.md',
        'Specifications/Compiler-Source-Effects.md',
        'Specifications/Compiler-Source-Function-Types.md',
        'Tests/Fixtures/Language-1.0/Closure-Capture-Semantics-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Closure-Lowering-Catalog-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Callable-Indirect-Execution.wv',
        'Tests/Fixtures/Language-1.0/Callable-Type-Catalog-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Effect-Semantics-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Function-Type-Catalog-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Function-Value-Front-End-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Named-Argument-Semantics-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-Callable-Semantics.cmd',
        'Tools/Native/Test-Language-1.0-Callable-Semantics.mjs',
        'Tools/Native/Test-Language-1.0-Callable-Semantics.sh'
    )) {
        Add-Suite 'language-1-callable-semantics'
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Language-1-Effect-Clause-Front-End.wvproj',
        'Tests/Fixtures/Language-1.0/Effect-Clause-Front-End-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-Effect-Clause-Front-End.cmd',
        'Tools/Native/Test-Language-1.0-Effect-Clause-Front-End.sh',
        'Tools/Native/Test-Language-1.0-Effect-Clause-Front-End.mjs'
    )) {
        Add-Suite 'language-1-effect-clause-front-end'
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Admission-Parser-Evidence-Core.wv',
        'Projects/Compiler/Windvale-Source-Admission-Parser-Evidence-Core.wvproj'
    )) {
        Add-Suite @(
            'language-1-system-ffi-front-end',
            'language-1-foreign-catalog-producer',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress'
        )
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Admission-Coordinator-Core.wv',
        'Compiler/Windvale/Source-Target-Admission-Core.wv',
        'Documents/Decisions/0892-Coordinate-Authenticated-Source-Admission.md',
        'Projects/Compiler/Windvale-Source-Admission-Coordinator-Core.wvproj',
        'Projects/Compiler/Windvale-Source-Target-Admission-Core.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Source-Admission-Coordinator.wvproj',
        'Tests/Fixtures/Language-1.0/Source-Admission-Coordinator-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-Source-Admission-Coordinator.cmd',
        'Tools/Native/Test-Language-1.0-Source-Admission-Coordinator.mjs',
        'Tools/Native/Test-Language-1.0-Source-Admission-Coordinator.sh'
    )) {
        if ($Path -in @(
            'Compiler/Windvale/Source-Admission-Coordinator-Core.wv',
            'Compiler/Windvale/Source-Target-Admission-Core.wv'
        )) {
            Add-Suite @(
                'language-1-source-admission-coordinator',
                'language-1-production-admission-ingress'
            )
        } else {
            Add-Suite 'language-1-source-admission-coordinator'
        }
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Admission-Authentication-Core.wv',
        'Projects/Tools/Windvale-Compiler-Source-Authenticator.wvproj',
        'Tools/Windvale.Build/Compiler-Source-Authenticator-Driver.wv'
    )) {
        Add-Suite 'language-1-production-admission-ingress'
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Foreign-Catalog-Authentication-Core.wv',
        'Projects/Compiler/Windvale-Source-Foreign-Catalog-Authentication-Core.wvproj'
    )) {
        Add-Suite 'language-1-production-admission-ingress'
        if ($Path -eq
            'Compiler/Windvale/Source-Foreign-Catalog-Authentication-Core.wv') {
            Add-Suite 'language-1-authenticated-foreign-binding'
        }
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Foreign-Binding-Core.wv',
        'Compiler/Windvale/Source-Foreign-Lowering-Carrier-Core.wv',
        'Compiler/Windvale/Source-Foreign-Lowering-Pairing-Core.wv',
        'Projects/Compiler/Windvale-Source-Foreign-Binding-Core.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Authenticated-Foreign-Binding-Combined.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Authenticated-Foreign-Binding-Portable.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Authenticated-Foreign-Binding.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Foreign-Lowering-Pairing.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Typed-Foreign-Call-Wir.wvproj',
        'Tests/Fixtures/Language-1.0/Authenticated-Foreign-Binding-Combined-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Authenticated-Foreign-Binding-Portable-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Authenticated-Foreign-Binding-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Foreign-Lowering-Pairing-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Typed-Foreign-Call-Wir-Validation-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Typed-Foreign-Call-Wir-Self-Test.wv'
    )) {
        Add-Suite 'language-1-authenticated-foreign-binding'
        if ($Path -in @(
            'Compiler/Windvale/Source-Foreign-Binding-Core.wv',
            'Compiler/Windvale/Source-Foreign-Lowering-Carrier-Core.wv',
            'Compiler/Windvale/Source-Foreign-Lowering-Pairing-Core.wv'
        )) {
            Add-Suite 'language-1-production-admission-ingress'
        }
    } elseif ($Path -in @(
        'Compiler/Windvale/Admission-Evidence-Core.wv',
        'Compiler/Windvale/Admission-Evidence-Validator-Core.wv',
        'Compiler/Windvale/Admission-Source-Set-Core.wv',
        'Documents/Decisions/0887-Use-A-Separately-Bounded-Admission-Validator.md',
        'Projects/Compiler/Windvale-Admission-Evidence-Core.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Admission-Evidence.wvproj',
        'Projects/Tools/Windvale-Compiler-Admission-Evidence-Validator.wvproj',
        'Specifications/Compiler-Admission-Evidence.md',
        'Tests/Fixtures/Language-1.0/Admission-Evidence-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-Admission-Evidence-Format.cmd',
        'Tools/Native/Test-Language-1.0-Admission-Evidence-Format.mjs',
        'Tools/Native/Test-Language-1.0-Admission-Evidence-Format.sh',
        'Tools/Windvale.Build/Compiler-Admission-Evidence-Validator-Driver.wv'
    )) {
        Add-Suite @(
            'language-1-admission-evidence-format',
            'language-1-source-admission-coordinator',
            'language-1-production-admission-ingress'
        )
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Target-Core.wv',
        'Projects/Tests/Windvale-Native-Test-Language-1-System-Ffi-Front-End.wvproj',
        'Specifications/Windvale-Language-1.0-Target-Descriptor.md',
        'Tests/Fixtures/Language-1.0/System-Ffi-Front-End-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-System-Ffi-Front-End.cmd',
        'Tools/Native/Test-Language-1.0-System-Ffi-Front-End.sh',
        'Tools/Native/Test-Language-1.0-System-Ffi-Front-End.mjs'
    )) {
        if ($Path -eq 'Compiler/Windvale/Source-Target-Core.wv') {
            Add-Suite @(
                'language-1-system-ffi-front-end',
                'language-1-admission-evidence-format',
                'language-1-source-admission-coordinator',
                'language-1-production-admission-ingress',
                'language-1-authenticated-foreign-binding',
                'language-1-foreign-memory-semantics'
            )
        } elseif ($Path -eq
            'Specifications/Windvale-Language-1.0-Target-Descriptor.md') {
            Add-Suite @(
                'language-1-system-ffi-front-end',
                'language-1-source-admission-coordinator',
                'language-1-production-admission-ingress',
                'language-1-authenticated-foreign-binding',
                'language-1-foreign-memory-semantics'
            )
        } else {
            Add-Suite 'language-1-system-ffi-front-end'
        }
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Language-1-System-Ffi-Unsafe-Context.wvproj',
        'Tests/Fixtures/Language-1.0/System-Ffi-Unsafe-Context-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-System-Ffi-Unsafe-Context.cmd',
        'Tools/Native/Test-Language-1.0-System-Ffi-Unsafe-Context.sh',
        'Tools/Native/Test-Language-1.0-System-Ffi-Unsafe-Context.mjs'
    )) {
        Add-Suite 'language-1-system-ffi-unsafe-context'
    } elseif ($Path -eq
        'Specifications/Windvale-Language-1.0-Foreign-Catalog.md') {
        Add-Suite 'language-1-foreign-catalog-format'
        Add-Suite 'language-1-foreign-catalog-producer'
        Add-Suite 'language-1-source-admission-coordinator'
        Add-Suite 'language-1-production-admission-ingress'
        Add-Suite 'language-1-authenticated-foreign-binding'
        Add-Suite 'language-1-foreign-memory-semantics'
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Foreign-Catalog-Core.wv',
        'Projects/Tests/Windvale-Native-Test-Language-1-Foreign-Catalog-Format.wvproj',
        'Tests/Fixtures/Language-1.0/Foreign-Catalog-Format-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-Foreign-Catalog-Format.cmd',
        'Tools/Native/Test-Language-1.0-Foreign-Catalog-Format.sh',
        'Tools/Native/Test-Language-1.0-Foreign-Catalog-Format.mjs'
    )) {
        if ($Path -eq 'Compiler/Windvale/Source-Foreign-Catalog-Core.wv') {
            Add-Suite @(
                'language-1-foreign-catalog-format',
                'language-1-admission-evidence-format',
                'language-1-foreign-catalog-producer',
                'language-1-source-admission-coordinator',
                'language-1-production-admission-ingress',
                'language-1-authenticated-foreign-binding',
                'language-1-foreign-memory-semantics'
            )
        } else {
            Add-Suite 'language-1-foreign-catalog-format'
        }
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Foreign-Catalog-Producer-Core.wv',
        'Documents/Decisions/0888-Publish-The-Canonical-WVFC-Producer.md',
        'Projects/Compiler/Windvale-Source-Foreign-Catalog-Producer-Core.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Foreign-Catalog-Producer.wvproj',
        'Tests/Fixtures/Language-1.0/Foreign-Catalog-Producer-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-Foreign-Catalog-Producer.cmd',
        'Tools/Native/Test-Language-1.0-Foreign-Catalog-Producer.sh',
        'Tools/Native/Test-Language-1.0-Foreign-Catalog-Producer.mjs'
    )) {
        if ($Path -eq
            'Compiler/Windvale/Source-Foreign-Catalog-Producer-Core.wv') {
            Add-Suite @(
                'language-1-foreign-catalog-producer',
                'language-1-source-admission-coordinator',
                'language-1-production-admission-ingress'
            )
        } else {
            Add-Suite @(
                'language-1-foreign-catalog-producer',
                'language-1-source-admission-coordinator'
            )
        }
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Foreign-Semantics-Core.wv',
        'Documents/Decisions/0889-Publish-The-Bounded-System-Ffi-Foreign-Memory-Semantic-Oracle.md',
        'Projects/Tests/Windvale-Native-Test-Language-1-Foreign-Memory-Profile-Regression.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Foreign-Memory-Semantics.wvproj',
        'Specifications/Windvale-Language-1.0-Foreign-Memory-Semantics.md',
        'Tests/Fixtures/Language-1.0/Foreign-Memory-Profile-Regression-System.wv',
        'Tests/Fixtures/Language-1.0/Foreign-Memory-Profile-Regression.wv',
        'Tests/Fixtures/Language-1.0/Foreign-Memory-Semantics-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-Foreign-Memory-Semantics.cmd',
        'Tools/Native/Test-Language-1.0-Foreign-Memory-Semantics.sh',
        'Tools/Native/Test-Language-1.0-Foreign-Memory-Semantics.mjs'
    )) {
        Add-Suite 'language-1-foreign-memory-semantics'
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Language-1-Using-Front-End.wvproj',
        'Tests/Fixtures/Language-1.0/Using-Front-End-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-Using-Front-End.cmd',
        'Tools/Native/Test-Language-1.0-Using-Front-End.sh',
        'Tools/Native/Test-Language-1.0-Using-Front-End.mjs'
    )) {
        Add-Suite 'language-1-using-front-end'
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Declaration.wvproj',
        'Tests/Fixtures/Language-1.0/Generic-Nominal-Declaration-Self-Test.wv'
    )) {
        Add-Suite 'generic-nominal-declarations'
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Type-Binding.wvproj',
        'Tests/Fixtures/Language-1.0/Borrow-Parser-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Fixed-Array-Parser-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Fixed-Array-Type-Binding-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Generic-Nominal-Type-Binding-Self-Test.wv',
        'Tools/Native/Test-Generic-Nominal-Type-Binding.cmd',
        'Tools/Native/Test-Generic-Nominal-Type-Binding.sh',
        'Tools/Native/Test-Generic-Nominal-Type-Binding.mjs'
    )) {
        Add-Suite 'generic-nominal-type-binding'
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Type-Layout.wvproj',
        'Tests/Fixtures/Language-1.0/Generic-Nominal-Type-Layout-Self-Test.wv',
        'Tools/Native/Test-Generic-Nominal-Type-Layout.cmd',
        'Tools/Native/Test-Generic-Nominal-Type-Layout.sh',
        'Tools/Native/Test-Generic-Nominal-Type-Layout.mjs'
    )) {
        Add-Suite 'generic-nominal-type-layout'
    } elseif ($Path -eq
        'Compiler/Windvale/Source-Wvb-Generic-Nominal-Types-Core.wv') {
        Add-Suite @(
            'generic-nominal-type-materialization',
            'language-1-memory-budget-split-execution'
        )
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Type-Materialization.wvproj',
        'Tests/Fixtures/Language-1.0/Generic-Nominal-Type-Materialization-Self-Test.wv',
        'Tools/Native/Test-Generic-Nominal-Type-Materialization.cmd',
        'Tools/Native/Test-Generic-Nominal-Type-Materialization.sh',
        'Tools/Native/Test-Generic-Nominal-Type-Materialization.mjs'
    )) {
        Add-Suite 'generic-nominal-type-materialization'
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Nominal-Wvlb-Carrier.wvproj',
        'Tests/Fixtures/Language-1.0/Generic-Nominal-Wvlb-Carrier-Self-Test.wv',
        'Tools/Native/Test-Generic-Nominal-Wvlb-Carrier.cmd',
        'Tools/Native/Test-Generic-Nominal-Wvlb-Carrier.sh',
        'Tools/Native/Test-Generic-Nominal-Wvlb-Carrier.mjs'
    )) {
        Add-Suite 'generic-nominal-wvlb-carrier'
    } elseif ($Path -eq
        'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Memory-Budget-Core.wv') {
        Add-Suite @(
            'language-1-memory-budget-accounting',
            'language-1-memory-budget-split-execution'
        )
    } elseif ($Path -in @(
        'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Envelope.wv',
        'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Fixed-Integer-Core.wv',
        'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Floating-Core.wv',
        'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Main.wv',
        'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Rune-Core.wv'
    )) {
        Add-Suite @(
            'language-1-front-door',
            'language-1-callable-semantics',
            'language-1-memory-budget-split-execution'
        )
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Language-1-Memory-Budget-Accounting.wvproj',
        'Tests/Fixtures/Language-1.0/Memory-Budget-Accounting-Self-Test.wv',
        'Tools/Native/Test-Language-1.0-Memory-Budget-Accounting.cmd',
        'Tools/Native/Test-Language-1.0-Memory-Budget-Accounting.sh'
    )) {
        Add-Suite 'language-1-memory-budget-accounting'
    } elseif ($Path -in @(
        'Runtime/Hosted/Tasks/Bounded-Parallel-Task-Scheduler.mjs',
        'Runtime/Hosted/Tasks/Bounded-Parallel-Task-Worker.mjs',
        'Documents/Decisions/0875-Add-A-Bounded-Parallel-Hosted-Task-Scheduler.md',
        'Specifications/Windvale-Hosted-Task-Scheduling.md',
        'Tests/Fixtures/Hosted/Bounded-Parallel-Task-Executor.mjs',
        'Tools/Native/Test-Bounded-Parallel-Task-Scheduler.cmd',
        'Tools/Native/Test-Bounded-Parallel-Task-Scheduler.mjs',
        'Tools/Native/Test-Bounded-Parallel-Task-Scheduler.sh'
    )) {
        Add-Suite 'language-1-parallel-task-scheduler'
    } elseif ($Path -in @(
        'Libraries/Foundation/Operations/Operation.wv',
        'Libraries/Foundation/Tasks/Task.wv',
        'Projects/Tests/Windvale-Language-1-Structured-Task-Call-Depth-Limit.wvproj',
        'Projects/Tests/Windvale-Language-1-Structured-Task-Retained-Result.wvproj',
        'Projects/Tests/Windvale-Language-1-Structured-Task-Trap.wvproj',
        'Projects/Tests/Windvale-Language-1-Structured-Task-Work-Limit.wvproj',
        'Projects/Tests/Windvale-Language-1-Structured-Tasks.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Structured-Task-Runtime.wvproj',
        'Specifications/Compiler-Source-Structured-Tasks.md',
        'Tests/Fixtures/Language-1.0/Async-Direct-Aggregate-Call-Executable.wv',
        'Tests/Fixtures/Language-1.0/Async-Direct-Call-Executable.wv',
        'Tests/Fixtures/Language-1.0/Async-Direct-Call-Missing-Await.wv',
        'Tests/Fixtures/Language-1.0/Async-Indirect-Call-Executable.wv',
        'Tests/Fixtures/Language-1.0/Async-Indirect-Call-Missing-Await.wv',
        'Tests/Fixtures/Language-1.0/Memory-Budget-Split-Executable.wv',
        'Tests/Fixtures/Language-1.0/Memory-Budget-Split-Failure-Executable.wv',
        'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Executable.wv',
        'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Failure-Executable.wv',
        'Tests/Fixtures/Language-1.0/Vector-Construct-Reserved-Zero-Executable.wv',
        'Tests/Fixtures/Language-1.0/Owned-Vector-Calls-And-Joins-Wir.wv',
        'Tests/Fixtures/Language-1.0/Owned-Vector-Call-Use-After.wv',
        'Tests/Fixtures/Language-1.0/Owned-Vector-Call-Duplicate.wv',
        'Tests/Fixtures/Language-1.0/Owned-Vector-Call-Asymmetric-Join.wv',
        'Tests/Fixtures/Language-1.0/Owned-Vector-Loop-Invariant-Wir.wv',
        'Tests/Fixtures/Language-1.0/Owned-Vector-Loop-State-Mismatch.wv',
        'Tests/Fixtures/Language-1.0/Owned-Aggregate-Duplicate-Move.wv',
        'Tests/Fixtures/Language-1.0/Owned-Aggregate-Field-Move.wv',
        'Tests/Fixtures/Language-1.0/Owned-Aggregate-Mutable-Borrow-From-Let.wv',
        'Tests/Fixtures/Language-1.0/Owned-Aggregate-Use-After-Move.wv',
        'Tests/Fixtures/Language-1.0/Owned-Aggregate-Vector-Executable.wv',
        'Tests/Fixtures/Language-1.0/Source-File-Snapshot-Executable.wv',
        'Tests/Fixtures/Language-1.0/Structured-Task-Call-Depth-Limit-Executable.wv',
        'Tests/Fixtures/Language-1.0/Structured-Task-Completion-Order-Executable.wv',
        'Tests/Fixtures/Language-1.0/Structured-Task-Environment-Executable.wv',
        'Tests/Fixtures/Language-1.0/Structured-Task-Four-Child-Cancellation-Executable.wv',
        'Tests/Fixtures/Language-1.0/Structured-Task-Memory-Limit-Executable.wv',
        'Tests/Fixtures/Language-1.0/Structured-Task-Provider-Recovery-Executable.wv',
        'Tests/Fixtures/Language-1.0/Structured-Task-Retained-Result-Executable.wv',
        'Tests/Fixtures/Language-1.0/Structured-Task-Runtime-Self-Test.wv',
        'Tests/Fixtures/Language-1.0/Structured-Task-Trap-Executable.wv',
        'Tests/Fixtures/Language-1.0/Structured-Task-Work-Limit-Executable.wv',
        'Tests/Fixtures/Language-1.0/Structured-Tasks-Executable.wv',
        'Tests/Fixtures/Language-1.0/Sync-Call-Awaited.wv',
        'Tests/Fixtures/Language-1.0/Sync-Caller-Awaits-Async.wv',
        'Tests/Fixtures/Language-1.0/Using-Non-Resource.wv',
        'Tests/Fixtures/Language-1.0/Using-Vector-Fallthrough-Wir.wv',
        'Tests/Fixtures/Language-1.0/Using-Vector-Loop-Exits-Wir.wv',
        'Tests/Fixtures/Language-1.0/Using-Vector-Moved-Before-Release.wv',
        'Tests/Fixtures/Language-1.0/Using-Vector-Nested-Return-Wir.wv',
        'Tests/Fixtures/Language-1.0/Using-Vector-Try-Propagation-Wir.wv',
        'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Task-Core.wv',
        'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.cmd',
        'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs',
        'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.sh',
        'Tools/Native/Verify-Language-1.0-Async-Call-Await.mjs',
        'Tools/Native/Verify-Language-1.0-Using-Wir.mjs'
    )) {
        Add-Suite 'language-1-memory-budget-split-execution'
    } elseif ($Path.StartsWith('Tests/Fixtures/Language-1.0/', [StringComparison]::Ordinal) -or
        $Path -in @(
        'Compiler/Windvale/Source-Descriptor-Core.wv',
        'Libraries/Foundation/Collections/Collections.wv',
        'Libraries/Foundation/Values/Option.wv',
        'Libraries/Foundation/Values/Result.wv',
        'Projects/Compiler/Windvale-Source-Descriptor-Core.wvproj',
        'Projects/Tests/Language-1.0-Foundation-Generic-Result.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Borrow-Call.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Calls.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Collection-Analysis-Publication.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Declarations.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Multiple-Specializations.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Resolution.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Generic-Type-Catalog.wvproj',
        'Projects/Tests/Windvale-Native-Test-Source-Descriptor.wvproj',
        'Projects/Tests/Windvale-Native-Test-Language-1-Value-Front-End.wvproj',
        'Projects/Tests/Windvale-Native-Test-Wvb-Floating-Runtime.wvproj',
        'Tests/Fixtures/WebAssembly/Wvb-Scalar-Interpreter-Collection-Core.wv',
        'Tests/Native/Language-1.0-Fixture-Inventory.txt',
        'Tools/Native/Verify-Source-Wir-Incremental-Generics.mjs'
    )) {
        if ($Path -eq 'Compiler/Windvale/Source-Descriptor-Core.wv') {
            Add-Suite @(
                'language-1-front-door',
                'language-1-admission-evidence-format',
                'language-1-foreign-catalog-producer',
                'language-1-source-admission-coordinator',
                'language-1-production-admission-ingress'
            )
        } else {
            Add-Suite 'language-1-front-door'
        }
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Function-Only.wvproj',
        'Tests/Fixtures/Source-Wvb/Function-Only.wv'
    )) {
        Add-Suite @('seed', 'compiler-reconstruction', 'language-1-front-door')
    } elseif ($Path -eq 'Compiler/Windvale/Native-X64-Lowering-Metadata.wv') {
        Add-Suite 'native-x64-lowering-development'
    } elseif ($Path -in @(
        'Tests/Native/Wvb-To-Wvo-Rejections/Foreign-Runtime-Stale.wvb.b64',
        'Tests/Native/Wvb-To-Wvo-Rejections/Foreign-Runtime-Success.wvb.b64',
        'Tests/Native/Wvb-To-Wvo-Rejections/Unsafe-Write-Pointer.wvb.b64',
        'Tests/Native/Wvb-To-Wvo-Rejections/Unsafe-Write-Pointer-Runtime.wvb.b64',
        'Tests/Native/X64-Paper-Buffer-Source.wva'
    )) {
        Add-Suite 'native-x64-lowering-development'
    } elseif ($Path -eq 'Runtime/Native/Linux-X64-Paper-Buffer-Source.wva') {
        Add-Suite @(
            'native-x64-lowering-development',
            'language-1-production-admission-ingress'
        )
    } elseif ($Path -eq 'Runtime/Windvale/Foreign-Record-Consumer.wv') {
        Add-Suite 'language-1-production-admission-ingress'
    } elseif ($Path -eq
        'Tests/Native/Os-X64-Code-Emission-Development-Targets.txt') {
        $RunPlanVerification = $true
        Add-Suite 'os-x64-code-emission'
    } elseif ($Path -in @(
        '.gitattributes',
        'Documents/Project/Dotnet-Retirement-Inventory.json',
        'Documents/Project/Stage0-Recovery-Dependencies.json',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0815-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0833-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0857-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0861-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0870-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0894-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0901-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0915-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0917-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0919-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Amendment-0942-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt',
        'Documents/Project/Windvale-Language-1.0-Source-Freeze-Candidate.txt',
        'Specifications/README.md',
        'Specifications/Windvale-Native-Changed-Verification.md',
        'Specifications/Windvale-Native-Retirement-Test-Suite.md',
        'Specifications/Windvale-Native-Verification-Owners.md',
        'Tests/Native/Retirement-Suite.txt',
        'Tests/Native/Library-Development-Targets.txt',
        'Tests/Native/Verification-Owners.txt',
        'Tests/Native/Verification-Duration-Profiles.txt',
        'Tests/Native/Development-Owner-Dependencies.txt',
        'Tools/Verify/Verify-Seed-Native-Front-Door.ps1',
        'Tools/Verify/Verify-Seed-Native-Front-Door.sh'
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
    } elseif ($Path -eq
        'Artifacts/WebAssembly-Playground/Windvale-Compiler-Direct.wasm') {
        Add-WebAssemblyEngineVerification
        Add-Suite 'source-containment'
    } elseif ($Path -in @(
        'Tools/Verify/Verify-WebAssembly-Engine.ps1',
        'Tools/Website/Verify-WebAssembly-Playground-Package.mjs',
        'Tools/Website/Verify-WebAssembly-Compiler-Core.mjs',
        'Tools/Windvale.Playground/wwwroot/js/windvale-compiler-core.js'
    ) -or
        $Path.StartsWith(
            'Artifacts/WebAssembly-Playground/',
            [StringComparison]::Ordinal)) {
        Add-WebAssemblyEngineVerification
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
    } elseif ($Path.StartsWith(
        'Tests/Fixtures/Scripting/',
        [StringComparison]::Ordinal) -or
        $Path -eq 'Specifications/Windvale-Scripting.md') {
        Add-Suite 'scripting'
    } elseif ($Path.StartsWith('Tools/Verify/', [StringComparison]::Ordinal)) {
        if ([IO.Path]::GetFileName($Path) -in @(
            'Verify-GitHub-Native-Qualification.ps1'
        )) {
            Add-GitHubQualificationVerification
            $RunPlanVerification = $true
        } elseif ([IO.Path]::GetFileName($Path) -eq 'Verify-Os-Boot.ps1') {
            Add-Suite 'os-probe'
        } elseif ([IO.Path]::GetFileName($Path) -eq
            'Invoke-WindvaleTests.ps1') {
            Add-Suite 'verification-owner-stream'
            $RunPlanVerification = $true
        } elseif ([IO.Path]::GetFileName($Path) -in @(
            'Verify-Seed-Native-Console-Aot.ps1',
            'Verify-Seed-Native-Console-Aot.sh'
        )) {
            Add-Suite 'seed-native-console-aot'
        } elseif ([IO.Path]::GetFileName($Path) -in @(
            'Verify-Seed-Native-Front-Door-Reconstruction.ps1',
            'Verify-Seed-Native-Front-Door-Reconstruction.sh'
        )) {
            # Deletion tombstone: only planner policy remains affected.
            $RunPlanVerification = $true
        } elseif ([IO.Path]::GetFileName($Path) -in @(
            'Verify-Bootstrap.cmd',
            'Verify-Bootstrap.sh'
        )) {
            Add-Suite 'compiler-reconstruction'
        } elseif ([IO.Path]::GetFileName($Path) -in @(
            'Classify-Verification-Changes.ps1',
            'Get-Verification-Plan.ps1',
            'Get-Native-Changed-Verification-Plan.ps1',
            'Verify-Changed.ps1',
            'Verify-Change-Classification.ps1',
            'Verify-Documentation.ps1',
            'Verify-Dotnet-Retirement-Inventory.ps1',
            'Verify-Native-Development-Dependencies.ps1',
            'Update-Verification-Timing-History.ps1',
            'Verify-Verification-Plan.ps1'
        )) {
            $RunPlanVerification = $true
        } else {
            Add-Gap "verification:$([IO.Path]::GetFileName($Path))"
        }
    } elseif ($Path.StartsWith('Tools/Native/', [StringComparison]::Ordinal)) {
        if ([IO.Path]::GetFileName($Path) -in @(
            'Test-Development-Installers.cmd',
            'Test-Development-Installers.sh'
        )) {
            Add-Suite 'installers'
        } else {
            Add-Native-Tool-Suite $Path
        }
    } elseif ($Path.StartsWith('Tools/Release/', [StringComparison]::Ordinal)) {
        if ($Path -in @(
            'Tools/Release/Build-Development-Installers.mjs',
            'Tools/Release/Build-Installers.mjs'
        )) {
            Add-Suite 'installers'
        } elseif ($Path -eq 'Tools/Release/Deterministic-Compression.mjs') {
            Add-Suite @('installers', 'installer-repository')
        } elseif ($Path -in @(
            'Tools/Release/Build-Installer-Repository.mjs',
            'Tools/Release/Verify-Installer-Repository.mjs'
        )) {
            Add-Suite 'installer-repository'
        } elseif ($Path -in @(
            'Tools/Release/Create-Release-Envelope.mjs',
            'Tools/Release/Verify-Release-Envelope.mjs'
        )) {
            Add-Suite @('release-envelope', 'offline-package-stage')
        } elseif ($Path -eq 'Tools/Release/Verify-Wvdb-Approval-Records.mjs') {
            Add-Suite @('wvdb-approval', 'offline-package-stage')
        } else {
            Add-Gap "release-tool:$([IO.Path]::GetFileName($Path))"
        }
    } elseif ($Path.StartsWith('Distribution/Installers/', [StringComparison]::Ordinal)) {
        Add-Suite 'installers'
        if ($Path -eq 'Distribution/Installers/Windvale-Development-Installer.json') {
            Add-Suite 'installer-repository'
        }
        if ($Path.EndsWith('/wv', [StringComparison]::Ordinal) -or
            $Path.EndsWith('/wv.cmd', [StringComparison]::Ordinal) -or
            $Path.EndsWith('/wv-run.ps1', [StringComparison]::Ordinal) -or
            $Path.EndsWith('Installer.json', [StringComparison]::Ordinal)) {
            Add-Suite 'scripting'
        }
    } elseif ($Path -eq 'LICENSE.md') {
        Add-Suite @('installers', 'offline-package-stage')
    } elseif ($Path.StartsWith('Distribution/Releases/', [StringComparison]::Ordinal)) {
        Add-Suite 'release-envelope'
    } elseif ($Path.StartsWith('Tools/Package/', [StringComparison]::Ordinal)) {
        if ([IO.Path]::GetFileName($Path) -in @(
            'Publish-Installation-Activation.mjs',
            'Verify-Installation-Activation-Publisher.mjs'
        )) {
            Add-Suite @('installation-activation', 'offline-generation-lifecycle')
        } elseif ([IO.Path]::GetFileName($Path) -eq
            'Publish-Installation-Generation.mjs') {
            Add-Suite @(
                'offline-generation-lifecycle',
                'installation-generation-publication',
                'offline-package-stage'
            )
        } elseif ([IO.Path]::GetFileName($Path) -eq
            'Verify-Installation-Generation-Publisher.mjs') {
            Add-Suite 'installation-generation-publication'
        } elseif ([IO.Path]::GetFileName($Path) -eq
            'Verify-Installation-Command-Resolver.mjs') {
            Add-Suite 'installation-command-resolution'
        } elseif ([IO.Path]::GetFileName($Path) -in @(
            'Dispatch-Installation-Command.mjs',
            'Verify-Installation-Command-Dispatcher.mjs'
        )) {
            Add-Suite @('echo-command-launch', 'installation-command-dispatch')
        } elseif ([IO.Path]::GetFileName($Path) -eq
            'Verify-Echo-Command-Launch.mjs') {
            Add-Suite 'echo-command-launch'
        } elseif ([IO.Path]::GetFileName($Path) -in @(
            'Verify-Installation-Activation-Planner.mjs',
            'Verify-Offline-Generation-Lifecycle.mjs'
        )) {
            Add-Suite 'offline-generation-lifecycle'
        } elseif ([IO.Path]::GetFileName($Path) -in @(
            'Uninstall-Offline-Package-State.mjs',
            'Verify-Offline-Package-Uninstall.mjs'
        )) {
            Add-Suite @('offline-generation-lifecycle', 'offline-package-uninstall')
        } elseif ([IO.Path]::GetFileName($Path) -eq
            'Create-Offline-Package-Stage-Input.mjs') {
            Add-Suite 'offline-package-stage'
        } else {
            Add-Suite 'package-bundle'
        }
    } elseif ($Path.StartsWith('Tools/Windvale.Package/', [StringComparison]::Ordinal)) {
        if ([IO.Path]::GetFileName($Path) -eq
            'Installation-Command-Resolver-Tool.wv') {
            Add-Suite @(
                'package-format',
                'offline-generation-lifecycle',
                'installation-command-resolution'
            )
        } elseif ([IO.Path]::GetFileName($Path) -eq
            'Installation-Activation-Planner-Tool.wv') {
            Add-Suite 'offline-generation-lifecycle'
        } elseif ([IO.Path]::GetFileName($Path) -eq
            'Installation-Generation-Verifier-Tool.wv') {
            Add-Suite @('package-format', 'offline-package-stage')
        } else {
            Add-Suite @('package-bundle', 'offline-package-stage')
        }
    } elseif ($Path.StartsWith('Tools/Windvale.Project/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tools/Windvale.Build/', [StringComparison]::Ordinal)) {
        Add-Suite 'workspace-project2'
        Add-Compiler-Suites
        if ($Path -eq 'Tools/Windvale.Build/Compiler-Build-Driver.wv') {
            Add-Suite @(
                'language-1-front-door',
                'segmented-compiler-toolset-reconstruction'
            )
        }
    } elseif ($Path -in @(
        'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Metadata-Core.wv',
        'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Semantic-Core.wv',
        'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Executable-Core.wv',
        'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Typed-Directories.wv',
        'Tools/Windvale.Verify/Compiler-Wvb-Verifier-Tool.wv',
        'Tools/Windvale.Verify/Wvb-Metadata-Normalization.wv'
    )) {
        Add-Bytecode-Suites
        Add-Suite @(
            'libraries',
            'model-provider',
            'file-read-application',
            'language-1-front-door',
            'language-1-callable-semantics',
            'language-1-memory-budget-split-execution'
        )
    } elseif ($Path -in @(
        'Libraries/Foundation/Operations/Bounded-Operation-Core.wv',
        'Projects/Libraries/Windvale-Library-Bounded-Operation-Core.wvproj',
        'Projects/Tests/Windvale-Native-Test-Bounded-Operation-Core.wvproj',
        'Tests/Fixtures/Network/Bounded-Operation-Core-Self-Test.wv',
        'Specifications/Bounded-Operation-Core.md'
    )) {
        Add-Suite @('operation-core', 'network-connect-stream')
    } elseif ($Path -in @(
        'Libraries/Platform/Streams/Standard-Byte-Output-Core.wv',
        'Projects/Libraries/Windvale-Library-Standard-Byte-Output-Core.wvproj',
        'Projects/Tests/Windvale-Native-Test-Standard-Byte-Output-Core.wvproj',
        'Tests/Fixtures/Streams/Standard-Byte-Output-Core-Self-Test.wv',
        'Specifications/Standard-Byte-Output-Core.md'
    )) {
        Add-Suite @('standard-byte-output', 'file-read-application')
    } elseif ($Path -in @(
        'Libraries/Platform/Streams/Standard-Byte-Output.wv',
        'Libraries/Platform/Streams/Standard-Byte-Output-Response-Core.wv',
        'Projects/Libraries/Windvale-Library-Standard-Byte-Output.wvproj',
        'Projects/Libraries/Windvale-Library-Standard-Byte-Output-Response-Core.wvproj',
        'Projects/Tests/Windvale-Native-Test-Standard-Byte-Output-Response-Core.wvproj',
        'Tests/Fixtures/Streams/Standard-Byte-Output-Response-Core-Self-Test.wv',
        'Specifications/Standard-Byte-Output-Capability.md'
    )) {
        Add-Suite 'file-read-application'
    } elseif ($Path -in @(
        'Libraries/Network/Address-Authority.wv',
        'Projects/Libraries/Windvale-Library-Network-Address-Authority.wvproj',
        'Projects/Tests/Windvale-Native-Test-Network-Address-Authority.wvproj',
        'Tests/Fixtures/Network/Address-Authority-Self-Test.wv',
        'Specifications/Network-Address-Authority.md'
    )) {
        Add-Suite @('network-authority', 'network-connect-stream')
    } elseif ($Path -in @(
        'Libraries/Network/Connect-Stream-Core.wv',
        'Projects/Libraries/Windvale-Library-Network-Connect-Stream-Core.wvproj',
        'Projects/Tests/Windvale-Native-Test-Network-Connect-Stream-Core.wvproj',
        'Tests/Fixtures/Network/Connect-Stream-Core-Self-Test.wv',
        'Specifications/Network-Connect-Stream-Core.md'
    )) {
        Add-Suite 'network-connect-stream'
    } elseif ($Path -in @(
        'Libraries/Platform/Models/Bound-Model-Provider.wv',
        'Projects/Libraries/Windvale-Library-Bound-Model-Provider.wvproj',
        'Projects/Tests/Windvale-Native-Test-Hosted-Model-Provider.wvproj',
        'Tests/Fixtures/Models/Native-Hosted-Model-Provider-Self-Test.wv',
        'Specifications/Windvale-Bound-Model-Provider.md'
    )) {
        Add-Suite 'model-provider'
    } elseif ($Path -in @(
        'Runtime/Hosted/Network/Host-Network-Protocol.mjs',
        'Runtime/Hosted/Network/Host-Network-Provider-Core.mjs',
        'Runtime/Hosted/Network/Host-Network-Provider-Process.mjs',
        'Runtime/Hosted/Network/Host-Network-Supervisor.mjs'
    )) {
        Add-Suite @('host-network-provider', 'host-tls-provider')
    } elseif ($Path -in @(
        'Runtime/Hosted/Network/Host-Network-Provider.mjs',
        'Tools/Network/Test-Host-Network-Provider.mjs',
        'Tools/Native/Test-Host-Network-Provider.cmd',
        'Tools/Native/Test-Host-Network-Provider.sh',
        'Specifications/Host-Network-Provider.md'
    )) {
        Add-Suite 'host-network-provider'
    } elseif ($Path -in @(
        'Runtime/Hosted/Network/Host-Tls-Provider-Core.mjs',
        'Runtime/Hosted/Network/Host-Tls-Provider.mjs',
        'Runtime/Hosted/Network/Host-Tls-Supervisor.mjs',
        'Tools/Network/Ephemeral-Tls-Fixture.mjs',
        'Tools/Network/Test-Host-Tls-Provider.mjs',
        'Tools/Native/Test-Host-Tls-Provider.cmd',
        'Tools/Native/Test-Host-Tls-Provider.sh',
        'Specifications/Host-Tls-Provider.md'
    )) {
        Add-Suite 'host-tls-provider'
    } elseif ($Path.StartsWith('Runtime/Hosted/Http/', [StringComparison]::Ordinal) -or
        $Path -in @(
            'Tools/Network/Test-Bounded-Https.mjs',
            'Tools/Native/Test-Bounded-Https.cmd',
            'Tools/Native/Test-Bounded-Https.sh',
            'Specifications/Bounded-Https.md'
    )) {
        Add-Suite 'bounded-https'
    } elseif ($Path.StartsWith('Runtime/Hosted/Credentials/', [StringComparison]::Ordinal) -or
        $Path -in @(
            'Tools/Credentials/Test-Protected-Credential.mjs',
            'Tools/Native/Test-Protected-Credential.cmd',
            'Tools/Native/Test-Protected-Credential.sh',
            'Specifications/Protected-Provider-Credential.md'
    )) {
        Add-Suite 'protected-credential'
    } elseif ($Path.StartsWith('Applications/Model-Chat/', [StringComparison]::Ordinal) -or
        $Path -in @(
            'Runtime/Hosted/Models/External-Model-Gateway-Client.mjs',
            'Tools/Models/Test-Model-Chat.mjs',
            'Tools/Native/Test-Model-Chat.cmd',
            'Tools/Native/Test-Model-Chat.sh',
            'Specifications/Hosted-Model-Chat-Command.md'
        )) {
        Add-Suite 'model-chat'
    } elseif ($Path -in @(
            'Runtime/Hosted/Models/Native-External-Model-Gateway-Supervisor.mjs',
            'Runtime/Native/X64-External-Model-Gateway-Host.wva',
            'Runtime/Native/Windows-X64-External-Model-Gateway.wva',
            'Runtime/Native/Linux-X64-External-Model-Gateway.wva',
            'Tests/Native/X64-External-Model-Gateway-Probe.wva',
            'Tools/Models/Fixtures/Native-Model-Gateway-Peer.mjs',
            'Tools/Models/Test-Native-External-Model-Gateway-Supervisor.mjs',
            'Tools/Models/Test-Native-External-Model-Gateway-Execution.mjs',
            'Tools/Native/Test-Native-External-Model-Gateway.cmd',
            'Tools/Native/Test-Native-External-Model-Gateway.sh',
            'Specifications/Native-External-Model-Gateway-Bridge.md'
        )) {
        Add-Suite 'native-external-model-gateway'
    } elseif ($Path.StartsWith('Runtime/Hosted/Models/', [StringComparison]::Ordinal) -or
        $Path -in @(
            'Tools/Models/Test-External-Model-Gateway-Core.mjs',
            'Tools/Models/Test-Supervised-External-Model-Gateway.mjs',
            'Tools/Native/Test-External-Model-Gateway.cmd',
            'Tools/Native/Test-External-Model-Gateway.sh',
            'Specifications/Supervised-External-Model-Gateway.md'
        )) {
        Add-Suite @('external-model-gateway', 'native-external-model-gateway')
    } elseif ($Path.StartsWith('Tools/Network/', [StringComparison]::Ordinal) -or
        $Path -in @(
            'Tools/Native/Test-Host-Network-Provider.cmd',
            'Tools/Native/Test-Host-Network-Provider.sh'
        )) {
        Add-Suite @('host-network-provider', 'host-tls-provider')
    } elseif ($Path.StartsWith('Tools/Models/', [StringComparison]::Ordinal) -or
        $Path -in @(
            'Tools/Native/Test-External-Model-Reference.cmd',
            'Tools/Native/Test-External-Model-Reference.sh',
            'Specifications/Windvale-External-Model-Reference.md'
        )) {
        Add-Suite 'external-model-reference'
    } elseif ($Path -in @(
        'Libraries/Models/Model-Protocol.wv',
        'Libraries/Models/Scripted-Model-Provider.wv',
        'Tests/Fixtures/Models/Model-Protocol-Self-Test.wv',
        'Projects/Libraries/Windvale-Library-Model-Protocol.wvproj',
        'Projects/Libraries/Windvale-Library-Scripted-Model-Provider.wvproj',
        'Projects/Tests/Windvale-Native-Test-Model-Protocol.wvproj',
        'Specifications/Windvale-Model-Protocol.md'
    )) {
        Add-Suite @('model-provider', 'workspace-project2', 'libraries')
    } elseif ($Path -in @(
        'Libraries/Shell/Shell-1-Parser.wv',
        'Projects/Libraries/Windvale-Library-Shell-1-Parser.wvproj',
        'Projects/Tests/Windvale-Native-Test-Shell-1-Parser.wvproj',
        'Projects/Tests/Windvale-Native-Test-Shell-1-Parser-WebAssembly-Smoke.wvproj',
        'Tests/Fixtures/Shell/Shell-1-Parser-Self-Test.wv',
        'Tests/Fixtures/Shell/Shell-1-Parser-WebAssembly-Smoke.wv',
        'Tools/Website/Verify-Shell-1-Parser-WebAssembly.mjs',
        'Specifications/Windvale-Shell-1.md'
    )) {
        Add-Suite 'shell-one-parser'
        if ($Path -eq 'Specifications/Windvale-Shell-1.md') {
            Add-Suite @('echo-application', 'file-read-application')
        }
    } elseif ($Path -in @(
        'Applications/Shell/Echo.wv',
        'Projects/Applications/Windvale-Echo.wvproj'
    )) {
        Add-Suite @('echo-application', 'echo-command-launch')
        if ($Path.StartsWith('Projects/Applications/', [StringComparison]::Ordinal)) {
            Add-Suite 'workspace-project2'
        }
    } elseif ($Path -in @(
        'Applications/Shell/File-Read.wv',
        'Projects/Applications/Windvale-File-Read.wvproj'
    )) {
        Add-Suite 'file-read-application'
        if ($Path.StartsWith('Projects/Applications/', [StringComparison]::Ordinal)) {
            Add-Suite 'workspace-project2'
        }
    } elseif ($Path -in @(
        'Projects/Libraries/Windvale-Library-Canonical-Package-Text.wvproj',
        'Projects/Libraries/Windvale-Library-Installation-Generation.wvproj',
        'Projects/Libraries/Windvale-Library-Package-Consistency.wvproj',
        'Projects/Libraries/Windvale-Library-Package-Manifest.wvproj',
        'Projects/Libraries/Windvale-Library-Package-Lock.wvproj',
        'Projects/Libraries/Windvale-Library-Package-Resource-Admission.wvproj',
        'Projects/Tools/Windvale-Installation-Activation-Planner.wvproj',
        'Projects/Tools/Windvale-Installation-Command-Resolver.wvproj',
        'Projects/Tools/Windvale-Installation-Generation-Verifier.wvproj',
        'Projects/Tests/Windvale-Native-Test-Canonical-Package-Text.wvproj',
        'Projects/Tests/Windvale-Native-Test-Installation-Generation.wvproj',
        'Projects/Tests/Windvale-Native-Test-Package-Consistency.wvproj',
        'Projects/Tests/Windvale-Native-Test-Package-Manifest.wvproj',
        'Projects/Tests/Windvale-Native-Test-Package-Lock.wvproj',
        'Projects/Tests/Windvale-Native-Test-Package-Resource-Admission.wvproj'
    )) {
        Add-Suite 'package-format'
        if ($Path -eq 'Projects/Tools/Windvale-Installation-Generation-Verifier.wvproj') {
            Add-Suite 'offline-package-stage'
        } elseif ($Path -eq 'Projects/Tools/Windvale-Installation-Command-Resolver.wvproj') {
            Add-Suite @('offline-generation-lifecycle', 'installation-command-resolution')
        } elseif ($Path -eq 'Projects/Tools/Windvale-Installation-Activation-Planner.wvproj') {
            Add-Suite 'offline-generation-lifecycle'
        }
    } elseif ($Path.StartsWith('Projects/Libraries/', [StringComparison]::Ordinal)) {
        Add-Suite 'workspace-project2'
        Add-Library-Suite-If-Owned
        if ($Path.Contains('Durable-Superblock', [StringComparison]::Ordinal)) {
            Add-Suite @('database-superblock', 'database-durable-commit')
        }
        if ($Path.Contains('Durable-Page', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Commit-Record', [StringComparison]::Ordinal) -or
            $Path.Contains('Commit-Publication', [StringComparison]::Ordinal)) {
            Add-Suite 'database-durable-commit'
        }
        if ($Path.Contains('Durable-Page', [StringComparison]::Ordinal) -or
            $Path.Contains('Storage-Publication', [StringComparison]::Ordinal) -or
            $Path.Contains('Storage-Recovery', [StringComparison]::Ordinal) -or
            $Path.Contains('Single-Writer-Commit', [StringComparison]::Ordinal) -or
            $Path.Contains('Single-Leaf-Upsert', [StringComparison]::Ordinal) -or
            $Path.Contains('Commit-Batch', [StringComparison]::Ordinal) -or
            $Path.Contains('Root-Split-Upsert', [StringComparison]::Ordinal) -or
            $Path.Contains('Depth-Two-Upsert', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Branch-Split', [StringComparison]::Ordinal) -or
            $Path.Contains('Depth-Three-Root-Growth', [StringComparison]::Ordinal) -or
            $Path.Contains('Depth-Three-Upsert', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Path-Upsert', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Path-Delete', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Node', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Leaf-Scan', [StringComparison]::Ordinal) -or
            $Path.Contains('Logical-Record', [StringComparison]::Ordinal) -or
            $Path.Contains('Schema-Definition', [StringComparison]::Ordinal) -or
            $Path.Contains('Typed-Row', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Mutations', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Leaf-Rewrite', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Paths', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Leaf-Groups', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Leaf-Partition', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Leaf-Pages', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Child-Replacements', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Branch-Partition', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Parent-Groups', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Branch-Pages', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Ancestor-Groups', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Ancestor-Pages', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Root-Growth', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Tree-Completion', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Commit', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Query-Ir', [StringComparison]::Ordinal) -or
            $Path.Contains('Sql-Lowerer', [StringComparison]::Ordinal) -or
            $Path.Contains('Json-Value', [StringComparison]::Ordinal) -or
            $Path.Contains('Json-Protocol', [StringComparison]::Ordinal) -or
            $Path.Contains('Local-Database-', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Local-', [StringComparison]::Ordinal) -or
            $Path.Contains('Collection-Catalog', [StringComparison]::Ordinal) -or
            $Path.Contains('Database-Bootstrap', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Storage-Executor', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Database-Engine', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Database-Lifecycle', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Tree-Reader', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Tree-Scan', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Tree-Path', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Tree-Delete', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Root-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Tree-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Native-Capability-Provider-Table', [StringComparison]::Ordinal)) {
            if ($DatabaseDevelopmentPaths.Contains($Path)) {
                Add-Suite 'database-storage'
            } else {
                Require-Full-Database-Storage
            }
        }
    } elseif ($Path -eq 'Specifications/Windvale-Native-Random-Containment-Tests.md') {
        $SourceContainmentCompilerDevelopmentEligible = $false
        Add-Suite @('wvb-containment', 'wvo-containment', 'source-containment')
    } elseif ($Path.StartsWith('Libraries/Package/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Package/', [StringComparison]::Ordinal)) {
        Add-Suite @('package-format', 'package-bundle')
    } elseif ($Path.StartsWith('Distribution/Applications/Echo/', [StringComparison]::Ordinal)) {
        Add-Suite @('echo-command-launch', 'package-bundle', 'package-format', 'packages')
    } elseif ($Path.StartsWith('Distribution/Applications/Wvdb-Query/', [StringComparison]::Ordinal) -and
        ([IO.Path]::GetExtension($Path) -in @('.wvapproval', '.wvlaunch'))) {
        Add-Suite @('installation-command-dispatch', 'wvdb-approval', 'offline-package-stage')
    } elseif ($Path.StartsWith('Applications/Database/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Distribution/Applications/Wvdb-Query/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Projects/Applications/', [StringComparison]::Ordinal)) {
        Add-Suite @('packages', 'package-bundle', 'wvdb-query-capability', 'wvdb-approval', 'offline-package-stage')
        if ($Path.StartsWith('Projects/Applications/', [StringComparison]::Ordinal)) {
            Add-Suite 'workspace-project2'
        }
    } elseif ($Path.StartsWith('Distribution/Applications/Wvb-Inspector/', [StringComparison]::Ordinal)) {
        Add-Suite @('packages', 'package-format', 'offline-package-stage')
        if ([IO.Path]::GetExtension($Path) -in @('.wvapproval', '.wvlaunch')) {
            Add-Suite @('installation-command-dispatch', 'wvdb-approval')
        }
    } elseif ($Path -eq 'Specifications/Windvale-Package.md') {
        Add-Suite @('packages', 'package-format')
    } elseif ($Path -eq 'Specifications/Windvale-Installation-Generation.md') {
        Add-Suite @(
            'echo-command-launch',
            'package-format',
            'installation-activation',
            'offline-generation-lifecycle',
            'installation-command-dispatch',
            'installation-command-resolution',
            'installation-generation-publication',
            'offline-package-stage'
        )
    } elseif ($Path -eq 'Specifications/Windvale-Package-Bundle.md' -or
        $Path -in @(
            'Projects/Tests/Windvale-Native-Test-Package-Bundle.wvproj',
            'Projects/Tools/Windvale-Package-Bundle-Writer.wvproj',
            'Projects/Tools/Windvale-Package-Bundle-Verifier.wvproj'
        )) {
        Add-Suite @('package-bundle', 'offline-package-stage')
    } elseif ($Path -eq 'Windvale.wvws' -or
        $Path -eq 'Specifications/Windvale-Project.md' -or
        $Path.StartsWith('Tests/Fixtures/Project/', [StringComparison]::Ordinal)) {
        Add-Suite 'workspace-project2'
        if ($Path -eq 'Specifications/Windvale-Project.md' -or
            $Path -eq 'Tests/Fixtures/Project/Language-1.0-Project3-Build.wvproj') {
            Add-Suite 'language-1-front-door'
        }
    } elseif ($Path -eq 'Libraries/Foundation/Unsafe/Unsafe.wv') {
        Add-Suite 'language-1-callable-semantics'
    } elseif ($Path -eq 'Libraries/Platform/Filesystem/File.wv') {
        Add-Suite 'language-1-memory-budget-split-execution'
    } elseif ($Path.StartsWith('Libraries/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Libraries/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Models/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Database/Database-Storage-Page-', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Database/Database-Superblock-', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Database/Database-Durable-Commit-', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Database/Database-Storage-Publication-', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Database/Database-Storage-Recovery-', [StringComparison]::Ordinal) -or
        $Path -eq 'Tests/Fixtures/Database/Database-Single-Writer-Commit-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Tree-Node-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Logical-Record-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Typed-Row-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Transaction-Mutations-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Transaction-Leaf-Rewrite-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Transaction-Paths-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Transaction-Leaf-Groups-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Transaction-Leaf-Partition-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Transaction-Leaf-Pages-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Transaction-Branch-Partition-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Transaction-Parent-Groups-Self-Test.wv' -or
        $Path.StartsWith('Tests/Fixtures/Database/Database-Transaction-Branch-Pages-', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Database/Database-Transaction-Ancestor-Groups-', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Database/Database-Transaction-Ancestor-Pages-', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Database/Database-Transaction-Root-Growth-', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Tests/Fixtures/Database/Database-Transaction-Tree-Completion-', [StringComparison]::Ordinal) -or
        $Path -eq 'Tests/Fixtures/Database/Database-Commit-Batch-Capacity-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Transaction-Commit-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Query-Ir-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Sql-Lowerer-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Json-Value-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Json-Protocol-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Local-Database-Service-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Collection-Catalog-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Bootstrap-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Single-Leaf-Upsert-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Branch-Split-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Root-Split-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Depth-Two-Upsert-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Depth-Three-Root-Growth-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Depth-Three-Upsert-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Tree-Path-Upsert-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Database-Tree-Path-Delete-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Storage-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Database-Engine-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Tree-Reader-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Tree-Scan-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Tree-Delete-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Root-Writer-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Root-Fill-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Root-Split-Writer-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Local-Put-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Local-Get-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Tree-Writer-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Logical-Tree-Writer-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Durable-Logical-Tree-Get-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Hosted-Persistent-Transaction-Writer-Self-Test.wv' -or
        $Path.StartsWith('Tests/Fixtures/Database/Native-Hosted-Snapshot-Page', [StringComparison]::Ordinal) -or
        $Path -eq 'Tests/Fixtures/Database/Native-Capability-Provider-Table-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-X64-Provider-Call-Self-Test.wv' -or
        $Path -eq 'Tests/Fixtures/Database/Native-Execution-Context-9-Self-Test.wv' -or
        $Path.StartsWith('Projects/Tests/Windvale-Native-Test-Database-Storage-Page', [StringComparison]::Ordinal) -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Durable-Superblock.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Durable-Commit.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Storage-Publication.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Storage-Recovery.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Single-Writer-Commit.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Tree-Node.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Logical-Record.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Typed-Row.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Mutations.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Rewrite.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Paths.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Groups.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Partition.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Leaf-Pages.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Partition.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Parent-Groups.wvproj' -or
        $Path.StartsWith('Projects/Tests/Windvale-Native-Test-Database-Transaction-Branch-Pages', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Groups', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Projects/Tests/Windvale-Native-Test-Database-Transaction-Ancestor-Pages', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Projects/Tests/Windvale-Native-Test-Database-Transaction-Root-Growth', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Projects/Tests/Windvale-Native-Test-Database-Transaction-Tree-Completion', [StringComparison]::Ordinal) -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Commit-Batch-Capacity.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Transaction-Commit.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Query-Ir.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Sql-Lowerer.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Json-Value.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Json-Protocol.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Local-Database-Service.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Collection-Catalog.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Bootstrap.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Single-Leaf-Upsert.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Branch-Split.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Root-Split.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Root-Growth.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Upsert.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Upsert.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Delete.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Storage.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Engine.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Reader.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Scan.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Delete.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Writer.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Fill.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Split-Writer.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Local-Put.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Local-Get.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Writer.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Writer.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Database-Persistent-Transaction-Writer.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Capability-Provider-Table.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-X64-Provider-Call.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Execution-Context-9.wvproj' -or
        $Path -eq 'Projects/Tests/Windvale-Native-Test-Native-Hosted-Snapshot-Page.wvproj' -or
        $Path -eq 'Specifications/Windvale-Model-Protocol.md' -or
        $Path.StartsWith('Specifications/Windvale-Database', [StringComparison]::Ordinal) -or
        $Path -in @(
            'Specifications/Read-Only-Directory-Capability.md',
            'Specifications/Random-Access-Storage-Capability.md',
            'Specifications/Windvale-Native-Capability-Provider-Table.md',
            'Specifications/Windvale-Native-Provider-Call.md',
            'Specifications/Windvale-Native-Execution-Context-9-Construction.md'
        )) {
        Add-Library-Suite-If-Owned
        if ($DatabaseDevelopmentContractTargets.ContainsKey($Path)) {
            Add-Suite 'database-storage'
        }
        if ($Path.Contains('Native-Hosted-Snapshot-Page', [StringComparison]::Ordinal)) {
            Add-Suite 'native-u64-lowering'
        }
        if ($Path.Contains('Durable-Superblock', [StringComparison]::Ordinal) -or
            $Path.Contains('Database-Superblock', [StringComparison]::Ordinal)) {
            Add-Suite @('database-superblock', 'database-durable-commit')
        }
        if ($Path.Contains('Durable-Page', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Commit', [StringComparison]::Ordinal) -or
            $Path.Contains('Commit-Publication', [StringComparison]::Ordinal)) {
            Add-Suite 'database-durable-commit'
        }
        if ($Path.Contains('Durable-Page', [StringComparison]::Ordinal) -or
            $Path.Contains('Storage-Publication', [StringComparison]::Ordinal) -or
            $Path.Contains('Storage-Recovery', [StringComparison]::Ordinal) -or
            $Path.Contains('Single-Writer-Commit', [StringComparison]::Ordinal) -or
            $Path.Contains('Single-Leaf-Upsert', [StringComparison]::Ordinal) -or
            $Path.Contains('Commit-Batch', [StringComparison]::Ordinal) -or
            $Path.Contains('Root-Split', [StringComparison]::Ordinal) -or
            $Path.Contains('Depth-Two-Upsert', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Branch-Split', [StringComparison]::Ordinal) -or
            $Path.Contains('Depth-Three-Root-Growth', [StringComparison]::Ordinal) -or
            $Path.Contains('Depth-Three-Upsert', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Path-Upsert', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Path-Delete', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Node', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Leaf-Scan', [StringComparison]::Ordinal) -or
            $Path.Contains('Tree-Leaf-Operations', [StringComparison]::Ordinal) -or
            $Path.Contains('Logical-Record', [StringComparison]::Ordinal) -or
            $Path.Contains('Schema-Definition', [StringComparison]::Ordinal) -or
            $Path.Contains('Typed-Row', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Mutations', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Leaf-Rewrite', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Paths', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Leaf-Groups', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Leaf-Partition', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Leaf-Pages', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Child-Replacements', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Branch-Partition', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Parent-Groups', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Branch-Pages', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Ancestor-Groups', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Ancestor-Pages', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Root-Growth', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Tree-Completion', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Commit', [StringComparison]::Ordinal) -or
            $Path.Contains('Transaction-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Query-Ir', [StringComparison]::Ordinal) -or
            ($Path.Contains('Sql-Lowerer', [StringComparison]::Ordinal) -or
                $Path -eq 'Specifications/Windvale-Database-Sql.md') -or
            $Path.Contains('Json-Value', [StringComparison]::Ordinal) -or
            $Path.Contains('Json-Protocol', [StringComparison]::Ordinal) -or
            $Path.Contains('Local-Database-', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Local-', [StringComparison]::Ordinal) -or
            $Path.Contains('Collection-Catalog', [StringComparison]::Ordinal) -or
            $Path.Contains('Database-Bootstrap', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Storage-Executor', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Database-Engine', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Database-Lifecycle', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Tree-Reader', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Tree-Scan', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Tree-Path', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Tree-Delete', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Root-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Root-Split-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Tree-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Durable-Logical-Tree-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Host-Tree-Reader', [StringComparison]::Ordinal) -or
            $Path.Contains('Host-Tree-Scan', [StringComparison]::Ordinal) -or
            $Path.Contains('Host-Tree-Delete', [StringComparison]::Ordinal) -or
            $Path.Contains('Host-Root-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Host-Root-Fill', [StringComparison]::Ordinal) -or
            $Path.Contains('Host-Root-Split-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Host-Local-', [StringComparison]::Ordinal) -or
            $Path.Contains('Host-Tree-Writer', [StringComparison]::Ordinal) -or
            $Path.Contains('Host-Logical-Tree-', [StringComparison]::Ordinal) -or
            $Path.Contains('Native-Hosted-Durable-Storage', [StringComparison]::Ordinal) -or
            $Path.Contains('Database-Host-Storage', [StringComparison]::Ordinal)) {
            if ($DatabaseDevelopmentPaths.Contains($Path)) {
                Add-Suite 'database-storage'
            } else {
                Require-Full-Database-Storage
            }
        }
        if ($Path -in @(
            'Libraries/Database/Wvdb-Reader.wv',
            'Libraries/Platform/Filesystem/Read-Only-Directory.wv',
            'Libraries/Platform/Database/Read-Only-Wvdb.wv'
        )) {
            Add-Suite 'packages'
        }
    } elseif ($Path.StartsWith('Tools/Database/', [StringComparison]::Ordinal)) {
        Add-Suite 'database-storage'
    } elseif ($Path -in @(
        'Libraries/Platform/Operations/Bounded-Operation.wv',
        'Projects/Libraries/Windvale-Library-Bounded-Operation.wvproj',
        'Tests/Fixtures/Libraries/Bounded-Operation-Self-Test.wv',
        'Projects/Tests/Windvale-Native-Test-Bounded-Operation.wvproj',
        'Specifications/Windvale-Bounded-Operation.md'
    )) {
        Add-Suite @('bounded-operation', 'native-u64-lowering')
    } elseif ($Path -in @(
        'Libraries/Platform/Networking/Network-Authority.wv',
        'Projects/Libraries/Windvale-Library-Network-Authority.wvproj',
        'Tests/Fixtures/Libraries/Network-Authority-Self-Test.wv',
        'Projects/Tests/Windvale-Native-Test-Network-Authority.wvproj',
        'Specifications/Windvale-Network-Authority.md'
    )) {
        Add-Suite @('os-network-authority', 'native-u64-lowering')
    } elseif ($Path -in @(
        'Libraries/Platform/Filesystem/Filesystem-Semantics.wv',
        'Projects/Libraries/Windvale-Library-Filesystem-Semantics.wvproj',
        'Tests/Fixtures/Libraries/Filesystem-Semantics-Self-Test.wv',
        'Projects/Tests/Windvale-Native-Test-Filesystem-Semantics.wvproj',
        'Specifications/Windvale-Filesystem-Semantics.md'
    )) {
        Add-Suite @('filesystem-semantics', 'native-u64-lowering')
    } elseif ($Path.StartsWith('Operating-System/', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Projects/Operating-System/', [StringComparison]::Ordinal)) {
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
    } elseif ($Path -eq 'Runtime/Windvale/Filesystem-Host-Adapter-Core.wv') {
        Add-Suite @('os-filesystem-service', 'native-u64-lowering')
    } elseif ($Path -in @(
        'Runtime/Native/X64-Read-Only-Directory-Host.wva',
        'Runtime/Native/Windows-X64-Read-Only-Directory.wva',
        'Runtime/Native/Linux-X64-Read-Only-Directory.wva'
    )) {
        Add-Assembler-Suites
        Add-Suite @('wvdb-query-capability', 'file-read-application')
    } elseif ($Path -in @(
        'Runtime/Native/X64-File-Read-Host.wva',
        'Runtime/Native/Windows-X64-Standard-Byte-Output.wva',
        'Runtime/Native/Linux-X64-Standard-Byte-Output.wva'
    )) {
        Add-Assembler-Suites
        Add-Suite 'file-read-application'
    } elseif ($Path -eq 'Runtime/Native/X64-Scripted-Model-Provider-Host.wva') {
        Add-Assembler-Suites
        Add-Suite 'model-provider'
    } elseif ($Path -in @(
        'Runtime/Windvale/Native-Capability-Provider-Table-Core.wv',
        'Runtime/Windvale/Native-Capability-Provider-Table-Core.wvproj',
        'Runtime/Windvale/Native-Capability-Provider-Table-Bridge.wv',
        'Runtime/Windvale/Native-Capability-Provider-Table.wvproj',
        'Compiler/Windvale/Native-X64-Provider-Call.wv',
        'Runtime/Windvale/Native-Execution-Context-9-Core.wv',
        'Runtime/Windvale/Native-Execution-Context-9-Core.wvproj',
        'Runtime/Windvale/Native-Execution-Context-9-Bridge.wv',
        'Runtime/Windvale/Native-Execution-Context-9.wvproj',
        'Runtime/Native/X64-Segmented-Hosted-Main-Trampoline.wva',
        'Runtime/Native/X64-Random-Access-Storage-Describe-Probe.wva',
        'Runtime/Native/X64-Random-Access-Storage-Host.wva',
        'Runtime/Native/Windows-X64-Random-Access-Storage.wva',
        'Runtime/Native/Linux-X64-Random-Access-Storage.wva'
    )) {
        Add-Suite 'database-storage'
    } elseif ($Path -in @(
        'Compiler/Windvale/Native-X64-Lowering-Sha256.wv',
        'Projects/Tests/Windvale-Native-Test-Sha256-Native-Kat.wvproj',
        'Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Sha256.wvproj',
        'Tests/Fixtures/Native-X64/Sha256-Native-Kat.wv',
        'Tests/Fixtures/Native-X64/Wvb-To-Wvo-Sha256-Smoke.wv'
    )) {
        Add-Suite 'native-sha256-lowering'
    } elseif ($Path -in @(
        'Compiler/Windvale/Native-X64-Lowering-Core.wv',
        'Compiler/Windvale/Native-X64-Lowering-Affine-Ownership.wv',
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
        'Compiler/Windvale/Native-X64-Lowering-Memory-Budget.wv',
        'Compiler/Windvale/Native-X64-Lowering-Object.wv',
        'Compiler/Windvale/Native-X64-Lowering-Publication.wv',
        'Compiler/Windvale/Native-X64-Lowering-Records.wv',
        'Compiler/Windvale/Native-X64-Lowering-Record-Allocation.wv',
        'Compiler/Windvale/Native-X64-Lowering-Record-Instructions.wv',
        'Compiler/Windvale/Native-X64-Lowering-Record-Local-Liveness.wv',
        'Compiler/Windvale/Native-X64-Lowering-Record-Storage.wv',
        'Compiler/Windvale/Native-X64-Lowering-Variant-Analysis.wv',
        'Compiler/Windvale/Native-X64-Lowering-Variant-Instructions.wv',
        'Compiler/Windvale/Native-X64-Lowering-Variant-Storage.wv',
        'Compiler/Windvale/Native-X64-Lowering-Runtime-Descriptors.wv',
        'Compiler/Windvale/Native-X64-Lowering-Static-Data-Instructions.wv',
        'Compiler/Windvale/Native-X64-Lowering-Types.wv',
        'Compiler/Windvale/Native-X64-Lowering-Unsafe-Scratch.wv',
        'Compiler/Windvale/Native-X64-Lowering-Unsafe-Write-Region.wv'
    )) {
        Add-Suite 'native-x64-lowering-development'
    } elseif ($Path -in @(
        'Compiler/Windvale/Native-X64-Lowering-Staging-Manifest.wv',
        'Compiler/Windvale/Native-X64-Lowering-Staging-Tool.wv',
        'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Envelope.wv',
        'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Relocations-Native-Bridge.wv',
        'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Relocations.wv',
        'Compiler/Windvale/Native-X64-Lowering-Staging-Wvo-Symbols.wv'
    )) {
        Add-Suite 'segmented-compiler-toolset-reconstruction'
    } elseif ($Path -eq 'Compiler/Windvale/Native-X64-Lowering-Tool.wv') {
        Add-Suite 'native-x64-lowering-development'
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Generic-Resolution-Core.wv',
        'Specifications/Compiler-Source-Generic-Resolution.md'
    )) {
        Add-Suite @('source-containment', 'language-1-front-door')
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Generic-Lowering-Core.wv',
        'Compiler/Windvale/Source-Bindings-Generic-Types-Core.wv',
        'Compiler/Windvale/Source-Generic-Type-Binding-Core.wv',
        'Compiler/Windvale/Source-Generic-Type-Layout-Core.wv',
        'Compiler/Windvale/Source-Generic-Type-Materialization-Core.wv',
        'Compiler/Windvale/Source-Generic-Type-Lowering-Core.wv',
        'Specifications/Compiler-Source-Generic-Types.md'
    )) {
        Add-Suite @(
            'source-containment',
            'generic-nominal-type-binding',
            'generic-nominal-type-layout',
            'generic-nominal-type-materialization',
            'generic-nominal-wvlb-carrier',
            'language-1-front-door',
            'language-1-memory-budget-split-execution'
        )
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Lexer-Core.wv',
        'Compiler/Windvale/Source-Declaration-Parser.wv',
        'Compiler/Windvale/Source-Body-Parser.wv',
        'Compiler/Windvale/Source-Profile-Core.wv',
        'Compiler/Windvale/Source-Set-Core.wv',
        'Compiler/Windvale/Source-Graph-Core.wv',
        'Compiler/Windvale/Source-Symbols-Core.wv',
        'Compiler/Windvale/Source-Bindings-Core.wv'
    )) {
        Add-Source-Front-End-Suites
        if ($Path -in @(
            'Compiler/Windvale/Source-Lexer-Core.wv',
            'Compiler/Windvale/Source-Declaration-Parser.wv',
            'Compiler/Windvale/Source-Profile-Core.wv',
            'Compiler/Windvale/Source-Set-Core.wv'
        )) {
            Add-Suite 'language-1-source-admission-coordinator'
        }
        if ($Path -in @(
            'Compiler/Windvale/Source-Lexer-Core.wv',
            'Compiler/Windvale/Source-Declaration-Parser.wv',
            'Compiler/Windvale/Source-Body-Parser.wv',
            'Compiler/Windvale/Source-Set-Core.wv'
        )) {
            Add-Suite 'language-1-foreign-catalog-producer'
        }
        if ($Path -in @(
            'Compiler/Windvale/Source-Lexer-Core.wv',
            'Compiler/Windvale/Source-Declaration-Parser.wv',
            'Compiler/Windvale/Source-Set-Core.wv'
        )) {
            Add-Suite 'language-1-effect-clause-front-end'
            Add-Suite 'language-1-system-ffi-front-end'
        }
        if ($Path -in @(
            'Compiler/Windvale/Source-Lexer-Core.wv',
            'Compiler/Windvale/Source-Body-Parser.wv',
            'Compiler/Windvale/Source-Set-Core.wv'
        )) {
            Add-Suite 'language-1-using-front-end'
        }
        if ($Path -eq 'Compiler/Windvale/Source-Body-Parser.wv') {
            Add-Suite 'language-1-system-ffi-unsafe-context'
        }
        if ($Path -in @(
            'Compiler/Windvale/Source-Lexer-Core.wv',
            'Compiler/Windvale/Source-Declaration-Parser.wv',
            'Compiler/Windvale/Source-Body-Parser.wv',
            'Compiler/Windvale/Source-Set-Core.wv',
            'Compiler/Windvale/Source-Bindings-Core.wv'
        )) {
            Add-Suite 'language-1-callable-semantics'
        }
        if ($Path -in @(
            'Compiler/Windvale/Source-Symbols-Core.wv',
            'Compiler/Windvale/Source-Bindings-Core.wv'
        )) {
            Add-Suite 'language-1-authenticated-foreign-binding'
        }
    } elseif ($Path -in @(
        'Compiler/Windvale/Source-Lexer-Core.wv',
        'Compiler/Windvale/Source-Declaration-Parser.wv',
        'Compiler/Windvale/Source-Body-Parser.wv',
        'Compiler/Windvale/Source-Profile-Core.wv',
        'Compiler/Windvale/Source-Set-Core.wv',
        'Compiler/Windvale/Source-Graph-Core.wv',
        'Compiler/Windvale/Source-Symbols-Core.wv',
        'Compiler/Windvale/Source-Bindings-Core.wv',
        'Compiler/Windvale/Source-Wir-Consumer-Core.wv',
        'Compiler/Windvale/Source-Wir-Core.wv',
        'Compiler/Windvale/Source-Wvb-Compilation-Core.wv',
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
        'Tests/Fixtures/Source-Wvb/Hosted-Capabilities.wv',
        'Tests/Fixtures/Source-Wvb/Pruning.wv',
        'Tests/Fixtures/Source-Wvb/Try-Propagation.wv',
        'Projects/Compiler/Windvale-Source-Lexer-Core.wvproj',
        'Projects/Examples/Windvale-Source-Lexer-Demo.wvproj',
        'Projects/Compiler/Windvale-Source-Declaration-Parser.wvproj',
        'Projects/Examples/Windvale-Source-Declaration-Parser-Demo.wvproj',
        'Projects/Examples/Windvale-Source-Declaration-Parser-Tool.wvproj',
        'Projects/Compiler/Windvale-Source-Body-Parser.wvproj',
        'Projects/Examples/Windvale-Source-Body-Parser-Demo.wvproj',
        'Projects/Examples/Windvale-Source-Body-Parser-Tool.wvproj',
        'Projects/Compiler/Windvale-Source-Profile-Core.wvproj',
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
        if ($Path.StartsWith('Compiler/Windvale/Source-', [StringComparison]::Ordinal) -or
            $Path.StartsWith('Examples/Compiler/Source-', [StringComparison]::Ordinal) -or
            $Path.StartsWith('Projects/Compiler/Windvale-Source-', [StringComparison]::Ordinal) -or
            $Path.StartsWith('Projects/Examples/Windvale-Source-', [StringComparison]::Ordinal) -or
            $Path -eq 'Projects/Examples/Windvale-Compiler.wvproj' -or
            $Path.StartsWith('Tests/Fixtures/Source-Wvb/', [StringComparison]::Ordinal)) {
            Add-Suite 'language-1-front-door'
        }
        if ($Path -in @(
            'Compiler/Windvale/Source-Declaration-Parser.wv',
            'Compiler/Windvale/Source-Profile-Core.wv',
            'Compiler/Windvale/Source-Set-Core.wv',
            'Compiler/Windvale/Source-Symbols-Core.wv',
            'Compiler/Windvale/Source-Bindings-Core.wv',
            'Compiler/Windvale/Source-Lexer-Core.wv',
            'Compiler/Windvale/Source-Wir-Core.wv',
            'Compiler/Windvale/Source-Wvb-Core.wv',
            'Projects/Compiler/Windvale-Source-Profile-Core.wvproj'
        )) {
            Add-Suite 'language-1-front-door'
        }
        if ($Path -in @(
            'Compiler/Windvale/Source-Wvb-Core.wv',
            'Compiler/Windvale/Source-Wvb-Temporary-Slots.wv',
            'Projects/Compiler/Windvale-Source-Wvb-Core.wvproj',
            'Projects/Examples/Windvale-Source-Wvb-Demo.wvproj'
        )) {
            Add-Suite 'segmented-compiler-toolset-reconstruction'
        }
        if ($Path -eq 'Compiler/Windvale/Source-Wvb-Core.wv') {
            Add-Suite 'language-1-memory-budget-split-execution'
        }
        if ($Path -in @(
            'Compiler/Windvale/Source-Wir-Consumer-Core.wv',
            'Compiler/Windvale/Source-Wir-Core.wv',
            'Compiler/Windvale/Source-Wvb-Compilation-Core.wv',
            'Compiler/Windvale/Source-Wvb-Core.wv'
        )) {
            Add-Suite 'language-1-callable-semantics'
        }
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
        'Runtime/Windvale/Native-Hosted-Enum-Service-Request.wv',
        'Runtime/Windvale/Native-Hosted-Fixed-Services-Tool.wv',
        'Runtime/Windvale/Native-Hosted-Container-Runtime-Tool.wv',
        'Runtime/Windvale/Native-Hosted-Orchestration-Control-Core.wv',
        'Runtime/Windvale/Native-Hosted-Source-Geometry-Tool.wv',
        'Runtime/Windvale/Native-Hosted-Tool-Metadata-Request-Tool.wv',
        'Runtime/Windvale/Streaming-Sha256-Evidence-Core.wv'
    )) {
        Add-Bytecode-Suites
        Add-Hosted-Publisher-Suites
        Add-Suite @(
            'console-packager-container-reconstruction',
            'seed-native-front-door'
        )
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
        if ($Path -eq 'Foundation/Sha256.wv') {
            Add-Suite 'language-1-foreign-catalog-producer'
            Add-Suite 'language-1-source-admission-coordinator'
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
        'Linker/Startup/Windows-X64-Hosted-Compiler.wva',
        'Linker/Reference/Consumers/Windows-X64-Hosted-Compiler.wvo'
    )) {
        Add-Assembler-Suites
        Add-Linker-Suites
        Add-Hosted-Publisher-Suites
        Add-Suite 'seed-native-front-door'
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
        'Artifacts/Native-Os-Probe-Object-Producer-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite @('os-probe-object', 'os-probe')
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Os-Process-Object-Toolset-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite @('os-process-object', 'os-probe')
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
        Add-Suite @('wvb-runner-reconstruction', 'scripting')
        Add-Suite 'seed-native-front-door'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wvb-Runner-0.1.0/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'installers'
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wvb-Verifier-0.1.0/',
        [StringComparison]::Ordinal)) {
        Add-Suite 'installers'
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
        Add-Suite @(
            'wv-linker-reconstruction',
            'console-verifier-reconstruction',
            'compiler-split-development'
        )
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Hosted-Enum-Request-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Hosted-Publisher-Suites
        Add-Suite @('native-u64-lowering', 'wv-linker-reconstruction')
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Wv-Linker-Candidate/',
        [StringComparison]::Ordinal)) {
        Add-Suite @(
            'wv-linker-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'installers'
        )
    } elseif ($Path.StartsWith(
        'Artifacts/Native-Front-Door/',
        [StringComparison]::Ordinal)) {
        Add-Suite @(
            'seed-native-front-door',
            'wvb-runner-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'installers'
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
        Add-Suite @(
            'wvb-runner-reconstruction',
            'scripting',
            'language-1-memory-budget-split-execution'
        )
        Add-Suite 'seed-native-front-door'
    } elseif ($Path -eq
        'Tools/Windvale.Verify/Console-Application-Verifier-Tool.wv') {
        Add-Suite 'console-verifier-reconstruction'
    } elseif ($Path -eq
        'Tools/Windvale.Verify/Native-X64-Provider-Call-Verification.wv') {
        Require-Full-Database-Storage
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
    } elseif ($Path -match 'X64-Process-Filesystem-(?:Record|Paging|Image|Machine)-Emission') {
        Add-Suite 'os-x64-filesystem-machine-emission'
    } elseif ($Path -match 'X64-Process-Final-State-Validation-Epilogue-Emission') {
        Add-Suite 'os-x64-code-emission'
    } elseif ($Path.StartsWith('Tests/Fixtures/Operating-System/Os-', [StringComparison]::Ordinal) -or
        $Path.StartsWith('Projects/Tests/Windvale-Native-Test-Os-', [StringComparison]::Ordinal)) {
        if ($Path -match 'X64-(?:Code|Process-(?:Entry|Privileged-Entry|Thread-Timer-State|Timer-Activation|Init-Reply-Publish-Resume|Directory-Reply-Publish-Resume|Init-Return-(?:Program-Validation|Budget-Validation|Store-Directory-Validation)|Provider-(?:User-Transfer|Return-Init-Transfer)|Coordinator|Endpoint|Memory-Allocation|Record|Paging|Image|Client-(?:Reservation|Record|Paging|Image|User-Transfer|Return-Init-Transfer|Reply-Delivery|Directory-Request-Delivery|Directory-Reply-Delivery|Completion-Cleanup|Reclamation-Preflight|Memory-Recycle|Generation-Two-(?:Record|Paging|Image|Endpoint-Rebind|Reentry|Return-Validation|User-Transfer|Return-Init-Transfer|Init-Reply-Publish-Resume|Reply-Delivery|Directory-Request-Delivery|Directory-Reply-Lifecycle|Completion-Cleanup|Completion-Finalize-Resume)|Program-Resource|Budget-Resource|Store-Resource|Directory-Resource|Store-Validation|Directory-Validation)|Directory-(?:Allocation|Record|Paging|Image|Generation-Two-Reply-Publish-Resume)))-Emission') {
            Add-Suite 'os-x64-code-emission'
        } elseif ($Path.Contains('Provider-Launch-', [StringComparison]::Ordinal)) {
            Add-Suite 'os-provider-launch-transaction'
        } elseif ($Path.Contains('Endpoint-Transfer-Profile', [StringComparison]::Ordinal)) {
            Add-Suite @('os-endpoint-transfer', 'native-u64-lowering')
        } elseif ($Path.Contains('Fat32-Block-Read', [StringComparison]::Ordinal) -or
            $Path.Contains('Fat32-Block-Provider', [StringComparison]::Ordinal) -or
            $Path.Contains('Fat32-Block-Exchange', [StringComparison]::Ordinal) -or
            $Path.Contains('Fat32-Block-Image', [StringComparison]::Ordinal)) {
            Add-Suite @('os-fat32-block-read', 'native-u64-lowering')
        } elseif ($Path.Contains('Fat32-Chain-Position', [StringComparison]::Ordinal) -or
            $Path.Contains('Fat32-File-Read-Transaction', [StringComparison]::Ordinal)) {
            Add-Suite @('os-fat32-file-read', 'native-u64-lowering')
        } elseif ($Path.Contains('Fat32-Volume-Admission', [StringComparison]::Ordinal) -or
            $Path.Contains('Fat32-Cluster-Chain', [StringComparison]::Ordinal) -or
            $Path.Contains('Fat32-Directory-Admission', [StringComparison]::Ordinal) -or
            $Path.Contains('Fat32-File-Read-Plan', [StringComparison]::Ordinal)) {
            Add-Suite @('os-fat32-volume', 'native-u64-lowering')
        } elseif ($Path.Contains('Filesystem-Service', [StringComparison]::Ordinal)) {
            Add-Suite @('os-filesystem-service', 'native-u64-lowering')
        } elseif ($Path.Contains('Application-Launch', [StringComparison]::Ordinal) -or
            $Path.Contains('Application-Start-', [StringComparison]::Ordinal) -or
            $Path.Contains('Application-Machine-Construction', [StringComparison]::Ordinal)) {
            Add-Suite 'os-application-launch'
        } elseif ($Path.Contains('Resource-Domain', [StringComparison]::Ordinal)) {
            Add-Suite 'os-resource-domain'
        } else {
            Add-Suite 'os-services'
        }
    } elseif ($Path -eq 'Tests/Fixtures/Native-X64/Baseline-Jit-Patch-Plan-Self-Test.wv') {
        Add-Suite 'baseline-jit'
    } elseif ($Path -eq 'Tests/Fixtures/Native-X64/Wvb-To-Wvo-Return-42.wv') {
        Add-Suite 'wvb-to-wvo-reconstruction'
    } elseif ($Path -in @(
        'Tests/Fixtures/Native-X64/Native-X64-Lowering-Metadata-Self-Test.wv',
        'Tests/Fixtures/Native-X64/Wvb-To-Wvo-Metadata.wv'
    )) {
        Add-Suite 'wvb-to-wvo-reconstruction'
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Wvb-Metadata-Normalization-Self-Test.wvproj',
        'Tests/Fixtures/Source-Wvb/Metadata-Normalization-Self-Test.wv'
    )) {
        Add-Suite 'wvb-to-wvo-reconstruction'
    } elseif ($Path -eq 'Tests/Fixtures/Native-X64/Wvb-To-Wvo-U64.wv') {
        Add-Suite 'native-u64-lowering'
    } elseif ($Path -eq 'Tests/Native/Random-Containment/Corpus.tar.gz.b64') {
        $SourceContainmentCompilerDevelopmentEligible = $false
        Add-Suite @('wvb-containment', 'wvo-containment', 'source-containment')
    } elseif ($Path -eq 'Tests/Fixtures/Native-X64/Nested-Record-Fields.wv') {
        Require-Full-Database-Storage
    } elseif ($Path -eq 'Tests/Native/Plan.txt' -or
        $Path.StartsWith('Tests/Native/Malformed-Wvb/', [StringComparison]::Ordinal)) {
        Add-Suite 'seed'
    } elseif ($Path -in @(
        'Tests/Native/X64-Application-Start-Publication-Self-Test.wva',
        'Tests/Native/X64-Application-Start-User-Copy-Self-Test.wva',
        'Tests/Native/X64-Application-Start-Syscall-Context-Self-Test.wva'
    )) {
        Add-Suite 'os-application-launch'
    } elseif ($Path.StartsWith('Tests/Native/Wvo/', [StringComparison]::Ordinal)) {
        Add-Object-Suites
    } elseif ($Path.StartsWith('Tests/Native/', [StringComparison]::Ordinal)) {
        Add-Gap "native-test:$([IO.Path]::GetFileName($Path))"
    } elseif ($Path.StartsWith('Tests/', [StringComparison]::Ordinal)) {
        Add-Gap 'managed-test-recovery-source'
    } elseif ($Path.StartsWith('Specifications/', [StringComparison]::Ordinal)) {
        if ($Path -in @(
            'Specifications/Windvale-Language-1.0.md',
            'Specifications/Windvale-Language-1.0-Grammar.md',
            'Specifications/Windvale-Language-1.0.ebnf',
            'Specifications/Windvale-Language-1.0-Localized-Source.md',
            'Specifications/Windvale-Language-1.0-Source-Profile-Formats.md',
            'Specifications/Windvale-Language-1.0-Foundation.md',
            'Specifications/Windvale-Language-1.0-Foundation-Registry.md'
        )) {
            # These candidates do not describe the implemented Seed surface. Keep
            # their routing checked without claiming current compiler conformance;
            # source freeze must replace this with the affected implementation owners.
            $RunPlanVerification = $true
        } elseif ($Path -eq 'Specifications/Seed-Conformance.md') {
            $RunPlanVerification = $true
        } elseif ($Path -in @(
            'Specifications/Windvale-Development-Installer.md',
            'Specifications/Windvale-Installer.md'
        )) {
            Add-Suite 'installers'
        } elseif ($Path -eq 'Specifications/Windvale-Installer-Repository.md') {
            Add-Suite 'installer-repository'
        } elseif ($Path -eq 'Specifications/Windvale-Release-Envelope.md') {
            Add-Suite @(
                'release-envelope',
                'offline-package-stage',
                'installer-repository'
            )
        } elseif ($Path -eq 'Specifications/Windvale-Capability-Approval-And-Launch.md') {
            Add-Suite @('echo-command-launch', 'wvdb-approval')
        } elseif ($Path -in @(
            'Specifications/Windvale-Resource-Service-Ipc.md',
            'Specifications/Windvale-Directory-Service-Ipc.md',
            'Specifications/Windvale-Directory-Snapshot.md'
        )) {
            Add-Os-Suite $Path
        } elseif ($Path -eq 'Specifications/Seed-CLI.md') {
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
        } elseif ($Path -eq
            'Specifications/Windvale-Native-Wvb-Unsafe-Rejection-Tests.md') {
            Add-Suite 'unsafe-wvb'
        } elseif ($Path -eq 'Specifications/Windvale-WebAssembly.md') {
            Add-WebAssemblyVerification
        } elseif ($Path -eq 'Specifications/Windvale-Native-Compiler-Reconstruction.md') {
            Add-Suite 'compiler-reconstruction'
        } elseif ($Path -in @(
            'Specifications/Windvale-Linking.md',
            'Specifications/Windvale-Segmented-Hosted-Overlay.md',
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
                if ($Path -eq 'Specifications/Windvale-Segmented-Hosted-Overlay.md') {
                    Add-Suite 'database-storage'
                }
            }
        } elseif ($Path -eq 'Specifications/Windvale-Native-Wvb-To-Wvo.md') {
            Add-Suite 'wvb-to-wvo-reconstruction'
        } elseif ($Path -eq
            'Specifications/Windvale-Native-Wvb-To-Wvo-Rejection-Tests.md') {
            Add-Suite 'lowerer-rejections'
        } elseif ($Path -eq 'Specifications/Windvale-Native-Wvb-Runner.md') {
            Add-Suite 'seed-native-front-door'
            Add-Suite 'language-1-memory-budget-split-execution'
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
        } elseif ($Path -in @(
            'Specifications/Windvale-Native-Hosted-Container-Construction.md',
            'Specifications/Windvale-Native-Hosted-Container-Metadata.md',
            'Specifications/Windvale-Native-Hosted-Container-Planner.md',
            'Specifications/Windvale-Native-Hosted-Container-Platform-Bytes.md',
            'Specifications/Windvale-Native-Hosted-Container-Segment-Manifest.md',
            'Specifications/Windvale-Native-Hosted-Container-Segment-Request.md',
            'Specifications/Windvale-Native-Hosted-Container-Segment-Set.md',
            'Specifications/Windvale-Native-Hosted-Container-Startup.md',
            'Specifications/Windvale-Native-Hosted-Enum-Processes.md',
            'Specifications/Windvale-Native-Hosted-Fixed-Services.md',
            'Specifications/Windvale-Native-Hosted-Metadata-Request.md',
            'Specifications/Windvale-Native-Hosted-Orchestration-Control.md',
            'Specifications/Windvale-Native-Hosted-Publication-Request.md',
            'Specifications/Windvale-Native-Hosted-Service-Bundle-Request.md',
            'Specifications/Windvale-Native-Hosted-Service-Bundle.md',
            'Specifications/Windvale-Native-Hosted-Startup-Instantiation.md',
            'Specifications/Windvale-Native-Hosted-Tool-Metadata-Construction.md',
            'Specifications/Windvale-Native-Publication-Lifetime.md',
            'Specifications/Windvale-Native-Publication-Plan.md',
            'Specifications/Windvale-Native-Service-Bundle-Materialization.md',
            'Specifications/Windvale-Native-Streaming-Sha256-Evidence.md'
        )) {
            Add-Hosted-Publisher-Suites
            Add-Suite @(
                'wv-linker-reconstruction',
                'console-packager-container-reconstruction',
                'seed-native-front-door'
            )
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
            $null = $SelectedDatabaseDevelopmentTargets.Add('host-storage')
            Add-Suite @('native-u64-lowering', 'model-provider', 'database-storage', 'wvb-to-wvo-reconstruction')
        } elseif ($Path -eq
            'Specifications/Compiler-Source-Foreign-Lowering-Carrier.md') {
            Add-Suite @(
                'language-1-production-admission-ingress',
                'language-1-authenticated-foreign-binding'
            )
        } elseif ($Path -eq 'Specifications/Compiler-Source-Body-Parser.md') {
            Add-Compiler-Suites
            Add-Suite @(
                'language-1-using-front-end',
                'language-1-system-ffi-unsafe-context'
            )
        } elseif ($Path -in @(
            'Specifications/Seed-Language.md',
            'Specifications/Seed-Records.md',
            'Specifications/Compiler-Source-Symbols.md',
            'Specifications/Compiler-Source-Bindings.md',
            'Specifications/Compiler-Source-Wir.md'
        )) {
            Add-Compiler-Suites
            Add-Suite 'language-1-front-door'
            if ($Path -eq 'Specifications/Seed-Language.md') {
                Add-Suite 'language-1-using-front-end'
            }
            if ($Path -in @(
                'Specifications/Compiler-Source-Symbols.md',
                'Specifications/Compiler-Source-Bindings.md'
            )) {
                Add-Suite 'language-1-authenticated-foreign-binding'
            }
            if ($Path -eq 'Specifications/Compiler-Source-Bindings.md') {
                Add-Suite 'language-1-callable-semantics'
            }
            if ($Path -eq 'Specifications/Compiler-Source-Wir.md') {
                Add-Suite 'language-1-callable-semantics'
            }
        } elseif ($Path -eq 'Specifications/Windvale-Uefi-Application.md') {
            Add-Suite 'uefi-packager'
        } elseif ($Path -eq 'Specifications/Wv-Dump-Core.md') {
            Add-Object-Suites
            Add-Suite 'wvo-inspector-reconstruction'
        } elseif ($Path -eq 'Specifications/Windvale-Native-Test-Plan.md') {
            $RunPlanVerification = $true
        } elseif ($Path -eq 'Specifications/Windvale-Native-Tool-Checkpoint.md') {
            Add-Suite 'database-storage'
        } elseif ($Path -eq 'Specifications/Windvale-Console-Application-Verification.md') {
            Add-Suite 'console-verifier-reconstruction'
        } elseif ($Path -eq 'Specifications/Windvale-Hosted-Verifier-Application.md') {
            Add-Hosted-Publisher-Suites
            Add-Suite 'console-verifier-reconstruction'
        } elseif ($Path -eq 'Specifications/Windvale-Protected-Process.md') {
            Add-Suite @('os-process-policy', 'os-process-object', 'os-probe')
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
    } elseif ($Path -eq 'Projects/Tools/Windvale-Compiler-Build-Driver.wvproj') {
        Add-Compiler-Suites
        Add-Suite @(
            'language-1-front-door',
            'workspace-project2',
            'segmented-compiler-toolset-reconstruction'
        )
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
        Add-Suite @(
            'wvb-runner-reconstruction',
            'language-1-memory-budget-split-execution'
        )
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
        'Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Admission.wvproj',
        'Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj',
        'Projects/Tests/Windvale-Native-Test-Staging-Content-Native.wvproj',
        'Projects/Linker/Windvale-Compiler-Image-Staging.wvproj',
        'Projects/Linker/Windvale-Compiler-Image-Canonical-Transport.wvproj'
    )) {
        Add-Suite @(
            'segmented-compiler-toolset-reconstruction',
            'wv-linker-reconstruction'
        )
        if ($Path -eq 'Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj') {
            Add-Suite 'native-sha256-lowering'
        }
    } elseif ($Path -eq
        'Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj') {
        Add-Suite 'native-x64-lowering-development'
    } elseif ($Path -eq 'Projects/Compiler/Windvale-Native-X64-Lowering.wvproj') {
        Add-Suite @(
            'wvb-to-wvo-reconstruction',
            'wv-linker-reconstruction',
            'wvo-inspector-reconstruction',
            'console-verifier-reconstruction',
            'console-publisher-reconstruction',
            'wvo-publisher-reconstruction'
        )
    } elseif ($Path -eq 'Projects/Tests/Windvale-Native-Test-Model-Protocol.wvproj') {
        Add-Suite 'libraries'
    } elseif ($Path -eq 'Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj') {
        Add-Suite 'wvb-to-wvo-reconstruction'
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Wvb-Fixed-Integer-Runtime.wvproj',
        'Projects/Tests/Windvale-Native-Test-Wvb-Rune-Runtime.wvproj'
    )) {
        Add-Suite 'language-1-front-door'
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-X64-Lowering-Metadata.wvproj'
    )) {
        Add-Suite 'wvb-to-wvo-reconstruction'
    } elseif ($Path -eq 'Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-U64.wvproj') {
        Add-Suite 'native-u64-lowering'
    } elseif ($Path -in @(
        'Projects/Tests/Windvale-Native-Test-Nested-Record-Fields.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Storage-Publication.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Storage-Recovery.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Single-Writer-Commit.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Tree-Node.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Logical-Record.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Collection-Catalog.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Bootstrap.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Single-Leaf-Upsert.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Branch-Split.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Root-Split.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Depth-Two-Upsert.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Root-Growth.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Depth-Three-Upsert.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Upsert.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Tree-Path-Delete.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Host-Storage.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Engine.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Reader.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Delete.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Scan.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Fill.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Host-Root-Split-Writer.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Host-Tree-Writer.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Writer.wvproj',
        'Projects/Tests/Windvale-Native-Test-Database-Host-Logical-Tree-Get.wvproj',
        'Projects/Tests/Windvale-Native-Test-Capability-Provider-Table.wvproj',
        'Projects/Tests/Windvale-Native-Test-X64-Provider-Call.wvproj',
        'Projects/Tests/Windvale-Native-Test-Execution-Context-9.wvproj'
    )) {
        if ($DatabaseDevelopmentPaths.Contains($Path)) {
            Add-Suite 'database-storage'
        } else {
            Require-Full-Database-Storage
        }
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
            'Examples/Foundation/Wv-Dump-Core.wv',
            'Projects/Examples/Windvale-Wvb-Inspector.wvproj'
        )) {
            Add-Suite 'wvb-inspector-reconstruction'
        }
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
    } else {
        Add-Gap "unmapped:$Path"
    }
}

$OrderedSuites = @($SuiteEntries.Name | Where-Object { $SelectedSuites.Contains($_) })
$SelectedSuiteEntries = @(
    $SuiteEntries | Where-Object { $SelectedSuites.Contains($_.Name) })
$SelectedExpectedSeconds = if ($SelectedSuiteEntries.Count -eq 0) {
    [long]0
} else {
    [long](($SelectedSuiteEntries |
        Measure-Object -Property ExpectedSeconds -Sum).Sum)
}
$SelectedMaximumSeconds = if ($SelectedSuiteEntries.Count -eq 0) {
    [long]0
} else {
    [long](($SelectedSuiteEntries |
        Measure-Object -Property MaximumSeconds -Sum).Sum)
}
$OrderedGaps = @($Gaps | Sort-Object)
$DatabaseDevelopmentTarget = 'all'
if (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-commit')) {
    $DatabaseDevelopmentTarget = 'transaction-commit'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 3 -and
    $SelectedDatabaseDevelopmentTargets.Contains('typed-row') -and
    $SelectedDatabaseDevelopmentTargets.Contains('query-ir') -and
    $SelectedDatabaseDevelopmentTargets.Contains('sql-lowerer')) {
    $DatabaseDevelopmentTarget = 'typed-query-sql'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 11 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-mutations') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-rewrite') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-paths') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-partition') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-parent-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-branch-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-ancestor-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-ancestor-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 8 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-paths') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-parent-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-branch-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-ancestor-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-ancestor-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction-paths'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 6 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-partition') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-parent-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-branch-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction-leaf-partition'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 5 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-parent-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-branch-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction-leaf-groups'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 4 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-leaf-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-parent-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-branch-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction-leaf-pages'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 7 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-branch-partition') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-parent-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-branch-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-ancestor-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-ancestor-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-root-growth') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction-branch-partition'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 3 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-ancestor-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-ancestor-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction-ancestor-groups'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 3 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-parent-groups') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-branch-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction-parent-groups'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 2 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-branch-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction-branch-pages'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 2 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-ancestor-pages') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction-ancestor-pages'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 2 -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-root-growth') -and
    $SelectedDatabaseDevelopmentTargets.Contains('transaction-tree-completion')) {
    $DatabaseDevelopmentTarget = 'transaction-root-growth'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 2 -and
    $SelectedDatabaseDevelopmentTargets.Contains('query-ir') -and
    $SelectedDatabaseDevelopmentTargets.Contains('sql-lowerer')) {
    $DatabaseDevelopmentTarget = 'query-sql'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 2 -and
    $SelectedDatabaseDevelopmentTargets.Contains('typed-row') -and
    $SelectedDatabaseDevelopmentTargets.Contains('query-ir')) {
    $DatabaseDevelopmentTarget = 'typed-query'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 2 -and
    $SelectedDatabaseDevelopmentTargets.Contains('json-value') -and
    $SelectedDatabaseDevelopmentTargets.Contains('json-protocol')) {
    $DatabaseDevelopmentTarget = 'json'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 2 -and
    $SelectedDatabaseDevelopmentTargets.Contains('tree-node') -and
    $SelectedDatabaseDevelopmentTargets.Contains('host-tree-scan')) {
    $DatabaseDevelopmentTarget = 'tree-scan'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 2 -and
    $SelectedDatabaseDevelopmentTargets.Contains('tree-path-delete') -and
    $SelectedDatabaseDevelopmentTargets.Contains('host-tree-delete')) {
    $DatabaseDevelopmentTarget = 'host-tree-delete'
} elseif (!$DatabaseDevelopmentRequiresAllTargets -and
    $SelectedDatabaseDevelopmentTargets.Count -eq 1) {
    $DatabaseDevelopmentTarget = @(
        $DatabaseDevelopmentTargetProjects.Keys |
            Where-Object { $SelectedDatabaseDevelopmentTargets.Contains($_) }
    )[0]
}
$OsX64CodeEmissionDevelopmentTarget = 'all'
if (!$OsX64CodeEmissionDevelopmentRequiresAllTargets -and
    $SelectedOsX64CodeEmissionDevelopmentTargets.Count -eq 1) {
    $OsX64CodeEmissionDevelopmentTarget = @(
        $SelectedOsX64CodeEmissionDevelopmentTargets
    )[0]
}
$LibraryDevelopmentTarget = 'all'
if (!$LibraryDevelopmentRequiresAllTargets -and
    $SelectedLibraryDevelopmentTargets.Count -eq 1) {
    $LibraryDevelopmentTarget = @($SelectedLibraryDevelopmentTargets)[0]
}
$SourceContainmentDevelopmentMode = if (
    $SelectedSuites.Contains('source-containment') -and
    $SourceContainmentCompilerDevelopmentEligible) {
    'compiler-only'
} else {
    'complete'
}
if (!$Quiet) {
    Write-Host "Native owners: [$($OrderedSuites -join ', ')]"
    Write-Host "Native owner expected seconds: $SelectedExpectedSeconds"
    Write-Host "Native owner maximum seconds: $SelectedMaximumSeconds"
    Write-Host "Native coverage gaps: [$($OrderedGaps -join ', ')]"
    Write-Host "Plan verification: $($RunPlanVerification.ToString().ToLowerInvariant())"
    Write-Host "WebAssembly engine verification: $($RunWebAssemblyEngineVerification.ToString().ToLowerInvariant())"
    Write-Host "WebAssembly verification: $($RunWebAssemblyVerification.ToString().ToLowerInvariant())"
    Write-Host "GitHub qualification verification: $($RunGitHubQualificationVerification.ToString().ToLowerInvariant())"
    Write-Host "Source containment development mode: $SourceContainmentDevelopmentMode"
    Write-Host "OS x64 code-emission development target: $OsX64CodeEmissionDevelopmentTarget"
    Write-Host "Library development target: $LibraryDevelopmentTarget"
    Write-Host "Database storage development checkpoint: $((
        $SelectedSuites.Contains('database-storage') -and
        $DatabaseStorageDevelopmentEligible).ToString().ToLowerInvariant())"
    Write-Host "Database storage development target: $DatabaseDevelopmentTarget"
}
if ($PassThru) {
    [pscustomobject]@{
        Suites = $OrderedSuites
        ExpectedSeconds = $SelectedExpectedSeconds
        MaximumSeconds = $SelectedMaximumSeconds
        Gaps = $OrderedGaps
        RunPlanVerification = $RunPlanVerification
        RunWebAssemblyEngineVerification = $RunWebAssemblyEngineVerification
        RunWebAssemblyVerification = $RunWebAssemblyVerification
        RunGitHubQualificationVerification = $RunGitHubQualificationVerification
        UseSourceContainmentCompilerDevelopment = (
            $SelectedSuites.Contains('source-containment') -and
            $SourceContainmentCompilerDevelopmentEligible)
        UseOsX64CodeEmissionDevelopment = (
            $SelectedSuites.Contains('os-x64-code-emission') -and
            $OsX64CodeEmissionDevelopmentEligible)
        OsX64CodeEmissionDevelopmentTarget = $OsX64CodeEmissionDevelopmentTarget
        UseLibraryDevelopment = (
            $SelectedSuites.Contains('libraries') -and
            $LibraryDevelopmentEligible -and
            $LibraryDevelopmentTarget -ne 'all')
        LibraryDevelopmentTarget = $LibraryDevelopmentTarget
        UseDatabaseStorageDevelopment = (
            $SelectedSuites.Contains('database-storage') -and
            $DatabaseStorageDevelopmentEligible)
        DatabaseStorageDevelopmentTarget = $DatabaseDevelopmentTarget
        ChangedCount = $Paths.Count
    }
}
