# Protected Windvale processes and section-derived bytecode runtime

## Status and purpose

Protected-process contract version 4 defines Windvale OS's section-derived user-space bytecode-runtime process. Firmware probe 25 keeps the receive-only Windvale init/resource service, while process `2` runs the AOT-built profile-2 Windvale interpreter at CPL3. The interpreter discovers checked WVB sections instead of depending on fixed serialized offsets; the admitted program's host-built AOT derivative remains outside the client computation.

[Decision 0094](../Documents/Decisions/0094-First-Section-Derived-User-Space-Wvb-Profile.md) owns version 4. [Decision 0093](../Documents/Decisions/0093-First-User-Space-Windvale-Bytecode-Interpreter.md) retains the cross-host-qualified version-3 proof at exact commit `190174a01299369fb855e27ea676d34062e09c5b`.

Focused Windows tests and all four pinned-QEMU scenarios pass for version 4. Cross-host qualification of probe 25 is pending. This is an internal experiment, not a stable public syscall ABI, general process manager, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- [`Process-Foundation.wv`](../Operating-System/Kernel/Process-Foundation.wv) binds the interpreter, admitted-program, and init-service identities; fixed roles, runtime profile, and budgets; reduced endpoints; wait/wake sequence; and policy token `94`.
- [`Bytecode-Interpreter.wv`](../Operating-System/Runtime/Bytecode-Interpreter.wv) is portable Windvale source. Its AOT derivative is the client process image; at runtime it interprets the embedded admitted WVB rather than calling the program's AOT derivative.
- [`Init-Resource-Service.wv`](../Operating-System/Kernel/Init-Resource-Service.wv) remains the receive-side user service and returns exact value `29` after its WVA entry receives the client's request.
- The service and client WVA shims own fixed syscall entry and exit mechanics. The client shim calls the Windvale interpreter export, sends its result, and exits or takes the selected CPL3 `CLI` fault.
- The Stage 0 planner and x64 process object temporarily own page-table and descriptor writes, record mutation, syscall dispatch, and fixed coordination. These remain named replacement seams for system-profile Windvale policy and WVA machine mechanics.

C# builds and independently checks the images, but it does not define the interpreter's source semantics or execute the admitted program in the guest.

## Fixed identities, roles, and budgets

Version 4 binds three canonical WVB identities:

| Identity | Process/thread | WVB SHA-256 | Endpoint right |
| --- | --- | --- | ---: |
| Init/resource service | `1` / `1` | `478dfcd36fed7c8063cfb3f53a6a1362bda5353656339b730be573a1be8f95b0` | receive, value `2` |
| Bytecode interpreter | `2` / `2` | `909e624df86e614b6f7dcaa61e75ffa685467015015bfafd7b0772ee41a89920` | send, value `1` |
| Interpreter input | owned by process `2` | `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2` | none |

The init process has three user pages, instruction budget `64`, call-depth budget `1`, one stack page, and runtime profile `0`. The interpreter has 37 user pages, instruction budget `4,671`, call-depth budget `3`, 32 RX code pages, four RW/NX stack pages, one RW/NX context page, and section-interpreter profile `2`. Both have one capability handle, two system calls, expected terminal result `29`, slot `0`, generation `1`, reference `65536`, and channel capacity `1`.

The expanded eight-function AOT interpreter did not fit safely on the preceding 8 KiB stack. Version 4 records and maps four contiguous NX stack pages for that role. Pinned QEMU page-faulted with two pages and completes with four, making 16 KiB an observed bound rather than an arbitrary reserve.

Policy WVB must return token `94` before channel, process, paging, descriptor, or MSR state is published. A changed interpreter or program identity, role, runtime profile, budget, stack extent, or endpoint right fails before CPL3 entry.

## Separate address spaces

Init receives a seven-page zeroed extent:

| Relative page | Purpose | Process access |
| ---: | --- | --- |
| `0..3` | Private paging hierarchy | None |
| `4` | Linked init-service image | user RX |
| `5` | Stack | user RW/NX |
| `6` | ABI-16 context/data | user RW/NX |

The interpreter receives a 41-page zeroed extent:

| Relative page | Purpose | Process access |
| ---: | --- | --- |
| `0..3` | Private paging hierarchy | None |
| `4..35` | Linked interpreter image | user RX |
| `36..39` | Down-growing stack | user RW/NX |
| `40` | ABI-16 context/data | user RW/NX |

Only the required hierarchy path and leaves gain user permission. Kernel mappings remain supervisor-only, page zero remains absent, and no present leaf is writable and executable. Initial user `RSP` is the exclusive end of the complete role-specific stack extent.

## Process records

