# Localization workload 1 review findings

## Status

These findings from the complete first-author bundle are accepted by the project
owner as replacement-candidate directions. No finding is an implementation
authorization or source freeze.

## Finding 1: one canonical record format is sufficient

Accept strict UTF-8/LF delimiter records with fixed order, no escaping,
externally supplied SHA-256, and per-format bounds. JSON canonicalization,
executable plugins, arbitrary metadata, and embedded paths add complexity without
helping the source front door.

## Finding 2: the composite profile is the source-selected unit

Keep `#!wv/1 <profile>@<version>` as the only file selector. The selected
`.wvsp` binds Unicode, token registry, keyword lexicon, and vocabulary profile.
Do not expose independent in-file switches or an omitted English default.

## Finding 3: Unicode 17.0.0 is the working edition-1 table identity

Pin the exact eleven upstream files, sizes, and hashes in `Unicode-17-Source.wvup`.
Use NFC, XID, UTS #39 Allowed, Highly Restrictive scripts, one numeric system per
segment, and scoped LTR/RTL confusable rejection. Host Unicode tables are never
semantic input.

## Finding 4: join controls remain outside edition 1 for now

Reject all default-ignorables, including ZWJ and ZWNJ. This is simple and secure
but can prevent preferred spellings in some languages. Localization workload 4
must test real Arabic-derived and other affected source with native reviewers;
only a later named decision may add exact contextual rules.

## Finding 5: stable token IDs are independent of English source bytes

Accept the 66-row registry with contiguous four-digit ordinals and mnemonic
`KW_*` identities. Lexicons repeat ordinal and identity, allowing a stale or
reordered mapping to fail before lexer publication.

## Finding 6: generic parameter labels are not source catalog entries

Catalog modules, declarations, cases, fields, operations, and named value
parameters. Generic type arguments remain positional in edition 1, so `T` and
`U` are documentation labels rather than source-addressable API vocabulary.

## Finding 7: synthetic Unicode evidence precedes translation qualification

Retain `test-Unicode@1` only as a structural fixture. It proves non-ASCII bytes,
normalization, keyword mapping, catalog resolution, and canonical lowering while
making no Chinese/Japanese terminology claim. A real `zh-Hans@1` profile belongs
to workload 2 and requires named native review.

## Finding 8: content hashes remain external and non-self-referential

The lock hashes composite profiles; profiles hash components; catalogs hash
their vocabulary profile and bind an interface hash. No artifact contains its
own content hash. Exact whole-file bytes, including final LF, are the hash input.

## Finding 9: cache publication is generation-scoped and atomic

Key components by format plus exact content hash. Validate privately, publish
only complete immutable state, retain request source spans separately, release
race losers, avoid durable negative caching, and bound generation teardown.

## Finding 10: algorithmic bounds are ready; host thresholds are not

Accept the one-pass, no-pairwise-scan structural requirements and measurement
protocol. Do not invent time or memory thresholds before a representative
validator/compiler front door is measured on Windows and Linux. Those results
remain an implementation-qualification and release blocker, not a design-freeze
blocker.

## Quantitative record

| Evidence | Value |
| --- | ---: |
| Exact content artifacts | 11 |
| Artifact bytes | 12,895 |
| Artifact index bytes / SHA-256 | 1,214 / `562be979…0d3c9f` |
| Composite profiles | 2 |
| Shared Unicode profiles / token registries | 1 / 1 |
| Lexicon rows per profile | 66 |
| `Foundationˉoption` labels per catalog | 16 |
| Accepted cases | 25 |
| Rejected/boundary cases | 43 |
| Current compiler/runtime implementation | None |

## Accepted disposition

Accept findings 1 through 10 as the input to localization workloads 2 through 5.
All later paper workloads are now reconciled by Decision 0766. Native review of
an optional natural-language pack and executable/measured results remain
qualification gates; the exact formats, algorithms, cases, structural bounds,
and measurement protocol may enter the replacement design manifest.
