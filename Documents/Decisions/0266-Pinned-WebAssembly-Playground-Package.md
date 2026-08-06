# Decision 0266: Pinned WebAssembly playground package

- Date: 2026-08-05
- Status: Implemented with focused local Windows and Node.js evidence
- Follows: [Decision 0264](0264-First-Exact-WebAssembly-Hosted-Compilation.md)
- Target: `wasm32-browser-v1-experimental`

## Context

Exact compilation now succeeds inside WebAssembly, but the normal website cannot depend on ignored local probe artifacts. Rebuilding every browser artifact during an ordinary deployment would still invoke Stage 0: the pinned native source front door reproduces the scalar interpreter WVB, but its current hosted resource envelope does not publish the complete portable compiler project, and applying the Windvale-authored WebAssembly backend remains a Stage 0-hosted recovery operation.

These regeneration gaps must remain visible without forcing .NET into every website deployment. A fixed, digest-owned distribution input is preferable to an implicit copy from a developer's `artifacts/` directory.

## Decision

- Add `Artifacts/WebAssembly-Playground/` as the versioned package boundary for the exact portable compiler WVB, scalar interpreter WVB, and import-free scalar interpreter Wasm.
- Record exact byte lengths, SHA-256 identities, source projects, production provenance, source commit, and target in `Manifest.json`.
- Define normal website publication as verification plus byte-preserving copy. It must not start .NET or regenerate the package.
- Retain Stage 0 only for explicit recovery reconstruction of the compiler WVB and application of the WebAssembly backend until native or WebAssembly-hosted replacements are qualified. The manifest and package README name those seams directly.
- Verify every artifact identity, the Wasm import/export and fixed-memory ABI-3 contract, and exact one-instruction admission of the packaged compiler through the packaged interpreter before website publication.

## Consequences

Website packaging now has stable, repository-owned compiler inputs that can be consumed without .NET. This does not yet switch the editable playground from Blazor, nor does it claim a .NET-free regeneration closure. The next browser slice can depend only on the package manifest and transfer the complete compile/verify/execute pipeline into a disposable worker.

The checked-in binary cost is 1,846,726 bytes plus the small manifest and provenance document. This is bounded and cacheable, and avoids embedding roughly 2.5 MiB of base64 in JavaScript.

## Focused local evidence

The package verifier passes under Node.js's optimizing WebAssembly tier. It checks the 816,339-byte interpreter Wasm, the 919,577-byte compiler WVB, and the 110,810-byte native-built interpreter WVB against their manifest hashes. The import-free interpreter admits the compiler candidate, enters source compilation, and returns exact guest budget status `3011/1` after 77,103,665 outer instructions.

The ordinary native front door independently republishes the interpreter WVB with SHA-256 `58b21baac3f9a2c2f3bc52c0bb6c331230c300e81373380a9694b850373d87fe`. Attempting the complete compiler project through that same pinned application returns exit one without publishing a candidate, so the compiler regeneration seam remains documented rather than inferred closed.

## Rejected alternatives

Committing base64 JavaScript was rejected because it enlarges parsing and memory overhead without adding integrity. Copying ignored local artifacts was rejected because their provenance is not a repository contract. Rebuilding through `dotnet publish` or the Stage 0 CLI on every deployment was rejected because it preserves .NET in the normal artifact path. Claiming complete native regeneration was rejected by the measured compiler-project failure.

## Reconsider when

- The pinned native front door can reproduce the compiler WVB under an explicit qualified resource envelope.
- The WebAssembly backend can be applied without a Stage 0 host.
- Any packaged artifact or target contract changes.
- The browser worker package gains a content-addressed multi-version distribution format.
