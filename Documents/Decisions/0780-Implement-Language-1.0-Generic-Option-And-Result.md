# Decision 0780: Implement Language 1.0 generic Option and Result

- Status: Accepted; Foundation-only representation superseded by Decision 0819
- Date: 2026-08-20

## Context

The first Slice 3 checkpoint implemented value-producing `try` over one
concrete nominal Result-shaped variant. It deliberately required the operand
and containing function to use the same result shape. That proved control-flow
lowering, but it did not publish the frozen generic Foundation identities,
permit `Result<T, E>` to propagate into `Result<U, E>`, or establish how
concrete source specializations become ordinary WVB types.

The complete general generic contract belongs to Slice 4. Implementing an
unbounded or inference-driven generic solver solely for Option and Result would
prematurely choose that design. Keeping structural Result recognition would be
worse: any lookalike could silently acquire language-defined propagation
semantics.

## Decision

1. Publish edition-1 source modules for the exact frozen
   `Foundationˉoption.Option<T>` and `Foundationˉresult.Result<T, E>` variant
   identities. The compiler recognizes the complete module, declaration,
   parameter, case, field-name, field-type, order, and arity contract rather
   than structural similarity.
2. Admit full-arity type uses only for those two families in Slice 3. Each
   argument is one implemented primitive or one of the first 1,024 ordinary
   record, enum, or variant identities. Nested generic arguments, collections,
   capabilities, `never`, bare names, wrong arity, and unsupported shapes fail
   closed.
3. Carry each concrete use as a private compact shape through WVLB and WVIR.
   These numbers are compiler-phase evidence, not source identities or a new
   serialized public type system.
4. Collect at most 256 distinct used Foundation generic shapes before WVB type
   emission, preserve their deterministic first use across WIR function
   signatures and then bindings, and emit each as an ordinary private WVB
   variant specialization. Fixed-width private names `__WvZ000` through
   `__WvZ255` preserve canonical name order across rank 10. Existing variant
   bytecode, verifier, runtime, and native rules remain the execution contract.
5. Define edition-1 `try` over exact Foundation Result. The operand
   `Result<T, E>` may propagate from a function returning `Result<U, E>`. The
   success path extracts `T`; the failure path returns the original value when
   the shapes agree and otherwise reconstructs exactly
   `Result<U, E>.Failure(Error: E)`. A different `E` requires an explicit
   source adapter.
6. Retain the earlier descriptorless Seed statement-try behavior only for the
   planned source migration. Do not extend structural recognition or add a
   second compiler path.
7. Demonstrate manual-status migration at the edition boundary by converting a
   `Valid`/`Value`/`Error` record to Result through an explicit error-domain
   adapter. Existing Seed compiler and library families migrate coherently only
   when their source area moves to edition 1; no file may import across the
   edition boundary implicitly.
8. Add the English Result public-source catalog to the pinned input lock. This
   materializes already frozen Foundation signatures and source labels; it does
   not change Decision 0767 language semantics.

## Evidence

`Foundation-Generic-Result.wv` and the exact Option and Result modules analyze
to a 3,169-byte admitted source set, 104-byte WVCA manifest, 900-byte WVLB
directory, and 5,792-byte WVIR directory. Independent emission twice produces
the same 3,383-byte WVB with SHA-256
`64da5d52c01301c54f9391c9f8cdc3f7a8000e7c21694b06baa096354ba1d09f`.
The current compiler-aligned verifier accepts it and the source-built scalar
runner returns `42`.

The fixture covers Option presence and absence, Result construction and
matching, different-success/same-error propagation, statement `try`, one
manual-status adapter, one explicit error-domain adapter, and 16 concrete
specializations crossing the rank-9/rank-10 private-name boundary. Four malformed
sources independently reject missing and extra type arguments, bare Result use,
and different-error `try` without publishing phase evidence or WVB.

The profile-aware admission product contains 40 functions in an 82,781-byte
WVB and packages as a 797,184-byte Windows x64 executable in one fragment. The
analyzer contains 403 reachable functions in a 976,748-byte WVB and packages
as a 31,013,888-byte executable in eight fragments. The emitter contains 368
functions in a 775,522-byte WVB and packages as a 17,712,128-byte executable in
five fragments. All remain within the existing segmented limits; no compiler,
object, fragment, verifier, or runtime bound was widened.

The English Result catalog has SHA-256
`f8bb5f44b2f64af34bb2eea54f7050fba0136ebf69a7bab1b1d28e8968357508`.
The updated source-input lock has SHA-256
`9e2ca572552ed52ed496142d18539f2f55fed2bbdfb1ec602f283b5d72386f3e`.
The current migration-verifier closure contains 251 files and 1,726,783 bytes;
its 46,260-byte identity stream has SHA-256
`de39b8f4042c98d34ff3676ec111a7ffca6e91c529f0e40f2250a824c54ad415`.

## Consequences

Language 1.0 code can use null-free optional values and typed failures with the
canonical public identities now, and `try` no longer forces the successful type
to remain unchanged. Concrete specializations remain normal verified WVB
variants, so no generic runtime, reflection table, dictionary dispatch, or new
opcode is introduced.

Source areas still written in Seed keep their current manual records until a
coherent edition-1 migration can update their producers and consumers together.
That is migration sequencing, not permanent legacy semantics.

## Non-decision

This decision does not implement generic functions, general generic user types,
protocol resolution, nested generic arguments, collections, inferred type
arguments, overload selection, result-context search, generic dictionaries,
monomorphized native code policy, localized Result execution, Project 3
profile-aware caching, or paired-host qualification. Those remain Slice 4 or
final integration work as assigned by the migration plan.

## Reconsideration triggers

Reconsider the compact shape evidence when Slice 4 introduces general bounded
specialization or nested arguments. Reconsider specialization naming if public
debug metadata requires a stable source-spelled generic identity. Reconsider
the 256-use bound only with a representative workload, explicit memory/time
measurement, and a format that still rejects excessive untrusted type graphs
before expensive emission.
