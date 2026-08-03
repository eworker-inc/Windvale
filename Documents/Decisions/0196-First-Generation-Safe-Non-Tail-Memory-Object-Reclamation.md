# Decision 0196: First generation-safe non-tail memory-object reclamation

- Date: 2026-08-03
- Status: Cross-host qualified
- Advances: [Decision 0181](0181-Next-Windvale-Os-Mechanism-Contracts.md)
- Retains: qualified Probe 39, `WVPROC17`, paging 5, ABI 22/context 7, and the private timer evidence
- Contract: [Windvale kernel memory](../../Specifications/Windvale-Kernel-Memory.md)

## Context

Qualified Probe 39 can run three protected roots and preempt them, but process reclamation still depends on the client being the allocator tail. That makes lifetime order part of an accidental physical-layout contract. Decision 0181 already selects a bounded bitmap, explicit page ownership, generation-safe memory objects, and zero-before-reuse as the next mechanism.

The smallest useful pressure is not a general allocator. It is one real non-tail transition: allocate the client before the directory provider, keep directory live above it, retire client generation 1, and prove generation 2 safely reuses only the client's pages.

## Decision

Firmware Probe 40 advances kernel memory to `WVKMEM17` without changing paging 5, `WVPROC17`, or ABI 22:

- Retain the 157-page arena. Fixed Stage 0 allocation still owns only the IDT and seven paging pages and leaves its raw cursor at page 13.
- Add a 160-bit allocation bitmap and one owner byte for each of the 157 real pages. Bits outside the arena remain reserved. Owner `255` denotes fixed kernel pages; owner `1..254` denotes the low object identifier; zero denotes free.
- Add three private 304-byte `WVMEMO01` records for init, client, and directory. A record binds generation-stamped reference, owner, lifecycle state, allocation/release history, base, page count, and an exact `u16` page vector.
- Assemble first-fit allocate and exact release mechanics from `X64-Memory-Object-Shims.wva`. Both paths validate complete retained state before mutation. Allocation preflights bitmap and owner evidence, zeroes pages, and publishes the record. Release preflights the full vector, bitmap, owner map, base, generation, and free bound, then clears ownership and zeroes pages.
- Allocate init at pages `13..24`, client generation 1 at `25..146`, and directory at `147..156`. Release client while directory remains active and byte-exact. First-fit generation 2 must return pages `25..146` again.
- Portable `Process-Foundation.wv` owns the matching object identities, page ordering, lifecycle states, generation change, counts, free-page transitions, and live-directory invariant. WVA owns bounded x86-64 mechanics. C# remains the Stage 0 builder, reference oracle, and recovery path.
- Emit `memory-object-reuse=pass` only after the complete normal or contained-client-fault process path returns successfully. The contained service-fault path retains no generation-2 claim.

The page vector is explicit now so future objects can be backed by noncontiguous physical pages, but Probe 40 deliberately continues to allocate one contiguous run and does not publish this private state-page layout as a stable ABI.

## Consequences

- Client lifetime is no longer coupled to being the highest physical allocation. The later directory object remains live across release and reuse.
- Stale or wrong-generation release fails without bitmap mutation. Released bytes are zero before another generation can observe them.
- Memory-object ownership moves materially into Windvale: portable `.wv` owns the policy invariant and `.wva` owns the privileged machine leaf. Stage 0 no longer emits process-tail release mechanics.
- Raw cursor and first-fit object state are deliberately distinct. The raw cursor remains a fixed-bootstrap evidence field; aggregate free pages and bitmap/owner state govern objects.
- The exact WVA object is relocation-free and independently verified. Windows x64 focused tests execute it directly through the controlled native publication lifetime; the complete firmware path supplies architecture-real QEMU evidence.
- This is not a general heap, buddy allocator, slab allocator, virtual-memory manager, memory capability API, dynamic process loader, or SMP allocator.

## Qualification evidence

- The focused OS suite contains 39 tests, including strict object-codec rejection, wrong-generation no-mutation, full zeroing, live-directory preservation, same-root reuse, deterministic machine artifacts, and direct Windows-x64 execution of the assembled WVA leaf.
- The exact memory-object WVA is 2,538 bytes with SHA-256 `fe0a94461b743be58319d2e2f8b737840ec1216e61a98ee7e210f96f97f85bee`; its 2,374-byte text contains the 1,389-byte allocator and 985-byte releaser.
- Probe-40 deterministic EFI identities are recorded in [Windvale-Os-Boot-Probe.md](../../Specifications/Windvale-Os-Boot-Probe.md).
- All five local pinned Windows QEMU scenarios pass with exact Probe-40 transcripts and deterministic EFI identities.
- Exact implementation commit `c4008e75db061df375eb323d75a818863aee553f` passes GitHub [Verify run 30853255559](https://github.com/eworker-inc/Windvale/actions/runs/30853255559): Windows and digest-pinned Debian each complete a zero-warning Release build, all 87 Seed tests, all 39 OS tests, and the complete native CLI gate.

## Reconsider when

- a real object needs noncontiguous physical backing;
- fragmentation makes first-fit insufficient under a measured workload;
- dynamic process launch needs a variable object table or durable identifier allocator;
- virtual mappings, executable publication, pinning, DMA, or capability transfer require separate object records;
- concurrent allocation or SMP requires synchronization and shootdown contracts; or
- a smaller Windvale-owned mechanism can replace the remaining Stage 0 fixed-page leaf.
