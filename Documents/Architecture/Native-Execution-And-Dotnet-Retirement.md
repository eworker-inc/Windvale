# Native execution and .NET retirement

## Status

Accepted architectural direction under [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md). Decision 0058 qualifies bytecode compiler self-reproduction. Decisions 0059 through 0063 cross-host qualify the shared Stage 0 seam through constants, checked arithmetic/traps, typed control, dynamic instruction budgets, backward control, internal calls, bounded recursion, and immutable i32 data through WVO/AOT and Windows/Linux W^X paths. Decision 0064 qualifies the first downstream Windvale OS AOT consumer of that same ABI at exact candidate `708242e`. [Decision 0065](../Decisions/0065-Versioned-Native-Execution-Context-And-Console-Service.md) implements ABI 6's versioned execution context and first explicitly authorized static-text console service; regular Windows and pinned development-QEMU evidence passes while exact cross-host qualification is pending. This document defines the larger native destination and migration boundaries; it does not claim general capabilities, an in-guest WVB loader, a general native runtime, broad JIT or AOT compiler, PE host, ELF host, garbage collector, or native self-hosting chain.

## Destination

Windvale source has one semantic frontend and two durable publication levels:

```text
Windvale source
      |
      +--> typed Windvale IR --> canonical verified WVB
      |                                |
      |                                +--> verified interpreter
      |                                +--> baseline JIT
      |                                +--> optimizing JIT
      |                                `--> install-time or cached native compilation
      |
      `--> typed Windvale IR --> shared native backend --> WVO/AOT image
```

WVB is the portable distributable contract. WIR and the future native machine IR are compiler contracts. WVO, PE/COFF, ELF, in-memory linked code, and Windvale OS process images are target artifacts. None silently defines source semantics for another layer.

Windows and Linux remain permanent Windvale hosts after .NET retirement. Windvale OS adds another platform implementation; it does not absorb or replace the host tool and application ecosystem.

## Compilation is a continuum

JIT and AOT describe when native compilation occurs, not competing language definitions:

| Time | Form | Primary use |
| --- | --- | --- |
| Build time | Deterministic AOT | Kernel, drivers, core tools, release applications |
| Install time | Target-local AOT or cache population | Portable packages deployed to a known machine |
| Load time | Eager JIT | Small complete modules where predictable latency matters |
| First call | Lazy baseline JIT | Ordinary portable applications |
| Hot execution | Selective optimizing JIT | Measured long-running functions |
| Post-link/profile rebuild | Profile-guided AOT | Qualified release performance |

A target may support only a safe subset of these forms. Environments that prohibit dynamic executable memory remain valid AOT/interpreter hosts.

## Execution tiers

### Tier 0: verified interpreter

The interpreter is the simplest executable semantic oracle. Its Windvale-native successor should favor transparent behavior, bounded resource accounting, and useful diagnostics over peak speed. It remains valuable after JIT/AOT qualification for differential tests, debugging, hostile-input containment, and architectures without a native backend.

### Tier 1: baseline copy-and-patch JIT

The first proposed JIT maps verified WVB operations or typed micro-operations to prebuilt machine stencils. A stencil contains instruction bytes plus typed holes for constants, branches, runtime services, data, and calls. The JIT copies selected stencils, lays them out, applies checked patches, independently validates the result, and finalizes executable permissions.

Windvale already has useful ingredients:

- WVA owns explicit machine operations and encodings;
- WVO owns sections, symbols, and typed relocations;
- the linker owns checked layout and patch application; and
- WVB supplies verified types, branch targets, stack depths, and resource limits.

The experiment should reuse those contracts without serializing and rereading a complete WVO for every function when an in-memory link graph is sufficient. AOT and JIT sinks may share the same structured machine fragment and relocation model.

Initial stencils may use fixed register conventions and conservative runtime calls. Superinstructions or fused micro-operations may later reduce dispatch-shaped overhead without weakening verification.

### Tier 2: optimizing JIT

Only measured hot functions justify a slower optimizing tier. It may introduce SSA-like native IR, register allocation, inlining, constant propagation, branch layout, bounds-check elimination supported by proof, and target-specific scheduling. Windvale's static types should avoid much of the speculative type guarding and deoptimization required by dynamic languages.

Speculation, tracing, deoptimization, and on-stack replacement are not baseline requirements. Each adds live-frame maps, state reconstruction, invalidation, concurrency, and debugging obligations. They remain later decisions driven by evidence.

### AOT and profile-guided AOT

