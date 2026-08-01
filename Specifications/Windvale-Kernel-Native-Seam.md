# Windvale kernel native implementation seam

## Status and purpose

Kernel native seam version 14 and bridge 10 are cross-host qualified through the pre-paging firmware probe-20 baseline at exact commit `12e9e2e`. Versions 13 and 14 add WVA-owned Q35 poweroff and normalized vector-6/vector-13 entries. Candidate version 15 composes that baseline with Decision 0088's kernel-owned paging. Current candidate version 16 retains both paths and adds Decision 0090's Windvale-owned WVB admission profile, WVA seam version 7, and admission bridge version 1. [Decision 0081](../Documents/Decisions/0081-First-Terminal-X64-Cpu-Exception-Boundary.md) owns historical version 12; [Decisions 0085](../Documents/Decisions/0085-First-Wva-Owned-Q35-Clean-Shutdown.md), [0086](../Documents/Decisions/0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md), and [0087](../Documents/Decisions/0087-Native-Windows-And-Linux-File-Output.md) own the qualified versions 13 and 14 plus bridge 10; candidate [Decision 0088](../Documents/Decisions/0088-First-Kernel-Owned-X64-Page-Tables.md) owns version 15; and candidate [Decision 0090](../Documents/Decisions/0090-First-In-Guest-Wvb-Admission.md) owns version 16. [Decision 0056](../Documents/Decisions/0056-Windvale-Owned-Post-Memory-Evidence.md) records the qualified version-2 bidirectional WVA/WV boundary.

This contract prevents temporary C# machine-code generation from silently becoming the kernel architecture. It also avoids pretending that privileged x86-64 entry mechanics belong in ordinary source code.

## Implementation roles

| Layer | Current responsibility | Direction |
| --- | --- | --- |
| C# reference/recovery host | Compile WVB, select and verify ABI-15 native code, adapt external object symbols, assemble, link, package PE32+, provide host oracles, and retain bounded loader, memory, exception, paging-construction, and bridge emitters while replacements are unavailable. | Remains an independent recovery and comparison path; does not define kernel or WVB-admission policy. |
| Shared portable-WVB backend | Lower verified portable semantics into the same versioned native ABI and WVO object used by host JIT/AOT evidence. | Replaces special OS instruction selection incrementally and becomes Windvale-written. |
| WVA machine layer | Own explicit entry/exit and capability-adapter shims, register-frame mechanics, and later named privileged instructions that ordinary Windvale code must not execute ambiently. | Grows only from concrete kernel requirements with exact encodings and verification rules. |
| `.wv` system layer | Own kernel policy, state transitions, diagnostics, allocation decisions, exception dispatch, and later runtime services once the shared native target can lower them. | Becomes the primary kernel implementation. |

New kernel mechanisms should default to WVA for irreducible machine mechanics and `.wv` for policy. Additional raw C# instruction emission requires a documented compiler or assembler blocker and a named replacement seam.

## Executable WVA version 7, admission bridge version 1, bridge version 10, and portable probe version 5 seam

`Operating-System/Kernel/X64-Kernel-Shims.wva` assembles through the qualified WVA 1 reference/recovery assembler into a canonical WVO object:

- exported compiler-facing function `Windvale_kernel_write_byte`;
- exported machine-entry function `Windvale_kernel_wva_main`;
- exported normalized exception entries `Windvale_kernel_x64_exception_6_entry` and `Windvale_kernel_x64_exception_13_entry`;
- exported paging functions `Windvale_kernel_x64_page_protection_enable` and `Windvale_kernel_x64_page_table_activate`;
- exported target-adapter function `Windvale_kernel_x64_q35_shutdown`;
- imported common terminal function `Windvale_kernel_x64_exception_terminal`;
- imported admission bridge `Windvale_kernel_x64_wvb_admission`;
- imported internal machine function `Windvale_kernel_x64_write_byte`; and
- five tail jumps represented by `relative-i32` relocations with addend `-4`: two ordinary imported-function shims, two normalized entries targeting the common terminal handler, and one self-relative shutdown retry. The paging wrappers contain no relocations.

The kernel memory object calls the exception installer and paging installer after switching to the kernel-owned stack, then calls `Windvale_kernel_wva_main`. The paging installer constructs a checked six-page hierarchy and calls the two WVA paging functions before Windvale Main can emit `paging=owned`. The inbound WVA Main tail transfer reaches admission bridge version 1. That bridge constructs a service-free context with exact budgets 8,948/2, calls AOT Windvale export `Windvale_kernel_wvb_admit`, accepts only token 73, reloads the context pointer, calls the separately AOT-compiled admitted export `Windvale_kernel_embedded_main`, and accepts only result 29. It then tail-transfers to retained native bridge 10. [Windvale-Os-Wvb-Admission.md](Windvale-Os-Wvb-Admission.md) owns its exact byte identity, symbols, ordering, and failure rules.

The retained native probe bridge then:

