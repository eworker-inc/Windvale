# Decision 0242: First hosted capability in native lowering

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0241](0241-Multi-Block-Native-Record-Liveness.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0241 completes the nominal behavior required by the current real portable fixtures. The accepted Windvale lowerer still rejects every hosted WVB, so it cannot replace Stage 0 for tools whose code obtains process arguments, reads or writes files, or reports diagnostics. Moving that boundary one exact capability at a time keeps capability admission, authorization, ABI placement, and generated machine bytes reviewable.

`process.argument_count() -> u32` is the smallest real hosted boundary. It has no descriptor argument or dynamic result, already has an ABI 22 service-table leaf, and lets a native executable prove an actual service call without introducing file mutation or uncertain completion behavior.

## Decision

### Validate the bounded hosted capability table

Accept portable WVB only with an empty capability table and hosted WVB only with one through six declarations from the exact Stage 0 native-service subset: `console.write_line`, `diagnostic.write_line`, `file.read_bytes`, `file.write_bytes`, `process.argument`, and `process.argument_count`. Require canonical order, unique names, and exact parameter and return types. Unknown names, duplicates, reordered declarations, malformed ranges, and signature drift fail as `Unsupportedˉmodule`.

Keep this parser in focused `Native-X64-Lowering-Capabilities.wv` rather than enlarging the already-large instruction core. Only `process.argument_count` may be called in this slice; the other validated declarations prepare deterministic admission for later item-by-item transfers but remain rejected when named by `call.capability`.

### Emit the exact ABI 22 scalar service call

Admit opcode 65 only when its index resolves to `process.argument_count() -> u32`. Load the service table from the existing `R15` execution context, call the process-argument-count slot at byte offset 16, and store `EAX` in the next `u32` frame cell. Charge the ordinary ten-byte instruction-meter sequence plus the exact seventeen operation bytes, for twenty-seven planned bytes. The emitted object retains the existing ABI and status propagation rules.

WVO 1.0 does not serialize required-service metadata. This candidate therefore remains safe only inside the existing verified-fragment and hosted-package boundary, where the capability declaration and bound service table remain available. General independently loadable hosted WVO is not claimed.

### Require focused differential and native-source evidence

Add `Wvb-To-Wvo-Process-Argument-Count.wv`, which declares only `process.argument_count`, returns 42 for an empty argument vector, and returns 1 otherwise. Require agreement across the reference interpreter, Stage 0 native backend, Windvale memory adapter, hosted Windvale lowerer, and direct current-host native WVB-to-WVO package. Mutated capability indices and signatures must fail before object publication.

Build both checked-in lowering manifests through the qualified native build driver. Their dependencies are retained in canonical module-name order so the ordinary .NET-free source front door accepts the same closure and reproduces the Stage 0 identities exactly.

The affected shared-backend and standalone package selections are the only local behavioral checks for this slice. Local Standard, Qualification, the full Seed/OS suites, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- The Windvale lowerer now accepts its first hosted WVB and emits a real ABI 22 service-table call without using the C# lowerer for that input.
- Capability metadata is validated independently from capability-call lowering, making subsequent service transfers explicit and bounded.
- The current core, memory-adapter, and hosted-tool WVB hashes are `f64bb5e60f69cc2e1ae6662b307d6407bfe70bc8fcf6c277d91143426a4e9143`, `7ea14319b4b27546a4933783ad794fc9b736665f79fd5790f30265a5cb74d8cf`, and `c96e2aa6b0cc77f03f482cd52d5b2488046da4efb453248877c851cdfefbcf16`. The latter two contain 279,763 and 280,791 bytes.
- The hosted tool lowers through Stage 0 to 3,931,712 code bytes and a 3,943,022-byte WVO. Current unpromoted packages are 3,950,080 Windows bytes at SHA-256 `0ea29b5a5e916a18f0d5b998617adc3f8f4da3e2de50c328cc3b993d16839156` and 3,948,544 Linux bytes at SHA-256 `1dfe66328b38964063b61918e2316af24430603f09ffc8af1595e8c956303440`.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Transfer the remaining hosted services only when a concrete tool fixture requires them. Descriptor-bearing `process.argument`, text output, file reads, and file writes each require their own ownership and failure review. Do not infer authorization from declaration, add ambient services, or treat WVO 1.0 as independently service-loadable without an explicit metadata contract.
