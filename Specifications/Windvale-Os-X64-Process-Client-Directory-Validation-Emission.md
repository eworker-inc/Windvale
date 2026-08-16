# Windvale OS x86-64 client directory-validation emission

This contract source-owns fixture offsets 11,032 through 11,441. It validates
the private generation-four read-only directory descriptor before use and does
not publish the client. Checks cover signature, version, size, kind, rights,
generation, immutable byte/page bounds, state, digest, private extent pointers,
page-table linkage, mapped byte count, snapshot count, and W^X leaf permissions.

The 410-byte normalized payload keeps twenty-three `jne` displacement fields
zero and publishes their exact fixture offsets and displacements as checked
metadata. Every field resolves to failure offset 33,826. Its SHA-256 is
`204d82fd3eebd1e2d99ad5c0e5fd35a4466406d7696c78cb3449e22c5360dd08`.

The WVB is 4,544 bytes at
`9d04682e657cb5f3dbf2c1ce505e144458c2348c9248cb0862393b4ae143c23a`;
Windows is 64,000 bytes at
`7a0b611673c9d8aeea54a3e78ea8030f67d99cfce2de4c54cb0f99fe238c30d7`;
Linux is 69,744 bytes at
`60b016054f3205ad2726548b7ddc17d463179a9b7d204066addf63b1dd9c8d51`.
The focused owner validates exact bytes and targets, four bounded hashes, paired
images, and local result 71. Combined ownership reaches byte 11,441 with 79
explicit relocation fields.
