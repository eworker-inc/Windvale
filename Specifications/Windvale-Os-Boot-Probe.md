# Windvale OS firmware boot probe

## Status and purpose

Firmware probe version 27 is the current qualified init-owned-resource proof. It composes ABI 16/context 7, separate Windvale identities for init, the interpreter, and admitted WVB input, kernel memory version 5, paging version 3, protected-process version 6, a WVA-owned resource leaf, an init-owned RO/NX page, a one-shot immutable grant, two CPL3 roots, reduced authority, deterministic block/wake coordination, interpreted result IPC, and contained interpreter-process general protection. [Decision 0096](../Documents/Decisions/0096-First-Windvale-Init-Owned-Boot-Resource-Grant.md) owns the slice.

Exact commit `4701200e707786f04d4462bb75a1664cd5ed13cc` and probe 27 are the latest fully cross-host-qualified OS baseline. GitHub Verify run `30739172682` passes all 67 Seed tests and all 25 OS tests on Windows and digest-pinned Debian 12; all four exact pinned-QEMU scenarios pass on Windows.

Decisions 0045 through 0095 retain the historical progression through qualified probe 26. Current component contracts are [protected process](Windvale-Protected-Process.md), [bytecode interpreter](Windvale-Os-Bytecode-Interpreter.md), [WVB admission](Windvale-Os-Wvb-Admission.md), [kernel memory](Windvale-Kernel-Memory.md), [kernel paging](Windvale-Kernel-Paging.md), [CPU exceptions](Windvale-Kernel-Exceptions.md), [trap frame](Windvale-Kernel-Trap-Frame.md), [kernel handoff](Windvale-Kernel-Handoff.md), [shutdown](Windvale-Kernel-Shutdown.md), [native seam](Windvale-Kernel-Native-Seam.md), [execution context](Windvale-Native-Execution-Context.md), [UEFI application](Windvale-Uefi-Application.md), and [boot environment](Windvale-Os-Boot-Environment.md).

The firmware ABI and table rules follow UEFI 2.11 x64 calling conventions, system/boot-service table layouts, `GetMemoryMap`, and `ExitBootServices`. These platform mechanics do not define portable Windvale semantics.

## Artifact construction

The builder embeds these deterministic source inputs:

- system-profile `Hello-World.wv`;
- portable `Embedded-Wvb-Program.wv`, `Wvb-Admission.wv`, `Process-Foundation.wv`, `Init-Resource-Service.wv`, and `Native-Wvb-Probe.wv`, plus hosted `Bytecode-Interpreter.wv`; and
- `X64-Kernel-Shims.wva`, the init-service WVA shim, the boot-resource WVA stencil, and the normal or deliberate-fault client WVA shim.

The special kernel source passes through the ordinary frontend/typed WIR and versioned system target. Every portable module passes through canonical WVB production, mandatory verification, the shared ABI-16 selector/fragment verifier, and the ordinary WVO sink. Stage 0 requires `Wvb-Admission.wv` and `Process-Foundation.wv` to bind the exact interpreter, admitted-program, and service WVB identities. It rewrites only verified external `Main` symbols needed for deterministic linking.

The Stage 0 builder also creates loader, kernel-memory, CPU-exception, kernel-paging, admission-bridge, process-machine, retained native-bridge, and x64 byte-adapter WVO objects. The WVA sources assemble through the reference/recovery assembler and are compared with the Windvale-written assembler where their grammar overlaps. Stage 0 accepts the boot-resource stencil only as one exact 199-byte relocation-free read-only symbol, then publishes those already verified bytes as one function object. The linker resolves every call, tail jump, and RIP-relative data relocation, reconstructs the base-zero image, and passes verified code/read-only data to UEFI application writer version 3.

The linked payload must begin at entry offset zero and fit the fixed 256 KiB kernel executable window. Both role-specific user images must begin at entry offset zero. The init image fits one 4 KiB RX page; the interpreter image fits 32 4 KiB RX pages. No generated WVB, WVO, EFI application, FAT view, variable store, firmware image, or captured memory map is committed.

Important current exact artifacts include:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Special kernel WVO | 6,494 | `77bfe58344ad82c3ef9255ee31e860c8df132fe8e71880d7683feba81f71e43f` |
| WVA kernel seam WVO | 1,123 | `8a6f54950f15c7331107a5bfa7bd2d863f64b25d395b7cfd9983c31130599363` |
| Paging v3 WVO | 1,244 | `63e3cbd8cfb0f5a6260b660d4f2253c3f14b3a5f71271fe99ecf04644c4b6c2d` |
| Terminal exception WVO | 4,667 | `49f15606d2cd41236f87e8a7a7e24a9532683ffe9d5a59795dc8084288b2f84a` |
| WVB admission v2 bridge | 484 | `7b53fc11e4e99966386994c247c3a2a19f99ef8da751dbd9dc53f5575871a00d` |

