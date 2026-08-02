# Protected Windvale processes and runtime-supplied bytecode

## Status and purpose

Protected-process contract version 5 defines Windvale OS's first runtime-supplied user-space bytecode process. Firmware probe 26 keeps the receive-only Windvale init/resource service, while process `2` runs the AOT-built profile-3 Windvale interpreter at CPL3. The interpreter obtains `boot:main.wvb` through one ABI-16 capability backed by a separate RO/NX resource page; the admitted program is absent from the interpreter WVB and linked RX image.

[Decision 0095](../Documents/Decisions/0095-First-Runtime-Supplied-Wvb-Boot-Resource.md) owns version 5. [Decision 0094](../Documents/Decisions/0094-First-Section-Derived-User-Space-Wvb-Profile.md) retains the cross-host-qualified version-4 proof at exact commit `33555fdc4305f457638431ddbc40cb79fafa51c3`.

Version 5 is a candidate pending cross-host qualification. Focused Windows evidence passes all 25 OS tests, and all four pinned-QEMU scenarios pass; normal and contained-fault execution complete the real CPL3 resource call. This is an internal experiment, not a stable public syscall ABI, general process manager, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- [`Process-Foundation.wv`](../Operating-System/Kernel/Process-Foundation.wv) binds the interpreter, admitted-program, and init-service identities; fixed roles, runtime profile, and budgets; reduced endpoints; wait/wake sequence; and policy token `94`.
- [`Bytecode-Interpreter.wv`](../Operating-System/Runtime/Bytecode-Interpreter.wv) is hosted Windvale source declaring only `file.read_bytes`. Its AOT derivative is the client process image; at runtime it reads the separate admitted WVB resource rather than carrying the program or calling its AOT derivative.
- [`Init-Resource-Service.wv`](../Operating-System/Kernel/Init-Resource-Service.wv) remains the receive-side user service and returns exact value `29` after its WVA entry receives the client's request.
- The service and client WVA shims own fixed syscall entry and exit mechanics. [`Boot-Resource-Service.wva`](../Operating-System/Runtime/Boot-Resource-Service.wva) owns the exact ABI-16 resource leaf as a read-only stencil. The client shim calls the Windvale interpreter export, sends its result, and exits or takes the selected CPL3 `CLI` fault.
- The Stage 0 planner and x64 process object temporarily own page-table and descriptor writes, record mutation, verified stencil publication, immutable boot-resource placement, syscall dispatch, and fixed coordination. These remain named replacement seams for system-profile Windvale policy and WVA machine mechanics.

C# builds and independently checks the images, but it does not define the interpreter's source semantics or execute the admitted program in the guest.

## Fixed identities, roles, and budgets

Version 5 binds three canonical WVB identities:

| Identity | Process/thread | WVB SHA-256 | Endpoint right |
| --- | --- | --- | ---: |
| Init/resource service | `1` / `1` | `478dfcd36fed7c8063cfb3f53a6a1362bda5353656339b730be573a1be8f95b0` | receive, value `2` |
| Bytecode interpreter | `2` / `2` | `25a223346c6357290680476a39a4e67821e5efc9420933a90486f993aef46bf2` | send, value `1`; file read through fixed local service |
| Interpreter input | RO/NX resource of process `2` | `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2` | borrowed read only |

The init process has three user pages, instruction budget `64`, call-depth budget `1`, one stack page, and runtime profile `0`. The interpreter has 38 user pages, instruction budget `4,678`, call-depth budget `3`, 32 RX code pages, four RW/NX stack pages, one RW/NX context page, one RO/NX resource page, and boot-resource-interpreter profile `3`. Both have one kernel endpoint handle, two system calls, expected terminal result `29`, slot `0`, generation `1`, reference `65536`, and channel capacity `1`. The ABI-16 `file.read_bytes` service is a process-local runtime service pointer rather than a kernel capability handle.

The expanded eight-function AOT interpreter did not fit safely on the preceding 8 KiB stack. Version 4 records and maps four contiguous NX stack pages for that role. Pinned QEMU page-faulted with two pages and completes with four, making 16 KiB an observed bound rather than an arbitrary reserve.

Policy WVB must return token `94` before channel, process, paging, descriptor, resource, or MSR state is published. A changed interpreter or program identity, supplied resource bytes, service-leaf range, role, runtime profile, budget, stack extent, or endpoint right fails before CPL3 entry.

## Separate address spaces

Init receives a seven-page zeroed extent:

| Relative page | Purpose | Process access |
| ---: | --- | --- |
| `0..3` | Private paging hierarchy | None |
| `4` | Linked init-service image | user RX |
| `5` | Stack | user RW/NX |
| `6` | ABI-16 context/data | user RW/NX |

The interpreter receives a 42-page zeroed extent:

| Relative page | Purpose | Process access |
| ---: | --- | --- |
| `0..3` | Private paging hierarchy | None |
| `4..35` | Linked interpreter image | user RX |
| `36..39` | Down-growing stack | user RW/NX |
| `40` | ABI-16 context/data | user RW/NX |
| `41` | Immutable `boot:main.wvb` resource plus zero tail | user RO/NX |

