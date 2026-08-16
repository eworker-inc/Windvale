# Windvale OS provider launch transaction

## Status and scope

Provider launch transaction 1 is the portable construction and teardown gate
for the boot-embedded filesystem and network images. It composes `WVPR 1`
service-request admission with resource-domain policy 1. It is executable policy
evidence, not a public syscall, allocator, page-table implementation, endpoint
table, scheduler, or claim that Probe 40 launches either provider.

The filesystem profile admits process reference `65540`, domain and endpoint
reference `65538`, the exact 195,657-byte image, 48 RX image pages, 17 private
RW/NX pages, one process, and one endpoint under a 65-page ceiling. The final
private page contains the tail of the 65,600-byte service envelope that begins
1,024 bytes into private memory. The network profile admits process reference
`65541`, domain and endpoint reference `65539`,
the exact 242,571-byte image, 60 RX image pages, 36 private RW/NX pages, one
process, and one endpoint under a 96-page ceiling. Image identity, image bytes,
W^X mapping evidence, endpoint binding, and readiness must all agree.

## Transaction

Construction follows one order:

1. validate the complete `WVPR 1` request and exact profile references;
2. require the exact embedded image identity and byte length;
3. create the isolated generation-one resource domain and reserve its complete
   one-process/page/one-endpoint charge;
4. privately construct RX image mappings, RW/NX private memory, and the bound
   endpoint;
5. commit the exact reservation;
6. require provider readiness; and
7. publish the process as `Available`.

A construction failure discards the unpublished reservation and proves that the
empty domain reaches `Dead`. A readiness failure releases the committed charge,
stops the domain, and proves the same terminal state. No failure returns a
process reference or committed page count.

Teardown changes `Available` to `Draining`, rejects a stale process reference,
and refuses completion while work remains. Successful completion stops the
domain, releases its exact complete charge, reaches `Dead`, clears the process
reference, and returns zero committed pages. Version 1 performs no automatic
restart and never replays a request.

## Evidence and limits

The combined lifecycle policy builds as a 28,419-byte WVB at SHA-256
`7db47678e01b52473084fe65fc5430bb7b6e8c4e960ae6f6dd032aeab50f04f4`.
The focused owner builds three projects and executes ten deterministic behavior
cases through two independent roots, returning 48 for construction and 49 for
lifecycle. The split avoids a source-graph diamond while preserving one linear
dependency chain per executable.

The current Probe 40 architecture fixture still has three fixed process slots.
It does not consume this transaction, allocate provider pages, install their
page tables, create their endpoints, enter their user images, or dispatch a
request. Direct source composition with the nearly saturated process policy is
rejected by the compiler's bounded binding-evidence table, so the next
privileged slice must split or replace that fixed boundary and bind these
admitted values to real machine records before a live filesystem or network
service is claimed.
