# Decision 0388: Immutable hosted-container segment set

- Status: Implemented candidate
- Date: 2026-08-08
- Contract: [Windvale native hosted-container segment set](../../Specifications/Windvale-Native-Hosted-Container-Segment-Set.md)
- Predecessor: [Decision 0387](0387-Standalone-Native-Hosted-Container-Segmenter.md)
- Advanced by: [Decision 0389](0389-Shared-Immutable-Snapshot-Sequence.md)

## Context

Decision 0387 made each bounded `WVHT 1` to `WVHU 1` construction available as
a standalone .NET-free process. The normal route still lacked one immutable
description of the complete ordered result and therefore could not safely hand
those files to the existing native durable multi-chunk transaction.

A response-only manifest would be insufficient: valid envelope fields do not
prove that a payload came from the selected plan and source regions. The
admission boundary must bind the layout and independently reconstruct every
response before mutation begins.

## Decision

Add the `WVHM 1` manifest with a canonical segment count, limit, offsets,
request sizes, and response sizes. Limit it to 31 segments so the plan,
manifest, and paired request/response snapshots fit the native host's existing
64-file immutable snapshot table.

Add a portable manifest-admission core and a small hosted admission tool. The
tool validates the selected successful plan, requires every request to carry
its exact 128-byte layout header, reruns the shared Windvale constructor, and
compares every complete response byte. It reserves the destination name for
alias checks but performs no mutation in this slice.

Keep manifest and content semantics in Windvale. The C# fixture builder and
reference-runtime driver are deletion-bound transition evidence; they do not
enter the normal future product path.

## Exact local evidence

The exact admission WVB is 31,271 bytes at SHA-256
`6ce0c3a4bf48b6d0db4c50574805655777be93f6a10555a4d423947b00bd0018`.
The native Project 1 front door reproduces it byte for byte.

One focused current-host test passes. It admits a real segment set and rejects
response corruption, a different valid request plan header, a reordered
manifest entry, and a resource alias before reads. The broad verifier and
Linux execution remain intentionally deferred to the grouped retirement gate.

## Consequences

- The complete segment result now has one bounded immutable admission contract.
- No managed concatenation is needed to establish content integrity.
- This slice performs no output and therefore does not yet retire managed final
  publication.
- The next slice extracts the focused native durable transaction and publishes
  admitted response payloads directly.
- Temporary C# fixture and harness code has an explicit removal condition.

## Reconsideration triggers

Reconsider the file-set boundary when Windvale owns an in-process bounded
pipeline that can preserve the same immutable snapshot, reconstruction, alias,
and failure-before-mutation guarantees. Do not weaken full response comparison
to envelope-only checks.
