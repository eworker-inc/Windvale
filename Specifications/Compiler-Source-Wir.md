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

Construction uses function-private payloads and merges each completed function once. Symbol lookup uses a deterministic first-byte index over absolute WVSS spans. Canonical record/enum shapes and directory identities use the private WVSI bidirectional nominal tables rather than repeated ordinal rescans. Parameter/local WVLB evidence and typed WVIR are constructed in the same successful-path statement traversal. A local initializer is lowered before its declaration becomes visible, and the growing binding state is carried through nested blocks. If typed lowering fails, the local-only and complete binding passes remain diagnostic oracles so established binding failures retain precedence.

## Verification tiers and current boundary

The fast conformance case compiles the core, runs the semantic/corruption demo, and sends a control-heavy hosted fixture through the real file-reading tool. The fixture produces:

```text
source wir status=Valid modules=1 functions=8 blocks=11 operations=44 temporaries=36 operands=29 directory-bytes=3200
```

Qualified milestone artifacts are:

- `Source-Wir-Core.wvb`: 515,845 bytes, SHA-256 `959a9341668215bd748d5a04946ff5a598c443dd788b551b9062fe47a5d7bca8`.
- `Source-Wir-Demo.wvb`: 521,546 bytes, SHA-256 `a32ae736936f459a33e0e9733593926b8d4f345d7f399310adb61b7e136f142d`.
- `Source-Wir-Tool.wvb`: 517,697 bytes, SHA-256 `8da075794db7227c8e89b48885a227d501a3ca03b2de7a186c27c97100060b4f`.

The original typed-WVIR candidate was cross-host qualified at `bf77f70`, the fused local-discovery/typed-WVIR implementation at `b1241157310bc597dbdf0d24146f4d81f0128712`, and Decision 0050's bidirectional nominal-index implementation at `e37204ffcdf17b39a486466cc13f35d8ee00b4b4`. Decision 0055's validated-scan reuse implementation is cross-host qualified at `1a4fca7e295545b3b815bbf187fc048f1a885c74`; Decision 0058's exact bootstrap artifact set is cross-host qualified at `5c16547`.

The ten-module compiler closure is intentionally not in the fast loop. Decision 0042 reduced the focused typed-WVIR fixture from 8,074,045 to 5,735,695 instructions; Decision 0050 reduced it again to 5,715,847 and removed directory-entry construction and nominal-rank derivation as dominant costs. Decision 0055's implementation falls to 3,626,693 focused instructions and completes the exact ten-module input in 3,912,239,584 instructions under the unchanged 4,000,000,000 ceiling. That clears the typed-WVIR performance entry gate. Decision 0058's separate dedicated verifier proceeds through WVB and qualifies exact Stage 1 to Stage 2 convergence.

WVIR-to-WVB lowering is specified separately in the initial [source-to-WVB backend contract](Compiler-Source-Wvb.md). WVIR execution, optimization, native IR, and OS-specific lowering are not part of this contract.
