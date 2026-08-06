# Decision 0293: Bounded staged-WVO content identity

- Date: 2026-08-06
- Status: Implemented candidate; staged-resource identity, platform publication, and grouped dual-host qualification pending
- Advances: [Decision 0291](0291-Bounded-Compiler-Wvo-Relocation-And-Placeholder-Verification.md), [Decision 0283](0283-Bounded-Native-Object-Publication-Cursor.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0291 validates relocation records, relocation placeholders, and text
padding, but it deliberately does not prove arbitrary machine-code bytes or
immutable-data bytes. A fixed platform publisher must reject a staged object
whose structurally valid chunk differs from the retained compiler plan without
joining a potentially 32 MiB WVO into one ordinary Windvale `bytes` value.

The publication cursor already owns the exact canonical byte sequence. The
versioned `WVOP 1` manifest owns the nonempty chunk positions and lengths. The
missing boundary is therefore a typed cursor that binds each actual bounded
chunk to both sources of evidence and compares its complete content.

## Decision

- Add `Native-X64-Lowering-Staging-Content.wv` as a focused portable module.
  It consumes one retained lowering plan, publication-region plan, immutable
  manifest snapshot, flat scalar content cursor, and actual bounded chunk.
- `content.begin` validates the manifest and phase evidence and captures the
  manifest chunk count plus the initial publication position.
- `content.next` revalidates that evidence, advances the existing publication
  cursor past zero-length regions, binds the next nonempty value to the exact
  manifest position and length, requires the actual chunk to have that length,
  compares every byte, and advances only on complete equality.
- `content.finish` requires every manifest chunk to have been consumed and the
  publication cursor to reach `Complete` at the admitted object length.
- Use nine explicit states: `Active`, `Complete`, `Invalid_plan`,
  `Invalid_manifest`, `Invalid_cursor`, `Publication`, `Manifest_entry`,
  `Chunk_length`, and `Content`. Rejection clears all cursor evidence.
- Keep the content cursor flat and scalar so a later fixed adapter can preserve
  it without inventing a serialized token. The lowering plan and publication
  regions remain typed immutable phase evidence and are constructed once by
  the caller.
- Do not add a digest format or reconstruct the complete WVO merely to prove
  equality. Every compared value remains within the existing 4 MiB chunk
  ceiling.

This layer accepts caller-supplied values. It does not prove that two reads
name the same staged host resource or that a resource remained unchanged
between validation and publication.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 6.769 test seconds
  after an 8.05-second zero-warning Release build; the complete passing command
  takes 19.4 seconds. No broader local verification level was run.
- One-section and two-section compiler objects complete byte for byte. The
  matrix rejects malformed WVB and manifest input, a strict but shifted
  manifest boundary, a shortened chunk, changed arbitrary code, changed
  immutable data, an invalid cursor, and trailing input.
- The Stage 0 evidence adapter is 403,243 bytes at SHA-256
  `2fb1ad5fc4e9561faf20ecd2390e9069e635909a1aeb4cea1ffcac13004a1634`.
- The scalar native runner is 404,838 bytes at SHA-256
  `491e504bd2c15889d6cedd282f5cc637e7011099cb6a761d815aefdb0f61eceb`.
  Stage 0 and the native source front door produce it byte for byte; independent
  fragment verification admits exactly `Text_utf8_is_valid`, `Text_concat`,
  and `U32_format`, and current-host x86-64 execution returns 42.
- The native source front door currently rejects the general bytes-in/bytes-out
  evidence harness at its loop-shaped source-binding boundary
  (`Source_wir`/`Source_bindings`, function 4, operation 0). It publishes no
  candidate. The independently built scalar runner includes and executes the
  complete content module, so this is a test-harness limitation rather than a
  product-module fallback.
- The content contract and tests are focused files rather than additions to the
  already-large lowering or WVB-to-WVO test sources.

No C# product implementation or WebAssembly implementation changed. This
slice does not bind staged resource identities, close read/replace races,
perform durable sibling replacement or cleanup, complete tool self-lowering,
promote artifacts, cut over the ordinary path, or retire .NET. Development,
Standard, Qualification, Linux execution, and the grouped end-of-goal gate
remain deferred.

## Reconsideration triggers

Revisit this contract if publication permits a manifest chunk to span more
than one cursor value, introduces a chunk larger than 4 MiB, serializes phase
evidence, or adds a content transform between lowering and staging. Resource
identity and transactional replacement remain separate platform contracts.
