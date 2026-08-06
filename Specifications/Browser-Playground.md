# Windvale browser playground host contract

- Status: Experimental Stage 0 host contract with standalone direct-Wasm and browser-native compiler probes
- Implemented by: `Tools/Windvale.Playground.Engine` and `Tools/Windvale.Playground`
- Permanent WebAssembly target: Not accepted by this contract

## Purpose

The browser playground exposes the existing Windvale source-to-WVB-to-runtime path as a fully client-side demonstration. It lets a user edit Windvale source, compile it with the C# Stage 0 reference compiler, independently verify the resulting canonical WVB, execute it with the reference interpreter, and inspect evidence from every completed phase. For the bounded portable subset in [`Windvale-WebAssembly.md`](Windvale-WebAssembly.md), the UI also runs the qualified Windvale-authored backend over that WVB and executes the generated Wasm in a disposable worker as differential evidence.

This host contract contains the experiment without making .NET or WebAssembly the definition of Windvale semantics. The source language, canonical WVB, verifier, runtime behavior, profiles, and capability names remain owned by their existing specifications.

The separate `/playground/wasm-demo/` route exposes one pinned generated module without starting Blazor or .NET. It verifies the current 791-byte profile-8 artifact identity in ordinary JavaScript and passes the bytes plus an editable UTF-8 value through the same disposable-worker boundary. The artifact exercises execution ABI 3's fixed linear-memory input and output regions, strict guest-side text validation, and exact instruction budget. Its displayed `.wv` source is read-only provenance; editing the input value does not provide source compilation.

The separate `/playground/webassembly-compiler/` route is the first source-to-result probe without Blazor or .NET. A disposable module worker verifies the pinned package identities, executes the portable compiler WVB under the import-free ABI-3 interpreter Wasm, parses its `WVCO 1` result, and resubmits the returned WVB through the same interpreter for verification and scalar execution. Its current source surface is deliberately bounded to the exact compiler-success proof and its measured multi-minute Chromium latency is not an interactive-playground contract.

## Pipeline

One playground run follows this sequence:

```text
Windvale source
    -> Stage 0 reference compiler
    -> canonical WVB bytes
    -> WVB decoder and verifier
    -> browser-profile and capability checks
    -> bounded reference interpreter
    -> output, diagnostics, exit code, instruction count, digest, and disassembly

eligible portable WVB
    -> digest-pinned Windvale-authored backend under the reference interpreter
    -> deterministic import-free Wasm
    -> disposable Web Worker validation and execution
    -> result and instruction-count comparison with the reference interpreter

pinned qualified Wasm artifact
    -> ordinary JavaScript size and SHA-256 check
    -> editable text encoded as strict UTF-8
    -> the same disposable Web Worker validation and execution
    -> ABI 3, exact buffer layout, round-trip, success, and exhaustion evidence

bounded Windvale source
    -> SHA-256-verified portable compiler WVB and interpreter Wasm
    -> disposable worker `WVSS 1` / `WVXI 2` compilation
    -> strict `WVXO 2` / `WVCO 1` result admission
    -> returned WVB resubmission through `WVXI 1`
    -> verified scalar status, result, identities, and instruction evidence
```

Compilation failure produces no WVB identity. Verification and playground-policy failures may retain the compiled WVB, digest, module profile, declared capabilities, and inspection report when those values were established safely before the failure. Execution starts only after bytecode verification and playground-policy checks succeed. Direct Wasm lowering is attempted only after a portable, capability-free module completes through the reference interpreter. A valid module outside the bounded Wasm selector remains a normal reference-interpreter result rather than a playground failure.

## Accepted module profiles

The experimental host admits:

- `portable` modules; and
- `hosted` modules whose complete declared capability set is exposed by this host.

The host does not execute `system` modules. It reports `WVPG1003` before runtime construction. The playground therefore demonstrates the Windvale language and portable runtime; it is not a Windvale OS emulator.

## Browser capabilities

The initial allowlist is exact:

| Capability | Browser mapping |
| --- | --- |
| `console.write` | Append text to the bounded standard-output channel |
| `console.write_line` | Append text and one line feed to the bounded standard-output channel |
| `diagnostic.write_line` | Append text and one line feed to the bounded diagnostic channel |

Each capability remains denied unless the user authorizes it for the current run. A module declaration is not authorization. A requested authorization outside the allowlist reports `WVPG1002`; a verified module requiring a capability outside the allowlist reports `WVPG1004`.

The first host supplies no arguments, files, network, clocks, randomness, browser storage, DOM access, native execution, or ambient process state.

## Resource limits

The Stage 0 host enforces these ceilings:

| Resource | Ceiling |
| --- | ---: |
| Source text | 65,536 UTF-16 code units |
| User-selected instruction budget | 1 to 1,000,000 instructions |
| Default instruction budget | 250,000 instructions |
| Call depth | 128 frames |
| Standard output | 65,536 UTF-8 bytes |
| Diagnostic output | 65,536 UTF-8 bytes |
| Inspection report shown to the UI | 262,144 characters plus a truncation marker |
| Windvale backend execution | 100,000,000 reference instructions and 128 call frames |
| Generated WebAssembly | 524,288 bytes |
| Disposable worker wall clock | 2,000 milliseconds |

The normal runtime trap codes continue to describe instruction, call-depth, authorization, entry-point, type, and execution failures. A rejected output write is surfaced through the existing hosted-output runtime boundary.

The Stage 0 compiler, canonical verifier, reference interpreter, and Windvale backend interpreter still run on the browser UI thread. Only the generated import-free Wasm executes in a disposable worker. Instruction and call-depth budgets contain ordinary Windvale execution, but compilation, verification, lowering, and fallback execution must also move behind a worker boundary before treating the host as hardened against arbitrary hostile source or browser main-thread denial of service.

