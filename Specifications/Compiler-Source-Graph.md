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

- `Source-Graph-Core.wvb`: 203,731 bytes, SHA-256 `a6ef5896e45593f45b136cc73f3e8c57dd33274ff4736eff18795276fb0c8885`.
- `Source-Graph-Demo.wvb`: 210,131 bytes, SHA-256 `7c0e191c6a931617aee23fbc91dac61648ebc2f8f2a40a1690ff648a6b9d60de`.
- `Source-Graph-Tool.wvb`: 207,412 bytes, SHA-256 `ffbbea564754c667961680497d3b077f38626a4993ed9c1e1a0d5966e5378aba`.

The Windows and Debian conformance runners each pass all 44 tests with zero build warnings/errors. The demo covers valid and rejected topology, and the hosted tool validates the real compiler closure as:

```text
source graph status=Valid modules=7 imports=6 reachable=7
```

The graph contract was originally cross-host qualified at `09c6f54`. Decision 0042's artifact identity was requalified byte for byte with the role-based compiler layout at `4fdc6bf`; the graph summary itself remains unchanged. Decision 0055 changes only embedded frontend implementation bytes in these artifacts and is cross-host qualified at `1a4fca7`.
