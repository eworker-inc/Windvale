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
image pages, 17 private RW/NX pages, one process, and one endpoint under a
65-page ceiling. The final private page contains the tail of the 65,600-byte
service envelope that begins 1,024 bytes into private memory.

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
`38cb2ab5167bc5a1839bb47ce1de155c78854e85b888116e1600447d2d87a05e`.
The transaction and lifecycle self-tests are 30,694 bytes at
`e815598a078e8e8e5807a56e1651a21564df9db9b7d3c844fd44cfcf1f692def`
and 30,640 bytes at
`f70f571614e510bf973e5af8f67a7292af6bc35e5361fbcf558a2332e9e18613`.
The focused owner builds three projects and executes thirteen deterministic
behavior cases through two independent roots, returning 48 for construction
and 49 for lifecycle. The split avoids a source-graph diamond while preserving
one linear dependency chain per executable.

The current Probe 40 architecture fixture still has three fixed process slots.
This policy deliberately reuses one instead of claiming a nonexistent fourth
slot, but the fixture does not yet consume the transaction, perform the third
allocation generation, rebuild the page tables and record, advance either
endpoint, enter the user image, or dispatch a request. The embedded images now
name the admitted future endpoint generations. Direct source composition with
the nearly saturated process policy is rejected by the compiler's bounded
binding-evidence table, so the next privileged slice must source-own the
generation-three filesystem reconstruction before a live service is claimed.