The separate browser-native compiler probe accepts at most 64 KiB of UTF-8 source, allows one to 20,000,000 result-execution instructions, gives the compiler 2,000,000 guest and 1,800,000,000 outer instructions, gives returned-WVB execution 200,000,000 outer instructions, caps call depth at 64, and terminates its worker after ten minutes. These values contain the exact experiment; they are not inherited by the normal Stage 0 playground.

## Request and result boundary

The reusable engine accepts:

- one source string;
- an ordinal set of authorized capability names; and
- an instruction budget.

It returns immutable evidence containing:

- pipeline status;
- compiler, verifier, playground-policy, or runtime diagnostics;
- bounded standard and diagnostic output;
- canonical WVB bytes, size, SHA-256 identity, and structural disassembly when available;
- module profile plus required and authorized capability sets; and
- exit code and executed instruction count after successful execution.

The engine contains no browser, DOM, file, network, or deployment API. The Blazor WebAssembly project is one UI adapter over that testable boundary.

The reusable engine also exposes the bounded WebAssembly lowerer as a separate immutable result. It embeds the portable backend and hosted shell as `.wv` source, compiles the composition once through Stage 0, requires the 319,111-byte backend WVB identity SHA-256 `e1b186989d158bf0f39830493ebf5e3ee54100d8340b31f0d2245623e38391a9`, and publishes no Wasm on selector failure. The browser adapter transfers successful bytes to a new module worker, which applies `WebAssembly.validate`, rejects all imports, checks the exact ABI-0, ABI-1, ABI-2, or ABI-3 exports, and is terminated after success, failure, or timeout. ABI 3 additionally validates its fixed memory extent and non-growth, disjoint 4 MiB regions, input length, output descriptor, and strict UTF-8 output before returning a bounded copied buffer; earlier ABIs return only scalar evidence. The lowerer reaches profile 17's bounded root-first interpreter capacity and retains the complete compiler-aligned verifier plus the separately reclaiming interpreter. The verifier covers structure, identities, typed executable flow, reachability, and exact stack contracts; the interpreter adds scalar calls/control, static data, descriptor calls, immutable text/bytes operations, strict UTF-8, invariant formatting, deterministic quoting, Windvale-authored SHA-256, records, enums, exact per-function frames, reclaiming generated-Wasm value storage, conservative bounded guest-record tracing, the canonical WVB 1.11 header and wide scalar shapes, `u32` divide/remainder/bitwise/shift execution, and explicit bounded failures. The static direct page deliberately remains on its smaller profile-8 text artifact; these deeper artifacts do not yet change that page or its worker lifetime.

The UI adapter hosts a locally bundled Monaco editor. Its Windvale tokenizer mirrors the implemented lexical categories in `Tools/Editors/Windvale/syntaxes/Windvale.tmLanguage.json`; it is presentation support rather than a source-language contract or compiler front end. Compiler diagnostics with source locations are projected into editor markers, while the compiler remains their authority. Editor text crosses into the reusable engine only when a run is requested.

## Static-hosting contract

The local Monaco ESM bundle is built with the pinned Node dependencies under `Tools/Windvale.Playground` before publishing the Blazor project. Publishing then produces static files under the publish `wwwroot` directory. The application uses a relative base path and includes `.nojekyll`, allowing the same output to be served from a domain root or a GitHub Pages project path without a server runtime. A static host must serve `.wasm` files using the `application/wasm` media type and use HTTPS outside local development.

The nested `wasm-demo/` and `webassembly-compiler/` directories are copied as ordinary static routes by the same publication. Their HTML entry points import only local application modules, shared JavaScript worker code, presentation assets, and the site analytics bootstrap. They contain no `_framework`, Blazor, or .NET startup reference. The direct demo's retained artifact is encoded as JavaScript deployment data, reconstructed byte-for-byte, and SHA-256 checked before worker transfer. The compiler probe loads manifest-owned package files copied during the npm build, verifies their size and SHA-256 identity before use, and transfers only the successful copied WVB result back to the page.

Repository deployment and public availability are operational evidence rather than semantic guarantees of this specification.

## Diagnostic codes owned by the playground

| Code | Meaning |
| --- | --- |
| `WVPG1001` | Source exceeds the playground source ceiling |
| `WVPG1002` | The request attempts to authorize a capability outside the browser allowlist |
| `WVPG1003` | A verified system-profile module cannot execute in the playground |
| `WVPG1004` | A verified module declares a capability the browser host does not expose |
| `WVPG1005` | The requested instruction budget is outside the accepted interval |

Compiler, bytecode, and runtime diagnostic codes retain their existing meanings and are not translated into playground-owned codes.

## Non-claims and reconsideration

This experiment does not establish:

- a general Windvale-to-WebAssembly compiler backend;
- integration of the implemented Windvale-native WVB interpreter into the normal editable page's source path;
- complete Windvale-native executable type/control-flow verification;
- general source-to-WVB compilation on a standalone route; the compiler probe is bounded to its exact current source profile, while the direct-artifact route edits only program input;
- WebAssembly as an accepted permanent Windvale target;
- browser UI APIs as portable Windvale UI semantics;
- native x86-64, PE, ELF, WVO, UEFI, or Windvale OS execution in the browser;
- production isolation for hostile source;
- worker containment for the normal Stage 0 compilation, WVB verification, Windvale-authored lowering, or reference-interpreter fallback; or
- .NET-free artifact construction, qualification, recovery, or repository automation.

The host contract should be reconsidered when the complete Stage 0 pipeline moves behind a worker boundary, a new browser capability is proposed, a Windvale-native interpreter can replace part of Stage 0, the direct subset expands, or cross-browser differential evidence is ready for qualification.
