# Windvale kernel native implementation seam

## Status and purpose

Kernel native seam version 10 defines how the shared portable-WVB native backend, reference host tools, Windvale Assembly, and system-profile Windvale source divide responsibility while the native runtime grows. Decisions 0064 through 0068 qualify the shared consumer through ABI 9, bridge 4, and firmware probe 11. [Decision 0069](../Documents/Decisions/0069-Dynamic-Native-Text-And-Complete-Wvdump.md) cross-host qualifies ABI 10, bridge 5, and firmware probe 12 at exact commit `7979933`. [Decision 0071](../Documents/Decisions/0071-Native-Text-Arena-And-Core-Text-Services.md) cross-host qualifies ABI 11, bridge 6, and firmware probe 13 at exact commit `8888951`. [Decision 0073](../Documents/Decisions/0073-Native-Argument-Table-And-Process-Input-Services.md) cross-host qualifies ABI 12, bridge 7, and firmware probe 14 at exact commit `328e455`. [Decision 0074](../Documents/Decisions/0074-Native-Windows-And-Linux-Output-Services.md) cross-host qualifies ABI 13, bridge 8, and firmware probe 15 at exact commit `66b273f`. [Decision 0056](../Documents/Decisions/0056-Windvale-Owned-Post-Memory-Evidence.md) records the qualified version-2 bidirectional WVA/WV boundary.

This contract prevents temporary C# machine-code generation from silently becoming the kernel architecture. It also avoids pretending that privileged x86-64 entry mechanics belong in ordinary source code.

## Implementation roles

| Layer | Current responsibility | Direction |
| --- | --- | --- |
| C# reference/recovery host | Compile WVB, select and verify ABI-13 native code, assemble, link, package PE32+, provide host oracles, and retain bounded loader/memory/bridge emitters while replacements are unavailable. | Remains an independent recovery and comparison path; does not define kernel policy. |
| Shared portable-WVB backend | Lower verified portable semantics into the same versioned native ABI and WVO object used by host JIT/AOT evidence. | Replaces special OS instruction selection incrementally and becomes Windvale-written. |
| WVA machine layer | Own explicit entry/exit and capability-adapter shims, register-frame mechanics, and later named privileged instructions that ordinary Windvale code must not execute ambiently. | Grows only from concrete kernel requirements with exact encodings and verification rules. |
| `.wv` system layer | Own kernel policy, state transitions, diagnostics, allocation decisions, exception dispatch, and later runtime services once the shared native target can lower them. | Becomes the primary kernel implementation. |

New kernel mechanisms should default to WVA for irreducible machine mechanics and `.wv` for policy. Additional raw C# instruction emission requires a documented compiler or assembler blocker and a named replacement seam.

## Executable WVA version 3, bridge version 8, and portable probe version 5 seam

`Operating-System/Kernel/X64-Kernel-Shims.wva` assembles through the qualified WVA 1 reference/recovery assembler into a canonical WVO object:

- exported compiler-facing function `Windvale_kernel_write_byte`;
- exported machine-entry function `Windvale_kernel_wva_main`;
- imported native bridge `Windvale_kernel_x64_native_probe`;
- imported internal machine function `Windvale_kernel_x64_write_byte`; and
- two five-byte tail jumps, each represented by a `relative-i32` relocation with addend `-4`.

The kernel memory object calls `Windvale_kernel_wva_main` after switching to the kernel-owned stack. The inbound WVA tail transfer now reaches the verified native bridge. That bridge:

1. reserves 104 aligned stack bytes and preserves the copied-handoff pointer from `RCX`;
2. constructs the exact 96-byte execution context with version/size, WVB instruction budget 271, call-depth budget 2, zero service-table, record-arena, text-arena, service-failure-detail, argument-table/count, output-table, and reserved fields;
3. passes the context pointer in `RDX` and calls the ABI-13 portable export `Main`;
4. accepts only the complete packed result `RAX == 29`;
5. returns failure 1 on a trap, exhausted budget, or wrong result; and
6. restores `RCX` and tail-transfers to special compiler export `Windvale_kernel_main` on success.

