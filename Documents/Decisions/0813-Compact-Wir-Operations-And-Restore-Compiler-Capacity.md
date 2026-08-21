# Decision 0813: Compact WIR operations and restore compiler capacity

- Status: Accepted with current-Windows reconstruction evidence; independent Linux qualification pending
- Date: 2026-08-21
- Advances: [typed source IR](../../Specifications/Compiler-Source-Wir.md), [hosted-container packaging](../../Specifications/Windvale-Native-Hosted-Container-Packaging.md), and [Decision 0812](0812-Thread-Generic-Nominal-Evidence-Through-Main-Analysis.md)

## Context

Connecting generic nominal evidence to main analysis made an existing compiler
scale problem visible. The exact 1,727,318-byte split-emitter source closure
exhausted the fixed 64,000,000,000-instruction profile-7 budget and returned
native runtime status `2`. Its sampled working set was about 114 MiB, so this was
instruction capacity rather than memory exhaustion.

Two sources of avoidable work were present. Source WIR scanned every function
signature for generic nominal applications even when the validated declaration
directory contained no generic record or variant. Every persisted operation also
repeated an eight-byte source span that Source WVB never consumed. The owning
block and result temporary were considered for removal, but both are semantic:
operations are not globally grouped by block, and block-order traversal cannot
derive function-local temporary definitions. Those experiments were rejected
before this decision.

## Decision

1. Inspect the already validated WVSD declaration directory before generic
   nominal signature scanning. When it contains no generic record or variant,
   skip the entire scan and retain the empty WVGT catalog.
2. Reduce each persisted WIR operation from 40 to 32 bytes by removing only its
   source byte offset and length. Retain owning block, operation kind, result
   shape, result temporary, first operand/count, target, and auxiliary value.
   Construction failures continue to carry exact source locations.
3. Advance ordinary WVIR from 1.1 to 1.3 and specialized WVIR from 1.2 to 1.4.
   Reject the obsolete layouts; do not add a legacy decoder or parallel compiler
   path.
4. Give hosted-container profile 7 an 80,000,000,000-instruction bound. Profiles
   1 through 6 retain 64,000,000,000. The increase is measured capacity for the
   split compiler, not a speed optimization or a reason to change ordinary tool
   application bytes.
5. Reconstruct only the metadata, runtime-header, and planner WVBs plus their
   paired profile-1 outer applications. Preserve specialized outer adapters such
   as durable segment-set publication. Bind the complete 72-artifact candidate
   to one inventory.
6. Advance the portable split analyzer/emitter bootstrap checkpoint atomically
   to the current WVIR versions. Remove the Language 1.0 gate's unused
   monolithic compiler-source-set build and package the active split analyzer
   and emitter under profile 7.

## Evidence

On the exact final emitter source closure, the retained 40-byte-operation oracle
publishes 4,144,676 WIR bytes. The current 32-byte layout publishes 3,526,316
bytes: 618,360 fewer bytes, or 14.919 percent. Both paths emit the exact same
895,787-byte WVB at SHA-256
`ea8ade4774236a84208242a6e17d271077b9a4a94fb40c47ec487d43a97b2b94`.

The final analyzer contains 477 functions and 811,632 code bytes in a
992,412-byte WVB at SHA-256
`26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120`.
Its eight-fragment profile-7 Windows package is 31,740,416 bytes at SHA-256
`52c6cccdcaed1e99ea87759751d232e0f39bd1ed923d0555e4da5f4b236b442f`.
It analyzes the previously failing emitter closure successfully under the
80,000,000,000 bound.

The final emitter contains 470 functions and 742,026 code bytes in the WVB
identity above. Its five-fragment profile-7 Windows package is 19,718,656 bytes
at SHA-256
`3f5d3d6baf9a41926b1e0c9068e31aea0612df51c5675ff69228f54874ab5347`.
Together, those final packages reproduce the emitter WVB byte for byte. A
content-keyed repeat takes 0.295 seconds on the current Windows host.

The development bootstrap now carries that exact 992,412-byte analyzer and
895,787-byte emitter pair. The gate no longer spends an additional bounded run
constructing a monolithic `Compiler.wvb` that it never reads.

The compiler-scale Generic-WIR sentinel advances to a deterministic
1,145,513-byte WVB at SHA-256
`d56de5ae356a5e3dd6a36f3665792dce0e2c7ba968826c92e27ba0f4a046243e`.
Its analyzer publishes 216,512 WVLB bytes and 2,962,236 WVIR bytes; its emitter
publishes 537 functions and 947,713 code bytes. The independent compiler-aligned
WVB verifier accepts it. The gate does not widen native staging output or
general runner call depth merely to execute this compiler-scale product.

The generic-nominal main fixture publishes exact 238-byte WVSS, 104-byte WVCA,
192-byte WVLB 1.3, and 320-byte WVIR 1.3 artifacts and passes all 12 structural
cases. The profile-1 hosted-packaging suite passes all 5 cases, including exact
Windows/Linux reproduction, malformed-WVB rejection, destination/input
preservation, and scratch cleanup. The final hosted-toolset inventory contains
72 valid entries in 6,927 bytes at SHA-256
`23f2bf3e62212d37d2eb07ab95620e9c708181e7d6e899e8dc409d89e72bbe8e`.

These are current-Windows development results. They are not independent Linux
execution or paired-host qualification.

## Consequences

The compiler carries less intermediate data and avoids one irrelevant semantic
scan on ordinary source. The higher bound restores measured profile-7 capacity,
but it does not make an uncached analysis faster by itself. Content-addressed
analysis and emission checkpoints remain the primary repeat-work optimization.

Source tools that require operation-level diagnostics must retain them while
constructing or regenerate them from source; they cannot expect source spans in
published WVIR. The next performance refactor should separate successful product
construction from diagnostic replay and independent validation more sharply,
then measure traversal/allocation cost before widening any other bound.

## Reconsideration triggers

Reconsider the operation layout if a backend needs independently persisted
operation source maps, if operations stop carrying explicit block or temporary
identity, or if streaming WVIR requires noncontiguous ownership. Reconsider the
profile-7 ceiling if representative compiler closures approach 80,000,000,000,
if a lower measured bound becomes sufficient after refactoring, or if either
permanent host cannot enforce the same exact `u64` counter contract.
