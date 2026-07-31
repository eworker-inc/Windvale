# Windvale kernel native implementation seam

## Status and purpose

Kernel native seam version 2 defines how reference host tools, Windvale Assembly, and Windvale source divide responsibility while the native compiler grows. The bidirectional executable seam and source-owned memory-through-Hello evidence are implemented and qualified under firmware probe version 6. [Decision 0056](../Documents/Decisions/0056-Windvale-Owned-Post-Memory-Evidence.md) owns the current boundary; [Decision 0054](../Documents/Decisions/0054-First-Wva-To-Windvale-Kernel-Seam.md) records the initial one-way shape.

This contract prevents temporary C# machine-code generation from silently becoming the kernel architecture. It also avoids pretending that privileged x86-64 entry mechanics belong in ordinary source code.

## Implementation roles

| Layer | Current responsibility | Direction |
| --- | --- | --- |
| C# reference/recovery host | Compile, assemble, link, verify, package PE32+, provide host oracles, and retain the existing bounded loader and memory emitters while replacements are unavailable. | Remains an independent recovery and comparison path; does not define kernel policy. |
| WVA machine layer | Own explicit entry/exit and capability-adapter shims, register-frame mechanics, and later named privileged instructions that ordinary Windvale code must not execute ambiently. | Grows only from concrete kernel requirements with exact encodings and verification rules. |
| `.wv` system layer | Own kernel policy, state transitions, diagnostics, allocation decisions, exception dispatch, and later runtime services once the native target can lower them. | Becomes the primary kernel implementation. |

New kernel mechanisms should default to WVA for irreducible machine mechanics and `.wv` for policy. Additional raw C# instruction emission requires a documented compiler or assembler blocker and a named replacement seam.

## Executable version 2 seam

`Operating-System/Kernel/X64-Kernel-Shims.wva` assembles through the qualified WVA 1 reference/recovery assembler into a canonical WVO object:

- exported compiler-facing function `Windvale_kernel_write_byte`;
- exported machine-entry function `Windvale_kernel_wva_main`;
- imported compiler function `Windvale_kernel_main`;
- imported internal machine function `Windvale_kernel_x64_write_byte`; and
- two five-byte tail jumps, each represented by a `relative-i32` relocation with addend `-4`.

The kernel memory object imports `Windvale_kernel_wva_main` instead of importing compiler Main directly. After switching to the kernel-owned stack, it uses an ordinary x64 call to enter the shim. The shim performs a tail transfer without modifying registers or stack state, so compiler Main observes the same ABI it had before the seam was inserted.

The compiler object imports `Windvale_kernel_write_byte`, which now resolves to the WVA export rather than directly to the C# adapter. WVA tail-transfers each call to explicitly internal symbol `Windvale_kernel_x64_write_byte`. The public kernel capability boundary is therefore WVA-owned even though its current COM1 instruction sequence remains bootstrap code.

The builder independently decodes the assembled object and requires the exact architecture, section, symbol, code, and relocation shape before linking it. The linker then resolves the WVA import of `Windvale_kernel_main` against the compiler-produced WVO.

## Native compiler requirements for policy migration

Moving the current allocator and future exception policy into `.wv` requires a bounded native subset with:

- internal function definitions and calls;
- conditional control flow and bounded loops;
- `u64` values and checked address arithmetic;
- explicit unsafe or system-visible bounded memory loads and stores;
- a specified kernel call ABI for scalars, state pointers, return values, and nonvolatile registers; and
- imports of narrow WVA services without granting ambient privileged instructions.

These are requirements on the native target, not permission to change source semantics implicitly. The compiler may land them incrementally, and each migrated kernel policy function must retain a C# oracle or differential test until independent native evidence is sufficient.

## Safety and migration limits

WVA remains semantically checked assembly. Version 2 does not add arbitrary executable-byte directives, local branch labels, memory operands, or privileged instructions to WVA 1. Any such extension requires a concrete operation, exact encoding, context rules, malformed-input coverage, and synchronization of the reference and Windvale-written assemblers.

The seam moves memory, allocator, stack, and Hello World line selection into `.wv` and both public kernel symbol boundaries into WVA. Loader continuation and final-status markers remain loader-owned. The seam does not move the UEFI loader, memory-map scanner, arena initializer, allocator machine implementation, COM1 instruction sequence, or PE32+ packaging out of C#. It establishes the verified link and ABI positions through which those pieces can be replaced without changing the loader-to-source evidence chain.

## Current evidence

The canonical WVA shim object is 279 bytes with SHA-256 `36ea8c6ebcd5e1ef51ff332344aa549a8ec7aadaf485d44306ee63d5b41d4123`. The compiler object containing the success lines from `memory-owned=pass` through `Hello from Windvale` is 2,564 bytes with SHA-256 `f2c28eb5f020f59b8acb480fc8dc62e393ebb14405b3c12ecb05076176d44420`. Firmware probe version 6 links five WVO objects and produces a 7,168-byte EFI application with SHA-256 `92ad46700b058cd3a8846c59c227a33ef3832b080fb408e8eee42dc301336d9a`.

The accepted QEMU transcript remains unchanged because version 2 changes implementation ownership, not observable behavior. Every line from `memory-owned=pass` through Hello World proves that execution crossed the inbound WVA tail transfer and reached compiler-generated `.wv` Main; every byte of those lines then crosses the outbound WVA adapter.
