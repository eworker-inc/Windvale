# Decision 0606: First live immutable application-launch transaction

- Status: Implemented current-Windows-host native candidate; cross-host qualification pending
- Date: 2026-08-15
- Advances: [Decision 0567](0567-Live-Probe-40-Resource-Domain-Accounting.md)
- Contracts: [application-launch policy](../../Specifications/Windvale-Os-Application-Launch-Policy.md), [resource-domain policy](../../Specifications/Windvale-Os-Resource-Domain-Policy.md), [process-policy object](../../Specifications/Windvale-Os-Process-Policy-Object.md), and [Probe 40](../../Specifications/Windvale-Os-Boot-Probe.md)

## Context

Probe 40's fixed ResourceDomain1 gate reserved and committed all three
processes before publication, but no immutable launch plan owned one child's
reserve, private construction, publication, and rollback. The OS-1 atomic
launch gate therefore remained designed but unimplemented.

The existing native image also retained a 768 KiB supervisor RX window. A
literal multi-field plan with separate record-producing phase functions did
not fit once linked with the process machine and kernel shims. Widening the
mapping merely to hide policy duplication would have invalidated the bounded
machine proof.

## Decision

- Add ApplicationLaunchPolicy1 as a compact immutable transition with explicit
  planned, reserved, constructed, published, rolled-back, and rejected states.
- Freeze the first measured admission inputs: domain `1/1`, image reference
  `40`, one process, 122 pages, rights profile `46 → 17`, three streams, one
  observer, and generation-safe plan/process references.
- Use one `advance` operation and one transition constructor for all phase
  changes. Only `Published` defines visibility; every other state yields no
  usable child identity.
- Compose the first client into the live ResourceDomain1 transcript. Retain
  init and directory as a two-process/22-page/two-endpoint base, reserve and
  privately construct the child, commit its charge, and publish the plan only
  after all checks succeed.
- Add a live failed-construction transcript. Discard the private domain
  reservation, retain no published child state, and require the exact base
  accounting record afterward.
- Keep the previously qualified second client generation on the direct reuse
  path. Generalizing more children is a successor pressure, not part of this
  first plan.
- Advance only the measured process-policy instruction budget to 20,911 while
  retaining depth 4, ABI 22, context format 7, the kernel stack, and the fixed
  768 KiB supervisor RX window.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 41,290 | `42e5fa16218402e9ed46e1b73200fdea6138b27f226fe5087508229150f92ff2` |
| Link-facing process-policy WVO | 696,014 | `1532e2b93684cb6dc9f450b375ecd49b2529578e130c822701b14d6668b55fa2` |
| Process architecture fixture | 46,678 | `f0ea8297edb42bc2cd9a1fd2db72a01e4e3e8bc4b66a4107b3661fd00af09310` |
| Normal process WVO | 512,978 | `05d703465cef6ffa38f11593620a134157a061d25631528e62b483de30a5a19f` |
| Normal Probe 40 EFI | 1,249,792 | `143c99b04789db2cfec2439d8964a2e9d5a83700280b5de4f996bed958d3d8c2` |

The focused application-launch test passes rejection, publication, and
rollback groups. The native runner returns 97 at exactly 20,911 instructions.
The exact normal EFI passes pinned QEMU 11.0/Q35/TCG on Windows, reaches the
existing `resource-domain=pass current=0 peak=3/144/2` and `status=pass`
markers, and powers off with host code 0. The invalid-opcode and
general-protection images are deterministically constructed and pinned; live
execution of those refreshed identities and independent Linux evidence remain
for broader verification.

This is the first live immutable launch transaction, not a public dynamic
launcher. It does not yet resolve or load an arbitrary application, construct
machine objects from a syscall, serialize plans, transfer capabilities,
publish structured completion, cancel a launch, or supervise/restart a child.

## Reconsideration triggers

Replace the fixed input tuple when a user-space semantic plan resolves a second
image or an explicit launch interface supplies the kernel admission record.
Add variable charges, transfer modes, cancellation, and completion only with
their first measured consumers. Requalify both hosts before promoting this
current-host candidate to the Probe 40 qualified baseline.
