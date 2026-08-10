# Windvale native hosted-container source set

## Status and scope

This contract converts the independently produced hosted-container resources
into the exact immutable six-region source set consumed by the native final
segment-request producer. It admits the complete container plan, platform and
startup response envelopes, every segmented service-bundle response, and the
raw runtime header before publishing any derived resource.

The command does not construct those inputs, materialize final application
segments, build the segment-set manifest, publish the destination, launch
children, or manage a temporary directory. It owns response payload admission,
runtime-bound bundle identity, raw source-chunk projection, and final `WVSG 1`
geometry.

## Command contract

```text
wvhostsources <plan.wvcd> <platform.wvhb> <startup.wvsd> <bundle-prefix> <runtime.wvhr> <chunk-prefix> <sources.wvsg>
```

The command derives `<bundle-prefix>.response-N` in canonical order. Every
response must be an exact successful `WVSI 2` envelope whose image extent,
segment offset, segment length, plan bytes, and ten-service count agree with
the admitted `WVCD 1` plan. The process streams the native-fragment and ten
service regions from those payloads and recomputes their SHA-256 values against
the canonical metadata embedded at runtime offset 480. An envelope-preserving
payload mutation is therefore rejected before output.

The admitted startup response binds the complete current request size: 4,674
bytes for Windows and 2,622 bytes for Linux. These totals include the expanded
startup relocation target table and reject a response from the prior geometry.

Successful output consists of `<chunk-prefix>.chunk-N` plus one `WVSG 1`
manifest. Chunks remain below the ordinary 4 MiB byte-value ceiling:

1. raw platform header;
2. raw relocated startup;
3. one raw chunk per service-bundle response payload;
4. raw Windows imports when present;
5. the raw 4,096-byte runtime header; and
6. raw Windows relocation bytes when present.

The manifest always contains six regions in header, startup, bundle, imports,
runtime, and relocation order. Empty Linux imports and relocation retain their
region ordinals but do not create empty chunks. A nonempty region uses its exact
admitted plan offset. An empty region is anchored at the preceding region's
image end rather than copying the plan's zero absent-section sentinel, keeping
the manifest's image offsets nondecreasing. The shared immutable-source
admission core validates the complete manifest before it is written.

All input, derived response, derived chunk, and manifest names are checked for
textual aliases. The command admits every response and its bundle evidence
before writing chunks, and writes the manifest last as the source-set commit
marker. Rejection returns 2, reports one diagnostic, and preserves an existing
manifest and chunks. Wrong argument count returns 64. The application declares
exactly console and diagnostic output, file read/write, and process
argument/count capabilities.

## Exact identities

- `windows-x64-hosted-container-source-set-v1`, producing `.exe`;
- `linux-x64-hosted-container-source-set-v1`, producing `.elf`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Source-set WVB | 82,068 | `7f110c0e7fe9a4a50627e9c600f19c61850e12a265cc44c26ad704353f4b2a74` |
| Windows application | 1,284,096 | `c4626edcc40c2b0c8aff4f4eec8af494034d9bf42fb04959dca393945f7eadfb` |
| Linux application | 1,286,144 | `a2a4687804e063d6f2d9b9c965b07893f749d34da53f271373bc0b41ae671e63` |

The Stage 0 recovery compiler and native Project 1 front door produce identical
WVB bytes. Package layout and identity wiring remain deletion-bound Stage 0
evidence until grouped qualification and promotion.

## Retirement boundary

Managed code no longer needs to strip producer envelopes, concatenate a whole
service bundle, copy final raw regions, or construct the six-region `WVSG` in
the candidate pipeline. The following native segment-request producer consumes
the result without a format adapter.

The [native segment-manifest producer](Windvale-Native-Hosted-Container-Segment-Manifest.md)
now supplies the final `WVHM 1`. Decision 0414 composes the complete candidate
through digest-bound tool acquisition, ordered child execution, bounded bundle
and application segment iteration, and private cleanup. Remaining retirement
work is focused Linux execution, promotion, and the grouped retirement gate.
