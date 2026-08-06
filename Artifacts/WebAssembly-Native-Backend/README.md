# WebAssembly native backend package

This digest-pinned package applies the Windvale-authored WebAssembly backend without starting .NET. The normal launcher verifies the selected native compiler before it reads the interpreter WVB, requires valid import-free execution-ABI-3 output with the exact browser export and memory contract, and publishes through a unique same-directory candidate:

```powershell
node Tools/WebAssembly/Build-Interpreter-Wasm.mjs
```

Use `--check` to reproduce the checked-in `Artifacts/WebAssembly-Playground/Wvb-Scalar-Interpreter.wasm` without replacing it. Use `-i <input.wvb> -o <output.wasm>` to publish another interpreter candidate. Rebuild the input WVB first through the ordinary native source front door when its Windvale sources change.

`wvwasm.exe` and `wvwasm.elf` are named members of the bounded format-3 compiler family. The dedicated artifact adapter uses the same exact six capabilities and ten services as the source compiler, the same 48-billion-instruction ceiling and 128 MiB dynamic arena, and no CLR or .NET runtime. It does not broaden format 3 into a general hosted-application profile.

The native packages are reconstructed only through the explicit Stage 0 recovery route:

```powershell
pwsh -NoProfile -File Tools/Recovery/Rebuild-WebAssembly-Native-Backend.ps1 `
  artifacts/recovered-webassembly-native-backend
```

Recovery rebuilds the WVB and paired PE/ELF containers through the feature-frozen C# bootstrap and then requires every identity to match this inventory. Recovery is not invoked by normal WebAssembly regeneration, website build, website verification, or deployment.
