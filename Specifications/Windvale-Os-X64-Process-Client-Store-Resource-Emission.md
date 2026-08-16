# Windvale OS x86-64 client store-resource emission

This contract source-owns fixture offsets 10,160 through 10,398. It constructs
the private generation-three immutable store descriptor and does not publish it.
An exact 32-byte digest is required; invalid geometry emits no bytes. The record
binds the 1,196-byte store, explicit generation/rights/mutation fields, private
extent and response pointers, digest, and derived page-table reference.

`Test-Os-X64-Code-Emission` validates the exact 239-byte payload, rejection,
four bounded hashes, paired images, and result 68. Payload SHA-256 is
`279526ded2e778bf10716f07a304ee82201feac248e33b1fc9dd463657704e7f`.
The WVB is 12,594 bytes at
`e367cd4e99c842b1e18e9eba459ce034263b3cd6add89ee5d15153015e10dde6`;
Windows is 166,400 bytes at
`28d3812b8a5a627eda4a4c8eeb854a4ca266da49c35eef07997affcb05edc9ec`;
Linux is 172,144 bytes at
`96d9d990d5500af4975c54083e76a4837b2915cb873c390d0e32c7650bcb1987`.
Combined ownership reaches byte 10,398 with 34 relocation fields.
