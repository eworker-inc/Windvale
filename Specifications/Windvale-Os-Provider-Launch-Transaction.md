# Windvale OS provider launch transaction

## Status and scope

Provider launch transaction 1 is the portable construction and teardown gate
for the boot-embedded filesystem and network images. It composes `WVPR 1`
service-request admission with resource-domain policy 1. It is executable policy
evidence, not a public syscall, allocator, page-table implementation, endpoint
table, scheduler, or claim that Probe 40 launches either provider.

The filesystem profile keeps request reference `65540` and domain `65538`, then
binds them to generation-three process reference `196610` and generation-two
endpoint reference `131072`. It admits the exact 195,657-byte image, 48 RX
image pages, 33 private RW/NX pages, one process, and one endpoint under an
81-page ceiling. The first 17 private pages contain the context and complete
65,600-byte service envelope; the final 16 pages are a disjoint native stack.

The network profile keeps request reference `65541` and domain `65539`, then
binds them to generation-four process reference `262146` and generation-two
endpoint reference `131073`. It admits the exact 242,571-byte image, 60 RX
image pages, 36 private RW/NX pages, one process, and one endpoint under a
96-page ceiling. These first machine bindings are sequential: filesystem may
reuse process/object slot 2 only after client generation 2 is released, and
network may reuse it only after filesystem teardown. Their endpoint slots must
also be closed before generation advance. Image identity, image bytes, W^X
mapping evidence, endpoint binding, and readiness must all agree.

## Transaction

Construction follows one order:

1. validate the complete `WVPR 1` request and exact profile references;
2. require the exact embedded image identity and byte length;
3. require the exact generation-safe process and endpoint identities plus
   released process/object and closed endpoint slot evidence;
4. create the isolated generation-one resource domain and reserve its complete
   one-process/page/one-endpoint charge;
5. privately construct RX image mappings, RW/NX private memory, and the bound
   endpoint;
6. commit the exact reservation;
7. require provider readiness; and
8. publish the process as `Available`.

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

The provider policy builds as a 30,268-byte WVB at SHA-256
`9c69b5f8ae752367d6ad1052ada500a864a77d89ef1bede72daff5c48b0eaa6d`.
The transaction and lifecycle self-tests are 30,694 bytes at
`417bfb23392835a0fabd30760397ada1eb5b2a1ed93fa7b572ae3d40de840abd`
and 30,640 bytes at
`9900807157cdad54571983fba41d05ebd69d172b29ec5a7fe144c72104f52436`.
The focused owner builds three projects and executes thirteen deterministic
behavior cases through two independent roots, returning 48 for construction
and 49 for lifecycle. The split avoids a source-graph diamond while preserving
one linear dependency chain per executable.

The current Probe 40 architecture fixture still has three fixed process slots.
This policy deliberately reuses one instead of claiming a nonexistent fourth
slot. The generation-three filesystem record, W^X page tables, service copy,
and context setup are now source-owned by a separate focused three-case owner,
including the exact 85-physical/81-user-page distinction. The fixture does not
yet consume those constructors, perform the third allocation generation,
publish the rebuilt record, advance either endpoint, enter the user image, or
dispatch a request. The embedded images name the admitted future endpoint
generations. The next privileged slice must boot-link and execute the verified
filesystem reconstruction before a live service is claimed.
