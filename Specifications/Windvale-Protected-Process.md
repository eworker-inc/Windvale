# Protected Windvale processes and typed service resources

## Status and purpose

Protected-process contract version 16 is the cross-host-qualified Probe-37 contract owned by [Decision 0172](../Documents/Decisions/0172-First-Kernel-Owned-Service-Endpoint.md). It retains version 15's two-generation reclaim/rebuild, service-failure containment, and exact service exchanges while inserting one kernel-owned `WVENDP01` between process capability entries and `WVCHAN04`.

Version 16 is cross-host qualified at exact implementation commit `2a1461b6528c38a73be251a149d97be2854571a1` and GitHub [Verify run 30819690110](https://github.com/eworker-inc/Windvale/actions/runs/30819690110): all 87 Seed and 38 OS tests pass on Windows and digest-pinned Debian, and all five pinned Windows QEMU scenarios pass. This is an internal experiment, not a stable syscall ABI, process manager, endpoint registry, supervisor, VFS, transferable-capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- `Process-Foundation.wv` binds init, interpreter, program, budget, the exact boot-store and directory-snapshot identities, roles, ordered grants, both service exchanges, endpoint identity and lifecycle, two generations, page/profile/syscall budgets, exact reuse, cleanup, result policy, and the contained service-failure transition.
- `Init-Resource-Service.wv` selects ordered boot resource identifiers `(1,2)`. Its WVA seam serves both the measured `WVRS 1` lookup and the measured `WVDQ 1` / `WVDR 1` directory read.
- `Bytecode-Interpreter.wv` reads both granted runtime resources, validates runtime profile 7, charges the guest budget, and interprets the admitted program.
- `Boot-Resource-Service.wva` owns exact typed lookup for the two `WVBR002` entries used by the interpreter runtime.
- Stage 0 temporarily owns raw page-table writes, records, machine dispatch/coordination, immutable store/snapshot construction, and firmware packaging, with independent checked planners.

The kernel treats `WVRS 1` and `WVDS 1` as immutable byte extents with mapping and identity metadata. It does not parse names, snapshot entries, `WVRQ`, `WVRY`, `WVDQ`, or `WVDR`.

## Fixed identities, roles, and budgets

| Identity | SHA-256 | Authority |
| --- | --- | --- |
| Init/service WVB | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` | scalar receive, fixed set grant, service-request receive, and reply |
| Bytecode-interpreter WVB | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` | scalar send, service call, and two reads after grant |
| `boot:main.wvb` | `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761` | resource 1, WVB, immutable RO/NX |
| `boot:main.budget` | `add7f2a4843f8c512c0e2875546581db11b9ba227ee008b5f719dfacb125de76` | resource 2, four-byte LE value 199, immutable RO/NX |
| Boot `WVRS 1` store | `e06cb88bc97c8a8c8413c476c41ec86eafb8d1ee3fab0daee8e3b50e788023b8` | resource 4, 1,195 immutable RO/NX bytes attached only to init |
| Directory `WVDS 1` snapshot | `0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a` | resource 5, 3,184 immutable RO/NX bytes attached only to init |

The store contains the WVB and budget above plus resource 3, kind `opaque-bytes`, name `boot:main.configuration`, attributes `7`, and bytes `[3,5,8,13]`. The snapshot contains `folder` as `other` and `kernel.wv` as a 3,072-byte file where byte `i` is `i mod 251`.

Init is process/thread `1/1`, generation 1, process reference `65537`, runtime profile 2, instruction/call budgets `64/1`, nine user pages, and one handle. It uses eleven syscalls on the normal two-generation path and faults during its fourth syscall on the service-fault path. The client is process/thread `2/2`, generation 1 then 2, references `65538` then `131074`, runtime profile 7, native instruction/call budgets `189,114/5`, 755 physical frame cells, exact call-graph stack use 24,240 bytes, 118 pre-grant and 120 post-grant user pages, one handle, and four syscalls per normal generation. The contained client-fault and service-fault clients each use three syscalls: the former faults after sending its result, while the latter receives exact peer-loss status and exits cleanly. The separate guest execution budget remains `199` with maximum `256`.

Both retain result `6`, capability slot 0/generation 1, endpoint reference `65536`, channel capacity 1, and ABI 22/context 7/service-table 5. Process policy must return token `97` before machine state is published.

## Address spaces

Init receives 13 physical pages:

| Relative page | Purpose | User mapping |
| ---: | --- | --- |
| `0..3` | private paging hierarchy | none |
| `4..5` | linked init image | RX |
| `6` | stack | RW/NX |
| `7` | data/context, descriptors, request windows | RW/NX |
| `8` | admitted runtime WVB | RO/NX |
| `9` | execution budget | RO/NX |
| `10` | complete boot `WVRS 1` image | RO/NX |
| `11` | complete directory `WVDS 1` image | RO/NX |
| `12` | dedicated service response | RW/NX |

Each client generation receives this reclaimed 122-page physical extent plus two later aliases:

| Relative page | Purpose | Before grant | After grant |
| ---: | --- | --- | --- |
| `0..3` | private paging hierarchy | none | none |
| `4..113` | 110-page interpreter image | user RX | user RX |
| `114..119` | six-page stack | user RW/NX | user RW/NX |
| `120` | ABI-22 context/data | user RW/NX | user RW/NX |
| `121` | dedicated service response | user RW/NX | user RW/NX |
| `122` | module alias | absent | init WVB page, user RO/NX |
| `123` | budget alias | absent | init budget page, user RO/NX |

No placeholder backs an alias. Neither init-owned store is mapped into a client. Generation-1 cleanup clears both aliases and publications; the complete 122-page extent is zeroed and released. Generation 2 reconstructs every table, image, stack, data, response, context, and record byte at the same physical root with a different logical identity.

## `WVPROC16`, `WVENDP01`, `WVCHAN04`, and `WVRES006`

The state page stores two 272-byte little-endian process records at offsets `0x100` and `0x300`. Version 16 binds:

- magic/version `WVPROC16` and `16`;
- user-page budgets init `9`, client `120`;
- init allocation/code pages `13/2`, client allocation/code pages `122/110`;
- runtime profiles init `2`, client `7`;
- process generation init/first client `1`, rebuilt client `2`;
- exact canonical program digest at offset `0xD8`; and
- the kernel-only `WVENDP01` address at offset `0xC0`; and
- the page-aligned dedicated user service-response address at offset `0x108`.

Both context pages retain valid context-7 headers under ABI 22. Init data publishes the store descriptor at `0x180`, snapshot descriptor at `0x1A0`, and request windows beginning at `0x400`; its dedicated response page prevents a maximal 3,096-byte reply from overlapping live data. Each rebuilt client begins with runtime service/resource pointers zero and a dormant 1,024-byte compatibility record arena at data offset `0x200`, with used length zero.

`WVENDP01` is a 64-byte kernel-owned record at state offset `0x480`. It records open/closed state, reference `65536`, service kind, capacity one, provider reference `65537`, current client reference `65538` or `131074`, the retained channel address, resolution and close counts, provider status, and a zero reserved field. Every capability-bearing syscall validates the process entry and resolves this object against the caller's exact process generation before it can reach the channel or mutate a resource. Generation-1 completion requires eight resolutions before the client reference changes; normal completion closes at sixteen resolutions, while contained service failure closes at six.

`WVCHAN04` remains a 112-byte kernel-owned, capacity-one record at state offset `0x410`. Syscall numbers 5 through 7 retain their wire values as service-generic receive, call, and reply operations. They require nonempty extents no larger than 4,096 bytes, checked end arithmetic, RX sources, RW/NX destinations, exact endpoint roles, and directional rights. No user mapping exposes either record.

Four 128-byte `WVRES006` records track fixed identifiers/kinds, generation-stamped owner/borrower references, exact extents, immutable flags, SHA-256 identities, histories, and target PTEs. Resources 1 and 2 form the client grant set. Resource 4 attaches the 1,195-byte store only to init. Resource 5, kind `wvds-snapshot`, attaches the 3,184-byte snapshot only to init with descriptor generation 1. Neither attached resource participates in `WVBR002` or syscall 4.

Terminal peer cleanup clears message state, sender, receiver, waiter, byte length, and both destination/capacity pairs before recording whether the peer exited or faulted, its process reference, and the incremented close count. If a waiter was retained, closure also increments the wake count exactly once and resumes it with transport result `-1`. A checked reopen clears terminal peer evidence before normal generation 2. Generation 2 ends terminally closed; no request bytes or destination pointers survive either client lifetime.

## Grant, service calls, execution, cleanup, and reuse

1. Stage 0 verifies and maps the complete boot store and directory snapshot only into init, publishes checked descriptors, and records both attached capabilities.
2. Init returns resource-set token `131073`; syscall 4 validates both client-grant records, pages, digests, absent PTEs, service leaf, and token.
3. The kernel installs both RO/NX client aliases and publishes service table 5 plus `WVBR002` atomically.
4. Client generation 1 calls the resource service with the exact 55-byte `boot:main.configuration` request. Init dynamically selects the entry and returns the canonical 116-byte `WVRY 1` response through the dedicated pages.
5. Init re-registers its receive window. The client sends the exact 37-byte `WVDQ 1` request for `kernel.wv`, offset 0, maximum 3,072. Init validates the measured snapshot and constructs the exact 3,096-byte `WVDR 1` response.
6. The client validates the entire envelope and all 3,072 bytes, interprets the exact 815-byte program for 199 guest instructions, sends `6`, then exits or takes the contained fault.
7. Cleanup clears channel state, records terminal peer status, removes client aliases/publication, reloads init's CR3, and zeroes/releases the exact 122-page tail.
8. The same root is immediately reallocated and rebuilt as generation 2; the channel reopens cleanly and `WVENDP01` rebinds its client reference from `65538` to `131074` while retaining the provider and endpoint identity.
9. Generation 2 independently repeats grant, resource lookup, maximal directory read, interpretation, result, peer cleanup, and resource cleanup.
10. Init receives the second result and exits. `WVENDP01` closes exactly once with provider status exited after sixteen successful resolutions. The allocator ends exactly exhausted at cursor page 147; both init-owned immutable mappings remain outside the recycled suffix.

The contained user-fault scenario sends `6` then executes privileged `CLI`; cleanup records fault status and still completes. CPL0 invalid-opcode and general-protection scenarios remain terminal.

The contained service-fault scenario branches after generation 1's successful resource lookup. Its client sends 37 bytes whose `WVDQ 1` total-length field declares 36, then blocks in syscall 6. Init rejects the inconsistent request and executes privileged `CLI`, producing vector 13/error 0 at CPL3. The kernel accepts only the exact role, syscall, endpoint, channel, waiter, counter, and message shape; records init as the faulted peer; closes `WVENDP01` once at six resolutions with provider status faulted; clears every transient channel field; increments channel close and wake counts to one; and resumes the client with exact result `-1`. The client treats this as transport peer loss, exits after three syscalls with result `6`, and has both resource aliases revoked. This scenario deliberately stops before generation 2 and does not imply restart or replacement policy.

## Deterministic candidate artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 12,398 | `42676fb558683a5a1a1b30d7f74c15fc0396f0e384bed86db0a3d1f3fb4c0bda` |
| Process-policy WVO | 84,836 | `6d0e4e88d862438702da5b034fcaae1a9fcb9e7aac6044d6ebbd254c2f2c10f8` |
| Init WVB | 525 | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` |
| Init WVA object | 3,119 | `792314c634fb1c4d701080a1b9cb12037e21ea242971413a94f86e8569ce766d` |
| Linked init image | 6,119 | `8ba61e354de025d8b02dedcca22d901936d8aecbf5ff2df728648abc714e5f64` |
| Boot `WVRS 1` store | 1,195 | `e06cb88bc97c8a8c8413c476c41ec86eafb8d1ee3fab0daee8e3b50e788023b8` |
| Directory `WVDS 1` snapshot | 3,184 | `0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a` |
| Interpreter WVB | 56,165 | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` |
| Interpreter WVO | 447,652 | `0748200721cab7d5c3c6a43916fc623dfa0ee35e304fea6ad899877c9601c8e2` |
| Linked normal client | 448,045 | `369e7f22c8bfd48b033c38407be06ff181372a0891db1108e6b987ac14dd7e9b` |
| Linked fault client | 448,013 | `8153e1e389ee18068b17bc615d2211737160143565aeb852b137ef31fce5513b` |
| Service-fault client WVA object | 1,153 | `c5ed46d78cd8b8fba30b3425d23d1d8adcbe3638a7706f81945e2eeebb833214` |
| Linked service-fault client | 447,821 | `451364e3f4595bf9c44707da8dafae75ebc37c18860f94cebed743817f533bff` |
| Normal process-machine WVO | 493,286 | `5c3f291e8180e448cdd6a0e65fb187c6c1941477a43ab4cb986596a05504c4ed` |
| User-fault process-machine WVO | 493,334 | `9a08dfe59cff6a2bb88200ebf5a2bd48242806d97d34d3e88266615ef294feff` |
| Service-fault process-machine WVO | 481,598 | `609b46dccc32f44341f59c04f6362d7ee0111083ba5bdb83a01d7eaf79ca58cb` |

## Deliberate limits

Version 16 retains one exact boot store, one exact directory snapshot, one provider, one current client binding, at most two client generations, two ordered grants, bounded service calls, and one exact LIFO reuse on the normal path. It adds one internal endpoint object and generation-safe rebind, not names, lookup, a registry, public endpoint creation, nested paths, enumeration, open handles, mounts, arbitrary providers, transfer/delegation, concurrent calls, cancellation, timeout, restart, replacement, general supervision, general scheduling, block storage, mutation, persistence, packages, networking, Hyper-V, or physical-hardware evidence.
