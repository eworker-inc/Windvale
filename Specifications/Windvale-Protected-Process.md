# Protected Windvale processes and init-owned boot resources

## Status and purpose

Protected-process contract version 6 defines Windvale OS's first Windvale-selected immutable resource grant. Firmware probe 27 starts process `2` without its WVB mapping or usable `file.read_bytes` tables. Process `1` runs Windvale init, selects resource identifier `1`, and requests one exact kernel-mediated borrow before the unchanged profile-4 interpreter executes at CPL3.

[Decision 0096](../Documents/Decisions/0096-First-Windvale-Init-Owned-Boot-Resource-Grant.md) owns candidate version 6. [Decision 0095](../Documents/Decisions/0095-First-Runtime-Supplied-Wvb-Boot-Resource.md) retains the cross-host-qualified version-5 proof at exact commit `6bb34bb4c6dc23e89fbdcd8592b31f0585f91ec5`.

Version 6 has focused Windows and all four pinned-QEMU scenario evidence. Cross-host qualification is not yet claimed. This remains an internal experiment, not a stable syscall ABI, general process manager, resource namespace, transferable capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- [`Process-Foundation.wv`](../Operating-System/Kernel/Process-Foundation.wv) binds the init, interpreter, and admitted-program identities; roles, runtime profiles, budgets, one-shot grant, reduced rights, result channel, and policy token `95`.
- [`Init-Resource-Service.wv`](../Operating-System/Kernel/Init-Resource-Service.wv) is portable Windvale source that selects fixed resource identifier `1`. Its WVA shim calls that decision, invokes grant syscall `4`, then receives the interpreter result and exits.
- [`Bytecode-Interpreter.wv`](../Operating-System/Runtime/Bytecode-Interpreter.wv) remains the byte-identical hosted Windvale interpreter declaring only `file.read_bytes`. Its AOT derivative reads the immutable WVB after the grant; it does not carry the admitted program or call its AOT derivative.
- [`Boot-Resource-Service.wva`](../Operating-System/Runtime/Boot-Resource-Service.wva) remains the exact ABI-16 resource leaf. The client shim calls the interpreter, sends the result, and exits or takes the selected CPL3 `CLI` fault.
- Stage 0 temporarily owns raw page-table writes, initial record construction, service-stencil publication, syscall dispatch, fixed coordination, and firmware packaging. These are named replacement seams for system-profile Windvale policy and WVA machine mechanics.

C# constructs and independently checks the images, but it does not choose resource `1`, define interpreter semantics, or execute the admitted program in the guest.

## Fixed identities, roles, and budgets

Version 6 binds three canonical WVB identities:

| Identity | Process/thread | WVB SHA-256 | Authority |
| --- | --- | --- | --- |
| Init/resource service | `1` / `1` | `0fe423c499ce4f573095ddb9ff03355ee8b6ad927941f764ddaf2eaf9537f78b` | receive `2` plus fixed grant `4` |
| Bytecode interpreter | `2` / `2` | `25a223346c6357290680476a39a4e67821e5efc9420933a90486f993aef46bf2` | send `1`; local file read only after grant |
| Interpreter input | resource `1`, owned by process `1`, borrowed by process `2` | `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2` | immutable, read only, non-executable |

Init has four user pages, instruction budget `64`, call-depth budget `1`, one stack page, runtime profile `1`, one handle, and three system calls. The interpreter has a pre-grant count of 37 user pages and post-grant budget `38`, instruction budget `4,678`, call-depth budget `3`, runtime profile `4`, one handle, and two system calls. Both retain expected result `29`, capability slot `0`, generation `1`, reference `65536`, and result-channel capacity `1`.

The interpreter still uses 32 RX code pages, four RW/NX stack pages, one RW/NX context page, and the granted RO/NX alias. Pinned QEMU previously established the 16 KiB interpreter stack as an observed bound. The ABI-16 `file.read_bytes` leaf remains a process-local service pointer, not a second kernel handle.

