# Decision 0625: Windvale-owned init process-record construction

- Status: Implemented current-Windows-host native candidate; fixture replacement pending
- Date: 2026-08-15
- Advances: [Decision 0624](0624-First-Windvale-Owned-Init-Extent-Allocation.md)
- Contract: [init process-record emission](../../Specifications/Windvale-Os-X64-Process-Record-Emission.md)

## Context

Windvale source now allocates and validates the first 12-page protected-process
extent, but the retained architecture fixture still initializes the process
record that makes that extent meaningful. Source ownership must preserve every
record field and both measured image identities without turning one fixture's
digests into permanent kernel constants.

## Decision

- Emit the exact 462-byte init process-record construction at fixture offsets
  1,971 through 2,432.
- Require exactly 32 bytes for both executable and program digests, and accept
  those identities as constructor inputs.
- Clear the complete 288-byte record before publishing its exact identities,
  states, addresses, budgets, capability, role, runtime, and generation fields.
- Derive user addresses from the already validated extent and the endpoint from
  retained kernel state; do not claim that the record is live or published.
- Keep the constructor disconnected from the provider transaction until paging,
  endpoint allocation, record publication, dispatcher entry, and QEMU evidence
  are composed as one failure-atomic machine path.

## Evidence and consequences

The exact slice has SHA-256
`2a5b757c6550a381ea3a22c0edbe9d6f24e6804274cd1cab8c28be721b448b65`.
The self-test WVB is 16,069 bytes at
`be44b1d300abd532a5689755f9ab9ed75b49e7e4954395d3626ee175b9b97e13`.
Its Windows executable is 236,032 bytes at
`693ce53db751bd537ade2933adc8f688ff42492aad6091e005ea9b6391d7ff16`;
the paired Linux image is 241,776 bytes at
`1ecaa2ac3dda959a632b88c753d4189ecd3213a2f04c69c886f5bc0f11db23c0`.
The focused owner passes 36 cases across six projects with local results
50/51/52/53/54/55. The retirement inventory is 70 suites and 3,600 cases.

Windvale source now reconstructs the first 2,433 process-machine bytes and all
13 relocation fields in that interval. The next source boundary is kernel-table
copy and init page-table construction, not process publication.

## Reconsideration triggers

Replace the fixed init profile when general process construction owns dynamic
roles, budgets, capabilities, and address-space geometry. Preserve complete
zero-initialization, verified digest inputs, checked addresses, generation-safe
publication, and rollback under the new layout.
