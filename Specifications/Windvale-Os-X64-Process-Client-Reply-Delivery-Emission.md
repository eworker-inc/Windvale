# Windvale OS x86-64 client reply-delivery emission

This contract source-owns fixture offsets 15,244 through 15,574. It validates
the reply-delivery syscall/thread state and init-owned reply record, dispatches
only the admitted client generation, reactivates its checked page table, restores
its saved context, and returns the exact 116-byte reply result through `sysretq`.

The 331-byte normalized payload keeps page-table activation at symbol 17 and
fourteen internal call, branch, and continuation fields explicit. Its SHA-256
is `4e527e65c9007a43dd523b1fd8e2518a0be8699738d574177eb66b25b0ff8773`.
The WVB is 3,806 bytes at
`668972466f58918a5d13930fdce2ff160d56d25d45907cd0d17214b2689cf44f`;
Windows is 28,160 bytes at
`11d820588e286c52ea4c6374a5e99c80c5803ed1840a9bce9833053fd9b4baa5`;
Linux is 32,880 bytes at
`1bce868f6b93bfafc8433cf501678736bef090a2511dd7c835eef9f8be6733c2`.
The focused owner validates every field and target, both host images, and result
83. Combined ownership reaches byte 15,574 with 104 external relocation fields.

This proves delivery of the first retained reply to the client. It does not yet
prove the later directory-provider exchange, general handlers, context
switching, or live application execution.
