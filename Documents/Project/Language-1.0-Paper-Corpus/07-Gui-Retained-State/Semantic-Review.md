# Workload 7 semantic review

## Part and authority boundary

Six Core modules own package bytes, parsing, values, retained state, layout, and
rendering. Two Hosted modules own task/provider types and orchestration. Only the
Hosted application declares `display.surface`, `input.event_batch`, and
`timer.tick` version 1.

Core code cannot observe endpoints, operation contexts, provider generations,
tasks, wall time, native windows, or host event loops. Hosted code does not gain
filesystem, network, process, GPU, clipboard, font, global desktop, raw input, or
thread authority merely because it can publish one surface and receive its one
semantic input/timer stream.

## Value and ownership inventory

| Value | Class | Owner and transfer |
| --- | --- | --- |
| Theme package bytes | Shared immutable | Package resource domain; parsed once without a second payload. |
| Colors, rectangles, widgets, events, limits, handles, snapshots, layout results, receipts | Copy aggregates | Explicit lexical values; copied snapshot/scalars are the only child captures. |
| Root and five child budgets | Move-owned resources | Launcher transfers root; application splits arena, map, two frame, and task children. |
| Retained state | Move-owned aggregate | Parent event path exclusively owns arena, identity map, handles, scalar state, and closure state. |
| Arena nodes | Arena-owned | Four inserted; status removal returns its Copy node and advances the slot generation. |
| Identity map | Map-owned | Four stable logical identities; status mapping becomes a revalidated tombstone. |
| Package/event/frame bytes | Shared immutable | May share backing; no mutable byte alias exists after publication. |
| Task scope and handle | Move-owned scoped resources | One child; handle consumed once; no detach. |
| Layout child closure | Move-owned until spawn acceptance | Explicitly copies snapshot and two maxima; never borrows retained state. |
| GUI endpoints/context | Copy generation-bound references / lifetime-bound context | Launcher endpoints and parent context; scope derives child context for all in-scope provider calls. |

No record stores a source borrow. No child, provider, receipt, frame, or event
sequence contains a mutable reference to retained state. A scheduler may run the
child in parallel because it owns a snapshot copy while the parent owns the only
mutable state path.

## Retained-state invariants

The arena and identity map are distinct owners with one invariant:

```text
every live logical identity maps to a handle that validates in Widgets;
removed logical identities may remain only as explicit stale tombstones;
no logical identity is reused during one Retainedˉstate lifetime.
```

Every identity read first finds the deterministic map rank, copies the handle,
then validates it against the arena before borrowing. `Handle<T>` equality or map
presence alone never proves liveness.

Initial construction publishes neither collection until four arena inserts and
four unique map entries succeed. Map insertion returns the original key/handle
on duplicate/capacity failure. Arena insertion returns the proposed widget on
failure. Reverse local cleanup releases any earlier owner.

Status removal validates its handle before taking the option. The uninterrupted
exclusive state borrow then makes arena removal infallible under the validated
state. The old map entry intentionally becomes stale evidence. It cannot alias a
future insertion because removal advances or retires the slot generation.

## Atomic layout application

A layout result carries the snapshot's base layout generation. Application:

1. compares it to the current state generation;
2. checks status-presence agreement;
3. validates every handle that will be changed under the same exclusive state
   borrow; and only then
4. replaces all corresponding Copy widget values.

A stale generation/handle or shape mismatch therefore returns before the first
replacement. `Arenaˉreplace` itself returns the old owned node on success and the
proposed node on failure, so generic ownership is explicit. Replacement keeps
the handle generation stable; removal is the operation that advances it.

No transaction syntax, rollback log, object identity, class field mutation, or
tracing-GC graph is required. The exclusive aggregate borrow plus complete
prevalidation is the atomicity boundary.

## Event semantics

`Event` is a closed variant, not a kind enum plus unrelated optional fields.
Each case carries only its meaningful payload. One event is applied completely
on the owning path before the next begins:

- pointer press reads action identity 3 through map+arena validation and may
  increment the counter;
- resize validates dimensions before changing dimensions/generation;
- request-layout changes no retained value by itself;
- remove-status validates then removes the one node/takes the optional handle;
  and
- close marks the state closed.

Only accepted events increment `Eventˉsequence`. Invalid resize, stale state, or
collection failure leaves that sequence unchanged. Bounded event/batch maxima
make all accepted counter and generation increments provably far below u64
overflow in this workload.

