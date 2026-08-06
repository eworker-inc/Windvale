# Decision 0273: Warmed WebAssembly compiler worker

- Date: 2026-08-06
- Status: Implemented with focused local Node.js and Chromium evidence
- Advances: [Decision 0270](0270-First-Browser-Native-Source-Pipeline.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The first exact browser-native source pipeline is semantically complete but not yet interactive. Its pinned 100-byte source compiles in 61 to 67 seconds when V8 starts with its optimizing WebAssembly tier, while a cold ordinary engine run takes about 355 to 378 seconds. WebAssembly tiering does not replace the active interpreter function during one long call on the measured engine, so the browser remains in baseline code until that call returns.

The scalar interpreter root is also a very large function. Cold preflight, record-metadata construction, and integer formatting enlarge that root even though they are cohesive operations outside the dominant instruction-dispatch loop.

## Decision

- Move nominal-type preflight into the existing envelope module.
- Build the fixed record metadata through a second envelope helper.
- Move integer formatting into a focused formatting module. Keep every helper capability-free and within the root-first `bytes -> bytes` WebAssembly graph.
- Before an exact browser compilation, run the same compiler and source request with a 100,000-instruction guest budget on the same WebAssembly instance. Require the ordinary `WVR3011` budget response, the exact requested guest count, and an empty result before submitting the full request.
- Report warmup and compilation counters separately. Keep the complete sequence inside the disposable worker and under its existing wall-clock timeout.
- Treat Stage 0 application of the Windvale-authored WebAssembly backend only as recovery reconstruction. Do not change C# product source.

## Consequences

The native front door publishes the extracted five-function interpreter as 112,216 WVB bytes with SHA-256 `6842a32e78ce8c6b347bd76b2a0da6dd4879dee4bb0580177bfb659f5323aa3a`. The current recovery lowering produces 839,104 import-free Wasm bytes with SHA-256 `f65c4e203d4b244ec52e0619f9d1a99ce1d2809296313cb154bba8316c6d916c`.

The digest-pinned browser package now owns those artifacts against source commit `f6db3cea965db39698cf5329210a7fa88498a673`. Normal website verification and publication still copy the checked identities without starting .NET.

The exact compiler output remains the same 183 WVB bytes with SHA-256 `3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6`, reached in 1,183,292 guest and 1,513,523,789 outer instructions. The extracted artifact completes in 61.469 seconds under the optimizing tier and 354.854 seconds as one cold ordinary call on the measured Windows host.

On the same ordinary engine, a 100,000-guest-instruction warmup returns `WVR3011` after 185,543,072 outer instructions in 31.755 seconds. The subsequent exact call completes in 58.136 seconds, for about 89.9 seconds total. This is a roughly 75 percent reduction from the single cold call without changing source semantics, counters, result framing, or output bytes. Wall time remains informative local evidence rather than a portable contract.

The same warmed sequence completes in 85.9 seconds in real Chromium, down from the preceding 378.1-second cold browser proof while preserving the exact WVB, counters, result, and zero-framework-request boundary. [Decision 0275](0275-Normal-Browser-Native-Playground.md) uses that measured result to promote the bounded worker into the normal editor while retaining Stage 0 only for recovery.

## Rejected alternatives

A two-bank immutable local-frame representation preserved exact output but increased the complete run to 1,544,851,156 outer instructions and did not improve optimizing-tier time, so it was reverted. A guarded low-opcode dispatch tail built natively but exceeded the retained WebAssembly backend's supported control shape, so it was also reverted. Neither experiment remains in product source.

## Reconsider when

- Browser remeasurement disagrees materially with the ordinary Node.js tiering result.
- A smaller bounded warmup reliably triggers optimization across supported engines.
- The interpreter root is replaced by a direct compiler target or another verified execution strategy.
- The package artifacts or compiler WVB identity change.
