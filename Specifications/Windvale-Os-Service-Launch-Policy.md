# Windvale OS service-launch policy 1

## Status and scope

Service-launch policy 1 is the first implemented-candidate checked request and
lifecycle model for isolated filesystem and network providers. `WVPR 1` is an
exact 80-byte little-endian value. It admits only init as caller, one named
role/profile, one process, one endpoint, exact page/binding/rights budgets, a
four-slot queue with one control-reserved slot, `Never` restart, and zero flags.

Filesystem profile 2 selects domain reference `65538`, 64 pages, three initial
bindings, rights mask `3`, and a 65,536-byte transfer ceiling. Network profile 3
selects domain reference `65539`, 96 pages, five initial bindings, rights mask
`15`, and a 1,048,576-byte queued-transfer ceiling. These masks are profile
identities, not ambient filesystem or raw-network grants.

The lifecycle is `Planned → Starting → Available → Draining → Stopped`, with
`Faulted` on failed construction or initialization. Publication requires a
nonzero process and endpoint plus successful initialization. Drain checks the
process generation, prevents new work, and cannot finish while work remains.
Version 1 has no automatic restart or mutation replay.

## Evidence and limits

[`Service-Launch-Policy.wv`](../Operating-System/Kernel/Service-Launch-Policy.wv)
builds as a 10,150-byte WVB at SHA-256
`b81513e5ac366389b09fd5bce075d6bd480c970ef910250f2d2281e64bb57eed`.
Its 13,333-byte behavior WVB is
`6692d65d3c428138d157e81d4fde967df181e16337085866e7a253c1b2e8c2ab`.
The 32-case launch owner covers the application transaction, `WVSR 1`, both
service profiles, initialization, stale drain, active-work refusal, and failed
construction.

The policy does not copy user memory, allocate the domain, launch executable
code, create an IPC endpoint, publish a capability, run FAT32, or process a
packet. [Provider launch transaction 1](Windvale-Os-Provider-Launch-Transaction.md)
now composes these values with resource-domain reservation, exact image/page
geometry, private construction evidence, readiness publication, rollback, and
teardown. The privileged machine mechanisms still must bind that transaction
before a live provider claim is made.