1. reserves 120 aligned stack bytes and preserves the copied-handoff pointer from `RCX`;
2. constructs the exact 112-byte execution context with version/size, WVB instruction budget 271, call-depth budget 2, zero service-table, record-arena, text-arena, service-failure-detail, argument-table/count, output-table, file-input-table, file-output-table, and reserved fields;
3. passes the context pointer in `RDX` and calls the ABI-15 portable export `Main`;
4. accepts only the complete packed result `RAX == 29`;
5. returns failure 1 on a trap, exhausted budget, or wrong result; and
6. restores `RCX` and tail-transfers to special compiler export `Windvale_kernel_main` on success.

The compiler object imports `Windvale_kernel_write_byte`, which resolves to the WVA export. WVA tail-transfers each call to explicitly internal symbol `Windvale_kernel_x64_write_byte`. The public kernel capability boundary is therefore WVA-owned even though its current COM1 instruction sequence remains bootstrap code.

The builder independently decodes the assembled WVA object, paging object, both bridge objects, and three portable native objects and requires their exact architecture, section, symbol, code, and relocation shapes before linking. On the normal probe-21 path, the loader emits the final success and shutdown markers after the admission and existing Main chains return under the new CR3, then calls the WVA Q35 adapter. That adapter requests poweroff through the fixed target port and enters a disabled-interrupt halt/retry fallback if the machine does not terminate. The two fault paths instead enter WVA stubs under that same root and normalize a CPU-no-error-code frame and a CPU-error-code frame before reaching one terminal handler. The ABI-15 selector and fragment verifier independently validate every portable module, empty service list, and unused zero-length runtime resources before each WVO is admitted.

## Portable native probe

`Operating-System/Kernel/Native-Wvb-Probe.wv` is an ordinary portable module, not system-profile source. It owns immutable i32 array `[3, 5, 8, 13]`, immutable byte header `[0, 5, 0, 0, 0, 255]`, a two-parameter `Add`, borrowed-byte `Readˉheader`, one bounded loop, and exported `Main() -> i32`. It passes bytes through an internal function, slices a four-byte payload, performs checked `u8` and little-endian `u32` reads, and validates the values before accepting the sum. The reference interpreter requires exactly 271 WVB instructions and active call depth 2 to return 29. The shared native object has separate code and read-only data plus four verified RIP-relative relocations.

The retained probe is compiled and AOT-linked on the host. The guest does not retain, decode, verify, or JIT that probe's WVB bytes. Probe 21 separately retains and validates the exact 174-byte `Embeddedˉwvbˉprogram` identity before its derived AOT object executes. Reaching `native-wvb=pass` still proves the retained preverified computation returned the expected packed result; `wvb-admission=pass` owns the new fixed-profile evidence. Neither marker claims a general loader or JIT.

## Normalized terminal CPU exception seam

Probe 17 qualified one exact vector-6 Stage 0 object between kernel-memory initialization and the existing WVA/native Main path. Qualified probe 20 extends the table to vectors 6 and 13 through probe 19's retained mechanics. Its Stage 0 object still clears the owned page, reads live `CS`, publishes both descriptors, executes `LIDT`, and owns the fixed terminal writer. The descriptors now target WVA functions: vector 6 pushes synthetic error 0 plus vector 6, while vector 13 preserves the CPU error cell and pushes vector 13. Both jump to one common handler over [trap frame version 1](Windvale-Kernel-Trap-Frame.md).

This raw object is an explicit replacement seam, not new portable semantics or a general kernel ABI. WVA now expresses the exact `CLI`, `HLT`, 16-bit port output, and immediate stack-cell mechanics required by current shutdown and trap entry. It still cannot express live segment-register capture, checked descriptor memory writes, `LIDT`, frame comparisons, conditional serial loops, or terminal policy. Further WVA extension requires named instructions and independent encoding rules; exception dispatch policy moves to system-profile `.wv` only after bounded unsafe memory and a kernel call convention are specified. [Windvale-Kernel-Exceptions.md](Windvale-Kernel-Exceptions.md) owns the exact version-2 table and scenarios.

## Native compiler requirements for policy migration

ABI 15 retains those portable semantics and adds exact Windows/Linux file-output leaves to ABI 14's native host boundary. The kernel still passes zero service-table, record/text-arena, argument, output, file-input, and file-output fields because its current module uses none of those facilities. Moving an allocator and future exception policy into ordinary `.wv` still requires:

- `u64` values and checked address arithmetic;
- explicit unsafe or system-visible bounded memory loads and stores;
- a specified kernel ABI for scalars, state pointers, return values, nonvolatile registers, and trap dispatch; and
- imports of narrow WVA services without granting ambient privileged instructions.

These are requirements on the native target, not permission to change source semantics implicitly. Each migrated kernel policy function must retain a C# oracle or differential test until independent native evidence is sufficient.

## Safety and migration limits

