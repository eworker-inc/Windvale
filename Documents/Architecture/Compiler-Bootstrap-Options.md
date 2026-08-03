# Compiler bootstrap options

## Status

C# Stage 0, typed WIR, and Windvale bytecode are accepted and implemented by Decision 0002. Decision 0049 implements the first bounded direct x86-64 target for one kernel-entry source shape. [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) accepts the general destination: canonical WVB, a shared Windvale-native JIT/AOT backend and runtime on Windows, Linux, and Windvale OS, and retirement of .NET from the normal workflow after an explicit qualification gate. Decision 0058 qualifies exact Stage 1 to Stage 2 reproduction by the Windvale bytecode compiler on Windows and Debian. Neither this bytecode proof nor the bounded native target settles the future ABI or backend design.

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
- Remains the active recovery and comparison oracle until Decision 0057's native-retirement gate; its final qualified release is then archived as recovery/provenance evidence rather than used by normal automation.

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

WebAssembly offers a standardized portable execution environment and can provide useful ecosystem reach. Making it the primary path would, however, duplicate Windvale bytecode’s role and would not remove the need for a native kernel backend. [Decision 0102](../Decisions/0102-First-Windvale-WebAssembly-Backend-Slice.md) implements the first deliberately bounded interoperability experiment: portable `.wv` lowers one exact canonical WVB constant profile to deterministic Wasm. [Decision 0104](../Decisions/0104-WebAssembly-Checked-Addition-And-Execution-Contract.md) adds checked `i32.add` and a versioned status/result/instruction boundary without an engine trap. [Decision 0106](../Decisions/0106-Bounded-Straight-I32-WebAssembly-Lowering.md) generalizes that seam to a validated straight-line `i32` stream with locals and all four checked arithmetic operations. WebAssembly remains an optional target rather than a bootstrap requirement, and structured control flow, browser integration, and cross-host qualification are still open.

### LLVM IR — optional accelerator, not the core contract

LLVM can provide optimization, architecture coverage, debug information, and mature machine-code generation. It is also a large dependency and would bypass much of the small assembler/linker demonstration. Its IR should not become Windvale’s stable module format. An optional LLVM backend may be valuable after the owned semantics and native path are established.

### .NET IL — unsuitable as the central Windvale representation

.NET IL would make early hosted execution convenient but would couple language semantics to the CLR type, metadata, runtime, and object models. It does not provide the intended route to a small freestanding OS. It may be an interoperability target later.

### Direct machine code — first bounded target implemented, shared backend accepted

A direct native backend is necessary for a self-owned kernel and host toolchain. Decision 0049 adds the first intentionally narrow implementation only after typed WIR, verified WVO, linking, and the kernel handoff exist. Decisions 0059 through 0083 qualify a broader but still bounded ABI-14 WVB subset through both deterministic WVO/AOT and in-memory W^X publication, including control flow, calls, static data, borrowed and arena-backed values, all eleven then-current native service leaves, live Windvale-produced process-input leaves, and Windvale-owned executable-image layout and lifetime. Decision 0087 cross-host qualifies ABI 15's twelfth native leaf and advances exact compiler preflight past file output at `12e9e2e`. Decision 0089 cross-host qualifies the language-bounded 64-parameter internal convention under ABI 16 at `860c69c`. Qualified Decision 0099 advances the backend to ABI 17's bounded 2,048-cell frame at `4a077ab`, clears the former 1,049-local preflight failure, and measures the next lowered-value pressure at slot 2,049 in `Compilerˉbodyˉparseˉprimary`. Qualified Decision 0105 advances the backend to ABI 18 at `484c228`: globally canonical semantic IDs map to exact-type physical cells reused only across verified empty-stack blocks, clearing slot 2,049 without increasing the physical bound. Qualified Decision 0108 advances the backend to ABI 19 at `a35c348`: exact one-byte construction preserves that map and advances compiler preflight from `Bytesˉfromˉu8` to `Bytesˉfromˉu16ˉlittle`. Qualified Decision 0109 advances the backend to ABI 20 at `a63ca0f`: checked two-byte little-endian construction clears the remaining observed operation blocker, and exact preflight now selects a deterministic 4,556,121-byte fragment against the current 1,048,576-byte admission limit. Code compaction or bounded function-granular publication, register allocation, standalone host containers, and native self-hosting remain Phase 9 and Phase 10 work.

Decisions 0115 through 0118 measure the failed monotonic-record lifetime, retain nominal identity, prove bounded record liveness, and publish deterministic frame offsets. Implemented Decision 0133 consumes those offsets in ABI 21: direct records live in verified frames, internal calls pass backing pointers, and returns copy into caller-owned destinations. The exact compiler now executes its single-source fixture with zero record-arena use, while the full Stage 1 inventory advances from `WVR3017` to the retained 16 MiB dynamic text/byte boundary `WVR3018`. Explicit baseline copies produce a deterministic 16,905,513-byte fragment under a synchronized 32 MiB fragment limit and the unchanged 34 MiB publication-image limit. The shared OS consumer is rebuilt through the same selector; its projected-frame stack proof remains inside six pages and Probe 32 uses zero record-arena bytes under `WVKMEM11`. Cross-host qualification, the dynamic-value lifetime decision, standalone host containers, and native self-hosting remain later work.

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

Stages 1 through 4 and stage 7's bytecode compiler convergence are qualified. The Windvale-written compiler and major binary tools satisfy stage 6 in their current portable scopes. Decisions 0059 through 0087 supply bounded qualified evidence for stages 5, 8, 9, and 10: ABI 15, interpreter/JIT/WVO-AOT agreement, all twelve current native service leaves, live Windvale-produced leaf bytes, and Windvale-owned publication layout and lifetime. Qualified Decision 0093 adds one fixed in-guest interpreter proof; qualified Decision 0094 derives the input's section payloads without yet accepting runtime-supplied modules. These slices do not qualify general backend coverage, native compiler execution, standalone native host tools, .NET retirement, or general in-guest WVB execution. The optional restricted C experiment is not a prerequisite for the accepted bytecode bootstrap proof or Decision 0057's owned native destination.

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
- The first measured stencil/container extension beyond the two exact qualified service leaves and the Windvale-owned publication-layout contract
- Tier thresholds, native-cache policy, and which resource counters are execution-mode-independent
- The minimum native allocator/reclamation strategy needed before .NET retirement
- Cross-target policy after the accepted x86-64/UEFI first boundary
- The boundary between bytecode self-hosting, native self-hosting, and removal of any recovery dependency
