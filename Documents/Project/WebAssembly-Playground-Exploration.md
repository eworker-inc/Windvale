# WebAssembly and browser playground exploration

- Date: 2026-08-01
- Status: Accepted staged product direction under [Decision 0182](../Decisions/0182-Browser-And-WebAssembly-Product-Direction.md), with an implemented Stage 0 playground and bounded Windvale-authored backend; not yet an accepted permanent WebAssembly host or target

## Purpose

This document records the implementation and evidence inventory around a browser-based Windvale playground and possible WebAssembly host and compiler target. Decision 0182 accepts an early experimental Windvale-native route, a later default-route replacement gate, typed-WIR direct compilation, a bounded event direction, and separate permanent-host and compiler-target gates. It does not yet accept WebAssembly permanently. The implemented host boundary is specified separately in [`Specifications/Browser-Playground.md`](../../Specifications/Browser-Playground.md).

The direction under consideration is that portable Windvale source could eventually execute across:

- Windows;
- Linux;
- WebAssembly hosts, initially web browsers; and
- Windvale OS.

Windows and Linux remain accepted permanent hosts, and Windvale OS remains the vertical integration target. WebAssembly is not yet an accepted compiler target or runtime host. A useful Windvale-native route may be published earlier with an explicit experimental profile; permanent host and direct compiler-target acceptance remain separate later evidence decisions.

## Central distinction

A Windvale playground is not the same product as a browser-hosted Windvale OS demonstration.

- A playground compiles and executes portable or explicitly hosted Windvale programs inside a browser sandbox.
- An OS demonstration boots the x86-64 UEFI Windvale image in a machine emulator, whether that emulator runs locally, on a server, or through WebAssembly in the browser.

The playground does not need to emulate x86-64. It can operate on canonical WVB or compile Windvale semantics directly to WebAssembly. A browser OS demonstration must instead preserve the machine and firmware boundary or define and qualify another explicit OS environment.

## What WebAssembly would mean for Windvale

WebAssembly is a portable virtual instruction set and binary module format. It is not an x86-64 executable format and does not directly run Windvale's x86-64 WVO, PE, ELF, or UEFI output. A browser validates a WebAssembly module and translates it for the user's actual processor inside the browser sandbox.

The intended compiler relationship would be:

```text
Windvale source
        |
        +-- shared syntax and semantic analysis
        |
        +-- typed WIR for direct source compilation
        +-- canonical verified WVB for distribution and hosted execution
                    |
                    +-- x86-64 backend --> Windows/Linux/Windvale OS adapters
                    |
                    `-- WebAssembly backend or interpreter --> browser adapter
```

Portable language behavior must not change with the selected target. Checked arithmetic, text and byte behavior, traps, capability authorization, resource limits, and diagnostics remain Windvale contracts. Instruction selection, executable representation, machine ABI, and host adaptation are target responsibilities.

Canonical WVB should remain the portable distribution identity unless a later accepted decision changes that direction. A WebAssembly backend would be another execution target for verified semantics, not a replacement definition of the language.

## Candidate implementation routes

The routes below are stages with separate evidence value. The .NET route remains the active Stage 0 oracle, a bounded Windvale-native route may appear before complete replacement, and direct compilation remains a later target path. None is accepted as a permanent WebAssembly host or target merely by appearing here.

### Stage 0 compiler and interpreter hosted by .NET WebAssembly

The browser downloads a browser-compatible .NET runtime with the existing C# reference compiler, WVB verifier, and interpreter. A browser UI adapter supplies the editor and narrow capability controls; the current experiment uses Blazor components and requires no TypeScript application layer.

```text
Windvale source --> C# reference compiler --> WVB --> C# verifier/interpreter
                         all hosted by .NET WebAssembly
