# Windvale OS resource-domain policy

## Status and scope

Resource-domain policy 1 is the portable accounting gate used by the current native Probe 40 construction. It models the resources already measured there: processes, committed 4 KiB pages, and service endpoints. It is an immutable policy model, not a serialized format, kernel ABI, public object table, allocator, capability bundle, or dynamic launch implementation.

The live process-policy path composes this module and must return token 97 before any of the three process objects are published. It reserves and commits the exact ordinary-domain charge of three processes, 144 process-owned pages, and two endpoints; the thirteen kernel and recovery pages remain outside the domain. The two client generations temporarily reduce current use to two processes and 22 pages, reserve the complete replacement charge before reuse, and retain peaks of 3/144/2.

## Transition contract

[`Resource-Domain-Policy.wv`](../Operating-System/Kernel/Resource-Domain-Policy.wv) carries a generation-safe identity, `Alive → Stopping → Dead` lifecycle, limits, one complete in-flight reservation, committed use, peak use, and a stable stop reason. Every operation returns a complete replacement transition record; rejected operations reproduce the input accounting unchanged.

Acquisition is deliberately single-transaction in version 1:

1. `reserve` checks the complete process/page/endpoint charge against committed use and every ceiling;
2. construction occurs outside this capability-free policy model;
3. `commit` accepts only the exact reservation, clears it, and advances committed and peak use; or
4. the caller discards the reservation record, exposing no committed object.

Checked subtraction avoids overflow at each ceiling. A second reservation is rejected while one is live. Release rejects any charge larger than committed use and never reduces peak evidence.

`reserve_status` and `finish_stop_status` expose the same rejection decisions as pure preflights. The live fixed transcript uses those scalar results where no replacement record may be published, while `reserve` and `finish_stop` remain the state-producing operations. This keeps rejection-before-exposure explicit and bounds aggregate-copy depth in the native policy context.

## Stop behavior

`stop` is idempotent. The first call records `Stopping` and the reason; later calls preserve that reason. New reservations are rejected after stop begins. `finish_stop` reaches `Dead` only when reservations and committed use are zero. Repeated stop or finish of a dead domain preserves the same terminal generation, zero current charge, peaks, and reason.

## Evidence and limits

The `os-resource-domain` native owner compiles the policy as an independent Project 2 module and executes rejection-before-exposure, exact reserve/commit, busy reservation, mismatched commit, peak retention, post-stop acquisition rejection, live-resource stop rejection, complete release, and repeated-stop/finish cases. The `os-process-policy` owner proves that the exact transcript composes into the link-facing native policy object, while `os-probe` pins all three current EFI identities. Live normal-boot verification additionally requires `resource-domain=pass current=0 peak=3/144/2` after the complete process path returns.

Version 1 does not yet count threads, handles, capabilities, queued messages, CPU, pinned/DMA/shared/guest memory, output, diagnostics, or teardown work. It gates the fixed Probe 40 transcript; it is not a general mutable kernel object, dynamic membership interface, public syscall contract, or substitute for reclaiming the machine's reserved recovery capacity. Those additions require a measured consumer and a new contract revision rather than silently widening this record.
