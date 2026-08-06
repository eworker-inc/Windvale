# Windvale browser playground host contract

- Status: Experimental browser-native normal host with retained Stage 0 recovery implementation
- Implemented by: static code under `Tools/Windvale.Playground/wwwroot`, with `Tools/Windvale.Playground.Engine` and the C# UI retained for recovery
- Permanent WebAssembly target: Not accepted by this contract

## Purpose

The normal browser playground is a fully client-side static application over the bounded Windvale-native WebAssembly path. It lets a user edit one Windvale source module, compile it with the pinned Windvale-authored portable compiler WVB under the import-free interpreter Wasm, strictly admit the returned canonical WVB, execute its scalar entry point, and inspect evidence from every completed phase. Package loading, compilation, verification, and execution occur in a disposable worker. Normal browser startup and website publication do not load or invoke .NET.

The C# reference compiler, reusable playground engine, Blazor UI source, and differential Windvale-to-Wasm adapter remain recovery and comparison evidence. They are not the normal page adapter. This contract does not make WebAssembly, JavaScript, Monaco, or browser behavior the definition of Windvale semantics.

This host contract contains the experiment without making .NET or WebAssembly the definition of Windvale semantics. The source language, canonical WVB, verifier, runtime behavior, profiles, and capability names remain owned by their existing specifications.

The separate `/playground/wasm-demo/` route exposes one pinned generated module without starting Blazor or .NET. It verifies the current 791-byte profile-8 artifact identity in ordinary JavaScript and passes the bytes plus an editable UTF-8 value through the same disposable-worker boundary. The artifact exercises execution ABI 3's fixed linear-memory input and output regions, strict guest-side text validation, and exact instruction budget. Its displayed `.wv` source is read-only provenance; editing the input value does not provide source compilation.

The `/playground/webassembly-compiler/` route retains a focused presentation of the same source-to-result worker used by the normal page. A digest-pinned 292-byte scalar guest warms the same WebAssembly instance for exactly 20,000 instructions before compilation. The current local Chromium proof completes in 64.3 seconds without changing the exact WVB or result, down from 90.0 seconds with the preceding compiler self-warmup. Its latency and compiler surface remain experimental rather than portable interactive-playground guarantees.

## Pipeline

One normal playground run follows this sequence:

```text
bounded Windvale source
    -> canonical single-module WVSS 1
    -> SHA-256-verified compiler WVB, interpreter Wasm, and scalar warmup WVB
    -> exact 20,000-guest-instruction warmup on one disposable worker instance
    -> WVXI 2 compilation and strict WVXO 2 / WVCO 1 result admission
    -> returned WVB resubmission through WVXI 1
    -> verified scalar status, result, identities, bytes, and instruction evidence

pinned qualified Wasm artifact
    -> ordinary JavaScript size and SHA-256 check
    -> editable text encoded as strict UTF-8
    -> the same disposable Web Worker validation and execution
    -> ABI 3, exact buffer layout, round-trip, success, and exhaustion evidence

recovery comparison only
    -> C# Stage 0 compiler, verifier, and reference interpreter
    -> optional Windvale-authored WVB-to-Wasm lowering
    -> differential result evidence outside the normal website path
```

Compilation failure produces no WVB identity. Execution starts only after the returned WVB crosses the interpreter's ordinary admission boundary. A nonzero execution status may retain established WVB bytes, digest, and counters. Unsupported language or capability surface fails explicitly; the normal page does not silently fall back to Stage 0.

## Accepted module profiles

The normal browser-native host admits only the current single-module, capability-free `portable` compiler and scalar-execution profile. It does not execute `hosted` or `system` modules and does not fall back to the retained managed host. The recovery engine still implements the earlier portable/bounded-hosted policy and reports `WVPG1003` for a verified system module.

## Browser capabilities

The normal browser-native profile binds no host capabilities. It supplies no standard-output adapter, arguments, files, network, clocks, randomness, storage, DOM access, native execution, or ambient process state. Capability-bearing source is outside the current native profile.

The retained Stage 0 recovery engine's allowlist remains exact:

| Capability | Browser mapping |
| --- | --- |
| `console.write` | Append text to the bounded standard-output channel |
| `console.write_line` | Append text and one line feed to the bounded standard-output channel |
| `diagnostic.write_line` | Append text and one line feed to the bounded diagnostic channel |

Each recovery capability remains denied unless explicitly authorized. A module declaration is not authorization. A requested authorization outside the recovery allowlist reports `WVPG1002`; a verified module requiring another capability reports `WVPG1004`.

## Resource limits

The normal browser-native host enforces these ceilings:

