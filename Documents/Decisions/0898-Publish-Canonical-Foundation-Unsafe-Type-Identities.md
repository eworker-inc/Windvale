# Decision 0898: publish canonical Foundation unsafe type identities

## Status

Accepted and implemented as a focused local Windows checkpoint on 2026-08-31.
This decision does not complete Slice 8 or claim paired-host qualification.
Foundation unsafe operations, authenticated Foreign-call WVIR, WVB lowering,
verifier/runtime/native containment, and one migrated runtime or OS boundary
remain pending.

## Context

The frozen Language 1.0 System/FFI surface names four representation-hidden
generic values: `Foreignˉpointer<T, Abi>`,
`Nullableˉforeignˉpointer<T, Abi>`, `Foreignˉscratch<Abi>`, and
`Foreignˉwriteˉregion<Abi>`. The compiler can parse and bind those type
spellings, and Decisions 0889 and 0895 provide a bounded semantic oracle and
authenticated Foreign declaration facts, but no canonical source module
previously owned the nominal identities.

Decision 0889 therefore prohibited ordinary record substitutes while they
would have been forgeable. That was a necessary interim restriction, not a
requirement to add another public source construct before the System/FFI path
could advance. The typed-WVIR phase now has enough nominal, generic-layout,
module, edition, and profile evidence to recognize a closed canonical set and
reject ordinary value operations on it.

Adding the future operations at the same time would mix three boundaries:
nominal type identity, compiler-owned production/observation, and authenticated
native ABI execution. The dependency-safe checkpoint is to publish the types
and exact failure variants while keeping the values unproducible and
unobservable from ordinary source.

## Decision

1. Publish `Libraries/Foundation/Unsafe/Unsafe.wv` as the canonical edition-1
   System library module `Foundationˉunsafe`.
2. Represent the four current compiler-owned opaque identities as generic
   records with one `Opaqueˉidentity: u64` physical field. This field is a
   private compiler representation device, not public source API or address
   evidence.
3. Recognize an opaque identity only when all of the following agree: admitted
   edition 1, System profile, exact module `Foundationˉunsafe`, exact type
   name and generic arity, valid one-field generic record layout, and exact
   `Opaqueˉidentity: u64` field.
4. Reject named construction and direct or chained field observation of such an
   identity with appended typed-WVIR status `Invalidˉunsafeˉvalue = 48`.
   Preserve every earlier status value.
5. Do not reject same-named records from another module. Opacity is nominal and
   cannot be acquired accidentally from spelling alone.
6. Publish the frozen `Foreignˉmemoryˉfailure` and
   `Foreignˉpointerˉfailure` variants in the same module, including the
   exact `Foundationˉmemory.Allocationˉfailure` payload.
7. Publish no unsafe Foundation operation in this checkpoint. Ordinary source
   therefore has no producer or observer for the opaque values. Later compiler
   intrinsics must be the only producers and observers and must enforce the
   accepted ownership, borrow, range, alignment, generation, alias, lifetime,
   effect, and capability contracts.
8. Make no WVIR operation, WVB, object, native ABI, or runtime format change.
   The physical field is compiler-private staging representation and must not
   be treated as a stable serialization or ABI promise.
9. Supersede Decision 0889's interim prohibition on source declarations only
   for these exact compiler-recognized identities. Its prohibition on forgeable
   ordinary substitutes and every semantic-oracle safety rule remain in force.

## Evidence

The complete current split Analyzer was rebuilt once after the compiler change.
Analysis consumed 2,168,862 source bytes and published a 104-byte manifest,
326,328 binding bytes, and 3,897,912 WVIR bytes. Optimized emission produced a
1,584,518-byte Analyzer WVB at SHA-256
`7deb3588e9fcba32b90ee16b66ffb115a7e96d0603377610e5917e4b72924075`.
The packaged Windows Analyzer is 51,897,344 bytes at SHA-256
`cc0e23d23e8a059862bf350777d9029c9c59dbe2f8103834e46684c3217198de`.

The paired emitter closure remains below its immutable WVIR bound at 4,097,080
bytes, leaving 97,224 bytes. Its WVB is 1,575,772 bytes at SHA-256
`51b9075ae1f4f3d5a15ab1e608bfbc56ce6da325a49e4bcbe052d92ea57862a1`.

`Tools/Native/Test-Language-1.0-Unsafe-Type-Surface.mjs` injects the retained
Analyzer into ten bounded descriptor-free WVSS 2 cases. The canonical module
and all four identities are valid in type positions; four construction and four
field-observation attempts reject with exact status 48; and a same-named
noncanonical record remains constructible and observable. The focused run
reports:

```text
native language 1 unsafe type surface status=Passed cases=10 valid=2 rejected=8 opaque-identities=4
```

This is local implementation evidence. The retained Analyzer is reused while
its compiler inputs remain unchanged; this checkpoint does not justify another
complete build or the final paired-host gate.

## Consequences

- Frozen System/FFI signatures now have real canonical type and failure
  identities instead of paper-only names.
- Source cannot forge a pointer, scratch owner, or write region by constructing
  the compiler's physical record shape or reading its carrier field.
- The next checkpoint can add the smallest typed Foundation intrinsic producer
  and authenticated Foreign-call operation without inventing types at the same
  time.
- The current representation is deliberately not a general user-definable
  opaque-type facility. A later edition may add such syntax independently.

## Reconsideration triggers

Reconsider this source representation if an ordinary source path can construct,
observe, serialize, copy, retain, or escape a compiler-produced identity; if a
new unsafe type needs a different ownership class; if package/module identity
cannot protect the canonical namespace; or if downstream lowering would expose
`Opaqueˉidentity` as ABI. Any such case must fail closed and gain focused
evidence before the first producer is admitted.
