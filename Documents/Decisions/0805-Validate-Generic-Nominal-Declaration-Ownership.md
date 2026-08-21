# Decision 0805: Validate generic nominal declaration ownership

- Status: Accepted
- Date: 2026-08-20

## Context

The declaration grammar already accepts generic parameters on records,
variants, and functions, and WVGT now defines collision-free concrete generic
nominal identity. Source Symbols nevertheless interpreted generic parameters
only for functions. A general record field `Value: T` therefore failed as an
unknown type, while the exact Foundation `Option` and `Result` variants avoided
that failure through an early return that also skipped ordinary variant
validation. A generic template could additionally bind as a bare nominal type.

Connecting full generic uses and WVB materialization before declarations own
their parameter namespace would mix structural validation, concrete identity,
substitution, and emission in one change.

## Decision

1. Generalize the existing bounded function-generic descriptor parser to all
   record, variant, and function declarations. Retain its historical public
   name while downstream function lowering still consumes that contract, rather
   than emitting a second wrapper solely for terminology.
2. Validate record and variant generic parameters in declaration order and
   reject duplicate type/constant names with `Duplicateˉparameter`.
3. During template validation, admit a declared type parameter as a complete
   unresolved field or payload. Reject a declared constant in type position.
   Defer parameters nested inside collection or nominal type syntax to the
   recursive type-use binder so this checkpoint does not create a second
   partial generic parser.
4. Treat a generic record or variant declaration as a template. Reject its bare
   use with `Unknownˉtype`; do not assign the template an ordinary concrete
   nominal shape.
5. Remove the Foundation generic variant bypass. Validate the exact edition-1
   `Option<T>` and `Result<T, E>` declarations through the same parameter and
   payload path while retaining their already-proven specialized use shapes.
6. Defer full-arity `Box<i32>` and nested generic uses, WVGT admission,
   substituted field/case validation, typed WIR carriage, and reachable WVB
   materialization to the next connected checkpoint.
7. Give this boundary an independent 12-case verification owner. Build its
   compiler-sized fixture through the project cache and four-fragment segmented
   hosted path so ordinary development does not need to rerun the complete
   Language 1, storage, or OS gates.

## Evidence

The focused fixture admits generic records, variants, phantom parameters,
ordinary nominals, and the exact Foundation generic declarations carried in
WVSS 2. It rejects duplicate or kind-mismatched parameters, a type parameter
misused as a generic template, builder storage in a record, and a bare generic
nominal template.

Its 604,172-byte WVB has SHA-256
`fa056f720caa741d3b3312e97ccd0b5dfce46559c07871f0e99eca229e06ca85`.
The 15,083,520-byte four-fragment hosted Windows executable has SHA-256
`c8e87418ca758bab33f9f16ff93d36f74f1a8f2e5cb962a6bc56c1c29ab4d83a`,
returns `42`, and writes no output. The maintained owner exposes build, package,
and execute progress and uses content-keyed checkpoints.

The complete compiler source sentinel still rejects the generated compiler at
the native stager's immutable `Outputˉlimit`. An isolated detached build of
unchanged `origin/main` rejects at the same boundary with 600 functions,
1,048,159 code bytes, and 1,262,958 module bytes. The sentinel is therefore
recorded as an existing main limitation rather than attributed to this
checkpoint or bypassed by increasing its safety bound.

## Consequences

Generic nominal templates now have an explicit, validated source identity and
cannot escape as fake concrete types. Foundation generic declarations no longer
depend on a validation exception. The change deliberately does not claim that a
general concrete generic nominal use compiles; WVGT is still unconnected to
type-use binding and emitted type materialization.

The focused owner shortens local evidence for this boundary, but the central
`Source-Symbols-Core.wv` path still selects the broader front-end owners until a
separate source-symbol verification investigation can preserve all existing
diagnostics. An attempted consolidation exposed an older segmented-native
failure in the unrelated cross-module diagnostic-field assertion; this
decision neither weakens nor reclassifies that contract.

## Reconsideration triggers

Rename the historical function-generic descriptor only if every consumer can
migrate without adding duplicate compiler code. Broaden the focused
owner to the complete Source Symbols boundary only after its existing demo runs
unchanged through the segmented native backend and retains every diagnostic
field assertion.
