# Windvale OS resource-domain policy

## Status and scope

Resource-domain policy 1 is the first portable implementation pressure from the recommended flat-domain architecture. It models the resources already measured by Probe 40: processes, committed 4 KiB pages, and service endpoints. It is an immutable policy model, not a serialized format, kernel ABI, public object table, allocator, capability bundle, or dynamic launch implementation.

## Transition contract

[`Resource-Domain-Policy.wv`](../Operating-System/Kernel/Resource-Domain-Policy.wv) carries a generation-safe identity, `Alive → Stopping → Dead` lifecycle, limits, one complete in-flight reservation, committed use, peak use, and a stable stop reason. Every operation returns a complete replacement transition record; rejected operations reproduce the input accounting unchanged.

Acquisition is deliberately single-transaction in version 1:

1. `reserve` checks the complete process/page/endpoint charge against committed use and every ceiling;
2. construction occurs outside this capability-free policy model;
3. `commit` accepts only the exact reservation, clears it, and advances committed and peak use; or
4. the caller discards the reservation record, exposing no committed object.

Checked subtraction avoids overflow at each ceiling. A second reservation is rejected while one is live. Release rejects any charge larger than committed use and never reduces peak evidence.

## Stop behavior

`stop` is idempotent. The first call records `Stopping` and the reason; later calls preserve that reason. New reservations are rejected after stop begins. `finish_stop` reaches `Dead` only when reservations and committed use are zero. Repeated stop of a dead domain preserves the same terminal generation and reason.

## Evidence and limits

The `os-resource-domain` native owner compiles the policy as an independent Project 2 module and executes rejection-before-exposure, exact reserve/commit, busy reservation, mismatched commit, peak retention, live-resource stop rejection, complete release, and repeated-stop cases.

Version 1 does not yet count threads, handles, capabilities, queued messages, CPU, pinned/DMA/shared/guest memory, output, diagnostics, or teardown work. It does not mutate Probe 40 or claim the domain surrounds the live three-process firmware path. Those additions require a measured consumer and a new contract revision rather than silently widening this record.
