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

`Projects/Tools/Windvale-Wvo-Publisher.wvproj` builds the hosted module
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
| Publisher WVO | 408,284 | `29c1cc269b9387944b4d43fe9215392044996ad47da55be45a1d177f26e5bafb` |
| Windows publisher | 430,080 | `76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910` |
| Linux publisher | 426,997 | `2889237d7fdb20b1d420c05834f19183d18b02112e3f4eea0ed7ff43414814f2` |

`Tools/Native/Publish-Wvo.cmd` and `.sh` verify the current-host publisher
digest before execution. `Lower-Wvb-To-Wvo.cmd` and `.sh` now lower into a
private candidate and invoke this publisher for the requested destination. The
direct publisher retains its completion transcript; the composite launchers
suppress only that success line so the lowerer's established report remains the
complete command output. Publisher diagnostics and failure status remain
visible.

The shared [fixed native publisher rejection contract](Windvale-Native-Publisher-Rejection-Tests.md)
now exercises invalid WVO admission through this exact launcher, requiring
destination preservation and zero scratch without rebuilding the publisher or
invoking .NET.

The separate [hostile-size contract](Windvale-Native-Wvo-Hostile-Size-Tests.md)
requires the first file beyond the 4-MiB snapshot limit to fail before the
transaction while preserving the candidate, destination, and scratch boundary.

## Native reconstruction

[Decision 0499](../Documents/Decisions/0499-Native-Wvo-Publisher-Reconstruction.md)
extends the existing hosted-verifier publisher-construction pipeline with an
exact role 3. `Tools/Native/Construct-Wvo-Publisher.cmd` and `.sh` reconstruct
either target application on the current Windows host through retained native
compiler, lowerer, linker, hosted-container, and publisher-construction
toolsets. The route invokes the digest-bound raw lowerer rather than the public
lower-and-publish wrapper, then requires the resulting 408,284-byte WVO to
match the exact retained oracle before linking. This keeps the target WVO
publisher out of its own object-publication path.

The fixed `wvo-publisher-reconstruction` owner passes 2/2 on the current
Windows host: exact candidate inventory followed by native WVB and paired
application byte equality. The shared 15-case publisher pipeline also remains
exact after the role-3 extension.

## Remaining gate

The current Windows host now reconstructs the exact WVB-to-WVO-to-paired-
application closure without a managed writer. This is retained-seed cross-target
construction evidence, not independent Linux execution, clean bootstrap,
qualification, or promotion. Direct reconstruction and execution on Linux, the
grouped native-retirement gate, candidate promotion, and final Stage 0 recovery
release remain. The retained WVB publisher fault/concurrency matrix continues
to qualify the shared native transaction; this profile adds WVO admission and
lowerer integration evidence rather than duplicating those tests.
