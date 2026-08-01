# Decision 0061: Typed native blocks and forward control flow

- Date: 2026-07-31
- Status: Implemented and validated on Windows x64; exact-commit Debian qualification pending
- Refines: [Decision 0060](0060-Checked-Native-I32-Arithmetic-And-Traps.md)'s straight-line machine IR and fragment decoder

## Context

Decision 0060 proves checked straight-line computation and recoverable overflow through one shared WVO/AOT and W^X fragment. Comparisons and source-level `if` require explicit control flow. Encoding them as an x86-only selector pattern would leave the machine IR unable to express the portable program and would make later calls, loops, other architectures, and independent verification harder.

The native executor does not yet have the reference interpreter's instruction budget or a native safe-point contract. Accepting backward branches now would let an otherwise byte-safe fragment run forever inside the host process. The executable-code verifier must also prevent branches into the middle of an admitted instruction sequence and must not expose uninitialized host stack bytes through WVB locals.

## Decision

- Replace the straight-line function operation list with typed native locals, typed numbered values, canonical basic blocks, and explicit `jump`, `branch`, and `return` terminators. Block IDs are dense and deterministic. Operations remain architecture-neutral; the x86-64 selector owns encodings and relative displacements.
- Lower only already verified portable WVB with one exported `Main() -> i32`, i32/bool locals, an empty operand stack at every control boundary, constants, local loads/stores, checked i32 arithmetic, all six signed i32 comparisons, bool equality/inequality/negation, forward jumps, conditional branches, and returns. Calls, data, capabilities, nominal values, other primitive widths, cross-block operand stacks, and unsupported opcodes still fail without fallback.
- Permit mutable frame-backed locals so forward branches can merge ordinary source state without inventing premature SSA phi nodes. Static operation results remain typed and canonically numbered. Locals occupy the first frame slots and operation values follow them.
- Advance the internal experimental target to `x86-64-wvb-baseline-v3` and ABI version `3`. There is no compatibility reader for version 2. Decision 0060's exact version-2 evidence remains historical at its qualified commit.
- Keep the one-page 4 KiB frame ceiling. The prologue zeroes every frame slot, including alignment padding, before any body load. This matches the reference runtime's typed default-local initialization and prevents crafted admitted code from reading stale host stack bytes.
- Emit signed comparisons as `cmp` plus one exact `setcc` and `movzx` sequence. Emit a conditional terminator as an exact condition load/test, `jne` true target, and direct `jmp` false target. Emit every internal target as a fully resolved relative i32 displacement in the shared fragment so JIT and WVO/AOT consume identical bytes.
- Accept only forward control targets in this slice. Verified WVB with a backward edge fails as `WVN2005`; the independent machine-code decoder also rejects backward or mid-group targets as `WVN3030`. Loops become eligible only after a native instruction-budget or safe-point design prevents unbounded in-process execution.
- Independently decode the exact frame initialization and every admitted semantic instruction group. Require in-frame aligned accesses, exact checked-overflow targets, allowed `setcc` conditions, exact balanced return/trap epilogues, forward targets on decoded group boundaries, complete reachability, complete byte consumption, and a minimal aligned frame. Any other byte sequence fails before WVO or executable-memory publication.

## Initial evidence

The primary differential source computes `42`, exercises all six signed i32 comparisons, bool equality and negation, nested true branches, and an early return. A second program proves the false path. The reference interpreter, W^X JIT fragment, and WVO-linked AOT image agree.

The deterministic control fragment is 2,428 bytes with SHA-256 `55f7ec906db8c8e06dc89ba0a55556cd1248d3a5491585134bbacbeaa6400d44`; its 2,530-byte WVO has SHA-256 `7442f46a0a03b4870150b34ad75724e44cd9ded8b73e875db84130bb6c42f8f2`. The version-3 checked-arithmetic fragment is 745 bytes with SHA-256 `8876722852c5cae70e61fb3d3f65bfb3c4d0651ec3f8adde63d2e258aaea44e9`; its 847-byte WVO has SHA-256 `6740c3d95f8746137d55ad1b339ad0bb772a4c3ccf6f29d0836576959057317a`. The six-byte constant and its 79-byte WVO remain exact.

Corruption evidence rejects a missing zero-initialization prefix, an invalid comparison condition, a target in the middle of a branch group, and a backward target as `WVN3030`. A verified WVB loop fails as `WVN2005`, while calls retain their existing bounded rejection. The focused Windows test passes with a zero-warning build; Development passes all 48 regular tests in 55.008 seconds; Standard passes all 49 cases in 230.225 suite seconds and 234.6 wall-clock seconds. Exact-commit Windows/Debian Qualification remains required.

## Consequences

The machine IR now represents real portable control structure instead of an x86 byte pattern, and mutable source state has an explicit frame owner. The same design can later admit loops without changing block semantics once execution budgeting exists. Calls and static data can add typed terminators/operations and fragment patches without creating a second compiler path.

The larger native stack remains Stage 0 C# orchestration. This decision does not claim native loops, calls, data, capabilities, PE/ELF hosts, a code cache, safe points, native compiler execution, Windvale-written runtime ownership, OS adoption, or .NET retirement.

## Reconsider when

- A native execution-budget design can account for backward branches without changing portable observable behavior.
- Register allocation makes one frame slot per static result unnecessary while retaining independently verifiable spill and initialization rules.
- Calls require phi-like merge values, a memory return area, unwind metadata, or a different frame convention.
- AArch64 selection reveals that a block or condition contract is unnecessarily x86-shaped.
