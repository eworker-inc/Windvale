# Decision 0818: Execute generic variant construction and matching

- Status: Accepted with current-Windows development evidence; independent Linux qualification pending
- Date: 2026-08-21
- Advances: [Language 1.0](../../Specifications/Windvale-Language-1.0.md), [generic type evidence](../../Specifications/Compiler-Source-Generic-Types.md), [source bindings](../../Specifications/Compiler-Source-Bindings.md), [source WIR](../../Specifications/Compiler-Source-Wir.md), [source WVB](../../Specifications/Compiler-Source-Wvb.md), and [Decision 0817](0817-Admit-Generic-Nominal-Layout-Dependencies.md)

## Context

The compiler could admit, substitute, materialize, and serialize a generic
variant instance, but ordinary source bodies could not construct or match one.
The remaining path had to preserve exact field evaluation and binding,
monomorphic WVB, immutable phase evidence, deterministic diagnostics, and the
existing type and product-size bounds.

Decision 0815 already amended `Nominalˉconstruction` to admit a complete applied
nominal target followed by braces. Its grammar covers the variant spelling
`Outcome.Value<Point> { ... }`; no further grammar amendment is required.
Repeating `<Point>` in a match arm would duplicate selector evidence and create
an unnecessary second place for type arguments to disagree.

## Decision

1. A generic variant case construction places the owning variant's complete
   type arguments after the selected case:
   `Outcome.Value<Point> { Item: Value, Attempts: Count }`.
2. Bind that target through the existing recursive WVGT admission path and
   independently validate the complete substituted variant layout before
   accepting any field value.
3. Preserve left-to-right evaluation of supplied expressions, then order only
   their temporary identities by the case declaration. Retain the existing
   exact unknown, duplicate, missing, and type-mismatch failures.
4. Reuse `Variantˉcreate = 65` with the private WVGT instance shape as result and
   target. The auxiliary field remains the declaration-order case index.
5. A match arm names `case Outcome.Value { ... }` without type arguments. The
   selector's exact `Outcome<Point>` shape determines the specialization.
   Require the named template and case to belong to that exact instance.
6. Reuse `Variantˉcase = 66` and `Variantˉfield = 164` with the same private
   target. Every non-discarded binding receives its exact substituted field
   shape; `_` remains storage-free.
7. Make the independent WIR validator reconstruct the same WVGT layout and
   prove instance, declaration kind, case, field, operand, arity, packed
   identity, and result shape. A failure publishes no partial analysis family.
8. Source WVB translates the private target through the existing materialization
   plan and emits only ordinary variant metadata and operations. No template,
   argument vector, private shape, or runtime-generic dispatch survives.
9. Keep all established bounds: 256 admitted generic instances, depth 32, 256
   cases, 64 fields per case, 1,024 WVB Types entries, and the existing compiler
   product payload policies. Optimize shared layout and construction paths
   rather than widening those limits without evidence.
10. Continue evolving the active Windvale-written compiler toward Compiler 1.0.
    The immutable Stage 0 release remains recovery provenance, not a parallel
    semantic implementation.

## Evidence

The split route builds a 1,062,350-byte analyzer WVB with 522 functions and
869,515 code bytes at SHA-256
`518f8a08a67a83f3338ff9bd5994afdc616c7750e6ab77e077d3f8bd32f16666`.
Its eight-fragment Windows application is 33,452,032 bytes at SHA-256
`d73c61b713067180029edf0112e6beddf02be35ffce58a03c9dce7da583ebbd5`,
leaving 102,400 bytes under the unchanged 32 MiB payload policy. The connected
990,701-byte emitter WVB has 542 functions and 821,031 code bytes at SHA-256
`05d130390f82b07a5d760f503fcc97b5157c0d5ed343d57bc8e04eb4bf4c1665`;
its Windows application is 21,803,520 bytes at SHA-256
`31e0f1828f14cf4143ef9bafa68cdedb37c4b164d5793321b0976aad7afe62c4`.

`Generic-Nominal-Variant.wv` publishes exact 771-byte WVSS, 104-byte WVCA,
316-byte WVLB 1.3, and 1,828-byte WVIR 1.3 products. The WVGT catalog contains
one `Outcome<Point>` instance. WIR uses private operation `65` for construction,
`66` for two reachable case tests, and `164` for three substituted field reads.
The resulting 947-byte WVB 1.16 has SHA-256
`5dda1cc2c65bd8af7d9a1f8b52f83002c083014303c8604ad667d217880971f7`.
It contains ordinary `Point` and private-named `__WvY0000` Types entries, no
source template, and executes with result `42`. The independent inspector has
94 structural assertions. Three companion fixtures reject construction type
mismatch, a missing construction field, and pattern type mismatch with one exact
diagnostic and no partial downstream artifacts.

The compiler-scale generic-WIR fixture is deterministic at 1,217,428 bytes and
SHA-256
`e7e991248d658e7b6551137f75f3782c4b977c8d9be35c158a26d5c815035b2a`.
The focused Language owner registers 349 cases, including 97 for this
checkpoint, and passes them in 582,970 milliseconds (583,670 milliseconds
including coordinator overhead). The 109-owner registry contains 5,099 cases at SHA-256
`1db0080019c37a83b25a025c4171fd902a5a28dba6012b14803f30ca5208ff83`.
The directly changed generic type-binding, layout, materialization, and WVLB
carrier owners also pass 18, 21, 30, and 20 cases respectively, each with result
`42`.
These are current-Windows development results, not paired-host conformance or
release qualification.

## Consequences

General generic variants now cross construction, parameter, local, exhaustive
match, named destructuring, and return boundaries using the same concrete
runtime representation as an ordinary variant. Library authors can define
generic result-like domains without adding runtime generics or compiler-special
families.

The frozen grammar remains stable. Pattern type arguments are intentionally
absent, so the exact selector is the single specialization authority. Deeper
generic-function formal patterns, broader constant expressions, collection
implementation, and remaining Foundation migration stay separate checkpoints.

## Reconsideration triggers

Revisit this decision if patterns gain an explicit independent type-test
contract, Language 1.0 adds inferred nominal construction arguments, templates
acquire runtime identity, WVB gains reified generics, or representative compiler
closures cannot remain inside their established bounds after measured
refactoring.
