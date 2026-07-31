# Windvale typed source IR

## Status and purpose

`Compilerˉsourceˉwir` is the first portable Windvale-written typed lowering phase. It consumes one complete WVSS 1 source graph, reuses validated WVSD symbol evidence and WVLB local evidence, checks expression and control-flow semantics, and publishes canonical `WVIR 1` bytes.

WVIR is a compiler boundary, not executable bytecode. It preserves typed operations, basic blocks, calls, source spans, and stable declaration identities so later passes can lower the same program to WVB or future native and system targets without inheriting C# host behavior.

## Public result

```text
Compilerˉvalidateˉsourceˉwir(Input: bytes)
    -> Compilerˉsourceˉwirˉsummary
```

On success, the summary contains module, function-entry, block, operation, temporary, and operand counts plus an independently validated WVIR directory. On failure, the directory is empty and the summary identifies the first deterministic failure by module, related module, WVSD function entry, byte offset, and one-based line/column.

The status contract distinguishes upstream source-binding rejection, evidence limits, malformed constructed evidence, type mismatch, invalid conditions and returns, missing returns, unreachable statements, invalid data/index/field/operator use, and invalid call arguments.

## Typed lowering rules

The phase currently lowers:

- `i32`, `u8`, `u32`, `bool`, `text`, `bytes`, record, and enum values;
- literals, parameters, locals, assignment, data length/load, record construction and fields, enum members, Foundation intrinsics, functions, and declared capabilities;
- arithmetic, comparison, equality, boolean negation, and signed negation;
- expression statements, `return`, lexical blocks, `if`/`else`, and `while`;
- explicit jump, branch, and return terminators.

Shape `0` is `void`; `1` through `6` are `i32`, `u8`, `u32`, `bool`, `text`, and `bytes`. Record shapes start at `65536`; enum shapes start at `131072 + RecordCount`. Nominal suffixes are canonical WVSD nominal indices.

Each result-producing operation receives the next function-local temporary ID. Operands may refer only to earlier temporaries in the same function. Basic-block IDs are function-local and canonical in construction order. Function entries align one-for-one with WVSD declaration entries; non-function declarations have all-zero function entries.

## WVIR 1 binary directory

All integers are unsigned little-endian and the directory contains no padding.

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVIR` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Function-entry count |
| 12 | 4 | Function-entry size `48` |
| 16 | 4 | Block count |
| 20 | 4 | Block-entry size `36` |
| 24 | 4 | Operation count |
| 28 | 4 | Operation-entry size `48` |
| 32 | 4 | Temporary count |
| 36 | 4 | Temporary-entry size `4` |
| 40 | 4 | Operand count |
| 44 | 4 | Operand-entry size `4` |

Sections follow in that exact order.

Each 48-byte function entry contains twelve `u32` fields: module, first block/count, first operation/count, first temporary/count, first operand/count, parameter count, local count, and return shape.

Each 36-byte block entry contains nine `u32` fields: module, function, block ID, first operation/count, terminator, value temporary, first target, and second target. The sentinel `4294967295` represents an absent value or target.

Each 48-byte operation entry contains twelve `u32` fields: module, function, block, operation kind, result shape, result temporary, first operand/count, target, auxiliary value, source byte offset, and source byte length.

The temporary section is a sequence of result shapes. The operand section is a sequence of function-local temporary IDs.

## Operation families

Operation values `1` through `10` cover constants, local load/store, and static data. Values `11` through `37` cover the current Foundation byte/text/formatting intrinsics, records, and enums. Values `38` through `61` cover typed arithmetic, comparison, equality, and unary operations. Values `62` and `63` are function and capability calls. Value `0` is invalid in published evidence.

The numeric mapping is frozen by `Compilerˉsourceˉwirˉoperation` and verified by the focused demo. Adding an operation requires updating its result shape, operand arity and shapes, target/auxiliary contract, demo coverage, this specification, and both native qualification scripts.

## Independent validation

`Compilerˉsourceˉwirˉdirectoryˉisˉvalid` verifies:

- magic, version, fixed entry sizes, bounded counts, exact section offsets, and exact total length;
- canonical function ranges aligned with WVSD and WVLB, parameter/local counts, and source return shapes;
- canonical block IDs and ownership, gap-free operation coverage, valid targets, and terminator value types;
- operation ownership, kind, source span, result shape, temporary sequencing, and operand sequencing;
- prior-temporary use, local slots, data identities, record/enum identities, field/member indices, call targets, arity, and dynamic parameter/result shapes; and
- rejection of trailing bytes and corrupted function, block, operation, temporary, or operand entries.

Construction uses function-private payloads and merges each completed function once. Symbol lookup uses a deterministic first-byte index over absolute WVSS spans. Local evidence may be prepared from already validated symbols without repeating full body-reference binding; if typed lowering fails, the complete binding pass remains the diagnostic oracle so established binding failures retain precedence.

## Verification tiers and current boundary

The fast conformance case compiles the core, runs the semantic/corruption demo, and sends a control-heavy hosted fixture through the real file-reading tool. The fixture produces:

```text
source wir status=Valid modules=1 functions=8 blocks=11 operations=44 temporaries=36 operands=29 directory-bytes=3200
```

Candidate artifacts are:

- `Source-Wir-Core.wvb`: 493,411 bytes, SHA-256 `89e2590e99ea96ebea5995491bc13d9497b2b5c41b566c3653acfc4713b6414b`.
- `Source-Wir-Demo.wvb`: 499,202 bytes, SHA-256 `2d58a05a5ad7e39fda20e4706f52d365f15fe53d3cfae998431024fa1c1edada`.
- `Source-Wir-Tool.wvb`: 495,353 bytes, SHA-256 `8bbca67184db5d8d980e61268021771d25b20f47624878abec6b9e54afbd6c4d`.

The prior typed-WVIR candidate at exact commit `bf77f70b08f332deda9ea3a1691e262e1426c1c1` was cross-host qualified on Windows x64 and Debian GNU/Linux 12 x64. Both hosts passed all 47 tests and the complete native verifier, and all 48 portable artifacts compared for that historical candidate were byte-identical.

The ten-module compiler closure is intentionally not in the fast loop. Its current separate local-discovery and IR body traversals still exceed the fixed 4,000,000,000-instruction qualification ceiling. Raising the ceiling is not the remedy; a later slice must fuse those traversals or publish reusable typed body evidence before the full self-lowering case becomes a required gate.

WVIR-to-WVB lowering is specified separately in the initial [source-to-WVB backend contract](Compiler-Source-Wvb.md). WVIR execution, optimization, native IR, and OS-specific lowering are not part of this contract.
