# Decision 0574: Active Generation 1 command resolution

- Status: Implemented and paired-host verified in GitHub run `31903569891`
- Date: 2026-08-15
- Advances: Milestone 4 and Decision 0590
- Contract: [Generation 1 and Activation 1](../../Specifications/Windvale-Installation-Generation.md)

## Context

Generation 1 now carries explicit `wvdump` and `wvquery` command records, and
Activation 1 durably selects one generation. A launcher must not infer a command
from a package name, trust a caller-selected inactive file, or reproduce the
portable record grammar in host JavaScript.

Actual process execution has a separate boundary: the selected launch record,
approval, package object, and host image must all be present and reverified
before authority is bound. Combining selection and execution prematurely would
hide which guarantee failed.

## Decision

Add a Windvale-written active-command resolver over the portable Generation 1
and Activation 1 implementation. Given the public activation bytes, one
generation file, the current host target, and a command identifier, it:

- validates both canonical records;
- requires the generation SHA-256 to equal the active identity;
- requires the exact current target;
- looks up one ordered command record; and
- reports its package, part, approval, and launch identities.

The first paired owner resolves both real commands on Windows and Linux and
rejects unknown commands, wrong targets, inactive generations, malformed
activation, malformed generation, and invalid invocation.

Keep process launch outside this resolver. The next host adapter consumes only
a successful exact resolution, reverifies its referenced objects, binds the
rights-limited launch profile, and then executes the target.

## Consequences

- Active command selection follows Windvale semantics on both permanent hosts.
- Command names and policy identities remain signed Generation 1 data rather
  than launcher conventions.
- Resolution alone grants no capability and makes no process-execution claim.
- The host adapter needs only bounded path acquisition and execution mechanics;
  it may not reinterpret the records.

## Reconsideration triggers

Reconsider when aliases, command arguments in Generation 1, multiple active
targets, command shadowing, dependency-provided commands, or policy revocation
must be represented.
