# Windvale OS x86-64 client store-validation emission

This contract source-owns fixture offsets 10,638 through 11,031. It validates
the private generation-three immutable-store descriptor before the recyclable
client may use it and does not publish the client. The checks cover the record
signature, version, size, kind, rights, generation, immutable byte and page
bounds, descriptor state, four measured digest words, private extent pointers,
page-table linkage, and W^X leaf permissions.

The 394-byte normalized payload keeps twenty-two `jne` displacement fields zero
and publishes their exact fixture offsets and signed-positive displacements as
separate checked metadata. Every field resolves from fixture offset 10,638 to
the existing failure target at offset 33,826. The normalized payload SHA-256 is
`104dbc9735859a1ac61f3d03e47c613d60ea9eea665c418db211dab650f62ec7`.

The WVB is 4,504 bytes at
`8e0e5c8b0dcc5d58c6f89a517af6ae1bcc30fcf99da2e63fef09892d67c81ead`;
Windows is 63,488 bytes at
`ec3353bc21a776fdb2970e709cf9ba1282e33d3f42e086c27054d925b2cf105f`;
Linux is 69,744 bytes at
`96f3b1fb420ac01b38c553051c88a2d9fca453d11cd417e99e8c8ae1aff6a699`.
The focused owner validates length, zeroed fields, all target equations, four
bounded hashes, paired images, and local result 70. Combined ownership reaches
byte 11,031 with 56 explicit relocation fields.
