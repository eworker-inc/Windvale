# Windvale native hosted-container segment manifest

## Status and scope

This contract constructs the final `WVHM 1` segment-set manifest from one
admitted complete `WVCD 1` plan and the canonical `WVHT 1` request / `WVHU 1`
response resource pairs already produced for that application.

The command owns resource-name derivation, envelope and extent binding, exact
manifest bytes, self-admission, and one immutable output. It does not construct
requests or responses, re-execute response payloads, publish the destination,
launch children, or manage a temporary directory. The existing segment-set
admission and publisher remain the independent content and mutation boundary.

## Command contract

```text
wvhostsegmentmanifest <plan.wvcd> <segment-prefix> <manifest.wvhm>
```

The command derives `<segment-prefix>.request-N` and
`<segment-prefix>.response-N` in canonical order. Segment count and every
application offset/length come only from the admitted plan and the fixed
4,194,144-byte segment ceiling.

Each request must be an exact `WVHT 1` envelope with the expected plan header,
segment offset and length, payload extent, total length, and zero reserved
field. Each response must be an exact successful `WVHU 1` envelope bound to
that request length, complete application length, segment offset and length,
128-byte plan header, and six-region count. The producer records the actual
request and response byte lengths in each 20-byte manifest entry.

The complete result is independently admitted by the shared segment-set core
before publication. The following `wvhostadmit`/publisher path rereads every
resource, reruns the Windvale segment constructor, and requires exact response
payload equality before destination mutation. Keeping that content check in
the consumer avoids a second segmentation implementation here.

Plan, derived resources, and output must not alias textually. Rejection returns
2, reports one diagnostic, and preserves an existing output. Wrong argument
count returns 64. The application declares exactly console and diagnostic
output, file read/write, and process argument/count capabilities.

## Exact identities

- `windows-x64-hosted-container-segment-manifest-v1`, producing `.exe`;
- `linux-x64-hosted-container-segment-manifest-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Segment-manifest WVB | 35,605 | `8fd1c3f0537694189928f2e78745179519546e91c3df4325fccc451bfebc3133` |
| Windows application | 411,136 | `a63152d9ff108b1e4ccddefbce028a64b43c1b6618efbd288bd07734e39a0ac6` |
| Linux application | 409,600 | `6225cfaab695f281a563b47fe39f1bd2c178c463504951d45c7e7cf73d09c828` |

The Stage 0 recovery compiler and native Project 1 front door produce identical
WVB bytes. Package construction remains deletion-bound Stage 0 wiring until
grouped qualification and promotion.

## Retirement boundary

Managed orchestration no longer needs to count final segments, inspect their
resource envelopes, or serialize the final segment-set manifest. All known
hosted-container binary-format seams now have native process owners.

The remaining normal-path work is digest-bound tool acquisition, ordered child
execution and bounded iteration, private resource cleanup, complete current-
host composition, Linux execution, promotion, and the grouped retirement gate.
