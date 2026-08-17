# Workload 6 review findings

## Status

All seven findings were accepted by the project owner on 2026-08-17 under
[Decision 0760](../../../Decisions/0760-Resolve-Language-1.0-Concurrent-Service-Findings.md).
They are normative-candidate source-freeze inputs, not implementation or final
source-freeze claims.

## Pressure matrix

| Required pressure | Evidence | Status |
| --- | --- | --- |
| Explicit effects/captures | Every child names four capture modes and complete finite effects. | Pass |
| Copy/move/borrow rules | Shared endpoint/context copy, budget move, mutable outer capture rejection. | Pass; awaited-borrow clarification accepted |
| Bounded task resources | Child/runnable/completion/memory/work/depth/timer/diagnostic limits are explicit. | Pass; construction failure completed |
| Typed handles/await | Five monomorphic handles are consumed once. | Pass |
| Deterministic join | Explicit await order is child creation order. | Pass |
| Cancellation | One named idempotent request reaches scope-derived contexts. | Pass; candidate addition accepted |
| Trap separation | Runtime trap identity is distinct from typed child failure. | Pass |
| Provider restart | Old outcomes retained, exact one-generation refresh, fresh work only. | Pass; endpoint/refresh completion accepted |

## Finding 1: task construction must consume a parent operation context

Before Decision 0760, the candidate prose said task construction consumed optional
deadline/cancellation providers, but its accepted signature accepts only budget
and limits. That cannot connect workload 5's opaque operation context to child
cancellation.

Recommend requiring `Parentˉcontext` in `Task.Construct`, returning a typed
`Taskˉscopeˉfailure` for invalid limits, allocation, stale parent context, or
unavailable task runtime. Accept `Task.Operationˉcontext(Scope)` as a Copy,
scope-derived, non-forgeable child view. Its deadline cannot be later than the
parent; scope teardown invalidates its cancellation generation.

## Finding 2: cancellation needs one explicit idempotent request

Exit policies are insufficient when source must react to an observed child and
then collect remaining outcomes. Recommend:

```text
Task.Requestˉcancel(
    Scope: borrow mut Taskˉscope,
) -> Cancelˉrequestˉoutcome effects(task.cancel)
```

The first request closes spawn and marks every live/future observation view;
later calls report already requested. It is cooperative, never an asynchronous
exception, and never replaces join.

## Finding 3: task-runtime failures need exact, unambiguous generations

The payload-free `Taskˉoutcome.Providerˉlost` is ambiguous with a child's
service-provider failure and loses evidence. Recommend replacing it with
`Runtimeˉlost(Expectedˉgeneration, Observedˉgeneration)` and
`Runtimeˉrestarted(Expectedˉgeneration, Observedˉgeneration)`. Service-provider
loss/restart remains inside the child error type `E`.

## Finding 4: provider operations should be explicitly asynchronous

The hosted service must let other bounded children run while accept/read/write
wait. Hiding suspension beneath a synchronous source call makes effects,
continuation ownership, cancellation points, and borrow diagnostics misleading.

Recommend making semantic `Acceptˉone`, `Read`, `Write`, and `Refresh` async and
requiring `await`/`task.suspend`. Amend workload 5's `Run`, receive, and write
functions accordingly. Preserve every accepted exact-progress, deadline,
generation, and no-replay rule from Decision 0759.

## Finding 5: shared accepts require a generation-bound endpoint value

Four children cannot safely share one move-owned listener, and a bare static
capability call hides which provider generation they used. Recommend a Copy
`Serviceˉendpoint` only for an interface that explicitly permits concurrent
shared accepts. It binds service identity, exact rights/limits, provider
identity, and generation. Workload 5 should accept that endpoint explicitly.

Refresh accepts the stale endpoint and exact observed successor generation and
may return only the same approved authority at that generation. It is not
discovery, automatic retry, or a grant.

## Finding 6: valid awaited provider borrows and invalid task captures differ

Do not impose a blanket “no mutable borrow across await” rule. A temporary
exclusive argument into an awaited provider operation is valid when its owner
lives in that same child continuation and no alias can execute. Reject storing
the borrow, returning it, or capturing an outer mutable borrow into a spawned
child. This keeps async buffer/stream I/O expressible without weakening
ownership.

## Finding 7: fixed typed awaits are enough for version 1

The workload can prove deterministic creation-order collection with five named,
homogeneous handles. Do not add detach, a general task vector, completion-order
stream, select/race syntax, channels, actors, or thread primitives for Language
1.0. Later measured workloads may add a bounded library abstraction over the
same handles and scope contract.

## Owner resolution

The project owner accepted all seven findings together because context
derivation, cancellation, async provider suspension, and generation refresh are
one safety boundary. Workloads 5 and 11 and the candidate
language/grammar/Foundation documents now carry the coherent contract. Workload
6 is draft reviewed. No current compiler, runtime, capability identity, or
source-freeze implementation claim follows.
