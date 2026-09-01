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

New specifications must include a `## Status` section near the title. The
status says whether the contract is current, a candidate, proposed,
experimental, superseded, or historical. The generated specification catalog
keeps older files without status visible as `Unclassified`; that label means
"read the document before relying on it," not "accepted."

Active metadata has a review window. Progress is reviewed at least every 45
days, Roadmap every 90 days, and the other active navigation, architecture, and
runbook entry points every 180 days. A review may confirm that no prose change
is needed, but its date must move only after someone actually checks the page
against its owners.

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

Long or commonly used specifications need a short at-a-glance introduction. Put
it in the specification only when that file is not an identity-bound or frozen
contract; otherwise put it in the generated specification index so an editorial
improvement does not change semantic bytes. It answers:

- what this specification controls;
- which developers or components normally use it;
- its current status or format version;
- what it deliberately does not guarantee; and
- where its behavior is verified.

This introduction is a reading aid, not a second normative contract. Exact
rules remain in the body of the specification.

## Causal verification

A check is useful only when a failure could expose a defect introduced by the
change. State that connection before starting a long verifier. A file path can
suggest an owner, but it is not enough by itself: changing a generated index is
not a bytecode change, and changing an implementation does not become safe
because a prose check passes.

If automatic routing selects unrelated or disproportionate work, stop and fix
the routing, narrow the change, or move editorial text to its proper guide. Do
not keep a long check running to justify time already spent. Preserve completed
results, but never report an interrupted or irrelevant suite as required
evidence for the change.

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

The generated [decision catalog](Decisions/README.md) is the entry point for
status and recent history. The machine-readable catalog uses a full filename
key, because a decision number alone is not unique for the twelve preserved
legacy collisions.

## Catalogs and generated indexes

`Tools/Documentation/Update-Documentation-Catalogs.ps1` owns the specification
domain indexes and the decision catalog. Run it after adding, renaming, or
changing the title or opening status of a specification or decision. Generated
files say that they are generated and must not be edited by hand.

Catalog status categories are search aids. The exact opening status copied into
the machine-readable catalog remains authoritative. In particular,
`Implemented`, `Verified`, `Qualified`, `Accepted`, and `Released` are not
interchangeable claims.

## Evidence records

New exact run and artifact evidence belongs under
[`Documents/Evidence/`](Evidence/README.md). Prefer one small record per claim
or coherent verification run, with links to the specification and decision it
supports. Keep large append-only historical evidence where it is; migrate it
only when a task needs to change its ownership.

The evidence schema records the subject, claim, exact source state, host and
tool identity, inputs, outputs, result, limitations, and review class. It does
not treat a passing command, an artifact hash, AI review, human inspection, or
independent reproduction as substitutes for one another.

## Verification

`Tools/Verify/Verify-Documentation.ps1` checks maintained Markdown links and
anchors, exact path casing, active-document metadata, review windows and word
budgets, hash-free active narrative pages, generated catalog freshness, new
decision and specification status, and the frozen list of legacy
decision-number collisions. `Tools/Verify/Verify-Changed.ps1` runs it whenever
Markdown or documentation-policy files change.
