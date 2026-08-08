# Decision 0372: Windvale-owned bounded service-bundle materialization

- Status: Accepted current-host bounded normal-path cutover; segmented large-bundle construction, Linux execution, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0371](0371-Windvale-Owned-Native-File-Input-Leaves.md), [Decision 0365](0365-Native-Publication-Planner-Execution.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native service-bundle materialization](../../Specifications/Windvale-Native-Service-Bundle-Materialization.md)
- Advanced by: [Decision 0373](0373-Windvale-Owned-Segmented-Service-Bundle-Materialization.md)

## Context

Decisions 0355 through 0371 transfer construction of every fixed and
platform-specific native service leaf to Windvale and make the normal runtime
consume exact artifacts. Service-bundle construction still allocated a C#
array, copied the verified fragment, selected zero or NOP alignment fill, and
copied every verified leaf into the Windvale-owned publication plan.

The complete compiler-family images can exceed the source language's ordinary
4 MiB byte-value contract, but many focused hosted tools and ordinary small
fragments fit it. Keeping all materialization managed until the segmented case
is ready would preserve duplicate normal-path ownership unnecessarily.

## Decision

- Define `WVSQ 1` as one exact `WVPQ 1` plus its fragment and ordered leaf
  payload, and `WVSI 1` as the accepted placements plus the complete bounded
  image. Both complete values must fit 4 MiB.
- Make a focused portable Windvale core validate the request, reuse the
  accepted publication plan, copy the exact fragment and leaves, zero-fill the
  initial gap, NOP-fill later gaps, and return the complete image.
- Embed only the digest-bound WVNF fragment in the normal runtime. Retain its
  source and WVB for source reproduction, qualification, differential
  evidence, and Stage 0 recovery.
- Independently verify the complete Windvale response in the host transport:
  envelope, plan, placements, fragment bytes, service bytes, gap bytes, and
  final extent must all agree before use.
- Generalize the narrow publication-planner bootstrap name to a service-free
  bootstrap. Its accepted role remains internal, digest-bound, independently
  verified `Main(bytes) -> bytes` fragments with no services. It does not load
  ambient WVBs or bind capabilities.
- Route every fitting service bundle through the Windvale materializer. Keep
  the byte-identical C# constructor only as
  `Materializeˉstage0ˉlargeˉbundle` when either complete value would exceed
  4 MiB. A later segmented constructor must remove this explicit fallback.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Materialization core WVB | 15,245 | `54a0cb83cba3c9c9118cfc209aaef43938f9f9a9f4212ccb9d4657ce6a139ba1` |
| Materialization bridge WVB | 15,253 | `25512a7c3e6eae0dd060426d5a51a93abfc7a7127f59538fd2a315242ed2b660` |
| Materialization bridge WVNF | 157,174 | `8bb1f06bd8b25d9a5ff78971ad4af36b609c618b080ed0fa9b17fe4b51669629` |

## Evidence and consequences

The focused test was reviewed before execution. It compiles and pins the
portable core and bridge, compares retained WVB and WVNF bytes, proves the
runtime does not embed the WVB, compares valid and six malformed requests
between the reference interpreter and native execution, checks exact zero and
NOP fills, exercises the normal Windows and Linux one-service bundle paths,
and reproduces the bridge through the native project front door.

The first run rejected the valid request immediately because the decimal
`WVSQ` magic literal was 4,096 below `0x51535657`; no image was constructed.
After correcting and regenerating all pinned identities, the final-state
focused test passes 1/1 in 1.956 seconds. The affected Release test project
builds with zero warnings and errors in 8.31 seconds.

Bounded normal bundles no longer use C# to construct image bytes or choose
alignment fill. Platform leaf selection and verification, adapter/table-slot
metadata, the large compiler-image fallback, final W^X authority, context and
arena ownership, table binding, invocation, and teardown remain managed.
Linux execution, Development, Standard, Qualification, and the grouped
retirement gate were not run.

## Reconsideration triggers

Replace the fallback with a segmented Windvale materializer before claiming
complete service-bundle ownership. Do not raise the general byte-value limit,
join a compiler-scale image temporarily, weaken complete-byte verification, or
allow arbitrary service-bearing fragments through the service-free bootstrap
to make that later slice appear complete.
