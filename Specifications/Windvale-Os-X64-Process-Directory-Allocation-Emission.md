# Windvale OS x86-64 directory-provider allocation emission

## Status and scope

This contract source-owns fixture offsets 3,216 through 3,322. It selects the
retained directory process-record slot, allocates the isolated ten-page provider
extent, and validates its returned geometry before any provider record, page
table, image, endpoint publication, or dispatcher visibility exists.

## Allocation and validation

The constructor loads stack slot `0xc8`, passes memory-object record offset
`0xec0`, directory process reference `0x00010003`, and exactly ten pages to
allocator import symbol 13. A successful return must be nonzero and 4 KiB
aligned. Its exclusive 40,960-byte end must remain in the 1 GiB identity window,
and its first and last byte must share one 2 MiB identity-mapped window.

The allocation call field is local offset 32 with relative-i32 addend -4.
Failure fields 41, 57, 76, and 103 resolve from fixture offset 3,216 to common
failure target 33,826 using displacements 30,565, 30,549, 30,530, and 30,503.

## Verification

`Test-Os-X64-Code-Emission` validates the normalized 107-byte payload, exact
call metadata, every failure-target equation, four independent bounded hashes,
paired deterministic host images, and local result 59. The normalized payload
has SHA-256
`4b1c706de37503a89df9eecc9245f9e38f36a5281e0fcd1d370b921912b4be88`.

The self-test WVB is 14,733 bytes at
`c75790ba9823172830b6da72f83a77ce9de2014e0ac9ce4730283a21e261d76f`.
Its Windows executable is 207,872 bytes at
`45d79cbb35032809d41adb4711803772dad0f07a8696674614e832c651748d75`;
the Linux image is 213,104 bytes at
`551df680881fb91b911caa77f92cb60e02e5f68c11544ea24ffe9b3b634486a3`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 3,322 and all 29 branch/call/data relocation fields encountered
there. The next region constructs the directory process record, private paging,
immutable service/snapshot inputs, context, and descriptor.
