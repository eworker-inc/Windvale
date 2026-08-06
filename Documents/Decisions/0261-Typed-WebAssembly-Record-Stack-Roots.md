# Decision 0261: Typed WebAssembly record stack roots

- Date: 2026-08-05
- Status: Implemented with focused local Windows and Node.js evidence
- Follows: [Decision 0258](0258-Typed-WebAssembly-Record-Field-Roots.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The type-aware field collector preserves the exact portable compiler's post-frame boundary: a 600,000-instruction request still returns `WVR3017` at guest instruction 592,658 after 707,020,913 outer instructions. Current locals, saved frames, construction fields, and transitive record fields already use verified shapes. The operand stack remains the only domain whose complete eight-byte cells are conservatively compared with live record metadata.

The interpreter already carries two 32-bit descriptor masks for its bounded 64-cell operand stack. Those masks prevent scalar or nominal bit patterns from participating in text/bytes ownership. Record identity needs the same dynamic evidence because a record value can be produced by local load, construction, record field load, or a callee return and can be consumed by stores, calls, field access, pop, return, and other typed operations.

## Decision

- Add two 32-bit record masks parallel to the descriptor masks. Bit `i` identifies whether operand-stack cell `i` is a record value.
- Classify kind-7 local loads, record construction, kind-7 field loads, and record-valued returns as record-producing operations. Preserve the bit when a callee result is appended back to its caller's stack.
- Apply every operation's already derived consumed/produced effect to both mask families. Share the existing low/high truncation branches and masks so record typing adds no duplicate block family to the bounded native liveness planner.
- Seed record collection by visiting only stack cells whose record bit is set. Continue using verified local, frame, construction-field, and transitive-field shapes for every other root.
- Retain the fixed 4,096-byte arena, 512-byte mark vector, stable slot handles, first-fit reuse, guest charging, public execution ABI 3, and exact `WVR3017` when the typed live set has no adequate span.

## Consequences

Every admitted root domain now has type evidence. Scalar, enum, text, bytes, packed frame metadata, and dead record bit patterns cannot retain record spans. Collection remains bounded and deterministic but is no longer deliberately conservative for the currently admitted immutable, acyclic record model.

The mask adds two long-lived `u32` state values and one produced-kind discriminator. An initial mechanically duplicated truncation implementation crossed the native compiler's bounded basic-block/liveness-planning envelope and was rejected before publication. Sharing the descriptor truncation branches retains the same semantics and restores the mandatory no-.NET native build front door. This is a source-organization and bounded-control concern, not a reason to widen the native planner.

No C# product source changes. Native application and publication of the WebAssembly backend remain a separate Stage 0 retirement seam.

## Focused local evidence

The pinned Windvale-native build front door publishes a 110,649-byte three-function WVB with 108,058 code bytes, 1,032 compact root locals, and SHA-256 `ed57ee58216b09c92fef18a2bd60dfa4192b1ffb405a151df0c87b69647e1ee5`. The retained backend lowers it to 815,211 import-free ABI-3 Wasm bytes with SHA-256 `87759392e55e5955745e1c809462b76ec881dddb49647e4a49ea174603f10426`.

The false-frame-retention case completes twice in one instance with result 539 at guest instruction 2,285 and 3,160,421 outer instructions. The true 512-cell live-set fixture still returns exact `WVR3017` at guest instruction 4,332 after 5,768,869 outer instructions. The ownership pressure case still constructs 143,364 descriptor bytes and 1,136 record field cells, returning 69 at guest instruction 15,627. Text/bytes, formatting, SHA-256, one-short budget, reset, and seven malformed-request cases preserve their exact semantic results.

The 600,000-instruction compiler measurement has not yet been repeated against this exact-stack artifact. The next run must determine whether instruction 592,658 represents a true live set larger than 512 cells or the removed stack ambiguity; neither outcome is inferred from the focused probes.

## Rejected alternatives

Growing the arena was rejected before obtaining exact root evidence. Reusing descriptor bits for records was rejected because the two value classes can coexist and have independent ownership. Encoding record identity by changing the eight-byte value representation was rejected because it would alter stable handles and nominal equality. Duplicating the complete mask-maintenance control tree was rejected after it crossed the native compiler's bounded planning envelope.

## Reconsider when

- The exact compiler still reaches `WVR3017` with typed stack roots.
- Mutable or cyclic records change the reachability model.
- Operand-stack capacity exceeds 64 cells.
- A shared compact runtime type-map can replace the two independent mask families without weakening ownership evidence.
