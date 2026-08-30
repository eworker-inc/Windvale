# Windvale Language 1.0 target descriptor

## Status

This document defines the implemented `WVTD 1.0` construction and validation
checkpoint accepted by [Decision 0886](../Documents/Decisions/0886-Make-Target-And-Foreign-Admission-A-Mandatory-Language-1.0-Phase.md).
`Compiler/Windvale/Source-Target-Core.wv` owns the current registry constants,
canonical constructor, structural validator, semantic validator, and source
platform predicates.

The portable authenticated source-admission coordinator now carries exact WVTD
input/output through in-memory `wvadmit` construction and enforces target and
foreign predicates before WVAE construction. The target-aware hosted `wvadmit`
publishes that exact descriptor, and the complete `wvauth` independently proves
the retained source/catalog target predicate without publishing a certificate.
The proposed production-ingress candidate now retains and rechecks that private
snapshot through the non-authoritative Analyzer handoff. Exact local Windows
product pins and the complete focused owner pass. Paired-host acceptance,
foreign-call WIR/WVB and native lowering, linker imports, and provider
containment remain pending. WVTD is an internal compiler-phase format, not a
package or distribution format. Canonical WVB remains the distribution
contract.

## Encoding and limits

`WVTD 1.0` is one concrete caller-selected build target. All multibyte values
are unsigned little-endian integers. Its complete length is from 64 through 320
bytes. The format contains one fixed 64-byte header and zero, one, or two
contiguous identity directories. No padding, trailing data, alternate ordering,
or omitted header word is permitted.

