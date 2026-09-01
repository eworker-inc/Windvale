[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$Classifier = Join-Path $PSScriptRoot 'Classify-Verification-Changes.ps1'
$Cases = @(
    @{
        Name = 'documentation only'
        Paths = @('README.md', 'Documents/Project/Roadmap.md')
        Scope = 'lightweight'
        Editor = $false
    },
    @{
        Name = 'documentation with editorial image'
        Paths = @('README.md', 'Documents/Project/Images/Windvale-Project-Progress-2026-08-04.png')
        Scope = 'lightweight'
        Editor = $false
    },
    @{
        Name = 'editor only'
        Paths = @('Tools/Editors/Windvale/package.json')
        Scope = 'lightweight'
        Editor = $true
    },
    @{
        Name = 'classification rule verification only'
        Paths = @(
            'Documents/Decisions/Legacy-Id-Collisions.txt',
            'Tools/Verify/Classify-Verification-Changes.ps1',
            'Tools/Verify/Verify-Changed.ps1',
            'Tools/Verify/Verify-Change-Classification.ps1',
            'Tools/Verify/Verify-Documentation.ps1',
            'Tools/Verify/Verify-Verification-Plan.ps1'
        )
        Scope = 'lightweight'
        Editor = $false
    },
    @{
        Name = 'website static content'
        Paths = @('Website/index.html', 'Website/styles.css')
        Scope = 'website'
        Editor = $false
    },
    @{
        Name = 'browser application and shared web library'
        Paths = @(
            'Applications/Web/Wvdb-Workbench/Source/Main.ts',
            'Libraries/Web/Framework/State/State-Owner.ts'
        )
        Scope = 'website'
        Editor = $false
    },
    @{
        Name = 'website tooling and editor metadata'
        Paths = @('Tools/Windvale.Playground/Editor/Vite-Config.mjs', 'Tools/Editors/Windvale/package.json')
        Scope = 'website'
        Editor = $true
    },
    @{
        Name = 'website and documentation'
        Paths = @('Website/site.js', 'README.md')
        Scope = 'website'
        Editor = $false
    },
    @{
        Name = 'website deployment and function'
        Paths = @(
            '.github/workflows/deploy-homepage.yml',
            '.github/workflows/deploy-maintenance.yml',
            'Website/functions/api/supporters.js'
        )
        Scope = 'website'
        Editor = $false
    },
    @{
        Name = 'browser playground contract and website verifier'
        Paths = @('Specifications/Browser-Playground.md', 'Tools/Verify/Verify-Website.ps1')
        Scope = 'website'
        Editor = $false
    },
    @{
        Name = 'source specification'
        Paths = @('Specifications/Seed-Language.md')
        Scope = 'development'
        Editor = $true
    },
    @{
        Name = 'runtime implementation'
        Paths = @('Runtime/Windvale/Native-Execution-Context-Core.wv')
        Scope = 'development'
        Editor = $false
    },
    @{
        Name = 'native compiler implementation'
        Paths = @('Compiler/Windvale/Native-X64-Lowering-Core.wv')
        Scope = 'development'
        Editor = $true
    },
    @{
        Name = 'mixed documentation and compiler implementation'
        Paths = @(
            'README.md',
            'Documents/Project/Images/Windvale-Project-Progress-2026-08-04.png',
            'Compiler/Windvale/Source-Lexer-Core.wv'
        )
        Scope = 'development'
        Editor = $true
    },
    @{
        Name = 'mixed website and compiler implementation'
        Paths = @('Website/site.js', 'Compiler/Windvale/Source-Lexer-Core.wv')
        Scope = 'development'
        Editor = $true
    },
    @{
        Name = 'unrecognized configuration'
        Paths = @('.github/dependabot.yml')
        Scope = 'development'
        Editor = $false
    },
    @{
        Name = 'retirement inventory'
        Paths = @('Documents/Project/Dotnet-Retirement-Inventory.json')
        Scope = 'development'
        Editor = $false
    }
)

foreach ($Case in $Cases) {
    $Result = & $Classifier -ChangedPath $Case.Paths -PassThru -Quiet
    if (
        $Result.Scope -ne $Case.Scope -or
        $Result.Editor -ne $Case.Editor
    ) {
        throw (
            "Classification '$($Case.Name)' expected scope=$($Case.Scope), " +
            "editor=$($Case.Editor); found scope=$($Result.Scope), editor=$($Result.Editor)."
        )
    }
}

$Empty = & $Classifier -ChangedPath @() -PassThru -Quiet
if ($Empty.Scope -ne 'qualification' -or !$Empty.Editor) {
    throw 'An empty changed-path set did not select qualification and editor verification.'
}

$Unresolved = & $Classifier `
    -BaseReference '__windvale_missing_base_reference__' `
    -HeadReference 'HEAD' `
    -PassThru `
    -Quiet
if ($Unresolved.Scope -ne 'qualification' -or !$Unresolved.Editor) {
    throw 'An unresolved comparison did not fail closed to qualification and editor verification.'
}

$Forced = & $Classifier -ForceQualification -PassThru -Quiet
if ($Forced.Scope -ne 'qualification' -or !$Forced.Editor) {
    throw 'A forced run did not select qualification and editor verification.'
}

Write-Host "Verification change classification passed ($($Cases.Count + 3) cases)."