The compiler object imports `Windvale_kernel_write_byte`, which resolves to the WVA export. WVA tail-transfers each call to explicitly internal symbol `Windvale_kernel_x64_write_byte`. The public kernel capability boundary is therefore WVA-owned even though its current COM1 instruction sequence remains bootstrap code.

The builder independently decodes the assembled WVA object and the bridge object and requires their exact architecture, section, symbol, code, and relocation shapes before linking. The ABI-13 selector and fragment verifier independently validate the portable module, borrowed-byte descriptors and reads, empty service list, and unused zero-length record/text/argument/output resources before its WVO is admitted.

## Portable native probe

`Operating-System/Kernel/Native-Wvb-Probe.wv` is an ordinary portable module, not system-profile source. It owns immutable i32 array `[3, 5, 8, 13]`, immutable byte header `[0, 5, 0, 0, 0, 255]`, a two-parameter `Add`, borrowed-byte `Readˉheader`, one bounded loop, and exported `Main() -> i32`. It passes bytes through an internal function, slices a four-byte payload, performs checked `u8` and little-endian `u32` reads, and validates the values before accepting the sum. The reference interpreter requires exactly 271 WVB instructions and active call depth 2 to return 29. The shared native object has separate code and read-only data plus four verified RIP-relative relocations.

The probe is compiled and AOT-linked on the host. The guest does not retain, decode, verify, or JIT its WVB bytes. Reaching `native-wvb=pass` proves the preverified AOT computation returned the expected packed result on the kernel-owned stack; it is not runtime bytecode-loader evidence.

## Native compiler requirements for policy migration

ABI 13 retains those portable semantics and immutable argument access while adding explicit host-side output-table ownership and exact Windows/Linux output leaves. The kernel still passes zero service-table, record/text-arena, argument, and output fields because its current module uses none of those facilities. Moving an allocator and future exception policy into ordinary `.wv` still requires:

- `u64` values and checked address arithmetic;
- explicit unsafe or system-visible bounded memory loads and stores;
- a specified kernel ABI for scalars, state pointers, return values, nonvolatile registers, and trap dispatch; and
- imports of narrow WVA services without granting ambient privileged instructions.

These are requirements on the native target, not permission to change source semantics implicitly. Each migrated kernel policy function must retain a C# oracle or differential test until independent native evidence is sufficient.

## Safety and migration limits

WVA remains semantically checked assembly. Version 3 does not add arbitrary executable-byte directives, local branch labels, memory operands, or privileged instructions to WVA 1. The current 133-byte raw bridge exists because WVA lacks stack/register moves, comparisons, and conditional branches required by the ABI transition; it has one exact independently decoded shape and a named future replacement seam.

The seam does not move the UEFI loader, memory-map scanner, arena initializer, allocator machine implementation, COM1 instruction sequence, PE32+ packaging, or native compiler out of C#. The raw bridge establishes verified link and ABI positions through which those pieces can be replaced without changing the loader-to-source evidence chain.

## Current evidence

The portable WVB is 929 bytes with SHA-256 `0653613d868abbba99b5e31230fb2a1f92581c4989318577cb77a6d6e60f8339`. Its 7,882-byte service-free ABI-8 WVO remains byte-identical with SHA-256 `24f5359fa5d335eb273e8680c671924dda43d4ee4c6e00c95f1bbebc742dfa99`. The 305-byte native bridge object has SHA-256 `ef8993c59eb816c7983c5b8033922231baf1d897f846a8d5e54d8232677ef75a`; its layout remains version 2. The version-3 WVA shim object remains 291 bytes with SHA-256 `332a0158c51e81d1beb5d212f508649c8efe2874af712d6d8ef15929ffd438fc`.

Firmware probe version 9 links seven WVO objects and produces a deterministic 15,360-byte EFI application with SHA-256 `ac92cd4759961c7a046ede49af8dce7626016fbcf8bb46e7d90027f5974bffa4`. Exact candidate `8d375bf` passes all 15 OS tests on Windows and Debian and a real pinned QEMU/OVMF boot; the gated transcript starts `windvale-os-boot 9`. The shared ABI-7 backend passes complete Windows/Debian qualification at the same candidate.

