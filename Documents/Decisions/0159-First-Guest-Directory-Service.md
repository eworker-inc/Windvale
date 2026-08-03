# Decision 0159: First guest directory service

- Status: Implemented candidate with local Windows and pinned-QEMU evidence; cross-host qualification pending
- Date: 2026-08-03
- Owners: Windvale OS init service, protected-process service transport, and immutable directory capability
- Contracts: [`WVDQ 1` / `WVDR 1`](../../Specifications/Windvale-Directory-Service-Ipc.md), [`WVDS 1`](../../Specifications/Windvale-Directory-Snapshot.md), [`WVPROC14`](../../Specifications/Windvale-Protected-Process.md), and [`WVKMEM14`](../../Specifications/Windvale-Kernel-Memory.md)
- Advances: [Decision 0155](0155-First-Immutable-Windvale-Directory-Snapshot.md) and [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Retains: ABI 22/context 7, admission 4/bridge 2, retained bridge 10, paging 4, `WVCHAN03`, canonical WVB 1.6/1.7, `WVRS 1`, and application-visible `filesystem.directory_read_v1` semantics

## Context

Decisions 0153 through 0155 establish a typed immutable directory read, its checked `WVDQ 1` / `WVDR 1` service protocol, and a one-page `WVDS 1` provider. None proves that an isolated Windvale guest process can use the capability. Probe 34's shared data-page reply windows are too small for the protocol's 3,096-byte maximum reply, and its resource-named syscall vocabulary would make a second service type accidental rather than architectural.

The smallest useful adoption slice must therefore map the exact immutable snapshot only into init, give both service and client a complete RW/NX response page, reuse the existing format-blind 4 KiB copied-message state machine, and execute the request twice across the existing release/rebuild lifecycle. It must not create a VFS, path namespace, block layer, or new compiler merely to claim filesystem progress.

## Decision

- Advance firmware to Probe 35, the composed x86-64 WVA seam to 11, memory to `WVKMEM14`, processes to `WVPROC14`, and resource records to `WVRES006`. Retain ABI 22, `WVCHAN03`, paging 4, and all portable language semantics.
- Expand the deterministic arena from 144 to 147 pages. Init grows from 11 to 13 pages: one RO/NX page for the exact `WVDS 1` snapshot and one RW/NX service-response page. Each client grows from 121 to 122 physical pages by adding one RW/NX response page before the two later resource aliases.
- Add one attached resource record for directory resource 5, kind 4. It binds the snapshot address, exact 3,184-byte length, complete SHA-256, immutable RO/NX flags, init ownership, and generation 1. The kernel treats the bytes as opaque and does not parse names or `WVDR`.
- Extend the process record to 272 bytes. Offset `0x108` stores the complete page-aligned user response address so one dispatcher can validate resource and directory replies without embedding a shared-data-page convention.
- Rename syscalls 5 through 7 and their rights/wait reasons from resource-specific to service-generic names without changing their numeric values or the format-blind capacity-one transport contract.
- Require init to grant the existing boot resources, serve the exact `WVRQ 1` lookup, re-register its receive window, validate the exact 37-byte `WVDQ 1` request, validate the measured `WVDS 1` metadata, and construct the exact 3,096-byte `WVDR 1` success in its dedicated response page.
- Require each client generation to complete the existing resource call, then call the directory service for `kernel.wv`, validate the complete reply, and check every returned file byte against `i mod 251` before running the retained interpreter and returning `6`.
- Preserve cleanup and reuse: generation 1 exits or faults terminally, channel state is cleared, the 122-page client tail is zeroed and released, the same root is rebuilt as generation 2, and the directory request succeeds again without remapping the snapshot into either client.
- Require portable `Process-Foundation.wv` to bind the exact store and snapshot identities, init/client page budgets `9/120`, runtime profiles `2/7`, both ordered service exchanges in each generation, and syscall budgets `11/4` before machine state is published.
- Keep machine construction, page-table publication, syscall dispatch, copying, checked service-byte mechanics, and PE/UEFI packaging as explicit Stage 0/WVA replacement seams. Their longer-term owner remains Windvale `.wv` plus narrow WVA machine leaves once the system target can express the required checked memory/state operations.
- Do not change the compiler for this slice. The existing ABI-22 backend and WVA instruction surface are sufficient; unrelated compiler evolution can proceed independently.

## Exact candidate

The canonical directory snapshot remains 3,184 bytes with SHA-256 `0f793a41a701240b9cf41179dafa252384b43cd23214646ff021d245657c235a`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 8,001 | `73f67a7c8294b7a2d3e2633fab482fa8eabe53a14dc8883821dacd7812b822aa` |
| Process-policy WVO | 53,456 | `edf5d9a767a46b91577b739f6dbcf6c57a963c91c18cab5b8364746b3451dd44` |
| Init WVA object | 3,119 | `64214b7b3ce90365f4ee9962ba1fbdb416f14ce4316b8b309106b8523a80c917` |
| Linked init image | 6,119 | `d8285cf68d0df45afe9d78f4dc65de427ed9e58b6d24c962f3b4dc9cb7bd9f18` |
| Normal client WVA object | 1,369 | `8ea2869d5e2a54c2a3392acb59cabee7bbf639bb0dd228fad08618ad47b1fd73` |
| Fault client WVA object | 1,338 | `0b0fc5212f8b301da7ea4c59469300039332b023678dc0b6990c2c548fc1a23d` |
| Linked normal client | 448,045 | `369e7f22c8bfd48b033c38407be06ff181372a0891db1108e6b987ac14dd7e9b` |
| Linked fault client | 448,013 | `8153e1e389ee18068b17bc615d2211737160143565aeb852b137ef31fce5513b` |
| Normal process-machine WVO | 490,972 | `cbeb8d22c1237d8456c3e68cfb8434a9b48d1ec861e66ce98aca11486fb9c0f0` |
| Fault process-machine WVO | 491,004 | `04e25f89c1946b02a29af0c738dc5ad74ba042d9106a9d8e76fb60c15738737b` |

The deterministic firmware candidates are:

| Scenario | EFI bytes | SHA-256 | Host code |
| --- | ---: | --- | ---: |
| Normal | 582,144 | `a1157d2f367cee2755120264621c9d9f5d5f410ade3d54fd27bca5a50ded9b9f` | 0 |
| Invalid opcode | 582,144 | `c1ada91a1928e380166ba87b4d454415e8c0256218f4b98cad3feee57d674af3` | 3 |
| General protection | 582,144 | `bbc472e29d38ff327dbb90f63ae994731fd07c97aa09bc8b3f36fd179744a946` | 3 |
| Contained user fault | 582,656 | `3ad6ab38405d78ee8af73758a203633a00c2a5e8e4d07cd6a964b7a24f446c16` | 0 |

## Evidence

A zero-warning Release build and all 37 focused OS tests pass locally on Windows. The suite covers exact object and firmware reproduction, the policy WVB's exact store/snapshot identities, separate response mappings, snapshot mapping and padding, the attached directory resource, process-record response addresses, malformed snapshot rejection, exact WVA object shapes and syscall counts, generation-safe cleanup, and hostile lower-level `WVDS`/`WVDQ`/`WVDR` cases.

All four pinned QEMU/OVMF scenarios pass. Normal and contained-fault paths emit `directory-service=pass` and `ipc=resource-and-directory`; both independently rebuilt clients complete the maximal 3,096-byte reply and validate all 3,072 file bytes. The two CPL0 fault scenarios retain their exact terminal markers and host code 3. Cross-host build/test evidence and GitHub qualification remain pending, so this decision does not yet claim a qualified Windows/Linux checkpoint.

## Consequences

Windvale now has an end-to-end guest instance of its first filesystem-shaped capability: application semantics, checked IPC, immutable provider bytes, isolated mappings, a format-blind kernel transport, and repeatable lifecycle evidence. The kernel still knows no paths, filenames, directory layout, or filesystem response format.

The next architecture work should separate the directory endpoint from the boot-resource endpoint when an independently lived second service or concurrent caller makes that distinction measurable, and should move the remaining checked service-byte and dispatch policy from WVA into `.wv` as the system target gains the required bounded memory/state operations. A later storage track can add enumeration or a block-backed provider behind new capability contracts; it should not reinterpret this immutable read contract.

This decision does not implement nested paths, enumeration, open handles, mutation, persistence, a filesystem root, mounts, cache coherence, concurrent calls, cancellation, transferable capabilities, service discovery, block I/O, drivers, DMA, SMP, Hyper-V, physical-hardware qualification, or .NET retirement.

## Reconsider when

- Multiple services or simultaneous calls require distinct endpoints, correlation, queues, cancellation, or fairness.
- A reply cannot fit one checked 4 KiB copied message.
- Snapshot content must span independently verified immutable extents.
- The system target can own the dispatcher/service policy without weakening the current independent machine checks.
- A mutable or block-backed provider requires coherence and lifetime rules absent from `WVDS 1`.
