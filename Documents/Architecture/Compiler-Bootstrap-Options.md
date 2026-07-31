# Compiler bootstrap options

## Status

C# Stage 0, typed WIR, and Windvale bytecode are accepted and implemented by Decision 0002. Decision 0049 implements the first bounded direct x86-64 target for one kernel-entry source shape. [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) accepts the general destination: canonical WVB, a shared Windvale-native JIT/AOT backend and runtime on Windows, Linux, and Windvale OS, and retirement of .NET from the normal workflow after an explicit qualification gate. The bounded target does not settle the future ABI or backend design.

“Bootstrap” names the staged process that starts from an existing host toolchain and reaches a reproducible Windvale-built stack. It is not the durable product name of either compiler implementation. The Windvale-written implementation is the **Windvale compiler** even before it passes self-hosting qualification; it lives in `Compiler/Windvale`. The C# implementation is the independent **reference/recovery compiler** and lives in `Compiler/Reference`. Bootstrap provenance and recovery instructions remain explicitly documented. This role layout is cross-host qualified at `4fdc6bf` under Decision 0043.

## Two different choices

“Intermediate language” can refer to two separate decisions:

1. The existing language used to write the first compiler and tools.
2. The representation produced between Windvale source and its final execution form.

Keeping these decisions separate prevents the bootstrap implementation from becoming the permanent Windvale architecture accidentally.

## Recommended short path

The accepted staged path is:

```text
Stage 0 tools: C#
        |
Windvale source --> AST --> typed Windvale IR (WIR)
                              |-- canonical Windvale bytecode (WVB)
                              |             |-- verified interpreter
                              |             |-- baseline/optimizing JIT
                              |             `-- cached or install-time native code
                              `-- shared native backend --> WVO/AOT

Assembly source --> instruction model --> shared native object model
Native backend --------------------------^             |
                                                       `--> Windvale linker
