# Windvale Language 1.0 source-freeze review

## Owner decision status

The replacement Language 1.0 design candidate is complete but not frozen or
implemented. [Decision 0765](../Decisions/0765-Complete-Language-1.0-Source-Freeze-Candidate.md)
accepts the original eleven-workload reconciliation, and
[Decision 0766](../Decisions/0766-Complete-Language-1.0-Localized-Source-Reconciliation.md)
accepts the five localized-source workloads and their complete-suite
reconciliation. A separate source-freeze decision must cite the exact
replacement manifest before migration implementation begins.

The earlier pre-localization candidate remains immutable historical evidence in
[`Windvale-Language-1.0-Source-Freeze-Candidate.txt`](Windvale-Language-1.0-Source-Freeze-Candidate.txt):

- 2,700 exact bytes;
- manifest SHA-256
  `152d7ae3b8463b395d42937b4271f757bb921d16046fd78354c7b0821c2b0099`;
- 183 identity-input files totaling 1,459,498 bytes;
- aggregate candidate SHA-256
  `a750a141bb077fbd2ef42f8718a2bc5fbae3b02f8e862140dde436fbb91b65e3`;
  and
- repository revision `c060cb3553a06ed97c4b42d751534f7f4bcaa62e`.

The replacement candidate has its own exact
[`Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt`](Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt)
manifest:

- 3,702 exact bytes;
- manifest SHA-256
  `c9517841eae6b6e86778cb1dd88711feb38929dec8fe79e084eec44fa22c512a`;
- 250 identity-input files totaling 1,724,854 bytes; and
- aggregate candidate SHA-256
  `fb918a763ae7c8c85dd1a2ffecee6587ab93bbf846ae31ae19b53509aed36a0a`.

It does not edit or silently reinterpret the historical identity above.

## Practical result

The replacement candidate now contains one coherent source design:

- one semantic specification;
- one human grammar and matching machine EBNF;
- one exact Foundation behavior contract and eleven independently hashed
  Foundation major-1 signature sets;
- eleven owner-reviewed application/system workload bundles;
- one universal explicit source descriptor and seven exact source-profile data
  formats;
- one canonical token/declaration model beneath exact stored source lexicons and
  public-library vocabularies;
- exact Unicode 17.0.0 identifier, script, number, confusable, bidirectional,
  source-display, and provenance rules;
- deterministic conversion, formatter, editor, diagnostic, copy/paste, and
  source-map contracts;
- content-addressed shipment, immutable update/rollback, compiler-service cache,
  cross-host comparison, and performance-measurement contracts; and
- one ordered Seed-to-edition-1 migration plan without a parallel compiler.

This is a design identity. It does not mean that the Seed compiler accepts
edition 1, that the new Foundation exists, or that an editor, runtime, backend,
installer, operating system, or permanent host has passed edition-1
qualification.

## Complete-suite reconciliation

Decision 0765 retains these original corrections:

1. one complete machine grammar with all human productions and externally owned
   scanner tokens;
2. one complete Foundation registry and eleven exact module signature hashes;
3. the required `while let Pattern = Expression` loop;
4. one unambiguous binding production over `Pattern`;
5. exact Unicode-escape and raw-delimiter bounds;
6. corrected Foundation spellings in paper source;
7. explicit bounded formatting while standalone interpolation remains later;
   and
8. a strict separation between source-design evidence and implementation
   qualification.

Decision 0766 adds these replacement corrections:

1. every file begins with `#!wv/1 <profile>@<version>` and has no ambient or
   omitted language default;
2. stored localized keywords and public-library labels lower to one canonical
   token and declaration model;
3. project-owned identifiers use exact pinned Unicode semantics while registered
   machine identities remain ASCII-safe;
4. the exact seven artifact formats are the complete edition-1 semantic pack
   surface;
5. one logical left-to-right grammar remains valid for every script;
6. ambiguous localized imports fail because no untested project-override format
   enters edition 1;
7. display catalogs and translated project-name views remain non-semantic later
   tooling;
8. runtime, source profile, diagnostics, documentation, and application
   localization are separate package selections;
9. exact content deduplication and generation-scoped caches avoid repeated pack
   storage, hashing, and parsing; and
