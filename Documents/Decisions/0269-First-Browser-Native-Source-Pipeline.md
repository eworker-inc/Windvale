# Decision 0269: First browser-native source pipeline

- Date: 2026-08-06
- Status: Implemented with focused local Windows, Node.js, and Chromium evidence
- Follows: [Decision 0266](0266-Pinned-WebAssembly-Playground-Package.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The pinned playground package removes .NET from ordinary artifact publication, and the exact portable compiler already produces canonical WVB while hosted by the import-free interpreter Wasm. The website still lacked a browser boundary that could load that package, validate its identities, compile source, treat the compiler output as untrusted input, verify and execute it, and terminate independently of the page.

The same measured pipeline also carries a substantial performance cost. The current compiler needs 1,183,292 guest instructions and 1,513,529,072 outer interpreter instructions for the pinned 100-byte source. A successful browser proof must expose that cost honestly rather than implying that the normal editable playground is ready to switch.

## Decision

- Add a reusable JavaScript core for canonical `WVSS 1` construction, `WVXI 2` compiler execution, strict `WVXO 2` and `WVCO 1` parsing, returned-WVB resubmission through `WVXI 1`, and scalar result parsing.
- Reject interpreter imports, unexpected exports, the wrong ABI or fixed regions, growable memory, malformed responses, inconsistent lengths, non-WVB compiler output, excessive source, and invalid instruction limits.
- Load package artifacts in a module worker from the manifest, verify every byte length and SHA-256 identity, and return the successful WVB through a transferred buffer.
- Create and terminate one worker per request. The host owns a ten-minute wall-clock ceiling in addition to the interpreter's guest and outer instruction budgets.
- Add a static experimental probe that performs the exact source-to-result path without Blazor or .NET requests and shows elapsed time. It is evidence for the emerging pipeline, not a replacement for the normal editable playground.
- Run a fast static containment check during website verification and an exact optimizing-tier Node.js pipeline check during publication.

## Consequences

The website now contains a complete browser-native source-to-result seam over repository-owned artifacts. Ordinary deployment still copies already verified artifacts and does not run .NET. Compiler output crosses the same untrusted WVB admission boundary used for execution, and expensive work no longer blocks the browser UI thread.

The normal playground remains Stage 0. The current Chromium run takes minutes, the compiler source profile is still intentionally bounded, browser capabilities and general diagnostics are not integrated, and compiler and backend regeneration retain the recovery seams recorded by Decision 0266. Performance and surface expansion are required before replacing the editable Blazor route.

## Focused local evidence

The exact Node.js optimizing-tier check compiles the pinned source to 183 WVB bytes with SHA-256 `3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6`. Compilation uses 1,183,292 guest and 1,513,529,072 outer instructions. Resubmission verifies and executes in four guest and 8,554 outer instructions with status zero and result `42`.

The same static route passes in local Chromium through its real module worker, manifest and artifact fetches, WebAssembly instance, transferred result, and visible UI boundary. It produces the same byte identity and result in 378.1 seconds and records zero .NET or Blazor framework requests. This is focused browser evidence, not cross-browser or performance qualification.

## Rejected alternatives

Running the interpreter on the UI thread was rejected because the measured compile can deny page responsiveness for minutes. Trusting a successful compiler status without resubmitting its WVB was rejected because compiler output remains untrusted serialized input. Loading artifacts without digest checks was rejected because a mixed deployment could silently combine incompatible compiler and interpreter versions. Calling the six-minute browser result production-ready was rejected because the latency and bounded source surface do not meet the normal playground contract.

## Reconsider when

- The browser pipeline supports the editable playground's required source, diagnostic, inspection, and capability surface.
- Compiler execution is reduced enough for an interactive browser budget.
- Chromium, Firefox, WebKit, and real Safari evidence supports a broader browser claim.
- Native or WebAssembly-hosted production closes either remaining Stage 0 regeneration seam.
