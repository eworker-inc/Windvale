# Windvale Seed verification evidence

- Evidence date: 2026-07-30
- Milestone: Windvale Seed
- Qualified hosts: Windows x64 and Debian Linux x64
- Qualified commit: `a689617`

## Requirement evidence

| Requirement | Evidence |
| --- | --- |
| C# Stage 0 toolchain | Pinned .NET SDK, solution, compiler, bytecode, runtime, object-model oracle, WVA assembler, CLI, and test projects build with warnings treated as errors. |
| Source-to-bytecode path | `Sum-Data.wv`, `Hello-Windvale.wv`, `Read-Wvb-Header.wv`, `Wv-Dump-Core.wv`, `Wvo-Object-Core.wv`, and `Wva-Assembler-Core.wv` compile through syntax, semantic WIR, lowering, canonical encoding, and mandatory verification. |
| Deterministic portable modules | Repeated compilation and codec round trips compare complete bytes; fixed SHA-256 identities are checked for all six Windvale examples. |
| Module, assembly, and object CLI | The native verifiers invoke `compile`, `assemble`, `inspect`, `verify`, `run`, `object-inspect`, and `object-verify` against generated artifacts. |
| Reference runtime | Portable .NET interpreter executes only `Verifiedˉmodule`, with checked signed and unsigned arithmetic, array and byte-range bounds, step, call-depth, capability, and host-result controls. |
| Explicit capabilities | Hosted arguments, file-byte input and output, standard output, line output, and diagnostics must be declared, supported by the selected host, and granted separately; refusal and success are tested. |
| Hosted resource boundary | Arguments are immutable and UTF-8 bounded, native file reads stream into bounded immutable bytes, file writes replace one bounded whole value and durably flush, normal and diagnostic sinks are separate, and stable missing/invalid/oversized/bad-host cases are exercised. |
| Useful diagnostics | Language compiler, assembly parser/semantics, binary readers, verifiers, and runtime failures expose stable codes; source errors retain one-based line and column. |
| Source conventions | U+02C9 identifiers and exported `Main` compile and execute; immutable `let` locals and parameters reject assignment; mutable `var` locals accept it; malformed and confusable separators are rejected. |
| Foundation binary and text primitives | `u8`, `u32`, immutable `bytes`, zero-copy slices, immutable concatenation, signed and unsigned little-endian reads and fixed-width construction, explicit byte widening, strict UTF-8 validation/encoding/decoding, and ASCII-safe quoting are type-checked, verified, inspected, executed, and covered by deterministic trap and size-limit tests. |
| Immutable nominal records | Record declarations, positional construction, named field reads, nominal function signatures, canonical WVB schemas, verifier rejection cases, runtime values, and inspector output are exercised end to end. |
| Nominal enums and bounded formatting | Explicit enum declarations, exact nominal equality, enum-valued record fields, member naming, invariant `i32`/`u8`/`u32` formatting, bounded text concatenation, verifier rejection cases, and deterministic runtime output are exercised end to end. |
| Windvale-written inspection | `Wvˉdumpˉcore` reads an explicit real file through hosted resources while pure Windvale functions validate all seven envelopes, decode every declaration payload and value shape, walk every instruction, reject malformed lengths/UTF-8/opcodes without escaping diagnostic boundaries, and emit a versioned ASCII-safe line report. |
| WVO 1.0 object model | The C# oracle canonically encodes and strictly verifies x86-64-first sections, symbols, imports/exports, ranges, machine names, zero relocation placeholders, and non-overlapping `absolute-u32`/`relative-i32` relocations. |
| Windvale-written object production | `Wvoˉobjectˉcore` constructs and structurally validates a 189-byte representative object in pure Windvale, persists it only through `file.write_bytes`, and matches the C# oracle byte for byte. |
| WVA 1 Stage 0 assembler | The bounded parser enforces canonical declarations, definition ownership, contexts, integer widths, and references; the x86-64 encoder covers the complete initial code/data subset, derives ranges, creates both relocation kinds, and returns only independently verified WVO bytes. |
| Windvale-written WVA assembler | `Wvaˉassemblerˉcore` validates source size, strict UTF-8, physical-line bytes, line endings, token boundaries, the complete grammar, declarations, definitions, integer widths, contexts, limits, ownership, and references over immutable bytes without host text parsing. It measures the complete result, derives definition ranges, encodes every WVA 1 instruction/data statement, and constructs canonical WVO sections, symbols, and relocations. |
| Representative programs | The portable sum returns `29`; the hosted example prints `Hello from Windvale`; the Foundation header returns `1`; WvDump emits the complete golden Sum report; the WVO core writes its exact object; and the hosted Windvale assembler writes the 218-byte canonical `Hello-Object.wvo` with two sections, three symbols, and two relocations. |
| Malformed-input coverage | Structured adversarial cases plus deterministic bounded random Windvale source, WVA source, module, and object input exercise diagnostic and rejection boundaries. The Windvale assembler covers malformed UTF-8, wrong/extra/missing headers, exact and exceeded line/source bounds, all accepted line endings, all eleven WVA diagnostic families, cross-binding/cross-kind duplicates, invalid ownership/contexts, missing/wrong-kind references, unclosed structures, integer overflows, aggregate-size overflow, and 200 deterministic source mutations compared with Stage 0 acceptance and complete object bytes. Rejected inputs invoke the hosted writer zero times. |
| Cross-host coverage | The same 28-test suite passed on Windows and Debian Linux. Its normalized contracts compare equal for WVB/WVO/WVA versions, all module and object hashes, results, hosted output, every WvDump and WVA assembler report line, both Windvale-written binary modules, and complete assembled WVO bytes. |
| Scope control | The Windvale WVA assembler emits canonical single-object WVO values but does not resolve multiple objects, apply relocations, select final addresses, or produce an image. No Windvale-written linker, native backend, executable image, self-hosted compiler, firmware, kernel, driver, or OS implementation is included. |

