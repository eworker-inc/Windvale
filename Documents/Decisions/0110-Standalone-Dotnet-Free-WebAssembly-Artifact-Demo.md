# Decision 0110: Standalone .NET-free WebAssembly artifact demo

- Date: 2026-08-02
- Status: Implemented and deployed with deterministic-engine, Chromium browser, and cross-host repository evidence; cross-browser qualification pending
- Extends: [Decision 0107](0107-Playground-Disposable-WebAssembly-Worker.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Decision 0107 proves that the browser can execute Windvale-generated WebAssembly in a disposable worker, but the editable playground starts Blazor and .NET before reaching that worker. The generated module itself has no .NET dependency. Keeping that distinction hidden makes the first lower execution layer harder to inspect and can imply that complete compiler replacement is required before any .NET-free browser evidence is possible.

The retained profile-3 success module already has an exact qualified identity and scalar execution contract. A separate static route can therefore expose that artifact directly without changing Windvale semantics, broadening the accepted WebAssembly profile, or weakening the existing differential playground.

## Decision

- Publish `/playground/wasm-demo/` beside the editable Stage 0 playground. The route uses ordinary HTML, CSS, and JavaScript and contains no Blazor bootstrap or .NET framework reference.
- Retain the exact 432-byte profile-3 module as base64 deployment data with SHA-256 `15f2d58746ff2b0ae33a0de05e2781949c9d908fab46dd4072bfe3b2fa42b0bb`. Decode it into the original bytes and check both size and SHA-256 in the browser before execution.
- Reuse Decision 0107's existing JavaScript host and disposable worker. The worker retains the 64-KiB input limit, independent WebAssembly validation, import rejection, exact ABI export check, one execution, scalar-only result boundary, two-second timeout, and unconditional termination.
- Require ABI `1`, status `0`, result `42`, and 30 attempted instructions. Report failure instead of presenting different evidence.
- Show the originating `.wv` fixture as read-only provenance. Editing and compilation remain in the Stage 0 playground until a Windvale-native browser compiler path exists.
- Verify the deployment representation independently under Node.js: reconstruct the exact bytes, check identity, validate the module, reject any imports, require the exact exports, and execute twice to prove result and counter reset behavior.
- Keep the claim precise: visiting and running this route requires no .NET runtime. Artifact construction and qualification still use the Stage 0 toolchain and remain outside this claim.

## Consequences

Windvale gains a small browser execution lane whose runtime dependency is the browser's WebAssembly engine rather than .NET. It provides a concrete intermediate milestone and a reusable destination for later qualified artifacts without displacing canonical WVB or the semantic oracle.

The embedded base64 is a deployment representation of an exact golden artifact, not a new module format. Canonical WVB remains the portable distribution contract, and `Specifications/Windvale-WebAssembly.md` remains the authority for the generated module.

This demo is intentionally not an editable playground, a browser compiler, a general WVB loader, a general WebAssembly backend, a capability ABI, cross-browser qualification, or .NET retirement from artifact production and repository automation.

## Initial evidence

`npm run verify:wasm-demo` reconstructs the exact 432 bytes, reproduces the pinned SHA-256, validates the import-free exact export contract under Node.js, and executes the same instance twice with ABI `1`, status `0`, result `42`, and 30 instructions after each reset. A zero-warning Release build and publication retain `wasm-demo/index.html` plus the shared worker in the published `wwwroot`. The focused Seed browser-playground engine case also passes.

A local Chromium-based in-app browser loads the direct HTML entry, reports the same identity and ABI tuple after one button action, and records no warning or error. Its observed asset inventory contains only the demo stylesheet, logo, analytics bootstrap, application module, artifact-data module, shared host, and shared worker; it contains no `_framework`, Blazor, or .NET asset. This local step does not by itself establish cross-browser or public-deployment qualification.

Exact implementation commit `e9480bca814318fb6fdcab0c4f3f1db699a01e6f` passes GitHub [Verify run 30767375189](https://github.com/eworker-inc/Windvale/actions/runs/30767375189): Windows and digest-pinned Debian 12 each complete the full repository qualification gate successfully. GitHub [Deploy homepage run 30767375190](https://github.com/eworker-inc/Windvale/actions/runs/30767375190) independently passes `verify:wasm-demo`, publishes the static route, and completes successfully. The public `windvale.ca` route then returns the exact ABI tuple in a Chromium-based browser with no framework asset or browser warning. This establishes deployment and one browser-engine family, not cross-browser qualification.

## Reconsider when

- The direct backend qualifies a new artifact that materially improves the demonstration.
- A Windvale-native verifier, interpreter, or compiler can replace another Stage 0 browser component.
- Static duplication of the retained bytes becomes less reliable than a build-time generated and independently pinned `.wasm` deployment asset.
- Cross-browser evidence requires changes to worker construction, cryptographic hashing, WebAssembly compilation, or timeout behavior.
