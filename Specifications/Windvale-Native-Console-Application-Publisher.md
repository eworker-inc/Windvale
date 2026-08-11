# Windvale native console-application publisher

## Status and scope

`WVPA 1` is the bounded Windvale-native atomic publisher for completed
version-1 and format-2 hosted Windows and Linux console applications. It admits
one immutable `.exe` or `.elf` candidate through the portable
[console-application admission boundary](Windvale-Console-Application-Verification.md),
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
`Consoleˉapplicationˉadmission`. Exact format-2 markers select the focused
hosted verifier; all other candidates retain the version-1 recipe verifier.
Any non-`Valid` result reports
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

The public recovery construction targets are:

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
| Publisher WVB | 115,107 | `e8121fb76c7cc39b159d53a3c28d1da8bc2d44968d630495c692a7761656923d` |
| Publisher WVO oracle | 1,139,440 | `259c7d746c3a217c32706bfd617cf66894066bd2e50850cbe5733ac3338e4952` |
| Linked fragment | 1,135,424 | `c6b199644be8ca19cce0110a5090e84c736220a130f9b48a4366caf36254e6e2` |
| Windows profile base | 1,151,488 | `23bf32201666f99af52015d9b3c10ab27d48f088cb766c8701f3f1973b7ab69b` |
| Linux profile base | 1,150,976 | `a12ab6d136b53c53322d4b7ff612a5f41a2653c30210a4f5dbfb27027bc29f5e` |
| Windows publisher | 1,158,656 | `0bafe84096859f4b88dc14be92c6cdc5336d791b7c5b0a332dccb76b913dd24e` |
| Linux publisher | 1,156,085 | `e9b8771978c9fb06c3a8ecc55c7b9a3ba1acd24faa541dc669920c10ed792925` |

The WVB rebuilds through the native Project 1 front door after canonicalizing
its order-independent source inventory. Decision 0503 lowers that exact WVB
through the retained raw accepted-subset lowerer and requires the complete WVO
oracle above. The WVO has 1,134,976 text bytes, 448 read-only-data bytes, 109
symbols, and 15 relocations. `Main` is symbol 108 at offset 18,902 with 5,436
bytes; the private apply and begin functions are symbols 14 and 15 at offsets 0
and 789 with 789 and 389 bytes respectively.

The current Windows native constructor links that admitted object once and
uses shared publisher-overlay variant 4 to construct both target applications.
Variant 4 fixes the target-specific startup, metadata, base, object, import,
and final identities while the embedded public metadata remains `WVPA 1` with
stored role zero. The route invokes the raw lowerer rather than either target
publisher, so neither final application publishes or constructs itself.

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

The [hosted-console mutation contract](Windvale-Native-Hosted-Console-Container-Mutation-Tests.md)
additionally drives two valid format-2 applications and thirteen exact managed
mutation operations through this launcher. Valid candidates must publish
exactly; invalid candidates must preserve the existing destination and leave no
scratch. Permanent execution does not consult .NET.

## Remaining gate

The exact WVB, WVO oracle, linked fragment, profile bases, and paired
applications now reconstruct on the current Windows host through retained
Windvale-native compiler, lowerer, linker, hosted-container, and role-aware
publisher-construction tools. This is same-release cross-target evidence, not a
clean bootstrap or promotion transaction. Independent Linux reconstruction and
execution, grouped qualification, ordinary-path promotion, and the final Stage
0 recovery release remain. The final candidate refresh binds the current
file-input leaf and replaces its stale application bytes and digests without
changing the WVB, WVO, fragment, bases, or public `WVPA 1` contract. The retained
WVB publisher fault/concurrency matrix continues to qualify the shared native
transaction; this profile adds
console-application admission and integration evidence rather than duplicating
those tests.

The focused current-Windows-host owner passes 3/3 in 68.6 seconds, including
exact reconstruction and independent version-1 publication/rejection
preservation. The established shared publisher pipeline also passes 15/15 in
188.7 seconds, preserving exact roles 0 through 3. Linux execution, grouped
qualification, promotion, clean bootstrap, and recovery deletion remain open.
