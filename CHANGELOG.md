# Windvale changelog

Windvale has not issued a stable release. Development history and qualification evidence are recorded in Git, `Documents/Decisions/`, and `Documents/Project/Seed-Verification-Evidence.md`.

Once releases begin, Windvale will use `v0.y.z` tags while public contracts remain experimental. A `0.y` change may revise an experimental contract without backward compatibility, but release notes must identify affected formats and migration expectations. A future `1.0.0` release requires an explicit stability and support decision.

## Unreleased

### Added

- MIT licensing and [E-Worker Inc](https://eworker.ca) stewardship.
- Vendor-neutral AI-authorship and attribution policy.
- Public contribution, governance, security, support, conduct, and project-identity policies.
- The public [`eworker-inc/Windvale`](https://github.com/eworker-inc/Windvale) repository, issue and pull-request templates, private vulnerability reporting, and public development workflow.
- Windows and Linux repository verification workflows.
- Repository-owned Windvale syntax highlighting and Visual Studio Code language support for `.wv` source files.
- A GitHub-rendered visual progress dashboard with evidence-based roadmap indicators and a dated SVG phase map.
- Opt-in deterministic per-function instruction reporting in the C# reference runtime and Seed CLI.
- The accepted first x86-64 UEFI/QEMU boot environment and a path-independent dependency preflight over exact EDK II firmware inputs.
- The first verified WVA-to-Windvale kernel execution seam, linking an assembled machine shim to compiler-generated `.wv` Main on kernel-owned memory.
- Windvale-owned post-memory boot evidence and a bidirectional WVA seam for Main entry and console-byte output.
- The accepted Windvale-native interpreter/JIT/AOT direction and explicit qualification gate for retiring .NET from the normal Windows and Linux workflow.
- Qualified exact Windvale bytecode compiler self-reproduction: Stage 0 builds Stage 1, and Stage 1 builds a byte-identical Stage 2 from the same committed 12-module inventory on Windows and Debian.
- The cross-host-qualified Windvale Project 1 manifest, `windvale build` command, portable Windvale-written parser, and native hosted shell for selecting and inspecting one root plus explicit source dependencies without changing import, WVSS, or WVB semantics.
- The cross-host-qualified shared native path through ABI 14/context 6, including all eleven native service leaves, bounded borrowed and arena-backed values, deterministic interpreter/JIT/WVO-AOT agreement, and live Windvale-produced process-input leaves.
- A cross-host-qualified Windvale publication planner that owns every live executable-image extent and canonical runtime-service placement before the narrow Windows/Linux W^X adapter allocates memory.
- A cross-host-qualified Windvale publication-lifetime graph that gates allocate, copy, seal, invoke, and release operations while one internal C# owner contains raw Windows/Linux executable-memory authority.
- A cross-host-qualified probe-17 terminal invalid-opcode boundary with a kernel-owned vector-6 IDT, deterministic panic transcript, and separate pinned-QEMU success and fault images.
- The accepted capability-oriented Windvale OS architecture: a small kernel written primarily in `.wv`, a bounded `.wva` machine layer, isolated services, AOT system code, and no permanent C#/.NET dependency.
- A cross-host-qualified WVA-owned Q35 poweroff adapter, with exact assembler parity and clean pinned-QEMU exit after the successful kernel path.
- Cross-host-qualified WVA-owned normalized x86-64 entries for invalid opcode and general protection, one explicit ring-0 trap-frame prefix, and three deterministic pinned-QEMU boot scenarios.
- Qualification-pending kernel-owned x86-64 page tables with a low-1-GiB identity map, null-page guard, NX/WP enforcement, fixed read-only/executable boot window, WVA-owned CR3 mechanics, and continued success/fault evidence under the new root.
- Qualification-pending in-guest WVB admission: an AOT Windvale verifier checks one exact embedded WVB 1.6 identity before the bridge can execute its separately AOT-compiled form, while malformed and changed candidates fail closed.
- A permanent UTF-8 native-process verifier boundary and detached/redirected macron regression check, closing the recurring launcher-only decoding failure.
- Cross-host-qualified ABI 15/context 7 native whole-file output through exact Windows and Linux leaves, advancing the exact compiler WVB beyond its `file.write_bytes` admission blocker to bounded record-shaped function admission.
- The public [`windvale.ca`](https://windvale.ca/) project home, system-responsive light and dark themes, new Windvale visual identity, and independently deployed [`play.windvale.ca`](https://play.windvale.ca/) browser playground.

### Changed

- The Windvale-written lexer uses bounded keyword, identifier, and Unicode-whitespace dispatch while preserving the Seed lexical contract.
- Compiler folders now describe implementation roles: `Compiler/Windvale` contains the Windvale-written compiler, while `Compiler/Reference` contains the independent C# reference/recovery compiler.
- Assembler folders now describe implementation roles: `Assembler/Windvale` contains the Windvale-written assembler, while `Assembler/Reference` contains the independent C# Stage 0 reference/recovery assembler. Canonical WVA inputs remain under `Examples/Assembler`.
- Linker folders now describe implementation roles: `Linker/Windvale` contains the Windvale-written flat-image linker, while `Linker/Reference` contains the independent C# Stage 0 reference/recovery linker and its currently C#-only UEFI target adapter. Canonical WVA provider inputs remain under `Examples/Linker`.

### Current development status

- Windvale Seed, its runtime and bytecode foundation, the object model, assembler, linker, Foundation modules, complete portable compiler, and exact bytecode compiler self-reproduction have Windows/Debian qualification evidence.
- Native compiler execution, Windvale-native platform memory authority, complete execution-support lifetime ownership, the shared JIT/AOT backend, .NET retirement, and Windvale OS remain active or planned milestones rather than completed releases; the qualified terminal exception boundary and kernel-owned memory evidence do not yet constitute a functioning kernel runtime.
