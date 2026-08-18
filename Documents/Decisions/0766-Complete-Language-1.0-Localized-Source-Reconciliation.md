# Decision 0766: Complete Language 1.0 localized-source reconciliation

- Status: Accepted
- Date: 2026-08-18

## Context

The project owner held the exact pre-localization Language 1.0 candidate from
Decision 0765, then selected one universal source descriptor, stored localized
keywords, exact stored public-library source labels, and Unicode project
identifiers for the replacement candidate. Five localization workloads tested
that direction:

- Workload 1 defines seven exact bounded source-profile artifact formats, 25
  accepted cases, 43 rejected cases, and canonical plus synthetic Unicode
  reference chains;
- Workload 2 supplies a complete first-author `zh-Hans@1` draft with all 66
  keyword mappings, one complete 16-label Foundation catalog, paired source,
  exact hashes, and mechanical equivalence, while correctly retaining draft
  status pending native review;
- Workload 3 defines deterministic source conversion, formatting, copy/paste,
  rename, diagnostics, and provenance through 30 accepted and 30 rejected cases;
- Workload 4 fixes Unicode 17.0.0 identifier, normalization, script, number,
  confusable, bidirectional-source, and display security through 32 accepted and
  46 rejected cases; and
- Workload 5 defines installation selection, exact-content deduplication,
  immutable update/rollback, compiler-service cache generations, cross-host
  comparison, and performance measurement through 34 accepted and 42 rejected
  cases.

Complete reconciliation found two scope errors in the exploratory material. It
mentioned a project source-vocabulary override without an exact artifact format,
and it treated several future natural-language packs and measured compiler
thresholds as prerequisites for freezing the design. Neither is justified: an
undefined override would hide a new semantic build input, while an unimplemented
front door cannot supply honest timing thresholds.

## Decision

Accept the five localization workload findings and prepare the replacement
Language 1.0 source-freeze candidate with these rules.

1. Every edition-1 source file begins at byte zero with the ASCII descriptor
   `#!wv/1 <profile>@<version>`. There is no omitted, host-locale, installed-pack,
   or English fallback.
2. One exact composite source profile binds the Unicode profile, canonical
   keyword-token registry, one complete keyword lexicon, and one public source-
   vocabulary profile. Exact interface-bound catalogs map stored public labels
   to the library's one canonical declaration identity.
3. Adopt the seven Workload 1 artifact formats and external SHA-256 binding as
   the complete Language 1.0 semantic localization-data surface. Packs are
   bounded declarative data, not compiler plugins or capability providers.
4. Adopt the Workload 4 Unicode 17.0.0 boundary: exact NFC/XID/Allowed/Highly
   Restrictive inputs, per-segment script and decimal-number checks, scoped
   LTR/RTL `bidiSkeleton` collision rejection, UTS #55 source-aware display, and
   no identifier join controls in edition 1.
5. Keep one logical left-to-right grammar. Localized tokens and identifiers may
   use right-to-left scripts, but they do not reorder syntax or delimiters.
6. Do not add a project source-vocabulary override or semantic display-catalog
   format to Language 1.0. An ambiguous localized module/declaration label is a
   hard build failure. A later proposal may add an exact disambiguation or
   presentation format with its own workload and decision.
7. Deterministic conversion changes only the descriptor, keyword spellings, and
   resolved public-library labels. It preserves project identifiers, comments,
   literals, resources, schemas, whitespace, and other prose unless a separate
   explicitly named operation owns that change.
8. Source profile, diagnostic language, documentation language, and application
   runtime localization remain independent selections. Runtime-only products
   contain no source-localization data.
9. Reuse the existing content-addressed package/store/generation architecture.
   The minimal developer selection contains the shared edition data and `en@1`;
   other source profiles, diagnostics, and documentation are explicit optional
   packages. The current English/Chinese fixture contains 12,288 unique semantic
   bytes, not a duplicate compiler or runtime.
10. The first compiler implementation uses generation-scoped immutable content,
    composite-profile, and module-front-door caches. Warm hits reread, rehash,
    and reparse zero unchanged localization artifact bytes. Persistent cross-
    process semantic caching is deferred until measurement justifies its added
    trust and invalidation surface.
11. Replacement source freeze accepts exact formats, algorithms, resource
    bounds, cases, and measurement protocols. First-implementation qualification
    establishes reviewed Windows/Linux numeric time and memory ceilings; release
    qualification enforces them.
12. `en@1` is the canonical minimal developer profile. `zh-Hans@1` remains a
    draft shipment target until the named native technical and independent
    readability reviews, executable equivalence, editor, cross-host, installer,
    and performance gates pass. The draft terminology is evidence for the
    mechanism, not an official-language claim.
13. Namespaced community profiles may use the same exact formats and explicit
    build locking. They do not become Windvale-qualified or officially
    distributed without the same language-specific review and qualification
    states.

## Non-decision

This decision does not freeze Language 1.0, authorize compiler/editor/package
implementation, claim that current Seed tools accept edition 1, certify Chinese
terminology, or publish an official Chinese pack. It does not localize machine
identities, numeric type spellings, punctuation, operators, runtime APIs,
application resources, or user data.

The separately generated replacement manifest remains the identity that a
future source-freeze decision must cite. That freeze does not need to wait for
native review of an optional draft pack, but official shipment of that pack
does.

## Consequences

Language 1.0 keeps one compiler and one semantic token/declaration model while
allowing complete stored source in a selected human language. Profiles are
small immutable source dependencies; they neither duplicate executable code nor
grant authority. Exact Unicode and bidirectional rules make multilingual source
portable and reviewable instead of inheriting host behavior.

The candidate has no undefined localization artifact. Tool presentation may
grow independently, but it cannot enter builds accidentally. Implementation can
now proceed in the existing migration slices after the separate source-freeze
decision, with simple correctness oracles and focused verification owners.

## Reconsideration triggers

Reconsider this decision if implementation cannot preserve byte-identical
canonical semantic output across admitted profiles, a real dependency closure
cannot be expressed without a project disambiguation artifact, Unicode security
rules reject essential reviewed identifiers, the one-profile-per-file rule makes
ordinary source materially unusable, or measured loading/cache costs justify a
different bounded trust protocol.
