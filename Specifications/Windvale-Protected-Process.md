# Protected Windvale processes and first bytecode runtime

## Status and purpose

Protected-process contract version 3 defines Windvale OS's first user-space bytecode-runtime process. Firmware probe 24 keeps the receive-only Windvale init/resource service from version 2, but process `2` now runs an AOT-built Windvale interpreter that decodes and executes the exact admitted WVB at CPL3. The admitted program's host-built AOT derivative is no longer the client computation.

[Decision 0093](../Documents/Decisions/0093-First-User-Space-Windvale-Bytecode-Interpreter.md) owns version 3. [Decision 0092](../Documents/Decisions/0092-First-Windvale-Init-Resource-Service.md) remains the version-2 proof; exact commit `22e350b8965bbe70452261dabfc411d28cf7a1d5` passes Windows/Linux build and Seed qualification, while Linux OS-test execution remains pending.

Focused Windows tests and all four pinned-QEMU scenarios pass for version 3. Cross-host qualification of probe 24 is pending. This is an internal experiment, not a stable public syscall ABI, general process manager, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- [`Process-Foundation.wv`](../Operating-System/Kernel/Process-Foundation.wv) binds the interpreter, admitted-program, and init-service identities; fixed roles and budgets; reduced endpoints; wait/wake sequence; and policy token `93`.
- [`Bytecode-Interpreter.wv`](../Operating-System/Runtime/Bytecode-Interpreter.wv) is portable Windvale source. Its AOT derivative is the client process image; at runtime it interprets the embedded admitted WVB rather than calling the program's AOT derivative.
- [`Init-Resource-Service.wv`](../Operating-System/Kernel/Init-Resource-Service.wv) remains the receive-side user service and returns exact value `29` after its WVA entry receives the client's request.
- The service and client WVA shims own fixed syscall entry and exit mechanics. The client shim calls the Windvale interpreter export, sends its result, and exits or takes the selected CPL3 `CLI` fault.
- The Stage 0 planner and x64 process object temporarily own page-table and descriptor writes, record mutation, syscall dispatch, and fixed coordination. These remain named replacement seams for system-profile Windvale policy and WVA machine mechanics.

C# builds and independently checks the images, but it does not define the interpreter's source semantics or execute the admitted program in the guest.

## Fixed identities, roles, and budgets

Version 3 binds three canonical WVB identities:

| Identity | Process/thread | WVB SHA-256 | Endpoint right |
| --- | --- | --- | ---: |
| Init/resource service | `1` / `1` | `478dfcd36fed7c8063cfb3f53a6a1362bda5353656339b730be573a1be8f95b0` | receive, value `2` |
| Bytecode interpreter | `2` / `2` | `639e191af1844b6660750978854f5e168c25f4949f1d9282ca5777d65f617083` | send, value `1` |
| Interpreter input | owned by process `2` | `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2` | none |

The init process has three user pages, instruction budget `64`, call-depth budget `1`, and one stack page. The interpreter has eleven user pages, instruction budget `567`, call-depth budget `2`, eight RX code pages, two RW/NX stack pages, and one RW/NX context page. Both have one capability handle, two system calls, expected terminal result `29`, slot `0`, generation `1`, reference `65536`, and channel capacity `1`.

The measured AOT interpreter frame did not fit safely on one 4 KiB stack page. Version 3 therefore records and maps two contiguous NX stack pages for that role. This consumes the remaining fixed arena capacity and is an explicit bound, not an expandable allocator policy.

Policy WVB must return token `93` before channel, process, paging, descriptor, or MSR state is published. A changed interpreter or program identity, role, budget, stack extent, or endpoint right fails before CPL3 entry.

## Separate address spaces

Init receives a seven-page zeroed extent:

| Relative page | Purpose | Process access |
| ---: | --- | --- |
| `0..3` | Private paging hierarchy | None |
| `4` | Linked init-service image | user RX |
| `5` | Stack | user RW/NX |
| `6` | ABI-16 context/data | user RW/NX |

The interpreter receives a fifteen-page zeroed extent:

| Relative page | Purpose | Process access |
| ---: | --- | --- |
| `0..3` | Private paging hierarchy | None |
| `4..11` | Linked interpreter image | user RX |
| `12..13` | Down-growing stack | user RW/NX |
| `14` | ABI-16 context/data | user RW/NX |

Only the required hierarchy path and leaves gain user permission. Kernel mappings remain supervisor-only, page zero remains absent, and no present leaf is writable and executable. Initial user `RSP` is the exclusive end of the complete role-specific stack extent.

