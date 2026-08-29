# Decision 0879: repair final Slice 7 qualification boundaries

## Status

Accepted implementation correction on 2026-08-29. Final paired-host evidence
remains pending.

## Context

The paired Qualification run after Decision 0878 proved bootstrap and
WebAssembly execution on both hosts and proved two of the four native shards on
each host. The remaining native failures were integration-boundary defects,
not evidence that the frozen Seed or Language 1.0 semantics should change:

- Windows accepted the canonical temporary root but some tests then formed an
  allocated child through a non-canonical spelling. Exact cache evidence
  correctly rejected that child.
- the model-provider owner still rebuilt and packaged a monolithic compiler
  build driver and lowerer before exercising the model provider. That obsolete
  prelude no longer represented the split compiler and consumed most of the
  owner time.
- the Echo package lock, provenance, approval, launch records, and verification
  tools retained the identity of an older current compiler product.
- one Linux front-door rejection reported only that its diagnostic differed,
  discarding the bounded diagnostic needed to distinguish a semantic failure
  from a runner or harness difference. Its cleanup also knew only an outdated
  list of nested malformed-fixture directories.

The exact current Linux analyzer and admitter reproduce all four nearby
sequence-read rejections with the expected status and one-line diagnostic in
five fresh isolated executions. No source-language correction is justified by
the available evidence.

## Decision

1. Canonicalize each allocated temporary child and derive its cleanup parent
   from that canonical child before creating cache or path evidence. Keep the
   canonical-path validation itself strict.
2. Make the model-provider owner consume the exact retained, digest-pinned
   `wvbuild` and `Wvb-To-Wvo` candidates. Compiler convergence owns rebuilding
   those tools; model-provider qualification owns deterministic compilation,
   lowering, linking, hosting, execution, and cross-target image checks.
3. Refresh the complete Echo identity chain from the current retained compiler:
   Lock 1, provenance, Bundle 1, capability approval, both Launch Record 3
   targets, their specifications, and every executable verifier.
4. Package the enlarged package-bundle writer and verifier through the bounded
   segmented compiler-packaging profile. Preserve the existing single-fragment
   output ceiling instead of weakening it for growing compiler tools.
5. Include the bounded observed diagnostic in exact source-analysis mismatch
   errors. Do not change a semantic status without reproducing the differing
   value.
6. Remove the exact validated Linux front-door work directory recursively at
   exit. Refuse cleanup outside the freshly allocated canonical prefix.

## Consequences

- Seed remains the unchanged bootstrap and recovery oracle. Current compiler
  behavior continues to live in the split analyzer and emitter.
- the model-provider owner no longer spends roughly 24 to 29 cold minutes
  reconstructing unrelated compiler tools. Both retained inputs are checked by
  byte length and SHA-256 before use.
- the Echo application WVB remains the byte-identical 927-byte product with
  SHA-256
  `b83890661281e79b17d14c49e7b971e37701c8112310b7b5f1f3f05e035dc713`.
  Its updated 17,009-byte Bundle 1 has SHA-256
  `a649a98c6d6f8dd2873f1b5097f74f613f7e3422929ef8838a7f6522bc464a0e`.
- Windows focused model-provider evidence passes 11 cases. Windows focused Echo
  application and command-launch evidence passes 9 and 10 cases respectively.
- a repeated Linux sequence-read diagnostic check passes 20 analyzer
  rejections across four fixtures and five fresh admitted source sets. This is
  focused diagnostic evidence, not a substitute for the final paired gate.
- cleanup remains bounded to one exact locally allocated directory while
  accommodating present and future nested malformed-fixture outputs.

## Reconsideration triggers

Rebuild compiler tooling inside the model-provider owner only if a named model
contract depends on compiler construction rather than compiler output. Change
the frozen Seed only through a separate recovery or security decision with new
immutable provenance. Alter a sequence-read semantic diagnostic only after an
exact differing diagnostic is captured and reduced. Replace the refreshed Echo
identity chain whenever its source, compiler, package metadata, or executable
bytes change again.
