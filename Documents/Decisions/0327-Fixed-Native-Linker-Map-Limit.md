# Decision 0327: Fixed native linker map limit

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0311](0311-Fixed-Native-Linker-Rejections.md), and [Decision 0325](0325-Expanded-Native-Linker-Rejection-Families.md)
- Contract: [Native Windvale linker](../../Specifications/Windvale-Native-Wv-Linker.md#fixed-native-map-limit-contract)

## Context

Decision 0325 transfers every externally driven compact `WVL1001` through
`WVL1010` family to the digest-bound native linker. `WVL1012` still depends on
a managed test that constructs four valid objects with 16,384 definitions and
runs the Windvale linker through the interpreted evidence path. Repeating that
construction in WVA would add very large source fixtures or spend about a
minute assembling each generated object on the current Windows candidate.

The boundary needs independent permanent evidence, but it does not justify a
large committed source file or another successful AOT-chain execution.

## Decision

- Add separate `Test-Linker-Map-Limit.cmd` and `.sh` commands that invoke only
  the digest-bound native linker after fixture admission.
- Define two generated canonical WVO fixtures: one empty `.text` object with
  local functions `L0000` through `L4095`, and one otherwise identical object
  ending at `L4094`. Store them together in one base64-encoded gzip tar archive
  rather than retaining their mechanically repeated WVA source.
- Link the existing canonical `Main` WVO, the 4,096-local object three times,
  and the 4,095-local object once. The five valid inputs contain exactly 16,384
  definitions and reach canonical-map rejection without crossing aggregate WVO
  limits first.
- Require exit `2`, empty standard output, the complete LF-terminated `WVL1012`
  report, unchanged entry and fixture objects, and preservation of the existing
  479-byte destination sentinel.
- Keep `WVL1011` with internal reconstruction-mismatch and corruption evidence;
  it is not an externally selectable malformed-link input.

## Evidence and consequences

- The compressed archive is 21,046 bytes at SHA-256
  `1c6227931496f54c93677b4dfecfbfa256214a5da72ecfd05d441e49c809e27d`;
  its LF base64 representation is 28,065 bytes. The extracted objects are
  102,449 and 102,424 bytes at SHA-256
  `a05c4f51be960c7fc900d8cc9fc39dbc525ccd0b2b1a4c55b12ca8396107ee75`
  and
  `398737cfd465fb976e6319ce7ddc4dbefb9e082d39432d09474cf75f8aafffdc`.
- The exact report is
  `link status=WVL1012 inputs=5 sections=5 symbols=16384 relocations=0 image-bytes=0 entry-address=0 input=4294967295`
  plus LF, at SHA-256
  `097ad88fa0e4fd48504da8d69516e47ff7f6b5979fccf186e0307b814b5af86e`.
- Direct Windows execution passes 1/1 in about 1.0 second. After reviewing the
  command and wrapper, the exact Seed selection
  `native linker map limit rejects without .NET` passes 1/1 in 1.008 seconds
  after an 11.93-second zero-warning Release build; the complete command takes
  17.4 seconds.
- The permanent command starts no .NET process, rebuilds no product or fixture,
  and commits no large generated WVA source. No linker implementation, WVO
  format, WebAssembly implementation, or candidate artifact changes.
- Linux execution and the grouped end-of-goal Windows/Linux Qualification gate
  remain deferred. This local slice does not promote or delete Stage 0.

## Reconsideration triggers

Regenerate the archive and all pinned identities if WVO 1, the canonical-map
format, the exact definition boundary, or the fixture naming rule changes. Keep
the fixture mechanically specified and compressed; do not replace it with
numbered fragments or a large generated source merely to expose repetition.
