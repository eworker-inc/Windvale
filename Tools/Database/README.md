# Windvale database tools

`Measure-Database-Comparison.ps1` records a bounded local latency comparison
for one 16-byte durable put followed by a separate restart read. One-time
database/schema setup and per-sample reset are excluded. Every engine uses a
new client process for the put and another for the read.

The comparison deliberately reports PostgreSQL as an already-running server,
while Windvale and SQLite use copied base files. It is useful cold-client
latency, storage-size, and client-memory evidence. It is not a throughput,
concurrency, SQL-feature, or server-feature equivalence claim.
The JSON report records exact Windvale executable hashes and the SQLite,
Python, and PostgreSQL client versions used for reproducibility.

PostgreSQL authentication comes only from its normal `PGPASSWORD` or password
file handling. The script never accepts, prints, or stores a password. Use
`-SkipPostgres` when no safe local authentication profile is available.

Example:

```powershell
pwsh -NoProfile -File Tools/Database/Measure-Database-Comparison.ps1 `
    -WindvaleStorageApplication <storage.exe> `
    -WindvalePutGetApplication <root-writer.exe> `
    -PythonPath <python.exe> `
    -Iterations 30 `
    -OutputJson <report.json>
```

The Python helper uses only the standard `sqlite3` module and sets
`journal_mode=DELETE` plus `synchronous=FULL` before schema or mutation work.
