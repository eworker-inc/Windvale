# Decision 0039: Capability profiles in the Windvale backend

- Date: 2026-07-31
- Status: Accepted and cross-host qualified

## Context

The qualified backend can emit primitive data, nominal types, and every portable WVIR operation, but it rejects hosted/system profiles, every capability declaration, and WVIR `Callˉcapability`. This blocks the existing Windvale compiler tools from becoming backend inputs even though WVSD already validates the complete seven-capability Seed catalog, WVIR already carries canonical capability identities and signatures, and WVB 1.6 already defines capability metadata, authorization, verification, and runtime behavior.

Adding another host-call format would duplicate qualified contracts. Supporting multi-module lowering at the same time would mix an independent identity-flattening problem into a small metadata/code-generation extension.

## Decision

Extend `Compilerˉsourceˉwvb` for all three existing module profiles while retaining the one-module input boundary.

- Preserve the validated source profile as the WVB portable, hosted, or system profile byte.
- Require portable modules to remain capability-free through the existing WVSD/WVB validation contracts.
- Emit capability entries in ordinal name order using the exact current Seed catalog signatures.
- Translate each validated WVSD capability identity to its canonical WVB capability index.
- Lower WVIR operation `63` to the existing WVB `call.capability` opcode.
- Keep capability support explicit: generated hosted/system modules still require host support and separate runtime authorization.
- Keep imports and multi-module backend translation as the next independent extension.

The Stage 0 compiler remains the differential oracle. A deliberately unsorted hosted fixture must declare and call all seven current capabilities, exercise void and value-returning signatures, pass mandatory verification, expose canonical inspection order, and execute a no-argument path without external file mutation.

## Consequences

The backend can now produce the hosted wrappers already used by Windvale compiler, assembler, linker, and inspection tools without changing WVB 1.6 or weakening the host boundary. System-profile preservation also avoids a later profile-only rewrite; it does not add privileged capabilities or define an OS ABI.

The compiler is still not self-hosted. Its real closure spans multiple modules, and the measured source-envelope and repeated-body-traversal limits remain separate blockers.

## Verification gate

The candidate must pass:

- the focused source-to-WVB test with all four differential fixtures;
- exact WVB byte equality with Stage 0 for the hosted fixture;
- exact capability ordering and signatures, mandatory verification, inspection of capability indices `0` and `6`, and authorized runtime result `0`;
- the complete Standard suite and native verifier on Windows; and
- exact-commit Debian qualification with matching normalized reports and byte-identical retrieved portable artifacts.

Exact implementation commit `98117c15255ce5a95d41ca13e43f92a4af77ef98` passed every gate on Windows x64 and Debian Linux x64. The complete evidence is recorded in `Documents/Project/Seed-Verification-Evidence.md`.
