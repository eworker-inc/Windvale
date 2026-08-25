# Decision 0856: execute noncapturing callable values as WVB 1.30

## Status

Accepted on 2026-08-25.

## Context

Decisions 0852 through 0855 established exact structural function types,
explicit capture checking, bounded transitive effect analysis, and the WVCF
catalog of concrete callable functions. That evidence deliberately stopped
before portable function values: WVIR could not construct or invoke a callable,
WVB had no structural callable descriptor, and the verifier and runtime had no
representation-hidden indirect-call contract.

Language 1.0 needs one executable checkpoint before closure environments are
added. It must not use a host address as portable identity, infer a compatible
overload, hide an effect or capture, or create a second compiler/runtime path.
It must also remain within the existing compiler, bytecode, verifier, and scalar
runtime bounds.

## Decision

1. The first executable callable profile contains only named, non-generic,
   noncapturing functions with explicit empty `effects()`, no `async` or
   `unsafe` flag, by-value parameters, and a value result other than `unit` or
   `never`. Every omitted feature rejects rather than receiving an implicit
   representation.
2. WVIR 1.17 introduces operation `177 Functionˉreference`; WVIR 1.18 adds
   operation `178 Callˉindirect`. A private WVIC 1.0 catalog interns complete
   callable identities by module profile, result shape, and ordered parameter
   shapes. `Source-Wir-Consumer-Core.wv` owns bounded immutable WVIR consumer
   helpers; it is a cohesive extraction, not an alternate IR.
3. WVB 1.30 adds terminal Types kind `8`, value shape `35`, opcode `D3
   function.reference`, and opcode `D4 call.indirect`. The kind-`8` descriptor
   contains the profile, result shape, parameter count, and ordered parameter
   shapes. Shape `35` names that exact descriptor. Neither carries source names,
   host pointers, closure storage, or a native ABI layout.
4. The producer admits at most 256 callable descriptors with at most 64
   parameters each and at most 1,024 total Types entries. It emits terminal
   callable descriptors in deterministic WVIC order and maps every private
   shape to the resulting exact Types index. WVB 1.30 must contain at least one
   `D3` or `D4`; each opcode is limited to 65,536 occurrences per module.
5. The compiler-aligned verifier requires an exact descriptor/target match for
   `D3`. For `D4`, it requires the callable value below its exact arguments and
   consumes those arguments in reverse declaration order before the callable.
   Existing stack, reachability, branch-join, call, ownership, and resource
   rules continue to apply.
6. The source-built scalar runner represents a callable in one existing
   eight-byte value cell. Its current implementation stores the function index
   in the low `u32` and the callable type index plus one in the high `u32`.
   Execution rechecks both identities and enters the ordinary bounded frame
   path. This cell layout is private runtime state, not portable semantics or a
   native calling convention.
7. WVB 1.30 inherits the earlier vocabulary and ownership proofs. Borrowed
   aggregate views remain valid through 1.30; the WVB 1.29 source-file shape and
   operation remain valid under their exact confinement when present. Capturing
   closures, nonempty callable effects, flag-bearing or borrowed signatures,
   callable equality, environment lifetime and escape analysis, native ABI
   lowering, browser execution, and OS execution remain later checkpoints.
8. Callable-type catalog coverage stays in its own compiler-scale project so
   that the existing linker symbol ceiling does not force an unrelated
   production limit increase. The separate project is verification structure,
   not a second compiler or semantic path.

## Consequences

Language 1.0 now has a portable, verified, executable function-value core.
Programmers may store a proven named function in a local and invoke it through
its exact structural type. The bytecode remains deterministic and independent
of host addresses, and the runtime gains no ambient authority.

This checkpoint is intentionally narrower than general closures. A source
program that captures state, declares nonempty effects, uses flags or borrowed
parameters, returns `unit` or `never`, or needs a native callable ABI still
rejects. Later work must extend the descriptor and lifetime proof explicitly;
it cannot reinterpret existing WVB 1.30 bytes.

## Evidence

`Tests/Fixtures/Language-1.0/Callable-Indirect-Execution.wv` emits a
deterministic 400-byte WVB 1.30 module at SHA-256
`30eab353a6187ead317438d2c63a2bd6aa53d9ec682bc5c59d9d3b82530edfaf`.
The source-built scalar runner returns `42` in 24 guest instructions. The
compiler-aligned verifier accepts that exact module and rejects five mutations:
version downgrade, target-signature mismatch, reference-type mismatch,
invocation-type mismatch, and replacement of the callable descriptor kind.

The focused Windows owner reports:

```text
native language 1 callable semantics status=Passed cases=38 result=42 modules=7 wvb-bytes=4168987 evidence-sha256=d3f04b28ea1c76150cf8a1219fa25c7587969ddf4072af1cf2baeaf8828c5fe0
```

This is focused development evidence. Independent Linux reproduction, the
repository-wide Qualification gate, direct-native/browser/OS execution, and
promoted runner repinning remain separate claims.

## Reconsideration triggers

Reconsider this decision if capture environments cannot extend the structural
identity without ambiguity, if an exact effect representation requires a new
descriptor version, if native ABI lowering needs observable source semantics,
or if representative workloads show that descriptor lookup or indirect-frame
entry violates a named performance bound. Any replacement must preserve exact
typing, capability/effect visibility, deterministic bytes, and bounded
verification.
