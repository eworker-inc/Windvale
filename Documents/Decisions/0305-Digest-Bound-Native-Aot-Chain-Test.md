# Decision 0305: Digest-bound native AOT-chain test

- Date: 2026-08-06
- Status: Implemented current-host candidate; grouped Windows/Linux qualification pending
- Advances: [Decision 0225](0225-Native-Source-To-Aot-Composition-Proof.md), [Decision 0301](0301-Digest-Bound-Native-Wvo-Candidate-Launchers.md), [Decision 0302](0302-Digest-Bound-Native-Wvo-Linker-Candidate.md), [Decision 0303](0303-Digest-Bound-Native-Console-Packager-Candidate.md), and [Decision 0304](0304-Digest-Bound-Native-Wvb-To-Wvo-Candidate.md)
- Contract: [Native source-to-AOT composition](../../Specifications/Windvale-Native-Source-To-Aot-Composition.md)

## Context

Decision 0225 proved the accepted-subset source-to-executable chain as native
child processes, but its C# test harness reconstructed the unpromoted lowerer,
linker, and packager applications on every run. Decisions 0301 through 0304 now
pin the exact candidate artifacts and digest-bound entry points needed to run
that same proof without a live managed oracle or package constructor.

Permanent post-retirement tests should use versioned inputs, exact identities,
structural admission, deterministic failure outcomes, and direct behavior. They
should not require the deleted C# implementation to generate expected results.

## Decision

- Add `Test-Aot-Chain.cmd` and `.sh` as no-argument current-host test
  coordinators. They invoke only pinned native launchers plus inbox host command
  and digest utilities; they do not invoke `dotnet` or reconstruct any tool.
- Build the fixed return-42 Project 1 input through the qualified native source
  front door, then lower, independently verify, link, package, and execute it as
  distinct native processes.
- Require the exact canonical WVB, WVO, link map, flat image, and host
  application SHA-256 identities recorded by Decision 0225. Require the final
  process result to be exactly 42 with no diagnostic.
- Keep every phase visible. A failed build, digest check, WVO admission, link,
  package, or execution stops the script without falling back to Stage 0.
- Use a bounded private temporary directory. Windows deletes only the five
  named files before removing the now-empty directory; Linux validates the
  generated prefix, deletes only named files, and removes the empty directory.
- Retain the earlier managed composition test as differential/recovery evidence
  until the final archive. The new script is the permanent fixed-vector route
  that survives C# deletion.

## Evidence and consequences

- The Windows script completes the native source → WVB → WVO → verified WVO →
  flat image/map → PE → execution chain in 1.4 seconds and reports exactly
  `native aot chain status=Passed result=42`.
- The reviewed focused compiler selection passes 1/1 in 0.926 test seconds
  after a 7.17-second zero-warning Release build; the complete command takes
  10.5 seconds.
- The first focused run exposed an LF batch-file `call :label` portability
  problem before any native phase failed. The final Windows script uses inline
  digest checks and nonrecursive named cleanup, eliminating that dependency.
- This adds a reusable native test rather than another line-for-line port of the
  C# harness. It covers exact end-to-end products and behavior while existing
  focused native tools retain their detailed malformed-input ownership.
- No WebAssembly implementation or compiler semantics changed. Linux execution,
  grouped qualification, candidate promotion, complete backend/test transfer,
  and release automation remain deferred.

## Reconsideration triggers

Update the fixed identities only when a named format or semantic decision
intentionally changes the chain. Introduce a native coordinator only when it
has an explicit transaction, authority, and failure contract; process-startup
cost alone does not justify hiding phase boundaries.
