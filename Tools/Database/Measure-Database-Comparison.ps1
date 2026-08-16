[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$WindvaleStorageApplication,
    [Parameter(Mandatory)]
    [string]$WindvalePutGetApplication,
    [Parameter(Mandatory)]
    [string]$PythonPath,
    [ValidateRange(3, 1000)]
    [int]$Iterations = 30,
    [ValidateRange(0, 100)]
    [int]$Warmups = 5,
    [string]$PostgresBin = 'C:\Program Files\PostgreSQL\18\bin',
    [string]$PostgresHost = 'localhost',
    [ValidateRange(1, 65535)]
    [int]$PostgresPort = 5432,
    [string]$PostgresUser = 'postgres',
    [string]$PostgresDatabase = 'postgres',
    [switch]$SkipPostgres,
    [string]$OutputJson
)

$ErrorActionPreference = 'Stop'
$RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$SqliteTool = Join-Path $PSScriptRoot 'SQLite-Durable-Cycle.py'
$PayloadBytes = 16

function Resolve-OrdinaryFile([string]$Path, [string]$Label) {
    $Resolved = (Resolve-Path -LiteralPath $Path).Path
    $Item = Get-Item -LiteralPath $Resolved -Force
    if (!$Item.PSIsContainer -and !$Item.LinkType) { return $Resolved }
    throw "$Label must be an ordinary non-link file: $Path"
}

function Invoke-MeasuredProcess(
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
    if (!$Process.Start()) { throw "Could not start $Application." }
    $OutputTask = $Process.StandardOutput.ReadToEndAsync()
    $ErrorTask = $Process.StandardError.ReadToEndAsync()
    $Peak = 0L
    while (!$Process.WaitForExit(1)) {
        try {
            $Process.Refresh()
            $Peak = [Math]::Max($Peak, $Process.WorkingSet64)
        } catch { }
    }
    $Output = $OutputTask.GetAwaiter().GetResult()
    $ErrorOutput = $ErrorTask.GetAwaiter().GetResult()
    try { $Peak = [Math]::Max($Peak, $Process.PeakWorkingSet64) } catch { }
    if ($Process.ExitCode -ne 0) {
        throw "$Application exited $($Process.ExitCode): $($ErrorOutput.Trim())"
    }
    return [pscustomobject]@{ Peak = $Peak; Output = $Output }
}

function Get-Percentile([double[]]$Values, [double]$Fraction) {
    $Sorted = @($Values | Sort-Object)
    $Index = [Math]::Ceiling($Fraction * $Sorted.Count) - 1
    if ($Index -lt 0) { $Index = 0 }
    return $Sorted[$Index]
}

function Measure-Case(
    [string]$Name,
    [object]$Engine,
    [scriptblock]$Prepare,
    [scriptblock]$Run,
    [scriptblock]$Size
) {
    $Times = [System.Collections.Generic.List[double]]::new()
    $Peaks = [System.Collections.Generic.List[long]]::new()
    $Total = $Warmups + $Iterations
    for ($Index = 0; $Index -lt $Total; $Index += 1) {
        & $Prepare $Index
        $Watch = [Diagnostics.Stopwatch]::StartNew()
        $Peak = & $Run $Index
        $Watch.Stop()
        if ($Index -ge $Warmups) {
            $Times.Add($Watch.Elapsed.TotalMilliseconds)
            $Peaks.Add($Peak)
        }
    }
    $Bytes = & $Size
    return [ordered]@{
        name = $Name
        engine = $Engine
        workload = 'cold-client-durable-put-restart-read'
        iterations = $Iterations
        payload_bytes = $PayloadBytes
        latency_ms = [ordered]@{
            minimum = [Math]::Round(($Times | Measure-Object -Minimum).Minimum, 3)
            mean = [Math]::Round(($Times | Measure-Object -Average).Average, 3)
            p50 = [Math]::Round((Get-Percentile $Times.ToArray() 0.50), 3)
            p95 = [Math]::Round((Get-Percentile $Times.ToArray() 0.95), 3)
            maximum = [Math]::Round(($Times | Measure-Object -Maximum).Maximum, 3)
        }
        peak_client_working_set_bytes = [long](($Peaks | Measure-Object -Maximum).Maximum)
        storage_bytes = $Bytes
    }
}

