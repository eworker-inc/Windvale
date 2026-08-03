# Protected Windvale processes and typed resource sets

## Status and purpose

Protected-process contract version 13 is the current Probe-34 implemented candidate. It retains version 12's two-generation reclaim/rebuild and bounded request/reply, then gives init an independently lived immutable `WVRS 1` mapping, dynamic guest lookup, and explicit peer-death cleanup. [Decision 0142](../Documents/Decisions/0142-Immutable-Guest-Resource-Store.md) owns version 13. Version 11 remains the latest cross-host-qualified process contract under [Decision 0133](../Documents/Decisions/0133-Frame-Owned-Direct-Native-Records.md).

This is an internal experiment, not a stable syscall ABI, general process manager, filesystem namespace, transferable capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- `Process-Foundation.wv` binds init, interpreter, program, budget, roles, ordered grants, two generations, exact reuse, cleanup, and result policy.
- `Init-Resource-Service.wv` selects ordered identifiers `(1,2)`; its WVA seam dynamically validates and searches the exact boot `WVRS 1` profile, builds a response, and repeats before exit.
- `Bytecode-Interpreter.wv` reads both granted names, validates runtime profile 6, charges the guest budget, and interprets the program.
- `Boot-Resource-Service.wva` owns exact typed lookup for the two `WVBR002` entries used by the interpreter runtime.
- Stage 0 temporarily owns raw page-table writes, records, publication, dispatch, coordination, immutable-store construction, and firmware packaging, with independent checked planners.

The kernel treats the init store as bytes with mapping and identity metadata. It does not parse `WVRS 1`, names, requests, or replies.

## Fixed identities, roles, and budgets

| Identity | SHA-256 | Authority |
| --- | --- | --- |
| Init/resource-service WVB | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` | scalar receive, fixed set grant, resource-request receive, and reply |
| Bytecode-interpreter WVB | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` | scalar send, resource-service call, and two reads after grant |
| `boot:main.wvb` | `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761` | resource 1, WVB, immutable RO/NX |
| `boot:main.budget` | `add7f2a4843f8c512c0e2875546581db11b9ba227ee008b5f719dfacb125de76` | resource 2, four-byte LE value 199, immutable RO/NX |
| Boot `WVRS 1` store | `e06cb88bc97c8a8c8413c476c41ec86eafb8d1ee3fab0daee8e3b50e788023b8` | resource 4, 1,195 immutable RO/NX bytes attached only to init |

The store contains the WVB and budget above plus resource 3, kind `opaque-bytes`, name `boot:main.configuration`, attributes `7`, and bytes `[3,5,8,13]`.

Init is process/thread `1/1`, generation 1, process reference `65537`, runtime profile 1, instruction/call budgets `64/1`, seven user pages, one handle, and seven syscalls.

The client is process/thread `2/2`, generation 1 then 2, references `65538` then `131074`, runtime profile 6, native instruction/call budgets `189,114/5`, 755 physical frame cells, exact call-graph stack use 24,240 bytes, 116 pre-grant and 118 post-grant user pages, one handle, and three syscalls per generation. The separate guest execution budget is `199` with maximum `256`.

Both retain result `6`, capability slot 0/generation 1, channel capacity 1, and ABI 21/context 7/service-table 5. Process policy must return token `97` before machine state is published.

## Address spaces

Init receives 11 physical pages:

| Relative page | Purpose | User mapping |
| ---: | --- | --- |
| `0..3` | private paging hierarchy | none |
| `4..5` | linked init image | RX |
| `6` | stack | RW/NX |
| `7` | data/context, store descriptor, request and reply windows | RW/NX |
| `8` | admitted runtime WVB | RO/NX |
| `9` | execution budget | RO/NX |
| `10` | complete immutable boot `WVRS 1` image | RO/NX |

Each client generation receives this reclaimed 120-page physical extent plus two later aliases:

| Relative page | Purpose | Before grant | After grant |
| ---: | --- | --- | --- |
| `0..3` | private paging hierarchy | none | none |
| `4..112` | 109-page interpreter image | user RX | user RX |
| `113..118` | six-page stack | user RW/NX | user RW/NX |
| `119` | ABI-21 context/data and reply window | user RW/NX | user RW/NX |
| `120` | module alias | absent | init WVB page, user RO/NX |
| `121` | budget alias | absent | init budget page, user RO/NX |

