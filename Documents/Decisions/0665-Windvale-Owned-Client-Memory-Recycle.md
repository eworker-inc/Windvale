# Decision 0665: Windvale-owned client-memory recycle

- Status: Implemented current-Windows-host native candidate; live application execution pending
- Date: 2026-08-16
- Advances: [Decision 0663](0663-Windvale-Owned-Client-Reclamation-Preflight.md)
- Contract: [client-memory recycle emission](../../Specifications/Windvale-Os-X64-Process-Client-Memory-Recycle-Emission.md)

## Decision

Emit fixture offsets 19,526 through 19,741 as one checked release/reallocation
transaction. Release only the selected generation-1 client's exact 122-page
memory object, require the allocator to restore cursor 13 and 122 free pages,
allocate generation 2 under reference `0x00020002`, reapply all geometry checks,
and require the identical physical root before object reconstruction.

## Evidence and consequences

The normalized slice SHA-256 is
`831ec3d8cf08158457764eea0980ab9e0a431b27c6f1fd46c863ec86c3bbf51d`.
The focused owner advances to forty projects and 240 cases with results 50
through 89. Windvale source owns the first 19,742 process-machine bytes and 120
external relocation fields.

Generation-safe client memory recycling is now checked at the process-machine
boundary without disturbing the later directory allocation. Generation-2
record, paging, resource, endpoint, and context reconstruction and re-entry
remain separate.

## Reconsideration triggers

Another recycling design must preserve exact generation-1 admission, release
identity, zero-before-reuse behavior, allocator restoration, generation-2
identity, bounded geometry, same-root evidence, and fail-closed publication.
