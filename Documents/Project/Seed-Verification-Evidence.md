# Windvale Seed verification evidence

- Evidence date: 2026-07-29
- Milestone: Windvale Seed
- Qualified hosts: Windows x64 and Debian Linux x64
- Qualified commit: `a829fc8`

## Requirement evidence

| Requirement | Evidence |
| --- | --- |
| C# Stage 0 toolchain | Pinned .NET SDK, solution, compiler, bytecode, runtime, CLI, and test projects build with warnings treated as errors. |
| Source-to-bytecode path | `Sum-Data.wv`, `Hello-Windvale.wv`, `Read-Wvb-Header.wv`, and `Wv-Dump-Core.wv` compile through syntax, semantic WIR, lowering, canonical encoding, and mandatory verification. |
| Deterministic portable modules | Repeated compilation and codec round trips compare complete bytes; fixed SHA-256 identities are checked for all four examples. |
| `compile`, `inspect`, `verify`, and `run` | The Windows verifier invokes all four Release CLI paths against generated modules. |
| Reference runtime | Portable .NET interpreter executes only `Verifiedˉmodule`, with checked signed and unsigned arithmetic, array and byte-range bounds, step, call-depth, capability, and host-result controls. |
| Explicit capabilities | Hosted arguments, file-byte input, standard output, line output, and diagnostics must be declared, supported by the selected host, and granted separately; refusal and success are tested. |
| Hosted resource boundary | Arguments are immutable and UTF-8 bounded, native file reads stream into bounded immutable bytes, normal and diagnostic sinks are separate, line endings are Windvale-defined LF, and stable missing/invalid/oversized/bad-host cases are exercised. |
| Useful diagnostics | Lexer, parser, semantic, binary, verifier, and runtime failures expose stable codes; source errors retain one-based line and column. |
| Source conventions | U+02C9 identifiers and exported `Main` compile and execute; immutable `let` locals and parameters reject assignment; mutable `var` locals accept it; malformed and confusable separators are rejected. |
| Foundation binary and text primitives | `u8`, `u32`, immutable `bytes`, zero-copy slices, signed and unsigned little-endian reads, explicit byte widening, strict UTF-8 validation/decoding, and ASCII-safe quoting are type-checked, verified, inspected, executed, and covered by deterministic trap and size-limit tests. |
| Immutable nominal records | Record declarations, positional construction, named field reads, nominal function signatures, canonical WVB schemas, verifier rejection cases, runtime values, and inspector output are exercised end to end. |
| Nominal enums and bounded formatting | Explicit enum declarations, exact nominal equality, enum-valued record fields, member naming, invariant `i32`/`u8`/`u32` formatting, bounded text concatenation, verifier rejection cases, and deterministic runtime output are exercised end to end. |
| Windvale-written inspection | `Wvˉdumpˉcore` reads an explicit real file through hosted resources while pure Windvale functions validate all seven envelopes, decode every declaration payload and value shape, walk every instruction, reject malformed lengths/UTF-8/opcodes without escaping diagnostic boundaries, and emit a versioned ASCII-safe line report. |
| Representative programs | The portable sum returns `29`; the hosted example prints `Hello from Windvale` and returns `0`; the Foundation header example returns `1`; the Windvale-written inspector passes embedded adversarial fixtures, emits the complete golden Sum report, and returns `0`. |
| Malformed-input coverage | Structured adversarial cases plus deterministic bounded random source and module input exercise diagnostic and rejection boundaries. |
| Cross-host coverage | The same suite passed on Windows and Debian Linux. Its normalized contracts compare equal for WVB version, complete module hashes, results, hosted output, and every line of the Windvale-generated real-module report. |
| Scope control | No native backend, assembler, linker, self-hosted compiler, firmware, kernel, driver, or OS implementation is included. |

## Verified on the implementation host

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

The verifier performs a Release build, runs the conformance suite, produces a Windows report, publishes the CLI as framework-dependent `linux-x64`, and exercises the real CLI examples including unauthorized-capability refusal.

## Linux QA qualification

Commit `a829fc8` was archived, transferred with matching SHA-256 `a6dd2a25064d0b500f1d2542cf1612a7943d6e6d73c70f946df8fdf7b5597241`, and verified in the uniquely named disposable directory `/tmp/windvale-wvdump-a829fc8-20260729` on the isolated E-Worker QA host. The host ran Debian GNU/Linux 12 x64 with .NET SDK `10.0.302`. The verification did not use E-Worker release, configuration, service, or durable-data paths. The Debian report was retrieved with SHA-256 `39cc9cfb469b38e63e9d64486157a881a6ee26567f2dc0781ab269afcc80ee8b`, then the validated temporary directory and transferred archive were removed.

```sh
Tools/Verify/Verify-Seed.sh
```

The Release build completed with zero warnings and errors. All 21 conformance tests passed, including bounded hosted arguments and files, separated output, support and authorization refusal, host-result validation, immutable nominal records and enums, signed binary reads, strict UTF-8, safe quoting, Windvale-native payload/instruction decoding, malformed payload and opcode diagnostics, deterministic compilation, verifier safety, runtime limits, bounded random input, and golden hashes. The real CLI flow passed for `compile`, `verify`, `inspect`, and `run` across all four examples. On both Windows and Debian, the hosted WvDump shell read the generated Sum module through the native adapter and emitted the expected module, section, declaration, instruction, and export records; it also diagnosed non-WVB input separately, refused missing grants, and returned stable invalid-name and missing-file codes.

The Windows and Linux reports were then compared with the test runner:

```powershell
dotnet run --project Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj --configuration Release --no-build -- --compare-reports artifacts/seed-conformance-windows-x64.json artifacts/seed-conformance-debian-x86_64-a829fc8.json
```

The comparator confirmed equality between Microsoft Windows `10.0.26200` x64 and Debian GNU/Linux 12 x64 on .NET `10.0.10`. Both hosts produced:

- Module format: `1.4`.
- `Sum-Data.wvb`: `6a40e6172787ae294361b3a5d9abc92e7b3f004b1e59eabb999a7b844a21bf78`, result `29`.
- `Hello-Windvale.wvb`: `5b9101e15ae42acb333a8a05c60e6d6dbb548e5a04b9c96fdb717dbc58bf9cbe`, output `Hello from Windvale` plus LF, result `0`.
- `Read-Wvb-Header.wvb`: `26176eac5e2f00bb96a4b1ad95ad79238045932b64d8220edcfdea13af202c6a`, result `1`.
- `Wv-Dump-Core.wvb`: `74c5400120f01f8d4a3e0fa87c3bb20d2edd645208d8ccb930e994a416c497f1`, result `0`.
- The complete normalized hosted `wvdump` report for `Sum-Data.wvb`, beginning with `wvdump 1` and ending with the `Main` export record, was byte-for-byte equal.

This qualifies Seed's current cross-host contract and completes roadmap Phase 4. Phase 3 remains historically qualified at `1f4b48a`, the Phase 2 WVB 1.3 enum/formatting slice at `e6c51c6`, the WVB 1.2 record slice at `a4b0f5d`, and the WVB 1.1 envelope-only slice at `60fd261`; none is a compatibility promise or supported obsolete input format. Future bytecode, compiler, verifier, runtime, host adapter, or report-contract changes must run the real native cases and regenerate both reports rather than inheriting this evidence automatically.
