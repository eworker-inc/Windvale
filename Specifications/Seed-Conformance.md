# Windvale Seed conformance

## Purpose

The Seed conformance suite proves that the compiler, bytecode codec, verifier, inspector, and reference runtime agree on one portable contract. It uses only the pinned .NET SDK and repository source.

## Required checks

- Portable code-and-data compilation and execution
- Hosted capability declaration, refusal without authorization, and successful authorized output
- Exact deterministic module bytes and canonical declaration ordering
- Exact codec read/write round trips
- Inspector metadata and disassembly
- Functions, `if`, `while`, booleans, immutable text, immutable integer data, indexing, and `length`
- `u8`, `u32`, immutable `bytes`, slice views, and bounded little-endian reads
- Immutable nominal record construction, field access, function parameters/results, canonical encoding, and verifier rejection cases
- Nominal enum constants, record fields, equality, declared names, canonical encoding, and verifier rejection cases
- Invariant signed and unsigned integer formatting plus bounded text-concatenation traps
- Windvale-written WVB section walking with structured results plus valid, wrong-kind, nonzero-flags, hostile-length, truncated, and trailing-byte cases
- Checked `u32` overflow and underflow plus byte-read and slice bounds traps
- U+02C9 source identifiers, immutable `let`, mutable `var`, immutable parameters, and exported `Main`
- Rejection of malformed or confusable identifier separators
- Stable source diagnostic codes with line and column information
- Malformed header, version, section, length, UTF-8, truncation, trailing-data, and oversize rejection
- Unknown opcode, truncated operand, invalid branch, invalid local, unreachable instruction, inconsistent stack merge, and maximum-stack rejection
- Runtime integer-overflow, data-bounds, instruction-limit, call-depth, and capability-authorization traps
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

The comparison intentionally ignores operating-system description, architecture label, and installed runtime description. It requires equal module-format version, complete golden-module SHA-256 values, results, and normalized program output. The golden contract covers the portable integer-data example, hosted text example, portable Foundation byte-header example, and Windvale-written `wvdump` envelope core.

## Evidence discipline

A Windows pass proves Windows behavior and produces one side of the cross-host contract. A Linux pass proves Linux behavior and supplies the second side. Portable source and a cross-platform target framework are supporting evidence, not substitutes for actually running both reports.
