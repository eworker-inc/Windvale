# Decision 0810: Use the split compiler for compiler-scale development

- Status: Accepted
- Date: 2026-08-21

## Context

Windvale retains an immutable native Seed so the compiler can be recovered from
a known artifact. The current compiler source is larger and has newer semantic
work than that Seed. The existing Generic-WIR Project 2 closure exposes the
boundary: with its canonical source order, the recovery Seed stops at source
binding 499, operation zero, while a retained later compiler completes it.

The Language 1.0 front door already reconstructs a current analyzer and emitter
as two executable products over the same compiler phases. Requiring the old
monolithic Seed front door to compile every current compiler-scale fixture would
make recovery capacity an accidental limit on Language 1.0. Adding another
independent compiler owner would instead repeat the expensive reconstruction
that the front door has already performed.

## Decision

1. Keep the immutable native Seed unchanged as bootstrap and recovery
   provenance. It is not the ordinary compiler-scale Language 1.0 front door.
2. Treat the reconstructed analyzer and emitter as two products of one evolving
   Windvale compiler, not as separate semantic compilers.
3. Compile the Generic-WIR Project 2 closure through the current split compiler
   after the Language 1.0 owner has reconstructed those products.
4. Compile it twice through the target-aware split cache and require identical
   output, the exact 1,065,737-byte WVB, and SHA-256
   `c8aa63e688ee53ed5ee72cc75db4b3852f0b6431a501a4f6230d680b6a4dcefc`.
5. Package one result under profile 1 and require a silent exit value of `42`.
6. Count these as four cases in the existing Language 1.0 owner: first build,
   deterministic rebuild, exact artifact identity, and packaged execution.
   Do not create another heavy verification owner for the same compiler build.
7. Continue changing the active compiler as needed to implement the frozen
   Language 1.0 contracts. Once it compiles its own accepted source, call the
   product the Windvale 1.0 compiler; retain “Seed” only for the recovery role.

## Evidence

The current Windows analyzer publishes a 104-byte WVCA, 196,496 WVLB bytes, and
3,212,716 WVIR bytes for the canonical Generic-WIR project. WVSS retains source
path metadata, so its temporary-workspace-dependent byte length is diagnostic
rather than a reproducibility identity. The current emitter publishes 470
functions and 880,773 code bytes in the exact WVB above. The hosted
five-fragment application returns `42` and writes no output.

The evolving compiler itself remains within the unchanged 32 MiB native-object
limit after compacting private generic-parameter validation records, passing
only the source-symbol lookup and nominal counts where the complete 22-field
summary was unnecessary, and sharing construction of successful type bindings.
The current analyzer contains 478 functions in an exact 1,073,582-byte WVB. Its
33,545,634-byte complete native object is 8,798 bytes below the limit, and its
packaged 33,549,312-byte Windows product has SHA-256
`bd1413394e3e08d59ec992d6a6da8b65a10f3db1a8b1d61c93acd8f58c80dd45`.
The refactor does not change the native-object bound or source semantics.

The Language 1.0 owner now performs the deterministic double build and
cacheable package/execute checks after reconstructing its current analyzer and
emitter. The verification-owner registry records 160 Language 1.0 cases, 36 of
which are compiler cases.

The independent segmented-toolset reconstruction owner uses the immutable
992,412-byte bootstrap analyzer as its compiler-scale staging sentinel. It does
not ask the recovery Seed to rebuild evolving compiler source. This preserves
the segmented stager's exact compiler-scale evidence without duplicating the
current split compilation owned by the Language 1.0 front door.

## Consequences

Compiler development is no longer falsely gated by the historical Seed's
capacity, while recovery remains reproducible and untouched. The split phase
artifact becomes the supported compiler-scale development boundary. This also
avoids reconstructing large compiler products in another owner and keeps the
new check close to the language changes it protects.

This decision does not alter source semantics, WVIR, WVB, packaging profiles,
or release qualification. It does not yet thread WVGT into main WIR or consume
generic nominal materialization in Source WVB. Those remain the next Slice 4
integration checkpoints.

## Reconsideration triggers

Revisit this decision if the split compiler ceases to be semantically equivalent
to the diagnostic one-shot path for shared supported inputs, if compiler-scale
projects require a new explicit phase contract, or if a newly reconstructed
bootstrap artifact can replace the recovery Seed under a separate provenance
decision.
