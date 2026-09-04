# Decision 0947: treat complete qualification as one evidence graph

- Date: 2026-09-04
- Status: Implemented candidate for inventory and prioritization; owner-level
  claim migration remains in progress
- Extends: [Decision 0557](0557-Separate-Development-Verification-From-Qualification.md)
- Extended by: [Decision 0949](0949-Balance-Qualification-Shards-By-Declared-Cost.md)

## Context

Windvale's native qualification registry now contains 126 independently launched
owners and 5,981 declared cases. Its duration profiles sum to 19,560 expected
seconds per host. With four shards, the declared expected critical path is still
7,545 seconds. Ten owners account for 68.71 percent of the expected work.

Owner isolation is useful for diagnosis and resumption, but it has also made
construction ownership local to scripts. Static inspection finds common
compiler, lowerer, linker, verifier, and packager entry points repeated across
many owners. Adding another behavioral test can therefore add another complete
toolchain pipeline even when an existing immutable product could serve it.

Removing tests solely because they are slow would weaken qualification. Keeping
every historical wrapper solely because it once supplied evidence makes the
system eventually unusable. The unit that must be preserved is the distinct
failure signal and its bound evidence, not necessarily a process, script, or
separate construction.

## Decision

1. Complete Windows/Linux qualification is modeled as one versioned evidence
   graph. Owners remain diagnostic and scheduling boundaries, but construction,
   admission, behavior, platform, reproducibility, and coverage are separate
   node kinds with explicit dependencies.
2. Every retained test or owner must identify the contract it protects and the
   distinct failure signal it can reveal. A case count, historical existence, or
   broad file match is not sufficient ownership.
3. Tests with the same profile, authority, immutable input product, host
   boundary, resource limits, and isolation needs should share construction and
   admission nodes. Their logical case names and diagnostics remain distinct.
4. A test is removed only when its failure signal is proved to be fully covered
   by another retained node or when it protects an obsolete contract. Coverage
   validation must fail closed if the removed claim has no replacement.
5. An owner is merged when it merely replays another owner's construction and
   adds no independent behavior, host, malformed-input, recovery, security, or
   reproducibility boundary. A merged owner remains represented as claims in the
   graph rather than disappearing from audit history.
6. Mutable execution, crash recovery, revocation, races, and hostile-input
   behavior always rerun with fresh bounded state. Only immutable products and
   admissions may be shared or restored from a complete content identity.
7. The read-only qualification planner reports total and critical-path declared
   cost, shard balance, long-owner concentration, repeated project ownership,
   nested owner calls, and common pipeline call sites without launching tests.
8. Optimization proceeds by measured critical-path contribution, starting with
   the ten owners at or above 900 declared seconds. Database work continues, but
   shard 2 and `language-1-authenticated-foreign-binding` are higher aggregate
   critical-path priorities than small quick owners.
9. Duration profiles are scheduling bounds, not performance evidence. They are
   replaced with measured owner/node history only after the input, host, source
   state, tool identities, elapsed time, and memory context are recorded.
10. Complete qualification is published only when coverage proves every required
    claim and both host result sets bind the same selected source state. A fast
    development result or partial shard cannot be promoted.

## Initial evidence

`Tools/Verify/Plan-Qualification-Work.mjs` reads the canonical owner and duration
registries, validates both host entry points, and inspects a same-named
JavaScript orchestration module when one exists. Its initial snapshot reports:

- 126 owners and 5,981 cases;
- 19,560 expected and 79,200 maximum owner-seconds per host;
- a 7,545-second expected and 27,300-second maximum declared critical path;
- 13,440 expected seconds in ten long owners;
- 277 analyzed wrapper/orchestration files with 126 owner analysis rows;
- 221 `Build-Wvb` script call sites across 48 owners;
- 124 `Link-Wvo` call sites across 39 owners;
- 111 `Package-Hosted-Wvb` call sites across 19 owners; and
- no direct registered-owner-to-owner invocation edges.

These static counts locate candidates. They do not by themselves authorize a
merge or prove that two invocations have the same complete inputs.

Decision 0949 subsequently rebalanced only the existing shard assignments. The
current declared expected critical path is 4,890 seconds with zero expected
spread; no owner or case was removed. The initial snapshot above remains the
before-state for that scheduling change.

## Consequences

- The redesign covers all qualification areas rather than treating database
  storage as a special one-off optimization.
- Review can see which owners dominate the critical path before an hours-long
  run begins.
- Adding behavioral coverage should usually add one graph claim and execution,
  not a private compiler-to-package pipeline.
- Some current scripts will remain temporarily while their claims and inputs are
  migrated. Temporary dual representation cannot make duplicate execution part
  of the final design.

## Reconsideration triggers

Reconsider the graph boundary if sharing immutable products obscures source or
tool provenance, changes diagnostics nondeterministically, crosses a capability
or host boundary, or makes one failing behavior contaminate another's state.
Retain a separate owner whenever that separation is required for correctness,
security, bounded resources, recovery isolation, or independently resumable
evidence.
