# Windvale OS x86-64 process memory-allocation emission

## Status and scope

This contract source-owns fixture offsets 1,872 through 1,970 for the first
protected-process memory-object allocation. It prepares the existing kernel
allocation call and validates the returned init extent before any process record
or page table is published.

The slice is privileged mechanism evidence. It is not a portable allocator,
general dynamic application launch, or proof that the new provider launch
transaction is connected to live kernel allocation.

## Allocation and validation

The constructor passes the memory-state base, memory-object record offset
`0xc60`, init process reference `0x00010001`, and exactly 12 pages to the
kernel allocation import. A successful return must be nonzero and 4 KiB
aligned. The exclusive 49,152-byte end must remain within the 1 GiB identity
window, and the allocation's first and last byte must occupy the same 2 MiB
identity-mapped large-page window.

Four failure branches remain zeroed in the independent 99-byte slice at
displacement fields `33`, `49`, `68`, and `95`. At fixture offset 1,872 they
resolve to absolute failure target 33,826 using displacements `31,917`,
`31,901`, `31,882`, and `31,855`. The allocation call field is `24` and uses a
relative-i32 WVO relocation to import symbol index 11 with addend -4.

These offsets are fixture-reconstruction evidence, not a stable kernel ABI.

## Verification

`Test-Os-X64-Code-Emission` validates the normalized 99-byte payload, every
failure target equation, allocation import metadata, paired host images, and
local result 54. The normalized payload has SHA-256
`971392d74447dd464c33d6df5379891d324afe29f4bad21384c35942f9612723`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 1,970 and all 13 branch/call relocation fields encountered there.
The next region initializes the complete init process record and begins page-
table construction.
