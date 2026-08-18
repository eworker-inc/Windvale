# Simplified Chinese native-review checklist

## Current disposition

| Field | Value |
| --- | --- |
| Profile | `zh-Hans@1` |
| Current status | `draft` |
| Draft origin | AI-authored engineering candidate |
| Native technical reviewer | Not yet assigned |
| Independent readability reviewer | Not yet assigned |
| Reviewed artifact-index hash | Not yet approved |
| Review date | Not yet approved |

No unchecked or empty field may be interpreted as approval.

## Required reviewer perspectives

Official promotion should include:

1. a fluent native Simplified Chinese reviewer with professional programming or
   compiler experience; and
2. an independent fluent reader who did not author the terminology, focused on
   readability, ambiguity, and Mainland/Singapore or broader `zh-Hans`
   expectations.

One person may establish the initial `reviewed` state when necessary. The
independent perspective is required before `qualified` or `officially
distributed` status.

## Keyword review

For every one of the 66 rows, record `accept`, `replace`, or `retain canonical`
and answer:

- Does the term express Windvale's exact meaning rather than only its English
  dictionary meaning?
- Does it read naturally in declarations and statements, not only in a glossary?
- Is it distinct from every other keyword by spelling and likely appearance?
- Could it be confused with a common project identifier or public API label?
- Do paired families remain coherent: immutable/mutable, borrow/move/copy,
  async/await/task, join/cancel-join/fail-join, record/enum/variant, and
  profile/platform/authority/capability?
- Is the term appropriate across communities that use Simplified Chinese, or
  must the pack identity be narrowed?

Special attention is mandatory for `权限`, `能力`, `效应`, `配置`, `令`, `汇合`,
`取消汇合`, `失败汇合`, `字符标量`, and `可选`.

## Public-library review

For all 16 `Foundationˉoption` labels:

- Verify that `基础库ˉ可选值` and `可选值` communicate a typed optional value,
  not an implicit nullable reference.
- Verify that `无值` and `有值` are clear exhaustive variant cases.
- Verify that `获取ˉ借用` and `获取ˉ可变ˉ借用` distinguish API operations from
  the `借用` syntax keyword without sounding artificial.
- Verify that `映射`, `取出`, `是否ˉ有值`, `转换器`, and `值` fit real call sites.
- Verify U+02C9 concept boundaries and whether a concept should instead remain
  one natural Chinese word.
- Verify that repeated `值` parameters are readable in each owning namespace.

## Source-reading exercises

Reviewers read both source files without first reading the mapping table, then:

1. describe the program's behavior in Chinese;
2. identify which names belong to the project and which come from Foundation;
3. identify the immutable/mutable, ownership, effect, authority, and platform
   information visible in the file;
4. flag any phrase that reads like translated prose rather than code;
5. type or edit the Chinese file with a normal IME and report awkward token
   boundaries; and
6. compare the Chinese file with the canonical projection and confirm that the
   same semantic program is visible.

## Mechanical checks after every review edit

- strict UTF-8, NFC, XID, Allowed, and Highly Restrictive admission;
- no default-ignorables, join controls, bidi controls, or mixed numbers;
- exact and confusable uniqueness across all keyword spellings;
- keyword/public-label usability collision audit;
- complete catalog keys and namespace-local label uniqueness;
- paired-source canonical token and declaration mapping;
- new lexicon, vocabulary/catalog when changed, profile, lock, and artifact-index
  sizes and hashes; and
- clean changed-verification planning for paper evidence.

## Status transitions

| From | To | Required evidence |
| --- | --- | --- |
| `draft` | `reviewed` | Named native technical reviewer, exact reviewed hashes, disposition for every term, and passing mechanical checks. |
| `reviewed` | `qualified` | Independent fluent review plus executable compiler/editor/equivalence evidence on Windows and Linux. |
| `qualified` | `officially distributed` | Project-owner release decision, signed package metadata, installer selection, update/rollback, and shipment-size evidence. |

Terminology review alone never authorizes compiler implementation or distribution.
