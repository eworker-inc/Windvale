# Decision 0182: Browser and WebAssembly product direction

- Date: 2026-08-03
- Status: Accepted product direction; WebAssembly remains an experimental host and target until the separate evidence gates pass
- Extends: [Decision 0177](0177-Exact-Per-Function-Wasm-Interpreter-Frames.md), which extends the compiler-memory boundary from [Decision 0174](0174-Portable-Compiler-Memory-Contract-And-Wasm-Bytes-Entry.md)
- Retains: canonical WVB, the .NET Stage 0 oracle, disposable-worker containment, explicit resource ceilings, and the distinction between a browser playground and Windvale OS emulation

## Context

The browser experiment already proves a .NET-hosted source-to-WVB path and a growing Windvale-authored import-free WebAssembly path. Decision 0174 adds a capability-free in-memory Windvale compiler adapter and byte-array guest entry. Decisions 0175 and 0177 then admit that exact compiler to the retained interpreter, enter guest execution, and advance it through 1,511 guest instructions; budget 1,512 is the pinned first enclosing allocation failure because immutable frame reconstruction consumes the fixed Wasm value arena. Bounded reusable or reclaiming interpreter storage is now the next measured execution boundary.

The project wants that earlier Windvale path without deleting useful C# reference code or prematurely declaring WebAssembly a permanent product target.

## Decision

### Introduce the Windvale-native route before retiring the .NET route

Publish an explicitly experimental Windvale-native browser route as soon as a bounded worker can perform an honest useful slice of the pipeline: accept immutable input, run the Windvale verifier or compiler component actually supported, execute or inspect accepted WVB, enforce current bounds, and publish exact identities and limitations.

The early route need not satisfy the final .NET replacement gate and need not become the default. It must not claim source compilation, complete WVB execution, capability support, or cross-browser portability beyond its measured profile.

Promote the Windvale-native route to the default only when a disposable worker can:

- compile editable source through the capability-free Windvale compiler adapter into exact canonical WVB;
- verify the produced WVB before execution;
- execute the selected bounded profile with explicit authorization, instruction, memory, output, call-depth, and wall-clock limits;
- contain malformed input, exhaustion, recursion, provider failure, and worker termination;
- agree with Windows and Linux reference evidence across the selected engines; and
- load or invoke no .NET runtime in the normal route.

Keep the C#/.NET browser implementation in source as bootstrap, differential, recovery, and historical evidence after it leaves the default route. Removing it entirely requires a separate cleanup decision.

### Use typed WIR for direct source compilation

A direct source-to-WebAssembly backend consumes typed WIR because WIR retains source types, structured control, temporaries, and ownership before stack serialization. Canonical WVB remains the distributed artifact and the input to a separate verified WVB interpreter or install/runtime compilation path.

Do not invent a third permanent shared lowering IR before native and WebAssembly backends demonstrate a common need. Both routes must remain differential implementations of Windvale semantics rather than independent language definitions.

### Define one bounded browser profile

The first supported evidence profile covers Chromium, Firefox, and WebKit automation. A Safari support claim additionally requires evidence from real Safari rather than assuming that automated WebKit is identical.

Retain one disposable worker, fixed non-growing memory, no threads or shared memory, no ambient network or storage, explicit immutable inputs, the current fixed 4 MiB input/output ABI windows where applicable, bounded standard and diagnostic output, exact instruction and call-depth budgets, and hard worker termination. Record module identities, selected engine/version, results or traps, output, semantic counters, cold start, execution time, peak memory, and termination behavior.

Windvale pins the WebAssembly subset it emits and the JavaScript/Web embedding behavior it requires. It does not use “latest browser” or “latest WebAssembly” as an unversioned contract.

### Separate permanent-host and compiler-target acceptance

WebAssembly may become an accepted permanent Windvale host after the Windvale-native source/WVB worker, cross-engine differential, hostile-input, resource, reproducibility, deployment, and maintenance gates pass.

Direct WebAssembly compilation becomes a permanent compiler target only through a later decision with a real application consumer, deterministic publication, semantic parity, useful size/startup/execution evidence, and no parallel semantic implementation. Host acceptance does not automatically accept the compiler target.

### Use a portable wait/event contract instead of callbacks

The smallest asynchronous application boundary is a bounded wait set or event stream containing immutable typed events. It defines source identity, interest, queue limits, ordering, coalescing, deadline, cancellation, closed-source behavior, and bounded event batches. Providers do not invoke application callbacks reentrantly.

Browser adapters map DOM and worker events into the stream; Windows, Linux, and Windvale OS map their wait, message, or IPC mechanisms into the same semantic boundary. Rendering, widgets, windows, input methods, accessibility, clipboard, and drawing remain separate capability families.

### Use one exact cross-host sample

The first permanent sample is a Windvale Module Inspector. It accepts explicit WVB bytes, verifies the canonical identity and section structure, executes one bounded exported function when admitted, and emits canonical text or JSON evidence. Windows, Linux, and the browser consume the exact same WVB and input bytes. A later Windvale OS instance can join without changing the application contract.

## Consequences

Users can experience a real Windvale-authored browser path earlier, while its limitations remain visible. The mature path still has a strict replacement gate, and retained C# code preserves an independent oracle and recovery route.

Typed WIR supports efficient direct compilation without displacing WVB as the distribution identity. Host acceptance can advance independently from direct-target acceptance.

The event contract establishes a portable asynchronous seam without adopting browser callbacks or DOM behavior as Windvale semantics. A complete cross-platform UI toolkit remains later work.

No route is made default, browser profile qualified, .NET component retired, permanent WebAssembly host or target accepted, event capability implemented, or sample completed by this decision.

## Reconsider when

- the bounded Windvale-native route cannot provide useful interaction before full compiler execution;
- browser engines impose incompatible limits on the selected verifier, compiler, interpreter, or direct backend;
- WVB-to-WebAssembly compilation provides a simpler single path than the separate WIR and WVB consumers without losing identity or verification;
- callback-free event delivery cannot represent an essential host interaction; or
- the Module Inspector does not exercise enough shared language, verifier, and runtime behavior to serve as a convincing portability sample.
