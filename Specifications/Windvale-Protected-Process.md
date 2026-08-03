# Protected Windvale processes and typed resource sets

## Status and purpose

Protected-process contract version 12 is the current Probe-33 implemented candidate. It retains version 11's two-generation reclaim/rebuild cycle and adds one synchronous checked resource request/reply in each client generation. [Decision 0135](../Documents/Decisions/0135-Bounded-Guest-Resource-Request-Reply.md) owns version 12. Version 11 remains the latest cross-host-qualified process contract under [Decision 0133](../Documents/Decisions/0133-Frame-Owned-Direct-Native-Records.md).

This is an internal experiment, not a stable syscall ABI, general process manager, dynamic namespace, transferable capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- `Process-Foundation.wv` binds init, interpreter, program, budget, roles, ordered grants, two generations, exact reuse, cleanup, and result policy.
- `Init-Resource-Service.wv` selects ordered identifiers `(1,2)`; its WVA seam grants, receives and validates the fixed configuration request, replies, and repeats before exit.
- `Bytecode-Interpreter.wv` reads both exact names, validates runtime profile 6, charges the guest budget, and interprets the program.
- `Boot-Resource-Service.wva` owns exact typed lookup for the two `WVBR002` entries.
- Stage 0 temporarily owns raw page-table writes, records, publication, dispatch, coordination, and firmware packaging, with independent checked planners.

## Fixed identities, roles, and budgets

| Identity | SHA-256 | Authority |
| --- | --- | --- |
| Init/resource-service WVB | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` | scalar receive, fixed set grant, resource-request receive, and reply |
| Bytecode-interpreter WVB | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` | scalar send, resource-service call, and two reads after grant |
| `boot:main.wvb` | `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761` | resource 1, WVB, immutable RO/NX |
| `boot:main.budget` | `add7f2a4843f8c512c0e2875546581db11b9ba227ee008b5f719dfacb125de76` | resource 2, four-byte LE value 199, immutable RO/NX |

Init is process/thread `1/1`, generation 1, process reference `65537`, runtime profile 1, instruction/call budgets `64/1`, five user pages, one handle, and seven syscalls.

The client is process/thread `2/2`, generation 1 then 2, references `65538` then `131074`, runtime profile 6, native instruction/call budgets `189,114/5`, 755 physical frame cells, exact call-graph stack use 24,240 bytes, 116 pre-grant and 118 post-grant user pages, one handle, and three syscalls per generation. The separate guest execution budget is `199` with maximum `256`.

Both retain result `6`, capability slot 0/generation 1, channel capacity 1, and ABI 21/context 7/service-table 5. Process policy must return token `97` before machine state is published.

## Address spaces

Init receives nine physical pages: four table pages, one RX image page, one RW/NX stack page, one RW/NX data/context page, and two owned RO/NX resource pages.

Each client generation receives this reclaimed 120-page physical extent plus two later aliases:

| Relative page | Purpose | Before grant | After grant |
| ---: | --- | --- | --- |
| `0..3` | private paging hierarchy | none | none |
| `4..112` | 109-page interpreter image | user RX | user RX |
| `113..118` | six-page stack | user RW/NX | user RW/NX |
| `119` | ABI-21 context/data and reply window | user RW/NX | user RW/NX |
| `120` | module alias | absent | init WVB page, user RO/NX |
| `121` | budget alias | absent | init budget page, user RO/NX |

No placeholder backs an alias. Generation-1 cleanup clears both aliases and publications; the complete 120-page extent is zeroed and released. Generation 2 reconstructs every table, image, stack, data, context, and record byte at the same physical root with a different logical identity.

## `WVPROC12` and `WVCHAN02` records

The state page stores two 264-byte little-endian process records at offsets `0x100` and `0x300`. Version 12 preserves version 11's field offsets while binding the new measured values:

- magic/version `WVPROC12` and `12`;
- user-page budgets init `5`, client `118`;
- client native instruction budget `189,114`, call depth `5`, and 109 RX code pages;
- runtime profiles init `1`, client `6`;
- process generation init/first client `1`, rebuilt client `2`;
- exact canonical program digest at offset `0xD8`.

Both context pages retain valid context-7 headers under ABI 21. Init's record-arena fields remain zero. Each rebuilt client begins with runtime service/resource pointers zero and a dormant 1,024-byte compatibility record arena at data offset `0x200`, with used length zero. Grant publishes service table 5 at data offset `0x80`, `WVBR002` at `0x100`, and both resource pointers atomically while preserving the arena fields. ABI 21's frame-owned direct records leave arena use at zero; cleanup and generation-2 reconstruction preserve that value.

