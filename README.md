# Windvale

Windvale is an MIT-licensed [E-Worker Inc](https://eworker.ca) experiment to build an entire, understandable computing stack from the ground up. Its code and documentation are authored entirely by AI systems under human direction and review.

AI systems produce the source and prose. Humans define the objectives, direct the work, review and test the results, decide what the project accepts and publishes, and remain responsible for publication. [E-Worker Inc](https://eworker.ca) provides project stewardship.

At its center is a **new programming language**, together with its compiler, portable bytecode, verified runtime, assembler, object model, linker, and Foundation library. The long-term integration goal is a **new small operating system** capable of loading and running the same verified Windvale programs that run on Windows and Linux. The language and tools remain independently useful before the operating system is complete.

**[Visit windvale.ca](https://windvale.ca/)** · **[Try the browser playground](https://play.windvale.ca/)**

## Project overview

![Windvale project overview for July 2026](Documents/Project/Images/Windvale-Progress-July-2026.png)

*This July 2026 overview was refreshed on 1 August 2026 and is a periodic visual snapshot. The progress table below, accepted decisions, and qualification evidence govern where the project has advanced beyond what the image shows.*

## Progress at a glance

**Status key:** ✅ Working now · 🚧 Working but incomplete · ○ Planned

*“Working now” means implemented and tested for the current experimental Windvale Seed scope. It does not mean permanently finished or production-stable.*

See the [visual progress dashboard](Documents/Project/Progress.md) for the roadmap phase map, current transfer, and working end-to-end paths.

| Project area | Status | What works today | Next major milestone |
| --- | :---: | --- | --- |
| Windvale language | ✅ Working now | Typed portable and hosted programs with modules, functions, control flow, records, enums, text, bytes, and explicit capabilities | Expand the language only as compiler, library, and operating-system work requires |
| C# Stage 0 toolchain | ✅ Working now | Compiles Windvale source and provides the current recovery and reference implementation on Windows and Linux | Retire from the normal workflow after the native Windows/Linux bootstrap gate; preserve final recovery evidence |
| Compiler written in Windvale | 🚧 In progress | Compiles its complete accepted source graph and reproduces the exact 599,868-byte bytecode compiler | Move the qualified compiler through the shared native execution path |
| Portable bytecode and verifier | ✅ Working now | Deterministic `.wvb` modules can be decoded, inspected, disassembled, and safety-checked before execution | Evolve the format from implementation evidence without weakening its safety boundary |
| Runtime on Windows and Linux | ✅ Working now | The same verified module runs through the .NET-hosted portable reference interpreter with bounded resources and explicit host services | Build Windvale-native interpreter, baseline JIT, AOT, memory, and host adapters |
| Foundation library | 🚧 In progress | Shared machine, byte-ordering, decimal-parsing, and byte-construction modules are used by real Windvale tools | Add collections, text, diagnostics, and other facilities demanded by self-hosting |
| Assembler | ✅ Working now | C# and Windvale assemblers turn textual WVA assembly into matching verified WVO objects | Add instructions and addressing required by native compilation and the kernel |
| Object-file model | ✅ Working now | WVO defines verified sections, symbols, and relocations shared by the assembler and linker | Support richer object requirements discovered by the native backend |
| Linker | ✅ Working now | C# and Windvale linkers resolve symbols and relocations into the same deterministic flat x86-64 image and map | Produce executable and bootable target formats |
| CLI and inspection tools | ✅ Working now | One CLI can build explicit projects; compile, verify, inspect, and run modules; assemble and link; and inspect or verify objects | Move more command implementations into Windvale and add broader developer tooling |
| Editor support | ✅ Working now | Windvale syntax highlighting and language configuration work locally in Visual Studio Code | Package it publicly and pursue GitHub language recognition when eligible |
| Tests, specifications, and reproducibility | ✅ Working now | Valid, malformed, boundary, random-input, deterministic-output, and cross-host checks protect the current contracts | Extend the same evidence discipline to self-hosting, native code, and the operating system |
| Native compiler and host programs | 🚧 In progress | ABI 15/context 7 implements all 12 current native service leaves, including bounded Windows/Linux `file.write_bytes`; Windvale supplies live process-input leaves and owns executable-image layout plus lifetime policy | Admit the compiler's first record-shaped function workload, then continue exact native preflight |
| Windvale operating system | 🚧 In progress | The pre-paging probe-20 baseline qualifies ABI 15/context 7, normalized vectors 6/13, and clean Q35 poweroff; the current page-table extension is an implemented candidate | Cross-host qualify the page-table candidate, then boot the AOT Windvale verifier in-guest |
| Open-source project foundation | 🚧 In progress | The public GitHub repository, MIT licensing, contribution, security, governance, support, and authorship policies are live | Record the initial publication baseline and establish ongoing public project operations |

**Working end to end today:**

```text
Windvale source -> compiled WVB -> verification -> execution on Windows or Linux
Windvale assembly -> verified WVO object -> deterministic linked x86-64 image
System-profile Hello-World.wv -> verified WVO -> linked UEFI image -> post-firmware serial output
Portable Native-Wvb-Probe.wv -> verified WVB -> ABI-15 WVO -> linked UEFI image -> kernel-owned execution
Hosted Wv-Dump-Core.wv -> ABI-15 W^X/WVO execution -> complete deterministic real-WVB report
Verified native fragment -> Windvale publication plan -> narrow Windows/Linux W^X adapter
Windvale lifetime plan -> internal executable-image state owner -> allocate/copy/seal/invoke/release
WVA Q35 shutdown adapter -> deterministic clean VM poweroff after successful kernel return
WVA trap entries -> one normalized ring-0 frame for CPU faults with and without error codes
Hosted file.write_bytes -> exact native Windows/Linux leaf -> bounded whole-file publication
WVA paging mechanics -> kernel-owned low-1-GiB W^X identity root -> continued Windvale execution
```

**Shared native WVB in the OS:** [`Hello-World.wv`](Operating-System/Kernel/Hello-World.wv) still supplies the special system-profile diagnostics, while ordinary portable [`Native-Wvb-Probe.wv`](Operating-System/Kernel/Native-Wvb-Probe.wv) compiles to canonical verified WVB and then to the shared native path. Exact commit `12e9e2e` qualifies the pre-paging probe-20 baseline with bridge 10, ABI 15/context 7, a zero file-output-table pointer, WVA-owned Q35 poweroff, and normalized vector-6/vector-13 entries. The current Decision 0088 candidate retains those contracts and runs them under a kernel-owned page-table root. After the loader exits UEFI boot services, claims and clears a 64 KiB arena, exercises its allocator, copies the handoff, and switches to an 8 KiB kernel stack, the portable module loops over immutable i32 data, passes borrowed bytes into an internal function, slices and reads them, checks `u8`/`u32` results, and must return exact result 29 before boot continues. This is host-built AOT evidence; the guest does not yet load or verify WVB. Version 17 remains useful historical evidence:

```text
windvale-os-boot 17
entry=pass
system-table=pass
memory-map=pass
boot-services=exited
memory-owned=pass
allocator=pass
kernel-stack=pass
Hello from Windvale
cpu-exceptions=armed
native-context=pass
native-wvb=pass
windvale-source=pass
status=pass
```

Probe 17 uses the first allocated 4 KiB page for a vector-6-only IDT, disables maskable interrupts, installs the gate from live `CS` and the complete terminal-handler address on the kernel stack, and adds `cpu-exceptions=armed` after Hello World on the normal path. A separately selected `invalid-opcode` image runs the same Main chain, executes `UD2`, and terminates with exact `panic=invalid-opcode`, `vector=6`, `error-code=none`, and `status=panic` lines plus QEMU host code 3. Both exact 17,920-byte images are qualified under pinned QEMU: the normal image SHA-256 is `d2c0a7e4e5e1605fc8639c05ab27ad07ee2b015ad2dc151d8637830b8acb3f18`, and the invalid-opcode image SHA-256 is `26ccfaf862024e022339ca9fa8114c71b4fe601fe59a806d366e1d330b6d106d`. This is a terminal CPU exception proof, not recovery, a general interrupt system, or a mapping of Windvale runtime traps to processor faults.

Probe 19 introduced the retained WVA-authored Q35 shutdown function and first reusable exception-entry mechanics. Vector 6 pushes a synthetic error code; vector 13 preserves the CPU error code; both reach one 40-byte normalized ring-0 frame and terminal handler. Its standalone image identities are construction history; the same unchanged machine contracts are cross-host qualified through probe 20.

The pre-paging probe-20 baseline at exact commit `12e9e2e` qualifies Decision 0087's ABI rebuild with those WVA paths. Its three 20,992-byte images pass Windows and Debian qualification, all 18 OS tests, and exact-archive pinned QEMU: normal SHA-256 `d4a9e3625779dd3ef2a03fd71ecfe1502c1ad39378da7adbcf7e4b55636eed8c` exits 0, invalid-opcode SHA-256 `705670b1054589b80e3c918c03e9f751304e3f4b5bda77485f606433db68a757` exits 3 with `(6, 0)`, and general-protection SHA-256 `df45d8e0f69581e5ed3b46608598e6170413f80c5c1bbba9233e9842cdd7a04d` exits 3 with `(13, 0)`.

Candidate Decision 0088 extends the same probe version with the first kernel-owned page-table root. Page zero is absent, ordinary low-1-GiB pages are writable/NX, and a fixed 64 KiB boot window is read-only/executable under `CR0.WP`; compiler-generated Windvale prints `paging=owned` only after CR3 readback succeeds. Three exact 22,016-byte images pass 20 local OS tests, 6 focused assembler tests, and real pinned QEMU: normal SHA-256 `392a2801bd8d8895bd9c34213336a69057c1ae81675269056c60b8c3e974ab01`, invalid opcode SHA-256 `aa610e6ac00ed43466a87521bb4cebb2934d0885acb960db8913f025ced9cce9`, and general protection SHA-256 `74632fcde4873f2d46e18b1b77c5cc8b495e83f0f750930e039da27dd67cd0ee`. Its cross-host qualification remains pending. This is one ring-0 identity map, not process isolation, page-fault recovery, or a general virtual-memory manager.

The exact ownership and evidence limits are recorded in the decisions through candidate [Decision 0088](Documents/Decisions/0088-First-Kernel-Owned-X64-Page-Tables.md), the [native file-output specification](Specifications/Windvale-Native-File-Output.md), the [kernel-paging specification](Specifications/Windvale-Kernel-Paging.md), and the linked native, memory, trap-frame, CPU-exception, and shutdown specifications.

**Current focus:** admit the next exact compiler blocker: record-shaped parameters or locals in `Compilerˉbodyˉblockˉstepˉvalid` (`WVN2002`). Native file output and the pre-paging probe-20 baseline are qualified at `12e9e2e`; the page-table extension still requires exact cross-host qualification. The next OS vertical slice after that evidence is an AOT Windvale decoder/verifier admitting one embedded canonical WVB inside the guest. Raw Windows/Linux memory authority plus OS table construction, descriptor publication, terminal policy, packaging, and linking remain inside narrow C# Stage 0 adapters.

**Latest qualified evidence:** exact commit `12e9e2e` passes zero-warning Windows and Debian Qualification with all 66 Seed tests and the complete CLI/reproduction gate. ABI 15/context 7 supplies all 12 current native service leaves and advances the exact compiler preflight beyond `file.write_bytes`. Normalized contracts and all 69 portable artifacts (7,848,859 bytes) match byte for byte; both hosts pass all 18 OS tests. Pinned QEMU qualifies probe 20's clean exit plus normalized vector-6 and vector-13 terminal paths. C# remains the native WVB loader/compiler/verifier, platform memory and arena authority, invocation adapter, and independent recovery implementation. See [Decision 0087](Documents/Decisions/0087-Native-Windows-And-Linux-File-Output.md), the [qualification evidence](Documents/Project/Seed-Verification-Evidence.md), and the [development roadmap](Documents/Project/Roadmap.md) for the complete scope.

Today, Windvale uses dependency-free C# and .NET as its Stage 0 bootstrap. C# is a transition and reference implementation: it makes the compiler, bytecode verifier, runtime, assembler, object model, linker, and CLI executable, testable, and recoverable on Windows and Linux while those components are progressively implemented in Windvale itself. C# does not define Windvale's language semantics or the final self-hosted path. After the native-retirement gate, .NET leaves the normal build, test, packaging, and execution workflow; the final Stage 0 release may remain only as archived recovery and provenance evidence.

The accepted native destination keeps canonical WVB as the portable program identity while a Windvale-written execution stack supplies a verified interpreter, low-latency baseline JIT, optional measured optimizing tier, deterministic AOT, native memory management, and narrow Windows, Linux, and Windvale OS adapters. JIT and AOT share one native ABI, backend, and relocation model. [Decision 0059](Documents/Decisions/0059-First-Shared-Native-Wvb-Slice.md) through [Decision 0076](Documents/Decisions/0076-Native-Windows-And-Linux-File-Input.md) cross-host qualify the Stage 0 seam through ABI 14 and remove the last managed service callback. [Decision 0077](Documents/Decisions/0077-First-Windvale-Owned-Native-Stencil.md) qualifies the first active WVA-authored, Windvale-assembled runtime stencil without changing its machine bytes; [Decision 0078](Documents/Decisions/0078-Multi-Patch-Windvale-Native-Stencil.md) qualifies the measured eight-location extension for the second argument leaf; [Decision 0079](Documents/Decisions/0079-First-Windvale-Native-Stencil-Consumer.md) qualifies their exact validator and patch applier in Windvale; [Decision 0080](Documents/Decisions/0080-Native-Byte-Result-And-Live-Stencil-Consumption.md) adds the bounded descriptor-entry bridge and routes the live service path through its retained Windvale WVB. [Decision 0082](Documents/Decisions/0082-Windvale-Owned-Native-Publication-Layout.md) cross-host qualifies retained Windvale code that plans every executable-image extent and service placement before the host allocates writable memory. [Decision 0083](Documents/Decisions/0083-Windvale-Owned-Native-Publication-Lifetime.md) cross-host qualifies the next transfer: Windvale defines the complete lifecycle graph, while one internal C# owner contains raw platform memory authority and actual state. [Decisions 0085](Documents/Decisions/0085-First-Wva-Owned-Q35-Clean-Shutdown.md) and [0086](Documents/Decisions/0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md) qualify WVA-owned Q35 shutdown and normalized trap entries through probe 20. [Decision 0087](Documents/Decisions/0087-Native-Windows-And-Linux-File-Output.md) qualifies ABI 15/context 7 and the twelfth exact native service leaf, advancing the real compiler preflight to bounded record-shaped function admission. The larger direction, safety boundary, and exact .NET retirement conditions remain defined by [Decision 0057](Documents/Decisions/0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and the [native-execution architecture](Documents/Architecture/Native-Execution-And-Dotnet-Retirement.md).

The Windvale-written compiler lives under `Compiler/Windvale`; the independent C# reference/recovery compiler lives under `Compiler/Reference`. “Bootstrap” describes the staged and reproducible process between them, not the product name of either implementation.

[E-Worker Inc](https://eworker.ca) initiated and stewards the project. Windvale is model- and vendor-neutral. A particular system or provider is recorded only when technically, legally, or operationally material; such a record does not imply sponsorship, affiliation, endorsement, or ownership by its provider.

As of July 2026, Windvale is among the earliest known open-source efforts to build this full breadth as one coherent, AI-authored stack from an empty project: its own source-language semantics, compiler, verified bytecode, runtime, assembler, object model, linker, Foundation library, native path, and operating system. Earlier AI-authored operating systems and language/toolchain projects exist; this claim concerns the combined scope, not priority for any one component. The scope, search method, and close comparisons are recorded in the [earliest-known claim evidence](Documents/Project/Earliest-Known-Claim-Evidence.md).

Windvale is experimental and not yet stable. The assembler, object model, linker, bytecode/runtime foundation, and complete Windvale-written bytecode compiler have reproducible cross-host evidence. Exact Stage 1 to Stage 2 self-reproduction is qualified on Windows and Debian; native compiler execution, the general native toolchain, and Windvale OS remain active milestones. Development contracts may change without backward compatibility until they are explicitly stabilized.

## Current milestone: Windvale Seed

Windvale Seed is implemented as a dependency-free C# Stage 0 toolchain. It provides:

- A small typed source language with modules, functions, locals, control flow, immutable nominal records and enums, immutable text, integer and byte data, and explicit capabilities
- Bounded deterministic compile-time source-module composition with explicit transitive dependencies, nominal source contracts, and no runtime linkage
- A bounded deterministic `.wvproj` manifest and `build` command that select one root plus explicit source dependencies, plus a qualified portable Windvale-written manifest parser and native hosted shell, without changing import, WVSS, or WVB semantics
- Portable Foundation modules for bounded machine contracts, ordinal byte-span ordering, structured unsigned decimal parsing, and immutable byte construction, driven by the object core, assembler, linker, and future compiler needs
- A first Windvale-written compiler lexer that streams the complete implemented Seed token surface over strict UTF-8 bytes without a token collection
- A Windvale-written declaration parser that discovers module/declaration shapes and balanced function-body spans as immutable source views without a declaration collection
- A Windvale-written body parser that reproduces the complete implemented statement/expression grammar as flat child-span views without a syntax tree collection
- A canonical Windvale Source Set (`WVSS 1`) reader that gives the portable semantic pipeline bounded random access to a root plus ordered dependency sources without host objects or native paths
- A Windvale-written import graph and declaration/signature binder with independently validated packed symbol evidence, transitive visibility, and deterministic nominal identities
- A cross-host-qualified Windvale-written body binder with canonical parameter/local evidence and a typed WVIR producer with explicit blocks, temporaries, operations, source spans, independent binary validation, and fused successful-path local discovery
- A Windvale-written WVIR-to-WVB backend with static multi-module flattening, canonical function/data/type/capability ordering, all three root profiles, primitive static data, immutable records, nominal enums, explicit capabilities, text/bytes and Foundation intrinsics, exact Stage 0 byte equality, mandatory verification, runtime execution, and qualified exact Stage 1 to Stage 2 self-reproduction
- A Windvale-written import-graph phase that resolves the complete WVSS root closure and rejects duplicate, missing, cyclic, and unreachable imports without host collections
- Foundation `u8`, `u32`, immutable byte slices and concatenation, bounded signed/unsigned little-endian reads and writes, exact SHA-256 identity, and explicit byte widening
- Strict UTF-8 validation/encoding/decoding, safe ASCII quoting, deterministic enum names, invariant integer formatting, and bounded text construction
- A Windvale-written `.wvb` decoder that validates every section payload, reports declarations, and walks complete instruction streams through a hosted file shell
- A canonical x86-64-first WVO 1.0 object model with sections, symbols, relocations, a bounded C# oracle, and a Windvale-written producer/structural inspector
- A versioned WVA 1 textual assembly contract and Stage 0 assembler that infers definition offsets/sizes and emits verified WVO objects
- A Windvale-written WVA assembler that performs bounded scanning and semantic validation, derives definition ranges, encodes the complete initial x86-64 instruction/data set, constructs canonical WVO objects, and writes only a fully accepted result
- Two qualified active WVA-authored native-runtime stencils whose Windvale-assembled WVOs are strictly validated and typed-patched into the unchanged `process.argument_count` and `process.argument` leaves
- A separate Stage 0 linker that resolves verified WVO inputs, lays out a bounded x86-64 flat memory image, applies checked relocations, independently reconstructs the result, and emits a canonical path-free map
- A qualified Windvale-written linker that validates WVO, resolves and lays out inputs, constructs and independently reconstructs relocated images, emits the canonical map, and publishes only after complete success
- A stack-independent typed Windvale IR
- Deterministic `.wvb` bytecode generation
- A bounded binary reader and mandatory control-flow/type verifier
- A human-readable module inspector and disassembler
- A portable .NET reference runtime
- Explicit hosted arguments, bounded first-read file snapshots and file output, standard output, separate diagnostics, support preflight, and exact capability authorization
- Conformance, malformed-input, determinism, diagnostics, and runtime-limit coverage
- One CLI with project `build`, module `compile`, `inspect`, `verify`, and `run`, textual `assemble`, deterministic `link`, plus object `object-inspect` and `object-verify` commands

## License and stewardship

Windvale is open source under the [MIT License](LICENSE). Copyright © 2026 [E-Worker Inc](https://eworker.ca) and Windvale contributors. The company is the project business and steward.

“Author” and “authored” describe how the project was produced; they do not assert that an AI system is a legal person or copyright holder. The MIT License grants permissions from each applicable rightsholder for rights that subsist. See [Decision 0031](Documents/Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md) for the project-wide attribution policy.

## Contributing and project policies

- [Contributing](CONTRIBUTING.md) — development model, evidence, review, DCO sign-off, and licensing
- [Security](SECURITY.md) — supported versions and private vulnerability reporting
- [Governance](GOVERNANCE.md) — stewardship, roles, decisions, releases, and identity
- [Code of conduct](CODE_OF_CONDUCT.md) — participation and private conduct reporting
- [Support](SUPPORT.md) — public help channels and current support limits
- [Project identity](TRADEMARKS.md) — permitted reference to Windvale and E-Worker names and visual identity
- [Changelog](CHANGELOG.md) — unreleased status and initial `0.y.z` versioning policy
- [GitHub publication runbook](Documents/Project/GitHub-Publication-Runbook.md) — completed visibility procedure and remaining publication-baseline follow-up

## Requirements

- .NET SDK 10.0.302 or a compatible later patch in the same feature band
- Windows or Linux

The repository pins the SDK in `global.json` and uses no external NuGet packages.

## Editor support

The repository includes [Windvale language support](Tools/Editors/Windvale/README.md) for `.wv` source files. Its TextMate-compatible `source.windvale` grammar provides Windvale syntax highlighting and a Visual Studio Code language configuration while the language builds enough public adoption for a future GitHub Linguist submission.

Preview it from the repository root in a Visual Studio Code Extension Development Host:

```powershell
code --extensionDevelopmentPath=Tools/Editors/Windvale .
```

GitHub does not load repository-local grammars, so GitHub file highlighting and the repository language bar will continue to omit Windvale until Linguist accepts the language. The project does not misclassify `.wv` as C# or another existing language in the meantime.

## Browser playground

The experimental Stage 0 playground now compiles, verifies, and runs Windvale entirely in the browser through .NET WebAssembly. It reuses the C# reference compiler and interpreter as the current semantic oracle, produces canonical WVB, exposes only explicitly checked console and diagnostic capabilities, and shows bytecode identity and disassembly alongside execution results.

**[Open the Windvale Playground](https://play.windvale.ca/)**

Run it locally:

```powershell
dotnet run --project Tools/Windvale.Playground
```

The [playground host specification](Specifications/Browser-Playground.md) defines its exact limits and non-claims; the [tool README](Tools/Windvale.Playground/README.md) records static publication through GitHub Pages and the later custom-domain option. This is a browser host for the language, not a browser boot of Windvale OS and not yet an accepted permanent WebAssembly compiler target.

## Quick start

Build and run the complete Seed verifier on Windows:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

For the normal inner loop, let the changed paths select the relevant test areas:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Changed.ps1
```

Use the fast tier directly when you need to override that plan. Areas are repeatable and combine as a union; a displayed-name filter narrows that union further:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1 `
  -Level Fast `
  -TestArea compiler,runtime `
  -TestFilter 'declaration namespaces' `
  -FailFast `
  -TimingReportPath artifacts/seed-timing-fast.json
```

The available areas are `assembler`, `bytecode`, `compiler`, `foundation`, `golden`, `linker`, `object-model`, and `runtime`. `Verify-Changed.ps1` fails closed to all areas for broad or unrecognized implementation changes. Changed-file and Fast runs are development feedback only.

Choose verification in proportion to the changed boundary. Do not run a long gate merely because it exists:

| Change or purpose | Usual gate |
| --- | --- |
| One implementation area or focused fix | `Verify-Changed.ps1` or a filtered `Fast` run |
| Coherent cross-area development batch | `Development` |
| Release/qualification candidate or changed portable artifact identity | `Standard`, then cross-host `Qualification` when the candidate is ready |
| Compiler inventory, compiler project, or bootstrap-convergence change | `Verify-Bootstrap.ps1` or `.sh` once for the final candidate |
| OS boot, image, firmware, or kernel-seam change | The focused OS tests and boot gate |

Record which broader gates were not run and why. Skipping an unrelated long gate is expected; skipping a gate that protects the changed contract or a claimed qualification is not.

When a timing report includes the golden test, its `goldenPhases` array separates artifact compilation, baseline runtime work, compiler closures, inspection tools, assembler, linker, and contract assembly. Each phase reports elapsed time, executed VM instructions, current-thread allocated bytes, and garbage-collection deltas. These metrics are diagnostic and do not enter the conformance report.

Runtime performance work should iterate with a narrow compiler or runtime filter, use `-TestArea golden` alone for periodic measured checkpoints, and run Standard only for the final candidate. The complete suite is not required after every optimization edit.

`-Level Development` builds and runs every regular in-process test while deferring the multi-billion-instruction golden cross-host contract. It is the broad pre-commit development check. `-Level Standard` builds and runs the complete in-process conformance suite but skips native CLI qualification. The default `Qualification` level retains the complete verifier and remains mandatory for qualifying portable semantics or artifact identities.

On Linux:

```sh
./Tools/Verify/Verify-Seed.sh
```

Linux exposes the same tiers through `VERIFY_LEVEL`, comma-separated `TEST_AREAS`, `TEST_FILTER`, `FAIL_FAST`, and `TIMING_REPORT_PATH`; use `VERIFY_LEVEL=development` for the broad regular suite. GitHub runs the default qualification level on Windows and Linux concurrently.

Compiler bootstrap convergence is intentionally separate from the normal development suite because it executes billions of verified VM instructions. Run it once for a final compiler candidate:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Bootstrap.ps1
```

```sh
./Tools/Verify/Verify-Bootstrap.sh
```

The verifier builds Stage 1 with the C# recovery compiler, asks that Windvale bytecode compiler to build Stage 2 from the exact canonical source inventory, independently verifies both modules, and requires complete byte equality.

Compile, verify, inspect, and run the portable example:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Sum-Data.wv -o artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- verify artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- inspect artifacts/Sum-Data.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Sum-Data.wvb
```

The result is:

```text
Result: 29
```

Compile and run the hosted example with its capability granted explicitly:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Seed/Hello-Windvale.wv -o artifacts/Hello-Windvale.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Hello-Windvale.wvb --allow console.write_line
```

The runtime refuses that module without `--allow console.write_line`.

Compile and run the portable Foundation example that validates a static `.wvb` header:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Foundation/Read-Wvb-Header.wv -o artifacts/Read-Wvb-Header.wvb
dotnet run --project Tools/Windvale.Tool -- inspect artifacts/Read-Wvb-Header.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Read-Wvb-Header.wvb
```

It exercises `u8`, `u32`, immutable byte slices, and bounded little-endian reads and returns `Result: 1`.

Compile and run the first shared Foundation contract:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Foundation/Machine-Contracts.wv -o artifacts/Machine-Contracts.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Foundation/Machine-Contracts-Demo.wv `
  --module Foundation/Machine-Contracts.wv `
  -o artifacts/Machine-Contracts-Demo.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Machine-Contracts-Demo.wvb
```

The module exposes the exact alignment and ASCII machine-name predicates shared by the Windvale assembler and linker. The boundary demo returns `Result: 0`.

Compile and run the shared ordinal byte-span contract:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Foundation/Byte-Ordering.wv -o artifacts/Byte-Ordering.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Foundation/Byte-Ordering-Demo.wv `
  --module Foundation/Byte-Ordering.wv `
  -o artifacts/Byte-Ordering-Demo.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Byte-Ordering-Demo.wvb
```

The module compares validated spans of one immutable byte value without allocation or text decoding. The WVO object core, assembler, and linker share it for canonical name ordering; the demo returns `Result: 0`.

Compile and run the shared bounded decimal parser:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Foundation/Decimal-Parsing.wv -o artifacts/Decimal-Parsing.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Foundation/Decimal-Parsing-Demo.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Decimal-Parsing-Demo.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Decimal-Parsing-Demo.wvb
```

The module returns an imported immutable `Foundationˉu32ˉparse` record, validates arbitrary byte spans without trapping, and accepts only bounded ASCII decimal values through `u32` maximum. The assembler and linker share it; the demo returns `Result: 0`.

Compile and run the shared immutable byte-construction contract:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Foundation/Byte-Construction.wv -o artifacts/Byte-Construction.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Foundation/Byte-Construction-Demo.wv `
  --module Foundation/Byte-Construction.wv `
  -o artifacts/Byte-Construction-Demo.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Byte-Construction-Demo.wvb
```

The module creates repeated byte values with logarithmic concatenation and replaces validated ranges without mutation or traps. Its demo covers the exact 4 MiB limit; the assembler and linker share it, and the future bytecode encoder can use the same replacement contract for measured backpatching.

Compile and run the Windvale-written native-stencil consumer:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Compiler/Windvale/Native-Stencil-Core.wv -o artifacts/Native-Stencil-Core.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Compiler/Native-Stencil-Demo.wv `
  --module Compiler/Windvale/Native-Stencil-Core.wv `
  -o artifacts/Native-Stencil-Demo.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Native-Stencil-Demo.wvb --max-steps 20000000
```

The demo returns `Result: 0` after accepting and constructing both exact production stencil contracts twice and rejecting representative malformed variants. The `.wv` core contains the acceptance and patch semantics; the `dotnet` command is the current Stage 0 host used to compile and launch it.

Compile and run the transitive source-module composition example:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Foundation/Module-Composition-Demo.wv `
  --module Examples/Foundation/Module-Composition-Middle.wv `
  --module Examples/Foundation/Module-Composition-Leaf.wv `
  -o artifacts/Module-Composition-Demo.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Module-Composition-Demo.wvb
```

The compiler resolves only the explicitly supplied portable source modules, internalizes dependency records, enums, and functions into one ordinary WVB, and returns `Result: 42`. The leaf's nominal result crosses the transitive module boundary. Reordering the two `--module` inputs produces identical bytes.

Build the same exact module from its Project 1 manifest:

```powershell
dotnet run --project Tools/Windvale.Tool -- build `
  Examples/Foundation/Module-Composition-Demo.wvproj `
  -o artifacts/Module-Composition-Demo-Project.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Module-Composition-Demo-Project.wvb
```

The manifest identifies one root and the explicit source files available to its imports. It is declarative build input rather than `.wv` source, and the project and repeated-`--module` commands produce the same canonical WVB bytes. [`Windvale-Compiler.wvproj`](Windvale-Compiler.wvproj) is the first full consumer: the bootstrap verifier uses it to select the complete 12-module compiler closure while preserving the exact 599,868-byte Stage 1 artifact.

The qualified Windvale-written parser under `Tools/Windvale.Project/` validates the same supplied Project 1 bytes and exposes bounded root/source path views. Its hosted shell produces deterministic reports for differential testing. The normal `build` command still owns host-relative path resolution in C#; portable `.wv` code does not inspect Windows/Linux path syntax or ambient working-directory rules.

Compile and run the first Windvale-written compiler slice:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Compiler/Windvale/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Lexer-Core.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Compiler/Source-Lexer-Demo.wv `
  --module Compiler/Windvale/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Lexer-Demo.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Source-Lexer-Demo.wvb --max-steps 10000000
```

The lexer returns one token plus the next byte cursor and source position. It covers the complete current Seed keyword/operator surface, U+02C9 names, typed decimal literals, strict UTF-8 and string escapes, and bounded failures. The demo returns `0`. The parser advances this streaming contract; indexed token rescanning is retained only for verification.

Compile the streaming declaration pass and make it parse its own source:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Compiler/Windvale/Source-Declaration-Parser.wv `
  --module Compiler/Windvale/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Declaration-Parser.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Compiler/Source-Declaration-Parser-Tool.wv `
  --module Compiler/Windvale/Source-Declaration-Parser.wv `
  --module Compiler/Windvale/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Declaration-Parser-Tool.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Source-Declaration-Parser-Tool.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.read_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 45000000 `
  -- Compiler/Windvale/Source-Declaration-Parser.wv
```

The declaration pass parses signatures and balanced body spans so later passes can bind declarations first and parse bodies from exact immutable views. The hosted shell only supplies an explicit file snapshot and report sink.

Compile the statement/expression pass and make it parse its own source:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Compiler/Windvale/Source-Body-Parser.wv `
  --module Compiler/Windvale/Source-Declaration-Parser.wv `
  --module Compiler/Windvale/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Body-Parser.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Compiler/Source-Body-Parser-Tool.wv `
  --module Compiler/Windvale/Source-Body-Parser.wv `
  --module Compiler/Windvale/Source-Declaration-Parser.wv `
  --module Compiler/Windvale/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Body-Parser-Tool.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Source-Body-Parser-Tool.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.read_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 160000000 `
  -- Compiler/Windvale/Source-Body-Parser.wv
```

The body pass returns flat statement and expression views with bounded child spans, counts, and depths. It validates the lexer, declaration parser, and itself without retaining tokens or a syntax tree; semantic binding is the next compiler slice.

Compile the packed source-set reader and validate the real compiler frontend set:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Compiler/Windvale/Source-Set-Core.wv `
  --module Compiler/Windvale/Source-Body-Parser.wv `
  --module Compiler/Windvale/Source-Declaration-Parser.wv `
  --module Compiler/Windvale/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Set-Core.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Compiler/Source-Set-Tool.wv `
  --module Compiler/Windvale/Source-Set-Core.wv `
  --module Compiler/Windvale/Source-Body-Parser.wv `
  --module Compiler/Windvale/Source-Declaration-Parser.wv `
  --module Compiler/Windvale/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Set-Tool.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Source-Set-Tool.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.read_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 800000000 `
  -- Compiler/Windvale/Source-Set-Core.wv `
     Compiler/Windvale/Source-Body-Parser.wv `
     Compiler/Windvale/Source-Declaration-Parser.wv `
     Compiler/Windvale/Source-Lexer-Core.wv `
     Foundation/Decimal-Parsing.wv
```

WVSS 1 keeps the root first and dependencies in declared-module-name order, validates every source through the qualified syntax frontend, and exposes immutable source slices by index. `Compilerˉsourceˉgraph` resolves import topology over that boundary. `Compilerˉsourceˉsymbols` validates global declaration namespaces and signatures, creates an independently checked `WVSD 1` declaration directory, computes transitive module visibility once, and assigns canonical nominal indices. Its private `WVSI 1.1` evidence maps source-order directory identities to canonical record/enum ordinals in both directions without changing public WVSD bytes. `Compilerˉsourceˉbindings` assigns parameter/local slots and scopes, resolves body reads, assignments, constructors, functions, capabilities, and Foundation intrinsics, and publishes an independently checked `WVLB 1` binding directory. `Compilerˉsourceˉwir` performs complete implemented expression typing, field/operator checks, control-flow construction, and independent `WVIR 1` validation. Its successful path constructs parameter/local evidence and typed WVIR in one statement traversal, reuses validated lexical/declaration evidence across compiler phases, and retains checked standalone boundaries and diagnostic oracles. The exact ten-module typed-WVIR input completes in 3,912,239,584 instructions under the unchanged four-billion ceiling. `Compilerˉsourceˉwvb` lowers a complete graph to one canonical WVB 1.6 module: it preserves the portable, hosted, or system root, statically internalizes portable dependency functions and nominal types, precomputes immutable canonical order tables, and avoids reparsing accepted source merely to recover emission coordinates. It translates owner-aware WVSD identities to ordinal WVB function/data/capability indices, emits only root exports, serializes canonical Types and Capabilities metadata, interns escaped Unicode literals across modules deterministically, and remains byte-identical to Stage 0 for all five differential fixtures. Over the real 12-module, 677,073-source-byte compiler closure, Stage 0 produces a 599,868-byte Stage 1 compiler and Stage 1 produces a byte-identical Stage 2 compiler in 6,700,562,174 verified VM instructions. Both artifacts have SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`. The current 4 MiB WVSS envelope is therefore sufficient for this real bootstrap; parity with Stage 0's 16 MiB input limit remains a separate future contract decision.

Compile and run the first Windvale-written `wvdump` core:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile Examples/Foundation/Wv-Dump-Core.wv -o artifacts/Wv-Dump-Core.wvb
dotnet run --project Tools/Windvale.Tool -- inspect artifacts/Wv-Dump-Core.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Wv-Dump-Core.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.read_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 10000000 `
  -- artifacts/Sum-Data.wvb
```

This hosted module reads an explicit file argument through a bounded capability while pure Windvale functions validate WVB 1.6, decode declarations and nominal shapes, walk every instruction, and emit a versioned ASCII-safe line report. It validates the complete module before normal output. With no program arguments it runs embedded valid and adversarial self-checks.

Compile the first Windvale-written WVO object producer, write its representative object through an explicit capability, and inspect it with the independent Stage 0 object reader:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Foundation/Wvo-Object-Core.wv `
  --module Foundation/Byte-Ordering.wv `
  -o artifacts/Wvo-Object-Core.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Wvo-Object-Core.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.write_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 10000000 `
  -- artifacts/Sample.wvo
dotnet run --project Tools/Windvale.Tool -- object-verify artifacts/Sample.wvo
dotnet run --project Tools/Windvale.Tool -- object-inspect artifacts/Sample.wvo
```

The object is exactly 189 bytes. It contains `.text` and `.rodata`, local/export/import symbols, and one x86-64 relative `i32` relocation. The verifier rejects noncanonical or malformed objects before inspection.

Assemble the first WVA 1 source and inspect its canonical object:

```powershell
dotnet run --project Tools/Windvale.Tool -- assemble Examples/Assembler/Hello-Object.wva -o artifacts/Hello-Object.wvo
dotnet run --project Tools/Windvale.Tool -- object-verify artifacts/Hello-Object.wvo
dotnet run --project Tools/Windvale.Tool -- object-inspect artifacts/Hello-Object.wvo
```

The Stage 0 assembler emits exact x86-64 instruction bytes, derives symbol offsets and sizes from named definitions, and records unresolved relative and absolute fixups. It never performs link layout or import resolution. The same WVA contract remains the recovery oracle for the Windvale-written implementation.

Compile and run the Windvale-written WVA assembler against that source:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Assembler/Windvale/Wva-Assembler-Core.wv `
  --module Foundation/Machine-Contracts.wv `
  --module Foundation/Byte-Ordering.wv `
  --module Foundation/Decimal-Parsing.wv `
  --module Foundation/Byte-Construction.wv `
  -o artifacts/Wva-Assembler-Core.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Wva-Assembler-Core.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.read_bytes `
  --allow file.write_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 10000000 `
  -- Examples/Assembler/Hello-Object.wva artifacts/Hello-Object-Windvale.wvo
dotnet run --project Tools/Windvale.Tool -- object-verify artifacts/Hello-Object-Windvale.wvo
```

The assembler validates the complete WVA 1 declaration, section, definition, statement, numeric, ordering, limit, and reference model without host text parsing. It measures the complete object, derives ranges and relocation indices through bounded passes, encodes exact instruction/data and canonical WVO records, and calls the hosted writer only after success. With no program arguments it runs embedded valid, adversarial, and encoding checks. Link layout and relocation application remain separately owned.

Assemble a provider object and link the two verified inputs into the first flat image:

```powershell
dotnet run --project Tools/Windvale.Tool -- assemble Examples/Linker/Console-Provider.wva -o artifacts/Console-Provider.wvo
dotnet run --project Tools/Windvale.Tool -- link `
  --base-address 1048576 `
  --entry Main `
  -o artifacts/Hello-Linked.bin `
  artifacts/Hello-Object.wvo `
  artifacts/Console-Provider.wvo
```

The Stage 0 linker verifies both WVO inputs, resolves `Console_write`, places actual section addresses with alignment, materializes zero padding, applies the relative call and absolute data relocation, independently reconstructs every output byte, and writes a 24-byte image. The Windvale-written `Wvˉlinkerˉcore` under `Linker/Windvale/` implements the same contract as verified bytecode: it validates each immutable input snapshot, independently reconstructs the image, constructs the complete bounded canonical map, and invokes the host writer once only after every deterministic step succeeds. The two implementations produce byte-identical images and maps. The raw image SHA-256 is `0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a`; it is a memory-layout experiment, not itself a Windows, Linux, UEFI, or Windvale OS executable. The separate UEFI adapter under `Linker/Reference/` consumes verified flat link evidence for the accepted boot path.

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

- `Compiler/` — source lexer, parser, semantic analysis, typed WIR, and bytecode lowering
- `Foundation/` — portable Windvale source modules with explicit multi-consumer contracts
- `Assembler/Windvale/` — Windvale-written WVA parser, semantic validation, x86-64 encoding, and WVO production
- `Assembler/Reference/` — independent C# Stage 0 assembler and recovery oracle
- `Linker/Windvale/` — Windvale-written global symbol resolution, flat-image layout, relocation, independent verification, and canonical maps
- `Linker/Reference/` — independent C# Stage 0 linker, recovery oracle, and currently C#-only UEFI target adapter
- `Runtime/Windvale.Bytecode/` — module contracts, codec, verifier, digest, and inspector
- `Runtime/Windvale.Runtime/` — verified-bytecode reference interpreter and capability host
- `Object-Model/Windvale.ObjectModel/` — WVO contracts, codec, verifier, digest, and inspector
- `Tools/Windvale.Project/` — the C# project reader/resolver plus the portable Windvale-written manifest parser and hosted inspection shell
- `Tools/Windvale.Tool/` — command-line composition
- `Tools/Verify/` — Windows and Linux verification entry points
- `Tests/` — dependency-free Seed conformance runner
- `.github/` — public issue, pull-request, dependency-update, and verification automation
- `Examples/Seed/` — portable and hosted example programs
- `Examples/Foundation/` — incremental programs that exercise self-hosting prerequisites
- `Examples/Assembler/` — canonical WVA input examples and object-production coverage
- `Examples/Linker/` — canonical WVA provider inputs for multi-object linking
- `Specifications/` — implemented source, bytecode, CLI, and conformance contracts
- `Documents/` — architecture, decisions, project direction, and open questions

## Architecture direction

- Windows and Linux are permanent Windvale runtime and development hosts.
- Portable Windvale modules run through Windvale-defined contracts instead of inheriting host semantics.
- Source syntax, typed WIR, distributable bytecode, and future native IR remain distinct contracts.
- Portable, hosted, and system programming have explicit capability profiles.
- The OS begins with the [accepted x86-64 UEFI 2.11 environment](Documents/Decisions/0044-First-X64-Uefi-Boot-Environment.md): pinned QEMU Q35/TCG automation first, then Hyper-V Generation 2 compatibility, without making either host define Windvale semantics.
- The durable [Windvale OS architecture](Documents/Architecture/Windvale-Os-Architecture.md) uses a small capability-oriented kernel written primarily in `.wv`, a bounded `.wva` machine layer, and isolated Windvale services; C# remains Stage 0 and recovery rather than an OS dependency.
- Bootstrap dependencies and AI contributions must be documented honestly and reproducibly.

## Documents

Start with the [documentation guide](Documents/README.md). It separates current project status, enduring architecture, specifications, decisions, historical evidence, and operational records so the root README does not become a second incomplete index.

- [Visual progress dashboard](Documents/Project/Progress.md) and [development roadmap](Documents/Project/Roadmap.md)
- [Project vision](Documents/Project/Project-Vision.md) and [open questions](Documents/Project/Open-Questions.md)
- [Architecture documents](Documents/README.md#architecture)
- [Specification index](Specifications/README.md)
- [Accepted decisions](Documents/Decisions/) and [latest qualification evidence](Documents/Project/Seed-Verification-Evidence.md)

## Development

Read [AGENTS.md](AGENTS.md) and [CONTRIBUTING.md](CONTRIBUTING.md) before making non-trivial changes. Use the repository remote and active branch configured for your checkout, preserve unrelated work, and run the platform verifier appropriate to the change.
