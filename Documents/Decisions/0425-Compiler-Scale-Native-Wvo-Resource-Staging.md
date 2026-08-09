# Decision 0425: Compiler-scale native WVO resource staging

- Status: Implemented candidate; Windows compiler-scale staging passes
- Date: 2026-08-09
- Advances: [Decision 0423](0423-Compiler-Scale-Native-Lowerer-Admission.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

Decision 0423 proved that the Windvale lowerer can emit the exact compiler WVO,
but its 167 publication steps exceeded the 62 resource names available after
the input and manifest occupy the first two entries of the immutable 64-entry
snapshot table. Increasing the table would spread a compiler-scale concern
through otherwise bounded host contracts. Changing the shared publication
batch policy would also repin unrelated tools.

Hosted-container composition exposed two adjacent measured bounds. The current
compiler contains 57 records and 22 enums, while the enum-request producer
still carried an older combined nominal-type limit. Its seven canonical native
image fragments plus ten fixed service resources also require 17 `WVHS`
chunks, one more than the previous streaming-evidence limit.

## Decision

Keep the shared lowerer publication policy unchanged. The staging producer
alone coalesces consecutive code publication steps into resources targeted at
1 MiB. Prefix, padding, read-only, symbol, and relocation steps retain their
original resource boundaries because the segmented linker validates those
regions independently. A naturally larger single code step remains protected
by the existing 4 MiB publication maximum.

Raise the hosted streaming-evidence chunk limit from 16 to 18, exactly matching
eight permitted native fragments followed by ten fixed service resources.
Require an 18-resource valid case and a 19-resource rejection that preserves
the existing destination.

Admit the compiler's existing 79 nominal declarations within the already
implemented maximum of 128 total, 64 records, and 64 enums. When hashing
multiple identity regions, read only chunks intersecting the current region;
the hosted metadata regions cover the complete logical source sequence, so
every source byte remains admitted and hashed.

The corresponding C# application writers remain Stage 0 recovery and exact
identity wiring. They do not select staging boundaries, nominal types, chunk
limits, regions, or digests.

## Evidence and consequences

The final producer WVB is 434,372 bytes with SHA-256
`47c400f4069f1dffd84118ac30244dbf80628bdf7b92bdf115152caa2c908cde`.
Its Stage 0 recovery containers are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 6,358,528 | `6aa436726070ec5b478aa9717c8773338e34446db4cefc0bca6deeb427ce9e0b` |
| Linux x64 | 6,356,992 | `3f0f508d1a09d40c14ce084fdec2623822731fb1e182e4fead87a898021a9c93` |

The focused reconstruction owner executes the current-host producer and
segmented linker without loading CLR modules, reconstructs the exact WVO, and
matches the linked image with the independent structural oracle.

One direct Windows native run stages the pinned 914,746-byte compiler WVB at
SHA-256
`48ff781359d9bab96ec3e19e4edba19a26ba82552d5bfd1c1a72d64b75f224a6`.
It produces the exact 27,458,862-byte WVO as 36 resources and a 456-byte
manifest in about 97 seconds. This clears the 62-resource snapshot gate without
changing WVO bytes or the shared publication contract.

The compiler enum request is 13,265 bytes and the unchanged native enum service
is 13,564 bytes. The focused enum owner runs both native processes, verifies all
79 nominal declarations, and requires that no CLR module is loaded. Focused
metadata and streaming-evidence owners pass the 18/19 boundary and native
execution on the final source state.

The complete hosted compiler wrapper is not yet a successful executable. It
passes fixed services, enum construction, 17-resource geometry, publication,
and orchestration, then the metadata-request process returns bounded status 1
while hashing the first 27 MiB compiler identity region. The same portable
SHA-256 path passes its 4 MiB focused case. The next slice must remove the
compiler-scale transient allocation in SHA-256 compression or otherwise prove
a fixed-space portable schedule; it must not merely enlarge an arena.

This is local Windows candidate evidence. Linux execution of the final producer,
final compiler-image link/transport on the 36-resource manifest, hosted compiler
smoke execution, promotion, and grouped dual-host qualification remain open.

## Reconsideration triggers

Revisit the 1 MiB coalescing target only when a measured accepted compiler
cannot fit the 62-resource table or when a smaller deterministic resource plan
improves bounded lifetime. Preserve non-code boundaries unless the segmented
linker contract changes with corresponding malformed-input evidence. Revisit
the 18-chunk limit only if the hosted-container fragment or fixed-service
contracts change explicitly.
