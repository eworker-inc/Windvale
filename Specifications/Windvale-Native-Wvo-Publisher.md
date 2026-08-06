# Windvale native WVO publisher

## Status and scope

`WVPO 1` is the bounded Windvale-native atomic publisher for one complete WVO
1.0 object. It admits an immutable `.wvo` candidate through the same portable
verification module used by the native inspector, then reuses the exact native
[publication transaction](Windvale-Wvb-Publication-Transaction.md) to replace a
distinct `.wvo` destination.

The publisher does not lower WVB, construct WVO, interpret WVA, or define a
second filesystem transaction. The existing native adapters own immutable
candidate snapshot identity, destination-directory anchoring, exclusive sibling
creation, complete durable write, exact reread and SHA-256 comparison, native
file-identity alias rejection, atomic replacement, directory durability, and
scratch cleanup.

## Windvale module and command

`Windvale-Wvo-Publisher.wvproj` builds the hosted module
`Windvaleˉwvoˉpublisherˉtool`. It exports only `Main() -> i32` and declares
these exact capabilities:

1. `console.write_line`;
2. `diagnostic.write_line`;
3. `file.read_bytes`;
4. `process.argument`;
5. `process.argument_count`.

The raw application accepts:

```text
wvopublish <candidate.wvo> <destination.wvo>
```

Both paths must have a `.wvo` suffix and be textually distinct. The native
adapter separately rejects candidate/destination aliases by file identity. The
candidate must fit in one bounded 4 MiB snapshot.

`Object-Model/Windvale/Wvo-Object-Verification.wv` owns the complete portable
WVO admission path, including bounded strict UTF-8 name validation. The
inspector shell and this publisher import that one implementation; publication
does not introduce a second WVO parser or require a text service. Invalid bytes
report `publication status=Rejected phase=wvo`, return 1, and never begin
mutation. Wrong arguments or suffixes report usage and return 64.

After admission, the native adapter performs the exact transaction:

```text
snapshot -> exclusive sibling -> durable write -> exact reread
         -> atomic replacement -> directory durability -> complete
```

Success returns 0 and reports exact bytes and SHA-256. A pre-replacement failure
leaves the destination known unchanged after bounded cleanup. A failure after
replacement remains explicitly indeterminate and must not be retried blindly.

## Container contract

The public Stage 0 construction targets are:

- `windows-x64-wvo-publisher-v1`, producing `.exe`;
- `linux-x64-wvo-publisher-v1`, producing `.elf`.

Both reuse the qualified WVB publisher startup, Windows/Linux publication
adapter, SHA-256 object, five-service native bundle, and platform container
mechanics. No new WVA or platform mutation implementation is added. The
historical exported WVB publication bridge names remain internal construction
details.

`WVPO 1` is a 128-byte little-endian metadata record:

| Offset | Field |
| ---: | --- |
| 0 | Magic `WVPO`, integer `0x4F505657` |
| 4 | Version, `1` |
| 8 | Record bytes, `128` |
| 12 | WVO publisher target |
| 16 | Capability count, `5` |
| 20 | Startup bytes |
| 24 | Startup entry offset |
| 28 | Windvale native entry offset |
| 32 | Transaction-begin function offset |
| 36 | Transaction-apply function offset |
| 40 | Candidate snapshot limit, `4,194,304` |
| 44 | Publication-transaction version, `1` |
| 48 | Startup SHA-256, 32 bytes |
| 80 | Publisher WVB SHA-256, 32 bytes |
| 112 | Reserved zero bytes |

## Candidate identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Publisher WVB | 41,365 | `4e8c81da38f5eb06f9334c2d2c5e35120a13e73bac3a9375b5e6a2eff04438c5` |
| Windows publisher | 430,080 | `035a1baaada6be8d057b782804a8650d978da53dd008337ab00258f2ab597cb7` |
| Linux publisher | 426,949 | `ac2bb513e2145e9eb911a9be142fc2f1f990a1bab21f278dd841043042b51f7a` |

`Tools/Native/Publish-Wvo.cmd` and `.sh` verify the current-host publisher
digest before execution. `Lower-Wvb-To-Wvo.cmd` and `.sh` now lower into a
private candidate and invoke this publisher for the requested destination.

## Remaining gate

The WVB and paired applications are Stage 0-constructed because the qualified
native source builder currently rejects the complete hosted project at
`Sourceˉbindings`. Promotion requires deterministic reconstruction and direct
execution on Windows and Linux, the grouped native-retirement gate, and a native
replacement for this host-container constructor. The retained WVB publisher
fault/concurrency matrix continues to qualify the shared native transaction;
this profile adds WVO admission and lowerer integration evidence rather than
duplicating those tests.
