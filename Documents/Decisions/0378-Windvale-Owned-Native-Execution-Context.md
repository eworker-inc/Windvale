# Decision 0378: Windvale-owned native execution context

- Status: Accepted current-host normal-path context-construction transfer; service-free bootstrap replacement, Linux execution, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0377](0377-Windvale-Owned-Native-Service-Table.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native execution-context construction](../../Specifications/Windvale-Native-Execution-Context-Construction.md)
- Advanced by: [Decision 0379](0379-Windvale-Owned-Native-Argument-Table.md)

## Context

Decision 0377 moved the final binding table to Windvale. The normal executor
still allocated and wrote every field of the 112-byte execution-context
version 7 in C#: budgets, service and arena pointers, initial mutable state,
arguments, and the three specialized table pointers.

The context is also required to execute each retained service-free Windvale
constructor. Calling the new constructor unconditionally from that executor
would therefore recurse while trying to construct the constructor's own
context. The transfer needs an explicit bootstrap boundary rather than a
hidden recursive dependency.

## Decision

- Define exact 120-byte `WVXQ 1` input and 32-byte `WVXR 1` response envelopes.
- Let portable Windvale validate positive budgets, exact normal-host arena
  bounds, zero initial mutable/reserved state, the 67-argument limit, and the
  five optional-table presence relationships before constructing unchanged
  context version 7.
- Route ordinary application execution through one exact digest-bound
  service-free WVNF. Keep source and WVB for reproduction, qualification, and
  recovery; do not embed the WVB in the normal runtime.
- Move allocation, initial-byte verification, permitted post-call mutation
  checks, and teardown into a focused context owner outside the already large
  executor source.
- Keep one explicitly named frozen Stage 0 context oracle only when executing
  service-free bootstrap constructors. Compare it byte-for-byte with the
  Windvale result and retire it when a later bootstrap host can execute those
  constructors without this cycle.
- Retain host ownership of resource allocation, opaque pointer acquisition,
  invocation, result admission, and platform teardown.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Execution-context core WVB | 5,530 | `dda77e9fd637746bf5b1179136deee0bbae2d8d6b57982323b868b98a8daa29b` |
| Retained execution-context bridge WVB | 5,531 | `86b9a139a387eb3c4fb86f43731e442a62af8ce3c7289cf914b31a9256d21a68` |
| Retained execution-context bridge WVNF | 58,363 | `acdfc7d71b5fc2f0c1cfd76242fddc59db2563a4026ac286313711f0e2eb05de` |

## Evidence and consequences

The reviewed focused case pins and reproduces all source/WVB/WVNF identities;
confirms that the runtime embeds no constructor WVB; compares minimal and
fully populated requests through the reference interpreter, retained native
fragment, independent response verifier, and Stage 0 oracle; checks thirteen
malformed requests; verifies the three permitted live mutations while
rejecting an immutable-field mutation; reproduces the bridge through the
ordinary native source front door; and executes a real `Textˉconcat` call
through the normal context path. The single selected test passes 1/1 in 1.497
seconds through the Release test application. The affected runtime also builds
in Release with zero warnings and errors.

The broader hosted-service, file-I/O, exact-compiler, Development, Standard,
Qualification, and Linux gates were reviewed but not run under the goal's
deferred-broad-verification rule.

The normal executor no longer writes execution-context fields or directly
reads mutable context words. It consumes a verified Windvale result, passes the
owned context to native code, and receives a bounded completion record. The
remaining Stage 0 writer is isolated to service-free constructor bootstrap and
is therefore a visible unfinished retirement item. W^X allocation authority,
arena allocation, native invocation, result admission, and teardown remain
later host slices.

## Reconsideration triggers

Remove the oracle as soon as a qualified native bootstrap host can execute the
retained constructors without using this managed executor. Version the request
when the context layout, normal-host arena bounds, optional pointer set, or
permitted mutations change. Never place live addresses in retained WVB, WVNF,
WVO, or cache artifacts.
