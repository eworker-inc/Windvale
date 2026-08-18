# Decision 0767: Freeze Windvale Language 1.0 source

- Status: Accepted
- Date: 2026-08-18

## Context

Decisions 0751 through 0765 accept the complete Language 1.0 direction, eleven
application/system workload findings, machine grammar, Foundation registry, and
pre-localization reconciliation. Decision 0766 accepts the five localized-source
workloads and reconciles the universal descriptor, exact source profiles and
catalogs, Unicode 17 security, conversion/tooling, shipment/cache, cross-host,
and performance-measurement contracts.

The resulting immutable candidate is
`Documents/Project/Windvale-Language-1.0-Replacement-Source-Freeze-Candidate.txt`:

- manifest byte length: `3702`;
- manifest SHA-256:
  `c9517841eae6b6e86778cb1dd88711feb38929dec8fe79e084eec44fa22c512a`;
- identity-input count: `250`;
- identity-input bytes: `1724854`; and
- aggregate candidate SHA-256:
  `fb918a763ae7c8c85dd1a2ffecee6587ab93bbf846ae31ae19b53509aed36a0a`.

The manifest recomputes from exact repository bytes, its grammar projection has
143 human definitions, 155 machine definitions, 12 explicitly external scanner
definitions, and no missing or undefined reference, and all eleven Foundation
signature-set hashes recompute. All five localization packets and all eleven
application/system packets have owner-accepted paper findings.

## Decision

Freeze the exact manifest identity above as Windvale Language 1.0 source.

1. The manifest's 250 selected inputs are the frozen source-design identity.
   This promotion decision intentionally remains outside that identity so it
   can cite the manifest without a self-referential hash.
2. The semantic specification, grammar and machine projection, localized-source
   addendum, seven source-profile artifact formats, Foundation contract and
   registry, naming rules, design/migration contracts, decisions 0751 through
   0766, eleven-workload paper corpus, and five-workload localization corpus are
   accepted together. No one file can redefine another owner's rule.
3. Authorize Migration Slice 0, followed by the ordered vertical slices in the
   frozen migration plan. Implementation stays in the existing Windvale compiler
   and does not create a parallel edition-1 compiler architecture.
4. Windvale Seed remains the only implemented source contract until a named
   migration gate proves the corresponding edition-1 surface. A partial compiler
   must report the exact supported edition/profile/features and cannot claim
   complete Language 1.0 conformance.
5. The exact `en@1` reference profile is the minimal canonical development
   path. The current `zh-Hans@1` bytes remain a draft mechanism fixture. They do
   not become native-reviewed, qualified, or officially distributed through
   this freeze.
6. First-implementation qualification must execute the frozen accepted/rejected
   cases, establish Windows/Linux performance and memory ceilings, and preserve
   byte-identical portable semantics. Those results qualify an implementation;
   they do not retroactively become the source-design identity.
7. A later semantic change requires a named decision, updated affected
   specifications/evidence, and a new exact manifest identity. Implementation
   may not introduce undocumented aliases or target-dependent semantics to work
   around a frozen rule.

## Non-decision

This decision does not claim that the current compiler, Foundation library,
runtime, editor, formatter, installer, backend, operating system, Windows host,
or Linux host implements or qualifies Language 1.0. It does not promote any
optional natural-language pack, select production capability-provider
identities, add a new WVB version, or require a second compiler.

The manifest file retains its historical
`candidate-not-frozen-not-implemented` bytes because those exact bytes are what
this external decision freezes. Editing that status line would create a
different, unfrozen manifest.

## Consequences

Language design is no longer an open-ended prerequisite for implementation.
Compiler, library, runtime, editor, package, and OS work can now advance through
the ordered migration slices against one exact contract. Paper cases become
executable conformance fixtures; implementation findings may trigger explicit
reconsideration, but cannot silently alter the language.

The project must keep three states distinct in progress reporting:

- **frozen source design**: achieved by this decision;
- **implemented feature slice**: achieved only by focused executable evidence;
  and
- **complete Language 1.0 conformance**: achieved only after every required
  migration and permanent-host gate passes.

## Reconsideration triggers

Reconsider a frozen rule only when implementation proves a contradiction,
unimplementable bound, unsafe behavior, irreconcilable target difference, or
material usability failure in the accepted workloads. The reconsideration must
name the exact affected identity and preserve a simple oracle for the old and
proposed behavior.
