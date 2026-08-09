# Windvale native hosted-verifier publisher metadata

## Status and scope

This contract moves construction and independent admission of the exact
128-byte `WVVP 1` hosted-verifier publisher record into portable Windvale. It
does not construct the surrounding PE/ELF, discover native symbols, instantiate
publisher objects, or define publication behavior.

The constructor remains separate from the publisher WVB because `WVVP` embeds
that WVB's SHA-256. Importing the constructor into the publisher would create a
digest self-reference and an unresolvable repin loop.

## `WVPM 1` request

The evidence request is exactly 112 little-endian bytes:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| 0 | 4 | Magic `WVPM`, integer `0x4d505657` |
| 4 | 4 | Version `1` |
| 8 | 4 | Total bytes `112` |
| 12 | 4 | Target: `1` Windows or `2` Linux |
| 16 | 4 | Startup bytes `5` |
| 20 | 4 | Startup entry `0` |
| 24 | 4 | Native `Main` offset `3,001` |
| 28 | 4 | Transaction-begin offset `789` |
| 32 | 4 | Transaction-apply offset `0` |
| 36 | 12 | Reserved zero bytes |
| 48 | 32 | Exact target-specific startup SHA-256 |
| 80 | 32 | Exact publisher WVB SHA-256 |

The Windows request has SHA-256
`4533fb4c90bab03d5aeb39f6bd8943424f228fb66846d467d333c182d2a2b8f2`;
the Linux request has SHA-256
`a285e192992a5239495fc4046cc59390504ffefbe1ab7b863da2d370e615c500`.

## `WVPD 1` response and `WVVP 1`

Failure returns one 32-byte `WVPD 1` header with an exact status and failure
offset. Success returns that header followed by the 128-byte metadata and sets
total bytes to 160, status to zero, consumed request bytes to 112, and metadata
bytes to 128.

The metadata contains magic `WVVP`, version and size, target, five capabilities,
the exact startup/native/transaction offsets, a 4 MiB candidate limit,
publication-transaction version 1, both exact digests, and a 16-byte zero tail.
The constructor must pass its result through the separate admission module.

| Target | Metadata SHA-256 |
| --- | --- |
| Windows x64 | `40e73f9c4ac9e27c9dea7f9bed8217be125159f89cb2ea314a91bc66da389b74` |
| Linux x64 | `393253dab73387a0c96fd33c278b350fe43e5466a243eabe3f62a6652c946035` |

## Ownership and evidence

The three focused Windvale source files separate admission, construction, and
the byte-input bridge. The native project front door builds 13 functions and
9,230 code bytes into a 10,441-byte WVB with SHA-256
`208b2724a10f2e497ef13be51d254426e86afda99600c61dd937cdf4171d3bbd`.

One focused current-host test proves a service-free `Main(bytes) -> bytes`,
interpreter/native equality, exact Windows/Linux request and metadata hashes,
byte equality with both committed publisher applications, and nine malformed
request boundaries. No new C# construction semantics are introduced.

Native symbol/evidence discovery, the five startup/adapter/SHA object inputs,
publisher-specific Windows imports, target layout, complete application
materialization, independent Linux execution, and grouped promotion remain.
