# Decision 0348: Bounded segmented compiler-WVO linking

- Status: Accepted local implementation; publisher-scale transfer and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0346](0346-Bounded-Native-Publisher-Self-Lowering.md), [Decision 0291](0291-Bounded-Compiler-Wvo-Relocation-And-Placeholder-Verification.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale linking](../../Specifications/Windvale-Linking.md#segmented-compiler-wvo-flat-image-candidate)
- Advanced by: [Decision 0349](0349-Independent-Segmented-Compiler-Wvo-Image-Verification.md)

## Context

The native producer and publisher can now self-lower the exact 6,449,889-byte
publisher WVO, but the ordinary Windvale-written linker still receives one
complete `bytes` value and retains its 4 MiB image ceiling. The Stage 0
large-native linker can join the same WVO at base zero, but using that C# path
to construct each successor package would preserve a normal product
dependency.

The staged compiler-WVO readers already validate the exact section envelope,
symbol profile, relocation records, zero placeholders, and padding by bounded
chunk. Compiler-generated WVO has no imports. Its optional `.rodata` follows
already aligned `.text`, and every relocation field is wholly owned by one
text chunk. Those properties are sufficient to transfer the first base-zero
flat-image operation without widening Windvale values or duplicating a WVO
parser in platform assembly.

## Decision

- Add `Linker/Windvale/Compiler-Wvo-Segmented-Flat-Image.wv` as a focused
  portable Windvale linker module. Keep it separate from the already-large
  general linker core.
- Reuse the strict staging manifest, compiler-WVO envelope, symbol, and
  relocation validators. Accept only one compiler-produced `.text` plus
  optional `.rodata` object, local definitions, one exported `Main`, no
  imports, and canonical relative relocations with addend `-4`.
- Build a scalar plan exposing exact image bytes, entry offset, manifest and
  output chunk counts, text chunks, and relocations. Rejection returns named
  status and zero evidence.
- Process one actual manifest chunk at a time. Exact metadata chunks emit no
  image value; text chunks retain all non-placeholder bytes and replace only
  their owned fields; padding and read-only-data chunks retain exact content
  and derived image positions.
- At base zero, calculate each compiler relocation as
  `text-bytes + data-symbol-offset - patch-offset - 4`. Existing validation
  proves the value is nonnegative, signed-32-bit representable, and contained
  in one bounded text value.
- Support more than one read-only-data staging chunk even though the current
  producer emits one, while requiring all section boundaries to remain exact.
- Do not construct one complete WVO or flat image and do not raise the 4 MiB
  `bytes` limit. Keep complete image admission at 32 MiB.

This is a compiler-specific base-zero candidate, not a second general linker.
The complete general WVO contract, multi-object resolution, imports, nonzero
base addresses, canonical maps, independent output verification, and durable
publication remain separate boundaries.

## Evidence

After reviewing the focused matrix, the exact named Fast selection passes 1/1
in 2.341 test seconds after a 7.89-second zero-warning Release build. It covers
two relocations in one code chunk, separate text padding, two read-only-data
chunks, every metadata chunk, a relocation-free one-section object, and
manifest, envelope, symbol, relocation, index, length, content, and placeholder
rejections. The expected linked text and final chunk sequence are fixed bytes.

A capability-free Windvale runner calls the plan and chunk operations,
requires the exact base-zero image positions and relocation bits, rejects a
changed placeholder, passes independent native-fragment verification, requires
zero services, executes as current-host x86-64 machine code, and returns 42.

No C# product implementation or platform assembly changed. The managed test
host remains temporary orchestration and compilation evidence. The interrupted
short-timeout invocation and one missing test namespace import did not execute
the case; after that narrow harness correction, only the same named test was
run. Development, Standard, Qualification, Linux execution, the existing
publisher-scale extended test, and the grouped end-of-goal gate were not run.

## Consequences

Windvale now owns the first bounded algorithm that turns staged compiler WVO
content into exact base-zero flat-image chunks. Large-native linking no longer
requires one in-memory WVO or one in-memory result at this layer.

The actual 6.45 MiB publisher stream has not yet crossed this new module.
Decision 0349 adds the independent output cursor/verifier. A fixed snapshot
adapter, durable image publisher, publisher-scale exact comparison, and later
service-bundle and PE/ELF construction transfer remain required before the
Stage 0 package constructor can move to recovery-only status.

## Reconsideration triggers

Revisit the profile if the native compiler emits imports, another section or
relocation kind, a non-`-4` addend, a relocation field crossing chunk
boundaries, writable or zero-fill data, or noncontiguous compiler section
layout. General format evolution belongs in WVO and the general linker rather
than being inferred by this specialized transfer path.
