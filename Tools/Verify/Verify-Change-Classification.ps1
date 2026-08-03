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
        Name = 'editor only'
        Paths = @('Tools/Editors/Windvale/package.json')
        Scope = 'lightweight'
        Editor = $true
    },
    @{
        Name = 'website static content'
        Paths = @('Website/index.html', 'Website/styles.css')
        Scope = 'website'
        Editor = $false
        Homepage = $true
    },
    @{
        Name = 'website tooling and editor metadata'
        Paths = @('Tools/Windvale.Playground/Editor/Vite-Config.mjs', 'Tools/Editors/Windvale/package.json')
        Scope = 'website'
        Editor = $true
        Homepage = $true
    },
    @{
        Name = 'website and documentation'
        Paths = @('Website/site.js', 'README.md')
        Scope = 'website'
        Editor = $false
        Homepage = $true
    },
    @{
        Name = 'website deployment and function'
        Paths = @('.github/workflows/deploy-homepage.yml', 'functions/api/supporters.js')
        Scope = 'website'
        Editor = $false
        Homepage = $true
    },
    @{
        Name = 'source specification'
        Paths = @('Specifications/Seed-Language.md')
        Scope = 'qualification'
        Editor = $true
    },
    @{
        Name = 'runtime implementation'
        Paths = @('Runtime/Windvale.Runtime/Reference-Runtime.cs')
        Scope = 'qualification'
        Editor = $false
        Homepage = $true
    },
    @{
        Name = 'playground host implementation'
        Paths = @('Tools/Windvale.Playground/Program.cs')
        Scope = 'qualification'
        Editor = $false
        Homepage = $true
    },
    @{
        Name = 'mixed documentation and compiler implementation'
        Paths = @('README.md', 'Compiler/Reference/Source-Lexer.cs')
        Scope = 'qualification'
        Editor = $true
        Homepage = $true
    },
    @{
        Name = 'mixed website and compiler implementation'
        Paths = @('Website/site.js', 'Compiler/Reference/Source-Lexer.cs')
        Scope = 'qualification'
        Editor = $true
        Homepage = $true
    },
    @{
        Name = 'unrecognized configuration'
        Paths = @('.github/dependabot.yml')
        Scope = 'qualification'
        Editor = $false
    },
    @{
        Name = 'homepage build configuration'
        Paths = @('Directory.Build.props')
        Scope = 'qualification'
        Editor = $false
        Homepage = $true
    }
)

foreach ($Case in $Cases) {
    $Result = & $Classifier -ChangedPath $Case.Paths -PassThru -Quiet
    $ExpectedHomepage = $Case.Homepage -eq $true
    if (
        $Result.Scope -ne $Case.Scope -or
        $Result.Editor -ne $Case.Editor -or
        $Result.Homepage -ne $ExpectedHomepage
    ) {
        throw (
            "Classification '$($Case.Name)' expected scope=$($Case.Scope), " +
            "editor=$($Case.Editor), homepage=$ExpectedHomepage; found scope=$($Result.Scope), " +
            "editor=$($Result.Editor), homepage=$($Result.Homepage)."
        )
    }
}

$Empty = & $Classifier -ChangedPath @() -PassThru -Quiet
if ($Empty.Scope -ne 'qualification' -or !$Empty.Editor -or $Empty.Homepage) {
    throw 'An empty changed-path set did not select qualification and editor verification without deployment.'
}

$Unresolved = & $Classifier `
    -BaseReference '__windvale_missing_base_reference__' `
    -HeadReference 'HEAD' `
    -PassThru `
    -Quiet
if ($Unresolved.Scope -ne 'qualification' -or !$Unresolved.Editor -or !$Unresolved.Homepage) {
    throw 'An unresolved comparison did not fail closed to qualification, editor verification, and deployment.'
}

$Forced = & $Classifier -ForceQualification -PassThru -Quiet
if ($Forced.Scope -ne 'qualification' -or !$Forced.Editor -or !$Forced.Homepage) {
    throw 'A forced run did not select qualification, editor verification, and deployment.'
}

Write-Host "Verification change classification passed ($($Cases.Count + 3) cases)."
