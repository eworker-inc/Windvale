# Windvale linking contract

## Status and purpose

Windvale Linking version 1 defines the first deterministic multi-object link contract and the `flat-x86-64-v1` output target. The C# implementation under `Linker/Reference/` is the Stage 0 oracle and is cross-host qualified at `9c4b9f5`: the exact committed archive passed the 31-test suite and real CLI flow on Windows and Debian, with identical input objects, complete image bytes, canonical map bytes, and normalized conformance contracts. The complete Windvale-written implementation is owned by `Linker/Windvale/`. Decision 0160 adds the Stage 0 `flat-x86-64-large-v1` admission target as a locally verified candidate; it is not yet part of that cross-host-qualified portable implementation.

The flat target is a raw memory image, not an executable container. It proves input validation, global symbol resolution, address-aware alignment, checked relocation arithmetic, zero-fill materialization, independent output verification, and canonical map evidence without importing PE, ELF, UEFI, loader, ABI, or operating-system policy. The assembler continues to own WVA parsing and WVO construction; the linker consumes only complete verified WVO objects. A separate narrow UEFI target adapter now consumes a successful flat result under [Windvale-Uefi-Application.md](Windvale-Uefi-Application.md); it does not change this flat-image contract.

This is an early-development contract without a backward-compatibility promise. Unsupported or obsolete inputs must be rejected rather than guessed or migrated.

## Inputs and options

A link request contains:

- one through 64 WVO 1.0 x86-64 objects as immutable byte values in explicit semantic order;
- one `u32` base address;
- one required entry-symbol machine name.
- one explicit admission profile, defaulting to standard.

Every object is decoded and independently verified before resolution or layout. Host paths, timestamps, file order, locale, and ambient process state are not portable inputs. The caller-supplied object order is meaningful and is preserved in input indices and contribution order; the linker never enumerates or sorts host directories.

The aggregate link is limited to 256 sections, 16,384 symbols, and 65,536 relocations. Counts are checked while inputs are loaded so a hostile collection cannot bypass the link-wide limits even when each object is independently valid.

Standard admission retains the qualified contract: each WVO is at most 4 MiB, aggregate input bytes are bounded by 64 such objects, the image is at most 4 MiB, and the map target is `flat-x86-64-v1`. Large-native admission must be requested explicitly: each WVO is at most 32 MiB, aggregate encoded input is at most 32 MiB, the image is at most 32 MiB, and the map target is `flat-x86-64-large-v1`. The same loader, resolver, layout engine, relocation rules, independent verifier, and map writer serve both profiles. A large WVO never selects the larger profile by itself.

## Symbol resolution

Local symbols are visible only to relocations in their defining object. Export names form one ordinal global namespace. More than one export with the same name is an error, regardless of whether an import references it.

Every import must resolve to exactly one export with the same ordinal machine name and symbol kind. Unused imports are still unresolved contract obligations and therefore must resolve. An import never falls back to a host library, dynamic loader, ambient symbol, or local symbol in another object.

The requested entry name must identify one exported function. The raw image does not embed the entry address; the canonical map and the link result carry it explicitly for a later image adapter or loader.

## Flat-image layout

The image represents bytes beginning at the requested base address. Section contributions are placed in this exact order:

1. WVO section kind: code, read-only data, writable data, then zero-fill;
2. input-object index;
3. source section index within that object.

Before each contribution, the linker aligns the actual address `base address + current image offset` to the section alignment. Alignment gaps contain zero bytes. Materialized sections copy their exact WVO data. Zero-fill sections contribute their declared memory size as zero bytes. Empty sections retain a canonical aligned placement even when they add no bytes.

The complete image is limited to 4 MiB under standard admission and 32 MiB under large-native admission. Every section start, symbol address, and relocation-field address must fit `u32`; the one-past final byte may equal `2^32`. Layout arithmetic is checked and never wraps.

Grouping by kind makes the flat memory policy visible while preserving semantic input order within a kind. Version 1 does not merge same-named sections, discard unused contributions, coalesce constants, reorder by symbol, or insert target-specific headers.

## Relocation application

The linker begins with the independently verified WVO requirement that every four-byte relocation field contains zero. For final target-symbol address `S`, relocation-field address `P`, and signed addend `A`:

