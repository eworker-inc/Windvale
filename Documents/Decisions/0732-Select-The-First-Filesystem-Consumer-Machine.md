# Decision 0732: Select the first filesystem consumer machine

- Status: Accepted; live recycling, domain publication, and endpoint binding pending
- Date: 2026-08-16
- Advances: [Decision 0731](0731-Publish-Durable-Filesystem-Domain-Ledger.md)
- Contracts: [provider launch transaction](../../Specifications/Windvale-Os-Provider-Launch-Transaction.md) and [kernel memory](../../Specifications/Windvale-Kernel-Memory.md)

## Context

The filesystem provider and its `1/81/1` domain ledger are ready, but endpoint
`131072` has client 0. Init cannot simply become the consumer: it is the exited
generation-one provider of the endpoint slot that was recycled for the
filesystem. Binding that terminal process reference would create stale
authority rather than an application.

The terminal directory process and its ten-page memory object still occupy
identifier 3. They have no live endpoint after its closed record became the
filesystem domain ledger. Recycling that pair gives the first consumer a fresh
generation without displacing the filesystem provider or consuming the slot
reserved for its later sequential network replacement.

## Decision

Select process and memory reference `131075`, generation 2 of identifier 3, as
the first filesystem consumer machine. Select application domain `65540`,
separate from filesystem domain `65538` and future network domain `65539`.
Charge the application domain for one process, six user pages, and zero owned
endpoints.

Require the following exact preflight before any live mutation:

- filesystem endpoint `131072` and ready provider `196610`;
- terminal/releasable directory process and memory slots;
- a terminal record slot for the application domain ledger;
- exactly 47 free pages after releasing the ten-page directory object;
- a ten-page replacement with four kernel paging and six user pages; and
- endpoint rights 17, never provider rights 46.

The endpoint remains charged to the filesystem provider domain. The consumer
domain owns a rights-limited capability, not the endpoint object. Selecting the
machine does not yet identify which exact retired resource-record bytes will
hold its domain ledger; live WVA must preflight that slot before reuse.

## Consequences

The portable machine-binding policy rejects the stale init reference, wrong
endpoint or domain, a provider that is not ready, a live process/memory/domain
slot, wrong page geometry, and provider-strength rights. The focused provider
launch owner now passes 26 cases across 14 behavior groups.

No EFI identity changes in this slice. No directory memory is released, no
consumer record or domain ledger is written, endpoint client remains 0, and no
provider or application thread is entered. The next machine slice must select
and validate the exact retired domain-record storage, construct private
generation-two paging/image/context/process/thread state, and commit the domain
plus endpoint client atomically.

## Reconsideration triggers

Reconsider the ten-page geometry if the smallest useful native consumer cannot
fit six user pages with bounded transfer and stack space. Reconsider the
directory slot only if its complete terminal process, thread, memory, and
resource ownership cannot be preflighted. Never fall back to exited init or put
the application in the provider domain merely to avoid a separate ledger.
