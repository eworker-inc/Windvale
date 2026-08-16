# Windvale OS x86-64 generation-2 client return validation emission

This contract source-owns fixture offsets 25,954 through 26,964. After the
first generation-2 `sysretq`, the 1,011-byte handler validates processor/GS
state, completion counters, both retained resource records, generation-bounded
references, page-table entries, backing-object aliases, and per-resource native
context records before the next dispatcher crossing.

All sixty PC-relative fields are fail-closed branches to the shared terminal
target at offset 33,826. The slice has no external call relocation. Its
normalized SHA-256 is
`340f3a8475b659130200e9422629c51e2889a76b4e2c1ddf54f88f84b6146d97`.

The WVB is 4,209 bytes at
`8bcbfd66daed3fb0b92c3977374725df6264e0c591444f7d40528badb2aeb1c9`;
the WVO is 23,047 bytes at
`8907202105823e1a422036ddd2f93cd578cbdc205f1fc6457f2fb3e08705edc2`;
the linked binary is 22,627 bytes at
`976c872494037a1f83428cfbcd7d6b80f581469d6c59d473bc2ddfaf7202ad19`;
Windows is 24,576 bytes at
`a26c69002b00dd46f04000ee40f2e6f4603033d2bd5028c171cd0f81dd5114f2`;
Linux is 28,784 bytes at
`6e04e1f5c7420df1f91a0df1f1085bd8af15b428bd79a7108a8390d604de7926`.
The focused owner validates all fields and targets, four payload hashes, both
host images, and result 95. Combined source ownership reaches byte 26,964 with
223 internal or external relocation fields.

The next `swapgs`, dispatcher call, subsequent re-entry, and live guest evidence
remain separate boundaries.
