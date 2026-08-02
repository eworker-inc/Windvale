# Native execution and .NET retirement

## Status

Accepted architectural direction under [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md). Decision 0058 qualifies bytecode compiler self-reproduction. Decisions 0059 through 0083 cross-host qualify the shared Stage 0 and first OS consumer seams through ABI 14, native leaves for all eleven then-current service slots, two Windvale-owned stencil consumers, a bounded byte-result entry, Windvale-owned executable-image layout and lifetime policy, and firmware probe 17's terminal invalid-opcode boundary. Decisions 0085 through 0087 cross-host qualify ABI 15/context 7, the twelfth exact native file-output leaf, WVA-owned Q35 shutdown and normalized trap entries, and composed firmware probe 20 at exact commit `12e9e2e`. Exact commit `860c69c` qualifies Decisions 0088 through 0090: ABI 16's bounded 64-parameter convention, a kernel-owned W^X root, and one fixed Windvale-owned in-guest WVB admission profile. Exact implementation commit `4a077ab` qualifies [Decision 0099](../Decisions/0099-Bounded-Native-Frame-Admission.md) and advances the backend to ABI 17's 2,048-cell envelope. Exact implementation commit `484c228` qualifies [Decision 0105](../Decisions/0105-Typed-Block-Scoped-Native-Value-Slots.md) and ABI 18: it retains that physical ceiling while separating canonical semantic value IDs from typed physical cells reused across verified empty-stack blocks. This document defines the larger native destination and migration boundaries; it does not claim a general in-guest WVB loader/verifier, a general Windvale-owned native runtime, broad JIT or AOT compiler, PE host, ELF host, garbage collector, or native self-hosting chain.

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

Decision 0077's first qualified slice intentionally stops below a general JIT: one canonical WVO contains a five-byte `process.argument_count` template and one typed one-byte execution-context-offset patch. The Windvale-written assembler produces the object, while the C# native owner validates its complete fixed shape, instantiates the patch, verifies the unchanged qualified leaf identity, and supplies it to the live executor. This proves source ownership, deterministic construction, typed patching, and hostile-record rejection before the contract admits branches, calls, data references, or multiple templates.

Decision 0078 cross-host qualifies the measured extension at exact commit `50294d9`: the exact 70-byte `process.argument` template has eight strictly ordered one-byte locations and six named ABI meanings. Repeated meanings share one checked value, while the WVO header fixes patch count and template size. The live runtime consumes the Windvale-assembled object without changing the leaf digest. This establishes the minimum multi-patch model but still deliberately excludes branch-target patching, calls, relocations, data references, template selection, native Windvale validation, and executable-memory ownership.

Decision 0079 cross-host qualifies the first portable Windvale consumer for both retained WVO/WVSP shapes at exact commit `f3a4ba4`. Its capability-free core validates every byte, maps closed semantic patch kinds to the current ABI values, and constructs immutable results; one production-tied demo agrees through the reference interpreter, native JIT, and linked WVO/AOT and rejects a mutation at every input position. At that boundary, this removed C# from stencil acceptance and patch semantics but not from the live data path: native invocation returned only `i32`, so C# still supplied and checked the runtime bytes. Decision 0080 closes that specific result seam.

Decision 0080 cross-host qualifies that non-cyclic seam at exact commit `f547af8` without adding a service or changing existing scalar entries. A descriptor-returning `Main` receives its result cell in physical `RCX` on both host ABIs, moves it into the existing `RAX` hidden-result convention, and is independently classified before execution. The host accepts returned bytes only from exact static-data symbols or the committed execution-arena prefix and copies them before teardown. One retained, source-reproducible WVB validates both exact stencil objects and returns the two leaves as a 75-byte bundle; the live process-input service path now consumes those Windvale-produced bytes. Windows and Debian agree through the reference interpreter, W^X JIT, and linked WVO/AOT; all 65 portable artifacts match. C# remains the WVB loader, native compiler/verifier, W^X and arena owner, invocation adapter, and independent stencil oracle.

Decision 0082 cross-host qualifies the next bounded transfer at exact commit `ba2cf69` without creating a circular linker or a premature general FFI. Portable Windvale code accepts one strict [`WVPQ 1`](../../Specifications/Windvale-Native-Publication-Plan.md) request and produces the canonical `WVPL 1` image extent and 16-byte-aligned service placements under a 34 MiB ceiling. Its retained hosted wrapper runs through the reference interpreter with only an in-memory `file.read_bytes` capability before allocation; C# independently reconstructs every accepted placement. Because fragment verification already proves exact base-independent displacement fields, publication now copies fragment code unchanged and removes the redundant C# patch rewrite. Windows and Debian reports plus all 67 portable artifacts agree; both hosts pass all 63 Seed tests and all 17 OS tests, and both pinned-QEMU probe-17 scenarios pass. Windows/Linux allocation, W^X transition, instruction-cache flush, invocation, arenas, and teardown remain the narrow Stage 0 adapter.

