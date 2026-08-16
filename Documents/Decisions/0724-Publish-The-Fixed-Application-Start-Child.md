# Decision 0724: Publish the fixed application-start child

**Status:** Accepted
**Date:** 2026-08-16

## Context

Operation 8 reached the boot-linked process machine with a derived init
context, bounded user copy, and admitted `WVSR 1` snapshot. The boot process
policy already constructed and charged one fixed generation-1 child, but the
syscall returned only admission status and exposed no process reference.

General executable loading and private object allocation require larger
package, memory, rollback, and capability-transfer boundaries. Treating the
existing child as if it were general construction would overstate the current
OS.

## Decision

Operation 8 may publish only child reference `65538`. After the existing copy
leaf succeeds, its handler supplies kernel-derived pairs from the fixed
child's `WVPROC17` record to a separately assembled publication leaf. That
leaf revalidates all 16 fields of the exact 64-byte request and the record's
magic, version, size, process/thread states, process/thread identities,
generation, and rights profile. A complete match returns `65538`; a null,
drifted, missing, or differently shaped machine returns status `7` and exposes
no child.

The kernel snapshot remains erased on every post-copy return. Operations 1
through 7 and their original branch targets remain unchanged. The publication
leaf is a distinct link input and the process object carries an explicit typed
relocation to it.

## Consequences

The fixed application-start path now has a usable process-reference result and
can support the next boot-linked service-launch work. It does not allocate,
map, enter, cancel, or supervise an arbitrary process, and the retained init
does not yet invoke operation 8.

Focused application-launch evidence grows to 69 cases and three native leaves.
The process WVO is 952,002 bytes at SHA-256
`c4606029a8af59770b2022f710b26fcd6d4207dba9d9d939c25faf423ee96d50`.
All three EFI scenarios are 1,693,184 bytes; the normal image at SHA-256
`e3ac1ee784ce4ccd00821ff87e0931b73397d70974867343248ff632ab20641c`
passes the pinned QEMU/OVMF marker and guest-controlled shutdown.

## Reconsideration triggers

Replace this fixed publication boundary when application start owns general
private machine construction, when init issues the first live request, or when
service launch needs a separately versioned request and result contract.
