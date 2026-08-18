# Localization workload 2 draft findings

## Status

These first-author technical and design findings are accepted by the project
owner. They are not native-reviewed terminology conclusions and cannot complete
Workload 2 until the native checklist and mechanical rerun are accepted.

## Finding 1: the entire mapped keyword surface can be Chinese

All 66 mapped tokens have distinct candidate Simplified Chinese spellings under
the existing one-segment lexicon contract. Numeric widths, registered platform
IDs, punctuation, and the language-neutral descriptor remain universal without
preventing a Chinese program body.

## Finding 2: the compiler still needs only canonical tokens

The Chinese spellings disappear at lexing. No parser production, AST rule, type,
ownership rule, effect, capability, WIR operation, WVB instruction, backend, or
runtime branch needs a Chinese-specific form.

## Finding 3: project-owned Unicode names and localized APIs compose

One file can use Chinese project module/function/parameter/alias names and also
consume an English-authored library through a Chinese catalog. Those are two
independent mechanisms: project names are their own exact identities; imported
labels resolve to the library's canonical identities.

## Finding 4: canonical equivalence needs identical project names

The paired source deliberately keeps Chinese project-owned identifiers in both
profiles. This isolates keyword/public-label localization and permits exact
semantic comparison. A separate project-name translation is allowed only when
explicitly requested and cannot be part of the automatic profile converter.

## Finding 5: keyword and public-label vocabularies need a joint collision audit

Chinese has no casing distinction comparable to English `borrow` versus
`Borrow`. The operation therefore uses `获取ˉ借用` while the keyword uses `借用`.
Each official catalog must be tested against the selected lexicon, not only
against other catalog rows.

## Finding 6: `zh-Hans` names a terminology contract, not a host locale

The descriptor explicitly selects `zh-Hans@1`; host UI locale never selects or
changes source semantics. Native review must still decide whether one
Simplified-Chinese terminology contract is broad enough or a later regional
profile is necessary.

## Finding 7: machine identities should remain visible and universal

Registered values such as `windows`, `linux`, and `windvale` are not English
fallback. Translating them would create aliases for machine contracts and make
cross-profile inspection harder.

## Finding 8: exact review attaches to bytes and hashes

A reviewer approves the `.wvlex` and `.wvcat` content identified by the artifact
index, not a detached prose list. Any terminology change invalidates dependent
hashes and review evidence until regeneration and revalidation finish.

## Finding 9: AI drafting and official language support are different states

AI can produce a complete, consistent candidate and expose likely tradeoffs. It
cannot self-certify native naturalness, regional suitability, or official
support. Draft, reviewed, qualified, and officially distributed remain separate
states.

## Finding 10: the pack-size cost is small but qualification is not free

The five new content artifacts total 4,775 bytes and reuse shared Unicode/token
inputs. Runtime products need none of those bytes. The substantial cost is
review, editor/conversion quality, security testing, cross-host equivalence, and
maintenance when public interfaces change.

## Quantitative record

| Evidence | Value |
| --- | ---: |
| New content artifacts | 5 |
| New content-artifact bytes | 4,775 |
| Artifact index bytes / SHA-256 | 602 / `2d888310…5fb044` |
| Keyword rows | 66 |
| Localized `Foundationˉoption` labels | 16 |
| Paired source files | 2 |
| Accepted cases | 27 |
| Rejected/boundary cases | 28 |
| Native approvals | 0 |
| Current implementation evidence | None |

## Accepted disposition

Send the exact terminology and source packet to named native reviewers. The
structural mechanism is owner accepted; keep every Chinese spelling and the
overall Workload 2 result in draft state until reviewed hashes and dispositions
return.
