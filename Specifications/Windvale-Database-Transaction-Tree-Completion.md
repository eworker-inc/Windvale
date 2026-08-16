# Windvale database transaction tree completion

## Status

- Version: `WVTC 1`
- Profile: portable
- Input root depths: 2 through 8
- Maximum ancestor rounds: 6
- Maximum new pages: 792
- Maximum page companion: 51,904,512 bytes
- Evidence: focused segmented Windows native execution; independent Linux
  execution pending

## Purpose

`Databaseˉtransactionˉtreeˉcompletionˉplan` is the first general bottom-up
transaction-tree coordinator. It consumes one selected snapshot, one canonical
`WVTM 1` mutation set, and the exact validated path for every mutation. It
returns every new durable tree page in one consecutive allocation batch and
names exactly one completed root. It performs no storage I/O and does not
publish a superblock.

The coordinator reuses the existing planners instead of implementing their
rules again:

1. `WVBD 1` creates changed leaves and their immediate parents.
2. For input depths three through eight, exactly `root depth - 2` rounds of
   `WVAG 1` and `WVAP 1` rebuild every remaining committed ancestor.
3. A one-page old-root result is the completed root.
4. A split old-root result is passed to `WVRG 1`, which creates as many bounded
   new levels as required to leave exactly one root.

Depth-two input skips ancestor rounds. A logical no-op returns an unchanged
manifest with no pages and retains the committed root, generation, and
sequence.

## Allocation and bounds

Page identities begin at the selected snapshot's committed page count. Leaf,
immediate-parent, ancestor, and root-growth pages are appended in that order
without gaps or overlap. Every changed page uses the next generation and
commit sequence. The completed root is the last allocated page.

The component ceilings give one explicit worst-case envelope:

| Component | Maximum pages |
| --- | ---: |
| changed leaves | 64 |
| immediate parents | 95 |
| six ancestor rounds | 570 |
| split-root growth | 63 |
| total | 792 |

At the maximum admitted 64 KiB page size, 792 pages are 51,904,512 bytes.
The mutation companion remains capped at 256 KiB and the path companion at
16 MiB. A transaction that exceeds any component or aggregate ceiling fails
before returning a partial result.

The current immutable byte builders copy while combining rounds. The fixed
ceiling prevents unbounded growth, but it is not a claim that the worst shape
is an acceptable persistent-server allocation profile. Server benchmarks must
measure peak memory and replace aggregation with a pre-sized or streaming page
sink when the measured cost is material.

## `WVTC 1` encoding

The fixed 208-byte little-endian manifest binds, but does not duplicate, the
mutation, path, and page companions:

| Offset | Type | Meaning |
| ---: | --- | --- |
| 0 | `u32` | magic `WVTC` |
| 4 | `u32` | version `1` |
| 8 | `u32` | header length `208` |
| 12 | `u32` | flags: changed bit 0 and root-grown bit 1 |
| 16 | `u32` | durable page size |
| 20 | `u32` | input root depth |
| 24 | `u32` | output root depth |
| 28 | `u32` | ancestor-round count |
| 32 | `u32` | leaf-page count |
| 36 | `u32` | immediate branch-page count |
| 40 | `u32` | aggregate ancestor-page count |
| 44 | `u32` | root-growth page count |
| 48 | `u32` | total new-page count |
| 52 | `u32` | mutation companion length |
| 56 | `u32` | path companion length |
| 60 | `u32` | page companion length |
| 64 | `u64` | target generation |
| 72 | `u64` | target commit sequence |
| 80 | `u64` | first allocated page, or no-page for no-op |
| 88 | `u64` | next unallocated page, or no-page for no-op |
| 96 | `u64` | completed or retained root page |
| 104 | `u32` | total manifest length, exactly `208` |
| 108 | `u32` | reserved, zero |
| 112 | 32 bytes | SHA-256 of the complete mutations |
| 144 | 32 bytes | SHA-256 of the complete paths |
| 176 | 32 bytes | SHA-256 of the complete page batch |

Decoding checks all flags, counts, depths, lengths, arithmetic, no-op rules,
root-growth rules, and allocation identities. `validate` additionally checks
all three companion hashes and replays the complete plan from the selected
snapshot. The replayed manifest and page bytes must match exactly.

## Native construction

The composed integration projects exceed the ordinary native lowerer's fixed
4 MiB complete-object limit. They therefore use the already qualified staged
WVO publication, staged image link, canonical image transport, and segmented
hosted packaging path. This preserves the ordinary limit and deterministic
function ordering; it does not weaken verification or create a database
format dependency on native segmentation.

## Verification and next step

Focused native tests cover a two-mutation depth-four tree completing seven
consecutive pages across two ancestor rounds, logical no-op, deterministic
planning, manifest replay, malformed manifests, and digest mismatch. A second
test fills a depth-two root to 3,959 bytes, splits one leaf, splits the old root
into two branches, and proves that the coordinator invokes `WVRG 1` to produce
one depth-three root.

The next transaction milestone aggregates obsolete-page ownership, constructs
the compact commit log, and hands this exact page batch and completed root to
the existing durable publication planner. The persistent server follows that
complete portable commit boundary.
