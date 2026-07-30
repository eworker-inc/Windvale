# Windvale Seed verification evidence

- Evidence date: 2026-07-29
- Milestone: Windvale Seed
- Qualified hosts: Windows x64 and Debian Linux x64
- Qualified commit: `f87a5fa`

## Requirement evidence

| Requirement | Evidence |
| --- | --- |
| C# Stage 0 toolchain | Pinned .NET SDK, solution, compiler, bytecode, runtime, object-model oracle, CLI, and test projects build with warnings treated as errors. |
| Source-to-bytecode path | `Sum-Data.wv`, `Hello-Windvale.wv`, `Read-Wvb-Header.wv`, `Wv-Dump-Core.wv`, and `Wvo-Object-Core.wv` compile through syntax, semantic WIR, lowering, canonical encoding, and mandatory verification. |
| Deterministic portable modules | Repeated compilation and codec round trips compare complete bytes; fixed SHA-256 identities are checked for all five examples. |
| Module and object CLI | The native verifiers invoke `compile`, `inspect`, `verify`, `run`, `object-inspect`, and `object-verify` against generated artifacts. |
| Reference runtime | Portable .NET interpreter executes only `Verifiedˉmodule`, with checked signed and unsigned arithmetic, array and byte-range bounds, step, call-depth, capability, and host-result controls. |
| Explicit capabilities | Hosted arguments, file-byte input and output, standard output, line output, and diagnostics must be declared, supported by the selected host, and granted separately; refusal and success are tested. |
| Hosted resource boundary | Arguments are immutable and UTF-8 bounded, native file reads stream into bounded immutable bytes, file writes replace one bounded whole value and durably flush, normal and diagnostic sinks are separate, and stable missing/invalid/oversized/bad-host cases are exercised. |
| Useful diagnostics | Lexer, parser, semantic, binary, verifier, and runtime failures expose stable codes; source errors retain one-based line and column. |
| Source conventions | U+02C9 identifiers and exported `Main` compile and execute; immutable `let` locals and parameters reject assignment; mutable `var` locals accept it; malformed and confusable separators are rejected. |
| Foundation binary and text primitives | `u8`, `u32`, immutable `bytes`, zero-copy slices, immutable concatenation, signed and unsigned little-endian reads and fixed-width construction, explicit byte widening, strict UTF-8 validation/encoding/decoding, and ASCII-safe quoting are type-checked, verified, inspected, executed, and covered by deterministic trap and size-limit tests. |
| Immutable nominal records | Record declarations, positional construction, named field reads, nominal function signatures, canonical WVB schemas, verifier rejection cases, runtime values, and inspector output are exercised end to end. |
| Nominal enums and bounded formatting | Explicit enum declarations, exact nominal equality, enum-valued record fields, member naming, invariant `i32`/`u8`/`u32` formatting, bounded text concatenation, verifier rejection cases, and deterministic runtime output are exercised end to end. |
| Windvale-written inspection | `Wvˉdumpˉcore` reads an explicit real file through hosted resources while pure Windvale functions validate all seven envelopes, decode every declaration payload and value shape, walk every instruction, reject malformed lengths/UTF-8/opcodes without escaping diagnostic boundaries, and emit a versioned ASCII-safe line report. |
| WVO 1.0 object model | The C# oracle canonically encodes and strictly verifies x86-64-first sections, symbols, imports/exports, ranges, machine names, zero relocation placeholders, and non-overlapping `absolute-u32`/`relative-i32` relocations. |
| Windvale-written object production | `Wvoˉobjectˉcore` constructs and structurally validates a 189-byte representative object in pure Windvale, persists it only through `file.write_bytes`, and matches the C# oracle byte for byte. |
| Representative programs | The portable sum returns `29`; the hosted example prints `Hello from Windvale`; the Foundation header returns `1`; WvDump emits the complete golden Sum report; and the WVO core writes the exact `.text`, `.rodata`, three-symbol, one-relocation object. |
| Malformed-input coverage | Structured adversarial cases plus deterministic bounded random source, module, and object input exercise diagnostic and rejection boundaries. |
| Cross-host coverage | The same 24-test suite passed on Windows and Debian Linux. Its normalized contracts compare equal for WVB/WVO versions, all module and object hashes, results, hosted output, every WvDump report line, and the exact Windvale-written WVO bytes. |
| Scope control | No native backend, assembler, linker, self-hosted compiler, firmware, kernel, driver, or OS implementation is included. |