Firmware probe version 10 rebuilds that service-free module through ABI 8 and produces a deterministic 15,872-byte EFI application with SHA-256 `9228995f3b2522e15bd87ca63dc2637cc290f93b37f3e32b24cd8e3906671b75`. Exact candidate `d970c27` passes all 15 OS tests on Windows and Debian and the real pinned QEMU/OVMF boot; the gated transcript starts `windvale-os-boot 10`. The shared ABI-8 backend passes complete Windows/Debian qualification at the same candidate.

Exact candidate `7edc243` rebuilds the portable WVO through ABI 9 to 7,946 bytes with SHA-256 `94d10d60f5d37cdab3d0f0c3678ee1e86b312564c06d633baf4736939595fed2`. Its 315-byte bridge object has SHA-256 `949c5fdd641722c541e2ed6583a5c28bd681b77d91f28ea5cda2999026d75a23`. Firmware probe 11 remains 15,872 bytes and has SHA-256 `100070e26666bfc97d0fff8da42d996249d072b5771014838a376abfc0a13d6a`; all 15 OS tests pass on Windows and Debian, the shared ABI-9 backend passes complete cross-host qualification, and the real pinned QEMU/OVMF boot emits `windvale-os-boot 11` through `status=pass` before guest-controlled host exit code 1.

Exact commit `7979933` rebuilds the portable WVO through ABI 10 to 8,010 bytes with SHA-256 `f3d0d2aec5b7fb81d02e4188fb6ba48b6a21dc91c89bdf7f00daaf7b0a981038`. Its unchanged 315-byte bridge object retains SHA-256 `949c5fdd641722c541e2ed6583a5c28bd681b77d91f28ea5cda2999026d75a23`. Firmware probe 12 remains 15,872 bytes with SHA-256 `3010bc72b9c26386f062f78481c900cac841321b040b41447a0bbb65a9e392fe`; all 15 OS tests pass on Windows and Debian, the shared ABI-10 backend passes complete cross-host qualification, and pinned QEMU/OVMF on Windows emits `windvale-os-boot 12` through `status=pass` before guest-controlled host exit code 1. The Debian QA host does not currently provide QEMU, so no duplicate Debian emulator run is claimed.

Exact commit `8888951` keeps the portable WVB and 8,010-byte service-free WVO byte-identical while rebuilding the context constructor for ABI 11. Its 118-byte bridge code produces a 330-byte object with SHA-256 `8b28ed85af29baa65810e0ed0ce8e2893e9696cebd666ccf72a1a53f68cde2b9`. Firmware probe 13 remains 15,872 bytes with SHA-256 `ceffc3e33bf007e47b109f3b6a71db2fdceac3c0e908d1471f056909ee42532d`; both hosts pass all 15 OS tests, the shared ABI-11 backend passes complete cross-host qualification, and pinned QEMU/OVMF on Windows emits `windvale-os-boot 13` through `status=pass` before guest-controlled host exit code 1. The Debian QA host does not provide QEMU.

Exact Decision 0073 commit `328e455` keeps the portable WVB and 8,010-byte service-free WVO byte-identical while rebuilding the context constructor for ABI 12. Its 128-byte bridge code produces a 340-byte object with SHA-256 `0f6d4f00e6a66c23dedc7c6224cdae3f556c5d1c0ff927e596c927a73fd9829f`. Firmware probe 14 remains 15,872 bytes with SHA-256 `aadfbc5cb56f6afea94605ad31ee6af6a60b1e821403dfb8e1c2550631b6d548`; both hosts pass all 15 OS tests and the pinned-QEMU gate passes on Windows.

Exact Decision 0074 commit `66b273f` again keeps the portable WVB and service-free WVO byte-identical while rebuilding the complete context constructor for ABI 13. Its 133-byte bridge code produces a 345-byte object with SHA-256 `0a0393457200dbf5ecfbb667c6c283510a6eb13a3e7e77537a0b6d8e0f503d68`. Firmware probe 15 remains 15,872 bytes with SHA-256 `d716b77a91646da6b423bacb1faa6d70f5a097241c610fe49291b068f33d5f29`; both hosts pass all 15 OS tests and the pinned-QEMU gate passes on Windows.
