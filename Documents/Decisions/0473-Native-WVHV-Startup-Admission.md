# Decision 0473: Native WVHV startup admission

- Status: Implemented current-host candidate; completed-application admission pending
- Date: 2026-08-09
- Advances: [Decision 0472](0472-Shared-WVHV-Startup-Targets.md), [Decision 0470](0470-Native-WVHV-Container-Composition.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier container](../../Specifications/Windvale-Native-Hosted-Verifier-Container.md)

## Context

The format-4 composer admitted the startup response envelope and exact payload
length, but it trusted the preceding startup producer for code integrity. A
durable publisher must independently reject a changed relocation or template
byte before mutation. Decision 0472 made the producer's ordered relocation
targets reusable without copying its address policy.

## Decision

- Add a focused portable startup-admission module to the verifier composer.
- Check all 45 Windows or 24 Linux relative relocations against the shared
  runtime/import/service/native-entry target list.
- Zero those relocation fields in a private normalized value and require the
  retained canonical template SHA-256.
- Reuse the existing streaming SHA implementation already required for bundle
  admission. Do not add a parallel hash path or duplicate target table.
- Reject startup failure through the existing phase-3 composer result before
  writing an output.

## Evidence and consequences

The reviewed focused container test passes 1/1 in 21.352 seconds after the
incremental build. It retains exact Windows/Linux application equality, direct
Windows composer and verifier execution without .NET, bundle-digest rejection,
destination preservation, and alias rejection. A one-byte change in the first
Windows startup relocation is now separately rejected at phase 3 while the
sentinel destination remains unchanged.

The strengthened composer identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Tool WVB | 69,165 | `908dd3261d4075ee0f34a5976832e81f6bd16e742caf9469b48bcad43c773872` |
| Windows x64 | 1,088,000 | `a84e7aac58ce5d1f41ffb82efd0bf4c4fceb6cabdf9515d919a160a39e94a9ff` |
| Linux x64 | 1,089,536 | `b2d8f2a3fe23f974ee23c313840d14f195f0043a7a237c119d22ff7d2ae3d304` |

The hosted candidate remains 72 artifacts. Its 6,927-byte inventory has
SHA-256 `a7eb43d58a81ee57881f800b2c17b70c2014c26ce4454fa299feb2986348fb58`;
all entries match. Including manifest and inventory, it contains 74 files
totaling 20,990,843 bytes. Targeted native packaging reconstructed only the two
changed applications. The unchanged packaging smoke and broad suites were not
rerun.

Standalone extraction and admission of a completed format-4 application,
publisher reconstruction, independent Linux execution, grouped qualification,
promotion, and recovery deletion remain. No broad Seed, OS, Standard,
Qualification, WebAssembly, or QEMU gate ran.

## Reconsideration triggers

Version this boundary if the startup templates, relocation encoding or order,
runtime geometry, import slots, service layout, or native-entry policy changes.
