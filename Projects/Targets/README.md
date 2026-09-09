# Explicit project build targets

These generated WVTD 1.0 inputs select a concrete build target for Project 4.
They are not inferred from the machine running the compiler and do not grant
capabilities or select an installed runtime.

The Windows and Linux x64 descriptors select **no foreign ABI**. They are for
source closures without foreign declarations. Whether the resulting WVB is
portable is derived from its source requirements, not the descriptor filename.

The registry and exact encoding belong to the
[target-descriptor specification](../../Specifications/Windvale-Language-1.0-Target-Descriptor.md).
Generate each file into a new path with the existing writer:

```text
node Tools/Native/Write-Canonical-Language-1.0-Target-Descriptor.mjs --target windows.x86_64.none_v1 Projects/Targets/Windows-X64-No-Foreign.wvtd
node Tools/Native/Write-Canonical-Language-1.0-Target-Descriptor.mjs --target linux.x86_64.none_v1 Projects/Targets/Linux-X64-No-Foreign.wvtd
```

The writer refuses to overwrite an existing file. To check reproducibility,
generate into temporary paths and compare bytes. The maintained Project 4
admission tests validate both target encodings and compile the same portable
consumer against each target. These inputs do not establish installed-toolchain
or full target qualification by themselves.
