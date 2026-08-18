# Localization workload 1 accepted cases

## Rule

Every case must resolve from explicit bytes and hashes. Host locale, working
directory, installation order, network state, and previously admitted profiles
are not inputs.

## Descriptor and lock cases

| Case | Input | Expected result |
| --- | --- | --- |
| 1 | `#!wv/1 en@1` at byte zero with LF | Select the exact `en@1` lock row. |
| 2 | `#!wv/1 test-Unicode@1` with CRLF in an externally edited file | Admit one logical descriptor line and preserve the raw source hash. |
| 3 | Two source files select the same profile hash | Share one immutable validated generation after independent descriptor spans are retained. |
| 4 | Lock contains both reference profiles and both catalogs in canonical order | Admit all four rows without filesystem or network search. |
| 5 | Content store returns the exact expected bytes | Hash first, then parse under the declared artifact format. |

## Artifact-chain cases

| Case | Evidence | Expected result |
| --- | --- | --- |
| 6 | `en@1` profile hash `e678b1b5…bdaf27` | Bind Unicode profile, token registry, English lexicon, and English vocabulary profile exactly. |
| 7 | `test-Unicode@1` profile hash `40f71700…6deba` | Bind the shared Unicode/registry artifacts plus the synthetic lexicon/vocabulary artifacts. |
| 8 | Both profiles reference token registry hash `cefa459d…c6286e` | Observe the same 66 stable token identities and ordinals. |
| 9 | Both profiles reference Unicode profile hash `2772f229…08ce9` | Use the same Unicode 17.0.0 properties and security rules. |
| 10 | Every dependent identity/version/hash matches its parsed content | Publish one private complete candidate, then make it visible atomically. |

## Lexicon cases

| Case | Evidence | Expected result |
| --- | --- | --- |
| 11 | English lexicon has 66 registry-ordered rows | Every mapped token has exactly one canonical English source spelling. |
| 12 | Synthetic lexicon changes four rows only | `偽測試`, `条件測試`, `返却測試`, and `真測試` map to `KW_FALSE`, `KW_IF`, `KW_RETURN`, and `KW_TRUE`. |
| 13 | Canonical spelling equals a technical word retained by another profile | Accept because the exact selected profile supplies that primary row. |
| 14 | Keyword followed by punctuation or token whitespace | Recognize the complete mapped token. |
| 15 | Keyword text followed by U+02C9 or an identifier continuation | Treat it as identifier input rather than a keyword prefix. |

## Unicode identifier cases

These representative NFC identifiers contain no join control or bidi formatting
control and are expected to satisfy the pinned Allowed/Highly-Restrictive profile:

| Language/script pressure | Identifier |
| --- | --- |
| Spanish/Latin | `Políticaˉdeˉentrega` |
| Simplified Chinese/Han | `配送ˉ政策` |
| Japanese/Han and Katakana in separate segments | `配送ˉポリシー` |
| Korean/Hangul | `배송ˉ정책` |
| Arabic | `سياسةˉالتوصيل` |
| Hebrew | `מדיניותˉמשלוח` |

Each segment is checked independently. Exact ordinal NFC UTF-8 bytes remain the
identifier identity; case folding, transliteration, locale collation, and silent
normalization do not occur.

## Catalog cases

| Case | Evidence | Expected result |
| --- | --- | --- |
| 16 | English catalog hash `91f5d7c0…bacd37` | Bind all 16 canonical `Foundationˉoption` source labels to the exact interface hash. |
| 17 | Synthetic catalog hash `3daed357…09b0f` | Bind the same 16 canonical keys to its Unicode labels. |
| 18 | Repeated localized `対象` parameter under different operations | Accept because each parameter resolves in a distinct operation namespace. |
| 19 | `Map` has localized labels for both `Transform` and `Value` | Resolve named arguments without position or result-type inference. |
| 20 | Catalog omits generic parameter names `T` and `U` | Accept because edition-1 generic arguments are positional and those names are explanatory only. |

## Source equivalence case

`Source/En-Admission.wv` and `Source/Test-Unicode-Admission.wv` must lower to:

- the same canonical keyword token sequence after the descriptor;
- the same canonical imported module, type, case, and named-parameter identities;
- the same project-owned module, alias, function, parameter, type, and literal
  identities; and
- different raw-source/profile/catalog provenance.

No current compiler acceptance or artifact equality is claimed by this paper
case.

## Cache cases

| Case | Event | Expected state |
| --- | --- | --- |
| 21 | First request validates a new exact hash chain | Candidate state remains request-private until complete publication. |
| 22 | Concurrent request asks for the same component hash | It may await one bounded validation or independently validate; at most one immutable published value survives. |
| 23 | Warm request uses the same complete profile and catalog hashes | Reuse validated immutable tables while creating new request-owned source maps and diagnostics. |
| 24 | Compiler-service generation retires | Release tables when no request retains that generation. |
| 25 | Raw source spelling changes but canonical tokens do not | Reuse semantic work only when raw diagnostics/debug provenance is regenerated correctly. |
