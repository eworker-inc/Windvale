# Native execution and .NET retirement

## Status

Accepted architectural direction under [Decision 0057](../Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md). Decision 0058 qualifies bytecode compiler self-reproduction. Decisions 0059 through 0083 cross-host qualify the shared Stage 0 and first OS consumer seams through ABI 14, native leaves for all eleven then-current service slots, two Windvale-owned stencil consumers, a bounded byte-result entry, Windvale-owned executable-image layout and lifetime policy, and firmware probe 17's terminal invalid-opcode boundary. Decisions 0085 through 0087 cross-host qualify ABI 15/context 7, the twelfth exact native file-output leaf, WVA-owned Q35 shutdown and normalized trap entries, and composed firmware probe 20 at exact commit `12e9e2e`. Exact commits `860c69c`, `4a077ab`, `484c228`, `a35c348`, and `a63ca0f` qualify ABIs 16 through 20. Decisions 0119, 0122, 0124, 0127, 0130, and 0132 cross-host qualify paired capability-free PE/ELF targets, normalized process results, atomic publication, exact WVA startups, and Windvale-owned layout, construction, and verification at descendant `ea1aa89`. [Decision 0133](../Decisions/0133-Frame-Owned-Direct-Native-Records.md) consumes deterministic record-storage offsets in ABI 21 and rebuilds Probe 32. [Decision 0150](../Decisions/0150-Bounded-Native-Dynamic-Value-Lifetimes.md) advances the shared backend to ABI 22 and completes native Stage 1-to-Stage 2 compiler reproduction at descendant `2591cd5`. [Decision 0156](../Decisions/0156-First-Standalone-Hosted-Console-Capability.md) cross-host qualifies the first serialized standalone `console.write_line` capability in paired version-2 containers at `ed4a0b4`. [Decision 0213](../Decisions/0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md) freezes forward C# source-language growth at the next qualified WVB 1.11 baseline and selects the native source-to-verified-WVB front door as the first normal-path cutover. [Decision 0215](../Decisions/0215-Native-Wvb-Verify-And-Inspect-Front-Door.md) qualifies native WVB verification and deterministic inspection at `e2d9c52548fd782a57765b1a9635d8cbe009df20`. [Decision 0217](../Decisions/0217-Windvale-Sha256-And-Native-Wvb-Runner-Profile.md) implements the next bounded runner candidate with SHA-256 and interpretation retained in Windvale source; dual-host qualification is pending. Decisions 0226, 0227, 0229, and 0230 expand its native plan to five successful portable fixtures, three exact runtime failures, five malformed-WVB envelope rejections, eight typed-execution corruptions, and one control-reachability corruption. Decisions 0221 through 0224 add native linker, WVO read-only, version-1 console-packager, and accepted-subset WVB-to-WVO candidates while reusing existing platform startups. [Decision 0225](../Decisions/0225-Native-Source-To-Aot-Composition-Proof.md) composes those process boundaries with the qualified native source builder into one fixed current-host source-to-executable proof. [Decision 0228](../Decisions/0228-Bounded-Acyclic-Native-Call-Directory.md) expands native lowering to eight decreasing-ordinal scalar/control functions through a focused layout directory while preserving earlier WVO bytes. [Decision 0231](../Decisions/0231-Native-I32-Static-Data-Lowering.md) adds one bounded immutable i32 declaration and exact canonical `Sum-Data.wv` `.rodata`/relocation lowering through focused data and object modules. [Decision 0232](../Decisions/0232-General-Native-Call-Directory.md) replaces the decreasing-ordinal/Main-last restriction with a complete signature pass, arbitrary exported-Main order, and bounded general scalar calls under ABI 22's instruction/depth budgets. [Decision 0233](../Decisions/0233-Bounded-Native-U8-U32-Scalars.md) adds the `u8`/`u32` and typed-return subset required by compiler-produced `Function-Only.wv`. [Decision 0234](../Decisions/0234-Bounded-Native-Scalar-Comparisons.md) completes the bounded `u32`/`u8` comparisons shared by all remaining compiler-produced fixtures. [Decision 0235](../Decisions/0235-Bounded-Static-Descriptor-Lowering.md) adds bounded multiple immutable data and static borrowed text/bytes descriptor lowering through a focused instruction-state module. [Decision 0236](../Decisions/0236-Bounded-Native-Text-Services.md) adds bounded service-backed text concatenation, UTF-8 validation/conversion, and quoting. [Decision 0237](../Decisions/0237-Bounded-Native-Bytes-Concatenation.md) adds bounded generation-owned bytes concatenation through a focused instruction-template module. [Decision 0238](../Decisions/0238-Bounded-Native-Enum-Lowering.md) adds bounded nominal-type admission plus enum locals, constants, comparisons, and name lookup through focused type and enum modules. [Decision 0239](../Decisions/0239-Bounded-Direct-Record-Lowering.md) adds bounded direct record construction, local storage and copying, and field reads through deterministic frame-owned field ranges. Their grouped dual-host gate and artifact promotion remain pending. This document defines the larger native destination and migration boundaries; it does not claim a general in-guest WVB loader/verifier, a general Windvale-owned native runtime, broad JIT or AOT compiler, general hosted-service surface, garbage collector, complete native toolchain, or .NET retirement.

