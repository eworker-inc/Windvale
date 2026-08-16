# Windvale OS x86-64 client user-transfer emission

This contract source-owns fixture offsets 14,403 through 14,576. It leaves the
returning init context, invokes the fixed dispatcher, requires the selected
thread to be the admitted client generation, reactivates its checked page table,
binds kernel GS and continuation state, loads its private instruction/stack
context, and performs `sysretq`.

The 174-byte normalized payload keeps page-table activation at symbol 17 and six
internal call, branch, and continuation fields explicit. Its SHA-256 is
`74a9b3c03618324e6acdbc56f088c9385be19b7d71fa2ecd9ce4eca8e13f8a84`.
The WVB is 3,861 bytes at
`396c95aacd156af86f6b56d2461a255de115cd5292267035a7d4e5ae4f2ea8a1`;
Windows is 37,376 bytes at
`67eb2ded3d5b168c75b6b8300b30e026f5bf46110fed521f517277e69522effc`;
Linux is 41,072 bytes at
`4ad53750962c1363f1c1460253c0d917139e581efac4df436c4979c4f60e82d4`.
The focused owner validates every field and target, both host images, and result
80. Combined ownership reaches byte 14,576 with 101 external relocation fields.

This proves exact guarded client entry in the recovered machine contract. It
does not yet prove the client-return path, handler bodies, context switching, or
live application execution under QEMU.
