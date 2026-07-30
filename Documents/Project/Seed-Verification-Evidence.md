# Windvale Seed verification evidence

- Evidence date: 2026-07-29
- Milestone: Windvale Seed
- Qualified hosts: Windows x64 and Debian Linux x64
- Qualified commit: `3f79530`

## Requirement evidence

| Requirement | Evidence |
| --- | --- |
| C# Stage 0 toolchain | Pinned .NET SDK, solution, compiler, bytecode, runtime, CLI, and test projects build with warnings treated as errors. |
| Source-to-bytecode path | `Sum-Data.wv`, `Hello-Windvale.wv`, and `Read-Wvb-Header.wv` compile through syntax, semantic WIR, lowering, canonical encoding, and mandatory verification. |
| Deterministic portable modules | Repeated compilation and codec round trips compare complete bytes; fixed SHA-256 identities are checked for all three examples. |
| `compile`, `inspect`, `verify`, and `run` | The Windows verifier invokes all four Release CLI paths against generated modules. |
| Reference runtime | Portable .NET interpreter executes only `Verifiedˉmodule`, with checked signed and unsigned arithmetic, array and byte-range bounds, step, call-depth, and capability controls. |
| Explicit capabilities | Hosted output must be declared in the module and granted separately with `--allow console.write_line`; refusal and success are tested. |
| Useful diagnostics | Lexer, parser, semantic, binary, verifier, and runtime failures expose stable codes; source errors retain one-based line and column. |
| Source conventions | U+02C9 identifiers and exported `Main` compile and execute; immutable `let` locals and parameters reject assignment; mutable `var` locals accept it; malformed and confusable separators are rejected. |
| Foundation byte primitives | `u8`, `u32`, immutable `bytes`, zero-copy slice views, and bounded little-endian reads are type-checked, verified, inspected, executed, and covered by deterministic trap tests. |
| Representative programs | The portable sum returns `29`; the hosted example prints `Hello from Windvale` and returns `0`; the portable Foundation example validates a static `.wvb` header and returns `1`. |
| Malformed-input coverage | Structured adversarial cases plus deterministic bounded random source and module input exercise diagnostic and rejection boundaries. |
| Cross-host coverage | The same suite passed on Windows and Debian Linux. Its normalized reports compare equal for module format, complete module hashes, results, and hosted output. |
| Scope control | No native backend, assembler, linker, self-hosted compiler, firmware, kernel, driver, or OS implementation is included. |

## Verified on the implementation host

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

The verifier performs a Release build, runs the conformance suite, produces a Windows report, publishes the CLI as framework-dependent `linux-x64`, and exercises the real CLI examples including unauthorized-capability refusal.

The POSIX verifier also passed end to end under Git for Windows Bash with an explicitly Windows-named report. This validates its shell flow without misrepresenting that run as Linux execution.

## Linux QA qualification

Commit `3f79530` was archived, transferred with matching SHA-256 `73b59dedc97c40d63f1453c396eaf0c7853aa3514089f63ac768214d4526118b`, and verified in a uniquely named disposable directory on the isolated E-Worker QA host. The host ran Debian GNU/Linux 12 x64 with .NET SDK `10.0.302`. The verification did not use E-Worker release, configuration, service, or durable-data paths, and the validated temporary directory was removed afterward.

```sh
Tools/Verify/Verify-Seed.sh
```

The Release build completed with zero warnings and errors. All 16 conformance tests passed, including source naming and mutation rules, Foundation byte values and reads, deterministic compilation, malformed source and module rejection, verifier safety, runtime traps and limits, capability authorization, bounded random input, and golden contract hashes. The real CLI flow also passed for `compile`, `verify`, `inspect`, and `run` across all three examples.

The Windows and Linux reports were then compared with the test runner:

```powershell
dotnet run --project Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj --configuration Release --no-build -- --compare-reports artifacts/seed-conformance-windows-x64.json artifacts/seed-conformance-linux-x86_64.json
```

The comparator confirmed equality between Microsoft Windows `10.0.26200` x64 and Debian GNU/Linux 12 x64 on .NET `10.0.10`. Both hosts produced:

- Module format: `1.1`.
- `Sum-Data.wvb`: `4570d02bc558a5e5d4e341cd9a0edcec733c7fe6d797bf371669305169ef386f`, result `29`.
- `Hello-Windvale.wvb`: `79185b8c138e2f7d6dc34cbdcf82a8a467601c7ae6383bb76305e4d57e4e8a62`, output `Hello from Windvale` plus LF, result `0`.
- `Read-Wvb-Header.wvb`: `72cb8f2af8aa7813d76e528973476147f12b4c548c114b7276ccc99f92b1c48a`, result `1`.

This qualifies Seed's current cross-host contract. Future bytecode, compiler, verifier, runtime, or contract changes must regenerate and compare both reports rather than inheriting this evidence automatically.
