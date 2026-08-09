# Windvale native hosted-verifier publisher Windows materialization

## Status and scope

This controlled downstream contract joins the generic publisher base PE,
`WVCR 1`, `WVIO 1`, exact Windows `WVVP 1`, and successful `WVIM 1` import
response into the canonical publisher application. Inputs come from preceding
admitted native stages; completed application admission verifies exact release
identity before durable publication.

## Request and response

`WVWM 1` has a 64-byte little-endian header containing magic `WVWM`
(`0x4d575657`), version, total bytes, target 1, five `(offset, length)` pairs,
and two reserved zeros. Packed resources are the 248,832-byte base PE,
416-byte `WVCR`, 7,040-byte `WVIO`, 128-byte `WVVP`, and 4,128-byte `WVIM`.
Total request bytes are 260,608.

`WVWO 1` has a 32-byte header containing magic `WVWO` (`0x4f575657`),
version, total, status, consumed input, application offset and bytes, and
target. Success appends the 256,000-byte PE. Rejection returns only the header
with status 1, 2, or 3 for envelope, input-contract, or final-size failure.

## Construction

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
| Constructor WVB | 15,431 | `73786b8bb60f8dc472c8ff111104480e16d1ac46e485125713a3fa4159aee633` |
| Final PE | 256,000 | `735320b5ff33419d685925044add6f254bf402c0d49fc575c77f6110fac705f6` |

The focused test checks the pinned WVB and base identities, service-free native
entry, interpreter/native equality, complete final byte equality, final SHA,
and narrow rejection. Both target-specific final materializers now exist;
ordinary native pipeline wiring and broader retirement qualification remain.
