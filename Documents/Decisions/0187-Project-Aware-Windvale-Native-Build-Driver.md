# Decision 0187: Project-aware Windvale-native build driver

- Date: 2026-08-03
- Status: Cross-host qualified at exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b`
- Advances: Phase 10 native host tools and the [Decision 0057 native-retirement gate](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Extends: [Decision 0186](0186-First-Windvale-Native-Compiler-Build-Driver.md) and [Decision 0075](0075-Minimal-Deterministic-Windvale-Projects.md)
- Contract: [Windvale compiler build-driver application](../../Specifications/Windvale-Compiler-Build-Driver.md)

## Context

Decision 0186 packages one Windvale-native process that compiles explicit source paths, admits the resulting WVB through the shared portable verifier, and publishes accepted bytes. The normal project path still entered the C# CLI even though the Project 1 parser was already Windvale-owned and native-execution qualified.

The portable parser intentionally knows nothing about native path separators or identity. A general directory capability, canonical resource identity, atomic replacement, and native x64 backend transfer remain larger contracts. Folding any of those into Project 1 would turn host policy into source semantics.

The executable recommendation also requires an honest dependency order: PE/ELF headers are not the active source-to-executable blocker. The shared x64 lowering backend is still C#-owned. Moving only container bytes would retain .NET in every ordinary native build while creating a misleading retirement claim.

## Decision

Compose `Tools/Windvale.Project/Project-Manifest-Core.wv` into the existing build driver and add this exact form:

```text
wvbuild --project <project.wvproj> <output.wvb>
```

Keep Project 1 parsing portable. In the hosted adapter, require `/` in the manifest resource name, retain its prefix through the final separator, and append each already validated Project 1 relative path. Reject `\` rather than interpreting it differently by host.

Use a conservative ASCII-case-folded resource comparison before source access. Reject repeated source names as `WVP1007` and output/input equality as invalid invocation. This rejects a narrow set of distinct case-sensitive Linux resources but prevents the ordinary Windows case alias without pretending to resolve links or provider identities.

Retain the fixed 64-snapshot runtime profile. Project mode therefore admits at most 63 modules because the manifest consumes one snapshot. Report the 64th module as `WVP1005`. Do not enlarge the runtime, change `WVHB 1`, add authority, or alter the format-3 compiler packages.

Read the manifest and every selected source exactly once. Construct the canonical WVSS directory while retaining source payloads, then use the existing in-memory compiler, verifier, and sole post-admission `file.write_bytes` call.

Extend the existing exact-compiler AOT test rather than adding another compiler construction. Run a real three-module project through the current-host native package, compare exact WVB bytes with the reference compiler, reject malformed and conservative-duplicate manifests while preserving a sentinel output, and retain the direct no-.NET inspection.

## Consequences

- A packaged Windvale-native driver now owns Project 1 parsing, bounded manifest-relative resource derivation, source selection, source-to-WVB compilation, verification, and accepted publication.
- Explicit and project modes snapshot each source once rather than reopening it to build WVSS.
- The Project 1 portable parser and source dependency semantics remain unchanged.
- The project subset is deliberately conservative: 63 modules, `/` resource names, ASCII case-folded duplicate checks, and no canonical link/provider identity.
- `file.write_bytes` remains durable but non-atomic; deterministic rejection still occurs before its only call.
- Native PE/ELF source builds remain blocked on Windvale ownership of the shared x64 lowering backend. Outer-container transfer alone is insufficient.
- Stage 0 still builds, lowers, packages, and independently verifies the driver and remains the recovery oracle.

## Qualification

Exact commit `524e84afb6e5bab6bbd95ebc0b9eeaf886af834b` passes GitHub
[Verify run 30964566192](https://github.com/eworker-inc/Windvale/actions/runs/30964566192).
Both permanent hosts exercise explicit-source and Project 1 modes through the raw
native driver, compare exact verifier-admitted output, preserve an existing output
on deterministic rejection, and complete the full repository Qualification gate.

## Reconsideration triggers

- a canonical resource-identity or directory-relative read capability can replace conservative name derivation;
- project pressure requires all 64 source modules in one retained-input profile;
- an atomic-replacement capability has exact Windows/Linux progress and durability semantics;
- the shared native backend can be invoked from Windvale without a managed bridge; or
- cross-host qualification changes artifact identity, behavior, or accepted resource names.
