# Protected Windvale processes and first init service

## Status and purpose

Protected-process contract version 2 defines Windvale OS's first two protection domains and first user-space init/resource service. Firmware probe 23 runs a receive-only Windvale service and a send-only admitted client under separate CPL3 page-table roots, transfers one kernel-owned register message, wakes the blocked service, and completes after both domains become terminal. [Decision 0092](../Documents/Decisions/0092-First-Windvale-Init-Resource-Service.md) owns version 2; [Decision 0091](../Documents/Decisions/0091-First-Protected-Windvale-Process.md) remains the historical version-1 proof.

Focused Windows and pinned-QEMU evidence pass. Cross-host qualification is pending, so probe 21 remains the latest cross-host-qualified firmware baseline.

This contract is an internal experiment, not a stable public syscall ABI or a general process manager. It proves the smallest coherent service, blocking, wake-up, rights-reduction, and inter-process IPC path while deliberately deferring a general scheduler, loader, resource namespace, and capability lifecycle.

## Ownership split

- [`Process-Foundation.wv`](../Operating-System/Kernel/Process-Foundation.wv) is the immutable Windvale policy oracle. It binds both WVB identities, the two roles and identities, fixed budgets, reduced endpoint rights, capacity-one channel, wait/wake sequence, and policy token `92`.
- [`Init-Resource-Service.wv`](../Operating-System/Kernel/Init-Resource-Service.wv) is the first user-space Windvale service. It owns one fixed immutable resource and returns exact value `29` after its WVA entry receives the client's request.
- [`Init-Resource-Service-Shim.wva`](../Operating-System/Kernel/Init-Resource-Service-Shim.wva) owns receive, Windvale-service invocation, and exit. The existing client shims own admitted-program invocation, send, and either exit or deliberate CPL3 `CLI`.
- [`X64-Kernel-Shims.wva`](../Operating-System/Kernel/X64-Kernel-Shims.wva) owns the `SYSCALL` instruction and normalized vector-6, vector-13, and vector-14 entry bytes.
- The Stage 0 planner and x64 process object temporarily own page-table and descriptor writes, GDT/TSS/IDT publication, MSR setup, process-record mutation, the syscall dispatcher, and the fixed coordinator. These are named replacement seams for system-profile Windvale state/validation and WVA machine mechanics.

The service and client computations are ordinary Windvale source compiled through canonical WVB and the shared ABI-16 native backend. C# does not define their source semantics.

## Fixed identities, roles, and budgets

Version 2 admits exactly these canonical WVB identities:

| Role | Process/thread | WVB SHA-256 | Endpoint right |
| --- | --- | --- | ---: |
| Init/resource service | `1` / `1` | `478dfcd36fed7c8063cfb3f53a6a1362bda5353656339b730be573a1be8f95b0` | receive, value `2` |
| Admitted client | `2` / `2` | `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2` | send, value `1` |

Each process has three user pages, native instruction budget `64`, native call-depth budget `1`, one capability handle, two system calls, and expected terminal result `29`. Both endpoint records use slot `0`, generation `1`, experimental reference `65536`, and channel capacity `1`. Neither process receives the combined rights value `3`.

Policy WVB must return token `92` before the channel, process records, page tables, descriptors, or MSRs are published. Any changed identity, token, role, budget, or endpoint right fails before CPL3 entry.

## Separate address spaces

The allocator requests two independent seven-page, zeroed extents below 1 GiB. Each extent fits wholly within one 2 MiB page-table region:

| Relative page | Purpose | Process access |
| ---: | --- | --- |
| `0` | PML4 root | None |
| `1` | PDPT | None |
| `2` | Page directory | None |
| `3` | One 2 MiB-region page table | None |
| `4` | Role-specific linked image | user read/execute; not writable |
| `5` | Initial user stack | user read/write; NX |
| `6` | ABI-16 execution context/data | user read/write; NX |

Kernel hierarchy entries remain supervisor-only. User permission is added only along each process's required hierarchy path and exactly three leaf pages. The code page is RX; stack and context are RW/NX; page zero remains absent. No present leaf may be writable and executable. The service and client roots and images must be distinct.

