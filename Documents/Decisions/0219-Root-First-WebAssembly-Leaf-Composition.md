# Decision 0219: Root-first WebAssembly leaf composition

- Date: 2026-08-05
- Status: Implemented as a candidate; dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md) and [Decision 0217](0217-Windvale-Sha256-And-Native-Wvb-Runner-Profile.md)
- Contract: [Windvale WebAssembly](../../Specifications/Windvale-WebAssembly.md)

## Context

The first pinned native WVB runner passed its focused native execution and test-orchestration cases on Windows and Linux. The same dual-host candidate exposed one shared failure in the broader retained corpus: the ordinary scalar-interpreter project now statically composes root `Main` with the portable Windvale SHA module, while the previous descriptor-call selector admitted only decreasing function ordinals with `Main` last.

Canonical source composition places root functions before internalized dependency functions. The Stage 0 root uses 4,612 nonparameter locals and 71,214 code bytes; both measurements fit the already-bounded single-function interpreter envelope but cross the older descriptor-call per-function ceilings. Moving hashing into assembly, a host service, C#, or a duplicate interpreter source would preserve the wrong retirement boundary.

## Decision

Add WebAssembly profile 17 over the existing ABI-3 descriptor-call encoder. It retains the decreasing-ordinal form and adds exactly one alternative graph: two `bytes -> bytes` functions, exported `Main` at ordinal zero, one call-free leaf at ordinal one, and only root-to-leaf calls. The selector independently scans every root call target, validates both complete bodies, rejects any leaf call, and keeps the 131,072 aggregate-code and 400,000-instruction bounds. The per-function reader adopts the measured 8,191-local and 131,072-code ceilings already used by the retained interpreter profile.

Admit the already-qualified checked `i32` arithmetic/comparison and `u32.multiply` operations in descriptor functions. Checked-arithmetic failures in private descriptor functions publish the status and return the required zero `i64` descriptor; the public `i32` result path is unchanged. The new validation executes only when an opcode was outside the earlier descriptor subset, so previously accepted inputs retain their generated WebAssembly bytes and ordinary selection path.

Consolidate `Foundation/Sha256.wv` into one focused Windvale `bytes -> bytes` function. It keeps explicit bitwise carry propagation for wrapping addition, uses a quartered round-constant selection to remain within the retained instruction budget, and emits the exact 64 lowercase ASCII digest bytes. The semantic `Bytesˉsha256ˉhex` intrinsic remains `bytes -> text`.

The unrelated Windows qualification failure was an executable cleanup race after a successful process result. Test cleanup now retries only the exact temporary executable deletion for at most 20 attempts at 50 milliseconds; the final failure still propagates.

## Consequences

- WVB execution and SHA-256 remain Windvale-owned; no assembly or host hash implementation is added.
- The ordinary scalar-interpreter composition falls from fifteen mixed-signature functions to two descriptor functions.
- General forward calls, recursion, cycles, and arbitrary root-first graphs remain rejected.
- Existing accepted WebAssembly artifacts retain their byte representation; the backend WVB identity changes because the selector gains the new bounded profile.
- The SHA source remains a cohesive 321-line module rather than expanding the already large WebAssembly core with hashing logic.

## Evidence

The Stage 0 composition is 91,731 WVB bytes with SHA-256 `a1af180e5fb55b92a163c4ff9f88d67fe65a06207527948e2fb58426692d9005`. A focused direct lowering produces a 601,759-byte import-free WebAssembly module with SHA-256 `c45ce1a445c398884c969daa65e1fe79ae16b46ffb727de906d7472f03c87495` under execution ABI 3, and Node.js independently accepts it as a valid WebAssembly module. The exact corrected native-runner identities and Windows/Linux qualification report remain pending until the coherent candidate is rebuilt and pushed.

## Reconsideration triggers

Reconsider the exact root-first form when canonical source composition needs a second independently measured forward leaf or a general acyclic graph. Reconsider the single SHA function when Windvale gains a source-level private inline/helper contract that preserves one implementation, bounded native execution, and the WebAssembly call graph without byte-packing scalar arguments.
