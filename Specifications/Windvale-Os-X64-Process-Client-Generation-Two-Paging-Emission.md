# Windvale OS x86-64 generation-2 client-paging emission

This contract source-owns fixture offsets 20,241 through 24,988 by deliberately
reusing the checked recyclable-client paging constructor. The generation-2
fixture region is byte-for-byte identical to generation 1's offsets 4,859
through 9,606.

The 4,748-byte constructor copies the retained kernel paging tables, creates the
private lower hierarchy, retains the null-page hole, maps code pages 4 through
113 read/execute, maps stack/data/response pages 114 through 121 writable/NX,
and clears guard entries 122 and 123. Local branch fields 155 and 171 retain
their exact `-21` and `+11` displacements. No external relocation is added.

The payload SHA-256 remains
`824ec2c944b5bebe479bf785eb2e30eeb05d06e04e95245e90c83cea27585a62`.
The generation-2 self-test WVB is 14,544 bytes at
`f7e189d04bdf740c5c1b2224c5872a2e3c0159e6408dd4de69dbd6ab3a1db9f2`;
Windows is 206,336 bytes at
`62c74695812eec852cf3dddee37cec39596da28397e0f2abdbaf28e8475119c2`;
Linux is 209,008 bytes at
`bae80f3e33d79d1446cd54af10c066da7d2bcdb2e95f74ddd4b32eab2d9a1511`.
The focused owner validates the reuse identity, both local branches, four
bounded hashes, both host images, and result 91. Combined ownership reaches
byte 24,988 with 120 external relocation fields.

This proves private generation-2 paging reconstruction. Image copies,
resources, context, endpoint rebinding, ready publication, and re-entry remain
separate transactions.
