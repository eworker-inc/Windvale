# Windvale OS x86-64 generation-2 client re-entry emission

This contract source-owns fixture offsets 25,513 through 25,953 as one checked
generation-2 re-entry transaction. It validates the recycled 13-page memory
object, crosses the existing dispatcher boundary, validates the returned client
generation, binds kernel GS state, publishes the resume context, completes the
resource-state transition, restores user registers, and executes `sysretq`.

The 441-byte normalized payload has fourteen internal PC-relative fields:
twelve fail-closed branches to offset 33,826, one dispatcher call to offset 37,
and one resume address to offset 25,954. It has no external object relocation.
Its SHA-256 is
`13dd9ec88a9f406705bda82054ce4935a66f134e3bd582a93d7a4f6e8c6ce2c8`.

The WVB is 3,518 bytes at
`835c7a03de1da731172f5e5d8b515c18f5dc62a40ca030351be90ee9ed6760a3`;
the WVO is 23,885 bytes at
`448053de1bc86b48f211668229d6c5f0af21c9e1da73273c36e802b8e995811c`;
the linked binary is 23,465 bytes at
`77dc2b346ab09fd34f1458517371df4ce826a6f019f5736155c525c927e5f2eb`;
Windows is 25,600 bytes at
`1cad43b99f4712ea1779d4bbc34238e02be1e1974cbcdaaa3957db83f7c64bcb`;
Linux is 28,784 bytes at
`4c810ad307e38c6db1264147f23d5e51f3f89375648fcc7813ac4c6ab6e590ea`.
The focused owner validates every internal field and target, four payload
hashes, both host images, and result 94. Combined ownership reaches byte 25,953
with 163 internal or external relocation fields.

This proves exact machine-code ownership through the first generation-2
`sysretq`; it does not yet prove a live guest transition. The resumed handler
body, subsequent application behavior, teardown, and live QEMU evidence remain.
