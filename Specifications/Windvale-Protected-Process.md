# Protected Windvale processes and typed service resources

## Status and purpose

Protected-process contract version 17 remains unchanged. Cross-host-qualified Probe 39, owned by [Decision 0188](../Documents/Decisions/0188-First-Hpet-Calibrated-Local-Apic-Preemption-Proof.md), supplies the retained timer evidence. Cross-host-qualified Probe 40, owned by [Decision 0196](../Documents/Decisions/0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md), changes process allocation order and reclamation mechanics without changing any `WVPROC17` field or syscall behavior.

Probe 39 is cross-host qualified at exact implementation commit `6a250c86c30e8921d6bf9244a27d0fd763716cb0` and GitHub [Verify run 30847279400](https://github.com/eworker-inc/Windvale/actions/runs/30847279400). Probe 40 is cross-host qualified at exact implementation commit `c4008e75db061df375eb323d75a818863aee553f` and GitHub [Verify run 30853255559](https://github.com/eworker-inc/Windvale/actions/runs/30853255559): Windows and digest-pinned Debian pass all 87 Seed tests, all 39 OS tests, and the native CLI gate; all five pinned Windows QEMU scenarios pass. This is an internal experiment, not a stable syscall ABI, process manager, endpoint registry, supervisor, VFS, transferable-capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- `Process-Foundation.wv` binds the three process identities, two endpoint/channel lifecycles, exact boot-store and directory-snapshot identities, roles, ordered grants, both service exchanges, two client generations, page/profile/syscall budgets, ready/wait selection, the private four-tick/three-switch preemption policy, and the exact `WVMEMO01` non-tail release/reuse invariant.
- `Init-Resource-Service.wv` selects ordered boot resource identifiers `(1,2)` and serves only the measured `WVRS 1` lookup. Init no longer maps or parses `WVDS 1`.
- `Directory-Process-Service.wv` owns the measured `WVDQ 1` / `WVDR 1` directory policy. Its WVA seam reads the checked `WVDS 1` descriptor, receives requests through capability `65537`, and replies from a dedicated page.
- `Bytecode-Interpreter.wv` reads both granted runtime resources, validates runtime profile 7, charges the guest budget, and interprets the admitted program.
- `Boot-Resource-Service.wva` owns exact typed lookup for the two `WVBR002` entries used by the interpreter runtime.
- Each process WVA shim exports one exact 88-byte CPU-bound preemption probe with process-specific register sentinels. `X64-Timer-Shims.wva` owns HPET/APIC admission, IRQ entry, clock reads, one-shot rearm, `SWAPGS`, `IRETQ`, and stop.
- `X64-Memory-Object-Shims.wva` owns bounded first-fit allocation, complete bitmap/owner preflight, generation-safe release, page-vector publication, and zeroing for process memory objects.
- Current `main` constructs the fixed IDT/paging, checked process records, x86-64 dispatch/context objects, immutable store/snapshot, linked image, and firmware through pinned native Windvale/WVA owners. The immutable Stage 0 recovery release preserves the former emitter and differential provenance; it is not an ordinary build dependency. Portable Windvale owns the matching ready/wait, bounded preemption, and memory-object lifecycle policy models.

The kernel treats `WVRS 1` and `WVDS 1` as immutable byte extents with mapping and identity metadata. It does not parse names, snapshot entries, `WVRQ`, `WVRY`, `WVDQ`, or `WVDR`.

## Fixed identities, roles, and budgets

| Identity | SHA-256 | Authority |
| --- | --- | --- |
| Init/resource WVB | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` | scalar receive, fixed-set grant, resource-request receive, and reply through endpoint `65536` |
| Directory-service WVB | `33b0e425bd6e2a1cd6ae8f95d4645748a6031b93684a9b1ac4d0e56e8408bef7` | directory-request receive and reply through endpoint `65537` |
| Bytecode-interpreter WVB | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` | scalar send plus resource and directory service calls |
| `boot:main.wvb` | `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936` | resource 1, WVB 1.11, immutable RO/NX |
| `boot:main.budget` | `add7f2a4843f8c512c0e2875546581db11b9ba227ee008b5f719dfacb125de76` | resource 2, four-byte LE value 199, immutable RO/NX |
| Boot `WVRS 1` store | `624ece2d2e032f6f0929675a8f79ceb223538d84bccace264ecbbfdce5eca4ad` | resource 4, 1,196 immutable RO/NX bytes attached only to init |
| Directory `WVDS 1` snapshot | `0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a` | resource 5, 3,184 immutable RO/NX bytes attached only to process 3 |

The store contains the WVB and budget above plus resource 3, kind `opaque-bytes`, name `boot:main.configuration`, attributes `7`, and bytes `[3,5,8,13]`. The snapshot contains `folder` as `other` and `kernel.wv` as a 3,072-byte file where byte `i` is `i mod 251`.

Init is process/thread `1/1`, generation 1, reference `65537`, runtime profile 2, instruction/call budgets `64/1`, eight user pages, one handle, and nine syscalls on the normal two-generation path. The directory provider is process/thread `3/3`, generation 1, reference `65539`, runtime profile 4, instruction/call budgets `64/1`, six user pages, one handle, and five syscalls on the normal path. The service-fault provider faults during its first request after one receive syscall.

The client is process/thread `2/2`, generation 1 then 2, references `65538` then `131074`, runtime profile 7, native instruction/call budgets `189,137/5`, 755 physical frame cells, exact call-graph stack use 24,240 bytes, 118 pre-grant and 120 post-grant user pages, two handles, and four syscalls per normal generation. The contained client-fault and service-fault clients each use three syscalls. The separate guest execution budget remains `199` with maximum `256`.

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

Each client generation receives the same reclaimed 122-page physical extent plus two later aliases. Generation 1 occupies arena pages `25..146`; the directory object remains live at `147..156`; generation 2 first-fits `25..146` again.

| Relative page | Purpose | Before grant | After grant |
| ---: | --- | --- | --- |
| `0..3` | private paging hierarchy | none | none |
| `4..113` | 110-page interpreter image | user RX | user RX |
| `114..119` | six-page stack | user RW/NX | user RW/NX |
| `120` | ABI-22 context/data | user RW/NX | user RW/NX |
| `121` | dedicated service response | user RW/NX | user RW/NX |
| `122` | module alias | absent | init WVB page, user RO/NX |
| `123` | budget alias | absent | init budget page, user RO/NX |

No placeholder backs an alias. Neither provider-owned store is mapped into a client. Generation-1 cleanup clears both aliases and publications; `WVMEMO01` validates and releases the complete 122-page vector while the later directory object remains active. Every released page is zeroed before generation 2 reconstructs each table, image, stack, data, response, context, and process-record byte at the same physical root with a different logical identity.

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

1. The checked process machine maps the boot store only into init and the directory snapshot only into process 3, publishes checked descriptors, records both attached capabilities, and completes the bounded four-tick preemption experiment.
2. The dispatcher starts process 3 to register its request page, then starts init. Init returns resource-set token `131073`; syscall 4 validates both grant records, pages, digests, absent PTEs, service leaf, and token.
3. The kernel installs both RO/NX client aliases and publishes service table 5 plus `WVBR002` atomically.
4. Client generation 1 calls the resource endpoint with the exact 55-byte `boot:main.configuration` request. Init dynamically selects the entry and returns the canonical 116-byte `WVRY 1` response.
5. The client calls the independent directory endpoint with the exact 37-byte request for `kernel.wv`, offset 0, maximum 3,072. Process 3 validates its snapshot and returns the exact 3,096-byte `WVDR 1` response.
6. The client validates the complete reply and all 3,072 bytes, interprets the exact 816-byte program for 199 guest instructions, sends `6`, then exits or takes the contained fault.
7. Cleanup clears both channels' transient client state, records terminal status, removes client aliases/publication, reloads init's CR3, and zeroes/releases the exact 122-page client object while the later directory object remains live.
8. The same root is immediately reallocated and rebuilt as generation 2; both endpoints rebind their client reference from `65538` to `131074` while retaining provider and endpoint identity.
9. Generation 2 independently repeats grant, both service calls, interpretation, result, peer cleanup, and resource cleanup.
10. Process 3 exits after its final reply and closes its endpoint at six resolutions. Init receives the second result and exits, closing the resource endpoint at twelve. The bitmap ends fully allocated, free pages are zero, and the retained fixed-bootstrap cursor remains page 13.

The contained user-fault scenario sends `6` then executes privileged `CLI`; cleanup records fault status and still completes. CPL0 invalid-opcode and general-protection scenarios remain terminal.

The contained service-fault scenario branches after generation 1's successful resource lookup. The client sends 37 bytes whose `WVDQ 1` total-length field declares 36 and blocks in syscall 6. Process 3 rejects the inconsistent request and executes privileged `CLI`, producing vector 13/error 0 at CPL3. The kernel accepts only the exact role, syscall, endpoint, channel, waiter, counter, and message shape; records the directory provider as faulted; closes only its endpoint/channel at two resolutions; clears every transient directory-channel field; wakes the client once with result `-1`; and leaves init alive. The client exits after three syscalls with result `6` and loses both resource aliases. No generation 2, restart, replacement, or supervision is claimed.

## Historically qualified Probe-40 artifacts

The table records the exact artifacts qualified at implementation commit `c4008e75db061df375eb323d75a818863aee553f`. Current native `main` reconstructs the normal process object as a 512,978-byte WVO with SHA-256 `dff07c3f6a52dedf6bcd96181221cba50c831359502ec763ee77f6aaaaafdfaa`; the current builder does not reconstruct the two contained-fault process objects.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 18,763 | `907d89aae0575d05306d4111c87f52f5a684085576a19d6425968ebe83afa3f4` |
| Process-policy WVO | 129,310 | `483ba9c752862fa739dea5fb9c40ce747e3210797d39bc73ac3f8d22084f669a` |
| Init/resource WVB | 525 | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` |
| Init WVA object | 2,118 | `6199fd8b46a384669ecbbf019e87bce0dd98728b0858be72669c276e6d5834e2` |
| Linked init image | 5,159 | `02122441f8cab2577588b09242a2781e93ec65bc6defed8676905e712473e0c5` |
| Directory-service WVB | 473 | `33b0e425bd6e2a1cd6ae8f95d4645748a6031b93684a9b1ac4d0e56e8408bef7` |
| Directory WVA object | 1,549 | `c0a7524130b8733ed17a3ce52fc04986cb449394c9ee509280120b86a3ed8c88` |
| Linked directory image | 3,911 | `f4d047c6f311b1561a5621b98f3db2868a969c54bb81dac2f75d599b7207f3fb` |
| Boot `WVRS 1` store | 1,196 | `624ece2d2e032f6f0929675a8f79ceb223538d84bccace264ecbbfdce5eca4ad` |
| Directory `WVDS 1` snapshot | 3,184 | `0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a` |
| Interpreter WVB | 56,165 | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` |
| Interpreter WVO | 447,652 | `0748200721cab7d5c3c6a43916fc623dfa0ee35e304fea6ad899877c9601c8e2` |
| Linked normal client | 448,141 | `28f3df14ea8260695b40a2a68728790c6ee2c1faff47be3f53d9a8187187263b` |
| Linked user-fault client | 448,109 | `09556fe33c03571d8b701854f2dfeb4c12c68de5a3339e2f19ec3a64d8dd91a8` |
| Linked service-fault client | 447,917 | `2ecb1d13e45073d93ed56d03e0f7c84633714a42332e6ed67c3fea7db9e53c97` |
| Timer WVA object | 1,202 | `e331a1db404b8b8359d35d410792496683a63acee621ff64f128a6eae128c344` |
| Memory-object WVA object | 2,538 | `fe0a94461b743be58319d2e2f8b737840ec1216e61a98ee7e210f96f97f85bee` |
| Normal process-machine WVO | 511,856 | `052e0a86ae59e753a986cfa675b013222208b82f4501236e5f0822d6db2dab0a` |
| User-fault process-machine WVO | 511,904 | `b38aa657b004ee1725078889004f4a654e4cc7d81b5930215f3908f56103b388` |
| Service-fault process-machine WVO | 498,032 | `545b45c2956a706e8f81cf60ce1c64b37f4ce15ee32221f16a7ff27345b18a81` |

## Deliberate limits

Version 17 retains one exact boot store, one exact directory snapshot, two fixed providers, one current client binding per endpoint, at most two client generations, two ordered grants, bounded service calls, a fixed three-record dispatcher, three fixed memory objects, and one private four-tick preemption experiment. It adds no object registry, scatter allocation, names, discovery, public endpoint creation, nested paths, enumeration, open handles, mounts, arbitrary providers, transfer/delegation, concurrent calls, cancellation, timeout/deadline API, priority, restart, replacement, general supervision, dynamic process creation, general scheduling, idle policy, multiple threads, SMP, block storage, mutation, persistence, packages, networking, Hyper-V, or physical-hardware evidence.
