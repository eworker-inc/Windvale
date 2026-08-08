# Decision 0362: Windvale-owned segmented native enum metadata

- Status: Accepted current-host complete WVEN construction transfer; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0361](0361-Windvale-Owned-Bounded-Native-Enum-Metadata.md), [Decision 0072](0072-Final-Pure-Runtime-Native-Services.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Requires: [Decision 0360](0360-Native-Bounded-Byte-Entry-Input.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#dynamic-text-and-byte-arena)

## Context

Decision 0361 transferred canonical `WVEN` construction through the ordinary
4 MiB Windvale byte-value limit, but the qualified metadata contract permits a
complete block through 32 MiB. A valid larger block therefore still used the
isolated C# writer. Increasing the language-wide byte-value limit for one
runtime-private format would widen unrelated semantics, and reducing `WVEN`
would invalidate an existing contract.

The metadata layout already consists of four ordered sections: one header, all
type directories, all member records, and all name bytes. Windvale can produce
bounded pieces of those sections independently as long as one session owns
their global counts and offsets and the final result is checked as a whole.

## Decision

- Replace runtime-private `WVEQ` version 1 with segmented version 2. Its
  48-byte header records request and final `WVEN` extents, global type/member
  counts, group type/member starts and counts, the group's absolute output name
  start, and the directory offset.
- Partition only between complete nominal types. Never split one enum's member
  set, so duplicate value and name validation remains local and complete.
- Limit one group to 2,048 enum members and require both its request and result
  to fit the existing 4 MiB byte-value limit. Preserve the 100,000,000 native
  instruction ceiling independently for every group.
- Make the portable Windvale core validate every global and group bound,
  canonical directory kind/count, exact request offset and extent, identifier,
  duplicate value, duplicate name, first-group origin, and last-group final
  coverage before emitting bytes. An invalid group returns empty.
- Give each 16-byte request member a lexical-name rank. Windvale proves the
  ranks are a complete permutation and compares each adjacent ranked name in
  strict byte order, avoiding quadratic full-name work without trusting a
  hash or accepting a duplicate.
- Return runtime-private `WVEC` version-1 envelopes. Each 32-byte envelope
  declares exact lengths for an optional first-group `WVEN` header, directory
  bytes, member bytes, and name bytes, followed by those Windvale-constructed
  sections and a zero reserved field.
- Keep the temporary C# session limited to deterministic request projection,
  exact envelope validation, and ordered concatenation into separate header,
  directory, member, and name streams. It does not write a `WVEN` header,
  directory entry, member record, name offset, or reserved field.
- Remove the C# recovery writer. Route every valid `WVEN` through the same
  segmented Windvale constructor, including results between 4 and 32 MiB.
- Retain the independent managed parser temporarily. It checks the assembled
  bytes against every already verified nominal declaration and exact final
  extent before the enum-name service bundle can be published.
- Keep source responsibilities reviewable: the 284-line model/validator owner,
  279-line session transport, and 288-line portable constructor each have one
  named boundary instead of returning the logic to the broad text-service file.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windvale segmented enum-metadata core WVB | 13,946 | `9c61f7d436854ace71ab17fcf33da73c40d37d612f68ba08bfa929ab4e710ef1` |
| Retained segmented enum-metadata bridge WVB | 13,920 | `a43a89cedd7fc58740132c2f666ea69866ceff6ebb87d090124207ff3e9154ce` |

These identities supersede the bounded version-1 constructor identities in
Decision 0361. The unchanged 323-byte enum-name leaf retains its Decision 0359
identity.

## Evidence and consequences

The reviewed focused test pins both new WVB identities, compares the retained
bridge exactly, and reproduces it through the ordinary native source front
door. A fixed 86-byte expected `WVEN` value replaces the deleted C# writer as
the ordinary exact-byte oracle. The reference interpreter and verified x64
backend agree on the same versioned response envelope, while malformed magic,
truncation, duplicate values, duplicate names, and invalid identifiers fail
closed through both.

The legal worst-name boundary keeps 256 members whose 255-byte names share a
251-byte prefix, proving lexical-rank validation remains within the bounded
instruction allowance. The segmented case
constructs an exact 4,217,910-byte `WVEN` from 114 complete maximum-member
enums through 15 bounded requests; every response envelope is checked before
assembly and the complete result passes the independent declaration-aware
validator. Grouped member emission uses one 16-byte append per member so the
bounded result stays within the unchanged 128 MiB execution arena. The final
focused Release project build passed with zero warnings and errors in 9.00
seconds, and the single named test passed 1/1 in 3.807 seconds.

There is no longer a production C# `WVEN` writer. This does not yet remove .NET
from the enum service: C# still projects groups from the managed nominal model,
loads and lowers the retained WVB, owns W^X execution and copying, concatenates
the returned sections, independently validates the complete block, and builds
the containing service bundle. Those are transport, recovery, and publication
items for later slices. Linux execution and the grouped broad gate remain
deferred.

## Reconsideration triggers

Replace `WVEQ` and `WVEC` when a native nominal-declaration owner can invoke the
constructor without managed projection or when a native session can publish
the four sections directly. Change the group limit only with measured
instruction, arena, teardown, and cross-host evidence. Preserve the 32 MiB
`WVEN` bound unless the service contract itself is explicitly versioned.
