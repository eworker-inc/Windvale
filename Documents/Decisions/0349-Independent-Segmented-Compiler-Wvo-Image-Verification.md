# Decision 0349: Independent segmented compiler-WVO image verification

- Status: Accepted local implementation; publisher-scale transfer and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0348](0348-Bounded-Segmented-Compiler-Wvo-Linking.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale linking](../../Specifications/Windvale-Linking.md#segmented-compiler-wvo-flat-image-candidate)

## Context

Decision 0348 transfers base-zero compiler-WVO relocation and flat-image chunk
construction into Windvale, but its first producer result was not yet accepted
by a separately implemented verifier. Publishing those bytes directly would
weaken the existing general link contract, whose Stage 0 and ordinary
Windvale-written paths independently reconstruct every candidate image before
exposure.

The complete large-native image cannot become one Windvale `bytes` value.
Independent verification must therefore preserve the same bounded chunk
transport and prove complete ordered coverage without joining either the WVO
or image.

## Decision

- Add
  `Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image-Verification.wv` as a
  focused portable module separate from the producer and the large general
  linker core.
- Revalidate the strict staging manifest, section envelope, compiler symbol
  profile, relocation records, source placeholders, and padding from the same
  immutable snapshots. Do not import or call the Decision 0348 producer
  module and do not accept its plan as evidence.
- Begin one flat scalar cursor at manifest chunk zero and image position zero.
  Each step consumes exactly the next manifest chunk and requires the supplied
  candidate position to equal the next image position.
- Require WVO metadata chunks to match their admitted snapshots and emit no
  candidate bytes. Require read-only-data candidates to match their source
  chunks exactly.
- Independently inspect text candidates in reverse relocation-record order.
  Compare all non-placeholder ranges with their source, derive each base-zero
  relative value from admitted section and data-symbol offsets, and require
  the exact little-endian candidate field.
- Reject changed ordinary code, changed relocation bits, invalid source
  placeholders, wrong candidate position or length, changed data, skipped or
  repeated chunks under the retained cursor, invalid cursor shape, incomplete
  input, and excess output with named status and zero cursor evidence.
- Reach `Complete` only after all manifest chunks are consumed and the image
  cursor equals the exact `.text` plus `.rodata` extent.

Every source and candidate value remains within the ordinary 4 MiB limit. The
complete admitted image remains within the explicit 32 MiB large-native bound.
The cursor is typed execution-local evidence retained by the fixed owner, not a
serialized token or an authenticity proof for caller-supplied state.

## Evidence

After test review, the same focused named Fast selection passes 1/1 in 1.756
test seconds after a 7.88-second zero-warning Release build. The dynamic matrix
accepts exact linked bytes and rejects wrong position, short candidate, changed
ordinary code, changed relocation value, invalid source placeholder, short
source, and malformed manifest, envelope, symbol, and relocation evidence.

The capability-free native runner processes the complete eight-chunk fixture:
WVO prefix, code, padding, read-only header, two read-only-data chunks, symbols,
and relocations. It independently accepts the four output chunks at exact
positions, reaches `Complete` at 21 image bytes, passes native-fragment
verification, requires zero services, executes as current-host x86-64, and
returns 42.

Two narrow test-harness composition errors were corrected before the passing
selection: an unreachable verifier dependency was supplied to the producer-only
root, then one adapter cursor local reused an existing byte-offset name. Both
failed before product behavior executed. No broader verification level was
run.

## Consequences

The specialized large-native link boundary now has separate Windvale producer
and verifier implementations, both operating on bounded values and a complete
ordered cursor. A future publisher can reject a changed or incomplete output
stream before durable exposure without calling .NET or holding the whole image
in memory.

This does not yet provide the fixed snapshot bridge, output staging manifest,
durable publication transaction, canonical link map, actual 6.45 MiB publisher
comparison, multi-object linking, nonzero bases, service-bundle composition,
host-container construction, Linux execution, promotion, or grouped
qualification.

## Reconsideration triggers

Revisit the independent algorithm if compiler WVO gains another section,
relocation, import, addend, base-address, or chunk-boundary shape. Do not reuse
producer cursor or plan state as verifier evidence merely to reduce code; a
shared structural WVO reader is permitted, while final output reconstruction
must remain independently implemented.
