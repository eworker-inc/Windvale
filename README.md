# Windvale

Windvale is an MIT-licensed [E-Worker Inc](https://eworker.ca) experiment to build an entire, understandable computing stack from the ground up. Its code and documentation are authored entirely by AI systems under human direction and review.

AI systems produce the source and prose. Humans define the objectives, direct the work, review and test the results, decide what the project accepts and publishes, and remain responsible for publication. E-Worker Inc provides project stewardship.

At its center is a **new programming language**, together with its compiler, portable bytecode, verified runtime, assembler, object model, linker, and Foundation library. The long-term integration goal is a **new small operating system** capable of loading and running the same verified Windvale programs that run on Windows and Linux. The language and tools remain independently useful before the operating system is complete.

Today, Windvale uses dependency-free C# and .NET as its Stage 0 bootstrap. C# is a transition and reference implementation: it makes the compiler, bytecode verifier, runtime, assembler, object model, linker, and CLI executable, testable, and recoverable on Windows and Linux while those components are progressively implemented in Windvale itself. C# does not define Windvale's language semantics or the final self-hosted path, although it may remain as an independent recovery and comparison oracle.

E-Worker Inc initiated and stewards the project. Windvale is model- and vendor-neutral. A particular system or provider is recorded only when technically, legally, or operationally material; such a record does not imply sponsorship, affiliation, endorsement, or ownership by its provider.

As of July 2026, Windvale is among the earliest known open-source efforts to build this full breadth as one coherent, AI-authored stack from an empty project: its own source-language semantics, compiler, verified bytecode, runtime, assembler, object model, linker, Foundation library, native path, and operating system. Earlier AI-authored operating systems and language/toolchain projects exist; this claim concerns the combined scope, not priority for any one component. The scope, search method, and close comparisons are recorded in the [earliest-known claim evidence](Documents/Project/Earliest-Known-Claim-Evidence.md).

Windvale is experimental and not yet stable. The assembler, object model, linker, bytecode/runtime foundation, and Windvale-written compiler frontend have reproducible evidence; the self-hosted compiler, native toolchain, and Windvale OS remain active milestones. Development contracts may change without backward compatibility until they are explicitly stabilized.

## Project overview

![Windvale project overview for July 2026](Documents/Project/Images/Windvale-Progress-July-2026.png)

*This July 2026 overview is a periodic visual snapshot. Current repository contracts and qualification evidence govern if the project later advances beyond what the image shows.*

## Current milestone: Windvale Seed

Windvale Seed is implemented as a dependency-free C# Stage 0 toolchain. It provides:

- A small typed source language with modules, functions, locals, control flow, immutable nominal records and enums, immutable text, integer and byte data, and explicit capabilities
- Bounded deterministic compile-time source-module composition with explicit transitive dependencies, nominal source contracts, and no runtime linkage
- Portable Foundation modules for bounded machine contracts, ordinal byte-span ordering, structured unsigned decimal parsing, and immutable byte construction, driven by the object core, assembler, linker, and future compiler needs
- A first Windvale-written compiler lexer that streams the complete implemented Seed token surface over strict UTF-8 bytes without a token collection
- A Windvale-written declaration parser that discovers module/declaration shapes and balanced function-body spans as immutable source views without a declaration collection
- A Windvale-written body parser that reproduces the complete implemented statement/expression grammar as flat child-span views without a syntax tree collection
- A canonical Windvale Source Set (`WVSS 1`) reader that gives the portable semantic pipeline bounded random access to a root plus ordered dependency sources without host objects or native paths
- A Windvale-written import graph and declaration/signature binder with independently validated packed symbol evidence, transitive visibility, and deterministic nominal identities
- A cross-host-qualified Windvale-written body binder with canonical parameter/local evidence and a typed WVIR producer with explicit blocks, temporaries, operations, source spans, and independent binary validation
- A Windvale-written import-graph phase that resolves the complete WVSS root closure and rejects duplicate, missing, cyclic, and unreachable imports without host collections
- Foundation `u8`, `u32`, immutable byte slices and concatenation, bounded signed/unsigned little-endian reads and writes, exact SHA-256 identity, and explicit byte widening
- Strict UTF-8 validation/encoding/decoding, safe ASCII quoting, deterministic enum names, invariant integer formatting, and bounded text construction
- A Windvale-written `.wvb` decoder that validates every section payload, reports declarations, and walks complete instruction streams through a hosted file shell
- A canonical x86-64-first WVO 1.0 object model with sections, symbols, relocations, a bounded C# oracle, and a Windvale-written producer/structural inspector
- A versioned WVA 1 textual assembly contract and Stage 0 assembler that infers definition offsets/sizes and emits verified WVO objects
- A Windvale-written WVA assembler that performs bounded scanning and semantic validation, derives definition ranges, encodes the complete initial x86-64 instruction/data set, constructs canonical WVO objects, and writes only a fully accepted result
- A separate Stage 0 linker that resolves verified WVO inputs, lays out a bounded x86-64 flat memory image, applies checked relocations, independently reconstructs the result, and emits a canonical path-free map
- A qualified Windvale-written linker that validates WVO, resolves and lays out inputs, constructs and independently reconstructs relocated images, emits the canonical map, and publishes only after complete success
- A stack-independent typed Windvale IR
- Deterministic `.wvb` bytecode generation
- A bounded binary reader and mandatory control-flow/type verifier
- A human-readable module inspector and disassembler
- A portable .NET reference runtime
- Explicit hosted arguments, bounded first-read file snapshots and file output, standard output, separate diagnostics, support preflight, and exact capability authorization
- Conformance, malformed-input, determinism, diagnostics, and runtime-limit coverage
- One CLI with module `compile`, `inspect`, `verify`, and `run`, textual `assemble`, deterministic `link`, plus object `object-inspect` and `object-verify` commands

## License and stewardship

Windvale is open source under the [MIT License](LICENSE). Copyright © 2026 E-Worker Inc and Windvale contributors. E-Worker Inc is the project business and steward.

“Author” and “authored” describe how the project was produced; they do not assert that an AI system is a legal person or copyright holder. The MIT License grants permissions from each applicable rightsholder for rights that subsist. See [Decision 0031](Documents/Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md) for the project-wide attribution policy.

## Contributing and project policies

- [Contributing](CONTRIBUTING.md) — development model, evidence, review, DCO sign-off, and licensing
- [Security](SECURITY.md) — supported versions and private vulnerability reporting
- [Governance](GOVERNANCE.md) — stewardship, roles, decisions, releases, and identity
- [Code of conduct](CODE_OF_CONDUCT.md) — participation and private conduct reporting
- [Support](SUPPORT.md) — public help channels and current support limits
- [Project identity](TRADEMARKS.md) — permitted reference to Windvale and E-Worker names and visual identity
- [Changelog](CHANGELOG.md) — unreleased status and initial `0.y.z` versioning policy
- [GitHub publication runbook](Documents/Project/GitHub-Publication-Runbook.md) — private-first import, initial baseline, and public-visibility checklist

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

When a timing report includes the golden test, its `goldenPhases` array separates artifact compilation, baseline runtime work, compiler closures, inspection tools, assembler, linker, and contract assembly. Each phase reports elapsed time, executed VM instructions, current-thread allocated bytes, and garbage-collection deltas. These metrics are diagnostic and do not enter the conformance report.

Runtime performance work should iterate with a narrow compiler or runtime filter, use `-TestArea golden` alone for periodic measured checkpoints, and run Standard only for the final candidate. The complete suite is not required after every optimization edit.

`-Level Standard` builds and runs the complete 47-test in-process conformance suite but skips native CLI qualification. The default `Qualification` level retains the complete verifier and remains mandatory for qualifying portable semantics or artifact identities.

On Linux:

```sh
./Tools/Verify/Verify-Seed.sh
```

Linux exposes the same tiers through `VERIFY_LEVEL`, comma-separated `TEST_AREAS`, `TEST_FILTER`, `FAIL_FAST`, and `TIMING_REPORT_PATH`. GitHub runs the default qualification level on Windows and Linux concurrently.

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

Compile and run the first Windvale-written compiler slice:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Compiler/Bootstrap/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Lexer-Core.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Compiler/Source-Lexer-Demo.wv `
  --module Compiler/Bootstrap/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Lexer-Demo.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Source-Lexer-Demo.wvb --max-steps 10000000
```

The lexer returns one token plus the next byte cursor and source position. It covers the complete current Seed keyword/operator surface, U+02C9 names, typed decimal literals, strict UTF-8 and string escapes, and bounded failures. The demo returns `0`. The parser advances this streaming contract; indexed token rescanning is retained only for verification.

Compile the streaming declaration pass and make it parse its own source:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Compiler/Bootstrap/Source-Declaration-Parser.wv `
  --module Compiler/Bootstrap/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Declaration-Parser.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Compiler/Source-Declaration-Parser-Tool.wv `
  --module Compiler/Bootstrap/Source-Declaration-Parser.wv `
  --module Compiler/Bootstrap/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Declaration-Parser-Tool.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Source-Declaration-Parser-Tool.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.read_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 45000000 `
  -- Compiler/Bootstrap/Source-Declaration-Parser.wv
```

The declaration pass parses signatures and balanced body spans so later passes can bind declarations first and parse bodies from exact immutable views. The hosted shell only supplies an explicit file snapshot and report sink.

Compile the statement/expression pass and make it parse its own source:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Compiler/Bootstrap/Source-Body-Parser.wv `
  --module Compiler/Bootstrap/Source-Declaration-Parser.wv `
  --module Compiler/Bootstrap/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Body-Parser.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Compiler/Source-Body-Parser-Tool.wv `
  --module Compiler/Bootstrap/Source-Body-Parser.wv `
  --module Compiler/Bootstrap/Source-Declaration-Parser.wv `
  --module Compiler/Bootstrap/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Body-Parser-Tool.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Source-Body-Parser-Tool.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.read_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 160000000 `
  -- Compiler/Bootstrap/Source-Body-Parser.wv
```

The body pass returns flat statement and expression views with bounded child spans, counts, and depths. It validates the lexer, declaration parser, and itself without retaining tokens or a syntax tree; semantic binding is the next compiler slice.

Compile the packed source-set reader and validate the real compiler frontend set:

```powershell
dotnet run --project Tools/Windvale.Tool -- compile `
  Compiler/Bootstrap/Source-Set-Core.wv `
  --module Compiler/Bootstrap/Source-Body-Parser.wv `
  --module Compiler/Bootstrap/Source-Declaration-Parser.wv `
  --module Compiler/Bootstrap/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Set-Core.wvb
dotnet run --project Tools/Windvale.Tool -- compile `
  Examples/Compiler/Source-Set-Tool.wv `
  --module Compiler/Bootstrap/Source-Set-Core.wv `
  --module Compiler/Bootstrap/Source-Body-Parser.wv `
  --module Compiler/Bootstrap/Source-Declaration-Parser.wv `
  --module Compiler/Bootstrap/Source-Lexer-Core.wv `
  --module Foundation/Decimal-Parsing.wv `
  -o artifacts/Source-Set-Tool.wvb
dotnet run --project Tools/Windvale.Tool -- run artifacts/Source-Set-Tool.wvb `
  --allow console.write_line `
  --allow diagnostic.write_line `
  --allow file.read_bytes `
  --allow process.argument `
  --allow process.argument_count `
  --max-steps 800000000 `
  -- Compiler/Bootstrap/Source-Set-Core.wv `
     Compiler/Bootstrap/Source-Body-Parser.wv `
     Compiler/Bootstrap/Source-Declaration-Parser.wv `
     Compiler/Bootstrap/Source-Lexer-Core.wv `
     Foundation/Decimal-Parsing.wv
```

WVSS 1 keeps the root first and dependencies in declared-module-name order, validates every source through the qualified syntax frontend, and exposes immutable source slices by index. `Compilerˉsourceˉgraph` resolves import topology over that boundary. `Compilerˉsourceˉsymbols` validates global declaration namespaces and signatures, creates an independently checked `WVSD 1` declaration directory, computes transitive module visibility once, and assigns canonical nominal indices. `Compilerˉsourceˉbindings` assigns parameter/local slots and scopes, resolves body reads, assignments, constructors, functions, capabilities, and Foundation intrinsics, and publishes an independently checked `WVLB 1` binding directory. `Compilerˉsourceˉwir` now performs complete implemented expression typing, field/operator checks, control-flow construction, and independent `WVIR 1` validation. WVIR-to-WVB lowering remains the next compiler stage. The current 4 MiB aggregate envelope is an explicit Seed limitation while later memory/collection evidence determines how to close parity with Stage 0's 16 MiB aggregate input contract.

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
  Examples/Assembler/Wva-Assembler-Core.wv `
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

The Stage 0 linker verifies both WVO inputs, resolves `Console_write`, places actual section addresses with alignment, materializes zero padding, applies the relative call and absolute data relocation, independently reconstructs every output byte, and writes a 24-byte image. The Windvale-written `Wvˉlinkerˉcore` now implements the same contract as verified bytecode: it validates each immutable input snapshot, independently reconstructs the image, constructs the complete bounded canonical map, and invokes the host writer once only after every deterministic step succeeds. The two implementations produce byte-identical images and maps. The raw image SHA-256 is `0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a`; it is a memory-layout experiment, not yet a Windows, Linux, UEFI, or Windvale OS executable.

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
- `Assembler/Windvale.Assembler/` — WVA parser, semantic validation, x86-64 encoding, and WVO production
- `Linker/Windvale.Linker/` — global symbol resolution, flat-image layout, relocation, independent verification, and canonical maps
- `Runtime/Windvale.Bytecode/` — module contracts, codec, verifier, digest, and inspector
- `Runtime/Windvale.Runtime/` — verified-bytecode reference interpreter and capability host
- `Object-Model/Windvale.ObjectModel/` — WVO contracts, codec, verifier, digest, and inspector
- `Tools/Windvale.Tool/` — command-line composition
- `Tools/Verify/` — Windows and Linux verification entry points
- `Tests/` — dependency-free Seed conformance runner
- `.github/` — public issue, pull-request, dependency-update, and verification automation
- `Examples/Seed/` — portable and hosted example programs
- `Examples/Foundation/` — incremental programs that exercise self-hosting prerequisites
- `Examples/Assembler/` — canonical WVA sources for assembler and object-production coverage
- `Examples/Linker/` — multi-object providers and the Windvale-written linker core
- `Specifications/` — implemented source, bytecode, CLI, and conformance contracts
- `Documents/` — architecture, decisions, project direction, and open questions

## Architecture direction

- Windows and Linux are permanent Windvale runtime and development hosts.
- Portable Windvale modules run through Windvale-defined contracts instead of inheriting host semantics.
- Source syntax, typed WIR, distributable bytecode, and future native IR remain distinct contracts.
- Portable, hosted, and system programming have explicit capability profiles.
- The OS should begin as a small vertical system running in virtual machines, with QEMU as the likely automation environment and Hyper-V as an important Windows compatibility target.
- Bootstrap dependencies and AI contributions must be documented honestly and reproducibly.

## Documents

- [Project vision](Documents/Project/Project-Vision.md)
- [GitHub publication runbook](Documents/Project/GitHub-Publication-Runbook.md)
- [Seed implementation](Documents/Architecture/Seed-Implementation.md)
- [Platform and portability model](Documents/Architecture/Platform-And-Portability.md)
- [Compiler bootstrap options](Documents/Architecture/Compiler-Bootstrap-Options.md)
- [Seed language specification](Specifications/Seed-Language.md)
- [Seed immutable records](Specifications/Seed-Records.md)
- [Seed enums and bounded formatting](Specifications/Seed-Enums-And-Formatting.md)
- [Hosted resource boundary](Specifications/Hosted-Resources.md)
- [Foundation byte primitives](Specifications/Foundation-Bytes.md)
- [Windvale wvdump core](Specifications/Wv-Dump-Core.md)
- [Windvale wvdump report](Specifications/Wv-Dump-Report.md)
- [Windvale WVO object core](Specifications/Wvo-Object-Core.md)
- [Source naming conventions](Specifications/Source-Naming.md)
- [Seed bytecode specification](Specifications/Seed-Bytecode.md)
- [Windvale object format](Specifications/Windvale-Object-Format.md)
- [Windvale textual assembly](Specifications/Windvale-Assembly.md)
- [Windvale WVA assembler core](Specifications/Wva-Assembler-Core.md)
- [Windvale linking contract](Specifications/Windvale-Linking.md)
- [Windvale linker core](Specifications/Wv-Linker-Core.md)
- [Foundation machine contracts](Specifications/Foundation-Machine-Contracts.md)
- [Foundation ordinal byte-span ordering](Specifications/Foundation-Byte-Ordering.md)
- [Foundation bounded decimal parsing](Specifications/Foundation-Decimal-Parsing.md)
- [Foundation bounded byte construction](Specifications/Foundation-Byte-Construction.md)
- [Compiler source lexer](Specifications/Compiler-Source-Lexer.md)
- [Compiler source declaration parser](Specifications/Compiler-Source-Declaration-Parser.md)
- [Compiler source body parser](Specifications/Compiler-Source-Body-Parser.md)
- [Compiler source-set contract](Specifications/Compiler-Source-Set.md)
- [Compiler source-graph contract](Specifications/Compiler-Source-Graph.md)
- [Compiler declaration and signature symbols](Specifications/Compiler-Source-Symbols.md)
- [Compiler body, local, and call binding](Specifications/Compiler-Source-Bindings.md)
- [Compiler typed source IR](Specifications/Compiler-Source-Wir.md)
- [Seed CLI specification](Specifications/Seed-CLI.md)
- [Seed conformance specification](Specifications/Seed-Conformance.md)
- [Seed verification throughput](Documents/Architecture/Seed-Verification-Throughput.md)
- [Seed verification evidence](Documents/Project/Seed-Verification-Evidence.md)
- [Repository foundation decision](Documents/Decisions/0001-Repository-And-Foundation.md)
- [Seed bootstrap decision](Documents/Decisions/0002-Windvale-Seed-Bootstrap.md)
- [Source naming and mutation decision](Documents/Decisions/0003-Source-Naming-And-Mutation.md)
- [Foundation byte primitives decision](Documents/Decisions/0004-Foundation-Byte-Primitives.md)
- [Immutable nominal records decision](Documents/Decisions/0005-Immutable-Nominal-Records.md)
- [Nominal enums and bounded formatting decision](Documents/Decisions/0006-Nominal-Enums-And-Bounded-Formatting.md)
- [Explicit hosted resources decision](Documents/Decisions/0007-Explicit-Hosted-Resources.md)
- [WvDump payload and report decision](Documents/Decisions/0008-WvDump-Payload-Decoding-And-Safe-Reports.md)
- [Minimal object foundation decision](Documents/Decisions/0009-Minimal-Windvale-Object-Foundation.md)
- [Minimal assembly contract decision](Documents/Decisions/0010-Minimal-Windvale-Assembly-Contract.md)
- [Deterministic flat-image linker decision](Documents/Decisions/0011-Deterministic-Flat-Image-Linker.md)
- [Windvale linker bootstrap prerequisites decision](Documents/Decisions/0012-Windvale-Linker-Bootstrap-Prerequisites.md)
- [Balanced persistent byte sequences decision](Documents/Decisions/0013-Balanced-Persistent-Byte-Sequences.md)
- [Windvale linker object views decision](Documents/Decisions/0014-Windvale-Linker-Object-Views.md)
- [Windvale linker resolution and layout decision](Documents/Decisions/0015-Windvale-Linker-Resolution-And-Layout.md)
- [Windvale immutable image and relocations decision](Documents/Decisions/0016-Windvale-Immutable-Image-And-Relocations.md)
- [Independent Windvale image reconstruction decision](Documents/Decisions/0017-Independent-Windvale-Image-Reconstruction.md)
- [Canonical Windvale map and publication decision](Documents/Decisions/0018-Canonical-Windvale-Map-And-Publication.md)
- [Bounded static source-module composition decision](Documents/Decisions/0019-Bounded-Static-Source-Module-Composition.md)
- [First two-consumer Foundation module decision](Documents/Decisions/0020-First-Two-Consumer-Foundation-Module.md)
- [Shared ordinal byte-span ordering decision](Documents/Decisions/0021-Shared-Ordinal-Byte-Span-Ordering.md)
- [Static nominal source contracts decision](Documents/Decisions/0022-Static-Nominal-Source-Contracts.md)
- [Shared bounded u32 decimal parsing decision](Documents/Decisions/0023-Shared-U32-Decimal-Parsing.md)
- [Bounded immutable byte construction decision](Documents/Decisions/0024-Bounded-Byte-Construction.md)
- [Streaming bootstrap source lexer decision](Documents/Decisions/0025-Streaming-Bootstrap-Source-Lexer.md)
- [Streaming declaration views decision](Documents/Decisions/0026-Streaming-Declaration-Views.md)
- [Streaming statement and expression views decision](Documents/Decisions/0027-Streaming-Statement-And-Expression-Views.md)
- [MIT license and E-Worker stewardship decision](Documents/Decisions/0028-MIT-License-And-E-Worker-Stewardship.md)
- [Canonical packed compiler source sets decision](Documents/Decisions/0029-Canonical-Packed-Compiler-Source-Sets.md)
- [Portable compiler import graphs decision](Documents/Decisions/0030-Portable-Compiler-Import-Graphs.md)
- [AI authorship and vendor neutrality decision](Documents/Decisions/0031-AI-Authorship-And-Vendor-Neutrality.md)
- [Public contribution and governance foundation decision](Documents/Decisions/0032-Public-Contribution-And-Governance-Foundation.md)
- [Portable declaration and signature binding decision](Documents/Decisions/0033-Portable-Declaration-And-Signature-Binding.md)
- [Portable body, local, and call binding decision](Documents/Decisions/0034-Portable-Body-Local-And-Call-Binding.md)
- [Canonical typed source IR decision](Documents/Decisions/0035-Canonical-Typed-Source-IR.md)
- [Open questions](Documents/Project/Open-Questions.md)
- [Development roadmap](Documents/Project/Roadmap.md)

## Development

Read [AGENTS.md](AGENTS.md) and [CONTRIBUTING.md](CONTRIBUTING.md) before making non-trivial changes. Use the repository remote and active branch configured for your checkout, preserve unrelated work, and run the platform verifier appropriate to the change.