Decision 0083 cross-host qualifies the next ownership transfer at exact commit `a898fe8` through the separate [`WVLQ 1`/`WVLT 1`](../../Specifications/Windvale-Native-Publication-Lifetime.md) contract. Windvale emits the complete allowed state/action graph once per accepted image extent. C# independently reconstructs every transition, then one internal executable-image owner holds the raw address and actual state, gates allocate/copy/seal/invoke/release operations, and admits release from every post-allocation partial state. The larger executor no longer imports executable-memory platform calls. Windows/Linux P/Invoke authority, context/service/arena/result-cell lifetime, and native compilation/verification remain Stage 0 responsibilities for later measured transfers.

Decision 0087 qualifies the first capability slice selected by preflighting the exact qualified compiler WVB at exact commit `12e9e2e`. ABI 15/context 7 appends a file-output-table pointer and service 12. Exact Windows and Linux leaves create or replace one bounded file, complete partial writes, flush durably, and return stable contained failures without a managed callback or general FFI. Real-host JIT and linked WVO/AOT tests pass through 4 MiB. Repeating exact compiler preflight moves the blocker from unsupported `file.write_bytes` to `WVN2002` in `Compilerˉbodyˉblockˉstepˉvalid`. Later exact signature inventory proves its record return was already admitted and the rejection came from eight parameters against the four-register ceiling. Windows/Debian qualification, portable-artifact equality, all 18 OS tests, independent GitHub verification, and all three pinned-QEMU probe-20 scenarios pass.

Decision 0089 qualifies ABI 16 without changing context 7 or the service table. The first four parameters retain the shared volatile registers; up to 60 later positions use exact 16-byte outgoing cells, matching the language's bounded 64-parameter limit. The strict fragment decoder reconstructs stack reservation, scalar/descriptor cells, adjusted hidden result, direct call, release, and caller/callee agreement. Maximum-width scalar, descriptor-returning, and void calls agree across interpreter, W^X JIT, and linked WVO/AOT on Windows and Debian, while targeted corruption is rejected. Exact compiler preflight passes all functions with five through 23 parameters and advances to its sole 1,049-local function against the current 1,024-slot native frame bound.

Decision 0099 advances the implementation to ABI 17 without changing context 7, service table 5, WVB, WVO, or the call convention. The hard combined local/value envelope doubles to 2,048 16-byte cells (32 KiB), and independent reconstruction retains the same bound. Exact compiler preflight passes the former 1,049-local function and reaches a later lowered-value pressure point: `Compilerˉbodyˉparseˉprimary` requests slot 2,049. Frame optimization remains preferable to another unmeasured increase; the exact compiler does not yet execute natively.

Decision 0105 qualifies that measured optimization as ABI 18 without raising the physical bound or changing source/WVB semantics. Machine IR retains globally canonical semantic value IDs and types, then records a separate canonical physical map whose exact-type ranges are reused only across basic blocks. The empty-stack edge rule proves that no value can cross the reuse boundary; independent verification reconstructs the map and rejects cross-block operands before selection. The exact compiler preflight clears slot 2,049 and reaches unsupported `Bytesˉfromˉu8` in `Compilerˉcompileˉsourceˉwvb`. This is deterministic stack-slot allocation, not register allocation or native compiler execution.

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

Decision 0065 advances the qualified current target to `x86-64-wvb-baseline-v6`. `Main` receives one pointer in `RDX` to a 32-byte versioned execution context containing the instruction budget, depth budget, and optional 16-byte versioned service table. The first closed service entry is `console.write_line`: generated code passes one verified static UTF-8 range through an identical `R8`/`R9D` convention, while exact runtime-owned thunks adapt only that call to Windows x64 or System V. Authorization and implementation preflight precede executable allocation; callback failures return packed status 5; and the independent decoder validates the prologue, service call, relocation, UTF-8 target, failure path, and context-register restoration. The OS bridge constructs the same context with no services for its portable module. Windows, Debian, and pinned-QEMU evidence is qualified at exact candidate `2fcf531`.

