# WebAssembly playground artifacts

This directory is the digest-pinned input package for the Windvale-native browser compiler experiment. Normal website verification and publication may validate and copy these files without starting .NET. The package is not a claim that every artifact can already be regenerated without Stage 0.

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

The browser worker must validate the complete manifest identities, reject WebAssembly imports, enforce execution ABI 3 and its fixed memory regions, and treat compiler output as untrusted WVB. [Decision 0264](../../Documents/Decisions/0264-First-Exact-WebAssembly-Hosted-Compilation.md) pins the first exact source-to-WVB result. [Decision 0273](../../Documents/Decisions/0273-Warmed-WebAssembly-Compiler-Worker.md) owns the current extracted interpreter identity and its validated same-instance warmup.