```

This path uses the current Stage 0 to reach useful milestones quickly while preserving a small, owned Windvale stack as the destination. JIT and AOT share verified semantics, a native ABI, machine lowering, structured patches, and platform adapters rather than becoming parallel compilers. The complete execution architecture is in [Native-Execution-And-Dotnet-Retirement.md](Native-Execution-And-Dotnet-Retirement.md).

## Bootstrap implementation languages

### C# — selected for Stage 0

Advantages:

- Runs well on Windows and Linux.
- Strong exact-width integer, binary I/O, Unicode, diagnostics, testing, and immutable-model support.
- Suitable for a compiler, assembler, linker, disassembler, object inspector, and reference VM in one solution.
- Already familiar in the surrounding [E-Worker](https://eworker.ca) development environment.
- Faster and safer to iterate than low-level languages for binary parsers and semantic models.

Costs:

- Requires the .NET SDK for the bootstrap toolchain.
- Cannot serve directly as the Windvale kernel implementation.
- Must eventually become a reference/recovery implementation or be replaced by a self-hosted Windvale compiler.

### TypeScript — strongest rapid-prototype alternative

Advantages:

- Very fast grammar, parser, diagnostics, and tooling iteration.
- Runs on Windows and Linux through Node.js.
- Good fit for visualization and interactive inspection tools.

Costs:

- Exact 64-bit arithmetic and binary-layout work require additional care.
- A Node dependency is less natural for low-level runtime and object tooling.
- It may encourage dynamic shapes where explicit compiler contracts are preferable.

TypeScript remains attractive for format explorers and developer interfaces even if C# owns the first compiler.

### C

Advantages:

- Widely available and close to native ABIs.
- Can support freestanding runtime, firmware, and early kernel experiments.
- Makes bootstrap requirements understandable.

Costs:

- Slower and riskier for a rapidly evolving compiler frontend and untrusted binary parsers.
- Manual memory ownership can obscure language-design work.
- Undefined and implementation-defined behavior require strict discipline.

C is a better bridge target and small runtime substrate than the first high-level compiler implementation.

### Rust

Advantages:

- Strong memory safety, enums, pattern matching, binary parsing, and native performance.
- Suitable for production-quality runtime and low-level tools.

Costs:

- Ownership and lifetime work can slow early semantic experimentation.
- Adds a substantial existing compiler/toolchain dependency during bootstrap.
- A Rust-first implementation does not directly simplify self-hosting Windvale.

Rust is a credible later implementation choice, but it does not currently appear to minimize the first loop.

### Python

Python can produce a parser or semantics experiment very quickly, but packaging, static guarantees, binary manipulation, and long-term verifier performance make it a weaker repository foundation. It remains useful for isolated generators and test analysis.

## Candidate intermediate representations

### Typed Windvale IR — permanent internal representation

Windvale needs an owned semantic IR regardless of the first external backend. It should represent typed operations, explicit control flow, calls, data layout requests, capabilities, and source mappings without inheriting one host ABI.

The first WIR can be deliberately simple. It does not need an advanced optimizer before it can support bytecode or C generation.

### Windvale bytecode — permanent distributable representation

Bytecode is the portable application and tool format. It should be versioned, typed or verifiably type-safe, deterministic, resource-bounded, and designed for validation before execution.

Bytecode is not merely a serialized compiler IR. Compiler IR changes with optimization needs; distributable bytecode needs a durable compatibility and security contract.

### Restricted C — optional bootstrap or recovery bridge

Generating simple C can provide a contingency path from a young Windvale frontend to native hosts through existing C compilers. Decision 0057 no longer makes it a required step: the shared owned WVO/JIT/AOT backend is the accepted destination, and the existing bounded x86-64 target has already proved direct native publication.

The bridge must use a controlled subset:

- Fixed-width types
- Runtime helpers for operations whose C behavior is undefined or host-dependent
- Explicit layout and calling-boundary rules
- No reliance on signed overflow, evaluation order, native `long` width, or host text behavior
- Differential tests against the reference VM

C must remain an optional backend, not the definition of Windvale semantics or a permanent retirement dependency. Adding it requires a concrete recovery or differential need and must not delay the owned backend.

### WebAssembly — optional experimental backend

WebAssembly offers a standardized portable execution environment and can provide useful ecosystem reach. Making it the primary path would, however, duplicate Windvale bytecode’s role and would not remove the need for a native kernel backend. Treat it as a later interoperability experiment rather than a bootstrap requirement.

### LLVM IR — optional accelerator, not the core contract

LLVM can provide optimization, architecture coverage, debug information, and mature machine-code generation. It is also a large dependency and would bypass much of the small assembler/linker demonstration. Its IR should not become Windvale’s stable module format. An optional LLVM backend may be valuable after the owned semantics and native path are established.

### .NET IL — unsuitable as the central Windvale representation

.NET IL would make early hosted execution convenient but would couple language semantics to the CLR type, metadata, runtime, and object models. It does not provide the intended route to a small freestanding OS. It may be an interoperability target later.

### Direct machine code — first bounded target implemented, shared backend accepted

A direct native backend is necessary for a self-owned kernel and host toolchain. Decision 0049 adds the first intentionally narrow implementation only after typed WIR, verified WVO, linking, and the kernel handoff exist: one linear system-profile entry, constant ASCII line output through an imported adapter, and a constant return. This proves that WIR can reach booted native code through the shared object model. Decision 0057 requires the general backend to serve both deterministic WVO/AOT and in-memory JIT publication. Register allocation, general control flow, data addressing, a stable native ABI, host executables, native WVB lowering, and differential semantic coverage remain Phase 9 work.

## Proposed bootstrap stages

1. Specify a minimal source subset, WIR, bytecode, and observable semantics.
2. Implement the Stage 0 compiler and a simple reference VM in C#.
3. Run the same bytecode modules on Windows and Linux.
4. Implement the assembler, object model, object inspector, and linker as independently testable tools.
5. Add a direct x86-64 native backend that writes through the shared object model.
6. Implement increasing portions of the compiler and tools in Windvale itself.
7. Prove Stage 0, Stage 1, and Stage 2 compiler convergence and archive the recovery inputs.
8. Define the native ABI, compact values, runtime services, memory ownership, and host thunks.
9. Lower verified WVB and typed WIR through one structured native backend.
10. Qualify deterministic WVO/AOT and a low-latency baseline JIT on Windows and Linux, with interpreter/JIT/AOT differential evidence.
11. Rebuild and run the compiler, verifier, assembler, linker, runtime, tests, and packaging through Windvale-native tools, then retire .NET from the normal workflow under Decision 0057's gate.
12. Run the same verified WVB modules through equivalent Windvale-native execution paths on Windows, Linux, and Windvale OS.

## Why this minimizes loops

- One frontend feeds all execution forms.
- One WIR preserves semantics across backends.
- One object model serves both assembler and compiler output.
- The reference VM supplies executable semantics before native code generation is complete.
- WVA, WVO, and the linker provide owned native reach without requiring C, LLVM, or CLR formats as semantic contracts.
- Windows and Linux ports remain useful when the OS arrives.
- Self-hosted tools cross into Windvale OS as existing bytecode modules or shared-backend AOT artifacts instead of being rewritten for it.
- One native backend supports baseline JIT, cached/install-time code, AOT host tools, and AOT system components.
- The retirement gate replaces an implicit indefinite .NET dependency with an explicit reproducible native bootstrap.

## Decisions still needed

- Whether a future bytecode version should remain stack-based after Seed experience
- Memory management and object representation
- Error and exception semantics
- Integer overflow and floating-point reproducibility rules
- General native value representation, ABI, and object-layout expansion beyond the accepted WVO kernel subset
- The first baseline-JIT stencil and typed patch contract
- Tier thresholds, native-cache policy, and which resource counters are execution-mode-independent
- The minimum native allocator/reclamation strategy needed before .NET retirement
- Cross-target policy after the accepted x86-64/UEFI first boundary
- Criteria for calling the compiler self-hosting
