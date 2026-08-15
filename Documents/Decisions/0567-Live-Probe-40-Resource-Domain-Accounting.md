# Decision 0567: Live Probe 40 resource-domain accounting

- Status: Implemented current-Windows-host native candidate; cross-host qualification pending
- Date: 2026-08-15
- Advances: [Decision 0196](0196-First-Generation-Safe-Non-Tail-Memory-Object-Reclamation.md) and [Decision 0198](0198-Next-Integrated-Architecture-Defaults.md)
- Contracts: [resource-domain policy](../../Specifications/Windvale-Os-Resource-Domain-Policy.md), [process-policy object](../../Specifications/Windvale-Os-Process-Policy-Object.md), and [Probe 40](../../Specifications/Windvale-Os-Boot-Probe.md)

## Context

Resource-domain policy 1 already implemented immutable reserve, commit,
release, stop, and finish transitions, but it remained a standalone model.
Probe 40 published three processes, 144 ordinary process-owned pages, and two
service endpoints without requiring that policy transcript first. The OS-1
roadmap therefore still had no live accounting gate between admission and
publication.

The existing process-policy native context also had exact limits of 16,384
charged WVB instructions and call depth 3. Composing record-valued policy
operations measured 17,493 instructions and depth 4. Silently escaping either
limit or widening the shared ABI would invalidate the resource proof.

## Decision

- Compose `Resource-Domain-Policy.wv` into the existing process-policy project.
  Token 97 now requires the original process foundation and the complete fixed
  ResourceDomain1 transcript before the process machine publishes any object.
- Charge exactly three processes, 144 ordinary pages, and two endpoints. Keep
  the thirteen kernel and recovery pages outside the ordinary domain.
- Reject an over-limit aggregate before construction, reserve the complete
  accepted aggregate, and commit it only before publication. Release the first
  122-page client generation, reserve and commit its replacement, and retain
  exact peaks of 3 processes, 144 pages, and 2 endpoints.
- Stop with reason 40, reject later reservation without changing committed
  use, refuse terminal completion while resources remain live, release the
  client, directory, and init charges to zero, and require repeated finish to
  preserve `Dead`, zero current charge, peaks, generation, and reason.
- Keep pure reserve and finish preflights in the same policy module. They expose
  rejection decisions without publishing a replacement transition and keep
  the live aggregate path bounded.
- Advance only the reviewed normal process architecture fixture's policy
  context to the measured 17,493-instruction and depth-4 budgets. Retain the
  kernel stack, context format 7, ABI 22, every other process byte, and the
  fixed 768 KiB supervisor RX window.
- Emit `resource-domain=pass current=0 peak=3/144/2` only after the complete
  process path returns, and make the normal QEMU verifier require that marker.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 33,786 | `26a540bc1435114608aa597545c805e0786c9593b6e8ba19e8919b9f7718b0c1` |
| Link-facing process-policy WVO | 583,416 | `4d3ffefc6be3c4edb48f1032415d96987bbd62899cdadd1fb4f0dc91ca319428` |
| Process architecture fixture | 46,678 | `993827337a101660107742e4f41de5d2df849517d4aa16bb919d708531592bfc` |
| Normal process WVO | 512,978 | `e9e77ec2550f7e6c8e853a622f0f34a6f932c7c0ed73022d2bca57f1922f239a` |
| Kernel marker WVB | 1,581 | `795734982cded8b3605cb5cf0f110667b71140d5639185c3ef94cde3174b3bc0` |
| Normal Probe 40 EFI | 1,137,152 | `3edd328fb014fe51708513594672a72bb245617b4950275f1b1b04b566c4cd06` |

The native WVB runner reports result 97 at exactly 17,493 instructions. The
focused resource-domain and process-object owners pass locally. The exact
normal EFI passes the pinned QEMU/OVMF gate on Windows, emits the new accounting
marker, reaches `status=pass`, and exits through guest-controlled poweroff with
host code 0. Invalid-opcode and general-protection construction retain their
terminal behavior under refreshed exact identities. Independent Linux
execution and the complete dual-host qualification gate are not claimed.

This decision implements a fixed live accounting gate, not a general mutable
kernel resource-domain object. It does not add dynamic membership, threads,
handles, capabilities, queued messages, CPU, DMA/pinned/shared memory, work,
general rollback, a public syscall ABI, service supervision, or restart policy.
The policy's terminal zero charge is accounting evidence; it does not claim
that Probe 40 dynamically reclaims every init or directory machine object
before firmware shutdown.

## Reconsideration triggers

Replace the fixed transcript when the first immutable launch plan owns a real
reserve/construct/publish/rollback transaction. Revise the record only for a
measured capability, work, memory, or supervision consumer. Requalify both
hosts before promoting this current-host candidate to the Probe 40 qualified
baseline.
