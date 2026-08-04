[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ExtensionRoot = Join-Path $PSScriptRoot 'Windvale'
$PackagePath = Join-Path $ExtensionRoot 'package.json'
$ConfigurationPath = Join-Path $ExtensionRoot 'Language-Configuration.json'
$GrammarPath = Join-Path $ExtensionRoot 'syntaxes/Windvale.tmLanguage.json'

function Assert-Condition {
    param(
        [Parameter(Mandatory)]
        [bool]$Condition,
        [Parameter(Mandatory)]
        [string]$Message
    )

    if (!$Condition) {
        throw $Message
    }
}

function Read-Json {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    Assert-Condition (Test-Path -LiteralPath $Path -PathType Leaf) "Required editor file is missing: $Path"
    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -Depth 100
}

function Get-RulePattern {
    param(
        [Parameter(Mandatory)]
        [object]$Grammar,
        [Parameter(Mandatory)]
        [string]$RuleName,
        [int]$PatternIndex = 0
    )

    $RuleProperty = $Grammar.repository.PSObject.Properties[$RuleName]
    Assert-Condition ($null -ne $RuleProperty) "Grammar repository rule '$RuleName' is missing."
    $Patterns = @($RuleProperty.Value.patterns)
    Assert-Condition ($PatternIndex -lt $Patterns.Count) "Grammar repository rule '$RuleName' has no pattern $PatternIndex."
    $Pattern = $Patterns[$PatternIndex].match
    Assert-Condition (![string]::IsNullOrWhiteSpace($Pattern)) "Grammar repository rule '$RuleName' has no match expression."
    return [regex]::new($Pattern, [System.Text.RegularExpressions.RegexOptions]::CultureInvariant)
}

function Assert-FullMatch {
    param(
        [Parameter(Mandatory)]
        [regex]$Pattern,
        [Parameter(Mandatory)]
        [string]$Value,
        [Parameter(Mandatory)]
        [string]$Description
    )

    $Match = $Pattern.Match($Value)
    Assert-Condition ($Match.Success -and $Match.Index -eq 0 -and $Match.Length -eq $Value.Length) `
        "$Description '$Value' is not matched exactly by the grammar."
}

$Package = Read-Json $PackagePath
$Configuration = Read-Json $ConfigurationPath
$Grammar = Read-Json $GrammarPath

Assert-Condition ($Package.name -eq 'windvale-language') 'The extension package name must remain windvale-language.'
Assert-Condition ($Package.license -eq 'SEE LICENSE IN LICENSE') 'The extension package must name its bundled custom license.'
$Licenseˉpath = Join-Path $ExtensionRoot 'LICENSE'
Assert-Condition (Test-Path -LiteralPath $Licenseˉpath -PathType Leaf) 'The extension package must include its license.'
$Licenseˉtext = Get-Content -LiteralPath $Licenseˉpath -Raw
Assert-Condition ($Licenseˉtext.StartsWith('# Windvale Community Source License 1.0')) 'The extension license must match the repository license family.'
Assert-Condition (@($Package.contributes.languages).Count -eq 1) 'The extension must contribute exactly one source language.'
Assert-Condition (@($Package.contributes.grammars).Count -eq 1) 'The extension must contribute exactly one source grammar.'

$Language = @($Package.contributes.languages)[0]
$GrammarContribution = @($Package.contributes.grammars)[0]
Assert-Condition ($Language.id -eq 'windvale') 'The contributed language id must be windvale.'
Assert-Condition (@($Language.extensions) -contains '.wv') 'The contributed language must own the .wv extension.'
Assert-Condition (!(@($Language.extensions) -contains '.wva')) 'WVA must remain a separate textual-assembly language.'
Assert-Condition ($GrammarContribution.language -eq 'windvale') 'The grammar contribution must target the windvale language id.'
Assert-Condition ($GrammarContribution.scopeName -eq 'source.windvale') 'The contributed TextMate scope must be source.windvale.'
Assert-Condition ($Grammar.scopeName -eq 'source.windvale') 'The grammar root scope must be source.windvale.'
Assert-Condition (@($Grammar.fileTypes) -contains 'wv') 'The TextMate grammar must declare the wv file type.'
Assert-Condition ($Configuration.comments.lineComment -eq '//') 'Windvale line comments must use //.'

$ConfigurationReference = Join-Path $ExtensionRoot ($Language.configuration -replace '^\./', '')
$GrammarReference = Join-Path $ExtensionRoot ($GrammarContribution.path -replace '^\./', '')
Assert-Condition ((Resolve-Path -LiteralPath $ConfigurationReference).Path -eq (Resolve-Path -LiteralPath $ConfigurationPath).Path) `
    'The package language configuration path does not resolve to the maintained configuration.'
Assert-Condition ((Resolve-Path -LiteralPath $GrammarReference).Path -eq (Resolve-Path -LiteralPath $GrammarPath).Path) `
    'The package grammar path does not resolve to the maintained grammar.'

$RequiredIncludes = @(
    '#comments',
    '#strings',
    '#declarations',
    '#declaration-keywords',
    '#control-keywords',
    '#storage-keywords',
    '#profile-keywords',
    '#type-keywords',
    '#boolean-literals',
    '#built-in-functions',
    '#numbers',
    '#named-record-literals',
    '#calls',
    '#members',
    '#operators',
    '#punctuation'
)
$ActualIncludes = @($Grammar.patterns | ForEach-Object { $_.include })
foreach ($Include in $RequiredIncludes) {
    Assert-Condition ($ActualIncludes -contains $Include) "The root grammar does not include required rule '$Include'."
}

$KeywordCases = [ordered]@{
    'declaration-keywords' = @('module', 'profile', 'import', 'as', 'capability', 'data', 'const', 'record', 'enum', 'variant', 'export', 'fn')
    'control-keywords' = @('if', 'else', 'while', 'for', 'in', 'match', 'case', 'push', 'break', 'continue', 'return')
    'storage-keywords' = @('let', 'var', 'freeze')
    'profile-keywords' = @('portable', 'hosted', 'system')
    'type-keywords' = @('i32', 'i64', 'u8', 'u32', 'u64', 'bool', 'text', 'bytes', 'sequence', 'builder', 'void')
    'boolean-literals' = @('true', 'false')
    'built-in-functions' = @('length')
}
foreach ($Rule in $KeywordCases.GetEnumerator()) {
    $Pattern = Get-RulePattern $Grammar $Rule.Key
    foreach ($Keyword in $Rule.Value) {
        Assert-FullMatch $Pattern $Keyword "Reserved word in rule '$($Rule.Key)'"
    }
}

$ReservedPatterns = @($KeywordCases.Keys | ForEach-Object { Get-RulePattern $Grammar $_ })
foreach ($Identifier in @('moduleˉname', 'trueˉvalue', 'i32ˉvalue', 'lengthening')) {
    foreach ($Pattern in $ReservedPatterns) {
        Assert-Condition (!$Pattern.IsMatch($Identifier)) "Identifier '$Identifier' is incorrectly matched as a reserved word."
    }
}

$ConstantDeclarationPattern = Get-RulePattern $Grammar 'declarations' 4
$ConstantDeclarationSource = 'const MAXIMUM_RECORDS'
$ConstantDeclaration = $ConstantDeclarationPattern.Match($ConstantDeclarationSource)
Assert-Condition ($ConstantDeclaration.Success -and $ConstantDeclaration.Length -eq $ConstantDeclarationSource.Length) `
    'The editor grammar must recognize a complete typed-constant declaration prefix.'
Assert-Condition ($ConstantDeclaration.Groups[2].Value -eq 'MAXIMUM_RECORDS') `
    'The editor grammar must scope the ALL_CAPS constant name separately.'

$NumberPattern = Get-RulePattern $Grammar 'numbers'
foreach ($Number in @(
    '0',
    '2147483647',
    '0i64',
    '9223372036854775807i64',
    '0u8',
    '255u8',
    '0u32',
    '4294967295u32',
    '0u64',
    '18446744073709551615u64')) {
    Assert-FullMatch $NumberPattern $Number 'Numeric token'
}
foreach ($Identifier in @('Value0', '0u32suffix', '0u64suffix', 'Fieldˉ0u8')) {
    Assert-Condition (!$NumberPattern.IsMatch($Identifier)) "Identifier '$Identifier' is incorrectly matched as a numeric token."
}

$NamedRecordPattern = Get-RulePattern $Grammar 'named-record-literals'
$NamedRecordSource = 'Readˉrequest { Name:'
$NamedRecord = $NamedRecordPattern.Match($NamedRecordSource)
Assert-Condition ($NamedRecord.Success -and $NamedRecord.Value -eq 'Readˉrequest') `
    'The editor grammar must recognize a named-record literal type before its field block.'
Assert-Condition (!$NamedRecordPattern.IsMatch('Ready { Process(Value); }')) `
    'The named-record grammar must not classify an ordinary condition before a block as a type.'

$ControlPattern = Get-RulePattern $Grammar 'control-keywords'
Assert-Condition ($ControlPattern.Matches('else if').Count -eq 2) `
    'The editor grammar must recognize both control words in block-form else if.'

$CommentPattern = Get-RulePattern $Grammar 'comments'
Assert-FullMatch $CommentPattern '// Windvale comment' 'Line comment'
$OperatorPattern = Get-RulePattern $Grammar 'operators'
foreach ($Operator in @('->', '&&', '||', '<<', '>>', '==', '!=', '<=', '>=', '+=', '-=', '*=', '+', '-', '*', '/', '%', '&', '|', '^', '~', '!', '<', '>', '=')) {
    Assert-FullMatch $OperatorPattern $Operator 'Operator'
}

$SamplePath = Join-Path $RepositoryRoot 'Examples/Seed/Hello-Windvale.wv'
$Sample = Get-Content -LiteralPath $SamplePath -Raw
foreach ($Fragment in @('module Helloˉwindvale profile hosted;', 'capability console.write_line;', 'export fn Main() -> i32', 'return 0;')) {
    Assert-Condition ($Sample.Contains($Fragment, [StringComparison]::Ordinal)) `
        "Representative Windvale source no longer contains expected grammar fixture '$Fragment'."
}

Write-Host 'Windvale editor support contract passed.'
