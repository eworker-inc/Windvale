# Decision 0256: Compact native record liveness

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0241](0241-Multi-Block-Native-Record-Liveness.md), and [Decision 0254](0254-Measured-Native-Function-Envelope.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

The real hosted lowerer contains record-bearing functions with as many as 1,717 combined parameters/locals, 729 basic blocks, and 5,929 decoded instructions. Only 133 of that largest function's locals are declared records; the maximum declared-record-local count is 190 in another function. The former planner indexed dense use, definition, liveness, and interference data by every scalar compiler temporary, then ran exactly one whole pass per block even after reaching its fixed point. Raising its 128-local/block and 1,024-instruction prototype guards directly would waste memory and work.

Independent Stage 0 planning confirms that the real functions remain within the unchanged native frame contract. The tightest projected frame is 1,999 of 2,048 cells. The planner needs compact ownership, not a larger frame.

## Decision

Build one deterministic directory from original local index to compact declared-record-local index. Bound it to 256 entries, use it for per-block use, definition, live-in, live-out, and interference tables, and expand final record-field offsets back to the original local-index layout consumed by machine emission.

Use immutable whole-pass fixed-point updates and stop once no cell changes, while retaining at most one pass per basic block. Admit at most 1,024 blocks and 8,192 instructions. Retain at most 128 block-scoped record values, the existing single-record-argument call accounting, immutable record parameters, exact liveness/interference allocation, and the hard final 2,048-cell frame check.

Add one compact test-built module whose record-returning helper has one scalar local, 129 declared record locals, 130 reachable blocks, 3,356 code bytes, and 1,032 instructions. Stage 0 and both Windvale adapters must produce the same complete WVO and native execution must return 42.

## Consequences

- Large scalar-temporary inventories no longer enlarge record-liveness matrices or record-local interference graphs.
- The real tool's later large record-bearing functions fit the compact planner envelope without changing native frame geometry or selected WVO semantics.
- Direct self-lowering still fails closed before those functions: ordinal 18 calls a four-record-parameter helper, while this decision deliberately retains single-record-use event accounting.
- The core closure is 325,523 bytes at SHA-256 `50107b76ed109819bb3578bb43174e4beb560eb792ed8dc67712606cf80b6828`.
- The memory adapter is 320,612 bytes at SHA-256 `f340cf67b4063b315a531b17d28eec0f8c3813cb1b98b201f74a3ff6dcda34b9`; the hosted tool is 321,640 bytes at SHA-256 `7921493f5b918073600d47e168c42d2a051dfda6e1586bf3520a723f0e0c8876`.
- Current unpromoted packages are 4,439,552 Windows bytes at SHA-256 `697db83716652b39d820e769270dceacf93d4dce277adf3a03273bb2c98bb91a` and 4,440,064 Linux bytes at SHA-256 `e7efe875df51c54d998862a61735cdcae2729c737949d021f154eb946ea2d8b2`.
- No C# implementation changed. Stage 0 remains the independent oracle and recovery path until the grouped dual-host and complete retirement gates pass.

## Reconsideration triggers

Replace the single-use record event with bounded multi-record argument accounting before treating a larger function as the next active rejection. Revisit the compact limits only when a qualified accepted module reaches one of them.
