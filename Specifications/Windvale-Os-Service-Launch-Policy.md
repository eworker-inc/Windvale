# Windvale OS service-launch policy 1

## Status and scope

Service-launch policy 1 is the first implemented-candidate checked request and
lifecycle model for isolated filesystem and network providers. `WVPR 1` is an
exact 80-byte little-endian value. It admits only init as caller, one named
role/profile, one process, one endpoint, exact page/binding/rights budgets, a
four-slot queue with one control-reserved slot, `Never` restart, and zero flags.

Filesystem profile 2 selects domain reference `65538`, 81 pages, three initial
bindings, rights mask `3`, and a 65,536-byte transfer ceiling. Network profile 3
selects domain reference `65539`, 96 pages, five initial bindings, rights mask
`15`, and a 1,048,576-byte queued-transfer ceiling. These masks are profile
identities, not ambient filesystem or raw-network grants.

The lifecycle is `Planned → Starting → Available → Draining → Stopped`, with
`Faulted` on failed construction or initialization. Publication requires a
nonzero process and endpoint plus successful initialization. Drain checks the
process generation, prevents new work, and cannot finish while work remains.
Version 1 has no automatic restart or mutation replay.

The filesystem partition is exactly 48 RX image pages plus 33 RW/NX private
pages. Its 65,600-byte endpoint envelope occupies the first 17 private pages,
starting 1,024 bytes into the first page. A separate measured 16-page native
stack occupies the remaining private pages. The prior 65-page profile aliases
that stack with live transfer bytes and is rejected by current admission.

## Evidence and limits

[`Service-Launch-Policy.wv`](../Operating-System/Kernel/Service-Launch-Policy.wv)
builds as a 10,150-byte WVB at SHA-256
`d7eabbaaab65ce642f7eb3fd1362429d9994d979974c525408bd7e46a470fa73`.
Its 13,794-byte behavior WVB is
`080520b9caac4b7d47fa6c2972898b7b45cac08f8bab3eb8bea99e4f92aed75c`.
The 42-case launch owner covers the application transaction, `WVSR 1`, both
service profiles, initialization, stale drain, active-work refusal, and failed
construction, including rejection of an undersized filesystem plan.

The policy does not copy user memory, allocate the domain, launch executable
code, create an IPC endpoint, publish a capability, run FAT32, or process a
packet. [Provider launch transaction 1](Windvale-Os-Provider-Launch-Transaction.md)
now composes these values with resource-domain reservation, exact image/page
geometry, private construction evidence, readiness publication, rollback, and
teardown. Its machine-binding leaf maps the logical request/domain identities
to sequential generation-safe process and endpoint identities only after their
fixed slots are released and closed. The privileged record, paging, endpoint,
and context-transfer mechanisms still must consume that transaction before a
live provider claim is made.
