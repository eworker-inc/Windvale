# Windvale OS x86-64 provider-return and init-transfer emission

This contract source-owns fixture offsets 13,169 through 13,447. It validates
the returning directory-provider thread and process records, invokes the checked
dispatcher, requires the selected init thread and its page table to remain
admitted, binds kernel GS ownership, records the continuation, moves the init
thread to running state, loads its user instruction and stack context, and
performs `sysretq`.

The 279-byte normalized payload keeps page-table activation at symbol 17 and
twelve internal call, branch, and continuation fields explicit. Its SHA-256 is
`4ce7000384b72c28244c707357189c29fb6679519df928c4f24d829e8daff607`.
The WVB is 3,645 bytes at
`4f7ba1ef897096f9ae461539edde3f67f5fc2754fc2068533796ed35b6d72e18`;
Windows is 27,648 bytes at
`36ba9a985fd48c19dcca036d88ee0dde1c8dde33b426c8582e8d7788a817931c`;
Linux is 32,880 bytes at
`e6ad54583fbcdb6f5c020a1748caa02dd037af37522b46ea7fe149620662130f`.
The focused owner validates every field and target, both host images, and result
76. Combined ownership reaches byte 13,447 with 100 relocation fields.

This is fail-closed return and context-transfer evidence. It does not prove a
general syscall ABI, arbitrary scheduler selection, client publication, or live
application execution.