WVA remains semantically checked assembly. Executable version 7 changes only the imported Main destination; it retains version 6's paging operations, version 5's `push_i32`, and version 4's shutdown operations. It does not add arbitrary executable-byte directives, generic MSR/control-register access, local branch labels, or memory operands. The current 143-byte native-probe bridge and 163-byte admission bridge exist because WVA lacks stack/register moves, comparisons, and conditional branches required by these ABI transitions. Both have exact independently decoded shapes and named future replacement seams. The CPU-exception publication/terminal-policy object and paging constructor are similarly bounded and independently checked because WVA and system-profile `.wv` lack the remaining explicit operations.

The seam does not move the UEFI loader, memory-map scanner, arena initializer, allocator machine implementation, page-table constructor, IDT publication, terminal COM1 policy, external-symbol adaptation, PE32+ packaging, or native compiler out of C#. The raw bridges, exception object, and paging object establish verified link positions through which those pieces can be replaced without changing the loader-to-source evidence chain. WVB admission policy itself is now Windvale-owned. Q35 poweroff, the two normalized trap-entry sequences, NX/WP activation, and CR3 activation are no longer emitted by C#; Stage 0 assembles, validates, links, and packages those WVA-authored bytes.

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

Firmware probe 18 keeps those qualified WVB, WVO, bridge, context, memory, and exception contracts unchanged. Its 382-byte executable-WVA version-4 shim object has SHA-256 `4cbc235de885ab9307974128e55d8c7472cc889349f94ca4b87587ee9399a08c` and adds the exact 19-byte Q35 shutdown function. Both construction-candidate EFI images are 18,432 bytes: normal SHA-256 `035f7a25c263efdd0cec30c081ee36799b04ca85eba57d9f54a98e1ce06a6de5` and invalid-opcode SHA-256 `3d0cd8f66a7cd50826f2b66b3961cb06888956c72e3925d5f2837405f0c9dacf`. The unchanged shutdown contract is cross-host qualified through composed probe 20.

Firmware probe 19 supersedes that local construction candidate. Its 620-byte executable-WVA version-5 shim object has SHA-256 `4bb07c28877905de6e57d79454e33402de4ac54048d5ce09a26b49ad0d8347a5` and owns both normalized entry stubs plus the unchanged Q35 adapter. Its three 20,992-byte image identities remain useful construction history; the unchanged seam, shutdown, and normalized-entry contracts are cross-host qualified through composed probe 20.

The pre-paging probe-20 baseline at exact commit `12e9e2e` retains the 620-byte WVA object, the 929-byte WVB, and the 8,010-byte service-free WVO while advancing to bridge 10 and ABI 15/context 7. Its three 20,992-byte images pass all 66 Seed tests and all 18 OS tests on Windows and Debian, independent GitHub verification, and all three pinned-QEMU scenarios; Decision 0087 records their exact identities.

Candidate firmware probe 20 supersedes the local probe-19 candidate while composing ABI 15/context 7 and bridge 10 with paging. It retains the 929-byte portable WVB and 8,010-byte service-free WVO; its 143-byte bridge code produces a 355-byte object with SHA-256 `bfa2b522b2bf22b3681b523c66c2986b1af366ba042adeddd8a106b5a96a5225`. Its 773-byte executable-WVA version-6 shim object has SHA-256 `5c4b0bcfa1c6463ebbe631562deb7714aa510dfbc2418b1544b0df6c8df6bedb`; its separate 1,244-byte paging object has SHA-256 `deeebe592b38890c9964cc4d9736b1d617c0d6b20bed494ba533dcb9b1d4f318`. All three EFI images are 22,016 bytes: normal SHA-256 `392a2801bd8d8895bd9c34213336a69057c1ae81675269056c60b8c3e974ab01`, invalid-opcode SHA-256 `aa610e6ac00ed43466a87521bb4cebb2934d0885acb960db8913f025ced9cce9`, and general-protection SHA-256 `74632fcde4873f2d46e18b1b77c5cc8b495e83f0f750930e039da27dd67cd0ee`. All 20 OS tests, all 6 focused assembler tests, and all three pinned-QEMU scenarios pass locally. Exact cross-host qualification remains pending.

Candidate firmware probe 21 retains every probe-20 paging and native-probe object and adds WVB admission version 1. The 174-byte embedded WVB, 2,786-byte admission WVB, 504-byte embedded WVO, 24,445-byte admission WVO, and 481-byte admission bridge have the exact identities recorded by [Decision 0090](../Documents/Decisions/0090-First-In-Guest-Wvb-Admission.md). The executable-WVA version-7 shim object is 774 bytes with SHA-256 `2ef94f867226059e858e874d1260743e411bd1fd22887a84d35c2e508d410393`. All three EFI images are 47,104 bytes: normal SHA-256 `c3a07e1a6c8f162720a3dcd690fdb945bd862b360b26665c53e5be0642a87c38`, invalid opcode SHA-256 `0bbc0b6eedbd21aef853a2233d3fa3dbaa9564eca36f3344067b1c9b240237fc`, and general protection SHA-256 `724907ffd0963f015003c91431b79b728109ed861de73f3c1b0bf5e7b58568b6`. All 21 OS tests and all three pinned-QEMU scenarios pass locally. Exact cross-host qualification remains pending.