[Decision 0240](../Decisions/0240-Bounded-Native-Record-Calls.md) now adds nonzero-first enum admission and bounded one-block record parameters, caller-owned returns, and calls. Its focused call-instruction extraction keeps the Windvale tool under the existing native frame bound; multi-block record liveness and the scalar-returning record consumer required by compiler-produced `Nominal-Types.wv` remain the next lowering boundary.

[Decision 0241](../Decisions/0241-Multi-Block-Native-Record-Liveness.md) closes that boundary with fixed-point record-local liveness, block-scoped record-temporary allocation, and scalar-returning record consumers. The exact compiler-produced `Nominal-Types.wv` WVO now agrees across Stage 0, the Windvale adapters, and the direct current-host native package; grouped Windows/Linux qualification and artifact promotion remain pending.

[Decision 0242](../Decisions/0242-First-Hosted-Capability-In-Native-Lowering.md) crosses the next backend boundary with exact hosted capability-table admission and the first `process.argument_count() -> u32` service-table call. Portable modules still require no capabilities, other capability calls remain rejected, and WVO 1.0 does not yet carry independently loadable required-service metadata.

[Decision 0243](../Decisions/0243-Native-Process-Argument-Capability.md) adds `process.argument(u32) -> text` with exact borrowed-descriptor ownership and runtime-service failure propagation. Both process-input leaves now lower through Windvale source and the direct current-host package; file and output capabilities remain separate pending slices.

[Decision 0244](../Decisions/0244-Native-File-Read-Bytes-Capability.md) adds `file.read_bytes(text) -> bytes` with exact immutable-snapshot borrowing and runtime-service failure propagation. Capability-specific analysis and emitted-result state now live in the focused capability module after the general instruction core reached its native frame limit. File mutation and console/diagnostic output remain separate pending slices.

[Decision 0245](../Decisions/0245-Native-File-Write-Bytes-Capability.md) adds `file.write_bytes(text, bytes) -> void` while preserving the existing whole-value bound, externally visible replacement, durable-success condition, and failure propagation. Both file-byte leaves now lower through Windvale source; console and diagnostic output remain separate pending slices.

[Decision 0246](../Decisions/0246-Native-Console-Write-Line-Capability.md) adds `console.write_line(text) -> void` with exact text-plus-LF behavior and the existing partial-visibility failure boundary. The real hosted lowerer's successful input, publication, and reporting path now lowers through Windvale source; diagnostic usage and rejection output remain the final capability slice.

[Decision 0247](../Decisions/0247-Native-Diagnostic-Write-Line-Capability.md) adds `diagnostic.write_line(text) -> void` through the separate diagnostic channel and a shared focused text-output emitter. All six hosted calls declared by the real lowerer now lower through Windvale source; its broader self-lowering blockers and grouped Windows/Linux qualification remain.

[Decision 0248](../Decisions/0248-Measured-Native-Lowering-Module-Envelope.md) replaces the lowerer's three eight-entry prototype guards with a measured bounded envelope of 512 functions, 64 immutable data declarations, and 64 nominal types. Canonical D4 WVO helper and data names are now generated through the focused layout module. The real hosted tool crosses these table boundaries; its remaining instruction and shape gaps and grouped Windows/Linux qualification remain.

[Decision 0249](../Decisions/0249-Bounded-Native-Descriptor-Calls.md) adds `text` and `bytes` helper parameters and returns within the existing four-register call envelope. Complete descriptor cells pass by address, caller-owned result cells pass in `RAX`, and the callee preserves borrowed values or validates and compacts arena-owned returns at its saved checkpoint. The focused descriptor-call emitter keeps this ownership-heavy machine logic outside the large core; stack-passed arguments and grouped Windows/Linux qualification remain.

[Decision 0251](../Decisions/0251-Bounded-Native-Wide-Calls.md) extends that same ABI 22 call envelope to its bounded 64-parameter maximum. The first four representations stay in registers; each later scalar, record handle, or complete descriptor uses one canonical 16-byte outgoing cell, and caller-owned result addresses account for the temporary stack adjustment. A focused call-argument module removes duplicated register-only emission from the already-large core and adjacent modules. The real hosted lowerer now crosses its measured six-through-16-parameter helpers and reproduces through the pinned native source front door; grouped Windows/Linux qualification remains.

[Decision 0140](../Decisions/0140-Per-Module-Platform-Scope-And-Filesystem-Capabilities.md) additionally makes portability a per-part and derived artifact property. Native publication must preserve the canonical module's platform scope and capability requirements even when a particular application intentionally targets only one environment.

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

WVB is the verified cross-host distributable contract. An individual WVB may use only shared contracts or declare explicit platform-scoped requirements. WIR and the future native machine IR are compiler contracts. WVO, PE/COFF, ELF, in-memory linked code, and Windvale OS process images are target artifacts. None silently defines source semantics or discards the canonical module's platform and capability requirements.

Windows and Linux remain permanent Windvale hosts after .NET retirement. Windvale OS adds another platform implementation; it does not absorb or replace the host tool and application ecosystem.

