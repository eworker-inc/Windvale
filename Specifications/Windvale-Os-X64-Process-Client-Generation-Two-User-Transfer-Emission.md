# Windvale OS x86-64 generation-2 client user-transfer emission

This contract source-owns fixture offsets 26,965 through 27,138. It derives the
existing checked 174-byte client user-transfer constructor with exactly one
semantic change: the required selected-client generation byte is 2 instead of
1. Dispatcher exit, page-table activation at external symbol 17, kernel-GS and
continuation binding, private instruction/stack restoration, and `sysretq`
remain unchanged.

The normalized payload keeps symbol field 59 and six internal fields explicit.
Its SHA-256 is
`4dd0b6f855e8bcbce9f719d520d3c1902d4a71a65528e887950fc578e86ce9b7`.
The WVB is 4,631 bytes at
`42360bdbb290b1bdbd404aaacccbaeb5ac39fc594a9816d0387b4b62a628550f`;
the WVO is 45,972 bytes at
`fd48f7fb722a87c1fed401d62b0f7ab0176245af51057b53492272241eacbaad`;
the linked binary is 45,246 bytes at
`71e40197bee68552acdd22d9bd85a789ccd862c112a50e980e4f26a016364370`;
Windows is 47,104 bytes at
`1fa2d7a6e9b1764dba3c3212ea4ec4c74cecbf5a79b6351a703e6cbe5571f7a7`;
Linux is 53,360 bytes at
`9a9c9e5ebed6472e6eed72f2d002c72e52629657a579c50c73fc917604790de9`.
The focused native owner validates generation adaptation, all fields, four
hashes, both host images, and result 96.

Combined source ownership reaches byte 27,138 with 230 internal or external
relocation fields. The following resumed handler, later lifecycle, and live
guest evidence remain separate boundaries.
