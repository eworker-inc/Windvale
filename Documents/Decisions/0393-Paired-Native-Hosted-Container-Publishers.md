# Decision 0393: Paired native hosted-container publishers

- Status: Implemented candidate; advanced by [Decision 0394](0394-Pruned-Staged-Publisher-Bridge-Closure.md)
- Date: 2026-08-08
- Advances: [Decision 0392](0392-Shared-Immutable-Snapshot-Publisher-Shells.md), [Decision 0388](0388-Immutable-Hosted-Container-Segment-Set.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [Native hosted-container segment set](../../Specifications/Windvale-Native-Hosted-Container-Segment-Set.md) and [WVB publication transaction](../../Specifications/Windvale-Wvb-Publication-Transaction.md)

## Context

Decision 0392 left one connection boundary: package the hosted-container
admission root with the alternating-response selector, shared immutable-snapshot
shell, and durable platform transaction. Stage 0 still concatenated admitted
responses and published the resulting application directly.

The platform transactions also called two private WVB-fragment functions for
publication-state transitions. That kept a managed package builder responsible
for locating and exporting runtime bridge code even though the transition is a
small, format-neutral native contract.

## Decision

Add paired `windows-x64-hosted-container-publisher-v1` and
`linux-x64-hosted-container-publisher-v1` application targets. Each package
contains the exact 31,271-byte Windvale admission module, platform startup,
hosted snapshot policy, shared acquisition shell, hosted sequence validator,
durable transaction, and one shared publication-state object.

`X64-Publication-Transaction-State.wva` now owns the narrow
`Native_publication_begin` and `Native_publication_apply` ABI used by both WVO
and hosted-container transactions. It implements the already specified token
transitions directly and has no capability, service, format, or platform
dependency. The product package no longer extracts those bridge functions from
the C#-compiled WVB fragment.

Expose both targets through `windvale compile` and `windvale aot`. Keep the C#
application writer only as deletion-bound Stage 0 layout, relocation, identity,
and recovery construction. It does not acquire resources, validate payloads,
concatenate segments, or publish a destination at runtime.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Shared publication-state WVO | 433 | `54f18e6221bd40ee9c32a5ad32a747706de9857b5e605c6658a31aaf9c13a0ec` |
| Windows hosted startup WVO | 194 | `84475183f21b69abde8d73cc9748cca7b7c8377335d4a8ddabe8a9dfc88ea57b` |
| Linux hosted startup WVO | 190 | `88d45c0936a81d1727a36a6013353e4b01da2ac3c3e121baa7cf21ee17234965` |
| Windows hosted publisher | 379,904 | `823b9ed3bafdb4a8cb8e5a5a3fe4c9d834f6702771766add5fbf439d8d5d2b37` |
| Linux hosted publisher | 377,725 | `02602e7fb552dafcb6bf2ed2a858eec9c17e257bfd4bc097c47f55fd155a50c9` |
| Windows staged-WVO publisher | 6,459,392 | `5a52359901b1a95b86685ff881a6a242f4573f4bec73b926d149b3c8b6d89f4f` |
| Linux staged-WVO publisher | 6,456,173 | `5cca1d2108c6da5e0c4aa0a751a950a5e97fa595f940cf0883e8ded350d63546` |

The reviewed focused hosted-container test builds through the public CLI target,
executes the current-host Windows package without CLR, `hostfxr`, or
`hostpolicy`, publishes the exact admitted payload, rejects changed response
content, rejects a hard-linked destination alias, preserves inputs and existing
destinations, leaves zero `.wvpub-*` scratch, and reconstructs the exact WVB
through the native Project 1 front door. It passes 1/1 in 5.195 test seconds
after a 9.74-second zero-warning build.

The focused staged-WVO test also passes after adopting the shared state object.
No broader verifier was run because the grouped dual-host gate remains deferred
to the end of the retirement goal.

## Consequences

- Final hosted-container publication no longer requires managed response
  concatenation or managed mutation in the candidate runtime path.
- WVO and hosted-container packages share the same Windvale-assembly publication
  state owner and the same platform acquisition and durable-mutation boundaries.
- The package constructors remain a Stage 0 dependency until their layout and
  relocation work is reconstructed natively and the exact artifacts are
  promoted.
- Linux package identity is pinned, but Linux process behavior is not claimed
  until the independent host run completes.

## Reconsideration triggers

Version the application target or shared state ABI if the admitted snapshot
sequence, publication tokens, startup convention, host runtime layout, or
durability contract changes. Do not move format admission or host mutation back
into the Stage 0 package writer.
