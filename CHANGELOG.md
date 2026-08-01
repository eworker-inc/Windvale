# Windvale changelog

Windvale has not issued a stable release. Development history and qualification evidence are recorded in Git, `Documents/Decisions/`, and `Documents/Project/Seed-Verification-Evidence.md`.

Once releases begin, Windvale will use `v0.y.z` tags while public contracts remain experimental. A `0.y` change may revise an experimental contract without backward compatibility, but release notes must identify affected formats and migration expectations. A future `1.0.0` release requires an explicit stability and support decision.

## Unreleased

### Added

- MIT licensing and [E-Worker Inc](https://eworker.ca) stewardship.
- Vendor-neutral AI-authorship and attribution policy.
- Public contribution, governance, security, support, conduct, and project-identity policies.
- Windows and Linux repository verification workflows.
- Repository-owned Windvale syntax highlighting and Visual Studio Code language support for `.wv` source files.
- A GitHub-rendered visual progress dashboard with evidence-based roadmap indicators and a dated SVG phase map.
- Opt-in deterministic per-function instruction reporting in the C# reference runtime and Seed CLI.
- The accepted first x86-64 UEFI/QEMU boot environment and a path-independent dependency preflight over exact EDK II firmware inputs.
- The first verified WVA-to-Windvale kernel execution seam, linking an assembled machine shim to compiler-generated `.wv` Main on kernel-owned memory.
- Windvale-owned post-memory boot evidence and a bidirectional WVA seam for Main entry and console-byte output.
- The accepted Windvale-native interpreter/JIT/AOT direction and explicit qualification gate for retiring .NET from the normal Windows and Linux workflow.
- Qualified exact Windvale bytecode compiler self-reproduction: Stage 0 builds Stage 1, and Stage 1 builds a byte-identical Stage 2 from the same committed 12-module inventory on Windows and Debian.
- The bounded deterministic Windvale Project 1 manifest and `windvale build` command for selecting one root plus explicit source dependencies without changing import, WVSS, or WVB semantics.
- The ABI-8 hosted-input candidate, with execution-bounded borrowed text/bytes, explicit process argument and file snapshot services, and the first Windvale-written native WVB header inspector.

### Changed

- The Windvale-written lexer uses bounded keyword, identifier, and Unicode-whitespace dispatch while preserving the Seed lexical contract.
- Compiler folders now describe implementation roles: `Compiler/Windvale` contains the Windvale-written compiler, while `Compiler/Reference` contains the independent C# reference/recovery compiler.
- Assembler folders now describe implementation roles: `Assembler/Windvale` contains the Windvale-written assembler, while `Assembler/Reference` contains the independent C# Stage 0 reference/recovery assembler. Canonical WVA inputs remain under `Examples/Assembler`.
- Linker folders now describe implementation roles: `Linker/Windvale` contains the Windvale-written flat-image linker, while `Linker/Reference` contains the independent C# Stage 0 reference/recovery linker and its currently C#-only UEFI target adapter. Canonical WVA provider inputs remain under `Examples/Linker`.

### Current development status

- Windvale Seed, its runtime and bytecode foundation, the object model, assembler, linker, Foundation modules, complete portable compiler, and exact bytecode compiler self-reproduction have Windows/Debian qualification evidence.
- Native compiler execution, the shared JIT/AOT backend, .NET retirement, and Windvale OS remain active or planned milestones rather than completed releases; the bounded boot path and kernel-owned memory evidence do not yet constitute a functioning kernel runtime.
