# Protected Windvale processes and typed service resources

## Status and purpose

Protected-process contract version 17 is the locally implemented Probe-38 candidate owned by [Decision 0176](../Documents/Decisions/0176-Third-Protected-Service-And-Ready-Wait-Dispatcher.md). It retains version 16's two-generation reclaim/rebuild and endpoint lifecycle while splitting directory service into a third protected process, adding a second endpoint/channel, and selecting ready threads through one bounded state-driven dispatcher.

Version 16 remains cross-host qualified at exact commit `2a1461b6528c38a73be251a149d97be2854571a1` and GitHub [Verify run 30819690110](https://github.com/eworker-inc/Windvale/actions/runs/30819690110). Version 17 has all 38 focused OS tests and five pinned Windows QEMU scenarios passing locally; cross-host qualification is pending. This is an internal experiment, not a stable syscall ABI, process manager, endpoint registry, supervisor, VFS, transferable-capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- `Process-Foundation.wv` binds the three process identities, two endpoint/channel lifecycles, exact boot-store and directory-snapshot identities, roles, ordered grants, both service exchanges, two client generations, page/profile/syscall budgets, ready/wait selection, exact reuse, cleanup, result policy, and contained directory-provider failure.
- `Init-Resource-Service.wv` selects ordered boot resource identifiers `(1,2)` and serves only the measured `WVRS 1` lookup. Init no longer maps or parses `WVDS 1`.
- `Directory-Process-Service.wv` owns the measured `WVDQ 1` / `WVDR 1` directory policy. Its WVA seam reads the checked `WVDS 1` descriptor, receives requests through capability `65537`, and replies from a dedicated page.
- `Bytecode-Interpreter.wv` reads both granted runtime resources, validates runtime profile 7, charges the guest budget, and interprets the admitted program.
- `Boot-Resource-Service.wva` owns exact typed lookup for the two `WVBR002` entries used by the interpreter runtime.
- Stage 0 temporarily owns raw page-table writes, checked record serialization, x86-64 dispatch/orchestration, immutable store/snapshot construction, and firmware packaging. Portable Windvale owns the matching ready/wait policy model.

The kernel treats `WVRS 1` and `WVDS 1` as immutable byte extents with mapping and identity metadata. It does not parse names, snapshot entries, `WVRQ`, `WVRY`, `WVDQ`, or `WVDR`.

## Fixed identities, roles, and budgets

| Identity | SHA-256 | Authority |
| --- | --- | --- |
| Init/resource WVB | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` | scalar receive, fixed-set grant, resource-request receive, and reply through endpoint `65536` |
| Directory-service WVB | `33b0e425bd6e2a1cd6ae8f95d4645748a6031b93684a9b1ac4d0e56e8408bef7` | directory-request receive and reply through endpoint `65537` |
| Bytecode-interpreter WVB | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` | scalar send plus resource and directory service calls |
| `boot:main.wvb` | `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761` | resource 1, WVB, immutable RO/NX |
| `boot:main.budget` | `add7f2a4843f8c512c0e2875546581db11b9ba227ee008b5f719dfacb125de76` | resource 2, four-byte LE value 199, immutable RO/NX |
| Boot `WVRS 1` store | `e06cb88bc97c8a8c8413c476c41ec86eafb8d1ee3fab0daee8e3b50e788023b8` | resource 4, 1,195 immutable RO/NX bytes attached only to init |
| Directory `WVDS 1` snapshot | `0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a` | resource 5, 3,184 immutable RO/NX bytes attached only to process 3 |

The store contains the WVB and budget above plus resource 3, kind `opaque-bytes`, name `boot:main.configuration`, attributes `7`, and bytes `[3,5,8,13]`. The snapshot contains `folder` as `other` and `kernel.wv` as a 3,072-byte file where byte `i` is `i mod 251`.

Init is process/thread `1/1`, generation 1, reference `65537`, runtime profile 2, instruction/call budgets `64/1`, eight user pages, one handle, and nine syscalls on the normal two-generation path. The directory provider is process/thread `3/3`, generation 1, reference `65539`, runtime profile 4, instruction/call budgets `64/1`, six user pages, one handle, and five syscalls on the normal path. The service-fault provider faults during its first request after one receive syscall.

The client is process/thread `2/2`, generation 1 then 2, references `65538` then `131074`, runtime profile 7, native instruction/call budgets `189,114/5`, 755 physical frame cells, exact call-graph stack use 24,240 bytes, 118 pre-grant and 120 post-grant user pages, two handles, and four syscalls per normal generation. The contained client-fault and service-fault clients each use three syscalls. The separate guest execution budget remains `199` with maximum `256`.

All paths retain result `6`, channel capacity 1, and ABI 22/context 7/service-table 5. Process policy must return token `97` before machine state is published.

## Address spaces

Init receives 12 physical pages:

| Relative page | Purpose | User mapping |
| ---: | --- | --- |
| `0..3` | private paging hierarchy | none |
| `4..5` | linked init/resource image | RX |
| `6` | stack | RW/NX |
| `7` | data/context, store descriptor, request windows | RW/NX |
| `8` | admitted runtime WVB | RO/NX |
| `9` | execution budget | RO/NX |
| `10` | complete boot `WVRS 1` image | RO/NX |
| `11` | dedicated service response | RW/NX |

The directory provider receives ten physical pages:

| Relative page | Purpose | User mapping |
| ---: | --- | --- |
| `0..3` | private paging hierarchy | none |
| `4..5` | linked directory-service image | RX |
| `6` | stack | RW/NX |
| `7` | data/context and snapshot descriptor | RW/NX |
| `8` | complete directory `WVDS 1` image | RO/NX |
| `9` | dedicated service response | RW/NX |

Each client generation receives one reclaimed 122-page physical extent plus two later aliases:

| Relative page | Purpose | Before grant | After grant |
| ---: | --- | --- | --- |
| `0..3` | private paging hierarchy | none | none |
| `4..113` | 110-page interpreter image | user RX | user RX |
| `114..119` | six-page stack | user RW/NX | user RW/NX |
| `120` | ABI-22 context/data | user RW/NX | user RW/NX |
| `121` | dedicated service response | user RW/NX | user RW/NX |
| `122` | module alias | absent | init WVB page, user RO/NX |
| `123` | budget alias | absent | init budget page, user RO/NX |

No placeholder backs an alias. Neither provider-owned store is mapped into a client. Generation-1 cleanup clears both aliases and publications; the complete 122-page extent is zeroed and released. Generation 2 reconstructs every table, image, stack, data, response, context, and record byte at the same physical root with a different logical identity.

## `WVPROC17`, two `WVENDP01` records, two `WVCHAN04` records, and `WVRES006`

The state page stores three 288-byte little-endian process records at offsets `0x100`, `0x300`, and `0x6D0`. Version 17 binds:

- magic/version `WVPROC17` and `17`;
- user-page budgets init `8`, client `120`, directory `6`;
- allocation/code pages init `12/2`, client `122/110`, directory `10/2`;
- runtime profiles init `2`, client `7`, directory `4`;
- process generations init/directory/first client `1`, rebuilt client `2`;
- exact canonical program or provider-data digest at offset `0xD8`;
- primary endpoint address at offset `0xC0` and dedicated user response address at `0x108`; and
- optional second capability reference, rights, and endpoint address at `0x110`, `0x114`, and `0x118`. Only the client populates these fields.

Both provider context pages and each client context page retain valid context-7 headers under ABI 22. Init data publishes only the store descriptor; directory data publishes only the snapshot descriptor. Dedicated response pages prevent a maximal 3,096-byte reply from overlapping live data. Each rebuilt client begins with runtime service/resource pointers zero and a dormant 1,024-byte compatibility record arena at data offset `0x200`, with used length zero.

The resource channel/endpoint occupy state intervals `0x420..0x48F` and `0x490..0x4CF`. The directory channel/endpoint occupy `0x7F0..0x85F` and `0x860..0x89F`. Four 128-byte resources occupy `0x4D0..0x6CF`. The complete state layout is disjoint and checked before code emission. Each capability-bearing syscall validates the process entry, exact endpoint, provider/client generation, exact channel address, `WVCHAN04` magic/version/size/capacity, and directional rights before mutation.

Capability slot 0/generation 1 encodes resource reference `65536`. Capability slot 1/generation 1 encodes directory reference `65537`. The resource endpoint binds provider reference `65537`; the directory endpoint binds provider reference `65539`. Both bind current client reference `65538` or `131074`. Normal completion closes the resource endpoint at twelve resolutions and the directory endpoint at six. Contained directory-provider failure closes only its endpoint at two resolutions with provider status faulted.

Syscall numbers 5 through 7 retain their wire values as service-generic receive, call, and reply operations. They require nonempty extents no larger than 4,096 bytes, checked end arithmetic, RX sources, RW/NX destinations, exact endpoint roles, and directional rights. No user mapping exposes a kernel record.

Resources 1 and 2 form the client grant set. Resource 4 attaches the 1,195-byte store only to init. Resource 5, kind `wvds-snapshot`, attaches the 3,184-byte snapshot only to the directory provider with descriptor generation 1. Neither attached resource participates in `WVBR002` or syscall 4.

Terminal peer cleanup clears message state, sender, receiver, waiter, byte length, and both destination/capacity pairs before recording whether the peer exited or faulted, its process reference, and the incremented close count. If a waiter was retained, closure increments the wake count exactly once and resumes it with transport result `-1`. A checked reopen clears terminal client evidence before generation 2. No request bytes or destination pointers survive either client lifetime.

## Ready/wait dispatch

The reference and x86-64 dispatchers consume exactly three validated records in fixed slot order init, client, directory. The persistent cursor must be `0..2`. A record is runnable only when its process state is ready or running and its sole thread state is ready. A selected record yields its exact generation-stamped process reference and advances the cursor to the following slot modulo three.

The dispatcher rejects malformed record length, magic, version, byte size, process identity, or generation. It skips waiting, exited, and faulted threads and returns no selection when all three are non-runnable. Every initial process entry and every exact wake goes through the dispatcher. The coordinator still validates which selected process is permitted by the fixed scenario before activating its root.

This is cooperative transition scheduling, not preemption. There is no timer, quantum, priority, starvation promise, run-queue allocation, multiple threads, SMP, or public scheduler ABI.

## Grant, service calls, execution, cleanup, and reuse

1. Stage 0 maps the boot store only into init and the directory snapshot only into process 3, publishes checked descriptors, and records both attached capabilities.
2. The dispatcher starts process 3 to register its request page, then starts init. Init returns resource-set token `131073`; syscall 4 validates both grant records, pages, digests, absent PTEs, service leaf, and token.
3. The kernel installs both RO/NX client aliases and publishes service table 5 plus `WVBR002` atomically.
4. Client generation 1 calls the resource endpoint with the exact 55-byte `boot:main.configuration` request. Init dynamically selects the entry and returns the canonical 116-byte `WVRY 1` response.
5. The client calls the independent directory endpoint with the exact 37-byte request for `kernel.wv`, offset 0, maximum 3,072. Process 3 validates its snapshot and returns the exact 3,096-byte `WVDR 1` response.
6. The client validates the complete reply and all 3,072 bytes, interprets the exact 815-byte program for 199 guest instructions, sends `6`, then exits or takes the contained fault.
7. Cleanup clears both channels' transient client state, records terminal status, removes client aliases/publication, reloads init's CR3, and zeroes/releases the exact 122-page tail.
8. The same root is immediately reallocated and rebuilt as generation 2; both endpoints rebind their client reference from `65538` to `131074` while retaining provider and endpoint identity.
9. Generation 2 independently repeats grant, both service calls, interpretation, result, peer cleanup, and resource cleanup.
10. Process 3 exits after its final reply and closes its endpoint at six resolutions. Init receives the second result and exits, closing the resource endpoint at twelve. The allocator ends exhausted at cursor page 156; both provider extents remain outside the recycled suffix.

The contained user-fault scenario sends `6` then executes privileged `CLI`; cleanup records fault status and still completes. CPL0 invalid-opcode and general-protection scenarios remain terminal.

The contained service-fault scenario branches after generation 1's successful resource lookup. The client sends 37 bytes whose `WVDQ 1` total-length field declares 36 and blocks in syscall 6. Process 3 rejects the inconsistent request and executes privileged `CLI`, producing vector 13/error 0 at CPL3. The kernel accepts only the exact role, syscall, endpoint, channel, waiter, counter, and message shape; records the directory provider as faulted; closes only its endpoint/channel at two resolutions; clears every transient directory-channel field; wakes the client once with result `-1`; and leaves init alive. The client exits after three syscalls with result `6` and loses both resource aliases. No generation 2, restart, replacement, or supervision is claimed.

## Deterministic candidate artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 16,023 | `319a7fb7f3ea08ff3c7c4aba8b37ee90106f5360f62abcc529fd51286bee34ad` |
| Process-policy WVO | 109,340 | `860e893dab8b170a9a9d49cdcda2d8997e351a3e6e13b03b7d92f1ad38f7cf74` |
| Init/resource WVB | 525 | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` |
| Init WVA object | 1,977 | `f95d011602e92c210c769ed13f6bfa9b012e537223a8e333a2dfcbfd0a23f385` |
| Linked init image | 5,063 | `01ee9ddd99f4c75be3f8848324b9c95e6ab76aa9204e2ad5466d5009bcc25994` |
| Directory-service WVB | 473 | `33b0e425bd6e2a1cd6ae8f95d4645748a6031b93684a9b1ac4d0e56e8408bef7` |
| Directory WVA object | 1,408 | `db4e0b8d54148e4ff654c22bf337e3fe027d3143c035bf11ae31096f88ba42b0` |
| Linked directory image | 3,831 | `bf25040b4925a13c4a919ffd5a53de8ff281e4452132a9f7cd9bb3624740c883` |
| Boot `WVRS 1` store | 1,195 | `e06cb88bc97c8a8c8413c476c41ec86eafb8d1ee3fab0daee8e3b50e788023b8` |
| Directory `WVDS 1` snapshot | 3,184 | `0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a` |
| Interpreter WVB | 56,165 | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` |
| Interpreter WVO | 447,652 | `0748200721cab7d5c3c6a43916fc623dfa0ee35e304fea6ad899877c9601c8e2` |
| Linked normal client | 448,045 | `4bb73a9a46a318a1fe5068a4d17e35db134f00c68a81566937a9b6ffa275e3b2` |
| Linked user-fault client | 448,013 | `2963161bd954f400d41bc3771f9a91efc3653435ec25f9d0b7fa3ec7bfcca958` |
| Linked service-fault client | 447,821 | `6a697e08be953be9d5868068a9bd2b329f180b6cd7c139c34cb9f9f688bccb8b` |
| Normal process-machine WVO | 502,697 | `6435782bc20b63b187e31a28634022d8f910ed92f49889ecfe1cb6e829de7dd2` |
| User-fault process-machine WVO | 502,729 | `5e95c321a862f52e2fbbc29c96b485441dcbf32f8253bd539197209c749d1979` |
| Service-fault process-machine WVO | 488,889 | `cea85b20933fd989b02ccaec5e77db88a0ad981e4bc9ca58226e42e905a13af1` |

## Deliberate limits

Version 17 retains one exact boot store, one exact directory snapshot, two fixed providers, one current client binding per endpoint, at most two client generations, two ordered grants, bounded service calls, a fixed three-record dispatcher, and one exact LIFO reuse on the normal path. It adds no names, lookup, registry, public endpoint creation, nested paths, enumeration, open handles, mounts, arbitrary providers, transfer/delegation, concurrent calls, cancellation, timeout, timer, preemption, priority, restart, replacement, general supervision, dynamic process creation, general scheduling, block storage, mutation, persistence, packages, networking, Hyper-V, or physical-hardware evidence.