No placeholder backs an alias. The init store has no client PTE. Generation-1 cleanup clears both client aliases and publications; the complete 120-page extent is zeroed and released. Generation 2 reconstructs every table, image, stack, data, context, and record byte at the same physical root with a different logical identity.

## `WVPROC13` and `WVCHAN03` records

The state page stores two 264-byte little-endian process records at offsets `0x100` and `0x300`. Version 13 preserves version 12's field offsets while binding the new measured values:

- magic/version `WVPROC13` and `13`;
- user-page budgets init `7`, client `118`;
- init allocation/code pages `11/2`, client allocation/code pages `120/109`;
- client native instruction budget `189,114`, call depth `5`, and six stack pages;
- runtime profiles init `1`, client `6`;
- process generation init/first client `1`, rebuilt client `2`;
- exact canonical program digest at offset `0xD8`.

Both context pages retain valid context-7 headers under ABI 21. Init's data page publishes the store descriptor at offset `0x180`, a 1,056-byte request window at `0x400`, and a 2,016-byte response window at `0x820`. Each rebuilt client begins with runtime service/resource pointers zero and a dormant 1,024-byte compatibility record arena at data offset `0x200`, with used length zero. Grant publishes service table 5 at data offset `0x80`, `WVBR002` at `0x100`, and both runtime resource pointers atomically while preserving the arena fields. ABI 21's frame-owned direct records leave arena use at zero; cleanup and generation-2 reconstruction preserve that value.

`WVCHAN03` is a 112-byte kernel-owned record at state offset `0x410`. It retains request/reply counters, byte length, and service/client destinations and capacities. Offsets `0x60`, `0x64`, `0x68`, and `0x6C` add peer status, peer process, close count, and reserved zero. Syscalls 5 through 7 require nonempty extents no larger than 4,096 bytes, checked end arithmetic, RX sources, RW/NX destinations, exact endpoint roles, and directional rights. No user mapping exposes the record.

Terminal peer cleanup clears message state, sender, receiver, waiter, byte length, and both destination/capacity pairs before recording whether the client exited or faulted, its process reference, and the incremented close count. A checked reopen clears terminal peer evidence before generation 2. Generation 2 ends terminally closed; no request bytes or destination pointers survive either client lifetime.

## Native stack preflight

The builder decodes the verified interpreter WVO before process construction. Starting at the client entry export, it computes the maximum reachable stack use from generated frame sizes and exact call edges, adding return addresses and the entry shim's saved `r15`. A recursive edge is rejected for this bounded profile. The exact maximum is 24,240 bytes. Six pages provide 24,576 bytes and are the minimal whole-page envelope; five pages provide only 20,480 bytes.

## Typed resources and publication

Three 128-byte `WVRES005` records track fixed identifiers/kinds, generation-stamped owner/borrower references, source and target addresses, exact lengths, immutable RO/NX flags, SHA-256 identity, historical grant count, live mapping count, and exact target PTE.

Resources 1 and 2 retain lengths 815 and 4 and form the client grant set. Resource 4 is the 1,195-byte `WVRS 1` store: state `attached`, owner and borrower both init reference `65537`, source and target the init store page, one historical attachment, one live mapping, and an exact present/user/RO/NX PTE. It is not part of `WVBR002`, not transferred by syscall 4, and never mapped into either client.

`WVBR002` remains exactly 80 bytes: a 16-byte header and two ordered 32-byte entries. Entry zero is `(1, wvb-module)` and entry one is `(2, u32-execution-budget)`. Unknown, duplicate, reversed, or partial tokens fail before mutation. The WVA leaf accepts only `boot:main.wvb` and `boot:main.budget` and validates the selected typed entry before returning a descriptor.

Before machine construction, Stage 0 verifies the exact store and complete-image SHA-256. The init WVA seam then validates checked extents, exact canonical three-entry layout, metadata, strict order, and digest text before scanning directory names and copying the selected digest and value into a response. This measured guest seam does not recompute per-entry payload SHA-256; the immutable page and complete-image digest are the machine boundary for this exact profile.

