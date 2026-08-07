# Decision 0339: Fixed native WVO hostile size

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0301](0301-Digest-Bound-Native-Wvo-Candidate-Launchers.md), [Decision 0302](0302-Digest-Bound-Native-Wvo-Linker-Candidate.md), [Decision 0308](0308-Native-Wvo-Publication.md), and [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md)
- Contract: [Native WVO hostile-size tests](../../Specifications/Windvale-Native-Wvo-Hostile-Size-Tests.md)

## Context

The fixed WVO family matrix and differential/containment corpora all fit inside
one ordinary Windvale `bytes` value. Stage 0 separately rejects one zero-filled
standard WVO input of `MAX_OBJECT_BYTES + 1`, but the native read-only, linker,
and publisher tools acquire inputs through the deliberately bounded
`file.read_bytes` service.

Passing a smaller malformed object would test the inner scanner again and would
not transfer the hostile-size contract. Claiming an inner native WVO diagnostic
would also be false because a 4,194,305-byte value cannot enter Windvale under
the current language/runtime limit.

## Decision

- Freeze the exact zero-filled 4,194,305-byte Stage 0 input in one compact
  digest-bound archive.
- Treat the outer native snapshot boundary as the correct ordinary-path owner.
  Require process result `1`, empty channels, and complete input preservation.
- Exercise all four distinct normal WVO consumers: verify, inspect, link, and
  publish. Additionally preserve the link output and publication destination,
  and require zero publisher scratch.
- Retain `WVO1001` and `WVL1002` as Stage 0 recovery provenance, not as native
  process reports. The native service's too-large classification remains
  `WVR3025`, normalized by the current startup to silent result `1`.
- Keep 32-MiB large-native and any future segmented WVO inspection as separate
  contracts. Do not widen the ordinary byte-value limit for a test.

## Evidence and consequences

- The exact input is 4,194,305 bytes at SHA-256
  `95e441ca65cd41fa01b2a71799e79fd60db59ed34f13af32a91e85f90378676c`.
  Its 4,178-byte archive has SHA-256
  `4c9e5ed9aa6a822c64e799378ede641d86c37a6cc639003286afd2277144ef89`.
- Pre-run review verifies the archive's sole safe member, expanded identity,
  all four pinned launchers, Windows CRLF, shell LF and Bash syntax, and the
  absence of any permanent managed invocation.
- The direct Windows command passes 4/4 in 1.375 seconds. Both read-only modes,
  the linker, and publisher return the exact outer-boundary behavior; all inputs
  and sentinels remain unchanged and publication leaves no scratch.
- The retirement plan is now 1,533 LF-only bytes at SHA-256
  `83b6bab28343d2a6c4c3c4e9c69991512d8663baa628e82421e59b42748fbf8d`;
  it fixes 19 suites and 3,009 declared cases.

The passing child is not rerun through the changed coordinator. Linux
execution, large-native segmented-object transfer, hosted PE/ELF mutations,
segmented console-size rejection, promotion, and the grouped end-of-goal gate
remain deferred. No product implementation, format, candidate artifact,
managed reference, or WebAssembly implementation changes.

## Reconsideration triggers

Revise this contract if the ordinary byte-value or object limit, native file
snapshot service, startup failure mapping, WVO consumer inventory, or
publication transaction changes. A future segmented reader must add positive
evidence for its own larger bound without weakening this first-byte-over-limit
containment case.
