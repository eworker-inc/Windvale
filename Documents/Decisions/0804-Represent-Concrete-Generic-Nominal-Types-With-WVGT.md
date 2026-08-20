# Decision 0804: Represent concrete generic nominal types with WVGT

- Status: Accepted
- Date: 2026-08-20

## Context

Language 1.0 permits generic records and variants, nested generic arguments,
compile-time constant parameters, and parameters that are not mentioned by a
field. The compiler already monomorphizes generic functions through WVGS/WVGC,
but its nominal type shapes still have only two special packed encodings for
Foundation `Option` and `Result`.

That packing cannot represent arbitrary declarations or nesting without
collisions, hidden limits, or loss of ordered generic arguments. Reusing the
template's ordinary nominal index would also make `Box<i32>` and `Box<u32>` the
same type. Adding runtime-erased generic values would conflict with the frozen
monomorphic WIR/WVB direction.

## Decision

1. Add `WVGT 1.0`, a separate bounded catalog for concrete generic record and
   variant identities. Keep function-code specializations in WVGC; the two
   catalogs have different consumers and growth accounting.
2. Give at most 256 catalog instances private compiler shapes
   `0x80000000..0x800000ff`. These shapes never enter WVB.
3. Define identity as canonical WVSD declaration, record/variant kind, and the
   complete ordered type/constant argument list. Retain phantom arguments as
   identity. Origin, depth, and estimated bytes are not identity.
4. Require nested generic arguments to reference an earlier WVGT instance.
   Recompute depth as one plus the maximum nested dependency and reject depth
   above 32, forward references, and every other value at or above
   `0x80000000`.
5. Retain the existing 32-parameter, 256-instance, 1 MiB evidence, and 16 MiB
   aggregate-growth bounds. Reuse an equal instance before testing new growth.
6. Validate exact format length, aggregates, constants, dependency order,
   depths, and duplicate identities before accepting or extending evidence.
7. Keep this checkpoint at the representation boundary. Source binding, field
   substitution, typed WIR carriage, reachable WVB type materialization, and
   eventual migration of the Foundation special cases are subsequent connected
   checkpoints and must not be described as implemented here.

## Evidence

The executable self-test admits record and variant identities, exact reuse,
type-plus-constant and nested arguments, a 32-level chain, and all 256 instance
slots. It rejects malformed and duplicate evidence, invalid declaration kinds,
forward references, depth 33, excessive growth, and instance 257.

The fixture is a 65,457-byte WVB with SHA-256
`1387baaf0d9da4deed9ac5a7d37530f47c086c178461576e29f66168240e7d8b`.
Its 681,472-byte hosted Windows executable has SHA-256
`b6bf5abea06bf9ab2d6fc081742dc4c6812d0a3b80d149cb5bf733443ad7c924`
and exits with `42`. The maintained Language 1 owner runs the same project on
both hosts; final paired-host and broad integration evidence remains deferred
to the seven-slice integration gate.

## Consequences

The compiler now has one collision-free, bounded identity for arbitrary
concrete generic nominal types and nested instances. That identity can be
connected to source phases without widening the public WVB type model or
creating a second compiler/runtime generic mechanism.

General generic record and variant source still does not compile merely because
the catalog exists. The next checkpoint must connect declaration parameters and
full-arity type uses to WVGT, then substitute fields and materialize concrete
WVB types. `Option` and `Result` retain their current proven encodings until
that path can replace them without changing semantics.

## Reconsideration triggers

Reconsider the 256-instance private-shape window only if representative source
workloads reach it under the unchanged overall specialization bounds. Replace
linear lookup only if measured catalog work dominates compilation. Version the
format rather than weakening dependency ordering if a future package-interface
contract needs cross-catalog identities or independently shipped templates.