```text
absolute-u32 = S + A
relative-i32 = S + A - P
```

`absolute-u32` must fit the full unsigned 32-bit range. `relative-i32` must fit the full signed 32-bit range. The exact little-endian result replaces the four-byte placeholder. Negative absolute results, oversized absolute results, and relative overflow are errors; relocation values never truncate or wrap.

Relocations remain in input-object order and source relocation order for map evidence. A relocation targeting a local or exported definition uses that exact definition. A relocation targeting an import uses the resolved export.

## Independent image verification

The Stage 0 linker does not publish bytes immediately after its layout pass. A separately implemented flat-image verifier reconstructs canonical section order and alignment, all defined-symbol addresses, export uniqueness, import resolutions, entry selection, original section bytes, zero padding, BSS bytes, and every relocation value from the verified inputs. It compares the complete reconstructed image with the candidate. Verification failure returns `WVL1011` and no output bytes.

This verifier is an implementation oracle, not a substitute for later target-specific executable or boot-image validation.

## Canonical map version 1

The map is strict UTF-8 containing only ASCII and LF. It ends with one LF and is limited to 1 MiB. Decimal integers use invariant base 10 with no grouping or padding. Names already satisfy the WVO machine-name grammar and require no quoting. The map contains no paths or timestamps.

Record order is fixed:

```text
windvale-link-map 1
target name=<flat-x86-64-v1|flat-x86-64-large-v1> architecture=x86-64 base-address=<u32> image-bytes=<u32>
entry name=<name> address=<u32>
image sha256=<lowercase-hex>
inputs count=<u32>
input index=<u32> sha256=<lowercase-hex>
...
sections count=<u32>
section index=<u32> input=<u32> source-index=<u32> kind=<kind> name=<name> image-offset=<u32> address=<u32> memory-bytes=<u32> data-bytes=<u32> alignment=<u32>
...
defined-symbols count=<u32>
symbol index=<u32> input=<u32> source-index=<u32> binding=<local|export> kind=<function|data> name=<name> address=<u32> size=<u32>
...
imports count=<u32>
import index=<u32> input=<u32> source-index=<u32> kind=<function|data> name=<name> provider-input=<u32> provider-source-index=<u32> address=<u32>
...
relocations count=<u32>
relocation index=<u32> input=<u32> source-index=<u32> kind=<absolute-u32|relative-i32> patch-offset=<u32> patch-address=<u32> target=<name> target-input=<u32> target-source-index=<u32> target-address=<u32> addend=<i32> value=<i64-range-decimal>
...
```

Defined symbols and imports use input order and source symbol order. Section records use final layout order. The map digest is itself part of cross-host conformance evidence.

## Diagnostics

Link failure produces no image or map bytes. The first deterministic failure is reported:

| Code | Meaning |
| --- | --- |
| `WVL1001` | Invalid request, object count, entry name, admission profile, or CLI contract |
| `WVL1002` | Uninitialized, malformed, unsupported, or unverifiable input object |
| `WVL1003` | Aggregate input-byte, section, symbol, or relocation limit exceeded |
| `WVL1004` | Duplicate exported symbol |
| `WVL1005` | Undefined imported symbol |
| `WVL1006` | Import/export symbol-kind mismatch |
| `WVL1007` | Entry is missing, not exported, or not a function |
| `WVL1008` | Image-size, alignment, or `u32` address-space overflow |
| `WVL1009` | `absolute-u32` relocation overflow |
| `WVL1010` | `relative-i32` relocation overflow |
| `WVL1011` | Independent flat-image verification failure |
| `WVL1012` | Canonical map exceeds its byte limit |

An input-specific diagnostic carries its zero-based input index. Global request and aggregate failures use no input index.

## Hosted CLI boundary

The standard-profile Stage 0 CLI form is deliberately explicit:

```text
windvale link --base-address <u32> --entry <export> -o <image.bin> <object.wvo>...
```

The CLI requires exact `.wvo` inputs and a distinct `.bin` output. It reads bounded object bytes, completes link validation, image construction, independent verification, and map construction in memory, and only then writes the image once. On success, standard output is exactly the canonical map. Link failures write diagnostics and leave a missing output absent or an existing output unchanged. Native file failures remain host-boundary diagnostics.

