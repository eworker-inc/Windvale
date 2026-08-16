# Windvale bounded operation model

## Status and scope

Bounded operation profile 1 is the implemented-candidate asynchronous lifecycle
shared by networking and later filesystem, process, terminal, and device work.
It is capability-free and uses an explicit virtual monotonic time input. It does
not create a thread, callback, event loop, host wait object, or network socket.

## Contract

An operation has a nonzero `u64` identity, nonzero provider generation, exact
requested/progress byte counts, optional `u64` monotonic deadline, mutation and
control classification, and explicit queue accounting. The first transfer bound
is 65,536 bytes.

The queue reserves configured terminal capacity for control work. Ordinary data
is rejected before consuming that reserve; control is accepted until the exact
queue limit. Every accepted operation moves from `Queued` to `Active` or one
terminal outcome. Terminal outcomes are completed, cancelled, timed out, stale,
provider exited, or indeterminate. Immediate malformed, capacity, and expired-
deadline rejection is explicit.

Progress cannot exceed the request or claim completion before exact progress.
Cancellation or timeout of an active mutation is `Indeterminate` unless the
provider confirms cancellation. Provider loss makes an active mutation
indeterminate and a queued/read operation `Provider_exited`. Releasing a
terminal operation decrements queue use exactly once and retains its evidence.

## Evidence and limits

The focused self-test is a 12,769-byte WVB at SHA-256
`dac9582ae8ea2202fc16e5e15020136b63a668c722dbdab6863a98e07d7ff477`.
The current Windows image returns 44; exact Windows and Linux images are built
across twelve cases covering data/control capacity, partial and exact progress,
release, stale generations, cancellation, deadline, provider loss, and uncertain
mutation outcomes.

This candidate has one modeled operation at a time. Bounded wait batches, more
than one simultaneous operation record, kernel timer/wake mechanisms, provider
IPC, and network-specific values remain successor work.
