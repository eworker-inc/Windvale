# Decision 0869: expose structured-task completion order

## Status

Accepted on 2026-08-28.

## Context

Decision 0868 gives the portable WVB 1.32 runner a deterministic queued
scheduler. Within each four-slot group it executes children in lane order
`3, 1, 0, 2`, while affine task handles and consuming awaits remain source
creation ordered. The cancellation fixture proves that four siblings coexist,
but it does not show which child actually completed first.

Workload 6 requires completion order to be independently observable. Adding a
portable scheduler-inspection API would expose a reference-runtime policy as
language semantics. A synthetic host trace would not prove that the real child
work ran in that order. Windvale already has an explicit, rights-limited
`console.write_line(text) -> void` capability, so a bounded task fixture can
produce the evidence through normal program behavior.

Execution-request major `6`, minor `1` intentionally admits no capabilities and
has no output streams. Widening it in place would make its existing envelope
ambiguous and weaken its exact no-capability validation.

## Decision

Execution-request major `6` gains minor `2` as one narrowly scoped observable-
output form. Minor `1` remains byte-for-byte the zero-capability task request.

Minor `2` has a fixed 84-byte little-endian header. It retains the minor-1
fields through offset 71 and appends:

- offset 72: one `u32` capability-grant bitset;
- offset 76: one `u32` standard-output byte limit; and
- offset 80: one required-zero reserved `u32`.

The module begins at offset 84. Validation rejects a grant above bit zero, an
output limit above 65,536 bytes, a nonzero reserved field, inconsistent lengths,
a non-hosted module, any capability count other than one, or any capability
other than exact version-1 `console.write_line(text) -> void`. The ordinary
runner grants bit zero with a 64-byte standard-output limit.

The preflight record carries the request minor and, for minor `2`, the exact
capability section identity, grants, output limit, declared-capability bitset,
and one-byte capability-kind catalog before the existing type offsets. The
interpreter returns response minor `2` and appends two bounded `u32` stream
lengths plus the exact standard and diagnostic bytes after the common 20-byte
execution result. The runner validates that both lengths exactly cover the
response before emitting them to their separate host sinks.

`Structured-Task-Completion-Order-Executable.wv` creates four accepted child
tasks. Each child captures its task-slot value, writes it, and returns it. The
parent awaits handles in creation order and returns `42` only for values
`0`, `1`, `2`, `3`. The required exact transcript is:

```text
3
1
0
2
Result: 42
```

This is application-level evidence produced by real task work. It is not a
scheduler event API, a worker-count promise, or permission for an undeclared
capability.

## Evidence

The fixture compiles to a deterministic 6,544-byte WVB at SHA-256
`6b6eb29ae5b711358e582c42d2667ab21c0861ac1ca5b1bc70b3ab575711c80c`.
The compiler-aligned verifier accepts it. A source-built runner with 228
functions and 430,311 code bytes packages successfully for the current Windows
host and emits the exact transcript above with process status zero.

The affected owner passes all 59 named phases and 161 declared cases, including
29 structured-task, 46 task-runtime, 17 task-environment, and 69 malformed-input
cases.

## Consequences

- Runtime completion order is now observable through ordinary, explicitly
  authorized program behavior.
- Awaited values remain bound to their creation-ordered affine handles.
- Zero-capability task modules retain execution-request minor `1` unchanged.
- The new request form cannot grant filesystem, network, process, clock, or any
  other capability.
- Source syntax, Foundation task signatures, WVIR 1.21, and WVB 1.32 do not
  change.
- The portable oracle remains single threaded. Provider-generation recovery
  and parallel-capable paired-host qualification remain separate work.

## Reconsideration triggers

Reconsider this request form if another task fixture needs a second capability,
if a stable hosted runner contract replaces the diagnostic envelope, if output
must become structured task-event evidence rather than application text, or if
a parallel adapter cannot reproduce the same typed results and bounded stream
contract.