| Offset | Bytes | Field | Version-1 requirement |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVTD`, integer `1146377815` |
| 4 | 2 | major version | `1` |
| 6 | 2 | minor version | `0` |
| 8 | 4 | complete byte length | exactly the supplied value length |
| 12 | 4 | concrete build-target identity | registered and cross-field exact |
| 16 | 4 | environment identity | registered |
| 20 | 4 | architecture identity | registered |
| 24 | 4 | foreign ABI identity | registered |
| 28 | 4 | address width in bits | `64` for every accepted version-1 target |
| 32 | 4 | byte-order identity | registered and target-exact |
| 36 | 4 | no-unwind C scalar/pointer interface major | target-exact |
| 40 | 4 | extension-identity count | at most `32` |
| 44 | 4 | target-interface-identity count | at most `32` |
| 48 | 4 | extension directory offset | `0` when empty; otherwise `64` |
| 52 | 4 | target-interface directory offset | `0` when empty; otherwise immediately after the extension directory |
| 56 | 4 | flags | zero |
| 60 | 4 | reserved | zero |

Each directory entry is one four-byte registry identity. Entries in each
directory are strictly increasing and therefore nonzero, ordered, and unique.
The complete length is exactly
`64 + 4 * (extension count + target-interface count)`. Counts are checked
before multiplication, so this expression cannot exceed 320 or overflow the
implemented `u32` calculation.

The extension directory, when nonempty, begins at 64. The target-interface
directory, when nonempty, begins at `64 + 4 * extension count`, including when
the extension count is zero. Empty directories always use offset zero; an
offset into empty space is noncanonical.

WVTD 1.0 currently registers no extension or target-interface identities.
Consequently, a value from 68 through 320 bytes can be structurally canonical
but cannot yet pass semantic validation. Every semantically accepted current
WVTD is the canonical 64-byte empty-directory form. The 320-byte shape remains
an enforced structural boundary rather than a promise that 64 optional
identities already exist.

## Version-1 registries

Zero is unregistered in every registry. A known identity is not necessarily a
supported target combination: AArch64 and big endian have stable identities so
they can be diagnosed precisely, but WVTD 1.0 publishes no concrete target that
uses either one.

### Concrete build targets

| Identity | Canonical registry name | Environment | Architecture | ABI | Address bits | Byte order | No-unwind major |
| ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | `windows.x86_64.none_v1` | 1 | 1 | 1 | 64 | 1 | 0 |
| 2 | `linux.x86_64.none_v1` | 2 | 1 | 1 | 64 | 1 | 0 |
| 3 | `windvale.x86_64.none_v1` | 3 | 1 | 1 | 64 | 1 | 0 |
| 4 | `linux.x86_64.sysv_amd64_c_v1` | 2 | 1 | 2 | 64 | 1 | 1 |

ABI identity `1` means that this source/build requires no concrete foreign ABI.
It does not erase the target's machine ABI, make the target portable, or allow
a foreign declaration. Build target 4 is the only version-1 target admitted for
the first System foreign declaration.

### Component registries

| Registry | Identity | Name |
| --- | ---: | --- |
| environment | 1 | `windows` |
| environment | 2 | `linux` |
| environment | 3 | `windvale` |
| architecture | 1 | `x86_64` |
| architecture | 2 | `aarch64` |
| foreign ABI | 1 | `none` |
| foreign ABI | 2 | `sysv_amd64_c_v1` |
| byte order | 1 | `little` |
| byte order | 2 | `big` |

### Textual source-platform predicates

Source platform predicate identities are separate from concrete build-target
identities. Their masks are an in-memory finite parser representation and are
not serialized in WVTD.

| Predicate identity | Source spelling | Mask | Match over a validated WVTD |
| ---: | --- | ---: | --- |
| 1 | `windows` | 1 | environment identity 1 |
| 2 | `linux` | 2 | environment identity 2 |
| 3 | `windvale` | 4 | environment identity 3 |
| 4 | `linux.x86_64.sysv_amd64_c_v1` | 8 | concrete build-target identity 4 |

The known mask is exactly `15`. A source platform list contains at most 32
declared alternatives, rejects unknown or duplicate evidence, and admits when
at least one registered predicate matches. The concrete foreign declaration
requires predicate identity 4 alone; generic `linux` is insufficient.

The exact predicate must be the only alternative for a module with foreign
declarations. A list containing generic `linux` and the exact ABI predicate is
broader than the foreign contract and rejects with
`FOREIGN_REQUIRES_CONCRETE_PLATFORM`; it is not normalized to the exact member.
Unknown spelling retains parser status `Unknownˉplatform` and maps to target
status `UNKNOWN_PLATFORM`.

Durable foreign-catalog readback reproduces canonical source-ordered WVFC from
admitted WVSS before reporting target success. Stable retained geometry is
12,583,216 bytes: supplied WVFC retains the general 4,194,304-byte format
ceiling and reproduced canonical WVFC is at most 4,194,288 bytes. Conservative
simultaneously-live large immutable payload retention is 16,777,456 bytes
including the bounded 4,194,240-byte record accumulator. This is not a process
working-set bound; comparison is one bounded linear scan. Producer failures
collapse to outer target-admission status `WVFC` while preserving subordinate
source-set, parser, catalog, module, and offset evidence.

## Construction

The canonical constructor consumes one typed target descriptor plus raw
extension and target-interface identity directories. It performs these checks
without invoking either serialized-input validator:

1. every descriptor identity is registered;
2. the complete descriptor exactly matches its concrete build-target row;
3. each directory length is divisible by four and at most 128 bytes;
4. each directory is strictly increasing; and
5. every directory identity is registered for WVTD 1.0.

Only after all checks pass does the constructor allocate the canonical header
and append the exact directory bytes. Rejection returns no partial candidate.
The current empty optional registries mean a successful constructor takes two
empty directory values and emits exactly 64 bytes.

## Independent validation

Construction is not trusted as validation. The structural validator consumes
untrusted bytes and checks, in order:

1. the 64..320 outer bound;
2. magic and exact major/minor version;
3. declared versus actual length;
4. zero flags and reserved word;
5. both counts against 32;
6. exact length derived from the checked counts;
7. both canonical offsets; and
8. strict order in each bounded directory.

The semantic validator first requires structural success, then independently
reads the descriptor and directories. It rejects the first unknown descriptor
identity, rejects a known but unsupported cross-field combination, and rejects
the first unknown extension or target-interface identity. It does not infer the
host, fill omitted fields, rewrite an unsupported target, treat a WVB emission
mode as a target, or trust a digest or path as semantic evidence.

Successful structural and semantic validation returns the accepted input bytes
unchanged and byte-for-byte equal to the caller's value. Rejection returns an
empty byte value. Validation never returns a normalized or repaired WVTD.

## Status and failure offsets

The result carries a fixed `u32` status, a `u32` failure offset, and bytes.
Status zero has failure offset zero. A rejection's failure offset names the
header word or the exact offending directory identity; the returned bytes are
empty.

| Status | Name | Meaning and failure offset |
| ---: | --- | --- |
| 0 | `VALID` | accepted; offset 0; exact input or completed construction |
| 1 | `INVALID_DESCRIPTOR` | typed source-platform admission received an invalid descriptor |
| 2 | `INVALID_PLATFORM_EVIDENCE` | malformed count/mask evidence for textual source scopes |
| 3 | `UNKNOWN_PLATFORM` | authenticated source scope contains an unknown spelling |
| 4 | `UNSUPPORTED_TARGET` | no declared source predicate matches the valid target |
| 5 | `FOREIGN_REQUIRES_CONCRETE_PLATFORM` | foreign source lacks the exact predicate/target pair |
| 6 | `WVTD_INVALID_INPUT` | constructor directory byte shape; offset 40 or 44 |
| 7 | `WVTD_INVALID_SIZE` | outer/declared/derived length; offset 8 |
| 8 | `WVTD_INVALID_MAGIC` | offset 0 |
| 9 | `WVTD_INVALID_VERSION` | offset 4 for an unknown major or minor |
| 10 | `WVTD_INVALID_FLAGS` | offset 56 |
| 11 | `WVTD_INVALID_RESERVED` | offset 60 |
| 12 | `WVTD_INVALID_COUNT` | offset 40 or 44 |
| 13 | `WVTD_INVALID_OFFSET` | offset 48 or 52 |
| 14 | `WVTD_INVALID_ORDER` | exact duplicate, zero, or descending directory word |
| 15 | `WVTD_UNKNOWN_IDENTITY` | exact descriptor field or directory word |
| 16 | `WVTD_TARGET_MISMATCH` | known fields do not equal the build-target row; offset 12 |

For descriptor identities, unknown-identity offsets are 12 for build target, 16
for environment, 20 for architecture, 24 for ABI, and 32 for byte order.
Address width and the no-unwind major are scalars governed by the concrete
target row; a mismatch reports status 16 at offset 12.

## Security, determinism, and nonclaims

WVTD carries target evidence, not authority. It contains no host state, path,
library handle, native address, capability grant, symbol lookup, pointer,
runtime service, cache identity, source digest, or permission. Validation does
not prove that source is well typed or that foreign code is safe to call.

The constructor and validators are bounded by 320 input bytes, two counts of at
most 32, and linear scans of at most 64 directory words. They allocate no
unbounded collection and perform no host I/O. Identical inputs produce
byte-identical results on every host implementing the same version-1 registry.

This checkpoint makes no AArch64, big-endian, extension, target-interface,
cross-host qualification, product-driver, cache-migration, or compatibility
claim. Adding a new supported target requires a new stable concrete identity
and exact cross-field row. Reusing an identity or reserved field is forbidden;
a shape that cannot be represented by these fields requires a new WVTD version.