The memory-state page stores 256-byte little-endian `WVPROC04` records at offset `0x100` for init and `0x300` for the interpreter. Version 4 uses this layout:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| `0x00` | 8 | ASCII magic `WVPROC04` |
| `0x08` | 4 | Version `4` |
| `0x0C` | 4 | Record bytes `256` |
| `0x10` | 4 | Process state |
| `0x14` | 4 | Thread state |
| `0x18` | 4 | Process identifier |
| `0x1C` | 4 | Thread identifier |
| `0x20` | 32 | Role module SHA-256: service or interpreter WVB |
| `0x40` | 8 | Page-table root |
| `0x48` | 8 | User code address |
| `0x50` | 8 | Lowest user stack address |
| `0x58` | 8 | User context/data address |
| `0x60` | 4 | User-page budget: init `3`, interpreter `37` |
| `0x64` | 4 | Instruction budget: init `64`, interpreter `4,671` |
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
| `0xAC` | 4 | Stack-page count: init `1`, interpreter `4` |
| `0xB0` | 4 | Runtime profile: init `0`, section interpreter `2` |
| `0xB4` | 4 | Result |
| `0xB8` | 4 | Fault vector |
| `0xBC` | 4 | Fault error |
| `0xC0` | 8 | Kernel-owned shared-channel address |
| `0xC8` | 4 | Role: init `1`, interpreter `2` |
| `0xCC` | 4 | Wait reason: none `0`, channel receive `1` |
| `0xD0` | 8 | Saved user native-context pointer from `RDX` |
| `0xD8` | 32 | Runtime-input WVB SHA-256; zero for init |
| `0xF8` | 4 | RX code-page count: init `1`, interpreter `32` |
| `0xFC` | 4 | Runtime kind: AOT service `1`, bytecode interpreter `2` |

Process states are ready `1`, running `2`, exited `3`, and faulted `4`; thread states add waiting `5`. Saving and restoring `RDX` remains required because ABI 16 uses it for the execution-context pointer.

## Channel and execution sequence

The version-1 `WVCHAN01` capacity-one record and experimental register ABI are unchanged: `EBX` selects send `1`, receive `2`, or exit `3`; `ESI` carries the capability reference; and `EAX` carries the message or result.

The accepted normal sequence is:

1. Init enters CPL3, attempts receive, records waiter `1`, and returns to the fixed coordinator with its thread waiting.
2. The interpreter process enters CPL3. Its AOT Windvale implementation checks the complete seven-section envelope, derives each payload offset, validates the admitted semantic subset, and interprets it to `29`.
3. The client shim sends `29` through its send-only endpoint and exits `29`.
4. The coordinator validates the interpreter's terminal state and runtime identities, reactivates init, consumes the message, restores its context, and resumes it with `EAX = 29`.
5. The Windvale init service returns and exits `29`; both records and the exact send/receive/wake counts must be terminal and consistent.

The user-fault image interprets and sends `29`, then executes privileged `CLI` instead of exit. Vector 13/error 0 faults only the interpreter process; init still wakes and completes. Equivalent CPL0 faults remain terminal.

## Planner diagnostics

| Code | Meaning |
| --- | --- |
| `WVOS6001` | A role-specific extent is null, unaligned, incomplete, or outside low 1 GiB. |
| `WVOS6002` | The extent crosses a 2 MiB page-table region. |
| `WVOS6003` | The role-specific image is empty or exceeds its bounded RX extent. |
| `WVOS6004` | The role-module identity is not one SHA-256 digest. |
| `WVOS6005` | The extent overlaps the retained kernel executable window. |
| `WVOS6006` | Process/thread identity, role, reduced rights, or channel address is invalid. |
| `WVOS6007` | Runtime-input identity is nonzero for init, zero for the interpreter, or not one digest. |

## Deterministic evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 3,708 | `c84aeb3b9658b9a2c5847bd769aef6eb87a9b46b743123ef49a5faf530f7a65b` |
| Process-policy WVO | 31,998 | `a992d44ce72cbee7b0bd3bb110cb4c5ce2a027d788dfb1cfa67e2ad461ddd0bf` |
| Interpreter WVB | 12,359 | `909e624df86e614b6f7dcaa61e75ffa685467015015bfafd7b0772ee41a89920` |
| Interpreter WVO | 128,129 | `9788ad4159d783ebc35ee5af6c73b7c294643261bc00acfcf0f33a6bdf35c140` |
| Normal client WVA object | 205 | `6a22069adef6f9a4b58d1dda2bfe0c2b35e8563bb4e7e73641f050c2eeae058d` |
| Fault client WVA object | 193 | `c57327ddf897fb32cc57dd1266c467283273eddafd8d4b78edfc43e59fc8eeee` |
| Linked normal interpreter image | 127,598 | `c293f84199fecce07c3a0dbafb6406e7c2aad3521782df7095fe8ee6ca58a0e8` |
| Linked fault interpreter image | 127,598 | `6364289e6ddaaa125969bb27626672f08f114ebd738b7b191d2c55125c45fc6e` |
| Normal process-machine WVO | 136,668 | `33ef216d89926bacd53b5a46c5f39f3802c778bdfee44de0d7c79a440637e696` |
| Normal process-machine code | 5,722 | `c7f1a05cb3ca5d5f47a6f6702f2375de84630afb8be36bd3dbe0a65bd692b715` |
| Fault process-machine WVO | 136,700 | `123adcc0c0dfb9c919ae1abdcdc1f4e330e98b86d6d753b110c3ddcb04a9de44` |
| Fault process-machine code | 5,754 | `8930d1b285f916bd18f5b5442aa52187f5ed3667f72f0c39667d6eaa6216637d` |

All 25 focused OS tests and all four pinned-QEMU probe-25 scenarios pass on Windows. Cross-host qualification remains pending.

## Deliberate limits

Version 4 does not provide a general scheduler, runtime-supplied module loading, complete semantic verification, dynamic boot resources, capability transfer/revocation, executable publication, JIT code generation, process creation, teardown, reclamation, larger IPC, shared memory, filesystems, packages, networking, Hyper-V, or physical-hardware evidence. The admitted WVB remains fixed and embedded in the interpreter image even though its section offsets are now derived. These limits prevent the runtime proof from being mistaken for the finished runtime architecture.
