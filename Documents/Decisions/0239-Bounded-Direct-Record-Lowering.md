# Decision 0239: Bounded direct record lowering

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0238](0238-Bounded-Native-Enum-Lowering.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0238 admitted nominal metadata and the enum operations required by compiler-produced `Nominal-Types.wv`. The same fixture also constructs records, stores and reloads record locals, reads fields, passes records to functions, and returns a record. Transferring direct record storage separately from nominal calls keeps frame ownership, value copying, call transport, and the remaining nonzero-first enum constraint independently reviewable.

ABI 22 represents a record value as a handle to caller-visible frame-owned field storage. The accepted lowerer therefore needs deterministic backing storage for each record value; treating the handle as an ordinary scalar cell would alias temporary values or lose descriptor fields. The implementation also must remain compilable by the qualified Stage 0 backend without raising its fixed 2,048-physical-cell function limit.

## Decision

### Admit bounded direct record values

Decode the canonical WVB record shape at nominal tag 7 and retain its exact nominal identity in private analysis state. Admit record declarations with one through 64 primitive, descriptor, or enum fields; nested record fields remain rejected. A record-bearing function has exactly one basic block and uses only `i32.const`, `local.load`, `local.store`, checked `i32.add`, `record.create`, `record.field`, and `return`. It has at most 128 declared locals and produces at most 128 record values. Record parameters, record returns, and calls that transport records remain outside this slice.

### Plan frame-owned storage deterministically

Reserve persistent field cells for record locals and scratch field cells for record construction and record loads. A bounded storage pass records record-value lifetimes, builds separate persistent and scratch interference sets, and assigns field ranges by descending record width with deterministic first-fit placement. Every field occupies one complete 16-byte ABI cell. Construction and local movement copy complete field cells, so text and bytes descriptors preserve both address and length without depending on host memory layout.

The ordinary record value cell holds a handle to its assigned field range. `record.field` validates the nominal record identity and field index during analysis, then emits a complete field-cell load into the selected result group. The focused `Native-X64-Lowering-Record-Storage.wv`, `Native-X64-Lowering-Records.wv`, and `Native-X64-Lowering-Record-Instructions.wv` modules own planning, machine-byte templates, and instruction-state transitions.

Extract static-data instruction emission into `Native-X64-Lowering-Static-Data-Instructions.wv`. This cohesive extraction brings the already-large core emission function back below Stage 0's fixed 2,048-physical-cell compilation boundary; the safety limit is not raised.

### Require focused differential evidence

Add `Wvb-To-Wvo-Records.wv`, which constructs a two-field `Pair`, stores it in a local, reloads it, reads both fields, and returns 42. It must return 42 through the reference interpreter and Stage 0 native backend. The Windvale memory adapter and hosted tool must reproduce Stage 0's complete WVO byte for byte. A mutated out-of-range record type index must be rejected before publication.

The affected shared-backend and standalone WVB-to-WVO package tests are the local evidence for this slice. Local Standard, Qualification, GitHub verification, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- Direct construction, local storage, local copying, and field reads now have reusable Windvale-owned accepted-subset x86-64 lowering.
- Compiler-produced `Nominal-Types.wv` is not yet admitted: it still requires record parameters and returns, calls transporting record values, and removal of the nonzero-first enum restriction.
- The current hosted tool is 235,007 WVB bytes and lowers through Stage 0 to 3,375,924 code bytes and a 3,385,336-byte WVO. Current paired package measurements are 3,394,048 bytes on Windows and 3,395,584 bytes on Linux. These are unpromoted candidate measurements, not optimization promises.
- No normal .NET dependency is removed by this local proof, and no production C# backend code changes.

## Reconsideration triggers

Extend the same frame-owned representation across record parameters, caller-owned record returns, and nominal calls only as required by the real `Nominal-Types.wv` fixture. Remove the zero-valued-first-enum-member constraint in that bounded transfer. Do not broaden nested records, multi-block record liveness, heap records, payload variants, or unrelated capability transport without a concrete remaining fixture and an explicit ownership boundary.
