# Windvale OS x86-64 client-return and init-transfer emission

This contract source-owns fixture offsets 14,577 through 14,907. It validates
the returning client thread, syscall state, and init-owned process record, marks
the client waiting, dispatches only the admitted init generation, reactivates
its checked page table, binds kernel GS and continuation state, restores its
saved user context, and performs `sysretq`.

The 331-byte normalized payload keeps page-table activation at symbol 17 and
fourteen internal call, branch, and continuation fields explicit. Its SHA-256
is `382e1acec4dd1b287bf3a183c30c02443e4e170334406c13683955cbf58ac4f7`.
The WVB is 3,813 bytes at
`96456642337d7eaf7ef4c8c497eb3f262fd6722ac71f0efe18b6ee9e12f84950`;
Windows is 28,160 bytes at
`cf8fe7a00c3b5bd183bedb6a9878d8f1d2aad32f83d98fecf7700b3a3d10c553`;
Linux is 32,880 bytes at
`b2a09c2ee5aaa2255866247c6ca5ba22ca5526f3dd2dedce9c67832cc89f2213`.
The focused owner validates every field and target, both host images, and result
81. Combined ownership reaches byte 14,907 with 102 external relocation fields.

This proves exact guarded return from the client to init in the recovered
machine contract. It does not yet prove the following init/provider exchange,
general handler bodies, context switching, or live application execution.