| Resource | Ceiling |
| --- | ---: |
| Source text | 65,536 UTF-8 bytes |
| UI-selected result-execution budget | 10,000, 250,000, or 1,000,000 guest instructions |
| Host-accepted result-execution budget | 1 to 20,000,000 guest instructions |
| Interpreter warmup | exactly 20,000 scalar guest instructions, zero result, and `WVR3011` |
| Compiler execution | 2,000,000 guest and 1,800,000,000 outer instructions |
| Returned-WVB execution | 200,000,000 outer instructions |
| Call depth | 64 frames |
| Worker wall clock | 600,000 milliseconds |

The disposable worker removes expensive compilation from the browser UI thread and is terminated after success, failure, or timeout. The compiler and execution instruction ceilings remain the primary deterministic work bounds. Ten minutes is a containment ceiling, not an interactivity promise.

## Request and result boundary

The normal JavaScript host accepts:

- one source string; and
- one returned-WVB execution budget.

It returns immutable evidence containing:

- pipeline status or bounded compiler/runtime failure text;
- canonical WVB bytes, size, SHA-256 identity, and hexadecimal inspection when compilation succeeds;
- warmup, compiler, and execution instruction counters; and
- verified scalar status and result.

The worker core contains no DOM, file, network capability, or deployment API. The static JavaScript page is the normal UI adapter. The reusable C# engine remains a separate recovery and differential-test boundary.

The retained C# recovery engine also exposes the bounded WebAssembly lowerer as a separate immutable result. It embeds the portable backend and hosted shell as `.wv` source, compiles the composition once through Stage 0, requires the 319,111-byte backend WVB identity SHA-256 `e1b186989d158bf0f39830493ebf5e3ee54100d8340b31f0d2245623e38391a9`, and publishes no Wasm on selector failure. Its recovery browser adapter transfers successful bytes to a new module worker, which applies `WebAssembly.validate`, rejects all imports, checks the exact ABI-0, ABI-1, ABI-2, or ABI-3 exports, and is terminated after success, failure, or timeout. ABI 3 additionally validates its fixed memory extent and non-growth, disjoint 4 MiB regions, input length, output descriptor, and strict UTF-8 output before returning a bounded copied buffer; earlier ABIs return only scalar evidence. The lowerer reaches profile 17's bounded root-first interpreter capacity and retains the complete compiler-aligned verifier plus the separately reclaiming interpreter. The verifier covers structure, identities, typed executable flow, reachability, and exact stack contracts; the interpreter adds scalar calls/control, static data, descriptor calls, immutable text/bytes operations, strict UTF-8, invariant formatting, deterministic quoting, Windvale-authored SHA-256, records, enums, exact per-function frames, reclaiming generated-Wasm value storage, conservative bounded guest-record tracing, the canonical WVB 1.11 header and wide scalar shapes, `u32` divide/remainder/bitwise/shift execution, and explicit bounded failures. The static direct page deliberately remains on its smaller profile-8 text artifact; these deeper artifacts do not yet change that page or its worker lifetime.

The UI adapter hosts a locally bundled Monaco editor. Its Windvale tokenizer mirrors the implemented lexical categories in `Tools/Editors/Windvale/syntaxes/Windvale.tmLanguage.json`; it is presentation support rather than a source-language contract or compiler front end. Editor text crosses into the disposable worker only when a run is requested. The current native compiler diagnostic envelope does not carry source locations, so the normal page reports its exact bounded text without inventing editor markers.

## Static-hosting contract

The local Monaco ESM bundle is built with the pinned Node dependencies under `Tools/Windvale.Playground`. The same npm build verifies and copies only manifest-owned WebAssembly package artifacts into the ignored publication directory. Normal publication then copies `Tools/Windvale.Playground/wwwroot` directly; it does not build or publish the C# project. The application uses a relative base path and includes `.nojekyll`, allowing it to live below `/playground/` without a server runtime. A static host must serve `.wasm` as `application/wasm`, allow same-origin module workers and WebAssembly compilation, and use HTTPS outside local development.

The root page plus the nested `wasm-demo/` and `webassembly-compiler/` routes import only local application modules, shared worker code, presentation assets, and the site analytics bootstrap. They contain no `_framework`, Blazor, or .NET startup reference. Package manifests and artifacts revalidate; the worker verifies exact size and SHA-256 identity before execution and transfers only a successful copied WVB result back to the page.

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
- complete source-language and hosted-capability breadth in the normal native page;
- complete Windvale-native executable type/control-flow verification;
- general multi-module, hosted, or system source-to-WVB compilation in the browser;
- WebAssembly as an accepted permanent Windvale target;
- browser UI APIs as portable Windvale UI semantics;
- native x86-64, PE, ELF, WVO, UEFI, or Windvale OS execution in the browser;
- production isolation for arbitrary hostile source;
- retirement of the retained Stage 0 recovery implementation; or
- .NET-free regeneration and qualification of every pinned compiler/interpreter/WebAssembly artifact.

The host contract should be reconsidered when a browser capability is proposed, multi-module source is admitted, native diagnostics gain locations, the compiler becomes materially interactive, pinned artifacts regenerate through the native front door, or cross-browser evidence is ready for qualification.
