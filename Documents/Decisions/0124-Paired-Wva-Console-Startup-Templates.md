# Decision 0124: Paired WVA console startup templates

- Date: 2026-08-02
- Status: Implemented candidate; fresh dual-host qualification pending
- Targets: `windows-x64-console-v1` and `linux-x64-console-v1`
- Retains: Canonical WVB 1.6, native ABI 20/context 7, WVA 1, WVO 1.0, both container format versions, and the .NET retirement gate

## Context

Decisions 0119 and 0122 established deterministic import-free PE and ELF containers, but their exact process-entry code existed only as C# byte arrays. WVA 1 now has the register, local-control, RIP-relative address, scalar, syscall, and fixed-memory forms required to express both complete startups without an opaque executable-byte escape.

The production linker must not parse WVA or reference the assembler. That would collapse the assembler/linker boundary and create a bootstrap dependency in the Stage 0 recovery path. The transfer therefore needs independent assembly evidence while C# remains the executable constructor oracle until a later Windvale-written container core is qualified.

## Decision

- Add `Linker/Startup/Windows-X64-Console.wva` and `Linker/Startup/Linux-X64-Console.wva` as the exact Windvale-owned startup candidates.
- Express all executable instructions with ordinary WVA operations. Do not add macros, includes, final-image directives, or opaque code-byte statements.
- Declare four explicit imports per template: `Execution_context`, `Native_main`, `Record_arena`, and `Text_arena`.
- Require exactly four WVO `relative-i32` relocations with addend `-4`. The container remains responsible for mapping those semantic imports to final virtual addresses.
- Retain separately written C# byte templates in the Stage 0 PE and ELF adapters. They are reference/recovery encodings, not the semantic definition.
- Integrate the differential proof into the existing PE and ELF tests: assemble each WVA source once, validate its object shape and imports, instantiate its relocations at the real container addresses, and compare the complete startup bytes. Do not create another console-construction suite.
- Retain the independent untrusted-container verifiers. They continue to check instruction shapes and resolved targets without consuming either the WVA source or writer template.

The deterministic WVA encodings replace compact hand-selected instruction variants. The Windows startup is 98 bytes with relocation fields at offsets 10, 17, 32, and 54; the native image begins at byte 112. The Linux startup is 158 bytes with fields at 64, 74, 89, and 112; its native image begins at byte 160. Near branches and fixed `disp32` memory operands follow the existing WVA contract exactly.

## Local evidence

The C# assembler produces a 98-byte Windows code section in a WVO with SHA-256 `e27b79aa4f55554a89abddb08dc765b19f2b6cd484a1ef7ef3878990941fcce9` and a 158-byte Linux code section in a WVO with SHA-256 `e592d5d63048587f775ae2802aac3c40c747a62865f38059c094e7d3f95ed028`. Both objects contain one code section, one exported startup function, the four canonical imports, and the four exact typed relocation records.

The two existing console-executable tests pass together with a zero-warning Release build. The Windows case assembles and relocates the PE candidate, compares all 98 bytes, independently verifies the container, and directly executes the complete shared process-result corpus. The Linux case assembles and relocates the ELF candidate, compares all 158 bytes, and completes construction, malformed-input, and PE/ELF recovery parity on Windows; direct ELF execution remains reserved for the Linux host. Canonical `Sum-Data.wv` remains 5,120 PE bytes with SHA-256 `5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77` and 8,304 ELF bytes with SHA-256 `8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4`.

Windows Development completes a zero-warning Release build, all 74 regular Seed tests, and all 25 bounded OS tests in 97.9 seconds wall time on the rebased candidate. Seed takes 76.360 seconds; the WVA-backed Windows and Linux container cases take 799 and 24 milliseconds. The qualification-only golden contract and direct Linux execution are not part of Development, so fresh dual-host Qualification remains pending.

## Consequences

The process-entry machine layer is now readable and buildable in Windvale's own assembly language. Typed WVO records identify every final-image dependency, and a writer change cannot silently diverge from the WVA source while the existing tests pass.

Normal executable construction remains Stage 0. The portable PE/ELF layout planners, byte constructors, and untrusted-input verifiers still need `.wv` implementations and differential malformed-input evidence before normal ownership can move away from C#.

The larger goal still requires fresh Windows/Linux qualification, native ABI 21 record storage, complete native Stage 1-to-Stage 2 reproduction, and an explicitly serialized hosted-console capability. This decision claims none of those later gates.

## Reconsider when

- A later WVA version provides explicit final-image symbol binding or a shared generated-template mechanism without introducing an assembler/linker dependency cycle.
- A compact instruction form is needed for a measured size boundary and receives a precise WVA encoding rule.
- A Windvale-written container constructor consumes verified WVO templates directly and makes the C# byte arrays recovery-only.
