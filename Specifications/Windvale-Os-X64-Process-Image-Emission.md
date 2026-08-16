# Windvale OS x86-64 init image and context emission

## Status and scope

This contract source-owns fixture offsets 2,949 through 3,097. It copies the
four immutable init inputs into their private mapped pages, initializes the
native execution context, and publishes the resource-store descriptor inside
the still-unpublished init extent.

This slice does not activate the root, expose the process to the dispatcher, or
make the filesystem/network provider images runnable. Its four zeroed RIP-
relative data fields remain typed WVO relocations until complete machine
composition.

## Bounded inputs and construction

The constructor requires a nonempty service image no larger than two pages and
nonempty admitted-program, execution-budget, and resource-store inputs no
larger than one page each. Invalid geometry produces no bytes.

The current fixture copies 5,159 service bytes to page 4, 816 admitted-program
bytes to page 8, four execution-budget bytes to page 9, and 1,196 store bytes to
page 10. It then writes native execution-context format 7, size 112, instruction
budget 64, and call-depth budget 1 at page 7. The store descriptor at extent
offset `0x7180` points to page 10 and records the exact store length.

Local relocation fields 3, 25, 47, and 69 map to process-object symbols 0, 2,
3, and 5 respectively. Each is relative-i32 with addend -4; at fixture offset
2,949 the corresponding absolute fields are 2,952, 2,974, 2,996, and 3,018.

## Verification

`Test-Os-X64-Code-Emission` validates geometry rejection, all four relocation
records, the exact 149-byte payload, four independent bounded hashes, paired
deterministic host images, and local result 57. The payload has SHA-256
`6f8d6a7c0fcdf3c1b76955b43057c7b3e1e52d9eeedc069b51c6fbd718316b8e`.

The self-test WVB is 16,434 bytes at
`3207175a3928407f8b0fb1976e8f55c3643ffa5f0555a46fa9379354d90c0ae1`.
Its Windows executable is 212,480 bytes at
`722e4d867408a750d534ddd2ca55b43512ef934d68fd66aaf8e8ba1411d6c8e7`;
the Linux image is 217,200 bytes at
`58b42db3daa211c10f79426dae970fb635233ec19e7f135a1e54ed963e526a87`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 3,097 and all 19 branch/call/data relocation fields encountered
there. The next region reserves the recyclable client extent before constructing
the directory provider and client records.