## Compilation is a continuum

JIT and AOT describe when native compilation occurs, not competing language definitions:

| Time | Form | Primary use |
| --- | --- | --- |
| Build time | Deterministic AOT | Kernel, drivers, core tools, release applications |
| Install time | Target-local AOT or cache population | Portable packages deployed to a known machine |
| Load time | Eager JIT | Small complete modules where predictable latency matters |
| First call | Lazy baseline JIT | Ordinary applications whose declared providers are available |
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

Decision 0108 cross-host qualifies the next measured compiler operation as ABI 19. Verified `Bytesˉfromˉu8` allocates exactly one byte in the execution-owned arena, publishes a length-one borrowed descriptor, and stores the complete unsigned `u8` range. The independent decoder reconstructs the checked allocation and exact byte store, rejects scalar/result aliasing for both one- and four-byte encoders, and preserves ABI 18's typed physical map, call convention, context, service table, and limits. Exact compiler preflight now reaches `Bytesˉfromˉu16ˉlittle`.

Qualified Decision 0109 advances the implementation to ABI 20. Verified `Bytesˉfromˉu16ˉlittle` checks its complete `u32` source against 65,535 before arena mutation, writes an exact two-byte little-endian result, and maps larger inputs through generated-failure detail 12 to `WVR3016`. The independent decoder reconstructs the guard, allocation, descriptor, store, distinct cells, and both failure edges. Exact compiler preflight now completes lowering and selection and measures a 4,556,121-byte fragment against the retained 1,048,576-byte code limit. Exact implementation commit `a63ca0f` passes complete Windows/Debian qualification in GitHub run 30766123518.

Qualified Decision 0111 attributes 4,555,263 of those bytes to function code across 328 functions and 191,632 machine-IR operations. Because eliminating all 1,360,840 bytes of current frame initialization would still leave more than 3 MiB, the baseline retains one contiguous independently decoded image and expands only its hard fragment ceiling to 8 MiB, below the qualified 34 MiB W^X publication bound. ABI 20, context 7, service table 5, and machine bytes remain unchanged. The exact compiler now passes independent decoding and live publication, then reaches the retained 1 MiB immutable-record arena as `WVR3017` before output. WVO/object and flat-linker 4 MiB ceilings remain separate AOT boundaries. Exact commit `e139e4e` passes complete Windows/Debian qualification in GitHub run 30768107059.

Qualified Decision 0112 measures the exact compiler before revising that execution-memory boundary. It consumes exactly 1,480,096 record-arena bytes and 4,340,388 text-arena bytes while compiling the existing function-only fixture. A 2 MiB host record arena leaves 617,056 bytes of headroom and lets the native compiler publish the exact 815-byte Stage 0 WVB with identical success output and no diagnostics. The arena remains checked, monotonic, immutable, and execution-scoped; ABI 20, context layout, generated code, the 16 MiB text arena, and independently sized Windvale OS contexts do not change. Exact commit `bbec1ae` passes complete Windows/Debian qualification in GitHub run 30769250223; the 4 MiB WVO/object and flat-linker boundaries remain separate AOT work.

Qualified Decision 0115 measures the full native Stage 1 inventory rather than extending that single-source capacity conclusion. The exact 12-module workload exhausts diagnostic 64 MiB and 256 MiB monotonic record arenas; a successful reference profile attributes at least 77,821,091 constructed fields, equivalent to more than 1.24 GB in ABI 20's current layout. The retained 2 MiB limit therefore stays a useful bound rather than becoming an accidental whole-compiler memory promise. An opt-in semantic profiler and a fast ordinary-capacity `WVR3017` test preserve the evidence. Before a reclaiming ABI is selected, native machine IR must retain nominal record identity for every parameter, local, semantic value, call, and return so storage sizes and copies can be verified explicitly. Exact integration commit `05e5ef1` passes complete Windows/Debian qualification in GitHub run 30771491421.

Qualified [Decision 0117](../Decisions/0117-Nominal-Native-Record-Storage-Plan.md) preserves that nominal identity without changing ABI 20 or selected bytes, then derives deterministic reusable storage from native control-flow and value liveness. Across the exact compiler, 137,512 declared record-local field cells compact to 9,291 persistent cells summed across functions, while 88,669 coarse record-slot field cells compact to 7,463 peak-live scratch cells. The largest projected function frame is 1,489 cells (23,824 bytes), below the retained 2,048-cell envelope. This is implementation evidence for caller-owned record returns and frame-owned direct-record storage in a future ABI 21; the current runtime remains monotonic, the full native bootstrap still reaches `WVR3017`, and nested records are not admitted. Exact implementation commit `57416d0` passes complete Windows/Debian qualification in GitHub run 30773327094.

Qualified [Decision 0118](../Decisions/0118-Deterministic-Native-Record-Storage-Offsets.md) publishes the exact next selector input. Every owned record local and semantic result receives an absolute projected-frame cell offset; borrowed parameters and non-record identities receive `-1`. One deterministic width-first interference allocator places persistent CFG lifetimes and block-local result lifetimes into separate contiguous regions, followed by the optional caller-result pointer. Independent tests reconstruct both lifetime models and reject overlap or region escape. The exact compiler's scratch map has no fragmentation, retains the 1,489-cell maximum, and has canonical map digest `aff287fba46a840e454e4cc7bf4751d3152474caf09331a526f3730ba280816e`. ABI 20 bytes and execution remain unchanged. Exact implementation commit `060cf48` passes complete Windows/Debian qualification in GitHub run 30774669075.

