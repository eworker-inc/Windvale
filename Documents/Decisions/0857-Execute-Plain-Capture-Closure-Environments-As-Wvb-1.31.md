# Decision 0857: execute plain-capture closure environments as WVB 1.31

## Status

Accepted on 2026-08-25.

## Context

Decision 0856 made exact noncapturing callable values executable through WVIR
1.17/1.18 and WVB 1.30. The source front end already validates explicit
`copy`, `move`, and borrow capture lists, but the portable pipeline had no
environment operation or lifetime model. Extending all source capture classes
at once would silently require reference counting for text and bytes,
destruction for owned values, and escape proofs for borrows.

Language 1.0 needs an executable environment substrate that can be verified
and bounded independently before source closure bodies are lowered into it. It
must preserve the exact WVB 1.30 callable descriptor and indirect-call
semantics, avoid host pointers, and reject every capture whose lifetime cannot
yet be represented exactly.

## Decision

1. WVIR 1.19 introduces operation `179 Closureˉcreate`; WVIR 1.20 is the same
   closure vocabulary with the generic-instance header inherited from WVIR
   1.18. The operation has 1 through 64 capture operands in declaration order,
   names one physical target function and one WVIC callable instance, and
   produces that exact callable shape.
2. The physical target's parameters are exactly the captured prefix followed
   by the public callable parameters. Its result exactly matches the callable
   descriptor. The target is synchronous, safe, nongeneric, effect-free,
   profile-matched, and entirely by value.
3. This first environment admits only copied inline scalar and enum values:
   `i32`, `bool`, `u8`, `u32`, `i64`, `u64`, `i8`, `i16`, `u16`, `rune`,
   `f32`, `f64`, and exact nominal enums. Text, bytes, callable values,
   aggregates, collections, resource owners, move captures, and borrows reject.
   Source-level `copy` remains broader; admission by the capture analyzer is
   not permission to publish a WVB 1.31 environment.
4. WVB 1.31 adds thirteen-byte opcode `D5 closure.create`, followed by the
   target function index, kind-`8` callable Types index, and capture count.
   `D5` consumes captures in declaration order and produces the existing exact
   shape-`35` callable. Kind `8`, shape `35`, `D3`, and `D4` retain their WVB
   1.30 encodings and are inherited by WVB 1.31.
5. The compiler-aligned verifier reconstructs the physical target signature,
   proves every captured stack value against the matching prefix parameter,
   rejects reference-backed and affine shapes, and requires every WVB 1.31
   module to contain at least one `D5`. At most 65,536 `D5` instructions occur
   in one module.
6. The scalar runner snapshots captures into a representation-private immutable
   arena. A callable cell carries a tagged arena offset and the callable type;
   `D4` validates the stored target/type/count, installs captured parameters,
   then installs public arguments and enters the ordinary bounded call frame.
   Direct WVB 1.30 function references keep their existing representation.
7. One execution creates at most 1,024 environments and retains at most 536,576
   bytes (524 KiB) in the environment arena, which is discarded as one unit at
   teardown. It performs no tracing, retain/release, or destructor work because
   every admitted capture is an inline value.
8. Source closure-expression lowering, synthetic target publication, captured
   move invalidation, borrow escape/lifetime proof, effectful callable values,
   native callable ABI lowering, browser execution, and OS execution remain
   connected Slice 6 work. This decision does not claim that a source closure
   expression yet emits `Closureˉcreate`.
9. The accepted source identity advances through
   `Windvale-Language-1.0-Source-Amendment-0857-Candidate.txt`. The only changed
   frozen input is the migration document that records this implementation
   checkpoint; no frozen grammar or source semantic rule changes. The 0833
   manifest remains immutable provenance and the accepted set remains 251
   inputs.

## Consequences

Windvale now has one portable, verified, executable closure-environment
substrate without weakening the exact structural callable identity. The same
typed indirect call executes direct references and plain-capture closures, and
the runtime remains independent of host addresses and ambient authority.

The narrow capture class is deliberate. Text and bytes cannot enter an
environment until retain/release and escape behavior is specified; owned and
borrowed values cannot enter until their invalidation and lifetime proofs are
connected. Those features must extend the contract explicitly rather than
reinterpret WVB 1.31.

The runner initially crossed the native lowerer's unchanged 64 MiB plan limit.
Consolidating repeated upper-version checks at the already-validated module
boundary reduced the runner from 366,728 to 361,080 WVB bytes and restored
packaging without raising a limit or weakening validation. Constructing the
small bounded environment record before one arena append also avoids copying
the complete retained arena once per header field. With 524 bytes as the
largest record and 1,024 creations, cumulative bytes copied by arena appends
are bounded to 274,995,200.

## Evidence

The deterministic closure-environment oracle is a 325-byte WVB 1.31 module at
SHA-256
`397f716af132192697c77d9f4f03e72c937e188aca78cf0474c9faaa2234e0e2`.
It captures `i32` value `40`, supplies public argument `2`, returns `42`, and
reports 11 guest instructions. The compiler-aligned verifier accepts the exact
module and rejects version downgrade, target-signature mismatch, callable-type
mismatch, zero captures, 65 captures, capture-shape mismatch, a
reference-backed capture, indirect-call type mismatch, and replacement of the
callable descriptor kind.

The focused Windows owner result is recorded in the Language 1.0 migration
evidence. Independent Linux reproduction, the repository-wide Qualification
gate, source closure lowering, direct-native/browser/OS execution, and promoted
runner repinning remain separate claims.

The 3,730-byte source-amendment manifest has SHA-256
`6d00f33f87dc62f5df55dc5dd5a882c3d4a7984a31bf376d85076ff0fe578e48`.
Its 251 inputs total 1,752,500 bytes and have aggregate entry-stream SHA-256
`57c37c700e311e7bfb8bd384f3f3fc4c88ff7c78ef23187b245ae739b24d13ad`.

## Reconsideration triggers

Reconsider this decision if source closure lowering cannot preserve the exact
captured-prefix signature, if copyable reference-backed values need a different
environment ownership model, if arena retention violates a named workload
bound, or if native ABI lowering makes an observable source distinction. Any
replacement must retain deterministic bytes, exact typing, explicit effects
and capabilities, bounded verification, and failure-closed lifetime behavior.
