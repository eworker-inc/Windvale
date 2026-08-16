# Decision 0674: Compiler-scale project-WVB checkpoint

- Date: 2026-08-16
- Status: Implemented development-tool checkpoint
- Extends: [native tool checkpoint contract](../../Specifications/Windvale-Native-Tool-Checkpoint.md)
- Replaces: project-WVB checkpoint family `project-wvb-v1`

## Context

The current compiler build-driver project emits a 1,156,427-byte WVB. Its
mandatory compiler-aligned build succeeds, but the general publisher and
read-only WVB front door retain narrower ordinary-module envelopes. The v1
cache routed its private candidate through both boundaries, discarded the
valid compiler-scale result, and forced every database verifier to fail during
tool preparation.

The cache already owns a fresh private directory, hashes the candidate, writes
a complete manifest, atomically publishes the directory, rehashes every hit,
and materializes a byte-identical owner copy. General output publication is not
needed inside that private checkpoint transaction.

## Decision

- Add `project-wvb-v2` for compiler-aligned project outputs.
- Key the exact workspace, project and source closure, native-front-door
  inventory, and current-host native build-driver.
- Let that keyed build driver write only to the fresh private candidate. Admit
  the result only after its mandatory compiler-aligned verification reports
  success.
- Preserve the 67,108,864-byte cache bound, exact digest manifest, link
  rejection, atomic directory publication, hit rehash, and byte-for-byte
  materialization check.
- Do not pass the private compiler-scale candidate through the independent
  general publisher or read-only verifier envelopes. Do not execute cached WVB.
- Keep qualification and ordinary `Build-Wvb` publication behavior unchanged.

## Evidence

Cold Windows creation admitted the exact 1,156,427-byte build-driver WVB at
SHA-256 `88d84cb3a18d095ca1a3cc4b92dffb2d9a05de661ce4f0db5c671b5c78a7242d`
under key
`0b15f46d8f5f73647decb845eafad783f2b6aef6f1a84705f99026108415cecb`.
A subsequent hit rehashed and copied the same file in 530.337 ms. Native
development dependency closure passes for three owners and 34 declarations;
both cache scripts pass shell syntax checking.

This repair exposes the next independent bootstrap limit: segmented staging of
the current compiler WVB reports `Unsupported_module`. Decision 0674 therefore
repairs project-WVB caching but does not claim that the full compiler packaging
chain or database verifier is restored.

## Consequences

Local tools no longer spend compiler-build time only to lose an already
verified compiler-scale WVB at a narrower general publication boundary. Cache
identity now states the producer that actually establishes admission.

The current staging incompatibility remains visible and fail-closed. It needs
its own compiler/bootstrap correction rather than weakening the staged
compiler contract inside this cache.

## Reconsideration triggers

Retire v2 when one qualified general publisher and verifier admit the complete
compiler envelope without making ordinary WVB limits implicit, or when the
bootstrap path no longer needs a cached compiler WVB.
