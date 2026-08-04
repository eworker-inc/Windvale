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

## Current deterministic artifacts and retained evidence

- `Source-Graph-Core.wvb`: 271,314 bytes, SHA-256 `574528635f818694fb72ba1fe1d4634cf0fddf4976b6733a1f96a9cf2dbd8cd0`.
- `Source-Graph-Demo.wvb`: 277,325 bytes, SHA-256 `63e72328ec5897695ac4c7b9c044a409068d726e294011f00ff4f72221a6087a`.
- `Source-Graph-Tool.wvb`: 274,512 bytes, SHA-256 `697a803b57229ca4e5a7e66053f696f45f0b6f35c0c3bc6cfe87f47a6f3aa56b`.

These local candidate identities contain the updated frontend while preserving WVSS graph semantics. The current local hosted report is:

```text
source graph status=Valid modules=7 imports=10 reachable=7
```

The graph contract was originally cross-host qualified at `09c6f54`. Decision 0042's artifact identity was requalified byte for byte with the role-based compiler layout at `4fdc6bf`; the graph summary itself remains unchanged. Decision 0055 changes only embedded frontend implementation bytes in these artifacts and is cross-host qualified at `1a4fca7`. Decision 0058 uses exact span equality on graph identity paths without changing graph semantics or its report and is cross-host qualified at `5c16547`.
