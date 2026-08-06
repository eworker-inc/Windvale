# WebAssembly playground artifacts

This directory is the digest-pinned input package for the Windvale-native browser compiler. Normal website verification and publication validate and copy the two manifest-owned Wasm files without starting .NET. Compiler and interpreter WVB files remain pinned source provenance rather than browser downloads.

`Wvb-Scalar-Interpreter.wvb` is reproduced by the ordinary pinned native front door:

```bat
Tools\Native\Build-Wvb.cmd Windvale-Wvb-Scalar-Interpreter.wvproj output.wvb
```

`Windvale-Compiler-Memory.wvb` is reproduced by the digest-pinned standalone native compiler and separately pinned native publisher:

```powershell
node Tools/WebAssembly/Build-Compiler-Wvb.mjs
```

Use `--check` for temporary byte-exact reproduction without replacing the package. [Decision 0277](../../Documents/Decisions/0277-Native-WebAssembly-Compiler-Regeneration.md) records why this narrower compiler application fits while the combined project-aware build driver does not. Stage 0 remains only the explicit recovery route for reconstructing the pinned native compiler packages.

`Wvb-Scalar-Interpreter.wasm` is reproduced from that native-built WVB by the digest-pinned native WebAssembly compiler:

```powershell
node Tools/WebAssembly/Build-Interpreter-Wasm.mjs
```

Use `--check` for temporary byte-exact reproduction. [Decision 0278](../../Documents/Decisions/0278-Native-WebAssembly-Artifact-Regeneration.md) owns the bounded format-3 compiler-family application and exact output evidence. Stage 0 remains only the explicit recovery route for reconstructing the pinned native compiler packages; it is absent from normal browser artifact production, website build, and deployment.

`Windvale-Compiler-Direct.wasm` is reproduced from the portable compiler WVB by the digest-pinned segmented generator:

```powershell
node Tools/WebAssembly/Build-Direct-Compiler-Wasm.mjs
```

Use `--check` for temporary byte-exact reproduction. [Decision 0333](../../Documents/Decisions/0333-Segmented-Direct-WebAssembly-Compiler.md) owns the fixed segment protocol, direct compiler ABI, exact output, and retained recovery seam for reconstructing the generator Wasm. The browser worker rejects imports, validates ABI 4 for direct compilation and ABI 3 for returned-WVB execution, enforces fixed memory regions, and treats compiler output as untrusted WVB.
