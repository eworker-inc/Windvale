# Decision 0264: First exact WebAssembly-hosted compilation

- Date: 2026-08-05
- Status: Implemented with focused local Windows and Node.js evidence
- Follows: [Decision 0263](0263-Measured-WebAssembly-Record-Capacity.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The type-traced 768-cell interpreter crossed every earlier record and descriptor-storage boundary, but the first retained minimal compiler request had only reached ordinary guest-budget status at 600,000 instructions. A completed `WVCO 1` result was still required before source-to-WVB compilation could be claimed inside WebAssembly. A completed diagnostic is not success, and an ordinary budget response proves progress but publishes no compiler artifact.

## Decision

- Retain `WebAssembly-Compiler-Success.wv` as the 100-byte minimal accepted source and its Stage 0-produced 183-byte WVB as an independent byte oracle.
- Execute the exact 919,577-byte portable compiler through the import-free ABI-3 interpreter using the version-2 byte-entry protocol and canonical one-source `WVSS 1` envelope.
- Count compilation as successful only when the outer run succeeds, the guest returns status zero, the result is a canonical `WVCO 1` kind-zero envelope, and its complete payload is byte-identical to the oracle WVB.
- Pin the first completion point at 1,183,292 guest instructions and 1,513,529,072 outer instructions. A two-million guest and three-billion outer ceiling is only the bounded request envelope; execution returns as soon as compilation completes.
- Use V8's optimizing WebAssembly tier for long measurement runs. Engine-tier selection changes elapsed time, not Windvale instruction counts, result framing, or artifact bytes.

## Consequences

Windvale source now compiles to verified WVB entirely inside a host WebAssembly engine. The run itself does not invoke .NET, and its output exactly matches the Stage 0 recovery oracle. This establishes compiler execution and artifact publication through `WVSS 1 -> WVXO 2 -> WVCO 1`; it does not yet make the website's normal artifact-production path .NET-free.

The remaining retirement seam is integration and provenance: publish the interpreter and compiler inputs through the pinned Windvale-native build path, package their exact identities for the website, run verification, compilation, and result execution inside a disposable browser worker, and retain Stage 0 only as recovery and independent evidence.

No C# product source changes.

## Focused local evidence

The 816,339-byte interpreter Wasm has SHA-256 `fa819b439f8590bed96dba0dce2c1d79e4d5d41a9a3765dc5b6c37d54f601504`. It executes the 919,577-byte compiler WVB, SHA-256 `2bf84dc2a8cbb80c52ec7fb6cb2e29eef27def1707f398a276c61063d73df06e`, over the 100-byte source, SHA-256 `f2a1c48ae527b1b595b3097f67f8f0f666098a6ace9583e1ad7854d2006dbd9c`.

The completed response is 219 bytes: a 20-byte `WVXO 2` success envelope containing a 199-byte `WVCO 1` kind-zero result whose 183-byte payload has SHA-256 `3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6`. That payload is byte-identical to the independently compiled Stage 0 WVB. Earlier exact runs returned ordinary budget status at 750,000 guest / 949,915,429 outer instructions and 1,000,000 guest / 1,283,600,015 outer instructions, so completion is bounded between the latter point and 1,183,292 without an intervening resource failure.

## Rejected alternatives

Treating any completed `WVCO 1` diagnostic as success was rejected because kind one publishes no WVB. Treating a guest-budget response as success was rejected because it has no result payload. Increasing an arena again was rejected because the 600,000, 750,000, and 1,000,000 checkpoints all returned ordinary budget status rather than a resource failure. Using elapsed host time as semantic evidence was rejected because engine tiers materially change it while exact Windvale counters remain stable.

## Reconsider when

- The portable compiler WVB or minimal source fixture changes.
- Another browser engine disagrees on the exact response or artifact bytes.
- The website package can no longer reproduce the pinned interpreter and compiler identities through the native front door.
- A broader source exercises a capability, control-flow, or memory contract outside the retained profile.
