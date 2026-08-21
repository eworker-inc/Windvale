# Decision 0807: Substitute generic nominal type layouts

- Status: Accepted
- Date: 2026-08-20

## Context

Decision 0806 made a concrete use such as `Choice<Box<i32>, text>` produce a
bounded, canonical WVGT identity. That identity still did not answer the next
compiler question: which concrete fields belong to `Box<i32>`, and which
payload shapes belong to each case of the concrete `Choice` instance?

Letting WIR or WVB emission reinterpret template source independently would
duplicate generic substitution and could bypass record-storage or variant-
payload restrictions after a parameter became concrete. Materializing every
possible layout eagerly would also retain evidence for unused instances and
inflate the compiler's bounded intermediate state.

## Decision

1. Add one generic nominal layout boundary over validated Source Set, Source
   Symbols, and WVGT evidence. A layout binds one WVGT instance back to its
   exact source declaration, parameter descriptor, module, and private shape.
2. Require the declaration kind, identity, origin, arity, parameter kinds, and
   parameter shapes to match the WVGT entry exactly before substitution.
3. Substitute a direct type parameter with its ordered WVGT type contribution.
   Bind an ordinary concrete field or payload type through the established
   source-symbol type binder. Nested template field syntax such as `Box<T>`
   remains a later recursive-substitution checkpoint.
4. Reapply concrete safety rules after substitution. A record field rejects a
   builder or capability shape. A variant payload additionally rejects another
   variant shape, including a private WVGT variant.
5. Publish bounded item and total-field counts plus lazy record-field,
   variant-case, and variant-field accessors. Each accessor validates the
   supplied layout and source evidence rather than trusting a caller-created
   record. Do not serialize a second potentially large layout catalog at this
   checkpoint.
6. Preserve expression `>>` tokenization while allowing adjacent generic type
   closers. A nested ordinary or generic type consumes one logical `>` and
   leaves the remaining closer for its enclosing type-use parser.
7. Defer WIR carriage, reachability ordering, WVB type materialization,
   Foundation migration, and nested template field substitution to subsequent
   connected checkpoints.
8. Give this boundary an independent 18-case owner. Keep storage, OS, complete
   Language 1, paired-host, and Qualification gates for the final integrated
   migration gate.

## Evidence

The focused fixture lays out `Box<i32>`,
`Packet<Box<i32>, text>`, and `Choice<Box<i32>, text>`. It checks record field
order, variant case and payload counts, substituted private and ordinary
shapes, missing-index behavior, source/catalog identity mismatch, a missing
declaration, malformed evidence, and caller-created layout tampering. It also
proves that `Slot<builder<i32, 8u32>>` is rejected only after substitution and
that `Choice<Choice<i32, text>, text>` parses adjacent closers but rejects its
nested variant payload.

After Decision 0808 adds compact sequential evidence, the fixture builds to a
688,672-byte WVB with SHA-256
`55fe9cf4744cfe26f42900c85ad8eed9f6e0940cd7d6b533b7a6a94295c042b1`.
Its 16,976,896-byte hosted Windows executable has SHA-256
`f28acda8fb1dc64da27e7e08d191ab637600e23c2e69505ee89aed40cc374f5c`,
returns `42`, and writes no output. The maintained owner reports visible build,
package, and execute phases and passes all 18 declared cases.

## Consequences

The compiler now has one executable source of truth for the concrete structure
of a direct-parameter generic record or variant instance. Invalid concrete
storage cannot hide behind a generic parameter, and a WVGT identity cannot be
combined with a different declaration or source set.

The lazy accessors bound retained evidence but may rescan an already-validated
layout. WIR integration should traverse one layout sequentially or materialize
one bounded compiler-owned representation when repeated random access becomes
measurable. This checkpoint does not claim that an application using a general
generic nominal reaches WVB.

## Reconsideration triggers

Extract a shared layout cursor when WIR materialization would otherwise repeat
the full source scan for every field. Extend substitution recursively when the
next fixture admits template field syntax such as `Node<Option<T>>`; keep the
same post-substitution storage and payload checks.
