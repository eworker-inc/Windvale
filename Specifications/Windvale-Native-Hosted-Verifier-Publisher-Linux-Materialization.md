# Windvale native hosted-verifier publisher Linux materialization

## Status and scope

This controlled downstream contract joins the generic publisher base ELF,
`WVCR 1`, `WVIO 1`, and exact Linux `WVVP 1` or `WVPB 1` into the canonical publisher
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
1,306,624-byte WVB-publisher base and totals 1,312,349 bytes.

`WVLO 1` has a 32-byte header containing magic `WVLO` (`0x4f4c5657`),
version, total, status, consumed input, application offset and bytes, and
target. Success appends the 254,917-byte ELF. Rejection returns only the
header with status 1, 2, or 3 for envelope, input-contract, or final-size
failure.

## Construction

The concrete ranges below describe role 0. Roles 1 and 2 apply the same admitted
join to the larger promoter base and uses every placement and final length from
its exact `WVCR`; the materializer requires matching `WVCR`/`WVVP` roles.

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
| Base ELF | 249,856 | `0bdeee07a49f75781767934884cbbc7dd085abff4507e2f78210fa225638539a` |
| Constructor WVB | 14,349 | `432cc05edc7708d96092063c2d256d01dcad19f32f934bdcf7d1515abdeefb59` |
| Final ELF | 254,917 | `de4f06f6d837eb58457a31b4757c3410e389ecc3c11fd79daf229dbdeb23e02a` |
| Promoter final ELF | 680,901 | `9406a1e2610db48e744a0912ab4abb2281856e92f7a0d870292c16105d9b9af0` |
| WVB-publisher base ELF | 1,306,624 | `c2f710921da8b2f39a8f927b0054a59f00957b9cfc449a687dd600eb9e508427` |
| WVB-publisher final ELF | 1,311,685 | `3bb76b7ab4f5f5a00d9f949e70a65d49aac7b0973856e6a6148f2a9a5ca38c72` |

The focused test checks the pinned WVB and base identities, service-free native
entry, interpreter/native equality, complete final byte equality, final SHA,
and narrow rejection. [Decision 0480](../Documents/Decisions/0480-Native-WVHV-Publisher-Windows-Materialization.md)
adds the Windows PE counterpart. All three roles now run through the ordinary native
pipeline; independent Linux execution and the broader retirement gates remain.
