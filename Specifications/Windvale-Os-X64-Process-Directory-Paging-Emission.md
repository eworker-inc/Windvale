# Windvale OS x86-64 directory-provider paging emission

## Status and scope

This contract source-owns fixture offsets 3,785 through 4,224. It copies the
three retained kernel paging-table pages into the validated directory extent and
constructs the provider's private, null-safe, W^X user mappings.

It does not copy the provider service/snapshot inputs, initialize context, or
publish the provider. Those remain required before dispatcher visibility.

## Table construction

The constructor copies 512 qwords from each retained kernel table page, binds
the private lower tables, fills one bounded 512-entry identity PTE page,
preserves the null-page hole when its physical window begins at zero, and binds
that PTE page at the derived third-level index.

Code pages 4 and 5 are present/user and executable but non-writable. Stack page
6, data page 7, and response page 9 are present/user/writable/NX. Immutable
snapshot page 8 is present/user/read-only/NX. No unlisted page is made user-
accessible.

Local `jne` fields 155 and 171 encode -21 and +11, targeting the bounded PTE
loop and null-page completion. No external import is added.

## Verification

`Test-Os-X64-Code-Emission` validates the exact 440-byte payload, both branch
fields, four independent bounded hashes, paired deterministic host images, and
local result 61. The payload has SHA-256
`6ec4a6d510027b8871346b888e0e6c0479a17696f6e0372bfbec45aa5c993bf4`.

The self-test WVB is 14,228 bytes at
`caba027a75434fc07c2f44cafead16f595e7ce4fc13a84864041204d24cd5c17`.
Its Windows executable is 203,776 bytes at
`0308cf1a5d01eeb2d463f43bc4ea3b3993f4922b5732cee7e8b23964e2d001c0`;
the Linux image is 209,008 bytes at
`303eada707e4868fba8406ccc304e5764ce069d156808d6f44245e98629fb0d9`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 4,224 and all 31 relocation fields encountered there. The next
region copies the directory service and immutable snapshot, initializes native
context, and publishes the private snapshot descriptor.
