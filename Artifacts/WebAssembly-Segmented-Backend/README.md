# Segmented WebAssembly backend package

This package pins the import-free Windvale WebAssembly generator used to publish compiler-scale direct WebAssembly artifacts. Normal use is .NET-free: Node.js runs the pinned generator Wasm over a canonical WVB input and reconstructs its bounded segment manifest.

The generator WVB is reproduced through the ordinary pinned native source-compiler front door:

```bat
Tools\Native\Build-Wvb.cmd Windvale-WebAssembly-Scalar-Dispatcher-Segmented-Memory-Tool.wvproj output.wvb
```

The generator Wasm is a recovery artifact. Reconstructing it currently uses the qualified Stage 0 native-backend recovery seam because the preceding pinned direct backend does not admit a record-returning segment manifest. Normal compiler publication, website build, website verification, deployment, and browser execution consume the digest-pinned Wasm and do not start .NET.

Generate or byte-check the browser compiler through:

```powershell
node Tools/WebAssembly/Build-Direct-Compiler-Wasm.mjs
node Tools/WebAssembly/Build-Direct-Compiler-Wasm.mjs --check
```

[Decision 0333](../../Documents/Decisions/0333-Segmented-Direct-WebAssembly-Compiler.md) owns the segment contract, bounds, provenance, and browser cutover.
