# Decision 0289: Bounded WebAssembly interpreter warmup

- Date: 2026-08-06
- Status: Implemented with focused Windows-local Node.js and Chromium evidence
- Advances: [Decision 0273](0273-Warmed-WebAssembly-Compiler-Worker.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The normal browser playground warmed the WebAssembly interpreter by running the
exact 919,577-byte compiler guest for 100,000 instructions. That call repeated
the compiler's complete envelope validation, module preflight, and early
execution under the engine's baseline tier. It consumed 185,543,072 outer
instructions and about 31.8 seconds before the exact optimized compiler call
began.

The warmup exists to trigger WebAssembly tiering, not to establish a second
compiler result. A much smaller guest can exercise the same generated
interpreter root and bounded instruction-budget return without duplicating the
compiler's large cold preflight.

## Decision

- Add a capability-free, nonterminating scalar Windvale guest whose only
  observable product behavior is the interpreter's instruction-budget result.
- Build and publish its canonical WVB through the existing .NET-free native
  build-driver and verifier boundary. Pin its length and digest as a fourth
  browser-package artifact.
- Run that guest for 20,000 instructions before the compiler request on the
  same WebAssembly instance. Require exact `WVR3011`, the requested guest
  count, a zero scalar result, and a successful outer execution.
- Keep the compiler request, complete returned-WVB validation, execution
  request, worker isolation, fixed memory, artifact digest checks, and outer
  budgets unchanged.
- Treat the 20,000-instruction choice as a measured tiering margin rather than
  a portable WebAssembly guarantee. Re-measure supported browser engines before
  claiming a cross-browser performance contract.

## Exact evidence

The warmup source builds to 292 WVB bytes with SHA-256
`a529d370a1b0b1610c207eebd7ff5a14658f40867be9d4ca7244bc83f9cf81c8`.
At 20,000 guest instructions it returns exact `WVR3011` after 17,005,452
outer instructions in 7.211 seconds on the measured Windows host.

The following exact compiler call still completes in 1,183,292 guest and
1,513,523,789 outer instructions. It takes 58.271 seconds and publishes the
same 183 WVB bytes with SHA-256
`3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6`.
The complete local Chromium path takes 64.3 seconds, down from the preceding
90.0-second live-browser measurement without changing compiler semantics or
product dependencies. It returns scalar result `42`, preserves every exact
counter and WVB identity, and reports zero .NET or Blazor framework requests.

A 10,000-instruction loop did not trigger the optimizing tier on the measured
engine; 12,000 did. The retained 20,000 budget leaves margin above that local
threshold. These wall times are development evidence, not deterministic
format or execution contracts.

## Consequences

The first compile remains too slow for an interactive product experience. This
slice removes duplicated warmup work; it does not solve the interpreter's
1.5-billion-operation compiler execution. Direct compiler WebAssembly or a
substantially cheaper verified execution representation remains the real
latency boundary.

The website package grows by 292 bytes plus manifest metadata and remains
fully .NET-free in normal build, verification, deployment, and browser use.
Stage 0 remains recovery-only under the existing decisions.

## Reconsider when

- Chromium, Firefox, WebKit, or Safari tiering needs a different bounded
  threshold or does not optimize from the small guest;
- the interpreter root, compiler WVB, or browser engine changes materially;
- a direct compiler target makes tier warmup unnecessary; or
- a resumable verified interpreter can retain useful compiler state across
  bounded calls without weakening isolation or reproducibility.
