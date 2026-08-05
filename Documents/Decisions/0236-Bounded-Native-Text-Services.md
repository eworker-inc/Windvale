# Decision 0236: Bounded native text services

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0235](0235-Bounded-Static-Descriptor-Lowering.md)
- Contract: [Windvale-native x86-64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

After Decision 0235 admitted static borrowed descriptors, compiler-produced `Data-And-Text.wv` reached five remaining unsupported instructions: `text.concat`, `text.utf8_is_valid`, `text.from_utf8`, `text.quote`, and `bytes.concat`. The first four already have bounded ABI 22 runtime-service contracts. `bytes.concat` instead owns a substantially larger generated-arena and descriptor-generation template, so combining them would obscure its allocation and lifetime boundary.

## Decision

### Reuse the existing text-service boundary

Admit `text.concat`, `text.utf8_is_valid`, `text.from_utf8`, and `text.quote` without defining a second runtime contract. UTF-8 validation and conversion call the existing service-table slot at offset 40, concatenation uses offset 64, and quoting uses offset 72. Nonzero service results branch to ABI 22's runtime-service status. Failed `text.from_utf8` branches to the distinct invalid-UTF-8 status; success copies the complete source descriptor as a borrowed text view.

Concatenation and quoting publish their complete runtime-owned text descriptors only through their service output cells. Stack analysis requires exact input descriptor types, accounts for the resulting Boolean or text cells, and preserves the existing 2,048-cell frame limit. `bytes.concat`, generated descriptor allocation, descriptor parameters/returns, and capability services remain fail-closed.

Keep raw service-call machine-byte templates in the new focused `Native-X64-Lowering-Runtime-Descriptors.wv` module. The existing descriptor-instruction module owns typed stack transitions and result-cell selection. This keeps the large orchestration core from absorbing another byte-template family.

### Require exact differential and malformed evidence

Use `Wvb-To-Wvo-Text-Services.wv` as the focused vector. It concatenates two static texts, converts the result to bytes, validates and converts it back, quotes text, checks the two derived lengths, and returns 42. Stage 0 interpretation and native execution agree, while the Windvale memory adapter and hosted tool reproduce Stage 0's exact 5,140-byte WVO. A mutated validation instruction with a text-only operation is rejected before publication.

The source closure now exposes its existing UTF-8 validation use in the generated native tool's required-service set. Update the WVB-to-WVO host-bundle admission contract to the same established ten-service layout already carried by the bundle. This is a bounded Stage 0 packaging correction, not new source-language semantics.

The reviewed shared-backend and WVB-to-WVO selections are the only local verifiers for this coherent slice. Standard, Qualification, Linux execution, GitHub verification, and artifact promotion remain deferred to the grouped end-of-goal gate.

## Consequences

- Four of the five dynamic descriptor blockers measured after Decision 0235 now have Windvale-owned lowering.
- Runtime-service and invalid-UTF-8 status targets remain byte-identical to Stage 0.
- The current hosted tool is 158,172 WVB bytes and lowers through Stage 0 to 2,138,628 code bytes and a 2,144,658-byte WVO.
- The paired current package measurements are 2,156,544 bytes on Windows and 2,158,592 bytes on Linux; they remain unpromoted candidate evidence.
- No normal .NET dependency is removed by this local proof.

## Reconsideration triggers

Measure `bytes.concat` as the next exact `Data-And-Text.wv` blocker. Its slice must retain ABI 22 arena limits, owner generation, failure detail, and complete descriptor publication rather than treating it as another service-only text operation.