## Grant, execution, cleanup, and reuse

1. Stage 0 validates and maps the complete boot store only into init, publishes its descriptor, and records the attached `WVRES005` capability.
2. Init returns resource-set token `131073`; syscall 4 validates both client-grant records, pages, digests, absent PTEs, service leaf, and token.
3. The kernel installs both RO/NX client aliases and publishes service table 5 plus `WVBR002` atomically.
4. Client generation 1 calls the resource service with the exact 55-byte `boot:main.configuration` request. The kernel copies it into init's registered window and blocks the client.
5. Init validates the request, dynamically searches its mapped store, and constructs the canonical 116-byte `WVRY 1` response for resource 3. The kernel copies it into the client's upper data-page window.
6. The client validates the complete response, interprets the exact 815-byte program for 199 guest instructions, sends `6`, then exits or takes the contained fault.
7. Cleanup clears channel message and destination state, records terminal peer status, validates generation 1, clears both aliases and runtime publication, and preserves grant count 1.
8. Init's CR3 reload retires non-global translations. The exact 120-page tail is zeroed, released, immediately reallocated at the same root, and the channel is cleanly reopened for generation 2.
9. Init receives `6`, grants again, and blocks. The second grant records borrower `131074` and grant count 2.
10. Generation 2 independently repeats dynamic request/reply, validation, interpretation, result send, peer cleanup, and grant cleanup.
11. Init receives the second result and exits. The allocator ends exactly exhausted at cursor page 143; the init store remains one immutable mapping.

The contained user-fault scenario sends `6` then executes privileged `CLI`; cleanup records fault status and still completes. CPL0 invalid-opcode and general-protection scenarios remain terminal.

## Deterministic candidate artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 6,973 | `04c91ebca24d72ba13ab3b8c6d3d0fb4a1ad0be807de58584caa4df5005ab956` |
| Process-policy WVO | 47,496 | `6ec4ca02eb59959ebcfc245fee07dd0a8d1c26e5e2f6d9de1566c37ea65c5f9b` |
| Init WVB | 525 | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` |
| Init WVA object | 1,929 | `93d212d61723e5d43d30a4fbe3319b5b448cb7f52e147cd02f95a69cb722b53f` |
| Linked init image | 5,015 | `0e4afe4990bb6c4dfe1f255ec51594a58a6aaa1ef857d9ea48d44eb5e58e9a5e` |
| Boot `WVRS 1` store | 1,195 | `e06cb88bc97c8a8c8413c476c41ec86eafb8d1ee3fab0daee8e3b50e788023b8` |
| Interpreter WVB | 56,165 | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` |
| Interpreter WVO | 445,684 | `3840f10bacf8b7b498f28646b947a53841baf00241cd21bc94423ab5a43e8e31` |
| Linked normal client | 445,789 | `c3046836c9048f8aef2765337a2831a34dd8014489afcbcc1aceddd1ce019578` |
| Linked fault client | 445,773 | `dd880728016b305c002cd6270e18168de613c513eb444943c1429a30e037a19e` |
| Normal process-machine WVO | 480,666 | `7c445a204aa906b0411f1fbd15f7df5aea4feae2c36a76cf914e39f6b59645fe` |
| Fault process-machine WVO | 480,714 | `64577556b02f59e03a3645292402a40d57821313a618e39d60f8d8b92aa513a8` |

The current candidate passes all 31 bounded OS tests and all four Windows pinned-QEMU scenarios. Fresh Debian and complete dual-host qualification are pending; no Debian QEMU execution is claimed.

## Deliberate limits

Version 13 retains one exact three-entry boot store, one owner, one logical borrower, two generations, two ordered grants, one request per generation, and one exact LIFO reuse. It adds no path components, directories, enumeration, handles, mounts, arbitrary stores, payload-digest recomputation in the guest, transfer/delegation, non-tail release, concurrent root reuse, general scheduling, block storage, mutation, persistence, packages, networking, Hyper-V, or physical-hardware evidence.
