# Windvale Playground

This project is the experimental, fully client-side Stage 0 Windvale playground. The browser downloads .NET WebAssembly plus the existing C# reference compiler, canonical WVB verifier, and reference interpreter. For the bounded direct-Wasm subset, an embedded `.wv` backend lowers canonical WVB and a disposable browser worker executes the generated module for comparison. Source stays in the browser.

The host contract is defined by [`Specifications/Browser-Playground.md`](../../Specifications/Browser-Playground.md). The broader WebAssembly direction remains exploratory.

Public playground: <https://windvale.ca/playground/>

Direct .NET-free artifact demo: <https://windvale.ca/playground/wasm-demo/>

## Direct WebAssembly demo

The nested `wwwroot/wasm-demo/` route is ordinary static HTML, CSS, and JavaScript. It does not reference the Blazor bootstrap or any .NET framework asset. It reconstructs the exact 1,185-byte profile-6 artifact containing three functions and a maximum dynamic call depth of three, checks SHA-256, and sends it through the same disposable worker used by the differential playground. The page requires ABI `2`: budget 66 must return status `0`, result `42`, and 66 instructions; budget 65 must return status `3011` (`WVR3011`), result zero, and 65 instructions. It also requires zero .NET/Blazor resource requests.

The source displayed by this route is read-only provenance. The artifact was produced and qualified through the current Stage 0 toolchain, so this is a .NET-free browser execution path rather than a .NET-free compiler or build workflow. Run its independent static and engine checks with:

```powershell
npm run verify:wasm-demo
```

## Run locally

From the repository root:

```powershell
npm ci --prefix Tools/Windvale.Playground
npm run build --prefix Tools/Windvale.Playground
dotnet run --project Tools/Windvale.Playground
```

Open <http://127.0.0.1:5174/> to test the playground alone or <http://127.0.0.1:5174/wasm-demo/index.html> for the direct artifact route. To test the website and shared theme together, also run `npm run dev` at the repository root and open <http://127.0.0.1:5173/playground/>. Vite proxies that path to the Blazor server while preserving the website origin. The explicit `index.html` suffix bypasses the Blazor development server's fallback route; production static hosting exposes the clean `/playground/wasm-demo/` URL. The first two commands build the locally hosted Monaco editor bundle; they are required only after the editor dependencies or integration change. The first editable-playground browser download includes the editor, Stage 0 managed runtime, and compiler; the direct artifact route does not.

The current dated .NET 10 Release publication totals approximately 3.60 MiB across its Brotli-compressed representations, or 13.84 MiB for the corresponding uncompressed static files. The enhanced Monaco editor itself is approximately 1.03 MiB over Brotli. These local measurements are guidance, not a size contract.

## Publish static files

```powershell
npm ci --prefix Tools/Windvale.Playground
npm run build --prefix Tools/Windvale.Playground
dotnet publish Tools/Windvale.Playground/Windvale.Playground.csproj `
  --configuration Release `
  --output artifacts/playground
```

Deploy the contents of `artifacts/playground/wwwroot`, not its parent directory. The output has a relative base path and a `.nojekyll` marker, so it can live below the website's `/playground/` path. The static host must serve WebAssembly with the `application/wasm` media type.

The `Deploy homepage` GitHub Actions workflow builds this project, copies its published `wwwroot` into the website artifact at `playground/`, and publishes the combined static site to Cloudflare Pages. It does not add an application server.

## Current boundary

- Profiles: `portable` and bounded `hosted`; never `system`.
- Capabilities: `console.write`, `console.write_line`, and `diagnostic.write_line`, denied until checked.
- Limits: 64 KiB source, 250,000 instructions by default, 1,000,000 instructions maximum, 128 call frames, and 64 KiB for each output channel.
- Direct Wasm: capability-free portable programs are offered to the digest-pinned Windvale backend; supported output is import-free, at most 64 KiB, and runs in a fresh worker with both the selected WVB instruction limit and a two-second timeout.
- Evidence: compiler/runtime diagnostics, standard and diagnostic output, canonical WVB size and SHA-256, disassembly, profile, capabilities, exit code, instruction count, and selected Wasm/backend identities plus differential results.
- Editor: a local Monaco ESM bundle, Windvale-specific highlighting, contextual language completions, `Ctrl+M` insertion of the `ˉ` name separator, and compiler diagnostics projected as source markers. No editor asset is fetched from a CDN.

The current compiler, verifier, Windvale backend interpreter, and general reference interpreter run on the browser UI thread. The generated Wasm worker is one containment boundary; moving the remaining pipeline off the UI thread is still required before accepting arbitrary hostile public input.
