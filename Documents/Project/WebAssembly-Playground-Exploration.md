# WebAssembly and browser playground exploration

> Status: Accepted staged direction; broader browser features remain proposed
> Authority: Informative; Decision 0182 and specifications own contracts
> Last reviewed: 2026-09-26

## Purpose

[Decision 0182](../Decisions/0182-Browser-And-WebAssembly-Product-Direction.md)
sets the staged browser direction. The normal playground now uses a bounded
Windvale-native compiler and interpreter in a disposable browser worker.
[Browser-Playground](../../Specifications/Browser-Playground.md) owns its exact
profile; the [playground guide](../../Tools/Windvale.Playground/README.md) owns
current use and publication commands.

Windows and Linux remain permanent hosts, with Windvale OS as the vertical
integration target. The experimental browser route does not itself accept
WebAssembly as a permanent host or direct compiler target. Broader Language 1.0
work remains a [subset proposal](Windvale-Language-1.0-WebAssembly-Subset-Plan.md).

The completed Blazor/Stage 0 transition and incremental backend measurements
are retained in [Git history](../Git-History.md#superseded-browser-and-verification-notes). Managed browser source is available
only through the [Stage 0 recovery release](../../Bootstrap/Stage0/README.md).

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

### Current browser execution

The normal static playground compiles one supported source module with a
Windvale-native compiler Wasm, admits the returned WVB and executes the scalar
entry through a separate interpreter Wasm. Package loading, compilation,
verification and execution are worker-contained. Normal startup and publication
use no .NET runtime. Exact capability, memory, input and instruction limits
remain in the [browser contract](../../Specifications/Browser-Playground.md).
This bounded profile does not establish complete Language 1.0 support.

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

## Proposed playground shape

The initial user experience could have four primary views:

1. **Source** — editable Windvale source and selectable examples.
2. **Output** — deterministic standard output, diagnostics, and the program result.
3. **Bytecode** — WVB sections, declarations, functions, data, and decoded instructions.
4. **Execution** — verifier status, requested and granted capabilities, instruction count, memory limits, and traps.

The page and editor remain host UI code. The Windvale compiler and runtime
execute behind the bounded worker interface defined by the browser contract.
Broader inspection and diagnostic views must preserve that separation.

An initial playground should be deployable as static assets after its toolchain artifacts are produced. A server-executed fallback is possible, but it has materially different cost, isolation, privacy, and abuse-control requirements and should not be confused with client-side execution.

## Demonstration catalog

The gallery ideas below extend the current examples. Check the
[playground guide](../../Tools/Windvale.Playground/README.md) for the supported
profile and current examples; this list does not expand accepted capabilities.

### Initial language and Foundation demonstrations

- Hello World with explicit `console.write_line` authorization (already available in the bounded hosted profile).
- Checked arithmetic, loops, functions, and bounded recursion.
- Immutable records and nominal enums.
- Strict UTF-8 validation, quoting, integer formatting, and decimal parsing.
- Immutable byte construction, slicing, fixed-width little-endian reads, and ordering.
- Deterministic success and failure diagnostics.

Existing examples that could seed this future gallery include:

- [`Examples/Seed/Hello-Windvale.wv`](../../Examples/Seed/Hello-Windvale.wv) — the bounded hosted greeting example
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

The bounded editable worker route is already delivered. Broader work should
start from the [Language 1.0 subset plan](Windvale-Language-1.0-WebAssembly-Subset-Plan.md)
and [shell integration plan](Windvale-WebAssembly-Shell-Integration-Plan.md),
which remain proposals with their own limits and gates.

Before widening the profile, retain semantic differential checks, deterministic
artifacts, bounded resource use and disposable-worker containment. Measure
download size, startup, compilation, execution, browser memory and termination
on each claimed engine. File input, module inspection, richer UI and event
streams require their own accepted contracts and consumer evidence.

A wait-set/event-stream implementation must first define ordering, batching,
queue bounds, cancellation, deadlines and lifetime. Permanent-host acceptance
and later direct typed-WIR target acceptance remain separate decisions.

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

A later direct-target acceptance additionally requires a real typed-WIR consumer, deterministic `.wasm` publication, semantic parity with canonical WVB execution, useful size/startup/execution evidence, and confirmation that it does not create a parallel language implementation. The retired .NET browser implementation remains recoverable from the immutable Stage 0 release.

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
