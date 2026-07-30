# Windvale Seed verification evidence

- Evidence date: 2026-07-30
- Milestone: Windvale Seed
- Qualified hosts: Windows x64 and Debian Linux x64
- Qualified commit: `9c4b9f5`

## Requirement evidence

| Requirement | Evidence |
| --- | --- |
| C# Stage 0 toolchain | Pinned .NET SDK, solution, compiler, bytecode, runtime, object-model oracle, WVA assembler, linker oracle, CLI, and test projects build with warnings treated as errors. |
| Source-to-bytecode path | `Sum-Data.wv`, `Hello-Windvale.wv`, `Read-Wvb-Header.wv`, `Wv-Dump-Core.wv`, `Wvo-Object-Core.wv`, and `Wva-Assembler-Core.wv` compile through syntax, semantic WIR, lowering, canonical encoding, and mandatory verification. |
| Deterministic portable modules | Repeated compilation and codec round trips compare complete bytes; fixed SHA-256 identities are checked for all six Windvale examples. |
| Module, assembly, object, and link CLI | The native verifiers invoke `compile`, `assemble`, `link`, `inspect`, `verify`, `run`, `object-inspect`, and `object-verify` against generated artifacts. |
| Reference runtime | Portable .NET interpreter executes only `Verifiedˉmodule`, with checked signed and unsigned arithmetic, array and byte-range bounds, step, call-depth, capability, and host-result controls. |
| Explicit capabilities | Hosted arguments, file-byte input and output, standard output, line output, and diagnostics must be declared, supported by the selected host, and granted separately; refusal and success are tested. |
| Hosted resource boundary | Arguments are immutable and UTF-8 bounded, native file reads stream into bounded immutable bytes, file writes replace one bounded whole value and durably flush, normal and diagnostic sinks are separate, and stable missing/invalid/oversized/bad-host cases are exercised. |
| Useful diagnostics | Language compiler, assembly parser/semantics, binary readers, link resolution/layout/relocation, verifiers, and runtime failures expose stable codes; source errors retain one-based line and column. |
| Source conventions | U+02C9 identifiers and exported `Main` compile and execute; immutable `let` locals and parameters reject assignment; mutable `var` locals accept it; malformed and confusable separators are rejected. |
| Foundation binary and text primitives | `u8`, `u32`, immutable `bytes`, zero-copy slices, immutable concatenation, signed and unsigned little-endian reads and fixed-width construction, explicit byte widening, strict UTF-8 validation/encoding/decoding, and ASCII-safe quoting are type-checked, verified, inspected, executed, and covered by deterministic trap and size-limit tests. |
| Immutable nominal records | Record declarations, positional construction, named field reads, nominal function signatures, canonical WVB schemas, verifier rejection cases, runtime values, and inspector output are exercised end to end. |
| Nominal enums and bounded formatting | Explicit enum declarations, exact nominal equality, enum-valued record fields, member naming, invariant `i32`/`u8`/`u32` formatting, bounded text concatenation, verifier rejection cases, and deterministic runtime output are exercised end to end. |
| Windvale-written inspection | `Wvˉdumpˉcore` reads an explicit real file through hosted resources while pure Windvale functions validate all seven envelopes, decode every declaration payload and value shape, walk every instruction, reject malformed lengths/UTF-8/opcodes without escaping diagnostic boundaries, and emit a versioned ASCII-safe line report. |
| WVO 1.0 object model | The C# oracle canonically encodes and strictly verifies x86-64-first sections, symbols, imports/exports, ranges, machine names, zero relocation placeholders, and non-overlapping `absolute-u32`/`relative-i32` relocations. |
| Windvale-written object production | `Wvoˉobjectˉcore` constructs and structurally validates a 189-byte representative object in pure Windvale, persists it only through `file.write_bytes`, and matches the C# oracle byte for byte. |
| WVA 1 Stage 0 assembler | The bounded parser enforces canonical declarations, definition ownership, contexts, integer widths, and references; the x86-64 encoder covers the complete initial code/data subset, derives ranges, creates both relocation kinds, and returns only independently verified WVO bytes. |
| Windvale-written WVA assembler | `Wvaˉassemblerˉcore` validates source size, strict UTF-8, physical-line bytes, line endings, token boundaries, the complete grammar, declarations, definitions, integer widths, contexts, limits, ownership, and references over immutable bytes without host text parsing. It measures the complete result, derives definition ranges, encodes every WVA 1 instruction/data statement, and constructs canonical WVO sections, symbols, and relocations. |
| Stage 0 linker oracle | Windvale Linking 1 validates complete WVO inputs and aggregate limits, keeps locals object-private, resolves every import to one same-kind export, requires an exported-function entry, lays out actual addresses by section kind/input/source order, materializes zero padding and BSS, applies both relocation kinds with checked arithmetic, and independently reconstructs the complete image before returning bytes. |
| Canonical link evidence | The path-free ASCII/LF map records input digests, section placements, definitions, import providers, patch and target addresses, relocation values, entry, and image digest in deterministic order. It is byte-limited, locale-independent, and compared as a complete value across hosts. |
| Representative programs | The portable sum returns `29`; the hosted example prints `Hello from Windvale`; the Foundation header returns `1`; WvDump emits the complete golden Sum report; the WVO core writes its exact object; the hosted Windvale assembler writes the 218-byte canonical `Hello-Object.wvo`; and Stage 0 links it with the 91-byte `Console-Provider.wvo` into an independently verified 24-byte image and 1,721-byte map. |
| Malformed-input coverage | Structured adversarial cases plus deterministic bounded random Windvale source, WVA source, module, object, and link input exercise diagnostic and rejection boundaries. Link coverage includes invalid and oversized objects, invalid entry names, duplicate exports, undefined imports, kind mismatches, non-function entries, aggregate section limits, map limits, unaligned bases, all section kinds, BSS/padding zeros, image/u32 overflow, absolute and relative relocation overflow, 200 hostile objects, locale changes, semantic input reordering, and missing/unchanged output cases. |
| Cross-host coverage | The same 31-test suite passed on Windows and Debian Linux. Its normalized contracts compare equal for WVB/WVO/WVA/link versions, all module/object/image/map hashes, results, hosted output, complete WvDump/WVA/link reports, and exact assembled and linked bytes. |
| Scope control | The qualified Stage 0 linker produces one bounded raw flat memory image and canonical map. No Windvale-written linker, executable-container adapter, native backend, directly executable host program, self-hosted compiler, firmware, kernel, driver, or OS implementation is included. |

