# Decision 0607: Two-generation application-machine-construction admission

- Status: Implemented current-Windows-host native candidate; cross-host qualification pending
- Date: 2026-08-15
- Advances: [Decision 0606](0606-First-Live-Immutable-Application-Launch-Transaction.md)
- Contracts: [machine-construction policy](../../Specifications/Windvale-Os-Application-Machine-Construction-Policy.md), [application-launch policy](../../Specifications/Windvale-Os-Application-Launch-Policy.md), [process-policy object](../../Specifications/Windvale-Os-Process-Policy-Object.md), and [Probe 40](../../Specifications/Windvale-Os-Boot-Probe.md)

## Context

Decision 0569 made the first fixed Probe 40 child transactional but retained
the second generation on a direct reuse path. The transaction also accepted a
single construction boolean, so it did not prove which private machine objects,
page partition, mapping rights, capability-table capacity, or initial bindings
had been checked before publication.

The OS construction remains constrained by the frozen profile-5 runner and the
fixed 768 KiB supervisor RX window. Experiments with a packed request required
newer arithmetic operations that the runner rejects. Broadening that execution
profile solely to encode this policy would mix a runtime expansion into the OS
admission slice.

## Decision

- Add ApplicationMachineConstructionPolicy1 as a pure portable admission gate
  over explicit scalar inputs.
- Require the private address space, code/data/stack objects, observer, and
  initial thread; exact read+execute code; read+write non-executable data and
  stack; 4 through 64 capability slots; and all four required initial bindings.
- Retain the fixed 122-page LaunchPlan1 charge while admitting two different
  checked partitions: `110/8/4` for generation 1 and `100/18/4` for generation
  2.
- Route both generation-safe plan/process pairs through reserve, private machine
  admission, domain commit, and publication. No child identity is exposed
  before the matching charge and machine shape succeed.
- Turn the rollback transcript into an explicit writable-code rejection and
  require the exact two-process/22-page/two-endpoint base afterward.
- Keep the policy scalar under the frozen runner. Defer serialization and typed
  decoding until a user-callable kernel admission record is implemented.
- Advance the measured process-policy budget to 21,914 instructions and native
  context depth to 5 while retaining portable maximum dynamic source depth 4,
  ABI 22, context format 7, the kernel stack, and the fixed supervisor RX
  window. The native context additionally counts the exported entry frame.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Machine-construction self-test WVB | 2,513 | `f3537c3c2686cc852a83ca065ff14e6f449e08f7e638efe6a0f54d3857e071f0` |
| Process-policy WVB | 41,705 | `af183a9a7ce5b3504f4cde31ce54d2e5bcb656b1a9d177e66f4613c4ba5147df` |
| Link-facing process-policy WVO | 694,482 | `fcbd1493c1a59ca4121ab4205b02c76aeb95e08bdc09080ccb29385b7fe2011b` |
| Process architecture fixture | 46,678 | `16873575d4b38da97e164853e6a58725f0baf8d64ed67abca5d1decdb123d985` |
| Normal process WVO | 512,978 | `7b62466199fdfe9f55ea8c71f9c8b66ec766f1ad4706dbf159efa8c471b5b6a6` |
| Normal Probe 40 EFI | 1,248,256 | `c2f2b3d3d313ead4373a04697755a15f56dab835eaf787bc0cbff5ee0d803c88` |
| Invalid-opcode Probe 40 EFI | 1,248,256 | `f6226b76aadf1b356b5e63ff9c4ed3f2b11fd0b03977f6dd85716dc4141aee4e` |
| General-protection Probe 40 EFI | 1,248,256 | `205f7c40d29c18ee3d86774da9539187476c15da1c60e1f95081d3dc56004dbf` |

The standalone native runner returns 43 at exactly 997 instructions. The
composed runner returns 97 at exactly 21,914 instructions and preserves peak
domain use `3/144/2`. An initial record-rich composition placed the code tail
beyond the 768 KiB supervisor RX window, so the launch transition now consumes
the boolean result of an exact domain check retained by the process-composition
owner. The final normal code tail is byte 781,079, leaving 5,353 bytes in the
existing window; fault scenarios end at byte 781,095. The builder pins that
boundary as well as all three deterministic EFI identities. The first exact
native boot also proved that the entry-inclusive native depth must be 5 even
though the portable runner reports maximum source depth 4; the depth-4 native
context rejected before publication, while the minimal depth-5 context boots
the exact normal image through guest-controlled shutdown under pinned
QEMU/OVMF. Independent Linux construction and execution remain pending.

This advances the internal launch path, but it still cannot start an arbitrary
application. The executable identity and total charge are fixed; object
allocation and mapping remain in the existing machine seam; no serialized
request, public syscall, capability transfer, completion record, cancellation,
service supervision, or restart policy is added.

## Reconsideration triggers

Replace the two-plan allowlist when a verified executable publication can be
bound into a typed kernel admission request. Generalize total page charges only
with checked allocator and resource-domain consumers. Widen the runner only for
a separately measured language/runtime requirement, not to conceal this
policy's current scalar boundary.
