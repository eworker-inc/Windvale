# Decision 0622: First Windvale-owned process-coordinator initialization

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0621](0621-First-Windvale-Owned-Process-Machine-Code-Emission.md)
- Contract: [process-coordinator initialization emission](../../Specifications/Windvale-Os-X64-Process-Coordinator-Emission.md)

## Context

Decision 0584 owns the checked x86-64 builder and exact 1,119-byte process entry
and ready/wait dispatcher. The next bytes cross the first privileged composition
boundary: memory-state validation, native policy context, one external policy
call, arena recovery, and fixed process-record pointers. Copying only their raw
bytes would hide seven long failure branches and the external WVO relocation.

## Decision

- Construct fixture offsets 1,119 through 1,427 as one bounded Windvale module.
- Retain the current measured 112-byte context format 7, instruction/depth
  budgets 21,918/5, policy token 97, 2 MiB arena alignment, and fixed process
  record offsets without making them portable application semantics.
- Leave seven failure displacements and one policy-call displacement explicit
  and zero in the independent slice.
- Publish every failure field, its exact displacement to absolute failure target
  33,826, and the policy import's symbol index 12/addend -4.
- Reject any emitted length, field, target, normalized payload, or paired host
  image that differs from the current authoritative fixture evidence.
- Do not install the slice into the process object or claim live providers until
  the complete branch/import graph composes and boots.

## Evidence and consequences

The 309-byte normalized payload is SHA-256
`e5fc847bd3843f3db571ca779059362f62e1e6fd824aef43978573173ebc2464`.
The self-test WVB is 17,360 bytes at
`da3d04e734f6057ce9665e1e1c48d6c9dfcdbe0a9396cd1a94397ac4d284a203`.
Its current Windows executable is 251,392 bytes at
`128269a0d5cedd8e2eed4ab4a569b355a1811b2300fd7b16a36078f3eee15c36`;
the deterministic Linux image is 254,064 bytes at
`8c160bc19330784ca82ca837d5a33fe93fe44fc5701cf491c661b1d06e728318`.
The owner passes 21 cases across three projects with local results 50/51/52.
The retirement inventory is 70 suites and 3,585 cases.

Windvale source plus explicit relocation evidence now reconstructs the first
1,428 process-machine bytes. At this decision point, the boot artifact remains
unchanged because the following channel/endpoint, memory-object, page-table,
syscall, exception, and timer regions are not yet source-composed.

[Decision 0623](0623-Windvale-Owned-Process-Channel-And-Endpoint-Initialization.md)
subsequently source-owns the fixed channel/endpoint region through byte 1,871;
memory-object allocation is now the next fixture boundary.

## Reconsideration triggers

Revisit the slice boundary if channel/endpoint initialization requires a shared
typed emitter smaller than the current boundary, or if complete-object
composition proves that a failure/import relocation differs from this measured
surface. Do not preserve these offsets as a compatibility ABI after the fixture
is retired.
