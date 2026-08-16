# Windvale OS x86-64 init process paging emission

## Status and scope

This contract source-owns fixture offsets 2,433 through 2,948. The first 80
bytes copy the three retained kernel paging-table pages into the validated init
extent. The following 436 bytes construct the init process's private root and
bounded user mappings.

This is exact privileged mechanism evidence. It does not define portable paging
semantics, publish the process, copy executable bytes, or connect the filesystem
and network provider transactions to live kernel page-table allocation.

## Table construction

The constructor copies 512 qwords from each of the retained root, second-level,
and third-level kernel table pages. It then binds the new root to its private
lower tables, derives the containing 2 MiB physical window, fills exactly 512
identity PTEs, preserves a null-page hole when the window begins at zero, and
binds the private PTE page at the derived third-level index.

The init profile maps code pages 4 and 5 present/user without write or NX. Stack
page 6, data page 7, and response page 11 are present/user/writable/NX. Runtime
input pages 8 and 9 plus resource-store page 10 are present/user/read-only/NX.
Those exact mappings preserve W^X and keep unlisted pages unavailable.

Two local `jne` fields are part of the slice. At local displacement fields 155
and 171 they encode -21 and +11, targeting the bounded PTE loop and null-page
completion respectively. They add no external WVO imports.

## Verification

`Test-Os-X64-Code-Emission` validates the exact 516-byte payload, both local
branch fields, four independent bounded hashes, paired deterministic host
images, and local result 56. The payload has SHA-256
`9ad8bfc3fe718503a4b1ff8d456e99125020e45e58ecb9293f7aafd5167456a0`.

The self-test WVB is 14,379 bytes at
`e2f712fb99ecc186211c957a4bdf9f9b0991ad7c735dcb8d47c643e85f9fd50d`.
Its Windows executable is 206,848 bytes at
`857d384d8e62ccfb435986c4b607d8a7615b9d9bc8c78d1bd73efa38f0dc832e`;
the Linux image is 213,104 bytes at
`fd20a386a8a0e03a9efce86444498e119f7dffbd67263c3845659d1a7f949ef2`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 2,948 and all 15 branch/call relocation fields encountered there.
The next region copies the init executable, admitted WVB, execution budget, and
resource store into the private address space before constructing user context.
