# Windvale OS x86-64 init reply-publication resume emission

This contract source-owns fixture offsets 14,908 through 15,243. It validates
init's returning syscall/thread state and the init process's retained 116-byte
reply record, clears the channel publication state, dispatches only the admitted
init generation, reactivates its checked page table, restores its saved context,
and returns a zero completion result through `sysretq`.

The 336-byte normalized payload keeps page-table activation at symbol 17 and
fourteen internal call, branch, and continuation fields explicit. Its SHA-256
is `ea3769665f95a2054d4cc2594d743555a2893cc7067720add66ccf5ee995dc94`.
The WVB is 3,816 bytes at
`8f23f7f711f25908c4910ed5de9b2c4097d28d0ae6c1fdc57a0cbffb6cf5c92b`;
Windows is 28,160 bytes at
`68370c479947101120bc84ef6e910aa9b1b8d3f74f42d201934a8144c25f38d2`;
Linux is 32,880 bytes at
`72993c6a4b09d484d56d414b8d41b0ec900c748ce20314eb4a57bee8763c1f4c`.
The focused owner validates every field and target, both host images, and result
82. Combined ownership reaches byte 15,243 with 103 external relocation fields.

This proves the reply-publication completion returned to init. It does not yet
prove delivery of the reply to the client, later directory-provider exchanges,
general handlers, context switching, or live application execution.
