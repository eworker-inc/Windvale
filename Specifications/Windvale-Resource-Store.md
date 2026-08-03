# Windvale read-only resource store

## Status and purpose

`WVRS 1` is the implemented-candidate deterministic read-only resource-store contract owned by [Decision 0126](../Documents/Decisions/0126-First-Read-Only-Resource-Store.md). It packages a bounded set of typed immutable resources behind opaque strict-UTF-8 names and gives portable Windvale code an independently validated lookup algorithm.

This is a resource image, not a general filesystem. It has no paths, directories, handles, metadata queries, mutation, block-device behavior, or ambient namespace. Its immediate purpose is to pressure dynamic name lookup with a real third resource without combining IPC, storage drivers, writable state, and crash consistency in one experiment.

## Limits

| Limit | Value |
| --- | ---: |
| Complete store | 4,194,304 bytes |
| Entries | 1 through 64 |
| Encoded resource name | 1 through 1,024 bytes |
| One resource value | 0 through 4,194,304 bytes, further bounded by the complete store |
| Digest | 64 lowercase ASCII SHA-256 characters |

Names are opaque ordinal strict-UTF-8 values. NUL is invalid. A name is not split on `/`, `\`, `:`, `.`, or any other character, and Windvale code does not apply host path, case, normalization, current-directory, or separator rules.

## Binary format

Every integer is unsigned little-endian. The complete image contains a 32-byte header, a fixed-width entry directory, a packed name region, and a packed data region. There is no alignment padding or trailing data.

### Header

| Offset | Bytes | Meaning |
| ---: | ---: | --- |
| `0` | 4 | Magic `WVRS` |
| `4` | 4 | Format version `1` |
| `8` | 4 | Exact complete image bytes |
| `12` | 4 | Entry count |
| `16` | 4 | Directory bytes, exactly `entry count * 96` |
| `20` | 4 | Packed name bytes |
| `24` | 4 | Packed resource-data bytes |
| `28` | 4 | Reserved zero |

The name region begins at `32 + directory bytes`. The data region begins immediately after the name region. Header arithmetic is checked before either region is read, and the four regions must cover the exact input.

### Directory entry

Each entry is 96 bytes:

| Relative offset | Bytes | Meaning |
| ---: | ---: | --- |
| `0` | 4 | Nonzero store-local identifier |
| `4` | 4 | Resource kind |
| `8` | 4 | Attributes, exactly `7` |
| `12` | 4 | Absolute name offset |
| `16` | 4 | Name bytes |
| `20` | 4 | Absolute data offset |
| `24` | 4 | Data bytes |
| `28` | 4 | Reserved zero |
| `32` | 64 | Lowercase ASCII SHA-256 of the exact data bytes |

Kinds are `1` `wvb-module`, `2` `u32-execution-budget`, and `3` `opaque-bytes`. Attribute bits are `1` immutable, `2` read-only, and `4` no-execute; version 1 requires all three and permits no other bit.

Identifiers are unique but do not define directory order. Entries are strictly sorted by unsigned ordinal encoded-name bytes. Their names occupy the name region consecutively in that order, and their data occupies the data region consecutively in the same order. Offsets must therefore equal the running canonical cursors. Duplicate names, duplicate or zero identifiers, gaps, overlaps, reordered payloads, invalid UTF-8, NUL, digest mismatch, and trailing bytes are invalid.

## Construction and verification

The Stage 0 writer accepts initialized typed entries, validates limits and identities, sorts by encoded name bytes, emits exact offsets and SHA-256 text, then reparses the result through the independent strict verifier. Caller order cannot affect output bytes.

The verifier treats the complete image as untrusted. It validates extent, magic/version, header coverage, count and directory arithmetic, identifiers, kinds and attributes, canonical name/data cursors, strict ordering, strict UTF-8, reserved fields, and every digest before publishing `Verifiedˉresourceˉstore`. Lookup on that result uses exact ordinal names and exposes no native path.

Stage 0 format failures use `WVRS1001` through `WVRS1008`; invalid construction uses `WVRS2001` through `WVRS2003`. The code and byte offset identify the rejected boundary.

## Windvale-owned lookup

[`Resource-Store-Core.wv`](../Operating-System/Services/Resource-Store-Core.wv) independently validates `WVRS 1` and returns this bounded result:

```text
Resourceˉstoreˉresult(
    Status,
    Identifier,
    Kind,
    Attributes,
    Value,
    Failureˉoffset
)
```

Statuses distinguish invalid size, magic, version, header, identifier, kind, attributes, name, order, data, digest, not-found, and invalid-query cases. Failure returns an empty value. Success returns an immutable slice of the supplied store after every entry and digest has been validated; lookup never returns early from a partially validated image.

[`Resource-Store-Service.wv`](../Operating-System/Services/Resource-Store-Service.wv) is the first hosted wrapper. It declares only `file.read_bytes`, reads opaque resource `boot:resources.wvrs`, resolves third typed resource `boot:main.configuration`, and checks its identifier, kind, attributes, length, and bytes. Declaration, authorization, adapter implementation, store verification, and name lookup remain separate boundaries.

## Implementation evidence and limits

The focused OS suite constructs one canonical three-entry store containing a real WVB, a four-byte execution budget, and configuration bytes `[3,5,8,13]`. It proves caller-order-independent bytes, Stage 0/Windvale agreement, authorization and missing-store failures, targeted malformed fields, digest failure, unknown lookup, and containment of 256 deterministic hostile inputs.

This candidate does not change protected-process version 11, `WVRES004`, `WVBR002`, the one-`u32` channel, the current boot image, or pinned QEMU identities. The hosted wrapper is evidence for service policy and portable format ownership, not evidence that Windvale OS already supplies a dynamic service namespace.

[`WVRQ 1` / `WVRY 1`](Windvale-Resource-Service-Ipc.md) now supplies the separate implemented-candidate one-page request/reply and explicit exchange-lifecycle boundary. Guest adoption still requires a protected-process/channel ABI, checked buffer copies, an immutable store capability, service/client death propagation, and QEMU cleanup evidence. A later block driver may supply store bytes, but its device and DMA authority must remain outside this format. Writable files, directories, replacement, allocation, crash recovery, caches, and an on-disk filesystem require separate measured contracts.
