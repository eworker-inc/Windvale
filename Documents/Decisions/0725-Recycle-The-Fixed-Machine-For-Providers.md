# Decision 0725: Recycle the fixed machine for providers

- Status: Accepted; privileged reconstruction pending
- Date: 2026-08-16
- Contract: [provider launch transaction](../../Specifications/Windvale-Os-Provider-Launch-Transaction.md)

## Context

Probe 40's 157-page arena is exactly full with init, the recyclable client,
and directory. Its dispatcher and state page contain three process/object
slots and two endpoint slots. Adding filesystem and network as simultaneous
fourth and fifth processes would require a new memory-state version before it
could prove even one guest file read.

The existing client slot already proves complete generation-safe release,
zeroing, and same-root reuse. The two existing endpoint slots also have checked
close and generation semantics. A bounded first provider machine can reuse
those mechanisms after their current work is complete.

## Decision

Keep the filesystem and network request/domain identities, but bind their first
machine executions sequentially:

| Role | Request | Domain | Process | Endpoint |
| --- | ---: | ---: | ---: | ---: |
| Filesystem | `65540` | `65538` | `196610` (generation 3, slot 2) | `131072` (generation 2, slot 0) |
| Network | `65541` | `65539` | `262146` (generation 4, slot 2) | `131073` (generation 2, slot 1) |

Admission requires explicit evidence that the process/object slot is released
and the selected endpoint slot is closed. A live or wrong-generation slot
fails before resource-domain reservation or publication. The embedded shims
now wait on the same generation-two endpoint references.

## Consequences

This avoids pretending the full arena contains additional concurrent process
records and gives the privileged implementation a concrete next transition:
reconstruct generation-three filesystem state in the released client slot,
advance the resource endpoint, enter the image, and complete one request.
Filesystem must drain before generation-four network reuse. This is suitable
for the first deterministic integration proof, not a permanent service manager
or claim that stable systems should serialize all providers.

The provider policy passes 18 focused cases. The boot-linked provider bytes and
therefore the process object and EFI identities change, but retained Probe 40
behavior is unchanged because it does not yet enter either provider image.

## Reconsideration triggers

Introduce a larger memory-state/object-table version when filesystem and
network must remain concurrently available, when more than one application
consumer is live, or when restart requires reserved recovery capacity. Do not
reuse a slot until teardown proves no active work, mapping, capability, or
uncertain mutation remains.
