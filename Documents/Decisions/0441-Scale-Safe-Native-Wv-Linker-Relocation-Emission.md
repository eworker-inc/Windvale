# Decision 0441: Scale-safe native Wv-Linker relocation emission

- Status: Implemented current-host candidate; dual-host qualification pending
- Date: 2026-08-09
- Advances: [Decision 0440](0440-Probe-40-Object-Inventory-Boundary.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)
- Contract: [Windvale native linker](../../Specifications/Windvale-Native-Wv-Linker.md)

## Context

The fourteen-input Probe 40 link contains 498 relocations and produces a
681,913-byte flat image. The retained Windows native linker admitted every
argument and object but returned `1` before publication. Exact in-process
execution of the same WVB and inputs identified the runtime failure:

```text
Native text arena exhausted its 134217728-byte limit in entry 'Main'.
```

The container already carried a 48,000,000,000-instruction budget and exited
in less than one second, so instruction exhaustion was not the blocker. Each
immutable four-byte patch previously constructed a complete replacement image;
the independent verifier repeated that pattern. Retained native descriptor
generations therefore exceeded the 128 MiB arena even though the final image
was smaller than 1 MiB.

## Decision

- Preserve production validation in input and source-relocation order so
  deterministic linker diagnostics do not change.
- Preserve a verifier-owned reverse input/reverse relocation validation pass
  with its separate provider lookup and signed-magnitude arithmetic.
- After validation, exploit WVO's required section/offset relocation ordering
  and walk canonical final placement order. Append borrowed unrelocated spans
  and borrowed one-byte table entries so each production and verifier result
  grows as one owned tail instead of retaining one complete generation per
  relocation.
- Return the already-built value immediately when no input contains a
  relocation.
- Keep the 4 MiB image-limit contract, but exercise that exact boundary through
  the native AOT linker test. The reference interpreter remains differential
  evidence for the ordinary semantic and rejection cases, not a performance
  oracle for the maximum native image.
- Change no WVO, linking, map, diagnostic, capability, or hosted-container
  contract. Keep the candidate behind digest-bound launchers until the same
  source and packages pass the independent Windows/Linux gate.

## Evidence and consequences

The resulting WVB is 135,740 bytes at SHA-256
`02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874`.
Its Windows application is 1,796,608 bytes at SHA-256
`c42b75a033fc79c5a967330e83fc498704840d2cb45723471a8c752dadf0b6e3`;
its Linux application is 1,798,144 bytes at SHA-256
`4007b083e7c612e4b7bb9e77d35625fa564c17a077a7183f3f489456468bf4fb`.

On the current Windows host, the regenerated native application links the
fourteen exact Probe 40 WVOs in 4.3 seconds, returns `0`, and publishes the
established 681,913-byte image at SHA-256
`76aa64cc03c8b86dfe96f83d761be40e8128b988a182fd971004a287a5990af0`.
The focused Windvale semantic/map contract and native AOT package contract
both pass; the latter also accepts the exact 4 MiB image boundary. No broad
Seed, OS, QEMU, Qualification, or Linux execution gate ran for this local
candidate.

The normal recovery command still uses the managed linker. Cut that command
over only after current upstream work is integrated and this exact candidate
passes on Linux as well as Windows. The large linker source remains a focused
organization concern; extract a cohesive module only when its shared view and
error-order invariants can remain explicit, rather than splitting it into
numbered fragments for line count alone.

## Reconsideration triggers

Reconsider the emission representation if a later mutable/affine byte builder
can preserve immutable publication semantics with simpler verified code, if a
valid canonical WVO can produce non-ascending final patch offsets, or if the
dual-host gate exposes a different arena/lifetime result.
