# Decision 0809: Retain generic nominal evidence in WVLB

- Status: Accepted
- Date: 2026-08-21

## Context

WVGT 1.0 gives generic record and variant instances deterministic compiler
identities, and Decision 0808 maps those instances to ordinary output types.
The split compiler still needs to carry the admitted catalog from semantic
analysis to emission. WVCA already owns only counts and integrity metadata,
WVIR deliberately contains monomorphic operations, and WVLB already retains
the analogous WVGC function-specialization catalog.

Creating another cached artifact would add a new integrity relation, cache
key, serialized format, and verification path. Reusing WVLB without separating
the WVGC and WVGT lengths would make malformed evidence ambiguous and prevent
independent catalog validation.

## Decision

1. Extend WVLB to minor version 1.3 when and only when a non-empty WVGT catalog
   must be retained. Preserve exact WVLB 1.1 output for ordinary programs and
   exact WVLB 1.2 output for function-specialization-only programs.
2. Use a 40-byte header with separate WVGC and WVGT byte lengths, combined
   catalog layout version `2`, and one required zero reserved field.
3. Keep the existing 16-byte specialized function ranges. WVGC instances may
   append function ranges; WVGT instances never do.
4. Serialize ranges, optional WVGC, required WVGT, and binding entries in that
   order. Keep each catalog within its existing 1 MiB validation bound and the
   complete directory within 4 MiB.
5. Admit a private WVGT binding shape only when its instance index is present
   in the retained catalog. Do not treat private shapes as WVB or runtime type
   identities.
6. Validate the two catalogs independently and reject invalid versions,
   lengths, empty required WVGT evidence, range/catalog disagreement,
   out-of-catalog private shapes, nonzero reserved values, truncation, and
   trailing bytes.
7. Publish through one focused generic-type specialization function. Delegate
   an empty WVGT catalog to the unchanged existing function-only API.
8. Do not claim general generic source integration at this checkpoint. Main
   WIR construction must still produce and pass the catalog, and Source WVB
   must still consume the materialization plan.

## Evidence

The focused fixture constructs a real generic `Box<i32>` WVGT instance, embeds
its private shape in one binding entry, publishes and independently validates a
type-only WVLB 1.3 directory, reconstructs both catalogs, proves exact entry and
range offsets, and exercises the final combined publication entry point. A
second valid directory retains non-empty WVGC and WVGT catalogs together and
proves the appended function-specialization range identity. The fixture also
proves the empty-catalog WVLB 1.1 fallback and rejects an out-of-catalog shape,
short headers, truncation, trailing bytes, invalid catalog lengths and magic,
catalog layout and reserved mutations, range-count disagreement, and
cross-catalog length confusion.

The exact focused artifact sizes and digests are recorded in the Language 1.0
migration evidence. Its five-fragment hosted Windows executable returns `42`,
writes no output, and the focused owner passes 20 cases.

## Consequences

WVGT now has a durable home in the existing split-compiler evidence chain. An
emitter can recover both specialization catalogs from one validated WVLB value,
and ordinary builds pay no serialized-format or cache-identity cost.

`Compiler/Windvale/Source-Bindings-Core.wv` and all established compiler
project closures remain byte-for-byte unchanged. The new focused
`Compiler/Windvale/Source-Bindings-Generic-Types-Core.wv` module imports generic
type lowering only for consumers that retain WVGT. This avoids adding the new
code to every Seed binding build while preserving one WVLB artifact family.
General generic nominal source still does not reach WVIR or WVB until the next
integration checkpoints.

The isolation is also a bootstrap constraint. The large existing Generic-WIR
fixture currently makes the native Seed build driver exit without diagnostics
even with its exact `HEAD` source closure. It therefore cannot serve as passing
evidence for this checkpoint, and the new module must not be added to that
closure until the failing Seed boundary is measured and repaired or replaced
code frees capacity.

## Reconsideration triggers

Revisit the envelope only if a later specialization family cannot be expressed
as bounded semantic evidence inside WVLB, or if measured catalogs approach the
current 1 MiB per-catalog or 4 MiB complete-directory limits.
