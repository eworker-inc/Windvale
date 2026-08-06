# Windvale Playground

The normal Windvale playground is a static browser application over the experimental Windvale-native WebAssembly pipeline. Monaco remains the local editor, while a disposable module worker loads the identity-pinned interpreter and portable source compiler, compiles one `WVSS 1` source module to canonical WVB, admits the returned bytes again, and executes the verified scalar entry point. The page does not start Blazor or a .NET runtime, and source never leaves the browser.

The C# project and reusable engine remain in this directory only as the documented Stage 0 bootstrap and recovery implementation. They are not used by normal local website startup, browser startup, website verification, or Cloudflare publication. The host contract is defined by [`Specifications/Browser-Playground.md`](../../Specifications/Browser-Playground.md).

Public playground: <https://windvale.ca/playground/>

Focused compiler proof: <https://windvale.ca/playground/webassembly-compiler/>

## Native browser pipeline

One normal run performs this complete sequence inside a disposable worker:

1. fetch the package manifest with revalidation;
2. fetch and SHA-256 verify the pinned compiler WVB and interpreter Wasm;
3. construct the canonical single-module `WVSS 1` request;
4. warm the same interpreter instance under an exact 100,000-guest-instruction budget;
5. compile source through the Windvale-authored compiler;
6. strictly parse the `WVXO 2` and `WVCO 1` result envelopes;
7. treat the returned WVB as untrusted input and resubmit it through `WVXI 1`; and
8. report status, scalar result, instruction evidence, WVB bytes, and WVB SHA-256.

The pinned Chromium proof produces 183 WVB bytes with SHA-256 `3d29618283648cb0d23987075912a218ac212d8c8fa31ec00b72f4bf3df795c6`, returns `42`, and requests zero .NET or Blazor assets. The focused proof completes in 85.9 seconds and the complete normal editor route in 101.3 seconds on the measured Windows browser. That latency is development evidence, not a portable performance contract.

The normal page retains multiple in-memory source tabs, local Monaco syntax support and completions, resizable editor and evidence panes, mobile layout, light/dark themes, and execution/diagnostic/bytecode views. The first native compiler profile is intentionally honest and narrow: one UTF-8 source module, a portable scalar `Main`, no host capabilities, and no standard-output channel yet. Other source can be edited and attempted, but unsupported surface fails explicitly.

## Run locally

From the repository root:

```powershell
npm ci --prefix Tools/Windvale.Playground
npm run build --prefix Tools/Windvale.Playground
npm run dev --prefix Tools/Windvale.Playground
```

Open <http://127.0.0.1:5174/> for the normal playground or <http://127.0.0.1:5174/webassembly-compiler/> for the focused compiler proof.

For the same-origin website integration, install the website dependencies and run `npm --prefix Website run dev`, then open <http://127.0.0.1:5173/playground/>. The website proxy preserves the public path while the internal playground server remains an ordinary Vite static server.

The npm build regenerates the locally hosted Monaco bundle, copies the shared website analytics bootstrap, and copies only manifest-owned, identity-verified compiler package artifacts into ignored publication paths. It runs no compiler and does not require .NET.

## Publish static files

```powershell
npm ci --prefix Tools/Windvale.Playground
npm run build --prefix Tools/Windvale.Playground
```

Deploy the contents of `Tools/Windvale.Playground/wwwroot` below the website's `/playground/` path. The directory has a relative base and `.nojekyll` marker. The static host must serve `.wasm` as `application/wasm` and permit local module workers plus WebAssembly compilation.

The scheduled deployment builds the editor, verifies and copies the pinned package, and copies this `wwwroot` tree directly into the same-origin website artifact. It no longer installs .NET or runs `dotnet publish` for the playground.

## Current boundary

- Source: one canonical `WVSS 1` root, strict UTF-8, at most 64 KiB.
- Profile: capability-free `portable` source whose verified `Main` returns an `i32` scalar.
- Compiler: fixed 2,000,000 guest and 1,800,000,000 outer instruction ceilings with a required 100,000-instruction warmup response.
- Execution: user-selectable 10,000, 250,000, or 1,000,000 guest instructions; 200,000,000 outer instructions; 64 call frames.
- Isolation: package loading, compilation, WVB admission, and execution occur in a disposable worker with a ten-minute host timeout.
- Evidence: pipeline status, scalar result, elapsed time, warmup/compiler/execution counters, canonical WVB bytes and digest, and a zero-framework-request assertion.
- Editor: a repository-built Monaco ESM bundle with Windvale highlighting, completions, source tabs, `Ctrl+Enter` execution, and `Ctrl+M` or `Ctrl+;` insertion of `ˉ`.

## Stage 0 recovery boundary

The retained C# project can still reconstruct and compare behavior during recovery and qualification. It is feature-frozen under Decision 0213 and must not become the normal website path again. The pinned portable compiler WVB and interpreter Wasm retain Stage 0 recovery provenance, while their normal regeneration routes are Windvale-native.
