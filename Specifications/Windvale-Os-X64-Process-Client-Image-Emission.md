# Windvale OS x86-64 recyclable-client image emission

## Status and scope

This contract source-owns fixture offsets 9,607 through 9,682. It copies the
admitted bytecode interpreter into the private executable pages and initializes
the client execution context. It does not construct resource records or publish
the client.

## Bounded construction

The constructor requires a nonempty interpreter no larger than 110 pages
(450,560 bytes); invalid geometry emits no bytes. The current fixture copies
449,261 bytes to page 4. It writes native context format 7, size 112,
instruction budget 189,137, call-depth budget 5, and a private 1,024-byte text
arena at offset 512 inside data page 120.

The RIP-relative field at local offset 3 maps to process-object symbol 1 with
addend -4. Its absolute fixture field is 9,610.

## Verification

`Test-Os-X64-Code-Emission` validates bounds rejection, the typed relocation,
the exact 76-byte payload, four independent bounded hashes, paired deterministic
host images, and local result 65. The payload has SHA-256
`54432a2880a44c20e9c9246eeab45a488a9f9aa7746d2eff9aaef0671faac633`.

The self-test WVB is 13,798 bytes at
`e45446f9c0aa6d8806c3427d2aa3900266067112ff90c29b8d0dea2ea4f4aafd`.
Its Windows executable is 187,904 bytes at
`741049bdb17717f89fc617322a5aa07fe94a4e2c2e3e1286a5a83d62b285067f`;
the Linux image is 192,624 bytes at
`a2b3880da1d0bdefaf491717d180bb638118d9b706f550f2100b7e596382c1fe`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 9,682 and all 34 relocation fields encountered there. Client
resource records and program binding remain private construction work before
readiness publication.
