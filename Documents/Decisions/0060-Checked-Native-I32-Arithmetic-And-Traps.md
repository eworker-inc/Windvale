# Decision 0060: Checked native i32 arithmetic and traps

- Date: 2026-07-31
- Status: Implemented and validated on Windows x64; exact-commit Debian qualification pending
- Refines: [Decision 0059](0059-First-Shared-Native-Wvb-Slice.md)'s first shared native fragment

## Context

Decision 0059 proves the verified-WVB, machine-IR, WVO/AOT, W^X publication, and interpreter/JIT/AOT seam with one constant return. The next useful slice must execute real computation without letting x86 wrapping behavior silently replace Windvale's checked `i32` semantics. Host signals or process faults would make overflow recovery platform-specific and unsafe for an in-process runtime. A reserved integer result cannot represent a trap because every 32-bit bit pattern is a valid Windvale `i32` value.

The fragment verifier must also remain an executable-code security boundary. Widening the selector cannot mean accepting arbitrary bytes merely because they were packaged as `Nativeˉfragment`.

## Decision

- Extend the machine IR with explicit checked `i32` add, subtract, multiply, and negate operations over numbered immutable values. Lower only already verified straight-line WVB using constants, single-assignment i32 temporaries, those arithmetic opcodes, and one final return. Calls, branches, comparisons, data, capabilities, and mutable locals still fail without fallback.
- Bound a native function to 1,024 values and a 4 KiB stack frame. The selector assigns one four-byte frame slot per machine-IR value, rounds the frame to 16 bytes, uses only volatile `eax` and `ecx`, and emits no calls. The one-page ceiling avoids an implicit Windows stack-probing dependency and is valid under both Windows x64 and System V x86-64 entry conventions.
- Advance the internal experimental target to `x86-64-wvb-baseline-v2` and ABI version `2`. There is no compatibility reader for version 1. Decision 0059's exact version-1 evidence remains historical at its qualified commit.
- Return one packed unsigned 64-bit outcome in `rax`: the low 32 bits carry the `i32` result and the high 32 bits carry status. Status `0` means success; status `1` means integer overflow. This preserves every `i32` bit pattern without a sentinel. The runtime maps status `1` to Windvale trap `WVR3007` and rejects any unknown status as `WVN4005`.
- Emit `jo` after every checked arithmetic instruction. All overflow branches target one local epilogue that restores `rsp`, publishes status `1`, and returns normally to the runtime. Successful execution restores `rsp` and returns status `0`; no path leaves writable/executable overlap or raises a host signal.
- Keep the emitted internal overflow displacements fully resolved in the shared fragment. The WVO and in-memory sinks therefore consume the same exact bytes. Typed external/internal patch records remain reserved for the later call and data slices.
- Independently decode the permitted x86-64 bytes before either sink can use them. The fragment verifier proves the exact prologue and epilogues, bounded/minimal frame, contiguous single-assignment slots, initialized loads, allowed arithmetic encodings, every overflow-branch target, local/export symbol ranges, packed trap status, and complete byte consumption. Any other instruction or control target fails as `WVN3030` before publication.

## Initial evidence

The Windows x64 differential case compiles:

```windvale
return -(((2 + 2) - (7 * 6)) - 4);
```

The interpreter, W^X JIT fragment, and WVO-linked AOT image all return `42`. The fragment is 239 bytes with SHA-256 `9ab91125f773fc56f6aafc4ac66ebb4596d2dc93f7eec713d13aaf1053558bf6`; its deterministic 341-byte WVO has SHA-256 `ef8bb7e4322ef7bce4ad9de0710a441f617bfb6dbc5e7d05b3e883fce1319cbc`. Independent selection and serialization produce identical bytes.

Separate add, subtract, multiply, and negate overflow programs trap as `WVR3007` in both the reference interpreter and native JIT; the add case also traps after WVO linking and AOT execution. Corrupting an overflow displacement is rejected as `WVN3030` before executable memory publication. The complete solution builds with zero warnings and errors, and the focused native differential case passes on Windows. Exact-commit Debian qualification and broader Windows/Linux verification remain required.

## Consequences

The native seam now expresses useful checked computation and a platform-neutral recoverable trap without introducing calls or host exception machinery. The generated code itself is native, while the compiler, verifier, runtime orchestration, and trap translation remain C#/.NET Stage 0.

The next evidence-driven backend slice should add boolean comparisons and structured control flow while reusing the same packed outcome, bounded frame, independent byte decoder, WVO sink, and W^X adapter. Runtime service calls, static data, broader values, heap ownership, Windvale-written implementation, and operating-system adoption remain later gates.

## Reconsider when

- A later result category needs more than the current two-word outcome without a memory return area.
- Calls require a different frame/unwind convention or prove that direct overflow branches should use typed patch records.
- AArch64 cannot share the status/value semantics cleanly behind architecture-specific selection.
- Profiling shows that one frame slot per immutable value is impractical before register allocation is introduced.