## Task and stale-result semantics

The child receives one copied generation-1 snapshot. Timer/input calls execute
through the scope-derived operation context while the parent mutates state.
Await consumes the one handle. A valid child result still requires generation
and handle revalidation on the owning path; task success is not permission to
commit stale state.

Typed child failure, cancellation, deadline, task-runtime loss/restart, and
contained trap remain separate. GUI provider failure remains in the input,
timer, or surface domain and never masquerades as task-runtime failure. Any
early return inside the scope applies cancel-and-join before releasing state or
provider accounting.

## Render publication

Render validates surface bounds, computes u32-product pixel count in u64,
checks work before allocation, checks `Pixels * 4` before multiplication, and
constructs one exact-capacity byte builder. Each pixel appends exactly RGBA order
and the builder freezes into immutable bytes.

The initial/final frames retain state event sequence and layout generation.
Publishing consumes the frame argument into one asynchronous provider call.
Rejected publication proves zero progress and returns the frame; the adapter
releases it and returns a small normalized failure. Indeterminate publication
returns no replayable frame. Success proves complete local-provider acceptance,
not scanout or human observation.

## Bounds

| Dimension | Reference | Hard paper ceiling |
| --- | ---: | ---: |
| live widgets / identities | 4→3 / 4 | 64 / 64 |
| events / batches | 5 / 1 | 64 / 16 |
| task children / retained completions | 1 / 1 | 8 / 8 |
| initial pixels/bytes | 1,920 / 7,680 | within 65,536 / 262,144 |
| final pixels/bytes | 3,072 / 12,288 | within 65,536 / 262,144 |
| application memory budget | 73,728 | exact reference |
| provider frames / timer completions / event batches | 2 / 1 / 1 | 2 / 16 / 16 for this run profile |
| runtime diagnostics | 16 | admitted task/provider maxima |

No collection, task graph, batch, event loop, render loop, diagnostic stream,
retry, recursion path, or retained frame list is unbounded.

## Failure-domain table

| Domain | Typed evidence | Mutation guarantee |
| --- | --- | --- |
| Package/theme | length/digest/magic/separator/hex failures | before state/provider work |
| Configuration/memory | exact field/bound or allocation evidence | before affected owner publication |
| Map/arena | duplicate/missing/wrong arena/range/vacant/stale/retired/capacity | named collection unchanged unless success says otherwise |
| Layout | stale snapshot/widget or status mismatch | before first widget replacement |
| Event | invalid surface, collection failure, after-close, batch count/sequence | current invalid event not applied; prior accepted events remain |
| Task | spawn, typed child failure, cancel, deadline, runtime generations, trap | scope joins before exit |
| GUI provider | domain, reason, expected/observed generation | operation-specific zero/complete/indeterminate meaning |
| Frame | work/byte/arithmetic/allocation/state failure | no partial immutable frame |

There is no catchable general exception or implicit failure conversion. Each
cross-domain conversion is a named source function.

## Common corpus questions

| Question | Finding |
| --- | --- |
| Is every mutation visible? | Yes: exclusive state/arena/map/builder borrows and explicit scalar assignments. |
| Are allocation and retained state bounded? | Yes: five child budgets, collection maxima, two exact frames, and provider ceilings. |
| Are capabilities minimal and explicit? | Yes: display, input, and timer are independent requirements/endpoints/effects. |
| Can child work race mutable UI state? | No: it receives a copied snapshot; only parent applies results. |
| Can a stale handle alias a new widget? | No: arena identity/slot/generation validation and retirement prevent it. |
| Is frame publication immutable? | Yes: builder freeze precedes the provider call; no mutable pixel alias remains. |
| Is provider restart automatic? | No: exact failure, no endpoint rebind, refresh, or replay. |
| Does readable source require classes or GC cycles? | No: one owned state aggregate, typed arena handles, closed events, and lexical tasks suffice. |

## Paper standing

The source proves the existing language direction is sufficient. The general
Foundation gap is the already-described but previously unsigned arena mutation
surface: exact replace and remove operations with ownership-return and
generation rules. The GUI endpoint signatures, immutable publication outcomes,
stable identity-tombstone policy, and Core/Hosted split are library/package
contracts, not new grammar or WIR opcodes.
