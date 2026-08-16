# Windvale OS x86-64 recyclable-client reservation emission

## Status and scope

This contract source-owns fixture offsets 3,098 through 3,215. It selects the
retained client process-record slot, reserves the 122-page recyclable client
extent, validates the returned geometry, and retains its root address for later
directory-provider and client construction.

This slice does not initialize or publish the client record, install page tables,
or make an application runnable. It preserves the kernel allocator import and
failure edges as typed relocation evidence.

## Allocation and validation

The constructor loads stack slot `0xc0`, passes memory-object record offset
`0xd90`, first-client reference `0x00010002`, and exactly 122 pages to allocator
import symbol 13. A successful return must be nonzero and 4 KiB aligned. Its
exclusive 499,712-byte end must remain within the 1 GiB identity window, and
its first and last byte must share one 2 MiB identity-mapped window. The root is
then retained in stack slot `0xd8` without process publication.

The allocation call field is local offset 32 and uses a relative-i32 WVO
relocation with addend -4. Failure fields 41, 57, 76, and 103 resolve from
fixture offset 3,098 to common failure target 33,826 using displacements
30,683, 30,667, 30,648, and 30,621.

## Verification

`Test-Os-X64-Code-Emission` validates the normalized 118-byte payload, exact
call metadata, every failure-target equation, four independent bounded hashes,
paired deterministic host images, and local result 58. The normalized payload
has SHA-256
`e5aeaef67c50076c8b46c1da56dd788420020a1fe1c41ca88f4f3a41cd27c0ab`.

The self-test WVB is 14,957 bytes at
`bd9bd8bb378642e707e5a328a783dd42df20457aa04c967fcbf63cf8845678b4`.
Its Windows executable is 211,968 bytes at
`b98c4e3351ea369e6eb70fb8476b03d61300065ae0b57e0d860de458a955196f`;
the Linux image is 217,200 bytes at
`547f5351c84530e41436b51f03b25680f1added815d6238998dc5fe7915e0684`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 3,215 and all 24 branch/call/data relocation fields encountered
there. The next region constructs the isolated directory provider before the
recyclable client is initialized.
