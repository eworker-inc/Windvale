# Decision 0743: Prehash project-cache producer context

- Date: 2026-08-17
- Status: Implemented with Windows development evidence; independent Linux execution pending
- Advances: [Decision 0737](0737-Batch-Os-X64-Project-Wvb-Development-Checkpoints.md), [Decision 0738](0738-Reuse-Project-Object-Checkpoint-Admission.md), and [Decision 0740](0740-Reuse-Hosted-Application-Producer-Trust.md)
- Contract: [Windvale native tool checkpoint 1](../../Specifications/Windvale-Native-Tool-Checkpoint.md)

## Context

After unifying linked-image checkpoints, the all-hit Windows database owner took
85,010 ms. A representative TreeNode case still spent 220 through 240 ms in
project-object materialization, even though its immutable WVB and WVO were
already admitted. Ten empty Node invocations averaged 54.4 ms. The old
standalone project-key command averaged 124.9 ms because every request reopened
the workspace, build driver, lowerer, and checkpoint driver before hashing the
project closure.

The OS x64 project-WVB batch already derived 56 keys in one process but repeated
the same inventory and build-driver hashing for every project. Simply retaining
whole producer buffers would reduce I/O while increasing the owner session by
about 37 MiB of working set. Producer trust is common to one bounded owner
invocation; project manifests and source closures remain request-specific.

## Decision

Introduce `windvale-native-project-cache-key 2`. Frame the namespace, workspace,
canonical producer count, and ordered producer bytes before the project identity,
manifest, and ordered root/source closure. Hash the common prefix once and clone
the SHA-256 state for each request. Keep the standalone key command as an adapter
over the same prepare-and-request implementation.

Accept one through 16 producers, no more than 128 MiB in aggregate. Stream each
producer in 1 MiB chunks and retain only its canonical path, size, and digest as
evidence. Accept at most 1,024 project inputs, no more than 256 MiB in aggregate,
with the existing 64 MiB per-file limit. Reject a producer that grows, shrinks,
escapes its canonical path, or exceeds an aggregate bound.

Extend the existing authenticated, loopback-only hosted-application session with
one serialized, read-only project-object hit operation. It computes each exact
project closure from the cloned producer context, fully validates the immutable
checkpoint, copies both products to private owner paths, and rehashes the copies.
A missing key returns exit 75 without output mutation and the database helper
invokes the standalone publisher. Corruption remains an error and never falls
back. No new daemon or independent session is introduced.

Use one prepared format-2 context across all selected OS x64 project-WVB rows as
well. After a cold project-object or OS project-WVB build, recheck the workspace,
every producer, the project manifest, and every source against the request
evidence before writing the record or atomically renaming the candidate. Retain
the guarded `finally` cleanup for the exact locally allocated `.new-*` directory;
a successful rename preserves the published checkpoint and a race loser accepts
only a completely validated winner.

Keep no-argument and qualification owners cache-independent. Format 2 deliberately
selects new entries for `project-wvb-v2`, `database-project-object-v2`, and
`database-segmented-project-v1`; older keys are inert and are neither migrated
nor deleted by repository tools.

## Evidence

An isolated Windows population created one project-object checkpoint in 2,965
ms. Eight following standalone hits averaged 149.0 ms, while eight hits through
the already-running owner session averaged 98.0 ms. The session boundary saves
34.23 percent and is 1.52 times faster. Two representative TreeNode runs took
810 ms, down from the preceding 940 through 960 ms range while retaining the
340 through 350 ms fresh execution boundary.

A hosted-only ready session used 69.84 MiB working set and 80.32 MiB private
memory. A rejected whole-buffer context used 107.45 MiB and 117.61 MiB. The
streaming implementation used 73.78 MiB and 83.14 MiB, only 3.94 MiB working-set
and 2.82 MiB private memory above the hosted-only session.

The bounded project-object regression passes creation, hit, corruption
preservation, failed-producer cleanup, a four-way same-key race with no debris,
four exact serialized session hits, miss preservation, session corruption
rejection, clean lifecycle teardown, a stable prehashed producer snapshot, and
the 16-producer bound. The one-time format-2 Windows population passed all 51
database development steps in 736,610 ms. The OS population created all 56
project-WVB entries and then passed all 336 code-emission cases. The final
change-aware warm database owner passed in 81,910 ms, down from 85,010 ms; its
portable section fell from 58,410 ms to 55,340 ms. The complete owner saves
3.65 percent and its portable section saves 5.26 percent. Independent Linux
runtime evidence remains pending; no cross-host timing claim is made.

## Consequences

Ordinary development no longer reopens or retains the same large producer
closure for each project. Per-request manifests and sources are still read and
hashed, every hit still validates immutable products and private copies, and all
selected applications still execute.

The session represents a startup snapshot. If a producer changes after readiness,
the session can only select entries under the old producer identity; a miss falls
back to the standalone command, which derives the current identity. Restarting
the bounded owner session adopts the new producer context. Cold publication is
slightly stricter because mid-build input mutation now fails before rename.

The one-time key migration is deliberate setup cost. It preserves deterministic
checkpoint bytes and avoids compatibility code for obsolete experimental keys.
The hosted-session module now transports two read-only hit operations; renaming
or extracting a generic development-cache transport is deferred until another
consumer makes that boundary materially clearer.

## Reconsideration triggers

Reconsider this decision if session and standalone format-2 keys differ; a
workspace, producer, project, or source mutation can publish under stale identity;
the context exceeds its documented memory bounds; a miss or corruption changes
an owner output; cleanup can remove anything outside its exact checkpoint family;
a session survives owner teardown; Linux behavior differs; qualification starts
the session; or a native multi-request compiler exposes a more valuable bounded
incremental phase.
