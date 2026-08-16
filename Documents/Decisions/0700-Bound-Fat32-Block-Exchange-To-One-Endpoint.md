# Decision 0700: Bind the FAT32 block exchange to one endpoint

- Status: Implemented architecture-neutral native candidate; privileged endpoint adapter pending
- Date: 2026-08-16
- Advances: [filesystem implementation plan](../Project/Windvale-Filesystem-Implementation-Plan.md)
- Contract: [FAT32 block-exchange state 1](../../Specifications/Windvale-Os-Fat32-Block-Exchange-State.md)

## Context

Decision 0699 freezes the provider bytes, but bytes alone do not prevent two
outstanding requests, early completion, duplicate completion, replay after
cancellation, or reuse after peer loss. The filesystem service needs an exact
capacity-one lifecycle before a privileged endpoint adapter can transport the
protocol safely.

## Decision

- Bind one endpoint reference, one block reference, and one admitted grant per
  exchange generation.
- Construct at most one request and mark dispatch separately from construction.
- Reject completion before dispatch and require the bound endpoint identity.
- Consume a dispatched sequence exactly once, including malformed responses,
  cancellation, and provider loss; never replay it implicitly.
- Permit pre-dispatch cancellation to return to ready without consuming the
  sequence.
- Require confirmed teardown after uncertain cancellation or provider loss,
  then clear authority and require a new binding.

The 20,279-byte exchange WVB has SHA-256
`820617dc73799c5cbaea318d85a0e6352e539889eb6f3ea525c2dee22cca6690`.
Its composed 37-case owner returns 47 and pins paired Windows/Linux images.

## Consequences

The next integration increment has one explicit place to translate endpoint
send, receive, cancellation, and peer-exit events. It cannot claim a live block
path until a kernel/service adapter actually performs those operations. Queue
depth greater than one, batching, driver execution, media change, and partition
discovery remain outside this decision.

## Reconsideration triggers

Reconsider the capacity only when measured directory or file-data workloads
require more concurrency and a bounded queue can preserve per-operation grant,
generation, sequence, cancellation, accounting, and teardown evidence.
