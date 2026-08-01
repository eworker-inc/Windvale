# Decision 0061: Typed native blocks and forward control flow

- Date: 2026-07-31
- Status: Cross-host qualified on Windows x64 and Debian x64
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

Corruption evidence rejects a missing zero-initialization prefix, an invalid comparison condition, a target in the middle of a branch group, and a backward target as `WVN3030`. A verified WVB loop fails as `WVN2005`, while calls retain their existing bounded rejection. The focused Windows test passes with a zero-warning build; Development passes all 48 regular tests in 55.008 seconds; Standard passes all 49 cases in 230.225 suite seconds and 234.6 wall-clock seconds.

## Qualification evidence

Exact candidate commit `f0a53a99b25638cd02acafba81c066be931ef5e4`, tree `e425e9d24f373a1d09694a38910b85a98359725c`, was archived as `windvale-native-control-f0a53a9.tar.gz`, 2,794,632 bytes with SHA-256 `5448e3eab034997b877ab05c7b61dc8337adc7ec78b4f095f54f6b8e898b0603`. The digest matched after transfer to the isolated E-Worker Debian QA host, where the focused native case passed through Linux `mmap`/`mprotect` in 166 milliseconds with a zero-warning build.

Windows Qualification completed in 469.086 seconds with a 224.296-second suite; Debian Qualification completed in approximately 489.6 seconds with a 243.020-second suite. Both hosts used .NET SDK `10.0.302`, passed zero-warning Release builds, all 49 tests, and the complete native CLI verifier. The 15,563-byte Windows conformance report has SHA-256 `6780bd68cfcf7dacea10d34f5a4b9d7eeb6cdc2c2c4a70cf055c6830429330f`; the 15,473-byte Debian report has SHA-256 `1fec4c222425f2737a7f41c415b1841f88aab0a8e7458b40d4e7d170e1d9c35d`. Their normalized contracts match exactly.

All 61 directly retrieved portable artifacts, totaling 7,752,612 bytes, matched Windows byte for byte. The 2,292,663-byte Debian evidence bundle has SHA-256 `afe0d6a69b3a67ddb1af6e7b0214b9899cc18dcd3078b1e7245292439a42196e`. After retrieval and comparison, the resolved exact QA directory, transferred source archive, and evidence bundle were removed and confirmed absent.

## Consequences

The machine IR now represents real portable control structure instead of an x86 byte pattern, and mutable source state has an explicit frame owner. The same design can later admit loops without changing block semantics once execution budgeting exists. Calls and static data can add typed terminators/operations and fragment patches without creating a second compiler path.

The larger native stack remains Stage 0 C# orchestration. This decision does not claim native loops, calls, data, capabilities, PE/ELF hosts, a code cache, safe points, native compiler execution, Windvale-written runtime ownership, OS adoption, or .NET retirement.

## Reconsider when

- A native execution-budget design can account for backward branches without changing portable observable behavior.
- Register allocation makes one frame slot per static result unnecessary while retaining independently verifiable spill and initialization rules.
- Calls require phi-like merge values, a memory return area, unwind metadata, or a different frame convention.
- AArch64 selection reveals that a block or condition contract is unnecessarily x86-shaped.
