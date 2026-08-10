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
1,335,296-byte WVB-publisher base and totals 1,341,021 bytes; role 3 uses the
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
| Base ELF | 249,856 | `687338281ca78c9d3a4d08b601c1efbcc198ec3c8fcc96fbf34f5dc349cafae2` |
| Constructor WVB | 15,551 | `0384c8a4e5d8acbed587aa2e31a21832f56a0e71243260d9a213940b589373cd` |
| Final ELF | 254,917 | `babe721a573e29f89ec095c35677880077ff465d4e2129063f6742cd47591a97` |
| Promoter base ELF | 675,840 | `768ca223c99e901d17a1c5d86744515e4b571a6feae329fb6fc3cf225215a133` |
| Promoter final ELF | 680,901 | `422332fb4f2824ae558bf93adadb6470597399d07810f5428f71aa4d971a4f58` |
| WVB-publisher base ELF | 1,335,296 | `f53a4c8c5d292e999735cf5fd337b7c6997c0a8e6d2ba316ec94cd6b0838b090` |
| WVB-publisher final ELF | 1,340,357 | `7f2dbfaecf2734c5afdbd6e2e54263a5a74038b8a498eeb1e155ee71788b630c` |
| WVO-publisher base ELF | 421,888 | `af61a601f4cd8e7fb81704353160a518d2e4f199084fde4b29518d27c89774f7` |
| WVO-publisher final ELF | 426,949 | `4b0ce2d332648e3dd572596db4490748bf62ee4448a9550d83c152de60f7e51d` |

The focused test checks the pinned WVB and base identities, service-free native
entry, interpreter/native equality, complete final byte equality, final SHA,
and narrow rejection. [Decision 0480](../Documents/Decisions/0480-Native-WVHV-Publisher-Windows-Materialization.md)
adds the Windows PE counterpart. All four roles now run through the ordinary native
pipeline; independent Linux execution and the broader retirement gates remain.
