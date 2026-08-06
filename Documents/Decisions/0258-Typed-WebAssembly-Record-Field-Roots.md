# Decision 0258: Typed WebAssembly record field roots

- Date: 2026-08-05
- Status: Implemented with focused local Windows and Node.js evidence
- Follows: [Decision 0257](0257-Typed-WebAssembly-Record-Frame-Roots.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Typed local and saved-frame roots advanced the exact portable compiler's 600,000-instruction request from guest `WVR3017` at instruction 572,612 to the same bounded status at instruction 592,658. The change removed one real false-retention source but left two conservative domains: the operand stack and every field cell copied from the record currently under construction or from a marked record.

Record field shapes are already fully verified during interpreter preflight. The record-construction handler has the exact target type and declaration cursor. A marked record's metadata contains its nonzero nominal type token and exact field count, and the verified type-offset table identifies the corresponding declaration. Treating scalar, enum, text, or bytes field cells as possible record handles therefore discards available type evidence.

## Decision

- Retain the fixed 4,096-byte record arena, 512-byte mark vector, stable slot handles, address-ordered first fit, and exact `WVR3017` live-set failure.
- During collection, walk the current construction's verified field declarations in lockstep with its field cells and append only kind-7 record values to the root worklist.
- When a record is first marked, recover its verified declaration from its metadata owner token, walk the exact field count, and append only kind-7 nested record values. Scalar, enum, text, and bytes fields cannot retain record spans.
- Preserve the existing descriptor-release pass when an unmarked record span is reclaimed. Record reachability and descriptor ownership remain separate bounded contracts.
- Keep operand-stack cells conservatively recognized at complete eight-byte boundaries until record identity receives its own stack mask or equivalent verified dynamic shape evidence.

## Consequences

Current locals, saved caller locals, construction fields, and transitive record fields now use verified types. Packed frame words and non-record field bit patterns no longer retain record storage. The collector remains partially conservative only for the bounded 64-cell operand stack, so exact-precision garbage collection is still not claimed.

No arena capacity, public ABI, WVB contract, guest charging, or C# product source changes. Source-level record fields currently reject other record types under the retained Seed semantic profile, so this slice does not broaden the language surface merely to create a nested-record fixture. The generic WVB interpreter continues to trace a verified kind-7 field if such an admitted canonical module reaches this profile.

## Focused local evidence

The pinned Windvale-native build front door publishes a 108,825-byte three-function interpreter WVB with 106,249 code bytes and SHA-256 `9ce32ed4dc0c9ca58495accd1b49b7a963e433ddf3c2c117753c08dd73beda6e`. The retained backend lowers it to 802,900 import-free ABI-3 Wasm bytes with SHA-256 `9058762b4e7dcbe9416ca923295105e002ca9bd35280b544f1913e37138bc5a9`.

The false-frame-retention case still completes with result 539 at guest instruction 2,285 and repeats with identical status and counters in one instance. Type-aware construction fields reduce its outer meter from 3,100,871 to 3,080,366. The true 512-cell live-set fixture still returns `WVR3017` at guest instruction 4,332, now after 5,627,095 outer instructions.

The existing ownership pressure case still constructs 143,364 descriptor bytes and 1,136 record field cells and returns 69 at guest instruction 15,627. Text/bytes, formatting, SHA-256, one-short budget, same-instance reset, and all seven malformed request probes preserve their exact results. A compiler-capacity measurement against this artifact remains the next evidence step rather than being inferred from the smaller probes.

## Rejected alternatives

Growing the arena was rejected because conservative roots remain removable using already verified evidence. Scanning every field as an untyped cell was rejected because it caused the measured false-retention boundary. Broadening Seed source semantics to permit nested record fields solely for a test was rejected; that language decision is independent of the generic interpreter's safe WVB handling.

## Reconsider when

- The compiler still reaches `WVR3017` after field-typed tracing.
- Operand-stack false retention becomes observable.
- Record-typed Seed fields are proposed with complete semantic and conformance coverage.
- Mutable or cyclic records require a different reachability contract.
