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

### Current development status

- Windvale Seed, its runtime and bytecode foundation, the object model, assembler, linker, Foundation modules, and compiler frontend through declaration/signature binding have qualification evidence.
- Portable body/local/call binding is cross-host qualified; typed semantics, the native toolchain, and Windvale OS remain active or planned milestones rather than completed releases.
