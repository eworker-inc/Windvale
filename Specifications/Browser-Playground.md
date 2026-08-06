# Windvale browser playground host contract

- Status: Experimental browser-native normal host with retained Stage 0 recovery implementation
- Implemented by: static code under `Tools/Windvale.Playground/wwwroot`, with `Tools/Windvale.Playground.Engine` and the C# UI retained for recovery
- Permanent WebAssembly target: Not accepted by this contract

## Purpose

The normal browser playground is a fully client-side static application over a bounded Windvale-native WebAssembly path. It lets a user edit one Windvale source module, compile it with an import-free direct compiler Wasm, strictly admit the returned canonical WVB with a separate import-free interpreter Wasm, execute its scalar entry point, and inspect evidence from every completed phase. Package loading, compilation, verification, and execution occur in a disposable worker. Source does not leave the browser, and normal browser startup and website publication do not load or invoke .NET.

The C# reference compiler, reusable playground engine, and former Blazor UI source remain recovery and comparison evidence. They are not the normal page adapter. This contract does not make WebAssembly, JavaScript, Monaco, or browser behavior the definition of Windvale semantics. The source language, canonical WVB, verifier, runtime behavior, profiles, and capability names remain owned by their existing specifications.

The normal `/playground/` page is the only public playground presentation. Earlier focused proof pages are retired; their accepted decisions remain historical evidence.

## Pipeline

One normal playground run follows this sequence:

```text
bounded Windvale source
    -> canonical single-module WVSS 1
    -> SHA-256-verified direct compiler Wasm
    -> import rejection, ABI 4, output-kind 1, fixed-memory checks
    -> direct compilation under the deterministic compiler instruction ceiling
    -> strict WVCO 1 result admission
    -> returned WVB resubmission through WVXI 1
    -> SHA-256-verified scalar interpreter Wasm
    -> import rejection, ABI 3, fixed-memory checks
    -> verified scalar status, result, identities, bytes, and instruction evidence

recovery comparison only
    -> C# Stage 0 compiler, verifier, and reference interpreter
    -> optional Windvale-authored WVB-to-Wasm lowering
    -> differential result evidence outside the normal website path
```

Compilation failure produces no WVB identity. Execution starts only after the returned WVB crosses the interpreter's ordinary admission boundary. A nonzero execution status may retain established WVB bytes, digest, and counters. Unsupported language or capability surface fails explicitly; the normal page does not silently fall back to Stage 0 or a server compiler.

## Accepted module profiles

The browser compiler accepts one canonical `WVSS 1` source-set root. The normal browser execution host admits only the current single-module, capability-free `portable` scalar-execution profile. It does not execute `hosted` or `system` modules and does not fall back to the retained managed host. The recovery engine still implements its earlier portable/bounded-hosted policy and reports `WVPG1003` for a verified system module.

## Browser capabilities

The normal browser-native profile binds no host capabilities. It supplies no standard-output adapter, arguments, files, network, clocks, randomness, storage, DOM access, native execution, or ambient process state. Capability-bearing execution is outside the current native profile.

The retained Stage 0 recovery engine's allowlist remains exact:

| Capability | Browser mapping |
| --- | --- |
| `console.write` | Append text to the bounded standard-output channel |
| `console.write_line` | Append text and one line feed to the bounded standard-output channel |
| `diagnostic.write_line` | Append text and one line feed to the bounded diagnostic channel |

Each recovery capability remains denied unless explicitly authorized. A module declaration is not authorization. A requested authorization outside the recovery allowlist reports `WVPG1002`; a verified module requiring another capability reports `WVPG1004`.

## WebAssembly package contract

The browser package manifest owns exactly two fetched artifacts:

| Artifact | Contract |
| --- | --- |
| Direct compiler | ABI 4, output kind 1, no imports, 2,497 fixed pages, 4 MiB input region, 16 MiB output region |
| Scalar interpreter | ABI 3, output kind 1, no imports, 129 fixed pages, disjoint 4 MiB input and output regions |

