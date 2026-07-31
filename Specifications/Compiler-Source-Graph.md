# Windvale compiler source graph

## Status and purpose

`Compilerˉsourceˉgraph` is the cross-host-qualified portable import-topology phase at commit `09c6f54` under Decision 0030. It consumes one completely validated WVSS 1 value and proves that its supplied sources form one complete, acyclic graph rooted at entry zero.

This is the first semantic phase above the source container. It does not discover files, parse paths, bind declarations or bodies, construct WIR, or emit WVB.

## Result contract

```text
enum Compilerˉsourceˉgraphˉstatus {
    Valid;
    Sourceˉset;
    Duplicateˉimport;
    Missingˉimport;
    Cycle;
    Unreachable;
}

record Compilerˉsourceˉgraphˉsummary {
    Status: Compilerˉsourceˉgraphˉstatus;
    Sourceˉsetˉstatus: Compilerˉsourceˉsetˉstatus;
    Modules: u32;
    Imports: u32;
    Reachable: u32;
    Failureˉmodule: u32;
    Failureˉtarget: u32;
    Failureˉoffset: u32;
    Failureˉline: u32;
    Failureˉcolumn: u32;
}

Compilerˉvalidateˉsourceˉgraph(Input: bytes)
    -> Compilerˉsourceˉgraphˉsummary
```

On success, `Modules` equals `Reachable`, `Imports` is the aggregate import count, both failure indices equal `Modules`, the failure offset equals the complete WVSS length, and the failure position is zero. A missing target uses `Modules` as its target sentinel. A source-set rejection preserves the source-set status and failure location.

## Deterministic graph rules

Entry zero is the root. Import names resolve by exact unsigned ordinal comparison against declared module-name UTF-8 spans. Every import in the reachable closure must resolve, and a module may name a target only once. Every supplied dependency must be reachable from the root. The reachable graph must be acyclic; self-imports are cycles.

Reachability uses immediate immutable state updates, so a dependency later in WVSS order may be visited in the same pass while an earlier dependency is visited in the next pass. This affects only work performed, never the result. Passes stop when no new module is marked or after the module bound.

Acyclicity uses deterministic zero-incoming removal. Candidate modules are considered in WVSS index order. Failure to remove another module proves that a cycle remains; a separate bounded frontier walk returns a real import edge that closes a path to its start module.

The current fail-fast ordering is: complete WVSS validation; repeated or missing imports encountered while expanding the root closure; first unreachable dependency in canonical WVSS order; then cycle rejection. Inputs containing several independent faults receive the first failure under this order.

## Bounds and ownership

The graph contains one through 64 modules because WVSS owns that bound. Traversal state is exactly one byte per module and uses `Foundationˉbytesˉrepeat` plus checked single-byte replacement. It contains no native path, timestamp, source text copy, token, declaration object, or host handle.

Headers and leading import declarations are rescanned from accepted immutable sources. No edge table is retained. The graph phase does not increase the 4 MiB WVSS ceiling or the parser's declaration limits.

## Current candidate artifacts and evidence

- `Source-Graph-Core.wvb`: 196,493 bytes, SHA-256 `4b45616ff0304f59f16c44afea637fabd0f66f68ae4b5d7b149e23a9e8e70662`.
- `Source-Graph-Demo.wvb`: 202,893 bytes, SHA-256 `309c7ca3815c709c759b4673036ffd747d95450c651aa21c3dbf66e59dbe903c`.
- `Source-Graph-Tool.wvb`: 200,174 bytes, SHA-256 `4a07816c65d82b6594270a9b253b999700196d07e10faed0221fc6b1ca7e1e9e`.

The Windows and Debian conformance runners each pass all 44 tests with zero build warnings/errors. The demo covers valid and rejected topology, and the hosted tool validates the real compiler closure as:

```text
source graph status=Valid modules=7 imports=6 reachable=7
```

The graph contract was originally cross-host qualified at `09c6f54`. The current Decision 0042 artifact identity was requalified byte for byte with the role-based compiler layout at `4fdc6bf`; the graph summary itself is unchanged.
