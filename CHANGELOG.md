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
- Opt-in deterministic per-function instruction reporting in the C# reference runtime and Seed CLI.
- The accepted first x86-64 UEFI/QEMU boot environment and a path-independent dependency preflight over exact EDK II firmware inputs.
- The first verified WVA-to-Windvale kernel execution seam, linking an assembled machine shim to compiler-generated `.wv` Main on kernel-owned memory.
- Windvale-owned post-memory boot evidence and a bidirectional WVA seam for Main entry and console-byte output.

### Changed

- The Windvale-written lexer uses bounded keyword, identifier, and Unicode-whitespace dispatch while preserving the Seed lexical contract.
- Compiler folders now describe implementation roles: `Compiler/Windvale` contains the Windvale-written compiler, while `Compiler/Reference` contains the independent C# reference/recovery compiler.
- Assembler folders now describe implementation roles: `Assembler/Windvale` contains the Windvale-written assembler, while `Assembler/Reference` contains the independent C# Stage 0 reference/recovery assembler. Canonical WVA inputs remain under `Examples/Assembler`.
- Linker folders now describe implementation roles: `Linker/Windvale` contains the Windvale-written flat-image linker, while `Linker/Reference` contains the independent C# Stage 0 reference/recovery linker and its currently C#-only UEFI target adapter. Canonical WVA provider inputs remain under `Examples/Linker`.

### Current development status

- Windvale Seed, its runtime and bytecode foundation, the object model, assembler, linker, Foundation modules, compiler frontend, and portable WVIR-to-WVB backend have qualification evidence.
- Compiler self-hosting qualification, the native toolchain, and Windvale OS remain active or planned milestones rather than completed releases; the first OS environment is accepted, but no boot image or kernel is claimed, and the C# reference/recovery compiler remains intentional.