## Process records

The kernel memory-state page stores 256-byte little-endian `WVPROC02` records at offset `0x100` for init and `0x300` for the client. Each record has this version-2 layout:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| `0x00` | 8 | ASCII magic `WVPROC02` |
| `0x08` | 4 | Version `2` |
| `0x0C` | 4 | Record bytes `256` |
| `0x10` | 4 | Process state |
| `0x14` | 4 | Thread state |
| `0x18` | 4 | Process identifier |
| `0x1C` | 4 | Thread identifier |
| `0x20` | 32 | Exact role-specific WVB SHA-256 |
| `0x40` | 8 | Page-table root |
| `0x48` | 8 | User code address |
| `0x50` | 8 | User stack address |
| `0x58` | 8 | User context/data address |
| `0x60` | 4 | User-page budget `3` |
| `0x64` | 4 | Instruction budget `64` |
| `0x68` | 4 | Handle budget `1` |
| `0x6C` | 4 | System-call budget `2` |
| `0x70` | 4 | Capability slot `0` |
| `0x74` | 4 | Capability generation `1` |
| `0x78` | 4 | Role-reduced rights `1` or `2` |
| `0x7C` | 4 | Channel capacity `1` |
| `0x80` | 8 | Saved kernel `RSP` |
| `0x88` | 8 | Kernel continuation |
| `0x90` | 8 | Saved user `RSP` |
| `0x98` | 8 | Saved user `RIP` from `RCX` |
| `0xA0` | 8 | Saved user flags from `R11` |
| `0xA8` | 4 | System-call count |
| `0xAC` | 8 | Reserved, zero |
| `0xB4` | 4 | Result |
| `0xB8` | 4 | Fault vector |
| `0xBC` | 4 | Fault error |
| `0xC0` | 8 | Kernel-owned shared-channel address |
| `0xC8` | 4 | Role: init `1`, client `2` |
| `0xCC` | 4 | Wait reason: none `0`, channel receive `1` |
| `0xD0` | 8 | Saved user native-context pointer from `RDX` |
| `0xD8..0xFF` | 40 | Reserved, zero |

Process states are `1` ready, `2` running, `3` exited, and `4` faulted. Thread states add `5` waiting. A blocked receive leaves the init process running while its sole thread waits. Saving and restoring `RDX` is required because ABI-16 uses it for the native execution-context pointer across the block/wake boundary.

## Kernel-owned channel

One 64-byte little-endian `WVCHAN01` record lives at memory-state offset `0x400`:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| `0x00` | 8 | ASCII magic `WVCHAN01` |
| `0x08` | 4 | Version `1` |
| `0x0C` | 4 | Record bytes `64` |
| `0x10` | 4 | State: empty `0`, full `1` |
| `0x14` | 4 | One `i32` message |
| `0x18` | 4 | Sender process |
| `0x1C` | 4 | Receiver process |
| `0x20` | 4 | Send count |
| `0x24` | 4 | Receive count |
| `0x28` | 4 | Waiting receiver process |
| `0x2C` | 4 | Wake count |
| `0x30` | 4 | Capacity `1` |
| `0x34..0x3F` | 12 | Reserved, zero |

The record is supervisor-only and is not mapped as a user capability payload. A capability reference authorizes an operation; it is not a pointer to the channel.

## Fixed coordinator and syscall sequence

The experimental number/register assignment remains: `EBX` is operation (`1` send, `2` receive, `3` exit), `ESI` is capability reference, and `EAX` carries message/result. It remains internal and versioned.

The only accepted normal sequence is:

1. Activate the init root and enter its WVA shim at CPL3.
2. Init calls receive on the empty channel. The dispatcher validates receive-only authority, records waiter `1`, increments init's syscall count, marks its thread waiting, and returns to the fixed kernel coordinator.
3. Activate the client root. The admitted Windvale program returns `29`; the client sends `29` through its send-only endpoint, then exits `29`.
4. Require the client terminal state, full channel, exact message, sender, waiter, and counts. Activate the init root, consume the message, record receiver and one wake, clear the waiter, restore init's saved user context including `RDX`, and resume after its receive with `EAX = 29`.
5. The Windvale init/resource service returns `29` and exits. Require both terminal records, empty channel retaining message `29`, and exact send/receive/wake counts of one.

