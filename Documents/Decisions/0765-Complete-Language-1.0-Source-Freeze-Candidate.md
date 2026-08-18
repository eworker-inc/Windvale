# Decision 0765: Complete the Language 1.0 source-freeze candidate

- Status: Accepted
- Date: 2026-08-17

## Context

All eleven mandatory Language 1.0 paper workloads have completed owner review,
and the project owner accepted every resulting correctness and usability
finding. The complete-suite reconciliation then compared the paper source with
the semantic specification, grammar, Foundation calls, migration ownership, and
freeze gates.

That audit found no reason to add classes, implicit conversions, exceptions,
garbage-collected object semantics, detached tasks, general reflection, broader
identifier syntax, or a second compiler. It did find several exactness gaps that
would make a source freeze unreproducible:

- Foundation signatures were distributed among prose and workload-selected
  examples without one complete per-module identity;
- the grammar required a machine-readable projection but did not include one;
- the package-parser source used `while let`, while the grammar recorded only a
  Boolean `while`;
- separate simple-binding and destructuring productions overlapped;
- the lexical productions did not encode the prose bounds of one-through-six
  Unicode escape digits and zero-through-eight raw-literal hash marks;
- no paper source exercised the initially proposed interpolated-text expression,
  whose spelling exposed neither a destination owner nor an allocation budget;
- two paper bundles retained earlier Foundation spellings, the package bundle
  used `Some`/`None` rather than the accepted `Present`/`Absent`, and the
  candidate registry named the first `Sequenceˉlength` and `Sequenceˉat`
  parameter differently from the paper calls; and
- freeze checklists mixed source-design acceptance with future executable
  compiler, editor, cross-host, and performance qualification.

## Decision

Accept the complete-suite reconciliation findings and prepare one exact
Language 1.0 source-freeze candidate as follows.

1. Publish `Specifications/Windvale-Language-1.0.ebnf` as the canonical
   machine-readable projection of the grammar companion. External scanner
   tokens remain governed by the exact UTF-8, comment, literal, and raw-delimiter
   contracts in the human-readable grammar.
2. Publish the complete Foundation major-1 declaration surface in
   `Specifications/Windvale-Language-1.0-Foundation-Registry.md`. Each of the
   eleven required modules has one independently reproducible candidate
   signature-set SHA-256.
3. Admit `while let Pattern = Expression` as the sole conditional-pattern loop.
   It evaluates the expression once per attempted iteration, enters only on a
   match, and applies ordinary match ownership.
4. Replace overlapping simple/destructuring statement productions with one
   `let`/`var` binding production over `Pattern`; an optional type annotates the
   complete right-hand value.
5. Encode one-through-six non-separated hexadecimal digits for Unicode escapes
   and zero-through-eight hash marks for raw literal delimiters directly in both
   grammar representations.
6. Keep Foundation major 1 small and exact. Close Option/Result observations and
   pure maps, finite owned-item iterators, reserved vectors/builders, deterministic
   maps/sets/arenas, strict numeric generated families, explicit invariant formatting,
   structured tasks, and the audited unsafe scratch boundary. Do not imply
   effect-polymorphic maps, borrowing iterators, mutable builder aliases, lossy
   integer conversions, bulk partial-progress calls, or standalone interpolation
   syntax that the workloads did not justify. A later interpolation proposal
   must expose its bounded destination, memory owner, and failure path.
7. Correct the retained-GUI byte append, package vector constructor, package
   Option spellings, and Sequence named-parameter identity to their canonical
   Foundation contracts without changing expected output bytes or behavior.
8. Treat source freeze as acceptance of exact language contracts and candidate
   identities. Paper cases become executable conformance fixtures during
   migration. The source-freeze decision must not claim current compiler,
   Foundation, editor, formatter, cross-host, or performance conformance.

## Non-decision

This decision does not freeze Language 1.0, authorize implementation, rename the
current Seed contract, or claim that any edition-1 paper source currently
compiles. The explicit owner source-freeze decision remains separate and must
name the final candidate manifest identity.

Production stream, filesystem, database, accelerator, and other capability
signature sets remain separately versioned library/provider contracts. Their
paper interfaces are sufficient source-language fixtures and do not become
ambient language APIs.

## Consequences

The source suite now has one complete machine grammar, one complete Foundation
signature registry, coherent paper calls, a design-versus-implementation gate,
and reproducible candidate identities. The project owner can review one concise
freeze packet rather than infer the final contract from eleven bundles and
scattered examples.

Any change after this candidate is published must update the owning normative
document, dependent paper source, affected module identity, and candidate
manifest together. Implementation feedback may still reopen an accepted rule
through a named decision; it may not create an undocumented compatibility alias
or parallel compiler path.

## Reconsideration triggers

Reconsider this decision if the machine grammar cannot represent a documented
production, a registry declaration cannot be typed under the grammar and
semantic rules, a paper workload requires an omitted general source construct,
or implementation proves that an accepted finite bound cannot be met on a
permanent target without changing observable semantics.
