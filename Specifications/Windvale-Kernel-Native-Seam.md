# Windvale kernel native implementation seam

## Status and purpose

Kernel native seam version 5 defines how the shared portable-WVB native backend, reference host tools, Windvale Assembly, and system-profile Windvale source divide responsibility while the native runtime grows. The first shared ABI-5 WVB consumer remains historical qualified evidence under firmware probe version 7 and [Decision 0064](../Documents/Decisions/0064-First-Shared-Native-Wvb-In-Windvale-Os.md). [Decision 0065](../Documents/Decisions/0065-Versioned-Native-Execution-Context-And-Console-Service.md) cross-host qualifies ABI 6, native bridge version 2, and firmware probe version 8 at exact candidate `2fcf531`. [Decision 0066](../Documents/Decisions/0066-Borrowed-Bytes-And-Unsigned-Native-Values.md) cross-host qualifies the current ABI 7, portable probe version 3, and firmware probe version 9 at exact candidate `8d375bf`. [Decision 0056](../Documents/Decisions/0056-Windvale-Owned-Post-Memory-Evidence.md) records the qualified version-2 bidirectional WVA/WV boundary.

This contract prevents temporary C# machine-code generation from silently becoming the kernel architecture. It also avoids pretending that privileged x86-64 entry mechanics belong in ordinary source code.

## Implementation roles

| Layer | Current responsibility | Direction |
| --- | --- | --- |
| C# reference/recovery host | Compile WVB, select and verify ABI-7 native code, assemble, link, package PE32+, provide host oracles, and retain bounded loader/memory/bridge emitters while replacements are unavailable. | Remains an independent recovery and comparison path; does not define kernel policy. |
| Shared portable-WVB backend | Lower verified portable semantics into the same versioned native ABI and WVO object used by host JIT/AOT evidence. | Replaces special OS instruction selection incrementally and becomes Windvale-written. |
| WVA machine layer | Own explicit entry/exit and capability-adapter shims, register-frame mechanics, and later named privileged instructions that ordinary Windvale code must not execute ambiently. | Grows only from concrete kernel requirements with exact encodings and verification rules. |
| `.wv` system layer | Own kernel policy, state transitions, diagnostics, allocation decisions, exception dispatch, and later runtime services once the shared native target can lower them. | Becomes the primary kernel implementation. |

New kernel mechanisms should default to WVA for irreducible machine mechanics and `.wv` for policy. Additional raw C# instruction emission requires a documented compiler or assembler blocker and a named replacement seam.

## Executable WVA version 3, bridge version 2, and portable probe version 3 seam

`Operating-System/Kernel/X64-Kernel-Shims.wva` assembles through the qualified WVA 1 reference/recovery assembler into a canonical WVO object:

- exported compiler-facing function `Windvale_kernel_write_byte`;
- exported machine-entry function `Windvale_kernel_wva_main`;
- imported native bridge `Windvale_kernel_x64_native_probe`;
- imported internal machine function `Windvale_kernel_x64_write_byte`; and
- two five-byte tail jumps, each represented by a `relative-i32` relocation with addend `-4`.

The kernel memory object calls `Windvale_kernel_wva_main` after switching to the kernel-owned stack. The inbound WVA tail transfer now reaches the verified native bridge. That bridge:

1. reserves 40 aligned stack bytes and preserves the copied-handoff pointer from `RCX`;
2. constructs the exact 32-byte execution context with version/size, WVB instruction budget 271, call-depth budget 2, and a zero service-table pointer;
3. passes the context pointer in `RDX` and calls the ABI-7 portable export `Main`;
4. accepts only the complete packed result `RAX == 29`;
5. returns failure 1 on a trap, exhausted budget, or wrong result; and
6. restores `RCX` and tail-transfers to special compiler export `Windvale_kernel_main` on success.

The compiler object imports `Windvale_kernel_write_byte`, which resolves to the WVA export. WVA tail-transfers each call to explicitly internal symbol `Windvale_kernel_x64_write_byte`. The public kernel capability boundary is therefore WVA-owned even though its current COM1 instruction sequence remains bootstrap code.

