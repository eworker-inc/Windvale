# Windvale kernel native implementation seam

## Status and purpose

Kernel native seam version 6 defines how the shared portable-WVB native backend, reference host tools, Windvale Assembly, and system-profile Windvale source divide responsibility while the native runtime grows. The first shared ABI-5 WVB consumer remains historical qualified evidence under firmware probe version 7 and [Decision 0064](../Documents/Decisions/0064-First-Shared-Native-Wvb-In-Windvale-Os.md). [Decision 0065](../Documents/Decisions/0065-Versioned-Native-Execution-Context-And-Console-Service.md) cross-host qualifies ABI 6 and firmware probe 8; [Decision 0066](../Documents/Decisions/0066-Borrowed-Bytes-And-Unsigned-Native-Values.md) qualifies ABI 7 and probe 9; [Decision 0067](../Documents/Decisions/0067-Borrowed-Hosted-Input-And-First-Native-Wvb-Inspector.md) qualifies ABI 8 and probe 10; [Decision 0068](../Documents/Decisions/0068-Bounded-Native-Nominal-Values-And-Wvdump-Structural-Core.md) qualifies ABI 9, bridge 4, and firmware probe 11. [Decision 0056](../Documents/Decisions/0056-Windvale-Owned-Post-Memory-Evidence.md) records the qualified version-2 bidirectional WVA/WV boundary.

This contract prevents temporary C# machine-code generation from silently becoming the kernel architecture. It also avoids pretending that privileged x86-64 entry mechanics belong in ordinary source code.

## Implementation roles

| Layer | Current responsibility | Direction |
| --- | --- | --- |
| C# reference/recovery host | Compile WVB, select and verify ABI-9 native code, assemble, link, package PE32+, provide host oracles, and retain bounded loader/memory/bridge emitters while replacements are unavailable. | Remains an independent recovery and comparison path; does not define kernel policy. |
| Shared portable-WVB backend | Lower verified portable semantics into the same versioned native ABI and WVO object used by host JIT/AOT evidence. | Replaces special OS instruction selection incrementally and becomes Windvale-written. |
| WVA machine layer | Own explicit entry/exit and capability-adapter shims, register-frame mechanics, and later named privileged instructions that ordinary Windvale code must not execute ambiently. | Grows only from concrete kernel requirements with exact encodings and verification rules. |
| `.wv` system layer | Own kernel policy, state transitions, diagnostics, allocation decisions, exception dispatch, and later runtime services once the shared native target can lower them. | Becomes the primary kernel implementation. |

New kernel mechanisms should default to WVA for irreducible machine mechanics and `.wv` for policy. Additional raw C# instruction emission requires a documented compiler or assembler blocker and a named replacement seam.

## Executable WVA version 3, bridge version 4, and portable probe version 4 seam

`Operating-System/Kernel/X64-Kernel-Shims.wva` assembles through the qualified WVA 1 reference/recovery assembler into a canonical WVO object:

- exported compiler-facing function `Windvale_kernel_write_byte`;
- exported machine-entry function `Windvale_kernel_wva_main`;
- imported native bridge `Windvale_kernel_x64_native_probe`;
- imported internal machine function `Windvale_kernel_x64_write_byte`; and
- two five-byte tail jumps, each represented by a `relative-i32` relocation with addend `-4`.

The kernel memory object calls `Windvale_kernel_wva_main` after switching to the kernel-owned stack. The inbound WVA tail transfer now reaches the verified native bridge. That bridge:

1. reserves 56 aligned stack bytes and preserves the copied-handoff pointer from `RCX`;
2. constructs the exact 48-byte execution context with version/size, WVB instruction budget 271, call-depth budget 2, a zero service-table pointer, and a zero-length record arena;
3. passes the context pointer in `RDX` and calls the ABI-9 portable export `Main`;
4. accepts only the complete packed result `RAX == 29`;
5. returns failure 1 on a trap, exhausted budget, or wrong result; and
6. restores `RCX` and tail-transfers to special compiler export `Windvale_kernel_main` on success.

The compiler object imports `Windvale_kernel_write_byte`, which resolves to the WVA export. WVA tail-transfers each call to explicitly internal symbol `Windvale_kernel_x64_write_byte`. The public kernel capability boundary is therefore WVA-owned even though its current COM1 instruction sequence remains bootstrap code.

The builder independently decodes the assembled WVA object and the bridge object and requires their exact architecture, section, symbol, code, and relocation shapes before linking. The ABI-9 selector and fragment verifier independently validate the portable module, borrowed-byte descriptors and reads, empty service list, and unused zero-length record arena before its WVO is admitted.

## Portable native probe

