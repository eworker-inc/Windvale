# Decision 0017: Independent Windvale image reconstruction

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified at `d8008e3`

## Context

The qualified Windvale linker can construct a relocated candidate with the correct Stage 0 digest, but a digest and one construction path are not sufficient publication evidence. Windvale Linking 1 requires independent complete-image reconstruction before returning bytes. A defect shared by candidate construction and its checker must not silently authorize a host write.

The verifier may trust the already qualified WVO structural boundary, but it must not merely call the candidate builder a second time or compare only hashes.

## Decision

- Add a separately structured verifier inside `Wvˉlinkerˉcore` and run it for every successful candidate before reporting success.
- Recompute actual-address alignment by incrementing the address until an independently checked alignment predicate succeeds, rather than using the production padding calculation.
- Rebuild padding, materialized section data, and BSS into a second immutable image using verifier-owned placement functions.
- Rescan every symbol to find exports without the production canonical-range lookup shortcut, require one provider, and recompute defined-symbol addresses through verifier-owned placement.
- Reapply relocations in reverse input and reverse source-relocation order. Use verifier-owned signed-magnitude arithmetic for both relocation kinds.
- Compare candidate and reconstructed values byte by byte, including exact length. SHA-256 is not used as the equality predicate.
- Return `WVL1011`, discard all candidate bytes, and keep the host writer unreachable if reconstruction or equality fails.
- Exercise the failure boundary with an embedded deliberately corrupted reconstruction in addition to successful end-to-end oracle cases.

## Consequences

- A successful report now means two differently ordered and independently calculated Windvale paths agree on every image byte.
- The verifier deliberately repeats work and is not optimized around candidate intermediates. This improves fault separation but increases instruction cost.
- The conformance suite exercises the complete 4 MiB image limit under an explicit 200,000,000-instruction ceiling; smaller native smoke cases retain the ordinary 20,000,000-instruction ceiling.
- Shared primitives remain limited to the qualified immutable WVO reader, byte slices/concatenation, and fixed-width byte operations; resolution shortcuts, placement arithmetic, relocation evaluation, and traversal order are separate.
- Image publication remained disabled until Decision 0018 completed the canonical map and publish-after-success boundary, because a later map-limit failure must not leave an output image.

## Reconsider when

- Representative valid images approach the instruction limit; preserve independent calculations while introducing only measured immutable indexes.
- A future backend needs a verifier for another target. Keep target-specific reconstruction explicit rather than weakening this flat-image check into a generic hash comparison.
