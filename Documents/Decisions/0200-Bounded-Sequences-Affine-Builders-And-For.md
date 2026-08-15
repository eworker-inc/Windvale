# Decision 0200: Bounded sequences, affine builders, and `for`

- Date: 2026-08-03
- Status: Accepted, implemented, and included in the cross-host-qualified WVB 1.11 semantic-freeze baseline under Decision 0213
- Refines: [Decision 0137](0137-Bounded-Owned-Values-Before-Dynamic-Collections.md), [Decision 0179](0179-Language-Application-And-Capability-Metadata-Direction.md), and [Decision 0184](0184-Language-Syntax-And-Operator-Evolution.md)
- Retains: immutable published values, checked `u32` sizes, deterministic iteration, explicit mutation, canonical WVB verification, and no backward-compatibility promise during active development

## Context

The accepted language sequence requires one bounded typed sequence, one uniquely owned builder, consuming publication, and bounded `for`. The architecture deliberately left exact constructor and mutation spelling to this focused decision. The first contract must be useful without selecting maps, sets, lazy iteration, general generics, tracing collection, or mutable aliasing.

## Decision

### Source surface

The first collection types are `sequence<T, N>` and `builder<T, N>`, where `N` is an unsigned decimal source constant from 1 through 4095. `T` is one scalar, `text`, `bytes`, record, enum, or variant value shape. A collection cannot directly contain another sequence or builder in this version.

An empty builder is constructed with its exact type spelling. `push` is a statement that consumes the current builder value and replaces the same mutable local only after the append succeeds. `freeze` is an expression that consumes the builder and returns the corresponding immutable sequence.

```text
var Pending: builder<Request, 256> = builder<Request, 256>();
push Pending, First;
push Pending, Second;

let Published: sequence<Request, 256> = freeze Pending;

for Request in Published {
    Submit(Request);
}
```

The builder target of `push` and `freeze` is one unqualified mutable local. Builders cannot be parameters, results, record fields, variant payloads, constants, data, call arguments, or assigned to a different local. A consumed builder cannot be read, pushed, or frozen again. Source analysis rejects a later use conservatively in deterministic source order; the runtime also invalidates consumed instances so malformed verified input cannot recover mutable aliasing.

`for Name in Sequence` introduces one immutable element binding scoped to its block. It evaluates the sequence expression once, iterates indices from zero through length minus one in order, and lowers to checked sequence length/index operations plus ordinary control flow. The loop variable cannot be assigned. `break` and `continue` target the nearest `for` or `while`.

### Bounds, failure, and ownership

- Builder construction publishes one empty uniquely owned builder with declared maximum `N`.
- Each successful `push` creates the next unique builder state and invalidates the input state. Element evaluation happens before the consuming append.
- Pushing when length equals `N` traps as `WVR3030` before a replacement builder becomes visible.
- Reusing a consumed builder traps as `WVR3031` if malformed bytecode reaches the runtime.
- `freeze` invalidates the builder and publishes one immutable sequence whose length and declaration maximum are retained.
- Sequence indexing uses the existing `WVR3008` checked-index failure.
- Empty sequences are valid. Sequence defaults are empty; builder defaults are unusable and source construction is required before builder operations.
- The Stage 0 runtime may use managed immutable storage internally, but reference identity, host garbage collection, and mutable aliasing are not observable semantics.

### WIR and WVB

WVIR packs collection shapes into one `u32`: the high family identifies sequence or builder, twelve bits retain maximum `N`, and sixteen bits retain one non-collection element descriptor. This is compiler evidence, not source ABI.

WVB 1.10 adds value-shape kinds `12` (`sequence`) and `13` (`builder`). Each serializes the non-collection element shape followed by `u32 Maximum`. It adds five verified operations/opcodes: builder create, builder push, builder freeze, sequence length, and sequence element. Builder create carries the element shape and maximum; the remaining operations derive and verify their exact types from the operand stack. Every shape, maximum, operand, local, call boundary, and control-flow join is independently verified before execution.

WVB readers need not accept the superseded 1.9-only surface as a compatibility obligation. Repository fixtures and generated evidence advance together under the active-development policy.

The Stage 0 and Windvale-written compilers, WVIR validation, canonical WVB writer/reader/verifier/inspector, and reference runtime implement this contract. Both compilers produce the exact same 809-byte collection oracle with SHA-256 `5cd5e686cd8bbbe6d8bc793dcf7c270acd643301e16cdcf53a65a317ef08a8ee`; the verifier accepts it and the runtime returns `16`. The final change-aware gate passed all 92 affected Seed tests. The later complete WVB 1.11 semantic-freeze baseline passed all 97 Seed tests on Windows and digest-pinned Debian 12 at exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b` under [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md); target-specific execution profiles remain separately bounded.

## Consequences

Windvale gains useful bounded dynamic aggregation, explicit publication, and deterministic collection loops without general mutable collections or iterator machinery. The affine runtime transition keeps the semantic contract meaningful even before permanent native storage and reclamation are selected.

This first contract is deliberately strict. It does not include maps, sets, sorting, slicing, collection equality, collection literals, nested collections, builder borrowing, builder escape, general iterator protocols, generators, implicit growth, or automatic capacity selection.

## Reconsider when

- a measured consumer requires nested immutable sequences;
- conditional builder construction or transfer cannot be expressed clearly under conservative affine analysis;
- permanent native storage evidence selects a smaller equivalent representation;
- a real map or set consumer supplies exact ordering, hashing, collision, and capacity rules; or
- the statement-form `push` materially harms ordinary library design after methods or traits exist.
