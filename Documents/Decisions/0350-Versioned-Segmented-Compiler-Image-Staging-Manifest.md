# Decision 0350: Versioned segmented compiler-image staging manifest

- Status: Accepted local implementation; hosted staging integration and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0349](0349-Independent-Segmented-Compiler-Wvo-Image-Verification.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale linking](../../Specifications/Windvale-Linking.md#wvli-10-linked-image-staging-manifest)

## Context

The bounded compiler-WVO linker and its independent verifier now agree on an
ordered flat-image chunk stream, but no serialized evidence described the
completed output set. A hosted adapter could write correctly linked chunks and
still leave callers unable to distinguish complete staging from a truncated,
reordered, repeated, or stale collection.

Reusing the source `WVOP` manifest would omit the linked entry offset and blur
WVO byte positions with flat-image positions. Reusing the two-chunk `WVCS`
console manifest would impose a fixed application-container shape unrelated to
the compiler image.

## Decision

- Add `Linker/Windvale/Compiler-Flat-Image-Staging-Manifest.wv` as a focused
  portable owner of `WVLI 1.0` validation and construction.
- Use a 28-byte little-endian header containing magic/version, exact manifest
  bytes, complete flat-image bytes, base-zero `Main` entry offset, chunk count,
  and the unchanged 4 MiB per-value ceiling.
- Encode one 12-byte index, image-position, and length entry for each nonempty
  linked output chunk. Require one through 518 chunks, ordinal indices,
  contiguous positions, nonzero bounded lengths, exact image coverage, a
  nonempty image no larger than 32 MiB, and an entry strictly inside it.
- Validate caller-supplied entries again after construction. Invalid builder
  input returns an empty value; invalid read input returns one of eleven named
  statuses and zero image, entry, count, and limit evidence.
- Keep the manifest structural. It is not a digest, capability grant,
  canonical link map, durable transaction record, or host executable format.
- Require a future hosted tool to write all image chunks first and the `WVLI`
  manifest last. Public destination replacement and cleanup remain a separate
  transaction boundary.

The complete manifest is at most 6,244 bytes. It therefore remains one small
ordinary Windvale value even when the represented image reaches 32 MiB.

## Evidence

After reviewing the focused matrix, the exact named Fast selection passes 1/1
in 1.295 test seconds after a 15.53-second zero-warning Release build. It
requires exact `WVLI` construction and round-trip validation for the four
linked fixture chunks and rejects truncation, magic, version, encoded size,
image size, entry, count, ceiling, index, position, and length mutations.
Invalid build input publishes no manifest.

A capability-free native runner builds the exact 76-byte four-chunk manifest,
validates every admitted scalar, checks first and final entries, rejects an
out-of-range entry and truncated input, passes native-fragment verification,
requires zero services, executes as current-host x86-64, and returns 42.

No C# product implementation or platform assembly changed. Development,
Standard, Qualification, Linux execution, the publisher-scale extended case,
and the grouped end-of-goal gate were not run.

## Consequences

The large-native linked-image staging set now has a strict, bounded,
Windvale-owned completion marker. The future hosted bridge can build entries as
the producer/verifier cursor advances and can publish the manifest only after
every candidate chunk has been accepted and written.

This does not yet bind host resource identities, construct chunk names, stage
the actual publisher image, verify rereads, replace a durable destination,
compose native services, construct PE/ELF, emit a canonical link map, qualify
Linux, promote an ordinary path, or retire Stage 0.

## Reconsideration triggers

Version the contract if linked-image staging gains nonzero bases, sparse holes,
multiple input/output images, per-chunk digests, a different value ceiling, or
an entry outside the represented image. Do not overload `WVLI` with executable
container, capability, or transaction semantics.
