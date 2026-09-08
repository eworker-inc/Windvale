# Decision 0960: admit verified Foundation borrows in the host scalar runner

## Status

Implemented as a bounded source-built Windows/Debian execution checkpoint on
2026-09-08. This is not installed-tool promotion or complete Option/Result
qualification.

## Context

The immutable payload representation and exact borrowed call identity from
[Decision 0957](0957-Represent-Immutable-Foundation-Payload-Borrows-In-Candidate-Wvb-1.39.md)
and [Decision 0958](0958-Preserve-Foundation-Borrow-Identity-Across-Direct-Calls.md)
now have complete verifier and bounded runtime component evidence. The remaining
host envelope rejected minor 39 before the connected runtime could execute it.
Checking only the command-line wrapper would leave direct interpreter callers
without the required static ownership proof.

## Decision

1. Admit minor 39 through the host scalar envelope only after the existing
   complete metadata, semantic, typed/loan, and control-flow verifier succeeds.
   Run that check before interpreting candidate type, function, or value cells.
2. Require request major 1. Preserve the existing portable, capability-free
   profile check and synchronous scanner restrictions. Async calls, task/unsafe
   operations, and provider requests are not newly admitted.
3. Reuse the existing scalar interpreter, frame-owned leases, original-owner
   collector roots, descriptor retention, and bounded failure/teardown paths.
   Do not create a second evaluator or trust a caller-provided verification bit.
4. Preserve all existing instruction, stack, call-depth, aggregate, and lease
   limits. Verification success is necessary but does not guarantee that every
   valid module fits this bounded execution profile.
5. Keep the separate browser envelope, native lowerer, installed tool identities,
   packages, and Windvale OS at their existing version boundaries.

## Evidence and limits

The [runtime evidence](../Evidence/2026-09-08-Foundation-Borrow-Runtime-Execution.json)
records the current-source runner build and its Windows and Debian packages.
The exact previously published candidate exercises three projections and
borrowed direct/forwarding calls and returns 42 on both hosts. Nine damaged
variants reject before execution. Three earlier-version aggregate Sequence
lifetime workloads also pass on both hosts with eight cycles each.

Both native packages consume the same Windows-built runner WVB; this is paired
execution, not independent compiler reconstruction. The fixture is a pinned
publication, not a fresh source compilation in this run. Real-consumer migration,
broader payload execution, source classifier integration, exclusive borrowing,
take, mapping, and final compiler/library qualification remain open.