Cross-host-qualified [Decision 0133](../Decisions/0133-Frame-Owned-Direct-Native-Records.md) consumes those maps in the single shared selector and advances host and OS consumers to ABI 21. Record handles are frame-backing pointers; construction and local movement copy complete direct fields; calls pass backing pointers; record returns copy into a caller-owned hidden destination. The independent decoder reconstructs nominal tags, full field widths, frame ranges, stack pointer/descriptor distinctions, and record call/return agreement. Repeated construction, exact single-source compiler execution, and rebuilt Probe 32 consume zero record-arena bytes. The full bootstrap clears `WVR3017` and reaches the retained dynamic text/byte boundary `WVR3018`. Explicit copies grow the exact fragment to 16,905,513 bytes, so the synchronized Stage 0 and Windvale publication limit becomes 32 MiB while the 34 MiB image ceiling remains. The OS stack proof reads projected ABI-21 frames directly; its exact path remains in six pages, while seven additional RX pages advance kernel memory to `WVKMEM11`.

Locally implemented [Decisions 0136](../Decisions/0136-Exact-Compiler-Dynamic-Value-Pressure.md) and [0141](../Decisions/0141-Exact-Compiler-Dynamic-Value-Lifetime.md) measure the next boundary without changing ABI 21. The successful exact compiler constructs 902,262,268 flat dynamic bytes cumulatively but reaches an ideal peak of 9,030,829 bytes across 17 typed backing identities, including input/output copy overlap, and releases every root by completion. [Decision 0143](../Decisions/0143-Bounded-First-Fit-Dynamic-Arena-Replay.md) completes the concrete capacity test: a 16-byte-header, 16-byte-aligned first-fit/coalescing policy admits all 1,852,773 allocations, peaks at 9,031,216 charged bytes, reaches address 10,700,368 despite 7,324,224 bytes of peak external fragmentation, and restores the full retained 16 MiB arena. [Decision 0147](../Decisions/0147-Native-Descriptor-Ownership-Plan.md) publishes the next selector boundary as 186,557 exact-compiler ownership actions. Cross-host-qualified [Decision 0148](../Decisions/0148-First-Wva-Native-Descriptor-Allocator-Leaf.md) supplies the executable 2,989-byte WVA first-fit/reference-count leaf and live independent differential evidence. [Decision 0151](../Decisions/0151-Native-Descriptor-Allocator-Emission-Schedule.md) maps its 180,190 invocations to exact physical owner locations and five phases, splits 180,168 generated-code calls from 22 runtime-service acquisitions, appends candidate context-8 state/leaf pointers, reserves three request cells in 265 functions, and requires `R10`/`R11` budget preservation. The largest projected frame is 1,492 of 2,048 cells. Decision 0150's ABI-22 generation/checkpoint policy still does not invoke the leaf; owner-token copies, selected calls, service migration, and shared host/OS rebuilds remain the later full-allocator gate. This does not select a permanent collection mechanism.

Cross-host-qualified [Decision 0150](../Decisions/0150-Bounded-Native-Dynamic-Value-Lifetimes.md) resolves that measured boundary without introducing a second compiler or unbounded allocator. ABI 22 requires Decision 0147 ownership-plan agreement, propagates complete descriptor words, gives larger generated byte buffers checked capacity/generation headers, reuses only a valid owner at the arena tail, and verifies function-entry checkpoints that reset or compact direct descriptor results. Scalar-only record returns also roll back; descriptor-bearing aggregates deliberately wait for caller-liveness evidence. The exact 17,130,441-byte compiler passes independent decoding and compiles all 12 canonical sources to a byte-identical 599,868-byte Stage 2 while peaking at 64,476,249 bytes in a 64 MiB host arena. The shared Probe-34 client grows to 110 RX pages under `WVKMEM13`; its stack, resource, and guest contracts remain unchanged. Exact descendant `2591cd5` passes complete Windows and digest-pinned Debian qualification in GitHub Verify run 30797770080.

The Decision 0201 local compiler candidate preserves ABI 22's ownership and allocator model but grows to 26,299,864 native bytes and peaks at 104,885,093 dynamic-value bytes while reproducing its exact 859,555-byte Stage 2. Explicit large-native WVO/link admission advances to 32 MiB, the ordinary host and version-2/3 hosted-container arena advances to 128 MiB, and the compiler instruction ceiling advances to 48,000,000,000; narrow version-1 containers remain 16 MiB. Focused native, WVO/AOT, raw Windows PE, and WebAssembly compiler paths pass locally. Independent Debian execution and dual-host identities remain pending, so the preceding Decision 0150 result remains the latest cross-host-qualified capacity evidence.

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

