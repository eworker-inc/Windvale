# Native baseline-JIT publication verification

The baseline-JIT publisher is a bounded native W^X test. It composes exact
`WVJP 1` patch-plan admission with the exact six-byte `WVLT 1` lifetime, then
publishes, invokes, and releases generated code through the current host's
native memory API.

From the repository root on Windows:

```cmd
Tools\Native\Test-Baseline-Jit-Publisher.cmd
```

From the repository root on Linux:

```sh
./Tools/Native/Test-Baseline-Jit-Publisher.sh
```

The launchers first build the Windvale producer bridge WVB through the native
source front door, compare it byte for byte with the retained WVB, and verify
the retained producer WVO. They then assemble and verify the shared plan
component and current-host adapter, link all three objects, check every pinned
intermediate identity and entry offset, and verify the committed application
digest. Linux additionally reconstructs the complete ELF byte for byte.
Windows executes the import-bound candidate PE; its import-directory
construction remains an explicit recovery operation.

A passing run prints one of:

```text
native baseline jit publisher status=Passed result=0 platform=windows-x64
native baseline jit publisher status=Passed result=0 platform=linux-x64
```

The applications themselves cover corrupted lifetime and patch plans, results
`42` and `-1`, a forced permission-transition failure, and release after every
allocation. They produce no output; the launcher requires result `0` and an
empty diagnostic stream.

To reconstruct both candidate artifacts from pinned native intermediates on a
Windows recovery host:

```powershell
pwsh -NoProfile -File Tools/Recovery/Rebuild-Baseline-Jit-Publisher.ps1 `
  -Destination Artifacts/Baseline-Jit-Publisher
```

That recovery script invokes the native source builder, assembler, WVO
verifier, linker, and console packager. It rebuilds and checks the bridge WVB,
but deliberately consumes the digest-bound retained bridge WVO. That WVO has
Stage 0 recovery provenance because the native lowerer does not yet admit a
descriptor-returning `Main() -> bytes`. PowerShell validates and writes the
two Windows PE data-directory entries needed to expose the four fixed imports;
the normal build, verification, and execution path does not compile or execute
C#.

Passing one launcher is current-host evidence only. Both launchers must pass
at one exact commit before cross-host qualification is claimed.
