# WebAssembly playground artifacts

This directory is the digest-pinned input package for the Windvale-native browser compiler experiment. Normal website verification and publication may validate and copy these files without starting .NET. The package is not a claim that every artifact can already be regenerated without Stage 0.

`Wvb-Scalar-Interpreter.wvb` is reproduced by the ordinary pinned native front door:

```bat
Tools\Native\Build-Wvb.cmd Windvale-Wvb-Scalar-Interpreter.wvproj output.wvb
```

`Windvale-Compiler-Memory.wvb` remains the qualified Stage 0 recovery baseline. The current pinned native build-driver application does not yet publish this compiler project under its fixed hosted resource envelope. `Wvb-Scalar-Interpreter.wasm` is lowered from the native-built interpreter WVB by the Windvale-authored WebAssembly backend, but applying that hosted backend still uses the Stage 0 runtime during recovery reconstruction. Those are the remaining artifact-regeneration seams; neither is hidden in normal website deployment.

The browser worker must validate the complete manifest identities, reject WebAssembly imports, enforce execution ABI 3 and its fixed memory regions, and treat compiler output as untrusted WVB. [Decision 0264](../../Documents/Decisions/0264-First-Exact-WebAssembly-Hosted-Compilation.md) pins the first exact source-to-WVB result.
