# Decision 0263: Measured WebAssembly record capacity

- Date: 2026-08-05
- Status: Implemented with focused local Windows and Node.js evidence
- Follows: [Decision 0261](0261-Typed-WebAssembly-Record-Stack-Roots.md)
- Target: `wasm32-browser-v1-experimental`

## Context

After every record root domain gained type evidence, the exact portable compiler still returned guest `WVR3017` at instruction 592,658. The exact-stack artifact completed normally at the outer layer after 724,196,654 instructions, so this was neither false retention nor an outer meter failure. The compiler's simultaneous live record fields genuinely exceeded the original 512-cell arena.

Capacity cannot be selected independently of descriptor ownership. A prototype 1,024-cell arena let the compiler pass the old boundary, but delayed record collection long enough that the retained ownership-pressure guest reached text/bytes heap `WVR3018` at instruction 13,759 instead of completing at 15,627. Record reclamation releases descriptor fields, so a larger record arena changes when the separate 64 KiB heap recovers storage.

## Decision

- Increase the guest record-field arena from 512 to 768 eight-byte cells: 6,144 field bytes, 6,144 metadata bytes, and a 768-byte mark vector.
- Derive bounds and suffix lengths from named `Recordˉcapacity` and `Recordˉmetadataˉlength` values. Build the reusable zero backing by powers of two, then slice metadata to its exact non-power-of-two length.
- Retain stable slot handles, address-ordered first fit, typed tracing, deterministic collection, the fixed 64 KiB descriptor heap, and exact `WVR3017` when 768 typed live cells leave no adequate span.
- Expand the false-retention and true-live-set fixtures from 32 to 48 sixteen-field records. The precision case must reclaim the dead first span and complete with all 768 cells genuinely live; the exhaustion case must fail when its already live 768 cells precede one more allocation.
- Keep 1,024 cells rejected until record and descriptor collection are coordinated explicitly rather than relying on arena pressure timing.

## Consequences

The exact portable compiler crosses its former 592,658-instruction record boundary. A minimal source-compilation request reaches an ordinary `WVR3011` at its explicit 600,000 guest budget, after 785,328,885 outer instructions, with no record or descriptor-heap failure. [Decision 0264](0264-First-Exact-WebAssembly-Hosted-Compilation.md) continues the same request to complete byte-exact `WVCO 1` success.

The current `Function-Only.wv` source is no longer a compiler-success fixture for this portable compiler baseline. It completes earlier with the stable 80-byte `Sourceˉbindings` diagnostic at guest instruction 163,256. `WebAssembly-Compiler-Success.wv` therefore pins a minimal accepted source separately.

The focused compiler probe now distinguishes an ordinary guest-budget response from a completed `WVCO 1` diagnostic or WVB result. When an expected Stage 0 WVB path is supplied, success requires kind zero and byte-for-byte payload equality; a valid diagnostic cannot be mistaken for compilation success.

No C# product source changes. The interpreter WVB continues to publish through the pinned Windvale-native build front door; applying the WebAssembly backend remains the separate Stage 0 seam.

## Focused local evidence

The native front door publishes a 110,810-byte three-function WVB with 108,217 code bytes and SHA-256 `58b21baac3f9a2c2f3bc52c0bb6c331230c300e81373380a9694b850373d87fe`. The retained backend lowers it to 816,339 import-free ABI-3 Wasm bytes with SHA-256 `fa819b439f8590bed96dba0dce2c1d79e4d5d41a9a3765dc5b6c37d54f601504`.

The 5,517-byte precision WVB has SHA-256 `c076fc60b6c6a79e1d9e7653006ec2c3582044eb1592afa1733385e250d7dcad`. It completes twice in one instance with result 1,171 at guest instruction 3,373 and 5,039,628 outer instructions. The 5,909-byte true-live WVB has SHA-256 `aa809a66f881c4b2369a1e338893491aef527a5157ccbfc8c13ec96789b6e7ce` and returns exact `WVR3017` at guest instruction 5,276 after 7,524,325 outer instructions.

The independent Stage 0 reference runtime completes that same true-live source normally with result 1,202 after 5,786 instructions. The difference from the bounded WebAssembly interpreter is therefore the selected 768-cell resource contract rather than invalid source or an unverified fixture.

The descriptor-ownership pressure case again succeeds at guest instruction 15,627 with result 69 after 74,059,771 outer instructions. Text/bytes, formatting, SHA-256, one-short budget, reset, and seven malformed requests preserve their exact semantic results.

The 100-byte minimal compiler source has SHA-256 `f2a1c48ae527b1b595b3097f67f8f0f666098a6ace9583e1ad7854d2006dbd9c`. Stage 0 emits an expected 183-byte WVB with SHA-256 `3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6`. The WebAssembly-hosted compiler reaches exact budget status `3011/600000` after 785,328,885 outer instructions; Decision 0264 records its later byte-identical completion.

## Rejected alternatives

Keeping 512 cells was rejected by exact typed-root evidence from the real compiler. Expanding directly to 1,024 was rejected because it regressed the independent descriptor-ownership workload. Inferring success from a completed guest with `WVCO` diagnostic kind one was rejected; compiler execution and compiler success are separate claims.

## Reconsider when

- A broader compiler workload reaches another typed record-capacity boundary.
- Descriptor-heap collection can request record tracing directly without waiting for record-arena pressure.
- The minimal source completes and its WVB differs from the Stage 0 oracle.
- Browser engines disagree on the exact capacity, meters, or result bytes.
