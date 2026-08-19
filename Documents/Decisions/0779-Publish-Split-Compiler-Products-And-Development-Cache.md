# Decision 0779: Publish split compiler products and a development cache

- Status: Accepted
- Date: 2026-08-19

## Context

Decision 0778 created an independently validated boundary between source/type
analysis and WVB emission, but it did not provide executable front doors or a
persistent development cache. Agents still had to reconstruct a monolithic
compiler product, and an early cache prototype rehashed roughly 47 MiB of
analyzer and emitter executables on every hit. That made a correct hit take
about 285 milliseconds for a project whose one-shot compilation took about 306
milliseconds, defeating most of the intended feedback-time benefit.

The compiler products are large enough that neither should be treated as a
casual per-test input. They also cross different packaging limits: the analyzer
needs eight bounded native fragments and the emitter needs five. Raising those
limits would hide product pressure rather than separate the real phase owners.

## Decision

1. Publish a hosted `wvanalyze` product that accepts the exact ordered Project
   2 source closure and writes WVSS, WVCA, WVLB, and WVIR.
2. Publish a hosted `wvemit` product that treats all four values as untrusted,
   validates them, and emits unoptimized portable WVB through the shared
   prepared backend.
3. Retain the one-shot compiler as the compatibility and equality oracle. The
   split products do not create another grammar, semantic analyzer, typed IR,
   optimizer, opcode selector, or WVB serializer.
4. Use small exact producer-identity manifests as persistent key inputs. The
   packaging step hashes each executable once. A cache hit reads the identities
   and validates the bounded checkpoint values, but does not read or launch the
   executables. A miss hashes the selected executable before and after its use.
5. Bind the complete Project 2 source closure, workspace marker, producer
   identities, explicit `portable-wvb-v1` target, host family, and analysis key
   at the owned cache layers. Reject Project 3 and implicit optimization until
   their profile and option inputs receive exact key contracts.
6. Publish checkpoints through private candidate directories and remove the
   exact locally created candidate in `finally` after every failure or lost
   race. Never remove a path whose resolved parent and key prefix do not match
   the selected cache family.
7. Use one focused compiler/language verification gate for each coherent
   Language 1.0 implementation slice. Run broad storage, OS, complete native,
   and dual-host qualification once at final integration, unless a slice
   directly changes one of those boundaries.

## Evidence

The analyzer compiles to a 949,355-byte WVB and packages as a 30,276,096-byte
Windows x64 executable in eight bounded fragments. Its native plan contains
30,245,965 machine-code bytes and 2,104 relocations. The emitter compiles to a
746,557-byte WVB and packages as a 16,976,384-byte executable in five bounded
fragments; its plan contains 16,945,060 machine-code bytes and 1,232
relocations. No compiler, object, module, or fragment bound was raised.

For `Windvale-Native-Test-Source-Descriptor.wvproj`, the retained one-shot
compiler produced a 12,633-byte WVB in about 306 milliseconds. Direct split
execution took 335 milliseconds for analysis and 148 milliseconds for emission
on this deliberately tiny project, so an uncached split is slower as expected.
All direct, split, cold-cache, and warm-cache products have SHA-256
`53de13cfb20e237e71d5e34e6010f193eccbe815cc58a214b8c5ee2acf76bcc2`.

After moving executable hashes to packaging identities and explicitly ordering
the Project 2 root plus sorted dependencies, five Windows x64 warm cache samples
took 155.4, 142.7, 143.2, 149.7, and 142.8 milliseconds, with a
143.2-millisecond median. The prior executable-rehashing hit took about 285
milliseconds. The focused forced-failure test supplies a mismatched analyzer
identity, observes rejection, and proves that no `.new-*` directory remains.

The named focused owner completes four checks: the 956,883-byte source-analysis
corruption fixture, 949,355-byte analyzer, and 746,557-byte emitter all
recompile to exact reachable-product WVB identities, followed by forced-failure
cache cleanup. Each long compile reports a heartbeat every 30 seconds. The
planner selects only this owner for the separated compiler boundary; it selects
no storage or OS owner.

## Consequences

Analyzer-only changes can rebuild and verify the analyzer without rebuilding
the emitter. Emitter-only changes can reuse valid analysis evidence. Downstream
development work can reuse byte-identical WVB without source analysis or large
producer hashes, while corrupt phase bytes still fail closed at the
checkpoint and compiler validation boundaries.

Small projects may still compile faster through the one-shot product on a cold
cache. The split is valuable for warm reuse, phase ownership, and growing
compiler workloads; it is not presented as a universal cold-build speedup.
Qualification remains cache-independent.

## Non-decision

This checkpoint does not remove the one-shot compiler, cache localized Project
3 profile inputs, enable optimized emission, qualify Linux equality, execute
the compiler-heavy corruption fixture through an unsupported backend, complete
generic `Option` or `Result`, or run broad storage and OS verification.

## Reconsideration triggers

Reconsider the product boundary if representative larger projects do not gain
from reusable analysis, if checkpoint validation dominates warm feedback, or
if a compiler service can retain equally exact producer and project generations
with lower process-start cost. Reconsider the verification policy when a slice
changes storage, OS, ABI, serialized-format, or cross-host semantics directly.