Policy WVB must return token `95` before channel, resource, process, paging, descriptor, or MSR state is published. A changed identity, resource byte, digest, role, profile, budget, extent, right, service range, target PTE, or record field fails before the affected CPL3 entry or grant.

## Separate address spaces

Init receives an eight-page zeroed extent:

| Relative page | Purpose | Process access |
| ---: | --- | --- |
| `0..3` | Private paging hierarchy | None |
| `4` | Linked init-service image | user RX |
| `5` | Stack | user RW/NX |
| `6` | ABI-16 context/data | user RW/NX |
| `7` | Owned `boot:main.wvb` plus zero tail | user RO/NX |

The interpreter receives a 42-page zeroed extent:

| Relative page | Purpose | Access before grant | Access after grant |
| ---: | --- | --- | --- |
| `0..3` | Private paging hierarchy | None | None |
| `4..35` | Linked interpreter image | user RX | user RX |
| `36..39` | Down-growing stack | user RW/NX | user RW/NX |
| `40` | ABI-16 context/data | user RW/NX | user RW/NX |
| `41` | Target virtual page for resource `1` | absent | user RO/NX alias of init page `7` |

Only required hierarchy paths and leaves gain user permission. Kernel mappings remain supervisor-only, page zero remains absent, and no present leaf is writable and executable. The client target PTE is exactly zero before grant; the grant writes the init resource's physical address with user/present/NX and without writable. The client's reserved physical page `41` is not exposed by this version.

## Process records

The memory-state page stores 256-byte little-endian `WVPROC06` records at offset `0x100` for init and `0x300` for the interpreter. Version 6 uses this layout:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| `0x00` | 8 | ASCII magic `WVPROC06` |
| `0x08` | 4 | Version `6` |
| `0x0C` | 4 | Record bytes `256` |
| `0x10` | 4 | Process state |
| `0x14` | 4 | Thread state |
| `0x18` | 4 | Process identifier |
| `0x1C` | 4 | Thread identifier |
| `0x20` | 32 | Role-module SHA-256: service or interpreter WVB |
| `0x40` | 8 | Page-table root |
| `0x48` | 8 | User-code address |
| `0x50` | 8 | Lowest user-stack address |
| `0x58` | 8 | User context/data address |
| `0x60` | 4 | User-page budget: init `4`, interpreter `38` |
| `0x64` | 4 | Instruction budget: init `64`, interpreter `4,678` |
| `0x68` | 4 | Handle budget `1` |
| `0x6C` | 4 | System-call budget: init `3`, interpreter `2` |
| `0x70` | 4 | Capability slot `0` |
| `0x74` | 4 | Capability generation `1` |
| `0x78` | 4 | Rights: interpreter send `1`; init receive-plus-grant `6` |
| `0x7C` | 4 | Channel capacity `1` |
| `0x80` | 8 | Saved kernel `RSP` |
| `0x88` | 8 | Kernel continuation |
| `0x90` | 8 | Saved user `RSP` |
| `0x98` | 8 | Saved user `RIP` from `RCX` |
| `0xA0` | 8 | Saved user flags from `R11` |
| `0xA8` | 4 | System-call count |
| `0xAC` | 4 | Stack-page count: init `1`, interpreter `4` |
| `0xB0` | 4 | Runtime profile: init boot-resource owner `1`, granted interpreter `4` |
| `0xB4` | 4 | Result |
| `0xB8` | 4 | Fault vector |
| `0xBC` | 4 | Fault error |
| `0xC0` | 8 | Kernel-owned shared-channel address |
| `0xC8` | 4 | Role: init `1`, interpreter `2` |
| `0xCC` | 4 | Wait reason: none `0`, channel receive `1` |
| `0xD0` | 8 | Saved user native-context pointer from `RDX` |
| `0xD8` | 32 | Runtime-input WVB SHA-256 for resource `1` |
| `0xF8` | 4 | RX code-page count: init `1`, interpreter `32` |
| `0xFC` | 4 | Runtime kind: AOT service `1`, bytecode interpreter `2` |

