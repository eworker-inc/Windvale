# First protected Windvale process

## Status and purpose

Protected-process contract version 1 defines Windvale OS's first real user protection domain. Firmware probe 22 implements the contract on x86-64: one process, one CPL3 thread, one separate page-table root, one capability, one capacity-one register channel, three bounded system calls, clean process exit, and containment of one deliberate user general-protection fault. [Decision 0091](../Documents/Decisions/0091-First-Protected-Windvale-Process.md) owns the slice.

The implementation has focused Windows and pinned-QEMU evidence. Cross-host qualification is pending, so probe 21 remains the latest cross-host-qualified firmware baseline.

This contract is deliberately narrower than a general process manager or stable public user ABI. It fixes enough state and mechanism to test the architecture from Windvale policy through a real privilege transition without pretending that scheduling, service discovery, arbitrary module loading, or general IPC already exists.

## Ownership split

- [`Process-Foundation.wv`](../Operating-System/Kernel/Process-Foundation.wv) is the immutable Windvale policy oracle. It binds the admitted WVB identity, process/thread identities, budgets, capability, capacity-one channel, state transitions, and expected result. Its exact successful result is policy token `91`.
- [`Process-User-Shim.wva`](../Operating-System/Kernel/Process-User-Shim.wva) and [`Process-User-Fault-Shim.wva`](../Operating-System/Kernel/Process-User-Fault-Shim.wva) own the bounded user-entry machine sequences. The first calls the admitted Windvale AOT program, then performs send, receive, and exit. The second performs send and receive, then executes privileged `CLI` from CPL3 to obtain deterministic general protection.
- [`X64-Kernel-Shims.wva`](../Operating-System/Kernel/X64-Kernel-Shims.wva) owns the `SYSCALL` instruction encoding and normalized vector-6, vector-13, and vector-14 process exception entries.
- The Stage 0 process planner and x64 object temporarily own arbitrary page-table and descriptor writes, GDT/TSS/IDT publication, MSR setup, `SWAPGS`, `SYSRETQ`, process-record mutation, and the bounded syscall dispatcher. These are named replacement seams for system-profile Windvale state/validation plus WVA machine mechanics, not permanent C# kernel policy.
- The C# planner is also an independent exact-byte oracle for page permissions, record construction, diagnostics, and deterministic artifacts.

The admitted user computation is still ordinary Windvale source compiled through canonical WVB and the shared ABI-16 native backend. WVA supplies its process entry and syscall mechanics; it does not replace the Windvale program.

## Fixed first identity and budgets

Version 1 admits only the exact canonical WVB whose SHA-256 is:

```text
7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2
```

The fixed policy values are:

| Value | Version 1 rule |
| --- | ---: |
| Process identifier | `1` |
| Thread identifier | `1` |
| User memory-page budget | `3` |
| Native instruction budget | `4` |
| Native call-depth budget | `1` |
| Capability/handle budget | `1` |
| System-call budget | `3` |
| Channel capacity | `1` register message |
| Expected admitted-program and process result | `29` |

The policy WVB must return token `91` before any process allocation or descriptor mutation begins. A changed WVB identity, policy shape, token, budget, or capability value fails before CPL3 entry.

## Address space and page permissions

The process allocator requests exactly seven contiguous zeroed pages below 1 GiB and wholly within one 2 MiB page-table region:

| Relative page | Purpose | Process access |
| ---: | --- | --- |
| `0` | PML4 root | None |
| `1` | PDPT | None |
| `2` | Page directory | None |
| `3` | One 2 MiB-region page table | None |
| `4` | Linked user image | user read/execute; not writable |
| `5` | Initial user stack | user read/write; NX |
| `6` | ABI-16 execution context/data | user read/write; NX |

The first three hierarchy pages are copied from the active kernel root into a new root. User permission is added only to the hierarchy path needed by the process allocation. Within its selected 2 MiB region, every ordinary leaf remains supervisor read/write and NX; exactly the code, stack, and data leaves receive the user bit. Page zero remains absent when that region includes address zero. No present leaf may be both writable and executable.

The linked user image must occupy 1 through 4,096 bytes and begin at its page. The data page begins with the exact ABI-16 execution-context version and size, instruction budget `4`, and call-depth budget `1`. Version 1 has no demand paging, shared memory, mapping API, user allocator, page release, teardown, or reclamation.

