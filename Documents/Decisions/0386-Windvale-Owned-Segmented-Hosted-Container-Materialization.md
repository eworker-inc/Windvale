# Decision 0386: Windvale-owned segmented hosted-container materialization

- Status: Accepted current-host complete byte-construction transfer; native publication, Linux execution, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0385](0385-Windvale-Owned-Hosted-Container-Construction.md), [Decision 0373](0373-Windvale-Owned-Segmented-Service-Bundle-Materialization.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native hosted-container construction](../../Specifications/Windvale-Native-Hosted-Container-Construction.md)

## Context

Decision 0385 transferred layout, targets, startup instantiation, platform
headers, imports, and relocation semantics to Windvale. Its temporary managed
relay still allocated one complete roughly 27 MiB application and copied each
Windvale-owned and opaque source region into the declared positions. That
physical writer remained in the normal construction call graph and could not
be reused directly by a bounded native publisher.

The ordinary Windvale `bytes` limit remains 4 MiB. Raising it for one private
linker operation would widen unrelated language and runtime contracts.

## Decision

- Add one portable `WVHT 1` / `WVHU 1` segment constructor over the successful
  `WVCD 1` plan header.
- Fix canonical segment length at 4,194,144 bytes, leaving exact request space
  for the segment header and plan while keeping the response below 4 MiB.
- Carry only source bytes intersecting the requested segment in canonical file
  order. Windvale rederives the complete layout and constructs every omitted
  zero and padding byte.
- Replace the complete managed array writer with a focused deletion-bound
  session that projects requests, invokes the digest-bound WVNF, independently
  validates each returned source/fill range, and concatenates segments in order.
- Preserve the existing complete application and PE/ELF verifiers. Keep the
  former C# application builders only as `Buildˉstage0` differential/recovery
  oracles.
- Keep the 238-line portable constructor and the transport session as focused
  owned files; do not fold either into the existing planner or platform byte
  constructors merely to reduce the file count.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Segmentation WVB | 21,806 | `c1c446d22e578eac330a0bead108d4d759b7c346c48c335601df62e19538bca4` |
| Segmentation WVNF | 278,243 | `f80570a216cbf99e04b83f8e5c8f576f0f8f9d179fdc907715b7f80a57e43c3a` |

## Evidence and consequences

The reviewed focused segmentation case pins and reconstructs both artifacts,
compares interpreter and native execution for Windows and Linux plans, crosses
the canonical segment boundary with a two-segment application, checks the
deletion-bound session, and rejects eight truncated, oversized,
misidentified, malformed-plan, noncanonical-segment, and inconsistent-payload
envelopes. It passes 1/1 in 3.040 seconds. The pre-existing complete-container
case then passes 1/1 in 7.170 seconds through the new path, retaining all twelve
byte-for-byte Stage 0 application comparisons and independent PE/ELF checks.
Both affected builds complete with zero warnings and errors.

There is no longer a production C# final-image region writer. C# still performs
native-fragment dispatch, bounded request projection, independent response
validation, ordered segment concatenation, and caller-facing publication.
Direct native durable publication is the next boundary; Linux execution and
the grouped broad gate remain deferred.

## Reconsideration triggers

Change the segment bound only with measured request, response, instruction,
arena, and publication evidence. Replace managed concatenation when the native
publisher consumes `WVHU` segments directly. Do not create a second large-value
container contract or reintroduce platform layout calculations in the adapter.