Decision 0066 advances the qualified target to `x86-64-wvb-baseline-v7`. Every frame cell is a zero-initialized 16-byte value boundary; scalars use its low dword, while borrowed immutable bytes use an exact pointer/length/reserved descriptor. Static bytes, length, checked slicing, checked little-endian reads, `u8`/`u32` comparisons, widening, and checked `u32` arithmetic share JIT and WVO/AOT selection. A borrowed-byte argument passes a pointer to the caller descriptor and is copied immediately into the callee frame. Byte bounds return packed status 6 / `WVR3008`. The strict decoder proves descriptor provenance, typed call forms, range branches, reads, and scalar/descriptor slot separation. Windows, Debian, and firmware-probe-9 pinned-QEMU evidence is qualified at exact candidate `8d375bf`.

Decision 0067 advances the qualified target to `x86-64-wvb-baseline-v8`. Borrowed text now shares the pointer/length/reserved descriptor shape, and text parameters/locals can carry static or host-returned UTF-8. Service-table version 2 adds bounded `process.argument_count`, `process.argument`, and `file.read_bytes` beside console output. One execution owner bounds and frees host buffers; the reference and native hosts share file-snapshot semantics and exact resource errors. The checked-in `Wvb-Header-Inspector.wv` accepts and validates a real compiler-produced WVB under the interpreter and actual Windows/System V W^X execution. This milestone does not yet qualify full `wvdump` or dynamic allocation.

Decision 0068 advances the cross-host-qualified target to `x86-64-wvb-baseline-v9` at exact candidate `7edc243`. Enums retain canonical signed values and nominal identity. Immutable records use 32-bit offsets into one checked 1 MiB execution-owned arena, with one existing 16-byte value cell per field and deterministic `WVR3017` exhaustion. Execution-context version 2 carries the arena; service-table version 3 adds capability-free strict UTF-8 validation. The existing structural portion of `Wv-Dump-Core.wv` runs identically under the interpreter, Windows/Linux W^X JIT, and linked WVO/AOT; firmware probe 11 passes pinned QEMU. Dynamic text, descriptor returns, void calls, and diagnostic output still gate the complete hosted report path.

Decision 0069 advances the cross-host-qualified target to `x86-64-wvb-baseline-v10` at exact commit `7979933`. A fixed 16 MiB execution-owned text arena backs bounded enum names, invariant integer formatting, concatenation, and quoting; individual values retain the 1 MiB WVB bound and aggregate exhaustion is `WVR3018`. A hidden verified result cell admits descriptor returns without colliding with packed status in `RAX`; void calls use the same status path. Service-table version 4 adds those pure operations plus authorized diagnostics. The complete checked-in `Wv-Dump-Core.wv` agrees across the interpreter, Windows/Linux W^X JIT, and linked WVO/AOT. Both hosts pass all 15 OS tests, and firmware probe 12 passes pinned QEMU on Windows.

Decision 0070 cross-host qualifies the first runtime-native service without advancing ABI 10. Strict UTF-8 validation now executes as one exact platform-neutral x86-64 leaf instead of a managed delegate plus Windows/System V adapter. The managed decoder remains the oracle, while exhaustive encoding boundaries and exact service bytes are covered in the focused native loop. Windows and Linux W^X execution agree; every allocation-bearing and hosted service remains managed.

Decision 0071 cross-host qualifies ABI 11 and context version 3 at exact commit `8888951`. The context appends the 16 MiB text-arena base, capacity, one shared managed/native cursor, and exact service-failure detail. Concatenation and signed/unsigned integer formatting now execute as three exact platform-neutral x86-64 leaves; enum naming and deterministic quoting continue to share the same arena through managed adapters. Windows and Debian W^X paths agree, normalized contracts and all 61 portable artifacts match, both hosts pass all 15 OS tests, and pinned-QEMU probe 13 passes on Windows.

Decision 0072 cross-host qualifies the final two pure runtime services at exact commit `f97d221` without advancing ABI 11, context 3, service-table 4, or firmware probe 13. Enum naming uses one exact native leaf plus an adjacent bounded, independently verified runtime-private `WVEN` metadata block reconstructed from canonical fragment types. Deterministic quoting uses one exact two-pass strict-UTF-8 native leaf and preserves the existing UTF-16-code-unit escape contract. All six deterministic pure services are native and agree across Windows and Debian W^X paths; normalized contracts and all 61 portable artifacts match, both hosts pass all 15 OS tests, and pinned-QEMU probe 13 remains exact. The five hosted/capability adapters and Stage 0 construction, verification, W^X publication, arena ownership, and execution remain managed.

