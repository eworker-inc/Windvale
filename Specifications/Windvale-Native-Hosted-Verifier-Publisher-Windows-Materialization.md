# Windvale native hosted-verifier publisher Windows materialization

## Status and scope

This controlled downstream contract joins the generic publisher base PE,
`WVCR 1`, `WVIO 1`, exact Windows `WVVP 1`, `WVPB 1`, or `WVPO 1`, and successful
`WVIM 1` import response into the canonical publisher application. Inputs come from preceding
admitted native stages; completed application admission verifies exact release
identity before durable publication.

## Request and response

`WVWM 1` has a 64-byte little-endian header containing magic `WVWM`
(`0x4d575657`), version, total bytes, target 1, five `(offset, length)` pairs,
and two reserved zeros. Packed resources are the 248,832-byte base PE,
416-byte `WVCR`, 7,040-byte `WVIO`, 128-byte `WVVP`, and 4,128-byte `WVIM`.
Role 0 uses the 248,832-byte publisher base and totals 260,608 bytes; role 1
uses the 674,816-byte promoter base and totals 686,592 bytes; role 2 uses the
1,333,760-byte WVB-publisher base and totals 1,345,536 bytes; role 3 uses the
422,912-byte WVO-publisher base and totals 434,688 bytes.

`WVWO 1` has a 32-byte header containing magic `WVWO` (`0x4f575657`),
version, total, status, consumed input, application offset and bytes, and
target. Success appends the role's exact PE. Rejection returns only the header
with status 1, 2, or 3 for envelope, input-contract, or final-size failure.

## Construction

The concrete ranges below describe role 0. Roles 1 through 3 apply the same admitted
join to the larger promoter base, relocated import page, and exact `WVCR`
placements; the materializer requires matching construction and public metadata
roles.

The constructor updates PE entry, image, import, IAT, relocation, and section
geometry in the 512-byte header. It emits, in order:

1. mutated PE header;
2. five-byte startup and 4,091 zero bytes;
3. unchanged 235,394-byte bundle and 14 zero bytes;
4. 5,286-byte adapter, 10 zero bytes, 1,685-byte SHA image, and 299 zeros;
5. exact 4,096-byte publisher import page;
6. 1,504 unchanged runtime bytes, 128-byte `WVVP`, and the remaining 2,976
   runtime/relocation bytes.

The resulting adapter, SHA, import, metadata, and relocation file offsets are
240,016, 245,312, 247,296, 252,896, and 255,488. This monotonic join avoids
repeated replacement of the full 256 KiB value.

## Identities and evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Base PE | 248,832 | `2afd9d92422b063abd3cd20d8da6056efbbbff9e7ac8baeef9c8b60b391686c5` |
| Constructor WVB | 18,939 | `062d06db7f3e591740e66f1c2a543b4096baee0f7057fc79d3a8e63a45be66c0` |
| Final PE | 256,000 | `17cb5c4228e8448693b17f1b73695fd0ecfd03d7ada922794a5bf3bd7594fc96` |
| Promoter base PE | 674,816 | `927476ca389c7449fb0c72341f26d68577a6a9e0c0ed02fa45ac8c4af935c77f` |
| Promoter final PE | 681,472 | `598bd2de8247abd19d931efa1edcc8323adef7f56da51da1d41256933667eb23` |
| WVB-publisher base PE | 1,333,760 | `8fcdcfc755439ebae5086c72d88113fb52f397ba0687c785af247230a7732fff` |
| WVB-publisher final PE | 1,340,928 | `71794a6a254ccfd652ffe3bad556c32f86e2d9210a5a3099bad576f97476a8f3` |
| WVO-publisher base PE | 422,912 | `1f9361126c368f133693222cbaa4c21e2d0948e79df7bf945b7b037ac815e884` |
| WVO-publisher final PE | 430,080 | `ad4c2a05115b2acdb074c0f53b6d7470c8bcacfdfea86583043bdd0ff511188a` |

The focused test checks the pinned WVB and base identities, service-free native
entry, interpreter/native equality, complete final byte equality, final SHA,
and narrow rejection. Both target-specific materializers now support all four
roles; independent Linux execution and broader retirement
qualification remain.
