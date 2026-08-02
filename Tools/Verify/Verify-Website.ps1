[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$PlaygroundRoot = Join-Path $RepositoryRoot 'Tools/Windvale.Playground'

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

Invoke-External $RepositoryRoot 'npm' @('ci')
Invoke-External $PlaygroundRoot 'npm' @('ci')
Invoke-External $PlaygroundRoot 'npm' @('run', 'build')
Invoke-External $RepositoryRoot 'npm' @('run', 'verify:wasm-demo')

$WebsiteScripts = Get-ChildItem -LiteralPath (Join-Path $RepositoryRoot 'Tools/Website') -Filter '*.mjs' -File
foreach ($WebsiteScript in $WebsiteScripts) {
    Invoke-External $RepositoryRoot 'node' @('--check', $WebsiteScript.FullName)
}

$BrowserScripts = @(
    Get-ChildItem -LiteralPath (Join-Path $RepositoryRoot 'Website') -Filter '*.js' -File -Recurse
    Get-ChildItem -LiteralPath (Join-Path $PlaygroundRoot 'wwwroot') -Filter '*.js' -File -Recurse |
        Where-Object { !$_.FullName.StartsWith((Join-Path $PlaygroundRoot 'wwwroot/editor'), [StringComparison]::OrdinalIgnoreCase) }
)
foreach ($BrowserScript in $BrowserScripts) {
    Invoke-External $RepositoryRoot 'node' @('--check', $BrowserScript.FullName)
}

Invoke-External $RepositoryRoot 'node' @('Tools/Website/Verify-Supporters.mjs')
Invoke-External $RepositoryRoot 'npm' @('exec', '--', 'vite', 'build', '--config', 'Vite-Config.mjs')

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
