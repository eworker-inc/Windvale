# Windvale native hosted-verifier publisher Linux materialization

## Status and scope

This controlled downstream contract joins the generic publisher base ELF,
`WVCR 1`, `WVIO 1`, and exact Linux `WVVP 1`, `WVPB 1`, or `WVPO 1` into the canonical publisher
application. The inputs come from the preceding admitted native stages; the
completed application is subsequently checked by the existing exact release
admission before durable publication.

## Request and response

`WVLM 1` has a 64-byte little-endian header containing magic `WVLM`
(`0x4d4c5657`), version, total bytes, target 2, four `(offset, length)` pairs,
and four reserved zeros. Packed resources are the 249,856-byte base ELF,
416-byte `WVCR`, 5,117-byte `WVIO`, and 128-byte `WVVP`. Role 0 uses the
249,856-byte publisher base and totals 255,581 bytes; role 1 uses the
675,840-byte promoter base and totals 681,565 bytes; role 2 uses the
1,363,968-byte WVB-publisher base and totals 1,369,693 bytes; role 3 uses the
421,888-byte WVO-publisher base and totals 427,613 bytes.

`WVLO 1` has a 32-byte header containing magic `WVLO` (`0x4f4c5657`),
version, total, status, consumed input, application offset and bytes, and
target. Success appends the role's exact ELF. Rejection returns only the
header with status 1, 2, or 3 for envelope, input-contract, or final-size
failure.

## Construction

The concrete ranges below describe role 0. Roles 1 through 3 apply the same admitted
join to the larger promoter base and uses every placement and final length from
its exact `WVCR`; the materializer requires matching construction and public
metadata roles.

The constructor captures the 28-byte format-4 note at file offset 384, clears
its old location, adds a sixth read/execute load segment, moves the note to
512, and updates its format word to 5. The new segment begins at file offset
249,856 and address 142,929,920 with 5,061 file/memory bytes and 4,096-byte
alignment.

It then emits, in order:

1. mutated 4,096-byte header page;
2. five-byte startup and 4,091 zero bytes;
3. unchanged base bytes `[8192,247264)`;
4. 128-byte `WVVP`;
5. unchanged base bytes `[247392,249856)`;
6. 3,363-byte adapter, 13 zero bytes, and 1,685-byte SHA image.

This monotonic join avoids repeated replacement of a 250 KiB byte value.

## Identities and evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Base ELF | 249,856 | `577bda8af2b1d8fca6f37e894c6b7f920e547f3e2b0bd1a28d2af518743a6629` |
| Constructor WVB | 15,551 | `9f0d6a1537717771c05dfb8c21633f6ec7a2019c0a428dcf8402d59cf1c2e8df` |
| Final ELF | 254,965 | `8c9a1dbbb177041c61e4606696ce9ddf9225a98407a7d3af0a4338069a15979e` |
| Promoter base ELF | 675,840 | `848ee9ed30ffc5094f77b4f79b72e3b4a426b4f9e0fc8e26631ed6619596f782` |
| Promoter final ELF | 680,949 | `3cd1c82807495e34445345b5e61b8c5911434c84d2a6f49a11b21fd2521423f5` |
| WVB-publisher base ELF | 1,363,968 | `2fc0332887c96ad0fa34d1987091d60ddbbe61f019739d41734cd491b8ca4b64` |
| WVB-publisher final ELF | 1,369,077 | `b8efb90f7d7c4eae99de01df6c0a3c24a7396d9b9e717ff69d005282ed3d63af` |
| WVO-publisher base ELF | 421,888 | `af61a601f4cd8e7fb81704353160a518d2e4f199084fde4b29518d27c89774f7` |
| WVO-publisher final ELF | 426,997 | `2889237d7fdb20b1d420c05834f19183d18b02112e3f4eea0ed7ff43414814f2` |

The focused test checks the pinned WVB and base identities, service-free native
entry, interpreter/native equality, complete final byte equality, final SHA,
and narrow rejection. [Decision 0480](../Documents/Decisions/0480-Native-WVHV-Publisher-Windows-Materialization.md)
adds the Windows PE counterpart. All four roles now run through the ordinary native
pipeline; independent Linux execution and the broader retirement gates remain.
