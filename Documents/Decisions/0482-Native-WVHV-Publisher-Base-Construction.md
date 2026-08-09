# Decision 0482: Native WVHV publisher base construction

- Status: Implemented current-host candidate; Linux execution and durable promotion pending
- Date: 2026-08-09
- Advances: [Decision 0481](0481-Native-WVHV-Publisher-File-Pipeline.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [publisher base construction](../../Specifications/Windvale-Native-Hosted-Verifier-Publisher-Base-Construction.md)

## Context

Decision 0481 connected the publisher-specific records and final PE/ELF
materializers through hosted Windvale file tools. Its generic six-service base
still came from the frozen managed builder, so the otherwise native pipeline did
not yet form one ordinary .NET-free construction path.

The existing verifier container stages already owned service-bundle requests,
bundle materialization, startup instantiation, platform regions, and final base
composition. The missing seam was ordinary file production of exact `WVHV`
metadata and the corresponding `WVHR` runtime header.

## Decision

- Treat one canonical `WVSQ 2` request as the immutable source for both service
  hashing and bundle materialization. Do not introduce another public evidence
  record family.
- Add one focused metadata tool that constructs private `WVVE` and invokes the
  existing `WVVR` and `WVHV` cores, plus one focused runtime wrapper over the
  existing `WVHR` constructor.
- Package all eleven publisher-construction commands for both permanent hosts in
  the existing construction candidate rather than duplicating a second toolset.
- Add paired exact candidate constructors that natively lower/link the publisher
  WVB, require `Main` at 3,001, verify every pinned input, and reproduce both
  complete publisher applications without .NET.
- Give the path one six-case native retirement owner. Keep the managed integrated
  test only as recovery/differential evidence.
- Refuse existing construction destinations. Do not claim durable publication
  until a completed-publisher admission and replacement transaction exist.

## Evidence and consequences

The two new WVBs build through the digest-bound native source front door at
70,166 and 20,387 bytes. Focused record probes reproduce target-specific
1,024-byte metadata and 4,096-byte runtime values, bind all seven WVSQ payload
digests, reject truncated input, and preserve exact alias inputs and existing
destinations.

The 22 paired host packages are pinned by the 42-entry construction inventory.
The reviewed focused native lane passes 6/6 in 29 seconds on Windows. It
constructs the exact 256,000-byte Windows and
254,917-byte Linux publisher candidates, rejects malformed and aliased base
inputs without mutation, and uses the constructed current-host publisher to
install the exact verifier candidate.

This removes the frozen managed generic-base builder from the normal candidate
construction path. It does not delete Stage 0 recovery source, qualify Linux
execution, or make the construction copy a durable publication transaction.
No unfiltered retirement, Seed, OS, Standard, Qualification, WebAssembly, QEMU,
or broad Linux gate is part of this decision.

## Reconsideration triggers

Version the candidate or focused tools if WVSQ geometry, ordered services,
publisher WVB identity, entry offset, metadata/runtime contracts, startup or
adapter objects, base layout, final image identity, or destination behavior
changes. Introduce durable promotion only through an admitted transaction with
explicit indeterminate-completion behavior.
