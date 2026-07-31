# Decision 0056: Windvale-owned post-memory evidence

- Date: 2026-07-31
- Status: Accepted, implemented, and qualified on the first Windows QEMU environment
- Supersedes: Decision 0054's native seam version 1 shape

## Context

Decision 0054 inserts a verified WVA tail transfer before compiler-generated Windvale Main, but the memory emitter still generates `memory-owned=pass` and `allocator=pass` byte calls in raw C#. Compiler output also imports the C# COM1 function directly. The native target cannot yet implement pointer-heavy allocation policy, but it can already emit deterministic source-selected lines after a successful memory transition.

Leaving those markers in the raw emitter would overstate the amount of output policy that still requires C#. Leaving the byte-writer import direct would also bypass the WVA machine seam in the WV-to-platform direction.

## Decision

- Advance [kernel native seam version 2](../../Specifications/Windvale-Kernel-Native-Seam.md).
- Move `memory-owned=pass` and `allocator=pass` into `Operating-System/Kernel/Hello-World.wv`, ahead of the existing stack and Hello World lines. Compiler-generated Main now selects every line from memory evidence through Hello World; loader-owned continuation and final-status lines remain distinct.
- Emit none of those lines from the memory machine object. It calls Main only after complete map validation, arena initialization, one successful allocator probe, handoff copying, and the owned-stack switch, so reaching the source lines remains conditional on all corresponding operations succeeding.
- Replace the one-way shim with `Operating-System/Kernel/X64-Kernel-Shims.wva`. It exports both `Windvale_kernel_wva_main` and the compiler-facing `Windvale_kernel_write_byte`, and imports compiler Main plus internal machine symbol `Windvale_kernel_x64_write_byte`.
- Make the compiler and memory objects resolve only WVA-owned public seam symbols. Keep COM1 polling in C# temporarily under the explicitly machine-specific internal symbol.
- Independently verify the exact two-tail-transfer WVA object before linking it.

## Consequences

The memory-through-Hello portion of the transcript is now source-owned rather than bootstrap-authored. The WVA object mediates execution in both directions: machine entry tail-transfers to `.wv` Main, and every source-selected console byte tail-transfers through WVA to the remaining x64 adapter.

The canonical compiler object is 2,564 bytes with SHA-256 `f2c28eb5f020f59b8acb480fc8dc62e393ebb14405b3c12ecb05076176d44420`. The canonical WVA seam is 279 bytes with SHA-256 `36ea8c6ebcd5e1ef51ff332344aa549a8ec7aadaf485d44306ee63d5b41d4123`. The deterministic 7,168-byte EFI application has SHA-256 `92ad46700b058cd3a8846c59c227a33ef3832b080fb408e8eee42dc301336d9a` and passes the unchanged exact QEMU transcript.

Source ownership of a success marker is control-flow evidence, not independent reinspection of memory state. The map scanner, arena initializer, allocator machine implementation, UEFI loader, COM1 I/O, linker, and PE32+ packager remain C# reference/bootstrap code. Moving their policy into `.wv` still requires the native pointer, control-flow, internal-call, and unsafe-memory subset identified by the seam contract.

## Reconsider when

- `.wv` can receive and inspect the versioned memory-state pointer directly.
- WVA can express the named port-I/O operations needed to replace the remaining COM1 emitter.
- A real kernel diagnostics service supersedes the per-byte bootstrap adapter.
- Native internal functions make `Hello-World.wv` separable into policy and demonstration modules.