## Process records

The memory-state page stores 256-byte little-endian `WVPROC03` records at offset `0x100` for init and `0x300` for the interpreter. Version 3 uses this layout:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| `0x00` | 8 | ASCII magic `WVPROC03` |
| `0x08` | 4 | Version `3` |
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
| `0x60` | 4 | User-page budget: init `3`, interpreter `11` |
| `0x64` | 4 | Instruction budget: init `64`, interpreter `567` |
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
| `0xAC` | 4 | Stack-page count: init `1`, interpreter `2` |
| `0xB0` | 4 | Reserved, zero |
| `0xB4` | 4 | Result |
| `0xB8` | 4 | Fault vector |
| `0xBC` | 4 | Fault error |
| `0xC0` | 8 | Kernel-owned shared-channel address |
| `0xC8` | 4 | Role: init `1`, interpreter `2` |
| `0xCC` | 4 | Wait reason: none `0`, channel receive `1` |
| `0xD0` | 8 | Saved user native-context pointer from `RDX` |
| `0xD8` | 32 | Runtime-input WVB SHA-256; zero for init |
| `0xF8` | 4 | RX code-page count: init `1`, interpreter `8` |
| `0xFC` | 4 | Runtime kind: AOT service `1`, bytecode interpreter `2` |

Process states are ready `1`, running `2`, exited `3`, and faulted `4`; thread states add waiting `5`. Saving and restoring `RDX` remains required because ABI 16 uses it for the execution-context pointer.

## Channel and execution sequence

The version-1 `WVCHAN01` capacity-one record and experimental register ABI are unchanged: `EBX` selects send `1`, receive `2`, or exit `3`; `ESI` carries the capability reference; and `EAX` carries the message or result.

The accepted normal sequence is:

1. Init enters CPL3, attempts receive, records waiter `1`, and returns to the fixed coordinator with its thread waiting.
2. The interpreter process enters CPL3. Its AOT Windvale implementation checks and interprets the embedded admitted WVB subset, producing `29`.
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
| Process-policy WVB | 3,512 | `af4f1865a65be48b6fbefbe8995b4638fe91f579616fcd32cd1d05b16d684330` |
| Process-policy WVO | 30,142 | `333046213d54a098f16e6668ee875231f3a0ee87e55a34db567ff2b8ff650806` |
| Interpreter WVB | 3,211 | `639e191af1844b6660750978854f5e168c25f4949f1d9282ca5777d65f617083` |
| Interpreter WVO | 30,457 | `fbe3592e5459723c2b36330ec93659fb387de497b31fa59b8e629668297aaac6` |
| Normal client WVA object | 205 | `6a22069adef6f9a4b58d1dda2bfe0c2b35e8563bb4e7e73641f050c2eeae058d` |
| Fault client WVA object | 193 | `c57327ddf897fb32cc57dd1266c467283273eddafd8d4b78edfc43e59fc8eeee` |
| Linked normal interpreter image | 30,270 | `72f81045c525f1ad055127f3bb7917dace22b0a3b35ff3b6fefec28b37a6058c` |
| Linked fault interpreter image | 30,270 | `b24007c770c1ff9d0c8a05702a6b05ead8a9361f55b6394b34cc3202343622aa` |
| Normal process-machine WVO | 38,332 | `44559a001988e503374c2b83bc8056d928e075381ca3cd93e155040a2f63fd10` |
| Normal process-machine code | 4,714 | `75e9cd05b3093d50b5c38a7466ec36a8ba5369cec99e35d63eba584ca7310500` |
| Fault process-machine WVO | 38,364 | `c8ed18169fe56bd44d1594e4e7ed4cf403e157c5f831b511640f0d5f28f003fc` |
| Fault process-machine code | 4,746 | `d1093fa03967b58fbb1654bb96a40f4b6b8481218d521001ff933338673399d7` |

All 25 focused OS tests and all four pinned-QEMU scenarios pass on Windows. Cross-host qualification remains pending.

## Deliberate limits

Version 3 does not provide a general scheduler, arbitrary module loading, generic WVB section decoding, complete semantic verification, dynamic boot resources, capability transfer/revocation, executable publication, JIT code generation, process creation, teardown, reclamation, larger IPC, shared memory, filesystems, packages, networking, Hyper-V, or physical-hardware evidence. The admitted WVB remains fixed and embedded in the interpreter image. These limits prevent the first runtime proof from being mistaken for the finished runtime architecture.
