# Decision 0186: First Windvale-native compiler build driver

- Date: 2026-08-03
- Status: Cross-host qualified at exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b`
- Advances: Phase 10 native host tools and the [Decision 0057 native-retirement gate](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Extends: [Decision 0185](0185-Standalone-Compiler-Wvb-Verifier-Applications.md) and [Decision 0169](0169-Public-Format3-Compiler-Targets.md)
- Contract: [Windvale compiler build-driver application](../../Specifications/Windvale-Compiler-Build-Driver.md)

## Context

The canonical compiler and compiler-aligned verifier already run as standalone Windows and Linux applications without loading .NET. Their hosted adapters remained separate, however: the compiler wrote a candidate after compilation, while the verifier admitted an existing file in a second process. The normal source-to-verified-WVB workflow still entered the C# CLI.

Adding process launch now would require a new capability, child-authority transfer, bounded output capture, cancellation, and lifecycle contract. It would also introduce a race between verifier admission and later publication. The compiler and verifier already expose portable Windvale cores that can be composed without either expansion.

The existing source-visible `file.write_bytes` service is durable but non-atomic. Silently making its build-driver binding atomic would violate the filesystem contract by giving one operation name different semantics.

## Decision

Move the compiler-aligned verifier algorithm behind one portable `Compilerˉwvbˉverify(bytes) -> u32` entry and retain the standalone verifier as a thin hosted adapter.

Implement `Windvale-Compiler-Build-Driver.wvproj`. Its hosted root constructs the bounded source set from explicit arguments, rejects exact input/output resource-name equality, invokes the Windvale compiler in memory, invokes the shared verifier over the resulting bytes, and calls `file.write_bytes` exactly once only after acceptance.

Package the driver through public `windows-x64-build-driver-v1` and `linux-x64-build-driver-v1` targets. Give the profile distinct `WVHB 1` magic, outer container format 5, and profile flags 3. Reuse the existing compiler-authority runtime, exact ten-service bundle, and canonical WVA startups because the authority and platform calls are identical. Require the canonical driver WVB module name before format-5 packaging and make the independent parsers take the expected profile explicitly.

Integrate the proof into the existing exact-compiler AOT case. Reuse its compiler evidence, then compile the driver once, verify paired deterministic packages, reject compiler/driver format confusion and corrupted outer inputs, exercise current-host CLI packaging, run the raw driver over a real source, compare exact WVB bytes, and prove source rejection preserves an existing output while no .NET module or mapping enters the child.

## Consequences

- One standalone Windvale process now owns source composition, compilation, compiler-aligned verification, and accepted WVB publication.
- The verifier implementation is shared rather than copied between standalone verification and the driver.
- Deterministic failures occur before the sole output call and preserve an existing output.
- A host write failure remains governed by the non-atomic `file.write_bytes` contract. This milestone does not claim atomic source-visible publication.
- Exact resource-name equality is rejected, but distinct path aliases cannot be proven distinct without a future canonical resource-identity contract.
- The original driver consumes explicit source paths. Decision 0187 extends the same application with bounded Project 1 input while dependency discovery, native packaging, tests, assembler/linker/inspector orchestration, and normal repository automation remain Stage 0 work.
- The format-3 compiler artifacts and their qualified identities remain unchanged.
- The exact driver profile counts as qualified dual-host retirement evidence; atomic source-visible publication and normal-path cutover remain separate gates.

## Qualification

Exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b` passes GitHub
[Verify run 30964566192](https://github.com/eworker-inc/Windvale/actions/runs/30964566192).
Windows and Debian each execute the raw compiler, verifier, and build-driver
applications, reproduce the canonical compiler WVB, preserve outputs on deterministic
rejection, observe no .NET mapping in the child processes, and pass the complete
97-Seed/39-OS/native-CLI gate.

## Reconsideration triggers

Reconsider this fixed profile when one of these becomes true:

- the project adapter requires a canonical resource-identity provider or a larger retained-snapshot profile;
- a named atomic-replacement capability has exact Windows/Linux progress, durability, and indeterminate-failure semantics;
- compiler/verifier isolation justifies a versioned child-process capability and rights-limited launch plan;
- the compiler-aligned verifier no longer admits the compiler's normal WVB output;
- cross-host qualification changes the canonical identities, behavior, or authority inventory.