The large-native profile is currently an internal Stage 0 API used to establish the exact compiler transport boundary. It is not ambient CLI policy. The qualified Windvale-written linker remains on standard admission because an ordinary Windvale `bytes` value remains limited to 4 MiB; transferring large-link ownership requires a later bounded segmented or sparse input path.

## Segmented compiler-WVO flat-image candidate

The first bounded large-native transfer profile is deliberately narrower than
the general multi-object linker. It accepts one base-zero, compiler-produced
WVO represented by a strict `WVOP 1` manifest and its separately retained
chunks. The existing compiler-WVO envelope, symbol, relocation, placeholder,
and padding validators must admit the same immutable metadata snapshots before
link planning succeeds. The profile permits only the compiler's `.text` and
optional `.rodata` sections, local data and helper symbols, one exported
`Main`, no imports, and ordered `relative-i32` relocations with addend `-4`.

The plan exposes the exact flat-image length, `Main` entry offset, manifest and
output chunk counts, text-chunk count, and relocation count. At base zero the
image is exactly `.text` followed by `.rodata`; the compiler's text padding
already establishes the required 16-byte read-only-data alignment. For a
relocation at text offset `P` targeting a data symbol at read-only offset `D`,
the replacement bits are:

```text
text-bytes + D - P - 4
```

Each actual manifest chunk is processed separately. WVO header, section
header, symbol, and relocation chunks must match their admitted snapshots and
produce no image bytes. Text chunks must pass the existing per-chunk
placeholder/padding verifier; their owned relocation fields are replaced while
the rest of the chunk is copied exactly. Read-only-data chunks are copied at
their derived image positions. Every input and output value remains within the
ordinary 4 MiB value ceiling, while the complete planned image remains within
the explicit 64 MiB segmented-compiler ceiling.

`Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image-Verification.wv` is the
separate output verifier for this candidate. Its scalar cursor revalidates the
same immutable WVO snapshots, consumes every manifest chunk in order, requires
contiguous candidate image positions, compares unchanged text and read-only
data, checks relocation fields in reverse record order, and reaches `Complete`
only after the exact planned image extent. It does not call the producer linker
module or accept producer plan evidence.

This candidate does not generalize standard linking, accept multiple objects
or imports, emit a canonical map, or publish a public host resource. The
hosted immutable-snapshot staging boundary below now owns one private staged
resource set; durable public publication remains separate.

### `WVLI 1.0` linked-image staging manifest

The bounded producer records each nonempty image chunk in one strict
little-endian manifest written only after every chunk has been staged:

| Offset | Size | Meaning |
| ---: | ---: | --- |
| 0 | 4 | ASCII `WVLI` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Total manifest bytes, exactly `28 + chunk-count * 12` |
| 12 | 4 | Complete flat-image bytes, one through 64 MiB |
| 16 | 4 | `Main` entry offset, strictly inside the image |
| 20 | 4 | Chunk count, one through 518 |
| 24 | 4 | Maximum chunk bytes, exactly `4,194,304` |

Each following 12-byte entry contains its exact `u32` chunk index, image
position, and nonzero length. Indices equal their ordinals, the first position
is zero, positions are contiguous, every length is within the ordinary 4 MiB
value ceiling and remaining image extent, and the final extent equals the
declared image length. Rejection exposes zero size/count evidence. `WVLI` is a
structural completion marker, not a content digest, capability, durable commit
record, canonical link map, or executable-container manifest.

### Hosted immutable-snapshot staging boundary

The first hosted Windvale owner has this exact command shape:

```text
wvlink-stage <wvo-chunk-prefix> <wvo-manifest.wvop> <image-chunk-prefix> <image-manifest.wvli>
```

Manifest resource names are nonempty and at most 4,095 UTF-8 bytes. Chunk
prefixes are nonempty and at most 4,078 UTF-8 bytes so appending
`.chunk-<canonical-u32-decimal>` remains within the same resource-name limit.
The two manifest names must differ, and the two chunk prefixes must differ.
Every possible source and output chunk name is checked against both manifest
names before chunk acquisition or mutation.

