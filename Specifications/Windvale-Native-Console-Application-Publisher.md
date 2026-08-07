# Windvale native console-application publisher

## Status and scope

`WVPA 1` is the bounded Windvale-native atomic publisher for completed version-1
Windows and Linux console applications. It admits one immutable `.exe` or `.elf`
candidate through the portable
[console-application verifier](Windvale-Console-Application-Verification.md),
then reuses the exact native
[publication transaction](Windvale-Wvb-Publication-Transaction.md) to replace a
distinct same-kind destination.

The publisher does not construct PE or ELF, lower WVB, or define another
filesystem transaction. The existing native adapters own immutable candidate
snapshot identity, destination-directory anchoring, exclusive sibling creation,
complete durable write, exact reread and SHA-256 comparison, native file-identity
alias rejection, atomic replacement, directory durability, and scratch cleanup.

## Windvale module and command

`Windvale-Console-Application-Publisher.wvproj` builds the hosted module
`Windvaleˉconsoleˉapplicationˉpublisherˉtool`. It exports only `Main() -> i32`
and declares these exact capabilities:

1. `console.write_line`;
2. `diagnostic.write_line`;
3. `file.read_bytes`;
4. `process.argument`;
5. `process.argument_count`.

The raw application accepts:

```text
wvappublish <candidate.exe|candidate.elf> <destination.exe|destination.elf>
```

Both paths must use the same recognized suffix and must be textually distinct.
The native adapter separately rejects candidate/destination aliases by file
identity. The candidate must fit in one bounded 4 MiB snapshot; this covers every
output of the current one-value native console packager but not the larger
segmented maximum admitted by the portable verifier.

The Windvale entry point passes the candidate and an empty second chunk to
`Consoleˉapplicationˉverification`. Any non-`Valid` result reports
`publication status=Rejected phase=console-application`, returns 1, and never
begins mutation. Wrong arguments or suffixes report usage and return 64.

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

- `windows-x64-console-application-publisher-v1`, producing `.exe`;
- `linux-x64-console-application-publisher-v1`, producing `.elf`.

Both reuse the qualified WVB publisher startup, Windows/Linux publication
adapter, SHA-256 object, five-capability native service bundle, and platform
container mechanics. No new WVA or platform mutation implementation is added.
The historical exported WVA symbol names remain internal construction details.

`WVPA 1` is a 128-byte little-endian metadata record:

| Offset | Field |
| ---: | --- |
| 0 | Magic `WVPA`, integer `0x41505657` |
| 4 | Version, `1` |
| 8 | Record bytes, `128` |
| 12 | Console-application target |
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
| Publisher WVB | 56,375 | `1e35f7cc9e53322ebcc70c332486eef983ff59370246c62ec4e8cbcd144d8403` |
| Windows publisher | 642,048 | `1bd3bbd24fc22940b96badb7e809899d42e42a25a5247dfededb00048232675d` |
| Linux publisher | 639,941 | `2edc7ebe23660e299d9db4bf55d4537ec102b7a3b2d46ba833e549cd355a0af7` |

`Tools/Native/Publish-Console.cmd` and `.sh` verify the current-host publisher
digest before execution. `Package-Console.cmd` and `.sh` now materialize into a
private candidate and invoke this publisher for the requested destination. The
direct publisher retains its completion transcript; the composite launchers
suppress only that success line so the packager's established report remains
the complete command output. Publisher diagnostics and failure status remain
visible.

The shared [fixed native publisher rejection contract](Windvale-Native-Publisher-Rejection-Tests.md)
now exercises invalid console-application admission through this exact launcher,
requiring destination preservation and zero scratch without rebuilding the
publisher or invoking .NET.

The separate [console-container hostile-input contract](Windvale-Native-Console-Container-Hostile-Input-Tests.md)
drives 128 immutable PE candidates and 128 immutable ELF candidates through
this same public launcher. It requires exact rejection, complete candidate and
destination preservation, and zero scratch without executing either container,
generating runtime input, or consulting .NET.

## Remaining gate

The WVB and paired applications are Stage 0-constructed because the qualified
native source builder currently rejects the complete hosted project at
`Sourceˉbindings`. Promotion requires deterministic reconstruction and direct
execution on Windows and Linux, the grouped native-retirement gate, and a native
replacement for this host-container constructor. The retained WVB publisher
fault/concurrency matrix continues to qualify the shared native transaction;
this profile adds console-application admission and integration evidence rather
than duplicating those tests.
