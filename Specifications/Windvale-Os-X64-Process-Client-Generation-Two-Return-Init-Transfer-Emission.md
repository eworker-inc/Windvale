# Windvale OS x86-64 generation-2 client-return/init-transfer emission

This contract source-owns fixture offsets 27,139 through 27,469. It derives the
existing checked 331-byte client-return/init-transfer constructor with exactly
one semantic change: the returning client generation byte is 2 instead of 1.
Processor and thread-result validation, dispatcher entry, page-table activation
at external symbol 17, init GS/continuation publication, result 55, and the init
`sysretq` remain unchanged.

The normalized payload keeps symbol field 225 and fourteen internal fields
explicit. Its SHA-256 is
`c8ca2e13217d55c420ff809110d0ea8596e09a99b69a01b8fbb0fb3be8f4d9c0`.
The WVB is 4,615 bytes at
`b40078fd3d2d928280b697af647bca5a6b399eae9de598e1c43672289f33abcb`;
the WVO is 36,875 bytes at
`900e0571a75ad5f7ed06ddf2817aea32d26867cbfbed55b64d543aea5e5c5d18`;
the linked binary is 36,099 bytes at
`efc70741079a752de3c1e4a1e547dd84f365b5a0bd01fa3400d81ab85378319a`;
Windows is 37,888 bytes at
`31a60d1b18a4bdb2f73b37e8e7141dcb9891196812101ba26cfcafbcaafbcbff`;
Linux is 41,072 bytes at
`e57c926117d430ea068b294ec5d9bce0fe35bdf24a0463b15721bdd9b86f5775`.
The focused native owner validates the generation adaptation, every field,
four hashes, both host images, and result 97.

Combined source ownership reaches byte 27,469 with 245 internal or external
relocation fields. The following init return handler, later lifecycle, and live
guest evidence remain separate boundaries.
