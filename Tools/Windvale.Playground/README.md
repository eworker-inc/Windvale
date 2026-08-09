# Windvale Playground

The Windvale playground is a static browser application over the experimental Windvale-native WebAssembly pipeline. Monaco remains the local editor. A disposable module worker loads an identity-pinned direct compiler Wasm and scalar interpreter Wasm, compiles one canonical `WVSS 1` source set to WVB, admits the returned bytes as untrusted input, and executes the verified scalar entry point. Source never leaves the browser.

The C# project and reusable engine remain in this directory only as documented Stage 0 recovery and comparison implementations. Normal local startup, browser startup, website verification, and Cloudflare publication do not start Blazor or a .NET runtime. The host contract is defined by [`Specifications/Browser-Playground.md`](../../Specifications/Browser-Playground.md).

Public playground: <https://windvale.ca/playground/>

## Native browser pipeline

One normal run performs this sequence inside a disposable worker:

1. fetch the revalidated package manifest;
2. fetch and SHA-256 verify the pinned direct compiler and interpreter Wasm;
3. reject imports and validate each exact ABI, fixed memory extent, and I/O region;
4. construct the canonical single-module `WVSS 1` input;
5. compile source directly in WebAssembly under a 2,000,000-instruction ceiling;
6. strictly parse the `WVCO 1` compiler result;
7. treat the returned WVB as untrusted input and resubmit it through `WVXI 3` with an explicit console grant; and
8. report status, bounded standard output, scalar result, instruction evidence, WVB bytes, and WVB SHA-256.

The default `Hello-Windvale.wv` proof produces 253 WVB bytes with SHA-256 `0a9230e700a10d14e718340e49562e5b0184a3c3a71b5cd29915126a6b28c28f`, writes `Hello from Windvale` plus LF, returns `0`, and requests zero .NET or Blazor assets. With its visible console grant disabled, the same module returns `WVR3010` before executing a guest instruction. The retained portable proof produces 183 WVB bytes, returns `42`, and exercises the same v3 response boundary without a capability. Initial download and WebAssembly compilation vary by browser, network, cache, and CPU; that timing is evidence rather than a portable performance contract.

The page retains multiple in-memory source tabs, local Monaco syntax support and completions, resizable editor and evidence panes, mobile layout, light/dark themes, and execution/diagnostic/bytecode views. The browser execution profile remains deliberately narrow: one UTF-8 source module, a scalar `Main` result, and at most the explicitly granted bounded `console.write_line` capability. This is ordinary framework code in the page and worker; it requires no Chrome or other browser extension. Unsupported execution surface fails explicitly.

## Run locally

From the repository root:

```powershell
npm ci --prefix Tools/Windvale.Playground
npm run build --prefix Tools/Windvale.Playground
npm run dev --prefix Tools/Windvale.Playground
```

Open <http://127.0.0.1:5174/>. For same-origin website integration, install the website dependencies and run `npm --prefix Website run dev`, then open <http://127.0.0.1:5173/playground/>.

The npm build regenerates the locally hosted Monaco bundle, copies the shared website analytics bootstrap, and copies only the two manifest-owned browser artifacts into the ignored `wwwroot/compiler-package/` publication path. It runs no compiler and does not require .NET.

## Publish static files

```powershell
npm ci --prefix Tools/Windvale.Playground
npm run build --prefix Tools/Windvale.Playground
```

Deploy `Tools/Windvale.Playground/wwwroot` below the website's `/playground/` path. The static host must serve `.wasm` as `application/wasm` and permit local module workers plus WebAssembly compilation. The scheduled website deployment builds the editor, verifies and copies the pinned package, and publishes this tree directly.

## Current boundary

- Source: one canonical `WVSS 1` root, strict UTF-8, at most 64 KiB.
- Compiler: import-free ABI 4 direct Wasm, 20,000,000 instructions, fixed 2,497-page memory, and a 16 MiB output region.
- Execution: import-free ABI 3 interpreter Wasm; user-selectable 10,000, 250,000, or 1,000,000 guest instructions; 200,000,000 outer instructions; 64 call frames.
- Capabilities: optional per-tab `console.write_line` grant; 65,536-byte all-or-nothing standard-output envelope; no other browser authority.
- Isolation: package loading, compilation, WVB admission, and execution occur in a disposable worker with a five-minute containment timeout.
- Evidence: pipeline status, bounded standard output, scalar result, elapsed time, compiler/execution counters, canonical WVB bytes and digest, and a zero-framework-request assertion.
- Editor: repository-built Monaco ESM with Windvale highlighting, completions, source tabs, `Ctrl+Enter` execution, and `Ctrl+M` or `Ctrl+;` insertion of `ˉ`.

## Artifact production and recovery

`Windvale-Compiler-Memory.wvb` and `Wvb-Scalar-Interpreter.wvb` have pinned Windvale-native source-compilation routes. `Wvb-Scalar-Interpreter.wasm` has a pinned native WebAssembly-lowering route. `Windvale-Compiler-Direct.wasm` is normally regenerated by the import-free segmented WebAssembly generator:

```powershell
node Tools/WebAssembly/Build-Direct-Compiler-Wasm.mjs
node Tools/WebAssembly/Build-Direct-Compiler-Wasm.mjs --check
```

The 88-second artifact-generation operation is a maintainer workflow, not part of browser compilation or website deployment. Reconstructing the pinned segmented generator Wasm still uses the explicit Stage 0 recovery seam. Normal artifact use, website build, deployment, browser compilation, returned-WVB admission, and execution are .NET-free.
