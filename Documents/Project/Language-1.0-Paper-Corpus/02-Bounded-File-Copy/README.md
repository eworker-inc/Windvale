# Language 1.0 workload 2: bounded file copy

## Status

Draft-reviewed paper bundle for workload 2 under the
[Language 1.0 paper corpus](../../Windvale-Language-1.0-Paper-Corpus.md).
[Decision 0756](../../../Decisions/0756-Resolve-Language-1.0-File-Copy-Findings.md)
accepts its general byte-buffer, resource-completion, partial-progress, and
filesystem-authority findings. It is not frozen source, an implemented
application, or a published filesystem capability. Current compilers continue
to accept Windvale Seed.

## Result first

The bundle expresses one bounded regular-file copy without ambient paths,
native handles, hidden allocation, implicit close-as-finish, unsafe buffers,
unbounded retry, or a broad filesystem grant. A launcher supplies:

- one immutable `Configuration` value;
- one owned memory budget;
- a read-only source-directory capability root;
- a create/write destination-directory capability root; and
- one explicit provider cancellation generation shared by those roots.

The application opens an immutable source snapshot, creates a new destination
without replacement, copies through one mutable caller-owned buffer using
explicit offsets, durably finishes content, length, and created-name publication,
and releases both handles on every ordinary path.

The paper profile admits at most 1,048,576 source bytes, 65,536 bytes per chunk,
and 2,097,152 total read/write calls. Each concrete launch may choose smaller
limits. A pathological provider that never makes useful progress terminates at
a typed progress or operation-limit failure.

## Bundle contents

| Item | Owner |
| --- | --- |
| [`Source/`](Source/) | Four complete candidate edition-1 modules for domain values, validation, the bounded transfer engine, and application orchestration. |
| [Package plan](Package-Plan.md) | Exact module graph, launcher profile, capability closure, resource limits, and process-status mapping. |
| [Filesystem contract](Filesystem-Contract.md) | Paper-only source/destination acquisition, read/write progress, durable finish, cancellation, generation, and release semantics. |
| [Semantic review](Semantic-Review.md) | Values, borrows, ownership, effects, failures, cleanup, bounds, and common-corpus answers. |
| [Rejected cases](Rejected-Cases.md) | Compile, launch, acquisition, transfer, finish, cleanup, malformed-provider, and no-retry cases. |
| [Expected outcomes](Expected-Outcomes.md) | Exact successful and failing semantic transcripts independent of backend or host. |
| [Implementation responsibilities](Implementation-Responsibilities.md) | Compiler, Foundation, capability, provider, runtime, launcher, and verifier ownership. |
| [Review findings](Review-Findings.md) | Acceptance matrix, resolved design findings, quantitative record, and review status. |

## Source graph

```text
Copyˉapplication
  -> Copyˉvalidate
       -> Copyˉtypes
  -> Copyˉengine
       -> Copyˉtypes
       -> Foundationˉbytes/resource/result
       -> Platformˉfilesystem
  -> Copyˉtypes
  -> Foundationˉbytes/memory/resource/result
  -> Platformˉfilesystem

Platformˉfilesystem
  -> filesystem.copy.source
  -> filesystem.copy.destination
```

Core validation cannot acquire authority. The Hosted engine and application
declare the exact capability effects they use. Importing either module performs
no registration or provider lookup.

## Successful example

Given a 10-byte immutable source containing:

```text
Windvale!\n
```

and a 4-byte chunk maximum, the deterministic minimum transcript is:

```text
open-source(length=10)
create-destination(maximum-length=10)
read-at(position=0, completed=4)
write-at(position=0, completed=4)
read-at(position=4, completed=4)
write-at(position=4, completed=4)
read-at(position=8, completed=2)
write-at(position=8, completed=2)
finish-durable(expected-length=10, completed-length=10)
release-destination
release-source
```

Providers may return shorter positive reads or `Shortˉacceptance` writes, so a
successful transcript can contain more calls. The content and final length are
identical, positions never repeat accepted bytes, and the operation limit remains
enforced.

## Scenario boundary

This workload proves resource-bearing sequential copy, not a general shell copy
command. It deliberately excludes recursive paths, replacement, metadata,
permissions, timestamps, links, deletion, atomic rename, sparse files, resume,
checksums, concurrent copying, asynchronous tasks, and progress display.

Workload 3 owns transaction/commit composition. Workloads 5 and 6 own
source-visible deadlines, cancellation, asynchronous streams, and concurrent
provider restart. Workload 10 owns unsafe native filesystem adapters.

## Review rule

Review source and evidence together. A reviewer must not treat `using` release as
durable finish, continue after an indeterminate mutation, hide a native path in
`text`, or widen either capability root for convenience. Exact known partial
progress may advance the next position; unknown progress may not.
