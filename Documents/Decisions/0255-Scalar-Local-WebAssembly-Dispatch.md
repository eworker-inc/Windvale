# Decision 0255: Scalar-local WebAssembly dispatch

- Date: 2026-08-05
- Status: Implemented as a candidate; dual-host qualification pending
- Advances: [Decision 0253](0253-Native-Built-WebAssembly-Interpreter.md)
- Contract: [Windvale WebAssembly](../../Specifications/Windvale-WebAssembly.md)

## Context

The exact portable compiler contains 157,844 decoded instructions across 417 functions. Static frequency is dominated by `local.load` at 66,030 instructions and `local.store` at 50,915 instructions, together 74.1 percent. The interpreter's local-load path read nominal metadata and tested zero defaults for every scalar value even though only records and enums use that rule. After handling opcodes zero through 22, execution also evaluated every disjoint higher-opcode dispatch region before reaching the common ownership epilogue.

The native-built interpreter already reduced WebAssembly engine compilation pressure, but its 100,000-step compiler calibration still consumed 192,935,833 enclosing instructions. A broader attempt to bypass descriptor bookkeeping for scalar locals reduced enclosing instructions slightly but enlarged the generated control graph and made measured Node.js wall time worse; that experiment is not retained.

## Decision

Read nominal tokens and apply zero-default reconstruction only for local shape kinds seven and eight. Mark descriptor production inside the existing shape-five/six ownership path. Scalar kinds one through four now slice their exact eight-byte cell without nominal reads or zero-default branches.

Make the existing opcode-zero-through-22 region the first side of an explicit `if`/`else`. Place every opcode-32-and-higher handler in the other side. Unsupported opcodes 23 through 31 still reach the same invalid result, and all operation-specific ownership plus the common epilogue remain unchanged.

## Consequences

- Scalar, descriptor, record, enum, call, heap, instruction-meter, status, and reset semantics do not change.
- The interpreter retains three capability-free `bytes -> bytes` functions, a 981-local root, no data section, no host imports, fixed 129-page memory, and execution ABI 3.
- The native WVB keeps its 105,936-byte and 103,396-code-byte extents; control targets change its artifact identity.
- Low opcodes avoid disjoint high-opcode comparisons, while high opcodes pay one outer branch. Nominal local behavior remains isolated to the shapes that require it.
- The rejected scalar-epilogue bypass remains out of source because lower instruction count did not compensate for worse engine optimization and mixed-workload wall time.

## Focused evidence

The ordinary native front door publishes WVB SHA-256 `7dd10696dbc00741911f68e65dbae8623e7aafe3bfc7896c76a0e0e1474059b6`. The retained backend lowers it in 260,827,558 instructions to 782,416 import-free Wasm bytes with SHA-256 `ba1d67e254191731c174f0644735403a01fb2382b7cad36623d04f707efd338e`.

Ordinary Node.js passes the complete retained probe. The 15,627-instruction ownership workload returns `69` after 61,459,738 outer instructions; text/bytes returns `42` after 565,759; formatting returns `42` after 4,850,580; SHA-256 returns `42` after 5,887,762; guest budget 350 returns exact `WVR3011` after 516,056; and budget 351 then returns `42` after 516,499. Seven malformed-envelope cases still return empty output.

The exact 919,577-byte portable compiler reaches guest budget 100,000 after 185,288,631 outer instructions, down 7,647,202 or 4.0 percent from Decision 0253. Complete `WVCO 1` output is not yet claimed.

## Native publication boundary

The WebAssembly backend project itself builds byte-identically through the ordinary native front door at 321,867 WVB bytes with SHA-256 `2e5fa504aa16c17c567f0e35161f2c5024336cc9f70617313270d7ae72d824fa`. A reverted experiment composed that backend into the compiler build driver without C# source changes. Stage 0 recovery compilation produced a valid 1,400,728-byte combined WVB, but x64 selection measured 34,076,699 fragment bytes across 609 functions and 478,421 operations, exceeding the qualified 33,554,432-byte bound by 522,267 bytes.

Do not raise the shared native limit or label the backend as another fixed application profile merely to fit this tool. A dedicated WebAssembly application profile, or the general Windvale-native lower/link/package path when qualified, remains the required boundary for normal no-.NET Wasm publication.

## Reconsideration triggers

Revisit local representation when a Windvale-owned mutable frame primitive or a verified chunked-cell representation can avoid whole-value reconstruction without weakening immutable language semantics. Revisit dispatch shape with dynamic opcode evidence rather than static frequency alone. Replace Stage 0-hosted backend execution only through a correctly named, independently verified native publication contract.
