# Windvale native hosted-verifier metadata

## Status and scope

This contract moves construction and portable admission of the fixed read-only
verifier-family `WVHV 1` metadata into Windvale. It does not reinterpret the
seven compiler-family hosted profiles. In particular, `WVHV` profile 2 remains
the compiler-aligned WVB verifier while `WVHB` profile 2 remains the separate
compiler build driver.

The portable constructor accepts one exact `WVVR 1` request and returns the
shared `WVHD 1` metadata-construction response. Profiles 2 and 8 retain the
compiler-aligned verifier's five capabilities and six services. Profiles 6
and 7 own the WVO inspector and console-application verifier respectively;
each retains the same five capabilities plus five pure report services.
Other inspector and runner variants remain separate explicit extensions.

## `WVVR 1` request

The profile-2/profile-8 request is exactly 384 bytes and the profile-6/profile-7
request is exactly 624 bytes. Both are little-endian and contain no paths or host
handles.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVVR` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `384` |
| 12 | 4 | target | `1` Windows or `2` Linux |
| 16 | 4 | verifier profile | `2` or `8` |
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

Profiles 6 and 7 change only the fixed shape fields and service suffix: total
bytes is 624, the profile is 6 or 7, service count is 11, and offsets 96 through
623 contain eleven 48-byte records. Services 7 through 11 are `enum.name`,
`text.concat`, `text.quote`, `i32.format`, and `u32.format`; they use zero
capability identity, pure-service flags, and fixed adapters 9 through 13.

## `WVHD 1` response

Failure is the shared 32-byte `WVHD 1` response. Statuses distinguish size,
magic, version, fixed header/authority, service record, and constructed-metadata
admission failures; failure offset identifies the rejected boundary.

Success is 1,056 bytes: the 32-byte response header followed by exact 1,024-byte
`WVHV 1` metadata. Windvale constructs:

- magic `WVHV`, metadata version 1, outer container format 4, ABI 22,
  execution-context format 7, and service-table format 5;
- five canonical read-only capability records;
- six or eleven canonical service records with target-specific adapters and
  exact input placements/digests;
- the 2 MiB record arena, 128 MiB hosted text arena, selected profile, and the
  bounded 16,000,000,000-instruction meter;
- a completely zero reserved tail: 432 bytes after the six-service records or
  112 bytes after the eleven-service records.

The constructor invokes the separate portable admission module over its own
result before returning success. Admission independently checks the header,
authority records, service mapping, ordered extents, digests, bounds, meter,
and reserved bytes.

## Ownership and evidence

[`Native-Hosted-Verifier-Metadata-Construction-Core.wv`](../Runtime/Windvale/Native-Hosted-Verifier-Metadata-Construction-Core.wv)
owns construction. [`Native-Hosted-Verifier-Metadata-Admission.wv`](../Runtime/Windvale/Native-Hosted-Verifier-Metadata-Admission.wv)
owns admission. The small byte-input bridge is the root of
[`Windvale-Native-Hosted-Verifier-Metadata.wvproj`](../Windvale-Native-Hosted-Verifier-Metadata.wvproj).

The current profile-7-capable source constructs a 23,446-byte WVB with SHA-256
`4041ba7b2188127f3a0bb7f20673376812b8d52e207cb237349fb0cdd63d7470`.
The focused current-host test retains the service-free
`Main(bytes) -> bytes` shape and its profile-2 malformed cases, and now defines
successful Windows/Linux profile-7 comparisons across the Windvale interpreter,
native backend, and frozen C# recovery oracle. This source slice measured the
WVB without executing that test; complete application construction and
promotion remain separately owned.

The frozen C# source compiler does not compile this new module; new source
semantics belong to the Windvale compiler under Decision 0213. C# participates
only after native WVB construction as differential metadata and execution
evidence. Native immutable evidence acquisition, retained application
packaging, verifier startup selection, outer layout/segmentation, independent
Linux execution, and promotion remain separate later slices. Exact `WVVR`
projection and the runtime header are now owned by verifier-specific Windvale
constructors under Decisions 0463 and 0462.
