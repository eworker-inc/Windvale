# Windvale native owned-console application

## Status and scope

Candidate implementation contract under
[Decision 0975](../Documents/Decisions/0975-Launch-Owned-Storage-Console-Applications.md).
Console format three runs a capability-free Core ABI-24 native `Main` returning i32 with
context 10 and the existing fixed 64-slot owned-storage/budgeted adapter on
Windows x64 and Linux x64. It supplies scalar Vector storage and deterministic
domain teardown. It does not supply hosted services or historical byte/text
arenas, and it does not establish shared immutable storage, installed delivery,
independent tool reconstruction or complete 1.0 qualification.

The compiler's owned entry supplies the root budget to the supported source
`Main(Budget: Memoryˉbudget) -> i32`; that source parameter is distinct from the
native context-pointer calling convention.

## Construction versions and bounds

The existing [planner](Windvale-Console-Application-Plan.md),
[constructor](Windvale-Console-Application-Construction.md) and
[verifier](Windvale-Console-Application-Verification.md) extend their existing
owners. WVCQ request version two is exactly 32 bytes with the existing fields:
target 1 selects `windows-x64-console-v3`, and target 2 selects
`linux-x64-console-v3`. WVCP response version two remains exactly 108 bytes;
WVCC recipe version two retains the 40-byte header and 12-byte descriptors.
All integer fields remain unsigned little-endian with the earlier validation
order, refusal statuses and zero reserved fields. Known version-two failures
retain version two; truncated or unknown-version failures use version one.
WVCV version-one evidence continues to report platform target 1 or 2. Container
bytes, rather than that platform-only evidence, identify the startup profile.

Native images remain 1 through 4,194,304 bytes, and their entry offset must be
within the image. The ordinary contiguous packager limits its complete result
to 4,194,304 bytes. Structural admission retains the earlier complete-container
limits of 4,196,352 bytes on Windows and 4,202,608 bytes on Linux. It does not
interpret or sandbox the opaque native image. Format-one requests and recipes
retain their exact bytes. Hosted console format two retains its separate
contract and admission path.

## Canonical layout

The existing PE32+ writer retains its 512-byte header, text file offset 512,
text virtual address 4,096, 512-byte file alignment, 4-KiB virtual alignment,
512-byte initialized data block and 512-byte relocation block containing 12
virtual bytes. Header byte 154 is 3. Startup is 143 bytes; the native image
begins at text offset 144. Its call displacement is at startup offset 99,
with entry offset equal to that displacement minus 41.

The existing ELF64 writer retains its 4-KiB header/text placement and load
alignment, read-only headers, executable non-writable text, writable
non-executable data, 112 initialized data bytes, version note at offset 384 and
non-executable stack declaration. Note word 408 is 3. Startup is 206 bytes;
the native image begins at text offset 208. Its call displacement is at startup
offset 160, with entry offset equal to that displacement minus 44. Linux startup
maps the existing bounded 64-MiB stack and reserves 32 caller-shadow bytes before
calling the shared entry. Windows retains the existing stack profile.

Both data mappings are exactly 16,783,296 bytes. Relative to their data base:

| Offset | Extent | Purpose |
| ---: | ---: | --- |
| 0 | 112 | Fresh-domain version-one request. |
| 112 | 136 | Context 10. |
| 248 | 8 | Zero alignment padding. |
| 256 | 2,112 | Physical owned-storage state. |
| 2,368 | 2,616 | Canonical budget state. |
| 4,984 | 8 | Zero alignment padding. |
| 4,992 | 1,088 | Budgeted storage adapter. |
| 6,080 | 16,777,216 | Owned arena. |

Only the request's non-pointer fields are initialized in the file. All other
bytes are zero-filled by the container. Startup derives the request, context,
physical state, accounting, adapter and arena addresses with checked canonical
PC-relative placements, then fills request pointer fields 16 through 48.
The request uses arena capacity and root maximum 16,777,216, epoch 1, maximum
64 children, fuel 100,000,000 and depth 1,024. Reserved fields are zero. These
are finite launcher-profile limits, not portable language limits. Epoch 1 is
fresh because each process owns exactly one invocation and no owners escape it.

## Shared entry and failure behavior

`Windvale_owned_console_entry` receives the complete immutable request in RDX
and invokes the [fresh-domain constructor](Windvale-Native-Owned-Domain.md).
Initialization refusal returns packed status 9 without invoking Main. Success
passes the initialized context to the linked `Native_main`. Generated ABI-24
Main performs its normal and trapped terminal teardown; the outer owner closes
an otherwise-open domain after return. It verifies the physical closed flag,
zero live allocation/charge state, canonical accounting validity and no active
budget/lease record before returning Main's packed result.

Cleanup failure preserves an earlier nonzero language trap; otherwise it returns
packed status 9. Platform startup maps any nonzero packed status or successful
i32 result outside 0 through 255 to process exit 1. A successful result in that
range becomes the process exit code. Neither startup acquires hosted capabilities
or changes the source ownership rules.

`Package-Console.cmd/.sh --owned <windows|linux> <abi-24-main.wvo> <output>`
links the shared entry and existing runtime leaves after renaming Main to
Native_main. `Package-Console.mjs --current <target> <image.bin> <entry> <output>`
also exposes current-source raw construction. Both require a prepared current
compiler, build the existing source packager/publisher through complete-input
caches, construct a private candidate, recheck immutable inputs and use the
existing native publication transaction. Refusal leaves the destination under
that transaction's exact progress/indeterminate-completion contract; an
indeterminate mutation must be inspected before retrying.

The object caller must supply the documented ABI-24 Main contract. WVO's
structural validation and export renaming do not prove that machine code follows
that ABI. Container admission treats the native image as opaque and proves
layout and bounded entry placement, not executable semantics; source compiler
and lowering evidence remain separate.

The existing source-reconstruction owner has separate `--prepare-only` and
`--prepared-products-only` selections. Preparation receives a finite deadline,
retains completed input-bound compiler/tool caches and executes no behavior
cases. Prepared behavior refuses a missing compiler, native tool or console
publisher checkpoint before reconstruction; it still constructs and executes
the selected application fixtures. Automatic development uses the existing
preparation job rather than hiding cold construction inside the 600-second
behavior owner.
