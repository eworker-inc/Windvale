# Decision 0030: Portable compiler import graphs

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified at `09c6f54`

## Context

WVSS 1 gives portable Windvale code a bounded root source plus canonically ordered dependency sources, but it deliberately does not resolve imports. Leaving graph resolution in the C# Stage 0 composer would make a host dictionary, host traversal order, and host diagnostics part of every future Windvale compiler invocation.

The graph has at most 64 modules and every accepted source already has a qualified streaming declaration view. A retained general graph, token collection, or compiler heap is therefore not yet required to establish complete, deterministic topology.

## Decision

Introduce `Compilerˉsourceˉgraph` as the first portable semantic phase above WVSS. It first requires complete source-set validation. Starting at entry zero, it resolves every reachable import by exact ordinal module-name bytes, rejects a repeated import within one module, rejects missing targets, computes the root closure, and rejects the first canonically supplied dependency outside that closure.

Reachability state is exactly one immutable byte per supplied module, created and replaced through the qualified `Foundationˉbyteˉconstruction` contract. This is compiler-owned traversal evidence, not a public general collection. The phase detects cycles by deterministic zero-incoming removal over the accepted graph; if removal cannot advance, a bounded frontier walk returns an actual closing import edge as failure evidence.

Failures identify graph status, source-set status, importing module, resolved target when one exists, and the import or module-name byte offset plus one-based line and column. The hosted tool only packs explicit first-read source snapshots and reports the portable result. Paths, timestamps, host handles, ambient discovery, and host collection order never enter graph semantics.

This decision qualifies graph topology only. Declaration namespaces, nominal types, function signatures, body names, expression types, control flow, WIR, and WVB production remain later semantic phases.

## Consequences

The future binder receives a complete acyclic root closure and can resolve declaration visibility without repeating topology policy in C#. The implementation deliberately rescans bounded headers and leading import declarations rather than retaining an edge table. The real seven-module compiler closure and exact 64-module/63-edge chain measure whether this remains practical.

If symbol binding demonstrates that repeated graph/name queries dominate execution or obscure ownership, the next compiler-owned packed index may retain module and declaration spans. Such an index must replace demonstrated rescanning and remain independently validated; it must not turn native paths or host objects into compiler state.

## Verification gate

The exact candidate must pass the complete conformance and native CLI verifiers on Windows and Debian. Portable coverage includes a valid diamond, transitive reachability, repeated and missing imports, direct and self cycles, unreachable supplied modules, WVSS failure propagation, stable failure coordinates, the exact 64-module/63-edge chain, and the real seven-module compiler graph.

Both hosts must produce identical graph core, demo, and tool WVB files. The hosted tool must report `modules=7 imports=6 reachable=7` from the exact compiler sources, normalized conformance reports must match, and previously qualified direct artifacts must retain their identities.

Candidate commit `09c6f54` passed this gate from its exact archive on Windows x64 and Debian GNU/Linux 12 x64. Both hosts completed a zero-warning Release build, all 44 conformance tests, and the complete native CLI verifier. Their normalized reports matched, all 22 directly retrieved graph/dependency/downstream artifacts were byte-identical, and the hosted tool produced the required real-graph report. The archive and resolved QA directory were removed after evidence retrieval.
