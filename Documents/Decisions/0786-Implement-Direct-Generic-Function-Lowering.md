# Decision 0786: Implement direct generic function lowering

- Status: Accepted
- Date: 2026-08-20

## Context

Decision 0783 admitted bounded generic declaration and explicit-call syntax.
Decision 0785 then fixed canonical WVGS solution evidence and WVGC
specialization identity, but deliberately left source symbols, bindings, WIR,
and WVB disconnected. The compiler therefore recognized generic syntax without
being able to execute even an identity function.

The complete resolution oracle is intentionally rich: it retains structured
diagnostics, constant identities, and independent evidence validation. Adding
that entire implementation to every compiler product exceeded the practical
native-image margin observed during the compiler-capacity work. The executable
path needs the same canonical identity and limits without duplicating unused
oracle machinery.

## Decision

1. Add the compact `Compilerˉsourceˉgenericˉlowering` producer to every source
   analysis and emission closure. It writes the same WVGS 1.0 and WVGC 1.0
   evidence identity as the canonical resolver and preserves its 32-parameter,
   256-instance, depth-32, 1 MiB evidence, and 16 MiB estimated-code bounds.
2. Keep `Compilerˉsourceˉgenericˉresolution` as the independent correctness
   oracle. Production does not import both implementations merely to obtain
   capabilities outside this checkpoint.
3. Admit direct type parameters on functions. A supported parameter may be the
   complete type of a value parameter, the complete result type, or both.
   Inferred and full-arity `::` explicit calls contribute to one declaration-
   ordered solution and reuse the same specialization identity.
4. Substitute the selected concrete shape while building function bindings,
   compiling the body, and checking call and return types. The independent WIR
   validator reconstructs the same solution from concrete parameter and result
   evidence before accepting the monomorphic directory.
5. Run one planning pass to discover specializations reachable from ordinary
   functions. If compiling a generic body discovers another generic instance,
   rerun the final pass to a fixed point, with an exact maximum of 32 passes.
   A source set without admitted generic instances retains the existing single
   WIR pass.
6. Keep WVGS/WVGC transient. Published WVIR and WVB contain only ordinary
   concrete signatures, calls, operations, and function bodies. The existing
   WVB emitter, verifier, and runtime remain unchanged.
7. Limit this first connected checkpoint to one distinct specialization per
   generic declaration. Repeated inferred or explicit uses of the same concrete
   arguments reuse it; a second distinct identity reports
   `Genericˉspecialization` before partial publication.
8. Defer constant generics, nested generic type expressions, generic records
   and variants, phantom parameters, and template-only declarations that have
   no current-compilation instance. These remain Language 1.0 implementation
   work rather than compatibility behavior.

## Evidence

The focused hosted publisher checks a conflicting-inference rejection, a
distinct-specialization rejection, an explicit result-only specialization, and
an identity function called once by inference and once explicitly. It publishes
the successful program as independently consumable WVSS, WVCA, WVLB, and WVIR.

A same-length monomorphic source oracle preserves every relevant source offset.
The generic and oracle WVCA, WVLB, and WVIR values compare byte-for-byte. The
existing qualified emitter consumes the oracle-equivalent evidence, and the
pinned scalar runner returns `42` in 26 instructions. This proves that generic
syntax disappears before WVB and runtime execution.

The publisher is a 1,048,153-byte WVB with SHA-256
`a2befed440f070ed934dd3ca783129cad30016ec2b46007548507f415cb3974a`.
The equal generic/oracle hashes are
`7c30318a94a9c16965347d17da358b309aefaa01519bafed80e48eb52b4a294a`
for WVCA,
`bda5d2ec661429a8649b3a23c905d1986fa5ad081b8c891c0283f5c534582a37`
for WVLB, and
`dc3810d6b498fc2ff6d5676a584331df47292105daa13f5926dff309b1322be5`
for WVIR. Emission produces a 297-byte WVB with SHA-256
`cb7f970929bcdafa15c5f13b817f013ba30c033933d2988283b2e5c41ea316b3`.

The focused compiler product requires the segmented native packager. The
monolithic package route rejects it at the unchanged output limit; no native
object, fragment, compiler, or runtime limit was raised. Storage, OS,
paired-host, and complete Qualification gates are deferred to the final
seven-slice integration gate because this checkpoint changes only compiler
source analysis and monomorphic lowering.

## Consequences

Windvale can now compile and execute the first ordinary generic functions
without a second backend or runtime generic service. Explicit and inferred
spelling are provably the same semantic instance, and failures remain bounded
before WIR/WVB publication.

This is the first connected portion of migration Slice 4, not completion of
general generics or collections. Source libraries cannot yet rely on unused
generic templates or multiple concrete instances of one declaration.

## Reconsideration triggers

Replace the one-instance checkpoint when the monomorphic function directory can
represent multiple concrete bodies per source declaration with deterministic
symbol identity. Extend the compact producer only when the next source feature
needs a capability already proven by the canonical oracle. Reconsider the
planning strategy if representative nested generic workloads show that bounded
whole-source passes dominate compilation time or retained memory.