`Operating-System/Kernel/Native-Wvb-Probe.wv` is an ordinary portable module, not system-profile source. It owns immutable i32 array `[3, 5, 8, 13]`, immutable byte header `[0, 5, 0, 0, 0, 255]`, a two-parameter `Add`, borrowed-byte `Readˉheader`, one bounded loop, and exported `Main() -> i32`. It passes bytes through an internal function, slices a four-byte payload, performs checked `u8` and little-endian `u32` reads, and validates the values before accepting the sum. The reference interpreter requires exactly 271 WVB instructions and active call depth 2 to return 29. The shared native object has separate code and read-only data plus four verified RIP-relative relocations.

The probe is compiled and AOT-linked on the host. The guest does not retain, decode, verify, or JIT its WVB bytes. Reaching `native-wvb=pass` proves the preverified AOT computation returned the expected packed result on the kernel-owned stack; it is not runtime bytecode-loader evidence.

## Native compiler requirements for policy migration

ABI 9 retains those portable semantics while adding host-side enums, immutable records, and pure UTF-8 validation. The kernel still passes a zero service-table pointer and a zero-length record arena because its current module uses neither facility. Moving the allocator and future exception policy into ordinary `.wv` still requires:

- `u64` values and checked address arithmetic;
- explicit unsafe or system-visible bounded memory loads and stores;
- a specified kernel ABI for scalars, state pointers, return values, nonvolatile registers, and trap dispatch; and
- imports of narrow WVA services without granting ambient privileged instructions.

These are requirements on the native target, not permission to change source semantics implicitly. Each migrated kernel policy function must retain a C# oracle or differential test until independent native evidence is sufficient.

## Safety and migration limits

WVA remains semantically checked assembly. Version 3 does not add arbitrary executable-byte directives, local branch labels, memory operands, or privileged instructions to WVA 1. The 103-byte raw bridge exists because WVA lacks stack/register moves, comparisons, and conditional branches required by the ABI transition; it has one exact independently decoded shape and a named future replacement seam.

The seam does not move the UEFI loader, memory-map scanner, arena initializer, allocator machine implementation, COM1 instruction sequence, PE32+ packaging, or native compiler out of C#. It establishes verified link and ABI positions through which those pieces can be replaced without changing the loader-to-source evidence chain.

## Current evidence

The portable WVB is 929 bytes with SHA-256 `0653613d868abbba99b5e31230fb2a1f92581c4989318577cb77a6d6e60f8339`. Its 7,882-byte service-free ABI-8 WVO remains byte-identical with SHA-256 `24f5359fa5d335eb273e8680c671924dda43d4ee4c6e00c95f1bbebc742dfa99`. The 305-byte native bridge object has SHA-256 `ef8993c59eb816c7983c5b8033922231baf1d897f846a8d5e54d8232677ef75a`; its layout remains version 2. The version-3 WVA shim object remains 291 bytes with SHA-256 `332a0158c51e81d1beb5d212f508649c8efe2874af712d6d8ef15929ffd438fc`.

Firmware probe version 9 links seven WVO objects and produces a deterministic 15,360-byte EFI application with SHA-256 `ac92cd4759961c7a046ede49af8dce7626016fbcf8bb46e7d90027f5974bffa4`. Exact candidate `8d375bf` passes all 15 OS tests on Windows and Debian and a real pinned QEMU/OVMF boot; the gated transcript starts `windvale-os-boot 9`. The shared ABI-7 backend passes complete Windows/Debian qualification at the same candidate.

Firmware probe version 10 rebuilds that service-free module through ABI 8 and produces a deterministic 15,872-byte EFI application with SHA-256 `9228995f3b2522e15bd87ca63dc2637cc290f93b37f3e32b24cd8e3906671b75`. Exact candidate `d970c27` passes all 15 OS tests on Windows and Debian and the real pinned QEMU/OVMF boot; the gated transcript starts `windvale-os-boot 10`. The shared ABI-8 backend passes complete Windows/Debian qualification at the same candidate.

Exact candidate `7edc243` rebuilds the portable WVO through ABI 9 to 7,946 bytes with SHA-256 `94d10d60f5d37cdab3d0f0c3678ee1e86b312564c06d633baf4736939595fed2`. Its 315-byte bridge object has SHA-256 `949c5fdd641722c541e2ed6583a5c28bd681b77d91f28ea5cda2999026d75a23`. Firmware probe 11 remains 15,872 bytes and has SHA-256 `100070e26666bfc97d0fff8da42d996249d072b5771014838a376abfc0a13d6a`; all 15 OS tests pass on Windows and Debian, the shared ABI-9 backend passes complete cross-host qualification, and the real pinned QEMU/OVMF boot emits `windvale-os-boot 11` through `status=pass` before guest-controlled host exit code 1.