The admitted 174-byte WVB remains `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2`. Its 2,786-byte admission WVB, 504-byte embedded-program reference WVO, and 24,445-byte admission WVO remain byte-identical to Decision 0090. Admission bridge version 2 calls the protected-process entry after token 73. Probe 27's 1,385-byte init and 128,157-byte client images contain neither the embedded-program WVO nor the complete admitted WVB. The process object carries the WVB as separate read-only data, places it in init's owner page, and maps an alias into the client only after the grant. [Windvale-Protected-Process.md](Windvale-Protected-Process.md) records the policy, user-image, resource, and process-machine identities.

Current complete images are:

| Scenario | EFI bytes | SHA-256 | Expected host code |
| --- | ---: | --- | ---: |
| `normal` | 224,768 | `709ebd7f643f2f9d9c7cf4eb4042977c675a3ff19d7a34da4d7e26e0526a29b7` | 0 |
| `invalid-opcode` | 224,768 | `a89e66da871fcf46637ce4d91463268b2c8cce4309d12953eb7c7b464f57178f` | 3 |
| `general-protection` | 224,768 | `ab10c10cc0af01ebe5603033d2f35bce86660b23953c33ff6012ad7cee83a1c5` | 3 |
| `user-fault` | 225,280 | `b5e726b51f26f48cc9948095bfce4eabaf0b3bc90b89a6c3b3650325adfb05bb` | 0 |

## Firmware entry and bounded exit

On x64 UEFI entry, `RCX` carries the image handle and `RDX` the `EFI_SYSTEM_TABLE` pointer. The probe constructs a 136-byte aligned call frame with 32-byte shadow space, the fifth `GetMemoryMap` argument, map outputs/capacity, retained table pointers, image handle, a three-attempt exit counter, and an eventual 48-byte handoff overlay.

Before any firmware call it structurally validates non-null system and boot-service tables, their signatures, minimum EFI 1.02 revisions and header sizes, zero reserved fields, and non-null `GetMemoryMap`, `AllocatePool`, `FreePool`, and `ExitBootServices` pointers. It does not recompute table CRCs.

The memory-map sequence is bounded to 1 MiB, descriptor strides 40 through 256 bytes, two descriptors of allocation slack, and exact validation of returned size, version, alignment, nonzero page counts, and last-page arithmetic. Pre-exit failure frees an allocated map buffer once.

The probe makes at most three `ExitBootServices` attempts. A stale map key permits only a bounded `GetMemoryMap` refresh into the existing buffer before retry. Success is irreversible: no firmware service or invalidated table field is used afterward. Terminal post-attempt failure reports through debug exit and halts rather than returning to potentially partially shut-down firmware.

## Kernel entry and owned memory

After successful exit, the loader overlays exact `WVKHAND1`, calls `Windvale_kernel_entry`, and retains a 16-byte-aligned loader frame. The compiler wrapper validates the handoff before entering the memory object.

Kernel memory version 5 selects the lowest eligible 2 MiB-aligned 60-page/240 KiB conventional-memory arena below 4 GiB, checks the complete aligned range, rejects descriptor overlap, clears it, publishes `WVKMEM05`, copies the handoff, and switches to the two-page kernel stack. Allocation page 3 becomes the zeroed IDT page.

Kernel paging version 3 allocates pages 4 through 9, builds the low-1-GiB identity hierarchy, enables NX/WP, activates/read-backs CR3, and publishes `WVKPAG03`. Page zero is absent, ordinary leaves are supervisor RW/NX, and the 256 KiB linked-payload window is supervisor RX.

The WVA tail reaches admission bridge version 2. It constructs an ABI-16/context-7 service-free context with exact budgets 8,944/2 and calls AOT Windvale admission export `Windvale_kernel_wvb_admit`. Only token 73 permits the protected-process call.

The protected-process path then:

1. runs `Process-Foundation.wv` through the shared native backend and requires policy token 95;
2. allocates pages 10 through 17 for init and 18 through 59 for the interpreter process;
3. creates two separate roots, places the admitted WVB in init's RO/NX page, leaves the client's target PTE and resource pointers zero, and publishes `WVPROC06`, `WVCHAN01`, and owned `WVRES001` records;
4. installs the private GDT/TSS, extended IDT, and syscall MSRs, then enters init;
5. requires Windvale init to select resource `1`, invoke one fixed grant through its reduced right, and then wait on the empty result channel after exactly two calls;
6. validates the borrowed resource record, client RO/NX alias, service table, and `WVBR` publication before entering the client;
7. runs the send-only Windvale interpreter, which fetches `boot:main.wvb` through the exact service leaf, decodes the WVB to result 29, and reaches exact exit or the explicitly selected contained client fault;
8. reactivates init, consumes message 29, restores its saved native context, requires the Windvale service to exit 29, and returns result 29 to the admission bridge.

Only then does the admission bridge tail-transfer to retained native bridge 10. That bridge executes the retained portable probe under its separate exact context and accepts only result 29 before reaching compiler-generated `Windvale_kernel_main`. Main emits the memory, paging, admission, process-isolation, resource-grant, interpreted-runtime, init-service, cross-process IPC, and Hello markers. The loader adds exception/native/source evidence, then either cleanly powers off or selects one explicit fault scenario.

