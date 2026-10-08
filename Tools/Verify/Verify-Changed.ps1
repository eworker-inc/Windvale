[CmdletBinding()]
param(
    [string]$BaseReference,
    [string]$HeadReference = 'HEAD',
    [AllowEmptyCollection()]
    [string[]]$ChangedPath,
    [switch]$PlanOnly,
    [switch]$PreparationOnly,
    [switch]$UsePreparedProducts,
    [ValidateRange(0, 16)]
    [int]$NativeDevelopmentShard = 0,
    [string]$NativeSharedCompilerHostRecord,
    [string]$NativeSharedCompilerHostRecordSha256,
    [string]$NativeSharedCompilerHostSelectionPath,
    [ValidateRange(30, 5400)]
    [int]$PreparationMaximumSeconds = 4500,
    [switch]$AllowLongRun,
    [switch]$NoFailFast,
    [string]$TimingReportPath,
    [switch]$NoResultCache,
    [string]$ResultCacheRoot,
    [switch]$SkipDocumentationVerification,
    [switch]$AllowIncompleteInfrastructure,
    [switch]$PlanVerificationInClassification,
    [switch]$GitHubVerificationOnLinux
)

$ErrorActionPreference = 'Stop'
$LOCAL_DEVELOPMENT_BUDGET_SECONDS = 600
$VerificationStartedUtc = [DateTime]::UtcNow
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$Planner = Join-Path $PSScriptRoot 'Get-Verification-Plan.ps1'
$NativePlanner = Join-Path $PSScriptRoot 'Get-Native-Changed-Verification-Plan.ps1'
$ConstructionReadinessRunner = @'
import {pathToFileURL} from 'node:url';
const Deadline = Date.now() + 115000;
const [Core, Owner] = process.argv.slice(1);
try {
    const {Runˉdevelopmentˉcommand: Run} = await import(pathToFileURL(Core).href);
    const Result = await Run(process.execPath, [Owner, '--construction-readiness'],
        Deadline, false, 1048576);
    if (Result.Output) process.stdout.write(Result.Output);
    if (Result.Error) process.stderr.write(Result.Error);
    if (!Number.isInteger(Result.Code) || Result.Code < 0 || Result.Code > 255) {
        throw new Error('Invalid readiness process result.');
    }
    process.exitCode = Result.Code;
} catch (Error) {
    process.stderr.write(String(Error?.message ?? Error).slice(0, 4096) + '\n');
    process.exitCode = Error?.cleanupUncertain === true ? 2 : Error?.exitCode === 124 ? 124 : 2;
}
'@
$PlanVerifier = Join-Path $PSScriptRoot 'Verify-Verification-Plan.ps1'
$WebAssemblyEngineVerifier = Join-Path $PSScriptRoot 'Verify-WebAssembly-Engine.ps1'
$WebAssemblyVerifier = Join-Path $PSScriptRoot 'Verify-WebAssembly.ps1'
$GitHubQualificationVerifier = Join-Path $PSScriptRoot 'Verify-GitHub-Native-Qualification.ps1'
$WebsiteVerifier = Join-Path $PSScriptRoot 'Verify-Website.ps1'
$DocumentationVerifier = Join-Path $PSScriptRoot 'Verify-Documentation.ps1'
$ChangeClassificationVerifier = Join-Path $PSScriptRoot 'Verify-Change-Classification.ps1'
$EditorVerifier = Join-Path (Split-Path -Parent $PSScriptRoot) 'Editors/Verify-Windvale-Editor.ps1'
$ResultCacheTool = Join-Path (
    Split-Path -Parent $PSScriptRoot) 'Native/Verification-Owner-Result-Cache.mjs'
$VerificationOwnerRegistry = Join-Path $RepositoryRoot `
    'Tests/Native/Verification-Owners.txt'
$CompatibleResultCacheBarrierPaths = @(
    'Tests/Native/Verification-Owners.txt',
    'Tests/Native/Verification-Duration-Profiles.txt',
    'Tools/Native/Stream-Verification-Owner.mjs',
    'Tools/Native/Verification-Owner-Result-Cache.mjs',
    'Tools/Native/Verification-Owner-Stream-Path.mjs',
    'Tools/Verify/Get-Native-Changed-Verification-Plan.ps1',
    'Tools/Verify/Get-Native-Development-Shards.ps1',
    'Tools/Verify/Get-Verification-Plan.ps1',
    'Tools/Verify/Invoke-WindvaleTests.ps1',
    'Tools/Verify/Verify-Changed.ps1'
)

function Get-VerificationHostName {
    if ($env:RUNNER_OS -in @('Windows', 'Linux', 'macOS')) {
        return $env:RUNNER_OS
    }
    if ([Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
            [Runtime.InteropServices.OSPlatform]::Windows)) {
        return 'Windows'
    }
    if ([Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
            [Runtime.InteropServices.OSPlatform]::Linux)) {
        return 'Linux'
    }
    if ([Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
            [Runtime.InteropServices.OSPlatform]::OSX)) {
        return 'macOS'
    }
    return [Environment]::OSVersion.Platform.ToString()
}

function Invoke-VerificationResultCache {
    param(
        [Parameter(Mandatory)]
        [string[]]$CacheArgument
    )

    $Output = @(& node $ResultCacheTool @CacheArgument 2>&1)
    $ExitCode = $LASTEXITCODE
    $Text = ($Output | ForEach-Object { $_.ToString() }) -join "`n"
    if ($ExitCode -ne 0) {
        throw "Verification result cache command failed: $Text"
    }
    return $Text.Trim()
}

function Get-Sha256Text {
    param(
        [Parameter(Mandatory)]
        [string]$Value
    )

    $Bytes = [Text.UTF8Encoding]::new($false).GetBytes($Value)
    return [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData($Bytes)
    ).ToLowerInvariant()
}

function Get-BootstrapVerifierCacheRoot {
    if ($null -ne $env:WINDVALE_NATIVE_CACHE_ROOT) {
        if ([string]::IsNullOrWhiteSpace($env:WINDVALE_NATIVE_CACHE_ROOT)) {
            throw 'The native bootstrap verifier cache root is empty.'
        }
        return [IO.Path]::GetFullPath($env:WINDVALE_NATIVE_CACHE_ROOT)
    }
    if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) {
        if ([string]::IsNullOrWhiteSpace($env:LOCALAPPDATA)) { throw 'LOCALAPPDATA is unavailable.' }
        return Join-Path $env:LOCALAPPDATA 'Windvale/Native-Tool-Cache'
    }
    $UserCache = if ($null -ne $env:XDG_CACHE_HOME) { $env:XDG_CACHE_HOME } else {
        Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)) '.cache'
    }
    return [IO.Path]::GetFullPath((Join-Path $UserCache 'windvale/native-tool-cache'))
}

function Assert-BootstrapVerifierDirectory {
    param([Parameter(Mandatory)][string]$Path)
    $Current = [IO.DirectoryInfo]::new([IO.Path]::GetFullPath($Path))
    $Count = 0
    while ($null -ne $Current) {
        if (++$Count -gt 512) { throw 'The bootstrap verifier cache path is too deep.' }
        if ([IO.File]::Exists($Current.FullName)) { throw 'The bootstrap verifier cache path contains a file.' }
        if ($null -ne $Current.LinkTarget -or
            ($Current.Exists -and ($Current.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0)) {
            throw 'The bootstrap verifier cache path contains a link.'
        }
        $Current = $Current.Parent
    }
}

function Get-BootstrapVerifierIdentity {
    if ($env:WINDVALE_BOOTSTRAP_VERIFIER_CHECKPOINT -cnotmatch '^[0-9a-f]{64}$') {
        throw 'The bootstrap verifier selection must be an explicit lowercase SHA-256 key.'
    }
    $HostedModule = Join-Path $RepositoryRoot 'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.mjs'
    # The argv marker prevents the imported module's existing CLI guard from
    # running Main. This export only reads and validates an existing checkpoint.
    $ReadIdentity = @'
import { pathToFileURL } from 'node:url';
const Module = await import(pathToFileURL(process.argv[2]));
const Selected = await Module.Readˉbootstrapˉverifier();
if (Selected === null) throw new Error('An explicit bootstrap verifier selection is required.');
process.stdout.write(JSON.stringify(Selected.identity) + '\n');
'@
    $Output = @(& node --input-type=module -e $ReadIdentity bootstrap-verifier-identity $HostedModule 2>&1)
    $ExitCode = $LASTEXITCODE
    $Text = ($Output | ForEach-Object { $_.ToString() }) -join "`n"
    if ($ExitCode -ne 0 -or [Text.Encoding]::UTF8.GetByteCount($Text) -gt 4096) {
        throw "The selected bootstrap verifier could not be admitted (exit $ExitCode): $($Text.Substring(0, [Math]::Min(2048, $Text.Length)))"
    }
    return $Text | ConvertFrom-Json -AsHashtable
}

function Get-BootstrapVerifierSelectionText {
    param([Parameter(Mandatory)][System.Collections.IDictionary]$Identity)
    $CanonicalIdentity = [ordered]@{}
    foreach ($Field in @('key', 'host', 'profile', 'inputBytes', 'inputSha256', 'productBytes', 'productSha256')) {
        if (!$Identity.Contains($Field)) { throw "The bootstrap verifier identity is missing $Field." }
        $CanonicalIdentity[$Field] = $Identity[$Field]
    }
    return (([ordered]@{
        format = 'windvale-bootstrap-complete-verifier-selection-1'
        identity = $CanonicalIdentity
    } | ConvertTo-Json -Depth 4 -Compress) + "`n")
}

function Read-BootstrapVerifierSelection {
    param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)][string]$HostFamily)
    Assert-BootstrapVerifierDirectory -Path (Split-Path -Parent $Path)
    $File = [IO.FileInfo]::new($Path)
    if (!$File.Exists -or $File.Length -lt 1 -or $File.Length -gt 4096 -or
        ($File.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw 'The prepared bootstrap verifier selection descriptor is missing or unsafe.'
    }
    # Bound the read itself even if another process changes the file after stat.
    $Bytes = [byte[]]::new(4097)
    $Stream = [IO.File]::Open($File.FullName, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
    try {
        $Length = 0
        do {
            $Read = $Stream.Read($Bytes, $Length, $Bytes.Length - $Length)
            $Length += $Read
        } while ($Read -gt 0 -and $Length -lt $Bytes.Length)
    } finally { $Stream.Dispose() }
    if ($Length -lt 1 -or $Length -gt 4096) { throw 'The bootstrap verifier selection descriptor changed size.' }
    try {
        $Text = [Text.UTF8Encoding]::new($false, $true).GetString($Bytes, 0, $Length)
        $Record = $Text | ConvertFrom-Json -AsHashtable
    } catch { throw 'The prepared bootstrap verifier selection descriptor is malformed.' }
    if ($Record -isnot [System.Collections.IDictionary] -or $Record.Count -ne 2 -or
        !$Record.Contains('format') -or !$Record.Contains('identity') -or
        $Record.format -cne 'windvale-bootstrap-complete-verifier-selection-1' -or
        $Record.identity -isnot [System.Collections.IDictionary] -or $Record.identity.Count -ne 7 -or
        $Record.identity.host -cne $HostFamily -or $Record.identity.profile -cne '8' -or
        $Record.identity.key -cnotmatch '^[0-9a-f]{64}$') {
        throw 'The prepared bootstrap verifier selection descriptor has an invalid identity or host.'
    }
    $PreviousKey = $env:WINDVALE_BOOTSTRAP_VERIFIER_CHECKPOINT
    try {
        $env:WINDVALE_BOOTSTRAP_VERIFIER_CHECKPOINT = $Record.identity.key
        $Identity = Get-BootstrapVerifierIdentity
        # Reconstructing canonical bytes also rejects duplicate, extra or wrongly
        # cased JSON members, and binds every recorded digest to the actual product.
        if ($Text -cne (Get-BootstrapVerifierSelectionText -Identity $Identity)) {
            throw 'The prepared bootstrap verifier selection descriptor differs from the admitted product.'
        }
    } catch {
        $env:WINDVALE_BOOTSTRAP_VERIFIER_CHECKPOINT = $PreviousKey
        throw
    }
    return $Identity
}

function Write-BootstrapVerifierSelection {
    param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)][System.Collections.IDictionary]$Identity)
    $Parent = Split-Path -Parent $Path
    Assert-BootstrapVerifierDirectory -Path $Parent
    $Text = Get-BootstrapVerifierSelectionText -Identity $Identity
    $Bytes = [Text.UTF8Encoding]::new($false).GetBytes($Text)
    if ($Bytes.Length -gt 4096) { throw 'The bootstrap verifier selection descriptor exceeds its bound.' }
    $Temporary = Join-Path $Parent ('.bootstrap-verifier-selection-' + [Guid]::NewGuid().ToString('N') + '.tmp')
    try {
        $Stream = [IO.File]::Open($Temporary, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
        try { $Stream.Write($Bytes); $Stream.Flush($true) } finally { $Stream.Dispose() }
        [IO.File]::Move($Temporary, $Path, $true)
    } finally {
        if ([IO.File]::Exists($Temporary)) { [IO.File]::Delete($Temporary) }
    }
}

