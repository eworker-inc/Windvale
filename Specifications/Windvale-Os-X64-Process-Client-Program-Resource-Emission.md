# Windvale OS x86-64 client program-resource emission

## Status and scope

This contract source-owns fixture offsets 9,683 through 9,930. It clears the
private client resource table and constructs the first generation-one program
resource descriptor. It does not publish the resource or client.

## Bounded construction

The constructor requires an exact 32-byte admitted program digest; invalid
geometry emits no bytes. The descriptor records `WVRES006`, the fixed 128-byte
record shape, generation-one identity and rights, private client extent and
record pointers, response bounds, the exact program digest, and the derived
private page-table reference. The complete table clear occurs before any field
is populated.

## Verification

`Test-Os-X64-Code-Emission` validates digest rejection, the exact 248-byte
payload, four independent bounded hashes, paired deterministic host images,
and local result 66. The payload has SHA-256
`a8e2b2f3be9588c6b3b044aa6bf67a75f06a38b19b902bd4f3c665640e7fad20`.

The self-test WVB is 12,763 bytes at
`d0c7e8f7890e6cbc0168dfe122564b48f03a2c4d5bfb658e4e20a9c4ec4e85a1`.
Its Windows executable is 168,960 bytes at
`ac00e3dc1267d2c1c5ce11e389ea93711297930a7b99c1fb061d148b3c001f49`;
the Linux image is 172,144 bytes at
`d8b7bf66d482a976a7ecec2b3c0d408c52d942e0b0c75360883cf117aab3d72f`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 9,930 and all 34 relocation fields encountered there. Remaining
client resources and program input binding must complete before readiness.
