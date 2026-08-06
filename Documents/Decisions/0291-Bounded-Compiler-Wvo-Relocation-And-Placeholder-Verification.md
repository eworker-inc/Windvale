# Decision 0291: Bounded compiler-WVO relocation and placeholder verification

- Date: 2026-08-06
- Status: Implemented candidate; staged-content identity, platform publication, and grouped dual-host qualification pending
- Advances: [Decision 0290](0290-Bounded-Compiler-Wvo-Symbol-Verification.md), [Decision 0288](0288-Segmented-Large-Native-Wvo-Section-Envelope.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [Windvale object format](../../Specifications/Windvale-Object-Format.md) and [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0290 validates the complete compiler-produced symbol chunk and fixes
the exact following relocation extent. The relocation records themselves and
the code bytes that they patch remained untrusted. A fixed native publication
adapter must not duplicate those compiler-specific rules or accept a staged
object whose relocation field already contains an address-dependent value.

The publication cursor already emits code in bounded function batches and
emits optional text padding as a separate bounded chunk. Relocation and
placeholder verification can therefore stay within the ordinary 4 MiB value
contract while the complete WVO may reach 32 MiB.

## Decision

- Add a focused capability-free relocation reader above the validated symbol
  boundary. It consumes the same manifest, prefix, read-only header, and symbol
  snapshots plus the actual bounded relocation chunk.
- Require the relocation chunk length to equal the exact extent admitted by the
  symbol reader.
- Derive the code length from the already validated function-symbol ranges.
  Optional zero-through-15-byte text padding must begin at one exact manifest
  boundary and occupy one complete chunk.
- Accept only the compiler's canonical 20-byte records: `Relative_i32` kind
  `2`, zero flags and reserved fields, section zero, addend `-4`, a four-byte
  field wholly inside code, ascending nonoverlapping offsets, and a target
  index naming one admitted data symbol.
- Return code bytes, the exact number of text-region chunks, and relocation
  count only after every summary check succeeds. Rejection returns one of eight
  summary rejection statuses and zero evidence.
- Add a per-text-chunk validator. It revalidates the summary, binds the supplied
  chunk index to its exact manifest position and length, rejects code/padding
  crossings, requires every relocation placeholder owned by a code chunk to be
  four zero bytes, and requires every padding byte to be `0x90`.
- Add a focused ABI-22 scalar bridge that maps all thirteen statuses and reruns
  validation over the same borrowed snapshots for each query.

The future fixed adapter must invoke the per-chunk validator for the exact
text-chunk count. This boundary does not compare arbitrary non-placeholder
machine-code bytes or immutable-data bytes with compiler evidence, and it does
not bind host resource identities.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 1.990 test seconds
  after a 22.55-second zero-warning Release build; the complete command takes
  28.680 seconds. No broader local verification level was run.
- The matrix accepts relocation-free, one-relocation, and separately padded
  compiler layouts. It rejects invalid preceding symbols, relocation length,
  flags, shape, order, range, target, padding boundary, text-chunk index and
  length, nonzero placeholders, and noncanonical padding.
- A capability-free native runner validates the summary, a valid code chunk,
  and an invalid placeholder, passes independent fragment verification,
  requires zero services, executes as x86-64 machine code, and returns 42.
- Stage 0 and the native source front door independently produce the same
  six-module artifacts. The evidence adapter is 41,953 bytes at SHA-256
  `9e0dfc3db2e8c03f903e7a40e13bbb3c1b56a9f938420e5e5535c80c1f4c5d2a`;
  the native runner is 40,660 bytes at SHA-256
  `b254d54d29f162fb0a3232c052bbe985885d2e9b94bf7d732002c6adac1e7d30`.
- The reader, scalar bridge, fixtures, and tests remain focused files rather
  than enlarging the lowering core or the existing 100 KiB WVB-to-WVO test.

No C# product implementation or WebAssembly implementation changed. This
slice does not validate arbitrary non-placeholder code bytes, immutable-data
content, or staged resource identity; reconstruct the complete WVO; perform
durable sibling replacement or cleanup; complete tool self-lowering; promote
artifacts; cut over the ordinary path; or retire .NET. Development, Standard,
Qualification, Linux execution, WebAssembly verification, and the grouped gate
remain deferred.

## Reconsideration triggers

Revisit this profile if the native writer emits another relocation kind,
section-relative target, addend, imported symbol, nonzero placeholder,
interleaved padding, or a relocation field that may cross a function-batch
boundary. General WVO evolution belongs in the object-format contract and must
not be inferred from this compiler-specific publication profile.