function Initialize-BootstrapVerifierSelection {
    param([bool]$Prepare, [long]$Deadline)
    $HostFamily = if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) { 'windows-x64' } else { 'linux-x64' }
    $CacheRoot = Get-BootstrapVerifierCacheRoot
    $Selection = Join-Path $CacheRoot "Bootstrap-Complete-Verifier-Selection.$HostFamily.json"
    if ($null -ne $env:WINDVALE_BOOTSTRAP_VERIFIER_CHECKPOINT) {
        $Identity = Get-BootstrapVerifierIdentity
    } elseif (!$Prepare) {
        $Identity = Read-BootstrapVerifierSelection -Path $Selection -HostFamily $HostFamily
    } else {
        Assert-BootstrapVerifierDirectory -Path $CacheRoot
        $null = [IO.Directory]::CreateDirectory($CacheRoot)
        Assert-BootstrapVerifierDirectory -Path $CacheRoot
        $CandidateLeaf = '.bootstrap-verifier-preparation-' + [Guid]::NewGuid().ToString('N')
        $Candidate = Join-Path $CacheRoot $CandidateLeaf
        $null = [IO.Directory]::CreateDirectory($Candidate)
        $Suffix = if ($HostFamily -ceq 'windows-x64') { 'exe' } else { 'elf' }
        $Capture = @{ Key = $null; Records = 0 }
        try {
            Write-Host 'Native phase=preparation step=bootstrap-complete-verifier profile=8 behavior-cases=0'
            & node (Join-Path $RepositoryRoot 'Tools/Native/Build-Cached-Segmented-Hosted-Wvb.mjs') `
                --deadline-ms $Deadline 8 (Join-Path $RepositoryRoot 'Artifacts/Native-Front-Door/Wvb/Compiler-Wvb-Verifier.wvb') `
                (Join-Path $Candidate "Verifier.$Suffix") 2>&1 | ForEach-Object {
                    $Line = $_.ToString()
                    Write-Host $Line
                    if ($Line -cmatch '^segmented hosted WVB cache status=(?:Hit|Created) key=([0-9a-f]{64}) host=(windows-x64|linux-x64) target=(windows|linux) profile=8$') {
                        if ($Matches[2] -cne $HostFamily) { throw 'The prepared bootstrap verifier reported a different host.' }
                        $Capture.Key = $Matches[1]
                        $Capture.Records++
                    }
                }
            if ($LASTEXITCODE -ne 0 -or $Capture.Records -ne 1) {
                throw "Bootstrap verifier preparation failed (exit $LASTEXITCODE); completed checkpoints remain reusable."
            }
            $env:WINDVALE_BOOTSTRAP_VERIFIER_CHECKPOINT = $Capture.Key
            $Identity = Get-BootstrapVerifierIdentity
        } finally {
            $Resolved = [IO.DirectoryInfo]::new([IO.Path]::GetFullPath($Candidate))
            if ($Resolved.Parent.FullName -ne [IO.Path]::GetFullPath($CacheRoot) -or $Resolved.Name -cne $CandidateLeaf) {
                throw 'Refusing to remove an unowned bootstrap verifier preparation directory.'
            }
            Assert-BootstrapVerifierDirectory -Path $Resolved.FullName
            Remove-Item -LiteralPath $Resolved.FullName -Recurse -Force
        }
    }
    if ($Prepare) {
        Assert-BootstrapVerifierDirectory -Path $CacheRoot
        $null = [IO.Directory]::CreateDirectory($CacheRoot)
        Write-BootstrapVerifierSelection -Path $Selection -Identity $Identity
    }
    Write-Host "Native bootstrap-complete-verifier selection=Explicit key=$($Identity.key) host=$($Identity.host) profile=8 product-sha256=$($Identity.productSha256) construction=$(if ($Prepare) { 'PreparationOnly' } else { 'Forbidden' })"
}

function Get-NativeSharedCompilerIdentity {
    param([Parameter(Mandatory)][string]$RecordPath,
        [Parameter(Mandatory)][string]$RecordSha256, [Parameter(Mandatory)][long]$Deadline)
    if ($RecordSha256 -cnotmatch '^[0-9a-f]{64}$' -or $Deadline -le [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()) {
        throw 'Invalid prepared shared compiler identity or exhausted deadline.'
    }
    $Reader = Join-Path $RepositoryRoot 'Tools/Native/Build-Shared-Compiler-Host.mjs'
    $ReadIdentity = @'
import { pathToFileURL } from 'node:url';
const Module = await import(pathToFileURL(process.argv[2]));
const Host = await Module.Readˉpreparedˉsharedˉcompilerˉhost(process.argv[3],process.argv[4],undefined,Number(process.argv[5]));
await Host.Requireˉunchanged();
process.stdout.write(JSON.stringify({recordPath:process.argv[3],recordSha256:Host.Recordˉsha256,
    compilerKey:Host.Compilerˉkey,path:Host.Path,bytes:Host.Bytes,sha256:Host.Sha256,
    host:process.platform+'-'+process.arch})+'\n');
'@
    $RecordPath = [IO.Path]::GetFullPath($RecordPath)
    $Output = @(& node --input-type=module -e $ReadIdentity shared-compiler-identity $Reader $RecordPath $RecordSha256 $Deadline 2>&1)
    $ExitCode = $LASTEXITCODE
    $Text = ($Output | ForEach-Object { $_.ToString() }) -join "`n"
    if ($ExitCode -ne 0 -or [Text.Encoding]::UTF8.GetByteCount($Text) -gt 131072) {
        throw "Prepared shared compiler admission failed (exit $ExitCode): $($Text.Substring(0, [Math]::Min(4096, $Text.Length)))"
    }
    $Identity = $Text | ConvertFrom-Json -AsHashtable
    $ExpectedHost = if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) { 'win32-x64' } else { 'linux-x64' }
    if ($Identity.Count -ne 7 -or $Identity.recordPath -cne $RecordPath -or
        $Identity.recordSha256 -cne $RecordSha256 -or $Identity.compilerKey -cnotmatch '^[0-9a-f]{64}$' -or
        $Identity.sha256 -cnotmatch '^[0-9a-f]{64}$' -or $Identity.host -cne $ExpectedHost -or
        $Identity.bytes -lt 1 -or $Identity.bytes -gt 67108864 -or ![IO.Path]::IsPathFullyQualified($Identity.path)) {
        throw 'The prepared shared compiler returned an invalid identity.'
    }
    return $Identity
}

function Get-NativeSharedCompilerSelectionText {
    param([Parameter(Mandatory)][System.Collections.IDictionary]$Identity)
    $Canonical = [ordered]@{}
    foreach ($Field in @('recordPath','recordSha256','compilerKey','path','bytes','sha256','host')) {
        if (!$Identity.Contains($Field)) { throw "The prepared shared compiler identity is missing $Field." }
        $Canonical[$Field] = $Identity[$Field]
    }
    return (([ordered]@{ format = 'windvale-native-shared-compiler-selection-1'; identity = $Canonical } |
        ConvertTo-Json -Depth 4 -Compress) + "`n")
}

function Read-NativeSharedCompilerSelection {
    param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)][long]$Deadline)
    Assert-BootstrapVerifierDirectory (Split-Path -Parent $Path)
    $File = [IO.FileInfo]::new($Path)
    if (!$File.Exists -or $File.Length -lt 1 -or $File.Length -gt 131072 -or $null -ne $File.LinkTarget -or
        ($File.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw 'Prepared shared compiler selection is missing or unsafe. Run the exact selected -PreparationOnly -AllowLongRun phase, or supply -NativeSharedCompilerHostRecord <Host-Bridge.json> -NativeSharedCompilerHostRecordSha256 <sha256>. Behavior never prepares the host or uses the old lowerer.'
    }
    $Bytes = [byte[]]::new(131073)
    $Stream = [IO.File]::Open($Path, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
    try {
        $Length = 0
        do { $Count = $Stream.Read($Bytes, $Length, $Bytes.Length - $Length); $Length += $Count }
        while ($Count -gt 0 -and $Length -lt $Bytes.Length)
    } finally { $Stream.Dispose() }
    if ($Length -lt 1 -or $Length -gt 131072) { throw 'Prepared shared compiler selection changed size.' }
    try {
        $Text = [Text.UTF8Encoding]::new($false, $true).GetString($Bytes, 0, $Length)
        $Value = $Text | ConvertFrom-Json -AsHashtable
    } catch { throw 'Prepared shared compiler selection is malformed.' }
    if ($Value -isnot [System.Collections.IDictionary] -or $Value.Count -ne 2 -or
        $Value.format -cne 'windvale-native-shared-compiler-selection-1' -or
        $Value.identity -isnot [System.Collections.IDictionary] -or $Value.identity.Count -ne 7) {
        throw 'Prepared shared compiler selection has an invalid contract.'
    }
    $Identity = Get-NativeSharedCompilerIdentity $Value.identity.recordPath $Value.identity.recordSha256 $Deadline
    if ($Text -cne (Get-NativeSharedCompilerSelectionText $Identity)) {
        throw 'Prepared shared compiler selection differs from its authenticated current record.'
    }
    return $Identity
}

function Write-NativeSharedCompilerSelection {
    param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)][System.Collections.IDictionary]$Identity)
    $Parent = Split-Path -Parent $Path
    Assert-BootstrapVerifierDirectory $Parent
    $Bytes = [Text.UTF8Encoding]::new($false).GetBytes((Get-NativeSharedCompilerSelectionText $Identity))
    if ($Bytes.Length -gt 131072) { throw 'Prepared shared compiler selection exceeds its bound.' }
    $Temporary = Join-Path $Parent ('.shared-compiler-selection-' + [Guid]::NewGuid().ToString('N') + '.tmp')
    try {
        $Stream = [IO.File]::Open($Temporary, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
        try { $Stream.Write($Bytes); $Stream.Flush($true) } finally { $Stream.Dispose() }
        [IO.File]::Move($Temporary, $Path, $true)
    } finally { if ([IO.File]::Exists($Temporary)) { [IO.File]::Delete($Temporary) } }
}