## Verified on the implementation host

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

The verifier performs a Release build, runs the 24-test conformance suite, produces a Windows report, publishes the CLI as framework-dependent `linux-x64`, and exercises the real module and object CLI paths including unauthorized-capability refusal and native file writer failures. The Windows report SHA-256 was `6a409e9627bec9853b74558d105ca6b0b753a64bd94b87e10cb220a8eabf1e48`.

## Linux QA qualification

Commit `f87a5fa` was archived and transferred with matching SHA-256 `e3d9c394be4c8e50b1bcf85046b4778341bfdaa4974c99b376f4a3540e31182f`, then verified in the uniquely named disposable directory `/tmp/windvale-object-f87a5fa-20260729` on the isolated E-Worker QA host. The host ran Debian GNU/Linux 12 x64 with .NET SDK `10.0.302`. The verification did not use E-Worker release, configuration, service, or durable-data paths. The Debian report was retrieved with SHA-256 `37c43c5e25854cfafd1231388c64780fbda3f7b88f4da28b2ce833d9ce558298`; the emitted WVO object was retrieved separately; then the resolved exact temporary directory and transferred archive were removed.

```sh
Tools/Verify/Verify-Seed.sh
```

The Release build completed with zero warnings and errors. All 24 conformance tests passed, including bounded hosted reads and writes, separated output, support and authorization refusal, host-result validation, immutable nominal records and enums, byte construction, strict UTF-8, WvDump payload/instruction decoding, WVO canonical encoding and malformed-object rejection, deterministic compilation, runtime limits, bounded random input, and golden hashes. The real CLI flow passed across all five examples and both object commands. On both hosts, the WVO core refused missing grants, passed its no-write self-tests, wrote the representative object through the native adapter, returned stable invalid-name and missing-parent errors, and was accepted by the independent object verifier and inspector.

The Windows and Linux reports were then compared with the test runner:

```powershell
dotnet run --project Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj --configuration Release --no-build -- --compare-reports artifacts/seed-conformance-windows-x64.json artifacts/seed-conformance-debian-x86_64-f87a5fa.json
```

The comparator confirmed equality between Microsoft Windows `10.0.26200` x64 and Debian GNU/Linux 12 x64 on .NET `10.0.10`. Both hosts produced:

- Module format `1.5` and object format `1.0`.
- `Sum-Data.wvb`: `64134dfd779b353c5e501c9c23337a0c3849bfef2c97a63a07913705b0f10c6b`, result `29`.
- `Hello-Windvale.wvb`: `43d565c304cf2e2f5d886ee30b1fabf0b2fbfb0c8cd28bd932d85d5add0bf504`, output `Hello from Windvale` plus LF, result `0`.
- `Read-Wvb-Header.wvb`: `0cdf05f6c9e1fb1db0d5ab449207870b5e47cc248f187cd43cd9a5c3c9eee995`, result `1`.
- `Wv-Dump-Core.wvb`: `2957fc5523ae3ca16cf1aaeb9104c14a3342a0aefde9ac591bb689f744f1467f`, result `0`, with the complete hosted Sum report equal.
- `Wvo-Object-Core.wvb`: `a5d574ea646946b159d95bd7e51434bfcbf7545083a54541438a79a2e5e999df`, result `0`, output `Wrote WVO 1.0 bytes=189` plus LF.
- `Sample.wvo`: `006fd80183da7fbc71d3c6d63b65e6f3551765508fe9dba6f38ba80e002eb28a`; the directly retrieved Windows and Debian files were byte-for-byte equal.

This qualifies Seed's current cross-host contract and completes roadmap Phase 5. Phase 4 remains historically qualified at `a829fc8`, Phase 3 at `1f4b48a`, the Phase 2 WVB 1.3 enum/formatting slice at `e6c51c6`, the WVB 1.2 record slice at `a4b0f5d`, and the WVB 1.1 envelope-only slice at `60fd261`; none is a compatibility promise or supported obsolete input format. Future bytecode, object, compiler, verifier, runtime, host adapter, or report-contract changes must run the real native cases and regenerate both reports rather than inheriting this evidence automatically.
