# Decision 0578: Composed offline activation and rollback

- Status: Implemented locally; paired-host evidence pending
- Date: 2026-08-15
- Advances: Milestone 4 and Decision 0568
- Depends on: Decisions 0574 and 0576
- Contract: [Generation 1 and Activation 1](../../Specifications/Windvale-Installation-Generation.md)

## Context

Portable Generation 1 and Activation 1 semantics, immutable generation
publication, durable activation replacement/recovery, active-command resolution,
and verified process dispatch exist as separate boundaries. The milestone still
needs one composed proof that an interruption preserves the old generation, an
update selects a complete new generation, and rollback selects retained content
with a higher serial.

The portable transition planner returns a `u64` serial. The current native AOT
subset deliberately admits `u64` arithmetic but not `U64ˉformat`. Expanding the
backend merely to serialize one decimal field would mix compiler scope into the
package lifecycle.

## Decision

Add a Windvale-written activation planner CLI over the existing portable
transition functions. It emits an exact bounded plan report containing the next
serial as low/high `u32` limbs plus the current and previous generation
identities. The host adapter mechanically reconstructs the unsigned serial,
serializes canonical Activation 1 text, and sends that record back through the
Windvale planner/parser before durable publication.

Compose the planner with the existing generation publisher, activation
publisher/recovery adapter, and active-command resolver. The focused owner:

- publishes an initial one-package generation and the exact two-package
  Generation 1 record;
- activates the first generation and observes only `wvdump`;
- writes a complete private update candidate, recovers it, and proves the first
  generation remains active;
- plans, revalidates, and publishes serial-2 activation of the two-package
  generation, then observes `wvdump` and `wvquery`;
- plans, revalidates, and publishes serial-3 rollback; and
- proves both immutable generation records remain byte-identical and recovery
  has no remaining candidate.

## Consequences

- Transition semantics remain Windvale-owned while filesystem durability remains
  a narrow host responsibility.
- The host does not interpret transition policy; its serial conversion is an
  exact two-limb unsigned representation change followed by semantic recheck.
- Recovery never promotes a private candidate or guesses whether publication
  occurred.
- Rollback changes only Activation 1 and retains both generations.
- Revocation and uninstall remain the last open lifecycle boundaries.

## Reconsideration triggers

Reconsider the limb report when native AOT admits `U64ˉformat`, Activation 2
changes serial representation, multiple writers are introduced, or channel
freshness policy becomes part of the transition request.
