# WebAssembly and browser playground exploration

- Date: 2026-08-01
- Status: Exploration with an implemented Stage 0 playground and bounded Windvale-authored metered-control-flow/direct-call backend; not an accepted permanent WebAssembly target

## Purpose

This document records the current product and architecture discussion around a browser-based Windvale playground and a possible WebAssembly target. A bounded Stage 0 playground now implements the first route below; the larger target direction remains open and is not a permanent platform commitment. The implemented host boundary is specified separately in [`Specifications/Browser-Playground.md`](../../Specifications/Browser-Playground.md).

The direction under consideration is that portable Windvale source could eventually execute across:

- Windows;
- Linux;
- WebAssembly hosts, initially web browsers; and
- Windvale OS.

Windows and Linux remain accepted permanent hosts, and Windvale OS remains the vertical integration target. WebAssembly is not yet an accepted compiler target or runtime host. Accepting it would require a later decision grounded in a bounded prototype and differential evidence.

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
        +-- typed WIR and/or canonical verified WVB
                    |
                    +-- x86-64 backend --> Windows/Linux/Windvale OS adapters
                    |
                    `-- WebAssembly backend or interpreter --> browser adapter
```

Portable language behavior must not change with the selected target. Checked arithmetic, text and byte behavior, traps, capability authorization, resource limits, and diagnostics remain Windvale contracts. Instruction selection, executable representation, machine ABI, and host adaptation are target responsibilities.

Canonical WVB should remain the portable distribution identity unless a later accepted decision changes that direction. A WebAssembly backend would be another execution target for verified semantics, not a replacement definition of the language.

## Candidate implementation routes

The routes below are alternatives or stages. The first route is selected only for the current experiment; this exploratory document does not accept a permanent product architecture.

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

This is bounded browser integration, not a general backend or replacement of the .NET playground path. It does not yet implement recursion, `break`, `continue`, reclaiming allocation, browser capability authorization/imports, general WVB nonempty stack joins, a Wasm-hosted WVB interpreter, compiler self-hosting in WebAssembly, cross-browser qualification, or complete worker containment for the editable pipeline. The exact experimental contract is [`Specifications/Windvale-WebAssembly.md`](../../Specifications/Windvale-WebAssembly.md).

## Proposed playground shape

The initial user experience could have four primary views:

1. **Source** — editable Windvale source and selectable examples.
2. **Output** — deterministic standard output, diagnostics, and the program result.
3. **Bytecode** — WVB sections, declarations, functions, data, and decoded instructions.
4. **Execution** — verifier status, requested and granted capabilities, instruction count, memory limits, and traps.

The surrounding page can use TypeScript or JavaScript, ordinary HTML, and an established browser editor. The generated Wasm subset now executes in a disposable worker, and the complete compiler-aligned WVB verifier now executes as import-free Wasm under Node.js. Source compilation, verified-WVB interpretation, authorization, and their worker integration should follow so expensive or malicious input cannot freeze the page's user-interface thread. The Windvale program itself does not need to contain JavaScript.

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
- [`Examples/Foundation/Wvo-Object-Core.wv`](../../Examples/Foundation/Wvo-Object-Core.wv)

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
2. Compile and run bounded portable programs in a Web Worker. Lowering and compiler-aligned WVB verification are implemented locally through profile 13; WVB interpreter execution and source compilation remain open. Profile 10 remains the latest cross-host-qualified backend boundary, while the static .NET-free page intentionally remains on profile 8.
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

1. Retain the established Windows/Linux byte equality for the bounded one-function straight-line `i32` lowerer, execution ABI 1, and the Stage 0 differential oracle.
2. Retain the implemented disposable-worker path and its canonical WVB plus interpreter/WebAssembly differential evidence while broadening browser coverage.
3. Compose the implemented sequential structured control and bounded call graph, then add a UI/event experiment only after each capability, lifetime, resource, and asynchronous execution contract is explicit.
4. Decide whether WebAssembly becomes a permanent Windvale host and AOT target.

## Qualification direction if WebAssembly is accepted

A future acceptance decision should require at least:

- one specified browser execution profile and versioned Windvale-to-WebAssembly ABI;
- identical portable source and canonical WVB inputs across Windows, Linux, and the browser;
- differential outputs, diagnostics, traps, and resource counters;
- malformed module and hostile capability tests;
- deterministic `.wasm` bytes when WebAssembly modules are published artifacts;
- explicit browser and version evidence;
- worker-containment and resource-exhaustion evidence;
- deployment asset identities and license review; and
- a statement of which browser engines, devices, and UI capabilities were not qualified.

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

- Is the playground's first value education, public demonstration, development inspection, or all three in a deliberately ordered interface?
- What complete-pipeline worker containment and cross-browser evidence threshold should qualify the playground beyond the current local generated-Wasm path?
- Does direct WebAssembly compilation consume typed WIR, canonical verified WVB, or a shared machine-independent lowering model?
- Which execution and memory limits provide useful interaction on desktop and mobile browsers?
- Which browser engines form the first supported profile?
- Which diagnostics and intermediate compiler evidence are safe, stable, and useful enough to expose publicly?
- What is the smallest asynchronous event contract that works across browser, Windows, Linux, and Windvale OS adapters?
- Should ordinary portable UI map to semantic controls, drawing commands, pixels, or a layered combination?
- How are accessibility, text shaping, international input, focus, clipboard, and event ordering specified without adopting browser behavior as language semantics?
- Which exact sample should become the cross-host Windows/Linux/WebAssembly portability proof?
- At what evidence threshold does WebAssembly move from exploration to an accepted permanent target?

## References

- [Project vision](Project-Vision.md)
- [Platform and portability model](../Architecture/Platform-And-Portability.md)
- [Native execution and .NET retirement](../Architecture/Native-Execution-And-Dotnet-Retirement.md)
- [Hosted resources](../../Specifications/Hosted-Resources.md)
- [WebAssembly core specification](https://www.w3.org/TR/wasm-core/)
- [WebAssembly web embedding](https://webassembly.org/docs/web/)
- [.NET WebAssembly hosting model](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-10.0)