```

This is now the implemented Stage 0 experiment because it reuses the active semantic oracle. The reference compiler, bytecode verifier, reference runtime, and bounded browser host compile successfully for .NET WebAssembly. The native x86-64 executor and its executable-memory, native-pointer, `VirtualAlloc`, and `mmap` behavior remain outside the browser project.

This route would be an explicit Stage 0 browser host. It must not turn .NET into a permanent Windvale product dependency or weaken the existing native-retirement gate.

#### Current experiment evidence

The first implementation builds the compiler, verifier, interpreter, reusable playground boundary, and Blazor UI successfully with .NET 10 WebAssembly. A Release static publication made on 2026-08-01 contains 49 ordinary files totaling 9.201 MiB; the corresponding Brotli representations total 2.770 MiB. This is a local measurement, not a size contract, and the SDK reported that the optional `wasm-tools` workload was not installed for further publication optimization.

The experiment is published with the static project website at <https://windvale.ca/playground/> through Cloudflare Pages. Compilation, verification, and execution remain inside the user's browser; the site has no application server.

Focused tests exercise portable and hosted success, nominal records and enums, capability denial, malformed source, system-profile rejection, oversized input, and deterministic instruction-budget exhaustion. The Stage 0 compiler, verifier, backend interpreter, and general reference interpreter remain on the browser UI thread. The bounded generated-Wasm path now has its own disposable worker, but complete pipeline containment remains open.

### Windvale-native WVB interpreter compiled to WebAssembly

A Windvale-written interpreter and verifier execute canonical WVB inside a WebAssembly module. This preserves the bytecode-first model and can eventually replace the Stage 0 browser host.

This route depends on enough Windvale-native runtime, memory, and WebAssembly publication support to build and host the interpreter safely. It should be evaluated after the native Windows/Linux execution stack has supplied useful ownership and performance evidence.

### Direct Windvale-to-WebAssembly compilation

A WebAssembly backend lowers typed WIR or canonical verified WVB into a `.wasm` module. This can improve startup and execution performance and can publish independently loadable application modules.

It also requires the largest new compiler surface:

- WebAssembly instruction selection and structured control-flow lowering;
- module encoding and independent validation;
- a Windvale-to-WebAssembly value, call, memory, and trap ABI;
- deterministic capability imports and result conventions;
- source and diagnostic mapping;
- resource-accounting preservation;
- differential tests against the reference interpreter and native backend; and
- browser packaging and compatibility evidence.

The direct backend must not become a parallel language implementation. It should consume the same verified semantic evidence used by other execution modes.

[Decision 0102](../Decisions/0102-First-Windvale-WebAssembly-Backend-Slice.md) implements the first bounded direct slice in `.wv`. The portable selector revalidates one exact compiler-produced WVB shape, lowers `Main() -> i32` returning any constant, and emits a deterministic import-free and memory-free WebAssembly version-1 module. The first 37-byte module validates in an independent WebAssembly engine and returns `42`, matching the reference runtime.

[Decision 0104](../Decisions/0104-WebAssembly-Checked-Addition-And-Execution-Contract.md) adds the second exact profile and execution ABI 1. Generated Wasm performs checked `i32.add`, returns status `3007` for `WVR3007`, and publishes the same seven-or-ten attempted WVB instruction count as the reference runtime. Successful and overflowing modules both validate and run in Node.js without an engine trap.

[Decision 0106](../Decisions/0106-Bounded-Straight-I32-WebAssembly-Lowering.md) adds a third profile that validates and lowers one bounded straight-line `i32` instruction stream. It covers locals, discarded values, and checked addition, subtraction, multiplication, and negation while retaining execution ABI 1 and exact pre-execution instruction charging. Four profile-3 artifacts validate and run in Node.js with the same status/result/count tuples as the reference runtime.

[Decision 0107](../Decisions/0107-Playground-Disposable-WebAssembly-Worker.md) embeds and digest-pins that `.wv` backend in the playground, offers completed capability-free portable WVB to it, and executes successful output in a fresh two-second worker. The retained profile-3 example reports ABI 1, status 0, result 42, and 30 instructions equal to the reference path while exposing the exact WVB, Wasm, and backend identities.

[Decision 0110](../Decisions/0110-Standalone-Dotnet-Free-WebAssembly-Artifact-Demo.md) established the separate ordinary HTML/JavaScript route with the initial 432-byte ABI-1 artifact. Visiting and executing `/playground/wasm-demo/` starts no Blazor or .NET runtime. Its displayed source remains read-only, and Stage 0 still produces and qualifies the artifact.

[Decision 0113](../Decisions/0113-Metered-WebAssembly-Control-Flow.md) adds the first structured loop profile and execution ABI 2. The `.wv` selector validates one canonical `while` region, reconstructs it as a WebAssembly `block` and `loop`, and dynamically charges every WVB instruction. The terminating fixture succeeds exactly at budget 157 and returns `WVR3011` at 156; a nonterminating fixture returns the same deterministic status at budget 50. The .NET-free route now displays and executes the profile-4 loop artifact at both 157 and 156.

[Decision 0116](../Decisions/0116-Sequential-WebAssembly-Control-Regions.md) adds cross-host-qualified profile 5 for two or more sequential nonnested regions. The selector classifies compiler-produced `while`, `if`, and `if/else` shapes, rejects crossing or malformed targets, emits direct WebAssembly structured control, and preserves ABI-2 metering. Retained fixtures cover two sequential `if` statements and two loops surrounding both the true and false `if/else` routes. The deployed .NET-free route advances to the 1,923-byte mixed-control artifact at exact budgets 184 and 183.

[Decision 0120](../Decisions/0120-Bounded-WebAssembly-Call-Graph.md) adds local profile-6 evidence for two through eight acyclic direct functions with zero through two `i32` parameters. The selector rejects calls that do not target a lower canonical ordinal, which excludes recursion and statically caps depth at eight. It emits real private Wasm functions behind the unchanged ABI-2 wrapper and shares one exact instruction budget and status across all callees. The .NET-free route advances to the 1,185-byte three-function artifact at exact budgets 66 and 65.

[Decision 0121](../Decisions/0121-WebAssembly-Calls-With-Structured-Control.md) adds local profile-7 evidence for composing those real calls with sequential nonnested loops and conditionals. Retained fixtures call helpers from a loop and both conditional routes while one shared ABI-2 budget spans every caller and callee. The .NET-free route advances to the 2,729-byte composition artifact at exact budgets 196 and 195.

[Decision 0123](../Decisions/0123-Versioned-WebAssembly-Linear-Memory-And-Utf8-Buffers.md) adds local profile-8 and execution-ABI-3 evidence for fixed linear-memory transport. Exact compiler-produced `bytes -> bytes` and `text -> text` identities use disjoint 4 MiB host-input and guest-output windows, strict guest-side UTF-8 validation, and exact metering. The shared worker independently checks the layout and returned descriptor; the .NET-free route advances to an editable Unicode input over the 791-byte text artifact.

[Decision 0128](../Decisions/0128-Bounded-WebAssembly-Runtime-Values.md) adds local profile-9 evidence for a reusable straight-line primitive/bytes runtime over ABI 3. It statically verifies definite local initialization and typed stack flow, represents bytes as private packed descriptors, supports bounded reads, slices, widening, little-endian construction, and concatenation, and distinguishes value overflow, u16 narrowing, range failure, and aggregate monotonic-arena exhaustion. This foundation is deliberately shaped for the first Windvale-native WVB verifier; the static page remains on profile 8.

[Decision 0131](../Decisions/0131-Windvale-Native-WebAssembly-Wvb-Envelope-Verifier.md) adds cross-host-qualified profile-10 evidence for compiler-produced nested control over the same bounded runtime values. Validated WVB basic blocks lower through a metered Wasm program-counter dispatch loop. The first real consumer is a Windvale-written WVB 1.6 envelope verifier: valid input returns `[1]`, hostile or incomplete envelopes return `[0]`, and exact budget exhaustion remains `WVR3011`. It verifies the outer envelope only; section payloads, executable semantics, and source compilation remain later gates. The static page remains on profile 8.

[Decision 0134](../Decisions/0134-Windvale-Native-WebAssembly-Wvb-Structural-Verifier.md) adds local profile-11 evidence for complete bounded consumption of all seven WVB 1.6 payload schemas. A one-pass instruction-boundary mask and per-basic-block Wasm emission let the 4,062-instruction Windvale verifier lower inside the retained 100,000,000-step hosted gate. Its 113,385-byte import-free artifact accepts nonempty data/text, nominal-type, and hosted-capability modules and rejects targeted corruption in every section. This proves payload structure, not UTF-8/name, identity, index, type-flow, control-flow, reachability, stack, or authorization semantics. The static page remains on profile 8.

[Decision 0139](../Decisions/0139-Descriptor-Bearing-WebAssembly-Call-Graph.md) adds local profile-12 evidence for two through eight acyclic `bytes -> bytes` functions over the profile-11 runtime and control operations. Generated private `(i64) -> i64` Wasm functions share status, instruction budget, and the monotonic arena without exposing descriptors through ABI 3. The three-function fixture composes nested calls with a conditional route and agrees exactly with the reference runtime at success and one-instruction-short budgets. This is the function boundary needed to split the semantic verifier; the static page remains on profile 8.

[Decision 0144](../Decisions/0144-Modular-WebAssembly-Wvb-Canonical-Metadata-And-References.md) uses that profile-12 boundary for an eight-function Windvale-written verifier. Its 440,093-byte import-free Wasm validates complete WVB 1.6 structure plus canonical names/order, capability catalog signatures, strict text UTF-8, nominal identities, instruction operand indices and data kinds, exact branch targets, export identity, record-to-enum fields, and enum uniqueness. Three representative modules, one-short exhaustion, and thirteen semantic mutations agree with the Stage 0 oracle and Node.js. This is the canonical metadata/reference phase; typed stacks and locals, call value flow, record-field receiver types, joins, reachability, maximum-stack agreement, and authorization remain. The static page remains on profile 8.

[Decision 0146](../Decisions/0146-Expanded-Descriptor-Bearing-WebAssembly-Call-Graph.md) adds local profile 13 over unchanged execution ABI 3 after the semantic verifier reached both profile-12 capacity limits. It retains every per-function rule and decreasing-ordinal call proof while expanding the bounded graph to sixteen functions, 131,072 aggregate code bytes, 400,000 decoded instructions, and 1 MiB of generated Wasm. A derived nine-function verifier crosses both former limits, lowers to 440,333 import-free bytes, agrees with the reference runtime and Node.js on all three representative inputs, and preserves the original profile-12 artifact byte for byte. The added capacity is reserved for executable type/control-flow proof; the static page remains on profile 8.

[Decision 0149](../Decisions/0149-Windvale-Native-WebAssembly-Wvb-Executable-Verifier.md) uses that capacity for a two-function executable phase composed after the retained semantic checks. The complete ten-function verifier proves typed access to deterministically default-valued locals, operand-stack flow, calls, capability signatures, records, enums, returns, source-compiler-aligned reachability, and exact declared stack depth. It lowers to 722,837 import-free Wasm bytes and agrees with the Stage 0 oracle on three accepted modules plus nine executable mutations under Node.js. General nonempty stack joins and capability authorization remain separate gates; the static page remains on profile 8.

[Decision 0152](../Decisions/0152-First-Wasm-Hosted-Wvb-Scalar-Interpreter.md) adds local profile-14 evidence and the first real verified-WVB execution path under Wasm. A separate 145,469-byte import-free interpreter consumes only candidates first accepted by the complete verifier, then executes bounded `i32`, `u32`, `u8`, and `bool` locals, checked arithmetic, comparisons, calls, branches, loops, and returns. `Function-Only.wv` agrees with the reference runtime at result `6` and 199 guest instructions; a wider scalar fixture agrees at `42` and 351. Independent outer and guest budgets, call depth, checked overflow, and repeat reset are exact under Node.js. The static page remains on profile 8.

[Decision 0157](../Decisions/0157-Wasm-Hosted-Wvb-Text-And-Bytes-Values.md) adds local profile-15 evidence. The expanded 253,707-byte interpreter uses uniform eight-byte cells, fixed frames, and a 64 KiB append-only heap to execute static data, descriptor calls, byte reads/slices/builders/concatenation, text concatenation/conversion, and strict UTF-8. Positive and boundary fixtures agree with the reference runtime, while range, invalid-UTF-8, narrowing, per-value, and aggregate-heap failures remain exact. The complete verifier is still the mandatory first stage, and all earlier generated Wasm artifacts remain byte-identical. The static page remains on profile 8.

[Decision 0158](../Decisions/0158-Wasm-Hosted-Wvb-Formatting-And-Quoting.md) expands the same profile-15 interpreter to 306,560 import-free Wasm bytes without changing the selector contract. Invariant signed/unsigned formatting covers extrema and zero, while deterministic quoting covers short escapes, printable ASCII, DEL, BMP values, and a supplementary surrogate pair exactly like the reference runtime. The compiler-produced data/text fixture now executes as result `13`; complete-verifier-approved SHA-256 remains an explicit outside-profile boundary. The static page remains on profile 8.

[Decision 0162](../Decisions/0162-Import-Free-WebAssembly-Sha256-Lowering.md) advances the selector locally to profile 16 and expands the interpreter to 334,209 import-free Wasm bytes. The backend lowers the existing SHA-256 WVB operation to explicit Wasm scratch-memory, rotate, shift, bitwise, padding, block, and lowercase-hex code without JavaScript or Web Crypto. Complete-verifier-first empty, padding-boundary, and multi-block fixtures agree with the reference runtime. Records and enums are now the remaining runtime-value gap before compiler execution; the static page remains on profile 8.

[Decision 0166](../Decisions/0166-Wasm-Hosted-Record-And-Enum-Values.md) expands the retained profile-16 interpreter to 404,340 import-free Wasm bytes without changing execution ABI 3 or the selector contract. Verified nominal declarations now drive eight-byte record and enum cells, deterministic typed defaults, a bounded 4 KiB record arena, field access, enum comparison, and enum-name publication. Compiler-produced nominal values agree with the reference runtime, and a dedicated fixture reaches exact guest `WVR3017` with deterministic reset. This closes the planned runtime-value families before measured compiler execution; the static page remains on profile 8.

[Decision 0170](../Decisions/0170-Compiler-Capacity-Wasm-Wvb-Verifier-Bundle.md) admits the exact 599,868-byte compiler WVB without changing profile 16, execution ABI 3, or any retained Wasm identity. Three fresh import-free instances derived from the canonical verifier sources separately prove metadata/references, typed execution, and control/reachability because their combined 5,768,687,747 instructions cannot fit the ABI's 32-bit meter. The compiler's 328 functions, 481,356 code bytes, 100,194 instructions, maximum 1,049 locals, stack depth 34, recursion, and six capability declarations are now measured. This closes compiler admission, not execution: the bounded interpreter still rejects the compiler during preflight, and the static page remains on profile 8.

[Decision 0174](../Decisions/0174-Portable-Compiler-Memory-Contract-And-Wasm-Bytes-Entry.md) removes hosted capabilities from the browser-facing compiler boundary. The exact 597,545-byte portable compiler accepts canonical WVSS through `Main(bytes) -> bytes` and returns a versioned `WVCO 1` WVB-or-diagnostic envelope. The same three Wasm verifier phases admit it. The expanded interpreter preserves scalar protocol version 1 and adds `WVXI 2` / `WVXO 2`; a complete-verifier-approved 209-byte guest returns `[1, 2, 3, 42]` from input `[1, 2, 3]` under Node.js. With a valid guest budget, the compiler reached the measured sixteen-function preflight rejection after 96,927 outer instructions, selecting function capacity as that stage's next boundary. The static page remains on profile 8.

[Decision 0175](../Decisions/0175-Compiler-Scale-Wasm-Interpreter-Execution-Entry.md) advances the retained interpreter to compiler-scale function, parameter, adaptive-frame, stack, instruction, budget, and call-depth bounds. The exact portable compiler now completes preflight and executes its first instruction: canonical `Function-Only.wv` WVSS returns a normal one-instruction `WVXO 2` budget failure in both the reference runtime and Node.js. A full-budget run then reaches enclosing Wasm `WVR3018` because immutable whole-frame replacement exhausts the 4 MiB monotonic value arena. The static page remains on profile 8.

[Decision 0177](../Decisions/0177-Exact-Per-Function-Wasm-Interpreter-Frames.md) replaces one candidate-wide local-frame width with exact per-function frame lengths and compact saved frames. The portable compiler now returns an exact `WVXO 2` budget result through 1,511 guest instructions; 1,512 is the first enclosing `WVR3018`. The existing direct selector is not yet a shorter compiler path because its static-data, nominal-value, recursion, and call-order limits exclude the measured artifact. Immutable `local.store` still reconstructs frames, so reusable/reclaiming storage remains the next execution boundary and the static page remains on profile 8.

[Decision 0189](../Decisions/0189-Bounded-Reclaiming-Wasm-Value-Storage.md) replaces single-function runtime-profile monotonic construction with bounded first-fit value storage, descriptor retain/release, split/coalescing, and full reset while retaining fixed memory and execution ABI 3. A 16 MiB cumulative workload succeeds through the fixed 4 MiB arena and repeats in one Node.js instance. Constant-time local-shape metadata reduces incremental compiler interpretation cost by about 83%; the portable compiler crosses 1,512 and reaches exact guest `WVR3017` at instruction 37,085 in the separate 4 KiB record arena. Guest record and heap ownership remain before complete `WVCO 1`; the static page remains on profile 8.

[Decision 0197](../Decisions/0197-Bounded-Reclaiming-Wasm-Guest-Records.md) retains stable record handles while reclaiming dead field-cell spans through fixed metadata and conservative tracing across locals, the operand stack, saved call frames, construction fields, and nested records. Cumulative churn reuses the 512-cell arena; a distinct full-live-set case proves exact `WVR3017` and reset without exceeding the verifier's stack-16 profile. The 468,320-byte import-free interpreter now carries the exact portable compiler to an ordinary 100,000-instruction guest-budget result. The separate 64 KiB guest text/bytes heap is next; the static page remains on profile 8.

The WVB 1.11 compiler later made the interpreter's root-first SHA call composition eligible for the descriptor-bearing call-module path. That exposed an older layout gap: guest functions occupied the allocator and descriptor-reference helper indices, private bodies retained the monotonic path, and the current compiler stopped before guest entry with enclosing `WVR3018`. The call module now reserves wrapper index zero, allocator/reference indices one and two, and guest indices three onward; it initializes reclamation once and uses reclaiming descriptor operations in private bodies. A standalone Node probe records current identities and proves ordinary budget exhaustion at one, 100,000, and 500,000 guest instructions while preserving the fixed 129-page ABI. The original three-function descriptor fixture succeeds, exhausts one instruction short, handles empty input, and resets in the same instance. A separate two-function pressure fixture performs 8,192 descriptor-returning calls over a 1,024-byte value, cumulatively constructs 8 MiB through the fixed 4 MiB arena, and proves success / one-short exhaustion / success in one instance. No complete compiler result or guest-heap reclamation is claimed.

The first separate guest-heap slice adds stable-offset allocation metadata and reference transitions for descriptor locals and descriptor-valued record fields. Fixed-width constructors reuse released spans rather than advancing the 64 KiB high-water boundary. A focused guest cumulatively constructs 65,604 bytes while retaining one four-byte value and succeeds at 205,032 guest instructions with result `4,099`; the same Wasm instance then executes the retained scalar guest at 351 instructions with result `42`. Descriptor-consuming stack operations and returns still conservatively retain storage, so full heap reclamation and complete compiler execution remain pending. The allocator remains inside the interpreter because the current root-first descriptor call profile admits only root `Main` plus its SHA leaf; a separately organized helper module requires a later call-graph expansion.

The completed guest-heap slice adds an exact 64-cell descriptor ownership mask, releases descriptor-consuming stack values and departing function locals, transfers call and return values without duplication, releases descriptor fields when conservative record collection proves their owner dead, and routes every descriptor producer through one release-before-allocation first-fit path. A two-function record-bearing fixture cumulatively constructs 143,364 bytes and 1,136 record field cells, succeeds at 15,627 guest instructions with result `69`, then shares one instance with the retained text/bytes, formatting/quoting, SHA-256, exact one-short, and reset cases. The 765,691-byte artifact remains import-free and fixed at 129 pages. Node's baseline WebAssembly tier passes; the default optimizing tier exceeds the available local process memory on the enlarged 5,740-local interpreter function, making call-graph expansion and cohesive function extraction the next boundary rather than a playground switch.

The root-first descriptor contract now admits only fully reachable graphs whose call ordinals strictly increase, preserving a maximum of sixteen functions while making cycles and recursion structurally impossible. Opcode stack-effect classification moves into a focused Windvale helper; the composed interpreter has three functions and reduces the root from 5,740 to 5,551 locals. The 111,316-byte WVB lowers in 261,291,275 instructions to 770,608 import-free Wasm bytes. Ordinary optimizing-tier Node.js now compiles and executes the complete pressure/text/formatting/SHA/exhaustion/reset probe in one instance, including the 143,364-byte cumulative guest-heap workload. This restores normal engine compatibility; it does not yet complete portable compiler execution or remove Stage 0 from artifact production because the pinned native build driver does not bind the new three-module source composition.

The next refinement uses the new graph for a focused request/WVB envelope reader and restores static opcode effects to the root as a balanced packed-word lookup. The root falls again to 5,364 locals without requiring a WVB data section, which the bounded WebAssembly backend currently rejects. Its 110,319-byte WVB lowers in 267,391,678 instructions to 791,182 import-free Wasm bytes. The retained ownership workload falls from 69,597,159 to 62,743,806 outer instructions while preserving exact results and guest meters. The exact 919,577-byte portable compiler reaches guest budget 100,000 at 192,935,833 outer instructions; this calibrates compiler execution beyond the prior 500,000-instruction evidence without claiming a complete `WVCO 1` result or spending an unmeasured multi-minute outer budget.

Canonical dependency inventory order now lets the ordinary pinned Windvale-native front door build and compiler-aligned-verify that same three-source interpreter project without .NET. The native compiler emits a 105,936-byte WVB whose root uses 981 locals, while Stage 0 still emits the byte-identical 110,319-byte prior artifact after the order-only manifest change. The native WVB lowers through the retained experimental backend to 782,416 import-free Wasm bytes and passes the same positive, budget, reset, ownership, and malformed-envelope Node.js probe. This removes Stage 0 from interpreter WVB production; lowering and publishing the resulting Wasm still use an experimental host route and are not yet the normal website pipeline.

Scalar local loads now bypass nominal-default work, and opcodes zero through 22 skip the disjoint higher-opcode dispatch region. The native WVB keeps the same size, code bytes, and 981-local root; its 782,416-byte Wasm lowers the exact portable compiler's 100,000-instruction calibration from 192,935,833 to 185,288,631 outer instructions while reducing every retained positive and budget case. A measured attempt to compose the backend into the existing native compiler build driver was reverted: its valid 1,400,728-byte WVB selected a 34,076,699-byte x64 fragment, 522,267 bytes above the qualified 32 MiB limit. The backend WVB itself already builds byte-identically through the no-.NET front door; a dedicated native WebAssembly tool profile or the emerging general native packager remains the honest Wasm-publication boundary.

The interpreter next expands its typed record arena to 768 cells and sustains the evolved 919,577-byte portable compiler through complete compilation. The pinned 100-byte source becomes a byte-identical 183-byte canonical WVB after 1,183,292 guest and 1,513,529,072 outer instructions, then verifies and returns `42` when resubmitted through the same import-free interpreter. Repository-owned package identities separate ordinary verified-copy website publication from the remaining Stage 0 recovery regeneration seams.

The first complete static-worker pipeline now loads that package by manifest, verifies every artifact digest, builds canonical `WVSS 1`, admits `WVXO 2` and `WVCO 1`, resubmits the untrusted result through `WVXI 1`, and transfers the copied WVB back to ordinary JavaScript. Local Chromium produces the exact expected WVB and result with zero .NET/Blazor requests in 378.1 seconds. This is bounded browser integration, not a general backend or replacement of the .NET playground path. It does not implement `break`, `continue`, browser capability authorization/imports for compiled applications, general WVB nonempty stack joins, compiler self-hosting in WebAssembly, cross-browser qualification, or complete worker containment for the editable pipeline. The exact experimental contract is [`Specifications/Windvale-WebAssembly.md`](../../Specifications/Windvale-WebAssembly.md).

A warmed successor extracts type preflight, record-metadata construction, and integer formatting into a five-function interpreter graph, then validates a 100,000-guest-instruction budget run before exact compilation on the same instance. Ordinary Node.js falls from 354.9 seconds for one cold call to about 89.9 seconds total while preserving the exact compiler result; that checkpoint's digest-pinned package contains the 112,216-byte native WVB and 839,104-byte import-free Wasm.

The bounded direct static-data slice then permits the interpreter to replace its balanced per-instruction opcode-effect branch tree with one immutable 256-byte lookup table. The current 110,700-byte WVB lowers through the normal .NET-free native publisher to 828,165 import-free Wasm bytes. Exact compilation preserves the 1,183,292 guest instructions, 183-byte WVB identity, verification, and result `42` while reducing outer compiler execution from 1,513,523,789 to 1,404,070,227 instructions. A complete warmup, compile, and execution run takes 59.4 seconds in Node.js on the measured Windows host. This is a useful interpreter improvement, but current-package Chromium and cross-browser measurements remain pending and the remaining 1.4-billion-operation compiler path is not yet interactive.

The direct backend next completely validates up to 1,024 unused nominal declarations, including bounded record fields, enum members, nested shape references, and exact inner payload consumption. A primitive fixture with unused record and enum declarations retains byte-identical WebAssembly. The exact portable compiler now clears its 104 static declarations and 82 nominal declarations before returning `Unsupportedˉcode` without output at its 417-function executable graph. This narrows the direct-compiler frontier without claiming nominal value lowering or changing the still-interpreted browser path.

## Proposed playground shape

The initial user experience could have four primary views:

1. **Source** — editable Windvale source and selectable examples.
2. **Output** — deterministic standard output, diagnostics, and the program result.
3. **Bytecode** — WVB sections, declarations, functions, data, and decoded instructions.
4. **Execution** — verifier status, requested and granted capabilities, instruction count, memory limits, and traps.

The surrounding page can use TypeScript or JavaScript, ordinary HTML, and an established browser editor. The generated Wasm subset and the exact source-to-WVB-to-result compiler proof now execute in disposable workers. The warmed exact proof is practical for continued browser development but is not yet an interactive normal editor engine; authorization, general diagnostics, inspection evidence, and complete editable-pipeline integration must follow without moving expensive or malicious work onto the UI thread. The Windvale program itself does not need to contain JavaScript.

An initial playground should be deployable as static assets after its toolchain artifacts are produced. A server-executed fallback is possible, but it has materially different cost, isolation, privacy, and abuse-control requirements and should not be confused with client-side execution.

## Demonstration catalog

The strongest gallery should explain Windvale's architecture rather than present only generic programming exercises.

### Initial language and Foundation demonstrations

- Hello World with explicit `console.write_line` authorization.
- Checked arithmetic, loops, functions, and bounded recursion.
- Immutable records and nominal enums.
- Strict UTF-8 validation, quoting, integer formatting, and decimal parsing.
- Immutable byte construction, slicing, fixed-width little-endian reads, and ordering.
- Deterministic success and failure diagnostics.

Existing examples that could seed this gallery include:

- [`Examples/Seed/Hello-Windvale.wv`](../../Examples/Seed/Hello-Windvale.wv)
- [`Examples/Foundation/Decimal-Parsing-Demo.wv`](../../Examples/Foundation/Decimal-Parsing-Demo.wv)
- [`Examples/Foundation/Byte-Construction-Demo.wv`](../../Examples/Foundation/Byte-Construction-Demo.wv)
- [`Examples/Foundation/Wvb-Header-Inspector.wv`](../../Examples/Foundation/Wvb-Header-Inspector.wv)
- [`Examples/Foundation/Wv-Dump-Core.wv`](../../Examples/Foundation/Wv-Dump-Core.wv)
- [`Object-Model/Windvale/Wvo-Object-Core.wv`](../../Object-Model/Windvale/Wvo-Object-Core.wv)

### Windvale-specific demonstrations

- **Source-to-WVB explorer:** edit source, compile it, verify it, and inspect the canonical module.
- **Capability gate:** run without a requested console or file capability, observe explicit refusal, grant it, and rerun.
- **Malformed-module lab:** change one WVB byte and show rejection before execution with an exact diagnostic and offset when available.
- **Upload and inspect:** select a local WVB or WVO file and run Windvale's own bounded inspection logic over its bytes.
- **Cross-host reproducibility:** compare the browser-produced module identity and result with retained Windows and Linux evidence from the same source and tool version.
- **Resource boundary:** demonstrate deterministic instruction, call-depth, input-size, output-size, or memory exhaustion rather than a frozen browser tab.
- **Compiler layers:** expose source, typed intermediate evidence, WVB, verifier result, and execution as distinct stages.

The complete self-hosted compiler workload is not automatically an interactive playground example merely because it can execute. Its current multi-billion-instruction reference workload requires measurement and likely a faster execution tier before it can meet an acceptable browser response time.

### Later visual and interactive demonstrations

After an explicit graphics, input, timer, storage, and event contract exists, possible samples include:

- Conway's Game of Life;
- a Mandelbrot or cellular-automata explorer;
- a sorting or graph-algorithm visualizer;
- a pixel-art editor;
- Snake, Pong, or a maze game;
- a binary-file visualizer;
- a calculator or unit converter;
- a bounded CSV or log viewer; and
- a small notes application backed by explicitly authorized browser storage.

These are candidate product demonstrations, not claims about the implemented Seed surface.

## UI and graphics direction under consideration

WebAssembly does not itself define buttons, windows, HTML, a document tree, or a graphical desktop. Browser UI must cross an explicit host boundary.

Three layers should remain distinct:

```text
Playground chrome and source editor --> ordinary browser HTML/TypeScript
Portable Windvale application UI    --> proposed Windvale UI/event contract
Custom graphics                     --> proposed pixel/vector surface contract
```

### DOM-backed controls

A browser adapter could map portable Windvale controls and events to HTML elements. This offers mature accessibility, text input, international input methods, responsive layout, selection, focus, and browser styling. The DOM must remain a browser adapter rather than become the semantic definition of portable Windvale UI.

### Canvas or WebGPU-backed surfaces

A Windvale application could submit a bounded pixel buffer or drawing-command buffer to a browser surface. This offers predictable custom graphics and a possible conceptual bridge toward a later Windvale OS compositor. It also makes accessibility, text shaping, focus, clipboard integration, and input behavior Windvale responsibilities.

### Tentative hybrid

The most practical early split appears to be:

- ordinary DOM controls for the playground, editor, documentation, diagnostics, and file picker;
- a small portable Windvale UI/event contract for ordinary applications;
- a canvas-style surface for graphics, games, charts, and OS-display experiments; and
- separate Windows, Linux, browser, and Windvale OS adapters behind the same accepted portable contracts when those adapters exist.

This is a product hypothesis, not an accepted UI architecture.

## Candidate browser capability mappings

| Windvale request | Possible browser adapter | Important boundary |
| --- | --- | --- |
| `console.write_line` | Append to the output view | Bound total lines and UTF-8 bytes. |
| Process arguments | Explicit playground fields | No ambient browser or machine arguments. |
| File read | User-selected files or a bounded virtual file set | No arbitrary native paths. |
| File write | Download, explicit save, or virtual storage | No silent host-file overwrite. |
| Diagnostics | Separate diagnostics view | Preserve the standard-output distinction. |
| Clock or timer | Browser timer service | Time is hosted and must not enter portable determinism implicitly. |
| Persistent storage | Browser storage adapter | Require explicit naming, quotas, and authorization. |
| Network request | Constrained browser request adapter | Browser permissions, origin policy, and reproducibility remain explicit. |
| UI events | Bounded event queue | Define ordering, cancellation, reentrancy, and overload behavior. |
| Graphics | Bounded pixel or command buffers | Validate sizes and commands before presentation. |
| Native interoperability | Unsupported | Browser Wasm cannot load arbitrary Windows or Linux libraries. |

The first playground should probably offer no network capability. Console, explicit arguments, and user-selected immutable files are enough to demonstrate the language, compiler, verifier, runtime, and tools without introducing remote state.

## Browser constraints to account for

- WebAssembly does not execute x86-64 machine code or boot UEFI images.
- Browser code has no ambient filesystem, process, native-library, raw-socket, device, or privileged-memory access.
- Browser services such as file selection and network operations are often asynchronous; Windvale will need a defined event or suspension boundary before portable applications can use them ergonomically.
- Long-running compilation and execution must not occupy the browser UI thread.
- Worker termination is a containment fallback, not a substitute for semantic instruction, depth, memory, and output budgets.
- Common WebAssembly deployments use a linear memory with target-specific address and size constraints; native x86-64 pointer layouts must not leak into the WebAssembly ABI.
- Multithreading introduces worker, shared-memory, deployment-header, determinism, and synchronization questions and should not be required for the first playground.
- Code size, runtime download size, cold startup, compilation latency, memory use, and mobile-browser behavior are product constraints even when semantic tests pass.
- Browser-origin, content-security, storage, cache, and update rules affect deployment but do not define Windvale language semantics.

## Security and resource boundary

The playground executes untrusted user input even when all computation remains client-side. A prototype should establish explicit limits for:

- source and supplied-module bytes;
- compile work;
- verified WVB bytes;
- executed instructions;
- call depth;
- runtime and text arenas;
- output and diagnostic bytes;
- uploaded file count and aggregate size;
- UI event queue depth; and
- wall-clock containment through a disposable worker.

Malformed source, WVB, WVO, UI commands, and capability arguments must fail before unsafe use. The browser sandbox is an additional containment boundary, not a replacement for Windvale verification.

## Candidate delivery sequence

### Exploration spike

1. Prove whether the current C# source compiler, WVB codec/verifier, and reference interpreter can build for browser-hosted .NET WebAssembly without the native project.
2. Compile and run bounded portable programs in a Web Worker. Lowering and interpreter execution are implemented locally through profile 16, and a three-stage verifier bundle admits the exact compiler WVB. Compiler execution and source-to-WVB worker composition remain open. Profile 10 remains the latest cross-host-qualified backend boundary, while the static .NET-free page intentionally remains on profile 8.
3. Compare its WVB bytes, result or output buffer, trap status, and defined instruction count with the reference path. Implemented for ABI 1 through ABI 3; cross-browser evidence remains open.
4. Measure compressed download size, cold start, compile time, execution time, peak browser memory, and worker termination behavior.
5. Record unsupported APIs and required adapter seams before choosing a product route.

### First usable playground

1. Add source editing, examples, deterministic output, and diagnostics.
2. Expose verifier, bytecode, capability, and resource evidence.
3. Support explicit arguments and immutable user-selected input files.
4. Add the source-to-WVB, capability-gate, malformed-module, and upload-and-inspect demonstrations.
5. Test current major browser engines without claiming parity where evidence is absent.

### Native browser execution

1. Publish an explicitly experimental Windvale-native worker route as soon as it honestly performs a useful bounded verifier, interpreter, compiler, or Module Inspector slice and displays every limitation.
2. Expand Decision 0174's capability-free source/WVB memory contract through compiler-scale function, frame, recursion, instruction-meter, and owned-value execution without weakening complete verification.
3. Retain the implemented disposable-worker path and its canonical WVB plus interpreter/WebAssembly differential evidence while broadening Chromium, Firefox, WebKit, and real-Safari coverage where claimed.
4. Move the complete editable source-to-WVB-to-verification-to-execution route behind the worker, then make it the default only after it uses no .NET runtime in normal publication and passes the replacement gate.
5. Add the bounded wait-set/event-stream experiment only after ordering, queue, cancellation, deadline, lifetime, and resource semantics are explicit.
6. Decide whether WebAssembly becomes a permanent host; decide direct WIR-to-WebAssembly target permanence later from a real application consumer.

## Qualification direction for permanent acceptance

A future permanent-host acceptance decision should require at least:

- one specified browser execution profile and versioned Windvale-to-WebAssembly ABI;
- identical portable source and canonical WVB inputs across Windows, Linux, and the browser;
- differential outputs, diagnostics, traps, and resource counters;
- malformed module and hostile capability tests;
- deterministic `.wasm` bytes when WebAssembly modules are published artifacts;
- explicit browser and version evidence;
- worker-containment and resource-exhaustion evidence;
- deployment asset identities and license review; and
- a statement of which browser engines, devices, and UI capabilities were not qualified.

A later direct-target acceptance additionally requires a real typed-WIR consumer, deterministic `.wasm` publication, semantic parity with canonical WVB execution, useful size/startup/execution evidence, and confirmation that it does not create a parallel language implementation. The retained .NET browser code may remain as reference and recovery evidence after it leaves the normal route.

Browser equality should be claimed only for behavior defined by Windvale. Layout, fonts, browser chrome, scheduling latency, and other host presentation details require separate contracts if they are expected to agree.

## Non-goals for an initial playground

- Booting or representing Windvale OS.
- Executing Windvale x86-64 output in the browser.
- Supporting system-profile instructions, raw memory, ports, interrupts, or devices.
- Providing unrestricted filesystem, networking, subprocess, or native-library access.
- Designing a complete cross-platform desktop toolkit before a bounded demonstration requires it.
- Treating JavaScript, the DOM, .NET, or WebAssembly as the definition of Windvale semantics.
- Claiming that all existing compiler or self-hosting workloads are immediately interactive in a browser.

## Open decisions

- Which bounded Windvale-native component and UI produces the first useful experimental route before complete compiler execution?
- Which measured interpreter or backend optimization makes complete portable compiler execution interactive enough for the normal worker route?
- Which execution and memory limits provide useful interaction on desktop and mobile browsers while retaining the current fixed-memory ABI where applicable?
- Which exact Chromium, Firefox, WebKit, and real-Safari versions form the first supported profile?
- Which diagnostics and intermediate compiler evidence are safe, stable, and useful enough to expose publicly?
- What are the exact signatures, batching, ordering, cancellation, deadline, and close semantics of the accepted wait-set/event-stream direction?
- Should ordinary portable UI map to semantic controls, drawing commands, pixels, or a layered combination?
- How are accessibility, text shaping, international input, focus, clipboard, and event ordering specified without adopting browser behavior as language semantics?
- Which canonical Module Inspector output schema and exported function become the cross-host proof?
- Which evidence moves WebAssembly first from exploration to a permanent host, and which later real application separately qualifies the direct compiler target?

## References

- [Project vision](Project-Vision.md)
- [Platform and portability model](../Architecture/Platform-And-Portability.md)
- [Native execution and .NET retirement](../Architecture/Native-Execution-And-Dotnet-Retirement.md)
- [Hosted resources](../../Specifications/Hosted-Resources.md)
- [WebAssembly core specification](https://www.w3.org/TR/wasm-core/)
- [WebAssembly web embedding](https://webassembly.org/docs/web/)
- [.NET WebAssembly hosting model](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-10.0)
