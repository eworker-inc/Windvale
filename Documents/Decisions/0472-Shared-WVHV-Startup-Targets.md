# Decision 0472: Shared WVHV startup targets

- Status: Implemented current-host candidate; application admission consumer pending
- Date: 2026-08-09
- Advances: [Decision 0468](0468-Native-WVHV-Startup-Composition.md), [Decision 0471](0471-Native-WVHV-Direct-Execution.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier startup](../../Specifications/Windvale-Native-Hosted-Verifier-Startup.md)

## Context

The startup request producer already owned the exact ordered relocation targets
for both format-4 verifier templates. The next durable-publication step must
verify those same targets in a completed application. Copying the address table
would create two semantic owners precisely while retiring the managed oracle.

## Decision

- Extract target construction into one focused portable module.
- Keep target order, runtime offsets, import slots, service addresses, native
  entry calculation, and target counts unchanged.
- Make the existing request producer consume the shared result. The forthcoming
  format-4 application admission will consume that same result to check every
  relative relocation and normalized template digest.
- Do not add a second startup table or a large combined verifier module.

## Evidence and consequences

The existing reviewed focused test passes 1/1 in 8.814 seconds. It still
reproduces the exact 1,275-byte Windows and 668-byte Linux instantiated startup
code and retains invalid-object, destination-preservation, and alias coverage.

The module boundary changes the producer package identities without changing
its output contract:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Tool WVB | 64,198 | `36b6b638f14e0ecaff7ad8934ce785e22f9fa3d6c3cd5dcd91cfc98a4fa569d6` |
| Windows x64 | 701,440 | `4ae0435d62a1576f3b33573dd4402704ee35e4745dc2897f5dca1659899fd268` |
| Linux x64 | 700,416 | `04eeb622fdcfc0eec5fa030a1529657c6333a7842bb3244065959e40c7fc5c86` |

The hosted candidate remains 72 artifacts. Its 6,927-byte inventory now has
SHA-256 `7986dae7404baf5443758a2dc6d30df95337e4d3c35c44fc30181b98fd196161`;
all entries match. Including manifest and inventory, it contains 74 files
totaling 20,444,122 bytes. Targeted native packaging reconstructed only the two
changed applications. The unchanged packaging smoke and broad suites were not
rerun.

The format-4 application admission consumer, publisher reconstruction,
independent Linux execution, grouped qualification, promotion, and recovery
deletion remain. No broad Seed, OS, Standard, Qualification, WebAssembly, or
QEMU gate ran.

## Reconsideration triggers

Version or split the shared target model if startup object identity, patch
order, runtime geometry, import layout, service placement, or native-entry
addressing changes.