The builder independently decodes the assembled WVA object and the bridge object and requires their exact architecture, section, symbol, code, and relocation shapes before linking. The ABI-7 selector and fragment verifier independently validate the portable module, borrowed-byte descriptors and reads, and empty service list before its WVO is admitted.

## Portable native probe

`Operating-System/Kernel/Native-Wvb-Probe.wv` is an ordinary portable module, not system-profile source. It owns immutable i32 array `[3, 5, 8, 13]`, immutable byte header `[0, 5, 0, 0, 0, 255]`, a two-parameter `Add`, borrowed-byte `Readˉheader`, one bounded loop, and exported `Main() -> i32`. It passes bytes through an internal function, slices a four-byte payload, performs checked `u8` and little-endian `u32` reads, and validates the values before accepting the sum. The reference interpreter requires exactly 271 WVB instructions and active call depth 2 to return 29. The shared native object has separate code and read-only data plus four verified RIP-relative relocations.

The probe is compiled and AOT-linked on the host. The guest does not retain, decode, verify, or JIT its WVB bytes. Reaching `native-wvb=pass` proves the preverified AOT computation returned the expected packed result on the kernel-owned stack; it is not runtime bytecode-loader evidence.

## Native compiler requirements for policy migration

ABI 7 retains i32/bool internal functions and calls, conditional control, bounded loops, exact resource counters, immutable i32 data, and bounds traps while adding borrowed immutable bytes, `u8`/`u32`, slicing, fixed-width reads, and byte parameters. Its host-side static-text service does not give the kernel a service table. Moving the allocator and future exception policy into ordinary `.wv` still requires:

- `u64` values and checked address arithmetic;
- explicit unsafe or system-visible bounded memory loads and stores;
- a specified kernel ABI for scalars, state pointers, return values, nonvolatile registers, and trap dispatch; and
- imports of narrow WVA services without granting ambient privileged instructions.

These are requirements on the native target, not permission to change source semantics implicitly. Each migrated kernel policy function must retain a C# oracle or differential test until independent native evidence is sufficient.

## Safety and migration limits

WVA remains semantically checked assembly. Version 3 does not add arbitrary executable-byte directives, local branch labels, memory operands, or privileged instructions to WVA 1. The 93-byte raw bridge exists because WVA lacks stack/register moves, comparisons, and conditional branches required by the ABI transition; it has one exact independently decoded shape and a named future replacement seam.

The seam does not move the UEFI loader, memory-map scanner, arena initializer, allocator machine implementation, COM1 instruction sequence, PE32+ packaging, or native compiler out of C#. It establishes verified link and ABI positions through which those pieces can be replaced without changing the loader-to-source evidence chain.

## Current evidence

The portable WVB is 929 bytes with SHA-256 `0653613d868abbba99b5e31230fb2a1f92581c4989318577cb77a6d6e60f8339`. Its current 7,882-byte ABI-7 WVO has SHA-256 `24f5359fa5d335eb273e8680c671924dda43d4ee4c6e00c95f1bbebc742dfa99`. The 305-byte native bridge object has SHA-256 `ef8993c59eb816c7983c5b8033922231baf1d897f846a8d5e54d8232677ef75a`; its layout remains version 2, while its exact budget immediate changed. The version-3 WVA shim object remains 291 bytes with SHA-256 `332a0158c51e81d1beb5d212f508649c8efe2874af712d6d8ef15929ffd438fc`.

Firmware probe version 9 links seven WVO objects and produces a deterministic 15,360-byte EFI application with SHA-256 `ac92cd4759961c7a046ede49af8dce7626016fbcf8bb46e7d90027f5974bffa4`. Exact candidate `8d375bf` passes all 15 OS tests on Windows and Debian and a real pinned QEMU/OVMF boot; the gated transcript starts `windvale-os-boot 9`. The shared ABI-7 backend passes complete Windows/Debian qualification at the same candidate.