## Verified on the implementation host

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

The verifier performs a Release build, runs the 31-test conformance suite, produces a Windows report, publishes the CLI as framework-dependent `linux-x64`, and exercises the real module, Windvale/Stage 0 assembly, object, and Stage 0 link CLI paths. It checks capability refusal, byte-for-byte assembler equality, independent WVO and flat-image verification, exact map output, no file for rejected source/link or a missing parent, and preservation of existing object/image outputs when validation fails. The Windows report SHA-256 was `31e5c02c2a5ddd28b63201803bd733a5cb6356ec34593553503f2451b1781248`.

## Linux QA qualification

Commit `9c4b9f5` was archived and transferred with matching SHA-256 `44565d0f6366eb418dc1a4555d1f7b355df63ec2ea836440de54639087fc335a`, then verified in the uniquely named disposable directory `/tmp/windvale-linker-9c4b9f5-20260730` on the isolated E-Worker QA host. The archive contained the canonical WVA fixture as 403 bytes plus the linker project, provider fixture, and shell verifier. The host ran Debian GNU/Linux 12 x64 with .NET SDK `10.0.302`. The verification did not use E-Worker release, configuration, service, or durable-data paths. The Debian report was retrieved with SHA-256 `b84cd3a180e8cb167dca3cf909457de91a8643f19911628c426c6e52c6fe5fc0`; the provider WVO, linked image, and canonical map were retrieved separately and each matched the Windows artifact byte for byte; then the resolved exact temporary directory and transferred archive were removed.

An initial QA attempt at `57f2544` exposed that `.wva` lacked an explicit archive line-ending attribute: the Windows working file was LF while the exported Debian fixture was CRLF, producing semantically equal scans but different byte-count reports. Commit `e5fd109` added `*.wva text eol=lf`; its archive lists the canonical fixture as 403 bytes, and the complete qualification was rerun from that replacement commit. This was treated as a reproducibility defect rather than normalizing away the evidence.

```sh
Tools/Verify/Verify-Seed.sh
```

The Release build completed with zero warnings and errors. All 31 conformance tests passed, preserving every prior language, bytecode, runtime, WvDump, WVO, and Windvale-assembler case while adding complete Stage 0 linker coverage. The linker tests exercise actual-address alignment from aligned and unaligned bases, all section kinds, materialized BSS and padding, local/export/import resolution, both relocation kinds, semantic input order, locale independence, every WVL failure family reachable from external input, aggregate and map limits, relocation/address overflow, 200 deterministic hostile objects, full-image reconstruction, and exact image/map digests. The real CLI flow assembled both inputs, linked them, checked every essential map record and exact artifact digest, rejected an undefined import without output, preserved an existing image on rejection, surfaced a native missing-parent failure without output, and preserved every prior native-host case on both systems.

The Windows and Linux reports were then compared with the test runner:

```powershell
dotnet run --project Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj --configuration Release --no-build -- --compare-reports artifacts/seed-conformance-windows-x64.json artifacts/seed-conformance-debian-x86_64-9c4b9f5.json
```

The comparator confirmed equality between Microsoft Windows `10.0.26200` x64 and Debian GNU/Linux 12 x64 on .NET `10.0.10`. Both hosts produced:

- Module format `1.5`, object format `1.0`, assembly format `1`, and link format `1`.
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
- `Console-Provider.wvo`: `486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab`; the directly retrieved Windows and Debian objects were byte-for-byte equal.
- `Hello-Linked.bin`: 24 bytes, `0e02d447ec379e8bc8be373694d6ca14fdde0125550cbd34ee05b3ecc63ffe9a`; the directly retrieved Windows and Debian flat images were byte-for-byte equal.
- `Hello-Linked.wvmap`: 1,721 bytes, `31bc6a8e90d5f3049ae3e2eb0735a901923186d6a03ed40f22762b557b2ba5f4`; the directly retrieved Windows and Debian maps and the complete report-embedded maps were byte-for-byte equal.

This qualifies roadmap gate 6F: the separate Windvale Linking 1 contract and C# Stage 0 flat-image oracle. The Windvale assembler remains qualified at `a689617`, semantic inspector at `cc57bf9`, lexical scanner at `e5fd109`, WVA 1 Stage 0 oracle at `3bfc6bb`, Phase 5 at `f87a5fa`, Phase 4 at `a829fc8`, Phase 3 at `1f4b48a`, the Phase 2 WVB 1.3 slice at `e6c51c6`, WVB 1.2 at `a4b0f5d`, and WVB 1.1 at `60fd261`; none is a compatibility promise or supported obsolete input format. The next Phase 6 gate is the Windvale-written verified-bytecode linker producing exact oracle images and maps through explicit hosted boundaries. Future bytecode, assembly, object, link, compiler, verifier, runtime, host-adapter, or report-contract changes must regenerate both native reports rather than inheriting this evidence automatically.
