# Windvale OS x86-64 recyclable-client paging emission

## Status and scope

This contract source-owns fixture offsets 4,859 through 9,606. It copies the
three retained kernel paging-table pages into the private client extent,
constructs the client root, maps the exact admitted user geometry, and clears
two following guard entries. It does not copy the interpreter/program inputs,
initialize context, or publish the client.

## Table construction

The constructor copies 512 qwords from each retained kernel table page, binds
the private lower tables, fills one bounded 512-entry identity PTE page,
preserves the null-page hole when its physical window begins at zero, and binds
that PTE page at the derived third-level index.

Code pages 4 through 113 are present/user, executable, and non-writable. Stack
pages 114 through 119, data page 120, and response page 121 are present/user,
writable, and NX. Pages 122 and 123 are explicitly cleared as post-extent guard
entries. No unlisted page is made user-accessible.

Local `jne` fields 155 and 171 encode -21 and +11, targeting the bounded PTE
loop and null-page completion. No external import is added.

## Verification

`Test-Os-X64-Code-Emission` validates the exact 4,748-byte payload, both local
branch fields, four independent bounded hashes, paired deterministic host
images, and local result 64. The payload has SHA-256
`824ec2c944b5bebe479bf785eb2e30eeb05d06e04e95245e90c83cea27585a62`.

The self-test WVB is 14,563 bytes at
`b848688f23ff1e1750044eaec3b4f1837454f7a0c73938699435ce56f81b8fe9`.
Its Windows executable is 206,336 bytes at
`5e67969e9047f8b5d71ec79d0de6c86bfdaa77905fac314d12a6ab9d8e7cced7`;
the Linux image is 209,008 bytes at
`bd58157bc0b8023ea2a413c50a5b275bf958b256d08fcbb310a8abb96cca740e`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 9,606 and all 33 relocation fields encountered there. Interpreter
and program copies, execution context, and final resource records remain
private construction work before readiness publication.