## Process and thread state record

The process machine stores one 256-byte little-endian `WVPROC01` record in the kernel memory-state page at offset `0x100`:

| Offset | Bytes | Field | Version 1 rule |
| ---: | ---: | --- | --- |
| `0x00` | 8 | Magic | ASCII `WVPROC01` |
| `0x08` | 4 | Version | `1` |
| `0x0C` | 4 | Record bytes | `256` |
| `0x10` | 4 | Process state | ready/running/exited/faulted |
| `0x14` | 4 | Thread state | ready/running/exited/faulted |
| `0x18` | 4 | Process identifier | `1` |
| `0x1C` | 4 | Thread identifier | `1` |
| `0x20` | 32 | Module identity | Exact admitted-WVB SHA-256 |
| `0x40` | 8 | Process root | First process allocation page |
| `0x48` | 8 | User code page | Allocation page 4 |
| `0x50` | 8 | User stack page | Allocation page 5 |
| `0x58` | 8 | User data page | Allocation page 6 |
| `0x60` | 4 | User page budget | `3` |
| `0x64` | 4 | Instruction budget | `4` |
| `0x68` | 4 | Handle budget | `1` |
| `0x6C` | 4 | System-call budget | `3` |
| `0x70` | 4 | Capability slot | `0` |
| `0x74` | 4 | Capability generation | `1` |
| `0x78` | 4 | Capability rights | send and receive, value `3` |
| `0x7C` | 4 | Channel capacity | `1` |
| `0x80` | 8 | Saved kernel `RSP` | Written immediately before CPL3 entry |
| `0x88` | 8 | Kernel resume address | Completion continuation |
| `0x90` | 8 | Saved user `RSP` | Updated on syscall entry |
| `0x98` | 8 | Saved user `RIP` | `RCX` from syscall entry |
| `0xA0` | 8 | Saved user flags | `R11` from syscall entry |
| `0xA8` | 4 | System-call count | Initially zero |
| `0xAC` | 4 | Channel state | `0` empty, `1` full |
| `0xB0` | 4 | Channel message | One `i32` value |
| `0xB4` | 4 | Result | Process result or failure `1` |
| `0xB8` | 4 | Fault vector | CPU vector or `0xFFFFFFFF` for syscall failure |
| `0xBC` | 4 | Fault error | Normalized CPU error code |
| `0xC0..0xFF` | 64 | Reserved | Zero |

Process and thread states use the same closed values: `1` ready, `2` running, `3` exited, and `4` faulted. The record is internal evidence, not a userspace-mappable ABI or persistent object format.

## Capability and channel

The only capability is slot `0`, generation `1`, with rights bits `1` send and `2` receive. Its first experimental machine reference is:

```text
(generation << 16) | slot = 65536
```

The dispatcher requires the exact reference and the needed right. The generation prevents an all-zero or stale slot value from being accepted by this contract; version 1 performs no capability allocation, reduction, transfer, revocation, destruction, or generation rollover.

The channel holds one `i32` register message. Send succeeds only when empty and makes it full. Receive succeeds only when full, returns the message, and makes it empty. There is no queue, waiting, blocking, peer process, copy buffer, shared memory, or backpressure policy beyond immediate contract failure.

## First experimental syscall mechanics

The semantic operations are versioned by this process contract. Their x86-64 number/register assignment is deliberately experimental and is not a stable public Windvale ABI:

| `EBX` | Operation | Inputs | Successful result |
| ---: | --- | --- | --- |
| `1` | send | `ESI = 65536`, `EAX = i32 message` | `EAX = 0` |
| `2` | receive | `ESI = 65536` | `EAX = queued i32 message` |
| `3` | exit | `EAX = process result` | Does not return to user mode |

`SYSCALL` saves the next user instruction pointer in `RCX` and flags in `R11`. The machine entry uses `SWAPGS`, saves user state through the process record, switches to the recorded kernel stack, checks number, capability, channel state, and budget, and either resumes with `SWAPGS; SYSRETQ` or returns directly to the kernel continuation. Exit is accepted only with result `29` after exactly three calls and an empty channel whose retained message is `29`.

Any invalid number, reference, right, channel transition, result, or budget marks both states faulted, records vector `0xFFFFFFFF`, result `1`, and resumes the kernel. The current dispatcher does not copy user memory and therefore has no unchecked user pointer input.

