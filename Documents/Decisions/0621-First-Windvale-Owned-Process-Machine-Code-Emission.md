# Decision 0621: First Windvale-owned process-machine code emission

- Status: Implemented current-Windows-host native candidate; process-machine migration active
- Date: 2026-08-15
- Advances: [Decision 0620](0620-First-Checked-Os-Provider-Launch-Transaction.md) and [Decision 0456](0456-Native-Probe-40-Process-Object.md)
- Contract: [checked x86-64 code emission](../../Specifications/Windvale-Os-X64-Code-Emission.md)

## Context

The filesystem and network images and their checked launch transaction are
Windvale-owned, but Probe 40 still has only three fixed process records. Its
768 KiB supervisor executable window has 464 bytes free, and directly composing
the provider policy with `Process-Foundation` reaches the native compiler's
bounded source-binding evidence boundary. Adding another opaque boot call would
therefore neither fit nor create the required memory, page-table, endpoint,
process, dispatcher, and teardown mechanisms.

Historical provenance confirms that only the 46,678-byte `.text.process`
section remains a reviewed architecture fixture. Its archived Stage 0 source
used one small x86-64 byte/label/fixup builder in front of cohesive process,
syscall, exception, and timer emitters. That reusable builder is the smallest
honest replacement seam.

## Decision

- Add one portable Windvale module that owns a 65,536-byte checked x86-64
  emission state, 256 numeric labels, and 512 relative fixups.
- Fail closed on code, label, fixup, condition, alignment, duplicate-label, and
  unresolved-label errors; preserve terminal failure states.
- Own exact immediate, placeholder, local call/jump/condition, RIP-relative
  `LEA RDX`, NOP alignment, and final two's-complement displacement behavior.
- Keep external WVO relocations separate and observable through scalar
  placeholder offsets.
- Use the primitive to source-own the exact 1,119-byte process entry and
  ready/wait dispatcher: preserve the eight nonvolatile registers, reserve its
  `0xe0` frame, initialize the dispatcher cursor to slot two, validate all three
  fixed process records and generations, scan ready/running records in bounded
  round-robin order, and return an explicit no-selection result.
- Respect the current accepted subset: avoid nested-record return values,
  compare condition opcodes as `u32`, and keep the test source graph linear.
- Do not change the current process fixture, EFI identities, or claim live
  filesystem/network providers in this slice.

## Evidence and consequences

The generic-emitter self-test WVB is 13,597 bytes at
`3bdfd99bb37c4ff037a2d57bfdd89e67a2f190df77f113b50effba1f9c6bd24f`.
The process-entry self-test WVB is 18,819 bytes at
`3d830d8788372bfb35e59f86f1cd2fce4bcbab38536d3e1da287f4cac4d15749`.
The native owner passes sixteen behavior groups with local results 50 and 51
and pins deterministic Windows and Linux console images. The complete emitted
entry/dispatcher is byte-identical to fixture offsets 0 through 1,118, whose
SHA-256 is
`f873105c2495a3fda6b0b26b0a0cb1a527f1bee042c4f77140ec683fdbba3bd8`;
the entry jump displacement is 1,082 and the coordinator begins at 1,119. The
retirement inventory is now 70 suites and 3,580 cases.

This removes no fixture bytes yet. It establishes the source-owned construction
primitive required to port them without restoring managed source to `main` or
turning raw decimal machine bytes into pretend source. The entry and dispatcher
are now source-owned, but they have not replaced fixture bytes.
[Decision 0622](0622-First-Windvale-Owned-Process-Coordinator-Initialization.md)
advances through coordinator context construction and the first exact WVO
relocation surface; channel/endpoint construction, syscall, exception, timer,
provider allocation, and live QEMU integration follow.

## Reconsideration triggers

Replace this builder with WVA only when WVA can express the process constructor
without hiding generated records or weakening exact relocation evidence. Raise
any limit only for a measured source-owned process-machine consumer.
