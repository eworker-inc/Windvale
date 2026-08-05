# Decision 0238: Bounded native enum lowering

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0237](0237-Bounded-Native-Bytes-Concatenation.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After Decision 0237 completed the compiler-produced `Data-And-Text.wv` lowering surface, `Nominal-Types.wv` became the next concrete backend-transfer fixture. That program combines two enums, two records, record construction and fields, record parameters and returns, enum locals and constants, enum comparisons, and `Enumˉname`. Transferring all of those contracts together would mix type-table admission, scalar-like enum behavior, direct record storage, descriptor-bearing records, and nominal calls in one change.

Enum values already have a narrow ABI 22 representation: one 32-bit backing value in a normal 16-byte cell. They therefore form the first coherent nominal slice. The current Stage 0 native selector and independent fragment verifier additionally require the first member of every admitted enum to have backing value zero. Preserving that accepted boundary keeps Stage 0 usable as the exact independent WVO oracle; removing it is a separate bounded correction rather than an accidental expansion here.

## Decision

### Parse one bounded nominal-type table

Admit zero through eight canonical record or enum declarations. Validate section bounds, declared names, canonical record-before-enum ordering, one through 64 record fields, one through 256 enum members, admitted primitive or enum record-field shapes, and the Stage 0 zero-valued first-enum-member condition. Record metadata is parsed so enum references have stable type indices, but record values and record instructions remain unsupported in this slice.

Represent an admitted enum's analysis identity internally as one byte derived from its nominal type index. This is private lowerer state, not a new WVB or ABI encoding. Before code emission the type maps back to ABI 22's enum runtime group while equality still requires the exact same nominal identity.

### Admit generic enum operations

Admit enum-typed declared locals plus `enum.const`, `enum.equal`, `enum.not_equal`, and `enum.name`. Constants store the declared member's signed 32-bit backing value. Comparisons consume two values of the same nominal enum and produce `bool`. `enum.name` passes the nominal type index and backing value to the existing ABI 22 runtime-service-table entry and produces `text`, branching to the existing runtime-service failure tail on rejection.

Keep type-table walking, low-level enum machine-byte emission, and stack/slot instruction orchestration in the focused `Native-X64-Lowering-Types.wv`, `Native-X64-Lowering-Enums.wv`, and `Native-X64-Lowering-Enum-Instructions.wv` modules. The instruction-state extraction keeps the already-large control emitter below Stage 0's fixed 2,048-physical-slot compilation limit without raising that safety bound.

### Require focused differential evidence

Add `Wvb-To-Wvo-Enums.wv`, containing one three-member enum, enum local store/load, both equality directions, `Enumˉname`, UTF-8 conversion, and a byte read. It returns 42 through the reference interpreter and Stage 0 native backend. The Windvale memory adapter and hosted tool must reproduce Stage 0's complete WVO byte for byte. A mutated out-of-range enum type index must be rejected before publication.

The affected shared-backend and standalone WVB-to-WVO package tests are the local evidence for this slice. The former compares interpreter, JIT, memory-lowerer, hosted-lowerer, and exact WVO results; the latter rebuilds and executes the deterministic Windows/Linux package candidates. Local Standard, Qualification, GitHub verification, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- Enum type identity, constants, locals, comparisons, and name lookup now have reusable Windvale-owned accepted-subset x86-64 lowering.
- The compiler-produced `Nominal-Types.wv` fixture is not yet admitted: it still requires nonzero-first enum baselines, record construction and fields, descriptor-bearing record storage, and record parameters and returns.
- `Enumˉname` continues to require verified nominal metadata through ABI 22. WVO 1.0 does not serialize the fragment's required-service and nominal-type metadata, so a service-bearing WVO is not independently loadable from the object alone; this existing packaging boundary remains open.
- The current hosted tool is 191,086 WVB bytes and lowers through Stage 0 to 2,617,332 code bytes and a 2,624,840-byte WVO. Current paired package measurements are 2,635,776 bytes on Windows and 2,637,824 bytes on Linux. These are unpromoted candidate measurements, not optimization promises.
- No normal .NET dependency is removed by this local proof, and no production C# backend code changes.

## Reconsideration triggers

Address the nonzero-first enum constraint together with the record and nominal-call work required by the real `Nominal-Types.wv` fixture. Do not broaden unrelated nominal payload variants, general heap records, descriptor calls, or capability transport without a concrete remaining fixture and an explicit ownership boundary.
