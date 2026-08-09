# Windvale native hosted-verifier metadata

## Status and scope

This contract moves construction and portable admission of the fixed read-only
verifier-family `WVHV 1` metadata into Windvale. It does not reinterpret the
seven compiler-family hosted profiles. In particular, `WVHV` profile 2 remains
the compiler-aligned WVB verifier while `WVHB` profile 2 remains the separate
compiler build driver.

The portable constructor accepts one exact `WVVR 1` request and returns the
shared `WVHD 1` metadata-construction response. The current slice owns the
compiler-aligned verifier's five capabilities and six services. Inspector,
runner, WVO-inspector, and console-application-verifier service variants remain
later explicit extensions.

## `WVVR 1` request

The request is exactly 384 bytes, little-endian, and contains no paths or host
handles.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVVR` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `384` |
| 12 | 4 | target | `1` Windows or `2` Linux |
| 16 | 4 | verifier profile | `2` |
| 20 | 4 | bundle offset | `4,096` |
| 24 | 4 | bundle bytes | `1` through `35,651,584` |
| 28 | 4 | native image bytes | `1` through `33,554,432`, within the bundle |
| 32 | 4 | native entry | Within the native image |
| 36 | 4 | service count | `6` |
| 40 | 32 | native SHA-256 | Nonzero exact evidence |
| 72 | 24 | reserved | Zero |
| 96 | 288 | six service records | Six records of 48 bytes |

Each service record contains image offset, nonzero code length, SHA-256, and
eight zero reserved bytes. Records are ordered, nonoverlapping, and wholly
inside the bundle after the native image. The fixed service order is console
output, argument count, argument lookup, file input, strict UTF-8 validation,
and diagnostic output.

## `WVHD 1` response

Failure is the shared 32-byte `WVHD 1` response. Statuses distinguish size,
magic, version, fixed header/authority, service record, and constructed-metadata
admission failures; failure offset identifies the rejected boundary.

Success is 1,056 bytes: the 32-byte response header followed by exact 1,024-byte
`WVHV 1` metadata. Windvale constructs:

- magic `WVHV`, metadata version 1, outer container format 4, ABI 22,
  execution-context format 7, and service-table format 5;
- five canonical read-only capability records;
- six canonical service records with target-specific adapters and exact input
  placements/digests;
- the 2 MiB record arena, 128 MiB hosted text arena, profile 2, and the bounded
  16,000,000,000-instruction meter;
- a completely zero reserved tail.

The constructor invokes the separate portable admission module over its own
result before returning success. Admission independently checks the header,
authority records, service mapping, ordered extents, digests, bounds, meter,
and reserved bytes.

## Ownership and evidence

[`Native-Hosted-Verifier-Metadata-Construction-Core.wv`](../Runtime/Windvale/Native-Hosted-Verifier-Metadata-Construction-Core.wv)
owns construction. [`Native-Hosted-Verifier-Metadata-Admission.wv`](../Runtime/Windvale/Native-Hosted-Verifier-Metadata-Admission.wv)
owns admission. The small byte-input bridge is the root of
[`Windvale-Native-Hosted-Verifier-Metadata.wvproj`](../Windvale-Native-Hosted-Verifier-Metadata.wvproj).

The native project front door constructs an exact 21,566-byte WVB with SHA-256
`dc7c88f8ec9b6ddd77695b7890eeb6292314fcabd4939239c273908f3afa894b`.
The focused current-host test obtains that WVB from the native compiler, proves
its service-free `Main(bytes) -> bytes` shape, executes both the Windvale
interpreter and native backend, and compares successful Windows and Linux
metadata byte-for-byte with the frozen C# recovery oracle. It also covers
truncation, magic, version, total, target, profile, bundle/native bounds,
service count, native digest, reserved bytes, and malformed service records.

The frozen C# source compiler does not compile this new module; new source
semantics belong to the Windvale compiler under Decision 0213. C# participates
only after native WVB construction as differential metadata and execution
evidence. Native request production, retained application packaging, verifier
startup selection, outer layout/segmentation, independent Linux execution, and
promotion remain separate later slices. The exact runtime header is now owned
by the verifier-specific Windvale constructor under Decision 0462.
