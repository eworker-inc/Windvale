# Decision 0411: Native hosted-container source set

- Status: Implemented candidate; ordered host-process composition pending
- Date: 2026-08-08
- Advances: [Decision 0410](0410-Native-Hosted-Orchestration-Control.md), [Decision 0404](0404-Native-Hosted-Container-Segment-Request.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted-container source set](../../Specifications/Windvale-Native-Hosted-Container-Source-Set.md)

## Context and decision

The native candidate had independent producers for the platform response,
startup response, segmented service bundle, and raw runtime header, plus a
consumer for final `WVHT 1` segment requests. Managed orchestration still had
to admit and strip those response envelopes, reconstruct the logical service
bundle, copy six raw application regions, and encode their `WVSG 1` geometry.

Add one focused portable admission/geometry core and one paired hosted process.
Keep every large service-bundle payload as its own immutable chunk instead of
constructing a complete application or bundle byte value. Recompute the native
fragment and ten service digests from the response payloads against runtime-
embedded metadata before producing output. Preserve empty Linux region
ordinals, and write the self-admitted manifest last as the commit marker.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Source-set WVB | 72,997 | `5d5b7c36643bbe29f19e9e31d49d635abe7b0a46260aa9ded541239c0bd0eda9` |
| Windows application | 1,021,952 | `378110b7961b374803e0f541f8ffc643672942e1ad7535aa1a3f22af56b4771a` |
| Linux application | 1,024,000 | `aa519c28dc8a0010bdc891899031c0ce6b5f8c30a7ae7f623c5fb53582922831` |

The native front door and Stage 0 recovery compiler produce identical WVB
bytes. After reviewing the test before execution, the focused Release build
passes with zero warnings and the single selected test passes 1/1 in 7.200
seconds. It checks exact WVB and paired application identities, public target
routing, canonical aligned bundle resources, raw output chunks, all six source
regions, empty target-specific ordinals, payload-only tamper rejection before
output mutation, alias rejection, native reconstruction, and current-host
execution without CLR loading. No broad local verifier ran.

The final container-segment request path can now consume native producer
outputs without managed response extraction, managed bundle concatenation, or
managed source geometry. [Decision 0412](0412-Native-Hosted-Container-Segment-Manifest.md)
now owns the final segment-set `WVHM 1`. Remaining lifecycle work is digest-
bound tool selection, ordered bounded iteration, private cleanup, Linux
execution, promotion, and grouped qualification.

## Reconsideration triggers

Version the command if `WVCD`, `WVWB`, `WVLB`, `WVSD`, `WVSI`, `WVSG`, runtime
metadata placement, service order, bundle segment size, or final region order
changes. Do not add child-process authority or temporary-directory policy to
the portable core.