Process states are ready `1`, running `2`, exited `3`, and faulted `4`; thread states add waiting `5`. Saving and restoring `RDX` remains required because ABI 16 uses it for the native-context pointer.

Both context pages start with valid context headers. Init leaves service and `WVBR` pointers zero. The client also starts with both pointers zero. Grant syscall `4` writes, into client data page only, service-table version 5/size 104 at offset `0x80` with only `file.read_bytes` nonzero, `WVBR` version 1/size 32 at offset `0x100`, and context pointers at offsets 24 and 96.

## Resource record and one-shot borrow

The memory-state page stores a 128-byte little-endian `WVRES001` record at offset `0x440`, immediately after the 64-byte `WVCHAN01` record:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| `0x00` | 8 | ASCII magic `WVRES001` |
| `0x08` | 4 | Version `1` |
| `0x0C` | 4 | Record bytes `128` |
| `0x10` | 4 | State: owned `1`, borrowed `2` |
| `0x14` | 4 | Resource identifier `1` |
| `0x18` | 4 | Owner process `1` |
| `0x1C` | 4 | Borrower: zero before grant, process `2` after grant |
| `0x20` | 8 | Init source virtual/physical identity address |
| `0x28` | 4 | Exact WVB byte length `174` |
| `0x2C` | 4 | Flags `7`: immutable, read only, no execute |
| `0x30` | 8 | Target page-table root |
| `0x38` | 8 | Target context/data address |
| `0x40` | 8 | Target resource virtual address |
| `0x48` | 8 | Target `file.read_bytes` service address |
| `0x50` | 32 | Exact admitted WVB SHA-256 |
| `0x70` | 4 | Grant count: zero then one |
| `0x74` | 4 | Mapping count: zero then one |
| `0x78` | 8 | Exact target PTE address |

The source field is an identity address because these low-memory process extents are still identity-mapped. This does not establish a general virtual-to-physical lookup contract.

Syscall `4` accepts only init process `1`, capability reference `65536`, resource identifier `1`, grant right `4`, owned state, zero borrower/counts, the fixed flags, a bounded aligned source, nonzero publication addresses, and an absent target PTE. Stage 0 constructs the exact digest, length, and owner/client addresses in the kernel-only record; after the transition, the coordinator checks the exact resulting record fields, alias, service table, and resource table before entering the client. The transition cannot be replayed. The client receives a borrowed alias; init remains owner for the fixed lifetime.

## Channel and execution sequence

The version-1 `WVCHAN01` result channel is unchanged. In the experimental register ABI, `EBX` selects send `1`, receive `2`, exit `3`, or fixed resource grant `4`; `ESI` carries capability reference `65536`; `EAX` carries the result or Windvale-selected resource identifier.

The accepted normal sequence is:

1. Init enters CPL3. Its WVA shim calls Windvale `Main`, which returns resource identifier `1`.
2. Init invokes syscall `4`. The kernel validates `WVRES001`, installs the client's RO/NX alias, publishes the client's private ABI-16 tables, records borrower `2`, and sets both counts to one.
3. Init invokes receive syscall `2`, records waiter `1`, and returns to the fixed coordinator after exactly two calls with its thread waiting.
4. The coordinator verifies the complete borrowed record and post-grant client state, then enters process `2`.
5. The interpreter calls `file.read_bytes("boot:main.wvb")`, validates the complete WVB profile, and interprets it to `29`.
6. The client sends `29` through its send-only endpoint and exits `29`.
7. The coordinator validates the client state, reactivates init, consumes the result, restores init's context, and resumes with `EAX = 29`.
8. Init exits `29`; both process records, the one grant/mapping, and exact send/receive/wake counts must be terminal and consistent.

The user-fault image interprets and sends `29`, then executes privileged `CLI` instead of exit. Vector 13/error 0 faults only process `2`; init still wakes and completes. Equivalent CPL0 faults remain terminal.

## Planner diagnostics

