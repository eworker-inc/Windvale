# Decision 0043: Compiler implementation role layout

- Date: 2026-07-31
- Status: Qualified at `4fdc6bfcb864251ab18584933319abe837703612`

## Context

The Windvale-written compiler had grown from its first lexer into a complete bounded frontend, typed WVIR producer, and multi-module WVB backend, but its source still lived under `Compiler/Bootstrap`. The independent C# implementation lived under `Compiler/Windvale.Compiler`. Those names mixed lifecycle status with implementation identity: the Windvale code was already a compiler even though self-hosting remained incomplete, while the C# project was intended to remain an explicit reference and recovery oracle.

Leaving that ambiguity until self-hosting would make provenance and future Stage 0/Stage 1/Stage 2 evidence harder to explain. Renaming the public C# namespace or assembly, however, would create unrelated API churn and would not improve the repository boundary.

## Decision

- `Compiler/Windvale` owns the compiler implementation written in Windvale.
- `Compiler/Reference` owns the independent C# Stage 0 reference/recovery compiler.
- The C# project, assembly, and namespace remain `Windvale.Compiler`; their capability did not change.
- “Bootstrap” remains the name of the staged recovery and self-hosting process, its prerequisites, provenance, and evidence. It is not the product name of either implementation.
- Naming the Windvale implementation as a compiler does not claim that it is self-hosting. That status still requires the separate reproducible self-compilation gate.

Every solution entry, project reference, embedded source resource, verifier path, hosted source name, example command, verification-routing fixture, specification, and current architecture document moves with the owning implementation. Historical decision titles and evidence paths remain unchanged because they describe the repository state that was actually qualified at those commits.

## Consequences

The repository now communicates implementation identity directly, and future self-hosting evidence can distinguish the Windvale compiler from the reference compiler without inventing a second semantic architecture. Nineteen implementation files moved byte for byte; the C# project file changed only its descriptive metadata. Canonical WVB, WVO, flat-image, map, diagnostic, runtime, and report contracts remain unchanged.

The layout may be reconsidered if a future compiler split gains a real contract boundary or another independently maintained reference implementation appears. A cosmetic preference alone is not enough to introduce parallel compiler trees or rename the public C# API.

## Verification

Exact commit `4fdc6bfcb864251ab18584933319abe837703612`, tree `8637171764f2c70c290fa547fd9aba0bb8c474cf`, passed the focused 24-test compiler area, the complete Standard suite, and full Windows and Debian Qualification. Both hosts completed zero-warning Release builds and all 48 tests, their normalized contracts matched, and all 61 portable artifacts were byte-identical. The complete evidence is recorded in [Seed verification evidence](../Project/Seed-Verification-Evidence.md#compiler-implementation-role-layout-qualification).
