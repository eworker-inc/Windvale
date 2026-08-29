# Windvale Seed implementation

> Status: Current component ownership after native retirement and managed Stage 0
> archival. Specifications remain normative for exact formats and behavior.

Windvale Seed is one coherent source-to-execution stack. Source, typed semantic
models, WVB, WVA, WVO, linked images, host containers, and Windvale OS resources
are distinct contracts; no backend format implicitly defines the language.

```text
Windvale source + Project manifest
        |
        v
Windvale compiler ----> canonical verified WVB
        |                       |
        |                       +--> native interpreter/runner
        |                       +--> WebAssembly interpreter
        |                       +--> Windvale OS admitted resource
        v
accepted-subset WVB-to-WVO lowering
        |
        v
verified WVO --> link --> flat image --> explicit PE/ELF/OS packaging
```

## Source compiler

`Compiler/Windvale/` owns the forward compiler:

- UTF-8 lexical analysis and source positions;
- declaration, statement, and expression parsing;
- contained multi-module source sets and import graphs;
- profile, capability, nominal-type, name, signature, body, local, and call
  validation;
- typed Windvale IR and stable diagnostics; and
- deterministic canonical WVB 1.11 emission.

Compiler phases communicate through explicit validated models. Source modules,
imports, declarations, nominal identities, capabilities, functions, data, and
exports are ordered canonically where their specifications require it. Compiler
output is re-admitted before publication.

`Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj` and
`Windvale-Compiler-Emission-Driver.wvproj` are the exact bootstrap-convergence
consumers. Their products form one compiler pipeline and converge independently
because analysis and emission have different ownership and memory bounds.
`Projects/Examples/Windvale-Compiler.wvproj` remains a complete integration
manifest, but it is not a second monolithic bootstrap compiler. The immutable
Seed, target-aware bootstrap products, and constructed compiler identities are
digest-bound repository artifacts; changes to those inventories are deliberate
bootstrap changes.

## Project and package inputs

Project manifests (`.wvproj`) own a contained root plus explicit source closure.
`Tools/Windvale.Project/Project-Manifest-Core.wv` and
`Project-Manifest-Tool.wv` are the active Windvale-owned parsing/application
source. Project input does not discover ambient files or grant capabilities.

Workspace 2, Package 1, and Lock 1 add deterministic dependency and resource
description without redefining Project 1. Package requirements and transitive
capability closure remain separate from runtime approval and provider binding.

## WVB verification and execution

WVB is the canonical verified cross-host distribution contract. Native verifier,
inspector, runner, build, and publisher applications are pinned under
`Artifacts/Native-Front-Door/` and invoked through paired launchers in
`Tools/Native/`.

Verification precedes inspection or execution and checks the complete admitted
module envelope: versions, counts, indices, canonical ordering, UTF-8, types,
capabilities, control-flow targets, stack states, declared limits, and trailing
data. Inputs remain untrusted even when produced by the repository compiler.

`Runtime/Windvale/` owns Windvale-written runtime contracts and hosted/native
composition source. Accepted execution profiles include bounded interpreter and
native paths, explicit instruction/call/value budgets, typed traps, and
rights-limited provider tables. Host adapters map only declared semantic
capabilities; ambient process, filesystem, console, or network authority is not
inherited.

## Native lowering and execution

`Compiler/Windvale/` also owns the accepted-subset x86-64 lowering modules,
including function layout, scalar/control/call lowering, static data, nominal
records/enums, descriptor ownership, runtime-service calls, relocations, and WVO
construction. Unsupported operations or ownership shapes fail explicitly.

Interpreter, AOT, and baseline-JIT modes share verified WVB semantics, native ABI
rules, machine lowering, runtime services, and typed relocation contracts. JIT
publication uses planned extents and W^X transitions; AOT publishes verified WVO
and linked/container products. Neither mode is a parallel source compiler.

Platform-specific memory allocation, executable publication, file access, and
process startup remain narrow host adapters. Portable Windvale code never sees
native paths, handles, or ambient authority.

## WVA assembler

`Assembler/Windvale/Wva-Assembler-Core.wv` owns textual WVA parsing, symbol and
section rules, supported x86-64 encoding, relocations, deterministic WVO output,
limits, and diagnostics. Paired `Assemble-Wva` launchers verify their exact native
application before use.

