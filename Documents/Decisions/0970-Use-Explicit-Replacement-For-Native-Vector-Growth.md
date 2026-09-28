# Decision 0970: use explicit replacement for native Vector growth

## Status

Candidate implementation of the accepted Language 1.0 memory direction.
The [native collection contract](../../Specifications/Windvale-Native-Owned-Collections.md)
and [budgeted adapter contract](../../Specifications/Windvale-Native-Budgeted-Storage.md)
own the precise subset, limits and failure behavior. Routine host verification
belongs in the commit summary. Installed promotion remains a separate gate.

## Decision

Append within reserved capacity and indexed scalar reads operate directly on
owned backing. Append never grows implicitly: a full collection returns its
input item and a capacity failure. Explicit growth borrows a funding budget
and reserves the complete replacement while the old allocation remains live.
It copies only the header and live cells, then swaps ownership and credits the
old lease. Refusal preserves both owners and consumes no generations.

Charging the complete overlapping allocation makes peak memory explicit and
keeps failure atomic. An implementation that first releases the old backing
cannot promise to preserve the collection if replacement fails. In-place
growth could reduce peak use but needs a separately proved provider contract;
it is not assumed here. The adapter retains one accounting tree and one
physical allocation domain, without a second collection allocator.

## Boundaries

Reuse ABI 24/context 10 and admit only the necessary existing WVB subsets.
Keep locals, native frames, code, elements and committed backing bounded.
Continue running the complete WVB verifier before stricter native admission.
Ownership joins still require matching live owners. The existing source borrow
rules freeze a Vector after indexed reads; this change does not widen them.

The next integration step is owned helper/aggregate transfer, followed by
interpreter working state. Shared immutable backing and normal installed
launchers remain open. These native workload results do not establish reduced
ordinary interpreter RSS or complete Windvale 1.0 memory management.