The tool reads the source manifest first and admits at most 62 source chunks.
It then calls `file.read_bytes` for every canonical source chunk in index order
and checks each exact manifest length. These 63 distinct names fit the native
64-snapshot input table. All later metadata discovery, linking, and
verification reads use those same exact names and therefore receive the
execution-owned immutable snapshots rather than reopening mutable resources.

The tool locates the validated optional read-only header, symbol chunk, and
relocation chunk from strict manifest positions. It builds the segmented plan,
starts the independent verifier, and processes every source chunk in order. A
nonempty linked candidate is written only after the independent cursor accepts
its source, position, length, unchanged bytes, and relocation fields. The tool
records each accepted output position and length, requires complete source and
image coverage, builds a strict `WVLI 1.0` value, and writes that manifest last.

This is private staging, not durable publication. Exact resource-name
separation does not prove Windows file identity or Linux device/inode
separation; a failed run may leave output chunks without a completion manifest;
and the boundary does not flush, reread, rename, clean stale resources, emit a
digest, or replace a public destination. A later fixed publisher must bind
native identities and perform the existing sibling/reread/atomic-replacement
transaction before exposing the image.

Digest-bound application candidates expose this exact hosted root as
`windows-x64-compiler-image-staging-v1` and
`linux-x64-compiler-image-staging-v1`. The generated Windvale fragment calls
nine services: console output, process argument count/value, file input,
diagnostic output, enum naming, text concatenation, unsigned formatting, and
file output. The existing hosted-container layout additionally carries its
canonical UTF-8 adapter slot as infrastructure; generated code does not call or
gain another capability from that slot.

The exact 75,666-byte tool WVB has SHA-256
`b2460134db0bd9810d9d20415ba5cf1807b24a0df71b42e5955cd40851481b46`.
Its canonical repository source closure is
`Projects/Linker/Windvale-Compiler-Image-Staging.wvproj`; the ordinary native source front door
publishes that exact identity byte for byte. The project retains dependencies
in canonical ordinal module-name order until the native project driver derives
the order itself. Stage 0 is the differential/recovery oracle for this WVB,
not its normal constructor.

The natively built WVB also crosses the current-host segmented native
reconstruction path. The bounded native producer writes a strict `WVOP` and no
more than 62 chunks; this staging application consumes those immutable
snapshots and writes a strict `WVLI`. The joined WVO, flat image, and `Main`
entry match the independent Stage 0 recovery oracles byte for byte, and neither
native child loads a managed runtime. The application containers used for this
evidence now reconstruct through the current-Windows-host native cross-target
path recorded by Decision 0496. That path consumes the retained candidate
toolset as its seed; it is self-reconstruction evidence rather than a
non-circular bootstrap or paired-host qualification.

The Windows candidate is 854,016 bytes at SHA-256
`ed87716ae3ae805d38bfbe5fcfeee1a74c983f82416e5d36a9695085747de3fe`;
the Linux candidate is 856,064 bytes at SHA-256
`8d94c36a58da696caa09d5323c826936373f48d538f404fdc01df3bea9c4d379`.
Both containers have independent structural verification. Current-host Windows
execution stages the complete small fixture without loading a CLR component;
Linux execution remains a separate qualification item. The completed process
report includes the exact decimal image size, `Main` entry offset, output chunk
count, and manifest size so an orchestrator never needs to decode `WVLI`.
The former Stage 0 application writer remains recovery and differential
evidence; it is no longer the only constructor for these exact candidates.

### Canonical compiler-image transport

`Linker/Windvale/Compiler-Image-Canonical-Transport-Tool.wv` converts a strict
`WVLI 1.0` sequence of semantic linker chunks into the canonical fragment
geometry consumed by hosted packaging. Its four arguments are the source chunk
prefix, source `WVLI`, output chunk prefix, and output `WVLI`.

Before reading any chunk, the portable resource plan validates both manifests
and prefixes, rejects every control/chunk collision, admits one through 62
source chunks, and derives one through sixteen output chunks from the declared
image length. The process reads every source chunk once in manifest order and
checks its exact length. It preserves the byte sequence while emitting every
non-final output at exactly 4,194,304 bytes and one nonempty final remainder.
The image length and entry offset remain unchanged. A newly validated `WVLI` is
written last; incomplete output chunks are not completion evidence.

