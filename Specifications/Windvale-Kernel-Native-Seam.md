# Windvale kernel native implementation seam

## Status and purpose

Kernel native seam version 12 is cross-host qualified for firmware probe 17 at exact commit `ba2cf69cd4a97876f5e953b3938d032fc75a8ff7`. It retains the qualified ABI-14, bridge-9, and executable-WVA boundary and adds one exact Stage 0 object for the first kernel-owned terminal CPU exception. Decisions 0064 through 0076 cross-host qualify seam version 11 through firmware probe 16; [Decision 0081](../Documents/Decisions/0081-First-Terminal-X64-Cpu-Exception-Boundary.md) owns version 12. [Decision 0056](../Documents/Decisions/0056-Windvale-Owned-Post-Memory-Evidence.md) records the qualified version-2 bidirectional WVA/WV boundary.

This contract prevents temporary C# machine-code generation from silently becoming the kernel architecture. It also avoids pretending that privileged x86-64 entry mechanics belong in ordinary source code.

## Implementation roles

| Layer | Current responsibility | Direction |
| --- | --- | --- |
| C# reference/recovery host | Compile WVB, select and verify ABI-14 native code, assemble, link, package PE32+, provide host oracles, and retain bounded loader, memory, exception, and bridge emitters while replacements are unavailable. | Remains an independent recovery and comparison path; does not define kernel policy. |
| Shared portable-WVB backend | Lower verified portable semantics into the same versioned native ABI and WVO object used by host JIT/AOT evidence. | Replaces special OS instruction selection incrementally and becomes Windvale-written. |
| WVA machine layer | Own explicit entry/exit and capability-adapter shims, register-frame mechanics, and later named privileged instructions that ordinary Windvale code must not execute ambiently. | Grows only from concrete kernel requirements with exact encodings and verification rules. |
| `.wv` system layer | Own kernel policy, state transitions, diagnostics, allocation decisions, exception dispatch, and later runtime services once the shared native target can lower them. | Becomes the primary kernel implementation. |

New kernel mechanisms should default to WVA for irreducible machine mechanics and `.wv` for policy. Additional raw C# instruction emission requires a documented compiler or assembler blocker and a named replacement seam.

## Executable WVA version 3, bridge version 9, and portable probe version 5 seam

`Operating-System/Kernel/X64-Kernel-Shims.wva` assembles through the qualified WVA 1 reference/recovery assembler into a canonical WVO object:

- exported compiler-facing function `Windvale_kernel_write_byte`;
- exported machine-entry function `Windvale_kernel_wva_main`;
- imported native bridge `Windvale_kernel_x64_native_probe`;
- imported internal machine function `Windvale_kernel_x64_write_byte`; and
- two five-byte tail jumps, each represented by a `relative-i32` relocation with addend `-4`.

The kernel memory object calls `Windvale_kernel_wva_main` after switching to the kernel-owned stack. The inbound WVA tail transfer now reaches the verified native bridge. That bridge:

1. reserves 120 aligned stack bytes and preserves the copied-handoff pointer from `RCX`;
2. constructs the exact 104-byte execution context with version/size, WVB instruction budget 271, call-depth budget 2, zero service-table, record-arena, text-arena, service-failure-detail, argument-table/count, output-table, file-input-table, and reserved fields;
3. passes the context pointer in `RDX` and calls the ABI-14 portable export `Main`;
4. accepts only the complete packed result `RAX == 29`;
5. returns failure 1 on a trap, exhausted budget, or wrong result; and
6. restores `RCX` and tail-transfers to special compiler export `Windvale_kernel_main` on success.

The compiler object imports `Windvale_kernel_write_byte`, which resolves to the WVA export. WVA tail-transfers each call to explicitly internal symbol `Windvale_kernel_x64_write_byte`. The public kernel capability boundary is therefore WVA-owned even though its current COM1 instruction sequence remains bootstrap code.

The builder independently decodes the assembled WVA object and the bridge object and requires their exact architecture, section, symbol, code, and relocation shapes before linking. The ABI-14 selector and fragment verifier independently validate the portable module, borrowed-byte descriptors and reads, empty service list, and unused zero-length record/text/argument/output/file resources before its WVO is admitted.

## Portable native probe

`Operating-System/Kernel/Native-Wvb-Probe.wv` is an ordinary portable module, not system-profile source. It owns immutable i32 array `[3, 5, 8, 13]`, immutable byte header `[0, 5, 0, 0, 0, 255]`, a two-parameter `Add`, borrowed-byte `Readˉheader`, one bounded loop, and exported `Main() -> i32`. It passes bytes through an internal function, slices a four-byte payload, performs checked `u8` and little-endian `u32` reads, and validates the values before accepting the sum. The reference interpreter requires exactly 271 WVB instructions and active call depth 2 to return 29. The shared native object has separate code and read-only data plus four verified RIP-relative relocations.

