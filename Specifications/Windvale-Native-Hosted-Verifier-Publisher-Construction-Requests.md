# Windvale native hosted-verifier publisher construction requests

## Status and scope

This contract transfers the exact publisher-module identity, native symbol
discovery, five WVO asset identities, target layout, and ordered relocation
targets from the frozen C# publisher builder into focused Windvale modules.
Downstream native stages now consume these records to instantiate the objects
and materialize both publisher and promoter PE/ELF applications.

The boundary is split into four immutable records because combining SHA-256,
object inspection, layout, and all target bindings exceeds the current
4,096-binding source-WIR ceiling. The split is architectural evidence, not
numbered source fragmentation: each stage owns one independent invariant.

## `WVPI 1`: exact identity envelope

`WVPI 1` begins with a 64-byte little-endian header. Offsets 0, 4, 8, and 12
contain magic `WVPI` (`0x49505657`), version 1, total bytes, and target 1
(Windows) or 2 (Linux). Six `(offset, length)` records begin at offset 16 and
name, in order, the publisher WVB, its native-lowered WVO, target startup WVO,
target publication-adapter WVO, shared SHA-256 WVO, and `WVVP` metadata.

The identity producer hashes every complete input and accepts only the pinned
release identities. It infers variant 0 (publisher) or variant 1 (promoter)
from the exact WVB/WVO identity pair rather than accepting a caller-selected
variant. The canonical variant-0 totals are 275,054 Windows bytes and 271,013
Linux bytes; variant 1 totals are 713,471 and 709,430 bytes. It prevents the
structure stage from trusting paths, embedded managed resources, or a live C#
builder.

## `WVPS 1`: admitted structure

`WVPS 1` is exactly 128 bytes. It records target and identity-envelope bytes;
the publisher WVB/WVO sizes; WVO section, symbol, and relocation counts;
native `Main` offset 3,001; private transaction-begin offset 789; private
transaction-apply offset 0; startup and adapter geometry; the adapter import
count/export offset; shared SHA object geometry; and metadata size. Offset 120
records the inferred variant; offset 124 remains reserved zero. Variant 0
therefore retains its original bytes.

The structure producer scans the admitted canonical WVO records itself. The
publisher WVO is 233,804 bytes with two sections, 27 symbols, five
relocations, 232,448 code bytes, and 288 read-only-data bytes. Startup is five
instantiated bytes. The promoter WVO is 660,123 bytes with two sections, 49
symbols, three relocations, 658,160 code bytes, 179 read-only-data bytes, and
native `Main` at offset 1,178; its transaction offsets remain 789 and 0.
Windows adapter geometry is 5,286 code bytes, 46 symbols, 111 relocations, 43
imports, and export offset 251; Linux is 3,363, 28, 49, 26, and 60. The shared
SHA object instantiates to 1,685 bytes from 1,350 code and 333 read-only-data
bytes with two internal relocations.

## `WVCR 1`: layout and output identity

`WVCR 1` is exactly 416 bytes. Its 192-byte numeric prefix contains target,
mutation flags, role at offset 28, all admitted structure values, final file/address placements,
output bytes, image end, import-page geometry, and binding counts. Seven raw
32-byte SHA-256 values follow for publisher WVB, publisher WVO, startup WVO,
adapter WVO, SHA WVO, `WVVP`, and expected final publisher application.

Key target placements are:

| Field | Windows | Linux |
| --- | ---: | ---: |
| Bundle bytes | 235,394 | 235,077 |
| Adapter file/address | 240,016 / 243,600 | 249,856 / 142,929,920 |
| SHA file/address | 245,312 / 248,896 | 253,232 / 142,933,296 |
| `WVVP` file/address | 252,896 / 259,552 | 247,264 / 247,264 |
| Final bytes | 256,000 | 254,917 |
| Image end | 143,994,880 | 142,934,981 |

Role-1 promoter placements are:

| Field | Windows | Linux |
| --- | ---: | ---: |
| Bundle bytes | 661,010 | 660,693 |
| Adapter file/address | 665,664 / 669,248 | 675,840 / 143,355,904 |
| SHA file/address | 670,960 / 674,544 | 679,216 / 143,359,280 |
| `WVVP` file/address | 678,368 / 685,536 | 673,248 / 673,248 |
| Final bytes | 681,472 | 680,901 |
| Image end | 144,420,864 | 143,360,965 |

Windows mutation flags require startup replacement, metadata, adapter, SHA,
the 4,096-byte 17-function import page, and shifted data/relocation sections.
Linux flags require startup replacement, metadata, adapter, SHA, a sixth load
segment, and moving the note from file offset `0x180` to `0x200`.

## `WVPT 1`: ordered external targets

`WVPT 1` begins with magic `WVPT` (`0x54505657`), version 1, total bytes, and
target. It then contains one `u32` address per external binding: startup-run
first, followed by adapter imports in exact admitted WVO symbol order. Windows
has 44 entries and 192 total bytes; Linux has 27 entries and 124 total bytes.
The exact WVO fixes relocation order, patch offsets, symbol indices, and `-4`
addends, so repeating all 111 or 49 relocation occurrences is unnecessary.

## Evidence and remaining work

All eight bridge/tool WVBs build through the digest-bound native source front
door and are pinned with the publisher WVO in the candidate manifest. One
reviewed focused test passes for both targets: it checks service-free bridge
entry shapes, native identity admission, interpreter/native equality for the
three small downstream records, every numeric placement, all seven output
digests, all 44/27 ordered target addresses, and malformed identity/structure
rejection.

The role-aware native file pipeline consumes the admitted geometry and `WVPT`
bindings, constructs exact startup, adapter, SHA, imports, and metadata, and
materializes both target applications. Role 0 remains byte-identical; role 1
produces the exact promoter identities recorded by the promotion contract.
Independent Linux execution and the grouped retirement gate remain before the
frozen C# writer can be removed as recovery/differential evidence.
