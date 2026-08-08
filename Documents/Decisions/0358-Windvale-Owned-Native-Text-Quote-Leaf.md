# Decision 0358: Windvale-owned native text-quote leaf

- Status: Accepted current-host ownership transfer; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0072](0072-Final-Pure-Runtime-Native-Services.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#dynamic-text-and-byte-arena)

## Context

Decision 0072 moved deterministic text quoting out of a managed callback and
into one exact x86-64 leaf shared by Windows and Linux, but a large C# method
still emitted that leaf for every normal service bundle. The leaf validates
strict UTF-8, measures the complete escaped value before allocation, writes
the Foundation escape representation, and publishes exact failure detail.

The 1,165-byte leaf is already qualified and is a descendant input to pinned
applications. Re-expressing every byte as a second long sequence of Windvale
emitter calls would preserve duplication and make the source harder to review.

## Decision

- Move the exact qualified leaf into one named portable Windvale machine
  template and expose it through a capability-free byte-result bridge.
- Keep quote semantics in the specification and behavioral tests. Treat the
  machine template as the x64 implementation artifact, not as the definition
  of text quoting or UTF-8 validity.
- Prefer the compact 82-line focused source over a much larger mechanical
  translation of the former C# byte-emission method. An assembly-level rewrite
  is a separate change and must reproduce the exact leaf or intentionally
  advance its contract.
- Require an ordinary Project 1 closure to reproduce the exact bridge through
  the native source front door.
- Retain the bridge WVB in the runtime. The C# recovery wrapper may verify,
  lower, execute, identity-check, and cache it, but no longer contains the
  quote emission algorithm.
- Preserve the native ABI, service-table slot, arena/failure contracts, exact
  leaf identity, placement, and every descendant bundle/application identity.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windvale text-quote core WVB | 1,471 | `b23c077329de43fcc307f7e7f564aefe318ca1dd7dc6543bfa10160ab724c453` |
| Retained text-quote bridge WVB | 1,435 | `306b76bcf7e6b3252ce0f9509664acc5ee5a2bcc8fa411e8fdcf2c6a1fb4b631` |
| `Textˉquote` leaf | 1,165 | `4f334af9b6349437d36fd703edb6b5882416f033fae47906a40a4bafdc083bb7` |

## Evidence and consequences

Stage 0 compiled the focused source for precise diagnostics. The ordinary
native source front door then published the same 1,435-byte bridge WVB with the
same digest. After reviewing the focused test against this ownership change,
the affected Release solution built with zero warnings and errors in 13.18
seconds. The single named case passed 1/1 in 1.618 seconds.

The test pins both source results, compares the retained WVB byte for byte,
rebuilds it through the native source front door, verifies the unchanged leaf
identity, and requires the reference interpreter and verified x64 backend to
return the same 1,165 bytes. Existing dynamic-text coverage remains the
qualified semantic evidence for every short escape, ASCII control, BMP and
supplementary scalar, malformed UTF-8, value-limit failure, arena exhaustion,
and mixed allocation. It was not rerun for this exact-byte ownership transfer.

The final qualification scripts now reproduce and inspect both modules and
compare the retained bridge exactly. Only their PowerShell and shell syntax is
checked in this slice. C# still constructs the enum-name leaf and metadata and
still owns retained-WVB loading, native lowering, W^X execution/publication,
runtime arenas, service-bundle construction, and host-container orchestration.
Linux execution and the grouped broad gate remain deferred.

## Reconsideration triggers

Replace the retained bridge when native service-bundle construction can consume
the Windvale result without the managed loader/executor. Replace the machine
template with structured assembly when that source is independently owned and
can preserve the exact identity, or through a new explicit runtime contract
with complete descendant qualification.
