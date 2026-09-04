# Decision 0939: raise native x64 static-data capacity for compiler convergence

## Status

Accepted implementation checkpoint on 2026-09-03. The native x64 lowerer and
segmented WVO reader admit at most 512 immutable data declarations. Final
artifact reconstruction and paired-host qualification remain required.

## Context

The compiler checkpoint that added canonical Foundation `Option` and `Result`
predicates produced a structurally valid 1,656,229-byte stage-1 analyzer WVB
with 259 UTF-8 static-data entries. The native x64 lowerer rejected that module
as `Unsupportedˉmodule` detail `104` because its older compiler-scale profile
allowed only 256 entries. The source compiler's general WVB contract permits
4,096 entries, but native compilation deliberately retains a smaller bounded
profile.

The rejection prevented the current source compiler from reconstructing the
WVB runner even though every emitted data record was valid and the complete
data payload ended at its declared section boundary.

## Decision

1. Raise the native x64 static-data limit from 256 to 512 declarations.
2. Retain the existing per-item limits: text is valid UTF-8 of at most 1 MiB,
   bytes is at most 4 MiB, and an `[i32]` value contains at most 262,144
   elements. The enclosing WVB and object limits continue to bound aggregate
   allocation and output.
3. Keep canonical `$data_NNNN` naming. The four-digit representation already
   covers every admitted ordinal, so existing object bytes do not change only
   because the bound changed.
4. Raise the segmented WVO symbol reader to the same 512-data limit and to
   1,537 total symbols: 512 data symbols, 1,024 functions, and at most one
   private helper.
5. Add a focused data-reader case that admits a complete 512-entry payload and
   rejects count 513 before reading or allocating entry state.
6. Treat successful current-compiler reconstruction as the integration proof
   for the observed 259-entry module. Do not weaken or remove the source
   predicates to stay under an obsolete backend limit.

## Consequences

- The native backend remains explicitly bounded while regaining headroom for
  current compiler and library growth.
- Existing modules below the old limit preserve their canonical data-symbol
  names and object layout.
- The lowerer and segmented staging-producer artifacts must be reconstructed
  and promoted because their validation code changes.
- The refreshed lowerer publication is the 747,242-byte WVB at SHA-256
  `83dd8baeea28baf73a6b5343602cddc89af0328cb6b212a4e9d613b513cd8ee1`,
  the 10,656,768-byte Windows wrapper at SHA-256
  `cc20f3b4c411eb4dd4933cead629729cc5b62dd79f3313dfbc4385f75868edeb`,
  and the 10,657,792-byte Linux wrapper at SHA-256
  `8531d1873882ced250d02a4ca1997e9b2ec89db72f91b7c6ed8b3d6a71045d9c`.
- The refreshed staging-producer publication is the 774,524-byte WVB at
  SHA-256
  `13fbd0cfe71dc4bdc25346398a0e9edd23414d55d6922515abdaed8645b577c8`,
  the 11,184,128-byte Windows wrapper at SHA-256
  `624e22913a45b642053a3ac14ed12f5d50b59b7c23c485310afb1c02f154a07e`,
  and the 11,186,176-byte Linux wrapper at SHA-256
  `aa14c945c7e32b5bde6eb29ee41f84ef45231d11328d9fe3eb9c14b373b56ca6`.
  Its staged object is 11,192,888 bytes in 16 chunks; its canonical image is
  11,164,872 bytes at entry offset 23,873 and transports in three fragments.
- The same current-source reconstruction refreshed the compiler-image staging
  family transitively: the 81,530-byte WVB is SHA-256
  `03a928f036a188fc943d3d197d45114cbb327d5edffae62ee3cc842186267bbc`,
  the 931,840-byte Windows wrapper is SHA-256
  `cc94fba08e6f4a5b20a0ddfc509f40f9fe8e801375d5e97320aec01f9f9f1b5b`,
  and the 933,888-byte Linux wrapper is SHA-256
  `bdbea8e2e8c8eb48211be5068bd93b5f4011814bc2ef15acffb5fdee622ac58d`.
  The canonical-transport family remains byte-identical.
- A future need above 512 requires new measurement and a deliberate bound
  review; this decision does not silently inherit the source compiler's 4,096
  entry maximum.

## Reconsideration triggers

Revisit this decision before admitting more than 512 data entries, changing
symbol-name width, raising aggregate WVB or WVO limits, or allowing data
records to bypass the current per-item validation and exact-section-consumption
rules.
