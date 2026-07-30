# Decision 0035: Canonical typed source IR

- Date: 2026-07-30
- Status: Accepted; cross-host qualification pending

## Context

Windvale already has portable source-set, graph, declaration/signature, and body-binding phases. The next boundary must prove that Windvale code can perform complete expression typing and control-flow construction before the project asks that code to emit WVB, native objects, or OS images.

Lowering directly from source views to WVB would entangle language semantics with one execution format. Retaining a host object graph would make determinism and cross-host evidence difficult to inspect. Rebinding every source name and reparsing every signature inside each later pass also exhausts the fixed instruction budget on real compiler-sized inputs.

## Decision

Introduce `WVIR 1`, a packed immutable typed source IR constructed by the portable `Compilerˉsourceˉwir` module.

WVIR contains five canonical sections: declaration-aligned function entries, basic blocks, typed operations, temporary shapes, and temporary operands. Operations retain source spans and stable WVSD/WVLB identities. Control flow uses explicit jump, branch, and return terminators. The phase rejects type, condition, return, reachability, data, field, enum, operator, and argument errors before publishing evidence.

The constructed directory crosses the phase boundary only after `Compilerˉsourceˉwirˉdirectoryˉisˉvalid` independently checks its complete binary shape and semantic relationships. Corruption coverage includes every section and trailing data.

Valid-path preparation reuses one validated symbol summary and collects only parameter/local evidence. Full body binding remains the error oracle when typed lowering rejects a program, preserving the earlier phase's diagnostic precedence without imposing duplicate body-reference work on successful small programs.

Routine verification uses a focused semantic/corruption demo and a control-heavy tool fixture. Full compiler self-lowering remains a measured performance target under the unchanged four-billion-instruction ceiling; it is not placed in the fast or standard loop until local discovery and IR construction share a body traversal or reusable typed body evidence.

## Consequences

Windvale now has an explicit language-semantic boundary between source and executable bytecode. The future WVB backend can consume typed blocks and operations rather than reimplementing name resolution or source typing, and a future native backend can share the same front end.

WVIR is intentionally internal and versioned. It may evolve while bootstrap compatibility is not promised, but a version change must remain deterministic, bounded, documented, independently validated, and cross-host qualified.

The retained implementation uses function-private payload construction and indexed symbol lookup. Performance work must reduce repeated traversal or materialization; increasing execution limits is not accepted as proof that the representation scales.

## Verification gate

The candidate must pass:

- the focused Release WVIR conformance test;
- all existing source-symbol and source-binding tests after their prepared-evidence changes;
- the complete standard conformance suite;
- native Windows and Debian qualification with exact candidate hashes and byte-identical retrieved artifacts; and
- the control-heavy hosted tool fixture under its fixed instruction limit.

The full ten-module self-lowering performance target is tracked separately and becomes mandatory only after the body-traversal fusion slice makes it fit under the existing ceiling.