The worker checks each artifact's exact manifest byte length and SHA-256 before `WebAssembly.compile`, rejects unexpected imports or exports, checks every ABI global and fixed memory extent, and rejects growable memory. It copies result bytes out of linear memory before returning them to the UI.

The direct compiler artifact is generated from the pinned portable compiler WVB by the fixed 34-segment generator contract in [Decision 0333](../Documents/Decisions/0333-Segmented-Direct-WebAssembly-Compiler.md). The generator is a maintainer artifact-production tool and is not downloaded or executed by playground users.

## Resource limits

The normal browser-native host enforces these ceilings:

| Resource | Ceiling |
| --- | ---: |
| Source text | 65,536 UTF-8 bytes |
| Direct compiler input region | 4,194,304 bytes |
| Direct compiler output region | 16,777,216 bytes |
| Direct compiler memory | 2,497 fixed WebAssembly pages |
| Compiler execution | 2,000,000 instructions |
| UI-selected result-execution budget | 10,000, 250,000, or 1,000,000 guest instructions |
| Host-accepted result-execution budget | 1 to 20,000,000 guest instructions |
| Returned-WVB outer execution | 200,000,000 instructions |
| Call depth | 64 frames |
| Worker wall clock | 300,000 milliseconds |

The disposable worker removes package loading and compilation from the browser UI thread and is terminated after success, failure, or timeout. The deterministic compiler and execution instruction ceilings remain the primary work bounds. Five minutes is a containment ceiling, not an interactivity promise.

## Request and result boundary

The normal JavaScript host accepts:

- one source string; and
- one returned-WVB execution budget.

It returns immutable evidence containing:

- pipeline status or bounded compiler/runtime failure text;
- canonical WVB bytes, size, SHA-256 identity, and hexadecimal inspection when compilation succeeds;
- direct compiler and interpreter execution instruction counters; and
- verified scalar status and result.

The worker core contains no DOM, file, network capability, or deployment API. The static JavaScript page is the normal UI adapter. The reusable C# engine remains a separate recovery and differential-test boundary.

The UI adapter hosts a locally bundled Monaco editor. Its Windvale tokenizer mirrors the implemented lexical categories in `Tools/Editors/Windvale/syntaxes/Windvale.tmLanguage.json`; it is presentation support rather than a source-language contract or compiler front end. Editor text crosses into the disposable worker only when a run is requested. The current native compiler diagnostic envelope does not carry source locations, so the normal page reports its exact bounded text without inventing editor markers.

## Static-hosting contract

The local Monaco ESM bundle is built with the pinned Node dependencies under `Tools/Windvale.Playground`. The same npm build verifies and copies only manifest-owned browser artifacts into the ignored `wwwroot/compiler-package/` publication directory. Normal publication then copies `Tools/Windvale.Playground/wwwroot` directly; it does not build or publish the C# project. The application uses a relative base path and includes `.nojekyll`, allowing it to live below `/playground/` without a server runtime. A static host must serve `.wasm` as `application/wasm`, allow same-origin module workers and WebAssembly compilation, and use HTTPS outside local development.

The root page imports only local application modules, worker code, presentation assets, the compiler package, and the site analytics bootstrap. It contains no `_framework`, Blazor, or .NET startup reference. Package manifests and artifacts revalidate; the worker verifies exact size and SHA-256 identity before execution. Repository deployment and public availability are operational evidence rather than semantic guarantees of this specification.

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

- complete source-language and hosted-capability breadth in the normal native page;
- general multi-module, hosted, or system source-to-WVB compilation in the browser;
- WebAssembly as an accepted permanent Windvale target;
- browser UI APIs as portable Windvale UI semantics;
- native x86-64, PE, ELF, WVO, UEFI, or Windvale OS execution in the browser;
- production isolation for arbitrary hostile source;
- retirement of the retained Stage 0 recovery implementation; or
- .NET-free recovery reconstruction of every pinned compiler/interpreter/WebAssembly artifact.

The host contract should be reconsidered when a browser capability is proposed, multi-module source is admitted, native diagnostics gain locations, cross-browser memory or latency evidence requires a smaller compiler profile, the segmented generator gains a non-Stage-0 recovery route, or cross-browser evidence is ready for qualification.
