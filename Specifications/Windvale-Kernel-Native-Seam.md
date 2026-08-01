# Windvale kernel native implementation seam

## Status and purpose

Kernel native seam version 4 defines how the shared portable-WVB native backend, reference host tools, Windvale Assembly, and system-profile Windvale source divide responsibility while the native runtime grows. The first shared ABI-5 WVB consumer remains qualified under firmware probe version 7 and [Decision 0064](../Documents/Decisions/0064-First-Shared-Native-Wvb-In-Windvale-Os.md) at exact candidate `708242e`. [Decision 0065](../Documents/Decisions/0065-Versioned-Native-Execution-Context-And-Console-Service.md) advances the current implementation to ABI 6, native bridge version 2, and firmware probe version 8; its development-QEMU gate passes and exact cross-host qualification is pending. [Decision 0056](../Documents/Decisions/0056-Windvale-Owned-Post-Memory-Evidence.md) records the qualified version-2 bidirectional WVA/WV boundary.

This contract prevents temporary C# machine-code generation from silently becoming the kernel architecture. It also avoids pretending that privileged x86-64 entry mechanics belong in ordinary source code.

## Implementation roles

| Layer | Current responsibility | Direction |
| --- | --- | --- |
| C# reference/recovery host | Compile WVB, select and verify ABI-6 native code, assemble, link, package PE32+, provide host oracles, and retain bounded loader/memory/bridge emitters while replacements are unavailable. | Remains an independent recovery and comparison path; does not define kernel policy. |
| Shared portable-WVB backend | Lower verified portable semantics into the same versioned native ABI and WVO object used by host JIT/AOT evidence. | Replaces special OS instruction selection incrementally and becomes Windvale-written. |
| WVA machine layer | Own explicit entry/exit and capability-adapter shims, register-frame mechanics, and later named privileged instructions that ordinary Windvale code must not execute ambiently. | Grows only from concrete kernel requirements with exact encodings and verification rules. |
| `.wv` system layer | Own kernel policy, state transitions, diagnostics, allocation decisions, exception dispatch, and later runtime services once the shared native target can lower them. | Becomes the primary kernel implementation. |

New kernel mechanisms should default to WVA for irreducible machine mechanics and `.wv` for policy. Additional raw C# instruction emission requires a documented compiler or assembler blocker and a named replacement seam.

## Executable WVA version 3 and bridge version 2 seam

`Operating-System/Kernel/X64-Kernel-Shims.wva` assembles through the qualified WVA 1 reference/recovery assembler into a canonical WVO object:

- exported compiler-facing function `Windvale_kernel_write_byte`;
- exported machine-entry function `Windvale_kernel_wva_main`;
- imported native bridge `Windvale_kernel_x64_native_probe`;
- imported internal machine function `Windvale_kernel_x64_write_byte`; and
- two five-byte tail jumps, each represented by a `relative-i32` relocation with addend `-4`.

The kernel memory object calls `Windvale_kernel_wva_main` after switching to the kernel-owned stack. The inbound WVA tail transfer now reaches the verified native bridge. That bridge:

1. reserves 40 aligned stack bytes and preserves the copied-handoff pointer from `RCX`;
2. constructs the exact 32-byte ABI-6 execution context with version/size, WVB instruction budget 203, call-depth budget 2, and a zero service-table pointer;
3. passes the context pointer in `RDX` and calls the ABI-6 portable export `Main`;
4. accepts only the complete packed result `RAX == 29`;
5. returns failure 1 on a trap, exhausted budget, or wrong result; and
6. restores `RCX` and tail-transfers to special compiler export `Windvale_kernel_main` on success.

The compiler object imports `Windvale_kernel_write_byte`, which resolves to the WVA export. WVA tail-transfers each call to explicitly internal symbol `Windvale_kernel_x64_write_byte`. The public kernel capability boundary is therefore WVA-owned even though its current COM1 instruction sequence remains bootstrap code.

The builder independently decodes the assembled WVA object and the bridge object and requires their exact architecture, section, symbol, code, and relocation shapes before linking. The ABI-6 selector and fragment verifier independently validate the portable module and require an empty service list before its WVO is admitted.

## Portable native probe

`Operating-System/Kernel/Native-Wvb-Probe.wv` is an ordinary portable module, not system-profile source. It owns immutable i32 array `[3, 5, 8, 13]`, a two-parameter `Add`, one bounded loop, and exported `Main() -> i32`. The reference interpreter requires exactly 203 WVB instructions and active call depth 2 to return 29. The shared native object has separate code and read-only data plus one verified RIP-relative relocation.

The probe is compiled and AOT-linked on the host. The guest does not retain, decode, verify, or JIT its WVB bytes. Reaching `native-wvb=pass` proves the preverified AOT computation returned the expected packed result on the kernel-owned stack; it is not runtime bytecode-loader evidence.

## Native compiler requirements for policy migration

ABI 6 retains i32/bool internal functions and calls, conditional control, bounded loops, exact resource counters, immutable i32 data, and bounds traps while replacing positional entry limits with the versioned execution context. Its host-side static-text service does not give the kernel a service table. Moving the allocator and future exception policy into ordinary `.wv` still requires:

- `u64` values and checked address arithmetic;
- explicit unsafe or system-visible bounded memory loads and stores;
- a specified kernel ABI for scalars, state pointers, return values, nonvolatile registers, and trap dispatch; and
- imports of narrow WVA services without granting ambient privileged instructions.

These are requirements on the native target, not permission to change source semantics implicitly. Each migrated kernel policy function must retain a C# oracle or differential test until independent native evidence is sufficient.

## Safety and migration limits

WVA remains semantically checked assembly. Version 3 does not add arbitrary executable-byte directives, local branch labels, memory operands, or privileged instructions to WVA 1. The 93-byte raw bridge exists because WVA lacks stack/register moves, comparisons, and conditional branches required by the ABI transition; it has one exact independently decoded shape and a named future replacement seam.

The seam does not move the UEFI loader, memory-map scanner, arena initializer, allocator machine implementation, COM1 instruction sequence, PE32+ packaging, or native compiler out of C#. It establishes verified link and ABI positions through which those pieces can be replaced without changing the loader-to-source evidence chain.

## Current evidence

The portable WVB remains 502 bytes with SHA-256 `1f384f77c4e1c718a331aaa1a3c1f1e4173bbae9d870ec9023d70c7b15c1f7ef`. Its current 2,360-byte ABI-6 WVO has SHA-256 `712350a395120c42f604966dffe04c397012af3696666f51c1a069cd9db0be61`. The current native bridge object is 305 bytes with SHA-256 `2e22ee17e52ee8cc2c8fa6547424f0234bd770555b0a75c23d63003b6257331e`. The version-3 WVA shim object remains 291 bytes with SHA-256 `332a0158c51e81d1beb5d212f508649c8efe2874af712d6d8ef15929ffd438fc`.

The current firmware probe version 8 links seven WVO objects and produces a deterministic 10,240-byte EFI application with SHA-256 `61cd90eac963ed96d1fdd86d447cb7f2cdeffa50a3de2f5306239de27c1be6b0`. All 15 OS tests and a real pinned development-QEMU/OVMF boot pass; the gated transcript adds `native-context=pass`. Exact committed-candidate and cross-host evidence is still pending. Exact candidate `708242e` remains the qualified version-7/ABI-5 baseline.
