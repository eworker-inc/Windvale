# Windvale Playground

This project is the experimental, fully client-side Stage 0 Windvale playground. The browser downloads .NET WebAssembly plus the existing C# reference compiler, canonical WVB verifier, and reference interpreter. Source stays in the browser.

The host contract is defined by [`Specifications/Browser-Playground.md`](../../Specifications/Browser-Playground.md). The broader WebAssembly direction remains exploratory.

Public playground: <https://eworker-inc.github.io/Windvale/>

## Run locally

From the repository root:

```powershell
dotnet run --project Tools/Windvale.Playground
```

Open the HTTPS or HTTP address printed by the development server. The first download includes the Stage 0 managed runtime and compiler; subsequent loads can use the browser cache.

The first measured .NET 10 Release publication totals approximately 2.77 MiB across its Brotli-compressed representations, or 9.20 MiB for the corresponding uncompressed static files. This dated local measurement is not a size contract.

## Publish static files

```powershell
dotnet publish Tools/Windvale.Playground/Windvale.Playground.csproj `
  --configuration Release `
  --output artifacts/playground
```

Deploy the contents of `artifacts/playground/wwwroot`, not its parent directory. The output has a relative base path and a `.nojekyll` marker, so the same publication can live at a custom-domain root or below a GitHub Pages repository path. The static host must serve WebAssembly with the `application/wasm` media type.

The `Deploy playground` GitHub Actions workflow publishes this directory to the repository's GitHub Pages project site after relevant changes reach `main`. A later custom domain can point to the same Pages deployment without adding an application server.

## Current boundary

- Profiles: `portable` and bounded `hosted`; never `system`.
- Capabilities: `console.write`, `console.write_line`, and `diagnostic.write_line`, denied until checked.
- Limits: 64 KiB source, 250,000 instructions by default, 1,000,000 instructions maximum, 128 call frames, and 64 KiB for each output channel.
- Evidence: compiler/runtime diagnostics, standard and diagnostic output, canonical WVB size and SHA-256, disassembly, profile, capabilities, exit code, and instruction count.

The current interpreter runs on the browser UI thread. Moving compilation and execution to a Web Worker is a hardening step before accepting arbitrary hostile public input.
