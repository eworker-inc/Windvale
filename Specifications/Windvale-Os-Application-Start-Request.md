# Windvale OS application-start request 1

## Status and scope

`WVSR 1` is the first implemented-candidate serialized boundary in front of
[application-launch policy 1](Windvale-Os-Application-Launch-Policy.md). It is
an exact 64-byte little-endian value copied from an untrusted caller before
validation. It contains no pointer, path, package name, image address, native
handle, or variable-length region. The decoder is portable Windvale code and
does not invoke a provider or mutate kernel state.

Version 1 represents only the measured Probe 40 application profile. It does
not admit service roles, arbitrary resource charges, or a public syscall.

## Encoding

| Offset | Bytes | Field | Version-1 requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVSR` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | `64` |
| 12 | 4 | role | application `1` |
| 16 | 4 | request reference | nonzero; typed policy currently accepts generation references `65537` or `131073` |
| 20 | 4 | caller reference | nonzero; typed policy requires init `65537` |
| 24 | 4 | resource-domain reference | nonzero; typed policy requires `65537` |
| 28 | 4 | executable-publication reference | nonzero; typed policy requires `65576` |
| 32 | 4 | admission profile | profile `1` |
| 36 | 4 | process charge | `1` |
| 40 | 4 | ordinary-page charge | `122` |
| 44 | 4 | endpoint charge | `0` |
| 48 | 4 | initial binding count | `4` |
| 52 | 4 | flags | zero |
| 56 | 8 | reserved | zero |

All integers are unsigned. The decoder first requires the exact outer length,
then validates magic, version, encoded length, reserved fields, role, nonzero
references, exact resources, and exact binding profile in that order. It never
reads beyond the 64-byte value. A structurally invalid request produces a
rejected launch transition with plan reference zero; a structurally valid
request is passed to the typed policy, which still validates generations and
authorization.

## Evidence and limits

[`Application-Start-Request.wv`](../Operating-System/Kernel/Application-Start-Request.wv)
builds as a 6,555-byte WVB at SHA-256
`1c30a368dbe8a1f233f652fb9211d8f85273fdc09716ec2559fd5b3b1c91f90a`.
The focused launch owner now covers 20 cases, including every structural status,
successful typed handoff, stale executable rejection, and a zero-reference
malformed result.

The implemented [user-copy policy](Windvale-Os-Application-Start-User-Copy.md)
now copies the complete request into an immutable value, checks an admitted
window with subtraction-first arithmetic, and compares the encoded caller with
an independently supplied current-caller identity before calling this decoder.
The x86-64 internal copy leaf enforces one exact admitted page, copies the
request into a kernel-owned snapshot, validates every version-1 field and the
independently supplied caller, and erases rejected copied bytes. Its internal
syscall-context adapter now accepts separate machine process id/generation and
current-page inputs and derives reference `65537`; request bytes cannot select
them. The retained-machine cutover must still select the public
number/registers, load that context from the current process, stabilize live
mappings, enforce its budget, and define completion, cancellation, and fault
behavior. Service launch
requires a successor request version with named filesystem/network executable,
endpoint, capability, and resource profiles; version 1 rejects those roles.