Only the required hierarchy path and leaves gain user permission. Kernel mappings remain supervisor-only, page zero remains absent, and no present leaf is writable and executable. The resource page is never writable or executable. Initial user `RSP` is the exclusive end of the complete role-specific stack extent.

## Process records

The memory-state page stores 256-byte little-endian `WVPROC05` records at offset `0x100` for init and `0x300` for the interpreter. Version 5 uses this layout:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| `0x00` | 8 | ASCII magic `WVPROC05` |
| `0x08` | 4 | Version `5` |
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
| `0x60` | 4 | User-page budget: init `3`, interpreter `38` |
| `0x64` | 4 | Instruction budget: init `64`, interpreter `4,678` |
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
| `0xB0` | 4 | Runtime profile: init `0`, boot-resource interpreter `3` |
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

The client context page contains two private tables outside the 112-byte ABI-16 context. At offset `0x80`, native service-table version 5/size 104 has every pointer zero except `file.read_bytes` at offset 32. At offset `0x100`, the 32-byte `WVBR` version-1 table contains the resource-page address at offset 16, byte length at offset 24, and zero reserved word at offset 28. Context offsets 24 and 96 point to those tables. Init leaves both pointers zero.

## Channel and execution sequence

The version-1 `WVCHAN01` capacity-one record and experimental register ABI are unchanged: `EBX` selects send `1`, receive `2`, or exit `3`; `ESI` carries the capability reference; and `EAX` carries the message or result.

The accepted normal sequence is:

1. Init enters CPL3, attempts receive, records waiter `1`, and returns to the fixed coordinator with its thread waiting.
2. The interpreter process enters CPL3. Its AOT Windvale implementation calls its sole ABI-16 service with `boot:main.wvb`; the exact leaf returns a borrowed descriptor for the separately mapped RO/NX page. The interpreter checks the complete seven-section envelope, derives each payload offset, validates the admitted semantic subset, and interprets it to `29`.
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
| `WVOS6008` | A runtime resource is present for init, absent/out of bounds for the interpreter, does not match its recorded digest, or names a service leaf outside the linked RX image. |

## Deterministic evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 3,708 | `9a080dcc55cb862018bd4808e82308a202f7ede1d6b32082be6840591f4d4e06` |
| Process-policy WVO | 31,998 | `8553ae419744cc93fed680400f5b34f14aa1aa1cbc60b15bae39652e04f8060a` |
| Interpreter WVB | 12,265 | `25a223346c6357290680476a39a4e67821e5efc9420933a90486f993aef46bf2` |
| Interpreter WVO | 128,340 | `5157b4446422d37597b16b5f29b5aae3f05920fc4718af1a9759efe29f4e73b7` |
| WVA resource stencil WVO | 314 | `1e690b8eebe6a21e4c4f6b697258c33c47370eb6b1277bdd40959cc077c29816` |
| Published resource service WVO | 314 | `610b861538697ca15c7f2b5fac5bc222be5697a2063509ffb7ab5b0e669a226d` |
| Normal client WVA object | 205 | `6a22069adef6f9a4b58d1dda2bfe0c2b35e8563bb4e7e73641f050c2eeae058d` |
| Fault client WVA object | 193 | `c57327ddf897fb32cc57dd1266c467283273eddafd8d4b78edfc43e59fc8eeee` |
| Linked normal interpreter image | 128,157 | `5a0acf3db339df5c3308f51a2e7ce182ee884d9b528db2998e9d0dcbf3b30655` |
| Linked fault interpreter image | 128,157 | `1a56e471c06702e479ec7c1cee49d98415734e7d5fca24f46fbc3c66c8175a83` |
| Normal process-machine WVO | 137,665 | `6d1517bbf5f947f55e07cbb582b3bf7050199bd8b31a1425a82a891a68730f14` |
| Normal process-machine code | 5,882 | `6238b2e8b3c70678d62c926921f9f177c5f3165664adf09f960b62553e2747a1` |
| Fault process-machine WVO | 137,697 | `385fc83b83e7be331e8b8479abc0d75e23e6bb30554bd9983766a547a996a09c` |
| Fault process-machine code | 5,914 | `b0cf19a876badadc6b1023d3243f0e4e5b43f84936a554e21f9427b7278e95b6` |

All 25 focused OS tests pass on Windows for this candidate. All four pinned-QEMU probe-26 scenarios pass and prove the actual CPL3 service call, preserved kernel-fault terminals, and contained client fault. Cross-host qualification remains required before this paragraph may claim qualification.

## Deliberate limits

Version 5 does not provide a general scheduler, arbitrary module selection, complete semantic verification, dynamic resource discovery, capability transfer/revocation, executable publication, JIT code generation, process creation, teardown, reclamation, larger IPC, shared memory, filesystems, packages, networking, Hyper-V, or physical-hardware evidence. The admitted WVB is runtime-supplied but remains one fixed, boot-created immutable resource. These limits prevent the runtime proof from being mistaken for the finished runtime architecture.