| Code | Meaning |
| --- | --- |
| `WVOS6001` | A role-specific extent is null, unaligned, incomplete, or outside low 1 GiB. |
| `WVOS6002` | The extent crosses a 2 MiB page-table region. |
| `WVOS6003` | The role-specific image is empty or exceeds its bounded RX extent. |
| `WVOS6004` | The role-module identity is not one SHA-256 digest. |
| `WVOS6005` | The extent overlaps the retained kernel executable window. |
| `WVOS6006` | Process/thread identity, role, reduced rights, or channel address is invalid. |
| `WVOS6007` | Runtime-input identity is missing, malformed, or inconsistent with the role. |
| `WVOS6008` | Runtime-resource presence, bounds, digest, or service-leaf range violates the role contract. |
| `WVOS6101` | The owner resource page or identity does not match the admitted resource. |
| `WVOS6102` | The client target page is already mapped or otherwise not a valid absent target. |
| `WVOS6103` | The target service leaf or publication range is outside the fixed client image/data bounds. |

## Deterministic candidate evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 4,610 | `fad470d9988c997daf4e44f90bbfe665391f5f02dd84ba8e8025580efc11c49f` |
| Process-policy WVO | 40,702 | `364b3ea7b4de30b17af93b5132812b7290c67255028482873a52d7a0c49cb960` |
| Init/resource-service WVB | 273 | `0fe423c499ce4f573095ddb9ff03355ee8b6ad927941f764ddaf2eaf9537f78b` |
| Init/resource-service WVO | 1,441 | `bccf48af1600cf3be8b93c8f132f227a064a324ac47b23d8ff9cdcf7f21d799a` |
| Init WVA shim WVO | 214 | `914327761fee08c69979c0da8a2ef513ac569bd39ab76597590fdf65a5df0511` |
| Linked init image | 1,385 | `ba2a2abe03d420506c79af61cc917f4b0124a2ad7687fa80117e353dde475727` |
| Interpreter WVB | 12,265 | `25a223346c6357290680476a39a4e67821e5efc9420933a90486f993aef46bf2` |
| Interpreter WVO | 128,340 | `5157b4446422d37597b16b5f29b5aae3f05920fc4718af1a9759efe29f4e73b7` |
| WVA resource stencil WVO | 314 | `1e690b8eebe6a21e4c4f6b697258c33c47370eb6b1277bdd40959cc077c29816` |
| Published resource service WVO | 314 | `610b861538697ca15c7f2b5fac5bc222be5697a2063509ffb7ab5b0e669a226d` |
| Linked normal interpreter image | 128,157 | `5a0acf3db339df5c3308f51a2e7ce182ee884d9b528db2998e9d0dcbf3b30655` |
| Linked fault interpreter image | 128,157 | `1a56e471c06702e479ec7c1cee49d98415734e7d5fca24f46fbc3c66c8175a83` |
| Normal process-machine WVO | 137,807 | `d863e61be67659b30b370da8ba9174b712f0d0bd8f02f31b9cdbb9fd523334c3` |
| Normal process-machine code | 6,941 | `ca0ac1c6110628b3c0cc1b582c905b2610222646b65f43a40e1a729b157828df` |
| Fault process-machine WVO | 137,839 | `c227055913f085d118996e05bde910e37fc5c4af1ef887c2bf91f029a4ca4dc4` |
| Fault process-machine code | 6,973 | `85a966450e3568db149984fb2f290596d8291ab1b572cccaae7cfdcc7edb94c3` |

The focused Windows suite passes 25 of 25 tests, and all four pinned-QEMU probe-27 scenarios pass. Cross-host qualification remains pending.

## Deliberate limits

Version 6 provides exactly one immutable resource borrow from fixed owner `1` to fixed borrower `2`. It does not provide names in init, multiple resources or recipients, capability-table transfer, ownership migration, revocation, teardown, reclamation, general shared memory, process creation, scheduling, arbitrary loading, complete semantic verification, executable publication, JIT, filesystems, packages, networking, Hyper-V, or physical-hardware evidence.
