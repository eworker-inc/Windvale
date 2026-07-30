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
    },
    @{
        Name = 'mixed documentation and compiler implementation'
        Paths = @('README.md', 'Compiler/Windvale.Compiler/Source-Lexer.cs')
        Scope = 'qualification'
        Editor = $true
    },
    @{
        Name = 'unrecognized configuration'
        Paths = @('.github/dependabot.yml')
        Scope = 'qualification'
        Editor = $false
    }
)

foreach ($Case in $Cases) {
    $Result = & $Classifier -ChangedPath $Case.Paths -PassThru -Quiet
    if ($Result.Scope -ne $Case.Scope -or $Result.Editor -ne $Case.Editor) {
        throw (
            "Classification '$($Case.Name)' expected scope=$($Case.Scope), " +
            "editor=$($Case.Editor), found scope=$($Result.Scope), editor=$($Result.Editor)."
        )
    }
}

$Empty = & $Classifier -ChangedPath @() -PassThru -Quiet
if ($Empty.Scope -ne 'qualification' -or !$Empty.Editor) {
    throw 'An empty changed-path set did not fail closed to qualification and editor verification.'
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
