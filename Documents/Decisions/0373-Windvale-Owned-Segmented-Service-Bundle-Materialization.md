# Decision 0373: Windvale-owned segmented service-bundle materialization

- Status: Accepted current-host complete construction transfer; Linux execution and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0372](0372-Windvale-Owned-Bounded-Service-Bundle-Materialization.md), [Decision 0365](0365-Native-Publication-Planner-Execution.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native service-bundle materialization](../../Specifications/Windvale-Native-Service-Bundle-Materialization.md)
- Advanced by: [Decision 0377](0377-Windvale-Owned-Native-Service-Table.md)

## Context

Decision 0372 moved complete service-bundle construction to Windvale when one
request and one whole-image response both fit the ordinary 4 MiB byte-value
limit. Valid compiler-family fragments and final publication images can reach
32 MiB and 34 MiB respectively, so those bundles retained an explicitly named
C# copying and fill fallback.

Raising the language-wide byte-value limit for this private construction step
would widen unrelated semantics. Splitting the final image at service
boundaries is also insufficient because one verified fragment may itself be
larger than 4 MiB. The image is nevertheless a simple ordered set of fragment,
fill, and leaf regions whose intersections can be constructed independently.

## Decision

- Replace whole-image `WVSQ 1` / `WVSI 1` with segmented version 2. Do not
  retain a production version-1 compatibility branch during early development.
- Fix the maximum image segment at 4,194,104 bytes, reserving enough of one
  4 MiB request for the 32-byte segment header and maximum 168-byte `WVPQ`.
- Require canonical segment starts at exact multiples of that bound, full
  nonfinal segments, and one exact positive remainder. Every request embeds and
  revalidates the complete publication plan.
- Carry only fragment and service-leaf byte ranges intersecting the requested
  segment. Omit all alignment fill. Windvale derives zero/NOP regions from the
  accepted plan and constructs the complete response segment.
- Return a 40-byte `WVSI 2` envelope followed by the exact segment. Each
  response remains below 4 MiB.
- Keep a focused managed session limited to deterministic bounded request
  projection, ascending response invocation, exact envelope/source/fill
  validation, and ordered concatenation. It does not construct a source or fill
  byte in the final image.
- Route both small and large bundles through the same segmented Windvale path.
  Remove `Canˉmaterialize` and `Materializeˉstage0ˉlargeˉbundle` rather than
  retaining parallel construction policies.
- Keep source responsibilities focused: the portable constructor owns segment
  semantics, the managed session owns transport and independent validation,
  and the retained-fragment loader owns exact artifact admission.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Segmented materialization core WVB | 17,185 | `97063c0c3d264d9b9ede73cc316c68798c66d61732c5b115f71a33e486ee7008` |
| Retained segmented bridge WVB | 17,150 | `327b753062d46755b934cfe6e6bc16550ec711c8b7d2aff46eac4bf0d8d9d902` |
| Retained segmented bridge WVNF | 179,452 | `d0b12e426e891f6ee78209ab817dde7c547c0f68541750d39dd665607434e7a9` |

These identities supersede the bounded version-1 constructor identities in
Decision 0372.

## Evidence and consequences

The reviewed focused case pins and reproduces all source/WVB/WVNF identities,
compares valid and malformed version-2 requests through the reference
interpreter and verified native fragment, checks exact zero/NOP fill, and
constructs an image crossing the segment boundary through two bounded native
invocations. The same case keeps the ordinary Windows/Linux one-service bundle
selection on this sole Windvale path. The first focused run stopped before
segment execution because a test-only request-list builder used a finalizer
that requires preallocated capacity; the helper was corrected without changing
the contract or artifacts. The final affected Release build passed with zero
warnings and errors in 2.86 seconds, and the single named test passed 1/1 in
2.348 seconds.

There is no longer a production C# service-bundle image writer. C# still owns
verified model projection, bounded transport, independent output validation,
the containing service/adapter metadata, executable-memory allocation and W^X,
service tables, contexts, arenas, platform calls, invocation, and teardown.
Those are later retirement slices rather than hidden construction fallbacks.

Linux execution and the grouped broad gate remain deferred.

## Reconsideration triggers

Replace the segmented request session when native fragment and service owners
can invoke the constructor without managed projection or can publish segments
directly into the W^X transaction. Change the segment bound only with measured
request, response, instruction, arena, teardown, and cross-host evidence.
Preserve the 34 MiB publication limit unless that contract is explicitly
versioned.
