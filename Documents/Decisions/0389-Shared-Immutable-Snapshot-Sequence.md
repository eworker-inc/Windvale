# Decision 0389: Shared immutable snapshot sequence

- Status: Implemented candidate
- Date: 2026-08-08
- Advances: [Decision 0351](0351-Immutable-Snapshot-Compiler-Image-Staging.md), [Decision 0388](0388-Immutable-Hosted-Container-Segment-Set.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native x64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md#hosted-immutable-snapshot-staging-boundary)
- Advanced by: [Decision 0390](0390-Reusable-Linux-Durable-Multi-Chunk-Publication.md)

## Context

The staged-WVO publisher already validated the native immutable snapshot table
before its durable multi-chunk transaction, but the validator embedded WVO's
ordinal-two, contiguous-chunk, zero-header, and 32 MiB policy. The hosted
container segment set uses the same trusted table shape with a different
selection: plan and manifest first, then alternating `WVHT` requests and
`WVHU` responses whose 40-byte envelopes must not reach the destination.

Copying table and pointer validation into another large platform adapter would
create two security-sensitive implementations and work against the repository's
focused-source guidance.

## Decision

Add `X64-Immutable-Snapshot-Sequence.wva` as the format-neutral owner of exact
`WVFI 1` table identity, platform binding, arena pointers, per-record pointers,
name and data bounds, selected payload bounds, checked aggregate size, and a
nonempty selected sequence. The caller supplies bounded minimum/maximum record
counts, first payload ordinal, stride one or two, fixed header skip, and maximum
aggregate payload bytes.

Replace the former complete WVO validator with a small policy wrapper selecting
ordinals `2,3,4...`, stride one, skip zero, and the retained 32 MiB ceiling.
Add a separate hosted-container wrapper selecting ordinals `3,5,7...`, stride
two, skip 40, an even snapshot count, and the canonical 31-segment maximum of
130,018,464 payload bytes.

Link the shared object and WVO wrapper as separate focused WVOs. Do not add a
second platform table parser or embed hosted constants in the future Windows
and Linux transaction sources.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Shared immutable sequence WVO | 1,282 | `7c6ea6b16ac8cfcfed9e0983b7e6aedc3ead4aab3a54cb207b75d22a228db676` |
| Staged-WVO policy wrapper | 224 | `03ff27e8a8fce7b3eddfb0191b6626c20971df32790f8f7274cd9091a4b69628` |
| Hosted-container policy wrapper | 256 | `390ee99e24e02cfa904f64d1ab772d76f5de358783c3f75e0310e37750cc5e86` |
| Windows staged-WVO publisher | 6,458,368 | `8c966338fe0a138fba967ece764883c6b34c25104fb9eb1f8c6995a040ae303b` |
| Linux staged-WVO publisher | 6,455,341 | `03f15565cb00ad69ecfed45beb8e58d1898d8fcb29fa5ab2c7aef982ea9f7ea7` |

The reviewed focused test assembles and pins all three policy objects, rebuilds
both staged-WVO applications, and executes the current-host publisher through
successful atomic replacement, changed-content rejection, destination alias
rejection, destination preservation, and zero scratch residue. It passes 1/1
in 6.174 test seconds after a 12.17-second zero-warning affected-project build.

The hosted wrapper has assembly and exact-object evidence only in this slice.
Its real snapshot-table execution, Windows/Linux application packages, payload
publication, failure preservation, and dual-host qualification belong to the
next hosted-container publisher slice.

## Consequences

- Native immutable snapshot validation now has one format-neutral owner.
- Existing WVO publication retains its exact policy and real behavior.
- The hosted publisher can consume response payloads without copying or
  compacting requests and without writing `WVHU` envelopes.
- The large platform adapters can be reorganized around acquisition and durable
  transaction ownership rather than repeating table validation.
- No C# product semantics were added; changed C# code only links and pins
  Windvale assembly objects during the Stage 0 packaging transition.

## Reconsideration triggers

Version or replace this boundary if the `WVFI` table layout, arena geometry,
snapshot lifetime, platform codes, 64-record ceiling, or supported stride
family changes. Do not accept arbitrary selector arithmetic or mutable records
without a new verifier and hostile-input evidence.