The exact transport WVB is 23,836 bytes at SHA-256
`d4bdfa7588e4431432a300e0da257507d73846931f5dd1296855b03714d218c8`.
The Windows application is 269,312 bytes at SHA-256
`e724a5efbffc233fda76f55bfb5cc01c044e221882b5de5f247b0ab236726f81`;
the Linux application is 270,336 bytes at SHA-256
`9ff5401eca1ffd93a49077dd6ebc56c446c59939379a481f22662465fc3cf6db`.
Both applications reconstruct through Decision 0496's current-Windows-host
native cross-target path. The checked-in native processes and digest-bound
launchers remain the normal candidate front door; Linux execution and
independent-host reconstruction remain open.

### Segmented compiler hosted-package composition

`Tools/Native/Package-Segmented-Compiler-Wvb.cmd` and `.sh` compose the pinned
segmented WVO producer, segmented image linker, canonical image transport, and
hosted-container toolset. The launcher owns only process orchestration and
parsing of the transport process's strict decimal completion line.

The segmented producer WVB is exactly 542,219 bytes at SHA-256
`4507a1497bc5805e98d66efbef6277a42763107b84d10e7e915eb11ba2b92738`.
Its Windows application is 7,855,104 bytes at SHA-256
`ec4d0a6f0c4670c16abe2615d6668f7a52266182d18ddc31ffb8162a8872ecda`;
its Linux application is 7,856,128 bytes at SHA-256
`8522d10ba8005d82dd206f2766d54f2d6f7661242de6346b14b9a396ad1f3401`.
Decisions 0496, 0542, 0545, 0583, 0713, 0724, and 0770 reconstruct this pair and the staging and transport pairs from
the retained native candidate toolset on Windows. That construction does not
by itself prove Stage 2, Linux execution, promotion, or a seed-independent
bootstrap.

The shared `Package-Hosted-Wvb` launcher exposes a separate image-input form:
`image <profile> <input.wvb> <chunk-prefix> <fragment-count> <entry-offset>
<output> [target]`. Its ordinary form accepts the same optional final
`windows` or `linux` target; omission retains current-host selection. It admits
profile 1 through 7, one through sixteen canonical chunks, and a decimal entry.
The native source-geometry process independently reopens
and validates exact chunk lengths and the canonical non-final 4 MiB rule before
metadata or container construction. The source WVB remains the capability and
enum evidence owner; the supplied image and entry remain the generated-code
owner.

Image-input mode reuses the exact fixed-service, evidence, metadata, runtime,
layout, bundle, source-set, segment, manifest, and durable publication chain of
ordinary hosted packaging. It does not lower or link again, and it has no
fallback to Stage 0 or the whole-value lowerer.

### Segmented hosted storage overlay

Large database verification applications use the narrow composition contract
in `Specifications/Windvale-Segmented-Hosted-Overlay.md`. The already verified
compiler image remains unchanged. The ordinary Wv linker validates and lays
out the common provider, selected host provider, and one exact `Main`
trampoline after that image; composition patches only the trampoline's signed
relative jump back to the compiler image entry. The provider must fit in the
unused tail of the last canonical fragment, so the existing one-through-sixteen
fragment and 64 MiB limits remain unchanged.

This boundary supplies hosted storage code, not storage authority. The hosted
container still binds the provider table and its one rights-limited storage
object under the existing profile contract.

## Deliberate omissions

The flat target has no PE, ELF, UEFI, archive/library search, dynamic linking, weak symbols, COMDAT selection, dead stripping, section merging, executable permissions, debug data, stack/heap declaration, ABI, start-up code, 64-bit absolute relocation, internal-label model, or loader metadata. A raw flat image is not directly executed by Windows or Linux. The UEFI application adapter and capability-free `windows-x64-console-v1` and `linux-x64-console-v1` adapters are downstream targets with their own narrower input and verification rules.

Add target adapters and new relocation or section concepts only when the native backend or VM boot path supplies a concrete case and an independent verification rule.