`WVCHAN02` is a 96-byte kernel-owned record at state offset `0x410`. It retains the scalar state and counters, adds request/reply counters and byte length, and stores one service destination/capacity plus one client destination/capacity. Syscalls 5 through 7 require nonempty extents no larger than 4,096 bytes, checked end arithmetic, RX sources, RW/NX destinations, exact endpoint roles, and directional rights. No user mapping exposes the record.

## Native stack preflight

The builder decodes the verified interpreter WVO before process construction. Starting at the client entry export, it computes the maximum reachable stack use from generated frame sizes and exact call edges, adding return addresses and the entry shim's saved `r15`. A recursive edge is rejected for this bounded profile. The exact maximum is 24,240 bytes. Six pages provide 24,576 bytes and are the minimal whole-page envelope; five pages provide only 20,480 bytes.

## Typed resources and publication

The two retained 128-byte `WVRES004` records track fixed identifiers/kinds, generation-stamped owner/borrower references, source and target addresses, exact lengths, immutable RO/NX flags, SHA-256 identity, historical grant count, live mapping count, and exact target PTE. Resource 1 length is 815; resource 2 remains four bytes.

`WVBR002` remains exactly 80 bytes: a 16-byte header and two ordered 32-byte entries. Entry zero is `(1, wvb-module)` and entry one is `(2, u32-execution-budget)`. Unknown, duplicate, reversed, or partial tokens fail before mutation. The WVA leaf accepts only `boot:main.wvb` and `boot:main.budget` and validates the selected typed entry before returning a descriptor.

## Grant, execution, cleanup, and reuse

1. Init returns resource-set token `131073`; syscall 4 validates both owned records, pages, digests, absent PTEs, service leaf, and token.
2. The kernel installs both RO/NX aliases and publishes service table 5 plus `WVBR002` atomically.
3. Client generation 1 calls the resource service with the exact 55-byte `boot:main.configuration` request. The kernel copies it into init's registered window and blocks the client.
4. Init validates the complete request and replies with the canonical 116-byte `WVRY 1` envelope for bytes `[3,5,8,13]`; the kernel copies it into the client's upper 2 KiB data window.
5. The client validates the complete reply, interprets the exact 815-byte program for 199 guest instructions, sends `6`, then exits or takes the contained fault.
6. Cleanup validates generation 1, clears aliases and publication, and preserves grant count 1.
7. Init's CR3 reload retires non-global translations. The exact 120-page tail is zeroed, released, immediately reallocated at the same root, and rebuilt as generation 2.
8. Init receives `6`, grants again, and blocks. The second grant records borrower `131074` and grant count 2.
9. Generation 2 independently repeats request/reply, reply validation, interpretation, result send, and cleanup.
10. Init receives the second result and exits. The allocator again ends exactly exhausted at cursor page 141.

The contained user-fault scenario sends `6` then executes privileged `CLI`; cleanup still completes. CPL0 invalid-opcode and general-protection scenarios remain terminal.

## Deterministic candidate artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 6,973 | `04c91ebca24d72ba13ab3b8c6d3d0fb4a1ad0be807de58584caa4df5005ab956` |
| Process-policy WVO | 47,496 | `6ec4ca02eb59959ebcfc245fee07dd0a8d1c26e5e2f6d9de1566c37ea65c5f9b` |
| Init WVB | 525 | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` |
| Interpreter WVB | 56,165 | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` |
| Interpreter WVO | 445,684 | `3840f10bacf8b7b498f28646b947a53841baf00241cd21bc94423ab5a43e8e31` |
| Linked normal client | 445,789 | `c3046836c9048f8aef2765337a2831a34dd8014489afcbcc1aceddd1ce019578` |
| Linked fault client | 445,773 | `dd880728016b305c002cd6270e18168de613c513eb444943c1429a30e037a19e` |
| Normal process-machine WVO | 476,552 | `ceede3bacb80a9888eb1b21314d7736d2f628c9b7c60a0c1f3bfd753d3bd7069` |
| Fault process-machine WVO | 476,616 | `27d8a1b9587048b136484f3fb91f309e77746c943e4b58ab0f59e43bd5193fb8` |

The current candidate passes all 31 bounded OS tests and all four Windows pinned-QEMU scenarios. Fresh Debian and complete dual-host qualification are pending; no Debian QEMU execution is claimed.

## Deliberate limits

Version 12 retains two boot-resource names, one fixed configuration request, one owner, one logical borrower, two generations, two ordered grants, and one exact LIFO reuse. It adds no dynamic guest store, enumeration, arbitrary resources, transfer/delegation, independent lifetimes, non-tail release, concurrent root reuse, SMP shootdown, general process creation, scheduling, arbitrary loading, executable publication, JIT, filesystem paths, packages, networking, Hyper-V, or physical-hardware evidence.
