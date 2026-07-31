# Decision 0052: First kernel-owned memory foundation

- Date: 2026-07-31
- Status: Accepted, implemented, and qualified on the first Windows QEMU environment

## Context

Decision 0049 boots compiler-generated Windvale after firmware shutdown, but the generated entry still runs on the loader stack and owns no physical page. The handoff retains a validated UEFI map in loader data, so the next bounded kernel slice can establish real ownership without introducing paging, a general heap, or firmware allocation after `ExitBootServices`.

Claiming every memory type that may become reclaimable would require image, runtime-services, ACPI, and loader-lifetime policy that the current probe does not need. Merely counting pages would leave the kernel borrowing all storage. The next evidence must safely write owned memory and execute Windvale code on a stack inside it.

## Decision

- Define [kernel memory version 1](../../Specifications/Windvale-Kernel-Memory.md) over the existing handoff map.
- Claim only `EfiConventionalMemory`. Leave every other known or unknown UEFI type unowned.
- Select the lowest eligible descriptor start at or above 1 MiB that contains one complete 16-page arena below 4 GiB. Reject structural errors, arithmetic overflow, missing space, or any other descriptor overlapping the selected range.
- Zero the complete arena before publishing state. Reserve page zero for a 64-byte `WVKMEM01` state header and copied 48-byte handoff, pages one and two for an 8 KiB kernel stack, and pages three through fifteen for allocation.
- Export an allocate-only page function with explicit state pointer and page count. Allocate monotonically, zero on every successful return, publish state only after complete validation, and leave state unchanged on failure.
- Preserve the loader stack only as a bounded return path. Switch to the owned stack before invoking compiler export `Windvale_kernel_main`, then restore the old stack after that callback returns.
- Keep the retained map buffer borrowed and unmodified. General reclamation waits for a later ownership decision.
- Implement an independent C# planner and simulated allocator first so malformed maps, overlap policy, deterministic selection, exhaustion, state preservation, and zeroing have inspectable tests before matching raw x86-64 is added.

## Consequences

The host oracle makes ownership policy executable without presenting it as boot evidence. It selects the same arena regardless of descriptor order, rejects contradictory overlaps, never claims loader/runtime/platform memory, and provides a deterministic zeroing allocator model.

Firmware probe version 6 now provides that matching machine-code evidence. Its exact QEMU transcript proves arena selection and clearing, one successful zeroing allocation, a copied handoff, and compiler-generated source output after the stack switch. The deterministic 7,168-byte EFI application has SHA-256 `9b58992e480536e9fcf1d4715da04417200cf923388b262aab474abdbf140868`.

The 4 GiB ceiling, fixed 64 KiB arena, allocate-only policy, restored loader stack, and conservative type policy are deliberate first-version limits. They avoid silently becoming the general physical-memory manager.

## Reconsider when

- The kernel must reclaim loader or boot-services memory.
- Paging or higher-half addressing removes the first environment's identity-address assumption.
- Guard pages, multiple stacks, multiple CPUs, or interrupts require a different arena layout.
- A persistent allocator needs release, coalescing, a bitmap, or ownership beyond one range.
- The retained memory map must be copied in full rather than borrowed.