## Privilege transition and fault containment

The Stage 0 machine seam constructs a private GDT containing null, kernel code/data, user data/code, and one 64-bit TSS descriptor. The TSS supplies the ring-0 stack for privilege-changing exceptions. It extends the existing kernel-owned IDT page through vector 14 and routes vectors 6, 13, and 14 through WVA-normalized process stubs. Kernel-originated faults still tail-transfer to the qualified terminal handler.

The syscall path checks CPU `SYSCALL/SYSRET` support, enables `EFER.SCE`, programs `STAR`, `LSTAR`, and `FMASK`, and stores the process-record base in `KERNEL_GS_BASE`. CPL3 entry uses `SYSRETQ` only after the new process root, descriptors, TSS, IDT, MSRs, record, code, stack, and context are complete.

The deliberate user-fault image sends and receives `29`, then executes WVA `disable_interrupts`. `CLI` is privileged at CPL3 and deterministically raises general protection vector 13 with error code 0. The WVA stub normalizes the privilege-transition frame, the kernel records faulted process/thread state, and control resumes on the saved kernel stack. The scenario is accepted only after two successful system calls, an empty channel retaining message `29`, and exact `(13, 0)` fault evidence. Kernel faults remain terminal; user containment does not weaken the existing panic contract.

## Planner diagnostics

| Code | Meaning |
| --- | --- |
| `WVOS6001` | The seven-page process allocation is null, unaligned, incomplete, or outside the low 1 GiB. |
| `WVOS6002` | The process allocation crosses a 2 MiB page-table region. |
| `WVOS6003` | The linked user image is empty or exceeds one page. |
| `WVOS6004` | The module identity is not one SHA-256 digest. |
| `WVOS6005` | The process allocation overlaps the retained kernel executable window. |

## Deterministic evidence

Focused tests independently rebuild and compare every artifact. Current candidate identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windvale process-policy WVB | 2,780 | `fc47ce2d256bea69edbc086bf288136dd7f557d8250e397a2a9e82a66c23078d` |
| Process-policy WVO | 25,062 | `7b3096698ec6730ffa8c17488e00155a1700ecd879ae5fe8130a096b40311aff` |
| Normal user WVA WVO | 202 | `1a3065bcfa9ddcd973ede2b36ac918544a1c4c63aa44729ce1f1d970413fba76` |
| User-fault WVA WVO | 195 | `b67bbdaa78f492564d21d18bd5fc2abd75978c89bafe882418237e53503bc14f` |
| Linked normal user image | 454 | `6558145ea3bfecc4f9f312ba886ffbdc7a902ed14e66684c5e992d4bc5653947` |
| Linked user-fault image | 438 | `973ead836f588bc6ef2b0fa31754f75e12a88ab229048d85075884f654ed5356` |
| Normal process-machine WVO | 3,846 | `641500ad40d2ab7cf36d12ac9cc51163c690341ec031c847497874cf9f0c3576` |
| Normal process-machine code | 2,514 | `ef7af1c127dd0973e78f4f57ad7f77c6e16f4fe8f2427a359f177311b028bbb6` |
| User-fault process-machine WVO | 3,862 | `2b74ecabe417e4a3917f90e0b49a472faab2fb36d7fc9813808cb260acc0a327` |
| User-fault process-machine code | 2,546 | `7381d1f14bcbab99e8194fcd1248b43494543a16c53d06f2ea59ee9d4cc5f0e8` |

Pinned QEMU must additionally prove the normal CPL3 send/receive/exit path, kernel continuation, clean poweroff, deliberate CPL3 `CLI`, contained vector-13 return, and unchanged terminal kernel-fault scenarios. [Windvale-Os-Boot-Probe.md](Windvale-Os-Boot-Probe.md) records the complete probe-22 images and transcripts.

## Deliberate limits

Version 1 has one statically known process and one thread. It has no scheduler, preemption, wait, second process, capability transfer, revocation, general loader, arbitrary user program, process creation API, teardown, page reclamation, demand paging, shared memory, user allocator, signal model, complete user-pointer validation, general trap dispatcher, interrupts, timer, SMP, service discovery, init process, filesystem, or package service. Those are later contracts; they must not be inferred from the fixed experimental syscall numbers or record layout.
