# Decision 0257: Typed WebAssembly record frame roots

- Date: 2026-08-05
- Status: Implemented with focused local Windows and Node.js evidence
- Refines: [Decision 0197](0197-Bounded-Reclaiming-Wasm-Guest-Records.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The exact portable compiler continued beyond its earlier 100,000-instruction boundary, but a 600,000-instruction request returned guest `WVR3017` after 572,612 instructions. The outer interpreter completed normally after 701,005,862 instructions, so neither the guest instruction budget nor the outer Wasm meter caused the failure. The remaining boundary was the fixed 512-cell record arena.

Decision 0197 intentionally used a conservative collector. It concatenated current locals, the operand stack, packed saved frames, fields under construction, and nested record fields, then examined every four-byte-aligned pair as a possible `(record slot, nominal type)` handle. Saved frames contain return PCs and function indices alongside eight-byte local cells. Scalar locals also contain independently meaningful low words. Those bytes can accidentally equal a live record handle and retain dead spans.

The interpreter already records an exact eight-byte local shape entry for every verified parameter and local. Every saved frame ends with its caller function index, which identifies the frame's exact local length and shape-table offset. The collector can therefore distinguish record locals without changing WVB, execution ABI 3, the fixed arena, or the saved-frame representation.

## Decision

- Retain the 4,096-byte, 512-field-cell record arena, its stable handles, first-fit allocation, fixed mark vector, and exact `WVR3017` live-set failure.
- Seed collection with the operand stack and the current record-construction fields, whose values remain conservatively recognized in complete eight-byte cells.
- Walk current locals by the verified current-function shape table and append only kind-7 record cells as roots.
- Walk saved frames backward. Read the terminal caller function index, derive the exact local length and frame start from verified function metadata, and append only caller locals whose verified shape is kind 7. Never inspect packed return PCs, function indices, scalar locals, or descriptor locals as record handles.
- Examine candidate root cells at eight-byte boundaries. Marked records still append their field cells for bounded transitive tracing; enum and non-record field bit patterns may conservatively retain a matching record, so exact-precision collection is still not claimed.
- Pin both sides of the boundary: a false-frame-retention fixture must complete, while the existing true 512-cell live-set fixture must continue to return `WVR3017` at the same guest instruction.

## Consequences

The collector no longer lets packed call-frame metadata or scalar local pairs retain record spans. It remains deterministic and bounded, and it does not grow memory or rewrite stable handles. The change reduces false retention but does not prove that the compiler's measured 512-cell boundary is eliminated; the next compiler-capacity probe must determine whether its live set is legitimate or was retained by the former frame scan.

No C# product source changes. The current interpreter continues to build through the pinned Windvale-native front door. The retained WebAssembly lowering invocation is still the separately documented Stage 0 publication seam until a dedicated native WebAssembly backend application is qualified.

## Focused local evidence

The native front door publishes a 107,351-byte three-function WVB with 104,790 code bytes and SHA-256 `26573d67087f6b5a9e75334146a25f9fe1d3aff1e359e2c40780888ee6c0b85f`. The retained backend lowers it to 792,591 import-free ABI-3 Wasm bytes with SHA-256 `3c2a03151d57809790a49c530c581a095834162db179208b6d06777db602d82e`.

`Record-Arena-Frame-Precision.wv` first leaves one dead 16-field record at slot zero. Active and saved scalar locals then contain adjacent low values zero and one, which the former four-byte scan recognized as the dead record's `(slot 0, nominal type 1)` handle. Thirty-two genuinely live 16-field records subsequently occupy all 512 cells. The typed-frame collector reclaims slot zero, completes after exactly 2,285 guest and 3,100,871 outer instructions, and returns 539. Repeating the request in the same Wasm instance returns identical status, counters, and result.

The existing 4,405-byte live-set fixture retains all 512 field cells through actual record locals. It still returns exact `WVR3017` at guest instruction 4,332, now after 5,670,974 outer instructions. The focused ownership probe also preserves success for 143,364 cumulatively constructed descriptor bytes and 1,136 record field cells, plus text/bytes, formatting, SHA-256, budget, reset, and seven malformed-request cases.

## Rejected alternatives

Increasing the arena was rejected because the measured case first required distinguishing a legitimate live set from known conservative false retention. Parsing saved frames as untyped bytes was rejected because the verifier already supplies exact local shapes. Changing the frame wire layout or public execution ABI was rejected because backward traversal derives the required type information from existing verified metadata.

## Reconsider when

- The next measured compiler run still reports `WVR3017` with typed frame roots.
- Enum or stack values produce observable conservative false retention.
- A shared typed value-stack or heap ownership model can replace the remaining conservative stack and nested-field recognition.
- Mutable or cyclic records require a different tracing contract.
