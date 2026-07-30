# Windvale Seed verification evidence

- Evidence date: 2026-07-30
- Milestone: Windvale Seed
- Qualified hosts: Windows x64 and Debian Linux x64
- Qualified commit: `e5fd109`

## Requirement evidence

| Requirement | Evidence |
| --- | --- |
| C# Stage 0 toolchain | Pinned .NET SDK, solution, compiler, bytecode, runtime, object-model oracle, WVA assembler, CLI, and test projects build with warnings treated as errors. |
| Source-to-bytecode path | `Sum-Data.wv`, `Hello-Windvale.wv`, `Read-Wvb-Header.wv`, `Wv-Dump-Core.wv`, `Wvo-Object-Core.wv`, and `Wva-Scanner-Core.wv` compile through syntax, semantic WIR, lowering, canonical encoding, and mandatory verification. |
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
| Windvale-written WVA scanning | `Wvaˉscannerˉcore` validates source size, strict UTF-8, physical-line bytes, LF/CRLF/CR, spaces, tabs, comments, word boundaries, and the exact WVA 1 header over immutable bytes without host text parsing; the hosted shell reads only one explicit resource and emits a stable ASCII report. |
| Representative programs | The portable sum returns `29`; the hosted example prints `Hello from Windvale`; the Foundation header returns `1`; WvDump emits the complete golden Sum report; the WVO core writes its exact object; `Wvaˉscannerˉcore` recognizes the real assembly source; and `Hello-Object.wva` assembles into exact code/data, three symbols, and two relocations. |
| Malformed-input coverage | Structured adversarial cases plus deterministic bounded random Windvale source, WVA source, module, and object input exercise diagnostic and rejection boundaries. The Windvale scanner additionally covers malformed UTF-8, wrong/extra/missing headers, exact and exceeded line bounds, the source-size bound, comments, tabs, and all accepted line endings. |
| Cross-host coverage | The same 27-test suite passed on Windows and Debian Linux. Its normalized contracts compare equal for WVB/WVO/WVA versions, all module and object hashes, results, hosted output, every WvDump and WVA scan report line, both Windvale-written modules, and assembled WVO bytes. |
| Scope control | The Windvale scanner is lexical only and the Stage 0 WVA assembler emits objects only. No Windvale-written WVA semantic inspector or encoder, linker, native backend, executable image, self-hosted compiler, firmware, kernel, driver, or OS implementation is included. |

## Verified on the implementation host

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

The verifier performs a Release build, runs the 27-test conformance suite, produces a Windows report, publishes the CLI as framework-dependent `linux-x64`, and exercises the real module, WVA scan, assembly, and object CLI paths including unauthorized-capability refusal and native file writer failures. The Windows report SHA-256 was `eb9ce2b7af7dab46f70a5ddcb30109eec87fb813ba2392e9ddd4edbee3e25b00`.

## Linux QA qualification

Commit `e5fd109` was archived and transferred with matching SHA-256 `29da4728d80ebd8c920f2246821a0ebbc2b4012527dfdbd2f7ec01c668090a37`, then verified in the uniquely named disposable directory `/tmp/windvale-wva-scanner-e5fd109-20260730` on the isolated E-Worker QA host. The host ran Debian GNU/Linux 12 x64 with .NET SDK `10.0.302`. The verification did not use E-Worker release, configuration, service, or durable-data paths. The Debian report was retrieved with SHA-256 `1472dbdeebb79e81ffa5ed505f3194af6e231a0c9ffffeb46ac32df269281efb`; the scanner module was retrieved separately and matched the Windows module byte for byte; then the resolved exact temporary directory and transferred archive were removed.

An initial QA attempt at `57f2544` exposed that `.wva` lacked an explicit archive line-ending attribute: the Windows working file was LF while the exported Debian fixture was CRLF, producing semantically equal scans but different byte-count reports. Commit `e5fd109` added `*.wva text eol=lf`; its archive lists the canonical fixture as 403 bytes, and the complete qualification was rerun from that replacement commit. This was treated as a reproducibility defect rather than normalizing away the evidence.

```sh
Tools/Verify/Verify-Seed.sh
```

The Release build completed with zero warnings and errors. All 27 conformance tests passed, including bounded hosted resources, immutable values, strict UTF-8, WvDump decoding, WVO validation, the complete WVA 1 instruction/data subset, Windvale-written WVA scanning, deterministic assembly, stable assembly diagnostics, runtime limits, bounded random input, and golden hashes. The real CLI flow compiled and verified `Wvaˉscannerˉcore`, refused its ungranted capabilities, ran its embedded adversarial checks, scanned `Hello-Object.wva`, assembled that source, verified and inspected its exact WVO, and preserved every prior module/object/native-host case on both systems.

The Windows and Linux reports were then compared with the test runner:

```powershell
dotnet run --project Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj --configuration Release --no-build -- --compare-reports artifacts/seed-conformance-windows-x64.json artifacts/seed-conformance-debian-x86_64-e5fd109.json
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
- `Wva-Scanner-Core.wvb`: `6c0bd3e6a203debb5779f0c0934d9b751921de6a1ac21aaa9744f1afcb97004d`, result `0`; the directly retrieved Windows and Debian modules were byte-for-byte equal.
- Hosted scan report: `wvascan 1` followed by `status=valid bytes=403 lines=21 meaningful-lines=17 tokens=52 offset=403 line=22 column=1` on both hosts.

This qualifies roadmap gate 6B, the Windvale-written WVA source scanner, and advances Phase 6 without completing it. The WVA 1 Stage 0 oracle remains qualified at `3bfc6bb`, Phase 5 at `f87a5fa`, Phase 4 at `a829fc8`, Phase 3 at `1f4b48a`, the Phase 2 WVB 1.3 slice at `e6c51c6`, WVB 1.2 at `a4b0f5d`, and WVB 1.1 at `60fd261`; none is a compatibility promise or supported obsolete input format. The next Phase 6 gate is a Windvale-written semantic inspector whose accepted and rejected classifications agree with Stage 0, followed by exact WVO encoding and the separately owned linker. Future bytecode, assembly, object, compiler, verifier, runtime, host-adapter, or report-contract changes must regenerate both native reports rather than inheriting this evidence automatically.