The probe is compiled and AOT-linked on the host. The guest does not retain, decode, verify, or JIT its WVB bytes. Reaching `native-wvb=pass` proves the preverified AOT computation returned the expected packed result on the kernel-owned stack; it is not runtime bytecode-loader evidence.

## Terminal CPU exception seam

Probe 17 adds one exact x86-64 exception object between kernel-memory initialization and the existing WVA/native Main path. Kernel memory supplies its already-zeroed first allocation and switches to the owned stack. The exception object disables maskable interrupts, reads live `CS`, constructs the complete vector-6 interrupt gate in that page, and executes `LIDT`. The normal variant returns so the existing Main chain runs. After Main returns, the explicit invalid-opcode variant executes one `UD2`; its terminal handler writes the fixed panic suffix and QEMU failure value, then uses CLI/HLT if the test device does not complete.

This raw object is an explicit replacement seam, not new portable semantics or a general kernel ABI. WVA 1 cannot express live segment-register capture, checked descriptor memory writes, `CLI`, `LIDT`, or the terminal exception entry. Extending WVA requires named instructions and independent encoding rules; exception dispatch policy moves to system-profile `.wv` only after bounded unsafe memory and a kernel call convention are specified. [Windvale-Kernel-Exceptions.md](Windvale-Kernel-Exceptions.md) owns the exact version-1 table, handler, scenarios, validation boundary, and exclusions.

## Native compiler requirements for policy migration

ABI 14 retains those portable semantics and adds exact Windows/Linux file-input leaves to ABI 13's native host boundary. The kernel still passes zero service-table, record/text-arena, argument, output, and file-input fields because its current module uses none of those facilities. Moving an allocator and future exception policy into ordinary `.wv` still requires:

- `u64` values and checked address arithmetic;
- explicit unsafe or system-visible bounded memory loads and stores;
- a specified kernel ABI for scalars, state pointers, return values, nonvolatile registers, and trap dispatch; and
- imports of narrow WVA services without granting ambient privileged instructions.

These are requirements on the native target, not permission to change source semantics implicitly. Each migrated kernel policy function must retain a C# oracle or differential test until independent native evidence is sufficient.

## Safety and migration limits

WVA remains semantically checked assembly. Version 3 does not add arbitrary executable-byte directives, local branch labels, memory operands, or privileged instructions to WVA 1. The current 138-byte raw bridge exists because WVA lacks stack/register moves, comparisons, and conditional branches required by the ABI transition; it has one exact independently decoded shape and a named future replacement seam. The qualified CPU-exception object is similarly bounded and independently checked because WVA lacks the explicit privileged and descriptor-memory operations named above.

The seam does not move the UEFI loader, memory-map scanner, arena initializer, allocator machine implementation, CPU-exception machine implementation, COM1 instruction sequence, PE32+ packaging, or native compiler out of C#. The raw bridge and exception object establish verified link positions through which those pieces can be replaced without changing the loader-to-source evidence chain.

## Current evidence

The portable WVB is 929 bytes with SHA-256 `0653613d868abbba99b5e31230fb2a1f92581c4989318577cb77a6d6e60f8339`. Its 7,882-byte service-free ABI-8 WVO remains byte-identical with SHA-256 `24f5359fa5d335eb273e8680c671924dda43d4ee4c6e00c95f1bbebc742dfa99`. The 305-byte native bridge object has SHA-256 `ef8993c59eb816c7983c5b8033922231baf1d897f846a8d5e54d8232677ef75a`; its layout remains version 2. The version-3 WVA shim object remains 291 bytes with SHA-256 `332a0158c51e81d1beb5d212f508649c8efe2874af712d6d8ef15929ffd438fc`.

Firmware probe version 9 links seven WVO objects and produces a deterministic 15,360-byte EFI application with SHA-256 `ac92cd4759961c7a046ede49af8dce7626016fbcf8bb46e7d90027f5974bffa4`. Exact candidate `8d375bf` passes all 15 OS tests on Windows and Debian and a real pinned QEMU/OVMF boot; the gated transcript starts `windvale-os-boot 9`. The shared ABI-7 backend passes complete Windows/Debian qualification at the same candidate.

Firmware probe version 10 rebuilds that service-free module through ABI 8 and produces a deterministic 15,872-byte EFI application with SHA-256 `9228995f3b2522e15bd87ca63dc2637cc290f93b37f3e32b24cd8e3906671b75`. Exact candidate `d970c27` passes all 15 OS tests on Windows and Debian and the real pinned QEMU/OVMF boot; the gated transcript starts `windvale-os-boot 10`. The shared ABI-8 backend passes complete Windows/Debian qualification at the same candidate.

