# Decision 0100: First reclaimed and reused process root

- Status: Qualified
- Date: 2026-08-02
- Implements: The first reclamation pressure from [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Extends: [Decision 0098](0098-First-Typed-Two-Resource-Lookup.md)
- Contracts: kernel memory 7, protected process 9, resource record 4, native ABI 17/context 7/service table 5, and firmware probe 30

## Context

Probe 29 consumes all 63 arena pages. Its terminal cleanup removes both borrowed aliases and the complete private publication, but deliberately retains the dead client's 42-page address space. A third allocation cannot succeed even though the client can never run again.

The client extent is also the complete suffix ending at the allocator cursor. The fixed process layout and generation-1 record prove that this 42-page suffix is the retired client. That makes one checked LIFO release and immediate reuse the smallest honest reclamation proof: it pressures zeroing, allocator restoration, stale identity, page-table reconstruction, resource regrant, and a second real CPL3 execution without inventing a general physical allocator or scheduler.

## Decision

- Advance firmware to Probe 30, kernel memory to `WVKMEM07`, protected processes to `WVPROC09`, and resource records to `WVRES004`. Retain the 63-page arena, four-page kernel stack, two fixed resources, and 42-page client extent.
- Add `Windvale_kernel_release_tail_pages`. It accepts only a suffix ending exactly at the current cursor, a nonzero page count, checked in-arena arithmetic, and a valid version-7 state. Success restores the prior cursor/free count, zeroes every released byte, and returns the released address. Failure returns zero without mutation. The memory layer has no allocation-boundary record; the bounded process caller proves that this suffix is its complete retired extent.
- Release only the terminal generation-1 client's exact 42-page extent. The allocator moves from cursor `63`/free `0` to cursor `21`/free `42`; immediate allocation of 42 pages must return the identical physical root and restore cursor `63`/free `0`.
- Rebuild every page-table, code, stack, data, context, and process-record byte for generation 2. The reused physical root is an address, not a process identity.
- Append a generation field to each 264-byte process record. A process reference is `(generation << 16) | process_id`: init is `65537`, client generation 1 is `65538`, and client generation 2 is `131074`.
- Store generation-stamped owner and borrower references in `WVRES004`. The first grant requires pristine owned records and records grant count `1`; the second grant requires the exact released generation-1 records, records borrower `131074`, and advances the historical grant count to `2`.
- Reject generation-1 replay against the generation-2 process or resource state. Terminal cleanup must validate the matching generation, borrower reference, mappings, aliases, private directory, and historical grant transition before releasing pages.
- Let the Windvale init service perform grant/receive twice and exit on its fifth syscall. Run the same user-space interpreter twice, once from each generation, returning `29` through the existing channel each time. The contained-user-fault scenario must contain both generation-specific faults and still complete init.
- Keep Windvale source responsible for identity and lifecycle policy, WVA responsible for syscall and privileged mechanics, and Stage 0 responsible for the temporary raw x86-64 release/rebuild emitter and independent oracle.
- Emit `process-reuse=pass` only after the complete machine transition returns successfully. The marker is evidence, not the mechanism.

## Required evidence

- Reference tests for invalid zero/non-tail release, exact tail release, complete zeroing, deterministic same-address reallocation, and unchanged state on failure.
- Planner tests for generation-1 and generation-2 records, both atomic grants and cleanups, historical grant count `2`, and rejection of stale process/resource generations.
- Deterministic WVB, WVA, WVO, process-machine, and firmware identities.
- All 25 OS tests and all four pinned-QEMU scenarios on Windows. Normal and contained-user-fault paths must execute both clients; terminal kernel faults must retain exact panic evidence after the reuse marker.
- The complete Windows/Debian qualification gate before this decision becomes Qualified.

The focused Windows evidence passes all 25 OS tests and all four pinned-QEMU scenarios. Probe 30 produces these exact images:

| Scenario | EFI bytes | SHA-256 | Guest result |
| --- | ---: | --- | ---: |
| Normal | 261,120 | `5034c01a98f20344d96fa091fd9a55a303e72669d746a4b83df2900eed93992f` | poweroff `0` |
| Invalid opcode | 261,120 | `bb57ebf7e50eb56bf3d42d91b2213ed5b262554416fdf76609142eccba44cc55` | panic/host `3` |
| General protection | 261,120 | `d56fe572fb7a7ff724f7b7c26aa5299a6c5cee4c203f009b63d651c1d3cd8fcc` | panic/host `3` |
| Contained user fault | 261,632 | `78dfa73a80a05021273cb44587f6b957d16d4cd4ebaec487f7b8a8f5427846ca` | poweroff `0` |

Exact implementation commit `4a077ab9ebaf2108201927eef3095e87ef2ed907` passes GitHub [Verify run 30749304867](https://github.com/eworker-inc/Windvale/actions/runs/30749304867). Windows and digest-pinned Debian 12 each pass all 67 Seed tests, all 25 OS tests, and the complete non-Fast verifier. Seed elapsed time is 221.700 seconds on Windows and 201.079 seconds on Linux; both logs emit the same 56 SHA-256 values in exact order. QEMU execution remains Windows-only evidence.

## Consequences

Windvale now has its first physical process extent that is actually reclaimed, scrubbed, reused at the same address, and executed again without reusing the old logical identity. Resource history survives the borrower while live aliases do not.

The exact tail rule is intentionally much smaller than a free-list allocator. It is sufficient to prove the lifetime boundary and gives future allocator design real evidence rather than speculation.

## Deliberate non-claims

This decision does not add allocation-boundary provenance inside the memory layer, arbitrary free order, coalescing, fragmentation policy, owner exit, multiple concurrent clients, process identifiers allocated at runtime, PCID, `INVLPG`, SMP shootdown, scheduling, transferable capabilities, dynamic resource names, arbitrary loading, executable publication, JIT, Hyper-V evidence, or removal of Stage 0.

## Reconsideration triggers

Reconsider this boundary when:

- a non-tail process or independently lived resource must be reclaimed;
- two live clients require allocator fragmentation/coalescing policy;
- root reuse overlaps CPUs or PCIDs and requires explicit shootdown;
- process identity allocation must become dynamic; or
- a third runnable creates measured scheduling pressure.
