# Windvale OS x86-64 client budget-resource emission

## Status and scope

This contract source-owns fixture offsets 9,931 through 10,159. It constructs
the private generation-two execution-budget descriptor. It does not publish the
resource or client.

The constructor requires an exact 32-byte budget digest; invalid geometry emits
no bytes. The 229-byte descriptor records `WVRES006`, the four-byte bounded
budget payload, generation and rights, private extent/record/response pointers,
the exact digest, and the derived private page-table reference.

`Test-Os-X64-Code-Emission` validates rejection, exact bytes, four bounded
hashes, paired host images, and local result 67. The payload SHA-256 is
`c302afea1399673cc047272d17f712a301d9bff35c1c5df062eec2232776605f`.
The WVB is 12,586 bytes at
`080eec8cd90b5364bc374eed8fdd3dae520ce7ee9bfb48c0ff30e08aa7150939`;
Windows is 166,400 bytes at
`577eb58b87816cc15004096f1a20b5e042e3196b5ee3b71af691e91f89f92725`;
Linux is 172,144 bytes at
`7461b66d1b74e3dbab07f682d00627c16b32bd1a15a99beef52a8da2aeeb288f`.

Combined source ownership now reaches byte 10,159 with 34 relocation fields.