Exact candidate `7edc243` rebuilds the portable WVO through ABI 9 to 7,946 bytes with SHA-256 `94d10d60f5d37cdab3d0f0c3678ee1e86b312564c06d633baf4736939595fed2`. Its 315-byte bridge object has SHA-256 `949c5fdd641722c541e2ed6583a5c28bd681b77d91f28ea5cda2999026d75a23`. Firmware probe 11 remains 15,872 bytes and has SHA-256 `100070e26666bfc97d0fff8da42d996249d072b5771014838a376abfc0a13d6a`; all 15 OS tests pass on Windows and Debian, the shared ABI-9 backend passes complete cross-host qualification, and the real pinned QEMU/OVMF boot emits `windvale-os-boot 11` through `status=pass` before guest-controlled host exit code 1.

Exact commit `7979933` rebuilds the portable WVO through ABI 10 to 8,010 bytes with SHA-256 `f3d0d2aec5b7fb81d02e4188fb6ba48b6a21dc91c89bdf7f00daaf7b0a981038`. Its unchanged 315-byte bridge object retains SHA-256 `949c5fdd641722c541e2ed6583a5c28bd681b77d91f28ea5cda2999026d75a23`. Firmware probe 12 remains 15,872 bytes with SHA-256 `3010bc72b9c26386f062f78481c900cac841321b040b41447a0bbb65a9e392fe`; all 15 OS tests pass on Windows and Debian, the shared ABI-10 backend passes complete cross-host qualification, and pinned QEMU/OVMF on Windows emits `windvale-os-boot 12` through `status=pass` before guest-controlled host exit code 1. The Debian QA host does not currently provide QEMU, so no duplicate Debian emulator run is claimed.

Exact commit `8888951` keeps the portable WVB and 8,010-byte service-free WVO byte-identical while rebuilding the context constructor for ABI 11. Its 118-byte bridge code produces a 330-byte object with SHA-256 `8b28ed85af29baa65810e0ed0ce8e2893e9696cebd666ccf72a1a53f68cde2b9`. Firmware probe 13 remains 15,872 bytes with SHA-256 `ceffc3e33bf007e47b109f3b6a71db2fdceac3c0e908d1471f056909ee42532d`; both hosts pass all 15 OS tests, the shared ABI-11 backend passes complete cross-host qualification, and pinned QEMU/OVMF on Windows emits `windvale-os-boot 13` through `status=pass` before guest-controlled host exit code 1. The Debian QA host does not provide QEMU.

Exact Decision 0073 commit `328e455` keeps the portable WVB and 8,010-byte service-free WVO byte-identical while rebuilding the context constructor for ABI 12. Its 128-byte bridge code produces a 340-byte object with SHA-256 `0f6d4f00e6a66c23dedc7c6224cdae3f556c5d1c0ff927e596c927a73fd9829f`. Firmware probe 14 remains 15,872 bytes with SHA-256 `aadfbc5cb56f6afea94605ad31ee6af6a60b1e821403dfb8e1c2550631b6d548`; both hosts pass all 15 OS tests and the pinned-QEMU gate passes on Windows.

Exact Decision 0074 commit `66b273f` again keeps the portable WVB and service-free WVO byte-identical while rebuilding the complete context constructor for ABI 13. Its 133-byte bridge code produces a 345-byte object with SHA-256 `0a0393457200dbf5ecfbb667c6c283510a6eb13a3e7e77537a0b6d8e0f503d68`. Firmware probe 15 remains 15,872 bytes with SHA-256 `d716b77a91646da6b423bacb1faa6d70f5a097241c610fe49291b068f33d5f29`; both hosts pass all 15 OS tests and the pinned-QEMU gate passes on Windows.

Exact Decision 0076 commit `ef08619` again keeps the 929-byte portable WVB and 8,010-byte service-free WVO byte-identical while rebuilding the complete context constructor for ABI 14. Its 138-byte bridge code produces a 350-byte object with SHA-256 `3cbf50a4828a1a69ca7441a667cb95e569055468c345ed26b8a580fda3facfc5`. Firmware probe 16 remains 15,872 bytes with SHA-256 `206a036f8cbe3198544b6878bf52c80ef8d489c14d5437c6c7004ff1d6599504`; both hosts pass all 15 OS tests, the shared ABI-14 backend passes complete Windows/Debian qualification, and pinned QEMU/OVMF on Windows emits `windvale-os-boot 16` through `status=pass` before guest-controlled host exit code 1. The Debian QA host does not provide QEMU.

Firmware probe 17 retains those WVB, WVO, bridge, context, and WVA contracts while adding the vector-6 exception object, allocated IDT page, ordinary armed marker, and explicit invalid-opcode image. Exact commit `ba2cf69` passes all 63 Seed tests and all 17 OS tests on Windows and Debian. Pinned QEMU qualifies the exact 17,920-byte normal image with SHA-256 `d2c0a7e4e5e1605fc8639c05ab27ad07ee2b015ad2dc151d8637830b8acb3f18` and the exact 17,920-byte invalid-opcode image with SHA-256 `26ccfaf862024e022339ca9fa8114c71b4fe601fe59a806d366e1d330b6d106d`; [Decision 0081](../Documents/Decisions/0081-First-Terminal-X64-Cpu-Exception-Boundary.md) records the complete qualification.
