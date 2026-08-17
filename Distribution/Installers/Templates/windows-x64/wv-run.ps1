$ErrorActionPreference = 'Stop'

function Fail([string]$Message, [int]$Code = 64) {
    [Console]::Error.WriteLine($Message)
    exit $Code
}

if ($args.Count -lt 2 -or $args[0] -cne 'run') {
    Fail 'Usage: wv run <source.wv> [argument ...]'
}

$Source = $args[1]
if ([IO.Path]::GetExtension($Source) -ine '.wv') {
    Fail 'wv run: source must use the .wv extension'
}
if (!(Test-Path -LiteralPath $Source -PathType Leaf)) {
    Fail "wv run: source not found: $Source" 1
}

$Root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$TemporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
$Work = Join-Path $TemporaryRoot ("windvale-run-{0}" -f [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($Work) | Out-Null

try {
    $Module = Join-Path $Work 'Script.wvb'
    & (Join-Path $Root 'bin\wvbuild.exe') $Source $Module | Out-Null
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    & (Join-Path $Root 'bin\wvverify.exe') $Module | Out-Null
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $RunnerArguments = @('--script', $Module)
    if ($args.Count -gt 2) {
        $RunnerArguments += $args[2..($args.Count - 1)]
    }
    & (Join-Path $Root 'bin\wvrun.exe') @RunnerArguments
    exit $LASTEXITCODE
} finally {
    $ResolvedWork = [IO.Path]::GetFullPath($Work)
    $ExpectedParent = [IO.Path]::GetDirectoryName($ResolvedWork)
    $ExpectedName = [IO.Path]::GetFileName($ResolvedWork)
    if (
        $ExpectedParent -eq $TemporaryRoot -and
        $ExpectedName -match '^windvale-run-[0-9a-f]{32}$' -and
        (Test-Path -LiteralPath $ResolvedWork -PathType Container)
    ) {
        Remove-Item -LiteralPath $ResolvedWork -Recurse -Force
    }
}
