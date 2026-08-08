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
region ordinals but do not create empty chunks. Region offsets and lengths must
match the admitted plan exactly. The shared immutable-source admission core
validates the complete manifest before it is written.

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
| Source-set WVB | 72,997 | `5d5b7c36643bbe29f19e9e31d49d635abe7b0a46260aa9ded541239c0bd0eda9` |
| Windows application | 1,021,952 | `378110b7961b374803e0f541f8ffc643672942e1ad7535aa1a3f22af56b4771a` |
| Linux application | 1,024,000 | `aa519c28dc8a0010bdc891899031c0ce6b5f8c30a7ae7f623c5fb53582922831` |

The Stage 0 recovery compiler and native Project 1 front door produce identical
WVB bytes. Package layout and identity wiring remain deletion-bound Stage 0
evidence until grouped qualification and promotion.

## Retirement boundary

Managed code no longer needs to strip producer envelopes, concatenate a whole
service bundle, copy final raw regions, or construct the six-region `WVSG` in
the candidate pipeline. The following native segment-request producer consumes
the result without a format adapter.

The remaining composition work includes digest-bound tool acquisition, ordered
child execution, bounded bundle and application segment iteration, native
construction of the final `WVHM 1` segment-set manifest, private resource
cleanup, Linux execution, promotion, and the grouped retirement gate.
