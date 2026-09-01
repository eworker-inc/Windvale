# Windvale documentation policy

> Status: Current repository documentation policy
> Authority: Normative for repository-maintained documentation
> Last reviewed: 2026-08-31

Windvale keeps detailed technical history without making every historical fact
part of the normal development context. A document should answer one kind of
question and point to the owner of any deeper detail.

## Documentation layers

### Active guidance

Active guidance is the small reading set used for ordinary development:

- the root README for the public overview and working entry points;
- [Progress](Project/Progress.md) for what works, what is missing, and what is
  being built now;
- [Roadmap](Project/Roadmap.md) for forward product gates and dependencies;
- architecture documents for durable ownership and design boundaries;
- specifications for exact current contracts; and
- runbooks for commands and operational procedures.

Active guidance must be concise, current, and written in day-to-day language.
It should link to detailed evidence instead of reproducing it.

### Current contracts

Specifications define exact behavior, formats, limits, and failure rules.
Architecture documents explain durable ownership and why components are
separated. Exact normative terms, field sizes, status codes, and security rules
belong here when implementations depend on them.

### Historical records

Decisions preserve rationale. Evidence records preserve exact completed runs,
artifact identities, hosts, and measurements. Superseded plans and dated
snapshots preserve context. These records remain searchable and linkable, but
they are not default current-state guidance.

Historical records must not be silently rewritten to describe current behavior.
Correct a factual or link error in place when needed, and use current documents
to explain what later work superseded.

## Required metadata

Every newly created or materially revised active document under `Documents/`
starts with these fields. Backfill unchanged active documents when they next
receive a substantive edit:

```text
> Status: Current, Proposed, Superseded, or Historical
> Authority: Normative, Informative, or Evidence
> Last reviewed: YYYY-MM-DD
```

Use a more specific status description after the category when useful. A
proposal must never be presented as accepted or implemented behavior.

New numbered decisions must include an explicit `## Status` section or a
`- Status:` field near the title. Existing decisions are backfilled only when
they are already being changed for a substantive reason.

## Plain-language structure

Lead with the practical outcome. For a current project or architecture page,
prefer this order:

1. what works now;
2. what does not work yet;
3. what will be built next;
4. how a developer verifies it; and
5. the deeper mechanism or evidence links.

Explain a necessary technical term the first time it appears. Keep exact
contract language where precision matters, but avoid long noun chains, status
chronologies, and artifact inventories in overview paragraphs.

The active navigation pages have word budgets so they remain useful as human
and AI context:

| Document | Maximum words |
| --- | ---: |
| `Documents/README.md` | 2,000 |
| `Documents/Project/Progress.md` | 2,500 |
| `Documents/Project/Roadmap.md` | 3,500 |

When a page reaches its budget, move historical detail to a dated historical or
evidence record rather than compressing it into denser sentences.

## Hash ownership

Windvale needs strong hash verification, not repeated hash strings in prose.
Use one authoritative, machine-readable home for each current artifact identity.

Keep complete hashes in:

- artifact and package manifests consumed by tools;
- launchers that enforce a pinned tool identity;
- signed release checksums and release envelopes;
- deterministic golden fixtures and conformance vectors; and
- evidence records that identify an exact completed run.

Current overview, Progress, Roadmap, architecture introduction, and development
runbook pages should name and link the evidence record instead of copying a
64-character digest. A decision includes a complete hash only when the exact
artifact identity is itself part of the decision. A specification includes
complete hashes only for a digest format, protocol value, or test vector.

A digest proves byte identity. It does not by itself prove correctness, safety,
authorship, or independent trust. A digest stored beside a mutable artifact is
useful for drift detection and reproducibility; signed or independently pinned
evidence is required when the claim needs a stronger trust root.

## Decisions

Decision links should include the title or an unambiguous description, not only
the number. Twelve early identifier collisions are preserved in
[`Legacy-Id-Collisions.txt`](Decisions/Legacy-Id-Collisions.txt) because
renumbering published history would damage existing references. New collisions
are prohibited.

Use a numbered decision only for a durable semantic or format change, public
capability or ABI contract, security or authority boundary, bootstrap or
recovery policy, qualification-model change, or another choice that would be
difficult to reverse silently. Routine checkpoints, artifact refreshes, and
measurements belong in code, specifications, evidence, the changelog, or
Progress.

## Verification

`Tools/Verify/Verify-Documentation.ps1` checks maintained Markdown links,
active-document metadata and word budgets, hash-free active narrative pages,
new decision status, and the frozen list of legacy decision-number collisions.
`Tools/Verify/Verify-Changed.ps1` runs it whenever Markdown or documentation
policy files change.
