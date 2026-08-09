# Decision 0419: Descriptor-returning native Main

- Status: Implemented candidate; native tool promotion pending
- Date: 2026-08-08
- Advances: [Decision 0304](0304-Digest-Bound-Native-Wvb-To-Wvo-Candidate.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The Windvale-owned baseline-JIT producer exposes the portable entry
`Main() -> bytes`. Its WVB already rebuilt through the native source front door,
but the Windvale native x64 lowerer admitted only scalar-returning exported
`Main`. The baseline publisher therefore retained a WVO produced by Stage 0,
leaving managed backend code in that artifact's construction provenance even
though normal publication and execution loaded no managed runtime.

Internal descriptor-returning helpers already implemented caller-owned result
cells, arena checkpoints, validation, and compaction. The missing boundary was
the distinct exported descriptor ABI: its caller-owned result cell arrives in
`RCX`, its execution context arrives in `RDX`, its caller retains the entry
arena, and the host's nonvolatile `R15` must be restored on return.

## Decision

Admit exactly a parameterless exported `Main() -> bytes` in addition to the
existing `Main() -> i32`; do not widen the entry contract to text, records, or
parameters. Preserve `RCX` in the existing hidden descriptor-result frame cell,
load the shared execution context from `RDX`, and publish the returned pointer
and length directly into that cell with a zero reserved word. Do not take or
restore an internal-call arena checkpoint: the exported caller owns the entry
arena through result consumption.

The exported descriptor return emits 47 machine bytes plus the existing ten-byte
instruction charge, so its measured sequence is 57 bytes. Its entry-prologue
base is 15 bytes and its fixed function-size base is 205 bytes. The internal
descriptor return remains the 306-byte checkpoint/validation/compaction stencil;
scalar exported entries and all internal descriptor returns retain their
existing bytes.

## Evidence and consequences

The focused 201-byte descriptor-entry WVB has SHA-256
`e736d9d1b1223ae1e90ee0660a42af671f9b7faee5c38ba8187b56b045bd6d8f`.
The current Windvale lowerer and the frozen Stage 0 backend independently
produce the same 793-byte WVO with SHA-256
`9936663f45c194441bfc5e8464286e57f83cd3a18948597a8011af608a4faa51`.

The 4,574-byte baseline-JIT bridge WVB with SHA-256
`2dc536e9d3511d4fde3191e1084d9634543154a525623fd3c7c669f9d3bf20d9`
executes 8,932,333 instructions in the reference-hosted Windvale lowerer and
reproduces the retained verified 56,226-byte Stage 0 WVO byte for byte at
SHA-256
`bcc02cdc6134da2388265ad308d3dc739a7e10c1911effa918d5f2577c86ae8c`.
That object links into the unchanged 59,904-byte Windows publisher with SHA-256
`8ea1a0d6371c9447031db4ae2b56ecfef5f022a83b6bdd7831020a2628bee01c`;
the application returns zero after both generated functions, the forced seal
failure, and teardown. The paired 65,648-byte Linux ELF has SHA-256
`29538c93d28bcd1feae175519f5b2950d5e8dfcde24afa3f0039863fb1706a90`
and still requires Linux execution.

No C# compiler or runtime behavior changes are part of this decision. The
existing managed reference runtime hosted the Windvale lowering module only as
explicit recovery evidence, and its test suite advances only the frozen size
and digest pins for the changed Windvale source closures.
The current lowerer project builds through the native source front door as a
399,691-byte, 409-function WVB with SHA-256
`92655af0632b4dd3525c2b2de98353b095fa1df94b524a94aa47f16014f1e508`.
The pinned native lowerer applications cannot yet reconstruct that evolved
closure, so native current-tool reconstruction and promotion remain a separate
N1 blocker. Normal baseline-JIT verification and execution consume the retained
verified WVO and do not load .NET.

## Reconsideration triggers

Widen exported descriptor entries only through an explicit ABI decision with
paired-host service-bearing execution evidence. Revisit the recovery provenance
when a native lowerer application reconstructs this exact source closure and the
resulting bridge WVO byte for byte.
