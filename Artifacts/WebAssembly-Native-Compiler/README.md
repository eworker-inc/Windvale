# WebAssembly native compiler package

This digest-pinned package lets Windows and Linux regenerate the portable browser compiler WVB without starting .NET. The normal launcher verifies the selected native compiler and the existing native WVB publisher before it reads project sources or publishes output:

```powershell
node Tools/WebAssembly/Build-Compiler-Wvb.mjs
```

Use `--check` to reproduce the checked-in `Artifacts/WebAssembly-Playground/Windvale-Compiler-Memory.wvb` in a temporary directory and require byte-for-byte equality. Use `-o <output.wvb>` to publish elsewhere.

`wvcompile.exe` and `wvcompile.elf` are the existing format-3 hosted compiler profile. They load no CLR or .NET runtime. They accept the ordered root and source paths from `Windvale-Compiler-Memory.wvproj`, construct canonical `WVSS 1`, compile through Windvale source, and write one caller-owned candidate. The launcher then passes that candidate to the separately pinned native WVB publisher for verifier-admitted atomic replacement.

The native packages are reconstructed only through the explicit Stage 0 recovery route:

```powershell
pwsh -NoProfile -File Tools/Recovery/Rebuild-WebAssembly-Native-Compiler.ps1 `
  artifacts/recovered-webassembly-native-compiler
```

Recovery uses the feature-frozen C# bootstrap to rebuild the WVB and paired PE/ELF containers, then requires all three results to match this manifest. Recovery is not invoked by the normal compiler-WVB build, website build, website verification, or deployment path.
