[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$InputWvb,
    [ValidateRange(1, 7)]
    [int]$Profile = 7,
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

function Invoke-Packager(
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
    $Output = $OutputTask.GetAwaiter().GetResult()
    $ErrorOutput = $ErrorTask.GetAwaiter().GetResult()
    if ($Process.ExitCode -ne 0) {
        throw "$Application exited $($Process.ExitCode): $($ErrorOutput.Trim())"
    }
    return [pscustomobject]@{
        ElapsedMilliseconds = $Watch.Elapsed.TotalMilliseconds
        Output = ($Output + $ErrorOutput).Trim()
    }
}

$InputWvb = Resolve-OrdinaryFile $InputWvb 'Input WVB'
$BenchmarkRoot = Join-Path ([IO.Path]::GetTempPath()) (
    'windvale-segmented-compiler-package-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $BenchmarkRoot | Out-Null
$ResolvedBenchmarkRoot = (Resolve-Path -LiteralPath $BenchmarkRoot).Path
$TemporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
if (!$ResolvedBenchmarkRoot.StartsWith($TemporaryRoot, [StringComparison]::OrdinalIgnoreCase) -or
    !(Split-Path -Leaf $ResolvedBenchmarkRoot).StartsWith(
        'windvale-segmented-compiler-package-', [StringComparison]::Ordinal)) {
    throw 'The benchmark root escaped its bounded temporary location.'
}

$OriginalCacheRoot = $env:WINDVALE_NATIVE_CACHE_ROOT
try {
    $env:WINDVALE_NATIVE_CACHE_ROOT = Join-Path $ResolvedBenchmarkRoot 'Cache'
    $Extension = if ($IsWindows) { '.exe' } else { '.elf' }
    $ScriptName = if ($IsWindows) {
        'Package-Segmented-Compiler-Wvb.cmd'
    } else {
        'Package-Segmented-Compiler-Wvb.sh'
    }
    $Script = Join-Path $PSScriptRoot $ScriptName
    $Application = if ($IsWindows) { 'cmd.exe' } else { '/usr/bin/env' }
    $Results = [System.Collections.Generic.List[object]]::new()
    for ($RunIndex = 0; $RunIndex -lt 2; $RunIndex += 1) {
        $OutputPath = Join-Path $ResolvedBenchmarkRoot "Application-$RunIndex$Extension"
        $Arguments = [System.Collections.Generic.List[string]]::new()
        if ($IsWindows) {
            $Arguments.Add('/d')
            $Arguments.Add('/c')
        } else {
            $Arguments.Add('bash')
        }
        $Arguments.Add($Script)
        $Arguments.Add("$Profile")
        $Arguments.Add($InputWvb)
        $Arguments.Add($OutputPath)
        $Arguments.Add('--development-cache')
        $ExpectedStatus = if ($RunIndex -eq 0) { 'Created' } else { 'Hit' }
        Write-Host "segmented compiler package benchmark run=$($RunIndex + 1)/2 expected-cache=$ExpectedStatus"
        $Run = Invoke-Packager $Application $Arguments.ToArray() $RepositoryRoot
        if ($Run.Output -notmatch "native hosted application cache status=$ExpectedStatus(?:\s|$)") {
            throw "Expected hosted application cache status $ExpectedStatus."
        }
        $OutputPath = Resolve-OrdinaryFile $OutputPath 'Packaged application'
        $Results.Add([ordered]@{
            run = $RunIndex + 1
            cache_status = $ExpectedStatus
            elapsed_ms = [Math]::Round($Run.ElapsedMilliseconds, 3)
            output_bytes = (Get-Item -LiteralPath $OutputPath).Length
            output_sha256 = (Get-FileHash -LiteralPath $OutputPath -Algorithm SHA256).Hash.ToLowerInvariant()
        })
    }
    if ($Results[0].output_sha256 -ne $Results[1].output_sha256 -or
        $Results[0].output_bytes -ne $Results[1].output_bytes) {
        throw 'Cold and warm packaging did not produce byte-identical applications.'
    }
    $Report = [ordered]@{
        format = 'windvale-segmented-compiler-package-benchmark-1'
        generated_utc = [DateTime]::UtcNow.ToString('O')
        host = if ($IsWindows) { 'windows-x64' } else { 'linux-x64' }
        profile = $Profile
        input_bytes = (Get-Item -LiteralPath $InputWvb).Length
        input_sha256 = (Get-FileHash -LiteralPath $InputWvb -Algorithm SHA256).Hash.ToLowerInvariant()
        output_bytes = $Results[0].output_bytes
        output_sha256 = $Results[0].output_sha256
        cold_elapsed_ms = $Results[0].elapsed_ms
        warm_elapsed_ms = $Results[1].elapsed_ms
        warm_speedup = [Math]::Round($Results[0].elapsed_ms / $Results[1].elapsed_ms, 3)
        byte_identical = $true
        runs = $Results
    }
    $Json = $Report | ConvertTo-Json -Depth 6
    if ($OutputJson) {
        $OutputPath = [IO.Path]::GetFullPath($OutputJson)
        [IO.File]::WriteAllText($OutputPath, $Json + "`n", [Text.UTF8Encoding]::new($false))
    }
    $Json
} finally {
    $env:WINDVALE_NATIVE_CACHE_ROOT = $OriginalCacheRoot
    if (Test-Path -LiteralPath $ResolvedBenchmarkRoot) {
        Remove-Item -LiteralPath $ResolvedBenchmarkRoot -Recurse -Force
    }
}
