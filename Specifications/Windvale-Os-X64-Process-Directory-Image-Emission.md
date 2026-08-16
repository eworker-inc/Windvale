# Windvale OS x86-64 directory-provider image emission

## Status and scope

This contract source-owns fixture offsets 4,225 through 4,340. It copies the
measured directory service and immutable snapshot into private mapped pages,
initializes native execution context, and writes a generation-tagged snapshot
descriptor before any provider publication.

## Bounded construction

The constructor requires a nonempty service image no larger than two pages and
a nonempty snapshot no larger than one page. Invalid geometry emits no bytes.
The current fixture copies 3,911 service bytes to page 4 and 3,184 snapshot
bytes to read-only/NX page 8. It writes native context format 7, size 112,
instruction budget 64, and call-depth budget 1 at page 7. The descriptor at
extent offset `0x7180` points to page 8, records the exact snapshot length, and
publishes descriptor generation 1 inside the still-private extent.

RIP-relative fields 3 and 25 map to process-object symbols 6 and 4 with addend
-4. At fixture offset 4,225 their absolute fields are 4,228 and 4,250.

## Verification

`Test-Os-X64-Code-Emission` validates bounds rejection, both typed data
relocations, the exact 116-byte payload, four independent bounded hashes,
paired deterministic host images, and local result 62. The payload has SHA-256
`2cd7e484a2eb928cdc3862d2660a7a05f5b39a92efee153b1aa992eaf3dd30b2`.

The self-test WVB is 15,098 bytes at
`589034ed2ae906ba8c96ebedb3e583decb9d9181527b70b389d64296f66a4171`.
Its Windows executable is 204,288 bytes at
`b20d649b83c3b3ca54550118f77c7775a4937d789f0c08832c03444861c68fbd`;
the Linux image is 209,008 bytes at
`4c66120f10ba53e10cf1e7e31ca600eef51d47874b5f629aec0f8c46091bef98`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 4,340 and all 33 relocation fields encountered there. The next
region initializes the recyclable client record, page tables, interpreter image,
context, and resource records before either process becomes runnable.
