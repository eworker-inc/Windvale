# Decision 0663: Windvale-owned client-reclamation preflight

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0662](0662-Windvale-Owned-First-Client-Completion-Cleanup.md)
- Contract: [client-reclamation preflight emission](../../Specifications/Windvale-Os-X64-Process-Client-Reclamation-Preflight-Emission.md)

## Decision

Emit fixture offsets 17,924 through 19,525 as one fail-closed pre-reclamation
transaction. Enter the selected client address space and revalidate the closed
endpoint/channel state, dormant compatibility arena, retained program, store,
and directory descriptors, exact hashes and mappings, and selected exiting
client state before permitting memory-object release.

## Evidence and consequences

The normalized slice SHA-256 is
`b490ec77921d546399d27dd907d76ace8c7f2b1a4b04ee50ab1d01af8a37d514`.
The focused owner advances to thirty-nine projects and 234 cases with results
50 through 88. Windvale source owns the first 19,526 process-machine bytes and
108 external relocation fields.

Reclamation now has a checked admission boundary that cannot silently consume
stale endpoint, channel, descriptor, hash, mapping, or process state. Actual
memory release, generation-2 allocation and reconstruction, later lifecycle,
handler bodies, context switching, and live QEMU execution remain separate.

## Reconsideration triggers

Another reclamation design must preserve exact address-space activation and all
closed-state, generation, descriptor, hash, mapping, and selected-client checks
before releasing or reusing any object, and must fail closed on every mismatch.
