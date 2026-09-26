# Decision 0968: thread owned budgets through collection helpers

## Status

Implemented candidate compiler/verifier/interpreter path with focused Windows
and Debian development evidence. This extends execution of the accepted
Foundation budget and collection operations for the typed Package-Lock consumer;
it does not accept new public signatures or promote installed products. The
[stabilization record](../Evidence/2026-09-26-Development-Stabilization.json)
records the 40 record/helper groups, retained borrowing and ownership checks,
and exact products. Independent qualification and consumer integration remain
separate requirements.

## Context

A library parser must construct and return its own typed directory. Requiring
all construction, splitting and growth to occur in exported Main prevents that
consumer. Passing an owned budget into a helper also requires cleanup on early
return, including the case where the helper never allocates. Releasing every
budget when any function returns would instead invalidate caller resources.

The accounting collector from the [preceding checkpoint](../Evidence/2026-09-25-Unreachable-Budget-Accounting.json)
preserves exact live token generations, delays parent reclamation while children
remain live, and validates all roots before changing state. It needs complete
runtime roots before it can safely serve helper returns.

## Decision

Extend candidate WVB 1.42, retaining its existing instruction and shape bytes.
Canonical emission selects it for a reachable helper that accepts an owned
budget or performs Split, Vector reserved construction, append or growth. A
minor-42 module must contain a record collection type or an owned-budget/helper
operation witness. Earlier encoded modules retain their versioned admission.

The complete verifier permits these operations outside Main in this candidate
only. Exact operand types, budget availability, nominal identity, failure
layouts and forward/backedge ownership checks continue to apply. Shape 36
remains an immutable budget view; it is never permission to split or consume
the caller's budget. Direct return of an opaque budget remains unsupported;
an admitted owned Result transfers its budget payload through normal aggregate
ownership.

After aggregate collection at a helper return, gather current budget roots from
remaining operand values, caller locals, live aggregate fields, active task
scopes and allocation leases. The returned aggregate is already a live root of
aggregate collection. A dedicated budget stack flag prevents interpreting scalar
bit patterns as ownership. Lease roots remain until the existing descriptor
release commits, so accounting collection cannot preempt a later lease release.
Unreachable owners release through the existing bounded accounting chain.

The runtime retains its 64 operand cells, 768 aggregate cells, 65 accounting
entries and 4,096 lease slots. The accounting root-buffer ceiling is 2,162,688
bytes. Scans, indices and malformed input remain bounded and fail explicitly.

## Delivery limits

This is one interpreter/compiler candidate path, not a parallel library or
runtime. Candidate selection can advance a source program that passes owned
budgets through helpers from its earlier emitted minor. That does not grant
native 1.42 lowering, installed promotion or a new qualification identity.
Those gates remain required for complete Windvale 1.0 delivery.

Structured-task programs with owned-budget helpers also select this candidate.
The runner checks their exact two-parameter task entry through the existing
interpreter directory reader before choosing the task request envelope. WVB
1.42 retains the same task environment, capability grants and failure rules;
the profile byte alone cannot select the task entry. Immutable budget views
remain representation-hidden borrowed cells, including in earlier source IR.
The instruction scanner and borrow preparation admit task dispatch only for
that candidate task request. Spawned callback entry, normal return and trap
unwinding keep borrow frames aligned with execution frames. Existing capability
checks still guard host calls; unsafe and foreign instructions stay excluded
from this path.

The budget-return probe uses the canonical Split Result. Arbitrary manual
construction of a Result containing an opaque budget is still rejected by WIR
ownership validation; this change does not claim that wider composition.

The maintained Package-Lock directory migration still must preserve serialized
lock bytes, validation order and explicit allocation failure behavior. Passing
isolated helper fixtures does not complete that consumer or Libraries 1.0.
