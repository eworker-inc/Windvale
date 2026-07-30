# Windvale Seed conformance

## Purpose

The Seed conformance suite proves that the compiler, bytecode codec, verifier, reference runtime, object model, and Stage 0 assembler agree on one portable contract. It uses only the pinned .NET SDK and repository source.

## Required checks

- Portable code-and-data compilation and execution
- Hosted capability declaration, refusal without authorization, and successful authorized output
- Ordered hosted arguments, bounded native file input and output, separate output and diagnostic sinks, unsupported-host refusal, stable resource failures, and host-result validation
- Exact deterministic module bytes and canonical declaration ordering
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

The comparison intentionally ignores operating-system description, architecture label, and installed runtime description. It requires equal module-, object-, and assembly-format versions, complete golden module and object SHA-256 values, results, and normalized program output. The golden contract covers the portable integer-data example, hosted text example, portable Foundation byte-header example, Windvale-written hosted `wvdump`, the complete normalized `wvdump` line report for the real integer-data module, the Windvale-written WVO core and its exact 189-byte object, and the canonical WVA example object. Each host verifier also exercises the native assembly and object-inspection CLI paths plus the hosted file adapters.

## Evidence discipline

A Windows pass proves Windows behavior and produces one side of the cross-host contract. A Linux pass proves Linux behavior and supplies the second side. Portable source and a cross-platform target framework are supporting evidence, not substitutes for actually running both reports.
