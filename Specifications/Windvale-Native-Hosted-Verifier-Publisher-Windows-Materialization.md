# Windvale native hosted-verifier publisher Windows materialization

## Status and scope

This controlled downstream contract joins the generic publisher base PE,
`WVCR 1`, `WVIO 1`, exact Windows `WVVP 1` or `WVPB 1`, and successful `WVIM 1` import
response into the canonical publisher application. Inputs come from preceding
admitted native stages; completed application admission verifies exact release
identity before durable publication.

## Request and response

`WVWM 1` has a 64-byte little-endian header containing magic `WVWM`
(`0x4d575657`), version, total bytes, target 1, five `(offset, length)` pairs,
and two reserved zeros. Packed resources are the 248,832-byte base PE,
416-byte `WVCR`, 7,040-byte `WVIO`, 128-byte `WVVP`, and 4,128-byte `WVIM`.
Role 0 uses the 248,832-byte publisher base and totals 260,608 bytes; role 1
uses the 674,816-byte promoter base and totals 686,592 bytes; role 2 uses the
1,307,136-byte WVB-publisher base and totals 1,318,912 bytes.

`WVWO 1` has a 32-byte header containing magic `WVWO` (`0x4f575657`),
version, total, status, consumed input, application offset and bytes, and
target. Success appends the 256,000-byte PE. Rejection returns only the header
with status 1, 2, or 3 for envelope, input-contract, or final-size failure.

## Construction

The concrete ranges below describe role 0. Roles 1 and 2 apply the same admitted
join to the larger promoter base, relocated import page, and exact `WVCR`
placements; the materializer requires matching `WVCR`/`WVVP` roles.

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
| Base PE | 248,832 | `cf204201e5c26d71e78da1112de2bc724d389a5222cc835d48dbe8cd8bbc5988` |
| Constructor WVB | 17,799 | `4042ae5fdac77f9a0fce5a194c620ad0486455457904b30ce59648f5bb90d81b` |
| Final PE | 256,000 | `735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6` |
| Promoter final PE | 681,472 | `9cb234a57c9ff71b6ee44a0d687521e6fd7ccf82784b369e5e65b8ed40666069` |
| WVB-publisher base PE | 1,333,760 | `a06095df9ab46b3816c376c2bedc6b07c8e6aff0eaf6c92ff2c2a47d9b210466` |
| WVB-publisher final PE | 1,340,928 | `9ee91e3044193e2e90461ecf4e7ddefa4b5583f55b041b31911044c6d65b92c7` |

The focused test checks the pinned WVB and base identities, service-free native
entry, interpreter/native equality, complete final byte equality, final SHA,
and narrow rejection. Both target-specific materializers now support publisher
all three roles; independent Linux execution and broader retirement
qualification remain.
