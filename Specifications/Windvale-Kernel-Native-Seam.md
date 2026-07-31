# Windvale kernel native implementation seam

## Status and purpose

Kernel native seam version 1 defines how reference host tools, Windvale Assembly, and Windvale source divide responsibility while the native compiler grows. The first executable seam is implemented and qualified under firmware probe version 6. [Decision 0054](../Documents/Decisions/0054-First-Wva-To-Windvale-Kernel-Seam.md) owns this boundary.

This contract prevents temporary C# machine-code generation from silently becoming the kernel architecture. It also avoids pretending that privileged x86-64 entry mechanics belong in ordinary source code.

## Implementation roles

| Layer | Current responsibility | Direction |
| --- | --- | --- |
| C# reference/recovery host | Compile, assemble, link, verify, package PE32+, provide host oracles, and retain the existing bounded loader and memory emitters while replacements are unavailable. | Remains an independent recovery and comparison path; does not define kernel policy. |
| WVA machine layer | Own explicit entry/exit shims, register-frame mechanics, and later named privileged instructions that ordinary Windvale code must not execute ambiently. | Grows only from concrete kernel requirements with exact encodings and verification rules. |
| `.wv` system layer | Own kernel policy, state transitions, diagnostics, allocation decisions, exception dispatch, and later runtime services once the native target can lower them. | Becomes the primary kernel implementation. |

New kernel mechanisms should default to WVA for irreducible machine mechanics and `.wv` for policy. Additional raw C# instruction emission requires a documented compiler or assembler blocker and a named replacement seam.

## Executable version 1 seam

`Operating-System/Kernel/X64-Main-Shim.wva` assembles through the qualified WVA 1 reference/recovery assembler into a canonical WVO object:

- exported function `Windvale_kernel_wva_main`;
- imported compiler function `Windvale_kernel_main`;
- one five-byte `jump Windvale_kernel_main` body; and
- one `relative-i32` relocation at displacement offset 1 with addend `-4`.

The kernel memory object imports `Windvale_kernel_wva_main` instead of importing compiler Main directly. After switching to the kernel-owned stack, it uses an ordinary x64 call to enter the shim. The shim performs a tail transfer without modifying registers or stack state, so compiler Main observes the same ABI it had before the seam was inserted.

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

WVA remains semantically checked assembly. Version 1 does not add arbitrary executable-byte directives, local branch labels, memory operands, or privileged instructions to WVA 1. Any such extension requires a concrete operation, exact encoding, context rules, malformed-input coverage, and synchronization of the reference and Windvale-written assemblers.

The seam does not move the UEFI loader, memory-map scanner, arena initializer, allocator, byte writer, or PE32+ packaging out of C#. It establishes the verified link and ABI position through which those pieces can be replaced without changing the loader-to-source evidence chain.

## Current evidence

The canonical WVA shim object is 158 bytes with SHA-256 `f7525da5e8365b75adc68bd2174ad5763ed05b774d861c2a0cd6aad6c0e8e1b7`. Firmware probe version 6 links five WVO objects and produces a 7,168-byte EFI application with SHA-256 `b4f557fdd39d44858ce05fd6a99b0128a791053a5d3c2aa9e68dc5b5c34a3808`.

The accepted QEMU transcript remains unchanged because the seam changes implementation ownership, not observable source behavior. Its `kernel-stack=pass` and `Hello from Windvale` lines prove that execution crossed the WVA tail transfer and reached compiler-generated `.wv` Main.