10. numeric performance ceilings follow first-implementation measurement rather
    than being invented before the compiler exists.

## Freeze-gate reconciliation

| Gate group | Replacement evidence | Standing |
| --- | --- | --- |
| Grammar and precedence | Human grammar plus machine EBNF, explicit descriptor, canonical-token mapping, Unicode external scanners, and production audit. | Passed on paper |
| Foundation signatures and failures | Complete behavior document; eleven exact major-1 registry blocks and verified block hashes. | Passed on paper |
| Application/system usability | 11 reviewed bundles; 64 source files; complete accepted/rejected, ownership, capability, failure, and cleanup walkthroughs. | Passed on paper |
| Ownership, collections, concurrency, unsafe, targets, and AI readiness | Decisions 0752 through 0764 and the eleven bundles. | Passed on paper |
| Source-profile admission | Workload 1: seven formats, 11 artifacts, 25 accepted and 43 rejected cases. | Passed on paper |
| Simplified Chinese source | Workload 2: 66-keyword draft, one complete 16-label catalog, paired source, exact hashes, and mechanical equivalence. | Mechanism passed; terminology remains draft |
| Conversion and source tooling | Workload 3: 30 accepted, 30 rejected, and three exact expected-source fixtures. | Passed on paper |
| Unicode and multilingual security | Workload 4: 32 accepted, 46 rejected, exact Unicode-17 validation, and multilingual source. | Passed on paper |
| Shipment, cache, cross-host, and performance | Workload 5: 34 accepted, 42 rejected, exact 12,288-byte fixture inventory, and qualification protocol. | Passed on paper |
| Compiler/library/runtime/tool ownership | Migration responsibility matrix plus workload implementation-responsibility records. | Passed on paper |
| Canonical identity | Replacement exact manifest over all selected normative and evidence inputs. | Ready for explicit freeze decision |
| Current implementation conformance | Deliberately assigned to migration and qualification; Seed remains implemented. | Not claimed |

## Native-language review boundary

No AI author can certify that every Chinese technical term sounds natural to a
native programmer. The exact `zh-Hans@1` artifacts therefore remain `draft`.
Promotion requires the named native technical reviewer, an independent fluent
readability reviewer, exact reviewed hashes, rerun mechanical checks, executable
Windows/Linux evidence, and an official distribution decision.

That honest limitation does not block freezing the generic Language 1.0
mechanism. The exact descriptor, format, Unicode, lookup, conversion, failure,
and shipment rules are language-independent and have a synthetic Unicode oracle.
`en@1` remains the minimal canonical developer profile. An optional Chinese pack
cannot be described or shipped as official until its separate qualification
gate passes.

## Deliberately later or implementation-owned

These items do not block source-language freeze:

- executable compiler/parser/type/ownership/formatter/editor cases derived from
  the paper workloads;
- native Chinese terminology approval and any additional official language pack;
- measured Windows/Linux compiler-service time, allocation, retained-state, and
  cache ceilings;
- package/installer implementation and signed language-pack distribution;
- production capability identities for filesystem, streams, database, display,
  networking, and accelerators;
- verified accelerator representations and physical GPU providers;
- WIR, WVB, object, or native additions proven necessary by implementation;
- a project localized-name override, non-semantic display catalog, translated
  project-identifier view, join-control relaxation, default arguments, dynamic
  module loading, unrestricted macros, or standalone interpolation; and
- target-specific optimization or packaging work that does not alter the source
  contract.

Each remains a separately versioned implementation, library, provider, package,
tooling, or later-edition decision. None may silently redefine edition-1
semantics.

## Replacement approval path

The preserved pre-localization manifest must not be frozen. The explicit
replacement source-freeze decision should:

1. cite the replacement manifest's exact byte length and SHA-256;
2. change only that replacement suite's status to frozen Language 1.0;
3. authorize migration slice 0 and then the ordered vertical implementation
   slices;
4. preserve Seed as the only implemented contract until each migration gate
   passes;
5. keep `zh-Hans@1` draft until its language-specific promotion evidence exists;
   and
6. require any later semantic change to use a named decision and new exact
   identity.

Until that approval, the replacement suite is complete candidate design, not an
implemented or compatibility-promised language.