The deliberate user-fault client sends `29` and then executes privileged `CLI` instead of exit. Exact vector 13/error 0 faults only the client; the coordinator still wakes init, which completes normally. Equivalent CPL0 faults remain terminal. Invalid syscall, capability, role, state, result, or budget marks the current domain faulted with failure result `1`.

This is deterministic cooperative coordination, not round-robin scheduling or preemption.

## Planner diagnostics

| Code | Meaning |
| --- | --- |
| `WVOS6001` | A seven-page extent is null, unaligned, incomplete, or outside low 1 GiB. |
| `WVOS6002` | The extent crosses a 2 MiB page-table region. |
| `WVOS6003` | The role-specific image is empty or exceeds one page. |
| `WVOS6004` | The module identity is not one SHA-256 digest. |
| `WVOS6005` | The extent overlaps the retained kernel executable window. |
| `WVOS6006` | Process/thread identity, role, reduced rights, or channel address is invalid. |

## Deterministic evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windvale process-policy WVB | 3,092 | `4b52c9d0d868c2eb058b419ef1fde8f38c4c7dc492640421f974b94ca6838b9f` |
| Process-policy WVO | 27,130 | `b4c7178d687fbef7b0b32d911cc3c8e24d9760da68af2fc4b9cc5f51ad001767` |
| Init-service WVB | 371 | `478dfcd36fed7c8063cfb3f53a6a1362bda5353656339b730be573a1be8f95b0` |
| Init-service WVO | 2,374 | `a7beabde6cc429f2d4632e58cd8ff5134d61713ca1b103a83fee23d838687057` |
| Init-service WVA object | 202 | `1300167e11cb4db5704499a6d9f76ffe803130c5a64355e33e231aeaeccf6066` |
| Linked init-service image | 2,302 | `e3d1e13f3ea9d914c7d9ee5b624171cda549aecf9ecf38b77a06142ae74a586f` |
| Normal client WVA object | 195 | `7cf803c818437e8f662ece3b69757b957f67a648900c0aa91822cca47c4aad17` |
| Linked normal client image | 438 | `69aeb9946942b75d1af5890a7186c1026bc562bc8aa4e59e088d4ea93f784acc` |
| Fault client WVA object | 183 | `90ce43c85a3791881e0780161928b2f7e9b415e9b1e860179a87ece48fe06c44` |
| Linked fault client image | 438 | `1731dffafdec9ff3ca8ac056eb1298d80a029355b19dee036a2a3d31d37ae840` |
| Normal process-machine WVO | 7,988 | `3607d66b7e633027062a9fbb963dbd1a2723c2f7ee77fab235443c7ddd266809` |
| Normal process-machine code | 4,202 | `260ea2b5ee15376518f571d3302e7f90e777727a55ac77480484b9adaeb7d6ac` |
| Fault process-machine WVO | 8,020 | `c735c4f8185312cef4cd1c1c77aebba2f39b096c7e4051d1367c4a94c7ef8d3e` |
| Fault process-machine code | 4,234 | `915f79d3d18ac165351a3a7c8936b749f3559bb7b020391d97e825cb02bad0c4` |

All 25 focused OS tests and all four pinned-QEMU scenarios pass on Windows. Cross-host qualification remains pending.

## Deliberate limits

Version 2 has no general scheduler, timer, preemption, process-creation API, arbitrary module loader, capability allocation/transfer/revocation/generation rollover, endpoint discovery, queue, larger message, user pointer, shared memory, namespace, resource enumeration, teardown, page reclamation, signal ABI, driver, filesystem, package service, network service, Hyper-V evidence, or physical-hardware evidence. The fixed service exposes one immutable value solely to prove the boundary. These omissions are reconsideration points, not hidden implementation claims.