function Invoke-NativeSharedPreparationStep {
    param([Parameter(Mandatory)][string]$Name, [Parameter(Mandatory)][string[]]$Arguments,
        [Parameter(Mandatory)][long]$Deadline)
    if ([DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() -ge $Deadline) {
        throw 'Shared compiler preparation deadline is exhausted; completed caches remain reusable.'
    }
    Write-Host "Preparation owner=native-x64-lowering-development step=$Name status=Started behavior-cases=0"
    $Lines = [System.Collections.Generic.List[string]]::new()
    $Bytes = 0
    & node @Arguments 2>&1 | ForEach-Object {
        $Line = $_.ToString(); Write-Host $Line
        $Bytes += [Text.Encoding]::UTF8.GetByteCount($Line) + 1
        if ($Bytes -gt 1048576) { throw 'Shared compiler preparation output exceeds its bound.' }
        $Lines.Add($Line)
    }
    if ($LASTEXITCODE -ne 0) { throw "Shared compiler preparation $Name failed (exit $LASTEXITCODE); completed caches remain reusable." }
    if ([DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() -ge $Deadline) { throw 'Shared compiler preparation deadline expired.' }
    Write-Host "Preparation owner=native-x64-lowering-development step=$Name status=Complete behavior-cases=0"
    return $Lines.ToArray()
}

function Prepare-NativeSharedCompilerHost {
    param([Parameter(Mandatory)][long]$Deadline)
    $CacheRoot = Get-BootstrapVerifierCacheRoot
    Assert-BootstrapVerifierDirectory $CacheRoot
    $null = [IO.Directory]::CreateDirectory($CacheRoot)
    $Parent = Join-Path $CacheRoot ('Shared-Compiler-Preparation-' + [Guid]::NewGuid().ToString('N'))
    $null = [IO.Directory]::CreateDirectory($Parent)
    $Native = Join-Path $RepositoryRoot 'Tools/Native'
    $Suffix = if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) { '.exe' } else { '.elf' }
    $null = Invoke-NativeSharedPreparationStep current-compiler @((Join-Path $Native 'Build-Current-Split-Project-Wvb.mjs'),
        '--prepare-only','--deadline-ms',"$Deadline") $Deadline
    $VerifierDeadline = [Math]::Min($Deadline, [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() + 600000)
    $null = Invoke-NativeSharedPreparationStep current-verifier @((Join-Path $Native 'Verify-Wvb.mjs'),
        '--prepare','--deadline-ms',"$VerifierDeadline") $Deadline
    $ReadKey = "import{pathToFileURL}from'node:url';const M=await import(pathToFileURL(process.argv[2]));process.stdout.write(await M.Getˉcurrentˉsplitˉcompilerˉkey());"
    $Key = @(& node --input-type=module -e $ReadKey current-compiler-key (Join-Path $Native 'Current-Split-Compiler-Cache-Core.mjs')) -join ''
    if ($LASTEXITCODE -ne 0 -or $Key -cnotmatch '^[0-9a-f]{64}$') { throw 'Prepared current compiler key could not be read.' }
    $CapacityProduced = Invoke-NativeSharedPreparationStep staging-capacity @((Join-Path $Native 'Native-Staging-Capacity-Bridge-Core.mjs'),
        'prepare','--compiler-checkpoint',$Key,'--workspace-parent',$Parent,'--deadline-ms',"$Deadline") $Deadline
    $CapacityLine = @($CapacityProduced | Where-Object { $_ -cmatch '^staging capacity status=Prepared ' })
    if ($CapacityLine.Count -ne 1 -or $CapacityLine[0] -cnotmatch ' record=(.+) record-sha256=([0-9a-f]{64})$') {
        throw 'Capacity preparation omitted its exact separately prepared producer/linker handoff.'
    }
    $CapacityRecord = $Matches[1]
    $CapacitySha256 = $Matches[2]
    $ReadCapacity = "import{pathToFileURL}from'node:url';const M=await import(pathToFileURL(process.argv[2]));const P=await M.Readˉpreparedˉstagingˉcapacity(process.argv[3],process.argv[4],process.argv[5],Number(process.argv[6]));console.log('staging capacity linker wvb='+P.Record.products.find(V=>V.name==='Linker.wvb').path);"
    $CapacityLinkerLines = Invoke-NativeSharedPreparationStep staging-capacity-read @('--input-type=module','-e',$ReadCapacity,'capacity-linker',
        (Join-Path $Native 'Native-Staging-Capacity-Bridge-Core.mjs'),$CapacityRecord,$CapacitySha256,$Key,"$Deadline") $Deadline
    $CapacityLinkerLine = @($CapacityLinkerLines | Where-Object { $_ -cmatch '^staging capacity linker wvb=' })
    if ($CapacityLinkerLine.Count -ne 1 -or $CapacityLinkerLine[0] -cnotmatch '^staging capacity linker wvb=(.+)$') {
        throw 'Prepared staging pair omitted its exact linker bytecode handoff.'
    }
    $LinkerWvb = $Matches[1]
    $Linker = Join-Path $Parent ('Shared-Host-Linker' + $Suffix)
    # The full compiler can require more than the capacity pair's 32 input slots.
    # Profile 7 preserves 64 slots for the manifest, chunks and provider packet.
    $null = Invoke-NativeSharedPreparationStep shared-host-linker @((Join-Path $Native 'Build-Cached-Segmented-Hosted-Wvb.mjs'),
        '--deadline-ms',"$Deadline",'7',$LinkerWvb,$Linker) $Deadline
    if (!(Test-Path -LiteralPath $Linker -PathType Leaf)) {
        throw 'Prepared staging pair did not yield its exact current compact linker.'
    }
    $ProjectionTool = Join-Path $Native 'Bootstrap-Native-Compiler-Projection.mjs'
    $Prepared = Invoke-NativeSharedPreparationStep projection-snapshot @($ProjectionTool,'prepare',
        '--compiler-checkpoint',$Key,'--deadline-ms',"$Deadline",'--workspace-parent',$Parent,
        '--capacity-record',$CapacityRecord,'--capacity-sha256',$CapacitySha256) $Deadline
    $ProjectionLine = @($Prepared | Where-Object { $_ -cmatch '^native bootstrap projection status=Prepared workspace=' })
    if ($ProjectionLine.Count -ne 1 -or $ProjectionLine[0] -cnotmatch '^native bootstrap projection status=Prepared workspace=(.+) compiler-construction=disabled$') { throw 'Projection preparation omitted its workspace handoff.' }
    $ProjectionWork = $Matches[1]
    $null = Invoke-NativeSharedPreparationStep projection-construct @($ProjectionTool,'construct',
        '--compiler-checkpoint',$Key,'--deadline-ms',"$Deadline",'--workspace',$ProjectionWork) $Deadline
    $AdmissionWvb = Join-Path $Parent 'Admission.wvb'
    $null = Invoke-NativeSharedPreparationStep host-source-products @((Join-Path $Native 'Build-Current-Split-Project-Wvb.mjs'),
        '--compiler-checkpoint',$Key,'--deadline-ms',"$Deadline",
        (Join-Path $RepositoryRoot 'Projects/Linker/Windvale-Shared-Compiler-Byte-Result-Admission.wvproj'),$AdmissionWvb) $Deadline
    $ProjectionRecord = Join-Path $ProjectionWork 'Projection.json'
    # Reuse the admitted hosted module with the complete outer I/O declarations.
    $Carrier = $LinkerWvb
    $HostProduct = Join-Path $Parent ('Shared-Compiler-Host' + $Suffix)
    $Produced = Invoke-NativeSharedPreparationStep host-bridge @((Join-Path $Native 'Build-Shared-Compiler-Host.mjs'),
        '--deadline-ms',"$Deadline",'--workspace-parent',$Parent,
        '--projection-record',$ProjectionRecord,'--projection-sha256',((Get-FileHash $ProjectionRecord -Algorithm SHA256).Hash.ToLowerInvariant()),
        '--staging-linker',$Linker,'--staging-linker-sha256',((Get-FileHash $Linker -Algorithm SHA256).Hash.ToLowerInvariant()),
        '--admission-wvb',$AdmissionWvb,'--admission-wvb-sha256',((Get-FileHash $AdmissionWvb -Algorithm SHA256).Hash.ToLowerInvariant()),
        '--carrier-wvb',$Carrier,'--carrier-wvb-sha256',((Get-FileHash $Carrier -Algorithm SHA256).Hash.ToLowerInvariant()),
        '--output',$HostProduct) $Deadline
    $HostLine = @($Produced | Where-Object { $_ -cmatch '^shared compiler host status=Produced ' })
    if ($HostLine.Count -ne 1 -or $HostLine[0] -cnotmatch ' host-record=(.+) host-record-sha256=([0-9a-f]{64})$') {
        throw 'Shared compiler preparation omitted its exact host record handoff.'
    }
    return Get-NativeSharedCompilerIdentity $Matches[1] $Matches[2] $Deadline
}

function Initialize-NativeSharedCompilerSelection {
    param([bool]$Prepare, [long]$Deadline)
    $CacheRoot = Get-BootstrapVerifierCacheRoot
    $HostFamily = if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) { 'windows-x64' } else { 'linux-x64' }
    $Selection = Join-Path $CacheRoot "Native-Shared-Compiler-Host-Selection.$HostFamily.json"
    if (![string]::IsNullOrWhiteSpace($NativeSharedCompilerHostSelectionPath)) {
        $Selection = [IO.Path]::GetFullPath($NativeSharedCompilerHostSelectionPath)
    }
    if (![string]::IsNullOrWhiteSpace($NativeSharedCompilerHostRecord)) {
        $Identity = Get-NativeSharedCompilerIdentity $NativeSharedCompilerHostRecord $NativeSharedCompilerHostRecordSha256 $Deadline
    } elseif ([IO.File]::Exists($Selection)) {
        # A stale descriptor fails visibly; explicit preparation may replace it.
        if ($Prepare) {
            try { $Identity = Read-NativeSharedCompilerSelection $Selection $Deadline }
            catch { Write-Host 'Preparation owner=native-x64-lowering-development selection=Stale'; $Identity = Prepare-NativeSharedCompilerHost $Deadline }
        } else { $Identity = Read-NativeSharedCompilerSelection $Selection $Deadline }
    } elseif ($Prepare) { $Identity = Prepare-NativeSharedCompilerHost $Deadline }
    else { return Read-NativeSharedCompilerSelection $Selection $Deadline }
    if ($Prepare) {
        # Prepare fixture analysis for both a newly constructed host and a valid
        # reused host. Prepared behavior still refuses every analysis cache miss.
        $SourceHost = if ($HostFamily -ceq 'windows-x64') { 'windows' } else { 'linux' }
        $SourceProducts = Join-Path $RepositoryRoot 'Tools/Native/Test-Native-Unsafe-Write-Pointer-Lowering.mjs'
        foreach ($ProductSelection in @('source','plan','retained')) {
            # Each step retains its existing finite ceiling; all three also
            # share the original absolute preparation deadline.
            $ProductArguments = @($SourceProducts,$SourceHost,$RepositoryRoot,
                '--prepare-shared-source-products','--compiler-checkpoint',$Identity.compilerKey,
                '--selection',$ProductSelection,'--maximum-seconds','900','--deadline-ms',"$Deadline")
            if ($ProductSelection -ceq 'source') {
                $ProductArguments += @('--shared-compiler-host-record',$Identity.recordPath,$Identity.recordSha256)
            }
            $null = Invoke-NativeSharedPreparationStep "shared-$ProductSelection-products" $ProductArguments $Deadline
        }
        $ConfirmedIdentity = Get-NativeSharedCompilerIdentity $Identity.recordPath $Identity.recordSha256 $Deadline
        if ((Get-NativeSharedCompilerSelectionText $ConfirmedIdentity) -cne
            (Get-NativeSharedCompilerSelectionText $Identity)) {
            throw 'The prepared shared compiler changed during source product preparation.'
        }
        Assert-BootstrapVerifierDirectory $CacheRoot
        $null = [IO.Directory]::CreateDirectory($CacheRoot)
        Write-NativeSharedCompilerSelection $Selection $Identity
    }
    Write-Host "Native shared compiler selection=Explicit record-sha256=$($Identity.recordSha256) compiler-checkpoint=$($Identity.compilerKey) product-sha256=$($Identity.sha256) construction=$(if ($Prepare) { 'PreparationOnly' } else { 'Forbidden' })"
    return $Identity
}

if ($PSBoundParameters.ContainsKey('ChangedPath')) {
    $Paths = @($ChangedPath)
} elseif (![string]::IsNullOrWhiteSpace($BaseReference)) {
    $Paths = @(& git -C $RepositoryRoot diff `
        --name-only `
        --no-renames `
        --diff-filter=ACDMRTUXB `
        $BaseReference `
        $HeadReference `
        --)
    if ($LASTEXITCODE -ne 0) {
        throw 'Git could not enumerate the requested committed changes.'
    }
} else {
    $TrackedPaths = @(& git -C $RepositoryRoot diff `
        --name-only `
        --no-renames `
        --diff-filter=ACDMRTUXB `
        HEAD `
        --)
    if ($LASTEXITCODE -ne 0) {
        throw 'Git could not enumerate tracked working-tree changes.'
    }
    $UntrackedPaths = @(& git -C $RepositoryRoot ls-files --others --exclude-standard)
    if ($LASTEXITCODE -ne 0) {
        throw 'Git could not enumerate untracked working-tree changes.'
    }
    $Paths = @($TrackedPaths; $UntrackedPaths)
}

$Paths = @($Paths | Where-Object { ![string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
if ($Paths.Count -eq 0) {
    throw 'No changed paths were found. Supply -BaseReference or -ChangedPath when the working tree is clean.'
}

$Plan = & $Planner -ChangedPath $Paths -PassThru
$NativePlan = if ($Plan.Scope -in @('development', 'qualification')) {
    & $NativePlanner -ChangedPath $Paths -PassThru -PreparedProductsOnly:$UsePreparedProducts
} else {
    [pscustomobject]@{
        Suites = @()
        Gaps = @()
        RunPlanVerification = $false
        RunWebAssemblyEngineVerification = $false
        RunWebAssemblyVerification = $false
        RunGitHubQualificationVerification = $false
        UseSourceContainmentCompilerDevelopment = $false
        ChangedCount = $Paths.Count
    }
}
if ($NativeDevelopmentShard -ne 0) {
    if (!$UsePreparedProducts -or $PreparationOnly -or $Plan.Scope -ne 'development') {
        throw 'Development shards require prepared behavior in development scope.'
    }
    $DevelopmentShards = @(& (Join-Path $PSScriptRoot 'Get-Native-Development-Shards.ps1') -NativePlan $NativePlan)
    if ($NativeDevelopmentShard -gt $DevelopmentShards.Count) { throw 'Development shard is outside the current complete plan.' }
    $DevelopmentShard = $DevelopmentShards[$NativeDevelopmentShard - 1]
    $NativePlan.Suites = @($DevelopmentShard.Suites)
    $NativePlan.ExpectedSeconds = $DevelopmentShard.ExpectedSeconds
    $NativePlan.MaximumSeconds = $DevelopmentShard.MaximumSeconds
    Write-Host "Native development shard=$NativeDevelopmentShard/$($DevelopmentShards.Count) owners=$($NativePlan.Suites.Count) maximum-seconds=$($DevelopmentShard.MaximumSeconds)"
}
if ($PlanVerificationInClassification -and
    ($Plan.Scope -ne 'development' -or
        $env:GITHUB_ACTIONS -ne 'true' -or
        $env:RUNNER_OS -notin @('Windows', 'Linux'))) {
    throw (
        '-PlanVerificationInClassification is reserved for automatic ' +
        'development jobs whose required classification predecessor passed.')
}
if ($GitHubVerificationOnLinux -and
    ($Plan.Scope -ne 'development' -or
        $env:GITHUB_ACTIONS -ne 'true' -or
        $env:RUNNER_OS -ne 'Windows')) {
    throw (
        '-GitHubVerificationOnLinux is reserved for automatic Windows ' +
        'development jobs whose Linux peer runs the GitHub verifier.')
}
if ($PreparationOnly -and $UsePreparedProducts) {
    throw 'Preparation and prepared-product execution are separate phases.'
}
if ([string]::IsNullOrWhiteSpace($NativeSharedCompilerHostRecord) -ne
    [string]::IsNullOrWhiteSpace($NativeSharedCompilerHostRecordSha256)) {
    throw '-NativeSharedCompilerHostRecord and -NativeSharedCompilerHostRecordSha256 must be supplied together.'
}
if (($PreparationOnly -or $UsePreparedProducts) -and $Plan.Scope -ne 'development') {
    throw 'Prepared development phases require development scope.'
}
if (($PreparationOnly -or $UsePreparedProducts) -and
    $NativePlan.Suites -contains 'language-1-authenticated-foreign-binding') {
    if ($PreparationOnly) {
        Write-Host "Native phase=preparation owner=language-1-authenticated-foreign-binding maximum-seconds=$PreparationMaximumSeconds behavior-cases=0"
    } else {
        Write-Host 'Native phase=prepared-execution owner=language-1-authenticated-foreign-binding maximum-seconds=600 behavior-cases=27 construction=Forbidden'
    }
}
if ($PlanOnly) {
    return
}
$NativeSharedCompilerIdentity = $null
$BootstrapVerifierRequired = $Plan.Scope -eq 'development' -and (
    $NativePlan.UseOwnedConsoleDevelopment -or $NativePlan.UseCurrentVerifierDevelopment -or
    $NativePlan.UseFoundationLibraryDevelopment -or $NativePlan.UseFoundationBorrowOwnerDevelopment -or
    $NativePlan.UseLanguage1FrontDoorPreparation -or $NativePlan.UseCallablePreparation -or
    @($NativePlan.Suites | Where-Object { $_ -eq 'language-1-authenticated-foreign-binding' -or
        ($_ -eq 'native-x64-lowering-development' -and !$NativePlan.UseNativeSharedStorageDevelopment) }).Count -ne 0)
if ($BootstrapVerifierRequired -and ($PreparationOnly -or $UsePreparedProducts -or $NativePlan.NativeSharedCompilerBehaviorRequired)) {
    if ($PreparationOnly -and (!$AllowLongRun -or $NativePlan.Gaps.Count -ne 0)) {
        throw 'Bootstrap verifier preparation requires a bounded -AllowLongRun selection without coverage gaps.'
    }
    $BootstrapVerifierDeadline = [DateTimeOffset]::new($VerificationStartedUtc).ToUnixTimeMilliseconds() + [long]$PreparationMaximumSeconds * 1000
    Initialize-BootstrapVerifierSelection -Prepare $PreparationOnly -Deadline $BootstrapVerifierDeadline
}
if ($NativePlan.NativeSharedCompilerBehaviorRequired -and !$PreparationOnly) {
    # The current compiler key includes the explicitly selected bootstrap
    # verifier. Admit that read-only selection before measuring the host key.
    $NativeSharedCompilerIdentity = Initialize-NativeSharedCompilerSelection -Prepare $false `
        -Deadline ([DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() + 600000)
}
if ($PreparationOnly) {
    if (!$AllowLongRun) { throw 'Preparation requires an explicitly bounded -AllowLongRun selection.' }
    if ($NativePlan.Gaps.Count -ne 0) { throw 'Preparation cannot proceed with native coverage gaps.' }
    if ($NativePlan.Suites -contains 'language-1-authenticated-foreign-binding') {
        $RemainingSeconds = [int][Math]::Floor($PreparationMaximumSeconds - ([DateTime]::UtcNow - $VerificationStartedUtc).TotalSeconds)
        if ($RemainingSeconds -lt 60) { throw 'The shared preparation deadline is exhausted; completed caches remain reusable.' }
        Write-Host "Preparation owner=language-1-authenticated-foreign-binding maximum-seconds=$RemainingSeconds"
        & node (Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Authenticated-Foreign-Binding.mjs') `
            --prepare-only --maximum-seconds $RemainingSeconds
        if ($LASTEXITCODE -ne 0) { throw "Native preparation failed with exit $LASTEXITCODE; completed caches remain reusable." }
    }
    if ($NativePlan.Suites -contains 'native-x64-lowering-development' -and
        !$NativePlan.UseNativeSharedStorageDevelopment) {
        $RemainingSeconds = [int][Math]::Floor($PreparationMaximumSeconds - ([DateTime]::UtcNow - $VerificationStartedUtc).TotalSeconds)
        if ($RemainingSeconds -lt 30) { throw 'The shared preparation deadline is exhausted; completed caches remain reusable.' }
        $HostTarget = if ($IsWindows -or [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT) { 'windows' } else { 'linux' }
        if ($NativePlan.NativeSharedCompilerBehaviorRequired) {
            $NativeSharedCompilerIdentity = Initialize-NativeSharedCompilerSelection -Prepare $true `
                -Deadline $BootstrapVerifierDeadline
        } else {
            Write-Host "Preparation owner=native-x64-lowering-development maximum-seconds=$RemainingSeconds"
            & node (Join-Path $RepositoryRoot 'Tools/Native/Test-Native-Unsafe-Write-Pointer-Lowering.mjs') `
                $HostTarget $RepositoryRoot --prepare-only --maximum-seconds $RemainingSeconds
            if ($LASTEXITCODE -ne 0) { throw "Native lowerer preparation failed with exit $LASTEXITCODE; completed caches remain reusable." }
        }
    }
    if ($NativePlan.UseOwnedConsoleDevelopment) {
        $RemainingSeconds = [int][Math]::Floor($PreparationMaximumSeconds - ([DateTime]::UtcNow - $VerificationStartedUtc).TotalSeconds)
        if ($RemainingSeconds -lt 60) { throw 'The shared preparation deadline is exhausted; completed caches remain reusable.' }
        Write-Host "Preparation owner=console-packager-source-reconstruction maximum-seconds=$RemainingSeconds"
        & node (Join-Path $RepositoryRoot 'Tools/Native/Test-Console-Packager-Source-Reconstruction.mjs') `
            --prepare-only --maximum-seconds $RemainingSeconds
        if ($LASTEXITCODE -ne 0) { throw "Owned console preparation failed with exit $LASTEXITCODE; completed caches remain reusable." }
    }
    if ($NativePlan.UseCurrentVerifierDevelopment) {
        $RemainingSeconds = [int][Math]::Floor($PreparationMaximumSeconds - ([DateTime]::UtcNow - $VerificationStartedUtc).TotalSeconds)
        if ($RemainingSeconds -lt 60) { throw 'The shared preparation deadline is exhausted; completed caches remain reusable.' }
        Write-Host "Preparation owner=language-1-production-admission-ingress mode=current-verifier maximum-seconds=$RemainingSeconds"
        & node (Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Production-Admission-Ingress.mjs') `
            --prepare-current-verifier --maximum-seconds $RemainingSeconds
        if ($LASTEXITCODE -ne 0) { throw "Current verifier preparation failed with exit $LASTEXITCODE; completed caches remain reusable." }
    }
    if ($NativePlan.UseFoundationLibraryDevelopment) {
        $RemainingSeconds = [int][Math]::Floor($PreparationMaximumSeconds - ([DateTime]::UtcNow - $VerificationStartedUtc).TotalSeconds)
        if ($RemainingSeconds -lt 60) { throw 'The shared preparation deadline is exhausted; completed caches remain reusable.' }
        $FoundationPreparationDeadline = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() + [long]$RemainingSeconds * 1000
        Write-Host "Preparation owner=libraries target=foundation-values maximum-seconds=$RemainingSeconds behavior-cases=0"
        & node (Join-Path $RepositoryRoot 'Tools/Native/Build-Current-Split-Project-Wvb.mjs') `
            --prepare-only --deadline-ms $FoundationPreparationDeadline
        if ($LASTEXITCODE -ne 0) { throw "Foundation compiler preparation failed with exit $LASTEXITCODE; completed caches remain reusable." }
        $FoundationVerifierDeadline = [Math]::Min($FoundationPreparationDeadline, [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() + 600000)
        & node (Join-Path $RepositoryRoot 'Tools/Native/Verify-Wvb.mjs') `
            --prepare --deadline-ms $FoundationVerifierDeadline
        if ($LASTEXITCODE -ne 0) { throw "Foundation verifier preparation failed with exit $LASTEXITCODE; completed caches remain reusable." }
    }
    if ($NativePlan.UseFoundationBorrowOwnerDevelopment) {
        $RemainingSeconds = [int][Math]::Floor($PreparationMaximumSeconds - ([DateTime]::UtcNow - $VerificationStartedUtc).TotalSeconds)
        if ($RemainingSeconds -lt 30) { throw 'The shared preparation deadline is exhausted; completed caches remain reusable.' }
        $FoundationOwnerSeconds = [int][Math]::Min(4500, $RemainingSeconds)
        Write-Host "Preparation owner=language-1-memory-budget-split-execution mode=foundation-borrow-owners maximum-seconds=$FoundationOwnerSeconds behavior-cases=0"
        & node (Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs') `
            --foundation-borrow-owners --prepare-only --maximum-seconds $FoundationOwnerSeconds
        if ($LASTEXITCODE -ne 0) { throw "Foundation owner preparation failed with exit $LASTEXITCODE; completed caches remain reusable." }
    }
    if ($NativePlan.UseCallablePreparation) {
        $RemainingSeconds = [int][Math]::Floor($PreparationMaximumSeconds - ([DateTime]::UtcNow - $VerificationStartedUtc).TotalSeconds)
        if ($RemainingSeconds -lt 30) { throw 'The shared preparation deadline is exhausted; completed caches remain reusable.' }
        $CallablePreparationSeconds = [int][Math]::Min(4500, $RemainingSeconds)
        $CallablePreparationDeadline = [Math]::Min($BootstrapVerifierDeadline, [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() + [long]$CallablePreparationSeconds * 1000)
        if ($null -eq $NativeSharedCompilerIdentity -and !$NativePlan.UseFoundationLibraryDevelopment) {
            Write-Host "Preparation owner=language-1-callable-semantics step=current-compiler maximum-seconds=$CallablePreparationSeconds behavior-cases=0"
            & node (Join-Path $RepositoryRoot 'Tools/Native/Build-Current-Split-Project-Wvb.mjs') `
                --prepare-only --deadline-ms $CallablePreparationDeadline
            if ($LASTEXITCODE -ne 0) { throw "Callable compiler preparation failed with exit $LASTEXITCODE; completed caches remain reusable." }
        }
        $CallablePreparationSeconds = [int][Math]::Min(4500, [Math]::Floor(($CallablePreparationDeadline - [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()) / 1000))
        if ($CallablePreparationSeconds -lt 30) { throw 'The callable preparation deadline is exhausted; completed caches remain reusable.' }
        $CallableOwnerDeadline = [Math]::Min($CallablePreparationDeadline, [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() + [long]$CallablePreparationSeconds * 1000)
        Write-Host "Preparation owner=language-1-callable-semantics maximum-seconds=$CallablePreparationSeconds behavior-cases=0"
        & node (Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Callable-Semantics.mjs') `
            --prepare-only --maximum-seconds $CallablePreparationSeconds --deadline-ms $CallableOwnerDeadline
        if ($LASTEXITCODE -ne 0) { throw "Callable preparation failed with exit $LASTEXITCODE; completed caches remain reusable." }
    }
    if ($NativePlan.UseLanguage1FrontDoorPreparation) {
        $RemainingSeconds = [int][Math]::Floor($PreparationMaximumSeconds - ([DateTime]::UtcNow - $VerificationStartedUtc).TotalSeconds)
        if ($RemainingSeconds -lt 60) { throw 'The shared preparation deadline is exhausted; completed caches remain reusable.' }
        Write-Host "Preparation owner=language-1-front-door target=$($NativePlan.Language1FrontDoorDevelopmentTarget) maximum-seconds=$RemainingSeconds behavior-cases=0"
        & node (Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Front-Door-Development.mjs') `
            --prepare-only --maximum-seconds $RemainingSeconds --target $NativePlan.Language1FrontDoorDevelopmentTarget
        if ($LASTEXITCODE -ne 0) { throw "Front-end preparation failed with exit $LASTEXITCODE; completed caches remain reusable." }
    }
    if (!$NativePlan.UseOwnedConsoleDevelopment -and !$NativePlan.UseCurrentVerifierDevelopment -and
        !$NativePlan.UseFoundationLibraryDevelopment -and !$NativePlan.UseFoundationBorrowOwnerDevelopment -and
        !$NativePlan.UseLanguage1FrontDoorPreparation -and !$NativePlan.UseCallablePreparation -and
        !$NativePlan.UseNativeSharedStorageDevelopment -and
        @($NativePlan.Suites | Where-Object { $_ -in @('language-1-authenticated-foreign-binding', 'native-x64-lowering-development') }).Count -eq 0) {
        Write-Host 'Native preparation status=NotRequired selected-preparation-owners=0'
    }
    return
}
if ($AllowIncompleteInfrastructure -and $Plan.Scope -ne 'development') {
    throw '-AllowIncompleteInfrastructure is valid only for development scope.'
}

if ($PSBoundParameters.ContainsKey('ChangedPath')) {
    git -C $RepositoryRoot diff --check
} elseif (![string]::IsNullOrWhiteSpace($BaseReference)) {
    git -C $RepositoryRoot diff --check $BaseReference $HeadReference --
} else {
    git -C $RepositoryRoot diff --check HEAD --
}
if ($LASTEXITCODE -ne 0) {
    throw 'Changed-file whitespace verification failed.'
}

$RunDocumentationVerification = @(
    $Paths | Where-Object {
        $_.EndsWith('.md', [StringComparison]::OrdinalIgnoreCase) -or
        $_.StartsWith('Documents/Evidence/', [StringComparison]::Ordinal) -or
        $_.StartsWith('Tools/Documentation/', [StringComparison]::Ordinal) -or
        $_ -in @(
            'Documents/Decisions/Decision-Catalog.json',
            'Documents/Decisions/Legacy-Id-Collisions.txt',
            'Documents/Decisions/Legacy-Missing-Status.txt',
            'Specifications/Legacy-Missing-Status.txt',
            'Specifications/Legacy-Status-Classifications.json',
            'Specifications/Specification-Catalog.json',
            'Tools/Verify/Verify-Documentation.ps1'
        )
    }
).Count -ne 0
if ($RunDocumentationVerification -and !$SkipDocumentationVerification) {
    & $DocumentationVerifier
}

$RunChangeClassificationVerification = @(
    $Paths | Where-Object {
        $_ -in @(
            'Tools/Verify/Classify-Verification-Changes.ps1',
            'Tools/Verify/Verify-Changed.ps1',
            'Tools/Verify/Verify-Change-Classification.ps1'
        )
    }
).Count -ne 0
if ($RunChangeClassificationVerification -and $NativeDevelopmentShard -le 1) {
    & $ChangeClassificationVerifier
}

if ($Plan.Editor -and $NativeDevelopmentShard -le 1) {
    & $EditorVerifier
}

if ($Plan.Scope -in @('development', 'qualification') -and
    $NativePlan.Gaps.Count -ne 0) {
    throw (
        'Changed-file verification has uncovered native evidence gaps: ' +
        ($NativePlan.Gaps -join ', ') +
        '. Add or select a native owner; no managed fallback was invoked.'
    )
}
if ($Plan.Website) {
    & $WebsiteVerifier
}
if ($Plan.Scope -in @('development', 'qualification')) {

    Write-Warning 'Changed-file verification is native development feedback, not conformance or qualification evidence.'
    $Failures = [System.Collections.Generic.List[string]]::new()
    $Incomplete = [System.Collections.Generic.List[string]]::new()
    $Timings = [System.Collections.Generic.List[object]]::new()
    if ($NativePlan.RunPlanVerification -and
        !$PlanVerificationInClassification) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        try {
            & $PlanVerifier
        } catch {
            $Failures.Add('verification-plan')
            if (!$NoFailFast) { throw }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = 'verification-plan'
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
            })
        }
    }

    if ($NativePlan.RunGitHubQualificationVerification -and $NativeDevelopmentShard -le 1 -and
        !$GitHubVerificationOnLinux) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        try {
            & $GitHubQualificationVerifier
        } catch {
            $Failures.Add('github-qualification')
            if (!$NoFailFast) { throw }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = 'github-qualification'
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
            })
        }
    }

    $IsWindowsHost = [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT
    $Coordinator = Join-Path $PSScriptRoot 'Invoke-WindvaleTests.ps1'
    $OwnerContractHashes = @{}
    $OwnerMaximumSeconds = @{}
    if (@($NativePlan.Suites).Count -ne 0) {
        $OwnerLines = [IO.File]::ReadAllLines($VerificationOwnerRegistry)
        $DurationLines = [IO.File]::ReadAllLines((Join-Path $RepositoryRoot 'Tests/Native/Verification-Duration-Profiles.txt'))
        $DurationMaximums = @{}
        if ($DurationLines[0] -cne 'windvale-native-verification-duration-profiles 1') { throw 'The native duration registry header differs.' }
        foreach ($DurationLine in @($DurationLines | Select-Object -Skip 1)) {
            $DurationFields = $DurationLine -split '\|', 4
            if ($DurationFields.Count -ne 4 -or $DurationFields[2] -cnotmatch '^[1-9][0-9]*$' -or
                $DurationMaximums.ContainsKey($DurationFields[0])) { throw 'The native duration registry row differs.' }
            $DurationMaximums[$DurationFields[0]] = [long]$DurationFields[2]
        }
        if ($OwnerLines.Count -lt 2 -or
            $OwnerLines[0] -cne 'windvale-native-verification-owners 2') {
            throw 'The verification-owner registry header differs.'
        }
        foreach ($OwnerLine in @($OwnerLines | Select-Object -Skip 1)) {
            $OwnerFields = $OwnerLine -split '\|', 6
            if ($OwnerFields.Count -ne 6 -or
                [string]::IsNullOrWhiteSpace($OwnerFields[0]) -or
                $OwnerContractHashes.ContainsKey($OwnerFields[0])) {
                throw "The verification-owner registry row is malformed: $OwnerLine"
            }
            $OwnerContractHashes[$OwnerFields[0]] = Get-Sha256Text $OwnerLine
            if (!$DurationMaximums.ContainsKey($OwnerFields[4])) { throw 'The native owner has no duration profile.' }
            $OwnerMaximumSeconds[$OwnerFields[0]] = $DurationMaximums[$OwnerFields[4]]
        }
        foreach ($SelectedOwner in @($NativePlan.Suites)) {
            if (!$OwnerContractHashes.ContainsKey($SelectedOwner)) {
                throw "The selected native owner is not registered: $SelectedOwner"
            }
        }
    }
    $ResultCacheState = $null
    $CompatibleResultPlanCache = @{}
    if ($Plan.Scope -eq 'development' -and !$NoResultCache -and
        @($NativePlan.Suites).Count -ne 0) {
        try {
            $PrepareArguments = @('prepare', $RepositoryRoot)
            if ($PSBoundParameters.ContainsKey('ResultCacheRoot')) {
                $PrepareArguments += $ResultCacheRoot
            }
            $ResultCacheState = (
                Invoke-VerificationResultCache -CacheArgument $PrepareArguments
            ) | ConvertFrom-Json
            if ($ResultCacheState.format -ne 'windvale-verification-owner-state-1' -or
                $ResultCacheState.stateKey -notmatch '^[0-9a-f]{64}$' -or
                $ResultCacheState.sourceTree -notmatch '^[0-9a-f]{40}(?:[0-9a-f]{24})?$' -or
                $ResultCacheState.sourceSentinel -notmatch '^[0-9a-f]{64}$' -or
                $ResultCacheState.repositoryKey -notmatch '^[0-9a-f]{64}$' -or
                $ResultCacheState.hostKey -notmatch '^[0-9a-f]{64}$') {
                throw 'Verification result cache returned an invalid state record.'
            }
            Write-Host (
                'Verification result cache status=Ready ' +
                "state=$($ResultCacheState.stateKey.Substring(0, 12))"
            )
        } catch {
            Write-Warning (
                'Persistent verification resume is unavailable; owners will run: ' +
                $_.Exception.Message
            )
            $ResultCacheState = $null
        }
    }
    $GenericNominalBundlePassed = $false
    $GenericNominalBundleSentinel = $null
    $GenericNominalBundleOwners = @(
        'generic-nominal-type-binding',
        'generic-nominal-type-layout',
        'generic-nominal-type-materialization')
    foreach ($Suite in $NativePlan.Suites) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        $TimingStatus = 'executed'
        $TimingOutcome = 'passed'
        $OwnerExitCode = 0
        $StopAfterOwner = $false
        try {
            $IsGenericNominalBundle = $Plan.Scope -eq 'development' -and
                $NativePlan.UseGenericNominalDevelopmentBundle -and
                $Suite -in $GenericNominalBundleOwners
            if ($IsGenericNominalBundle -and $GenericNominalBundlePassed) {
                $BundleConfirmation = Invoke-VerificationResultCache -CacheArgument @(
                    'confirm', $RepositoryRoot, $GenericNominalBundleSentinel)
                if ($BundleConfirmation -ne 'Unchanged') {
                    throw 'Repository inputs changed after the generic nominal bundle passed.'
                }
                $TimingStatus = 'covered-by-bundle'
                Write-Host "PASS  native owner $Suite result=CoveredByBundle bundle-cases=145"
                continue
            }
            if ($IsGenericNominalBundle) {
                $GenericNominalBundleSentinel = Invoke-VerificationResultCache -CacheArgument @(
                    'measure', $RepositoryRoot)
                if ($GenericNominalBundleSentinel -notmatch '^[0-9a-f]{64}$') {
                    throw 'Generic nominal bundle source measurement is invalid.'
                }
            }
            $OwnerCommand = $Coordinator
            $OwnerArguments = @('-Owner', $Suite)
            if ($AllowLongRun) {
                $OwnerArguments += '-AllowLongRun'
            }
            $OwnerMessage = $null
            $OwnerContinuation = $null
            $IsNativeSharedCompiler = $Suite -eq 'native-x64-lowering-development' -and $NativePlan.NativeSharedCompilerBehaviorRequired
            $NativeSharedCompilerDeadline = 0
            if ($Suite -eq 'native-x64-lowering-development' -and $UsePreparedProducts -and
                !$NativePlan.UseNativeSharedStorageDevelopment) {
                $OwnerCommand = 'node'
                $HostTarget = if ($IsWindowsHost) { 'windows' } else { 'linux' }
                $OwnerArguments = @((Join-Path $RepositoryRoot 'Tools/Native/Test-Native-Unsafe-Write-Pointer-Lowering.mjs'),
                    $HostTarget, $RepositoryRoot, '--prepared-products-only', '--maximum-seconds', '600')
                $OwnerMessage = 'Native owner native-x64-lowering-development mode=prepared-products maximum-seconds=600'
            }
            if ($Suite -eq 'native-x64-lowering-development' -and $NativePlan.UseNativeSharedStorageDevelopment) {
                $OwnerCommand = 'node'
                $HostTarget = if ($IsWindowsHost) { 'windows' } else { 'linux' }
                $OwnerArguments = @((Join-Path $RepositoryRoot 'Tools/Native/Test-Native-Unsafe-Write-Pointer-Lowering.mjs'),
                    $HostTarget, $RepositoryRoot, '--shared-storage')
                $OwnerMessage = 'Native owner native-x64-lowering-development mode=shared-storage cases=17 expected-seconds=540 maximum-seconds=600 compiler-construction=Forbidden'
            }
            if ($IsNativeSharedCompiler) {
                $ExpectedSeconds = [long]$NativePlan.NativeSharedCompilerBehaviorExpectedSeconds
                $MaximumSeconds = [long]$NativePlan.NativeSharedCompilerBehaviorMaximumSeconds
                if ($ExpectedSeconds -le 0 -or $ExpectedSeconds -gt $MaximumSeconds -or $MaximumSeconds -gt 7200) {
                    throw 'Prepared shared compiler owner has an invalid selected duration.'
                }
                $NativeSharedCompilerDeadline = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() + $MaximumSeconds * 1000
                $ConfirmedSharedCompiler = Get-NativeSharedCompilerIdentity $NativeSharedCompilerIdentity.recordPath `
                    $NativeSharedCompilerIdentity.recordSha256 $NativeSharedCompilerDeadline
                if ((Get-NativeSharedCompilerSelectionText $ConfirmedSharedCompiler) -cne
                    (Get-NativeSharedCompilerSelectionText $NativeSharedCompilerIdentity)) {
                    throw 'Prepared shared compiler selection changed before owner execution.'
                }
                $OwnerCommand = 'node'
                $HostTarget = if ($IsWindowsHost) { 'windows' } else { 'linux' }
                $OwnerArguments = @((Join-Path $RepositoryRoot 'Tools/Native/Test-Native-Unsafe-Write-Pointer-Lowering.mjs'),
                    $HostTarget, $RepositoryRoot, '--shared-compiler-values', '--shared-compiler-host',
                    $NativeSharedCompilerIdentity.path, $NativeSharedCompilerIdentity.sha256,
                    '--maximum-seconds', "$MaximumSeconds")
                if (!$NativePlan.UseNativeSharedCompilerDevelopment) { $OwnerArguments += '--full-lowering' }
                $OwnerArguments += @('--deadline-ms', "$NativeSharedCompilerDeadline", '--shared-compiler-host-record',
                    $NativeSharedCompilerIdentity.recordPath, $NativeSharedCompilerIdentity.recordSha256)
                $OwnerMessage = "Native owner native-x64-lowering-development mode=prepared-shared-compiler source-cases=10 consumer-cases=3 full-lowering=$(!$NativePlan.UseNativeSharedCompilerDevelopment) expected-seconds=$ExpectedSeconds maximum-seconds=$MaximumSeconds compiler-construction=Forbidden"
            }
            if ($Suite -eq 'language-1-callable-semantics' -and $UsePreparedProducts) {
                $OwnerCommand = 'node'
                $OwnerArguments = @((Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Callable-Semantics.mjs'),
                    '--prepared-products-only', '--maximum-seconds', '3600')
                $OwnerMessage = 'Native owner language-1-callable-semantics mode=prepared-products cases=64 maximum-seconds=3600'
            } elseif ($Suite -eq 'language-1-authenticated-foreign-binding' -and $UsePreparedProducts) {
                $OwnerExtension = if ($IsWindowsHost) { 'cmd' } else { 'sh' }
                $OwnerCommand = Join-Path $RepositoryRoot (
                    "Tools/Native/Test-Language-1.0-Authenticated-Foreign-Binding.$OwnerExtension")
                $OwnerArguments = @('--prepared-products-only', '--maximum-seconds', '600')
                $OwnerMessage = 'Native owner language-1-authenticated-foreign-binding mode=prepared-products cases=27 maximum-seconds=600'
            } elseif ($Suite -eq 'compiler-reconstruction' -and
                $Plan.Scope -eq 'development') {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Compiler-Reconstruction.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Compiler-Reconstruction.sh'
                }
                $OwnerArguments = @('--development')
                $OwnerMessage = (
                    'Native owner compiler-reconstruction ' +
                    'mode=development-smoke')
            } elseif ($Suite -eq 'wvb-runner-reconstruction' -and
                $Plan.Scope -eq 'development') {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Wvb-Runner-Reconstruction.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Wvb-Runner-Reconstruction.sh'
                }
                $OwnerArguments = @('--development')
                $OwnerMessage = (
                    'Native owner wvb-runner-reconstruction ' +
                    'mode=development-candidate-smoke')
            } elseif ($Suite -eq 'console-packager-source-reconstruction' -and
                $Plan.Scope -eq 'development' -and $NativePlan.UseOwnedConsoleDevelopment) {
                $OwnerExtension = if ($IsWindowsHost) { 'cmd' } else { 'sh' }
                $OwnerCommand = Join-Path $RepositoryRoot (
                    "Tools/Native/Test-Console-Packager-Source-Reconstruction.$OwnerExtension")
                $OwnerArguments = @('--owned-current', '--maximum-seconds', '600')
                if ($UsePreparedProducts) {
                    $OwnerArguments[0] = '--prepared-products-only'
                }
                $OwnerMessage = 'Native owner console-packager-source-reconstruction mode=owned-current expected-seconds=300 maximum-seconds=600'
            } elseif ($Suite -eq 'hosted-verifier-publisher-files' -and
                $Plan.Scope -eq 'development' -and $NativePlan.UsePublisherCurrentObjectDevelopment) {
                $OwnerExtension = if ($IsWindowsHost) { 'cmd' } else { 'sh' }
                $OwnerCommand = Join-Path $RepositoryRoot (
                    "Tools/Native/Test-Hosted-Verifier-Publisher-File-Pipeline.$OwnerExtension")
                $OwnerArguments = @('--current-objects')
                $OwnerMessage = 'Native owner hosted-verifier-publisher-files mode=current-objects cases=1 expected-seconds=180'
            } elseif ($Suite -eq 'hosted-verifier-publisher-files' -and
                $Plan.Scope -eq 'development' -and $NativePlan.UsePublisherCurrentSourceDevelopment) {
                $OwnerExtension = if ($IsWindowsHost) { 'cmd' } else { 'sh' }
                $OwnerCommand = Join-Path $RepositoryRoot (
                    "Tools/Native/Test-Hosted-Verifier-Publisher-File-Pipeline.$OwnerExtension")
                $OwnerArguments = @('--current-source')
                $OwnerMessage = 'Native owner hosted-verifier-publisher-files mode=current-source cases=1 expected-seconds=60'
            } elseif ($Suite -eq 'language-1-production-admission-ingress' -and
                $Plan.Scope -eq 'development' -and $NativePlan.UseCurrentVerifierDevelopment) {
                $OwnerCommand = 'node'
                $OwnerArguments = @((Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Production-Admission-Ingress.mjs'), '--current-verifier')
                $OwnerMessage = 'Native owner language-1-production-admission-ingress mode=current-verifier cases=23 expected-seconds=300 maximum-seconds=600'
            } elseif ($Suite -eq 'language-1-production-admission-ingress' -and
                $Plan.Scope -eq 'development' -and $NativePlan.UseProject4LauncherDevelopment) {
                $OwnerExtension = if ($IsWindowsHost) { 'cmd' } else { 'sh' }
                $OwnerCommand = Join-Path $RepositoryRoot (
                    "Tools/Native/Test-Language-1.0-Production-Admission-Ingress.$OwnerExtension")
                $OwnerArguments = @('--project4-launcher')
                $OwnerMessage = 'Native owner language-1-production-admission-ingress mode=project4-launcher cases=9 expected-seconds=900'
            } elseif ($Suite -eq 'compiler-split-development' -and
                $Plan.Scope -eq 'development' -and $NativePlan.UseConstructionReadinessDevelopment -and
                !$NativePlan.AnalysisDiagnosticsRequired) {
                $OwnerCommand = 'node'
                $OwnerArguments = @('--input-type=module', '-e', $ConstructionReadinessRunner,
                    (Join-Path $RepositoryRoot 'Tools/Native/Development-Command-Core.mjs'),
                    (Join-Path $RepositoryRoot 'Tools/Native/Test-Cached-Split-Project-Wvb.mjs'))
                $OwnerMessage = 'Native owner compiler-split-development mode=construction-readiness readiness-cases=23 process-actions=13 package-deadline-assertions=12 expected-seconds=15 maximum-seconds=120 compiler-executions=0'
            } elseif ($Suite -eq 'compiler-split-development' -and
                $Plan.Scope -eq 'development' -and $NativePlan.AnalysisDiagnosticsRequired) {
                if ($null -eq $NativeSharedCompilerIdentity -or
                    $NativeSharedCompilerIdentity.compilerKey -cnotmatch '^[0-9a-f]{64}$') {
                    throw 'Analysis diagnostics require an admitted explicit current shared compiler selection.'
                }
                $DiagnosticsSelection = if ($NativePlan.UseAnalysisDiagnosticsWithExisting) {
                    '--analysis-diagnostics-with-existing'
                } else { '--analysis-diagnostics' }
                $OwnerCommand = 'node'
                $OwnerArguments = @((Join-Path $RepositoryRoot 'Tools/Native/Test-Compiler-Split-Development.mjs'),
                    $DiagnosticsSelection, $NativeSharedCompilerIdentity.compilerKey)
                $OwnerMessage = 'Native owner compiler-split-development mode=analysis-diagnostics cases=3 expected-seconds=30 maximum-seconds=120 construction=Forbidden'
                if ($NativePlan.UseAnalysisDiagnosticsWithExisting) {
                    $OwnerMessage = 'Native owner compiler-split-development mode=existing-and-analysis-diagnostics existing-cases=4 analysis-cases=3 expected-seconds=330 maximum-seconds=720'
                }
            } elseif ($Suite -eq 'compiler-split-development' -and
                $Plan.Scope -eq 'development' -and $NativePlan.UseProject4PublisherCacheDevelopment) {
                $OwnerCommand = 'node'
                $OwnerArguments = @((Join-Path $RepositoryRoot 'Tools/Native/Test-Compiler-Split-Development.mjs'), '--publisher-cache')
                $OwnerMessage = 'Native owner compiler-split-development mode=publisher-cache cases=5 expected-seconds=60 maximum-seconds=180'
            } elseif ($Suite -eq 'language-1-front-door' -and
                $Plan.Scope -eq 'development') {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Front-Door.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Front-Door.sh'
                }
                $OwnerArguments = @('--development')
                $OwnerMessage = (
                    'Native owner language-1-front-door ' +
                    "mode=development-front-end cases=$($NativePlan.Language1FrontDoorDevelopmentCaseCount) " +
                    "target=$($NativePlan.Language1FrontDoorDevelopmentTarget) " +
                    "expected-seconds=$($NativePlan.Language1FrontDoorDevelopmentExpectedSeconds)")
                if ($NativePlan.Language1FrontDoorDevelopmentTarget -ne 'all') {
                    $OwnerArguments = @('--development-target', $NativePlan.Language1FrontDoorDevelopmentTarget)
                }
                if ($UsePreparedProducts -and $NativePlan.UseLanguage1FrontDoorPreparation) {
                    $OwnerCommand = 'node'
                    $OwnerArguments = @((Join-Path $RepositoryRoot 'Tools/Native/Test-Language-1.0-Front-Door-Development.mjs'),
                        '--target', $NativePlan.Language1FrontDoorDevelopmentTarget, '--prepared-products-only')
                    $OwnerMessage += ' construction=Forbidden'
                }
            } elseif ($Suite -eq 'language-1-memory-budget-split-execution' -and
                $Plan.Scope -eq 'development' -and $NativePlan.UseVectorBorrowIntegrationDevelopment) {
                $OwnerExtension = if ($IsWindowsHost) { 'cmd' } else { 'sh' }
                $OwnerCommand = Join-Path $RepositoryRoot (
                    "Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.$OwnerExtension")
                $OwnerArguments = @('--vector-borrow-integration', '--maximum-seconds', '3600')
                $OwnerMessage = ('Native owner language-1-memory-budget-split-execution ' +
                    'mode=vector-borrow-integration cases=594 expected-seconds=900 maximum-seconds=3600 ' +
                    'cold-duration-measured=false')
            } elseif ($Suite -eq 'language-1-memory-budget-split-execution' -and
                $Plan.Scope -eq 'development' -and
                ($NativePlan.UseFoundationBorrowPlanDevelopment -or
                    $NativePlan.UseFoundationBorrowDirectoryDevelopment -or
                    $NativePlan.UseFoundationBorrowOwnerDevelopment -or
                    $NativePlan.UseFoundationBorrowComponentsDevelopment)) {
                $OwnerExtension = if ($IsWindowsHost) { 'cmd' } else { 'sh' }
                $OwnerCommand = Join-Path $RepositoryRoot (
                    "Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.$OwnerExtension")
                $OwnerArguments = @('--foundation-borrow-plan')
                $OwnerMessage = 'Native owner language-1-memory-budget-split-execution mode=foundation-borrow-plan cases=32 expected-seconds=30'
                if ($NativePlan.UseFoundationBorrowDirectoryDevelopment) {
                    $OwnerArguments = @('--foundation-borrow-directories')
                    $OwnerMessage = 'Native owner language-1-memory-budget-split-execution mode=foundation-borrow-directories cases=27 expected-seconds=30'
                }
                if ($NativePlan.UseFoundationBorrowOwnerDevelopment) {
                    $OwnerCommand = 'node'
                    $FoundationOwnerDriverPath = Join-Path $RepositoryRoot `
                        'Tools/Native/Test-Language-1.0-Memory-Budget-Split-Execution.mjs'
                    $OwnerArguments = @($FoundationOwnerDriverPath, '--foundation-borrow-owners')
                    if ($UsePreparedProducts) { $OwnerArguments += @('--prepared-products-only', '--maximum-seconds', '600') }
                    $OwnerMessage = 'Native owner language-1-memory-budget-split-execution mode=foundation-borrow-owners cases=370 expected-seconds=180'
                }
                if ($NativePlan.UseFoundationBorrowComponentsDevelopment) {
                    $OwnerArguments = @('--foundation-borrow-components')
                    $OwnerMessage = 'Native owner language-1-memory-budget-split-execution mode=foundation-borrow-components cases=429 expected-seconds=180'
                }
            } elseif ($Suite -in @(
                    'generic-nominal-type-binding',
                    'generic-nominal-type-layout',
                    'generic-nominal-type-materialization') -and
                $Plan.Scope -eq 'development' -and
                $NativePlan.UseGenericNominalDevelopmentBundle) {
                $OwnerCommand = 'node'
                $BundleCommand = Join-Path $RepositoryRoot 'Tools/Native/Test-Generic-Nominal-Development-Bundle.mjs'
                $OwnerArguments = @($BundleCommand, 'all')
                $OwnerMessage = (
                    "Native owner $Suite mode=development-bundle " +
                    'bundle-cases=145 ' +
                    "selected-owners=$($NativePlan.GenericNominalDevelopmentBundleSelectedOwnerCount) " +
                    'expected-seconds=330')
            } elseif ($Suite -eq 'native-sha256-lowering' -and
                $Plan.Scope -eq 'development' -and
                $NativePlan.UseStreamingSha256Development) {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Native-Sha256.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Native-Sha256.sh'
                }
                $OwnerArguments = @('--streaming')
                $OwnerMessage = 'Native owner native-sha256-lowering mode=streaming-development cases=20 expected-seconds=30'
            } elseif ($Suite -eq 'source-containment' -and
                $Plan.Scope -eq 'development' -and
                $NativePlan.UseSourceContainmentCompilerDevelopment) {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Source-Containment.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Source-Containment.sh'
                }
                $OwnerArguments = @('--compiler-only')
                $OwnerMessage = 'Native owner source-containment mode=compiler-only'
            } elseif ($Suite -eq 'database-storage' -and
                $NativePlan.UseDatabaseStorageDevelopment) {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Database-Storage.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Database-Storage.sh'
                }
                $DatabaseTarget = $NativePlan.DatabaseStorageDevelopmentTarget
                $DatabaseCases = $NativePlan.DatabaseStorageDevelopmentCaseCount
                $DatabaseExecutions =
                    $NativePlan.DatabaseStorageDevelopmentExecutionCount
                $DatabaseBundles =
                    $NativePlan.DatabaseStorageDevelopmentBundleCount
                $DatabasePortableCases =
                    $NativePlan.DatabaseStorageDevelopmentPortableCaseCount
                $DatabaseHostedCases =
                    $NativePlan.DatabaseStorageDevelopmentHostedCaseCount
                $DatabaseExpectedSeconds =
                    $NativePlan.DatabaseStorageDevelopmentExpectedSeconds
                $OwnerArguments = @('--development-target-set', $DatabaseTarget)
                $OwnerMessage = (
                    'Native owner database-storage mode=development-checkpoint ' +
                    "target=$DatabaseTarget cases=$DatabaseCases " +
                    "executions=$DatabaseExecutions " +
                    "bundles=$DatabaseBundles " +
                    "portable-cases=$DatabasePortableCases " +
                    "hosted-cases=$DatabaseHostedCases " +
                    "expected-seconds=$DatabaseExpectedSeconds")
            } elseif ($Suite -eq 'libraries' -and
                $NativePlan.UseLibraryDevelopment) {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Libraries.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Libraries.sh'
                }
                $LibraryTarget = $NativePlan.LibraryDevelopmentTarget
                $OwnerArguments = @('--development-target', $LibraryTarget)
                $LibraryExpectedSeconds = if ($LibraryTarget -eq 'foundation-values') {
                    180
                } else {
                    300
                }
                $OwnerMessage = (
                    'Native owner libraries mode=development-target ' +
                    "target=$LibraryTarget expected-seconds=$LibraryExpectedSeconds")
            } elseif ($Suite -eq 'os-x64-code-emission' -and
                $NativePlan.UseOsX64CodeEmissionDevelopment) {
                $OwnerCommand = if ($IsWindowsHost) {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Os-X64-Code-Emission.cmd'
                } else {
                    Join-Path $RepositoryRoot 'Tools/Native/Test-Os-X64-Code-Emission.sh'
                }
                $OsX64Target = $NativePlan.OsX64CodeEmissionDevelopmentTarget
                if ($OsX64Target -eq 'all') {
                    $OwnerArguments = @('--development-all')
                    $OwnerMessage = (
                        'Native owner os-x64-code-emission ' +
                        'mode=development-checkpoint target=all')
                } else {
                    $OwnerArguments = @('--development-target', $OsX64Target)
                    $OwnerMessage = (
                        'Native owner os-x64-code-emission ' +
                        "mode=development-checkpoint target=$OsX64Target")
                }
            }

            if ($Suite -eq 'compiler-split-development' -and $Plan.Scope -eq 'development' -and
                $NativePlan.ConstructionReadinessRequired -and
                !($NativePlan.UseConstructionReadinessDevelopment -and !$NativePlan.AnalysisDiagnosticsRequired)) {
                $OwnerContinuation = [pscustomobject]@{
                    Command = 'node'
                    Arguments = @('--input-type=module', '-e', $ConstructionReadinessRunner,
                        (Join-Path $RepositoryRoot 'Tools/Native/Development-Command-Core.mjs'),
                        (Join-Path $RepositoryRoot 'Tools/Native/Test-Cached-Split-Project-Wvb.mjs'))
                    Message = 'Native owner compiler-split-development phase=construction-readiness readiness-cases=23 process-actions=13 package-deadline-assertions=12 expected-seconds=15 maximum-seconds=120 compiler-executions=0'
                }
            }
            $RelativeOwnerCommand = [IO.Path]::GetRelativePath(
                $RepositoryRoot,
                $OwnerCommand
            ).Replace('\', '/')
            $OwnerCacheArguments = @($OwnerArguments)
            if ($IsNativeSharedCompiler) {
                # The finite duration policy is part of the action. Its fresh
                # absolute start time must not make otherwise exact evidence miss.
                $DeadlineArgument = [Array]::IndexOf($OwnerCacheArguments, '--deadline-ms')
                $OwnerCacheArguments[$DeadlineArgument + 1] = '<absolute-owner-deadline>'
            }
            $OwnerActionModel = [ordered]@{
                format = 'windvale-verification-owner-action-2'
                suite = $Suite
                command = $RelativeOwnerCommand
                arguments = @($OwnerCacheArguments)
                scope = $Plan.Scope
                ownerContractSha256 = $OwnerContractHashes[$Suite]
            }
            if ($Suite -eq 'language-1-callable-semantics' -and $UsePreparedProducts) {
                $OwnerActionModel['preparedProductsOnly'] = $true
            }
            if ($IsNativeSharedCompiler) {
                $OwnerActionModel['preparedSharedCompiler'] = $NativeSharedCompilerIdentity
                $OwnerActionModel['preparedProductsOnly'] = $true
            }
            if ($null -ne $OwnerContinuation) {
                $OwnerActionModel['continuation'] = [ordered]@{
                    command = 'node'
                    arguments = @('--input-type=module', '-e', $ConstructionReadinessRunner,
                        'Tools/Native/Development-Command-Core.mjs', 'Tools/Native/Test-Cached-Split-Project-Wvb.mjs')
                    maximumSeconds = [long]120
                }
            }
            $OwnerAction = $OwnerActionModel | ConvertTo-Json -Compress

            $ResultCacheReused = $false
            if ($null -ne $ResultCacheState) {
                try {
                    $Probe = Invoke-VerificationResultCache -CacheArgument @(
                        'probe',
                        $ResultCacheState.root,
                        $ResultCacheState.stateKey,
                        $Suite,
                        $OwnerAction
                    )
                    if ($Probe -eq 'Hit') {
                        $Confirmation = Invoke-VerificationResultCache `
                            -CacheArgument @(
                                'confirm',
                                $RepositoryRoot,
                                $ResultCacheState.sourceSentinel
                            )
                        if ($Confirmation -eq 'Unchanged') {
                            $TimingStatus = 'reused'
                            Write-Host (
                                "PASS  native owner $Suite result=Reused " +
                                'source-state=Exact'
                            )
                            $ResultCacheReused = $true
                        } elseif ($Confirmation -eq 'Changed') {
                            Write-Warning (
                                'Repository inputs changed before exact result ' +
                                'reuse; the owner will run.')
                            $ResultCacheState = $null
                        } else {
                            throw "Unexpected source-state confirmation '$Confirmation'."
                        }
                    } elseif ($Probe -ne 'Miss') {
                        throw "Unexpected cache probe result '$Probe'."
                    } elseif (!$IsGenericNominalBundle) {
                        $CandidateRecord = (
                            Invoke-VerificationResultCache -CacheArgument @(
                                'candidates',
                                $ResultCacheState.root,
                                $ResultCacheState.stateKey,
                                $ResultCacheState.repositoryKey,
                                $ResultCacheState.hostKey,
                                $Suite,
                                $OwnerAction
                            )
                        ) | ConvertFrom-Json
                        $Candidates = @($CandidateRecord.candidates)
                        if ($CandidateRecord.format -ne
                                'windvale-verification-owner-candidates-1' -or
                            $Candidates.Count -gt 15) {
                            throw 'Verification result cache returned invalid candidates.'
                        }
                        foreach ($Candidate in $Candidates) {
                            if ($Candidate.stateKey -notmatch '^[0-9a-f]{64}$' -or
                                $Candidate.sourceTree -notmatch
                                    '^[0-9a-f]{40}(?:[0-9a-f]{24})?$') {
                                throw 'Verification result cache returned an invalid candidate.'
                            }
                            $Compatibility = $CompatibleResultPlanCache[$Candidate.sourceTree]
                            if ($null -eq $Compatibility) {
                                $ChangeRecord = (
                                    Invoke-VerificationResultCache -CacheArgument @(
                                        'changes',
                                        $RepositoryRoot,
                                        $Candidate.sourceTree,
                                        $ResultCacheState.sourceTree
                                    )
                                ) | ConvertFrom-Json
                                $CandidatePaths = @($ChangeRecord.paths)
                                if ($ChangeRecord.format -ne
                                        'windvale-verification-owner-changed-paths-1' -or
                                    $CandidatePaths.Count -gt 65536 -or
                                    @($CandidatePaths | Where-Object {
                                        $_ -isnot [string] -or
                                        [string]::IsNullOrWhiteSpace($_)
                                    }).Count -ne 0) {
                                    throw 'Verification compatibility paths are invalid.'
                                }
                                $Barrier = @($CandidatePaths | Where-Object {
                                    $CompatibleResultCacheBarrierPaths -ccontains $_
                                }).Count -ne 0
                                $DeltaPlan = if ($Barrier -or
                                    $CandidatePaths.Count -eq 0) {
                                    $null
                                } else {
                                    & $NativePlanner `
                                        -ChangedPath $CandidatePaths `
                                        -PassThru `
                                        -Quiet
                                }
                                $Compatibility = [pscustomobject]@{
                                    PathCount = $CandidatePaths.Count
                                    Barrier = $Barrier
                                    Plan = $DeltaPlan
                                }
                                $CompatibleResultPlanCache[$Candidate.sourceTree] =
                                    $Compatibility
                            }
                            if ($Compatibility.Barrier -or
                                $null -eq $Compatibility.Plan -or
                                @($Compatibility.Plan.Gaps).Count -ne 0 -or
                                @($Compatibility.Plan.Suites) -contains $Suite) {
                                continue
                            }
                            $CandidateProbe = Invoke-VerificationResultCache `
                                -CacheArgument @(
                                    'probe',
                                    $ResultCacheState.root,
                                    $Candidate.stateKey,
                                    $Suite,
                                    $OwnerAction
                                )
                            if ($CandidateProbe -ne 'Hit') {
                                continue
                            }
                            $Promotion = Invoke-VerificationResultCache `
                                -CacheArgument @(
                                    'publish',
                                    $RepositoryRoot,
                                    $ResultCacheState.root,
                                    $ResultCacheState.stateKey,
                                    $ResultCacheState.sourceTree,
                                    $ResultCacheState.sourceSentinel,
                                    $Suite,
                                    $OwnerAction
                                )
                            if ($Promotion -eq 'StateChanged') {
                                Write-Warning (
                                    'Repository inputs changed during compatible ' +
                                    'result reuse; the owner will run.')
                                $ResultCacheState = $null
                                break
                            }
                            if ($Promotion -ne 'Stored') {
                                throw "Unexpected compatible result publication '$Promotion'."
                            }
                            $TimingStatus = 'reused'
                            $ResultCacheReused = $true
                            Write-Host (
                                "PASS  native owner $Suite result=Reused " +
                                'source-state=Compatible ' +
                                "changed-paths=$($Compatibility.PathCount) " +
                                'from-state=' +
                                $Candidate.stateKey.Substring(0, 12)
                            )
                            break
                        }
                    }
                } catch {
                    Write-Warning (
                        "Verification result cache probe failed for '$Suite'; " +
                        'the owner will run: ' + $_.Exception.Message
                    )
                    $ResultCacheState = $null
                }
            }
            if ($ResultCacheReused) {
                if ($IsNativeSharedCompiler) {
                    $ConfirmedSharedCompiler = Get-NativeSharedCompilerIdentity $NativeSharedCompilerIdentity.recordPath `
                        $NativeSharedCompilerIdentity.recordSha256 $NativeSharedCompilerDeadline
                    if ((Get-NativeSharedCompilerSelectionText $ConfirmedSharedCompiler) -cne
                        (Get-NativeSharedCompilerSelectionText $NativeSharedCompilerIdentity)) {
                        throw 'Prepared shared compiler changed before cached evidence could be reused.'
                    }
                }
                if ($IsGenericNominalBundle) { $GenericNominalBundlePassed = $true }
                continue
            }

            if ($null -ne $OwnerMessage) {
                Write-Host $OwnerMessage
            }
            if ($Suite -eq 'database-storage' -and
                $NativePlan.UseDatabaseStorageDevelopment -and
                $NativePlan.DatabaseStorageDevelopmentExpectedSeconds -gt
                    $LOCAL_DEVELOPMENT_BUDGET_SECONDS -and
                !$AllowLongRun) {
                $BudgetMessage = (
                    'The focused database development plan selects ' +
                    "$($NativePlan.DatabaseStorageDevelopmentCaseCount) cases " +
                    "in $($NativePlan.DatabaseStorageDevelopmentExecutionCount) executions " +
                    'and expects ' +
                    "$($NativePlan.DatabaseStorageDevelopmentExpectedSeconds) " +
                    'seconds, which exceeds the ' +
                    "$LOCAL_DEVELOPMENT_BUDGET_SECONDS-second local budget. " +
                    'Inspect -PlanOnly, narrow the changed-path set, or pass ' +
                    '-AllowLongRun only for an approved named longer run.')
                Write-Warning $BudgetMessage
                throw $BudgetMessage
            }
            $VectorBorrowBudgetRefused = $Suite -eq 'language-1-memory-budget-split-execution' -and
                $Plan.Scope -eq 'development' -and $NativePlan.UseVectorBorrowIntegrationDevelopment -and
                $NativePlan.VectorBorrowIntegrationDevelopmentExpectedSeconds -gt
                    $LOCAL_DEVELOPMENT_BUDGET_SECONDS -and !$AllowLongRun
            if ($VectorBorrowBudgetRefused) {
                Write-Warning (
                    'Vector borrow integration was not executed: its 900-second planning cost class ' +
                    'exceeds the selected 600-second default; cold construction is not measured ' +
                    'and can exceed the automatic 15-minute CI budget. Inspect -PlanOnly and ' +
                    'select -AllowLongRun for the bounded 3600-second command under standing approval. ' +
                    'No cold product acquisition was started and no passing evidence was recorded.')
                $OwnerExitCode = 64
            } else {
                $PreparedProductEnvironment = $null
                if ($IsNativeSharedCompiler) {
                    $PreparedProductEnvironment = @{}
                    foreach ($Variable in @('WINDVALE_PREPARED_PRODUCTS_ONLY', 'WINDVALE_PREPARED_COMPILER_ONLY')) {
                        $PreparedProductEnvironment[$Variable] = [Environment]::GetEnvironmentVariable($Variable, 'Process')
                        [Environment]::SetEnvironmentVariable($Variable, '1', 'Process')
                    }
                }
                try {
                    if ($OwnerCommand -ceq $Coordinator) {
                        & pwsh -NoProfile -File $OwnerCommand @OwnerArguments
                    } else {
                        & $OwnerCommand @OwnerArguments
                    }
                    $OwnerExitCode = $LASTEXITCODE
                    if ($OwnerExitCode -eq 0 -and $null -ne $OwnerContinuation) {
                        Write-Host $OwnerContinuation.Message
                        $ContinuationCommand = $OwnerContinuation.Command
                        $ContinuationArguments = @($OwnerContinuation.Arguments)
                        & $ContinuationCommand @ContinuationArguments
                        $OwnerExitCode = $LASTEXITCODE
                    }
                } finally {
                    if ($null -ne $PreparedProductEnvironment) {
                        foreach ($Variable in $PreparedProductEnvironment.Keys) {
                            $Value = $PreparedProductEnvironment[$Variable]
                            if ($null -eq $Value) { $Value = [NullString]::Value }
                            [Environment]::SetEnvironmentVariable($Variable, $Value, 'Process')
                        }
                    }
                }
            }
            $OwnerSucceeded = $OwnerExitCode -eq 0
            if ($OwnerSucceeded -and $IsNativeSharedCompiler) {
                $ConfirmedSharedCompiler = Get-NativeSharedCompilerIdentity $NativeSharedCompilerIdentity.recordPath `
                    $NativeSharedCompilerIdentity.recordSha256 $NativeSharedCompilerDeadline
                if ((Get-NativeSharedCompilerSelectionText $ConfirmedSharedCompiler) -cne
                    (Get-NativeSharedCompilerSelectionText $NativeSharedCompilerIdentity)) {
                    throw 'Prepared shared compiler changed during owner execution.'
                }
            }
            if ($OwnerSucceeded -and $IsGenericNominalBundle) {
                $BundleConfirmation = Invoke-VerificationResultCache -CacheArgument @(
                    'confirm', $RepositoryRoot, $GenericNominalBundleSentinel)
                if ($BundleConfirmation -ne 'Unchanged') {
                    throw 'Repository inputs changed during the generic nominal bundle.'
                }
                $GenericNominalBundlePassed = $true
            }
            if (!$OwnerSucceeded -and
                ($OwnerCommand -ceq $Coordinator -or $Suite -eq 'language-1-front-door' -or
                    ($Suite -eq 'compiler-split-development' -and
                        ($NativePlan.AnalysisDiagnosticsRequired -or $NativePlan.ConstructionReadinessRequired)) -or
                    $IsNativeSharedCompiler -or
                    ($UsePreparedProducts -and $Suite -in @('language-1-callable-semantics', 'language-1-authenticated-foreign-binding', 'native-x64-lowering-development')) -or
                    ($Suite -eq 'language-1-memory-budget-split-execution' -and
                        $NativePlan.UseVectorBorrowIntegrationDevelopment)) -and
                $OwnerExitCode -ne 1) {
                $TimingOutcome = if ($OwnerExitCode -eq 124) {
                    'timed-out'
                } else {
                    'framework-error'
                }
                $Incomplete.Add($Suite)
                Write-Warning (
                    "Native owner '$Suite' is verification-incomplete " +
                    "outcome=$TimingOutcome exit=$OwnerExitCode. " +
                    'No passing evidence was recorded.')
                if (!$AllowIncompleteInfrastructure -or
                    $IsNativeSharedCompiler -or
                    ($UsePreparedProducts -and $Suite -in @('language-1-callable-semantics', 'language-1-authenticated-foreign-binding', 'native-x64-lowering-development', 'language-1-front-door'))) {
                    $StopAfterOwner = $true
                }
            } elseif (!$OwnerSucceeded) {
                $TimingOutcome = 'test-failed'
                throw "Native owner '$Suite' exited $OwnerExitCode."
            }
            if ($OwnerSucceeded -and $null -ne $ResultCacheState) {
                try {
                    $Publish = Invoke-VerificationResultCache -CacheArgument @(
                        'publish',
                        $RepositoryRoot,
                        $ResultCacheState.root,
                        $ResultCacheState.stateKey,
                        $ResultCacheState.sourceTree,
                        $ResultCacheState.sourceSentinel,
                        $Suite,
                        $OwnerAction
                    )
                    if ($Publish -eq 'StateChanged') {
                        Write-Warning (
                            'Repository inputs changed during verification; ' +
                            'new passes will not be cached in this run.'
                        )
                        $ResultCacheState = $null
                    } elseif ($Publish -ne 'Stored') {
                        throw "Unexpected cache publication result '$Publish'."
                    }
                } catch {
                    Write-Warning (
                        "Verification result cache publication failed for '$Suite': " +
                        $_.Exception.Message
                    )
                }
            }
        } catch {
            if ($TimingOutcome -eq 'passed') {
                $TimingOutcome = 'framework-error'
            }
            $Failures.Add($Suite)
            if (!$NoFailFast) { $StopAfterOwner = $true }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = $Suite
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
                status = $TimingStatus
                outcome = $TimingOutcome
                exitCode = $OwnerExitCode
            })
        }
        if ($StopAfterOwner) { break }
    }

    if ($NativePlan.RunWebAssemblyEngineVerification -and $NativeDevelopmentShard -le 1) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        try {
            & $WebAssemblyEngineVerifier
        } catch {
            $Failures.Add('webassembly-engine')
            if (!$NoFailFast) { throw }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = 'webassembly-engine'
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
            })
        }
    }

    if ($NativePlan.RunWebAssemblyVerification -and $NativeDevelopmentShard -le 1) {
        $Stopwatch = [Diagnostics.Stopwatch]::StartNew()
        try {
            & $WebAssemblyVerifier
        } catch {
            $Failures.Add('webassembly')
            if (!$NoFailFast) { throw }
        } finally {
            $Stopwatch.Stop()
            $Timings.Add([pscustomobject]@{
                name = 'webassembly'
                elapsedMilliseconds = $Stopwatch.ElapsedMilliseconds
            })
        }
    }

    if (![string]::IsNullOrWhiteSpace($TimingReportPath)) {
        $TimingParent = Split-Path -Parent $TimingReportPath
        if (![string]::IsNullOrWhiteSpace($TimingParent) -and
            !(Test-Path -LiteralPath $TimingParent -PathType Container)) {
            throw 'The native changed-file timing-report parent does not exist.'
        }
        $OverallOutcome = if ($Failures.Count -ne 0) {
            'failed'
        } elseif ($Incomplete.Count -ne 0) {
            'verification-incomplete'
        } else {
            'passed'
        }
        [pscustomobject]@{
            format = 'windvale-native-changed-verification-timing-2'
            host = Get-VerificationHostName
            startedUtc = $VerificationStartedUtc.ToString('O')
            outcome = $OverallOutcome
            incompleteOwners = @($Incomplete)
            entries = @($Timings)
        } | ConvertTo-Json -Depth 4 |
            Set-Content -LiteralPath $TimingReportPath -Encoding utf8
    }
    if ($Failures.Count -ne 0) {
        throw "Native changed-file verification failed: $($Failures -join ', ')."
    }
    if ($Incomplete.Count -ne 0) {
        $Message = (
            'Native changed-file verification is incomplete: ' +
            ($Incomplete -join ', ') + '.')
        if (!$AllowIncompleteInfrastructure -or
            ($UsePreparedProducts -and @($Incomplete | Where-Object {
                $_ -in @('language-1-authenticated-foreign-binding',
                    'native-x64-lowering-development', 'language-1-front-door')
            }).Count -ne 0)) {
            throw $Message
        }
        Write-Warning "$Message Automatic development feedback remains nonblocking."
    }
} else {
    Write-Host 'Changed-file verification passed without native owner execution.'
}
