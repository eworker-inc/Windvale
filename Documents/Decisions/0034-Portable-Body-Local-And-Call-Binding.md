# Decision 0034: Portable body, local, and call binding

- Date: 2026-07-30
- Status: Accepted and implemented; cross-host qualification pending

## Context

The qualified declaration/signature phase publishes canonical declarations, nominal identities, and transitive module visibility, but it deliberately stops before function bodies. The next compiler layer needs stable local slots and scopes, body-name resolution, assignment policy, callable resolution, arity checks, and evidence that later typed semantics and WIR construction can consume without depending on host collections.

Several bounded designs were measured against the real compiler closure. A separate collection pass followed by a second body-binding pass repeated body parsing and directory work. A later combined pass still rebuilt the complete growing binding payload into a temporary directory at each statement and used `Bytesˉslice` to materialize a module source for each symbol candidate. Those variants exhausted the fixed 4,000,000,000-instruction ceiling after approximately 276 and 264 seconds. Increasing the ceiling would have hidden representation costs that would recur in later compiler phases.

## Decision

Introduce `Compilerˉsourceˉbindings` as a portable semantic phase above `Compilerˉsourceˉsymbols`. It uses one canonical module/declaration/statement traversal to append parameter and local evidence while binding the expressions that can refer to evidence already in scope.

The phase publishes `WVLB 1`, an immutable packed directory with:

- a 24-byte header;
- one 8-byte function-range entry for every WVSD declaration entry; and
- fixed 36-byte binding entries containing module, function identity, kind, slot, source-name span, type shape, and scope span.

Parameters receive the first slots and cover the full function body. A local initializer is bound before its declaration is appended. Locals become active after their declaration statement and end at their containing block. Parameter and local names remain unique across the complete function; shadowing is not introduced. `let` and parameters are immutable, while `var` is assignable.

Body names resolve to active locals/parameters or accessible global data. Calls resolve Foundation intrinsics, record constructors, functions, and declared capabilities, then enforce visibility and arity. Known but undeclared capabilities receive their own failure. Complete expression typing, field ownership, operator checking, control-flow proof, WIR, and lowering remain separate later work.

The retained hot-path representation is offset based:

- local lookup receives the immutable payload and the current function's first/end binding indices directly;
- declaration and call lookup compare names against absolute source offsets inside WVSS; and
- final identifier validation reads absolute WVSS spans directly.

No lookup materializes a module source per directory candidate, and no statement rebuilds a temporary WVLB wrapper. The published directory is still constructed once and independently validated before it crosses the phase boundary.

## Consequences

The compiler now has deterministic body-level identities and stable failure evidence without a syntax tree, token collection, host dictionary, or general collection library. The directory is suitable input for typed expression/control-flow semantics and later WIR construction, but it is an internal development contract and may evolve while bootstrap compatibility is not promised.

The no-shadowing rule is intentionally simpler than many general-purpose languages. It aligns with Windvale's explicit naming standards, prevents ambiguous evidence, and can be revisited only with a concrete language requirement and deterministic slot/scope design.

The direct packed-span approach is now a compiler performance rule: immutable packed input should be addressed by validated offsets. Repeated slicing or rebuilding is acceptable only when measurement shows it is bounded and proportionate.

The candidate real nine-module closure completes under the unchanged 4,000,000,000-instruction ceiling and reports 177 functions, 777 parameters, 896 locals, 7,937 reads, 602 assignments, 1,344 calls, and a 62,044-byte WVLB directory.

## Verification gate

The exact candidate must pass the complete Release conformance and native CLI verifiers on Windows and Debian. Coverage must include:

- parameter/local slot and scope behavior, initializer-before-declaration, nested scope, whole-function duplicate rejection, and mutability;
- primitive, visible nominal, unknown, and inaccessible local types;
- local and global reads, assignments, intrinsics, constructors, functions, capabilities, visibility, arity, and stable failure order;
- upstream source-symbol failure propagation;
- independent rejection of malformed WVLB magic, version, length, ranges, entries, identifiers, scopes, shapes, and trailing data;
- exact core, demo, and hosted-tool hashes; and
- the exact real-closure report under the fixed instruction ceiling.

Windows and Debian reports must normalize to the same contract, and all directly compared verifier artifacts must be byte-identical. Qualification evidence and the candidate commit are recorded only after that gate passes.
