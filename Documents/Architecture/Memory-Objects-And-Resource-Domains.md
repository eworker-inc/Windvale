# Windvale memory-object and resource-domain architecture

## Status

Recommended successor architecture under proposed [Decision 0198](../Decisions/0198-Next-Integrated-Architecture-Defaults.md). It refines the accepted process and allocator direction in [Decision 0173](../Decisions/0173-Windvale-Process-Service-And-Driver-Architecture.md) and [Decision 0181](../Decisions/0181-Next-Windvale-Os-Mechanism-Contracts.md) from cross-host-qualified [Decision 0196](../Decisions/0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md). Probe 40 implements a fixed bitmap, owner map, three generation-safe memory objects, non-tail release, zeroing, and same-root reuse. It does not implement a general object inventory, noncontiguous page-set allocation, virtual mapping API, resource domain, overcommit, swap, copy-on-write, or public memory ABI.

## Recommendation

Windvale should introduce memory in three deliberately separate layers:

1. the physical-page allocator tracks machine pages and their kernel owner;
2. generation-safe memory objects own committed page backing and maximum rights; and
3. address-space mappings place a rights-reduced view of an object into one process.

A flat resource domain accounts aggregate use and owns teardown policy above individual processes. It is not a memory allocator and it grants no capabilities.

This split prevents a physical address, a virtual mapping, an accounting charge, and a transferable memory reference from becoming one overloaded object.

## Physical-page ownership

Probe 40 establishes the first bounded form: a fixed 160-bit bitmap, one owner byte per admitted arena page, three `WVMEMO01` records, and an exact page vector per object. A successor should retain that deterministic evidence while generalizing only from a measured consumer. Use one allocation bitmap over admitted 4 KiB pages plus a compact checked page-record table when owner bytes can no longer explain every required state. The bitmap answers whether a page is available; the record explains why it is not.

The logical page record carries:

- page-frame index, never a raw pointer in an external contract;
- state such as reserved, free, kernel, page table, memory object, DMA, guest, or quarantined;
- owner object identity and generation where applicable;
- zeroed-before-publication evidence;
- pin or DMA state where applicable; and
- bounded diagnostic provenance for invariant failures.

Exact bit widths and packing remain private until measured. One ambiguous multi-purpose bit field should not encode allocation, ownership, pinning, and type together.

Probe 40 first-fits one contiguous object while retaining an explicit page vector. The successor policy should scan the bitmap from a deterministic cursor and, when a real object requires it, select the required number of free pages in ascending page-index order without requiring physical contiguity. Release clears the checked page records and bitmap positions in page-index order. This is intentionally simpler than a buddy allocator and avoids failing ordinary allocations merely because free pages are separated. A DMA, huge-page, firmware, or device consumer that requires a physically contiguous run receives a separate allocation contract and reserved pool or measured allocator revision.

Every newly assigned user-visible page is zeroed after its previous ownership ends and before any new mapping becomes visible. Zeroing is revalidated across non-LIFO reuse, not inferred from the boot allocator's initially clean arena. Reserved firmware, kernel image, page-table, MMIO, and unusable ranges can never enter the free bitmap.

## Memory objects

Probe 40 already proves a fixed-length, page-aligned, anonymously backed, committed, generation-safe private memory object. The durable successor retains that semantic shape while replacing the fixed three-record inventory and private layout. It has:

- object identity and generation;
- page count and committed-byte count;
- owning resource-domain identity;
- maximum permitted mapping rights;
- backing class and lifecycle state;
- current mapping and pin counts; and
- the exact page-frame set owned by the object.

The recommended general lifecycle is `Constructing`, `Alive`, `Revoking`, then `Dead`; Probe 40's smaller private lifecycle remains evidence rather than being reinterpreted as this public shape. Construction reserves the domain charge and physical pages, zeroes the pages, completes the record, and only then publishes an object reference. Any failure rolls back every reservation and exposes no partial object. Destruction first prevents new mappings, revokes or closes existing mappings, completes required address-space invalidation, clears ownership, zeroes according to the reuse policy, releases pages and charges, increments the generation, and only then permits identity reuse.

The first object is not resizable, pageable, sparse, demand-zero, file-backed, copy-on-write, deduplicated, compressed, or overcommitted. Later backing classes may include shared anonymous memory, executable staging, file cache, device/DMA memory, and guest memory, but each adds its own rights and teardown contract rather than flags that weaken the anonymous-object invariant.