The invalid-opcode and general-protection scenarios fault at CPL0 after both protected processes and Main have completed, so they retain the terminal panic contract. The user-fault scenario instead executes `CLI` in the client after send; the process path contains vector 13/error 0, wakes the independent init service, and continues to clean shutdown.

## Exact serial evidence

Normal success requires:

```text
windvale-os-boot 27
entry=pass
system-table=pass
memory-map=pass
boot-services=exited
memory-owned=pass
allocator=pass
kernel-stack=pass
paging=owned
wvb-admission=pass
processes=isolated
resource-grant=pass
wvb-runtime=interpreted
init-service=pass
ipc=cross-process
Hello from Windvale
cpu-exceptions=armed
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
shutdown=poweroff
```

The user-fault scenario requires the same success path plus this line immediately before final success:

```text
user-fault=contained
```

The invalid-opcode scenario shares the prefix through Hello World and terminates with:

```text
panic=invalid-opcode
vector=6
error-code=0
status=panic
```

The general-protection scenario shares that prefix and terminates with:

```text
panic=general-protection
vector=13
error-code=0
status=panic
```

Kernel fault handlers write value 1 to QEMU test port `0xF4`, producing host code 3, then enter a CLI/HLT loop if needed. Normal and contained-user-fault success call the WVA Q35 adapter, which writes `0x2000` to port `0x0604` and produces host code 0. The complete scenario-specific serial marker is mandatory because host code alone is ambiguous.

## Boot harness

From the repository root:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario normal
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario invalid-opcode
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario general-protection
pwsh -NoProfile -File Tools/Verify/Verify-Os-Boot.ps1 -Scenario user-fault
```

The harness preflights the pinned environment, builds a run-private removable-media tree and variable-store copy, and launches QEMU `pc-q35-11.0,accel=tcg` with `qemu64`, one CPU, 128 MiB, no network/display/monitor, captured serial, and `isa-debug-exit`. It verifies EFI and firmware identities before and after launch. The default timeout is 60 seconds. Run artifacts are deleted unless `-KeepRunDirectory` is supplied, after validating the exact temporary path.

Normal report evidence has this path-free shape:

```text
windvale-os-boot-report 27
status=pass
scenario=normal
architecture=x86-64
application-format=pe32-plus-uefi-application-v3
probe-version=27
efi-bytes=224768
efi-sha256=709ebd7f643f2f9d9c7cf4eb4042977c675a3ff19d7a34da4d7e26e0526a29b7
serial-marker=windvale-os-boot-27-entry-system-table-memory-map-boot-services-exited-memory-owned-allocator-kernel-stack-paging-owned-wvb-admission-processes-isolated-resource-grant-pass-wvb-runtime-interpreted-init-service-pass-ipc-cross-process-hello-cpu-exceptions-armed-native-context-native-wvb-windvale-source-status-pass-shutdown-poweroff
qemu-exit-code=0
```

Other reports select their scenario, exact digest/size, marker suffix, and expected host code from the table above. `-KeepRunDirectory` adds a native diagnostic path and is therefore not portable report evidence.

## Failures

| Code | Meaning |
| --- | --- |
| `WVOS3001` | The host or probe build failed. |
| `WVOS3002` | QEMU could not start. |
| `WVOS3003` | The bounded boot timeout expired. |
| `WVOS3004` | QEMU returned an unexpected exit code. |
| `WVOS3005` | Serial output lacks the unique complete scenario marker or contains conflicting evidence. |
| `WVOS3006` | The EFI application or installed firmware input changed during the run. |
| `WVOS3007` | Temporary-directory cleanup failed its absolute-path boundary check. |

## What this does not prove

Probe 27 does not authenticate firmware CRCs, own general physical memory, reclaim loader ranges, implement a general VM manager, load arbitrary WVB, provide complete semantic verification, JIT bytecode, or select a native cache dynamically. It grants one fixed admitted module from init to one fixed borrower and interprets only one bounded semantic subset. The resource-record/table/PTE writer, verified-stencil publisher, system target, page/descriptor constructors, symbol adaptation, admission/process bridges, syscall dispatcher, and COM1 adapter retain named Stage 0 seams.

The fixed two-process proof has no general scheduler, preemption, timer, process creation API, general capability transfer/revocation, IPC queue, larger messages, user allocator, teardown, page reclamation, shared memory, demand paging, signal ABI, general service discovery, resource namespace, filesystem, package service, network, or device service. Resource identifier `1` and its single immutable borrow are not a namespace or ownership-transfer facility. The arena is fully consumed and the syscall/register assignment is experimental, not a stable public ABI.

The exception boundary has no `CR2` page-fault evidence, double-fault containment, IST, NMI, IRQ, PIC/APIC, interrupt enablement, nested-fault policy, general resumption, or WVR-to-CPU mapping. Shutdown remains pinned-Q35-specific. Hyper-V and physical-hardware qualification remain later gates.
