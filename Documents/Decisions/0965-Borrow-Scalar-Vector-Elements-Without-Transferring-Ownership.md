# Decision 0965: borrow scalar Vector elements without transferring ownership

## Status

Candidate implementation with selected source-to-execution verification on
Windows and Debian; see the
[exact checkpoint evidence](../Evidence/2026-09-22-Scalar-Vector-Indexed-Borrow.json).
This implements a bounded part of the already accepted `Vectorˉborrowˉat` API,
not its full generic element contract or Libraries 1.0 completion.

## Context

Before this candidate, a Vector helper could observe length but could not
inspect an element through the accepted immutable borrowing API. Sequence
indexing is not a substitute:
its selected Copy-element path returns an ordinary value. An indexed Vector
view must preserve its original owner, including a Vector projected from an
Option or Result, until the view is no longer usable.

Current collection storage supports resource-free scalar elements. Record
elements also need collection-child tracing and exact ownership work; widening
a scalar type gate would leave retained aggregate children unprotected. The
typed package-lock directory remains a subsequent maintained consumer.

## Decision

1. Resolve only the canonical `Foundationˉcollections.Vectorˉborrowˉat` function
   to WVIR operation `192`: exact Vector source slot in `Target`, exact Vector
   shape in `Auxiliary`, one `u64` index operand, and the exact element result
   with immutable-borrow provenance. Select WVIR 1.35, or 1.36 with generic
   specialization, and reject the operation in earlier minors.
2. Encode candidate WVB 1.41 instruction `E3 vector.borrow_at`, followed by a
   little-endian `u32` owner slot and `u32` Vector type index. Consume one `u64`
   index and produce the existing recursive shape-`37` element view. An E3 is
   required in minor 41; changing a header alone does not establish the feature.
3. Admit available owned Vector slots, immutable/exclusive Vector parameters,
   and exact projected borrowed Vector locals. An immutable reborrow does not
   grant mutation or transfer ownership. Preserve the original Option/Result
   loan for projected owners. Copy-scalar read-through remains explicit and
   does not turn the intrinsic's signature into a value-returning API.
4. Check the logical index before reading backing storage. Out-of-range access
   traps with no element publication or mutation. The scalar read requires no
   collection allocation, lease, or independent release obligation.
5. Reject owner mutation, consumption, replacement, and stale view use in
   complete bytecode verification, not only source analysis. Until exact
   ordinary-Vector argument provenance is available, a direct call with any
   exclusive Vector parameter conservatively invalidates all live loans;
   unrelated views therefore cannot be used across that call or passed as
   borrowed arguments to that same call.
   Materialize proven Copy results immediately unless an immutable call needs
   the actual loan. A bounded emitter preflight rejects retained Foundation
   slots, cross-block loan temporaries, and stale same-block loan use in
   functions containing exclusive Vector calls, before publishing bytecode.
6. Keep the source owner's conservative function-exit freeze and existing
   bounded provenance, stack, slot, instruction, descriptor, and call-depth
   limits. Do not add a parallel verifier or execution path.
7. Keep direct local length observation after indexing closed: the older local
   length sequence takes/restores the owner. Immutable helper length observation
   remains available through the parameter instruction. Conservatively reject
   same-owner value loads or mutation while evaluating the index, including
   reversed named arguments; general argument-effect alias analysis is separate.

## Selected evidence and remaining boundaries

The existing memory-budget split-execution owner's supplied-product
`--vector-parameter-reads` selection is the current evidence: eight positive
scenarios, fourteen malformed-bytecode cases, ten source rejections, and three
bounds traps pass on Windows and Debian. Deterministic bytecode and guest
instruction counts agree across hosts. Selected execution covers `i32`, exact
full-width `u64`, and one `u8`-backed enum with nominal-name checking, not every
scalar kind or enum backing family. Coverage includes local and parameter
reads, immutable helper forwarding, projected Option and both Result payloads,
repeated calls, and nonzero indexing. The retained Copy/exclusive-call exports
are complete-verified structural controls, not runtime-executed scenarios.

The source-built Analyzer is freshly complete-verified; four unchanged tool
products and component execution retain exact earlier passing evidence. The
earlier full checkpoint remains failed. Owned-payload and runtime regression
owners pass freshly. Linux containers use Windows-produced native images;
this is paired execution, not independent reconstruction. Automatic CI
execution of the supplied-product indexed selection remains an open gate.

Record/owned elements, mutable indexing, replacement/removal, general borrowed
returns, native lowering, browser execution, installed promotion, independent
compiler reconstruction, and complete qualification remain separate gates.
