# Windvale OS FAT32 block-exchange state 1

## Status and scope

Block-exchange state 1 is the implemented capacity-one lifecycle between the
isolated FAT32 service and one separately bound block-provider endpoint. It
composes the admitted read transaction with `WVBR 1`/`WVBP 1` without granting
an ambient device namespace or allowing more than one outstanding operation.

[`Fat32-Block-Exchange-State.wv`](../Operating-System/Services/Fat32-Block-Exchange-State.wv)
binds one nonzero endpoint reference, one nonzero block-capability reference,
one admitted read grant, and one generation. A fresh binding starts at sequence
one. Beginning an operation derives the exact sequence and block-read plan from
private state and produces one immutable 48-byte request. A second begin is
busy until that request reaches a terminal transition.

## Dispatch and completion

The lifecycle distinguishes request construction from dispatch. Completion is
rejected before dispatch and must present the bound endpoint. A dispatched
operation consumes its sequence exactly once for complete, unavailable, stale,
or malformed replies. Duplicate and late replies are rejected after the state
returns to ready; an invalid reply becomes invalid payload and is not replayed.

Cancellation before dispatch returns to ready without consuming the sequence.
Cancellation after dispatch enters stopping, consumes the sequence, and cannot
be retried. Provider loss also enters stopping; when a request was dispatched,
the caller receives provider-lost and the sequence is consumed. Teardown must
confirm provider release before the state becomes dead and clears both bound
references. A new provider generation requires a new binding.

The persistent state uses a flat snapshot of the admitted capability, current
plan, endpoint identity, and terminal result. Typed block records are rebuilt at
transaction and protocol validation boundaries; this state is not a serialized
wire format.

## Evidence and limits

The exchange WVB is 20,279 bytes at SHA-256
`820617dc73799c5cbaea318d85a0e6352e539889eb6f3ea525c2dee22cca6690`.
The composed 59-case owner returns 47 on Windows and pins deterministic
Windows/Linux images. It covers invalid binding, capacity rejection, dispatch
identity, completion-before-dispatch, cancellation on both sides of dispatch,
teardown, exact completion, duplicate completion, malformed reply consumption,
peer loss, and a checked immutable-image provider round trip.

This policy does not yet issue a kernel endpoint syscall, execute a hardware
block driver, detect media change, or discover partitions. File-read transaction 1
now owns the admitted identity and its exact begin, dispatch, and completion
transitions while copying validated partial-sector data into a shared filesystem
reply, but live guest execution remains a separate integration claim.