WVA is an explicit machine-language contract, not Windvale source. Assembly
acceptance does not bypass WVO admission.

## WVO object model

`Object-Model/Windvale/` owns WVO construction, structural verification, export
renaming, symbols, relocations, sections, checked offsets/sizes, and deterministic
serialization. WVO readers treat all objects as untrusted and reject malformed,
truncated, oversized, inconsistent, noncanonical, or hostile inputs before later
use.

`Verify-Wvo` and `Inspect-Wvo` are the ordinary paired native front doors. Exact
golden bytes are paired with structural assertions so failures remain
diagnosable.

## Linker and containers

`Linker/Windvale/` owns symbol resolution, aligned layout, checked relocation,
canonical maps, flat image construction, and explicit Windows/Linux/UEFI/hosted
container planning and verification. Native consumer artifacts retained below
`Linker/Reference/Consumers/` are products and fixed inputs; the former managed
reference source is not present.

Flat images, PE, ELF, UEFI applications, hosted WVB containers, and Windvale OS
resources are different output contracts. A packager consumes admitted link
evidence and publishes through a bounded transaction; it does not silently add
capabilities or change source semantics.

## Libraries and applications

`Foundation/` owns reusable low-level Windvale contracts needed by the compiler,
tools, runtime, and OS. `Libraries/` owns portable, hosted, capability-specific,
protocol, and system libraries. `Applications/` owns deployable consumers that
make those contracts useful.

The active package-backed consumer is WVDB Query. It is intended to prove one
deterministic admitted bundle, content-addressed publication, exact capability
approval/binding, rights-reduced hosted execution, and explicit denial without
starting a registry, SQL engine, or broad database server.

## Windvale OS

`Operating-System/` owns boot, kernel, drivers, processes, and native services.
The OS consumes the same verified portable WVB contract used by Windows and
Linux where its declared subset permits it. WVA owns bounded machine seams;
portable Windvale owns policy and verified data structures; the kernel retains
only mechanisms that require privilege.

Host image construction and OS execution are separate evidence. Pinned emulation
is an oracle for the named guest topology, not a definition of language/runtime
semantics and not proof of nested-virtualization support.

## Browser and WebAssembly

`Tools/Windvale.Playground/` is a static Monaco application. Its worker verifies
digest-pinned compiler/interpreter Wasm, constructs canonical source input,
re-admits returned WVB, and executes under explicit budgets and grants. The
normal build contains no Blazor host or managed engine.

WebAssembly is an explicit target/host profile. It does not define Windvale
semantics or imply that Windvale OS boots in a browser.

## Determinism and publication

Reproducible bytes are a product feature. Exact inputs, declared tool identities,
and options must produce exact outputs. Publications use private candidates,
independent admission, reread/identity checks where specified, and atomic
replacement. Development caches are content-addressed accelerators; complete
qualification is cold and cannot depend on cache state.

Generated products remain checked in only when they are named bootstrap,
distribution, browser, OS, or verification inputs with explicit provenance and
owners. Ordinary edits should not repin unrelated artifact families.

## Verification ownership

`Tools/Verify/Verify-Changed.ps1` classifies a coherent change and selects the
narrowest reliable native owners. `Tests/Native/Verification-Owners.txt` is the
fixed complete manifest used by explicit qualification, with paired Windows and
Linux owners and canonical sharding.

Development and qualification answer different questions:

- ordinary changes receive affected-owner feedback on both permanent hosts;
- a passing broader owner subsumes its narrower checks for the unchanged state;
- unmapped active boundaries fail closed; and
- complete dual-host qualification runs once for a deliberately selected
  release, promotion, bootstrap, security, ABI, or conformance state.

## Managed Stage 0 archive

C# source, managed projects, solution/SDK metadata, managed tests, and direct
managed commands are absent from `main` under Decision 0558. The immutable
`stage0-recovery-e5a1a7473c57` release preserves the exact qualified pre-removal
state, history, dependencies, licenses, artifacts, runbook, reports, and
checksums. `Bootstrap/Stage0/README.md` is the current recovery pointer.

Recovery begins in a separate workspace. Historical managed evidence may explain
or diagnose the frozen state but is not a maintained oracle for current language,
repository, package, runtime, or OS behavior.
