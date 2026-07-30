# Windvale Seed conformance

## Purpose

The Seed conformance suite proves that the compiler, bytecode codec, verifier, reference runtime, object model, and Stage 0 assembler agree on one portable contract. It uses only the pinned .NET SDK and repository source.

## Required checks

- Portable code-and-data compilation and execution
- Hosted capability declaration, refusal without authorization, and successful authorized output
- Ordered hosted arguments, bounded native file input and output, separate output and diagnostic sinks, unsupported-host refusal, stable resource failures, and host-result validation
- Exact deterministic module bytes and canonical declaration ordering
- Bounded compile-time source-module composition, transitive nominal record/enum use, dependency-order independence, root-only WVB exports, dependency semantic isolation, source-specific diagnostics, graph/shape rejection, and no-partial-output CLI behavior
- Standalone and composed `Foundationˉmachineˉcontracts` validation, exact alignment/name boundaries, two real tool consumers, dependency internalization, and unchanged assembler/linker outputs
- Standalone and composed `Foundationˉbyteˉordering` validation, exact ordinal span boundaries, three real tool consumers, and preserved tool ceilings and outputs
- Standalone and composed `Foundationˉdecimalˉparsing` validation, imported nominal results, exact range/digit/overflow boundaries, assembler/linker consumers, and unchanged binary outputs
- Standalone and composed `Foundationˉbyteˉconstruction` validation, exact 4 MiB repeat and total replacement boundaries, assembler/linker consumers, and preserved tool outputs and ceilings
- Exact codec read/write round trips
- Inspector metadata and disassembly
- Functions, `if`, `while`, booleans, immutable text, immutable integer data, indexing, and `length`
- `u8`, `u32`, immutable `bytes`, slice views, bounded unsigned and signed little-endian reads, fixed-width byte construction, immutable concatenation, strict UTF-8 encoding, and explicit `u8` widening
- Strict UTF-8 validation/decoding, ASCII-safe deterministic quoting, and decoded/quoted text-limit traps
- Immutable nominal record construction, field access, function parameters/results, canonical encoding, and verifier rejection cases
- Nominal enum constants, record fields, equality, declared names, canonical encoding, and verifier rejection cases
- Invariant signed and unsigned integer formatting plus bounded text-concatenation traps
- Windvale-written WVB envelope and payload decoding with structured results plus valid, wrong-kind, nonzero-flags, hostile-length, truncated, trailing-byte, bad-payload, and unknown-opcode cases
- Canonical WVO 1.0 sections, symbols, relocations, strict decoding, deterministic encoding, malformed-input rejection, and bounded random object inputs
- Exact byte equality between the Windvale-written WVO producer and the independent C# object oracle, including native hosted-file persistence
- WVA 1 parsing, stable diagnostics, canonical symbol/section requirements, the complete initial x86-64 instruction/data subset, inferred definition ranges, deterministic WVO output, and bounded random source
- Windvale-written WVA scanning, multi-pass semantics, exact object measurement, instruction/data encoding, definition ranges, canonical section/symbol/relocation records, and whole-object hosted persistence
- Complete byte equality between Windvale and Stage 0 for canonical, numeric-boundary, register, multi-definition, line-ending, empty-object, and accepted deterministic-mutation cases; rejected input invokes no writer and produces no native output
- Windvale-written complete WVO structural validation, immutable section/symbol/relocation views, representative-object acceptance, deterministic mutation and random-byte differential classification, capability refusal, and real hosted scanning
- Windvale-written aggregate validation, duplicate-export detection, import-kind resolution, exported-function entry selection, actual-address alignment, deterministic section placement, defined-symbol address validation, and exact analysis comparison with Stage 0
- Windvale-written immutable padding/data/zero-fill construction, local/export/import target address resolution, checked absolute and relative relocation arithmetic, persistent four-byte patching, and exact candidate-image SHA-256 comparison with Stage 0
- Independently structured actual-address placement, unrelocated-image reconstruction, full export rescanning, reverse-order relocation with separate arithmetic, byte-for-byte candidate comparison, and injected `WVL1011` mismatch rejection
- Windvale-written canonical map construction over once-validated immutable snapshots, exact 1 MiB enforcement, `WVL1012` rejection, and one publish-after-success image write only after independent reconstruction and complete map success
- Windvale Linking 1 input validation, unique export resolution, strict import-kind matching, required exported-function entry, canonical kind/input/source layout, actual-address alignment, zero padding and BSS, checked `absolute-u32` and `relative-i32` application, and complete independent image reconstruction
- Exact deterministic flat-image and canonical map bytes, including input-order sensitivity, aggregate limits, map limits, malformed objects, undefined and duplicate symbols, address and relocation overflow, no-output rejection, and real multi-object CLI linking
- Checked `u32` overflow and underflow plus byte-read and slice bounds traps
- U+02C9 source identifiers, immutable `let`, mutable `var`, immutable parameters, and exported `Main`
- Rejection of malformed or confusable identifier separators
- Stable source diagnostic codes with line and column information
- Malformed header, version, section, length, UTF-8, truncation, trailing-data, and oversize rejection
- Unknown opcode, truncated operand, invalid branch, invalid local, unreachable instruction, inconsistent stack merge, and maximum-stack rejection
- Runtime integer-overflow, data-bounds, instruction-limit, call-depth, capability-authorization, argument-bound, file-resource, and invalid-host-result traps
- Deterministic bounded random source and module inputs remain contained by result or diagnostic boundaries

## Host verification

Windows:

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

Linux:

```sh
./Tools/Verify/Verify-Seed.sh
```

Each verifier builds Release binaries, runs the complete suite, and writes an ignored JSON report under `artifacts/`. The report separates the portable `contract` evidence from host metadata.

## Comparing hosts

After collecting a report from Windows and Linux, compare their portable contracts with the test runner:

```powershell
dotnet run --project Tests/Windvale.Seed.Tests --configuration Release --no-build -- --compare-reports artifacts/seed-conformance-windows-x64.json artifacts/seed-conformance-linux-x86_64.json
```

The comparison intentionally ignores operating-system description, architecture label, and installed runtime description. It requires equal module-, object-, assembly-, and link-format versions, complete golden module, source-composition, Foundation-module, object, flat-image, and map SHA-256 values, results, and normalized program output. The golden contract covers the portable integer-data example, hosted text example, portable Foundation byte-header example, deterministic transitive nominal source composition, machine-contract, ordinal-byte-span, bounded-decimal, and immutable-byte-construction Foundation modules plus their demos, Windvale-written hosted `wvdump`, the complete normalized `wvdump` line report for the real integer-data module, the Windvale-written WVO core and its exact 189-byte object, both Stage 0 and Windvale-written encodings of the canonical WVA example, the Windvale linker core plus its canonical hosted WVO scan, and the complete Windvale-written two-object link map. Each host verifier also exercises native source composition, Foundation composition, assembly, Windvale assembly, Windvale WVO scanning, exact Windvale/Stage 0 image and map equality, no-partial-output and existing-output preservation, independent object/image verification, map-limit rejection, and hosted file adapters.

## Evidence discipline

A Windows pass proves Windows behavior and produces one side of the cross-host contract. A Linux pass proves Linux behavior and supplies the second side. Portable source and a cross-platform target framework are supporting evidence, not substitutes for actually running both reports.
