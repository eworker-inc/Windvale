# Windvale Seed verification evidence

- Evidence date: 2026-07-29
- Milestone: Windvale Seed
- Implementation host: Windows x64

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
| Representative programs | The portable example returns `29` from immutable code and data; the hosted example prints `Hello from Windvale` and returns `0`. |
| Malformed-input coverage | Structured adversarial cases plus deterministic bounded random source and module input exercise diagnostic and rejection boundaries. |
| Cross-host coverage | One suite emits normalized contract reports on Windows or Linux, and the comparator requires one report from each family before claiming equality. |
| Scope control | No native backend, assembler, linker, self-hosted compiler, firmware, kernel, driver, or OS implementation is included. |

## Verified on the implementation host

```powershell
pwsh -NoProfile -File Tools/Verify/Verify-Seed.ps1
```

The verifier performs a Release build, runs the conformance suite, produces a Windows report, publishes the CLI as framework-dependent `linux-x64`, and exercises the real CLI examples including unauthorized-capability refusal.

The POSIX verifier also passed end to end under Git for Windows Bash with an explicitly Windows-named report. This validates its shell flow without misrepresenting that run as Linux execution.

## Host evidence boundary

This development host has no WSL or container runtime, so an actual Linux conformance report was not collected here. The Linux verifier and framework-dependent Linux publication are present, but cross-host behavioral equality must not be claimed until `Verify-Seed.sh` runs on Linux and its report compares successfully with the Windows report.

This boundary concerns platform qualification evidence. It does not introduce a second runtime implementation or an unresolved Seed code path.
