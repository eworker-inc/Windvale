# Decision 0608: Versioned application-start and executable-publication admission

- Status: Implemented current-Windows-host native candidate; cross-host qualification pending
- Date: 2026-08-15
- Advances: [Decision 0607](0607-Two-Generation-Application-Machine-Construction-Admission.md)
- Contracts: [application-launch policy](../../Specifications/Windvale-Os-Application-Launch-Policy.md), [process-policy object](../../Specifications/Windvale-Os-Process-Policy-Object.md), and [Probe 40](../../Specifications/Windvale-Os-Boot-Probe.md)

## Context

Decision 0570 admitted two exact private machine layouts, but its launch
function still accepted a raw image identity and caller-selected process
reference. It did not distinguish an immutable executable publication from an
image number, prove caller authority, or version the request. Those omissions
would let a future syscall boundary accidentally inherit fixed-probe inputs as
an ambient interface.

The fixed supervisor RX window had only 5,353 bytes free. A six-field source
record prototype behaved correctly but added about 20 KiB of native code and
crossed that boundary. The checked serialized user-buffer layout and syscall
number are also still deliberately open architecture details.

## Decision

- Replace the raw launch-plan entry with `Application_start_admit`, one
  versioned, statically typed Windvale function over six `u32` mechanism
  values. This is a source ABI, not a native structure layout.
- Accept only request version `1`, init caller reference `65537`, resource
  domain reference `65537`, executable-publication reference `65576`, and
  admission profile `1` in the current measured profile.
- Interpret executable reference `65576` as identity/generation `40/1`. Reject
  raw identity `40`, stale generations, unauthorized callers, unsupported
  versions, wrong domains, invalid request generations, and broader capability
  profiles before resource reservation or child exposure.
- Let the caller supply request reference `65537` or `131073`; derive child
  reference as request plus one. A request cannot choose or reuse a child
  identity independently.
- Route both live Probe 40 generations and the failed-construction transcript
  through this same entry before machine admission and the existing atomic
  reserve/construct/commit/publish transition.
- Defer the architecture-neutral checked-buffer encoding and public syscall to
  the next machine-facing slice. That boundary must validate address, length,
  alignment, access direction, arithmetic, capability rights, and output
  publication without exposing a host or compiler record layout.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 42,005 | `9b61046a0413b655340eed67814eed8596b101107d5be6ce59f4dcc475ab207f` |
| Unrenamed process-policy WVO | 699,160 | `e66298754566209bd57fe56361da2d5064a4b487874db01b819dcbf8fdbab779` |
| Link-facing process-policy WVO | 699,186 | `c8dedae961b0fc200e9e678019879bfad9ed4d90a2f1086e4e8079a16827537d` |
| Normal Probe 40 EFI | 1,252,864 | `981e15e479828ba2475eb620b1ab20bf69dd3b7c2af51e3c2c0fc78b8e748bbd` |
| Invalid-opcode Probe 40 EFI | 1,252,864 | `c950b2ecd4db106a4b1f6a6e21394dec233943316446928960bdf5acffaf955c` |
| General-protection Probe 40 EFI | 1,252,864 | `3ba2b3310f30c897cddeea43a985a58c13d448a2fd244822fa0df751771d1c3f` |

The composed portable runner returns 97 at exactly 21,913 instructions and
maximum source depth 4. Native context depth remains 5. The link-facing WVO
grows by 4,704 bytes; the normal link tail ends at byte 785,783 and both fault
tails at byte 785,799, leaving 633 bytes in the fixed 768 KiB supervisor RX
window. The outer PE image crosses its next alignment boundary and grows by
4,608 bytes. The exact normal image boots under pinned QEMU/OVMF through both
application generations and guest-controlled shutdown.

The focused application-launch owner now reports eleven cases. It adds exact
success plus unsupported-version, unauthorized-caller, stale-publication, and
wrong-domain request evidence while retaining malformed-plan, rights-profile,
publication, rollback, and machine-construction evidence.

This is real executable-publication admission in the boot-composed policy, but
it still does not make arbitrary applications startable. There is no public
syscall, checked user-buffer decoder, dynamic executable inventory, arbitrary
image loader, dynamic object allocator, capability move, completion record, or
service manager yet.

## Reconsideration triggers

Replace the two-request allowlist only when dynamic request and child-object
allocation are checked together. Add a source record only after its measured
native cost fits the selected execution profile or a newer profile is accepted
for an independent reason. Do not treat the eventual wire request as the native
layout of any Windvale, C, or WVA record.
