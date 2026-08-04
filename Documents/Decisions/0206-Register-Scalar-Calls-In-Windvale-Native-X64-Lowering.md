# Decision 0206: Register scalar calls in Windvale-native x86-64 lowering

- Date: 2026-08-04
- Status: Implemented locally; independent cross-host qualification pending
- Advances: Phase 10 native host tools and the [Decision 0057 native-retirement gate](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Extends: [Decision 0205](0205-Bounded-Direct-Calls-In-Windvale-Native-X64-Lowering.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0205 transferred declaration traversal, two independent frames, shared instruction and call-depth budgets, packed-status propagation, direct call patching, and two-symbol WVO emission into Windvale source. Its helper was parameterless. The next ownership boundary was therefore the typed value transfer across that call, not a broader graph or a second ABI.

ABI 22 already assigns its first four private scalar call arguments to `R8D`, `R9D`, `ECX`, and `EDX`. Canonical WVB records parameter types before declared local types, while machine lowering treats parameters as the first ordinary local cells. Reusing those contracts keeps Stage 0 and Windvale selection byte-identical and leaves stack-passed arguments as a separate measurable boundary.

## Decision

Retain Decision 0205's exact one-helper graph and `i32` return. Permit helper index zero to declare zero through four `i32` or `bool` parameters; `Main` remains parameterless. Count parameters and declared locals together against the existing 1,024-local bound and retain the 2,048-cell complete-frame bound.

Parse and validate the canonical parameter vector. During control analysis, require each `call 0` operand stack suffix to match the helper's declared types in source order, consume exactly those values, and produce one `i32`. Reject excess parameters, unsupported types, underflow, type disagreement, helper calls, and any target other than zero before object emission.

Before the direct call, load the argument cells into `R8D`, `R9D`, `ECX`, and `EDX` using the exact shared x86-64 encodings. After the helper frame is allocated and zeroed, store those registers into parameter cells zero through three. Include those bytes in prologue, body, block-target, function-symbol, and complete-object size calculations. Do not change ABI 22, WVB, WVO, statuses, budgets, or the existing parameterless bytes.

Extend the existing shared-backend differential lane rather than adding or repeating a top-level test. Its new oracle uses four mixed `i32`/`bool` parameters, helper control flow, all four register positions, and result `42`. Require exact Stage 0 WVO bytes through the Windvale memory adapter, hosted tool, and the same tool compiled to native x86-64. Mutating one declared parameter type must be rejected before emission.

## Consequences

- The Windvale-written selector now owns the complete register-only scalar value transfer for its bounded direct-call edge.
- The mixed-scalar oracle is exactly 2,581 code bytes / SHA-256 `1a0a541d2bd59378b4fa6df53248c3c359e909a0b7446198ebb1a58ca5a79721`; its 2,688-byte WVO has SHA-256 `cb7d2c74edb7aa3443e1e23cf0d762d4c15b79c39ea4f363531b2ec80633c13f`.
- The core, memory-adapter, and hosted-tool WVB identities are respectively `75bc5ed88d2da94e602957ea9df6751470e277a135de76286c249d163983e26d`, `34acd0b6b0b58eb08c737c1e86b940314223a23ee968aabdfb098ec645462930`, and `bb76b15ba551f16101ecef600f47fabb8098e5c9b903a17aa14d893e9fe1854d`.
- The hosted tool currently lowers through Stage 0 to 1,114,491 code bytes and a 1,116,927-byte WVO. These are implementation measurements, not optimization promises.
- The former parameterless oracle remains byte-identical at 795 code bytes and a 902-byte WVO.
- Stack-passed or descriptor parameters, non-`i32` returns, deeper acyclic graphs, recursion, data, and general call-graph metadata remain outside this slice.

## Reconsideration triggers

- stack-passed parameters require a different frame reservation or source-slot adjustment than ABI 22;
- more than one helper requires stored function directories or a general bounded call-graph proof;
- mutable parameters or aggregate copies require entry semantics beyond ordinary scalar local cells;
- internal calls move across object sections or objects and require typed relocations; or
- independent Windows/Linux evidence changes any accepted byte, status, or rejection result.
