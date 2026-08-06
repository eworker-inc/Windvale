# Decision 0275: Normal browser-native playground

- Date: 2026-08-06
- Status: Implemented with focused local static and Chromium pipeline evidence
- Advances: [Decision 0273](0273-Warmed-WebAssembly-Compiler-Worker.md)
- Target: `wasm32-browser-v1-experimental`

## Context

The warmed browser worker compiles the pinned source, strictly admits its returned canonical WVB, and executes result `42` in about 85.9 seconds in real Chromium. It reproduces the exact 183-byte WVB identity while requesting zero .NET or Blazor resources. That latency remains far from an ordinary interactive compiler, but it is bounded and practical enough for the project's current development-stage playground.

The public root playground still started Blazor and the managed Stage 0 compiler. Website development also ran the C# project, while scheduled deployment installed .NET and used `dotnet publish` merely to assemble static files. Keeping that path after the Windvale-native worker became usable would make the normal product surface depend on the recovery implementation and would delay exercising the native boundary.

## Decision

- Make the root playground an ordinary static HTML, CSS, Monaco, and JavaScript application over the existing browser-native compiler worker.
- Preserve source tabs, keyboard execution, Windvale syntax support, resizable panes, mobile layout, themes, execution settings, progress feedback, diagnostics, WVB inspection, and digest/instruction evidence.
- Present only the qualified single-module portable scalar profile as normal behavior. Keep arbitrary editing available, but report unsupported compiler or capability surface explicitly and never fall back to Stage 0.
- Keep package identity validation, the bounded warmup, source compilation, returned-WVB admission, and execution inside one disposable worker with the existing deterministic budgets and ten-minute host ceiling.
- Revalidate the package manifest and artifacts before accepting their exact sizes and SHA-256 identities. Do not allow a cached artifact from an older mutable filename to create a mixed package.
- Replace local Blazor startup with a static Vite server. During normal website publication, copy the built `wwwroot` directly; do not install .NET or invoke `dotnet publish`.
- Retain the C# playground sources and engine unchanged as documented Stage 0 recovery and differential evidence. Do not delete their implementation merely to make the normal route native.

## Consequences

Normal browser startup, local website development, website verification, and Cloudflare artifact assembly no longer require a .NET runtime. The normal page asserts the framework-request boundary at runtime and reports its count with execution evidence. The focused compiler route remains useful as a small pinned proof over the same worker.

The switch deliberately trades the old managed playground's broad language, hosted capabilities, output channels, source-located diagnostics, structural disassembly, and fast startup for direct use of the emerging native path. The UI labels that boundary. Expansion belongs in the Windvale compiler, interpreter, package, and diagnostic contracts rather than in a hidden managed fallback.

This decision does not close WebAssembly retirement. The pinned portable compiler WVB still records a Stage 0 recovery baseline, and the interpreter Wasm still records Stage 0-hosted recovery lowering. Normal publication only verifies and copies them. Native regeneration, qualification, and recovery-front-door closure remain required before the WebAssembly path is independent of .NET end to end.

## Focused evidence

- The Monaco/editor npm build and verified package copy complete without .NET.
- The static containment verifier checks both the root and focused compiler routes, rejects framework asset references, requires the worker/package/admission boundaries, and rejects .NET from normal local startup and deployment assembly.
- Real Chromium executes the shared worker pipeline in 85.9 seconds, publishes WVB SHA-256 `3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6`, returns `42`, and records zero .NET or Blazor requests.
- The complete root editor route repeats the same identity, counters, result, and zero-framework-request boundary in 101.3 seconds. Its Monaco editor, source-tab lifecycle, result navigation, WVB evidence, and browser log remain clean.

## Reconsider when

- compiler latency or source breadth makes additional normal examples honest;
- the native diagnostic envelope can expose stable source locations;
- the native path admits explicit hosted capabilities and bounded output channels;
- package artifacts use content-addressed public filenames; or
- the remaining Stage 0 regeneration seams close.
