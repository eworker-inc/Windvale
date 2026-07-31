# Decision 0040: Static multi-module Windvale backend

- Date: 2026-07-31
- Status: Accepted and cross-host qualified

## Context

The qualified Windvale backend emits the complete implemented one-module language surface, while the preceding source-set, graph, symbol, binding, and WVIR phases already validate as many as 64 statically supplied modules. Rejecting that existing evidence at the backend prevents Windvale-written compiler and Foundation modules from being flattened into one executable WVB.

WVB 1.6 has no runtime imports, and the current source composition contract deliberately requires every dependency to be portable, data-free, capability-free, and function-export-complete. Adding a runtime module linker or another intermediate format would duplicate the deterministic static composition model already qualified in WVSS, WVSD, WVLB, and WVIR.

## Decision

Extend `Compilerˉsourceˉwvb` to lower one complete validated WVSS graph directly into one canonical WVB 1.6 module.

- Preserve the root module name, profile, capabilities, static data, and exported functions.
- Internalize dependency records, enums, and functions; dependency source exports do not become WVB exports.
- Use the global WVSD namespaces and canonical ordinal names for function, data, capability, and nominal identities regardless of owner module or source declaration order.
- Resolve every WVSD entry back to its owning WVSS source before reading names, declarations, types, bodies, or literal spans.
- Discover text literals by canonical global function order, reuse root explicit text data by value, avoid root data-name collisions, and emit one canonical merged data section.
- Continue rejecting invalid, unsorted, missing, cyclic, unreachable, non-portable, data-bearing, capability-bearing, or private-function dependencies through the existing upstream contracts.
- Do not add WVB imports, runtime linkage, package discovery, or host-dependent source lookup.

The hosted source-to-WVB tool accepts a root source, zero or more already canonically sorted dependency sources, and a final output resource. It constructs WVSS without paths or host enumeration entering the portable backend.

## Consequences

The Windvale backend can now reproduce Stage 0's bounded static composition model and emit an ordinary self-contained WVB. The output remains loadable by the unchanged verifier and runtime on every host.

This closes the backend's module-count restriction but does not complete compiler bootstrap closure. The current 4 MiB WVSS envelope and repeated local/body traversals remain measured blockers for the real compiler source graph.

## Verification gate

The implementation must pass:

- the focused source-to-WVB test with all existing single-module fixtures unchanged;
- a three-module Stage 0 differential fixture covering cross-module calls and nominal values, dependency-owned text literals, root data, synthetic-name collision avoidance, canonical global function/data/type ordering, root-only exports, mandatory verification, and runtime result `42`;
- rejection of noncanonical dependency order with no output publication;
- the complete Standard suite and native verifier on Windows; and
- exact-commit Debian qualification with matching normalized reports and byte-identical retrieved portable artifacts.

Exact implementation commit `cb1db235ef1ecf9697693f260516d0e241ced012` passed every gate on Windows x64 and Debian Linux x64. The complete evidence is recorded in `Documents/Project/Seed-Verification-Evidence.md`.
