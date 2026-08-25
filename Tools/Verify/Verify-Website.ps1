[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$WebsiteRoot = Join-Path $RepositoryRoot 'Website'
$PlaygroundRoot = Join-Path $RepositoryRoot 'Tools/Windvale.Playground'
$WvdbWorkbenchRoot = Join-Path $RepositoryRoot 'Applications/Web/Wvdb-Workbench'

function Invoke-External {
    param(
        [Parameter(Mandatory)]
        [string]$WorkingDirectory,
        [Parameter(Mandatory)]
        [string]$FilePath,
        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    Push-Location $WorkingDirectory
    try {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed with exit code ${LASTEXITCODE}: $FilePath $($Arguments -join ' ')"
        }
    } finally {
        Pop-Location
    }
}

Invoke-External $WebsiteRoot 'npm' @('ci')
Invoke-External $PlaygroundRoot 'npm' @('ci')
Invoke-External $WvdbWorkbenchRoot 'npm' @('ci')
Invoke-External $PlaygroundRoot 'npm' @('run', 'build')
Invoke-External $WvdbWorkbenchRoot 'npm' @('run', 'check')
Invoke-External $WvdbWorkbenchRoot 'npm' @('run', 'build')
Invoke-External $WebsiteRoot 'npm' @('run', 'verify:wasm-demo')
Invoke-External $WebsiteRoot 'npm' @('run', 'verify:wasm-compiler-package')
Invoke-External $WebsiteRoot 'npm' @('run', 'verify:wasm-compiler-demo')
Invoke-External $WebsiteRoot 'npm' @('run', 'verify:wasm-compiler-core')
Invoke-External $WebsiteRoot 'npm' @('run', 'verify:wasm-workbench')

$WebsiteScripts = Get-ChildItem -LiteralPath (Join-Path $RepositoryRoot 'Tools/Website') -Filter '*.mjs' -File
foreach ($WebsiteScript in $WebsiteScripts) {
    Invoke-External $RepositoryRoot 'node' @('--check', $WebsiteScript.FullName)
}

$BrowserScripts = @(
    Get-ChildItem -LiteralPath $WebsiteRoot -Filter '*.js' -File -Recurse |
        Where-Object {
            !$_.FullName.StartsWith((Join-Path $WebsiteRoot 'node_modules'), [StringComparison]::OrdinalIgnoreCase) -and
            !$_.FullName.StartsWith((Join-Path $WebsiteRoot 'dist'), [StringComparison]::OrdinalIgnoreCase) -and
            !$_.FullName.StartsWith((Join-Path $WebsiteRoot 'Generated'), [StringComparison]::OrdinalIgnoreCase)
        }
    Get-ChildItem -LiteralPath (Join-Path $PlaygroundRoot 'wwwroot') -Filter '*.js' -File -Recurse |
        Where-Object { !$_.FullName.StartsWith((Join-Path $PlaygroundRoot 'wwwroot/editor'), [StringComparison]::OrdinalIgnoreCase) }
    Get-ChildItem -LiteralPath (Join-Path $WvdbWorkbenchRoot 'Public') -Filter '*.js' -File -Recurse
)
foreach ($BrowserScript in $BrowserScripts) {
    Invoke-External $RepositoryRoot 'node' @('--check', $BrowserScript.FullName)
}

Invoke-External $RepositoryRoot 'node' @('Tools/Website/Verify-Supporters.mjs')
Invoke-External $RepositoryRoot 'node' @('Tools/Website/Verify-Maintenance-Site.mjs')
Invoke-External $WebsiteRoot 'npm' @('run', 'build')
Invoke-External $RepositoryRoot 'node' @('Tools/Website/Verify-Repository-Browser.mjs')

$NoticePairs = @(
    @('node_modules/monaco-editor/LICENSE', 'wwwroot/editor/notices/monaco-editor-LICENSE.txt'),
    @('node_modules/monaco-editor/ThirdPartyNotices.txt', 'wwwroot/editor/notices/monaco-editor-ThirdPartyNotices.txt'),
    @('node_modules/dompurify/LICENSE', 'wwwroot/editor/notices/DOMPurify-LICENSE.txt'),
    @('node_modules/marked/LICENSE.md', 'wwwroot/editor/notices/marked-LICENSE.md')
)
foreach ($NoticePair in $NoticePairs) {
    $SourceHash = (Get-FileHash -LiteralPath (Join-Path $PlaygroundRoot $NoticePair[0]) -Algorithm SHA256).Hash
    $PublishedHash = (Get-FileHash -LiteralPath (Join-Path $PlaygroundRoot $NoticePair[1]) -Algorithm SHA256).Hash
    if ($SourceHash -ne $PublishedHash) {
        throw "Published third-party notice does not match its installed package: $($NoticePair[1])"
    }
}

Write-Host 'Windvale website verification passed.'
