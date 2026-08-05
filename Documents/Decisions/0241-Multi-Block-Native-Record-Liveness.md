# Decision 0241: Multi-block native record liveness

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0240](0240-Bounded-Native-Record-Calls.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0240 transferred the record ABI mechanics independently, but record-bearing functions still had to contain one basic block and a record argument could only be passed to a record-returning helper. Compiler-produced `Nominal-Types.wv` is the next complete source fixture: its `Main` constructs descriptor-bearing records, carries record locals through a branch-heavy control-flow graph, passes a `Reading` through `Keep(Reading) -> Reading`, and later calls `Measure(Reading) -> i32`.

The existing linear record-storage scan could not derive persistent local lifetimes across successors. A naive immutable-byte implementation of the required fixed point also consumed too much temporary native arena space when applied to the fixture's 67 locals and many control blocks, even though it produced the correct result under the reference runtime.

## Decision

### Plan persistent record locals over the control-flow graph

Admit up to 128 blocks in a record-bearing function. Build exact per-block record-local use and definition sets plus zero, one, or two validated successors. Iterate liveness backward to derive `LiveIn = Use union (LiveOut - Definition)` and `LiveOut` from successor `LiveIn` sets. Reverse-scan each block from its final `LiveOut`, keeping every record store separate from other live record locals before killing that definition, then assign deterministic width-first first-fit field ranges.

Replace the live-in and live-out maps one complete block row at a time. This preserves the fixed-point result while avoiding a complete immutable matrix copy for every local cell. The generated Windows native tool must lower the real nominal fixture within its established bounded runtime arena.

### Keep record temporaries block-scoped

Reset record value identities at every block leader, matching the shared backend's typed value-slot reuse. Within each block, record the last use of every record result, add interference between a new result and all currently live record results, and require no record temporary to remain live at a block edge. When the same physical record slot carries different admitted record types in different blocks, reserve the widest required field range.

Extract deterministic interference allocation to `Native-X64-Lowering-Record-Allocation.wv` and persistent control-flow analysis to `Native-X64-Lowering-Record-Local-Liveness.wv`. `Native-X64-Lowering-Record-Storage.wv` remains responsible for bounded instruction scanning, block topology, record-result/use events, and composing the two plans.

### Admit scalar-returning record consumers

Use the same ABI-22 64-bit record-handle argument loads for scalar-returning and record-returning calls. Measure call argument bytes from the exact parameter types rather than assuming every argument is a 32-bit scalar. Retain the existing limit of at most one record argument and the ban on mutable record parameters, stack-passed records, and nested record fields.

### Require the real source and packaged-process evidence

Extend the shared-backend differential case with the exact 1,782-byte `Nominal-Types.wv` WVB, SHA-256 `b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b`. Interpretation and Stage 0 native execution must return 11. The Windvale memory adapter and hosted source tool must reproduce Stage 0's exact 22,404-byte WVO, SHA-256 `460695af54b5cd4f7d4597f9bc60a17e29e236ddacc0330b1f541ab455759085`.

The standalone WVB-to-WVO package test must additionally run that fixture through the direct native package for the current host and compare the complete WVO. The reviewed shared-backend and package selections are the only local verifiers for this slice. Local Standard, Qualification, the full Seed/OS suites, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- Compiler-produced `Nominal-Types.wv` now lowers byte-for-byte through Windvale source and through the direct Windows WVB-to-WVO package without loading .NET.
- Persistent record locals have reusable control-flow liveness rather than fixture-specific storage, and block-scoped record temporaries retain an explicit no-cross-edge invariant.
- Scalar helpers such as `Measure(Reading) -> i32` now use the same bounded record argument ABI as record-returning helpers.
- The current core, memory-adapter, and hosted-tool WVB identities are `20b0f6158e2ce968b3e5bcaf472e291bf62cfc3ec0dc9e3569b68acf4ca528f8`, `b06944f1d7a3977275fe147ab8fd8f283dbcfe00332f00af13485e296f20f86f`, and `1689fdb55f3e6cd1b9bf75d7f94f7f7e8550c264a6c8c2af7008c5b176364eb2`. Their sizes are 275,174, 270,067, and 271,095 bytes respectively.
- The hosted-tool WVB lowers through Stage 0 to 3,828,196 code bytes and a 3,838,832-byte WVO. Current unpromoted packages are 3,846,656 Windows bytes at SHA-256 `868ce0511e84ddfac0424b65ceb2d11f8d7bf408afef3560b4dc6749bcdef35f` and 3,846,144 Linux bytes at SHA-256 `b6323b850d493a98a7362e0a9e01695a6d581764e4430f39129d6e645ea9eb90`.
- No C# implementation changed in this slice. Stage 0 remains the independent oracle and candidate constructor until the grouped dual-host and artifact-promotion gates pass.

## Reconsideration triggers

Continue with the next concrete compiler-produced fixture or hosted-capability boundary. Do not broaden multiple record arguments, mutable record parameters, stack-passed records, nested records, heap records, or payload variants without a measured consumer and an explicit ownership contract.
