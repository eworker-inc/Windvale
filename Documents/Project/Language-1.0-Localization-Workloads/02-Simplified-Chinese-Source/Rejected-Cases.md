# Localization workload 2 rejected and boundary cases

## Rule

Workload 1 owns general malformed artifact behavior. These cases add
Simplified-Chinese terminology, source, catalog, equivalence, and review-state
failures. Rejection publishes no profile generation or qualification claim.

## Terminology and lexical rejection

| Case | Mutation | Earliest rejection |
| --- | --- | --- |
| 1 | Two token IDs receive the same Chinese spelling | Lexicon exact-collision validation. |
| 2 | Two token spellings have the same admitted LTR or RTL confusable skeleton | Lexicon security validation. |
| 3 | A spelling is not NFC, violates XID/Allowed, contains U+02C9, or exceeds its limit | Unicode/lexicon admission. |
| 4 | A Chinese token is recognized by prefix before an identifier continuation | Lexer boundary differential. |
| 5 | `汇合`, `取消汇合`, and `失败汇合` collapse to one token | Registry-to-lexicon comparison. |
| 6 | A fixed-width numeric word or registered machine identity is inserted as a localized keyword row | Lexicon format/completeness rejection. |
| 7 | English keyword spelling is accepted as fallback under `zh-Hans@1` | Ordinary identifier or syntax failure; no fallback. |

## Catalog rejection

| Case | Mutation | Earliest rejection |
| --- | --- | --- |
| 8 | Catalog uses single-segment operation label `借用` while `借用` is a keyword | Keyword/public-label usability collision review. |
| 9 | Catalog omits one of the 16 source-addressable keys | Complete interface comparison. |
| 10 | Catalog adds generic parameter `T` as a localized source label | Catalog source-addressability rejection. |
| 11 | `映射` parameters `转换器` and `值` collapse to one label | Operation-namespace collision validation. |
| 12 | A label is exact/confusable with another competitor in its resolution namespace | Catalog security validation. |
| 13 | Catalog interface hash differs from `Foundationˉoption` major 1 | Import catalog binding. |
| 14 | Canonical English member name works implicitly in Chinese source | Public-name resolution failure; no canonical fallback. |

## Stored-source and equivalence rejection

| Case | Mutation | Earliest rejection |
| --- | --- | --- |
| 15 | Chinese source contains an English localizable keyword | Lexer treats it as an identifier or syntax error under the strict profile. |
| 16 | Chinese source names canonical `Foundationˉoption` without an explicit Chinese catalog entry selecting that label | Import resolution failure. |
| 17 | Converter changes `选项ˉ选择`, `选择ˉ启用`, `已启用`, or `选项库` while only profile conversion was requested | Conversion ownership failure. |
| 18 | Converter translates `windows`, `linux`, or `windvale` | Registered machine-identity failure. |
| 19 | Corresponding source produces a different canonical keyword token | Canonical token-stream comparison. |
| 20 | Corresponding source resolves a different public declaration/case/parameter | Canonical declaration-stream comparison. |
| 21 | A later compiler stage branches on `zh-Hans` rather than canonical tokens/IDs | Architecture/conformance failure. |
| 22 | Raw source hashes are expected to match | Oracle misuse; raw source is intentionally different. |

## Review and publication rejection

| Case | Event | Required outcome |
| --- | --- | --- |
| 23 | AI draft is described as native-reviewed, qualified, or official | Reject the status claim. |
| 24 | Reviewer approval refers to prose that differs from exact artifact bytes | Reject review evidence and regenerate the packet. |
| 25 | One term changes without updating profile, lock, and artifact-index hashes | Hash-chain rejection. |
| 26 | Reviewer identity, language competence, reviewed hashes, or disposition is missing | Keep status `draft`. |
| 27 | Review approves terminology but structural/equivalence validation fails | Keep status `draft`; correctness precedes promotion. |
| 28 | Pack is shipped automatically because the host locale is Chinese | Installer/profile-selection failure; installation remains explicit. |
