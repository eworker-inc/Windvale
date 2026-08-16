[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$CompilerApplication,
    [string]$Project = 'Projects/Examples/Windvale-Compiler.wvproj',
    [ValidateRange(2, 20)]
    [int]$Iterations = 2,
    [string]$OutputJson
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

function Resolve-OrdinaryFile([string]$Path, [string]$Label) {
    $Resolved = (Resolve-Path -LiteralPath $Path).Path
    $Item = Get-Item -LiteralPath $Resolved -Force
    if (!$Item.PSIsContainer -and !$Item.LinkType) { return $Resolved }
    throw "$Label must be an ordinary non-link file: $Path"
}

function Invoke-Compiler(
    [string]$Application,
    [string[]]$Arguments,
    [string]$WorkingDirectory
) {
    $StartInfo = [Diagnostics.ProcessStartInfo]::new()
    $StartInfo.FileName = $Application
    $StartInfo.WorkingDirectory = $WorkingDirectory
    $StartInfo.UseShellExecute = $false
    $StartInfo.CreateNoWindow = $true
    $StartInfo.RedirectStandardOutput = $true
    $StartInfo.RedirectStandardError = $true
    foreach ($Argument in $Arguments) { $null = $StartInfo.ArgumentList.Add($Argument) }
    $Process = [Diagnostics.Process]::new()
    $Process.StartInfo = $StartInfo
    $Watch = [Diagnostics.Stopwatch]::StartNew()
    if (!$Process.Start()) { throw "Could not start $Application." }
    $OutputTask = $Process.StandardOutput.ReadToEndAsync()
    $ErrorTask = $Process.StandardError.ReadToEndAsync()
    $Process.WaitForExit()
    $Watch.Stop()
    $Output = $OutputTask.GetAwaiter().GetResult().Trim()
    $ErrorOutput = $ErrorTask.GetAwaiter().GetResult().Trim()
    if ($Process.ExitCode -ne 0) {
        throw "$Application exited $($Process.ExitCode): $ErrorOutput"
    }
    return [pscustomobject]@{
        ElapsedMilliseconds = $Watch.Elapsed.TotalMilliseconds
        Output = $Output
    }
}

$CompilerApplication = Resolve-OrdinaryFile $CompilerApplication 'Compiler application'
$ProjectPath = if ([IO.Path]::IsPathRooted($Project)) {
    $Project
} else {
    Join-Path $RepositoryRoot $Project
}
$ProjectPath = Resolve-OrdinaryFile $ProjectPath 'Project manifest'
$Sources = [System.Collections.Generic.List[string]]::new()
foreach ($Line in Get-Content -LiteralPath $ProjectPath) {
    if ($Line -match '^(?:root|source) "([^"\r\n]+)"$') {
        $Sources.Add((Resolve-OrdinaryFile (Join-Path $RepositoryRoot $Matches[1]) 'Project source'))
    }
}
if ($Sources.Count -lt 1) { throw 'The project manifest has no root or source entries.' }

$BenchmarkRoot = Join-Path ([IO.Path]::GetTempPath()) (
    'windvale-source-wvb-compilation-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $BenchmarkRoot | Out-Null
$ResolvedBenchmarkRoot = (Resolve-Path -LiteralPath $BenchmarkRoot).Path
$TemporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
if (!$ResolvedBenchmarkRoot.StartsWith($TemporaryRoot, [StringComparison]::OrdinalIgnoreCase) -or
    !(Split-Path -Leaf $ResolvedBenchmarkRoot).StartsWith(
        'windvale-source-wvb-compilation-', [StringComparison]::Ordinal)) {
    throw 'The benchmark root escaped its bounded temporary location.'
}

$Samples = [System.Collections.Generic.List[object]]::new()
$ExpectedHashes = @{}
try {
    for ($Pair = 0; $Pair -lt $Iterations; $Pair += 1) {
        $Modes = if (($Pair % 2) -eq 0) { @('optimized', 'complete') } else { @('complete', 'optimized') }
        foreach ($Mode in $Modes) {
            $OutputPath = Join-Path $ResolvedBenchmarkRoot "$Mode-$Pair.wvb"
            $Arguments = [System.Collections.Generic.List[string]]::new()
            if ($Mode -eq 'complete') { $Arguments.Add('--complete') }
            foreach ($Source in $Sources) { $Arguments.Add($Source) }
            $Arguments.Add($OutputPath)
            Write-Host "source compiler benchmark mode=$Mode iteration=$($Pair + 1)/$Iterations"
            $Run = Invoke-Compiler $CompilerApplication $Arguments.ToArray() $RepositoryRoot
            $OutputPath = Resolve-OrdinaryFile $OutputPath 'Compiler output'
            $Hash = (Get-FileHash -LiteralPath $OutputPath -Algorithm SHA256).Hash.ToLowerInvariant()
            if ($ExpectedHashes.ContainsKey($Mode) -and $ExpectedHashes[$Mode] -ne $Hash) {
                throw "$Mode compilation was not byte deterministic."
            }
            $ExpectedHashes[$Mode] = $Hash
            $Samples.Add([ordered]@{
                mode = $Mode
                iteration = $Pair + 1
                elapsed_ms = [Math]::Round($Run.ElapsedMilliseconds, 3)
                output_bytes = (Get-Item -LiteralPath $OutputPath).Length
                output_sha256 = $Hash
                compiler_report = $Run.Output
            })
        }
    }
    $ModeReports = foreach ($Mode in @('optimized', 'complete')) {
        $ModeSamples = @($Samples | Where-Object mode -eq $Mode)
        [ordered]@{
            mode = $Mode
            iterations = $ModeSamples.Count
            mean_elapsed_ms = [Math]::Round(
                ($ModeSamples.elapsed_ms | Measure-Object -Average).Average, 3)
            minimum_elapsed_ms = [Math]::Round(
                ($ModeSamples.elapsed_ms | Measure-Object -Minimum).Minimum, 3)
            maximum_elapsed_ms = [Math]::Round(
                ($ModeSamples.elapsed_ms | Measure-Object -Maximum).Maximum, 3)
            output_bytes = $ModeSamples[0].output_bytes
            output_sha256 = $ModeSamples[0].output_sha256
            byte_deterministic = $true
        }
    }
    $Report = [ordered]@{
        format = 'windvale-source-wvb-compilation-benchmark-1'
        generated_utc = [DateTime]::UtcNow.ToString('O')
        compiler_application_sha256 = (Get-FileHash -LiteralPath $CompilerApplication -Algorithm SHA256).Hash.ToLowerInvariant()
        project = [IO.Path]::GetRelativePath($RepositoryRoot, $ProjectPath).Replace('\', '/')
        source_count = $Sources.Count
        modes = $ModeReports
        samples = $Samples
    }
    $Json = $Report | ConvertTo-Json -Depth 8
    if ($OutputJson) {
        $OutputPath = [IO.Path]::GetFullPath($OutputJson)
        [IO.File]::WriteAllText($OutputPath, $Json + "`n", [Text.UTF8Encoding]::new($false))
    }
    $Json
} finally {
    if (Test-Path -LiteralPath $ResolvedBenchmarkRoot) {
        Remove-Item -LiteralPath $ResolvedBenchmarkRoot -Recurse -Force
    }
}