Decision 0073 cross-host qualifies ABI 12 and context version 4 at exact commit `328e455`. The context appends an execution-owned immutable argument-table pointer and count. The Stage 0 owner packs at most 67 already validated strict-UTF-8 arguments, independently rereads every descriptor/range/byte before publication, and releases the snapshot after execution. Exact 5-byte count and 70-byte checked descriptor-copy leaves replace both managed argument delegates and platform adapters while retaining capability preflight and generated call shapes. Windows and Debian W^X paths agree, normalized contracts and all 61 portable artifacts match, both hosts pass all 15 OS tests, and pinned-QEMU probe 14 passes on Windows. Console, diagnostic, and file input are now the three managed runtime callbacks.

Decision 0074 cross-host qualifies ABI 13 and context version 5 at exact commit `66b273f`. A runtime-private 48-byte `WVIO` table supplies explicit console and diagnostic targets and the narrow Windows writer boundary. Exact Windows `WriteFile` and Linux `write` leaves emit strict UTF-8 plus LF directly, handle partial writes, preserve the Windvale counters/context, and return stable `WVR3029` on an OS-rejected write. Generated fragment and service-table shapes stay unchanged. Windows and Debian W^X paths agree, normalized contracts and all 61 portable artifacts match, both hosts pass all 15 OS tests, and pinned-QEMU probe 15 passes on Windows. The C# Stage 0 owner still constructs, verifies, pins, publishes, and releases the output table and leaves. `file.read_bytes` is now the sole managed runtime-service callback.

Decision 0076 cross-host qualifies ABI 14 and context version 6 at exact commit `ef08619`. A runtime-private 136-byte `WVFI` table owns bounded path scratch, 64 canonical name/data snapshot slots, and the narrow Windows function-pointer boundary. Exact Windows and Linux leaves perform file input without a managed delegate, preserve first-success immutable snapshots, and map native failures back to stable `WVR302x` details. All eleven service-table slots now have qualified native leaves. Windows and Debian reports and all 61 portable artifacts agree, both OS suites pass, GitHub passes independently, and pinned-QEMU probe 16 passes on Windows. Decisions 0077 and 0078 then qualify both process-input templates and typed patch descriptions as Windvale-assembled WVOs while preserving their exact final bytes. Decision 0079 cross-host qualifies their exact validator and patch applier in portable Windvale. Decision 0080 cross-host qualifies the bounded byte-result entry and routes live argument-leaf construction through the retained Windvale consumer at exact commit `f547af8`; all 65 portable artifacts agree, both OS suites and GitHub pass, and probe 16 remains exact. C# Stage 0 still loads and lowers WVB, allocates, verifies, publishes, invokes, maps, and releases the runtime artifacts and retains the independent stencil oracle.

## Native runtime ABI

Generated code targets a Windvale-owned internal ABI rather than emitting host calls throughout ordinary functions. The current ABI-18 entry receives a pointer in `RDX` to the exact [native execution context](../../Specifications/Windvale-Native-Execution-Context.md). One identical `Main` preserves that pointer in `R15` and loads the versioned instruction/depth budgets into reserved `R11` and `R10`. Internal functions accept at most 64 scalar, enum, record-offset, or borrowed-text/byte parameters. The first four retain `R8`, `R9`, `RCX`, and `RDX`; later positions use verified 16-byte outgoing stack cells bounded to 960 bytes. Scalars, enums, and records use low dwords. Borrowed descriptors use a register pointer or complete stack cell that the callee immediately copies. Scalar/enum/record results and packed statuses return in `RAX`. ABI 18 retains this ABI-16 call convention and changes only deterministic physical storage for machine-IR values.

The context's optional service-table pointer is the only generated-code route to host or runtime support. ABI 18 retains ABI 16's closed version-5 table for console/diagnostic output, bounded argument/file input and file output, UTF-8 validation, enum naming, integer formatting, concatenation, and quoting; only capability services require authorization. All twelve slots use exact native leaves. Platform-specific output, file input, and file output read independently verified runtime-private tables, while the remaining leaves are platform-neutral. WVO 1.0 does not serialize fragment service, ABI, or nominal requirements, so a linked service-bearing image is not independently loadable without its verified fragment metadata. Host-returned text/bytes, text-arena values, argument-table values, file snapshots, and record-arena values are execution-owned and cannot escape the run. This remains a bounded experimental convention, not a stable public FFI or the final general-allocation, asynchronous-I/O, or safe-point ABI.

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