Generated code targets a Windvale-owned internal ABI rather than emitting host calls throughout ordinary functions. The current ABI-22 entry receives a pointer in `RDX` to the exact [native execution context](../../Specifications/Windvale-Native-Execution-Context.md). One identical `Main` preserves that pointer in `R15` and loads the versioned instruction/depth budgets into reserved `R11` and `R10`. Internal functions accept at most 64 scalar, enum, record-pointer, or borrowed-text/byte parameters. The first four retain `R8`, `R9`, `RCX`, and `RDX`; later positions use verified 16-byte outgoing stack cells bounded to 960 bytes. Scalars and enums use low dwords, records use one pointer-sized word, and borrowed descriptors use a register pointer or complete two-word stack cell. Scalar and enum results return with packed status in `RAX`; descriptors and records use caller-owned destinations, with records copied field-by-field before returning zero status. ABI 22 retains ABI 21's frame-owned records and adds independently verified descriptor ownership/checkpoint shapes without changing portable semantics.

The context's optional service-table pointer is the only generated-code route to host or runtime support. ABI 22 retains ABI 16's closed version-5 table for console/diagnostic output, bounded argument/file input and file output, UTF-8 validation, enum naming, integer formatting, concatenation, and quoting; only capability services require authorization. All twelve slots use exact native leaves. Platform-specific output, file input, and file output read independently verified runtime-private tables, while the remaining leaves are platform-neutral. WVO 1.0 does not serialize fragment service, ABI, nominal, or arena requirements, so a linked service-bearing image is not independently loadable without its verified fragment metadata. Host-returned text/bytes, text-arena values, argument-table values, file snapshots, and frame-owned records are execution-owned and cannot escape the run. This remains a bounded experimental convention, not a stable public FFI or the final general-allocation, asynchronous-I/O, or safe-point ABI.

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

The normal post-retirement bootstrap uses a documented previous native Windvale release to rebuild the next. Exact source, seed binaries, manifests, signatures or digests, target identities, and Stage comparisons form the trust record. [Decision 0178](../Decisions/0178-Project-Stewardship-Archives-And-Recovery.md) makes this an incremental recovery stream: update the runbook and identities during ordinary work, retain milestone snapshots, and construct one final clean dual-host recovery release before retirement. That final .NET Stage 0 release remains recoverable historical evidence unless a later decision selects and qualifies another minimal from-zero path. The [operational retirement inventory](../Project/Dotnet-Retirement-Inventory.md) tracks each product surface, direct managed entry point, and retained source owner until that gate closes.

