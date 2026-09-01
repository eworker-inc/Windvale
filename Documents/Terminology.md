# Windvale terminology

> Status: Current terminology guide
> Authority: Informative; specifications own exact definitions
> Last reviewed: 2026-08-31

This page translates common Windvale terms into day-to-day language. Follow the
linked specification when an exact binary, semantic, or security rule matters.

## Stack and formats

| Term | Day-to-day meaning |
| --- | --- |
| Windvale Seed | The small, working language and toolchain used to build the next layers of Windvale. Seed is useful now, but it is not the whole Language 1.0 design. |
| Windvale Language 1.0 | The accepted source-language design Windvale is implementing in slices. A frozen source design does not mean every feature is already executable on every target. |
| WVB | Windvale bytecode: the verified, portable program format shared by hosts. Its version is separate from the source-language version. |
| WVA | Windvale textual assembly: human-readable native assembly input. It is not Windvale source code. |
| WVO | Windvale object format: structured native sections, symbols, and relocations used before final linking. |
| WIR / WVIR | Windvale's typed internal representation between source analysis and code generation. It is compiler evidence, not a distribution format. |
| Stage 0 | The retired C#/.NET bootstrap preserved in an immutable recovery release. It is not part of the normal `main` development path. |
| Foundation | The core Windvale library contracts used by source programs and tooling. A library requirement still does not grant a capability. |

## Scope and authority

| Term | Day-to-day meaning |
| --- | --- |
| Host | The environment running a Windvale tool or program, currently including Windows and Linux. Host behavior does not define language semantics. |
| Target | The environment or machine contract code is built for. The build host and output target can differ. |
| Profile | A named, bounded set of supported language, bytecode, runtime, or platform behavior. |
| Capability | An explicit, rights-limited way to ask a provider for an operation such as file, console, network, or model access. |
| Provider | The component that implements a capability. Availability at startup does not guarantee that the provider cannot exit or be revoked later. |
| Portable | Uses only shared Windvale contracts promised by its declared profile. It does not mean that every Windvale component or artifact runs everywhere. |
| System code | Code allowed to use named privileged or platform-specific contracts. That authority must remain explicit. |

## Evidence words

| Word | What the project is claiming |
| --- | --- |
| Proposed | Open for review. Do not depend on it as accepted behavior. |
| Accepted | The direction or contract is approved. Work may still be missing. |
| Implemented | Code exists for the stated scope. This alone says nothing about test depth or host coverage. |
| Verified | A named check passed for an exact state and scope. |
| Qualified | The required qualification plan passed on every host and boundary named by that plan. |
| Released | A versioned artifact was published through the release process. |
| Historical | Preserves an earlier state, run, or rationale and is not current guidance. |
| Superseded | A later owner replaced it; retain it for history and follow the replacement. |

These words deliberately do not collapse into one progress label. For example,
a source design can be accepted while its runtime implementation is incomplete,
and an implementation can pass a focused Windows check without being qualified
across Windows and Linux.

## Review words

AI-produced, AI-reviewed, machine-verified, human-inspected, independently
reproduced, and externally audited describe different kinds of evidence. None
implies the others. [Decision 0849](Decisions/0849-Define-AI-Led-Research-And-Review-Evidence.md)
owns the exact project policy.
