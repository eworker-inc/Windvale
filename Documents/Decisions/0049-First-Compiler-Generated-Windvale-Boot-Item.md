# Decision 0049: First compiler-generated Windvale boot item

- Date: 2026-07-31
- Status: Accepted, implemented, and qualified on the first Windows QEMU environment

## Context

Decision 0048 established a verified WVO import/export seam and versioned post-firmware handoff, but its kernel entry was raw x86-64 assembled by a private bootstrap builder. The C# reference compiler already produced typed WIR and verified WVB, while the object model and linker already supplied the required native publication boundary. The next proof needed to originate observable post-firmware behavior in a real `.wv` source file without pretending to implement a broad native backend or embedding a source-string recognizer in the OS.

The smallest useful source program declares `console.write_line`, prints one immutable text value, and returns an `i32`. Directly granting source code a COM1 port would erase the capability boundary. Requiring static data or a new executable format would expand the step without improving the central source-to-boot proof.

## Decision

- Define [`x86-64-kernel-entry-wvo-v1`](../../Specifications/Windvale-X64-Kernel-Target.md) as a narrow C# reference/recovery compiler target over the existing typed WIR.
- Require a `system` module containing one exported linear `Main() -> i32`, text constants, declared `console.write_line` calls, and a constant return. Reject every unsupported profile, capability, type, operation, control-flow shape, non-ASCII output, and output beyond 4 KiB with stable native-backend diagnostics.
- Generate one code-only WVO object that exports `Windvale_kernel_entry`, imports `Windvale_kernel_write_byte`, and represents every adapter call with a verified `relative-i32` relocation. Decode and independently verify the serialized WVO before returning it.
- Generate the handoff-version-1 validation wrapper in the compiler target. Source-derived calls run only after the wrapper accepts the retained memory-map record.
- Keep serial device ownership in the OS. The bootstrap supplies a separate WVO adapter that accepts one byte in `ECX`, polls COM1, writes it, and returns. The declared source capability is therefore explicit even though its first target implementation is intentionally small.
- Store the canonical source as `Operating-System/Kernel/Hello-World.wv` and embed it as a build input. Firmware probe version 5 compiles that resource, links the loader, generated kernel, and OS adapter through the existing linker, and packages the verified all-code result through UEFI application writer version 2.
- Require the exact post-firmware source line `Hello from Windvale`, followed by loader evidence `windvale-source=pass`. Do not accept the loader marker alone as proof that source-derived code ran.

## Consequences

The accepted QEMU run now proves the complete bounded path from `.wv` text through the ordinary frontend and semantic WIR, verified native WVO production, symbol and relative-relocation resolution, PE32+ packaging, UEFI entry, bounded firmware exit, handoff validation, compiler-generated x86-64 execution, an explicit OS capability adapter, and deterministic serial observation.

The canonical kernel object is 905 bytes with SHA-256 `22ccc0d50b6170bc53fb6844d2fb7ec76b8a87e720dac8d7dacf2f2a71256cb9`. The complete 5,632-byte EFI application has SHA-256 `6f3a77b6d769ed157d92dc2da95c4bb7c01f19ec704d8223c3396584a75c0ccb`.

This is the first compiler-generated Windvale boot item, not a functioning kernel and not qualification of roadmap Phase 9 or Phase 11. It does not provide a general native ABI, static-data addressing, Unicode output, arbitrary WIR lowering, a kernel stack, memory ownership, paging, traps, interrupts, processes, a bytecode runtime, clean hardware shutdown, Hyper-V evidence, or cross-host boot qualification. The native target is in the C# reference/recovery compiler; compiler self-hosting remains independent work.

## Reconsider when

- A second native source shape requires general register allocation, control-flow selection, or static-data addressing.
- The OS establishes a console or diagnostics subsystem that replaces the temporary byte adapter.
- A stable native ABI can replace the special kernel-entry wrapper without weakening handoff validation.
- Native compiler work supplies reusable target models that should subsume this bounded implementation.
- Hyper-V or real hardware cannot provide the COM1 evidence used by the first QEMU qualification.
