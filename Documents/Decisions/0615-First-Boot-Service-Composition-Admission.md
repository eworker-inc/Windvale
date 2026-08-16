# Decision 0615: First boot service composition admission

- Status: Implemented current-host native candidate; service launch and cross-host qualification pending
- Date: 2026-08-15
- Advances: [filesystem plan](../Project/Windvale-Filesystem-Implementation-Plan.md) and [networking foundation plan](../Project/Windvale-Networking-Foundation-Implementation-Plan.md)
- Contract: [boot service composition policy 1](../../Specifications/Windvale-Os-Boot-Service-Composition.md)

## Context

The filesystem semantic/service contracts, shared bounded-operation core, and
first network address/authority core have independent focused owners. Probe 40
did not yet name any of their version or capacity assumptions, so later service
launch could silently select incompatible limits even though each module passed
alone.

Directly importing every shared implementation into the current process-policy
project exceeds the present combined source-binding capacity and would duplicate
their focused behavioral proofs. The boot boundary instead needs a small exact
selection record while dynamic service processes and IPC remain separate work.

## Decision

- Compile boot-service-composition policy version 1 directly into the live
  portable process-policy root and require its exact token before token 97 can
  be returned.
- Pin filesystem contract version 1 and a 65,536-byte transfer ceiling.
- Pin an operation queue capacity of four with one control-reserved slot.
- Admit only IPv4 or IPv6, ports 1 through 65,535, and a nonempty subset of the
  four defined direction rights.
- Keep the complete filesystem, operation, and network behavior in their
  independently executed focused owners. This policy selects their shared
  envelope without copying their validators into the fixed 768 KiB supervisor
  window; it does not impersonate a provider or grant ambient authority.

## Evidence and consequences

The composed process-policy WVB is 42,027 bytes at SHA-256
`22e40a95100c635a2bf8980ee6f81f5660e3ac6bf2251a2355e5c9b6106e3d55`.
The unrenamed WVO is 699,368 bytes at
`46844c80221180e039cfb9d45ed2493486d1b026d9712517f64025db202100a9`;
the 699,394-byte link-facing object is
`dea015f8cafac002eddb9383691e2de10cbdcd0c0a589a88d88fbef95241f5b5`.
The current normal EFI is 1,252,864 bytes at
`ff6472065c681735e83e1365c2d149cf64f035173421a0a38366825d1ec7be2c`.
The invalid-opcode and general-protection EFIs are each 1,253,376 bytes at
`41138e57f27e476d75a43a062d939f36b597b49fe6b9b0cef8bd97032928665f`
and `e4157972cc89e7149d5a85dd6a2bbd4d8684d38546c4bce39b826e3e0256fe9e`.
The normal identity passes the pinned current-host QEMU 11.0/Q35/TCG boot
gate with the complete Probe 40 serial transcript and host exit code zero.

The portable policy returns 97 in 21,917 instructions. The reviewed process
fixture therefore admits 21,918 native-context steps including the exported
entry frame, at SHA-256
`112dd0cb06de269e069436720876829313fb0a20546d1d7b38ea336fea26e6fd`;
the reconstructed process object is
`c62399b8090ebf0412172bb01f40fe5f1ab659677f7ac58f6df02d397cc0b586`.

This establishes boot-policy dependency and deterministic identities. It does
not launch a filesystem or network process, decode a user start request, bind
an IPC endpoint or resource domain, parse FAT32, drive a link device, or process
packets.

## Reconsideration triggers

Replace the fixed arguments with a checked, versioned service-composition record
when the first dynamic provider is launched. Do not broaden the kernel or fold
provider semantics into this gate to avoid that explicit transition.
