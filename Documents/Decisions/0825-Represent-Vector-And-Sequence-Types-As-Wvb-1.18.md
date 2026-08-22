# Decision 0825: Represent Vector and Sequence types as WVB 1.18

## Status

Accepted on 2026-08-22.

## Context

Decisions 0823 and 0824 established exact Language 1.0 Vector and Sequence
identities, their element types, and their different ownership classes in the
compiler. Publication still stopped before WVB because the retained
fixed-capacity `sequence<T, N>` and `builder<T, N>` encodings cannot represent a
runtime-budgeted `Vector<T>` or its immutable `Sequence<T>` result. Reusing
those encodings would make an implementation capacity part of source type
identity and would preserve the collection model that Language 1.0 replaces.

The compiler-scale emitter also approached the native bootstrap's fixed 4 MiB
WVIR product limit. Adding another direct-compilation dependency to the
prepared emitter would have crossed that limit even though the emitted WVB
format change is small.

## Decision

1. WVB 1.18 adds Types kind `5` for the exact Language 1.0 `Vector<T>` identity
   and kind `6` for `Sequence<T>`. Each descriptor contains its canonical
   compiler-private name followed by one exact non-`never` element shape. It
   contains no source maximum, backing capacity, allocator, or authority.
2. Value shape `23` followed by a `u32` Types index denotes kind-5 Vector.
   Shape `24` followed by a `u32` Types index denotes kind-6 Sequence. The
   referenced descriptor kind and element type must agree exactly.
3. Any kind-5 or kind-6 descriptor or shape selects minor version 18. A 1.18
   reader retains all earlier 1.11-through-1.17 vocabulary, and the new forms
   are invalid under every earlier minor version.
4. This checkpoint represents collection identities in function, local,
   field, payload, and nested type metadata. It adds no allocation, append,
   freeze, length, indexing, borrowing, storage layout, or collection opcode.
   Those operations require a separate runtime-backed representation decision.
5. The prepared WVB emitter remains capability-free and independent of source
   profiles. Direct source compilation and profile admission move to
   `Compilerˉsourceˉwvbˉcompilation`, which composes the prepared emitter with
   analysis and profile inputs. Consumers import the narrow owner they use.
6. The lexer uses its single bounded two-limb decimal accumulator for every
   integer literal width. Narrow forms reject a nonzero high limb and then
   apply their exact bound. This removes a duplicate Foundation decimal parser
   from every compiler source closure without changing accepted source.

## Consequences

- The 436-byte fixture publishes two exact descriptors and retains exported
  `Vector<i32>` and `Sequence<i32>` function signatures. It has SHA-256
  `c51529baa7fb7b5cfb24e2508520044cce9f2661b9fb1dccb2321b5e122ec73d`.
- Six focused cases prove compiler-aligned validation, scalar execution of the
  unaffected `Main` result, and exact semantic rejection of an old minor,
  invalid element shape, kind confusion, and Types-index confusion. The scalar
  runner parses the metadata but does not yet construct or execute collection
  values.
- The refactored verifier is 222,399 WVB bytes at SHA-256
  `9424d62eba7f5efb37363bcef439afeb198c943a1439703bb3492378310a24d0`.
  Its 1,827,840-byte Windows application accepts the fixture. The executable
  verifier's encoded-shape helper also keeps its largest native function below
  the fixed frame-local limit.
- The prepared emitter source set is 1,899,183 bytes and publishes 3,780,080
  bytes of WVIR, below the 4 MiB bound. Its 1,013,482-byte WVB has SHA-256
  `3fb526c3298406a3ba71df5e074d58d000532e80640421fc4d665389d7a0ea0d`.
  The current analyzer publishes 3,287,604 bytes of WVIR and a 1,098,751-byte
  WVB at SHA-256
  `4e24d6312b01efbd8caeb155ed1a0ce4339f4debe3cf2d77e300798e11ccd68b`.
  Rebuilding both with that pair produces byte-identical WVB products.
- `Source-Wir-Core.wv` remains large, but this change adds no collection
  serializer or runtime implementation to it. Future extraction follows a
  cohesive expression-lowering ownership boundary; line count alone does not
  justify numbered or mechanical fragments.
- Existing WVB consumers remain at their named maximum versions unless this
  decision explicitly advances them. Metadata parsing is not a claim of
  executable Vector or Sequence support.
- The verification registry contains 108 owners and 5,155 declared cases at
  SHA-256
  `7f102e24a7035aab8c0c7c135e9df44bfc864fc2e772e66fe8f85ad1108afc72`.
  The dual-host Language 1.0 owner declares 371 cases, including the six new
  type-representation cases.

## Reconsideration triggers

Reconsider the descriptors if the runtime-backed operation slice proves that
portable type identity needs another semantic parameter. Do not add an
allocator identity, hidden capacity, host pointer size, or provider handle to
the type merely because one implementation needs it. Reconsider the compiler
module split only if prepared emission and direct source orchestration can be
recombined without crossing a bounded bootstrap product or recreating profile
dependencies in the emitter.
