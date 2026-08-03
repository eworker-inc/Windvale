# Protected Windvale processes and typed service resources

## Status and purpose

Protected-process contract version 17 remains the cross-host-qualified Probe-38 record contract owned by [Decision 0176](../Documents/Decisions/0176-Third-Protected-Service-And-Ready-Wait-Dispatcher.md). Implemented-candidate Probe 39, owned by [Decision 0188](../Documents/Decisions/0188-First-Hpet-Calibrated-Local-Apic-Preemption-Proof.md), retains every `WVPROC17` field and lifecycle while adding private thread-context/timer evidence and one bounded involuntary-preemption experiment before the existing workload.

Version 17 is cross-host qualified at exact implementation commit `aae6818e3226e9e7e88d205b4666fb9904e4735b` and GitHub [Verify run 30834243770](https://github.com/eworker-inc/Windvale/actions/runs/30834243770): all 87 Seed and 38 OS tests pass on Windows and Debian, and all five pinned Windows QEMU scenarios pass. Probe 39 has focused Windows and all five pinned-QEMU results; cross-host qualification remains pending. This is an internal experiment, not a stable syscall ABI, process manager, endpoint registry, supervisor, VFS, transferable-capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- `Process-Foundation.wv` binds the three process identities, two endpoint/channel lifecycles, exact boot-store and directory-snapshot identities, roles, ordered grants, both service exchanges, two client generations, page/profile/syscall budgets, ready/wait selection, the private four-tick/three-switch preemption policy, exact reuse, cleanup, result policy, and contained directory-provider failure.
- `Init-Resource-Service.wv` selects ordered boot resource identifiers `(1,2)` and serves only the measured `WVRS 1` lookup. Init no longer maps or parses `WVDS 1`.
- `Directory-Process-Service.wv` owns the measured `WVDQ 1` / `WVDR 1` directory policy. Its WVA seam reads the checked `WVDS 1` descriptor, receives requests through capability `65537`, and replies from a dedicated page.
- `Bytecode-Interpreter.wv` reads both granted runtime resources, validates runtime profile 7, charges the guest budget, and interprets the admitted program.
- `Boot-Resource-Service.wva` owns exact typed lookup for the two `WVBR002` entries used by the interpreter runtime.
- Each process WVA shim exports one exact 88-byte CPU-bound preemption probe with process-specific register sentinels. `X64-Timer-Shims.wva` owns HPET/APIC admission, IRQ entry, clock reads, one-shot rearm, `SWAPGS`, `IRETQ`, and stop.
- Stage 0 temporarily owns raw page-table writes, checked record serialization, x86-64 dispatch/context orchestration, immutable store/snapshot construction, and firmware packaging. Portable Windvale owns the matching ready/wait and bounded preemption policy model.

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

Probe 38's workload transitions remain cooperative. Probe 39 adds a separate private machine experiment: all three records are made runnable; directory begins in a CPU-bound WVA loop; timer interrupts switch directory to init, init to client, and client back to directory; a fourth interrupt ends the experiment. This proves bounded involuntary progress for the exact three roots without changing the retained dispatcher or `WVPROC17`.

Three 224-byte `WVTHR001` records at state offsets `0x8A0`, `0x980`, and `0xA60` contain identity, state, tick/dispatch/resume/preemption counts, and one normalized 176-byte x86-64 privilege-transition frame. `WVTIME01` at `0xB40` is 96 bytes and records HPET clocksource 2, local-APIC one-shot event 1, vector 32, private 5,000-microsecond quantum, four ticks, three switches, four EOIs, cursor, active process, one directory resume, measured nonzero APIC initial count, exact 10,000,000-femtosecond HPET period, 500,000 calibration ticks, monotonic last clock, and terminal state.

The interrupt boundary saves all fifteen GPRs, preserves uncontrolled live RFLAGS bits, validates controlled safety bits, copies the exact outgoing frame, activates the next root, rearms the local APIC, and resumes through `IRETQ`. `GS.base` and `IA32_KERNEL_GS_BASE` are kept distinct across CPL3/CPL0 transitions. The private records are implementation evidence, not a public scheduler or thread ABI. Delayed interrupts, wrap, idle, wake latency, priorities, dynamic queues, multiple threads, and SMP remain unproved.

## Grant, service calls, execution, cleanup, and reuse

1. Stage 0 maps the boot store only into init and the directory snapshot only into process 3, publishes checked descriptors, records both attached capabilities, and completes the bounded four-tick preemption experiment.
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

## Deterministic Probe-39 candidate artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 16,812 | `4904b44715399048e920d126d8a49f15a6b437cd4c77a25b23c3b113b9e7655d` |
| Process-policy WVO | 115,198 | `a4e218e5417e4ed605ddfbd7df2f92d9df7a9a154a41382f4e94f1ff9bc4c2ed` |
| Init/resource WVB | 525 | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` |
| Init WVA object | 2,118 | `6199fd8b46a384669ecbbf019e87bce0dd98728b0858be72669c276e6d5834e2` |
| Linked init image | 5,159 | `02122441f8cab2577588b09242a2781e93ec65bc6defed8676905e712473e0c5` |
| Directory-service WVB | 473 | `33b0e425bd6e2a1cd6ae8f95d4645748a6031b93684a9b1ac4d0e56e8408bef7` |
| Directory WVA object | 1,549 | `c0a7524130b8733ed17a3ce52fc04986cb449394c9ee509280120b86a3ed8c88` |
| Linked directory image | 3,911 | `f4d047c6f311b1561a5621b98f3db2868a969c54bb81dac2f75d599b7207f3fb` |
| Boot `WVRS 1` store | 1,195 | `e06cb88bc97c8a8c8413c476c41ec86eafb8d1ee3fab0daee8e3b50e788023b8` |
| Directory `WVDS 1` snapshot | 3,184 | `0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a` |
| Interpreter WVB | 56,165 | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` |
| Interpreter WVO | 447,652 | `0748200721cab7d5c3c6a43916fc623dfa0ee35e304fea6ad899877c9601c8e2` |
| Linked normal client | 448,141 | `28f3df14ea8260695b40a2a68728790c6ee2c1faff47be3f53d9a8187187263b` |
| Linked user-fault client | 448,109 | `09556fe33c03571d8b701854f2dfeb4c12c68de5a3339e2f19ec3a64d8dd91a8` |
| Linked service-fault client | 447,917 | `2ecb1d13e45073d93ed56d03e0f7c84633714a42332e6ed67c3fea7db9e53c97` |
| Timer WVA object | 1,202 | `e331a1db404b8b8359d35d410792496683a63acee621ff64f128a6eae128c344` |
| Normal process-machine WVO | 511,765 | `0b348d59ee8659b91bfa317031396292480fe8d26bce77c9712441cc2f43f43f` |
| User-fault process-machine WVO | 511,797 | `d7aa5d67407321482cbb75f5b9fa90e93106f1647f92d51373b718c587de41e0` |
| Service-fault process-machine WVO | 497,957 | `4747e4fd56d63ab9d7a945754781eeb535907b650f3ff402976ff95f6fc9d17a` |

## Deliberate limits

Version 17 retains one exact boot store, one exact directory snapshot, two fixed providers, one current client binding per endpoint, at most two client generations, two ordered grants, bounded service calls, a fixed three-record dispatcher, one exact LIFO reuse, and one private four-tick preemption experiment. It adds no names, lookup, registry, public endpoint creation, nested paths, enumeration, open handles, mounts, arbitrary providers, transfer/delegation, concurrent calls, cancellation, timeout/deadline API, priority, restart, replacement, general supervision, dynamic process creation, general scheduling, idle policy, multiple threads, SMP, block storage, mutation, persistence, packages, networking, Hyper-V, or physical-hardware evidence.
