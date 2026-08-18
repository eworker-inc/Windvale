# Localization workload 2 accepted cases

## Rule

These are expected outcomes for the draft profile. Terminology quality remains
unaccepted until the native-review checklist is signed; structural acceptance
does not silently promote a draft pack.

## Dependency and artifact cases

| Case | Evidence | Expected result |
| --- | --- | --- |
| 1 | `zh-Hans@1` lock row names profile hash `a58160de…6f0ffe` | Select only the exact draft composite profile. |
| 2 | Profile reuses Unicode hash `2772f229…08ce9` | Use the same Unicode 17.0.0 source semantics as `en@1`. |
| 3 | Profile reuses token registry hash `cefa459d…c6286e` | Bind the same 66 canonical token identities and ordinals. |
| 4 | Lexicon has 66 registry-ordered rows | Give every localizable edition-1 token exactly one primary Chinese spelling. |
| 5 | Catalog binds interface hash `1fe70000…011e4` | Map the complete current `Foundationˉoption` public source surface. |
| 6 | Five new artifacts total 4,775 bytes | Match the exact artifact index before review begins. |

## Keyword cases

| Case | Source | Expected canonical token |
| --- | --- | --- |
| 7 | `模块` | `KW_MODULE` |
| 8 | `如果` | `KW_IF` |
| 9 | `返回` | `KW_RETURN` |
| 10 | `借用` | `KW_BORROW` |
| 11 | `汇合` / `取消汇合` / `失败汇合` | Three distinct structured-task policy tokens. |
| 12 | `字符标量` | `KW_RUNE`, not a project identifier. |
| 13 | A Chinese keyword followed by U+02C9 or an XID continuation | Treat the complete bytes as an identifier rather than a keyword prefix. |

## Catalog cases

| Case | Source label | Canonical identity |
| --- | --- | --- |
| 14 | `基础库ˉ可选值` | module `Foundationˉoption` |
| 15 | `可选值.有值 { 值: 真 }` | `Option.Present { Value: true }` |
| 16 | `可选值.无值` | `Option.Absent` |
| 17 | `获取ˉ借用` | operation `Borrow`, without colliding with keyword `借用` |
| 18 | Repeated parameter label `值` | Accept in distinct operation/case namespaces. |
| 19 | `转换器` and `值` under `映射` | Resolve the two distinct `Map` named parameters. |

## Stored-source cases

| Case | Evidence | Expected result |
| --- | --- | --- |
| 20 | `Zh-Hans-Application.wv` contains Chinese keywords and imported labels | Store the localized program directly; no editor-only translated view is required. |
| 21 | Both source files use project module `选项ˉ选择` | Preserve identical project-owned identity across the equivalence pair. |
| 22 | Both files retain platform IDs `windows`, `linux`, and `windvale` | Treat registered machine IDs as universal, not untranslated fallback. |
| 23 | Chinese source contains no canonical English keyword or Foundation label | Resolve entirely through the selected Chinese lexicon/catalog. |
| 24 | Canonical token and imported-declaration projections match the oracle | Preserve one parser, type system, optimizer, and artifact model. |

## Review-state cases

| Case | Event | Expected state |
| --- | --- | --- |
| 25 | AI-authored artifacts pass structural validation | Retain `draft`; do not claim native review. |
| 26 | A native reviewer requests a terminology change | Change exact artifact bytes, regenerate dependent hashes, and retain review history. |
| 27 | Every term is accepted and all structural/equivalence checks rerun | Advance to `reviewed`; qualification and distribution remain separate states. |
