# Decision 0288: Segmented large-native WVO section envelope

- Date: 2026-08-06
- Status: Implemented candidate; remaining WVO records, platform adapter, and grouped dual-host qualification pending
- Advances: [Decision 0287](0287-Validated-Native-Staging-Manifest-Accessors.md), [Decision 0283](0283-Bounded-Native-Object-Publication-Cursor.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [Windvale object format](../../Specifications/Windvale-Object-Format.md) and [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

The strict `WVOP 1` reader proves bounded contiguous chunk extents, but it does
not interpret the staged WVO. The existing Windvale-written general WVO reader
accepts one ordinary `bytes` value and correctly retains its 4 MiB standard
profile. Passing a 32 MiB large-native object to that reader as one borrowed
value would silently turn a platform adapter into an undocumented widening of
the portable value contract.

The native lowerer's publication cursor already gives the compiler-produced
WVO a narrower useful boundary. Its first nonempty chunk is exactly the WVO
header plus `.text` section header, and its optional `.rodata` header is a
separate exact chunk after the complete text extent. Those metadata values are
small even when the intervening section data approaches the 32 MiB object
ceiling.

## Decision

- Add a focused capability-free section-envelope reader for the
  compiler-produced WVO profile. It receives the same immutable manifest
  snapshot, the actual first chunk, and the actual optional read-only header
  chunk. It never receives or constructs one complete WVO value.
- Reuse the strict `WVOP 1` reader before interpreting any entry. Require chunk
  zero at position zero with the exact supplied prefix length.
- Require the exact 49-byte WVO 1.0 x86-64 prefix: supported version and flags,
  one or two sections, one through 4,096 symbols, at most 65,536 relocations,
  and the canonical 16-byte-aligned `.text` declaration with equal memory and
  data lengths. The one-section compiler profile has no relocations.
- For two sections, require an exact 27-byte chunk at the computed end of text
  and validate the canonical 16-byte-aligned `.rodata` declaration with equal
  memory and data lengths.
- Require a manifest chunk boundary at every following metadata position,
  bound combined section data to 32 MiB, and prove that the remaining object
  extent can contain at least every declared symbol and relocation record.
- Return the admitted object length, counts, section data lengths, and symbol
  position only after the complete section envelope is valid. Every rejection
  returns named status and zero evidence.
- Add a focused ABI-22 scalar bridge. Each query reruns validation over the
  same borrowed manifest and chunk snapshots so fixed platform assembly does
  not parse WVO header or section fields.

This is the compiler-output section envelope, not the general WVO verifier.
Symbol records, relocation records, relocation placeholders, staged-resource
identity, and final publication remain independently required.

## Evidence and consequences

- The reviewed focused compiler selection passes 1/1 in 1.449 test seconds
  after an 11.11-second zero-warning Release build. No broader local
  verification level was run.
- The matrix accepts one- and two-section envelopes, admits an exact 32 MiB
  object through ten bounded manifest chunks without constructing its section
  data, and rejects malformed manifest, first-chunk, prefix, header, count,
  section, extent, and metadata-boundary cases with zeroed evidence.
- A separate capability-free runner calls the bridge over immutable
  manifest/prefix/read-only values, passes independent native-fragment
  verification, requires zero services, executes as x86-64 machine code, and
  returns 42.
- The native source front door compiles the four-module evidence adapter to
  18,869 bytes at SHA-256
  `ab7893899e5f2a17bac735cdf6cb1f2e67a725a34cac82809eb2000026501106`
  and the four-module native runner to 18,449 bytes at SHA-256
  `d69a70c6c79bc1bd3e898af05f26d335b449128c4b1f19f6c9a437cdf33301f9`.
- The parser, scalar bridge, and tests are separate focused files rather than
  additions to the already-large lowering core or WVB-to-WVO test file.

No C# product implementation or WebAssembly implementation changed. This
slice does not validate symbol or relocation records, compare all staged chunk
content, retain native resource identities, construct a durable sibling,
replace atomically, clean up, complete tool self-lowering, promote artifacts,
cut over the ordinary path, or retire .NET. Development, Standard,
Qualification, Linux execution, WebAssembly verification, and the grouped gate
remain deferred.

## Reconsideration triggers

Revisit this focused profile if the native writer emits more section kinds,
stops aligning metadata to publication chunks, or gains imports or relocations
without read-only data. Do not generalize it by accepting an oversized
portable `bytes` value; extend the segmented verifier or select another
explicit large-native transport instead.
