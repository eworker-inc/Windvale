# Language 1.0 localization workload 2: Simplified Chinese source

## Status

Complete first-author draft for the `zh-Hans@1` source-profile workload. The
artifact, terminology, source-equivalence, and review packets are ready for
native-language technical review, but no native reviewer has approved them yet.
The project owner accepts the structural and technical design findings. This
bundle therefore still does not claim a qualified Chinese pack, implementation,
cross-host artifact equivalence, or release readiness.

## Result first

The draft demonstrates that one Windvale file can store its programming surface
in Simplified Chinese without creating a Chinese compiler:

- all 66 localized keyword spellings map to the existing canonical token IDs;
- the complete 16-label `Foundationˉoption` interface is available through one
  hash-bound Simplified Chinese catalog;
- the source profile reuses the exact Unicode 17.0.0 and token-registry inputs
  from Workload 1;
- the Chinese application uses Chinese keywords, library labels, module,
  function, parameter, and alias names;
- only the universal descriptor, registered platform identities, punctuation,
  and format-required ASCII remain non-Chinese; and
- a paired `en@1` file retains the same Chinese project-owned identifiers, so
  canonical keyword and imported-declaration equivalence can be tested without
  conflating translation of project names.

The five new content artifacts total 4,775 bytes. Their 602-byte index has
SHA-256
`2d8883100fefbb6a5cb3b3387f4aab48eacc88bc5f7b609c24245ca96e5fb044`.

## Bundle contents

| Item | Purpose |
| --- | --- |
| [`Reference-Artifacts/`](Reference-Artifacts/) | Exact draft lexicon, vocabulary profile, complete Foundation catalog, composite profile, lock, sizes, and hashes. |
| [`Source/`](Source/) | Paired `en@1` and `zh-Hans@1` source with identical project-owned identities. |
| [Terminology review](Terminology-Review.md) | All 66 keyword decisions and 16 public-label decisions. |
| [Equivalence oracle](Equivalence-Oracle.md) | Exact lowering boundary and future executable comparisons. |
| [Accepted cases](Accepted-Cases.md) | Structural, lexical, catalog, source, and review-state success cases. |
| [Rejected cases](Rejected-Cases.md) | Chinese-specific ambiguity, fallback, collision, review, and equivalence failures. |
| [Native-review checklist](Native-Review-Checklist.md) | Required reviewer evidence and status transition. |
| [Review findings](Review-Findings.md) | Proposed design conclusions and remaining blockers. |

## Exact dependency graph

~~~text
Source-Inputs.wvlock
  +-- en@1
  |   +-- Workload 1 English profile and Foundation catalog
  |
  +-- zh-Hans@1
      +-- Workload 1 Unicode-17-Source.wvup
      +-- Workload 1 Language-1-Keyword-Tokens.wvktr
      +-- Zh-Hans-Keywords.wvlex
      +-- Zh-Hans-Vocabulary.wvsvp
      +-- Zh-Hans-Foundation-Option.wvcat
~~~

The lock and profiles bind exact content hashes, not relative paths. The diagram
shows where the review copies live; a future build resolves bytes through an
approved content store.

## What “fully Chinese” means

The Chinese source body does not contain English keywords or canonical English
Foundation labels. Windvale deliberately keeps these items universal:

- `#!wv/1 zh-Hans@1`, because the byte-zero descriptor is language-neutral;
- `windows`, `linux`, and `windvale`, because platform IDs are registered
  machine identities;
- fixed-width numeric words such as `u64` when a program needs them; and
- punctuation and operators.

Those universal forms are not fallback. Every localizable keyword and imported
public label in the example comes from the selected exact Chinese profile.

## Native-review boundary

The draft uses established Simplified Chinese software terms where they fit,
then adapts terms to Windvale's exact semantics. For example, `async` uses
`异步`, `task` uses `任务`, and `enum` uses `枚举`; ownership, structured-task,
and source-profile terms require more Windvale-specific judgment.

AI authorship can prepare a complete and internally consistent candidate, but it
cannot certify whether every spelling is natural to a native Chinese programmer.
The pack remains `draft` until the named checklist is completed. Reviewers may
change any spelling; each change creates new artifact hashes and must rerun the
collision and equivalence checks.

## Language-specific qualification rule

The first-author paper/design findings are accepted, but `zh-Hans@1` language-
specific review completes only when:

1. every keyword and catalog label has a recorded native disposition;
2. the final terminology has no exact, keyword, namespace, or confusable
   collision;
3. the paired source remains readable and mechanically equivalent after
   canonical lowering;
4. exact artifact hashes are regenerated after the final review edit; and
5. the project owner accepts the reviewed findings.

Until then, these exact bytes are review inputs rather than the official
`zh-Hans@1` release profile.