## Verified on the implementation host

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

The verifier performs a Release build, runs the 28-test conformance suite, produces a Windows report, publishes the CLI as framework-dependent `linux-x64`, and exercises the real module, hosted Windvale assembly, Stage 0 assembly, and object CLI paths. It checks unauthorized-capability refusal, byte-for-byte Stage 0 equality, independent WVO verification, zero writer calls for rejected source, no file for rejected source or a missing parent, and preservation of an existing object when validation fails. The Windows report SHA-256 was `06f302323d3b64a7fcd204e2ebf385147bd54365b904876d35da59746411d733`.

## Linux QA qualification

Commit `a689617` was archived and transferred with matching SHA-256 `2d4dc945b6d7f4454575ee5f4219397b7fae483999b20c4e611f05c9fbca4567`, then verified in the uniquely named disposable directory `/tmp/windvale-wva-assembler-a689617-20260730` on the isolated E-Worker QA host. The archive contained the canonical WVA fixture as 403 bytes. The host ran Debian GNU/Linux 12 x64 with .NET SDK `10.0.302`. The verification did not use E-Worker release, configuration, service, or durable-data paths. The Debian report was retrieved with SHA-256 `bb6025ae6d15a7e11057ab1ccc24a5196b7bcff85e8640b9b07e7a15a4338f56`; the assembler module and its hosted WVO output were retrieved separately and each matched the Windows artifact byte for byte; then the resolved exact temporary directory and transferred archive were removed.

An initial QA attempt at `57f2544` exposed that `.wva` lacked an explicit archive line-ending attribute: the Windows working file was LF while the exported Debian fixture was CRLF, producing semantically equal scans but different byte-count reports. Commit `e5fd109` added `*.wva text eol=lf`; its archive lists the canonical fixture as 403 bytes, and the complete qualification was rerun from that replacement commit. This was treated as a reproducibility defect rather than normalizing away the evidence.

```sh
Tools/Verify/Verify-Seed.sh
```