The shared backend writes WVO for deterministic linking into PE/COFF, ELF, UEFI, or Windvale-native containers. Kernel and driver code is always AOT. Core tools should prefer AOT once the native backend covers them because it minimizes startup, executable-memory policy, and code-cache state.

Profile-guided optimization is allowed only when the complete profile, target, tool versions, and options are explicit, reproducible inputs. Post-link layout may reorder functions and blocks or split cold code, but it must preserve verified semantics and publish independently checkable output.

## Shared native backend

The backend should have one semantic input and multiple publication sinks:

```text
verified WVB or typed WIR
          |
     native machine IR
          |
     instruction selection
          |
  structured code + data + patches
       /                       \
WVO serialization         in-memory link graph
       |                       |
PE/ELF/OS AOT image        finalized JIT pages
```

The WVB path serves portable deployed modules. The WIR path allows source AOT without round-tripping through a distributable stack format when richer compiler evidence is available. Both must implement the same defined operations and share differential programs.

Architecture-specific selection, register assignment, encoding, relocation, and ABI policy stay behind explicit contracts. The initial x86-64 backend must not prevent a later AArch64 backend.

### Implemented slices

`Compiler/Native` accepts only a `Verifiedˉmodule`. Decision 0059's qualified `x86-64-wvb-baseline-v1` slice lowers the first canonical portable WVB shape into explicit `Nativeˉi32ˉconstant` and `Nativeˉreturn` operations. The same independently verified fragment is serialized to WVO for the existing linker or handed to `Runtime/Windvale.Native` for checked in-memory linking and W^X publication.

The version-1 program is deliberately only one exported `Main() -> i32` returning a constant. Its exact `return 42` code is `B8 2A 00 00 00 C3`; interpreter, JIT-fragment, and WVO-linked-image execution agree on Windows and Debian x64 at exact commit `962bb85`. That commit proves the ownership and publication seam, not general WVB coverage; the version-2 arithmetic/trap extension is recorded separately below.

Decision 0060 advances the experimental current target to `x86-64-wvb-baseline-v2`. It lowers verified single-assignment straight-line `i32` add, subtract, multiply, and negate into a bounded one-page frame, branches on x86 overflow to one checked epilogue, and returns a packed value/status word without host signals. The runtime maps overflow status to `WVR3007`. The fragment verifier independently decodes every allowed instruction, slot, branch target, epilogue, and status before WVO or W^X publication. Windows and Debian interpreter/JIT/AOT evidence is cross-host qualified at `84dd908`. Comparisons, control flow, calls, data, capabilities, other traps, heap ownership, PE/ELF containers, and Windvale-written implementation remain later gates.

Decision 0061 advances the experimental current target to `x86-64-wvb-baseline-v3`. Machine IR functions now own typed locals, typed numbered values, canonical basic blocks, and explicit terminators. The one-page frame is completely zero-initialized, signed and bool comparisons produce normalized values, and forward branches target decoded semantic-group boundaries. The selector fully resolves internal relative displacements for both sinks; the independent verifier proves exact frame access, forward-only reachability, balanced exits, and complete byte consumption. Windows and Debian interpreter/JIT/AOT and hostile-fragment evidence are cross-host qualified at `f0a53a9`. Backward branches remain rejected until native instruction budgeting or safe points prevent an unbounded in-process loop.

Decision 0062 advances the experimental current target to `x86-64-wvb-baseline-v4`. Every lowered WVB instruction has an explicit charge; a positive per-run maximum arrives in `RDX` through a three-argument Windows/System V bridge and is held in reserved `R11`. Unsigned underflow returns packed status 2 as `WVR3011`. Backward targets are admitted only at decoded charge boundaries, and the verifier proves exact charge/semantic alternation plus complete cyclic reachability. Windows and Debian interpreter/JIT/AOT success and exhaustion boundaries are cross-host qualified at `2b67c8a`. Calls must later preserve one shared remaining budget explicitly.

Decision 0063 advances the experimental current target to `x86-64-wvb-baseline-v5`. A six-argument host bridge places instruction and depth maxima identically under both supported ABIs; `R11` carries one exact instruction counter and `R10` one depth counter through every internal call. Up to four i32/bool parameters use shared volatile registers, recursion is bounded as `WVR3004`, and callee traps propagate without executing later WVB instructions. Immutable i32 arrays lower through checked RIP-relative reads; bounds failures map to `WVR3005`; the flat JIT tail and WVO `.rodata` relocation resolve to identical bytes. The fragment verifier independently decodes every function, call edge, counter transition, data patch, trap path, and reachable byte. Windows and Debian interpreter/JIT/AOT, exact resource-boundary, and hostile-fragment evidence are cross-host qualified at `1af2eca`.

