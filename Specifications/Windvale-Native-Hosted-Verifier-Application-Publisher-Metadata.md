# Windvale native hosted-verifier publisher metadata

## Status and scope

This contract moves construction and independent admission of the exact
128-byte `WVVP 1` hosted-verifier publisher record into portable Windvale. The
same record now distinguishes the original verifier-application publisher as
construction variant `0` from the publisher promoter as variant `1`. It
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
| 24 | 4 | Native `Main` offset: `3,001` variant 0 or `1,178` variant 1 |
| 28 | 4 | Transaction-begin offset `789` |
| 32 | 4 | Transaction-apply offset `0` |
| 36 | 4 | Construction variant: `0` publisher or `1` promoter |
| 40 | 8 | Reserved zero bytes |
| 48 | 32 | Exact target-specific startup SHA-256 |
| 80 | 32 | Exact variant-specific WVB SHA-256 |

The exact request identities are:

| Variant | Windows x64 | Linux x64 |
| ---: | --- | --- |
| `0`, publisher | `4533fb4c90bab03d5aeb39f6bd8943424f228fb66846d467d333c182d2a2b8f2` | `a285e192992a5239495fc4046cc59390504ffefbe1ab7b863da2d370e615c500` |
| `1`, promoter | `144199ee91341765488cf33a50d8e80d036442cc155bba7863f78be6e746f749` | `560569d3d2282e3722d2624196b2bcb16062d65313d9235e3a7fc718c67123e9` |

Variant 0 admits the 29,170-byte publisher WVB at SHA-256
`77c6f34a823fc41175647c4d0c4708507ab8b97c7b1726c983188f962fd5509f`.
Variant 1 admits the 41,268-byte promoter WVB at SHA-256
`30eb1e8c93b01266592b322b9c5154b27782ea6c7cd2b6522a10781bf935bec9`.
Both variants admit the unchanged target startup WVO: Windows 168 bytes at
`bb136af0382b2f72efc8a07f58fb2368319fce7c119bc7bbfa1b94da6ded9367`
or Linux 164 bytes at
`eee997412ced0d7edacaf39dae9c4a3c51e859dce4537045f3972be990b115a4`.

## `WVPD 1` response and `WVVP 1`

Failure returns one 32-byte `WVPD 1` header with an exact status and failure
offset. Success returns that header followed by the 128-byte metadata and sets
total bytes to 160, status to zero, consumed request bytes to 112, and metadata
bytes to 128.

The metadata contains magic `WVVP`, version and size, target, five capabilities,
the exact startup/native/transaction offsets, a 4 MiB candidate limit,
publication-transaction version 1, both exact digests, the variant at offset
112, and a 12-byte zero tail. The constructor must pass its result through the
separate variant-aware admission module. The original admission exports remain
strict wrappers for variant 0.

| Variant | Windows x64 | Linux x64 |
| ---: | --- | --- |
| `0`, publisher | `40e73f9c4ac9e27c9dea7f9bed8217be125159f89cb2ea314a91bc66da389b74` | `393253dab73387a0c96fd33c278b350fe43e5466a243eabe3f62a6652c946035` |
| `1`, promoter | `15e65b46463e118c546b847cde4b561d12390f30c772d947ad2a148477b25498` | `2a4b7b8b82dcd792781240499facfaeacb9b994dcbb89710321720fcb060dd22` |

The producer command keeps its four-argument form as variant 0 and accepts an
explicit optional first argument for either exact variant:

```text
wvhostverifierproducemetadata [variant:0|1] <target:1|2> <module.wvb> <startup.wvo> <metadata.wvvp>
```

## Ownership and evidence

The three focused Windvale source files separate admission, construction, and
the byte-input bridge. The native project front door builds 15 functions and
10,655 code bytes into a 12,025-byte WVB with SHA-256
`dcf3ec5e052a3c065fe655d2fd8a6c2daac48cc90f00e3aff01feb2f6d894043`.

The focused current-host test remains the recovery owner of service-free
`Main(bytes) -> bytes`, interpreter/native equality, exact request and metadata
hashes, committed publisher byte equality, and malformed-request boundaries.
The 12-case native publisher-file owner exercises both variant-1 targets while
retaining exact variant-0 assertions. No new C# construction semantics are
introduced.

Native symbol/evidence discovery, the startup/adapter/SHA object inputs,
Windows imports, target layout, and complete application materialization now
support both roles. Independent Linux execution and grouped promotion remain.
