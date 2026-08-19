# Decision 0778: Separate bounded source analysis and WVB emission

- Status: Accepted
- Date: 2026-08-19

## Context

Decision 0777 recovered compiler space but measured only 337,262 bytes of
headroom below the fixed 32 MiB native object ceiling. The remaining Language
1.0 typed-failure work needs generic specialization identity, substituted
Foundation fields, and deterministic type emission. Keeping every new analysis
and emission concern in one native product would quickly consume that margin
and would continue to make agents rebuild and reverify unrelated compiler
phases.

WVSD, WVLB, and WVIR already provide versioned and independently validated
evidence, but there was no small artifact that bound their exact counts and no
WVB entry point that consumed previously validated evidence. The one-shot WVB
function owned analysis and emission in one body.

## Decision

1. `Compilerˉsourceˉanalysis` publishes a fixed 104-byte `WVCA 1.0` manifest,
   canonical `WVLB 1.1`, and canonical `WVIR 1.1` as separate bounded values.
2. WVCA carries only lengths and counts that an independent consumer can prove.
   It does not carry trusted diagnostics, timing measurements, hashes, source
   bytes, or an unverified cache identity.
3. Validation rescans the supplied WVSS, reconstructs source symbols, compares
   every source count, validates WVLB, compares WVIR header counts, and validates
   WVIR before returning prepared evidence.
4. The existing WVB backend body becomes
   `Compilerˉemitˉpreparedˉsourceˉwvb`. The retained one-shot compiler performs
   analysis and delegates to that same body. There is one emitter and one set of
   source semantics.
5. `Compilerˉsourceˉemission` is the safe adapter from untrusted WVCA/WVLB/WVIR
   values to prepared WVB emission. It never invokes emission after failed
   validation.
6. WVCA is an internal compiler-phase contract, not a distributable format.
   Canonical WVB remains the verified cross-host distribution boundary.
7. Development verification may validate analyzer-only changes independently
   from emitter-only changes. Broad storage, OS, and dual-host qualification is
   deferred to the final coherent Language 1.0 integration state unless a
   changed boundary specifically requires it.

## Evidence

The source-analysis core compiles successfully to a 952,903-byte module with
386 retained functions and 787,036 code bytes. The corruption fixture compiles
successfully with 392 retained functions, 791,178 code bytes, and a 957,810-byte
module. It encodes deterministic publication plus manifest, WVLB, and WVIR
corruption assertions. The emission-side closure independently compiles with
349 retained functions, 615,041 code bytes, and a 743,989-byte module: 34.3%
smaller than the reconstructed one-shot compiler module before native packaging.

After extracting the backend body, the complete compiler reconstructs with 506
functions, 939,530 code bytes, and 1,132,278 module bytes. Relative to Decision
0777, the compatibility wrapper costs one function, 106 code bytes, and 194
module bytes. No source rule, WIR operation, WVB opcode, serialized WVB output
rule, or admitted size limit changes.

The focused fixture cannot yet be executed honestly by the available scalar,
general native, or WebAssembly paths: they reject the compiler-heavy closure at
their existing operation, module, or code boundary. This checkpoint records
successful source compilation and encoded validation coverage, not a false
runtime pass. A compiler-capable qualified executor must add runtime equality
evidence before the separated tools replace the final one-shot release gate.

## Non-decision

This checkpoint does not yet publish analyzer or emitter command-line products,
define a persistent cache key, remove the one-shot compiler, implement generic
declarations, specialize `Option<T>` or `Result<T, E>`, widen `try`, migrate a
manual status family, or claim paired-host qualification.

## Consequences

Source/type-analysis changes and WVB-emission changes now have an explicit
validated seam. A future development cache can reuse analysis without trusting
local bytes, and verification can select the owner of the changed phase instead
of repeatedly exercising storage and OS workloads.

The immediate next step is to give the analyzer and emitter small command-line
front doors with a cache key that binds WVSS bytes, compiler identity, target,
optimization mode, and source-profile inputs. Once measurements prove smaller
products and byte-identical WVB output, Slice 3 generic specialization belongs
in the analyzer side and deterministic specialized type serialization belongs
in the shared emitter.

## Reconsideration triggers

Reconsider WVCA only if the emitter needs another independently provable field
or if retaining source for revalidation becomes more expensive than a stronger
bounded integrity design. Reconsider the product split if measured analyzer and
emitter packages do not reduce build, cache, or verification cost while
preserving one-shot byte identity.
