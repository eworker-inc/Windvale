# Decision 0826: Execute scalar Vector and Sequence backings as WVB 1.19

## Status

Accepted on 2026-08-22.

## Context

Decision 0825 gave Language 1.0 Vector and Sequence exact WVB identities but
deliberately stopped before allocation and execution. The older Seed
`builder<T, N>` and `sequence<T, N>` opcodes encode maximum capacity in source
type identity, use `u32` indices, and have no executable implementation in the
current scalar runner. Reusing them would preserve the collection model that
Language 1.0 replaces.

The public Foundation contract is also wider than one backend instruction. A
fallible `Vectorˉconstructˉreserved` consumes an explicit `Memoryˉbudget` and
returns a typed `Result`; append reports a recoverable unchanged-on-rejection
failure. Freezing that complete source API before the bytecode has a proved
owned backing would conflate language behavior, allocation authority, and one
runtime representation.

## Decision

1. WVB 1.19 adds six low-level operations: `C7`
   `vector.create_reserved`, `C8` `vector.append_unchecked`, `C9`
   `vector.freeze`, `CA` `vector.length`, `CB` `sequence.length`, and `CC`
   `sequence.element`. Their immediate Types indices and operand/result shapes
   must match kind-5 Vector or kind-6 Sequence exactly.
2. The first executable subset admits only resource-free scalar element shapes:
   `i32`, `bool`, `u8`, `u32`, exact enums, `i64`, `u64`, `i8`, `i16`, `u16`,
   `rune`, `f32`, and `f64`. Element-owned descriptors, aggregates, and nested
   collections remain rejected until type-directed destruction and tracing are
   implemented.
3. The scalar runner represents a Vector or Sequence as one eight-byte heap
   descriptor. The backing contains a `u32` current length, a positive `u32`
   retained maximum, and that maximum's eight-byte cells. One allocation is
   bounded to 2,047 cells and 16 KiB.
4. Vector mutation updates its uniquely owned backing. Freeze consumes the
   mutable value and publishes an immutable Sequence over the same backing.
   Reserved construction creates non-serialized linear Vector evidence;
   append and Vector length preserve it, and freeze consumes it. Ordinary local
   loading does not recreate that evidence. Sequence is shared immutable.
   Descriptor loads retain; stores and function teardown release. Inactive
   allocation records and released first-fit spans are reusable.
5. Append is explicitly the post-check primitive. Capacity violation and
   Sequence index violation return terminal `WVR3008`; a high-level compiler
   must lower the Foundation recoverable checks before selecting it. This
   checkpoint does not implement or bypass `Memoryˉbudget`, construct a source
   `Result`, or grant allocation authority.
6. The runner project uses the current split analyzer/emitter pair in the
   Language 1.0 front door. Its ninth focused source module exceeds the retained
   pinned monolithic builder's usable source closure, while the already
   published current split compiler accepts and emits it deterministically.
   This is not a second compiler or a relaxation of verification.

## Consequences

- The compiler-aligned verifier publishes 232,414 WVB bytes at SHA-256
  `27941493d2c818d67da8cffbcb686de32517ac46a2a659b3a5e5884e2d59fb7e`.
  It checks all six opcode widths, type-index ranges, exact scalar elements,
  stack effects, freeze compatibility, local flow, and control boundaries.
- The scalar runner publishes 226,540 WVB bytes at SHA-256
  `a3b63a20d7a360889477346d970490c2f1139be8687add203955271844bc92f9`.
  Collection backing initialization doubles bounded zero-cell blocks instead
  of concatenating once per reserved cell.
- The deterministic runtime fixture is 971 bytes at SHA-256
  `14c8f442c499669139b5106d62bf4687450a6b4537b5e224f637fbecc4ada251`.
  It executes all six operations, repeats six 16-KiB allocation cycles, reuses
  released state, and returns `42`. Four semantic corruptions, one typed
  copied-Vector rejection, and two exact `WVR3008` executions bring the focused
  runtime evidence to nine cases.
- `Source-Wir-Core.wv` is unchanged. Runtime backing belongs to a focused
  interpreter collection module; future source lowering will reuse the
  existing WIR collection operations and a cohesive WVB-selection owner rather
  than add runtime mechanics to the already large source-WIR orchestrator.
- The verification registry retains 108 owners and advances to 5,164 declared
  cases at SHA-256
  `4505c493c6836e7df9f3dff14b25c513aaa54febc6ccb030972272057416cd94`.
- Source-level construction, recoverable append, `Memoryˉbudget` charging,
  borrowing, compiler selection of the new opcodes, non-scalar elements,
  native lowering, and WebAssembly qualification remain later checkpoints.

## Reconsideration triggers

Reconsider the scalar cell/backing representation if executable non-scalar
elements require a different ownership layout, if a shared native backend
cannot preserve the same move/freeze behavior, or if measured collection
workloads show that the 16-KiB scalar-runner allocation ceiling prevents useful
bounded evidence. Do not expose the descriptor, heap offset, refcount, or
current runner ceiling as portable source semantics.
