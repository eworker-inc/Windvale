# Decision 0284: Versioned native object-staging manifest

- Date: 2026-08-06
- Status: Implemented candidate; native commit adapter and grouped dual-host qualification pending
- Advances: [Decision 0283](0283-Bounded-Native-Object-Publication-Cursor.md), [Decision 0214](0214-Exact-Native-Wvb-Publication-Step.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0283 exposes one exact bounded value for each canonical WVO region,
but the ordinary hosted `file.write_bytes` capability still accepts only one
complete bounded value. Extending that capability with path creation, mutable
handles, positioned writes, or replacement would add source-language and host
semantics while the C# compiler is feature-frozen. Requiring one complete WVO
would instead restore the whole-value limit that the cursor removed.

The repository already has a qualified native WVB publication transaction for
exclusive sibling creation, exact writes, durable flush, atomic replacement,
cleanup, and indeterminate completion. The next WVO slice should prepare
bounded inputs for an equally small fixed native adapter without duplicating
that transaction inside the compiler or pretending that ordinary file writes
are atomic publication.

## Decision

- Add a focused hosted staging tool rather than enlarging the native lowering
  core. Its command is `wvnative-stage <input.wvb> <chunk-prefix>
  <manifest.wvop>`.
- Consume the immutable publication cursor with the ordinary 4 MiB artifact
  ceiling. Omit zero-length steps and write every nonempty value once to the
  derived resource `<chunk-prefix>.chunk-<decimal-index>` in cursor order.
- Require a nonempty chunk prefix of at most 4,078 UTF-8 bytes. Reject a
  derived chunk resource that equals the input or manifest resource.
- Serialize a little-endian `WVOP 1.0` manifest only after every chunk write
  returns. Its 24-byte header records manifest length, final WVO length, chunk
  count, and the 4 MiB chunk ceiling. Each 12-byte entry records chunk index,
  exact WVO position, and length. Indices begin at zero and positions are
  contiguous for this producer.
- Keep the manifest deliberately small and structural. It does not contain a
  digest and is not an authority token. A commit adapter must retain or bind
  the exact staged-resource identities, validate the complete manifest and
  write sequence, reconstruct and verify the WVO, and only then enter the
  qualified sibling-replacement transaction.
- Treat the manifest write as a completion marker for staging, not as final
  publication. A rejected or indeterminate chunk write may leave scratch
  resources and must never be retried as though no mutation occurred. The
  caller owns a unique private prefix and cleanup until the fixed commit
  adapter takes that responsibility.
- Do not change WebAssembly implementation or C# product implementation in
  this slice. Stage 0 remains the independent object and execution oracle.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 17.262 test seconds
  after a 20.92-second zero-warning Release build. No broader local
  verification level was run.
- The return-42 case produces three nonempty chunks and a 60-byte manifest.
  Independent Stage 0 parsing proves exact indices, positions, lengths,
  manifest-last order, and reconstruction of the canonical 479-byte WVO.
- The native source front door compiles the 23-module staging-tool closure to
  a 390,066-byte WVB at SHA-256
  `c916610ad1d4ca3b5d1573f5775aaf1a102a89587a2fb3cba8941d42c93136ba`.
- Existing unpromoted Windows and Linux WVB-to-WVO packages remain 5,348,864
  and 5,349,376 bytes at SHA-256
  `0e0d0c87f82f6576b11f888cfa26469f86f157064ea605a4bb188bcee5e3b280`
  and `c6ba202ffcb32a261bfd9c997e4bab754ab5a636e2d0b95e5de5f55e598c6358`.

The separate file also keeps staging policy out of the already-large lowering
core. This slice does not claim hostile manifest consumption, retained native
file handles, cleanup, durable final-object construction, atomic replacement,
complete-tool self-lowering, ordinary-path cutover, artifact promotion, or
.NET retirement. Development, Standard, Qualification, Linux execution,
WebAssembly verification, and the complete grouped gate remain deferred.

## Reconsideration triggers

Revisit `WVOP 1` before a consumer is accepted if the final adapter cannot
retain exact resource identity, if one cursor value can exceed 4 MiB, if the
accepted module envelope can make the manifest exceed the ordinary value
limit, or if WVO publication becomes noncontiguous. A future reader must add
truncated, oversized, inconsistent, reordered, duplicate, missing-resource,
and malicious-input coverage before the format can cross a trust boundary.