Decision 0064 uses ABI 5 without adding an OS-specific instruction selector. One ordinary portable source module becomes canonical verified WVB and a deterministic WVO with `.text` plus `.rodata`; an exact bridge supplies the already-defined instruction/depth budgets and accepts only packed result 29 before the existing system-profile kernel Main may run. The module executes on the Windvale OS kernel-owned stack under pinned QEMU. This proves downstream AOT reuse, not an OS runtime: C#/.NET still builds the image on the host, and the guest neither retains nor decodes nor verifies WVB.

Decision 0065 advances the current implementation to `x86-64-wvb-baseline-v6`. `Main` receives one pointer in `RDX` to a 32-byte versioned execution context containing the instruction budget, depth budget, and optional 16-byte versioned service table. The first closed service entry is `console.write_line`: generated code passes one verified static UTF-8 range through an identical `R8`/`R9D` convention, while exact runtime-owned thunks adapt only that call to Windows x64 or System V. Authorization and implementation preflight precede executable allocation; callback failures return packed status 5; and the independent decoder validates the prologue, service call, relocation, UTF-8 target, failure path, and context-register restoration. The current OS bridge constructs the same context with no services for its portable module. The pinned development-QEMU boot passes; exact cross-host qualification remains pending.

## Native runtime ABI

Generated code targets a Windvale-owned internal ABI rather than emitting host calls throughout ordinary functions. The version-6 entry receives a pointer in `RDX` to the exact [native execution context](../../Specifications/Windvale-Native-Execution-Context.md). One identical `Main` preserves that pointer in `R15` and loads the versioned instruction/depth budgets into reserved `R11` and `R10`. Internal functions accept as many as four i32/bool parameters in `R8D`, `R9D`, `ECX`, and `EDX` and return one packed value/status in `RAX`.

The context's optional service-table pointer is the only generated-code route to a host service. ABI 6 defines one closed `console.write_line` entry for verified immutable UTF-8 text, explicit authorization, and runtime-owned platform thunks. WVO 1.0 does not serialize fragment service requirements, so a linked hosted image is not independently loadable without its verified fragment metadata. This remains a bounded experimental convention, not the final aggregate, stack-argument, allocation, file/process-service, or safe-point ABI.

The ABI must eventually define:

- value tags and payload layouts;
- parameter, return, stack, and register rules;
- text, bytes, record, enum, and future collection layouts;
- allocation, roots, safe points, reclamation, and out-of-memory behavior;
- checked arithmetic, bounds, invalid UTF-8, traps, and diagnostics;
- capability request and return conventions;
- module, function, type, and data identity;
- thread, synchronization, unwind, and debugging boundaries when introduced; and
- runtime-service-table extension and independently loadable service metadata.

Small WVA thunks translate this internal ABI to Windows x64, System V x86-64, UEFI, or Windvale OS boundaries. PE/COFF and ELF container differences do not belong in portable code generation.

## Memory and size direction

The C# reference runtime is a semantic oracle, not a memory oracle. It currently benefits from a mature CLR but retains managed object headers, a broad value structure, decoded instruction objects, UTF-16 host strings, original code plus decoded forms, garbage-collector metadata, and the CLR/JIT process footprint.

The native design should measure and minimize:

- packed tagged values rather than one field for every possible value kind;
- packed decoded operations or direct verified byte views;
- immutable UTF-8 text where compatible with source semantics;
- shared module pages and zero-copy slices;
- phase or request arenas for compiler and linker temporary state;
- bounded per-process heaps and native-code caches;
- explicit roots and reclamation; and
- duplicate static runtime code across applications.

Small host applications may use a shared Windvale runtime to minimize individual image size or static linking to eliminate installed dependencies. Windvale OS may share runtime services while keeping application heaps and authorization isolated.

No fixed size or speed ratio is an architectural promise. Qualification reports should distinguish file size, installed shared-runtime size, cold-start committed memory, peak working set, live Windvale heap, code-cache bytes, allocations, reclamation pauses, compilation latency, and execution throughput.

## Platform boundary

| Concern | Windows | Linux | Windvale OS |
| --- | --- | --- | --- |
| Native container | PE/COFF | ELF | Windvale-defined process image or WVO-derived container |
| External calling convention | Windows x64 | System V x86-64 | Windvale kernel/user ABI |
| Executable memory | Narrow virtual-memory adapter | Narrow virtual-memory adapter | Kernel virtual-memory service |
| Capabilities | Windows adapters | Linux adapters | Native Windvale services |
| JIT placement | Process or isolated compiler service | Process or isolated compiler service | User process or isolated system service |
| Kernel/driver code | Not applicable | Not applicable | AOT only |

