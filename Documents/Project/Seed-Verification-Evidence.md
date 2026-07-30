# Windvale Seed verification evidence

- Evidence date: 2026-07-29
- Milestone: Windvale Seed
- Qualified hosts: Windows x64 and Debian Linux x64
- Qualified commit: `1f4b48a`

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
| Foundation byte primitives | `u8`, `u32`, immutable `bytes`, zero-copy slice views, and bounded little-endian reads are type-checked, verified, inspected, executed, and covered by deterministic trap tests. |
| Immutable nominal records | Record declarations, positional construction, named field reads, nominal function signatures, canonical WVB schemas, verifier rejection cases, runtime values, and inspector output are exercised end to end. |
| Nominal enums and bounded formatting | Explicit enum declarations, exact nominal equality, enum-valued record fields, member naming, invariant `i32`/`u8`/`u32` formatting, bounded text concatenation, verifier rejection cases, and deterministic runtime output are exercised end to end. |
| Windvale-written inspection | `Wvˉdumpˉcore` reads an explicit real file through hosted resources while its pure functions walk the canonical seven-section envelope, return named status/count/offset, route deterministic summaries, reject hostile lengths safely, and retain embedded self-tests. |
| Representative programs | The portable sum returns `29`; the hosted example prints `Hello from Windvale` and returns `0`; the Foundation header example returns `1`; the Windvale-written section core passes its valid and adversarial fixtures and returns `0`. |
| Malformed-input coverage | Structured adversarial cases plus deterministic bounded random source and module input exercise diagnostic and rejection boundaries. |
| Cross-host coverage | The same suite passed on Windows and Debian Linux. Its normalized reports compare equal for module format, complete module hashes, results, and hosted output. |
| Scope control | No native backend, assembler, linker, self-hosted compiler, firmware, kernel, driver, or OS implementation is included. |

## Verified on the implementation host

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

The verifier performs a Release build, runs the conformance suite, produces a Windows report, publishes the CLI as framework-dependent `linux-x64`, and exercises the real CLI examples including unauthorized-capability refusal.

## Linux QA qualification

Commit `1f4b48a` was archived, transferred with matching SHA-256 `ee981fb4f0c001cfbef81cdca4a800a6a8e3a1263fce71672442c478cd988777`, and verified in a uniquely named disposable directory on the isolated E-Worker QA host. The host ran Debian GNU/Linux 12 x64 with .NET SDK `10.0.302`. The verification did not use E-Worker release, configuration, service, or durable-data paths, and the validated temporary directory and transferred archive were removed afterward.

```sh
Tools/Verify/Verify-Seed.sh
```

The Release build completed with zero warnings and errors. All 20 conformance tests passed, including bounded hosted arguments and files, separated output, support and authorization refusal, host-result validation, immutable nominal records, exact nominal enums, invariant formatting, Foundation byte reads, the Windvale-written section walker, deterministic compilation, malformed input rejection, verifier safety, runtime limits, bounded random input, and golden hashes. The real CLI flow passed for `compile`, `verify`, `inspect`, and `run` across all four examples. On both Windows and Debian, the hosted WvDump shell also read a generated module, diagnosed a non-WVB file separately, refused missing grants, and returned stable invalid-name and missing-file codes through the native adapter.

The Windows and Linux reports were then compared with the test runner:

```powershell
dotnet run --project Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj --configuration Release --no-build -- --compare-reports artifacts/seed-conformance-windows-x64.json artifacts/seed-conformance-linux-x86_64.json
```

The comparator confirmed equality between Microsoft Windows `10.0.26200` x64 and Debian GNU/Linux 12 x64 on .NET `10.0.10`. Both hosts produced:

- Module format: `1.3`.
- `Sum-Data.wvb`: `63ad39f6dbfff9b5ec31deb2d99d235dc59069a14a77033cf0a8284063578947`, result `29`.
- `Hello-Windvale.wvb`: `e113e56fef9bd108722fb8b16da93a42eec74699952d9055334c7ae0fe9db79b`, output `Hello from Windvale` plus LF, result `0`.
- `Read-Wvb-Header.wvb`: `66e3ec061c06428b3b6fb7f43c45386e1a34f68e4d93ffb0c2a046f2ecca2bed`, result `1`.
- `Wv-Dump-Core.wvb`: `666808a2266557c721f952dd6068b2493bd213da358fee3e20a5c3e7a545523e`, result `0`.

This qualifies Seed's current cross-host contract and completes roadmap Phase 3. The Phase 2 WVB 1.3 enum/formatting slice remains historically qualified at commit `e6c51c6`, the WVB 1.2 record slice at `a4b0f5d`, and the WVB 1.1 envelope-only slice at `60fd261`; none is a compatibility promise or supported obsolete input format. Future bytecode, compiler, verifier, runtime, host adapter, or contract changes must run the real native cases and regenerate both reports rather than inheriting this evidence automatically.
