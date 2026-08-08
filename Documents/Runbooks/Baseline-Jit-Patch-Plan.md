# Baseline-JIT patch-plan verification

The `WVJP 1` slice is a capability-free native test of the first typed
baseline-JIT lowering boundary. It verifies plan production and independent
materialization as data. It does not publish or execute the six generated code
bytes.

From the repository root on Windows:

```cmd
Tools\Native\Test-Baseline-Jit-Patch-Plan.cmd
```

From the repository root on Linux:

```sh
./Tools/Native/Test-Baseline-Jit-Patch-Plan.sh
```

Each launcher uses only the digest-bound native build, WVB-to-WVO, WVO
verification, linking, and console-packaging front doors. It derives the
application entry offset from the link map, requires the pinned value `3808`,
checks every intermediate identity, executes the packaged self-test, and
requires result `0` with no diagnostic output.

A passing run prints:

```text
native baseline jit patch plan status=Passed result=0 entry-offset=3808
```

The Windows and Linux launchers are paired host evidence. Passing only one host
does not establish cross-host qualification.
