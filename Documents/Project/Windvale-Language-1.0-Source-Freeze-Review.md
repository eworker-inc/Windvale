# Windvale Language 1.0 source-freeze review

## Owner decision status

The Language 1.0 design is ready for the explicit project-owner source-freeze
decision. It is not frozen or implemented yet.

The owner has accepted every workload and complete-suite reconciliation finding.
[Decision 0765](../Decisions/0765-Complete-Language-1.0-Source-Freeze-Candidate.md)
records those resolutions without making the separate freeze decision. The
candidate identity is
[`Windvale-Language-1.0-Source-Freeze-Candidate.txt`](Windvale-Language-1.0-Source-Freeze-Candidate.txt):

- 2,700 exact bytes;
- SHA-256
  `152d7ae3b8463b395d42937b4271f757bb921d16046fd78354c7b0821c2b0099`;
- 183 identity-input files totaling 1,459,498 bytes; and
- aggregate candidate SHA-256
  `a750a141bb077fbd2ef42f8718a2bc5fbae3b02f8e862140dde436fbb91b65e3`.

The explicit freeze decision should cite the manifest byte length and SHA-256,
not copy a mutable list of files into prose.

## Practical result

Language 1.0 now has one coherent candidate contract:

- one human semantic specification;
- one human lexical/grammar specification and one matching machine EBNF;
- one behavioral Foundation specification and eleven exact, independently
  hashed Foundation major-1 module signature sets;
- one accepted design rationale and Seed migration plan;
- eleven owner-reviewed application/system workloads; and
- one reproducible manifest over the specifications, decisions, workload source,
  package data, oracles, rejected cases, and implementation responsibilities.

Freezing this candidate would fix the language design that implementation must
target. It would not say that the Seed compiler already accepts edition 1, that
Foundation is implemented, or that an editor, runtime, backend, operating
system, or permanent host has passed edition-1 conformance.

## Complete-suite corrections

The final reconciliation made these accepted corrections:

1. Added the complete machine grammar with 147 defined productions/tokens and no
   undefined reference. It projects all 138 human-document productions plus
   nine explicitly owned external scanner tokens.
2. Added the complete Foundation registry and exact candidate signature hashes
   for all eleven required modules. It closes previously prose-only Option,
   Result, iterator, formatting, vector, numeric-family, and profile boundaries.
3. Added the `while let Pattern = Expression` loop already required twice by the
   package-parser workload.
4. Replaced overlapping simple/destructuring statements with one unambiguous
   binding production.
5. Encoded the exact one-through-six Unicode-escape digit bound and
   zero-through-eight raw-literal delimiter bound in both grammar forms.
6. Corrected paper source to use `Bytes.Appendˉu8`,
   `Collections.Vectorˉconstructˉreserved`, and the canonical
   `Option.Present`/`Option.Absent` cases. The package parser now uses
   `Option.Isˉpresent` rather than an invalid overloaded equality test. The
   registry's `Sequenceˉlength` and `Sequenceˉat` parameter is now `Value`,
   matching the named argument in every paper call.
7. Deferred standalone interpolation syntax because no workload supplied its
   explicit destination owner, allocation budget, or failure path; the bounded
   formatting protocol and builders remain in edition 1.
8. Separated source-design freeze evidence from future implementation and
   qualification evidence.

The corrected GUI bundle remains 8 source modules, 2,004 LF lines, and 65,936
UTF-8 bytes. The corrected package bundle remains 7 source modules and 1,478 LF
lines and is now 48,468 UTF-8 bytes. These are name-level corrections; their
specified output bytes and semantic oracles do not change.

## Freeze-gate reconciliation

| Gate group | Candidate evidence | Standing |
| --- | --- | --- |
| Grammar and precedence | Human grammar plus machine EBNF; production-set and undefined-reference audit. | Ready for owner freeze |
| Foundation signatures and failures | Complete behavior document; eleven exact major-1 registry blocks and verified block hashes. | Ready for owner freeze |
| Mandatory usability evidence | 11 reviewed bundles; 64 source files; 14,445 LF lines; 479,069 UTF-8 source bytes. | Passed on paper |
| Ownership, cleanup, collections, and concurrency | Workloads 2–9 and 11 cover moves, borrows, release/completion, progress, maps, sets, arenas, slices, tasks, cancellation, and deterministic publication. | Passed on paper |
| Unsafe and target boundary | Workload 10 covers profile/target/ABI admission, pointer kinds, checked scratch/range/lifetime/aliasing, untrusted results, and terminal containment. | Passed on paper |
| Package data and shipment | Workloads 1, 7, 9, and 11 cover exact binding, strict text, accounting, content deduplication, and no implicit filesystem authority. | Passed on paper |
| Numeric and accelerator readiness | Workloads 8 and 11 cover strict float behavior, explicit conversion policy, bit-identical parallel equivalence, quantized storage, and the library/target/provider split. | Passed on paper |
| Compiler/library/runtime/tool ownership | Migration responsibility matrix and every bundle's implementation-responsibility document. | Ready for owner freeze |
| Canonical identities | Candidate manifest and eleven Foundation module hashes. | Ready for owner freeze |
| Current implementation conformance | Deliberately deferred to migration; Seed remains the implemented language. | Not claimed |

The source-freeze requirements that mention accepted, boundary, malformed, and
rejected cases are satisfied as paper contracts: the input, expected result,
failure owner/order, and bound are fixed. Migration must convert those contracts
into executable parser, semantic, Foundation, editor, formatter, cross-host, and
target tests before implementation conformance is claimed.

## Deliberately later, not missing

These items do not block the source-language freeze:

- production capability signature identities for filesystem, streams, database,
  display, networking, and accelerators;
- a verified accelerator kernel representation and physical GPU providers;
- WIR/WVB/native additions proven necessary by implementation;
- broader Unicode identifiers, localized keywords, sets beyond the accepted
  deterministic set, dynamic module loading, default arguments, or unrestricted
  macros;
- interpolated-text syntax until its bounded destination and memory owner are
  explicit; and
- measured compiler/runtime/editor/formatter and dual-host qualification.

They remain separate versioned library, format, provider, later-edition, or
implementation contracts. None may silently redefine edition-1 semantics.

## Effect of owner approval

An explicit approval to freeze this exact candidate should produce one named
decision that:

1. cites manifest SHA-256
   `152d7ae3b8463b395d42937b4271f757bb921d16046fd78354c7b0821c2b0099`;
2. changes the candidate suite's status to frozen Language 1.0;
3. authorizes migration slice 0 and then the ordered vertical implementation
   slices;
4. preserves Seed as the only implemented contract until each migration gate
   passes; and
5. requires any later semantic change to use a named decision and a new exact
   identity.

Until that approval, this review packet is the final candidate presented to the
owner, not the freeze itself.