Decision 0213 deliberately separates the semantic-freeze checkpoint from final retirement. Exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b` qualifies the WVB 1.11 freeze baseline, so the C# source compiler no longer receives new language features while remaining correctable recovery evidence. The qualified native build driver and verifier replace the ordinary source-to-verified-WVB entry point after exact artifacts, provenance, and atomic publication are available. Decision 0217's fixed WVB runner and digest-checking launchers are pinned as the current execution candidate. Decision 0218 then implements the first native test-orchestration candidate as a fixed digest-bound plan over stable WVB fixtures and structural result oracles rather than a line-for-line port of the C# harness; Decisions 0226, 0227, 0229, and 0230 expand it to five successful portable fixtures, three exact runtime failures, five malformed-WVB envelope rejections, eight typed-execution corruptions, and one control-reachability corruption without adding a broad coordinator. Decisions 0221 through 0224 extend the same transfer to standard flat linking, WVO read-only inspection, bounded version-1 PE/ELF materialization, and accepted-subset WVB-to-WVO lowering. Decision 0225 composes the qualified native source builder and those candidates as separate processes into one fixed current-host executable proof; Decision 0231 extends the lowering candidate with canonical bounded i32 static data, `.rodata`, and typed relocation ownership; Decision 0232 admits arbitrary exported-Main order plus bounded general scalar calls and recursion; Decision 0233 adds bounded `u8`/`u32` values and typed scalar returns against `Function-Only.wv`; Decision 0234 completes the bounded `u32`/`u8` comparison families; Decision 0235 adds exact multiple immutable data plus static borrowed descriptor locals, views, slicing, length, and little-endian reads; Decision 0236 reuses ABI 22's bounded runtime services for text concatenation, UTF-8 validation/conversion, and quoting; Decision 0237 adds bounded generation-owned bytes concatenation through a focused instruction-template module; Decision 0238 adds the first nominal slice with bounded type-table parsing and enum locals, constants, comparisons, and name lookup; and Decision 0239 adds bounded direct record construction, local copying, and field access over deterministic frame-owned backing storage. Stage 0 still constructs unpromoted tool packages until the grouped source and pinned-artifact gates pass. Native test coverage, complete backend transfer, remaining hosted packaging, release recovery, and final archive work continue under the complete Decision 0057 gate.

Decisions 0301 through 0304 pin the current WVO read-only, flat-linker, bounded
console-packager, and accepted-subset lowerer candidates behind digest-bound
Windows/Linux launchers without promoting them. Decision 0305 composes those
launchers with the qualified native source builder into a permanent fixed-vector
test: exact WVB, WVO, link map, raw image, and host application identities lead
to direct result 42 without a managed child or live C# oracle. Stage 0 still
constructs several host-tool containers, and Linux execution plus grouped
promotion remain pending.

Decision 0307 closes the current console packager's publication half. A focused
Windvale tool admits the completed version-1 PE/ELF through the portable verifier,
then reuses the qualified native WVB publication transaction and platform adapters
for exclusive-sibling durable write, exact reread, atomic replacement, directory
durability, and cleanup. Digest-bound packaging no longer needs managed
publication, but the publisher WVB and host containers remain Stage 0-constructed
until native construction and the grouped Windows/Linux gate pass.

Decision 0340 extends that same publication boundary to format-2 hosted PE/ELF
without replacing the version-1 recipe modules. Focused portable common,
Windows, and Linux verifiers own canonical startup, metadata, SHA-256, imports,
segments, and native recovery behind one shared dispatcher. The normal publisher
no longer needs C# to admit ordinary hosted containers, while Stage 0 remains
the constructor and frozen independent recovery oracle until final qualification.

Decision 0341 freezes the two version-1 maximum-plus-one PE/ELF boundaries as
bounded two-snapshot inputs and transfers their exact rejection ordering to a
dedicated read-only Windvale verifier. Decision 0342 adds the accepted sibling:
a focused recipe streamer constructs each maximum valid application as one exact
4 MiB chunk, one bounded remainder, and a `WVCS 1.0` manifest, then a separate
portable wrapper revalidates those values and recovers the original native image
without joining the completed container. The outputs match the independent
Stage 0 PE/ELF identities. The host packages are still Stage 0-constructed
candidates; native reconstruction, durable segmented publication, Linux
execution, and grouped promotion remain required before this boundary retires.

Decision 0308 closes the current accepted-subset lowerer's whole-object
publication half. Complete WVO admission moves into one focused portable module
shared by the read-only inspector and a new five-service publisher. The lowerer
launchers now write a private candidate before the same native transaction owns
durable replacement and cleanup. The repinned inspector and new publisher remain
Stage 0-constructed candidates pending Linux execution, native host-container
construction, and grouped promotion.

Decision 0310 extends the fixed native test boundary from WVB-only inputs to one
canonical accepted WVO plus bad-magic, truncated, and trailing-byte objects. The
host adapters pin complete input and report identities and invoke only the native
WVO verifier; no live C# oracle or additional object parser enters the normal
test path. Broader malformed and randomized WVO coverage remains in the explicit
recovery lane until separately transferred.

Decision 0240 extends that accepted subset with nonzero-first enum tables plus one-block record parameter, caller-owned return, and call transport. The bounded C# enum-admission correction preserves Stage 0 as the independent WVB 1.11 oracle rather than adding forward language semantics there.

Decision 0241 extends record storage through validated control-flow successors and admits scalar-returning record consumers. Its planner keeps persistent locals live across edges while requiring scratch record values to die inside their defining block, and its row-wise immutable fixed point remains within the native tool's bounded arena on the real nominal fixture.

Decision 0242 then admits exact portable-or-hosted profile rules, validates the six current Stage 0 native capability signatures, and lowers only the parameterless scalar `process.argument_count` call. The focused capability module and canonically ordered project dependencies keep the ordinary native source front door usable without folding more policy into the large instruction core.

Decision 0243 extends that focused boundary with `process.argument`, passing its scalar index and borrowed-text output through ABI 22's service table and existing runtime-service failure tail. The accepted WVO remains package-bound because WVO 1.0 does not serialize its required service.

Decision 0244 adds the read side of the real hosted lowering shell. It passes a borrowed resource-name descriptor to service-table slot 32, receives a service-owned immutable bytes snapshot, and keeps capability analysis and emission state outside the general instruction core. The direct current-host package reproduces Stage 0's exact file-read fixture WVO; grouped dual-host qualification remains pending.

Decision 0245 adds the mutation side through service-table slot 96. It consumes borrowed text and bytes descriptors, produces no value, and retains the exact create-or-replace, 4 MiB, durable-flush, and non-atomic contract. The direct current-host package reproduces Stage 0's exact file-write fixture WVO; grouped dual-host qualification remains pending.

Decision 0246 adds successful reporting through service-table slot 8. It consumes one borrowed text descriptor, emits the exact text plus LF, produces no value, and retains runtime-service failure propagation without promising atomic visibility or retry. The direct current-host package reproduces Stage 0's exact console-output fixture WVO; grouped dual-host qualification remains pending.

Decision 0247 adds usage and rejection reporting through service-table slot 48. It reuses the verified text-output machine shape while retaining a distinct capability identity, service grant, and diagnostic sink. The direct current-host package reproduces Stage 0's exact diagnostic-output fixture WVO, completing all six hosted calls admitted by this candidate; grouped dual-host qualification remains pending.

Decision 0248 admits the real hosted tool's 297 functions, 33 immutable data declarations, and 29 nominal types within explicit 512/64/64 bounds. One layout-owned generator replaces hard-coded names beyond ordinal seven while retaining Stage 0's exact `$function_0000` and `$data_0000` D4 contracts. A 9-data, 9-type, 10-function crossing fixture agrees byte-for-byte through Stage 0 and both Windvale adapters; later unsupported instructions and shapes remain separate measured blockers.

Decision 0249 admits descriptor parameters and returns inside the retained four-register directory. Callers pass complete descriptor cells by address and descriptor-result destinations in `RAX`; callees preserve external values and compact owned returns against a hidden arena checkpoint. The current 321-function hosted tool now reaches its first six-parameter helper, selecting stack-passed argument transport as the next measured blocker.

Decision 0251 closes that measured blocker by expanding the internal directory to 64 padded parameter types and matching ABI 22's exact register-plus-stack transport. The updated 330-function adapter closures reproduce their Stage 0 WVB identities through the pinned native build driver, and the widened descriptor fixture reproduces the complete Stage 0 WVO. Enum parameters/returns, multiple record arguments, and remaining instruction shapes are now the next measured backend gaps.

Decision 0254 replaces three smaller prototype guards with one measured general-function envelope: fewer than 2,048 combined parameters/locals, at most 32,768 code bytes, and at most 8,192 instructions, still subject to the exact 2,048-cell native frame check. The real 330-function tool fits those general bounds. Complete signature preflight next rejects an enum parameter at function ordinal 117; static inspection also identifies independent later gaps at function 18's four-record call and the former ordinal-88 record-planner capacity.

Decision 0256 compacts record-local liveness from all local cells to a directory of at most 256 declared record locals, raises the independently bounded control envelope to 1,024 blocks and 8,192 instructions, and stops the immutable fixed-point pass once stable. Expanded offsets retain the original local-index contract, and the hard 2,048-cell native frame remains unchanged. A compact 129-record-local, 130-block, 1,032-instruction fixture agrees with Stage 0.

Decision 0259 replaces the planner's single optional record-use event with a bounded ordered operand list, allowing every record argument admitted by ABI 22's existing 64-parameter call directory. Replay validates all operands before releasing last uses, and a four-record helper reproduces Stage 0's complete WVO through both Windvale adapters. Full-tool self-lowering remains fail-closed earlier in complete signature preflight at ordinal 117's enum parameter, which is the next active backend slice.

Decision 0260 admits bounded enum parameters and returns without creating another ABI representation: the call directory preserves nominal identity while the existing 32-bit scalar path carries the backing value. Returned enums use the existing enum value-slot group, and the focused `Keep(Weather) -> Weather` fixture agrees exactly with Stage 0. Full-tool self-lowering now reaches code analysis and stops at `Main` offset `0x01D1`'s `u32.format` instruction.

Decision 0262 admits `u32.format` through ABI 22's existing service-table slot, with explicit unsigned input transport, caller-owned text-descriptor output, and shared runtime-failure propagation. The focused maximum-value fixture agrees exactly with Stage 0 through both Windvale adapters. Full-tool self-lowering now advances into its first byte-construction helper and stops at function 1 offset `0x0019`'s `bytes.from_u8` instruction.

Decision 0265 admits one-byte construction from `u8` through the existing bounded dynamic-byte arena and descriptor lifetime. Its machine template stays with byte concatenation so it can reuse the same patch machinery without expanding the pinned native compiler's binding surface with a duplicate helper module. The focused maximum-byte fixture agrees exactly with Stage 0 through both Windvale adapters. Full-tool self-lowering advances to function 2 offset `0x0019`'s `bytes.from_i32_little` instruction.

Decision 0267 extends that same fixed-width construction path to `bytes.from_i32_little`. Typed analysis replaces one signed scalar with one owned four-byte descriptor; lowering checks exact four-byte arena growth, publishes length four, and stores the complete little-endian scalar without a runtime service. The combined focused fixture agrees exactly with Stage 0 through both Windvale adapters. A complete ordinal scan corrects the initially reported next frontier: full-tool self-lowering clears functions 1 and 2, then stops in function 3 at offset `0x0233`'s `u32.multiply` instruction.

Decision 0268 admits `bytes.from_u32_little` through the same four-byte machine emitter as the signed constructor while retaining exact `u32` typed analysis. The combined focused fixture checks a high-bit unsigned value and agrees exactly with Stage 0 through both Windvale adapters. Because function 3 precedes the first unsigned-constructor helper, complete self-lowering remains fail-closed at the already measured `u32.multiply` frontier.

Decision 0269 closes the checked `u32` add/subtract/multiply family. Addition and subtraction use the exact carry/borrow branch while multiplication rejects a nonzero high product word; every route reaches the existing `WVR3007` tail. A focused high-value fixture and overflow vector agree exactly with Stage 0 through both Windvale adapters. Complete self-lowering now reaches function 26's `bytes.from_u16_little` instruction.

Decision 0271 adds bounded `bytes.from_u16_little` construction to the same owned dynamic-byte path. Typed analysis preserves the `u32` input, lowering rejects values above 65,535 through the exact `WVR3016` branch, and successful construction publishes a two-byte descriptor before storing the low word. The expanded focused fixture agrees exactly with Stage 0 through both Windvale adapters. Complete self-lowering now reaches function 29's `u32.from_u8` instruction.

Decision 0272 admits lossless `u32.from_u8` through the existing descriptor-instruction state. Canonical `u8` slots already contain the complete unsigned value, so the exact lowering is one 32-bit source-slot load and target-slot store with no runtime service or failure branch. Keeping the stack-state transition outside the large core preserves the retained 2,048-cell frame ceiling. The focused `0u8` and `255u8` fixture agrees exactly with Stage 0 through both Windvale adapters. Complete self-lowering now reaches function 36's `u32.remainder` instruction.

Decision 0277 closes the portable browser-compiler WVB seam with a separately pinned format-3 native source compiler. Its normal launcher verifies the native compiler and publisher, reads the exact project inventory, writes one temporary candidate, and reproduces the 919,577-byte compiler byte for byte without .NET. Decision 0278 closes the remaining interpreter-Wasm seam with a second named format-3 compiler-family member. After Decisions 0292 and 0294 add bounded direct static data and consume it for opcode effects, the native WebAssembly compiler reproduces the current exact 828,165-byte import-free execution-ABI-3 artifact, while the Node launcher independently checks its exports, memory regions, and atomic replacement. Decision 0296 admits and completely validates the exact compiler's 82 nominal declarations without changing primitive output. Decision 0297 consumes its bounded 417-function directory; Decision 0298 then decodes its complete 157,844-instruction stream and validates all 2,991 direct-call targets without a direction restriction or `u32` reachability mask. Decision 0306 adds a separate immutable 32-byte-entry directory that resolves every target's signature and code range in constant time while leaving the established backend and browser package unchanged. Decision 0309 proves typed agreement across all exact calls, and Decision 0312 resolves arbitrary `Main` index 2 into a deterministic 397-function reachable order plus all 2,991 ordered targets without the small selector's mask or direction rule. Control representation, nominal runtime values, general emission, and direct execution remain the compiler-Wasm boundary. Both package constructors remain Stage 0 recovery commands; no C# product implementation changed.

Decisions 0280 through 0288, 0290, 0291, 0293, 0295, 0299, and 0300 advance the large native-object seam without
widening Windvale's ordinary 4 MiB value contract. Immutable analysis evidence
precedes emission; the WVO writer exposes separately owned canonical regions;
maximal contiguous function batches carry bounded code and relocation values;
and a focused capability-free publication cursor yields exact positioned WVO
chunks through the planned final length. A focused hosted staging tool writes
those bounded chunks through the existing whole-value capability and writes a
small `WVOP 1` structural manifest last. The cursor and staging sequence
reproduce the independent Stage 0 object byte for byte. A focused portable
reader now owns the same canonical serializer and strictly rejects malformed
magic, version, lengths, limits, indices, positions, and final coverage before
host mutation. A capability-free scalar bridge carries those exact status
results and every admitted object/chunk extent across ABI 22's existing
borrowed-descriptor call convention. Its valid, out-of-range, and malformed
routes have executed as native machine code without a service. Platform
assembly no longer needs to parse `WVOP 1`. A separate segmented reader now
validates the exact compiler-produced WVO header, `.text`, optional `.rodata`,
32 MiB section extents, following chunk boundaries, and minimum symbol/
relocation tail from bounded metadata chunks. Its scalar bridge also executes
natively without a service. A following bounded reader validates the complete
compiler-produced symbol chunk, including exact data/function/Main names and
ordering, section coverage, Main's omitted ordinal/range, optional text padding,
and the exact relocation-table boundary. The following reader accepts only the
compiler's ordered section-zero `Relative_i32` relocations and data-symbol
targets, exposes the exact text-chunk count, proves every relocation field is a
zero placeholder in its owning code chunk, and proves separate padding contains
only `0x90`. A following typed content cursor replays that retained publication
plan, binds every nonempty value to its exact manifest position and length,
compares arbitrary code, data, and metadata bytes completely, and requires the
publication cursor to finish at the admitted object length. A following
resource plan assigns the input, manifest, and no more than 62 canonical chunk
names to the existing 64-entry first-success snapshot table. Its hosted
admission root reads those names once in exact ordinal order, passes the same
borrowed chunk snapshots through complete content validation, and performs no
destination mutation. Fixed Windows and Linux WVA adapters now independently
validate the native snapshot table, reopen and compare every resource against
its immutable snapshot, reject destination aliases by native identity, consume
only the verified chunk descriptors, and perform exclusive-sibling write,
flush, reread, exact-EOF, atomic-replacement, and cleanup transitions. Linux
also synchronizes the destination directory; Windows flushes the renamed file
handle. The Windows package executes the complete focused transaction without
loading .NET. The Linux package is structurally pinned but still awaits the
grouped execution gate. A separately digest-bound Windows/Linux staging
producer now emits the exact chunks and manifest, and the current-host native
producer/publisher processes compose on the canonical small fixture without
loading .NET. Full compiler self-lowering remains in the final grouped gate.
These layers deliberately
do not reuse the pre-opened
random-access-storage capability, whose contract
excludes path creation, replacement, and directory publication, and do not
claim that scratch resources are an atomic destination. Complete compiler
self-staging integration, native replacement of the Stage 0 package
constructor, extended fault/concurrency evidence, promotion, and grouped
dual-host qualification remain before this transfer replaces the managed
ordinary route.

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
