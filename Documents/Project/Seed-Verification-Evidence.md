# Windvale Seed verification evidence

- Evidence date: 2026-07-29
- Milestone: Windvale Seed
- Qualified hosts: Windows x64 and Debian Linux x64
- Qualified commit: `abb7630`

## Requirement evidence

| Requirement | Evidence |
| --- | --- |
| C# Stage 0 toolchain | Pinned .NET SDK, solution, compiler, bytecode, runtime, CLI, and test projects build with warnings treated as errors. |
| Source-to-bytecode path | `Sum-Data.wv` and `Hello-Windvale.wv` compile through syntax, semantic WIR, lowering, canonical encoding, and mandatory verification. |
| Deterministic portable modules | Repeated compilation and codec round trips compare complete bytes; fixed SHA-256 identities are checked for both examples. |
| `compile`, `inspect`, `verify`, and `run` | The Windows verifier invokes all four Release CLI paths against generated modules. |
| Reference runtime | Portable .NET interpreter executes only `Verifiedˉmodule`, with checked arithmetic, bounds, step, call-depth, and capability controls. |
| Explicit capabilities | Hosted output must be declared in the module and granted separately with `--allow console.write_line`; refusal and success are tested. |
| Useful diagnostics | Lexer, parser, semantic, binary, verifier, and runtime failures expose stable codes; source errors retain one-based line and column. |
| Source conventions | U+02C9 identifiers and exported `Main` compile and execute; immutable `let` locals and parameters reject assignment; mutable `var` locals accept it; malformed and confusable separators are rejected. |
| Representative programs | The portable example returns `29` from immutable code and data; the hosted example prints `Hello from Windvale` and returns `0`. |
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

Commit `abb7630` was archived, transferred with a matching SHA-256, and verified in a disposable directory on the isolated E-Worker QA host. The host ran Debian GNU/Linux 12 x64 with .NET SDK `10.0.302`. The verification did not use E-Worker release, configuration, service, or durable-data paths, and the temporary directory was removed afterward.

```sh
Tools/Verify/Verify-Seed.sh
```

The Release build completed with zero warnings and errors. All 15 conformance tests passed, including source naming and mutation rules, deterministic compilation, malformed source and module rejection, verifier safety, runtime traps and limits, capability authorization, bounded random input, and golden contract hashes. The real CLI flow also passed for `compile`, `verify`, `inspect`, and `run`.

The Windows and Linux reports were then compared with the test runner:

```powershell
dotnet run --project Tests/Windvale.Seed.Tests/Windvale.Seed.Tests.csproj --configuration Release --no-build -- --compare-reports artifacts/seed-conformance-windows-x64.json artifacts/seed-conformance-linux-x86_64.json
```

The comparator confirmed equality between Microsoft Windows `10.0.26200` x64 and Debian GNU/Linux 12 x64 on .NET `10.0.10`. Both hosts produced:

- `Sum-Data.wvb`: `faf44208d41c852f575e4f3025b0722c8fe6ee2d1c1a55b71b9e109c3eb54ef2`, result `29`.
- `Hello-Windvale.wvb`: `fafbc14e7e82626bcfacf358f777c1b6ce6821a335677a35148da9f857eefed5`, output `Hello from Windvale` plus LF, result `0`.

This qualifies Seed's current cross-host contract. Future bytecode, compiler, verifier, runtime, or contract changes must regenerate and compare both reports rather than inheriting this evidence automatically.
