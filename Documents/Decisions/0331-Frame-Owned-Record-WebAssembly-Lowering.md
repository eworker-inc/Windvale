# Decision 0331: Frame-owned record WebAssembly lowering

- Date: 2026-08-06
- Status: Implemented candidate; exact compiler cutover pending
- Advances: [Decision 0328](0328-Direct-Enum-WebAssembly-Lowering.md), [Decision 0296](0296-Bounded-Direct-WebAssembly-Nominal-Tables.md), and [Decision 0133](0133-Frame-Owned-Direct-Native-Records.md)

## Context

The direct WebAssembly dispatcher already lowers scalar control, static borrowed descriptors, and enum values, but it stopped at `record.create` and `record.field`. The current browser compiler contains 298 static record constructions and 7,909 static field reads. Leaving those operations in the interpreted compiler would retain a large part of the measured 1.5-billion-operation path.

A monotonic record arena is not an acceptable shortcut. The native backend previously measured more than 77 million dynamically constructed fields in a successful compiler run; retaining every transient construction would turn temporary values into a multi-gigabyte lifetime promise. WebAssembly therefore needs the same ownership principle as the qualified native ABI even though its physical cell width and call encoding differ.

The existing typed-call verifier already proves the nominal type at every instruction. A separate record-only type simulator would create a second semantic path and could disagree at calls or control boundaries.

## Decision

- Expose the typed verifier's bounded instruction state and one-instruction transition as shared immutable evidence. The record planner consumes that transition rather than reimplementing bytecode stack semantics.
- Build a WebAssembly-owned record-storage directory for every function. It records persistent local offsets, block-scoped producer offsets, exact nominal operation tags, frame size, and record-return width.
- Represent each direct record field in one eight-byte WebAssembly memory cell. `i32`-class values use the low four bytes; descriptor-class values use the complete `i64` cell. Nested record-valued fields remain rejected pending an explicit deep-copy contract.
- Give every record parameter and declared record local fixed persistent frame backing. Record loads copy into planned block scratch backing, stores copy into persistent backing, and construction writes directly into planned scratch backing. Scratch numbering restarts only after a verified control terminator, where the operand stack is empty.
- Reserve one bounded memory frame per dynamic call through a private mutable WebAssembly frame pointer. The wrapper starts and resets the pointer at byte 65,536. A checked activation that would exceed the fixed 129-page memory returns status 3017. Failure propagation may leave intermediate frames reserved because execution immediately unwinds to the wrapper, which resets the private pointer before exposing the result.
- Pass record arguments as backing pointers. A record-returning function receives one hidden final `i32` destination parameter, copies its result into caller-owned scratch backing, restores its frame, and returns that caller-owned pointer. Ordinary successful returns restore the frame before returning.
- Retain the existing semantic instruction meter. Frame copies and address arithmetic are implementation work and do not increment the Windvale instruction count.
- Keep the implementation split by ownership: `WebAssembly-Record-Storage.wv` plans bounded backing and `WebAssembly-Record-Operations.wv` emits record memory and calling-convention instructions. Do not add the implementation to `WebAssembly-Core.wv`.

The first planner intentionally assigns each record producer in a basic block its own scratch range instead of reproducing the native interference allocator immediately. Each activation is capped at 1,048,576 bytes and the aggregate live call stack is checked against the fixed WebAssembly memory. Compact record liveness is a later code/data-density improvement, not permission to fall back to a monotonic execution arena.

## Evidence

The 583-byte record fixture has SHA-256 `56cde8d1ad3723353fca2712351338d0013ab3550c24f47a24978438d04bff84`. It exercises direct construction, local store/load copies, field reads, a record argument, and a caller-owned record return. The reference runtime returns 42 after 56 semantic instructions.

The Windvale-written lowering tool deterministically emits a 2,553-byte WebAssembly module with SHA-256 `f73c54d74a753380690e893d325a76fbdfc803553bc02cb1adfa0aae7c8a7432`. Independent Node WebAssembly execution reports status 0, result 42, and exactly 56 metered instructions. Structural inspection confirms fixed 129-page memory and the private fifth frame-stack global.

The focused extended test `Windvale emits general scalar dispatcher WebAssembly` passes. Its earlier scalar, checked-failure, descriptor, descriptor-range, and enum output identities remain unchanged. The composed memory tool is 201,745 bytes at SHA-256 `58533237b210de6da05a4bcaea8a582c571e1b4224915987d8cb15feb5359dce`.

This decision removes executable records as a direct-WebAssembly semantic gap. It does not claim that the complete compiler lowers yet: dynamic text/byte construction, remaining descriptor ownership, the browser `bytes -> bytes` entry contract, and final exact-output comparison remain subsequent gates.

## Consequences

- Repeated record construction reuses bounded frame storage rather than consuming an execution-lifetime arena.
- Record values remain valid across calls, recursion, local assignment, and callee return without host objects or JavaScript assistance.
- The direct WebAssembly function type for a record result differs internally by one hidden destination parameter while the WVB signature and semantics remain unchanged.
- The wrapper remains the only public scalar entry. A later browser compiler entry will define its separate input/output descriptor contract rather than exposing frame pointers.

## Reconsider when

- A supported module needs nested record fields, escaping record references, closures, asynchronous retention, or a public FFI.
- Measured exact-compiler frame size or call depth approaches the fixed memory bound.
- The per-block producer plan materially inflates generation time or emitted code, justifying a WebAssembly-owned compact liveness allocator.