The Release build completed with zero warnings and errors. All 28 conformance tests passed, including bounded hosted resources, immutable values, strict UTF-8, WvDump decoding, WVO validation, the complete WVA 1 instruction/data subset, Windvale-written WVA scanning, semantics, and object encoding, deterministic assembly, stable diagnostics, runtime limits, bounded random input, and golden hashes. The assembler test exercises exact numeric boundaries, all eight move-register encodings, multiple definition ranges, empty objects, every WVA diagnostic family, targeted cross-pass failures, and 200 deterministic character mutations against Stage 0 acceptance and complete output bytes. The real CLI flow compiled and verified `Wvaˉassemblerˉcore`, refused its ungranted capabilities, ran its embedded adversarial checks, assembled `Hello-Object.wva` through Windvale, rejected non-WVA source without creating or changing output, surfaced a native missing-parent failure without output, independently verified and inspected the exact WVO, compared it directly with Stage 0, and preserved every prior module/object/native-host case on both systems.

The Windows and Linux reports were then compared with the test runner:

```powershell
dotnet run --project Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj --configuration Release --no-build -- --compare-reports artifacts/seed-conformance-windows-x64.json artifacts/seed-conformance-debian-x86_64-a689617.json
```

The comparator confirmed equality between Microsoft Windows `10.0.26200` x64 and Debian GNU/Linux 12 x64 on .NET `10.0.10`. Both hosts produced:

- Module format `1.5`, object format `1.0`, and assembly format `1`.
- `Sum-Data.wvb`: `64134dfd779b353c5e501c9c23337a0c3849bfef2c97a63a07913705b0f10c6b`, result `29`.
- `Hello-Windvale.wvb`: `43d565c304cf2e2f5d886ee30b1fabf0b2fbfb0c8cd28bd932d85d5add0bf504`, output `Hello from Windvale` plus LF, result `0`.
- `Read-Wvb-Header.wvb`: `0cdf05f6c9e1fb1db0d5ab449207870b5e47cc248f187cd43cd9a5c3c9eee995`, result `1`.
- `Wv-Dump-Core.wvb`: `2957fc5523ae3ca16cf1aaeb9104c14a3342a0aefde9ac591bb689f744f1467f`, result `0`, with the complete hosted Sum report equal.
- `Wvo-Object-Core.wvb`: `a5d574ea646946b159d95bd7e51434bfcbf7545083a54541438a79a2e5e999df`, result `0`, output `Wrote WVO 1.0 bytes=189` plus LF.
- `Sample.wvo`: `006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a`; the directly retrieved Windows and Debian files were byte-for-byte equal.
- Assembled `Hello-Object.wvo`: `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85`; the directly retrieved Windows and Debian CLI outputs were byte-for-byte equal.
- `Wva-Assembler-Core.wvb`: `7dbcf042f011adab5a04670973fc17b6b63d50fb08c09e8e54c3a4adb2c00825`, result `0`; the directly retrieved Windows and Debian modules were byte-for-byte equal.
- Hosted assembler object: `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85`; the directly retrieved Windvale-written Windows and Debian objects were byte-for-byte equal to each other and to Stage 0.
- Hosted assembler report: `wvasm 1`, then `assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1` on both hosts.

This qualifies roadmap gates 6D and 6E: the complete Windvale-written WVA object encoder and its hosted assembler shell. The semantic inspector remains qualified at `cc57bf9`, the lexical scanner at `e5fd109`, the WVA 1 Stage 0 oracle at `3bfc6bb`, Phase 5 at `f87a5fa`, Phase 4 at `a829fc8`, Phase 3 at `1f4b48a`, the Phase 2 WVB 1.3 slice at `e6c51c6`, WVB 1.2 at `a4b0f5d`, and WVB 1.1 at `60fd261`; none is a compatibility promise or supported obsolete input format. The next Phase 6 gate is a separately specified linker contract and Stage 0 oracle, followed by a Windvale-written linker. Future bytecode, assembly, object, compiler, verifier, runtime, host-adapter, or report-contract changes must regenerate both native reports rather than inheriting this evidence automatically.
