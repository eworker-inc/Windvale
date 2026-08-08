# Decision 0359: Windvale-owned native enum-name leaf

- Status: Accepted current-host leaf transfer; complete WVEN construction advanced by Decision 0362; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0072](0072-Final-Pure-Runtime-Native-Services.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Advanced by: [Decision 0361](0361-Windvale-Owned-Bounded-Native-Enum-Metadata.md) and [Decision 0362](0362-Windvale-Owned-Segmented-Native-Enum-Metadata.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#dynamic-text-and-byte-arena)

## Context

Decision 0072 defines enum naming as a fixed 323-byte x64 leaf followed by one
type-dependent, runtime-private `WVEN` block. C# still emitted the fixed leaf
and constructed the metadata. Those are separate ownership boundaries: the
leaf's RIP-relative displacement targets the byte immediately after its fixed
extent, while the metadata contents vary with verified nominal declarations.

Moving both at once would hide whether a failure came from executable identity
or metadata serialization. This slice transfers only the invariant leaf and
leaves the already verified metadata builder and validator unchanged.

## Decision

- Move the exact qualified 323-byte leaf into one named portable Windvale
  machine template and expose it through a capability-free byte-result bridge.
- Keep enum-name semantics and `WVEN` interpretation in the specification and
  behavioral tests. The template is the x64 implementation artifact, not the
  definition of enum metadata.
- Make the temporary C# bundle wrapper load and verify the Windvale leaf, then
  append its existing deterministically constructed `WVEN` bytes. Remove the
  now-unused general C# service-code builder.
- Keep `Buildˉenumˉmetadata` and `Verifyˉenumˉmetadata` managed for the next
  bounded slice. Do not claim full enum-service construction ownership yet.
- Require an ordinary Project 1 closure to reproduce the exact bridge through
  the native source front door.
- Preserve the ABI, service-table slot, adjacent-metadata placement, arena and
  failure contracts, leaf identity, and descendant application identities.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windvale enum-name leaf core WVB | 625 | `b404104b8e5ca174841b47d02ea45f197599179e0cb23ba778d6a2cdf7846948` |
| Retained enum-name leaf bridge WVB | 592 | `46d806adcceee597a139976748c2e1d5a25dbf57a3fba61c6836b6cf3ce1f76c` |
| `Enumˉname` leaf | 323 | `fb05590c5b6e1791380ba288c4112387e791a18722428c90276796bd409d130a` |

## Evidence and consequences

Stage 0 compiled the focused source for precise diagnostics. The ordinary
native source front door then published the same 592-byte bridge WVB with the
same digest. After reviewing the focused test, the affected Release solution
built with zero warnings and errors in 10.25 seconds. The single named case
passed 1/1 in 1.390 seconds.

The test pins both source results, compares the retained WVB byte for byte,
rebuilds it through the native source front door, and requires the reference
interpreter and verified x64 backend to return the same 323 bytes. It also
constructs a real record-and-enum nominal directory, appends the unchanged C#
metadata result, and passes the complete existing bundle verifier. Existing
dynamic-text coverage remains the semantic evidence for member lookup, unknown
values, metadata corruption, arena allocation, and failure behavior.

The final qualification scripts now reproduce and inspect both modules and
compare the retained bridge exactly. Only their PowerShell and shell syntax is
checked in this slice. C# still constructs and validates `WVEN` and still owns
retained-WVB loading, native lowering, W^X execution/publication, runtime
arenas, service-bundle construction, and host-container orchestration. Linux
execution and the grouped broad gate remain deferred.

## Reconsideration triggers

Replace the leaf bridge when native service-bundle construction can consume it
without the managed loader/executor. Replace the machine template with
structured assembly only while preserving exact identity or through a new
explicit runtime contract. Transfer `WVEN` construction only with independent
valid, boundary, malformed, and deterministic reconstruction evidence.
