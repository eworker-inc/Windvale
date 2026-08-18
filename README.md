# Windvale

![Four-panel Day 21 comic showing Windvale Language 1.0's multilingual source-profile design converging on one exact program before compiler implementation](Documents/Project/Images/Windvale-Project-Progress-2026-08-18.png)

*Day 21: Windvale Language 1.0 is now exactly specified. Its multilingual source-profile design defines how different human-language words can express one program with the same meaning. The specification is ready; teaching that contract to the compiler comes next.*

Windvale is a source-available [E-Worker Inc](https://eworker.ca) experiment to build an entire, understandable computing stack from the ground up. Its code and documentation are authored entirely by AI systems under human direction and review.

AI systems produce the source and prose. Humans define the objectives, direct the work, review and test the results, decide what the project accepts and publishes, and remain responsible for publication. [E-Worker Inc](https://eworker.ca) provides project stewardship.

At its center is a **new programming language**, together with its compiler, portable bytecode, verified runtime, assembler, object model, linker, and Foundation library. The long-term integration goal is a **new small operating system** capable of loading and running the same verified Windvale programs that run on Windows and Linux. The language and tools remain independently useful before the operating system is complete.

**[Visit windvale.ca](https://windvale.ca/)** · **[Try the browser playground](https://windvale.ca/playground/)** · **[Support Windvale](https://windvale.ca/support/)**

## Project status

Windvale Seed is experimental and not production-stable. This README is the stable public overview; changing implementation detail belongs in the documents that own it:

- [Progress dashboard](Documents/Project/Progress.md) — authoritative current implementation and qualification snapshot
- [Development roadmap](Documents/Project/Roadmap.md) — forward product and workflow milestones, sequencing, and completion gates
- [Filesystem implementation plan](Documents/Project/Windvale-Filesystem-Implementation-Plan.md) — current host-adapter, FAT32-service, and boot-integration sequence
- [Seed implementation](Documents/Architecture/Seed-Implementation.md) — component ownership and implemented boundaries
- [Specification index](Specifications/README.md) — current language, format, runtime, native, and OS contracts
- [Qualification evidence](Documents/Project/Seed-Verification-Evidence.md) — exact completed cross-host runs and artifact identities

## Working paths

Several paths already work end to end:

```text
Windvale source -> deterministic WVB -> verification -> execution on Windows or Linux
Windvale assembly -> verified WVO object -> deterministic linked x86-64 image
Portable Sum-Data.wv -> the same canonical WVB -> Windows, Linux, and Windvale OS
Portable Function-Only.wv -> the same canonical WVB -> Windows, Linux, and Windvale OS
```

The Windvale-written compiler and native tools now own the accepted normal
Windows and Linux build, verification, inspection, execution, assembly, link,
packaging, WebAssembly, OS-image, and bootstrap paths. Decisions 0526 and 0558
qualified the native transition and preserved the frozen Stage 0 implementation
in its exact recovery release. Forward language semantics belong exclusively in
`Compiler/Windvale`.

## Browser playground

The normal playground is a static Monaco application over the Windvale-native WebAssembly pipeline. One disposable worker loads the digest-pinned interpreter and portable source compiler, constructs canonical `WVSS 1`, compiles to WVB, verifies the returned module again, and executes its scalar entry point. Source stays in the browser.

**[Open the Windvale Playground](https://windvale.ca/playground/)**

Normal browser artifact production uses the native source front door to rebuild both WVB inputs, the pinned native source compiler to rebuild the portable compiler, and the pinned native WebAssembly compiler to rebuild the interpreter Wasm. Historical Stage 0 reconstruction begins from the immutable recovery release rather than this checkout.

Run it locally:

```powershell
cd Website
npm install
npm run dev
```

The [playground host specification](Specifications/Browser-Playground.md) defines its limits and non-claims. This is a browser host for the language, not a browser boot of Windvale OS and not an assertion that WebAssembly defines Windvale semantics.

The [experimental WebAssembly target](Specifications/Windvale-WebAssembly.md) and [playground exploration](Documents/Project/WebAssembly-Playground-Exploration.md) describe the implemented profiles, evidence, limits, and remaining cross-host/browser-hardening work. Those documents own the fast-changing backend detail.

## Quick start

Install the Windvale 0.1.0 preview per user without administrator or root access.
The bootstrap downloads the exact published installer, verifies its SHA-256,
installs it, persists the command path, and activates that path in the current
terminal.

On Windows PowerShell, download the script and dot-source it so `wv` is
available immediately:

```powershell
Invoke-WebRequest https://raw.githubusercontent.com/eworker-inc/Windvale/main/Distribution/Installers/Install-Windvale.ps1 -OutFile Install-Windvale.ps1
Unblock-File .\Install-Windvale.ps1; . .\Install-Windvale.ps1
```

On Linux, download and source the corresponding script:

```sh
curl -fLO https://raw.githubusercontent.com/eworker-inc/Windvale/main/Distribution/Installers/Install-Windvale.sh
. ./Install-Windvale.sh
```

Confirm either installation with:

```text
wv version
wv doctor
wv tools
```

The versioned archives, checksums, signed manifest, and complete offline
verification envelope remain available from the
[Windvale 0.1.0 release](https://github.com/eworker-inc/Windvale/releases/tag/v0.1.0).

### Compile with the installed tools

Clone or download this repository, then run the same commands from its root on
Windows PowerShell or Linux:

```text
wvbuild Examples/Seed/Sum-Data.wv Artifacts/Sum-Data.wvb
wvverify Artifacts/Sum-Data.wvb
wvdump Artifacts/Sum-Data.wvb
wvrun Artifacts/Sum-Data.wvb
```

The installed compiler publishes a 494-byte WVB module, the verifier admits it,
and the runner reports `Result: 29`.

### Contributor build wrappers

The checkout also provides digest-pinned wrappers for reproducible contributor
work. They use project metadata and atomically replace only a verifier-admitted
output:

```bat
Tools\Native\Build-Wvb.cmd Examples\Seed\Sum-Data.wvproj Artifacts\Sum-Data.wvb
Tools\Native\Verify-Wvb.cmd Artifacts\Sum-Data.wvb
Tools\Native\Inspect-Wvb.cmd Artifacts\Sum-Data.wvb
Tools\Native\Run-Wvb.cmd Artifacts\Sum-Data.wvb
```

The Linux equivalents are `Build-Wvb.sh`, `Verify-Wvb.sh`, `Inspect-Wvb.sh`, and
`Run-Wvb.sh` in the same directory. The
[Stage 0 recovery pointer](Bootstrap/Stage0/README.md) records the archived
toolchain separately.

For native build and read-only WVB tooling, see the [native source-to-WVB runbook](Documents/Runbooks/Native-Source-To-Wvb.md). For the focused verification rhythm, bootstrap convergence, assembly, linking, and component examples, continue with the [Seed development runbook](Documents/Runbooks/Seed-Development.md).

## Seed language example

```text
module Sumˉdata profile portable;

data Values: [i32] = [3, 5, 8, 13];

fn Add(Left: i32, Right: i32) -> i32 {
    return Left + Right;
}

export fn Main() -> i32 {
    var Index: i32 = 0;
    var Total: i32 = 0;

    while Index < length(Values) {
        Total = Add(Total, Values[Index]);
        Index = Index + 1;
    }

    return Total;
}
```

## Repository layout

- `Compiler/` — parsing, semantic analysis, Windvale IR, and code generation
- `Assembler/` — Windvale and reference WVA parsing, instruction encoding, and WVO production
- `Object-Model/` — structured sections, symbols, relocations, and serialization
- `Linker/` — symbol resolution, layout, relocation, and image production
- `Runtime/` — bytecode loading, verification, execution, and host adaptation
- `Foundation/` — reusable Windvale APIs and implementations needed by current tools
- `Libraries/` — reusable portable, hosted, protocol, and system-facing Windvale APIs
- `Applications/` — useful deployable Windvale entry points
- `Projects/` — cross-component Workspace 1 / Project 2 build inputs
- `Distribution/` — checked-in package manifests, locks, and future release metadata
- `Operating-System/` — boot, kernel, processes, services, and platform work
- `Tools/` — CLI, editors, website support, inspection, and verification tools
- `Tests/` — conformance, integration, malformed-input, and reproducibility coverage
- `Specifications/` — implemented language, bytecode, module, native, and platform contracts
- `Documents/` — architecture, decisions, project direction, evidence, and runbooks
- `Website/` — the public [windvale.ca](https://windvale.ca/) project site

## Architecture direction

- Windows and Linux are permanent Windvale runtime and development hosts, not the semantic definition of the language.
- Each Windvale part declares its honest platform scope; parts that use shared contracts can cross environments, while Windows-, Linux-, or Windvale OS-specific parts remain valid and explicit.
- Source syntax, typed WIR, distributable bytecode, native IR, object files, and executable images remain distinct contracts.
- Platform scope, authority level, required capabilities, and optional capabilities remain separate metadata dimensions; a requirement never grants authority by itself.
- The durable [Windvale OS architecture](Documents/Architecture/Windvale-Os-Architecture.md) uses a small capability-oriented kernel written primarily in `.wv`, a bounded `.wva` machine layer, and isolated Windvale services.
- [Proposed next integrated defaults](Documents/Decisions/0198-Next-Integrated-Architecture-Defaults.md) connect the next resource-domain, process, console, network, trust, package, and language contracts for product review without claiming implementation.
- The frozen Stage 0 implementation remains in an immutable qualified release for recovery and historical differential work.
- Bootstrap dependencies and AI contributions must be documented honestly and reproducibly.

## Documentation

Start with the [documentation guide](Documents/README.md). It separates current status, enduring architecture, specifications, accepted decisions, historical evidence, and operational records.

## License, stewardship, and participation

Windvale-owned work is source-available under the [Windvale Community Source License 1.0](LICENSE.md). Personal, noncommercial, evaluation, and qualifying small-organization uses are free; large-organization production use and Windvale-as-a-product use require a separate commercial agreement with [E-Worker Inc](https://eworker.ca). Independent applications created with Windvale belong to their creators and may use terms of their choice. Third-party components remain under their [separate licenses](THIRD-PARTY-NOTICES.md).

Copyright © 2026 E-Worker Inc and Windvale contributors. “Author” and “authored” describe how the project was produced; they do not assert that an AI system is a legal person or copyright holder. See [Decision 0031](Documents/Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md) for the project-wide attribution policy and [Decision 0114](Documents/Decisions/0114-Community-Source-Licensing-And-Commercial-Stewardship.md) for the licensing decision.

- [Contributing](CONTRIBUTING.md)
- [Contributor License Agreement](CONTRIBUTOR-LICENSE-AGREEMENT.md)
- [Third-party notices](THIRD-PARTY-NOTICES.md)
- [Security](SECURITY.md)
- [Governance](GOVERNANCE.md)
- [Code of conduct](CODE_OF_CONDUCT.md)
- [Support](SUPPORT.md)
- [Project identity](TRADEMARKS.md)
- [Changelog](CHANGELOG.md)

Read [AGENTS.md](AGENTS.md) and [CONTRIBUTING.md](CONTRIBUTING.md) before making non-trivial changes.