Portable and hosted WVB must not observe which native container, page API, or external calling convention supplied execution.

## JIT safety and publication

Every JIT path follows a fail-closed publication sequence:

1. Decode the complete bounded WVB module.
2. Perform mandatory semantic verification.
3. Lower only verified functions through a versioned backend.
4. Measure code, data, patches, metadata, and cache impact before allocation.
5. Allocate writable, non-executable working pages.
6. Emit bytes and apply checked typed patches.
7. Independently reconstruct or validate code ranges, targets, runtime entries, and metadata.
8. Publish instruction-cache changes required by the architecture.
9. Transition final code to read/execute and remove writable access.
10. Register the function atomically only after all prior steps succeed.

Compilation may occur in a separate process with a narrower privilege set. The executor must distrust returned code metadata and repeat every boundary check it owns. Permanently writable/executable pages are outside the accepted design.

## Native-code cache

Cached code is a derived optimization rather than the portable program identity. A cache entry includes or is keyed by:

- complete canonical WVB SHA-256;
- module profile and authorized capability shape where material;
- JIT/compiler and native-IR versions;
- native ABI and runtime-service-table versions;
- architecture, operating environment, and CPU feature baseline;
- optimization tier and explicit profile identity; and
- code, relocation, metadata, and final-image digests.

Unknown, partial, mismatched, truncated, or unauthorized entries are rejected and regenerated. Reproducible baseline code should remain available even when profile-guided cached code is machine-specific.

## Bootstrap and retirement sequence

```text
C# Stage 0 compiler/runtime
        |
        +--> Windvale compiler WVB
        |          |
        |          `--> Stage 1 and Stage 2 convergence
        |
        `--> Windvale native verifier/backend/JIT WVB
                         |
                         `--> native Windvale tools on Windows/Linux
                                      |
                                      `--> normal workflow without .NET
```

The normal post-retirement bootstrap uses a documented previous native Windvale release to rebuild the next. Exact source, seed binaries, manifests, signatures or digests, target identities, and Stage comparisons form the trust record. The final .NET Stage 0 release remains recoverable historical evidence unless a later decision selects and qualifies another minimal from-zero path.

Removing .NET from automation before the [Decision 0057 retirement gate](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md#native-retirement-gate) would trade an explicit bootstrap dependency for an undocumented binary trust dependency and is not accepted.

## Qualification matrix

Representative programs must compare all supported execution forms:

| Evidence | Interpreter | Baseline JIT | Optimizing JIT | AOT |
| --- | :---: | :---: | :---: | :---: |
| Module accepted or rejected identically | required | required | required | required |
| Return value and output bytes | required | required | required | required |
| Traps and diagnostics | required | required | required | required |
| Capability authorization | required | required | required | required |
| Defined resource counters | required | required | required | required |
| Deterministic artifact bytes | runtime report | baseline target | only with fixed inputs | required |
| W^X and cache rejection | not applicable | required | required | container policy |

Windows and Linux require matching semantic reports. Windvale OS joins the same matrix when its process/runtime boundary exists.

## Technique adoption order

Adopt early when the prerequisite exists:

1. compact typed micro-operations;
2. WVA-generated copy-and-patch baseline stencils;
3. lazy per-function compilation;
4. persistent content-addressed native caches;
5. a shared AOT/JIT backend and in-memory linker;
6. isolated compilation and strict W^X publication; and
7. explicit profile-guided AOT.

Defer until measurements justify their complexity:

- speculative optimization and deoptimization;
- tracing JIT compilation and on-stack replacement;
- a large general-purpose external compiler framework in the runtime;
- machine-learned inlining or register-allocation heuristics; and
- hardware-specific code that lacks a portable baseline fallback.

These external systems provide useful design evidence without becoming Windvale dependencies: CPython's [copy-and-patch JIT direction](https://peps.python.org/pep-0744/), Wasmtime's [baseline Winch and optimizing Cranelift split](https://bytecodealliance.org/articles/winch-aarch64-support), LLVM's [ORC](https://llvm.org/docs/ORCv2.html) and [JITLink](https://llvm.org/docs/JITLink.html) separation, [BOLT post-link optimization](https://llvm.org/docs/AdvancedBuilds.html#bolt), and [ML-guided optimization facilities](https://llvm.org/docs/MLGO.html).
