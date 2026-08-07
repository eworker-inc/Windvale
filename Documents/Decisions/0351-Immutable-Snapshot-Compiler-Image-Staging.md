# Decision 0351: Immutable-snapshot compiler-image staging

- Status: Accepted local implementation; publisher-scale transfer and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0350](0350-Versioned-Segmented-Compiler-Image-Staging-Manifest.md), [Decision 0349](0349-Independent-Segmented-Compiler-Wvo-Image-Verification.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale linking](../../Specifications/Windvale-Linking.md#hosted-immutable-snapshot-staging-boundary)
- Advanced by: [Decision 0352](0352-Digest-Bound-Compiler-Image-Staging-Applications.md)

## Context

The segmented Windvale linker, independent verifier, and strict `WVLI`
manifest existed as separate portable operations. No hosted Windvale owner yet
acquired a complete staged WVO, preserved one immutable input view across all
three operations, wrote the verified output chunks, or published the
completion manifest last. A platform adapter that parsed WVO metadata or
reopened mutable source paths would duplicate policy and weaken the retained
snapshot evidence.

The native hosted file service already snapshots the first successful read of
each exact resource name for one execution. A strict `WVOP` plus at most 62
source chunks occupies only 63 of its 64 entries, so later Windvale calls can
reuse those values without introducing another large buffer or native parser.

## Decision

- Add `Compiler-Flat-Image-Staging-Resources.wv` as the focused portable owner
  of exact source/output names, prefix bounds, manifest admission, source
  chunk count, snapshot count, and control/chunk collision rejection.
- Add `Compiler-Wvo-Segmented-Flat-Image-Staging-Tool.wv` as the hosted
  Windvale orchestration root. It accepts source chunk prefix, `WVOP` resource,
  output chunk prefix, and `WVLI` resource.
- Read the source manifest once, then acquire every canonical source chunk in
  index order and require its exact manifest length. At most 62 chunks plus
  the manifest occupy 63 distinct immutable input snapshots.
- Resolve the WVO prefix, optional read-only header, symbol chunk, and optional
  relocation chunk by exact validated positions. Later reads use the same
  resource names and therefore the retained snapshots rather than mutable host
  paths.
- Build the compiler-image plan, link one bounded source chunk, and pass its
  candidate through the separately implemented verifier before writing it.
  Metadata emits no output. Every nonempty candidate must begin at the
  verifier's exact image cursor.
- Record only accepted output indices, positions, and lengths. Require complete
  source and image coverage, exact agreement with the planned output count,
  and a valid `WVLI 1.0` value before writing the output manifest last.
- Keep private staging separate from public publication. Exact names do not
  prove native file identity; this tool does not reread output, flush a set,
  atomically replace a destination, or clean incomplete/stale chunk resources.
- Keep the 157-line resource policy, 372-line hosted orchestration, and focused
  test in separate files. Their boundaries follow ownership rather than a
  numeric line target.

## Evidence and consequences

After test review, the exact named case passes 1/1 in 1.431 test seconds after
a 7.34-second zero-warning Release build of the affected test project. The
eight-chunk fixture performs nine underlying reads—the manifest and each source
chunk exactly once—even though metadata, producer, and verifier passes request
the same names again. It writes four exact linked chunks plus the 76-byte
`WVLI` manifest last, covering the complete 21-byte image and entry offset 4.

The same case rejects equal source/output prefixes before any read or write.
The compiled native fragment passes independent fragment verification and
requires only console output, process arguments, file input/output, diagnostic
output, enum naming, text concatenation, and unsigned formatting services.

The first focused invocation exposed a short-circuit Boolean return that the
current source front end left unterminated; explicit bounded returns corrected
the new helper before product behavior ran. The next invocation completed the
link and found only an overbroad test expectation for an unused UTF-8 service;
the test was narrowed to the actual service surface before the final pass.

No C# product implementation or platform assembly changed. The managed test
harness remains temporary orchestration evidence. The actual 6,449,889-byte
publisher WVO, a current-host native process, Linux execution, resource-identity
binding, durable publication, canonical map production, host-container
construction, Development, Standard, Qualification, and the grouped retirement
gate remain deferred.

## Reconsideration triggers

Revisit the resource plan if the native input table changes, the source stream
needs more than 62 chunks, another distinct input must precede them, or
`file.read_bytes` stops returning one immutable snapshot per exact name and
execution. Version the staging contract rather than inferring new sections,
relocation kinds, sparse image ranges, nonzero bases, or authenticity evidence.
