# Localization workload 1 rejected and boundary cases

## Rule

Each case names the earliest deterministic owner. Rejection publishes no
partially validated profile, lexicon, vocabulary, catalog, Unicode table, cache
entry, or semantic artifact. One primary bounded diagnostic may carry a fixed
small set of related fields.

## Descriptor rejection

| Case | Mutation | Earliest rejection |
| --- | --- | --- |
| 1 | Missing descriptor, byte-order mark, leading space, or comment before `#!wv` | Descriptor reader before profile lookup. |
| 2 | Wrong magic/edition, two spaces, tab, extra field, non-ASCII profile syntax, or missing line ending | Descriptor grammar. |
| 3 | Descriptor exceeds 128 bytes or profile identity exceeds 96 bytes | Descriptor length admission while scanning. |
| 4 | Version is zero, signed, underscored, has a leading zero, or exceeds `u32` | Descriptor numeric admission. |
| 5 | Profile is absent from the explicit lock | Build-input resolution; no installation search. |

## Common artifact rejection

| Case | Mutation | Earliest rejection |
| --- | --- | --- |
| 6 | Invalid UTF-8, BOM, CR, NUL, tab, blank line, comment, or missing final LF | Common byte admission. |
| 7 | Record exceeds 1,024 bytes or artifact exceeds its format maximum | Streaming size admission before retained allocation. |
| 8 | Header/end mismatch, unknown record, wrong order, duplicate record, empty field, or unexpected delimiter | Format-specific record parser. |
| 9 | Uppercase/short/long/nonhex SHA-256, malformed identity, or malformed version/count | Field parser. |
| 10 | Actual bytes do not match the externally expected SHA-256 | Content admission before record parsing. |
| 11 | Declared count is smaller/larger than rows or arithmetic overflows | Format-specific completion check. |

## Unicode-profile rejection

| Case | Mutation | Earliest rejection |
| --- | --- | --- |
| 12 | Unknown Unicode policy enum or unsupported reference revision | Unicode-profile parser. |
| 13 | Input rows are unsorted, duplicated, missing, or have duplicate URLs | Unicode-profile structural admission. |
| 14 | Upstream byte length/hash differs from the pinned row | Unicode table generation or supplied-table proof. |
| 15 | Compiler substitutes host Unicode, normalization, collation, or security tables | Conformance failure before source qualification. |
| 16 | Identifier is not NFC but would normalize to an admitted spelling | Identifier lexer; report exact required NFC spelling without rewriting source. |
| 17 | Identifier contains a default-ignorable, ZWJ, ZWNJ, bidi control, private-use, unassigned, noncharacter, Pattern_Syntax, or Pattern_White_Space scalar | Identifier security admission. |
| 18 | Segment begins with a digit or contains a scalar outside XID/Allowed plus `_` | Identifier lexical admission. |
| 19 | Segment is Moderately/Minimally Restrictive or mixes decimal-number systems | UTS #39 restriction/mixed-number admission. |
| 20 | Distinct competing labels have the same LTR or RTL skeleton in one lookup scope | Confusable-scope admission with both locations. |

Join controls are rejected even where a natural language normally uses them.
Localization workload 4 must determine whether a later contextual exception is
necessary; a lexicon cannot create an exception itself.

## Registry and lexicon rejection

| Case | Mutation | Earliest rejection |
| --- | --- | --- |
| 21 | Token ordinal is missing, repeated, noncontiguous, out of order, or not four digits | Token-registry parser. |
| 22 | Token identity is malformed, duplicated, or the edition-1 count is not 66 | Token-registry completion. |
| 23 | Lexicon references wrong registry/Unicode identity, version, or hash | Lexicon dependency admission. |
| 24 | Lexicon row changes/reorders an ordinal or token identity | Lexicon-to-registry comparison. |
| 25 | Missing/extra entry, duplicate spelling, confusable spelling, universal numeric word collision, U+02C9, or invalid segment | Lexicon completion/security admission. |
| 26 | English spelling appears in a selected profile where that profile chose a different primary spelling | Lexer produces an ordinary identifier or syntax failure; no fallback token. |

## Vocabulary and catalog rejection

| Case | Mutation | Earliest rejection |
| --- | --- | --- |
| 27 | Vocabulary profile uses a different Unicode profile or catalog format | Vocabulary-profile admission. |
| 28 | Catalog vocabulary identity/version/hash differs from selected profile | Catalog dependency admission. |
| 29 | Package/module/major/interface hash differs from imported canonical interface | Import catalog binding. |
| 30 | Catalog kind is unknown, rows are unsorted, key exceeds 512 bytes, or label exceeds its byte/scalar limit | Catalog parser. |
| 31 | Canonical key is missing, extra, duplicate, stale, or not source-addressable | Complete interface-to-catalog comparison. |
| 32 | Two labels are exact/confusable competitors in one namespace | Catalog namespace admission. |
| 33 | Catalog attempts a generic-parameter label, fallback chain, alias, executable hook, path, or URL | Catalog format rejection. |

## Composite profile and lock rejection

| Case | Mutation | Earliest rejection |
| --- | --- | --- |
| 34 | Profile identity/version does not match descriptor or lock row | Composite-profile admission. |
| 35 | Referenced component identity/version/edition/hash does not match parsed content | Component dependency admission. |
| 36 | Lock profile/catalog rows are unsorted, duplicated, or disagree with counts | Lock parser. |
| 37 | Lock contains path, URL, fallback, host selector, or installation selector | Lock format rejection. |
| 38 | Required content hash is unavailable from approved build inputs | Build-input failure; compiler does not fetch. |

## Cache and resource rejection

| Case | Event | Required outcome |
| --- | --- | --- |
| 39 | Validation fails after some private tables were constructed | Destroy candidate state; publish nothing. |
| 40 | Two concurrent candidates race to publish the same hash | Keep one immutable equivalent entry; release the loser without replacing live state. |
| 41 | Same identity/version arrives with a different hash | Treat as different/unapproved content; identity alone never hits cache. |
| 42 | Retained cache, waiter, work, or diagnostic bound would be exceeded | Bounded rejection or eviction of an unreferenced generation; never unbounded growth. |
| 43 | Negative result is offered as a shared permanent cache entry | Reject that cache policy; later valid content must not be poisoned. |