## Mappings and executable memory

A mapping is an address-space-owned view containing object identity and generation, object offset, virtual range, rights, and mapping generation. Mapping rights must be a subset of both the memory object's maximum rights and the process's grant. A mapping never conveys ownership of the object by itself.

The first dynamic address-space allocator uses explicit admitted regions and deterministic placement; it does not expose a stable native virtual address to application semantics. Checked arithmetic covers every object offset, page count, virtual range, alignment, and end address.

Writable-or-executable discipline remains mandatory:

- ordinary anonymous memory is read/write and non-executable;
- verified executable publication writes into a non-executable staging object;
- sealing validates identity, relocations, and bounds before creating an executable read-only view; and
- no ordinary operation turns a writable live mapping executable in place.

One-CPU invalidation is sufficient for the first slice. SMP page-table shootdown, PCID reuse, huge pages, memory-mapped files, and user fault handling remain later measured contracts.

## Flat resource domains

Every process belongs to exactly one initial resource domain. The domain record is generation-safe and contains limits, current reservations, current committed use, peak use, terminal reason, and member identities for at least:

- processes and threads;
- committed and mapped memory;
- handles, capabilities, endpoints, and queued messages;
- CPU accounting and execution budgets;
- pinned, DMA, shared, and later guest memory; and
- bounded output, diagnostics, and teardown work where their providers require aggregate control.

Limits are ceilings, not preallocated authority. A resource domain is not a capability bundle: a process can have available budget without having permission to create a memory object, launch a child, attach a device, or open a stream.

Resource acquisition follows one invariant: reserve the complete charge, construct and validate the object, publish it, then convert the reservation to committed use. Release occurs in reverse. A rejected acquisition leaves usage unchanged. A provider must not publish an object and charge it afterward.

The kernel reserves recovery capacity outside ordinary domains. No application, service, driver, or VM can consume the pages, endpoint slots, interrupt work, or diagnostic space needed to cancel operations and tear itself down.

## Domain stop and teardown

The first domain hierarchy is flat. A stop operation proceeds in a bounded order:

1. atomically mark the domain stopping and reject new members, mappings, capabilities, and provider work;
2. request cancellation and wake all interruptible waits;
3. stop new device, DMA, network, or VM submissions owned by the domain;
4. terminate remaining threads and processes after the graceful deadline;
5. close endpoints and capability references and fail external waiters explicitly;
6. revoke mappings, pins, DMA, and executable publications;
7. release memory objects and physical pages; and
8. publish one structured terminal record with leaked-resource counts required to be zero for qualification.

Domain stop is idempotent. A repeated stop observes the same terminal generation and cannot start teardown twice. Parent process exit does not silently destroy an unrelated domain; ownership and supervision policy are explicit.

## First measured slices

1. Retain cross-host-qualified Probe 40 as the exact baseline. Do not widen its fixed bitmap, owner bytes, page vectors, or three records without a measured consumer.
2. Add one flat resource domain around the three static processes and their existing memory objects. Prove admission rejection before exposure, exact peak/current accounting, repeated-stop idempotence, and zero live charges after teardown.
3. Use the first dynamic clean-spawn process to justify a generation-safe object inventory beyond three records. Retain contiguous first-fit when it satisfies the measured object; add deterministic noncontiguous page-set selection only when that or another workload requires it.
4. Separate address-space mappings from object backing as dynamic launch and executable publication require them, retaining W^X and complete rollback.
5. Add shared memory, pinned/DMA memory, executable publication, and guest memory only as separate backing-class revisions with independent revocation evidence.

The trigger for replacing the bitmap scan should be empirical: representative process, stream, filesystem, network, and VM workloads must show unacceptable scan cost, metadata cost, or a justified contiguous-allocation need. Ordinary page-set allocation should not fail while enough admissible free pages remain. Do not add allocator complexity solely because a more sophisticated strategy is conventional.

## Deliberately open details

The architecture does not freeze Probe 40's bitmap size, owner-byte encoding, object packing, or page-vector bound as the successor ABI. It also does not yet freeze a general page-record layout, page-table layout, virtual addresses, allocator scan limits, domain numeric ceilings, object-table indices, syscall encoding, shared-memory layout, or an SMP invalidation protocol. Those values require a measured memory inventory and malformed-state tests. They do not reopen the layer split, atomic reserve/construct/publish rule, zero-before-reuse requirement, generation safety, W^X discipline, flat first domain, or reserved recovery capacity.