$WindvaleStorageApplication = Resolve-OrdinaryFile $WindvaleStorageApplication 'Windvale storage application'
$WindvalePutGetApplication = Resolve-OrdinaryFile $WindvalePutGetApplication 'Windvale put/get application'
$PythonPath = Resolve-OrdinaryFile $PythonPath 'Python runtime'
$SqliteTool = Resolve-OrdinaryFile $SqliteTool 'SQLite benchmark tool'

$BenchmarkRoot = Join-Path ([IO.Path]::GetTempPath()) (
    'windvale-database-comparison-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $BenchmarkRoot | Out-Null
$ResolvedBenchmarkRoot = (Resolve-Path -LiteralPath $BenchmarkRoot).Path
if (!$ResolvedBenchmarkRoot.StartsWith(
        [IO.Path]::GetFullPath([IO.Path]::GetTempPath()),
        [StringComparison]::OrdinalIgnoreCase)) {
    throw 'The benchmark root escaped the temporary directory.'
}

$Results = [System.Collections.Generic.List[object]]::new()
$PostgresTable = 'windvale_benchmark_' + $PID
try {
    $WindvaleEngine = [ordered]@{
        storage_application_sha256 = (Get-FileHash -LiteralPath $WindvaleStorageApplication -Algorithm SHA256).Hash.ToLowerInvariant()
        put_get_application_sha256 = (Get-FileHash -LiteralPath $WindvalePutGetApplication -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    $PythonVersion = (Invoke-MeasuredProcess $PythonPath @('--version') $BenchmarkRoot).Output.Trim()
    $SqliteVersion = (Invoke-MeasuredProcess $PythonPath @($SqliteTool, 'version') $BenchmarkRoot).Output.Trim()
    $SqliteEngine = [ordered]@{
        sqlite_version = $SqliteVersion
        client_runtime = $PythonVersion
    }
    $WindvaleBase = Join-Path $BenchmarkRoot 'Windvale-Base'
    New-Item -ItemType Directory -Path $WindvaleBase | Out-Null
    $null = Invoke-MeasuredProcess $WindvaleStorageApplication @() $WindvaleBase
    $WindvaleBaseFile = Join-Path $WindvaleBase 'Windvale-Database-Storage.bin'
    if ((Get-Item -LiteralPath $WindvaleBaseFile).Length -ne 4608) {
        throw 'Windvale did not create the canonical 4,608-byte base database.'
    }
    $WindvaleRun = Join-Path $BenchmarkRoot 'Windvale-Run'
    New-Item -ItemType Directory -Path $WindvaleRun | Out-Null
    $WindvaleFile = Join-Path $WindvaleRun 'Windvale-Database-Storage.bin'
    $Results.Add((Measure-Case 'Windvale' $WindvaleEngine {
        param($Index)
        Copy-Item -LiteralPath $WindvaleBaseFile -Destination $WindvaleFile -Force
    } {
        param($Index)
        $Put = Invoke-MeasuredProcess $WindvalePutGetApplication @() $WindvaleRun
        $CommittedHash = (Get-FileHash -LiteralPath $WindvaleFile -Algorithm SHA256).Hash
        $Get = Invoke-MeasuredProcess $WindvalePutGetApplication @() $WindvaleRun
        if ($CommittedHash -ne (Get-FileHash -LiteralPath $WindvaleFile -Algorithm SHA256).Hash) {
            throw 'Windvale restart read changed the database file.'
        }
        return [Math]::Max($Put.Peak, $Get.Peak)
    } { (Get-Item -LiteralPath $WindvaleFile).Length }))

    $SqliteBase = Join-Path $BenchmarkRoot 'SQLite-Base.db'
    $null = Invoke-MeasuredProcess $PythonPath @($SqliteTool, 'initialize', $SqliteBase) $BenchmarkRoot
    $SqliteFile = Join-Path $BenchmarkRoot 'SQLite-Run.db'
    $Results.Add((Measure-Case 'SQLite' $SqliteEngine {
        param($Index)
        Copy-Item -LiteralPath $SqliteBase -Destination $SqliteFile -Force
    } {
        param($Index)
        $Put = Invoke-MeasuredProcess $PythonPath @($SqliteTool, 'put', $SqliteFile) $BenchmarkRoot
        $CommittedHash = (Get-FileHash -LiteralPath $SqliteFile -Algorithm SHA256).Hash
        $Get = Invoke-MeasuredProcess $PythonPath @($SqliteTool, 'get', $SqliteFile) $BenchmarkRoot
        if ($CommittedHash -ne (Get-FileHash -LiteralPath $SqliteFile -Algorithm SHA256).Hash) {
            throw 'SQLite restart read changed the database file.'
        }
        return [Math]::Max($Put.Peak, $Get.Peak)
    } { (Get-Item -LiteralPath $SqliteFile).Length }))

    if (!$SkipPostgres) {
        $Psql = Resolve-OrdinaryFile (Join-Path $PostgresBin 'psql.exe') 'psql'
        $PostgresEngine = [ordered]@{
            client_version = (Invoke-MeasuredProcess $Psql @('--version') $BenchmarkRoot).Output.Trim()
            connection = "$PostgresHost`:$PostgresPort/$PostgresDatabase"
        }
        $Connection = @('-X', '-w', '-h', $PostgresHost, '-p', "$PostgresPort", '-U', $PostgresUser, '-d', $PostgresDatabase, '-v', 'ON_ERROR_STOP=1', '-Atq')
        $CreateSql = "DROP TABLE IF EXISTS $PostgresTable; CREATE TABLE $PostgresTable (Identity text PRIMARY KEY, Payload bytea NOT NULL);"
        $null = Invoke-MeasuredProcess $Psql ($Connection + @('-c', $CreateSql)) $BenchmarkRoot
        $Results.Add((Measure-Case 'PostgreSQL' $PostgresEngine {
            param($Index)
            $null = Invoke-MeasuredProcess $Psql ($Connection + @('-c', "TRUNCATE $PostgresTable;")) $BenchmarkRoot
        } {
            param($Index)
            $PutSql = "BEGIN; INSERT INTO $PostgresTable VALUES ('first-record', decode('73757276697665732d72657374617274', 'hex')); COMMIT;"
            $Put = Invoke-MeasuredProcess $Psql ($Connection + @('-c', $PutSql)) $BenchmarkRoot
            $GetSql = "SELECT CASE WHEN Payload = decode('73757276697665732d72657374617274', 'hex') THEN 1 ELSE 0 END FROM $PostgresTable WHERE Identity = 'first-record';"
            $Get = Invoke-MeasuredProcess $Psql ($Connection + @('-c', $GetSql)) $BenchmarkRoot
            if ($Get.Output.Trim() -ne '1') { throw 'PostgreSQL restart read returned the wrong payload.' }
            return [Math]::Max($Put.Peak, $Get.Peak)
        } { $null }))
    }

    $Report = [ordered]@{
        format = 'windvale-database-comparison-1'
        generated_utc = [DateTime]::UtcNow.ToString('O')
        host = [ordered]@{
            operating_system = [Environment]::OSVersion.VersionString
            processors = [Environment]::ProcessorCount
        }
        notes = @(
            'One-time database and schema creation is excluded.',
            'Each sample includes a new client/process for put and another for restart read.',
            'Windvale and SQLite copy a base file before each unmeasured sample setup.',
            'PostgreSQL is an already-running server; TRUNCATE occurs before each measured sample.',
            'This is latency evidence, not a throughput or feature-equivalence claim.'
        )
        results = $Results
    }
    $Json = $Report | ConvertTo-Json -Depth 8
    if ($OutputJson) {
        $OutputPath = [IO.Path]::GetFullPath($OutputJson)
        [IO.File]::WriteAllText($OutputPath, $Json + "`n", [Text.UTF8Encoding]::new($false))
    }
    $Json
} finally {
    if (!$SkipPostgres -and (Test-Path Variable:Psql)) {
        try {
            $null = Invoke-MeasuredProcess $Psql ($Connection + @('-c', "DROP TABLE IF EXISTS $PostgresTable;")) $BenchmarkRoot
        } catch { Write-Warning "Could not remove PostgreSQL benchmark table $PostgresTable." }
    }
    if (Test-Path -LiteralPath $ResolvedBenchmarkRoot) {
        Remove-Item -LiteralPath $ResolvedBenchmarkRoot -Recurse -Force
    }
}
