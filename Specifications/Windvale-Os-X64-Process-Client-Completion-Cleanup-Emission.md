# Windvale OS x86-64 client-completion cleanup emission

This contract source-owns fixture offsets 16,573 through 17,923. It validates
the exiting generation-1 client, its dormant compatibility arena, both
rights-limited endpoint/channel records, generation references, page mappings,
and retained request/reply geometry. It then clears both endpoint PTEs, scrubs
every retained IPC destination and message field, and returns both endpoint
records to their closed generation-safe state before later reclamation.

The 1,351-byte normalized payload has 61 explicit fail-closed branches to the
shared terminal target and no new external relocation. Its SHA-256 is
`6b66ef89c367d568bf54b3bf07c8d123d06ef72054d61f6d02b19aa1734bfb9c`.
The WVB is 4,541 bytes at
`36b58e50809e26264419c1fca7e429b337fb08f71f4da91fc5a9887cb05306e2`;
Windows is 25,088 bytes at
`d231f32c41c0ef2d7493180edfeb3edb8f04aed8b1ccb1d5711b8772b0fc28eb`;
Linux is 28,784 bytes at
`78130c20bab66eb81a42700f6a0c77c89db51457e56c808d674e7f7b1a9e495a`.
The focused owner validates all branch fields and targets, both host images,
and result 87. Combined ownership reaches byte 17,923 with 107 external
relocation fields.

This proves the first client's checked terminal IPC cleanup. It does not yet
prove memory-object reclamation, generation-2 reconstruction, later lifecycle,
general handlers, context switching, or live application execution.
