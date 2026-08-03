# Windvale browser playground host contract

- Status: Experimental Stage 0 host contract with a standalone direct-Wasm artifact lane
- Implemented by: `Tools/Windvale.Playground.Engine` and `Tools/Windvale.Playground`
- Permanent WebAssembly target: Not accepted by this contract

## Purpose

The browser playground exposes the existing Windvale source-to-WVB-to-runtime path as a fully client-side demonstration. It lets a user edit Windvale source, compile it with the C# Stage 0 reference compiler, independently verify the resulting canonical WVB, execute it with the reference interpreter, and inspect evidence from every completed phase. For the bounded portable subset in [`Windvale-WebAssembly.md`](Windvale-WebAssembly.md), the UI also runs the qualified Windvale-authored backend over that WVB and executes the generated Wasm in a disposable worker as differential evidence.

This host contract contains the experiment without making .NET or WebAssembly the definition of Windvale semantics. The source language, canonical WVB, verifier, runtime behavior, profiles, and capability names remain owned by their existing specifications.

The separate `/playground/wasm-demo/` route exposes one pinned generated module without starting Blazor or .NET. It verifies the current 1,185-byte profile-6 artifact identity in ordinary JavaScript and passes the bytes through the same disposable-worker boundary. Its displayed `.wv` source is read-only provenance; it does not provide source compilation.

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
    -> the same disposable Web Worker validation and execution
    -> ABI 1, status 0, result 42, and 30-instruction evidence
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
| Generated WebAssembly | 65,536 bytes |
| Disposable worker wall clock | 2,000 milliseconds |

The normal runtime trap codes continue to describe instruction, call-depth, authorization, entry-point, type, and execution failures. A rejected output write is surfaced through the existing hosted-output runtime boundary.

The Stage 0 compiler, canonical verifier, reference interpreter, and Windvale backend interpreter still run on the browser UI thread. Only the generated import-free Wasm executes in a disposable worker. Instruction and call-depth budgets contain ordinary Windvale execution, but compilation, verification, lowering, and fallback execution must also move behind a worker boundary before treating the host as hardened against arbitrary hostile source or browser main-thread denial of service.

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

The reusable engine also exposes the bounded WebAssembly lowerer as a separate immutable result. It embeds the portable backend and hosted shell as `.wv` source, compiles the composition once through Stage 0, requires backend WVB SHA-256 `318acb4cd0deb866c0e9596f1006102943f554db2bde99faec7c6702f05f282d`, and publishes no Wasm on selector failure. The browser adapter transfers successful bytes to a new module worker, which applies `WebAssembly.validate`, rejects all imports, checks the exact ABI-0, ABI-1, or ABI-2 exports, executes once, returns only bounded scalar evidence, and is terminated after success, failure, or timeout.

The UI adapter hosts a locally bundled Monaco editor. Its Windvale tokenizer mirrors the implemented lexical categories in `Tools/Editors/Windvale/syntaxes/Windvale.tmLanguage.json`; it is presentation support rather than a source-language contract or compiler front end. Compiler diagnostics with source locations are projected into editor markers, while the compiler remains their authority. Editor text crosses into the reusable engine only when a run is requested.

## Static-hosting contract

The local Monaco ESM bundle is built with the pinned Node dependencies under `Tools/Windvale.Playground` before publishing the Blazor project. Publishing then produces static files under the publish `wwwroot` directory. The application uses a relative base path and includes `.nojekyll`, allowing the same output to be served from a domain root or a GitHub Pages project path without a server runtime. A static host must serve `.wasm` files using the `application/wasm` media type and use HTTPS outside local development.

The nested `wasm-demo/` directory is copied as an ordinary static route by the same publication. Its HTML entry point imports only its local application module, the shared JavaScript worker host, presentation assets, and the site analytics bootstrap. It contains no `_framework`, Blazor, or .NET startup reference. The retained artifact is encoded as JavaScript deployment data, reconstructed byte-for-byte, and SHA-256 checked before worker transfer.

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
- a Windvale-native WVB interpreter compiled to WebAssembly;
- source editing or source-to-WVB compilation on the standalone direct-artifact route;
- WebAssembly as an accepted permanent Windvale target;
- browser UI APIs as portable Windvale UI semantics;
- native x86-64, PE, ELF, WVO, UEFI, or Windvale OS execution in the browser;
- production isolation for hostile source;
- worker containment for Stage 0 compilation, WVB verification, Windvale-authored lowering, or reference-interpreter fallback; or
- .NET-free artifact construction, qualification, recovery, or repository automation.

The host contract should be reconsidered when the complete Stage 0 pipeline moves behind a worker boundary, a new browser capability is proposed, a Windvale-native interpreter can replace part of Stage 0, the direct subset expands, or cross-browser differential evidence is ready for qualification.
